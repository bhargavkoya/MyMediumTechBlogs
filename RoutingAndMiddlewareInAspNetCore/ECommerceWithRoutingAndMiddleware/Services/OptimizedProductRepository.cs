using ECommerceWithRoutingAndMiddleware.Data;
using ECommerceWithRoutingAndMiddleware.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWithRoutingAndMiddleware.Services
{
    public class OptimizedProductRepository : IProductRepository
    {
        private readonly ECommerceDbContext _context;

        public async Task<PagedResult<Product>> GetProductsAsync(
            ProductFilter filter,
            int page,
            int pageSize)
        {
            var query = _context.Products.AsNoTracking(); // Read-only queries  

            // Apply filters efficiently  
            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(p => p.Category == filter.Category);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => EF.Functions.Contains(p.Name, filter.SearchTerm) ||
                                       EF.Functions.Contains(p.Description, filter.SearchTerm));
            }

            // Fix for CS1061: Check if MinPrice is greater than 0 instead of using HasValue  
            if (filter.MinPrice > 0)
            {
                query = query.Where(p => p.Price >= (decimal)filter.MinPrice);
            }

            // Count before applying pagination  
            var totalCount = await query.CountAsync();

            // Apply pagination  
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category) // Only include what's needed  
                .ToListAsync();

            return new PagedResult<Product>(products, totalCount, page, pageSize);
        }
    }
}
