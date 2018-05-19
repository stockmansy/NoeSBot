using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NoeSbot.Database;
using NoeSbot.Database.Models;

namespace NoeSbot.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20180519120140_AddProfileBackground")]
    partial class AddProfileBackground
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

            modelBuilder.Entity("NoeSbot.Database.Models.EventItem", b =>
                {
                    b.Property<int>("EventItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<DateTime>("CreationDate");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<long>("GuildId");

                    b.Property<DateTime?>("MatchDate");

                    b.Property<DateTime>("ModifiedDate");

                    b.Property<string>("Name");

                    b.Property<int>("Type");

                    b.Property<string>("UniqueIdentifier");

                    b.HasKey("EventItemId");

                    b.ToTable("EventItemEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.EventItem+Organiser", b =>
                {
                    b.Property<int>("OrganiserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("EventItemId");

                    b.Property<long>("UserId");

                    b.HasKey("OrganiserId");

                    b.HasIndex("EventItemId");

                    b.ToTable("Organiser");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.EventItem+Participant", b =>
                {
                    b.Property<int>("ParticipantId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("EventItemId");

                    b.Property<long?>("MatchUserId");

                    b.Property<long>("UserId");

                    b.HasKey("ParticipantId");

                    b.HasIndex("EventItemId");

                    b.ToTable("Participant");
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

            modelBuilder.Entity("NoeSbot.Database.Models.NotifyItem", b =>
                {
                    b.Property<int>("NotifyItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GuildId");

                    b.Property<string>("Logo");

                    b.Property<string>("Name");

                    b.Property<int>("Type");

                    b.Property<string>("Value");

                    b.HasKey("NotifyItemId");

                    b.ToTable("NotifyItemEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.NotifyItem+NotifyRole", b =>
                {
                    b.Property<int>("NotifyRoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("NotifyItemId");

                    b.Property<long>("RoleId");

                    b.Property<string>("Rolename");

                    b.HasKey("NotifyRoleId");

                    b.HasIndex("NotifyItemId");

                    b.ToTable("NotifyRole");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.NotifyItem+NotifyUser", b =>
                {
                    b.Property<int>("NotifyUserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("NotifyItemId");

                    b.Property<long>("UserId");

                    b.HasKey("NotifyUserId");

                    b.HasIndex("NotifyItemId");

                    b.ToTable("NotifyUser");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.Profile", b =>
                {
                    b.Property<int>("ProfileId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GuildId");

                    b.Property<long>("UserId");

                    b.HasKey("ProfileId");

                    b.ToTable("ProfileEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.ProfileBackground", b =>
                {
                    b.Property<int>("ProfileBackgroundId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GuildId");

                    b.Property<int>("ProfileBackgroundSettingId");

                    b.Property<long?>("UserId");

                    b.Property<string>("Value");

                    b.HasKey("ProfileBackgroundId");

                    b.ToTable("ProfileBackgroundEntities");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.ProfileBackground+ProfileBackgroundAlias", b =>
                {
                    b.Property<int>("AliasId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Alias");

                    b.Property<int?>("ProfileBackgroundId");

                    b.HasKey("AliasId");

                    b.HasIndex("ProfileBackgroundId");

                    b.ToTable("ProfileBackgroundAlias");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.ProfileItem", b =>
                {
                    b.Property<int>("ProfileItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ProfileId");

                    b.Property<int>("ProfileItemTypeId");

                    b.Property<string>("Value");

                    b.HasKey("ProfileItemId");

                    b.HasIndex("ProfileId");

                    b.ToTable("ProfileItemEntities");
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

            modelBuilder.Entity("NoeSbot.Database.Models.EventItem+Organiser", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.EventItem")
                        .WithMany("Organisers")
                        .HasForeignKey("EventItemId");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.EventItem+Participant", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.EventItem")
                        .WithMany("Participants")
                        .HasForeignKey("EventItemId");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.NotifyItem+NotifyRole", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.NotifyItem")
                        .WithMany("Roles")
                        .HasForeignKey("NotifyItemId");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.NotifyItem+NotifyUser", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.NotifyItem")
                        .WithMany("Users")
                        .HasForeignKey("NotifyItemId");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.ProfileBackground+ProfileBackgroundAlias", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.ProfileBackground")
                        .WithMany("Aliases")
                        .HasForeignKey("ProfileBackgroundId");
                });

            modelBuilder.Entity("NoeSbot.Database.Models.ProfileItem", b =>
                {
                    b.HasOne("NoeSbot.Database.Models.Profile", "Profile")
                        .WithMany("Items")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
