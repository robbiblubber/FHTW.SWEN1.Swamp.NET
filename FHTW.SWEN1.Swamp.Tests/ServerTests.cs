using Moq;
using NUnit.Framework;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;



namespace FHTW.SWEN1.Swamp.Tests
{
    public class ServerTests
    {
        /// <summary>Performs test setup.</summary>
        [SetUp]
        public void Setup()
        {
            Program.InitDb();
        }


        /// <summary>Tests core implementation in event handler _Svr_Incoming().</summary>
        [Test]
        public void TestBaseFunc()
        {
            Mock<HttpSvrEventArgs> args = new Mock<HttpSvrEventArgs>();
            args.SetupGet(m => m.Method).Returns("GET");
            args.SetupGet(m => m.Path).Returns("/messages/1");

            Program._Svr_Incoming(args.Object);

            args.Verify(m => m.Reply(200, "[1] Hallo\r\n"), Times.Once());
        }


        /// <summary>Tests event argument class HttpSvrEventArgs.</summary>
        [Test]
        public void TestEventArgs()
        {
            HttpSvrEventArgs args = new HttpSvrEventArgs("GET /messages/1 HTTP/1.1\r\nHost: localhost:12000\r\nUser-Agent: curl/7.83.1\r\nAccept: */*\r\nContent-Type: text/plain\r\nContent-Length: 0", null);

            Assert.That(args.Method, Is.EqualTo("GET"));
            Assert.That(args.Path, Is.EqualTo("/messages/1"));
        }


        /// <summary>Tests the Readmessages() method.</summary>
        [Test]
        public void TestReadMessages()
        {
            MethodInfo m = typeof(Program).GetMethod("_ReadMessages", BindingFlags.NonPublic | BindingFlags.Static);

            string result = (string) m.Invoke(null, new object[] { 1 });
            Assert.That(result, Is.EqualTo("[1] Hallo\r\n"));
        }


        /// <summary>Tests the main server (end to end).</summary>
        [Test]
        public void TestServer()
        {
            HttpSvr svr = new HttpSvr();

            ThreadPool.QueueUserWorkItem(m =>
                {
                    svr.Incoming += Program._Svr_Incoming;

                    svr.Run();
                });

            HttpClient c = new HttpClient();

            Task<Stream> ts = c.GetStreamAsync("http://localhost:12000/messages/1");
            Stream s = ts.Result;

            StreamReader re = new StreamReader(s);
            string result = re.ReadToEnd();
            re.Close();
            re.Dispose();
            s.Close();
            s.Dispose();

            c.Dispose();
            svr.Active = false;

            Assert.That(result, Is.EqualTo("[1] Hallo\r\n"));
        }
    }
}