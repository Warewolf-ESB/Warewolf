using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd947684(v=office.12).aspx
	internal class SHEETEXTRecord : Biff8RecordBase
	{
		// MD 1/27/12 - 12.1 - Cell Format Updates
		// Rewrote this code to reflect the recent cell format changes and the fact that this record now stores more info.
		#region Old Code

		//public override void Load( BIFF8WorkbookSerializationManager manager )
		//{
		//    Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

		//    if ( worksheet == null )
		//    {
		//        Utilities.DebugFail("There is no worksheet in the context stack.");
		//        return;
		//    }

		//    BIFF8RecordType repeatedRecordType = (BIFF8RecordType)manager.CurrentRecordStream.ReadUInt16();
		//    Debug.Assert( repeatedRecordType == this.Type );

		//    manager.CurrentRecordStream.ReadUInt16(); // FRT flags, not used currently
		//    manager.CurrentRecordStream.ReadBytes( 8 ); // Not used

		//    uint repeatedRecordLength = manager.CurrentRecordStream.ReadUInt32();
		//    Debug.Assert( repeatedRecordLength == manager.CurrentRecordStream.Length );

		//    uint optionFlags = manager.CurrentRecordStream.ReadUInt32();

		//    worksheet.DisplayOptions.TabColorIndex = (int)( optionFlags & 0x0000007F );
		//}

		//public override void Save( BIFF8WorkbookSerializationManager manager )
		//{
		//    Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

		//    if ( worksheet == null )
		//    {
		//        Utilities.DebugFail("There is no worksheet in the context stack.");
		//        return;
		//    }

		//    manager.CurrentRecordStream.Write( (ushort)this.Type );	// Repeated type
		//    manager.CurrentRecordStream.Write( (ushort)0 );			// FRT flags
		//    manager.CurrentRecordStream.Write( new byte[ 8 ] );		// not used
		//    manager.CurrentRecordStream.Write( (uint)0x14 );		// Repeated length
		//    manager.CurrentRecordStream.Write( (uint)worksheet.DisplayOptions.TabColorIndex );
		//}

		#endregion // Old Code

		private const uint SHEETEXTRecordSize = 0x28;

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];

			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.ReadFrtHeader();

			uint cb = manager.CurrentRecordStream.ReadUInt32();
			Debug.Assert(cb == manager.CurrentRecordStream.Length);

			uint rgbShxData = manager.CurrentRecordStream.ReadUInt32();
			int icvPlain = Utilities.GetBits(rgbShxData, 0, 6);

			if (cb < SHEETEXTRecordSize)
			{
				worksheet.DisplayOptions.TabColorInfo = new WorkbookColorInfo(manager.Workbook, icvPlain);
			}
			else
			{
				Debug.Assert(cb == SHEETEXTRecordSize, "There is now more information in the SHEETEXT record. Try to load it.");

				uint temp = manager.CurrentRecordStream.ReadUInt32();
				int icvPlain12 = Utilities.GetBits(temp, 0, 6);
				bool fCondFmtCalc = Utilities.TestBit(temp, 7);
				bool fNotPublished = Utilities.TestBit(temp, 8);
				Debug.Assert(icvPlain12 == icvPlain, "The indexes do not match.");

				worksheet.DisplayOptions.TabColorInfo = manager.CurrentRecordStream.ReadCFColor();
			}
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];

			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			manager.CurrentRecordStream.WriteFrtHeader();
			manager.CurrentRecordStream.Write(SHEETEXTRecordSize); // cb

			WorkbookColorInfo tabColorInfo = worksheet.DisplayOptions.TabColorInfo;
			int icvPlain = tabColorInfo.GetIndex(manager.Workbook, ColorableItem.WorksheetTab);

			manager.CurrentRecordStream.Write(icvPlain); // icvPlain

			uint temp = 0;
			Utilities.AddBits(ref temp, icvPlain, 0, 6); // icvPlain12
			Utilities.SetBit(ref temp, true, 7); // fCondFmtCalc
			Utilities.SetBit(ref temp, false, 8); // fNotPublished
			manager.CurrentRecordStream.Write(temp);

			manager.CurrentRecordStream.WriteCFColor(tabColorInfo, ColorableItem.WorksheetTab);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SHEETEXT; }
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