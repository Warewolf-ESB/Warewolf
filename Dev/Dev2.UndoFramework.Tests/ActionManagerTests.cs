using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Undo;

// ReSharper disable CheckNamespace
namespace UndoFramework.Tests
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for ActionManagerTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ActionManagerTests
    {
        Mock<AbstractAction> mockAction = new Mock<AbstractAction>();

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
        [TestInitialize]
        public void MyTestInitialize()
        {
            mockAction.Setup(c => c.CanExecute()).Returns(true).Verifiable();
            mockAction.Setup(c => c.CanUnExecute()).Returns(true).Verifiable();
            mockAction.Setup(c => c.Execute()).Verifiable();
            mockAction.Setup(c => c.UnExecute()).Verifiable();


        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        [TestMethod]
        public void Undo_Expected_Positive()
        {
            ActionManager actManager = new ActionManager();
            actManager.RecordAction(mockAction.Object);
            actManager.Undo();
            Assert.IsTrue(actManager.CanRedo);
        }

        [TestMethod]
        public void Redo_Expected_Positive()
        {
            ActionManager actManager = new ActionManager();
            actManager.RecordAction(mockAction.Object);
            actManager.Undo();
            actManager.Redo();
            Assert.IsTrue(actManager.CanUndo);
        }

        [TestMethod]
        public void Two_Actions_One_Undo_Expected_CanUndo_true()
        {
            ActionManager actManager = new ActionManager();
            actManager.RecordAction(mockAction.Object);
            actManager.RecordAction(mockAction.Object);
            actManager.Undo();

            Assert.IsTrue(actManager.CanUndo && actManager.CanRedo);
        }

        [TestMethod]
        public void Two_Actions_Two_Undo_One_Redo_Expected_CanRedo_true()
        {
            ActionManager actManager = new ActionManager();
            actManager.RecordAction(mockAction.Object);
            actManager.RecordAction(mockAction.Object);
            actManager.Undo();
            actManager.Undo();
            actManager.Redo();
            Assert.IsTrue(actManager.CanUndo && actManager.CanRedo);
        }

        [TestMethod]
        public void RecordAction_Null_Action_Expected_No_Action()
        {
            ActionManager actManager = new ActionManager();
            actManager.RecordAction(null);
            Assert.IsTrue(!actManager.CanUndo && !actManager.CanRedo);
        }

        [TestMethod]
        public void RecordAction_ExecutingAction_Expected_No_Action()
        {
            ActionManager actManager = new ActionManager();
            actManager.ExecuteImmediatelyWithoutRecording = true;
            actManager.RecordAction(mockAction.Object);

            Assert.IsTrue(!actManager.CanUndo && !actManager.CanRedo);
        }
    }
}
