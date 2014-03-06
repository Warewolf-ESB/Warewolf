using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;






using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;


namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class BLIP : EscherRecordBase
	{
		// MD 10/30/11 - TFS90733
		// Added support for EMF files (except in SL due to lack of GZip support) and cleaned up and reorganized the class.

		#region Member Variables

		private EscherRecordType type;

		private BLIPType blipType;

		private Image image;
		private byte[] imageData;

		private Guid secondaryId;
		private byte tag;

		#endregion // Member Variables

		#region Constructors

		public BLIP(Image image)
			: base(0x00, 0x0000, 0)
		{
			this.tag = 0xFF;

			this.image = image;

			if (this.image.RawFormat.Equals(ImageFormat.Jpeg))
			{
				this.blipType = BLIPType.JPEG;
				this.Instance = (ushort)BLIPRecordType.JPEG;
			}
			else
			{
				this.blipType = BLIPType.PNG;
				this.Instance = (ushort)BLIPRecordType.PNG;
			}

			this.type = (EscherRecordType)((int)this.blipType + (int)EscherRecordType.BLIPMin);

			using (MemoryStream stream = new MemoryStream())
			{
				if (this.blipType == BLIPType.JPEG)
					this.image.Save(stream, ImageFormat.Jpeg);
				else
					this.image.Save(stream, ImageFormat.Png);

				stream.Position = 0;
				this.imageData = new byte[stream.Length];
				stream.Read(this.imageData, 0, this.imageData.Length);
			}

			this.RecordLength = (uint)this.imageData.Length + 17;

			this.secondaryId = Utilities.ComputeMD4Hash(this.imageData);
		}

		public BLIP(EscherRecordType type, byte version, ushort instance, uint recordLength)
			: base(version, instance, recordLength)
		{
			this.type = type;
			this.blipType = (BLIPType)(this.type - EscherRecordType.BLIPMin);
			Debug.Assert(version == 0x00);

			Debug.Assert(Enum.IsDefined(typeof(BLIPRecordType), (BLIPRecordType)this.Instance));
		}

		#endregion // Constructors

		#region Base Class Overrides

		#region Load

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			switch (this.blipType)
			{
				case BLIPType.JPEG:
					Debug.Assert(this.Instance == (ushort)BLIPRecordType.JPEG);
					this.LoadBitmap(manager, ImageFormat.Jpeg);
					break;

				case BLIPType.PNG:
					Debug.Assert(this.Instance == (ushort)BLIPRecordType.PNG);
					this.LoadBitmap(manager, ImageFormat.Png);
					break;

				case BLIPType.DIB:
					Debug.Assert(this.Instance == (ushort)BLIPRecordType.DIB);
					this.LoadDIB(manager);
					break;

				case BLIPType.EMF:
					Debug.Assert(this.Instance == (ushort)BLIPRecordType.EMF);
					this.LoadEMF(manager);
					break;

				default:
					Utilities.DebugFail("Unknown blip type: " + this.blipType);
					break;
			}
		}

		#endregion // Load

		#region Save

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			base.Save(manager);

			manager.CurrentRecordStream.Write(this.secondaryId.ToByteArray());
			manager.CurrentRecordStream.Write(this.tag);
			manager.CurrentRecordStream.Write(this.imageData);
		}

		#endregion // Save

		#endregion // Base Class Overrides

		#region Methods

		#region LoadBitmap

		private void LoadBitmap(BIFF8WorkbookSerializationManager manager, ImageFormat imageFormat)
		{
			this.secondaryId = new Guid(manager.CurrentRecordStream.ReadBytes(16));
			this.tag = (byte)manager.CurrentRecordStream.ReadByte();

			this.imageData = manager.CurrentRecordStream.ReadBytes((int)(this.RecordLength - 17));
			Debug.Assert(this.secondaryId == Utilities.ComputeMD4Hash(this.imageData));

			MemoryStream memStream = new MemoryStream(this.imageData);




			this.image = Image.FromStream(memStream);

		}

		#endregion // LoadBitmap

		#region LoadDIB

		private void LoadDIB(BIFF8WorkbookSerializationManager manager)
		{
			this.secondaryId = new Guid(manager.CurrentRecordStream.ReadBytes(16));
			this.tag = (byte)manager.CurrentRecordStream.ReadByte();

			int remainingLength = (int)(this.RecordLength - 17);

			byte[] data = new byte[14 + remainingLength];
			manager.CurrentRecordStream.Read(data, 14, remainingLength);

			MemoryStream memStream = new MemoryStream(data);

			BinaryWriter writer = new BinaryWriter(memStream);

			writer.Write((byte)0x42);
			writer.Write((byte)0x4D);
			writer.Write((int)data.Length);
			writer.Write((int)0);
			writer.Write((int)54);

			memStream.Position = 0;




			this.image = Image.FromStream(memStream);

		}

		#endregion // LoadDIB

		// MD 10/30/11 - TFS90733
		#region LoadEMF

		private void LoadEMF(BIFF8WorkbookSerializationManager manager)
		{
			this.secondaryId = new Guid(manager.CurrentRecordStream.ReadBytes(16));
			int metafileSize = manager.CurrentRecordStream.ReadInt32();
			int rcBoundsLeft = manager.CurrentRecordStream.ReadInt32();
			int rcBoundsTop = manager.CurrentRecordStream.ReadInt32();
			int rcBoundsRight = manager.CurrentRecordStream.ReadInt32();
			int rcBoundsBottom = manager.CurrentRecordStream.ReadInt32();
			int ptSizeX = manager.CurrentRecordStream.ReadInt32();
			int ptSizeY = manager.CurrentRecordStream.ReadInt32();
			int cachedSize = manager.CurrentRecordStream.ReadInt32();
			byte compression = (byte)manager.CurrentRecordStream.ReadByte();
			byte filter = (byte)manager.CurrentRecordStream.ReadByte();

			byte[] data = manager.CurrentRecordStream.ReadBytes(cachedSize);

			if (compression == 0)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					// Got these values from the gzip documentation: http://www.gzip.org/zlib/rfc-gzip.html
					//
					ms.WriteByte((byte)0x1F); // Magic number part 1
					ms.WriteByte((byte)0x8B); // Magic number part 2
					ms.WriteByte((byte)0x08); // Compression method - (8: Deflate)

					// MD 12/6/11 - TFS92740
					// Change the flags to 0x00 because we are not going to write out the CRC field before the EMF data anymore.
					//ms.WriteByte((byte)0x02); // Flags
					ms.WriteByte((byte)0x00); // Flags

					ms.WriteByte((byte)0x00); // MTIME part 1
					ms.WriteByte((byte)0x00); // MTIME part 2
					ms.WriteByte((byte)0x00); // MTIME part 3
					ms.WriteByte((byte)0x00); // MTIME part 4
					ms.WriteByte((byte)0x02); // XFL - (2: max compression)
					ms.WriteByte((byte)0xFF); // OS - (255: Unknown)

					// MD 12/6/11 - TFS92740
					// Write the data starting at the 3rd byte so we don't include the CRC number. We write out 0x00 for the flags
					// so the GZipStream should not be expecting it.
					
					
					
					//ms.Write(data, 0, data.Length);
					ms.Write(data, 2, data.Length - 2);

					ms.Flush();
					ms.Position = 0;

					using (MemoryStream resultStream = new MemoryStream())
					{
						
						
						
						
						
						try
						{
							using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress))
							{
								byte[] buffer = new byte[4096];
								int read;
								while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
									resultStream.Write(buffer, 0, read);

								resultStream.Flush();
								this.imageData = resultStream.ToArray();
							}
						}
						catch (Exception ex)
						{
							Utilities.DebugFail("Exception when loading EMF file (this may be expected on SL): " + ex.ToString());
						}
					}
				}
			}
			else
			{
				Debug.Assert(compression == 254, "Unexpected compression value.");
				this.imageData = data;
			}

			Debug.Assert(this.secondaryId == Utilities.ComputeMD4Hash(this.imageData));

			MemoryStream memStream = new MemoryStream(this.imageData);




			this.image = Image.FromStream(memStream);

		}

		#endregion // LoadEMF

		#endregion // Methods

		#region Properties

		#region BlipType

		public BLIPType BlipType
		{
			get { return this.blipType; }
		}

		#endregion // BlipType

		#region Image

		public Image Image
		{
			get { return this.image; }
		}

		#endregion // Image

		#region SecondaryID

		public Guid SecondaryID
		{
			get { return this.secondaryId; }
		}

		#endregion // SecondaryID

		#region Type

		public override EscherRecordType Type
		{
			get { return this.type; }
		}

		#endregion // Type

		#endregion // Properties
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