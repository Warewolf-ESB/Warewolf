/*
 * Coverage tests for Dev2.Activities.WF.WorkflowToX6Converter.
 *
 * Why this file exists
 * --------------------
 * WorkflowToX6Converter is a ~3,400-line partial class split across ~60
 * "_XxxActivityHelper" files, each of which builds an X6 graph Cell for one
 * Warewolf activity type.  Almost all of those helper files were at 0% line
 * coverage — the converter was only exercised for the FlowSwitch path.
 *
 * The class has a single clean entry point, ConvertToX6Json(ActivityBuilder, xml),
 * and no external dependencies, so it is cheaply unit-testable by handing it a
 * workflow whose Implementation is the activity under test and asserting on the
 * produced X6WorkflowLoadModel.  Each CreateXxxActivity helper is reached through
 * the big type-dispatch in CreateActivityNode, so converting one activity of each
 * type drives that type's helper plus its dispatch arm.
 *
 * The tests assert on the most stable contract — that conversion succeeds and
 * produces the expected node/edge structure — rather than the exact serialized
 * data, which belongs to each activity's own ToX6Json (covered elsewhere).
 *
 * Tests are deliberately one-activity-per-method so that, if a single activity's
 * ToX6Json regresses and throws, only that test fails instead of silently voiding
 * coverage for an entire batch.
 */

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.DateAndTime;
using Dev2.Activities.Exchange;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Activities.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Activities.Scripting;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class WorkflowToX6ConverterCoverageTests
    {
        const string Cat = "WorkflowToX6Converter_Coverage";

        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────

        static X6WorkflowLoadModel Convert(Activity implementation)
        {
            var converter = new WorkflowToX6Converter();
            var json = converter.ConvertToX6Json(new ActivityBuilder { Implementation = implementation }, "<xml/>");
            Assert.IsNotNull(json);
            var model = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(json);
            Assert.IsNotNull(model);
            return model;
        }

        // A leaf activity placed directly as the workflow Implementation produces a
        // start node plus exactly one activity node, joined by one edge.
        static void AssertLeaf(Activity activity)
        {
            var model = Convert(activity);
            Assert.IsTrue(model.Nodes.Count >= 2,
                $"Expected at least a start node and the activity node for {activity.GetType().Name}, got {model.Nodes.Count}.");
            Assert.IsTrue(model.Edges.Count >= 1,
                $"Expected an edge from start to {activity.GetType().Name}, got {model.Edges.Count}.");
        }

        // ─────────────────────────────────────────────────────────────────
        // Recordset / data activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DataSplit() => AssertLeaf(new DsfDataSplitActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DataMerge() => AssertLeaf(new DsfDataMergeActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_BaseConvert() => AssertLeaf(new DsfBaseConvertActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Replace() => AssertLeaf(new DsfReplaceActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_CaseConvert() => AssertLeaf(new DsfCaseConvertActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FindIndex() => AssertLeaf(new DsfIndexActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FindRecords() => AssertLeaf(new DsfFindRecordsMultipleCriteriaActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DeleteRecordNullHandler() => AssertLeaf(new DsfDeleteRecordNullHandlerActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DeleteRecord() => AssertLeaf(new DsfDeleteRecordActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SortRecords() => AssertLeaf(new DsfSortRecordsActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_CountRecordset() => AssertLeaf(new DsfCountRecordsetNullHandlerActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_RecordsetLength() => AssertLeaf(new DsfRecordsetNullhandlerLengthActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Unique() => AssertLeaf(new DsfUniqueActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_AdvancedRecordset() => AssertLeaf(new AdvancedRecordsetActivity());

        // ─────────────────────────────────────────────────────────────────
        // Web activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WebGet() => AssertLeaf(new WebGetActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WebGetRequestWithTimeout() => AssertLeaf(new DsfWebGetRequestWithTimeoutActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WebPost() => AssertLeaf(new WebPostActivityNew());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WebPut() => AssertLeaf(new WebPutActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WebDelete() => AssertLeaf(new DsfWebDeleteActivity());

        // ─────────────────────────────────────────────────────────────────
        // Database activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SqlServer() => AssertLeaf(new DsfSqlServerDatabaseActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PostgreSql() => AssertLeaf(new DsfPostgreSqlActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_MySql() => AssertLeaf(new DsfMySqlDatabaseActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SqlBulkInsert() => AssertLeaf(new DsfSqlBulkInsertActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Oracle() => AssertLeaf(new DsfOracleDatabaseActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Odbc() => AssertLeaf(new DsfODBCDatabaseActivity());

        // ─────────────────────────────────────────────────────────────────
        // File / path activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FileRead() => AssertLeaf(new DsfFileRead());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FileReadWithBase64() => AssertLeaf(new FileReadWithBase64());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FileWrite_Dsf() => AssertLeaf(new DsfFileWrite());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FileWrite_PathOps() => AssertLeaf(new FileWriteActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FolderRead_Activity() => AssertLeaf(new DsfFolderReadActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_FolderRead() => AssertLeaf(new DsfFolderRead());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PathCreate() => AssertLeaf(new DsfPathCreate());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PathCopy() => AssertLeaf(new DsfPathCopy());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PathMove() => AssertLeaf(new DsfPathMove());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PathRename() => AssertLeaf(new DsfPathRename());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PathDelete() => AssertLeaf(new DsfPathDelete());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Zip() => AssertLeaf(new DsfZip());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_UnZip() => AssertLeaf(new DsfUnZip());

        // ─────────────────────────────────────────────────────────────────
        // Scripting / calculation / misc leaf activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Javascript() => AssertLeaf(new DsfJavascriptActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Ruby() => AssertLeaf(new DsfRubyActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Python() => AssertLeaf(new DsfPythonActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_CommandLine() => AssertLeaf(new DsfExecuteCommandLineActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Comment() => AssertLeaf(new DsfCommentActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_CreateJson() => AssertLeaf(new DsfCreateJsonActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_XPath() => AssertLeaf(new DsfXPathActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Random() => AssertLeaf(new DsfRandomActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_NumberFormat() => AssertLeaf(new DsfNumberFormatActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Calculate_DotNet() => AssertLeaf(new DsfDotNetCalculateActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_AggregateCalculate() => AssertLeaf(new DsfAggregateCalculateActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_AggregateCalculate_DotNet() => AssertLeaf(new DsfDotNetAggregateCalculateActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DateTime() => AssertLeaf(new DsfDateTimeActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DateTime_DotNet() => AssertLeaf(new DsfDotNetDateTimeActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DateTimeDifference() => AssertLeaf(new DsfDateTimeDifferenceActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DateTimeDifference_DotNet() => AssertLeaf(new DsfDotNetDateTimeDifferenceActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_GatherSystemInformation() => AssertLeaf(new DsfGatherSystemInformationActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_GatherSystemInformation_DotNet() => AssertLeaf(new DsfDotNetGatherSystemInformationActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SendEmail() => AssertLeaf(new DsfSendEmailActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_ExchangeEmail() => AssertLeaf(new DsfExchangeEmailNewActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_WorkflowActivity() => AssertLeaf(new DsfWorkflowActivity());

        // ─────────────────────────────────────────────────────────────────
        // RabbitMQ / Redis leaf activities
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PublishRabbitMq_Dsf() => AssertLeaf(new DsfPublishRabbitMQActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PublishRabbitMq() => AssertLeaf(new PublishRabbitMQActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_ConsumeRabbitMq() => AssertLeaf(new DsfConsumeRabbitMQActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_RedisRemove() => AssertLeaf(new RedisRemoveActivity());

        // ─────────────────────────────────────────────────────────────────
        // Container activities — converted empty (drives Create + Process +
        // the null-handler guard in Process*NestedActivities).
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Gate_Empty() => AssertLeaf(new GateActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SuspendExecution_Empty() => AssertLeaf(new SuspendExecutionActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_ManualResumption_Empty() => AssertLeaf(new ManualResumptionActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_RedisCache_Empty() => AssertLeaf(new RedisCacheActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_SelectAndApply_Empty() => AssertLeaf(new DsfSelectAndApplyActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_ForEach_Empty() => AssertLeaf(new DsfForEachActivity());

        // ─────────────────────────────────────────────────────────────────
        // DsfSequenceActivity — its nested children live in an Activities
        // collection (not an ActivityFunc), so we can fully drive
        // ProcessSequenceNestedActivities by adding children.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DsfSequence_Empty()
        {
            var model = Convert(new DsfSequenceActivity());
            // start node + sequence node
            Assert.IsTrue(model.Nodes.Count >= 2);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DsfSequence_WithNestedChildren_MarksNesting()
        {
            var sequence = new DsfSequenceActivity();
            sequence.Activities.Add(new DsfMultiAssignActivity());
            sequence.Activities.Add(new DsfDataSplitActivity());

            var model = Convert(sequence);

            // start + sequence + 2 children
            Assert.IsTrue(model.Nodes.Count >= 4,
                $"Expected start, sequence and two nested nodes, got {model.Nodes.Count}.");
            // At least one node should carry the nesting metadata set by the helper.
            var nested = model.Nodes.FindAll(n => n.data != null && n.data.ContainsKey("isNested"));
            Assert.IsTrue(nested.Count >= 1, "Expected nested children to be tagged as nested.");
        }

        // ─────────────────────────────────────────────────────────────────
        // Multi-assign leaf activities (have bespoke ToX6Json dispatch arms).
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_MultiAssign() => AssertLeaf(new DsfMultiAssignActivity());

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_MultiAssignObject() => AssertLeaf(new DsfDotNetMultiAssignObjectActivity());

        // ─────────────────────────────────────────────────────────────────
        // Control-flow constructs (System.Activities.Statements types) — these
        // drive the Process* methods rather than CreateActivityNode helpers.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_If_BothBranches()
        {
            var ifActivity = new If
            {
                Then = new DsfMultiAssignActivity(),
                Else = new DsfDataSplitActivity()
            };
            var model = Convert(ifActivity);
            Assert.IsTrue(model.Nodes.Count >= 3,
                $"Expected start + then + else nodes, got {model.Nodes.Count}.");
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_If_OnlyThen()
        {
            var model = Convert(new If { Then = new DsfMultiAssignActivity() });
            Assert.IsTrue(model.Nodes.Count >= 2);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_If_NoBranches()
        {
            var model = Convert(new If());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_While_WithBody()
        {
            var model = Convert(new While { Body = new DsfMultiAssignActivity() });
            Assert.IsTrue(model.Nodes.Count >= 2);
            // While creates a "Loop" back-edge.
            Assert.IsTrue(model.Edges.Exists(e => e.label == "Loop"));
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_While_NoBody()
        {
            var model = Convert(new While());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DoWhile_WithBody()
        {
            var model = Convert(new DoWhile { Body = new DsfMultiAssignActivity() });
            Assert.IsTrue(model.Edges.Exists(e => e.label == "Loop"));
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_DoWhile_NoBody()
        {
            var model = Convert(new DoWhile());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_TryCatch_TryAndFinally()
        {
            var model = Convert(new TryCatch
            {
                Try = new DsfMultiAssignActivity(),
                Finally = new DsfDataSplitActivity()
            });
            Assert.IsTrue(model.Nodes.Count >= 3);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_TryCatch_Empty()
        {
            var model = Convert(new TryCatch());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Parallel_WithBranches()
        {
            var parallel = new System.Activities.Statements.Parallel();
            parallel.Branches.Add(new DsfMultiAssignActivity());
            parallel.Branches.Add(new DsfDataSplitActivity());
            var model = Convert(parallel);
            Assert.IsTrue(model.Nodes.Count >= 3);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Parallel_Empty()
        {
            var model = Convert(new System.Activities.Statements.Parallel());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Sequence_System()
        {
            var sequence = new Sequence();
            sequence.Activities.Add(new DsfMultiAssignActivity());
            sequence.Activities.Add(new DsfDataSplitActivity());
            var model = Convert(sequence);
            Assert.IsTrue(model.Nodes.Count >= 3);
        }

        // ─────────────────────────────────────────────────────────────────
        // Flowchart traversal — FlowStep chains, FlowDecision branches.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Flowchart_StepChain()
        {
            var step2 = new FlowStep { Action = new DsfDataSplitActivity() };
            var step1 = new FlowStep { Action = new DsfMultiAssignActivity(), Next = step2 };
            var model = Convert(new Flowchart { StartNode = step1 });
            Assert.IsTrue(model.Nodes.Count >= 3,
                $"Expected start + two step nodes, got {model.Nodes.Count}.");
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Flowchart_Empty()
        {
            var model = Convert(new Flowchart());
            Assert.IsTrue(model.Nodes.Count >= 1);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_Flowchart_Decision_TrueAndFalse()
        {
            var decision = new FlowDecision
            {
                Condition = new DsfFlowDecisionActivityStub(),
                True = new FlowStep { Action = new DsfMultiAssignActivity() },
                False = new FlowStep { Action = new DsfDataSplitActivity() }
            };
            var model = Convert(new Flowchart { StartNode = decision });
            Assert.IsTrue(model.Nodes.Count >= 3);
            Assert.IsTrue(model.Edges.Exists(e => e.label == "True" || e.label == "true"));
        }

        // A minimal Activity<bool> usable as a FlowDecision.Condition.
        public sealed class DsfFlowDecisionActivityStub : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context) => false;
        }

        // ─────────────────────────────────────────────────────────────────
        // Entry-point edge cases
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_NullImplementation_ProducesStartNodeOnly()
        {
            var converter = new WorkflowToX6Converter();
            var json = converter.ConvertToX6Json(new ActivityBuilder { Implementation = null }, "<xml/>");
            var model = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(json);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Nodes.Count, "Only the start node should be produced for a null implementation.");
            Assert.AreEqual(0, model.Edges.Count);
        }

        [TestMethod, Timeout(60000), Owner("Ashley Lewis"), TestCategory(Cat)]
        public void Convert_PreservesWorkflowXml()
        {
            var converter = new WorkflowToX6Converter();
            var json = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new DsfMultiAssignActivity() }, "<some-xaml/>");
            var model = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(json);
            Assert.AreEqual("<some-xaml/>", model.WorkflowXml);
        }
    }
}
