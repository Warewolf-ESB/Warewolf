using System.Drawing;
using System.IO;

namespace Dev2.Warewolf.Security.Steganography
{
    public interface ISteganography
    {
        /// <summary>
        /// Hides a message in a bitmap
        /// </summary>
        /// <param name="message">The message to hide</param>
        /// <param name="privateKey">The key to use</param>
        /// <param name="bitmap">The carrier bitmap</param>
        /// <param name="useGrayscale"></param>
        void HideMessage(string message, string privateKey, Bitmap bitmap, bool useGrayscale = false);

        /// <summary>
        /// Hides a message in a bitmap. This method saves the bitmap automatically after the message has been hidden
        /// </summary>
        /// <param name="message">The message to hide</param>
        /// <param name="privateKey">The key to use</param>
        /// <param name="bitmapPath">The path to the bitmap file</param>
        /// <param name="useGrayscale"></param>
        void HideMessage(string message, string privateKey, string bitmapPath, bool useGrayscale = false);

        /// <summary>
        /// Hides a message in a bitmap represented by <paramref name="bitmapData"/> and returns the new byte[]
        /// </summary>
        /// <param name="message">The message to hide</param>
        /// <param name="privateKey">The key to use</param>
        /// <param name="useGrayscale"></param>
        void HideMessage(string message, string privateKey, ref byte[] bitmapData, bool useGrayscale = false);

        /// <summary>
        /// Hides a message in a bitmap represented by <paramref name="bitmapData"/> and returns the new byte[]
        /// </summary>
        /// <param name="message">The message to hide</param>
        /// <param name="privateKey">The key to use</param>
        /// <param name="useGrayscale"></param>
        byte[] HideMessage(string message, string privateKey, byte[] bitmapData, bool useGrayscale = false);

        /// <summary>Extracts a hidden message from a bitmap</summary>
        /// <param name="bitmap">The carrier bitmap</param>
        /// <param name="privateKey">The private key</param>
        /// <param name="messageStream">Empty stream to receive the message</param>
        void GetMessage(Bitmap bitmap, string privateKey, ref Stream messageStream);

        /// <summary>Extracts a hidden message from a bitmap</summary>
        /// <param name="bitmapPath"></param>
        /// <param name="privateKey">The private key</param>
        string GetMessage(string bitmapPath, string privateKey);

        /// <summary>Extracts a hidden message from a bitmap</summary>
        /// <param name="bitmapData"></param>
        /// <param name="privateKey">The private key</param>
        string GetMessage(byte[] bitmapData, string privateKey);

        /// <summary>Extracts a hidden message from a bitmap</summary>
        /// <param name="bitmap">The carrier bitmap</param>
        /// <param name="privateKey">The private key</param>
        string GetMessage(Bitmap bitmap, string privateKey);

        /*string UserKey { get; }*/

        /// <summary>
        /// Returns true if this <paramref name="bitmapData"/> has any base64 steganography
        /// </summary>
        /// <param name="bitmapData"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        bool HasBase64Message(byte[] bitmapData, string privateKey);
    }
}
