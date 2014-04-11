using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfGatherSystemInformationActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void GatherSystemInfomation_ExcuteActivityTwice_SameVariableNamesUsedForInputs_VariablesComesOutCorrectlyOnDebug()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "11330_Integration tests");

            Guid id = Guid.NewGuid();
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            Assert.AreEqual(3, debugItems.Count);
            Assert.AreEqual("[[date]]", debugItems[0].Outputs[0].ResultsList[1].Variable);
            Assert.AreEqual("[[date]]", debugItems[1].Outputs[0].ResultsList[1].Variable);
            Assert.AreEqual("[[cpu]]", debugItems[0].Outputs[1].ResultsList[1].Variable);
            Assert.AreEqual("[[cpu]]", debugItems[1].Outputs[1].ResultsList[1].Variable);
        }
    }
}
