﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProdigyPlanningAPI.Data;

#nullable disable

namespace ProdigyPlanningAPI.Migrations
{
    [DbContext(typeof(ProdigyPlanningContext))]
    partial class ProdigyPlanningContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.18")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CategoryEvent", b =>
                {
                    b.Property<int>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("category_id");

                    b.Property<int>("EventId")
                        .HasColumnType("int")
                        .HasColumnName("event_id");

                    b.HasKey("CategoryId", "EventId")
                        .HasName("PK__category__B779E6C6F9A4F5CC");

                    b.HasIndex("EventId");

                    b.ToTable("category_events", (string)null);
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("PK__categori__3213E83FC6E8B697");

                    b.ToTable("categories", (string)null);
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)")
                        .HasColumnName("description");

                    b.Property<int?>("Duration")
                        .HasColumnType("int")
                        .HasColumnName("duration");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Location")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("location");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<TimeOnly?>("Time")
                        .HasColumnType("time")
                        .HasColumnName("time");

                    b.HasKey("Id")
                        .HasName("PK__events__3213E83F0186EDBA");

                    b.HasIndex("CreatedBy");

                    b.ToTable("events", (string)null);
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.EventBanner", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int?>("EventId")
                        .HasColumnType("int")
                        .HasColumnName("event_id");

                    b.Property<byte[]>("EventImage")
                        .HasColumnType("image")
                        .HasColumnName("event_image");

                    b.HasKey("Id");

                    b.ToTable("event_banners", (string)null);
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("email");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("password");

                    b.Property<string>("Roles")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("roles");

                    b.Property<string>("Surname")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("surname");

                    b.HasKey("Id")
                        .HasName("PK__users__3213E83F54C06594");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("CategoryEvent", b =>
                {
                    b.HasOne("ProdigyPlanningAPI.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .IsRequired()
                        .HasConstraintName("FK__category___categ__6EF57B66");

                    b.HasOne("ProdigyPlanningAPI.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("EventId")
                        .IsRequired()
                        .HasConstraintName("FK__category___event__6FE99F9F");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Event", b =>
                {
                    b.HasOne("ProdigyPlanningAPI.Models.User", "CreatedByNavigation")
                        .WithMany("Events")
                        .HasForeignKey("CreatedBy")
                        .HasConstraintName("FK__events__created___70DDC3D8");

                    b.Navigation("CreatedByNavigation");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.EventBanner", b =>
                {
                    b.HasOne("ProdigyPlanningAPI.Models.Event", "EventNavigation")
                        .WithOne("Banner")
                        .HasForeignKey("ProdigyPlanningAPI.Models.EventBanner", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventNavigation");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Event", b =>
                {
                    b.Navigation("Banner");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.User", b =>
                {
                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}
