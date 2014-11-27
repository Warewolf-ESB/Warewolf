
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Globalization;
using System.IO;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    public class IsValidFileNameRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IsValidFileNameRule_Check")]
        public void IsValidFileNameRule_Check_ItemIsValid_ResultIsNull()
        {
            Verify_Check(true, @"c:\errors1.png");
            Verify_Check(true, @"c:\logs\errors.tx1");
            Verify_Check(true, @"c:\logs\errors.1");
            Verify_Check(true, @"c:\logs\errors1.txt");
            Verify_Check(true, @"\\svr1\logs\errors1.log");
            Verify_Check(true, @"c:\my documents\test.docx");
            Verify_Check(true, @"c:\my documents\my application-.config");
            Verify_Check(true, @"c:\tmp.txt", @"c:\errors1.doc");
            Verify_Check(true, @"C:\Users\barney.buchan\Desktop\debug.log");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IsValidFileNameRule_Check")]
        public void IsValidFileNameRule_Check_ItemIsNotValid_ResultIsError()
        {
            Verify_Check(false, @"c:\log");
            Verify_Check(false, @"c:\log.");
            Verify_Check(false, @"c:\");
            Verify_Check(false, @"c:\*file.pdf");
            Verify_Check(false, @"file.pdf");

            foreach(var c in Path.GetInvalidFileNameChars())
            {
                Verify_Check(false, string.Format(@"c:\myfile{0}.txt", c));
                Verify_Check(c == '\\', string.Format(@"c:\myfile{0}name.txt", c));
            }

            foreach(var c in Path.GetInvalidPathChars())
            {
                Verify_Check(false, string.Format(@"c:\mydir{0}\myfile.txt", c));
                Verify_Check(false, string.Format(@"c:\mydir{0}name\myfile.txt", c));
            }
        }

        // ReSharper disable UnusedParameter.Local
        void Verify_Check(bool isValid, params string[] values)
        // ReSharper restore UnusedParameter.Local
        {
            //------------Setup for test--------------------------
            const char SplitToken = ',';
            Func<string> getValue = () => string.Join(SplitToken.ToString(CultureInfo.InvariantCulture), values);

            var rule = new IsValidFileNameRule(getValue, SplitToken) { LabelText = "The item" };

            //------------Execute Test---------------------------
            var result = rule.Check();

            //------------Assert Results-------------------------
            if(isValid)
            {
                Assert.IsNull(result);
            }
            else
            {
                StringAssert.Contains("The item contains an invalid file name", result.Message);
            }
        }
    }
}
