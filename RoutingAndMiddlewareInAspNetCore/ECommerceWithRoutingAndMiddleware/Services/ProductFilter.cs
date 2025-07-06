namespace ECommerceWithRoutingAndMiddleware.Services
{
    public class ProductFilter
    {
        public string? Category { get; internal set; }
        public string? SearchTerm { get; internal set; }
        public double MinPrice { get; internal set; }
    }
}