using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Infragistics.Windows.Helpers;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Infragistics.Shared;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Licensing;
using System.IO;
using System.Windows.Xps.Packaging;
using System.IO.Packaging;
using System.Windows.Threading;
using System.Printing;

namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// Report preview control that contains a standard <see cref="System.Windows.Controls.DocumentViewer"/>.
    /// </summary>
    /// <seealso cref="Report"/>
    /// <seealso cref="DocumentViewer"/>
    /// <seealso cref="GeneratePreview"/>
    [TemplatePart(Name = "Part_Viewer", Type = typeof(DocumentViewer))]
				// JM 10-27-08 TFS9642
			// JM 10-27-08 TFS9642
	public class XamReportPreview : System.Windows.Controls.Control
    {
        #region Member Variables
       
        private FixedDocumentSequence _documentForPreview;
        private DocumentViewer _documentViewer; //DocumentViewer inside XamReportPreview

        // JJD 12/17/08 TFS10903
        // Added stream and package member so we could generate the document by exporting to a memory
        // stream and still clean up when we are done
        private Stream _stream; 
        private Package _package; 
        private Uri _packageRegisteredUri;

        // JJD 2/11/09 - TFS10860/TFS13609
        // cache xpsdocument so we can close it after we are done
        private XpsDocument _xpsDocument;

        // JJD 10/15/08 - added licensing support
        private Infragistics.Windows.Licensing.UltraLicense _license;

        #endregion //Member Variables

        #region Constructors

        static XamReportPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamReportPreview), new FrameworkPropertyMetadata(typeof(XamReportPreview)));
            //TK 10/16/08 TFS9084
            EventManager.RegisterClassHandler(typeof(XamReportPreview), GotFocusEvent, new RoutedEventHandler(XamReportPreview_GotFocus));

            // JJD 12/17/08 TFS10903
            // Register a class handler so we can deal with the Print command
            EventManager.RegisterClassHandler(
                typeof(XamReportPreview),
                CommandManager.PreviewExecutedEvent,
                new ExecutedRoutedEventHandler(OnPreviewDocumentCommand));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="XamReportPreview"/> class
        /// </summary>
        public XamReportPreview()
        {
            // JJD 10/15/08 - added licensing support
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(XamReportPreview), this) as Infragistics.Windows.Licensing.UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }

            // JJD 12/17/08 TFS10903
            // hook the unloaded even so we know when to cean up resources
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
        }

        #endregion //Constructors

        #region Base Class Overrides

            #region OnApplyTemplate
        /// <summary>
        /// Invoked when the template for the element has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // AS 3/12/09
            if (null != _documentViewer)
                this.ClearValue(DocumentTrackerProperty);

            _documentViewer = base.GetTemplateChild("Part_Viewer") as DocumentViewer;

            // TK 10/17/08 TFS8714
            if (_documentViewer != null)
            {
                // JJD 12/17/08 TFS10903
                // bind the viewer's document property so we know whe it's cleared
                this.SetBinding(DocumentTrackerProperty, Utilities.CreateBindingObject(DocumentViewer.DocumentProperty, BindingMode.OneWay, this._documentViewer ));

                
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            }
            Refresh();
        }
            #endregion //OnApplyTemplate

        #endregion //Base Class Overrides

        #region Properties

        #region Public Properties

            #region DocumentViewer
        /// <summary>
        /// Returns a reference to <see cref="System.Windows.Controls.DocumentViewer"/> which is internaly used for preview.
        /// </summary>
        //[Description("Returns reference to DocumentViewer which is internaly used for preview.")]
        //[Category("Reporting Properties")] // Behavior
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DocumentViewer DocumentViewer
        {
            get { return this._documentViewer; }
        }
            #endregion

			// JM 10-09-08 
			#region DocumentViewerStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="System.Windows.Controls.DocumentViewer"/> used within the <see cref="XamReportPreview"/> control.
		/// </summary>
		public static readonly ResourceKey DocumentViewerStyleKey = new StaticPropertyResourceKey(typeof(XamReportPreview), "DocumentViewerStyleKey");

			#endregion //DocumentViewerStyleKey

			#region IsContentVisible

		/// <summary>
        /// Identifies the <see cref="IsContentVisible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsContentVisibleProperty = DependencyProperty.Register("IsContentVisible",
            typeof(bool), typeof(XamReportPreview), 
            new FrameworkPropertyMetadata(KnownBoxes.FalseBox, OnIsContentVisibleChangedCallback, OnCoerceIsContentVisible));

        /// <summary>
        /// Return/sets whether or not the contained DocumentViewer is visible.
        /// </summary>
        /// <seealso cref="IsContentVisibleProperty"/>
        //[Description("Return/sets whether or not the contained DocumentViewer is visible.")]
        //[Category("Reporting Properties")] // Appearance
        [Bindable(true)]
        public bool IsContentVisible
        {
            get
            {
                return (bool)this.GetValue(XamReportPreview.IsContentVisibleProperty);
            }
            set
            {
                this.SetValue(XamReportPreview.IsContentVisibleProperty, value);
            }
        }

        private static void OnIsContentVisibleChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            XamReportPreview control = obj as XamReportPreview;

            bool oldValue = (bool)args.OldValue;
            bool newValue = (bool)args.NewValue;

            if (newValue)
                control._documentViewer.Visibility = Visibility.Visible;
            else
                control._documentViewer.Visibility = Visibility.Hidden;
        }

        private static object OnCoerceIsContentVisible(DependencyObject target, object value)
        {
            XamReportPreview control = target as XamReportPreview;

            if (control != null && control._documentViewer != null)
                return value;

            return 0;
        }

            #endregion //IsContentVisible

            #region Report

        private static readonly DependencyPropertyKey ReportPropertyKey =
            DependencyProperty.RegisterReadOnly("Report",
            typeof(Report), typeof(XamReportPreview), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Report"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ReportProperty =
            ReportPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the Report that is being previewed (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will return null until <see cref="GeneratePreview"/> is called.</para>
        /// </remarks>
        /// <seealso cref="ReportProperty"/>
        /// <seealso cref="Infragistics.Windows.Reporting.Report"/>
        //[Description("Returns the Report that is being previewed (read-only).")]
        //[Category("Reporting Properties")] // Data
        [Bindable(true)]
        [ReadOnly(true)]
        public Report Report
        {
            get
            {
                return (Report)this.GetValue(XamReportPreview.ReportProperty);
            }
        }

            #endregion //Report

        #endregion //Public Properties

        #region Private Properties

            #region DocumentTracker

        // JJD 12/17/08 TFS10903
        // Added private DocumentTracker property so we can tell when the 
        // DocumentViewer's document property has been cleared so we can release
        // any resources
        private static readonly DependencyProperty DocumentTrackerProperty = DependencyProperty.Register("DocumentTracker",
            typeof(IDocumentPaginatorSource), typeof(XamReportPreview), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDocumentTrackerChanged)));

        private static void OnDocumentTrackerChanged(object target, DependencyPropertyChangedEventArgs e )
        {
            XamReportPreview reportPreview = target as XamReportPreview;

            if ( reportPreview != null  && e.NewValue == null )
                reportPreview.ReleaseDocumentResources();
        }

            #endregion //DocumentTracker

        #endregion //Private Properties

        #region Internal Properties
        #endregion //Internal Properties

        #region Protected Properties
        #endregion //Protected Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region GeneratePreview
        /// <summary>
        /// Generates a preview of a specified report and displays in via the contained <see cref="DocumentViewer"/>
        /// </summary>
        /// <param name="report">The report to generate the preview for.</param>
        /// <param name="showPrintDialog">If true will display a standard PrintDialog before the report is generated.</param>
        /// <param name="showReportProgressControl">If true will show a progress window while the report is being generated.</param>
        /// <seealso cref="Report"/>
        /// <seealso cref="ReportProgressControl"/>
        public void GeneratePreview(Report report, bool showPrintDialog, bool showReportProgressControl)
        {
            // JJD 12/17/08 TFS10903
            // Release old document resources
            if (this._documentViewer != null)
                this._documentViewer.ClearValue(DocumentViewer.DocumentProperty);

            // store Report 
            this.SetValue(ReportPropertyKey, report);

            if (BrowserInteropHelper.IsBrowserHosted == false)
            {
                if (report.InitializeReport(showPrintDialog) == false)
                    return;
            }
            else
            {
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                printDialog.UserPageRangeEnabled = report.ReportSettings.UserPageRangeEnabled;

                if (showPrintDialog)
                {
                    if (printDialog.ShowDialog() != true)
                    {
                        return;
                    }

                    report.ReportSettings.PageRange = printDialog.PageRange;

					// MD 6/29/11 - TFS73145
					// Asking for the PrintableAreaWidth could cause a PrintQueueException if there is an error with the print driver,
					// so we should catch that here.
                    //report.ReportSettings.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
					try
					{
						report.ReportSettings.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
					}
					catch (PrintQueueException) { }
                }
                else
                {
                    // JJD 9/1/09 - TFS19395
                    // Don't explicity set the pagesize on the settings object
                    //// check to see if user didnt set range and page size.
                    //if ((report.ReportSettings.PageSize == null) ||
                    //        (report.ReportSettings.PageSize.Width == 0 && report.ReportSettings.PageSize.Height == 0))
                    //{
                    //    report.ReportSettings.PageSize = new Size(793.92, 1122.24);
                    //}

                    if ((report.ReportSettings.PageRange == null) ||
                        (report.ReportSettings.PageRange.PageFrom == 0))
                    {
                        report.ReportSettings.PageRange = new PageRange(1, 0);
                    }
                }
            }

            // JJD 2/11/09 - TFS10860/TFS13609
            // if we are holding old resources release them now
            this.ReleaseDocumentResources();

            // JJD 12/17/08 TFS10903
            // Export an Xps document into a memory stream. Then use it to get a FixedDocumentSequnce to
            // set the DocumentViewer's document property to.
            //this._documentForPreview = report.GetPreviewDocument(showReportProgressControl);
            this._stream = new MemoryStream();

            // JJD 11/24/09 - TFS24840 
            // set the IsGeneratingPreview property
            report.IsGeneratingPreview = true;

            // show the progress control if requested 
            if ( showReportProgressControl )
                report.ShowReportProgressControl(this);

            // export the report as an Xps document into the memory stream
            report.Export(OutputFormat.XPS, this._stream);

            // create and cache a Package off the stream to use to create the XpsDocument below.
            // Note: we use a ZipPackage here since the PackageStore will demand certain rights
            // if it is any other type of package
            this._package = ZipPackage.Open(this._stream, FileMode.Open, FileAccess.Read);

            // Create a unique Uri that we can register the package with using the PackageStore.
            string uriString = "pack://xamreportpreview.document";
            Uri uri = new Uri(uriString + ".xps");
            int suffix = 1;

            // Keep checking with the PackageStore to make sure this uri isn't already registered.
            // if so keep appending a suffix number until we find an unused name
            while (PackageStore.GetPackage(uri) != null)
            {
                suffix++;
                uri = new Uri(uriString + suffix.ToString() + ".xps");
            }

            // cache the uri so we can clean it up latewr
            this._packageRegisteredUri = uri;

            // add the package to the PackageStore which is required since
            // the call to XpsDocument.GetFixedDocumentSequence will fail unless
            // it can make sense out of the uri we pass into the XpsDocument ctor
            PackageStore.AddPackage(this._packageRegisteredUri, this._package);

            // Create an XpsDocument
            // JJD 2/11/09 - TFS10860/TFS13609
            // cache the xpsdocument so we can close it after we are done
            //XpsDocument doc = new XpsDocument(this._package, CompressionOption.Maximum, this._packageRegisteredUri.AbsoluteUri);
            this._xpsDocument = new XpsDocument(this._package, CompressionOption.Maximum, this._packageRegisteredUri.AbsoluteUri);

            // get a FixedDocumentSequence for preview
            //this._documentForPreview = doc.GetFixedDocumentSequence();
            this._documentForPreview = this._xpsDocument.GetFixedDocumentSequence();

            this.Refresh();

            // JJD 11/24/09 - TFS24840 
            // clear the IsGeneratingPreview property
            report.IsGeneratingPreview = false;

            


            report.ReportSettings.PrintQueue = null;
           
        }
                #endregion  //GeneratePreview

            #endregion //Public Methods

            #region Private Methods

                // JJD 12/17/08 TFS10903 - added
                #region OnPreviewDocumentCommand

        private static void OnPreviewDocumentCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Print)
            {
                XamReportPreview preview = sender as XamReportPreview;

                if (preview != null)
                {
                    Report report = preview.Report;

                    if (report != null)
                    {
                        report.Print(true, true, preview);

                        // clear the print queue for the next print job
                        report.ReportSettings.PrintQueue = null;
                    }

                    e.Handled = true;
                }
            }
        }

                #endregion //OnPreviewDocumentCommand	
        
                // JJD 12/17/08 TFS10903 - added
                #region OnUnloaded
        private delegate void MethodDelegate();

        // hook the unloaded even so we know when to cean up resources
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new MethodDelegate(this.VerifyUnloaded));

        }

                #endregion //OnUnloaded	
    
                // JJD 12/17/08 TFS10903 - added
                #region ReleaseDocumentResources

        private void ReleaseDocumentResources()
        {
            if (this._packageRegisteredUri != null)
            {
                if (PackageStore.GetPackage(this._packageRegisteredUri) == this._package)
                    PackageStore.RemovePackage(this._packageRegisteredUri);

                this._packageRegisteredUri = null;
            }

            // JJD 2/11/09 - TFS10860/TFS13609
            // close the cached xpsdocument 
            if (this._xpsDocument != null)
            {
                this._xpsDocument.Close();
                this._xpsDocument = null;
            }

            if (this._package != null)
            {
                this._package.Close();
                this._package = null;
            }

            if (this._stream != null)
            {
                this._stream.Close();
                this._stream = null;
            }

            if (this._documentForPreview != null)
                this._documentForPreview = null;
        }

                #endregion //ReleaseDocumentResources	

                // JJD 12/17/08 TFS10903 - added
                #region VerifyUnloaded

        private void VerifyUnloaded()
        {
            // If we don't have any loaded ancestors then release any document resources
            if (this.IsLoaded == false &&
                Utilities.HasLoadedAncestor(this) == false)
            {
                if ( this._documentViewer != null )
                     this._documentViewer.ClearValue(DocumentViewer.DocumentProperty);

                this.ReleaseDocumentResources();
            }
        }

                #endregion //VerifyUnloaded	
    
                #region XamReportPreview_GotFocus

        private static void XamReportPreview_GotFocus(object sender, RoutedEventArgs e)
        {
            //TK 10/16/08 TFS9084
            XamReportPreview preview = e.Source as XamReportPreview;
            if (preview != null)
            {
                if (preview._documentViewer != null)
                {
                    // translate focus to DocumentViewer
                    preview._documentViewer.Focus();
                }
            }
        }

                #endregion //XamReportPreview_GotFocus
 
            #endregion //Private Methods

            #region Internal Methods

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

                #region Refresh






        internal void Refresh()
        {
            if (this._documentViewer != null)
            {
                this._documentViewer.Document = this._documentForPreview;
                if (DocumentViewer.FitToMaxPagesAcrossCommand.CanExecute(1, this._documentViewer))
                    DocumentViewer.FitToMaxPagesAcrossCommand.Execute(1, this._documentViewer);
            }
        }

                #endregion //Refresh	
    
            #endregion //Internal Methods

            #region Protected Methods
        #endregion //Protected Methods

        #endregion //Methods

        #region Events

        #endregion //Events

        #region Commands

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