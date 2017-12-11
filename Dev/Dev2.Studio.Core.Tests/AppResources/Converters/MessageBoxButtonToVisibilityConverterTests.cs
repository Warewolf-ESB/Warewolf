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
using System.Windows;
using System.Windows.Data;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class MessageBoxButtonToVisibilityConverterTests
    {
        #region Tests

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackExpectedNotImplementedException()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            messageBoxButtonToVisibilityConverter.ConvertBack(null, null, null, null);
        }

        [TestMethod]
        public void ConvertWhereValueIsNullExpectedNothing()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(null, null, MessageBoxResult.OK, null);
            var expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the value is null binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereParameterIsNullExpectedNothing()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, null, null);
            var expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the parameter is null binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereValueIsUnexpectedTypeExpectedNothing()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert("", null, MessageBoxResult.OK, null);
            var expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the value is an unexpected type binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereParameterIsUnexpectedTypeExpectedNothing()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, "", null);
            var expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the parameter is null binding.donothing is expected");
        }

        #region Ok Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsOkCancelAndParameterIsOkTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.OK, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOkAndParameterIsOkTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, MessageBoxResult.OK, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsOkExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.OK, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Cancel Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsOkCancelAndParameterIsCancelTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsCancelTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsCancelExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Yes Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsYesTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsYesTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOKCancelAndParameterIsYesExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Yes Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsNoTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.No, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsNoTypeExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.No, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOKCancelAndParameterIsNoExpectedVisible()
        {
            var messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            var actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.No, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #endregion Tests
    }
}
