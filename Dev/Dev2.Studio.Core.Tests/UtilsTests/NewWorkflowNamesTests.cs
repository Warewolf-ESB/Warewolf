using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.UtilsTests
{
    /// <summary>
    /// Summary description for NewWorkflowNamesTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NewWorkflowNamesTests
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


        #region Complex Test

        [TestMethod]
        public void CanFunctionNormallyWithMixedAddRemoveOdd()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            int cnt = 1;

            for(int i = cnt; i < 10; i++)
            {
                string name = "Unsaved " + i;
                workflowNames.Add(name);
            }

            for(int i = 1; i < 10; i += 2)
            {
                string name = "Unsaved " + i;
                workflowNames.Remove(name);
            }

            var next = workflowNames.GetNext();

            Assert.AreEqual("Unsaved 1", next);
        }

        [TestMethod]
        public void CanFunctionNormallyWithMixedAddRemoveEven()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            int cnt = 1;

            for(int i = cnt; i < 10; i++)
            {
                string name = "Unsaved " + i;
                workflowNames.Add(name);
            }

            for(int i = 2; i < 10; i += 2)
            {
                string name = "Unsaved " + i;
                workflowNames.Remove(name);
            }

            var next = workflowNames.GetNext();

            Assert.AreEqual("Unsaved 2", next);
        }

        #endregion

        #region Add Tests

        [TestMethod]
        public void NewWorkflowNamesAddNewNameToHashSetExpectedNameToBeAdded()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";
            workflowNames.Add(name);
            Assert.IsTrue(workflowNames.Contains(name));
        }



        #endregion

        #region Remove Tests

        [TestMethod]
        public void NewWorkflowNamesRemoveExistingNameFromHashSetExpectedNameToBeRemoved()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";
            workflowNames.Add(name);
            workflowNames.Remove(name);
            Assert.IsFalse(workflowNames.Contains(name));
        }

        [TestMethod]
        public void NewWorkflowNamesRemoveNonExistingNameFromHashSetExpectedReturnOfFalse()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";

            Assert.IsFalse(workflowNames.Remove(name));
        }

        #endregion

        #region Contains Tests

        [TestMethod]
        public void NewWorkflowNamesContainsNameWhenNameExistsInHashSetExpectedReturnOfTrue()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";
            workflowNames.Add(name);
            Assert.IsTrue(workflowNames.Contains(name));
        }

        [TestMethod]
        public void NewWorkflowNamesContainsNameWhenNameDoesntExistsInHashSetExpectedReturnOfFalse()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";
            Assert.IsFalse(workflowNames.Contains(name));
        }

        #endregion

        #region GetNext Tests

        [TestMethod]
        public void NewWorkflowNamesGetNextNameWhenOneExistsInHashSetExpectedReturnNewWorkflow2()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            string name = "Unsaved 1";
            workflowNames.Add(name);

            Assert.AreEqual("Unsaved 2", workflowNames.GetNext());
        }

        [TestMethod]
        public void NewWorkflowNamesGetNextNameWhenManyExistInHashSetAndGapAtTeoExpectedReturnNewWorkflow2()
        {
            NewWorkflowNames workflowNames = new NewWorkflowNames();
            for(int i = 0; i < 5; i++)
            {
                string name = "Unsaved " + i;
                workflowNames.Add(name);
            }
            workflowNames.Remove("Unsaved 2");

            Assert.AreEqual("Unsaved 2", workflowNames.GetNext());
        }

        #endregion
    }
}
