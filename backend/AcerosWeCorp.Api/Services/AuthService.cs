using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Repositories;

namespace AcerosWeCorp.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthService(IUsuarioRepository usuarios, ITokenService tokens) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await usuarios.ObtenerPorEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (usuario is null || !usuario.Estado || usuario.Rol is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash)) return null;

        var (token, expira) = tokens.Generar(usuario, usuario.Rol.Nombre);
        var dto = new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email,
            usuario.Telefono, usuario.Rol.Nombre, usuario.Estado, usuario.FechaRegistro);
        return new LoginResponse(token, expira, dto);
    }
}
