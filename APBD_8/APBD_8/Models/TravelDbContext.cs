using APBD_8.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_8.Models
{
    public class TravelDbContext : DbContext
    {
        public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Client_Trip> ClientTrips { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Country_Trip> CountryTrips { get; set; }
        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.IdClient);
            });

            modelBuilder.Entity<Client_Trip>(entity =>
            {
                entity.HasKey(e => new { e.IdClient, e.IdTrip });
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.ClientTrips)
                      .HasForeignKey(e => e.IdClient);
                entity.HasOne(e => e.Trip)
                      .WithMany(t => t.ClientTrips)
                      .HasForeignKey(e => e.IdTrip);
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.IdCountry);
            });

            modelBuilder.Entity<Country_Trip>(entity =>
            {
                entity.HasKey(e => new { e.IdCountry, e.IdTrip });
                entity.HasOne(e => e.Country)
                      .WithMany(c => c.CountryTrips)
                      .HasForeignKey(e => e.IdCountry);
                entity.HasOne(e => e.Trip)
                      .WithMany(t => t.CountryTrips)
                      .HasForeignKey(e => e.IdTrip);
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.IdTrip);
            });
        }
    }
}
