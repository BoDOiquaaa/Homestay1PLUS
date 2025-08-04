// Repositories/IUserRepository.cs
using Homestay1.Models.Entities;

namespace Homestay1.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync(string search = null);
        Task<User> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}