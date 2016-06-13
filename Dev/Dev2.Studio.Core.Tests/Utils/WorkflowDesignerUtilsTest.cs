
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Utils
{
    /// <summary>
    /// Summary description for WorkflowDesignerUtilsTest
    /// </summary>
    [TestClass]
    public class WorkflowDesignerUtilsTest
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
        public void CanFormatDsfActivityFieldHandleSpecialCharsWithNoException()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

            IList<string> result = wdu.FormatDsfActivityField(TestResourceStringsTest.SpecialChars);

            Assert.AreEqual(0, result.Count, "Strange behaviors parsing special chars, I got results when I should not?!");
        }

        [TestMethod]
        public void CanFormatDsfActivityFieldHandleNormalParse()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

            IList<string> result = wdu.FormatDsfActivityField("[[MoIsNotUber]]");

            Assert.AreEqual(1, result.Count, "Strange behaviors parsing normal regions, I was expecting 1 result");
        }

        //2013.06.10: Ashley Lewis for bug 9306 - Format DsfActivity handles mismatched region braces better
        [TestMethod]
        public void CanFormatDsfActivityFieldWithMissmatchedRegionBracesExpectedNotParsed()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

            IList<string> result = wdu.FormatDsfActivityField("[[MoIsNotUber([[invalid).field]]");

            Assert.AreEqual(0, result.Count, "Format DsfActivity returned results when the region braces where missmatched");
        }

        [TestMethod]
        public void EditResource_UnitTest_EditResourceWhereWorkflow_ExpectAddWorksurfaceMessageHandled()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ResourceType).Returns(ResourceType.WorkflowService);
            var eventAggregator = new EventAggregator();
            var handleMessages = new TestHandleMessages();
            eventAggregator.Subscribe(handleMessages);
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object, eventAggregator);
            //------------Assert Results-------------------------
            Assert.IsTrue(handleMessages.WorkSurfaceMessageCalled);
            Assert.IsFalse(handleMessages.EditResourceMessageCalled);
        }

        [TestMethod]
        public void EditResource_UnitTest_EditResourceWhereService_ExpectShowEditResourceWizardMessageHandled()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ResourceType).Returns(ResourceType.Service);
            var eventAggregator = new EventAggregator();
            var handleMessages = new TestHandleMessages();
            eventAggregator.Subscribe(handleMessages);
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object, eventAggregator);
            //------------Assert Results-------------------------
            Assert.IsTrue(handleMessages.EditResourceMessageCalled);
            Assert.IsFalse(handleMessages.WorkSurfaceMessageCalled);
        }

        [TestMethod]
        public void EditResource_UnitTest_EditResourceWhereSource_ExpectShowEditResourceWizardMessageHandled()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ResourceType).Returns(ResourceType.Source);
            var eventAggregator = new EventAggregator();
            var handleMessages = new TestHandleMessages();
            eventAggregator.Subscribe(handleMessages);
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object, eventAggregator);
            //------------Assert Results-------------------------
            Assert.IsTrue(handleMessages.EditResourceMessageCalled);
            Assert.IsFalse(handleMessages.WorkSurfaceMessageCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EditResource_UnitTest_EditResourceWhereNullEventAggregator_ExpectException()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ResourceType).Returns(ResourceType.Service);
            var eventAggregator = new EventAggregator();
            var handleMessages = new TestHandleMessages();
            eventAggregator.Subscribe(handleMessages);
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object, null);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Leon rajindrapersadh")]
        [TestCategory("WorkflowDesignerUtils_OnClick")]
        public void WorkflowDesignerUtils_CheckIfRemoteWorkflowAndSetProperties_ServerName_SetAsSource()
        {

            var x = new Mock<IContextualResourceModel>();
            var mockEnv = new Mock<IEnvironmentModel>();
            var mockCon = new Mock<IEnvironmentConnection>();
            mockEnv.Setup(a => a.Connection).Returns(mockCon.Object);
            mockCon.Setup(a => a.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            var envGuid = Guid.NewGuid();

            x.Setup(a => a.Environment).Returns(mockEnv.Object);
            mockEnv.Setup(a => a.ID).Returns(envGuid);
            x.Setup(a => a.Environment).Returns(mockEnv.Object);
            var act = new DsfActivity("a", "b", "c", "d", "e", "f");
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(act, x.Object, mockEnv.Object);
            Assert.AreEqual("rsaklf", act.FriendlySourceName.Expression.ToString());

        }





        [TestMethod]
        [Owner("Leon rajindrapersadh")]
        [TestCategory("WorkflowDesignerUtils_OnClick")]
        public void WorkflowDesignerUtils_CheckIfRemoteWorkflowAndSetProperties_DsfDateTime_DateTimeExampleShown()
        {

            var x = new Mock<IContextualResourceModel>();
            var mockEnv = new Mock<IEnvironmentModel>();
            var mockEnvRes = new Mock<IEnvironmentModel>();
            var mockCon = new Mock<IEnvironmentConnection>();
            var mockConRes = new Mock<IEnvironmentConnection>();
            mockEnv.Setup(a => a.Connection).Returns(mockCon.Object);
            mockEnvRes.Setup(a => a.Connection).Returns(mockConRes.Object);
            mockCon.Setup(a => a.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            var envGuid = Guid.NewGuid();
            var envGuidRes = Guid.NewGuid();
            x.Setup(a => a.Environment).Returns(mockEnvRes.Object);
            mockEnv.Setup(a => a.ID).Returns(envGuid);
            mockEnv.Setup(a => a.ID).Returns(envGuidRes);
            x.Setup(a => a.Environment).Returns(mockEnv.Object);
            var act = new DsfActivity("a", "b", "c", "d", "e", "f");
            //------------Execute Test---------------------------
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(act, x.Object, mockEnvRes.Object);
            Assert.AreEqual("http://rsaklf/bob", act.ServiceUri);

        }
    }

    internal class TestHandleMessages : IHandle<AddWorkSurfaceMessage>, IHandle<ShowEditResourceWizardMessage>
    {
        #region Implementation of IHandle<AddWorkSurfaceMessage>

        public void Handle(AddWorkSurfaceMessage message)
        {
            WorkSurfaceMessageCalled = true;
        }

        public bool WorkSurfaceMessageCalled { get; set; }

        #endregion

        #region Implementation of IHandle<ShowEditResourceWizardMessage>

        public void Handle(ShowEditResourceWizardMessage message)
        {
            EditResourceMessageCalled = true;
        }

        public bool EditResourceMessageCalled { get; set; }

        #endregion
    }
}
