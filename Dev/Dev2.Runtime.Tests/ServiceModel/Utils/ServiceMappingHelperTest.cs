using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Utils;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.Output;

namespace Dev2.Tests.Runtime.ServiceModel.Utils
{
    /// <summary>
    /// Summary description for ServiceMappingHelperTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceMappingHelperTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceMappingHelper_MapDbOutputs")]
        public void ServiceMappingHelper_MapDbOutputs_WhenOutputsWithPaths_ExpectTwoOutputMappings()
        {
            //------------Setup for test--------------------------

            var outputDefs = @"<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:_x003C_Paths_x003E_k__BackingField><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Dev2.Converters.Graph.DataTable"" i:type=""d5p1:DataTablePath""><_x003C_ActualPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">CountryID</_x003C_ActualPath_x003E_k__BackingField><_x003C_DisplayPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">CountryID</_x003C_DisplayPath_x003E_k__BackingField><_x003C_OutputExpression_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">[[dbo_Pr_CitiesGetCountries().CountryID]]</_x003C_OutputExpression_x003E_k__BackingField><_x003C_SampleData_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">1__COMMA__2__COMMA__3__COMMA__4__COMMA__5__COMMA__6__COMMA__7__COMMA__8__COMMA__9__COMMA__10</_x003C_SampleData_x003E_k__BackingField></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Dev2.Converters.Graph.DataTable"" i:type=""d5p1:DataTablePath""><_x003C_ActualPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Description</_x003C_ActualPath_x003E_k__BackingField><_x003C_DisplayPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Description</_x003C_DisplayPath_x003E_k__BackingField><_x003C_OutputExpression_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">[[dbo_Pr_CitiesGetCountries().Description]]</_x003C_OutputExpression_x003E_k__BackingField><_x003C_SampleData_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Afghanistan__COMMA__Albania__COMMA__Algeria__COMMA__Andorra__COMMA__Angola__COMMA__Argentina__COMMA__Armenia__COMMA__Australia__COMMA__Austria__COMMA__Azerbaijan</_x003C_SampleData_x003E_k__BackingField></d2p1:anyType></d1p1:_x003C_Paths_x003E_k__BackingField></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>";

            var serviceMappingHelper = new ServiceMappingHelper();
            IOutputDescription outputs = new OutputDescriptionSerializationService().Deserialize(outputDefs);
            outputs.DataSourceShapes.Add(new DataSourceShape());
            DbService theService = CreateCountriesDbService();
            theService.Recordset.Fields.Clear();

            //------------Execute Test---------------------------
            serviceMappingHelper.MapDbOutputs(outputs, ref theService, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, theService.Recordset.Fields.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceMappingHelper_MapDbOutputs")]
        public void ServiceMappingHelper_MapDbOutputs_WhenNoOutputsWithPaths_ExpectNoOutputMappings()
        {
            //------------Setup for test--------------------------
            var outputDefs = @"<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:_x003C_Paths_x003E_k__BackingField><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Dev2.Converters.Graph.DataTable"" i:type=""d5p1:DataTablePath""><_x003C_ActualPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Column1</_x003C_ActualPath_x003E_k__BackingField><_x003C_DisplayPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph""></_x003C_DisplayPath_x003E_k__BackingField><_x003C_OutputExpression_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph""></_x003C_OutputExpression_x003E_k__BackingField><_x003C_SampleData_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">the result</_x003C_SampleData_x003E_k__BackingField></d2p1:anyType></d1p1:_x003C_Paths_x003E_k__BackingField></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>";

            var serviceMappingHelper = new ServiceMappingHelper();
            IOutputDescription outputs = new OutputDescriptionSerializationService().Deserialize(outputDefs);
            outputs.DataSourceShapes.Add(new DataSourceShape());
            DbService theService = CreateCountriesDbService();
            theService.Recordset.Fields.Clear();

            //------------Execute Test---------------------------
            serviceMappingHelper.MapDbOutputs(outputs, ref theService, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, theService.Recordset.Fields.Count);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceMappingHelper_MapDbOutputs")]
        public void ServiceMappingHelper_MapDbOutputs_WhenNoOutputsContainNameWithDot_ExpectDotRemainsInNameReplacedInAlias()
        {
            //------------Setup for test--------------------------
            const string outputDefs = @"<z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:_x003C_Paths_x003E_k__BackingField><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Dev2.Converters.Graph.DataTable"" i:type=""d5p1:DataTablePath""><_x003C_ActualPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Column1</_x003C_ActualPath_x003E_k__BackingField><_x003C_DisplayPath_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">foo.bar</_x003C_DisplayPath_x003E_k__BackingField><_x003C_OutputExpression_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">foobar</_x003C_OutputExpression_x003E_k__BackingField><_x003C_SampleData_x003E_k__BackingField xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">the result</_x003C_SampleData_x003E_k__BackingField></d2p1:anyType></d1p1:_x003C_Paths_x003E_k__BackingField></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType>";

            var serviceMappingHelper = new ServiceMappingHelper();
            IOutputDescription outputs = new OutputDescriptionSerializationService().Deserialize(outputDefs);
            outputs.DataSourceShapes.Add(new DataSourceShape());
            DbService theService = CreateCountriesDbService();
            theService.Recordset.Fields.Clear();

            //------------Execute Test---------------------------
            serviceMappingHelper.MapDbOutputs(outputs, ref theService, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, theService.Recordset.Fields.Count);
            Assert.AreEqual("foo.bar", theService.Recordset.Fields[0].Name);
            Assert.AreEqual("foobar", theService.Recordset.Fields[0].Alias);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceMappingHelper_MapDbOutputs")]
        public void ServiceMappingHelper_MapDbOutputs_WhenSampleDataContainsCommaReplacement_Expect10SampleItems()
        {
            //
            // 2013.12.11 - COMMUNITY BUG - 341463 - Fixed test to include empty cells
            //

            //------------Setup for test--------------------------
            var expectedData = CreateDbServiceLocationsDataTable();

            // OutputDescription MUST contain empty cells!!!
            var outputDefs = XmlResource.Fetch("DbServiceLocationsOutputDescription");

            var serviceMappingHelper = new ServiceMappingHelper();
            var outputs = new OutputDescriptionSerializationService().Deserialize(outputDefs.ToString());
            outputs.DataSourceShapes.Add(new DataSourceShape());
            var theService = CreateCountriesDbService();
            theService.Recordset.Fields.Clear();

            //------------Execute Test---------------------------
            serviceMappingHelper.MapDbOutputs(outputs, ref theService, true);

            //------------Assert Results-------------------------            
            Assert.AreEqual(expectedData.Columns.Count, theService.Recordset.Fields.Count);

            for(var i = 0; i < expectedData.Rows.Count; i++)
            {
                var record = theService.Recordset.Records[i];
                Assert.AreEqual(expectedData.Columns.Count, record.Cells.Length);

                for(var j = 0; j < expectedData.Columns.Count; j++)
                {
                    var expected = expectedData.Rows[i][j];
                    var actual = record.Cells[j].Value;
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        static DataTable CreateDbServiceLocationsDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("CountryID", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Latitude", typeof(string));
            dt.Columns.Add("Longitude", typeof(string));

            dt.Rows.Add(new object[] { "1", "19 Pineside Road, New Germany", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "2", "1244 Old North Coast Rd, Redhill", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "3", "Westmead Road, Westmead", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "4", "Turquoise Road, Queensmead", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "5", "Old Main Road, Isipingo", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "7", "2 Brook Street North, Warwick Junction", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "8", "Bellair Road, Corner Bellair & Edwin Swales/Sarnia Arterial", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "9", "Riverside Road, Durban North", string.Empty, string.Empty });
            dt.Rows.Add(new object[] { "10", "Malacca Road, Durban North", string.Empty, string.Empty });

            return dt;
        }

        public static DbService CreateCountriesDbService()
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CountriesService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                Method = new ServiceMethod
                {
                    Name = "dbo.spGetCountries",
                    Parameters = new List<MethodParameter>(new[]
                    {
                        new MethodParameter { Name = "@Prefix", EmptyToNull = false, IsRequired = true, Value = string.Empty, DefaultValue = "b" }
                    })
                },
                Recordset = new Recordset
                {
                    Name = "Countries",
                },
                Source = new DbSource
                {

                    ResourceID = Guid.NewGuid(),
                    ResourceName = "CitiesDB",
                    ResourceType = ResourceType.DbSource,
                    ResourcePath = "Test",
                    Server = "RSAKLFSVRGENDEV",
                    DatabaseName = "Cities",
                    AuthenticationType = AuthenticationType.Windows,
                }
            };
            service.Recordset.Fields.AddRange(new[]
            {
                new RecordsetField { Name = "CountryID", Alias = "CountryID" },
                new RecordsetField { Name = "Description", Alias = "Name" }
            });

            return service;
        }
    }
}
