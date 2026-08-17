using Pos.Entities;

namespace Pos.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByIdAsync(int id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<IEnumerable<Usuario>> GetActiveUsersAsync();
        Task<Usuario?> GetByUserNameAsync(string userName);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserNameExistsAsync(string userName);
        Task<bool> ValidateCredentialsAsync(string email, string password);
        Task<Usuario?> CreateAsync(Usuario usuario);
        Task<Usuario?> UpdateAsync(Usuario usuario);
        Task<bool> DeleteAsync(int id);
        Task<bool> IncrementAccessFailedCountAsync(int userId);
        Task<bool> ResetAccessFailedCountAsync(int userId);
        Task<bool> UpdatePasswordAsync(int userId, string passwordHash);
    }
}