using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.Tests.Activities.ActivityTests.SelectAndApply
{
    [TestClass]
    public class SelectAndApplyActivityTests : BaseActivityUnitTest
    {
        DsfSelectAndApplyActivity CreateActivity()
        {
            return new DsfSelectAndApplyActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_Construct")]
        public void SelectAndApplyActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(selectAndApplyActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyActivity_GetFindMissingType")]
        public void SelectAndApplyActivity_GetFindMissingType_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var enFindMissingType = selectAndApplyActivity.GetFindMissingType();
            Assert.AreEqual(enFindMissingType, enFindMissingType.ForEach);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyActivity_GetFindMissingType")]
        public void SelectAndApplyActivity_GetOutputs_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var outputs = selectAndApplyActivity.GetOutputs();
            Assert.AreEqual(0, outputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_DisplayName")]
        public void SelectAndApplyActivity_DisplayName_GivenIsCreated_ShouldBeSelectAndApply()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(selectAndApplyActivity);
            Assert.AreEqual("Select and apply", selectAndApplyActivity.DisplayName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenCaseConvertActivityApplied_ToUpperApplied()
        {
            //------------Setup for test--------------------------
            var activity = new DsfCaseConvertActivity
            {
                ConvertCollection = new List<ICaseConvertTO>
                {
                    new CaseConvertTO("[[a]]", "UPPER", "[[a]]", 1)
                    {
                        ExpressionToConvert = "[[a]]"
                    }
                }
            };
            SetupArguments("", "", activity);
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var names = DataObject.Environment.EvalAsListOfStrings("[[Person(*).Name]]", 0);
            Assert.IsTrue(names.Any(s => s == "BOB"));
            Assert.IsTrue(names.Any(s => s == "DORA"));
            Assert.IsTrue(names.Any(s => s == "SUPERMAN"));
            Assert.IsTrue(names.Any(s => s == "BATMAN"));
            Assert.IsTrue(names.Any(s => s == "ORLANDO"));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenCaseConvertActivityApplied_ToCorrectIndex()
        {
            //------------Setup for test--------------------------
            var activity = new DsfCaseConvertActivity
            {
                ConvertCollection = new List<ICaseConvertTO>
                {
                    new CaseConvertTO("[[a]]", "UPPER", "[[a]]", 1)
                    {
                        ExpressionToConvert = "[[a]]"
                    }
                }
            };
            SetupArguments("", "", activity);
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var names = DataObject.Environment.EvalAsListOfStrings("[[Person(*).Name]]", 0);
            Assert.AreEqual("BOB", names[0]);
            Assert.AreEqual("DORA", names[1]);
            Assert.AreEqual("SUPERMAN", names[2]);
            Assert.AreEqual("BATMAN", names[3]);
            Assert.AreEqual("ORLANDO", names[4]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenCaseConvertActivityApplied_ToCorrectIndex_ComplexObjects()
        {
            //------------Setup for test--------------------------
            var activity = new DsfCaseConvertActivity
            {
                ConvertCollection = new List<ICaseConvertTO>
                {
                    new CaseConvertTO("[[a]]", "UPPER", "[[a]]", 1)
                    {
                        ExpressionToConvert = "[[a]]"
                    }
                }
            };

            DataObject.Environment = new ExecutionEnvironment();
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Name]]", "Bob"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Name]]", "Dora"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Name]]", "Superman"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Name]]", "Batman"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Name]]", "Orlando"), 0);
            const string dataSource = "[[@Person(*).Name]]";
            const string alias = "[[a]]";
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,

                //ApplyActivityFunc = activity
            };
            var handler = activity as Activity;
            dsfSelectAndApplyActivity.ApplyActivityFunc.Handler = handler;
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = "";
            TestData = "";
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var names = DataObject.Environment.EvalAsListOfStrings("[[@Person(*).Name]]", 0);
            Assert.AreEqual("BOB", names[0]);
            Assert.AreEqual("DORA", names[1]);
            Assert.AreEqual("SUPERMAN", names[2]);
            Assert.AreEqual("BATMAN", names[3]);
            Assert.AreEqual("ORLANDO", names[4]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenNumberFormatTool_ToCorrectFormat()
        {
            var activity = new DsfNumberFormatActivity
                {
                    Expression = "[[result]]",
                    Result = "[[result]]",
                    RoundingType = enRoundingType.Up.GetDescription(),
                    RoundingDecimalPlaces = "2",
                    DecimalPlacesToShow = "2"
                };
            
            
            //------------Setup for test--------------------------
            SetupArgumentsForFormatNumber("", "", activity);
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var ages = DataObject.Environment.EvalAsListOfStrings("[[Person(*).Age]]", 0);
            Assert.AreEqual("5.27", ages[0]);
            Assert.AreEqual("2.30", ages[1]);
            Assert.AreEqual("1.00", ages[2]);
            Assert.AreEqual("-3.46", ages[3]);
            Assert.AreEqual("0.88", ages[4]);
        } 
         
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenNumberFormatTool_ToCorrectFormat_ComplexObjects()
        {
            var activity = new DsfNumberFormatActivity
                {
                    Expression = "[[result]]",
                    Result = "[[result]]",
                    RoundingType = enRoundingType.Up.GetDescription(),
                    RoundingDecimalPlaces = "2",
                    DecimalPlacesToShow = "2"
                };
            
            
            //------------Setup for test--------------------------
            //SetupArgumentsForFormatNumber("", "", activity);
            DataObject.Environment = new ExecutionEnvironment();
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "5.2687454"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "2.3"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "1"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "-3.4554"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "0.875768"), 0);
            const string dataSource = "[[@Person(*).Age]]";
            const string alias = "[[result]]";
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                //ApplyActivity = activity
            };
            dsfSelectAndApplyActivity.ApplyActivityFunc.Handler = activity;
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = string.Empty;
            TestData = string.Empty;
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var ages = DataObject.Environment.EvalAsListOfStrings("[[@Person(*).Age]]", 0);
            Assert.AreEqual("5.27", ages[0]);
            Assert.AreEqual("2.30", ages[1]);
            Assert.AreEqual("1.00", ages[2]);
            Assert.AreEqual("-3.46", ages[3]);
            Assert.AreEqual("0.88", ages[4]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenNullDataSource_DataObjectsHasCorrectErrors()
        {
            var activity = new DsfNumberFormatActivity
                {
                    Expression = "[[result]]",
                    Result = "[[result]]",
                    RoundingType = enRoundingType.Up.GetDescription(),
                    RoundingDecimalPlaces = "2",
                    DecimalPlacesToShow = "2"
                };
            
            
            //------------Setup for test--------------------------
            //SetupArgumentsForFormatNumber("", "", activity);
            DataObject.Environment = new ExecutionEnvironment();
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "5.2687454"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "2.3"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "1"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "-3.4554"), 0);
            DataObject.Environment.AssignJson(new AssignValue("[[@Person().Age]]", "0.875768"), 0);
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity();
            dsfSelectAndApplyActivity.ApplyActivityFunc.Handler = activity;
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = string.Empty;
            TestData = string.Empty;
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var errors = DataObject.Environment.Errors;
            Assert.AreEqual(2, errors.Count);
            Assert.IsTrue(errors.Contains(ErrorResource.DataSourceEmpty));
            Assert.IsTrue(errors.Contains(string.Format(ErrorResource.CanNotBeEmpty, "Alias")));
        }  

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenNumberFormatTool_ToDifferentResult()
        {
            var activity = new DsfNumberFormatActivity
                {
                    Expression = "[[result]]",
                    Result = "[[b]]",
                    RoundingType = enRoundingType.Up.GetDescription(),
                    RoundingDecimalPlaces = "2",
                    DecimalPlacesToShow = "2"
                };
            
            
            //------------Setup for test--------------------------
            SetupArgumentsForFormatNumber("", "", activity);
            //------------Execute Test---------------------------
            ExecuteProcess(DataObject);
            //------------Assert Results-------------------------
            var ages = DataObject.Environment.EvalAsListOfStrings("[[Person(*).Age]]", 0);
            var evalAsList = DataObject.Environment.EvalAsList("[[b]]", 1);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        [ExpectedException(typeof(NotImplementedException))]
        public void SelectAndApplyActivity_UpdateForEachInputs_ThrowsException()
        {
            var activity = new DsfSelectAndApplyActivity();
            activity.UpdateForEachInputs(new List<Tuple<string, string>>());
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        [ExpectedException(typeof(NotImplementedException))]
        public void SelectAndApplyActivity_UpdateForEachOutputs_ThrowsException()
        {
            var activity = new DsfSelectAndApplyActivity();
            activity.UpdateForEachOutputs(new List<Tuple<string, string>>());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_GetForEachInputs()
        {
            var activity = new DsfSelectAndApplyActivity();
            activity.Alias = "[[Rec(*)]]";
            var dsfForEachItems = activity.GetForEachInputs();
            Assert.AreEqual(dsfForEachItems.Single().Value, "[[Rec(*)]]");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_GetForEachOutputs()
        {
            var activity = new DsfSelectAndApplyActivity();
            activity.Alias = "[[Rec(*)]]";
            var dsfForEachItems = activity.GetForEachOutputs();
            Assert.AreEqual(dsfForEachItems.Single().Value, "[[Rec()]]");
        }

        #region Private Test Methods

        DsfSelectAndApplyActivity SetupArguments(string currentDl, string testData, IDev2Activity activity, bool isInputMapping = false, string inputMapping = null)
        {
            //DsfActivity activity = inputMapping != null ? CreateWorkflow(inputMapping, isInputMapping) : CreateWorkflow();
            DataObject.Environment = new ExecutionEnvironment();
            DataObject.Environment.Assign("[[Person().Name]]", "Bob", 0);
            DataObject.Environment.Assign("[[Person().Name]]", "Dora", 0);
            DataObject.Environment.Assign("[[Person().Name]]", "Superman", 0);
            DataObject.Environment.Assign("[[Person().Name]]", "Batman", 0);
            DataObject.Environment.Assign("[[Person().Name]]", "Orlando", 0);
            const string dataSource = "[[Person(*).Name]]";
            const string alias = "[[a]]";
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,

                //ApplyActivityFunc = activity
            };
            var handler = activity as Activity;
            dsfSelectAndApplyActivity.ApplyActivityFunc.Handler = handler;
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = testData;
            TestData = currentDl;
            return dsfSelectAndApplyActivity;
        }

        DsfSelectAndApplyActivity SetupArgumentsForFormatNumber(string currentDl, string testData, IDev2Activity activity, bool isInputMapping = false, string inputMapping = null)
        {
            //DsfActivity activity = inputMapping != null ? CreateWorkflow(inputMapping, isInputMapping) : CreateWorkflow();
            DataObject.Environment = new ExecutionEnvironment();
            DataObject.Environment.Assign("[[Person().Age]]", "5.2687454", 0);
            DataObject.Environment.Assign("[[Person().Age]]", "2.3", 0);
            DataObject.Environment.Assign("[[Person().Age]]", "1", 0);
            DataObject.Environment.Assign("[[Person().Age]]", "-3.4554", 0);
            DataObject.Environment.Assign("[[Person().Age]]", "0.875768", 0);
            const string dataSource = "[[Person(*).Age]]";
            const string alias = "[[result]]";
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                //ApplyActivity = activity
            };
            dsfSelectAndApplyActivity.ApplyActivityFunc.Handler = activity as Activity;
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = testData;
            TestData = currentDl;
            return dsfSelectAndApplyActivity;
        }

        #endregion Private Test Methods
    }
}
