using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.WF
{
    [TestClass]
    public class WfApplicationUtilsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_ShouldConstruct()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var newWfApplicationUtils = new WfApplicationUtils();
            //---------------Test Result -----------------------
            Assert.IsNotNull(newWfApplicationUtils, "Cannot create new WfApplicationUtils object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugValues_GivenGiven2Values_ShouldReturnTwoDebugValueItems()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>();
            definitions.Add(new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"));
            definitions.Add(new Dev2Definition("Name2", "Map2", "value", false, "defaultValue", false, "rawValue"));
            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, objMock.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(2, debugItems.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugValues_GivenDuplicateDefs_ShouldReturnDistinctValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>();
            definitions.Add(new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"));
            definitions.Add(new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"));
            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, objMock.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugValues_GivenDefinations_ShouldAddBracketsToValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>();
            definitions.Add(new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"));
            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, objMock.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);
            var count = debugItems.Count(item => item.ResultsList.Count(result => result.Variable == "[[Name1]]") == 1);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugValues_GivenRecSetDefinations_ShouldAddRecNotationToValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>();
            definitions.Add(new Dev2Definition("rec().Name1", "rec().Map1", "value", false, "defaultValue", false, "rawValue"));
            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, objMock.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);
            var count = debugItems.Count(item => item.ResultsList.Count(result => result.Variable == "[[rec(*).Name1]]") == 1);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindServiceShape_GivenNoResource_ShouldEmptyShape()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var catLog = new Mock<IResourceCatalog>();
            catLog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(default(IResource));
            privateObject.SetField("_lazyCat", catLog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var shape = privateObject.Invoke("FindServiceShape", Guid.NewGuid(), Guid.NewGuid()).ToString();
            //---------------Test Result -----------------------
            Assert.AreEqual("<DataList></DataList>", shape);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindServiceShape_GivenResource_ShouldReturnShape()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var catLog = new Mock<IResourceCatalog>();
            IResource dropBoxSource = new DropBoxSource() { AppKey = "Key", AccessToken = "token" };
            dropBoxSource.DataList = new StringBuilder("SomeValue");
            catLog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dropBoxSource);
            privateObject.SetField("_lazyCat", catLog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var shape = privateObject.Invoke("FindServiceShape", Guid.NewGuid(), Guid.NewGuid()).ToString();
            //---------------Test Result -----------------------
            Assert.AreNotEqual("<DataList></DataList>", shape);
            Assert.AreEqual("SomeValue", shape);
        }

        //DispatchDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, out ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false, bool durationVisible=true)
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DispatchDebugState_GivenValidParams_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();

            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            ErrorResultTO error;
            try
            {
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, false, string.Empty, out error);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
            //---------------Test Result -----------------------
        }

        //DispatchDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, out ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false, bool durationVisible=true)
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DispatchDebugState_GivenValidParamsAndIsDebugMode_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var debugState = new DebugState { StateType = StateType.Start };
            debugDispatcher.Setup(dispatcher => dispatcher.Write(debugState, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()));
            var mock = new Mock<Func<IDebugDispatcher>>();
            mock.Setup(func => func()).Returns(() => debugDispatcher.Object);
            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            mockObj.Setup(o => o.IsDebugMode()).Returns(true);
            PrivateObject privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            ErrorResultTO error;
            try
            {
               
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, false, string.Empty, out error);
                var state = debugState;
                debugDispatcher.Verify(dispatcher => dispatcher.Write(state, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()));

                debugState = new DebugState { StateType = StateType.End };
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.End, false, string.Empty, out error);

                debugDispatcher.Verify(dispatcher => dispatcher.Write(debugState, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
            //---------------Test Result -----------------------
        } 
        //DispatchDebugState(IDSFDataObject dataObject, StateType stateType, bool hasErrors, string existingErrors, out ErrorResultTO errors, DateTime? workflowStartTime = null, bool interrogateInputs = false, bool interrogateOutputs = false, bool durationVisible=true)
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DispatchDebugState_GivenValidParamsAndIntergoateInputs_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var debugState = new DebugState { StateType = StateType.Start };
            debugDispatcher.Setup(dispatcher => dispatcher.Write(debugState, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()));
            var mock = new Mock<Func<IDebugDispatcher>>();
            mock.Setup(func => func()).Returns(() => debugDispatcher.Object);
            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            mockObj.Setup(o => o.IsDebugMode()).Returns(true);
            PrivateObject privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            ErrorResultTO error;
            try
            {
               
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, false, string.Empty, out error, DateTime.Now, true);
                var state = debugState;
                debugDispatcher.Verify(dispatcher => dispatcher.Write(state, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()));
                
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
            //---------------Test Result -----------------------
        }
    }
}
