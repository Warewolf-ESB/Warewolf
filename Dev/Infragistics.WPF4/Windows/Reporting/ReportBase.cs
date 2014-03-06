using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System;
using System.Printing;

namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// Abstract base class for reporting. 
    /// </summary>
    /// <remarks>
    /// <p class="body">The class exposes properties that determine the default settings for the report.</p>
    /// <para class="note"><b>Note:</b> the derived class, Report, is in the Infragistics.Wpf.Reporting assembly, and it exposes a 
    /// collection of <see cref="ReportSection"/>s which surface additional properties that can be used to override the defaults specified here.</para>
    /// </remarks>
    public abstract class ReportBase : PropertyChangeNotifier
    {
        #region Member Variables

        private DataTemplateSelector _pageHeaderTemplateSelector;
        private DataTemplateSelector _pageContentTemplateSelector;
        private DataTemplateSelector _pageFooterTemplateSelector;
        private DataTemplate _pageFooterTemplate;
        private DataTemplate _pageContentTemplate;
        private DataTemplate _pageHeaderTemplate;
        private object _pageFooter;
        private object _pageHeader;
        private Style _pagePresenterStyle;
        private bool _accessFailed;
        private Vector _origin = new Vector();
		// JJD 3/25/11 - TFS70336 - Initialize to Empty instead
        //private Size _imageableAreaExtent = new Size();
        private Size _imageableAreaExtent = Size.Empty;

		// JJD 3/25/11 - TFS70336 - Added
		private int _pageMetricsVersion;
		private WeakReference _lastSettings;

        #endregion //Member Variables

        #region Constructors
        /// <summary>
        /// Protected constructor
        /// </summary>
        protected ReportBase()
        {
        }
        #endregion

        #region Properties

            #region Public Properties

                #region PageContentTemplate
        /// <summary>
        /// The template used to display the content of ReportPagePresenter control.
        /// </summary>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the content of ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplate PageContentTemplate
        {
            get
            {
                return _pageContentTemplate;
            }
            set
            {
                _pageContentTemplate = value;
            }
        }
                #endregion //PageContentTemplate

                #region PageContentTemplateSelector
        /// <summary>
        /// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the ReportPagePresenter's content.
        /// </summary>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the content in the ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplateSelector PageContentTemplateSelector
        {
            get
            {
                return _pageContentTemplateSelector;
            }
            set
            {
                _pageContentTemplateSelector = value;
            }
        }
                #endregion //PageContentTemplateSelector

                #region PageFooter
        /// <summary>
        /// Gets or sets the content of the footer used inside the ReportPagePresenter element.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property is used to set the content of the footer used inside the ReportPagePresenter element, in the Infragistics.Wpf.Reporting assembly.   
        /// </p>
        /// </remarks>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("Gets or sets the content of the footer used inside the ReportPagePresenter element.")]
        //[Category("Data")]
        [DefaultValue(null)]
        public object PageFooter
        {
            get
            {
                return _pageFooter;
            }
            set
            {
                _pageFooter = value;
            }
        }
                #endregion

                #region PageFooterTemplate
        /// <summary>
        /// The template used to display the footer content of ReportPagePresenter control.
        /// </summary>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the footer content of ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplate PageFooterTemplate
        {
            get
            {
                return _pageFooterTemplate;
            }
            set
            {
                _pageFooterTemplate = value;
            }
        }
                #endregion //PageFooterTemplate

                #region PageFooterTemplateSelector
        /// <summary>
        /// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the footer in the ReportPagePresenter control.
        /// </summary>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the footer in the ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplateSelector PageFooterTemplateSelector
        {
            get
            {
                return _pageFooterTemplateSelector;
            }
            set
            {
                _pageFooterTemplateSelector = value;
            }
        }
                #endregion //PageFooterTemplateSelector

                #region PageHeader
        /// <summary>
        /// Gets or sets the content of the header used inside the ReportPagePresenter element.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property is used to set the content of the header used inside the ReportPagePresenter element, in the Infragistics.Wpf.Reporting assembly.   
        /// </p>
        /// </remarks>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("Gets or sets the content of the header used inside the ReportPagePresenter element.")]
        //[Category("Data")]
        [DefaultValue(null)]
        public object PageHeader
        {
            get
            {
                return _pageHeader;
            }
            set
            {
                _pageHeader = value;
            }
        }
        #endregion //PageHeader

                #region PageHeaderTemplate
        /// <summary>
        /// The template used to display the header content of ReportPagePresenter control.
        /// </summary>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the header content of ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplate PageHeaderTemplate
        {
            get
            {
                return _pageHeaderTemplate;
            }
            set
            {
                _pageHeaderTemplate = value;
            }
        }
                #endregion //PageHeaderTemplate

                #region PageHeaderTemplateSelector
        /// <summary>
        /// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the header in the ReportPagePresenter control.
        /// </summary>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the header in the ReportPagePresenter control.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public DataTemplateSelector PageHeaderTemplateSelector
        {
            get
            {
                return _pageHeaderTemplateSelector;
            }
            set
            {
                _pageHeaderTemplateSelector = value;
            }
        }
                #endregion // PageHeaderTemplateSelector

                #region PageImageableAreaExtent

        /// <summary>
        /// The size of the imageable area that the printer will support.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this value is only valid during a report generation or export operation. Also it doesn't take into account the page orientation.</para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(false)]
        [Browsable(false)]
        public Size PageImageableAreaExtent
        {
            get
            {
                // JJD 11/24/09 - TFS25026
                // Call the internal helper method which will initialize the origin
                Size pageSize = this.GetPageSizeInternal();
                return this._imageableAreaExtent;
            }
        }

                #endregion //PageImageableAreaExtent	

                #region PageOrigin

        /// <summary>
        /// The origin of the page i.e. offset to allow for the capabilities of the printer.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this value is only valid during a report generation or export operation. Also it doesn't take into account the page orientation.</para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(false)]
        [Browsable(false)]
        public Vector PageOrigin
        {
            get
            {
                // JJD 11/24/09 - TFS25026
                // Call the internal helper method which will initialize the origin
                Size pageSize = this.GetPageSizeInternal();
                return this._origin;
            }
        }

                #endregion //PageOrigin	

                #region PagePresenterStyle
        /// <summary>
        /// Gets or sets the style for the ReportPagePresenter
        /// </summary>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        //[Description("Gets or sets the style for the ReportPagePresenter.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        public Style PagePresenterStyle
        {
            get
            {
                return _pagePresenterStyle;
            }
            set
            {
                _pagePresenterStyle = value;
            }
        }
                #endregion //PagePresenterStyle

                // JJD 9/109 - TFS19395 - added
                #region PageSizeResolved

        /// <summary>
        /// The actual size of the page used during a report generation or export operation (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this value is only valid during a report generation or export operation. Also it doesn't take into account the page orientation.</para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(false)]
        [Browsable(false)]
        virtual public Size PageSizeResolved
        {
            get 
            {
                // JJD 11/24/09 - TFS25026
                // Moved logic to helper method
                return this.GetPageSizeInternal();
            }
        }

                #endregion //PageSizeResolved	

                #region ReportSettings
        /// <summary>
        /// Gets the object that contains the settings for the entire Report (read-only)
        /// </summary>
        /// <remarks>
        /// <p class="body">This is an abstract property and must be overriden in derived classes to provide the settings object for report.</p>
        /// </remarks>
        //[Description("Gets the settings for the Report.")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public abstract ReportSettings ReportSettings { get; }

            #endregion //ReportSettings

            #endregion //Public Properties
    
        #endregion

        #region Methods

			// JJD 3/25/11 - TFS70336 - Added
			#region ClearCachedOriginAndSize

		/// <summary>
		/// Clears the cached orign and image area sizes so they can be re-cacluated the next time they are asked for
		/// </summary>
		protected void ClearCachedOriginAndSize()
		{
			this._origin = new Vector();
			this._imageableAreaExtent = Size.Empty;
		}

			#endregion //ClearCachedOriginAndSize	
    
            // JJD 9/109 - TFS19395 - added
            #region GetPageSizeFromQueue

        // JJD 11/24/09 - TFS25026
        // Added origin and imageableArea out params
        //private static Size GetPageSizeFromQueue(PrintQueue queue)
        private static Size GetPageSizeFromQueue(PrintQueue queue, out Vector origin, out Size imageableAreaExtent)
        {
            if (queue == null)
            {
                // JJD 11/24/09 - TFS25026
                // set the origin and imageableArea out params
                origin = new Vector();
                imageableAreaExtent = Size.Empty;
                return Size.Empty;
            }

            // get the print ticket
			// JJD 07/18/11 - TFS81426
			// Try to get the UserPrintTicket to pick up any specific settings
            //PrintTicket ticket = queue.DefaultPrintTicket;
            PrintTicket ticket = queue.UserPrintTicket;

			if (ticket == null)
				ticket = queue.DefaultPrintTicket;

            // JJD 11/24/09 - TFS25026
            // Get the PrintCapabilities
			// MD 6/29/11 - TFS73145
			// Asking for the PageImageableArea could cause a PrintQueueException if there is an error with the print driver,
			// so we should catch that here. Also, we should cache the PageImageableArea once instead of asking for it multiple times.
            //PrintCapabilities pc;
			//if (ticket != null)
			//    pc = queue.GetPrintCapabilities(ticket);
			//else
			//    pc = queue.GetPrintCapabilities();
			PageImageableArea pageImageableArea = null;
			try
			{
				PrintCapabilities pc;

				if (ticket != null)
					pc = queue.GetPrintCapabilities(ticket);
				else
					pc = queue.GetPrintCapabilities();

				pageImageableArea = pc.PageImageableArea;
			}
			catch (PrintQueueException) { }

			// MD 6/29/11 - TFS73145
			// Check if we have a cached PageImageableArea instead, because that is all we really need.
            //if (pc != null)
			if (pageImageableArea != null)
            {
                // JJD 11/24/09 - TFS25026
                // set the origin out param
				// MD 6/29/11 - TFS73145
				// Use the cached PageImageableArea.
                //origin = new Vector(pc.PageImageableArea.OriginWidth, pc.PageImageableArea.OriginHeight);
				origin = new Vector(pageImageableArea.OriginWidth, pageImageableArea.OriginHeight);

                // JJD 11/24/09 - TFS25026
                // set the imageableArea out param
				// MD 6/29/11 - TFS73145
				// Use the cached PageImageableArea.
                //imageableAreaExtent = new Size(pc.PageImageableArea.ExtentWidth, pc.PageImageableArea.ExtentHeight);
				imageableAreaExtent = new Size(pageImageableArea.ExtentWidth, pageImageableArea.ExtentHeight);
            }
            else
            {
                origin = new Vector();
                imageableAreaExtent = Size.Empty;
            }

            if (ticket != null)
            {
                PageMediaSize pageMediaSize = ticket.PageMediaSize;

                // see if the media size is set
                if (pageMediaSize.Height.HasValue &&
                     pageMediaSize.Width.HasValue)
                {
                    // JJD 11/24/09 - TFS25026
                    // Return the smaller of the media size and the ImageableAreaExtent
                    //return new Size(pageMediaSize.Width.Value, pageMediaSize.Height.Value);
					if (imageableAreaExtent.IsEmpty)
						return new Size(pageMediaSize.Width.Value, pageMediaSize.Height.Value);
					else
					{
						// JJD 10/28/10 - TFS57489
						// If the media is portrait and the PageImageableArea size is landsccpe (or vice versa)
						// then flip the media area size to conform so we don't restrict the page size
						// improperly below
						//return new Size(Math.Min(pc.PageImageableArea.ExtentWidth, pageMediaSize.Width.Value),
						//            Math.Min(pc.PageImageableArea.ExtentHeight, pageMediaSize.Height.Value));
						Size mediaSize = new Size( pageMediaSize.Width.Value, pageMediaSize.Height.Value);

						// MD 6/29/11 - TFS73145
						// Use the cached PageImageableArea.
						//Size printableAreaSize = new Size(pc.PageImageableArea.ExtentWidth, pc.PageImageableArea.ExtentHeight);
						Size printableAreaSize = new Size(pageImageableArea.ExtentWidth, pageImageableArea.ExtentHeight);
						
						if ((mediaSize.Width > mediaSize.Height) !=
							 (printableAreaSize.Width > printableAreaSize.Height))
						{
							double holdWidth = mediaSize.Width;
							mediaSize.Width = mediaSize.Height;
							mediaSize.Height = holdWidth;
						}

						return new Size(Math.Min(printableAreaSize.Width, mediaSize.Width),
										Math.Min(printableAreaSize.Height, mediaSize.Height));
					}
                }
            }

            // JJD 11/24/09 - TFS25026
            // Return the ImageableAreaExtent
			// MD 6/29/11 - TFS73145
			// Use the cached PageImageableArea.
			//if (pc != null )
			//    return new Size(pc.PageImageableArea.ExtentWidth, pc.PageImageableArea.ExtentHeight);
			if (pageImageableArea != null)
				return new Size(pageImageableArea.ExtentWidth, pageImageableArea.ExtentHeight);

            return Size.Empty;
        }

            #endregion //GetPageSizeFromQueue

            // JJD 11/24/09 - TFS25026 - added
            #region GetPageSizeInternal

        // JJD 11/24/09 - TFS25026 
        // Re-factored code into helper method
        private Size GetPageSizeInternal()
        {
			// JJD 3/25/11 - TFS70336 
			// Moved from below
			ReportSettings settings = this.ReportSettings;
			ReportSettings oldSettings = Utilities.GetWeakReferenceTargetSafe(_lastSettings) as ReportSettings;

			// JJD 3/25/11 - TFS70336 
			// If the settings object or its version number have changed set
			// _imageableAreaExtent empty so we will re-calcualte it below
			if (settings != oldSettings ||
				 (settings != null && settings.PageMetricsVersion != _pageMetricsVersion))
				_imageableAreaExtent = Size.Empty;
			
			// JJD 3/25/11 - TFS70336 - Optimization
			// If we have a cached value return it
			if (!_imageableAreaExtent.IsEmpty)
				return _imageableAreaExtent;

			// JJD 3/25/11 - TFS70336 - Optimization
			// cache the settings object and its current version number
			_lastSettings = settings != null ? new WeakReference(settings) : null;
			_pageMetricsVersion = settings != null ? settings.PageMetricsVersion : 0;

            // JJD 11/24/09 - TFS25026 
            // reset the origin value
            this._origin = new Vector();
            this._imageableAreaExtent = Size.Empty;

            Size pageSizeFromQueue = Size.Empty;

			// JJD 3/25/11 - TFS70336 
			// Moved above
            //ReportSettings settings = this.ReportSettings;

            // check the accessFailed flag so we don't keep trying when we
            // don't have access rights
            if (!this._accessFailed)
            {
                try
                {
                    // get the default print queue
                    PrintQueue queue = settings != null ? settings.PrintQueue : null;

                    if (queue != null)
                    {
                        // JJD 11/24/09 - TFS25026
                        // Added origin and imageableAreaExtent out params
                        pageSizeFromQueue = GetPageSizeFromQueue(queue, out this._origin, out this._imageableAreaExtent);
                    }

                    if (queue == null || pageSizeFromQueue.IsEmpty)
                    {
                        // JJD 11/24/09 - TFS25026
                        // Added origin and imageableAreaExtent out params
                        pageSizeFromQueue = GetPageSizeFromQueue(LocalPrintServer.GetDefaultPrintQueue(), out this._origin, out this._imageableAreaExtent);
                    }
                }
                catch (Exception)
                {
                    // if an exception was thrown set a flag so we don't try again the
                    // next time someone asks for this property
                    this._accessFailed = true;
                }
            }

            Size pageSize = Size.Empty;

            if (settings != null)
            {
                // get the page size 
                pageSize = settings.PageSize;

                // Check to see if the settings contain a valid size
                if (pageSize.IsEmpty ||
                    pageSize.Width < 1 ||
                    pageSize.Height < 1)
                {
                    pageSize = pageSizeFromQueue;
                }
                else
                {
                    // JJD 11/24/09 - TFS25026
                    // Make sure the size doesnt exceed the imageable area extent
                    if (pageSizeFromQueue.IsEmpty == false &&
                         pageSizeFromQueue.Width > 0 &&
                         pageSizeFromQueue.Height > 0)
                    {
                        pageSize.Width = Math.Min(pageSize.Width, pageSizeFromQueue.Width);
                        pageSize.Height = Math.Min(pageSize.Height, pageSizeFromQueue.Height);
                    }
                }
            }
            else
            {
                pageSize = pageSizeFromQueue;
            }

            // if we still don't have a size at this point fall back to using
            // 8.5 x 11 at 96 dpi
            if (pageSize.IsEmpty ||
                pageSize.Width < 1 ||
                pageSize.Height < 1)
            {
                pageSize = new Size(816, 1056);
            }

            return pageSize;
        }

            #endregion //GetPageSizeInternal	
    
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