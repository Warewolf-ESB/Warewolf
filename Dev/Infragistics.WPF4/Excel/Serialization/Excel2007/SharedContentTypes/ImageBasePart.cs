using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;





using System.Drawing;
using System.Drawing.Imaging;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes
{
    #region ImageBasePart class
    internal abstract class ImageBasePart : ContentTypeBase
	{
		#region Constants

		private const string BasePartNameWithoutExtension = "/xl/media/image";

        //  NA 2011.1 - Infragistics.Word
        internal const string BasePartNameWithoutExtensionWord = "/word/media/image";

		private const string ExtensionGif = ".gif";
		private const string ExtensionJpeg = ".jpeg";
		private const string ExtensionPng = ".png";
		private const string ExtensionTiff = ".tiff";

        //  NA 2011.1 - Infragistics.Word
		private const string ExtensionBmp = ".bmp";

		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image"; 

		#endregion Constants

        #region Methods

        #region GetEncoderInfo

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; j++)
            {
                if (encoders[j].FormatID == format.Guid)
                    return encoders[j];
            }
            return null;
        }
        #endregion //GetEncoderInfo


		// MD 10/24/11 - TFS91531
		// Moved this code from the Load method.
		#region GetImageHolder


		public static ImageHolder GetImageHolder(Stream imageStream)
		{
			using (Bitmap bmp = new Bitmap(imageStream))
			{
				// Copy the original image into a new image, since we cannot 
				// maintain the original bitmap around a stream
				// MD 7/12/10 - TFS35668
				// Images with certain pixel formats throw errors when they are passed in to Graphics.FromImage.
				// When those formats are used, we should copy the image without preserving the pixel format.
				//Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
				//using (Graphics g = Graphics.FromImage(bmp2))
				//{
				//    g.DrawImage(bmp, Point.Empty);                                        
				//}
				Bitmap bmp2;
				switch (bmp.PixelFormat)
				{
					case PixelFormat.Format1bppIndexed:
					case PixelFormat.Format4bppIndexed:
					case PixelFormat.Format8bppIndexed:
					case PixelFormat.Undefined:
					case PixelFormat.Format16bppArgb1555:
					case PixelFormat.Format16bppGrayScale:
						bmp2 = new Bitmap(bmp);
						break;

					default:
						bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
						using (Graphics g = Graphics.FromImage(bmp2))
						{
							// MD 8/2/11 - TFS82969
							// This seems to distort image sometimes, but if we specify the bounds and graphics unit, 
							// things seem to work ok.
							//g.DrawImage(bmp, Point.Empty);
							Rectangle rect = new Rectangle(Point.Empty, bmp.Size);
							g.DrawImage(bmp, rect, rect, GraphicsUnit.Pixel);
						}

						break;
				}

				foreach (PropertyItem item in bmp.PropertyItems)
					bmp2.SetPropertyItem(item);

				// We need to store the image in a holder with the original format
				// since we can't set the image format, so we'll return the original format
				// later when saving the image
				return new ImageHolder(bmp2, bmp.RawFormat);
			}
		}


		#endregion  // GetImageHolder

        #endregion //Methods

        #region Base Class Overrides

        #region Load

        public override object Load( Excel2007WorkbookSerializationManager manager, Stream contentTypeStream )
		{

			// MD 10/24/11 - TFS91531
			// Moved this code to the new GetImageHolder method so it could be used in other places.
			#region Moved

			//using (Bitmap bmp = new Bitmap(contentTypeStream))
			//{
			//    // Copy the original image into a new image, since we cannot 
			//    // maintain the original bitmap around a stream
			//    // MD 7/12/10 - TFS35668
			//    // Images with certain pixel formats throw errors when they are passed in to Graphics.FromImage.
			//    // When those formats are used, we should copy the image without preserving the pixel format.
			//    //Bitmap bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
			//    //using (Graphics g = Graphics.FromImage(bmp2))
			//    //{
			//    //    g.DrawImage(bmp, Point.Empty);                                        
			//    //}
			//    Bitmap bmp2;
			//    switch (bmp.PixelFormat)
			//    {
			//        case PixelFormat.Format1bppIndexed:
			//        case PixelFormat.Format4bppIndexed:
			//        case PixelFormat.Format8bppIndexed:
			//        case PixelFormat.Undefined:
			//        case PixelFormat.Format16bppArgb1555:
			//        case PixelFormat.Format16bppGrayScale:
			//            bmp2 = new Bitmap(bmp);
			//            break;

			//        default:
			//            bmp2 = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
			//            using (Graphics g = Graphics.FromImage(bmp2))
			//            {
			//                // MD 8/2/11 - TFS82969
			//                // This seems to distort image sometimes, but if we specify the bounds and graphics unit, 
			//                // things seem to work ok.
			//                //g.DrawImage(bmp, Point.Empty);
			//                Rectangle rect = new Rectangle(Point.Empty, bmp.Size);
			//                g.DrawImage(bmp, rect, rect, GraphicsUnit.Pixel);
			//            }

			//            break;
			//    }

			//    foreach (PropertyItem item in bmp.PropertyItems)
			//        bmp2.SetPropertyItem(item);

			//    // We need to store the image in a holder with the original format
			//    // since we can't set the image format, so we'll return the original format
			//    // later when saving the image
			//    return new ImageHolder(bmp2, bmp.RawFormat);
			//}

			#endregion  // Moved
			return ImageBasePart.GetImageHolder(contentTypeStream);




		}

		#endregion Load

		#region RelationshipType

		public override string RelationshipType
		{
			get { return JpegImagePart.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region Save

		public override void Save( Excel2007WorkbookSerializationManager manager, System.IO.Stream contentTypeStream )
		{
                        Image image = (Image)manager.ContextStack[ typeof( Image ) ];

                        if ( image == null )
                        {
                            Utilities.DebugFail( "Could not find an image on the context stack." );
                            return;
                        }
            
                        ImageCodecInfo codecInfo = GetEncoderInfo(this.Format);
                        if (codecInfo != null)
                        {
                            Encoder encoder = Encoder.Quality;
                            EncoderParameters encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(encoder, 100L);
                            image.Save(contentTypeStream, codecInfo, encoderParameters);
                        }
                        else
                            Utilities.DebugFail("Unable to get the code info for the specified format: " + this.Format);
            



		}


        #endregion Save 

		#endregion Base Class Overrides

		protected abstract ImageFormat Format { get; }

		#region AddImageToPackage

        //  NA 2011.1 - Infragistics.Word
        //  Refactored
		public static Uri AddImageToPackage( Excel2007WorkbookSerializationManager manager, Image image, ImageFormat preferredFormat, out string relationshipId )
		{
            return ImageBasePart.AddImageToPackage( manager, image, preferredFormat, ImageBasePart.BasePartNameWithoutExtension, true, out relationshipId );
        }

		public static Uri AddImageToPackage(
            Excel2007WorkbookSerializationManager manager,
            Image image,
            ImageFormat preferredFormat,
            string basePartNameWithoutExtension,
            bool convertBMPToPNG,
            out string relationshipId )
		{
			string contentType;
			string extension;

			Guid formatGuid = (preferredFormat ?? image.RawFormat).Guid;


            //  NA 2011.1 - Infragistics.Word
            //
            //  Note: I went through this trouble because I noticed
            //  that Word preserves the file format when bitmaps are used.
            //  However, the bitmap I used for testing has an RawFormat
            //  with a Guid that is the same as the ImageFormat.PNG.Guid,
            //  which is odd to say the least. I can't think of a good way
            //  to make these bitmaps be serialized as bitmaps, and I'm not
            //  even sure that we should do that. In any case, it seems that
            //  bitmaps will be serialized as PNGs in these cases.
            //
            #region Obsolete
            //bool flag = formatGuid == ImageFormat.Bmp.Guid ||
            //    formatGuid == ImageFormat.MemoryBmp.Guid ||
            //    formatGuid == ImageFormat.Png.Guid;
            #endregion Obsolete

            bool isBitmap = formatGuid == ImageFormat.Bmp.Guid || formatGuid == ImageFormat.MemoryBmp.Guid;
            bool usePng = (isBitmap && convertBMPToPNG) || formatGuid == ImageFormat.Png.Guid;



            if ( usePng )
			{
				contentType = PngImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionPng;
			}
			else if ( formatGuid == ImageFormat.Jpeg.Guid )
			{
				contentType = JpegImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionJpeg;
			}
			else if ( formatGuid == ImageFormat.Gif.Guid )
			{
				contentType = GifImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionGif;
			}
			else if ( formatGuid == ImageFormat.Tiff.Guid )
			{
				contentType = TiffImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionTiff;
			}

			//  NA 2011.1 - Infragistics.Word
			else if (isBitmap)
			{
				contentType = BmpImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionBmp;
			} 

			else
			{
				Utilities.DebugFail( "Unsupported image format: " + formatGuid );
				contentType = PngImagePart.ContentTypeValue;
				extension = ImageBasePart.ExtensionPng;
			}

            //  NA 2011.1 - Infragistics.Word
			//string partPath = manager.GetNumberedPartName( ImageBasePart.BasePartNameWithoutExtension + extension, true );
			string partPath = manager.GetNumberedPartName( basePartNameWithoutExtension + extension, true );

			try
			{
				manager.ContextStack.Push( image );
				return manager.CreatePartInPackage( contentType, partPath, image, out relationshipId );
			}
			finally
			{
				manager.ContextStack.Pop(); // image
			}
		} 

		#endregion AddImageToPackage
	}
    #endregion ImageBasePart class



	//  NA 2011.1 - Infragistics.Word
	#region BmpImagePart class
	internal class BmpImagePart : ImageBasePart
	{
		#region Constants

		public const string ContentTypeValue = "image/bmp";

		#endregion Constants

		#region Base Class Overrides

		#region ContentType

		public override string ContentType
		{
			get { return BmpImagePart.ContentTypeValue; }
		}

		#endregion ContentType

		#region Format

		protected override ImageFormat Format
		{
			get { return ImageFormat.Bmp; }
		}

		#endregion Format

		#endregion Base Class Overrides
	}
	#endregion BmpImagePart class


}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved