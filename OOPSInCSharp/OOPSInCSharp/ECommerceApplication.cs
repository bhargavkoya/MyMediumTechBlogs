using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPSInCSharp
{
    public class Order
    {
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ECommerceApplication
    {
        private readonly PaymentService _paymentService;

        public ECommerceApplication()
        {
            _paymentService = new PaymentService();
        }

        public async Task ProcessOrderAsync(Order order, string paymentMethod, PaymentDetails details)
        {
            Console.WriteLine($"\n=== Processing Order Total: ${order.Total} ===");

            // Polymorphism in action - same method call, different implementations
            var result = await _paymentService.ProcessPaymentAsync(paymentMethod, details);

            if (result.Success)
            {
                Console.WriteLine($"✅ Payment successful!");
                Console.WriteLine($"Transaction ID: {result.TransactionId}");
                Console.WriteLine($"Details: {result.Message}");

                // Send confirmation email, update inventory, etc.
                await ProcessSuccessfulOrder(order, result.TransactionId);
            }
            else
            {
                Console.WriteLine($"❌ Payment failed: {result.Message}");
                await HandleFailedPayment(order, result.Message);
            }
        }

        private async Task ProcessSuccessfulOrder(Order order, string transactionId)
        {
            // Common post-payment processing
            Console.WriteLine("📧 Sending confirmation email...");
            Console.WriteLine("📦 Updating inventory...");
            Console.WriteLine("🚚 Scheduling shipment...");
            await Task.Delay(100); // Simulate processing
        }

        private async Task HandleFailedPayment(Order order, string errorMessage)
        {
            Console.WriteLine("📧 Sending payment failure notification...");
            Console.WriteLine("🔄 Suggesting alternative payment methods...");
            await Task.Delay(100); // Simulate processing
        }
    }
}