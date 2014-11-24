using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class TimeZoneChanges
    {
        private readonly string _webServerUri = ServerSettings.WebserverURI;
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GlobalConstants_TimezoneChanges")]
        // ReSharper disable once InconsistentNaming
        public void GlobalConstants_TimezoneChanges_Change_ExpectDatetime_Now_Change()
        {
            try
            {


                // ReSharper disable once UnusedVariable
                var a = GlobalConstants.NullDataListID;
                ChangeTimeZone("South Africa Standard Time");
                var reponseDataCurrent = ReponseData();
                ChangeTimeZone("Pacific Standard Time");
                Thread.Sleep(10000);
                var reponseDataChanged = ReponseData();
                XDocument my = XDocument.Parse(reponseDataCurrent);
                XDocument their = XDocument.Parse(reponseDataChanged);
                var theirHour = DateTime.Parse( their.Descendants().First(x => x.Name == "a").Value);
                var myHour = DateTime.Parse(my.Descendants().First(x => x.Name == "a").Value);
                Assert.IsTrue( Math.Abs(theirHour.Subtract(myHour).Hours) >1);

            }
            finally
            {

                ChangeTimeZone("South Africa Standard Time");
            }
        }

        string ReponseData()
        {
            const string ServiceName = "Acceptance Testing Resources/NowTimezoneChanges";

            string reponseData = TestHelper.PostDataToWebserver(string.Format("{0}{1}", _webServerUri, ServiceName));
            return reponseData;
        }

        private void ChangeTimeZone(string timeZone)
        {
            ProcessStartInfo p = new ProcessStartInfo("tzutil" , String.Format("/s \"{0}\"",timeZone)) { UseShellExecute = false };
            var process = Process.Start(p);
            if(process != null)
            {
                process.WaitForExit();
            }

        }
    }
}
