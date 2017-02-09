using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharepointDesignerViewModelBaseTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(null, new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointListDesignerViewModelBase);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new TestSharepointListDesignerViewModelBase(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), null, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
        }  
      
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullEventAggregator_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object, null, false);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        public void SharepointListDesignerViewModelBase_Constructor_ParametersPassed_ShouldSetupCorrectly()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase);
            Assert.AreEqual(Visibility.Collapsed,sharepointListDesignerViewModelBase.ShowExampleWorkflowLink);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SharepointServers);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.EditSharepointServerCommand);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.RefreshListsCommand);
            Assert.IsTrue(sharepointListDesignerViewModelBase.RefreshListsCommand.CanExecute(null));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_MethodName")]
        public void SharepointListDesignerViewModelBase_SharepointServerList_ShouldHaveServerListWithNewSharepointOption()
        {
            //------------Setup for test--------------------------
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel();
            
            //------------Execute Test---------------------------
            var sharepointServerList = sharepointListDesignerViewModelBase.SharepointServers;
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointServerList);
            Assert.AreEqual(1,sharepointServerList.Count);
            Assert.AreEqual(sharepointListDesignerViewModelBase.GetNewSharepointSource, sharepointServerList[0]);
        }

        private static TestSharepointListDesignerViewModelBase CreateSharepointListDesignerViewModel()
        {
            return CreateSharepointListDesignerViewModel(new Mock<IEnvironmentModel>());
        }
        
        private static TestSharepointListDesignerViewModelBase CreateSharepointListDesignerViewModel(Mock<IEnvironmentModel> mockEnvironmentModel)
        {
            return CreateSharepointListDesignerViewModel(mockEnvironmentModel, new Mock<IEventAggregator>());
        }

        private static TestSharepointListDesignerViewModelBase CreateSharepointListDesignerViewModel(Mock<IEnvironmentModel> mockEnvironmentModel, Mock<IEventAggregator> mockEventAggregator)
        {
            return new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), mockEnvironmentModel.Object, mockEventAggregator.Object, false);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_LoadSharepointServers")]
        public void SharepointListDesignerViewModelBase_SharepointListDesignerViewModelBase_LoadSharepointServers_HasServers_ShouldPopulateList()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource>{sharepointSource};
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel);
            
            //------------Execute Test---------------------------
            var sharepointServers = sharepointListDesignerViewModelBase.SharepointServers;
            //------------Assert Results-------------------------
            Assert.AreEqual(3,sharepointServers.Count);
            Assert.AreEqual(sharepointListDesignerViewModelBase.GetSelectSharepointSource,sharepointServers[0]);
            Assert.AreEqual(sharepointListDesignerViewModelBase.GetNewSharepointSource,sharepointServers[1]);
            Assert.AreEqual(sharepointSource,sharepointServers[2]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_SetSelectedSharepointServer")]
        public void SharepointListDesignerViewModelBase_SetSelectedSharepointServer_SetToServer_ShouldLoadLists()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            var sharepointListTo = new SharepointListTo
            {
                FullName = "Share List",
                Fields = new List<ISharepointFieldTo>
                {
                    new SharepointFieldTo
                    {
                        InternalName = "Field 1",
                        IsEditable = false,
                        Name = "Name 1",
                        IsRequired = false,
                        Type = SharepointFieldType.Text
                    }
                }
            };
            var sharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(sharepointListTos);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel);
            //------------Execute Test---------------------------
            sharepointListDesignerViewModelBase.SelectedSharepointServer = sharepointSource;
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(2,sharepointListDesignerViewModelBase.Lists.Count);
            Assert.AreEqual(sharepointListDesignerViewModelBase.GetSelectAList, sharepointListDesignerViewModelBase.Lists[0]);
            Assert.AreEqual(sharepointListTo, sharepointListDesignerViewModelBase.Lists[1]);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_SetSelectedSharepointServer")]
        public void SharepointListDesignerViewModelBase_SetSelectedSharepointServer_EditCommand_ShouldCallOpenResource()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(),It.IsAny<IServer>())).Verifiable();
            var serverMock = new Mock<IServer>();
            mockShellViewModel.Setup(viewModel => viewModel.ActiveServer).Returns(() => serverMock.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel);
            sharepointListDesignerViewModelBase.SelectedSharepointServer = sharepointSource;
            //------------Execute Test---------------------------
            sharepointListDesignerViewModelBase.EditSharepointServerCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            mockShellViewModel.Verify(model => model.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_SetSelectedSharepointServer")]
        public void SharepointListDesignerViewModelBase_SetSelectedSharepointServer_SetToNewSharepointServer_ShouldPublishEvent()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);            
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.NewSharepointSource(It.IsAny<string>()));
            var shellViewModel = mockShellViewModel.Object;
            CustomContainer.Register(shellViewModel);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel,mockEventAggregator);
            //------------Execute Test---------------------------
            sharepointListDesignerViewModelBase.SelectedSharepointServer = sharepointListDesignerViewModelBase.GetNewSharepointSource;
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            mockShellViewModel.Verify(model => model.NewSharepointSource(It.IsAny<string>()));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_SetSelectedSharepointList")]
        public void SharepointListDesignerViewModelBase_SetSelectedList_SetToList_ShouldLoadFields()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            var sharepointFieldTos = new List<ISharepointFieldTo>
            {
                new SharepointFieldTo
                {
                    InternalName = "Field 1",
                    IsEditable = false,
                    Name = "Name 1",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                },
                new SharepointFieldTo
                {
                    InternalName = "fMyField",
                    IsEditable = false,
                    Name = "fMyField 1",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                },
                new SharepointFieldTo
                {
                    InternalName = "1 Field Name",
                    IsEditable = false,
                    Name = "1 Field Name",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                },
                new SharepointFieldTo
                {
                    InternalName = "_Field",
                    IsEditable = false,
                    Name = "_Field Name",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                },
                new SharepointFieldTo
                {
                    InternalName = "_Field_",
                    IsEditable = false,
                    Name = "Field_Name_1",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                }
            };
            var sharepointListTo = new SharepointListTo
            {
                FullName = "Share List",
                Fields = sharepointFieldTos
            };
            var sharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(sharepointListTos);
            mockResourceRepo.Setup(repository => repository.GetSharepointListFields(It.IsAny<ISharepointSource>(), It.IsAny<SharepointListTo>(), false)).Returns(sharepointFieldTos);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel);
            sharepointListDesignerViewModelBase.SelectedSharepointServer = sharepointSource;
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(2, sharepointListDesignerViewModelBase.Lists.Count);
            //------------Execute Test---------------------------
            sharepointListDesignerViewModelBase.SelectedList = sharepointListTo;
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedList);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.ListItems);
            Assert.AreEqual(5,sharepointListDesignerViewModelBase.ListItems.Count);
            Assert.AreEqual("Name 1",sharepointListDesignerViewModelBase.ListItems[0].FieldName);
            Assert.AreEqual("Field 1",sharepointListDesignerViewModelBase.ListItems[0].InternalName);
            Assert.AreEqual(SharepointFieldType.Text.ToString(), sharepointListDesignerViewModelBase.ListItems[0].Type);
            Assert.IsFalse(sharepointListDesignerViewModelBase.ListItems[0].IsRequired);
            Assert.AreEqual("[[ShareList(*).Name1]]", sharepointListDesignerViewModelBase.ListItems[0].VariableName);
            Assert.AreEqual("[[ShareList(*).MyField1]]", sharepointListDesignerViewModelBase.ListItems[1].VariableName);
            Assert.AreEqual("[[ShareList(*).x0031_FieldName]]", sharepointListDesignerViewModelBase.ListItems[2].VariableName);
            Assert.AreEqual("[[ShareList(*).FieldName]]", sharepointListDesignerViewModelBase.ListItems[3].VariableName);
            Assert.AreEqual("[[ShareList(*).Fiele_1]]", sharepointListDesignerViewModelBase.ListItems[4].VariableName);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_RefreshListsCommand")]
        public void SharepointListDesignerViewModelBase_RefreshListsCommand_ShouldReloadLists()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            var sharepointListTo = new SharepointListTo
            {
                FullName = "Share List",
                Fields = new List<ISharepointFieldTo>
                {
                    new SharepointFieldTo
                    {
                        InternalName = "Field 1",
                        IsEditable = false,
                        Name = "Name 1",
                        IsRequired = false,
                        Type = SharepointFieldType.Text
                    }
                }
            };
            var sharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(sharepointListTos);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointListDesignerViewModelBase = CreateSharepointListDesignerViewModel(mockEnvironmentModel);
            sharepointListDesignerViewModelBase.SelectedSharepointServer = sharepointSource;
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(2, sharepointListDesignerViewModelBase.Lists.Count);
            //------------Execute Test---------------------------
            var newSharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo,
                new SharepointListTo
                {
                    FullName = "Share List 2",
                    Fields = new List<ISharepointFieldTo>
                    {
                        new SharepointFieldTo
                        {
                            InternalName = "Field 2",
                            IsEditable = false,
                            Name = "Name 2",
                            IsRequired = false,
                            Type = SharepointFieldType.Text
                        }
                    }
                }
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(newSharepointListTos);
            sharepointListDesignerViewModelBase.RefreshListsCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(3, sharepointListDesignerViewModelBase.Lists.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_SetSelectedSharepointList")]
        public void SharepointListDesignerViewModelBase_Constructor_Activity_ShouldLoadFromActivity()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            var sharepointFieldTos = new List<ISharepointFieldTo>
            {
                new SharepointFieldTo
                {
                    InternalName = "Field 1",
                    IsEditable = false,
                    Name = "Name 1",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                }
            };
            var sharepointListTo = new SharepointListTo
            {
                FullName = "Share List",
                Fields = sharepointFieldTos
            };
            var sharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(sharepointListTos);
            mockResourceRepo.Setup(repository => repository.GetSharepointListFields(It.IsAny<ISharepointSource>(), It.IsAny<SharepointListTo>(), false)).Returns(sharepointFieldTos);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointReadListTos = new List<SharepointReadListTo>
            {
                new SharepointReadListTo("[[ShareList(*).Name1]]","Name 1","Field 1",SharepointFieldType.Text.ToString())
            };
            var modelItem = CreatePopulatedModelItem(sharepointSource.ResourceID,sharepointReadListTos,"Share List" );
            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(modelItem, new SynchronousAsyncWorker(), mockEnvironmentModel.Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(1, sharepointListDesignerViewModelBase.Lists.Count);            
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedList);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.ListItems);
            Assert.AreEqual(1, sharepointListDesignerViewModelBase.ListItems.Count);
            Assert.AreEqual("Name 1", sharepointListDesignerViewModelBase.ListItems[0].FieldName);
            Assert.AreEqual("Field 1", sharepointListDesignerViewModelBase.ListItems[0].InternalName);
            Assert.AreEqual(SharepointFieldType.Text.ToString(), sharepointListDesignerViewModelBase.ListItems[0].Type);
            Assert.IsFalse(sharepointListDesignerViewModelBase.ListItems[0].IsRequired);
            Assert.AreEqual("[[ShareList(*).Name1]]", sharepointListDesignerViewModelBase.ListItems[0].VariableName);
            Assert.AreEqual(2,sharepointListDesignerViewModelBase.ModelItemCollection.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_RemoveFilterItem")]
        public void SharepointListDesignerViewModelBase_RemoveFilterItem_ShouldRemoveFromCollection()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var sharepointSource = new SharepointSource
            {
                ResourceName = "SharepointServer1",
                ResourceID = Guid.NewGuid()
            };
            var sharepointSources = new List<SharepointSource> { sharepointSource };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<SharepointSource>(It.IsAny<IEnvironmentModel>(), enSourceType.SharepointServerSource)).Returns(sharepointSources);
            var sharepointFieldTos = new List<ISharepointFieldTo>
            {
                new SharepointFieldTo
                {
                    InternalName = "Field 1",
                    IsEditable = false,
                    Name = "Name 1",
                    IsRequired = false,
                    Type = SharepointFieldType.Text
                }
            };
            var sharepointListTo = new SharepointListTo
            {
                FullName = "Share List",
                Fields = sharepointFieldTos
            };
            var sharepointListTos = new List<SharepointListTo>
            {
                sharepointListTo
            };
            mockResourceRepo.Setup(repository => repository.GetSharepointLists(It.IsAny<SharepointSource>())).Returns(sharepointListTos);
            mockResourceRepo.Setup(repository => repository.GetSharepointListFields(It.IsAny<ISharepointSource>(), It.IsAny<SharepointListTo>(), false)).Returns(sharepointFieldTos);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var sharepointReadListTos = new List<SharepointReadListTo>
            {
                new SharepointReadListTo("[[ShareList(*).Name1]]","Name 1","Field 1",SharepointFieldType.Text.ToString())
            };
            var modelItem = CreatePopulatedModelItem(sharepointSource.ResourceID,sharepointReadListTos,"Share List" );
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(modelItem, new SynchronousAsyncWorker(), mockEnvironmentModel.Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Precondtion---------------------
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedSharepointServer);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.Lists);
            Assert.AreEqual(1, sharepointListDesignerViewModelBase.Lists.Count);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.SelectedList);
            Assert.IsNotNull(sharepointListDesignerViewModelBase.ListItems);
            Assert.AreEqual(1, sharepointListDesignerViewModelBase.ListItems.Count);
            Assert.AreEqual("Name 1", sharepointListDesignerViewModelBase.ListItems[0].FieldName);
            Assert.AreEqual("Field 1", sharepointListDesignerViewModelBase.ListItems[0].InternalName);
            Assert.AreEqual(SharepointFieldType.Text.ToString(), sharepointListDesignerViewModelBase.ListItems[0].Type);
            Assert.IsFalse(sharepointListDesignerViewModelBase.ListItems[0].IsRequired);
            Assert.AreEqual("[[ShareList(*).Name1]]", sharepointListDesignerViewModelBase.ListItems[0].VariableName);
            Assert.AreEqual(2, sharepointListDesignerViewModelBase.ModelItemCollection.Count);
            //------------Execute Test---------------------------
            sharepointListDesignerViewModelBase.RemoveAt(1);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, sharepointListDesignerViewModelBase.ModelItemCollection.Count);
            
        }


        static ModelItem CreateModelItem()
        {
            var readListActivity = new SharepointReadListActivity();
            return ModelItemUtils.CreateModelItem(readListActivity);
        }

        static ModelItem CreatePopulatedModelItem(Guid sourceId, List<SharepointReadListTo> readListItems, string listName)
        {
            var sharepointReadListActivity = new SharepointReadListActivity
            {
                FilterCriteria = new List<SharepointSearchTo>
                {
                    new SharepointSearchTo("Name 1", "=", "TestVal", 1)
                },
                ReadListItems = readListItems,
                SharepointList = listName,
                SharepointServerResourceId = sourceId
            };
            return ModelItemUtils.CreateModelItem(sharepointReadListActivity);
        }
    }

    public class TestSharepointListDesignerViewModelBase : SharepointListDesignerViewModelBase
    {
        public TestSharepointListDesignerViewModelBase(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher, bool loadOnlyEditableFields)
            : base(modelItem, asyncWorker, environmentModel, eventPublisher, loadOnlyEditableFields)
        {
            dynamic mi = ModelItem;
            InitializeItems(mi.FilterCriteria);
        }

        #region Overrides of ActivityDesignerViewModel

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        public SharepointSource GetSelectSharepointSource => SelectSharepointSource;
        public SharepointSource GetNewSharepointSource => NewSharepointSource;

        public SharepointListTo GetSelectAList => SelectSharepointList;

        #region Overrides of ActivityCollectionDesignerViewModel

        public override string CollectionName => "FilterCriteria";

        #endregion
    }
}
