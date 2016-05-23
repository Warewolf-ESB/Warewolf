
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Explorer;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
// ReSharper disable InconsistentNaming

namespace Warewolf.AcceptanceTesting.Deploy
{
    [Binding]
    public class DeployTabSteps
    {
        [BeforeFeature("DeployTab")]
        public static void SetupForSystem()
        {
            Core.Utils.SetupResourceDictionary();
            var shell = GetMockShellVm(true,"localhost");
            var dest = new DeployDestinationViewModelForTesting(GetMockShellVm(false, "DestinationServer"), GetMockAggegator());
            FeatureContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest)) { Children = new List<IExplorerItemViewModel> { CreateExplorerVms() } };
            FeatureContext.Current["Src"] = src;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, GetPopup());
            FeatureContext.Current["vm"] = vm;
            var view = new DeployView { DataContext = vm };
            FeatureContext.Current["View"] = view;
            Core.Utils.ShowTheViewForTesting(view);
        }

        [AfterScenario]
        public void Cleanup()
        {
            var shell = GetMockShellVm(true, "localhost");
            var dest = new DeployDestinationViewModelForTesting(GetMockShellVm(false, "DestinationServer"), GetMockAggegator());
            FeatureContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest)) { Children = new List<IExplorerItemViewModel> { CreateExplorerVms() } };
            FeatureContext.Current["Src"] = src;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, GetPopup());
            FeatureContext.Current["vm"] = vm;
            var view = FeatureContext.Current["View"] as DeployView;
            if(view != null)
            {
                view.DataContext = vm;
            }
            GetPopupFromContext().Setup(a => a.ShowDeployConflict(It.IsAny<int>()));
            GetPopupFromContext().Setup(a => a.ShowDeployNameConflict(It.IsAny<string>()));
            _deployed = false;
        }

        public SingleExplorerDeployViewModel GetVm()
        {
            return (SingleExplorerDeployViewModel)FeatureContext.Current["vm"];
        }

        static IPopupController GetPopup()
        {
            var popup = new Mock<IPopupController>();
            FeatureContext.Current["Popup"] = popup;
            return popup.Object;
        }

        static IDeployStatsViewerViewModel GetStatsVm(IExplorerViewModel dest)
        {
            return new DeployStatsViewerViewModel(dest);
        }

        static bool _deployed;

        static Microsoft.Practices.Prism.PubSubEvents.IEventAggregator GetMockAggegator()
        {
            return new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>().Object;
        }

        static IShellViewModel GetMockShellVm(bool setContext = false, string name = "")
        {
            var shell = new Mock<IShellViewModel>();
            shell.Setup(a => a.LocalhostServer).Returns(GetMockServer(name));
            shell.Setup(a => a.DeployResources(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IList<Guid>>())).Callback(() =>
            {
                _deployed = true;
            });
            if (setContext)
                FeatureContext.Current["Shell"] = shell;
            return shell.Object;
        }

        static IServer GetMockServer(string name)
        {
            var server = new Mock<IServer>();
            var qp = new Mock<IQueryManager>();
            qp.Setup(a => a.FetchDependenciesOnList(It.IsAny<IEnumerable<Guid>>())).Returns(new List<Guid> { Guid.Parse("5C8B5660-CE6E-4D22-84D8-5B77DC749F70") });
            server.Setup(a => a.LoadExplorer()).Returns(new Task<IExplorerItem>(CreateExplorerSourceItems));
            server.Setup(a => a.GetServerConnections()).Returns(GetServers());
            server.Setup(a => a.Permissions).Returns(new List<IWindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    Administrator = true,
                    DeployFrom = true,
                    DeployTo = true
                }
            });
            server.Setup(a => a.DisplayName).Returns("LocalHost");
            server.Setup(a => a.ResourceName).Returns("LocalHost");
            server.Setup(a => a.IsConnected).Returns(true);
            server.Setup(a => a.ResourceID).Returns(Guid.NewGuid());
            server.Setup(a => a.EnvironmentID).Returns(Guid.Empty);
            server.Setup(a => a.QueryProxy).Returns(qp.Object);
            if(!String.IsNullOrEmpty(name))
            {
                if(!FeatureContext.Current.ContainsKey(name))
                {
                    FeatureContext.Current.Add(name, server);
                }
            }
            return server.Object;
        }

        static IList<IServer> GetServers()
        {
            var server = new Mock<IServer>();
            server.Setup(a => a.LoadExplorer()).Returns(new Task<IExplorerItem>(CreateExplorerSourceItems));
            server.Setup(a => a.DisplayName).Returns("Remote");
            server.Setup(a => a.ResourceName).Returns("Remote");
            server.Setup(a => a.Permissions).Returns(new List<IWindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    Administrator = true,
                    DeployFrom = true,
                    DeployTo = true
                }
            });
            server.Setup(a => a.EnvironmentID).Returns(Guid.NewGuid());
            server.Setup(a => a.IsConnected).Returns(true);
            server.Setup(a => a.ConnectAsync()).Returns(Task.FromResult(true));
            FeatureContext.Current["DestinationServer"] = server;
            return new List<IServer>
            {
                server.Object
            };
        }

        static IExplorerItem CreateExplorerSourceItems()
        {
            return new ServerExplorerItem
            {
                DisplayName = "Examples",
                Children = new List<IExplorerItem>
                {
                    new ServerExplorerItem{DisplayName = "Utility - Date and Time" , ResourcePath = "Examples\\Utility - Date and Time" }
                }

            };
        }

        static IExplorerItemViewModel CreateExplorerVms()
        {
            ExplorerItemViewModel ax = null;
            ax = new ExplorerItemViewModel(new Mock<IServer>().Object, null, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object)
            {
                ResourceName = "Examples",
                Children = new ObservableCollection<IExplorerItemViewModel>
                {
                    // ReSharper disable once ExpressionIsAlwaysNull
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) { ResourceName = "Utility - Date and Time", ResourcePath = "Examples\\Utility - Date and Time" }
                }
            };
            return ax;
        }

        [Given(@"selected Source Server is ""(.*)""")]
        public void GivenSelectedSourceServerIs(string p0)
        {
            Assert.IsTrue(GetView().SelectedServer.DisplayName.ToLower().Equals(p0.ToLower()));
        }

        DeployView GetView()
        {
            return (DeployView)FeatureContext.Current["View"];
        }

        static Mock<IPopupController> GetPopupFromContext()
        {
            return (Mock<IPopupController>)FeatureContext.Current["Popup"];
        }

        [When(@"""(.*)"" is Disconnected")]
        public void WhenIsDisconnected(string p0)
        {
            var svr = GetDestinationServer();
            svr.Setup(a => a.IsConnected).Returns(false);
        }

        Mock<IServer> GetDestinationServer()
        {
            return (Mock<IServer>)FeatureContext.Current["DestinationServer"];
        }

        [When(@"selected Destination Server is ""(.*)""")]
        public void WhenSelectedDestinationServerIs(string d)
        {
            GetView().SelectDestinationServer(d);
        }

        [Given(@"selected Destination Server is ""(.*)""")]
        public void GivenSelectedDestinationServerIs(string d)
        {
            Assert.IsTrue(GetView().SelectedDestinationServer.DisplayName.ToLower().Equals(d.ToLower()));
        }

        [Then(@"the validation message is ""(.*)""")]
        public void ThenTheValidationMessageIs(string message)
        {

            Assert.IsTrue(GetView().ErrorMessage.ToLower().Equals(message.ToLower())
                || GetView().StatusPassedMessage.ToLower().Equals(message.ToLower()));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            Assert.AreEqual(p1, GetView().CanDeploy);

        }

        [When(@"I Select All Dependecies")]
        public void WhenISelectAllDependecies()
        {
            GetView().SelectDependencies();
        }

        [Then(@"Select All Dependencies is ""(.*)""")]
        public void ThenSelectAllDependenciesIs(string p0)
        {
            Assert.AreEqual(GetView().CanSelectDependencies, p0);
        }

        [Given(@"I select ""(.*)"" from Source Server")]
        [When(@"I select ""(.*)"" from Source Server")]
        [Then(@"I select ""(.*)"" from Source Server")]
        public void WhenISelectFromSourceServer(string p0)
        {
            GetView().SelectPath(p0);
        }

        [When(@"I check ""(.*)"" on Source Server")]
        public void WhenICheckOnSourceServer(string p0)
        {
            GetView().SelectServer(p0);
        }

        [Then(@"""(.*)"" the resources are checked")]
        public void ThenTheResourcesAreChecked(string p0)
        {
            Assert.IsTrue(GetView().VerifyAllSelected(p0));
        }


        [When(@"I Unselect ""(.*)"" from Source Server")]
        public void WhenIUnselectFromSourceServer(string p0)
        {
            GetView().UnSelectPath(p0);
        }

        [Then(@"""(.*)"" from Source Server is ""(.*)""")]
        public void ThenFromSourceServerIs(string p0, string p1)
        {
            Assert.AreEqual(p1, GetView().VerifySelectPath(p0));
        }

        [When(@"I type ""(.*)"" in Source Server filter")]
        public void WhenITypeInSourceServerFilter(string filter)
        {
            GetView().SetFilter(filter);
        }


        [Then(@"visibility of ""(.*)"" from Source Server is ""(.*)""")]
        public void ThenVisibilityOfFromSourceServerIs(string p0, string p1)
        {
            Assert.AreEqual(GetView().VerifySelectPathVisibility(p0), p1);
        }

        [Given(@"source is connected")]
        public void GivenSourceIsConnected()
        {
            GetView().ConnectSourceServer();
        }

        [Given(@"destination is connected")]
        [When(@"destination is connected")]
        [Then(@"destination is connected")]
        public void GivenDestinationIsConnected()
        {
            GetView().ConnectDestinationServer();
        }

        [Given(@"I deploy")]
        [When(@"I deploy")]
        [Then(@"I deploy")]
        public void WhenIDeploy()
        {
            GetView().DeployItems();
        }

        [Then(@"deploy is successfull")]
        public void ThenDeployIsSuccessfull()
        {
            Assert.IsTrue(_deployed);
        }

        [Then(@"deploy is not successfull")]
        public void ThenDeployIsNotSuccessfull()
        {
            Assert.IsFalse(_deployed);
        }

        [Then(@"Resource exists in the destination server popup is shown")]
        public void ThenResourceExistsInTheDestinationServerPopupIsShown(Table table)
        {
            GetPopupFromContext().Verify(a => a.ShowDeployConflict(It.IsAny<int>()));
            var conflictItems = GetVm().ConflictItems;
            foreach(var conflictItem in conflictItems)
            {
                var item = conflictItem;
                var exists = table.Rows.Any(row => row["Source Resource"] == item.SourceName && row["Destination Resource"] == item.DestinationName);
                Assert.IsTrue(exists,conflictItem.DestinationName+" failed");
            }
        }

        [Given(@"I cannot deploy to destination")]
        public void GivenICannotDeployToDestination()
        {
            var destinationServer = GetDestinationServer();
            destinationServer.Setup(server => server.Permissions).Returns(new List<IWindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    DeployFrom = true,
                    Administrator = false,
                    Contribute = false,
                    DeployTo = false,
                    IsServer = true,
                    ResourceID = Guid.Empty
                }
            });
        }

        [Given(@"I cannot deploy from source")]
        public void GivenICannotDeployFromSource()
        {
            GetVm().Source.SelectedServer.Permissions = new List<IWindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    DeployFrom = false,
                    Administrator = false,
                    Contribute = false,
                    DeployTo = true,
                    IsServer = true,
                    ResourceID = Guid.Empty
                }
            };
        }

        [When(@"I click Cancel on Resource exists in the destination server popup")]
        public void WhenIClickCancelOnResourceExistsInTheDestinationServerPopup()
        {
            GetPopupFromContext().Setup(a => a.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.Cancel);
        }

        [When(@"I click OK on Resource exists in the destination server popup")]
        public void WhenIClickOKOnResourceExistsInTheDestinationServerPopup()
        {
            GetPopupFromContext().Setup(a => a.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.OK);
        }

        [Then(@"the User is prompted to ""(.*)"" one of the resources")]
        public void ThenTheUserIsPromptedToOneOfTheResources(string p0)
        {
            GetPopupFromContext().Verify(a => a.ShowDeployNameConflict(It.IsAny<string>()));
        }

        [Then(@"Services is ""(.*)""")]
        public void ThenServicesIs(string p0)
        {
            Assert.AreEqual(GetView().Services, p0);
        }

        [Then(@"Sources is ""(.*)""")]
        public void ThenSourcesIs(string p0)
        {
            Assert.AreEqual(GetView().Sources, p0);
        }

        [Then(@"New Resource is ""(.*)""")]
        public void ThenNewResourceIs(string p0)
        {
            Assert.AreEqual(GetView().New, p0);
        }

        [Given(@"Override is ""(.*)""")]
        [When(@"Override is ""(.*)""")]
        [Then(@"Override is ""(.*)""")]
        public void ThenOverrideIs(string p0)
        {
            Assert.AreEqual(GetView().Overrides, p0);
        }

        [Then(@"the ""(.*)"" is ""(.*)""")]
        public void ThenTheIs(string control, string visibility)
        {
            GetView().CheckVisibility(control, visibility);
        }

        [Then(@"Context Menu is ""(.*)""")]
        public void ThenContextMenuIs(string visibility)
        {
            GetView().CheckVisibility("Context Menu", visibility);
        }

        [Given(@"I have deploy tab opened")]
        public void GivenIHaveDeployTabOpened()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Then(@"a warning message appears ""(.*)""")]
        public void ThenAWarningMessageAppears(string p0)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
