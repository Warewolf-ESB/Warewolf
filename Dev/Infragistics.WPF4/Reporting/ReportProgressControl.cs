using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using Infragistics.Windows.Reporting.Events;
using System.Windows.Threading;
using System.ComponentModel;
using Infragistics.Shared;
using System.Diagnostics;

namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// Indicates the progress of a print operation.
    /// </summary>
    /// <remarks>
    /// <p class="body">To show this control automatically on a modeless window, you can call <see cref="Infragistics.Windows.Reporting.Report.Print(bool,bool)"/> method. 
    /// Otherwise, if you are using it on one of your windows you should set the <see cref="Report"/> property to wire things up.</p>
    /// </remarks>
    [TemplatePart(Name = "PART_ProgressBar", Type = typeof(ProgressBar))]
    [TemplatePart(Name = "PART_Description", Type = typeof(ContentControl))]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class ReportProgressControl : Control
    {
        #region Member Variables

        private delegate void SetUI(PrintProgressEventArgs e, object frame);

        ProgressBar _progressBar;
        ContentControl _description;
        Report _report;
        bool _mustCancel;
        
        #endregion

        #region Constructors

        static ReportProgressControl()
        {
            // override custom style
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ReportProgressControl), new FrameworkPropertyMetadata(typeof(ReportProgressControl)));
            // Specify the gesture that triggers the command:
            CommandManager.RegisterClassInputBinding(typeof(ReportProgressControl), new InputBinding(CancelCommand, new MouseGesture(MouseAction.LeftClick)));
            // Attach the command to custom logic:
            CommandManager.RegisterClassCommandBinding(typeof(ReportProgressControl), new CommandBinding(CancelCommand, CancelCommandHandler));
        }
        /// <summary>
        /// Creates a new instance of the <see cref="ReportProgressControl"/> class
        /// </summary>
        public ReportProgressControl()
        {
            // JJD 2/11/09 - TFS10860/TFS13609
            // Moved to static constructor
            //// Specify the gesture that triggers the command:
            //CommandManager.RegisterClassInputBinding(typeof(ReportProgressControl), new InputBinding(CancelCommand, new MouseGesture(MouseAction.LeftClick)));
            //// Attach the command to custom logic:
            //CommandManager.RegisterClassCommandBinding(typeof(ReportProgressControl), new CommandBinding(CancelCommand, CancelCommandHandler));
            this._mustCancel = false;
            this.Unloaded += new RoutedEventHandler(ProgressWindow_Unloaded);
        }

        void ProgressWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            UnSubscribe();
        }
        #endregion

        #region Base Class Overrides

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            this._progressBar = base.GetTemplateChild("PART_ProgressBar") as ProgressBar;
            this._description = base.GetTemplateChild("PART_Description") as ContentControl;
        }

        #endregion Base class overrides

        #region Properties

            #region Public Properties

                #region Report
        /// <summary>
        /// Returns/sets <see cref="Infragistics.Windows.Reporting.Report"/> object whose progress will be indicated.
        /// </summary>
        //[Description("Returns/sets report object whose progress will be indicated.")]
        //[Category("Reporting Properties")] // Behavior
        public Report Report
        {
            set
            {
				// JJD 2/9/11
				// Only process if the value has changed
				if (_report != value)
				{
					if (this._report != null)
					{
						UnSubscribe();
					}

					_report = value;

					// JJD 2/9/11
					// Check for a null value
					if (_report != null)
					{
						_report.PrintStart += new EventHandler(Report_PrintStart);
						_report.PrintProgress += new EventHandler<PrintProgressEventArgs>(Report_PrintProgress);
						_report.PrintEnded += new EventHandler<PrintEndedEventArgs>(Report_PrintEnded);
					}
				}
            }
            get { return _report; }
        }

        #endregion //Report

            #endregion //Public Properties

            #region Internal Properties

            #endregion //Internal Properties

        #endregion // Properties

        #region Methods

            #region Internal methods
        internal void UnSubscribe()
        {
            if (_report == null)
                return;
            _report.PrintStart -= new EventHandler(Report_PrintStart);
            _report.PrintProgress -= new EventHandler<PrintProgressEventArgs>(Report_PrintProgress);
            _report.PrintEnded -= new EventHandler<PrintEndedEventArgs>(Report_PrintEnded);
        }

            #endregion //Internal method

            #region Private Methods

                #region Report_PrintStart

        void Report_PrintStart(object sender, EventArgs e)
        {
            // JJD 2/18/09 - TFS14099
            // Clear the cancel flag and the status description at the start of a print
            this._mustCancel = false;
            if (this._description != null)
                this._description.ClearValue(ContentControl.ContentProperty);
        }

                #endregion //Report_PrintStart

                #region Report_PrintProgress

        void Report_PrintProgress(object sender, PrintProgressEventArgs e)
        {
            //    //this._progressBar.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new SetUI(SetProgresBar), e, null);
            //    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new SetUI(SetProgresBar), e, null);
            
            // stop message loop of main thread to update control progres bar
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new SetUI(SetProgresBar), e, frame);

            Dispatcher.PushFrame(frame);

            if (_mustCancel)
            {
                e.Cancel = true;
            }
        }
                #endregion Report_PrintProgress

                #region Report_PrintEnded

        void Report_PrintEnded(object sender, PrintEndedEventArgs e)
        {
            string description="";
            if (e.Status == PrintStatus.Canceled)
            {
                // JJD 11/24/09 - TFS24840
                // Use preview string if appropriate
                if ( this._report != null && this._report.IsGeneratingPreview )
					description = XamReportPreview.GetString("ProgressCanceledDescription_Preview");
                else
					description = XamReportPreview.GetString("ProgressCanceledDescription");
            }
            else
            {
                // JJD 11/24/09 - TFS24840
                // Use preview string if appropriate
                if ( this._report != null && this._report.IsGeneratingPreview )
					description = string.Format(XamReportPreview.GetString("ProgressCompletedDescription_Preview", e.TotalPrintedPages));
                else
					description = string.Format(XamReportPreview.GetString("ProgressCompletedDescription", e.TotalPrintedPages));
            }

            PrintProgressEventArgs args = new PrintProgressEventArgs(e.TotalPrintedPages, description, 0);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new SetUI(SetProgresBar), args, null);

            this._mustCancel = false;
        }

                #endregion //Report_PrintEnded


        private void SetProgresBar(PrintProgressEventArgs args, object f)
        {
            if (this._progressBar != null)
            {
                this._progressBar.Value = args.PercentCompleted;
            }
            if (this._description != null)
            {
                this._description.Content = args.Description;
            }

            if (f != null)
                ((DispatcherFrame)f).Continue = false;
        }

        // JJD 2/11/09 - TFS10860/TFS13609
        // made static 
        private static void CancelCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ReportProgressControl rpc = sender as ReportProgressControl;

            Debug.Assert(rpc != null);

            if ( rpc != null )
                rpc._mustCancel = true;
        }

            #endregion //Private Methods

        #endregion // Methods

        #region Commands
         /// <summary>
        /// Cancels the print operation.
        /// </summary>
        public static RoutedUICommand CancelCommand = new RoutedUICommand("Cancel", "CancelCommand", typeof(ReportProgressControl));

        #endregion //Commands
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