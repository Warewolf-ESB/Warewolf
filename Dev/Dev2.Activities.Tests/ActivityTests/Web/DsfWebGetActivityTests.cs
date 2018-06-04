using System;
using System.Activities;
using System.Collections.Generic;
using System.Text;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class DsfWebGetActivityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebGetActivity_Execute")]
        public void DsfWebGetActivity_Execute_WithValidTextResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Location\": \"Paris\",\"Time\": \"May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC\"," +
                                    "\"Wind\": \"from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0\"," +
                                    "\"Visibility\": \"greater than 7 mile(s):0\"," +
                                    "\"Temperature\": \"59 F (15 C)\"," +
                                    "\"DewPoint\": \"41 F (5 C)\"," +
                                    "\"RelativeHumidity\": \"51%\"," +
                                    "\"Pressure\": \"29.65 in. Hg (1004 hPa)\"," +
                                    "\"Status\": \"Success\"" +
                                    "}";
            var environment = new ExecutionEnvironment();
            environment.Assign("[[City]]", "PMB", 0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            var dsfWebGetActivity = new TestDsfWebGetActivity();
            dsfWebGetActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            dsfWebGetActivity.Inputs = serviceInputs;
            dsfWebGetActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebGetActivity.OutputDescription = service.GetOutputDescription();
            dsfWebGetActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebGetActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebGetActivity.QueryString = "";

            dsfWebGetActivity.SourceId = Guid.Empty;
            dsfWebGetActivity.Headers = new List<INameValue>();
            dsfWebGetActivity.OutputDescription = new OutputDescription();
            dsfWebGetActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebGetActivity.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetActivity_Execute")]
        public void DsfWebGetActivity_Execute_ErrorResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Message\":\"Error\"}";
            var environment = new ExecutionEnvironment();

            var dsfWebGetActivity = new TestDsfWebGetActivity();
            dsfWebGetActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;

            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Message", "[[Message]]", "") };
            dsfWebGetActivity.Outputs = serviceOutputs;

            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebGetActivity.OutputDescription = service.GetOutputDescription();
            dsfWebGetActivity.ResponseFromWeb = response;
            dsfWebGetActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebGetActivity.QueryString = "Error";

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
          
            dsfWebGetActivity.SourceId = Guid.Empty;
            dsfWebGetActivity.Headers = new List<INameValue>();
            dsfWebGetActivity.OutputDescription = new OutputDescription();
            dsfWebGetActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Message]]", 0)));
        }
    }

    public class MockEsb : IEsbChannel
    {

        #region Not Implemented

        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceID,
                                   out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return Guid.NewGuid();
        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public string FindServiceShape(Guid workspaceID, string serviceName, int update)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceID, Guid resourceID)
        {
            return null;
        }

        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(
            IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors,
                                    IDataListCompiler compiler)
        {
            throw new NotImplementedException();
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceID, string uri,
                                           out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            return null;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {
        }

        #endregion

        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs,
                                      out ErrorResultTO errors, int update, bool b)
        {
                        
            errors = new ErrorResultTO();
            return dataObject.Environment;
        }
    }

    public class TestDsfWebGetActivity : DsfWebGetActivity
    {
        #region Overrides of DsfWebPutActivity

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebRequest(IEnumerable<NameValue> head, string query, WebSource url)
        {
            return ResponseFromWeb;
        }

        #endregion
    }
}
