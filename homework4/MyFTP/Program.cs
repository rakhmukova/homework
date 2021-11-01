using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyFTP
{
    public class Program
    {
        private static async Task Main()
        {
            try
            {
                int serverPort = 8000;
                var address = IPAddress.Parse("127.0.0.1");
                var endPoint = new IPEndPoint(address, serverPort);
                var server = new Server(endPoint);
                server.Run();
                var client = new Client(endPoint);
                var dirInfo = await client.ListFilesAsync("../../../..");
                foreach (var (name, isDir) in dirInfo)
                {
                    Console.WriteLine($"{name} {isDir}");
                }

                var (_, content) = await client.DownloadFileAsync("../../../../.editorconfig");
                var contentAsString = Encoding.ASCII.GetString(content);
                Console.WriteLine(contentAsString);
                server.Stop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
