using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Download;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.DropBox2016.Download
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DropBoxDownloadViewModelTest
    {
        private DropBoxDownloadViewModel CreateMockViewModel()
        {
            var dropBoxSourceManager = new Mock<IDropboxSourceManager>();
            var agg = new Mock<IEventAggregator>();
            var dropBoxDownloadViewModel = new DropBoxDownloadViewModel(CreateModelItem(), dropBoxSourceManager.Object);
            return dropBoxDownloadViewModel;
        }

        private ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDropBoxDownloadActivity());
            return modelItem;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DropBoxDownloadViewModel_Construct")]
        public void DropboxDownload_DropBoxDownloadViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxDownloadViewModel = CreateMockViewModel();
            dropBoxDownloadViewModel.Validate();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxDownloadViewModel);
            Assert.IsFalse(dropBoxDownloadViewModel.ShowLarge);
            Assert.AreEqual(dropBoxDownloadViewModel.ThumbVisibility, Visibility.Collapsed);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxDownloadViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDownloadViewModel_Handle")]
        public void DropBoxDownloadViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateMockViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_Sources_GivenANewDropBoxViewModel_ShouldHaveNotBeNull()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(downloadViewModel, typeof(ActivityDesignerViewModel));
            //---------------Execute Test ----------------------
            Assert.IsNotNull(downloadViewModel.Sources);
            //---------------Test Result -----------------------
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_ToPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.ToPath));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_FromPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.FromPath));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_Result_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.Result));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_SelectedSourceName_GivenActivityIsNewAndNoSourceSelected_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.Result));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(downloadViewModel.SelectedSource);
        }




        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxDownload_downloadViewModel_EditSourcePublishesMessage()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()));
            mockShellViewModel.Setup(viewModel => viewModel.ActiveServer).Returns(() => serverMock.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            //------------Setup for test--------------------------
            var downloadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value) { SelectedSource = new DropBoxSource() };
            downloadViewModel.EditDropboxSourceCommand.Execute(null);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxDownload_downloadViewModel_EditSourceOnlyAvailableIfSourceSelected()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var downloadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);
            Assert.IsFalse(downloadViewModel.IsDropboxSourceSelected);
            downloadViewModel.SelectedSource = new DropBoxSource();
            Assert.IsTrue(downloadViewModel.IsDropboxSourceSelected);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxDownload_downloadViewModel_EditSourceAvailableIfSourceSelected()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);
            Assert.IsFalse(boxUploadViewModel.IsDropboxSourceSelected);
            boxUploadViewModel.SelectedSource = new DropBoxSource();
            Assert.IsTrue(boxUploadViewModel.IsDropboxSourceSelected);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_ToPath_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

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
        public void DropboxDownload_Frompath_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

            //------------Execute Test---------------------------
            boxUploadViewModel.FromPath = "A";
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["FromPath"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual("A", modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_Result_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

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
        public void DropboxDownload_OverwriteFile_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

            //------------Execute Test---------------------------
            boxUploadViewModel.OverwriteFile = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["OverwriteFile"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual(true, modelPropertyValue);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxDownload_CreateOAuthSource_GivenCanPublish_ShouldResfreshSources()
        {
            var agg = new Mock<IEventAggregator>();

            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var mock = new Mock<IDropboxSourceManager>();
            mock.Setup(catalog => catalog.FetchSources<DropBoxSource>()).Returns(new List<DropBoxSource>()
            {
                new DropBoxSource(), new DropBoxSource()
            });
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mockVM = new DropBoxDownloadViewModel(model, mock.Object);
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
        public void DropboxDownload_CreateOAuthSource_GivenCanPublish_ShouldPublish()
        {
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            agg.Setup(aggregator => aggregator.Publish(It.IsAny<IMessage>()));
            var model = CreateModelItem();
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(viewModel => viewModel.NewDropboxSource(It.IsAny<string>()));
            CustomContainer.Register(shellViewModelMock.Object);
            //---------------Setup for test-----------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mockVM = new DropBoxDownloadViewModel(model, TestResourceCatalog.LazySourceManager.Value);
            //---------------Assert Precondition------------------

            //---------------Execute Test ------------------------
            mockVM.CreateOAuthSource();
            //---------------Test Result -------------------------
            shellViewModelMock.Verify(viewModel => viewModel.NewDropboxSource(It.IsAny<string>()),Times.Once);
        }
    }
}