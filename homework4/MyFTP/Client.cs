using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MyFTP
{
    /// <summary>
    /// TCP client.
    /// </summary>
    public class Client
    {
        private readonly TcpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="endPoint">Client's address.</param>
        public Client(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }

            this.client = new (endPoint);
        }

        /// <summary>
        /// Sends a requset to server with the intention
        /// to get the information about the directory.
        /// </summary>
        /// <param name="serverEndPoint">Server address.</param>
        /// <param name="path">A relative directory path.</param>
        /// <returns>The number of directories and their list.</returns>
        public async Task<string> ListFilesAsync(IPEndPoint serverEndPoint, string path)
            => await SendRequestAsync(serverEndPoint, path, 1);

        /// <summary>
        /// Sends a request to server with the intention
        /// to download the file.
        /// </summary>
        /// <param name="serverEndPoint">Server address.</param>
        /// <param name="path">A relative directory path.</param>
        /// <returns>The file's size and content.</returns>
        public async Task<string> DownloadFileAsync(IPEndPoint serverEndPoint, string path)
            => await SendRequestAsync(serverEndPoint, path, 2);

        private async Task<string> SendRequestAsync(IPEndPoint serverEndPoint, string path, int requestType)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            client.Connect(serverEndPoint);
            var stream = client.GetStream();
            using var streamWriter = new StreamWriter(stream) { AutoFlush = true };
            using var streamReader = new StreamReader(stream);
            streamWriter.Write($"{requestType} {path}");
            return await streamReader.ReadLineAsync();
        }
    }
}
