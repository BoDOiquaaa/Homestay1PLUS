using Homestay1.Data;
using Homestay1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homestay1.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var room = await _context.Rooms
                    // Nếu có bảng Booking hoặc RoomImages thì include luôn
                    //.Include(r => r.Bookings)
                    //.Include(r => r.RoomImages)
                    .FirstOrDefaultAsync(r => r.RoomID == id);

                if (room != null)
                {
                    // Xóa các bản ghi liên quan nếu cần
                    // _context.Bookings.RemoveRange(room.Bookings);
                    // _context.RoomImages.RemoveRange(room.RoomImages);

                    _context.Rooms.Remove(room);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Không thể xóa phòng vì đang có dữ liệu liên quan (ví dụ: đơn đặt phòng).");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi không xác định khi xóa phòng: " + ex.Message);
            }
        }



        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _context.Rooms.Include(r => r.Homestay).ToListAsync();
        }

        public async Task<Room> GetByIdAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task AddAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }



        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Rooms.AnyAsync(r => r.RoomID == id);
        }


    }

}