using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// http://msdn.microsoft.com/en-us/library/dd908919(v=office.12).aspx
	internal sealed class FORMULARecord : CellValueRecordBase
	{
		// MD 9/2/08 - Excel formula solving
		// Changed signature to allow FORMULA records to save out calculated values
		//protected override object LoadCellValue( BIFF8WorkbookSerializationManager manager )
		protected override void LoadCellValue( BIFF8WorkbookSerializationManager manager, ref byte[] data, ref int dataIndex, out object cellValue, out object lastCalculatedCellValue )
		{
			// MD 9/2/08 - Excel formula solving
			//manager.CurrentRecordStream.ReadUInt64(); // Cached cell value
			ulong cachedValueCode = manager.CurrentRecordStream.ReadUInt64FromBuffer( ref data, ref dataIndex ); // Cached cell value
			lastCalculatedCellValue = FORMULARecord.DecodeCacheValue( manager, cachedValueCode );

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			bool recalculateAlways =	( optionFlags & 0x0001 ) == 0x0001;
			//bool calculateOnOpen =		( optionFlags & 0x0002 ) == 0x0002;
			//bool isSharedFormula =	( optionFlags & 0x0008 ) == 0x0008;

			manager.CurrentRecordStream.ReadUInt32FromBuffer( ref data, ref dataIndex );

			Formula formula = Formula.Load( manager.CurrentRecordStream, FormulaType.Formula, ref data, ref dataIndex );

			Debug.Assert( formula.IsSpecialFormula || formula.RecalculateAlways == recalculateAlways, "The recalculateAlways bit is not correct." );

			cellValue = formula;
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
			// Refactored for performance.
			#region Refactored

			//// MD 9/2/08 - Excel formula solving
			////WorksheetDataTable dataTable = value as WorksheetDataTable;
			//WorksheetDataTable dataTable = cellValue as WorksheetDataTable;
			//
			//if ( dataTable != null )
			//{
			//    // MD 9/2/08 - Excel formula solving
			//    //value = new Formula( dataTable );
			//    cellValue = new Formula( dataTable );
			//}
			//
			//// MD 9/2/08 - Excel formula solving
			////Formula formula = value as Formula;
			//Formula formula = cellValue as Formula;
			//
			//if ( formula == null )
			//{
			//    Utilities.DebugFail("Incorrect cell value type");
			//    return;
			//} 

			#endregion // Refactored
			Formula formula = cellContext.Value as Formula;
			if (formula == null)
			{
				WorksheetDataTable dataTable = cellContext.Value as WorksheetDataTable;

				if (dataTable != null)
					formula = new Formula(dataTable);

				if (formula == null)
				{
					Utilities.DebugFail("Incorrect cell value type");
					return;
				}
			}

			// MD 2/1/12 - TFS100573
			// This is now an instance method.
			//object lastCalculatedCellValue = WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, cellContext.ColumnIndex, true);
			object lastCalculatedCellValue = manager.GetSerializableCellValue(cellContext.Row, cellContext.ColumnIndex, true);

			ArrayFormula arrayFormula = formula as ArrayFormula;

			if ( arrayFormula != null )
				formula = arrayFormula.InteriorCellFormula;

			// MD 9/2/08 - Excel formula solving
			//manager.CurrentRecordStream.Write( (ulong)0 );
			string stringValue;

			// MD 3/13/12
			// Found while fixing TFS104753
			// EncodeCacheValue needs a workbook reference to convert calculated dates to doubles.
			//ulong cachedValue = FORMULARecord.EncodeCacheValue( lastCalculatedCellValue, out stringValue );
			ulong cachedValue = FORMULARecord.EncodeCacheValue(manager.Workbook, lastCalculatedCellValue, out stringValue);

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write( cachedValue );
			initialData.Write(BitConverter.GetBytes(cachedValue), 0, 8);

			ushort optionFlags = 0;

			if ( formula.RecalculateAlways )
				optionFlags |= 0x0001;

			// MD 4/18/11 - TFS62026
			// Instead of writing to the record stream, write to the memory stream and we will send it to the record stream in one shot.
			//manager.CurrentRecordStream.Write( optionFlags );
			//manager.CurrentRecordStream.Write( (uint)0 );
			initialData.Write(BitConverter.GetBytes(optionFlags), 0, 2);
			initialData.Write(BitConverter.GetBytes((uint)0), 0, 4);

			// MD 4/18/11 - TFS62026
			// Writing the formula itself depends on the alignment of the blocks in the record stream, so we must push the initial data
			// to the record stream here and then write the formula normally.
			manager.CurrentRecordStream.Write(initialData);

			formula.Save( manager.CurrentRecordStream, true, false );

			// MD 7/6/09 - TFS18932
			// If an empty string is the cached value, we still need to write out the STRING record after this. 
			// If we don't Excel can't open the workbook properly.
			//if ( String.IsNullOrEmpty( stringValue ) )
			if ( stringValue == null )
				return;

			manager.ContextStack.Push( new ChildDataItem( stringValue ) );
			manager.WriteRecord( BIFF8RecordType.STRING );
			manager.ContextStack.Pop();
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.FORMULA; }
		}

		// MD 9/2/08 - Excel formula solving
		#region DecodeCacheValue

		private static object DecodeCacheValue( BIFF8WorkbookSerializationManager manager, ulong cachedValueCode )
		{
			byte[] cachedValueBytes = BitConverter.GetBytes( cachedValueCode );
			double number = BitConverter.ToDouble( cachedValueBytes, 0 );

			if ( Double.IsNaN( number ) == false )
				return number;

			switch ( cachedValueBytes[ 0 ] )
			{
				case 0:
					{
						long originalPosition = manager.CurrentRecordStream.Position;
						manager.CurrentRecordStream.Position = manager.CurrentRecordStream.Length;

						try
						{
							// MD 1/14/09 - TFS12404
							// Removed the using statement and manually disposed the stream in a try...finally so we can reassign the stringStream
							// variable if necessary.
							//using ( Biff8RecordStream stringStream = new Biff8RecordStream( manager ) )
							Biff8RecordStream stringStream = new Biff8RecordStream( manager );
							try
							{
								// MD 1/14/09 - TFS12404
								// If this FORMULA record is for the top-left cell of a shared formula, the SHRFMLA will come first and the STRING 
								// record will appear afterwards, so force the processing of the SHRFMLA record here.
								if ( stringStream.RecordType == BIFF8RecordType.SHRFMLA )
								{
									manager.CurrentRecordStream.AddSubStream( stringStream );
									manager.LoadRecord( stringStream );

									// Dispose the SHRFMLA record stream and move to the next record, which should be the STRING record.
									stringStream.Dispose();
									stringStream = new Biff8RecordStream( manager );
								}

								if ( stringStream.RecordType != BIFF8RecordType.STRING )
								{
                                    Utilities.DebugFail("The next record type should have been a STRING record.");
									return string.Empty;
								}

								manager.CurrentRecordStream.AddSubStream( stringStream );

								ChildDataItem stringHolder = new ChildDataItem();

								manager.ContextStack.Push( stringHolder );
								manager.LoadRecord( stringStream );
								manager.ContextStack.Pop(); // stringHolder

								Debug.Assert( stringHolder.Data is string, "A string should have been loaded from the record." );

								return stringHolder.Data;
							}
							// MD 1/14/09 - TFS12404
							finally
							{
								stringStream.Dispose();
							}
						}
						finally
						{
							manager.CurrentRecordStream.Position = originalPosition;
						}
					}

				case 1:
					return BitConverter.ToBoolean( cachedValueBytes, 2 );

				case 2:
					return ErrorValue.FromValue( cachedValueBytes[ 2 ] );

				case 3:
					return string.Empty;

				default:
                    Utilities.DebugFail("Invalid cached value.");
					return null;
			}
		}

		#endregion DecodeCacheValue

		// MD 9/2/08 - Excel formula solving
		#region EncodeCacheValue

		// MD 3/13/12
		// Found while fixing TFS104753
		// EncodeCacheValue needs a workbook reference to convert calculated dates to doubles.
		//private static ulong EncodeCacheValue( object value, out string stringValue )
		//{
		//    return BitConverter.ToUInt64( FORMULARecord.GetCacheValueBytes( value, out stringValue ), 0 );
		//}
		private static ulong EncodeCacheValue(Workbook workbook, object value, out string stringValue)
		{
			return BitConverter.ToUInt64(FORMULARecord.GetCacheValueBytes(workbook, value, out stringValue), 0);
		}

		#endregion EncodeCacheValue

		// MD 9/2/08 - Excel formula solving
		#region GetCacheValueBytes

		// MD 3/13/12
		// Found while fixing TFS104753
		// GetCacheValueBytes needs a workbook reference to convert calculated dates to doubles.
		//private static byte[] GetCacheValueBytes( object value, out string stringValue )
		private static byte[] GetCacheValueBytes(Workbook workbook, object value, out string stringValue)
		{
			stringValue = null;

			if ( value is bool )
			{
				return new byte[]
				{
					0x01,
					0x00,
					Convert.ToByte( (bool)value ),
					0x00,
					0x00,
					0x00,
					0xFF,
					0xFF,
				};
			}

			ErrorValue errorValue = value as ErrorValue;

			if ( errorValue != null )
			{
				return new byte[]
				{
					0x02,
					0x00,
					errorValue.Value,
					0x00,
					0x00,
					0x00,
					0xFF,
					0xFF,
				};
			}

			string strValue = value as string;

			if ( strValue == null )
			{
				// MD 11/3/10 - TFS49093
				// The formatted string data is now stored on the FormattedStringElement.
				//FormattedString formattedString = value as FormattedString;
				StringElement formattedString = value as StringElement;

				if ( formattedString != null )
					strValue = formattedString.UnformattedString;
			}

			if ( strValue != null )
			{
				stringValue = strValue;

				return new byte[]
				{
					0x00,
					0x00,
					0x00,
					0x00,
					0x00,
					0x00,
					0xFF,
					0xFF,
				};
			}

			// MD 3/13/12
			// Found while fixing TFS104753
			if (value is DateTime)
			{
				double? excelDate = Infragistics.Documents.Excel.CalcEngine.ExcelCalcValue.DateTimeToExcelDate(workbook, (DateTime)value);
				if (excelDate.HasValue)
					return BitConverter.GetBytes(excelDate.Value);

				return new byte[8];
			}

			try
			{
				// MD 4/6/12 - TFS101506
				//double numberValue = (double)Convert.ToDouble( value, CultureInfo.CurrentCulture );
				double numberValue = (double)Convert.ToDouble(value, workbook.CultureResolved);

				return BitConverter.GetBytes( numberValue );
			}
			catch
			{
                Utilities.DebugFail("Error converting cached value.");
				return new byte[ 8 ];
			}
		} 

		#endregion GetCacheValueBytes
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