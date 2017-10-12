﻿// <auto-generated />
using Exercise.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Exercise.Data.Migrations
{
    [DbContext(typeof(MeetingContext))]
    [Migration("20171011183446_AlternateKey")]
    partial class AlternateKey
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Exercise.Domain.Meeting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date")
                        .HasColumnType("Date");

                    b.Property<string>("Name");

                    b.Property<int>("OuterId");

                    b.Property<string>("State");

                    b.HasKey("Id");

                    b.HasAlternateKey("OuterId");

                    b.ToTable("Meetings");
                });

            modelBuilder.Entity("Exercise.Domain.Race", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Endtime");

                    b.Property<int>("MeetingId");

                    b.Property<string>("Name");

                    b.Property<int>("Number");

                    b.Property<int>("OuterId");

                    b.Property<DateTime>("Starttime");

                    b.HasKey("Id");

                    b.HasAlternateKey("OuterId");

                    b.HasIndex("MeetingId");

                    b.ToTable("Races");
                });

            modelBuilder.Entity("Exercise.Domain.Race", b =>
                {
                    b.HasOne("Exercise.Domain.Meeting", "Meeting")
                        .WithMany("Races")
                        .HasForeignKey("MeetingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
