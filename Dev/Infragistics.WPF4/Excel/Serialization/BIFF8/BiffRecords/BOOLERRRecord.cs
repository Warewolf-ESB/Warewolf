using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal sealed class BOOLERRRecord : CellValueRecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override object LoadCellValue( BIFF8WorkbookSerializationManager manager )
		protected override void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue )
		{
			byte value = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			bool isError = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex ) == 1;

			// MD 9/2/08 - Excel formula solving
			//if ( isError )
			//    return ErrorValue.FromValue( value );
			//else
			//    return Convert.ToBoolean( value );
			if ( isError )
				cellValue = ErrorValue.FromValue( value );
			else
				cellValue = Convert.ToBoolean( value );

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
			// MD 9/2/08 - Excel formula solving
			//ErrorValue error = value as ErrorValue;
			// MD 4/18/11 - TFS62026
			// The cellValue is no longer passed in. Get it from the cell context.
			//ErrorValue error = cellValue as ErrorValue;
			ErrorValue error = cellContext.Value as ErrorValue;

			if ( error != null )
			{
				// MD 4/18/11 - TFS62026
				// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
				//manager.CurrentRecordStream.Write(error.Value);
				//manager.CurrentRecordStream.Write((byte)1);
				initialData.WriteByte(error.Value);
				initialData.WriteByte(1);
			}
			// MD 9/2/08 - Excel formula solving
			//else if ( value is bool )
			//{
			//    manager.CurrentRecordStream.Write( Convert.ToByte( (bool)value ) );
			//    manager.CurrentRecordStream.Write( (byte)0 );
			//}
			// MD 4/18/11 - TFS62026
			// The cellValue is no longer passed in. Get it from the cell context.
			// Also, instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//else if ( cellValue is bool )
			//{
			//    manager.CurrentRecordStream.Write( Convert.ToByte( (bool)cellValue ) );
			//    manager.CurrentRecordStream.Write( (byte)0 );
			//}
			else if (cellContext.Value is bool)
			{
				initialData.WriteByte(Convert.ToByte((bool)cellContext.Value));
				initialData.WriteByte(0);
			}
			else
			{
                Utilities.DebugFail("Incorrect cell value type");
			}

			// MD 4/18/11 - TFS62026
			// Write all cell data at once for performance.
			manager.CurrentRecordStream.Write(initialData);
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.BOOLERR; }
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