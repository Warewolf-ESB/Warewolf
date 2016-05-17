using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Delete;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
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
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<DropBoxSource> { new DropBoxSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<DropBoxSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(CreateModelItem(), env.Object, agg.Object);
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
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.Register(mockShellViewModel.Object);
            
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, env.Object, agg.Object);
            dropBoxDeleteViewModel.SelectedSource = dropBoxDeleteViewModel.Sources.First();
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
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource))
                .Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                .Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, env.Object, agg.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(dropBoxDeleteViewModel.IsDropboxSourceSelected);
            dropBoxDeleteViewModel.SelectedSource = sources[1];
            Assert.IsTrue(dropBoxDeleteViewModel.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_Sources")]
        public void DropBoxDeleteViewModel_Sources_EditSource_AvailableIfSourceSelected()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            
            //------------Execute Test---------------------------
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, env.Object, agg.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(dropBoxDeleteViewModel.IsDropboxSourceSelected);
            dropBoxDeleteViewModel.SelectedSource = dropBoxDeleteViewModel.Sources[0];
            Assert.IsTrue(dropBoxDeleteViewModel.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxDeleteViewModel_DeletePath")]
        public void DropBoxDeleteViewModel_DeletePath_GivenIsSet_ShouldSetModelItemProperty()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            
            //------------Execute Test---------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, env.Object, agg.Object);
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
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();

            //------------Execute Test---------------------------
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dropBoxDeleteViewModel = new DropBoxDeleteViewModel(model, env.Object, agg.Object);
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
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<DropBoxSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new Mock<IResourceModel>().Object);

            var model = CreateModelItem();
            
            //------------Execute Test---------------------------
            var mockVM = new DropBoxDeleteViewModel(model, env.Object, agg.Object);
            mockVM.Sources.Clear();
            var count = mockVM.Sources.Count();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, count);

            mockVM.Sources = mockVM.LoadOAuthSources();
            Assert.AreEqual(2, mockVM.Sources.Count);
        }

        List<DropBoxSource> GetSources()
        {
            return new List<DropBoxSource> { new DropBoxSource { ResourceName = "bob" }, new DropBoxSource { ResourceName = "dave" } };
        }
    }
}
