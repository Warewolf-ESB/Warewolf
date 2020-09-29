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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;
using Unlimited.UnitTest.Framework.ConverterTests.GraphTests;


namespace Dev2.Tests.ConverterTests.GraphTests.OutputTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ShapedXmlOutputFormatterTests
    {
        #region Private/Internal Methods
        internal PocoTestData GivenPocoWithParallelAndNestedEnumerables()
        {
            var testData = new PocoTestData
            {
                Name = "Brendon",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherBrendon",
                    Age = 31,
                },
            };

            var nestedTestData1 = new PocoTestData
            {
                Name = "Mo",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMo",
                    Age = 31,
                },
            };

            var nestedTestData2 = new PocoTestData
            {
                Name = "Trav",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherTrav",
                    Age = 31,
                },
            };

            var nestedTestData3 = new PocoTestData
            {
                Name = "Jayd",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherJayd",
                    Age = 31,
                },
            };

            var nestedTestData4 = new PocoTestData
            {
                Name = "Dan",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherDan",
                    Age = 31,
                },
            };

            var nestedTestData5 = new PocoTestData
            {
                Name = "Mark",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMark",
                    Age = 31,
                },
            };

            var nestedTestData6 = new PocoTestData
            {
                Name = "Warren",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherWarren",
                    Age = 31,
                },
            };

            var nestedTestData8 = new PocoTestData
            {
                Name = "Franco",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherFranco",
                    Age = 31,
                },
            };

            var nestedTestData9 = new PocoTestData
            {
                Name = "Taryn",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherTaryn",
                    Age = 31,
                },
            };

            var nestedTestData10 = new PocoTestData
            {
                Name = "Melissa",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMelissa",
                    Age = 31,
                },
            };

            var nestedTestData11 = new PocoTestData
            {
                Name = "Melanie",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherMelanie",
                    Age = 31,
                },
            };

            var nestedTestData12 = new PocoTestData
            {
                Name = "Justin",
                Age = 30,
                NestedData = new PocoTestData
                {
                    Name = "AnotherJustin",
                    Age = 31,
                },
            };


            nestedTestData1.EnumerableData = new List<PocoTestData> { nestedTestData3, nestedTestData4, nestedTestData6 };
            nestedTestData2.EnumerableData = new List<PocoTestData> { nestedTestData5 };

            testData.EnumerableData = new List<PocoTestData> { nestedTestData1, nestedTestData2 };
            testData.EnumerableData1 = new List<PocoTestData> { nestedTestData8, nestedTestData9, nestedTestData10, nestedTestData11, nestedTestData12 };

            return testData;
        }

        internal string GivenXml()
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

        internal string GivenJson()
        {
            return @"{
    ""Name"": ""Dev2"",
    ""Motto"": ""Eat lots of cake"",
    ""Departments"": [      
        {
          ""Name"": ""Dev"",
          ""Employees"": [
              {
                ""Name"": ""Brendon"",
                ""Surename"": ""Page""
              },
              {
                ""Name"": ""Jayd"",
                ""Surename"": ""Page""
              }
            ]
        },
        {
          ""Name"": ""Accounts"",
          ""Employees"": [
              {
                ""Name"": ""Bob"",
                ""Surename"": ""Soap""
              },
              {
                ""Name"": ""Joe"",
                ""Surename"": ""Pants""
              }
            ]
        }
      ],
    ""Contractors"": [      
        {
          ""Name"": ""Roofs Inc."",
          ""PhoneNumber"": ""123"",
        },
        {
          ""Name"": ""Glass Inc."",
          ""PhoneNumber"": ""1234"",
        },
        {
          ""Name"": ""Doors Inc."",
          ""PhoneNumber"": ""1235"",
        },
        {
          ""Name"": ""Cakes Inc."",
          ""PhoneNumber"": ""1236"",
        }
      ],
    ""PrimitiveRecordset"": [
      ""
        RandomData
    "",
      ""
        RandomData1
    ""
    ],
  }";
        }

        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }
        #endregion Private/Internal Methods

        #region Scalar Tests
        /// <summary>
        /// Format with scalar in output description from json expected XML with scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithScalarInOutputDescriptionFromJson_Expected_XmlWithScalarValue()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Name>Dev2</Name>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant scalar in output description from json expected XML with emptycalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantScalarInOutputDescriptionFromJson_Expected_XmlWithEmptycalarValue()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("CakeName", "CakeName", "[[CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <CakeName></CakeName>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with scalar in output description from XML expected XML with scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithScalarInOutputDescriptionFromXml_Expected_XmlWithScalarValue()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Name>Dev2</Name>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant scalar in output description from XML expected XML with empty scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantScalarInOutputDescriptionFromXml_Expected_XmlWithEmptyScalarValue()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:CakeName", "Company:CakeName", "[[CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <CakeName></CakeName>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with scalar in output description from reference type expected XML with scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithScalarInOutputDescriptionFromReferenceType_Expected_XmlWithScalarValue()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Name>Brendon</Name>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant scalar in output description from reference type expected XML with empty scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantScalarInOutputDescriptionFromReferenceType_Expected_XmlWithEmptyScalarValue()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("CakeName", "CakeName", "[[CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <CakeName />
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }
        #endregion Scalar Tests

        #region Enumerable Tests
        /// <summary>
        /// Format the with enumerable in output description from json expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithEnumerableInOutputDescriptionFromJson_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Departments().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Departments>
    <Name>Dev</Name>
  </Departments>
  <Departments>
    <Name>Accounts</Name>
  </Departments>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected,ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant enumerable in output description from json expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantEnumerableInOutputDescriptionFromJson_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Departments().CakeName", "Departments.CakeName", "[[Departments().CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Departments>
    <CakeName></CakeName>
  </Departments>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with nested enumerables in output description from json expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNestedEnumerablesInOutputDescriptionFromJson_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant nested enumerables in output description from json expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantNestedEnumerablesInOutputDescriptionFromJson_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().CakeName", "Departments.Employees.CakeName", "[[Names().EmployeeCakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated enumerables in output description from json expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedEnumerablesInOutputDescriptionFromJson_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset", "[[Names().InnerName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Brendon</EmployeeName>
    <InnerName>
        RandomData
    ,
        RandomData1
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Jayd</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Bob</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Joe</EmployeeName>
    <InnerName></InnerName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated and nested enumerables in output description from json expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedAndNestedEnumerablesInOutputDescriptionFromJson_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset", "[[Names().InnerName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
    <InnerName>
        RandomData
    ,
        RandomData1
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
    <InnerName></InnerName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with enumerable in output description from XML expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithEnumerableInOutputDescriptionFromXml_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Departments().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Departments>
    <Name>Dev</Name>
  </Departments>
  <Departments>
    <Name>Accounts</Name>
  </Departments>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant enumerable in output description from XML expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantEnumerableInOutputDescriptionFromXml_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:CakeName", "Company.Departments.Department:CakeName", "[[Departments().CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Departments>
    <CakeName></CakeName>
  </Departments>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with nested enumerables in output description from XML expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNestedEnumerablesInOutputDescriptionFromXml_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant nested enumerables in output description from XML expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantNestedEnumerablesInOutputDescriptionFromXml_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().PersonCake:Name", "Company.Departments.Department.Employees.Person:CakeName", "[[Names().EmployeeCakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeCakeName></EmployeeCakeName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated enumerables in output description from XML expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedEnumerablesInOutputDescriptionFromXml_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company().InlineRecordSet", "Company().InlineRecordSet", "[[Names().InnerName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Brendon</EmployeeName>
    <InnerName>
        RandomData
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Jayd</EmployeeName>
    <InnerName>
        RandomData1
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Bob</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <EmployeeName>Joe</EmployeeName>
    <InnerName></InnerName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated and nested enumerables in output description from XML expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedAndNestedEnumerablesInOutputDescriptionFromXml_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company().InlineRecordSet", "Company().InlineRecordSet", "[[Names().InnerName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
    <InnerName>
        RandomData
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
    <InnerName>
        RandomData1
    </InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
    <InnerName></InnerName>
  </Names>
  <Names>
    <CompanyName>Dev2</CompanyName>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
    <InnerName></InnerName>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with enumerable in output description from reference type expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithEnumerableInOutputDescriptionFromReferenceType_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <Name>Mo</Name>
  </Names>
  <Names>
    <Name>Trav</Name>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant enumerable in output description from reference type expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantEnumerableInOutputDescriptionFromReferenceType_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().CakeName", "EnumerableData.CakeName", "[[Names().CakeName]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <CakeName />
  </Names>
  <Names>
    <CakeName />
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with nested enumerables in output description from reference type_ expected_ XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNestedEnumerablesInOutputDescriptionFromReferenceType_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().RootName]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().NameAtLevel1]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name", "[[Names().NameAtLevel2]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <NameAtLevel2>Jayd</NameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <NameAtLevel2>Dan</NameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <NameAtLevel2>Warren</NameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Trav</NameAtLevel1>
    <NameAtLevel2>Mark</NameAtLevel2>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with non existant nested enumerables in output description from reference type expected XML with empty recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithNonExistantNestedEnumerablesInOutputDescriptionFromReferenceType_Expected_XmlWithEmptyRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().RootName]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().NameAtLevel1]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().EnumerableData().CakeName", "EnumerableData.EnumerableData.CakeName", "[[Names().CakeNameAtLevel2]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <CakeNameAtLevel2></CakeNameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <CakeNameAtLevel2></CakeNameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Mo</NameAtLevel1>
    <CakeNameAtLevel2></CakeNameAtLevel2>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1>Trav</NameAtLevel1>
    <CakeNameAtLevel2></CakeNameAtLevel2>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated enumerables in output description from reference type expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedEnumerablesInOutputDescriptionFromReferenceType_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().RootName]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().NameAtLevel1a]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData1().Name", "EnumerableData1.Name", "[[Names().NameAtLevel1b]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel1b>Franco</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Trav</NameAtLevel1a>
    <NameAtLevel1b>Taryn</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a></NameAtLevel1a>
    <NameAtLevel1b>Melissa</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a></NameAtLevel1a>
    <NameAtLevel1b>Melanie</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a></NameAtLevel1a>
    <NameAtLevel1b>Justin</NameAtLevel1b>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with unrelated and nested enumerables in output description from reference type expected XML with recordset values.
        /// </summary>
        [TestMethod]
        public void FormatWithUnrelatedAndNestedEnumerablesInOutputDescriptionFromReferenceType_Expected_XmlWithRecordsetValues()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().RootName]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().NameAtLevel1a]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name", "[[Names().NameAtLevel2a]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData1().Name", "EnumerableData1.Name", "[[Names().NameAtLevel1b]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Jayd</NameAtLevel2a>
    <NameAtLevel1b>Franco</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Dan</NameAtLevel2a>
    <NameAtLevel1b>Taryn</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Warren</NameAtLevel2a>
    <NameAtLevel1b>Melissa</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a>Trav</NameAtLevel1a>
    <NameAtLevel2a>Mark</NameAtLevel2a>
    <NameAtLevel1b>Melanie</NameAtLevel1b>
  </Names>
  <Names>
    <RootName>Brendon</RootName>
    <NameAtLevel1a></NameAtLevel1a>
    <NameAtLevel2a></NameAtLevel2a>
    <NameAtLevel1b>Justin</NameAtLevel1b>
  </Names>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }
        #endregion Enumerable Tests

        #region Multiple Output Tests
        /// <summary>
        /// Format with multiple output expressions in output description from json expected XML with recordset values and a scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithMultipleOutputExpressionsInOutputDescriptionFromJson_Expected_XmlWithRecordsetValuesAndAScalarValue()
        {
            var testData = GivenJson();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[ScalarName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset", "[[OtherNames().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <ScalarName>Dev2</ScalarName>
  <Names>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
  </Names>
  <OtherNames>
    <Name>
        RandomData
    </Name>
  </OtherNames>
  <OtherNames>
    <Name>
        RandomData1
    </Name>
  </OtherNames>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with multiple output expressions in output description from XML expected XML with recordset values and A scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithMultipleOutputExpressionsInOutputDescriptionFromXml_Expected_XmlWithRecordsetValuesAndAScalarValue()
        {
            var testData = GivenXml();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[ScalarNames]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company().InlineRecordSet", "Company().InlineRecordSet", "[[OtherNames().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <ScalarNames>Dev2</ScalarNames>
  <Names>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Brendon</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Dev</DepartmentName>
    <EmployeeName>Jayd</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Bob</EmployeeName>
  </Names>
  <Names>
    <DepartmentName>Accounts</DepartmentName>
    <EmployeeName>Joe</EmployeeName>
  </Names>
  <OtherNames>
    <Name>
        RandomData
    </Name>
  </OtherNames>
  <OtherNames>
    <Name>
        RandomData1
    </Name>
  </OtherNames>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Format with multiple output expressions in output description from reference type expected XML with recordset values and A scalar value.
        /// </summary>
        [TestMethod]
        public void FormatWithMultipleOutputExpressionsInOutputDescriptionFromReferenceType_Expected_XmlWithRecordsetValuesAndAScalarValue()
        {
            var testData = GivenPocoWithParallelAndNestedEnumerables();

            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[ScalarName]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().Name", "EnumerableData.Name", "[[Names().NameAtLevel1a]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData().EnumerableData().Name", "EnumerableData.EnumerableData.Name", "[[Names().NameAtLevel2a]]"));
            dataSourceShape.Paths.Add(new PocoPath("EnumerableData1().Name", "EnumerableData1.Name", "[[OtherNames().Name]]"));

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputFormatter = new ShapedXmlOutputFormatter(outputDescription) { RootNodeName = "ADL" };

            var expected = @"<ADL>
  <ScalarName>Brendon</ScalarName>
  <Names>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Jayd</NameAtLevel2a>
  </Names>
  <Names>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Dan</NameAtLevel2a>
  </Names>
  <Names>
    <NameAtLevel1a>Mo</NameAtLevel1a>
    <NameAtLevel2a>Warren</NameAtLevel2a>
  </Names>
  <Names>
    <NameAtLevel1a>Trav</NameAtLevel1a>
    <NameAtLevel2a>Mark</NameAtLevel2a>
  </Names>
  <OtherNames>
    <Name>Franco</Name>
  </OtherNames>
  <OtherNames>
    <Name>Taryn</Name>
  </OtherNames>
  <OtherNames>
    <Name>Melissa</Name>
  </OtherNames>
  <OtherNames>
    <Name>Melanie</Name>
  </OtherNames>
  <OtherNames>
    <Name>Justin</Name>
  </OtherNames>
</ADL>";

            var actual = outputFormatter.Format(testData).ToString();
            FixBreaks(ref expected, ref actual);
            Assert.AreEqual(expected, actual);
        }
        #endregion Multiple Output Tests  
    }
}
