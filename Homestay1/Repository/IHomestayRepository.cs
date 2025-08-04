using Homestay1.Models;

namespace Homestay1.Repository
{
    public interface IHomestayRepository
    {
        Task<IEnumerable<Homestay>> GetAllAsync(string search = null);
        Task<Homestay> GetByIdAsync(int id);
        Task<Homestay> GetByIdWithRoomsAsync(int id);
        Task AddAsync(Homestay homestay);
        Task UpdateAsync(Homestay homestay);
        Task DeleteAsync(int id);
    }
}
