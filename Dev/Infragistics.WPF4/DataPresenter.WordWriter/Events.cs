using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Word;
using System.Collections.ObjectModel;
using System.IO;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	#region ExportStartedEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.ExportStarted"/> event.
	/// </summary>
	public class ExportStartedEventArgs : EventArgs
	{
		#region Member Variables

		private DataPresenterBase _source;
		private DataPresenterBase _exportControl;
		private WordDocumentWriter _writer;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExportStartedEventArgs"/>
		/// </summary>
		/// <param name="writer">The writer being used for the exporter operation.</param>
		/// <param name="source">The control being exported</param>
		/// <param name="exportControl">The copy of the <paramref name="source"/> that is being used for the export operation.</param>
		internal ExportStartedEventArgs(WordDocumentWriter writer, DataPresenterBase source, DataPresenterBase exportControl)
		{
			Utilities.ValidateNotNull(writer, "writer");
			Utilities.ValidateNotNull(source, "source");
			Utilities.ValidateNotNull(exportControl, "exportControl");

			_source = source;
			_exportControl = exportControl;
			_writer = writer;
		}
		#endregion //Constructor

		#region Properties

		#region ExportControl
		/// <summary>
		/// Returns the <see cref="DataPresenterBase"/> created for the export operation whose settings were initialized based on the <see cref="Source"/> control provided to the <see cref="DataPresenterWordWriter.Export(DataPresenterBase,Stream)"/> method.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The DataPresenter has been intialized from the original
		/// <see cref="Source"/>, but the <see cref="DataPresenterBase.DataSource"/> property has not been copied.  
		/// Attempting to set the DataSource manually will result in an exception.</p>
		/// </remarks>
		public DataPresenterBase ExportControl
		{
			get { return _exportControl; }
		}
		#endregion //ExportControl

		#region Source
		/// <summary>
		/// Returns the <see cref="DataPresenterBase"/> passed into the <see cref="DataPresenterWordWriter.Export(DataPresenterBase,Stream)"/> method.
		/// </summary>
		public DataPresenterBase Source
		{
			get { return _source; }
		}
		#endregion //Source

		#region Writer
		/// <summary>
		/// Returns the writer that will be used to perform the export operation and generate the Word document.
		/// </summary>
		public WordDocumentWriter Writer
		{
			get { return _writer; }
		}
		#endregion //Writer

		#endregion //Properties
	} 
	#endregion //ExportStartedEventArgs

	#region ExportEndingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.ExportEnding"/> event.
	/// </summary>
	public class ExportEndingEventArgs : EventArgs
	{
		#region Member Variables

		private RecordExportCancellationInfo _cancelInfo;
		private DataPresenterBase _source;
		private DataPresenterBase _exportControl;
		private WordDocumentWriter _writer;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExportEndingEventArgs"/>
		/// </summary>
		/// <param name="writer">The writer being used for the exporter operation.</param>
		/// <param name="source">The control being exported</param>
		/// <param name="exportControl">The copy of the <paramref name="source"/> that is being used for the export operation.</param>
		/// <param name="cancelInfo">Provides information about the cancellation or is null if the operation completed successfully.</param>
		internal ExportEndingEventArgs(WordDocumentWriter writer, DataPresenterBase source, DataPresenterBase exportControl, RecordExportCancellationInfo cancelInfo)
		{
			Utilities.ValidateNotNull(writer, "writer");
			Utilities.ValidateNotNull(source, "source");
			Utilities.ValidateNotNull(exportControl, "exportControl");

			_source = source;
			_exportControl = exportControl;
			_writer = writer;
			_cancelInfo = cancelInfo;
		}
		#endregion //Constructor

		#region Properties

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
		/// Returns a boolean indicating if the export operation was canceled.
		/// </summary>
		public bool Canceled
		{
			get { return _cancelInfo != null; }
		}
		#endregion //Canceled

		#region ExportControl
		/// <summary>
		/// Returns the <see cref="DataPresenterBase"/> created for the export operation whose settings were initialized based on the <see cref="Source"/> control provided to the <see cref="DataPresenterWordWriter.Export(DataPresenterBase,Stream)"/> method.
		/// </summary>
		public DataPresenterBase ExportControl
		{
			get { return _exportControl; }
		}
		#endregion //ExportControl

		#region Source
		/// <summary>
		/// Returns the <see cref="DataPresenterBase"/> passed into the <see cref="DataPresenterWordWriter.Export(DataPresenterBase,Stream)"/> method.
		/// </summary>
		public DataPresenterBase Source
		{
			get { return _source; }
		}
		#endregion //Source

		#region Writer
		/// <summary>
		/// Returns the writer that will be used to perform the export operation and generate the Word document.
		/// </summary>
		public WordDocumentWriter Writer
		{
			get { return _writer; }
		}
		#endregion //Writer

		#endregion //Properties
	} 
	#endregion //ExportEndingEventArgs

	#region ExportEndedEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.ExportEnded"/> event.
	/// </summary>
	public class ExportEndedEventArgs : EventArgs
	{
		#region Member Variables

		private RecordExportCancellationInfo _cancelInfo;
		private DataPresenterBase _source;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExportEndedEventArgs"/>
		/// </summary>
		/// <param name="source">The control being exported</param>
		/// <param name="cancelInfo">Provides information about the cancellation or is null if the operation completed successfully.</param>
		internal ExportEndedEventArgs(DataPresenterBase source, RecordExportCancellationInfo cancelInfo)
		{
			Utilities.ValidateNotNull(source, "source");

			_source = source;
			_cancelInfo = cancelInfo;
		}
		#endregion //Constructor

		#region Properties

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
		/// Returns a boolean indicating if the export operation was canceled.
		/// </summary>
		public bool Canceled
		{
			get { return _cancelInfo != null; }
		}
		#endregion //Canceled

		#region Source
		/// <summary>
		/// Returns the <see cref="DataPresenterBase"/> passed into the <see cref="DataPresenterWordWriter.Export(DataPresenterBase,Stream)"/> method.
		/// </summary>
		public DataPresenterBase Source
		{
			get { return _source; }
		}
		#endregion //Source

		#endregion //Properties
	}
	#endregion //ExportEndedEventArgs

	#region InitializeRecordEventArgs

	/// <summary>
	/// Event parameters used for the <see cref="DataPresenterWordWriter.InitializeRecord"/> event.
	/// </summary>
	public class InitializeRecordEventArgs : EventArgs
	{
		#region Private Members

		private Record record;
		private ProcessRecordParams processRecordParams;
		private bool skipRecord;

		#endregion //Private Members

		#region Constructor

		internal InitializeRecordEventArgs(Record record, ProcessRecordParams processRecordParams)
		{
			Utilities.ValidateNotNull(record, "record");
			Utilities.ValidateNotNull(processRecordParams, "processRecordParams");

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

	#region CellExportingEventArgsBase
	/// <summary>
	/// Base class for an export event involving a value of a record.
	/// </summary>
	public class CellExportingEventArgsBase : EventArgs
	{
		#region Member Variables

		private object _value;
		private Record _record;
		private Field _field;
		private WordTableCellSettings _cellSettings;
		private WordTableCellSettings _cloneCellSettings;

		#endregion //Member Variables

		#region Constructor
		internal CellExportingEventArgsBase(Field field, Record record, object value, WordTableCellSettings cellSettings)
		{
			Utilities.ValidateNotNull(record, "record");

			_record = record;
			_value = value;
			_field = field; // this can be null
			_cellSettings = cellSettings;
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region CellSettings
		/// <summary>
		/// Returns the resolved settings to be applied to the cell.
		/// </summary>
		public WordTableCellSettings CellSettings
		{
			get
			{
				if (_cloneCellSettings == null && _cellSettings != null)
				{
					_cloneCellSettings = new CloneManager().Clone(_cellSettings) as WordTableCellSettings;
				}

				return _cloneCellSettings;
			}
		}
		#endregion //CellSettings

		#region Field
		/// <summary>
		/// Returns the Field associated with the value being exported or null if there is no field associated with the value.
		/// </summary>
		public Field Field
		{
			get { return _field; }
		}
		#endregion //Field

		#region Record
		/// <summary>
		/// Returns the associated Record.
		/// </summary>
		public Record Record
		{
			get { return _record; }
		}
		#endregion //Record

		#region Value
		/// <summary>
		/// Returns or sets the value that should be written for the cell.
		/// </summary>
		public object Value
		{
			get { return _value; }
			set { _value = value; }
		}
		#endregion //Value

		#endregion //Public Properties

		#region Internal Properties

		#region CellSettingsInternal
		internal WordTableCellSettings CellSettingsInternal
		{
			get { return _cloneCellSettings ?? _cellSettings; }
		}
		#endregion //CellSettingsInternal

		#endregion //Internal Properties

		#endregion //Properties
	} 
	#endregion //CellExportingEventArgsBase

	#region CellExportingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.CellExporting"/> event
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note:</b> The <see cref="CellExportingEventArgsBase.Value"/> does not necessarily represent the 
	/// <see cref="Infragistics.Windows.DataPresenter.Cell.Value"/>. Instead it returns the formatted display text as provided by the ValueEditor associated with 
	/// the Field.</p>
	/// </remarks>
	public class CellExportingEventArgs : CellExportingEventArgsBase
	{
		#region Constructor
		internal CellExportingEventArgs(Field field, DataRecord record, object value, WordTableCellSettings cellSettings)
			: base(field, record, value, cellSettings)
		{
			Utilities.ValidateNotNull(field, "field");
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the cell whose value is being exported.
		/// </summary>
		public Cell Cell
		{
			get { return this.Record.Cells[this.Field]; }
		}

		/// <summary>
		/// Returns the DataRecord whose cell is being exported.
		/// </summary>
		public new DataRecord Record
		{
			get { return base.Record as DataRecord; }
		}
		#endregion //Properties
	} 
	#endregion //CellExportingEventArgs

	#region LabelExportingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.LabelExporting"/> event
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note:</b> the <see cref="CellExportingEventArgsBase.Record"/> property returns the 
	/// record for which the header is being generated. Depending on setting of the DataPresenter being exported 
	/// the header may be separated from the record (e.g. if the ChildRecordsDisplayOrder is BeforeParent.</p>
	/// </remarks>
	public class LabelExportingEventArgs : CellExportingEventArgsBase
	{
		#region Constructor
		internal LabelExportingEventArgs(Field field, Record record, object value, WordTableCellSettings cellSettings)
			: base(field, record, value, cellSettings)
		{
			Utilities.ValidateNotNull(field, "field");
		}
		#endregion //Constructor
	} 
	#endregion //LabelExportingEventArgs

	#region SummaryResultsExportingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="DataPresenterWordWriter.SummaryResultsExporting"/> event
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="CellExportingEventArgsBase.Record"/> property returns the record 
	/// associated with the summaries and could be either a <see cref="GroupByRecord"/> or a 
	/// <see cref="SummaryRecord"/> although it is possible that in future this could return another 
	/// type if required.</p>
	/// <p class="body">The <see cref="CellExportingEventArgsBase.Field"/> property represents the 
	/// Field with which the summaries are being aligned when the <see cref="SummaryDefinition.Position"/> 
	/// is set to <b>UseSummaryPositionField</b> and will be null for all other cases.</p>
	/// </remarks>
	public class SummaryResultsExportingEventArgs : CellExportingEventArgsBase
	{
		#region Member Variables

		private IList<SummaryResult> _results;
		private SummaryPosition _position;
		private ReadOnlyCollection<SummaryResult> _readOnlyResults;

		#endregion //Member Variables

		#region Constructor
		internal SummaryResultsExportingEventArgs(IList<SummaryResult> results, SummaryPosition position, Field positionField, Record record, object value, WordTableCellSettings cellSettings)
			: base(positionField, record, value, cellSettings)
		{
			Utilities.ValidateNotNull(results, "results");
			_results = results;
			_position = position;
		}
		#endregion //Constructor

		#region Properties

		#region Position
		/// <summary>
		/// Returns an enumeration indicating the position of the summary.
		/// </summary>
		public SummaryPosition Position
		{
			get { return _position; }
		}
		#endregion //Position

		#region SummaryResults
		/// <summary>
		/// Returns a read-only collection of the results being output.
		/// </summary>
		public ReadOnlyCollection<SummaryResult> SummaryResults
		{
			get
			{
				if (_readOnlyResults == null)
					_readOnlyResults = new ReadOnlyCollection<SummaryResult>(_results);

				return _readOnlyResults;
			}
		}
		#endregion //SummaryResults

		#endregion //Properties
	} 
	#endregion //SummaryResultsExportingEventArgs
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