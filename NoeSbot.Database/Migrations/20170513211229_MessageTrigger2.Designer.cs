using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;

namespace NoeSbot.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20170513211229_MessageTrigger2")]
    partial class MessageTrigger2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("NoeSbot.Database.Models.Config", b =>
                {
                    b.Property<int>("ConfigurationId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ConfigurationTypeId");

                    b.Property<long>("GuildId");

                    b.Property<string>("Value");

                    b.HasKey("ConfigurationId");

                    b.ToTable("ConfigurationEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.CustomPunished", b =>
                {
                    b.Property<int>("CustomPunishedId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DelayMessage");

                    b.Property<string>("Reason");

                    b.Property<long>("UserId");

                    b.HasKey("CustomPunishedId");

                    b.ToTable("CustomPunishedEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.MessageTrigger", b =>
                {
                    b.Property<int>("MessageTriggerId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message");

                    b.Property<long>("Server");

                    b.Property<string>("Trigger");

                    b.Property<bool>("Tts");

                    b.HasKey("MessageTriggerId");

                    b.ToTable("MessageTriggerEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.Punished", b =>
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
