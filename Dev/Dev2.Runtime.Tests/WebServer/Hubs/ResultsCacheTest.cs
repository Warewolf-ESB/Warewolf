
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
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Hubs
{
    [TestClass]
    public class ResultsCacheTest
    {
        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_ContainsPendingRequestForUser")]
        public void ResultsCache_ContainsPendingRequestForUser_WhenValidKey_ExpectTrue()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "foobar");
            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.ContainsPendingRequestForUser(reciept.User);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_ContainsPendingRequestForUser")]
        public void ResultsCache_ContainsPendingRequestForUser_WhenValidKeyNotExist_ExpectFalce()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "foobar");
            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.ContainsPendingRequestForUser("a-b-c");

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_ContainsPendingRequestForUser")]
        public void ResultsCache_ContainsPendingRequestForUser_WhenValidKeyPartialMatch_ExpectFalce()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "foobar");
            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.ContainsPendingRequestForUser("Bo");

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_ContainsPendingRequestForUser")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_ContainsPendingRequestForUser_WhenNullKey_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "foobar");
            //------------Execute Test---------------------------
            ResultsCache.Instance.ContainsPendingRequestForUser(null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_ContainsPendingRequestForUser")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_ContainsPendingRequestForUser_WhenEmptyKey_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "foobar");
            //------------Execute Test---------------------------
            ResultsCache.Instance.ContainsPendingRequestForUser(null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_AddResult")]
        public void ResultsCache_AddResult_WhenValidKey_ExpectAdded()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();

            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.AddResult(reciept, "foobar");

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_AddResult")]
        [ExpectedException(typeof(Exception))]
        public void ResultsCache_AddResult_WhenInvalidKey_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            reciept.User = null;

            //------------Execute Test---------------------------
            ResultsCache.Instance.AddResult(reciept, "foobar");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_AddResult")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_AddResult_WhenNullKey_ExpectException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            ResultsCache.Instance.AddResult(null, "foobar");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_AddResult")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_AddResult_WhenNullValue_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();

            //------------Execute Test---------------------------
            ResultsCache.Instance.AddResult(reciept, null);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_AddResult")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_AddResult_WhenEmptyValue_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();

            //------------Execute Test---------------------------
            ResultsCache.Instance.AddResult(reciept, string.Empty);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_FetchResult")]
        public void ResultsCache_FetchResult_WhenValidKey_ExpectResult()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "bar");

            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.FetchResult(reciept);

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "bar");
        }



        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_FetchResult")]
        public void ResultsCache_FetchResult_WhenKeyNotPresent_ExpectEmptyString()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "bar");
            reciept.User = "Fred";

            //------------Execute Test---------------------------
            var result = ResultsCache.Instance.FetchResult(reciept);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result, "Non-empty string on invalid key");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_FetchResult")]
        [ExpectedException(typeof(Exception))]
        public void ResultsCache_FetchResult_WhenInvalidKey_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "bar");
            reciept.User = null;

            //------------Execute Test---------------------------
            ResultsCache.Instance.FetchResult(reciept);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResultsCache_FetchResult")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultsCache_FetchResult_WhenNullKey_ExpectException()
        {
            //------------Setup for test--------------------------

            var reciept = GenerateReciept();
            ResultsCache.Instance.AddResult(reciept, "bar");

            //------------Execute Test---------------------------
            ResultsCache.Instance.FetchResult(null);
        }

        static FutureReceipt GenerateReciept()
        {
            var reciept = new FutureReceipt { PartID = 1, RequestID = Guid.NewGuid(), User = "Bob" };
            return reciept;
        }

        // ReSharper restore InconsistentNaming
    }

}
