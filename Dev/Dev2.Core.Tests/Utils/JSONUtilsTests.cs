using Dev2.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Utils
{
    [TestClass]
    public class JSONUtilsTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("JSONUtils_ReplaceSlashes")]
        public void JSONUtils_ReplaceSlashes_BackSlashes_SlashesReplaced()
        {
            //------------Execute Test---------------------------
            var actual = JSONUtils.ReplaceSlashes(@"a\b\c");

            // Assert Slashes Replaced
            Assert.AreEqual("a/b/c", actual, "Slashes not replaced by JSON util");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("JSONUtils_ReplaceSlashes")]
        public void JSONUtils_ReplaceSlashes_BackSlashesBeforeQuotes_SlashesNotReplaced()
        {
            //------------Execute Test---------------------------
            var actual = JSONUtils.ReplaceSlashes(@"a\""b\""c\""");

            // Assert Slashes Not Replaced
            Assert.AreEqual(@"a\""b\""c\""", actual, "Slashes not replaced by JSON util");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("JSONUtils_ReplaceSlashes")]
        public void JSONUtils_ReplaceSlashes_TrailingSlashes_SlashesStillReplaced()
        {
            //------------Execute Test---------------------------
            var actual = JSONUtils.ReplaceSlashes(@"a\\b/c\\");

            // Assert Slashes Replaced
            Assert.AreEqual(@"a/b/c/", actual, "Slashes not replaced by JSON util");
        }
    }
}
