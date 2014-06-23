using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.WF;
using Dev2.UnitTestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            mockDispatcher.Setup(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()))
                .Verifiable();
            var dataObject = mockDataObject.Object;
            const StateType StateType = StateType.All;
            // ReSharper disable RedundantAssignment
            ErrorResultTO errors = new ErrorResultTO();
            // ReSharper restore RedundantAssignment
            DateTime? workflowStartTime = DateTime.Now;
            //------------Execute Test---------------------------
            wfUtils.DispatchDebugState(dataObject, StateType, false, string.Empty, out errors, workflowStartTime);
            //------------Assert Results-------------------------
            mockDispatcher.Verify(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()), Times.Once());
            Assert.IsFalse(errors.HasErrors());
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
            mockDispatcher.Setup(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()))
                .Verifiable();
            var dataObject = mockDataObject.Object;
            const StateType StateType = StateType.All;
            // ReSharper disable RedundantAssignment
            ErrorResultTO errors = new ErrorResultTO();
            // ReSharper restore RedundantAssignment
            DateTime? workflowStartTime = DateTime.Now;
            //------------Execute Test---------------------------
            wfUtils.DispatchDebugState(dataObject, StateType, false, string.Empty, out errors, workflowStartTime);
            //------------Assert Results-------------------------
            mockDispatcher.Verify(m => m.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()), Times.Never());
            Assert.IsFalse(errors.HasErrors());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_CreatesOneWorkflowItemPerOutputScalar()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[bob]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(()=>compiler.Object,((a,b)=>{}));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(1)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "" }, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[]{"bob"} ,a=>a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();
           
            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(1,results.Count);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_CreatesOneWorkflowItemPerSingleRecordSet()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo()]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(() => compiler.Object, ((a, b) => { }));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(1)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo" }, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "bob" }, a => a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();

            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(1, results.Count);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_CreatesOneWorkflowItemIfDuplicatesOccur()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo()]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(() => compiler.Object, ((a, b) => { }));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(2)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "moo" }, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "bob", "bob" }, a => a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();

            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(1, results.Count);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_CreatesOneWorkflowItemIfDuplicatesOccur_MultipleResultSets_ExpectNoDuplicates()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo()]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(() => compiler.Object, ((a, b) => { }));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(2)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "moo" }, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "moo" }, a => a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();

            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(1, results.Count);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_CreatesOneWorkflowItemIfDuplicatesOccur_MultipleResultSetsAndScalars_ExpectNoDuplicates()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo()]]", false, out err)).Returns(entr.Object);
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[bob()]]", false, out err)).Returns(entr.Object);
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(() => compiler.Object, ((a, b) => { }));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(3)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "" ,"bob"}, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "moo","murali" }, a => a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();

            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(3, results.Count);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WfApplicationUtils_GetDebugValues")]
        public void WfApplicationUtils_GetDebugValues_MultipleResultSetsAndScalars_ExpectNoDuplicates()
        {
            //------------Setup for test--------------------------
            var compiler = new Mock<IDataListCompiler>();
            var entr = new Mock<IBinaryDataListEntry>();
            ErrorResultTO err;
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo()]]", false, out err)).Returns(entr.Object);
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[bob()]]", false, out err)).Returns(entr.Object);
            compiler.Setup(a => a.Evaluate(It.IsAny<Guid>(), enActionType.User, "[[moo]]", false, out err)).Returns(entr.Object);
            var wfUtils = new WfApplicationUtils(() => compiler.Object, ((a, b) => { }));
            var items = MoqUtils.GenerateMockEnumerable<IDev2Definition>(3)
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "", "bob" ,"moo"}, (a => a.RecordSetName))
                       .SetupExpectationsOnEnumerableWithReturnValues(new[] { "moo", "moo", "murali","moo" }, a => a.Name)
                       .ProxiesFromMockEnumerable().ToList();

            var dl = new Mock<IBinaryDataList>();

            var results = wfUtils.GetDebugValues(items, dl.Object, out err);
            Assert.AreEqual(3, results.Count);

        }
    }
}
