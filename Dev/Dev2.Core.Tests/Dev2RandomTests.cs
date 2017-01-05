/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2RandomTests
    {
        readonly Dev2Random _dev2Random = new Dev2Random();
        readonly Regex _lettersRegex = new Regex(@"[a-z]*");
        readonly Regex _mixedRegex = new Regex(@"[a-z\d]*");

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        //[ClassInitialize]
        //public static void ClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Successfully Generate Random Strings

        //Moved to working test because of BUG 9506
        [TestMethod]
        public void GenerateNumsWithFromIsGreaterThanToExpectedValidNumber()
        {
            int res;
            int.TryParse(_dev2Random.GetRandom(enRandomType.Numbers, -1, 100, 5), out res);
            Assert.IsTrue(res >= 5 && res <= 100, "Did not generate the right random number.");
        }

        [TestMethod]
        public void GenerateNumsWithFromIsGreaterThanToExpectedValidNumberForZeroFrom()
        {
            int res;
            int.TryParse(_dev2Random.GetRandom(enRandomType.Numbers, -1, 0, 5), out res);
            Assert.IsTrue(res >= 0 && res <= 5, "Did not generate the right random number.");
        }

        [TestMethod]
        public void GenerateWithGuidExpectedValidGuid()
        {
            var result = _dev2Random.GetRandom(enRandomType.Guid, -1, -1, -1);
            Guid tryParse;
            Assert.IsTrue(Guid.TryParse(result, out tryParse), "Did not generate a valid guid");
            Assert.AreNotEqual(Guid.Empty, tryParse, "Generated an empty Guid");
        }

        [TestMethod]
        public void GenerateNumsWithFromIsGreaterThanToExpectedValidNumbersWithDecimalRange()
        {
            double res;
            double.TryParse(_dev2Random.GetRandom(enRandomType.Numbers, -1, 0.01, 0.1), out res);
            Assert.IsTrue(res >= 0.01 && res <= 0.1, "Did not generate the right random number.");
        }

        [TestMethod]
        public void GenerateWithLettersExpectedLettersGenerated()
        {
            var result = _dev2Random.GetRandom(enRandomType.Letters, 5000, -1, -1);
            Assert.AreEqual(result.Length, 5000, "Dev2Random generated letters to the incorrect length");
            Assert.AreEqual(2, _lettersRegex.Matches(result).Count, "_dev2Random generated letters outside the specified range");
            Assert.IsTrue(result.Contains('a'), "Dev2Random did not generate an 'a' in 5000 letters");
            Assert.IsTrue(result.Contains('z'), "Dev2Random did not generate a 'z' in 5000 letters");
        }

        [TestMethod]
        public void GenerateWithNumbersExpectedNumbersGeneratedForRangeLargerThanIntegerForPositiveCase()
        {
            const double MoreThanMaxInt = (double)int.MaxValue + 1;
            const double LessThanMaxDouble = double.MaxValue - 1;
            double result = double.Parse(_dev2Random.GetRandom(enRandomType.Numbers, -1, MoreThanMaxInt, LessThanMaxDouble));
            Assert.IsTrue(result <= LessThanMaxDouble, "Dev2Random generated a number above the specified range");
            Assert.IsTrue(result >= MoreThanMaxInt, "Dev2Random generated a number below the specified range");
        }

        [TestMethod]
        public void GenerateWithNumbersExpectedNumbersGeneratedForRangeLargerThanIntegerForNegativeCase()
        {
            const double LessThanMinInt = (double)int.MinValue - 1;
            const double LessThanMaxDouble = double.MaxValue - 1;
            double result = double.Parse(_dev2Random.GetRandom(enRandomType.Numbers, -1, LessThanMinInt, LessThanMaxDouble));
            Assert.IsTrue(result <= LessThanMaxDouble, "Dev2Random generated a number above the specified range");
            Assert.IsTrue(result >= LessThanMinInt, "Dev2Random generated a number below the specified range");
        }

        //http://www.hanselman.com/blog/WhyYouCantDoubleParseDoubleMaxValueToStringOrSystemOverloadExceptionsWhenUsingDoubleParse.aspx
        [TestMethod]
        public void GenerateWithNumbersExpectedNumbersMaximumDoubleRange()
        {
            const double MinDouble = double.MinValue;
            const double MaxDouble = double.MaxValue;
            var random = _dev2Random.GetRandom(enRandomType.Numbers, -1, MinDouble, MaxDouble);
            if(random.ToString(CultureInfo.InvariantCulture) == "1.79769313486232E+308")
            {
                random = "1.79769313486231E+308";
            }
            if (random.ToString(CultureInfo.InvariantCulture) == "-1.79769313486232E+308")
            {
                random = "-1.79769313486231E+308";
            }
           
            var result = Convert.ToDouble(random);
            Assert.IsTrue(result <= MaxDouble, "Dev2Random generated a number above the specified range");
            Assert.IsTrue(result >= MinDouble, "Dev2Random generated a number below the specified range");
        }


        [TestMethod]
        public void GenerateWithNumbersExpectedNumbersMaximumDoubleRangeNoDecimals()
        {
            const double MinDouble = 0d;
            const double MaxDouble = double.MaxValue;
            double result = double.Parse(_dev2Random.GetRandom(enRandomType.Numbers, -1, MinDouble, MaxDouble));
            Assert.IsTrue(result <= MaxDouble, "Dev2Random generated a number above the specified range");
            Assert.IsTrue(result >= MinDouble, "Dev2Random generated a number below the specified range");
        }


        [TestMethod]
        public void GenerateWithNumbersExpectedNumbersGenerated()
        {
            var result = _dev2Random.GetRandom(enRandomType.Numbers, -1, 0, 5000);
            Assert.IsTrue(int.Parse(result) <= 5000, "Dev2Random generated a number above the specified range");
            Assert.IsTrue(int.Parse(result) >= 0, "Dev2Random generated a number below the specified range");
        }

        [TestMethod]
        public void GenerateWithLettersAndNumbersExpectedLettersAndNumbersGenerated()
        {
            var result = _dev2Random.GetRandom(enRandomType.LetterAndNumbers, 5000, 0, 26);
            Assert.AreEqual(5000, result.Length, "Dev2Random generated letters and numbers of an incorrect length");
            Assert.AreEqual(2, _mixedRegex.Matches(result).Count, "Dev2Random generated letters and numbers outside the specified range");
        }

        #endregion

        #region Fail to Generate Random Strings

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateCharsWithNegativeRangeExpectedArgException()
        {
            _dev2Random.GetRandom(enRandomType.Letters, -5, -1, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateMixedWithNegativeExpectedArgException()
        {
            _dev2Random.GetRandom(enRandomType.LetterAndNumbers, -5, -1, -1);
        }

        #endregion

        #region Test Randomness (going for 60% uniqueness)

        [TestMethod]
        public void GenerateWithOneLetter10TimesExpectedDifferentLettersEachTime()
        {
            //Initialize
            var results = new List<string>();

            //Get 10 random letters
            for (var i = 0; i < 10; i++)
            {
                results.Add(_dev2Random.GetRandom(enRandomType.Letters, 1, -1, -1));
            }

            //Count duplicates
            var countDuplicates = results.GroupBy(group => group)
                                         .Select(item => new { Value = item.Key, Count = item.Count() });

            //Assert no item is duplicated more than twice
            foreach (var elem in countDuplicates)
            {
                Assert.IsTrue(elem.Count < 5, elem.Count + " duplicate letters where generated by Dev2Random class (out of 10)");
            }
        }

        [TestMethod]
        public void GenerateWithNumber10TimesExpectedDifferentNumbersEachTime()
        {
            //Initialize
            var results = new List<string>();

            //Get 10 random letters
            for (var i = 0; i < 10; i++)
            {
                results.Add(_dev2Random.GetRandom(enRandomType.Numbers, -1, 0, 10));
            }

            //Count duplicates
            var countDuplicates = results.GroupBy(group => group)
                                         .Select(item => new { Value = item.Key, Count = item.Count() });

            //Assert no item is duplicated more than twice
            foreach (var elem in countDuplicates)
            {
                Assert.IsTrue(elem.Count < 7, elem.Count + " duplicate numbers where generated by Dev2Random class (out of 10)");
            }
        }

        [TestMethod]
        public void GenerateWithOneLetterOrNumber10TimesExpectedDifferentNumberAndLettersEachTime()
        {
            //Initialize
            var results = new List<string>();

            //Get 10 random letters
            for (var i = 0; i < 10; i++)
            {
                results.Add(_dev2Random.GetRandom(enRandomType.LetterAndNumbers, 1, -1, -1));
            }

            //Count duplicates
            var countDuplicates = results.GroupBy(group => group)
                                         .Select(item => new { Value = item.Key, Count = item.Count() });

            //Assert no item is duplicated more than twice
            foreach (var elem in countDuplicates)
            {
                Assert.IsTrue(elem.Count <= 5, elem.Count + " duplicate letters and numbers where generated by Dev2Random class (out of 10)");
            }
        }

        #endregion
    }
}
