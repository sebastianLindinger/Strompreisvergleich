﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StromDbLib;

#nullable disable

namespace StromDbLib.Migrations
{
    [DbContext(typeof(StromDbContext))]
    partial class StromDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.25");

            modelBuilder.Entity("StromDbLib.Strompreis", b =>
                {
                    b.Property<int>("StrompreisId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Bis")
                        .HasColumnType("TEXT");

                    b.Property<double>("Preis")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Von")
                        .HasColumnType("TEXT");

                    b.HasKey("StrompreisId");

                    b.ToTable("Strompreis");
                });

            modelBuilder.Entity("StromDbLib.Stromverbrauch", b =>
                {
                    b.Property<int>("StromverbrauchId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsWaermepumpe")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Verbrauch")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("Zeitpunkt")
                        .HasColumnType("TEXT");

                    b.HasKey("StromverbrauchId");

                    b.ToTable("Stromverbrauch");
                });
#pragma warning restore 612, 618
        }
    }
}
