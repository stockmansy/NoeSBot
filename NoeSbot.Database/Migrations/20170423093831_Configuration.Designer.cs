using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;

namespace NoeSbot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20170423093831_Configuration")]
    partial class Configuration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("NoeSbot.Database.Config", b =>
                {
                    b.Property<int>("ConfigurationId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ConfigurationTypeId");

                    b.Property<long>("GuildId");

                    b.Property<string>("Value");

                    b.HasKey("ConfigurationId");

                    b.ToTable("ConfigurationEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.CustomPunished", b =>
                {
                    b.Property<int>("CustomPunishedId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DelayMessage");

                    b.Property<string>("Reason");

                    b.Property<long>("UserId");

                    b.HasKey("CustomPunishedId");

                    b.ToTable("CustomPunishedEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Punished", b =>
                {
                    b.Property<int>("PunishedId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Duration");

                    b.Property<string>("Reason");

                    b.Property<DateTime>("TimeOfPunishment");

                    b.Property<long>("UserId");

                    b.HasKey("PunishedId");

                    b.ToTable("PunishedEntities");
                });
        }
    }
}
