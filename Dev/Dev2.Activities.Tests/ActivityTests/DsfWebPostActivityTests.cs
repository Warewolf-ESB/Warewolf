﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfWebPostActivityTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructed")]
        public void DsfWebPostActivity_Constructed_Correctly_ShouldHaveInheritDsfActivity()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfWebPostActivity = new DsfWebPostActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfWebPostActivity,typeof(DsfActivity));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructor")]
        public void DsfWebPostActivity_Constructor_Correctly_ShouldSetTypeDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfWebPostActivity = new DsfWebPostActivity();
            //------------Assert Results-------------------------
            Assert.AreEqual("Web Post Request Connector", dsfWebPostActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructed")]
        public void DsfWebPostActivity_Constructed_Correctly_ShouldHaveCorrectProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var attributes = typeof(DsfWebPostActivity).GetCustomAttributes(false);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, attributes.Length);
            var toolDescriptor = attributes[0] as ToolDescriptorInfo;
            Assert.IsNotNull(toolDescriptor);
            Assert.AreEqual("Post Web Service", toolDescriptor.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithValidWebResponse_ShouldSetVariables()
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
            environment.Assign("[[City]]","PMB",0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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
            environment.Assign("[[City]]","PMB",0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            environment.Assign("[[Post]]", "Some data", 0);
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            //------------Assert Preconditions-------------------
            Assert.AreEqual(1,dsfWebPostActivity.Headers.Count);
            Assert.AreEqual("Header 1",dsfWebPostActivity.Headers.ToList()[0].Name);
            Assert.AreEqual("[[City]]",dsfWebPostActivity.Headers.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/[[CountryName]]", dsfWebPostActivity.QueryString);
            Assert.AreEqual("This is post:[[Post]]", dsfWebPostActivity.PostData);
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("PMB",dsfWebPostActivity.Head.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/South Africa", dsfWebPostActivity.QueryRes);
            Assert.AreEqual("This is post:Some data", dsfWebPostActivity.PostValue);
        }
    }

    public class TestDsfWebPostActivity : DsfWebPostActivity
    {
        #region Overrides of DsfWebPostActivity

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string postData)
        {
            Head = head;
            QueryRes = query;
            PostValue = postData;
            return ResponseFromWeb;
        }

        public string PostValue { get; set; }

        public string QueryRes { get; set; }

        public IEnumerable<NameValue> Head { get; set; }

        #endregion

        
    }
}
