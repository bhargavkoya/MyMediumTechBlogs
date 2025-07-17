using CliCal;
using Microsoft.Extensions.Logging;
using System.Text;

Console.WriteLine("Hello, World!");


CalculatorApp calculatorApp=new CalculatorApp(
    new ExpressionParser(),
    new CalculatorDatabase(),
    LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CalculatorApp>()
);


await calculatorApp.RunAsync(args);