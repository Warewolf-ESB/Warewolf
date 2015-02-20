using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
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
    public class ManageDatabaseSourceViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ManageDatabaseSourceViewModel_Ctor_NullArgs_ExpectExceptions()

        {
            //unused variable so that the test fails at compile time. 
            //------------Setup for test--------------------------
            // ReSharper disable NotAccessedVariable
            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object);
           
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageDatabaseSourceViewModel));
            //------------Execute Test---------------------------
            
            // ReSharper disable RedundantAssignment
            manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object, new Mock<IDbSource>().Object);

            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object, new Mock<IDbSource>().Object }, typeof(ManageDatabaseSourceViewModel));
        
            manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IManageDatabaseSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object,new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IManageDatabaseSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageDatabaseSourceViewModel));
            // ReSharper restore NotAccessedVariable
            // ReSharper restore RedundantAssignment
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Ctor")]
        public void ManageDatabaseSourceViewModel_Ctor_FromExistingSetsItems()
        {
            var dbSource = new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType.User,
                UserName = "Bob",
                Password = "The Builder",
                Path = "moo\\Bob",
                DbName = "SuppliesDB",
                Id=Guid.NewGuid(),
                Name = "BobsSuppliesSource",
                ServerName = "ServerName",
                Type = enSourceType.SqlDatabase
            };
           var  manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IManageDatabaseSourceModel>().Object, new Mock<IEventAggregator>().Object,dbSource );
            Assert.AreEqual(manageDatabaseSourceViewModel.ResourceName,dbSource.Name);
            Assert.AreEqual(dbSource.DbName,manageDatabaseSourceViewModel.DatabaseName);
            Assert.AreEqual(dbSource.Password,manageDatabaseSourceViewModel.Password);
            Assert.AreEqual(dbSource.UserName,manageDatabaseSourceViewModel.UserName);
            Assert.AreEqual(manageDatabaseSourceViewModel.AuthenticationType,dbSource.AuthenticationType);
            Assert.AreEqual(dbSource.Type,manageDatabaseSourceViewModel.ServerType);
            Assert.AreEqual(manageDatabaseSourceViewModel.Image,ResourceType.DbSource);
           
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Save")]
        public void ManageDatabaseSourceViewModel__Save_DoesNotBringUpDialogForExisting()
        {
            var dbSource = new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType.User,
                UserName = "Bob",
                Password = "The Builder",
                Path = "moo\\Bob",
                DbName = "SuppliesDB",
                Id = Guid.NewGuid(),
                Name = "BobsSuppliesSource",
                ServerName = "ServerName",
                Type = enSourceType.SqlDatabase
            };
            var updateManager = new Mock<IManageDatabaseSourceModel>();
          
            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, new Mock<IEventAggregator>().Object, dbSource);
            Assert.AreEqual(manageDatabaseSourceViewModel.Header,  "Edit Database Service-" + dbSource.Name);
            PrivateObject p = new PrivateObject(manageDatabaseSourceViewModel);
            var dialog = new Mock<IRequestServiceNameViewModel>();
            p.SetProperty("RequestServiceNameViewModel",dialog.Object);
            manageDatabaseSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a=>a.ShowSaveDialog(),Times.Never());
            updateManager.Verify(a=>a.Save(It.IsAny<DbSourceDefinition>()));


        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Save")]
        public void ManageDatabaseSourceViewModel__Save_BringsUpDialogForNonExisting()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object,new Mock<IEventAggregator>().Object);
            updateManager.Setup(a => a.TestDbConnection(It.IsAny<IDbSource>())).Returns(new List<string>());
            Assert.IsFalse(manageDatabaseSourceViewModel.OkCommand.CanExecute(null));
            manageDatabaseSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
            updateManager.Verify(a => a.Save(It.IsAny<DbSourceDefinition>()));
          

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Save")]
        public void ManageDatabaseSourceViewModel__Save_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object) { Password = "bob", ServerType = enSourceType.SqlDatabase, AuthenticationType = AuthenticationType.Public, UserName = "dave", DatabaseName = "dbNAme" };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestDbConnection(It.IsAny<IDbSource>())).Returns(new List<string>()).Callback((IDbSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.AuthenticationType,AuthenticationType.Public);
                Assert.AreEqual(a.DbName,"dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName,"dave");

            });

            updateManager.Setup(a => a.Save(It.IsAny<IDbSource>())).Callback((IDbSource a) =>
            {
                Assert.AreEqual(a.AuthenticationType, AuthenticationType.Public);
                Assert.AreEqual(a.DbName, "dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName, "dave");

            });
            Assert.IsFalse(manageDatabaseSourceViewModel.OkCommand.CanExecute(null));
            manageDatabaseSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
            updateManager.Verify(a => a.Save(It.IsAny<DbSourceDefinition>()));
            Assert.AreEqual(manageDatabaseSourceViewModel.Header, "Edit Database Service-" +"name" );

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Test")]
        public void ManageDatabaseSourceViewModel_Test_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object) { Password = "bob", ServerType = enSourceType.SqlDatabase, AuthenticationType = AuthenticationType.Public, UserName = "dave", DatabaseName = "dbNAme" };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestDbConnection(It.IsAny<IDbSource>())).Returns(new List<string>(){"moo","roo"}).Callback((IDbSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.AuthenticationType, AuthenticationType.Public);
                Assert.AreEqual(a.DbName, "dbNAme");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.UserName, "dave");

            });
            Assert.IsFalse(manageDatabaseSourceViewModel.OkCommand.CanExecute(null));
            manageDatabaseSourceViewModel.TestCommand.Execute(null);
            Assert.IsTrue(manageDatabaseSourceViewModel.TestPassed);
            Assert.AreEqual(manageDatabaseSourceViewModel.DatabaseNames.Count,2);

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_CanTest")]
        public void ManageDatabaseSourceViewModel_CanTest_Public()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageDatabaseSourceViewModel.CanTest());
            manageDatabaseSourceViewModel.Password = "bob";
            manageDatabaseSourceViewModel.ServerType = enSourceType.SqlDatabase;
            manageDatabaseSourceViewModel.AuthenticationType = AuthenticationType.Public;
            manageDatabaseSourceViewModel.UserName = "dave";
            manageDatabaseSourceViewModel.DatabaseName = "dbNAme";
            manageDatabaseSourceViewModel.ServerName = "mon";
            Assert.IsTrue(manageDatabaseSourceViewModel.CanTest());


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_CanTest")]
        public void ManageDatabaseSourceViewModel_CanTest_User()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageDatabaseSourceViewModel.CanTest());
            manageDatabaseSourceViewModel.Password = "bob";
            manageDatabaseSourceViewModel.ServerType = enSourceType.SqlDatabase;
            manageDatabaseSourceViewModel.AuthenticationType = AuthenticationType.User;
           
            manageDatabaseSourceViewModel.DatabaseName = "dbNAme";
            manageDatabaseSourceViewModel.ServerName = "mon";
            Assert.IsFalse(manageDatabaseSourceViewModel.CanTest());

            manageDatabaseSourceViewModel.UserName = "dave";
            Assert.IsTrue(manageDatabaseSourceViewModel.CanTest());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_CanTest")]
        public void ManageDatabaseSourceViewModel_Header()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageDatabaseSourceModel>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageDatabaseSourceViewModel.CanTest());
            manageDatabaseSourceViewModel.Password = "bob";
            manageDatabaseSourceViewModel.ServerType = enSourceType.SqlDatabase;
            manageDatabaseSourceViewModel.AuthenticationType = AuthenticationType.Public;
            manageDatabaseSourceViewModel.UserName = "dave";
            manageDatabaseSourceViewModel.DatabaseName = "dbNAme";
            Assert.AreEqual(manageDatabaseSourceViewModel.Header, "New Database Connector Source Server");


        }
    }
    // ReSharper restore InconsistentNaming
}
