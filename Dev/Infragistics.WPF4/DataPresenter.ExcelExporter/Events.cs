using Infragistics.Documents.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter.ExcelExporter
{   
    #region BeginExportEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.BeginExport"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
	[Obsolete("The 'BeginExport' method is obsolete. The 'ExportStarted' event should be used instead.", false)] // AS 2/11/11 NA 2011.1 Word Writer
	public class BeginExportEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private DataPresenterBase dataPresenter;

        #endregion //Private Members

        #region Constructor

        internal BeginExportEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            DataPresenterBase dataPresenter)
            : base(exporterHelper)
        {
            this.dataPresenter = dataPresenter;
        }
        #endregion //Constructor

        #region Public Properties

        #region DataPresenter

        /// <summary>
        /// Returns a clone of the DataPresenter that is about to be exported.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>The data presenter has been intialized from the original
        /// source, but the DataSource property has not been copied.  Attempting to set the DatSource
        /// manually will result in an exception.</p>
        /// </remarks>
        public DataPresenterBase DataPresenter
        {
            get { return this.dataPresenter; }
        }
        #endregion //DataPresenter

        #endregion //Public Properties
    }
    #endregion //BeginExportEventArgs

    #region CellExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.CellExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class CellExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private DataRecord record;
        private Field field;
        private object value;

        #endregion //Private Members

        #region Constructor

        internal CellExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            DataRecord record, Field field, object value) :
            base(exporterHelper)
        {
            this.record = record;
            this.field = field;
            this.value = value;
        }
        #endregion //Constructor

        #region Public Properties

        #region Field

        /// <summary>
        /// Returns the field containing the cell.
        /// </summary>
        public Field Field
        {
            get { return this.field; }
        }
        #endregion //Field

        #region Record

        /// <summary>
        /// Returns the record containing the cell.
        /// </summary>
        public DataRecord Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #region Value

        /// <summary>
        /// Returns the exported value of the cell.
        /// </summary>
        public object Value
        {
            get { return this.value; }            
        }
        #endregion //Value

        #endregion //Public Properties
    }
    #endregion //CellExportedEventArgs

    #region CellExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.CellExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class CellExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members
        
        private Field field;
        private DataRecord record;

        #endregion //Private Members

        #region Constructor

        internal CellExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            DataRecord record, Field field, FormatSettings cellFormatSettings) :
            base(exporterHelper, cellFormatSettings)
        {            
            this.field = field;
            this.record = record;            
        }
        #endregion //Constructor

        #region Public Properties

        #region Field

        /// <summary>
        /// Returns the field containing the cell.
        /// </summary>
        public Field Field
        {
            get { return this.field; }
        }
        #endregion //Field

        #region Record

        /// <summary>
        /// Returns the record containing the cell.
        /// </summary>
        public DataRecord Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #region Value

        /// <summary>
        /// Gets or sets the value to export for the cell.
        /// </summary>
        public object Value
        {
            get { return this.Record.GetCellValue(this.Field); }
            set { this.Record.SetCellValue(this.Field, value); }
        }
        #endregion //Value

        #endregion //Public Properties
    }
    #endregion //CellExportingEventArgs

    #region EndExportEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.EndExport"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
	[Obsolete("The 'EndExport' method is obsolete. The 'ExportEnding' or 'ExportEnded' event should be used instead. See the comments for those events for more information on which to use.", false)] // AS 2/11/11 NA 2011.1 Word Writer
    public class EndExportEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private bool canceled;        

        #endregion //Private Members

        #region Constructor

        internal EndExportEventArgs(DataPresenterExcelExporterHelper exporterHelper, bool canceled) :
            base(exporterHelper)
        {
            this.canceled = canceled;            
        }
        #endregion //Constructor

        #region Public Properties

        #region Canceled

        /// <summary>
        /// True if exporting process was been canceled.
        /// </summary>
        public bool Canceled
        {
            get { return this.canceled; }
        }
        #endregion //Canceled

        #endregion //Public Properties
    }
    #endregion //EndExportEventArgs

    #region ExcelExportCancelEventArgs

    /// <summary>
    /// ExcelExportCancelEventArgs is a base class for cancelable event argument classes.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class ExcelExportCancelEventArgs : System.ComponentModel.CancelEventArgs
    {
        #region Private Members

        private FormatSettings clonedFormatSettings;
        private DataPresenterExcelExporterHelper exporterHelper;
        private FormatSettings formatSettings;

        #endregion Private Members

        #region Constructor

        internal ExcelExportCancelEventArgs(DataPresenterExcelExporterHelper exporterHelper, FormatSettings formatSettings)
        {
            this.exporterHelper = exporterHelper;
            this.formatSettings = formatSettings;
        }
        #endregion Constructor

        #region Internal Properties

        #region FormatSettingsInternal

        internal FormatSettings FormatSettingsInternal
        {
            get
            {
                if (this.clonedFormatSettings != null)
                    return this.clonedFormatSettings;

                return this.formatSettings;
            }
        }
        #endregion //FormatSettingsInternal

        #endregion //Internal Properties

        #region Public Properties

        #region Workbook

        /// <summary>
        /// Exporting workbook.
        /// </summary>
        public Workbook Workbook
        {
            get { return this.exporterHelper.CurrentWorksheet.Workbook; }
        }
        #endregion //Workbook

        #region CurrentColumnIndex

        /// <summary>
        /// Zero-based index of current exporting column in excel worksheet..
        /// </summary>			
        public int CurrentColumnIndex
        {
            get { return this.exporterHelper.currentPos.X; }
            set { this.exporterHelper.currentPos.X = value; }
        }
        #endregion //CurrentColumnIndex

        #region CurrentRowIndex

        /// <summary>
        /// Zero-based index of current exporting row in excel worksheet.
        /// </summary>
        public int CurrentRowIndex
        {
            get { return this.exporterHelper.currentPos.Y; }
            set { this.exporterHelper.currentPos.Y = value; }
        }
        #endregion //CurrentRowIndex

        #region CurrentOutlineLevel

        /// <summary>
        /// Current outline level used for grouping.
        /// </summary>
        public int CurrentOutlineLevel
        {
            get { return this.exporterHelper.HierarchyLevel; }
        }
        #endregion //CurrentOutlineLevel

        #region CurrentWorksheet

        /// <summary>
        /// Current exporting worksheet.
        /// </summary>
        public Worksheet CurrentWorksheet
        {
            get { return this.exporterHelper.CurrentWorksheet; }
            set { this.exporterHelper.CurrentWorksheet = value; }
        }
        #endregion //CurrentWorksheet

        #region FormatSettings

        /// <summary>
        /// Gets or sets the settings that should be applied to exported label.  Settings specified on
        /// individual labels take precedence.
        /// </summary>
        public FormatSettings FormatSettings
        {
            get
            {
                // Since we pass in the cached FormatSettings object here, we don't want the
                // user to modify this object directly here since it could apply to many
                // other instances of the export.  For the sake of efficiency, only clone
                // this object when the user requests it
                if (this.clonedFormatSettings != null)
                    return this.clonedFormatSettings;

                if (this.formatSettings != null)
                    this.clonedFormatSettings = (FormatSettings)this.formatSettings.Clone();
                else
                    Debug.Fail("We shouldn't have a null labelFormatSettings");

                return this.clonedFormatSettings;
            }
            set
            {
                // Set the cloned format settings so that we know that we don't need to clone this object
                this.clonedFormatSettings = value;
            }
        }
        #endregion //FormatSettings

        #endregion Public Properties
    }
    #endregion //ExcelExportCancelEventArgs

    #region ExcelExportEventArgs

    /// <summary>
    /// ExcelExportEventArgs is a base class for non-cancelable event argument classes.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class ExcelExportEventArgs : System.EventArgs
    {
        #region Private Members

        private DataPresenterExcelExporterHelper exporterHelper;

        #endregion Private Members

        #region Constructor

        internal ExcelExportEventArgs(DataPresenterExcelExporterHelper exporterHelper)
        {
            this.exporterHelper = exporterHelper;
        }

        #endregion Constructor

        #region Public Properties

        #region CurrentColumnIndex

        /// <summary>
        /// Zero-based index of current exporting column in excel worksheet.
        /// </summary>			
        public int CurrentColumnIndex
        {
            get { return this.exporterHelper.currentPos.X; }
            set { this.exporterHelper.currentPos.X = value; }
        }
        #endregion //CurrentColumnIndex

        #region CurrentOutlineLevel

        /// <summary>
        /// Current outline level used for grouping.
        /// </summary>
        public int CurrentOutlineLevel
        {
            get { return this.exporterHelper.HierarchyLevel; }           
        }
        #endregion //CurrentOutlineLevel

        #region CurrentRowIndex

        /// <summary>
        /// Zero-based index of current exporting row in excel worksheet.
        /// </summary>
        public int CurrentRowIndex
        {
            get { return this.exporterHelper.currentPos.Y; }
            set { this.exporterHelper.currentPos.Y = value; }
        }
        #endregion //CurrentRowIndex

        #region CurrentWorksheet

        /// <summary>
        /// Current exporting worksheet.
        /// </summary>
        public Worksheet CurrentWorksheet
        {
            get { return this.exporterHelper.CurrentWorksheet; }
            set 
            {                
                if (value == null)
					throw new ArgumentNullException(DataPresenterExcelExporter.GetString("LER_CurrentWorksheetNull"));

                this.exporterHelper.CurrentWorksheet = value; 
            }
        }
        #endregion //CurrentWorksheet

        #region Workbook

        /// <summary>
        /// Exporting workbook.
        /// </summary>
        public Workbook Workbook
        {
            get { return this.exporterHelper.CurrentWorksheet.Workbook; }            
        }
        #endregion //Workbook

        #endregion Public Properties
    }
    #endregion //ExcelExportEventArgs

	// AS 2/11/11 NA 2011.1 Word Writer
	#region ExportStartedEventArgs

	/// <summary>
	/// Event parameters used for the <see cref="DataPresenterExcelExporter.ExportStarted"/> event.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class ExportStartedEventArgs : ExcelExportEventArgs
	{
		#region Private Members

		private DataPresenterBase dataPresenter;

		#endregion //Private Members

		#region Constructor

		internal ExportStartedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
			DataPresenterBase dataPresenter)
			: base(exporterHelper)
		{
			this.dataPresenter = dataPresenter;
		}
		#endregion //Constructor

		#region Public Properties

		#region DataPresenter

		/// <summary>
		/// Returns a clone of the DataPresenter that is about to be exported.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The data presenter has been intialized from the original
		/// source, but the DataSource property has not been copied.  Attempting to set the DatSource
		/// manually will result in an exception.</p>
		/// </remarks>
		public DataPresenterBase DataPresenter
		{
			get { return this.dataPresenter; }
		}
		#endregion //DataPresenter

		#endregion //Public Properties
	}
	#endregion //ExportStartedEventArgs

	// AS 2/11/11 NA 2011.1 Word Writer
	#region ExportEndedEventArgs

	/// <summary>
	/// Event parameters used for the <see cref="DataPresenterExcelExporter.ExportEnded"/> event.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class ExportEndedEventArgs : ExcelExportEventArgs
	{
		#region Private Members

		private RecordExportCancellationInfo _cancelInfo;

		#endregion //Private Members

		#region Constructor

		internal ExportEndedEventArgs(DataPresenterExcelExporterHelper exporterHelper, RecordExportCancellationInfo cancelInfo) :
			base(exporterHelper)
		{
			_cancelInfo = cancelInfo;
		}
		#endregion //Constructor

		#region Public Properties

		#region CancelInfo
		/// <summary>
		/// Returns an object that provides information about action that canceled the export.
		/// </summary>
		public RecordExportCancellationInfo CancelInfo
		{
			get { return _cancelInfo; }
		}
		#endregion //CancelInfo

		#region Canceled

		/// <summary>
		/// True if exporting process was been canceled.
		/// </summary>
		public bool Canceled
		{
			get { return _cancelInfo != null; }
		}
		#endregion //Canceled

		#endregion //Public Properties
	}
	#endregion //ExportEndedEventArgs

	// AS 2/11/11 NA 2011.1 Word Writer
	#region ExportEndingEventArgs

	/// <summary>
	/// Event parameters used for the <see cref="DataPresenterExcelExporter.ExportEnding"/> event.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class ExportEndingEventArgs : ExcelExportEventArgs
	{
		#region Private Members

		private RecordExportCancellationInfo _cancelInfo;

		#endregion //Private Members

		#region Constructor

		internal ExportEndingEventArgs(DataPresenterExcelExporterHelper exporterHelper, RecordExportCancellationInfo cancelInfo) :
			base(exporterHelper)
		{
			_cancelInfo = cancelInfo;
		}
		#endregion //Constructor

		#region Public Properties

		#region CancelInfo
		/// <summary>
		/// Returns an object that provides information about action that canceled the export.
		/// </summary>
		public RecordExportCancellationInfo CancelInfo
		{
			get { return _cancelInfo; }
		}
		#endregion //CancelInfo

		#region Canceled

		/// <summary>
		/// True if exporting process was canceled.
		/// </summary>
		public bool Canceled
		{
			get { return _cancelInfo != null; }
		}
		#endregion //Canceled

		#endregion //Public Properties
	}
	#endregion //ExportEndingEventArgs
		
    #region HeaderAreaExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.HeaderAreaExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class HeaderAreaExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private FieldLayout fieldLayout;
        private Record record;

        #endregion //Private Members

        #region Constructor

        internal HeaderAreaExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            FieldLayout fieldLayout, Record record) :
            base(exporterHelper)
        {
            this.fieldLayout = fieldLayout;
            this.record = record;
        }
        #endregion //Constructor

        #region Public Properties

        #region FieldLayout

        /// <summary>
        /// Returns the layout associated with the header being exported.
        /// </summary>
        public FieldLayout FieldLayout
        {
            get { return this.fieldLayout; }
        }
        #endregion //FieldLayout

        #region Record

        /// <summary>
        /// Returns the first record associated with the header.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #endregion //Public Properties
    }
    #endregion //HeaderAreaExportedEventArgs

    #region HeaderAreaExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.HeaderAreaExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class HeaderAreaExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members

        private FieldLayout fieldLayout;        
        private Record record;

        #endregion //Private Members

        #region Constructor

        internal HeaderAreaExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            FieldLayout fieldLayout, Record record, FormatSettings labelFormatSettings) :
            base(exporterHelper, labelFormatSettings)
        {
            this.fieldLayout = fieldLayout;            
            this.record = record;
        }
        #endregion //Constructor

        #region Public Properties

        #region FieldLayout

        /// <summary>
        /// Returns the layout associated with the header being exported.
        /// </summary>
        public FieldLayout FieldLayout
        {
            get { return this.fieldLayout; }
        }
        #endregion //FieldLayout        

        #region Record

        /// <summary>
        /// Returns the first record associated with the header.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #endregion //Public Properties
    }
    #endregion //HeaderAreaExportingEventArgs

    #region HeaderLabelExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.HeaderLabelExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class HeaderLabelExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private Field field;
        private Record record;

        #endregion //Private Members

        #region Constructor

        internal HeaderLabelExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            Field field, Record record) :
            base(exporterHelper)
        {
            this.field = field;
            this.record = record;
        }
        #endregion //Constructor

        #region Public Properties

        #region Field

        /// <summary>
        /// Returns the field associated with the header cell.
        /// </summary>
        public Field Field
        {
            get { return this.field; }
        }
        #endregion //Field

        #region Label

        /// <summary>
        /// Returns the label value of the header cell.
        /// </summary>
        public object Label
        {
            get { return this.Field.Label; }            
        }
        #endregion //Label

        #region Record

        /// <summary>
        /// Returns the first record associated with the headers.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #endregion //Public Properties
    }
    #endregion //HeaderLabelExportedEventArgs

    #region HeaderLabelExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.HeaderLabelExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class HeaderLabelExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members
        
        private Field field;
        private Record record;        

        #endregion //Private Members

        #region Constructor

        internal HeaderLabelExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            Field field, Record record, FormatSettings labelFormatSettings) :
            base(exporterHelper, labelFormatSettings)
        {
            this.field = field;            
            this.record = record;
        }
        #endregion //Constructor        

        #region Public Properties

        #region Field

        /// <summary>
        /// Returns the field associated with the header cell.
        /// </summary>
        public Field Field
        {
            get { return this.field; }
        }
        #endregion //Field

        #region Label

        /// <summary>
        /// Gets or sets the label value of the header cell.
        /// </summary>
        public object Label
        {
            get { return this.Field.Label; }
            set { this.Field.Label = value; }
        }
        #endregion //Label        

        #region Record

        /// <summary>
        /// Returns the first record associated with the headers.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #endregion //Public Properties
    }
    #endregion //HeaderLabelExportingEventArgs

    #region InitializeRecordEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.InitializeRecord"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class InitializeRecordEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private Record record;
        private ProcessRecordParams processRecordParams;
        private bool skipRecord;

        #endregion //Private Members

        #region Constructor

        internal InitializeRecordEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            ProcessRecordParams processRecordParams, Record record)
            : base(exporterHelper)
        {
            this.record = record;
            this.processRecordParams = processRecordParams;            
        }
        #endregion //Constructor

        #region Public Properties

        #region Record

        /// <summary>
        /// Returns the record that is going to be exported.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #region SkipDescendants

        /// <summary>
        /// Specifies whether to skip the descendants of the current record.
        /// </summary>
        public bool SkipDescendants
        {
            get { return this.processRecordParams.SkipDescendants; }
            set { this.processRecordParams.SkipDescendants = value; }
        }
        #endregion //SkipDescendants

        #region SkipRecord

        /// <summary>
        /// Specifies whether to skip the current record.
        /// </summary>
        public bool SkipRecord
        {
            get { return this.skipRecord; }
            set { this.skipRecord = value; }
        }
        #endregion //SkipRecord

        #region SkipSiblings

        /// <summary>
        /// Specifies whether to skip sibling records of the current record.
        /// </summary>
        public bool SkipSiblings
        {
            get { return this.processRecordParams.SkipSiblings; }
            set { this.processRecordParams.SkipSiblings = value; }
        }
        #endregion //SkipSiblings

        #region TerminateExport

        /// <summary>
        /// Specifies whether to terminate the export process.  The current record will 
        /// not be processed.
        /// </summary>
        public bool TerminateExport
        {
            get { return this.processRecordParams.TerminateExport; }
            set { this.processRecordParams.TerminateExport = value; }
        }
        #endregion //TerminateExport

        #endregion //Public Properties
    }
    #endregion //InitializeRecordEventArgs

    #region RecordExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.RecordExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class RecordExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private Record record;

        #endregion //Private Members

        #region Constructor

        internal RecordExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            Record record)
            : base(exporterHelper)
        {
            this.record = record;
        }
        #endregion //Constructor

        #region Public Properties

        #region Record

        /// <summary>
        /// Returns the record being exported.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record

        #endregion //Public Properties
    }
    #endregion //RecordExportedEventArgs

    #region RecordExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.RecordExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class RecordExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members

        private Record record;        

        #endregion //Private Members

        #region Constructor

        internal RecordExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            Record record, FormatSettings recordFormatSettings)
            : base(exporterHelper, recordFormatSettings)
        {
            this.record = record;            
        }
        #endregion //Constructor

        #region Public Properties

        #region Record

        /// <summary>
        /// Returns the record that is being exported.
        /// </summary>
        public Record Record
        {
            get { return this.record; }
        }
        #endregion //Record        

        #endregion //Public Properties
    }
    #endregion //RecordExportingEventArgs    

    #region SummaryCellExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.SummaryCellExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class SummaryCellExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private SummaryResult summary;
        private int summaryLevel;

        #endregion //Private Members

        #region Constructor

        internal SummaryCellExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            SummaryResult summary, int summaryLevel)
            : base(exporterHelper)
        {
            this.summary = summary;
            this.summaryLevel = summaryLevel;
        }
        #endregion //Constructor

        #region Public Properties

        #region Summary

        /// <summary>
        /// Returns the summary being exported.
        /// </summary>
        public SummaryResult Summary
        {
            get { return this.summary; }
        }
        #endregion //Summary

        #region SummaryLevel

        /// <summary>
        /// Returns the 0-based index of the current summary record.
        /// </summary>
        public int SummaryLevel
        {
            get { return this.summaryLevel; }
        }
        #endregion //SummaryLevel

        #endregion //Public Properties
    }
    #endregion //SummaryCellExportedEventArgs

    #region SummaryCellExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.SummaryCellExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class SummaryCellExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members

        private SummaryResult summary;        
        private int summaryLevel;

        #endregion //Private Members

        #region Constructor

        internal SummaryCellExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            SummaryResult summary, int summaryLevel, FormatSettings summaryFormatSettings)
            : base(exporterHelper, summaryFormatSettings)
        {
            this.summary = summary;            
            this.summaryLevel = summaryLevel;
        }
        #endregion //Constructor

        #region Public Properties

        #region Summary

        /// <summary>
        /// Returns the summary being exported.
        /// </summary>
        public SummaryResult Summary
        {
            get { return this.summary; }
        }
        #endregion //Summary        

        #region SummaryLevel

        /// <summary>
        /// Returns the 0-based index of the current summary record.
        /// </summary>
        public int SummaryLevel
        {
            get { return this.summaryLevel; }
        }
        #endregion //SummaryLevel

        #endregion //Public Properties
    }
    #endregion //SummaryCellExportingEventArgs

    #region SummaryRowExportingEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.SummaryRowExporting"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class SummaryRowExportingEventArgs : ExcelExportCancelEventArgs
    {
        #region Private Members

        private IEnumerable<SummaryResult> summaries;        
        private int summaryLevel;

        #endregion //Private Members

        #region Constructor

        internal SummaryRowExportingEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            IEnumerable<SummaryResult> summaries, int summaryLevel, FormatSettings summaryFormatSettings)
            : base(exporterHelper, summaryFormatSettings)
        {
            this.summaries = summaries;            
            this.summaryLevel = summaryLevel;            
        }
        #endregion //Constructor

        #region Properties

        #region Summaries

        /// <summary>
        /// Returns the collection of summary values that are being exported.
        /// </summary>
        public IEnumerable<SummaryResult> Summaries
        {
            get { return this.summaries; }
        }
        #endregion //Summaries

        #region SummaryLevel

        /// <summary>
        /// Returns the 0-based index of the current summary record.
        /// </summary>
        public int SummaryLevel
        {
            get { return this.summaryLevel; }
        }
        #endregion //SummaryLevel

        #endregion //Properties
    }
    #endregion //SummaryAreaExportingEventArgs

    #region SummaryRowExportedEventArgs

    /// <summary>
    /// Event parameters used for the <see cref="DataPresenterExcelExporter.SummaryRowExported"/> event.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class SummaryRowExportedEventArgs : ExcelExportEventArgs
    {
        #region Private Members

        private IEnumerable<SummaryResult> summaries;
        private int summaryLevel;

        #endregion //Private Members

        #region Constructor

        internal SummaryRowExportedEventArgs(DataPresenterExcelExporterHelper exporterHelper,
            IEnumerable<SummaryResult> summaries, int summaryLevel)
            : base(exporterHelper)
        {
            this.summaries = summaries;
            this.summaryLevel = summaryLevel;
        }
        #endregion //Constructor

        #region Public Properties

        #region Summaries

        /// <summary>
        /// Returns the collection of summary values that are being exported.
        /// </summary>
        public IEnumerable<SummaryResult> Summaries
        {
            get { return this.summaries; }
        }
        #endregion //Summaries

        #region SummaryLevel

        /// <summary>
        /// Returns the 0-based index of the current summary record.
        /// </summary>
        public int SummaryLevel
        {
            get { return this.summaryLevel; }
        }
        #endregion //SummaryLevel

        #endregion //Public Properties
    }
    #endregion //SummaryRowExportedEventArgs
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