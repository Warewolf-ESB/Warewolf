/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2NumberFormatterTests
    {
        #region Class Members

        #endregion Class Members

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion Properties

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        #endregion

        #region Test Methods

        #region Error Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Format_Where_FormatNumberTOIsNull_Expected_ArgumentNullExcpetion()
        {
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_NumberStringIsntANumber_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("ABC", enRoundingType.None, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_NumberStringWithSpecialChars_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("!@$%*&*(^(^))((.{}:?<>", enRoundingType.None, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_RoundingDecimalPlacesGreaterThan14_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("123.123", enRoundingType.None, 15, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_RoundingDecimalPlacesLessThanNegative14_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("123.123", enRoundingType.None, -15, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_DecimalPlacesToShowGreaterThan14_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("123.123", enRoundingType.None, 0, true, 15);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Format_Where_DecimalPlacesToShowGreaterThanNegative14_Expected_InvalidOperation()
        {
            var formatNumberTO = new FormatNumberTO("123.123", enRoundingType.None, 0, true, -15);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            dev2NumberFormatter.Format(formatNumberTO);
        }

        #endregion Error Tests

        #region No Opertation Tests
        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingTypeIsNoneAndAdjustDecimalsIsFalse_Expected_RawNumberBack()
        {
            var formatNumberTO = new FormatNumberTO("123.123456", enRoundingType.None, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            actual = "'" + actual + "'";

            Assert.AreEqual(formatNumberTO.Number, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingTypeIsNoneAndAdjustDecimalsIsFalse_Expected_RawNumberBack()
        {
            var formatNumberTO = new FormatNumberTO("-123.123456", enRoundingType.None, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            actual = "'" + actual + "'";

            Assert.AreEqual(formatNumberTO.Number, actual);
        }

        #endregion No Opertation Tests

        #region Normal Rounding Tests

        [TestMethod]
        public void Format_Given_PositiveNumberWithDecimalsThatShouldRoundDown_Where_RoundingTypeIsNormalWithDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("123.12345", enRoundingType.Normal, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithDecimalsAndNoneRoundingType_ShouldNotRoundDown()
        {
            var formatNumberTO = new FormatNumberTO();
            Assert.IsNotNull(formatNumberTO);
            formatNumberTO = new FormatNumberTO("123.12345", "None", 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.12345d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithDecimalsThatShouldRoundDown_Where_RoundingTypeIsNormalWithDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345", enRoundingType.Normal, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberWithDecimalsThatShouldRoundUp_Where_RoundingTypeIsNormalWithDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("123.12645", enRoundingType.Normal, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.13d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithDecimalsThatShouldRoundUp_Where_RoundingTypeIsNormalWithDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("-123.12645", enRoundingType.Normal, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.13d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberWithDecimalsThatShouldRoundDown_Where_RoundingTypeIsNormalWithNoDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("123.12345", enRoundingType.Normal, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithDecimalsThatShouldRoundDown_Where_RoundingTypeIsNormalWithNoDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345", enRoundingType.Normal, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberWithDecimalsThatShouldRoundUp_Where_RoundingTypeIsNormalWithNoDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("123.92345", enRoundingType.Normal, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 124d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithDecimalsThatShouldRoundUp_Where_RoundingTypeIsNormalWithNoDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("123.92345", enRoundingType.Normal, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 124;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingTypeIsNormal_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Normal, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        #endregion Normal Rounding Tests

        #region Up Rounding tests

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingTypeIsUpWithDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("123.12345", enRoundingType.Up, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.13d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingTypeIsUpWithDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345", enRoundingType.Up, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.13d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingTypeIsUpWithNoDecimalPlaces_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("123.92345", enRoundingType.Up, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 124d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingTypeIsUp_Expected_RoundedUp()
        {
            var formatNumberTO = new FormatNumberTO("-123.92345", enRoundingType.Up, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -124d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingTypeIsUpWithNoDecimalPlaces_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Up, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        #endregion Up Rounding tests

        #region Down Rounding tests

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingTypeIsDownWithDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("123.12345", enRoundingType.Down, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingTypeIsDownWithDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345", enRoundingType.Down, 2, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingTypeIsDownWithNoDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("123.92345", enRoundingType.Down, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingTypeIsDownWithNoDecimalPlaces_Expected_RoundedDown()
        {
            var formatNumberTO = new FormatNumberTO("-123.92345", enRoundingType.Down, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingTypeIsDown_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Down, 0, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        #endregion Down Rounding tests

        #region Negative Rounding

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingDecimalPlacesAreNegative_Expected_RoundingToMultiplesOf10ForEveryNegative()
        {
            var formatNumberTO = new FormatNumberTO("123.12345", enRoundingType.Normal, -1, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 120d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingDecimalPlacesAreNegative_Expected_RoundingToMultiplesOf10ForEveryNegative()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345", enRoundingType.Normal, -1, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -120d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingDecimalPlacesAreNegative_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Normal, -1, false, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        #endregion Negative Rounding

        #region Adjust Decimal Places Tests

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_AdjustDecimalPlacesBy0_Expected_DecimalsAreDropped()
        {
            var formatNumberTO = new FormatNumberTO("123.12345678911235", enRoundingType.None, 0, true, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_AdjustDecimalPlacesBy0_Expected_DecimalsAreDropped()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345678911235", enRoundingType.None, 0, true, 0);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberWithMoreThan2DecimalPlaces_Where_AdjustDecimalPlacesBy2_Expected_2DecimalPlaces()
        {
            var formatNumberTO = new FormatNumberTO("123.12345678911235", enRoundingType.None, 0, true, 2);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberNumberWithMoreThan2DecimalPlaces_Where_AdjustDecimalPlacesBy2_Expected_2DecimalPlaces()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345678911235", enRoundingType.None, 0, true, 2);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberWithLessThanThan5DecimalPlaces_Where_AdjustDecimalPlacesBy5_Expected_2DecimalPlaces()
        {
            var formatNumberTO = new FormatNumberTO("123.123", enRoundingType.None, 0, true, 5);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.12300d;
            var expected = expectedDouble.ToString("###.###00");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberWithLessThanThan5DecimalPlaces_Where_AdjustDecimalPlacesBy5_Expected_2DecimalPlaces()
        {
            var formatNumberTO = new FormatNumberTO("-123.123", enRoundingType.None, 0, true, 5);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.12300d;
            var expected = expectedDouble.ToString("###.###00");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_AdjustDecimalPlacesByNegative1_Expected_AnyValuBelow10IsDropped()
        {
            var formatNumberTO = new FormatNumberTO("123.12345678911235", enRoundingType.None, 0, true, -1);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);


            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_AdjustDecimalPlacesByNegative1_Expected_AnyValuAboveNagative10IsDropped()
        {
            var formatNumberTO = new FormatNumberTO("-123.12345678911235", enRoundingType.None, 0, true, -1);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -12d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumberLessThan1000_Where_AdjustDecimalPlacesByNegative3_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("1.12345678911235", enRoundingType.None, 0, true, -3);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumberGreaterThanNegative1000_Where_AdjustDecimalPlacesByNegative3_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("-1.12345678911235", enRoundingType.None, 0, true, -3);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        #endregion Adjust Decimal Places Tests

        #region Round And Adjust Decimal Tests

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingDecimalPlacesAreMoreThanWhatIsBeingAdjusted_Expected_RoundingHappensThenDecimalsAreDropped()
        {
            var formatNumberTO = new FormatNumberTO("123.12395678911235", enRoundingType.Normal, 4, true, 3);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.124d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingDecimalPlacesAreMoreThanWhatIsBeingAdjusted_Expected_RoundingHappensThenDecimalsAreDropped()
        {
            var formatNumberTO = new FormatNumberTO("-123.12395678911235", enRoundingType.Normal, 4, true, 3);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.124d;
            var expected = expectedDouble.ToString(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingDecimalPlacesAreMoreThanWhatIsBeingAdjusted_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Normal, 4, true, 3);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString("0.000");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_PositiveNumber_Where_RoundingDecimalPlacesAreLessThanWhatIsBeingAdjusted_Expected_RoundingHappensThenDecimalsAreAdded()
        {
            var formatNumberTO = new FormatNumberTO("123.12395678911235", enRoundingType.Normal, 3, true, 4);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 123.124d;
            var expected = expectedDouble.ToString("###.###0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_NegativeNumber_Where_RoundingDecimalPlacesArelessThanWhatIsBeingAdjusted_Expected_RoundingHappensThenDecimalsAreAdded()
        {
            var formatNumberTO = new FormatNumberTO("-123.12395678911235", enRoundingType.Normal, 3, true, 4);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = -123.124d;
            var expected = expectedDouble.ToString("###.###0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Format_Given_0_Where_RoundingDecimalPlacesAreLessThanWhatIsBeingAdjusted_Expected_0()
        {
            var formatNumberTO = new FormatNumberTO("0", enRoundingType.Normal, 3, true, 4);
            var dev2NumberFormatter = new Dev2NumberFormatter();
            var actual = dev2NumberFormatter.Format(formatNumberTO);
            const double expectedDouble = 0d;
            var expected = expectedDouble.ToString("0.0000");

            Assert.AreEqual(expected, actual);
        }

        #endregion Round And Adjust Decimal Tests

        #endregion Test Methods
    }
}
