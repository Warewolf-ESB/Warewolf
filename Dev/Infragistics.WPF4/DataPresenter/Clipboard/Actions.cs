using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using System.Windows.Controls;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace Infragistics.Windows.DataPresenter
{
    #region DataPresenterAction
    /// <summary>
    /// Custom action associated with a specific <see cref="FieldLayout"/>
    /// </summary>
    internal abstract class DataPresenterAction : ActionHistory.ActionBase
    {
        #region Member Variables

        private DataPresenterBase _dp;

        #endregion //Member Variables

        #region Constructor
		internal DataPresenterAction(DataPresenterBase dp)
        {
            GridUtilities.ValidateNotNull(dp, "dp");
            _dp = dp;
        }
        #endregion //Constructor

        #region Properties

        #region DataPresenter
        /// <summary>
        /// Returns the associated <see cref="DataPresenterBase"/> control
        /// </summary>
        public DataPresenterBase DataPresenter
        {
            get { return _dp; }
        }
        #endregion //DataPresenter

        #endregion //Properties

		#region Methods

		#region AddUndeleteDescendantInfo
		internal protected virtual void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
		}
		#endregion //AddUndeleteDescendantInfo

		#region CanPerformInternal

		/// <summary>
		/// These methods are for use by the <see cref="DataPresenterCompositeAction"/>
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool CanPerformInternal(object context)
		{
			return this.CanPerform(context);
		}

		#endregion //CanPerformInternal

		#region IsValid
		protected static bool IsValid(Field field)
		{
			return null != field &&
				field.Owner != null &&
				field.Index >= 0;
		} 
		#endregion //IsValid

		#region GetDataItemRecord
		internal static DataRecord GetDataItemRecord(Record record)
		{
			if (record == null)
				return null;

			DataRecord dr = record as DataRecord;

			if (null != dr)
			{
				// filter records don't have a data item. use the data item
				// of the associated record manager
				FilterRecord fr = dr as FilterRecord;

				if (fr != null)
					return GetDataItemRecord(fr.RecordManager);
				
				// JJD 10/26/11 - TFS91364 
				// Ignore HeaderRecords
				HeaderRecord hr = dr as HeaderRecord;

				if (hr != null)
					return null;

				return dr;
			}

			ExpandableFieldRecord efr = record as ExpandableFieldRecord;

			if (null != efr)
				return GetDataItemRecord(efr.ParentRecord);

			if (record is GroupByRecord)
				return null;

			Debug.Fail("Unrecognized record type:" + record.ToString());
			return null;
		}

		internal static DataRecord GetDataItemRecord(RecordManager rm)
		{
			return null != rm ? rm.ParentDataRecord : null;
		} 
		#endregion //GetDataItemRecord

		#region GetRecordDataItem
		internal static object GetRecordDataItem(Record record)
		{
			DataRecord dataItemRecord = GetDataItemRecord(record);

			if (null == dataItemRecord)
				return null;

			return dataItemRecord.DataItem;
		}

		internal static object GetRecordDataItem(RecordManager rm)
		{
			return GetRecordDataItem(GetDataItemRecord(rm));
		}
		#endregion //GetRecordDataItem

		#region OnCustomizationsChanged

		/// <summary>
		/// Used to obtain the action, if any, that should be kept in the undo/redo stack after the following changes are applied.
		/// </summary>
		/// <param name="fieldLayout">The field layout whose customizations are being affected.</param>
		/// <param name="customizationType">The type of customization(s) being reset.</param>
		/// <returns>The action to place in the undo/redo stack or null if the action should be removed.</returns>
		internal virtual DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			return this;
		}

		#endregion //OnCustomizationsChanged

		#region OnRecordsUndeleted
		internal protected virtual void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
		}
		#endregion //OnRecordsUndeleted

		#region PerformInternal

		/// <summary>
		/// These methods are for use by the <see cref="DataPresenterCompositeAction"/>
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal ActionHistory.ActionBase PerformInternal(object context)
		{
			return this.Perform(context);
		}

		#endregion //PerformInternal

		#region ProcessRecordUndeleted
		internal static bool ProcessRecordUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords,
			ref object dataItem, ref Record record)
		{
			DataRecord newRecord;

			if (null != dataItem && oldDataTimeToRecords.TryGetValue(dataItem, out newRecord))
			{
				if (record is FilterRecord)
				{
					RecordManager rm = record.RecordManager;
					ExpandableFieldRecord oldParentRecord = null != rm ? rm.ParentRecord : null;

					if (null == oldParentRecord)
						return false;

					Field field = rm.Field;

					Debug.Assert(field != null);

					if (!IsValid(field))
						return false;

					int index = newRecord.ChildRecords.IndexOf(field);

					if (index < 0)
						return false;

					ExpandableFieldRecord parentRecord = newRecord.ChildRecords[index];

					FilterRecord fr = parentRecord.ChildRecordManager.ViewableRecords.GetSpecialRecord(RecordType.FilterRecord) as FilterRecord;

					if (null == fr || fr.FieldLayout != record.FieldLayout)
						return false;

					record = fr;
				}
				else if (record is DataRecord)
				{
					if (newRecord.FieldLayout != record.FieldLayout)
						return false;

					record = newRecord;
				}
				else if (record is ExpandableFieldRecord)
				{
					ExpandableFieldRecord efRecord = (ExpandableFieldRecord)record;
					Debug.Assert(null != efRecord.Field);

					int index = null != efRecord.Field
						? newRecord.ChildRecords.IndexOf(efRecord.Field)
						: -1;

					if (index < 0)
						return false;

					record = newRecord.ChildRecords[index];
				}
				else if (record is GroupByRecord)
				{
					return false;
				}
				else
				{
					Debug.Fail("Unexpected record type:" + record.ToString());
					return false;
				}

				dataItem = GetRecordDataItem(record);
				return true;
			}

			return false;
		}
		#endregion //ProcessRecordUndeleted

		#region SelectCells
		internal static void SelectCells(DataPresenterBase dp, List<CellKey> modifiedCells, bool scrollIntoView, bool activateFirstCell)
		{
			List<Cell> cells = new List<Cell>();

			foreach (CellKey cellKey in modifiedCells)
			{
				// if a record was deleted during the paste/undo/redo then skip
				// it in the selection
				if (ClipboardOperationInfo.IsDeleted(cellKey.Record))
					continue;

				Cell cell = cellKey.Cell;

				if (cell.IsSelectable)
					cells.Add(cell);
			}

			if (cells.Count == 0)
				return;

			FieldLayout fl = cells[0].Field.Owner;
			Debug.Assert(null != fl);

			if (null == fl)
				return;

			int maxSelectionCount = fl.MaxSelectedCellsResolved;

			if (maxSelectionCount < cells.Count)
				cells.RemoveRange(maxSelectionCount, cells.Count - maxSelectionCount);

			DataPresenterBase.SelectedItemHolder selected = new DataPresenterBase.SelectedItemHolder(dp);
			selected.Cells.InternalAddRange(cells);

			// if the selection is updated...
			if (dp.SelectNewSelection(typeof(Cell), selected))
			{
				if (activateFirstCell)
					cells[0].IsActive = true;

				
				
				
				
				
				
				
				
				
				
				
				
				if (scrollIntoView && null != dp.ActiveCell)
				{
					dp.BringCellIntoView(cells[0]);
				}
			}
		}
		#endregion //SelectCells

		#region SelectFields
		internal static void SelectFields(DataPresenterBase dp, List<CellKey> modifiedCells, 
			ICellValueProvider values, bool scrollIntoView, bool activateFirstCell)
		{
			List<Field> fields = new List<Field>();

			for (int i = 0, count = values.FieldCount; i < count; i++)
			{
				Field field = values.GetField(i);

				if (!IsValid(field))
					continue;
				
				fields.Add(field);
			}

			if (fields.Count == 0)
				return;

			FieldLayout fl = fields[0].Owner;
			Debug.Assert(null != fl);

			if (null == fl)
				return;

			DataPresenterBase.SelectedItemHolder selected = new DataPresenterBase.SelectedItemHolder(dp);
			selected.Fields.InternalAddRange(fields);
			selected.Records.InternalClear();
			selected.Cells.InternalClear();

			// if the selection is updated...
			if (dp.SelectNewSelection(typeof(Field), selected))
			{
				Cell cellToActivate = null;

				if (activateFirstCell || scrollIntoView)
				{
					foreach (CellKey cellKey in modifiedCells)
					{
						DataRecord record = cellKey.Record;

						if (!record.IsEnabledResolved)
							continue;

						if (!record.IsStillValid)
							continue;

						cellToActivate = cellKey.Cell;
						break;
					}
				}

				if (null != cellToActivate)
				{
					if (activateFirstCell)
						cellToActivate.IsActive = true;

					if (scrollIntoView)
						dp.BringCellIntoView(cellToActivate);
				}
			}
		} 
		#endregion //SelectFields

		#region SelectRecords
		internal static void SelectRecords(DataPresenterBase dp, List<CellKey> modifiedCells, bool scrollIntoView, bool activateFirstRecord)
		{
			List<DataRecord> records = new List<DataRecord>();
			DataRecord previousRecord = null;

			foreach (CellKey cellKey in modifiedCells)
			{
				DataRecord record = cellKey.Record;

				if (previousRecord == record)
					continue;

				previousRecord = record;

				records.Add(record);
			}

			SelectRecords(dp, records, scrollIntoView, activateFirstRecord);
		}

		internal static void SelectRecords(DataPresenterBase dp, IList<DataRecord> recordsToSelect, bool scrollIntoView, bool activateFirstRecord)
		{
			List<Record> records = new List<Record>();

			for (int i = 0, count = recordsToSelect.Count; i < count; i++)
			{
				DataRecord record = recordsToSelect[i];

				if (record == null)
					continue;

				if (ClipboardOperationInfo.IsDeleted(record))
					continue;

				if (!record.IsSelectable)
					continue;

				records.Add(record);
			}

			if (records.Count == 0)
				return;

			FieldLayout fl = records[0].FieldLayout;
			Debug.Assert(null != fl);

			if (null == fl)
				return;

			int maxSelectionCount = fl.MaxSelectedRecordsResolved;

			if (maxSelectionCount < records.Count)
				records.RemoveRange(maxSelectionCount, records.Count - maxSelectionCount);

			DataPresenterBase.SelectedItemHolder selected = new DataPresenterBase.SelectedItemHolder(dp);
			selected.Records.InternalAddRange(records);

			// if the selection is updated...
			if (dp.SelectNewSelection(typeof(Record), selected))
			{
				if (activateFirstRecord)
				{
					dp.ActiveCell = null;
					records[0].IsActive = true;
				}

				if (scrollIntoView && null != dp.ActiveRecord)
				{
					dp.BringRecordIntoView(dp.ActiveRecord);
				}
			}
		}
		#endregion //SelectRecords

		#endregion //Methods
	} 
    #endregion //DataPresenterAction

	#region DataRecordAction
	/// <summary>
	/// Custom action associated with a specific <see cref="DataRecord"/>
	/// </summary>
	internal abstract class DataRecordAction : DataPresenterAction
	{
		#region Member Variables

		private DataRecord _record;
		private object _recordDataItem;

		#endregion //Member Variables

		#region Constructor
		internal DataRecordAction(DataRecord record)
			: base(record.DataPresenter)
		{
			GridUtilities.ValidateNotNull(record);
			_record = record;
			_recordDataItem = GetRecordDataItem(record);
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the record associated with the action.
		/// </summary>
		public DataRecord Record
		{
			get { return _record; }
		}

		/// <summary>
		/// Returns the cached <see cref="DataRecord.DataItem"/>
		/// </summary>
		protected object OriginalDataItem
		{
			get { return _recordDataItem; }
		}

		#endregion //Properties

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		internal protected override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			descendantInfo.AddRecord(this.Record);
		}
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			if (ClipboardOperationInfo.IsDeleted(_record))
				return false;

			// validate the original data item if this is an undo/redo
			if (UndoManager.IsUndoRedoContext(context) &&
				!ClipboardOperationInfo.IsSameDataItem(_recordDataItem, GetRecordDataItem(_record)))
				return false;

			return true;
		}
		#endregion //CanPerform

		#region OnRecordsUndeleted
		protected internal override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			Record newRecord = _record;

			if (ProcessRecordUndeleted(oldDataTimeToRecords, ref _recordDataItem, ref newRecord))
				_record = (DataRecord)newRecord;
		}
		#endregion //OnRecordsUndeleted

		#endregion //Base class overrides

		#region Methods

		#region InitializeOriginalDataItem
		protected void InitializeOriginalDataItem()
		{
			Debug.Assert(null == _recordDataItem || ClipboardOperationInfo.IsSameDataItem(_recordDataItem, GetRecordDataItem(_record)));
			_recordDataItem = GetRecordDataItem(_record);
		}
		#endregion //InitializeOriginalDataItem

		#endregion //Methods
	}
	#endregion //DataRecordAction

    #region SingleCellAction
    /// <summary>
    /// Custom action associated with a specific <see cref="Cell"/>
    /// </summary>
    internal abstract class SingleCellAction : DataRecordAction
    {
        #region Member Variables

		private Field _field;

        #endregion //Member Variables

        #region Constructor
        internal SingleCellAction(DataRecord record, Field field)
            : base(record)
        {
            GridUtilities.ValidateNotNull(field);
			_field = field;
        }
        #endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			if (!base.CanPerform(context))
				return false;

			if (this.Record.FieldLayout != _field.Owner)
				return false;

			return IsValid(_field);
		}
		#endregion //CanPerform

		#endregion //Base class overrides

		#region Properties
		public Field Field
		{
			get { return _field; }
		} 
		#endregion //Properties
    } 
    #endregion //SingleCellAction

    #region SingleCellValueAction
    /// <summary>
    /// Custom action used to change the value of a single <see cref="Cell"/>
    /// </summary>
    internal class SingleCellValueAction : SingleCellAction
    {
        #region Member Variables

        private object _value;
        private bool _useConverter;
		private object _preferredOriginalValue = DataRecord.UndoValueMissing;
		private bool _isCommittingEdit;

        #endregion //Member Variables

        #region Constructor
        internal SingleCellValueAction(object value, bool useConverter, DataRecord record, Field field, object preferredOriginalValue, bool isCommittingEdit)
            : base(record, field)
        {
            _value = value;
            _useConverter = useConverter;
			_preferredOriginalValue = preferredOriginalValue;
			_isCommittingEdit = isCommittingEdit;
        }
        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Returns the value associated with the action.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }
        #endregion //Properties

        #region Base class overrides

		#region OnCustomizationsChanged

		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			FilterRecord fr = this.Record as FilterRecord;

			// if this cell values are for a filter record whose filters are coming
			// from the field layout then we want to release this action when 
			// the customizations change
			if (null != fr &&
				fr.FieldLayout == fieldLayout &&
				fr.Filters.GetRecordFilterScopeResolved(fieldLayout) == RecordFilterScope.AllRecords)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}

		#endregion //OnCustomizationsChanged

        #region Perform
        internal protected override ActionHistory.ActionBase Perform(object context)
        {
            DataPresenterAction undoAction = null;

			// AS 8/20/09 TFS20920
			//if (this.CanPerform(context))
			if (!this.CanPerform(context))
				return null;

			DataRecord record = this.Record;

			try
			{
				// AS 8/20/09 TFS20920
				// The record may be filtered in but when the value is changed, it could 
				// become filtered out before we get a chance to activate the record. To 
				// avoid this we'll suspend filtering on the record while we process the 
				// operation.
				//
				record.SuspendFiltering();

				Field field = this.Field;
				DataPresenterBase dp = this.DataPresenter;

				// we shouldn't need to use the converter for the undo even if we used 
				// it to perform the action. as long as we are consistent about what we
				// pass along
				bool useConverterForUndo = MultipleCellValuesAction.UseConverterForUndo;
				object oldValue = record.GetCellValue(field, useConverterForUndo);

				// AS 5/22/09
				// Some cells (like the filter cell) will actually update their value while 
				// they are in edit mode and so we cannot use the current value of the cell
				// as the undo value for the operation we are about to perform. Note when 
				// creating the undo action for this action we will never provide the undo 
				// value but will use the current value at that time. If we did need to provide 
				// the value we're about to change the cell to then we would want to pass
				// along the boolean indicating if the new value needs to be converted. Right 
				// now we don't need that. The editor value is a converted value.
				//
				if (_preferredOriginalValue != DataRecord.UndoValueMissing)
				{
					oldValue = _preferredOriginalValue;

					// the editor may have stored null and not dbnull but the underlying
					// dataitem may expect dbnull so adjust the edit value as needed
					record.CoerceNullEditValue(field, ref oldValue);

					useConverterForUndo = true;
				}

				bool wasTemplateAdd = record.IsAddRecordTemplate;

				// AS 8/20/09 TFS19091
				// We should try to update the cell value before forcing the template record to be created 
				// or marking the record as dirty so I moved this up from below.
				//
				// try and set the cell value. if it wasn't set then do not provide an undo action
				// unless we created a new record in which case we still need the delete action
				if (false == record.SetCellValue(field, _value, _useConverter, false))
				{
					// AS 8/20/09 TFS19091
					// Since we haven't done anything yet we can return null if the set fails.
					//
					//if (!wasTemplateAdd)
					//	undoAction = null;
					return null;
				}

				// make sure the underlying record is created. if that fails or is cancelled
				// then we cannot proceed with the action
				if (!record.OnEditValueChanged(false))
					return null;

				// if this is just being performed for the first time then cache the data item
				bool isUndoRedo = UndoManager.IsUndoRedoContext(context);

				if (!isUndoRedo)
					this.InitializeOriginalDataItem();

				// if this was a template record then we need to delete the record to undo it
				undoAction = wasTemplateAdd
					? new DeleteRecordsAction(dp, new Record[] { record }, false)
					: (DataPresenterAction)new SingleCellValueAction(oldValue, useConverterForUndo, record, field, DataRecord.UndoValueMissing, false);

				// if this action is the action used to commit an edit then we want to also store 
				// the FieldResizeInfo action so that we can restore the extents of the fields if 
				// it were changed while in edit mode
				if (_isCommittingEdit)
					undoAction = this.DataPresenter.EditHelper.CreateCommitEditUndoAction(undoAction);

				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


				// in an undo/redo situation we should activate the cell
				if (isUndoRedo && null != undoAction)
				{
					if (null != dp)
					{
						// clear the selection
						dp.SelectedItems.ClearAllSelected();

						dp.ActiveCell = record.Cells[field];
					}
				}
			}
			finally
			{
				// AS 8/20/09 TFS20920
				ResumeRecordFiltering(record);
			}

            return undoAction;
        } 
        #endregion //Perform

		#region ToString
		public override string ToString()
		{
			return string.Format("SingleCellValue Field:'{0}' Record:'{1}'", this.Field, this.Record);
		} 
		#endregion //ToString

        #endregion //Base class overrides

		#region Methods

		// AS 8/20/09 TFS20920
		#region ResumeRecordFiltering
		internal static void ResumeRecordFiltering(Record record)
		{
			// We want to avoid evaluating the filter if the record is active unless 
			// its already filtered out in which case the change made by this operation 
			// may actually make it filtered in so force the evaluation now.
			//
			record.ResumeFiltering(!record.IsActive || record.InternalIsFilteredOut_NoVerify);

			// we need to make sure that this record has its filter processing suspended if 
			// (and while) its active
			record.SuspendActiveRecordFiltering();
		}
		#endregion //ResumeRecordFiltering

		#endregion //Methods
	} 
    #endregion //SingleCellValueAction

    #region MultipleCellValuesAction
    /// <summary>
    /// Custom action used to change the value of multiple cells.
    /// </summary>
    internal class MultipleCellValuesAction : DataPresenterAction
    {
        #region Member Variables

		private ICellValueProvider _values;
		private bool _useConverter;
		private SelectionTarget _preferSelectionTarget;
		private List<object> _originalDataItems;

		internal const bool UseConverterForUndo = false;

        #endregion //Member Variables

        #region Constructor
		internal MultipleCellValuesAction(ICellValueProvider valueProvider, bool useConverter, SelectionTarget preferSelectionTarget)
            : base(valueProvider.FieldLayout.DataPresenter)
        {
			_values = valueProvider;
			_useConverter = useConverter;
			_preferSelectionTarget = preferSelectionTarget;

			this.StoreOriginalDataItems();
        }
		#endregion //Constructor

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		internal protected override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			for (int i = 0, count = _values.RecordCount; i < count; i++)
			{
				Record record = _values.GetRecord(i);
				descendantInfo.AddRecord(record);
			}
		} 
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected override bool CanPerform(object context)
        {
			bool hasRecords = false;

			for (int r = 0, rCount = _values.RecordCount; r < rCount; r++)
			{
				DataRecord record = _values.GetRecord(r);

				if (false == ClipboardOperationInfo.IsDeleted(record))
				{
					hasRecords = true;
					break;
				}
			}

			bool hasFields = false;

			for (int c = 0, cCount = _values.FieldCount; c < cCount; c++)
			{
				Field field = _values.GetField(c);

				if (IsValid(field))
				{
					hasFields = true;
					break;
				}
			}

			// if we have cells we can undo then return true
			return hasRecords && hasFields;
        }
        #endregion //CanPerform

		#region OnCustomizationsChanged

		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			FilterRecord fr = _values.GetRecord(0) as FilterRecord;

			// if this cell values are for a filter record whose filters are coming
			// from the field layout then we want to release this action when 
			// the customizations change
			if (null != fr && 
				fr.FieldLayout == fieldLayout && 
				fr.Filters.GetRecordFilterScopeResolved(fieldLayout) == RecordFilterScope.AllRecords)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}

		#endregion //OnCustomizationsChanged

		#region OnRecordsUndeleted
		internal protected override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			for (int i = 0, count = _originalDataItems.Count; i < count; i++)
			{
				object oldDataItem = _originalDataItems[i];

				if (null == oldDataItem)
					continue;

				Record record = _values.GetRecord(i);

				if (ProcessRecordUndeleted(oldDataTimeToRecords, ref oldDataItem, ref record))
				{
					Debug.Assert(record is DataRecord);

					_values.ReplaceRecord(i, (DataRecord)record);
					_originalDataItems[i] = GetRecordDataItem(record);
				}
			}
		}
		#endregion //OnRecordsUndeleted

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!this.CanPerform(context))
				return null;

			DataPresenterBase dp = this.DataPresenter;

			if (null == dp)
				return null;

			int rCount = _values.RecordCount;

			if (rCount < 1)
				return null;

			// we shouldn't need to use the converter for the undo even if we used 
			// it to perform the action. as long as we are consistent about what we
			// pass along
			bool useConverterForUndo = UseConverterForUndo;

			// get a cellvalueprovider that has the values for the undo operation
			ICellValueProvider previousValues = _values.GetCurrentCellValues(useConverterForUndo);

			// hold onto the undo action and return it if we successfully perform the action
			MultipleCellValuesAction undoAction = new MultipleCellValuesAction(previousValues, useConverterForUndo, _preferSelectionTarget);

			bool isUndoRedo = UndoManager.IsUndoRedoContext(context);
			List<CellKey> modifiedCells = new List<CellKey>();
			List<CellKey> newModifiedCells = new List<CellKey>();
			int cCount = _values.FieldCount;
			bool[] avoidFieldChange = new bool[cCount];
			ClipboardOperation? operation = ClipboardOperationInfo.GetClipboardOperation(context);

			if (!isUndoRedo)
			{
				for (int i = 0; i < cCount; i++)
				{
					Field f = _values.GetField(i);
					avoidFieldChange[i] = ClipboardOperationInfo.ShouldSkipFieldModification(f);
				}
			}

			// ensure the values represent cell values. this will also raise an error
			// if it encounters a conversion error
			RowColumnIndex stopAtIndex;
			bool checkEditableCells = true;

			RecordManager rm = _values.GetRecord(0).RecordManager;

			bool hadConversionErrors;
			bool converted = ClipboardOperationInfo.ConvertToCellValue(_values, _originalDataItems, avoidFieldChange, checkEditableCells, out stopAtIndex, operation, rm, out hadConversionErrors);

			// if the conversion was not performed or was reverted then null
			if (!converted)
				return null;

			bool stopProcessing = false;
			ClipboardOperationInfo clipInfo = dp.ClipboardOperationInfo;
			ClipboardErrorAction? errorAction = null;
			bool skippedCells = false;
			bool updateOnCellChange = false;
			bool updateOnRecordChange = false;

			switch (dp.UpdateMode)
			{
				case UpdateMode.OnCellChange:
				case UpdateMode.OnCellChangeOrLostFocus:
					updateOnCellChange = true;
					break;
				case UpdateMode.OnRecordChange:
				case UpdateMode.OnRecordChangeOrLostFocus:
					updateOnRecordChange = true;
					break;
			}

			bool activateFirstCell = false;

			if (operation == ClipboardOperation.Paste || isUndoRedo)
			{
				if (ShouldActivateFirstCell(_values, dp.ActiveCell))
					activateFirstCell = true;
			}

			List<DataRecord> newRecords = new List<DataRecord>();

			// AS 8/20/09 TFS20920
			// If this is a single record operation then suspend filtering while
			// we perform the cell value changes.
			//
			Record recordWithSuspendedFilter = rCount == 1 ? _values.GetRecord(0) : null;

			if (recordWithSuspendedFilter != null)
				recordWithSuspendedFilter.SuspendFiltering();

			try
			{
				for (int r = 0; r < rCount; r++)
				{
					DataRecord record = _values.GetRecord(r);
					int modifiedCount = modifiedCells.Count;

					if (record == null && rm != null)
					{
						record = rm.CurrentAddRecord;

						// we can only do this when the record is a template record. when its not 
						// its likely that we had an add record, tried to update it but it failed 
						// for one reason or another (e.g. constraint failure)
						if (null != record && record.IsAddRecordTemplate == false)
							continue;
					}

					Debug.Assert(null != record);

					bool isAddRecord = null != record && record.IsAddRecord;
					bool isAddRecordTemplate = null != record && record.IsAddRecordTemplate;

					// AS 1/6/12 TFS29076
					// Do not manipulate the template addrecord or a new data record would get 
					// created. This doesn't make sense when clearing or cutting the cells of 
					// a template add record.
					//
					if (isAddRecordTemplate && operation == ClipboardOperation.ClearContents)
						continue;

					bool wasInitializeSuspended = dp.SuspendInitializeRecordFor(record);
					bool modifiedRecord = false;
					// JJD 11/17/11 - TFS78651 
					// Added sortValueChanged parameter
					bool sortvalueChanged = false;

					// AS 8/20/09 TFS20920
					// Added try/finally to ensure we resume initialize record.
					//
					try
					{
						#region Process Cells
						for (int c = 0; c < cCount; c++)
						{
							if (null != stopAtIndex && stopAtIndex.Equals(r, c))
							{
								skippedCells = true;
								stopProcessing = true;
								break;
							}

							if (record == null)
								continue;

							CellValueHolder cvh = _values.GetValue(r, c);

							if (null != cvh)
							{
								Field field = _values.GetField(c);

								// skip cells that cannot be modified or could not be converted
								// and we marked as continue
								if (cvh.Ignore)
									continue;

								bool wasTemplateRecord = record.IsAddRecordTemplate;

								// if we are inserting multiple records and the add record is on top then 
								// we want to preserve the order from the paste so offset based on how many
								// records we have added
								int newRecordOnTopOffset = newRecords.Count;

								// if the initial add record was an add record but not a template then
								// we want the additional records to be after that so offset by 1 extra
								if (isAddRecordTemplate && modifiedCells.Count > 0)
									newRecordOnTopOffset++;

								bool changedValue = SetCellValue(record, field, cvh.Value, _useConverter, newRecordOnTopOffset,
									operation, clipInfo, out errorAction);

								// we may return true but the record wasn't changed from a template yet
								// AS 8/20/09 TFS19091
								// We may return true even if the value hasn't changed so it may be selected/activated.
								//
								//if (changedValue && wasTemplateRecord)
								if (changedValue && wasTemplateRecord && !record.IsAddRecordTemplate)
								{
									newRecords.Add(record);
								}

								// if the value was not changed and we got back an action of clear,
								// try to clear the cell (unless we were trying to clear it when the 
								// error occurred as would happen if this were from a cut/clear
								//
								#region Handle ClearCell
								if (!changedValue && errorAction == ClipboardErrorAction.ClearCellAndContinue)
								{
									Debug.Assert(!record.IsAddRecordTemplate, "We should not try to clear if we couldn't create the record");

									if (operation != ClipboardOperation.ClearContents &&
										operation != ClipboardOperation.Cut)
									{
										// if the error happened while setting the cell and we were told
										// to clear the value try to set it again with the null value for
										// this record
										object nullValue = null;
										record.CoerceNullEditValue(field, ref nullValue);

										changedValue = SetCellValue(record, field, nullValue, true, 0,
											operation, clipInfo, out errorAction);
									}

									// if we got here and we still want to clear then consider it a continue
									if (errorAction == ClipboardErrorAction.ClearCellAndContinue)
										errorAction = ClipboardErrorAction.Continue;
								}
								#endregion //Handle ClearCell

								// if we got here and have an error then we are either going 
								// to stop or we skipped cells
								if (null != errorAction)
								{
									Debug.Assert(!changedValue);
									skippedCells = true;
								}

								// if we did change the value then store a reference to the cell
								if (changedValue)
								{
									// JJD 11/17/11 - TFS78651 
									// Added sortValueChanged parameter
									sortvalueChanged = sortvalueChanged || field.SortStatus != SortStatus.NotSorted;

									Debug.Assert(null == errorAction);

									// if this is a template add record then don't store a reference to
									// its cell since there will be no associated cell in the undo action
									CellKey cellKey = new CellKey(record, field);
									if (!isAddRecordTemplate)
										modifiedCells.Add(cellKey);
									else // do track it separately though so we can select those too
										newModifiedCells.Add(cellKey);

									// follow along with wingrid and activate the first modified cell
									// if it wasn't in the list of cells that we were going to modify
									if (activateFirstCell == true)
									{
										activateFirstCell = false;
										record.Cells[field].IsActive = true;
									}

									// if its a template record then we'll wait until
									// we're done with the row. that's what happens in the ui
									if (updateOnCellChange && !isAddRecord)
									{
										// update the record with each change
										UpdateRecord(record, operation, clipInfo, out errorAction);
									}
								}
								else if (errorAction == ClipboardErrorAction.Continue)
								{
									Debug.Assert(!record.IsAddRecordTemplate, "If we're going to allow continuing then we should probably skip the data for this record");

									// if we were not able to update the cell and we're told to 
									// continue then mark this cell such that its not been processed
									cvh.Ignore = true;
								}

								// if an error happened (either while changing the cell or while updating the 
								// record) and we are told to stop or revert then we need to exit the processing
								// of the cells
								if (errorAction == ClipboardErrorAction.Stop ||
									errorAction == ClipboardErrorAction.Revert)
								{
									stopProcessing = true;
									break;
								}
							}
						}
						#endregion //Process Cells

						modifiedRecord = modifiedCells.Count > modifiedCount;

						// if this was a template add record then we would not have stored
						// any modified cells but we still need to consider calling update
						if (!modifiedRecord && isAddRecordTemplate && !record.IsAddRecordTemplate)
							modifiedRecord = true;

						#region UpdateMode - Record
						// if the record has been modified, we need to call update on the record
						// if the update mode is as such or if this is an addrecord since we would 
						// have skipped the call to update while editing the cells
						if (modifiedRecord && (updateOnRecordChange || isAddRecord))
						{
							// we want to avoid calling update if we're going to revert the values
							if (errorAction != ClipboardErrorAction.Revert)
							{
								// we want to skip calling update if we're only processing one record
								// and that record is active
								if (!record.IsActive || rCount > 1)
								{
									UpdateRecord(record, operation, clipInfo, out errorAction);

									// if an error happened while updating the record and we are told to 
									// stop or revert then we need to exit the processing
									// of the cells
									if (errorAction == ClipboardErrorAction.Stop ||
										errorAction == ClipboardErrorAction.Revert)
									{
										stopProcessing = true;
									}
								}
							}
						}
						#endregion //UpdateMode - Record
					}
					finally
					{
						// if we suspended the record initialization...
						if (!wasInitializeSuspended)
						{
							dp.ResumeInitializeRecordFor(record);

							// if we have modified any cells in the record then fire 
							// initialize record now with true for reinitialize
							if (modifiedRecord)
							{
								// JJD 11/17/11 - TFS78651 
								// Added sortValueChanged parameter
								//record.FireInitializeRecord(true);
								record.FireInitializeRecord(true, sortvalueChanged);
							}
						}
					}

					if (stopProcessing)
						break;
				}

				Debug.Assert(stopProcessing || stopAtIndex == null, "We were provided a stop at cell but never hit it. Was the CVH associated with that cell null?");

				// if we haven't processed any cells then we can return null to indicate there is nothing to undo
				if (modifiedCells.Count == 0 && newRecords.Count == 0)
					return null;

				#region Revert
				if (errorAction == ClipboardErrorAction.Revert)
				{
					// handle new records first

					// the first pass will cancel any add records
					for (int i = newRecords.Count - 1; i >= 0; i--)
					{
						DataRecord dr = newRecords[i];

						if (dr.IsAddRecord)
						{
							dr.CancelEdit();
							newRecords.RemoveAt(i);
						}
					}

					// if we still have records (which we should if we created more than 1)
					if (newRecords.Count > 0)
					{
						// then we need to delete the records
						UndeleteRecordsAction undeleteAction;
						int deletedCount;

						clipInfo.DataPresenter.DeleteRecords(newRecords.ToArray(), false, false,
							out undeleteAction, out deletedCount);

						Debug.Assert(deletedCount == newRecords.Count, "Couldn't delete all the records added");
					}

					// change the values of the modified cells back
					foreach (CellKey key in modifiedCells)
					{
						CellValueHolder cvhOriginal = previousValues.GetValue(key);
						Debug.Assert(null != cvhOriginal);

						key.Record.SetCellValue(key.Field, cvhOriginal.Value, useConverterForUndo, false, true, DataRecord.UndoValueMissing, false);
					}

					return null;
				}
				#endregion //Revert

				#region Fix undoaction
				// as we did with wingrid, we want to only undo actions where we modified the 
				// cells and skip ones that were readonly, had values we could not convert, etc.
				// to that end, we'll set all the items such that we ignore them and then not
				// ignore the ones we actually modified
				ClipboardOperationInfo.InitializeIgnoreState(previousValues, true);

				// then get the CVH for the cells actually modified and set the ignore to false
				for (int i = 0, count = modifiedCells.Count; i < count; i++)
				{
					CellValueHolder cvh = previousValues.GetValue(modifiedCells[i]);

					Debug.Assert(null != cvh);

					if (null != cvh)
						cvh.Ignore = false;
				}

				SelectionTarget selectionTarget = SelectionTarget.Cells;

				// if we hit an error then we want to select just the cells
				// that were modified to give a visual cue what was changed
				// if that's not the case then we may want to select records
				if (!skippedCells && !hadConversionErrors)
				{
					// if we created new records then select the records
					if (newRecords.Count > 0)
						selectionTarget = SelectionTarget.Records;
					else if (_preferSelectionTarget == SelectionTarget.Records)
						selectionTarget = SelectionTarget.Records;
					else if (_preferSelectionTarget == SelectionTarget.Fields)
						selectionTarget = SelectionTarget.Fields;
				}

				// store the state on the undo
				undoAction._preferSelectionTarget = selectionTarget;

				#endregion //Fix undoaction

				#region Select Records/Cells
				// lastly lets select the cells/records if needed
				if (operation == ClipboardOperation.Paste || isUndoRedo)
				{
					List<CellKey> cellsToSelect = new List<CellKey>(modifiedCells);
					cellsToSelect.AddRange(newModifiedCells);

					if (selectionTarget == SelectionTarget.Records)
						SelectRecords(dp, cellsToSelect, true, true);
					else if (selectionTarget == SelectionTarget.Fields)
						SelectFields(dp, cellsToSelect, _values, true, true);
					else
						SelectCells(dp, cellsToSelect, true, modifiedCells.Count == 1);
				}
				#endregion //Select Records/Cells
			}
			finally
			{
				// AS 8/20/09 TFS20920
				// If there was only 1 record then like the singlecellvalueaction we want to conditionally 
				// evaluate the filter on the record since we may need to filter in a record that was 
				// filtered out or may just need to evaluate if its not active even after the operation.
				//
				if (null != recordWithSuspendedFilter)
				{
					SingleCellValueAction.ResumeRecordFiltering(recordWithSuspendedFilter);
				}
			}

			// if undo is not enabled then return an action to indicate
			// that it was successful but that the history should be cleared
			if (!dp.IsUndoEnabled)
				return ActionHistory.NoUndoAction;

			// if we created new records then the undeletion
			if (newRecords.Count > 0)
			{
				return new DataPresenterCompositeAction(
					// pass in our cell change action first so we 
					// get the option of reverting the cell values
					undoAction,
					new DeleteRecordsAction(dp, newRecords.ToArray(), false));
			}

			return undoAction;
		}
		#endregion //Perform

		#region ToString
		public override string ToString()
		{
			return string.Format("MultipleCellValuesAction - {0}x{1}",
				_values.FieldCount, _values.RecordCount);
		} 
		#endregion //ToString

        #endregion //Base class overrides

		#region Methods

		#region SetCellValue
		private static bool SetCellValue(DataRecord record, Field field, object value, bool useConverter, int recordOnTopOffset,
			ClipboardOperation? operation, ClipboardOperationInfo clipInfo, out ClipboardErrorAction? errorAction)
		{
			DataErrorInfo errorInfo = null;
			Exception exception = null;

			// assume there will be no error
			errorAction = null;

			try
			{
				// AS 8/20/09 TFS19091
				// I moved this up from below. In addition, the return value may be false but not 
				// be an error condition - e.g. if CellUpdating is cancelled.
				//
				bool result = record.SetCellValue(field, value, useConverter, out errorInfo);

				Debug.Assert(!result || errorInfo == null);

				// AS 8/20/09 TFS19091
				// if there was no change then bail out now (e.g. CellUpdating was cancelled). 
				// note, we're returning true anyway so that the cell will still be selected 
				// and activated. in the test for this issue, cellupdating was always being 
				// cancelled but if the value we are going to paste is already the value of 
				// the cell then we would never get to the point of raising cellupdating and 
				// it would seem that that cell is modified. we want to consider that cell as 
				// modified because we want it to be part of the undo, selection, activation, etc. 
				// because under normal circumstances cellupdating wouldn't be cancelled. so 
				// we're going to consider a failed update as successful as well so it can 
				// be selected/activated. note, in wingrid setcellvalue actually returned true 
				// when beforecellupdate was cancelled so this essentially simulates that here.
				//
				if (!result && errorInfo == null)
				{
					return true;
				}

				// AS 8/20/09 TFS19091
				// Added if block. Since we are setting the cell value first, there may be an error 
				// from that operation in which case we want to raise that error message. 
				//
				if (errorInfo == null)
				{
					// let the record know that the value was changed as a result of an end user
					// action so that if its a template add record the new record will be inserted
					// AS 8/20/09 TFS19091
					// We only want/need to do this for a template add record to ensure the underlying 
					// datarecord actually gets created.
					//
					//if (!record.OnEditValueChanged(false, recordOnTopOffset))
					if (record.IsAddRecordTemplate && !record.OnEditValueChanged(false, recordOnTopOffset))
					{
						Debug.Assert(null != operation);
						Debug.Assert(record.IsAddRecord);

						if (operation != null)
						{
							errorAction = clipInfo.RaiseClipboardError(operation.Value, ClipboardError.InsertRecordError,
								ClipboardErrorAction.Stop, false, null, false, record, null);
						}
						else
						{
							errorAction = ClipboardErrorAction.Stop;
						}

						return false;
					}

					Debug.Assert(!record.IsAddRecordTemplate);
				}

				// AS 8/20/09 TFS19091
				// Moving up so that we don't mark the record as dirty unless something changes.
				// If we get here and we had a successful cell update then we can return true.
				//
				//bool result = record.SetCellValue(field, value, useConverter, out errorInfo);
				//
				//if (result)
				//{
				//    Debug.Assert(null == errorInfo);
				//    return true;
				//}
				if (result && errorInfo == null)
					return true;
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			if (null != errorInfo)
				exception = errorInfo.Exception;

			// we either have an errorInfo returned or an unhandled exception
			if (null != operation)
			{
				errorAction = clipInfo.RaiseClipboardError(operation.Value, ClipboardError.SetCellValueError,
					ClipboardErrorAction.Continue, true, exception, true, record, field);
			}
			else
			{
				Debug.Assert(null != errorInfo);

				if (null != errorInfo)
					clipInfo.DataPresenter.RaiseDataError(errorInfo);

				errorAction = ClipboardErrorAction.Stop;
			}

			return false;
		}
		#endregion //SetCellValue

		#region ShouldActivateFirstCell
		private static bool ShouldActivateFirstCell(ICellValueProvider values, Cell activeCell)
		{
			// if we follow along with the wingrid implementation, we should activate the first 
			// modified cell assuming the current active cell is not represented by 
			if (null == activeCell)
				return true;

			int cCount = values.FieldCount;
			int column = -1;
			Field field = activeCell.Field;

			for (int i = 0; i < cCount; i++)
			{
				if (values.GetField(i) == field)
				{
					column = i;
					break;
				}
			}

			if (column < 0)
				return true;

			int rCount = values.RecordCount;
			int row = -1;
			DataRecord record = activeCell.Record;

			for (int i = 0; i < rCount; i++)
			{
				if (values.GetRecord(i) == record)
				{
					row = i;
					break;
				}
			}

			if (row < 0)
				return true;

			// if its in the range but not one of the cells we're going to modify
			CellValueHolder cvh = values.GetValue(row, column);

			if (cvh == null || cvh.Ignore)
				return true;

			return false;
		}
		#endregion //ShouldActivateFirstCell

		#region StoreOriginalDataItems
		private void StoreOriginalDataItems()
		{
			// since some records could be an add record that is not committed
			// we need to cache the original data item for comparison later
			// should an undelete happen
			_originalDataItems = new List<object>();

			for (int i = 0, count = _values.RecordCount; i < count; i++)
			{
				DataRecord record = _values.GetRecord(i);
				object oldDataItem = GetRecordDataItem(record);
				_originalDataItems.Add(oldDataItem);
			}
		}
		#endregion //StoreOriginalDataItems

		#region UpdateRecord
		private static bool UpdateRecord(DataRecord record, ClipboardOperation? operation,
			ClipboardOperationInfo clipInfo, out ClipboardErrorAction? errorAction)
		{
			errorAction = null;

			DataErrorInfo errorInfo;
			bool result = record.Update(out errorInfo);

			if (null != errorInfo)
			{
				Debug.Assert(!result);
				if (null != operation)
				{
					errorAction = clipInfo.RaiseClipboardError(operation.Value, ClipboardError.UpdateRecordError, ClipboardErrorAction.Continue, true, errorInfo.Exception, true, record, null);
				}
				else
				{
					clipInfo.DataPresenter.RaiseDataError(errorInfo);

					errorAction = ClipboardErrorAction.Stop;
				}
			}

			return result;
		}
		#endregion //UpdateRecord

		#endregion //Methods

		#region SelectionTarget enum
		internal enum SelectionTarget
		{
			Cells,
			Records,
			Fields,
		} 
		#endregion //SelectionTarget enum
    } 
    #endregion //MultipleCellValuesAction

    #region DeleteRecordsAction
	/// <summary>
	/// Action used to delete one or more <see cref="DataRecord"/> instances
	/// </summary>
    internal class DeleteRecordsAction : DataPresenterAction
    {
        #region Member Variables

        private IList<Record> _records;
		private bool _displayPrompt;
		private Dictionary<object, int> _dataItemToIndexMap;
		private Dictionary<int, object> _indexToDataItemMap;

        #endregion //Member Variables

        #region Constructor
		internal DeleteRecordsAction(DataPresenterBase dp, IList<Record> records, bool displayPromptMessage)
            : base(dp)
        {
			GridUtilities.ValidateNotNull(records, "records");

			// since the records could be an add record that has not been
			// committed and could subsequently be, we need to hold a mapping
			// of the data item to its index in the collection so we can 
			// do fixups should the record be undeleted later
			_dataItemToIndexMap = new Dictionary<object, int>();

			// similarly, if this is an add record then it could be
			// cancelled and later reused so we don't want to delete
			// it if it was reused for a different data item unless
			// it gets fixed up
			_indexToDataItemMap = new Dictionary<int, object>();

			for (int i = 0; i < records.Count; i++)
			{
				DataRecord dataRecord = records[i] as DataRecord;

				Debug.Assert(null != dataRecord);

				if (null == dataRecord)
					continue;

				Debug.Assert(dataRecord.DataItem != null);

				object dataItem = dataRecord.DataItem;

				if (null != dataItem)
				{
					_dataItemToIndexMap[dataItem] = i;
					_indexToDataItemMap[i] = dataItem;
				}
			}

            _records = records;
			_displayPrompt = displayPromptMessage;
        }
        #endregion //Constructor

        #region Base class overrides

		#region AddUndeleteDescendantInfo
		internal protected override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			foreach (Record record in _records)
				descendantInfo.AddRecord(record);
		} 
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform

		internal protected override bool CanPerform(object context)
		{
			for (int i = 0; i < _records.Count; i++)
			{
				if (this.CanDelete(i))
					return true;
			}

			return false;
		}

		#endregion //CanPerform

		#region OnRecordsUndeleted
		internal protected override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			DataRecord newRecord;
			foreach (KeyValuePair<object, int> pair in _dataItemToIndexMap)
			{
				if (oldDataTimeToRecords.TryGetValue(pair.Key, out newRecord))
				{
					Debug.Assert(newRecord.DataItem != null);

					_records[pair.Value] = newRecord;
					_indexToDataItemMap[pair.Value] = newRecord.DataItem;
				}
			}
		}
		#endregion //OnRecordsUndeleted

		#region Perform

		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			// first try to delete the records
			int deletedRecordsCount = 0;

			List<Record> recordsToDelete = new List<Record>();
			DataPresenterBase dp = this.DataPresenter;

			for (int i = 0, count = _records.Count; i < count; i++)
			{
				if (!CanDelete(i))
					continue;

				DataRecord record = _records[i] as DataRecord;
				Debug.Assert(null != record);

				recordsToDelete.Add(record);
			}

			Dictionary<Record, object> dataItemTable = new Dictionary<Record, object>();

			foreach (DataRecord record in recordsToDelete)
				dataItemTable[record] = record.DataItem;

			UndeleteRecordsAction undoAction;
			bool deleted = dp.DeleteRecords(recordsToDelete, _displayPrompt, true, out undoAction, out deletedRecordsCount);

			// build or add to the undo
			if (!deleted || deletedRecordsCount == 0)
				return null;

			// records were deleted so even if we didn't get an undelete strategy
			// we want to return an object so it indicates the operation itself
			// was successful even though it won't be able to be undone
			return undoAction ?? CompletedAction.Instance;
		}

		#endregion //Perform

		#region ToString

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("DeleteRecords:");

			foreach (DataRecord dr in _records)
				sb.AppendLine("\t" + dr.ToString());

			return sb.ToString();
		}

		#endregion //ToString

        #endregion //Base class overrides

		#region Methods

		#region CanDelete
		private bool CanDelete(int index)
		{
			if (index < 0 || index >= _records.Count)
				return false;

			DataRecord record = _records[index] as DataRecord;

			if (null == record || ClipboardOperationInfo.IsDeleted(record))
				return false;

			if (record.IsAddRecordTemplate)
				return false;

			object currentDataItem = record.DataItem;

			// the record could have been an add record that was subsequently cancelled
			if (currentDataItem == null)
				return false;

			object oldDataItem = _indexToDataItemMap[index];

			// the record could have been an add record that was cancelled and then reused 
			// so make sure it still has the same data item reference
			if (false == ClipboardOperationInfo.IsSameDataItem(currentDataItem, oldDataItem))
				return false;

			return true;
		}
		#endregion //CanDelete

		#endregion //Methods
    } 
    #endregion //DeleteRecordsAction

    #region UndeleteRecordsAction
	/// <summary>
	/// Action that used an <see cref="UndeleteRecordsStrategy"/> to undelete records deleted through the data presenter ui.
	/// </summary>
    internal class UndeleteRecordsAction : DataPresenterAction
    {
		#region Member Variables

		private UndeleteRecordsStrategy _strategy;
		private IList<UndeleteRecordsStrategy.RecordInfo> _oldRecords;

		#endregion //Member Variables

		#region Constructor
		internal UndeleteRecordsAction(DataPresenterBase dp, IList<UndeleteRecordsStrategy.RecordInfo> oldRecords, UndeleteRecordsStrategy strategy)
			: base(dp)
		{
			GridUtilities.ValidateNotNull(oldRecords);

			_oldRecords = new ReadOnlyCollection<UndeleteRecordsStrategy.RecordInfo>(oldRecords);
			_strategy = strategy;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		protected internal override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			foreach (UndeleteRecordsStrategy.RecordInfo recordInfo in _oldRecords)
			{
				RecordManager rm = recordInfo.RecordManager;

				if (null != rm)
					descendantInfo.AddRecord(rm.ParentRecord);
			}
		} 
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected sealed override bool CanPerform(object context)
		{
			return null != _strategy && _strategy.CanUndelete(_oldRecords);
		}
		#endregion //CanPerform

		#region OnRecordsUndeleted
		internal protected override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			// in the case of an undelete records action there are no child records to fix up.
			// however, the record info maintains a reference to the containing record manager
			// so we need to fix those up for descendant records
			foreach (UndeleteRecordsStrategy.RecordInfo recordInfo in _oldRecords)
			{
				RecordManager rm = recordInfo.RecordManager;

				if (null == rm)
					continue;

				Field field = rm.Field;

				// if the record info is associated with a root record deletion 
				// or if the associated field has been removed then do not try 
				// to fix it
				if (!IsValid(field))
					continue;

				DataRecord parentRecord = rm.ParentDataRecord;
				DataRecord newRecord;
				object parentDataItem = parentRecord.DataItem;

				if (null != parentDataItem && oldDataTimeToRecords.TryGetValue(parentDataItem, out newRecord))
				{
					if (newRecord.FieldLayout == field.Owner)
					{
						ExpandableFieldRecord expandableFieldRecord = newRecord.ChildRecords[field];
						recordInfo.RecordManager = expandableFieldRecord.ChildRecordManager;
					}
				}
			}
		}
		#endregion //OnRecordsUndeleted

		#region Perform
		internal protected sealed override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			IDictionary<UndeleteRecordsStrategy.RecordInfo, object> undeletedRecordMapping = _strategy.Undelete(_oldRecords);

			if (null == undeletedRecordMapping || undeletedRecordMapping.Count == 0)
				return null;

			#region ProcessUndeletedRecords

			List<DataRecord> recordsCreated = new List<DataRecord>();
			Dictionary<object, DataRecord> oldDataItemToNewRecord = new Dictionary<object, DataRecord>();

			#region Build table of recordManager=>list of new dataitems
			// to be as effecient as possible we want to iterate the list for 
			// a given collection only once. since the records could be from 
			// different record manager we need a list broken down by record
			// managers
			Dictionary<RecordManager, Dictionary<object, UndeleteRecordsStrategy.RecordInfo>> recordManagers = new Dictionary<RecordManager, Dictionary<object, UndeleteRecordsStrategy.RecordInfo>>();

			foreach (UndeleteRecordsStrategy.RecordInfo recordInfo in _oldRecords)
			{
				Dictionary<object, UndeleteRecordsStrategy.RecordInfo> records;
				RecordManager rm = recordInfo.RecordManager;

				Debug.Assert(null != rm && null != rm.List);

				if (null == rm || rm.List == null)
					continue;

				object newDataItem;

				// if the record wasn't undeleted (or at least a mapping wasn't provided)...
				if (!undeletedRecordMapping.TryGetValue(recordInfo, out newDataItem))
					continue;

				Debug.Assert(null != newDataItem);

				if (newDataItem == null)
					continue;

				if (!recordManagers.TryGetValue(rm, out records))
				{
					records = new Dictionary<object, UndeleteRecordsStrategy.RecordInfo>();
					recordManagers[rm] = records;
				}

				records[newDataItem] = recordInfo;
			}
			#endregion //Build table of recordManager=>list of new dataitems

			#region Get DataRecords created for mapped data items
			foreach (KeyValuePair<RecordManager, Dictionary<object, UndeleteRecordsStrategy.RecordInfo>> pair in recordManagers)
			{
				RecordManager rm = pair.Key;
				IList list = rm.List;
				Dictionary<object, UndeleteRecordsStrategy.RecordInfo> newDataItemTable = pair.Value;
				IList<DataRecord> dataRecords = rm.Unsorted;

				#region Check Original Location

				object[] newDataItems = new object[pair.Value.Keys.Count];
				pair.Value.Keys.CopyTo(newDataItems, 0);

				// as an optimization we'll check the original slots first in case the items were
				// restored to their original locations
				for (int i = 0; i < newDataItems.Length; i++)
				{
					object newDataItem = newDataItems[i];
					UndeleteRecordsStrategy.RecordInfo recordInfo = newDataItemTable[newDataItem];
					int oldDataItemIndex = recordInfo.DataItemIndex;

					if (oldDataItemIndex >= 0 &&
						oldDataItemIndex < list.Count &&
						ClipboardOperationInfo.IsSameDataItem(newDataItem, list[oldDataItemIndex]))
					{
						DataRecord dataRecord = dataRecords[oldDataItemIndex];
						recordsCreated.Add(dataRecord);

						oldDataItemToNewRecord[recordInfo.DataItem] = dataRecord;

						newDataItemTable.Remove(newDataItem);

						if (null != recordInfo.Children)
							ProcessDescendants(_strategy, oldDataItemToNewRecord, dataRecord, recordInfo.Children);
					}
				} 
				#endregion //Check Original Location

				if (newDataItemTable.Count == 0)
					continue;

				// in all likelihood the items are at the end so iterate backwards
				for (int i = list.Count - 1; i >= 0; i--)
				{
					object listDataItem = list[i];

					UndeleteRecordsStrategy.RecordInfo recordInfo;

					if (null != listDataItem && newDataItemTable.TryGetValue(listDataItem, out recordInfo))
					{
						DataRecord dataRecord = dataRecords[i];
						recordsCreated.Add(dataRecord);

						oldDataItemToNewRecord[recordInfo.DataItem] = dataRecord;

						// remove it from our list
						newDataItemTable.Remove(listDataItem);

						if (null != recordInfo.Children)
							ProcessDescendants(_strategy, oldDataItemToNewRecord, dataRecord, recordInfo.Children);

						if (newDataItemTable.Count == 0)
							break;
					}
				}
			}
			#endregion //Get DataRecords created for mapped data items

			_strategy.ProcessUndeletedRecords(new ReadOnlyCollection<DataRecord>(recordsCreated));

			#endregion //ProcessUndeletedRecords

			DataPresenterBase dp = this.DataPresenter;

			// fix up any undo actions that referenced the old data item
			dp.History.FixHistoryAfterUndelete(oldDataItemToNewRecord);

			if (recordsCreated.Count == 0)
				return null;

			#region Select undeleted records

			DataRecord[] newRecords = recordsCreated.ToArray();

			// select the records that were deleted
			SelectRecords(dp, newRecords, true, true);

			#endregion //Select undeleted records

			// create an undo action that would delete the selected records
			return new DeleteRecordsAction(dp, newRecords, false);
		}

		#endregion //Perform

		#region ToString
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("UndeleteRecords:");

			foreach (UndeleteRecordsStrategy.RecordInfo record in _oldRecords)
			{
				if (null == record)
					sb.AppendLine("\t{null}");
				else
					sb.AppendLine("\t" + record.ToString());
			}

			return sb.ToString();
		}
		#endregion //ToString

		#endregion //Base class overrides

		#region Methods

		#region ProcessDescendants
		private static void ProcessDescendants(UndeleteRecordsStrategy _strategy,
			Dictionary<object, DataRecord> oldDataItemToNewRecord,
			DataRecord dataRecord,
			Dictionary<Field, IList<UndeleteRecordsStrategy.RecordInfo>> children)
		{
			// process all the expandable field records for which we had children with undo/redo actions...
			foreach (KeyValuePair<Field, IList<UndeleteRecordsStrategy.RecordInfo>> pair in children)
			{
				Debug.Assert(dataRecord.FieldLayout == pair.Key.Owner);

				// if the field was removed since the action was created then skip these children
				if (dataRecord.FieldLayout != pair.Key.Owner)
					continue;

				ExpandableFieldRecord expandableFieldRecord = dataRecord.ChildRecords[pair.Key];
				RecordManager rm = expandableFieldRecord.ChildRecordManager;

				// we need to make sure the record info's are fixed up before
				// calling into the strategy
				foreach (UndeleteRecordsStrategy.RecordInfo recordInfo in pair.Value)
					recordInfo.RecordManager = rm;

				// have the strategy provide the mapping
				IDictionary<UndeleteRecordsStrategy.RecordInfo, object> mapping = _strategy.ProvideDescendantMapping(pair.Value);

				if (null != mapping && mapping.Count > 0)
				{
					IList list = rm.List;
					IList<DataRecord> dataRecords = rm.Unsorted;

					#region Check Original Location
					// as an optimization, we'll see if the new data item exists at the old item's slot
					for (int i = 0, count = pair.Value.Count; i < count; i++)
					{
						UndeleteRecordsStrategy.RecordInfo recordInfo = pair.Value[i];

						object newDataItem;

						if (!mapping.TryGetValue(recordInfo, out newDataItem))
							continue;

						int oldDataItemIndex = recordInfo.DataItemIndex;

						if (oldDataItemIndex >= 0 &&
							oldDataItemIndex < list.Count &&
							ClipboardOperationInfo.IsSameDataItem(newDataItem, list[oldDataItemIndex]))
						{
							DataRecord childDataRecord = dataRecords[oldDataItemIndex];
							oldDataItemToNewRecord[recordInfo.DataItem] = childDataRecord;
							mapping.Remove(recordInfo);

							if (recordInfo.Children != null)
								ProcessDescendants(_strategy, oldDataItemToNewRecord, childDataRecord, recordInfo.Children);
						}
					} 
					#endregion //Check Original Location

					if (mapping.Count == 0)
						continue;

					Dictionary<object, UndeleteRecordsStrategy.RecordInfo> newDataItemToRecordInfo = new Dictionary<object, UndeleteRecordsStrategy.RecordInfo>();

					// if we need to go the other way then we need a dictionary that maps
					// from the new data item to the old record info
					foreach (KeyValuePair<UndeleteRecordsStrategy.RecordInfo, object> mappingPair in mapping)
					{
						if (null != mappingPair.Value)
							newDataItemToRecordInfo[mappingPair.Value] = mappingPair.Key;
					}

					UndeleteRecordsStrategy.RecordInfo oldRecordInfo;

					// the order we walk isn't really relevant here
					for (int i = 0, count = list.Count; i < count; i++)
					{
						object dataItem = list[i];

						if (newDataItemToRecordInfo.TryGetValue(dataItem, out oldRecordInfo))
						{
							DataRecord childDataRecord = dataRecords[i];

							Debug.Assert(!oldDataItemToNewRecord.ContainsKey(oldRecordInfo.DataItem));

							oldDataItemToNewRecord[oldRecordInfo.DataItem] = childDataRecord;

							if (oldRecordInfo.Children != null)
								ProcessDescendants(_strategy, oldDataItemToNewRecord, childDataRecord, oldRecordInfo.Children);
						}
					}

				}
			}
		}
		#endregion //ProcessDescendants

		#endregion //Methods
    } 
    #endregion //UndeleteRecordsAction

	#region DataPresenterCompositeAction
	/// <summary>
	/// Custom action that contains one or more actions.
	/// </summary>
	internal class DataPresenterCompositeAction : DataPresenterAction
	{
		#region Member Variables

		private DataPresenterAction[] _actions; 

		#endregion //Member Variables

		#region Constructor
		internal DataPresenterCompositeAction(params DataPresenterAction[] actions)
			: base(actions[0].DataPresenter)
		{
			_actions = actions;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		internal protected override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			foreach (DataPresenterAction action in _actions)
				action.AddUndeleteDescendantInfo(descendantInfo);
		} 
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			foreach (DataPresenterAction action in _actions)
			{
				if (action.CanPerformInternal(context))
					return true;
			}

			return false;
		}

		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			List<DataPresenterAction> newActions = null;

			for (int i = 0; i < _actions.Length; i++)
			{
				DataPresenterAction oldAction = _actions[i];
				DataPresenterAction newAction = oldAction.OnCustomizationsChanged(fieldLayout, customizationType);

				// if this action is different than the old one (or we already found 
				// that we need a new action for a previous action in the list)...
				if (newAction != oldAction || null != newActions)
				{
					if (null == newActions)
					{
						newActions = new List<DataPresenterAction>();

						// add any previous actions
						for (int j = 0; j < i; j++)
							newActions.Add(_actions[j]);
					}

					if (null != newAction)
						newActions.Add(newAction);
				}
			}

			if (null != newActions)
			{
				if (newActions.Count == 0)
					return null;

				return new DataPresenterCompositeAction(newActions.ToArray());
			}

			return this;
		}
		#endregion //OnCustomizationsChanged

		#region OnRecordsUndeleted
		internal protected override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			foreach (DataPresenterAction action in _actions)
				action.OnRecordsUndeleted(oldDataTimeToRecords);
		}
		#endregion //OnRecordsUndeleted

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			List<DataPresenterAction> undoActions = new List<DataPresenterAction>();
			bool noUndo = false;

			foreach (DataPresenterAction action in _actions)
			{
				ActionHistory.ActionBase undoAction = action.PerformInternal(context);

				if (undoAction == ActionHistory.NoUndoAction)
					noUndo = true;
				else if (null != undoAction)
				{
					DataPresenterAction dpAction = undoAction as DataPresenterAction;
					Debug.Assert(null != dpAction || undoAction is CompletedAction);

					if (null != dpAction)
						undoActions.Add(dpAction);
				}
			}

			if (noUndo)
				return ActionHistory.NoUndoAction;

			if (undoActions.Count > 0)
				return new DataPresenterCompositeAction(undoActions.ToArray());

			return null;
		} 
		#endregion //Perform

		#region ToString
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("CompositeAction:");

			foreach (DataPresenterAction action in _actions)
				sb.AppendLine("\t" + action.ToString());

			return sb.ToString();
		} 
		#endregion //ToString

		#endregion //Base class overrides
	}
	#endregion //DataPresenterCompositeAction

	#region CompletedAction
	/// <summary>
	/// Helper action used to identify an action that was successful but cannot be undone and shouldn't clear the undo stack either.
	/// </summary>
	internal class CompletedAction : ActionHistory.ActionBase
	{
		internal static readonly ActionHistory.ActionBase Instance = new CompletedAction();

		private CompletedAction()
		{
		}

		internal protected override bool CanPerform(object context)
		{
			return false;
		}

		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			return null;
		}
	} 
	#endregion //CompletedAction

	#region RecordFiltersAction
	/// <summary>
	/// Action used to affect record filters.
	/// </summary>
	internal class RecordFiltersAction : DataPresenterAction
	{
		#region Member Variables

		private RecordFilter[] _filters;
		private object _recordManagerDataItem;
		private RecordManager _recordManager;
		private FilterCell _cell;
		private object _cellDataItem;

		#endregion //Member Variables

		#region Constructor
		internal RecordFiltersAction(RecordFilter[] filters, object filtersOwner, FilterCell cell, DataPresenterBase dp)
			: base(dp)
		{
			Debug.Assert(filtersOwner is FieldLayout || filtersOwner is RecordManager);

			_filters = filters;
			_recordManager = filtersOwner as RecordManager;
			_cell = cell;

			Debug.Assert(_recordManager == null || _cell == null || _cell.Record.RecordManager == _recordManager);

			CacheParentDataItem();
		}
		#endregion //Constructor

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		protected internal override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			if (!this.RestoreWithUndelete)
				return;

			if (null != _recordManager)
				descendantInfo.AddRecord(_recordManager.ParentRecord);
			
			if (null != _cell)
				descendantInfo.AddRecord(_cell.Record);
		}
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			if (_filters.Length == 0)
				return false;

			// if this is for a filter on the field layout or a root recordmanager we 
			// can always perform the action
			if (null == _recordManager || _recordManager.ParentRecord == null)
				return true;

			// if its for a descendant record then only if the record is still valid
			return _recordManager.ParentRecord.IsStillValid;
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			// if this is for record filters on a recordmanager and not the filters on 
			// the field layout then we should not remove the undo since the clear/load
			// will not affect these filters
			if (_recordManager == null)
			{
				if (fieldLayout == this.GetFieldLayout() && (customizationType & CustomizationType.RecordFilters) != 0)
					return null;
			}

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region OnRecordsUndeleted
		protected internal override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			if (!this.RestoreWithUndelete)
				return;

			if (null != _recordManager)
			{
				Record record = _recordManager.ParentRecord;

				if (ProcessRecordUndeleted(oldDataTimeToRecords, ref _recordManagerDataItem, ref record))
				{
					ExpandableFieldRecord efRecord = (ExpandableFieldRecord)record;
					_recordManager = efRecord.ChildRecordManager;
				}
			}

			if (null != _cellDataItem && IsValid(_cell.Field))
			{
				Record record = _cell.Record;

				if (ProcessRecordUndeleted(oldDataTimeToRecords, ref _cellDataItem, ref record))
				{
					FilterRecord fr = (FilterRecord)record;
					_cell = fr.Cells[_cell.Field];
				}
			}
		}
		#endregion //OnRecordsUndeleted

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			#region Get RecordFilterCollection

			RecordFilterCollection recordFilters = null;

			if (null != _recordManager)
				recordFilters = _recordManager.RecordFilters;
			else
			{
				FieldLayout fl = GetFieldLayout();

				if (null != fl)
					recordFilters = fl.RecordFilters;
			}

			if (null == recordFilters)
				return null;

			#endregion //Get RecordFilterCollection

			#region Collect Undo Info

			List<RecordFilter> undoFilters = new List<RecordFilter>();
			Dictionary<RecordFilter, RecordFilter> newToOldFilters = new Dictionary<RecordFilter, RecordFilter>();

			// iterate the filters we are reinserting...
			foreach (RecordFilter filter in _filters)
			{
				Field field = filter.Field;

				Debug.Assert(null != field);

				if (null == field)
					continue;

				RecordFilter oldFilter = recordFilters[field];
				Debug.Assert(null != oldFilter);

				if (null == oldFilter)
					oldFilter = new RecordFilter(field);

				RecordFilter undoFilter = oldFilter.Clone(true, oldFilter.ParentCollection, true);
				undoFilters.Add(undoFilter);
				newToOldFilters[filter] = oldFilter;
			}
			#endregion //Collect Undo Info

			#region Update Filters
			int updated = 0;

			foreach (KeyValuePair<RecordFilter, RecordFilter> pair in newToOldFilters)
			{
				if (pair.Value.InitializeFrom(pair.Key, true, true))
					updated++;
			}
			#endregion //Update Filters

			if (null != _cell && !ClipboardOperationInfo.IsDeleted(_cell))
				_cell.IsActive = true;

			return new RecordFiltersAction(undoFilters.ToArray(), recordFilters.Owner, _cell, this.DataPresenter);
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Properties
		private bool RestoreWithUndelete
		{
			get 
			{ 
				// at least for now, we will not try to restore a filter action
				// scoped to a particular record manager. the reason being that 
				// the undelete strategy would have had to cache the record filters 
				// for descendant records. if it didn't you could get into a situation
				// where there are no filters but when the user does an undo, we 
				// undo the last filter change anyway. so we will only fix up 
				// the references if the record filters were not scoped to an island 
				// but we stored on the field layout
				return _recordManager == null; 
			}
		} 
		#endregion //Properties

		#region Methods

		#region CacheParentDataItem
		private void CacheParentDataItem()
		{
			_recordManagerDataItem = GetRecordDataItem(_recordManager);
			_cellDataItem = null != _cell ? GetRecordDataItem(_cell.Record) : null;
		}
		#endregion //CacheParentDataItem

		#region GetFieldLayout
		private FieldLayout GetFieldLayout()
		{
			if (null != _recordManager)
				return _recordManager.FieldLayout;

			foreach (RecordFilter filter in _filters)
			{
				Field field = filter.Field;

				if (null != field && null != field.Owner)
					return field.Owner;
			}

			return null;
		} 
		#endregion //GetFieldLayout

		#endregion //Methods
	} 
	#endregion //RecordFiltersAction

	#region FieldResizeInfoAction
	/// <summary>
	/// Action used to change the extents of fields as well as the extent of a record when synchronized sizing is enabled.
	/// </summary>
	internal class FieldResizeInfoAction : DataPresenterAction
	{
		#region Member Variables

		private FieldLayout _fieldLayout;
		private Field.FieldResizeInfo[] _resizeInfos;
		private double[] _rowExtents;
		private double[] _colExtents;
		private bool _isWidth;

		#endregion //Member Variables

		#region Constructor
		internal FieldResizeInfoAction(FieldLayout fl, Field.FieldResizeInfo[] resizeInfos, double[] rowExtents, double[] colExtents, bool isWidth)
			: base(fl.DataPresenter)
		{
			Debug.Assert(null != resizeInfos);

			_fieldLayout = fl;
			_resizeInfos = resizeInfos;
			_rowExtents = rowExtents;
			_colExtents = colExtents;
			_isWidth = isWidth;
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			foreach (Field.FieldResizeInfo resizeInfo in _resizeInfos)
			{
				if (IsValid(resizeInfo.Field))
					return true;
			}

			return false;
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			if (fieldLayout == _fieldLayout && (customizationType & CustomizationType.FieldExtent) != 0)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			FieldResizeInfoAction undoAction = Create(_fieldLayout, _isWidth);

			foreach (Field.FieldResizeInfo resizeInfo in _resizeInfos)
			{
				Field.FieldResizeInfo oldResizeInfo = resizeInfo.Field.ExplicitResizeInfo;
				oldResizeInfo.InitializeFrom(resizeInfo);
			}

			TemplateDataRecordCache cache = _fieldLayout.TemplateDataRecordCache;

			if (null != _rowExtents)
				PerformImpl(_rowExtents, cache.GridRowLayoutItems, _isWidth);

			if (null != _colExtents)
				PerformImpl(_colExtents, cache.GridColumnLayoutItems, _isWidth);

			_fieldLayout.BumpLayoutManagerVersion();

			return undoAction;
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		#region Create
		internal static FieldResizeInfoAction Create(FieldLayout fieldLayout, bool isWidth)
		{
			List<Field.FieldResizeInfo> resizeInfos = new List<Field.FieldResizeInfo>();

			foreach (Field field in fieldLayout.Fields)
				resizeInfos.Add(field.ExplicitResizeInfo.Clone());

			TemplateDataRecordCache cache = fieldLayout.TemplateDataRecordCache;

			return new FieldResizeInfoAction(fieldLayout, resizeInfos.ToArray(), 
				GetExtents(cache.GridRowLayoutItems, isWidth),
				GetExtents(cache.GridColumnLayoutItems, isWidth), 
				isWidth);
		}
		#endregion //Create

		#region GetExtents
		private static double[] GetExtents(IList<GridDefinitionLayoutItem> layoutItems, bool isWidth)
		{
			List<double> extents = new List<double>();

			foreach (GridDefinitionLayoutItem li in layoutItems)
			{
				extents.Add(isWidth ? li.PreferredWidth : li.PreferredHeight);
			}

			return extents.ToArray();
		}
		#endregion //GetExtents

		#region PerformImpl
		private static void PerformImpl(double[] extents, IList<GridDefinitionLayoutItem> layoutItems, bool isWidth)
		{
			for (int i = 0, count = Math.Min(layoutItems.Count, extents.Length); i < count; i++)
			{
				GridDefinitionLayoutItem li = layoutItems[i];

				if (isWidth)
					li.SetPreferredWidth(extents[i], ItemSizeType.Explicit);
				else
					li.SetPreferredHeight(extents[i], ItemSizeType.Explicit);
			}
		} 
		#endregion //PerformImpl

		#endregion //Methods
	} 
	#endregion //FieldResizeInfoAction

	#region ResizeRecordAction
	/// <summary>
	/// Action used to resize a cell in an individual record.
	/// </summary>
	internal class ResizeRecordAction : DataRecordAction
	{
		#region Member Variables

		private Dictionary<ILayoutItem, double?> _extents;
		private bool _isWidth;

		#endregion //Member Variables

		#region Constructor
		private ResizeRecordAction(bool isWidth, DataRecord record)
			: base(record)
		{
			_extents = new Dictionary<ILayoutItem, double?>();
			_isWidth = isWidth;
		}
		#endregion //Constructor

		#region Base class overrides
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!this.CanPerform(context))
				return null;

			FieldGridBagLayoutManager lm = this.Record.GetLayoutManager(true);
			ResizeRecordAction undoAction = Create(this.Record, _isWidth);

			foreach (ILayoutItem li in lm.LayoutItems)
			{
				IWrapperLayoutItem wrapperItem = li as IWrapperLayoutItem;

				if (null == wrapperItem)
					continue;

				double? extent;

				if (_extents.TryGetValue(wrapperItem.InnerLayoutItem, out extent))
				{
					if (_isWidth)
						wrapperItem.ExplicitWidth = extent;
					else
						wrapperItem.ExplicitHeight = extent;
				}
			}

			return undoAction;
		}
		#endregion //Base class overrides

		#region Methods

		#region Create
		internal static ResizeRecordAction Create(Record targetRecord, bool isWidth)
		{
			DataRecord dr = targetRecord as DataRecord;
			Debug.Assert(null != dr);

			if (null == dr)
				return null;

			FieldGridBagLayoutManager lm = targetRecord.GetLayoutManager(true);
			ResizeRecordAction action = new ResizeRecordAction(isWidth, dr);

			foreach (ILayoutItem li in lm.LayoutItems)
			{
				IWrapperLayoutItem wrapperItem = li as IWrapperLayoutItem;

				Debug.Assert(null != wrapperItem);

				if (null == wrapperItem)
					continue;

				action._extents[wrapperItem.InnerLayoutItem] =
					isWidth ? wrapperItem.ExplicitWidth : wrapperItem.ExplicitHeight;
			}

			return action;
		}
		#endregion //Create

		#endregion //Methods
	} 
	#endregion //ResizeRecordAction

	#region GroupByAction
	/// <summary>
	/// Action used to add/remove an item from the GroupBy
	/// </summary>
	internal class GroupByAction : DataPresenterAction
	{
		#region Member Variables

		private FieldSortDescription _sortDescription;
		private int _index;
		private Field _field;

		#endregion //Member Variables

		#region Constructor
		internal GroupByAction(Field field)
			: base(field.DataPresenter)
		{
			FieldLayout fl = field.Owner;
			FieldSortDescription oldDescription = null;

			_field = field;
			_index = fl.SortedFields.TryGetValue(field, out oldDescription);

			if (_index >= 0)
				_sortDescription = oldDescription.Clone();
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			return IsValid(_field);
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			if (fieldLayout == _field.Owner && (customizationType & CustomizationType.GroupingAndSorting) != 0)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			GroupByAction groupByAction = new GroupByAction(_field);
			FieldLayout fl = _field.Owner;

			if (null == fl)
				return null;

			DataPresenterBase dp = fl.DataPresenter;

			if (null == dp)
				return null;

			GroupingHelper groupingHelper = new GroupingHelper(fl);

			// Possible group by scenarios that got us here:
			// 1) A sorted field was dragged into the group by area. We need to ungroup by that field 
			//		and reinsert into the group by area.
			// 2) An unsorted field was dragging into the group by area. We need to ungroup by that field.
			// 3) A field was in the group by area but was moved to a different spot. We need to put it 
			//		back at the original index.
			// 4) A field was in the group by area and was being ungrouped.

			FieldSortDescription currentFsd;
			int currentIndex = fl.SortedFields.TryGetValue(_field, out currentFsd);

			// if the field was not sorted or it was but wasn't in the group by area then ungroup it
			if (_sortDescription == null || _sortDescription.IsGroupBy == false)
			{
				Debug.Assert(null != currentFsd && currentFsd.IsGroupBy);

				// if the field is no longer grouped by then there is nothing to do
				if (null == currentFsd || currentFsd.IsGroupBy == false)
					return null;

				if (!groupingHelper.ProcessTryUnGroup(_field))
					return null;

				// first ungroup by the field
				groupingHelper.ProcessApplyUnGroup(_field, false);
			}
			else
			{
				// if we get here then the field was grouped at the point of the snapshot
				// so we need to regroup the field...
				int newIndex = _index;

				// if its currently unsorted or sorted but not in the group by...
				if (null == currentFsd || currentFsd.IsGroupBy == false)
				{
					Debug.Assert(newIndex >= 0 && newIndex <= groupingHelper.Groups.Count);
					newIndex = Math.Max(0, Math.Min(groupingHelper.Groups.Count, newIndex));

					if (!groupingHelper.ProcessTryGrouping(_field, newIndex))
						return null;

					groupingHelper.ProcessApplyGrouping(_field, newIndex, false);
				}
				else
				{
					// its currently grouped by so we're just moving it in the group by list

					Debug.Assert(newIndex >= 0 && newIndex < groupingHelper.Groups.Count);
					newIndex = Math.Max(0, Math.Min(groupingHelper.Groups.Count - 1, newIndex));

					// if its already at that spot then there is nothing to do
					if (groupingHelper.Groups[newIndex].Field == _field)
						return null;

					if (!groupingHelper.ProcessTryReGroup(_field, newIndex))
						return null;

					groupingHelper.ProcessApplyReGroup(_field, newIndex, false);
				}
			}

			groupingHelper.RaiseGroupedEventHelper();

			// if the field used to be sorted before it was grouped then we need 
			// to reinsert it into the sorted fields without being grouped by
			if (null != _sortDescription && _sortDescription.IsGroupBy == false)
			{
				bool hasFsd = fl.SortedFields.Contains(_field);
				Debug.Assert(!hasFsd);

				if (!hasFsd)
				{
					FieldSortDescription newFsd = _sortDescription.Clone();
					newFsd.Seal();
					SortingEventArgs sortingArgs = new SortingEventArgs(fl, newFsd, false);

					dp.RaiseSorting(sortingArgs);

					if (!sortingArgs.Cancel && !fl.SortedFields.Contains(_field))
					{
						int index = Math.Max(0, Math.Min(_index, fl.SortedFields.Count));
						fl.SortedFields.Insert(index, newFsd);

						dp.RaiseSorted(new SortedEventArgs(fl, newFsd));
					}
				}
			}

			// AS 8/21/09 TFS19349
			// The group by structure could have been entirely shifted so we need 
			// to walk over the actions in the current direction (undo/redo) and 
			// restore the isExpanded state of any group by records we fix along 
			// the way to what they would have been based on the last user action.
			//
			dp.History.FixGroupExpansionHistory(context);

			return groupByAction;
		} 
		#endregion //Perform

		#endregion //Base class overrides
	} 
	#endregion //GroupByAction

	#region SortingAction
	/// <summary>
	/// Action used to change the SortedFields excluding group by actions
	/// </summary>
	internal class SortingAction : DataPresenterAction
	{
		#region Member Variables

		private Field _field;
		private ModifierKeys _modifierKeys;
		private FieldSortDescription[] _sortDescriptionsRemoved;
		private ListSortDirection? _oldFieldSortDirection;

		#endregion //Member Variables

		#region Constructor
		internal SortingAction(Field field, ModifierKeys modifierKeys)
			: base(field.DataPresenter)
		{
			_field = field;
			_modifierKeys = modifierKeys;
		}

		// note: this ctor is only used by the action itself to undo a sort labelclick action
		// which is why the ctor is private
		private SortingAction(Field field, ModifierKeys modifierKeys, FieldSortDescription[] sortDescriptionsRemoved, ListSortDirection? oldFieldSortDirection)
			: this(field, modifierKeys)
		{
			_sortDescriptionsRemoved = sortDescriptionsRemoved;
			_oldFieldSortDirection = oldFieldSortDirection;

			if (_sortDescriptionsRemoved != null &&
				_sortDescriptionsRemoved.Length == 1 &&
				_sortDescriptionsRemoved[0].Field == _field)
			{

			}
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			if (!IsValid(_field))
				return false;

			// if there were descriptions removed, make sure the fields that were 
			// associated with them are still being used and therefore we can recreate
			// sort descriptions for at least one of them
			if (null != _sortDescriptionsRemoved && _sortDescriptionsRemoved.Length > 0)
			{
				foreach (FieldSortDescription fsd in _sortDescriptionsRemoved)
				{
					if (IsValid(fsd.Field))
						return true;
				}

				return false;
			}

			return true;
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			if (fieldLayout == _field.Owner && (customizationType & CustomizationType.GroupingAndSorting) != 0)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			#region Setup

			if (!this.CanPerform(context))
				return null;

			FieldLayout fl = _field.Owner;

			// JJD 3/19/09 - TFS14672
			// Check for null
			if (fl == null)
				return null;

			// JJD 3/19/09 - TFS14672
			// Check for null
			DataPresenterBase dp = fl.DataPresenter;

			if (dp == null)
				return null;

			// JJD 5/9/07 - BR22685
			// Call the IsOkToScroll method 1st which will attempt to exit edit mode
			// and return false if that gets cancelled
			IViewPanelInfo viewPanelInfo = dp as IViewPanelInfo;

			if (viewPanelInfo != null &&
				viewPanelInfo.IsOkToScroll() == false)
				return null;

			#endregion //Setup

			FieldSortDescriptionCollection sortedFields = fl.SortedFields;

			if (_sortDescriptionsRemoved != null)
			{
				ActionHistory.ActionBase undoAction = ProcessUndoSortClickAction(fl, dp, sortedFields);

				if (null != undoAction)
				{
					dp.RecordManager.VerifySort();
				}

				return undoAction;
			}
			else
			{
				return ProcessSortClickAction(fl, dp, sortedFields);
			}
		}

		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		#region ProcessSortClickAction
		private ActionHistory.ActionBase ProcessSortClickAction(FieldLayout fl, DataPresenterBase dp, FieldSortDescriptionCollection sortedFields)
		{
			// we are sorting a field
			SortStatus status = _field.SortStatus;
			ListSortDirection direction;
			LabelClickAction clickAction = _field.LabelClickActionResolved;

			switch (clickAction)
			{
				case LabelClickAction.SortByMultipleFields:
				case LabelClickAction.SortByMultipleFieldsTriState:
				case LabelClickAction.SortByOneFieldOnly:
				case LabelClickAction.SortByOneFieldOnlyTriState:
					break;
				default:
					return null;
			}

			// if the current status is ascending then we want to
			// go to descending, otherwise use ascending
			if (status == SortStatus.Ascending)
				direction = ListSortDirection.Descending;
			else
				direction = ListSortDirection.Ascending;

			// AS 6/2/09
			//int index = sortedFields.IndexOf(_field);
			FieldSortDescription oldSortDescription;
			ListSortDirection? oldSortDirection = null;
			int index = sortedFields.TryGetValue(_field, out oldSortDescription);

			bool groupby = false;

			// JJD 12/03/08 
			// Added support for cycling thru ascending/descending/unsorted
			// Assume that we will be sorting, i.e. not unsorting
			bool sort = true;

			if (index >= 0)
			{
				oldSortDirection = oldSortDescription.Direction;

				groupby = oldSortDescription.IsGroupBy;

				// toggle the direction of the existing entry
				// AS 6/1/09 NA 2009.2 Undo/Redo
				//direction = oldSortDescription.Direction == ListSortDirection.Ascending ?
				//	ListSortDirection.Descending : ListSortDirection.Ascending;
				direction = oldSortDescription.GetToggleDirection();

				// JJD 12/03/08 
				// Added support for cycling thru ascending/descending/unsorted
				// but we only go into the unsorted state when if this is not a group by field
				// and the 'Ctrl' key isn't pressed
				if (groupby == false &&
					status == SortStatus.Descending &&
					_modifierKeys != ModifierKeys.Control)
				{
					sort = clickAction != LabelClickAction.SortByOneFieldOnlyTriState &&
						   clickAction != LabelClickAction.SortByMultipleFieldsTriState;
				}
			}


			// JJD 12/03/08 
			// Added support for cycling thru ascending/descending/unsorted
			//bool multiSort = clickAction == LabelClickAction.SortByMultipleFields &&
			//                    Keyboard.Modifiers == ModifierKeys.Control;
			bool multiSort = (clickAction == LabelClickAction.SortByMultipleFields ||
							  clickAction == LabelClickAction.SortByMultipleFieldsTriState) &&
								_modifierKeys == ModifierKeys.Control;

			FieldSortDescription sortDescription = null;
			SortingEventArgs args;

			// JJD 12/03/08 
			// Added support for cycling thru ascending/descending/unsorted
			// Only create the FieldSortDescription if we are not going into an unsorted state
			if (sort == true)
			{
				sortDescription = new FieldSortDescription();

				sortDescription.Field = _field;
				sortDescription.IsGroupBy = groupby;
				sortDescription.Direction = direction;
				sortDescription.Seal();
				args = new SortingEventArgs(fl, sortDescription, !multiSort);
			}
			else
			{
				// JJD 12/03/08 
				// When unsorting a field use the new overload that takes a Field parameter
				args = new SortingEventArgs(fl, _field);
			}

			// raise the sorting event to give the listener a chance to cancel
			dp.RaiseSorting(args);

			if (args.Cancel == true)
				return null;

			SortingAction undoAction = null;

			using (new CaptureMouseHelper(fl, dp))
			{
				multiSort = !args.ReplaceExistingSortCriteria;

				// JJD 12/03/08 
				// Added support for cycling thru ascending/descending/unsorted
				// If sort is false the remove the field from the sort description
				// collection
				if (sort == false)
				{
					Debug.Assert(oldSortDescription != null);
					undoAction = new SortingAction(_field, _modifierKeys, sortedFields.CloneNonGroupBy(), oldSortDirection);

					//Clear all non-groupby fields
					sortedFields.ClearNonGroupByFields();
				}
				else
					if (multiSort)
					{
						#region MultiSort

						undoAction = new SortingAction(_field, _modifierKeys, new FieldSortDescription[0], oldSortDirection);

						// if it is already in the sort description collection
						// then just toggle the direction and get out
						if (index >= 0)
						{
							sortedFields[index].ToggleDirection();
							fl.BumpSortVersion();
						}
						else
						{
							sortedFields.Add(sortDescription);
						}

						#endregion //MultiSort
					}
					else
					{
						#region Single Sort

						// if this is a groupby field then just toggle its direction
						if (groupby && index >= 0)
						{
							undoAction = new SortingAction(_field, _modifierKeys, new FieldSortDescription[0], oldSortDirection);

							sortedFields[index].ToggleDirection();
							fl.BumpSortVersion();
						}
						else
						{
							undoAction = new SortingAction(_field, _modifierKeys, sortedFields.CloneNonGroupBy(), oldSortDirection);

							// JJD 1/29/09
							// Since we will be making multiple updates call BeginUpdate
							sortedFields.BeginUpdate();

							try
							{
								sortedFields.ClearNonGroupByFields();
								sortedFields.Add(sortDescription);
							}
							finally
							{
								// JJD 1/29/09
								// Call EndUpdate since we called BeginUpdate above
								sortedFields.EndUpdate();
							}
						}

						#endregion //Single Sort
					}

				// make sure the records have been sorted before raising the after event.
				dp.RecordManager.VerifySort();
			}

			// raise the sorted event

			// JJD 12/03/08 
			// When unsorting a field use the new overload that takes a Field parameter
			SortedEventArgs sortedArgs;

			if (sort == false)
				sortedArgs = new SortedEventArgs(fl, _field);
			else
				sortedArgs = new SortedEventArgs(fl, sortDescription);

			dp.RaiseSorted(sortedArgs);

			return undoAction;
		}
		#endregion //ProcessSortClickAction

		#region ProcessUndoSortClickAction
		private ActionHistory.ActionBase ProcessUndoSortClickAction(FieldLayout fl, DataPresenterBase dp, FieldSortDescriptionCollection sortedFields)
		{
			CaptureMouseHelper captureHelper = null;

			try
			{
				// the undo will always be to re-perform the label click action
				SortingAction undoAction = new SortingAction(_field, _modifierKeys);

				// we need to remove the sort description for the field and readd the old ones

				// Possible scenarios:
				// 1) NonGroupByFields were cleared
				// 2) The sort direction of an existing field was toggled
				// 3) A new field was added to the sorted fields
				// 4) A new field was sorted and the existing sorted fields were cleared
				bool hasOldSortDescriptions = _sortDescriptionsRemoved.Length > 0;

				#region Readd Old FieldSortDescriptions
				if (hasOldSortDescriptions)
				{
					for (int i = 0; i < _sortDescriptionsRemoved.Length; i++)
					{
						FieldSortDescription newFsd = _sortDescriptionsRemoved[i].Clone();
						newFsd.Seal();

						SortingEventArgs sortingArgs = new SortingEventArgs(fl, newFsd, i == 0);

						dp.RaiseSorting(sortingArgs);

						// handle sortingArgs.Cancel
						if (sortingArgs.Cancel)
						{
							// if no sorting change has occurred then return null
							// to indicate no change has occurred
							if (i == 0)
								return null;

							// if something was changed just return the undo action
							// we created
							return undoAction;
						}

						if (null == captureHelper)
							captureHelper = new CaptureMouseHelper(fl, dp);

						if (sortingArgs.ReplaceExistingSortCriteria)
						{
							sortedFields.BeginUpdate();
							sortedFields.ClearNonGroupByFields();
							sortedFields.Add(newFsd);
							sortedFields.EndUpdate();
						}
						else
							sortedFields.Add(newFsd);

						dp.RaiseSorted(new SortedEventArgs(fl, newFsd));
					}
				}
				#endregion //Readd Old FieldSortDescriptions

				FieldSortDescription fsd;
				int fsdIndex = sortedFields.TryGetValue(_field, out fsd);

				if (_oldFieldSortDirection == null)
				{
					#region Remove FieldSortDescription
					// if its in the collection then remove it now
					if (fsdIndex >= 0)
					{
						SortingEventArgs sortingArgs = new SortingEventArgs(fl, _field);

						dp.RaiseSorting(sortingArgs);

						// if this sort change was cancelled and we didn't do anything
						// yet then return null to indicate nothing has been processed
						if (sortingArgs.Cancel && !hasOldSortDescriptions)
							return null;

						if (!sortingArgs.Cancel)
						{
							if (null == captureHelper)
								captureHelper = new CaptureMouseHelper(fl, dp);

							sortedFields.RemoveAt(fsdIndex);

							dp.RaiseSorted(new SortedEventArgs(fl, _field));
						}
					}
					#endregion //Remove FieldSortDescription
				}
				else
				{
					#region Toggle FieldSortDescription
					// if the field used to be in the sorted fields and its not 
					// sorted by the old field direction then update the direction
					if (null != fsd && fsd.Direction != _oldFieldSortDirection.Value)
					{
						// change the sort direction
						FieldSortDescription fsdNew = new FieldSortDescription();
						fsdNew.Field = _field;
						fsdNew.Direction = fsd.GetToggleDirection();
						fsdNew.IsGroupBy = fsd.IsGroupBy;

						SortingEventArgs sortingArgs = new SortingEventArgs(fl, fsdNew, false);
						dp.RaiseSorting(sortingArgs);

						// if this sort change was cancelled and we didn't do anything
						// yet then return null to indicate nothing has been processed
						if (sortingArgs.Cancel && !hasOldSortDescriptions)
							return null;

						if (!sortingArgs.Cancel)
						{
							if (null == captureHelper)
								captureHelper = new CaptureMouseHelper(fl, dp);

							fsd.ToggleDirection();
							fl.BumpSortVersion();

							dp.RaiseSorted(new SortedEventArgs(fl, fsd));
						}
					}
					#endregion //Toggle FieldSortDescription
				}

				return undoAction;
			}
			finally
			{
				if (null != captureHelper)
					captureHelper.Dispose();
			}
		}
		#endregion //ProcessUndoSortClickAction

		#endregion //Methods

		#region CaptureMouseHelper
		private class CaptureMouseHelper : IDisposable
		{
			#region Member Variables

			private bool _captureMouse;
			private FrameworkElement _elementWithCapture;

			#endregion //Member Variables

			#region Constructor
			internal CaptureMouseHelper(FieldLayout fieldLayout, DataPresenterBase dp)
			{
				// AS 6/2/09 NA 2009.2 Undo/Redo
				// This class is based on the fix for TFS14672 where we shift capture to the DataPresenter 
				// if nothing has capture or if the label presenter has capture. However since for undo/redo
				// the mouse could be somewhere else but within a label presenter, I've relaxed the check 
				// and will take capture if any label presenter within the specified field layout has 
				// capture rather than just checking the specified field.
				//

				// JJD 3/19/09 - TFS14672
				// See if the mouse is captured by anyone
				IInputElement elementWithCapture = Mouse.Captured;
				_captureMouse = false;

				if (elementWithCapture == null)
					_captureMouse = true;
				else if (null != dp)
				{
					FrameworkElement fe = elementWithCapture as FrameworkElement;

					if (fe != null)
					{
						LabelPresenter lp = fe as LabelPresenter ?? Utilities.GetAncestorFromType(fe, typeof(LabelPresenter), true, dp) as LabelPresenter;
						_captureMouse = lp != null && lp.Field.Owner == fieldLayout;
					}
				}

				if (_captureMouse)
				{
					_elementWithCapture = dp;

					if (null == _elementWithCapture)
						_captureMouse = false;

					// JJD 3/19/09 - TFS14672
					// If the mouse is not already captured by someone else then capture
					// the mouse on the data presenter to cause any IsMouseOver state to be cleared
					// so any pending animations will get cleared to prevent rooting of elements
					// by the framework
					if (_captureMouse)
						Mouse.Capture(_elementWithCapture, CaptureMode.Element);
				}
			}
			#endregion //Constructor

			#region IDisposable

			public void Dispose()
			{
				if (_captureMouse)
				{
					_captureMouse = false;

					if (Mouse.Captured == _elementWithCapture)
						_elementWithCapture.ReleaseMouseCapture();
				}
			}

			#endregion //IDisposable
		}
		#endregion //CaptureMouseHelper
	} 
	#endregion //SortingAction

	#region SummaryAction
	/// <summary>
	/// Action used to add/remove SummaryDefinitions from a FieldLayout.
	/// </summary>
	internal class SummaryAction : DataPresenterAction
	{
		#region Member Variables

		private Field _field;
		private SummaryDefinitionHolder[] _removed;
		private SummaryDefinition[] _added;

		#endregion //Member Variables

		#region Constructor
		private SummaryAction(Field field, IList<SummaryDefinition> added, IList<SummaryDefinitionHolder> removed)
			: base(field.DataPresenter)
		{
			_field = field;
			_added = GridUtilities.ToArray(added);
			_removed = GridUtilities.ToArray(removed);
			Array.Sort(_removed);
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			if (!IsValid(_field))
				return false;

			return true;
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			if (fieldLayout == _field.Owner && (customizationType & CustomizationType.Summaries) != 0)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			FieldLayout fl = _field.Owner;
			SummaryDefinitionCollection definitions = fl.SummaryDefinitions;
			List<SummaryDefinition> oldDefinitions = new List<SummaryDefinition>(definitions);
			List<SummaryDefinition> added = new List<SummaryDefinition>();
			List<SummaryDefinition> removed = new List<SummaryDefinition>();

			foreach (SummaryDefinition item in _added)
			{
				int index = definitions.IndexOf(item);

				if (index >= 0)
				{
					removed.Add(item);
					definitions.RemoveAt(index);
				}
			}

			foreach (SummaryDefinitionHolder holder in _removed)
			{
				SummaryDefinition item = holder.Definition;
				int index = definitions.IndexOf(item);
				Debug.Assert(index < 0);

				if (index < 0)
				{
					index = Math.Max(0, Math.Min(holder.Index, definitions.Count));
					definitions.Insert(index, item);
					added.Add(item);
				}
			}

			return Create(_field, oldDefinitions, removed, added);
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		#region Create
		internal static SummaryAction Create(Field field, IList<SummaryDefinition> oldDefinitionList, List<SummaryDefinition> definitionsRemoved, List<SummaryDefinition> definitionsAdded)
		{
			DataPresenterBase dp = field.DataPresenter;

			if (dp == null || !dp.IsUndoEnabled)
				return null;

			if (definitionsAdded.Count == 0 && definitionsRemoved.Count == 0)
				return null;

			List<SummaryDefinitionHolder> removed = new List<SummaryDefinitionHolder>();
			List<SummaryDefinition> added = new List<SummaryDefinition>();

			foreach (SummaryDefinition item in definitionsRemoved)
			{
				int oldIndex = oldDefinitionList.IndexOf(item);
				Debug.Assert(oldIndex >= 0);

				removed.Add(new SummaryDefinitionHolder(item, oldIndex));
			}

			foreach (SummaryDefinition item in definitionsAdded)
			{
				added.Add(item);
			}

			return new SummaryAction(field, added, removed);
		}
		#endregion //Create

		#endregion //Methods

		#region SummaryDefinitionHolder class
		private class SummaryDefinitionHolder : IComparable<SummaryDefinitionHolder>
		{
			internal SummaryDefinition Definition;
			internal int Index;

			internal SummaryDefinitionHolder(SummaryDefinition definition, int index)
			{
				this.Definition = definition;
				this.Index = index;
			}

			#region IComparable<SummaryDefinitionHolder> Members

			int IComparable<SummaryDefinitionHolder>.CompareTo(SummaryDefinitionHolder other)
			{
				if (null == other)
					return 1;

				return Index.CompareTo(other.Index);
			}

			#endregion
		}
		#endregion //SummaryDefinitionHolder class
	} 
	#endregion //SummaryAction

	#region RecordExpansionAction
	/// <summary>
	/// Action used to toggle the <see cref="Record.IsExpanded"/> state of a <see cref="Record"/>
	/// </summary>
	internal class RecordExpansionAction : DataPresenterAction
	{
		#region Member Variables

		private Record[] _records;
		private object[] _dataItems;
		private bool _isExpanded;

		#endregion //Member Variables

		#region Constructor
		internal RecordExpansionAction(IList<Record> records, bool isExpanded)
			: base(records[0].DataPresenter)
		{
			_records = GridUtilities.ToArray(records);
			_isExpanded = isExpanded;

			_dataItems = new object[_records.Length];

			for (int i = 0; i < _records.Length; i++)
			{
				Record r = _records[i];

				_dataItems[i] = GetRecordDataItem(r);

				// AS 8/21/09 TFS19349
				// There is/should be no data item for a groupby record so we're going 
				// to reuse this slot to store information about the group by record that 
				// we can use to restore the group by record should the group by structure 
				// be changed.
				//
				if (r is GroupByRecord)
				{
					Debug.Assert(_dataItems[i] == null);
					_dataItems[i] = GroupByContext.Create((GroupByRecord)r);
				}
			}
		}
		#endregion //Constructor

		#region Base class overrides

		#region AddUndeleteDescendantInfo
		protected internal override void AddUndeleteDescendantInfo(DescendantRecordInfo descendantInfo)
		{
			for(int i = 0; i < _records.Length; i++)
				descendantInfo.AddRecord(_records[i]);
		}
		#endregion //AddUndeleteDescendantInfo

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			// AS 8/21/09 TFS19349
			this.VerifyGroupByRecords(null);

			// the root record is the one that triggered the expansion. if its 
			// not valid then nothing can be done
			if (!_records[0].IsStillValid)
				return false;

			// make sure there is at least 1 record for which
			// this operation will actually change a value
			for (int i = 0; i < _records.Length; i++)
			{
				if (_records[i].IsExpanded != _isExpanded)
					return true;
			}

			return false;
		}
		#endregion //CanPerform

		#region OnRecordsUndeleted
		protected internal override void OnRecordsUndeleted(Dictionary<object, DataRecord> oldDataTimeToRecords)
		{
			for(int i = 0; i < _records.Length; i++)
			{
				Record record = _records[i];
				object dataItem = _dataItems[i];

				// AS 8/21/09 TFS19349
				// Since there will be no dataitem for a groupby record, we're using the same 
				// array used to hold the dataitems to reduce overhead so skip that when processing 
				// undeletion fixups.
				//
				if (dataItem is GroupByContext)
					continue;

				if (ProcessRecordUndeleted(oldDataTimeToRecords, ref dataItem, ref record))
				{
					_dataItems[i] = dataItem;
					_records[i] = record;
				}
			}
		}
		#endregion //OnRecordsUndeleted

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			List<Record> recordsChanged = new List<Record>();

			bool changed = this.DataPresenter.OnRecordExpandStateChanged(_records[0], _isExpanded, recordsChanged);

			// if the event was cancelled return null to indicate nothing changed
			if (!changed)
				return null;

			Debug.Assert(null != recordsChanged && recordsChanged.Count > 0);

			if (null == recordsChanged || recordsChanged.Count == 0)
				return null;

			// otherwise if it was changed and we had other records that had been toggled with the 
			// original change then we need to update those as well
			for (int i = 1; i < _records.Length; i++)
			{
				_records[i].SetIsExpanded(_isExpanded, null);
			}

			// activate the record to ensure its brought into view
			_records[0].IsActive = true;

			return new RecordExpansionAction(recordsChanged, !_isExpanded);
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		// AS 8/21/09 TFS19349
		#region FixGroupByRecords
		internal void FixGroupByRecords(HashSet fixedRecords)
		{
			this.VerifyGroupByRecords(fixedRecords);
		}
		#endregion //FixGroupByRecords

		// AS 8/21/09 TFS19349
		#region VerifyGroupByRecords
		private void VerifyGroupByRecords(HashSet fixedRecords)
		{
			for (int i = 0; i < _records.Length; i++)
			{
				GroupByRecord gbr = _records[i] as GroupByRecord;

				if (null != gbr)
				{
					GroupByContext gbc = _dataItems[i] as GroupByContext;
					Debug.Assert(null != gbc);

					_records[i] = gbc.Validate(gbr, !_isExpanded, fixedRecords, true);
				}
			}
		}
		#endregion //VerifyGroupByRecords 

		#endregion //Methods

		// AS 8/21/09 TFS19349
		#region GroupByContext class
		private class GroupByContext
		{
			#region Member Variables

			private RecordManager _rm;
			private object[] _values;
			private Field[] _fields;

			#endregion //Member Variables

			#region Constructor
			private GroupByContext()
			{
			}
			#endregion //Constructor

			#region Methods

			#region Internal

			#region Create
			internal static GroupByContext Create(GroupByRecord record)
			{
				GroupByContext gbc = new GroupByContext();
				gbc._rm = record.RecordManager;
				List<object> values = new List<object>();
				List<Field> fields = new List<Field>();

				while (record != null)
				{
					fields.Add(record.GroupByField);
					values.Add(record.Value);
					record = record.ParentRecord as GroupByRecord;
				}

				// since we are walking up the record chain to get the info
				// but will be walking from the recordmanager down if we use
				// the info we need to reverse the information stored
				values.Reverse();
				fields.Reverse();

				gbc._values = values.ToArray();
				gbc._fields = fields.ToArray();
				return gbc;
			}
			#endregion //Create

			#region Validate
			internal Record Validate(GroupByRecord gbr, bool isExpanded, HashSet fixedRecords, bool raiseEvents)
			{
				// validate the record. if its not valid then try to find the new one
				if (!gbr.IsStillValid && _rm.HasGroups && HasSameGroupByFields(gbr))
				{
					GroupByRecord temp = null;
					RecordCollectionBase records = _rm.Groups;

					for (int i = 0; i < _values.Length; i++)
					{
						temp = GetRecord(records, _values[i]);

						if (temp == null)
							return gbr;

						records = temp.ChildRecords;
					}

					gbr = temp;

					if (null != fixedRecords && !fixedRecords.Exists(gbr))
					{
						fixedRecords.Add(gbr);

						if (gbr.IsExpanded != isExpanded)
						{
							DataPresenterBase dp = raiseEvents ? gbr.DataPresenter : null;

							if (null != dp)
								dp.OnRecordExpandStateChanged(gbr, isExpanded, null);
							else
								gbr.SetIsExpanded(isExpanded, null);
						}
					}
				}

				return gbr;
			}
			#endregion //Validate

			#endregion //Internal

			#region Private

			#region GetRecord
			private static GroupByRecord GetRecord(RecordCollectionBase records, object value)
			{
				if (records.Count == 0 || records[0] is GroupByRecord == false)
					return null;

				foreach (Record record in records)
				{
					GroupByRecord gbr = record as GroupByRecord;

					if (null != gbr && object.Equals(gbr.Value, value))
						return gbr;
				}

				return null;
			}
			#endregion //GetRecord

			#region HasSameGroupByFields
			private bool HasSameGroupByFields(GroupByRecord record)
			{
				FieldSortDescriptionCollection sortedFields = record.FieldLayout.SortedFields;
				FieldLayout fl = record.FieldLayout;

				// if there are less group by fields then it isn't possible to find a better one
				if (sortedFields.CountOfGroupByFields < _fields.Length)
					return false;

				for (int i = 0; i < _fields.Length; i++)
				{
					if (_fields[i] != sortedFields[i].Field)
						return false;
				}

				return true;
			}
			#endregion //HasSameGroupByFields

			#endregion //Private

			#endregion //Methods
		} 
		#endregion //GroupByContext class
	} 
	#endregion //RecordExpansionAction

	#region FieldPositionAction
	/// <summary>
	/// Action used to change the position of one or more fields.
	/// </summary>
	internal class FieldPositionAction : DataPresenterAction
	{
		#region Member Variables

		private Field[] _fields;
		private LayoutInfo _layoutInfo;
		private FieldPositionChangeReason _reason;
		private FieldLayout _fl;
		private FixedFieldLocation _location;

		#endregion //Member Variables

		#region Constructor
		internal FieldPositionAction(IList<Field> fields, LayoutInfo layoutInfo, FieldPositionChangeReason reason, FixedFieldLocation location)
			: base(fields[0].DataPresenter)
		{
			_fl = fields[0].Owner;
			_fields = GridUtilities.ToArray(fields);

			// be sure to store a clone of the layout info since the dragmanager could manipulate
			// the instance it had
			_layoutInfo = layoutInfo != null ? layoutInfo.Clone() : null;

			_reason = reason;
			_location = location;
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			// since we have the resulting layout info which in the case
			// of a splitter position could have affected multiple fields 
			// we need to treat the action as a single atomic operation so 
			// if any field cannot perform the move then we cannot perform 
			// the action
			foreach (Field field in _fields)
			{
				if (!IsValid(field))
					return false;

				// if the action is to change the fixed state of the 
				// field then make sure that the field still allows it
				if (_reason == FieldPositionChangeReason.Fixed ||
					_reason == FieldPositionChangeReason.Unfixed)
				{
					if (!field.IsFixedLocationAllowed(_location))
						return false;
				}

				// SSP 6/26/09 - NAS9.2 Field Chooser
				// If hiding/unhiding as part of the undo then check to 
				// make sure the user is allowed to do so.
				// 
				if ( FieldPositionChangeReason.Displayed == _reason
					|| FieldPositionChangeReason.Hidden == _reason )
				{
					if ( AllowFieldHiding.Never == field.AllowHidingResolved )
						return false;
				}
			}

			return true;
		}
		#endregion //CanPerform

		#region OnCustomizationsChanged
		internal override DataPresenterAction OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizationType)
		{
			if (fieldLayout == _fl && (customizationType & CustomizationType.FieldPosition) != 0)
				return null;

			return base.OnCustomizationsChanged(fieldLayout, customizationType);
		}
		#endregion //OnCustomizationsChanged

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			DataPresenterBase dp = this.DataPresenter;

			// raise the changing for all fields - if any cancels then 
			// fail the entire operation since we have to perform it as 
			// a single atomic unit
			foreach (Field field in _fields)
			{
				FieldPositionChangingEventArgs changingArgs = new FieldPositionChangingEventArgs(field, _reason);
				dp.RaiseFieldPositionChanging(changingArgs);

				if (changingArgs.Cancel)
					return null;
			}

			FixedFieldLocation undoLocation = _fields[0].FixedLocation;

			// all the fields have to be in the same location
			foreach (Field field in _fields)
			{
				FixedFieldLocation fieldLocation = field.FixedLocation;
				Debug.Assert(fieldLocation == undoLocation);

				if (fieldLocation != undoLocation)
					return null;
			}

			FieldPositionChangeReason undoReason;

			switch (_reason)
			{
				case FieldPositionChangeReason.Unfixed:
					Debug.Assert(_location == FixedFieldLocation.Scrollable);
					undoReason = FieldPositionChangeReason.Fixed;
					break;
				case FieldPositionChangeReason.Fixed:
					Debug.Assert(_location != FixedFieldLocation.Scrollable);
					undoReason = FieldPositionChangeReason.Unfixed;
					break;
				case FieldPositionChangeReason.Moved:
					Debug.Assert(_location == undoLocation);
					undoReason = FieldPositionChangeReason.Moved;
					break;
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// Added Displayed and Hidden members to the FieldPositionChangeReason enum.
				// 
				case FieldPositionChangeReason.Displayed:
					Debug.Assert( _location == undoLocation );
					undoReason = FieldPositionChangeReason.Hidden;
					break;
				case FieldPositionChangeReason.Hidden:
					Debug.Assert( _location == undoLocation );
					undoReason = FieldPositionChangeReason.Displayed;
					break;
				default:
					Debug.Fail("Unrecognized reason:" + _reason.ToString());
					return null;
			}

			FieldPositionAction undoAction = new FieldPositionAction( 
				_fields,
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// We want to take the snapshot of the current field layout if 
				// _dragFieldLayoutInfo hasn't been created yet (the user hasn't 
				// moved any fields yet). This way when undo is done, we don't
				// go back to auto-generating field layout. We should remain in
				// customized mode even after performing undo.
				// 
				//_fl._dragFieldLayoutInfo, 
				_fl.GetFieldLayoutInfo( true, true ), 
				undoReason, 
				undoLocation);

			// to perform the action we will replace the field layout's drag info
			// and update the fixed locations of the fields
			// SSP 6/26/09 - NAS9.2 Field Chooser
			// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
			// directly accessing the member var.
			// 
			//_fl._dragFieldLayoutInfo = _layoutInfo;
			_fl.SetFieldLayoutInfo( _layoutInfo, false, true );

			foreach (Field field in _fields)
				field.SetFixedLocation(_location);

			// AS 7/7/09 TFS19145/Optimization
			// We shouldn't need to bump the internal version for a position change just 
			// like we don't need to when the field visibility changes.
			//
			//_fl.InvalidateGeneratedStyles(true, false);
			_fl.InvalidateGeneratedStyles(false, false);

			foreach (Field field in _fields)
				dp.RaiseFieldPositionChanged(new FieldPositionChangedEventArgs(field, _reason));

			return undoAction;
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		#region GetUndoReason
		internal static FieldPositionChangeReason GetUndoReason(FieldPositionChangeReason reason)
		{
			switch (reason)
			{
				case FieldPositionChangeReason.Unfixed:
					return FieldPositionChangeReason.Fixed;
				case FieldPositionChangeReason.Fixed:
					return FieldPositionChangeReason.Unfixed;
				case FieldPositionChangeReason.Moved:
					return FieldPositionChangeReason.Moved;
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// 
				case FieldPositionChangeReason.Displayed:
					return FieldPositionChangeReason.Hidden;
				case FieldPositionChangeReason.Hidden:
					return FieldPositionChangeReason.Displayed;
				default:
					Debug.Fail("Unrecognized reason:" + reason.ToString());
					return reason;
			}
		}
		#endregion //GetUndoReason

		#endregion //Methods
	} 
	#endregion //FieldPositionAction

    // JJD 6/10/09 - NA 2009 Vol2 - Record fixing
    #region RecordFixedStateAction
    /// <summary>
	/// Action used to change the fixed state of onew or more records.
	/// </summary>
	internal class RecordFixedStateAction : DataPresenterAction
	{
		#region Member Variables

		private Record[] _records;
		private FieldLayout _fl;
		private FixedRecordLocation _location;

		#endregion //Member Variables

		#region Constructor
		internal RecordFixedStateAction(IList<Record> records, FixedRecordLocation location)
			: base(records[0].DataPresenter)
		{
			_fl = records[0].FieldLayout;
			_records = GridUtilities.ToArray(records);
			_location = location;
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanPerform
		internal protected override bool CanPerform(object context)
		{
			// since we have the resulting layout info which in the case
			// of a splitter position could have affected multiple fields 
			// we need to treat the action as a single atomic operation so 
			// if any field cannot perform the move then we cannot perform 
			// the action
            foreach (Record record in _records)
            {
                if (!record.IsStillValid)
                    return false;

                // make sure that the record still allows it
                if (!record.IsFixedLocationAllowed(_location))
                    return false;
            }

			return true;
		}
		#endregion //CanPerform

		#region Perform
		internal protected override ActionHistory.ActionBase Perform(object context)
		{
			if (!CanPerform(context))
				return null;

			DataPresenterBase dp = this.DataPresenter;

			// raise the changing for all fields - if any cancels then 
			// fail the entire operation since we have to perform it as 
			// a single atomic unit
			foreach (Record record in _records)
			{
                RecordFixedLocationChangingEventArgs changingArgs = new RecordFixedLocationChangingEventArgs(record, _location);
				dp.RaiseRecordFixedLocationChanging(changingArgs);

				if (changingArgs.Cancel)
					return null;
			}

			FixedRecordLocation undoLocation = _records[0].FixedLocation;

			// all the fields have to be in the same location
			foreach (Record record in _records)
			{
				FixedRecordLocation fieldLocation = record.FixedLocation;
				Debug.Assert(fieldLocation == undoLocation);

				if (fieldLocation != undoLocation)
					return null;
			}

			RecordFixedStateAction undoAction = new RecordFixedStateAction(_records, undoLocation);

			foreach (Record record in _records)
				record.FixedLocation = _location;

			foreach (Record record in _records)
				dp.RaiseRecordFixedLocationChanged(new RecordFixedLocationChangedEventArgs(record));

			return undoAction;
		}
		#endregion //Perform

		#endregion //Base class overrides

		#region Methods

		#endregion //Methods
	} 
	#endregion //RecordPositionAction
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