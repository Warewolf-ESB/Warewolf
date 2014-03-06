using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;

// AS 3/3/11 NA 2011.1 - Async Exporting
namespace Infragistics.Windows.DataPresenter
{
	
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


	#region ExportRecordEnumeratorBase
	internal abstract class ExportRecordEnumeratorBase
	{
		#region Member Variables

		private DataPresenterExportControlBase _exportControl;
		private ViewableRecordCollection _rootCollection;
		private Stack<LevelInfo> _levels;
		private IExportOptions _exportOptions;
		private LevelInfo _currentLevel;
		private List<LevelInfo> _availableLevels;
		private ExportProcessResult? _previousResult;

		#endregion //Member Variables

		#region Constructor
		internal ExportRecordEnumeratorBase(DataPresenterExportControlBase exportControl, ViewableRecordCollection sourceRecords, IExportOptions exportOptions)
		{
			_exportControl = exportControl;
			_rootCollection = sourceRecords;
			_exportOptions = exportOptions;
		}
		#endregion //Constructor

		#region Methods

		#region Internal Methods

		#region Process
		internal ExportProcessResult Process(TimeSpan? duration)
		{
			Debug.Assert(_previousResult == null || _previousResult.Value == ExportProcessResult.Timeout, "The enumerator was completed or cancelled!");

			if (_previousResult != null && _previousResult.Value != ExportProcessResult.Timeout)
				return _previousResult.Value;

			int? tickDiff = duration != null ? (int)Math.Round(duration.Value.TotalMilliseconds, MidpointRounding.AwayFromZero) : (int?)null;
			int startTicks = Environment.TickCount;

			CurrentState currentState = _currentLevel == null ? CurrentState.InitializeLevel : _currentLevel.State;
			Record currentRecord = _currentLevel == null ? null : _currentLevel.Record;
			ProcessRecordParams currentRecordParams = _currentLevel == null ? null : _currentLevel.RecordParams;

			while (true)
			{
				switch (currentState)
				{
					// haven't set up the level yet. that involves copying the 
					// recordmanager state and making a copy of the record list
					// that we will be enumerating
					case CurrentState.InitializeLevel:
						{
							#region Initial

							if (_levels == null)
							{
								_levels = new Stack<LevelInfo>();
								this.StartLevel(_rootCollection, null);
							}
							else
							{
								Debug.Assert(currentRecord.HasChildrenInternal);
								// JJD 09/22/11  - TFS84708 - Optimization
								// Use ViewableChildRecordsIfNeeded instead
								//ViewableRecordCollection children = currentRecord.ViewableChildRecords;
								ViewableRecordCollection children = currentRecord.ViewableChildRecordsIfNeeded;

								// if there are no children then move on to the after
								if (children == null || children.Count == 0)
								{
									currentState = CurrentState.ProcessingRecordAfter;
									continue;
								}

								// store the next state on the current level
								_currentLevel.State = CurrentState.ProcessingRecordAfter;

								this.StartLevel(children, _currentLevel.RecordManager);
							}

							currentRecord = null;
							currentRecordParams = _currentLevel.RecordParams;
							currentState = CurrentState.EnumeratingRecords;
							break;

							#endregion //Initial
						}

					// enumerating record. we're about to initialize a record
					case CurrentState.EnumeratingRecords:
						{
							#region EnumeratingRecords

							currentRecord = _currentLevel.MoveNext();

							if (currentRecord != null)
							{
								if (currentRecord is FilterRecord)
									continue;

								DataRecord dr = currentRecord as DataRecord;

								if (dr != null && dr.IsAddRecord)
									continue;

								this.PrepareRecord(currentRecord);
								currentState = CurrentState.InitializingRecordParams;
								break;
							}
							else
							{
								// if this is the last level then just exit
								if (!EndLevel(ref currentState, ref currentRecord, ref currentRecordParams))
									return this.StorePreviousResult(ExportProcessResult.Completed);

								// we can just continue the loop without checking the time as 
								// the above doesn't have a chance of being time intensive
								continue;
							}
							#endregion //EnumeratingRecords
						}
					case CurrentState.InitializingRecordParams:
						{
							#region InitializingRecordParams
							// let the derived enumerator determine whether to traverse into this record,
							// process the siblings, etc.
							bool processRecord = this.InitializeRecordParams(currentRecord, currentRecordParams);

							if (currentRecordParams.TerminateExport)
								return this.StorePreviousResult(ExportProcessResult.Cancelled);

							// if we're not going to process the record then make sure this flag 
							// is off so we know not to process it after we're done with the children
							if (!processRecord)
								_currentLevel.ProcessRecordAfterChildren = false;

							// if the record doesn't have children then we can consider this the 
							// same as skipping the descendants. in this way we can assume that we 
							// should process the descendants after processing the record
							if (!currentRecordParams.SkipDescendants && !currentRecord.HasChildrenInternal)
								currentRecordParams.SkipDescendants = true;

							// if we are supposed to process the record and we do so before the children
							// then do that next
							if (processRecord && !_currentLevel.ProcessRecordAfterChildren)
							{
								currentState = CurrentState.ProcessingRecordBefore;
							}
							else if (!currentRecordParams.SkipDescendants)
							{
								// otherwise if we have descendants then process that now
								currentState = CurrentState.InitializeLevel;
							}
							else
							{
								currentState = CurrentState.ProcessingRecordAfter;
							}
							break;
							#endregion //InitializingRecordParams
						}
					case CurrentState.ProcessingRecordBefore:
						{
							#region ProcessingRecordBefore

							this.ProcessRecord(currentRecord, currentRecordParams);

							// the datapresenterexportcontrol did this extra check
							if (currentRecordParams.TerminateExport)
								return this.StorePreviousResult(ExportProcessResult.Cancelled);

							if (!currentRecordParams.SkipDescendants)
								currentState = CurrentState.InitializeLevel;
							else
								currentState = CurrentState.ProcessingRecordAfter;
							break;

							#endregion //ProcessingRecordBefore
						}
					case CurrentState.ProcessingRecordAfter:
						{
							#region ProcessingRecordAfter

							// if there is nothing to process
							if (_currentLevel.ProcessRecordAfterChildren)
							{
								this.ProcessRecord(currentRecord, currentRecordParams);

								// the datapresenterexportcontrol did this extra check
								if (currentRecordParams.TerminateExport)
									return this.StorePreviousResult(ExportProcessResult.Cancelled);
							}

							// if we skip the siblings then pop the level and continue
							if (currentRecordParams.SkipSiblings)
							{
								if (!this.EndLevel(ref currentState, ref currentRecord, ref currentRecordParams))
									return this.StorePreviousResult(ExportProcessResult.Completed);
							}
							else
							{
								// otherwise move on to the next record
								currentState = CurrentState.EnumeratingRecords;

								// if we didn't do anything above then don't bother with the time check
								if (!_currentLevel.ProcessRecordAfterChildren)
									continue;
							}
							break;
							#endregion //ProcessingRecordAfter
						}
					default:
						Debug.Fail("Unrecognized state");
						return this.StorePreviousResult(ExportProcessResult.Cancelled);
				}

				#region Time Check
				if (null != tickDiff)
				{
					// check the time spent. if we''re beyond our limit 
					// 
					if (Environment.TickCount - startTicks > tickDiff.Value)
					{
						// cache the state and return true to indicate we want 
						// to continue processing later
						_currentLevel.State = currentState;
						return this.StorePreviousResult(ExportProcessResult.Timeout);
					}
				}
				#endregion //Time Check
			}
		}
		#endregion //Process

		#endregion //Internal Methods

		#region Protected Methods
		/// <summary>
		/// Invoked to determine whether the record, its descendants and siblings should be processed.
		/// </summary>
		/// <param name="record"></param>
		/// <param name="recordParams"></param>
		/// <returns>Return true to process the record; otherwise return false to skip it. The values of the <paramref name="recordParams"/> indicate if the descendants/siblings will be processed</returns>
		protected abstract bool InitializeRecordParams(Record record, ProcessRecordParams recordParams);

		/// <summary>
		/// Invoked when the exporter should process the specified record.
		/// </summary>
		/// <param name="record">The record to be processed</param>
		/// <param name="recordParams">The record params from the InitializeRecordParams method</param>
		protected abstract void ProcessRecord(Record record, ProcessRecordParams recordParams);

		#endregion //Protected Methods

		#region Private Methods

		#region EndLevel
		private bool EndLevel(ref CurrentState state, ref Record currentRecord, ref ProcessRecordParams currentRecordParams)
		{
			// pop off a level since we're done with these children
			LevelInfo oldLevel = _levels.Pop();

			// if this is the root list then we're done enumerating
			if (_levels.Count == 0)
				return false;

			if (_availableLevels == null)
				_availableLevels = new List<LevelInfo>();

			_availableLevels.Add(oldLevel);

			// otherwise point to the new top of the stack and pick up 
			// its state where we left off
			_currentLevel = _levels.Peek();
			state = _currentLevel.State;
			currentRecordParams = _currentLevel.RecordParams;
			currentRecord = _currentLevel.Record;
			return true;
		}
		#endregion //EndLevel

		#region InitializeRecordManager
		/// <summary>
		/// Invoked when a different record manager is entered.
		/// </summary>
		/// <param name="recordManager">RecordManager to be initialized</param>
		private void InitializeRecordManager(RecordManager recordManager)
		{
			if (recordManager != null)
			{
				RecordManager associatedRm = recordManager.AssociatedRecordManager;

				if (associatedRm != null)
				{
					// Bypass the root manager and don't try to clone its filters since we never 
					// use the record manager to hold the filters for root records. we 
					// always store them on the fieldlayout
					//
					// MBS 7/28/09 - NA9.2 Excel Exporting
					//if (!view.ExcludeRecordFilters &&
					if ((_exportOptions == null || !_exportOptions.ExcludeRecordFilters) &&
						!recordManager.IsRootManager)
					{
						if (recordManager.RecordFilters.Count == 0)
						{
							if (associatedRm.RecordFiltersIfAllocated != null)
								recordManager.RecordFilters.CloneFrom(associatedRm.RecordFilters);
						}
					}
				}

				ViewableRecordCollection associatedVrc = associatedRm.ViewableRecords;

				// JJD 7/1/09 - NA 2009 Vol 2 - Record fixing
				// Maintain a map for fixed rcds keyed by the associated rcd
				// from the main grid
				if (associatedVrc != null &&
					associatedVrc.CountOfFixedRecordsOnBottom + associatedVrc.CountOfFixedRecordsOnTop > 0)
				{
					recordManager.ViewableRecords.CloneFixedRecords(associatedVrc);
				}
			}
		}
		#endregion //InitializeRecordManager

		#region PrepareRecord
		private void PrepareRecord(Record record)
		{
			Record associatedRcd = record.GetAssociatedRecord();

			if (associatedRcd != null)
				record.CloneAssociatedRecordSettings(associatedRcd, _exportOptions);

			// set the bypass fkag to false to we can raise the InitializeRecord event below
			//
			// MBS 7/24/09 - NA9.2 Excel Exporting
			//this._bypassInitializeRecordEvent = false;
			_exportControl.BypassInitializeRecordEvent = false;

			// Raise the InitilizeRecord event now that we have cloned all of the settings
			
			
			
			
			// JJD 11/17/11 - TFS78651 
			// Pass false for the new sortValueChanged parameter
			//_exportControl.RaiseInitializeRecord(record, false);
			_exportControl.RaiseInitializeRecord(record, false, false);

			// reset the flag for the next record
			//
			// MBS 7/24/09 - NA9.2 Excel Exporting
			//this._bypassInitializeRecordEvent = true;
			_exportControl.BypassInitializeRecordEvent = true;
		}
		#endregion //PrepareRecord

		#region StartLevel
		private void StartLevel(ViewableRecordCollection sourceRecords, RecordManager recordManager)
		{
			// AS 1/4/12 TFS98468
			// Type - supposed to get the record manager for the new collection.
			//
			//RecordManager rm = recordManager;
			RecordManager rm = sourceRecords.RecordManager;

			if (rm != recordManager)
			{
				// AS 1/4/12 TFS98468
				// Pass in the new record manager. recordManager is still the previous manager.
				//
				//this.InitializeRecordManager(recordManager);
				this.InitializeRecordManager(rm);
				recordManager = rm;
			}

			LevelInfo level;

			if (_availableLevels == null || _availableLevels.Count == 0)
			{
				level = new LevelInfo(sourceRecords, recordManager);
			}
			else
			{
				// since these object may be around a while we'll 
				// save them for reuse
				int index = _availableLevels.Count - 1;
				level = _availableLevels[index];
				_availableLevels.RemoveAt(index);

				level.Initialize(sourceRecords, recordManager);
			}

			_currentLevel = level;
			_levels.Push(level);
		}
		#endregion //StartLevel

		#region StorePreviousResult
		private ExportProcessResult StorePreviousResult(ExportProcessResult result)
		{
			_previousResult = result;
			return result;
		} 
		#endregion //StorePreviousResult

		#endregion //Private Methods

		#endregion //Methods

		#region CurrentState enum
		private enum CurrentState
		{
			InitializeLevel,
			EnumeratingRecords,
			InitializingRecordParams,
			ProcessingRecordBefore,
			ProcessingRecordAfter,
		}
		#endregion //CurrentState enum

		#region LevelInfo class
		private class LevelInfo
		{
			#region Member Variables

			internal ViewableRecordCollection SourceRecordCollection;
			internal RecordManager RecordManager;
			internal CurrentState State;

			internal Record Record;
			internal bool ProcessRecordAfterChildren;
			internal ProcessRecordParams RecordParams;

			private List<Record> _recordList;
			private int _recordListCount;
			private int _recordIndex;

			#endregion //Member Variables

			#region Constructor
			internal LevelInfo(ViewableRecordCollection sourceRecords, RecordManager recordManager)
			{
				this.Initialize(sourceRecords, recordManager);
			}
			#endregion //Constructor

			#region Methods

			#region Initialize
			internal void Initialize(ViewableRecordCollection sourceRecords, RecordManager recordManager)
			{
				this.SourceRecordCollection = sourceRecords;
				this.RecordManager = recordManager;

				// JJD 10/16/08 - TFS8092
				// First copy the records from the ViewableRecordCollection into a stack list
				// so that as we walk over the list the count won't change based on 
				// visibility of record being set to Collapsed
				this._recordList = new List<Record>(sourceRecords);

				// JJD 10/16/08 - TFS8092
				// Cache the count since it would be too expensive to get it the loop below
				//int count = records.Count;
				this._recordListCount = this._recordList.Count;

				this.RecordParams = new ProcessRecordParams();

				_recordIndex = 0;
				this.Record = null;
			}
			#endregion //Initialize

			#region MoveNext
			internal Record MoveNext()
			{
				if (_recordIndex >= _recordListCount)
					return null;

				Record r = this._recordList[this._recordIndex++];
				this.Record = r;
				this.ProcessRecordAfterChildren = !r.AreChildrenAfterParent;
				this.RecordParams.Reset();
				return r;
			}
			#endregion //MoveNext

			#endregion //Methods
		}
		#endregion //LevelInfo class
	} 
	#endregion //ExportRecordEnumeratorBase

	#region ExportProcessResult enum
	internal enum ExportProcessResult
	{
		Completed,
		Cancelled,
		Timeout,
	}
	#endregion //ExportProcessResult enum

	#region ExportRecordEnumerator
	internal class ExportRecordEnumerator : ExportRecordEnumeratorBase
	{
		#region Member Variables

		private IDataPresenterExporter _exporter;
		internal int ProcessedRecordCount;

		#endregion //Member Variables

		#region Constructor
		internal ExportRecordEnumerator(DataPresenterExportControl exportControl, ViewableRecordCollection sourceRecords, IExportOptions exportOptions)
			: base(exportControl, sourceRecords, exportOptions)
		{
			_exporter = exportControl.Exporter;
		}
		#endregion //Constructor

		#region Base class overrides

		#region InitializeRecordParams
		protected override bool InitializeRecordParams(Record record, ProcessRecordParams recordParams)
		{
			return _exporter.InitializeRecord(record, recordParams);
		}
		#endregion //InitializeRecordParams

		#region ProcessRecord
		protected override void ProcessRecord(Record record, ProcessRecordParams recordParams)
		{
			_exporter.ProcessRecord(record, recordParams);
			this.ProcessedRecordCount++;
		}
		#endregion //ProcessRecord

		#endregion //Base class overrides
	} 
	#endregion //ExportRecordEnumerator

	#region ReportRecordEnumerator
	internal class ReportRecordEnumerator : ExportRecordEnumeratorBase
	{
		#region Member Variables

		private IList<Record> _flattenedList;

		#endregion //Member Variables

		#region Constructor
		internal ReportRecordEnumerator(DataPresenterReportControl reportControl, ViewableRecordCollection sourceRecords, IExportOptions exportOptions, IList<Record> flattenedList)
			: base(reportControl, sourceRecords, exportOptions)
		{
			Utilities.ValidateNotNull(flattenedList, "flattenedList");
			_flattenedList = flattenedList;
		}
		#endregion //Constructor

		#region Properties
		internal IList<Record> FlattenedList
		{
			get { return _flattenedList; }
		}
		#endregion //Properties

		#region Base class overrides

		#region InitializeRecordParams
		protected override bool InitializeRecordParams(Record record, ProcessRecordParams recordParams)
		{
			Visibility visibility = record.VisibilityResolved;

			// only include the descendants if the record is explicitly visible and expanded
			if (visibility != Visibility.Visible || !record.IsExpanded)
				recordParams.SkipDescendants = true;

			// if the record is collapsed skip it
			return visibility != Visibility.Collapsed;
		}
		#endregion //InitializeRecordParams

		#region ProcessRecord
		protected override void ProcessRecord(Record record, ProcessRecordParams recordParams)
		{
			_flattenedList.Add(record);
		}
		#endregion //ProcessRecord

		#endregion //Base class overrides
	} 
	#endregion //ReportRecordEnumerator

	#region ExportSourceRecordEnumerator
	internal class ExportSourceRecordEnumerator
	{
		#region Member Variables

		private Stack<Level> _levels;
		private Level _currentLevel;

		#endregion //Member Variables

		#region Constructor
		internal ExportSourceRecordEnumerator(RecordManager recordManager)
		{
			_levels = new Stack<Level>();
			this.StartLevel(recordManager);
		}
		#endregion //Constructor

		#region Internal Methods

		#region MoveNext
		internal bool MoveNext()
		{
			while (_levels.Count > 0)
			{
				if (_currentLevel.CurrentRecord == null)
				{
					IList<DataRecord> records = _currentLevel.RecordManager.Unsorted;

					if (_currentLevel.Index < records.Count)
					{
						DataRecord record = records[_currentLevel.Index];

						// since we don't need the record we're just going to use the current record 
						// when we are traversing into the children. if it has no expandable field 
						// records then when we come back in we can just move to the next data record
						if (record.FieldLayout.Fields.NotCollapsedExpandableFieldsCount > 0)
							_currentLevel.CurrentRecord = record;

						_currentLevel.Index++;
						return true;
					}
				}
				else
				{
					ExpandableFieldRecordCollection childRecords = _currentLevel.CurrentRecord.ChildRecords;

					for (int i = _currentLevel.CurrentRecordChildIndex, count = childRecords.Count; i < count; i++)
					{
						ExpandableFieldRecord childRecord = childRecords[i];
						RecordManager rm = childRecord.ChildRecordManager;

						if (rm != null)
						{
							_currentLevel.CurrentRecordChildIndex = i + 1;
							this.StartLevel(rm);
							return true;
						}
					}

					// if we get here then we've enumerated all the child managers so we can 
					// move on to the next sibling record
					_currentLevel.CurrentRecord = null;
					continue;
				}

				this.EndCurrentLevel();
			}

			return false;
		}
		#endregion //MoveNext

		#region Process
		internal ExportProcessResult Process(TimeSpan duration)
		{
			int tickDiff = (int)Math.Round(duration.TotalMilliseconds, MidpointRounding.AwayFromZero);
			int startTicks = Environment.TickCount;
			int count = 0;
			const int RecordsPerTimeCheck = 100;

			while (this.MoveNext())
			{
				count++;

				if (count % RecordsPerTimeCheck != 0)
					continue;

				#region Time Check

				// check the time spent. if we're beyond our limit 
				// 
				if (Environment.TickCount - startTicks > tickDiff)
					return ExportProcessResult.Timeout;

				#endregion //Time Check
			}

			return ExportProcessResult.Completed;
		}
		#endregion //Process

		#endregion //Internal Methods

		#region Private Methods

		#region EndCurrentLevel
		private void EndCurrentLevel()
		{
			_levels.Pop();
			_currentLevel = _levels.Count > 0 ? _levels.Peek() : null;
		}
		#endregion //EndCurrentLevel

		#region StartLevel
		private void StartLevel(RecordManager rm)
		{
			Level level = new Level();
			level.RecordManager = rm;
			_levels.Push(level);
			_currentLevel = level;

			if (rm.IsResetPending)
				rm.OnSourceCollectionReset();
		}
		#endregion //StartLevel

		#endregion //Private Methods

		#region Level class
		private class Level
		{
			internal RecordManager RecordManager;
			internal int Index;
			internal DataRecord CurrentRecord;
			internal int CurrentRecordChildIndex;
		}
		#endregion //Level class

	} 
	#endregion //ExportSourceRecordEnumerator
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