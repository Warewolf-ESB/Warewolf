using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Reporting
{
    /// <summary>
    /// Represents one page in a <see cref="Report"/>
    /// </summary>
    /// <remarks>
    /// <p class="body">An instance of this element is created internally by each <see cref="ReportSection"/> to represent a page in the report.
    /// It's Margin property is set based on the <see cref="Report.ReportSettings"/>'s <see cref="ReportSettings.Margin"/>. 
    /// </p>
    /// <p class="body">Styling, templates, header content and footer content can be specified for the entire report by setting the following properties on <see cref="ReportBase"/>:
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
    /// <para class="note"><b>Note:</b> if an object implements the <see cref="IEmbeddedVisualPaginator"/> or 
    /// <see cref="IEmbeddedVisualPaginatorFactory"/> interface, e.g. a XamDataGrid, and it is inside an <see cref="EmbeddedVisualReportSection"/> as 
    /// each page is returned from <see cref="EmbeddedVisualReportSection.GetPage(int)"/> the DataContext of the ReportPagePresenter will be set
    /// to value returned from <see cref="IEmbeddedVisualPaginator.CurrentPageDataContext"/>. In the case of a XamDataGrid this will be the first Record on the page.</para>
    /// </remarks>
    [TemplatePart(Name = "PART_Header", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Footer", Type = typeof(ContentPresenter))]
    //[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ReportPagePresenter : HeaderedContentControl
    {
        #region Member Variables
        
        private ContentPresenter _partContent;
        private StyleSelectorHelper _styleSelectorHelper;
        private ReportSection _section;

        // JJD 11/24/09 - TFS25026 - added
        private Vector _pageOriginInternal;

        #endregion //Member Variables

        #region Constructors
        static ReportPagePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ReportPagePresenter), new FrameworkPropertyMetadata(typeof(ReportPagePresenter)));

            ContentProperty.OverrideMetadata(typeof(ReportPagePresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentChanged)));

			// AS 4/23/09 TFS17031
			Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ReportPagePresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(ReportPagePresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

            // JJD 11/24/09 - TFS25026
            // Added Coerce callback for Margin property
            Control.MarginProperty.OverrideMetadata(typeof(ReportPagePresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMargin)));
		}

        /// <summary>
        /// Creates an instance of the <see cref="ReportPagePresenter"/> class
        /// </summary>
        public ReportPagePresenter()
        {
            _styleSelectorHelper = new StyleSelectorHelper(this);
        }
        /// <summary>
        /// Creates an instance of the <see cref="ReportPagePresenter"/> class
        /// </summary>
        /// <param name="section">The owing section</param>
        public ReportPagePresenter(ReportSection section) : this()
        {
            this._section = section;
        }
        #endregion //Constructors

        #region Properties

            #region Public  

                #region Footer

            /// <summary>
            /// Identifies the <see cref="Footer"/> dependency property
            /// </summary>
            public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer",
                typeof(object), typeof(ReportPagePresenter), new FrameworkPropertyMetadata());

            /// <summary>
            /// Gets or sets the content of the footer.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> the <see cref="ReportSection"/> uses its <see cref="ReportSection.PageFooter"/> value to 
            /// set this property. If that is null set it will use the value of the <see cref="Report"/>'s <see cref="ReportBase.PageFooter"/> property.</para>
            /// </remarks>
            /// <seealso cref="ReportSection.PageFooter"/>
            /// <seealso cref="FooterTemplate"/>
            /// <seealso cref="FooterTemplateSelector"/>
            //[Description("Gets or sets the content of the footer.")]
            //[Category("Reporting Properties")] // Data
            [Bindable(true)]
            public Object Footer
            {
                get
                {
                    return (object)this.GetValue(ReportPagePresenter.FooterProperty);
                }
                set
                {
                    this.SetValue(ReportPagePresenter.FooterProperty, value);
                }
            }

                #endregion //FooterContent

                #region FooterStringFormat

            /// <summary>
            /// Identifies the <see cref="FooterStringFormat"/> dependency property
            /// </summary>
            public static readonly DependencyProperty FooterStringFormatProperty = DependencyProperty.Register("FooterStringFormat",
                typeof(string), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(null));

            /// <summary>
            /// Returns/sets a composite string that specifies how to format the Footer property if it is displayed as a string. 
            /// </summary>
            /// <remarks>
            /// <para class="body">FooterStringFormat can be a predefined, composite, or custom string format. For more information see the documentation on the corresponding HeaderFormatString property of the <see cref="System.Windows.Controls.HeaderedContentControl"/>.</para>
            /// <p class="note"><b>Note:</b>If you are using a version of Microsoft's .Net framework that is before 3.5-SP1 or if you set the <see cref="FooterTemplate"/> or <see cref="FooterTemplateSelector"/> property of a <see cref="ReportPagePresenter"/>, then the FooterStringFormat property is ignored.
            /// </p>
            /// </remarks>
            /// <seealso cref="FooterStringFormatProperty"/>
            //[Description("Returns/sets a composite string that specifies how to format the Footer property if it is displayed as a string.")]
            //[Category("Reporting Properties")] // Content
            [Bindable(true)]
            public string FooterStringFormat
            {
                get
                {
                    return (string)this.GetValue(ReportPagePresenter.FooterStringFormatProperty);
                }
                set
                {
                    this.SetValue(ReportPagePresenter.FooterStringFormatProperty, value);
                }
            }

                #endregion //FooterStringFormat

                #region FooterTemplate

            /// <summary>
            /// Identifies the <see cref="FooterTemplate"/> dependency property
            /// </summary>
            public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register("FooterTemplate",
                typeof(DataTemplate), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(null));

            /// <summary>
            /// The template used to display the footer content.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> the <see cref="ReportSection"/> uses its <see cref="ReportSection.PageFooterTemplate"/> value to 
            /// set this property. If that is null set it will use the value of the <see cref="Report"/>'s <see cref="ReportBase.PageFooterTemplate"/> property.</para>
            /// </remarks>
            /// <seealso cref="ReportSection.PageFooterTemplate"/>
            /// <seealso cref="Footer"/>
            /// <seealso cref="FooterTemplateSelector"/>
            //[Description("The template used to display the footer content.")]
            //[Category("Reporting Properties")] // Appearance
            [Bindable(true)]
            public DataTemplate FooterTemplate
            {
                get
                {
                    return (DataTemplate)this.GetValue(ReportPagePresenter.FooterTemplateProperty);
                }
                set
                {
                    this.SetValue(ReportPagePresenter.FooterTemplateProperty, value);
                }
            }

                #endregion //FooterTemplate

                #region FooterTemplateSelector

            /// <summary>
            /// Identifies the <see cref="FooterTemplateSelector "/> dependency property
            /// </summary>
			// JJD 5/11/12 - TFS111524
			// Removed trailing space from call to  DependencyProperty.Register
            //public static readonly DependencyProperty FooterTemplateSelectorProperty = DependencyProperty.Register("FooterTemplateSelector ",
            public static readonly DependencyProperty FooterTemplateSelectorProperty = DependencyProperty.Register("FooterTemplateSelector",
                typeof(DataTemplateSelector), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(null));

            /// <summary>
            /// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the footer.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> the <see cref="ReportSection"/> uses its <see cref="ReportSection.PageFooterTemplateSelector"/> value to 
            /// set this property. If that is null set it will use the value of the <see cref="Report"/>'s <see cref="ReportBase.PageFooterTemplateSelector"/> property.</para>
            /// </remarks>
            /// <seealso cref="ReportSection.PageFooterTemplateSelector"/>
            /// <seealso cref="Footer"/>
            /// <seealso cref="FooterTemplate"/>
            //[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the footer.")]
            //[Category("Reporting Properties")] // Appearance
            [Bindable(true)]
            public DataTemplateSelector FooterTemplateSelector 
            {
                get
                {
                    return (DataTemplateSelector)this.GetValue(ReportPagePresenter.FooterTemplateSelectorProperty);
                }
                set
                {
                    this.SetValue(ReportPagePresenter.FooterTemplateSelectorProperty, value);
                }
            }

                #endregion //FooterTemplateSelector 
    	
                #region Section
            /// <summary>
            /// The owning <see cref="ReportSection"/> (read-only)
            /// </summary>
            /// <seealso cref="ReportSection"/>
            /// <seealso cref="EmbeddedVisualReportSection"/>
            //[Description("The owning ReportSection (read-only.")]
            //[Category("Reporting Properties")] // Behavior
            [ ReadOnly(true)]
            [ DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public ReportSection Section
            {
                get { return _section; }
            }
                #endregion //Section

                #region LogicalPageNumber

            private static readonly DependencyPropertyKey LogicalPageNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("LogicalPageNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="LogicalPageNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty LogicalPageNumberProperty =
                LogicalPageNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The logical page number of this page within the report (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the logical page number of this page within the report, i.e. across sections</value>
            /// <remarks>
            /// <p class="body">If there are multiple sections within a report this property continues on from the section to section.</p>
            /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="PhysicalPageNumber"/>.</p>
            /// </remarks>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="PhysicalPageNumber"/>
            /// <seealso cref="SectionLogicalPageNumber"/>
            /// <seealso cref="SectionLogicalPagePartNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The logical page number of this page within the report (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int LogicalPageNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.LogicalPageNumberProperty);
                }
            }

                #endregion //LogicalPageNumber

                #region LogicalPagePartNumber

            private static readonly DependencyPropertyKey LogicalPagePartNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("LogicalPagePartNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="LogicalPagePartNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty LogicalPagePartNumberProperty =
                LogicalPagePartNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The logical page part number of this page (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the logical page part number of this page.</value>
            /// <remarks>
            /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
            /// </p>
            /// </remarks>
            /// <seealso cref="LogicalPageNumber"/>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="PhysicalPageNumber"/>
            /// <seealso cref="SectionLogicalPageNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The logical page part number of this page (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int LogicalPagePartNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.LogicalPagePartNumberProperty);
                }
            }

                #endregion //LogicalPagePartNumber

                #region PhysicalPageNumber

            private static readonly DependencyPropertyKey PhysicalPageNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("PhysicalPageNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="PhysicalPageNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty PhysicalPageNumberProperty =
                PhysicalPageNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The physical page number of this page within the report (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the physical page number of this page within the report.</value>
            /// <remarks>
            /// <p class="body">If there are multiple sections within a report this property continues on from the section to section.</p>
            /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="LogicalPageNumber"/>.</p>
            /// </remarks>
            /// <seealso cref="LogicalPageNumber"/>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="SectionLogicalPageNumber"/>
            /// <seealso cref="SectionLogicalPagePartNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The physical page number of this page within the report (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int PhysicalPageNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.PhysicalPageNumberProperty);
                }
            }

                #endregion //PhysicalPageNumber

                #region SectionLogicalPageNumber

            private static readonly DependencyPropertyKey SectionLogicalPageNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("SectionLogicalPageNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="SectionLogicalPageNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty SectionLogicalPageNumberProperty =
                SectionLogicalPageNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The logical page number of this page within this section (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the logical page number of this page within this section.</value>
            /// <remarks>
            /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
            /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="SectionPhysicalPageNumber"/>.</p>
            /// </remarks>
            /// <seealso cref="LogicalPageNumber"/>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="PhysicalPageNumber"/>
            /// <seealso cref="SectionLogicalPagePartNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The logical page number of this page within this section (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int SectionLogicalPageNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.SectionLogicalPageNumberProperty);
                }
            }

            #endregion //SectionLogicalPageNumber

                #region SectionLogicalPagePartNumber

            private static readonly DependencyPropertyKey SectionLogicalPagePartNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("SectionLogicalPagePartNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="SectionLogicalPagePartNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty SectionLogicalPagePartNumberProperty =
                SectionLogicalPagePartNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The logical page part number of this page (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the logical page part number of this page.</value>
            /// <remarks>
            /// <p class="note"><b>Note:</b> if the logical page is not wide enough to span multiple pages or the <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return 1.
            /// </p>
            /// </remarks>
            /// <seealso cref="LogicalPageNumber"/>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="PhysicalPageNumber"/>
            /// <seealso cref="SectionLogicalPageNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The logical page part number of this page (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int SectionLogicalPagePartNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.SectionLogicalPagePartNumberProperty);
                }
            }

            #endregion //SectionLogicalPagePartNumber

                #region SectionPhysicalPageNumber

            private static readonly DependencyPropertyKey SectionPhysicalPageNumberPropertyKey =
                DependencyProperty.RegisterReadOnly("SectionPhysicalPageNumber",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0));

            /// <summary>
            /// Identifies the <see cref="SectionPhysicalPageNumber"/> dependency property
            /// </summary>
            public static readonly DependencyProperty SectionPhysicalPageNumberProperty =
                SectionPhysicalPageNumberPropertyKey.DependencyProperty;

            /// <summary>
            /// The physical page number of this page within this section (read-only). 
            /// </summary>
            /// <value>A 1-based integer representing the physical page number of this page within this section.</value>
            /// <remarks>
            /// <p class="body">If there are multiple sections within a report this property restarts at 1 within each section.</p>
            /// <p class="note"><b>Note:</b> if <see cref="ReportSettings.HorizontalPaginationMode"/> property is set to 'Scale' then this property will return the same value as <see cref="LogicalPageNumber"/>.</p>
            /// </remarks>
            /// <seealso cref="LogicalPageNumber"/>
            /// <seealso cref="LogicalPagePartNumber"/>
            /// <seealso cref="PhysicalPageNumber"/>
            /// <seealso cref="SectionLogicalPagePartNumber"/>
            /// <seealso cref="SectionPhysicalPageNumber"/>
            /// <seealso cref="ReportSettings.HorizontalPaginationMode"/>
            /// <seealso cref="ReportSettings.PagePrintOrder"/>
            //[Description("The physical page number of this page within this section (read-only).")]
            //[Category("Reporting Properties")] // Behavior
            [Bindable(true)]
            [ReadOnly(true)]
            public int SectionPhysicalPageNumber
            {
                get
                {
                    return (int)this.GetValue(ReportPagePresenter.SectionPhysicalPageNumberProperty);
                }
            }

            #endregion //SectionPhysicalPageNumber

            #endregion //Public

            #region Internal Properties

        
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

        internal Size ContentAreaAvailableSize
        {
            get
            {
                if (_partContent != null)
                {
                    // JJD 5/20/10 - TFS32438
                    // In case the content's horizontal or vertical alignment is not set to
                    // 'stretch' we need to calculate the available extent based on the explicit 
                    // constraints and the layout slot info
                    //return new Size(_partContent.ActualWidth, _partContent.ActualHeight);

                    Rect layoutSlot = LayoutInformation.GetLayoutSlot(_partContent);

                    Thickness margin = _partContent.Margin;

                    double width = CalculateConstrainedExtent(layoutSlot.Width, 
                                                        _partContent.ActualWidth,
                                                        _partContent.Width,
                                                        _partContent.MinWidth,
                                                        _partContent.MaxWidth,
                                                        margin.Left + margin.Right,
                                                        _partContent.HorizontalAlignment == HorizontalAlignment.Stretch);

                    double height = CalculateConstrainedExtent(layoutSlot.Height,
                                                        _partContent.ActualHeight,
                                                        _partContent.Height,
                                                        _partContent.MinHeight,
                                                        _partContent.MaxHeight,
                                                        margin.Top + margin.Bottom,
                                                        _partContent.VerticalAlignment == VerticalAlignment.Stretch);
                    return new Size(width, height);
                }
                else
                    return new Size(0, 0);
            }
        }

                // JJD 11/24/09 - TFS25026 - added
                #region PageOriginInternal

        internal Vector PageOriginInternal
        {
            get { return this._pageOriginInternal; }
            set
            {
                if (this._pageOriginInternal != value)
                {
                    this._pageOriginInternal = value;
                    this.CoerceValue(MarginProperty);
                }
            }
        }

                #endregion //PageOriginInternal	

            #endregion // Internal Properties

            #region Private Properties

            #region LogicalPageNumberInternal

            private static readonly DependencyProperty LogicalPageNumberInternalProperty = DependencyProperty.Register("LogicalPageNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnLogicalPageNumberInternalChanged)));

            private static void OnLogicalPageNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(LogicalPageNumberPropertyKey, e.NewValue);
            }


            #endregion //LogicalPageNumberInternal

            #region LogicalPagePartNumberInternal

            private static readonly DependencyProperty LogicalPagePartNumberInternalProperty = DependencyProperty.Register("LogicalPagePartNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnLogicalPagePartNumberInternalChanged)));

            private static void OnLogicalPagePartNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(LogicalPagePartNumberPropertyKey, e.NewValue);
            }


            #endregion //LogicalPagePartNumberInternal

            #region PhysicalPageNumberInternal

            private static readonly DependencyProperty PhysicalPageNumberInternalProperty = DependencyProperty.Register("PhysicalPageNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnPhysicalPageNumberInternalChanged)));

            private static void OnPhysicalPageNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(PhysicalPageNumberPropertyKey, e.NewValue);
            }


            #endregion //PhysicalPageNumberInternal

            #region SectionLogicalPageNumberInternal

            private static readonly DependencyProperty SectionLogicalPageNumberInternalProperty = DependencyProperty.Register("SectionLogicalPageNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnSectionLogicalPageNumberInternalChanged)));

            private static void OnSectionLogicalPageNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(SectionLogicalPageNumberPropertyKey, e.NewValue);
            }

            #endregion //SectionLogicalPageNumberInternal

            #region SectionLogicalPagePartNumberInternal

            private static readonly DependencyProperty SectionLogicalPagePartNumberInternalProperty = DependencyProperty.Register("SectionLogicalPagePartNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnSectionLogicalPagePartNumberInternalChanged)));

            private static void OnSectionLogicalPagePartNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(SectionLogicalPagePartNumberPropertyKey, e.NewValue);
            }

            #endregion //SectionLogicalPagePartNumberInternal

            #region SectionPhysicalPageNumberInternal

            private static readonly DependencyProperty SectionPhysicalPageNumberInternalProperty = DependencyProperty.Register("SectionPhysicalPageNumberInternal",
                typeof(int), typeof(ReportPagePresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnSectionPhysicalPageNumberInternalChanged)));

            private static void OnSectionPhysicalPageNumberInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
            {
                ReportPagePresenter rpp = target as ReportPagePresenter;

                // sync up the value of the public read-only property
                if (rpp != null)
                    rpp.SetValue(SectionPhysicalPageNumberPropertyKey, e.NewValue);
            }


            #endregion //SectionPhysicalPageNumberInternal

        #endregion //Private Properties

        #endregion //Properties

        #region Base class overrides

            #region OnApplyTemplate
        /// <summary>
        /// Occurs when the template for the element has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._partContent = base.GetTemplateChild("PART_Content") as ContentPresenter;

            
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

        }
            #endregion //OnApplyTemplate

         #endregion //Base class overrides

        #region Methods

            #region Private Methods

                // JJD 5/20/10 - TFS32438 - added
                #region CalculateConstrainedExtent

        private static double CalculateConstrainedExtent(double layoutSlotExtent, double actualExtent, double explicitExtent, double minExtent, double maxExtent, double marginExtent, bool isStretched)
        {
            // if the element is stretched then the actualExtent should have been calcuklated at this point
            if (isStretched && actualExtent > 0)
                return actualExtent;

            double extent;

            // if an explicit Width/Height was set then use it.
            // Otherwise use the layout slot extent
            if (!double.IsNaN(explicitExtent))
                extent = explicitExtent;
            else
                extent = layoutSlotExtent;

            // constrain the extent if a min has been set
            if (minExtent > 0)
                extent = Math.Max(extent, minExtent);

            // constrain the extent if a max has been set
            if (!double.IsPositiveInfinity(maxExtent))
                extent = Math.Min(maxExtent, extent);

            // adjust for the margins
            extent = Math.Max(extent - marginExtent, 0);

            return extent;
        }

                #endregion //CalculateConstrainedExtent	
    
                #region CoerceMargin

        // JJD 11/24/09 - TFS25026
        // Added Coerce callback for Margin property
        private static object CoerceMargin(DependencyObject target, object value)
        {
            ReportPagePresenter rpp = target as ReportPagePresenter;
            Thickness margin = (Thickness)value;

            // add margin to the left and top to supply the proper offset for the printer
            margin.Left += rpp._pageOriginInternal.X;
            margin.Top += rpp._pageOriginInternal.Y;

            return margin;
        }

                #endregion //CoerceMargin	

                #region OnContentChanged
        private static void OnContentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ReportPagePresenter rpp = target as ReportPagePresenter;

            // JJD 10/14/08
            // We only need to refresh the style if we have content
            //if (rpp != null && e.NewValue != null)
            if (rpp != null && e.NewValue != null)
            {
                ReportSection section = rpp.Section;

                Debug.Assert(section != null, "We should have a section here");

                if (section != null)
                {
                    Report report = section.Report as Report;

                    Debug.Assert(report != null, "We should have a report here");

                    // JJD 10/14/08
                    // Bind the asociated Report properties so we sync up page numbers
                    if (report != null)
                    {
                        rpp.SetBinding(LogicalPageNumberInternalProperty,               Utilities.CreateBindingObject("LogicalPageNumber", BindingMode.OneWay, report));
                        rpp.SetBinding(LogicalPagePartNumberInternalProperty,           Utilities.CreateBindingObject("LogicalPagePartNumber", BindingMode.OneWay, report));
                        rpp.SetBinding(PhysicalPageNumberInternalProperty,              Utilities.CreateBindingObject("PhysicalPageNumber", BindingMode.OneWay, report));
                        rpp.SetBinding(SectionLogicalPageNumberInternalProperty,        Utilities.CreateBindingObject("SectionLogicalPageNumber", BindingMode.OneWay, report));
                        rpp.SetBinding(SectionLogicalPagePartNumberInternalProperty,    Utilities.CreateBindingObject("SectionLogicalPagePartNumber", BindingMode.OneWay, report));
                        rpp.SetBinding(SectionPhysicalPageNumberInternalProperty,       Utilities.CreateBindingObject("SectionPhysicalPageNumber", BindingMode.OneWay, report));
                    }

                }

                rpp._styleSelectorHelper.InvalidateStyle();
            }
        }
            #endregion

            #endregion //Private Methods

        #endregion //Methods

        #region Internal Style selector class

        private class StyleSelectorHelper : StyleSelectorHelperBase
        {
            private ReportPagePresenter _pp;

            internal StyleSelectorHelper(ReportPagePresenter pp)
                : base(pp)
            {
                this._pp = pp;
            }

            public override Style Style
            {
                get
                {
                    if (this._pp == null)
                        return null;

                    ReportSection section = _pp.Section;

                    // JJD 10/14/08 - do a null check
                    if ( section == null )
                        return null;

                    if (section.PagePresenterStyle != null)
                        return section.PagePresenterStyle;

                    ReportBase report = section.Report;

                    // JJD 10/14/08 
                    // Return the setting on the report object
                    if (report != null)
                        return report.PagePresenterStyle;

                    return null;// Application.Current.TryFindResource(_pp.GetType()) as Style;

                }
            }
        }

        #endregion //Internal Style selector class

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