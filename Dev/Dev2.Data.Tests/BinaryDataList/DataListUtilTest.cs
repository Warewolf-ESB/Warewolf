using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    public class DataListUtilTest
    {
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure star is replaced with an index")]
        [TestCategory("DataListUtil,UnitTest")]
        public void DataListUtil_UnitTest_ReplaceStarWithFixedIndex()
        {
            const string exp = "[[rs(*).val]]";
            var result = DataListUtil.ReplaceStarWithFixedIndex(exp, 1);

            Assert.AreEqual("[[rs(1).val]]", result, "Did not replace index in recordset");
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure star is not replaced with an invalid index")]
        [TestCategory("DataListUtil,UnitTest")]
        public void DataListUtil_UnitTest_NotReplaceStarWithInvalidIndex()
        {
            const string exp = "[[rs(*).val]]";
            var result = DataListUtil.ReplaceStarWithFixedIndex(exp, -1);

            Assert.AreEqual("[[rs(*).val]]", result, "Replaced with invalid index in recordset");
        }
    }
}
