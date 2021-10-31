using System;
using System.Collections.Generic;
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
        private readonly IPEndPoint serverEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="serverEndPoint">Server's address.</param>
        public Client(IPEndPoint serverEndPoint)
            => this.serverEndPoint = serverEndPoint
            ?? throw new ArgumentNullException(nameof(serverEndPoint));

        /// <summary>
        /// Sends a requset to server with the intention
        /// to get the information about the directory.
        /// </summary>
        /// <param name="path">A relative directory path.</param>
        /// <returns>The list of subdirectories and files.</returns>
        public async Task<List<(string name, bool isDir)>> ListFilesAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using var client = new TcpClient();
            client.Connect(serverEndPoint);
            using var stream = client.GetStream();
            using var streamWriter = new StreamWriter(stream) { AutoFlush = true };
            streamWriter.WriteLine($"1 {path}");

            using var streamReader = new StreamReader(stream);
            var response = await streamReader.ReadLineAsync();
            var responseParts = response.Split();
            long numOfDirectories = long.Parse(responseParts[0]);
            if (numOfDirectories == -1)
            {
                throw new ArgumentException("The path is invalid.", nameof(path));
            }

            var result = new List<(string, bool)>();
            for (var i = 1; i < responseParts.Length; i += 2)
            {
                var dirInfo = (responseParts[i],
                    responseParts[i + 1] == "true"
                    ? true
                    : false);
                result.Add(dirInfo);
            }

            return result;
        }

        /// <summary>
        /// Sends a request to server with the intention
        /// to download the file.
        /// </summary>
        /// <param name="path">A relative directory path.</param>
        /// <returns>The file's size and content.</returns>
        public async Task<(long size, byte[] content)> DownloadFileAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using var client = new TcpClient();
            client.Connect(serverEndPoint);
            using var stream = client.GetStream();
            using var streamWriter = new StreamWriter(stream) { AutoFlush = true };
            streamWriter.WriteLine($"2 {path}");

            using var streamReader = new StreamReader(stream);
            var sizeInfo = await streamReader.ReadLineAsync();
            long size = long.Parse(sizeInfo);
            if (size == -1)
            {
                throw new ArgumentException("The path is invalid.", nameof(path));
            }

            var content = new byte[size];
            await stream.ReadAsync(content);

            return (size, content);
        }
    }
}
