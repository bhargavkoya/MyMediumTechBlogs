using ExceptionHandlingInNET.ExceptionHandler;
using Serilog.Events;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.  
//Log.Logger = new LoggerConfiguration()
//   .MinimumLevel.Information()
//   .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//   .Enrich.FromLogContext()
//   .Enrich.WithProperty("Application", "MyApp")
//   .WriteTo.Console(outputTemplate:
//       "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
//   .WriteTo.File("logs/app-.log",
//       rollingInterval: RollingInterval.Day,
//       retainedFileCountLimit: 30)
//   .CreateLogger();

//builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<TimeoutExceptionHandler>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
