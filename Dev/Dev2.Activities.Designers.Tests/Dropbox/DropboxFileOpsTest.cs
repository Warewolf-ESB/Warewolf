﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.DropBox.Upload;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Dropbox
{
    [TestClass]
    public class DropboxFileOpsTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxFileUploadViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void DropboxFileUploadViewModel_Ctor()
        
        {
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new OauthSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(CreateModelItem(),env.Object,agg.Object);
            Assert.AreEqual(fileOps.Operations[0],"Read File");
            Assert.AreEqual(fileOps.Operations[1], "Write File");
            Assert.AreEqual(3,fileOps.Sources.Count);
            Assert.AreEqual("Dropbox Source",fileOps.Sources[2].ResourceName);
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxFileUploadViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void DropboxFileUploadViewModel_SelectASource_WhenConstructor()
        
        {
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<OauthSource> { new OauthSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.FindSourcesByType<OauthSource>(It.IsAny<IEnvironmentModel>(), enSourceType.OauthSource)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            var agg = new Mock<IEventAggregator>();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(CreateModelItem(),env.Object,agg.Object);
            Assert.AreEqual(fileOps.Operations[0],"Read File");
            Assert.AreEqual(fileOps.Operations[1], "Write File");
            Assert.AreEqual(3,fileOps.Sources.Count);
            Assert.AreEqual("Select a OAuth Source...", fileOps.SelectedSource.ResourceName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxFileUploadViewModel_Sources")]
        public void DropboxFileUploadViewModel_Sources()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);


            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(CreateModelItem(), env.Object, agg.Object);

            //------------Execute Test---------------------------
            var availableSources = fileOps.Sources.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(availableSources.Count(),4);
            Assert.AreEqual(availableSources[0].ResourceName, "Select a OAuth Source...");
            Assert.AreEqual(availableSources[1].ResourceName, "New OAuth Source...");
            Assert.AreEqual(availableSources[2].ResourceName,"bob");
            Assert.AreEqual(availableSources[3].ResourceName, "dave");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxFileUploadViewModel_Sources")]
        public void DropboxFileUploadViewModel_Source_SetsModelItem()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);

            var model =CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object);
            var availableSources = fileOps.Sources.ToList();
            fileOps.SelectedSource = availableSources[0];
            //------------Execute Test---------------------------
           
            //------------Assert Results-------------------------
            Assert.AreEqual(model.GetProperty<OauthSource>("SelectedSource"),availableSources[0]);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_ChangesViewModel")]
        public void DropboxFileUploadViewModel_SelectedOperationChangesViewModel()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);

            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File" };
            //------------Execute Test---------------------------
            var availableSources = fileOps.Sources.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(model.GetProperty<string>("Operation"),"Read File");
            Assert.AreEqual(availableSources[0].ResourceName, "Select a OAuth Source...");
            Assert.AreEqual(availableSources[1].ResourceName, "New OAuth Source...");
            Assert.AreEqual(availableSources[2].ResourceName, "bob");
            Assert.AreEqual(availableSources[3].ResourceName, "dave");
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxFileUploadViewModel_EditSourcePublishesMessage()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File", SelectedSource = sources[2] };
            fileOps.EditDropboxSourceCommand.Execute(null);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            agg.Verify(a => a.Publish(It.IsAny<ShowEditResourceWizardMessage>()));
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxFileUploadViewModel_NewSourceSelected_NewSourceMessagePublished()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File", SelectedSource = sources[0] };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            agg.Verify(a => a.Publish(It.IsAny<ShowNewResourceWizard>()));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxFileUploadViewModel_EditSourceOnlyAvailableIfSourceSelected()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File" };
            Assert.IsFalse(fileOps.IsDropboxSourceSelected);
            fileOps.SelectedSource = sources[2];
            Assert.IsTrue(fileOps.IsDropboxSourceSelected);

        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxFileUploadViewModel_EditSourceOnlyAvailableIfSourceSelected_NotNewSource()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File" };
            Assert.IsFalse(fileOps.IsDropboxSourceSelected);
            fileOps.SelectedSource = fileOps.Sources[1];
            Assert.IsFalse(fileOps.IsDropboxSourceSelected);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SelectedOperation_EditSource")]
        public void DropboxFileUploadViewModel_EditSourceOnlyAvailableIfSourceSelected_NotSelectASource()
        {
            var env = new Mock<IEnvironmentModel>();
            var res = new Mock<IResourceRepository>();
            var agg = new Mock<IEventAggregator>();
            env.Setup(a => a.ResourceRepository).Returns(res.Object);
            var sources = GetSources();
            res.Setup(a => a.FindSourcesByType<OauthSource>(env.Object, enSourceType.OauthSource)).Returns(sources);
            res.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            var model = CreateModelItem();
            //------------Setup for test--------------------------
            var fileOps = new DropBoxUploadFileViewModel(model, env.Object, agg.Object) { Operation = "Read File" };
            Assert.IsFalse(fileOps.IsDropboxSourceSelected);
            fileOps.SelectedSource = fileOps.Sources[0];
            Assert.IsFalse(fileOps.IsDropboxSourceSelected);
        }

        // ReSharper restore InconsistentNaming

        List<OauthSource> GetSources()
        {
            return  new List<OauthSource>{new OauthSource{ResourceName = "bob"} , new OauthSource {ResourceName = "dave"}};
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDropBoxFileActivity());
        }
    }
}
