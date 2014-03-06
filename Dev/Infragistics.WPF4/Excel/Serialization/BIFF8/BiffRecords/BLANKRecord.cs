using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal sealed class BLANKRecord : CellValueRecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override object LoadCellValue( BIFF8WorkbookSerializationManager manager )
		//{
		//    return null;
		//}
		protected override void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue )
		{
			cellValue = lastCalculatedCellValue = null;
		}

		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object value )
		//{
		//    Debug.Assert( value == null || value is DBNull );
		//}
		// MD 4/18/11 - TFS62026
		// Since the FORMULA record is the only one which needs the lastCalculatedCellValue, it is no longer passed in here. Instead, pass along the cell
		// context so the FORMULA record can ask for the calculated value directly.
		// Also, we will write the cell value records in one block if possible, so pass in the memory stream containing the initial data from the record.
		//protected override void SaveCellValue( BIFF8WorkbookSerializationManager manager, object cellValue, object lastCalculatedCellValue )
		//{
		//    Debug.Assert( cellValue == null || cellValue is DBNull, "A non-null value should not be serailized with a BLANK record." );
		//}
		protected override void SaveCellValue(BIFF8WorkbookSerializationManager manager, CellContext cellContext, MemoryStream initialData)
		{
			Debug.Assert(cellContext.Value == null || cellContext.Value is DBNull, "A non-null value should not be serialized with a BLANK record.");
			manager.CurrentRecordStream.Write(initialData);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BLANK; }
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