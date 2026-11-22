using System.ComponentModel.DataAnnotations;

namespace WeCorp.Ventas.Api.Validacion;

/// <summary>Normaliza y valida RUT chilenos con el dígito verificador (módulo 11).</summary>
public static class RutChileno
{
    /// <summary>Devuelve el RUT como "12345678-5" (sin puntos, DV en mayúscula) o null si el formato no sirve.</summary>
    public static string? Normalizar(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut)) return null;

        var limpio = rut.Replace(".", "").Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();
        if (limpio.Length < 2) return null;

        var cuerpo = limpio[..^1];
        var dv = limpio[^1];
        if (cuerpo.Length > 8 || !cuerpo.All(char.IsAsciiDigit)) return null;
        if (!char.IsAsciiDigit(dv) && dv != 'K') return null;

        return $"{int.Parse(cuerpo)}-{dv}";
    }

    public static bool EsValido(string? rut)
    {
        var normalizado = Normalizar(rut);
        if (normalizado is null) return false;

        var partes = normalizado.Split('-');
        return CalcularDv(partes[0]) == partes[1][0];
    }

    public static char CalcularDv(string cuerpo)
    {
        var suma = 0;
        var factor = 2;
        for (var i = cuerpo.Length - 1; i >= 0; i--)
        {
            suma += (cuerpo[i] - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        var resto = 11 - suma % 11;
        return resto switch
        {
            11 => '0',
            10 => 'K',
            _ => (char)('0' + resto)
        };
    }
}

/// <summary>Valida que el texto sea un RUT con dígito verificador correcto.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RutAttribute : ValidationAttribute
{
    public RutAttribute() : base("El RUT no es válido (revise el dígito verificador).") { }

    // null o vacío lo resuelve [Required].
    public override bool IsValid(object? value) =>
        value is null || value is string { Length: 0 } || (value is string texto && RutChileno.EsValido(texto));
}
