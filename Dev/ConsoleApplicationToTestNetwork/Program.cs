using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Network;

namespace ConsoleApplicationToTestNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://sandbox-dev2:3142";  //"http://sandbox-1:3142"; //"http://localHost:3142/";
            List<string> timings = new List<string>();
            float incrementingFactor = 1.125f;
            int roundsPerPacketStep = 10,
                numberOfGuidsPerChunk = 1 << 5,
                maxMessageSize = 1 << 24;

            List<double> timingsPerRound = new List<double>();
            StringBuilder sb = new StringBuilder(maxMessageSize);
            Enumerable.Range(0, numberOfGuidsPerChunk).ToList().ForEach(x => sb.Append(Guid.NewGuid()));
            string chunkOfGuids = sb.ToString();
            sb.Append(chunkOfGuids);

            string outFileName = string.Format(@"C:\temp\networkSpeedTo{0}.csv", uri.Replace(':', '-').Replace('/', '_')); 

            // header
            timings.Add(string.Format("URI:, '{0}'", uri));
            timings.Add(string.Format("Max message size:, {0}, Initial block size:, {1}, Incrementing factor:, {2}, Rounds per packet size:, {3}", maxMessageSize, numberOfGuidsPerChunk, incrementingFactor, roundsPerPacketStep));

            using (var serverProxy = new ServerProxy(new Uri(uri)))
            {
                serverProxy.Connect(Guid.NewGuid());
                CommunicationControllerFactory cf = new CommunicationControllerFactory();
                while (sb.Length < maxMessageSize)
                {
                    timingsPerRound.Clear();
                    for (int i = 0; i < roundsPerPacketStep; i++)
                    {
                        var controller = cf.CreateController("TestNetworkService");
                        controller.AddPayloadArgument("payload", sb);
                        DateTime start = DateTime.Now;
                        // send very large message
                        var svc = controller.ExecuteCommand<ExecuteMessage>(serverProxy, Guid.NewGuid());

                        TimeSpan elapsed = DateTime.Now - start;
                        timingsPerRound.Add(elapsed.TotalMilliseconds);

                        // give the server time to clear it's queue 
                        Thread.Sleep((int)Math.Round(elapsed.TotalMilliseconds) * 2);
                    }
                    string toAdd = string.Format("{0}, {1}", sb.Length, timingsPerRound.Sum() / roundsPerPacketStep);
                    Console.WriteLine(toAdd);
                    timings.Add(toAdd);
                    // build new packet that is incrementingFactor bigger than previous
                    StringBuilder tmpSb = new StringBuilder();
                    tmpSb.Append(sb);
                    Enumerable.Range(1, (int)Math.Ceiling(incrementingFactor)).ToList().ForEach(x =>
                    tmpSb.Append(tmpSb.ToString()));
                    sb.Append(tmpSb.ToString().Substring(0,
                        (int)((tmpSb.Length - sb.Length) * (incrementingFactor - 1))));
                }
            }
            File.WriteAllLines(outFileName, timings.ToArray());
            //Console.ReadKey();
        }
    }
}
