using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        // Initiate class constructor.
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        // Create database tables. 
        public DbSet<AppUser> Users { get; set; } 
    }
}
