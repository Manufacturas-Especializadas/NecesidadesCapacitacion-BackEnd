using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NecesidadesCapacitacion.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<TnPriority> TnPriority { get; set; }

    public virtual DbSet<TrainingNeeds> TrainingNeeds { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07BB4C88DF");

            entity.HasIndex(e => e.Name, "UQ__Roles__72E12F1B77628FD6").IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TnPriority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TnPriori__3214EC07E0C99849");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TrainingNeeds>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC076F1FF228");

            entity.Property(e => e.CurrentPerformance)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("currentPerformance");
            entity.Property(e => e.ExpectedPerformance)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("expectedPerformance");
            entity.Property(e => e.PositionsOrCollaborator)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("positionsOrCollaborator");
            entity.Property(e => e.PresentNeed)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("presentNeed");
            entity.Property(e => e.PriorityId).HasColumnName("priorityId");
            entity.Property(e => e.QualityObjective)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("qualityObjective");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("registrationDate");
            entity.Property(e => e.SuggestedTrainingCourse)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("suggestedTrainingCourse");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Priority).WithMany(p => p.TrainingNeeds)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TrainingN__prior__3E1D39E1");

            entity.HasOne(d => d.User).WithMany(p => p.TrainingNeeds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TrainingN__userI__3F115E1A");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07AC3EC40B");

            entity.HasIndex(e => e.PayRollNumber, "UQ__Users__9EAAFED528B180D0").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("passwordHash");
            entity.Property(e => e.PayRollNumber).HasColumnName("payRollNumber");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(256)
                .HasColumnName("refreshToken");
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("refreshTokenExpiryTime");
            entity.Property(e => e.RolId).HasColumnName("rolId");

            entity.HasOne(d => d.Rol).WithMany(p => p.Users)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK__Users__rolId__3493CFA7");
        });
        modelBuilder.HasSequence("Seq_Folio");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}