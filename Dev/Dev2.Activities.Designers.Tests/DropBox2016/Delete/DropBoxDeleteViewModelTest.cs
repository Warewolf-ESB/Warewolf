using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Delete;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.DropBox2016.Delete
{
    [TestClass]
    public class DropBoxDeleteViewModelTest
    {
        private DropBoxDeleteViewModel CreateMockViewModel()
        {
            var mock = new Mock<IResourceCatalog>();
            var sources = GetSources();
            mock.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(sources);
            var agg = new Mock<IEventAggregator>();
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(CreateModelItem(), agg.Object, mock.Object);
            return dropBoxDeleteViewModel;
        }

        private ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDropBoxDeleteActivity());
            return modelItem;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Construct")]
        public void DropBoxDeleteViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxDeleteViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxDeleteViewModel);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxDeleteViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Sources")]
        public void DropBoxDeleteViewModel_Sources_GivenANewDropBoxViewModel_ShouldHaveNotBeNull()
        {
            //------------Setup for test--------------------------
            var dropBoxDeleteViewModel = CreateMockViewModel();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxDeleteViewModel, typeof(ActivityDesignerViewModel));
            Assert.IsNotNull(dropBoxDeleteViewModel.Sources);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_DeletePath")]
        public void DropBoxDeleteViewModel_DeletePath_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //------------Setup for test--------------------------
            var dropBoxDeleteViewModel = CreateMockViewModel();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsNotNull(dropBoxDeleteViewModel.Sources);
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxDeleteViewModel.DeletePath));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Result")]
        public void DropBoxDeleteViewModel_Result_GivenActivityIsNew_ShouldBeNullOrEmpty()
        {
            //------------Setup for test--------------------------
            var dropBoxDeleteViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxDeleteViewModel.DeletePath));
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxDeleteViewModel.Result));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_SelectedSourceName")]
        public void DropBoxDeleteViewModel_SelectedSourceName_GivenActivityIsNewAndNoSourceSelected_ShouldBeNullOrEmpty()
        {
            //------------Setup for test--------------------------
            var dropBoxDeleteViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxDeleteViewModel.Result));
            Assert.IsNull(dropBoxDeleteViewModel.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Sources")]
        public void DropBoxDeleteViewModel_Sources_EditSource_PublishesMessage()
        {
            //------------Setup for test--------------------------

            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.Register(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, agg.Object, TestResourceCatalog.ResourceCatalog.Value) { SelectedSource = new DropBoxSource() };
            dropBoxDeleteViewModel.EditDropboxSourceCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Sources")]
        public void DropBoxDeleteViewModel_Sources_EditSource_OnlyAvailableIfSourceSelected()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, agg.Object, TestResourceCatalog.ResourceCatalog.Value);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropBoxDeleteViewModel.IsDropboxSourceSelected);
            dropBoxDeleteViewModel.SelectedSource = new DropBoxSource();
            Assert.IsTrue(dropBoxDeleteViewModel.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Sources")]
        public void DropBoxDeleteViewModel_Sources_EditSource_AvailableIfSourceSelected()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, agg.Object, TestResourceCatalog.ResourceCatalog.Value);

            //------------Assert Results-------------------------
            Assert.IsFalse(dropBoxDeleteViewModel.IsDropboxSourceSelected);
            dropBoxDeleteViewModel.SelectedSource = new DropBoxSource();
            Assert.IsTrue(dropBoxDeleteViewModel.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_DeletePath")]
        public void DropBoxDeleteViewModel_DeletePath_GivenIsSet_ShouldSetModelItemProperty()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();

            //------------Execute Test---------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, agg.Object, TestResourceCatalog.ResourceCatalog.Value);
            dropBoxDeleteViewModel.DeletePath = "A";

            //------------Assert Results-------------------------
            ModelProperty property = model.Properties["DeletePath"];
            if (property == null)
            {
                Assert.Fail("Property Does not exist");
            }
            var modelPropertyValue = property.ComputedValue;
            Assert.AreEqual("A", modelPropertyValue);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Result")]
        public void DropBoxDeleteViewModel_Result_GivenIsSet_ShouldSetModelItemProperty()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            //------------Execute Test---------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, agg.Object, TestResourceCatalog.ResourceCatalog.Value);
            dropBoxDeleteViewModel.Result = "A";

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
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_CreateOAuthSource")]
        public void DropBoxDeleteViewModel_CreateOAuthSource_GivenCanPublish_ShouldResfreshSources()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            var model = CreateModelItem();
            var mock = new Mock<IResourceCatalog>();
            mock.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(new List<IResource>()
            {
                new DropBoxSource(), new DropBoxSource()
            });
            //------------Execute Test---------------------------
            var mockVM = new DropBoxDeleteViewModel(model, agg.Object, mock.Object);
            mockVM.Sources.Clear();
            var count = mockVM.Sources.Count();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, count);

            mockVM.Sources = mockVM.LoadOAuthSources();
            Assert.AreEqual(2, mockVM.Sources.Count);
        }

        List<IResource> GetSources()
        {
            return new List<IResource> { new DropBoxSource { ResourceName = "bob" }, new DropBoxSource { ResourceName = "dave" } };
        }
    }
}
