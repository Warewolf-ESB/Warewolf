using Microsoft.VisualStudio.TestTools.UITest.Extension;
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

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            ExplorerUIMap.ClickServerInServerDDL(LocalServerName);
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        // Bug 8816
        [TestMethod]
        public void IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled()
        {
            // Click the Deploy button in the Ribbon
            RibbonUIMap.ClickRibbonMenuItem("Deploy");
            Playback.Wait(2000);

            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy");

            // Make sure the Deploy button is disabled
            Assert.IsFalse(DeployUIMap.IsDeployButtonEnabled(deployTab));

            // Connect to a Destination Server
            DeployUIMap.ChooseDestinationServer(deployTab, "_localhost");

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
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Set ourself as the destination server
            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy");
            DeployUIMap.ChooseDestinationServer(deployTab, "_localhost");

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
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "PluginsReturningXMLFromComplexType");
            var theTab = TabManagerUIMap.GetActiveTab();

            //wait for resource tree to load
            Playback.Wait(10000);

            // Assert All Service Types Visible
            var sourceResources = DeployUIMap.GetSourceNavigationTree();
            var environmentNode = (sourceResources.Nodes[0] as WpfTreeItem);
            Assert.IsNotNull(environmentNode, "Nothing in the deploy source resource navigation tree.");
            Assert.AreEqual("UI_SourceServer_WORKFLOWS_AutoID", environmentNode.Nodes[0].FriendlyName, "First service type is not workflows");
            Assert.AreEqual("UI_SourceServer_SERVICES_AutoID", environmentNode.Nodes[1].FriendlyName, "Second service type is not services");
            Assert.AreEqual("UI_SourceServer_SOURCES_AutoID", environmentNode.Nodes[2].FriendlyName, "Third service type is not sources");

            DeployUIMap.EnterTextInSourceServerFilterBox(theTab, "ldnslgnsdg"); // Random text
            Playback.Wait(1500);
            var result = DeployUIMap.DoesSourceServerHaveDeployItems(theTab);

            TabManagerUIMap.CloseTab("Deploy");

            if(!result)
            {
                Assert.Fail("The deployed item has been removed with the filter - It should not be");
            }
        }
    }
}
