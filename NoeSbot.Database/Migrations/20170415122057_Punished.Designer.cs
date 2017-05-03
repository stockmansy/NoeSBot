using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;

namespace NoeSbot.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20170415122057_Punished")]
    partial class Punished
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

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
