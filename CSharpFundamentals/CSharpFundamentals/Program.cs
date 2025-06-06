using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpFundamentals
{
    //custom data types
    public enum DrinkSize { Small, Medium, Large }
    public enum CustomerType { Regular, Student, VIP }

    public class DrinkOrder //custom data type
    {
        //properties with different data types
        public string DrinkName { get; set; } //string
        public DrinkSize Size { get; set; } //enum
        public decimal BasePrice { get; set; } //decimal
        public List<string> Modifiers { get; } = new List<string>(); //collection
        public CustomerType CustomerType { get; set; } //enum

        //method demonstrating control flow and calculations
        public decimal CalculateTotal()
        {
            decimal total = BasePrice;

            //switch expression for size pricing
            total *= Size switch
            {
                DrinkSize.Small => 0.8m,
                DrinkSize.Medium => 1m,
                DrinkSize.Large => 1.3m,
                _ => 1m
            };

            //pattern matching for discounts
            total *= CustomerType switch
            {
                CustomerType.Student => 0.9m,
                CustomerType.VIP => 0.85m,
                _ => 1m
            };

            return Math.Round(total + (Modifiers.Count * 0.5m), 2);
        }
    }

    class Program
    {
        //collections demonstrating different data types
        static Dictionary<string, decimal> menu = new Dictionary<string, decimal>
        {
            {"EggPuff", 2.50m}, {"Lussi", 3.75m}, {"Cake", 4.25m}
        };

        static Dictionary<string, int> inventory = new Dictionary<string, int>
        {
            {"EggPuff", 10}, {"Lussi", 8}, {"Cake", 5}
        };

        static List<DrinkOrder> orders = new List<DrinkOrder>();
        static decimal dailyTotal = 0m;

        static void Main()
        {
            Console.WriteLine("☕ Welcome to Andhra Pradesh Café! ☕\n");

            bool isOpen = true;
            while (isOpen) //main control flow loop
            {
                ShowMenu();
                ProcessUserInput(ref isOpen);
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("1. New Order\t2. Inventory\n3. Sales\t4. Exit");
        }

        static void ProcessUserInput(ref bool isOpen)
        {
            switch (Console.ReadLine())
            {
                case "1":
                    CreateNewOrder();
                    break;
                case "2":
                    ShowInventory();
                    break;
                case "3":
                    ShowSalesReport();
                    break;
                case "4":
                    isOpen = false;
                    Console.WriteLine("\n💰 Daily Total: $" + dailyTotal);
                    break;
                default:
                    Console.WriteLine("Invalid option!");
                    break;
            }
        }

        static void CreateNewOrder()
        {
            var order = new DrinkOrder();

            //type conversion and validation
            Console.Write("Enter drink name: ");
            order.DrinkName = ValidateDrink(Console.ReadLine());

            Console.Write("Size (S/M/L): ");
            //switch expression example
            order.Size = Console.ReadLine().ToUpper() switch
            {
                "S" => DrinkSize.Small,
                "L" => DrinkSize.Large,
                _ => DrinkSize.Medium
            };

            Console.Write("Customer type (R/S/V): ");
            order.CustomerType = Console.ReadLine().ToUpper() switch
            {
                "S" => CustomerType.Student,
                "V" => CustomerType.VIP,
                _ => CustomerType.Regular
            };

            //collection manipulation
            AddModifiers(order);

            ProcessPayment(order);
        }

        static string ValidateDrink(string input)
        {
            //LINQ query with null check
            return menu.Keys.FirstOrDefault(k =>
                k.Equals(input, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException("Invalid drink!");
        }

        static void AddModifiers(DrinkOrder order)
        {
            Console.WriteLine("Add modifiers (comma-separated):");
            Console.WriteLine("1. Extra Shot  2. Almond Milk  3. Iced");

            //Array handling and parsing
            var choices = Console.ReadLine().Split(',');
            foreach (var c in choices)
            {
                if (int.TryParse(c, out int idx) && idx > 0 && idx <= 3)
                {
                    order.Modifiers.Add(
                        new[] { "Extra Shot", "Almond Milk", "Iced" }[idx - 1]
                    );
                }
            }
        }

        static void ProcessPayment(DrinkOrder order)
        {
            decimal total = order.CalculateTotal();
            dailyTotal += total;
            inventory[order.DrinkName]--;

            Console.WriteLine($"\nTotal: ${total}\nThank you!");
        }

        static void ShowInventory()
        {
            //string formatting and interpolation
            Console.WriteLine("\nCurrent Inventory:");
            foreach (var item in inventory)
            {
                Console.WriteLine($"{item.Key.PadRight(10)}: {item.Value} left");
            }
        }

        static void ShowSalesReport()
        {
            //complex LINQ queries
            var popularDrinks = orders
                .GroupBy(o => o.DrinkName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "None";

            Console.WriteLine($"\n⭐ Most Popular: {popularDrinks}");
            Console.WriteLine($"📈 Total Orders: {orders.Count}");
            Console.WriteLine($"💰 Revenue: ${dailyTotal}");
        }
    }
}
