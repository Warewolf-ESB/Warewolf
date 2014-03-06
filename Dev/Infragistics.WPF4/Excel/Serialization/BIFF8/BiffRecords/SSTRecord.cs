using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class SSTRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.ReadUInt32(); // total strings used in document

			uint stringsInSST = manager.CurrentRecordStream.ReadUInt32();

			// MD 9/23/09 - TFS19150
			// Every read operation is relatively slow, so read the strings in blocks. 
			// This improves performance by about 300% in this method.
			//for ( int i = 0; i < stringsInSST; i++ )
			//{
			//    WorkbookSerializationManager.FormattedStringHolder holder = new WorkbookSerializationManager.FormattedStringHolder(
			//        manager.CurrentRecordStream.ReadFormattedString( LengthType.SixteenBit ),
			//        manager.CurrentRecordStream.PositionInCurrentBlock,
			//        manager.WorkbookStream.Position );
			//
			//    manager.SharedStringTable.Add( holder );
			//}
			manager.CurrentRecordStream.ReadFormattedStringBlock( stringsInSST, manager.SharedStringTable );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			manager.CurrentRecordStream.Write( (uint)manager.TotalStringsUsedInDocument );

			// MD 2/1/12 - TFS100573
			// The SharedStringTable collection now only stores additional strings not in the workbook's string table during 
			// a save operation, so use the SharedStringCountDuringSave instead.
			//manager.CurrentRecordStream.Write( (uint)manager.SharedStringTable.Count );
			manager.CurrentRecordStream.Write((uint)manager.SharedStringCountDuringSave);

			// MD 11/3/10 - TFS49093
			// Refactored to use the new FormattedStringElements.
			//foreach ( WorkbookSerializationManager.FormattedStringHolder holder in manager.SharedStringTable )
			//{
			//    holder.AbsolutePosition = manager.CurrentRecordStream.Position;
			//    holder.OffsetInRecordBlock = manager.CurrentRecordStream.PositionInCurrentBlock;
			//
			//    // MD 11/3/10 - TFS49093
			//    // The formatted string data is now stored on the FormattedStringElement. Also, we are removing the carriage 
			//    // returns on save so we only need to do it once per shared string.
			//    //manager.CurrentRecordStream.Write( holder.Value, LengthType.SixteenBit );
			//    FormattedStringElement element = holder.Value;
			//
			//    if (removeCarriageReturns)
			//        element = element.RemoveCarriageReturns();
			//
			//    manager.CurrentRecordStream.Write(element, LengthType.SixteenBit);
			//}
			bool removeCarriageReturns = manager.Workbook.ShouldRemoveCarriageReturnsOnSave;

			// MD 2/1/12 - TFS100573
			// Rewrote this code to first write out the workbook's string table, in order, and then the manager's string table (which during a 
			// save only stores additional strings for StringBuilders).
			#region Old Code

			//for(int i = 0; i < manager.SharedStringTable.Count; i++)
			//{
			//    if (i % 8 == 0)
			//    {
			//        BIFF8WorkbookSerializationManager.EXTSSTItem extSSTItem = new BIFF8WorkbookSerializationManager.EXTSSTItem();
			//        extSSTItem.AbsolutePosition = manager.CurrentRecordStream.Position;
			//        extSSTItem.OffsetInRecordBlock = manager.CurrentRecordStream.PositionInCurrentBlock;
			//        manager.EXTSSTData.Add(extSSTItem);
			//    }
			//
			//    StringElement element = manager.SharedStringTable[i];
			//
			//    if (removeCarriageReturns)
			//        element = element.RemoveCarriageReturns();
			//
			//    manager.CurrentRecordStream.Write(element, LengthType.SixteenBit);
			//}

			#endregion // Old Code
			int stringIndex = 0;
			foreach (StringElement element in manager.Workbook.SharedStringTable)
				SSTRecord.SaveStringElement(manager, element, removeCarriageReturns, ref stringIndex);

			// Write out the extra shared string table, which should only have the strings from string builders.
			for (int extraSharedStringIndex = 0; extraSharedStringIndex < manager.SharedStringTable.Count; extraSharedStringIndex++)
				SSTRecord.SaveStringElement(manager, manager.SharedStringTable[extraSharedStringIndex], removeCarriageReturns, ref stringIndex);
		}

		// MD 2/1/12 - TFS100573
		private static void SaveStringElement(BIFF8WorkbookSerializationManager manager, StringElement element, bool removeCarriageReturns, ref int stringIndex)
		{
			if (stringIndex % 8 == 0)
			{
				BIFF8WorkbookSerializationManager.EXTSSTItem extSSTItem = new BIFF8WorkbookSerializationManager.EXTSSTItem();
				extSSTItem.AbsolutePosition = manager.CurrentRecordStream.Position;
				extSSTItem.OffsetInRecordBlock = manager.CurrentRecordStream.PositionInCurrentBlock;
				manager.EXTSSTData.Add(extSSTItem);
			}

			if (removeCarriageReturns)
				element = element.RemoveCarriageReturns(manager.Workbook);

			manager.CurrentRecordStream.Write(element, LengthType.SixteenBit);
			stringIndex++;
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SST; }
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