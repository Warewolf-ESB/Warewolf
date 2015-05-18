using System;
using System.Windows;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageWebServiceSourceViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ManageWebServiceSourceViewModel_Ctor_NullArgs_ExpectExceptions()

        {
            //unused variable so that the test fails at compile time. 
            //------------Setup for test--------------------------
            // ReSharper disable NotAccessedVariable
            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(new Mock<IManageWebServiceSourceModel>().Object, new Mock<IEventAggregator>().Object);
           
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageWebserviceSourceViewModel));
            //------------Execute Test---------------------------
            
            // ReSharper disable RedundantAssignment
            manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(new Mock<IManageWebServiceSourceModel>().Object, new Mock<IEventAggregator>().Object, new Mock<IWebServiceSource>().Object);

            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object, new Mock<IDbSource>().Object }, typeof(ManageWebserviceSourceViewModel));

            manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(new Mock<IManageWebServiceSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageWebserviceSourceViewModel));
            // ReSharper restore NotAccessedVariable
            // ReSharper restore RedundantAssignment
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Ctor")]
        public void ManageWebServiceSourceViewModel_Ctor_FromExistingSetsItems()
        {
            var dbSource = new WebServiceSourceDefinition
            {
                AuthenticationType = AuthenticationType.User,
                UserName = "Bob",
                Password = "The Builder",
                Path = "moo\\Bob",
                DefaultQuery = "SuppliesDB",
                Id=Guid.NewGuid(),
                Name = "BobsSuppliesSource",
                HostName = "ServerName"
            };
            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(new Mock<IManageWebServiceSourceModel>().Object, new Mock<IEventAggregator>().Object, dbSource);
            Assert.AreEqual(manageWebServiceSourceViewModel.ResourceName, dbSource.Name);
            Assert.AreEqual(dbSource.DefaultQuery, manageWebServiceSourceViewModel.DefaultQuery);
            Assert.AreEqual(dbSource.Password, manageWebServiceSourceViewModel.Password);
            Assert.AreEqual(dbSource.UserName, manageWebServiceSourceViewModel.UserName);
            Assert.AreEqual(manageWebServiceSourceViewModel.AuthenticationType, dbSource.AuthenticationType);
            Assert.AreEqual(manageWebServiceSourceViewModel.Image, ResourceType.DbSource);
           
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Save")]
        public void ManageWebServiceSourceViewModel__Save_DoesNotBringUpDialogForExisting()
        {
            var dbSource = new WebServiceSourceDefinition
            {
                AuthenticationType = AuthenticationType.User,
                UserName = "Bob",
                Password = "The Builder",
                Path = "moo\\Bob",
                DefaultQuery = "SuppliesDB",
                Id = Guid.NewGuid(),
                Name = "BobsSuppliesSource",
                HostName = "ServerName"
            };
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, new Mock<IEventAggregator>().Object, dbSource);
            Assert.AreEqual(manageWebServiceSourceViewModel.Header, "Edit Database Service-" + dbSource.Name);
            PrivateObject p = new PrivateObject(manageWebServiceSourceViewModel);
            var dialog = new Mock<IRequestServiceNameViewModel>();
            p.SetProperty("RequestServiceNameViewModel",dialog.Object);
            manageWebServiceSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a=>a.ShowSaveDialog(),Times.Never());
            updateManager.Verify(a => a.Save(It.IsAny<WebServiceSourceDefinition>()));


        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Save")]
        public void ManageWebServiceSourceViewModel__Save_BringsUpDialogForNonExisting()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            updateManager.Setup(a => a.TestConnection(It.IsAny<IWebServiceSource>()));
            Assert.IsFalse(manageWebServiceSourceViewModel.OkCommand.CanExecute(null));
            manageWebServiceSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
            updateManager.Verify(a => a.Save(It.IsAny<WebServiceSourceDefinition>()));
          

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Save")]
        public void ManageWebServiceSourceViewModel__Save_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object) { Password = "bob", AuthenticationType = AuthenticationType.Public, UserName = "dave", DefaultQuery = "dbNAme" };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestConnection(It.IsAny<IWebServiceSource>())).Callback((IWebServiceSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.AuthenticationType,AuthenticationType.Anonymous);
                Assert.AreEqual(a.HostName,"dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName,"dave");

            });

            updateManager.Setup(a => a.Save(It.IsAny<IWebServiceSource>())).Callback((IWebServiceSource a) =>
            {
                Assert.AreEqual(a.AuthenticationType, AuthenticationType.User);
                Assert.AreEqual(a.HostName, "dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName, "dave");

            });
            Assert.IsFalse(manageWebServiceSourceViewModel.OkCommand.CanExecute(null));
            manageWebServiceSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
            updateManager.Verify(a => a.Save(It.IsAny<WebServiceSourceDefinition>()));
            Assert.AreEqual(manageWebServiceSourceViewModel.Header, "Edit Database Service-" + "name");

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_Test")]
        public void ManageWebServiceSourceViewModel_Test_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object) { Password = "bob",  AuthenticationType = AuthenticationType.Public, UserName = "dave", DefaultQuery = "dbNAme" };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestConnection(It.IsAny<IWebServiceSource>())).Callback((IWebServiceSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.AuthenticationType, AuthenticationType.Anonymous);
                Assert.AreEqual(a.HostName, "dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName, "dave");

            });
            Assert.IsFalse(manageWebServiceSourceViewModel.OkCommand.CanExecute(null));
            manageWebServiceSourceViewModel.TestCommand.Execute(null);
            Assert.IsTrue(manageWebServiceSourceViewModel.TestPassed);

        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_CanTest")]
        public void ManageWebServiceSourceViewModel_CanTest_Public()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageWebServiceSourceViewModel.CanTest());
            manageWebServiceSourceViewModel.Password = "bob";
            manageWebServiceSourceViewModel.AuthenticationType = AuthenticationType.Public;
            manageWebServiceSourceViewModel.UserName = "dave";
            manageWebServiceSourceViewModel.DefaultQuery = "dbNAme";
            manageWebServiceSourceViewModel.HostName = "mon";
            Assert.IsTrue(manageWebServiceSourceViewModel.CanTest());


        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_CanTest")]
        public void ManageWebServiceSourceViewModel_CanTest_User()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageWebServiceSourceViewModel.CanTest());
            manageWebServiceSourceViewModel.Password = "bob";
            manageWebServiceSourceViewModel.AuthenticationType = AuthenticationType.User;

            manageWebServiceSourceViewModel.DefaultQuery = "dbNAme";
            manageWebServiceSourceViewModel.HostName = "mon";
            Assert.IsFalse(manageWebServiceSourceViewModel.CanTest());

            manageWebServiceSourceViewModel.UserName = "dave";
            Assert.IsTrue(manageWebServiceSourceViewModel.CanTest());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ManageWebServiceSourceViewModel_CanTest")]
        public void ManageWebServiceSourceViewModel_Header()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageWebServiceSourceModel>();

            var manageWebServiceSourceViewModel = new ManageWebserviceSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageWebServiceSourceViewModel.CanTest());
            manageWebServiceSourceViewModel.Password = "bob";
            manageWebServiceSourceViewModel.AuthenticationType = AuthenticationType.Public;
            manageWebServiceSourceViewModel.UserName = "dave";
            manageWebServiceSourceViewModel.DefaultQuery = "dbNAme";
            Assert.AreEqual("New Webservice Connector Source Server", manageWebServiceSourceViewModel.Header);


        }
    }
}