using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExperimentWebApp.Entities;

public partial class JwtExperimentNugetDbContext : DbContext
{
    public JwtExperimentNugetDbContext()
    {
    }

    public JwtExperimentNugetDbContext(DbContextOptions<JwtExperimentNugetDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=tcp:nuran-cse-ju.database.windows.net,1433;Initial Catalog=JwtNugetDB;Persist Security Info=False;User ID=nuran.cse.ju;Password=NuR@n....;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserInfo");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
