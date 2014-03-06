using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.DataPresenter.Events;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// <see cref="ViewBase"/> derived class that defines settings and defaults for a view that arranges data using a classic grid layout.  The GridView object is used by <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/>
    /// </summary>
    /// <remarks>
    /// <p class="body">The GridView object is used by <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to provide 
    /// settings and defaults that <see cref="DataPresenterBase"/> (the base class for the <see cref="XamDataGrid"/> and 
    /// <see cref="XamDataPresenter"/> controls) can query when it provides UI element generation and field layout generation services in support of the View.  While the
    /// GridView is not actually reponsible for arranging items, it does expose a property called <see cref="ItemsPanelType"/> that returns the 
    /// <see cref="System.Windows.Controls.Panel"/> derived type that should be used to provide layout functionality for <see cref="DataRecord"/>s displayed in the
    /// view.  <see cref="DataPresenterBase"/> will ensure that a panel of <see cref="ItemsPanelType"/> is generated for use by the embedded <see cref="RecordListControl"/> 
    /// (the <see cref="System.Windows.Controls.ListBox"/> derived class used to display <see cref="DataRecord"/>s).</p>
    /// <p class="body">The GridView object exposes a property called <see cref="ViewSettings"/> that returns a <see cref="GridViewSettings"/> 
    /// object.  (Note: This property is not found on the <see cref="ViewBase"/> class but is specific to the GridView).  <see cref="GridViewSettings"/>
    /// in turn exposes properties that let you control features supported by the GridView.  
    /// Refer to <see cref="GridViewSettings"/> object for detailed information on these properties.</p>
    /// <p class="note"><b>Note: </b>GridView is only used by the <see cref="XamDataPresenter"/> control (as described above) when the <see cref="XamDataPresenter"/> control's <see cref="XamDataPresenter.View"/>
    /// property is set to an instance of GridView.</p>
    /// <p class="body">The following ViewBase properties are overridden by the CarouselView:
    /// 
    /// <table class="igbluetablenowidth">
    ///     <thead>
    ///         <tr>
    ///             <th>Property</th>
    ///             <th>Description</th>
    ///             <th>Overridden Value</th>
    ///         </tr>
    ///     </thead>
    ///     <tbody>
    ///         <tr>
    ///             <td>AutoFitToRecord</td>
    ///             <td>Returns a boolean indicating whether the cell area of a <see cref="DataRecordPresenter"/> will be auto sized to the <see cref="RecordPresenter"/> itself or based on the root <see cref="RecordListControl"/> when <see cref="DataPresenterBase.AutoFitResolved"/> is true.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>CellPresentation</td>
    ///             <td>Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.</td>
    ///             <td><see cref="CellPresentation"/>.GridView</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultAutoArrangeCells</td>
    ///             <td>Returns the default value for <see cref="AutoArrangeCells"/> for field layout templates generated on behalf of the View.</td>
    ///             <td>If <see cref="LogicalOrientation"/> is Vertical then <see cref="AutoArrangeCells"/>.LeftToRight otherwise <see cref="AutoArrangeCells"/>.LeftToRight</td>
    ///         </tr>
    ///         <tr>
    ///             <td>HasLogicalOrientation</td>
    ///             <td>Returns a value that indicates whether this View arranges its descendants in a particular dimension.</td>
    ///             <td>true</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsAutoFitHeightSupported</td>
    ///             <td>Returns true if the height of the cells within in each row should be adjusted so that all cells will fit within the vertical space available for the row.</td>
    ///             <td>If ViewSettings.Orientation is <see cref="System.Windows.Controls.Orientation"/>.Horizontal then true, otherwise false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsAutoFitWidthSupported</td>
    ///             <td>Returns true if the width of the cells within in each row should be adjusted so that all cells will fit within the horizontal space available for the row.</td>
    ///             <td>If ViewSettings.Orientation is <see cref="System.Windows.Controls.Orientation"/>.Vertical then true, otherwise false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsFixedRecordsSupported</td>
    ///             <td>Returns true if the <see cref="DataPresenterBase"/> should allow records to be fixed at the top or bottomof the UI.</td>
    ///             <td>true</td>
    ///         </tr>
    ///         <tr>
    ///             <td>ItemsPanelType</td>
    ///             <td>Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.</td>
    ///             <td>typeof(<see cref="GridViewPanel"/>)</td>
    ///         </tr>
    ///         <tr>
    ///             <td>LogicalOrientation</td>
    ///             <td>The <see cref="System.Windows.Controls.Orientation"/> of the View, if the view only supports layout in a particular dimension.</td>
    ///             <td>ViewSettings.Orientation</td>
    ///         </tr>
    ///         <tr>
    ///             <td>SupportedDataDisplayMode</td>
    ///             <td>Returns a value that indicates the <see cref="DataDisplayMode"/> supported by the View.</td>
    ///             <td><see cref="DataDisplayMode"/>.Hierarchical</td>
    ///         </tr>
    ///     </tbody>
    /// </table>
    /// </p>
    /// </remarks>
    /// <see cref="ViewBase"/>
    /// <see cref="XamDataGrid"/>
    /// <see cref="XamDataPresenter"/>
    /// <see cref="ItemsPanelType"/>
    /// <see cref="DataRecord"/>
    /// <see cref="RecordListControl"/>
    /// <see cref="ViewSettings"/>
    public class GridView : ViewBase
	{
		#region Member Variables

		private GridViewSettings						_viewSettings = null;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GridView"/> class
		/// </summary>
		public GridView()
		{
			this.SetValue(GridView.ViewSettingsProperty, this.ViewSettings);
		}

		#endregion //Constructor

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
        /// Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.
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
			get { return this.LogicalOrientation == Orientation.Vertical ? AutoArrangeCells.LeftToRight : AutoArrangeCells.TopToBottom; }
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
				return this.ViewSettings.Orientation == Orientation.Horizontal;
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
				return this.ViewSettings.Orientation == Orientation.Vertical;
			}
		}

   				#endregion //IsAutoFitWidthSupported	

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
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

				#region IsFilterRecordSupported

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// Added IsFilterRecordSupported property.
		// 
		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should display filter records.
		/// </summary>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		internal protected override bool IsFilterRecordSupported
		{
			get { return true; }
		}

				#endregion // IsFilterRecordSupported

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
				#region IsFixedFieldsSupported

        /// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow fields to be fixed.
        /// </summary>
        /// <seealso cref="Field.FixedLocation"/>
        /// <seealso cref="Field.IsFixed"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        internal protected override bool IsFixedFieldsSupported
		{
			get { return true; }
		}

				#endregion //IsFixedFieldsSupported

				#region IsFixedRecordsSupported

        /// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow records to be fixed at the top or bottom of the UI.
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
			get { return true; }
		}

				#endregion //IsFixedRecordsSupported

				#region IsFixingSupportedForNestedRecords

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow records to be fixed at the top of the UI for nested (i.e. non-root) records.
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
        internal protected override bool IsFixingSupportedForNestedRecords
		{
			get { return false == this.ViewSettings.UseNestedPanels; }
		}

				#endregion //IsFixingSupportedForNestedRecords


				// JJD 2/22/12 - TFS101199 - Touch support
				#region IsPanningModeSupported

		/// <summary>
		/// Returns true if the RecordListControl should coerce its ScrollViewer.PanningMode property to enable standard flick scrolling on touch enabled systems.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// This implementation returns true.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
		internal protected override bool IsPanningModeSupported
		{
			get { return true; }
		}

				#endregion //IsPanningModeSupported


				#region ItemsPanelType

        /// <summary>
        /// Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.
        /// </summary>
        /// <seealso cref="System.Windows.Controls.Panel"/>
        internal protected override Type ItemsPanelType
		{
            // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
            //get { return typeof(GridViewPanel); }
			get 
            {
                if (this.ViewSettings.UseNestedPanels)
                    return typeof(GridViewPanelNested);
                else
                    return typeof(GridViewPanelFlat); 
            }
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
			get { return this.ViewSettings.Orientation; }
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
            // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
            //get { return DataDisplayMode.Hierarchical; }
            get
            {
                if (this.ViewSettings.UseNestedPanels)
                    return DataDisplayMode.Hierarchical;
                else
                    return DataDisplayMode.FlattenedHierarchical;
            }
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
            return new HeaderedGridViewFieldLayoutTemplateGenerator(fieldLayout, this.ViewSettings.Orientation);
        }

                #endregion //GetFieldLayoutTemplateGenerator

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
                #region GetFixedFieldInfo
        /// <summary>
        /// Returns the fixed field information for the specified record element.
        /// </summary>
        /// <param name="recordPresenter">The record element for which the info is being provided.</param>
        /// <returns></returns>
        internal override FixedFieldInfo GetFixedFieldInfo(RecordPresenter recordPresenter)
        {
            DataPresenterBase dp = recordPresenter.DataPresenter;
            GridViewPanel panel = null != dp ? dp.CurrentPanel as GridViewPanel : null;
            return panel != null ? panel.FixedFieldInfo : null;
        } 
                #endregion //GetFixedFieldInfo

				#region OnPropertyChanged

		/// <summary>
		/// Called when the value of a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

            // AS 3/11/09 Optimization
			//switch (e.Property.Name)
			//{
            //    case "ViewSettings":
            //        {
            if (e.Property == ViewSettingsProperty)
            {
                        // JJD 5/19/08 - BR32749
                        // Hold to old orientation settings
                        Orientation oldOrientation = Orientation.Vertical;

                        // Unhook from old
                        if (this._viewSettings != null)
                        {
                            this._viewSettings.PropertyChanged -= new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);
                            
                            // JJD 5/19/08 - BR32749
                            // Hold to old orientation settings
                            oldOrientation = this._viewSettings.Orientation;
                        }


                        // Set our ViewSettings member variable.
                        this._viewSettings = e.NewValue as GridViewSettings;


                        // JJD 5/19/08 - BR32749
                        // Hold to new orientation settings
                        Orientation newOrientation = Orientation.Vertical;

                        // Hook new change event
                        if (this._viewSettings != null)
                        {
                            this._viewSettings.PropertyChanged += new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);

                            // JJD 5/19/08 - BR32749
                            // Hold to new orientation settings
                            newOrientation = this._viewSettings.Orientation;
                        }


                        // Raise a ViewStateChanged event.

                        // JJD 5/19/08 - BR32749
                        // If the orientaion has changed then make the action
                        // InvalidateFieldLayouts instead of just InvalidateMeasure
                        //this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));
                        this.RaiseViewStateChanged(new ViewStateChangedEventArgs(oldOrientation == newOrientation ? ViewStateChangedAction.InvalidateMeasure : ViewStateChangedAction.InvalidateFieldLayouts));

                        //break;
                    //}
			}
		}

				#endregion //OnPropertyChanged	

			#endregion //Methods
			
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
			typeof(GridViewSettings), typeof(GridView), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
			if (value == null)
				return new GridViewSettings();

			return value;
		}

		/// <summary>
        /// Returns/sets the <see cref="GridViewSettings"/> object for this GridView.
		/// </summary>
        /// <remarks>
        /// <p class="body"><see cref="GridViewSettings"/> exposes properties that let you control features supported by the GridView.  
        /// Refer to <see cref="GridViewSettings"/> object for detailed information on these properties.</p>
        /// </remarks>
		/// <seealso cref="ViewSettingsProperty"/>
		/// <seealso cref="GridViewSettings"/>
		//[Description("Returns/sets the GridViewSettings object for this GridView.")]
		//[Category("Appearance")]
		[Bindable(true)]
        public GridViewSettings ViewSettings
		{
			get
			{
				if (this._viewSettings == null)
					this._viewSettings = new GridViewSettings();

				return this._viewSettings;
			}
			set
			{
				this.SetValue(GridView.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (GridViewSettings)GridView.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ClearValue(GridView.ViewSettingsProperty);
		}

				#endregion //ViewSettings

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnViewSettingsPropertyChanged

		private void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Orientation":
				case "UseNestedPanels":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateFieldLayouts));
					break;
				case "HeightInInfiniteContainers":
				case "WidthInInfiniteContainers":
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));
					break;

				default:
					break;
			}
		}

				#endregion //OnViewSettingsPropertyChanged

			#endregion //Private Methods	
    
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