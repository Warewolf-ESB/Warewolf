using System;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
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
        public void ManageDatabaseSourceViewModel_Ctor_NullArgs_ExpectExceptions()
        {
            //unused variable so that the test fails at compile time. 
            //------------Setup for test--------------------------
            // ReSharper disable NotAccessedVariable
            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IStudioUpdateManager>().Object, new Mock<IEventAggregator>().Object);
           
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IStudioUpdateManager>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageDatabaseSourceViewModel));
            //------------Execute Test---------------------------
            
            // ReSharper disable RedundantAssignment
            manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IStudioUpdateManager>().Object, new Mock<IEventAggregator>().Object, new Mock<IDbSource>().Object);

            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IStudioUpdateManager>().Object, new Mock<IEventAggregator>().Object, new Mock<IDbSource>().Object }, typeof(ManageDatabaseSourceViewModel));
        
            manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IStudioUpdateManager>().Object, new Mock<IRequestServiceNameViewModel>().Object,new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IStudioUpdateManager>().Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object }, typeof(ManageDatabaseSourceViewModel));
            // ReSharper restore NotAccessedVariable
            // ReSharper restore RedundantAssignment
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Ctor")]
        public void ManageDatabaseSourceViewModel_Ctor_FromExistingSetsItems()
        {
            var dbSource = new DbSourceDefinition()
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
           var  manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(new Mock<IStudioUpdateManager>().Object, new Mock<IEventAggregator>().Object,dbSource );
            Assert.AreEqual(manageDatabaseSourceViewModel.ResourceName,dbSource.Name);
            Assert.AreEqual(dbSource.DbName,manageDatabaseSourceViewModel.DatabaseName);
            Assert.AreEqual(dbSource.Password,manageDatabaseSourceViewModel.Password);
            Assert.AreEqual(dbSource.UserName,manageDatabaseSourceViewModel.UserName);
            Assert.AreEqual(manageDatabaseSourceViewModel.AuthenticationType,dbSource.AuthenticationType);
            Assert.AreEqual(dbSource.Type,manageDatabaseSourceViewModel.ServerType);
            
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ManageDatabaseSourceViewModel_Save")]
        public void ManageDatabaseSourceViewModel__Save_dopesNotBringUpDialogForExisting()
        {
            var dbSource = new DbSourceDefinition()
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
            var updateManager = new Mock<IStudioUpdateManager>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, new Mock<IEventAggregator>().Object, dbSource);
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
        public void ManageDatabaseSourceViewModel__Save_BringsUpDialogForExisting()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();

            var updateManager = new Mock<IStudioUpdateManager>();

            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(updateManager.Object, dialog.Object,new Mock<IEventAggregator>().Object);

            Assert.IsFalse(manageDatabaseSourceViewModel.OkCommand.CanExecute(null));
            manageDatabaseSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Never());
            updateManager.Verify(a => a.Save(It.IsAny<DbSourceDefinition>()));


        }
    }
}
