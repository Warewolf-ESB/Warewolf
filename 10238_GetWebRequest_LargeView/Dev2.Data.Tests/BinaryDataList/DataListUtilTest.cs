using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DataListUtil_UpsertTokens_NullTarget_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_NullTokenizer_ClearsTarget()
        {
            //------------Setup for test--------------------------
            var target = new Collection<ObservablePair<string, string>>
            {
                new ObservablePair<string, string>("key1", "value1"), 
                new ObservablePair<string, string>("key2", "value2")
            };

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsToTarget()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsDuplicatesToTarget()
        {
            //------------Setup for test--------------------------
            const int DuplicateCount = 2;
            const int TokenCount = 3;
            var tokenNumber = 0;
            var iterCount = 0;
            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() =>
            {
                var result = string.Format("[[Var{0}]]", tokenNumber);
                if(++iterCount % DuplicateCount == 0)
                {
                    tokenNumber++;
                }
                return result;
            });

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount * DuplicateCount, target.Count);
            var groups = target.GroupBy(g => g.Key).ToList();
            Assert.AreEqual(3, groups.Count);
            foreach(var grp in groups)
            {
                var enumerator = grp.GetEnumerator();
                var duplicateCount = 0;
                while(enumerator.MoveNext())
                {
                    duplicateCount++;
                }
                Assert.AreEqual(DuplicateCount, duplicateCount);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_ClearsTargetFirst()
        {
            //------------Setup for test--------------------------
            const int TokenCount1 = 5;
            var tokenNumber1 = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber1 < TokenCount1);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber1++));

            var target = new Collection<ObservablePair<string, string>>();

            DataListUtil.UpsertTokens(target, tokenizer.Object);

            // Create a second tokenizer that will return 2 vars with the same name as the first tokenizer and 1 new one
            const int ExpectedCount = 3;
            const int TokenCount2 = 6;
            var tokenNumber2 = 3;
            var tokenizer2 = new Mock<IDev2Tokenizer>();
            tokenizer2.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber2 < TokenCount2);
            tokenizer2.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber2++));


            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer2.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedCount, target.Count);

            // Expect only vars from second tokenizer
            var keys = target.Select(p => p.Key);
            var i = 3;
            foreach(var key in keys)
            {
                var expected = string.Format("[[Var{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_StripsWhiteSpaces()
        {
            //------------Setup for test--------------------------
            const int TokenCount1 = 5;
            var tokenNumber1 = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber1 < TokenCount1);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var {0}]]", tokenNumber1++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(5, target.Count);

            // Expect only vars from second tokenizer
            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[Var{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsPrefixToKey()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "prefix");

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);

            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[prefixVar{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsSuffixToKey()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "prefix", "suffix");

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);

            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[prefixVar{0}suffix]]", i++);
                Assert.AreEqual(expected, key);
            }
        }
    }
}
