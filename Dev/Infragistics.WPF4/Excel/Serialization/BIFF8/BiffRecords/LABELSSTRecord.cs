using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal sealed class LABELSSTRecord : CellValueRecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override object LoadCellValue( BIFF8WorkbookSerializationManager manager )
		protected override void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue )
		{
			int sstIndex = manager.CurrentRecordStream.ReadInt32FromBuffer( ref data, ref dataIndex );

			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString formattedString = manager.SharedStringTable[ sstIndex ].Value;
			StringElement stringElement = manager.SharedStringTable[sstIndex];

			// MD 9/2/08 - Excel formula solving
			//if ( formattedString.HasFormatting == false )
			//    return formattedString.UnformattedString;
			//
			//// MD 2/29/08 - BR30961
			//// Each formatted string can only be used on one cell. If two cells have the same formatted string while 
			//// loading, we would get an exception for using the same formatted string. Clone the shared formatted string
			//// before returning it to be applied as the value of a cell.
			////return formattedString;
			//return formattedString.Clone();
			// MD 11/3/10 - TFS49093
			// To store strings, we now have to store a FormattedStringProxy.
			//if ( formattedString.HasFormatting == false )
			//    cellValue = formattedString.UnformattedString;
			//else
			//    cellValue = formattedString.Clone();
			//
			//lastCalculatedCellValue = cellValue;
			// MD 1/31/12 - TFS100573
			//if (formattedStringElement.HasFormatting)
			//    cellValue = new FormattedString(formattedStringElement);
			FormattedStringElement formattedStringElement = stringElement as FormattedStringElement;
			if (formattedStringElement != null && formattedStringElement.HasFormatting)
			{
				cellValue = new FormattedString(manager.Workbook, formattedStringElement);
			}
			else
			{
				// MD 4/12/11 - TFS67084
				// The regular strings will now be set with the element, not the proxy.
				//cellValue = new FormattedStringProxy(formattedStringElement, manager.Workbook.SharedStringTable);
				cellValue = stringElement;
			}

			lastCalculatedCellValue = cellValue;
		}

		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object value )
		// MD 4/18/11 - TFS62026
		// Since the FORMULA record is the only one which needs the lastCalculatedCellValue, it is no longer passed in here. Instead, pass along the cell
		// context so the FORMULA record can ask for the calculated value directly.
		// Also, we will write the cell value records in one block if possible, so pass in the memory stream containing the initial data from the record.
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object cellValue, object lastCalculatedCellValue )
		protected override void SaveCellValue(BIFF8WorkbookSerializationManager manager, CellContext cellContext, MemoryStream initialData)
		{
			// MD 4/18/11 - TFS62026
			#region Refactored

			//// MD 11/3/10 - TFS49093
			//// StringBuilders must be processed manually because they can't go into the shared string table.
			//StringBuilder sb = cellValue as StringBuilder;
			//if (sb != null)
			//{
			//    int sbIndex;
			//    if (manager.AdditionalStringsInStringTable.TryGetValue(sb, out sbIndex) == false)
			//        Utilities.DebugFail("The string value is not in the shared string table");

			//    manager.CurrentRecordStream.Write(sbIndex);
			//    return;
			//}

			//// MD 9/2/08 - Excel formula solving
			////FormattedString formattedString = value as FormattedString;
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString formattedString = cellValue as FormattedString;
			//FormattedStringElement formattedString = cellValue as FormattedStringElement;

			//if ( formattedString == null )
			//{
			//    Utilities.DebugFail("Incorrect cell value type");
			//    manager.CurrentRecordStream.Write( (int)0 );
			//    return;
			//}

			//// MD 6/11/07 - BR23706
			//// Do a binary search instead of a regular search...the table is sorted now and the binary search is much faster
			//// MD 11/3/10 - TFS49093
			//// The element now stores it's location in the table, so we don't need to do a search.
			////int index = manager.SharedStringTable.BinarySearch( 
			////    new WorkbookSerializationManager.FormattedStringHolder( formattedString, 0, 0 ) );
			//int index = formattedString.IndexInStringTable;
			////int index = manager.SharedStringTable.FindIndex( new Predicate<WorkbookSerializationManager.FormattedStringHolder>(
			////    delegate( WorkbookSerializationManager.FormattedStringHolder stringHolder )
			////    {
			////        return stringHolder.Value.Equals( formattedString );
			////    } ) );

			//if ( index < 0 )
			//{
			//    Utilities.DebugFail("The string value is not in the shared string table");
			//    manager.CurrentRecordStream.Write( (int)0 );
			//    return;
			//}

			//manager.CurrentRecordStream.Write( index ); 

			#endregion // Refactored
			// MD 2/1/12 - TFS100573
			// Refactored this code again now that the cell value is no longer the StringElement or StringBuilder, but their index 
			// in the shared string table.
			#region Refactored

			//StringElement formattedString = cellContext.Value as StringElement;
			//if (formattedString != null)
			//{
			//    int index = formattedString.IndexInStringTable;
			//    if (index < 0)
			//    {
			//        Utilities.DebugFail("The string value is not in the shared string table");
			//        initialData.Write(BitConverter.GetBytes((int)0), 0, 4);
			//    }
			//    else
			//    {
			//        initialData.Write(BitConverter.GetBytes(index), 0, 4);
			//    }
			//}
			//else
			//{
			//    StringBuilder sb = cellContext.Value as StringBuilder;
			//    if (sb != null)
			//    {
			//        int sbIndex;
			//        if (manager.AdditionalStringsInStringTable.TryGetValue(sb, out sbIndex) == false)
			//            Utilities.DebugFail("The string value is not in the shared string table");
			//
			//        initialData.Write(BitConverter.GetBytes(sbIndex), 0, 4);
			//    }
			//    else
			//    {
			//        Utilities.DebugFail("Incorrect cell value type");
			//        initialData.Write(BitConverter.GetBytes((int)0), 0, 4);
			//    }
			//}

			#endregion // Refactored
			StringElementIndex stringIndex = cellContext.Value as StringElementIndex;
			if (stringIndex != null)
			{
				initialData.Write(BitConverter.GetBytes(stringIndex.IndexInSharedStringTable), 0, 4);
			}
			else
			{
				Utilities.DebugFail("Incorrect cell value type");
				initialData.Write(BitConverter.GetBytes((int)0), 0, 4);
			}

			manager.CurrentRecordStream.Write(initialData);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.LABELSST; }
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