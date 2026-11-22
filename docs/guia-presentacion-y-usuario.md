# Aceros We-Corp — guía de presentación y uso

Guía de estudio y operación basada en el código, la configuración, los contratos y las pruebas revisados. Describe el
alcance implementado; no implica que la ejecución contra SQL Server o Docker haya sido probada en esta revisión.

## 1. Resumen para la presentación

**Aceros We-Corp** es un sistema web interno para una barraca de fierros. Centraliza el catálogo y el stock de productos,
la administración de usuarios y clientes, y la emisión y seguimiento de notas de venta.

El problema que aborda es mantener en un mismo flujo el catálogo, el inventario y las ventas, evitando registrar una
venta con productos inactivos o sin stock y conservando el precio y la descripción de lo vendido. Sus usuarios son:

- **Administrador:** gestiona catálogo, familias y usuarios; consulta las ventas y puede marcarlas pagadas o anularlas.
- **Vendedor:** consulta productos activos, registra y edita clientes, emite notas y consulta solo sus propias notas.

La propuesta de valor es tener un flujo simple de venta, acceso por rol, historial de venta con precios al momento de
la operación y sincronización de stock entre dos APIs. Es una solución académica/prototipo; no reemplaza un sistema
contable ni emite documentos tributarios electrónicos.

## 2. Arquitectura y tecnologías

```text
Navegador (React)
  ├─ /catalogo/api/v1/* ──> API Catálogo e Identidad ──> WeCorpCatalogoDB
  └─ /ventas/api/v1/* ────> API Ventas ───────────────> WeCorpVentasDB
                                  └─ HTTP + JWT ──────> API Catálogo
```

- **Cliente web:** React 19, React Router y Vite. Es una SPA con rutas de carga diferida. En desarrollo Vite hace proxy
  de `/catalogo` y `/ventas`; en Docker Nginx entrega los archivos compilados y hace el mismo proxy. El navegador usa
  un origen y no necesita configurar CORS para este flujo.
- **API de Catálogo e Identidad:** ASP.NET Core 10 con controladores versionados bajo `/api/v1`. Emite y valida el JWT,
  autentica con BCrypt y administra roles, familias, productos y movimientos de stock.
- **API de Ventas:** ASP.NET Core 10 con controladores, servicios y repositorios. Administra clientes y notas, valida el
  JWT emitido por Catálogo y consume el catálogo mediante `IHttpClientFactory`.
- **Persistencia:** EF Core 10 y SQL Server. Cada API posee su base, DbContext y migraciones; al iniciar aplica
  `Database.Migrate()`. Las pruebas de integración reemplazan SQL Server por EF Core InMemory.
- **Ejecución:** Docker Compose define SQL Server, ambas APIs y Nginx. Las imágenes de las APIs son multi-stage.
- **Contrato y diseño:** OpenAPI 3, Swagger, diagramas PlantUML, ADRs y matriz de trazabilidad en `docs/`.
- **Pruebas revisadas:** xUnit y `WebApplicationFactory` para las APIs; Node test runner para una prueba focalizada del
  cliente HTTP web.

Se eligieron dos APIs para separar responsabilidades y datos, como exige el alcance del Trabajo 3. EF Core reduce
código repetido de persistencia y sus migraciones versionan el esquema. JWT permite compartir identidad entre APIs sin
que Ventas almacene contraseñas. `IHttpClientFactory` centraliza la conexión a Catálogo y habilita timeout/configuración.
React/Vite permite una interfaz ligera con desarrollo rápido; Docker Compose facilita ejecutar las dependencias juntas.

**Precisión para la defensa:** son dos APIs separadas por responsabilidad y base, pero el flujo de venta sigue
dependiendo sincrónicamente de Catálogo y del SQL Server. No afirmes que ofrece independencia de disponibilidad o
consistencia distribuida completa.

## 3. Responsabilidad de los módulos

### API de Catálogo (`api-catalogo/WeCorp.Catalogo.Api`)

- `Program.cs`: configuración, lectura de `.env` en Development, validación de opciones, EF Core, DI, JWT, Swagger,
  health check, migraciones y pipeline.
- `AuthController`, `AuthService`, `TokenService`: validan email/contraseña y emiten JWT HS256 con id, nombre, email y
  rol. Las contraseñas se almacenan como hashes BCrypt.
- `ProductosController` y `ProductoRepository`: CRUD de productos, búsqueda por nombre/SKU, filtro por familia y
  paginación. El vendedor ve productos activos en el listado; el administrador ve también inactivos.
- `FamiliasController` y `FamiliaRepository`: consulta y CRUD de familias; impide eliminar una familia con productos.
- `UsuariosController` y `UsuarioRepository`: operaciones administrativas de usuarios y roles; la contraseña solo se
  define al crear el usuario. No permite al administrador desactivarse a sí mismo.
- `StockController`, `StockService`: verifica todas las líneas antes de modificar el stock; agrupa líneas repetidas y
  guarda el movimiento en una operación de EF Core. Al descontar devuelve el precio vigente.
- `AppDbContext`, modelos, `SeedData`, migraciones: restricciones, índices, relaciones y datos de catálogo de ejemplo.
  `DbInitializer` crea el administrador/vendedor solo si existen sus variables `Seed__*`.
- `ExceptionHandlingMiddleware`: transforma errores de negocio y persistencia a `ProblemDetails`.

### API de Ventas (`api-ventas/WeCorp.Ventas.Api`)

- `Program.cs`: configuración de base, JWT, cliente HTTP tipado, Swagger, health check, migraciones y middleware.
- `ClientesController`/`ClienteRepository`: CRUD, búsqueda paginada y unicidad de RUT. El RUT se normaliza y valida con
  módulo 11.
- `NotasVentaController`/`NotaVentaRepository`: listado filtrable por estado/fechas, detalle y cambio de estado. El
  vendedor solo recibe sus propias notas; el cambio de estado es administrativo.
- `NotaVentaService`: valida cliente, agrupa productos, coordina descuento y persistencia, guarda detalles históricos,
  calcula el total y realiza compensación si falla el primer guardado.
- `CatalogoClient`: llama a Catálogo con timeout de 5 s; reintenta hasta dos veces solo cuando reconoce que la conexión
  no llegó a establecerse. Traduce errores de Catálogo a errores HTTP del dominio.
- `ReenviarTokenHandler`: reenvía al catálogo el bearer token del usuario que llamó a Ventas.
- Modelos y EF Core: `Cliente`, `NotaVenta`, `DetalleNotaVenta`; FK local a Cliente y detalle, snapshots del producto
  vendido y sin FK a las bases externas.

### Cliente web (`cliente-web/src`)

- `api/cliente.js`: fetch, JWT, manejo de `ProblemDetails`, errores de API y vencimiento de sesión.
- `context/Sesion.jsx`: inicio/cierre de sesión, temporizador de expiración y caché de familias durante la sesión.
- `hooks/useDatos.js`: carga cancelable de datos y debounce de búsquedas.
- `components/`: formularios, diálogos, mensajes, paginación, estados y barra de stock.
- `pages/`: login, catálogo, familias, usuarios, clientes, nueva nota y listado/detalle de notas.
- `App.jsx` y `Layout.jsx`: rutas protegidas y navegación condicionada por rol.
- `estilos.css`: diseño adaptable, indicadores visuales, foco visible y preferencia de movimiento reducido.

## 4. Flujo de principio a fin

1. El usuario inicia sesión en Catálogo. `AuthService` busca al usuario, comprueba el hash BCrypt y `TokenService` firma
   un JWT de 60 minutos. El cliente guarda el token en `sessionStorage`.
2. El cliente envía `Authorization: Bearer ...` a ambas APIs. Las dos verifican firma, emisor, audiencia y expiración;
   los controladores aplican autorización por rol.
3. Para emitir una nota, Ventas comprueba que exista el cliente, combina cantidades repetidas y llama a
   `POST /api/v1/productos/stock/descontar` en Catálogo con el JWT reenviado.
4. Catálogo comprueba que los productos existan, estén activos y tengan stock. Verifica todo antes de modificar y
   devuelve SKU, nombre, precio vigente y stock resultante.
5. Ventas crea la nota Pendiente y sus detalles con una copia de SKU, nombre y precio. Calcula el total y asigna un
   folio `NV-...`. Si falla el guardado inicial intenta reponer el stock.
6. Para anular, el Administrador primero solicita la reposición a Catálogo; solo después cambia la nota a Anulada. Una
   nota Pendiente también puede marcarse Pagada. Los estados Pagada y Anulada son finales.

**Consistencia entre bases:** no hay una transacción ACID que abarque ambas APIs. Si Catálogo ya modificó stock pero la
respuesta se pierde por timeout/cancelación, Ventas no puede saber con certeza si debe repetir. No se reintentan esos
casos para evitar un doble descuento; falta una clave de idempotencia para resolverlos de forma robusta. La compensación
tampoco es infalible si Catálogo está caído al reponer.

## 5. Manual práctico de usuario

### 5.1 Preparación y arranque

Requisitos de desarrollo indicados por el proyecto: .NET SDK 10, Node.js 22 LTS/npm y SQL Server Express/LocalDB, o
Docker Desktop. Las pruebas y compilación de esta revisión se ejecutaron con .NET 10.0.402 y Node 24.19.0; Node 22 y
Docker no se verificaron en esta revisión.

1. En la raíz del proyecto copie `.env.example` a `.env` y complete valores locales. La llave JWT debe tener al menos
   32 caracteres; use la misma para ambas APIs. Defina las dos conexiones, credenciales iniciales del administrador y
   vendedor, `Catalogo__BaseUrl` y `SA_PASSWORD` si utilizará Docker. No comparta ni suba `.env`.
2. **Opción Docker:** desde la raíz ejecute `docker compose up --build`. Cliente: `http://localhost:8080`;
   Swagger Catálogo: `http://localhost:5081/swagger`; Swagger Ventas:
   `http://localhost:5082/swagger`. En Docker Compose las APIs usan `http://api-catalogo:8080` entre contenedores.
3. **Opción local:** asegúrese de que SQL Server esté iniciado y que las conexiones apunten a la instancia correcta.
   Abra tres terminales en la raíz y ejecute:

   ```powershell
   dotnet run --project api-catalogo\WeCorp.Catalogo.Api
   dotnet run --project api-ventas\WeCorp.Ventas.Api
   cd cliente-web
   npm ci
   npm run dev
   ```

   Cliente: `http://localhost:5173`; Swagger y health checks en los puertos anteriores. Las migraciones ya están
   incluidas y las APIs las aplican al iniciar. `dotnet-ef` solo hace falta para generar migraciones nuevas.
4. Inicie sesión con el email y contraseña definidos en `Seed__AdminEmail`/`Seed__AdminPassword` o
   `Seed__VendedorEmail`/`Seed__VendedorPassword`. Si el usuario ya existe, el inicializador no le cambia la contraseña.

Para apagar, detenga cada proceso con Ctrl+C o use `docker compose down`. Evite `docker compose down -v` salvo que
quiera borrar deliberadamente el volumen de datos de SQL Server.

### 5.2 Navegación y operaciones

| Sección | Quién puede usarla | Uso y resultado |
|---|---|---|
| **Catálogo** | Administrador y Vendedor | Busca por nombre/SKU, filtra por familia y pagina. La barra y el aviso muestran stock bajo el mínimo. El Administrador ve inactivos y dispone de **Nuevo producto**, **Editar** y **Eliminar**. El vendedor solo ve activos en el listado. |
| **Familias** | Administrador | **Nueva familia**, **Editar**, activar/desactivar y **Eliminar**. La API rechaza borrar una familia asociada a productos. |
| **Usuarios** | Administrador | **Nuevo usuario** crea nombre, email, contraseña (mínimo 8) y rol. **Editar** permite cambiar datos, rol y estado activo; no se puede desactivar a uno mismo. La contraseña y el email no se editan desde esta pantalla. |
| **Clientes** | Administrador y Vendedor | Busca por RUT/nombre, pagina, usa **Nuevo cliente** o **Editar**. La API normaliza/verifica RUT y no acepta duplicados. Solo el Administrador ve **Eliminar**; no se puede borrar un cliente con notas asociadas. |
| **Nueva nota de venta** | Administrador y Vendedor | Busca cliente por RUT/nombre o usa **Nuevo cliente**; busca productos por nombre/SKU, agrega productos disponibles, ajusta cantidad dentro del stock, o pulsa **Quitar**. Agrega observaciones opcionales y pulsa **Emitir nota de venta**. El sistema valida precio y stock vigentes en Catálogo. |
| **Notas de venta** | Administrador y Vendedor | Filtra por estado y rango de fechas, pagina y abre **Ver detalle** para revisar cliente, vendedor, líneas, precios y total. El vendedor solo ve sus notas. |
| **Detalle de nota** | Ambos; acciones solo Administrador | En una nota Pendiente el Administrador puede **Marcar como pagada** o **Anular nota**. Al anular, se solicita reponer el stock. No se permiten cambios desde un estado final. |
| **Cerrar sesión** | Ambos | Limpia el token y vuelve al login. La interfaz también cierra la sesión al detectar expiración o un 401 autenticado. |

### 5.3 Errores habituales

- **No inicia la API / falta `Jwt__Key`:** compruebe `.env` en la raíz, llave de 32+ caracteres y nombres exactos de las variables.
- **Error de SQL:** verifique que SQL Server esté iniciado y que la instancia/puerto y credenciales coincidan con ambas cadenas.
- **Puerto ocupado:** cierre el proceso que use 5081, 5082, 5173 o 8080, o resuelva el conflicto antes de reiniciar.
- **401:** faltan credenciales o expiró el JWT; vuelva a iniciar sesión.
- **403:** el usuario autenticado no tiene el rol permitido para esa operación.
- **400:** revise los campos requeridos y el formato informado por el formulario.
- **404:** el registro no existe; para un vendedor, una nota ajena también se presenta como no encontrada.
- **409:** RUT/SKU/email duplicado, recurso en uso o conflicto de concurrencia del stock.
- **422:** regla de negocio, por ejemplo stock insuficiente, producto inactivo o transición de estado no permitida.
- **503 al emitir/anular:** Catálogo no respondió o falló. No repita muchas veces a ciegas: si el timeout ocurrió después de
  aplicar el movimiento, puede existir una diferencia temporal de stock. Revise logs y stock antes de reintentar.
- **Cliente web muestra que no conecta:** confirme que ambas APIs estén arriba y que Vite/Nginx esté usando los prefijos proxy.

## 6. Conceptos para responder preguntas

- **JWT:** token firmado con claims. La firma permite detectar cambios; no cifra su contenido. En este proyecto contiene
  identidad/rol y se valida con HS256, emisor, audiencia y expiración.
- **BCrypt:** hash adaptativo de contraseña; se verifica con `BCrypt.Verify`, no se descifra.
- **Autenticación vs autorización:** demostrar identidad con JWT vs decidir permisos con rol Administrador/Vendedor.
- **DTO:** objeto de transferencia que define entradas/salidas HTTP y valida campos, separado de las entidades persistidas.
- **EF Core / DbContext / migración:** ORM, unidad de trabajo de una solicitud y cambios versionados del esquema SQL.
- **Repository / Service / DI:** repositorios encapsulan consultas; servicios coordinan reglas de negocio; inyección de
  dependencias desacopla implementaciones y facilita sustituir dependencias en pruebas.
- **Concurrencia optimista:** `Stock` es token de concurrencia. Si una actualización usa un valor que cambió, el guardado
  falla y se traduce a 409 en lugar de pisar silenciosamente el stock de otra venta.
- **HTTP tipado:** `CatalogoClient` abstrae llamadas a Catálogo; `IHttpClientFactory` administra configuración y ciclo de vida.
- **Idempotencia:** repetir una solicitud produce el mismo efecto una sola vez. Los movimientos actuales no tienen una
  clave idempotente; por eso un timeout es ambiguo y no se reintenta.
- **ProblemDetails:** formato estándar de errores HTTP que da código, título, detalle e instancia.
- **SPA/proxy:** aplicación React que cambia vistas en el navegador; Vite/Nginx enruta las llamadas API al backend.
- **Prueba de integración:** levanta el pipeline de API; aquí EF InMemory sustituye SQL Server y Ventas usa un falso de Catálogo.
  No prueba por sí sola el comportamiento real de SQL Server, Docker o la experiencia visual completa.

## 7. Preguntas probables y respuestas fundamentadas

**¿Por qué dos bases de datos?** Para que cada API sea dueña de sus datos y cumpla el requisito de responsabilidades
separadas. La consecuencia es que no hay FK entre productos/vendedores y ventas ni transacción compartida.

**¿Qué pasa si no alcanza el stock?** Catálogo valida todas las líneas antes de guardar. Responde 422; Ventas no registra
la nota. Dos ventas concurrentes no deben sobrescribirse: `Stock` es token de concurrencia y el conflicto responde 409.

**¿Por qué no se reintenta un timeout?** Porque el servidor pudo descontar stock y la respuesta perderse. Repetir una
operación no idempotente podría descontar dos veces. Solo se reintentan fallos identificados de conexión que no llegó a
establecerse; la solución futura es idempotencia/correlación y conciliación.

**¿Qué conserva el detalle si cambia/elimina un producto?** SKU, nombre y precio unitario se copian al detalle de la nota.
`ProductoId` queda como referencia informativa sin FK entre bases.

**¿Qué hace anular?** Solo el Administrador puede cambiar estado. La reposición se solicita primero; si falla, la nota se
mantiene Pendiente. Pagada y Anulada son estados finales.

**¿Es una factura electrónica?** No. Es una nota de venta interna del prototipo, sin integración tributaria, pago ni SII.

**¿Cómo se probaron las APIs?** 57 pruebas xUnit: 26 Catálogo y 31 Ventas; se ejecutan con `WebApplicationFactory`,
  EF InMemory y doble de Catálogo. Además, una prueba Node verifica el tratamiento de JSON malformado y Vite produce
  build. No se verificó SQL Server real, Docker Compose ni un E2E completo de navegador en esta revisión.

## 8. Auditoría: hallazgos y mejoras priorizadas

### Prioridad crítica antes de un despliegue real

1. **`POST /api/v1/productos/stock/reponer` acepta el rol Vendedor.** `StockController` autoriza a ambos roles a nivel de
   clase y no reduce el permiso de `Reponer`; un vendedor con JWT puede solicitar aumentos arbitrarios de stock sin
   emitir/anular una nota. El ADR 0002 ya reconoce el riesgo. Definir identidad/autorización de servicio para
   Ventas→Catálogo y conservar la capacidad de compensación, con pruebas que demuestren que un vendedor no puede invocar
   reponer directamente. No basta con restringir a Admin sin revisar compensaciones.
2. **Consistencia ante resultados ambiguos.** Si Catálogo aplica una operación y se pierde la respuesta por timeout o
   cancelación, no existe clave idempotente ni conciliación automática. Agregar idempotencia por operación y registro de
   movimientos con correlación; documentar un procedimiento de conciliación mientras tanto.

### Recomendables antes de entrega/producción

- Hacer una demostración E2E con dos bases SQL Server reales: login, emisión, descuento, anulación y stock restaurado.
- Probar en Docker Compose y revisar health/readiness de todas las dependencias. En esta revisión no se detectó Docker ni
  una instancia local `MSSQL$SQLEXPRESS`.
- Alinear ADR/diagramas con el transporte real: el diagrama de arquitectura muestra HTTPS, pero launchSettings, Compose y
  servidores locales exponen HTTP. Usar TLS en el despliegue real.
- Restringir `GET /api/v1/productos/{id}` para que un Vendedor no pueda consultar por ID un producto inactivo, coherente
  con el filtro del listado.
- Evaluar limitación de intentos de login, auditoría de cambios de stock/estado y revocación de sesión. El JWT dura 60
  minutos y no hay revocación inmediata al desactivar un usuario.
- Probar concurrencia, conflictos de unicidad y compensación contra SQL Server. EF InMemory no equivale a SQL Server y
  no acredita sus restricciones/aislamiento.
- Añadir más pruebas de UI y de errores/cancelación. La prueba web actual cubre solo la respuesta exitosa con JSON inválido.
- Completar las retrospectivas: `docs/gestion/retrospectivas.md` contiene secciones vacías.
- Revisar la rama de Catálogo/PR de Bryan; no se mezcló en `main` durante esta revisión.

### Mejoras futuras

- Idempotencia, patrón outbox/saga o libro de movimientos para mejorar tolerancia a fallos entre APIs.
- Observabilidad distribuida: correlation ID, métricas/trazas y alertas de fallos de compensación.
- Búsqueda escalable: índices/búsqueda normalizada y medición demostrable del requisito de latencia; `Contains` no acredita
  por sí solo el umbral de 500 ms.
- Recuperación de contraseña, edición controlada de credenciales, cierre/revocación de sesiones y políticas de contraseña.
- Auditoría de usuario/fecha en cada modificación, respaldo/restauración y datos de prueba separados de datos productivos.
- Pruebas visuales accesibles y responsive a 360 px; el CSS implementa breakpoints, pero no se midió visualmente en esta auditoría.

## 9. Lista breve para preparar la defensa

1. Dibujar las dos APIs y sus dos bases en una hoja y explicar quién es dueño de cada dato.
2. Explicar el flujo de login y nombrar los claims del JWT.
3. Seguir una nota de venta línea por línea: cliente → descuento/precio → snapshot del detalle → folio.
4. Contrastar 401, 403, 404, 409, 422 y 503 con ejemplos del proyecto.
5. Explicar por qué no hay transacción distribuida ni reintento de timeout; reconocer la limitación y la mejora de idempotencia.
6. Mostrar responsabilidades de Admin/Vendedor y hacer una prueba manual de una nota.
7. Reconocer con transparencia el permiso de reponer stock, la falta de HTTPS para producción y las verificaciones que aún faltan.
