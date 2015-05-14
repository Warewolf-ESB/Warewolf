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
        public void JsonMappingTo_CanAddRemove()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.SourceName = "bob";
            jsonMappingTo.DestinationName = "";
            Assert.IsTrue(jsonMappingTo.CanRemove());
            Assert.IsFalse(jsonMappingTo.CanAdd());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_SourceName")]
        public void JsonMappingTo_SourceName_SetsDestination()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.DestinationName = "";
            jsonMappingTo.SourceName = "[[bob]]";

            Assert.AreEqual("bob",jsonMappingTo.DestinationName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("JsonMappingTo_SourceName")]
        public void JsonMappingTo_SourceName_SetsDestinationRecset()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            jsonMappingTo.DestinationName = "";
            jsonMappingTo.SourceName = "[[bobby().tables]]";
     
            Assert.AreEqual("bobby", jsonMappingTo.DestinationName);
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
