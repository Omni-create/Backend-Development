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
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                
                // Unique constraints with error messages
                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasName("IX_User_Username");
                
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasName("IX_User_Email");
                
                entity.HasMany(e => e.Reservations)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasMany(e => e.PaymentInfos)
                        .WithOne(p => p.User)
                        .HasForeignKey(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                
                // Check constraint: UserRole validation
                entity.HasCheckConstraint("CK_User_Role", 
                    "UserRole IN ('Guest', 'Admin', 'Manager', 'Staff')")
                    .HasName("CK_User_Role_Invalid");
            });
            modelBuilder.Entity<ExtraOption>(entity =>
            {
                entity.HasKey(e => e.ExtraOptionId).HasName("PK_ExtraOption");
                entity.Property(e => e.OptionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                
                // Check constraint: Price must be positive
                entity.HasCheckConstraint("CK_ExtraOption_Price", "Price > 0")
                    .HasName("CK_ExtraOption_Price_Invalid");
                
                // Unique constraint: Option names must be unique
                entity.HasIndex(e => e.OptionName)
                    .IsUnique()
                    .HasName("IX_ExtraOption_Name");
            });
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeId).HasName("PK_RoomType");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Capacity).IsRequired().HasColumnType("int");
                
                // Check constraints with error messages
                entity.HasCheckConstraint("CK_RoomType_Price", "PricePerNight > 0")
                    .HasName("CK_RoomType_Price_Invalid");
                
                entity.HasCheckConstraint("CK_RoomType_Capacity", "Capacity > 0")
                    .HasName("CK_RoomType_Capacity_Invalid");
                
                // Unique constraint: Room type names should be unique
                entity.HasIndex(e => e.Type)
                    .IsUnique()
                    .HasName("IX_RoomType_Name");
            });
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId).HasName("PK_Room");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.RoomType)
                      .WithMany(rt => rt.Rooms)
                      .HasForeignKey(e => e.RoomTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Many-to-many relationship: A room can have many reservations, a reservation can have many rooms
                entity.HasMany(e => e.Reservations)
                      .WithMany(r => r.Rooms)
                      .UsingEntity("ReservationRoom",
                          l => l.HasOne(typeof(Reservation)).WithMany().HasForeignKey("ReservationId").OnDelete(DeleteBehavior.Cascade),
                          r => r.HasOne(typeof(Room)).WithMany().HasForeignKey("RoomId").OnDelete(DeleteBehavior.Cascade));
                
                // Check constraint: Status must be one of the valid values
                entity.HasCheckConstraint("CK_Room_Status", 
                    "Status IN ('Available', 'Occupied', 'Maintenance')")
                    .HasName("CK_Room_Status_Invalid");
            });
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.ReservationId).HasName("PK_Reservation");
                entity.Property(r => r.StartDate).IsRequired();
                entity.Property(r => r.EndDate).IsRequired();
                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Many-to-many relationship with Rooms (defined on Room side too)
                // A reservation can have multiple rooms, a room can have multiple reservations (different dates)
                
                // Check constraints with error messages
                entity.HasCheckConstraint("CK_Reservation_Dates", 
                    "EndDate > StartDate")
                    .HasName("CK_Reservation_Dates_Invalid");
                
                entity.HasCheckConstraint("CK_Reservation_Status", 
                    "Status IN ('Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled')")
                    .HasName("CK_Reservation_Status_Invalid");
                
                // Index for checking date overlaps
                entity.HasIndex(r => new { r.StartDate, r.EndDate })
                      .HasName("IX_Reservation_DateRange");
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
                
                // Unique constraint: Payment token should be unique (security)
                entity.HasIndex(e => e.PaymentToken)
                    .IsUnique()
                    .HasName("IX_PaymentInfo_Token");
            });
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId).HasName("PK_Invoice");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.IssueDate).IsRequired();
                entity.Property(e => e.PaymentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                
                entity.HasOne(e => e.Reservation)
                      .WithMany(r => r.Invoices)
                      .HasForeignKey(e => e.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.PaymentInfo)
                      .WithMany(p => p.Invoices)
                      .HasForeignKey(e => e.PaymentInfoId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Check constraints with error messages
                entity.HasCheckConstraint("CK_Invoice_TotalCost", "TotalCost > 0")
                    .HasName("CK_Invoice_TotalCost_Invalid");
                
                entity.HasCheckConstraint("CK_Invoice_PaymentStatus", 
                    "PaymentStatus IN ('Pending', 'Confirmed', 'Paid', 'Cancelled')")
                    .HasName("CK_Invoice_PaymentStatus_Invalid");
            });
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.FacilityId).HasName("PK_Facility");
                entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                
                // Check constraint: Price must be positive
                entity.HasCheckConstraint("CK_Facility_Price", "Price > 0")
                    .HasName("CK_Facility_Price_Invalid");
                
                // Unique constraint: Facility names must be unique
                entity.HasIndex(e => e.FacilityName)
                    .IsUnique()
                    .HasName("IX_Facility_Name");
            });
            modelBuilder.Entity<ReservedExtraOption>(entity =>
            {
                entity.HasKey(e => new { e.ReservationId, e.ExtraOptionId });
                entity.HasOne(e => e.ExtraOption)
                    .WithMany(eo => eo.ReservedExtraOptions)
                    .HasForeignKey(e => e.ExtraOptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reservation)
                    .WithMany(r => r.ReservedExtraOptions)
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReservedFacility>(entity =>
            {
                entity.HasKey(e => new { e.ReservationId, e.FacilityId });
                entity.HasOne(e => e.Facility)
                      .WithMany(f => f.ReservedFacilities)
                      .HasForeignKey(e => e.FacilityId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reservation)
                      .WithMany(r => r.ReservedFacilities)
                      .HasForeignKey(e => e.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }

    }
}