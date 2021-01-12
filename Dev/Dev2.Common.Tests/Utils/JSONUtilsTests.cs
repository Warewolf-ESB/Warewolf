/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Utils
{
    [TestClass]
    public class JSONUtilsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonUtils))]
        public void JSONUtils_ScrubJSON()
        {
            var fetch = JsonResource.Fetch("ForEachWorkFlow");
            var value = JsonUtils.ScrubJson(fetch);

            Assert.AreEqual(fetch, value);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonUtils))]
        public void JSONUtils_ScrubJSON_Clean()
        {
            var fetch = JsonResource.Fetch("ForEachWorkFlow");
            Assert.AreEqual(fetch, JsonUtils.ScrubJson("\"" + fetch + "\""));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonUtils))]
        public void JSONUtils_Format_Empty()
        {
            Assert.AreEqual("", JsonUtils.Format(""));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JsonUtils))]
        public void JSONUtils_Format_GivenMultibyteCharacter()
        {
            Assert.AreEqual("{\r\n\t\"field\":\"euro€value\"\r\n}", JsonUtils.Format(@"{""field"":""euro€value""}"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("JSON")]
        public void JSONUtils_Format()
        {
            var fetch = JsonResource.Fetch("Test");
            var formatted = JsonUtils.Format(fetch);
            var result = "[\r\n\t{\r\n\t\t\"ID\": \"00000000-0000-0000-0000-000000000000\",\r\n\t\t\"ParentID\": null,\r\n\t\t\"SourceResourceID\": \"037b7b8c-be29-4f2a-b648-6569051ca127\",\r\n\t\t\r\n\t}\r\n]";
            Assert.AreEqual(result, formatted);
        }
    }
}
