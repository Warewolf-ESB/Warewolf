using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;






using System.Drawing;
using System.Drawing.Imaging;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class BITMAPRecord : Biff8RecordBase
    {

		// MD 8/23/11
		// Found while fixing TFS84306
		[SecuritySafeCritical]

        public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			ushort unknown1 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( unknown1 == 0x0009 );

			ushort unknown2 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( unknown2 == 0x0001 );

			uint recordSize = manager.CurrentRecordStream.ReadUInt32();

			ushort unknown3 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( unknown3 == 0x000C );

			ushort unknown4 = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( unknown4 == 0x0000 );

			ushort width = manager.CurrentRecordStream.ReadUInt16();
			ushort height = manager.CurrentRecordStream.ReadUInt16();

			// MD 5/16/07 - BR22962
			// Each row writes 3 bytes for the RGB value, but the rows need to be aligned by 4 bytes
			//Debug.Assert( recordSize == ( width * height * 3 ) + 12 );
			int lineWidth = Utilities.RoundUpToMultiple( width * 3, 4 );
			int numRgbValues = lineWidth * height;
			Debug.Assert( recordSize == numRgbValues + 12 );

			ushort numberOfPlanes = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( numberOfPlanes == 0x0001 );

			ushort colorDepth = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( colorDepth == 0x0018 );

            // MD 5/16/07 - BR22962
            // Each row writes 3 bytes for the RGB value, but the rows are aligned by 4 bytes, so use the data size calculated above
            //byte[] rgbValues = manager.CurrentRecordStream.ReadBytes( width * height * 3 );
            byte[] rgbValues = manager.CurrentRecordStream.ReadBytes(numRgbValues);



#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

			Bitmap image = new Bitmap( width, height );

			bool imageLoaded = false;
			BitmapData data = null;

			try
			{
				data = image.LockBits(
					new Rectangle( Point.Empty, image.Size ),
					ImageLockMode.WriteOnly,
					PixelFormat.Format24bppRgb );

				Marshal.Copy( rgbValues, 0, data.Scan0, rgbValues.Length );

				imageLoaded = true;
			}
			catch ( SecurityException ) { }
			finally
			{
				if ( data != null )
					image.UnlockBits( data );
			}

			if ( imageLoaded == false )
			{
				int arrayPos = 0;
				for ( int y = 0; y < height; y++ )
				{
					for ( int x = 0; x < width; x++ )
					{
						int blue = rgbValues[ arrayPos++ ];
						int green = rgbValues[ arrayPos++ ];
						int red = rgbValues[ arrayPos++ ];

						image.SetPixel( x, y, Color.FromArgb( red, green, blue ) );
					}

					// MD 5/16/07 - BR22962
					// Each row must be aligned by 4 bytes
					while ( arrayPos % 4 != 0 )
						arrayPos++;
				}
			}

			image.RotateFlip( RotateFlipType.RotateNoneFlipY );

			worksheet.ImageBackground = image;

		}


		// MD 8/23/11
		// Found while fixing TFS84306
		[SecuritySafeCritical]

        public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}



#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

			Bitmap image = worksheet.ImageBackground;
			Debug.Assert( image != null );

			if ( image == null )
				return;

			// MD 5/16/07 - BR22962
			// Each row writes 3 bytes for the RGB value, but the rows need to be aligned by 4 bytes
			//byte[] rgbValues = new byte[ image.Width * image.Height * 3 ];
			int lineWidth = Utilities.RoundUpToMultiple( image.Width * 3, 4 );
			byte[] rgbValues = new byte[ lineWidth * image.Height ];

			image.RotateFlip( RotateFlipType.RotateNoneFlipY );

			bool imageSaved = false;
			BitmapData data = null;

			try
			{
				data = image.LockBits(
					new Rectangle( Point.Empty, image.Size ),
					ImageLockMode.ReadOnly,
					PixelFormat.Format24bppRgb );

				Marshal.Copy( data.Scan0, rgbValues, 0, rgbValues.Length );

				imageSaved = true;
			}
			catch ( SecurityException ) { }
			finally
			{
				if ( data != null )
					image.UnlockBits( data );
			}

			if ( imageSaved == false )
			{
				int arrayPos = 0;
				for ( int y = 0; y < image.Height; y++ )
				{
					for ( int x = 0; x < image.Width; x++ )
					{
						Color pixel = image.GetPixel( x, y );

						rgbValues[ arrayPos++ ] = pixel.B;
						rgbValues[ arrayPos++ ] = pixel.G;
						rgbValues[ arrayPos++ ] = pixel.R;
					}

					// MD 5/16/07 - BR22962
					// Each row must be aligned by 4 bytes
					while ( arrayPos % 4 != 0 )
						rgbValues[ arrayPos++ ] = 0;
				}
			}

			image.RotateFlip( RotateFlipType.RotateNoneFlipY );

			manager.CurrentRecordStream.Write( (ushort)0x0009 );
			manager.CurrentRecordStream.Write( (ushort)0x0001 );
			manager.CurrentRecordStream.Write( (uint)( rgbValues.Length + 12 ) );
			manager.CurrentRecordStream.Write( (ushort)0x000C );
			manager.CurrentRecordStream.Write( (ushort)0x0000 );




			manager.CurrentRecordStream.Write( (ushort)image.Width );
			manager.CurrentRecordStream.Write( (ushort)image.Height );

			manager.CurrentRecordStream.Write( (ushort)0x0001 );
			manager.CurrentRecordStream.Write( (ushort)0x0018 );
			manager.CurrentRecordStream.Write( rgbValues );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BITMAP; }
		}
	}
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