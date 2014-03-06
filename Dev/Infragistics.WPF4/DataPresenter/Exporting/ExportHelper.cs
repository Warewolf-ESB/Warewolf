using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using Infragistics.Collections;
using System.Windows.Threading;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DataPresenter
{
	// AS 3/3/11 NA 2011.1 - Async Exporting
	internal class ExportHelper
	{
		#region Member Variables

		private DataPresenterBase _source;
		private List<ExportRequest> _requestList;
		private WeakList<RecordExportStatusControl> _statusControls;
		private RecordExportStatusControl _internalStatusControl;

		[ThreadStatic]
		private static WeakList<ExportRequest> _asyncExportOperations;

		[ThreadStatic]
		private static DispatcherTimer _asyncExportTimer;

		[ThreadStatic]
		private static WeakReference _currentRequest;

		[ThreadStatic]
		private static bool _isProcessingCurrentRequest;

		[ThreadStatic]
		private static int _synchronousExportCount;

		#endregion //Member Variables

		#region Constructor
		internal ExportHelper(DataPresenterBase source)
		{
			Utilities.ValidateNotNull(source, "source");
			_source = source;
		} 
		#endregion //Constructor

		#region Properties

		#region InternalStatusControl
		internal RecordExportStatusControl InternalStatusControl
		{
			get { return _internalStatusControl; }
		} 
		#endregion //InternalStatusControl

		#region SynchronousExportCount
		private static int SynchronousExportCount
		{
			get { return _synchronousExportCount; }
			set
			{
				if (value != _synchronousExportCount)
				{
					if (_synchronousExportCount == 0)
						StopTimer();

					_synchronousExportCount = value;

					if (_synchronousExportCount == 0)
						StartTimer();
				}
			}
		}
		#endregion //SynchronousExportCount

		#endregion //Properties

		#region Internal Methods

		#region AccessAllRecords
		internal void AccessAllRecords()
		{
			ExportSourceRecordEnumerator rm = new ExportSourceRecordEnumerator(_source.RecordManager);

			while (rm.MoveNext()) ;
		} 
		#endregion //AccessAllRecords

		#region AddStatusControl
		internal void AddStatusControl(RecordExportStatusControl ctrl)
		{
			if (null == _statusControls)
				_statusControls = new WeakList<RecordExportStatusControl>();

			_statusControls.Add(ctrl);
		}
		#endregion //AddStatusControl

		#region CancelExport
		internal bool CancelExport(DataPresenterBase dataPresenter)
		{
			if (null != _asyncExportOperations)
			{
				for (int i = 0; i < _asyncExportOperations.Count; i++)
				{
					ExportRequest request = _asyncExportOperations[i];

					if (request != null && request.ExportHelper._source == dataPresenter)
					{
						CancelExport(request);
						return true;
					}
				}
			}

			return false;
		}

		internal void CancelExport(IDataPresenterExporterAsync exporter)
		{
			if (null != _asyncExportOperations)
			{
				for (int i = 0; i < _asyncExportOperations.Count; i++)
				{
					ExportRequest request = _asyncExportOperations[i];

					if (request != null && request.Exporter == exporter)
					{
						CancelExport(request);
						break;
					}
				}
			}
		}

		private static void CancelExport(ExportRequest request)
		{
			if (null != _asyncExportOperations)
			{
				request.CancelRequested = true;

				// if this is the current request in the queue...
				if (request == Utilities.GetWeakReferenceTargetSafe(_currentRequest))
				{
					// if we're in the middle of processing it then 
					// let the processing logic clean up
					if (_isProcessingCurrentRequest)
						return;

					// otherwise stop the current processing and we'll 
					// restart the timer after we cancel the export below
					_currentRequest = null;
					_asyncExportOperations.Remove(request);
					StopTimer();
				}

				request.CancelExport(RecordExportCancellationReason.Cancelled, null);

				StartTimer();
			}
		}

		#endregion //CancelExport

		#region Export
		internal void Export(IDataPresenterExporter exporter, IExportOptions options)
		{
			Utilities.ValidateNotNull(exporter, "exporter");
			Utilities.ValidateNotNull(options, "options");

			this.ValidateCanExport();

			DataPresenterExportControl dataPresenterControl = new DataPresenterExportControl(_source, exporter);
			try
			{
				SynchronousExportCount++;

				_source.ExportStatus = RecordExportStatus.Initializing;

				this.AccessAllRecords();

				this.InitializeExportControl(dataPresenterControl, options);

				_source.ExportStatus = RecordExportStatus.Exporting;

				// Notify the exporter that we're about to begin the exporting process.  Note that we haven't actually
				// created/cloned any of the records at this point since we haven't set the data source.  This event
				// is mainly used for the basic initialization pre-binding.  Since the various DP events should be firing
				// after the binding, the developer should have the standard customization ability at that point.
				exporter.BeginExport(dataPresenterControl, options);

				// We can bind the control at this point
				dataPresenterControl.BindToDataSource();

				// Iterate through rows
				bool cancelled = new ExportRecordEnumerator(dataPresenterControl, dataPresenterControl.ViewableRecords, options).Process(null) == ExportProcessResult.Cancelled;

				// Finally notify the exporter that the process has completed
				exporter.EndExport(cancelled);
			}
			finally
			{
				dataPresenterControl.IsExporting = false;

				// AS 8/25/11 TFS82921
				dataPresenterControl.OnEndExport();

				_source.ExportStatus = RecordExportStatus.NotExporting;

				SynchronousExportCount--;
			}
		}
		#endregion //Export

		#region ExportAsync
		internal void ExportAsync(IDataPresenterExporterAsync exporter, IExportOptions options, bool showExportStatus)
		{
			this.ExportAsync(exporter, options, showExportStatus, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(20));
		}

		internal void ExportAsync(IDataPresenterExporterAsync exporter, IExportOptions options, bool showExportStatus, TimeSpan duration, TimeSpan interval)
		{
			#region Validation

			Utilities.ValidateNotNull(exporter, "exporter");
			Utilities.ValidateNotNull(options, "options");

			if (duration.Ticks <= 0)
				throw new ArgumentOutOfRangeException("duration");

			if (interval.Ticks <= 0)
				throw new ArgumentOutOfRangeException("interval");

			this.ValidateCanExport();

			#endregion //Validation

			ExportRequest request = new ExportRequest(this, exporter, options, duration, interval, showExportStatus);
			AddAsyncExport(request);
		}
		#endregion //ExportAsync

		#region RemoveStatusControl
		internal void RemoveStatusControl(RecordExportStatusControl ctrl)
		{
			if (null != _statusControls)
			{
				_statusControls.Remove(ctrl);
			}
		}
		#endregion //RemoveStatusControl

		#endregion //Internal Methods

		#region Private Methods

		#region AddAsyncExport
		private static void AddAsyncExport(ExportRequest request)
		{
			if (_asyncExportOperations == null)
				_asyncExportOperations = new WeakList<ExportRequest>();

			if (request.ExportHelper._requestList == null)
				request.ExportHelper._requestList = new List<ExportRequest>();

			ExportHelper helper = request.ExportHelper;

			if (helper._source.ExportStatus == RecordExportStatus.NotExporting)
				helper.UpdateAsyncStatus(request, RecordExportStatus.Pending);

			// store the request
			_asyncExportOperations.Add(request);
			helper._requestList.Add(request);

			// make sure the timer is started
			StartTimer();
		} 
		#endregion //AddAsyncExport 

		#region InitializeExportControl
		private void InitializeExportControl(DataPresenterExportControl exportControl, IExportOptions options)
		{
			// AS 8/19/09 TFS20860
			// Since no one was calling BeginInit, the Initialized event was lazily raised by the frameworkelement. In this 
			// case it was happening when the first logical child was added to the DataPresenter. This should be controlled 
			// so that it doesn't happen in the middle of the field layout's initialization logic.
			//
			exportControl.BeginInit();

			// Set a flag that says that the control is being used for exporting so that we don't allow anyone to set the
			// DataSource except for when we copy it from the source control.
			exportControl.IsExporting = true;

			// MBS 8/25/09
			// Previously we were using a TabularReportView due to the shared logic of ReportViewBase, but now 
			// that was refactored since we don't really need that view.  However, since we do need *a* view,
			// create a new one here before cloning the DataPresenter.
			exportControl.CurrentViewInternal = new ExportView();

			exportControl.CloneSourceDataPresenter(null, options);

			// AS 8/19/09 TFS20860
			exportControl.EndInit();

			// Force the templates to be generated at this point because we'll need all that information when determining
			// where all of the rows should be positioned.
			//
			
			foreach (FieldLayout fieldLayout in exportControl.FieldLayouts)
			{
				fieldLayout.EnsureStyleGeneratorInitialized();
				fieldLayout.VerifyStyleGeneratorTemplates();
			}
		} 
		#endregion //InitializeExportControl

		#region InitializeRequestEnumerator
		private static void InitializeRequestEnumerator(ExportRequest request)
		{
			// indicate that the export process has started
			request.ExportHelper.UpdateAsyncStatus(request, RecordExportStatus.Exporting);

			request.InvokedBeginExport = true;

			// Notify the exporter that we're about to begin the exporting process.  Note that we haven't actually
			// created/cloned any of the records at this point since we haven't set the data source.  This event
			// is mainly used for the basic initialization pre-binding.  Since the various DP events should be firing
			// after the binding, the developer should have the standard customization ability at that point.
			request.Exporter.BeginExport(request.ExportControl, request.ExportOptions);

			// We can bind the control at this point
			request.ExportControl.BindToDataSource();

			// Iterate through rows
			//bool cancelled = !dataPresenterControl.ProcessRecords(options);
			request.ExportRecordEnumerator = new ExportRecordEnumerator(request.ExportControl, request.ExportControl.ViewableRecords, request.ExportOptions);
		}
		#endregion //InitializeRequestEnumerator

		#region InitializeRequestExportControl
		private static void InitializeRequestExportControl(ExportRequest request)
		{
			request.ExportControl = new DataPresenterExportControl(request.ExportHelper._source, request.Exporter);
			request.ExportHelper.InitializeExportControl(request.ExportControl, request.ExportOptions);
		}
		#endregion //InitializeRequestExportControl

		#region InitializeRequestInfo
		private static void InitializeRequestInfo(ExportRequest request)
		{
			Debug.Assert(request.ExportRecordEnumerator == null || request.ExportControl == null, "What is supposed to be initialized?");

			if (request.ExportControl == null)
			{
				InitializeRequestExportControl(request);
			}
			else if (request.ExportRecordEnumerator == null)
			{
				InitializeRequestEnumerator(request);
			}
		}
		#endregion //InitializeRequestInfo 

		#region OnAsyncTimerTick
		private static void OnAsyncTimerTick(object sender, EventArgs e)
		{
			ProcessAsyncExportBlock();
		} 
		#endregion //OnAsyncTimerTick

		#region ProcessAsyncExportBlock()
		private static void ProcessAsyncExportBlock()
		{
			StopTimer();

			if (_asyncExportOperations != null)
			{
				ExportRequest nextRequest = Utilities.GetWeakReferenceTargetSafe(_currentRequest) as ExportRequest;

				Debug.Assert(nextRequest != null, "A request was released but not cancelled?");

				if (null != nextRequest)
				{
					Debug.Assert(!_isProcessingCurrentRequest, "Already processing a request?");
					try
					{
						_isProcessingCurrentRequest = true;

						// if this request processes and has more to go then 
						// we can break out and restart the timer
						bool continueExport = ProcessAsyncExportBlock(nextRequest);
						if (!continueExport || nextRequest.CancelRequested)
						{
							_asyncExportOperations.Remove(nextRequest);
							_currentRequest = null;

							// if a cancel came in while we were processing the request...
							if (continueExport && nextRequest.CancelRequested)
								nextRequest.CancelExport(RecordExportCancellationReason.Cancelled, null);

							// also clear the nextRequest so we don't try to 
							// use this to restart the timer should there be 
							// no more items in the async list
							nextRequest = null;
						}
					}
					finally
					{
						_isProcessingCurrentRequest = false;
					}
				}

				// if we're not done with the current request then restart the timer 
				// for that request's interval
				if (nextRequest != null)
					StartTimer(nextRequest.InterProcessDelay);
				else
				{
					// otherwise remove any dead/released entries and restart the timer
					_asyncExportOperations.Compact();
					StartTimer();
				}
			}
		}
		#endregion //ProcessAsyncExportBlock()

		#region ProcessAsyncExportBlock(ExportRequest)
		private static bool ProcessAsyncExportBlock(ExportRequest request)
		{
			try
			{
				if (request.PendingEndExportResult != null)
				{
					ExportProcessResult result = request.PendingEndExportResult.Value;
					request.PendingEndExportResult = null;

					switch (result)
					{
						case ExportProcessResult.Cancelled:
							request.CancelExport(RecordExportCancellationReason.TerminateExport, null);
							return false;
						case ExportProcessResult.Completed:
							request.EndExport();
							return false;
						default:
							Debug.Fail("Unexpected result:" + result.ToString());
							break;
					}
				}

				if (request.ExportRecordEnumerator != null)
				{
					ExportProcessResult result = request.ExportRecordEnumerator.Process(request.ProcessDuration);

					request.ExportHelper.UpdateRecordCount(request.ExportRecordEnumerator.ProcessedRecordCount);

					switch (result)
					{
						case ExportProcessResult.Timeout:
							return true;
						case ExportProcessResult.Cancelled:
						case ExportProcessResult.Completed:
							// because the end of the operation could take some time, if we've started 
							// exporting then wait for the next slice to actually end the operation
							request.PendingEndExportResult = result;
							return true;
					}

					Debug.Fail("Unrecognized result:" + result.ToString());
					request.CancelExport(RecordExportCancellationReason.Cancelled, null);
					return false;
				}
				else if (!request.EnumeratedSourceRecords)
				{
					// indicate that we are in the process of copying the source info
					request.ExportHelper.UpdateAsyncStatus(request, RecordExportStatus.Initializing);

					ExportProcessResult result = request.SourceRecordEnumerator.Process(request.ProcessDuration);

					if (result == ExportProcessResult.Timeout)
						return true;
					else if (result == ExportProcessResult.Cancelled)
					{
						request.CancelExport(RecordExportCancellationReason.Cancelled, null);
						return false;
					}
					else
					{
						request.EnumeratedSourceRecords = true;
						return true;
					}
				}
				else
				{
					InitializeRequestInfo(request);
					return true;
				}
			}
			catch (Exception ex)
			{
				request.CancelExport(RecordExportCancellationReason.Exception, ex);
				return false;
			}
		}
		#endregion //ProcessAsyncExportBlock(ExportRequest)

		#region RemoveInternalStatusControl
		private void RemoveInternalStatusControl()
		{
			if (null != _internalStatusControl)
			{
				RecordExportStatusControl statusControl = _internalStatusControl;
				_internalStatusControl = null;

				// remove from the datapresenter
				_source.RemoveStatusControl(statusControl);
			}
		}
		#endregion //RemoveInternalStatusControl

		#region ShowInternalStatusControl
		private void ShowInternalStatusControl(ExportRequest request)
		{
			Debug.Assert(_internalStatusControl == null);

			this.RemoveInternalStatusControl();

			RecordExportStatusControl ctrl = new RecordExportStatusControl();
			ctrl.DataPresenter = _source;
			ctrl.ExportInfo = request.Exporter.ExportInfo;

			_internalStatusControl = ctrl;

			_source.AddStatusControl(ctrl);
		}
		#endregion //ShowInternalStatusControl

		#region StartTimer
		private static void StartTimer()
		{
			// if we're processing one then we've stopped the timer and 
			// will restart after we finish processing the current request
			if (_isProcessingCurrentRequest)
				return;

			ExportRequest nextRequest = Utilities.GetWeakReferenceTargetSafe(_currentRequest) as ExportRequest;

			// if we've already got a current request and the timer is started we don't need to do anything
			if (nextRequest != null && _asyncExportTimer != null && _asyncExportTimer.IsEnabled)
				return;

			if (_asyncExportOperations != null)
			{
				for (int i = 0; i < _asyncExportOperations.Count; i++)
				{
					nextRequest = _asyncExportOperations[i];

					if (nextRequest != null)
						break;
				}
			}

			_currentRequest = nextRequest == null ? null : new WeakReference(nextRequest);

			if (nextRequest == null)
				StopTimer();
			else
				StartTimer(nextRequest.InterProcessDelay);
		}

		private static void StartTimer(TimeSpan interval)
		{
			// if we have synchronous exporting going on then wait until
			// the synchronous operation is complete before starting the timer
			if (_synchronousExportCount > 0)
				return;

			const double MaxTimerInterval = 2147483647;

			if (interval.TotalMilliseconds > MaxTimerInterval)
				interval = TimeSpan.FromMilliseconds(MaxTimerInterval);

			if (_asyncExportTimer == null)
				_asyncExportTimer = new DispatcherTimer(interval, DispatcherPriority.Background, new EventHandler(OnAsyncTimerTick), Dispatcher.CurrentDispatcher);

			_asyncExportTimer.Interval = interval;
			_asyncExportTimer.Start();
		}
		#endregion //StartTimer

		#region StopTimer
		private static void StopTimer()
		{
			if (_asyncExportTimer != null)
				_asyncExportTimer.Stop();
		} 
		#endregion //StopTimer

		#region UpdateAsyncStatus
		private void UpdateAsyncStatus(ExportRequest request, RecordExportStatus status)
		{
			_source.ExportStatus = status;

			if (status != RecordExportStatus.NotExporting && request.ShowExportStatus)
			{
				request.ShowExportStatus = false;
				this.ShowInternalStatusControl(request);
			}

			if (null != _statusControls)
			{
				ExportInfo exportInfo = status == RecordExportStatus.NotExporting ? null : request.Exporter.ExportInfo;

				foreach (RecordExportStatusControl ctrl in _statusControls)
				{
					if (null != ctrl)
					{
						ctrl.Status = status;
						ctrl.ExportInfo = exportInfo;
					}
				}
			}

			if (status == RecordExportStatus.NotExporting)
				this.RemoveInternalStatusControl();
		}
		#endregion //UpdateAsyncStatus

		#region UpdateRecordCount
		private void UpdateRecordCount(int processedRecordCount)
		{
			if (null != _statusControls)
			{
				foreach (RecordExportStatusControl ctrl in _statusControls)
				{
					if (null != ctrl)
						ctrl.ExportedRecordCount = processedRecordCount;
				}
			}
		}
		#endregion //UpdateRecordCount

		#region ValidateCanExport
		private void ValidateCanExport()
		{
			if (_source.IsExporting)
				throw new InvalidOperationException(DataPresenterBase.GetString("Export_CannotExportMultipleTimes"));
		}
		#endregion //ValidateCanExport

		#endregion //Private Methods

		#region ExportRequest class
		private class ExportRequest
		{
			#region Member Variables

			internal readonly ExportHelper ExportHelper;
			internal readonly IDataPresenterExporterAsync Exporter;
			internal readonly IExportOptions ExportOptions;
			internal readonly ExportSourceRecordEnumerator SourceRecordEnumerator;
			internal readonly TimeSpan ProcessDuration;
			internal readonly TimeSpan InterProcessDelay;

			internal DataPresenterExportControl ExportControl;
			internal ExportRecordEnumerator ExportRecordEnumerator;
			internal bool InvokedBeginExport;
			internal bool EnumeratedSourceRecords;
			internal bool CancelRequested;
			internal bool ShowExportStatus;
			internal ExportProcessResult? PendingEndExportResult;

			#endregion //Member Variables

			#region Constructor
			internal ExportRequest(ExportHelper exportHelper, IDataPresenterExporterAsync exporter, IExportOptions options, TimeSpan processDuration, TimeSpan interprocessDelay, bool showExportStatus)
			{
				this.ExportHelper = exportHelper;
				this.Exporter = exporter;
				this.ExportOptions = options;
				this.ProcessDuration = processDuration;
				this.InterProcessDelay = interprocessDelay;
				this.SourceRecordEnumerator = new ExportSourceRecordEnumerator(exportHelper._source.RecordManager);
				this.ShowExportStatus = showExportStatus;
			} 
			#endregion //Constructor

			#region Methods

			#region CancelExport
			internal void CancelExport(RecordExportCancellationReason reason, Exception exception)
			{
				this.Exporter.CancelExport(new RecordExportCancellationInfo(reason, exception));
				this.EndExport(true);
			} 
			#endregion //CancelExport

			#region EndExport
			internal void EndExport()
			{
				this.EndExport(false);
			}

			private void EndExport(bool cancelled)
			{
				this.Exporter.EndExport(cancelled);

				if (this.ExportControl != null)
				{
					this.ExportControl.IsExporting = false;

					// AS 8/25/11 TFS82921
					this.ExportControl.OnEndExport();
				}

				this.ExportHelper.UpdateAsyncStatus(this, RecordExportStatus.NotExporting);
			}
			#endregion //EndExport

			#endregion //Methods
		} 
		#endregion //ExportRequest class
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