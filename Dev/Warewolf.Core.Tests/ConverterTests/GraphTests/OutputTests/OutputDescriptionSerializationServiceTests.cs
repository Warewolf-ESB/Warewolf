/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
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
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department:Name", "Company.Departments.Department:Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new XmlPath("Company.Departments().Department.Employees().Person:Name", "Company.Departments.Department.Employees.Person:Name", "[[Names().EmployeeName]]"));

            var testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            var serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            var deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            var expected = testOutputDescription.Format + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            var actual = deserializedOutputDescription.Format + "^" +
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
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new JsonPath("Name", "Name", "[[ScalarName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new JsonPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));
            dataSourceShape.Paths.Add(new JsonPath("PrimitiveRecordset()", "PrimitiveRecordset", "[[OtherNames().Name]]"));

            var testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            var serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            var deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            var expected = testOutputDescription.Format + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            var actual = deserializedOutputDescription.Format + "^" +
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
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new PocoPath("Name", "Name", "[[Names().CompanyName]]"));
            dataSourceShape.Paths.Add(new PocoPath("Departments().Name", "Departments.Name", "[[Names().DepartmentName]]"));
            dataSourceShape.Paths.Add(new PocoPath("Departments().Employees().Name", "Departments.Employees.Name", "[[Names().EmployeeName]]"));

            var testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            var serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);
            var deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(serializedData);

            var expected = testOutputDescription.Format + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", testOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            var actual = deserializedOutputDescription.Format + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.ActualPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.DisplayPath)) + "^" +
                string.Join("|", deserializedOutputDescription.DataSourceShapes.SelectMany(d => d.Paths).Select(p => p.OutputExpression));

            Assert.AreEqual(expected, actual);
        }
        #endregion Poco Paths

        #region Legacy POCO-fallback wire-format pinning (see BasePath.cs "Deliberately [Serializable]" comment)

        /// <summary>
        /// Known-good, already-persisted OutputDescription XML sample (captured from the current
        /// [Serializable]/POCO-fallback wire format via DataContractSerializer against a
        /// DataSourceShape containing a single XmlPath). This is a checked-in test asset per
        /// Defect 3 of the 8509-EOSGateFailure code review plan: if BasePath/DataSourceShape/
        /// PocoPath/JsonPath/XmlPath are ever changed back from [Serializable] to [DataContract],
        /// this sample will fail to deserialize into the expected shape (or the element names
        /// won't match), catching the regression immediately instead of silently corrupting
        /// already-persisted OutputDescription resources in production.
        /// </summary>
        const string LegacyPersistedOutputDescriptionSample =
            "<z:anyType xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:d1p1=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput\" i:type=\"d1p1:OutputDescription\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\">" +
            "<d1p1:DataSourceShapes xmlns:d2p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">" +
            "<d2p1:anyType i:type=\"d1p1:DataSourceShape\">" +
            "<d1p1:_x003C_Paths_x003E_k__BackingField>" +
            "<d2p1:anyType xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.String.Xml\" i:type=\"d5p1:XmlPath\">" +
            "<_x003C_ActualPath_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph\">Company:Name</_x003C_ActualPath_x003E_k__BackingField>" +
            "<_x003C_DisplayPath_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph\">Company:Name</_x003C_DisplayPath_x003E_k__BackingField>" +
            "<_x003C_OutputExpression_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph\">[[Names().CompanyName]]</_x003C_OutputExpression_x003E_k__BackingField>" +
            "<_x003C_SampleData_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph\" />" +
            "</d2p1:anyType>" +
            "</d1p1:_x003C_Paths_x003E_k__BackingField>" +
            "</d2p1:anyType>" +
            "</d1p1:DataSourceShapes>" +
            "<d1p1:Format>ShapedXML</d1p1:Format>" +
            "</z:anyType>";

        /// <summary>
        /// Defect3_Test1: deserializes the checked-in legacy-persisted sample and asserts the
        /// resulting object graph matches what a real, already-persisted OutputDescription
        /// resource is expected to contain.
        /// </summary>
        [TestMethod]
        public void Deserialize_LegacyPersistedOutputDescriptionXmlSample_MatchesExpectedObjectGraph()
        {
            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            var deserializedOutputDescription = outputDescriptionSerializationService.Deserialize(LegacyPersistedOutputDescriptionSample);

            Assert.IsNotNull(deserializedOutputDescription, "Legacy-persisted OutputDescription sample failed to deserialize - the [Serializable] POCO-fallback wire format may have changed.");
            Assert.AreEqual(OutputFormats.ShapedXML, deserializedOutputDescription.Format);
            Assert.AreEqual(1, deserializedOutputDescription.DataSourceShapes.Count);

            var path = deserializedOutputDescription.DataSourceShapes.Single().Paths.Single();
            Assert.IsInstanceOfType(path, typeof(XmlPath));
            Assert.AreEqual("Company:Name", path.ActualPath);
            Assert.AreEqual("Company:Name", path.DisplayPath);
            Assert.AreEqual("[[Names().CompanyName]]", path.OutputExpression);
            Assert.AreEqual(string.Empty, path.SampleData);
        }

        /// <summary>
        /// Defect3_Test2: serializes a DataSourceShape/XmlPath and pins the exact compiler-generated
        /// backing-field element names that DataContractSerializer's POCO fallback produces for
        /// [Serializable] (non-[DataContract]) types. If BasePath/DataSourceShape/PocoPath/JsonPath/
        /// XmlPath are ever switched back to [DataContract], these element names change (to the
        /// plain "ActualPath"/"Paths" etc. DataMember names) and this test fails immediately,
        /// instead of silently changing the wire format for already-persisted resources.
        /// </summary>
        [TestMethod]
        public void Serialize_DataSourceShapeWithXmlPath_PinsLegacyPocoFallbackBackingFieldElementNames()
        {
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            dataSourceShape.Paths.Add(new XmlPath("Company:Name", "Company:Name", "[[Names().CompanyName]]"));

            var testOutputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            testOutputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedData = outputDescriptionSerializationService.Serialize(testOutputDescription);

            StringAssert.Contains(serializedData, "_x003C_Paths_x003E_k__BackingField");
            StringAssert.Contains(serializedData, "_x003C_ActualPath_x003E_k__BackingField");
            StringAssert.Contains(serializedData, "_x003C_DisplayPath_x003E_k__BackingField");
            StringAssert.Contains(serializedData, "_x003C_OutputExpression_x003E_k__BackingField");
            StringAssert.Contains(serializedData, "_x003C_SampleData_x003E_k__BackingField");

            // The plain DataMember names (e.g. "<Paths>", "<ActualPath>") must NOT appear - their
            // presence would mean the type had switched to [DataContract] opt-in serialization,
            // changing the persisted wire format.
            StringAssert.DoesNotMatch(serializedData, new System.Text.RegularExpressions.Regex("<Paths>|<ActualPath>|<DisplayPath>|<OutputExpression>|<SampleData>"));
        }

        #endregion Legacy POCO-fallback wire-format pinning
    }
}
