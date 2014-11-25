
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EmptyStringToVisibilityConverterTests
    {
        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is empty, expect true when istrueisempty equals true")]
        public void EmptyStringToVisibilityConverter_UnitTest_CollapsedWhenEmptyStringEmpty_ExpectsCollapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Collapsed;
            var actual = (Visibility)converter.Convert(null, typeof(Visibility), null, null);
            const Visibility expected = Visibility.Collapsed;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is null, expect true when istrueisempty equals true")]
        public void EmptyStringToVisibilityConverter_UnitTest_CollapsedWhenEmptyStringNull_ExpectsCollapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Collapsed;
            var actual = (Visibility)converter.Convert(string.Empty, typeof(Visibility), null, null);
            const Visibility expected = Visibility.Collapsed;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is white space, expect true when istrueisempty equals true")]
        public void EmptyStringToVisibilityConverter_UnitTest_CollapsedWhenEmptyStringWhiteSpace_ExpectsCollapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Collapsed;
            var actual = (Visibility)converter.Convert(" ", typeof(Visibility), null, null);
            const Visibility expected = Visibility.Collapsed;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is not empty, null or whitespace, expect false when istrueisempty equals true")]
        public void EmptyStringToVisibilityConverter_UnitTest_CollapsedWhenEmptyStringValue_ExpectsVisible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Collapsed;
            var actual = (Visibility)converter.Convert("Anything", typeof(Visibility), null, null);
            const Visibility expected = Visibility.Visible;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is empty, expect dalse when istrueisempty equals falsee")]
        public void EmptyStringToVisibilityConverter_UnitTest_VisibleWhenEmptyStringEmpty_ExpectsVisible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Visible;
            var actual = (Visibility)converter.Convert(null, typeof(Visibility), null, null);
            const Visibility expected = Visibility.Visible;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is null, expect false when istrueisempty equals false")]
        public void
            EmptyStringToVisibilityConverter_UnitTest_VisibleWhenEmptyStringNull_ExpectsVisible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Visible;
            var actual = (Visibility)converter.Convert(string.Empty, typeof(Visibility), null, null);
            const Visibility expected = Visibility.Visible;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is white space, expect false when istrueisempty equals false")]
        public void EmptyStringToVisibilityConverter_UnitTest_VisibleWhenEmptyStringWhiteSpace_ExpectsVisible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Visible;
            var actual = (Visibility)converter.Convert(" ", typeof(Visibility), null, null);
            const Visibility expected = Visibility.Visible;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToVisibilityConverter")]
        [Description("When a string is not empty, null or whitespace, expect true when istrueisempty equals false")]
        public void EmptyStringToVisibilityConverter_UnitTest_VisibleWhenEmptyStringValue_ExpectsCollapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            converter.EmptyStringVisiblity = Visibility.Visible;
            var actual = (Visibility)converter.Convert("Anything", typeof(Visibility), null, null);
            const Visibility expected = Visibility.Collapsed;
            Assert.AreEqual(expected, actual);
        }
    }
}
