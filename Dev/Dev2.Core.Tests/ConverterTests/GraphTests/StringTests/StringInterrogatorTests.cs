/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String;
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
            StringInterrogator stringInterrogator = new StringInterrogator();

            IMapper mapper = stringInterrogator.CreateMapper(XmlGiven());

            Type expected = typeof(XmlMapper);
            Type actual = mapper.GetType();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Create navigator given xml expected XML navigator returned.
        /// </summary>
        [TestMethod]
        public void CreateNavigator_Expected_XmlNavigator() {            
            StringInterrogator stringInterrogator = new StringInterrogator();

            INavigator navigator = stringInterrogator.CreateNavigator(XmlGiven(), typeof(XmlPath));

            Type expected = typeof(XmlNavigator);
            Type actual = navigator.GetType();

            Assert.AreEqual(expected, actual);
        }
    }
}
