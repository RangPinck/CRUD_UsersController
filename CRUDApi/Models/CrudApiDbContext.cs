using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CRUDApi.Models;

public partial class CrudApiDbContext : DbContext
{
    public CrudApiDbContext()
    {
    }

    public CrudApiDbContext(DbContextOptions<CrudApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity
            .HasKey(e => e.Guid)
            .HasName("pk_user_guid");

            entity
            .ToTable("user");

            entity
            .Property(e => e.Guid)
            .HasColumnName("guid");

            entity
            .Property(e => e.Login)
            .HasColumnName("login");

            entity
            .Property(e => e.Password)
            .HasColumnName("password");

            entity
            .Property(e => e.Name)
            .HasColumnName("name");

            entity
            .Property(e => e.Gender)
            .HasColumnName("gender");

            entity
            .Property(e => e.Birthday)
            .HasColumnName("birthday")
            .HasColumnType("timestamp with time zone");

            entity
            .Property(e => e.Admin)
            .HasColumnName("admin")
            .HasDefaultValue(false);

            entity
            .Property(e => e.CreatedOn)
            .HasColumnName("created_on")
            .HasColumnType("timestamp with time zone");

            entity
            .Property(e => e.CreatedBy)
            .HasColumnName("created_by");

            entity
            .Property(e => e.ModifiedOn)
            .HasColumnName("modified_on")
            .HasColumnType("timestamp with time zone");

            entity.Property(e => e.ModifiedBy)
            .HasColumnName("modified_by");

            entity
            .Property(e => e.RevokedOn)
            .HasColumnName("revoked_on")
            .HasColumnType("timestamp with time zone");

            entity.Property(e => e.RevokedBy)
            .HasColumnName("revoked_by");
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
