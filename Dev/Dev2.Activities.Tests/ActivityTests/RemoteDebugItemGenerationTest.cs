using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Runtime.ESB.Execution;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RemoteDebugItemGenerationTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class RemoteDebugItemGenerationTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanGenerateRemoteDebugItems()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);
            Assert.AreEqual(1, msgs.Count);
        }

        [TestMethod]
        public void CanSerializeRemoteDebugItems()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);

            var tmp = JsonConvert.SerializeObject(msgs);

            var tmp2 = JsonConvert.DeserializeObject<IList<DebugState>>(tmp);
            
            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(1, tmp2.Count);
        }

        [TestMethod]
        public void CanFetchRemoteDebugItemsViaSystemService()
        {
            DsfCountRecordsetActivity act = new DsfCountRecordsetActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = (obj as IDSFDataObject);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            
            FetchRemoteDebugMessages frm = new FetchRemoteDebugMessages();

            IDictionary<string, string> d = new Dictionary<string, string>();
            d["InvokerID"] = id.ToString();

            var str = frm.Execute(d,null);

            var tmp2 = JsonConvert.DeserializeObject<IList<DebugState>>(str);

            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(1, tmp2.Count);
        }

        [TestMethod]
        public void CanParseRemoteItemsBackIntoSaneObjects()
        {
            const string data = @"<DataList><InvokerID>de952cfe-44af-4030-b6cb-42044c7ea43f</InvokerID><Dev2System.ManagmentServicePayload>[{""ID"":""7d99684d-eea1-4f53-a41b-721c5404d8a6"",""ParentID"":""00000000-0000-0000-0000-000000000000"",""ServerID"":""51a58300-7e9d-4927-a57b-e5d700b11b55"",""StateType"":128,""DisplayName"":""Assign (1)"",""HasError"":false,""ErrorMessage"":"""",""Version"":"""",""Name"":""Assign"",""ActivityType"":1,""Duration"":""00:00:04.3020000"",""StartTime"":""2013-06-07T18:36:29.4806803+02:00"",""EndTime"":""2013-06-07T18:36:33.7826803+02:00"",""Inputs"":[],""Outputs"":[],""Server"":"""",""WorkspaceID"":""00000000-0000-0000-0000-000000000000"",""OriginalInstanceID"":""14cfb456-eb67-44cf-82e7-de56546aae26"",""OriginatingResourceID"":""66ed47bb-6c19-4ac6-b397-1e272b755b9c"",""IsSimulation"":false,""Message"":null}]</Dev2System.ManagmentServicePayload></DataList>";

            var items = RemoteDebugItemParser.ParseItems(data);

            Assert.AreEqual(1, items.Count);
        }
    }
}
