using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DeployUITests : UIMapBase
    {
        #region Cleanup

        const string LocalServerName = "localhost";

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            ExplorerUIMap.ClickServerInServerDDL(LocalServerName);
            TabManagerUIMap.CloseAllTabs();
            Halt();
        }
        #endregion

        // Bug 8816
        [TestMethod]
        public void IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled()
        {
            // Click the Deploy button in the Ribbon
            RibbonUIMap.ClickRibbonMenuItem("Deploy", 20000);

            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy");

            // Make sure the Deploy button is disabled
            Assert.IsFalse(DeployUIMap.IsDeployButtonEnabled(deployTab));

            // Connect to a Destination Server
            DeployUIMap.ChooseDestinationServer(deployTab, "localhost");

            var result = DeployUIMap.IsDeployButtonEnabled(deployTab);

            TabManagerUIMap.CloseTab("Deploy");

            // Make sure its still disabled, as nothing has been chosen to deploy
            Assert.IsFalse(result, "As we have not chosen anything to deploy, the Deploy button should still be disabled!");
        }

        // Bug 8819
        [TestMethod]
        public void EnterFilterOnDestinationServer_Expected_DeployedItemsStillVisible()
        {
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            ExplorerUIMap.RightClickDeployProject("localhost", "MO", "CalculateTaxReturns");

            // Set ourself as the destination server
            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy");
            DeployUIMap.ChooseDestinationServer(deployTab, "localhost");

            // Make sure the Destination server has items
            Assert.IsTrue(DeployUIMap.DoesDestinationServerHaveItems(deployTab));

            // Enter a filter in the destination server
            DeployUIMap.EnterTextInDestinationServerFilterBox(deployTab, "zzzzzzzzz");

            var result = DeployUIMap.DoesDestinationServerHaveItems(deployTab);

            TabManagerUIMap.CloseTab("Deploy");
            // And make sure it still has items
            Assert.IsTrue(result, "After a filter was applied, the destination Server lost all its items!");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("Deploy_ResourceTree")]
        public void Deploy_ResourceTree_DeployFromExplorer_AllServiceTypesVisibleAndDeployedItemNotFiltered()
        {

            ExplorerUIMap.EnterExplorerSearchText("PluginsReturningXMLFromComplexType");
            //------------Execute Test---------------------------
            ExplorerUIMap.RightClickDeployProject("localhost", "INTEGRATION TEST SERVICES", "PluginsReturningXMLFromComplexType");
            var theTab = TabManagerUIMap.GetActiveTab();

            ExplorerUIMap.WaitForResourcesToLoad();

            // Assert All Service Types Visible
            var sourceResources = DeployUIMap.GetSourceNavigationTree();
            var environmentNode = (sourceResources.Nodes[0] as WpfTreeItem);
            Assert.IsNotNull(environmentNode, "Nothing in the deploy source resource navigation tree.");

            DeployUIMap.EnterTextInSourceServerFilterBox(theTab, "ldnslgnsdg"); // Random text

            var result = DeployUIMap.DoesSourceServerHaveDeployItems(theTab);

            TabManagerUIMap.CloseTab("Deploy");

            if(!result)
            {
                Assert.Fail("The deployed item has been removed with the filter - It should not be");
            }
        }
    }
}
