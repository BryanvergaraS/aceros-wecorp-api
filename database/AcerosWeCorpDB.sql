/* =====================================================================
   Aceros We-Corp - Base de datos para la API RESTful
   Trabajo 2 - Construcción de Software
   Autor: Bryan Vergara Santander
   Motor: SQL Server
   ===================================================================== */

IF DB_ID('AcerosWeCorpDB') IS NULL
    CREATE DATABASE AcerosWeCorpDB;
GO

USE AcerosWeCorpDB;
GO

/* ---------------------------------------------------------------------
   1. Roles
   --------------------------------------------------------------------- */
CREATE TABLE Roles (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nombre       NVARCHAR(30)      NOT NULL,
    Descripcion  NVARCHAR(200)     NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Nombre UNIQUE (Nombre)
);
GO

/* ---------------------------------------------------------------------
   2. Usuarios
   --------------------------------------------------------------------- */
CREATE TABLE Usuarios (
    Id             INT IDENTITY(1,1) NOT NULL,
    RolId          INT               NOT NULL,
    Nombre         NVARCHAR(60)      NOT NULL,
    Apellido       NVARCHAR(60)      NOT NULL,
    Email          NVARCHAR(150)     NOT NULL,
    PasswordHash   NVARCHAR(255)     NOT NULL,
    Telefono       NVARCHAR(20)      NULL,
    Estado         BIT               NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT (1),
    FechaRegistro  DATETIME2         NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Usuarios PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles (Id)
);
GO

/* ---------------------------------------------------------------------
   3. Familias (agrupación del catálogo: Abierto, Cerrado, Viga...)
   --------------------------------------------------------------------- */
CREATE TABLE Familias (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nombre       NVARCHAR(50)      NOT NULL,
    Descripcion  NVARCHAR(200)     NULL,
    Estado       BIT               NOT NULL CONSTRAINT DF_Familias_Estado DEFAULT (1),
    CONSTRAINT PK_Familias PRIMARY KEY (Id),
    CONSTRAINT UQ_Familias_Nombre UNIQUE (Nombre)
);
GO

/* ---------------------------------------------------------------------
   4. Productos (recurso principal del CRUD)
   --------------------------------------------------------------------- */
CREATE TABLE Productos (
    Id             INT IDENTITY(1,1) NOT NULL,
    FamiliaId      INT               NOT NULL,
    Sku            NVARCHAR(50)      NOT NULL,
    Nombre         NVARCHAR(150)     NOT NULL,
    Descripcion    NVARCHAR(300)     NULL,
    UnidadVenta    NVARCHAR(10)      NOT NULL,
    PesoKg         DECIMAL(10,3)     NULL,
    PrecioVenta    DECIMAL(12,0)     NOT NULL,
    Stock          INT               NOT NULL CONSTRAINT DF_Productos_Stock DEFAULT (0),
    StockMinimo    INT               NOT NULL CONSTRAINT DF_Productos_StockMinimo DEFAULT (0),
    Estado         BIT               NOT NULL CONSTRAINT DF_Productos_Estado DEFAULT (1),
    FechaCreacion  DATETIME2         NOT NULL CONSTRAINT DF_Productos_FechaCreacion DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Productos PRIMARY KEY (Id),
    CONSTRAINT UQ_Productos_Sku UNIQUE (Sku),
    CONSTRAINT FK_Productos_Familias FOREIGN KEY (FamiliaId) REFERENCES Familias (Id),
    CONSTRAINT CK_Productos_UnidadVenta CHECK (UnidadVenta IN ('Tira', 'Unidad')),
    CONSTRAINT CK_Productos_PrecioVenta CHECK (PrecioVenta > 0),
    CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0),
    CONSTRAINT CK_Productos_StockMinimo CHECK (StockMinimo >= 0)
);
GO

/* ---------------------------------------------------------------------
   5. Proveedores
   --------------------------------------------------------------------- */
CREATE TABLE Proveedores (
    Id           INT IDENTITY(1,1) NOT NULL,
    Rut          NVARCHAR(12)      NOT NULL,
    RazonSocial  NVARCHAR(150)     NOT NULL,
    Telefono     NVARCHAR(20)      NULL,
    Email        NVARCHAR(150)     NULL,
    Direccion    NVARCHAR(200)     NULL,
    Estado       BIT               NOT NULL CONSTRAINT DF_Proveedores_Estado DEFAULT (1),
    CONSTRAINT PK_Proveedores PRIMARY KEY (Id),
    CONSTRAINT UQ_Proveedores_Rut UNIQUE (Rut)
);
GO

/* ---------------------------------------------------------------------
   6. ProductoProveedores (relación N a N: un producto puede comprarse
      a varios proveedores y un proveedor vende varios productos)
   --------------------------------------------------------------------- */
CREATE TABLE ProductoProveedores (
    ProductoId   INT            NOT NULL,
    ProveedorId  INT            NOT NULL,
    CostoCompra  DECIMAL(12,2)  NULL,
    CONSTRAINT PK_ProductoProveedores PRIMARY KEY (ProductoId, ProveedorId),
    CONSTRAINT FK_ProductoProveedores_Productos FOREIGN KEY (ProductoId) REFERENCES Productos (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductoProveedores_Proveedores FOREIGN KEY (ProveedorId) REFERENCES Proveedores (Id) ON DELETE CASCADE,
    CONSTRAINT CK_ProductoProveedores_Costo CHECK (CostoCompra IS NULL OR CostoCompra >= 0)
);
GO

/* ---------------------------------------------------------------------
   7. NotasVenta (la emite el vendedor en el mesón)
   --------------------------------------------------------------------- */
CREATE TABLE NotasVenta (
    Id             INT IDENTITY(1,1) NOT NULL,
    UsuarioId      INT               NOT NULL,
    Folio          INT               NOT NULL,
    ClienteNombre  NVARCHAR(150)     NOT NULL,
    ClienteRut     NVARCHAR(12)      NULL,
    Total          DECIMAL(12,0)     NOT NULL CONSTRAINT DF_NotasVenta_Total DEFAULT (0),
    Estado         NVARCHAR(15)      NOT NULL CONSTRAINT DF_NotasVenta_Estado DEFAULT ('Pendiente'),
    Observaciones  NVARCHAR(300)     NULL,
    FechaCreacion  DATETIME2         NOT NULL CONSTRAINT DF_NotasVenta_FechaCreacion DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_NotasVenta PRIMARY KEY (Id),
    CONSTRAINT UQ_NotasVenta_Folio UNIQUE (Folio),
    CONSTRAINT FK_NotasVenta_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id),
    CONSTRAINT CK_NotasVenta_Estado CHECK (Estado IN ('Pendiente', 'Pagada', 'Anulada')),
    CONSTRAINT CK_NotasVenta_Total CHECK (Total >= 0)
);
GO

/* ---------------------------------------------------------------------
   8. DetalleNotasVenta (productos de cada nota de venta)
   --------------------------------------------------------------------- */
CREATE TABLE DetalleNotasVenta (
    Id              INT IDENTITY(1,1) NOT NULL,
    NotaVentaId     INT               NOT NULL,
    ProductoId      INT               NOT NULL,
    Cantidad        INT               NOT NULL,
    PrecioUnitario  DECIMAL(12,0)     NOT NULL,
    Subtotal        AS (Cantidad * PrecioUnitario) PERSISTED,
    CONSTRAINT PK_DetalleNotasVenta PRIMARY KEY (Id),
    CONSTRAINT FK_DetalleNotasVenta_NotasVenta FOREIGN KEY (NotaVentaId) REFERENCES NotasVenta (Id) ON DELETE CASCADE,
    CONSTRAINT FK_DetalleNotasVenta_Productos FOREIGN KEY (ProductoId) REFERENCES Productos (Id),
    CONSTRAINT CK_DetalleNotasVenta_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT CK_DetalleNotasVenta_Precio CHECK (PrecioUnitario > 0)
);
GO

/* ---------------------------------------------------------------------
   9. LogsAuditoria (lo escribe el middleware de logging de la API)
   --------------------------------------------------------------------- */
CREATE TABLE LogsAuditoria (
    Id                 BIGINT IDENTITY(1,1) NOT NULL,
    UsuarioId          INT                  NULL,
    MetodoHttp         NVARCHAR(10)         NOT NULL,
    RutaEndpoint       NVARCHAR(300)        NOT NULL,
    CodigoEstadoHttp   INT                  NOT NULL,
    IpOrigen           NVARCHAR(45)         NULL,
    TiempoRespuestaMs  INT                  NOT NULL,
    FechaRegistro      DATETIME2            NOT NULL CONSTRAINT DF_LogsAuditoria_FechaRegistro DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_LogsAuditoria PRIMARY KEY (Id),
    CONSTRAINT FK_LogsAuditoria_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
);
GO

/* ---------------------------------------------------------------------
   Índices para las claves foráneas
   --------------------------------------------------------------------- */
CREATE INDEX IX_Usuarios_RolId                  ON Usuarios (RolId);
CREATE INDEX IX_Productos_FamiliaId             ON Productos (FamiliaId);
CREATE INDEX IX_ProductoProveedores_ProveedorId ON ProductoProveedores (ProveedorId);
CREATE INDEX IX_NotasVenta_UsuarioId            ON NotasVenta (UsuarioId);
CREATE INDEX IX_DetalleNotasVenta_NotaVentaId   ON DetalleNotasVenta (NotaVentaId);
CREATE INDEX IX_DetalleNotasVenta_ProductoId    ON DetalleNotasVenta (ProductoId);
CREATE INDEX IX_LogsAuditoria_UsuarioId         ON LogsAuditoria (UsuarioId);
GO

/* ---------------------------------------------------------------------
   Datos iniciales (catálogo de ejemplo; precios referenciales)
   Los usuarios no se insertan aquí: la API crea el administrador
   inicial con la contraseña leída desde una variable de entorno.
   --------------------------------------------------------------------- */
INSERT INTO Roles (Nombre, Descripcion) VALUES
(N'Administrador', N'Acceso total: gestiona catálogo, precios, proveedores y usuarios'),
(N'Vendedor',      N'Consulta el catálogo y emite notas de venta');

INSERT INTO Familias (Nombre, Descripcion) VALUES
(N'Abierto',  N'Perfiles abiertos: ángulos, canales, tees'),
(N'Cerrado',  N'Perfiles cerrados: cuadrados y rectangulares'),
(N'Laminado', N'Perfiles laminados en caliente'),
(N'Viga',     N'Vigas IPE, HEA y similares'),
(N'Plancha',  N'Planchas lisas y diamantadas'),
(N'Malla',    N'Mallas electrosoldadas'),
(N'Barras',   N'Barras de construcción y lisas');

INSERT INTO Productos (FamiliaId, Sku, Nombre, UnidadVenta, PrecioVenta, Stock, StockMinimo) VALUES
(1, N'ANG-20-20-2', N'Ángulo 20x20x2',         N'Tira',   7990,  40, 10),
(1, N'ANG-30-30-3', N'Ángulo 30x30x3',         N'Tira',  14990,  35, 10),
(1, N'ANG-50-50-4', N'Ángulo 50x50x4',         N'Tira',  32990,  20,  5),
(2, N'CUA-40-40-2', N'Perfil cuadrado 40x40x2', N'Tira',  21990,  25,  8),
(5, N'PLA-LIS-2MM', N'Plancha lisa 2 mm',       N'Unidad', 59990, 12,  3),
(6, N'MAL-C92',     N'Malla C-92',              N'Unidad', 42990, 15,  5),
(7, N'BAR-12-6M',   N'Barra estriada 12 mm x 6 m', N'Tira', 9990, 80, 20);

INSERT INTO Proveedores (Rut, RazonSocial, Telefono, Email) VALUES
(N'76.111.111-1', N'Aceros del Sur SpA (ejemplo)',   N'+56 9 1111 1111', N'ventas@proveedor1.example'),
(N'76.222.222-2', N'Perfiles Biobío Ltda. (ejemplo)', N'+56 9 2222 2222', N'contacto@proveedor2.example');

INSERT INTO ProductoProveedores (ProductoId, ProveedorId, CostoCompra) VALUES
(1, 1,  5200), (2, 1,  9800), (3, 2, 21500),
(4, 2, 14300), (5, 1, 39000), (6, 2, 28000), (7, 1, 6500);
GO
