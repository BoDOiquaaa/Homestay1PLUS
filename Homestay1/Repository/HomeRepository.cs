using Homestay1.Data;
using Homestay1.Models;
using Homestay1.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _context;
        public HomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Homestay>> GetAllHomestaysAsync()
        {
            return await _context.Homestays
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}