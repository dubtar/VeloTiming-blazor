﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VeloTiming.Server.Data;

namespace VeloTiming.Server.Migrations
{
    [DbContext(typeof(RacesDbContext))]
    [Migration("20201126112813_RemoveNumberFK")]
    partial class RemoveNumberFK
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("VeloTiming.Server.Data.Number", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("NumberRfids")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Numbers");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Race", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Races");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.RaceCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MaxYearOfBirth")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MinYearOfBirth")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("RaceId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Sex")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RaceId");

                    b.ToTable("RaceCategories");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Result", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsIgnored")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Lap")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Number")
                        .HasColumnType("TEXT");

                    b.Property<string>("NumberSource")
                        .HasColumnType("TEXT");

                    b.Property<int>("Place")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StartId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Time")
                        .HasColumnType("TEXT");

                    b.Property<string>("TimeSource")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("StartId");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Rider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Number")
                        .HasColumnType("TEXT");

                    b.Property<int>("RaceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Sex")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Team")
                        .HasColumnType("TEXT");

                    b.Property<int>("YearOfBirth")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("RaceId");

                    b.ToTable("Riders");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Start", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DelayMarksAfterStartMinutes")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("End")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PlannedStart")
                        .HasColumnType("TEXT");

                    b.Property<int>("RaceId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("RealStart")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RaceId");

                    b.ToTable("Starts");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.StartCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StartId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("StartId");

                    b.ToTable("StartCategory");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.RaceCategory", b =>
                {
                    b.HasOne("VeloTiming.Server.Data.Race", "Race")
                        .WithMany("Categories")
                        .HasForeignKey("RaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Race");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Result", b =>
                {
                    b.HasOne("VeloTiming.Server.Data.Start", "Start")
                        .WithMany()
                        .HasForeignKey("StartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Start");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Rider", b =>
                {
                    b.HasOne("VeloTiming.Server.Data.RaceCategory", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("VeloTiming.Server.Data.Race", "Race")
                        .WithMany("Riders")
                        .HasForeignKey("RaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Race");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Start", b =>
                {
                    b.HasOne("VeloTiming.Server.Data.Race", "Race")
                        .WithMany("Starts")
                        .HasForeignKey("RaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Race");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.StartCategory", b =>
                {
                    b.HasOne("VeloTiming.Server.Data.RaceCategory", "Category")
                        .WithMany("Starts")
                        .HasForeignKey("CategoryId");

                    b.HasOne("VeloTiming.Server.Data.Start", "Start")
                        .WithMany("Categories")
                        .HasForeignKey("StartId");

                    b.Navigation("Category");

                    b.Navigation("Start");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Race", b =>
                {
                    b.Navigation("Categories");

                    b.Navigation("Riders");

                    b.Navigation("Starts");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.RaceCategory", b =>
                {
                    b.Navigation("Starts");
                });

            modelBuilder.Entity("VeloTiming.Server.Data.Start", b =>
                {
                    b.Navigation("Categories");
                });
#pragma warning restore 612, 618
        }
    }
}
