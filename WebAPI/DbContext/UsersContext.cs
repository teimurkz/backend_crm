using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    public class UserContext : DbContext
    {
        private readonly IConfiguration _configuration;
        
        public UserContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users => Set<User>(); 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Database"));
        }
    }

}