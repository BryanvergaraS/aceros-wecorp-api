namespace WeCorp.Catalogo.Api.Configuracion;

/// <summary>
/// Lee el archivo .env de la raíz del repositorio (solo en Development) para no tener que definir
/// las variables de entorno a mano en cada terminal. El archivo .env está en .gitignore; el repositorio
/// solo incluye .env.example sin valores. En Docker las variables llegan directamente del entorno.
/// </summary>
public static class ArchivoEnv
{
    public static Dictionary<string, string?> Leer()
    {
        var valores = new Dictionary<string, string?>();
        var ruta = Buscar();
        if (ruta is null) return valores;

        foreach (var linea in File.ReadAllLines(ruta))
        {
            var texto = linea.Trim();
            if (texto.Length == 0 || texto.StartsWith('#')) continue;
            var igual = texto.IndexOf('=');
            if (igual <= 0) continue;

            var clave = texto[..igual].Trim().Replace("__", ":");
            var valor = texto[(igual + 1)..].Trim().Trim('"');
            if (valor.Length > 0) valores[clave] = valor;
        }
        return valores;
    }

    private static string? Buscar()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
        {
            var candidato = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidato)) return candidato;
        }
        return null;
    }
}
