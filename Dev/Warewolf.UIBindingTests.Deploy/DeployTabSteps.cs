﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Explorer;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;

// ReSharper disable InconsistentNaming

namespace Warewolf.UIBindingTests.Deploy
{
    [Binding]
    public class DeployTabSteps
    {
        [BeforeFeature("DeployTab")]
        public static void SetupForSystem()
        {
            Core.Utils.SetupResourceDictionary();
            var shell = GetMockShellVm(true, "localhost");
            var shellViewModel = GetMockShellVm(false, "DestinationServer");
            var dest = new DeployDestinationViewModelForTesting(shellViewModel, GetMockAggegator());
            FeatureContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest)) { Children = new List<IExplorerItemViewModel> { CreateExplorerVms() } };
            FeatureContext.Current["Src"] = src;
            var popupController = GetPopup().Object;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, popupController);
            FeatureContext.Current["vm"] = vm;
            FeatureContext.Current["stats"] = stats;
        }

        [AfterScenario]
        public void Cleanup()
        {
            var shell = GetMockShellVm(true, "localhost");
            var dest = new DeployDestinationViewModelForTesting(GetMockShellVm(false, "DestinationServer"), GetMockAggegator());
            FeatureContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest))
            {
                Children = new List<IExplorerItemViewModel> { CreateExplorerVms() }
            };
            FeatureContext.Current["Src"] = src;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, GetPopup().Object);
            FeatureContext.Current["vm"] = vm;
            GetPopup().Setup(a => a.ShowDeployConflict(It.IsAny<int>()));
            GetPopup().Setup(a => a.ShowDeployNameConflict(It.IsAny<string>()));
            vm.PopupController = GetPopup().Object;
            _deployed = false;
        }

        static Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController> GetPopup()
        {
            var containsKey = FeatureContext.Current.ContainsKey("Popup");
            if (containsKey)
            {
                var popUp = FeatureContext.Current["Popup"];
                return (Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>)popUp;
            }
            var popup = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            FeatureContext.Current["Popup"] = popup;
            return popup;
        }

        static IDeployStatsViewerViewModel GetStatsVm(IDeployDestinationExplorerViewModel dest)
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
            var containsKey = FeatureContext.Current.ContainsKey("localhost");
            if (!containsKey)
            {
                if (name.Equals("LocalHost", StringComparison.CurrentCultureIgnoreCase))
                {
                    shell.Setup(a => a.LocalhostServer).Returns(GetMockServer(name));
                }
            }
            else
            {
                var server = FeatureContext.Current.Get<Mock<IServer>>("localhost");
                shell.Setup(a => a.LocalhostServer).Returns(server.Object);
                
            }
       
           
            shell.Setup(a => a.DeployResources(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IList<Guid>>(), It.IsAny<bool>())).Callback(() =>
            {
                _deployed = true;
            });
            if (setContext)
            {
                FeatureContext.Current["Shell"] = shell;
                CustomContainer.Register(shell.Object);
            }
            return shell.Object;
        }

        static IServer GetMockServer(string name)
        {
            var server = new Mock<IServer>();
            var qp = new Mock<IQueryManager>();
            qp.Setup(a => a.FetchDependenciesOnList(It.IsAny<IEnumerable<Guid>>())).Returns(new List<Guid> { Guid.Parse("5C8B5660-CE6E-4D22-84D8-5B77DC749F70") });
            server.Setup(a => a.LoadExplorer(It.IsAny<bool>())).Returns(Task.FromResult(CreateExplorerSourceItems()));
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

            server.Setup(a => a.ConnectAsync()).Returns(Task.FromResult(true));
            server.Setup(a => a.DisplayName).Returns("LocalHost");
            server.Setup(a => a.ResourceName).Returns("LocalHost");
            server.Setup(a => a.IsConnected).Returns(true);
            server.Setup(a => a.ResourceID).Returns(Guid.NewGuid());
            server.Setup(a => a.EnvironmentID).Returns(Guid.Empty);
            server.Setup(a => a.QueryProxy).Returns(qp.Object);
            server.Setup(a => a.CanDeployFrom).Returns(true);
            server.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            server.Setup(a => a.GetMinSupportedVersion()).Returns("1.0.0.0");
            if (!name.Equals("LocalHost", StringComparison.InvariantCultureIgnoreCase))
            {
                server.Setup(a => a.DisplayName).Returns(name);
                server.Setup(a => a.ResourceName).Returns(name);
                Func<Guid> valueFunction = Guid.NewGuid;
                server.Setup(a => a.EnvironmentID).Returns(valueFunction);
            }
            if (!string.IsNullOrEmpty(name))
            {
                if (!FeatureContext.Current.ContainsKey(name))
                {
                    FeatureContext.Current.Add(name, server);
                }
            }
            return server.Object;
        }

        static IList<IServer> GetServers()
        {
            var server = new Mock<IServer>();
            server.Setup(a => a.LoadExplorer(It.IsAny<bool>())).Returns(Task.FromResult(CreateExplorerSourceItems()));
            server.Setup(a => a.DisplayName).Returns("Remote");
            server.Setup(a => a.ResourceName).Returns("Remote");
            server.Setup(a => a.CanDeployTo).Returns(true);
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
            server.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            server.Setup(a => a.GetMinSupportedVersion()).Returns("1.0.0.0");
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
                    new ServerExplorerItem
                    {
                        DisplayName = "Utility - Date and Time"
                    , ResourcePath = "Examples\\Utility - Date and Time"
                    }
                }

            };
        }

        static IExplorerItemViewModel CreateExplorerVms()
        {
            ExplorerItemViewModel ax = null;
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.CanDeployFrom).Returns(true);
            ax = new ExplorerItemViewModel(server.Object, null, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object)
            {
                ResourceName = "Examples",
                Children = new ObservableCollection<IExplorerItemViewModel>
                {
                    // ReSharper disable once ExpressionIsAlwaysNull
                    new ExplorerItemViewModel(server.Object, ax, a => { }
                    , new Mock<IShellViewModel>().Object
                    , new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object)
                    {
                        ResourceName = "Utility - Date and Time", ResourcePath = "Examples\\Utility - Date and Time"
                    }
                }
            };
            return ax;
        }

        [Given(@"selected Source Server is ""(.*)""")]
        public void GivenSelectedSourceServerIs(string p0)
        {
            var displayName = GetViewModel().Source.SelectedServer.DisplayName;
            Assert.IsTrue(displayName.ToLower().Equals(p0.ToLower()));
        }

        SingleExplorerDeployViewModel GetViewModel()
        {
            var view = (SingleExplorerDeployViewModel)FeatureContext.Current["vm"];
            return view;
        }

        [When(@"destination Server Version is ""(.*)""")]
        public void WhenDestinationServerVersionIs(string p0)
        {
            GetPopup().Setup(controller => controller.ShowDeployServerVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            GetViewModel().PopupController = GetPopup().Object;
            var dest = GetDestinationServer();
            dest.Setup(server => server.GetMinSupportedVersion()).Returns(p0);
            dest.Setup(server => server.GetServerVersion()).Returns(p0);
        }

        [When(@"""(.*)"" is Disconnected")]
        public void WhenIsDisconnected(string p0)
        {
            var svr = GetDestinationServer();
            svr.Setup(a => a.IsConnected).Returns(false);
        }

        Mock<IServer> GetDestinationServer(string destname = "DestinationServer")
        {
            return (Mock<IServer>)FeatureContext.Current[destname];
        }

        [When(@"selected Destination Server is ""(.*)""")]
        public void WhenSelectedDestinationServerIs(string d)
        {
            var displayName = GetViewModel().Destination.SelectedServer.DisplayName;
            Assert.AreEqual(d, displayName);
        }

        [Then(@"I select Destination Server as ""(.*)""")]
        [When(@"I select Destination Server as ""(.*)""")]
        [Given(@"I select Destination Server as ""(.*)""")]
        public void ThenISelectDestinationServerAs(string p0)
        {
          
            var mockServer = GetMockServer(p0);
            var envMock = new Mock<IEnvironmentViewModel>();
            envMock.SetupGet(model => model.Server).Returns(mockServer);
            var deployDestinationExplorerViewModel = GetViewModel().Destination;
            deployDestinationExplorerViewModel.SelectedEnvironment = envMock.Object;
            Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
        }


        [Given(@"selected Destination Server is ""(.*)""")]
        public void GivenSelectedDestinationServerIs(string d)
        {
            var displayName = GetViewModel().Destination.SelectedServer.DisplayName;
            Assert.AreEqual(displayName.ToLower(), d.ToLower());
        }

        [Then(@"the validation message is ""(.*)""")]
        public void ThenTheValidationMessageIs(string message)
        {
            var deployViewModel = GetViewModel();
            var errorMsg = deployViewModel.ErrorMessage.ToLower();
            Assert.AreEqual(message.ToLower(), errorMsg);
        }

        [Then(@"the Deploy validation message is ""(.*)""")]
        public void ThenTheDeployValidationMessageIs(string message)
        {
            var deployViewModel = GetViewModel();

            var errorMsg = deployViewModel.DeploySuccessMessage;
            Assert.AreEqual(message.ToLower(), errorMsg);
        }


        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            var CanDeploy = GetViewModel().DeployCommand.CanExecute(null) ? "Enabled" : "Disabled";
            Assert.AreEqual(p1, CanDeploy);

        }

        [When(@"I Select All Dependecies")]
        public void WhenISelectAllDependecies()
        {
            GetViewModel().SelectDependenciesCommand.Execute(null);
        }

        [Then(@"Select All Dependencies is ""(.*)""")]
        public void ThenSelectAllDependenciesIs(string p0)
        {
            Assert.AreEqual(GetViewModel().CanSelectDependencies, p0);
        }

        [Then(@"source have deployable resources in ""(.*)""")]
        public void ThenSourceHaveDeployableResourcesIn(string p0)
        {
            var deployViewModel = (IDeployViewModel)FeatureContext.Current["vm"];
            var explorerItemViewModel = deployViewModel.Source.SelectedEnvironment.AsList()
                .FirstOrDefault(p => p.ResourcePath == p0);
            if (explorerItemViewModel != null)
                Assert.IsTrue(explorerItemViewModel.CanDeploy);
        }

        [Then(@"resources in ""(.*)"" are now unDeployable")]
        public void ThenResourcesInAreNowUnDeployable(string p0)
        {
            var deployViewModel = (IDeployViewModel)FeatureContext.Current["vm"];
            var explorerItemViewModel = deployViewModel.Source.SelectedEnvironment.AsList()
                .FirstOrDefault(p => p.ResourcePath == p0);
            if (explorerItemViewModel != null)
            {
                Assert.IsFalse(explorerItemViewModel.CanDeploy);
                Assert.IsFalse(explorerItemViewModel.IsResourceChecked != null && explorerItemViewModel.IsResourceChecked.Value);
                Assert.IsFalse(explorerItemViewModel.IsResourceCheckedEnabled);
            }
        }

        [When(@"I select deploy Reource Test")]
        public void WhenISelectDeployReourceTest()
        {
            var singleExplorerDeployViewModel = GetViewModel();
            singleExplorerDeployViewModel.Destination.DeployTests = true;
        }

        [Given(@"source is connected")]
        [Then(@"source is connected")]
        [When(@"source is connected")]
        public void GivenSourceIsConnected()
        {
            var isConnected = GetViewModel().Source.SelectedServer.IsConnected;
            Assert.IsTrue(isConnected);
        }

        [Given(@"destination is connected")]
        [When(@"destination is connected")]
        [Then(@"destination is connected")]
        public void GivenDestinationIsConnected()
        {
            
            GetViewModel().Destination.ConnectControlViewModel.Connect(GetMockServer("DestinationServer"));
        }

        [Given(@"destination ""(.*)"" is connected")]
        [When(@"destination ""(.*)"" is connected")]
        [Then(@"destination ""(.*)"" is connected")]
        public void ThenDestinationIsConnected(string p0)
        {
            var server = FeatureContext.Current.Get<Mock<IServer>>(p0);

            var singleExplorerDeployViewModel = GetViewModel();
            singleExplorerDeployViewModel.Destination.ConnectControlViewModel.SelectedConnection.EnvironmentID = server.Object.EnvironmentID;
            singleExplorerDeployViewModel.Destination.ConnectControlViewModel.Connect(server.Object);
        }


        [Given(@"I deploy")]
        [When(@"I deploy")]
        [Then(@"I deploy")]
        public void WhenIDeploy()
        {
            GetViewModel().DeployCommand.Execute(null);
        }

        [When(@"I Unselect ""(.*)"" from Source Server")]
        public void WhenIUnselectFromSourceServer(string p0)
        {
            var explorerTreeItem = GetViewModel().Source.SelectedItems.First(item => item.ResourceName.Contains(p0));
            GetViewModel().Source.SelectedItems.Remove(explorerTreeItem);
        }

        [When(@"I select ""(.*)"" from Source Server")]
        [Then(@"I select ""(.*)"" from Source Server")]
        [Given(@"I select ""(.*)"" from Source Server")]
        public void WhenISelectFromSourceServer(string p0)
        {

            var viewModel = GetViewModel();
            var explorerItemViewModels = viewModel.Source
                .SelectedEnvironment.Children[0].Children;
            var explorerItemViewModel = explorerItemViewModels
                                                 .FirstOrDefault(model => model.ResourceName.Contains(p0));
            viewModel.Source.SelectedItems = new[]
            {
                explorerItemViewModel
            };
        }

        [When(@"deploy is enabled")]
        public void WhenDeployIsEnabled()
        {
            var deployViewModel = GetViewModel();
            var canDeploy = deployViewModel.DeployCommand.CanExecute(null);
            Assert.IsTrue(canDeploy);
        }


        [Then(@"deploy is successfull")]
        public void ThenDeployIsSuccessfull()
        {
            Assert.IsTrue(GetViewModel().DeploySuccessfull);
        }

        [Then(@"deploy is not successfull")]
        public void ThenDeployIsNotSuccessfull()
        {
            var deployed = _deployed;
            Assert.IsFalse(GetViewModel().DeploySuccessfull);
        }

        [Then(@"Resource exists in the destination server popup is shown")]
        public void ThenResourceExistsInTheDestinationServerPopupIsShown(Table table)
        {
            try
            {
                GetPopup().Verify(a => a.ShowDeployConflict(It.IsAny<int>()), Times.AtLeastOnce);
            }
            catch (Exception)
            {
                GetPopup().Verify(a => a.ShowDeployResourceNameConflict(It.IsAny<string>()), Times.AtLeastOnce);
            }
            GetViewModel().PopupController = GetPopup().Object;
            var conflictItems = GetViewModel().ConflictItems;
            foreach (var conflictItem in conflictItems)
            {
                var item = conflictItem;
                var exists = table.Rows.Any(row => row["Source Resource"] == item.SourceName && row["Destination Resource"] == item.DestinationName);
                Assert.IsTrue(exists, conflictItem.DestinationName + " failed");
            }
        }

        [Given(@"I cannot deploy to destination")]
        public void GivenICannotDeployToDestination()
        {
            const bool deployFrom = true;
            const bool deployTo = false;
            SetDestPermisions(deployFrom, deployTo);
        }

        private void SetDestPermisions(bool deployFrom, bool deployTo)
        {
            var destinationServer = GetDestinationServer();
            destinationServer.Setup(server => server.CanDeployFrom).Returns(deployFrom);
            destinationServer.Setup(server => server.CanDeployTo).Returns(deployTo);
            destinationServer.Setup(server => server.Permissions).Returns(new List<IWindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    DeployFrom = deployFrom,
                    Administrator = false,
                    Contribute = false,
                    DeployTo = deployTo,
                    IsServer = true,
                    ResourceID = Guid.Empty
                }
            });
        }

        [Given(@"I cannot deploy from source")]
        public void GivenICannotDeployFromSource()
        {
            GetViewModel().Source.SelectedServer.Permissions = new List<IWindowsGroupPermission>
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

            GetPopup().Setup(a => a.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.Cancel);
            GetPopup().Setup(a => a.ShowDeployResourceNameConflict(It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            GetViewModel().PopupController = GetPopup().Object;
        }

        [When(@"I click OK on Resource exists in the destination server popup")]
        public void WhenIClickOKOnResourceExistsInTheDestinationServerPopup()
        {
            GetPopup().Setup(a => a.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.OK);
            GetPopup().Setup(a => a.ShowDeployResourceNameConflict(It.IsAny<string>())).Returns(MessageBoxResult.OK);
            GetViewModel().PopupController = GetPopup().Object;
        }

        [Then(@"the User is prompted to ""(.*)"" one of the resources")]
        public void ThenTheUserIsPromptedToOneOfTheResources(string p0)
        {
            GetPopup().Verify(a => a.ShowDeployNameConflict(It.IsAny<string>()));
            GetViewModel().PopupController = GetPopup().Object;
        }

        [Then(@"Services is ""(.*)""")]
        public void ThenServicesIs(string p0)
        {
            var deployStatsViewerViewModel = FeatureContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var services = deployStatsViewerViewModel.Services;
            Assert.AreEqual(services, p0);
        }

        [Then(@"Sources is ""(.*)""")]
        public void ThenSourcesIs(string p0)
        {
            var deployStatsViewerViewModel = FeatureContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var sources = deployStatsViewerViewModel.Sources;
            Assert.AreEqual(sources, p0);
        }

        [Then(@"New Resource is ""(.*)""")]
        public void ThenNewResourceIs(string p0)
        {
            var deployStatsViewerViewModel = FeatureContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var @new = deployStatsViewerViewModel.New;
            Assert.AreEqual(p0, @new);
        }

        [Given(@"Override is ""(.*)""")]
        [When(@"Override is ""(.*)""")]
        [Then(@"Override is ""(.*)""")]
        public void ThenOverrideIs(string expectedNumberOfOverrides)
        {
            var deployStatsViewerViewModel = FeatureContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var overrides = deployStatsViewerViewModel.Overrides;
            Assert.AreEqual(expectedNumberOfOverrides, overrides);
        }



        [Given(@"I have deploy tab opened")]
        public void GivenIHaveDeployTabOpened()
        {
            var msg = new Mock<IPopupMessage>();
            GetPopup().Object.Show(msg.Object);
        }

        [Then(@"a warning message appears ""(.*)""")]
        public void ThenAWarningMessageAppears(string p0)
        {
            try
            {
                GetPopup().Verify(controller => controller.ShowDeployServerVersionConflict("1.0.0.0", "0.0.0.1"));
            }
            // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                var message = GetViewModel().ErrorMessage;
                Assert.AreEqual(p0, message);
            }
        }
    }
}
