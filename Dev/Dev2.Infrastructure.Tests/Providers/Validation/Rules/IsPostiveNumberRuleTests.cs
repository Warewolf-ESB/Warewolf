
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Globalization;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    public class IsPostiveNumberRuleTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsPostiveNumberRule_Check")]
        public void IsPostiveNumberRule_Check_ItemIsValid_ResultIsNull()
        {
            Verify_Check(true, @"1");
            Verify_Check(true, @"10");
            Verify_Check(true, @"0");
            Verify_Check(true, @"3212329");
            Verify_Check(true, @"746373");
            Verify_Check(true, @"2589");
            Verify_Check(true, int.MaxValue.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsPostiveNumberRule_Check")]
        public void IsPostiveNumberRule_Check_ItemIsNotValid_ResultIsError()
        {
            Verify_Check(false, @"-1");
            Verify_Check(false, @"-8232");
            Verify_Check(false, @"aasdas");
            Verify_Check(false, @"52.253");
            Verify_Check(false, @"-89.63");
            Verify_Check(false, int.MinValue.ToString(CultureInfo.InvariantCulture));
        }

        // ReSharper disable UnusedParameter.Local
        void Verify_Check(bool isValid, string value)
        // ReSharper restore UnusedParameter.Local
        {
            //------------Setup for test--------------------------

            var rule = new IsPositiveNumberRule(() => value) { LabelText = "The item" };

            //------------Execute Test---------------------------
            var result = rule.Check();

            //------------Assert Results-------------------------
            if(isValid)
            {
                Assert.IsNull(result);
            }
            else
            {
                StringAssert.Contains("The item must be a real number", result.Message);
            }
        }
    }
}
