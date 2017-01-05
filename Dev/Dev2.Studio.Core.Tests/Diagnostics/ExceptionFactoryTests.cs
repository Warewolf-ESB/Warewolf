/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Core;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
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
            _con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(""));

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
