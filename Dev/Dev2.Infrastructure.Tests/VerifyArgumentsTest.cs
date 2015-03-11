
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
