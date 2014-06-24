using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for ForEachActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ForEachActivityFindMissingStrategyTests
    {
        public ForEachActivityFindMissingStrategyTests()
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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region ForEach Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDsfForEachActivityWithADsfActivityInsideItExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfActivity dsfActivity = new DsfActivity();
            dsfActivity.InputMapping = @"<Inputs><Input Name=""reg"" Source=""NUD2347"" DefaultValue=""NUD2347""><Validator Type=""Required"" /></Input><Input Name=""asdfsad"" Source=""registration223"" DefaultValue=""w3rt24324""><Validator Type=""Required"" /></Input><Input Name=""number"" Source=""[[number]]"" /></Inputs>";
            dsfActivity.OutputMapping = @"<Outputs><Output Name=""vehicleVin"" MapsTo=""VIN"" Value="""" /><Output Name=""vehicleColor"" MapsTo=""VehicleColor"" Value="""" /><Output Name=""speed"" MapsTo=""speed"" Value="""" Recordset=""Fines"" /><Output Name=""date"" MapsTo=""date"" Value=""Fines.Date"" Recordset=""Fines"" /><Output Name=""location"" MapsTo=""location"" Value="""" Recordset=""Fines"" /></Outputs>";
            DsfForEachActivity activity = new DsfForEachActivity();
            activity.ForEachElementName = "5";
            activity.DataFunc.Handler = dsfActivity;
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.ForEach);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "NUD2347", "registration223", "[[number]]", "Fines.Date", "5" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetActivityFieldsOffDsfForEachActivityWithADsfMultiAssignActivityInsideItExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfMultiAssignActivity multiAssignActivity = new DsfMultiAssignActivity();
            multiAssignActivity.FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[AssignRight1]]", "[[AssignLeft1]]", 1), new ActivityDTO("[[AssignRight2]]", "[[AssignLeft2]]", 2) };
            DsfForEachActivity activity = new DsfForEachActivity();
            activity.ForEachElementName = "5";
            activity.DataFunc.Handler = multiAssignActivity;
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.ForEach);
            List<string> actual = strategy.GetActivityFields(activity);
            List<string> expected = new List<string> { "[[AssignRight1]]", "[[AssignLeft1]]", "[[AssignRight2]]", "[[AssignLeft2]]", "5" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion
    }
}
