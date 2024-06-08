using Microsoft.EntityFrameworkCore;

namespace MeterReadingAPI.Models
{

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Account> Accounts 
        { 
            get; 
            set; 
        }

        public DbSet<MeterReading> MeterReadings 
        { 
            get; 
            set; 
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("YourConnectionStringHere");
        }

    }

}
