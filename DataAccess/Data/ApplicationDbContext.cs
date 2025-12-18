using System;
using System.Collections.Generic;
using Backend_Dev.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Dev.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExtraOption> ExtraOptions { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<PaymentInfo> PaymentInfos { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<ReservedExtraOption> ReservedExtraOptions { get; set; }

    public virtual DbSet<ReservedFacility> ReservedFacilities { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExtraOption>(entity =>
        {
            entity.HasKey(e => e.ExtraOptionId).HasName("PK__ExtraOpt__673F1A5ABB13DC48");

            entity.ToTable("ExtraOption");

            entity.Property(e => e.ExtraOptionId).HasColumnName("extraOptionID");
            entity.Property(e => e.OptionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("optionName");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Faciliti__AA54818400BFFCE6");

            entity.Property(e => e.FacilityId).HasColumnName("facilityID");
            entity.Property(e => e.FacilityName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("facilityName");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__1252410C1F10E7C1");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId).HasColumnName("invoiceID");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IssueDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issueDate");
            entity.Property(e => e.PaymentInfoId).HasColumnName("paymentInfoID");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("paymentStatus");
            entity.Property(e => e.ReservationId).HasColumnName("reservationID");
            entity.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalCost");

            entity.HasOne(d => d.PaymentInfo).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PaymentInfoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_paymentInfoID_Invoice");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reservationID_Invoice");
        });

        modelBuilder.Entity<PaymentInfo>(entity =>
        {
            entity.HasKey(e => e.PaymentInfoId).HasName("PK__PaymentI__837E4C0A01D269C5");

            entity.ToTable("PaymentInfo");

            entity.Property(e => e.PaymentInfoId).HasColumnName("paymentInfoID");
            entity.Property(e => e.BankHolderName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("bankHolderName");
            entity.Property(e => e.LastFourDigits)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("lastFourDigits");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("paymentMethod");
            entity.Property(e => e.PaymentToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("paymentToken");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentInfos)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_userID_PaymentInfo");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B14BF5A5091D04AB");

            entity.ToTable("Reservation");

            entity.Property(e => e.ReservationId).HasColumnName("reservationID");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.ReservationStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("reservationStatus");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userID_Reservation");
        });

        modelBuilder.Entity<ReservedExtraOption>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.ExtraOptionId).HasColumnName("extraOptionID");
            entity.Property(e => e.ReservationId).HasColumnName("reservationID");

            entity.HasOne(d => d.ExtraOption).WithMany()
                .HasForeignKey(d => d.ExtraOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_extraOptionID_ReservedExtraOptions");

            entity.HasOne(d => d.Reservation).WithMany()
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK_reservationID_ReservedExtraOptions");
        });

        modelBuilder.Entity<ReservedFacility>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.FacilityId).HasColumnName("facilityID");
            entity.Property(e => e.ReservationId).HasColumnName("reservationID");

            entity.HasOne(d => d.Facility).WithMany()
                .HasForeignKey(d => d.FacilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_facilityID_ReservedFacilities");

            entity.HasOne(d => d.Reservation).WithMany()
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK_reservationID_ReservedFacilities");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__6C3BF5DE958164C0");

            entity.ToTable("Room");

            entity.Property(e => e.RoomId).HasColumnName("roomID");
            entity.Property(e => e.ReservationId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("reservationID");
            entity.Property(e => e.RoomTypeId).HasColumnName("roomTypeID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("AVAILABLE")
                .HasColumnName("status");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_reservationID_Room");

            entity.HasOne(d => d.RoomType).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomTypeId)
                .HasConstraintName("FK_roomTypeID_Room");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId).HasName("PK__RoomType__5E5E0CD39A8F9B8C");

            entity.ToTable("RoomType");

            entity.Property(e => e.RoomTypeId).HasColumnName("roomTypeID");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.PricePerNight)
                .HasColumnType("money")
                .HasColumnName("pricePerNight");
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDF46C5709A");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164F203B81B").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC5728F0A33E3").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UserRole)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("GUEST")
                .HasColumnName("userRole");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
