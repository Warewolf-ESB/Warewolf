using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Integration.Tests.Interfaces;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    public class GetWorker : IWorker
    {
        private string _url { get; set; }
        private static string dataToCompareTo { get; set; }

        public void DoWork()
        {
            AsynchronousRequest async = new AsynchronousRequest();
            async.ScanSite(_url);
            dataToCompareTo = async.GetResponseData();
        }

        public GetWorker(string url)
        {
            _url = url;
        }

        public void Compare(string DataForComparison)
        {
            if (dataToCompareTo.Contains(DataForComparison))
                Console.WriteLine("Data match");
            else
                Console.WriteLine(String.Format("Data does not match\n Actual: \n{0}\n Expected: \n{1}\n", dataToCompareTo, DataForComparison));
        }

        public string GetResponseData()
        {
            return dataToCompareTo;
        }
    }
}
