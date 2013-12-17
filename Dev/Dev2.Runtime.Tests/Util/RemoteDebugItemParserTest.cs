using System;
using System.Collections.Generic;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
    public class RemoteDebugItemParserTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RemoteDebugItemParser_Parse")]
        public void RemoteDebugItemParser_Parse_WhenValidJsonList_ExpectItems()
        {
            //------------Setup for test--------------------------
            List<DebugState> items = new List<DebugState>
            {
                new DebugState {ActivityType = ActivityType.Workflow, ClientID = Guid.Empty, DisplayName = "DebugState"}
            };

            var data = JsonConvert.SerializeObject(items);

            //------------Execute Test---------------------------
            var result = RemoteDebugItemParser.ParseItems(data);

            //------------Assert Results-------------------------
            Assert.AreEqual(1,result.Count);
            Assert.AreEqual("DebugState", result[0].DisplayName);
            Assert.AreEqual(ActivityType.Workflow, result[0].ActivityType);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RemoteDebugItemParser_Parse")]
        public void RemoteDebugItemParser_Parse_WhenNullJsonList_ExpectNull()
        {
            //------------Setup for test--------------------------
            var data = JsonConvert.SerializeObject(null);

            //------------Execute Test---------------------------
            var result = RemoteDebugItemParser.ParseItems(data);

            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }
    }
}
