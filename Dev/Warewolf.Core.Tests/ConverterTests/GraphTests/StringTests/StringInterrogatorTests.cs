/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests {
    [TestClass]
    public class StringInterrogatorTests {

        internal string XmlGiven() {
            return @"<Company Name='Dev2'>
    <Motto>Eat lots of cake</Motto>
    <PreviousMotto/>
	<Departments TestAttrib='testing'>
		<Department Name='Dev'>
			<Employees>
				<Person Name='Brendon' Surename='Page' />
				<Person Name='Jayd' Surename='Page' />
			</Employees>
		</Department>
		<Department Name='Accounts'>
			<Employees>
				<Person Name='Bob' Surename='Soap' />
				<Person Name='Joe' Surename='Pants' />
			</Employees>
		</Department>
	</Departments>
    <InlineRecordSet>
        RandomData
    </InlineRecordSet>
    <InlineRecordSet>
        RandomData1
    </InlineRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val1' />
        <InnerNestedRecordSet ItemValue='val2' />
    </OuterNestedRecordSet>
    <OuterNestedRecordSet>
        <InnerNestedRecordSet ItemValue='val3' />
        <InnerNestedRecordSet ItemValue='val4' />
    </OuterNestedRecordSet>
</Company>";
        }



        /// <summary>
        /// Create mapper given XML expected XML mapper created.
        /// </summary>
        [TestMethod]
        public void CreateMapper_Expected_XmlMapper() {         
            var stringInterrogator = new StringInterrogator();

            var mapper = stringInterrogator.CreateMapper(XmlGiven());

            var expected = typeof(XmlMapper);
            var actual = mapper.GetType();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create navigator given xml expected XML navigator returned.
        /// </summary>
        [TestMethod]
        public void CreateNavigator_Expected_XmlNavigator() {            
            var stringInterrogator = new StringInterrogator();

            var navigator = stringInterrogator.CreateNavigator(XmlGiven(), typeof(XmlPath));

            var expected = typeof(XmlNavigator);
            var actual = navigator.GetType();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CreateNavigator_Given_TypeofIPath_Expected_Exception()
        {
            var stringInterrogator = new StringInterrogator();
            stringInterrogator.CreateNavigator(XmlGiven(), typeof(IPath));
        }

        [TestMethod]        
        public void CreateNavigator_Given_TypeofPocoPath_Expected_PocoNavigator()
        {
            var stringInterrogator = new StringInterrogator();
            var navigator = stringInterrogator.CreateNavigator(XmlGiven(), typeof(PocoPath));
            Assert.IsNotNull(navigator);
            Assert.IsTrue(navigator.GetType() == typeof(PocoNavigator));
        }

        [TestMethod]        
        public void CreateNavigator_Given_TypeofUnExistingType_Expected_PocoPath()
        {
            var stringInterrogator = new StringInterrogator();
            var navigator = stringInterrogator.CreateNavigator(XmlGiven(), typeof(UnExistingType));
            Assert.IsNull(navigator);
        }
    }

    class UnExistingType: BasePath
    {
        #region Overrides of BasePath

        public override IEnumerable<IPathSegment> GetSegements()
        {
            yield break;
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            return null;
        }

        #endregion
    }
}
