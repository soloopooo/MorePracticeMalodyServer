﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MorePracticeMalodyServer.Data;

namespace MorePracticeMalodyServer.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210824134819_UpdateSearch")]
    partial class UpdateSearch
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Chart", b =>
                {
                    b.Property<int>("ChartId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Creator")
                        .HasColumnType("TEXT");

                    b.Property<int>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Mode")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SongId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .HasColumnType("TEXT");

                    b.HasKey("ChartId");

                    b.HasIndex("Level");

                    b.HasIndex("Mode");

                    b.HasIndex("SongId");

                    b.HasIndex("Type");

                    b.ToTable("Charts");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Download", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChartId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("File")
                        .HasColumnType("TEXT");

                    b.Property<string>("Hash")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChartId");

                    b.ToTable("Downloads");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Event", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Active")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cover")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("End")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.HasKey("EventId");

                    b.HasIndex("Active");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.EventChart", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChartId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EventId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ChartId");

                    b.HasIndex("EventId");

                    b.ToTable("EventCharts");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Promotion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SongId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SongId");

                    b.ToTable("Promotions");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Song", b =>
                {
                    b.Property<int>("SongId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<double>("Bpm")
                        .HasColumnType("REAL");

                    b.Property<string>("Cover")
                        .HasColumnType("TEXT");

                    b.Property<int>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Mode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalArtist")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalSearchString")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalTitle")
                        .HasColumnType("TEXT");

                    b.Property<string>("SearchSting")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("SongId");

                    b.HasIndex("Mode");

                    b.HasIndex("OriginalSearchString");

                    b.HasIndex("SearchSting");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Chart", b =>
                {
                    b.HasOne("MorePracticeMalodyServer.Model.DbModel.Song", "Song")
                        .WithMany("Charts")
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Song");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Download", b =>
                {
                    b.HasOne("MorePracticeMalodyServer.Model.DbModel.Chart", "Chart")
                        .WithMany("Downloads")
                        .HasForeignKey("ChartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chart");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.EventChart", b =>
                {
                    b.HasOne("MorePracticeMalodyServer.Model.DbModel.Chart", "Chart")
                        .WithMany()
                        .HasForeignKey("ChartId");

                    b.HasOne("MorePracticeMalodyServer.Model.DbModel.Event", null)
                        .WithMany("EventCharts")
                        .HasForeignKey("EventId");

                    b.Navigation("Chart");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Promotion", b =>
                {
                    b.HasOne("MorePracticeMalodyServer.Model.DbModel.Song", "Song")
                        .WithMany()
                        .HasForeignKey("SongId");

                    b.Navigation("Song");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Chart", b =>
                {
                    b.Navigation("Downloads");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Event", b =>
                {
                    b.Navigation("EventCharts");
                });

            modelBuilder.Entity("MorePracticeMalodyServer.Model.DbModel.Song", b =>
                {
                    b.Navigation("Charts");
                });
#pragma warning restore 612, 618
        }
    }
}
