using Homestay1.Data;
using Homestay1.Models;
using Homestay1.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public class HomestayRepository : IHomestayRepository
    {
        private readonly ApplicationDbContext _context;
        public HomestayRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Homestay>> GetAllAsync(string search = null)
        {
            var query = _context.Homestays.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.Name.Contains(search) || h.Address.Contains(search));
            }
            return await query.OrderByDescending(h => h.CreatedAt).ToListAsync();
        }

        public async Task<Homestay> GetByIdAsync(int id)
        {
            return await _context.Homestays.FindAsync(id);
        }

        public async Task<Homestay> GetByIdWithRoomsAsync(int id)
        {
            return await _context.Homestays
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HomestayID == id);
        }

        public async Task AddAsync(Homestay homestay)
        {
            homestay.CreatedAt = DateTime.Now;
            await _context.Homestays.AddAsync(homestay);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Homestay homestay)
        {
            _context.Homestays.Update(homestay);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                // Lấy homestay cùng với các room liên quan
                var homestay = await _context.Homestays
                    .Include(h => h.Rooms)
                    .FirstOrDefaultAsync(h => h.HomestayID == id);

                if (homestay != null)
                {
                    // Log thông tin trước khi xóa
                    System.Diagnostics.Debug.WriteLine($"Deleting homestay: {homestay.Name}");
                    System.Diagnostics.Debug.WriteLine($"Number of rooms to delete: {homestay.Rooms?.Count ?? 0}");

                    // Xóa homestay (rooms sẽ được xóa tự động bởi cascade delete)
                    _context.Homestays.Remove(homestay);
                    var result = await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Delete operation affected {result} records");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Homestay with ID {id} not found");
                }
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error when deleting homestay: {ex.Message}");
                throw new Exception("Không thể xóa homestay. Có thể đang có dữ liệu liên quan khác (booking, etc.).", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General error when deleting homestay: {ex.Message}");
                throw new Exception("Lỗi không xác định khi xóa homestay: " + ex.Message, ex);
            }
        }
    }
}