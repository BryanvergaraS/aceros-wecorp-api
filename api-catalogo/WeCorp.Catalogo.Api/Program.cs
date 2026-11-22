using System.Reflection;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeCorp.Catalogo.Api.Configuracion;
using WeCorp.Catalogo.Api.Data;
using WeCorp.Catalogo.Api.Middleware;
using WeCorp.Catalogo.Api.Repositories;
using WeCorp.Catalogo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración: secretos solo por variables de entorno ----------
// En desarrollo se leen desde el archivo .env de la raíz (ignorado por Git).
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddInMemoryCollection(
        ArchivoEnv.Leer().Where(v => builder.Configuration[v.Key] is null)); // no pisa variables ya definidas

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Seccion))
    .Validate(o => o.Key.Length >= 32, "Falta la variable de entorno Jwt__Key o tiene menos de 32 caracteres.")
    .ValidateOnStart();

// ---------- Base de datos (EF Core + SQL Server) ----------
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    // Sin cadena igual se puede crear una migración (no se conecta); al arrancar la API se exige.
    var cadena = sp.GetRequiredService<IConfiguration>().GetConnectionString("CatalogoDb");
    if (string.IsNullOrWhiteSpace(cadena)) options.UseSqlServer();
    else options.UseSqlServer(cadena);
});

// ---------- Inyección de dependencias ----------
// Scoped: una instancia por petición, igual que el DbContext que usan.
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IFamiliaRepository, FamiliaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStockService, StockService>();
// Singleton: no guarda estado y solo lee la configuración.
builder.Services.AddSingleton<ITokenService, TokenService>();

// ---------- Controladores, errores y versionamiento ----------
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

// ---------- Autenticación JWT (esta API lo emite) y autorización por roles ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((o, jwt) =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// ---------- Health check (RNF07) ----------
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Catálogo e Identidad — Aceros We-Corp",
        Version = "v1",
        Description = "Emite el JWT del sistema y administra familias, productos, stock y usuarios. " +
                      "Inicie sesión en /api/v1/auth/login y use el botón Authorize con el token."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue solo el token JWT (sin la palabra Bearer)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();

// ---------- Base de datos: migraciones y usuarios iniciales ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("CatalogoDb")))
            throw new InvalidOperationException(
                "Falta la variable de entorno ConnectionStrings__CatalogoDb (ver .env.example).");
        await db.Database.MigrateAsync();
    }
    else
        await db.Database.EnsureCreatedAsync(); // pruebas con base en memoria
    await DbInitializer.CrearUsuariosInicialesAsync(db, app.Configuration);
}

// ---------- Pipeline ----------
app.UseMiddleware<ExceptionHandlingMiddleware>();  // convierte excepciones en ProblemDetails
app.UseStatusCodePages();                          // 401/403/404 sin cuerpo también salen como ProblemDetails

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Catálogo v1");
    c.DocumentTitle = "API de Catálogo — Aceros We-Corp";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

/// <summary>Necesario para las pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
