using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 10/30/11 - TFS90733
	internal sealed class HFPICTURERecord : Biff8RecordBase
	{
		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			ushort rt = manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert(rt == (ushort)this.Type, "The repeated type does not match.");

			ushort frtFlags = manager.CurrentRecordStream.ReadUInt16();

			// 8 Unused Bytes
			manager.CurrentRecordStream.Seek(8, SeekOrigin.Current);

			byte rfg = (byte)manager.CurrentRecordStream.ReadByte();

			bool isDrawing			= (rfg & 0x01) == 0x01;
			bool isDrawingGroup		= (rfg & 0x02) == 0x02;
			bool hasAdditionalData	= (rfg & 0x04) == 0x04;
			Debug.Assert(hasAdditionalData == false, "Implement the processing of this flag.");

			manager.CurrentRecordStream.Seek(1, SeekOrigin.Current);

			// MD 2/15/12 - TFS101503
			// The ClientAnchor record has different contents than it does with shapes (it seems to be the scaled width and height 
			// of the image if scaling were used, but I'm not sure). Also, we don't want to create WorksheetShape instanced for the
			// header/footer images anyway, so we'll just stop loading this until header/footer images are supported.
			//EscherRecordBase record = EscherRecordBase.LoadRecord(manager);
			//
			//if (isDrawing)
			//{
			//    Debug.Assert(record is DrawingContainer);
			//}
			//else if (isDrawingGroup)
			//{
			//    Debug.Assert(record is DrawingGroupContainer);
			//}
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			Utilities.DebugFail("Implement saving of this record.");
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.HFPicture; }
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