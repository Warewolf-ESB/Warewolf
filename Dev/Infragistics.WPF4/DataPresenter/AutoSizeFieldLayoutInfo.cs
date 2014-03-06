using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using Infragistics.Windows.Helpers;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	// AS 7/1/09 NA 2009.2 Field Sizing
	/// <summary>
	/// Helper class to manage autosize information for fields in a fieldlayout
	/// </summary>
	internal class AutoSizeFieldLayoutInfo
	{
		#region Member Variables

		private List<Field> _recordsInViewFields = new List<Field>();
		private List<Field> _allRecordFields = new List<Field>();
		private List<Field> _viewableRecordFields = new List<Field>();
		private List<Field> _pendingInitialSizeFields = new List<Field>();

		private FieldLayout _owner;

		// this is used to track which list we are maintaining the field
		private Dictionary<Field, FieldAutoSizeScope> _scopes = new Dictionary<Field, FieldAutoSizeScope>();

		internal static readonly int OperationTypeCount;
		private AutoSizeFieldOperation[] _pendingOperations;
		private int _pendingOperationTypes;

		// maintains a weak list of records & fields that have been processed for 
		// the records in view
		private AutoSizeFieldHelper.RecordFieldList _recordsInView;

		private int _layoutManagerVersion = -1;
		private int _lastFieldsVersion;
		private int _lastTemplateVersion;

		private int _autoSizeCount;

		private bool _hasOutOfViewAutoSizeFields;
		private bool _hasViewableRecordFields;
		private bool _hasAllRecordFields;

		private const bool DebugAutoSize = false;

		private int _maxPendingParameters = -1;

		// AS 3/14/11 TFS67970 - Optimization
		private int _fixedRecordInViewFieldCount = 0;
		private int _fixedFieldVersion = -1;

		#endregion //Member Variables

		#region Constructor
		static AutoSizeFieldLayoutInfo()
		{
			OperationTypeCount = Enum.GetValues(typeof(OperationType)).Length;
		}

		internal AutoSizeFieldLayoutInfo(FieldLayout owner)
		{
			_owner = owner;
		}
		#endregion //Constructor

		#region Properties

		#region DPMaxAutoSizeAllRecordsDepth
		private int DPMaxAutoSizeAllRecordsDepth
		{
			get
			{
				return null != _owner.DataPresenter ? _owner.DataPresenter.AllRecordsAutoSizeMaxDepth : -1;
			}
		} 
		#endregion //DPMaxAutoSizeAllRecordsDepth

		#region DPHasAllRecordFields
		private bool DPHasAllRecordFields
		{
			get
			{
				return null != _owner.DataPresenter ? _owner.DataPresenter.HasAutoSizeAllRecordFields : false;
			}
		}
		#endregion //DPHasAllRecordFields

		#region HasAllRecordFields
		internal bool HasAllRecordFields
		{
			get { return _hasAllRecordFields; }
			set
			{
				if (value != _hasAllRecordFields)
				{
					_hasAllRecordFields = value;

					if (null != _owner.DataPresenter)
					{
						_owner.DataPresenter.OnAutoSizeAllRecordsChanged(_owner, value);
					}
				}
			}
		}
		#endregion //HasAllRecordFields

		#region DPHasViewableRecordFields
		private bool DPHasViewableRecordFields
		{
			get
			{
				return null != _owner.DataPresenter ? _owner.DataPresenter.FieldLayoutAutoSizeViewableCount > 0 : false;
			}
		} 
		#endregion //DPHasViewableRecordFields

		#region HasViewableRecordFields
		internal bool HasViewableRecordFields
		{
			get { return _hasViewableRecordFields; }
			set
			{
				if (value != _hasViewableRecordFields)
				{
					_hasViewableRecordFields = value;

					Debug.Assert(!value || _owner.IsInitialRecordLoaded || (_owner.DataPresenter != null && _owner.DataPresenter.RecordManager.FieldLayout == _owner));

					if (null != _owner.DataPresenter)
						_owner.DataPresenter.FieldLayoutAutoSizeViewableCount += value ? 1 : -1;
				}
			}
		} 
		#endregion //HasViewableRecordFields

		// AS 2/26/10 TFS28159
		#region HasRecordsInViewFields
		private bool _hasRecordsInViewFields;

		internal bool HasRecordsInViewFields
		{
			get { return _hasRecordsInViewFields; }
			set
			{
				if (value != _hasRecordsInViewFields)
				{
					_hasRecordsInViewFields = value;

					//Debug.Assert(!value || _owner.IsInitialRecordLoaded || (_owner.DataPresenter != null && _owner.DataPresenter.RecordManager.FieldLayout == _owner) || _owner.DataPresenter.IsExportControl);

					if (null != _owner.DataPresenter)
						_owner.DataPresenter.FieldLayoutAutoSizeRecordsInViewCount += value ? 1 : -1;
				}
			}
		}
		#endregion //HasRecordsInViewFields

		#endregion //Properties

		#region Methods

		#region Private

		#region AddViewableRecordFieldLayouts
		private void AddViewableRecordFieldLayouts(FieldLayout parent, List<FieldLayout> fieldLayouts, Dictionary<FieldLayout, List<FieldLayout>> ancestorMap)
		{
			List<FieldLayout> childFieldLayouts;

			if (ancestorMap.TryGetValue(parent, out childFieldLayouts))
			{
				// remove the parent in case there is a circular dependency
				ancestorMap.Remove(parent);

				foreach (FieldLayout child in childFieldLayouts)
				{
					if (!child.IsInitialRecordLoaded)
						continue;

					child.AutoSizeInfo.Verify();

					if (child.AutoSizeInfo._hasViewableRecordFields)
						fieldLayouts.Add(child);

					AddViewableRecordFieldLayouts(child, fieldLayouts, ancestorMap);
				}
			}
		}
		#endregion //AddViewableRecordFieldLayouts

		#region AddPendingOperation

		/// <summary>
		/// This overload should only be used for operations that would not be aggregated.
		/// </summary>
		private void AddPendingOperation(OperationType operationType)
		{
			this.AddPendingOperation(operationType, null);
		}

		private void AddPendingOperation(OperationType operationType, object parameter)
		{
			int index = (int)operationType;

			// if this is the first operation then we need to register with the datapresenter
			// so it can call us back when the pending operations should be processed
			if (_pendingOperations == null)
			{
				Debug.WriteLineIf(DebugAutoSize, string.Format("First Pending - Op:{0} FL:{1}", operationType, _owner), "AutoSize - Add Pending:" + DateTime.Now.ToString("hh:mm:ss:ffffff"));

				_pendingOperations = new AutoSizeFieldOperation[OperationTypeCount];

				DataPresenterBase dp = _owner.DataPresenter;
				Debug.Assert(null != dp);

				// let the datapresenter know so we can force the processing before the display is updated 
				if (dp == null)
					return;

				dp.OnAutoSizePending(_owner);

				//// cache the scroll count when the first cached operation comes in to use as a basis 
				// for determining how to cap the pending operations that involve records
				int percentMax = (int)(((IViewPanelInfo)dp).OverallScrollCount * .10);
				_maxPendingParameters = Math.Max(500, percentMax);
			}

			AutoSizeFieldOperation operation = _pendingOperations[index];

			// if there is no operation pending yet...
			if (null == operation)
			{
				#region First Instance of OperationType

				Debug.WriteLineIf(DebugAutoSize, string.Format("First Pending of Type - Op:{0} FL:{1}", operationType, _owner), "AutoSize - Add Pending:" + DateTime.Now.ToString("hh:mm:ss:ffffff"));

				// create one for this type
				operation = new AutoSizeFieldOperation(operationType);

				if (null != parameter)
				{
					// if the operation has a parameter...
					operation.Parameters = new HashSet();

					HashSet hashSetParameter = parameter as HashSet;

					if (null == hashSetParameter)
					{
						operation.Parameters.Add(parameter);
					}
					else
					{
						// note we're not just taking a reference since this same hashset since 
						// the field layout may gets adds from multiple parent field layouts
						operation.Parameters.AddItems(hashSetParameter);
					}
				}

				_pendingOperationTypes |= 1 << index;
				_pendingOperations[index] = operation; 

				#endregion //First Instance of OperationType
			}
			else
			{
				#region Add To Existing Operation Info
				// if we already have logged a pending operation for this type, see if 
				// we can/should combine the parameter information

				// if this is not a "global" aggregate operation already then update it as needed
				if (operation.Parameters != null)
				{
					// if this is becoming a global operation then just clear the parameters
					if (parameter == null)
					{
						Debug.WriteLineIf(DebugAutoSize, string.Format("Clearing Parameters - Op:{0}, FL:{1}", operationType, _owner), "AutoSize - Add Pending:" + DateTime.Now.ToString("hh:mm:ss:ffffff"));

						operation.Parameters = null;
					}
					else
					{
						HashSet hashSetParameter = parameter as HashSet;

						int newCount = operation.Parameters.Count;

						if (hashSetParameter != null)
							newCount += hashSetParameter.Count;
						else
							newCount++;

						// once we hit a threshold just treat this as a global operation
						if (newCount >= this.GetMaxParameterCount(operationType))
						{
							Debug.WriteLineIf(DebugAutoSize, string.Format("Exceeded Max - Op:{0}, NewCount:{1}, FL:{2}", operationType, newCount, _owner), "AutoSize - Add Pending:" + DateTime.Now.ToString("hh:mm:ss:ffffff"));
							operation.Parameters = null;
						}
						else if (hashSetParameter != null)
							operation.Parameters.AddItems(hashSetParameter);
						else
							operation.Parameters.Add(parameter);
					}
				}

				#endregion //Add To Existing Operation Info			
			}
		}

		#endregion //AddPendingOperation

		#region ApplyAutoSize
		private static void ApplyAutoSize(AutoSizeFieldHelper.FieldInfo info, Field.FieldResizeInfo resizeInfo, bool increaseOnly, bool isHorizontal, ItemSizeType sizeType)
		{
			double labelExtent = info.GetExtent(true);
			double cellExtent = info.GetExtent(false);

			// we're going to store the auto size information in a separate
			// structure from the user resized information. in this way we 
			// don't have to worry about losing or recalculating this if the 
			// customizations are cleared, we don't have to worry about whether 
			// a layout was loaded that had customized extents, etc.
			ApplyAutoSize(resizeInfo, increaseOnly, !isHorizontal, sizeType, labelExtent, true);
			ApplyAutoSize(resizeInfo, increaseOnly, !isHorizontal, sizeType, cellExtent, false);
		}

		private static void ApplyAutoSize(Field.FieldResizeInfo resizeInfo, bool increaseOnly, bool isWidth, ItemSizeType sizeType, double extent, bool label)
		{
			if (!double.IsNaN(extent))
			{
				if (increaseOnly)
				{
					FieldSize currentSize = resizeInfo.GetSize(label, isWidth);

					if (currentSize.Type == sizeType)
						extent = AutoSizeFieldHelper.Max(extent, currentSize.Value);
				}

				resizeInfo.SetSize(label, isWidth, new FieldSize(extent, sizeType));
			}
		}
		#endregion //ApplyAutoSize

		#region GetAutoSizeScope
		private static FieldAutoSizeScope? GetAutoSizeScope(bool isHorz, Field field, bool? isLabel)
		{
			FieldAutoSizeScope? scope = null;
			FieldLength fieldLen = field.GetWidthOrHeight(!isHorz);

			// if this is an auto sized field see if it has been resized
			if (fieldLen.IsAuto)
			{
				bool useScope = false;
				Field.FieldResizeInfo resizeInfo = field.ExplicitResizeInfoIfAllocated;

				if (null == resizeInfo)
					useScope = true;
				else
				{
					bool hasExplicitLabel = resizeInfo.GetSize(true, !isHorz).HasExplicitSize;
					bool hasExplicitCell = resizeInfo.GetSize(false, !isHorz).HasExplicitSize;

					if (isLabel == null)
						useScope = !hasExplicitCell || !hasExplicitLabel;
					else
					{
						if (isLabel.Value && !hasExplicitLabel)
							useScope = true;
						else if (!isLabel.Value && !hasExplicitCell)
							useScope = true;
					}
				}

				if (useScope)
					scope = field.AutoSizeScopeResolved;
			}
			return scope;
		}
		#endregion //GetAutoSizeScope

		#region GetFieldList
		private List<Field> GetFieldList(FieldAutoSizeScope scope)
		{
			switch (scope)
			{
				case FieldAutoSizeScope.AllRecords:
					return _allRecordFields;
				case FieldAutoSizeScope.ViewableRecords:
					return _viewableRecordFields;
				case FieldAutoSizeScope.RecordsInView:
					return _recordsInViewFields;
				default:
					Debug.Fail("Unexpected scope:" + scope.ToString());
					return _recordsInViewFields;
			}
		}
		#endregion //GetFieldList

		#region GetMaxParameterCount
		private int GetMaxParameterCount(OperationType operationType)
		{
			switch (operationType)
			{
				case OperationType.LabelsChanged:
				case OperationType.EditModeChange:
					return int.MaxValue;
				case OperationType.OutOfViewRecords:
				case OperationType.ViewableRecordsChanged:
				case OperationType.TraverseAllRecords:
					return _maxPendingParameters;
				case OperationType.Initial:
				case OperationType.RecordsInView:
					Debug.Fail("Why was there a parameter for this type?");
					return 1;
				default:
					Debug.Fail("Unrecognized operation:" + operationType.ToString());
					return 1;
			}
		}
		#endregion //GetMaxParameterCount

		#region HasPendingOperation
		private bool HasPendingOperation(OperationType operationType)
		{
			return 0 != (_pendingOperationTypes & (1 << (int)operationType));
		} 
		#endregion //HasPendingOperation

		#region InitializeAutoSizedFieldsImpl
		private void InitializeAutoSizedFieldsImpl(RecordsInViewHelper recordsInViewHelper )
		{
			if (_pendingInitialSizeFields.Count == 0)
				return;

			Field[] fields = _pendingInitialSizeFields.ToArray();
			_pendingInitialSizeFields.Clear();

			bool isHorz = _owner.IsHorizontal;
			List<Field> autoFields = new List<Field>();

			// get all the fields that should be sized to content initially
			foreach (Field field in fields)
			{
				FieldLength len = field.GetWidthOrHeight(!isHorz);

				// make sure we don't process the initial autosize again for these fields
				field._hasPendingInitialAutoSize = false;

				if (len.IsAnyAuto && field.AutoSizeOptionsResolved != FieldAutoSizeOptions.None)
				{
					autoFields.Add(field);
				}
			}

			if (autoFields.Count > 0)
			{
				DataPresenterBase dp = _owner.DataPresenter;
				int recordManagerDepth = AutoSizeFieldHelper.GetNestingDepth(_owner);

				// note I'm passing in true for increase only in case this is an initial auto 
				// where a layout has been loaded in which case we want to use the current size
				// as the base size
				this.PerformAutoSize(isHorz, autoFields, dp.Records, recordManagerDepth, null, true, OperationType.Initial, FieldAutoSizeOptions.All, recordsInViewHelper );
			}
		}

		#endregion //InitializeAutoSizedFieldsImpl

		// AS 3/22/10 TFS29701
		#region IsValid
		private bool IsValid(CellValuePresenter cvp)
		{
			if (null == cvp)
				return false;

			if (!IsValid(cvp.Field))
				return false;

			if (!IsValid(cvp.Record))
				return false;

			return true;
		}

		private bool IsValid(Field field)
		{
			return null != field && field.Index >= 0 && field.Owner == _owner;
		}

		private bool IsValid(Record record)
		{
			return null != record && record.IsStillValid;
		}
		#endregion //IsValid

		#region OnRecordChanged
		private void OnRecordChanged(Record recordContext)
		{
			if (_hasOutOfViewAutoSizeFields)
			{
				this.AddPendingOperation(OperationType.OutOfViewRecords, recordContext);
			}
		}
		#endregion //OnRecordChanged

		#region OnRemoveField
		private void OnRemoveField(Field field)
		{
			FieldAutoSizeScope scope;
			if (_scopes.TryGetValue(field, out scope))
			{
				_scopes.Remove(field);
				List<Field> fieldList = GetFieldList(scope);
				fieldList.Remove(field);

				if (null != _recordsInView && scope == FieldAutoSizeScope.RecordsInView)
					_recordsInView.Remove(field);
			}
		}
		#endregion //OnRemoveField

		#region OnViewableRecordChanged
		private void OnViewableRecordChanged(Record recordContext)
		{
			// if there are no autosized fields that traverse viewable records then 
			// we can skip this processing
			if (!_hasViewableRecordFields && !this.DPHasViewableRecordFields)
				return;

			this.AddPendingOperation(OperationType.ViewableRecordsChanged, recordContext);
		}
		#endregion //OnViewableRecordChanged

		#region PerformAutoSize
		private void PerformAutoSize(bool isHorz, IEnumerable<Field> autoFields, RecordCollectionBase records,
			int recordManagerDepth, HashSet recordsSubset, bool increaseOnly, OperationType operationType, FieldAutoSizeOptions allowedOptions
			, RecordsInViewHelper recordsInViewHelper )
		{
			AutoSizeFieldHelper	helper = new AutoSizeFieldHelper(autoFields, _owner, allowedOptions);

			bool hasCalculated = false;

			
			
			
			AutoSizeCalculationFlags flags = AutoSizeCalculationFlags.SkipEditCell;

			if (operationType == OperationType.RecordsInView)
			{
				// if the fields collection has been changed then we cannot rely on the cached indexes
				int ownerFieldVersion = _owner.Fields.Version;

				if (_lastFieldsVersion != ownerFieldVersion)
					_recordsInView = null;

				_lastFieldsVersion = ownerFieldVersion;
				hasCalculated = helper.Calculate(flags, ref _recordsInView, true, recordsInViewHelper );
			}
			else
			{
				if (recordsSubset != null)
				{
					bool recursive = operationType == OperationType.ViewableRecordsChanged;
					IEnumerable<Record> outOfViewRecords = new OutOfViewRecordEnumerable(recordsSubset);
					hasCalculated = helper.Calculate(outOfViewRecords, recursive, _owner.MaxRecordManagerDepth, flags);
				}
				else
					hasCalculated = helper.Calculate(records, recordManagerDepth, flags, recordsInViewHelper );
			}

			bool isInitialSize = operationType == OperationType.Initial;

			if (!hasCalculated && !isInitialSize)
				return;

			// AS 9/30/09 TFS22891
			// The autoFields enumerable may be mutable in which case the list of fields may change 
			// during the calculation so just iterate the fields in the helper.
			//
			//foreach (Field field in autoFields)
			foreach(Field field in helper.GetFields())
			{
				AutoSizeFieldHelper.FieldInfo info = helper.GetFieldInfo(field);

				ApplyAutoSize(info, field.ExplicitResizeInfo, increaseOnly, isHorz, ItemSizeType.AutoMode);

				if (isInitialSize && double.IsNaN(info.CellExtent) && (info.Options & AutoSizeFieldHelper.RecordOptions) != 0)
				{
					// if there are no cells in view yet then wait to process this field again
					// but only if this is an initial auto. if its a auto field then we will 
					// process it when something is available (e.g. expansion, visibility, record added).
					//
					if (field.GetWidthOrHeight(!isHorz).IsInitialAuto)
					{
						field._hasPendingInitialAutoSize = true;
						_pendingInitialSizeFields.Add(field);
					}
				}
				else
				{
					if (field._hasPendingInitialAutoSize)
					{
						// if we got an extent for the cell or we're not supposed to include cells
						// in the calculation then clear the pending initial state
						if (!double.IsNaN(info.CellExtent) || (info.Options & AutoSizeFieldHelper.RecordOptions) == 0)
						{
							field._hasPendingInitialAutoSize = false;
							_pendingInitialSizeFields.Remove(field);
						}
					}
				}
			}
		}
		#endregion //PerformAutoSize

		#region ProcessEditCellValueChanged
		private void ProcessEditCellValueChanged(HashSet editChangeInfo)
		{
			if (editChangeInfo == null)
			{
				DataPresenterBase dp = _owner.DataPresenter;

				if (dp != null)
				{
					CellValuePresenter cvp = dp.EditHelper.CellValuePresenter;
					Field.FieldResizeInfo preEditInfo = dp.EditHelper.PreEditAutoSize;

					// AS 3/22/10 TFS29701
					//if (null != cvp && null != preEditInfo && cvp.Field.Owner == _owner)
					if (null != preEditInfo && IsValid(cvp))
						ProcessEditCellValueChanged(cvp, preEditInfo);

				}
			}
			else
			{
				Debug.Assert(editChangeInfo.Count == 1, "Should we try to skip some of these?");

				foreach (EditModeChangeInfo changeInfo in editChangeInfo)
				{
					ProcessEditCellValueChanged(changeInfo.CVP, changeInfo.PreEditAutoSize);
				}
			}
		}

		private void ProcessEditCellValueChanged(CellValuePresenter cvp, Field.FieldResizeInfo preEditAutoSize)
		{
			Field f = cvp.Field;
			Debug.Assert(f.Owner == _owner);

			// AS 3/22/10 TFS29701
			//if (f != cvp.Field) 
			if (!IsValid(cvp))
				return;

			bool isHorz = _owner.IsHorizontal;

			if (!this.IsInAutoSizeMode(f, false))
				return;

			// force a layout updated in case the elements haven't been updated yet
			cvp.UpdateLayout();

			// find out the new desired size
			cvp.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			Size desiredSize = cvp.DesiredSize;
			double cellExtent = isHorz ? desiredSize.Height : desiredSize.Width;
			cvp.InvalidateMeasure();

			// apply the change
			AutoSizeFieldHelper.FieldInfo fieldInfo = new AutoSizeFieldHelper.FieldInfo(f);

			// initialize using the original size
			fieldInfo.AddCellExtent(preEditAutoSize.GetSize(false, !isHorz).Value);
			fieldInfo.AddLabelExtent(preEditAutoSize.GetSize(true, !isHorz).Value);

			// then apply the new edit cell size
			fieldInfo.AddCellExtent(cellExtent);

			// AS 11/9/11 TFS91453
			fieldInfo.RoundExtents();

			ApplyAutoSize(fieldInfo, f.ExplicitResizeInfo, false, isHorz, ItemSizeType.AutoMode);
		}
		#endregion //ProcessEditCellValueChanged

		#region TransferViewableRecords
		private void TransferViewableRecords(HashSet records)
		{
			

			// if there is at least one expandable field in the field layout...
			if (_owner.Fields.ExpandableFieldsCount > 0)
			{
				Dictionary<FieldLayout, List<FieldLayout>> ancestorMap = new Dictionary<FieldLayout, List<FieldLayout>>();
				FieldLayout ownerParent = _owner.ParentFieldLayout;

				foreach (FieldLayout fl in _owner.DataPresenter.FieldLayouts)
				{
					FieldLayout parentFieldLayout = fl.ParentFieldLayout;

					// skip root field layouts and sibling field layouts
					if (null != parentFieldLayout && parentFieldLayout != ownerParent)
					{
						List<FieldLayout> childFieldLayouts;

						if (!ancestorMap.TryGetValue(parentFieldLayout, out childFieldLayouts))
						{
							childFieldLayouts = new List<FieldLayout>();
							ancestorMap[parentFieldLayout] = childFieldLayouts;
						}

						childFieldLayouts.Add(fl);
					}
				}

				if (ancestorMap.Count == 0)
					return;

				// build a list of the field layouts that are descendants of the owning 
				// field layout and tell them to process the viewable records for which 
				// we have logged changes
				List<FieldLayout> layoutsToTraverse = new List<FieldLayout>();
				AddViewableRecordFieldLayouts(_owner, layoutsToTraverse, ancestorMap);

				foreach (FieldLayout fl in layoutsToTraverse)
				{
					fl.AutoSizeInfo.AddPendingOperation(OperationType.ViewableRecordsChanged, records);
				}
			}
		} 
		#endregion //TransferViewableRecords

		// AS 3/14/11 TFS67970 - Optimization
		#region VerifyFixedFieldInfo
		private void VerifyFixedFieldInfo()
		{
			_fixedFieldVersion = _owner.FixedFieldVersion;

			int fixedRecordInViewCount = 0;

			if (_recordsInViewFields != null)
			{
				for (int i = 0, count = _recordsInViewFields.Count; i < count; i++)
				{
					if (_recordsInViewFields[i].FixedLocation != FixedFieldLocation.Scrollable)
						fixedRecordInViewCount++;
				}
			}

			_fixedRecordInViewFieldCount = fixedRecordInViewCount;
		}
		#endregion //VerifyFixedFieldInfo

		#endregion //Private

		#region Internal

		#region GetAutoSizeFieldLayouts
		/// <summary>
		/// Returns a list of field layouts that have auto sized fields scoped to allrecords/viewable or contain a descendant fieldlayout that does.
		/// </summary>
		/// <param name="dp">The owning data presenter</param>
		/// <returns>A dictionary of field layouts.</returns>
		internal static HashSet GetAutoSizeFieldLayouts(DataPresenterBase dp)
		{
			Debug.Assert(null != dp);

			Dictionary<FieldLayout, List<FieldLayout>> ancestorMap = new Dictionary<FieldLayout, List<FieldLayout>>();

			// it is possible that we don't have any records created but still need to auto size
			// the labels. this should only happen for the root collection
			FieldLayout rootFieldLayout = dp.RecordManager.FieldLayout;

			Predicate<FieldLayout> predicate = delegate(FieldLayout fl)
			{
				// make sure its auto size information is updated
				fl.AutoSizeInfo.Verify();

				return fl.AutoSizeInfo._hasOutOfViewAutoSizeFields;
			};

			// as we traverse we'll build a list of the field layouts that we know have 
			// autosized fields scoped to either viewable/all records
			HashSet fieldLayoutsWithAutoSizing = GetFieldLayoutsInUse(dp, new GridUtilities.MeetsCriteria_Predicate<FieldLayout>(predicate));

			// get a list of all the field layouts actually in use
			HashSet allFieldLayouts = GetFieldLayoutsInUse(dp, null);

			HashSet fieldLayouts = new HashSet();

			// walk over the field layouts that have auto sizing
			foreach (FieldLayout autoSizeFieldLayout in fieldLayoutsWithAutoSizing)
			{
				// skip any we've added along the way
				if (fieldLayouts.Exists(autoSizeFieldLayout))
					continue;

				// we haven't encountered this field layout so walk up the 
				// ancestor chain adding in any that are actually in use
				FieldLayout parent = autoSizeFieldLayout.ParentFieldLayout;

				while (null != parent)
				{
					// if the parent isn't valid then bail out
					if (!allFieldLayouts.Exists(parent))
						break;

					// if we hit one we've processed exit since this could be a recursive structure
					if (fieldLayouts.Exists(parent))
						break;

					fieldLayouts.Add(parent);
					parent = parent.ParentFieldLayout;
				}
			}

			return fieldLayouts;
		} 
		#endregion //GetAutoSizeFieldLayouts

		#region GetFieldLayoutsInUse
		/// <summary>
		/// Returns a collection of field layouts that have at least 1 autosized field whose scope is all/viewable records.
		/// </summary>
		/// <param name="dp">The owning data presenter</param>
		/// <param name="criteria">Filter criteria</param>
		/// <returns></returns>
		internal static HashSet GetFieldLayoutsInUse(DataPresenterBase dp, GridUtilities.IMeetsCriteria criteria)
		{
			// it is possible that we don't have any records created but still need to auto size
			// the labels. this should only happen for the root collection
			FieldLayout rootFieldLayout = dp.RecordManager.FieldLayout;

			// as we traverse we'll build a list of the field layouts that we know have 
			// autosized fields scoped to either viewable/all records
			HashSet fieldLayouts = new HashSet();

			// first build a list of field layouts with all/viewable record autosizing
			foreach (FieldLayout fl in dp.FieldLayouts)
			{
				// if this field layout isn't used then ignore it
				if (!fl.IsInitialRecordLoaded && fl != rootFieldLayout)
					continue;

				if (null != criteria && !criteria.MeetsCriteria(fl))
					continue;

				fieldLayouts.Add(fl);
			}

			return fieldLayouts;
		}
		#endregion //GetFieldLayoutsInUse

		#region IsInAutoSizeMode
		internal bool IsInAutoSizeMode(Field field, bool isLabel)
		{
			Debug.Assert(null != field && _owner == field.Owner);

			bool width = !_owner.IsHorizontal;

			return IsInAutoSizeMode(field, isLabel, width);
		}

		// AS 11/16/11 TFS95167
		// Changed to internal.
		//
		internal static bool IsInAutoSizeMode(Field field, bool isLabel, bool width)
		{
			if (!field.GetWidthOrHeight(width).IsAuto)
				return false;

			if (field.GetResizeSize(isLabel, width).HasExplicitSize)
				return false;

			return true;
		}
		#endregion //IsInAutoSizeMode

		#region OnAutoSizeSettingsChanged
		internal void OnAutoSizeSettingsChanged()
		{
			// if its not already dirty...
			if (_layoutManagerVersion == _owner.LayoutManagerVersion)
			{
				_layoutManagerVersion--;
				_lastTemplateVersion--;

				// just register one operation. if it turns out we need more than
				this.AddPendingOperation(OperationType.Initial);
			}
		} 
		#endregion //OnAutoSizeSettingsChanged

		#region OnCellPanelChange
		internal void OnCellPanelChange(VirtualizingDataRecordCellPanel cellPanel)
		{
			// when the field layout is first used process any pending fields
			if (_pendingInitialSizeFields.Count > 0)
				this.AddPendingOperation(OperationType.Initial);

			// since cells are being brought into view we also need to recalculate the 
			// records in view
			// AS 3/14/11 TFS67970 - Optimization
			// In a horizontal scroll we don't need to verify the records in view if we 
			// only have fixed fields in view since those would have already been 
			// measured.
			//
			//if (this._recordsInViewFields.Count > 0)
			if (this._recordsInViewFields.Count > _fixedRecordInViewFieldCount)
				this.AddPendingOperation(OperationType.RecordsInView);
		}
		#endregion //OnCellPanelChange

		#region OnDataChanged
		internal void OnDataChanged(DataChangeType dataChangeType, DataRecord recordContext, Field fieldContext, RecordManager rm)
		{
			Debug.Assert(rm != null);

			switch (dataChangeType)
			{
				case DataChangeType.ItemMoved:		// doesn't affect autosizing
				case DataChangeType.RemoveRecord:	// doesn't affect since we're grow only
					return;
				case DataChangeType.Reset:
					// assume the data cells in the record manager are dirty
					rm.FieldAutoSizeVersion++;

					// if there are autosized fields with a deeper nesting depth than this 
					// record manager then we need to walk the tree (wait til later to find out 
					// if we really can contain such a field layout)
					if (this.DPMaxAutoSizeAllRecordsDepth > rm.NestingDepth)
					{
						this.AddPendingOperation(OperationType.TraverseAllRecords, rm);
					}

					// if we have any out of view fields then we need to measure this rm's records
					if (_hasOutOfViewAutoSizeFields)
					{
						this.AddPendingOperation(OperationType.OutOfViewRecords, rm);
					}

					break;
				case DataChangeType.AddRecord:
					// if there are autosized fields with a deeper nesting depth than this 
					// record manager then we need to walk the tree (wait til later to find out 
					// if we really can contain such a field layout)
					if (this.DPMaxAutoSizeAllRecordsDepth > rm.NestingDepth)
					{
						this.AddPendingOperation(OperationType.TraverseAllRecords, recordContext);
					}

					// if there are autosized fields that act on viewable/all records then we need 
					// to incorporate the measurement of the cells in the new record in the calculation
					if (_hasOutOfViewAutoSizeFields)
						this.OnRecordChanged(recordContext);
					break;
				case DataChangeType.Unknown:
					Debug.Fail("How should this action affect autosizing?");
					break;
				case DataChangeType.CellDataChange:
				case DataChangeType.RecordDataChange:
					#region Expandable Field Value Change
					// if this is an expandable field that would have child records (i.e. its not displayed 
					// but could have child field layouts that are displayed)...
					if (fieldContext != null &&
						fieldContext.IsExpandableByDefault &&
						// note rather than checking IsExpandable which will unbox the dp, i'm using
						// the resolved property
						fieldContext.IsExpandableResolved)
					{
						Debug.Assert(null != recordContext);

						// ignore the field if is collapsed since the child records would not be 
						// shown in any island
						if (null != recordContext && fieldContext.VisibilityResolved != Visibility.Collapsed)
						{
							this.AddPendingOperation(OperationType.TraverseAllRecords, new CellKey(recordContext, fieldContext));
						}

						return;
					} 
					#endregion //Expandable Field Value Change

					// if we don't have any autosized fields then don't process this message. note if the 
					// list is dirty then this should get fixed up and an operation started
					if (_autoSizeCount == 0)
						return;

					bool isHorz = _owner.IsHorizontal;

					// we can ignore field changes in non-autosized fields (that are not expandable)
					if (null != fieldContext && !fieldContext.GetWidthOrHeight(!isHorz).IsAuto)
						return;

					if (null != recordContext)
					{
						this.OnDataChanged(recordContext, fieldContext);
					}
					break;
			}
		}

		private void OnDataChanged(DataRecord recordContext, Field fieldContext)
		{
			Debug.Assert(null != recordContext);

			// if we have processed some records in view and we have considered this 
			// record/cell as measured then we need to remove it from the cache
			if (_recordsInView != null && _recordsInView.Remove(recordContext, fieldContext))
				this.AddPendingOperation(OperationType.RecordsInView);

			// if this was a change for a specific field then mark that cell dirty
			CellCollection cells = recordContext.Cells;

			if (null != fieldContext) // mark the cell dirty
				cells[fieldContext].DirtyFieldAutoSizeVersion();
			else
			{
				cells.DirtyFieldAutoSizeVersion();
			}

			this.OnRecordChanged(recordContext);
		}
		#endregion //OnDataChanged

		#region OnEditCellValueChanged
		internal void OnEditCellValueChanged(CellValuePresenter cvp, Field.FieldResizeInfo preEditAutoSize)
		{
			// if the field isn't in auto mode (or more specifically we didn't have any auto size information 
			// before the edit mode) then there is nothing to do
			if (preEditAutoSize == null)
				return;

			Debug.Assert(cvp.Field == preEditAutoSize.Field);

			Field f = cvp.Field;
			FieldLayout fl = f.Owner;

			if (!f.GetWidthOrHeight(!fl.IsHorizontal).IsAuto)
				return;

			FieldAutoSizeOptions option = AutoSizeFieldHelper.GetCellOption(cvp.Record, cvp.Field);

			if (0 == (f.GetCurrentAutoSizeOptions() & option))
				return;

			EditModeChangeInfo changeInfo = new EditModeChangeInfo();
			changeInfo.CVP = cvp;
			changeInfo.PreEditAutoSize = preEditAutoSize;
			this.AddPendingOperation(OperationType.EditModeChange, changeInfo);
		}
		#endregion //OnEditCellValueChanged

		// AS 6/27/11 TFS79783
		#region OnFieldElementDesiredSizeChanged
		internal void OnFieldElementDesiredSizeChanged(Record record, Field field)
		{
			if (null != _recordsInView && _recordsInView.Remove(record, field))
			{
				this.AddPendingOperation(OperationType.RecordsInView);
			}
		}
		#endregion //OnFieldElementDesiredSizeChanged

		#region OnFilterCellChanged
		internal void OnFilterCellChanged(FilterCell cell)
		{
			if (_autoSizeCount == 0)
				return;

			DataRecord record = cell.Record;

			// make sure the cell is marked as dirty
			cell.DirtyFieldAutoSizeVersion();

			if (null != _recordsInView && _recordsInView.Remove(record, cell.Field))
			{
				this.AddPendingOperation(OperationType.RecordsInView);
			}

			this.OnRecordChanged(record);
		} 
		#endregion //OnFilterCellChanged

		#region OnLabelChanged
		internal void OnLabelChanged(Field f)
		{
			if (_autoSizeCount == 0)
				return;

			FieldAutoSizeScope scope;

			if (_scopes.TryGetValue(f, out scope))
			{
				this.AddPendingOperation(OperationType.LabelsChanged, f);
			}
		} 
		#endregion //OnLabelChanged

		#region OnMaxDepthChanged
		internal void OnMaxDepthChanged()
		{
			// if we have some fields that autosize based on all records then we may need 
			// to update the max level maintained on the datapresenter
			if (_hasAllRecordFields)
			{
				DataPresenterBase dp = _owner.DataPresenter;

				if (null != dp && dp.AllRecordsAutoSizeMaxDepth < _owner.MaxRecordManagerDepth)
					dp.ResetAllRecordsAutoSizeMaxDepth();
			}

			// when we find a field layout deeper than previously encountered
			// we need to measure down to that level
			if (_hasOutOfViewAutoSizeFields)
				this.AddPendingOperation(OperationType.OutOfViewRecords);
		}
		#endregion //OnMaxDepthChanged

		#region OnRecordExpanded
		internal void OnRecordExpanded(Record record)
		{
			if (record.IsExpanded)
				this.OnViewableRecordChanged(record);
		}
		#endregion //OnRecordExpanded

		// AS 2/26/10 TFS28159
		#region OnRecordsInViewChanged
		internal void OnRecordsInViewChanged()
		{
			if (this._hasRecordsInViewFields)
				this.AddPendingOperation(OperationType.RecordsInView);
		}
		#endregion //OnRecordsInViewChanged

		#region OnRecordVisibilityChanged
		internal void OnRecordVisibilityChanged(Record record)
		{
			// if the record is being made visible
			if (record.GetVisibilityResolved(false) != Visibility.Collapsed)
				this.OnViewableRecordChanged(record);
		}
		#endregion //OnRecordVisibilityChanged

		#region OnSummaryChanged
		internal void OnSummaryChanged(RecordCollectionBase records, SummaryResult summaryResult)
		{
			if (_autoSizeCount == 0)
				return;

			// consider the result dirty in any case
			summaryResult.FieldAutoSizeVersion--;

			if (null != _recordsInView && _recordsInView.Remove(RecordType.SummaryRecord))
			{
				this.AddPendingOperation(OperationType.RecordsInView);
			}

			// if we have autosizing being performed for records that may not be in view...
			if (_hasOutOfViewAutoSizeFields)
			{
				Field positionField = summaryResult.PositionFieldResolved;

				// if the position field is autosized...
				if (null != positionField && IsInAutoSizeMode(positionField, false))
				{
					FieldAutoSizeScope fieldScope;

					if (_scopes.TryGetValue(positionField, out fieldScope))
					{
						if (fieldScope != FieldAutoSizeScope.RecordsInView)
						{
							if (0 != (positionField.AutoSizeOptionsResolved & FieldAutoSizeOptions.Summaries))
							{
								this.AddPendingOperation(OperationType.OutOfViewRecords, summaryResult.ParentCollection);
							}
						}
					}
				}
			}
		}
		#endregion //OnSummaryChanged

		#region ProcessPendingOperations

		internal static void ProcessPendingOperations(List<FieldLayout> pendingAutoSize, RecordsInViewHelper recordsInViewHelper )
		{
			
			
			
			foreach (FieldLayout fl in pendingAutoSize)
				fl.AutoSizeInfo.ProcessPendingOperationsImpl(recordsInViewHelper );
		}

		private void ProcessPendingOperationsImpl(RecordsInViewHelper recordsInViewHelper )
		{
			// make sure that the lists have been calculated
			this.Verify();

			AutoSizeFieldOperation[] operations = _pendingOperations;
			_pendingOperations = null;
			_pendingOperationTypes = 0;

			foreach (AutoSizeFieldOperation operation in operations)
			{
				if (operation == null)
					continue;

				Debug.WriteLineIf(DebugAutoSize, string.Format("Processing - Op:{0} FL:{1}", operation.OperationType, _owner), "AutoSize - Processing:" + DateTime.Now.ToString("hh:mm:ss:ffffff"));

				switch (operation.OperationType)
				{
					case OperationType.Initial:
						this.InitializeAutoSizedFieldsImpl(recordsInViewHelper );
						break;
					case OperationType.RecordsInView:
						this.PerformAutoSize(_owner.IsHorizontal, _recordsInViewFields, null, 0, null, true, OperationType.RecordsInView, FieldAutoSizeOptions.All, recordsInViewHelper );
						break;
					case OperationType.EditModeChange:
					{
						this.ProcessEditCellValueChanged(operation.Parameters);
						break;
					}
					case OperationType.OutOfViewRecords:
					this.PerformAutoSize(_owner.IsHorizontal, new GridUtilities.AggregateEnumerable<Field>(_allRecordFields, _viewableRecordFields), null, _owner.MaxRecordManagerDepth, operation.Parameters, true, OperationType.OutOfViewRecords, FieldAutoSizeOptions.All, null );
						break;
					case OperationType.ViewableRecordsChanged:
						this.PerformAutoSize(_owner.IsHorizontal, _viewableRecordFields, null, _owner.MaxRecordManagerDepth, operation.Parameters, true, OperationType.ViewableRecordsChanged, FieldAutoSizeOptions.All, null );
						break;
					case OperationType.LabelsChanged:
					{
						IEnumerable<Field> fields;

						// if we have a list of fields that have change then just recalculate those labels
						if (operation.Parameters != null)
							fields = new TypedEnumerable<Field>(operation.Parameters);
						else // otherwise verify all of them
							fields = new GridUtilities.AggregateEnumerable<Field>(_recordsInViewFields, _allRecordFields, _viewableRecordFields);

						// just recalculate the labels
						this.PerformAutoSize(_owner.IsHorizontal, fields, null, 0, null, true, OperationType.LabelsChanged, FieldAutoSizeOptions.Label, null );
						break;
					}
					default:
						Debug.Fail("Unrecognized operation type:" + operation.OperationType.ToString());
						break;
				}
			}
		}

		#endregion //ProcessPendingOperations

		#region TransferViewableRecords
		internal static void TransferViewableRecords(List<FieldLayout> autoSizePendingList)
		{
			if (!GridUtilities.HasItems(autoSizePendingList))
				return;

			const int Index = (int)OperationType.ViewableRecordsChanged;
			FieldLayout[] fieldLayouts = autoSizePendingList.ToArray();

			foreach (FieldLayout fl in fieldLayouts)
			{
				AutoSizeFieldLayoutInfo autoSizeInfo = fl.AutoSizeInfo;

				if (autoSizeInfo.HasPendingOperation(OperationType.ViewableRecordsChanged))
				{
					AutoSizeFieldOperation operation = autoSizeInfo._pendingOperations[Index];

					if (null != operation.Parameters)
						autoSizeInfo.TransferViewableRecords(operation.Parameters);
				}
			}
		}
		#endregion //TransferViewableRecords

		#region Verify
		internal void Verify()
		{
			if (_layoutManagerVersion == _owner.LayoutManagerVersion)
			{
				// AS 3/14/11 TFS67970 - Optimization
				if (_fixedFieldVersion != _owner.FixedFieldVersion)
					this.VerifyFixedFieldInfo();

				return;
			}

			this.VerifyImpl();
		}

		private void VerifyImpl()
		{
			_layoutManagerVersion = _owner.LayoutManagerVersion;

			Dictionary<Field, AutoSizeFieldSettings> fieldsToUpdate = new Dictionary<Field, AutoSizeFieldSettings>();
			_pendingInitialSizeFields.Clear();
			_autoSizeCount = 0;

			bool templateChanged = _lastTemplateVersion != _owner.TemplateVersion;

			if (templateChanged)
			{
				_lastTemplateVersion = _owner.TemplateVersion;

				// if there was a large change and the templates were regenerated then we should
				// clear the records in view cache. otherwise we only need to remove entries 
				// when a field is removed
				_recordsInView = null;

				// if the templates have changed then dirty all the fields
				foreach (Field f in _owner.Fields)
					f.AutoSizeVersion++;
			}

			// AS 3/22/10 TFS29701
			// If fields that were autosized are removed then we need to make sure 
			// we pull them out of the scoped collections. Previously we were only 
			// removing fields when the autosize state changed but the fields are 
			// no longer in the fieldlayout's fields collection.
			//
			int ownerFieldVersion = _owner.Fields.Version;
			bool fieldsChanged = _lastFieldsVersion != _owner.Fields.Version;

			if (fieldsChanged)
			{
				// if there was a large change and the templates were regenerated then we should
				// clear the records in view cache. otherwise we only need to remove entries 
				// when a field is removed
				_lastFieldsVersion = ownerFieldVersion;
				_recordsInView = null;

				Field[] currentScopeFields = new Field[_scopes.Count];
				_scopes.Keys.CopyTo(currentScopeFields, 0);

				foreach (Field f in currentScopeFields)
				{
					if (!IsValid(f))
						OnRemoveField(f);
				}
			}

			bool isHorz = _owner.IsHorizontal;

			foreach (Field field in _owner.Fields)
			{
				// if the field is not visible then we don't need to spend the cycles calculating it...
				if (field.VisibilityResolved == Visibility.Collapsed)
				{
					OnRemoveField(field);
					continue;
				}

				if (field._hasPendingInitialAutoSize)
					_pendingInitialSizeFields.Add(field);

				// see when it wants to be auto sized and via what scope
				FieldAutoSizeScope? currentScope = GetAutoSizeScope(isHorz, field, null);

				// if it isn't in autosize mode then just make sure we're not calculating it
				if (currentScope == null)
				{
					OnRemoveField(field);
					continue;
				}

				FieldAutoSizeScope newScope = currentScope.Value;
				FieldAutoSizeScope scope;

				_autoSizeCount++;

				// if we have it in any list...
				if (_scopes.TryGetValue(field, out scope))
				{
					// its already in the right list so move on
					if (newScope == scope)
						continue;

					// remove it from the old list and add it to the new
					OnRemoveField(field);
				}

				// since we may have ignored changes for this field if it wasn't autosizing
				// we need to dirty its version number so its cells can be measured
				field.AutoSizeVersion++;

				// otherwise we should store it in the appropriate list
				// and make sure we update its autosize info when we're done
				_scopes[field] = newScope;
				GetFieldList(newScope).Add(field);

				if (newScope != FieldAutoSizeScope.RecordsInView)
					fieldsToUpdate[field] = new AutoSizeFieldSettings(field);
			}

			_hasOutOfViewAutoSizeFields = _allRecordFields.Count > 0 || _viewableRecordFields.Count > 0;

			// keep track of whether we have any autosizing that traverses viewable records so we can 
			// short circuit any expansion/visibility processing
			this.HasViewableRecordFields = _viewableRecordFields.Count > 0;

			// AS 2/26/10 TFS28159
			this.HasRecordsInViewFields = _recordsInViewFields.Count > 0;

			this.HasAllRecordFields = _allRecordFields.Count > 0;

			// if there are any fields that need their initial resize then process those
			if (_pendingInitialSizeFields.Count > 0)
				this.AddPendingOperation(OperationType.Initial);

			// if there are any fields based on the records in view then start a pending size calculation for that
			if (_recordsInViewFields.Count > 0)
				this.AddPendingOperation(OperationType.RecordsInView);

			// if we have new fields that are all/viewable then recalc or if we've change the templates
			if ( (fieldsToUpdate.Count > 0 || templateChanged)
					&&
				_hasOutOfViewAutoSizeFields)
			{
				this.AddPendingOperation(OperationType.OutOfViewRecords);
			}

			// AS 3/14/11 TFS67970 - Optimization
			this.VerifyFixedFieldInfo();
		}
		#endregion //Verify

		#region WalkAllRecordItems
		/// <summary>
		/// Helper method to traverse records down to the maximum depth to ensure their recordmanagers log a pending operation.
		/// </summary>
		/// <param name="autoSizePendingList"></param>
		internal static void WalkAllRecordItems(List<FieldLayout> autoSizePendingList)
		{
			if (!GridUtilities.HasItems(autoSizePendingList))
				return;

			HashSet combinedSets = new HashSet();
			const int Index = (int)OperationType.TraverseAllRecords;
			DataPresenterBase dp = autoSizePendingList[0].DataPresenter;

			Debug.Assert(null != dp);

			if (dp == null)
				return;

			foreach (FieldLayout fl in autoSizePendingList)
			{
				AutoSizeFieldLayoutInfo autoSizeInfo = fl.AutoSizeInfo;

				if (autoSizeInfo.HasPendingOperation(OperationType.TraverseAllRecords))
				{
					AutoSizeFieldOperation operation = autoSizeInfo._pendingOperations[Index];

					if (operation.Parameters != null)
					{
						combinedSets.AddItems(operation.Parameters);
					}
					else
						autoSizeInfo.AddPendingOperation(OperationType.OutOfViewRecords);

					// consider that operation processed
					autoSizeInfo._pendingOperations[Index] = null;
					autoSizeInfo._pendingOperationTypes &= ~(1 << Index);
				}
			}

			if (combinedSets.Count == 0)
				return;

			// this list will have RecordManager, DataRecord and CellKey instances
			// for CellKey we just need to get the RecordManager of the associated
			//	expandable field record and add that to the list of recordmanagers.
			// for recordmanager's we should skip those we've already processed.
			// for records we should skip any that are within the rm's we've processed
			// we should sort the rm's by nesting depth and process the 0th first
			HashSet recordManagers = new HashSet();

			// get a hash set of field layouts that are either autosized or ancestors
			// of autosized field layouts
			HashSet fieldLayoutsToTraverse = GetAutoSizeFieldLayouts(dp);

			foreach (object item in combinedSets)
			{
				RecordManager rm = item as RecordManager;

				if (rm != null)
				{
					if (fieldLayoutsToTraverse.Exists(rm.FieldLayout))
						recordManagers.Add(rm);
				}
				else
				{
					DataRecord dr = item as DataRecord;

					if (null != dr)
					{
						if (dr.HasChildrenInternal)
						{
							// add in the record manager of the child records
							foreach (ExpandableFieldRecord efr in dr.ChildRecords)
							{
								if (fieldLayoutsToTraverse.Exists(efr.FieldLayout))
								{
									// JJD 09/22/11  - TFS84708 - Optimization
									// Use the ChildRecordManagerIfNeeded instead which won't create
									// child rcd managers for leaf records
									//recordManagers.Add(efr.ChildRecordManager);
									RecordManager crm = efr.ChildRecordManagerIfNeeded;

									if ( crm != null )
										recordManagers.Add(crm);
								}
							}
						}
					}
					else
					{
						Debug.Assert(item is CellKey);

						if (item is CellKey)
						{
							CellKey ck = (CellKey)item;
							int index = ck.Record.ChildRecords.IndexOf(ck.Field);

							Debug.Assert(index >= 0);

							if (index >= 0)
							{
								ExpandableFieldRecord efr = ck.Record.ChildRecords[index];

								if (fieldLayoutsToTraverse.Exists(efr.FieldLayout))
								{
									// JJD 09/22/11  - TFS84708 - Optimization
									// Use the ChildRecordManagerIfNeeded instead which won't create
									// child rcd managers for leaf records
									//recordManagers.Add(efr.ChildRecordManager);
									RecordManager crm = efr.ChildRecordManagerIfNeeded;

									if (crm != null)
										recordManagers.Add(crm);
								}
							}
						}
					}
				}
			}

			if (recordManagers.Count == 0)
				return;

			RecordManager[] allRecordManagers = recordManagers.ToArray<RecordManager>();

			Comparison<RecordManager> comparison = delegate(RecordManager rm1, RecordManager rm2)
			{
				return rm1.NestingDepth.CompareTo(rm2.NestingDepth);
			};

			Utilities.SortMergeGeneric<RecordManager>(allRecordManagers, Utilities.CreateComparer(comparison));
			int initialDepth = allRecordManagers[0].NestingDepth;
			int maxDepth = dp.AllRecordsAutoSizeMaxDepth;

			for (int i = 0; i < allRecordManagers.Length; i++)
			{
				RecordManager rm = allRecordManagers[i];

				int rmDepth = rm.NestingDepth;

				// we don't have to walk into the records of the deepest level
				if (rmDepth == maxDepth)
					continue;

				// if this is deeper than the topmost recordmanagers we've iterated
				// then see if we would have already processed it
				if (initialDepth != rmDepth)
				{
					bool wasProcessed = false;

					for (int j = rmDepth; j > initialDepth; j--)
					{
						if (recordManagers.Exists(rm.ParentDataRecord.RecordManager))
						{
							wasProcessed = true;
							break;
						}
					}

					// skip a recordmanager that we traversed
					if (wasProcessed)
						continue;
				}

				WalkAllRecordItems(rm, maxDepth, fieldLayoutsToTraverse);
			}
		}

		private static void WalkAllRecordItems(RecordManager rm, int maxRecordManagerDepth, HashSet fieldLayoutsToTraverse)
		{
			if (rm.NestingDepth == maxRecordManagerDepth)
				return;

			foreach (DataRecord record in rm.Unsorted)
			{
				if (record.HasChildrenInternal)
				{
					foreach (ExpandableFieldRecord efr in record.ChildRecords)
					{
						if (fieldLayoutsToTraverse.Exists(efr.FieldLayout))
						{
							// JJD 09/22/11  - TFS84708 - Optimization
							// Use the ChildRecordManagerIfNeeded instead which won't create
							// child rcd managers for leaf records
							//WalkAllRecordItems(efr.ChildRecordManager, maxRecordManagerDepth, fieldLayoutsToTraverse);
							RecordManager crm = efr.ChildRecordManagerIfNeeded;

							if (crm != null)
								WalkAllRecordItems(crm, maxRecordManagerDepth, fieldLayoutsToTraverse);
						}
					}
				}
			}
		}
		#endregion //WalkAllRecordItems

		#endregion //Internal

		#endregion //Methods

		#region OperationType enum
		internal enum OperationType
		{
			/// <summary>
			/// Any field that hasn't processing its initial autosize - this is primarily meant for InitialAuto
			/// </summary>
			Initial = 0,

			/// <summary>
			/// Measure the records in view for this field layout
			/// </summary>
			RecordsInView = 1,

			/// <summary>
			/// The value of a cell in edit mode has been changed and requires a measure
			/// </summary>
			EditModeChange = 2,

			/// <summary>
			/// SummaryResult, New AutoSize Field, modified record, recordmanager reset notification, max depth change
			/// </summary>
			OutOfViewRecords = 3,

			/// <summary>
			/// The state of a record that would require measuring viewablerecord scopes fields has been modified. 
			/// Note this change will cause the descendant field layouts to measure as well.
			/// </summary>
			ViewableRecordsChanged = 4,

			/// <summary>
			/// The label of a field has changed.
			/// </summary>
			LabelsChanged = 5,

			/// <summary>
			/// A record/recordmanager/expandablefield has been touched and may need to be measured.
			/// </summary>
			TraverseAllRecords = 6,
		}
		#endregion //OperationType enum

		#region AutoSizeFieldOperation class
		internal class AutoSizeFieldOperation
		{
			internal OperationType OperationType;
			internal HashSet Parameters;

			internal AutoSizeFieldOperation(OperationType operationType)
			{
				this.OperationType = operationType;
			}
		}
		#endregion //AutoSizeFieldOperation class

		#region EditModeChangeInfo struct
		internal struct EditModeChangeInfo
		{
			internal CellValuePresenter CVP;
			internal Field.FieldResizeInfo PreEditAutoSize;
		}
		#endregion //EditModeChangeInfo struct

		#region OutOfViewRecordEnumerable class
		private class OutOfViewRecordEnumerable : IEnumerable<Record>
		{
			#region Member Variables

			private HashSet _items;

			#endregion //Member Variables

			#region Constructor
			internal OutOfViewRecordEnumerable(HashSet items)
			{
				_items = items;
			} 
			#endregion //Constructor

			#region IEnumerable<Record> Members

			public IEnumerator<Record> GetEnumerator()
			{
				return new Enumerator(_items.GetEnumerator());
			}

			#endregion //IEnumerable<Record> Members

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion //IEnumerable Members

			#region Enumerator class
			private class Enumerator : IEnumerator<Record>
			{
				private System.Collections.IEnumerator _enumerator;
				private System.Collections.IEnumerator _currentEnumerator;
				private Record _currentRecord;

				internal Enumerator(System.Collections.IEnumerator enumerator)
				{
					_enumerator = enumerator;
					this.Reset();
				}

				#region IEnumerator<Record> Members

				public Record Current
				{
					get { return _currentRecord; }
				}

				#endregion //IEnumerator<Record> Members

				#region IDisposable Members

				public void Dispose()
				{
				}

				#endregion //IDisposable

				#region IEnumerator Members

				object System.Collections.IEnumerator.Current
				{
					get { return _currentRecord; }
				}

				public bool MoveNext()
				{
					// if we were iterating a recordmanager then see if it has any more to process
					if (_currentEnumerator != null)
					{
						if (_currentEnumerator.MoveNext())
						{
							_currentRecord = _currentEnumerator.Current as Record;
							return true;
						}

						_currentEnumerator = null;
					}

					while (_enumerator.MoveNext())
					{
						object item = _enumerator.Current;
						_currentRecord = item as Record;

						if (_currentRecord != null)
							return true;

						if (item is RecordManager)
						{
							RecordManager rm = item as RecordManager;

							//_currentEnumerator = ((DataRecordCollection)rm.Sorted).GetAllRecords().GetEnumerator();
							_currentEnumerator = rm.Unsorted.GetEnumerator();
						}
						else if (item is SummaryResultCollection)
						{
							SummaryResultCollection summaries = (SummaryResultCollection)item as SummaryResultCollection;
							RecordCollectionBase records = summaries.Records;

							if (records != null)
								_currentEnumerator = records.GetSpecialRecords().GetEnumerator();
						}
						else
						{
							Debug.Fail("Unrecognized object in enumerator");
							continue;
						}


						if (null != _currentEnumerator && _currentEnumerator.MoveNext())
						{
							_currentRecord = _currentEnumerator.Current as Record;
							return true;
						}

						continue;
					}

					_currentRecord = null;
					return false;
				}

				public void Reset()
				{
					_currentEnumerator = null;
					_currentRecord = null;
				}

				#endregion //IEnumerator Members
			}
			#endregion //Enumerator class
		} 
		#endregion //OutOfViewRecordEnumerable class
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