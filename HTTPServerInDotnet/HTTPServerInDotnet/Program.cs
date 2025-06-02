using HttpServerInDotnet.HTTPListnerApproach;

class Program
{
    static async Task Main(string[] args)
    {
        string[] prefixes = { "http://localhost:8080/" };

        var server = new HttpListenerServer(prefixes);

        Console.WriteLine("Starting HTTP Server...");
        Console.WriteLine("Press 'q' to quit");

        // Start server in background
        var serverTask = server.StartAsync();

        // Wait for quit command
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                break;
        }

        Console.WriteLine("Stopping server...");
        server.Stop();

        try
        {
            await serverTask;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Server stopped with an exception."+ex.Message);
        }

        Console.WriteLine("Server stopped.");
    }
}