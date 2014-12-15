
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ActivityUnitTests;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RemoteDebugItemGenerationTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            Assert.IsNotNull(dObj);
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);
            Assert.AreEqual(2, msgs.Count);
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
            Assert.IsNotNull(dObj);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            var serialiser = new Dev2JsonSerializer();
            var tmp = serialiser.SerializeToBuilder(msgs);

            var tmp2 = serialiser.Deserialize<IList<DebugState>>(tmp);

            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(2, tmp2.Count);
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
            Assert.IsNotNull(dObj);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);

            FetchRemoteDebugMessages frm = new FetchRemoteDebugMessages();

            Dictionary<string, StringBuilder> d = new Dictionary<string, StringBuilder>();
            d["InvokerID"] = new StringBuilder(id.ToString());

            var str = frm.Execute(d, null);
            var jsonSer = new Dev2JsonSerializer();
            var tmp2 = jsonSer.Deserialize<IList<DebugState>>(str.ToString());

            // remove test datalist ;)
            DataListRemoval(dObj.DataListID);

            Assert.AreEqual(2, tmp2.Count);
        }

    }
}
