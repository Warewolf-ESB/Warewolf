using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter.Internal;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	// AS 6/16/09 NA 2009.2 Field Sizing
	internal class AutoSizeFieldHelper
	{
		#region Member Variables

		private FieldLayout _fieldLayout;
		private FieldAutoSizeOptions _options;
		private FieldInfo[] _fieldInfos;
		private Dictionary<FieldValue, double> _dataCellValues;
		private Dictionary<FieldAutoSizeScope, IList<FieldInfo>> _fieldInfosByScope;

		internal const FieldAutoSizeOptions RecordOptions = FieldAutoSizeOptions.All & ~FieldAutoSizeOptions.Label;

		#endregion //Member Variables

		#region Constructor
		internal AutoSizeFieldHelper(Dictionary<Field, AutoSizeFieldSettings> fields, FieldLayout fieldLayout)
		{
			GridUtilities.ValidateNotNull(fields);
			GridUtilities.ValidateNotNull(fieldLayout);

			_fieldLayout = fieldLayout;
			_fieldLayout.TemplateDataRecordCache.Verify();

			List<FieldInfo> fieldInfos = new List<FieldInfo>();
			_fieldInfosByScope = new Dictionary<FieldAutoSizeScope, IList<FieldInfo>>();

			foreach (KeyValuePair<Field, AutoSizeFieldSettings> pair in fields)
			{
				Field field = pair.Key;
				AutoSizeFieldSettings info = pair.Value;
				Debug.Assert(field.Owner == fieldLayout);

				// AS 3/22/10 TFS29701
				if (field.Index < 0)
					continue;

				FieldInfo fi = new FieldInfo(field);
				fi.Options = info.Options;

				if (field.TemplateCellIndex < 0)
					fi.Options = FieldAutoSizeOptions.None;

				fieldInfos.Add(fi);
				_options |= fi.Options;

				FieldAutoSizeScope scope = info.Scope;
				Debug.Assert(scope != FieldAutoSizeScope.Default);

				IList<FieldInfo> scopedFields;
				if (!_fieldInfosByScope.TryGetValue(scope, out scopedFields))
				{
					scopedFields = new List<FieldInfo>();
					_fieldInfosByScope[scope] = scopedFields;
				}

				scopedFields.Add(fi);
			}

			_fieldInfos = fieldInfos.ToArray();

			// as an optimization we will cache cell values for a given field when measuring 
			// all records using a template element
			_dataCellValues = new Dictionary<FieldValue, double>();
		}

		internal AutoSizeFieldHelper(IEnumerable<Field> fields, FieldLayout fieldLayout) 
			: this(fields, fieldLayout, FieldAutoSizeOptions.All)
		{
		}

		internal AutoSizeFieldHelper(IEnumerable<Field> fields, FieldLayout fieldLayout, FieldAutoSizeOptions allowedOptions)
			: this(GetFieldOptions(fields, allowedOptions), fieldLayout)
		{
		}

		internal AutoSizeFieldHelper(Field field, FieldAutoSizeOptions options, FieldAutoSizeScope scope)
			: this(GetFieldOption(field, options, scope), field.Owner)
		{
		}
		#endregion //Constructor

		#region Properties

		#region IsAutoSizeElement

		/// <summary>
		/// IsAutoSizeElement Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsAutoSizeElementProperty =
			DependencyProperty.RegisterAttached("IsAutoSizeElement", typeof(bool), typeof(AutoSizeFieldHelper),
				new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));

		internal static bool GetIsAutoSizeElement(DependencyObject d)
		{
			return (bool)d.GetValue(IsAutoSizeElementProperty);
		}

		#endregion //IsAutoSizeElement

		#endregion //Properties

		#region Methods

		#region AddProcessedField
		private static void AddProcessedField(Field field, ref FieldList newFieldList)
		{
			if (newFieldList == null)
			{
				newFieldList = new FieldList(field.Owner.Fields.Count);
			}

			newFieldList.Add(field);
		}
		#endregion //AddProcessedField

		#region Calculate
		public bool Calculate(RecordCollectionBase records, int recordManagerDepth, AutoSizeCalculationFlags flags, RecordsInViewHelper recordsInViewHelper )
		{
			if (_options == FieldAutoSizeOptions.None)
				return false;

			DataPresenterBase dp = _fieldLayout.DataPresenter;

			if (dp == null)
				return false;

			RecordEnumeratorBase outOfViewRecordEnumerator = null;

			if ((_options & RecordOptions) != 0)
				outOfViewRecordEnumerator = new RecordCollectionEnumerator(records ?? dp.Records, Math.Max(recordManagerDepth, 0));

			RecordFieldList recordFieldList = null;
			return Calculate(outOfViewRecordEnumerator, flags, ref recordFieldList, false, recordsInViewHelper );
		}

		public bool Calculate(IEnumerable<Record> outOfViewRecords, bool recursive, int recordManagerDepth, AutoSizeCalculationFlags flags)
		{
			Debug.Assert(outOfViewRecords is RecordCollectionBase == false, "Using wrong overload?");

			RecordFieldList recordFieldList = null;
			return Calculate(outOfViewRecords == null ? null : new RecordEnumerator(outOfViewRecords, recordManagerDepth, recursive), flags, ref recordFieldList, false, null );
		}

		public bool Calculate(AutoSizeCalculationFlags flags, ref RecordFieldList recordFieldList, bool trackRecords, RecordsInViewHelper recordsInViewHelper )
		{
			return Calculate(null, flags, ref recordFieldList, trackRecords, recordsInViewHelper );
		}

		private bool Calculate(RecordEnumeratorBase outOfViewRecords, AutoSizeCalculationFlags flags, ref RecordFieldList recordFieldList, bool trackRecords, RecordsInViewHelper recordsInViewHelper )
		{
			DataPresenterBase dp = _fieldLayout.DataPresenter;

			if (dp == null)
				return false;

			if (_options == FieldAutoSizeOptions.None)
				return false;

			bool isHorz = _fieldLayout.IsHorizontal;
			bool useCellPresenter = _fieldLayout.UseCellPresenters;
			TemplateDataRecordCache cache = _fieldLayout.TemplateDataRecordCache;

			cache.Verify();

			#region Label
			if (this.IncludesOption(FieldAutoSizeOptions.Label))
			{
				for (int i = 0; i < _fieldInfos.Length; i++)
				{
					FieldInfo info = _fieldInfos[i];

					if (!IncludesOption(FieldAutoSizeOptions.Label, info.Options))
						continue;

					LabelPresenter lp = GetLabelPresenter(info.Field, cache);

					if (lp == null)
						continue;

					// the label presenter may have been within the cellpresenter so make sure it gets measured
					lp.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

					Size desired = lp.DesiredSize;
					info.AddLabelExtent( isHorz ? desired.Height : desired.Width );
				}
			}
			#endregion //Label

			#region Records
			if (this.IncludesOption(RecordOptions))
			{
				TemplateDataRecord templateRecord = _fieldLayout.TemplateDataRecord as TemplateDataRecord;
				Debug.Assert(null != templateRecord);

				// the template record caches the CVP for its cells on the cells themselves. well if the 
				// template cache has changed then these may be out of date so this routine will clear 
				// those references if the version has changed
				templateRecord.VerifyCachedCellElements();

				bool useActualCellElements;
				bool useTemplateElements;
				bool viewableRecordsOnly;
				bool isOutOfViewRecords;
				bool skipEditModeCell = (flags & AutoSizeCalculationFlags.SkipEditCell) != 0;
				bool forceRecalc = (flags & AutoSizeCalculationFlags.RecalculateRecords) != 0;
				bool canUseTemplateRecordAsFallback = (flags & AutoSizeCalculationFlags.UseTemplateRecordAsFallback) != 0; // AS 8/10/11 TFS83904
				Cell activeCell = skipEditModeCell ? dp.ActiveCell : null;
				Record editModeRecord = activeCell != null && activeCell.IsInEditMode ? activeCell.Record : null;
				Field editModeField = editModeRecord != null ? activeCell.Field : null;

				RecordFieldList newInView = null;
				RecordFieldList oldInView = recordFieldList;

				if (trackRecords)
				{
					newInView = new RecordFieldList();
					recordFieldList = newInView;
				}

				SummaryResultsPresenter summaryResultsElement = null;
				Dictionary<Field, FilterCellValuePresenter> filterCellElements = new Dictionary<Field, FilterCellValuePresenter>();
				// AS 6/12/12 TFS113591
				var usedCellPanels = 

					new HashSet<VirtualizingDataRecordCellPanel>();  




				bool includeSummaries = this.IncludesOption(FieldAutoSizeOptions.Summaries);

				foreach (KeyValuePair<FieldAutoSizeScope, IList<FieldInfo>> pair in _fieldInfosByScope)
				{
					IEnumerator<Record> enumerator;
					switch (pair.Key)
					{
						default:
						case FieldAutoSizeScope.RecordsInView:
							Debug.Assert(pair.Key == FieldAutoSizeScope.RecordsInView);
							// AS 3/2/11 66934 - AutoSize
							// Use a helper so we only get it once for a given layoutupdated call.
							//
							//Record[] recordsInView = dp.GetRecordsInView(true, true /* AS 2/26/10 TFS28159 */);
							Record[] recordsInView = recordsInViewHelper != null ? recordsInViewHelper.GetRecords() : dp.GetRecordsInView(true, true);
							enumerator = ((IList<Record>)recordsInView).GetEnumerator();
							useActualCellElements = true;
							useTemplateElements = canUseTemplateRecordAsFallback; // AS 8/10/11 TFS83904 // false;
							viewableRecordsOnly = true;
							isOutOfViewRecords = false;
							break;
						case FieldAutoSizeScope.AllRecords:
						case FieldAutoSizeScope.ViewableRecords:
							Debug.Assert(!trackRecords, "The record tracking is really not meant for large scale tracking as occurs when processing all records");

							useActualCellElements = false;
							useTemplateElements = true;
							isOutOfViewRecords = true;
							viewableRecordsOnly = pair.Key == FieldAutoSizeScope.ViewableRecords;
							enumerator = outOfViewRecords;

							// if we weren't given an enumerable
							Debug.Assert(enumerator != null);

							if (enumerator == null)
								continue;

							outOfViewRecords.SetViewableOnly(viewableRecordsOnly);
							outOfViewRecords.Reset();
							break;
					}
					
					// if we are forcing a recalc then we not only need to skip any version checks
					// but we need to bump the field autosize version. otherwise when a record that 
					// was measured is expanded after a forced recalc, will assume its values don't 
					// need to be considered
					if (forceRecalc)
					{
						foreach(FieldInfo fi in pair.Value)
						{
							fi.Field.AutoSizeVersion++;
						}
					}

					int rmAutoSizeVersion = 0;

					while (enumerator.MoveNext())
					{
						Record record = enumerator.Current;

						Debug.Assert(record is HeaderRecord == false);

						if (record.FieldLayout != _fieldLayout)
							continue;

						GroupByRecord groupByRecord = record as GroupByRecord;

						// we can short circuit groupby records that have no summaries or 
						// if we don't consider summaries at all
						if (null != groupByRecord && (!includeSummaries || !GroupByRecord.ShouldDisplaySummaries(groupByRecord)))
							continue;


						// JJD 10/26/11 - TFS91364 
						// Ignore HeaderRecords
						if (record is HeaderRecord)
							continue;

						// AS 3/2/11 66934 - AutoSize
						// If we are processing the records in view, we can skip the record if there 
						// are no fields in view for the record manager.
						//
						if (useActualCellElements &&
							null != recordsInViewHelper &&
							null == oldInView &&
							record is DataRecord &&
							!recordsInViewHelper.ShouldProcessInViewRecord(record, pair.Value))
						{
							continue;
						}


						SummaryRecord summaryRecord = record as SummaryRecord;
						VirtualizingDataRecordCellPanel cellPanel = null;
						DataRecord dataRecord = record as DataRecord;

						FieldList oldFieldList = oldInView != null ? oldInView[record] : null;

						// we'll build on top of the old list if we get any new ones to process
						// so we only need a new list when we first encounter a record
						FieldList newFieldList = oldFieldList;

						if (isOutOfViewRecords)
							rmAutoSizeVersion = outOfViewRecords.RecordManagerVersion;

						for (int i = 0, count = pair.Value.Count; i < count; i++)
						{
							FieldInfo info = pair.Value[i];

							#region Verify RecordType is to be processed

							if (record is DataRecord)
							{
								#region DataRecord
								Debug.Assert(record is TemplateDataRecord == false);

								if (record is FilterRecord)
								{
									if (!IncludesOption(FieldAutoSizeOptions.FilterCells, info.Options))
										continue;
								}
								else
								{
									if (!IncludesOption(FieldAutoSizeOptions.DataCells, info.Options))
										continue;
								}
								#endregion //DataRecord
							}
							else if (record is ExpandableFieldRecord)
								continue;
							else // summary
							{
								#region Summaries
								if (summaryRecord != null)
								{
									if (!IncludesOption(FieldAutoSizeOptions.Summaries, info.Options))
										continue;
								}
								else if (groupByRecord != null)
								{
									if (!IncludesOption(FieldAutoSizeOptions.Summaries, info.Options))
										continue;
								}
								else
								{
									Debug.Fail("Unexpected record type");
									continue;
								}
								#endregion //Summaries
							}
							#endregion //Verify RecordType is to be processed

							Field field = info.Field;

							// if we've processed a field based on the provided list we can skip it
							if (!forceRecalc && null != oldFieldList && oldFieldList.Contains(field))
								continue;

							// the cell in edit mode will be handled separately
							if (record == editModeRecord && field == editModeField)
								continue;

							if (useActualCellElements)
							{
								#region Use VirtualizingDataRecordCellPanel elements

								// this is used to measure the cell using the actual "cell" elements
								if (null == cellPanel)
								{
									// AS 4/12/11 TFS62951
									// Moved the impl to the GridUtilities.
									//
									//cellPanel = GetCellPanel(RecordPresenter.FromRecord(record));
									cellPanel = GridUtilities.GetCellPanel(RecordPresenter.FromRecord(record), false);

									// AS 3/2/11 66934 - AutoSize
									// Instead of verifying with each call to GetCellElement, just 
									// verify once when we get the panel.
									//
									if (null != cellPanel)
										cellPanel.VerifyCellElements();
								}

								if (null != cellPanel)
								{
									// AS 3/2/11 66934 - AutoSize
									FrameworkElement fieldElement = cellPanel.GetCellElement(field, false);
									//Debug.Assert(null != fieldElement || !(record is DataRecord));

									if (null != fieldElement)
									{
										fieldElement = GetCellElement(fieldElement, false);

										if (null != fieldElement)
										{
											// AS 6/27/11 TFS79783
											// Set a flag while measuring so the cellpanel can skip some logic if needed.
											//
											//fieldElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
											bool oldIsMeasuring = cellPanel.IsPerformingAutoSizeMeasure;
											cellPanel.IsPerformingAutoSizeMeasure = true;

											try
											{
												fieldElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
											}
											finally
											{
												cellPanel.IsPerformingAutoSizeMeasure = oldIsMeasuring;
											}

											usedCellPanels.Add(cellPanel); // AS 6/12/12 TFS113591

											Size desired = fieldElement.DesiredSize;
											info.AddCellExtent(isHorz ? desired.Height : desired.Width);

											if (trackRecords)
												AddProcessedField(field, ref newFieldList);
											continue;
										}
									}
								}
								#endregion //Use VirtualizingDataRecordCellPanel elements
							}

							if (useTemplateElements)
							{
								#region Template Elements

								//Debug.Assert(isOutOfViewRecords, "If this is used for records in view then we probably shouldn't be looking at/adjusting the version");

								// optimization: skip cells that have been already calculated
								int fieldVersion = field.AutoSizeVersion;

								if (null != dataRecord)
								{
									// AS 8/10/11 TFS83904
									// Added if block so we only update the version when this is not 
									// using the template elements as a fallback.
									//
									if (isOutOfViewRecords)
									{
										// if this is a datarecord then also add in the recordmanager
										// version. this would get bumped when the rm was reset and 
										// therefore only affects datarecords
										if (dataRecord is FilterRecord == false)
											fieldVersion += rmAutoSizeVersion;

										Cell cell = dataRecord.Cells[field];

										if (!forceRecalc && cell.FieldAutoSizeVersion == fieldVersion)
											continue;

										cell.FieldAutoSizeVersion = fieldVersion;
									}

									if (dataRecord is FilterRecord)
									{
										#region FilterRecord

										FilterCellValuePresenter filterElement;

										if (!filterCellElements.TryGetValue(field, out filterElement))
										{
											filterElement = new FilterCellValuePresenter();
											filterElement.SetValue(IsAutoSizeElementProperty, KnownBoxes.TrueBox);
											filterCellElements.Add(field, filterElement);
											dp.InternalAddLogicalChild(filterElement);

											VirtualizingDataRecordCellPanel.InitializeCellElement(filterElement, field, record, false);
										}

										// give the cell the context of the record
										filterElement.DataContext = record;
										filterElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
										filterElement.UpdateLayout();

										Size desired = filterElement.DesiredSize;
										double extent = isHorz ? desired.Height : desired.Width;
										info.AddCellExtent(extent);

										if (trackRecords)
											AddProcessedField(field, ref newFieldList);

										continue;

										#endregion //FilterRecord
									}
									else if (dataRecord != null)
									{
										#region DataRecord

										// AS 11/23/09 TFS24996
										//object newValue = ((DataRecord)record).GetCellValue(field, false);
										object newValue = dataRecord.GetCellValue(field, true);

										FieldValue fv = new FieldValue(field, newValue);

										if (_dataCellValues.ContainsKey(fv))
										{
											if (trackRecords)
												AddProcessedField(field, ref newFieldList);

											continue;
										}

										templateRecord.SetValue(field, newValue);

										CellValuePresenter cp = GetCellPresenter(field, cache);

										if (cp != null)
										{

											Size desired = cp.DesiredSize;
											double extent = isHorz ? desired.Height : desired.Width;
											info.AddCellExtent(extent);

											_dataCellValues.Add(fv, extent);

											if (trackRecords)
												AddProcessedField(field, ref newFieldList);

											continue;
										}

										#endregion //DataRecord
									}
								}
								else if (groupByRecord != null)
								{
									// we need a summary record that would be used for the summaries. groupby 
									// record does this too - or more accurate the GroupBySummariesPresenter
									// does this to display the summary cells
									if (summaryRecord == null)
									{
										// AS 2/9/11 NA 2011.1 Word Writer
										//summaryRecord = new SummaryRecord(record.FieldLayout, record.ChildRecordsInternal,
										//	SummaryDisplayAreaContext.InGroupByRecordsSummariesContext);
										summaryRecord = groupByRecord.CreateSummaryRecord();
									}
								}

								if (summaryRecord != null)
								{
									#region Summaries

									IEnumerable<SummaryResult> results = summaryRecord.GetFixedSummaryResults(field, false);

									if (trackRecords)
										AddProcessedField(field, ref newFieldList);

									if (!forceRecalc)
									{
										// optimization skip summaries that have not been changed
										bool isDirty = false;

										foreach (SummaryResult result in results)
										{
											if (result.FieldAutoSizeVersion != fieldVersion)
											{
												isDirty = true;
												result.FieldAutoSizeVersion = fieldVersion;
											}
										}

										if (!isDirty)
											continue;
									}

									if (!GridUtilities.HasItems(results))
										continue;

									if (summaryResultsElement == null)
									{
										summaryResultsElement = new SummaryResultsPresenter();
										summaryResultsElement.SetValue(IsAutoSizeElementProperty, KnownBoxes.TrueBox);
										dp.InternalAddLogicalChild(summaryResultsElement);
									}

									summaryResultsElement.SummaryResults = results;
									summaryResultsElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
									summaryResultsElement.UpdateLayout();

									Size desired = summaryResultsElement.DesiredSize;
									info.AddCellExtent(isHorz ? desired.Height : desired.Width);
									continue;

									#endregion //Summaries
								}
								#endregion //Template Elements
							}
						}

						if (newFieldList != null)
							newInView[record] = newFieldList;
					}
				}

				#region Clean Up

				// remove any temporary elements we created during the process
				if (null != summaryResultsElement)
					dp.InternalRemoveLogicalChild(summaryResultsElement);

				foreach (FilterCellValuePresenter element in filterCellElements.Values)
				{
					// clear the context so the element won't have a cell reference and 
					// won't process notifications while its waiting to be released
					element.DataContext = null;

					dp.InternalRemoveLogicalChild(element);
				}

				// reset the template record values
				templateRecord.ClearAll();

				// AS 6/12/12 TFS113591
				// We need to invalidate the measure of any panel we used because we have measured its 
				// child with an infinite extent and it needs to be remeasured with the actual size 
				// available to that panel.
				//
				foreach (VirtualizingDataRecordCellPanel usedPanel in usedCellPanels)
					usedPanel.InvalidateMeasure();

				#endregion //Clean Up
			} 
			#endregion //Records

			// AS 11/9/11 TFS91453
			//foreach (FieldInfo fi in _fieldInfos)
			//{
			//    if (!double.IsNaN(fi.LabelExtent) || !double.IsNaN(fi.CellExtent))
			//        return true;
			//}
			//
			//return false;
			bool hasChanges = false;

			foreach (FieldInfo fi in _fieldInfos)
			{
				fi.RoundExtents();

				if (!hasChanges && (!double.IsNaN(fi.LabelExtent) || !double.IsNaN(fi.CellExtent)))
					hasChanges = true;
			}

			return hasChanges;
		}

		#endregion //Calculate

		#region ExitEditModeIfNeeded
		// AS 8/19/09 TFS21036
		// Return a value indicating if exiting was cancelled so the autosize can be cancelled.
		//
		//internal void ExitEditModeIfNeeded()
		internal bool ExitEditModeIfNeeded()
		{
			DataPresenterBase dp = _fieldLayout.DataPresenter;

			// there are complications to allowing autosizing to occur while in edit mode on a field 
			// that will be autosized (e.g. we only use the values of the actual record when the scope 
			// is viewable/all records and a modified cell in edit mode may not have a valid value 
			// and it definitely won't be in the underlying record yet). so at least for now we're going 
			// to exit edit mode when performing an auto size. note, in apps like excel you can't even 
			// resize a field let alone autosize while editing so its not unprecedented.

			
			
			
			

			if (null != dp)
			{
				Cell activeCell = dp.ActiveCell;

				if (activeCell != null && activeCell.IsInEditMode)
				{
					Field activeField = activeCell.Field;

					foreach (FieldInfo fi in _fieldInfos)
					{
						if (fi.Field == activeField)
						{
							if (0 != (fi.Options & GetCellOption(activeCell.Record, activeField)))
							{
								// AS 8/19/09 TFS21036
								// Try to accept the changes and allow exiting to be cancelled. If it 
								// is then let the caller know so we can skip the auto size operation.
								//
								//dp.EndEditMode(false, true);
								if (!dp.EndEditMode(true, false))
									return false;
							}
							break;
						}
					}
				}
			}

			return true;
		}
		#endregion //ExitEditModeIfNeeded

		#region GetCellElement
		// AS 10/9/09 NA 2010.1 - CardView
		// Changed from private to internal.
		//
		internal static FrameworkElement GetCellElement(FrameworkElement element, bool label)
		{
			if (label)
			{
				LabelPresenter lp = element as LabelPresenter;

				if (null != lp)
					return lp;
			}
			else
			{
				CellValuePresenter cvp = element as CellValuePresenter;

				if (null != cvp)
					return cvp;

				if (element is SummaryResultsPresenter)
					return element;
			}

			CellPresenterBase cp = element as CellPresenterBase;

			if (null == cp)
			{
				Debug.Assert(label == false || element is CellValuePresenter, "Possibly need to edit this method for a different element type?");
				return null;
			}

			return cp.GetChild(label);
		}
		#endregion //GetCellElement

		#region GetCellOption
		internal static FieldAutoSizeOptions GetCellOption(DataRecord record, Field field)
		{
			switch (record.RecordType)
			{
				case RecordType.FilterRecord:
					return FieldAutoSizeOptions.FilterCells;
				case RecordType.DataRecord:
					return FieldAutoSizeOptions.DataCells;
				default:
					Debug.Fail("What type of record is this that the type is neither filter nor data record?");
					return FieldAutoSizeOptions.None;
			}
		}
		#endregion //GetCellOption

		#region GetCellPanel
		// AS 4/12/11 TFS62951
		// Moved to the GridUtilities.
		//
		//internal static VirtualizingDataRecordCellPanel GetCellPanel(RecordPresenter rp)
		//{
		//    // MD 8/19/10
		//    // If the associated cell panel is cached, return it instead of searching for it.
		//    DataRecordPresenter drp = rp as DataRecordPresenter;
		//    if (drp != null)
		//    {
		//        VirtualizingDataRecordCellPanel associatedPanel = drp.AssociatedVirtualizingDataRecordCellPanel;
		//
		//        if (associatedPanel != null)
		//            return associatedPanel;
		//    }
		//
		//    FrameworkElement contentSite = rp.GetRecordContentSite();
		//    Debug.Assert(null != contentSite);
		//
		//    if (null == contentSite)
		//        return null;
		//
		//    VirtualizingDataRecordCellPanel vdrcp = Utilities.GetDescendantFromType(contentSite, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
		//    Debug.Assert(null != vdrcp);
		//
		//    return vdrcp;
		//} 
		#endregion //GetCellPanel

		#region GetCellPresenter
		internal static CellValuePresenter GetCellPresenter(Field f, TemplateDataRecordCache cache)
		{
			return GetTemplateElement(f, cache, false) as CellValuePresenter;
		}
		#endregion //GetCellPresenter

		#region GetExtent
		public double GetExtent(Field field, bool isLabel)
		{
			FieldInfo info = GetFieldInfo(field);
			Debug.Assert(null != info);

			return info.GetExtent(isLabel);
		}

		public double GetExtent(Field field)
		{
			FieldInfo info = GetFieldInfo(field);
			Debug.Assert(null != info);
			return Max(info.GetExtent(true), info.GetExtent(false));
		}
		#endregion //GetExtent

		#region GetFieldInfo
		internal FieldInfo GetFieldInfo(Field field)
		{
			Predicate<FieldInfo> callback = delegate(FieldInfo fieldInfo)
			{
				return fieldInfo.Field == field;
			};

			FieldInfo info = Array.Find(_fieldInfos, callback);
			return info;
		} 
		#endregion //GetFieldInfo

		// AS 9/30/09 TFS22891
		#region GetFields
		internal IEnumerable<Field> GetFields()
		{
			for (int i = 0; i < _fieldInfos.Length; i++)
			{
				yield return _fieldInfos[i].Field;
			}
		}
		#endregion //GetFields

		#region GetPreferredSizes
		public Dictionary<ILayoutItem, Size> GetPreferredSizes(FieldGridBagLayoutManager lm, IList<FieldLayoutItemBase> layoutItems)
		{
			Dictionary<ILayoutItem, Size> sizes = new Dictionary<ILayoutItem, Size>();
			Dictionary<Field, FieldInfo> fieldInfoTable = new Dictionary<Field, FieldInfo>();
			bool isHorz = _fieldLayout.IsHorizontal;

			foreach (FieldInfo fi in _fieldInfos)
				fieldInfoTable[fi.Field] = fi;

			bool useCellPresenters = _fieldLayout.UseCellPresenters;

			foreach (FieldLayoutItemBase layoutItem in layoutItems)
			{
				Field f = layoutItem.Field;

				Debug.Assert(layoutItem == lm.GetLayoutItem(f, layoutItem.IsLabel));

				FieldInfo fi;

				if (!fieldInfoTable.TryGetValue(f, out fi))
				{
					Debug.Fail("Layout item exists within list but not in field infos.");
					continue;
				}

				// when the labels and cells are separate then combine the calculated extents
				double extent = !useCellPresenters
					? Max(fi.LabelExtent, fi.CellExtent)
					: layoutItem.IsLabel ? fi.LabelExtent : fi.CellExtent;

				// if the item wasn't resized then skip it
				if (double.IsNaN(extent))
					continue;

				// AS 10/27/09
				// Found this while testing collapsed cells in card view. Essentially the resize manager
				// will throw an exception if its given a layout item that isn't in the layout so we 
				// need to exclude collapsed items since they won't be in the layout.
				//
				if (((ILayoutItem)layoutItem).Visibility == Visibility.Collapsed)
					continue;

				Size preferredSize = ((ILayoutItem)layoutItem).PreferredSize;

				if (isHorz)
					preferredSize.Height = extent;
				else
					preferredSize.Width = extent;

				sizes[layoutItem] = preferredSize;
			}

			return sizes;
		} 
		#endregion //GetPreferredSizes

		#region GetFieldOptions
		private static Dictionary<Field, AutoSizeFieldSettings> GetFieldOption(Field field, FieldAutoSizeOptions options, FieldAutoSizeScope scope)
		{
			Dictionary<Field, AutoSizeFieldSettings> fieldOptions = new Dictionary<Field, AutoSizeFieldSettings>();
			fieldOptions[field] = new AutoSizeFieldSettings(options, scope);
			return fieldOptions;
		}

		private static Dictionary<Field, AutoSizeFieldSettings> GetFieldOptions(IEnumerable<Field> fields, FieldAutoSizeOptions allowedOptions)
		{
			Dictionary<Field, AutoSizeFieldSettings> fieldOptions = new Dictionary<Field, AutoSizeFieldSettings>();

			foreach (Field field in fields)
			{
				// by default we will consider the current options. i.e. if the label has an explicit  
				// extent then don't resize it
				FieldAutoSizeOptions options = field.GetCurrentAutoSizeOptions();

				// only include the allowed options
				options &= allowedOptions;

				fieldOptions[field] = new AutoSizeFieldSettings(options, field.AutoSizeScopeResolved);
			}

			return fieldOptions;
		}
		#endregion //GetFieldOptions

		#region GetLabelPresenter
		internal static LabelPresenter GetLabelPresenter(Field f, TemplateDataRecordCache cache)
		{
			return GetTemplateElement(f, cache, true) as LabelPresenter;
		}
		#endregion //GetLabelPresenter

		#region GetNestingDepth
		internal static int GetNestingDepth(RecordManager recordManager)
		{
			Debug.Assert(null != recordManager);

			int depth = 0;
			while (null != recordManager && recordManager.ParentRecord != null)
			{
				depth += 2;
				recordManager = recordManager.ParentRecord.ParentDataRecord.RecordManager;
			}

			return depth;
		}

		internal static int GetNestingDepth(FieldLayout fieldLayout)
		{
			Debug.Assert(null != fieldLayout);
			return Math.Max(fieldLayout.NestingDepth, fieldLayout.NestingDepth);
		}
		#endregion //GetNestingDepth

		#region GetTemplateElement
		private static FrameworkElement GetTemplateElement(Field f, TemplateDataRecordCache cache, bool isLabel)
		{
			CellPlaceholder placeholder = cache.GetPlaceHolder(f, isLabel, true);
			Debug.Assert(null != placeholder);

			if (null == placeholder)
				return null;

			Control ctrl = placeholder.ChildCellElement;
			ctrl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			ctrl.UpdateLayout();

			return GetCellElement(ctrl, isLabel);
		}
		#endregion //GetTemplateElement

		#region IncludesOption
		internal bool IncludesOption(FieldAutoSizeOptions option)
		{
			return IncludesOption(option, _options);
		}

		internal static bool IncludesOption(FieldAutoSizeOptions option, FieldAutoSizeOptions options)
		{
			return 0 != (options & option);
		}

		#endregion //IncludesOption

		#region Max
		internal static double Max(double x, double y)
		{
			if (double.IsNaN(x))
				return y;

			if (double.IsNaN(y))
				return x;

			return Math.Max(x, y);
		} 
		#endregion //Max

		#endregion //Methods

		#region FieldInfo class
		internal class FieldInfo
		{
			#region Member Variables

			internal Field Field;
			internal FieldAutoSizeOptions Options;
			internal double CellExtent;
			internal double LabelExtent;

			#endregion //Member Variables

			#region Constructor
			internal FieldInfo(Field field)
			{
				Field = field;
				CellExtent = LabelExtent = double.NaN;
			}
			#endregion //Constructor

			#region Methods
			internal void AddLabelExtent(double extent)
			{
				SetMax(extent, ref LabelExtent);
			}

			internal void AddCellExtent(double extent)
			{
				SetMax(extent, ref CellExtent);
			}

			#region GetExtent
			internal double GetExtent(bool isLabel)
			{
				LayoutItemSize size = this.Field.Owner.IsHorizontal ? LayoutItemSize.PreferredHeight : LayoutItemSize.PreferredWidth;

				if (FieldLayoutItem.ShouldSynchronize(size, this.Field))
					return Max(this.LabelExtent, this.CellExtent);

				return isLabel ? this.LabelExtent : this.CellExtent;
			} 
			#endregion //GetExtent

			// AS 11/9/11 TFS91453
			// In some cases where we use the desired size there are rounding errors when the 
			// elements are arranged by their ancestors such that they get slightly less than 
			// the size they desired so we'll round up the values slightly.
			//
			#region RoundExtents
			internal void RoundExtents()
			{
				this.CellExtent = RoundUp(this.CellExtent);
				this.LabelExtent = RoundUp(this.LabelExtent);
			}

			private static double RoundUp(double value)
			{
				if (!double.IsNaN(value) && value != 0)
				{
					double original = value;

					// using some of the excel logic we had for rounding up a number
					int digits = 15 - (int)Math.Ceiling(Math.Log10(Math.Abs(value)));
					int sign = value >= 0 ? 1 : -1;
					value = Math.Abs(value);

					double multiplier = Math.Pow(10, digits);
					value *= multiplier;
					value = Math.Ceiling(value);
					value /= multiplier;
					value *= sign;

					if (value == -0)
						value = 0;
				}

				return value;
			}
			#endregion //RoundExtents

			private static void SetMax(double value, ref double extent)
			{
				if (double.IsNaN(extent))
					extent = value;
				else if (!double.IsNaN(value))
					extent = Math.Max(value, extent);
			}
			#endregion //Methods
		} 
		#endregion //FieldInfo class

		#region FieldValue struct
		private struct FieldValue : IEquatable<FieldValue>
		{
			#region Member Variables

			internal Field Field;
			internal object Value;

			#endregion //Member Variables

			#region Constructor
			public FieldValue(Field field, object value)
			{
				Field = field;
				Value = value;
			}
			#endregion //Constructor

			#region Base class overrides
			public override bool Equals(object obj)
			{
				if (obj == null || obj is FieldValue == false)
					return false;

				return this.Equals((FieldValue)obj);
			}

			public override int GetHashCode()
			{
				int hash = Field.GetHashCode();

				if (null != Value)
					hash |= Value.GetHashCode();

				return hash;
			}

			public override string ToString()
			{
				return string.Format("Field='{0}', Value='{1}'", Field, Value);
			}
			#endregion //Base class overrides

			#region IEquatable<FieldValue> Members

			public bool Equals(FieldValue other)
			{
				return other.Field == this.Field &&
					object.Equals(other.Value, this.Value);
			}

			#endregion //IEquatable<FieldValue> Members
		}
		#endregion //FieldValue struct

		#region RecordEnumeratorBase class
		private abstract class RecordEnumeratorBase : IEnumerator<Record>
		{
			#region Member Variables

			private IEnumerable<Record> _records;
			private int _depthCount;
			private int _currentDepth;
			private bool _viewableOnly;
			private RecordManager _recordManager;
			private bool _checkParents;
			private int _recordManagerVersion;

			private Record _currentRecord;
			private IEnumerator<Record> _recordsEnumerator;
			private RecordCollectionEnumerator _descendantEnumerator;

			#endregion //Member Variables

			#region Constructor
			protected RecordEnumeratorBase(IEnumerable<Record> records, RecordManager parentRecordManager, bool viewableOnly, int depthCount, int currentDepth, bool checkParents)
			{
				this.Initialize(depthCount, currentDepth, parentRecordManager);

				_viewableOnly = viewableOnly;
				_checkParents = checkParents;

				_records = records;

				this.Reset();
			}
			#endregion //Constructor

			#region Properties
			public int DepthCount
			{
				get { return _depthCount; }
			}

			public int RecordManagerVersion
			{
				get { return _recordManagerVersion; }
			}

			public bool ViewableOnly
			{
				get { return _viewableOnly; }
			} 
			#endregion //Properties

			#region Methods

			#region CreateChildEnumerator

			protected abstract RecordCollectionEnumerator CreateChildEnumerator(RecordCollectionBase childRecords, int newDepth); 

			#endregion //CreateChildEnumerator

			#region Initialize
			protected void Initialize(int depthCount, int currentDepth, RecordManager recordManager)
			{
				Debug.Assert(currentDepth <= depthCount);
				Debug.Assert(depthCount >= 0);

				_depthCount = depthCount;
				_currentDepth = currentDepth;
				_recordManager = recordManager;
				_recordManagerVersion = recordManager == null ? 0 : recordManager.FieldAutoSizeVersion;
			} 
			#endregion //Initialize

			#region OnNewRootRecord
			protected virtual void OnNewRootRecord(Record newRootRecord)
			{
			} 
			#endregion //OnNewRootRecord

			#region ReinitializeRecords
			protected virtual void ReinitializeRecords(ref IEnumerable<Record> _records)
			{
			}
			#endregion //ReinitializeRecords

			#region SetViewableOnly
			internal void SetViewableOnly(bool viewableRecordsOnly)
			{
				if (_viewableOnly != viewableRecordsOnly)
				{
					_viewableOnly = viewableRecordsOnly;
					this.ReinitializeRecords(ref _records);
				}
			}
			#endregion //SetViewableOnly

			#region ShouldInclude
			protected bool ShouldInclude(Record record, bool checkParents)
			{
				if (_viewableOnly)
				{
					if (record.VisibilityResolved != Visibility.Visible)
						return false;

					while (checkParents && null != (record = record.ParentRecord))
					{
						if (!record.IsExpanded)
							return false;

						if (record.VisibilityResolved != Visibility.Visible)
							return false;
					}
				}

				return true;
			}
			#endregion //ShouldInclude

			#region ShouldIncludeChildren
			protected virtual bool ShouldIncludeChildren(Record record)
			{
				// if we're only including viewable records then skip the children
				// if the record is collapsed. we don't need to check visibility 
				// as that would happen when the parent record was evaluated.
				if (_viewableOnly && !record.IsExpanded)
					return false;

				// do not walk into the children of a data record (as that would be 
				// an additional record manager level) if we're at the max depth
				DataRecord dr = record as DataRecord;

				if (null != dr && _currentDepth == _depthCount)
					return false;

				return record.HasChildrenInternal;
			}
			#endregion //ShouldIncludeChildren

			#endregion //Methods

			#region IEnumerator<Record> Members

			public Record Current
			{
				get { return _currentRecord; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { return _currentRecord; }
			}

			public bool MoveNext()
			{
				do
				{
					// if we're processing the children of one of our records...
					if (null != _descendantEnumerator)
					{
						// walk over the children until we find one that we can return 
						while (_descendantEnumerator.MoveNext())
						{
							_currentRecord = _descendantEnumerator.Current;
							return true;
						}

						_descendantEnumerator = null;
					}
					else
					{
						// if we haven't processed the children yet and we can...
						if (null != _currentRecord && ShouldIncludeChildren(_currentRecord))
						{
							// get an enumerator for the children and restart the loop to descend into its children
							int newDepth = _currentDepth;
							RecordCollectionBase children = _currentRecord.ChildRecordsInternal;

							if (children.ParentRecordManager != _recordManager)
								newDepth++;

							_descendantEnumerator = CreateChildEnumerator(_currentRecord.ChildRecordsInternal, newDepth);
							continue;
						}
					}

					// move on to the next sibling
					while (_recordsEnumerator.MoveNext())
					{
						_currentRecord = _recordsEnumerator.Current;

						// if we cannot return this record then try the next record
						if (!ShouldInclude(_currentRecord, _checkParents))
							continue;

						this.OnNewRootRecord(_currentRecord);

						// the record can be returned so bail out of the inner loop
						return true;
					}

					_currentRecord = null;
					return false;

				} while (null != _currentRecord);

				return null != _currentRecord;
			}

			public void Reset()
			{
				_recordsEnumerator = _records.GetEnumerator();
				_descendantEnumerator = null;
				_currentRecord = null;
			}

			#endregion //IEnumerator Members
		}
		#endregion //RecordCollectionEnumerator class

		#region RecordCollectionEnumerator class
		private class RecordCollectionEnumerator : RecordEnumeratorBase
		{
			#region Member Variables

			private RecordCollectionBase _records;

			#endregion //Member Variables

			#region Constructor
			internal RecordCollectionEnumerator(RecordCollectionBase records, int depthCount)
				: this(records, depthCount, true, 0, true)
			{
			}

			internal RecordCollectionEnumerator(RecordCollectionBase records, int depthCount, bool viewableOnly, int currentDepth, bool checkParents) 
				: base(viewableOnly ? records.ViewableRecords : records.GetAllRecords(), records.ParentRecordManager, viewableOnly, depthCount, currentDepth, checkParents)
			{
				_records = records;
			}
			#endregion //Constructor

			#region Base class overrides
			protected override RecordCollectionEnumerator CreateChildEnumerator(RecordCollectionBase childRecords, int newDepth)
			{
				return new RecordCollectionEnumerator(childRecords, this.DepthCount, this.ViewableOnly, newDepth, false);
			}

			protected override void ReinitializeRecords(ref IEnumerable<Record> records)
			{
				records = this.ViewableOnly ? _records.ViewableRecords : _records.GetAllRecords();
			}
			#endregion //Base class overrides
		}
		#endregion //RecordCollectionEnumerator class

		#region RecordEnumerator class
		private class RecordEnumerator : RecordEnumeratorBase
		{
			#region Member Variables

			private int _maxDepth;
			private bool _recursive;

			#endregion //Member Variables

			#region Constructor
			internal RecordEnumerator(IEnumerable<Record> records, int maxDepth, bool recursive)
				: base(records, null, true, 0, 0, true)
			{
				_maxDepth = maxDepth;
				_recursive = recursive;
			}
			#endregion //Constructor

			#region Base class overrides
			protected override RecordCollectionEnumerator CreateChildEnumerator(RecordCollectionBase childRecords, int newDepth)
			{
				return new RecordCollectionEnumerator(childRecords, this.DepthCount, this.ViewableOnly, newDepth, false);
			}

			protected override void OnNewRootRecord(Record newRootRecord)
			{
				base.OnNewRootRecord(newRootRecord);

				int levels = _maxDepth - newRootRecord.RecordManager.NestingDepth;

				Debug.Assert(levels >= 0);
				levels = Math.Max(0, levels);

				// since each record may be at a different level we want to update the record manager that the 
				// enumerator uses for its evaluation during the descent into the children of the current
				this.Initialize(levels, 0, newRootRecord.RecordManager);
			}

			protected override bool ShouldIncludeChildren(Record record)
			{
				return _recursive && base.ShouldIncludeChildren(record);
			}
			#endregion //Base class overrides
		}
		#endregion //RecordEnumerator class

		#region FieldList class
		/// <summary>
		/// Helper class for maintaing a list of fields based on their Index
		/// </summary>
		internal class FieldList
		{
			#region Member Variables

			private const int Factor = 10;
			private int[] _fields;

			#endregion //Member Variables

			#region Constructor
			internal FieldList(int size)
			{
				_fields = new int[size / Factor + 1];
			}
			#endregion //Constructor

			#region Member Variables
			internal bool Contains(Field field)
			{
				return 0 != (_fields[field.Index / Factor] & (1 << (field.Index % Factor)));
			}

			internal void Add(Field field)
			{
				_fields[field.Index / Factor] |= 1 << (field.Index % Factor);
			}

			internal bool Remove(Field field)
			{
				if (!Contains(field))
					return false;
				
				_fields[field.Index / Factor] &= ~(1 << (field.Index % Factor));
				return true;
			}
			#endregion //Member Variables
		} 
		#endregion //FieldList class

		#region RecordFieldList class
		/// <summary>
		/// Helper class for maintaing a list of fields per record based on the index of the field
		/// </summary>
		internal class RecordFieldList
		{
			#region Member Variables

			private const int Factor = 10;
			private WeakDictionary<Record, FieldList> _items;

			#endregion //Member Variables

			#region Constructor
			internal RecordFieldList()
			{
				_items = new WeakDictionary<Record, FieldList>(true, false);
			}
			#endregion //Constructor

			#region Properties
			internal FieldList this[Record record]
			{
				get
				{
					FieldList fields;
					_items.TryGetValue(record, out fields);
					return fields;
				}
				set
				{
					Debug.Assert(null != record);
					_items[record] = value;
				}
			} 
			#endregion //Properties

			#region Methods
			internal void Add(Record record, Field field)
			{
				FieldList fields;

				if (!_items.TryGetValue(record, out fields))
				{
					_items[record] = fields = new FieldList(record.FieldLayout.Fields.Count);
				}

				fields.Add(field);
			}

			internal void Clear()
			{
				_items.Clear();
			}

			internal bool Contains(Record record, Field field)
			{
				FieldList fields;

				if (!_items.TryGetValue(record, out fields))
					return false;

				if (field == null)
					return true;

				return fields.Contains(field);
			}

			internal bool Remove(Record record)
			{
				return _items.Remove(record);
			}

			internal bool Remove(RecordType recordType)
			{
				List<Record> records = null;

				foreach (Record record in _items.Keys)
				{
					if (record.RecordType == recordType)
					{
						if (records == null)
							records = new List<Record>();

						records.Add(record);
					}
				}

				if (null == records)
					return false;

				for (int i = 0, count = records.Count; i < count; i++)
					_items.Remove(records[i]);

				return true;
			}

			internal bool Remove(Field field)
			{
				bool changed = false;

				foreach (KeyValuePair<Record, FieldList> pair in _items)
				{
					if (pair.Value.Remove(field))
						changed = true;
				}

				return changed;
			}

			internal bool Remove(Record record, Field field)
			{
				if (field == null)
					return this.Remove(record);

				FieldList fields;

				if (!_items.TryGetValue(record, out fields))
					return false;

				return fields.Remove(field);
			}
			#endregion //Methods
		}
		#endregion //RecordFieldList class
	}

	#region AutoSizeFieldSettings
	/// <summary>
	/// Provides information about a field autosize operation.
	/// </summary>
	internal struct AutoSizeFieldSettings
	{
		#region Member Variables

		internal FieldAutoSizeOptions Options;
		internal FieldAutoSizeScope Scope; 

		#endregion //Member Variables

		#region Constructor
		internal AutoSizeFieldSettings(Field field)
		{
			Options = field.AutoSizeOptionsResolved;
			Scope = field.AutoSizeScopeResolved;
		}

		internal AutoSizeFieldSettings(FieldAutoSizeOptions options, FieldAutoSizeScope scope)
		{
			Options = options;
			Scope = scope;
		} 
		#endregion //Constructor
	}
	#endregion //AutoSizeFieldSettings

	// AS 3/2/11 66934 - AutoSize
	// Added a helper class so we don't get the RecordsInView multiple times 
	// and also so we can skip processing a record when none of the fields 
	// that are candidates for autosizing are actually going to be in view.
	//
	#region RecordsInViewHelper
	internal class RecordsInViewHelper
	{
		#region Member Variables

		private DataPresenterBase _control;
		private List<RecordPresenter> _recordPresentersInView;
		private Record[] _recordsInView;
		private RecordManager _lastRecordManager;
		private bool _lastShouldProcessResult;
		private Dictionary<RecordManager, bool> _inViewRecordManagerTable;
		private List<VisibleDataBlock> _visibleCells;

		#endregion //Member Variables

		#region Constructor
		internal RecordsInViewHelper(DataPresenterBase control)
		{
			_control = control;
		}
		#endregion //Constructor

		#region Internal Methods

		#region GetRecords
		internal Record[] GetRecords()
		{
			if (_recordsInView == null)
				this.CacheRecordsInView();

			return _recordsInView;
		}
		#endregion //GetRecords

		#region ShouldProcessInViewRecord
		internal bool ShouldProcessInViewRecord(Record record, IList<AutoSizeFieldHelper.FieldInfo> fieldInfos)
		{
			RecordManager rm = record.RecordManager;

			if (rm != _lastRecordManager)
			{
				if (_inViewRecordManagerTable == null)
					_inViewRecordManagerTable = new Dictionary<RecordManager, bool>();

				if (!_inViewRecordManagerTable.TryGetValue(rm, out _lastShouldProcessResult))
				{
					bool shouldProcess = ShouldProcessRecordManager(rm, fieldInfos);
					_inViewRecordManagerTable[rm] = _lastShouldProcessResult = shouldProcess;
				}

				_lastRecordManager = rm;
			}

			return _lastShouldProcessResult;
		}
		#endregion //ShouldProcessInViewRecord

		#endregion //Internal Methods

		#region Private Methods

		#region CacheRecordsInView
		private void CacheRecordsInView()
		{
			var recordPresenters = this.GetRecordPresentersInView();
			Record[] recordArray = new Record[recordPresenters.Count];

			// fill the array with the rps' Records
			for (int i = 0; i < recordPresenters.Count; i++)
				recordArray[i] = recordPresenters[i].Record;

			_recordsInView = recordArray;
		}
		#endregion //CacheRecordsInView

		#region GetRecordPresentersInView
		private List<RecordPresenter> GetRecordPresentersInView()
		{
			if (_recordPresentersInView == null)
				_recordPresentersInView = _control.GetRecordPresentersInView(true, true);

			return _recordPresentersInView;
		}
		#endregion //GetRecordPresentersInView

		#region ShouldProcessRecordManager
		private bool ShouldProcessRecordManager(RecordManager rm, IList<AutoSizeFieldHelper.FieldInfo> fieldInfos)
		{
			bool shouldProcess = false;

			if (_visibleCells == null)
				_visibleCells = VisibleDataBlock.Create(this.GetRecordPresentersInView());

			for (int i = 0, count = _visibleCells.Count; i < count; i++)
			{
				if (_visibleCells[i].RecordManager == rm)
				{
					foreach (AutoSizeFieldHelper.FieldInfo fieldInfo in fieldInfos)
					{
						if (_visibleCells[i].Fields.Contains(fieldInfo.Field))
						{
							shouldProcess = true;
							break;
						}
					}

					break;
				}

				Debug.Assert(i < count, "We didn't find the record manager in the in view list?");
			}
			return shouldProcess;
		}
		#endregion //ShouldProcessRecordManager

		#endregion //Private Methods
	} 
	#endregion //RecordsInViewHelper
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