using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;

namespace Warewolf.UI.Tests.Deploy.DeployUIMapClasses
{
    [Binding]
    public partial class DeployUIMap
    {
        [Given(@"Destination Remote Server Is Connected")]
        [Then(@"Destination Remote Server Is Connected")]
        public void ThenDestinationRemoteServerIsConnected()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteContainerText.Exists, "Remote Server is Disconnected");
        }

        [Then(@"The deploy validation message is ""(.*)""")]
        [When(@"The deploy validation message is ""(.*)""")]
        [Given(@"The deploy validation message is ""(.*)""")]
        public void ThenTheDeployValidationMessageIs(string message)
        {
            Assert.AreEqual(message, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButtonMessageText.DisplayText);
        }

        [Given(@"Deploy Window Is Still Open")]
        [When(@"Deploy Window Is Still Open")]
        [Then(@"Deploy Window Is Still Open")]
        public void ThenDeployWindowIsStillOpen()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.Exists);
        }

        [Then(@"Destination Deploy Information Clears")]
        public void ThenDestinationDeployInformationClears()
        {
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton.Enabled);
        }

        public void Click_SelectAllDependencies_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton);
        }

        [Then(@"Deploy Button Is Enabled")]
        public void ThenDeployButtonIsEnabled()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.WaitForControlCondition((control) => { return (control as WpfButton).Enabled; }, 60000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled, "Deploy button is not enabled");
        }

        [Given(@"I Click Close Deploy Tab")]
        [When(@"I Click Close Deploy Tab")]
        [Then(@"I Click Close Deploy Tab")]
        public void Click_Close_Deploy_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton, new Point(16, 11));
        }

        [Then(@"Filtered Resourse Is Checked For Deploy")]
        public void ThenFilteredResourseIsCheckedForDeploy()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.WaitForControlCondition((control) => { return (control as WpfCheckBox).Checked; }, 60000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked, "Deploy source explorer first item checkbox is not checked after filtering and waiting for 1 minute (60000ms).");
        }

        [Given(@"I Click Edit Deploy Destination Server Button")]
        [When(@"I Click Edit Deploy Destination Server Button")]
        [Then(@"I Click Edit Deploy Destination Server Button")]
        public void Click_Edit_Deploy_Destination_Server_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditDestinationButton);
        }

        [When(@"I Select localhost From Deploy Tab Destination Server Combobox")]
        public void Select_localhost_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost.Exists, "localhost (Connected) option does not exist in Destination server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedLocalhostText.Exists, "Selected destination server in deploy is not localhost (Connected).");
        }

        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        [Then(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        [Given(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox()
        {
            UIMap.WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected source server in deploy is not Remote Connection Integration (Connected).");
        }

        [When(@"I Select LocalServerSource From Deploy Tab Destination Server Combobox")]
        [Then(@"I Select LocalServerSource From Deploy Tab Destination Server Combobox")]
        [Given(@"I Select LocalServerSource From Deploy Tab Destination Server Combobox")]
        public void Select_LocalServerSource_From_Deploy_Tab_Destination_Server_Combobox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton.WaitForControlExist(300000);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalServerSource);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.LocalServerSourceText.Exists, "Selected source server in deploy is not Local Server Source.");
        }

        [When(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        [Then(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        [Given(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        public void WhenISelectLocalhostConnectedFromDeployWizardTabSourceServerCombobox()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteContainerText.Exists, "Selected source server in deploy is not Remote Connection Integration.");
        }

        [When(@"I Select localhost From Deploy Tab Source Server Combobox")]
        public void Select_localhost_From_Deploy_Tab_Source_Server_Combobox()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost.Exists, "localhost (Connected) option does not exist in Destination server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.LocalhostText.Exists, "Selected source server in deploy is not localhost (Connected).");
        }
        
        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(226, 13));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected destination server in deploy is not Remote Connection Integration.");
        }        

       [When(@"I Select LocalhostConnected From Deploy Tab Destination Server Combobox")]
        public void Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost.Exists, "Localhost option does not exist in Destination server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhost, new Point(226, 13));
        }
        
        [When(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        public void Enter_DeployViewOnly_Into_Deploy_Source_Filter(string SearchTextboxText)
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = SearchTextboxText;
            if (SearchTextboxText.ToLower() == "localhost".ToLower()) return;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.Exists, "First deploy tab source explorer item does not exist after filter is applied.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Exists, "Deploy source server explorer tree first item checkbox does not exist.");
        }

        public void Filter_Deploy_Source_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = FilterText;
        }

        [Given(@"I Click Deploy Tab Destination Server Combobox")]
        [When(@"I Click Deploy Tab Destination Server Combobox")]
        [Then(@"I Click Deploy Tab Destination Server Combobox")]
        public void Click_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
        }

        [Given(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        [When(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        [Then(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        public void Click_Deploy_Tab_Destination_Server_New_Remote_Server_Item()
        {
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsNewRemoteServer, new Point(223, 10));
        }

        [When(@"I Click Deploy Tab Destination Server Remote Connection Integration Item")]
        [Then(@"I Click Deploy Tab Destination Server Remote Connection Integration Item")]
        [Given(@"I Click Deploy Tab Destination Server Remote Connection Integration Item")]
        public void Click_Deploy_Tab_Destination_Server_Remote_Connection_Integration_Item()
        {
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(223, 10));
        }

        [Given(@"I Click Deploy Tab Source Server Combobox")]
        [When(@"I Click Deploy Tab Source Server Combobox")]
        [Then(@"I Click Deploy Tab Source Server Combobox")]
        public void Click_Deploy_Tab_Source_Server_Combobox()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Source server combobox.");
        }
        
        [Given(@"I Click Deploy Tab Source Server Edit Button")]
        [When(@"I Click Deploy Tab Source Server Edit Button")]
        [Then(@"I Click Deploy Tab Source Server Edit Button")]
        public void Click_Deploy_Tab_Source_Server_Edit_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton, new Point(13, 8));
        }

        [Given(@"I Click Deploy Tab Source Refresh Button")]
        [When(@"I Click Deploy Tab Source Refresh Button")]
        [Then(@"I Click Deploy Tab Source Refresh Button")]
        public void Click_Deploy_Tab_Source_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.RefreshButton);
        }

        [Given(@"I Click Deploy Tab WarewolfStore Item")]
        [When(@"I Click Deploy Tab WarewolfStore Item")]
        [Then(@"I Click Deploy Tab WarewolfStore Item")]
        public void Click_Deploy_Tab_WarewolfStore_Item()
        {
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsWarewolfStore, new Point(214, 9));
        }

        [Given(@"I Deploy ""(.*)"" From Deploy View")]
        [When(@"I Deploy ""(.*)"" From Deploy View")]
        [Then(@"I Deploy ""(.*)"" From Deploy View")]
        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
            Select_Deploy_First_Source_Item();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled,
                "Deploy button is not enabled after valid server and resource are selected.");
            Click_Deploy_Tab_Deploy_Button();
        }

        [Given(@"Resources is visible on the tree")]
        [Then(@"Resources is visible on the tree")]
        public void WhenResourcesIsVisibleOnTheTree()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.WaitForControlExist(60000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.Exists, "No items in the explorer after filtering with the name of a service that does exist and waiting for 1 minute (60000ms).");
        }

        [Given(@"Deploy Button is enabled  ""(.*)""")]
        [Then(@"Deploy Button is enabled  ""(.*)""")]
        public void ThenDeployButtonIsEnabled(string enabled)
        {
            var isEnabled = bool.Parse(enabled);
            if (isEnabled)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.WaitForControlEnabled();
            }
            else
                Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled);
        }

        [Given(@"I Select localhost checkbox from the source tab")]
        [When(@"I Select localhost checkbox from the source tab")]
        [Then(@"I Select localhost checkbox from the source tab")]
        public void WhenISelectLocalhostFromTheSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.EnvironmentNameCheckCheckBox);
        }

        [Given(@"Source explorer first item is checked")]
        [Then(@"Source explorer first item is checked")]
        public void SourceExplorerFirstItemIsChecked()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked, "Explorer first resources is unchecked.");
        }

        [Then(@"I validate I can not Deploy ""(.*)""")]
        public void ValidateICanNotDeploy(string resource)
        {
            Filter_Deploy_Source_Explorer(resource);
            Playback.Wait(2000);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.RemoteServer.FirstRemoteResource.FirstRemoteResourceCheckBox.Enabled, "The Deploy selection checkbox is Enabled");
        }

        [Then(@"I validate I can Deploy ""(.*)""")]
        public void ValidateICanDeploy(string resource)
        {
            Filter_Deploy_Source_Explorer(resource);
            Playback.Wait(2000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.RemoteServer.FirstRemoteResource.FirstRemoteResourceCheckBox.Enabled, "The Deploy selection checkbox is not Enabled");
        }

        [When(@"I Select Deploy First Source Item")]
        [Then(@"I Select Deploy First Source Item")]
        [Given(@"I Select Deploy First Source Item")]
        public void Select_Deploy_First_Source_Item()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = true;
        }

        [When(@"I DeSelect Deploy First Source Item")]
        [Then(@"I DeSelect Deploy First Source Item")]
        [Given(@"I DeSelect Deploy First Source Item")]
        public void DeSelect_Deploy_First_Source_Item()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = false;
        }

        [When(@"I Click Deploy Tab Deploy Button")]
        public void Click_Deploy_Tab_Deploy_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton);
            WaitForDeploySuccess();
        }

        void WaitForDeploySuccess()
        {
            bool seenVersionConflict = false;
            bool seenSecondVersionConflict = false;
            bool seenConflict = false;
            bool successful = TryWaitForADeployMessageDialog(ref seenVersionConflict, ref seenSecondVersionConflict, ref seenConflict);
            if (!successful)
            {
                successful = TryWaitForADeployMessageDialog(ref seenVersionConflict, ref seenSecondVersionConflict, ref seenConflict);
                if (!successful)
                {
                    successful = TryWaitForADeployMessageDialog(ref seenVersionConflict, ref seenSecondVersionConflict, ref seenConflict);
                    if (!successful)
                    {
                        successful = TryWaitForADeployMessageDialog(ref seenVersionConflict, ref seenSecondVersionConflict, ref seenConflict);
                    }
                }
            }
            Assert.IsTrue(successful, "Deploy failed.");
        }

        [When(@"I Click Deploy Tab Deploy Button with no version conflict dialog")]
        public void Click_Deploy_Tab_Deploy_Button_no_version_conflict_dialog()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton);
            WaitForDeploySuccessSkippingVersionConflicts();
        }

        void WaitForDeploySuccessSkippingVersionConflicts()
        {
            bool seenVersionConflict = false;
            bool seenConflict = false;
            bool successful = TryWaitForADeployMessageDialog(ref seenConflict, ref seenVersionConflict);
            if (!successful)
            {
                successful = TryWaitForADeployMessageDialog(ref seenConflict, ref seenVersionConflict);
                if (!successful)
                {
                    successful = TryWaitForADeployMessageDialog(ref seenConflict, ref seenVersionConflict);
                }
            }
            Assert.IsTrue(successful, "Deploy failed.");
        }

        bool TryWaitForADeployMessageDialog(ref bool seenConflict, ref bool seenVersionConflict, ref bool seenSecondVersionConflict)
        {
            bool OKButtonReady = DialogsUIMap.MessageBoxWindow.OKButton.WaitForControlCondition((control) => { return control.TryGetClickablePoint(out Point point); }, 60000);
            if (!seenVersionConflict && !seenSecondVersionConflict && !seenConflict)
            {
                seenVersionConflict = DialogsUIMap.MessageBoxWindow.DeployVersionConflicText.Exists;
                if (seenVersionConflict && OKButtonReady)
                {
                    Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
                    return false;
                }
            }
            if (!seenSecondVersionConflict && !seenConflict)
            {
                seenSecondVersionConflict = DialogsUIMap.MessageBoxWindow.DeployVersionConflicText.Exists;
                if (seenSecondVersionConflict && OKButtonReady)
                {
                    Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
                    return false;
                }
            }
            if (!seenConflict)
            {
                seenConflict = DialogsUIMap.MessageBoxWindow.DeployConflictsText.Exists;
                if (seenConflict && OKButtonReady)
                {
                    Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
                    return false;
                }
            }
            var successful = DialogsUIMap.MessageBoxWindow.ResourcesDeployedSucText.Exists;
            if (successful && OKButtonReady)
            {
                Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
            }
            return successful;
        }

        bool TryWaitForADeployMessageDialog(ref bool seenConflict, ref bool seenVersionConflict)
        {
            DialogsUIMap.MessageBoxWindow.OKButton.WaitForControlCondition((control) => { return control.TryGetClickablePoint(out Point point); }, 60000);
            if (!seenVersionConflict && !seenConflict)
            {
                if (DialogsUIMap.MessageBoxWindow.DeployVersionConflicText.Exists)
                {
                    Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
                    return false;
                }
            }
            if (!seenConflict)
            {
                if (DialogsUIMap.MessageBoxWindow.DeployConflictsText.Exists)
                {
                    Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
                    return false;
                }
            }
            var successful = DialogsUIMap.MessageBoxWindow.ResourcesDeployedSucText.Exists;
            if (successful)
            {
                Mouse.Click(DialogsUIMap.MessageBoxWindow.OKButton);
            }
            return successful;
        }

        [When(@"I Click Deploy Tab Deploy Button And Cancel")]
        public void Click_Deploy_Tab_Deploy_Button_And_Cancel()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton);
            Mouse.Click(DialogsUIMap.MessageBoxWindow.CancelButton);
        }

        [When(@"I Select ""(.*)"" from the source tab")]
        public void WhenISelectFromTheSourceTab(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
            Select_Deploy_First_Source_Item();
        }

        [When(@"I filter for ""(.*)"" on the source filter")]
        public void WhenIFilterForOnTheSourceFilter(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
        }
        
        [When(@"I Click Deploy button")]
        public void ThenIClickDeployButton()
        {
            Click_Deploy_Tab_Deploy_Button();
        }

        [Given(@"I Click Close Deploy Tab Button")]
        [When(@"I Click Close Deploy Tab Button")]
        [Then(@"I Click Close Deploy Tab Button")]
        public void Click_Close_Deploy_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton.Exists, "DeployTab close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton, new Point(16, 6));
        }

        [Given(@"The Deploy Tab is visible")]
        [Then(@"The Deploy Tab is visible")]
        public void The_Deploy_Tab_is_Visible()
        {
            Point point;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.TryGetClickablePoint(out point), "Deploy tab is not visible");
        }

        public UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;
    }
}
