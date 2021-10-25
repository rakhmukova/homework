using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MyFTP
{
    /// <summary>
    /// TCP server.
    /// </summary>
    public class Server
    {
        private readonly TcpListener listener;
        private readonly CancellationTokenSource source = new ();
        private readonly AutoResetEvent taskStopped = new (false);
        private int numberOfTasks = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="endPoint">Server address.</param>
        public Server(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }

            this.listener = new (endPoint);
        }

        /// <summary>
        /// Stops server's work.
        /// </summary>
        public void Stop()
        {
            source.Cancel();
            while (Volatile.Read(ref numberOfTasks) != 0)
            {
                taskStopped.WaitOne();
            }
        }

        /// <summary>
        /// Runs server.
        /// </summary>
        public async Task Run()
        {
            listener.Start();
            while (!source.Token.IsCancellationRequested)
            {
                using var client = listener.AcceptTcpClient();
                await Task.Run(() => HandleRequest(client));
            }

            listener.Stop();
        }

        private async Task HandleRequest(TcpClient client)
        {
            Interlocked.Increment(ref numberOfTasks);
            try
            {
                var stream = client.GetStream();
                using var streanReader = new StreamReader(stream);
                using var streamWriter = new StreamWriter(stream);
                var requestData = streanReader.ReadToEnd();
                var request = requestData.Split(' ');
                switch (request[0])
                {
                    case "1":
                        await ListFilesAsync(request[1], streamWriter);
                        break;
                    case "2":
                        await DownloadFileAsync(request[1], streamWriter);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid request type");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Interlocked.Decrement(ref numberOfTasks);
            taskStopped.Set();
        }

        private async Task ListFilesAsync(string path, StreamWriter streamWriter)
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                await streamWriter.WriteAsync("-1");
                return;
            }

            var subDirectories = directory.GetDirectories();
            var files = directory.GetFiles();
            long numOfFilesAndDirs = files.Length + subDirectories.Length;
            await streamWriter.WriteAsync($"{numOfFilesAndDirs}");
            foreach (var subDir in subDirectories)
            {
                await streamWriter.WriteAsync($" {subDir.Name} true");
            }

            foreach (var file in files)
            {
                await streamWriter.WriteAsync($" {file.Name} false");
            }
        }

        private async Task DownloadFileAsync(string path, StreamWriter streamWriter)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                await streamWriter.WriteAsync("-1");
                return;
            }

            await streamWriter.WriteAsync($"{file.Length}");
            using var fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(streamWriter.BaseStream);
        }
    }
}
