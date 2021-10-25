using System;
using System.Net;
using System.Threading.Tasks;

namespace MyFTP
{
    public class Program
    {
        public static void Main()
        {
            int serverPort = 8000;
            var address = IPAddress.Parse("127.0.0.1");
            var endPoint = new IPEndPoint(address, serverPort);
            var server = new Server(endPoint);
            Task.Run(async () => await server.Run());
            Console.ReadKey();
            server.Stop();
        }
    }
}
