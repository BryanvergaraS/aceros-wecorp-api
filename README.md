# Sistema de ventas Aceros We-Corp

Proyecto integrador de **Construcción de Software — Evaluación 3** (Instituto Virginio Gómez).
Equipo: **Bryan Vergara Santander** y **Aníbal Rodríguez**. Docente: Enso Guidotti.

Aceros We-Corp es una barraca de fierros de Nacimiento (Biobío). El sistema centraliza el catálogo de productos
y la emisión de notas de venta, con control de acceso por rol y descuento automático de stock.

| Componente | Carpeta | Tecnología | Puerto local |
|---|---|---|---|
| API de Catálogo e Identidad | `api-catalogo/` | ASP.NET Core 10 (controladores), EF Core, SQL Server | 5081 |
| API de Ventas | `api-ventas/` | ASP.NET Core 10 (controladores), EF Core, SQL Server, IHttpClientFactory | 5082 |
| Cliente web | `cliente-web/` | React 19 + Vite | 5173 (dev) / 8080 (Docker) |
| Documentación | `docs/` | PlantUML, OpenAPI 3, Markdown | — |

- **Catálogo e Identidad** emite el JWT (login) y administra familias, productos, stock y usuarios.
- **Ventas** valida ese JWT y administra clientes y notas de venta. Para emitir o anular una nota llama a Catálogo
  con un cliente HTTP tipado (timeout de 5 s y 2 reintentos; si Catálogo no responde, devuelve 503).
- Roles: **Administrador** y **Vendedor**.

> **Evolución del Trabajo 2.** Este repositorio contenía la API única del Trabajo 2 (catálogo y notas de venta en una sola
> API). Esa versión se conserva en la etiqueta [`v1.0-trabajo2`](../../tree/v1.0-trabajo2). El Trabajo 3 la divide en
> dos APIs independientes (ver `docs/adr/0001-dos-apis-con-base-de-datos-propia.md`).

## Estructura del repositorio

```
.
├── api-catalogo/
│   ├── WeCorp.Catalogo.Api/          API (controladores, servicios, repositorios, EF Core)
│   ├── WeCorp.Catalogo.Api.Tests/    Pruebas de integración (xUnit + WebApplicationFactory)
│   └── Dockerfile
├── api-ventas/
│   ├── WeCorp.Ventas.Api/
│   ├── WeCorp.Ventas.Api.Tests/
│   └── Dockerfile
├── cliente-web/                      React + Vite (Dockerfile y nginx.conf)
├── docs/
│   ├── openapi/                      Contratos OpenAPI (diseño primero) y su registro de cambios
│   ├── diagramas/                    Fuentes PlantUML (.puml) y exportaciones (.png / .svg)
│   ├── adr/                          Registros de decisiones de arquitectura
│   ├── trazabilidad.md               Requerimiento → historia de usuario → endpoint → prueba
│   ├── pruebas/                      Colección Postman y reportes de ejecución de las pruebas
│   ├── gestion/                      Retrospectivas y capturas del tablero
│   ├── hito1/                        Caso, ERS v1, backlog y casos de uso (Hito 1)
│   └── hito2/                        Documento de diseño (Hito 2)
├── docker-compose.yml
├── .env.example                      Variables de entorno necesarias (sin valores)
└── WeCorp.sln
```

## Requisitos

- .NET SDK 10
- Node.js 22 y npm
- SQL Server (Express sirve) **o** Docker Desktop
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

## Configuración (variables de entorno)

Ningún secreto está en el repositorio. Copie la plantilla y complete los valores:

```powershell
Copy-Item .env.example .env
```

| Variable | Usada por | Descripción |
|---|---|---|
| `Jwt__Key` | ambas APIs | Llave del JWT, mínimo 32 caracteres. **La misma** en las dos APIs. |
| `ConnectionStrings__CatalogoDb` | Catálogo | Cadena de conexión a `WeCorpCatalogoDB`. |
| `ConnectionStrings__VentasDb` | Ventas | Cadena de conexión a `WeCorpVentasDB`. |
| `Catalogo__BaseUrl` | Ventas | URL de la API de Catálogo (local: `http://localhost:5081`). |
| `Seed__AdminEmail`, `Seed__AdminPassword` | Catálogo | Administrador inicial que se crea al arrancar. |
| `Seed__VendedorEmail`, `Seed__VendedorPassword` | Catálogo | Vendedor de prueba que se crea al arrancar. |
| `SA_PASSWORD` | docker compose | Contraseña del usuario `sa` del contenedor de SQL Server. |

En desarrollo (`dotnet run`), cada API lee el archivo `.env` de la raíz automáticamente. En Docker, las variables
se pasan a los contenedores desde `docker-compose.yml`. El archivo `.env` está en `.gitignore`.

## Opción A — Ejecutar todo con Docker (un solo comando)

Requiere que las carpetas `Migrations` de cada API estén en el repositorio (se generan una vez, ver Opción B, paso 1).

```powershell
docker compose up --build
```

| Servicio | URL |
|---|---|
| Cliente web | http://localhost:8080 |
| Swagger Catálogo | http://localhost:5081/swagger |
| Swagger Ventas | http://localhost:5082/swagger |
| Health checks | http://localhost:5081/health · http://localhost:5082/health |

Al arrancar, cada API aplica sus migraciones y crea su base de datos en el contenedor de SQL Server.

## Opción B — Ejecutar en local (sin Docker)

1. **Migraciones** (solo la primera vez, si la carpeta `Migrations` no existe en cada API):
   ```powershell
   cd api-catalogo\WeCorp.Catalogo.Api
   dotnet ef migrations add InicialCreate
   cd ..\..\api-ventas\WeCorp.Ventas.Api
   dotnet ef migrations add InicialCreate
   cd ..\..
   ```
   Las bases de datos se crean al arrancar cada API (`Database.Migrate()`).

2. **API de Catálogo** (terminal 1):
   ```powershell
   cd api-catalogo\WeCorp.Catalogo.Api
   dotnet run
   ```

3. **API de Ventas** (terminal 2):
   ```powershell
   cd api-ventas\WeCorp.Ventas.Api
   dotnet run
   ```

4. **Cliente web** (terminal 3):
   ```powershell
   cd cliente-web
   npm install
   npm run dev
   ```
   Abra http://localhost:5173. Vite reenvía `/catalogo` a la API de Catálogo y `/ventas` a la de Ventas.

## Pruebas

```powershell
dotnet test WeCorp.sln
```

Las pruebas de integración levantan cada API completa con `WebApplicationFactory` y una base en memoria.
En la API de Ventas, la API de Catálogo se reemplaza por un doble de prueba y, además, se prueba `CatalogoClient`
con respuestas simuladas (422, errores de red, reintentos). Para guardar el reporte de ejecución:

```powershell
dotnet test WeCorp.sln --logger "trx;LogFileName=resultados.trx" --results-directory docs/pruebas
```

**Colección de pruebas de las APIs (Postman):** `docs/pruebas/AcerosWeCorp.postman_collection.json` y el entorno
`docs/pruebas/AcerosWeCorp.postman_environment.json`. Importe ambos, complete las credenciales en el entorno
(no se suben al repositorio) y ejecute la colección en orden con el Collection Runner, con las dos APIs corriendo.
Recorre login, 401/403/400/409, CRUD, emisión de una nota con descuento de stock en Catálogo, 422, anulación con
reposición de stock y los health checks. Si la ejecuta más de una vez, cambie `rutCliente` en el entorno.

## Documentación

- Contratos: `docs/openapi/catalogo-v1.yaml`, `docs/openapi/ventas-v1.yaml` (y Swagger en cada API).
- Diagramas: `docs/diagramas/` — casos de uso, clases de dominio, ER de cada base, arquitectura, 3 secuencias, despliegue y estados.
- Decisiones: `docs/adr/`.
- Trazabilidad: `docs/trazabilidad.md`.

Para regenerar las imágenes de los diagramas: extensión PlantUML de VS Code (Alt + D) o
`java -jar plantuml.jar -tpng -tsvg docs/diagramas/*.puml`.

## Flujo de trabajo con Git

- `main` protegida: solo se integra mediante **pull request revisado por el otro integrante**.
- Una rama por historia o tarea: `feature/HU09-emitir-nota`, `docs/diagramas-hito2`, etc.
- Tablero en GitHub Projects con las columnas Backlog, Sprint actual, En progreso, En revisión y Terminado.
