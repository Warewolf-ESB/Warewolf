
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
using System.Text.RegularExpressions;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDateTimeDifferenceWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfDateTimeDifferenceWFTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Test_DateTimeDifference_Simple()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/DateTimeDifference_Simple_Test");
            string expected = @"<MyDayOfBirth>1988/10/14 08:32:21 AM</MyDayOfBirth>    <Today>2012/10/08 09:43:05 AM</Today>    <DateFormat>yyyy/mm/dd 12h:min:ss am/pm</DateFormat>    <MyAgeInYears>23</MyAgeInYears>    <MyAgeInMonths>288</MyAgeInMonths>    <MyAgeInDays>8760</MyAgeInDays>    <MyAgeInWeeks>1251</MyAgeInWeeks>    <MyAgeInHours>210241</MyAgeInHours>    <MyAgeInMinutes>12614470</MyAgeInMinutes>    <MyAgeInSeconds>756868244</MyAgeInSeconds>    <MyAgeInSplitSeconds>756868244000</MyAgeInSplitSeconds>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_DateTimeDifference_Complex()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/DateTimeDifference_Complex_Test");
            const string expected = @"<DataList><MyDayOfBirth>1988/10/14 08:32:21 AM</MyDayOfBirth><DateFormat>yyyy/mm/dd 12h:min:ss am/pm</DateFormat><MyAgeInYears>27</MyAgeInYears><TenThousandDaysAlive>2016/03/01 08:32:21 AM</TenThousandDaysAlive>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}
