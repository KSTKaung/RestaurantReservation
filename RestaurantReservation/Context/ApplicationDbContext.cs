using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Models;

namespace RestaurantReservation.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>contextOptions)
            :base(contextOptions)
        {

        }

        public DbSet<UserModel> Users { get; set; }
    }
}
