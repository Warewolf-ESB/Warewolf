using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel;
using System.Diagnostics;
using System.Drawing;

namespace Infragistics.Windows.DataPresenter.ExcelExporter
{






    internal class DataPresenterExcelExporterHelper : IDataPresenterExporter
		// AS 3/3/11 NA 2011.1 - Async Exporting
		, IDataPresenterExporterAsync
    {
        #region Members
        
        private Worksheet _currentWorksheet;
        private DataPresenterExcelExporter _excelExporter;
        private int _hierarchyLevel;
        private Dictionary<RecordCollectionBase, Record> _lastExportedRecordTable = new Dictionary<RecordCollectionBase, Record>();
        private FieldLayout _lastFieldLayout;
        private Record _lastRecord;

		// AS 2/11/11 NA 2011.1 Word Writer
		private string _fileName; 
		private Workbook _workbook;

		// AS 3/3/11 NA 2011.1 - Async Exporting
		private DataPresenterBase _source;
		private DataPresenterExportCache _exportCache;

		// AS 3/10/11 NA 2011.1 - Async Exporting
		private ExportInfo _exportInfo;
		private InternalFlags _flags;

        internal Point currentPos;
        internal Rectangle extentOfCurrentGridObject;
        internal Point startPos;

        #endregion //Members

        #region Constructor

        internal DataPresenterExcelExporterHelper(DataPresenterExcelExporter excelExporter,
            Worksheet currentWorksheet, int startRow, int startColumn,
			string filename ,
			DataPresenterBase source )
        {
            this._excelExporter = excelExporter;
            this._currentWorksheet = currentWorksheet;

            this.startPos = new Point(startColumn, startRow);
            this.currentPos = this.startPos;

			// AS 2/11/11 NA 2011.1 Word Writer
			this._fileName = filename;
			this._workbook = currentWorksheet.Workbook;

			// AS 3/3/11 NA 2011.1 - Async Exporting
			this._source = source;
			this._exportCache = new DataPresenterExportCache();
			excelExporter.AddExporter(this);

			// AS 3/10/11 NA 2011.1 - Async Exporting
			_exportInfo = new ExportInfo();
			_exportInfo.ExportType = "Microsoft Excel";
			_exportInfo.FileName = _fileName;
        }
        #endregion //Constructor

        #region Properties

        #region CurrentWorksheet

        internal Worksheet CurrentWorksheet
        {
            get { return this._currentWorksheet; }
            set { this._currentWorksheet = value; }
        }
        #endregion //CurrentWorksheet

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region ExportCache
		internal DataPresenterExportCache ExportCache
		{
			get
			{
				return _exportCache;
			}
		} 
		#endregion //ExportCache

		// AS 2/11/11 NA 2011.1 Word Writer
		#region FileName
		internal string FileName
		{
			get { return _fileName; }
		} 
		#endregion //FileName

        #region HierarchyLevel

        internal int HierarchyLevel
        {
            get { return this._hierarchyLevel; }
            set { this._hierarchyLevel = value; }
        }
        #endregion //HierarchyLevel

        #region LastFieldLayout

        internal FieldLayout LastFieldLayout
        {
            get { return this._lastFieldLayout; }
        }
        #endregion //LastFieldLayout

        #region LastRecord







        internal Record LastExportedRecord
        {
            get { return this._lastRecord; }
        }
        #endregion //LastRecord

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region Source
		internal DataPresenterBase Source
		{
			get { return _source; }
		} 
		#endregion //Source

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region WasBeginExportInvoked
		internal bool WasBeginExportInvoked
		{
			get { return this.GetFlag(InternalFlags.InvokedBeginExport); }
		}
		#endregion //WasBeginExportInvoked

		// AS 2/11/11 NA 2011.1 Word Writer
		#region Workbook
		internal Workbook Workbook
		{
			get { return _workbook; }
		} 
		#endregion //Workbook

        #endregion //Properties

        #region Methods

		#region Private Methods

		// AS 3/10/11 NA 2011.1 - Async Exporting
		#region EndExport
		private void EndExport(RecordExportCancellationInfo cancelInfo)
		{
			if (this.GetFlag(InternalFlags.ExportEnded))
				return;

			this.SetFlag(InternalFlags.ExportEnded, true);

			this._excelExporter.EndExportInternal(this, cancelInfo);
		}
		#endregion //EndExport

		// AS 3/10/11 NA 2011.1 - Async Exporting
		#region GetFlag
		private bool GetFlag(InternalFlags flag)
		{
			return (flag & _flags) == flag;
		}
		#endregion //GetFlag

		// AS 3/10/11 NA 2011.1 - Async Exporting
		#region SetFlag
		private void SetFlag(InternalFlags flag, bool value)
		{
			if (value)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion //SetFlag

		#endregion //Private Methods

		#region Internal Methods

		#region GetLastExportedRecord

		internal Record GetLastExportedRecord(RecordCollectionBase parentCollection)
		{
			Record record;
			if (this._lastExportedRecordTable.TryGetValue(parentCollection, out record))
				return record;

			return null;
		}
		#endregion //GetLastExportedRecord

		#endregion //Internal Methods

        #endregion //Methods

        #region Interface Implementation

        public void BeginExport(DataPresenterBase dataPresenter, IExportOptions options)
        {
			// AS 3/3/11 NA 2011.1 - Async Exporting
			this.SetFlag(InternalFlags.InvokedBeginExport, true);

            ExportOptions exportOptions = options as ExportOptions;
            if (exportOptions == null)
            {
                Debug.Fail("Expected to have an ExportOptions object");

                // Create a default object, since this won't control any aspects of the 
                // IExportOptions interface, which the DataPresenter is already using internally
                exportOptions = new ExportOptions();
            }

            this._excelExporter.BeginExportInternal(this, dataPresenter, exportOptions);

			// MD 6/7/10 - ChildRecordsDisplayOrder feature
			if (dataPresenter.FieldLayouts.Count > 0)
			{
				bool showIndicatorBelowGroup = (dataPresenter.FieldLayouts[0].ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParent);
				this.CurrentWorksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRows = showIndicatorBelowGroup 
					? ExcelDefaultableBoolean.True 
					: ExcelDefaultableBoolean.False;
			}
        }

        public void EndExport(bool cancelled)
        {
			// AS 3/10/11 NA 2011.1 - Async Exporting
			//this._excelExporter.EndExportInternal(this, cancelled);
			RecordExportCancellationInfo cancelInfo = null;

			if (cancelled)
				cancelInfo = new RecordExportCancellationInfo(RecordExportCancellationReason.Unknown, null);

			this.EndExport(cancelInfo);
		}

		// MD 6/7/10 - ChildRecordsDisplayOrder feature
		public bool InitializeRecord(Record record, ProcessRecordParams processRecordParams)
		{
			// AS 2/22/11 NA 2011.1 Word Writer
			// InitializeRecord and ProcessRecord are now being invoked for ExpandableFieldRecords
			// so to maintain the previous behavior where these records were always traversed into 
			// we will simply exit.
			//
			if (record is ExpandableFieldRecord)
				return true;

			return this._excelExporter.InitializeRecordInternal(this, record, processRecordParams);
		}

        public void ProcessRecord(Record record, ProcessRecordParams processRecordParams)
        {
			// AS 2/22/11 NA 2011.1 Word Writer - See InitializeRecord comments
			if (record is ExpandableFieldRecord)
				return;

            if (this._excelExporter.ProcessRecordInternal(this, record, processRecordParams))
            {
                // If the record is exported, keep track of the last record and its FieldLayout so that we can make certain
                // decisions about things like header placement when exporting the next row
                this._lastFieldLayout = record.FieldLayout;
                this._lastRecord = record;
                this._lastExportedRecordTable[record.ParentCollection] = record;
            }
        }

        public void OnObjectCloned(System.Windows.DependencyObject sourceDependencyObject, System.Windows.DependencyObject targetDependencyObject)
        {
            // We need to ensure that we copy over the various FormatSettings objects to the cloned DataPresenter
            FieldSettings sourceFieldSettings = sourceDependencyObject as FieldSettings;
            FieldSettings targetFieldSettings = targetDependencyObject as FieldSettings;
            if (sourceFieldSettings != null && targetFieldSettings != null)
            {
                FormatSettings cellFormatSettings = DataPresenterExcelExporter.GetExcelCellFormatSettings(sourceFieldSettings);
                DataPresenterExcelExporter.SetExcelCellFormatSettings(targetFieldSettings, cellFormatSettings);

                FormatSettings groupFormatSettings = DataPresenterExcelExporter.GetExcelGroupFormatSettings(sourceFieldSettings);
                DataPresenterExcelExporter.SetExcelGroupFormatSettings(targetFieldSettings, groupFormatSettings);

                FormatSettings labelFormatSettings = DataPresenterExcelExporter.GetExcelLabelFormatSettings(sourceFieldSettings);
                DataPresenterExcelExporter.SetExcelLabelFormatSettings(targetFieldSettings, labelFormatSettings);

                FormatSettings summaryCellFormatSettings = DataPresenterExcelExporter.GetExcelSummaryCellFormatSettings(sourceFieldSettings);
                DataPresenterExcelExporter.SetExcelSummaryCellFormatSettings(targetFieldSettings, summaryCellFormatSettings);

                FormatSettings summaryLabelFormatSettings = DataPresenterExcelExporter.GetExcelSummaryLabelFormatSettings(sourceFieldSettings);
                DataPresenterExcelExporter.SetExcelSummaryLabelFormatSettings(targetFieldSettings, summaryLabelFormatSettings);
            }
        }

        #endregion //Interface Implementation

		// AS 3/10/11 NA 2011.1 - Async Exporting
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

		// AS 3/10/11 NA 2011.1 - Async Exporting
		#region InternalFlags enum
		[Flags]
		private enum InternalFlags
		{
			InvokedBeginExport = 1 << 0,
			ExportStarted = 1 << 1,
			ExportEnded = 1 << 2,
		}
		#endregion //InternalFlags enum
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