using Microsoft.EntityFrameworkCore;
using Poliak_UI_WT.Domain.Entities;

namespace Poliak_UI_WT.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Phone> Phones { get; set; }
    }
}
