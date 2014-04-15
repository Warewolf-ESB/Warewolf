using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass]
    public class DsfActivityAbstractTest
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;
        // Created by: Michael
        // For: Bug 7840
        [TestMethod]
        public void LastRecordSetNotionUpdatesEntry_Expected_RecordsCreated()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "LastRecordSetNotationUpdatesEntry");
            string expected = @"<nameSetindex=""1""><Name>Michael</Name><Surname>Cullen</Surname></nameSet>
                                <nameSetindex=""2""><Name>Massimo</Name><Surname>Guerrera</Surname></nameSet>
                                <nameSetindex=""3""><Name>MASSIMO</Name><Surname></Surname></nameSet>
                                <nameSetindex=""4""><Name>0x4d415353494d4f</Name><Surname></Surname></nameSet>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Standardize the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);
            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_DispatchDebugOutPut")]
        public void DsfActivity_MethodName_Scenerio_Result()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "LastRecordSetNotationUpdatesEntry");
            string expected = @"<nameSetindex=""1""><Name>Michael</Name><Surname>Cullen</Surname></nameSet>
                                <nameSetindex=""2""><Name>Massimo</Name><Surname>Guerrera</Surname></nameSet>
                                <nameSetindex=""3""><Name>MASSIMO</Name><Surname></Surname></nameSet>
                                <nameSetindex=""4""><Name>0x4d415353494d4f</Name><Surname></Surname></nameSet>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            // Standardize the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);
            StringAssert.Contains(ResponseData, expected);
        }

    }
}
