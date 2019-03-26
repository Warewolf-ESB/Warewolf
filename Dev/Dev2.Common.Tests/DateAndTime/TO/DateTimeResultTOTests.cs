/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Dev2.Common.Tests.DateAndTime.TO
{
    [TestClass]
    public class DateTimeResultTOTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_Constructor_Sets_TimeZone()
        {
            var dateTimeResultTO = new DateTimeResultTO();

            Assert.AreEqual("South Africa Standard Time", dateTimeResultTO.TimeZone.Name);
            Assert.AreEqual("(UTC+02:00) Harare, Pretoria", dateTimeResultTO.TimeZone.LongName);
            Assert.AreEqual("South Africa Standard Time", dateTimeResultTO.TimeZone.ShortName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeTime_Is24HisTrue_HoursGreaterThan12()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Is24H = true;
            dateTimeResultTO.Hours = 16;
            dateTimeResultTO.NormalizeTime();
            Assert.AreEqual(DateTimeAmPm.pm, dateTimeResultTO.AmPm);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeTime_Is24HisFalse_HoursGreaterThan12()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Is24H = false;
            dateTimeResultTO.Hours = 16;
            dateTimeResultTO.NormalizeTime();
            Assert.AreEqual(DateTimeAmPm.pm, dateTimeResultTO.AmPm);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeTime_AmPm_IsEqual_pm()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Hours = 6;
            dateTimeResultTO.AmPm = DateTimeAmPm.pm;
            dateTimeResultTO.NormalizeTime();
            Assert.AreEqual(18, dateTimeResultTO.Hours);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeTime_AmPm_IsEqual_12am()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Hours = 12;
            dateTimeResultTO.AmPm = DateTimeAmPm.am;
            dateTimeResultTO.NormalizeTime();
            Assert.AreEqual(0, dateTimeResultTO.Hours);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Everything_Equal_0()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Years = 0;
            dateTimeResultTO.Months = 0;
            dateTimeResultTO.Days = 0;
            dateTimeResultTO.Hours = 0;
            dateTimeResultTO.Seconds = 0;
            dateTimeResultTO.Milliseconds = 0;
            dateTimeResultTO.DaysOfYear = 0;
            dateTimeResultTO.Weeks = 0;
            
            var returnDate = dateTimeResultTO.ToDateTime();

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(1, dateTimeResultTO.Months);
            Assert.AreEqual(1, dateTimeResultTO.Days);
            Assert.AreEqual(0, dateTimeResultTO.Hours);
            Assert.AreEqual(0, dateTimeResultTO.Minutes);
            Assert.AreEqual(0, dateTimeResultTO.Seconds);
            Assert.AreEqual(0, dateTimeResultTO.Milliseconds);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Days_Equal_0_DaysOfWeek_NotEqual_0()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Years = 0;
            dateTimeResultTO.Months = 0;
            dateTimeResultTO.DaysOfWeek = 3;

            var returnDate = dateTimeResultTO.ToDateTime();

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(1, dateTimeResultTO.Months);
            Assert.AreEqual(3, dateTimeResultTO.Days);
            
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Months_Equal_0_DaysOfYear_NotEqual_0()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Years = 0;
            dateTimeResultTO.Months = 0;
            dateTimeResultTO.DaysOfYear = 20;

            var returnDate = dateTimeResultTO.ToDateTime();

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(1, dateTimeResultTO.Months);
            Assert.AreEqual(20, dateTimeResultTO.Days);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Months_Equal_0_Weeks_NotEqual_0()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            dateTimeResultTO.Years = 0;
            dateTimeResultTO.Months = 0;
            dateTimeResultTO.Weeks = 20;

            var returnDate = dateTimeResultTO.ToDateTime();

            var tmpDate = CultureInfo.CurrentCulture.Calendar.AddWeeks(new System.DateTime(1, 1, 1), 20);
            var Months = tmpDate.Month;
            var Days = tmpDate.Day;

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(Months, dateTimeResultTO.Months);
            Assert.AreEqual(Days, dateTimeResultTO.Days);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_SetEra()
        {
            var dateTimeResultTO = new DateTimeResultTO();
            var value = "A.D";
            dateTimeResultTO.Era = value.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(value, dateTimeResultTO.Era);
        }
    }
}
