
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dev2.Runtime.WebServer.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    [TestClass]
    public class QueryStringTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void QueryString_Constructor_ItemsIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var queryString = new QueryString(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_GetEnumerator")]
        public void QueryString_GetEnumerator_ItemsEnumerator()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var enumerator = queryString.GetEnumerator();

            //------------Assert Results-------------------------
            Assert.IsNotNull(enumerator);
            var count = 0;
            while(enumerator.MoveNext())
            {
                count++;
                var current = enumerator.Current;
                Assert.AreEqual(current.Key, "key");
                Assert.AreEqual(current.Value, "value");
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_GetEnumerator")]
        public void QueryString_IEnumerableGetEnumerator_ItemsEnumerator()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var enumerator = ((IEnumerable)queryString).GetEnumerator();

            //------------Assert Results-------------------------
            Assert.IsNotNull(enumerator);
            var count = 0;
            while(enumerator.MoveNext())
            {
                count++;
                var current = enumerator.Current;
                Assert.IsInstanceOfType(current, typeof(KeyValuePair<string, string>));
                var kvp = (KeyValuePair<string, string>)current;
                Assert.AreEqual(kvp.Key, "key");
                Assert.AreEqual(kvp.Value, "value");
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_GetValues")]
        public void QueryString_GetValues_KeyFound_ValuesForKey()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString.GetValues("key");

            //------------Assert Results-------------------------
            Assert.IsNotNull(values);
            var i = 1;
            foreach(var value in values)
            {
                Assert.AreEqual("value" + i++, value);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_GetValues")]
        public void QueryString_GetValues_KeyNotFound_EmptyEnumerable()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString.GetValues("key1");

            //------------Assert Results-------------------------
            Assert.AreEqual(0, values.Count());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_Get")]
        public void QueryString_Get_KeyFound_ValuesForKey()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString.Get("key");

            //------------Assert Results-------------------------
            Assert.AreEqual("value1; value2", values);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_Get")]
        public void QueryString_Get_KeyNotFound_EmptyString()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString.Get("key1");

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, values);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_Item")]
        public void QueryString_Item_KeyFound_ValuesForKey()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString["key"];

            //------------Assert Results-------------------------
            Assert.AreEqual("value1; value2", values);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("QueryString_Item")]
        public void QueryString_Item_KeyNotFound_EmptyString()
        {
            //------------Setup for test--------------------------
            var items = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value1; value2") };

            var queryString = new QueryString(items);

            //------------Execute Test---------------------------
            var values = queryString["key1"];

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, values);
        }
    }
}
