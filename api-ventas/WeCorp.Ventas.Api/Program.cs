using System.Reflection;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeCorp.Ventas.Api.Configuracion;
using WeCorp.Ventas.Api.Data;
using WeCorp.Ventas.Api.Middleware;
using WeCorp.Ventas.Api.Repositories;
using WeCorp.Ventas.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración: secretos solo por variables de entorno (.env en desarrollo) ----------
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddInMemoryCollection(
        ArchivoEnv.Leer().Where(v => builder.Configuration[v.Key] is null)); // no pisa variables ya definidas

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Seccion))
    .Validate(o => o.Key.Length >= 32, "Falta Jwt:Key (variable Jwt__Key) o tiene menos de 32 caracteres.")
    .ValidateOnStart();

builder.Services.AddOptions<CatalogoOptions>()
    .Bind(builder.Configuration.GetSection(CatalogoOptions.Seccion))
    .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
        "Falta Catalogo:BaseUrl (variable Catalogo__BaseUrl) con la URL de la API de Catálogo.")
    .Validate(o => o.TimeoutSegundos > 0 && o.Reintentos >= 0, "Catalogo:TimeoutSegundos o Catalogo:Reintentos no son válidos.")
    .ValidateOnStart();

// ---------- Base de datos (EF Core + SQL Server) ----------
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    // Sin cadena igual se puede crear una migración (no se conecta); al arrancar la API se exige.
    var cadena = sp.GetRequiredService<IConfiguration>().GetConnectionString("VentasDb");
    if (string.IsNullOrWhiteSpace(cadena)) options.UseSqlServer();
    else options.UseSqlServer(cadena);
});

// ---------- Inyección de dependencias ----------
// Scoped: una instancia por petición, igual que el DbContext que usan.
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<INotaVentaRepository, NotaVentaRepository>();
builder.Services.AddScoped<INotaVentaService, NotaVentaService>();

// ---------- Integración con la API de Catálogo (cliente HTTP tipado) ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ReenviarTokenHandler>();
builder.Services.AddHttpClient<ICatalogoClient, CatalogoClient>((sp, http) =>
    {
        var o = sp.GetRequiredService<IOptions<CatalogoOptions>>().Value;
        http.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
        http.Timeout = TimeSpan.FromSeconds(o.TimeoutSegundos); // tiempo de espera por intento
    })
    .AddHttpMessageHandler<ReenviarTokenHandler>();

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

// ---------- Autenticación: valida el JWT emitido por la API de Catálogo ----------
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

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Ventas — Aceros We-Corp",
        Version = "v1",
        Description = "Clientes y notas de venta. Use el token entregado por la API de Catálogo " +
                      "(POST /api/v1/auth/login) en el botón Authorize."
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

// ---------- Health check (RNF07) ----------
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// ---------- Base de datos: migraciones ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("VentasDb")))
            throw new InvalidOperationException(
                "Falta la cadena de conexión ConnectionStrings:VentasDb (variable ConnectionStrings__VentasDb).");
        await db.Database.MigrateAsync();
    }
    else
        await db.Database.EnsureCreatedAsync(); // pruebas con base en memoria
}

// ---------- Pipeline ----------
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1. convierte excepciones en ProblemDetails
app.UseStatusCodePages();                          // 2. 401/403/404 sin cuerpo también salen como ProblemDetails

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Ventas v1");
    c.DocumentTitle = "API de Ventas — Aceros We-Corp";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

/// <summary>Necesario para las pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
