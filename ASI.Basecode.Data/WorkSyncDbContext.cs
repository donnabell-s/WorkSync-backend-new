using System;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data;

public partial class WorkSyncDbContext : DbContext
{
    public WorkSyncDbContext()
    {
    }

    public WorkSyncDbContext(DbContextOptions<WorkSyncDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingLog> BookingLogs { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAmenity> RoomAmenities { get; set; }

    public virtual DbSet<RoomLog> RoomLogs { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Addr=localhost; database=WorkSync_db; Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AED31FDD369");

            entity.ToTable("Bookings", "ws");

            entity.Property(e => e.BookingId).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.RoomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_Bookings_Rooms");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Bookings_Users");
        });

        modelBuilder.Entity<BookingLog>(entity =>
        {
            entity.HasKey(e => e.BookingLogId).HasName("PK__BookingL__D6D56B32605743C8");

            entity.ToTable("BookingLogs", "ws");

            entity.Property(e => e.BookingLogId).ValueGeneratedNever();
            entity.Property(e => e.CurrentStatus).HasMaxLength(100);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingLogs)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_BookingLogs_Bookings");

            entity.HasOne(d => d.User).WithMany(p => p.BookingLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_BookingLogs_Users");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863939FDF5413B");

            entity.ToTable("Rooms", "ws");

            entity.Property(e => e.RoomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.SizeLabel).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<RoomAmenity>(entity =>
        {
            entity.HasKey(e => new { e.RoomId, e.Amenity });

            entity.ToTable("RoomAmenities", "ws");

            entity.Property(e => e.RoomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Amenity).HasMaxLength(100);

            entity.HasOne(d => d.Room).WithMany(p => p.RoomAmenities)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoomAmenities_Rooms");
        });

        modelBuilder.Entity<RoomLog>(entity =>
        {
            entity.HasKey(e => e.RoomLogId).HasName("PK__RoomLogs__71A264C4826A7346");

            entity.ToTable("RoomLogs", "ws");

            entity.Property(e => e.RoomLogId).ValueGeneratedNever();
            entity.Property(e => e.CurrentStatus).HasMaxLength(100);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.RoomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Room).WithMany(p => p.RoomLogs)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_RoomLogs_Rooms");

            entity.HasOne(d => d.User).WithMany(p => p.RoomLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_RoomLogs_Users");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Sessions__C9F492906CEBC5BB");

            entity.ToTable("Sessions", "ws");

            entity.Property(e => e.SessionId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Sessions_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C933F73B0");

            entity.ToTable("Users", "ws");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053413C382AE").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fname).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Lname).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.PrefId).HasName("PK__UserPref__1F832A20BF4E3852");

            entity.ToTable("UserPreferences", "ws");

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserPreferences)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserPreferences_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
