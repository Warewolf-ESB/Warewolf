using System;
using System.Collections.Generic;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.DsfWebGetRequestWithTimeoutActivityTests
{
    [TestClass]
    public class DsfWebGetRequestWithTimeoutActivityTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = url, Result = result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(url, act.Url);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test1", "Test1");
            var tuple2 = new Tuple<string, string>(Url, "Test2");

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Url);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };
            
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }
        
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test1", "Test1");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_1Update_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(url, dsfForEachItems[0].Name);
            Assert.AreEqual(url, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }
    }
}
