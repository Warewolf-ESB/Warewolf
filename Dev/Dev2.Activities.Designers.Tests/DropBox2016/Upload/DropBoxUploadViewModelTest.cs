﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.DropBox2016.Upload;
using Dev2.Activities.DropBox2016;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
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
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new OauthSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns((IResourceRepository)mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            var dropBoxUploadViewModel = new DropBoxUploadViewModel(CreateModelItem(), (IEnvironmentModel)env.Object, (IEventAggregator)agg.Object);
            return dropBoxUploadViewModel;
        }

        private ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDropBoxUploadAcivtity());
            return modelItem;
        }
    
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DropBoxUploadViewModel_Construct")]
        public void DropBoxUploadViewModel_Construct_GivenNewInstance_ShouldBeActivityViewModel()
        {
            //------------Setup for test--------------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //------------Execute Test---------------------------
            Assert.IsNotNull(dropBoxUploadViewModel);
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dropBoxUploadViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SelectedOathSource_GivenANewDropBoxViewModel_ShouldHaveADefaultSelectedOauthSource()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dropBoxUploadViewModel, typeof(ActivityDesignerViewModel));
            //---------------Execute Test ----------------------
            var selectOAuthSource = dropBoxUploadViewModel.SelectOAuthSource;
            Assert.IsNotNull(selectOAuthSource);
            //---------------Test Result -----------------------
            Assert.AreEqual("Select a OAuth Source...".ToUpper(), selectOAuthSource.ResourceName.ToUpper());
            Assert.AreEqual(string.Empty, selectOAuthSource.Secret);
            Assert.AreEqual(String.Empty, selectOAuthSource.Key);
            Assert.IsNotNull(selectOAuthSource.ResourceID);
        }  
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewOathSource_GivenANewDropBoxViewModel_ShouldHaveADefaultNewOauthSource()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dropBoxUploadViewModel, typeof(ActivityDesignerViewModel));
            //---------------Execute Test ----------------------
            var newOAuthSource = dropBoxUploadViewModel.NewOAuthSource;
            Assert.IsNotNull(newOAuthSource);
            //---------------Test Result -----------------------
            Assert.AreEqual("New OAuth Source...".ToUpper(), newOAuthSource.ResourceName.ToUpper());
            Assert.AreEqual(string.Empty, newOAuthSource.Secret);
            Assert.AreEqual(String.Empty, newOAuthSource.Key);
            Assert.IsNotNull(newOAuthSource.ResourceID);
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
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.SelectedSourceName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_GivenActivityIsNew_ShouldBeDefaultToTrue()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(dropBoxUploadViewModel.SelectedSourceName));
            //---------------Execute Test ----------------------
            Assert.IsTrue(dropBoxUploadViewModel.OverWrite);
            //---------------Test Result -----------------------
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Add_GivenActivityIsNew_ShouldBeDefaultToFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxUploadViewModel.OverWrite);
            //---------------Execute Test ----------------------
            Assert.IsFalse(dropBoxUploadViewModel.Add);
            //---------------Test Result -----------------------
        }[TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Update_GivenActivityIsNew_ShouldBeDefaultToFalse()
        {
            //---------------Set up test pack-------------------
            var dropBoxUploadViewModel = CreateMockViewModel();
            //---------------Assert Precondition----------------
            Assert.IsFalse(dropBoxUploadViewModel.Add);
            //---------------Execute Test ----------------------
            Assert.IsFalse(dropBoxUploadViewModel.Update);
            //---------------Test Result -----------------------
        }
    }
}