using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Printing;

namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// A class that exposes settings that relate to a Report.
    /// </summary>
    /// <remarks>
    /// <p class="body">This class is used to specify settings that determine the way a report will be printed.</p>
    /// </remarks>
    /// <seealso cref="ReportBase"/>
    /// <seealso cref="ReportBase.ReportSettings"/>
    public class ReportSettings
    {
        #region Member Variables
        
        private PrintQueue _printQueue;
        private RepeatType _repeatType;
        private PageRange _pageRange;
        private Size _pageSize;
        private PageOrientation _pageOrientation;
        private Thickness _margin;
        private String _fileName;
        private bool _userPageRangeEnabled;
        private PagePrintOrder _pagePrintOrder;

        private HorizontalPaginationMode _horizontalPaginationMode;

        private static PageRange DefaultPageRange = new PageRange(1, 0);

		// JJD 3/25/11 - TFS70336 - Optimization
		private int _pageMetricsVersion;

        #endregion //Member Variables

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSettings"/>  class.
        /// </summary>
        public ReportSettings()
        {
            this._fileName = "";
            this._margin = new Thickness();
            this._pageRange = DefaultPageRange;
            this._userPageRangeEnabled = true;
            this._horizontalPaginationMode = HorizontalPaginationMode.Mosaic;
        }

        #endregion //Constructors

        #region Properties

            #region Public Properties

                #region FileName
        /// <summary>
        /// Returns/sets the name of the file where the report will be saved.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> if this property is not set when exporting a report then a default name of <b>Export.xps</b> 
        /// will be used. If the FileName does not contain a fully qualified path then the 
        /// <see cref="System.Environment.CurrentDirectory"/> will be used.
        /// </p>
        /// </remarks>
        //[Description("Returns/sets the name of the file where the report will be saved.")]
        //[Category("Data")]
        [DefaultValue("")]
        public String FileName
        {
            get { return this._fileName; }
            set { this._fileName = value; }
        }
                #endregion

                #region HorizontalPaginationMode
        /// <summary>
        /// Returns/sets the way logical pages are printed when they are too wide to fit on one page.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property defines how to print a logical page when it is too wide to fit on a single physical page. 
        /// You can either scale it down so to fits on a single page or split it up over multiple page parts horizontally with the default 'Mosaic' setting.</p>
        /// <p class="note"><b>Note:</b> if 'Scale' is specified then the aspect ratio will be maintained, i.e. the scale factor for the width and height will be the same.</p>
        /// </remarks>
        /// <seealso cref="PagePrintOrder"/>
        /// <seealso cref="Infragistics.Windows.Reporting.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSection.LogicalPageNumber"/>
        /// <seealso cref="ReportSection.LogicalPagePartNumber"/>
        /// <seealso cref="ReportSection.PhysicalPageNumber"/>
        //[Description("Returns/sets the way logical pages are printed when they are too wide to fit on one page.")]
        //[Category("Behavior")]
        [DefaultValue(HorizontalPaginationMode.Mosaic)]
        public HorizontalPaginationMode HorizontalPaginationMode
        {
            get { return this._horizontalPaginationMode; }
            set { this._horizontalPaginationMode = value; }
        }
                #endregion

                #region Margin
        /// <summary>
        /// Gets or sets the margin around the ReportPagePresenter
        /// </summary>
        /// <remarks>
        /// <p class="body">The margin is the space between page presenter and page’s bounds, see <see cref="PageSize"/>.</p>
        /// </remarks>
        //[Description("Gets or sets the margin around the ReportPagePresenter.")]
        //[Category("Appearance")]
        public Thickness Margin
        {
            get { return this._margin; }
            set 
			{ 
				this._margin = value;
				// JJD 3/25/11 - TFS70336 - Optimization
				this._pageMetricsVersion++;
			}
        }

        /// <summary>
        /// Determines if the <see cref="Margin"/> property needs to be serialized.
        /// </summary>
        /// <returns>True if the property should be serialized</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMargin()
        {
            return this._margin != new Thickness();
        }

        /// <summary>
        /// Resets the <see cref="Margin"/> property to its default value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetMargin()
        {
            this._margin = new Thickness(); ;
        }

                #endregion

                #region PageOrientation
        /// <summary>
        /// Returns/sets the orientation of the page.
        /// </summary>
        /// <seealso cref="PageOrientation"/>
        //[Description("Returns/sets the orientation of the page.")]
        //[Category("Appearance")]
        [DefaultValue(PageOrientation.Portrait)]
        public PageOrientation PageOrientation
        {
            get { return this._pageOrientation; }
            set 
			{ 
				this._pageOrientation = value;
				
				// JJD 3/25/11 - TFS70336 - Optimization
				this._pageMetricsVersion++;
			}
        }
        #endregion

                #region PagePrintOrder
        /// <summary>
        /// Returns/sets the order that pages are printed when HorizontalPaginationMode is set to 'Mosaic'.
        /// </summary>
        /// <remarks>
        /// <p class="body">When the visual you want to print doesn't fit on a single page, and the <see cref="HorizontalPaginationMode"/> is set to "Mosaic", you must decide the order you want to split the visual on the page. There are two way to paginate visual. First print down, then over and second print over, then down. </p>
        /// </remarks>
        /// <p class="body">When a logical page within a report is too wide to fit and <see cref="HorizontalPaginationMode"/> is set to 'Mosaic' 
        /// then it will be will be split up into multiple page parts horizontally. This property determines the order that the page parts are placed in the report.</p>
        /// <seealso cref="Infragistics.Windows.Reporting.PagePrintOrder"/>
        /// <seealso cref="HorizontalPaginationMode"/>
        /// <seealso cref="ReportSection.LogicalPageNumber"/>
        /// <seealso cref="ReportSection.LogicalPagePartNumber"/>
        /// <seealso cref="ReportSection.PhysicalPageNumber"/>
        //[Description("Returns/sets the order that pages are printed when HorizontalPaginationMode is set to 'Mosaic'.")]
        //[Category("Behavior")]
        [DefaultValue(PagePrintOrder.Horizontal)]
        public PagePrintOrder PagePrintOrder
        {
            get { return this._pagePrintOrder; }
            set { this._pagePrintOrder = value; }
        }
                #endregion

                #region PageRange
        /// <summary>
        /// Returns/sets the range of pages to be included in the report. 
        /// </summary>
        /// <remarks>
        /// <para class="note">If PageTo is zero then the report will go from PageFrom until the end of the report.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">PageFrom is less than 1 or PageTo is less than 0 or PageTo greater than 0 but less than PageFrom.
        /// </exception>
        //[Description("Returns/sets the range of pages to be included in the report.")]
        //[Category("Data")]
        [DefaultValue(null)]
        public PageRange PageRange
        {
            get { return this._pageRange; }
            set
            {
                if (value.PageFrom < 1 )
                    throw new ArgumentException(SR.GetString("LE_ArgumentException_23"));
                if (value.PageTo < 0)
                    throw new ArgumentException(SR.GetString("LE_ArgumentException_24"));
                if (value.PageTo > 0 && value.PageTo < value.PageFrom)
                    throw new ArgumentException(SR.GetString("LE_ArgumentException_25"));

                this._pageRange = value;
            }
        }

        /// <summary>
        /// Determines if the <see cref="PageRange"/> property needs to be serialized.
        /// </summary>
        /// <returns>True if the property should be serialized</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializePageRange()
        {
            return this._pageRange != DefaultPageRange;
        }

        /// <summary>
        /// Resets the <see cref="PageRange"/> property to its default value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetPageRange()
        {
            this._pageRange = DefaultPageRange;
        }

                #endregion

                #region UserPageRangeEnabled
        /// <summary>
        /// Returns/sets whether the standard PrintDialog will allow to user to select a range of pages. 
        /// </summary>
        /// <remarks>
        /// <p class="body">Specifies whether the user will be able so specify whether all the pages or only a limited range will be printed or exported.
        /// The default value is true.</p>
        /// </remarks>
        //[Description("Returns/sets whether the standard PrintDialog will allow to user to select a range of pages.")]
        //[Category("Behavior")]
        [DefaultValue(true)]
        public bool UserPageRangeEnabled
        {
            get { return this._userPageRangeEnabled; }
            set { this._userPageRangeEnabled = value; }
        }
                #endregion

                #region PageSize
        /// <summary>
        /// Returns/sets the size of the page.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> normally you won’t need to explicitly set the page size since it will be picked up through the PrintDialog and via the optional <see cref="PrintQueue"/> property. </p>
        /// </remarks>
        //[Description("Returns/sets the size of the page.")]
        //[Category("Appearance")]
        public Size PageSize
        {
            get { return this._pageSize; }
            set 
			{ 
				this._pageSize = value;
				
				// JJD 3/25/11 - TFS70336 - Optimization
				this._pageMetricsVersion++;
			}
        }

        /// <summary>
        /// Determines if the <see cref="PageSize"/> property needs to be serialized.
        /// </summary>
        /// <returns>True if the property should be serialized</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializePageSize()
        {
            return this._pageSize.IsEmpty;
        }

        /// <summary>
        /// Resets the <see cref="PageSize"/> property to its default value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetPageSize()
        {
            this._pageSize = new Size();
        }
                #endregion

                #region RepeatType
        /// <summary>
        /// Determines how logical headers within the page content area will be treated within a report section..
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In the case of a XamDataGrid this setting determines when field headers are displayed.</para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Reporting.RepeatType"/>
        //[Description("Returns/sets the RepeatType for appearing of XamDataGrid's header on every printed page.")]
        //[Category("Behavior")]
        [DefaultValue(RepeatType.FirstOccurrence)]
        public RepeatType RepeatType
        {
            get { return this._repeatType; }
            set { this._repeatType = value; }
        }
                #endregion

                #region PrintQueue
        /// <summary>
        /// Gets or sets a PrintQueue object that represents the target printer.
        /// </summary>
        /// <remarks>
        /// <p class="body">You supply a <see cref="System.Printing.PrintQueue"/> object if you prefer not showing the standard <see cref="System.Windows.Controls.PrintDialog"/>.</p>
        /// </remarks>
        //[Description("Gets or sets a PrintQueue object that represents the target printer.")]
        //[Category("Data")]
        [DefaultValue(null)]
        public PrintQueue PrintQueue
        {
            get { return this._printQueue; }
            set 
			{ 
				this._printQueue = value;
				
				// JJD 3/25/11 - TFS70336 - Optimization
				this._pageMetricsVersion++;
			}
        }
                #endregion

            #endregion //Public Properties

			#region Internal Properties

				// JJD 3/25/11 - TFS70336 - Optimization
				#region PageMetricsVersion

		internal int PageMetricsVersion { get { return _pageMetricsVersion; } }

				#endregion //PageMetricsVersion

			#endregion //Internal Properties	
        
        #endregion //Properties
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