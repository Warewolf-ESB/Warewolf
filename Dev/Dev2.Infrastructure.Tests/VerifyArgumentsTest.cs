using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests
{
    [TestClass]
    public class VerifyArgumentsTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VerifyArgumentsTest_AreNotNull")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyArgumentsTest_AreNotNull_Throws()
        {
            try
            {
                VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    {"a", new object()},
                    {"b", ""},
                    {"c", null},
                    {"d", null},
                    {"e", ""},
                    {"f", ""}
                });
            }
            catch(Exception e)
            {
                Assert.AreEqual(@"The following arguments are not allowed to be null: c
d
", e.Message);
                throw;
            }

        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VerifyArgumentsTest_AreNotNull")]
        public void VerifyArgumentsTest_AreNotNull_DoesNotThrows()
        {

            VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    {"a", new object()},
                    {"b", ""},
                    {"c", ""},
                    {"d", ""},
                    {"e", ""},
                    {"f", ""}
                });



        }
    }
}
