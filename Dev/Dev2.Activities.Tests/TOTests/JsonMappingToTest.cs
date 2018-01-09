using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class JsonMappingToTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_Validate")]
        public void JsonMappingTo_Validate_ValidAndInvalidPassThrough()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            Assert.IsTrue(jsonMappingTo.GetRuleSet("SourceName", "").ValidateRules().Count == 0);
            jsonMappingTo.SourceName = "bob";
            Assert.IsTrue(jsonMappingTo.GetRuleSet("SourceName","").ValidateRules().Count==0);
            jsonMappingTo.SourceName = "[[rec()]],[[a]]";
         



            //------------Execute Test---------------------------
            Assert.IsTrue(jsonMappingTo.GetRuleSet("SourceName", "[[rec()]],[[a]]").ValidateRules().Count > 0);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_Clear")]
        public void JsonMappingTo_Clear()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "bob";
            jsonMappingTo.DestinationName = "asd";



            //------------Execute Test---------------------------
            jsonMappingTo.ClearRow();
            //------------Assert Results-------------------------
            Assert.AreEqual("",jsonMappingTo.SourceName);
            Assert.AreEqual("", jsonMappingTo.DestinationName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_AddRemove")]
        public void JsonMappingTo_CanAddRemove_NoSourceAndDestinationName_CanRemoveTrue_CanAddFalse()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "";
            jsonMappingTo.DestinationName = "";
            //-------------Assert-----------------------------------
            Assert.IsTrue(jsonMappingTo.CanRemove());
            Assert.IsFalse(jsonMappingTo.CanAdd());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_AddRemove")]
        public void JsonMappingTo_CanAddRemove_HasSourceName_CanRemoveFalse_CanAddTrue()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "[[val]]";
            jsonMappingTo.DestinationName = "";
            //-------------Assert-----------------------------------
            Assert.IsFalse(jsonMappingTo.CanRemove());
            Assert.IsTrue(jsonMappingTo.CanAdd());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_AddRemove")]
        public void JsonMappingTo_CanAddRemove_HasDestinationName_CanRemoveFalse_CanAddTrue()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "";
            jsonMappingTo.DestinationName = "val";
            //-------------Assert-----------------------------------
            Assert.IsFalse(jsonMappingTo.CanRemove());
            Assert.IsTrue(jsonMappingTo.CanAdd());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_AddRemove")]
        public void JsonMappingTo_CanAddRemove_HasSourceAndDestinationName_CanRemoveFalse_CanAddTrue()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "[[val]]";
            jsonMappingTo.DestinationName = "val";
            //-------------Assert-----------------------------------
            Assert.IsFalse(jsonMappingTo.CanRemove());
            Assert.IsTrue(jsonMappingTo.CanAdd());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_SourceName")]
        public void JsonMappingTo_SourceName_DoesNotSetsDestinationRecset()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.DestinationName = "asdas";
            jsonMappingTo.SourceName = "[[bobby().tables]]";

            Assert.AreEqual("asdas", jsonMappingTo.DestinationName);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_Ctor")]
        public void JsonMappingTo_Ctor()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo("[[bob]]",1,false);
            Assert.AreEqual("bob", jsonMappingTo.DestinationName);
            Assert.AreEqual("[[bob]]", jsonMappingTo.SourceName);
            Assert.IsFalse(jsonMappingTo.CanRemove());
            Assert.AreEqual(1,jsonMappingTo.IndexNumber);
        }


    }
}
