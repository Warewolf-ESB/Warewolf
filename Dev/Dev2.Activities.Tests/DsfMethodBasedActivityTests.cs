/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Storage;

namespace Dev2.Tests.Activities
{
    [TestClass]
    public class DsfMethodBasedActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfMethodBasedActivity))]
        public void DsfMethodBasedActivity_Inputs_Null_ExpectZeroListsCount()
        {
            //-------------------Arrange------------------------
            var itrCollection = new WarewolfListIterator();
            var itrs = new List<IWarewolfIterator>(5);
            var dSFDataObject = new DsfDataObject("", Guid.NewGuid());
            var methodParameters = new List<MethodParameter>();
            var testDsfMethodBasedActivity = new TestDsfMethodBasedActivity();

            var mockServiceInput = new Mock<IServiceInput>();
            var mockServiceInputCollection = new Mock<ICollection<IServiceInput>>();

            mockServiceInputCollection.Setup(o => o.Add(mockServiceInput.Object));
            //-------------------Act----------------------------
            testDsfMethodBasedActivity.TestBuildParameterIterators(0, methodParameters.ToList(), itrCollection, itrs, dSFDataObject);
            //-------------------Assert-------------------------
            Assert.AreEqual(0, itrs.Count);
            Assert.AreEqual(0, itrCollection.FieldCount);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfMethodBasedActivity))]
        public void DsfMethodBasedActivity_Inputs_NotNull_Name_NotNull_ExpectListsCountNonZero()
        {
            //-------------------Arrange------------------------
            var itrs = new List<IWarewolfIterator>(5);
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var testDsfMethodBasedActivity = new TestDsfMethodBasedActivity();
            var inputList = new List<MethodParameter>();
            var environment = new ExecutionEnvironment();
            var dataListID = Guid.NewGuid();
            var itrCollection = new WarewolfListIterator();
            var dSFDataObject = new DsfDataObject("", Guid.NewGuid());

            mockDSFDataObject.Setup(o => o.DataListID).Returns(dataListID);
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            var methodParamList = new MethodParameter { EmptyToNull = false, IsRequired = false, Name = "TestName", Value = "TestValue", TypeName = "TestTypeName" };

            inputList.Add(methodParamList);

            var methodParameters = inputList?.Select(a => new MethodParameter { EmptyToNull = a.EmptyToNull, IsRequired = a.IsRequired, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
            //-------------------Act----------------------------
            testDsfMethodBasedActivity.TestBuildParameterIterators(0, methodParameters, itrCollection, itrs, dSFDataObject);
            //-------------------Assert-------------------------
            Assert.AreEqual(1, itrs.Count);
            Assert.AreEqual(1, itrCollection.FieldCount);
            Assert.AreEqual(dataListID, mockDSFDataObject.Object.DataListID);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfMethodBasedActivity))]
        public void DsfMethodBasedActivity_Inputs_NotNull_Name_Null_ExpectListsCountNonZero()
        {
            //-------------------Arrange------------------------
            var itrs = new List<IWarewolfIterator>(5);
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var testDsfMethodBasedActivity = new TestDsfMethodBasedActivity();
            var inputList = new List<MethodParameter>();
            var environment = new ExecutionEnvironment();
            var dataListID = Guid.NewGuid();
            var itrCollection = new WarewolfListIterator();

            mockDSFDataObject.Setup(o => o.DataListID).Returns(dataListID);
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            var methodParamList = new MethodParameter { EmptyToNull = false, IsRequired = false, Name = null, Value = "TestValue", TypeName = "TestTypeName" };

            inputList.Add(methodParamList);

            var methodParameters = inputList?.Select(a => new MethodParameter { EmptyToNull = a.EmptyToNull, IsRequired = a.IsRequired, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
            //-------------------Act----------------------------
            testDsfMethodBasedActivity.TestBuildParameterIterators(0, methodParameters, itrCollection, itrs, mockDSFDataObject.Object);
            //-------------------Assert-------------------------
            Assert.AreEqual(1, itrs.Count);
            Assert.AreEqual(1, itrCollection.FieldCount);
            Assert.AreEqual(dataListID, mockDSFDataObject.Object.DataListID);
        }

        class TestDsfMethodBasedActivity : DsfMethodBasedActivity
        {
            public void TestBuildParameterIterators(int update, List<MethodParameter> inputs, IWarewolfListIterator itrCollection, List<IWarewolfIterator> itrs, IDSFDataObject dataObject)
            {
                base.BuildParameterIterators(update, inputs, itrCollection, itrs, dataObject);
            }
        }
    }
}
