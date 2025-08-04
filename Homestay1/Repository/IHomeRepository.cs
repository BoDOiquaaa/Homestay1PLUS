using Homestay1.Models;

namespace Homestay1.Repository
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Homestay>> GetAllHomestaysAsync();
    }
}
