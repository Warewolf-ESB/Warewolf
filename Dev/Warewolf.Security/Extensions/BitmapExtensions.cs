
using System.Drawing;
using System.IO;

namespace Dev2.Warewolf.Security.Extensions
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// 	Converts a byte[] to a bitmap
        /// </summary>
        /// <param name = "image">The byte[]</param>
        /// <returns>The bitmap</returns>
        public static Bitmap BytesToImage(this byte[] image)
        {
            var bitmap = new Bitmap(1, 1);
            Bitmap newBitmap = null;

            // use a memory stream
            using (var stream = new MemoryStream())
            {
                // write the byte array to the stream
                stream.Write(image, 0, image.Length);

                // create a bitmap from the stream
                bitmap = Image.FromStream(stream) as Bitmap;

                // TODO : quick fix for unlocking the bits on the new bitmap
                // create a new bitmap and basically clone the new one. (for some reason the bits get locked in the memory stream)
                newBitmap = new Bitmap(bitmap);
            }

            return newBitmap;
        }

        /// <summary>
        /// 	Converts a Image image to a byte[]
        /// </summary>
        /// <param name = "image">The image object</param>
        /// <returns></returns>
        public static byte[] ImageToBytes(this Image image)
        {
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
    }
}
