# API Aceros We-Corp

Trabajo 2 de Construcción de Software. Autor: Bryan Vergara Santander.

API RESTful en ASP.NET Core (.NET 10) para gestionar el catálogo de productos de **Aceros We-Corp**, barraca de fierros de Nacimiento, Biobío. El sistema tiene dos roles: el **Administrador** gestiona el catálogo y el **Vendedor** lo consulta y emite notas de venta. Incluye un frontend en React.

## Estructura

```
AcerosWeCorp/
├── database/                 Script SQL (AcerosWeCorpDB.sql) y diagrama de la base de datos
├── backend/
│   ├── AcerosWeCorp.Api/     API: controladores, EF Core, repositorios, middleware, JWT
│   └── AcerosWeCorp.Api.Tests/  Pruebas de integración (xUnit + WebApplicationFactory)
└── frontend/                 React + Vite
```

## Cómo cumple cada requisito

| Requisito | Dónde está |
| --- | --- |
| Web API con controladores (sin Minimal API) | `backend/AcerosWeCorp.Api/Controllers` |
| Middleware propio | `Middleware/RequestLoggingMiddleware.cs` guarda cada petición en la tabla LogsAuditoria. `Middleware/ExceptionHandlingMiddleware.cs` convierte los errores en ProblemDetails |
| Inyección de dependencias con scopes | `Program.cs`: DbContext, repositorios y servicios son **Scoped**; `TokenService` es **Singleton** |
| CRUD REST con validación y errores | `ProductosController` (GET, POST, PUT, DELETE), validación con DataAnnotations en `Dtos/`, códigos 200/201/204/400/401/403/404/409/422 |
| ORM, DbContext y migraciones | `Data/AppDbContext.cs` mapea las 9 tablas del script con sus claves, únicos, CHECK y relaciones |
| Patrón Repository | `Repositories/` (repositorio genérico más uno por entidad) |
| Versionamiento | Por URL: `/api/v1/...` (Asp.Versioning) |
| Swagger | `http://localhost:5080/swagger`, con el botón Authorize para el JWT |
| Login con JWT y 2 roles | `POST /api/v1/auth/login`; `[Authorize(Roles = ...)]` con Administrador y Vendedor |
| Secretos fuera del repositorio | La llave JWT, la cadena de conexión y las claves iniciales se leen de user-secrets o variables de entorno |
| Pruebas de integración | `backend/AcerosWeCorp.Api.Tests` |

## Endpoints (v1)

| Verbo | Ruta | Rol |
| --- | --- | --- |
| POST | `/api/v1/auth/login` | Público |
| GET | `/api/v1/productos?buscar=&familiaId=&pagina=&tamano=` | Administrador, Vendedor |
| GET | `/api/v1/productos/{id}` | Administrador, Vendedor |
| POST / PUT / DELETE | `/api/v1/productos[/{id}]` | Administrador |
| GET / POST / DELETE | `/api/v1/productos/{id}/proveedores[/{proveedorId}]` | Administrador |
| GET | `/api/v1/familias[/{id}]` | Administrador, Vendedor |
| POST / PUT / DELETE | `/api/v1/familias[/{id}]` | Administrador |
| GET / POST / PUT / DELETE | `/api/v1/proveedores[/{id}]` | Administrador |
| GET / POST | `/api/v1/notas-venta[/{id}]` | Administrador (todas), Vendedor (las suyas) |
| PATCH | `/api/v1/notas-venta/{id}/estado` | Administrador |
| GET / POST | `/api/v1/usuarios[/{id}]`, GET `/api/v1/usuarios/roles` | Administrador |
| GET | `/api/v1/logs-auditoria` | Administrador |

## Requisitos

- .NET SDK 10
- SQL Server (Express o Developer) o LocalDB
- Node.js 20 o superior
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

## 1. Configurar los secretos (una sola vez)

Los secretos se guardan con **user-secrets**, que quedan en tu usuario de Windows y no en el proyecto. Desde la carpeta `backend/AcerosWeCorp.Api`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=AcerosWeCorpDB;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "una-llave-larga-de-al-menos-32-caracteres-123456"
dotnet user-secrets set "Seed:AdminEmail" "admin@wecorp.cl"
dotnet user-secrets set "Seed:AdminPassword" "CambiarEstaClave123"
dotnet user-secrets set "Seed:VendedorEmail" "vendedor@wecorp.cl"
dotnet user-secrets set "Seed:VendedorPassword" "CambiarEstaClave123"
```

Si usas LocalDB, la cadena es `Server=(localdb)\MSSQLLocalDB;Database=AcerosWeCorpDB;Trusted_Connection=True`.

También se pueden usar variables de entorno con doble guion bajo: `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Seed__AdminEmail`, etc.

## 2. Crear la base de datos con migraciones

```powershell
cd backend/AcerosWeCorp.Api
dotnet ef migrations add InicialCreate
dotnet ef database update
```

La migración crea las 9 tablas e inserta los datos de ejemplo del script: roles, familias, productos y proveedores. Al arrancar, la API crea el administrador y el vendedor con las credenciales de los secretos.

`database/AcerosWeCorpDB.sql` crea la misma base directamente en SQL Server, sin pasar por EF Core.

## 3. Ejecutar la API

```powershell
cd backend/AcerosWeCorp.Api
dotnet run
```

Swagger queda en http://localhost:5080/swagger. Primero llama a `POST /api/v1/auth/login`, copia el `token` y pégalo en **Authorize**.

## 4. Ejecutar las pruebas de integración

```powershell
cd backend
dotnet test AcerosWeCorp.Api.Tests --logger "console;verbosity=normal"
```

Las pruebas levantan la API completa en memoria con una base de datos en memoria, así que no necesitan SQL Server. Cubren el login, las respuestas 401 sin token y 403 por rol, el CRUD de productos, las validaciones 400/404/409, el stock insuficiente (422), el descuento y la devolución de stock en las notas de venta, y el registro del middleware en LogsAuditoria.

Para el informe, se puede exportar un reporte: `dotnet test --logger "trx;LogFileName=resultados.trx"`.

## 5. Ejecutar el frontend

Con la API corriendo:

```powershell
cd frontend
npm install
npm run dev
```

Abrir http://localhost:5173. Vite redirige `/api` a la API, así que no hace falta configurar CORS.

Optimizaciones del frontend:
- Cada pantalla se descarga solo al abrirla (`React.lazy`).
- React y el router van en un archivo aparte que el navegador mantiene en caché.
- Los productos se paginan en el servidor.
- La búsqueda espera 300 ms a que el usuario deje de escribir y cancela la petición anterior con `AbortController`.
- Las familias se piden una sola vez por sesión.
- Los formateadores de moneda y fecha se crean una sola vez.
- Las filas de la tabla usan `memo`.
- Las fuentes son autoalojadas, solo el subconjunto latino.
- No usa librerías de UI.
