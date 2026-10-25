using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Data;

/// <summary>
/// Crea el administrador inicial (y un vendedor de prueba, si se configura) al arrancar.
/// Las credenciales se leen de la configuración (variables de entorno o user-secrets),
/// nunca del código ni del repositorio.
/// </summary>
public static class DbInitializer
{
    public static async Task CrearUsuariosInicialesAsync(AppDbContext db, IConfiguration config)
    {
        await CrearSiNoExisteAsync(db, config["Seed:AdminEmail"], config["Seed:AdminPassword"], 1, "Administrador");
        await CrearSiNoExisteAsync(db, config["Seed:VendedorEmail"], config["Seed:VendedorPassword"], 2, "Vendedor");
    }

    private static async Task CrearSiNoExisteAsync(AppDbContext db, string? email, string? password, int rolId, string nombre)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var normalizado = email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == normalizado)) return;

        db.Usuarios.Add(new Usuario
        {
            RolId = rolId,
            Nombre = nombre,
            Apellido = "We-Corp",
            Email = normalizado,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });
        await db.SaveChangesAsync();
    }
}
