using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProdigyPlanningAPI.Models;

namespace ProdigyPlanningAPI.Data;

public partial class ProdigyPlanningContext : DbContext
{
    public ProdigyPlanningContext()
    {
    }

    public ProdigyPlanningContext(DbContextOptions<ProdigyPlanningContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<EventBanner> EventBanners { get; set; }
    public virtual DbSet<SecurityQuestion> SecurityQuestions { get; set; }
    public virtual DbSet<UserQuestion> UserQuestions { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("conn_string_events");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83FC6E8B697");

            entity.HasMany(d => d.Events).WithMany(p => p.Categories)
                .UsingEntity<Dictionary<string, object>>(
                    "CategoryEvent",
                    r => r.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__category___event__6FE99F9F"),
                    l => l.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__category___categ__6EF57B66"),
                    j =>
                    {
                        j.HasKey("CategoryId", "EventId").HasName("PK__category__B779E6C6F9A4F5CC");
                        j.ToTable("category_events");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                        j.IndexerProperty<int>("EventId").HasColumnName("event_id");
                    });
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__events__3213E83F0186EDBA");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events).HasConstraintName("FK__events__created___70DDC3D8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F54C06594");
        });

        modelBuilder.Entity<Event>()
        .Property(e => e.IsActive)// here is the computed query definition
        .HasComputedColumnSql("CASE WHEN date >= GETDATE() THEN 1 ELSE 0 END", false);

        modelBuilder
        .Entity<Event>()
        .Property(e => e.IsActive)
        .HasConversion<int>();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
