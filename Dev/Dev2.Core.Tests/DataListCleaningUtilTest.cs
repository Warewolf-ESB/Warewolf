using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{


    /// <summary>
    ///This is a test class for DataListUtilTest and is intended
    ///to contain all DataListUtilTest Unit Tests
    ///</summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListCleaningUtilTest
    {
 
        [TestMethod]
        public void SplitIntoRegionsWithScalarsExpectedSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion]], [[secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual("[[firstregion]]", actual[0]);
            Assert.AreEqual("[[secondRegion]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecSetsExpectedSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion().field]], [[secondRegion().field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[secondRegion().field]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithBigGapBetweenRegionsExpectedSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion]],,,||###&&&/// [[secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual("[[firstregion]]", actual[0]);
            Assert.AreEqual("[[secondRegion]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithInvalidRegionsExpectedCannotSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion[[ [[secondRegion[[";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void SplitIntoRegionsWithNoOpenningRegionsExpectedCannotSeperateRegions()
        {
            //Initialize
            const string Expression = "]]firstregion]] ]]secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual(null, actual[0]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecordSetsAndScalarsRecordSetIndexsOfExpectedOneRegion()
        {
            //Initialize
            const string Expression = "[[firstregion1([[scalar]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(Expression);
            //Assert
            Assert.AreEqual(Expression, actual[0]);

        }

        [TestMethod]
        public void SplitIntoRegionsWithScalarsRecordSetsIndexsOfExpectedSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion([[firstregion]]).field]], [[secondRegion([[secondRegion]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(Expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[firstregion]]", actual[1]);
            Assert.AreEqual("[[secondRegion().field]]", actual[2]);
            Assert.AreEqual("[[secondRegion]]", actual[3]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecordSetsAndScalarsRecordSetIndexsOfExpectedSeperateRegions()
        {
            //Initialize
            const string Expression = "[[firstregion([[firstregion1([[scalar]]).field]]).field]], [[secondRegion([[secondRegion1([[scalar1]]).field]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(Expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[firstregion1().field]]", actual[1]);
            Assert.AreEqual("[[scalar]]", actual[2]);
            Assert.AreEqual("[[secondRegion().field]]", actual[3]);
            Assert.AreEqual("[[secondRegion1().field]]", actual[4]);
            Assert.AreEqual("[[scalar1]]", actual[5]);
        }

        //Author: Massimo.Guerrera - Bug 9611
        [TestMethod]
        public void SplitIntoRegionsForFindMissingWithRecordSetAndScalarInvalidRegionExpectedNoRegionsReturned()
        {
            //Initialize
            const string Expression = "[[rec().val]][a]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(Expression);
            //Assert
            Assert.AreEqual(null, actual[0]);
        }

        //Author: Massimo.Guerrera - Bug 9611
        [TestMethod]
        public void SplitIntoRegionsForFindMissingWithRecordSetAndScalarInvalidRegionExpectedRecordsetReturned()
        {
            //Initialize
            const string Expression = "[[rec().val][a]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(Expression);
            //Assert
            Assert.AreEqual("[[rec().val][a]]", actual[0]);
        }

    }
}
