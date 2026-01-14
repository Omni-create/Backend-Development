using HotelApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ExtraOption> ExtraOptions { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<PaymentInfo> PaymentInfos { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Facility> Facilities { get; set; } = null!;
        public DbSet<ReservedExtraOption> ReservedExtraOptions { get; set; } = null!;
        public DbSet<ReservedFacility> ReservedFacilities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys for join tables
            modelBuilder.Entity<ReservedExtraOption>()
                .HasKey(e => new { e.ExtraOptionId, e.ReservationId });

            modelBuilder.Entity<ReservedFacility>()
                .HasKey(e => new { e.FacilityId, e.ReservationId });

            // User - Reservation (1 - many)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reservation - Rooms (1 - many)
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Reservation) //MOET MANY TO MANY WORDEN
                .WithMany(res => res.Rooms)
                .HasForeignKey(r => r.ReservationId)
                .IsRequired(false); // Room may or may not be assigned to Reservation

            // Room - RoomType (many - 1)
            modelBuilder.Entity<Room>()
                .HasOne(r => r.RoomType)
                .WithMany(rt => rt.Rooms)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentInfo - User (many - 1)
            modelBuilder.Entity<PaymentInfo>()
                .HasOne(p => p.User)
                .WithMany(u => u.PaymentInfos) // 1 paymentinfo kan op meerdere invoices staan
                .HasForeignKey(p => p.UserId) 
                .OnDelete(DeleteBehavior.Cascade);

            // Invoice - Reservation (many - 1)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Reservation)
                .WithMany()
                .HasForeignKey(i => i.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice - PaymentInfo (many - 1, optional)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.PaymentInfo)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PaymentInfoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Optional: configure table names explicitly if needed
            modelBuilder.Entity<PaymentInfo>().ToTable("PaymentInfo");
            modelBuilder.Entity<Facility>().ToTable("Facilities");
            modelBuilder.Entity<Reservation>().ToTable("Reservation");
            modelBuilder.Entity<Room>().ToTable("Room");
            modelBuilder.Entity<ExtraOption>().ToTable("ExtraOption");
        }
    }
}
