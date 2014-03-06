using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region ICellValueProvider interface
	internal interface ICellValueProvider
    {
        int RecordCount { get; }
        int FieldCount { get; }
		FieldLayout FieldLayout { get; }

        Cell GetCell(int recordIndex, int fieldIndex);
        CellValueHolder GetValue(int recordIndex, int fieldIndex);
        CellValueHolder GetValue(Cell cell);
		CellValueHolder GetValue(CellKey cellKey);
		Field GetField(int fieldIndex);
        DataRecord GetRecord(int recordIndex);
		ICellValueProvider GetCurrentCellValues(bool useConvertedValues);

        void CopyTo(Array array, int arrayIndex);
        IEnumerator<CellValueHolder> GetValueEnumerator();
        int ValueCount { get; }

		void ReplaceRecord(int recordIndex, DataRecord newRecord);

		void SetValue(int recordIndex, int fieldIndex, CellValueHolder newValue);
	}
	#endregion //ICellValueProvider interface

	#region CellRangeValueProvider
	internal abstract class CellRangeValueProvider : ICellValueProvider
	{
		#region Member Variables

		private CellRange _range;

		#endregion //Member Variables

		#region Constructor
		protected CellRangeValueProvider(CellRange range)
		{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			_range = range;
		} 
		#endregion //Constructor

		#region Properties
		protected CellRange Range
		{
			get { return _range; }
		}

		public FieldLayout FieldLayout
		{
			get { return this.GetField(0).Owner; }
		}
		#endregion //Properties

		#region ICellValueProvider Members

		public virtual int RecordCount
		{
			get { return _range.RecordCount; }
		}

		public int FieldCount
		{
			get { return _range.FieldCount; }
		}

		public Cell GetCell(int recordIndex, int fieldIndex)
		{
			return _range.GetCell(recordIndex, fieldIndex);
		}

		public abstract CellValueHolder GetValue(int recordIndex, int fieldIndex);

		public abstract CellValueHolder GetValue(Cell cell);

		public abstract CellValueHolder GetValue(CellKey cellKey);

		public Field GetField(int fieldIndex)
		{
			return _range.GetField(fieldIndex);
		}

		public virtual DataRecord GetRecord(int recordIndex)
		{
			return _range.GetRecord(recordIndex);
		}

		public abstract void CopyTo(Array array, int arrayIndex);

		public abstract IEnumerator<CellValueHolder> GetValueEnumerator();

		public abstract int ValueCount
		{
			get;
		}

		public abstract ICellValueProvider GetCurrentCellValues(bool useConvertedValues);

		public void ReplaceRecord(int recordIndex, DataRecord newRecord)
		{
			DataRecord oldRecord = this.GetRecord(recordIndex);

			if (null != oldRecord)
			{
				Debug.Assert(recordIndex >= 0 && recordIndex < _range.RecordCount);
				Debug.Assert(oldRecord.FieldLayout == newRecord.FieldLayout);

				if (oldRecord.FieldLayout == newRecord.FieldLayout)
				{
					_range.SetRecord(recordIndex, newRecord);
					this.OnRecordChanged(oldRecord, newRecord);
				}
			}
		}

		protected abstract void OnRecordChanged(DataRecord oldRecord, DataRecord newRecord);

		public abstract void SetValue(int recordIndex, int fieldIndex, CellValueHolder newValue);

		#endregion
	}
	#endregion //CellRangeValueProvider

	#region GridCellRangeValueProvider class
	internal class GridCellRangeValueProvider : CellRangeValueProvider
	{
		#region Member Variables

		private Dictionary<Cell, int> _valueIndexes;
		private List<CellValueHolder> _values;

		#endregion //Member Variables

		#region Constructor
		// AS 10/21/10 TFS57900
		// Added overloads that take the fields in case the cells collection is empty.
		// The old overload and the new one that takes the fields both call to a helper 
		// ctor that takes the range and did the processing that was done by the sole 
		// ctor previously.
		//
		internal GridCellRangeValueProvider(Cell[] cells, Field[] fields)
			: this(cells, new GridCellRange(cells, fields))
		{
		}

		internal GridCellRangeValueProvider(Cell[] cells, int fieldCount)
			: this(cells, new GridCellRange(cells, fieldCount))
		{
		}

		private GridCellRangeValueProvider(Cell[] cells, GridCellRange range)
			: base(range)
		{
			Dictionary<Cell, int> valueIndexes = new Dictionary<Cell, int>();
			List<CellValueHolder> values = new List<CellValueHolder>();

			for (int i = 0; i < cells.Length; i++)
			{
				Cell c = cells[i];

				if (null != c)
				{
					valueIndexes[c] = values.Count;
					values.Add(new CellValueHolder(null, false));
				}
			}

			_values = values;
			_valueIndexes = valueIndexes;
		}
		#endregion //Constructor

		#region ICellValueProvider Members

		public override CellValueHolder GetValue(int recordIndex, int fieldIndex)
		{
			Cell cell = GetCell(recordIndex, fieldIndex);

			if (null == cell)
				return null;

			return GetValue(cell);
		}

		public override CellValueHolder GetValue(CellKey cellKey)
		{
			return GetValue(cellKey.Cell);
		}

		public override CellValueHolder GetValue(Cell cell)
		{
			int index;

			if (!_valueIndexes.TryGetValue(cell, out index))
				throw new ArgumentException();

			return _values[index];
		}

		public override void CopyTo(Array array, int arrayIndex)
		{
			((ICollection)_values).CopyTo(array, arrayIndex);
		}

		public override IEnumerator<CellValueHolder> GetValueEnumerator()
		{
			return _values.GetEnumerator();
		}

		public override int ValueCount
		{
			get { return _values.Count; }
		}

		public override ICellValueProvider GetCurrentCellValues(bool useConvertedValues)
		{
			GridCellRange range = (GridCellRange)this.Range;
			Cell[] cells = range.Cells;

			Predicate<Cell> findTemplate = delegate(Cell cell)
			{
				return cell != null && cell.Record != null && cell.Record.IsAddRecordTemplate;
			};

			// if there are any template add records then we need to remove them
			int templateIndex = Array.FindIndex(cells, findTemplate);

			if (templateIndex >= 0)
			{
				List<Cell> dataRecordCells = new List<Cell>(cells);
				dataRecordCells.RemoveAll(findTemplate);
				cells = dataRecordCells.ToArray();
			}

			GridCellRangeValueProvider newValues = new GridCellRangeValueProvider(cells, range.GetFields());

			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];

				if (null != cell)
				{
					object value = useConvertedValues ? cell.ConvertedValue : cell.Value;
					newValues.GetValue(cell).Initialize(value, false);
				}
			}

			return newValues;
		}

		protected override void OnRecordChanged(DataRecord oldRecord, DataRecord newRecord)
		{
			int valueIndex;
			for (int i = 0, count = this.FieldCount; i < count; i++)
			{
				Field field = this.GetField(i);
				Cell cell = oldRecord.GetCellIfAllocated(field);

				if (null != cell && _valueIndexes.TryGetValue(cell, out valueIndex))
				{
					_valueIndexes.Remove(cell);
					_valueIndexes[newRecord.Cells[field]] = valueIndex;
				}
			}
		}

		public override void SetValue(int recordIndex, int fieldIndex, CellValueHolder newValue)
		{
			Cell cell = GetCell(recordIndex, fieldIndex);

			if (cell == null)
			{
				Debug.Assert(null == newValue);
				return;
			}

			Debug.Assert(null != newValue);

			int index;

			if (_valueIndexes.TryGetValue(cell, out index))
				_values[index] = newValue;
		}

		#endregion //ICellValueProvider Members
	}
	#endregion //GridCellRangeValueProvider class

	#region RecordCellRangeValueProvider class
	internal class RecordCellRangeValueProvider : CellRangeValueProvider
	{
		#region Member Variables

		private RecordCellRange _range;
		private Dictionary<CellKey, int> _valueIndexes;
		private CellValueHolder[] _values;
		private int _rowCount;

		#endregion //Member Variables

		#region Constructor
		internal RecordCellRangeValueProvider(DataRecord[] records, Field[] fields) : this(records, fields, null, false)
		{
		}

		internal RecordCellRangeValueProvider(DataRecord[] records, Field[] fields, CellValueHolder[,] values, bool keepExtraRowData)
			: base(new RecordCellRange(records, fields))
		{
			_range = (RecordCellRange)this.Range;
			int count = _range.FieldCount * _range.RecordCount;
			CellValueHolder[] cellValues;

			if (null == values)
			{
				cellValues = new CellValueHolder[count];

				for (int i = 0; i < count; i++)
					cellValues[i] = new CellValueHolder(null, false);

				_rowCount = _range.RecordCount;
			}
			else
			{
				int valueRowCount, valueColumnCount;
				ClipboardOperationInfo.GetRowColumnCount(values, out valueRowCount, out valueColumnCount);

				Debug.Assert(valueRowCount >= _range.RecordCount);
				Debug.Assert(valueColumnCount >= _range.FieldCount);

				int actualRowCount = (keepExtraRowData ? valueRowCount : _range.RecordCount);
				int actualColumnCount = _range.FieldCount;
				int valueCount = actualColumnCount * actualRowCount;

				cellValues = new CellValueHolder[valueCount];
				int index = 0;

				for (int r = 0; r < actualRowCount; r++)
				{
					for (int c = 0; c < actualColumnCount; c++)
					{
						cellValues[index++] = values[r, c];
					}
				}

				_rowCount = actualRowCount;
			}

			_values = cellValues;
		}
		#endregion //Constructor

		#region Methods
		private void CreateValueIndexes()
		{
			_valueIndexes = new Dictionary<CellKey, int>();
			int fieldCount = _range.FieldCount;
			int i = 0;

			for (int r = 0, rCount = _range.RecordCount; r < rCount; r++)
			{
				DataRecord record = _range.GetRecord(r);

				for (int c = 0; c < FieldCount; c++)
				{
					_valueIndexes[new CellKey(record, _range.GetField(c))] = i++;
				}
			}
		} 
		#endregion //Methods

		#region ICellValueProvider Members

		public override int RecordCount
		{
			get
			{
				return _rowCount;
			}
		}

		public override DataRecord GetRecord(int recordIndex)
		{
			if (recordIndex < _range.RecordCount)
				return base.GetRecord(recordIndex);

			return null;
		}

		public override CellValueHolder GetValue(int recordIndex, int fieldIndex)
		{
			return _values[recordIndex * _range.FieldCount + fieldIndex];
		}

		public override CellValueHolder GetValue(Cell cell)
		{
			CellKey key = new CellKey(cell);

			return GetValue(key);
		}

		public override CellValueHolder GetValue(CellKey cellKey)
		{
			if (_valueIndexes == null)
			{
				this.CreateValueIndexes();
			}

			int index = _valueIndexes[cellKey];
			return _values[index];
		}

		public override void CopyTo(Array array, int arrayIndex)
		{
			_values.CopyTo(array, arrayIndex);
		}

		public override IEnumerator<CellValueHolder> GetValueEnumerator()
		{
			return ((IEnumerable<CellValueHolder>)_values).GetEnumerator();
		}

		public override int ValueCount
		{
			get { return _values.Length; }
		}

		public override ICellValueProvider GetCurrentCellValues(bool useConvertedValues)
		{
			RecordCellRange range = (RecordCellRange)this.Range;
			DataRecord[] records = range.Records;
			Field[] fields = range.Fields;

			Predicate<DataRecord> findTemplate = delegate(DataRecord record)
			{
				return record.IsAddRecordTemplate;
			};

			// if there are any template add records then we need to remove them
			int templateIndex = Array.FindIndex(records, findTemplate);

			if (templateIndex >= 0)
			{
				List<DataRecord> dataRecords = new List<DataRecord>(records);
				dataRecords.RemoveAll(findTemplate);
				records = dataRecords.ToArray();
			}

			RecordCellRangeValueProvider newValues = new RecordCellRangeValueProvider(records, fields);

			for (int r = 0; r < records.Length; r++)
			{
				DataRecord rcd = records[r];

				for (int c = 0; c < fields.Length; c++)
				{
					CellValueHolder cvh = newValues.GetValue(r, c);

					Debug.Assert(null != cvh);

					if (null != cvh)
					{
						object value = rcd.GetCellValue(fields[c], useConvertedValues);
						cvh.Initialize(value, false);
					}
				}
			}

			return newValues;
		}

		protected override void OnRecordChanged(DataRecord oldRecord, DataRecord newRecord)
		{
			int valueIndex;
			for (int i = 0, count = this.FieldCount; i < count; i++)
			{
				Field field = this.GetField(i);
				CellKey cellKey = new CellKey(oldRecord, field);

				if (_valueIndexes.TryGetValue(cellKey, out valueIndex))
				{
					_valueIndexes.Remove(cellKey);
					_valueIndexes[new CellKey(newRecord, field)] = valueIndex;
				}
			}
		}

		public override void SetValue(int recordIndex, int fieldIndex, CellValueHolder newValue)
		{
			_values[recordIndex * _range.FieldCount + fieldIndex] = newValue;
		}
		#endregion // ICellValueProvider
	}
	#endregion //RecordCellRangeValueProvider class
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