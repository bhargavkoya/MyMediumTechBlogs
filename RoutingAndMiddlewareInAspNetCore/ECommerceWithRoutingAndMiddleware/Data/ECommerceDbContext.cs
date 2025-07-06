using ECommerceWithRoutingAndMiddleware.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ECommerceWithRoutingAndMiddleware.Data
{
    public class ECommerceDbContext : DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
    }

}
