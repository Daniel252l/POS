using Pos.Entities;
using Pos.Models.DTOs;

namespace Pos.Services
{
    public interface ILoginService
    {
        // Login
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<Usuario?> GetUserByEmailAsync(string email);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<bool> IsUserLockedAsync(string email);
        Task<bool> ResetUserLockoutAsync(string email);
        Task LogoutAsync(int userId);
        Task<bool> UpdatePasswordAsync(Usuario usuario);

        // Recuperación de contraseña
        Task<bool> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ValidateResetTokenAsync(string email, string token);
        Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
    }
}