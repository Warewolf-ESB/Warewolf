
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExceptionFactoryTests
    {
        Mock<IEnvironmentModel> _contextModel;
        private Mock<IEnvironmentConnection> _con;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _contextModel = new Mock<IEnvironmentModel>();

            _con = new Mock<IEnvironmentConnection>();

            _con.Setup(c => c.IsConnected).Returns(true);
            _con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(""));

            _contextModel.Setup(c => c.Connection).Returns(_con.Object);

        }

        #region Create Exception

        [TestMethod]
        public void ExceptionFactoryCreateDefaultExceptionsExpectedCorrectExceptionsReturned()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.Create(e);

            //Assert
            Assert.AreEqual(StringResources.ErrorPrefix + "Test Exception", vm.Message, "Exception view model is displaying an incorrect default exception message");
            Assert.AreEqual(1, vm.Exception.Count, "Wrong number of exceptions displayed by exception view model");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test inner Exception", vm.Exception[0].Message, "Exception view model is displaying the wrong default inner exception message");
        }

        [TestMethod]
        public void ExceptionFactoryCreateCriticalExceptionsExpectedCorrectExceptionsReturned()
        {
            //Initialization
            var e = GetException();

            //Execute
            var vm = ExceptionFactory.Create(e, true);

            //Assert
            Assert.AreEqual(StringResources.CriticalExceptionMessage, vm.Message, "Exception view model is displaying an incorrect critical exception message");
            Assert.AreEqual(2, vm.Exception.Count, "Wrong number of exceptions displayed by exception view model");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test Exception", vm.Exception[0].Message, "Exception view model is displaying the wrong exception message");
            Assert.AreEqual(StringResources.ErrorPrefix + "Test inner Exception", vm.Exception[1].Message, "Exception view model is displaying the wrong inner exception message");
        }

        #endregion

        #region Create Exception View Model

        [TestMethod]
        public void ExceptionFactoryCreateDefaultExceptionViewModelExpectedNonCriticalModelCreated()
        {
            //Initialization
            var e = GetException();

            var resRepo = new Mock<IResourceRepository>();

            resRepo.Setup(r => r.GetServerLogTempPath(It.IsAny<IEnvironmentModel>())).Returns("");

            _contextModel.Setup(c => c.ResourceRepository).Returns(resRepo.Object);

            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object);

            //Assert
            Assert.IsFalse(vm.Critical, "Critical error view model created for non critical error");
        }

        [TestMethod]
        public void ExceptionFactoryCreateCriticalExceptionViewModelExpectedCriticalModelCreated()
        {
            //Initialization
            var e = GetException();

            var resRepo = new Mock<IResourceRepository>();

            resRepo.Setup(r => r.GetServerLogTempPath(It.IsAny<IEnvironmentModel>())).Returns("");

            _contextModel.Setup(c => c.ResourceRepository).Returns(resRepo.Object);

            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object, ErrorSeverity.Critical);

            //Assert
            Assert.IsTrue(vm.Critical, "Non critical error view model created for critical error");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        // ReSharper disable InconsistentNaming
        public void ExceptionFactoryCreateCriticalExceptionViewModel_LogFilesExists_EnsureThatTheLogFilesAreAlsoInitialized()
        {
            //Initialization
            var e = GetException();
            const string studioLog = "Studio.log";
            ExceptionFactory.GetStudioLogTempPath = () => studioLog;
            const string uniqueTxt = "Unique.txt";
            ExceptionFactory.GetUniqueOutputPath = ext => uniqueTxt;
            const string severTxt = "Sever.txt";
            ExceptionFactory.GetServerLogTempPath = evn => severTxt;
            //Execute
            var vm = ExceptionFactory.CreateViewModel(e, _contextModel.Object, ErrorSeverity.Critical);
            //Assert
            Assert.AreEqual(vm.StudioLogTempPath, studioLog);
            Assert.AreEqual(vm.ServerLogTempPath, severTxt);
            Assert.AreEqual(vm.OutputPath, uniqueTxt);
        }

        #endregion

        #region Create String Value

        // 14th Feb 2013
        // Created by Michael to verify additional trace info is included with the sent exception for Bug 8839
        [TestMethod]
        public void GetExceptionExpectedAdditionalTraceInfo()
        {
            string exceptionResult = ExceptionFactory.CreateStringValue(GetException()).ToString();
            StringAssert.Contains(exceptionResult, "Additional Trace Info", "Error - Additional Trace Info is missing from the exception!");
        }

        [TestMethod]
        public void GetExceptionWithCricalExceptionExpectedCriticalInfoIncluded()
        {
            string exceptionResult = ExceptionFactory.CreateStringValue(GetException(), null, true).ToString();
            StringAssert.Contains(exceptionResult, StringResources.CriticalExceptionMessage, "Error - Additional Trace Info is missing from the exception!");
        }

        #endregion

        #region Private Test Methods

        private static Exception GetException()
        {
            return new Exception("Test Exception", new Exception("Test inner Exception"));
        }

        #endregion
    }
}
