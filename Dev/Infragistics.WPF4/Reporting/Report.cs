using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Infragistics.Shared;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Reporting.Events;
using System.Windows.Threading;
using System.Drawing.Printing;

//using Microsoft.Win32;

namespace Infragistics.Windows.Reporting 
{
    /// <summary>
    /// The Report object exposes all properties and methods necessary to print a report or export it to a file in XPS format.
    /// </summary>
    /// <remarks>
    /// <p class="body">To create a report do the following:
    ///     <ul>
    ///         <li>1. Instantiate a Report object.</li>
    ///         <li>2. Specify appropriate settings through the <see cref="ReportSettings"/> property.</li>
    ///         <li>3. Create one or more <see cref="EmbeddedVisualReportSection"/>s and add them to the <see cref="Sections"/> collection.</li>
    ///         <li>4. Call either the <see cref="Print()"/>, <see cref="Export(OutputFormat)"/> or <see cref="XamReportPreview"/>.<see cref="XamReportPreview.GeneratePreview"/> methods.</li>
    ///     </ul>
    /// </p>
    /// <p class="note"><b>Note:</b> styling, templates, header content and footer content for each page's <see cref="ReportPagePresenter"/>s can be specified for the entire report by setting the following properties:
    ///     <ul>
    ///         <li><see cref="ReportBase.PageContentTemplate"/></li>
    ///         <li><see cref="ReportBase.PageContentTemplateSelector"/></li>
    ///         <li><see cref="ReportBase.PageFooter"/></li>
    ///         <li><see cref="ReportBase.PageFooterTemplate"/></li>
    ///         <li><see cref="ReportBase.PageFooterTemplateSelector"/></li>
    ///         <li><see cref="ReportBase.PageHeader"/></li>
    ///         <li><see cref="ReportBase.PageHeaderTemplate"/></li>
    ///         <li><see cref="ReportBase.PageHeaderTemplateSelector"/></li>
    ///         <li><see cref="ReportBase.PagePresenterStyle"/></li>
    ///     </ul>
    ///     The same set of properties is exposed by <see cref="ReportSection"/> to override these defaults on a section by section basis.
    /// </p>
    /// </remarks>
    ///	<seealso cref="EmbeddedVisualReportSection"/>
    /// <seealso cref="Infragistics.Windows.Reporting.ReportBase"/>

    public class Report : ReportBase, 
                        IDocumentPaginatorSource
    {

        #region Private Members
        
        //Window _buildInProgressWnd;
        ToolWindow _buildInProgressWnd;

        private int _countPrintedPages;
        private Paginator _paginator;
        private ReportSectionCollection _sections;
        private int _physicalPageNumber;
        private int _logicalPageNumber;
        private int _logicalPagePartNumber;
        private int _sectionPhysicalPageNumber;
        private int _sectionLogicalPageNumber;
        private int _sectionLogicalPagePartNumber;
        private int _runningLogicalPageCount;
        private FrameworkElement _reportProgressControlOwnedElement;

        private Package _exportPackage;
        private XpsDocument _exportDoc;

        // JJD 9/1/09 - TFS19395
        // Added PageSizeResolved property
        private Size _explicitPageSize = Size.Empty;

        // JJD 11/24/09 - TFS24840 - added
        private bool _isGeneratingPreview;
        
        private ReportSettings _reportSettings;
        private string _defaultExportFileName = "Export.xsp";

		// AS 8/25/11 TFS82921
		private bool _hasCalledBeginPagination;

        #endregion //Private Members	 

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="Report"/> class
        /// </summary>
        public Report()
        {
            this._paginator = new Paginator(this);
            this._sections = new ReportSectionCollection();
            this._paginator.PrintProgress += new EventHandler<PrintProgressEventArgs>(OnPrintProgress);
            this._reportSettings = new ReportSettings();
        }

        #endregion //Constructors

        #region Base class overrides

            // JJD 9/1/09 - TFS19395 - added
            #region PageSizeResolved

        /// <summary>
        /// The actual size of the page used during a report generation or export operation (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this value is only valid during a report generation or export operation. Also it doesn't take into the page orientation.</para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(false)]
        [Browsable(false)]
        public override Size PageSizeResolved
        {
            get 
            {
                if (IsValidPageSize(this._explicitPageSize))
                {
                    // JJD 11/24/09 - TFS25026
                    // Make sure the size returned is not outside the bounds of the printer's
                    // imageable area
                    Size imageableAreaExtent = this.PageImageableAreaExtent;

                    if (IsValidPageSize(imageableAreaExtent))
                    {
						// JJD 8/17/10 - TFS25292
						// If the imageablearea is portrait and the expilicit size is landsccpe (or vice versa)
						// then flip the imageable area extent to conform so we don't restrict the page size
						// improperly below
						if (this._explicitPageSize.Width > this._explicitPageSize.Height &&
							 imageableAreaExtent.Width < imageableAreaExtent.Height)
						{
							double holdWidth = imageableAreaExtent.Width;
							imageableAreaExtent.Width = imageableAreaExtent.Height;
							imageableAreaExtent.Height = holdWidth;
						}

                        return new Size(Math.Min(this._explicitPageSize.Width, imageableAreaExtent.Width),
                                        Math.Min(this._explicitPageSize.Height, imageableAreaExtent.Height));
                    }

                    return _explicitPageSize;
                }
                
                return base.PageSizeResolved;
            }
        }

            #endregion //PageSizeResolved	
        
            #region ReportSettings

        /// <summary>
        /// Gets the object that contains the settings for the entire Report (read-only)
        /// </summary>
        public override ReportSettings ReportSettings { get { return this._reportSettings; } }

                #endregion //ReportSettings

        #endregion //Base class overrides

        #region Properties

            #region Public Properties

                // JJD 11/24/09 - TFS24840 - added
                #region IsGeneratingPreview

        /// <summary>
        /// Returns whether the report is currently generating a preview (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsGeneratingPreview
        {
            get { return this._isGeneratingPreview; }
            internal set 
            {
                if (this._isGeneratingPreview != value)
                {
                    this._isGeneratingPreview = value;
                    this.RaisePropertyChangedEvent("IsGeneratingPreview");
                }
            }
         }

                #endregion //IsGeneratingPreview
    
                #region PhysicalPageNumber
        /// <summary>
        /// The physical page number of the current page being printed within the report (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the physical page number of the current page being printed within the report.</value>
        /// <remarks>
        /// <p class="body">If there are multiple sections within a report this property continues on from the section to section.</p>
        /// <p class="note"><b>Note:</b> if <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="LogicalPageNumber"/>.</p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="SectionLogicalPageNumber"/>
        /// <seealso cref="SectionLogicalPagePartNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The physical page number of the current page being printed within the report (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PhysicalPageNumber
        {
            get
            {
                return _physicalPageNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _physicalPageNumber = value;
                RaisePropertyChangedEvent("PhysicalPageNumber");
            }
        }

                #endregion //PhysicalPageNumber

                #region LogicalPageNumber
        /// <summary>
        /// The logical page part number of the current page being printed (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page part number of the current page being printed.</value>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
        /// </p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="SectionLogicalPageNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The logical page part number of the current page being printed (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LogicalPageNumber
        {
            get
            {
                return _logicalPageNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _logicalPageNumber = value;
                RaisePropertyChangedEvent("LogicalPageNumber");
            }
        }

                #endregion //LogicalPageNumber

                #region LogicalPagePartNumber
        /// <summary>
        /// The logical page part number of the current page being printed (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page part number of the current page being printed.</value>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
        /// </p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="SectionLogicalPageNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The logical page part number of the current page being printed (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LogicalPagePartNumber
        {
            get
            {
                return _logicalPagePartNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _logicalPagePartNumber = value;
                RaisePropertyChangedEvent("LogicalPagePartNumber");
            }
        }

                #endregion //LogicalPagePartNumber

                #region SectionPhysicalPageNumber
        /// <summary>
        /// The physical page number of the current page being printed within this section (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the physical page number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
        /// <p class="note"><b>Note:</b> if <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="LogicalPageNumber"/>.</p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="SectionLogicalPagePartNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The physical page number of the current page being printed within this section (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SectionPhysicalPageNumber
        {
            get
            {
                return _sectionPhysicalPageNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _sectionPhysicalPageNumber = value;
                RaisePropertyChangedEvent("SectionPhysicalPageNumber");
            }
        }

                #endregion //SectionPhysicalPageNumber

                #region SectionLogicalPageNumber
        /// <summary>
        /// The logical page number of the current page being printed within this section (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
        /// <p class="note"><b>Note:</b> if <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="SectionPhysicalPageNumber"/>.</p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="SectionLogicalPagePartNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The logical page number of the current page being printed within this section (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SectionLogicalPageNumber
        {
            get
            {
                return _sectionLogicalPageNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _sectionLogicalPageNumber = value;
                RaisePropertyChangedEvent("SectionLogicalPageNumber");
            }
        }

                #endregion //SectionLogicalPageNumber

                #region SectionLogicalPagePartNumber
        /// <summary>
        /// The logical page part number of the current page being printed (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page part number of the current page being printed.</value>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
        /// </p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="SectionLogicalPageNumber"/>
        /// <seealso cref="SectionPhysicalPageNumber"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="Infragistics.Windows.Reporting.ReportSettings.PagePrintOrder"/>
        //[Description("The logical page part number of the current page being printed (read-only).")]
        //[Category("Data")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SectionLogicalPagePartNumber
        {
            get
            {
                return _sectionLogicalPagePartNumber;
            }
            // JJD 10/14/08 - made setter intenral
            internal set
            {
                _sectionLogicalPagePartNumber = value;
                RaisePropertyChangedEvent("SectionLogicalPagePartNumber");
            }
        }

                #endregion //SectionLogicalPagePartNumber

                #region Sections
        /// <summary>
        /// Returns a collection of <see cref="Infragistics.Windows.Reporting.ReportSection"/> objects.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> each section in a Report starts on a new page. For example, if you created a <b>Report</b> with 3 <see cref="EmbeddedVisualReportSection"/>s, one with a XamDataGrid and 2 others with 
        /// simple visual elements, each section would start on a new page even though there might have been available space on the last page from the previous section.</para>
        /// </remarks>
        /// <seealso cref="ReportSectionCollection"/>
        /// <seealso cref="ReportSection"/>
        /// <seealso cref="EmbeddedVisualReportSection"/>
        //[Description("Returns a collection of ReportSection objects.")]
        //[Category("Data")]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public ReportSectionCollection Sections { get { return this._sections; } } 

                #endregion //Sections

            #endregion  // Public Properties

            #region   Internal Properties

                #region RunningLogicalPageCount
        internal int RunningLogicalPageCount
        {
            get { return _runningLogicalPageCount; }
        }
                #endregion //RunningLogicalPageCount

                #region ReportProgressControlOwnedElementResolved






        internal FrameworkElement ReportProgressControlOwnedElementResolved
        {
            get
            {
                if (_reportProgressControlOwnedElement != null)
                    return _reportProgressControlOwnedElement;

                foreach (ReportSection section in this.Sections)
                {
                    EmbeddedVisualReportSection emvrs = section as EmbeddedVisualReportSection;
                    if (emvrs != null )
                    {
                        // JJD 9/3/09 - TFS17584
                        // Try to get the Page before the Window.
                        // In a browser app if you use the window the progress control will not be
                        // visible
                        FrameworkElement page = Utilities.GetAncestorFromType(emvrs.SourceVisual, typeof(Page), true) as FrameworkElement;
                        if (page != null)
                            return page;

                        FrameworkElement win = Utilities.GetAncestorFromType(emvrs.SourceVisual, typeof(Window), true) as FrameworkElement;
                        if (win != null)
                            return win;

                        //FrameworkElement page = Utilities.GetAncestorFromType(emvrs.SourceVisual, typeof(Page), true) as FrameworkElement;
                        //if (page != null)
                        //    return page;

                        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                    }
                }

                return null;
            }
        }
                #endregion

            #endregion  // Internal Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods

                #region ExportToXps
        internal void ExportToXPS()
        {
            _exportDoc = null;
            _exportPackage = null;

            try
            {
                string exportFile = "";
                if (string.IsNullOrEmpty(ReportSettings.FileName))
                {
                    // set default folder and name
                    exportFile = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + _defaultExportFileName;
                }
                else
                {
                    string path = System.IO.Path.GetDirectoryName(ReportSettings.FileName);
                    string name = System.IO.Path.GetFileName(ReportSettings.FileName);

                    if (string.IsNullOrEmpty(name))
                    {
                        name = _defaultExportFileName;
                    }

                    if (!Directory.Exists(path))
                    {
                        path = Environment.CurrentDirectory;
                    }
                   

                    exportFile = path + System.IO.Path.DirectorySeparatorChar + name;
                }

                CheckUserSettings();
                _exportPackage = Package.Open(exportFile, FileMode.Create);
                _exportDoc = new XpsDocument(_exportPackage);
                GenerateReport(_exportDoc);
            }
            catch (IOException ex)
            {
                OnWritingCanceled(this, new WritingCancelledEventArgs(ex));
            }
        }

        internal void ExportToXPS(string fileName)
        {
			_exportDoc = null;
            _exportPackage = null;

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    // if fileName is not set, then try with report seting
                    ExportToXPS();

                    // JJD 9/24/08 - return
                    return;
                }


                string path = System.IO.Path.GetDirectoryName(fileName);
                string name = System.IO.Path.GetFileName(fileName);
                if (string.IsNullOrEmpty(name))
                {
                    ExportToXPS();
                    return;
                }
                
                if (!Directory.Exists(path))
                {
                    path = Environment.CurrentDirectory;
                }

                path += System.IO.Path.DirectorySeparatorChar + name;

                CheckUserSettings();
                _exportPackage = Package.Open(path, FileMode.Create);
                _exportDoc = new XpsDocument(_exportPackage);
                GenerateReport(_exportDoc);
            }
            catch (IOException ex)
            {
                OnWritingCanceled(this, new WritingCancelledEventArgs(ex));
            }
        }

        internal void ExportToXPS(Stream stream)
        {
			_exportDoc = null;
            _exportPackage = null;

            try
            {
                if (stream == null || stream.CanWrite == false)
                {
                    // if stream is not set, then try with report seting
                    ExportToXPS();
                    return;
                }

                CheckUserSettings();

                _exportPackage = Package.Open(stream, FileMode.Create);
                _exportDoc = new XpsDocument(_exportPackage);
                GenerateReport(_exportDoc);
            }
            catch (IOException ex)
            {
                OnWritingCanceled(this, new WritingCancelledEventArgs(ex));
            }
        }

                #endregion //ExportToXps

                #region GetPreviewDocument
        
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)


                #endregion //GetPreviewDocument

				#region CreateAndInitializePrintDialog

		internal System.Windows.Controls.PrintDialog CreateAndInitializePrintDialog()
		{

			System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
			printDialog.UserPageRangeEnabled = this.ReportSettings.UserPageRangeEnabled;

			// JJD 6/10/11 - TFS77153
			// If the settings has a valid page range then initialize the page range on the print dialog and 
			// set PageRangeSelection to UserPages
			PageRange range = this.ReportSettings.PageRange;
			if (range.PageFrom > 0 && range.PageTo > 0 && range.PageTo >= range.PageFrom)
			{
				printDialog.PageRange = range;
				printDialog.PageRangeSelection = PageRangeSelection.UserPages;
			}

			// JJD 11/10/09 - TFS24546
			// hold the ticket and queue in stack variables
			PrintTicket pt = printDialog.PrintTicket;
			PrintQueue pq = printDialog.PrintQueue;

			// JJD 10/24/08 - TFS9606
			// We need to set the dialog's PrintTicket as the PrintQueue's
			// print ticket to pick up the user preferences
			if (this.ReportSettings.PageOrientation == PageOrientation.Landscape)
			{
				// JJD 11/10/09 - TFS24546
				// Check for null
				//printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
				if (pt != null)
					pt.PageOrientation = System.Printing.PageOrientation.Landscape;

				// JJD 11/10/09 - TFS24546
				// Check for null
				//printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
				if (pq != null)
					pq.DefaultPrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
			}
			else
			{
				// JJD 11/10/09 - TFS24546
				// Check for null
				//printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
				if (pt != null)
					pt.PageOrientation = System.Printing.PageOrientation.Portrait;

				
				
				
				// JJD 11/10/09 - TFS24546
				// Check for null
				//printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
				if (pq != null)
					pq.DefaultPrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
			}

			// JJD 11/10/09 - TFS24546
			// Check for null
			// printDialog.PrintQueue.UserPrintTicket = printDialog.PrintTicket;
			if (pq != null)
				pq.UserPrintTicket = printDialog.PrintTicket;

			return printDialog;
		}

				#endregion //CreateAndInitializePrintDialog	
    
				#region UpdateSettingsFromPrintDialog

		internal void UpdateSettingsFromPrintDialog(System.Windows.Controls.PrintDialog printDialog, bool dialogDisplayed)
		{
			// JJD 11/10/09 - TFS24546
			// hold the ticket and queue in stack variables
			PrintTicket pt = printDialog.PrintTicket;
			PrintQueue pq = printDialog.PrintQueue;

			if (dialogDisplayed)
			{
				// JJD 10/24/08 - TFS9606
				// We need to set the dialog's PrintTicket as the PrintQueue's
				// print ticket to pick up the user preferences
				// JJD 11/10/09 - TFS24546
				// Check for null
				if (pq != null)
				{
					pq.UserPrintTicket = printDialog.PrintTicket;

					this.ReportSettings.PrintQueue = pq;
				}
				this.ReportSettings.PageRange = printDialog.PageRange;

				// JJD 9/1/09 - TFS19395
				// Set the new _pageSizeResolved member instead
				//this.ReportSettings.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
				// MD 6/29/11 - TFS73145
				// Asking for the PrintableAreaWidth could cause a PrintQueueException if there is an error with the print driver,
				// so we should catch that here.
				//this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
				try
				{
					this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
				}
				catch (PrintQueueException) { }
			}
			else
			{
				//PrintQueue pq = LocalPrintServer.GetDefaultPrintQueue();
				//if (pq == null)
				//    throw new Exception(SR.GetString("LE_Report_NoDefaultPrinter"));

				//printDialog.PrintQueue = pq;

				// JJD 12/11/08 - TFS10877
				// We need to set the dialog's PrintTicket as the PrintQueue's
				// print ticket to pick up the user preferences
				// JJD 11/10/09 - TFS24546
				// Check for null
				if (pq != null)
				{
					pq.UserPrintTicket = printDialog.PrintTicket;
					if (this.ReportSettings.PageOrientation == PageOrientation.Landscape)
					{
						pq.UserPrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
					}

					this.ReportSettings.PrintQueue = pq;
				}

				// check to see if user didnt set range and page size.
				// JJD 9/1/09 - TFS19395 
				// Use the IsValidPageSize helper method
				if (!IsValidPageSize(ReportSettings.PageSize))
				{
					// JJD 9/1/09 - TFS19395
					// Set the new _pageSizeResolved member instead
					//ReportSettings.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
					// MD 6/29/11 - TFS73145
					// Asking for the PrintableAreaWidth could cause a PrintQueueException if there is an error with the print driver,
					// so we should catch that here.
					//this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
					try
					{
						this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
					}
					catch (PrintQueueException) { }
				}
				else
				{
					// JJD 05/17/12 - TFS103571]
					// Since the ReportSettings.PageSize was set initialize the _explicitPageSize member from it
					this._explicitPageSize = ReportSettings.PageSize;
				}

				if ((ReportSettings.PageRange == null) ||
					(ReportSettings.PageRange.PageFrom == 0))
				{
					ReportSettings.PageRange = new PageRange(1, 0);
				}
			}

			if (printDialog.PrintTicket.PageOrientation.HasValue)
			{
				if (printDialog.PrintTicket.PageOrientation.Value.Equals(System.Printing.PageOrientation.Portrait))
				{ this.ReportSettings.PageOrientation = PageOrientation.Portrait; }
				else
				{ this.ReportSettings.PageOrientation = PageOrientation.Landscape; }

			}
			if (printDialog.PrintTicket.PageMediaSize != null)
			{
				if (printDialog.PrintTicket.PageMediaSize.Width.HasValue && printDialog.PrintTicket.PageMediaSize.Height.HasValue)
				{
					double printTicketWidth = printDialog.PrintTicket.PageMediaSize.Width.Value;
					double printTicketHeight = printDialog.PrintTicket.PageMediaSize.Height.Value;

					// JJD 10/24/08 - TFS9606
					// If the orientation is landscape we need to exchange the width for the height and vice versa
					if (this.ReportSettings.PageOrientation == PageOrientation.Landscape)
					{
						double holdWidth = printTicketWidth;
						printTicketWidth = printTicketHeight;
						printTicketHeight = holdWidth;
					}

					Size effectivePageSize;

					// JJD 9/1/09 - TFS19395 
					// Use the IsValidPageSize helper method
					if (IsValidPageSize(this._explicitPageSize))
						effectivePageSize = this._explicitPageSize;
					else
						effectivePageSize = this.ReportSettings.PageSize;

					if (effectivePageSize.Width > printTicketWidth || effectivePageSize.Height > printTicketHeight)
					{
						// JJD 10/24/08 - TFS9606
						// Constrain each value individually
						//this.ReportSettings.PageSize = new Size(printTicketWidth, printTicketHeight);
						// JJD 9/1/09 - TFS19395
						// Set the new _pageSizeResolved member instead
						//this.ReportSettings.PageSize = new Size(Math.Min(printTicketWidth, pageSizeOnSettings.Width),
						//                                        Math.Min(printTicketHeight, pageSizeOnSettings.Height));
						this._explicitPageSize = new Size(Math.Min(printTicketWidth, effectivePageSize.Width),
														  Math.Min(printTicketHeight, effectivePageSize.Height));
					}
				}
			}
		}

				#endregion //UpdateSettingsFromPrintDialog	
    
                #region InitializeReport
        internal bool InitializeReport(bool showPrintDialog)
        {
            // check if user is set its own print queue
            if (this.ReportSettings.PrintQueue != null)
            {
                // check and correct if needed PageSize and PageRange
                CheckUserSettings();
                return true;
            }

            System.Windows.Controls.PrintDialog printDialog = this.CreateAndInitializePrintDialog();

            
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


            if (showPrintDialog)
            {
                if (printDialog.ShowDialog() != true)
                {
                    return false;
                }
            }

            this.UpdateSettingsFromPrintDialog(printDialog, showPrintDialog );
             
            
#region Infragistics Source Cleanup (Region)













































































#endregion // Infragistics Source Cleanup (Region)

            return true;
        }
                #endregion

                #region PrintViaPrintDialog
        internal void PrintViaPrintDialog(bool showReportProgressControl)
        {
            // JJD 9/1/09 - TFS19395
            // Clear the _pageSizeResolved member 
            this._explicitPageSize = Size.Empty;

            // the PrintDialog has to be shown before printing. Otherwise the framework throws an exception
            // when we use it for printing.
            ReportSettings reportSettings = this.ReportSettings;
            System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
            printDialog.UserPageRangeEnabled = reportSettings.UserPageRangeEnabled;

			// JJD 8/24/11 - TFS81247
			// Initialize the orientation on the print ticket
			PrintTicket ticket = printDialog.PrintTicket;
			if ( ticket != null )
			{
				ticket.PageOrientation = 
					reportSettings.PageOrientation == PageOrientation.Landscape 
						? System.Printing.PageOrientation.Landscape 
						: System.Printing.PageOrientation.Portrait;
			}
            
            if (printDialog.ShowDialog() != true)
                return;

            reportSettings.PageRange = printDialog.PageRange;
			
			// JJD 8/24/11 - TFS81247
			// See if there was a change to the tickets orientation and update the settings
			if (ticket != null && ticket.PageOrientation.HasValue)
			{
				switch (ticket.PageOrientation.Value)
				{
					case System.Printing.PageOrientation.Landscape:
					case System.Printing.PageOrientation.ReverseLandscape:
						reportSettings.PageOrientation = PageOrientation.Landscape;
						break;
					default:
						reportSettings.PageOrientation = PageOrientation.Portrait;
						break;
				}
			}

			// JJD 3/25/11 - TFS70336 
			// Clear the cached origin and size before staring the report
			this.ClearCachedOriginAndSize();

            // JJD 9/1/09 - TFS19395
            // Set the new _pageSizeResolved member instead
            //this.ReportSettings.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
			// MD 6/29/11 - TFS73145
			// Asking for the PrintableAreaWidth could cause a PrintQueueException if there is an error with the print driver,
			// so we should catch that here.
            //this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
			try
			{
				this._explicitPageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
			}
			catch (PrintQueueException) { }

            if (showReportProgressControl)
                ShowReportProgressControl();

            this._paginator.Initialize(reportSettings);
            printDialog.PrintDocument(this._paginator, "");

            if (this._paginator.IsCancel == false)
            {
                // notify the printing is completed
                OnWritingCompleted(this, new WritingCompletedEventArgs(false, "", null));
            }
        }

                #endregion

            #endregion //Internal Methods

            #region Private Methods
       
#region Infragistics Source Cleanup (Region)










































#endregion // Infragistics Source Cleanup (Region)

                #region CheckUserSettings
        private void CheckUserSettings()
        {
            // JJD 9/1/09 - TFS19395
            // We shouldn't be setting the PageSize on the report settings.
            // Added PageSizeResolved property wil return the page sixed used
            // during a report or export run
            //if ((ReportSettings.PageSize == null) ||
            //    (ReportSettings.PageSize.Width == 0 && ReportSettings.PageSize.Height == 0))
            //{
            //    ReportSettings.PageSize = new Size(793.92, 1122.24);
            //}

            if ((ReportSettings.PageRange == null) ||
                (ReportSettings.PageRange.PageFrom == 0))
            {
                // there is a defference in page range setting, when we show dialog and when the dialog is not shown
                // In first case the range is (1;0), in second the range is (0,0). We need equals value so we correct the value
                ReportSettings.PageRange = new PageRange(1, 0);
            }
        }
                #endregion //CheckUserSettings

                // JJD 2/11/09 - TFS10860/TFS13609 - added
                #region CleanUpDocAndPackage

        // Moved to CleanUpDocAndPackage method
        void CleanUpDocAndPackage()
        {
            if (this._exportDoc != null)
            {
                this._exportDoc.Close();
                this._exportDoc = null;
            }

            if (this._exportPackage != null)
            {
                this._exportPackage.Close();
                this._exportPackage = null;
            }
        }

                #endregion //CleanUpDocAndPackage	
    
                #region CloseReportProgressControl
        private void CloseReportProgressControl()
        {
            if (this._buildInProgressWnd != null)
            {
                //this._buildInProgressWnd.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                this._buildInProgressWnd.Close();
                this._buildInProgressWnd = null;
            }
        }
                #endregion //CloseReportProgressControl

                #region CreateFixedPageContent
        
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

                #endregion //CreateFixedPageContent

                #region GenerateReport
        /// <summary>
        /// Generate report / Print or Xps file
        /// </summary>
        /// <param name="reportObject"></param>
        private void GenerateReport(Object reportObject)
        {

			// JJD 8/13/10 - TFS25292
			// Don't clear the _explicitPageSize here because in the case of a preview operation
			// it could have been set for landscape mode
            
            
            //this._explicitPageSize = Size.Empty;

            if (reportObject is XpsDocument)
            {

                XpsDocument xpsDocument = reportObject as XpsDocument;
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                PrintTicket printTicket = null;
				PrintQueue printQueue = null;

                // JJD 9/1/09 - TFS19395
                // Get the print ticket from the print queue and pass it into ProcessXpsDocumentWriter
                try
                {
					// JJD 4/21/11 - TFS73145 - added printQueue parameter to ProcessXpsDocumentWriter below
					//if (this.ReportSettings.PrintQueue != null)
					//    printTicket = this.ReportSettings.PrintQueue.DefaultPrintTicket;
					printQueue = this.ReportSettings.PrintQueue;
					if (printQueue != null)
						printTicket = this.ReportSettings.PrintQueue.DefaultPrintTicket;
				}
                catch (Exception)
                {
                }

				// JJD 4/21/11 - TFS73145 - added printQueue parameter
                //ProcessXpsDocumentWriter(writer, printTicket);
				// JJD 6/10/11 - TFS73145 
				// Wrap call to ProcessXpsDocumentWriter to catch a PrintQueueException which
				// can be thrown by some buggy print drivers 
				try
				{
					this.ProcessXpsDocumentWriter(writer, printTicket, printQueue);
				}
				catch (PrintQueueException)
				{
					// AS 8/25/11 TFS82921
					this.InvokeEndPaginationIfNeeded();

					// JJD 6/10/11 - TFS73145 
					// Re-try with a new PrintTicket which has been reported to solve
					// this issue with some print drivers
					writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
					this.ProcessXpsDocumentWriter(writer, new PrintTicket(), printQueue);
				}
            }
            else if (reportObject is PrintQueue)
            {
                // JJD 3/24/09 - TFS15887
                // We shouldn't be demanding full trust. The XpsDocumentWriter will take care
                // of demanding any rights it requires
                //new NamedPermissionSet("FullTrust").Demand();
                PrintQueue printQueue = reportObject as PrintQueue;
                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
				
				// JJD 4/21/11 - TFS73145 - added printQueue parameter
                //ProcessXpsDocumentWriter(writer);
				// JJD 6/10/11 - TFS73145 
				// Wrap call to ProcessXpsDocumentWriter to catch a PrintQueueException which
				// can be thrown by some buggy print drivers 
				try
				{
					this.ProcessXpsDocumentWriter(writer, printQueue);
				}
				catch (PrintQueueException)
				{
					// AS 8/25/11 TFS82921
					this.InvokeEndPaginationIfNeeded();

					// JJD 6/10/11 - TFS73145 
					// Re-try with a new PrintTicket which has been reported to solve
					// this issue with some print drivers
					writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
					this.ProcessXpsDocumentWriter(writer, new PrintTicket(), printQueue);
				}
            }
        }
        #endregion

				// AS 8/25/11 TFS82921
				#region InvokeEndPaginationIfNeeded
		private void InvokeEndPaginationIfNeeded()
		{
			if (_hasCalledBeginPagination)
			{
				_hasCalledBeginPagination = false;

				Debug.Assert(_paginator != null);

				//notify paginator for end of printing
				this._paginator.EndPagination();
			}
		}
				#endregion //InvokeEndPaginationIfNeeded

                #region IsPermissionGranted
        /// <summary>
        /// IsPermissionGranted - Detect whether or not this application has the requested permission
        /// </summary>
        /// <param name="requestedPermission"></param>
        /// <returns></returns>
        bool IsPermissionGranted(CodeAccessPermission requestedPermission)
        {
            try
            {
                // Try and get this permission
                requestedPermission.Demand();
                return true;
            }
            catch
            {
                return false;
            }
        } 
                #endregion

                // JJD 9/1/09 - TFS19395 - added
                #region IsValidPageSize

        private static bool IsValidPageSize(Size size)
        {
            return size.IsEmpty == false &&
                    size.Height >= 1 &&
                    size.Width >= 1;
        }

                #endregion //IsValidPageSize	
    
                #region ShowReportProgressControl

        // JJD 12/17/08 TFS10903 - added o verload to support
        // showing the preview control
        internal void ShowReportProgressControl(XamReportPreview preview)
        {
            // JJD 9/3/09 - TFS17584
            // Always use the prview control instead trying to find a Window or page
            // In a browser app if you use the window the progress control will not be
            // visible
            //this._reportProgressControlOwnedElement = Utilities.GetAncestorFromType(preview, typeof(Window), true) as FrameworkElement;

            //if (this._reportProgressControlOwnedElement == null)
            //{
            //    this._reportProgressControlOwnedElement = Utilities.GetAncestorFromType(preview, typeof(Page), true) as FrameworkElement;
            //    if (this._reportProgressControlOwnedElement == null)
            //        this._reportProgressControlOwnedElement = preview;
            //}
            this._reportProgressControlOwnedElement = preview;


            try
            {
                this.ShowReportProgressControl();
            }
            finally
            {
                this._reportProgressControlOwnedElement = null;
            }
        }

        private void ShowReportProgressControl()
        {
            Debug.Assert(this._buildInProgressWnd == null, "Progress window is not equal to null!");

            if (this._buildInProgressWnd == null)
            {
                FrameworkElement ownedEl = ReportProgressControlOwnedElementResolved;
                if (ownedEl == null)
                {
                    return;
                }

                ReportProgressControl prCtrl = new ReportProgressControl();
                prCtrl.Report = this;

                this._buildInProgressWnd = new ToolWindow();
                this._buildInProgressWnd.VerticalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
                this._buildInProgressWnd.VerticalAlignment = VerticalAlignment.Center;
                this._buildInProgressWnd.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
                this._buildInProgressWnd.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                this._buildInProgressWnd.Content = prCtrl;
                // AS 10/9/08
                // Changed to MinWidth/Height in case the content is larger.
                //
                //this._buildInProgressWnd.Height = 200;
                //this._buildInProgressWnd.Width = 300;
                this._buildInProgressWnd.MinHeight = 200;
                this._buildInProgressWnd.MinWidth = 300;

                // JJD 11/24/09 - TFS24840
                // Use preview string if appropriate
                if ( this.IsGeneratingPreview )
					this._buildInProgressWnd.Title = XamReportPreview.GetString("ReportProgressControlTitle_Preview");
                else
					this._buildInProgressWnd.Title = XamReportPreview.GetString("ReportProgressControlTitle");

                this._buildInProgressWnd.Show(ownedEl);//System.Windows.Application.Current.MainWindow);
            }
        }
                #endregion //ShowReportProgressControl

                #region ProcessXpsWriter
        /// <summary>
        /// ProcessXpsDocumentWriter(XpsDocumentWriter)
        /// </summary>
        /// <param name="writer"></param>
		/// <param name="printQueue"></param>
		// JJD 4/21/11 - TFS73145 - added printQueue parameter
		//private void ProcessXpsDocumentWriter(XpsDocumentWriter writer)
		private void ProcessXpsDocumentWriter(XpsDocumentWriter writer, PrintQueue printQueue)
		{
			ProcessXpsDocumentWriter(writer, null, printQueue);
		}

        /// <summary>
        /// ProcessXpsDocumentWriter(XpsDocumentWriter , PrintTicket)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="printTicket"></param>
		/// <param name="printQueue"></param>
		// JJD 4/21/11 - TFS73145 - added printQueue parameter
		//private void ProcessXpsDocumentWriter(XpsDocumentWriter writer, PrintTicket printTicket)
		private void ProcessXpsDocumentWriter(XpsDocumentWriter writer, PrintTicket printTicket, PrintQueue printQueue)
		{
			// JJD 3/25/11 - TFS70336 
			// Clear the cached origin and size before staring the report
			this.ClearCachedOriginAndSize();

			this._paginator.Initialize(this.ReportSettings);

			
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


			
			if (printTicket == null)
			{
				
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


				if (printQueue != null)
				{
					printTicket = printQueue.UserPrintTicket;

					if (printTicket == null)
						printTicket = printQueue.DefaultPrintTicket;
				}

				// JJD 4/21/11 - TFS73145
				// Create a print ticket to avoid a problem with certian print drivers
				if (printTicket == null)
					printTicket = new PrintTicket();
			}

            
            
            writer.Write(this._paginator, printTicket);
			

            // JJD 9/24/08
            // Moved logic to from OnWritingCancelled to prevent exception 
            // generated within writer.Write above
            if (this._paginator.IsCancel)
            {
                // JJD 2/11/09 - TFS10860/TFS13609
                // Moved to CleanUpDocAndPackage method
                //if (this._exportPackage != null)
                //{
                //    this._exportPackage.Close();
                //    this._exportPackage = null;
                //}
                this.CleanUpDocAndPackage();
            }
            else
            {
                // notify the printing is completed
                OnWritingCompleted(this, new WritingCompletedEventArgs(false, "State", null));
            }
        }

                #endregion //ProcessXpsWriter

                #region RaisePrintStartEvent
        /// <summary>
        /// Takes care for raise event in correct thread
        /// </summary>
        private void RaisePrintStartEvent(EventArgs e)
        {
            if (PrintStart != null)
            {
                PrintStart(this, e);
            }
        }
                #endregion

                #region RaisePrintProgressEvent
        /// <summary>
        /// Takes care for raise event in correct thread
        /// </summary>
        private void RaisePrintProgressEvent(PrintProgressEventArgs e)
        {
            if (PrintProgress != null)
            {
                PrintProgress(this, e);
            }
 
        }
                #endregion

                #region RaisePrintEndedEvent
        /// <summary>
        /// Takes care for raise event in correct thread
        /// </summary>
        private void RaisePrintEndedEvent(PrintEndedEventArgs e)
        {
            if (PrintEnded != null)
            {
                PrintEnded(this, e);
            }
        }
                #endregion

				// JJD 8/25/11 - TFS81022 - added
				#region VerifyAtLeastOneSection

		private void VerifyAtLeastOneSection()
		{
			ReportSectionCollection sections = this.Sections;

			if (sections == null || sections.Count == 0)
				throw new InvalidOperationException(SR.GetString("LE_Report_NoSections"));
		}

				#endregion //VerifyAtLeastOneSection	
    
            #endregion //Private Methods

            #region Protected Methods

                #region OnPrintProgress
        /// <summary>
        /// Called when the new page is printed.
        /// </summary>
        protected void OnPrintProgress(object sender, PrintProgressEventArgs e)
        {
            _countPrintedPages++;
            // rise the event
            RaisePrintProgressEvent(e);
            // check for canceling
            if (e != null && e.Cancel)
            {
                this._paginator.IsCancel = true;
                OnWritingCanceled(this, new WritingCancelledEventArgs(null));
            }
        }
                #endregion //OnPrintProgress

                #region OnWritingCompleted
        /// <summary>
        /// Called when the printing is completed.
        /// </summary>
        protected void OnWritingCompleted(object sender, WritingCompletedEventArgs e)
        {
            // JJD 2/11/09 - TFS10860/TFS13609
            // Moved to CleanUpDocAndPackage method
            //if (this._exportPackage != null)
            //{
            //    this._exportPackage.Close();
            //    this._exportPackage = null;
            //}
            this.CleanUpDocAndPackage();

			// AS 8/25/11 TFS82921
			////notify paginator for end of printing
			//this._paginator.EndPagination();
			this.InvokeEndPaginationIfNeeded();

            PrintStatus status;
            if (e.Cancelled)
                status = PrintStatus.Canceled;
            else if (e.Error == null)
                status = PrintStatus.Successful;
            else status = PrintStatus.Canceled;

            PrintEndedEventArgs args = new PrintEndedEventArgs(status, _countPrintedPages);
            // rise print ended event
            RaisePrintEndedEvent(args);
            CloseReportProgressControl();
        }

                #endregion OnWritingCompleted

                #region OnWritingCanceled
        /// <summary>
        /// Called when the printing is canceled.
        /// </summary>
        protected void OnWritingCanceled(object sender, WritingCancelledEventArgs e)
        {
            PrintEndedEventArgs args = new PrintEndedEventArgs(PrintStatus.Canceled, _countPrintedPages);
            RaisePrintEndedEvent(args);

			// AS 8/25/11 TFS82921
			//// JJD 9/24/08 
			////notify paginator for end of printing
			//this._paginator.EndPagination();
			this.InvokeEndPaginationIfNeeded();

            CloseReportProgressControl();
            
            // JJD 9/24/08 
            //Moved logic to ProcessXpsDocumentWriter
            //if (this._exportPackage != null)
            //{
            //    this._exportPackage.Close();
            //    this._exportPackage = null;
            //}
        }
                #endregion OnWritingCanceled

            #endregion //Protected Methods

            #region Public Methods

                #region Export
        /// <summary>
        /// Exports the Report to a file in a specified output format. The <see cref="Infragistics.Windows.Reporting.ReportSettings.FileName"/> determinates the exported file name.
        /// </summary>
        /// <param name="outputFormat">The output document type (XPS only). </param>
        /// <remarks>
        /// <p class="note">If the <see cref="ReportSettings"/>.<see cref="Infragistics.Windows.Reporting.ReportSettings.FileName"/> property is not set, 
        /// then the Report will be exported in <see cref="System.Environment.CurrentDirectory"/> with default file name: <b>Export.xps</b>. 
        /// If the <see cref="Infragistics.Windows.Reporting.ReportSettings.FileName"/> property contains only a folder name, e.g. 'C:\MyFolder\', 
        /// the Report will be exported in that folder with a filename of <b>Export.xps</b>.  If the property only contains a file name,
        /// e.g. "xamGridExport.xps", the Report will be exported in the CurrentDirectory with the specified name. 
        /// </p>
        /// </remarks>
        /// <exception cref="NotSupportedException">The exception will be thrown if you try to run export in non FullTrust environment.</exception>
		/// <exception cref="InvalidOperationException">The exception will be thrown if you try to export or print a Report that doesn't have any sections.</exception>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public void Export(OutputFormat outputFormat)
        {
			// JJD 8/25/11 - TFS81022
			// Call VerifyAtLeastOneSection which will throw an exception if there are no sections specified
			this.VerifyAtLeastOneSection();
			
			// JJD 3/24/09 - TFS15887
            // We shouldn't be throwing an exception in XBAP since the app may have
            // the appropriate conditions
            //if (BrowserInteropHelper.IsBrowserHosted)
            //{
            //    throw new NotSupportedException(SR.GetString("LE_Export_NotSupportedXBAP"));
            //}

            // rise print start event
            RaisePrintStartEvent(new EventArgs());

            switch (outputFormat)
            {
                case OutputFormat.XPS:
                    ExportToXPS();
                    break;
            }
        }

        /// <summary>
        /// Exports the Report to a file in the specified file format using the specified file name.
        /// </summary>
        /// <param name="outputFormat">The output document type (XPS only).</param>
        /// <param name="fileName">The file name where the report will be exported. If the file exists, it will be overwritten.</param>
        /// <remarks>
        /// <p class="note">If the fileName is null or an empty string then the Report will be exported in <see cref="System.Environment.CurrentDirectory"/> with default file name: <b>Export.xps</b>. 
        /// If the fileNam contains only a folder name, e.g. 'C:\MyFolder\', 
        /// the Report will be exported in that folder with a filename of <b>Export.xps</b>.  If the parameter only contains a file name,
        /// e.g. "xamGridExport.xps", the Report will be exported in the CurrentDirectory with the specified name. 
        /// </p>
        /// </remarks>
        /// <exception cref="NotSupportedException">The exception will be thrown if you try to run export in non FullTrust environment.</exception>
        /// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public void Export(OutputFormat outputFormat, string fileName)
        {
            Export(outputFormat, fileName, false);
        }

        /// <summary>
        /// Exports the Report to a file in the specified file format using the specified file name.
        /// </summary>
        /// <param name="outputFormat">The output document type (XPS only).</param>
        /// <param name="fileName">The file name where the report will be exported. If the file exists, it will be overwritten.</param>
        /// <param name="showSaveFileDialog">If true the SaveFileDialog will be shown.</param>
        /// <remarks>
        /// <p class="note">if showSaveFileDialog is true then the filename parameter will be used as the default file name 
        /// in the SaveFileDialog. Otherwise, if the fileName is null or an empty string then the Report will be exported in <see cref="System.Environment.CurrentDirectory"/> with default file name: <b>Export.xps</b>. 
        /// If the fileNam contains only a folder name, e.g. 'C:\MyFolder\', 
        /// the Report will be exported in that folder with a filename of <b>Export.xps</b>.  If the parameter only contains a file name,
        /// e.g. "xamGridExport.xps", the Report will be exported in the CurrentDirectory with the specified name. 
        /// </p>
        /// </remarks>
        /// <exception cref="ArgumentException">The exception can be thrown if the file name is invalid or the directory doesn't exists</exception>
        /// <exception cref="NotSupportedException">The exception will be thrown if you try to run export in non FullTrust environment.</exception>
		/// <exception cref="InvalidOperationException">The exception will be thrown if you try to export or print a Report that doesn't have any sections.</exception>
		/// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public void Export(OutputFormat outputFormat, string fileName, bool showSaveFileDialog)
        {
			// JJD 8/25/11 - TFS81022
			// Call VerifyAtLeastOneSection which will throw an exception if there are no sections specified
			this.VerifyAtLeastOneSection();

			// JJD 3/24/09 - TFS15887
            // We shouldn't be throwing an exception in XBAP since the app may have
            // the appropriate conditions
            //if (BrowserInteropHelper.IsBrowserHosted)
            //{
            //    throw new NotSupportedException(SR.GetString("LE_Export_NotSupportedXBAP"));
            //}

            string exportFile = "";
            if (showSaveFileDialog)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = fileName;
                dlg.Filter = "xps files (*.xps)|*.xps|All files (*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                exportFile = dlg.FileName;
            }
            else
            {
                exportFile = fileName;
            }
            // rise print start event
            RaisePrintStartEvent(new EventArgs());
             
            switch (outputFormat)
            {
                case OutputFormat.XPS:
                    ExportToXPS(exportFile);
                    break;
            }
        }
        
        /// <summary>
        /// Exports the Report in a specified file format in a specified stream.
        /// </summary>
        /// <param name="outputFormat">The output document type (XPS only).</param>
        /// <param name="stream">The stream to which the Report will be exported.</param>
        /// <remarks>
        /// <p class="body">If you stream is null then the result will be the same as calling <see cref="Export(OutputFormat)"/>.</p>
        /// </remarks>
		/// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        /// <exception cref="NotSupportedException">The exception will be thrown if you try to run export in a non FullTrust environment.</exception>
		/// <exception cref="InvalidOperationException">The exception will be thrown if you try to export or print a Report that doesn't have any sections.</exception>
        public void Export(OutputFormat outputFormat, Stream stream)
        {
			// JJD 8/25/11 - TFS81022
			// Call VerifyAtLeastOneSection which will throw an exception if there are no sections specified
			this.VerifyAtLeastOneSection();
			
			// JJD 3/24/09 - TFS15887
            // We shouldn't be throwing an exception in XBAP since the app may have
            // the appropriate conditions
            //if (BrowserInteropHelper.IsBrowserHosted)
            //{
            //    throw new NotSupportedException(SR.GetString("LE_Export_NotSupportedXBAP"));
            //}

            // rise print start event
            RaisePrintStartEvent(new EventArgs());

            switch (outputFormat)
            {
                case OutputFormat.XPS:
                    ExportToXPS(stream);
                    break;
            }
        }
                #endregion

                #region Print
        /// <summary>
        /// Prints the report first showing a standard print dialog but without showing a progress dialog.
        /// </summary>
        /// <seealso cref="ReportProgressControl"/>
        /// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print(bool, bool)"/>
        /// <seealso cref="Print(bool, bool, FrameworkElement)"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public void Print()
        {
            Print(true, false);
        }

        /// <summary>
        /// Prints the report and can show an optional progress window as well as a standard print dialog. 
        /// </summary>
        /// <param name="showReportProgressControl">If true a progress window is displayed during the print operation.</param>
        /// <param name="showPrintDialog">If true a standard print dialog is firat displayed to the user before printing begins.</param>
        /// <seealso cref="ReportProgressControl"/>
        /// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="Print(bool, bool, FrameworkElement)"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public void Print(bool showPrintDialog, bool showReportProgressControl)
        {
            Print(showPrintDialog, showReportProgressControl, null);
        }

        /// <summary>
		/// Prints the report and can show an optional progress window (with a specified owning window) as well as a standard print dialog.
		/// </summary>
        /// <param name="showReportProgressControl">If true a progress window is displayed during the print operation.</param>
        /// <param name="showPrintDialog">If true a standard print dialog is firat displayed to the user before printing begins.</param>
        /// <param name="reportProgressControlOwnedElement">The owning element. This element will be used to set the owner of the progress window displayed during the print operation.</param>
        /// <seealso cref="ReportProgressControl"/>
        /// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Export(OutputFormat, Stream)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="Print(bool, bool, FrameworkElement)"/>
        /// <seealso cref="ShowPrintDialog"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
		/// <exception cref="InvalidOperationException">The exception will be thrown if you try to export or print a Report that doesn't have any sections.</exception>
		public void Print(bool showPrintDialog, bool showReportProgressControl, FrameworkElement reportProgressControlOwnedElement)
        {
			// JJD 8/25/11 - TFS81022
			// Call VerifyAtLeastOneSection which will throw an exception if there are no sections specified
			this.VerifyAtLeastOneSection();

			// JJD 9/1/09 - TFS19395
            // Clear the _pageSizeResolved member 
            this._explicitPageSize = Size.Empty;

			// JJD 3/25/11 - TFS70336 
			// Clear the cached origin and size before staring the report
			this.ClearCachedOriginAndSize();

            this._reportProgressControlOwnedElement = reportProgressControlOwnedElement;
            _countPrintedPages = 0;
            // raise print start event
            RaisePrintStartEvent(new EventArgs());

			//System.Windows.Controls.PrintDialog pd = new System.Windows.Controls.PrintDialog();

			// JJD 11/10/09 - TFS24546
			// If there is no default printQueue then we are on a system that doesn't have any 
			// printers defined. Therefore we need raise the end event and return
			// JJD 11/15/11 - TFS95785
			// Instead of checking for a null PrintQueue (which can happen when impersonating another user)
			// check the count of installed printers
			//if (pd.PrintQueue == null)
			if (PrinterSettings.InstalledPrinters.Count == 0)
			{
				this._reportProgressControlOwnedElement = null;

				RaisePrintEndedEvent(new PrintEndedEventArgs(PrintStatus.Canceled, 0, PrintCancelationReason.NoPrinterAvailable));
				return;
			}

            if (BrowserInteropHelper.IsBrowserHosted == false)
            {
                if (InitializeReport(showPrintDialog) == false)
                    return;

                if (showReportProgressControl)
                    ShowReportProgressControl();

				// MD 6/28/11 - TFS73693
				// Generating the report may throw an exception. If it does, we should close the progress control before and try blocks handle 
				// the exception. Otherwise, it will stay open regardless of whether the exception is handled or not.
                //GenerateReport(((ReportSettings)this.ReportSettings).PrintQueue);
				try
				{
					GenerateReport(((ReportSettings)this.ReportSettings).PrintQueue);
				}
				catch
				{
					if (showReportProgressControl)
						CloseReportProgressControl();

					throw;
				}
            }
            else
            {
                //there is not XPS support, so print document by standart tools
                PrintViaPrintDialog(showReportProgressControl);
            }

            // JJD 12/17/08  clear the cached element reference
            this._reportProgressControlOwnedElement = null;
        }
                #endregion

                #region ShowPrintDialog
        /// <summary>
        /// Shows a standard PrintDialog and saves settings from it in the <see cref="ReportSettings"/> object.
        /// </summary>
        /// <returns>False if the dialog was cancelled, otherwise true.</returns>
        /// <remarks>
        /// <p class="body">You can use this method before calling any of the Print methods. This allows you to edit the user's settings before the report starts printing.
        /// If you use this method in non trusted environment (XBAP) you must initialize the <see cref="ReportSettings"/>.<see cref="Infragistics.Windows.Reporting.ReportSettings.PrintQueue"/>
        /// before calling one of Print methods.</p>
        /// </remarks>
        /// <seealso cref="Export(OutputFormat)"/>
        /// <seealso cref="Export(OutputFormat, string)"/>
        /// <seealso cref="Export(OutputFormat, string, bool)"/>
        /// <seealso cref="Print()"/>
        /// <seealso cref="Print(bool, bool, FrameworkElement)"/>
        /// <seealso cref="XamReportPreview.GeneratePreview"/>
        public bool ShowPrintDialog()
        {
            
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

            System.Windows.Controls.PrintDialog printDialog = this.CreateAndInitializePrintDialog();

            if (printDialog.ShowDialog() == false)
                return false;

            this.UpdateSettingsFromPrintDialog(printDialog, true);
            
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

            return true;

        }
        #endregion

            #endregion //Public Methods

        #endregion //Methods

        #region Events

        /// <summary>
        /// Occurs as each page is printed or exported.
        /// </summary>
        /// <seealso cref="PrintProgressEventArgs"/>
        //[Description("Occurs as each page is printed or exported..")]
        //[Category("Behavior")]
        public event EventHandler<PrintProgressEventArgs> PrintProgress;

        /// <summary>
        /// Occurs when a print or export operation is about to begin.
        /// </summary>
        //[Description("Occurs when a printing or exporting operation is about to begin.")]
        //[Category("Behavior")]
        public event EventHandler PrintStart;

        /// <summary>
        /// Occurs when a print or export operation ends.
        /// </summary>
        /// <seealso cref="PrintEndedEventArgs"/>
        //[Description("Occurs when a print or export operation ends.")]
        //[Category("Behavior")]
        public event EventHandler<PrintEndedEventArgs> PrintEnded;

        #endregion

        #region IDocumentPaginatorSource Members

        DocumentPaginator IDocumentPaginatorSource.DocumentPaginator
        {
            get { return this._paginator; }
        }

        #endregion

        #region Paginator private class
        /// <summary>
        /// private class Paginator
        /// </summary>
        private class Paginator : DocumentPaginator
        {
            #region Private Members
            private Visual _pageVisual;
            private List<int> _maxLogicalNumbers;
            private int _printPageCount;
            private PageRange _pageRange;
            private Size _pageSize;
            private Report _report;
            private bool _isPageCountValid;
            private bool _isPreviewMode;
            private bool _isCancel;
            #endregion //Private Members

            #region Constructors

            internal Paginator(Report report)
            {
                this._isPreviewMode = false;
                this._isPageCountValid = false;
                this._report = report;
                this._isCancel = false;
            }

            #endregion //Constructors
             
            #region Base class overrides

                #region GetPage

            /// <summary>
            /// Get the specified page for whole report 
            /// </summary>
            /// <param name="pageNumber"></param>
            /// <returns>Ready for print document page</returns>
            public override DocumentPage GetPage(int pageNumber)
            {
                int totalPages = 0;
                DocumentPage currPage = null;
                _report._runningLogicalPageCount = 0;

                if (this._report.Sections.Count == 0) { return null; }
                this._report.PhysicalPageNumber = pageNumber + this.PageRange.PageFrom; 

                for (int ind = 0; ind < this._report.Sections.Count; ind++)
                {
                    ReportSection reportSection = this._report.Sections[ind];
                    if (reportSection != null)
                    {
                        // request page from section
                        currPage = reportSection.GetPage(pageNumber + _pageRange.PageFrom - totalPages - 1);
                        if (currPage == null)
                        {
                            // the page is in another section
                            totalPages += reportSection.GeneratedPageCount;
                            _report._runningLogicalPageCount += (int)this._maxLogicalNumbers[ind];
                        }
                        else
                        {
                            // check for section end
                            if ((ind == this._report.Sections.Count - 1) && (reportSection.IsEndReached))
                            {
                                // check for end of report, this fix blank page in the end of report
                                _isPageCountValid = true;
                            }

                            // check for page range end
                            if ((pageNumber + this._pageRange.PageFrom >= this._pageRange.PageTo) &&
                                (this._pageRange.PageTo != 0))
                            {
                                _isPageCountValid = true;
                            }

                            // estimate right logical number
                            this._maxLogicalNumbers[ind] = Math.Max((int)this._maxLogicalNumbers[ind], (int)reportSection.LogicalPageNumber);

                            // JJD 10/14/08
                            // Update the report's page numbers 
                            this._report.LogicalPageNumber              = reportSection.LogicalPageNumber + this._report.RunningLogicalPageCount;
                            this._report.LogicalPagePartNumber          = reportSection.LogicalPagePartNumber;
                            this._report.SectionPhysicalPageNumber      = reportSection.PhysicalPageNumber;
                            this._report.SectionLogicalPageNumber       = reportSection.LogicalPageNumber;
                            this._report.SectionLogicalPagePartNumber   = reportSection.LogicalPagePartNumber;

                            UIElement element = currPage.Visual as UIElement;

                            // JJD 10/14/08
                            // Make sure it's ready for print by forcing a measure and arrange
                            if (element != null)
                            {
                                element.InvalidateMeasure();
                                element.InvalidateArrange();
                                element.Measure(currPage.Size);
                                element.Arrange(new Rect(new Point(), currPage.Size));
                                element.UpdateLayout();
                            }

                            if (this._isPreviewMode)
                            {
                                //this._pageVisual = reportSection.CreateVisualBrush();
                                this._pageVisual = CreatePreviewCopy(currPage.Visual);
                            }

                            // fire progress event
                            RaisePrintProgress(pageNumber);
                            return currPage;
                        }
                    }
                }

                if (pageNumber > totalPages)
					throw new ArgumentOutOfRangeException("pageNumber", XamReportPreview.GetString("LE_ReportPaginator_OutOfRangeException"));

                Debug.Assert(false, "The printing is not ended properly");

                // if methods returns null the print rises exception
                // program  must never go here
                _isPageCountValid = true;
                Size size = new Size(100, 100);
                return new DocumentPage(new Canvas(), size, new Rect(size), new Rect(size));
            }

            #endregion //GetPage

                #region IsPageCountValid
            public override bool IsPageCountValid
            {
                get { return _isPageCountValid; }
            }
                #endregion

                #region PageCount
            /// <summary>
            /// This must be always 0. We control print break by seting _isPageCountValid to true;
            /// </summary>
            public override int PageCount
            {
                get { return 0; }
            }
            #endregion

                #region PageSize
            public override System.Windows.Size PageSize
            {
                get
                {
                    return _pageSize;
                }
                set
                {
                    _pageSize = value;
                }
            }
            #endregion

                #region Source
            public override IDocumentPaginatorSource Source
            {
                get { return this._report; }
            }
            #endregion

            #endregion //Base class overrides

            #region Properties

                #region Internal Properties
                
                    #region PageRange

            internal PageRange PageRange
            {
                set { this._pageRange = value; }
                get { return this._pageRange; }
            }

                    #endregion //PageRange

                    #region PrintPageCount
            internal int PrintPageCount
            {
                set { _printPageCount = value; }
            }
                    #endregion //PrintPageCount

                    #region IsPreviewMode
            internal bool IsPreviewMode
            {
                set { _isPreviewMode = value; }
            }
                    #endregion //IsPreviewMode

                    #region PageVisual
            /// <summary>
            /// Use for print preview
            /// </summary>
            internal Visual PageVisual
            {
                get { return _pageVisual; }
            }
                    #endregion //PageVisual

            internal bool IsCancel
            {
                set
                {
                    // cancel printing
                    this._isPageCountValid = true;
                    // store cancel state
                    this._isCancel = true;
                }
                get
                {
                    return this._isCancel;
                }
            }

                #endregion //InternalProperties

            #endregion

            #region Methods

                #region Internal methods

                    #region BeginPagination

            internal void BeginPagination()
            {
                this._maxLogicalNumbers = new List<int>();
                this._printPageCount = 0;
                foreach (ReportSection reportSection in this._report.Sections)
                {
                    this._maxLogicalNumbers.Add(0);
                    reportSection.OnBeginPagination(this._report);
                    if (reportSection is EmbeddedVisualReportSection)
                    {
                        _printPageCount += ((EmbeddedVisualReportSection)reportSection).EstmatedPageCount;
                    }
                    else
                        _printPageCount++;

                    if ((this._pageRange.PageTo != 0) &&
                        (this._pageRange.PageTo >= this._pageRange.PageFrom) &&
                        (this._pageRange.PageTo <= this._printPageCount))
                    {
                        this._printPageCount = this._pageRange.PageTo - this._pageRange.PageFrom;
                    }
                }
            }

                    #endregion //BeginPagination
            
                    #region CreatePreviewCopy



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            internal Visual CreatePreviewCopy(Visual pageContent)
            {
                // update layout before get snapshot
                //_pagePresenter.UpdateLayout();
                double scaleFactor = 2;
                RenderTargetBitmap bmp = new RenderTargetBitmap((int)(this._pageSize.Width * scaleFactor),
                    (int)(this._pageSize.Height * scaleFactor),
                    96 * scaleFactor, 96 * scaleFactor, PixelFormats.Default);
                bmp.Render(pageContent);

                ImageBrush brush = new ImageBrush(bmp);
                StackPanel myStackPanel = new StackPanel();
                //VisualBrush brush = new VisualBrush();
                //brush.Visual = pageContent;

                Rectangle rect = new Rectangle();
                rect.Width = this._pageSize.Width;
                rect.Height = this._pageSize.Height;
                rect.Fill = brush;
                myStackPanel.Children.Add(rect);
                return myStackPanel;

            }
                    #endregion

                    #region EndPagination

            internal void EndPagination()
            {
                foreach (ReportSection reportSection in this._report.Sections)
                    reportSection.OnEndPagination();
            }

                    #endregion //EndPagination

                    #region Initialize
            /// <summary>
            /// Initializes paginator members
            /// </summary>
            internal void Initialize(ReportSettings settings)
            {
                this._isPreviewMode = false;
                this._isPageCountValid = false;
                this._isCancel = false;
                this.PageRange = settings.PageRange;
                
                // JJD 9/1/09 - TFS19395
                // Use the new PageSizeResolved property instead
                //this.PageSize = settings.PageSize;
                this.PageSize = this._report.PageSizeResolved;

                this.BeginPagination();

				// AS 8/25/11 TFS82921
				_report._hasCalledBeginPagination = true;
            }
                    #endregion //Initialize

                #endregion Internal methods

                #region Private methods
            /// <summary>
            /// Raises the PrintProgress event.
            /// </summary>
            /// <param name="pageNumber">A PrintProgressEventArgs that contains the event data.</param>
            private void RaisePrintProgress(int pageNumber)
            {
                // correct page number because it is 0 based index
                int correctPageNumber = pageNumber + 1;
                double PercentCompleted = ((double)correctPageNumber / this._printPageCount) * 100;

                string description;
                // JJD 11/24/09 - TFS24840
                // Use preview string if appropriate
                if ( this._report != null && this._report.IsGeneratingPreview )
					description = XamReportPreview.GetString("ProgressPrintingDescription_Preview", correctPageNumber);
                else
					description = XamReportPreview.GetString("ProgressPrintingDescription", correctPageNumber);

                //PrintProgressEventArgs args = new PrintProgressEventArgs(correctPageNumber, SR.GetString("ProgressPrintingDescription", correctPageNumber), (int)PercentCompleted);
                PrintProgressEventArgs args = new PrintProgressEventArgs(correctPageNumber, description, (int)PercentCompleted);
                if (this.PrintProgress != null)
                {
                    this.PrintProgress(this, args);
                }
            }
                #endregion

            #endregion //Methods

            #region Events

            internal event EventHandler<PrintProgressEventArgs> PrintProgress;

            #endregion
        }

        #endregion //Paginator private class
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