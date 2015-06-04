using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
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
    public class TestPluginServiceTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestPluginService_HandlesType")]
        public void TestPluginService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var testPlugin = new TestPluginService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("TestPluginService", testPlugin.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestPluginService_Execute")]
        public void TestPluginService_Execute_ExpectCreateCalled()
        {
            //------------Setup for test--------------------------
            var testPluginService = new TestPluginService();

            PluginServiceDefinition psd = new PluginServiceDefinition();
            var srcId = Guid.NewGuid();
            psd.Source = new PluginSourceDefinition {Id = srcId};

            var ws = new Mock<IWorkspace>();
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var cat = new Mock<IResourceCatalog>();
            var source = new PluginSource();
     
  
       
           
            cat.Setup(a => a.GetResource<PluginSource>(Guid.Empty, It.IsAny<Guid>())).Returns(source);

            testPluginService.ResourceCatalogue = cat.Object;

            psd.Source = new PluginSourceDefinition();
            psd.Inputs = new IServiceInput[]{new ServiceInput("bob","builder"), };
            inputs.Add("PluginService", serializer.SerializeToBuilder(psd));
            var w = new Mock<IPluginServices>();
            testPluginService.PluginServices = w.Object;

            ws.Setup(a => a.ID).Returns(Guid.Empty);
           

            //------------Execute Test---------------------------
            var output = testPluginService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            w.Verify(a=>a.Test(It.IsAny<string>(),Guid.Empty,Guid.Empty));

        }

        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TestPluginService_HandlesType")]
        public void TestPluginService_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var testPluginService = new TestPluginService();


            //------------Execute Test---------------------------
            var a = testPluginService.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual(@"<DataList><Roles ColumnIODirection=""Input""/><PluginService ColumnIODirection=""Input""/><WorkspaceID ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}