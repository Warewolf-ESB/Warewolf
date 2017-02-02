/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.ConverterTests.GraphTests.StringTests.JsonTest
{
    [TestClass]
    public class JsonPathSegmentTests {
        
        #region CreatePathSegment Tests

        /// <summary>
        /// To the string on enumerable segment expected enumerable format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_Expected_EnumerableFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            const string expected = "Collection()";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on enumerable segment where enumerables arent considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesArentConsidered_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            const string expected = "Collection";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on enumerable segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnEnumerableSegment_WhereEnumerablesAreConsidered_Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Collection()");

            const string expected = "Collection()";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment where enumerables aren't considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_WhereEnumerablesArentConsidered__Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString(false);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// To the string on scalar segment where enumerables are considered expected scalar format returned.
        /// </summary>
        [TestMethod]
        public void ToStringOnScalarSegment_WhereEnumerablesAreConsidered__Expected_ScalarFormat()
        {
            JsonPath path = new JsonPath();
            IPathSegment segment = path.CreatePathSegment("Name");

            const string expected = "Name";
            string actual = segment.ToString(true);

            Assert.AreEqual(expected, actual);
        }

        #endregion CreatePathSegment Tests
    }
}
