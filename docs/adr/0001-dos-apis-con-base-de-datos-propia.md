# ADR 0001 — Dividir el sistema en dos APIs, cada una con su propia base de datos

- **Estado:** aceptada
- **Fecha:** semana 2 (Hito 2)
- **Requerimientos relacionados:** RF01–RF12, enunciado (al menos dos APIs independientes)

## Contexto
El Trabajo 2 implementó una única API con catálogo, usuarios y notas de venta. El enunciado de la Evaluación 3 exige al menos dos APIs independientes, cada una con responsabilidad propia y acceso a datos con EF Core y migraciones.

## Decisión
- **API de Catálogo e Identidad** (`api-catalogo`): usuarios, roles, login/JWT, familias, productos y stock. Base `WeCorpCatalogoDB`.
- **API de Ventas** (`api-ventas`): clientes y notas de venta. Base `WeCorpVentasDB`.
- Cada API accede **solo** a su base. Los datos de la otra se obtienen por HTTP.

## Consecuencias
- (+) Responsabilidades claras; cada integrante es dueño de una API (Bryan: Catálogo, Aníbal: Ventas).
- (+) Cada API se prueba y despliega por separado.
- (−) No existen claves foráneas entre bases: `NotasVenta.VendedorId` y `DetalleNotasVenta.ProductoId` no tienen FK. Por eso el detalle guarda una **copia** del SKU, nombre y precio al momento de la venta (también deja el historial intacto si el producto cambia de precio o se elimina).
- (−) Una venta toca dos bases: se resuelve con la integración HTTP y la compensación descritas en el ADR 0003.
