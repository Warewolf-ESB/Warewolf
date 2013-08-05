using System;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.EmailSourceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.UIMaps
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    [Ignore]
    public class Wizards : UIMapBase
    {

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
        private TestContext testContextInstance;

        #region Deprecated Test

        //2013.06.22: Ashley Lewis for bug 9478
        [TestMethod]
        [Ignore] // Account ban!
        public void EmailSourceWizardCreateNewSourceExpectedSourceCreated()
        {
            //Initialization
            var sourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var name = "codeduitest" + sourceName;

            EmailSourceWizardUIMap.InitializeFullTestSource(name);

            //Assert
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", name));

            new TestBase().DoCleanup("Unsaved 1", true);
        }

        [TestMethod]
        [Ignore]
        public void WebServiceWizardCreateServiceAndSourceExpectedServiceCreated()
        {
            //Initialization
            var sourceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceName = "codeduitest" + sourceNameId;
            var serviceNameId = Guid.NewGuid().ToString().Substring(0, 5);
            var serviceName = "codeduitest" + serviceNameId;

            WebServiceWizardUIMap.InitializeFullTestServiceAndSource(serviceName, sourceName);

            //Assert
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SERVICES", "Unassigned", serviceName));
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(sourceName);
            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", "Unassigned", sourceName));
        }

        #endregion

        //2013.03.14: Ashley Lewis - Bug 9217
        [TestMethod]
        public void DatabaseServiceWizardCreateNewServiceExpectedServiceCreated()
        {
            //Initialization
            var serverSourceCategoryName = Guid.NewGuid().ToString().Substring(0, 5);
            var serverSourceName = Guid.NewGuid().ToString().Substring(0, 5);
            var cat = "CODEDUITESTS" + serverSourceCategoryName;
            var name = "codeduitest" + serverSourceName;

            DatabaseServiceWizardUIMap.InitializeFullTestServiceAndSource(cat, name);

            //Assert
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(name);

            Assert.IsTrue(ExplorerUIMap.ValidateServiceExists("localhost", "SOURCES", cat, name));
        }
    }
}
