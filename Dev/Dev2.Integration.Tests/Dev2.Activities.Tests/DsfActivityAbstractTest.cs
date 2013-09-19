using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class DsfActivityAbstractTest
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;
        // Created by: Michael
        // For: Bug 7840
        [TestMethod]
        public void LastRecordSetNotionUpdatesEntry_Expected_RecordsCreated()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "LastRecordSetNotationUpdatesEntry");
            string expected = @"<nameSet><Name>Michael</Name><Surname>Cullen</Surname></nameSet>
                                <nameSet><Name>Massimo</Name><Surname>Guerrera</Surname></nameSet>
                                <nameSet><Name>MASSIMO</Name><Surname></Surname></nameSet>
                                <nameSet><Name>0x4d415353494d4f</Name><Surname></Surname></nameSet>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Standardise the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);
            StringAssert.Contains(ResponseData, expected);
        }
    }
}
