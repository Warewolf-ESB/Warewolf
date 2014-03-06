
//#undef USE_CELLSBEFORE

using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Word;
using System.Windows;
using Infragistics.Windows.DataPresenter.Internal;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using Infragistics.Windows.DataPresenter.Events;
using WordFont = Infragistics.Documents.Word.Font;
using System.IO;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	
#region Infragistics Source Cleanup (Region)












































































#endregion // Infragistics Source Cleanup (Region)

	internal class WordExporter : IDataPresenterExporterAsync
		, IExportOptions
	{
		#region Member Variables

		internal static readonly Color EmptyColor = new Color();

		private DataPresenterWordWriter _owner;
		private WordDocumentWriter _writer;
		private DataPresenterBase _source;
		private DataPresenterBase _exportControl;
		private InternalFlags _flags;
		private Stream _exportStream;

		private Dictionary<FieldLayout, FieldLayoutCache> _fieldLayoutCacheTable;
		private Stack<TableInfo> _currentStack;
		private TableProperties _freeFormTableProperties;

		private StringBuilder _tempStringBuilder;
		private TableCellProperties _tempCellProperties;
		private TableCellProperties _tempCellPropertiesWithBorders;
		private ParagraphProperties _tempParagraphProperties;
		private TableRowProperties _tempRowProperties;
		private WordFont _tempFont;
		private TableCellProperties _hiddenCellProperties;
		private TableBorderProperties _tempBorderProperties;

		private ExportInfo _exportInfo;

		#endregion //Member Variables

		#region Constructor
		internal WordExporter(DataPresenterBase source, DataPresenterWordWriter owner, string filename)
			: this(source, owner, new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), true)
		{
			_exportInfo.FileName = filename;
		}

		internal WordExporter(DataPresenterBase source, DataPresenterWordWriter owner, Stream exportStream, bool closeOnEnd)
			: this(source, owner, (WordDocumentWriter)null)
		{
			ValidateExportStream(exportStream);

			_exportStream = exportStream;
			this.SetFlag(InternalFlags.CloseStreamOnEnd, closeOnEnd);
		}

		internal WordExporter(DataPresenterBase dataPresenter, DataPresenterWordWriter owner, WordDocumentWriter writer)
		{
			Utilities.ValidateNotNull(dataPresenter, "dataPresenter");
			ValidateCanExport(owner, dataPresenter);

			_source = dataPresenter;
			_owner = owner;
			_writer = writer;
			_currentStack = new Stack<TableInfo>();
			_fieldLayoutCacheTable = new Dictionary<FieldLayout, FieldLayoutCache>();
			_exportInfo = new ExportInfo();
			_exportInfo.ExportType = "Microsoft Word";

			// initialize state flags
			this.SetFlag(InternalFlags.ExcludeExpandedState, owner.ExcludeExpandedState);
			this.SetFlag(InternalFlags.ExcludeFieldLayoutSettings, owner.ExcludeFieldLayoutSettings);
			this.SetFlag(InternalFlags.ExcludeFieldSettings, owner.ExcludeFieldSettings);
			this.SetFlag(InternalFlags.ExcludeGroupBySettings, owner.ExcludeGroupBySettings);
			this.SetFlag(InternalFlags.ExcludeRecordFilters, owner.ExcludeRecordFilters);
			this.SetFlag(InternalFlags.ExcludeRecordVisibility, owner.ExcludeRecordVisibility);
			this.SetFlag(InternalFlags.ExcludeSortOrder, owner.ExcludeSortOrder);
			this.SetFlag(InternalFlags.ExcludeSummaries, owner.ExcludeSummaries);
		}
		#endregion //Constructor

		#region Properties

		#region Private Properties

		#region FreeFormTableProperties
		private TableProperties FreeFormTableProperties
		{
			get
			{
				if (_freeFormTableProperties == null)
				{
					Debug.Assert(null != _writer, "We need a writer!");

					TableProperties tableProps = _writer.CreateTableProperties();
					tableProps.PreferredWidthAsPercentage = 100f;
					tableProps.Layout = TableLayout.Fixed;
					tableProps.BorderProperties.Sides = TableBorderSides.None;
					tableProps.BorderProperties.Width = 0;
					tableProps.BorderProperties.Color = Colors.Black;
					tableProps.BorderProperties.Style = TableBorderStyle.None;
					_freeFormTableProperties = tableProps;
				}

				return _freeFormTableProperties;
			}
		}
		#endregion //FreeFormTableProperties

		#region HiddenCellProperties
		private TableCellProperties HiddenCellProperties
		{
			get
			{
				if (_hiddenCellProperties == null)
				{
					_hiddenCellProperties = _writer.CreateTableCellProperties();
					var borderProps = _hiddenCellProperties.BorderProperties;
					borderProps.Color = Colors.Transparent;
					borderProps.Sides = TableBorderSides.None;
					borderProps.Style = TableBorderStyle.None;
				}

				return _hiddenCellProperties;
			}
		} 
		#endregion //HiddenCellProperties

		#region TempRowProperties
		private TableRowProperties TempRowProperties
		{
			get
			{
				if (_tempRowProperties == null)
				{
					Debug.Assert(null != _writer, "We need a writer!");
					_tempRowProperties = _writer.CreateTableRowProperties();
				}

				return _tempRowProperties;
			}
		}
		#endregion //TempRowProperties

		#endregion //Private Properties

		#region Public Properties

		#region Source
		public DataPresenterBase Source
		{
			get { return _source; }
		}
		#endregion //Source 

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region ConvertUnits
		internal static float? ConvertUnits(DeviceUnitLength? value, float? minInTwips, float? maxInTwips, UnitOfMeasurement targetUnit)
		{
			if (value == null)
				return null;

			Debug.Assert(minInTwips == null || maxInTwips == null || minInTwips <= maxInTwips, "Min/Max are invalid");

			return ConvertUnits(value.Value.ConvertToUnitType(DeviceUnitType.Twip), minInTwips, maxInTwips, UnitOfMeasurement.Twip, targetUnit);
		}

		internal static float? ConvertUnits(double? value, float? min, float? max, UnitOfMeasurement sourceUnit, UnitOfMeasurement targetUnit)
		{
			if (value == null)
				return null;

			double floatValue = value.Value;

			Debug.Assert(!double.IsInfinity(value.Value), "Infinite?");
			Debug.Assert(!double.IsNaN(value.Value), "NaN?");

			if (double.IsInfinity(floatValue) || double.IsNaN(floatValue))
				return null;

			floatValue = Math.Min(floatValue, max ?? float.MaxValue);
			floatValue = Math.Max(floatValue, min ?? float.MinValue);

			return WordDocumentWriter.ConvertUnits((float)floatValue, sourceUnit, targetUnit);
		}

		internal static Padding? ConvertUnits(DeviceUnitThickness? value, UnitOfMeasurement targetUnit)
		{
			if (value == null)
				return null;

			DeviceUnitThickness twipPadding = value.Value.ConvertToUnitType(DeviceUnitType.Twip);

			return new Padding(
				ConvertUnits(twipPadding.Left, null, null, UnitOfMeasurement.Twip, targetUnit).Value,
				ConvertUnits(twipPadding.Right, null, null, UnitOfMeasurement.Twip, targetUnit).Value,
				ConvertUnits(twipPadding.Top, null, null, UnitOfMeasurement.Twip, targetUnit).Value,
				ConvertUnits(twipPadding.Bottom, null, null, UnitOfMeasurement.Twip, targetUnit).Value
				);
		}

		internal static Size? ConvertUnits(DeviceUnitSize? value, UnitOfMeasurement targetUnit)
		{
			if (value == null)
				return null;

			DeviceUnitSize twipSize = value.Value.ConvertToUnitType(DeviceUnitType.Twip);

			return new Size(
				ConvertUnits(twipSize.Width, null, null, UnitOfMeasurement.Twip, targetUnit).Value,
				ConvertUnits(twipSize.Height, null, null, UnitOfMeasurement.Twip, targetUnit).Value
				);
		}

		#endregion //ConvertUnits

		#region Export
		internal void Export()
		{
			if (!this.OnExportStarting())
				return;

			_source.Export(this, this);
		}
		#endregion //Export

		#region ExportAsync
		internal void ExportAsync()
		{
			if (!this.OnExportStarting())
				return;

			_source.ExportAsync(this, this, _owner.ShowAsyncExportStatus, _owner.AsyncExportDuration, _owner.AsyncExportInterval);
		}
		#endregion //ExportAsync

		#region GetString
		internal static string GetString(string resource)
		{
#pragma warning disable 436
			return SR.GetString(resource);
#pragma warning restore 436
		} 
		#endregion //GetString

		#region Merge
		/// <summary>
		/// Copies the settings of the specified dependency object using the specified method.
		/// </summary>
		/// <typeparam name="T">The type of object to merge</typeparam>
		/// <param name="mergedItem">The item into which the state should be merged or null to have the item lazily allocated</param>
		/// <param name="sourceItems">The items whose properties are being copied</param>
		/// <returns>The merged object or null if there were no objects to copy</returns>
		internal static T Merge<T>(ref T mergedItem, params T[] sourceItems)
			where T : DependencyObject, new()
		{
			if (null != sourceItems)
			{
				for (int i = 0; i < sourceItems.Length; i++)
				{
					T item = sourceItems[i];

					if (item == null)
						continue;

					var enumerator = item.GetLocalValueEnumerator();

					// skip objects that were allocated but have no settings
					if (enumerator.Count == 0)
						continue;

					// only allocate it if there are some settings
					if (mergedItem == null)
						mergedItem = new T();

					while (enumerator.MoveNext())
					{
						DependencyProperty property = enumerator.Current.Property;

						if (property.ReadOnly)
							continue;

						MergeHelper(property, item, mergedItem);
					}
				}
			}

			return mergedItem;
		}
		#endregion //Merge

		#region ResolveProperty
		internal static Color ResolveProperty(Color value, Color defaultColor)
		{
			return ResolveProperty(value, defaultColor, EmptyColor);
		}

		internal static T ResolveProperty<T>(T value, T defaultValue, T emptyValue)
		{
			return EqualityComparer<T>.Default.Equals(value, emptyValue)
				? defaultValue
				: value;
		}
		#endregion //ResolveProperty

		#region ValidateCanExport
		internal static void ValidateCanExport(DataPresenterWordWriter writer, DataPresenterBase exportSource)
		{
			Utilities.ValidateNotNull(exportSource, "exportSource");
			Utilities.ValidateNotNull(writer, "writer");
		}
		#endregion //ValidateCanExport

		#region ValidateExportStream
		internal static void ValidateExportStream(Stream stream)
		{
			if (null == stream)
				throw new ArgumentNullException("stream");

			if (!stream.CanWrite)
				throw new InvalidOperationException(GetString("NotSupported_UnwritableStream"));
		}
		#endregion //ValidateExportStream

		#endregion //Internal Methods

		#region Private Methods

		#region AddCellsRecord
		private void AddCellsRecord(Record record, TableInfo ti, int indent, IList<RowInfo> rowInfos)
		{
			this.AddCellsRecord(record, ti, indent, rowInfos, null, 0, null, null);
		}

		private void AddCellsRecord(Record record, TableInfo ti, int indent, IList<RowInfo> rowInfos, Dictionary<Field, IList<SummaryResult>> summaryTable, int leadingEmptyCellCount, TableBorderSides? rowBorderSides, WordTableCellSettings leadingCellSettings)
		{
			if (rowInfos.Count == 0)
				return;

			DataRecord dataRecord = record as DataRecord;

			bool isHeaderRow = rowInfos == ti.FieldLayoutCache.HeaderRecordRowInfos;
			var rowProps = this.TempRowProperties;
			rowProps.Reset();

			if (isHeaderRow)
			{
				rowProps.AllowPageBreak = false;
				rowProps.IsHeaderRow = isHeaderRow && !ti.HasAddedRootHeader;
			}

			int rowCount = rowInfos.Count;
			bool isHidden = isHeaderRow == false && record.VisibilityResolved == Visibility.Hidden;
			int cellIndent = indent + leadingEmptyCellCount;
			int maxColumn = Math.Min(ti.ColumnCount, FieldLayoutCache.MaxColumnCount) - cellIndent;

			// since the border properties are honored on continue cells we need to cache the settings 
			// the developer provided if they handle the cellinitializing and manipulate the settings
			Dictionary<Field, WordTableCellSettings> modifiedCellSettingTable = null;

			for (int i = 0; i < rowCount; i++)
			{
				var row = rowInfos[i];
				this.StartTableRow(rowProps, ti, indent);

				bool isFirstRow = i == 0;
				bool isLastRow = i == rowCount - 1;
				TableBorderSides? actualRowBorderSides = rowBorderSides;

				#region Empty Cells
				if (leadingEmptyCellCount > 0)
				{
					if (actualRowBorderSides != null && rowCount > 1)
					{
						if (!isFirstRow)
							actualRowBorderSides &= ~TableBorderSides.Top;

						if (!isLastRow)
							actualRowBorderSides &= ~TableBorderSides.Bottom;
					}

					actualRowBorderSides = this.AddLeadingEmptyCells(ti, actualRowBorderSides, leadingCellSettings, leadingEmptyCellCount);

					// put back the top and bottom. really the bottom should only be needed and then only 
					// because we could have a cell that spans multiple cells
					if (null != actualRowBorderSides)
						actualRowBorderSides |= (rowBorderSides & TableBorderSides.TopAndBottom);
				} 
				#endregion //Empty Cells

				#region Write Cell Values
				foreach (var item in row.Items)
				{
					// by default use the cached table cell props...
					var cellProps = item.TableCellProperties;
					var paraProps = item.ParagraphProperties;
					var font = item.Font;

					object value = null;

					// ignore fields beyond the max count
					if (cellIndent + item.Column >= FieldLayoutCache.MaxColumnCount)
						continue;

					if (item.VerticalMerge != TableCellVerticalMerge.Continue)
					{
						if (isHidden)
						{
							#region Hidden Cell

							cellProps = this.HiddenCellProperties;
							cellProps.ColumnSpan = item.ColumnSpan;
							cellProps.VerticalMerge = item.VerticalMerge;
							paraProps = null;
							font = null; 

							#endregion //Hidden Cell
						}
						else if (item.ItemType != RowInfoItemType.EmptySpace)
						{
							var field = item.Field;
							var cellSettings = item.CellSettings;

							#region Label/Cell Value
							if (item.ItemType == RowInfoItemType.Label)
							{
								value = this.RaiseLabelExporting(field, record, ref cellSettings);
							}
							else if (item.ItemType == RowInfoItemType.Value)
							{
								if (dataRecord != null)
								{
									value = this.RaiseCellExporting(field, dataRecord, ref cellSettings);
								}
								else if (summaryTable != null)
								{
									IList<SummaryResult> results;
									if (summaryTable.TryGetValue(field, out results))
									{
										value = this.RaiseSummaryResultsExporting(field, record, results, SummaryPosition.UseSummaryPositionField, ref cellSettings);
									}
								}
							} 
							#endregion //Label/Cell Value

							#region If Settings Change In Exporting Event
							// if the cell format settings were changed then we need a new 
							// tablecellproperties instead of the cached one we were about to use
							if (cellSettings != item.CellSettings)
							{
								if (modifiedCellSettingTable == null)
									modifiedCellSettingTable = new Dictionary<Field, WordTableCellSettings>();

								modifiedCellSettingTable[item.Field] = cellSettings;

								cellProps = this.GetTempCellProperties(cellSettings, item.ColumnSpan, item.VerticalMerge);
								paraProps = this.GetTempParagraphProperties(cellSettings, false);
								font = this.GetTempFont(cellSettings);
							}
							else if (font == null)
							{
								font = this.GetTempFont(cellSettings);
							}
							#endregion //If Settings Change In Exporting Event
						}
					}
					else
					{
						#region Continue Cell

						cellProps = this.GetTempCellProperties(null, item.ColumnSpan, TableCellVerticalMerge.Continue);
						font = null;
						paraProps = null;

						WordTableCellSettings continueCellSettings = null;

						// the border properties for a continue cell are honored. if the settings were 
						// changed by the developer in the exporting event then use those settings 
						// otherwise use the ones cached for the item
						if (null == modifiedCellSettingTable || !modifiedCellSettingTable.TryGetValue(item.Field, out continueCellSettings))
							continueCellSettings = item.CellSettings;

						// if we have cell settings then initialize the borders
						if (null != continueCellSettings && continueCellSettings.ShouldSerializerBorderSettings())
							continueCellSettings.Initialize(_writer, cellProps.BorderProperties);

						#endregion //Continue Cell
					}

					var borderPropsToRestore = cellProps.HasBorderProperties ? cellProps.BorderProperties : null;

					#region Row Borders
					// if we have row borders specified then we're not using cell borders so manipulate
					if (rowBorderSides != null)
					{
						TableBorderSides borderSides = actualRowBorderSides.Value;

						// strip off the border sides that don't apply based on where 
						// the cell is located...
						if (item.Column > 0)
							borderSides &= ~TableBorderSides.Left;

						if (item.Column + item.ColumnSpan < maxColumn)
							borderSides &= ~TableBorderSides.Right;

						if (!isFirstRow)
							borderSides &= ~TableBorderSides.Top;

						if (!isLastRow && i + item.RowSpan < rowCount)
							borderSides &= ~TableBorderSides.Bottom;

						var borderProps = this.GetTempBorderProperties(borderSides);
						cellProps.BorderProperties = borderProps;

						// copy over anything that was on the border properties
						if (null != borderPropsToRestore)
						{
							borderProps.Style = borderPropsToRestore.Style;
							borderProps.Color = borderPropsToRestore.Color;
							borderProps.Width = borderPropsToRestore.Width;
						}
					}
					#endregion //Row Borders

					int? oldColumnSpan = null;

					if (cellIndent + item.Column + cellProps.ColumnSpan > FieldLayoutCache.MaxColumnCount)
					{
						oldColumnSpan = cellProps.ColumnSpan;
						cellProps.ColumnSpan = FieldLayoutCache.MaxColumnCount - (cellIndent + item.Column);
					}

					_writer.StartTableCell(cellProps);

					this.WriteCellValue(paraProps, font, value);

					_writer.EndTableCell();

					if (oldColumnSpan != null)
						cellProps.ColumnSpan = oldColumnSpan.Value;

					// if we swapped out the borders then put the original back
					var currentBorderProps = cellProps.HasBorderProperties ? cellProps.BorderProperties : null;

					if (borderPropsToRestore != currentBorderProps)
						cellProps.BorderProperties = borderPropsToRestore;
				} 
				#endregion //Write Cell Values

				_writer.EndTableRow();
			}
		}
		#endregion //AddRecord

		#region AddExpandableFieldRecord
		private void AddExpandableFieldRecord(ExpandableFieldRecord record, TableInfo ti)
		{
			Debug.Assert(ti.CurrentExpandableFieldRecord == record, "Different record then we have a table for?");

			if (ti.CurrentExpandableFieldRecord != record)
				return;

			if (ti.IsExpandableFieldRecordTableNeeded)
				this.StartNestedExpandableFieldRecordTable(ti);
			else if (!ti.HasCreatedExpandableFieldRecordTable)
				return;

			// we need to get the cache for the record's field layout since the table info specified will be that 
			// of the child table with which this record is associated
			FieldLayoutCache cache = GetCache(record.FieldLayout);
			WordTableCellSettings headerCellSettings = cache.ExpandableFieldRecordCellSettingsCache[record.Field];
			RecordManager childManager = record.ChildRecordManager;
			object value = childManager != null ? null : record.ParentDataRecord.GetCellValue(record.Field);
			var dataRecordCellSettings = childManager != null ? null : cache.DataRecordCellSettingsCache[record.Field];

			if (childManager == null && ti.FieldLayoutCache.ChildRecordsDisplayOrder != ChildRecordsDisplayOrder.AfterParent)
				this.AddSingleValueRecord(ti, 0, 1, value, null, dataRecordCellSettings, false, false);

			if (ti.IsExpandableFieldRecordHeaderPending)
				this.AddSingleValueRecord(ti, 0, 1, record.Description, null, headerCellSettings, true, false);

			if (childManager == null && ti.FieldLayoutCache.ChildRecordsDisplayOrder == ChildRecordsDisplayOrder.AfterParent)
				this.AddSingleValueRecord(ti, 0, 1, value, null, dataRecordCellSettings, false, false);

			ti.IsExpandableFieldRecordHeaderPending = false;
		}
		#endregion //AddExpandableFieldRecord

		#region AddFreeFormSummaries
		/// <summary>
		/// Creates a row for the free form summaries (i.e. those aligned left/right/center).
		/// </summary>
		private void AddFreeFormSummaries(TableInfo ti, int recordIndent, Record record, TableBorderSides borderSides, Dictionary<SummaryPosition, IList<SummaryResult>> table, WordTableCellSettings cellSettings)
		{
			this.StartTableRow(null, ti, recordIndent);

			int emptyCellCount = ti.DataRecordIndent - recordIndent;

			if (emptyCellCount > 0)
				borderSides = this.AddLeadingEmptyCells(ti, borderSides, cellSettings, emptyCellCount).Value;

			// we need a cell without any margins that will contain the nested table 
			// which contains the free form summaries
			var cellProps = _writer.CreateTableCellProperties();
			cellProps.ColumnSpan = ti.ColumnCount - ti.DataRecordIndent;
			cellProps.Margins = new Padding();
			cellProps.BorderProperties = GetTempBorderProperties(borderSides);

			// note i'm applying the formatting here because there could be space around the table within and 
			// we don't want white space showing up
			if (null != cellSettings)
				cellSettings.Initialize(_writer, cellProps);

			_writer.StartTableCell(cellProps);

			StringBuilder sb = null;

			// the nested table should fill the cell and itself be border less
			TableProperties tableProps = this.FreeFormTableProperties;

			_writer.StartTable(3, tableProps);

			#region Summaries Table Row

			_writer.StartTableRow();

			IList<SummaryResult> tempResults;
			object value;

			WordTableCellSettings actualFormatSettings;

			// left
			actualFormatSettings = cellSettings;
			table.TryGetValue(SummaryPosition.Left, out tempResults);
			value = this.RaiseSummaryResultsExporting(null, record, tempResults, SummaryPosition.Left, ref actualFormatSettings);
			AddFreeFormSummaryCell(value, sb, actualFormatSettings, ParagraphAlignment.Left);

			// center
			actualFormatSettings = cellSettings;
			table.TryGetValue(SummaryPosition.Center, out tempResults);
			value = this.RaiseSummaryResultsExporting(null, record, tempResults, SummaryPosition.Center, ref actualFormatSettings);
			AddFreeFormSummaryCell(value, sb, actualFormatSettings, ParagraphAlignment.Center);

			// right
			table.TryGetValue(SummaryPosition.Right, out tempResults);
			value = this.RaiseSummaryResultsExporting(null, record, tempResults, SummaryPosition.Right, ref actualFormatSettings);
			AddFreeFormSummaryCell(value, sb, actualFormatSettings, ParagraphAlignment.Right);

			_writer.EndTableRow();

			#endregion //Summaries Table Row

			_writer.EndTable();

			_writer.EndTableCell();

			_writer.EndTableRow();
		}
		#endregion //AddFreeFormSummaries

		#region AddFreeFormSummaryCell
		private void AddFreeFormSummaryCell(object value,
			StringBuilder sb,
			WordTableCellSettings cellSettings,
			ParagraphAlignment paragraphAlignment)
		{
			var paraProperties = this.GetTempParagraphProperties(cellSettings, true);

			if (paraProperties.Alignment == null)
				paraProperties.Alignment = paragraphAlignment;

			_writer.StartTableCell(this.GetTempCellProperties(cellSettings, 1, TableCellVerticalMerge.None));
			this.WriteCellValue(paraProperties, this.GetTempFont(cellSettings), value);
			_writer.EndTableCell();
		}
		#endregion //AddFreeFormSummaryCell

		#region AddHeaderRecord
		private bool AddHeaderRecord(Record record, int recordIndent, TableInfo ti, bool isAboutToAddNestedTable)
		{
			// if we don't need one then don't add it now
			if (!ti.NeedsHeaderRecord)
				return false;

			// if we don't have separate headers then we don't need to do anything 
			// but clear the flag so we know we have processed at least one request 
			// for a header
			if (ti.FieldLayoutCache.LabelLocation != LabelLocation.SeparateHeader)
			{
				// set this flag as well so 
				ti.HasAddedRootHeader = true;

				ti.NeedsHeaderRecord = false;
				return false;
			}

			if (isAboutToAddNestedTable)
			{
				// if the header isn't supposed to be above the children then bail out now and wait to add 
				// the header
				if (ti.FieldLayoutCache.ChildRecordsDisplayOrder != ChildRecordsDisplayOrder.BeforeParent)
					return false;

				ti.NeedsHeaderRecord = false;
			}
			else
			{
				if (record is ExpandableFieldRecord)
				{
					if (ti.FieldLayoutCache.ChildRecordsDisplayOrder != ChildRecordsDisplayOrder.BeforeParent)
						return false;
				}

				if (ti.HasAddedRootHeader)
				{
					if (!ti.FieldLayoutCache.HasGroupBySortFields)
					{
						Debug.Assert(ti.FieldLayoutCache.HeaderPlacement == HeaderPlacement.OnRecordBreak, "We've added the header but we want to add another and we're not using onrecordbreak?");

						// clear the flag - we will either add one now or we won't need one
						ti.NeedsHeaderRecord = false;

						// depending on the record being processed we may not need the header
						if (record is GroupByRecord == false &&
							record.ParentCollection != null &&
							record.ParentCollection.GroupByField == null)
						{
							// let the header get added
						}
						else
						{
							return false;
						}
					}
					else
					{
						Debug.Assert(ti.FieldLayoutCache.HeaderPlacementInGroupBy == HeaderPlacementInGroupBy.WithDataRecords);
						ti.NeedsHeaderRecord = false;
					}
				}
				else
				{
					// headers are never above the sort by type based field layout record
					if (record.RecordType == RecordType.GroupByFieldLayout)
						return false;

					// if we have group by then use the header placement in group by
					if (ti.FieldLayoutCache.HasGroupBySortFields)
					{
						#region HeaderPlacementInGroupBy
						// if the mode is on top of group by records then let it get added now
						if (ti.FieldLayoutCache.HeaderPlacementInGroupBy == HeaderPlacementInGroupBy.OnTopOnly)
						{
							// let it add it to the first record and we don't need another (unless we get a 
							// record break but we'll handle that in the table change)
							ti.NeedsHeaderRecord = false;
						}
						else
						{
							Debug.Assert(ti.FieldLayoutCache.HeaderPlacementInGroupBy == HeaderPlacementInGroupBy.WithDataRecords);

							if (record is GroupByRecord == false &&
								record.ParentRecord is GroupByRecord &&
								record.ParentCollection != null &&
								record.ParentCollection.GroupByField == null)
							{
								// within the first sibling of the data records
								ti.NeedsHeaderRecord = false;
							}
							else
							{
								// wait until the above criteria
								return false;
							}
						}
						#endregion //HeaderPlacementInGroupBy
					}
					else
					{
						// let it add it to the first record and we don't need another (unless we get a 
						// record break but we'll handle that in the table change)
						ti.NeedsHeaderRecord = false;
					}
				}
			}

			// note for headers we're going to leave them starting indented based on where the cells are. if 
			// we try to do what we do with summaries/groupby whereby there is just a border around the record 
			// and not between items (which would look weird in a groupby record with summaries within) then 
			// its harder to see which cells the labels line up with
			this.AddCellsRecord(record, ti, ti.DataRecordIndent, ti.FieldLayoutCache.HeaderRecordRowInfos);

			ti.HasAddedRootHeader = true;
			return true;
		}
		#endregion //AddHeaderRecord

		#region AddLeadingEmptyCells
		private TableBorderSides? AddLeadingEmptyCells(TableInfo ti, TableBorderSides? borderSides, WordTableCellSettings cellSettings, int emptyCellIndent)
		{
			var emptyCellProps = this.GetTempCellPropertiesWithBorders(cellSettings, emptyCellIndent);

			TableBorderSides rightSide = TableBorderSides.None;

			// if there is a right border remove it for the leading cell
			if (borderSides != null)
			{
				rightSide = (borderSides.Value & TableBorderSides.Right);
				borderSides = borderSides.Value & ~TableBorderSides.Right;

				var borderProps = emptyCellProps.BorderProperties;
				borderProps.Sides = borderSides.Value;
			}

			_writer.AddTableCell(string.Empty, emptyCellProps);

			// remove the left side since this placeholder would provide it and readd the right if we had
			if (borderSides != null)
				borderSides = (borderSides.Value & ~TableBorderSides.Left) | rightSide;
			return borderSides;
		}
		#endregion //AddLeadingEmptyCells

		#region AddSingleValueRecord
		private bool AddSingleValueRecord(TableInfo ti, int indent, int currentTableColumnCount, object value, TableBorderSides? borderSides, WordTableCellSettings cellSettings, bool isHeaderRow, bool ignoreEmptyValue)
		{
			return this.AddSingleValueRecord(ti, indent, currentTableColumnCount, value, borderSides, cellSettings, isHeaderRow, ignoreEmptyValue, 0);
		}

		private bool AddSingleValueRecord(TableInfo ti, int indent, int currentTableColumnCount, object value, TableBorderSides? borderSides, WordTableCellSettings cellSettings, bool isHeaderRow, bool ignoreEmptyValue, int emptyCellIndent)
		{
			if (ignoreEmptyValue)
			{
				if (value == null || (value is string && ((string)value).Length == 0))
					return false;
			}

			this.StartTableRow(null, ti, indent);

			// there should be some empty cells before for indenting
			if (emptyCellIndent > 0)
			{
				borderSides = AddLeadingEmptyCells(ti, borderSides, cellSettings, emptyCellIndent);
			}

			// single cell that spans the extent
			int columnSpan = currentTableColumnCount - (indent + emptyCellIndent);
			var cellProps = borderSides != null
				? this.GetTempCellPropertiesWithBorders(cellSettings, columnSpan)
				: this.GetTempCellProperties(cellSettings, columnSpan, TableCellVerticalMerge.None);

			if (borderSides != null)
			{
				var borderProps = cellProps.BorderProperties;
				borderProps.Sides = borderSides;
			}

			if (null != cellSettings)
				cellSettings.Initialize(_writer, cellProps);

			_writer.StartTableCell(cellProps);

			this.WriteCellValue(this.GetTempParagraphProperties(cellSettings, false), this.GetTempFont(cellSettings), value);

			_writer.EndTableCell();

			_writer.EndTableRow();

			return true;
		}
		#endregion //AddSingleValueRecord

		#region AddSummaryRecord
		private void AddSummaryRecord(TableInfo ti, int indent, Record record, IEnumerable<SummaryResult> results, 
			RecordCollectionBase parentRecords, FieldLayout fieldLayout, bool hasTopBorder, 
			WordTableCellSettings cellSettings, ReadOnlyCollection<RowInfo> rowInfos)
		{
			if (results == null)
				return;

			#region Setup

			object header = null;

			var table = GetFreeFormSummaries(results);
			bool hasFreeFormSummaries = table != null && table.Count > 0;

			Dictionary<Field, IList<SummaryResult>> cellSummaries = null;
			bool hasSummaryCells = false;
			
			if (record is SummaryRecord || ti.FieldLayoutCache.GroupBySummaryDisplayMode == GroupBySummaryDisplayMode.SummaryCellsAlwaysBelowDescription)
			{
				cellSummaries = GetFieldSummaryResults(results);

				if (null != cellSummaries && cellSummaries.Count > 0 && ti.FieldLayoutCache.VisibleInCellAreaCellCount > 0)
					hasSummaryCells = true;
			}

			#endregion //Setup

			// note when the top border is provided by the caller we won't include the bottom border. instead 
			// we'll just add it to the last section below
			TableBorderSides borderSides = TableBorderSides.LeftAndRight;

			if (!hasTopBorder)
				borderSides |= TableBorderSides.Top;

			// if there are summaries...
			if (ti.FieldLayoutCache.SummaryDescriptionVisibilityResolved == Visibility.Visible)
			{
				object mask = SummaryRecord.GetHeaderMaskResolved(parentRecords);
				header = SummaryRecord.ResolveSummaryRecordHeaderValue(parentRecords, fieldLayout, mask);

				// if there's a header then output that first
				if (null != header)
				{
					// if there is nothing after this then include the bottom too
					if (!hasFreeFormSummaries && !hasSummaryCells)
						borderSides |= TableBorderSides.Bottom;

					this.AddSingleValueRecord(ti, indent, ti.ColumnCount, header.ToString(), borderSides, cellSettings, false, false, ti.DataRecordIndent - indent);

					// remove the top border if its there
					borderSides &= ~TableBorderSides.Top;
				}
			}

			// then add any summary cell values
			if (hasSummaryCells)
			{
				// if there are no free form summaries then this will be the bottom
				if (!hasFreeFormSummaries)
					borderSides |= TableBorderSides.Bottom;

				this.AddCellsRecord(record, 
					ti, 
					indent,  
					rowInfos, 
					cellSummaries,
					ti.DataRecordIndent - indent,
					borderSides,
					cellSettings);

				// remove the top border if its there
				borderSides &= ~TableBorderSides.Top;
			}

			if (hasFreeFormSummaries)
			{
				// this part has to have a bottom border if its there
				borderSides |= TableBorderSides.Bottom;

				AddFreeFormSummaries(ti, indent, record, borderSides, table, cellSettings);
			}
		}
		#endregion //AddSummaryRecord

		#region ApplyRecordIndent


#region Infragistics Source Cleanup (Region)










































#endregion // Infragistics Source Cleanup (Region)

		#endregion //ApplyRecordIndent

		#region CalculateIndent
		private static int CalculateIndent(Record record, TableInfo ti)
		{
			int indent = 0;

			// if this is a child of a record in the same record manager
			// then we need to indent the record based on its nesting depth
			if (record.ParentRecord != null &&
				record.ParentRecord.RecordManager == ti.RecordManager)
			{
				// for datarecords we can just use the calculated indent
				if (record.RecordType == RecordType.DataRecord)
				{
					indent = ti.DataRecordIndent;
				}
				else
				{
					// for group by records, etc. we will calculate the indent
					// based on the nesting depth relative to the nesting 
					// depth of the parent row
					indent = record.NestingDepth;

					if (ti.RecordManager.NestingDepth > 0)
						indent -= ti.RecordManager.ParentRecord.NestingDepth;
				}
			}
			return indent;
		}
		#endregion //CalculateIndent

		#region ConvertUnits
		private float ConvertUnits(float value, UnitOfMeasurement fromUnit)
		{
			return WordDocumentWriter.ConvertUnits(value, fromUnit, _writer.Unit);
		}
		#endregion //ConvertUnits

		#region CreateTableInfo
		private TableInfo CreateTableInfo(Record record)
		{
			FieldLayout fl = record.FieldLayout;
			TableInfo ti = new TableInfo(record.RecordManager, fl, GetCache(fl));

			ti.CurrentExpandableFieldRecord = record as ExpandableFieldRecord;

			return ti;
		}
		#endregion //CreateTableInfo

		#region EndExport
		private void EndExport(RecordExportCancellationInfo cancelInfo)
		{
			if (this.GetFlag(InternalFlags.ExportEnded))
				return;

			this.SetFlag(InternalFlags.ExportEnded, true);

			bool canceled = cancelInfo != null;

			try
			{
				// close any remaining open tables
				while (_currentStack.Count > 0)
					this.EndTable(true);

				if (this.GetFlag(InternalFlags.HasRaisedBeginExport))
				{
					_owner.OnExportEnding(new ExportEndingEventArgs(_writer, _source, _exportControl, cancelInfo));
				}
			}
			finally
			{
				_owner.RemoveExporter(this);
			}

			// if we called startdocument then we can end the writer
			if (this.GetFlag(InternalFlags.StartedDocument))
				_writer.EndDocument(true);

			// if this is async then we may need to close the stream
			if (this.GetFlag(InternalFlags.CloseStreamOnEnd))
			{
				_exportStream.Flush();
				_exportStream.Close();
			}

			_owner.OnExportEnded(new ExportEndedEventArgs(_source, cancelInfo));
		} 
		#endregion //EndExport

		#region EndTable
		/// <summary>
		/// Ends the current table and pops it off the stack and returns it.
		/// </summary>
		/// <param name="isCancellingExport">True if the export is being canceled.</param>
		/// <returns>The TableInfo that was on top of the stack</returns>
		private TableInfo EndTable(bool isCancellingExport)
		{
			if (_currentStack.Count > 0)
			{
				bool isNested = _currentStack.Count > 1;
				TableInfo ti = _currentStack.Pop();

				this.EndNestedExpandableFieldRecordTable(ti, isCancellingExport);

				// end the table. a paragraph is needed in the cell for a nested table
				_writer.EndTable();

				// if the table itself is nested then we need to end the containing cell and row
				if (isNested)
				{
					_writer.EndTableCell();
					_writer.EndTableRow();
				}

				return ti;
			}

			return null;
		}
		#endregion //EndTable

		#region EndNestedExpandableFieldRecordTable
		private void EndNestedExpandableFieldRecordTable(TableInfo ti, bool isCancellingExport)
		{
			if (ti.HasCreatedExpandableFieldRecordTable)
			{
				ti.HasCreatedExpandableFieldRecordTable = false;

				// end the table created for the expandable field record as well as the cell/row containing it
				_writer.EndTable();
				_writer.EndTableCell();
				_writer.EndTableRow();
			}

			ti.CurrentExpandableFieldRecord = null;
		}
		#endregion //EndNestedExpandableFieldRecordTable

		#region GetCache
		private FieldLayoutCache GetCache(FieldLayout fieldLayout)
		{
			FieldLayoutCache cache;

			if (!_fieldLayoutCacheTable.TryGetValue(fieldLayout, out cache))
			{
				Debug.Assert(this.GetFlag(InternalFlags.HasRaisedBeginExport), "We're creating the cache before we finished raising the BeginExport?");

				cache = new FieldLayoutCache(fieldLayout, this);
				_fieldLayoutCacheTable[fieldLayout] = cache;
			}

			return cache;
		}
		#endregion //GetCache

		#region GetFieldSummaryResults
		private static Dictionary<Field, IList<SummaryResult>> GetFieldSummaryResults(IEnumerable<SummaryResult> summaryResults)
		{
			Dictionary<Field, IList<SummaryResult>> table = new Dictionary<Field, IList<SummaryResult>>();

			foreach (SummaryResult result in summaryResults)
			{
				// skip freeform ones
				if (result.PositionResolved != SummaryPosition.UseSummaryPositionField)
					continue;

				Field field = result.PositionFieldResolved;

				// this shouldn't happen but skip any not associated with fields
				if (field == null)
					continue;

				IList<SummaryResult> results;

				if (!table.TryGetValue(field, out results))
					table[field] = results = new List<SummaryResult>();

				results.Add(result);
			}

			return table;
		}
		#endregion //GetFieldSummaryResults

		#region GetFlag
		private bool GetFlag(InternalFlags flag)
		{
			return (flag & _flags) == flag;
		}
		#endregion //GetFlag

		#region GetFreeFormSummaries
		private static Dictionary<SummaryPosition, IList<SummaryResult>> GetFreeFormSummaries(IEnumerable<SummaryResult> summaryResults)
		{
			Dictionary<SummaryPosition, IList<SummaryResult>> table = new Dictionary<SummaryPosition, IList<SummaryResult>>();

			foreach (SummaryResult result in summaryResults)
			{
				SummaryPosition position = result.PositionResolved;

				switch (position)
				{
					case SummaryPosition.Left:
					case SummaryPosition.Center:
					case SummaryPosition.Right:
						break;
					default:
						Debug.Assert(position == SummaryPosition.UseSummaryPositionField, "Unrecognized position");
						continue;
				}

				IList<SummaryResult> results;

				if (!table.TryGetValue(position, out results))
					table[position] = results = new List<SummaryResult>();

				results.Add(result);
			}

			return table;
		}
		#endregion //GetFreeFormSummaries

		#region GetSummaryText
		private static string GetSummaryText(ref StringBuilder sb, IList<SummaryResult> results)
		{
			if (results == null || results.Count == 0)
				return null;

			if (sb == null)
				sb = new StringBuilder();
			else
				sb.Length = 0;

			foreach (SummaryResult result in results)
			{
				string temp = result.DisplayText;

				if (string.IsNullOrEmpty(temp))
					continue;

				if (sb.Length > 0)
					sb.Append(Environment.NewLine);

				sb.Append(temp);
			}

			return sb.ToString();
		}
		#endregion //GetSummaryText

		#region GetTempBorderProperties
		private TableBorderProperties GetTempBorderProperties(TableBorderSides borderSides)
		{
			if (_tempBorderProperties == null)
			{
				_tempBorderProperties = _writer.CreateTableBorderProperties();
			}
			else
			{
				_tempBorderProperties.Reset();
			}

			_tempBorderProperties.Sides = borderSides;
			return _tempBorderProperties;
		}
		#endregion //GetTempBorderProperties

		#region GetTempCellProperties
		private TableCellProperties GetTempCellProperties(WordTableCellSettings cellSettings, int columnSpan, TableCellVerticalMerge verticalMerge)
		{
			if (cellSettings != null || columnSpan != 1 || verticalMerge != TableCellVerticalMerge.None)
			{
				if (_tempCellProperties == null)
					_tempCellProperties = _writer.CreateTableCellProperties();
				else
				{
					_tempCellProperties.Reset();
				}

				InitializeTableCellProperties(_tempCellProperties, _writer, cellSettings, columnSpan, verticalMerge);

				return _tempCellProperties;
			}

			return null;
		}
		#endregion //GetTempCellProperties

		#region GetTempCellPropertiesWithBorders
		private TableCellProperties GetTempCellPropertiesWithBorders(WordTableCellSettings cellSettings, int columnSpan)
		{
			if (_tempCellPropertiesWithBorders == null)
			{
				_tempCellPropertiesWithBorders = _writer.CreateTableCellProperties();
				_tempCellPropertiesWithBorders.BorderProperties.Sides = TableBorderSides.All;
			}
			else
			{
				_tempCellPropertiesWithBorders.Reset();
			}

			InitializeTableCellProperties(_tempCellPropertiesWithBorders, _writer, cellSettings, columnSpan, TableCellVerticalMerge.None);

			return _tempCellPropertiesWithBorders;
		}
		#endregion //GetTempCellPropertiesWithBorders

		#region GetTempParagraphProperties
		private ParagraphProperties GetTempParagraphProperties(WordTableCellSettings cellSettings, bool createIfNull)
		{
			if (createIfNull || (cellSettings != null && cellSettings.HasParagraphSettings()))
			{
				if (_tempParagraphProperties == null)
					_tempParagraphProperties = _writer.CreateParagraphProperties();
				else
					_tempParagraphProperties.Reset();

				if (cellSettings != null)
					cellSettings.Initialize(_writer, _tempParagraphProperties);

				return _tempParagraphProperties;
			}

			return null;
		}
		#endregion //GetTempParagraphProperties

		#region GetTempFont
		private WordFont GetTempFont(WordTableCellSettings cellSettings)
		{
			if (cellSettings != null && cellSettings.HasFontSettings())
			{
				if (_tempFont == null)
					_tempFont = _writer.CreateFont();
				else
					_tempFont.Reset();

				cellSettings.Initialize(_writer, _tempFont);
				return _tempFont;
			}

			return null;
		}
		#endregion //GetTempFont

		#region InitializeCreatedDocument
		private void InitializeCreatedDocument()
		{
			#region DefaultFont

			var defaultFontSettings = _owner.DefaultFontSettings;
			var font = _writer.DefaultFont;

			if (null != font && null != defaultFontSettings)
				defaultFontSettings.Initialize(_writer, font);

			#endregion //DefaultFont

			#region DefaultTableProperties

			var defaultTableSettings = _owner.DefaultTableSettings;
			var tableProps = _writer.DefaultTableProperties;

			if (null != tableProps && null != defaultTableSettings)
				defaultTableSettings.Initialize(_writer, tableProps, TableLayout.Auto);

			#endregion //DefaultTableProperties

			#region DefaultParagraphProperties

			var defaultParaSettings = _owner.DefaultParagraphSettings;
			var paraProps = _writer.DefaultParagraphProperties;

			if (null != paraProps && null != defaultParaSettings)
				defaultParaSettings.Initialize(_writer, paraProps);

			#endregion //DefaultParagraphProperties

			#region FinalSectionProperties

			var defaultSectionSettings = _owner.FinalSectionSettings;
			var finalSection = _writer.FinalSectionProperties;

			if (null != defaultSectionSettings && null != finalSection)
				defaultSectionSettings.Initialize(_writer, finalSection);

			#endregion //FinalSectionProperties

			#region DocumentProperties

			var docSettings = _owner.DocumentSettings;
			var docProps = _writer.DocumentProperties;

			if (null != docProps && null != docSettings)
				docSettings.Initialize(_writer, docProps);

			#endregion //DocumentProperties
		}
		#endregion //InitializeCreatedDocument

		#region InitializeTableCellProperties
		private static void InitializeTableCellProperties(TableCellProperties props, WordDocumentWriter writer, WordTableCellSettings cellSettings, int columnSpan, TableCellVerticalMerge verticalMerge)
		{
			props.ColumnSpan = columnSpan;
			props.VerticalMerge = verticalMerge;

			if (null != cellSettings)
				cellSettings.Initialize(writer, props);
		}
		#endregion //InitializeTableCellProperties

		#region Merge
		private static void Merge(ref WordTableCellSettings formatSettings, FieldLayout fieldLayout, DependencyProperty fieldSettingsProperty)
		{
			Debug.Assert(fieldLayout != null && fieldLayout.DataPresenter != null);

			Merge(ref formatSettings,
				fieldSettingsProperty,
				fieldLayout.FieldSettingsIfAllocated,
				fieldLayout.DataPresenter.FieldSettingsIfAllocated
				);
		}

		private static void Merge(ref WordTableCellSettings formatSettings, DependencyProperty fieldSettingsProperty, params DependencyObject[] sourceObjects)
		{
			WordTableCellSettings[] formatSettingList = new WordTableCellSettings[sourceObjects.Length];

			for (int i = 0; i < formatSettingList.Length; i++)
				formatSettingList[i] = sourceObjects[i] != null ? sourceObjects[i].GetValue(fieldSettingsProperty) as WordTableCellSettings : null;

			Merge(ref formatSettings, formatSettingList);
		}

		private static void Merge(ref WordTableCellSettings formatSettings, Field field, DependencyProperty fieldSettingsProperty)
		{
			Debug.Assert(field != null && field.Owner != null && field.DataPresenter != null);

			Merge(ref formatSettings,
				fieldSettingsProperty,
				field.SettingsIfAllocated,
				field.Owner.FieldSettingsIfAllocated,
				field.DataPresenter.FieldSettingsIfAllocated
				);
		}
		#endregion //Merge

		#region MergeHelper
		private static void MergeHelper(DependencyProperty property, DependencyObject source, DependencyObject target)
		{
			object sourceValue = source.GetValue(property);

			// for some objects we need to continue cloning/merging into the object's properties...
			if (sourceValue is WordBorderSettings)
				MergeNestedObject(property, sourceValue as WordBorderSettings, target);
			else if (sourceValue is WordFontSettings)
				MergeNestedObject(property, sourceValue as WordFontSettings, target);
			else
			{
				// skip anything we've copied over already
				if (Utilities.ShouldSerialize(property, target))
					return;

				target.SetValue(property, sourceValue);
			}
		}
		#endregion //MergeHelper

		#region MergeNestedObject
		private static void MergeNestedObject<T>(DependencyProperty property, T sourceValue, DependencyObject target)
			where T : DependencyObject, new()
		{
			Debug.Assert(sourceValue != null, "Was this a bad cast?");

			if (sourceValue == null)
				return;

			var targetValue = target.GetValue(property) as T;
			bool hadTargetValue = targetValue != null;

			Merge<T>(ref targetValue, sourceValue);

			if (!hadTargetValue && targetValue != null)
				target.SetValue(property, targetValue);
			return;
		}
		#endregion //MergeNestedObject

		#region OnAddingNestedTable
		private void OnAddingNestedTable(TableInfo ti, Record nestedRecord)
		{
			// if we already broke this table for another island and haven't processed 
			// a record for this table then we can skip this routine because we will have 
			// already set the NeedsHeaderRecord or have added the header
			if (ti.HasChildTableBreak)
				return;

			// keep track of whether the record processing has been "broken" for this table
			ti.HasChildTableBreak = true;

			// if we may need a separate header see if we need to add it now or set up 
			// the table so it will add it if needed with the next break
			if (ti.FieldLayoutCache.LabelLocation == LabelLocation.SeparateHeader)
			{
				if (ti.HasAddedRootHeader)
				{
					// if we've added the root header then we only need to try and add one 
					// if the field layout supports headers on breaks
					if (ti.FieldLayoutCache.HeaderPlacement != HeaderPlacement.OnRecordBreak)
						return;
				}

				// get the ancestor record to hand into the add header routine
				Record ancestor = nestedRecord;

				while (ancestor != null && ancestor.RecordManager != ti.RecordManager)
					ancestor = ancestor.ParentDataRecord;

				Debug.Assert(ancestor != null, "Unable to find ancestor record?");

				if (ti.HasAddedRootHeader)
				{
					ti.NeedsHeaderRecord = true;
				}

				// before we insert/start the nested table, let the current item see if it 
				// needs to insert its header above the nested children
				this.AddHeaderRecord(ancestor, CalculateIndent(ancestor, ti), ti, true);
			}
		}
		#endregion //OnAddingNestedTable

		#region OnExportControlFieldLayoutInitialized
		private void OnExportControlFieldLayoutInitialized(object sender, FieldLayoutInitializedEventArgs e)
		{
			var fl = e.FieldLayout;
			var fields = fl.Fields;
			
			for (int i = fields.Count - 1; i >= 0; i--)
			{
				Field f = fields[i];

				if (this.ShouldIgnoreField(f))
					ProcessIgnoredField(f);
			}
		}
		#endregion //OnExportControlFieldLayoutInitialized

		#region OnExportStarting
		private bool OnExportStarting()
		{
			Debug.Assert(!this.GetFlag(InternalFlags.ExportStarted), "An export operation has already been started");

			if (this.GetFlag(InternalFlags.ExportStarted))
				return false;

			this.SetFlag(InternalFlags.ExportStarted, true);
			_owner.AddExporter(this);
			return true;
		}
		#endregion //OnExportStarting

		#region ProcessIgnoredField
		private void ProcessIgnoredField(Field f)
		{
			FieldLayout fl = f.Owner;
			f.Visibility = Visibility.Collapsed;

			// remove the field from all collections
			RemoveAll(fl.SummaryDefinitions, f.Name, delegate(SummaryDefinition sd, string name) { return sd.SourceFieldName == name; });
			RemoveAll(fl.SortedFields, f, delegate(FieldSortDescription fsd, Field field) { return fsd.Field == field; });
			RemoveAll(fl.RecordFilters, f, delegate(RecordFilter rf, Field field) { return rf.Field == field; });
		}
		#endregion //ProcessIgnoredField

		#region RaiseCellExporting
		private object RaiseCellExporting(Field field, DataRecord record, ref WordTableCellSettings cellSettings)
		{
			
			object value = record.GetCellText(field);

			var args = new CellExportingEventArgs(field, record, value, cellSettings);
			_owner.OnCellExporting(args);
			value = args.Value;

			cellSettings = args.CellSettingsInternal;

			return value;
		}
		#endregion //RaiseCellExporting

		#region RaiseLabelExporting
		private object RaiseLabelExporting(Field field, Record record, ref WordTableCellSettings cellSettings)
		{
			object value = field.Label;

			var args = new LabelExportingEventArgs(field, record, value, cellSettings);
			_owner.OnLabelExporting(args);
			value = args.Value;

			cellSettings = args.CellSettingsInternal;

			return value;
		}
		#endregion //RaiseLabelExporting

		#region RaiseSummaryResultsExporting
		private object RaiseSummaryResultsExporting(Field field, Record record, IList<SummaryResult> results, SummaryPosition summaryPosition, ref WordTableCellSettings cellSettings)
		{
			object value = GetSummaryText(ref _tempStringBuilder, results);

			// if we get back null then there are no summaries so don't raise the event
			if (value != null)
			{
				var args = new SummaryResultsExportingEventArgs(results, summaryPosition, field, record, value, cellSettings);
				_owner.OnSummaryResultsExporting(args);
				value = args.Value;

				cellSettings = args.CellSettingsInternal;
			}

			return value;
		}
		#endregion //RaiseSummaryResultsExporting

		#region RemoveAll
		private static void RemoveAll<TItem, TParam>(IList<TItem> list, TParam parameter, PredicateEx<TItem, TParam> match)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (match(list[i], parameter))
					list.RemoveAt(i);
			}
		}
		#endregion //RemoveAll

		#region ResolveProperty
		private static T ResolveProperty<T>(DependencyProperty property, T defaultValue, T defaultResolvedValue, params DependencyObject[] sources)
		{
			T value = defaultValue;

			foreach (var source in sources)
			{
				if (source != null)
				{
					value = (T)source.GetValue(property);

					if (!EqualityComparer<T>.Default.Equals(value, defaultValue))
						return value;
				}
			}

			return defaultResolvedValue;
		}
		#endregion //ResolveProperty

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool value)
		{
			if (value)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion //SetFlag

		#region ShouldIgnoreField
		private bool ShouldIgnoreField(Field field)
		{
			return ResolveProperty<bool?>(DataPresenterWordWriter.IgnoreFieldProperty, null, false,
				field.SettingsIfAllocated,
				field.Owner.FieldSettingsIfAllocated,
				field.DataPresenter.FieldSettingsIfAllocated).Value;
		}
		#endregion //ShouldIgnoreField

		#region StartNestedExpandableFieldRecordTable
		private void StartNestedExpandableFieldRecordTable(TableInfo ti)
		{
			// if there is a header pending then we need to create a wrapping word table
			if (ti.IsExpandableFieldRecordTableNeeded)
			{
				ti.HasCreatedExpandableFieldRecordTable = true;

				this.StartTableRow(null, ti, ti.DataRecordIndent);

				StartNestedTableCell(ti.ColumnCount, ti.DataRecordIndent, ti.FieldLayoutCache.GetNestedTablePadding(_writer.Unit));

				var tableProps = ti.FieldLayoutCache.GetTableProperties(_writer);

				if (ti.CurrentExpandableFieldRecord.ChildRecordManager == null)
					tableProps.Layout = TableLayout.Auto;

				_writer.StartTable(1, tableProps);
			}
		}
		#endregion //StartNestedExpandableFieldRecordTable

		#region StartNestedTableCell
		private void StartNestedTableCell(int parentColumnCount, int dataRecordIndent, Padding? margins)
		{
			// create a cell that will contain the nested table
			var cellProps = _writer.CreateTableCellProperties();
			cellProps.ColumnSpan = parentColumnCount - dataRecordIndent;

			// the developer can control the amount of spacing around the child table. note
			// we're using the parent table's setting because the child records could be 
			// heterogeneous so we wouldn't know which field layout to use and even if we 
			// wanted to use the default fieldlayout of the childrecordmanager we don't have 
			// that for expandable field records that represents fields whose IsExpandable 
			// was explicitly set to true but are not collection fields so to be consistent 
			// we'll use the property on the parent field layout to control what happens 
			// to all child tables
			if (margins != null)
				cellProps.Margins = margins;

			var borderProps = cellProps.BorderProperties; // lazily allocated
			borderProps.Sides = TableBorderSides.None;
			borderProps.Width = this.ConvertUnits(0.5f, UnitOfMeasurement.Point);
			borderProps.Style = TableBorderStyle.None;

			_writer.StartTableCell(cellProps);
		}
		#endregion //StartNestedTableCell

		#region StartTable
		private TableInfo StartTable(Record record)
		{
			// if we're already setup for this then bail out
			if (_currentStack.Count > 0 && _currentStack.Peek().IsMatch(record, true))
				return _currentStack.Peek();

			return this.StartTableImpl(record);
		}

		private TableInfo StartTableImpl(Record record)
		{
			int originalCount = _currentStack.Count;
			int depth = record.RecordManager.NestingDepth;

			// if we're currently deeper than the record being processed then move back up
			for (int i = originalCount - 1; i > depth; i--)
				this.EndTable(false);

			List<TableInfo> infos = new List<TableInfo>();
			Record ancestor = record;

			#region Setup
			if (_currentStack.Count > 0)
			{
				// walk up the stack and look for a common ancestor
				for (int i = _currentStack.Count - 1; i >= 0; i--)
				{
					// if this is a match then just add to what we have so far
					// note we're ignoring whether the containing expandable field 
					// records match. we'll fix that up below
					if (_currentStack.Peek().IsMatch(ancestor, false))
						break;

					// if we've walked up to the point where we had 
					// a record manager for this depth and this wasn't
					// a match then we need to pop it off
					if (ancestor.RecordManager.NestingDepth == _currentStack.Count - 1)
						this.EndTable(false);

					infos.Add(this.CreateTableInfo(ancestor));

					// get the parent record for comparison in the next iteration
					ancestor = ancestor.RecordManager.ParentRecord ?? (Record)ancestor.ParentDataRecord;
				}
			}
			else
			{
				// there's nothing on the stack so add all the items to 
				// the list to push on the stack
				for (int i = 0; i <= depth; i++)
				{
					infos.Add(this.CreateTableInfo(ancestor));
					ancestor = ancestor.RecordManager.ParentRecord ?? (Record)ancestor.ParentDataRecord;
				}
			}
			#endregion //Setup

			if (infos.Count > 0)
			{
				for (int i = infos.Count - 1; i >= 0; i--)
				{
					TableInfo newTableInfo = infos[i];
					bool isNested = _currentStack.Count > 0;
					TableInfo parentTi = isNested ? _currentStack.Peek() : null;

					if (isNested)
					{
						ExpandableFieldRecord parentRecord = record.RecordManager.ParentRecord;

						// if this record is not a child of the current expandable field record
						// we are processing then we need to close that table or we don't have 
						// a parent record yet then we need to initialize it
						if (parentTi.CurrentExpandableFieldRecord != parentRecord)
						{
							this.EndNestedExpandableFieldRecordTable(parentTi, false);

							parentTi.CurrentExpandableFieldRecord = parentRecord;
						}

						// if the child records are before the parent then the parent header 
						// may be above the child records so try to add it now if needed
						this.OnAddingNestedTable(parentTi, record);

						// start a nested table if needed
						this.StartNestedExpandableFieldRecordTable(parentTi);

						int parentColumnCount = parentTi.ColumnCount;
						int parentDataRecordIndent = parentTi.DataRecordIndent;

						// if this table is nested within the parent then there is only 1 column
						if (parentTi.HasCreatedExpandableFieldRecordTable)
						{
							parentColumnCount = 1;
							parentDataRecordIndent = 0;
						}

						this.StartTableRow(null, parentTi, parentDataRecordIndent);

						// create a cell that will contain the nested table
						Padding? margin = !parentTi.HasCreatedExpandableFieldRecordTable
							? parentTi.FieldLayoutCache.GetNestedTablePadding(_writer.Unit)
							: new Padding();
						StartNestedTableCell(parentColumnCount, parentDataRecordIndent, margin);
					}
					else if (originalCount > 0)
					{
						// this new table is not nested but it has a sibling table. if we don't 
						// separate it then word may join the tables.
						_writer.AddEmptyParagraph();
					}

					_currentStack.Push(newTableInfo);

					TableProperties tableProps = newTableInfo.FieldLayoutCache.GetTableProperties(_writer);

					_writer.StartTable(newTableInfo.GetColumnWidths(_writer.Unit), tableProps);
				}
			}
			else
			{
				TableInfo ti = _currentStack.Peek();
				Debug.Assert(ti.IsMatch(record, false), "No new infos but not a match?");

				if (ti.CurrentExpandableFieldRecord != record)
				{
					if (ti.CurrentExpandableFieldRecord != null)
						this.EndNestedExpandableFieldRecordTable(ti, false);

					ti.CurrentExpandableFieldRecord = record as ExpandableFieldRecord;
				}

				// we won't start the table though even if the specified record is an expandable field record 
				// in case the caller will override the creation of the table (e.g. if the record is skipped)
			}

			return _currentStack.Peek();
		}
		#endregion //StartTable

		#region StartTableRow
		private void StartTableRow(TableRowProperties rowProps, TableInfo ti, int recordIndent)
		{

			if (recordIndent > 0)
			{
				if (rowProps == null)
					rowProps = _writer.CreateTableRowProperties();

				rowProps.CellsBefore = recordIndent;
			}


			_writer.StartTableRow(rowProps);






		} 
		#endregion //StartTableRow

		#region WriteCellValue
		private void WriteCellValue(ParagraphProperties paraProps, WordFont font, object value)
		{
			if (value != null)
			{
				_writer.StartParagraph(paraProps);
				_writer.AddTextRun(value.ToString(), font);
				_writer.EndParagraph();
			}
			else
			{
				_writer.AddEmptyParagraph();
			}
		}
		#endregion //WriteCellValue

		#endregion //Private Methods

		#endregion //Methods

		#region Delegates

		#region PredicateEx
		private delegate bool PredicateEx<TItem, TParam>(TItem item, TParam parameter);
		#endregion //PredicateEx

		#region ResolveCellSettingsCallback
		private delegate WordTableCellSettings ResolveCellSettingsCallback(RecordType recordType, Field field); 
		#endregion //ResolveCellSettingsCallback

		#endregion //Delegates

		#region IDataPresenterExporter Members

		#region BeginExport
		void IDataPresenterExporter.BeginExport(DataPresenterBase dataPresenter, IExportOptions exportOptions)
		{
			_exportControl = dataPresenter;

			// note i'm hooking the event before calling beginexport so that I can initialize 
			// the field layout (based on the IgnoreField property) before the developer gets 
			// the event so the field layout is in the final state for the export
			_exportControl.FieldLayoutInitialized += new EventHandler<FieldLayoutInitializedEventArgs>(OnExportControlFieldLayoutInitialized);

			Debug.Assert(exportOptions == this, "Different options were passed back to the begin export?");

			if (_writer == null)
			{
				_writer = WordDocumentWriter.Create(_exportStream);
				this.SetFlag(InternalFlags.CreatedWriter, true);
			}

			if (this.GetFlag(InternalFlags.CreatedWriter))
			{
				this.InitializeCreatedDocument();

				_writer.StartDocument();
				this.SetFlag(InternalFlags.StartedDocument, true);
			}

			_owner.OnExportStarted(new ExportStartedEventArgs(_writer, _source, _exportControl));

			this.SetFlag(InternalFlags.HasRaisedBeginExport, true);
		}
		#endregion //BeginExport

		#region EndExport
		void IDataPresenterExporter.EndExport(bool canceled)
		{
			RecordExportCancellationInfo cancelInfo = null;

			if (canceled)
				cancelInfo = new RecordExportCancellationInfo(RecordExportCancellationReason.Unknown, null);

			this.EndExport(cancelInfo);
		} 
		#endregion //EndExport

		#region InitializeRecord
		bool IDataPresenterExporter.InitializeRecord(Record record, ProcessRecordParams processRecordParams)
		{
			var cache = this.GetCache(record.FieldLayout);

			// if there are no visible fields whatsoever then we'll
			// automatically skip the record and the descendants
			if (cache.VisibleFieldCount == 0)
			{
				processRecordParams.SkipDescendants = true;
				return false;
			}

			var args = new InitializeRecordEventArgs(record, processRecordParams);

			// follow the behavior in the excel exporter and initialize the properties 
			// such that a record and its children will be excluded if the record is hidden
			if (record.VisibilityResolved == Visibility.Collapsed)
			{
				args.SkipDescendants = true;
				args.SkipRecord = true;
			}
			else if (!record.IsExpanded)
			{
				args.SkipDescendants = true;
			}

			// if there are no visible cells then skip the record by default too
			if (record is DataRecord && cache.VisibleInCellAreaCellCount == 0)
			{
				// really the only reason to still raise the event is to give 
				// the option of skipping the descendants
				args.SkipRecord = true;
			}

			_owner.OnInitializeRecord(args);

			// if this is an expandable field record where we still want to include the 
			// descendants then don't create the nesting table
			if (args.SkipRecord && !args.SkipDescendants && !args.TerminateExport)
			{
				ExpandableFieldRecord expandableFieldRecord = record as ExpandableFieldRecord;

				// note we only need to do this when it would have children. if it just represents 
				// a field whose isExpandable is true then we would skip the record anyway and 
				// don't need the nested table
				if (null != expandableFieldRecord && expandableFieldRecord.ChildRecordManager != null)
				{
					TableInfo ti = this.StartTableImpl(expandableFieldRecord);
					ti.IsExpandableFieldRecordHeaderPending = false;
				}
			}

			// if the params indicate to terminate or skip the record or if the record is collapsed then return false
			if (args.TerminateExport ||
				args.SkipRecord ||
				record.VisibilityResolved == Visibility.Collapsed)
			{
				return false;
			}
			else if (record is DataRecord && cache.VisibleInCellAreaCellCount == 0)
			{
				return false;
			}

			// otherwise return true to allow the record to be processed
			return true;
		} 
		#endregion //InitializeRecord

		#region OnObjectCloned
		void IDataPresenterExporter.OnObjectCloned(DependencyObject source, DependencyObject target)
		{
		} 
		#endregion //OnObjectCloned

		#region ProcessRecord
		void IDataPresenterExporter.ProcessRecord(Record record, ProcessRecordParams processRecordParams)
		{
			// make sure a table is started at the appropriate level for the record being processed
			TableInfo ti = this.StartTable(record);

			// if the headers are to be within the data record then we want to set the needs 
			// header if we go to another group by record child island
			if (ti.FieldLayoutCache.LabelLocation == LabelLocation.SeparateHeader &&
				ti.FieldLayoutCache.HeaderPlacementInGroupBy == HeaderPlacementInGroupBy.WithDataRecords)
			{
				if (record is GroupByRecord)
				{
					ti.PreviousDataRecordParentCollection = null;
				}
				else if (record.ParentRecord is GroupByRecord)
				{
					var parentCollection = record.ParentCollection;

					if (ti.PreviousDataRecordParentCollection != parentCollection)
					{
						ti.NeedsHeaderRecord = true;
						ti.PreviousDataRecordParentCollection = parentCollection;
					}
				}
			}

			Debug.Assert(null != ti, "There is no table for the specified record?");

			// clear the child table break so we will add another separated header if needed
			if (record is ExpandableFieldRecord == false)
				ti.HasChildTableBreak = false;

			// calculate the indent for the data of this record
			int indent = CalculateIndent(record, ti);

			// add a header record above this record if needed
			this.AddHeaderRecord(record, indent, ti, false);

			switch (record.RecordType)
			{
				case RecordType.ExpandableFieldRecord:
					{
						ExpandableFieldRecord efr = record as ExpandableFieldRecord;
						this.AddExpandableFieldRecord(efr, ti);
						break;
					}
				case RecordType.DataRecord:
					{
						this.AddCellsRecord(record, ti, indent, ti.FieldLayoutCache.DataRecordRowInfos);
						break;
					}
				case RecordType.GroupByField:
				case RecordType.GroupByFieldLayout:
					{
						GroupByRecord gbr = record as GroupByRecord;

						// get the summaries so we can understand how the border need to be
						IEnumerable<SummaryResult> results = gbr.GetSummaryResults();
						bool hasSummaries = results != null && Utilities.HasItems(results);
						string description = gbr.DescriptionWithSummaries;
						bool hasDescription = !string.IsNullOrEmpty(description);
						bool needsTopBorder = !hasDescription;

						WordTableCellSettings cellSettings = gbr.GroupByField != null
							? ti.FieldLayoutCache.GroupByRecordCellSettingsCache[gbr.GroupByField]
							: ti.FieldLayoutCache.GroupByRecordCellSettingsCache.FieldLayoutCellCache;

						if (hasDescription || !hasSummaries)
						{
							// if we have summaries then it will provide the bottom border
							TableBorderSides? descriptionBorders = hasSummaries
								? TableBorderSides.Left | TableBorderSides.Top | TableBorderSides.Right
								: (TableBorderSides?)null;

							// the group by description is completely left aligned
							if (!this.AddSingleValueRecord(ti, indent, ti.ColumnCount, gbr.DescriptionWithSummaries, descriptionBorders, cellSettings, false, hasSummaries))
							{
								needsTopBorder = true;
							}
						}

						// add the summary info
						if (hasSummaries)
						{
							this.AddSummaryRecord(ti, indent, record, results, gbr.ChildRecords, gbr.FieldLayout, !needsTopBorder, 
								cellSettings, ti.FieldLayoutCache.GetGroupByRowInfos(gbr));
						}
						break;
					}
				case RecordType.SummaryRecord:
					{
						SummaryRecord summary = record as SummaryRecord;
						IEnumerable<SummaryResult> results = summary.SummaryResults;

						if (results != null && Utilities.HasItems(results))
						{
							this.AddSummaryRecord(ti, indent, record, results, summary.ParentCollection, summary.FieldLayout, false,
								ti.FieldLayoutCache.SummaryRecordCellSettingsCache.FieldLayoutCellCache, ti.FieldLayoutCache.SummaryRecordRowInfos);
						}
						break;
					}
				default:
					Debug.Fail("Unexpected record type:" + record.RecordType.ToString());
					return;
			}
		} 
		#endregion //ProcessRecord

		#endregion //IDataPresenterExporter Members

		#region IDataPresenterExporterAsync Members

		void IDataPresenterExporterAsync.CancelExport(RecordExportCancellationInfo cancelInfo)
		{
			this.EndExport(cancelInfo);
		}

		ExportInfo IDataPresenterExporterAsync.ExportInfo
		{
			get { return _exportInfo; }
		}

		#endregion //IDataPresenterExporterAsync Members

		#region IExportOptions Members

		bool IExportOptions.ExcludeExpandedState
		{
			get { return this.GetFlag(InternalFlags.ExcludeExpandedState); }
		}

		bool IExportOptions.ExcludeFieldLayoutSettings
		{
			get { return this.GetFlag(InternalFlags.ExcludeFieldLayoutSettings); }
		}

		bool IExportOptions.ExcludeFieldSettings
		{
			get { return this.GetFlag(InternalFlags.ExcludeFieldSettings); }
		}

		bool IExportOptions.ExcludeGroupBySettings
		{
			get { return this.GetFlag(InternalFlags.ExcludeGroupBySettings); }
		}

		bool IExportOptions.ExcludeRecordFilters
		{
			get { return this.GetFlag(InternalFlags.ExcludeRecordFilters); }
		}

		bool IExportOptions.ExcludeRecordVisibility
		{
			get { return this.GetFlag(InternalFlags.ExcludeRecordVisibility); }
		}

		bool IExportOptions.ExcludeSortOrder
		{
			get { return this.GetFlag(InternalFlags.ExcludeSortOrder); }
		}

		bool IExportOptions.ExcludeSummaries
		{
			get { return this.GetFlag(InternalFlags.ExcludeSummaries); }
		}
		#endregion //IExportOptions members

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags
		{
			ExcludeExpandedState = 1 << 0,
			ExcludeFieldLayoutSettings = 1 << 1,
			ExcludeFieldSettings = 1 << 2,
			ExcludeGroupBySettings = 1 << 3,
			ExcludeRecordFilters = 1 << 4,
			ExcludeRecordVisibility = 1 << 5,
			ExcludeSortOrder = 1 << 6,
			ExcludeSummaries = 1 << 7,
			CreatedWriter = 1 << 8,
			HasRaisedBeginExport = 1 << 9,
			CloseStreamOnEnd = 1 << 10,
			StartedDocument = 1 << 11,
			ExportStarted = 1 << 12,
			ExportEnded = 1 << 13,
		}
		#endregion //InternalFlags enum

		#region CellSettingsCache class
		private class CellSettingsCache
		{
			#region Member Variables

			internal readonly WordTableCellSettings FieldLayoutCellCache;
			private RecordType _recordType;
			private Dictionary<Field, WordTableCellSettings> _fieldCellCache;
			private ResolveCellSettingsCallback _resolveCallback;

			#endregion //Member Variables

			#region Constructor
			internal CellSettingsCache(RecordType recordType, WordTableCellSettings fieldLayoutCache, ResolveCellSettingsCallback resolveCallback)
			{
				Utilities.ValidateNotNull(resolveCallback);
				Utilities.ValidateNotNull(fieldLayoutCache);

				_fieldCellCache = new Dictionary<Field, WordTableCellSettings>();
				_resolveCallback = resolveCallback;
				_recordType = recordType;
				this.FieldLayoutCellCache = fieldLayoutCache;
			} 
			#endregion //Constructor

			#region Properties

			#region Indexer
			public WordTableCellSettings this[Field field]
			{
				get
				{
					WordTableCellSettings settings;

					if (!_fieldCellCache.TryGetValue(field, out settings))
					{
						_fieldCellCache[field] = settings = _resolveCallback(this._recordType, field);
					}

					return settings;
				}
			}
			#endregion //Indexer

			#region IsGroupByCache
			internal bool IsGroupByCache
			{
				get { return _recordType == RecordType.GroupByField || _recordType == RecordType.GroupByFieldLayout; }
			} 
			#endregion //IsGroupByCache

			#endregion //Properties
		} 
		#endregion //CellSettingsCache class

		#region FieldLayoutCache class
		private class FieldLayoutCache
		{
			#region Member Variables

			internal const int MaxColumnCount = 63;
			internal const float MaxPageWidthInches = 22f;
			internal const float OnlyExpandableFieldColumnWidthInches = 96f * 4; // 4"

			internal readonly FieldLayout FieldLayout;

			internal readonly ExportLayoutInformation ValueLayoutInfo;
			internal readonly ExportLayoutInformation HeaderLayoutInfo;

			internal readonly LabelLocation LabelLocation;
			internal readonly GroupBySummaryDisplayMode GroupBySummaryDisplayMode;
			internal readonly Visibility SummaryDescriptionVisibilityResolved;
			internal readonly HeaderPlacement HeaderPlacement;
			internal readonly HeaderPlacementInGroupBy HeaderPlacementInGroupBy;
			internal readonly ChildRecordsDisplayOrder ChildRecordsDisplayOrder;
			internal readonly bool HasGroupBySortFields;
			internal readonly double RecordSelectorExtent;
			internal readonly Size ExpansionIndicatorSize;
			internal readonly int VisibleFieldCount;
			internal readonly int VisibleInCellAreaCellCount;
			internal readonly int VisibleExpandableCellCount;

			internal readonly ReadOnlyCollection<RowInfo> DataRecordRowInfos;
			internal readonly ReadOnlyCollection<RowInfo> HeaderRecordRowInfos;
			internal readonly ReadOnlyCollection<RowInfo> SummaryRecordRowInfos;

			private ReadOnlyCollection<RowInfo> _groupByRowInfosForFieldLayout;
			private Dictionary<Field, ReadOnlyCollection<RowInfo>> _groupByRowInfosForFieldTable;
			private Dictionary<Field, bool> _expandableFieldHeaderVisibility;
			private Padding? _nestedTablePadding;

			private readonly WordTableSettings FlatTableSettings;
			private readonly WordDocumentWriter Writer;
			private TableProperties _cachedTableProperties;

			internal readonly CellSettingsCache DataRecordCellSettingsCache;
			internal readonly CellSettingsCache HeaderRecordCellSettingsCache;
			internal readonly CellSettingsCache SummaryRecordCellSettingsCache;
			internal readonly CellSettingsCache GroupByRecordCellSettingsCache;
			internal readonly CellSettingsCache ExpandableFieldRecordCellSettingsCache;

			// cache information for getting column widths
			private List<float> _cachedColumnWidths;
			private bool _columnWidthsHasExpandableField;
			private int _columnWidthsDataRecordIndent;

			#endregion //Member Variables

			#region Constructor
			internal FieldLayoutCache(FieldLayout fieldLayout, WordExporter exporter)
			{
				this.Writer = exporter._writer;
				this.FieldLayout = fieldLayout;

				WordTableSettings tableSettings = null;
				WordExporter.Merge(ref tableSettings,
					DataPresenterWordWriter.GetTableSettingsForFieldLayout(fieldLayout.Settings),
					DataPresenterWordWriter.GetTableSettingsForFieldLayout(fieldLayout.DataPresenter.FieldLayoutSettings));
				this.FlatTableSettings = tableSettings ?? new WordTableSettings();

				// cached field layout resolved state
				this.LabelLocation = fieldLayout.LabelLocationResolved;
				this.GroupBySummaryDisplayMode = fieldLayout.GroupBySummaryDisplayModeResolved;
				this.SummaryDescriptionVisibilityResolved = fieldLayout.SummaryDescriptionVisibilityResolved;
				this.HeaderPlacementInGroupBy = fieldLayout.HeaderPlacementInGroupByResolved;
				this.HeaderPlacement = fieldLayout.HeaderPlacementResolved;
				this.ChildRecordsDisplayOrder = fieldLayout.ChildRecordsDisplayOrderResolved;
				this.HasGroupBySortFields = fieldLayout.HasGroupBySortFields;

				this.GroupByRecordCellSettingsCache = this.CreateCellSettingsCache(RecordType.GroupByField);
				this.SummaryRecordCellSettingsCache = this.CreateCellSettingsCache(RecordType.SummaryRecord);
				this.HeaderRecordCellSettingsCache = this.CreateCellSettingsCache(RecordType.HeaderRecord);
				this.ExpandableFieldRecordCellSettingsCache = this.CreateCellSettingsCache(RecordType.ExpandableFieldRecord);
				this.DataRecordCellSettingsCache = this.CreateCellSettingsCache(RecordType.DataRecord);

				this.ValueLayoutInfo = new ExportLayoutInformation(fieldLayout, false);
				this.HeaderLayoutInfo = new ExportLayoutInformation(fieldLayout, true);

				this.DataRecordRowInfos = new ReadOnlyCollection<RowInfo>(CreateRowInfos(fieldLayout, this.ValueLayoutInfo));
				this.HeaderRecordRowInfos = new ReadOnlyCollection<RowInfo>(CreateRowInfos(fieldLayout, this.HeaderLayoutInfo));
				this.SummaryRecordRowInfos = new ReadOnlyCollection<RowInfo>(CreateRowInfos(fieldLayout, this.ValueLayoutInfo));

				this.InitializeRowInfos(this.DataRecordRowInfos, this.DataRecordCellSettingsCache, null);
				this.InitializeRowInfos(this.HeaderRecordRowInfos, this.HeaderRecordCellSettingsCache, null);
				this.InitializeRowInfos(this.SummaryRecordRowInfos, this.SummaryRecordCellSettingsCache, null);

				this.RecordSelectorExtent = fieldLayout.RecordSelectorExtentResolved;
				this.ExpansionIndicatorSize = fieldLayout.ExpansionIndicatorSize;

				int visibleFieldCount = 0;
				int visibleExpandableCellCount = 0;
				int visibleInCellAreaCellCount = 0;

				foreach (Field field in fieldLayout.Fields)
				{
					if (field.VisibilityResolved == Visibility.Visible)
					{
						visibleFieldCount++;

						if (field.IsExpandableResolved)
						{
							if (!field.IsExpandableByDefault)
								visibleExpandableCellCount++;
						}
						else
						{
							visibleInCellAreaCellCount++;
						}
					}
				}

				this.VisibleFieldCount = visibleFieldCount;
				this.VisibleExpandableCellCount = visibleExpandableCellCount;
				this.VisibleInCellAreaCellCount = visibleInCellAreaCellCount;
			}
			#endregion //Constructor

			#region Methods

			#region Private Methods

			#region AddRowInfoItem
			private static void AddRowInfoItem(List<RowInfoItem>[] rowDetails, FieldPosition fieldPos, Field field, RowInfoItemType itemType)
			{
				TableCellVerticalMerge merge = fieldPos.RowSpan > 1 ? TableCellVerticalMerge.Start : TableCellVerticalMerge.None;

				for (int i = fieldPos.Row, end = fieldPos.Row + fieldPos.RowSpan; i < end; i++)
				{
					List<RowInfoItem> row = rowDetails[i];

					if (row == null)
						row = rowDetails[i] = new List<RowInfoItem>();

					row.Add(new RowInfoItem(field, fieldPos.Column, fieldPos.ColumnSpan, fieldPos.RowSpan, merge, itemType));

					// if it spans multiple then we'll use continue for the remaining
					merge = TableCellVerticalMerge.Continue;
				}
			}
			#endregion //AddRowInfoItem

			#region CreateCellSettings
			private WordTableCellSettings CreateCellSettings(RecordType recordType, Field field)
			{
				Debug.Assert(field.Owner == this.FieldLayout, "Creating a settings for a different field layout?");
				var formatSettings = new WordTableCellSettings();
				DependencyProperty property = GetCellFormatProperty(recordType);

				if (null != property)
					WordExporter.Merge(ref formatSettings, field, property);

				InitializeDefaultCellSettings(formatSettings, recordType, field);

				return formatSettings;
			}

			private WordTableCellSettings CreateCellSettings(RecordType recordType)
			{
				var formatSettings = new WordTableCellSettings();
				DependencyProperty property = GetCellFormatProperty(recordType);

				if (null != property)
					WordExporter.Merge(ref formatSettings, this.FieldLayout, property);

				InitializeDefaultCellSettings(formatSettings, recordType, null);

				return formatSettings;
			} 
			#endregion //CreateCellSettings

			#region CreateCellSettingsCache
			private CellSettingsCache CreateCellSettingsCache(RecordType recordType)
			{
				var fieldLayoutSettings = this.CreateCellSettings(recordType);
				return new CellSettingsCache(recordType, fieldLayoutSettings, this.CreateCellSettings);
			}
			#endregion //CreateCellSettingsCache

			#region CreateRowInfos
			private static IList<RowInfo> CreateRowInfos(FieldLayout fieldLayout, ExportLayoutInformation layoutInfo)
			{
				List<RowInfo> rows = new List<RowInfo>();
				int rowCount = layoutInfo.GetRowHeights().Length;
				int columnCount = layoutInfo.GetColumnWidths().Length;
				List<RowInfoItem>[] rowDetails = new List<RowInfoItem>[rowCount];

				foreach (Field f in fieldLayout.Fields)
				{
					FieldPosition? fieldPos = layoutInfo.GetFieldPosition(f, true);

					if (null != fieldPos)
					{
						AddRowInfoItem(rowDetails, fieldPos.Value, f, RowInfoItemType.Label);
					}

					fieldPos = layoutInfo.GetFieldPosition(f, false);

					if (null != fieldPos)
					{
						AddRowInfoItem(rowDetails, fieldPos.Value, f, RowInfoItemType.Value);
					}
				}

				Comparison<RowInfoItem> comparison = delegate(RowInfoItem item1, RowInfoItem item2)
				{
					int result = item1.Column.CompareTo(item2.Column);

					if (result == 0)
						result = item2.ColumnSpan.CompareTo(item1.ColumnSpan);

					return result;
				};

				IComparer<RowInfoItem> comparer = Utilities.CreateComparer(comparison);

				// make sure the rows are filled in...
				for (int i = rowCount - 1; i >= 0; i--)
				{
					List<RowInfoItem> row = rowDetails[i];

					Debug.Assert(row != null && row.Count > 0);

					if (row == null)
						row = rowDetails[i] = new List<RowInfoItem>();

					// sort the rows
					Utilities.SortMergeGeneric(row, comparer);

					int columnPos = columnCount;

					for (int j = row.Count - 1; j >= 0; j--)
					{
						RowInfoItem item = row[j];
						int itemEnd = item.Column + item.ColumnSpan;

						if (itemEnd < columnPos)
							row.Insert(j + 1, new RowInfoItem(null, itemEnd, columnPos - itemEnd, 1, TableCellVerticalMerge.None, RowInfoItemType.EmptySpace));

						// switch the position the spot before this item
						columnPos = item.Column;
					}

					if (columnPos > 0)
						row.Insert(0, new RowInfoItem(null, 0, columnPos, 1, TableCellVerticalMerge.None, RowInfoItemType.EmptySpace));
				}

				for (int i = 0; i < rowDetails.Length; i++)
					rows.Add(new RowInfo(rowDetails[i]));

				return rows;
			}
			#endregion //CreateRowInfos

			#region GetCellFormatProperty
			private static DependencyProperty GetCellFormatProperty(RecordType recordType)
			{
				switch (recordType)
				{
					case RecordType.HeaderRecord:
						return DataPresenterWordWriter.CellSettingsForLabelProperty;
					case RecordType.DataRecord:
						return DataPresenterWordWriter.CellSettingsForDataRecordProperty;
					case RecordType.GroupByField:
					case RecordType.GroupByFieldLayout:
						return DataPresenterWordWriter.CellSettingsForGroupByRecordProperty;
					case RecordType.SummaryRecord:
						return DataPresenterWordWriter.CellSettingsForSummaryRecordProperty;
					case RecordType.ExpandableFieldRecord:
						return DataPresenterWordWriter.CellSettingsForExpandableFieldRecordProperty;
					default:
						Debug.Fail("Unexpected record type:" + recordType.ToString());
						return null;
				}
			} 
			#endregion //GetCellFormatProperty

			#region InitializeDefaultCellSettings
			private static void InitializeDefaultCellSettings(WordTableCellSettings formatSettings, RecordType recordType, Field field)
			{
				Color? backColor = null;
				Color? foreColor = null;
				ParagraphAlignment? horizontalAlignment = null;
				TableCellVerticalAlignment? verticalAlignment = null;

				switch (recordType)
				{
					case RecordType.HeaderRecord:
						#region HeaderRecord
						{
							backColor = Color.FromArgb(0xFF, 0x66, 0x66, 0x66);
							foreColor = Colors.White;

							verticalAlignment = TableCellVerticalAlignment.Center;

							if (null != field)
							{
								// AS 3/11/11 TFS68207
								// Honor the LabelTextAlignmentResolved
								//
								switch (field.LabelTextAlignmentResolved)
								{
									case TextAlignment.Left:
										horizontalAlignment = ParagraphAlignment.Left;
										break;
									case TextAlignment.Center:
										horizontalAlignment = ParagraphAlignment.Center;
										break;
									case TextAlignment.Right:
										horizontalAlignment = ParagraphAlignment.Right;
										break;
									case TextAlignment.Justify:
										horizontalAlignment = ParagraphAlignment.Distribute;
										break;
								}
							}
							break;
						} 
						#endregion //HeaderRecord
					case RecordType.GroupByField:
					case RecordType.GroupByFieldLayout:
						#region GroupByField
						{
							backColor = Color.FromArgb(0xFF, 0x35, 0xA9, 0xDA);
							foreColor = Colors.White;
							verticalAlignment = TableCellVerticalAlignment.Top;
							break;
						} 
						#endregion //GroupByField
					case RecordType.ExpandableFieldRecord:
						#region ExpandableFieldRecord
						{
							backColor = Color.FromArgb(0xFF, 0x99, 0x99, 0x99);
							foreColor = Colors.White;
							break;
						} 
						#endregion //ExpandableFieldRecord
					case RecordType.SummaryRecord:
						#region SummaryRecord
						{
							backColor = Color.FromArgb(0xFF, 0xD3, 0xD3, 0xD3);
							foreColor = Colors.Black;
							verticalAlignment = TableCellVerticalAlignment.Top;
							break;
						} 
						#endregion //SummaryRecord
					case RecordType.DataRecord:
						#region DataRecord
						{
							verticalAlignment = TableCellVerticalAlignment.Center;

							if (null != field)
							{
								if (field.Owner.LabelLocationResolved == LabelLocation.InCells)
								{
									switch (field.CellContentAlignmentResolved)
									{
										case CellContentAlignment.LabelAboveValueAlignCenter:
										case CellContentAlignment.LabelBelowValueAlignCenter:
											horizontalAlignment = ParagraphAlignment.Center;
											break;
										case CellContentAlignment.LabelAboveValueAlignRight:
										case CellContentAlignment.LabelBelowValueAlignRight:
											horizontalAlignment = ParagraphAlignment.Right;
											break;
										case CellContentAlignment.LabelOnly:
											break;
										case CellContentAlignment.LabelAboveValueAlignLeft:
										case CellContentAlignment.LabelBelowValueAlignLeft:
											horizontalAlignment = ParagraphAlignment.Left;
											break;
									}
								}
							}
							break;
						} 
						#endregion //DataRecord
				}

				if (null != backColor && !Utilities.ShouldSerialize(WordTableCellSettings.BackColorProperty, formatSettings))
					formatSettings.BackColor = backColor.Value;

				if (null != foreColor && formatSettings.ForeColor == null)
					formatSettings.ForeColor = foreColor;

				if (null != horizontalAlignment && !Utilities.ShouldSerialize(WordTableCellSettings.HorizontalAlignmentProperty, formatSettings))
					formatSettings.HorizontalAlignment = horizontalAlignment.Value;

				if (null != verticalAlignment && !Utilities.ShouldSerialize(WordTableCellSettings.VerticalAlignmentProperty, formatSettings))
					formatSettings.VerticalAlignment = verticalAlignment.Value;
			}

			#endregion //InitializeDefaultCellSettings

			#region InitializeRowInfos
			private void InitializeRowInfos(IList<RowInfo> rows, CellSettingsCache cellCache, WordTableCellSettings valueCellSettings)
			{
				var emptyFormatSettings = cellCache.FieldLayoutCellCache;

				foreach (var row in rows)
				{
					foreach (var item in row.Items)
					{
						switch (item.ItemType)
						{
							case RowInfoItemType.EmptySpace:
								item.CellSettings = emptyFormatSettings;
								break;
							case RowInfoItemType.Label:
								item.CellSettings = this.HeaderRecordCellSettingsCache[item.Field];
								break;
							case RowInfoItemType.Value:
								if (valueCellSettings != null)
									item.CellSettings = valueCellSettings;
								else
									item.CellSettings = cellCache[item.Field];
								break;
							default:
								Debug.Fail("Unrecognized item type:" + item.ItemType.ToString());
								continue;
						}

						var cellSettings = item.CellSettings;

						item.TableCellProperties = this.Writer.CreateTableCellProperties();
						WordExporter.InitializeTableCellProperties(item.TableCellProperties, this.Writer, cellSettings, item.ColumnSpan, item.VerticalMerge);

						if (cellSettings.HasParagraphSettings())
						{
							item.ParagraphProperties = this.Writer.CreateParagraphProperties();
							cellSettings.Initialize(this.Writer, item.ParagraphProperties);
						}

						if (cellSettings.HasFontSettings())
						{
							var font = this.Writer.CreateFont();
							cellSettings.Initialize(this.Writer, font);
							item.Font = font;
						}
					}
				}
			}
			#endregion //InitializeRowInfos

			#endregion //Private Methods

			#region Internal Methods

			#region GetColumnWidths
			internal List<float> GetColumnWidths(UnitOfMeasurement unit, int dataRecordIndent, bool hasExpandableField)
			{
				if (dataRecordIndent != _columnWidthsDataRecordIndent ||
					hasExpandableField != _columnWidthsHasExpandableField ||
					_cachedColumnWidths == null)
				{
					_cachedColumnWidths = this.GetColumnWidthsImpl(unit, dataRecordIndent, hasExpandableField);

					_columnWidthsHasExpandableField = hasExpandableField;
					_columnWidthsDataRecordIndent = dataRecordIndent;
				}

				return _cachedColumnWidths;
			}

			private List<float> GetColumnWidthsImpl(UnitOfMeasurement unit, int dataRecordIndent, bool hasExpandableField)
			{
				List<float> widths = new List<float>();

				float indentWidth = GetColumnWidth(this.ExpansionIndicatorSize.Width, unit);
				for (int i = 0, count = dataRecordIndent; i < count; i++)
					widths.Add(indentWidth);

				foreach (double width in this.ValueLayoutInfo.GetColumnWidths())
					widths.Add(GetColumnWidth(width, unit));

				// in the unlikely event that there are no cells but there are child records then 
				// include a 4 inch column
				if (hasExpandableField)
					widths.Add(GetColumnWidth(OnlyExpandableFieldColumnWidthInches, unit));

				// don't exceed word's max
				if (widths.Count > MaxColumnCount)
					widths.RemoveRange(MaxColumnCount, widths.Count - MaxColumnCount);

				// not sure if its our engine or word but when the sum of the widths exceeds
				// the max page size the column sizes come out really odd
				float totalWidth = 0;
				for (int i = 0, count = widths.Count; i < count; i++)
					totalWidth += widths[i];

				float inches = WordDocumentWriter.ConvertUnits(totalWidth, unit, UnitOfMeasurement.Inch);

				if (inches > MaxPageWidthInches)
				{
					float factor = MaxPageWidthInches / inches;
					for (int i = 0, count = widths.Count; i < count; i++)
						widths[i] *= factor;
				}

				return widths;
			}

			private static float GetColumnWidth(double width, UnitOfMeasurement unit)
			{
				Debug.Assert(!double.IsNaN(width), "NaN width");
				Debug.Assert(width >= float.MinValue && width <= float.MaxValue, "Outside float range");

				if (double.IsNaN(width))
					width = 32f;

				float floatWidth = (float)Math.Max(Math.Min(width, float.MaxValue), float.MinValue);

				// now we have logical pixel widths. we need to convert those to units
				floatWidth /= 96.0f; // convert to logical inches

				floatWidth = WordDocumentWriter.ConvertUnits(floatWidth, UnitOfMeasurement.Inch, unit);

				return floatWidth;
			}
			#endregion //GetColumnWidths

			#region GetGroupByRowInfos
			// Returns the row infos to be used for a given group by record. This method is needed because 
			// we want all the word cells that represent a group by to used the same appearance for the entire 
			// row or rows. Normally the Value RowInfoItem will have its cellsettings based on the field 
			// for which it is associated. However the Value items for a group by would only be the summary.
			internal ReadOnlyCollection<RowInfo> GetGroupByRowInfos(GroupByRecord gbr)
			{
				ReadOnlyCollection<RowInfo> rowInfos;
				Field field = gbr.GroupByField;

				if (field == null)
				{
					if (_groupByRowInfosForFieldLayout == null)
					{
						_groupByRowInfosForFieldLayout = new ReadOnlyCollection<RowInfo>(CreateRowInfos(this.FieldLayout, this.ValueLayoutInfo));
						this.InitializeRowInfos(_groupByRowInfosForFieldLayout, this.GroupByRecordCellSettingsCache, this.GroupByRecordCellSettingsCache.FieldLayoutCellCache);
					}

					rowInfos = _groupByRowInfosForFieldLayout;
				}
				else
				{
					if (_groupByRowInfosForFieldTable == null)
						_groupByRowInfosForFieldTable = new Dictionary<Field, ReadOnlyCollection<RowInfo>>();

					if (!_groupByRowInfosForFieldTable.TryGetValue(field, out rowInfos))
					{
						_groupByRowInfosForFieldTable[field] = rowInfos = new ReadOnlyCollection<RowInfo>(CreateRowInfos(this.FieldLayout, this.ValueLayoutInfo));
						this.InitializeRowInfos(rowInfos, this.GroupByRecordCellSettingsCache, this.GroupByRecordCellSettingsCache[field]);
					}
				}

				return rowInfos;
			}
			#endregion //GetGroupByRowInfos

			#region GetNestedTablePadding

			internal Padding? GetNestedTablePadding(UnitOfMeasurement unit)
			{
				if (_nestedTablePadding == null)
				{
					_nestedTablePadding = WordExporter.ConvertUnits(this.FlatTableSettings.NestedTableMargins, unit);

					if (_nestedTablePadding == null)
					{
						_nestedTablePadding = new Padding(
							WordDocumentWriter.ConvertUnits(9f, UnitOfMeasurement.Point, unit), 
							WordDocumentWriter.ConvertUnits(3f, UnitOfMeasurement.Point, unit), 
							0, 
							0);
					}
				}

				return _nestedTablePadding;
			} 
			#endregion //GetNestedTablePadding

			#region GetTableProperties
			internal TableProperties GetTableProperties(WordDocumentWriter writer)
			{
				if (_cachedTableProperties == null)
				{
					_cachedTableProperties = writer.CreateTableProperties();
					this.FlatTableSettings.Initialize(writer, _cachedTableProperties, TableLayout.Fixed);
				}

				return _cachedTableProperties;
			} 
			#endregion //GetTableProperties

			#region IsExpandableHeaderVisible
			internal bool IsExpandableHeaderVisible(ExpandableFieldRecord record)
			{
				if (record == null)
					return false;

				if (_expandableFieldHeaderVisibility == null)
					_expandableFieldHeaderVisibility = new Dictionary<Field, bool>();

				Field field = record.Field;

				bool isVisible;
				if (!_expandableFieldHeaderVisibility.TryGetValue(field, out isVisible))
				{
					var mode = field.ExpandableFieldRecordHeaderDisplayModeResolved;

					switch (mode)
					{
						case ExpandableFieldRecordHeaderDisplayMode.AlwaysDisplayHeader:
						case ExpandableFieldRecordHeaderDisplayMode.DisplayHeaderOnlyWhenExpanded:
							// note for DisplayHeaderOnlyWhenExpanded that this is exporting so the 
							// expansion state itself doesn't have meaning so we'll assume expanded
							isVisible = true;
							break;
						default:
							Debug.Fail("Unrecognized mode:" + mode.ToString());
							isVisible = false;
							break;
						case ExpandableFieldRecordHeaderDisplayMode.DisplayHeaderOnlyWhenCollapsed:
						case ExpandableFieldRecordHeaderDisplayMode.NeverDisplayHeader:
							// note for DisplayHeaderOnlyWhenCollapsed that this is exporting so the 
							// expansion state itself doesn't have meaning so we'll assume expanded
							isVisible = false;
							break;
					}

					_expandableFieldHeaderVisibility[field] = isVisible;
				}

				return isVisible;
			} 
			#endregion //IsExpandableHeaderVisible

			#endregion //Internal Methods

			#endregion //Methods
		}
		#endregion //FieldLayoutCache class

		#region RowInfoItemType enum
		private enum RowInfoItemType
		{
			Value,
			Label,
			EmptySpace,
		}
		#endregion //RowInfoItemType enum

		#region RowInfoItem class
		private class RowInfoItem
		{
			internal readonly Field Field;
			internal readonly int Column;
			internal readonly int ColumnSpan;
			internal readonly int RowSpan;
			internal readonly TableCellVerticalMerge VerticalMerge;
			internal readonly RowInfoItemType ItemType;
			internal WordTableCellSettings CellSettings;
			internal TableCellProperties TableCellProperties;
			internal ParagraphProperties ParagraphProperties;
			internal WordFont Font;

			internal RowInfoItem(Field field, int column, int columnSpan, int rowSpan, TableCellVerticalMerge verticalMerge, RowInfoItemType itemType)
			{
				this.Field = field;
				this.Column = column;
				this.ColumnSpan = columnSpan;
				this.RowSpan = rowSpan;
				this.VerticalMerge = verticalMerge;
				this.ItemType = itemType;
			}
		}
		#endregion //RowInfoItem class

		#region RowInfo class
		private class RowInfo
		{
			internal readonly ReadOnlyCollection<RowInfoItem> Items;

			internal RowInfo(IList<RowInfoItem> items)
			{
				this.Items = new ReadOnlyCollection<RowInfoItem>(items);
			}
		}
		#endregion //RowInfo class

		#region TableInfo class
		private class TableInfo
		{
			#region Member Variables

			internal readonly RecordManager RecordManager;
			internal readonly FieldLayout FieldLayout;
			internal readonly FieldLayoutCache FieldLayoutCache;
			internal readonly int ColumnCount;
			internal readonly bool HasGroupByFieldLayout;
			internal readonly int DataRecordIndent;
			internal readonly bool HasExpandableFieldColumn;
			internal RecordCollectionBase PreviousDataRecordParentCollection;




			internal bool HasAddedRootHeader;
			internal bool HasChildTableBreak;

			// default this to true so we know that we have or have never processed the request
			internal bool NeedsHeaderRecord = true;

			// expandable field records are handled specially since we 
			// may need to insert another table to wrap the children
			private ExpandableFieldRecord _currentExpandableFieldRecord;
			internal bool IsExpandableFieldRecordHeaderPending;

			internal bool HasCreatedExpandableFieldRecordTable;

			#endregion //Member Variables

			#region Constructor
			internal TableInfo(RecordManager recordManager, FieldLayout fieldLayout, FieldLayoutCache cache)
			{
				this.RecordManager = recordManager;
				this.FieldLayout = fieldLayout;
				this.FieldLayoutCache = cache;
				this.HasGroupByFieldLayout = recordManager.HasGroups &&
					recordManager.Groups.Count > 0 &&
					recordManager.Groups[0].RecordType == RecordType.GroupByFieldLayout;

				int columnCount = 0;

				if (fieldLayout.HasGroupBySortFields)
					columnCount += fieldLayout.SortedFields.CountOfGroupByFields;

				if (HasGroupByFieldLayout)
					columnCount++;

				this.DataRecordIndent = columnCount;

				int cellAreaColumnCount = cache.ValueLayoutInfo.GetColumnWidths().Length;
				columnCount += cellAreaColumnCount;

				// on the off chance that there are no cells but we have children then include 1 column
				this.HasExpandableFieldColumn = cellAreaColumnCount == 0 && FieldLayoutCache.VisibleFieldCount > 0;
				if (HasExpandableFieldColumn)
					columnCount++;

				columnCount = Math.Min(columnCount, FieldLayoutCache.MaxColumnCount);

				this.ColumnCount = columnCount;
			}
			#endregion //Constructor

			#region Properties

			#region CurrentExpandableFieldRecord
			internal ExpandableFieldRecord CurrentExpandableFieldRecord
			{
				get { return _currentExpandableFieldRecord; }
				set
				{
					_currentExpandableFieldRecord = value;
					if (value == null)
						this.IsExpandableFieldRecordHeaderPending = false;
					else
						this.IsExpandableFieldRecordHeaderPending = this.FieldLayoutCache.IsExpandableHeaderVisible(value);
				}
			} 
			#endregion //CurrentExpandableFieldRecord

			#region IsExpandableFieldRecordTableNeeded
			internal bool IsExpandableFieldRecordTableNeeded
			{
				get
				{
					if (this.HasCreatedExpandableFieldRecordTable ||
						this.CurrentExpandableFieldRecord == null)
					{
						return false;
					}

					if (!this.IsExpandableFieldRecordHeaderPending &&
						this.CurrentExpandableFieldRecord.ChildRecordManager != null)
					{
						return false;
					}

					return true;
				}
			}
			#endregion //IsExpandableFieldRecordTableNeeded 

			#endregion //Properties

			#region Methods

			#region GetColumnWidths
			internal List<float> GetColumnWidths(UnitOfMeasurement unit)
			{
				return this.FieldLayoutCache.GetColumnWidths(unit, this.DataRecordIndent, this.HasExpandableFieldColumn);
			}
			#endregion //GetColumnWidths

			#region IsMatch
			internal bool IsMatch(Record record, bool compareExpandableFieldRecord)
			{
				if (record.RecordManager != this.RecordManager || record.FieldLayout != this.FieldLayout)
					return false;

				if (compareExpandableFieldRecord)
				{
					// if the record passed in is an expandable field record then see 
					// if its the same as the one we currently have
					if (record is ExpandableFieldRecord)
					{
						if (record != this.CurrentExpandableFieldRecord)
							return false;
					}
					else if (this.CurrentExpandableFieldRecord != null)
					{
						// if we're handed a regular record then make sure 
						// we don't have an expandable field record
						return false;
					}
				}

				return true;
			}
			#endregion //IsMatch

			#endregion //Methods
		}
		#endregion //TableInfo class
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