using WeCorp.Catalogo.Api.Dtos;
using WeCorp.Catalogo.Api.Repositories;

namespace WeCorp.Catalogo.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

/// <summary>Valida las credenciales (hash BCrypt) y entrega el JWT. Usuario inactivo = credenciales inválidas.</summary>
public class AuthService(IUsuarioRepository usuarios, ITokenService tokens) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await usuarios.ObtenerPorEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (usuario is null || !usuario.Activo || usuario.Rol is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash)) return null;

        var (token, expira) = tokens.Generar(usuario, usuario.Rol.Nombre);
        var dto = new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email,
            usuario.Telefono, usuario.Rol.Nombre, usuario.Activo, usuario.FechaRegistro);
        return new LoginResponse(token, expira, dto);
    }
}
