using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;

namespace Infragistics.Windows.Reporting 
{
    /// <summary>
    /// Abstract base class that represents a section in a report. 
    /// </summary>
    /// <remarks>
    /// <p class="body">The class exposes properties that relate to a specific section in a report and are used to override any report-wide default settings specified on the <see cref="ReportBase"/>.</p>
    /// <para class="body">The Report class, derived from <see cref="ReportBase"/>, is in the Infragistics.Wpf.Reporting assembly and it exposes a 
    /// collection of ReportSections. Each section can represent 1 or more pages in the report. For example the derived class EmbeddedVisualReportSection, also in the Infragistics.Wpf.Reporting assembly, 
    /// will accept in its constructor an element that implements either the <see cref="IEmbeddedVisualPaginator"/> or <see cref="IEmbeddedVisualPaginatorFactory"/> interface 
    /// to support printing multiple pages. This is how the XamDataGrid supports printing.</para>
    /// <para class="note"><b>Note:</b> each section in the report starts on a new page. For example, if you created a Report with 3 EmbeddedVisualReportSections, one with a XamDataGrid and 2 others with 
    /// simple elements, each section would start on a new page even though there might have been available space on the last page from the previous section.</para>
    /// </remarks>
    /// <seealso cref="ReportBase"/>
    /// <seealso cref="IEmbeddedVisualPaginator"/>
    /// <seealso cref="IEmbeddedVisualPaginatorFactory"/>
    public abstract class ReportSection : FrameworkContentElement
    {

        #region Member Variables

        private ReportBase _report;
        private object _pageFooter;
        private object _pageHeader;
        private DataTemplateSelector _pageHeaderTemplateSelector;
        private DataTemplateSelector _pageContentTemplateSelector;
        private DataTemplateSelector _pageFooterTemplateSelector;
        private DataTemplate _pageFooterTemplate;
        private DataTemplate _pageContentTemplate;
        private DataTemplate _pageHeaderTemplate;
        private Style _pagePresenterStyle;
 
        #endregion //Member Variables

        #region Constructors

        /// <summary>
        /// Initializes member variables
        /// </summary>
        public ReportSection()
        {
            // set the inherits readonly property so all elements in the logical tree can access it
            this.SetValue(IsInReportPropertyKey, KnownBoxes.TrueBox);
        }

        #endregion //Constructors


        #region Properties
       
            #region Public properties

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
        public abstract int GeneratedPageCount
        {
            get;
        }

                #endregion //GeneratedPageCount

                #region IsEndReached
        /// <summary>
        /// Returns whether the report section is on the last page (read-only).
        /// </summary>
        /// <remarks>
        /// <p class="note">This property only has meaning during a pagination.</p>
        /// </remarks>
        //[Description("Returns whether the report section is on the last page (read-only).")]
        //[Category("Reporting Properties")] // Behavior
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public abstract bool IsEndReached
        {
            get;
        }
                #endregion
                
                #region IsInReport readonly attached inherits

        private static readonly DependencyPropertyKey IsInReportPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsInReport",
            typeof(bool), typeof(ReportSection), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the "IsInReport" attached readonly dependency property
        /// </summary>
        /// <remarks>
        /// <p class="body">Since this is an attached inherited property it can be useful inside a template of any element to trigger changes to its look for printing.</p>
        /// </remarks>
        /// <seealso cref="GetIsInReport"/>
        public static readonly DependencyProperty IsInReportProperty =
            IsInReportPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the value of the "IsInReport" attached readonly property
        /// </summary>
        /// <remarks>
        /// <p class="body">Since this is an attached inherited property it can be useful inside a template of any element to trigger changes to its look for printing.</p>
        /// </remarks>
        /// <param name="d">The target object.</param>
        /// <seealso cref="IsInReportProperty"/>
        //[Description("Gets the value of the 'IsInReport' attached readonly property.")]
        //[Category("Reporting Properties")] // Behavior
        [Bindable(true)]
        [ReadOnly(true)]
        public static bool GetIsInReport(DependencyObject d)
        {
            return (bool)d.GetValue(ReportSection.IsInReportProperty);
        }

        /// <summary>
        /// Sets the value of the "IsInReport" attached readonly property
        /// </summary>
        /// <param name="d">The target object.</param>
        /// <param name="value">Value of the property</param>
        /// <seealso cref="IsInReportProperty"/>
        protected static void SetIsInReport(DependencyObject d, bool value)
        {
            d.SetValue(ReportSection.IsInReportPropertyKey, value);
        }

            #endregion //IsInReport readonly attached inherits

                #region LogicalPageNumber
        /// <summary>
        /// The logical page number of the current page being printed within this section (read-only). 
        /// </summary>
        /// <value>A 1-based integer representing the logical page number of the current page being printed within this section.</value>
        /// <remarks>
        /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
        /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="PhysicalPageNumber"/>.</p>
        /// </remarks>
        /// <seealso cref="LogicalPagePartNumber"/>
        /// <seealso cref="PhysicalPageNumber"/>
        /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSettings.PagePrintOrder"/>
        //[Description("The logical page number of the current page being printed within this section (read-only).")]
        //[Category("Reporting Properties")] // Data
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public abstract int LogicalPageNumber
        {
            get;
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
        /// <seealso cref="ReportSection.LogicalPageNumber"/>
        /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
        /// <seealso cref="ReportSettings.PagePrintOrder"/>
        //[Description("The logical page part number of the current page being printed within this section (read-only).")]
        //[Category("Reporting Properties")] // Data
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public abstract int LogicalPagePartNumber
        {
            get;
        }

                #endregion //LogicalPagePartNumber
      
                #region PageContentTemplate
        /// <summary>
        /// The template used to display the content of ReportPagePresenter control.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageContentTemplate"/>.</p>
        /// </remarks>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the content of ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageContentTemplateSelector"/>.</p>
        /// </remarks>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the content in the ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageFooter"/>.
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
        //[Category("Reporting Properties")] // Data
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
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageFooterTemplate"/>.</p>
        /// </remarks>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the footer content of ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageFooterTemplateSelector"/>.</p>
        /// </remarks>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the footer in the ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageHeader"/>.</p>
        /// </remarks>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("Gets or sets the content of the header used inside the ReportPagePresenter element.")]
        //[Category("Reporting Properties")] // Data
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
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageHeaderTemplate"/>.</p>
        /// </remarks>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("The template used to display the header content of ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PageHeaderTemplateSelector"/>.</p>
        /// </remarks>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        /// <seealso cref="PagePresenterStyle"/>
        //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the header in the ReportPagePresenter control.")]
        //[Category("Reporting Properties")] // Appearance
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

                #region PagePresenterStyle
        /// <summary>
        /// Gets or sets the style for the ReportPagePresenter
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> this property takes precedence over the setting on the <see cref="ReportBase"/>'s <see cref="ReportBase.PagePresenterStyle"/>.</p>
        /// </remarks>
        /// <seealso cref="PageHeader"/>
        /// <seealso cref="PageHeaderTemplate"/>
        /// <seealso cref="PageHeaderTemplateSelector"/>
        /// <seealso cref="PageFooter"/>
        /// <seealso cref="PageFooterTemplate"/>
        /// <seealso cref="PageFooterTemplateSelector"/>
        /// <seealso cref="PageContentTemplate"/>
        /// <seealso cref="PageContentTemplateSelector"/>
        //[Description("Gets or sets the style for the ReportPagePresenter.")]
        //[Category("Reporting Properties")] // Appearance
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
        //[Description("The physical page number of the current page being printed within this section (read-only).")]
        //[Category("Reporting Properties")] // Data
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public abstract int PhysicalPageNumber
        {
            get;
        }

                #endregion //PhysicalPageNumber
        
                #region Report
        /// <summary>
        /// The associated report (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is initialized at the begiining of the first pagination. Until then it returns null.</para>
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Browsable(false)]
        public ReportBase Report
        {
            get { return _report; }
        }
                #endregion

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

        #region Public Methods

            #region GetPage
        /// <summary>
        /// Gets the page from the section
        /// </summary>
        /// <param name="pageNumber">Zero-based physical page number relative to the beginning of the section.</param>
        /// <returns>A DocumentPage containing the page or null if past end of section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The page number parameter is less than zero.</exception>
        public abstract DocumentPage GetPage(int pageNumber);

            #endregion //GetPage	
    
            #region OnBeginPagination
        /// <summary>
        /// Called when a pagination operation is about to begin.
        /// </summary>
        /// <param name="report">The report that will be printed.</param>
        /// <remarks>
        /// <p class="body">This method is called once at the beginning of a pagination operation.</p>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If report is null</exception>
        /// <seealso cref="OnEndPagination"/>
        public virtual void OnBeginPagination(ReportBase report) 
        {
            Utilities.ThrowIfNull(report, "report");

            this._report = report;
        }

            #endregion //OnBeginPagination	
    
            #region OnEndPagination

        /// <summary>
        /// Called after a pagination operation has ended.
        /// </summary>
        /// <seealso cref="OnBeginPagination"/>
        public virtual void OnEndPagination() { }

            #endregion //OnEndPagination	
    
        #endregion //Public Methods

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