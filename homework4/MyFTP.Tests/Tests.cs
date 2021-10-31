using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MyFTP.Tests
{
    public class Tests
    {
        private static readonly IPEndPoint endPoint;
        private static readonly Server server;
        private static readonly Client client;

        static Tests()
        {
            endPoint = new (IPAddress.Parse("127.0.0.1"), 8000);
            server = new(endPoint);
            client = new(endPoint);
        }

        private static IEnumerable<(string, List<(string, bool)>)> ListFilesRequests()
        {
            yield return ("../../../..",
                new List<(string, bool)>() 
                {
                    (".vs", true),
                    ("MyFTP", true),
                    ("MyFTP.Tests", true),
                    (".editorconfig", false),
                    ("MyFTP.sln", false)
                });

            yield return ("../../../../MyFTP",
                new List<(string, bool)>()
                {
                    ("bin", true),
                    ("obj", true),
                    ("Client.cs", false),
                    ("MyFTP.csproj", false),
                    ("Program.cs", false),
                    ("Server.cs", false)
                });
        }

        [TestCaseSource(nameof(ListFilesRequests))]
        public async Task TestCallListAllFiles((string path, List<(string, bool)> expectedResponse) data)
        {
            server.Run();
            var response = await client.ListFilesAsync(data.path);
            CollectionAssert.AreEqual(data.expectedResponse, response);
            server.Stop();
        }

        [Test]
        public async Task TestCallDownloadFile()
        {
            server.Run();
            var path = "../../../../MyFTP.Tests/Example.Txt";
            var (size, content) = await client.DownloadFileAsync(path);
            Assert.AreEqual(11, size);
            Assert.AreEqual("???sometext", Encoding.ASCII.GetString(content));
            server.Stop();
        }

        private static IEnumerable<Func<string, Task>> ClientMethods()
        {
            yield return async (path) => await client.DownloadFileAsync(path);
            yield return async (path) => await client.ListFilesAsync(path);
        }

        public static IEnumerable<(Func<Task>, string)> InvalidPassesAndExceptions()
        {
            foreach (var func in ClientMethods())
            {
                yield return (() => func(null), nameof(ArgumentNullException));
                yield return (() => func("../non"), nameof(ArgumentException));
            }            
        }

        [TestCaseSource(nameof(InvalidPassesAndExceptions))]
        public void TestTryPassToClientMethodsInvalidPaths((Func<Task> func, string expectedExceptionType) data)
        {
            server.Run();
            try
            {
                data.func().Wait();
                Assert.Fail("Exception should be thrown.");
            }
            catch (AggregateException exception)
            {
                var innerException = exception.InnerException;
                Assert.AreEqual(
                    data.expectedExceptionType,
                    innerException.GetType().Name);
            }
            
            server.Stop();
        }

        public async Task TestTryBreakProtocolAndGetNotification()
        {
            server.Run();
            using var client = new TcpClient();
            client.Connect(endPoint);
            using var stream = client.GetStream();
            using var streamWriter = new StreamWriter(stream);
            using var streamReader = new StreamReader(stream);
            var incorrectRequests = new string[]
            {
                "bla",
                "1 ",
                "2\npath"
            };
            foreach (var request in incorrectRequests)
            {
                streamWriter.WriteLine(request);
                var response = streamReader.ReadToEnd();
                Assert.AreEqual("Protocol is broken", response);
            }

            server.Stop();
        }
    }
}