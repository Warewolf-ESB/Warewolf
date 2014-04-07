using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB.WF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Tests.Runtime.WF
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class WfApplicationUtilsTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WfApplicationUtils_DispatchDebugState")]
        public void WfApplicationUtils_DispatchDebugState_RunWorkflowAsyncIsTrue_WillDispatchDebugState()
        {
            //------------Setup for test--------------------------
            var wfUtils = new WfApplicationUtils();
            var mockDispatcher = new Mock<IDebugDispatcher>();
            wfUtils.GetDebugDispatcher = () => mockDispatcher.Object;
            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(d => d.IsDebugMode()).Returns(false);
            mockDataObject.Setup(d => d.IsFromWebServer).Returns(false);
            mockDataObject.Setup(d => d.RunWorkflowAsync).Returns(true);
            mockDispatcher.Setup(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<DebugState>>()))
                .Verifiable();
            IDSFDataObject dataObject = mockDataObject.Object;
            const StateType StateType = StateType.All;
            ErrorResultTO errors = new ErrorResultTO();
            DateTime? workflowStartTime = DateTime.Now;
            //------------Execute Test---------------------------
            wfUtils.DispatchDebugState(dataObject, StateType, errors, workflowStartTime);
            //------------Assert Results-------------------------
            mockDispatcher.Verify(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<DebugState>>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WfApplicationUtils_DispatchDebugState")]
        public void WfApplicationUtils_DispatchDebugState_RunWorkflowAsyncIsFalse_WillNotDispatchDebugState()
        {
            //------------Setup for test--------------------------
            var wfUtils = new WfApplicationUtils();
            var mockDispatcher = new Mock<IDebugDispatcher>();
            wfUtils.GetDebugDispatcher = () => mockDispatcher.Object;
            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(d => d.IsDebugMode()).Returns(false);
            mockDataObject.Setup(d => d.IsFromWebServer).Returns(false);
            mockDataObject.Setup(d => d.RunWorkflowAsync).Returns(false);
            mockDispatcher.Setup(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<DebugState>>()))
                .Verifiable();
            IDSFDataObject dataObject = mockDataObject.Object;
            const StateType StateType = StateType.All;
            ErrorResultTO errors = new ErrorResultTO();
            DateTime? workflowStartTime = DateTime.Now;
            //------------Execute Test---------------------------
            wfUtils.DispatchDebugState(dataObject, StateType, errors, workflowStartTime);
            //------------Assert Results-------------------------
            mockDispatcher.Verify(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<DebugState>>()), Times.Never());
        }
    }
}
