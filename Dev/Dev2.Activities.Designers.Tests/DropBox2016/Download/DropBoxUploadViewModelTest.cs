﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Download;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
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
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new OauthSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            var dropBoxDownloadViewModel = new DropBoxDownloadViewModel(CreateModelItem(), env.Object, agg.Object);
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
        public void DropBoxDownloadViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxDownloadViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxDownloadViewModel);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxDownloadViewModel, typeof(ActivityDesignerViewModel));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Sources_GivenANewDropBoxViewModel_ShouldHaveNotBeNull()
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
        public void ToPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
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
        public void FromPath_GivenActivityIsNew_ShouldBeNullOrEmpty()
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
        public void Result_GivenActivityIsNew_ShouldBeNullOrEmpty()
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
        public void OverwriteFile_GivenActivityIsNew_ShouldBefalse()
        {
            //---------------Set up test pack-------------------
            var downloadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(downloadViewModel.ToPath));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(downloadViewModel.OverwriteFile);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedSourceName_GivenActivityIsNewAndNoSourceSelected_ShouldBeNullOrEmpty()
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
        public void downloadViewModel_EditSourcePublishesMessage()
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
            var downloadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);
            downloadViewModel.SelectedSource = downloadViewModel.Sources.First();
            downloadViewModel.EditDropboxSourceCommand.Execute(null);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<IServer>()));
            CustomContainer.DeRegister<IShellViewModel>();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectedOperation_EditSource")]
        public void downloadViewModel_EditSourceOnlyAvailableIfSourceSelected()
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
            var downloadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);
            Assert.IsFalse(downloadViewModel.IsDropboxSourceSelected);
            downloadViewModel.SelectedSource = sources[1];
            Assert.IsTrue(downloadViewModel.IsDropboxSourceSelected);

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
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);
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
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);

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
        public void Frompath_GivenIsSet_ShouldSetModelItemProperty()
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
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);

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
        public void OverwriteFile_GivenIsSet_ShouldSetModelItemProperty()
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
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);

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
            var boxUploadViewModel = new DropBoxDownloadViewModel(model, env.Object, agg.Object);

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
            var mockVM = new DropBoxDownloadViewModel(model, env.Object, agg.Object);
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
            var mockVM = new DropBoxDownloadViewModel(model, env.Object, agg.Object);
            //---------------Assert Precondition----------------
            mockVM.CreateOAuthSource();
            //---------------Execute Test ----------------------
            agg.Verify(aggregator => aggregator.Publish(It.IsAny<IMessage>()));
            //---------------Test Result -----------------------
        }



        List<OauthSource> GetSources()
        {
            return new List<OauthSource> { new OauthSource { ResourceName = "bob" }, new OauthSource { ResourceName = "dave" } };
        }
    }
}