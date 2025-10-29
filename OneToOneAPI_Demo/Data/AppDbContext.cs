using Microsoft.EntityFrameworkCore;
using OneToOneAPI_Demo.Models;

namespace OneToOneAPI_Demo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Passport> Passports { get; set; }
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Passport)
                .WithOne(ppt => ppt.Person)
                .HasForeignKey<Passport>(ppt => ppt.PersonId);

            modelBuilder.Entity<Person>()
                .HasKey(e=>e.PersonId);
        }
    }
}
