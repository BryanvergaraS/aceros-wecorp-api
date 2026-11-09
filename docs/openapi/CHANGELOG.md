# Registro de cambios de los contratos OpenAPI

Los contratos se diseñaron **antes** de implementar (Hito 2, semana 2). Cualquier cambio posterior se registra aquí con su motivo.

| Fecha | Contrato | Cambio | Motivo |
|---|---|---|---|
| Semana 2 | catalogo-v1.yaml | Versión inicial | Diseño primero |
| Semana 2 | ventas-v1.yaml | Versión inicial | Diseño primero |
| Semana 2 (revisión) | ventas-v1.yaml | `GET /api/v1/notas-venta` agrega la respuesta 400 | Un filtro `estado` fuera de Pendiente/Pagada/Anulada se rechaza con ValidationProblemDetails en vez de ignorarse (detectado en la revisión del diseño) |
| Semana 2 (revisión) | catalogo-v1.yaml | `POST /productos/stock/descontar` agrega 409; `POST /productos/stock/reponer` ya no responde 404 | Control de concurrencia optimista sobre el stock; al anular una nota antigua se omiten los productos eliminados del catálogo |
