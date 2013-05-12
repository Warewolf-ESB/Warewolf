using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueueBuild;
using WaitForBuild;

namespace QueueBuildTest
{
    [TestClass]
    public class QueueBuildTest
    {
        [TestMethod]
        public void CanQueueBuildWithValidData()
        {
            BuildQueuer bq = new BuildQueuer();

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Async Integration Run - Travis";

            int id = bq.Run(server, project, def);

            Assert.IsTrue(id > 0, "ID is not valid");

        }

        [TestMethod]
        public void CanWatchBuild()
        {

            BuildQueuer bq = new BuildQueuer();

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Branch Gated Check-in - Dev Nightly";

            int id = bq.Run(server, project, def);

            int res = WaitForProgram.Main(new string[]{server, project, "20793"});


            Assert.IsTrue(res == 0, "Failed to watch build or bad parms exit code { " + id + " }");
        }
    }
}
