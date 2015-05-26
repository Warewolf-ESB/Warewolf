using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestWebserviceTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestWebService_HandlesType")]
        public void TestWebService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var addFolder = new TestWebService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("TestWebService", addFolder.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestWebService_Execute")]
        public void TestWebService_Execute_ExpectCreateCalled()
        {
            //------------Setup for test--------------------------
            var testWebService = new TestWebService();

            WebServiceDefinition wsd = new WebServiceDefinition();
            var srcId = Guid.NewGuid();
            wsd.Source = new WebServiceSourceDefinition {Id = srcId};

            var ws = new Mock<IWorkspace>();
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var cat = new Mock<IResourceCatalog>();
            WebSource source = new WebSource();
            source.Address = "bob";
  
       
           
            cat.Setup(a => a.GetResource<WebSource>(Guid.Empty, It.IsAny<Guid>())).Returns(source);

            testWebService.ResourceCatalogue = cat.Object;
           
            wsd.Source = new WebServiceSourceDefinition();
            wsd.QueryString = "";
            inputs.Add("WebService", serializer.SerializeToBuilder(wsd));
            var w = new Mock<IWebServices>();
            testWebService.WebServices = w.Object;

            ws.Setup(a => a.ID).Returns(Guid.Empty);
           

            //------------Execute Test---------------------------
            var output = testWebService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            w.Verify(a=>a.TestWebService(It.IsAny<WebService>()));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestWebService_Execute")]
        public void TestWebService_Execute_ValidSourceExpectTestOnWebservicesClass()
        {
            //------------Setup for test--------------------------
            var testWebService = new TestWebService();

            var ws = new Mock<IWorkspace>();
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var cat = new Mock<IResourceCatalog>();
            testWebService.ResourceCatalogue = cat.Object;
            WebServiceDefinition wsd = new WebServiceDefinition();
            wsd.Source = new WebServiceSourceDefinition();
            wsd.QueryString = "";
            inputs.Add("WebService", serializer.SerializeToBuilder(wsd));

            ws.Setup(a => a.ID).Returns(Guid.Empty);

            //------------Execute Test---------------------------
            var output = testWebService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(true, serializer.Deserialize<ExecuteMessage>(output).HasError);
            Assert.AreEqual("Unknown source", serializer.Deserialize<ExecuteMessage>(output).Message.ToString());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestWebService_HandlesType")]
        public void TestWebService_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var testWebService = new TestWebService();


            //------------Execute Test---------------------------
            var a = testWebService.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual(@"<DataList><Roles ColumnIODirection=""Input""/><WebService ColumnIODirection=""Input""/><WorkspaceID ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}