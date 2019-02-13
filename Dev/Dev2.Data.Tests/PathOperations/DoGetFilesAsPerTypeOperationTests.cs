/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Operations;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class DoGetFilesAsPerTypeOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_Ctor_WithNoPath_ExpectNullReferenceException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<NullReferenceException>(()=> new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_WithUnknownPath_ExpectException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockListIActivityIOPath = new Mock<IList<IActivityIOPath>>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files);
            //---------------------------Assert----------------------------
            Assert.ThrowsException<Exception>(()=> doGetFilesAsPerTypeOperation.ExecuteOperation());
        }

        //[TestMethod]
        //[Owner("Siphamandla Dube")]
        //[TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        //public void DoGetFilesAsPerTypeOperation_ExecuteOperation_WithKnownPath_Expect()
        //{
        //    //---------------------------Arrange---------------------------
        //    var mockActivityIOPath = new Mock<IActivityIOPath>();
        //    var mockListIActivityIOPath = new Mock<IList<IActivityIOPath>>();
        //    var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
        //    var mockFile = new Mock<IFile>();
        //    var mockDirectory = new Mock<IDirectory>();

        //    const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log";

        //    mockDev2LogonProvider.Setup(o => o.DoLogon(It.IsAny<IActivityIOPath>())).Returns(new SafeTokenHandle());

        //    mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
        //    mockActivityIOPath.Setup(o => o.Username).Returns("testUsername");
        //    mockActivityIOPath.Setup(o => o.Password).Returns("testPass");
        //    //---------------------------Act-------------------------------
        //    var ff = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object)
        //    {
              
        //    };
        //    var list = ff.ExecuteOperation();
        //    //---------------------------Assert----------------------------
        //}
    }
}
