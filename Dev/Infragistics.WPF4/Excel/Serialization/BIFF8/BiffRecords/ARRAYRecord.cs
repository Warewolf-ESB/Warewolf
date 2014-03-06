using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class ARRAYRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
                Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			ushort firstRow = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			ushort lastRow = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			byte firstColumn = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );
			byte lastColumn = manager.CurrentRecordStream.ReadByteFromBuffer( ref data, ref dataIndex );

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			bool recalculateAlways =	( optionFlags & 0x0001 ) == 0x0001;
			//bool calculateOnOpen =		( optionFlags & 0x0002 ) == 0x0002;

			manager.CurrentRecordStream.ReadUInt32FromBuffer( ref data, ref dataIndex ); // Not used

			ArrayFormula formula = (ArrayFormula)Formula.Load( manager.CurrentRecordStream, FormulaType.ArrayFormula, ref data, ref dataIndex );

			Debug.Assert( formula.RecalculateAlways == recalculateAlways );

			// MD 8/21/08 - Excel formula solving
			//formula.ApplyTo( new WorksheetRegion( worksheet, firstRow, firstColumn, lastRow, lastColumn ) );
			formula.ApplyTo( worksheet.GetCachedRegion( firstRow, firstColumn, lastRow, lastColumn ) );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 4/18/11 - TFS62026
			// The cell value is not on the context stack anymore. Instead, there is a CellContext which holds 
			// multiple pieces of information about the cell.
			//ArrayFormula formula = (ArrayFormula)manager.ContextStack[ typeof( ArrayFormula ) ];
			CellContext cellContext = (CellContext)manager.ContextStack[typeof(CellContext)];
			if (cellContext == null)
			{
				Utilities.DebugFail("There was no cell context in the context stack.");
				return;
			}

			ArrayFormula formula = cellContext.Value as ArrayFormula;

			if ( formula == null )
			{
                Utilities.DebugFail("There was no array formula in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)formula.CellRange.FirstRow );
			manager.CurrentRecordStream.Write( (ushort)formula.CellRange.LastRow );
			manager.CurrentRecordStream.Write( (byte)formula.CellRange.FirstColumn );
			manager.CurrentRecordStream.Write( (byte)formula.CellRange.LastColumn );

			ushort optionFlags = 0;

			if ( formula.RecalculateAlways )
				optionFlags |= 0x0001;

			manager.CurrentRecordStream.Write( (ushort)optionFlags );
			manager.CurrentRecordStream.Write( (uint)0 );

			formula.Save(manager.CurrentRecordStream, true, false );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.ARRAY; }
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