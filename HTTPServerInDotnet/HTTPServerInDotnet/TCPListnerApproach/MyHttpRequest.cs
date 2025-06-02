using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HTTPServerInDotnet.TCPListnerApproach
{
    public class MyHttpRequest
    {
        public string? Method { get; set; }
        public string? Target { get; set; }
        public string? Version { get; set; }
        public Dictionary<string, string>? Headers { get; set; }

        public static MyHttpRequest? ParseHttpRequest(Socket socket)
        {
            try
            {
                string response = Receive(socket);
                if(string.IsNullOrEmpty(response))
                {
                    return null; // No data received
                }

                var (method, target, version) = GetRequestParts(response);
                return new MyHttpRequest
                {
                    Method = method,
                    Target = target,
                    Version = version,
                    Headers = GetHeaders(response)
                };
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Error parsing HTTP request: " + ex.Message);
                return null;
            }
        }

        private static Dictionary<string, string> GetHeaders(string request)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string strHeaders = request.Substring(request.IndexOf("Host"), request.IndexOf("\r\n") - request.IndexOf("Host"));
            string[] lines = request.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) // Skip the first line which is the request line
            {
                var parts = line.Split(new[] { ':' });
                
                headers.Add(parts[0].Trim(),parts[1].Trim());
                
            }
            return headers;
        }

        public static string? Receive(Socket socket)
        {
            string data = string.Empty;
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = socket.Receive(buffer,buffer.Length,SocketFlags.None);
                data += Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (bytesRead <= 0 || socket.Available<=0)
                {
                    break; // Client disconnected
                }
                
            }
            Console.WriteLine("Received data: " + data);
            return data;
        }

        public static (string? method, string? target, string? version) GetRequestParts(string requestLine)
        {
            var parts = requestLine.Split(' ');
            if (parts.Length < 3)
            {
                throw new FormatException("Invalid HTTP request line format.");
            }
            
            return (parts[0], parts[1], parts[2]);
        }
    }
}
