# Matriz de trazabilidad

Requerimiento → historia de usuario → caso de uso → endpoint → prueba que lo verifica.
Contratos: `docs/openapi/catalogo-v1.yaml` (C) y `docs/openapi/ventas-v1.yaml` (V).

## Requerimientos funcionales

| RF | Historia | CU | Endpoint(s) | API | Prueba(s) |
|---|---|---|---|---|---|
| RF01 Iniciar sesión y emitir JWT | HU01 | CU01 | `POST /api/v1/auth/login` | C | `AuthTests.Login_ConCredencialesValidas_DevuelveTokenYRol`, `AuthTests.Login_ConClaveIncorrecta_Devuelve401EnProblemDetails`, `AuthTests.Login_SinEmail_Devuelve400` |
| RF02 Consultar catálogo con búsqueda, filtro y paginación | HU03 | CU02 | `GET /api/v1/productos`, `GET /api/v1/productos/{id}` | C | `ProductosTests.Listar_ComoVendedor_DevuelveProductosPaginados`, `ProductosTests.Buscar_PorTexto_FiltraResultados`, `ProductosTests.Listar_ComoVendedor_NoMuestraInactivos` |
| RF03 CRUD de productos (Administrador) | HU04 | CU03 | `POST`, `PUT`, `DELETE /api/v1/productos[/{id}]` | C | `ProductosTests.CrudCompleto_ComoAdmin`, `ProductosTests.Crear_ConSkuRepetido_Devuelve409`, `ProductosTests.Crear_ConDatosInvalidos_Devuelve400`, `ProductosTests.Crear_ComoVendedor_Devuelve403`, `ProductosTests.Crear_ConFamiliaInexistente_Devuelve422` |
| RF04 CRUD de familias | HU05 | CU04 | `GET`, `POST`, `PUT`, `DELETE /api/v1/familias[/{id}]` | C | `UsuariosTests.EliminarFamilia_ConProductos_Devuelve409`, `UsuariosTests.CrearFamilia_ConNombreRepetido_Devuelve409` |
| RF05 Gestionar usuarios | HU06 | CU05 | `GET`, `POST /api/v1/usuarios`, `PUT /api/v1/usuarios/{id}`, `GET /api/v1/usuarios/roles` | C | `UsuariosTests.CrearUsuario_ConEmailRepetido_Devuelve409`, `UsuariosTests.UsuarioDesactivado_NoPuedeIniciarSesion` |
| RF06 CRUD de clientes con RUT válido y único | HU07, HU08 | CU06 | `GET`, `POST /api/v1/clientes`, `GET`, `PUT`, `DELETE /api/v1/clientes/{id}` | V | `ClientesTests.Crear_ConRutValido_Devuelve201_YNormalizaElRut`, `ClientesTests.Crear_ConDigitoVerificadorIncorrecto_Devuelve400`, `ClientesTests.Crear_ConRutRepetido_Devuelve409`, `ClientesTests.Eliminar_ComoVendedor_Devuelve403`, `ClientesTests.Eliminar_ClienteConNotas_Devuelve409`, `ClientesTests.Eliminar_ClienteSinNotas_Devuelve204` |
| RF07 Emitir nota de venta | HU09 | CU07 | `POST /api/v1/notas-venta` | V | `NotasVentaTests.Emitir_Devuelve201_ConFolioTotalYPrecioDelCatalogo_YDescuentaStock`, `NotasVentaTests.Emitir_ConClienteInexistente_Devuelve404`, `NotasVentaTests.Emitir_SinLineas_Devuelve400` |
| RF08 Verificar y descontar stock | HU09 | CU08 | `POST /api/v1/productos/stock/descontar` (C), llamado desde `POST /api/v1/notas-venta` (V) | C + V | `StockTests.Descontar_ComoVendedor_DescuentaYDevuelvePrecio`, `StockTests.Descontar_ConStockInsuficiente_Devuelve422_YNoDescuentaNada`, `StockTests.Descontar_ProductoInactivo_Devuelve422`, `StockTests.Descontar_ProductoInexistente_Devuelve404`, `NotasVentaTests.Emitir_ConStockInsuficiente_Devuelve422_YNoRegistraLaNota`, `CatalogoClientTests.Respuesta200_DevuelveLasLineasConPrecio`, `CatalogoClientTests.Respuesta422_LanzaBusinessRule_ConElDetalle_YNoReintenta`, `CatalogoClientTests.Respuesta404_LanzaNotFound`, `CatalogoClientTests.Respuesta409_LanzaConflict` |
| RF09 Consultar notas (Vendedor solo las propias) | HU10 | CU09 | `GET /api/v1/notas-venta`, `GET /api/v1/notas-venta/{id}` | V | `NotasVentaTests.Vendedor_NoVeNotasDeOtroVendedor`, `NotasVentaTests.Admin_VeNotasDeTodosLosVendedores` |
| RF10 Cambiar estado de una nota | HU11 | CU10 | `PATCH /api/v1/notas-venta/{id}/estado` | V | `NotasVentaTests.CambiarEstado_DeNotaPagada_Devuelve422`, `NotasVentaTests.CambiarEstado_ComoVendedor_Devuelve403`, `NotasVentaTests.CambiarEstado_AEstadoInvalido_Devuelve400` |
| RF11 Reponer stock al anular | HU11 | CU11 | `POST /api/v1/productos/stock/reponer` (C), llamado desde `PATCH .../estado` (V) | C + V | `StockTests.Reponer_SumaStock`, `NotasVentaTests.Anular_ReponeElStock` |
| RF12 Cerrar sesión y aviso de sesión expirada | HU02 | CU01 | Cualquier endpoint protegido (401) | Cliente | `AuthTests.EndpointProtegido_ConTokenVencido_Devuelve401`, `SeguridadTests.TokenVencido_Devuelve401`; en el cliente: `src/api/cliente.js` y `src/context/Sesion.jsx` |

## Requerimientos no funcionales

| RNF | Cómo se cumple | Verificación |
|---|---|---|
| RNF01 Contraseñas con BCrypt | `AuthService`, `UsuariosController` (`BCrypt.HashPassword`/`Verify`) | Consulta a `Usuarios.PasswordHash` en SSMS |
| RNF02 JWT HS256 de 60 min validado por ambas APIs | `TokenService` (C); `AddJwtBearer` con emisor, audiencia, firma y expiración (C y V) | `AuthTests.EndpointProtegido_ConTokenVencido_Devuelve401`, `SeguridadTests.TokenVencido_Devuelve401`, `SeguridadTests.TokenConFirmaInvalida_Devuelve401`, `SeguridadTests.TokenValido_Devuelve200` |
| RNF03 401 sin token y 403 sin permiso | `[Authorize]` en `ApiControllerBase` y `[Authorize(Roles = ...)]` | `AuthTests.EndpointProtegido_SinToken_Devuelve401`, `AuthTests.EndpointDeAdmin_ConVendedor_Devuelve403`, `SeguridadTests.SinToken_Devuelve401`, `NotasVentaTests.CambiarEstado_ComoVendedor_Devuelve403` |
| RNF04 Secretos solo en variables de entorno | `.env.example` sin valores, `.gitignore` con `.env`, `ArchivoEnv` | Revisión del repositorio |
| RNF05 Listado paginado < 500 ms | `ProductoRepository.BuscarAsync` (proyección a DTO, `AsNoTracking`, índices) | Colección de pruebas de la API |
| RNF06 Timeout 5 s, hasta 2 reintentos (si la petición no llegó) y 503 | `CatalogoClient` (V) | `NotasVentaTests.Emitir_ConCatalogoCaido_Devuelve503_YNoRegistraLaNota`, `CatalogoClientTests.ConexionRechazada_LuegoExito_Reintenta`, `CatalogoClientTests.ConexionRechazadaPersistente_LanzaServicioNoDisponible_TrasTresIntentos`, `CatalogoClientTests.Error500_NoReintenta_YLanzaServicioNoDisponible`, `CatalogoClientTests.TiempoDeEsperaAgotado_LanzaServicioNoDisponible_SinReintentar` |
| RNF07 docker compose y `/health` | `docker-compose.yml`, `MapHealthChecks("/health")` | `AuthTests.Health_SinToken_Devuelve200`, `SeguridadTests.Health_SinToken_Devuelve200` y ejecución con Docker |
| RNF08 Carga y errores en español | `cliente-web/src/api/cliente.js`, componentes `Cargando` y `Aviso` | Demostración |
| RNF09 Usable desde 360 px | `cliente-web/src/estilos.css` | Vista móvil del navegador |
| RNF10 ProblemDetails, `/api/v1` y Swagger | `ExceptionHandlingMiddleware`, `Asp.Versioning`, Swashbuckle | `ProductosTests.Crear_ConDatosInvalidos_Devuelve400` (problem+json) |
