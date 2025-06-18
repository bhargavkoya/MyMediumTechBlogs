using System.Diagnostics;

namespace AsyncWebAPIExample.Services
{

    //Comprehensive Use Case: E-commerce Order Processing System
    public class OrderProcessingService
    {
        private readonly IPaymentService _paymentService;
        private readonly IInventoryService _inventoryService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(
            IPaymentService paymentService,
            IInventoryService inventoryService,
            IEmailService emailService,
            ILogger<OrderProcessingService> logger)
        {
            _paymentService = paymentService;
            _inventoryService = inventoryService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<OrderResult> ProcessOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            using var activity = Activity.StartActivity("ProcessOrder");
            activity?.SetTag("orderId", order.Id.ToString());

            try
            {
                // Step 1: Validate inventory concurrently
                var inventoryTasks = order.Items.Select(async item =>
                {
                    var available = await _inventoryService.CheckAvailabilityAsync(
                        item.ProductId, item.Quantity, cancellationToken)
                        .ConfigureAwait(false);

                    return new { Item = item, Available = available };
                });

                var inventoryResults = await Task.WhenAll(inventoryTasks)
                    .ConfigureAwait(false);

                var unavailableItems = inventoryResults
                    .Where(r => !r.Available)
                    .Select(r => r.Item)
                    .ToList();

                if (unavailableItems.Any())
                {
                    return OrderResult.Failed($"Items not available: {string.Join(", ", unavailableItems.Select(i => i.ProductId))}");
                }

                // Step 2: Reserve inventory and process payment concurrently
                var reserveInventoryTask = ReserveInventoryAsync(order.Items, cancellationToken);
                var processPaymentTask = _paymentService.ProcessPaymentAsync(
                    order.PaymentInfo, order.TotalAmount, cancellationToken);

                // Wait for both operations
                var results = await Task.WhenAll(reserveInventoryTask, processPaymentTask)
                    .ConfigureAwait(false);

                var inventoryReserved = results[0];
                var paymentResult = (PaymentResult)results[1];

                if (!paymentResult.IsSuccess)
                {
                    // Release reserved inventory
                    await ReleaseInventoryAsync(order.Items, cancellationToken)
                        .ConfigureAwait(false);
                    return OrderResult.Failed($"Payment failed: {paymentResult.ErrorMessage}");
                }

                // Step 3: Finalize order and send notifications
                await FinalizeOrderAsync(order, cancellationToken)
                    .ConfigureAwait(false);

                // Step 4: Send notifications asynchronously (fire-and-forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendOrderNotificationsAsync(order, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send order notifications for order {OrderId}", order.Id);
                    }
                }, cancellationToken);

                return OrderResult.Success(order.Id);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Order processing cancelled for order {OrderId}", order.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
                return OrderResult.Failed($"Internal error: {ex.Message}");
            }
        }

        private async Task<bool> ReserveInventoryAsync(List<OrderItem> items, CancellationToken cancellationToken)
        {
            var tasks = items.Select(item =>
                _inventoryService.ReserveAsync(item.ProductId, item.Quantity, cancellationToken));

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.All(r => r);
        }

        private async Task ReleaseInventoryAsync(List<OrderItem> items, CancellationToken cancellationToken)
        {
            var tasks = items.Select(item =>
                _inventoryService.ReleaseAsync(item.ProductId, item.Quantity, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task FinalizeOrderAsync(Order order, CancellationToken cancellationToken)
        {
            // Update order status in database
            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Simulate DB operation
            _logger.LogInformation("Order {OrderId} finalized", order.Id);
        }

        private async Task SendOrderNotificationsAsync(Order order, CancellationToken cancellationToken)
        {
            // Send multiple notifications concurrently
            var tasks = new List<Task>
        {
            _emailService.SendOrderConfirmationAsync(order.CustomerEmail, order, cancellationToken),
            _emailService.SendOrderNotificationToWarehouseAsync(order, cancellationToken),
            SendSmsNotificationAsync(order.CustomerPhone, order.Id, cancellationToken)
        };

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task SendSmsNotificationAsync(string phoneNumber, Guid orderId, CancellationToken cancellationToken)
        {
            // Simulate SMS sending
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("SMS notification sent for order {OrderId}", orderId);
        }
    }

    public class OrderResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public Guid? OrderId { get; private set; }

        private OrderResult() { }

        public static OrderResult Success(Guid orderId) => new OrderResult
        {
            IsSuccess = true,
            OrderId = orderId
        };

        public static OrderResult Failed(string errorMessage) => new OrderResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

}
