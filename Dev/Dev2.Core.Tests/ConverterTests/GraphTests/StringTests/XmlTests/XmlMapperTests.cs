
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
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.StringTests.XmlTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XmlMapperTests
    {
        internal string Given()
        {
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


        #region Map Tests

        /// <summary>
        /// Map XML with attribute expected path to attribute returned.
        /// </summary>
        [TestMethod]
        public void MapXmlWithAttribute_Expected_PathToAttribute()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company:Name"));
        }

        /// <summary>
        /// Map XML with scalar value expected path to scalar value returned.
        /// </summary>
        [TestMethod]
        public void MapXmlWithScalarValue_Expected_PathToScalarValue()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company.Motto"));
        }

        /// <summary>
        /// Map XML with blank scalar value expected path to scalar value returned.
        /// </summary>
        [TestMethod]
        public void MapXmlWithBlankScalarValue_Expected_PathToScalarValue()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company.PreviousMotto"));
        }

        /// <summary>
        /// Map XML with A recordset and attributes on items in the recordset_ expected_ path to attribute of elements in recordset.
        /// </summary>
        [TestMethod]
        public void MapXmlWithARecordsetAndAttributesOnItemsInTheRecordset_Expected_PathToAttributeOfElementsInRecordset()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company.Departments().Department:Name"));
        }

        /// <summary>
        /// Map XML with a recordset and attributes on the recordset expected path to attribute of recordset returned.
        /// </summary>
        [TestMethod]
        public void MapXmlWithARecordsetAndAttributesOnTheRecordset_Expected_PathToAttributeOfRecordset()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company.Departments:TestAttrib"));
        }

        /// <summary>
        /// Map XML with a inline recordset expected path to items in inner recordset returned.
        /// </summary>
        [TestMethod]
        public void MapXmlWithAInlineRecordset_Expected_PathToItemsInInnerRecordset()
        {
            XmlMapper xmlMapper = new XmlMapper();

            string xml = Given();
            IEnumerable<IPath> paths = xmlMapper.Map(xml);

            Assert.IsTrue(paths.Any(p => p.ActualPath == "Company().InlineRecordSet"));
        }

        #endregion Map Tests
    }
}
