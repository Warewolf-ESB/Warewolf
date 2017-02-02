/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

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
                var message = e.Message;

                var expected = @"The following arguments are not allowed to be null: cd";
                FixBreaks(ref expected, ref message);
                Assert.AreEqual(expected, message);
                throw;
            }

        }

        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "").Replace("\r", "").ToString();
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
