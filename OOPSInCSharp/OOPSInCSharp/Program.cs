// See https://aka.ms/new-console-template for more information
using OOPSInCSharp;

Console.WriteLine("Hello, World!");

//Encapsulation
Console.WriteLine("Encapsulation Example: Bank Account Management");
BankAccount account = new BankAccount("John Doe", 1000);
account.Deposit(500);
account.Withdraw(200);
account.GetTransactionHistory().ToList().ForEach(t =>
    Console.WriteLine($"{t.Date}: {t.Type} of ${t.Amount}"));

//Inheritance
Console.WriteLine("\nInheritance Example: Savings Account Management");
Console.WriteLine("Creating a savings account with 5% interest rate.");
SavingsAccount savingsAccount = new SavingsAccount("Jane Doe", 0.05m);
savingsAccount.Deposit(1000);
savingsAccount.Withdraw(300);
savingsAccount.AddInterest();
savingsAccount.GetTransactionHistory().ToList().ForEach(t =>
    Console.WriteLine($"{t.Date}: {t.Type} of ${t.Amount}"));

Console.WriteLine("Creating a checking account with 5% overdraft limit.");
CheckingAccount checkingAccount = new CheckingAccount("Alice Smith", 0.05m);
checkingAccount.Deposit(500);
checkingAccount.Withdraw(600); // Should succeed due to overdraft
checkingAccount.GetTransactionHistory().ToList().ForEach(t =>
    Console.WriteLine($"{t.Date}: {t.Type} of ${t.Amount}"));
Console.WriteLine($"Checking Available Funds: ${checkingAccount.AvailableFunds}");

//Abstraction with interfaces
Console.WriteLine("\nAbstraction Example: Payment processor service");
PaymentService paymentService = new PaymentService();

Console.WriteLine("Registering paypal payment processors...");
paymentService.RegisterProcessor("PayPal",new PayPalProcessor());
var payPalPaymentResult = paymentService.ProcessPaymentAsync("PayPal", new PayPalDetails(20,"paypal@gmail.com"));
Console.WriteLine($"PayPal Payment Result: {payPalPaymentResult.Result.Message}");
var payPalRefundResult = paymentService.RefundPaymentAsync("PayPal",payPalPaymentResult.Result.TransactionId, 20);
Console.WriteLine($"PayPal Refund Result: {payPalRefundResult.Result.Message}");

Console.WriteLine("Registering credit card payment processors...");
paymentService.RegisterProcessor("CreditCard", new PayPalProcessor());
var creditCardPaymentResult = paymentService.ProcessPaymentAsync("CreditCard", new PayPalDetails(20, "creditcard@gmail.com"));
Console.WriteLine($"Creditcard Payment Result: {creditCardPaymentResult.Result.Message}");
var creditCardRefundResult = paymentService.RefundPaymentAsync("CreditCard", creditCardPaymentResult.Result.TransactionId, 15);
Console.WriteLine($"Creditcard Refund Result: {creditCardRefundResult.Result.Message}");

Console.WriteLine("Registering crypto payment processors...");
paymentService.RegisterProcessor("Crypto", new PayPalProcessor());
var cryptoPaymentResult = paymentService.ProcessPaymentAsync("Crypto", new PayPalDetails(20, "crypto@gmail.com"));
Console.WriteLine($"Creditcard Payment Result: {cryptoPaymentResult.Result.Message}");
var cryptoRefundResult = paymentService.RefundPaymentAsync("Crypto", cryptoPaymentResult.Result.TransactionId, 10);
Console.WriteLine($"Crypto Refund Result: {cryptoRefundResult.Result.Message}");

//Polymorphism in action
Console.WriteLine("Polymorphism in action");
var app = new ECommerceApplication();
var order = new Order { Total = 99.99m };

// Same method call, different payment processors handle it differently
await app.ProcessOrderAsync(order, "CreditCard",
    new CreditCardDetails(99.99m, "4111111111111111", "123", DateTime.Now.AddYears(2)));

await app.ProcessOrderAsync(order, "PayPal",
    new PayPalDetails(99.99m, "user@example.com"));

await app.ProcessOrderAsync(order, "Crypto",
    new CryptoDetails(99.99m, "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa", "BTC"));



