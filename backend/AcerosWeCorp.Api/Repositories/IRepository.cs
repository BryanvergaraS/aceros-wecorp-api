namespace AcerosWeCorp.Api.Repositories;

/// <summary>Operaciones comunes de acceso a datos. Aísla a los controladores de EF Core.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(params object[] claves);
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
    Task GuardarCambiosAsync();
}
