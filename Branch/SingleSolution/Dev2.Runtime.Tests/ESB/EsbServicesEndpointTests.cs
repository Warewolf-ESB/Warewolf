using System;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    public class EsbServicesEndpointTests
    {
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
            dataObject.Setup(d => d.IsRemoteWorkflow).Returns(isRemoteWorkflow);
            dataObject.Setup(d => d.ServiceName).Returns("xxxx");

            var isLocalInvoke = !expectedIsLocal; 

            var invoker = new Mock<IDynamicServicesInvoker>();
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
    }
}
