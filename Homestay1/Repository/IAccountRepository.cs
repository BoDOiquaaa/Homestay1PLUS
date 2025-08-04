using Homestay1.Models.Entities;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public interface IAccountRepository
    {
        Task<User> AuthenticateAsync(string email, string password);
    }
}