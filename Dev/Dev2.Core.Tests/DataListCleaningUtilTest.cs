
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
    // ReSharper disable InconsistentNaming
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

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_VariableWithChildVariable_TwoVariables()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("[[a[[b]]]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(2, pieces.Count);
            Assert.AreEqual("[[b]]", pieces[0]);
            Assert.AreEqual("[[a]]", pieces[1]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_EmptyString_ZeroVariable()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, pieces.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_Number_ZeroVariable()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("19");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, pieces.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_RecursiveVariable_OneVariable()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("[[[[[[d]]]]]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, pieces.Count);
            Assert.AreEqual("[[d]]", pieces[0]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_MalFormedRecursiveVariable_TwoVariables()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("[[[[[[d]]]]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(2, pieces.Count);
            Assert.AreEqual("[[d]]", pieces[0]);
            Assert.AreEqual("[[]]]", pieces[1]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_ComplexRecordset_TwoVariable()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("[[rec().a[[a]]]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(2, pieces.Count);
            Assert.AreEqual("[[a]]", pieces[0]);
            Assert.AreEqual("[[rec().a]]", pieces[1]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListCleaningUtil_FindAllLanguagePieces")]
        public void DataListCleaningUtils_FindAllLanguagePieces_EmptyVariable_OneVariable()
        {
            //------------Execute Test---------------------------
            var pieces = DataListCleaningUtils.FindAllLanguagePieces("[[]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, pieces.Count);
            Assert.AreEqual("[[]]", pieces[0]);
        }
    }   
}
