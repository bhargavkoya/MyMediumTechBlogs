using ECommerceWithRoutingAndMiddleware.Entities;

namespace ECommerceWithRoutingAndMiddleware.Services
{
    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductsAsync(string? category, string? search, int page, int pageSize);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> GetRecommendationsAsync(string? userId);
    }

    public class ProductService : IProductService
    {
        public Task<Product?> GetProductByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Product>> GetProductsAsync(string? category, string? search, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> GetRecommendationsAsync(string? userId)
        {
            throw new NotImplementedException();
        }
    }

    public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
}
