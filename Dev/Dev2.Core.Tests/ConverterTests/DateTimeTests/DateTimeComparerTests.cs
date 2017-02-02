/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.ConverterTests.DateTimeTests
{
    /// <summary>
    /// Summary description for DateTimeComparerTests
    /// </summary>
    [TestClass]
    public class DateTimeComparerTests
    {
        #region Fields

        const string Input1 = "2011/06/05 08:20:30:123 AM";
        string _input2 = "2012/06/05 08:20:30:123 AM";
        const string InputFormat = "yyyy/mm/dd 12h:min:ss:sp am/pm";
        string _outputType = "";

        #endregion Fields

        #region TestContext

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion TestContext

        #region Comparer Tests

        #region Years Tests

        [TestMethod]
        public void TryCompare_Years_Negative_Years_Expected_NegativeOne_Years()
        {
            string result;
            string error;
            _input2 = "2010/06/05 08:20:30:124 AM";    
            _outputType = "Years";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }
     
        [TestMethod]
        public void TryCompare_Years_Equal_Expected_One_Years()
        {
            string result;
            string error;
            _outputType = "Years";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }
        
        [TestMethod]
        public void TryCompare_Years_One_Short_Expected_Zero_Years()
        {
            string result;
            string error;
            _input2 = "2012/06/05 08:20:30:122 AM";     
            _outputType = "Years";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }
        
        [TestMethod]
        public void TryCompare_Years_One_Over_Expected_One_Years()
        {
            string result;
            string error;
            _input2 = "2012/06/05 08:20:30:124 AM";
            _outputType = "Years";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Years Tests

        #region Months Tests

        [TestMethod]
        public void TryCompare_Months_Negative_Expected_NegativeOne_Months()
        {
            string result;
            string error;
            _input2 = "2011/05/05 08:20:30:123 AM";
            _outputType = "Months";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }
        
        [TestMethod]
        public void TryCompare_Months_Equal_Expected_One_Months()
        {
            string result;
            string error;        
            _input2 = "2011/07/05 08:20:30:123 AM";
            _outputType = "Months";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }
        
        [TestMethod]
        public void TryCompare_Months_One_Short_Expected_Zero_Months()
        {
            string result;
            string error;   
            _input2 = "2011/07/05 08:20:30:122 AM";           
            _outputType = "Months";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }

        
        [TestMethod]
        public void TryCompare_Months_One_Over_Expected_One_Months()
        {
            string result;
            string error;
            _input2 = "2011/07/05 08:20:30:124 AM";           
            _outputType = "Months";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Months Tests

        #region Days Tests

        [TestMethod]
        public void TryCompare_Days_Negative_Expected_NegativeOne_Days()
        {
            string result;
            string error;
            _input2 = "2011/06/04 08:20:30:123 AM";
            _outputType = "Days";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }

        [TestMethod]
        public void TryCompare_Days_Equal_Expected_One_Days()
        {
            string result;
            string error;
            _input2 = "2011/06/06 08:20:30:123 AM";
            _outputType = "Days";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        [TestMethod]
        public void TryCompare_Days_One_Short_Expected_Zero_Days()
        {
            string result;
            string error;
            _input2 = "2011/06/06 08:20:30:122 AM";
            _outputType = "Days";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }


        [TestMethod]
        public void TryCompare_Days_One_Over_Expected_One_Days()
        {
            string result;
            string error;
            _input2 = "2011/06/06 08:20:30:124 AM";
            _outputType = "Days";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Days Tests

        #region Weeks Tests

        [TestMethod]
        public void TryCompare_Weeks_Negative_Expected_NegativeOne_Weeks()
        {
            string result;
            string error;
            _input2 = "2011/05/28 08:20:30:123 AM";
            _outputType = "Weeks";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.AreEqual("-2",result);
        }

        [TestMethod]
        public void TryCompare_Weeks_Equal_Expected_One_Weeks()
        {
            string result;
            string error;
            _input2 = "2011/06/12 08:20:30:123 AM";
            _outputType = "Weeks";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        [TestMethod]
        public void TryCompare_Weeks_One_Short_Expected_Zero_Weeks()
        {
            string result;
            string error;
            _input2 = "2011/06/12 08:20:30:122 AM";
            _outputType = "Weeks";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }


        [TestMethod]
        public void TryCompare_Weeks_One_Over_Expected_One_Weeks()
        {
            string result;
            string error;
            _input2 = "2011/06/12 08:20:30:124 AM";
            _outputType = "Weeks";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Weeks Tests

        #region Hours Tests

        [TestMethod]
        public void TryCompare_Hours_Negative_Expected_NegativeOne_Hours()
        {
            string result;
            string error;
            _input2 = "2011/06/05 07:20:30:123 AM";
            _outputType = "Hours";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }

        [TestMethod]
        public void TryCompare_Hours_Equal_Expected_One_Hours()
        {
            string result;
            string error;
            _input2 = "2011/06/05 09:20:30:123 AM";
            _outputType = "Hours";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        [TestMethod]
        public void TryCompare_Hours_One_Short_Expected_Zero_Hours()
        {
            string result;
            string error;
            _input2 = "2011/06/05 09:20:30:122 AM";
            _outputType = "Hours";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }


        [TestMethod]
        public void TryCompare_Hours_One_Over_Expected_One_Hours()
        {
            string result;
            string error;
            _input2 = "2011/06/05 09:20:30:124 AM";
            _outputType = "Hours";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Hours Tests

        #region Minutes Tests

        [TestMethod]
        public void TryCompare_Minutes_Negative_Expected_NegativeOne_Minutes()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:19:30:123 AM";
            _outputType = "Minutes";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }

        [TestMethod]
        public void TryCompare_Minutes_Equal_Expected_One_Minutes()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:21:30:123 AM";
            _outputType = "Minutes";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        [TestMethod]
        public void TryCompare_Minutes_One_Short_Expected_Zero_Minutes()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:21:30:122 AM";
            _outputType = "Minutes";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }


        [TestMethod]
        public void TryCompare_Minutes_One_Over_Expected_One_Minutes()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:21:30:124 AM";
            _outputType = "Minutes";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Minutes Tests

        #region Seconds Tests

        [TestMethod]
        public void TryCompare_Seconds_Negative_Expected_NegativeOne_Seconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:29:123 AM";
            _outputType = "Seconds";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }

        [TestMethod]
        public void TryCompare_Seconds_Equal_Expected_One_Seconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:31:123 AM";
            _outputType = "Seconds";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        [TestMethod]
        public void TryCompare_Seconds_One_Short_Expected_Zero_Seconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:31:122 AM";
            _outputType = "Seconds";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }


        [TestMethod]
        public void TryCompare_Seconds_One_Over_Expected_One_Seconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:31:124 AM";
            _outputType = "Seconds";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "1");
        }

        #endregion Seconds Tests

        #region SplitSeconds Tests

        [TestMethod]
        public void TryCompare_SplitSeconds_Equal_Expected_Zero_SplitSeconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:30:123 AM";
            _outputType = "Split Secs";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "0");
        }

        [TestMethod]
        public void TryCompare_SplitSeconds_One_Short_Expected_Zero_SplitSeconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:30:122 AM";
            _outputType = "Split Secs";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.IsTrue(result == "-1");
        }


        [TestMethod]
        public void TryCompare_SplitSeconds_One_Over_Expected_One_SplitSeconds()
        {
            string result;
            string error;
            _input2 = "2011/06/05 08:20:30:124 AM";
            _outputType = "Split Secs";
            IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
            IDateTimeDiffTO dateTimeResult = DateTimeConverterFactory.CreateDateTimeDiffTO(Input1, _input2, InputFormat, _outputType);
            comparer.TryCompare(dateTimeResult, out result, out error);
            Assert.AreEqual("1", result);
        }

        #endregion SplitSeconds Tests

        #endregion Comparer Tests
    }
}
