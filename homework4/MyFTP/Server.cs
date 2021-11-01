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
        private readonly IPEndPoint endPoint;
        private readonly AutoResetEvent taskStopped = new (false);
        private TcpListener listener;
        private CancellationTokenSource source;
        private int numberOfTasks = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="endPoint">Server address.</param>
        public Server(IPEndPoint endPoint)
            => this.endPoint = endPoint
            ?? throw new ArgumentNullException(nameof(endPoint));

        /// <summary>
        /// Stops server's work.
        /// </summary>
        public void Stop()
        {
            source.Cancel();
            while (numberOfTasks != 0)
            {
                taskStopped.WaitOne();
            }

            listener.Stop();
        }

        /// <summary>
        /// Runs server.
        /// </summary>
        public async Task Run()
        {
            source = new ();
            listener = new (endPoint);
            listener.Start();
            while (!source.Token.IsCancellationRequested)
            {
                using var client = await listener.AcceptTcpClientAsync();
                await Task.Run(() => HandleRequest(client));
            }
        }

        private async Task NotifyClientOfProtocolBreaking(StreamWriter streamWriter)
        {
            await streamWriter.WriteLineAsync("Protocol is broken");
            await streamWriter.FlushAsync();
        }

        private (int, string) ParseRequest(StreamReader streamReader)
        {
            int type = streamReader.Read();
            if (type != '1' && type != '2')
            {
                return (0, string.Empty);
            }

            type = type == '1' ? 1 : 2;
            int nextChar = streamReader.Read();
            if (nextChar != ' ')
            {
                return (0, string.Empty);
            }

            return (type, streamReader.ReadLine());
        }

        private async Task HandleRequest(TcpClient client)
        {
            Interlocked.Increment(ref numberOfTasks);
            try
            {
                using var stream = client.GetStream();
                using var streamReader = new StreamReader(stream);
                using var streamWriter = new StreamWriter(stream);
                var (requestType, path) = ParseRequest(streamReader);
                switch (requestType)
                {
                    case 1:
                        await ListFilesAsync(path, streamWriter);
                        break;
                    case 2:
                        await DownloadFileAsync(path, streamWriter);
                        break;
                    default:
                        await NotifyClientOfProtocolBreaking(streamWriter);
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
                await streamWriter.WriteLineAsync("-1");
                await streamWriter.FlushAsync();
                return;
            }

            await streamWriter.WriteLineAsync($"{file.Length}");
            await streamWriter.FlushAsync();
            using var fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(streamWriter.BaseStream);
        }
    }
}
