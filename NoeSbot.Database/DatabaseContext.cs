using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NoeSbot.Database.Models;
using System.Linq;
using System.Reflection;

namespace NoeSbot.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
        : base(options)
        {
            // .NET Core EF7 fix -_-
            Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (`MigrationId` nvarchar(150) NOT NULL, `ProductVersion` nvarchar(32) NOT NULL,PRIMARY KEY(`MigrationId`))");

            Database.EnsureCreated();
            Database.Migrate();

            var entries = ChangeTracker
                            .Entries()
                            .Where(x => x.State == EntityState.Modified 
                                     || x.State == EntityState.Added 
                                     && x.Entity != null 
                                     && typeof(BaseModel).IsAssignableFrom(x.Entity.GetType()))
                            .ToList();

            
            foreach (var entry in entries)
            {
                var entityBase = entry.Entity as BaseModel;
                if (entry.State == EntityState.Added)
                {
                    entityBase.CreationDate = System.DateTime.UtcNow;
                }

                entityBase.ModifiedDate = System.DateTime.UtcNow;
            }
        }

        public DbSet<Punished> PunishedEntities { get; set; }

        public DbSet<Config> ConfigurationEntities { get; set; }

        public DbSet<CustomPunished> CustomPunishedEntities { get; set; }

        public DbSet<MessageTrigger> MessageTriggerEntities { get; set; }

        public DbSet<Profile> ProfileEntities { get; set; }

        public DbSet<ProfileItem> ProfileItemEntities { get; set; }

        public DbSet<NotifyItem> NotifyItemEntities { get; set; }

        public DbSet<EventItem> EventItemEntities { get; set; }

        public DbSet<ProfileBackground> ProfileBackgroundEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Punished>();
            modelBuilder.Entity<CustomPunished>();
            modelBuilder.Entity<MessageTrigger>();
            modelBuilder.Entity<Profile>();
            modelBuilder.Entity<ProfileItem>();
            modelBuilder.Entity<NotifyItem>();
            modelBuilder.Entity<EventItem>();
            modelBuilder.Entity<ProfileBackground>();
        }
    }

    // DbContextFactory => It's to generate the migrations
    public class DatabaseContextFactory : IDbContextFactory<DatabaseContext>
    {
        public DatabaseContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql("Server=localhost; Port=; Database=noesbot; Uid=noesbot; Pwd=123456;");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
