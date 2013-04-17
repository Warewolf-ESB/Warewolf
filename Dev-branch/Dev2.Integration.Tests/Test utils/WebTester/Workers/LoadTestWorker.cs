using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Dev2.Integration.Tests.Interfaces;


namespace Dev2.Integration.Tests.MEF.WebTester
{
    public class LoadTestWorker : IWorker
    {
        private List<string> _urls { get; set; }
        public StreamReader _reader { get; private set; }
        private string _filepath { get; set; }

        public LoadTestWorker()
        {
            //Get the filepath for the URLs from config
            _urls = new List<string>();
            _filepath = @"C:\Temp\Test.txt";
            ReadFromFile();
        }
        public LoadTestWorker(string filepath)
        {
            _urls = new List<string>();
            _filepath = filepath;
            ReadFromFile();
        }

        public void DoWork()
        {
            StartLoadTester();
            Console.WriteLine("Press <ENTER> to exit load test (Please Note that all threads that have started will complete)");
            Console.ReadLine();
        }

        private void ReadFromFile()
        {
            _reader = new StreamReader(_filepath);
            while (_reader.Peek() >= 0)
                _urls.Add(_reader.ReadLine());
            _reader.Close();
            _reader.Dispose();
        }

        private void StartLoadTester()
        {
            _urls.ForEach(url =>
            {
                ThreadInfo threadInfo = new ThreadInfo();
                threadInfo.url = url;
                ThreadPool.SetMaxThreads(1024, 1024);
                ThreadPool.QueueUserWorkItem(new WaitCallback(NewThread), threadInfo);
            });
            Console.WriteLine("All {0} Threads Started", _urls.Count);
        }

        private void NewThread(object threadObject)
        {
            ThreadInfo threadInfo = threadObject as ThreadInfo;
            AsynchronousRequest async = new AsynchronousRequest();
            async.ScanSiteStopWatch((string)threadInfo.url, null);
        }

        #region Thread Info Class
        public class ThreadInfo
        {
            public string url { get; set; }
        }
        #endregion
    }
}
