using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A report view that prints the contents of a <see cref="DataPresenterBase"/> derived control in a tabular format.
	/// </summary>
    /// <remarks>
    /// <p class="body">The TabularReportView object is used by <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to provide 
    /// settings and defaults that <see cref="DataPresenterBase"/> (the base class for the <see cref="XamDataGrid"/> and 
    /// <see cref="XamDataPresenter"/> controls) can query when it provides print generation services in support of the View.
    /// The TabularReportView exposes a lot of property to control print version of grid. You can interaction with TabularReportView through <see cref="DataPresenterBase.ReportView"/>. 
    /// By default XamDataGrid creates a TabularReportView with default settings as describe in <see cref="ViewBase"/>.</p>
    /// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ReportView"/>
	public class TabularReportView : ReportViewBase
	{

        #region Constants

        internal const double DEFAULT_LEVELINDENTATION = 10;

        #endregion //Constants

		#region Base Class Overrides

			#region Properties

				#region AutoFitToRecord

        /// <summary>
        /// Returns a boolean indicating whether the cell area of a <see cref="DataRecordPresenter"/> will 
        /// be auto sized to the <see cref="RecordPresenter"/> itself or based on the root <see cref="RecordListControl"/> when 
        /// <see cref="DataPresenterBase.AutoFitResolved"/> is true.
        /// </summary>
        /// <remarks>
        /// <p class="body">For <see cref="XamDataPresenter.View"/>s where the item size should dictate the size available to the <see cref="RecordPresenter"/> for 
        /// the cell area, this should return true. For views such as <see cref="GridView"/>, where all the records are to 
        /// be constrained by the size available within the control itself, this should return false.</p>
        /// </remarks>
        /// <seealso cref="DataRecordPresenter"/>
        /// <seealso cref="RecordListControl"/>
        /// <seealso cref="XamDataPresenter.View"/>
        /// <seealso cref="GridView"/>
        /// <seealso cref="ViewBase.DefaultAutoFit"/>
        /// <seealso cref="IsAutoFitHeightSupported"/>
        /// <seealso cref="IsAutoFitWidthSupported"/>
        internal protected override bool AutoFitToRecord
		{
		    get { return false; }
		}
 
				#endregion //AutoFitToRecord

				#region CellPresentation

        /// <summary>
        /// Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the 
        /// cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.
        /// </summary>
        /// <seealso cref="CellPresentation"/>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
        internal protected override CellPresentation CellPresentation
		{
			get { return CellPresentation.GridView; }
		}

				#endregion //CellPresentation

				#region DefaultAutoArrangeCells

        /// <summary>
        /// Returns the default value for <see cref="AutoArrangeCells"/> for field layout templates generated on behalf of the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="AutoArrangeCells"/>.LeftToRight.
        /// </p>
        /// </remarks>
        /// <seealso cref="AutoArrangeCells"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeCells"/>
        internal protected override AutoArrangeCells DefaultAutoArrangeCells
		{
            get { return this.LogicalOrientation == System.Windows.Controls.Orientation.Vertical ? AutoArrangeCells.LeftToRight : AutoArrangeCells.TopToBottom; }
		}

				#endregion //DefaultAutoArrangeCells	
   
				#region HasLogicalOrientation

		/// <summary>
		/// Returns a value that indicates whether this View arranges its descendants in a particular dimension.
		/// </summary>
		internal protected override bool HasLogicalOrientation
		{
			get { return true; }
		}

				#endregion //HasLogicalOrientation	

				#region IsAutoFitHeightSupported

        /// <summary>
        /// Returns true if the height of the cells within in each row should be adjusted so that all cells will fit within the vertical space available for the row.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default implementation returns false.
        /// </p>
        /// </remarks>
        internal protected override bool IsAutoFitHeightSupported
		{
			get	
			{
                return this.LogicalOrientation == System.Windows.Controls.Orientation.Horizontal;
			}
		}

   				#endregion //IsAutoFitHeightSupported	

				#region IsAutoFitWidthSupported

        /// <summary>
        /// Returns true if the width of the cells within in each row should be adjusted so that all cells will fit within the horizontal space available for the row.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default implementation returns false.
        /// </p>
        /// </remarks>
        internal protected override bool IsAutoFitWidthSupported
		{
			get	
			{
                return this.LogicalOrientation == System.Windows.Controls.Orientation.Vertical;
			}
		}

   				#endregion //IsAutoFitWidthSupported	

				// MD 6/3/10 - ChildRecordsDisplayOrder feature
				#region IsChildRecordsDisplayOrderSupported

		/// <summary>
		/// Returns true if the view supports the ability to control where the child records are ordered relative to the parent record.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		protected internal override bool IsChildRecordsDisplayOrderSupported
		{
			get { return true; }
		}

				#endregion // IsChildRecordsDisplayOrderSupported

				#region ItemsPanelType

        /// <summary>
        /// Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.
        /// </summary>
        /// <seealso cref="System.Windows.Controls.Panel"/>
        internal protected override Type ItemsPanelType
		{
			get { return typeof(TabularReportViewPanel); }
		}

				#endregion //ItemsPanelType
    
				#region LogicalOrientation

        /// <summary>
        /// The <see cref="System.Windows.Controls.Orientation"/> of the View, if the view only supports layout in a particular dimension.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The <see cref="HasLogicalOrientation"/> property returns whether the View only supports layout in a particular dimension.
        /// The base implementation returns <see cref="System.Windows.Controls.Orientation"/>.Vertical.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Windows.Controls.Orientation"/>
        /// <seealso cref="HasLogicalOrientation"/>
        internal protected override Orientation LogicalOrientation
		{
			get 
            {
                Orientation? orientation = this.Orientation;

                // if the Orientation property was explicitly set then use its value
                if (orientation.HasValue)
                    return orientation.Value;

                // If the UiView is not null (i.e. this is the cloned TabularReportView used
                // during a report pagination) and it has a logical orientation then
                // return its logical orientation.

                if (this.UiView != null &&
                     this.UiView.HasLogicalOrientation)
                    return this.UiView.LogicalOrientation;

                // finally default to Vertical
                return System.Windows.Controls.Orientation.Vertical; 
            }
		}

				#endregion //LogicalOrientation	
    
				#region SupportedDataDisplayMode

        /// <summary>
        /// Returns a value that indicates the <see cref="DataDisplayMode"/> supported by the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="DataDisplayMode"/>.Flat.
        /// Note: Views that support the Hierarchical <see cref="DataDisplayMode"/> are responsible for managing
        /// the display of nested data.  Such views will typically return true for <see cref="ViewBase.IsNestedPanelsSupported"/>.
        /// When the <see cref="DataDisplayMode"/>.Flat enumeration is returned, the DataPresenter will still include child
        /// records in the <see cref="ViewableRecordCollection"/>s but it will cause expansion indicators to be hidden and
        /// prohibit records from being expanded.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataDisplayMode"/>
        /// <seealso cref="ViewBase.IsNestedPanelsSupported"/>
        /// <seealso cref="ViewableRecordCollection"/>
        internal protected override DataDisplayMode SupportedDataDisplayMode
		{
			get { return DataDisplayMode.Hierarchical; }
		}

				#endregion //SupportedDataDisplayMode

			#endregion //Properties	
        
			#region Methods

                #region GetFieldLayoutTemplateGenerator

        /// <summary>
        /// Gets a <see cref="FieldLayoutTemplateGenerator"/> derived class for generating an appropriate template for the specified layout in the current View.
        /// </summary>
        /// <param name="fieldLayout">The specified layout</param>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
        /// <seealso cref="FieldLayout"/>
        internal protected override FieldLayoutTemplateGenerator GetFieldLayoutTemplateGenerator(FieldLayout fieldLayout)
        {
            return new HeaderedGridViewFieldLayoutTemplateGenerator(fieldLayout, this.LogicalOrientation);
        }

                #endregion //GetFieldLayoutTemplateGenerator

			#endregion //Methods
			
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

                #region LevelIndentation

        /// <summary>
        /// Identifies the <see cref="LevelIndentation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LevelIndentationProperty = DependencyProperty.Register("LevelIndentation",
            typeof(double), typeof(TabularReportView), new FrameworkPropertyMetadata(DEFAULT_LEVELINDENTATION));

        /// <summary>
        /// Returns/sets the how far each level is indented. 
        /// </summary>
        /// <seealso cref="LevelIndentationProperty"/>
        //[Description("Returns/sets the how far each level is indented. ")]
        //[Category("Appearance")]
        [Bindable(true)]
        public double LevelIndentation
        {
            get
            {
                return (double)this.GetValue(TabularReportView.LevelIndentationProperty);
            }
            set
            {
                this.SetValue(TabularReportView.LevelIndentationProperty, value);
            }
        }

                #endregion //LevelIndentation

				#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation?), typeof(TabularReportView), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/ sets the orientation of the records in the report contents.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns/ sets the orientation of the report contents.")]
		//[Category("Appearance")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<Orientation>))] // AS 5/15/08 BR32816
		public Orientation? Orientation
		{
			get
			{
				return (Orientation?)this.GetValue(TabularReportView.OrientationProperty);
			}
			set
			{
				this.SetValue(TabularReportView.OrientationProperty, value);
			}
		}

				#endregion //Orientation
		
			#endregion //Public Properties

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