using Microsoft.EntityFrameworkCore;
using WeCorp.Ventas.Api.Models;

namespace WeCorp.Ventas.Api.Data;

/// <summary>Clientes de ejemplo (ficticios) que se insertan con la migración.</summary>
public static class SeedData
{
    private static readonly DateTime FechaSemilla = new(2026, 11, 9);

    public static void Aplicar(ModelBuilder mb)
    {
        mb.Entity<Cliente>().HasData(
            new
            {
                Id = 1,
                Rut = "11111111-1",
                Nombre = "Constructora Los Robles Ltda. (ejemplo)",
                Email = "compras@cliente1.example",
                Telefono = "+56 9 3333 3333",
                Direccion = "Nacimiento, Biobío",
                FechaRegistro = FechaSemilla
            },
            new
            {
                Id = 2,
                Rut = "22222222-2",
                Nombre = "Juan Pérez (ejemplo)",
                Telefono = "+56 9 4444 4444",
                Direccion = "Los Ángeles, Biobío",
                FechaRegistro = FechaSemilla
            });
    }
}
