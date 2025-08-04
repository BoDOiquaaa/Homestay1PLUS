using Homestay1.Models;
using Homestay1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Homestay> Homestays { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình cho bảng Homestay
            builder.Entity<Homestay>(entity =>
            {
                entity.HasKey(h => h.HomestayID);
                entity.Property(h => h.Name).HasMaxLength(100).IsRequired();
                entity.Property(h => h.Address).HasMaxLength(200).IsRequired();
                entity.Property(h => h.ImageUrl).HasMaxLength(255);
                entity.Property(h => h.MapUrl).HasMaxLength(500); // Cấu hình cho MapUrl
                entity.Property(h => h.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Quan hệ với Room - CASCADE DELETE
                entity.HasMany(h => h.Rooms)
                      .WithOne(r => r.Homestay)
                      .HasForeignKey(r => r.HomestayID)
                      .OnDelete(DeleteBehavior.Cascade); // Thêm cascade delete
            });

            // Cấu hình cho bảng Room
            builder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.RoomID);
                entity.Property(r => r.RoomName).HasMaxLength(100).IsRequired();
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(10,2)");
                entity.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Available");
                entity.Property(r => r.ImageUrl).HasMaxLength(255);

                // Cấu hình foreign key constraint
                entity.HasOne(r => r.Homestay)
                      .WithMany(h => h.Rooms)
                      .HasForeignKey(r => r.HomestayID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình cho bảng User
            builder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique(); // Email unique constraint
                entity.Property(u => u.Password).IsRequired();
                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Quan hệ với Role
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleID)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho phép xóa role nếu còn user
            });

            // Cấu hình cho bảng Role
            builder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleID);
                entity.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
                entity.HasIndex(r => r.RoleName).IsUnique(); // Role name unique
            });

            // Seed roles nếu cần
            builder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Owner" },
                new Role { RoleID = 3, RoleName = "Staff" },
                new Role { RoleID = 4, RoleName = "Customer" }
            );
        }
    }
}