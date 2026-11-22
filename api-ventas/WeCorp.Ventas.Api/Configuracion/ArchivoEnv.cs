namespace WeCorp.Ventas.Api.Configuracion;

/// <summary>
/// Lee el archivo .env de la raíz del repositorio (solo en Development) para no tener que
/// definir las variables de entorno a mano en cada terminal. El .env nunca se sube al repositorio.
/// </summary>
public static class ArchivoEnv
{
    public static Dictionary<string, string?> Leer()
    {
        var valores = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var ruta = Buscar();
        if (ruta is null) return valores;

        foreach (var linea in File.ReadAllLines(ruta))
        {
            var texto = linea.Trim();
            if (texto.Length == 0 || texto.StartsWith('#')) continue;

            var igual = texto.IndexOf('=');
            if (igual <= 0) continue;

            var clave = texto[..igual].Trim().Replace("__", ":");
            var valor = texto[(igual + 1)..].Trim().Trim('"', '\'');
            if (valor.Length == 0) continue; // las claves vacías del .env.example no pisan nada
            valores[clave] = valor;
        }
        return valores;
    }

    private static string? Buscar()
    {
        var carpeta = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var nivel = 0; nivel < 6 && carpeta is not null; nivel++, carpeta = carpeta.Parent)
        {
            var ruta = Path.Combine(carpeta.FullName, ".env");
            if (File.Exists(ruta)) return ruta;
        }
        return null;
    }
}
