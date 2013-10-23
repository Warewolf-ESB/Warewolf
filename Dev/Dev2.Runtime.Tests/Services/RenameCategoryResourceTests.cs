using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Tests.Runtime.Services
{
    // ReSharper disable InconsistentNaming
    [TestClass][ExcludeFromCodeCoverage]
    public class RenameCategoryResourceTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        

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
            var actual = esb.Execute(new Dictionary<string, string>{{"DebugFilePath",null}}, null);
            Assert.AreEqual(string.Empty, actual);
        }
        
        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNullOldCategory_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, string> { { "OldCategory", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithBlankOldCategory_ExpectInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, string> { { "OldCategory", "" } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNoNewCategoryInValues_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, string> { { "OldCategory", "Test" } , {"Something",null}},null);
            Assert.AreEqual(string.Empty, actual);
        }
        
        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithNullNewCategoryExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, string> { { "OldCategory", "Test" },{"NewCategory",null} }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Description("Service should never get null values")]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void RenameResourceCategory_UnitTest_ExecuteWithBlankNewCategory_ExpectInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, string> { { "OldCategory", "Test" }, { "NewCategory", "" } }, null);
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