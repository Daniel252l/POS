using Microsoft.EntityFrameworkCore;
using Pos.Data;
using Pos.Entities;
using Pos.Helpers;
using Pos.Models.DTOs;
using Pos.Repositories.Interfaces;

namespace Pos.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly SeguridadContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginService> _logger;

        public LoginService(
            IUsuarioRepository usuarioRepository,
            SeguridadContext context,
            IConfiguration configuration,
            ILogger<LoginService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Intentando login para: {request.Email}");

                var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de login con email no registrado: {request.Email}");
                    throw new UnauthorizedAccessException("Credenciales inválidas");
                }

                if (!usuario.Estado)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo. Contacte al administrador.");
                }

                if (usuario.AccessFailedCount >= 5)
                {
                    throw new UnauthorizedAccessException("Usuario bloqueado por demasiados intentos fallidos.");
                }

                if (!PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash))
                {
                    await _usuarioRepository.IncrementAccessFailedCountAsync(usuario.Id);
                    _logger.LogWarning($"Intento de login fallido para usuario: {usuario.Email}");
                    throw new UnauthorizedAccessException("Credenciales inválidas");
                }

                await _usuarioRepository.ResetAccessFailedCountAsync(usuario.Id);

                var rol = await _context.Roles.FindAsync(usuario.RoleId);
                if (rol == null)
                {
                    throw new UnauthorizedAccessException("El usuario no tiene un rol válido");
                }

                // Buscar empresa
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.RoleId == usuario.RoleId);

                if (empresa == null)
                {
                    _logger.LogWarning($"No se encontró empresa para el RoleId: {usuario.RoleId}. Usando primera empresa disponible.");
                    empresa = await _context.Empresas.FirstOrDefaultAsync();
                }

                if (empresa == null)
                {
                    _logger.LogWarning("No hay empresas en la base de datos. Creando empresa por defecto...");
                    empresa = new Empresa
                    {
                        RazonSocial = "POS System S.A.",
                        Eslogan = "Soluciones POS",
                        Direccion = "Ciudad, Guatemala",
                        Nit = "123456789",
                        Telefono = "12345678",
                        Email = "info@pos.com",
                        Imagen = "logo.png",
                        Icono = "icon.ico",
                        Estado = true,
                        ImagenReportes = "report.png",
                        RoleId = usuario.RoleId,
                        NombreComercial = "POS System"
                    };
                    await _context.Empresas.AddAsync(empresa);
                    await _context.SaveChangesAsync();
                }

                await RegistrarBitacora(usuario.Id, "Inicio de sesión", $"Usuario {usuario.Email} inició sesión");

                var response = new LoginResponseDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    UserName = usuario.UserName,
                    RoleId = usuario.RoleId,
                    RoleName = rol.Nombre,
                    IsPasswordTemporary = usuario.ContraseniaTemporal, // ← Esto fuerza el cambio de contraseña
                    Token = GenerateSimpleToken(usuario.Email, usuario.Id.ToString()),
                    TokenExpiration = DateTime.UtcNow.AddHours(1),
                    Empresa = new EmpresaDto
                    {
                        Id = empresa.Id,
                        RazonSocial = empresa.RazonSocial,
                        NombreComercial = empresa.NombreComercial ?? empresa.RazonSocial,
                        Nit = empresa.Nit,
                        Email = empresa.Email,
                        Telefono = empresa.Telefono,
                        Direccion = empresa.Direccion,
                        Imagen = empresa.Imagen,
                        Icono = empresa.Icono
                    }
                };

                _logger.LogInformation($"✅ Login exitoso para: {usuario.Email}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en login: {ex.Message}");
                throw;
            }
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailAsync(email);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            return await _usuarioRepository.ValidateCredentialsAsync(email, password);
        }

        public async Task<bool> IsUserLockedAsync(string email)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            if (usuario == null)
                return false;
            return usuario.AccessFailedCount >= 5;
        }

        public async Task<bool> ResetUserLockoutAsync(string email)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            if (usuario == null)
                return false;
            return await _usuarioRepository.ResetAccessFailedCountAsync(usuario.Id);
        }

        public async Task LogoutAsync(int userId)
        {
            await RegistrarBitacora(userId, "Cierre de sesión", "Usuario cerró sesión");
        }

        public async Task<bool> UpdatePasswordAsync(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar contraseña: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GeneratePasswordResetTokenAsync(string email)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario == null)
                    return false;

                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                usuario.ResetToken = token;
                usuario.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Token generado para: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar token: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario == null)
                    return false;

                var isValid = usuario.ResetToken == token &&
                              usuario.ResetTokenExpiration.HasValue &&
                              usuario.ResetTokenExpiration.Value > DateTime.UtcNow;

                _logger.LogInformation($"Validando token para {email}: {(isValid ? "✅ Válido" : "❌ Inválido")}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al validar token: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario == null)
                    return false;

                if (usuario.ResetToken != token ||
                    !usuario.ResetTokenExpiration.HasValue ||
                    usuario.ResetTokenExpiration.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning($"❌ Token inválido o expirado para: {email}");
                    return false;
                }

                if (SoundexHelper.IsSimilarPassword(newPassword, usuario.PasswordHash))
                {
                    _logger.LogWarning($"❌ La nueva contraseña es similar a la anterior para: {email}");
                    return false;
                }

                usuario.PasswordHash = PasswordHelper.HashPassword(newPassword);
                usuario.ContraseniaTemporal = false; // ← Aquí se desactiva la contraseña temporal
                usuario.UltimoCambioDeContrasenia = DateOnly.FromDateTime(DateTime.Now);
                usuario.ResetToken = null;
                usuario.ResetTokenExpiration = null;
                usuario.AccessFailedCount = 0;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Contraseña restablecida para: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al restablecer contraseña: {ex.Message}");
                return false;
            }
        }

        private string GenerateSimpleToken(string email, string userId)
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}|{userId}|{DateTime.UtcNow.Ticks}"));
            return token;
        }

        private async Task RegistrarBitacora(int usuarioId, string evento, string descripcion)
        {
            try
            {
                var bitacora = new Bitacora
                {
                    UsuarioId = usuarioId,
                    Evento = evento,
                    Descripcion = descripcion,
                    Fecha = DateTime.Now,
                    Tabla = "Login"
                };

                await _context.Bitacoras.AddAsync(bitacora);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al registrar bitácora: {ex.Message}");
            }
        }
    }
}