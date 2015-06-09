using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Runtime.WebServer.Hubs;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Network;

namespace ConsoleApplicationToTestNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://localHost:3142";  //"http://sandbox-1:3142"; //"http://localHost:3142/";
            List<string> timings = new List<string>();
            float incrementingFactor = 1.125f;
            int maxNumberOfGuidChunks = 1 << 12,
                numberOfGuidsPerChunk = 1 << 5,
                maxMessageSize = 1 << 24;
            StringBuilder sb = new StringBuilder(maxMessageSize);
            Enumerable.Range(0, numberOfGuidsPerChunk).ToList().ForEach(x => sb.Append(Guid.NewGuid()));
            string chunkOfGuids = sb.ToString();
            sb.Append(chunkOfGuids);

            string outFileName = string.Format(@"C:\temp\networkSpeedTo{0}.csv", uri.Replace(':', '-').Replace('/', '_')); ;

            // header
            timings.Add(string.Format("URI:, '{0}'", uri));
            timings.Add(string.Format("Max message size:, {0}, Initial block size:, {1}, Incrementing factor:, {2}", maxMessageSize, numberOfGuidsPerChunk, incrementingFactor));

            using (var serverProxy = new ServerProxy(new Uri(uri)))
            {
                serverProxy.Connect(Guid.NewGuid());
                CommunicationControllerFactory cf = new CommunicationControllerFactory();
                while (sb.Length < maxMessageSize)
                {
                    var controller = cf.CreateController("TestNetworkService");
                    controller.AddPayloadArgument("payload", sb);
                    DateTime start = DateTime.Now;
                    // send very large message
                    var svc = controller.ExecuteCommand<ExecuteMessage>(serverProxy, Guid.NewGuid());

                    TimeSpan elapsed = DateTime.Now - start;
                    string toAdd = string.Format("{0}, {1}", sb.Length, elapsed.TotalMilliseconds);
                    Console.WriteLine(toAdd);
                    timings.Add(toAdd);
                    // give the server time to clear it's queue 
                    System.Threading.Thread.Sleep((int)Math.Round(elapsed.TotalMilliseconds) * 2);
                    StringBuilder tmpSb = new StringBuilder();
                    tmpSb.Append(sb.ToString());
                    Enumerable.Range(1, (int)Math.Ceiling(incrementingFactor)).ToList().ForEach(x =>
                    tmpSb.Append(tmpSb.ToString()));
                    sb.Append(tmpSb.ToString().Substring(0,
                        (int)((tmpSb.Length - sb.Length) * (incrementingFactor - 1))));
                }
            }
            System.IO.File.WriteAllLines(outFileName, timings.ToArray());
            //Console.ReadKey();
        }
    }
}
