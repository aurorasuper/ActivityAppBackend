using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ActivityJournal.Models
{
    public partial class ActivityDBContext : DbContext
    {
        public ActivityDBContext()
        {
        }

        public ActivityDBContext(DbContextOptions<ActivityDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivityActivity> ActivityActivities { get; set; } = null!;
        public virtual DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
        public virtual DbSet<ActivityUser> ActivityUsers { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityActivity>(entity =>
            {
                entity.ToTable("Activity_Activities");

                entity.Property(e => e.ActivityType)
                    .HasMaxLength(100)
                    .HasColumnName("Activity_Type");
            });

            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK_Logs");

                entity.ToTable("Activity_Logs");

                entity.Property(e => e.LogId).HasColumnName("Log_Id");

                entity.Property(e => e.ActivityType).HasColumnName("Activity_Type");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Ended).HasColumnType("datetime");

                entity.HasOne(d => d.ActivityTypeNavigation)
                    .WithMany(p => p.ActivityLogs)
                    .HasForeignKey(d => d.ActivityType)
                    .HasConstraintName("FK_Activity");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ActivityLogs)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_User");
            });

            modelBuilder.Entity<ActivityUser>(entity =>
            {
                entity.HasKey(e => e.UsrId)
                    .HasName("PK_Tbl_User");

                entity.ToTable("Activity_Users");

                entity.HasIndex(e => e.UsrEmail, "UE_Tbl_User")
                    .IsUnique();

                entity.Property(e => e.UsrId).HasColumnName("Usr_ID");

                entity.Property(e => e.UsrEmail)
                    .HasMaxLength(150)
                    .HasColumnName("Usr_Email");

                entity.Property(e => e.UsrFirstName)
                    .HasMaxLength(100)
                    .HasColumnName("Usr_FirstName");

                entity.Property(e => e.UsrIsAdmin).HasColumnName("Usr_IsAdmin");

                entity.Property(e => e.UsrLastName)
                    .HasMaxLength(100)
                    .HasColumnName("Usr_LastName");

                entity.Property(e => e.UsrPassword).HasColumnName("Usr_Password");

                entity.Property(e => e.UsrSalt).HasColumnName("Usr_Salt");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
