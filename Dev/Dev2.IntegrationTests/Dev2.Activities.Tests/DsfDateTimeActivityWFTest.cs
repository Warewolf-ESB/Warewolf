
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
using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfDateTimeActivityWFTest
    {

        #region DateTime Complex Tests

        [TestMethod]
        public void TestDateTimeWithComplexLiteral()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/DateAndTimeWithComplexLiteral");
            string expected = @"<InputDate>I was born on the 14th day of October in the year of 1988 by Greenwich Mean Time</InputDate>    <InputFormat>'I was born on the 'dd'th day of 'MM' in the year of 'yyyy' by 'ZZZ</InputFormat>    <OutputFormat>yyyy'-'mm'-'dd' 'Z</OutputFormat>    <MyBirthday>1988-10-14 GMT</MyBirthday>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData.Unescape(), expected);
        }

        #endregion DateTime Complex Tests

        #region DateTime Nominal Tests

        [TestMethod]
        public void TestDateTimeWithNoOutputFormat()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/DateAndTimeWithNotOutputFormat");

            string expected = @"<InputDate>2012-08-20</InputDate>    <InputFormat>yyyy'-'m'-'d</InputFormat>    <MyBirthday>2012-8-20</MyBirthday>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData.Unescape(), expected);
        }

        [TestMethod]
        public void TestDateTimeWithNoInputFormat()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/DefaultDateTimeInputFormatTest");

            string expected = @"<now>11 04 2013 09:30:54.0 AM</now>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");


            var ver = Environment.Version;
            var win8Ver = new Version(6, 2);

            if(ver >= win8Ver && string.IsNullOrEmpty(ResponseData))
            {
                Assert.Inconclusive("Strange difference between Travis.Frisinger account and IntegrationTest Account?!");
            }
            else
            {
                StringAssert.Contains(ResponseData, expected);
            }


        }

        #endregion
    }
}
