/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class WorkflowExecutorTestInputsPayloadTests
    {
        static ServiceTestModelTO Test(params (string Variable, string Value, bool EmptyIsNull)[] inputs)
        {
            var list = new List<IServiceTestInput>();
            foreach (var (variable, value, emptyIsNull) in inputs)
            {
                list.Add(new ServiceTestInputTO { Variable = variable, Value = value, EmptyIsNull = emptyIsNull });
            }
            return new ServiceTestModelTO { Inputs = list };
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildTestInputsPayload_NoInputs_ReturnsNull()
        {
            Assert.IsNull(WorkflowExecutor.BuildTestInputsPayload(new ServiceTestModelTO { Inputs = new List<IServiceTestInput>() }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildTestInputsPayload_ScalarInputs_AreFlat()
        {
            var json = JObject.Parse(WorkflowExecutor.BuildTestInputsPayload(Test(("[[amount]]", "123.45", false), ("name", "x", false))));

            Assert.AreEqual("123.45", (string)json["amount"]);
            Assert.AreEqual("x", (string)json["name"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildTestInputsPayload_RecordsetInputs_AreGroupedIntoRows()
        {
            var payload = WorkflowExecutor.BuildTestInputsPayload(Test(
                ("[[Dates(2).Date]]", "2023-11-02", false),
                ("[[Dates(1).Date]]", "2024-03-15", false),
                ("[[Dates(1).Note]]", "a", false),
                ("[[Other]]", "o", false)));

            var json = JObject.Parse(payload);
            var rows = (JArray)json["Dates"];

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("2024-03-15", (string)rows[0]["Date"]);
            Assert.AreEqual("a", (string)rows[0]["Note"]);
            Assert.AreEqual("2023-11-02", (string)rows[1]["Date"]);
            Assert.AreEqual("o", (string)json["Other"]);
            Assert.IsNull(json["Dates(1).Date"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildTestInputsPayload_EmptyIsNull_SkipsEmptyRecordsetAndScalarInputs()
        {
            var payload = WorkflowExecutor.BuildTestInputsPayload(Test(
                ("[[Rows(1).Name]]", "", true),
                ("[[blank]]", "", true)));

            Assert.IsNull(payload);
        }
    }
}
