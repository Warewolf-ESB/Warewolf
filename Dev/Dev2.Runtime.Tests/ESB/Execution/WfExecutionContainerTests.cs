/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities;
using System.Collections.Generic;
using Dev2.Data.Decision;
using System.Activities.Statements;
using Microsoft.VisualBasic.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common;
using Warewolf.Storage;
using Dev2.Runtime;
using Dev2.Data;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class WfExecutionContainerTests
    {
        //TODO: Perhaps we replace this builder as it ties our tests to Microsoft, try an approach that might make use of IDev2Activity. Suggest not using VisualBasic
        protected FlowNode TestStartNode { get; set; }
        protected DynamicActivity FlowchartProcess
        {
            get
            {
                var activity = new DynamicActivity { Implementation = () => FlowchartActivityBuilder.Implementation };
                foreach (DynamicActivityProperty prop in FlowchartActivityBuilder.Properties)
                {
                    activity.Properties.Add(prop);
                }

                return activity;
            }
        }
        protected ActivityBuilder FlowchartActivityBuilder
        {
            get
            {
                var builder = new ActivityBuilder
                {
                    Properties = {
                    new DynamicActivityProperty{Name = "AmbientDataList",Type = typeof(InOutArgument<List<string>>)}
                    ,new DynamicActivityProperty{ Name = "ParentWorkflowInstanceId", Type = typeof(InOutArgument<Guid>)}
                    ,new DynamicActivityProperty{ Name = "ParentServiceName", Type = typeof(InOutArgument<string>)}
                },
                    Implementation = new Flowchart
                    {

                        Variables = {
                         new Variable<List<string>>{Name = "InstructionList"},
                         new Variable<string>{Name = "LastResult"},
                         new Variable<bool>{Name = "HasError"},
                         new Variable<string>{Name = "ExplicitDataList"},
                         new Variable<bool>{Name = "IsValid"},
                         new Variable<Unlimited.Applications.BusinessDesignStudio.Activities.Util>{ Name = "t"},
                         new Variable<Dev2DataListDecisionHandler>{Name = "Dev2DecisionHandler"}
                        }
                        ,
                        StartNode = TestStartNode
                    }
                };

                var vbs = new VisualBasicSettings
                {
                    ImportReferences =     {
                        new VisualBasicImportReference {
                            Assembly = "Unlimited.Framework",
                            Import = "Unlimited.Framework"
                        },
                        new VisualBasicImportReference{
                             Assembly = "Unlimited.Applications.BusinessDesignStudio.Activities",
                             Import = "Unlimited.Applications.BusinessDesignStudio.Activities"

                        }
                    }
                };

                VisualBasic.SetSettings(builder, vbs);

                return builder;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            Config.Server.EnableDetailedLogging = false;
            
            TestStartNode = new FlowStep
            {
                Action = new DsfNumberFormatActivity(),
                Next = new FlowStep
                {
                    Action = new DsfNumberFormatActivity()
                }
            };
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(WfExecutionContainer))]
        public void WfExecutionContainer_OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var obj = new Mock<IDSFDataObject>();
                var workSpace = new Mock<IWorkspace>();
                var channel = new Mock<IEsbChannel>();
                var serviceAction = new ServiceAction();

                var wfExecutionContainer = new WfExecutionContainer(serviceAction, obj.Object, workSpace.Object, channel.Object);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(WfExecutionContainer))]
        public void WfExecutionContainer_ExecuteNode_CheckWhenDataObjectStopExecutionIsTrue_ShouldNotEmptyExecutionExceptionInDataObject()
        {
            //--------------Arrange------------------------------
            var dataObjectMock = new Mock<IDSFDataObject>();
            var workSpaceMock = new Mock<IWorkspace>();
            var esbChannelMock = new Mock<IEsbChannel>();
            var executionEnvironment = new Mock<ExecutionEnvironment>();
            var serviceAction = new ServiceAction();

            dataObjectMock.SetupAllProperties();
            dataObjectMock.SetupGet(o => o.Environment).Returns(executionEnvironment.Object);
            dataObjectMock.Setup(o => o.StopExecution).Returns(true);

            var wfExecutionContainer = new WfExecutionContainer(serviceAction, dataObjectMock.Object, workSpaceMock.Object, esbChannelMock.Object);

            //--------------Act----------------------------------
            wfExecutionContainer.Eval(FlowchartProcess, dataObjectMock.Object, 0);

            //--------------Assert-------------------------------
            Assert.IsNotNull(dataObjectMock.Object.ExecutionException);
            Assert.AreEqual(dataObjectMock.Object.ExecutionException.Message, "An error occurred while formatting a number, an invalid value of '' was returned from the rounding function.");
        }


        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(WfExecutionContainer))]
        public void WfExecutionContainer_ExecuteNode_CheckWhenDataObjectStopExecutionIsFalse_ShouldEmptyExecutionExceptionInDataObject()
        {
            //--------------Arrange------------------------------
            var dataObjectMock = new Mock<IDSFDataObject>();
            var workSpaceMock = new Mock<IWorkspace>();
            var esbChannelMock = new Mock<IEsbChannel>();
            var executionEnvironment = new Mock<ExecutionEnvironment>();
            var serviceAction = new ServiceAction();

            
            dataObjectMock.SetupAllProperties();
            dataObjectMock.SetupGet(o => o.Environment).Returns(executionEnvironment.Object);
            dataObjectMock.Setup(o => o.StopExecution).Returns(false);

            var wfExecutionContainer = new WfExecutionContainer(serviceAction, dataObjectMock.Object, workSpaceMock.Object, esbChannelMock.Object);

            //--------------Act----------------------------------
            wfExecutionContainer.Eval(FlowchartProcess, dataObjectMock.Object, 0);

            //--------------Assert-------------------------------
            Assert.IsNull(dataObjectMock.Object.ExecutionException);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WfExecutionContainer))]
        public void WfExecutionContainer_ExecuteNode_WhenSeverSettings_EnableDetailedLogging_IsTrue_Expect_LogAdditionalDetail()
        {
            //--------------Arrange------------------------------
            var dataObjectMock = new Mock<IDSFDataObject>();
            var workSpaceMock = new Mock<IWorkspace>();
            var esbChannelMock = new Mock<IEsbChannel>();
            var executionEnvironment = new Mock<ExecutionEnvironment>();
            var serviceAction = new ServiceAction();
            var mockStateNotifier = new Mock<IStateNotifier>();
            var mockActivityOne = new Mock<IDev2Activity>();
            var mockExecutionManager = new Mock<IExecutionManager>();
            Config.Server.EnableDetailedLogging = true;

            var wfSettings = new Dev2WorkflowSettingsTO
            {
                EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                LoggerType = LoggerType.JSON,
                KeepLogsForDays = 2,
                CompressOldLogFiles = true
            };
            dataObjectMock.SetupAllProperties();
            dataObjectMock.SetupGet(o => o.Environment).Returns(executionEnvironment.Object);
            dataObjectMock.Setup(o => o.StopExecution).Returns(false);
            dataObjectMock.Setup(o => o.StateNotifier).Returns(mockStateNotifier.Object);
            dataObjectMock.Setup(o => o.Settings).Returns(wfSettings);

            var wfExecutionContainer = new WfExecutionContainer(serviceAction, dataObjectMock.Object, workSpaceMock.Object, esbChannelMock.Object, mockExecutionManager.Object, mockStateNotifier.Object);

            //--------------Act----------------------------------
            wfExecutionContainer.Eval(FlowchartProcess, dataObjectMock.Object, 0);

            //--------------Assert-------------------------------
            Assert.IsNull(dataObjectMock.Object.ExecutionException);
            mockStateNotifier.Verify(o => o.LogPreExecuteState(It.IsAny<IDev2Activity>()), Times.Exactly(2)); //This should be called once? 
            mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<object>(), It.IsAny<string>()), Times.Once); //TODO: though this is now passing, the logAddtionalDetail is still null in db?
            mockStateNotifier.Verify(o => o.LogExecuteCompleteState(It.IsAny<IDev2Activity>()), Times.Once);
            mockExecutionManager.Verify(o => o.CompleteExecution(), Times.Once);
            mockStateNotifier.Verify(o => o.Dispose(), Times.Once);
        }

    }
}