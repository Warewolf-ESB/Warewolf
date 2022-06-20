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

            Assert.IsTrue("South Africa Standard Time" == dateTimeResultTO.TimeZone.Name || "GMT Standard Time" == dateTimeResultTO.TimeZone.Name || "Coordinated Universal Time" == dateTimeResultTO.TimeZone.Name, "DateTimeResultTO did not get the correct timezone for South Africa, Ireland or Azure Hosted Build Agents. Got: " + dateTimeResultTO.TimeZone.Name);
            Assert.IsTrue("(UTC+02:00) Harare, Pretoria" == dateTimeResultTO.TimeZone.LongName || "(UTC+00:00) Dublin, Edinburgh, Lisbon, London" == dateTimeResultTO.TimeZone.LongName || "(UTC) Coordinated Universal Time" == dateTimeResultTO.TimeZone.LongName, "DateTimeResultTO did not get the correct long format timezone for South Africa, Ireland or Generic UTC. Got: " + dateTimeResultTO.TimeZone.LongName);
            Assert.IsTrue("South Africa Standard Time" == dateTimeResultTO.TimeZone.ShortName || "GMT Standard Time" == dateTimeResultTO.TimeZone.ShortName || "Coordinated Universal Time" == dateTimeResultTO.TimeZone.ShortName, "DateTimeResultTO did not get the correct short format timezone for South Africa, Ireland or Generic UTC. Got: " + dateTimeResultTO.TimeZone.ShortName);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeHours_Is24HisTrue_HoursGreaterThan12()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Is24H = true,
                Hours = 16
            };
            dateTimeResultTO.NormalizeHours();
            Assert.AreEqual(DateTimeAmPm.pm, dateTimeResultTO.AmPm);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeHours_Is24HisFalse_HoursGreaterThan12()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Is24H = false,
                Hours = 16
            };
            dateTimeResultTO.NormalizeHours();
            Assert.AreEqual(DateTimeAmPm.pm, dateTimeResultTO.AmPm);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeHours_AmPm_IsEqual_pm()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Hours = 6,
                AmPm = DateTimeAmPm.pm
            };
            dateTimeResultTO.NormalizeHours();
            Assert.AreEqual(18, dateTimeResultTO.Hours);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_NormalizeHours_AmPm_IsEqual_12am()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Hours = 12,
                AmPm = DateTimeAmPm.am
            };
            dateTimeResultTO.NormalizeHours();
            Assert.AreEqual(0, dateTimeResultTO.Hours);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Everything_Equal_0()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Years = 0,
                Months = 0,
                Days = 0,
                Hours = 0,
                Seconds = 0,
                Milliseconds = 0,
                DaysOfYear = 0,
                Weeks = 0
            };

            dateTimeResultTO.ToDateTime();

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
            var dateTimeResultTO = new DateTimeResultTO
            {
                Years = 0,
                Months = 0,
                DaysOfWeek = 3
            };

            dateTimeResultTO.ToDateTime();

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(1, dateTimeResultTO.Months);
            Assert.AreEqual(3, dateTimeResultTO.Days);
            
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Months_Equal_0_DaysOfYear_NotEqual_0()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Years = 0,
                Months = 0,
                DaysOfYear = 20
            };
            dateTimeResultTO.ToDateTime();

            Assert.AreEqual(1, dateTimeResultTO.Years);
            Assert.AreEqual(1, dateTimeResultTO.Months);
            Assert.AreEqual(20, dateTimeResultTO.Days);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DateTimeResultTO))]
        public void DateTimeResultTO_SetProperties_Months_Equal_0_Weeks_NotEqual_0()
        {
            var dateTimeResultTO = new DateTimeResultTO
            {
                Years = 0,
                Months = 0,
                Weeks = 20
            };
            dateTimeResultTO.ToDateTime();
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
