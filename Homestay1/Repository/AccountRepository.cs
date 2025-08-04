using Homestay1.Data;
using Homestay1.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _db;
        public AccountRepository(ApplicationDbContext db) => _db = db;

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            // TODO: Hash password in production
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }
    }
}