
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
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class ResultListTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_Constructor")]
        public void ResultList_Constructor_Default_HasErrorsIsFalse()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var resultList = new ResultList<string>();

            //------------Assert Results-------------------------
            Assert.IsNotNull(resultList.Items);
            Assert.IsFalse(resultList.HasErrors);
            Assert.IsNull(resultList.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_Constructor")]
        public void ResultList_Constructor_ErrorFormatWithNoArgs_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var resultList = new ResultList<string>("Hello");

            //------------Assert Results-------------------------
            Assert.IsNotNull(resultList.Items);
            Assert.IsTrue(resultList.HasErrors);
            Assert.AreEqual("Hello", resultList.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_Constructor")]
        public void ResultList_Constructor_ErrorFormatWithArgs_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var resultList = new ResultList<string>("Hello {0}", "world");

            //------------Assert Results-------------------------
            Assert.IsNotNull(resultList.Items);
            Assert.IsTrue(resultList.HasErrors);
            Assert.AreEqual("Hello world", resultList.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_Constructor")]
        public void ResultList_Constructor_ExceptionIsNull_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var resultList = new ResultList<string>((Exception)null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(resultList.Items);
            Assert.IsTrue(resultList.HasErrors);
            Assert.AreEqual("", resultList.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_Constructor")]
        public void ResultList_Constructor_ExceptionIsNotNull_HasErrorsIsTrue()
        {
            //------------Setup for test--------------------------
            var ex = new Exception("Error Occurred", new Exception("Inner Error"));

            //------------Execute Test---------------------------
            var resultList = new ResultList<string>(ex);

            //------------Assert Results-------------------------
            Assert.IsNotNull(resultList.Items);
            Assert.IsTrue(resultList.HasErrors);
            Assert.AreEqual("Error Occurred\r\nInner Error\r\n", resultList.Errors);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResultList_ToString")]
        public void ResultList_ToString_Json()
        {
            //------------Setup for test--------------------------
            var resultList = new ResultList<string>("Hello");
            var expected = JsonConvert.SerializeObject(resultList);

            //------------Execute Test---------------------------
            var actual = resultList.ToString();

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
