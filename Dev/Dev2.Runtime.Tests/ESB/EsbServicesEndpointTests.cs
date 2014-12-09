
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
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class EsbServicesEndpointTests
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("EsbServicesEndpoint_ExecuteSubRequest")]
        public void EsbServicesEndpoint_ExecuteSubRequest_IsRemoteWorkflowWhenRemoteExecutionInLocalContext_ExpectTrue()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.NewGuid();
            dataObject.IsRemoteInvokeOverridden = true;

            bool isLocalInvoke = false;

            var invoker = new Mock<IEsbServiceInvoker>();
            invoker.Setup(i => i.GenerateInvokeContainer(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>())).Callback(
                (IDSFDataObject pDataObject, string pServiceName, bool pIsLocal, Guid pMasterDataListID) =>
                {
                    isLocalInvoke = pIsLocal;
                });

            var endpoint = new EsbServicesEndpointMock(invoker.Object);
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            endpoint.ExecuteSubRequest(dataObject, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors);

            //------------Assert Results-------------------------
            Assert.IsTrue(isLocalInvoke);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EsbServicesEndpoint_ExecuteSubRequest")]
        public void EsbServicesEndpoint_ExecuteSubRequest_SetServiceName()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.NewGuid();
            dataObject.IsRemoteInvokeOverridden = false;

            dataObject.EnvironmentID = Guid.NewGuid();
            var invoker = new Mock<IEsbServiceInvoker>();
            var resource = new SerializableResource(){ResourceName = "bob" ,ResourceType = ResourceType.WorkflowService,DataList = "bobthebuilder" ,ResourceCategory="bob"};
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var retvalue = ser.Serialize(new List<SerializableResource>{ resource});
            invoker.Setup(i => i.GenerateInvokeContainer(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>())).Callback(
                (IDSFDataObject pDataObject, string pServiceName, bool pIsLocal, Guid pMasterDataListID) =>
                { }).Returns(new RemoteWorkflowExecutionContainerMock(new ServiceAction(), new DsfDataObject("",Guid.NewGuid()),new Workspace(Guid.NewGuid()),new EsbServicesEndpointMock(invoker.Object),new Mock<IResourceCatalog>().Object   ){GetRequestRespsonse = retvalue });

            var endpoint = new EsbServicesEndpointMock(invoker.Object);
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            endpoint.ExecuteSubRequest(dataObject, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors);

            //------------Assert Results-------------------------
            Assert.AreEqual("bob",dataObject.ServiceName);

        }



        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("EsbServicesEndpoint_ExecuteSubRequest")]
        public void EsbServicesEndpoint_ExecuteSubRequest_IsRemoteWorkflowWhenRemoteExecutionInRemoteContext_ExpectFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.NewGuid();

            bool isLocalInvoke = false;

            var invoker = new Mock<IEsbServiceInvoker>();
            invoker.Setup(i => i.GenerateInvokeContainer(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>())).Callback(
                (IDSFDataObject pDataObject, string pServiceName, bool pIsLocal, Guid pMasterDataListID) =>
                {
                    isLocalInvoke = pIsLocal;
                });

            var endpoint = new EsbServicesEndpointMock(invoker.Object);
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            endpoint.ExecuteSubRequest(dataObject, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors);

            //------------Assert Results-------------------------
            Assert.IsFalse(isLocalInvoke);

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EsbServicesEndpoint_ExecuteSubRequest")]
        public void EsbServicesEndpoint_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly()
        {
            Verify_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly(remoteInvokerID: Guid.NewGuid().ToString(), isRemoteWorkflow: true, expectedIsLocal: false);
            Verify_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly(remoteInvokerID: Guid.NewGuid().ToString(), isRemoteWorkflow: false, expectedIsLocal: true);
            Verify_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly(remoteInvokerID: null, isRemoteWorkflow: true, expectedIsLocal: false);
            Verify_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly(remoteInvokerID: null, isRemoteWorkflow: false, expectedIsLocal: true);
        }

        void Verify_ExecuteSubRequest_IsRemoteWorkflow_InvokesGenerateInvokeContainerCorrectly(bool isRemoteWorkflow, string remoteInvokerID, bool expectedIsLocal)
        {
            //------------Setup for test--------------------------
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(d => d.RemoteInvokerID).Returns(remoteInvokerID);
            dataObject.Setup(d => d.IsRemoteWorkflow()).Returns(isRemoteWorkflow);
            dataObject.Setup(d => d.ServiceName).Returns("xxxx");

            var isLocalInvoke = !expectedIsLocal;

            var invoker = new Mock<IEsbServiceInvoker>();
            invoker.Setup(i => i.GenerateInvokeContainer(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>())).Callback(
                (IDSFDataObject pDataObject, string pServiceName, bool pIsLocal, Guid pMasterDataListID) =>
                {
                    isLocalInvoke = pIsLocal;
                });

            var endpoint = new EsbServicesEndpointMock(invoker.Object);
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            endpoint.ExecuteSubRequest(dataObject.Object, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedIsLocal, isLocalInvoke);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbServicesEndpoint_ShapeForSubRequest")]
        public void EsbServicesEndpoint_ShapeForSubRequest_RemoteServiceTypeIsDbServiceOrInvokeStoredProcTypesAndHasOutputDefinitions_ShapedData()
        {
            ShapeForSubRequest("DbService");
            ShapeForSubRequest("InvokeStoredProc");
        }

        static void ShapeForSubRequest(string serviceType)
        {
            var esb = new EsbServicesEndpoint();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(d => d.RemoteInvokerID).Returns(Guid.NewGuid().ToString());
            dataObject.Setup(d => d.IsRemoteWorkflow()).Returns(true);
            dataObject.Setup(d => d.RemoteServiceType).Returns(serviceType);
            dataObject.Setup(d => d.ServiceName).Returns("xxxx");
            ErrorResultTO error;
            const string outputDefs = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[mapRecSet(*).LocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";
            const string inputDefs = @"";
            //------------Execute Test---------------------------
            var result = esb.ShapeForSubRequest(dataObject.Object, inputDefs, outputDefs, out error);
            //------------Assert Results-----------------   --------
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbServicesEndpoint_ShapeForSubRequest")]
        public void EsbServicesEndpoint_ShapeForSubRequest_RemoteServiceTypeIsWorkflowTypeAndHasOutputDefinitions_ShapedData()
        {
            //------------Setup for test--------------------------
            var esb = new EsbServicesEndpoint();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(d => d.RemoteInvokerID).Returns(Guid.NewGuid().ToString());
            dataObject.Setup(d => d.IsRemoteWorkflow()).Returns(true);
            dataObject.Setup(d => d.RemoteServiceType).Returns("Workflow");
            dataObject.Setup(d => d.ServiceName).Returns("xxxx");
            ErrorResultTO error;
            const string outputDefs = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[mapRecSet(*).LocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";
            const string inputDefs = @"";
            //------------Execute Test---------------------------
            var result = esb.ShapeForSubRequest(dataObject.Object, inputDefs, outputDefs, out error);
            //------------Assert Results-----------------   --------
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enDev2ArgumentType.Output, result[0].Key);
            Assert.AreEqual(1, result[0].Value.Count);
            var val = result[0].Value[0];
            Assert.IsNotNull(val);
            Assert.AreEqual("MapLocationID", val.MapsTo);
            Assert.AreEqual("MapLocationID", val.Name);
            Assert.AreEqual("[[mapRecSet(*).LocationID]]", val.RawValue);
            Assert.AreEqual("dbo_proc_GetAllMapLocations", val.RecordSetName);
        }
    }
}
