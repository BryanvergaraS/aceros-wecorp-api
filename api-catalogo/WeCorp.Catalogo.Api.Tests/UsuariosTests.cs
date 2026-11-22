using System.Net;
using System.Net.Http.Json;
using WeCorp.Catalogo.Api.Dtos;

namespace WeCorp.Catalogo.Api.Tests;

/// <summary>Gestión de usuarios y familias por el Administrador (RF04, RF05).</summary>
public class UsuariosTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task CrearUsuario_ConEmailRepetido_Devuelve409()
    {
        var admin = await factory.ClienteAdminAsync();

        var respuesta = await admin.PostAsJsonAsync("/api/v1/usuarios", new
        {
            rolId = 2, nombre = "Otro", apellido = "Vendedor", email = ApiFactory.VendedorEmail, password = "ClaveSegura123"
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task UsuarioDesactivado_NoPuedeIniciarSesion()
    {
        var admin = await factory.ClienteAdminAsync();
        var crear = await admin.PostAsJsonAsync("/api/v1/usuarios", new
        {
            rolId = 2, nombre = "Temporal", apellido = "Prueba", email = "temporal@test.cl", password = "ClaveSegura123"
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var usuario = await crear.Content.ReadFromJsonAsync<UsuarioDto>();

        var desactivar = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario!.Id}", new
        {
            rolId = 2, nombre = "Temporal", apellido = "Prueba", activo = false
        });
        Assert.Equal(HttpStatusCode.NoContent, desactivar.StatusCode);

        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login",
            new { email = "temporal@test.cl", password = "ClaveSegura123" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task EliminarFamilia_ConProductos_Devuelve409()
    {
        var admin = await factory.ClienteAdminAsync();

        var respuesta = await admin.DeleteAsync("/api/v1/familias/1");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearFamilia_ConNombreRepetido_Devuelve409()
    {
        var admin = await factory.ClienteAdminAsync();

        var respuesta = await admin.PostAsJsonAsync("/api/v1/familias", new { nombre = "Abierto" });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }
}
