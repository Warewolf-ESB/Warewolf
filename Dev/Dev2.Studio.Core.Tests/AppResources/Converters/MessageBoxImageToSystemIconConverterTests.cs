
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
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MessageBoxImageToSystemIconConverterTests
    {
        #region Tests

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackExpectedNotImplementedException()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            messageBoxImageToSystemIconConverter.ConvertBack(null, null, null, null);
        }

        [TestMethod]
        public void ConvertNullExpectedEmptyBitmap()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(null, null, null, null) as BitmapSource;

            Color actualColor = GetPixel(actual);
            Color expectedColor = Color.FromArgb(0, 0, 0, 0);

            if(actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertNoneExpectedEmptyBitmap()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.None, null, null, null) as BitmapSource;

            Color actualColor = GetPixel(actual);
            Color expectedColor = Color.FromArgb(0, 0, 0, 0);

            if(actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertInvalidValueExpectedEmptyBitmap()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert("", null, null, null) as BitmapSource;

            Color actualColor = GetPixel(actual);
            Color expectedColor = Color.FromArgb(0, 0, 0, 0);

            if(actual != null)
            {
                Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
                Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            }
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertAsteriskExpectedSystemAsterisk()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Asterisk, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png", actual);
        }

        [TestMethod]
        public void ConvertErrorExpectedSystemError()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Error, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertExclamationExpectedSystemExclamation()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Exclamation, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png", actual);
        }

        [TestMethod]
        public void ConvertHandExpectedSystemHand()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Hand, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertInformationExpectedSystemInformation()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Information, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png", actual);
        }

        [TestMethod]
        public void ConvertQuestionExpectedSystemQuestion()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Question, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/GenericHelp-32.png", actual);
        }

        [TestMethod]
        public void ConvertStopExpectedSystemError()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Stop, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png", actual);
        }

        [TestMethod]
        public void ConvertWarningExpectedSystemWarning()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();

            string actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Warning, null, null, null) as string;

            Assert.AreEqual("pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png", actual);
        }

        #endregion

        #region Helper Methods

        private Color GetPixel(BitmapSource bitmapImage)
        {
            int nStride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[4];
            bitmapImage.CopyPixels(pixels, nStride, 0);
            return Color.FromArgb(pixels[0], pixels[1], pixels[2], pixels[3]);
        }

        private byte[] BytesFromBitmapSource(BitmapSource bitmapImage)
        {
            MemoryStream ms = new MemoryStream();

            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(ms);

            return ms.ToArray();
        }

        private byte[] BytesFromIcon(Icon icon)
        {
            return BytesFromBitmapSource(System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
        }

        #endregion Helper Methods
    }
}
