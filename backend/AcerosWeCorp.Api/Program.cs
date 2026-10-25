using System.Reflection;
using System.Security.Claims;
using System.Text;
using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Middleware;
using AcerosWeCorp.Api.Repositories;
using AcerosWeCorp.Api.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración (secretos por variables de entorno o user-secrets) ----------
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Seccion))
    .Validate(o => o.Key.Length >= 32, "Falta Jwt:Key (variable Jwt__Key) o tiene menos de 32 caracteres.")
    .ValidateOnStart();

// ---------- Base de datos (EF Core + SQL Server) ----------
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    // Sin cadena igual se puede crear una migración (no se conecta); al arrancar la API se exige.
    var cadena = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(cadena)) options.UseSqlServer();
    else options.UseSqlServer(cadena);
});

// ---------- Inyección de dependencias ----------
// Scoped: una instancia por petición, igual que el DbContext que usan.
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IFamiliaRepository, FamiliaRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<INotaVentaRepository, NotaVentaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ILogAuditoriaRepository, LogAuditoriaRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotaVentaService, NotaVentaService>();
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

// ---------- Autenticación JWT y autorización por roles ----------
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
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Aceros We-Corp",
        Version = "v1",
        Description = "API RESTful del catálogo de productos de Aceros We-Corp (barraca de fierros, Nacimiento). " +
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
        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection")))
            throw new InvalidOperationException(
                "Falta la cadena de conexión ConnectionStrings:DefaultConnection (user-secrets o variable ConnectionStrings__DefaultConnection).");
        await db.Database.MigrateAsync();
    }
    else
        await db.Database.EnsureCreatedAsync(); // pruebas con base en memoria
    await DbInitializer.CrearUsuariosInicialesAsync(db, app.Configuration);
}

// ---------- Pipeline ----------
app.UseMiddleware<RequestLoggingMiddleware>();     // 1. mide y registra cada petición en LogsAuditoria
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 2. convierte excepciones en ProblemDetails
app.UseStatusCodePages();                          // 3. 401/403/404 sin cuerpo también salen como ProblemDetails

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Aceros We-Corp v1");
    c.DocumentTitle = "API Aceros We-Corp";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Necesario para las pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
