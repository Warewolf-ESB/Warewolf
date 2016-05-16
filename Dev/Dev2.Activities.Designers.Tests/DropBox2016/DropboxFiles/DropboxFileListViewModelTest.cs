using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.DropboxFile;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.DropBox2016.DropboxFiles
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DropboxFileListViewModelTest
    {

        const string AppLocalhost = "http://localhost:3142";
        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = AppLocalhost;
        }

        private DropBoxFileListDesignerViewModel CreateMockViewModel()
        {
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new DropBoxSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            var dropBoxDownloadViewModel = new DropBoxFileListDesignerViewModel(CreateModelItem(), env.Object, agg.Object);
            return dropBoxDownloadViewModel;
        }

        private ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDropboxFileListActivity());
            return modelItem;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropBoxFileListViewModel_GivenIsNew_ShouldReturnInstance()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new DropBoxSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            mock.SetupGet(a => a.ResourceRepository).Returns(mockResourceRepo.Object);
            EnvironmentRepository.Instance.ActiveEnvironment = mock.Object;
            var viewModel = new DropBoxFileListDesignerViewModel(CreateModelItem());
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DropBoxFileListViewModel_Construct")]
        public void DropBoxFileListViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxFileListViewModel);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxFileListViewModel, typeof(ActivityDesignerViewModel));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Sources_GivenANewDropBoxViewModel_ShouldHaveNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dropBoxFileListViewModel, typeof(ActivityDesignerViewModel));
            //---------------Execute Test ----------------------
            Assert.IsNotNull(dropBoxFileListViewModel.Sources);
            //---------------Test Result -----------------------
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.ToPath));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Files_GivenActivityIsNew_ShouldBeEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxFileListViewModel.Files);
            Assert.AreEqual(0, dropBoxFileListViewModel.Files.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.Result));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsRecursive_GivenActivityIsNew_ShouldBeFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.Result));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(dropBoxFileListViewModel.IsRecursive);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeMediaInfo_GivenActivityIsNew_ShouldBeFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.Result));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(dropBoxFileListViewModel.IncludeMediaInfo);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeDeleted_GivenActivityIsNew_ShouldBeFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsFalse(dropBoxFileListViewModel.IsRecursive);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(dropBoxFileListViewModel.IncludeDeleted);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesSelected_GivenActivityIsNew_ShouldBeTrue()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsFalse(dropBoxFileListViewModel.IncludeDeleted);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(dropBoxFileListViewModel.IsFilesSelected);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFoldersSelected_GivenActivityIsNew_ShouldBeFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxFileListViewModel.IsFilesSelected);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(dropBoxFileListViewModel.IsFoldersSelected);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesAndFoldersSelected_GivenActivityIsNew_ShouldBeFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsFalse(dropBoxFileListViewModel.IsFoldersSelected);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(dropBoxFileListViewModel.IsFilesAndFoldersSelected);
        }
       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedSourceName_GivenActivityIsNewAndNoSourceSelected_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxFileListViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxFileListViewModel.Result));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(dropBoxFileListViewModel.SelectedSource);
        }




        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void dropBoxFileListViewModel_EditSourcePublishesMessage()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.Register(mockShellViewModel.Object);
            //------------Setup for test--------------------------
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);
            dropBoxFileListViewModel.SelectedSource = dropBoxFileListViewModel.Sources.First();
            dropBoxFileListViewModel.EditDropboxSourceCommand.Execute(null);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void dropBoxFileListViewModel_EditSourceOnlyAvailableIfSourceSelected()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource))
                .Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                .Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);
            Assert.IsFalse(dropBoxFileListViewModel.IsDropboxSourceSelected);
            dropBoxFileListViewModel.SelectedSource = sources[1];
            Assert.IsTrue(dropBoxFileListViewModel.IsDropboxSourceSelected);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void downloadViewModel_EditSourceAvailableIfSourceSelected()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var boxUploadViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);
            Assert.IsFalse(boxUploadViewModel.IsDropboxSourceSelected);
            boxUploadViewModel.SelectedSource = boxUploadViewModel.Sources[0];
            Assert.IsTrue(boxUploadViewModel.IsDropboxSourceSelected);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            boxUploadViewModel.ToPath = "A";
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["ToPath"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual("A", modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeMediaInfo_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IncludeMediaInfo = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IncludeMediaInfo"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsRecursive_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IsRecursive = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IsRecursive"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IncludeDeleted_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IncludeDeleted = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IncludeDeleted"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesSelected_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IsFilesSelected = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IsFilesSelected"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFoldersSelected_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IsFoldersSelected = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IsFoldersSelected"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesAndFoldersSelected_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxFileListViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            dropBoxFileListViewModel.IsFilesAndFoldersSelected = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["IsFilesAndFoldersSelected"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_GivenIsSet_ShouldSetModelItemProperty()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);

            //------------Execute Test---------------------------
            boxUploadViewModel.Result = "A";
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["Result"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual("A", modelPropertyValue);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateOAuthSource_GivenCanPublish_ShouldResfreshSources()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);

            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mockVM = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);
            //---------------Assert Precondition----------------
            mockVM.Sources.Clear();
            var count = mockVM.Sources.Count();
            Assert.AreEqual(0, count);
            //---------------Execute Test ----------------------
            mockVM.Sources = mockVM.LoadOAuthSources();
            //---------------Test Result -----------------------
            Assert.AreEqual(2, mockVM.Sources.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateOAuthSource_GivenCanPublish_ShouldPusbilsh()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            agg.Setup(aggregator => aggregator.Publish(It.IsAny<IMessage>()));
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mockVM = new DropBoxFileListDesignerViewModel(model, env.Object, agg.Object);
            //---------------Assert Precondition----------------
            mockVM.CreateOAuthSource();
            //---------------Execute Test ----------------------
            agg.Verify(aggregator => aggregator.Publish(It.IsAny<IMessage>()));
            //---------------Test Result -----------------------
        }



        List<OauthSource> GetSources()
        {
            return new List<OauthSource> { new DropBoxSource { ResourceName = "bob" }, new DropBoxSource { ResourceName = "dave" } };
        }
    }
}