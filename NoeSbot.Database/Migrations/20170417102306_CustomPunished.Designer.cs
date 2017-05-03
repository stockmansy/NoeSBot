using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;

namespace NoeSbot.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20170417102306_CustomPunished")]
    partial class CustomPunished
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

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
