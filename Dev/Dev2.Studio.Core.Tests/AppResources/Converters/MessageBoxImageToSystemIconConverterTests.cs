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
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class MessageBoxImageToSystemIconConverterTests
    {
        #region Tests

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackExpectedNotImplementedException()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            messageBoxImageToSystemIconConverter.ConvertBack(null, null, null, null);
        }

        [TestMethod]
        public void ConvertNullExpectedEmptyBitmap()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            var actual = messageBoxImageToSystemIconConverter.Convert(null, null, null, null) as BitmapSource;

            var actualColor = GetPixel(actual);
            var expectedColor = Color.FromArgb(0, 0, 0, 0);

            if (actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertNoneExpectedEmptyBitmap()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.None, null, null, null) as BitmapSource;

            var actualColor = GetPixel(actual);
            var expectedColor = Color.FromArgb(0, 0, 0, 0);

            if (actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertInvalidValueExpectedEmptyBitmap()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            var actual = messageBoxImageToSystemIconConverter.Convert("", null, null, null) as BitmapSource;

            var actualColor = GetPixel(actual);
            var expectedColor = Color.FromArgb(0, 0, 0, 0);

            if (actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertAsteriskExpectedSystemAsterisk()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Asterisk, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png", actual);
        }

        [TestMethod]
        public void ConvertErrorExpectedSystemError()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Error, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertExclamationExpectedSystemExclamation()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Exclamation, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png", actual);
        }

        [TestMethod]
        public void ConvertHandExpectedSystemHand()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Hand, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertInformationExpectedSystemInformation()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Information, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png", actual);
        }

        [TestMethod]
        public void ConvertQuestionExpectedSystemQuestion()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Question, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/GenericHelp-32.png", actual);
        }

        [TestMethod]
        public void ConvertStopExpectedSystemError()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Stop, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertWarningExpectedSystemWarning()
        {
            var messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            var actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Warning, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png", actual);
        }

        #endregion

        #region Helper Methods

        Color GetPixel(BitmapSource bitmapImage)
        {
            var nStride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[4];
            bitmapImage.CopyPixels(pixels, nStride, 0);
            return Color.FromArgb(pixels[0], pixels[1], pixels[2], pixels[3]);
        }

        #endregion Helper Methods
    }
}
