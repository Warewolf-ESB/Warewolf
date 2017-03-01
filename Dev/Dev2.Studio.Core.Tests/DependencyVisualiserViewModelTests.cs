using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Common.DependencyVisualization;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DependencyVisualiserViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenAggregator_ShouldInitializePopupController()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            var fieldInfo = typeof(DependencyVisualiserViewModel).GetField("_popupController", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Test Result -----------------------
            Assert.IsNotNull(fieldInfo);
            Assert.IsInstanceOfType(fieldInfo.GetValue(dependencyVisualiserViewModel), typeof(PopupController));
            Assert.IsFalse(dependencyVisualiserViewModel.HasVariables);
            Assert.IsFalse(dependencyVisualiserViewModel.HasDebugOutput);
            Assert.IsFalse(dependencyVisualiserViewModel.TextVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AvailableWidth_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            var wasCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AvailableWidth")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.AvailableWidth = 10;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(10, dependencyVisualiserViewModel.AvailableWidth);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AvailableHeight_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            var wasCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AvailableHeight")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.AvailableHeight = 10;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(10, dependencyVisualiserViewModel.AvailableHeight);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDependsOnMe_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            var wasCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "GetDependsOnMe")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.GetDependsOnMe = true;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(dependencyVisualiserViewModel.GetDependsOnMe);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AllNodes_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            var wasCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AllNodes")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            var explorerItemNodeViewModels = new BindableCollection<IExplorerItemNodeViewModel>();
            dependencyVisualiserViewModel.AllNodes = explorerItemNodeViewModels;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
            Assert.IsTrue(ReferenceEquals(explorerItemNodeViewModels, dependencyVisualiserViewModel.AllNodes));

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDependsOnOther_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------
            var aggreMock = new Mock<IEventAggregator>();
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(aggreMock.Object);
            var wasGetDependsOnOtherCalled = false;
            var wasGetDependsOnMeCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "GetDependsOnOther")
                {
                    wasGetDependsOnOtherCalled = true;
                    var visualiserViewModel = (DependencyVisualiserViewModel)sender;
                    if (visualiserViewModel.GetDependsOnOther)
                        wasGetDependsOnMeCalled = true;

                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.GetDependsOnOther = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasGetDependsOnOtherCalled);
            dependencyVisualiserViewModel.GetDependsOnOther = true;
            Assert.IsTrue(wasGetDependsOnMeCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceModel_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object);
            var wasResourceModelCalled = false;
            var wasDisplayNameCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ResourceModel")
                {
                    wasResourceModelCalled = true;
                }
                if (args.PropertyName == "DisplayName")
                {
                    wasDisplayNameCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.ResourceModel = resourceModel.Object;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasResourceModelCalled);
            Assert.IsTrue(wasDisplayNameCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NestingLevel_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object);
            var wasNestingLevelCalled = false;
            dependencyVisualiserViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "NestingLevel")
                {
                    wasNestingLevelCalled = true;
                }

            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.NestingLevel = "0";
            //---------------Test Result -----------------------
            Assert.IsTrue(wasNestingLevelCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenDependsOnMe_ShouldBuildDisplayNameCorrectly()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object)
            {
                GetDependsOnMe = true,
                ResourceModel = resourceModel.Object
            };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.NestingLevel = "0";
            //---------------Test Result -----------------------
            Assert.AreEqual("Dependency - a", dependencyVisualiserViewModel.DisplayName);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenNotDependsOnMe_ShouldBuildDisplayNameCorrectly()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object)
            {
                GetDependsOnMe = false,
                ResourceModel = resourceModel.Object
            };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.NestingLevel = "0";
            //---------------Test Result -----------------------
            Assert.AreEqual("a*Dependencies", dependencyVisualiserViewModel.DisplayName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FavoritesLabel_GivenIsSet_ShouldNotifyOfPropertyChange()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object) { };


            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            dependencyVisualiserViewModel.ResourceModel = resourceModel.Object;
            //---------------Test Result -----------------------
            Assert.AreEqual("Show what depends on a", dependencyVisualiserViewModel.FavoritesLabel);
            Assert.AreEqual("Show what a depends on", dependencyVisualiserViewModel.DependantsLabel);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetItems_GivenEmptyNodes_ShouldZeroExploreNodes()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var server = new Mock<IServer>();
            var value = new Graph("a");
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object);
            var mock = new Mock<IExplorerItemNodeViewModel>();
            var explorerItemNodeViewModels = new List<ExplorerItemNodeViewModel>();
            var guids = new List<Guid>();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            var itemNodeViewModels = dependencyVisualiserViewModel.GetItems(new List<IDependencyVisualizationNode>(), mock.Object, explorerItemNodeViewModels, guids);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, itemNodeViewModels.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetItems_GivenNodesAndChilrenWithDependencies_ShouldReturnNodesAndChildDependencies()
        {
            //---------------Set up test pack-------------------

            var aggreMock = new Mock<IEventAggregator>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var depGrap = new Mock<IDependencyGraphGenerator>();
            var resourceId = Guid.NewGuid();
            var resourceId1 = Guid.NewGuid();
            var server = new Mock<IServer>();
            var mVm = new Mock<IMainViewModel>();
            var shell = new Mock<IShellViewModel>();
            var env = new Mock<IEnvironmentViewModel>();
            var exploreItm = new Mock<IExplorerItemViewModel>();
            exploreItm.SetupGet(model => model.ResourceName).Returns("a");
            exploreItm.SetupGet(model => model.ResourceType).Returns("a");
            exploreItm.SetupGet(model => model.ResourceId).Returns(resourceId);
            exploreItm.SetupGet(model => model.UnfilteredChildren).Returns(new BindableCollection<IExplorerItemViewModel>());

            var exploreItm1 = new Mock<IExplorerItemViewModel>();
            exploreItm1.SetupGet(model => model.ResourceName).Returns("a");
            exploreItm1.SetupGet(model => model.ResourceType).Returns("a");
            exploreItm1.SetupGet(model => model.ResourceId).Returns(resourceId1);
            exploreItm1.SetupGet(model => model.UnfilteredChildren).Returns(new BindableCollection<IExplorerItemViewModel>());
            env.SetupGet(model => model.UnfilteredChildren).Returns(new BindableCollection<IExplorerItemViewModel>()
            {
                exploreItm.Object,exploreItm1.Object
            });
            mVm.SetupGet(model => model.ExplorerViewModel.Environments).Returns(new BindableCollection<IEnvironmentViewModel>()
            {
                env.Object
            });
            CustomContainer.Register(mVm.Object);
            CustomContainer.Register(shell.Object);
            var value = new Graph("a");

            value.Nodes.Add(new DependencyVisualizationNode(resourceId.ToString(), 2, 2, true, false));
            value.Nodes.Add(new DependencyVisualizationNode(resourceId.ToString(), 2, 2, true, false));
            depGrap.Setup(generator => generator.BuildGraph(It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(value);
            resourceModel.SetupGet(model => model.ResourceName).Returns("a");
            var envMock = new Mock<IEnvironmentModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(repository => repository.GetDependenciesXmlAsync(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()))
                .Returns(ValueFunction);
            envMock.SetupGet(model => model.ResourceRepository).Returns(resourceRepo.Object);
            resourceModel.SetupGet(model => model.Environment).Returns(envMock.Object);
            var dependencyVisualiserViewModel = new DependencyVisualiserViewModel(depGrap.Object, aggreMock.Object, server.Object);
            var mock = new Mock<IExplorerItemNodeViewModel>();
            var explorerItemNodeViewModels = new List<ExplorerItemNodeViewModel>();
            var guids = new List<Guid>();
            var item = new DependencyVisualizationNode(resourceId.ToString(), 2, 2, true, true);
            item.NodeDependencies.Add(new DependencyVisualizationNode(resourceId1.ToString(), 50, 50, true, true));

            var nodes = new List<IDependencyVisualizationNode>
            {
                item,
            };
            dependencyVisualiserViewModel.ResourceModel = resourceModel.Object;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dependencyVisualiserViewModel);
            //---------------Execute Test ----------------------
            var itemNodeViewModels = dependencyVisualiserViewModel.GetItems(nodes, mock.Object, explorerItemNodeViewModels, guids);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, itemNodeViewModels.Count);
            Assert.AreEqual(2, explorerItemNodeViewModels.Count);
            Assert.AreEqual(2, guids.Count);
        }



        private Task<ExecuteMessage> ValueFunction()
        {
            var executeMessage = new ExecuteMessage();
            var fromResult = Task.FromResult(executeMessage);
            return fromResult;
        }


    }
}
