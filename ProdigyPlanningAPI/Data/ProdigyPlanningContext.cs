using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProdigyPlanningAPI.Models;
using Microsoft.Extensions.Configuration;

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
            entity.HasKey(e => e.Id).HasName("PK__category__3213E83F550993FD");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasMany(d => d.Events).WithMany(p => p.Categories)
                .UsingEntity<Dictionary<string, object>>(
                    "CategoryEvent",
                    r => r.HasOne<Event>().WithMany()
                        .HasForeignKey("EventsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__category___event__3E52440B"),
                    l => l.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__category___categ__3D5E1FD2"),
                    j =>
                    {
                        j.HasKey("CategoryId", "EventsId").HasName("PK__category__12CF69588000BE2D");
                        j.ToTable("category_events");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                        j.IndexerProperty<int>("EventsId").HasColumnName("events_id");
                    });
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__events__3213E83FA11A6825");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events).HasConstraintName("FK__events__created___3F466844");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F92707C31");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
