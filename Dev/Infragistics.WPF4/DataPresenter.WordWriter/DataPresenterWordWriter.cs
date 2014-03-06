using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;
using Infragistics.Documents.Word;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Provides functionality for exporting <see cref="DataPresenterBase"/> derived controls to Microsoft Word.
	/// </summary>
	public class DataPresenterWordWriter : DependencyObject
	{
		#region Member Variables

		private WeakList<WordExporter> _exporters; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DataPresenterWordWriter"/>
		/// </summary>
		public DataPresenterWordWriter()
		{
			_exporters = new WeakList<WordExporter>();
		} 
		#endregion //Constructor

		#region Properties

		#region AsyncExportDuration

		/// <summary>
		/// Identifies the <see cref="AsyncExportDuration"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AsyncExportDurationProperty = DependencyProperty.Register("AsyncExportDuration",
			typeof(TimeSpan), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(50)), new ValidateValueCallback(ValidateTimeSpan));

		private static bool ValidateTimeSpan(object newValue)
		{
			TimeSpan ts = (TimeSpan)newValue;

			if (ts.Ticks <= 0)
				throw new ArgumentOutOfRangeException();

			return true;
		}

		/// <summary>
		/// Returns or sets the amount of time that should be spent processing the export each time the <see cref="AsyncExportInterval"/> elapses.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The TimeSpan must be greater than 0.</exception>
		/// <seealso cref="AsyncExportDurationProperty"/>
		/// <seealso cref="AsyncExportInterval"/>
		/// <seealso cref="ShowAsyncExportStatus"/>
		/// <seealso cref="ExportAsync(DataPresenterBase, string)"/>
		/// <seealso cref="ExportAsync(DataPresenterBase, Stream, bool)"/>
		[Bindable(true)]
		public TimeSpan AsyncExportDuration
		{
			get
			{
				return (TimeSpan)this.GetValue(DataPresenterWordWriter.AsyncExportDurationProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.AsyncExportDurationProperty, value);
			}
		}

		#endregion //AsyncExportDuration

		#region AsyncExportInterval

		/// <summary>
		/// Identifies the <see cref="AsyncExportInterval"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AsyncExportIntervalProperty = DependencyProperty.Register("AsyncExportInterval",
			typeof(TimeSpan), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(20)), new ValidateValueCallback(ValidateTimeSpan));

		/// <summary>
		/// Returns or sets the amount of time to wait after performing an asynchronous export for the amount of time specified by the <see cref="AsyncExportDuration"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The TimeSpan must be greater than 0.</exception>
		/// <seealso cref="AsyncExportIntervalProperty"/>
		/// <seealso cref="AsyncExportDuration"/>
		/// <seealso cref="ShowAsyncExportStatus"/>
		/// <seealso cref="ExportAsync(DataPresenterBase, string)"/>
		/// <seealso cref="ExportAsync(DataPresenterBase, Stream, bool)"/>
		[Bindable(true)]
		public TimeSpan AsyncExportInterval
		{
			get
			{
				return (TimeSpan)this.GetValue(DataPresenterWordWriter.AsyncExportIntervalProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.AsyncExportIntervalProperty, value);
			}
		}

		#endregion //AsyncExportInterval

		#region CellSettingsForDataRecord

		/// <summary>
		/// CellSettingsForDataRecord Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellSettingsForDataRecordProperty =
			DependencyProperty.RegisterAttached("CellSettingsForDataRecord", typeof(WordTableCellSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableCellSettings)null));

		/// <summary>
		/// Returns the format settings applied to Word Table Cell that represents a given field.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given <see cref="DataRecord"/> will use these settings. To affect all 
		/// DataRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// DataRecords for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect cells for a specific <see cref="Field"/>, you can set this property on the <see cref="Field.Settings"/> of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static WordTableCellSettings GetCellSettingsForDataRecord(DependencyObject d)
		{
			return (WordTableCellSettings)d.GetValue(CellSettingsForDataRecordProperty);
		}

		/// <summary>
		/// Sets the format settings applied to a <see cref="Cell"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given <see cref="DataRecord"/> will use these settings. To affect all 
		/// DataRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// DataRecords for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect cells for a specific <see cref="Field"/>, you can set this property on the <see cref="Field.Settings"/> of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		public static void SetCellSettingsForDataRecord(DependencyObject d, WordTableCellSettings value)
		{
			d.SetValue(CellSettingsForDataRecordProperty, value);
		}

		#endregion //CellSettingsForDataRecord

		#region CellSettingsForExpandableFieldRecord

		/// <summary>
		/// CellSettingsForExpandableFieldRecord Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellSettingsForExpandableFieldRecordProperty =
			DependencyProperty.RegisterAttached("CellSettingsForExpandableFieldRecord", typeof(WordTableCellSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableCellSettings)null));

		/// <summary>
		/// Returns the format settings applied to Word Table Cell that represents an <see cref="ExpandableFieldRecord"/> header.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given <see cref="ExpandableFieldRecord"/> will use these settings. To affect all 
		/// ExpandableFieldRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// ExpandableFieldRecords for a given parent <see cref="FieldLayout"/> (i.e. for all expandable field record headers that are children of a given field 
		/// layout), you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To affect cells for a specific <see cref="ExpandableFieldRecord.Field"/>, 
		/// you can set this property on the <see cref="Field.Settings"/> of that Field.</p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static WordTableCellSettings GetCellSettingsForExpandableFieldRecord(DependencyObject d)
		{
			return (WordTableCellSettings)d.GetValue(CellSettingsForExpandableFieldRecordProperty);
		}

		/// <summary>
		/// Sets the format settings applied to a <see cref="ExpandableFieldRecord"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given <see cref="ExpandableFieldRecord"/> will use these settings. To affect all 
		/// ExpandableFieldRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// ExpandableFieldRecords for a given parent <see cref="FieldLayout"/> (i.e. for all expandable field record headers that are children of a given field 
		/// layout), you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To affect cells for a specific <see cref="ExpandableFieldRecord.Field"/>, 
		/// you can set this property on the <see cref="Field.Settings"/> of that Field.</p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		public static void SetCellSettingsForExpandableFieldRecord(DependencyObject d, WordTableCellSettings value)
		{
			d.SetValue(CellSettingsForExpandableFieldRecordProperty, value);
		}

		#endregion //CellSettingsForExpandableFieldRecord

		#region CellSettingsForGroupByRecord

		/// <summary>
		/// CellSettingsForGroupByRecord Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellSettingsForGroupByRecordProperty =
			DependencyProperty.RegisterAttached("CellSettingsForGroupByRecord", typeof(WordTableCellSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableCellSettings)null));

		/// <summary>
		/// Returns the format settings applied to Word Table Cells that represents a <see cref="GroupByRecord"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given GroupByRecord will use these settings including summaries within 
		/// the GroupByRecord. To affect all GroupByRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// GroupByRecords for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect GroupByRecords grouped for a specific <see cref="GroupByRecord.GroupByField"/>, you can set this property on the <see cref="Field.Settings"/> 
		/// of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static WordTableCellSettings GetCellSettingsForGroupByRecord(DependencyObject d)
		{
			return (WordTableCellSettings)d.GetValue(CellSettingsForGroupByRecordProperty);
		}

		/// <summary>
		/// Sets the format settings applied to Word Table Cells that represents a <see cref="GroupByRecord"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given GroupByRecord will use these settings including summaries within 
		/// the GroupByRecord. To affect all GroupByRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// GroupByRecords for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect GroupByRecords grouped for a specific <see cref="GroupByRecord.GroupByField"/>, you can set this property on the <see cref="Field.Settings"/>.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		public static void SetCellSettingsForGroupByRecord(DependencyObject d, WordTableCellSettings value)
		{
			d.SetValue(CellSettingsForGroupByRecordProperty, value);
		}

		#endregion //CellSettingsForGroupByRecord

		#region CellSettingsForLabel

		/// <summary>
		/// CellSettingsForLabel Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellSettingsForLabelProperty =
			DependencyProperty.RegisterAttached("CellSettingsForLabel", typeof(WordTableCellSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableCellSettings)null));

		/// <summary>
		/// Returns the format settings applied to a <see cref="Field.Label"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells within a header record and all the labels within a record when the <see cref="FieldLayout.LabelLocationResolved"/> 
		/// is <b>InCells</b> will use these settings. To affect all labels through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. 
		/// To affect all the labels for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect labels for a specific <see cref="Field"/>, you can set this property on the <see cref="Field.Settings"/> of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static WordTableCellSettings GetCellSettingsForLabel(DependencyObject d)
		{
			return (WordTableCellSettings)d.GetValue(CellSettingsForLabelProperty);
		}

		/// <summary>
		/// Sets the format settings applied to a <see cref="Field.Label"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells within a header record and all the labels within a record when the <see cref="FieldLayout.LabelLocationResolved"/> 
		/// is <b>InCells</b> will use these settings. To affect all labels through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. 
		/// To affect all the labels for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect labels for a specific <see cref="Field"/>, you can set this property on the <see cref="Field.Settings"/> of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		public static void SetCellSettingsForLabel(DependencyObject d, WordTableCellSettings value)
		{
			d.SetValue(CellSettingsForLabelProperty, value);
		}

		#endregion //CellSettingsForLabel

		#region CellSettingsForSummaryRecord

		/// <summary>
		/// CellSettingsForSummaryRecord Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty CellSettingsForSummaryRecordProperty =
			DependencyProperty.RegisterAttached("CellSettingsForSummaryRecord", typeof(WordTableCellSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableCellSettings)null));

		/// <summary>
		/// Returns the format settings applied to Word Table Cells that represents a <see cref="SummaryRecord"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">All the word cells that are associated with a given SummaryRecord will use these settings. To affect all 
		/// SummaryRecords through out the control you can set this property on the <see cref="DataPresenter.FieldSettings"/>. To affect all the 
		/// SummaryRecords for a given <see cref="FieldLayout"/>, you can set this property on the <see cref="FieldLayout.FieldSettings"/>. To 
		/// affect SummaryRecords positioned for a specific <see cref="SummaryDefinition.PositionFieldName"/>, you can set this property on 
		/// the <see cref="Field.Settings"/> of that Field.
		/// </p>
		/// <p class="note"><b>Note:</b> This only affects the appearance of SummaryRecord objects and not the 
		/// summaries that are displayed within a group by record even when the summaries are aligned to the cells.</p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the Field's settings then the 
		/// FieldLayout's FieldSettings and lastly by the DataPresenter's FieldSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static WordTableCellSettings GetCellSettingsForSummaryRecord(DependencyObject d)
		{
			return (WordTableCellSettings)d.GetValue(CellSettingsForSummaryRecordProperty);
		}

		/// <summary>
		/// Sets the format settings applied to Word Table Cells that represents a <see cref="SummaryRecord"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This only affects the appearance of SummaryRecord objects and not the 
		/// summaries that are displayed within a group by record even when the summaries are aligned to the cells.</p>
		/// </remarks>
		public static void SetCellSettingsForSummaryRecord(DependencyObject d, WordTableCellSettings value)
		{
			d.SetValue(CellSettingsForSummaryRecordProperty, value);
		}

		#endregion //CellSettingsForSummaryRecord

		#region DefaultFontSettings

		/// <summary>
		/// Identifies the <see cref="DefaultFontSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultFontSettingsProperty = DependencyProperty.Register("DefaultFontSettings",
			typeof(WordFontSettings), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets an object that is used to initialize the <see cref="WordDocumentWriter.DefaultFont"/> when the exporter creates the writer.
		/// </summary>
		/// <seealso cref="DefaultFontSettingsProperty"/>
		/// <seealso cref="WordDocumentWriter.DefaultFont"/>
		public WordFontSettings DefaultFontSettings
		{
			get
			{
				return (WordFontSettings)this.GetValue(DataPresenterWordWriter.DefaultFontSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.DefaultFontSettingsProperty, value);
			}
		}

		#endregion //DefaultFontSettings

		#region DefaultParagraphSettings

		/// <summary>
		/// Identifies the <see cref="DefaultParagraphSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultParagraphSettingsProperty = DependencyProperty.Register("DefaultParagraphSettings",
			typeof(WordParagraphSettings), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets an object that is used to initialize the <see cref="WordDocumentWriter.DefaultTableProperties"/> when the exporter creates the writer.
		/// </summary>
		/// <seealso cref="DefaultParagraphSettingsProperty"/>
		/// <seealso cref="WordDocumentWriter.DefaultTableProperties"/>
		public WordParagraphSettings DefaultParagraphSettings
		{
			get
			{
				return (WordParagraphSettings)this.GetValue(DataPresenterWordWriter.DefaultParagraphSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.DefaultParagraphSettingsProperty, value);
			}
		}

		#endregion //DefaultParagraphSettings

		#region DefaultTableSettings

		/// <summary>
		/// Identifies the <see cref="DefaultTableSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultTableSettingsProperty = DependencyProperty.Register("DefaultTableSettings",
			typeof(WordDefaultTableSettings), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets an object that is used to initialize the <see cref="WordDocumentWriter.DefaultTableProperties"/> when the exporter creates the writer.
		/// </summary>
		/// <seealso cref="DefaultTableSettingsProperty"/>
		/// <seealso cref="WordDocumentWriter.DefaultTableProperties"/>
		public WordDefaultTableSettings DefaultTableSettings
		{
			get
			{
				return (WordDefaultTableSettings)this.GetValue(DataPresenterWordWriter.DefaultTableSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.DefaultTableSettingsProperty, value);
			}
		}

		#endregion //DefaultTableSettings

		#region DocumentSettings

		/// <summary>
		/// Identifies the <see cref="DocumentSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DocumentSettingsProperty = DependencyProperty.Register("DocumentSettings",
			typeof(WordDocumentSettings), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets an object that is used to initialize the <see cref="WordDocumentWriter.DocumentProperties"/> when the exporter creates the writer.
		/// </summary>
		/// <seealso cref="DocumentSettingsProperty"/>
		/// <seealso cref="WordDocumentWriter.DocumentProperties"/>
		/// <seealso cref="WordDocumentProperties"/>
		public WordDocumentSettings DocumentSettings
		{
			get
			{
				return (WordDocumentSettings)this.GetValue(DataPresenterWordWriter.DocumentSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.DocumentSettingsProperty, value);
			}
		}

		#endregion //DocumentSettings

		#region ExcludeExpandedState

		/// <summary>
		/// Identifies the <see cref="ExcludeExpandedState"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeExpandedStateProperty = DependencyProperty.Register("ExcludeExpandedState",
			typeof(bool), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a value indicating whether the expanded state of the source records will be copied to the records being exported.
		/// </summary>
		/// <seealso cref="ExcludeExpandedStateProperty"/>
		public bool ExcludeExpandedState
		{
			get
			{
				return (bool)this.GetValue(DataPresenterWordWriter.ExcludeExpandedStateProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeExpandedStateProperty, value);
			}
		}

		#endregion //ExcludeExpandedState

		#region ExcludeFieldLayoutSettings

		/// <summary>
		/// Identifies the <see cref="ExcludeFieldLayoutSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeFieldLayoutSettingsProperty = DependencyProperty.Register("ExcludeFieldLayoutSettings",
			typeof(bool), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings"/> from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeFieldLayoutSettingsProperty"/>
		/// <seealso cref="ExcludeFieldSettings"/>
		public bool ExcludeFieldLayoutSettings
		{
			get
			{
				return (bool)this.GetValue(DataPresenterWordWriter.ExcludeFieldLayoutSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeFieldLayoutSettingsProperty, value);
			}
		}

		#endregion //ExcludeFieldLayoutSettings

		#region ExcludeFieldSettings

		/// <summary>
		/// Identifies the <see cref="ExcludeFieldSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeFieldSettingsProperty = DependencyProperty.Register("ExcludeFieldSettings",
			typeof(bool), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the <see cref="Infragistics.Windows.DataPresenter.FieldSettings"/>  from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeFieldSettingsProperty"/>
		/// <seealso cref="ExcludeFieldLayoutSettings"/>
		public bool ExcludeFieldSettings
		{
			get
			{
				return (bool)this.GetValue(DataPresenterWordWriter.ExcludeFieldSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeFieldSettingsProperty, value);
			}
		}

		#endregion //ExcludeFieldSettings

		#region ExcludeGroupBySettings

		/// <summary>
		/// Identifies the <see cref="ExcludeGroupBySettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeGroupBySettingsProperty = DependencyProperty.Register("ExcludeGroupBySettings",
			typeof(Boolean), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the GroupBy state of the fields from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeGroupBySettingsProperty"/>
		public Boolean ExcludeGroupBySettings
		{
			get
			{
				return (Boolean)this.GetValue(DataPresenterWordWriter.ExcludeGroupBySettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeGroupBySettingsProperty, value);
			}
		}

		#endregion //ExcludeGroupBySettings

		#region ExcludeRecordFilters

		/// <summary>
		/// Identifies the <see cref="ExcludeRecordFilters"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeRecordFiltersProperty = DependencyProperty.Register("ExcludeRecordFilters",
			typeof(Boolean), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the RecordFilter state of the fields from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeRecordFiltersProperty"/>
		public Boolean ExcludeRecordFilters
		{
			get
			{
				return (Boolean)this.GetValue(DataPresenterWordWriter.ExcludeRecordFiltersProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeRecordFiltersProperty, value);
			}
		}

		#endregion //ExcludeRecordFilters

		#region ExcludeRecordVisibility

		/// <summary>
		/// Identifies the <see cref="ExcludeRecordVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeRecordVisibilityProperty = DependencyProperty.Register("ExcludeRecordVisibility",
			typeof(Boolean), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the Visibility setting of the records from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeRecordVisibilityProperty"/>
		public Boolean ExcludeRecordVisibility
		{
			get
			{
				return (Boolean)this.GetValue(DataPresenterWordWriter.ExcludeRecordVisibilityProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeRecordVisibilityProperty, value);
			}
		}

		#endregion //ExcludeRecordVisibility

		#region ExcludeSortOrder

		/// <summary>
		/// Identifies the <see cref="ExcludeSortOrder"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeSortOrderProperty = DependencyProperty.Register("ExcludeSortOrder",
			typeof(Boolean), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the sorted fields from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeSortOrderProperty"/>
		public Boolean ExcludeSortOrder
		{
			get
			{
				return (Boolean)this.GetValue(DataPresenterWordWriter.ExcludeSortOrderProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeSortOrderProperty, value);
			}
		}

		#endregion //ExcludeSortOrder

		#region ExcludeSummaries

		/// <summary>
		/// Identifies the <see cref="ExcludeSummaries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExcludeSummariesProperty = DependencyProperty.Register("ExcludeSummaries",
			typeof(Boolean), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether to copy the summaries of the FieldLayouts from the control being exported.
		/// </summary>
		/// <seealso cref="ExcludeSummariesProperty"/>
		public Boolean ExcludeSummaries
		{
			get
			{
				return (Boolean)this.GetValue(DataPresenterWordWriter.ExcludeSummariesProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ExcludeSummariesProperty, value);
			}
		}

		#endregion //ExcludeSummaries

		#region FinalSectionSettings

		/// <summary>
		/// Identifies the <see cref="FinalSectionSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FinalSectionSettingsProperty = DependencyProperty.Register("FinalSectionSettings",
			typeof(WordSectionSettings), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets an object that is used to initialize the <see cref="WordDocumentWriter.FinalSectionProperties"/> when the exporter creates the writer.
		/// </summary>
		/// <seealso cref="FinalSectionSettingsProperty"/>
		/// <seealso cref="WordDocumentWriter.FinalSectionProperties"/>
		public WordSectionSettings FinalSectionSettings
		{
			get
			{
				return (WordSectionSettings)this.GetValue(DataPresenterWordWriter.FinalSectionSettingsProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.FinalSectionSettingsProperty, value);
			}
		}

		#endregion //FinalSectionSettings

		#region IgnoreField

		/// <summary>
		/// IgnoreField Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IgnoreFieldProperty =
			DependencyProperty.RegisterAttached("IgnoreField", typeof(bool?), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the IgnoreField property.  This dependency property indicates whether the specified field should be collapsed/excluded from an export to Word using the DataPresenterWordWriter.
		/// </summary>
		/// <remarks>
		/// <para class="body">By default all fields that are displayed within the <see cref="DataPresenterBase"/> being exported are included in the 
		/// output. Setting this property to false for a given <see cref="Field"/> will cause the export to set the <see cref="Field.Visibility"/> to 
		/// collapsed when the field is exported. In addition, any summaries, sorted fields and filter references are removed from the containing 
		/// <see cref="FieldLayout"/>.</para>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldSettings))]
		public static bool? GetIgnoreField(DependencyObject d)
		{
			return (bool?)d.GetValue(IgnoreFieldProperty);
		}

		/// <summary>
		/// Sets the IgnoreField property.  This dependency property indicates whether the specified field should be collapsed/exclude from an export to Word using the DataPresenterWordWriter.
		/// </summary>
		/// <remarks>
		/// <para class="body">By default all fields that are displayed within the <see cref="DataPresenterBase"/> being exported are included in the 
		/// output. Setting this property to false for a given <see cref="Field"/> will cause the export to set the <see cref="Field.Visibility"/> to 
		/// collapsed when the field is exported. In addition, any summaries, sorted fields and filter references are removed from the containing 
		/// <see cref="FieldLayout"/>.</para>
		/// </remarks>
		public static void SetIgnoreField(DependencyObject d, bool? value)
		{
			d.SetValue(IgnoreFieldProperty, value);
		}

		#endregion //IgnoreField

		#region IsExporting

		private static readonly DependencyPropertyKey IsExportingPropertyKey =
			DependencyProperty.RegisterReadOnly("IsExporting",
			typeof(bool), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsExporting"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsExportingProperty =
			IsExportingPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the exporter is in the process of an export operation.
		/// </summary>
		/// <seealso cref="IsExportingProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsExporting
		{
			get
			{
				return (bool)this.GetValue(DataPresenterWordWriter.IsExportingProperty);
			}
			private set
			{
				this.SetValue(DataPresenterWordWriter.IsExportingPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsExporting

		#region ShowAsyncExportStatus

		/// <summary>
		/// Identifies the <see cref="ShowAsyncExportStatus"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowAsyncExportStatusProperty = DependencyProperty.Register("ShowAsyncExportStatus",
			typeof(bool), typeof(DataPresenterWordWriter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns or sets a value that indicates whether a control containing the export status is displayed to the end user while an asynchronous export is in progress.
		/// </summary>
		/// <seealso cref="ShowAsyncExportStatusProperty"/>
		/// <seealso cref="AsyncExportInterval"/>
		/// <seealso cref="AsyncExportDuration"/>
		[Description("Returns or sets a value that indicates whether a control containing the export status is displayed to the end user while an asynchronous export is in progress.")]
		[Category("Behavior")]
		[Bindable(true)]
		public bool ShowAsyncExportStatus
		{
			get
			{
				return (bool)this.GetValue(DataPresenterWordWriter.ShowAsyncExportStatusProperty);
			}
			set
			{
				this.SetValue(DataPresenterWordWriter.ShowAsyncExportStatusProperty, value);
			}
		}

		#endregion //ShowAsyncExportStatus

		#region TableSettingsForFieldLayout

		/// <summary>
		/// TableSettingsForFieldLayout Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty TableSettingsForFieldLayoutProperty =
			DependencyProperty.RegisterAttached("TableSettingsForFieldLayout", typeof(WordTableSettings), typeof(DataPresenterWordWriter),
				new FrameworkPropertyMetadata((WordTableSettings)null));

		/// <summary>
		/// Returns the format settings applied to a <see cref="FieldLayout"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property provides the settings applied to a Table created in the word export for a given <see cref="FieldLayout"/>.</p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the FieldLayout's Settings and 
		/// lastly by the DataPresenter's FieldLayoutSettings.</p>
		/// </remarks>
		[AttachedPropertyBrowsableForType(typeof(FieldLayoutSettings))]
		public static WordTableSettings GetTableSettingsForFieldLayout(DependencyObject d)
		{
			return (WordTableSettings)d.GetValue(TableSettingsForFieldLayoutProperty);
		}

		/// <summary>
		/// Sets the format settings applied to a <see cref="FieldLayout"/> when exported to word.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property provides the settings applied to a Table created in the word export for a given <see cref="FieldLayout"/>.</p>
		/// <p class="note"><b>Note:</b> The settings for these properties will be merged together giving precedence to the FieldLayout's Settings and 
		/// lastly by the DataPresenter's FieldLayoutSettings.</p>
		/// </remarks>
		public static void SetTableSettingsForFieldLayout(DependencyObject d, WordTableSettings value)
		{
			d.SetValue(TableSettingsForFieldLayoutProperty, value);
		}

		#endregion //TableSettingsForFieldLayout

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region CancelExport
		/// <summary>
		/// Cancels any asynchronous export operations for the specified <see cref="DataPresenterBase"/> that were initiated by this instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">An export operation may be canceled by handling the <see cref="InitializeRecord"/> event and setting the 
		/// <see cref="InitializeRecordEventArgs.TerminateExport"/> to true. However, for an asynchronous export the operation may be 
		/// pending while another asynchronous operation is pending or the export may still be preparing the source control. For those 
		/// situations this method may be used to cancel the operation.</p>
		/// </remarks>
		/// <param name="dataPresenter">The datapresenter whose asynchronous export is to be canceled</param>
		public void CancelExport(DataPresenterBase dataPresenter)
		{
			for (int i = _exporters.Count - 1; i >= 0; i--)
			{
				WordExporter exporter = _exporters[i];

				if (null != exporter && exporter.Source == dataPresenter)
				{
					dataPresenter.CancelExport(exporter);
				}
			}
		} 
		#endregion //CancelExport

		#region Export
		/// <summary>
		/// Exports the specified DataPresenter derived control to the specified file.
		/// </summary>
		/// <param name="dataPresenter">The control whose records are to be exported</param>
		/// <param name="fileName">The file to which the Word document should be written.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="dataPresenter"/> and <paramref name="fileName"/> must be non-null values.</exception>
		public void Export(DataPresenterBase dataPresenter, string fileName)
		{
			WordExporter exporter = new WordExporter(dataPresenter, this, fileName);
			exporter.Export();
		}

		/// <summary>
		/// Exports the specified DataPresenter derived control to the specified file.
		/// </summary>
		/// <param name="dataPresenter">The control whose records are to be exported</param>
		/// <param name="stream">The stream to which the Word document should be written.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="dataPresenter"/> and <paramref name="stream"/> must be non-null values.</exception>
		/// <exception cref="InvalidOperationException">The CanWrite of the <paramref name="stream"/> must be true.</exception>
		public void Export(DataPresenterBase dataPresenter, Stream stream)
		{
			WordExporter exporter = new WordExporter(dataPresenter, this, stream, false);
			exporter.Export();
		}
		#endregion //Export

		#region ExportAsync
		/// <summary>
		/// Asynchronously exports the specified DataPresenter derived control to the specified file.
		/// </summary>
		/// <param name="dataPresenter">The control whose records are to be exported</param>
		/// <param name="fileName">The file to which the Word document should be written.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="dataPresenter"/> and <paramref name="fileName"/> must be non-null values.</exception>
		public void ExportAsync(DataPresenterBase dataPresenter, string fileName)
		{
			WordExporter exporter = new WordExporter(dataPresenter, this, fileName);
			exporter.ExportAsync();
		}

		/// <summary>
		/// Asynchronously exports the specified DataPresenter derived control to the specified file.
		/// </summary>
		/// <param name="dataPresenter">The control whose records are to be exported</param>
		/// <param name="stream">The stream to which the Word document should be written.</param>
		/// <param name="closeStream">A boolean indicating whether the <paramref name="stream"/> should be explicitly closed after the <see cref="ExportEnding"/> event has been raised.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="dataPresenter"/> and <paramref name="stream"/> must be non-null values.</exception>
		/// <exception cref="InvalidOperationException">The CanWrite of the <paramref name="stream"/> must be true.</exception>
		public void ExportAsync(DataPresenterBase dataPresenter, Stream stream, bool closeStream)
		{
			WordExporter exporter = new WordExporter(dataPresenter, this, stream, closeStream);
			exporter.ExportAsync();
		}
		#endregion //ExportAsync

		#endregion //Public Methods

		#region Internal Methods

		#region AddExporter
		internal void AddExporter(WordExporter exporter)
		{
			_exporters.Add(exporter);
			this.IsExporting = _exporters.Count > 0;
		}
		#endregion //AddExporter

		#region RemoveExporter
		internal void RemoveExporter(WordExporter exporter)
		{
			_exporters.Remove(exporter);
			_exporters.Compact();
			this.IsExporting = _exporters.Count > 0;
		}
		#endregion //RemoveExporter

		#endregion //Internal Methods

		#region Private Methods

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		#region ExportStarted

		/// <summary>
		/// Used to invoke the <see cref="ExportStarted"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnExportStarted(ExportStartedEventArgs e)
		{
			var handler = this.ExportStarted;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs at the beginning of the export process.  The <see cref="ExportStartedEventArgs.ExportControl"/> has been initialized but the <see cref="DataPresenterBase.DataSource"/> has not yet been set.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <i>ExportControl</i> argument returns a reference to the cloned object that is about to be exported.</p>        
		/// </remarks>        
		public event EventHandler<ExportStartedEventArgs> ExportStarted;

		#endregion //ExportStarted

		#region CellExporting

		/// <summary>
		/// Used to invoke the <see cref="CellExporting"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnCellExporting(CellExportingEventArgs e)
		{
			var handler = this.CellExporting;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when a <see cref="Cell"/> of a <see cref="DataRecord"/> is about to be written to the Word document.
		/// </summary>
		/// <remarks>
		/// <p class="body">This event provides an opportunity to change the <see cref="CellExportingEventArgsBase.Value"/> that will 
		/// written out for the specified cell.</p>
		/// </remarks>
		public event EventHandler<CellExportingEventArgs> CellExporting;

		#endregion //CellExporting

		#region ExportEnding

		/// <summary>
		/// Used to invoke the <see cref="ExportEnding"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnExportEnding(ExportEndingEventArgs e)
		{
			var handler = this.ExportEnding;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs at the end of the export process.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ExportEnding event is invoked when the export process has completed to give the developer 
		/// an opportunity to manipulate the Writer being used. This is particularly important when using an 
		/// overload of the Export method that takes a filename as the file will be saved after the ExportEnding is complete 
		/// but before the <see cref="ExportEnded"/>. Also, since the file will not have been created until after the ExportEnding, 
		/// if you wanted to copy the file to another location or display the file to the end user, the ExportEnded event should be used to do that.</p>
		/// <p class="body">The <see cref="ExportEndingEventArgs.Canceled"/> property can be used to determine if the export operation was canceled.</p>
		/// <p class="note"><b>Note:</b> The <see cref="WordDocumentWriter.EndDocument(bool)"/> of the <see cref="ExportEndingEventArgs.Writer"/> will be invoked after this event is raised if the DataPresenterWordWriter created the writer.</p>
		/// </remarks>   
		/// <seealso cref="ExportEnded"/>
		public event EventHandler<ExportEndingEventArgs> ExportEnding;

		#endregion //ExportEnding

		#region ExportEnded

		/// <summary>
		/// Used to invoke the <see cref="ExportEnded"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnExportEnded(ExportEndedEventArgs e)
		{
			var handler = this.ExportEnded;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs after the export process has completed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ExportEnded event is raised after the export process has been completed. If an overload of 
		/// the Export method that accepts a filename was used, then the file would be available at this point since the file 
		/// is saved after the <see cref="ExportEnding"/> event has been completed but before the ExportEnded. Therefore any 
		/// manipulations of the Writer should be done in the <see cref="ExportEnding"/> event.</p>
		/// </remarks>        
		/// <seealso cref="ExportEnding"/>
		public event EventHandler<ExportEndedEventArgs> ExportEnded;

		#endregion //ExportEnded

		#region InitializeRecord

		/// <summary>
		/// Used to invoke the <see cref="InitializeRecord"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnInitializeRecord(InitializeRecordEventArgs e)
		{
			var handler = this.InitializeRecord;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when a record is about to be processed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="InitializeRecordEventArgs"/> exposes properties that can be used to prevent the current record, its 
		/// descendants or its siblings from being exported. The <see cref="InitializeRecordEventArgs.TerminateExport"/> property can be 
		/// set to true to end the export operation.</p>
		/// </remarks>        
		public event EventHandler<InitializeRecordEventArgs> InitializeRecord;

		#endregion //InitializeRecord

		#region LabelExporting

		/// <summary>
		/// Used to invoke the <see cref="LabelExporting"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnLabelExporting(LabelExportingEventArgs e)
		{
			var handler = this.LabelExporting;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the <see cref="Field.Label"/> of a <see cref="Field"/> is about to be written to the Word document.
		/// </summary>
		/// <remarks>
		/// <p class="body">This event provides an opportunity to change the <see cref="CellExportingEventArgsBase.Value"/> that will 
		/// written out for the specified Field label.</p>
		/// </remarks>
		public event EventHandler<LabelExportingEventArgs> LabelExporting;

		#endregion //LabelExporting

		#region SummaryResultsExporting

		/// <summary>
		/// Used to invoke the <see cref="SummaryResultsExporting"/> event.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected virtual void OnSummaryResultsExporting(SummaryResultsExportingEventArgs e)
		{
			var handler = this.SummaryResultsExporting;

			if (null != handler)
				handler(this, e);
		}

		/// <summary>
		/// Occurs when one or more <see cref="SummaryResult"/> instances are being written to the Word document.
		/// </summary>
		/// <remarks>
		/// <p class="body">This event provides an opportunity to change the <see cref="CellExportingEventArgsBase.Value"/> that will 
		/// written out based upon the specified <see cref="SummaryResultsExportingEventArgs.SummaryResults"/>. The results are aggregated 
		/// together for a given <see cref="SummaryRecord"/> or <see cref="GroupByRecord"/> based upon the resolved SummaryPosition. For 
		/// example the event will be raised once for all summaries within a SummaryRecord whose SummaryPosition is set to Left, Center or 
		/// Right. It will also be raised for each Field that has summaries positioned based on the Field location.</p>
		/// <p class="note"><b>Note:</b> The event is not raised for any position for which there are no summaries.</p>
		/// </remarks>
		public event EventHandler<SummaryResultsExportingEventArgs> SummaryResultsExporting;

		#endregion //SummaryResultsExporting

		#endregion //Events
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