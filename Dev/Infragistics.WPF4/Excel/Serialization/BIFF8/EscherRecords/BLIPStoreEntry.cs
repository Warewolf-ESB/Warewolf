using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords

{
	internal class BLIPStoreEntry : EscherRecordBase
	{
		private BLIPType typeForWin32;
		private BLIPType typeForMacOS;
		private Guid blipId;
		private ushort tag;
		private uint blipStreamSize;
		private uint referenceCountOnTheBlip;
		private uint fileOffsetInDelayStream;
		private BLIPUsage usage;

		private BLIP blip;

		public BLIPStoreEntry( BLIP blip, uint referenceCountOnTheBlip )
			: base( 0x02, (ushort)blip.BlipType, 36 + 8 + blip.RecordLength )
		{
			this.typeForWin32 = blip.BlipType;
			this.typeForMacOS = blip.BlipType;
			this.blipId = blip.SecondaryID;
			this.tag = 0x00FF;
			this.blipStreamSize = blip.RecordLength + 8;
			this.referenceCountOnTheBlip = referenceCountOnTheBlip;
			this.usage = BLIPUsage.Default;

			this.blip = blip;
		}

		public BLIPStoreEntry( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x02 );
			Debug.Assert( Enum.IsDefined( typeof( BLIPType ), (BLIPType)this.Instance ) );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			this.typeForWin32 = (BLIPType)manager.CurrentRecordStream.ReadByte();
			this.typeForMacOS = (BLIPType)manager.CurrentRecordStream.ReadByte();
			this.blipId = new Guid( manager.CurrentRecordStream.ReadBytes( 16 ) );
			this.tag = manager.CurrentRecordStream.ReadUInt16(); // not used
			this.blipStreamSize = manager.CurrentRecordStream.ReadUInt32();
			this.referenceCountOnTheBlip = manager.CurrentRecordStream.ReadUInt32();
			this.fileOffsetInDelayStream = manager.CurrentRecordStream.ReadUInt32();
			this.usage = (BLIPUsage)manager.CurrentRecordStream.ReadByte();
			byte lengthOfBlipName = (byte)manager.CurrentRecordStream.ReadByte();
			manager.CurrentRecordStream.ReadByte();
			manager.CurrentRecordStream.ReadByte();

			Debug.Assert( this.fileOffsetInDelayStream == 0 );
			Debug.Assert( lengthOfBlipName == 0 );

			if ( this.blipStreamSize > 0 )
			{
				this.blip = (BLIP)EscherRecordBase.LoadRecord( manager );
				Debug.Assert( this.blip.RecordLength + 8 == blipStreamSize );
				Debug.Assert( this.blip.SecondaryID == this.blipId );

				manager.Images.Add( new WorkbookSerializationManager.ImageHolder( this.blip.Image, this.referenceCountOnTheBlip ) );
			}
			else
			{
				Debug.Assert( this.typeForWin32 == BLIPType.Error );
				Debug.Assert( this.typeForMacOS == BLIPType.Error );
				Debug.Assert( this.blipId == Guid.Empty );
				Debug.Assert( this.RecordLength == 36 );
				Debug.Assert( this.referenceCountOnTheBlip == 0 );
				manager.Images.Add( new WorkbookSerializationManager.ImageHolder( null, this.referenceCountOnTheBlip ) );
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			manager.CurrentRecordStream.Write( (byte)this.typeForWin32 );
			manager.CurrentRecordStream.Write( (byte)this.typeForMacOS );
			manager.CurrentRecordStream.Write( this.blipId.ToByteArray() );
			manager.CurrentRecordStream.Write( this.tag );
			manager.CurrentRecordStream.Write( this.blipStreamSize );
			manager.CurrentRecordStream.Write( this.referenceCountOnTheBlip );
			manager.CurrentRecordStream.Write( this.fileOffsetInDelayStream );
			manager.CurrentRecordStream.Write( (byte)this.usage );
			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)0 );
			manager.CurrentRecordStream.Write( (byte)0 );

			this.blip.Save( manager );
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			sb.Append( "Reference Count on the BLIP: " + this.referenceCountOnTheBlip );
			sb.Append( "\n" );

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.BLIPStoreEntry; }
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