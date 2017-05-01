using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NoeSbot.Database;

namespace NoeSbot
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

    // Shouldn't really be here, is a stupid powershell .net core fix
    public class DatabaseContextFactory : IDbContextFactory<DatabaseContext>
    {
        // Generate migrations fix ><
        public DatabaseContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql("Server=localhost; Port=3306; Database=noesbot; Uid=root; Pwd=;");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
