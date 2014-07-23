using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TechTalk.SpecFlow;

namespace Dev2.Studio.UI.Specs.Connect_Control
{
    [Binding]
    public class ConnectControlSteps : UIMapBase
    {

        [Given(@"right click rename ""(.*)"" to ""(.*)""")]
        public void GivenRightClickRenameTo(string serverName, string newName)
        {
            ExplorerUIMap.RightClickRenameResource(serverName, "", newName);
        }
        
        [When(@"I click Test Connection")]
        public void WhenIClickTestConnection()
        {
            NewServerUIMap.ClickTestConnection();
        }

        [Then(@"Enter username as ""(.*)"" as password as ""(.*)""")]
        public void ThenEnterUsernameAsAsPasswordAs(string userName, string password)
        {
            NewServerUIMap.EnterUserName(userName);
            NewServerUIMap.EnterPassword(password);
        }
        
        [Given(@"I am connected to the server ""(.*)""")]
        public void GivenIAmConnectedToTheServer(string serverName)
        {
            ExplorerUIMap.ClickServerInServerDDL(serverName);
            var item = ExplorerUIMap.GetConnectControl("Button", "Connect");
            Mouse.Click(item);
            Playback.Wait(1000);
        }
        
        [When(@"'(.*)' should be updated with connected ""(.*)""")]
        public void WhenShouldBeUpdatedWithConnected(string p0, string p1)
        {
            //TJ-TODO:- Do we need to assert here
            // ScenarioContext.Current.Pending();
        }

        [When(@"I click on ""(.*)"" on connect control in '(.*)'")]
        public void WhenIClickOnOnConnectControlIn(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click Save Connection")]
        public void WhenIClickSaveConnection()
        {
            NewServerUIMap.ClickSave();
        }

        [When(@"I save the connection as ""(.*)""")]
        public void WhenISaveTheConnectionAs(string serverName)
        {
            NewServerUIMap.SaveNameInDialog(serverName);
        }

        [When(@"I click on ""(.*)""")]
        [When(@"I click on '(.*)'")]
        [When(@"I click on '(.*)'")]
        [Given(@"I click on '(.*)'")]
        [Given(@"I click on ""(.*)""")]
        public void WhenIClickOn(string tabName)
        {
            var tab = tabName.ToLower();
            if(tab == "deploy")
            {
                RibbonUIMap.OpenDeploy();
            }
            else if(tab == "manage settings")
            {
                RibbonUIMap.ClickManageSecuritySettings();
            }
            else if(tab == "scheduler")
            {
                RibbonUIMap.OpenScheduler();
            }
            else if(tab.Contains("setting"))
            {
                RibbonUIMap.OpenManageSettings();
            }
        }

        [When(@"I click on ""(.*)"" in ""(.*)"" conncet control")]
        [When(@"I click on ""(.*)"" in '(.*)' conncet control")]
        [When(@"I click on '(.*)' in '(.*)' connect control")]
        [When(@"I click on ""(.*)"" in '(.*)' connect control")]
        [When(@"I click on ""(.*)"" in ""(.*)"" connect control")]
        [Given(@"""(.*)"" is ""(.*)"" in ""(.*)"" connect control")]
        public void WhenIClickOnInConncetControl(string controlName, string at)
        {
            var where = at.ToLower();
            UITestControl control;

            if(where.Contains("destination"))
            {
                control = DeployUIMap.GetDestinationContolByFriendlyName(GetActiveTab(), controlName);
                Mouse.Click(control);
            }
            else if(where.Contains("source"))
            {
                control = DeployUIMap.GetSourceContolByFriendlyName(GetActiveTab(), controlName);
                Mouse.Click(control);
            }
            else if(where.Contains("explorer"))
            {
                control = ExplorerUIMap.GetConnectControl("Button", controlName);
                Mouse.Click(control);
            }
            else if(where == "scheduler")
            {
                control = SchedulerUiMap.GetConnectControl("Button", controlName);
                Mouse.Click(control);
            }
            else if(where == "manage settings" || where == "settings")
            {
                control = SecurityUiMap.GetConnectControl("Button", controlName);
                Mouse.Click(control);
            }
        }

        [When(@"""(.*)"" is ""(.*)"" in ""(.*)"" connect control")]
        [Then(@"""(.*)"" is """"(.*)"" in ""(.*)"" connect control")]
        [Then(@"""(.*)"" is ""(.*)"" in ""(.*)"" connect control")]
        [Then(@"""(.*)"" is """"(.*)"" in '(.*)' connect control")]
        [Then(@"""(.*)"" is ""(.*)"" in '(.*)' connect control")]
        [When(@"""(.*)"" is ""(.*)"" in '(.*)' connect control")]
        [When(@"'(.*)' is '(.*)' in '(.*)' connect control")]
        public void ThenIsInConnectControl(string serverName, string enabledStatus, string at)
        {
            var where = at.ToLower();
            var enabled = enabledStatus.ToLower() == "enabled";

            if(where.Contains("destination"))
            {
                var sourceEditConnectionButton = DeployUIMap.GetDestinationEditConnectionButton(GetActiveTab()) as WpfButton;
                Assert.IsNotNull(sourceEditConnectionButton);
                Assert.AreEqual(enabled, sourceEditConnectionButton.Enabled);
            }
            else if(where.Contains("source"))
            {
                var sourceEditConnectionButton = DeployUIMap.GetSourceEditConnectionButton(GetActiveTab()) as WpfButton;
                Assert.IsNotNull(sourceEditConnectionButton);
                Assert.AreEqual(enabled, sourceEditConnectionButton.Enabled);
            }
            else if(where.Contains("explorer"))
            {
                var button = ExplorerUIMap.GetConnectControl("Button");
                if(button != null)
                {
                    Assert.AreEqual(enabled, button.Enabled);
                }
            }
            else if(where == "scheduler")
            {
                var button = SchedulerUiMap.GetConnectControl("Button");
                if(button != null)
                {
                    Assert.AreEqual(enabled, button.Enabled);
                }
            }
            else if(where == "manage settings" || where == "settings")
            {
                var button = SecurityUiMap.GetConnectControl("Button");
                if(button != null)
                {
                    Assert.AreEqual(enabled, button.Enabled);
                }
            }
        }
        
        [When(@"I select ""(.*)"" from the connections list in the '(.*)'")]
        [When(@"I select ""(.*)"" from the connections list in the ""(.*)""")]
        [Given(@"I select ""(.*)"" from the connections list in the ""(.*)""")]
        public void WhenISelectFromTheConnectionsListInThe(string connection, string at)
        {
            var where = at.ToLower();

            if(where == "explorer")
            {
                if(connection == "New Remote Server...")
                {
                    ExplorerUIMap.ChooseServerWithKeyboard(TabManagerUIMap.GetActiveTab(), connection);

                    UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;

                    if(uiTestControl == null)
                    {
                        Assert.Fail("Error - Failed to show new server wizard!");
                    }
                }
                else
                {
                    ExplorerUIMap.ChooseSourceServer(TabManagerUIMap.GetActiveTab(), connection);
                }
            }
            else if(where == "destination")
            {
                if(connection == "New Remote Server...")
                {
                    DeployUIMap.ChooseSourceServerWithKeyboard(TabManagerUIMap.GetActiveTab(), connection);

                    UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;

                    if(uiTestControl == null)
                    {
                        Assert.Fail("Error - Failed to show new server wizard!");
                    }
                }
                else
                {
                    DeployUIMap.ChooseSourceServer(TabManagerUIMap.GetActiveTab(), connection);
                }
            }
            else if(where == "source")
            {
                if(connection == "New Remote Server...")
                {
                    DeployUIMap.ChooseDestinationServerWithKeyboard(TabManagerUIMap.GetActiveTab(), connection);

                    UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;

                    if(uiTestControl == null)
                    {
                        Assert.Fail("Error - Failed to show new server wizard!");
                    }
                }
                else
                {
                    DeployUIMap.ChooseDestinationServer(TabManagerUIMap.GetActiveTab(), connection);
                }
            }
            else if(where == "settings" || where == "manage settings")
            {
                if(connection == "New Remote Server...")
                {
                    SecurityUiMap.ChooseDestinationServerWithKeyboard(connection);

                    UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;

                    if(uiTestControl == null)
                    {
                        Assert.Fail("Error - Failed to show new server wizard!");
                    }
                }
                else
                {
                    SecurityUiMap.ChooseDestinationServer(connection);
                }
            }
            else if(where == "scheduler")
            {
                if(connection == "New Remote Server...")
                {
                    SchedulerUiMap.ChooseServerWithKeyboard(connection);

                    UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;

                    if(uiTestControl == null)
                    {
                        Assert.Fail("Error - Failed to show new server wizard!");
                    }
                }
                else
                {
                    SchedulerUiMap.ChooseServer(connection);
                }
            }
            Playback.Wait(5000);
        }

        [When(@"I click Save")]
        public void WhenIClickSave()
        {
            NewServerUIMap.ClickSave();
        }

        [Then(@"I enter the address ""(.*)"" in the connections dialog")]
        public void ThenIEnterTheAddressInTheConnectionsDialog(string serverAddress)
        {
            NewServerUIMap.EnterServerAddress(serverAddress);
        }

        [Then(@"I select authentication type ""(.*)""")]
        public void ThenISelectAuthenticationType(string authenticationType)
        {
            NewServerUIMap.SelectAuthenticationType(authenticationType);
        }
        
        [Then(@"""(.*)"" tab is opened")]
        [When(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string tabName)
        {
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            Assert.AreEqual(tabName, activeTabName);
        }

        [Then(@"""(.*)"" with server permissions ""(.*)"" is ""(.*)""")]
        public void ThenWithServerPermissionsIs(string p0, string p1, string p2)
        {
            
        }
        

        [When(@"I right click delete the server ""(.*)"" from the Explorer")]
        public void WhenIRightClickDeleteTheServerFromTheExplorer(string serverName)
        {
            ExplorerUIMap.RightClickDeleteResource(serverName, "", "localhost");
        }


        [When(@"I right click and remove the server ""(.*)"" from the Explorer")]
        public void WhenIRightClickAndRemoveTheServerFromTheExplorer(string serverName)
        {
            ExplorerUIMap.RightClickRemoveResource(serverName, "", "localhost");
        }
        
        UITestControl GetActiveTab()
        {
            return TabManagerUIMap.GetActiveTab();
        }

        [Then(@"""(.*)"" is connected to ""(.*)""")]
        [Then(@"'(.*)' is connected to ""(.*)""")]
        public void ThenIsConnectedTo(string p0, string p1)
        {
            var uiTestControl = GetActiveTab();
            var destinationServerList = DeployUIMap.GetDestinationServerList(uiTestControl) as WpfComboBox;
            Assert.IsNotNull(destinationServerList);
            Assert.AreEqual(p1, destinationServerList.SelectedItem);
        }

        [Then(@"the server ""(.*)"" will not be in the explorer connections")]
        public void ThenTheServerWillNotBeInTheExplorerConnections(string serverName)
        {
            var explorerServerList = ExplorerUIMap.GetConnectControl("ComboBox") as WpfComboBox;
            Assert.IsNotNull(explorerServerList);
            var uiTestControlCollection = explorerServerList.Items.GetValuesOfControls();
            CollectionAssert.DoesNotContain(uiTestControlCollection, serverName);
        }

        [Then(@"saved server ""(.*)"" is selected in '(.*)' connect control")]
        [Then(@"saved server ""(.*)"" is selected in ""(.*)"" connect control")]
        public void ThenSavedServerIsSelectedInConnectControl(string servername, string at)
        {
            WpfComboBox comboBox;
            var where = at.ToLower();
            if(where.Contains("destination"))
            {
                comboBox = DeployUIMap.GetSourceContolByFriendlyName(GetActiveTab(), "ComboBox") as WpfComboBox;
                Assert.IsNotNull(comboBox);
                Assert.AreEqual(servername, comboBox.SelectedItem);

            }
            else if(where.Contains("source"))
            {
                comboBox = DeployUIMap.GetDestinationContolByFriendlyName(GetActiveTab(), "ComboBox") as WpfComboBox;
                Assert.IsNotNull(comboBox);
                Assert.AreEqual(servername, comboBox.SelectedItem);
            }
            else if(where.Contains("explorer"))
            {
                comboBox = ExplorerUIMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(comboBox);
                Assert.AreEqual(servername, comboBox.SelectedItem);
            }
            else if(where == "scheduler")
            {
                comboBox = SchedulerUiMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(comboBox);
                Assert.AreEqual(servername, comboBox.SelectedItem);
            }
            else if(where == "manage settings" || where == "settings")
            {
                comboBox = SecurityUiMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(comboBox);
                Assert.AreEqual(servername, comboBox.SelectedItem);
            }
        }
        
        [Then(@"all ""(.*)"" resources should be ""(.*)"" in ""(.*)""")]
        [Then(@"all ""(.*)"" resources should be ""(.*)"" in '(.*)'")]
        [When(@"all ""(.*)"" resources should be ""(.*)"" in '(.*)'")]
        public void ThenAllResourcesShouldBeIn(string p0, string p1, string p2)
        {
            //TJ-TODO:- Confirm what to assert here
        }

        [Given(@"""(.*)"" saved servers are ""(.*)"" in '(.*)' connections")]
        [Then(@"""(.*)"" saved servers are ""(.*)"" in '(.*)' connections")]
        [Then(@"""(.*)"" saved servers are ""(.*)"" in ""(.*)"" connections")]
        public void ThenSavedServersAreInConnections(string p0, string p1, string p2)
        {
            //TJ-TODO:- How do we assert against all servers
            //ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" should be available in ""(.*)"" connections")]
        public void ThenShouldBeAvailableInConnections(string p0, string p1)
        {
            //TJ-TODO:- Replace with collections assert of available servers
            // ScenarioContext.Current.Pending();
        }

        [When(@"I click Cancel")]
        public void WhenIClickCancel()
        {
            NewServerUIMap.ClickCancel();
        }

        [Then(@"the following ""(.*)"" connections are shown")]
        public void ThenTheFollowingConnectionsAreShown(string at, Table table)
        {
            var where = at.ToLower();
            UITestControl currentTab = new UITestControl();

            if(where.Contains("current"))
            {
                currentTab = GetActiveTab();
            }

            if(where.Contains("explorer") || currentTab == null)
            {
                var explorerServerList = ExplorerUIMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(explorerServerList);
                var uiTestControlCollection = explorerServerList.Items.GetValuesOfControls();
                var servers = table.Rows.ToList();
                foreach(var server in servers)
                {
                    var value = server.Values.ToList()[0];
                    CollectionAssert.Contains(uiTestControlCollection, value);
                }
            }
            else if(where.Contains("destination"))
            {
                var uiTestControl = GetActiveTab();
                var destinationServerList = DeployUIMap.GetDestinationServerList(uiTestControl) as WpfComboBox;
                Assert.IsNotNull(destinationServerList);
                var uiTestControlCollection = destinationServerList.Items.GetValuesOfControls();
                var servers = table.Rows.ToList();
                foreach(var server in servers)
                {
                    CollectionAssert.Contains(uiTestControlCollection, server);
                }
            }
            else if(where.Contains("source"))
            {
                var uiTestControl = GetActiveTab();
                var sourceServerList = DeployUIMap.GetSourceServerList(uiTestControl) as WpfComboBox;
                Assert.IsNotNull(sourceServerList);
                var uiTestControlCollection = sourceServerList.Items.GetValuesOfControls();
                var servers = table.Rows.ToList();
                foreach(var server in servers)
                {
                    CollectionAssert.Contains(uiTestControlCollection, server);
                }
            }
            else if(where.Contains("scheduler"))
            {
                var schedulerServerList = SchedulerUiMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(schedulerServerList);
                var uiTestControlCollection = schedulerServerList.Items.GetValuesOfControls();
                var servers = table.Rows.ToList();
                foreach(var server in servers)
                {
                    var value = server.Values.ToList()[0];
                    CollectionAssert.Contains(uiTestControlCollection, value);
                }
            }
            else if(where.Contains("settings"))
            {
                var securityServerList = SecurityUiMap.GetConnectControl("ComboBox") as WpfComboBox;
                Assert.IsNotNull(securityServerList);
                var uiTestControlCollection = securityServerList.Items.GetValuesOfControls();
                var servers = table.Rows.ToList();
                foreach(var server in servers)
                {
                    var value = server.Values.ToList()[0];
                    CollectionAssert.Contains(uiTestControlCollection, value);
                }
            }
        }

        [Then(@"new server ""(.*)"" should be ""(.*)"" in ""(.*)""")]
        [Then(@"new server ""(.*)"" should be ""(.*)"" in '(.*)'")]
        public void ThenNewServerShouldBeIn(string serverName, string connectedState, string at)
        {   
            //TJ-TODO:- Currently not possible to validate the connected status
        }
        
        [When(@"""(.*)"" server ""(.*)"" should be ""(.*)"" in '(.*)'")]
        public void WhenServerShouldBeIn(string p0, string p1, string p2, string p3)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Connect control ""(.*)"" option is ""(.*)"" in '(.*)'")]
        public void ThenConnectControlOptionIsIn(string p0, string p1, string p2)
        {
            ScenarioContext.Current.Pending();
        }
        
    }
}
