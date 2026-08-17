using Microsoft.EntityFrameworkCore;
using Pos.Data;
using Pos.Entities;
using Pos.Repositories.Interfaces;
using Pos.Helpers;

namespace Pos.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SeguridadContext _context;
        private readonly ILogger<UsuarioRepository> _logger;

        public UsuarioRepository(SeguridadContext context, ILogger<UsuarioRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario por email: {ex.Message}");
                return null;
            }
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener todos los usuarios: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public async Task<IEnumerable<Usuario>> GetActiveUsersAsync()
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => u.Estado)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios activos: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public async Task<Usuario?> GetByUserNameAsync(string userName)
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.UserName == userName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario por nombre: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Usuarios.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UserNameExistsAsync(string userName)
        {
            try
            {
                return await _context.Usuarios.AnyAsync(u => u.UserName == userName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar nombre de usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                var usuario = await GetByEmailAsync(email);
                if (usuario == null || !usuario.Estado)
                    return false;

                return PasswordHelper.VerifyPassword(password, usuario.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al validar credenciales: {ex.Message}");
                return false;
            }
        }

        public async Task<Usuario?> CreateAsync(Usuario usuario)
        {
            try
            {
                await _context.Usuarios.AddAsync(usuario);
                await _context.SaveChangesAsync();
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<Usuario?> UpdateAsync(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var usuario = await GetByIdAsync(id);
                if (usuario == null)
                    return false;

                usuario.Estado = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IncrementAccessFailedCountAsync(int userId)
        {
            try
            {
                var usuario = await GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                usuario.AccessFailedCount++;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al incrementar intentos fallidos: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetAccessFailedCountAsync(int userId)
        {
            try
            {
                var usuario = await GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                usuario.AccessFailedCount = 0;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al resetear intentos fallidos: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash)
        {
            try
            {
                var usuario = await GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                usuario.PasswordHash = passwordHash;
                usuario.UltimoCambioDeContrasenia = DateOnly.FromDateTime(DateTime.Now);
                usuario.ContraseniaTemporal = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar contraseña: {ex.Message}");
                return false;
            }
        }
    }
}