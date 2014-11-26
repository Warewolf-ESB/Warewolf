
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
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB.Execution;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ESB
{
    // BUG 9710 - 2013.06.20 - TWR - Created
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DatabaseServiceContainerTests
    {
        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
        }

        #endregion

        #region Execute
        [TestMethod]
        public void DatabaseServiceContainer_UnitTest_ExecuteWhereHasDatabaseServiceExecution_Guid()
        {
            //------------Setup for test--------------------------
            var mockServiceExecution = new Mock<IServiceExecution>();
            ErrorResultTO errors;
            Guid expected = Guid.NewGuid();
            mockServiceExecution.Setup(execution => execution.Execute(out errors)).Returns(expected);
            DatabaseServiceContainer databaseServiceContainer = new DatabaseServiceContainer(mockServiceExecution.Object);
            //------------Execute Test---------------------------
            Guid actual = databaseServiceContainer.Execute(out errors);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "Execute should return the Guid from the service execution");
        }

        #endregion

        #region CreateDatabaseServiceContainer

        #endregion

        #region CreateServiceAction

        #endregion

    }
}
