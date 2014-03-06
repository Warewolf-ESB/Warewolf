using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel;
using Infragistics.Windows.DataPresenter;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Licensing;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter.ExcelExporter
{
    /// <summary>
    /// A class providing Excel exporting functionality for <see cref="DataPresenterBase"/>-derived controls.
    /// </summary>
    
    
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class DataPresenterExcelExporter
		// AS 3/3/11 NA 2011.1 - Async Exporting
		: DependencyObject
    {
        #region Members

        private float _columnWidthUnitsInPixelX;

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Moved to the DataPresenterExportHelper
		//
		//private DataPresenterExportCache _exportCache;

        private bool _shouldCalculateMetrics = true;
        private UltraLicense _license;
        private float _twipsInPixelY;

		// AS 3/3/11 NA 2011.1 - Async Exporting
		private WeakList<DataPresenterExcelExporterHelper> _exporters;

        #endregion //Members

        #region APIs

        #region GetTextMetrics

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;

            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;

            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [DllImport("Gdi32", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC textmetric);

        #endregion //GetTextMetrics

        #endregion //APIs

        #region Constructor

        /// <summary>
        /// Instantiates a new DataPresenterExcelExporter.
        /// </summary>
        public DataPresenterExcelExporter()
        {
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(XamDataGrid), this) as UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }

			// AS 3/3/11 NA 2011.1 - Async Exporting
			_exporters = new WeakList<DataPresenterExcelExporterHelper>();
        }
        #endregion //Constructor

        #region Private Properties

		// AS 3/3/11 NA 2011.1 - Async Exporting
		//private DataPresenterExportCache ExportCache
		//{
		//    get
		//    {
		//        if (this._exportCache == null)
		//            this._exportCache = new DataPresenterExportCache();
		//
		//        return this._exportCache;
		//    }
		//}
		#endregion //Private Properties

		#region Public Properties

		#region AsyncExportDuration

		/// <summary>
		/// Identifies the <see cref="AsyncExportDuration"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public static readonly DependencyProperty AsyncExportDurationProperty = DependencyProperty.Register("AsyncExportDuration",
			typeof(TimeSpan), typeof(DataPresenterExcelExporter), new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(50)), new ValidateValueCallback(ValidateTimeSpan));

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
		/// <seealso cref="ExportAsync(DataPresenterBase, Worksheet, int, int, ExportOptions)"/>
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public TimeSpan AsyncExportDuration
		{
			get
			{
				return (TimeSpan)this.GetValue(DataPresenterExcelExporter.AsyncExportDurationProperty);
			}
			set
			{
				this.SetValue(DataPresenterExcelExporter.AsyncExportDurationProperty, value);
			}
		}

		#endregion //AsyncExportDuration

		#region AsyncExportInterval

		/// <summary>
		/// Identifies the <see cref="AsyncExportInterval"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public static readonly DependencyProperty AsyncExportIntervalProperty = DependencyProperty.Register("AsyncExportInterval",
			typeof(TimeSpan), typeof(DataPresenterExcelExporter), new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(20)), new ValidateValueCallback(ValidateTimeSpan));

		/// <summary>
		/// Returns or sets the amount of time to wait after performing an asynchronous export for the amount of time specified by the <see cref="AsyncExportDuration"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The TimeSpan must be greater than 0.</exception>
		/// <seealso cref="AsyncExportIntervalProperty"/>
		/// <seealso cref="AsyncExportDuration"/>
		/// <seealso cref="ShowAsyncExportStatus"/>
		/// <seealso cref="ExportAsync(DataPresenterBase, Worksheet, int, int, ExportOptions)"/>
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public TimeSpan AsyncExportInterval
		{
			get
			{
				return (TimeSpan)this.GetValue(DataPresenterExcelExporter.AsyncExportIntervalProperty);
			}
			set
			{
				this.SetValue(DataPresenterExcelExporter.AsyncExportIntervalProperty, value);
			}
		}

		#endregion //AsyncExportInterval

		#region ExcelCellFormatSettings

		/// <summary>
        /// Identifies the ExcelCellFormatSettings property, used to control the various export-related settings for cells.
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Cell"/>
        public static readonly DependencyProperty ExcelCellFormatSettingsProperty = DependencyProperty.RegisterAttached("ExcelCellFormatSettings",
            typeof(FormatSettings), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ExcelCellFormatSettings' attached property that is used to control the various export-related settings.
        /// </summary>        
        /// <seealso cref="ExcelCellFormatSettingsProperty"/>
        /// <seealso cref="SetExcelCellFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static FormatSettings GetExcelCellFormatSettings(DependencyObject d)
        {
            return (FormatSettings)d.GetValue(DataPresenterExcelExporter.ExcelCellFormatSettingsProperty);
        }

        /// <summary>
        /// Sets the value of the 'ExcelCellFormatSettings' attached property.
        /// </summary>
        /// <seealso cref="ExcelCellFormatSettingsProperty"/>
        /// <seealso cref="GetExcelCellFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static void SetExcelCellFormatSettings(DependencyObject d, FormatSettings value)
        {
            d.SetValue(DataPresenterExcelExporter.ExcelCellFormatSettingsProperty, value);
        }
        #endregion //ExcelCellFormatSettings

        #region ExcelGroupFormatSettings

        /// <summary>
        /// Identifies the ExcelGroupFormatSettings property, used to control the various export-related settings for GroupByRecords.
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
        public static readonly DependencyProperty ExcelGroupFormatSettingsProperty = DependencyProperty.RegisterAttached("ExcelGroupFormatSettings",
            typeof(FormatSettings), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ExcelGroupFormatSettings' attached property that is used to control the various export-related settings.
        /// </summary>        
        /// <seealso cref="ExcelGroupFormatSettingsProperty"/>
        /// <seealso cref="SetExcelGroupFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static FormatSettings GetExcelGroupFormatSettings(DependencyObject d)
        {
            return (FormatSettings)d.GetValue(DataPresenterExcelExporter.ExcelGroupFormatSettingsProperty);
        }

        /// <summary>
        /// Sets the value of the 'ExcelGroupFormatSettings' attached property.
        /// </summary>
        /// <seealso cref="ExcelGroupFormatSettingsProperty"/>
        /// <seealso cref="GetExcelGroupFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static void SetExcelGroupFormatSettings(DependencyObject d, FormatSettings value)
        {
            d.SetValue(DataPresenterExcelExporter.ExcelGroupFormatSettingsProperty, value);
        }
        #endregion //ExcelGroupFormatSettings

        #region ExcelLabelFormatSettings

        /// <summary>
        /// Identifies the ExcelLabelFormatSettings property, used to control the various export-related settings for labels.
        /// </summary>        
        public static readonly DependencyProperty ExcelLabelFormatSettingsProperty = DependencyProperty.RegisterAttached("ExcelLabelFormatSettings",
            typeof(FormatSettings), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ExcelLabelFormatSettings' attached property that is used to control the various export-related settings.
        /// </summary>        
        /// <seealso cref="ExcelLabelFormatSettingsProperty"/>
        /// <seealso cref="SetExcelLabelFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static FormatSettings GetExcelLabelFormatSettings(DependencyObject d)
        {
            return (FormatSettings)d.GetValue(DataPresenterExcelExporter.ExcelLabelFormatSettingsProperty);
        }

        /// <summary>
        /// Sets the value of the 'ExcelLabelFormatSettings' attached property.
        /// </summary>
        /// <seealso cref="ExcelLabelFormatSettingsProperty"/>
        /// <seealso cref="GetExcelLabelFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static void SetExcelLabelFormatSettings(DependencyObject d, FormatSettings value)
        {
            d.SetValue(DataPresenterExcelExporter.ExcelLabelFormatSettingsProperty, value);
        }
        #endregion //ExcelCellFormatSettings

        #region ExcelSummaryCellFormatSettings

        /// <summary>
        /// Identifies the ExcelSummaryCellFormatSettings property, used to control the various export-related settings for cells.
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Cell"/>
        public static readonly DependencyProperty ExcelSummaryCellFormatSettingsProperty = DependencyProperty.RegisterAttached("ExcelSummaryCellFormatSettings",
            typeof(FormatSettings), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ExcelSummaryCellFormatSettings' attached property that is used to control the various export-related settings.
        /// </summary>        
        /// <seealso cref="ExcelSummaryCellFormatSettingsProperty"/>
        /// <seealso cref="SetExcelSummaryCellFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static FormatSettings GetExcelSummaryCellFormatSettings(DependencyObject d)
        {
            return (FormatSettings)d.GetValue(DataPresenterExcelExporter.ExcelSummaryCellFormatSettingsProperty);
        }

        /// <summary>
        /// Sets the value of the 'ExcelSummaryCellFormatSettings' attached property.
        /// </summary>
        /// <seealso cref="ExcelSummaryCellFormatSettingsProperty"/>
        /// <seealso cref="GetExcelSummaryCellFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static void SetExcelSummaryCellFormatSettings(DependencyObject d, FormatSettings value)
        {
            d.SetValue(DataPresenterExcelExporter.ExcelSummaryCellFormatSettingsProperty, value);
        }
        #endregion //ExcelCellFormatSettings

        #region ExcelSummaryLabelFormatSettings

        /// <summary>
        /// Identifies the ExcelLabelFormatSettings property, used to control the various export-related settings for labels.
        /// </summary>        
        public static readonly DependencyProperty ExcelSummaryLabelFormatSettingsProperty = DependencyProperty.RegisterAttached("ExcelSummaryLabelFormatSettings",
            typeof(FormatSettings), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ExcelSummaryLabelFormatSettings' attached property that is used to control the various export-related settings.
        /// </summary>        
        /// <seealso cref="ExcelSummaryLabelFormatSettingsProperty"/>
        /// <seealso cref="SetExcelSummaryLabelFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static FormatSettings GetExcelSummaryLabelFormatSettings(DependencyObject d)
        {
            return (FormatSettings)d.GetValue(DataPresenterExcelExporter.ExcelSummaryLabelFormatSettingsProperty);
        }

        /// <summary>
        /// Sets the value of the 'ExcelSummaryLabelFormatSettings' attached property.
        /// </summary>
        /// <seealso cref="ExcelSummaryLabelFormatSettingsProperty"/>
        /// <seealso cref="GetExcelSummaryLabelFormatSettings"/>
        [AttachedPropertyBrowsableForType(typeof(FieldSettings))]
        public static void SetExcelSummaryLabelFormatSettings(DependencyObject d, FormatSettings value)
        {
            d.SetValue(DataPresenterExcelExporter.ExcelSummaryLabelFormatSettingsProperty, value);
        }
        #endregion //ExcelSummaryLabelFormatSettings

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region IsExporting

		private static readonly DependencyPropertyKey IsExportingPropertyKey =
			DependencyProperty.RegisterReadOnly("IsExporting",
			typeof(bool), typeof(DataPresenterExcelExporter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsExporting"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public static readonly DependencyProperty IsExportingProperty =
			IsExportingPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the exporter is in the process of an export operation.
		/// </summary>
		/// <seealso cref="IsExportingProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public bool IsExporting
		{
			get
			{
				return (bool)this.GetValue(DataPresenterExcelExporter.IsExportingProperty);
			}
			private set
			{
				this.SetValue(DataPresenterExcelExporter.IsExportingPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsExporting

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region ShowAsyncExportStatus

		/// <summary>
		/// Identifies the <see cref="ShowAsyncExportStatus"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public static readonly DependencyProperty ShowAsyncExportStatusProperty = DependencyProperty.Register("ShowAsyncExportStatus",
			typeof(bool), typeof(DataPresenterExcelExporter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns or sets a value that indicates whether a control containing the export status is displayed to the end user while an asynchronous export is in progress.
		/// </summary>
		/// <seealso cref="ShowAsyncExportStatusProperty"/>
		/// <seealso cref="AsyncExportDuration"/>
		/// <seealso cref="AsyncExportInterval"/>
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public bool ShowAsyncExportStatus
		{
			get
			{
				return (bool)this.GetValue(DataPresenterExcelExporter.ShowAsyncExportStatusProperty);
			}
			set
			{
				this.SetValue(DataPresenterExcelExporter.ShowAsyncExportStatusProperty, value);
			}
		}

		#endregion //ShowAsyncExportStatus

        #endregion //Public Properties

        #region Private/Internal Methods

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region AddExporter
		internal void AddExporter(DataPresenterExcelExporterHelper exporter)
		{
			_exporters.Add(exporter);
			this.IsExporting = _exporters.Count > 0;
		}
		#endregion //AddExporter

		#region AdjustPositionForRecordCollection

		private void AdjustPositionForRecordCollection(DataPresenterExcelExporterHelper exportHelper, Record record)
        {
            int newHieararchyLevel = DataPresenterExcelExporter.GetHierarchyLevel(record);
            if (newHieararchyLevel != exportHelper.HierarchyLevel)
            {
                exportHelper.currentPos.X = exportHelper.startPos.X + newHieararchyLevel;
                while (exportHelper.HierarchyLevel != newHieararchyLevel)
                {
					// MD 3/15/11 - TFS21053
					// Determine whether the current row should be collapsed.
					Record parentRecord = record.ParentRecord;
					bool collapsed = parentRecord != null && parentRecord.HasCollapsedAncestor;

                    if (exportHelper.HierarchyLevel < newHieararchyLevel)
					{
                        exportHelper.HierarchyLevel++;
					}
                    else
					{
                        exportHelper.HierarchyLevel--;

						// MD 3/15/11 - TFS21053
						// If we are decreasing the hierarchy level and the record is a root record, 
						// walk backwards over the rows and find the first sibling or parent of this outline group. 
						// If the sibling or parent is collapsed, so should this row.
						if (parentRecord == null)
						{
							for (int previousIndex = exportHelper.currentPos.Y - 1; previousIndex >= 0; previousIndex--)
							{
								WorksheetRow previousRow = exportHelper.CurrentWorksheet.Rows[previousIndex];
								if (previousRow.OutlineLevel <= exportHelper.HierarchyLevel)
								{
									collapsed = previousRow.Hidden;
									break;
								}
							}
						}
					}

					// MD 3/15/11 - TFS21053
					// Use the overload
					//this.SetCollapsedRow(exportHelper.CurrentWorksheet, exportHelper.currentPos.Y, exportHelper.HierarchyLevel,record, exportHelper /* AS 3/3/11 NA 2011.1 - Async Exporting */);
					this.SetCollapsedRow(exportHelper.CurrentWorksheet, exportHelper.currentPos.Y, exportHelper.HierarchyLevel, collapsed, exportHelper );
    
					if (exportHelper.ExportCache.ExportOptions.ChildRecordCollectionSpacing == ChildRecordCollectionSpacing.SingleRow)
                        this.IncreaseCurrentRow(exportHelper, 1);
                }
            }
            else if(record.FieldLayout != exportHelper.LastFieldLayout && exportHelper.LastExportedRecord != null)
            {
                // Account for the case where we have multiple siblings layouts at the same hieararchy level
                this.SetCollapsedRow(exportHelper.CurrentWorksheet, exportHelper.currentPos.Y, 
                    exportHelper.HierarchyLevel, record, exportHelper );

				if (exportHelper.ExportCache.ExportOptions.ChildRecordCollectionSpacing == ChildRecordCollectionSpacing.SingleRow)
                    this.IncreaseCurrentRow(exportHelper, 1);
            }
        }
        #endregion //AdjustPositionForRecordCollection

        #region BeginExportInternal

        internal void BeginExportInternal(DataPresenterExcelExporterHelper exporterHelper, DataPresenterBase dataPresenter, ExportOptions exportOptions)
        {
#pragma warning disable 618
            this.OnBeginExport(new BeginExportEventArgs(exporterHelper, dataPresenter));
#pragma warning restore 618

			// AS 2/11/11 NA 2011.1 Word Writer
			this.OnExportStarted(new ExportStartedEventArgs(exporterHelper, dataPresenter));

			exporterHelper.ExportCache.BuildReportInfoCache(dataPresenter, exportOptions);
        }
        #endregion //BeginExportInternal

        #region EndExportInternal

		// AS 3/10/11 NA 2011.1 - Async Exporting
		//internal void EndExportInternal(DataPresenterExcelExporterHelper exporterHelper, bool cancelled)
        internal void EndExportInternal(DataPresenterExcelExporterHelper exporterHelper, RecordExportCancellationInfo cancelInfo)
        {
			Debug.Assert(_exporters != null && _exporters.Contains(exporterHelper), "Was the operation already ended/cancelled?");

			// AS 3/3/11 NA 2011.1 - Async Exporting
			// Like the wordwriter, only invoke the ing if the begin was invoked.
			//
			if (exporterHelper.WasBeginExportInvoked)
			{
				// AS 2/11/11 NA 2011.1 Word Writer
				this.OnExportEnding(new ExportEndingEventArgs(exporterHelper, cancelInfo));

#pragma warning disable 618
				this.OnEndExport(new EndExportEventArgs(exporterHelper, cancelInfo != null));
#pragma warning restore 618
			}

			// AS 3/3/11 NA 2011.1 - Async Exporting
			this.RemoveExporter(exporterHelper);

			// AS 3/3/11 NA 2011.1 - Async Exporting
			// Since the cache is stored on the export helper we don't need to clear it.
			//
			//// Clear any information that we store during an export operation, since each export should
			//// start from a clean slate and we shouldn't be holding onto any objects that aren't required
			//// when we're done
            //this.ExportCache.Clear();

			// AS 2/11/11 NA 2011.1 Word Writer
			if (null != exporterHelper.FileName)
			{





				exporterHelper.Workbook.Save(exporterHelper.FileName);





			}

			// AS 2/11/11 NA 2011.1 Word Writer
			this.OnExportEnded(new ExportEndedEventArgs(exporterHelper, cancelInfo));
		}
        #endregion //EndExportInternal

        #region EnsureMetricsCalculated

        private void EnsureMetricsCalculated(DataPresenterBase dataPresenter)
        {
            // Bail out if we've already had an exception thrown when trying to use the 
            // unsafe methods to access metrics, otherwise we want to recalculate these
            // every time that we export in case the DPI has changed (though it's unlikely)
            if (!this._shouldCalculateMetrics)
                return;

            try
            {
                // Attempt to calculate the various metrics information based on
                // the system and application settings
                this.EnsureMetricsCalculatedUnsafe(dataPresenter);
            }
            catch
            {
                // Use a resonable default with the standard 96dpi
                this._twipsInPixelY = 1440 / 96;
                this._columnWidthUnitsInPixelX = 256.0f / 7;

                // Don't try to calculate these values again
                this._shouldCalculateMetrics = false;
            }
        }

        private void EnsureMetricsCalculatedUnsafe(DataPresenterBase dataPresenter)
        {
            HwndSource source = PresentationSource.FromVisual(dataPresenter) as HwndSource;
            if (source != null)
            {
                int dpiY = (int)(96 * source.CompositionTarget.TransformToDevice.M22);
                this._twipsInPixelY = 1440 / dpiY;

                TEXTMETRIC textMetric;
                GetTextMetrics(source.Handle, out textMetric);

                if (textMetric.tmAveCharWidth > 0)
                    this._columnWidthUnitsInPixelX = 256.0f / textMetric.tmAveCharWidth;
                else
                    this._columnWidthUnitsInPixelX = 256.0f / 7;
            }
            // MBS 9/16/09
            // Ensure that some default values are set
            else
            {
                this._twipsInPixelY = 1440 / 96.0f;
                this._columnWidthUnitsInPixelX = 256.0f / 7;
            }
        }
        #endregion //EnsureMetricsCalculated

		// MD 3/21/11 - TFS56731
		// Moved code from ProcessRecordInternal so it could be used in multiple places.
		#region ExportHeaders

		private void ExportHeaders(DataPresenterExcelExporterHelper exportHelper, Record record, int nestingOffset)
		{
			// Update the cache so that future attemps to determine whether the headers are visible can tell if we've 
			// already exported headers for the current record collection
			exportHelper.ExportCache.SetHeadersExportedForRecordCollection(record.ParentCollection, true);

			exportHelper.currentPos.X += nestingOffset;
			this.SetCurrentGridObjectSpan(exportHelper, record.FieldLayout, DataPresenterExportCache.PlaceholderType.ColumnHeaders);
			this.ProcessHeaderRows(exportHelper, record);
		}

		#endregion // ExportHeaders

        #region GetCellFormatFromFormatSettings

        private IWorksheetCellFormat GetCellFormatFromFormatSettings(DataPresenterExcelExporterHelper exportHelper, FormatSettings formatSettings,
            Field field, FieldLayout layout, bool isLabel)
        {
			// MD 5/31/11 - TFS75574
			bool hasNonDefaultValues = false;

            Workbook wb = exportHelper.CurrentWorksheet.Workbook;
            IWorksheetCellFormat cellFormat = wb.CreateNewWorksheetCellFormat();

            Color borderColor = GetColorFromWPFColor(formatSettings.BorderColor);
            if (borderColor != Color.Empty)
            {
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                cellFormat.LeftBorderColor =
                    cellFormat.RightBorderColor =
                    cellFormat.TopBorderColor =
                    cellFormat.BottomBorderColor = borderColor;
            }

            if (formatSettings.BorderStyle != CellBorderLineStyle.Default)
            {
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                cellFormat.LeftBorderStyle =
                    cellFormat.RightBorderStyle =
                    cellFormat.TopBorderStyle =
                    cellFormat.BottomBorderStyle = formatSettings.BorderStyle;
            }

            Color bottomBorderColor = GetColorFromWPFColor(formatSettings.BottomBorderColor);
			if (bottomBorderColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.BottomBorderColor = bottomBorderColor;
			}

			if (formatSettings.FillPattern != FillPatternStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.FillPattern = formatSettings.FillPattern;
			}

            Color fillPatternBackgroundColor = GetColorFromWPFColor(formatSettings.FillPatternBackgroundColor);
			if (fillPatternBackgroundColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.FillPatternBackgroundColor = fillPatternBackgroundColor;
			}

            Color fillPatternForegroundColor = GetColorFromWPFColor(formatSettings.FillPatternForegroundColor);
			if (fillPatternForegroundColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.FillPatternForegroundColor = fillPatternForegroundColor;
			}

			if (formatSettings.BottomBorderStyle != CellBorderLineStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.BottomBorderStyle = formatSettings.BottomBorderStyle;
			}

            IWorkbookFont font = wb.CreateNewWorkbookFont();
            Color fontColor = GetColorFromWPFColor(formatSettings.FontColor);
			if (fontColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				font.Color = fontColor;
			}

			if (formatSettings.FontFamily != null)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				font.Name = formatSettings.FontFamily.Source;
			}

            if (formatSettings.FontSize.HasValue)
            {
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                DeviceUnitLength fontSize = formatSettings.FontSize.Value;
                font.Height = (int)fontSize.ConvertToUnitType(DeviceUnitType.Twip);
            }

			if (formatSettings.FontStrikeout != ExcelDefaultableBoolean.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				font.Strikeout = formatSettings.FontStrikeout;
			}

            if (formatSettings.FontStyle != null)
            {
				if (formatSettings.FontStyle == System.Windows.FontStyles.Italic)
				{
					// MD 5/31/11 - TFS75574
					hasNonDefaultValues = true;

					font.Italic = ExcelDefaultableBoolean.True;
				}
				// MD 5/31/11 - TFS75574
				// Leave the value as default in this case.
				//else
				//    font.Italic = ExcelDefaultableBoolean.False;
            }

			if (formatSettings.FontSuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				font.SuperscriptSubscriptStyle = formatSettings.FontSuperscriptSubscriptStyle;
			}

			if (formatSettings.FontUnderlineStyle != FontUnderlineStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				font.UnderlineStyle = formatSettings.FontUnderlineStyle;
			}

            if (formatSettings.FontWeight != null)
            {
                if (formatSettings.FontWeight == FontWeights.Black ||
                   formatSettings.FontWeight == FontWeights.Bold ||
                    formatSettings.FontWeight == FontWeights.DemiBold ||
                    formatSettings.FontWeight == FontWeights.ExtraBlack ||
                    formatSettings.FontWeight == FontWeights.ExtraBold ||
                    formatSettings.FontWeight == FontWeights.Heavy ||
                    formatSettings.FontWeight == FontWeights.Medium ||
                    formatSettings.FontWeight == FontWeights.SemiBold)
				{
					// MD 5/31/11 - TFS75574
					hasNonDefaultValues = true;

                    font.Bold = ExcelDefaultableBoolean.True;
				}
				// MD 5/31/11 - TFS75574
				// Leave the value as default in this case.
				//else
				//    font.Bold = ExcelDefaultableBoolean.False;
            }

            cellFormat.Font.SetFontFormatting(font);

			if (formatSettings.FormatString != null)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.FormatString = formatSettings.FormatString;
			}

            if (formatSettings.HorizontalAlignment != HorizontalCellAlignment.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                cellFormat.Alignment = formatSettings.HorizontalAlignment;
			}
            else
            {
                // As an ultimate default, if we're currently exporting labels that are positioned within
                // the cells, we should honor the alignment of those labels
                if (isLabel && layout.LabelLocationResolved == LabelLocation.InCells)
                {
                    if (field != null)
                    {
                        switch (field.CellContentAlignmentResolved)
                        {
                            case CellContentAlignment.LabelAboveValueAlignLeft:
                            case CellContentAlignment.LabelBelowValueAlignLeft:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.Alignment = HorizontalCellAlignment.Left;
                                break;

                            case CellContentAlignment.LabelAboveValueAlignCenter:
                            case CellContentAlignment.LabelAboveValueStretch:
                            case CellContentAlignment.LabelBelowValueAlignCenter:
                            case CellContentAlignment.LabelBelowValueStretch:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.Alignment = HorizontalCellAlignment.Center;
                                break;

                            case CellContentAlignment.LabelAboveValueAlignRight:
                            case CellContentAlignment.LabelBelowValueAlignRight:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.Alignment = HorizontalCellAlignment.Right;
                                break;
                        }
                    }
                }
            }

			if (formatSettings.Indent.HasValue)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.Indent = formatSettings.Indent.Value;
			}

            Color leftBorderColor = GetColorFromWPFColor(formatSettings.LeftBorderColor);
			if (leftBorderColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.LeftBorderColor = leftBorderColor;
			}

			if (formatSettings.LeftBorderStyle != CellBorderLineStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.LeftBorderStyle = formatSettings.LeftBorderStyle;
			}

			if (formatSettings.Locked != ExcelDefaultableBoolean.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.Locked = formatSettings.Locked;
			}

            Color rightBorderColor = GetColorFromWPFColor(formatSettings.RightBorderColor);
			if (rightBorderColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.RightBorderColor = rightBorderColor;
			}

			if (formatSettings.RightBorderStyle != CellBorderLineStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.RightBorderStyle = formatSettings.RightBorderStyle;
			}

			if (formatSettings.Rotation.HasValue)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.Rotation = formatSettings.Rotation.Value;
			}

			if (formatSettings.ShrinkToFit != ExcelDefaultableBoolean.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.ShrinkToFit = formatSettings.ShrinkToFit;
			}

            Color topBorderColor = GetColorFromWPFColor(formatSettings.TopBorderColor);
			if (topBorderColor != Color.Empty)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.TopBorderColor = topBorderColor;
			}

			if (formatSettings.TopBorderStyle != CellBorderLineStyle.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

				cellFormat.TopBorderStyle = formatSettings.TopBorderStyle;
			}

            if (formatSettings.VerticalAlignment != VerticalCellAlignment.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                cellFormat.VerticalAlignment = formatSettings.VerticalAlignment;
			}
            else
            {
                // As an ultimate default, if we're currently exporting labels that are positioned within
                // the cells, we should honor the alignment of those labels
                if (isLabel && layout.LabelLocationResolved == LabelLocation.InCells)
                {
                    if (field != null)
                    {
                        switch (field.CellContentAlignmentResolved)
                        {
                            case CellContentAlignment.LabelLeftOfValueAlignTop:
                            case CellContentAlignment.LabelRightOfValueAlignTop:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.VerticalAlignment = VerticalCellAlignment.Top;
                                break;

                            case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                            case CellContentAlignment.LabelLeftOfValueStretch:
                            case CellContentAlignment.LabelRightOfValueAlignMiddle:
                            case CellContentAlignment.LabelRightOfValueStretch:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.VerticalAlignment = VerticalCellAlignment.Center;
                                break;

                            case CellContentAlignment.LabelLeftOfValueAlignBottom:
                            case CellContentAlignment.LabelRightOfValueAlignBottom:
								// MD 5/31/11 - TFS75574
								hasNonDefaultValues = true;

                                cellFormat.VerticalAlignment = VerticalCellAlignment.Bottom;
                                break;
                        }
                    }
                }
            }

            if (formatSettings.WrapText != ExcelDefaultableBoolean.Default)
			{
				// MD 5/31/11 - TFS75574
				hasNonDefaultValues = true;

                cellFormat.WrapText = formatSettings.WrapText;
			}

			// MD 5/31/11 - TFS75574
			// If the format is all default values, return null so we don't set anything on the cell.
			if (hasNonDefaultValues == false)
				return null;

            return cellFormat;
        }
        #endregion //GetCellFormatFromFormatSettings

        #region GetColorFromWPFColor

        private static Color GetColorFromWPFColor(System.Windows.Media.Color? wpfColorNullable)
        {
            if (!wpfColorNullable.HasValue)
                return Color.Empty;

            System.Windows.Media.Color wpfColor = wpfColorNullable.Value;
            return Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);
        }
        #endregion //GetColorFromWPFColor

		// MD 3/21/11 - TFS56731
		// Moved code from ProcessRecordInternal so it could be used in multiple places.
		#region GetFirstGroupByChildRecord

		private static DataRecord GetFirstGroupByChildRecord(Record record, out int nestingOffset)
		{
			// When we're exporting a summary or headers, we need to check to see if we're dealing with positioning the 
			// summaries above a GroupByRow, since in this case we need to do some additional calculations
			// so that the summaries/headers will line up with the correct columns within the GroupByRecords.
			nestingOffset = 0;
			DataRecord firstGroupByChildRecord = null;
			if (record.RecordType != RecordType.DataRecord && record.RecordType != RecordType.ExpandableFieldRecord)
			{
				IList<DataRecord> unsortedRecords = record.RecordManager.Unsorted;
				if (unsortedRecords != null && unsortedRecords.Count > 0)
				{
					firstGroupByChildRecord = unsortedRecords[0];
					nestingOffset = firstGroupByChildRecord.NestingDepth - record.NestingDepth;
				}
			}

			return firstGroupByChildRecord;
		}

		#endregion // GetFirstGroupByChildRecord

        #region GetHierarchyLevel

        private static int GetHierarchyLevel(Record record)
        {
            Record parentRecord = record.ParentRecord;
            if (parentRecord == null)
                return 0;

            // Don't treat an ExpandableFieldRecord as a hierarchy level, since it's 
            // really only responsible for managing the child records
            if (parentRecord is ExpandableFieldRecord)
                return GetHierarchyLevel(parentRecord);

            return GetHierarchyLevel(record.ParentRecord) + 1;
        }
        #endregion //GetHierarchyLevel

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

        #region GetSummariesForLevel

        #endregion //GetSummariesForLevel

        #region IncreaseCurrentRow

        private void IncreaseCurrentRow(DataPresenterExcelExporterHelper exportHelper, int amount)
        {
            exportHelper.currentPos.Y += amount;
            exportHelper.currentPos.X = exportHelper.startPos.X + exportHelper.HierarchyLevel;
        }
        #endregion //IncreaseCurrentRow

		// MD 6/7/10 - ChildRecordsDisplayOrder feature
		// Moved this code from ProcessRecordInternal so it could be done separately from the record processing.
		#region InitializeRecordInternal

		internal bool InitializeRecordInternal(DataPresenterExcelExporterHelper exportHelper, Record record, ProcessRecordParams processRecordParams)
		{
			// MBS 9/11/09
			// It's possible that we haven't yet built any cache information for a FieldLayout, such as if
			// the DataPresenter hadn't created any FieldLayouts until the record was generated, so we should
			// ensure that we've done so here
			exportHelper.ExportCache.VerifyReportInfoCache(record.FieldLayout);

			// Set the default event args if the record won't be processed so that the user can tell
			// that we're not processing the record by default, but has the chance to change it
			InitializeRecordEventArgs initializeRecordEA = new InitializeRecordEventArgs(exportHelper, processRecordParams, record);
			if (record.VisibilityResolved == Visibility.Collapsed)
			{
				initializeRecordEA.SkipRecord = true;
				initializeRecordEA.SkipDescendants = true;
			}

			// We want to fire the InitializeRecord event before we do any sort of processing, including determining if we
			// should export headers, since the user could set the Visibility of a record to be Hidden or Collapsed.
			this.OnInitializeRecord(initializeRecordEA);
			if (initializeRecordEA.TerminateExport || initializeRecordEA.SkipRecord ||
				initializeRecordEA.Record.VisibilityResolved == Visibility.Collapsed)
			{
				this.AdjustPositionForRecordCollection(exportHelper, record);
				return false;
			}

			// MD 3/21/11 - TFS56731
			// If the ChildRecordsDisplayOrderResolved is BeforeParent, we need to write out the headers before any children are exported.
			if (record.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParent &&
				this.ShouldDisplayHeadersAboveRecord(exportHelper, record))
			{
				int nestingOffset;
				DataPresenterExcelExporter.GetFirstGroupByChildRecord(record, out nestingOffset);

				this.ExportHeaders(exportHelper, record, nestingOffset);
			}

			return true;
		} 

		#endregion // InitializeRecordInternal

		// AS 2/11/11 NA 2011.1 Word Writer
		#region PrepareForExport
		private DataPresenterExcelExporterHelper PrepareForExport(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn, ref ExportOptions options, string fileName)
		{
			if (dataPresenter == null)
				throw new ArgumentNullException("dataPresenter");

			// MBS 9/16/09
			// Don't allow the user to try to export the data presenter unless it has been initialized, since 
			// the record managers and other elements will not have been created.
			if (!dataPresenter.IsInitialized)
				throw new InvalidOperationException(DataPresenterExcelExporter.GetString("LER_DataPresenterNotInitialized"));

			if (worksheet == null)
				throw new ArgumentNullException("worksheet");

			if (startRow < 0 || startRow >= Workbook.GetMaxRowCount(worksheet.Workbook.CurrentFormat))
				throw new ArgumentOutOfRangeException("row", startRow, DataPresenterExcelExporter.GetString("LER_ArgumentOutOfRangeException_1"));

			if (startColumn < 0 || startColumn >= Workbook.GetMaxColumnCount(worksheet.Workbook.CurrentFormat))
				throw new ArgumentOutOfRangeException("column", startColumn, DataPresenterExcelExporter.GetString("LER_ArgumentOutOfRangeException_2"));

			DataPresenterExcelExporterHelper exporterHelper = new DataPresenterExcelExporterHelper(
				this, worksheet, startRow, startColumn,
				fileName, dataPresenter );

			// If the user didn't specify any options to use, create a default set
			if (options == null)
				options = new ExportOptions();

			// Attempt to calculate any metrics now based on the original data presenter, since it should actually
			// have a handle.
			this.EnsureMetricsCalculated(dataPresenter);

			return exporterHelper;
		}
		#endregion //PrepareForExport

        #region ProcessHeaderRows

        private void ProcessHeaderRows(DataPresenterExcelExporterHelper exportHelper, Record record)
        {
			FormatSettings headerFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutLabelFormatSettings(record.FieldLayout);
            HeaderAreaExportingEventArgs headerAreaExportingEA = new HeaderAreaExportingEventArgs(exportHelper, record.FieldLayout, record, headerFormatSettings);
            this.OnHeaderAreaExporting(headerAreaExportingEA);
            if (headerAreaExportingEA.Cancel)
                return;

            exportHelper.extentOfCurrentGridObject.Location = exportHelper.currentPos;

			ExportLayoutInformation layoutInfo = exportHelper.ExportCache.GetDocumentLabelLayoutInfo(record.FieldLayout);
            this.SetColumnWidthsAndRowHeights(exportHelper, layoutInfo.GetColumnWidths(), layoutInfo.GetRowHeights());

            foreach (Field field in record.FieldLayout.Fields)
            {
                Visibility fieldVisibility = field.VisibilityResolved;
                if (fieldVisibility == Visibility.Collapsed)
                    continue;

                FieldPosition? fieldPosNullable = layoutInfo.GetFieldPosition(field, true);
                if (fieldPosNullable.HasValue)
                {
                    bool setValue = true;
                    if (fieldVisibility == Visibility.Hidden)
                        setValue = false;

                    FieldPosition fieldPosition = fieldPosNullable.Value;
                    Rectangle relativeRect = new Rectangle(fieldPosition.Column, fieldPosition.Row, fieldPosition.ColumnSpan, fieldPosition.RowSpan);
                    this.WriteHeaderCell(exportHelper, record, field, relativeRect, headerAreaExportingEA.FormatSettingsInternal, setValue);
                }
            }

            this.SetCollapsedRows(
                exportHelper.CurrentWorksheet,
                exportHelper.extentOfCurrentGridObject.Y,
                exportHelper.extentOfCurrentGridObject.Bottom - 1,
                exportHelper.HierarchyLevel,
				// MD 3/15/11 - TFS21053
				// This parameter determines if the row is hidden, but it isn't enough to just check IsExpanded,
				// because the record's parents could be expanded but under a collapsed ancestor, in which case
				// this record should be hidden. so used HasCollapsedAncestor instead.
                //record.ParentRecord != null && !record.ParentRecord.IsExpanded,
				record.ParentRecord != null && record.ParentRecord.HasCollapsedAncestor,
				exportHelper );

            exportHelper.currentPos = exportHelper.extentOfCurrentGridObject.Location;
            this.IncreaseCurrentRow(exportHelper, exportHelper.extentOfCurrentGridObject.Height);

            HeaderAreaExportedEventArgs headerAreaExportedEA = new HeaderAreaExportedEventArgs(exportHelper, record.FieldLayout, record);
            this.OnHeaderAreaExported(headerAreaExportedEA);
        }
        #endregion //ProcessHeaderRows

        #region ProcessRecordInternal

        internal bool ProcessRecordInternal(DataPresenterExcelExporterHelper exportHelper, Record record, ProcessRecordParams processRecordParams)
        {
			// MD 6/7/10 - ChildRecordsDisplayOrder feature
			// Moved this code to InitializeRecordInternal so it could be done separately from the record processing.
			//// MBS 9/11/09
			//// It's possible that we haven't yet built any cache information for a FieldLayout, such as if
			//// the DataPresenter hadn't created any FieldLayouts until the record was generated, so we should
			//// ensure that we've done so here
			//this.ExportCache.VerifyReportInfoCache(record.FieldLayout);
			//
			//this.AdjustPositionForRecordCollection(exportHelper, record);
			//
			//// Set the default event args if the record won't be processed so that the user can tell
			//// that we're not processing the record by default, but has the chance to change it
			//InitializeRecordEventArgs initializeRecordEA = new InitializeRecordEventArgs(exportHelper, processRecordParams, record);
			//if (record.VisibilityResolved == Visibility.Collapsed)
			//{
			//    initializeRecordEA.SkipRecord = true;
			//    initializeRecordEA.SkipDescendants = true;
			//}
			//
			//// We want to fire the InitializeRecord event before we do any sort of processing, including determining if we
			//// should export headers, since the user could set the Visibility of a record to be Hidden or Collapsed.
			//this.OnInitializeRecord(initializeRecordEA);
			//if (initializeRecordEA.TerminateExport || initializeRecordEA.SkipRecord ||                 
			//    initializeRecordEA.Record.VisibilityResolved == Visibility.Collapsed)
			//    return false;
			this.AdjustPositionForRecordCollection(exportHelper, record);

			// MD 3/21/11 - TFS56731
			// Moved this code to GetFirstGroupByChildRecord so it could be used in multiple places.
			//// When we're exporting a summary or headers, we need to check to see if we're dealing with positioning the 
			//// summaries above a GroupByRow, since in this case we need to do some additional calculations
			//// so that the summaries/headers will line up with the correct columns within the GroupByRecords.
			//int nestingOffset = 0;
			//DataRecord firstGroupByChildRecord = null;
			//if (record.RecordType != RecordType.DataRecord && record.RecordType != RecordType.ExpandableFieldRecord)
			//{
			//    IList<DataRecord> unsortedRecords = record.RecordManager.Unsorted;
			//    if (unsortedRecords != null && unsortedRecords.Count > 0)
			//    {
			//        firstGroupByChildRecord = unsortedRecords[0];
			//        nestingOffset = firstGroupByChildRecord.NestingDepth - record.NestingDepth;
			//    }
			//}
			int nestingOffset;
			DataRecord firstGroupByChildRecord = DataPresenterExcelExporter.GetFirstGroupByChildRecord(record, out nestingOffset);

			// MD 3/21/11 - TFS56731
			// Moved this code to GetFirstGroupByChildRecord so it could be used in multiple places.
			// Also, only export the headers if ChildRecordsDisplayOrderResolved is not BeforeParent. 
			// If it is, we will export the headers in InitializeRecordInternal.
			//if (this.ShouldDisplayHeadersAboveRecord(exportHelper, record))
			//{
			//    // Update the cache so that future attemps to determine whether the headers are visible can tell if we've 
			//    // already exported headers for the current record collection
			//    exportHelper.ExportCache.SetHeadersExportedForRecordCollection(record.ParentCollection, true);
			//
			//    exportHelper.currentPos.X += nestingOffset;
			//    this.SetCurrentGridObjectSpan(exportHelper, record.FieldLayout, DataPresenterExportCache.PlaceholderType.ColumnHeaders);
			//    this.ProcessHeaderRows(exportHelper, record);
			//}
			if (record.FieldLayout.ChildRecordsDisplayOrderResolved != ChildRecordsDisplayOrder.BeforeParent &&
				this.ShouldDisplayHeadersAboveRecord(exportHelper, record))
			{
				this.ExportHeaders(exportHelper, record, nestingOffset);
			}

            if (record is GroupByRecord)
            {
                exportHelper.extentOfCurrentGridObject.Height = 1;

                // We want to make the GroupByRecord span the width of the child records instead of having a single column
                // that ends up being wide enough to fit all of the description
                if(firstGroupByChildRecord != null)
                {
					ExportLayoutInformation layoutInfo = exportHelper.ExportCache.GetDocumentFieldLayoutInfo(firstGroupByChildRecord.FieldLayout);
                    int extentOfChildRecord = layoutInfo.GetColumnWidths().Length;
                    exportHelper.extentOfCurrentGridObject.Width = nestingOffset + extentOfChildRecord;
                }
                else
                    exportHelper.extentOfCurrentGridObject.Width = 1;
            }
            else
                this.SetCurrentGridObjectSpan(exportHelper, record.FieldLayout, DataPresenterExportCache.PlaceholderType.Values);

            if (record.RecordType == RecordType.SummaryRecord)
            {
                exportHelper.currentPos.X += nestingOffset;

				FormatSettings summaryCellFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutSummaryCellFormatSettings(record.FieldLayout);
				FormatSettings summaryLabelFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutSummaryLabelFormatSettings(record.FieldLayout);
                this.ProcessSummaries(exportHelper, record, ((SummaryRecord)record).SummaryResults, summaryCellFormatSettings, summaryLabelFormatSettings);

                exportHelper.currentPos.X -= nestingOffset;
            }
            else            
                this.ProcessSingleRecord(exportHelper, record, nestingOffset);

            return true;
        }
        #endregion //ProcessRecordInternal

        #region ProcessSingleRecord

        private void ProcessSingleRecord(DataPresenterExcelExporterHelper exportHelper, Record record, int nestingOffset)
        {
            // we don't need to do anything at this point since a Collapsed visibility means that we don't
            // have to allocate the space that the record would normally occupy.
            Visibility recordVisibility = record.VisibilityResolved;
            if (recordVisibility == Visibility.Collapsed)
                return;

            FormatSettings resolvedFieldLayoutFormatSettings = null;
            GroupByRecord groupByRecord = record as GroupByRecord;
            if (groupByRecord != null)
            {
				resolvedFieldLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutGroupFormatSettings(record.FieldLayout);

                // MBS 9/18/09
                // Need to take into account field-specific format settings
                Field groupByField = groupByRecord.GroupByField;
                if (groupByField != null)
					resolvedFieldLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldGroupByFormatSettings(groupByField, resolvedFieldLayoutFormatSettings);
            }
            else
				resolvedFieldLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutCellFormatSettings(record.FieldLayout);

            RecordExportingEventArgs rowExportingEA = new RecordExportingEventArgs(exportHelper, record, resolvedFieldLayoutFormatSettings);
            this.OnRecordExporting(rowExportingEA);
            if (rowExportingEA.Cancel)
                return;

            exportHelper.extentOfCurrentGridObject.Location = exportHelper.currentPos;

            // Only bother exporting the data in the row if it's visible.  When it's Hidden, we just want to carve
            // out the space for the row.
            if (recordVisibility == Visibility.Visible)
            {
                if (groupByRecord != null)
                    this.WriteGroupByRecord(exportHelper, groupByRecord, rowExportingEA.FormatSettingsInternal, nestingOffset);
                else
                    this.WriteSingleRecord(exportHelper, record, rowExportingEA.FormatSettingsInternal);
            }

            this.SetCollapsedRows(
                exportHelper.CurrentWorksheet,
                exportHelper.extentOfCurrentGridObject.Y,
                exportHelper.extentOfCurrentGridObject.Bottom - 1,
                exportHelper.HierarchyLevel,
				// MD 3/15/11 - TFS21053
				// This parameter determines if the row is hidden, but it isn't enough to just check IsExpanded,
				// because the record's parents could be expanded but under a collapsed ancestor, in which case
				// this record should be hidden. so used HasCollapsedAncestor instead.
                //record.ParentRecord != null && !record.ParentRecord.IsExpanded,
				record.ParentRecord != null && record.ParentRecord.HasCollapsedAncestor,
				exportHelper );

            exportHelper.currentPos = exportHelper.extentOfCurrentGridObject.Location;
            this.IncreaseCurrentRow(exportHelper, exportHelper.extentOfCurrentGridObject.Height);

            RecordExportedEventArgs recordExportedEA = new RecordExportedEventArgs(exportHelper, record);
            this.OnRecordExported(recordExportedEA);
        }
        #endregion //ProcessSingleRecord

        #region ProcessSummaries

        private void ProcessSummaries(DataPresenterExcelExporterHelper exportHelper, Record associatedRecord, IEnumerable<SummaryResult> summaries,
            FormatSettings cellFormatSettings, FormatSettings labelFormatSettings)
        {
            FieldLayout layout = associatedRecord.FieldLayout;
            bool includeLables = layout.LabelLocationResolved == LabelLocation.InCells;
			ExportLayoutInformation layoutInfo = exportHelper.ExportCache.GetDocumentFieldLayoutInfo(layout);

            // When we're exporting summary rows, we should always be starting at the same X position as
            // we do for the first row.  The IncreaseCurrentRow method will reset the X position, which is
            // wrong in this particular case because all of the summaries are specified in relative positions
            // to the original column.  We will keep track of the X position here to ensure that all summary
            // positions are calculated rkelative to the same position
            int startXPos = exportHelper.currentPos.X;

            // Also keep track of the starting Y position so that we can correctly set the collapsed rows
            int startYPos = exportHelper.currentPos.Y;

            // Since we can have summaries that are positioned on different DataPresenter rows, but will still begin at the same logical column,
            // we need to keep a list of the summaries that line up with each DataPresenter row.  Each DataPresenter row can actually end up
            // spanning multiple Excel rows, such as if there are multiple summaries.  The reason for manipulating the summaries in this fashion
            // is that we need to be able to fire an event telling the user that we're exporting a summary row (not record), but each Field's 
            // summaries are just an enumerable list.
            List<List<List<object>>> summaryRowList = new List<List<List<object>>>();

            // Keep track of the labels that we've added to the list so that we don't export the same label multiple times
            Dictionary<Field, int> processedLabels = new Dictionary<Field, int>();

            // Generate the list of rows that the summaries will span            
            foreach (SummaryResult summary in summaries)
            {
				// MD 5/9/12 - TFS110527
				// If the summary result should not be displayed, we shouldn't export it.
				if (summary.DisplayAreaResolved == SummaryDisplayAreas.None)
					continue;

                Field summaryField = summary.PositionFieldResolved;
                this.ProcessSummariesHelper(summary, false, layoutInfo.GetFieldPosition(summaryField, false), ref summaryRowList);

                // If we need to display the labels in the cells with the summaries, we should include them
                // when determining the number of logical rows to export
                if (includeLables)
                {
                    int labelSummaryCount;
                    if (processedLabels.TryGetValue(summaryField, out labelSummaryCount) == false)
                    {
                        this.ProcessSummariesHelper(summary, true, layoutInfo.GetFieldPosition(summaryField, true), ref summaryRowList);
                        processedLabels.Add(summaryField, 1);
                    }
                    else
                        processedLabels[summaryField] = labelSummaryCount + 1;
                }
            }

			// MD 5/9/12 - TFS110527
			// If all summaries are hidden, return here.
			if (summaryRowList.Count == 0)
				return;

            // Set the height to 1, since we manually position each summary in its own cell
            exportHelper.extentOfCurrentGridObject.Height = 1;

            // Keep track of the current summary level total, since this needs to be provided
            // to the user in the various events
            int currentSummaryLevel = 0;

            for (int currentRow = 0; currentRow < summaryRowList.Count; currentRow++)
            {
                // We need to know what the maximum number of summaries is for the current FieldPosition.Row                
                int numLogicalRows = 0;
                int summariesInCurrentRow = 0;

                List<List<object>> summaryColumnList = summaryRowList[currentRow];
                foreach (List<object> summaryList in summaryColumnList)
                {
                    if (summaryList != null)
                    {
                        int tmpSummariesInCurrentRow = 0;
                        for (int x = 0; x < summaryList.Count; x++)
                        {
                            SummaryResult summary = summaryList[x] as SummaryResult;
                            if (summary != null)
                            {
                                FieldPosition position = layoutInfo.GetFieldPosition(summary.PositionFieldResolved, false).Value;
                                if (position.Row == currentRow && position.RowSpan == 1)
                                    tmpSummariesInCurrentRow++;
                            }
                        }

                        summariesInCurrentRow = Math.Max(summariesInCurrentRow, tmpSummariesInCurrentRow);
                        numLogicalRows = Math.Max(numLogicalRows, summaryList.Count);
                    }
                }

                for (int currentLogicalRow = 0; currentLogicalRow < numLogicalRows; currentLogicalRow++)
                {
                    SummaryRowExportingEventArgs summaryAreaExportingEA = new SummaryRowExportingEventArgs(exportHelper, summaries, currentSummaryLevel,
                        // Create a new FormatSettings object that we use here so that the user can specify a unique appearaence
                        // for the summary area that can be merged with both the label settings and the cell settings.
                        new FormatSettings());

                    this.OnSummaryRowExporting(summaryAreaExportingEA);

                    if (!summaryAreaExportingEA.Cancel)
                    {
                        FormatSettings resolvedCellSettings = DataPresenterExportCache.MergeFormatSettings(summaryAreaExportingEA.FormatSettingsInternal, cellFormatSettings);
                        FormatSettings resolvedLabelSettings = DataPresenterExportCache.MergeFormatSettings(summaryAreaExportingEA.FormatSettingsInternal, labelFormatSettings);

                        exportHelper.extentOfCurrentGridObject.Location = exportHelper.currentPos;
                        for (int currentColumn = 0; currentColumn < summaryColumnList.Count; currentColumn++)
                        {
                            List<object> columnSummaries = summaryColumnList[currentColumn];
                            if (columnSummaries == null || columnSummaries.Count <= currentLogicalRow)
                                continue;

                            SummaryResult summary = columnSummaries[currentLogicalRow] as SummaryResult;
                            if (summary != null)
                            {
                                FieldPosition summaryPosition = layoutInfo.GetFieldPosition(summary.PositionFieldResolved, false).Value;
                                Rectangle relativeRect = new Rectangle(summaryPosition.Column, 0, summaryPosition.ColumnSpan, 1);

                                exportHelper.currentPos.X = exportHelper.extentOfCurrentGridObject.X + relativeRect.X;
                                exportHelper.currentPos.Y = exportHelper.extentOfCurrentGridObject.Y + relativeRect.Y;

                                // The cells should be picking up their defaults from the non-summary cells
								FormatSettings cellLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutCellFormatSettings(layout);
								FormatSettings cellFieldFormatSettings = exportHelper.ExportCache.GetResolvedFieldCellFormatSettings(summary.SourceField, cellLayoutFormatSettings);
                                FormatSettings resolvedFormatSettings = DataPresenterExportCache.MergeFormatSettings(cellFieldFormatSettings, resolvedCellSettings);

                                // MBS 9/18/09
                                // Need to take into account field-specific settings
								resolvedFormatSettings = exportHelper.ExportCache.GetResolvedFieldSummaryCellFormatSettings(summary.PositionFieldResolved, resolvedFormatSettings);                                

                                SummaryCellExportingEventArgs summaryCellExportingEA = new SummaryCellExportingEventArgs(exportHelper, summary, currentSummaryLevel, resolvedFormatSettings);
                                this.OnSummaryCellExporting(summaryCellExportingEA);
                                if (summaryCellExportingEA.Cancel)
                                    continue;

                                object val = summary.DisplayText;

                                

                                IWorksheetCellFormat cellFormat = this.GetCellFormatFromFormatSettings(exportHelper, summaryCellExportingEA.FormatSettingsInternal, summary.SourceField, layout, false);
                                this.SetRegionRelativeToOrigin(exportHelper, relativeRect, val, cellFormat);

                                SummaryCellExportedEventArgs summaryCellExportedEA = new SummaryCellExportedEventArgs(exportHelper, summary, currentSummaryLevel);
                                this.OnSummaryCellExported(summaryCellExportedEA);
                            }
                            else
                            {
                                // We have labels to render within the summary area, so export them through the normal header exporting mechanisms
                                Field summaryField = columnSummaries[currentLogicalRow] as Field;
                                if (summaryField != null)
                                {
                                    

                                    FieldPosition labelPosition = layoutInfo.GetFieldPosition(summaryField, true).Value;

                                    // If a label is in this row and has a span that's greater than 1, it will actually be
                                    // increased to line up with the maximum number of summaries so that its own summaries
                                    // will start on the next 'row'.
                                    int rowSpan = labelPosition.RowSpan;
                                    if (rowSpan > 1)
                                    {
                                        // At the least the label should span the logical rows that this main row spans.
                                        rowSpan = Math.Max(rowSpan, numLogicalRows);

                                        // Check the total number of summaries for the other fields that we cached earlier
                                        foreach (List<object> tmpColumnSummaries in summaryColumnList)
                                        {
                                            if (tmpColumnSummaries == columnSummaries || tmpColumnSummaries.Count <= currentLogicalRow)
                                                continue;

                                            Field tmpField = tmpColumnSummaries[currentLogicalRow] as Field;
                                            if (tmpField == null)
                                                continue;

                                            // We could have a label with a smaller row span that forces all the other summaries to
                                            // be shifted downward, so take that into account.  Note that we at least add 1 to 
                                            // account for the amount of space the label will occupy.
                                            int additionalSpan = 1;
                                            FieldPosition tmpFieldPosition = layoutInfo.GetFieldPosition(tmpField, true).Value;
                                            if (tmpFieldPosition.RowSpan < labelPosition.RowSpan)
                                                additionalSpan = tmpField.RowSpan;

                                            int numSummariesForField;
                                            if (processedLabels.TryGetValue(tmpField, out numSummariesForField))
                                                rowSpan = Math.Max(rowSpan, additionalSpan + numSummariesForField);
                                        }
                                    }

                                    // The labels should be picking up their defaults from the non-summary labels
									FormatSettings headerLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutLabelFormatSettings(layout);
									FormatSettings headerFieldFormatSettings = exportHelper.ExportCache.GetResolvedFieldLabelFormatSettings(summaryField, headerLayoutFormatSettings);
                                    FormatSettings resolvedHeaderSettings = DataPresenterExportCache.MergeFormatSettings(headerFieldFormatSettings, resolvedLabelSettings);

                                    // MBS 9/18/09
                                    // Need to take into account field-specific settings
									resolvedHeaderSettings = exportHelper.ExportCache.GetResolvedFieldSummaryLabelFormatSettings(summaryField, resolvedHeaderSettings);                                

                                    bool setValue = true;
                                    if (summaryField.VisibilityResolved == Visibility.Hidden)
                                        setValue = false;

                                    
                                    // Always position the label at a relative row of 0, since we're controlling the current position
                                    // of the exporter ourselves.
                                    Rectangle relativeRect = new Rectangle(labelPosition.Column, 0, labelPosition.ColumnSpan, rowSpan);
                                    this.WriteHeaderCell(exportHelper, associatedRecord, summaryField, relativeRect, resolvedHeaderSettings, setValue);
                                }
                                else
                                    Debug.Fail("Unexpected type in the summary list");
                            }
                        }

                        this.IncreaseCurrentRow(exportHelper,
                            exportHelper.extentOfCurrentGridObject.Height);

                        exportHelper.currentPos.X = startXPos;

                        SummaryRowExportedEventArgs summaryAreaExportedEA = new SummaryRowExportedEventArgs(exportHelper, summaries, currentSummaryLevel);
                        this.OnSummaryRowExported(summaryAreaExportedEA);
                    }

                    // Increase the current summary level, since we're now moving to the next Excel row
                    currentSummaryLevel++;

                    // If we have any summaries that should be put on the next logical row, such as in the case of
                    // a field that has a RowSpan > 1, we should copy those values to the next row here
                    if (currentRow < summaryRowList.Count - 1 && currentLogicalRow == summariesInCurrentRow - 1 && summariesInCurrentRow > 0)
                    {
                        List<List<object>> nextRowSummaries = summaryRowList[currentRow + 1];
                        for (int y = 0; y < summaryColumnList.Count; y++)
                        {
                            List<object> columnSummaries = summaryColumnList[y];
                            if (columnSummaries == null || columnSummaries.Count <= summariesInCurrentRow)
                                continue;

                            List<object> nextRowColumnSummaries = nextRowSummaries[y];
                            if (nextRowSummaries[y] == null)
                            {
                                nextRowColumnSummaries = new List<object>();
                                nextRowSummaries[y] = nextRowColumnSummaries;
                            }

                            for (int x = numLogicalRows - 1; x >= summariesInCurrentRow; x--)
                            {
                                object summaryInfoToCopy = columnSummaries[x];
                                nextRowColumnSummaries.Insert(0, summaryInfoToCopy);
                            }
                        }

                        break;
                    }
                }
            }

            this.SetCollapsedRows(
                exportHelper.CurrentWorksheet,
                // Since we increase the row during the exporting process, the collapsed rows should start at the beginning of the summaries
                startYPos,
                exportHelper.extentOfCurrentGridObject.Bottom,
                exportHelper.HierarchyLevel,
                associatedRecord.ParentRecord != null && !associatedRecord.ParentRecord.IsExpanded,
				exportHelper );

            exportHelper.currentPos = exportHelper.extentOfCurrentGridObject.Location;
            this.IncreaseCurrentRow(exportHelper,
                exportHelper.extentOfCurrentGridObject.Height);

            exportHelper.currentPos.X = startXPos;
        }

        private void ProcessSummariesHelper(SummaryResult summary, bool isLabel, FieldPosition? fieldPositionNullable, ref List<List<List<object>>> summaryRowList)
        {
            if (fieldPositionNullable.HasValue)
            {
                FieldPosition fieldPosition = fieldPositionNullable.Value;

                // Create an array containing a list of SummaryResult objects, one list per column.  We walk through
                // the rows list to ensure that we position the column list at the correct DataPresenter row position,
                // such as if the first summary we encounter has a FieldPosition.Row of 1, but we will later get one with 0.
                List<List<object>> summaryColumnList = null;
                while (summaryRowList.Count <= fieldPosition.Row)
                {
                    summaryRowList.Add(new List<List<object>>());
                }
                summaryColumnList = summaryRowList[fieldPosition.Row];

                // Create the list of summaries for the column in this particular row.
                // We need to ensure that we have a list for each column as well, similar to what
                // we do for rows above, since fields could span across multiple columns
                List<object> summariesInColumn = null;
                while (summaryColumnList.Count <= fieldPosition.Column)
                {
                    summaryColumnList.Add(new List<object>());
                }
                summariesInColumn = summaryColumnList[fieldPosition.Column];

                // If we're adding the label so that we can render it later, store the actual field
                // so we can get the label's information later
                if (isLabel)
                    summariesInColumn.Add(summary.PositionFieldResolved);
                else
                    summariesInColumn.Add(summary);
            }
            else
                Debug.Fail("Encountered a summary without a calculated FieldPosition");
        }
        #endregion //ProcessSummaries

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region RemoveExporter
		internal void RemoveExporter(DataPresenterExcelExporterHelper exporter)
		{
			_exporters.Remove(exporter);
			_exporters.Compact();
			this.IsExporting = _exporters.Count > 0;
		}
		#endregion //RemoveExporter

		#region SetCollapsedRow

		private void SetCollapsedRow(Worksheet worksheet, int row, int hierarchyLevel, Record associatedRecord, DataPresenterExcelExporterHelper exportHelper )
        {
			// MD 3/15/11 - TFS21053
			// This parameter determines if the row is hidden, but it isn't enough to just check IsExpanded,
			// because the record's parents could be expanded but under a collapsed ancestor, in which case
			// this record should be hidden. so used HasCollapsedAncestor instead.
			//this.SetCollapsedRow(worksheet, row, hierarchyLevel, associatedRecord.ParentRecord != null && !associatedRecord.ParentRecord.IsExpanded, exportHelper /* AS 3/3/11 NA 2011.1 - Async Exporting */);
			this.SetCollapsedRow(worksheet, row, hierarchyLevel, associatedRecord.ParentRecord != null && associatedRecord.ParentRecord.HasCollapsedAncestor, exportHelper );
        }

		private void SetCollapsedRow(Worksheet worksheet, int row, int hierarchyLevel, bool collapsed, DataPresenterExcelExporterHelper exportHelper )
        {
            if (row >= Workbook.GetMaxRowCount(worksheet.Workbook.CurrentFormat))
            {
                if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                    return;

                throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelRowCountLimit"));
            }

            WorksheetRow tmRow = worksheet.Rows[row];
            tmRow.OutlineLevel = hierarchyLevel;
            tmRow.Hidden = collapsed;
        }
        #endregion //SetCollapsedRow

        #region SetCollapsedRows

		private void SetCollapsedRows(Worksheet worksheet, int firstRow, int lastRow, int hierarchyLevel, bool collapsed, DataPresenterExcelExporterHelper exportHelper )
        {
            for (int i = firstRow; i <= lastRow; i++)
                this.SetCollapsedRow(worksheet, i, hierarchyLevel, collapsed, exportHelper );
        }
        #endregion //SetCollapsedRows

        #region SetColumnWidthsAndRowHeights

        private void SetColumnWidthsAndRowHeights(DataPresenterExcelExporterHelper exportHelper, double[] columnWidths, double[] rowHeights)
        {
            int i, size;

            if (columnWidths != null)
            {
                for (i = 0; i < columnWidths.Length; i++)
                {
                    if (exportHelper.currentPos.X + i >= Workbook.GetMaxColumnCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
                    {
						if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                            break;

						throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelColumnCountLimit"));
                    }

                    WorksheetColumn wsCol = exportHelper.CurrentWorksheet.Columns[exportHelper.currentPos.X + i];

                    int columnWidthInPixels = Utilities.ConvertFromLogicalPixels(columnWidths[i]);
                    size = (int)(columnWidthInPixels * this._columnWidthUnitsInPixelX);

                    if (wsCol.Width < size)
                        wsCol.Width = size;
                }
            }

            if (rowHeights != null)
            {
                for (i = 0; i < rowHeights.Length; i++)
                {
                    if (exportHelper.currentPos.Y + i >= Workbook.GetMaxRowCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
                    {
						if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                            break;

						throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelRowCountLimit"));
                    }

                    WorksheetRow wsRow = exportHelper.CurrentWorksheet.Rows[exportHelper.currentPos.Y + i];

                    int rowHeightInPixels = Utilities.ConvertFromLogicalPixels(rowHeights[i]);
                    size = (int)(rowHeightInPixels * this._twipsInPixelY);

                    if (wsRow.Height < size)
                        wsRow.Height = size;
                }
            }
        }
        #endregion //SetColumnWidthsAndRowHeights

        #region SetCurrentGridObjectSpan

        private void SetCurrentGridObjectSpan(DataPresenterExcelExporterHelper exportHelper,
            FieldLayout layout, DataPresenterExportCache.PlaceholderType placeholderType)
        {
            bool includeLables = false;
            ExportLayoutInformation layoutInfo = null;
            switch (placeholderType)
            {
                case DataPresenterExportCache.PlaceholderType.ColumnHeaders:
					layoutInfo = exportHelper.ExportCache.GetDocumentLabelLayoutInfo(layout);
                    break;

                case DataPresenterExportCache.PlaceholderType.Values:
					layoutInfo = exportHelper.ExportCache.GetDocumentFieldLayoutInfo(layout);
                    if (layout.LabelLocationResolved == LabelLocation.InCells)
                        includeLables = true;

                    break;

                default:
                    Debug.Fail("Unknown placeholder type");
                    return;
            }

            if (layoutInfo == null)
            {
                Debug.Fail("Could not acquire a valid ExportLayoutInformation object");
                return;
            }

            int spanX = 0;
            int spanY = 0;

            this.SetCurrentGridObjectSpanHelper(layout, layoutInfo, placeholderType == DataPresenterExportCache.PlaceholderType.ColumnHeaders, ref spanX, ref spanY);

            // If we're positioning the labels within the rows with the cells, we need to take these into account
            // when determining the total height of the current object, since the labels could cause the height to be
            // twice what it would be normally.
            if (includeLables)
                this.SetCurrentGridObjectSpanHelper(layout, layoutInfo, true, ref spanX, ref spanY);

            exportHelper.extentOfCurrentGridObject.Width = spanX;
            exportHelper.extentOfCurrentGridObject.Height = spanY;
        }

        private void SetCurrentGridObjectSpanHelper(FieldLayout fieldLayout, ExportLayoutInformation exportLayoutInfo, bool labels, ref int spanX, ref int spanY)
        {
            foreach (Field field in fieldLayout.Fields)
            {
                FieldPosition? fieldPos = exportLayoutInfo.GetFieldPosition(field, labels);
                if (fieldPos.HasValue)
                {
                    spanX = Math.Max(spanX, fieldPos.Value.Column + fieldPos.Value.ColumnSpan);
                    spanY = Math.Max(spanY, fieldPos.Value.Row + fieldPos.Value.RowSpan);
                }
            }
        }
        #endregion //SetCurrentGridObjectSpan

        #region SetRegionRelativeToOrigin

        private WorksheetCell SetRegionRelativeToOrigin(DataPresenterExcelExporterHelper exportHelper,
            Rectangle rect, object val, IWorksheetCellFormat cellFormat)
        {
            if (rect.Width < 1 || rect.Height < 1)
                return null;

            exportHelper.currentPos.X = exportHelper.extentOfCurrentGridObject.X + rect.X;
            exportHelper.currentPos.Y = exportHelper.extentOfCurrentGridObject.Y + rect.Y;

            bool cancel = false;

            if (exportHelper.currentPos.Y + rect.Height - 1 >= Workbook.GetMaxRowCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
            {
				if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                    cancel = true;
                else
					throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelRowCountLimit"));
            }

            if (exportHelper.currentPos.X + rect.Width - 1 >= Workbook.GetMaxColumnCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
            {
                if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                    cancel = true;
                else
					throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelColumnCountLimit"));
            }

            if (!cancel)
            {
                if (exportHelper.CurrentWorksheet.MergedCellsRegions.IsOverlappingWithMergedRegion(
                    exportHelper.currentPos.Y,
                    exportHelper.currentPos.X,
                    exportHelper.currentPos.Y + rect.Height - 1,
                    exportHelper.currentPos.X + rect.Width - 1))
                {
                    return null;
                }

                if (rect.Width > 1 || rect.Height > 1)
                {
                    exportHelper.CurrentWorksheet.MergedCellsRegions.Add(
                        exportHelper.currentPos.Y,
                        exportHelper.currentPos.X,
                        exportHelper.currentPos.Y + rect.Height - 1,
                        exportHelper.currentPos.X + rect.Width - 1);
                }

                WorksheetCell excelCell = exportHelper.CurrentWorksheet.Rows[
                    exportHelper.currentPos.Y].Cells[exportHelper.currentPos.X];

				if (cellFormat != null)
				{
					excelCell.CellFormat.SetFormatting(cellFormat);
				}
				// MD 5/31/11 - TFS75574
				// The cell format will now be null when there are all default values. However, if it is null and the cell already has a cell format,
				// we need to clear the cell format values if any are present.
				else
				{
					if (excelCell.HasCellFormat)
						excelCell.CellFormat.SetFormatting(exportHelper.Workbook.CreateNewWorksheetCellFormat());
				}

                if (val is Image)
                {
                    
                    //Image image = (Image)val;

                    //// Create a WorksheetImage
                    //WorksheetImage worksheetImage = new WorksheetImage(image);

                    //// Position the image. This position will be wrong, since the height of 
                    //// the row has not yet been set. But we have to set the positions to 
                    //// something in order to add the image. 
                    ////
                    //UltraGridExcelExporter.PositionWorksheetImage(excelCell.Worksheet, worksheetImage, rect);

                    //// Add the image into the workbook
                    //excelCell.Worksheet.Shapes.Add(worksheetImage);
                    //Rectangle imageRect = new Rectangle(
                    //    exportHelper.currentPos.X,
                    //    exportHelper.currentPos.Y,
                    //    rect.Width - 1,
                    //    rect.Height - 1
                    //    );

                    //// Store the worksheetImage and it's rect so that we can set it's bounds
                    //// after the Row is exported. We have to do this because the row height's
                    //// have not been established, yet, and they won't be until after the
                    //// row is exported. 
                    ////
                    //this.UnpositionedImages[worksheetImage] = imageRect;
                }
                else
                {
                    excelCell.Value = val;
                }

                return excelCell;
            }

            return null;
        }
        #endregion //SetRegionRelativeToOrigin

        #region ShouldDisplayHeadersAboveRecord

        private bool ShouldDisplayHeadersAboveRecord(DataPresenterExcelExporterHelper exportHelper, Record record)
        {
            if (record == null)
                return false;

            // Check to see if we need to display the headers above the row.  Note that we don't check the HeaderPlacement property
            // because OnRecordBreak doesn't really make any sense for Excel, due to the fact that we can't automatically hide the
            // repeated headers if a record is collapsed.
            FieldLayout layout = record.FieldLayout;

            // We will take into account the other cases of the label location when we export each record.
            if (layout.LabelLocationResolved != LabelLocation.SeparateHeader)
                return false;

            GroupByRecord groupByRecord = record as GroupByRecord;
            if (groupByRecord != null)
            {
                return groupByRecord.VisibleIndex == 0 &&
                        // Don't show headers for records grouped by their FieldLayout, such as in cases
                        // with heterogeneous data when we group by a field that doesn't exist in all
                        // FieldLayouts
                       groupByRecord.RecordType == RecordType.GroupByField &&
                       layout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly;
            }

            if (record.RecordType == RecordType.SummaryRecord)
            {
                // MBS 9/17/09                
                if (record.ParentRecord != null)
                {
                    HeaderPlacementInGroupBy headerPlacement = layout.HeaderPlacementInGroupByResolved;
                    RecordType parentType = record.ParentRecord.RecordType;
                    return parentType == RecordType.ExpandableFieldRecord ||
                           (record.ParentRecord.RecordType == RecordType.GroupByField && headerPlacement == HeaderPlacementInGroupBy.WithDataRecords) ||
                           (headerPlacement == HeaderPlacementInGroupBy.OnTopOnly && record.ParentRecord.RecordType == RecordType.GroupByFieldLayout);                             
                }

                // When we're grouped we don't display any headers above the summaries except for the
                // grand summaries at the top with a HeaderPlacement of OnTopOnly
                //
                GroupByRecordCollection groupByRecordCollection = record.ParentCollection as GroupByRecordCollection;
                if (groupByRecordCollection != null)
                    return record.VisibleIndex == 0 && layout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly &&
                        // Ensure that this isn't just a record collection for GroupByFieldLayout types
                        groupByRecordCollection[0].GroupByField != null;

                return record.VisibleIndex == 0;
            }

            // We need to display the header if the record is the first record in the collection, or if
            // the record is part of a different FieldLayout (i.e. heterogenous data).  Note that we don't
            // want to do this if we're already displaying the header via OnTopOnly for GroupByRecords.
            GroupByRecord parentGroupByRecord = record.ParentRecord as GroupByRecord;            
            if (parentGroupByRecord == null || parentGroupByRecord.GroupByField == null ||
                layout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.WithDataRecords)
            {                
                // MBS 9/16/09 - TFS22270
                // Reworked some of this logic, since we need to keep track of more information on the
                // previous headers that were exported in order to make a better decision about whether
                // they should be shown for the current record
                //
                // if (layout != exportHelper.LastFieldLayout && 
                //    (exportHelper.LastExportedRecord == null || exportHelper.LastExportedRecord.ParentCollection != record.ParentCollection))
                //     return true;
                //
                //// If the previous record, within the same FieldLayout, displayed headers then we shouldn't
                //// repeat them here.  This can occur with, for example, summaries.  
                //return record.VisibleIndex == 0 && 
                //    (record == exportHelper.LastExportedRecord || !this.ShouldDisplayHeadersAboveRecord(exportHelper, exportHelper.LastExportedRecord));
                Record lastExportedRecord = exportHelper.GetLastExportedRecord(record.ParentCollection);
				if (exportHelper.ExportCache.HasExportedHeadersForRecordCollection(record.ParentCollection) == false ||
                    lastExportedRecord == null || lastExportedRecord.FieldLayout != layout)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion //ShouldDisplayHeadersAboveRecord

        #region WriteCellValue

        private object WriteCellValue(DataPresenterExcelExporterHelper exportHelper, DataRecord record, Field field, Rectangle cellRect, 
            FormatSettings cellFormatSettings, bool setValue)
        {
            object val = null;

            if (setValue)
            {
				// SSP 10/22/09 TFS24061
				// When cell values are mapped to display texts using XamComboEditor, we should export
				// the mapped displaye text. For that use the new GetExportValue method which will return 
				// the mapped display text if any otherwise the value converted to EditAsType.
				// 
                //val = record.GetCellValue(field, CellValueType.Converted);
				val = record.GetExportValue( field );

                // If the Type of value is not supported in an Excel cell (i.e. a GUID), then we should simply
                // export the string representation of that value
                if (val != null && !WorksheetCell.IsCellTypeSupported(val.GetType()))
                    val = record.GetCellText(field);
            }

            IWorksheetCellFormat cellFormat = this.GetCellFormatFromFormatSettings(exportHelper, cellFormatSettings, field, record.FieldLayout, false);
            WorksheetCell worksheetCell = SetRegionRelativeToOrigin(exportHelper, cellRect, val, cellFormat);

            return val;
        }
        #endregion //WriteCellValue

        #region WriteGroupByRow

        private void WriteGroupByRecord(DataPresenterExcelExporterHelper exportHelper, GroupByRecord record, FormatSettings formatSettings, int nestingOffset)
        {
            if (exportHelper.currentPos.Y >= Workbook.GetMaxRowCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
            {
				if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                    return;

				throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelRowCountLimit"));
            }

            if (exportHelper.currentPos.X >= Workbook.GetMaxColumnCount(exportHelper.CurrentWorksheet.Workbook.CurrentFormat))
            {
				if (exportHelper.ExportCache.ExportOptions.FileLimitBehaviour == FileLimitBehaviour.TruncateData)
                    return;
                else
					throw new Exception(DataPresenterExcelExporter.GetString("LER_MaxExcelColumnCountLimit"));
            }

            WorksheetCell excelCell = exportHelper.CurrentWorksheet.Rows[exportHelper.currentPos.Y].Cells[exportHelper.currentPos.X];
            IWorksheetCellFormat cellFormat = this.GetCellFormatFromFormatSettings(exportHelper, formatSettings, null, record.FieldLayout, false);
            Rectangle groupByRect = new Rectangle(System.Drawing.Point.Empty, exportHelper.extentOfCurrentGridObject.Size);
            this.SetRegionRelativeToOrigin(exportHelper, groupByRect, record.DescriptionWithSummaries, cellFormat);

            if (record.FieldLayout.GroupBySummaryDisplayModeResolved == GroupBySummaryDisplayMode.SummaryCellsAlwaysBelowDescription)
            {
                // Increase the current position here so that all of the summaries are correctly offset to the right of the
                // GroupByRecord, otherwise they will be off-by-one to the left.
                //exportHelper.currentPos.X += exportHelper.extentOfCurrentGridObject.Width;
                exportHelper.currentPos.X += nestingOffset;

                // Put GroupBy summaries a row below the actual row, for readability.  We do the same thing with 
                // the WinGridExcelExporter
                exportHelper.currentPos.Y += exportHelper.extentOfCurrentGridObject.Height;

				FormatSettings summaryCellFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutSummaryCellFormatSettings(record.FieldLayout);
				FormatSettings summaryLabelFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutSummaryLabelFormatSettings(record.FieldLayout);

                // The GroupByRecord should be providing the defaults for the cells and labels since the summaries are contained within them
                summaryCellFormatSettings = DataPresenterExportCache.MergeFormatSettings(formatSettings, summaryCellFormatSettings);
                summaryLabelFormatSettings = DataPresenterExportCache.MergeFormatSettings(formatSettings, summaryLabelFormatSettings);

                SummaryResultCollection summaryResults = record.ChildRecords.SummaryResults;
                if (summaryResults.Count > 0)
                    this.ProcessSummaries(exportHelper, record, record.ChildRecords.SummaryResults, summaryCellFormatSettings, summaryLabelFormatSettings);
            }
        }
        #endregion //WriteGroupByRow

        #region WriteHeaderCell

        private void WriteHeaderCell(DataPresenterExcelExporterHelper exportHelper, Record record, Field field, Rectangle relativeRect, 
            FormatSettings formatSettings, bool setValue)
        {
            exportHelper.currentPos.X = exportHelper.extentOfCurrentGridObject.X + relativeRect.X;
            exportHelper.currentPos.Y = exportHelper.extentOfCurrentGridObject.Y + relativeRect.Y;

            // Merge the format settings that are passed in with the settings that are specified to the field
			FormatSettings fieldLabelFormatSettings = exportHelper.ExportCache.GetResolvedFieldLabelFormatSettings(field, formatSettings);

            HeaderLabelExportingEventArgs labelExportingArgs = new HeaderLabelExportingEventArgs(exportHelper, field, record, fieldLabelFormatSettings);
            this.OnHeaderLabelExporting(labelExportingArgs);
            if (!labelExportingArgs.Cancel)
            {
                object value = setValue ? field.Label : null;                
                IWorksheetCellFormat labelFormat = this.GetCellFormatFromFormatSettings(exportHelper, labelExportingArgs.FormatSettingsInternal, field, record.FieldLayout, true);
                this.SetRegionRelativeToOrigin(exportHelper, relativeRect, value, labelFormat);

                HeaderLabelExportedEventArgs cellExportedArgs = new HeaderLabelExportedEventArgs(exportHelper, field, record);
                this.OnHeaderLabelExported(cellExportedArgs);
            }
        }
        #endregion //WriteHeaderCell

        #region WriteSingleRecord

        private void WriteSingleRecord(DataPresenterExcelExporterHelper exportHelper, Record record, FormatSettings cellFormatSettings)
        {
            DataRecord dataRecord = record as DataRecord;
            if (dataRecord == null)
            {
                Debug.Fail("Trying to write a non-DataRecord");
                return;
            }

			ExportLayoutInformation layoutInfo = exportHelper.ExportCache.GetDocumentFieldLayoutInfo(dataRecord.FieldLayout);
            this.SetColumnWidthsAndRowHeights(exportHelper, layoutInfo.GetColumnWidths(), layoutInfo.GetRowHeights());

            ExportLayoutInformation headerLayoutInfo = null;
            if (dataRecord.FieldLayout.LabelLocationResolved == LabelLocation.InCells)
            {
				headerLayoutInfo = exportHelper.ExportCache.GetDocumentLabelLayoutInfo(dataRecord.FieldLayout);
                this.SetColumnWidthsAndRowHeights(exportHelper, headerLayoutInfo.GetColumnWidths(), headerLayoutInfo.GetRowHeights());
            }

            foreach (Field field in dataRecord.FieldLayout.Fields)
            {
                Visibility fieldVisibility = field.VisibilityResolved;
                if (fieldVisibility == Visibility.Collapsed)
                    continue;

                bool setValue = true;
                if (fieldVisibility == Visibility.Hidden)
                    setValue = false;

                // Position the field's label first if we have the header info since we know that we need to position the labels 
                // with the individual cells.
                if (headerLayoutInfo != null)
                {
                    FieldPosition? headerFieldPosNullable = layoutInfo.GetFieldPosition(field, true);
                    if (headerFieldPosNullable.HasValue)
                    {
                        FieldPosition headerFieldPosition = headerFieldPosNullable.Value;
                        exportHelper.currentPos.X = exportHelper.extentOfCurrentGridObject.X + headerFieldPosition.Column;
                        exportHelper.currentPos.Y = exportHelper.extentOfCurrentGridObject.Y + headerFieldPosition.Row;

						FormatSettings headerLayoutFormatSettings = exportHelper.ExportCache.GetResolvedFieldLayoutLabelFormatSettings(record.FieldLayout);
                        Rectangle relativeRect = new Rectangle(headerFieldPosition.Column, headerFieldPosition.Row, headerFieldPosition.ColumnSpan, headerFieldPosition.RowSpan);
                        this.WriteHeaderCell(exportHelper, record, field, relativeRect, headerLayoutFormatSettings, setValue);
                    }
                }

                FieldPosition? fieldPosNullable = layoutInfo.GetFieldPosition(field, false);
                if (fieldPosNullable.HasValue)
                {
                    FieldPosition fieldPosition = fieldPosNullable.Value;
                    exportHelper.currentPos.X = exportHelper.extentOfCurrentGridObject.X + fieldPosition.Column;
                    exportHelper.currentPos.Y = exportHelper.extentOfCurrentGridObject.Y + fieldPosition.Row;

					FormatSettings resolvedFieldFormatSettings = exportHelper.ExportCache.GetResolvedFieldCellFormatSettings(field, cellFormatSettings);
                    CellExportingEventArgs cellExportingArgs = new CellExportingEventArgs(exportHelper, dataRecord, field, resolvedFieldFormatSettings);
                    this.OnCellExporting(cellExportingArgs);
                    if (!cellExportingArgs.Cancel)
                    {
                        Rectangle relativeRect = new Rectangle(fieldPosition.Column, fieldPosition.Row, fieldPosition.ColumnSpan, fieldPosition.RowSpan);
                        object resolvedVal = this.WriteCellValue(exportHelper, dataRecord, field, relativeRect, cellExportingArgs.FormatSettingsInternal, setValue);

                        CellExportedEventArgs cellExportedArgs = new CellExportedEventArgs(exportHelper, dataRecord, field, resolvedVal);
                        this.OnCellExported(cellExportedArgs);
                    }
                }
            }
        }
        #endregion //WriteSingleRecord

        #endregion //Internal Methods

        #region Protected Methods

        #region OnBeginExport

        /// <summary>
        /// Called before grid export starts.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
		[Obsolete("The 'BeginExport' method is obsolete. The 'ExportStarted' event should be used instead.", false)] // AS 2/11/11 NA 2011.1 Word Writer
		virtual protected void OnBeginExport(BeginExportEventArgs e)
        {
            if (this.BeginExport != null)
                this.BeginExport(this, e);
        }
        #endregion //OnBeginExport

        #region OnCellExported

        /// <summary>
        /// Called after grid cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the cell that was exported.</param>
        virtual protected void OnCellExported(CellExportedEventArgs e)
        {
            if (this.CellExported != null)
                this.CellExported(this, e);
        }
        #endregion //OnCellExported

        #region OnCellExporting

        /// <summary>
        /// Called before grid cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the cell being exported.</param>
        virtual protected void OnCellExporting(CellExportingEventArgs e)
        {
            if (this.CellExporting != null)
                this.CellExporting(this, e);
        }
        #endregion //OnCellExporting

        #region OnEndExport

        /// <summary>
        /// Called after grid export is finished.
        /// </summary>
        /// <param name="e">An object containing information about the final status of the export process.</param>
		[Obsolete("The 'EndExport' method is obsolete. The 'ExportEnding' or 'ExportEnded' event should be used instead. See the comments for those events for more information on which to use.", false)] // AS 2/11/11 NA 2011.1 Word Writer
		virtual protected void OnEndExport(EndExportEventArgs e)
        {
            if (this.EndExport != null)
                this.EndExport(this, e);
        }
        #endregion //OnEndExport

		// AS 2/11/11 NA 2011.1 Word Writer
		#region OnExportEnded

		/// <summary>
		/// Used to invoke the <see cref="ExportEnded"/> event.
		/// </summary>
		/// <param name="e">An object contained information about the final status of the export process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		protected virtual void OnExportEnded(ExportEndedEventArgs e)
		{
			EventHandler<ExportEndedEventArgs> handler = this.ExportEnded;

			if (handler != null)
				handler(this, e);
		}
		#endregion //OnExportEnded

		// AS 2/11/11 NA 2011.1 Word Writer
		#region OnExportEnding

		/// <summary>
		/// Used to invoke the <see cref="ExportEnding"/> event.
		/// </summary>
		/// <param name="e">An object containing information about the final status of the export process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		protected virtual void OnExportEnding(ExportEndingEventArgs e)
		{
			EventHandler<ExportEndingEventArgs> handler = this.ExportEnding;

			if (handler != null)
				handler(this, e);
		}
		#endregion //OnExportEnding

		// AS 2/11/11 NA 2011.1 Word Writer
		#region OnExportStarted

        /// <summary>
        /// Used to invoke the <see cref="ExportStarted"/> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		protected virtual void OnExportStarted(ExportStartedEventArgs e)
        {
			EventHandler<ExportStartedEventArgs> handler = this.ExportStarted;

			if (handler != null)
				handler(this, e);
		}
        #endregion //OnExportStarted

        #region OnHeaderAreaExported

        /// <summary>
        /// Called after header row is exported to excel.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
        virtual protected void OnHeaderAreaExported(HeaderAreaExportedEventArgs e)
        {
            if (this.HeaderAreaExported != null)
                this.HeaderAreaExported(this, e);
        }
        #endregion //OnHeaderAreaExported

        #region OnHeaderAreaExporting

        /// <summary>
        /// Called before header row is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the header row.</param>
        virtual protected void OnHeaderAreaExporting(HeaderAreaExportingEventArgs e)
        {
            if (this.HeaderAreaExporting != null)
                this.HeaderAreaExporting(this, e);
        }
        #endregion //OnHeaderAreaExporting

        #region OnHeaderLabelExported

        /// <summary>
        /// Called after header cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the header cell this was exported.</param>
        virtual protected void OnHeaderLabelExported(HeaderLabelExportedEventArgs e)
        {
            if (this.HeaderLabelExported != null)
                this.HeaderLabelExported(this, e);
        }
        #endregion //OnHeaderLabelExported

        #region OnHeaderLabelExporting

        /// <summary>
        /// Called before header cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the header cell being exported.</param>
        virtual protected void OnHeaderLabelExporting(HeaderLabelExportingEventArgs e)
        {
            if (this.HeaderLabelExporting != null)
                this.HeaderLabelExporting(this, e);
        }
        #endregion //OnHeaderLabelExporting

        #region OnInitializeRecord

        /// <summary>
        /// Called when a record is initialized.
        /// </summary>
        /// <param name="e">An object containing various options for controlling, or excluding, a record.</param>
        virtual protected void OnInitializeRecord(InitializeRecordEventArgs e)
        {
            if (this.InitializeRecord != null)
                this.InitializeRecord(this, e);
        }
        #endregion //OnInitializeRecord

        #region OnRecordExported

        /// <summary>
        /// Called after a record is exported to Excel.
        /// </summary>
        /// <param name="e">An object providing information about the record.</param>
        virtual protected void OnRecordExported(RecordExportedEventArgs e)
        {
            if (this.RecordExported != null)
                this.RecordExported(this, e);
        }
        #endregion //OnRecordExported

        #region OnRecordExporting

        /// <summary>
        /// Called before a record is exported to Excel.
        /// </summary>
        /// <param name="e">An object providing information about the record, or allowing the export to be cancelled.</param>
        virtual protected void OnRecordExporting(RecordExportingEventArgs e)
        {
            if (this.RecordExporting != null)
                this.RecordExporting(this, e);
        }
        #endregion //OnRecordExporting

        #region OnSummaryAreaExported

        /// <summary>
        /// Called after summary row is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the summary record that was exported.</param>
        virtual protected void OnSummaryRowExported(SummaryRowExportedEventArgs e)
        {
            if (this.SummaryRowExported != null)
                this.SummaryRowExported(this, e);
        }
        #endregion //OnSummaryAreaExported

        #region OnSummaryAreaExporting

        /// <summary>
        /// Called before summary record is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the summary record being exported.</param>
        virtual protected void OnSummaryRowExporting(SummaryRowExportingEventArgs e)
        {
            if (this.SummaryRowExporting != null)
                this.SummaryRowExporting(this, e);
        }
        #endregion //OnSummaryAreaExporting

        #region OnSummaryCellExported

        /// <summary>
        /// Called after summary cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the summary cell that was exported.</param>
        virtual protected void OnSummaryCellExported(SummaryCellExportedEventArgs e)
        {
            if (this.SummaryCellExported != null)
                this.SummaryCellExported(this, e);
        }
        #endregion //OnSummaryCellExported

        #region OnSummaryCellExporting

        /// <summary>
        /// Called before summary cell is exported to excel.
        /// </summary>
        /// <param name="e">An object containing information about the summary cell being exported.</param>
        virtual protected void OnSummaryCellExporting(SummaryCellExportingEventArgs e)
        {
            if (this.SummaryCellExporting != null)
                this.SummaryCellExporting(this, e);
        }
        #endregion //OnSummaryCellExporting

        #endregion //Protected Methods

        #region Public Methods

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region CancelExport
		/// <summary>
		/// Cancels any asynchronous export operations for the specified <see cref="DataPresenterBase"/> that were initiated by this instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">An export operation may be cancelled by handling the <see cref="InitializeRecord"/> event and setting the 
		/// <see cref="InitializeRecordEventArgs.TerminateExport"/> to true. However, for an asynchronous export the operation may be 
		/// pending while another asynchronous operation is pending or the export may still be preparing the source control. For those 
		/// situations this method may be used to cancel the operation.</p>
		/// </remarks>
		/// <param name="dataPresenter">The datapresenter whose asynchronous export is to be cancelled</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void CancelExport(DataPresenterBase dataPresenter)
		{
			for (int i = _exporters.Count - 1; i >= 0; i--)
			{
				DataPresenterExcelExporterHelper exporter = _exporters[i];

				if (null != exporter && exporter.Source == dataPresenter)
				{
					dataPresenter.CancelExport(exporter);
				}
			}
		}
		#endregion //CancelExport

        #region Export

        /// <summary>
        /// Exports the passed in data presenter to specified file. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="fileName">Name and path of resulting Excel file.</param>
        /// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
        /// <returns>Created <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, string fileName, WorkbookFormat workbookFormat)
        {
            return this.Export(dataPresenter, fileName, workbookFormat, null);
        }

        /// <summary>
        /// Exports the passed in data presenter to specified file. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="fileName">Name and path of resulting Excel file.</param>
        /// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Created <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, string fileName, WorkbookFormat workbookFormat, ExportOptions options)
        {
			// AS 2/11/11 NA 2011.1 Word Writer
			// Pass along the filename so we can save it before raising the ExportEnded.
			//
			//Workbook wb = this.Export(dataPresenter, workbookFormat, options);
            //wb.Save(fileName);
			Workbook wb = this.Export(dataPresenter, workbookFormat, options, fileName);

            return wb;
        }

        /// <summary>
        /// Creates a new workbook with empty <see cref="Infragistics.Documents.Excel.Worksheet"/> 
        /// and exports the passed in data presenter to it. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
        /// <returns>Created <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, WorkbookFormat workbookFormat)
        {
            return this.Export(dataPresenter, workbookFormat, null);
        }

        /// <summary>
        /// Creates a new workbook with empty <see cref="Infragistics.Documents.Excel.Worksheet"/> 
        /// and exports the passed in data presenter to it. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Created <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
		public Workbook Export(DataPresenterBase dataPresenter, WorkbookFormat workbookFormat, ExportOptions options)
		{
			// AS 2/11/11 NA 2011.1 Word Writer
			return this.Export(dataPresenter, workbookFormat, options, null);
		}

		// AS 2/11/11 NA 2011.1 Word Writer
		// Moved the implementation of the overload that just takes a WorkbookFormat so we can pass along the filename.
		//
		private Workbook Export(DataPresenterBase dataPresenter, WorkbookFormat workbookFormat, ExportOptions options, string filename)
        {
            Workbook wb = new Workbook(workbookFormat, WorkbookPaletteMode.StandardPalette);//this.DefaultWorkbookPaletteMode);
            wb.Worksheets.Add("Sheet1");

			return this.Export(dataPresenter, wb.WindowOptions.SelectedWorksheet, 0, 0, options, filename );
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified workbook in the active 
        /// worksheet. If there are no worksheets in workbook, new worksheet is created. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
        /// <returns>Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Workbook workbook)
        {
            return this.Export(dataPresenter, workbook, null);
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified workbook in the active 
        /// worksheet. If there are no worksheets in workbook, new worksheet is created. 
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Workbook workbook, ExportOptions options)
        {
            if (workbook.Worksheets.Count == 0)
                workbook.Worksheets.Add("Sheet1");

            return this.Export(dataPresenter, workbook.WindowOptions.SelectedWorksheet, 0, 0, options);
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified worksheet.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
        /// <returns>Parent workbook of passed in worksheet.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Worksheet worksheet)
        {
            return this.Export(dataPresenter, worksheet, null);
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified worksheet.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Parent workbook of passed in worksheet.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Worksheet worksheet, ExportOptions options)
        {
            return this.Export(dataPresenter, worksheet, 0, 0, options);
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified workbook in the active 
        /// worksheet starting at the specified row and column. If there are no 
        /// worksheets in workbook, new worksheet is created. It returns the passed 
        /// in workbook.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
        /// <param name="startRow">Start row for exported data (zero based).</param>
        /// <param name="startColumn">Start column for exported data (zero based).</param>
        /// <returns>Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Workbook workbook, int startRow, int startColumn)
        {
            return this.Export(dataPresenter, workbook, startRow, startColumn, null);
        }

        /// <summary>
        /// Exports the passed in data presenter to the specified workbook in the active 
        /// worksheet starting at the specified row and column. If there are no 
        /// worksheets in workbook, new worksheet is created. It returns the passed 
        /// in workbook.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
        /// <param name="startRow">Start row for exported data (zero based).</param>
        /// <param name="startColumn">Start column for exported data (zero based).</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Workbook workbook, int startRow, int startColumn, ExportOptions options)
        {
            if (workbook.Worksheets.Count == 0)
                workbook.Worksheets.Add("Sheet1");

            return this.Export(dataPresenter, workbook.WindowOptions.SelectedWorksheet, startRow, startColumn, options);
        }

        /// <summary>
        /// Exports the passed in XamDataGrid to the specified worksheet starting at 
        /// the specified row and column.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
        /// <param name="startRow">Start row for exported data (zero based).</param>
        /// <param name="startColumn">Start column for exported data (zero based).</param>
        /// <returns>Parent workbook of passed in worksheet.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn)
        {
            return this.Export(dataPresenter, worksheet, startRow, startColumn, null);
        }

        /// <summary>
        /// Exports the passed in XamDataGrid to the specified worksheet starting at 
        /// the specified row and column.
        /// </summary>
        /// <param name="dataPresenter">The data presenter to export.</param>
        /// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
        /// <param name="startRow">Start row for exported data (zero based).</param>
        /// <param name="startColumn">Start column for exported data (zero based).</param>
        /// <param name="options">A set of options controlling the output of the exporting process.</param>
        /// <returns>Parent workbook of passed in worksheet.</returns>
        public Workbook Export(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn, ExportOptions options)
        {
			// AS 2/11/11 NA 2011.1 Word Writer
			// Moved the impl into a helper overload that takes a filename.
			//
			return this.Export(dataPresenter, worksheet, startRow, startColumn, options, null);
		}

		// AS 2/11/11 NA 2011.1 Word Writer
		// Moved impl from the main Export method here so we can take a filename.
		//
		private Workbook Export(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn, ExportOptions options, string filename)
		{
			// AS 2/11/11 NA 2011.1 Word Writer
			// Moved the impl into a helper method so we can use it for asynchronous processing.
			//
			//if (dataPresenter == null)
			//    throw new ArgumentNullException("dataPresenter");

			//// MBS 9/16/09
			//// Don't allow the user to try to export the data presenter unless it has been initialized, since 
			//// the record managers and other elements will not have been created.
			//if (!dataPresenter.IsInitialized)
			//    throw new InvalidOperationException(DataPresenterExcelExporter.GetString("LER_DataPresenterNotInitialized"));

			//if (worksheet == null)
			//    throw new ArgumentNullException("worksheet");
			//
			//if (startRow < 0 || startRow >= Workbook.GetMaxRowCount(worksheet.Workbook.CurrentFormat))
			//    throw new ArgumentOutOfRangeException("row", startRow, DataPresenterExcelExporter.GetString("LER_ArgumentOutOfRangeException_1"));
			//
			//if (startColumn < 0 || startColumn >= Workbook.GetMaxColumnCount(worksheet.Workbook.CurrentFormat))
			//    throw new ArgumentOutOfRangeException("column", startColumn, DataPresenterExcelExporter.GetString("LER_ArgumentOutOfRangeException_2"));            
			//
			//DataPresenterExcelExporterHelper exporterHelper = new DataPresenterExcelExporterHelper(
			//    this, worksheet, startRow, startColumn);
			//
			//// If the user didn't specify any options to use, create a default set
			//if (options == null)
			//    options = new ExportOptions();
			//
			//// Attempt to calculate any metrics now based on the original data presenter, since it should actually
			//// have a handle.
			//this.EnsureMetricsCalculated(dataPresenter);
			DataPresenterExcelExporterHelper exporterHelper = this.PrepareForExport(dataPresenter, worksheet, startRow, startColumn, ref options, filename);

            dataPresenter.Export(exporterHelper, options);

            return worksheet.Workbook;
        }
        #endregion //Export

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region ExportAsync

		/// <summary>
		/// Asynchronously exports the passed in data presenter to specified file. 
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="fileName">Name and path of resulting Excel file.</param>
		/// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, string fileName, WorkbookFormat workbookFormat)
		{
			this.ExportAsync(dataPresenter, fileName, workbookFormat, null);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to specified file. 
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="fileName">Name and path of resulting Excel file.</param>
		/// <param name="workbookFormat">Specifies the format for the new workbook (Excel2007 or Excel97To2003) .</param>
		/// <param name="options">A set of options controlling the output of the exporting process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, string fileName, WorkbookFormat workbookFormat, ExportOptions options)
		{
			Workbook wb = new Workbook(workbookFormat, WorkbookPaletteMode.StandardPalette);//this.DefaultWorkbookPaletteMode);
			wb.Worksheets.Add("Sheet1");

			this.ExportAsync(dataPresenter, wb.WindowOptions.SelectedWorksheet, 0, 0, options, fileName);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified workbook in the active 
		/// worksheet. If there are no worksheets in workbook, new worksheet is created. 
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Workbook workbook)
		{
			this.ExportAsync(dataPresenter, workbook, null);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified workbook in the active 
		/// worksheet. If there are no worksheets in workbook, new worksheet is created. 
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
		/// <param name="options">A set of options controlling the output of the exporting process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Workbook workbook, ExportOptions options)
		{
			if (workbook.Worksheets.Count == 0)
				workbook.Worksheets.Add("Sheet1");

			this.ExportAsync(dataPresenter, workbook.WindowOptions.SelectedWorksheet, 0, 0, options);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified worksheet.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Worksheet worksheet)
		{
			this.ExportAsync(dataPresenter, worksheet, null);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified worksheet.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
		/// <param name="options">A set of options controlling the output of the exporting process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Worksheet worksheet, ExportOptions options)
		{
			this.ExportAsync(dataPresenter, worksheet, 0, 0, options);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified workbook in the active 
		/// worksheet starting at the specified row and column. If there are no 
		/// worksheets in workbook, new worksheet is created. It returns the passed 
		/// in workbook.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
		/// <param name="startRow">Start row for exported data (zero based).</param>
		/// <param name="startColumn">Start column for exported data (zero based).</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Workbook workbook, int startRow, int startColumn)
		{
			this.ExportAsync(dataPresenter, workbook, startRow, startColumn, null);
		}

		/// <summary>
		/// Asynchronously exports the passed in data presenter to the specified workbook in the active 
		/// worksheet starting at the specified row and column. If there are no 
		/// worksheets in workbook, new worksheet is created. It returns the passed 
		/// in workbook.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="workbook">Destination <see cref="Infragistics.Documents.Excel.Workbook"/>.</param>
		/// <param name="startRow">Start row for exported data (zero based).</param>
		/// <param name="startColumn">Start column for exported data (zero based).</param>
		/// <param name="options">A set of options controlling the output of the exporting process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Workbook workbook, int startRow, int startColumn, ExportOptions options)
		{
			if (workbook.Worksheets.Count == 0)
				workbook.Worksheets.Add("Sheet1");

			this.ExportAsync(dataPresenter, workbook.WindowOptions.SelectedWorksheet, startRow, startColumn, options);
		}

		/// <summary>
		/// Asynchronously exports the passed in XamDataGrid to the specified worksheet starting at 
		/// the specified row and column.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
		/// <param name="startRow">Start row for exported data (zero based).</param>
		/// <param name="startColumn">Start column for exported data (zero based).</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn)
		{
			this.ExportAsync(dataPresenter, worksheet, startRow, startColumn, null);
		}

		/// <summary>
		/// Asynchronously exports the passed in XamDataGrid to the specified worksheet starting at 
		/// the specified row and column.
		/// </summary>
		/// <param name="dataPresenter">The data presenter to export.</param>
		/// <param name="worksheet">Destination <see cref="Infragistics.Documents.Excel.Worksheet"/>.</param>
		/// <param name="startRow">Start row for exported data (zero based).</param>
		/// <param name="startColumn">Start column for exported data (zero based).</param>
		/// <param name="options">A set of options controlling the output of the exporting process.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public void ExportAsync(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn, ExportOptions options)
		{
			this.ExportAsync(dataPresenter, worksheet, startRow, startColumn, options, null);
		}

		private void ExportAsync(DataPresenterBase dataPresenter, Worksheet worksheet, int startRow, int startColumn, ExportOptions options, string filename)
		{
			DataPresenterExcelExporterHelper exporterHelper = this.PrepareForExport(dataPresenter, worksheet, startRow, startColumn, ref options, filename);
			dataPresenter.ExportAsync(exporterHelper, options, this.ShowAsyncExportStatus, this.AsyncExportDuration, this.AsyncExportInterval);
		}
		#endregion //ExportAsync

        #endregion //Public Methods

        #region Events

        /// <summary>
        /// Occurs at the beginning of the export process.  The data presenter has been initialized but the data source has not yet been set.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>DataPresenter</i> argument returns a reference to the cloned object that is about to be exported.</p>        
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired before all other events. You can use it to do any preprocessing before exporting process starts, such as writing a custom header to the Excel workbook.</p>
		/// <p class="note"><b>Note:</b> The BeginExport event has been deprecated. The <see cref="ExportStarted"/> event should be used instead.</p>
        /// </remarks>        
		[Obsolete("The 'BeginExport' method is obsolete. The 'ExportStarted' event should be used instead.", false)] // AS 2/11/11 NA 2011.1 Word Writer
		public event EventHandler<BeginExportEventArgs> BeginExport;

        /// <summary>
        /// Occurs when a record is initialized.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Record</i> argument returns a reference to the record that is being exported.</p>
        /// <p class="body">The <i>SkipRecord</i> argument specifies whether to skip the current record. This argument doesn't affect exporting of descendant records.</p>
        /// <p class="body">The <i>SkipDescendants</i> argument specifies whether to skip the descendats of the current record.</p>
        /// <p class="body">The <i>SkipSiblings</i> argument specifies whether to skip sibling rows of the current record.</p>
        /// <p class="body">The <i>TerminateExport</i> argument specifies whether to terminate the export process. The current record will not be processed.</p>
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired for every record that is being exported, and before <see cref="RecordExporting"/> and <see cref="RecordExported"/> are fired. Use this event to set record-specific properties and to control the exporting process.</p>
        /// </remarks>        
        public event EventHandler<InitializeRecordEventArgs> InitializeRecord;

        /// <summary>
        /// Occurs before a record is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Record</i> argument returns a reference to record being exported.</p>
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">This event is fired before the Excel row is processed. Use Cancel argument to cancel row exporting. If the records span multiple rows, this event is fired only for the first row.</p>
        /// </remarks>        
        public event EventHandler<RecordExportingEventArgs> RecordExporting;

        /// <summary>
        /// Occurs after a record is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Record</i> argument returns a reference to the record being exported.</p>
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired after the Excel row is processed. Use it to apply any additional formatting to Excel row. If the records span multiple rows, this event is fired only for the last row..</p>
        /// </remarks>        
        public event EventHandler<RecordExportedEventArgs> RecordExported;

        /// <summary>
        /// Occurs before a summary row is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Summaries</i> argument returns a reference to summary values.</p>
        /// <p class="body">The <i>SummaryLevel</i> argument returns a current summary level.</p>
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">This event is fired before Excel row with summary values for specific level is processed. Use the Cancel argument to cancel row exporting. If summaries span multiple Excel rows, this event is fired only for first row of summaries.</p>
        /// </remarks>        
        public event EventHandler<SummaryRowExportingEventArgs> SummaryRowExporting;

        /// <summary>
        /// Occurs after summary record is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Summaries</i> argument returns a reference to summary values.</p>
        /// <p class="body">The <i>SummaryLevel</i> argument returns a current summary level.</p>
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired after Excel row with summary values for specific level is processed. Use it to apply any additional formatting to Excel row. If summaries span multiple Excel rows, this event is fired only for last row of summaries.</p>
        /// </remarks>        
        public event EventHandler<SummaryRowExportedEventArgs> SummaryRowExported;

        /// <summary>
        /// Occurs before header area is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>FieldLayout</i> argument returns a reference the layout object associated with the headers.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the first record associated with the headers.</p>    
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">This event is fired before Excel row with header values is processed. Use Cancel argument to cancel row exporting. If headers span multiple Excel rows, this event is fired only for first header row.</p>
        /// <p class="note">This event is not fired when the labels are displayed with the cells instead of in a separate header area.</p>
        /// </remarks>        
        public event EventHandler<HeaderAreaExportingEventArgs> HeaderAreaExporting;

        /// <summary>
        /// Occurs after header area is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>FieldLayout</i> argument returns a reference the layout object associated with the headers.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the first record associated with the headers.</p>        
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired after Excel row with header values is processed. Use it to apply any additional formatting to Excel row. If headers span multiple Excel rows, this event is fired only for last header row.</p>
        /// <p class="note">This event is not fired when the labels are displayed with the cells instead of in a separate header area.</p>
        /// </remarks>        
        public event EventHandler<HeaderAreaExportedEventArgs> HeaderAreaExported;

        /// <summary>
        /// Occurs before a cell is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Field</i> argument returns a reference to the associated field.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the record containing the cell.</p>
        /// <p class="body">The <i>Value</i> argument returns the value of the cell.</p>
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">Use the Cancel argument to cancel cell exporting.</p>
        /// </remarks>        
        public event EventHandler<CellExportingEventArgs> CellExporting;

        /// <summary>
        /// Occurs after a cell is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Field</i> argument returns a reference to the associated field.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the record containing the cell.</p>
        /// <p class="body">The <i>Value</i> argument returns the value of the cell.</p>
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">Use this event to apply any additional formatting to Excel cell after the cell has been exported.</p>
        /// </remarks>        
        public event EventHandler<CellExportedEventArgs> CellExported;

        /// <summary>
        /// Occurs before a header label is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Field</i> argument returns a reference to the associated field.</p>
        /// <p class="body">The <i>Label</i> argument allows control over what value should be shown in the header cell.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the first record associated with the headers.</p>  
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">Use the Cancel argument to cancel the exporting of this cell.</p>
        /// </remarks>        
        public event EventHandler<HeaderLabelExportingEventArgs> HeaderLabelExporting;

        /// <summary>
        /// Occurs after a header label is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Field</i> argument returns a reference to the associated field.</p>
        /// <p class="body">The <i>Label</i> argument retuns the value that has been exported to the header cell.</p>
        /// <p class="body">The <i>Record</i> argument returns a reference to the first record associated with the headers.</p>  
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">Use this event to apply any additional formatting to Excel cell.</p>
        /// </remarks>        
        public event EventHandler<HeaderLabelExportedEventArgs> HeaderLabelExported;

        /// <summary>
        /// Occurs before a summary cell is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Summaries</i> argument returns a reference to summary values.</p>
        /// <p class="body">The <i>SummaryLevel</i> argument returns a current summary level.</p>
        /// <p class="body">Additionaly this event has Cancel, Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportCancelEventArgs"/>.</p>
        /// <p class="body">Use the Cancel argument to cancel the exporting of the summary cell.</p>
        /// </remarks>        
        public event EventHandler<SummaryCellExportingEventArgs> SummaryCellExporting;

        /// <summary>
        /// Occurs after a summary cell is exported to Excel.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Summary</i> argument returns a reference to a summary value.</p>
        /// <p class="body">The <i>SummaryLevel</i> argument returns current summary level.</p>
        /// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">Use this event to apply any additional formatting to a summary cell.</p>
        /// </remarks>        
        public event EventHandler<SummaryCellExportedEventArgs> SummaryCellExported;

        /// <summary>
        /// Occurs after the exporting process is finished.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <i>Canceled</i> argument is true if exporting process was been canceled.</p>
        /// <p class="body">This event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
        /// <p class="body">This event is fired after exporting process is finished.</p>
		/// <p class="note"><b>Note:</b> The EndExport event has been deprecated. The <see cref="ExportEnding"/> or <see cref="ExportEnded"/> event should be used instead. The direct replacement 
		/// for this event is the ExportEnding but if you used an overload of Export that saved to a file then you should use the <see cref="ExportEnded"/> to show the file since the 
		/// file will not be saved until after the ExportEnding has been raised.</p>
		/// </remarks>        
		[Obsolete("The 'EndExport' method is obsolete. The 'ExportEnding' or 'ExportEnded' event should be used instead. See the comments for those events for more information on which to use.", false)] // AS 2/11/11 NA 2011.1 Word Writer
        public event EventHandler<EndExportEventArgs> EndExport;

		// AS 2/11/11 NA 2011.1 Word Writer
		/// <summary>
		/// Occurs after the export process has started.  The data presenter has been initialized but the data source has not yet been set.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <i>DataPresenter</i> argument returns a reference to the cloned object that is about to be exported.</p>        
		/// <p class="body">Additionaly this event has Workbook, CurrentWorksheet, CurrentRowIndex, CurrentColumnIndex, CurrentOutlineLevel arguments inherited from <see cref="ExcelExportEventArgs"/>.</p>
		/// <p class="body">This event is fired before all other events. You can use it to do any preprocessing before exporting process starts, such as writing a custom header to the Excel workbook.</p>
		/// </remarks>        
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public event EventHandler<ExportStartedEventArgs> ExportStarted;

		// AS 2/11/11 NA 2011.1 Word Writer
		/// <summary>
		/// Invoked after the export process is complete but before the contents are saved to a file.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ExportEnding event is invoked when the export process has completed to give the developer 
		/// one final opportunity to manipulate the Workbook being generated. This is particularly important when using an 
		/// overload of the Export method that takes a filename as the file will be saved after the ExportEnding is complete 
		/// but before the <see cref="ExportEnded"/> so any changes to the Workbook in the ExportEnded will not be reflected in the saved 
		/// file. Also, since the file will not have been created until after the ExportEnding, if you wanted to copy the 
		/// file to another location or display the file to the end user, the ExportEnded event should be used to do that.</p>
		/// </remarks>        
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public event EventHandler<ExportEndingEventArgs> ExportEnding;

		// AS 2/11/11 NA 2011.1 Word Writer
		/// <summary>
		/// Occurs after the export process has completed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ExportEnded event is raised after the export process has been completed. If an overload of 
		/// the Export method that accepts a filename was used, then the file would be available at this point since the file 
		/// is saved after the <see cref="ExportEnding"/> event has been completed but before the ExportEnded. Therefore any 
		/// manipulations of the Workbook should be done in the <see cref="ExportEnding"/> event or else those changes will 
		/// not be reflected in the saved file.</p>
		/// </remarks>        
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		public event EventHandler<ExportEndedEventArgs> ExportEnded;

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