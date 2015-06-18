using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using FluentAssertions;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageEmailSourceViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ManageEmailSourceViewModel_Ctor_NullArgs_ExpectExceptions()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            // ReSharper disable NotAccessedVariable
            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(new Mock<IManageEmailSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManageEmailSourceViewModel_Ctor_NullSaveDialog_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new ManageEmailSourceViewModel(new Mock<IManageEmailSourceModel>().Object, new Mock<IRequestServiceNameViewModel>().Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Ctor")]
        public void ManageEmailSourceViewModel_Ctor_FromExistingSetsItems()
        {
            var emailSource = new EmailServiceSourceDefinition
            {
                HostName = "ServerName",
                UserName = "Bob",
                Password = "The Builder",
                EnableSsl = false,
                Port = 25,
                Timeout = 100,
                EmailFrom = "Bob@builders.com",
                EmailTo = "Bob@builders.com",
                Path = "moo\\Bob",
                Id=Guid.NewGuid(),
            };
            var mockUpdateManager = new Mock<IManageEmailSourceModel>();
            mockUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(mockUpdateManager.Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object, emailSource);
            Assert.AreEqual(emailSource.HostName, manageEmailSourceViewModel.HostName);
            Assert.AreEqual(emailSource.Password, manageEmailSourceViewModel.Password);
            Assert.AreEqual(emailSource.UserName, manageEmailSourceViewModel.UserName);
            Assert.AreEqual(emailSource.EnableSsl, manageEmailSourceViewModel.EnableSsl);
            Assert.AreEqual(emailSource.Port, manageEmailSourceViewModel.Port);
            Assert.AreEqual(emailSource.Timeout, manageEmailSourceViewModel.Timeout);
            Assert.AreEqual(emailSource.EmailFrom, manageEmailSourceViewModel.EmailFrom);
            Assert.AreEqual(emailSource.EmailTo, manageEmailSourceViewModel.EmailTo);
            Assert.AreEqual(manageEmailSourceViewModel.Image, ResourceType.EmailSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Save")]
        public void ManageEmailSourceViewModel__Save_DoesNotBringUpDialogForExisting()
        {
            var emailSource = new EmailServiceSourceDefinition
            {
                HostName = "ServerName",
                UserName = "Bob",
                Password = "The Builder",
                EnableSsl = false,
                Port = 25,
                Timeout = 100,
                EmailFrom = "Bob@builders.com",
                EmailTo = "Bob@builders.com",
                Path = "moo\\Bob",
                Id = Guid.NewGuid(),
            };
            var updateManager = new Mock<IManageEmailSourceModel>();
            updateManager.Setup(model => model.ServerName).Returns("localhost");
            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, new Mock<IRequestServiceNameViewModel>().Object, new Mock<IEventAggregator>().Object, emailSource);
            Assert.AreEqual(manageEmailSourceViewModel.Header, "ServerName");
            PrivateObject p = new PrivateObject(manageEmailSourceViewModel);
            var dialog = new Mock<IRequestServiceNameViewModel>();
            p.SetProperty("RequestServiceNameViewModel",dialog.Object);
            manageEmailSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a=>a.ShowSaveDialog(),Times.Never());
            updateManager.Verify(a => a.Save(It.IsAny<EmailServiceSourceDefinition>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Save")]
        public void ManageEmailSourceViewModel__Save_BringsUpDialogForNonExisting()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageEmailSourceModel>();

            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);

            updateManager.Setup(a => a.TestConnection(It.IsAny<IEmailServiceSource>())).Returns(It.IsAny<string>());
            Assert.IsFalse(manageEmailSourceViewModel.OkCommand.CanExecute(null));
            manageEmailSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Save")]
        public void ManageEmailSourceViewModel__Save_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageEmailSourceModel>();

            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object)
            {
                HostName = "ServerName",
                UserName = "Bob",
                Password = "The Builder",
                EnableSsl = false,
                Port = 25,
                Timeout = 100,
                EmailFrom = "Bob@builders.com",
                EmailTo = "Bob@builders.com",
            };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestConnection(It.IsAny<IEmailServiceSource>())).Returns(It.IsAny<string>()).Callback((IEmailServiceSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.HostName, "servername");
                Assert.AreEqual(a.UserName, "dave");
                Assert.AreEqual(a.Password, "bob");
                Assert.AreEqual(a.EnableSsl,false);
                Assert.AreEqual(a.Port,25);
                Assert.AreEqual(a.Timeout, 100);
                Assert.AreEqual(a.EmailFrom, "bob@builders.com");
                Assert.AreEqual(a.EmailTo, "bob@builders.com");
            });

            updateManager.Setup(a => a.Save(It.IsAny<IEmailServiceSource>())).Callback((IEmailServiceSource a) =>
            {
                Assert.AreEqual(a.HostName, "ServerName");
                Assert.AreEqual(a.UserName, "Bob");
                Assert.AreEqual(a.Password, "The Builder");
                Assert.AreEqual(a.EnableSsl, false);
                Assert.AreEqual(a.Port, 25);
                Assert.AreEqual(a.Timeout, 100);
                Assert.AreEqual(a.EmailFrom, "Bob@builders.com");
                Assert.AreEqual(a.EmailTo, "Bob@builders.com");

            });
            Assert.IsFalse(manageEmailSourceViewModel.OkCommand.CanExecute(null));
            manageEmailSourceViewModel.OkCommand.Execute(null);
            dialog.Verify(a => a.ShowSaveDialog(), Times.Once());
            Assert.AreEqual(manageEmailSourceViewModel.Header, "New Email Source");

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_Test")]
        public void ManageEmailSourceViewModel_Test_SetsUpCorrectValues()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageEmailSourceModel>();

            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object)
            {
                HostName = "ServerName",
                UserName = "Bob",
                Password = "The Builder",
                EnableSsl = false,
                Port = 25,
                Timeout = 100,
                EmailFrom = "Bob@builders.com",
                EmailTo = "Bob@builders.com",
            };
            // ReSharper disable MaximumChainedReferences
            updateManager.Setup(a => a.TestConnection(It.IsAny<IEmailServiceSource>())).Returns(It.IsAny<string>()).Callback((IEmailServiceSource a) =>
                // ReSharper restore MaximumChainedReferences
            {
                Assert.AreEqual(a.HostName, "ServerName");
                Assert.AreEqual(a.UserName, "Bob");
                Assert.AreEqual(a.Password, "The Builder");
                Assert.AreEqual(a.EnableSsl, false);
                Assert.AreEqual(a.Port, 25);
                Assert.AreEqual(a.Timeout, 100);
                Assert.AreEqual(a.EmailFrom, "Bob@builders.com");
                Assert.AreEqual(a.EmailTo, "Bob@builders.com");
            });
            Assert.IsFalse(manageEmailSourceViewModel.OkCommand.CanExecute(null));
            manageEmailSourceViewModel.SendCommand.Execute(null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_CanTest")]
        public void ManageEmailSourceViewModel_CanTest()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageEmailSourceModel>();

            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageEmailSourceViewModel.CanTest());
            manageEmailSourceViewModel.HostName = "servername";
            manageEmailSourceViewModel.Password = "bob";
            manageEmailSourceViewModel.UserName = "dave";
            manageEmailSourceViewModel.EnableSsl = false;
            manageEmailSourceViewModel.Port = 25;
            manageEmailSourceViewModel.Timeout = 100;
            manageEmailSourceViewModel.EmailFrom = "bob@builders.com";
            manageEmailSourceViewModel.EmailTo = "bob@builders.com";
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ManageEmailSourceViewModel_CanTest")]
        public void ManageEmailSourceViewModel_Header()
        {
            var dialog = new Mock<IRequestServiceNameViewModel>();
            dialog.Setup(a => a.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            dialog.Setup(a => a.ResourceName).Returns(new ResourceName("path", "name"));
            var updateManager = new Mock<IManageEmailSourceModel>();

            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(updateManager.Object, dialog.Object, new Mock<IEventAggregator>().Object);
            Assert.IsFalse(manageEmailSourceViewModel.CanTest());
            manageEmailSourceViewModel.HostName = "servername";
            manageEmailSourceViewModel.Password = "bob";
            manageEmailSourceViewModel.UserName = "dave";
            manageEmailSourceViewModel.EnableSsl = false;
            manageEmailSourceViewModel.Port = 25;
            manageEmailSourceViewModel.Timeout = 100;
            manageEmailSourceViewModel.EmailFrom = "bob@builders.com";
            manageEmailSourceViewModel.EmailTo = "bob@builders.com";
            Assert.AreEqual(manageEmailSourceViewModel.Header, "New Email Source");
        }
    }
}