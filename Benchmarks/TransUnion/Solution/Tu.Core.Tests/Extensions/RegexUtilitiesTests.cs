using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Extensions;

namespace Tu.Core.Tests.Extensions
{
    [TestClass]
    public class RegexUtilitiesTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RegexUtilities_IsValidEmail")]
        public void RegexUtilities_IsValidEmail_NullString_False()
        {
            //------------Setup for test--------------------------
            var regexUtilities = new RegexUtilities();

            //------------Execute Test---------------------------
            var result = regexUtilities.IsValidEmail(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }


        [TestMethod]
        [TestCategory("RegexUtilities_IsValidEmail")]
        [Owner("Trevor Williams-Ros")]
        public void RegexUtilities_IsValidEmail_AllVariations_Done()
        {
            //------------Setup for test--------------------------
            var emailAddresses = new[]
            {
                new KeyValuePair<string, bool>("david.jones@proseware.com", true),
                new KeyValuePair<string, bool>("d.j@server1.proseware.com", true),
                new KeyValuePair<string, bool>("jones@ms1.proseware.com", true),
                new KeyValuePair<string, bool>("j.@server1.proseware.com", false),
                new KeyValuePair<string, bool>("j@proseware.com9", true),
                new KeyValuePair<string, bool>("js#internal@proseware.com", true),
                new KeyValuePair<string, bool>("j_9@[129.126.118.1]", true),
                new KeyValuePair<string, bool>("j..s@proseware.com", false),
                new KeyValuePair<string, bool>("js*@proseware.com", false),
                new KeyValuePair<string, bool>("js@proseware..com", false),
                new KeyValuePair<string, bool>("js@proseware.com9", true),
                new KeyValuePair<string, bool>("j.s@server1.proseware.com", true)
            };

            var regexUtilities = new RegexUtilities();

            foreach(var emailAddress in emailAddresses)
            {
                //------------Execute Test---------------------------
                //------------Assert Results-------------------------
                Assert.AreEqual(emailAddress.Value, regexUtilities.IsValidEmail(emailAddress.Key));
            }
        }

    }
}
