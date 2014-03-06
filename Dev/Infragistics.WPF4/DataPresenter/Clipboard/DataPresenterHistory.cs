using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.DataPresenter
{
	internal class DataPresenterHistory
    {
        #region Member Variables

        private DataPresenterBase _owner;
		private UndoManager _undoManager;

        #endregion //Member Variables

        #region Constructor
        internal DataPresenterHistory(DataPresenterBase owner)
        {
            _owner = owner;
			_undoManager = new UndoManager(owner.UndoLimit);
        } 
        #endregion //Constructor

		#region Properties

		#region UndoLimit
		public int UndoLimit
		{
			
			
			get { return _undoManager.UndoLimit; }
			set
			{
				if (null != _undoManager)
					_undoManager.UndoLimit = value;
			}
		}
		#endregion //UndoLimit

		#region UndoManager
		private UndoManager UndoManager
		{
			get
			{
				return _undoManager;
			}
		} 
		#endregion //UndoManager

		#endregion //Properties

		#region Methods

		#region AddUndoActionInternal
		/// <summary>
		/// Invoked when an action was performed directly and the corresponding undo action needs to be added to the undo history.
		/// </summary>
		/// <remarks> As with a normal perform, the redo history will be cleared.</remarks>
		/// <param name="action">The undo action to store</param>
		internal void AddUndoActionInternal(DataPresenterAction action)
		{
			if (_owner.IsUndoEnabled)
			{
				this.UndoManager.AddToUndo(action);
			}
		}
		#endregion //AddUndoActionInternal

		#region CanUndo/Redo
		internal bool CanRedo()
		{
			return this.UndoManager.CanRedo();
		}

		internal bool CanUndo()
		{
			return this.UndoManager.CanUndo();
		} 
		#endregion //CanUndo/Redo

		#region Clear
		internal void Clear()
		{
			this.UndoManager.Clear();
		} 
		#endregion //Clear

		#region CreateUndeleteDescendantInfo
		internal DescendantRecordInfo CreateUndeleteDescendantInfo(DataRecord[] oldRecords)
		{
			DescendantRecordInfo descendantInfo = new DescendantRecordInfo(oldRecords);

			Action<ActionHistory.ActionBase> callback = delegate(ActionHistory.ActionBase action)
			{
				DataPresenterAction dpAction = action as DataPresenterAction;

				if (null != dpAction && dpAction.DataPresenter == _owner)
					dpAction.AddUndeleteDescendantInfo(descendantInfo);
			};

			this.UndoManager.ForEach(callback);

			return descendantInfo;
		}
		#endregion //CreateUndeleteDescendantInfo

		// AS 8/21/09 TFS19349
		#region FixGroupExpansionHistory
		internal void FixGroupExpansionHistory(object context)
		{
			bool isUndo = UndoManager.IsUndoContext(context);
			bool isRedo = UndoManager.IsRedoContext(context);
			HashSet fixedRecords = new HashSet();

			Action<ActionHistory.ActionBase> callback = delegate(ActionHistory.ActionBase action)
			{
				DataPresenterAction dpAction = action as DataPresenterAction;

				if (null != dpAction && dpAction.DataPresenter == _owner)
				{
					RecordExpansionAction expansionAction = dpAction as RecordExpansionAction;

					if (null != expansionAction)
						expansionAction.FixGroupByRecords(fixedRecords);
				}
			};

			this.UndoManager.ForEach(callback, isUndo, isRedo);
		} 
		#endregion //FixGroupExpansionHistory

		#region FixHistoryAfterUndelete
		internal void FixHistoryAfterUndelete(Dictionary<object, DataRecord> oldDataItemToNewRecord)
		{
			Action<ActionHistory.ActionBase> callback = delegate(ActionHistory.ActionBase action)
			{
				DataPresenterAction dpAction = action as DataPresenterAction;

				if (null != dpAction && dpAction.DataPresenter == _owner)
					dpAction.OnRecordsUndeleted(oldDataItemToNewRecord);
			};

			this.UndoManager.ForEach(callback);
		}
		#endregion //FixHistoryAfterUndelete

		#region OnCustomizationsChanged
		internal void OnCustomizationsChanged(FieldLayout fieldLayout, CustomizationType customizations)
		{
			UndoManager.ActionFilter callback = delegate(ActionHistory.ActionBase action)
			{
				DataPresenterAction dpAction = action as DataPresenterAction;

				if (null != dpAction && dpAction.DataPresenter == _owner)
				{
					action = dpAction.OnCustomizationsChanged(fieldLayout, customizations);
				}

				return action;
			};

			this.UndoManager.Filter(callback);
		}
		#endregion //OnCustomizationsChanged

		#region PerformAction
		internal bool PerformAction(ActionHistory.ActionBase action, object context)
		{
			bool result = this.UndoManager.PerformAction(action, context);

			if (!_owner.IsUndoEnabled)
				this.Clear();

			return result;
		}
		#endregion //PerformAction

		#region PerformUndoRedo
		internal bool PerformUndoRedo(bool isUndo)
		{
			bool handled = false;

			if (_owner.IsUndoEnabled)
			{
				// AS 3/10/11 NA 2011.1 - Async Exporting
				if (!_owner.VerifyOperationIsAllowed(UIOperation.Undo))
					return false;

				var activeCell = _owner.ActiveCell;

				// AS 6/27/11 TFS78060
				// Before we perform an undo/redo then make sure that we are not in edit
				// mode. Otherwise if the value of the cell was changed while we were in 
				// edit mode then the OriginalValue of the cell will still be the value 
				// from the point at which the cell entered edit mode so when we change 
				// the active cell (possibly during the action being invoked as was the 
				// case here) then an undo action would have been added even though the 
				// user was not changing the value but the value change occurred as a 
				// result of the undo.
				//
				if (null != activeCell && activeCell.IsInEditMode)
				{
					var cvp = CellValuePresenter.FromCell(activeCell);

					if (null != cvp && cvp.IsInEditMode)
					{
						cvp.EndEditMode(true, false);

						if (activeCell.IsInEditMode)
							return true;
					}
				}

				UndoManager manager = this.UndoManager;

				if (isUndo && manager.CanUndo())
				{
					handled = manager.Undo();
				}
				else if (!isUndo && manager.CanRedo())
				{
					handled = manager.Redo();
				}
			}

			return handled;
		}
		#endregion //PerformUndoRedo

		#endregion //Methods
	}

	internal class DescendantRecordInfo
	{
		#region Member Variables

		private static readonly IList<Record> ListPlaceholder = new ReadOnlyCollection<Record>(new Record[0]);
		private Dictionary<Record, IList<Record>> _ancestorToDescendants;
		private List<DataRecord> _records;
		private Dictionary<FieldLayout, List<Field>> _expandableFields;

		#endregion //Member Variables

		#region Constructor
		internal DescendantRecordInfo(DataRecord[] recordsToTrack)
		{
			_expandableFields = new Dictionary<FieldLayout, List<Field>>();
			_records = new List<DataRecord>(recordsToTrack);
			_ancestorToDescendants = new Dictionary<Record, IList<Record>>();

			foreach (DataRecord record in recordsToTrack)
				_ancestorToDescendants[record] = ListPlaceholder;

		} 
		#endregion //Constructor

		#region Methods

		#region AddRecord
		internal bool AddRecord(Record record)
		{
			return AddRecord(record, null);
		}

		private bool AddRecord(Record record, Record child)
		{
			if (record == null)
				return false;

			IList<Record> records;

			// see if we've encountered this record...
			if (!_ancestorToDescendants.TryGetValue(record, out records))
			{
				// if not then lets see if it gets added to the parent
				Record parent = record is ExpandableFieldRecord
					? record.ParentRecord
					: record.RecordManager.ParentRecord;

				// if the parent is being tracked (or is now) then 
				// we want to track this record as well
				if (AddRecord(parent, record))
				{
					records = ListPlaceholder;
				}

				// add it to the list so we know not to process it again
				_ancestorToDescendants.Add(record, records);
			}

			// if we're tracking this record and we're given a child...
			if (null != records && null != child)
			{
				// if we have only stored a placeholder for this record
				// then store a list now to hold the child
				if (records == ListPlaceholder)
				{
					records = new List<Record>(3);
					_ancestorToDescendants[record] = records;
				}

				// store the child
				records.Add(child);
			}

			return null != records;
		}
		#endregion //AddRecord

		#region GetChildren
		internal IList<Record> GetChildren(Record record)
		{
			IList<Record> records;
			_ancestorToDescendants.TryGetValue(record, out records);
			return records;
		}
		#endregion //GetChildren

		#region GetExpandableFields
		internal IList<Field> GetExpandableFields(FieldLayout fl)
		{
			List<Field> fields;

			if (!_expandableFields.TryGetValue(fl, out fields))
			{
				fields = GetExpandableFieldsImpl(fl);
				_expandableFields[fl] = fields;
			}

			return fields;
		}

		private static List<Field> GetExpandableFieldsImpl(FieldLayout fl)
		{
			List<Field> expandableFields = new List<Field>();

			foreach (Field f in fl.Fields)
			{
				if (f.IsExpandableResolved)
					expandableFields.Add(f);
			}

			return expandableFields;
		} 
		#endregion //GetExpandableFields

		#endregion //Methods
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