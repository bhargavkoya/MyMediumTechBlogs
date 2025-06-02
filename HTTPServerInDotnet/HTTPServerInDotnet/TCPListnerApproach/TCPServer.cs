using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HTTPServerInDotnet.TCPListnerApproach
{
    internal class TCPServer
    {
        //public static void Main(string[] args)
        //{
        //    List<IPAddress> localIPs = GetLocalHostIPs();
        //    TCPServer server = new TCPServer();
        //    TcpListener tcpListner = new TcpListener(System.Net.IPAddress.Any, 8080);
        //    server.Start(tcpListner);
        //    while (true)
        //    {
        //       if(tcpListner.Pending())
        //        {
        //          Console.WriteLine("Start processing a request");
        //            var socket = tcpListner.AcceptSocket();
        //            Task.Run(()=> ProcessRequest(socket));
        //        }
        //    }
        //}
        
        public void Start(TcpListener server)
        {
            server.Start();
            Console.WriteLine("Server started on port 8080...");
        }

        public static void ProcessRequest(Socket socket)
        {
            
            Console.WriteLine("Client connected.");



            var myHttpRequest = MyHttpRequest.ParseHttpRequest(socket);
            if (myHttpRequest is not null)
            {
                Console.WriteLine($"HTTP Method {myHttpRequest.Method}");
                Console.WriteLine($"HTTP Method {myHttpRequest.Target}");
                Console.WriteLine($"HTTP Method {myHttpRequest.Version}");
            }

            string responseString = "";
            if (myHttpRequest is not null)
            {
                if (myHttpRequest.Target == "/")
                {
                    responseString = "HTTP/1.1 200 OK\r\n\r\n";
                }
                else if (myHttpRequest.Target?.ToLower().StartsWith("/echo/", StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    string content = myHttpRequest.Target.Substring(6);
                    responseString = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n Content-Length: {content.Length}\r\n\r\n{content}";
                }
                else if (myHttpRequest.Target?.ToLower().StartsWith("/user-agent", StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    string userAgentValue = myHttpRequest.Headers["User-Agent"] ?? "";
                    responseString = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n Content-Length: {userAgentValue.Length}\r\n\r\n{userAgentValue}";
                }
                else
                {
                    responseString = "HTTP/1.1 404 Not Found\r\n\r\n";
                }
            }

            Console.WriteLine("Sending response " + responseString);
            byte[] bytes = Encoding.ASCII.GetBytes(responseString);
            socket.Send(bytes);
        }

        

        /// <summary>
        /// Returns list of IP addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
        /// </summary>
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;
        }
    }
}
