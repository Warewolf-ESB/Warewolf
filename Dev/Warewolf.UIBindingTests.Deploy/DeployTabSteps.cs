
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Common;
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
        [BeforeScenario(@"DeployTab")]
        public static void SetupForSystem()
        {
            Core.Utils.SetupResourceDictionary();
            var shell = GetMockShellVm(true, "localhost");
            var shellViewModel = GetMockShellVm(false, "DestinationServer");
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dest = new DeployDestinationViewModelForTesting(shellViewModel, GetMockAggegator());
            dest.ConnectControlViewModel.SelectedConnection = shellViewModel.ActiveServer;
            ScenarioContext.Current.Add("ConnectControl", dest.ConnectControlViewModel.SelectedConnection);
            
            ScenarioContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest)) { Children = new List<IExplorerItemViewModel> { CreateExplorerVms() } };
            ScenarioContext.Current["Src"] = src;
            var popupController = GetPopup().Object;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, popupController);
            ScenarioContext.Current["vm"] = vm;
            ScenarioContext.Current["stats"] = stats;
        }

        [AfterScenario(@"DeployTab")]
        public void Cleanup()
        {
            var shell = GetMockShellVm(true, "localhost");
            var dest = new DeployDestinationViewModelForTesting(GetMockShellVm(false, "DestinationServer"), GetMockAggegator());
            ScenarioContext.Current["Destination"] = dest;
            var stats = new DeployStatsViewerViewModel(dest);
            var src = new DeploySourceExplorerViewModelForTesting(shell, GetMockAggegator(), GetStatsVm(dest))
            {
                Children = new List<IExplorerItemViewModel> { CreateExplorerVms() }
            };
            ScenarioContext.Current["Src"] = src;
            var vm = new SingleExplorerDeployViewModel(dest, src, new List<IExplorerTreeItem>(), stats, shell, GetPopup().Object);
            vm.Destination.SelectedEnvironment.Children = new ObservableCollection<IExplorerItemViewModel>();
            ScenarioContext.Current["vm"] = vm;
            GetPopup().Setup(a => a.ShowDeployConflict(It.IsAny<int>()));
            GetPopup().Setup(a => a.ShowDeployNameConflict(It.IsAny<string>()));
            vm.PopupController = GetPopup().Object;
            _deployed = false;
        }

        static Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController> GetPopup()
        {
            var containsKey = ScenarioContext.Current.ContainsKey("Popup");
            if (containsKey)
            {
                var popUp = ScenarioContext.Current["Popup"];
                return (Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>)popUp;
            }
            var popup = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            ScenarioContext.Current["Popup"] = popup;
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
            var containsKey = ScenarioContext.Current.ContainsKey("localhost");
            if (!containsKey)
            {
                if (name.Equals("LocalHost", StringComparison.CurrentCultureIgnoreCase))
                {
                    shell.Setup(a => a.LocalhostServer).Returns(GetMockServer(name));
                }
            }
            else
            {
                var server = ScenarioContext.Current.Get<Mock<IServer>>("localhost");
                var mock = new Mock<IServer>();
                mock.SetupGet(p => p.DisplayName).Returns(name);
                mock.SetupGet(p => p.ResourceName).Returns(name);
                mock.SetupGet(p => p.ResourceID).Returns(Guid.NewGuid);
                mock.SetupGet(p => p.EnvironmentID).Returns(Guid.NewGuid);
                mock.SetupGet(p => p.IsConnected).Returns(true);
                mock.SetupGet(p => p.Permissions).Returns(new List<IWindowsGroupPermission>());
                mock.SetupGet(p => p.CanDeployTo).Returns(true);
                mock.SetupGet(p => p.CanDeployFrom).Returns(true);
                shell.Setup(a => a.LocalhostServer).Returns(server.Object);
                shell.Setup(a => a.ActiveServer).Returns(mock.Object);
                if (!ScenarioContext.Current.ContainsKey("mockDestServer"))
                    ScenarioContext.Current.Add("mockDestServer", mock);
            }

            shell.Setup(a => a.DeployResources(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IList<Guid>>(), It.IsAny<bool>())).Callback(() =>
            {
                _deployed = true;
            });
            if (setContext)
            {
                ScenarioContext.Current["Shell"] = shell;
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
                if (!ScenarioContext.Current.ContainsKey(name))
                {
                    ScenarioContext.Current.Add(name, server);
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
            server.Setup(a => a.CanDeployFrom).Returns(true);
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
            ScenarioContext.Current["DestinationServer"] = server;
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
                    , ResourcePath = "Examples\\Utility - Date and Time",
                        ResourceType = "WorkflowService"
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
                        ResourceName = "Utility - Date and Time", ResourcePath = "Examples\\Utility - Date and Time", ResourceType = "WorkflowService"
                    }
                }
            };
            return ax;
        }

        [Given(@"selected Source Server is ""(.*)""")]
        public void GivenSelectedSourceServerIs(string selectedSourceServer)
        {
            var displayName = GetViewModel().Source.SelectedServer.DisplayName;
            Assert.IsTrue(displayName.ToLower().Equals(selectedSourceServer.ToLower()));
        }

        SingleExplorerDeployViewModel GetViewModel()
        {
            var view = (SingleExplorerDeployViewModel)ScenarioContext.Current["vm"];
            return view;
        }

        [When(@"destination Server Version is ""(.*)""")]
        public void WhenDestinationServerVersionIs(string destinationServer)
        {
            GetPopup().Setup(controller => controller.ShowDeployServerVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.OK);
            GetViewModel().PopupController = GetPopup().Object;
            var dest = GetDestinationServer();
            dest.Setup(server => server.GetMinSupportedVersion()).Returns(destinationServer);
            dest.Setup(server => server.GetServerVersion()).Returns(destinationServer);
            
        }

        Mock<IServer> GetDestinationServer(string destname = "DestinationServer")
        {
            return (Mock<IServer>)ScenarioContext.Current[destname];
        }

        [When(@"selected Destination Server is ""(.*)""")]
        [Then(@"selected Destination Server is ""(.*)""")]
        public void WhenSelectedDestinationServerIs(string destinationServerName)
        {
            var displayName = GetViewModel().Destination.SelectedServer.DisplayName;
            Assert.AreEqual(destinationServerName, displayName);
        }

        [Then(@"I select Destination Server as ""(.*)""")]
        [When(@"I select Destination Server as ""(.*)""")]
        [Given(@"I select Destination Server as ""(.*)""")]
        public void ThenISelectDestinationServerAs(string p0)
        {
            if (p0.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(p0);

                var deployDestinationExplorerViewModel = GetViewModel().Destination;
                deployDestinationExplorerViewModel.ConnectControlViewModel.SelectedConnection = mockServer.Object;
                Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
            }
            else
            {
                var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(p0);
                mockServer.SetupGet(server => server.DisplayName).Returns(p0);
                mockServer.SetupGet(server => server.ResourceName).Returns(p0);
                var envMock = new Mock<IEnvironmentViewModel>();
                envMock.SetupGet(model => model.Server).Returns(mockServer.Object);
                envMock.Setup(model => model.AsList()).Returns(new List<IExplorerItemViewModel>());
                envMock.Setup(model => model.UnfilteredChildren).Returns(new ObservableCollection<IExplorerItemViewModel>());
                envMock.SetupGet(model => model.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
                var deployDestinationExplorerViewModel = GetViewModel().Destination;
                deployDestinationExplorerViewModel.SelectedEnvironment = envMock.Object;
                deployDestinationExplorerViewModel.ConnectControlViewModel.SelectedConnection.EnvironmentID = Guid.NewGuid();
                Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
            }

        }

        [Then(@"I select Destination Server as ""(.*)"" with items")]
        [When(@"I select Destination Server as ""(.*)"" with items")]
        [Given(@"I select Destination Server as ""(.*)"" with items")]
        public void ThenISelectDestinationServerAsWithItems(string selectedDestinationServer)
        {
            if (selectedDestinationServer.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(selectedDestinationServer);

                var deployDestinationExplorerViewModel = GetViewModel().Destination;
                deployDestinationExplorerViewModel.ConnectControlViewModel.SelectedConnection = mockServer.Object;
                Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
            }
            else
            {
                var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(selectedDestinationServer);
                mockServer.SetupGet(server => server.DisplayName).Returns(selectedDestinationServer);
                mockServer.SetupGet(server => server.ResourceName).Returns(selectedDestinationServer);
                var deployDestinationExplorerViewModel = GetViewModel().Destination;
                var envMock = new Mock<IEnvironmentViewModel>();
                envMock.SetupGet(model => model.Server).Returns(mockServer.Object);
                envMock.SetupGet(model => model.IsConnected).Returns(true);
                envMock.Setup(model => model.AsList()).Returns(deployDestinationExplorerViewModel.Environments[0].AsList());
                envMock.Setup(model => model.UnfilteredChildren).Returns(deployDestinationExplorerViewModel.Environments[0].AsList().ToObservableCollection());
                envMock.SetupGet(model => model.Children).Returns(deployDestinationExplorerViewModel.Environments[0].Children);
                deployDestinationExplorerViewModel.SelectedEnvironment = envMock.Object;
                deployDestinationExplorerViewModel.ConnectControlViewModel.SelectedConnection.EnvironmentID = Guid.NewGuid();
                Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
            }

        }

        [Then(@"I select Destination Server as ""(.*)"" with SameName confilcts")]
        [When(@"I select Destination Server as ""(.*)"" with SameName confilcts")]
        public void ThenISelectDestinationServerAsWithSameNameConfilcts(string selectedDestinationServer)
        {
            var deployViewModel = (IDeployViewModel)ScenarioContext.Current["vm"];
            var explorerItemViewModel = deployViewModel.Source.SelectedEnvironment.AsList();
            var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(selectedDestinationServer);
            mockServer.SetupGet(server => server.DisplayName).Returns(selectedDestinationServer);
            mockServer.SetupGet(server => server.ResourceName).Returns(selectedDestinationServer);
            var itemViewModel = explorerItemViewModel.First(model => model.ResourceName.Equals("Control Flow - Sequence", StringComparison.InvariantCultureIgnoreCase));
            var envMock = new Mock<IEnvironmentViewModel>();
            var item = new Mock<IExplorerItemViewModel>();
            item.SetupGet(model => model.ResourceName).Returns("DifferentNameSameID");
            item.SetupGet(model => model.ResourcePath).Returns(itemViewModel.ResourcePath);
            item.SetupGet(model => model.ResourceId).Returns(itemViewModel.ResourceId);
            var observableCollection = new ObservableCollection<IExplorerItemViewModel>()
            {
                item.Object
            };
            envMock.SetupGet(model => model.Server).Returns(mockServer.Object);
            envMock.Setup(model => model.AsList()).Returns(observableCollection);
            envMock.Setup(model => model.UnfilteredChildren).Returns(observableCollection);
            envMock.SetupGet(model => model.Children).Returns(observableCollection);
            var deployDestinationExplorerViewModel = GetViewModel().Destination;
            deployDestinationExplorerViewModel.SelectedEnvironment = envMock.Object;
            Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
        }

        [Then(@"I select Destination Server as ""(.*)"" with SameName different ID confilcts")]
        [When(@"I select Destination Server as ""(.*)"" with SameName different ID confilcts")]
        public void ThenISelectDestinationServerAsWithSameNameDifferentIDConfilcts(string selectedDestinationServer)
        {
            var deployViewModel = (IDeployViewModel)ScenarioContext.Current["vm"];
            var explorerItemViewModel = deployViewModel.Source.SelectedEnvironment.AsList();
            var mockServer = ScenarioContext.Current.Get<Mock<IServer>>(selectedDestinationServer);
            mockServer.SetupGet(server => server.DisplayName).Returns(selectedDestinationServer);
            mockServer.SetupGet(server => server.ResourceName).Returns(selectedDestinationServer);
            var itemViewModel = explorerItemViewModel.First(model => model.ResourceName.Equals("Control Flow - Sequence", StringComparison.InvariantCultureIgnoreCase));
            var envMock = new Mock<IEnvironmentViewModel>();
            var item = new Mock<IExplorerItemViewModel>();
            item.SetupGet(model => model.ResourceName).Returns("DifferentNameSameID");
            item.SetupGet(model => model.ResourcePath).Returns(itemViewModel.ResourcePath);
            item.SetupGet(model => model.ResourceId).Returns(Guid.NewGuid());
            var observableCollection = new ObservableCollection<IExplorerItemViewModel>()
            {
                item.Object
            };
            envMock.SetupGet(model => model.Server).Returns(mockServer.Object);
            envMock.Setup(model => model.AsList()).Returns(observableCollection);
            envMock.Setup(model => model.UnfilteredChildren).Returns(observableCollection);
            envMock.SetupGet(model => model.Children).Returns(observableCollection);
            var deployDestinationExplorerViewModel = GetViewModel().Destination;
            deployDestinationExplorerViewModel.SelectedEnvironment = envMock.Object;
            Assert.IsNotNull(deployDestinationExplorerViewModel.SelectedEnvironment);
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
            var contains = errorMsg.Contains("Deployed Successfully.");
            Assert.IsTrue(contains);
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string expectedValue)
        {
            var singleExplorerDeployViewModel = GetViewModel();
            var CanDeploy = singleExplorerDeployViewModel.DeployCommand.CanExecute(null) ? "Enabled" : "Disabled";
            Assert.AreEqual(expectedValue, CanDeploy);

        }

        [When(@"I Select All Dependecies")]
        public void WhenISelectAllDependecies()
        {
            GetViewModel().SelectDependenciesCommand.Execute(null);
        }

        [Then(@"Select All Dependencies is ""(.*)""")]
        public void ThenSelectAllDependenciesIs(string expectedValue)
        {
            var canExecute = GetViewModel().SelectDependenciesCommand.CanExecute(null);
            Assert.AreEqual(canExecute, Boolean.Parse(expectedValue));
        }

        [Given(@"source is connected")]
        [Then(@"source is connected")]
        [When(@"source is connected")]
        public void GivenSourceIsConnected()
        {
            var isConnected = GetViewModel().Source.SelectedServer.IsConnected;
            Assert.IsTrue(isConnected);
        }

        [Given(@"destination ""(.*)"" is connected")]
        [When(@"destination ""(.*)"" is connected")]
        [Then(@"destination ""(.*)"" is connected")]
        public void ThenDestinationIsConnected(string selectedServer)
        {
            var server = ScenarioContext.Current.Get<Mock<IServer>>(selectedServer);
            ScenarioContext.Current["ConnectControl"] = server.Object;
        }
        
        [Given(@"I deploy")]
        [When(@"I deploy")]
        [Then(@"I deploy")]
        public void WhenIDeploy()
        {
            GetViewModel().DeployCommand.Execute(null);
        }

        [When(@"I Unselect ""(.*)"" from Source Server")]
        public void WhenIUnselectFromSourceServer(string resourceName)
        {
            var explorerTreeItem = GetViewModel().Source.SelectedItems.First(item => item.ResourceName.Contains(resourceName));
            GetViewModel().Source.SelectedItems.Remove(explorerTreeItem);
        }

        [When(@"I select ""(.*)"" from Source Server")]
        [Then(@"I select ""(.*)"" from Source Server")]
        [Given(@"I select ""(.*)"" from Source Server")]
        public void WhenISelectFromSourceServer(string resourceName)
        {
            var viewModel = GetViewModel();
            var explorerItemViewModels = viewModel.Source
                .SelectedEnvironment.Children[0].Children;
            var explorerItemViewModel = explorerItemViewModels
                                                 .FirstOrDefault(model => model.ResourceName.Contains(resourceName));
            if(explorerItemViewModel != null)
            {
                explorerItemViewModel.CanDeploy = true;
                explorerItemViewModel.IsResourceChecked = true;
                ((DeploySourceExplorerViewModelForTesting)viewModel.Source).SetSelecetdItems(new List<IExplorerTreeItem>
                {
                    explorerItemViewModel
                });
            }

            ThenCalculationIsInvoked();
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
            //ConnectControl
            var destinationServer = GetDestinationServer("mockDestServer");
            SetDestPermisions(deployFrom, deployTo, destinationServer);
        }

        private void SetDestPermisions(bool deployFrom, bool deployTo, Mock<IServer> Mockserver)
        {
            var destinationServer = Mockserver;
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
        public void ThenServicesIs(string expectedServicesCount)
        {
            var deployStatsViewerViewModel = ScenarioContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var services = deployStatsViewerViewModel.Services.ToString();
            Assert.AreEqual(services, expectedServicesCount);
        }

        [Then(@"Sources is ""(.*)""")]
        public void ThenSourcesIs(string expectedSourcesCount)
        {
            var deployStatsViewerViewModel = ScenarioContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var sources = deployStatsViewerViewModel.Sources.ToString();
            Assert.AreEqual(sources, expectedSourcesCount);
        }

        [Then(@"Calculation is invoked")]
        [When(@"Calculation is invoked")]
        public void ThenCalculationIsInvoked()
        {
            var viewModel = GetViewModel();
            var deployStatsViewerViewModel = ScenarioContext.Current.Get<DeployStatsViewerViewModel>("stats");
            deployStatsViewerViewModel.Calculate(new List<IExplorerTreeItem>(viewModel.Source.SelectedItems));
        }

        [Then(@"New Resource is ""(.*)""")]
        public void ThenNewResourceIs(string expectedCount)
        {
            var deployStatsViewerViewModel = ScenarioContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var actualCount = deployStatsViewerViewModel.New.Count.ToString();
            Assert.AreEqual(expectedCount, actualCount);
        }

        [Given(@"Override is ""(.*)""")]
        [When(@"Override is ""(.*)""")]
        [Then(@"Override is ""(.*)""")]
        public void ThenOverrideIs(string expectedNumberOfOverrides)
        {
            var deployStatsViewerViewModel = ScenarioContext.Current.Get<DeployStatsViewerViewModel>("stats");
            var overrides = deployStatsViewerViewModel.Overrides.ToString();
            Assert.AreEqual(expectedNumberOfOverrides, overrides);
        }

        [Given(@"I have deploy tab opened")]
        public void GivenIHaveDeployTabOpened()
        {
            var msg = new Mock<IPopupMessage>();
            GetPopup().Object.Show(msg.Object);
        }

        [Then(@"a warning message appears ""(.*)""")]
        public void ThenAWarningMessageAppears(string expectedMessage)
        {
            try
            {
                GetPopup().Verify(controller => controller.ShowDeployServerVersionConflict("1.0.0.0", "0.0.0.1"));
            }
            // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                var message = GetViewModel().ErrorMessage;
                Assert.AreEqual(expectedMessage, message);
            }
        }
    }
}
