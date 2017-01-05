/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RemoteDebugItemGenerationTest
    /// </summary>
    [TestClass]
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
            DsfCountRecordsetNullHandlerActivity act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            act.ResourceCatalog = cat.Object;
            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = obj as IDSFDataObject;
            Guid id;
            Assert.IsNotNull(dObj);
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            // remove test datalist ;)
            Assert.AreEqual(1, msgs.Count);
        }

        [TestMethod]
        public void CanSerializeRemoteDebugItems()
        {
            DsfCountRecordsetNullHandlerActivity act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            act.ResourceCatalog = cat.Object;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = obj as IDSFDataObject;
            Assert.IsNotNull(dObj);
            Guid id;
            Guid.TryParse(dObj.RemoteInvokerID, out id);
            var msgs = RemoteDebugMessageRepo.Instance.FetchDebugItems(id);
            var serialiser = new Dev2JsonSerializer();
            var tmp = serialiser.SerializeToBuilder(msgs);

            var tmp2 = serialiser.Deserialize<IList<DebugState>>(tmp);

            // remove test datalist ;)

            Assert.AreEqual(1, tmp2.Count);
            Assert.AreEqual("bob",tmp2[0].Server);
        }

        [TestMethod]
        public void CanFetchRemoteDebugItemsViaSystemService()
        {
            DsfCountRecordsetNullHandlerActivity act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = "[[Customers()]]", CountNumber = "[[res]]" };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            act.ResourceCatalog = cat.Object;
            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var obj = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes, true);

            IDSFDataObject dObj = obj as IDSFDataObject;
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

            Assert.AreEqual(1, tmp2.Count);
        }

    }
}
