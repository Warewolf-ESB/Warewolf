using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Upload;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.DropBox2016.Upload
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DropBoxUploadViewModelTest
    {
        private DropBoxUploadViewModel CreateMockViewModel()
        {
            var dropBoxSourceManager = new Mock<IDropboxSourceManager>();
            var agg = new Mock<IEventAggregator>();
            var dropBoxUploadViewModel = new DropBoxUploadViewModel(CreateModelItem(), dropBoxSourceManager.Object);
            return dropBoxUploadViewModel;
        }

        private ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDropBoxUploadActivity());
            return modelItem;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DropBoxUploadViewModel_Construct")]
        public void DropBoxUploadViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            dropBoxUploadViewModel.Validate();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxUploadViewModel);
            Assert.IsFalse(dropBoxUploadViewModel.ShowLarge);
            Assert.AreEqual(dropBoxUploadViewModel.ThumbVisibility, Visibility.Collapsed);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxUploadViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxUploadViewModel_Handle")]
        public void DropBoxUploadViewModel_UpdateHelp_ShouldCallToHelpViewMode()
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
        public void Sources_GivenANewDropBoxViewModel_ShouldHaveNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dropBoxUploadViewModel, typeof(ActivityDesignerViewModel));
            //---------------Execute Test ----------------------
            Assert.IsNotNull(dropBoxUploadViewModel.Sources);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FromPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxUploadViewModel.Sources);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.FromPath));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.FromPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.ToPath));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.Result));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedSourceName_GivenActivityIsNewAndNoSourceSelected_ShouldBeNullOrEmpty()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.Result));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(dropBoxUploadViewModel.SelectedSource);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_GivenActivityIsNew_ShouldBeDefaultToTrue()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(dropBoxUploadViewModel.OverWriteMode);
            //---------------Test Result -----------------------
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Add_GivenActivityIsNew_ShouldBeDefaultToFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxUploadViewModel.OverWriteMode);
            //---------------Execute Test ----------------------
            Assert.IsFalse(dropBoxUploadViewModel.AddMode);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxUploadViewModel_EditSourcePublishesMessage()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()));
            mockShellViewModel.Setup(viewModel => viewModel.ActiveServer).Returns(() => serverMock.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            //------------Setup for test--------------------------
            var dropBoxUploadViewModel = new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value) { SelectedSource = new DropBoxSource() };
            dropBoxUploadViewModel.EditDropboxSourceCommand.Execute(null);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.DeRegister<IShellViewModel>();
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxUploadViewModel_NewSourcePublishesMessage()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            CustomContainer.DeRegister<IShellViewModel>();
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(viewModel => viewModel.NewDropboxSource(It.IsAny<string>()));
            CustomContainer.Register(shellViewModelMock.Object);
            //------------Setup for test--------------------------
            var dropBoxUploadViewModel = new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value) { SelectedSource = new DropBoxSource() };
            //------------Execute Test---------------------------
            dropBoxUploadViewModel.NewSourceCommand.Execute(null);
            //------------Assert Results-------------------------
            shellViewModelMock.Verify(viewModel => viewModel.NewDropboxSource(It.IsAny<string>()), Times.Once);
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxUploadViewModel_EditSourceOnlyAvailableIfSourceSelected()
        {
            var agg = new Mock<IEventAggregator>();
            var sources = GetSources();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var dropBoxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);
            Assert.IsFalse(dropBoxUploadViewModel.IsDropboxSourceSelected);
            dropBoxUploadViewModel.SelectedSource = sources[1] as DropBoxSource;
            Assert.IsTrue(dropBoxUploadViewModel.IsDropboxSourceSelected);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxUploadViewModel_EditSourceAvailableIfSourceSelected()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);
            Assert.IsFalse(boxUploadViewModel.IsDropboxSourceSelected);
            boxUploadViewModel.SelectedSource =new DropBoxSource();
            Assert.IsTrue(boxUploadViewModel.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddMode_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

            //------------Execute Test---------------------------
            boxUploadViewModel.AddMode = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["AddMode"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = Convert.ToBoolean(property.ComputedValue);
            Assert.IsTrue(modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OverwriteModel_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

            //------------Execute Test---------------------------
            boxUploadViewModel.OverWriteMode = true;
            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["OverWriteMode"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = Convert.ToBoolean(property.ComputedValue);
            Assert.IsTrue(modelPropertyValue);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FromPath_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

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
        public void ToPath_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

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
        public void Result_GivenIsSet_ShouldSetModelItemProperty()
        {
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var boxUploadViewModel =new DropBoxUploadViewModel(model, TestResourceCatalog.LazySourceManager.Value);

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
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var mock = new Mock<IDropboxSourceManager>();
            mock.Setup(catalog => catalog.FetchSources<DropBoxSource>()).Returns(new List<DropBoxSource>()
            {
                new DropBoxSource(), new DropBoxSource()
            });
            // ReSharper disable once UseObjectOrCollectionInitializer
            var mockVM = new DropBoxUploadViewModel(model, mock.Object);
            //---------------Assert Precondition----------------
            mockVM.Sources.Clear();
            var count = mockVM.Sources.Count();
            Assert.AreEqual(0,count);
            //---------------Execute Test ----------------------
            mockVM.Sources = mockVM.LoadOAuthSources();
            //---------------Test Result -----------------------
            Assert.AreEqual(2, mockVM.Sources.Count);
        }
        
      

        List<IResource> GetSources()
        {
            return new List<IResource> { new DropBoxSource { ResourceName = "bob" }, new DropBoxSource { ResourceName = "dave" } };
        }
    }
}