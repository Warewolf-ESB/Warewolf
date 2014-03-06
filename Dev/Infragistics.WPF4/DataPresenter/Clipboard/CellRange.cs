using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
	#region CellRange
	/// <summary>
	/// Base class for an object that represents a range of cells
	/// </summary>
	internal abstract class CellRange
	{
		#region Properties

		/// <summary>
		/// Returns the number of records in the range
		/// </summary>
		public abstract int RecordCount { get; }

		/// <summary>
		/// Returns the number of fields in the range
		/// </summary>
		public abstract int FieldCount { get; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Returns a boolean indicating if the range has a cell for the specified slot.
		/// </summary>
		/// <param name="recordIndex">The 0 based index of the row from which the cell should be obtained</param>
		/// <param name="fieldIndex">The 0 based index of the column from which the cell should be obtained</param>
		/// <returns>True if the range includes an entry for the specified row/column otherwise false is returned.</returns>
		public abstract bool Exists(int recordIndex, int fieldIndex);

		/// <summary>
		/// Returns a cell for a row/column.
		/// </summary>
		/// <param name="recordIndex">The 0 based index of the row from which the cell should be obtained</param>
		/// <param name="fieldIndex">The 0 based index of the column from which the cell should be obtained</param>
		/// <returns>Returns a <see cref="Cell"/> for a specific row/column offset.</returns>
		public abstract Cell GetCell(int recordIndex, int fieldIndex);

		/// <summary>
		/// Returns a <see cref="Field"/> for a given column.
		/// </summary>
		/// <param name="fieldIndex">A 0 based index of the column whose field is to be returned.</param>
		/// <returns>The field at the specified index</returns>
		public abstract Field GetField(int fieldIndex);

		/// <summary>
		/// Returns a <see cref="DataRecord"/> for a given row index.
		/// </summary>
		/// <param name="recordIndex">A 0 based index of the row whose record is to be returned.</param>
		/// <returns>The record at the specified index</returns>
		public abstract DataRecord GetRecord(int recordIndex);

		// AS 10/21/10 TFS57900
		// Added helper method to get all the fields.
		//
		#region GetFields
		/// <summary>
		/// Returns a new list containing the fields referenced by the range.
		/// </summary>
		/// <returns></returns>
		public Field[] GetFields()
		{
			int fieldCount = this.FieldCount;
			Field[] fields = new Field[fieldCount];

			for (int i = 0; i < fieldCount; i++)
				fields[i] = this.GetField(i);

			return fields;
		} 
		#endregion //GetFields

		#region SetRecord

		internal protected abstract void SetRecord(int recordIndex, DataRecord newRecord);

		#endregion //SetRecord

		#endregion //Methods
	} 
	#endregion //CellRange

	#region GridCellRange
	internal class GridCellRange : CellRange
	{
		#region Member Variables

		private Cell[] _cells;
		private int _fieldCount;
		private int _recordCount;
		private Field[] _fields;
		private DataRecord[] _records;

		#endregion //Member Variables

		#region Constructor
		// AS 10/21/10 TFS57900
		// Since the cells collection may be empty allow the fields to be provided.
		//
		internal GridCellRange(Cell[] cells, Field[] fields) 
			: this(cells, fields.Length)
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			_fields = fields;
		}

		internal GridCellRange(Cell[] cells, int fieldCount)
		{
			GridUtilities.ValidateNotNull(cells);

			Debug.Assert(fieldCount > 0);
			Debug.Assert(cells.Length % fieldCount == 0);
			_fieldCount = fieldCount;
			_recordCount = cells.Length / fieldCount;
			_cells = cells;
		}
		#endregion //Constructor

		#region Properties
		internal Cell[] Cells
		{
			get { return _cells; }
		} 
		#endregion //Properties

		#region Base class overrides

		public override int RecordCount
		{
			get { return _recordCount; }
		}

		public override int FieldCount
		{
			get { return _fieldCount; }
		}

		public override bool Exists(int recordIndex, int fieldIndex)
		{
			return recordIndex >= 0 && recordIndex < _recordCount &&
				fieldIndex >= 0 && fieldIndex < _fieldCount &&
				GetCell(recordIndex, fieldIndex) != null;
		}

		public override Cell GetCell(int recordIndex, int fieldIndex)
		{
			return _cells[recordIndex * _fieldCount + fieldIndex];
		}

		public override Field GetField(int fieldIndex)
		{
			if (null == _fields)
			{
				_fields = new Field[_fieldCount];

				for (int i = 0; i < _fieldCount; i++)
					_fields[i] = GetFieldHelper(i);
			}

			return _fields[fieldIndex];
		}

		private Field GetFieldHelper(int fieldIndex)
		{
			for (int i = 0; i < _recordCount; i++)
			{
				Cell cell = _cells[i * _fieldCount + fieldIndex];

				if (null != cell)
					return cell.Field;
			}

			Debug.Fail("There shouldn't be an empty column slot");
			return null;
		}

		public override DataRecord GetRecord(int recordIndex)
		{
			if (null == _records)
			{
				_records = new DataRecord[_recordCount];

				for (int i = 0; i < _recordCount; i++)
					_records[i] = GetRecordHelper(i);
			}

			return _records[recordIndex];
		}

		private DataRecord GetRecordHelper(int recordIndex)
		{
			for (int i = recordIndex * _fieldCount, end = i + _fieldCount; i < end; i++)
			{
				Cell cell = _cells[i];

				if (null != cell)
					return cell.Record;
			}

			Debug.Fail("There shouldn't be an empty record slot");
			return null;
		}

		protected internal override void SetRecord(int recordIndex, DataRecord newRecord)
		{
			if (null != _records)
			{
				Debug.Assert(recordIndex >= 0 && recordIndex < _records.Length);
				_records[recordIndex] = newRecord;
			}

			int fieldCount = _fieldCount;

			for (int i = recordIndex * fieldCount, end = i + fieldCount; i < end; i++)
			{
				Cell cell = _cells[i];

				if (null != cell)
					_cells[i] = newRecord.Cells[GetField(i % fieldCount)];
			}
		}
		#endregion //Base class overrides
	} 
	#endregion //GridCellRange

	#region RecordCellRange
	internal class RecordCellRange : CellRange
	{
		#region Member Variables

		private DataRecord[] _records;
		private Field[] _fields;

		#endregion //Member Variables

		#region Constructor
		internal RecordCellRange(DataRecord[] records, Field[] fields)
		{
			GridUtilities.ValidateNotNull(records);
			GridUtilities.ValidateNotNull(fields);

			_records = records;
			_fields = fields;
		}
		#endregion //Constructor

		#region Properties
		internal Field[] Fields
		{
			get { return _fields; }
		}

		internal DataRecord[] Records
		{
			get { return _records; }
		}
		#endregion //Properties

		#region Base class overrides
		public override int RecordCount
		{
			get { return _records.Length; }
		}

		public override int FieldCount
		{
			get { return _fields.Length; }
		}

		public override bool Exists(int recordIndex, int fieldIndex)
		{
			return recordIndex >= 0 && recordIndex < RecordCount &&
				fieldIndex >= 0 && fieldIndex < FieldCount;
		}

		public override Cell GetCell(int recordIndex, int fieldIndex)
		{
			DataRecord r = _records[recordIndex];
			Field f = _fields[fieldIndex];
			return r.Cells[f];
		}

		public override Field GetField(int fieldIndex)
		{
			return _fields[fieldIndex];
		}

		public override DataRecord GetRecord(int recordIndex)
		{
			return _records[recordIndex];
		}

		protected internal override void SetRecord(int recordIndex, DataRecord newRecord)
		{
			Debug.Assert(recordIndex >= 0 && recordIndex < _records.Length);
			_records[recordIndex] = newRecord;
		}

		#endregion //Base class overrides
	} 
	#endregion //RecordCellRange
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