using Microsoft.EntityFrameworkCore;
using Exercise.Domain;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Exercise.Data
{
    public class MeetingContext : DbContext
    {
        public DbSet<Meeting> Meetings { get; set; }

        public DbSet<Race> Races { get; set; }

        public MeetingContext()
        {
        }

        public MeetingContext(DbContextOptions options) : base(options) { }

        public MeetingContext(DbContextOptions<MeetingContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

                var connectionString = config["ConnectionStrings:ProdDatabase"] ?? "Server = (localdb)\\mssqllocaldb; Database = MeetingDb; Trusted_Connection = True;";
                optionsBuilder.UseSqlServer(connectionString,
                                            options => options.MaxBatchSize(30));
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meeting>().Property(p => p.Date)
                    .HasColumnType("Date");
            modelBuilder.Entity<Meeting>().HasAlternateKey(m => m.OuterId);

            modelBuilder.Entity<Race>().HasAlternateKey(r => r.OuterId);

        }
    }
}
