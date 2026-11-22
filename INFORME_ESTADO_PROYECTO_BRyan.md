# Informe de estado del proyecto Aceros We-Corp

**Para:** Bryan
**Revisión:** 22 de noviembre de 2026
**Repositorio:** [BryanvergaraS/aceros-wecorp-api](https://github.com/BryanvergaraS/aceros-wecorp-api)
**Rama de documentación de esta revisión:** `docs/auditoria-presentacion`

## A. Trabajo realizado en esta revisión

- Se revisó la estructura del monorepo, configuración, código de las dos APIs, cliente React, pruebas, README, ADR,
  contratos OpenAPI, matriz de trazabilidad y fuentes de diagramas PlantUML.
- Se siguieron en código los flujos de autenticación, permisos, CRUD, emisión/anulación, movimientos de stock,
  persistencia, compensación y los proxies del cliente.
- Se verificó el historial/estado remoto: PR #4 del cliente web ya está integrado; PR #3 de Ventas y PR #5 del cambio
  posterior del cliente siguen abiertos y solicitan a Bryan como revisor.
- Se añadió [`docs/guia-presentacion-y-usuario.md`](docs/guia-presentacion-y-usuario.md), con explicación del sistema,
  arquitectura, módulos, flujo, manual de ejecución/uso, conceptos de defensa, preguntas y respuestas y auditoría
  priorizada.
- Se corrigió el README para aclarar que las migraciones iniciales ya están incluidas y que `dotnet-ef` se necesita
  solamente para generar migraciones nuevas. También se agregó una referencia a la guía y la verificación web.
- No se modificó lógica de negocio en esta revisión. Los riesgos que requieren decisiones de autorización entre servicios
  o cambios de consistencia quedaron documentados, evitando una modificación que pudiera romper compensaciones.

## B. Funcionalidades implementadas en el estado revisado

| Área | Estado observado |
|---|---|
| Login, hash BCrypt, JWT HS256 y autorización Administrador/Vendedor | Implementado; incluye expiración de 60 minutos. |
| Gestión de productos y familias | CRUD administrativo, validación, búsqueda/filtro/paginación de productos. |
| Gestión de usuarios | Administración de roles y activación; no incluye cambio de contraseña en la pantalla de edición. |
| Clientes | CRUD, búsqueda/paginación y validación/normalización de RUT con módulo 11. |
| Notas de venta | Emisión con precio del catálogo, detalle histórico, estados Pendiente/Pagada/Anulada, listado y consulta según rol. |
| Movimiento de stock | Descuento/reposición desde Catálogo y compensación cuando falla el primer guardado de Ventas. |
| Cliente web | Login, navegación por rol, catálogo, familias, usuarios, clientes, nueva nota y listado/detalle de notas. |
| Documentación de diseño | ADR, contratos OpenAPI, diagramas, retrospectivas y trazabilidad ya están en el árbol. Las retrospectivas siguen como plantilla vacía. |

El estado funcional se refiere a lo encontrado en el código y a las pruebas enumeradas abajo; no significa que se haya
validado un despliegue conectado a SQL Server o Docker.

## C. Cambios registrados en Git

Commits de documentación creados durante esta revisión:

- `7855152` — `docs: agregar guia de usuario y defensa`: añade la guía de presentación/uso y corrige instrucciones del README.
- `f159038` — `docs: agregar informe de estado para Bryan`: incorpora este informe.
- `450349f` — `docs: ampliar hallazgos de auditoria`: agrega los hallazgos adicionales del folio temporal y el refresco
  silencioso de familias a la guía y al informe.
- Un commit posterior de documentación actualiza esta sección; su identificador final se indica en la entrega y en el
  historial de `docs/auditoria-presentacion`.

Estado de ramas/PR observado durante la revisión:

- PR [#6 documentación de auditoría](https://github.com/BryanvergaraS/aceros-wecorp-api/pull/6): abierto desde
  `docs/auditoria-presentacion`, con Bryan solicitado como revisor; la comprobación de GitHub indicó estado integrable
  al momento de crear el PR.
- PR [#3 API de Ventas](https://github.com/BryanvergaraS/aceros-wecorp-api/pull/3): abierto, rama `feature/api-ventas`,
  head `b93e278` (`fix(ventas): rechazar RUT con cuerpo cero`).
- PR [#5 corrección del cliente web](https://github.com/BryanvergaraS/aceros-wecorp-api/pull/5): abierto, rama
  `feature/cliente-web`, head `2c4a111` (`fix(cliente): reportar respuestas JSON inválidas`).
- PR [#4 cliente web inicial](https://github.com/BryanvergaraS/aceros-wecorp-api/pull/4): ya integrado en `main`.
- La documentación de esta revisión parte del `main` actualizado observado, commit `6b5f84d`. Los PR #3 y #5 no estaban
  integrados en ese `main` al momento de la revisión.

Los commits `b93e278` y `2c4a111` son cambios previos ya existentes en sus ramas; **no se cuentan como commits creados
durante esta revisión**.

## D. Estado y verificaciones

- `dotnet test WeCorp.sln -c Release`: **57/57 pruebas pasaron**, 31 de Ventas y 26 de Catálogo.
- `npm test`: **1/1 prueba pasó** en el árbol web revisado.
- `npm run build`: build de producción de Vite correcto.
- `git diff --check`: correcto en la rama preparada para documentación antes de registrar los cambios.
- Las pruebas de APIs usan `WebApplicationFactory`, EF Core InMemory y, en Ventas, un falso cliente de Catálogo. Las
  pruebas no acreditan el comportamiento de SQL Server real, aislamiento/concurrencia de ese proveedor o Docker.
- Se detectó Node `v24.19.0`; el requisito documentado es Node 22 LTS, por lo que la compatibilidad con Node 22 no se
  volvió a verificar en esta revisión.
- No se detectó el comando Docker ni el servicio local `MSSQL$SQLEXPRESS` en el entorno. Por eso Docker Compose,
  migraciones contra SQL Server y el flujo de navegador de punta a punta quedaron sin validar.
- No se encontró un archivo `.env` en la carpeta del proyecto ni un `.env` versionado en el árbol remoto inspeccionado.

## E. Pendientes

1. **Crítico — autorización de reposición:** `POST /api/v1/productos/stock/reponer` hereda autorización para
   Administrador y Vendedor. Un vendedor con JWT puede solicitar un aumento de stock directamente. El ADR 0002 lo
   reconoce. Definir identidad de servicio para Ventas→Catálogo y proteger reponer; conservar y probar la compensación.
2. **Crítico — resultado ambiguo de movimientos:** no hay idempotencia/correlación para una operación cuyo stock se
   aplicó pero cuya respuesta se perdió. Agregar idempotencia y una forma de reconciliar movimientos; hasta entonces,
   revisar stock/logs ante 503 antes de repetir.
3. **Importante — folio temporal persistido:** `CrearAsync` guarda primero la nota con `TMP-...`, y guarda el folio
   `NV-...` en una segunda operación fuera del bloque de compensación. Si ese segundo guardado falla, la nota puede
   permanecer registrada con un folio temporal, aunque Catálogo ya descontó el stock. Definir una persistencia atómica
   del registro/folio o una recuperación explícita y probar el fallo del segundo guardado.
4. Bryan debe revisar y aprobar los PR #3 y #5 antes de integrarlos, conforme al flujo de trabajo acordado.
5. Completar la revisión cruzada del PR de Catálogo cuando Bryan lo publique; la rama `feature/api-catalogo` no estaba
   disponible en el remoto durante la revisión.
6. Ejecutar integración manual con SQL Server y Docker Compose; probar login, crear cliente, emitir venta, confirmar
   descuento, anular y confirmar reposición.
7. Ejecutar las verificaciones con Node 22 LTS; ampliar pruebas de interfaz y de cancelación/compensación.
8. Alinear el diagrama de arquitectura que muestra HTTPS con la configuración que realmente sirve HTTP en local/Compose.
   El despliegue real debe usar TLS.
9. Revisar que un vendedor no pueda leer por `GET /productos/{id}` un producto inactivo, coherente con el filtro del listado.
10. Evitar que el refresco del caché de familias en `Familias.jsx` descarte errores con `.catch(() => {})`; mostrar o
    registrar el fallo para que el usuario sepa que el formulario puede estar usando datos desactualizados.
11. Completar las retrospectivas de `docs/gestion/retrospectivas.md` y mantener contratos/documentación sincronizados.

## F. Observaciones importantes

- La venta abarca dos bases separadas: no existe transacción distribuida. El código intenta compensar un fallo del primer
  guardado de Ventas, pero la compensación puede fallar si Catálogo tampoco responde.
- `CatalogoClient` limita reintentos a fallos de conexión donde la solicitud no llegó; no reintenta timeout ni 5xx para
  evitar duplicar un movimiento no idempotente.
- El folio se actualiza en una segunda escritura después de guardar la nota. Un fallo en esa escritura puede dejar el
  `TMP-...` persistido; también se descarta silenciosamente un error al refrescar el caché de familias. Ambos casos
  quedaron como pendientes, sin cambios de lógica en esta revisión.
- El JWT está en `sessionStorage` y no se revoca en servidor. Desactivar un usuario impide nuevos logins, pero tokens ya
  emitidos pueden seguir válidos hasta expirar.
- Las pruebas de concurrencia y restricciones de base requieren SQL Server para acreditar el comportamiento del proveedor
  real. La suite InMemory no es sustituto de esa prueba.
- Los diagramas/contratos describen la arquitectura, pero hay una diferencia de transporte HTTPS/HTTP que debe resolverse.
- El flujo es una nota de venta interna académica, no una factura electrónica, sistema de pagos ni integración con SII.
- Este informe y la guía describen el código revisado; las acciones sugeridas se mantienen como pendientes y no se marcan
  como funcionalidades terminadas.

## G. Próximos pasos ordenados por prioridad

1. Acordar e implementar control de acceso de servicio para el endpoint de reposición, con pruebas de autorización y
   regresión de compensación.
2. Diseñar idempotencia/correlación de movimientos y procedimiento de conciliación para timeouts.
3. Hacer atómica o recuperable la asignación del folio de la nota y agregar una prueba del fallo en la segunda escritura.
4. Reportar errores del refresco del caché de familias en vez de descartarlos.
5. Pedir a Bryan revisión de PR #3 y #5; integrar solo tras aprobación. Luego sincronizar la guía con el `main` resultante.
6. Completar revisión cruzada de Catálogo al existir su PR.
7. Ejecutar un ensayo completo en Docker/SQL Server y la prueba funcional desde la interfaz, guardando evidencia.
8. Corregir el esquema HTTP/HTTPS del despliegue y hacer pruebas con Node 22.
9. Cerrar documentación y retrospectivas, actualizar trazabilidad y repetir suite completa.
