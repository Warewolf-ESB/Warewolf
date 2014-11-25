
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Dev2.Studio.Core.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IntInRangeToVisibilityConverterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_ConvertBack")]
        [ExpectedException(typeof(NotImplementedException))]
        public void IntInRangeToVisibilityConverter_ConvertBack_ThrowsNotImplementedException()
        {
            //------------Setup for test--------------------------
            var converter = new IntInRangeToVisibilityConverter();

            //------------Execute Test---------------------------
            var result = converter.ConvertBack(null, new Type[] { typeof(int) }, null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_NullValues_Visible()
        {
            Verify_Convert(null, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_2OrLessValues_Visible()
        {
            Verify_Convert(new object[2], Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_MinEqualsMax_Visible()
        {
            const int Min = 2;
            const int Value = 3;
            const int Max = 2;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_ValueEqualToMinAndLessThanMax_Visible()
        {
            const int Min = 2;
            const int Value = 2;
            const int Max = 5;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_ValueGreaterThanMinAndLessThanMax_Visible()
        {
            const int Min = 3;
            const int Value = 4;
            const int Max = 5;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_ValueGreaterThanMinAndEqualToMax_Visible()
        {
            const int Min = 2;
            const int Value = 5;
            const int Max = 5;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_ValueLessThanMin_Collapsed()
        {
            const int Min = 3;
            const int Value = 2;
            const int Max = 5;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IntInRangeToVisibilityConverter_Convert")]
        public void IntInRangeToVisibilityConverter_Convert_GreaterThanMax_Collapsed()
        {
            const int Min = 3;
            const int Value = 6;
            const int Max = 5;
            Verify_Convert(new object[] { Value, Min, Max }, Visibility.Collapsed);
        }

        void Verify_Convert(object[] values, Visibility expectedVisibility)
        {
            //------------Setup for test--------------------------
            var converter = new IntInRangeToVisibilityConverter();

            //------------Execute Test---------------------------
            var result = converter.Convert(values, typeof(int), null, CultureInfo.CurrentCulture);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedVisibility, result);
        }
    }
}
