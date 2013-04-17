using System.Linq;
using Dev2.Studio.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

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

            Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
            Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertNoneExpectedEmptyBitmap()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.None, null, null, null) as BitmapSource;

            Color actualColor = GetPixel(actual);
            Color expectedColor = Color.FromArgb(0, 0, 0, 0);

            Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
            Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertInvalidValueExpectedEmptyBitmap()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert("", null, null, null) as BitmapSource;

            Color actualColor = GetPixel(actual);
            Color expectedColor = Color.FromArgb(0, 0, 0, 0);

            Assert.AreEqual(1, actual.PixelHeight, "Returned image should be a height of 1.");
            Assert.AreEqual(1, actual.PixelWidth, "Returned image should be a width of 1.");
            Assert.AreEqual(expectedColor, actualColor, "Returned image isn't empty.");
        }

        [TestMethod]
        public void ConvertAsteriskExpectedSystemAsterisk()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Asterisk, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Asterisk);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Asterisk value didn't return SystemIcons.Asterisk image.");
        }

        [TestMethod]
        public void ConvertErrorExpectedSystemError()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Error, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Error);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Error value didn't return SystemIcons.Error image.");
        }

        [TestMethod]
        public void ConvertExclamationExpectedSystemExclamation()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Exclamation, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Exclamation);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Exclamation value didn't return SystemIcons.Exclamation image.");
        }

        [TestMethod]
        public void ConvertHandExpectedSystemHand()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Hand, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Hand);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Hand value didn't return SystemIcons.Hand image.");
        }

        [TestMethod]
        public void ConvertInformationExpectedSystemInformation()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Information, null, null, null) as BitmapSource; ;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Information);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Information value didn't return SystemIcons.Information image.");
        }

        [TestMethod]
        public void ConvertQuestionExpectedSystemQuestion()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Question, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Question);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Question value didn't return SystemIcons.Question image.");
        }

        [TestMethod]
        public void ConvertStopExpectedSystemError()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Stop, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Error);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Stop value didn't return SystemIcons.Stop image.");
        }

        [TestMethod]
        public void ConvertWarningExpectedSystemWarning()
        {
            MessageBoxImageToSystemIconConverter messageBoxImageToSystemIconConverter = new MessageBoxImageToSystemIconConverter();
            BitmapSource actual = messageBoxImageToSystemIconConverter.Convert(MessageBoxImage.Warning, null, null, null) as BitmapSource;

            byte[] actualBytes = BytesFromBitmapSource(actual);
            byte[] expectedBytes = BytesFromIcon(SystemIcons.Warning);

            CollectionAssert.AreEqual(expectedBytes, actualBytes, "MessageBoxImage.Warning value didn't return SystemIcons.Warning image.");
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
