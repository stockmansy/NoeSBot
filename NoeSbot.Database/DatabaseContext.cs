using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NoeSbot.Database.Models;

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
        }

        public DbSet<Punished> PunishedEntities { get; set; }

        public DbSet<Config> ConfigurationEntities { get; set; }

        public DbSet<CustomPunished> CustomPunishedEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Punished>();
            modelBuilder.Entity<CustomPunished>();
        }
    }

    // DbContextFactory => It's to generate the migrations
    public class DatabaseContextFactory : IDbContextFactory<DatabaseContext>
    {
        public DatabaseContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql("Server=localhost; Port=3306; Database=noesbot; Uid=root; Pwd=example;");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
