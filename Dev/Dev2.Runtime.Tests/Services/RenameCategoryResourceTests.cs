
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RenameCategoryResourceTests
    {

        #region Execute

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNullValues_ExpectedInvalidDataContractException()
        {
            var esb = new RenameResourceCategory();
            var actual = esb.Execute(null, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNoOldCategoryInValues_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugFilePath", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNullOldCategory_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "OldCategory", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithBlankOldCategory_ExpectInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "OldCategory", new StringBuilder() } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNoNewCategoryInValues_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "OldCategory", new StringBuilder("Test") }, { "Something", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNullNewCategoryExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "OldCategory", new StringBuilder("Test") }, { "NewCategory", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithBlankNewCategory_ExpectInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "OldCategory", new StringBuilder("Test") }, { "NewCategory", new StringBuilder() } }, null);
            Assert.AreEqual(string.Empty, actual);
        }
        #endregion

        #region HandlesType

        [TestMethod]
        [Owner("Huggs")]
        public void RenameResourceCategory_UnitTest_HandlesType_ExpectedReturnsRenameResourceCategoryService()
        {
            var esb = new RenameResourceCategory();
            var result = esb.HandlesType();
            Assert.AreEqual("RenameResourceCategoryService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        public void RenameResourceCategory_UnitTest_CreateServiceEntry_ExpectedReturnsDynamicService()
        {
            var esb = new RenameResourceCategory();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><OldCategory ColumnIODirection=\"Input\"/><NewCategory ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
