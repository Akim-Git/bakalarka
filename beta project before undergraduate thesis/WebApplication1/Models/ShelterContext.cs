using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class ShelterContext : DbContext
    {
        public DbSet<Shelter> Shelters { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Dog> Dogs { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public ShelterContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
