
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class MultipleBoolToEnabledConverterTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MultipleBoolToEnabledConverter_Convert")]
        public void MultipleBoolToEnabledConverter_Convert_WithTrueTrueFalse_ReturnsFalse()
        {
            MultipleBoolToEnabledConverter multipleBoolToEnabledConverter = new MultipleBoolToEnabledConverter();
            object[] values = { true, true, false };
            var actual = multipleBoolToEnabledConverter.Convert(values, null, null, null);

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MultipleBoolToEnabledConverter_Convert")]
        public void MultipleBoolToEnabledConverter_Convert_WithTrueTrueTrue_ReturnsTrue()
        {
            MultipleBoolToEnabledConverter multipleBoolToEnabledConverter = new MultipleBoolToEnabledConverter();
            object[] values = { true, true, true };
            var actual = multipleBoolToEnabledConverter.Convert(values, null, null, null);

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MultipleBoolToEnabledConverter_Convert")]
        public void MultipleBoolToEnabledConverter_Convert_WithTrueTrueNull_ReturnsTrue()
        {
            MultipleBoolToEnabledConverter multipleBoolToEnabledConverter = new MultipleBoolToEnabledConverter();
            object[] values = { true, true, null };
            var actual = multipleBoolToEnabledConverter.Convert(values, null, null, null);

            Assert.AreEqual(true, actual);
        }
    }
}
