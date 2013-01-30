using System;
using System.Collections.Generic;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using Dev2;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Activities;
using Moq;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    public class ForEachActivityTest : BaseActivityUnitTest
    {
        public ForEachActivityTest()
            : base()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {




        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Number Of Execution Tests

        [TestMethod] // - OK
        public void NumberOfExecutionsNumeric_Expected_TotalExecutions2()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "2"
                          );
          IDSFDataObject result = ExecuteForEachProcess();
            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(2));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsRecordsetWithNoIndex_Expected_TotalExecutions5()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "[[recset()]]"
                          );

            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(5));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsRecordsetWithIndex_Expected_TotalExecutions2()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "[[recset(2)]]"
                          );
            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(2));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsScalar_Expected_TotalExecutions1()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "[[testScalar]]"
                          );
            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsRecordsetWithStar_Expected_TotalExecutions5()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "[[recset(*)]]"
                          );
            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(5));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsNegativeNumber_Expected_TotalExecutions0()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "-2"
                          );
            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        [TestMethod] // - OK
        public void NumberOfExecutionsRecordsetWithNegitiveNumberAsIndex_Expected_TotalExecutions0()
        {
            SetupArguments(
                             ActivityStrings.ForEachCurrentDataList
                           , ActivityStrings.ForEachDataListShape
                           , "[[recset(-2)]]"
                           );
            IDSFDataObject result = ExecuteForEachProcess();

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        #endregion Number Of Execution Tests

        //  Travis.Frisinger - No way to directly test inputs, we need intergration test for this ;)

        #region Input Mapping Tests

//        [TestMethod]
//        public void InputMapping_UsingRecordSetWithStar_Expected_EverythingMapped()
//        {
//            ErrorResultTO errors = new ErrorResultTO();
//            string error = string.Empty;

//            SetupArguments(
//                            ActivityStrings.ForEachCurrentDataList
//                          , ActivityStrings.ForEachDataListShape
//                          , "1"
//                          );


//            IDSFDataObject result = ExecuteForEachProcess();

//            IBinaryDataList bdl = _compiler.FetchBinaryDataList(result.DataListID, out errors);

//            IBinaryDataListEntry entry;
//            bdl.TryGetEntry("resultVar", out entry, out error);

//            Assert.AreEqual("recVal1", entry.FetchScalar().TheValue);

//            //_mockChannel.Verify(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
//        }

//        [TestMethod]
//        public void InputMapping_IndexInRecordSet_Expected_OnlyIterateForIndexNumberinRecordSet()
//        {
//            string inputMapping = ActivityStrings.ForEach_Input_Mapping;
//            inputMapping = inputMapping.Replace("[[recset(*).rec2]]", "[[recset(1).rec2]]");
//            SetupArguments(
//                            ActivityStrings.ForEachCurrentDataList
//                          , ActivityStrings.ForEachDataListShape
//                          , "[[recset(*)]]"
//                          , true
//                          , inputMapping
//                          );

//            IDSFDataObject result = ExecuteForEachProcess();

//            _mockChannel.Verify(channel => channel.ExecuteCommand(It.Is<string>(c => c.Contains(@"<innertesting>
//        <innertest>rec2Val1</innertest>
//      </innertesting>")), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(5));
//        }

//        [TestMethod]
//        public void InputMapping_RecsetWithoutIndex_Expected_LastValueAlwaysMapped()
//        {
//            string inputMapping = ActivityStrings.ForEach_Input_Mapping;
//            inputMapping = inputMapping.Replace("[[recset(*).rec2]]", "[[recset().rec2]]");

//            string activityDataList = ActivityStrings.ForEachCurrentDataList;
//            activityDataList = activityDataList.Insert(activityDataList.LastIndexOf("</recset>") + "</recset>".Length, "\r\n<recset><rec>recVal6</rec><rec2>rec2Val6</rec2></recset>");

//            SetupArguments(
//                            activityDataList
//                          , ActivityStrings.ForEachDataListShape
//                          , "[[recset(*)]]"
//                          , true
//                          , inputMapping
//                          );

//            IDSFDataObject result = ExecuteForEachProcess();

//            _mockChannel.Verify(channel => channel.ExecuteCommand(It.Is<string>(c => c.Contains(@"<innertesting>
//        <innertest>rec2Val6</innertest>
//      </innertesting>")), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(6));
//        }

//        [TestMethod]
//        public void InputMapping_RecsetWithScalar_Expected_ScalarAlwaysMapped()
//        {
//            string inputMapping = ActivityStrings.ForEach_Input_Mapping;
//            inputMapping = inputMapping.Replace("[[recset(*).rec2]]", "[[var]]");
//            SetupArguments(
//                            ActivityStrings.ForEachCurrentDataList
//                          , ActivityStrings.ForEachDataListShape
//                          , "[[recset(*)]]"
//                          , true
//                          , inputMapping
//                          );
//            IDSFDataObject result = ExecuteForEachProcess();

//            _mockChannel.Verify(channel => channel.ExecuteCommand(It.Is<string>(c => c.Contains(@"<innertesting>
//        <innertest>Static_Scalar</innertest>
//      </innertesting>")), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(5));
//        }

//        [TestMethod]
//        public void InputMapping_RecsetWithStatic_Expected_StaticAlwaysMapped()
//        {
//            string inputMapping = ActivityStrings.ForEach_Input_Mapping;
//            inputMapping = inputMapping.Replace("[[recset(*).rec2]]", "Hello");

//            SetupArguments(
//                            ActivityStrings.ForEachCurrentDataList
//                          , ActivityStrings.ForEachDataListShape
//                          , "[[recset()]]"
//                          , true
//                          , inputMapping
//                          );


//            IDSFDataObject result = ExecuteForEachProcess();

//            _mockChannel.Verify(channel => channel.ExecuteCommand(It.Is<string>(c => c.Contains(@"<innertesting>
//        <innertest>Hello</innertest>
//      </innertesting>")), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(5));
//        }

        #endregion Input Mapping Tests

        #region Output Mapping Tests

        [TestMethod] // - OK
        public void OutputMapping_UsingRecordSetWithNoIndex_Expected_AllRecordSetValsMappedCorrectly()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "1"
                          );


            IDSFDataObject result = ExecuteForEachProcess();
            List<string> expected = new List<string> { "recVal1"
                                                     , "recVal2"
                                                     , "recVal3"
                                                     , "recVal4"
                                                     , "recVal5"
                                                     , ""
                                                     };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "rec", out error);

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());

        }

        [TestMethod] // - OK
        public void OutputMapping_UsingRecordSetwithStar_Expected_OutputMappedToRecordSet()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(*).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "1"
                          , false
                          , outputMapping
                          );


            IDSFDataObject result = ExecuteForEachProcess();
            List<string> expected = new List<string> { "recVal1"
                                                     , "recVal2"
                                                     , "recVal3"
                                                     , "recVal4"
                                                     , "recVal5"
                                                     , "" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "rec", out error);
            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod] // - OK
        public void OutputMapping_UsingRecordSetMappedToScalar_ExpectedSameValueMappedForAllExecutions()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[var]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "1"
                          , false
                          , outputMapping
                          );


            IDSFDataObject result = ExecuteForEachProcess();
            string expected = "recVal1";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "resultVar", out actual, out error);
            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
            Assert.AreEqual(expected, actual);
        }


        [TestMethod] // - OK
        public void OutputMapping_UsingRecordsetWithAnIndex_Expected_SameStaticValuePassedInForAllExecutions()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "1"
                          , false
                          , outputMapping
                          );
            IDSFDataObject result = ExecuteForEachProcess();

            string expected = "recVal1";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "resultVar", out actual, out error);

            _mockChannel.Verify(c => c.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
            Assert.AreEqual(expected, actual);

        }

        [TestMethod] // - OK
        public void ForEach_WrongInput_Expected_Exception_Input_String_Not_In_Right_Format()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , "[[1]]"
                          , false
                          , outputMapping
                          );
            IDSFDataObject result = ExecuteForEachProcess();
            Assert.IsTrue(_compiler.HasErrors(result.DataListID));
        }

        #endregion Output Mapping Tests

        #region Private Test Methods

        private DsfActivity CreateWorkflow()
        {
            DsfActivity activity = new DsfActivity();
            activity.ServiceName = "MyTestService";
            activity.InputMapping = ActivityStrings.ForEach_Input_Mapping;
            activity.OutputMapping = ActivityStrings.ForEach_Output_Mapping;

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            DsfActivity activity = new DsfActivity();
            if (isInputMapping)
            {
                activity.InputMapping = mapping;
                activity.OutputMapping = ActivityStrings.ForEach_Output_Mapping;
            }
            else
            {
                activity.InputMapping = ActivityStrings.ForEach_Input_Mapping;
                activity.OutputMapping = mapping;
            }
            activity.ServiceName = "MyTestService";

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private void SetupArguments(string currentDL, string testData, string forEachElementName, bool isInputMapping = false, string inputMapping = null)
        {
            ActivityFunc<string, bool> activityFunction = new ActivityFunc<string, bool>();
            DsfActivity activity;
            if (inputMapping != null)
            {
                activity = CreateWorkflow(inputMapping, isInputMapping);
            }
            else
            {
                activity = CreateWorkflow();
            }

            activityFunction.Handler = activity;

            TestStartNode = new FlowStep
            {
                Action = new DsfForEachActivity
                {
                    DataFunc = activityFunction,
                    ForEachElementName = forEachElementName,
                }
            };

            CurrentDL = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
