using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Action = System.Action;

namespace Dev2.Core.Tests.Utils
{
    /// <summary>
    /// Summary description for WorkflowDesignerUtilsTest
    /// </summary>
    [TestClass]
    public class WorkflowDesignerUtilsTest
    {
        public WorkflowDesignerUtilsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object,eventAggregator);
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
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object,eventAggregator);
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
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object,eventAggregator);
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
            WorkflowDesignerUtils.EditResource(mockResourceModel.Object,null);
            //------------Assert Results-------------------------
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
