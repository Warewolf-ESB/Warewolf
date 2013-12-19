using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueueGatedBuild;
using WaitForBuild;

namespace QueueBuildTest
{
    [TestClass]
    public class QueueBuildTest
    {
        [TestMethod]
        public void CanQueueBuildWithValidData()
        {
            QueueGatedBuild.BuildQueuer bq = new QueueGatedBuild.BuildQueuer();

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Dev2 Min Test Run";
            string set = "some shelveset";
            string user = "Ashley Lewis";

            int id = bq.Run(server, project, def, set, user);

            Assert.IsTrue(id > 0, "ID is not valid");
        }

        [TestMethod]
        public void CanQueueBuildWithValidCIData()
        {
            QueueCIBuild.BuildQueuer bq = new QueueCIBuild.BuildQueuer();

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Dev2 Min Test Run";
            string user = "Ashley Lewis";
            string changesetID = "12345";

            int id = bq.Run(server, project, def, user, changesetID);

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
            var buildQueuer = new QueueCIBuildPlugin.BuildQueuer();
            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "DEV2 SCRUM Project";
            string def = "Async Integration Run - Dev Merge";
            string user = "Ashley Lewis";
            string changeSetID = "17942";

            //------------Execute Test---------------------------
            int id = buildQueuer.QueueBuild(server, project, def, user, changeSetID);

            // Assert Build Queued
            Assert.IsTrue(id > 0, "ID is not valid");
        }
    }
}
