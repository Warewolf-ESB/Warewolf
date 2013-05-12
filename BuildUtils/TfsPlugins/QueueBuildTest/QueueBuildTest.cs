using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueueBuild;

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
            WaitForBuild.

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Async Integration Run - Travis";

            int id = bq.Run(server, project, def);

            Assert.IsTrue(id > 0, "ID is not valid");

        }
    }
}
