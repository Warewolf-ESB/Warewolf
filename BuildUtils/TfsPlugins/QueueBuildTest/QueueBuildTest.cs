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
            string def = "Async Integration Run - Dev Merge";
            string set = "Gated_2013-08-05_01.51.52.5566";
            string user = "Ashley Lewis";
            string agentTag = "someagenttag";

            int id = bq.Run(server, project, def, set, user, agentTag);

            Assert.IsTrue(id > 0, "ID is not valid");
        }

        [TestMethod]
        public void CanWatchBuild()
        {
            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";

            int res = WaitForProgram.Main(new string[]{server, project, "20803"});


            Assert.IsTrue(res == 0, "Failed to watch build or bad parms exit code { " + res + " }");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BuildQueuer_QueueBuild")]
        public void BuildQueuer_QueueBuild_ChangeSetID_BuildQueued()
        {
            var buildQueuer = new QueueBuildPlugin.BuildQueuer();
            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Async Integration Run - Dev Merge";
            string user = "Ashley Lewis";
            string changeSetID = "17942";

            //------------Execute Test---------------------------
            int id = buildQueuer.QueueBuild(server, project, def, string.Empty, user, changeSetID);

            // Assert Build Queued
            Assert.IsTrue(id > 0, "ID is not valid");
        }
    }
}
