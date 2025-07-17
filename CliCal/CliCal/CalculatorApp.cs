using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliCal
{
    public class CalculatorApp
    {
        private readonly ExpressionParser _expressionParser;
        private readonly CalculatorDatabase _database;
        private readonly ILogger _logger;

        public CalculatorApp(ExpressionParser expressionParser, CalculatorDatabase database, ILogger logger)
        {
            _expressionParser = expressionParser;
            _database = database;
            _logger = logger;
        }

        public async Task RunAsync(string[] args)
        {
            if (args.Length > 0 && args[0] == "--history")
            {
                await ShowHistoryAsync();
                return;
            }

            Console.WriteLine("CLI Calculator - Type 'help' for commands or 'exit' to quit");

            while (true)
            {
                Console.Write("calc> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.ToLower() == "exit")
                    break;

                if (input.ToLower() == "help")
                {
                    ShowHelp();
                    continue;
                }

                if (input.ToLower() == "history")
                {
                    await ShowHistoryAsync();
                    continue;
                }

                await ProcessCalculationAsync(input);
            }
        }

        private async Task ProcessCalculationAsync(string expression)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = _expressionParser.Evaluate(expression);
                stopwatch.Stop();

                Console.WriteLine($"Result: {result}");

                await _database.LogCalculationAsync(expression, result, stopwatch.Elapsed);
                _logger.LogInformation("Calculated {Expression} = {Result} in {Duration}ms",
                    expression, result, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger.LogError(ex, "Failed to calculate expression: {Expression}", expression);
            }
        }

        private async Task ShowHistoryAsync()
        {
            var history = await _database.GetRecentCalculationsAsync();

            if (!history.Any())
            {
                Console.WriteLine("No calculations in history.");
                return;
            }

            Console.WriteLine("\nRecent Calculations:");
            Console.WriteLine("Timestamp               Expression        Result    Duration");
            Console.WriteLine("--------------------------------------------------------");

            foreach (var record in history)
            {
                Console.WriteLine($"{record.Timestamp:yyyy-MM-dd HH:mm:ss} {record.Expression,12} = {record.Result,8:F2} ({record.Duration.TotalMilliseconds,4:F1}ms)");
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("\nAvailable Commands:");
            Console.WriteLine("  help     - Show this help message");
            Console.WriteLine("  history  - Show recent calculations");
            Console.WriteLine("  exit     - Exit the calculator");
            Console.WriteLine("\nSupported Operations: +, -, *, /");
            Console.WriteLine("Example: 3 + 4 * 2");
        }
    }

}
