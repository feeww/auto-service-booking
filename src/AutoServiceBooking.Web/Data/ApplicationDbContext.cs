using AutoServiceBooking.Web.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }

        public DbSet<ClientUser> Clients { get; set; }

        public DbSet<AdminUser> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(user => user.Id);

                entity.HasIndex(user => user.Email).IsUnique();

                entity.Property(user => user.FullName).HasMaxLength(100).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(100).IsRequired();
                entity.Property(user => user.PasswordHash).IsRequired();
                entity.Property(user => user.PhoneNumber).HasMaxLength(30);

                entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.HasDiscriminator(user => user.Role)
                    .HasValue<ClientUser>(UserRole.Client)
                    .HasValue<AdminUser>(UserRole.Admin);
            });
        }
    }
}
