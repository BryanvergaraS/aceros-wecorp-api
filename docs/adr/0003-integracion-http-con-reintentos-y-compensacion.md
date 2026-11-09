# ADR 0003 — Integración Ventas → Catálogo con cliente HTTP tipado, timeout, reintentos y compensación

- **Estado:** aceptada
- **Fecha:** semana 2 (Hito 2)
- **Requerimientos relacionados:** RF07, RF08, RF11, RNF06

## Contexto
Al emitir una nota, la API de Ventas necesita el precio vigente y debe descontar stock en la API de Catálogo. Las dos operaciones viven en bases distintas, por lo que no hay una transacción común.

## Decisión
- `CatalogoClient` es un **cliente HTTP tipado** registrado con `AddHttpClient<ICatalogoClient, CatalogoClient>()` (**IHttpClientFactory**). La URL base viene de `Catalogo__BaseUrl`.
- **Tiempo de espera de 5 s** por intento y **hasta 2 reintentos** (con espera breve) **solo cuando la petición no llegó a Catálogo** (conexión rechazada o nombre no resuelto). Descontar y reponer stock **no son idempotentes**: ante un timeout o un 5xx no se reintenta, porque Catálogo podría haber aplicado el movimiento y un reintento lo duplicaría. En esos casos, y cuando se agotan los reintentos, se responde **503 ProblemDetails** y la nota **no** se registra.
- Los 4xx no se reintentan y se traducen: 404 → 404, 409 → 409 (stock modificado por otra venta), 422 → 422 (stock insuficiente), 401 → 401.
- En Catálogo, `Stock` es **token de concurrencia** (concurrencia optimista): dos ventas simultáneas del mismo producto no se pisan; la segunda recibe 409.
- Catálogo descuenta el stock de todas las líneas en **una sola operación todo o nada** (`POST /productos/stock/descontar`), que además devuelve el precio de cada producto.
- **Compensación:** si después de descontar falla el guardado de la nota en `WeCorpVentasDB`, Ventas llama a `POST /productos/stock/reponer` y luego informa el error.
- **Anulación:** primero se repone el stock en Catálogo y solo si responde bien la nota pasa a `Anulada`; si no, queda `Pendiente` y se responde 503.

## Alternativas descartadas
- **Transacción distribuida:** no es viable entre dos APIs con bases separadas.
- **Mensajería (cola) y consistencia eventual:** más robusta, pero excede el alcance de cuatro semanas.
- **Polly / Microsoft.Extensions.Http.Resilience:** válido, pero se prefirió una política propia y explícita, fácil de explicar y probar en la defensa.

## Consecuencias
- (+) El Vendedor recibe un error claro y el stock no queda descontado sin nota (salvo una falla también en la compensación, que queda registrada en el log).
- (−) Si Catálogo descuenta pero su respuesta no llega (timeout), la nota no se registra y ese stock queda descontado: el caso se registra en el log para revisión manual. La mejora futura es una **clave de idempotencia** por operación, que permitiría reintentar también los timeouts.
- (+) Al reponer se omiten los productos eliminados del catálogo, para que una nota antigua se pueda anular.
