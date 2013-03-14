using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    [TestClass]
    public class DsfDateTimeActivityWFTest {
        
        #region DateTime Complex Tests

        [TestMethod]
        public void TestDateTimeWithComplexLiteral()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "DateAndTimeWithComplexLiteral");
            string expected = @"<InputDate>I was born on the 14th day of October in the year of 1988 by Greenwich Mean Time</InputDate>    <InputFormat>'I was born on the 'dd'th day of 'MM' in the year of 'yyyy' by 'ZZZ</InputFormat>    <OutputFormat>yyyy'-'mm'-'dd' 'Z</OutputFormat>    <MyBirthday>1988-10-14 GMT</MyBirthday>";
            
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion DateTime Complex Tests

        #region DateTime Nominal Tests

        [TestMethod]
        public void TestDateTimeWithNoOutputFormat()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "DateAndTimeWithNotOutputFormat");

            string expected = @"<InputDate>2012-08-20</InputDate>    <InputFormat>yyyy'-'m'-'d</InputFormat>    <MyBirthday>2012-8-20</MyBirthday>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void TestDateTimeWithNoInputFormat()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "DefaultDateTimeInputFormatTest");

            string expected = @"<Result>2013/03/13 08:12:25 PM</Result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion
    }
}
