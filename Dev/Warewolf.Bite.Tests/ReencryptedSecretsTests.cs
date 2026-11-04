/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Bite.Tests
{
    /// <summary>Secrets encrypted again on every load are not reader differences.</summary>
    [TestClass]
    public class ReencryptedSecretsTests
    {
        const string Path = "nodes.n0000.Password";
        const string StoredBlob = "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAstored";
        const string FirstLoad = "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAfirst=";
        const string SecondLoad = "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAsecond";
        static readonly string Source = $"<uaba:DsfSendEmailActivity Password=\"{StoredBlob}\" />";

        /// <summary>As ActivityGraphDiff reports a string: JSON, with '+' escaped.</summary>
        static string Json(string value) => "\"" + value.Replace("+", "\\u002B") + "\"";

        static bool Ignored(string? expected, string? actual) =>
            ReencryptedSecrets.IsReencryptionDifference(new GraphDifference(Path, expected, actual), Source);

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_TwoDpapiBlobsNeitherStored_IsIgnored()
        {
            Assert.IsTrue(Ignored(Json(FirstLoad), Json(SecondLoad)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_TwoAesHookValuesNeitherStored_IsIgnored()
        {
            Assert.IsTrue(Ignored(Json("WFAES::first"), Json("WFAES::second")));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_OneSideIsTheStoredValue_IsADifference()
        {
            Assert.IsFalse(Ignored(Json(StoredBlob), Json(SecondLoad)), "A reader that kept the stored value while the other changed it differs.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_OneSideIsPlainText_IsADifference()
        {
            Assert.IsFalse(Ignored(Json("P@ssw0rd"), Json(SecondLoad)), "A reader that decrypted the secret differs.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_OneSideMissing_IsADifference()
        {
            Assert.IsFalse(Ignored(Json(FirstLoad), null));
            Assert.IsFalse(Ignored(null, Json(SecondLoad)));
            Assert.IsFalse(Ignored(Json(FirstLoad), "null"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_OtherBase64Values_AreDifferences()
        {
            Assert.IsFalse(Ignored(Json("aGVsbG8gd29ybGQ="), Json("Z29vZGJ5ZSB3b3JsZA==")));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteShadow")]
        public void ReencryptedSecrets_EscapedPlus_IsReadAsTheBlobItself()
        {
            Assert.IsTrue(Json(FirstLoad).Contains("\\u002B"), "the test must exercise the escaped form");
            Assert.IsFalse(Ignored(Json(StoredBlob), Json(StoredBlob + "x")), "the stored blob is still recognised once unescaped");
            Assert.IsTrue(Ignored(Json(FirstLoad), Json(SecondLoad)));
        }
    }
}
