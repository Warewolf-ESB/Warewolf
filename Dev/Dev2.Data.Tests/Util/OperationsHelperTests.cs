/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Data.Tests.Util
{
    [TestClass]
    public class OperationsHelperTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperationsHelper_ExtractUserName_ExpectedException()
        {
            OperationsHelper.ExtractUserName(null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractUserName()
        {
            const string userName = "testUser";
            const string expectedUserName = "testUser";
            var mockPathAuth = new Mock<IPathAuth>();
            mockPathAuth.Setup(pathAuth => pathAuth.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractUserName(mockPathAuth.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractUserName_With_Domain()
        {
            const string userName = "domain\\testUser";
            const string expectedUserName = "testUser";
            var mockPathAuth = new Mock<IPathAuth>();
            mockPathAuth.Setup(pathAuth => pathAuth.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractUserName(mockPathAuth.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractUserName_With_Domain1()
        {
            const string userName = "\\testUser";
            const string expectedUserName = "testUser";
            var mockPathAuth = new Mock<IPathAuth>();
            mockPathAuth.Setup(pathAuth => pathAuth.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractUserName(mockPathAuth.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperationsHelper_ExtractDomain_ExpectedException()
        {
            OperationsHelper.ExtractDomain(null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractDomain()
        {
            const string userName = "testDomain";
            const string expectedUserName = "";
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractDomain(mockActivityIOPath.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractDomain_With_UserName()
        {
            const string userName = "domain\\testUser";
            const string expectedUserName = "domain";
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractDomain(mockActivityIOPath.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OperationsHelper))]
        public void OperationsHelper_ExtractDomain_With_UserName1()
        {
            const string userName = "domain\\";
            const string expectedUserName = "domain";
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            mockActivityIOPath.Setup(activityIOPath => activityIOPath.Username).Returns(userName);
            var operationsHelper = OperationsHelper.ExtractDomain(mockActivityIOPath.Object);
            Assert.AreEqual(expectedUserName, operationsHelper);
        }
    }
}
