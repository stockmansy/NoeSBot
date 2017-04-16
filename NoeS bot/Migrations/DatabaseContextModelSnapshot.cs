using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;

namespace NoeSbot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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
