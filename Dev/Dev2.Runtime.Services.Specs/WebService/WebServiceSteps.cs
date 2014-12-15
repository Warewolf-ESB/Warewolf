
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.Services.Specs.WebService
{
    [Binding]
    public class WebServiceSteps : Steps
    {
        #region const data
        const string RequestResponse = @"{
    ""Name"": ""Dev2"",
    ""Motto"": ""Eat lots of cake"",
    ""Departments"": [      
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
            ],
         ""Areas"": [
              ""RandomData"",""RandomData1""
            ]
        }
      ]
  }";
        #endregion

        [Given(@"I have a webservice")]
        public void GivenIHaveAWebservice()
        {
            var webSourceXML = XmlResource.Fetch("Google_Address_Lookup");
            var webSource = new WebSource(webSourceXML);
            var service = new ServiceModel.Data.WebService { Source = webSource };
            ScenarioContext.Current.Add("WebService", service);
        }

        [Given(@"the webservice returns JSON with a primitive array")]
        public void GivenTheWebserviceReturnsJSONWithAPrimitiveArray()
        {
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            webService.RequestResponse = RequestResponse;
        }

        [When(@"the mapping is generated")]
        public void WhenTheMappingIsGenerated()
        {
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            IOutputDescription outputDescription = webService.GetOutputDescription();
            webService.OutputDescription = outputDescription;
            outputDescription.ToRecordsetList(webService.Recordsets);
            webService.OutputSpecification = webService.GetOutputString(webService.Recordsets);
        }

        [Given(@"I have a webservice calling http://maps\.googleapis\.com/maps/api/geocode/json\?sensor=true&amp;address=address")]
        public void GivenIHaveAWebserviceCallingHttpMaps_Googleapis_ComMapsApiGeocodeJsonSensorTrueAmpAddressAddress()
        {
            var webSourceXML = XmlResource.Fetch("Google_Address_Lookup");
            var webSource = new WebSource(webSourceXML);
            var service = new ServiceModel.Data.WebService { Source = webSource, RequestUrl = webSource.DefaultQuery };
            ErrorResultTO errors;
            WebServices.ExecuteRequest(service, false, out errors);
            ScenarioContext.Current.Add("WebService", service);
        }

        [When(@"the service is executed")]
        public void WhenTheServiceIsExecuted()
        {
            When(@"the mapping is generated");
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            var shape = compiler.ShapeDev2DefinitionsToDataList(webService.OutputSpecification, enDev2ArgumentType.Output, false, out errors);
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "", shape, out errors);
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);

            var serviceExecution = new WebserviceExecution(dataObj.Object, true);

            var webSource = webService.Source as WebSource;

            Assert.IsNotNull(webSource);
            serviceExecution.Service = webService;
            serviceExecution.Source = webSource;
            serviceExecution.InstanceOutputDefintions = webService.OutputSpecification;
            Guid executeID = serviceExecution.Execute(out errors);
            ScenarioContext.Current.Add("DataListID", executeID);
        }

        [Then(@"the mapping should contain the primitive array")]
        public void ThenTheMappingShouldContainThePrimitiveArray()
        {
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            RecordsetList recordsetList = webService.Recordsets;
            Assert.IsNotNull(recordsetList);
            var departmentsRecordSet = recordsetList.Find(recordset => recordset.Name == "Departments");
            Assert.IsNotNull(departmentsRecordSet);
            List<RecordsetField> departmentFields = departmentsRecordSet.Fields;
            Assert.AreEqual(2, departmentFields.Count);
            RecordsetField departmentNameField = departmentFields.Find(field => field.Name == "Name");
            Assert.IsNotNull(departmentNameField);
            RecordsetField departmentAreasField = departmentFields.Find(field => field.Name == "Areas");
            Assert.IsNotNull(departmentAreasField);
        }

        [Then(@"I have the following data")]
        public void ThenIHaveTheFollowingData(Table table)
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataListID = ScenarioContext.Current.Get<Guid>("DataListID");
            ErrorResultTO errors;
            var result = compiler.ConvertFrom(dataListID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            Assert.IsNotNull(result);
            var resultXml = XElement.Parse(result.ToString());
            var dataElements = resultXml.Elements().Where(element => !element.Name.LocalName.StartsWith("Dev2System") && element.Name.LocalName == "results");
            using(var dataSet = new DataSet())
            {

                using(var reader = dataElements.ToList()[0].CreateReader())
                {
                    dataSet.ReadXml(reader, XmlReadMode.Auto);
                    var dataListTable = dataSet.Tables[0];
                    var rowID = 0;
                    foreach(var tableRow in table.Rows)
                    {
                        var dataRow = dataListTable.Rows[rowID];
                        foreach(var header in table.Header)
                        {
                            Assert.AreEqual(dataRow[header], tableRow[header]);
                        }
                        rowID++;
                    }
                }
            }
        }

        [Given(@"I have a webservice with '(.*)' as a response")]
        public void GivenIHaveAWebserviceWithAsAResponse(string responseFileName)
        {
            var service = new ServiceModel.Data.WebService();
            var readToEnd = ReadFromAssemblyResource(responseFileName);
            service.RequestResponse = readToEnd;
            ScenarioContext.Current.Add("WebService", service);
        }

        [When(@"I apply '(.*)' to the response")]
        public void WhenIApplyToTheResponse(string jsonPath)
        {
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            webService.JsonPath = jsonPath;
            webService.ApplyPath();
            webService.RequestResponse = webService.JsonPathResult;
        }

        [Then(@"the mapping should be '(.*)'")]
        public void ThenTheMappingShouldBe(string resultingMapping)
        {
            var webService = ScenarioContext.Current.Get<ServiceModel.Data.WebService>("WebService");
            IOutputDescription outputDescription = webService.GetOutputDescription();
            var foundValidPath = outputDescription.DataSourceShapes.Find(shape => shape.Paths.Find(path => path.ActualPath == resultingMapping) != null);
            Assert.IsNotNull(foundValidPath);
        }



        static string ReadFromAssemblyResource(string responseFileName)
        {
            string readToEnd = "";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = string.Format("Dev2.Runtime.Services.Specs.{0}", responseFileName);
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream != null)
                {
                    readToEnd = new StreamReader(stream).ReadToEnd();
                }
            }
            return readToEnd;
        }
    }
}
