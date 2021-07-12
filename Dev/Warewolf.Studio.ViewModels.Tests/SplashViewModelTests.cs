/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Enums;
using Warewolf.Licensing;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SplashViewModelTests
    {
        Mock<IServer> _serverMock;
        Mock<IExternalProcessExecutor> _externalProcessExecutorMock;

        List<string> _changedProperties;
        SplashViewModel _target;

        [TestInitialize]
        public void TestInitialize()
        {
            _serverMock = new Mock<IServer>();
            _serverMock.Setup(o => o.GetSubscriptionData()).Returns(new Mock<ISubscriptionData>().Object);
            _externalProcessExecutorMock = new Mock<IExternalProcessExecutor>();

            _changedProperties = new List<string>();
            _target = new SplashViewModel(_serverMock.Object, _externalProcessExecutorMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SplashViewModel_TestSplashViewModelServerNull()
        {
            //act
            new SplashViewModel(null, _externalProcessExecutorMock.Object);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SplashViewModel_TestSplashViewModelExternalProcessExecutorNull()
        {
            //act
            new SplashViewModel(_serverMock.Object, null);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestContributorsCommandCanExecute()
        {
            //act
            var result = _target.ContributorsCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestContributorsCommandExecute()
        {
            //act
            _target.ContributorsCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.ContributorsUrl));
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestCommunityCommandCanExecute()
        {
            //act
            var result = _target.CommunityCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestCommunityCommandExecute()
        {
            //act
            _target.CommunityCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.CommunityUrl));
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestExpertHelpCommandCanExecute()
        {
            //act
            var result = _target.ExpertHelpCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestExpertHelpCommandExecute()
        {
            //act
            _target.ExpertHelpCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.ExpertHelpUrl));
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfUrlCommandCanExecute()
        {
            //act
            var result = _target.WarewolfUrlCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfUrlCommandExecute()
        {
            //act
            _target.WarewolfUrlCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.WarewolfUrl));
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.WarewolfUrl = expectedValue;
            var value = _target.WarewolfUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_Copyright_Valid_ShouldContainCurrentYear()
        {
            //------------Setup for test--------------------------
            var currentYear = DateTime.Now.Year.ToString();

            //------------Execute Test---------------------------
            var copyRightText = _target.WarewolfCopyright;
            //------------Assert Results-------------------------
            StringAssert.Contains(copyRightText, currentYear);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestContributorsUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.ContributorsUrl = expectedValue;
            var value = _target.ContributorsUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestCommunityUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.CommunityUrl = expectedValue;
            var value = _target.CommunityUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestExpertHelpUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.ExpertHelpUrl = expectedValue;
            var value = _target.ExpertHelpUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestServer()
        {
            //arrange
            var expectedValueMock = new Mock<IServer>();

            //act
            _target.Server = expectedValueMock.Object;
            var value = _target.Server;

            //assert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestExternalProcessExecutor()
        {
            //arrange
            var expectedValueMock = new Mock<IExternalProcessExecutor>();

            //act
            _target.ExternalProcessExecutor = expectedValueMock.Object;
            var value = _target.ExternalProcessExecutor;

            //assert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        [Timeout(250)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestServerVersion()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ServerVersion = expectedValue;
            var value = _target.ServerVersion;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("ServerVersion"));
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestStudioVersion()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.StudioVersion = expectedValue;
            var value = _target.StudioVersion;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("StudioVersion"));
        }
        [TestMethod]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfLicense()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.WarewolfLicense = expectedValue;
            var value = _target.WarewolfLicense;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("WarewolfLicense"));
        }
        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfLicense_IsLicensed_True()
        {
            //arrange
            var subscriptionData = new SubscriptionData
            {
                IsLicensed = true,
                PlanId = "Developer",
                Status = SubscriptionStatus.InTrial,
            };
            _serverMock.Setup(o => o.GetSubscriptionData()).Returns(subscriptionData);

            //act
            var splashViewModel = new SplashViewModel(_serverMock.Object, _externalProcessExecutorMock.Object);

            //assert
            Assert.AreEqual("Developer: InTrial", splashViewModel.WarewolfLicense);
        }

        [TestMethod]
        [Timeout(100)]
        [TestCategory(nameof(SplashViewModel))]
        public void SplashViewModel_TestWarewolfLicense_IsLicensed_False()
        {
            //arrange
            var subscriptionData = new SubscriptionData
            {
                IsLicensed = false,
                PlanId = "Not Registered",
                Status = SubscriptionStatus.NotActive,
            };
            _serverMock.Setup(o => o.GetSubscriptionData()).Returns(subscriptionData);

            //act
            var splashViewModel = new SplashViewModel(_serverMock.Object, _externalProcessExecutorMock.Object);

            //assert
            Assert.AreEqual("Not Registered: NotActive", splashViewModel.WarewolfLicense);
        }
    }
}