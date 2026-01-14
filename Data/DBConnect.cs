using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using HotelApi.Models;

namespace HotelApi.Data
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

            // Configure DateOnly conversions (for EF Core 6+)
            modelBuilder.Entity<Reservation>()
                .Property(r => r.StartDate)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.EndDate)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            modelBuilder.Entity<Invoice>()
                .Property(i => i.IssueDate)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK_User");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserRole)
                    .HasConversion<string>()  // Convert enum to string
                    .HasMaxLength(20)
                    .HasDefaultValue(UserRole.Guest);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Unique constraints
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Relationships
                entity.HasMany(e => e.Reservations)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.PaymentInfos)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ExtraOption>(entity =>
            {
                entity.HasKey(e => e.ExtraOptionId).HasName("PK_ExtraOption");
                entity.Property(e => e.OptionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();  // Changed to (10,2)

                // UPDATED: Check constraint with new syntax
                entity.ToTable(tb => tb.HasCheckConstraint("CK_ExtraOption_Price", "Price > 0"));
                entity.HasIndex(e => e.OptionName).IsUnique();
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeId).HasName("PK_RoomType");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(10,2)").IsRequired();  // Changed to (10,2)
                entity.Property(e => e.Capacity).IsRequired();

                // UPDATED: Check constraints with new syntax
                entity.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_RoomType_Price", "PricePerNight > 0");
                    tb.HasCheckConstraint("CK_RoomType_Capacity", "Capacity > 0");
                });
                entity.HasIndex(e => e.Type).IsUnique();
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId).HasName("PK_Room");
                entity.Property(e => e.Status)
                    .HasConversion<string>()  // Convert enum to string
                    .HasMaxLength(50)
                    .HasDefaultValue(Status.Available);

                entity.HasOne(e => e.RoomType)
                      .WithMany(rt => rt.Rooms)
                      .HasForeignKey(e => e.RoomTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.ReservationId).HasName("PK_Reservation");
                entity.Property(r => r.Status)
                    .HasConversion<string>()  // Convert enum to string
                    .HasMaxLength(20)
                    .HasDefaultValue(ReservationStatus.Pending);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Many-to-many with Rooms
                entity.HasMany(r => r.Rooms)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          "ReservationRoom",
                          j => j.HasOne<Room>().WithMany().HasForeignKey("RoomId").OnDelete(DeleteBehavior.Cascade),
                          j => j.HasOne<Reservation>().WithMany().HasForeignKey("ReservationId").OnDelete(DeleteBehavior.Cascade),
                          j => j.HasKey("ReservationId", "RoomId"));

                // UPDATED: Check constraint with new syntax
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Reservation_Dates", "EndDate > StartDate"));
                entity.HasIndex(r => new { r.StartDate, r.EndDate });
            });

            modelBuilder.Entity<PaymentInfo>(entity =>
            {
                entity.HasKey(e => e.PaymentInfoId).HasName("PK_PaymentInfo");
                entity.Property(e => e.BankHolderName).HasMaxLength(30);  // REMOVED IsRequired()
                entity.Property(e => e.LastFourDigits).HasMaxLength(10);  // REMOVED IsRequired()
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PaymentToken).HasMaxLength(200);  // REMOVED IsRequired()

                entity.HasOne(e => e.User)
                      .WithMany(u => u.PaymentInfos)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PaymentToken).IsUnique();
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId).HasName("PK_Invoice");
                entity.Property(e => e.Description).HasMaxLength(500);  // ADDED
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PaymentStatus)
                    .HasConversion<string>()  // Convert enum to string
                    .HasMaxLength(20)
                    .HasDefaultValue(PaymentStatus.Pending);

                entity.HasOne(e => e.Reservation)
                      .WithMany(r => r.Invoices)
                      .HasForeignKey(e => e.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PaymentInfo)
                      .WithMany(p => p.Invoices)
                      .HasForeignKey(e => e.PaymentInfoId)
                      .OnDelete(DeleteBehavior.SetNull);

                // UPDATED: Check constraint with new syntax
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Invoice_TotalCost", "TotalCost > 0"));
            });

            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.FacilityId).HasName("PK_Facility");
                entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();  // Changed to (10,2)

                // UPDATED: Check constraint with new syntax
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Facility_Price", "Price > 0"));
                entity.HasIndex(e => e.FacilityName).IsUnique();
            });

            modelBuilder.Entity<ReservedExtraOption>(entity =>
            {
                // Composite primary key (ReservationId, ExtraOptionId)
                entity.HasKey(reo => new { reo.ReservationId, reo.ExtraOptionId })
                    .HasName("PK_ReservedExtraOption");

                // Relationships
                entity.HasOne(reo => reo.Reservation)
                    .WithMany() // Assuming Reservation doesn't have navigation property back
                    .HasForeignKey(reo => reo.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(reo => reo.ExtraOption)
                    .WithMany() // Assuming ExtraOption doesn't have navigation property back
                    .HasForeignKey(reo => reo.ExtraOptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReservedFacility>(entity =>
            {
                // Composite primary key (ReservationId, FacilityId)
                entity.HasKey(rf => new { rf.ReservationId, rf.FacilityId })
                      .HasName("PK_ReservedFacility");

                // Relationships
                entity.HasOne(rf => rf.Reservation)
                      .WithMany() // Assuming Reservation doesn't have navigation property back
                      .HasForeignKey(rf => rf.ReservationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rf => rf.Facility)
                      .WithMany() // Assuming Facility doesn't have navigation property back
                      .HasForeignKey(rf => rf.FacilityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    // ADD these converter classes for DateOnly support:
    public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter() : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
        { }
    }

    public class DateOnlyComparer : ValueComparer<DateOnly>
    {
        public DateOnlyComparer() : base(
            (d1, d2) => d1.Equals(d2),
            d => d.GetHashCode())
        { }
    }
}