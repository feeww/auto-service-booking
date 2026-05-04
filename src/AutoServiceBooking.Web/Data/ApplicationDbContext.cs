using AutoServiceBooking.Web.Models;
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

        public DbSet<AutoService> AutoServices { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Booking> Bookings { get; set; }

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

            modelBuilder.Entity<AutoService>(entity =>
            {
                entity.ToTable("AutoServices");

                entity.HasKey(autoService => autoService.Id);

                entity.Property(autoService => autoService.Name).HasMaxLength(100).IsRequired();
                entity.Property(autoService => autoService.Description).HasMaxLength(500).IsRequired();
                entity.Property(autoService => autoService.Price).HasColumnType("numeric(10,2)").IsRequired();
                entity.Property(autoService => autoService.DurationMinutes).IsRequired();
                entity.Property(autoService => autoService.IsActive).IsRequired();
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles");

                entity.HasKey(vehicle => vehicle.Id);

                entity.Property(vehicle => vehicle.Make).HasMaxLength(60).IsRequired();
                entity.Property(vehicle => vehicle.Model).HasMaxLength(60).IsRequired();
                entity.Property(vehicle => vehicle.LicensePlate).HasMaxLength(20).IsRequired();
                entity.Property(vehicle => vehicle.Year).IsRequired();
                entity.Property(vehicle => vehicle.Mileage).IsRequired();
                entity.Property(vehicle => vehicle.FuelType).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.Property(vehicle => vehicle.IsArchived).IsRequired().HasDefaultValue(false);

                entity.HasOne(vehicle => vehicle.ClientUser)
                    .WithMany(client => client.Vehicles)
                    .HasForeignKey(vehicle => vehicle.ClientUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");

                entity.HasKey(booking => booking.Id);

                entity.Property(booking => booking.ProblemDescription).HasMaxLength(1000);
                entity.Property(booking => booking.CustomerName).HasMaxLength(100).IsRequired();
                entity.Property(booking => booking.CustomerPhone).HasMaxLength(30).IsRequired();
                entity.Property(booking => booking.CustomerEmail).HasMaxLength(100);
                entity.Property(booking => booking.GuestVehicleMake).HasMaxLength(60);
                entity.Property(booking => booking.GuestVehicleModel).HasMaxLength(60);
                entity.Property(booking => booking.GuestVehicleLicensePlate).HasMaxLength(20);
                entity.Property(booking => booking.GuestVehicleFuelType).HasConversion<string>().HasMaxLength(20);
                entity.Property(booking => booking.FinalPrice).HasColumnType("numeric(10,2)");
                entity.Property(booking => booking.EstimatedPrice).HasColumnType("numeric(10,2)");
                entity.Property(booking => booking.EstimatedDurationMinutes);
                entity.Property(booking => booking.AdminComment).HasMaxLength(1000);
                entity.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
                entity.Property(booking => booking.ScheduledAt).IsRequired();

                entity.HasOne(booking => booking.ClientUser)
                    .WithMany(client => client.Bookings)
                    .HasForeignKey(booking => booking.ClientUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(booking => booking.Vehicle)
                    .WithMany(vehicle => vehicle.Bookings)
                    .HasForeignKey(booking => booking.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(booking => booking.AutoService)
                    .WithMany(autoService => autoService.Bookings)
                    .HasForeignKey(booking => booking.AutoServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
