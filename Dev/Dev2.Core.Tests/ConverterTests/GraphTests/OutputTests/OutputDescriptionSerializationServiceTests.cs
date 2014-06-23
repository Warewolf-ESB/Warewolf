using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Unlimited.UnitTest.Framework.ConverterTests.GraphTests.OutputTests
{
    [TestClass]    
    public class OutputDescriptionSerializationServiceTests
    {
        #region XML Paths
        /// <summary>
        /// Serializes the output description with XML paths expected deserialization to work.
        /// </summary>
        [TestMethod]
        public void SerializeOutputDescriptionWithXMLPaths_Expected_DeserializationToWork()
        {
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));

            IOutputDescription testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            string serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            IOutputDescription deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            string expected = testOutputDescription.Format.ToString() + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            string actual = deserializedOutputDescription.Format.ToString() + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            Assert.AreEqual(expected, actual);
        }
        #endregion XML Paths

        #region JSON Paths
        /// <summary>
        /// Serializes the output description with JSON paths expected deserialization to work.
        /// </summary>
        [TestMethod]
        public void SerializeOutputDescriptionWithJSONPaths_Expected_DeserializationToWork()
        {
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[ScalarName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset", "[[OtherNames().Name]]"));

            IOutputDescription testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            string serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            IOutputDescription deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            string expected = testOutputDescription.Format.ToString() + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            string actual = deserializedOutputDescription.Format.ToString() + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            Assert.AreEqual(expected, actual);
        }
        #endregion JSON Paths

        #region Poco Paths
        /// <summary>
        /// Serializes the output description with Poco paths expected deserialization to work.
        /// </summary>
        [TestMethod]
        public void SerializeOutputDescriptionWithPocoPaths_Expected_DeserializationToWork()
        {
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new PocoPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new PocoPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));

            IOutputDescription testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            string serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            IOutputDescription deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            string expected = testOutputDescription.Format.ToString() + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            string actual = deserializedOutputDescription.Format.ToString() + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            Assert.AreEqual(expected, actual);
        }
        #endregion Poco Paths
    }
}
