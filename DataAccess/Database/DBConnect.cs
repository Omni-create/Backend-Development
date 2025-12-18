using Microsoft.EntityFrameworkCore;
using System;
using Backend_Dev.Models;

namespace DataAccess.Database
{
    public class DBConnect : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ExtraOption> ExtraOptions { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<PaymentInfo> PaymentInfos { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<ReservedExtraOption> ReservedExtraOptions { get; set; }
        public DbSet<ReservedFacility> ReservedFacilities { get; set; }

        public DBConnect(DbContextOptions<DBConnect> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK_User");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserRole).IsRequired().HasMaxLength(20).HasDefaultValue("Guest");
                entity.Property(e=> e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e=> e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e=> e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.HasMany(e => e.Reservations)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.PaymentInfos)
                        .WithOne(p => p.User)
                        .HasForeignKey(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ExtraOption>(entity =>
            {
                entity.HasKey(e => e.ExtraOptionId).HasName("PK_ExtraOption");
                entity.Property(e => e.OptionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            });
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeId).HasName("PK_RoomType");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Capacity).IsRequired().HasColumnType("int");
            });
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId).HasName("PK_Room");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.RoomType)
                      .WithMany(rt => rt.Rooms)
                      .HasForeignKey(e => e.RoomTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Reservation)
                      .WithMany(r => r.Rooms)
                      .HasForeignKey(e => e.Reservation)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.ReservationId).HasName("PK_Reservation");
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.ReservationStatus).HasMaxLength(50).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
            });
            modelBuilder.Entity<PaymentInfo>(entity =>
            {
                entity.HasKey(e => e.PaymentInfoId).HasName("PK_PaymentInfo");
                entity.Property(e => e.BankHolderName).IsRequired().HasMaxLength(30);
                entity.Property(e => e.LastFourDigits).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PaymentToken).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.PaymentInfos)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            });
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId).HasName("PK_Invoice");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.IssueDate).IsRequired();
                entity.HasOne(e => e.Reservation)
                      .WithMany(r => r.Invoices)
                      .HasForeignKey(e => e.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.FacilityId).HasName("PK_Facility");
                entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            });
            modelBuilder.Entity<ReservedExtraOption>(entity =>
            {
                entity.HasOne(e => e.ExtraOption)
                      .WithMany()
                      .HasForeignKey(e => e.ExtraOptionId);
                entity.HasOne(e => e.Reservation)
                      .WithMany()
                      .HasForeignKey(e => e.ReservationId);
            });
            modelBuilder.Entity<ReservedFacility>(entity =>
            {
                entity.HasOne(e => e.Facility)
                      .WithMany()
                      .HasForeignKey(e => e.FacilityId);
                entity.HasOne(e => e.Reservation)
                      .WithMany()
                      .HasForeignKey(e => e.ReservationId);
            });

        }

    }
}