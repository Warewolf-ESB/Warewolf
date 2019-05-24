﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Runtime.WF
{
    [TestClass]
    public class WfApplicationUtilsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetDebugValues_GivenGiven2Values_ShouldReturnTwoDebugValueItems()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"),
                new Dev2Definition("Name2", "Map2", "value", false, "defaultValue", false, "rawValue")
            };
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
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetDebugValues_GivenDuplicateDefs_ShouldReturnDistinctValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue"),
                new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue")
            };
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
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetDebugValues_GivenDefinations_ShouldAddBracketsToValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name1", "Map1", "value", false, "defaultValue", false, "rawValue")
            };
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
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetDebugValues_GivenRecSetDefinations_ShouldAddRecNotationToValues()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var objMock = new Mock<IDSFDataObject>();
            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("rec().Name1", "rec().Map1", "value", false, "defaultValue", false, "rawValue")
            };
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetVariableName_GetDebugValues_Value_IsJsonArray_ShouldAddRecNotationToValues()
        {
            //---------------Set up test pack-------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var wfApplicationUtils = new WfApplicationUtils();

            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("@rec(*).Name1", "rec().Map1", "value", false, "defaultValue", false, "rawValue")
                {
                    IsJsonArray = true
                }
            };

            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var privateObject = new PrivateObject(wfApplicationUtils);
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, mockDSFDataObject.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);
            var count = debugItems.Count(item => item.ResultsList.Count(result => result.Variable == "[[@rec(*).Name1(*)]]") == 1);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_GetVariableName_GetDebugValues_Value_IsJsonArrayAndRecordSetName_NotNull_ShouldAddRecNotationToValues()
        {
            //---------------Set up test pack-------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var wfApplicationUtils = new WfApplicationUtils();

            IList<IDev2Definition> definitions = new List<IDev2Definition>
            {
                new Dev2Definition("@rec(*).Name1", "rec().Map1", "value", false, "defaultValue", false, "rawValue")
                {
                    IsJsonArray = false,
                    RecordSetName = "some recordSet"
                }
            };

            var errorResultTO = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var privateObject = new PrivateObject(wfApplicationUtils);
            var debugItems = privateObject.Invoke("GetDebugValues", definitions, mockDSFDataObject.Object, errorResultTO) as List<DebugItem>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);
            var count = debugItems.Count(item => item.ResultsList.Count(result => result.Variable == "[[some recordSet(*).@rec(*).Name1]]") == 1);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_FindServiceShape_GivenNoResource_ShouldEmptyShape()
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
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_FindServiceShape_GivenResource_ShouldReturnShape()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var privateObject = new PrivateObject(wfApplicationUtils);
            var catLog = new Mock<IResourceCatalog>();
            IResource dropBoxSource = new DropBoxSource
            {
                AppKey = "Key",
                AccessToken = "token",
                DataList = new StringBuilder("SomeValue")
            };
            catLog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dropBoxSource);
            privateObject.SetField("_lazyCat", catLog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var shape = privateObject.Invoke("FindServiceShape", Guid.NewGuid(), Guid.NewGuid()).ToString();
            //---------------Test Result -----------------------
            Assert.AreNotEqual("<DataList></DataList>", shape);
            Assert.AreEqual("SomeValue", shape);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_DispatchDebugState_GivenValidParams_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();

            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, out ErrorResultTO error, false, false);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_DispatchDebugState_GetDebugState_HasRemote_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            mockDSFDataObject.Setup(o => o.RemoteInvokerID).Returns("fa4b4786-67d9-414f-8c2e-17673fdcef48");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockDSFDataObject.Object, StateType.Start, out ErrorResultTO error, false, false);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_DispatchDebugState_GivenValidParamsAndIsDebugMode_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var debugState = new DebugState { StateType = StateType.Start };
            debugDispatcher.Setup(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            var mock = new Mock<Func<IDebugDispatcher>>();
            mock.Setup(func => func()).Returns(() => debugDispatcher.Object);
            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            mockObj.Setup(o => o.IsDebugMode()).Returns(true);
            var privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, out var error, false, false);
                var state = debugState;
                debugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = state, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));

                debugState = new DebugState { StateType = StateType.End };
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.End, out error, false, false);

                debugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_TryWriteDebug_WriteDebug_Environment_NotHasErrors_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDebugDispatcher = new Mock<IDebugDispatcher>();
            var mockDebugDispatcherFunc = new Mock<Func<IDebugDispatcher>>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var debugState = new DebugState { StateType = StateType.Start };

            mockDebugDispatcher.Setup(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));

            mockDebugDispatcherFunc.Setup(func => func()).Returns(() => mockDebugDispatcher.Object);

            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDSFDataObject.Setup(o => o.ResourceID).Returns(new Guid("fa4b4786-67d9-414f-8c2e-17673fdcef48"));

            var privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mockDebugDispatcherFunc.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockDSFDataObject.Object, StateType.Start, out var error, false, false);
                var state = debugState;
                mockDebugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = state, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));

                debugState = new DebugState { StateType = StateType.End };
                wfApplicationUtils.DispatchDebugState(mockDSFDataObject.Object, StateType.End, out error, false, false);

                mockDebugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_TryWriteDebug_WriteDebug_Environment_HasErrors_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockDebugDispatcher = new Mock<IDebugDispatcher>();
            var mockDebugDispatcherFunc = new Mock<Func<IDebugDispatcher>>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var debugState = new DebugState { StateType = StateType.Start };

            mockDebugDispatcher.Setup(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));

            mockDebugDispatcherFunc.Setup(func => func()).Returns(() => mockDebugDispatcher.Object);

            mockDSFDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDSFDataObject.Setup(o => o.ResourceID).Returns(new Guid("fa4b4786-67d9-414f-8c2e-17673fdcef48"));
            mockDSFDataObject.Setup(o => o.Environment.HasErrors()).Returns(true);

            var privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mockDebugDispatcherFunc.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockDSFDataObject.Object, StateType.Start, out var error, false, false);
                var state = debugState;
                mockDebugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = state, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));

                debugState = new DebugState { StateType = StateType.End };
                wfApplicationUtils.DispatchDebugState(mockDSFDataObject.Object, StateType.End, out error, false, false);

                mockDebugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_DispatchDebugState_GivenValidParamsAndIntergoateInputs_ShouldWriteUsingDebugDispactcher()
        {
            //---------------Set up test pack-------------------
            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var debugState = new DebugState { StateType = StateType.Start };
            debugDispatcher.Setup(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            var mock = new Mock<Func<IDebugDispatcher>>();
            mock.Setup(func => func()).Returns(() => debugDispatcher.Object);
            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            mockObj.Setup(o => o.IsDebugMode()).Returns(true);
            var privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, out var error, true);
                var state = debugState;
                debugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = state, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(WfApplicationUtils))]
        public void WfApplicationUtils_DispatchDebugState_GivenValidParamsAndIntergoateOutputs_ShouldWriteUsingDebugDispactcher_GetResourceForDatalist()
        {
            //---------------Set up test pack-------------------
            var catLog = new Mock<IResourceCatalog>();
            catLog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new Resource
            {
                DataList = new StringBuilder()
            });

            IResource dropBoxSource = new DropBoxSource
            {
                AppKey = "Key",
                AccessToken = "token",
                DataList = new StringBuilder("<DataList></DataList>")
            };
            catLog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dropBoxSource);

            var wfApplicationUtils = new WfApplicationUtils();
            var envMock = new Mock<IExecutionEnvironment>();
            var debugDispatcher = new Mock<IDebugDispatcher>();
            var debugState = new DebugState { StateType = StateType.Start };
            debugDispatcher.Setup(dispatcher => dispatcher.Write(new WriteArgs { debugState = debugState, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
            var mock = new Mock<Func<IDebugDispatcher>>();
            mock.Setup(func => func()).Returns(() => debugDispatcher.Object);
            var mockObj = new Mock<IDSFDataObject>();
            mockObj.Setup(o => o.Environment).Returns(envMock.Object);
            mockObj.Setup(o => o.IsDebugMode()).Returns(true);
            var privateObject = new PrivateObject(wfApplicationUtils);
            privateObject.SetField("_getDebugDispatcher", mock.Object);
            privateObject.SetField("_lazyCat", catLog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                wfApplicationUtils.DispatchDebugState(mockObj.Object, StateType.Start, out var error);
                var state = debugState;
                debugDispatcher.Verify(dispatcher => dispatcher.Write(new WriteArgs { debugState = state, isTestExecution = It.IsAny<bool>(), isDebugFromWeb = It.IsAny<bool>(), testName = It.IsAny<string>(), isRemoteInvoke = It.IsAny<bool>(), remoteInvokerId = It.IsAny<string>(), parentInstanceId = It.IsAny<string>(), remoteDebugItems = It.IsAny<IList<IDebugState>>() }));
                catLog.Verify(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>()));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
