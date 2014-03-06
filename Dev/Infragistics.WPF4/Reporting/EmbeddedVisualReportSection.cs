using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;
using Infragistics.Windows.Reporting.Events;
using System.Collections.Generic;
using System.Windows.Data;
using Infragistics.Collections;


namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// Class that represents a section of a <see cref="Report"/> where each page's content is supplied by an embedded visual.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// An instance of this class can be added to a <see cref="Report"/>'s <see cref="Report.Sections"/> collection. 
    /// Then, when a pagination process is started, by calling <see cref="Report.Export(OutputFormat)"/> or <see cref="Report.Print()"/> on <see cref="Report"/>, 
    /// the EmbeddedVisualReportSection class creates an instance of a <see cref="ReportPagePresenter"/> and sets 
    /// its content based on the parameters passed into one of its constructors. It then rasies the <see cref="PaginationStarting"/>, <see cref="PaginationStarted"/> and finally the <see cref="PaginationEnded"/> events.
    /// </p>
    /// <p class="body">The EmbeddedVisualReportSection class can contain three types of objects, supplied in the constructor:
    ///     <ul>
    ///         <li>The objects that implement <see cref="IEmbeddedVisualPaginator"/> interface</li>
    ///         <li>The objects that implement <see cref="IEmbeddedVisualPaginatorFactory"/> interface, e.g. the XamDataGrid</li>
    ///         <li>The objects derived from <b>System.Windows.Media.Visual</b></li>
    ///     </ul>
    /// The first and second type of objects ultimately provide pagination support through the <see cref="IEmbeddedVisualPaginator"/> interface. 
    /// The third type of object will be printed in one page and if they don’t fit on page, they will be clipped by default. 
    /// To prevent clipping of the visual set <see cref="ReportSettings.HorizontalPaginationMode"/> on <see cref="ReportBase.ReportSettings"/> to 'Scale'. 
    /// </p>
    /// <para class="note"><b>Note:</b> each section in a <see cref="Report"/> starts on a new page. For example, if you created a <b>Report</b> with 3 EmbeddedVisualReportSections, one with a XamDataGrid and 2 others with 
    /// simple visual elements, each section would start on a new page even though there might have been available space on the last page from the previous section.</para>
    /// </remarks>
    public class EmbeddedVisualReportSection : ReportSection
    {
        #region Member Variables
        private Size _pageSize;
        private Vector _pageOrigin;// JJD 11/24/09 - TFS25026 - added
        private Visual _reportVisual;
        private IEmbeddedVisualPaginator _visualPaginator;
        private ReportPagePresenter _pagePresenter;
        private int _pageNumberOfLastGet = -1;
        private int _currentPageNumber;
        private Visual _sourceVisual;
        private bool _wasPaginatorPassedIntoConstructor;
        private List<PagePosition> _pagePositions = new List<PagePosition>();

        // JJD 10/10/08 - TFS8339
        // Added flag so we know when the content area size is too small
        private bool _isContentAreaTooSmallForPaginator;
        private const double MinimumPageContentWidth = 100;
        private const double MinimumPageContentHeight = 100;

        #endregion //Member Variables

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedVisualReportSection"/> class.
        /// </summary>
        /// <param name="sourceVisual">The source visual to be printed</param>
        /// <remarks>
        /// <p class="body">If sourceVisual implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface then the interface's <see cref="IEmbeddedVisualPaginatorFactory.Create"/> method 
        /// will be called to create the <see cref="VisualPaginator"/> before the <see cref="PaginationStarting"/> event is raised.</p>
        /// <p class="body">In this case the <see cref="VisualPaginator"/> will be cleared immediately after the <see cref="PaginationEnded"/> event is raised.</p>
        /// <p class="note"><b>Note:</b>XamDataGrid implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface and returns a special derived class of DataPresenterBase as the <see cref="VisualPaginator"/>.
        /// Therefore, if special event handling is required, the events can be wired up in the <see cref="PaginationStarting"/> event by casting the <see cref="VisualPaginator"/> to DataPresenterBase. 
        /// Those events can be unwired in the <see cref="PaginationEnded"/> event.</p>
        /// </remarks>
        /// <exception cref="ArgumentNullException">sourceVisual can not be null.</exception>
        /// <seealso cref="IEmbeddedVisualPaginatorFactory"/>
        /// <seealso cref="IEmbeddedVisualPaginator"/>
        /// <seealso cref="VisualPaginator"/>
        /// <seealso cref="PaginationStarting"/>
        /// <seealso cref="PaginationStarted"/>
        /// <seealso cref="PaginationEnded"/>
        /// <seealso cref="SourceVisual"/>
        public EmbeddedVisualReportSection(Visual sourceVisual) : this(sourceVisual, sourceVisual as IEmbeddedVisualPaginator )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedVisualReportSection"/> class.
        /// </summary>
        /// <param name="sourceVisual">The source visual to be printed</param>
        /// <param name="visualPaginator">Object that implements the IEmbeddedVisualPaginator interface to provide pagination.</param>
        /// <remarks>
        /// <p class="body">If sourceVisual implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface then the interface's <see cref="IEmbeddedVisualPaginatorFactory.Create"/> method 
        /// will be called to create the <see cref="VisualPaginator"/> before the <see cref="PaginationStarting"/> event is raised.</p>
        /// <p class="body">In this case the <see cref="VisualPaginator"/> will be cleared immediately after the <see cref="PaginationEnded"/> event is raised.</p>
        /// <p class="note"><b>Note:</b>XamDataGrid implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface and returns a special derived class of DataPresenterBase as the <see cref="VisualPaginator"/>.
        /// Therefore, if special event handling is required, the events can be wired up in the <see cref="PaginationStarting"/> event by casting the <see cref="VisualPaginator"/> to DataPresenterBase. 
        /// Those events can be unwired in the <see cref="PaginationEnded"/> event.</p>
        /// </remarks>
        /// <exception cref="ArgumentNullException">sourceVisual can not be null.</exception>
        /// <seealso cref="IEmbeddedVisualPaginatorFactory"/>
        /// <seealso cref="IEmbeddedVisualPaginator"/>
        /// <seealso cref="VisualPaginator"/>
        /// <seealso cref="PaginationStarting"/>
        /// <seealso cref="PaginationStarted"/>
        /// <seealso cref="PaginationEnded"/>
        /// <seealso cref="SourceVisual"/>
        public EmbeddedVisualReportSection(Visual sourceVisual, IEmbeddedVisualPaginator visualPaginator)
        {
            if (sourceVisual == null)
                throw new ArgumentNullException("sourceVisual");

            this._sourceVisual = sourceVisual;
            this._reportVisual = sourceVisual;

            if (visualPaginator != null)
            {
                this._visualPaginator = visualPaginator;
                this._wasPaginatorPassedIntoConstructor = true;
            }
        }

        #endregion //Constructors
        
        #region Events

            #region PaginationEnded

        /// <summary>
        /// Occurs after a pagination process has ended.
        /// </summary>
        /// <seealso cref="PaginationEnded"/>
        /// <seealso cref="OnPaginationEnded"/>
        /// <seealso cref="PaginationStarted"/>
        /// <seealso cref="PaginationStarting"/>
        //[Description("Occurs after a pagination process has ended")]
        //[Category("Reporting Properties")] // Behavior
        public event EventHandler<EmbeddedVisualPaginationEventArgs> PaginationEnded;

        /// <summary>
        /// Occurs after a pagination process has been started
        /// </summary>
        /// <seealso cref="PaginationEnded"/>
        protected virtual void OnPaginationEnded(EmbeddedVisualPaginationEventArgs args)
        {
            if (this.PaginationEnded != null)
                this.PaginationEnded(this, args);
         }

        internal void RaisePaginationEnded(IEmbeddedVisualPaginator paginator)
        {
            EmbeddedVisualPaginationEventArgs args = new EmbeddedVisualPaginationEventArgs(paginator);
            this.OnPaginationEnded(args);
        }

            #endregion //PaginationEnded

            #region PaginationStarted

        /// <summary>
        /// Occurs after a pagination process has been started.
        /// </summary>
        /// <seealso cref="PaginationStarted"/>
        /// <seealso cref="OnPaginationStarted"/>
        /// <seealso cref="PaginationEnded"/>
        /// <seealso cref="PaginationStarting"/>
        //[Description("Occurs after a pagination process has been started")]
        //[Category("Reporting Properties")] // Behavior
        public event EventHandler<EmbeddedVisualPaginationEventArgs> PaginationStarted;

        /// <summary>
        /// Occurs after a pagination process has been started
        /// </summary>
        /// <seealso cref="PaginationStarted"/>
        protected virtual void OnPaginationStarted(EmbeddedVisualPaginationEventArgs args)
        {
            if (this.PaginationStarted != null)
                this.PaginationStarted(this, args);
         }

        internal void RaisePaginationStarted(IEmbeddedVisualPaginator paginator)
        {
            EmbeddedVisualPaginationEventArgs args = new EmbeddedVisualPaginationEventArgs(paginator);
            this.OnPaginationStarted(args);
        }

            #endregion //PaginationStarted

            #region PaginationStarting

        /// <summary>
        /// Occurs before a pagination process is about to begin.
        /// </summary>
        /// <remarks>
        /// <p class="body">If <see cref="SourceVisual"/> implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface then the interface's <see cref="IEmbeddedVisualPaginatorFactory.Create"/> method 
        /// will be called to create the <see cref="VisualPaginator"/> before this event is raised.</p>
        /// <p class="body">In this case the <see cref="VisualPaginator"/> will be cleared immediately after the <see cref="PaginationEnded"/> event is raised.</p>
        /// <p class="note"><b>Note:</b>XamDataGrid implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface and returns a special derived class of DataPresenterBase as the <see cref="VisualPaginator"/>.
        /// Therefore, if special event handling is required, the events can be wired up in the <see cref="PaginationStarting"/> event by casting the <see cref="VisualPaginator"/> to DataPresenterBase. 
        /// Those events can then be unwired in the <see cref="PaginationEnded"/> event.</p>
        /// </remarks>
        /// <seealso cref="PaginationStarting"/>
        /// <seealso cref="OnPaginationStarting"/>
        /// <seealso cref="PaginationStarted"/>
        /// <seealso cref="PaginationEnded"/>
        //[Description("Occurs before a pagination process is about to begin.")]
        //[Category("Reporting Properties")] // Behavior
        public event EventHandler<EmbeddedVisualPaginationEventArgs> PaginationStarting;

        /// <summary>
        /// Occurs before a pagination process is about to begin.
        /// </summary>
        /// <seealso cref="PaginationStarting"/>
        protected virtual void OnPaginationStarting(EmbeddedVisualPaginationEventArgs args)
        {
            if (this.PaginationStarting != null)
                this.PaginationStarting(this, args);
         }

        internal void RaisePaginationStarting(IEmbeddedVisualPaginator paginator)
        {
            EmbeddedVisualPaginationEventArgs args = new EmbeddedVisualPaginationEventArgs(paginator);
            this.OnPaginationStarting(args);
        }

            #endregion //PaginationStarting

        #endregion //Events

        #region Properties

            #region Internal properties

                // JJD 10/14/08 - renamed
                #region EstmatedPageCount
        internal int EstmatedPageCount
        {
            get
            {
                // JJD 10/11/08 - TFS8339
                // Check IsSinglePageSection 
                if (this.IsSinglePageSection)
                    return 1;

                return this._visualPaginator.EstimatedPageCount;
            }
        }

                #endregion //EstmatedPageCount

                // JJD 10/10/08 - TFS8339 - added 
                #region IsSinglePageSection

        internal bool IsSinglePageSection
        {
            get
            {
                return this._visualPaginator == null || this._isContentAreaTooSmallForPaginator;
            }
        }

                #endregion //IsSinglePageSection	
    
                #endregion //Internal Properties

            #region Public properties

                #region SourceVisual
        /// <summary>
        /// Returns the source visual that was passed into the constructor (read-only).
        /// </summary>
        /// <value>The sourceVisual passed into the constructor.</value>
        //[Description("Returns the source visual that was passed into the constructor (read-only).")]
        //[Category("Reporting Properties")] // Behavior
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visual SourceVisual
        {
            get { return this._sourceVisual; }
        }
                #endregion
                
                #region VisualPaginator
        /// <summary>
        /// Returns the visual paginator that will be used inside the report section (read-only).
        /// </summary>
        /// <value>An object that implements the <see cref="IEmbeddedVisualPaginator"/> interface or null.</value>
        /// <remarks>
        /// <p class="body">If the VisualPaginator was supplied in the constructor it will be returned by this property. However, if the <see cref="SourceVisual"/> 
        /// implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface then this property will return null except during a pagination operation.</p>
        /// <p class="note"><b>Note:</b>XamDataGrid implements the <see cref="IEmbeddedVisualPaginatorFactory"/> interface and returns a special derived class of DataPresenterBase as the <see cref="VisualPaginator"/>.
        /// Therefore, if special event handling is required, the events can be wired up in the <see cref="PaginationStarting"/> event by casting the VisualPaginator to DataPresenterBase. 
        /// Those events can be unwired in the <see cref="PaginationEnded"/> event.</p>
        /// </remarks>
        //[Description("Returns the visual paginator that will be used inside the report section.")]
        //[Category("Reporting Properties")] // Behavior
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEmbeddedVisualPaginator VisualPaginator
        {
            get { return this._visualPaginator; }
        }
                #endregion

            #endregion //Public Properties

        #endregion //Properties

        #region Base class overrides

            #region GetPage

        /// <summary>
        /// Gets the page from the section
        /// </summary>
        /// <param name="pageNumber">Zero-based physical page number relative to the beginning of the section.</param>
        /// <returns>A DocumentPage containing the page or null if past end of section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The page number parameter is less than zero.</exception>
        public override DocumentPage GetPage(int pageNumber)
        {
            if (pageNumber < 0)
                throw new ArgumentOutOfRangeException(XamReportPreview.GetString("LE_EmbeddedVisualReportSection_GetPage_LessThanZero"));

            _currentPageNumber = pageNumber + 1;

			// JJD 10/19/10 - TFS57580
			// Call InitializePageTemplates to pikc up any changes made to any of the template properties
			this.InitializePageTemplates();

            Report report = this.Report as Report;

            #region Handle cases where we print only one page, e.g. if there is no paginator or not enough space for one
            // check if must print only visual
            // JJD 10/11/08 - TFS8339
            // Check IsSinglePageSection 
            if (this.IsSinglePageSection)
            {
                if (pageNumber > 0)
                    return null;
                else
                {
                    // we print only one page with visual
                    
                    
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


                    this._pagePositions.Add(PagePosition.LastPage);

                    // JJD 10/10/08 - TFS8339
                    // Check the _isContentAreaTooSmallForPaginator flag
                    if (this._isContentAreaTooSmallForPaginator)
                    {
                        #region Handle case where the content area on the page presenter is too small for a visual paginator

                        ContentControl cc = new ContentControl();
                        cc.Padding = new Thickness(50);
                        TextBlock tb = new TextBlock();
                        tb.Width = this._pageSize.Width - 100;
                        tb.Height = this._pageSize.Height - 100;
                        tb.TextWrapping = TextWrapping.Wrap;
                        //"Unable to print this report section because the content area on the page was too small. The page size (width,height) was {0} and the margin settings were: left: {3}, top: {4}, right: {5} and bottom: {6}. This resulted in a content area size, after headers and footers, of {1}. The mimimum content area required is {2}."
						tb.Text = XamReportPreview.GetString("PageContentAreaTooSmall", new object[] { this._pageSize, this._pagePresenter.ContentAreaAvailableSize, new Size(MinimumPageContentWidth, MinimumPageContentHeight), this._pagePresenter.Margin.Left, this._pagePresenter.Margin.Top, this._pagePresenter.Margin.Right, this._pagePresenter.Margin.Bottom });
                        cc.Content = tb;
                        
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                        // JJD 11/24/09 - TFS25026
                        // Re-factored into helper method
                        //return new DocumentPage(cc, _pageSize, new Rect(_pageSize), new Rect(_pageSize));
                        return this.CreateDocumentPage(cc);
                        #endregion //Handle case where the content area on the page presenter is too small for a visual paginator
                    }
                    else
                    {
                        // JJD 11/24/09 - TFS25026
                        // Re-factored into helper method
                        //return new DocumentPage(this._pagePresenter, _pageSize, new Rect(_pageSize), new Rect(_pageSize));
                        return this.CreateDocumentPage(this._pagePresenter);
                    }
                }
            }
            #endregion
    
            if (pageNumber == 0)
            {
                this._visualPaginator.MoveToPosition(PagePosition.FirstPage);
                this._pagePositions.Add(this._visualPaginator.CurrentPagePosition);
            }
            else if (pageNumber <= this._pageNumberOfLastGet)
            {
                this._visualPaginator.MoveToPosition(this._pagePositions[pageNumber]);
            }
            else
            {
                // Move to last known page...
                if (this._pagePositions.Count == 0)
                {
                    this._visualPaginator.MoveToPosition(PagePosition.FirstPage);
                    this._pagePositions.Add(this._visualPaginator.CurrentPagePosition);
                }
                else if(this._visualPaginator.CurrentPagePosition.Equals(this._pagePositions[this._pagePositions.Count - 1]) == false)
                {
                    this._visualPaginator.MoveToPosition(this._pagePositions[this._pagePositions.Count - 1]);
                }//else we are there
                
                //loop while we reach desire position
                while (this._pageNumberOfLastGet < pageNumber)
                {
                    if (!this._visualPaginator.MoveToNextPage())
                        return null;

                    this._pagePositions.Add(this._visualPaginator.CurrentPagePosition);
                    this._pageNumberOfLastGet++;
                }
            }

            
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


            // JJD 10/15/08
            // Set the DataContent of the page
            this._pagePresenter.DataContext = this._visualPaginator.CurrentPageDataContext;

            // JJD 11/24/09 - TFS25026
            // Re-factored into helper method
            //return new DocumentPage(this._pagePresenter, _pageSize, new Rect(_pageSize), new Rect(_pageSize));
            return this.CreateDocumentPage(this._pagePresenter);
        }
      
                #endregion //GetPage

            // JJD 10/14/08 - added
            #region GeneratedPageCount
        /// <summary>
        /// The number of physical pages generated so far (read-only). 
        /// </summary>
        /// <remarks>
        /// <p class="note">This property only has meaning during a pagination, otherwise it returns 0.</p>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GeneratedPageCount
        {
            get { return this._pagePositions.Count; }
        }

             #endregion //GeneratedPageCount

            #region IsEndReached
        /// <summary>
        /// Returns whether the report section is on the last page (read-only).
        /// </summary>
        /// <remarks>
        /// <p class="note">This property only has meaning during a pagination.</p>
        /// </remarks>
        //[Description("Returns value to determinate if the report section is on last page.")]
        //[Category("Reporting Properties")] // Behavior
        public override bool IsEndReached
        {
            get
            {
                // if we are using a visual paginator then check its current page position
                // JJD 10/11/08 - TFS8339
                // Check IsSinglePageSection 
                if (this.IsSinglePageSection == false)
                {
                    if (this._visualPaginator.CurrentPagePosition == PagePosition.LastPage)
                        return true;
                    else
                        return false;
                }
                // if not, we dont know about pages and print it in only one page
                else
                {
                    return true;
                }
            }
        }
            #endregion //IsEndReached

            #region LogicalChildren

        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return new SingleItemEnumerator(this._pagePresenter);
            }
        }

            #endregion //LogicalChildren
       
            #region LogicalPageNumber
        /// <summary>
        /// The logical page number of the current page being printed within this section. 
        /// </summary>
        /// <value>A 1-based integer representing the logical page number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if there are multiple sections within a report this property restarts at 1 within each section.</p>
        /// </remarks>
        /// <seealso cref="Report.SectionLogicalPageNumber"/>
        /// <seealso cref="ReportSection.LogicalPageNumber"/>
        /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSettings.PagePrintOrder"/>
        public override int LogicalPageNumber
        {
            get
            {
                // JJD 10/11/08 - TFS8339
                // Check IsSinglePageSection 
                if (this.IsSinglePageSection)
                    return 1;

                return this._visualPaginator.LogicalPageNumber;
            }
        }
            #endregion //LogicalPageNumber
        
            #region LogicalPagePartNumber
        /// <summary>
        /// The logical page part number of the current page being printed within this section (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page part number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
        /// </p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSettings.PagePrintOrder"/>
        /// <seealso cref="ReportPagePresenter.LogicalPagePartNumber"/>
        /// <seealso cref="ReportPagePresenter.SectionLogicalPagePartNumber"/>
        public override int LogicalPagePartNumber
        {
            get
            {
                // JJD 10/11/08 - TFS8339
                // Check IsSinglePageSection 
                if (this.IsSinglePageSection)
                    return 1;

                return this._visualPaginator.LogicalPagePartNumber;
            }
        }
            #endregion //LogicalPagePartNumber

            #region OnBeginPagination
        /// <summary>
        /// Called when a pagination operation is about to begin.
        /// </summary>
        /// <param name="report">The report that starts the printing.</param>
        /// <remarks>
        /// <p class="body">This method creates and prepares a <see cref="ReportPagePresenter"/> and raises the <see cref="PaginationStarting"/> and <see cref="PaginationStarted"/> events. </p>
        /// </remarks>
        /// <seealso cref="ReportSection.OnBeginPagination"/>
        /// <seealso cref="OnEndPagination"/>
        /// <seealso cref="PaginationStarting"/>
        /// <seealso cref="PaginationStarted"/>
      public override void OnBeginPagination(ReportBase report)
        {
            if (this._sourceVisual is IEmbeddedVisualPaginatorFactory && this._wasPaginatorPassedIntoConstructor == false)
            {
                IEmbeddedVisualPaginatorFactory factory = this._sourceVisual as IEmbeddedVisualPaginatorFactory;
                this._visualPaginator = factory.Create();
                if (this._visualPaginator == null)
					throw new NullReferenceException(XamReportPreview.GetString("LE_EmbeddedVisualReportSection_NullPaginator"));

                if ( this._visualPaginator is Visual)
                    this._reportVisual = this._visualPaginator as Visual;
            }

            // Set IsInReport attached property to true
            if ( this._reportVisual != null )
                ReportSection.SetIsInReport(_reportVisual, true);

            // call base method to store report obj in section
            base.OnBeginPagination(report);

            this._pageNumberOfLastGet = 0;

            this._pagePositions.Clear();
            
            // JJD 9/1/09 - TFS19395
            // Use the new PageSizeResolved property instead
            //this._pageSize = this.Report.ReportSettings.PageSize;
            this._pageSize = report.PageSizeResolved;

            // JJD 11/24/09 - TFS25026
            // Added page origin
            this._pageOrigin = report.PageOrigin;

            // JJD 9/1/09 - TFS19395
            // If the orientation is landscape and the page size is portriat
            // then flip the page size values
            if (report.ReportSettings.PageOrientation == PageOrientation.Landscape &&
                this._pageSize.Width < this._pageSize.Height)
            {
                this._pageSize = new Size(this._pageSize.Height, this._pageSize.Width);
                // JJD 11/24/09 - TFS25026
                // flip the origin values as well
                this._pageOrigin = new Vector(this._pageOrigin.Y, this._pageOrigin.X);
            }

            
            // JJD 11/24/09 - TFS25026
            // adjust the size by the origin amount
            this._pageSize.Width += this._pageOrigin.X;
            this._pageSize.Height += this._pageOrigin.Y;

            
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


            this.CreatePage();

            // JJD 10/01/08
            // Raise the PaginationStarting event
            this.RaisePaginationStarting(this._visualPaginator);

            // JJD 10/11/08 - TFS8339
            // Check IsSinglePageSection 
            if (this.IsSinglePageSection == false)
                this._visualPaginator.BeginPagination(this);

            // JJD 10/01/08
            // Raise the PaginationStarted event
            this.RaisePaginationStarted(this._visualPaginator);
        }

            #endregion //OnBeginPagination

            #region OnEndPagination

        /// <summary>
        /// Called when a pagination operation has ended.
        /// </summary>
        /// <remarks>
        /// <p class="body">This method releases the <see cref="ReportPagePresenter"/> and raises the <see cref="PaginationEnded"/> event. </p>
        /// </remarks>
        /// <seealso cref="ReportSection.OnEndPagination"/>
        /// <seealso cref="OnBeginPagination"/>
        /// <seealso cref="PaginationEnded"/>
        public override void OnEndPagination()
        {
            // JJD 10/11/08 - TFS8339
            // Check IsSinglePageSection 
            if (this.IsSinglePageSection == false)
                this._visualPaginator.EndPagination();

            // call the base
            base.OnEndPagination();

            // JJD 10/01/08
            // Raise the PaginationEnded event
            this.RaisePaginationEnded(this._visualPaginator);

            // AS 10/9/08
            // Do not hold onto the page presenter (or remain hooked into the Report) once
            // the report generation is complete.
            //
            this.ReleasePage();

            // AS 10/9/08
            // Moved up from the block below where we release the visual & paginator.
            //
            // Set IsInReport attached property to false
            if (this._reportVisual != null)
                ReportSection.SetIsInReport(_reportVisual, false);

            // JJD 10/01/08
            // If we created the paginator in OnBeginPagination then null out references to it
            if (this._visualPaginator != null && this._wasPaginatorPassedIntoConstructor == false)
            {
                if ( this._visualPaginator == this._reportVisual )
                    this._reportVisual = null;

                this._visualPaginator = null;
            }

            // JJD 10/14/08
            // Since the pagination process has ended clear the page positions collection
            this._pagePositions.Clear();

            
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        }

            #endregion //OnEndPagination

            #region PhysicalPageNumber
        /// <summary>
        /// The physical page number of the current page being printed within this section (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the physical page number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
        /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="LogicalPageNumber"/>.</p>
        /// </remarks>
        /// <seealso cref="LogicalPageNumber"/>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSettings.PagePrintOrder"/>
        public override int PhysicalPageNumber
        {
            get
            {
                return this._currentPageNumber;
            }
        }
            #endregion //PhysicalPageNumber

        #endregion //Base class overrides

        #region Methods

            #region Internal
      
            #endregion // Internal

            #region Protected

            #endregion //Protected methods

            #region Private

                // JJD 11/24/09 - TFS25026 - added
                #region CreateDocumentPage

        // JJD 11/24/09 - TFS25026
        // Re-factored into helper method
        private DocumentPage CreateDocumentPage(Visual visual)
        {
            Rect rect = new Rect(_pageSize);

            return new DocumentPage(visual, _pageSize, rect, rect);
        }

                #endregion //CreateDocumentPage	
    
                #region CreatePage

        // JJD 10/15/08
        // Made this method private 
        private void CreatePage()
        {
            if (this.Report == null)
				throw new InvalidOperationException(XamReportPreview.GetString("LE_MustCallCreatePageWithinPagination"));

            // AS 10/9/08
            // Ensure the last page was removed first.
            //
            this.ReleasePage();

            // AS 10/7/08 TFS8602
            // Changed these from int to double.
            //
            double width = 0;
            double height = 0;

            // JJD 10/10/08 - TFS8339
            // Added flag so we know when the content area size is too small. Initialize it here.
            this._isContentAreaTooSmallForPaginator = false;

            this._pagePresenter = new ReportPagePresenter(this);

			// JJD 10/19/10 - TFS57580
			// Moved logic into InitializePageTemplates method
			this.InitializePageTemplates();

            this.AddLogicalChild(this._pagePresenter);

            // AS 10/9/08
            // The Report may not be a Report instance.
            //
            //((Report)this.Report).PropertyChanged += new PropertyChangedEventHandler(OnReportPropertyChanged);
            Report report = this.Report as Report;

            if (null != report)
            {
                





                // JJD 10/10/08
                // Set the Padding on the page presenter to the Margin setting
                this._pagePresenter.Margin = report.ReportSettings.Margin;
            }

            // JJD 11/24/09 - TFS25026
            // Set PageOriginInternal which will cause the Margin property to be coerced
            this._pagePresenter.PageOriginInternal = this._pageOrigin;


            if (this._visualPaginator != null && this._reportVisual != null)
            {
                // JJD 10/15/08
                // Bind the data content of the report visual to this DataContext since we will set the DataContent of the page presenter
                // inside GetPage with the visual paginator's CurrentPageDataContext property and we don't want to interfere with its
                // normal inheritance of the DataContext
                BindingOperations.SetBinding(this._reportVisual, FrameworkContentElement.DataContextProperty, Utilities.CreateBindingObject(FrameworkElement.DataContextProperty, BindingMode.OneWay, this)); 
                
                this._pagePresenter.Content = this._reportVisual;
            }
            else
            {
                #region Create content for visual
                
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

                UIElement reportElement = this._reportVisual as UIElement;

                if (null != reportElement)
                {
                    Size desiredSize = reportElement.DesiredSize;

                    // use the desired by default
                    width = desiredSize.Width;
                    height = desiredSize.Height;

                    // S 10/7/08 TFS7924
                    if (desiredSize == new Size())
                    {
                        reportElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        desiredSize = reportElement.DesiredSize;
                        reportElement.Arrange(new Rect(desiredSize));

                        // JJD 9/17/09 - TFS22296
                        // Call UpdateLayout to make sure all elements are laid out correctly
                        reportElement.UpdateLayout();
                    }

                    Size renderSize = reportElement.RenderSize;

                    if (renderSize.Height != 0 || renderSize.Width != 0)
                    {
                        width = renderSize.Width;
                        height = renderSize.Height;
                    }
                }
                else
                {
                    Rect contentRect = VisualTreeHelper.GetContentBounds(this._reportVisual);
                    width = contentRect.Width;
                    height = contentRect.Height;
                }

                if (width != 0 && height != 0)
                {
                    VisualBrush visBrush = new VisualBrush(_reportVisual);
                    visBrush.Stretch = Stretch.None;

					// AS 12/9/09 TFS19786
					// There seems to be a bug in the VisualBrush that is causing the rendering 
					// to be offset. It seems that this is related to the fact that the 
					// VisualTreeHelper.GetDescendantBounds is including the ContentBounds of 
					// elements that are not visible (e.g. that are within a collapsed element).
					// I submitted the issue to MS but it seems that we can get around this for 
					// now by setting the alignment of the brush.
					//
					// https://connect.microsoft.com/WPF/feedback/ViewFeedback.aspx?FeedbackID=519254
					visBrush.AlignmentX = AlignmentX.Left;
					visBrush.AlignmentY = AlignmentY.Top;

                    // this is needed because if the visual is offseted than and bmp is ofsseted
                    // btw this is microsoft bug.
                    Rectangle content = new Rectangle();
                    content.Height = height;
                    content.Width = width;
                    content.Fill = visBrush;
                    content.Measure(new Size(width, height));
                    content.Arrange(new Rect(0, 0, width, height));

                    double dpiX = 192;
                    double dpiY = 192;
                    RenderTargetBitmap bmp = new RenderTargetBitmap((Int32)(width * dpiX / 96.0),
                                                                    (Int32)(height * dpiY / 96.0),
                                                                       dpiX,
                                                                       dpiY,
                                                                       PixelFormats.Pbgra32);


                    //RenderTargetBitmap bmp = new RenderTargetBitmap(width * scaleFactor, height * scaleFactor,
                    //    96 * scaleFactor, 96 * scaleFactor, PixelFormats.Default);

                    bmp.Render(content);

                    ImageBrush brush = new ImageBrush(bmp);
                    content.Fill = brush;

                    // measure printable area
                    this._pagePresenter.InvalidateMeasure();
                    this._pagePresenter.Measure(this._pageSize);
                    this._pagePresenter.Arrange(new Rect(this._pageSize));

                    //Apply ScaleTransform to Visual
                    
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                    Size renderSize = this._pagePresenter.RenderSize;
                    if (width > renderSize.Width ||
                        height > renderSize.Height)
                    {
                        if (this.Report.ReportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale)
                        {
                            double scaleX = renderSize.Width / width;
                            double scaleY = renderSize.Height / height;
                            double scale = Math.Min(scaleY, scaleX);

                            content.LayoutTransform = new ScaleTransform(scale, scale);
                        }
                    }

                    this._pagePresenter.Content = content;
                }
                #endregion
            }

            this._pagePresenter.InvalidateMeasure();
            this._pagePresenter.Measure(this._pageSize);
            this._pagePresenter.Arrange(new Rect(this._pageSize));
            this._pagePresenter.UpdateLayout();

            // JJD 10/10/08 - TFS8339
            // Check to see if the content area is large enought for a visual paginator assuming we have one.
            if (this._visualPaginator != null)
            {
                Size contentAreaSize = this._pagePresenter.ContentAreaAvailableSize;
                if (contentAreaSize.Width < MinimumPageContentWidth ||
                     contentAreaSize.Height < MinimumPageContentHeight)
                    this._isContentAreaTooSmallForPaginator = true;
            }

        }

                #endregion //CreatePage

				// JJD 10/19/10 - TFS57580
				#region InitializePageTemplates

		private void InitializePageTemplates()
		{
			if (this._pagePresenter == null)
				return;

			this._pagePresenter.Header = SelectHeader();
			this._pagePresenter.HeaderTemplate = SelectHeaderTemplate();
			this._pagePresenter.HeaderTemplateSelector = SelectHeaderTemplateSelector();

			this._pagePresenter.ContentTemplate = SelectContentTemplate();
			this._pagePresenter.ContentTemplateSelector = SelectContentTemplateSelector();

			this._pagePresenter.Footer = SelectFooter();
			this._pagePresenter.FooterTemplate = SelectFooterTemplate();
			this._pagePresenter.FooterTemplateSelector = SelectFooterTemplateSelector();
		}

				#endregion //InitializePageTemplates	
    
                #region OnReportPropertyChanged
        
#region Infragistics Source Cleanup (Region)





























































#endregion // Infragistics Source Cleanup (Region)

            #endregion //OnReportPropertyChanged


            // AS 10/9/08
            // Moved here from the OnBeginPagination so we can call this to clean up when pagination is complete.
            //
            #region ReleasePage
        private void ReleasePage()
        {
            // AS 10/9/08
            // Moved here from OnBeginPagination and reorganized it. You need to 
            // make sure that you don't return the element from the LogicalChildren
            // before you call RemoveLogicalChild because that will ask for the LogicalChildren
            // during the call to see if it still has any logical children.
            //
            ReportPagePresenter pagePresenter = this._pagePresenter;

            if (pagePresenter != null)
            {
                this._pagePresenter = null;

                // JJD 4/24/09 - TFS17057
                // clear the header, footer and the content
                pagePresenter.ClearValue(ReportPagePresenter.ContentProperty);
                pagePresenter.ClearValue(ReportPagePresenter.HeaderProperty);
                pagePresenter.ClearValue(ReportPagePresenter.FooterProperty);

                this.RemoveLogicalChild(pagePresenter);
            }

            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }
            #endregion //ReleasePage

            #region SelectHeader(Template(Selector))
        private object SelectHeader()
        {
            if (this.PageHeader != null)
                return this.PageHeader;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageHeader != null)
                return report.PageHeader;

            return null;
        }
        private DataTemplate SelectHeaderTemplate()
        {
            if (this.PageHeaderTemplate != null)
                return this.PageHeaderTemplate;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageHeaderTemplate != null)
                return report.PageHeaderTemplate;

            return null;
        }
        private DataTemplateSelector SelectHeaderTemplateSelector()
        {
            if (this.PageHeaderTemplateSelector != null)
                return this.PageHeaderTemplateSelector;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageHeaderTemplateSelector != null)
                return report.PageHeaderTemplateSelector;

            return null;
        } 
            #endregion //SelectHeader(Template(Selector))

            #region SelectFooter(Template(Selector))
        private object SelectFooter()
        {
            if (this.PageFooter != null)
                return this.PageFooter;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageFooter != null)
                return report.PageFooter;

            return null;
        }
        private DataTemplate SelectFooterTemplate()
        {
            if (this.PageFooterTemplate != null)
                return this.PageFooterTemplate;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageFooterTemplate != null)
                return report.PageFooterTemplate;

            return null;
        }
        private DataTemplateSelector SelectFooterTemplateSelector()
        {
            if (this.PageFooterTemplateSelector != null)
                return this.PageFooterTemplateSelector;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageFooterTemplateSelector != null)
                return report.PageFooterTemplateSelector;

            return null;
        } 
            #endregion //SelectFooter(Template(Selector))

            #region SelectContentTemplate(Selector)
        private DataTemplate SelectContentTemplate()
        {
            if (this.PageContentTemplate != null)
                return this.PageContentTemplate;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageContentTemplate != null)
                return report.PageContentTemplate;

            return null;
        }
        private DataTemplateSelector SelectContentTemplateSelector()
        {
            if (this.PageContentTemplateSelector != null)
                return this.PageContentTemplateSelector;

            // JJD 10/14/08
            // Check for a null report
            ReportBase report = this.Report;
            if (report == null)
                return null;

            if (report.PageContentTemplateSelector != null)
                return report.PageContentTemplateSelector;

            return null;
        } 
                #endregion //SelectContentTemplate(Selector)

            #endregion // Private

        #endregion //Methods
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