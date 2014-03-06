using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Abstract base class for all report views in <see cref="DataPresenterBase"/> derived controls.
    /// </summary>
    /// <remarks>
    /// <p class="body">ReportViewBase derived objects are used by the <see cref="XamDataGrid"/> to provide settings and defaults that can
    /// query when it provides print generation. This class provides option to control print version of a <see cref="XamDataGrid"/>. 
    /// For example if you have applied a CellValuePresenterStyle to a XamDataGrid, the ReportViewBase class exposes property 
    /// called <see cref="ExcludeCellValuePresenterStyles"/> to exclude this style in printing version of grid. </p>
    /// <p class="note"><b>Note: </b>One ReportViewBase derived view is included in this version of the controls:
    ///		<ul>
    ///			<li><see cref="TabularReportView"/> - Arranges items in a classic grid format with tabular indent for children records.</li>
    ///		</ul>
    /// </p>
    /// </remarks>
    /// <seealso cref="ViewBase"/>
    /// <seealso cref="GridView"/>
    /// <seealso cref="XamDataGrid"/>
	public abstract class ReportViewBase : ViewBase,
        // MBS 7/20/09 - NA9.2 Excel Exporting
        IExportOptions
	{
        #region Private Members
        CellPageSpanStrategy _cellPageSpanStrategy = CellPageSpanStrategy.NextPageFillWithPreviousCell;
        bool _excludeExpandedState;
        bool _excludeRecordVisibility;
        bool _excludeFieldLayoutSettings;
        bool _excludeFieldSettings;
        bool _excludeCellValuePresenterStyles;
        bool _excludeLabelPresenterStyles;
        bool _excludeEditorSettings;
        bool _excludeGroupBySettings;
        bool _excludeSortOrder;
        bool _excludeSummaries;
        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        bool _excludeRecordSizing;
        // JJD 1/16/09 NA 2009 Vol 1 - Record filtering
        bool _excludeRecordFilters;

        private string _theme;
        private ViewBase _uiView;
        #endregion //Private Members	

        #region Base class overrides

            #region HorizontalScrollBarVisibility

        /// <summary>
        /// Returns a value that indicates when the horizontal scrollbar should be shown in this view.
        /// </summary>
        /// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
        protected internal override ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get
            {
                return ScrollBarVisibility.Hidden;
            }
        }

            #endregion //HorizontalScrollBarVisibility	

			// JJD 2/15/12 - TFS101708 - Return value from UI View to pick up any fixed state styling
			#region IsFixedRecordsSupported

		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should allow root records to be fixed at the top or bottom of the UI.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base implementation returns false.
		/// When a record is fixed it's position in the <see cref="ViewableRecordCollection"/> is changed so the the 
		/// record is positioned with other fixed records at the beginning or end of the list.
		/// </p>
		/// </remarks>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="ViewableRecordCollection"/>
		internal protected override bool IsFixedRecordsSupported
		{
			get { return _uiView.IsFixedRecordsSupported; }
		}

			#endregion //IsFixedRecordsSupported
    
            #region VerticalScrollBarVisibility

        /// <summary>
        /// Returns a value that indicates when the vertical scrollbar should be shown in this view.
        /// </summary>
        /// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
        protected internal override ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                return ScrollBarVisibility.Hidden;
            }
        }

            #endregion //VerticalScrollBarVisibility	
    
        #endregion //Base class overrides

        #region Properties

            #region Protected Properties

            #region UiView

        /// <summary>
        /// Returns the view of the ui DataPresenter (read-only).
        /// </summary>
        /// <value>A clone instance of the view from a ui DataPresenter or null.</value>
        /// <remarks class="note">
        /// <b>Note:</b> This property will return null except for the cloned ReportViewBase used during a report pagination. It can be 
        /// used by derived classes to provide default values for certain properties.</remarks>
        protected ViewBase UiView { get { return this._uiView; } }

            #endregion //UiView	

        #endregion //Protected Properties

            #region Public Properties

            #region CellPageSpanStrategy

        /// <summary>
        /// Determines the strategy used during printing when a cell is encountered that will not fit on a page.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> The also applies to field labels as well so that the header area will line up properly.</para>
        /// </remarks>
        //[Description("Determines the strategy used during printing when a cell is encountered that will not fit on a page.")]
        //[Category("Behavior")]
        [DefaultValue(CellPageSpanStrategy.NextPageFillWithPreviousCell)]
        public CellPageSpanStrategy CellPageSpanStrategy
        {
            get
            {
                return this._cellPageSpanStrategy;
            }
            set
            {
                if (value != this._cellPageSpanStrategy)
                {
                    if (!Enum.IsDefined(typeof(CellPageSpanStrategy), value))
                        throw new ArgumentException();

                    this._cellPageSpanStrategy = value;
                }
            }
        }

            #endregion //CellPageSpanStrategy

            #region ExcludeCellValuePresenterStyles
        /// <summary>
        /// Determines if the CellValuePresenterStyle settings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to include the <see cref="FieldSettings.CellValuePresenterStyle"/> of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldSettings.CellValuePresenterStyle"/>
        /// <seealso cref="FieldSettings.LabelPresenterStyle"/>
        //[Description("Determines if the CellValuePresenterStyle settings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeCellValuePresenterStyles
        {
            get { return _excludeCellValuePresenterStyles; }
            set { _excludeCellValuePresenterStyles = value; }
        }
            #endregion //ExcludeCellValuePresenterStyles

            #region ExcludeEditorSettings

        /// <summary>
        /// Determines if the editor related settings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over any Editor releated of the associated ui XamDataGrid 
        /// in the report. These are on the <see cref="FieldSettings"/> object, e.g <see cref="FieldSettings.EditorStyle"/> and <see cref="FieldSettings.EditorType"/>. Default value is false. </p>
        /// </remarks>
        //[Description("Determines if the editor related settings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeEditorSettings
        {
            get { return _excludeEditorSettings; }
            set { _excludeEditorSettings = value; }
        }
            #endregion //ExcludeEditorSettings

            #region ExcludeExpandedState
        /// <summary>
        /// Determines if each record's IsExpanded state from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over each <see cref="Record"/>'s <see cref="Record.IsExpanded"/> state from the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="Record.IsExpanded"/>
        //[Description("Determines if each record's IsExpanded state from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeExpandedState
        {
            get { return _excludeExpandedState; }
            set { _excludeExpandedState = value; }
        }
            #endregion //ExcludeExpandedState

            #region ExcludeFieldLayoutSettings
        /// <summary>
        /// Determines if the FieldLayoutSettings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldLayoutSettings"/> of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayout.Settings"/>
        //[Description("Determines if the FieldLayoutSettings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeFieldLayoutSettings
        {
            get { return _excludeFieldLayoutSettings; }
            set { _excludeFieldLayoutSettings = value; }
        }
        #endregion //ExcludeFieldLayoutSettings

            #region ExcludeFieldSettings
        /// <summary>
        /// Determines if the FieldSettings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldSettings"/> of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase.FieldSettings"/>
        /// <seealso cref="FieldLayout.FieldSettings"/>
        /// <seealso cref="Field.Settings"/>
        //[Description("Determines if the FieldSettings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeFieldSettings 
        {
            get { return _excludeFieldSettings; }
            set { _excludeFieldSettings = value; }
        }
        #endregion //ExcludeFieldSettings

            #region ExcludeGroupBySettings
        /// <summary>
        /// Determines if the groupby settings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the groupby settings of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldSortDescription.IsGroupBy"/>
        /// <seealso cref="FieldLayout.SortedFields"/>
        //[Description("Determines if the groupby settings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeGroupBySettings
        {
            get { return _excludeGroupBySettings; }
            set { _excludeGroupBySettings = value; }
        }
        #endregion //ExcludeGroupBySettings

            #region ExcludeLabelPresenterStyles
        /// <summary>
        /// Determines if the LabelPresenterStyle settings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to include the <see cref="FieldSettings.LabelPresenterStyle"/> of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldSettings.LabelPresenterStyle"/>
        /// <seealso cref="FieldSettings.LabelPresenterStyleSelector"/>
        //[Description("Determines if the LabelPresenterStyle settings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeLabelPresenterStyles
        {
            get { return _excludeLabelPresenterStyles; }
            set { _excludeLabelPresenterStyles = value; }
        }
        #endregion //ExcludeLabelPresenterStyles

            // JJD 1/16/09 NA 2009 Vol 1 - Record filtering
            #region ExcludeRecordFilters
        /// <summary>
        /// Determines if record filter criteria from the ui XamDataGrid is used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over the record filter criteria from the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        /// <seealso cref="RecordManager.RecordFilters"/>
        //[Description("Determines if record filter criteria from the ui XamDataGrid is used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeRecordFilters
        {
            get { return _excludeRecordFilters; }
            set { _excludeRecordFilters = value; }
        }
            #endregion //ExcludeRecordFilters

            #region ExcludeRecordVisibility
        /// <summary>
        /// Determines if each record's Visibility setting from the ui XamDataGrid is used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over each <see cref="Record"/>'s <see cref="Record.Visibility"/> property setting from the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="Record.Visibility"/>
        //[Description("Determines if each record's Visibility setting from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeRecordVisibility
        {
            get { return _excludeRecordVisibility; }
            set { _excludeRecordVisibility = value; }
        }
        #endregion //ExcludeRecordVisibility

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            // Previously we were not copying over the sizing done on an individual record.
            // Since we do WYSIWYG by default we should copy this over but we wanted to provide
            // a way to skip this and other record sizing related properties.
            //
            #region ExcludeRecordSizing
        /// <summary>
        /// Determines if the extent of the record including the <see cref="FieldLayoutSettings.DataRecordSizingMode"/> and sizes of individually sized <see cref="Record"/> instances is copied from the associated record in the source XamDataGrid.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy over each <see cref="Record"/>'s sizing information from the associated ui XamDataGrid in the report as well as the 
        /// value of the <see cref="FieldLayoutSettings.DataRecordSizingMode"/>. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="Record.Visibility"/>
        //[Description("Determines if each record's Visibility setting from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeRecordSizing
        {
            get { return _excludeRecordSizing; }
            set { _excludeRecordSizing = value; }
        }
            #endregion //ExcludeRecordSizing

            #region ExcludeSortOrder
        /// <summary>
        /// Determines if the sorted fields from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the <see cref="FieldLayout.SortedFields"/> from the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
        /// <seealso cref="FieldLayout.SortedFields"/>
        //[Description("Determines if the sorted fields from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeSortOrder
        {
            get { return _excludeSortOrder; }
            set { _excludeSortOrder = value; }
        }

        #endregion //ExcludeSortOrder

            #region ExcludeSummaries
        /// <summary>
        /// Determines if the summary settings from the ui XamDataGrid are used within the report.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If set to true this means that we don't want to copy the summary settings of the associated ui XamDataGrid 
        /// in the report. Default value is false. </p>
        /// </remarks>
       /// <seealso cref="FieldSettings.AllowSummaries"/>
        /// <seealso cref="FieldSettings.SummaryUIType"/>
        /// <seealso cref="FieldSettings.SummaryDisplayArea"/>
        /// <seealso cref="FieldLayoutSettings.SummaryDescriptionVisibility"/>
        /// <seealso cref="FieldLayout.SummaryDefinitions"/>
        //[Description("Determines if the groupby settings from the ui XamDataGrid are used within the report.")]
        //[Category("Behavior")]
        [DefaultValue(false)]
        public bool ExcludeSummaries
        {
            get { return _excludeSummaries; }
            set { _excludeSummaries = value; }
        }
        #endregion //ExcludeSummaries

            #region Theme
        /// <summary>
        /// Returns/sets the default look for the control in a report. 
        /// </summary>
        /// <remarks>
        /// <p class="body">Identifies the theme for print version of grid.</p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase.Theme"/>
        //[Description("Determines if the sorted fields from the ui XamDataGrid are used within the report.")]
        //[Category("Appearance")]
        [DefaultValue(null)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.DataPresenterThemeTypeConverter))]
        public string Theme
        {
            set { _theme = value; }
            get { return _theme; }
        }
            #endregion //Theme

            #endregion //Public Properties

            #region Internal Properties
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods

                #region CloneView

        internal static ReportViewBase CloneView(DataPresenterBase uiDP)
        {
            Debug.Assert(!(uiDP is DataPresenterReportControl), "Only the ui datapresenter should have its views cloned.");

            // clone both the ui datapresenter's current view and its report view.
            // this will prevent setting changes from being applied during a report pagination

            // clone the ui view
            // JJD 2/9/09 - TFS13678
            // Pass false as 2nd param so we don't copy over those properties that are marked 
            // DesignerSerializationVisibility.Hidden
            //ViewBase uiViewClone = DataPresenterReportControl.CloneHelper(uiDP.CurrentViewInternal) as ViewBase;
            ViewBase uiViewClone = DataPresenterReportControl.CloneHelper(uiDP.CurrentViewInternal, false) as ViewBase;

            Debug.Assert(!(uiViewClone is ReportViewBase), "The current view of a ui datapresenter should not be derived from ReportViewBase.");

            ReportViewBase reportViewClone;
            // clone the report view
            if (uiDP.ReportView != null)
            {
                // JJD 2/9/09 - TFS13678
                // Pass false as 2nd param so we don't copy over those properties that are marked 
                // DesignerSerializationVisibility.Hidden
                reportViewClone = DataPresenterReportControl.CloneHelper(uiDP.ReportView, false) as ReportViewBase;
            }
            else
                reportViewClone = new TabularReportView();

            // initialize the cloned report view with the ui view cloned above so that
            // derived classes can default their properties appropriately
            reportViewClone._uiView = uiViewClone;

            return reportViewClone;

        }

                #endregion //CloneView

            #endregion //Internal Methods	
    
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