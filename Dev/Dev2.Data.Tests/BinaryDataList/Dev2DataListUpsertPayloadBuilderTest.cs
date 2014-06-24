using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Factories;
using Dev2.DataList.Contract.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DataListUpsertPayloadBuilderTest
    {
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we properly iterate star indexes when using the ReplaceStartWithFixedIndex option")]
        [TestCategory("Dev2DataListUpsertPayloadBuilder,UnitTest")]
        public void Dev2DataListUpsertPayloadBuilder_UnitTest_CanReplaceStarWithFixedIndexOnFlush()
        {
            IDev2DataListUpsertPayloadBuilder<string> builder = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            builder.ReplaceStarWithFixedIndex = true;

            builder.Add("[[rs(*).val]]", "aaa");
            builder.FlushIterationFrame();
            builder.Add("[[rs(*).val]]", "aaa");

            var items = builder.FetchFrames(true);
            int idx = 1;
            foreach (var itm in items)
            {
                var exp = itm.FetchNextFrameItem().Expression;
                var expected = "rs("+idx+").val";

                StringAssert.Contains(exp, expected, "Index substitution did not occur correctly");

                idx++;
            }
        }


        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure we do not replace * with a fixed index when the option is not on")]
        [TestCategory("Dev2DataListUpsertPayloadBuilder,UnitTest")]
        public void Dev2DataListUpsertPayloadBuilder_UnitTest_KeepsStarIndexingWhenNotReplacing()
        {
            IDev2DataListUpsertPayloadBuilder<string> builder = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            builder.Add("[[rs(*).val]]", "aaa");
            builder.FlushIterationFrame();
            builder.Add("[[rs(*).val]]", "aaa");

            var items = builder.FetchFrames(true);
            int idx = 1;
            foreach (var itm in items)
            {
                var exp = itm.FetchNextFrameItem().Expression;
                const string expected = "rs(*).val";

                StringAssert.Contains(exp, expected, "Index substitution occurred when not active");

                idx++;
            }
        }
    }
}
