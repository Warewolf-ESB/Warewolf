using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class CRNRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			// ---------------------------------
			// MD 3/30/11 - TFS69969
			// Implemented this method.
			// ---------------------------------

			if (manager.CurrentExternalWorksheetReference == null)
			{
				Utilities.DebugFail("There is no active worksheet reference.");
				return;
			}

			byte lastColumnIndex = (byte)manager.CurrentRecordStream.ReadByte();
			byte firstColumnIndex = (byte)manager.CurrentRecordStream.ReadByte();
			ushort rowIndex = manager.CurrentRecordStream.ReadUInt16();

			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//for (int columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; columnIndex++)
			for (short columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; columnIndex++)
			{
				byte grbit = (byte)manager.CurrentRecordStream.ReadByte();

				object value;
				switch (grbit)
				{
					case 0x01:
						value = manager.CurrentRecordStream.ReadDouble();
						break;

					case 0x02:
						// MD 4/19/11 - TFS72977
						// The documentation from MS was wrong here. The string length is 2 bytes, not 1.
						//value = manager.CurrentRecordStream.ReadFormattedString(LengthType.EightBit);
						value = manager.CurrentRecordStream.ReadFormattedString(LengthType.SixteenBit);
						break;

					case 0x04:
						value = manager.CurrentRecordStream.ReadUInt16() != 0;
						manager.CurrentRecordStream.Seek(6, System.IO.SeekOrigin.Current);
						break;

					case 0x10:
						value = ErrorValue.FromValue((byte)manager.CurrentRecordStream.ReadUInt16());
						manager.CurrentRecordStream.Seek(6, System.IO.SeekOrigin.Current);
						break;

					default:
						Utilities.DebugFail("Unknown grbit: " + grbit);
						value = null;

						// MD 4/19/11 - TFS72977
						// If there's a problem, just bail out, because we don't want to accidentally read past the end of the stream after this.
						//manager.CurrentRecordStream.Seek(8, System.IO.SeekOrigin.Current);
						//break;
						return;
				}

				manager.CurrentExternalWorksheetReference.SetCachedValue(rowIndex, columnIndex, value);
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
            Utilities.DebugFail("Implement");
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.CRN; }
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