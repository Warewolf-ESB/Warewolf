
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
using System.Windows;
using System.Windows.Data;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MessageBoxButtonToVisibilityConverterTests
    {
        #region Tests

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackExpectedNotImplementedException()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            messageBoxButtonToVisibilityConverter.ConvertBack(null, null, null, null);
        }

        [TestMethod]
        public void ConvertWhereValueIsNullExpectedNothing()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(null, null, MessageBoxResult.OK, null);
            object expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the value is null binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereParameterIsNullExpectedNothing()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, null, null);
            object expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the parameter is null binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereValueIsUnexpectedTypeExpectedNothing()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert("", null, MessageBoxResult.OK, null);
            object expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the value is an unexpected type binding.donothing is expected");
        }

        [TestMethod]
        public void ConvertWhereParameterIsUnexpectedTypeExpectedNothing()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, "", null);
            object expected = Binding.DoNothing;

            Assert.AreEqual(expected, actual, "When the parameter is null binding.donothing is expected");
        }

        #region Ok Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsOkCancelAndParameterIsOkTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.OK, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOkAndParameterIsOkTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OK, null, MessageBoxResult.OK, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsOkExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.OK, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Cancel Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsOkCancelAndParameterIsCancelTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsCancelTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsCancelExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.Cancel, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Yes Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsYesTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsYesTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOKCancelAndParameterIsYesExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.Yes, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #region Yes Button Visibility

        [TestMethod]
        public void ConvertWhereValueIsYesNoAndParameterIsNoTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNo, null, MessageBoxResult.No, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsYesNoCancelAndParameterIsNoTypeExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.YesNoCancel, null, MessageBoxResult.No, null);
            object expected = Visibility.Visible;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        [TestMethod]
        public void ConvertWhereValueIsOKCancelAndParameterIsNoExpectedVisible()
        {
            MessageBoxButtonToVisibilityConverter messageBoxButtonToVisibilityConverter = new MessageBoxButtonToVisibilityConverter();
            object actual = messageBoxButtonToVisibilityConverter.Convert(MessageBoxButton.OKCancel, null, MessageBoxResult.No, null);
            object expected = Visibility.Collapsed;

            Assert.AreEqual(expected, actual, "This compination of value and parameter should result in Visibility.Visible.");
        }

        #endregion Ok Button Visibility

        #endregion Tests
    }
}
