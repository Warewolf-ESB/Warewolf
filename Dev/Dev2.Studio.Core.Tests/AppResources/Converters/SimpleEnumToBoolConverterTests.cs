/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Globalization;
using System.Windows.Data;
using Dev2.AppResources.Converters;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
	[TestCategory("Studio Resources Core")]
    public class SimpleEnumToBoolConverterTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SimpleEnumToBoolConverter))]
        public void SimpleEnumToBoolConverter_Convert_GivenMatchingEnumValue_True()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (bool)simpleEnumToBoolConvert.Convert(LogLevel.DEBUG, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.IsTrue(convert);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SimpleEnumToBoolConverter))]
        public void SimpleEnumToBoolConverter_Convert_GivenNotMatchingEnumValue_False()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (bool)simpleEnumToBoolConvert.Convert(LogLevel.FATAL, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.IsFalse(convert);
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SimpleEnumToBoolConverter))]
        public void SimpleEnumToBoolConverter_ConvertBack_GivenMatchingEnumValue_ReturnEnum()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = (LogLevel)simpleEnumToBoolConvert.ConvertBack(true, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.DEBUG,convert);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SimpleEnumToBoolConverter))]
        public void SimpleEnumToBoolConverter_ConvertBack_GivenNotMatchingEnumValue_ReturnBindingNothing()
        {
            //------------Setup for test--------------------------
            var simpleEnumToBoolConvert = new SimpleEnumToBoolConverter();
            
            //------------Execute Test---------------------------
            var convert = simpleEnumToBoolConvert.ConvertBack(false, typeof(bool), LogLevel.DEBUG, CultureInfo.InvariantCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(Binding.DoNothing,convert);
        }
    }
}
