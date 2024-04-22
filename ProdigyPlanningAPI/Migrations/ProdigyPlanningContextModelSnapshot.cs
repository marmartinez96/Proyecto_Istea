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

                    b.Property<int>("EventsId")
                        .HasColumnType("int")
                        .HasColumnName("events_id");

                    b.HasKey("CategoryId", "EventsId")
                        .HasName("PK__category__12CF69588000BE2D");

                    b.HasIndex("EventsId");

                    b.ToTable("category_events", (string)null);
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("PK__category__3213E83F550993FD");

                    b.ToTable("category");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int?>("Category")
                        .HasColumnType("int")
                        .HasColumnName("category");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("description");

                    b.Property<string>("Location")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("location");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("PK__events__3213E83FA11A6825");

                    b.HasIndex("CreatedBy");

                    b.ToTable("events");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("email");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("password");

                    b.Property<string>("Roles")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("roles");

                    b.Property<string>("Surname")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("surname");

                    b.HasKey("Id")
                        .HasName("PK__user__3213E83F92707C31");

                    b.ToTable("user");
                });

            modelBuilder.Entity("CategoryEvent", b =>
                {
                    b.HasOne("ProdigyPlanningAPI.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .IsRequired()
                        .HasConstraintName("FK__category___categ__3D5E1FD2");

                    b.HasOne("ProdigyPlanningAPI.Models.Event", null)
                        .WithMany()
                        .HasForeignKey("EventsId")
                        .IsRequired()
                        .HasConstraintName("FK__category___event__3E52440B");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.Event", b =>
                {
                    b.HasOne("ProdigyPlanningAPI.Models.User", "CreatedByNavigation")
                        .WithMany("Events")
                        .HasForeignKey("CreatedBy")
                        .HasConstraintName("FK__events__created___3F466844");

                    b.Navigation("CreatedByNavigation");
                });

            modelBuilder.Entity("ProdigyPlanningAPI.Models.User", b =>
                {
                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}
