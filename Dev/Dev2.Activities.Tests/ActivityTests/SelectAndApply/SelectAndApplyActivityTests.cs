using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.SelectAndApply
{
    public class Person
    {
        public string Name { get; set; }
    }

    [TestClass]
    public class SelectAndApplyActivityTests : BaseActivityUnitTest
    {
        private DsfSelectAndApplyActivity CreateActivity()
        {
            return new DsfSelectAndApplyActivity();
        }
        [TestMethod]
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
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_DisplayName")]
        public void SelectAndApplyActivity_DisplayName_GivenIsCreated_ShouldBeSelectAndApply()
        {
            //------------Setup for test--------------------------
            //var persons = new List<Person>
            //{
            //    new Person
            //    {
            //        Name = "Bob"
            //    },
            //    new Person
            //    {
            //        Name = "Dora"
            //    },
            //    new Person
            //    {
            //        Name = "Superman"
            //    },new Person
            //    {
            //        Name = "Batman"
            //    }
            //};

            //persons.Select(a=>a.Name).ToList().ForEach(a=>a = a.ToUpper());
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(selectAndApplyActivity);
            Assert.AreEqual("Select and apply", selectAndApplyActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenCaseConvertActivityApplied_ToUpperApplied()
        {
            //------------Setup for test--------------------------
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity
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
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenCaseConvertActivityApplied_ToCorrectIndex()
        {
            //------------Setup for test--------------------------
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity
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
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenNumberFormatTool_ToCorrectFormat()
        {
            var activity = new DsfNumberFormatActivity
                {
                    Expression = "[[result]]",
                    Result = "[[result]]",
                    RoundingType = Dev2EnumConverter.ConvertEnumValueToString(enRoundingType.Up),
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
        
        //Ignored the test to be plumbed later
        [Ignore]
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_SetupExecute")]
        public void SelectAndApplyActivity_SetupExecute_GivenSelectAndApplyActivityWithNumberFormat_ToCorrectFormat()
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

            var activity = new DsfNumberFormatActivity
            {
                Expression = "[[result]]",
                Result = "[[result]]",
                RoundingType = Dev2EnumConverter.ConvertEnumValueToString(enRoundingType.Up),
                RoundingDecimalPlaces = "2",
                DecimalPlacesToShow = "2"
            };
            DsfSelectAndApplyActivity dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                ApplyActivity = activity
            };
            DsfSelectAndApplyActivity dsfSelectAndApplyActivityContainerActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                ApplyActivity = dsfSelectAndApplyActivity
            };
          
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivityContainerActivity
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



        #region Private Test Methods

        private DsfActivity CreateWorkflow()
        {
            DsfActivity activity = new DsfActivity
            {
                ServiceName = "SelectApplyTestService",
                InputMapping = ActivityStrings.SelectApply_Input_Mapping,
                OutputMapping = ActivityStrings.SelectApply_Output_Mapping
            };

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            DsfActivity activity = new DsfActivity();
            if (isInputMapping)
            {
                activity.InputMapping = mapping;
                activity.OutputMapping = ActivityStrings.SelectApply_Input_Mapping;
            }
            else
            {
                activity.InputMapping = ActivityStrings.SelectApply_Output_Mapping;
                activity.OutputMapping = mapping;
            }
            activity.ServiceName = "SelectApplyTestService";

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfSelectAndApplyActivity SetupArguments(string currentDl, string testData, IDev2Activity activity, bool isInputMapping = false, string inputMapping = null)
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
            DsfSelectAndApplyActivity dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                ApplyActivity = activity
            };
            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApplyActivity
            };
            CurrentDl = testData;
            TestData = currentDl;
            return dsfSelectAndApplyActivity;
        }
        
        private DsfSelectAndApplyActivity SetupArgumentsForFormatNumber(string currentDl, string testData, IDev2Activity activity, bool isInputMapping = false, string inputMapping = null)
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
            DsfSelectAndApplyActivity dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity
            {
                DataSource = dataSource,
                Alias = alias,
                ApplyActivity = activity
            };
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
