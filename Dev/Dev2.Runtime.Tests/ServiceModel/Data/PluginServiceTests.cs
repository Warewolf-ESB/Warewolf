using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // BUG 9500 - 2013.05.31 - TWR : added proper testing

    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PluginServiceTests
    {
        #region CTOR

        [TestMethod]
        public void PluginServiceContructorWithDefaultExpectedInitializesProperties()
        {
            var service = new PluginService();
            Assert.AreEqual(Guid.Empty, service.ResourceID);
            Assert.AreEqual(ResourceType.PluginService, service.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginServiceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            var service = new PluginService(null);
        }

        [TestMethod]
        public void PluginServiceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var service = new PluginService(xml);
            Assert.AreNotEqual(Guid.Empty, service.ResourceID);
            Assert.IsTrue(service.IsUpgraded);
            Assert.AreEqual(ResourceType.PluginService, service.ResourceType);
        }

        [TestMethod]
        public void PluginServiceContructorWithValidXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("PluginService");

            var service = new PluginService(xml);
            VerifyEmbeddedPluginService(service);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void PluginServiceToXmlExpectedSerializesProperties()
        {
            var expected = new PluginService
            {
                Source = new PluginSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "TestWebSource",
                },
                Namespace = "abc.pqr",
            };

            expected.Method.Parameters.AddRange(
                new[]
                {
                    new MethodParameter
                    {
                        Name = "Param1",
                        DefaultValue = "123"
                    },
                    new MethodParameter
                    {
                        Name = "Param2",
                        DefaultValue = "456"
                    }
                });

            var rs1 = new Recordset
            {
                Name = "Recordset1()"
            };
            rs1.Fields.AddRange(new[]
            {
                new RecordsetField
                {
                    Name = "Field1",
                    Alias = "Alias1"
                },
                new RecordsetField
                {
                    Name = "Field2",
                    Alias = "Alias2",
                    Path = new XmlPath("actual", "display", "outputExpression", "sampleData")
                },
                new RecordsetField
                {
                    Name = "Field3",
                    Alias = null
                }
            });
            expected.Recordsets.Add(rs1);

            var xml = expected.ToXml();

            var actual = new PluginService(xml);

            Assert.AreEqual(expected.Source.ResourceType, actual.Source.ResourceType);
            Assert.AreEqual(expected.Source.ResourceID, actual.Source.ResourceID);
            Assert.AreEqual(expected.Source.ResourceName, actual.Source.ResourceName);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.Namespace, actual.Namespace);

            foreach(var expectedParameter in expected.Method.Parameters)
            {
                var actualParameter = actual.Method.Parameters.First(p => p.Name == expectedParameter.Name);
                Assert.AreEqual(expectedParameter.DefaultValue, actualParameter.DefaultValue);
            }

            foreach(var expectedRecordset in expected.Recordsets)
            {
                // expect actual to have removed recordset notation ()...
                var actualRecordset = actual.Recordsets.First(rs => rs.Name == expectedRecordset.Name.Replace("()", ""));
                foreach(var expectedField in expectedRecordset.Fields)
                {
                    var actualField = actualRecordset.Fields.FirstOrDefault(f => expectedField.Name == null ? f.Name == "" : f.Name == expectedField.Name);
                    Assert.IsNotNull(actualField);
                    Assert.AreEqual(expectedField.Alias ?? "", actualField.Alias);
                    if(actualField.Path != null)
                    {
                        Assert.AreEqual(expectedField.Path.ActualPath, actualField.Path.ActualPath);
                        Assert.AreEqual(expectedField.Path.DisplayPath, actualField.Path.DisplayPath);
                        Assert.AreEqual(string.Format("[[{0}]]", expectedField.Alias), actualField.Path.OutputExpression);
                        Assert.AreEqual(expectedField.Path.SampleData, actualField.Path.SampleData);
                    }
                }
            }
        }

        #endregion

        #region VerifyEmbeddedPluginService

        public static void VerifyEmbeddedPluginService(PluginService service)
        {
            Assert.AreEqual(Guid.Parse("89098b76-ac11-40b2-b3e8-b175314cb3bb"), service.ResourceID);
            Assert.AreEqual(ResourceType.PluginService, service.ResourceType);
            Assert.AreEqual(Guid.Parse("00746beb-46c1-48a8-9492-e2d20817fcd5"), service.Source.ResourceID);
            Assert.AreEqual("PluginTesterSource", service.Source.ResourceName);
            Assert.AreEqual("Dev2.Terrain.Mountain", service.Namespace);
            Assert.AreEqual("Echo", service.Method.Name);

            Assert.AreEqual("<root>hello</root>", service.Method.Parameters.First(p => p.Name == "text").DefaultValue);

            Assert.AreEqual("reverb", service.Recordsets[0].Fields.First(f => f.Name == "echo").Alias);
        }

        #endregion


    }
}