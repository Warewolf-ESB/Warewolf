using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfTfsUtils;

namespace WarewolfTfsUtilsTest
{
    [TestClass]
    public class WorkspaceTest
    {
        [TestMethod]
        public void CanFetchTfsSolutionFromWorkspace()
        {

            string server = "http://rsaklfsvrgendev:8080/tfs/";
            string project = "$/DEV2 SCRUM Project/WarewolfInstaller-SharpSetup";

            WarewolfWorkspace wws = new WarewolfWorkspace();

            var result = wws.FetchWorkspace(server, project, "Release Engineering WS", @"F:\release_engineering_ws","IntegrationTester", "I73573r0");

            Assert.AreEqual("<TfsStatusMsg>Ok</TfsStatusMsg>", result);

        }
    }
}
