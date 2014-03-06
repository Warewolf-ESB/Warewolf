using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Used by a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> to specify settings for <see cref="Field"/>s.
    /// </summary>
	/// <remarks>
	/// <para class="body">This settings object is exposed via the following 3 properties:
	/// <ul>
	/// <li><see cref="DataPresenterBase"/>'s <see cref="DataPresenterBase.FieldSettings"/> - settings specified here become the default for all <see cref="Field"/>s in every <see cref="FieldLayout"/>.</li>
	/// <li><see cref="FieldLayout"/>'s <see cref="FieldLayout.FieldSettings"/> - settings specified here become the default for all <see cref="Field"/>s in this <see cref="FieldLayout"/>'s <see cref="FieldLayout.Fields"/> collection.</li>
	/// <li><see cref="Field"/>'s <see cref="Field.Settings"/> - settings specified here apply to only this one specific <see cref="Field"/>.</li>
	/// </ul>
	/// </para>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Settings.html">Field Settings</a> topic in the Developer's Guide for an explanation of the FieldSettings object.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Field"/>
    /// <seealso cref="FieldLayout"/>
    /// <seealso cref="FieldLayoutSettings"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldSettings"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StyleTypedProperty(Property = "CellPresenterStyle", StyleTargetType = typeof(CellPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "CellValuePresenterStyle", StyleTargetType = typeof(CellValuePresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "EditorStyle", StyleTargetType = typeof(ValueEditor))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ExpandedCellStyle", StyleTargetType = typeof(ExpandedCellPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ExpandableFieldRecordPresenterStyle", StyleTargetType = typeof(ExpandableFieldRecord))]	// AS 5/3/07
	[StyleTypedProperty(Property = "GroupByRecordPresenterStyle", StyleTargetType = typeof(GroupByRecord))]	// AS 5/3/07
	[StyleTypedProperty(Property = "LabelPresenterStyle", StyleTargetType = typeof(LabelPresenter))]	// AS 5/3/07
    // JJD 1/7/09 - NA 2009 vol 1 - record filtering
	[StyleTypedProperty(Property = "FilterCellValuePresenterStyle", StyleTargetType = typeof(FilterCellValuePresenter))]	
    [StyleTypedProperty(Property = "FilterCellEditorStyle", StyleTargetType = typeof(ValueEditor))]	
    // JJD 2/11/09 - TFS10860/TFS13609
    [CloneBehavior(CloneBehavior.CloneObject)]
    public class FieldSettings : DependencyObjectNotifier
    {
        #region Private Members

        #endregion //Private Members

        #region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldSettings"/> class
		/// </summary>
		public FieldSettings()
        {
        }

        #endregion //Constructors

        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        ///Called when a property has been set
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrides	
        
        #region Properties

            #region Public Properties

                #region AllowCellVirtualization

        /// <summary>
        /// Identifies the <see cref="AllowCellVirtualization"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowCellVirtualizationProperty = DependencyProperty.Register("AllowCellVirtualization",
                typeof(Nullable<bool>), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnAllowCellVirtualizationChanged )
			));

		// JJD 4/26/07
		// Optimization - cache the property locally
		private bool? _cachedAllowCellVirtualization;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnAllowCellVirtualizationChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			bool? newVal = (bool?)e.NewValue;

			fieldSettings._cachedAllowCellVirtualization = newVal;
		}

        /// <summary>
        /// Determines if the cell uielement creation can be deferred until the cell is brought into view.
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> even if this property is resolved to true it is possible that it will be ignored in certain conditions where the cells cannot be virtually allocated.</p>
		/// </remarks>
		/// <seealso cref="AllowCellVirtualizationProperty"/>
        /// <seealso cref="Field.AllowCellVirtualizationResolved"/>
        /// <seealso cref="Field"/>
		//[Description("Determines if the cell uielement creation can be deferred until the cell is brought into view.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowCellVirtualization
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool?)this.GetValue(FieldSettings.AllowCellVirtualizationProperty);
				return this._cachedAllowCellVirtualization;
            }
            set
            {
                this.SetValue(FieldSettings.AllowCellVirtualizationProperty, value);
            }
        }

                #endregion //AllowCellVirtualization

		//        #region AllowedFieldChooserCustomizations

		//// SSP 6/3/09 - NAS9.2 Field Chooser
		//// 

		///// <summary>
		///// Identifies the <see cref="AllowedFieldChooserCustomizations"/> dependency property.
		///// </summary>
		//[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		//public static readonly DependencyProperty AllowedFieldChooserCustomizationsProperty = DependencyProperty.Register(
		//    "AllowedFieldChooserCustomizations",
		//    typeof( AllowedFieldChooserCustomizations ),
		//    typeof( FieldSettings ),
		//    new FrameworkPropertyMetadata( AllowedFieldChooserCustomizations.All, FrameworkPropertyMetadataOptions.None )
		//);

		///// <summary>
		///// Specifies what type of customizations the user is allowed to perform via <see cref="FieldChooser"/>.
		///// </summary>
		///// <remarks>
		///// <para class="body">
		///// <b>AllowedFieldChooserCustomizations</b> specifies what type of customizations the user is allowed
		///// to perform via any <see cref="FieldChooser"/> associated with the data presenter. The user interface 
		///// that allows the user to custimize fields is exposed via the <see cref="FieldChooser"/>. You can
		///// set the <see cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/> property to 
		///// <b>FieldChooserButton</b> to display a button in header prefix area (by default the area left 
		///// of the field headers) that allows the user to display FieldChooser. You can also display
		///// a FieldChooser by calling data presenter's <see cref="DataPresenterBase.ShowFieldChooser()"/>
		///// method.
		///// </para>
		///// </remarks>
		///// <seealso cref="FieldChooser"/>
		///// <seealso cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/>
		//[Description( "" )]
		//[Category( "" )]
		//[Bindable( true )]
		//[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		//public AllowedFieldChooserCustomizations AllowedFieldChooserCustomizations
		//{
		//    get
		//    {
		//        return (AllowedFieldChooserCustomizations)this.GetValue( AllowedFieldChooserCustomizationsProperty );
		//    }
		//    set
		//    {
		//        this.SetValue( AllowedFieldChooserCustomizationsProperty, value );
		//    }
		//}

		///// <summary>
		///// Returns true if the AllowedFieldChooserCustomizations property is set to a non-default value.
		///// </summary>
		//[EditorBrowsable( EditorBrowsableState.Never )]
		//[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		//public bool ShouldSerializeAllowedFieldChooserCustomizations( )
		//{
		//    return Utilities.ShouldSerialize( AllowedFieldChooserCustomizationsProperty, this );
		//}

		///// <summary>
		///// Resets the AllowedFieldChooserCustomizations property to its default state.
		///// </summary>
		//[EditorBrowsable( EditorBrowsableState.Never )]
		//[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		//public void ResetAllowedFieldChooserCustomizations( )
		//{
		//    this.ClearValue( AllowedFieldChooserCustomizationsProperty );
		//}

		//        #endregion // AllowedFieldChooserCustomizations

                #region AllowEdit

        /// <summary>
        /// Identifies the <see cref="AllowEdit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowEditProperty = DependencyProperty.Register("AllowEdit",
                typeof(Nullable<bool>), typeof(FieldSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Determines if the user can edit a cell.
        /// </summary>
		/// <remarks>The ultimate default value used is true. However, even if this property is resolved to true it will be ignored if the <see cref="DataPresenterBase.DataSource"/> does not support editing.</remarks>
		/// <seealso cref="AllowEditProperty"/>
        /// <seealso cref="Field.AllowEditResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="FieldLayoutSettings.AllowAddNew"/>
        /// <seealso cref="FieldLayoutSettings.AllowDelete"/>
		//[Description("Determines if the user can edit a cell.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public Nullable<bool> AllowEdit
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldSettings.AllowEditProperty);
            }
            set
            {
                this.SetValue(FieldSettings.AllowEditProperty, value);
            }
        }

                #endregion //AllowEdit

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region AllowFixing

        /// <summary>
        /// Identifies the <see cref="AllowFixing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowFixingProperty = DependencyProperty.Register("AllowFixing",
                typeof(AllowFieldFixing), typeof(FieldSettings), new FrameworkPropertyMetadata(AllowFieldFixing.Default, 
                    new PropertyChangedCallback(OnAllowFixingChanged)
                    ));

        private static void OnAllowFixingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {



		}

        /// <summary>
        /// Determines if the end user should be allowed to change the fixed state of the field and to which edges the field may be fixed.
        /// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AllowFieldFixing"/>
        /// <seealso cref="Field.AllowFixingResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="FieldLayoutSettings.FixedFieldUIType"/>
        /// <seealso cref="Field.FixedLocation"/>
		//[Description("Determines if the end user should be allowed to change the fixed state of the field and to which edges the field may be fixed.")]
        //[Category("Behavior")]
		[Bindable(true)]
        public AllowFieldFixing AllowFixing
        {
            get
            {
                return (AllowFieldFixing)this.GetValue(FieldSettings.AllowFixingProperty);
            }
            set
            {
                this.SetValue(FieldSettings.AllowFixingProperty, value);
            }
        }

                #endregion //AllowFixing

                #region AllowGroupBy

        /// <summary>
        /// Identifies the <see cref="AllowGroupBy"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowGroupByProperty = DependencyProperty.Register("AllowGroupBy",
                typeof(Nullable<bool>), typeof(FieldSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Determines if the user can initiate a group by operation thru the UI for this <see cref="Field"/>
        /// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Grouping">About Grouping</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
		/// </remarks>
        /// <seealso cref="FieldSettings.AllowGroupBy"/>
        /// <seealso cref="FieldSettings.AllowGroupByProperty"/>
        /// <seealso cref="Field.AllowGroupByResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="GroupByComparer"/>
        /// <seealso cref="GroupByEvaluator"/>
        /// <seealso cref="GroupByMode"/>
        /// <seealso cref="GroupByRecordPresenterStyle"/>
        /// <seealso cref="GroupByRecordPresenterStyleSelector"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
        //[Description("Determines if the user can initiate a group by operation thru the UI for this field")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public Nullable<bool> AllowGroupBy
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldSettings.AllowGroupByProperty);
            }
            set
            {
                this.SetValue(FieldSettings.AllowGroupByProperty, value);
            }
        }

                #endregion //AllowGroupBy

				#region AllowHiding

		// SSP 8/21/09 - NAS9.2 Field Chooser
		
		
		// 

		/// <summary>
		/// Identifies the <see cref="AllowHiding"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty AllowHidingProperty = DependencyProperty.Register(
			"AllowHiding",
			typeof( AllowFieldHiding ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( AllowFieldHiding.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether the user can show or hide a field. It also controls whether the field
		/// is displayed in the field chooser control.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AllowHiding</b> property determines whether the user can show or hide a field.
		/// It also controls whether the field is displayed in the field chooser. If set to <b>Never</b>
		/// then the field is excluded from the field chooser and the user is not allowed to show or
		/// hide the field in any way.
		/// </para>
		/// <para class="body">
		/// If set to <b>ViaFieldChooserOnly</b>, the user can show or hide a field through the field
		/// chooser control only.
		/// </para>
		/// <para class="body">
		/// If set the <b>Always</b>, in addition to being able to show or hide via field chooser 
		/// control, the user can hide a field by dragging it outside of the data presenter.
		/// </para>
		/// </remarks>
		/// <seealso cref="AllowFieldHiding"/>
		//[Description( "Specifies if the user can show or hide the field." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public AllowFieldHiding AllowHiding
		{
			get
			{
				return (AllowFieldHiding)this.GetValue( AllowHidingProperty );
			}
			set
			{
				this.SetValue( AllowHidingProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the AllowHiding property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public bool ShouldSerializeAllowHiding( )
		{
			return Utilities.ShouldSerialize( AllowHidingProperty, this );
		}

		/// <summary>
		/// Resets the AllowHiding property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public void ResetAllowHiding( )
		{
			this.ClearValue( AllowHidingProperty );
		}

				#endregion // AllowHiding

                // JJD 2/7/08 - BR30444 - added
                #region AllowLabelVirtualization

        /// <summary>
        /// Identifies the <see cref="AllowLabelVirtualization"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowLabelVirtualizationProperty = DependencyProperty.Register("AllowLabelVirtualization",
                typeof(Nullable<bool>), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback( OnAllowLabelVirtualizationChanged )
			));

		// JJD 4/26/07
		// Optimization - cache the property locally
		private bool? _cachedAllowLabelVirtualization;


		
		
		
		
		
		private static void OnAllowLabelVirtualizationChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			bool? newVal = (bool?)e.NewValue;

			fieldSettings._cachedAllowLabelVirtualization = newVal;
		}

        /// <summary>
        /// Determines if the label uielement creation can be deferred until the label is brought into view.
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> even if this property is resolved to true it is possible that it will be ignored in certain conditions where the labels cannot be virtually allocated.</p>
		/// </remarks>
		/// <seealso cref="AllowLabelVirtualizationProperty"/>
        /// <seealso cref="Field.AllowLabelVirtualizationResolved"/>
        /// <seealso cref="Field"/>
		//[Description("Determines if the label uielement creation can be deferred until the label is brought into view.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowLabelVirtualization
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool?)this.GetValue(FieldSettings.AllowLabelVirtualizationProperty);
				return this._cachedAllowLabelVirtualization;
            }
            set
            {
                this.SetValue(FieldSettings.AllowLabelVirtualizationProperty, value);
            }
        }

                #endregion //AllowLabelVirtualization

				#region AllowRecordFiltering

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="AllowRecordFiltering"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AllowRecordFilteringProperty = DependencyProperty.Register(
			"AllowRecordFiltering",
			typeof( bool? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Enables the record filtering user interface. Default is resolved to <b>False</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AllowRecordFiltering</b> property enables the record filtering user interface where the user can 
		/// specify filter criteria to filter records. Records that do not match the specified filter criteria will 
		/// hidden (this is default behavior which can be changed by setting the <see cref="FieldLayoutSettings.FilterAction"/> property),
		/// and thus presenting the user with the view of only the data that matches the filter criteria.
		/// </para>
		/// <para class="body">
		/// <see cref="FieldLayoutSettings.FilterUIType"/> property specifies the type of user interface that the data presenter will
		/// use to let the user filter records.
		/// </para>
		/// <para class="body">
		/// To actually specify filter conditions in code to pre-filter the records, use the FieldLayout's <see cref="FieldLayout.RecordFilters"/>
		/// or the RecordManager's <see cref="RecordManager.RecordFilters"/> depending on the <see cref="FieldLayoutSettings.RecordFilterScope"/>
		/// property setting. By default FieldLayout's RecordFilters are used.
		/// </para>
		/// <para class="body">
		/// To check if a data record is filtered out, use the record's <see cref="DataRecord.IsFilteredOut"/> property.
		/// GroupByRecord also exposes <see cref="GroupByRecord.IsFilteredOut"/> property, which returns true if all
		/// of its descendant data records are filtered out.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> In filter record if a field's AllowRecordFiltering is set to false, the filter cell will be disabled.
		/// However it will display the current filter criteria if any. 
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.FilterOperandUIType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDefaultValue"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// <seealso cref="RecordFilterCollection"/>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="RecordManager.RecordFilters"/>
		/// <seealso cref="DataRecord.IsFilteredOut"/>
		/// <seealso cref="GroupByRecord.IsFilteredOut"/>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		//[Description( "Enables the record filtering user interface." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<bool> ) )]
		public bool? AllowRecordFiltering
		{
			get
			{
				return (bool?)this.GetValue( AllowRecordFilteringProperty );
			}
			set
			{
				this.SetValue( AllowRecordFilteringProperty, value );
			}
		}

				#endregion // AllowRecordFiltering

                #region AllowResize

        /// <summary>
        /// Identifies the <see cref="AllowResize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowResizeProperty = DependencyProperty.Register("AllowResize",
                typeof(Nullable<bool>), typeof(FieldSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Determines if the user can resize a cell or label in a <see cref="Field"/>
        /// </summary>
        /// <seealso cref="FieldSettings.AllowResize"/>
        /// <seealso cref="FieldSettings.AllowResizeProperty"/>
        /// <seealso cref="Field.AllowResizeResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
        /// <seealso cref="FieldLayoutSettings.ResizingMode"/>
		/// <remarks>
		/// <para>
		/// In <see cref="XamDataGrid"/> with <see cref="GridViewSettings.Orientation"/> set to Horizontal, this property governs row sizing. Otherwise it governs column resizing.
		/// </para>
		/// </remarks>
		//[Description("Determines if the user can resize a cell or label in a  field")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public Nullable<bool> AllowResize
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldSettings.AllowResizeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.AllowResizeProperty, value);
            }
        }

                #endregion //AllowResize

				#region AllowSummaries

		
		
		/// <summary>
		/// Identifies the <see cref="AllowSummaries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowSummariesProperty = DependencyProperty.Register(
			"AllowSummaries",
			typeof( bool? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies if the summary calculation selection UI is enabled for the user to select one or more
		/// summary calculations to perform on field values.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AllowSummaries</b> enables the summary calculation selection UI. A summary icon will be displayed
		/// inside the field label. <b>Note</b> that depending on the theme being used, the icon may get displayed
		/// when hovering the mouse over the field label. When the icon is clicked, it will display the UI for selecting 
		/// the type of summary calculation to perform on the field. The calculated summary result will be displayed 
		/// in a summary record. The summary result by default is displayed in the summary record at the end of 
		/// the record collection however you can control if and where the summary result is displayed by setting 
		/// the FieldSettings' <see cref="FieldSettings.SummaryDisplayArea"/> property.
		/// </para>
		/// <para class="body">
		/// Also <see cref="FieldSettings.SummaryUIType"/> property provides you with options for controlling 
		/// whether the summary selection UI allows the user to select only one summary per field as well as 
		/// whether summary selection UI is enabled only on numeric fields (see <b>SingleSelectForNumericsOnly</b> 
		/// and <b>MultiSelectForNumericsOnly</b> options of the <see cref="SummaryUIType"/> enumeration).
		/// </para>
		/// <para class="body">
		/// To specify summary calculations to perform in code, use the FieldLayout's 
		/// <seealso cref="FieldLayout.SummaryDefinitions"/> property. Also as the user selects summaries,
		/// this collection will be modified to reflect user selected summaries.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		/// <seealso cref="FieldSettings.SummaryUIType"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		//[Description( "Specifies if the summary calculation selection UI is enabled." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? AllowSummaries
		{
			get
			{
				return (bool?)this.GetValue( AllowSummariesProperty );
			}
			set
			{
				this.SetValue( AllowSummariesProperty, value );
			}
		}

				#endregion // AllowSummaries

				// AS 6/9/09 NA 2009.2 Field Sizing
				#region AutoSizeOptions

		/// <summary>
		/// Identifies the <see cref="AutoSizeOptions"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public static readonly DependencyProperty AutoSizeOptionsProperty = DependencyProperty.Register("AutoSizeOptions",
			typeof(FieldAutoSizeOptions?), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets what values are considered when autosizing a field.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, this property will resolve to None and autosizing via the UI will be disabled. When set to some 
		/// other value, the end user may double click on the far edge of a field label or cell to autosize the item based on the contents 
		/// as specified by the value of this property.</p>
		/// <p class="note"><b>Note:</b> If AllowResize is resolved to false then the end user will not be able to resize the field using the auto size functionality.</p>
		/// </remarks>
		/// <seealso cref="AutoSizeScope"/>
		/// <seealso cref="Field.Width"/>
		/// <seealso cref="Field.Height"/>
		/// <seealso cref="AutoSizeOptionsProperty"/>
		/// <seealso cref="Field.PerformAutoSize()"/>
		/// <seealso cref="AllowResize"/>
		//[Description("Returns or sets what values are considered when autosizing a field.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public FieldAutoSizeOptions? AutoSizeOptions
		{
			get
			{
				return (FieldAutoSizeOptions?)this.GetValue(FieldSettings.AutoSizeOptionsProperty);
			}
			set
			{
				this.SetValue(FieldSettings.AutoSizeOptionsProperty, value);
			}
		}

				#endregion //AutoSizeOptions

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region AutoSizeScope

		/// <summary>
		/// Identifies the <see cref="AutoSizeScope"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public static readonly DependencyProperty AutoSizeScopeProperty = DependencyProperty.Register("AutoSizeScope",
			typeof(FieldAutoSizeScope), typeof(FieldSettings), new FrameworkPropertyMetadata(FieldAutoSizeScope.Default));

		/// <summary>
		/// Returns or sets a value that indicates which records are evaluated when performing an autosize of a field.
		/// </summary>
		/// <seealso cref="AutoSizeScopeProperty"/>
		/// <seealso cref="AutoSizeOptions"/>
		/// <seealso cref="Field.Width"/>
		/// <seealso cref="Field.Height"/>
		/// <seealso cref="Field.PerformAutoSize()"/>
		/// <seealso cref="AllowResize"/>
		//[Description("Returns or sets a value that indicates which records are evaluated when performing an autosize of a field.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public FieldAutoSizeScope AutoSizeScope
		{
			get
			{
				return (FieldAutoSizeScope)this.GetValue(FieldSettings.AutoSizeScopeProperty);
			}
			set
			{
				this.SetValue(FieldSettings.AutoSizeScopeProperty, value);
			}
		}

				#endregion //AutoSizeScope

                #region CellClickAction

        /// <summary>
        /// Identifies the 'CellClickAction' dependency property
        /// </summary>
        public static readonly DependencyProperty CellClickActionProperty = DependencyProperty.Register("CellClickAction",
                typeof(CellClickAction), typeof(FieldSettings), new FrameworkPropertyMetadata(CellClickAction.Default), new ValidateValueCallback(IsCellClickActionValid));

        private static bool IsCellClickActionValid(object value)
        {
            return Enum.IsDefined(typeof(CellClickAction), value);
        }

        /// <summary>
        /// Determines what happens when the user clicks on a field cell
        /// </summary>
        //[Description("Determines what happens when the user clicks on a field cell")]
        //[Category("Behavior")]
		[Bindable(true)]
		public CellClickAction CellClickAction
        {
            get
            {
                return (CellClickAction)this.GetValue(FieldSettings.CellClickActionProperty);
            }
            set
            {
                this.SetValue(FieldSettings.CellClickActionProperty, value);
            }
        }

                #endregion //CellClickAction

                #region CellContentAlignment

        /// <summary>
        /// Identifies the 'CellContentAlignment' dependency property
        /// </summary>
        public static readonly DependencyProperty CellContentAlignmentProperty = DependencyProperty.Register("CellContentAlignment",
                typeof(CellContentAlignment), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(CellContentAlignment.Default,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellContentAlignmentChanged )
				)
				, new ValidateValueCallback(IsCellContentAlignmentValid)
			);

        private static bool IsCellContentAlignmentValid(object value)
        {
            return Enum.IsDefined(typeof(CellContentAlignment), value);
        }

		// JJD 4/26/07
		// Optimization - cache the property locally
		private CellContentAlignment _cachedCellContentAlignment = CellContentAlignment.Default;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellContentAlignmentChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			CellContentAlignment newVal = (CellContentAlignment)e.NewValue;

			fieldSettings._cachedCellContentAlignment = newVal;
		}

        /// <summary>
        /// Gets/sets the relative position of the label to its cell value when the LabelLocation resolves to 'InCells'.
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This setting only applies when the <see cref="FieldLayout.LabelLocationResolved"/> is 'InCells'. This setting is ignored when there is a separate header area.</p>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.LabelLocation"/>
		/// <seealso cref="FieldLayout.LabelLocationResolved"/>
        //[Description("Gets/sets the relative position of the label to its field")]
        //[Category("Appearance")]
		[Bindable(true)]
		public CellContentAlignment CellContentAlignment
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (CellContentAlignment)this.GetValue(FieldSettings.CellContentAlignmentProperty);
				return this._cachedCellContentAlignment;
            }
            set
            {
                this.SetValue(FieldSettings.CellContentAlignmentProperty, value);
            }
        }

                #endregion //CellContentAlignment

                #region CellHeight

        /// <summary>
        /// Identifies the 'CellHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty CellHeightProperty = DependencyProperty.Register("CellHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);
		
		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellHeight = newVal;
		}

        /// <summary>
        /// The height of the cell in device-independent units (1/96th inch per unit)
        /// </summary>
		/// <remarks>This setting is ignored if <see cref="GridViewSettings.Orientation"/> is 'Vertical' and <see cref="FieldLayoutSettings.DataRecordSizingMode"/> is not set to 'Fixed', 'IndividuallySizable' or 'SizableSynchronized'.</remarks>
		/// <seealso cref="CellHeightProperty"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		/// <seealso cref="DataRecordSizingMode"/>
		//[Description("The height of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellHeightProperty);
				return this._cachedCellHeight;
            }
            set
            {
                this.SetValue(FieldSettings.CellHeightProperty, value);
            }
        }

                #endregion //CellHeight

                #region CellMaxHeight

        /// <summary>
        /// Identifies the 'CellMaxHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty CellMaxHeightProperty = DependencyProperty.Register("CellMaxHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellMaxHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellMaxHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellMaxHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellMaxHeight = newVal;
		}

        /// <summary>
        /// The maximum height of the cell in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The height of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellMaxHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellMaxHeightProperty);
				return this._cachedCellMaxHeight;
            }
            set
            {
                this.SetValue(FieldSettings.CellMaxHeightProperty, value);
            }
        }

                #endregion //CellMaxHeight

                #region CellMaxWidth

        /// <summary>
        /// Identifies the 'CellMaxWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty CellMaxWidthProperty = DependencyProperty.Register("CellMaxWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellMaxWidthChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellMaxWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellMaxWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellMaxWidth = newVal;
		}

        /// <summary>
        /// The maximum width of the cell in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The width of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellMaxWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellMaxWidthProperty);
				return this._cachedCellMaxWidth;
            }
            set
            {
                this.SetValue(FieldSettings.CellMaxWidthProperty, value);
            }
        }

                #endregion //CellMaxWidth

                #region CellMinHeight

        /// <summary>
        /// Identifies the 'CellMinHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty CellMinHeightProperty = DependencyProperty.Register("CellMinHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellMinHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellMinHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellMinHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellMinHeight = newVal;
		}

        /// <summary>
        /// The minimum height of the cell in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The height of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellMinHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellMinHeightProperty);
				return this._cachedCellMinHeight;
            }
            set
            {
                this.SetValue(FieldSettings.CellMinHeightProperty, value);
            }
        }

                #endregion //CellMinHeight

                #region CellMinWidth

        /// <summary>
        /// Identifies the 'CellMinWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty CellMinWidthProperty = DependencyProperty.Register("CellMinWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellMinWidthChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellMinWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellMinWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellMinWidth = newVal;
		}

        /// <summary>
        /// The minimum width of the cell in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The width of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellMinWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellMinWidthProperty);
				return this._cachedCellMinWidth;
            }
            set
            {
                this.SetValue(FieldSettings.CellMinWidthProperty, value);
            }
        }

                #endregion //CellMinWidth

                #region CellPresenterStyle

        /// <summary>
        /// Identifies the 'CellPresenterStyle' dependency property
        /// </summary>
        public static readonly DependencyProperty CellPresenterStyleProperty = DependencyProperty.Register("CellPresenterStyle",
			// JJD 6/30/11 - TFS80466 - Optimization 
			// added prop value caching for high volume elements
			//typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(), new ValidateValueCallback(ValidateCellPresenterStyle));
            typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellPresenterStyleChanged)), new ValidateValueCallback(ValidateCellPresenterStyle));

		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private Style _cachedCellPresenterStyle;
		
		private static void OnCellPresenterStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedCellPresenterStyle = e.NewValue as Style;
		}

		private static bool ValidateCellPresenterStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(CellPresenter).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_17" ), "CellPresenterStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(CellPresenter), "FieldSettings.CellPresenterStyle");

			return true;
		}

        /// <summary>
        /// The style for the CellPresenter
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <see cref="Cell"/>
		/// <see cref="CellPresenter"/>
		/// <see cref="CellValuePresenter"/>
		//[Description("The style for the cell")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style CellPresenterStyle
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (Style)this.GetValue(FieldSettings.CellPresenterStyleProperty);
				return _cachedCellPresenterStyle;
            }
            set
            {
                this.SetValue(FieldSettings.CellPresenterStyleProperty, value);
            }
        }

                #endregion //CellPresenterStyle

                #region CellPresenterStyleSelector

        /// <summary>
        /// Identifies the 'CellPresenterStyleSelector' dependency property
        /// </summary>
        public static readonly DependencyProperty CellPresenterStyleSelectorProperty = DependencyProperty.Register("CellPresenterStyleSelector",
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
                //typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());
                typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellPresenterStyleSelectorChanged)));
		
		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private StyleSelector _cachedCellPresenterStyleSelector;

		private static void OnCellPresenterStyleSelectorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedCellPresenterStyleSelector = e.NewValue as StyleSelector;
		}

        /// <summary>
        /// A callback used for supplying styles for CellPresenters
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <see cref="Cell"/>
		/// <see cref="CellPresenter"/>
		/// <see cref="CellValuePresenter"/>
        //[Description("A callback used for supplying styles for cells")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StyleSelector CellPresenterStyleSelector
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (StyleSelector)this.GetValue(FieldSettings.CellPresenterStyleSelectorProperty);
				return _cachedCellPresenterStyleSelector;
            }
            set
            {
                this.SetValue(FieldSettings.CellPresenterStyleSelectorProperty, value);
            }
        }

                #endregion //CellPresenterStyleSelector

                #region CellValuePresenterStyle

        /// <summary>
        /// Identifies the 'CellValuePresenterStyle' dependency property
        /// </summary>
        public static readonly DependencyProperty CellValuePresenterStyleProperty = DependencyProperty.Register("CellValuePresenterStyle",
			// JJD 6/30/11 - TFS80466 - Optimization 
			// added prop value caching for high volume elements
            //typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(), new ValidateValueCallback(ValidateCellValuePresenterStyle));
            typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellValuePresenterStyleChanged)), new ValidateValueCallback(ValidateCellValuePresenterStyle));

		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private Style _cachedCellValuePresenterStyle;
		
		private static void OnCellValuePresenterStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedCellValuePresenterStyle = e.NewValue as Style;
		}

		private static bool ValidateCellValuePresenterStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(CellValuePresenter).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( SR.GetString( "LE_ArgumentException_18" ), "CellValuePresenterStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(CellValuePresenter), "FieldSettings.CellValuePresenterStyle");

			return true;
		}

        /// <summary>
        /// The style for the CellValuePresenter
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <see cref="Cell"/>
		/// <see cref="CellPresenter"/>
		/// <see cref="CellValuePresenter"/>
		//[Description("The style for the cell")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style CellValuePresenterStyle
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (Style)this.GetValue(FieldSettings.CellValuePresenterStyleProperty);
				return _cachedCellValuePresenterStyle;
            }
            set
            {
                this.SetValue(FieldSettings.CellValuePresenterStyleProperty, value);
            }
        }

                #endregion //CellValuePresenterStyle

                #region CellValuePresenterStyleSelector

        /// <summary>
        /// Identifies the 'CellValuePresenterStyleSelector' dependency property
        /// </summary>
        public static readonly DependencyProperty CellValuePresenterStyleSelectorProperty = DependencyProperty.Register("CellValuePresenterStyleSelector",
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
                //typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());
                typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCellValuePresenterStyleSelectorChanged)));

		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private StyleSelector _cachedCellValuePresenterStyleSelector;

		private static void OnCellValuePresenterStyleSelectorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedCellValuePresenterStyleSelector = e.NewValue as StyleSelector;
		}

        /// <summary>
        /// A callback used for supplying styles for cells
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <see cref="Cell"/>
		/// <see cref="CellPresenter"/>
		/// <see cref="CellValuePresenter"/>
		//[Description("A callback used for supplying styles for cells")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StyleSelector CellValuePresenterStyleSelector
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (StyleSelector)this.GetValue(FieldSettings.CellValuePresenterStyleSelectorProperty);
				return _cachedCellValuePresenterStyleSelector;
            }
            set
            {
                this.SetValue(FieldSettings.CellValuePresenterStyleSelectorProperty, value);
            }
        }

                #endregion //CellValuePresenterStyleSelector

				#region CellVisibilityWhenGrouped

		// SSP 7/17/07 BR22919
		// Added CellVisibilityWhenGrouped property so you can hide the group-by fields when
		// the user groups records by them.
		// 

		/// <summary>
		/// Identifies the <see cref="CellVisibilityWhenGrouped"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CellVisibilityWhenGroupedProperty = DependencyProperty.Register(
			"CellVisibilityWhenGrouped",
			typeof( Visibility? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( (Visibility?)null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnCellVisibilityWhenGroupedChanged ),
				null )
			);

		/// <summary>
		/// Specifies if the group-by fields should be hidden. Default is null which is 
		/// resolved to Visible.
		/// </summary>
		/// <remarks>
		/// By default when a field is used to group records, its cells will remain visible.
		/// <b>CellVisibilityWhenGrouped</b> property can be used to control whether the
		/// field remains visible or is collapsed or hidden when records are grouped by it.
		/// You may want to hide the group-by fields since all the values of the group-by fields
		/// are displayed in the group-by records. In that case you can set this property to 
		/// either Collapsed or Hidden.
		/// </remarks>
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<Visibility>))] // AS 5/15/08 BR32816
		public Visibility? CellVisibilityWhenGrouped
		{
			get
			{
				return _cachedCellVisibilityWhenGrouped;
			}
			set
			{
				this.SetValue( CellVisibilityWhenGroupedProperty, value );
			}
		}

		private Visibility? _cachedCellVisibilityWhenGrouped;

		private static void OnCellVisibilityWhenGroupedChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings settings = (FieldSettings)dependencyObject;
			Visibility? newVal = (Visibility?)e.NewValue;
			settings._cachedCellVisibilityWhenGrouped = newVal;
		}

				#endregion // CellVisibilityWhenGrouped

                #region CellWidth

        /// <summary>
        /// Identifies the 'CellWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty CellWidthProperty = DependencyProperty.Register("CellWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCellWidthChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedCellWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnCellWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedCellWidth = newVal;
		}

        /// <summary>
        /// The width of the cell in device-independent units (1/96th inch per unit).
        /// </summary>
		/// <remarks>
		/// <para class="body">
		/// Note that the cell width is restricted to being 6 units or larger. Setting the 
		/// <b>CellWidth</b> to a smaller value will result in the cell width of 6. 
		/// Also the field cannot be resized via UI smaller than 6 units. However 
		/// note that setting <see cref="CellMaxWidth"/> property to a smaller value will override 
		/// this restriction.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> This setting is ignored if <see cref="GridViewSettings.Orientation"/> is 'Horizontal' and 
		/// <see cref="FieldLayoutSettings.DataRecordSizingMode"/> is not set to 'Fixed', 
		/// 'IndividuallySizable' or 'SizableSynchronized'.
		/// </para>
		/// </remarks>
		/// <seealso cref="CellWidthProperty"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		/// <seealso cref="DataRecordSizingMode"/>
        //[Description("The width of the cell in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double CellWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.CellWidthProperty);
				return this._cachedCellWidth;
            }
            set
            {
                this.SetValue(FieldSettings.CellWidthProperty, value);
            }
        }

                #endregion //CellWidth

				// JM 11/09 NA 2010.1 CardView
				#region CollapseWhenEmpty

		/// <summary>
        /// Identifies the <see cref="CollapseWhenEmpty"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CollapseWhenEmptyProperty = DependencyProperty.Register("CollapseWhenEmpty",
                typeof(Nullable<bool>), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback( OnCollapseWhenEmptyChanged )
			));

		// Optimization - cache the property locally
		private bool? _cachedCollapseWhenEmpty;
		private static void OnCollapseWhenEmptyChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			bool? newVal = (bool?)e.NewValue;

			fieldSettings._cachedCollapseWhenEmpty = newVal;
		}

        /// <summary>
		/// Returns/sets whether Views that support the collapsing of cells should collapse cells associated with a Field when the cells contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).
        /// </summary>
		/// <seealso cref="CollapseWhenEmptyProperty"/>
        /// <seealso cref="Field.CollapseWhenEmptyResolved"/>
        /// <seealso cref="Field"/>
		//[Description("Returns/sets whether Views that support the collapsing of cells should collapse cells associated with a Field when the cells contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).")]
        //[Category("Appearance")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))]
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public bool? CollapseWhenEmpty
        {
            get
            {
				// Optimization - use the locally cached property 
				return this._cachedCollapseWhenEmpty;
            }
            set
            {
                this.SetValue(FieldSettings.CollapseWhenEmptyProperty, value);
            }
		}

				#endregion //CollapseWhenEmpty

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region DataValueChangedHistoryLimit

        /// <summary>
        /// Identifies the 'DataValueChangedHistoryLimit' dependency property
        /// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public static readonly DependencyProperty DataValueChangedHistoryLimitProperty = DependencyProperty.Register("DataValueChangedHistoryLimit",
                typeof(int?), typeof(FieldSettings),
				new FrameworkPropertyMetadata((int?)null, null, new CoerceValueCallback(CoerceDataValueChangedHistoryLimit)));

		private static object CoerceDataValueChangedHistoryLimit(DependencyObject target, object value)
		{
			if (value == null)
				return null;

			int? i = value as int?;
			if (i.HasValue && i.Value < 0)
			{
				return 0;
			}

			return value;
		}

        /// <summary>
        /// Returns/sets the maximum number of value changes to maintain for all <see cref="Cell"/>s associated with the <see cref="Field"/>.
        /// </summary>
		/// <remarks>When this property is set to zero, no data value change history will be kept for the <see cref="Cell"/>.  If <see cref="DataValueChangedNotificationsActive"/> is set to true,
		/// the <see cref="DataPresenterBase.DataValueChanged"/> event will still be fired, but the <see cref="Infragistics.Windows.DataPresenter.Events.DataValueChangedEventArgs.ValueHistory"/> property will be null.</remarks>
		/// <p class="note"><b>Note: </b>If this property is set a value less than zero, the property will be set to zero.</p>
		/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
		/// <seealso cref="DataPresenterBase.DataValueChanged"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Events.DataValueChangedEventArgs.ValueHistory"/>		
		/// <seealso cref="DataValueChangedNotificationsActive"/>
		/// <seealso cref="DataValueChangedScope"/>
		//[Description("Returns/sets the maximum number of value changes to maintain for all Cells associated with the Field.")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public int? DataValueChangedHistoryLimit
        {
            get
            {
				return this.GetValue(FieldSettings.DataValueChangedHistoryLimitProperty) as int?;
            }
            set
            {
                this.SetValue(FieldSettings.DataValueChangedHistoryLimitProperty, value);
            }
        }

                #endregion //DataValueChangedHistoryLimit

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region DataValueChangedNotificationsActive

        /// <summary>
        /// Identifies the 'DataValueChangedNotificationsActive' dependency property
        /// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public static readonly DependencyProperty DataValueChangedNotificationsActiveProperty = DependencyProperty.Register("DataValueChangedNotificationsActive",
                typeof(bool?), typeof(FieldSettings), 
				new FrameworkPropertyMetadata((bool?)null, new PropertyChangedCallback(OnDataValueChangedNotificationsActiveChanged)));

		private static void OnDataValueChangedNotificationsActiveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
		}

        /// <summary>
		/// Returns/sets whether <see cref="DataPresenterBase.DataValueChanged"/> and <see cref="DataPresenterBase.InitializeCellValuePresenter"/> events should be raised for all <see cref="Cell"/>s associated with the <see cref="Field"/>.
        /// </summary>
		/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
		/// <seealso cref="DataPresenterBase.DataValueChanged"/>
		/// <seealso cref="DataValueChangedHistoryLimit"/>
		/// <seealso cref="DataValueChangedScope"/>
		//[Description("Returns/sets whether DataValueChanged and InitializeCellValuePresenter events should be raised for all Cells associated with the Field.")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public bool? DataValueChangedNotificationsActive
        {
            get
            {
				return this.GetValue(FieldSettings.DataValueChangedNotificationsActiveProperty) as bool?;
            }
            set
            {
                this.SetValue(FieldSettings.DataValueChangedNotificationsActiveProperty, value);
            }
        }

                #endregion //DataValueChangedNotificationsActive

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region DataValueChangedScope

        /// <summary>
        /// Identifies the 'DataValueChangedScope' dependency property
        /// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public static readonly DependencyProperty DataValueChangedScopeProperty = DependencyProperty.Register("DataValueChangedScope",
                typeof(DataValueChangedScope), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(DataValueChangedScope.Default, new PropertyChangedCallback(OnDataValueChangedScopeChanged)));

		private static void OnDataValueChangedScopeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
		}

        /// <summary>
		/// Returns/sets a value that determines the range of <see cref="DataRecord"/>s for which <see cref="DataPresenterBase.DataValueChanged"/> and <see cref="DataPresenterBase.InitializeCellValuePresenter"/> events will be raised.
        /// </summary>
		/// <remarks>
		/// <p class="body"><see cref="DataValueChangedNotificationsActive"/> must be set to true for this property to take effect.</p>
		/// </remarks>
		/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
		/// <seealso cref="DataPresenterBase.DataValueChanged"/>
		/// <seealso cref="DataValueChangedHistoryLimit"/>
		/// <seealso cref="DataValueChangedNotificationsActive"/>
		//[Description("Returns/sets a value that determines the range of DataRecords for which DataValueChanged and InitializeCellValuePresenter events will be raised.")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public DataValueChangedScope DataValueChangedScope
        {
            get
            {
				return (DataValueChangedScope)this.GetValue(FieldSettings.DataValueChangedScopeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.DataValueChangedScopeProperty, value);
            }
        }

                #endregion //DataValueChangedScope

				#region EditAsType

		/// <summary>
		/// Identifies the <see cref="EditAsType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty EditAsTypeProperty = DependencyProperty.Register(
			"EditAsType",
			typeof( Type ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				
				
				
				
				
				
				
				//new CoerceValueCallback( OnCoerceEditAsType ) 
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnEditAsTypeChanged )
				)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private Type _cachedEditAsType;

		/// <summary>
		/// Gets/sets a type that will be used to edit the data while in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">This might be used with a field whose underlying datatype is 'string' or 'object' but where it should be edited as another type, e.g. a DateTime or double.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
		/// </remarks>
		/// <seealso cref="Cell.EditAsType"/>
		public Type EditAsType
		{
			get
			{
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (Type)this.GetValue( EditAsTypeProperty );
				return this._cachedEditAsType;
			}
			set
			{
				this.SetValue( EditAsTypeProperty, value );
			}
		}

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnEditAsTypeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			Type newVal = (Type)e.NewValue;

			fieldSettings._cachedEditAsType = newVal;
		}

				#endregion // EditAsType

                #region EditorStyle

        /// <summary>
        /// Identifies the <see cref="EditorStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EditorStyleProperty = DependencyProperty.Register("EditorStyle",
                typeof(Style), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnEditorStyleChanged )
				)
				, new ValidateValueCallback(ValidateEditorStyle)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private Style _cachedEditorStyle;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnEditorStyleChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			Style newVal = (Style)e.NewValue;

			fieldSettings._cachedEditorStyle = newVal;
		}

		private static bool ValidateEditorStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(ValueEditor).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( SR.GetString( "LE_ArgumentException_19" ), "EditorStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(ValueEditor), "FieldSettings.EditorStyle");

			// JM 02-08-10 TFS26665.
			if (style.TargetType != null && style.TargetType.IsInterface)
				throw new ArgumentException(DataPresenterBase.GetString("LE_EditorStyleTargetTypeMissingOrInvalid", new object[] { }), "EditorStyle");

			return true;
		}

        /// <summary>
        /// The style for the <see cref="ValueEditor"/> used within a cell
        /// </summary>
		/// <remarks>
		/// <para class="body">The TargetType of the style must me set to a class derived from <see cref="ValueEditor"/>, otherwise an exception will be thrown.</para>
		/// <para class="body">If either the <see cref="ValueEditor.EditTemplate"/> or <b>Template</b> properties are not set on the style then default templates will be supplied based on look.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">An Argument exception is thrown if the TargetType of the style is not set to a class derived from ValueEditor.</exception>
        /// <seealso cref="Cell.EditorStyle"/> 
        /// <seealso cref="ValueEditor.EditTemplate"/> 
		/// <seealso cref="EditorStyleProperty"/>
        /// <seealso cref="EditorStyleSelector"/>
		/// <seealso cref="ValueEditor"/>
        /// <seealso cref="FieldSettings"/>
		//[Description("The style for the ValueEditor used within a cell")]
        //[Category("Behavior")]
		[Bindable(true)]
		public Style EditorStyle
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (Style)this.GetValue(FieldSettings.EditorStyleProperty);
				return this._cachedEditorStyle;
            }
            set
            {
                this.SetValue(FieldSettings.EditorStyleProperty, value);
            }
        }

                #endregion //EditorStyle

                #region EditorStyleSelector

        /// <summary>
        /// Identifies the <see cref="EditorStyleSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EditorStyleSelectorProperty = DependencyProperty.Register("EditorStyleSelector",
                typeof(StyleSelector), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnEditorStyleSelectorChanged )
				)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private StyleSelector _cachedEditorStyleSelector;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnEditorStyleSelectorChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			StyleSelector newVal = (StyleSelector)e.NewValue;

			fieldSettings._cachedEditorStyleSelector = newVal;
		}

        /// <summary>
        /// A callback used for supplying a style for the <see cref="ValueEditor"/> used within a cell
		/// </summary>
		/// <remarks>
		/// <para class="body"> The TargetType of the style must me set to a class derived from <see cref="ValueEditor"/>, otherwise an exception will be thrown.</para>
		/// <para class="body">If either the <see cref="ValueEditor.EditTemplate"/> or <b>Template</b> properties are not set on the style then default templates will be supplied based on look.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">An Argument exception is thrown if the TargetType of the style is not set to a class derived from ValueEditor.</exception>
		/// <seealso cref="ValueEditor.EditTemplate"/> 
		/// <seealso cref="EditorStyle"/>
		/// <seealso cref="EditorStyleSelectorProperty"/>
		/// <seealso cref="ValueEditor"/>
		/// <seealso cref="FieldSettings"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public StyleSelector EditorStyleSelector
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (StyleSelector)this.GetValue(FieldSettings.EditorStyleSelectorProperty);
				return this._cachedEditorStyleSelector;
            }
            set
            {
                this.SetValue(FieldSettings.EditorStyleSelectorProperty, value);
            }
        }

                #endregion //EditorStyleSelector

                #region EditorType

        /// <summary>
        /// Identifies the <see cref="EditorType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EditorTypeProperty = DependencyProperty.Register("EditorType",
                typeof(Type), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnEditorTypeChanged )
				)
				, new ValidateValueCallback(ValidateEditorType)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private Type _cachedEditorType;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnEditorTypeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			Type newVal = (Type)e.NewValue;

			fieldSettings._cachedEditorType = newVal;
		}

		private static bool ValidateEditorType(object value)
		{
			if (value == null)
				return true;

			Type type = value as Type;

			if (!typeof(ValueEditor).IsAssignableFrom(type))
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_20" ), "EditorType" );

			return true;
		}

        /// <summary>
        /// The type for the <see cref="ValueEditor"/> used within a cell
        /// </summary>
		/// <remarks>
		/// <para class="body"> The Type must be set to a class derived from <see cref="ValueEditor"/>, otherwise an exception will be thrown.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">An Argument exception is thrown if the type is not set to a class derived from ValueEditor.</exception>
        /// <seealso cref="Cell.EditorType"/> 
        /// <seealso cref="ValueEditor.EditTemplate"/> 
 		/// <seealso cref="EditorTypeProperty"/>
		/// <seealso cref="ValueEditor"/>
        /// <seealso cref="FieldSettings"/>
		//[Description("The type of the ValueEditor used within a cell")]
        //[Category("Behavior")]
		[Bindable(true)]
		public Type EditorType
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (Type)this.GetValue(FieldSettings.EditorTypeProperty);
				return this._cachedEditorType;
            }
            set
            {
                this.SetValue(FieldSettings.EditorTypeProperty, value);
            }
        }

                #endregion //EditorType

                #region ExpandedCellStyle

        /// <summary>
        /// Identifies the <see cref="ExpandedCellStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExpandedCellStyleProperty = DependencyProperty.Register("ExpandedCellStyle",
                typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata());


        /// <summary>
        /// The style for <see cref="ExpandedCellPresenter"/>s which are used to display expanded cells.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b><see cref="Field"/>'s that have a <see cref="Field.DataType"/> that implements the <see cref="System.Collections.IEnumerable"/> interface 
		/// are expandable by default. However, any <see cref="Field"/> can be made expandable by setting its <see cref="Field.IsExpandable"/> property to true. 
		/// Expandable fields have associated <see cref="ExpandableFieldRecord"/>s and <see cref="ExpandableFieldRecordPresenter"/>s to represent them in the UI. When one of these is expanded 
		/// its nested content will contain either a <see cref="RecordListControl"/> if its value implements the <b>IEnumerable</b> interface and is not a string. All 
		/// other data types including string will display an <see cref="ExpandedCellPresenter"/>.</para>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
        /// <seealso cref="DataRecord"/>
        /// <seealso cref="FieldSettings.ExpandedCellStyle"/>
        /// <seealso cref="ExpandedCellStyleProperty"/>
        /// <seealso cref="ExpandedCellStyleSelector"/>
        /// <seealso cref="ExpandedCellPresenter"/>
        /// <seealso cref="FieldSettings"/>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="Field.IsExpandable"/>
        /// <seealso cref="Record.IsExpanded"/>
        /// <seealso cref="ExpandableFieldRecordExpansionMode"/>
        /// <seealso cref="ExpandableFieldRecordHeaderDisplayMode"/>
        //[Description("The style for the cell when it is expanded")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style ExpandedCellStyle
        {
            get
            {
                return (Style)this.GetValue(FieldSettings.ExpandedCellStyleProperty);
            }
            set
            {
                this.SetValue(FieldSettings.ExpandedCellStyleProperty, value);
            }
        }

                #endregion //ExpandedCellStyle

                #region ExpandedCellStyleSelector

        /// <summary>
        /// Identifies the <see cref="ExpandedCellStyleSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExpandedCellStyleSelectorProperty = DependencyProperty.Register("ExpandedCellStyleSelector",
                typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// A callback used for supplying a style for <see cref="ExpandedCellPresenter"/>s which are used to display expanded cells.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b><see cref="Field"/>'s that have a <see cref="Field.DataType"/> that implements the <see cref="System.Collections.IEnumerable"/> interface 
		/// are expandable by default. However, any <see cref="Field"/> can be made expandable by setting its <see cref="Field.IsExpandable"/> property to true. 
		/// Expandable fields have associated <see cref="ExpandableFieldRecord"/>s and <see cref="ExpandableFieldRecordPresenter"/>s to represent them in the UI. When one of these is expanded 
		/// its nested content will contain either a <see cref="RecordListControl"/> if its value implements the <b>IEnumerable</b> interface and is not a string. All 
		/// other data types including string will display an <see cref="ExpandedCellPresenter"/>.</para>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="FieldSettings.ExpandedCellStyle"/>
		/// <seealso cref="ExpandedCellPresenter"/>
		/// <seealso cref="FieldSettings"/>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="Field.IsExpandable"/>
		/// <seealso cref="Record.IsExpanded"/>
        /// <seealso cref="ExpandedCellStyleSelectorProperty"/>
        /// <seealso cref="ExpandedCellStyle"/>
        /// <seealso cref="ExpandableFieldRecordExpansionMode"/>
        /// <seealso cref="ExpandableFieldRecordHeaderDisplayMode"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public StyleSelector ExpandedCellStyleSelector
        {
            get
            {
                return (StyleSelector)this.GetValue(FieldSettings.ExpandedCellStyleSelectorProperty);
            }
            set
            {
                this.SetValue(FieldSettings.ExpandedCellStyleSelectorProperty, value);
            }
        }

                #endregion //ExpandedCellStyleSelector

                #region ExpandableFieldRecordExpansionMode

        /// <summary>
        /// Identifies the <see cref="ExpandableFieldRecordExpansionMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExpandableFieldRecordExpansionModeProperty = DependencyProperty.Register("ExpandableFieldRecordExpansionMode",
                typeof(ExpandableFieldRecordExpansionMode), typeof(FieldSettings), new FrameworkPropertyMetadata(ExpandableFieldRecordExpansionMode.Default), new ValidateValueCallback(IsExpandableFieldRecordExpansionModeValid));

        private static bool IsExpandableFieldRecordExpansionModeValid(object value)
        {
            return Enum.IsDefined(typeof(ExpandableFieldRecordExpansionMode), value);
        }

        /// <summary>
        /// Determines how an <see cref="ExpandableFieldRecordPresenter"/> will be expanded.
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldSettings"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecordExpansionMode"/>
        /// <seealso cref="ExpandableFieldRecordExpansionModeProperty"/>
        /// <seealso cref="ExpandableFieldRecordHeaderDisplayModeProperty"/>
        /// <seealso cref="Field.ExpandableFieldRecordExpansionModeResolved"/>
        //[Description("Determines how an ExpandableFieldRecordPresenter will be expanded")]
        //[Category("Behavior")]
		[Bindable(true)]
		public ExpandableFieldRecordExpansionMode ExpandableFieldRecordExpansionMode
        {
            get
            {
                return (ExpandableFieldRecordExpansionMode)this.GetValue(FieldSettings.ExpandableFieldRecordExpansionModeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.ExpandableFieldRecordExpansionModeProperty, value);
            }
        }

                #endregion //ExpandableFieldRecordExpansionMode

                #region ExpandableFieldRecordHeaderDisplayMode

        /// <summary>
        /// Identifies the <see cref="ExpandableFieldRecordHeaderDisplayMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExpandableFieldRecordHeaderDisplayModeProperty = DependencyProperty.Register("ExpandableFieldRecordHeaderDisplayMode",
                typeof(ExpandableFieldRecordHeaderDisplayMode), typeof(FieldSettings), new FrameworkPropertyMetadata(ExpandableFieldRecordHeaderDisplayMode.Default), new ValidateValueCallback(IsExpandableFieldRecordHeaderDisplayModeValid));

        private static bool IsExpandableFieldRecordHeaderDisplayModeValid(object value)
        {
            return Enum.IsDefined(typeof(ExpandableFieldRecordHeaderDisplayMode), value);
        }

        /// <summary>
        /// Determines how an <see cref="ExpandableFieldRecordPresenter"/> will display a header.
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldSettings"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecordHeaderDisplayMode"/>
        /// <seealso cref="ExpandableFieldRecordExpansionMode"/>
        /// <seealso cref="ExpandableFieldRecordHeaderDisplayModeProperty"/>
       /// <seealso cref="Field.ExpandableFieldRecordHeaderDisplayModeResolved"/>
        //[Description("Determines how an ExpandableFieldRecordPresenter will display a header.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public ExpandableFieldRecordHeaderDisplayMode ExpandableFieldRecordHeaderDisplayMode
        {
            get
            {
                return (ExpandableFieldRecordHeaderDisplayMode)this.GetValue(FieldSettings.ExpandableFieldRecordHeaderDisplayModeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.ExpandableFieldRecordHeaderDisplayModeProperty, value);
            }
        }

                #endregion //ExpandableFieldRecordHeaderDisplayMode

				#region ExpandableFieldRecordPresenterStyle

		/// <summary>
		/// Identifies the 'ExpandableFieldRecordPresenterStyle' dependency property
		/// </summary>
		public static readonly DependencyProperty ExpandableFieldRecordPresenterStyleProperty = DependencyProperty.Register("ExpandableFieldRecordPresenterStyle",
				typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata());

		/// <summary>
		/// The style for ExpandableFieldRecords
		/// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="ExpandableFieldRecordPresenter"/>
		//[Description("The style for ExpandableFieldRecords")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Style ExpandableFieldRecordPresenterStyle
		{
			get
			{
				return (Style)this.GetValue(FieldSettings.ExpandableFieldRecordPresenterStyleProperty);
			}
			set
			{
				this.SetValue(FieldSettings.ExpandableFieldRecordPresenterStyleProperty, value);
			}
		}

				#endregion //ExpandableFieldRecordPresenterStyle

				#region ExpandableFieldRecordPresenterStyleSelector

		/// <summary>
		/// Identifies the 'ExpandableFieldRecordPresenterStyleSelector' dependency property
		/// </summary>
		public static readonly DependencyProperty ExpandableFieldRecordPresenterStyleSelectorProperty = DependencyProperty.Register("ExpandableFieldRecordPresenterStyleSelector",
				typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());

		/// <summary>
		/// A callback used for supplying styles for ExpandableFieldRecords
		/// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="ExpandableFieldRecordPresenter"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		//[Description("A callback used for supplying styles for ExpandableFieldRecords")]
		public StyleSelector ExpandableFieldRecordPresenterStyleSelector
		{
			get
			{
				return (StyleSelector)this.GetValue(FieldSettings.ExpandableFieldRecordPresenterStyleSelectorProperty);
			}
			set
			{
				this.SetValue(FieldSettings.ExpandableFieldRecordPresenterStyleSelectorProperty, value);
			}
		}

				#endregion //ExpandableFieldRecordPresenterStyleSelector

                // JJD 1/7/09 - NA 2009 vol 1 - record filtering
                #region FilterCellValuePresenterStyle

        /// <summary>
        /// Identifies the 'FilterCellValuePresenterStyle' dependency property
        /// </summary>
        public static readonly DependencyProperty FilterCellValuePresenterStyleProperty = DependencyProperty.Register("FilterCellValuePresenterStyle",
                typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(), new ValidateValueCallback(ValidateFilterCellValuePresenterStyle));

		private static bool ValidateFilterCellValuePresenterStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(FilterCellValuePresenter).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_18" ), "FilterCellValuePresenterStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(FilterCellValuePresenter), "FieldSettings.FilterCellValuePresenterStyle");

			return true;
		}

        /// <summary>
        /// The style for the FilterCellValuePresenter
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <see cref="Cell"/>
		/// <see cref="CellPresenter"/>
		/// <see cref="FilterCellValuePresenter"/>
		//[Description("The style for the filter cell")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style FilterCellValuePresenterStyle
        {
            get
            {
                return (Style)this.GetValue(FieldSettings.FilterCellValuePresenterStyleProperty);
            }
            set
            {
                this.SetValue(FieldSettings.FilterCellValuePresenterStyleProperty, value);
            }
        }

                #endregion //FilterCellValuePresenterStyle

                // JJD 1/7/09 - NA 2009 vol 1 - record filtering
                #region FilterCellEditorStyle

        /// <summary>
        /// Identifies the <see cref="FilterCellEditorStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterCellEditorStyleProperty = DependencyProperty.Register("FilterCellEditorStyle",
                typeof(Style), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnFilterCellEditorStyleChanged )
				)
				, new ValidateValueCallback(ValidateFilterCellEditorStyle)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private Style _cachedFilterCellEditorStyle;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnFilterCellEditorStyleChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			Style newVal = (Style)e.NewValue;

			fieldSettings._cachedFilterCellEditorStyle = newVal;
		}

		private static bool ValidateFilterCellEditorStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(ValueEditor).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( SR.GetString( "LE_ArgumentException_19" ), "FilterCellEditorStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(ValueEditor), "FieldSettings.FilterCellEditorStyle");

			return true;
		}

        /// <summary>
        /// The style for the <see cref="ValueEditor"/> used within a cell
        /// </summary>
		/// <remarks>
		/// <para class="body">The TargetType of the style must me set to a class derived from <see cref="ValueEditor"/>, otherwise an exception will be thrown.</para>
		/// <para class="body">If either the <see cref="ValueEditor.EditTemplate"/> or <b>Template</b> properties are not set on the style then default templates will be supplied based on look.</para>
		/// <p class="body">Refer to the <a href="xamData_Terms_Editors.html">Editors</a> topic in the Developer's Guide for an explanation of editors.</p>
		/// <p class="body">Refer to the <a href="xamData_Editing_Cell_Values.html">Editing Cell Values</a> topic in the Developer's Guide for an explanation of how editing works.</p>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">An Argument exception is thrown if the TargetType of the style is not set to a class derived from ValueEditor.</exception>
        /// <seealso cref="ValueEditor.EditTemplate"/> 
		/// <seealso cref="FilterCellEditorStyleProperty"/>
		/// <seealso cref="ValueEditor"/>
        /// <seealso cref="FieldSettings"/>
		//[Description("The style for the ValueEditor used within a cell")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style FilterCellEditorStyle
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (Style)this.GetValue(FieldSettings.FilterCellEditorStyleProperty);
				return this._cachedFilterCellEditorStyle;
            }
            set
            {
                this.SetValue(FieldSettings.FilterCellEditorStyleProperty, value);
            }
        }

                #endregion //FilterCellEditorStyle

				#region FilterClearButtonVisibility

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterClearButtonVisibility"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterClearButtonVisibilityProperty = DependencyProperty.Register(
			"FilterClearButtonVisibility",
			typeof( Visibility? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies the visibility of the filter clear button in the filter cell. Default is resolved to <b>Visible</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterClearButtonVisibility</b> property specifies the visibility of the filter clear button in the filter cell.
		/// Also note the <see cref="FieldLayoutSettings.FilterClearButtonLocation"/> property which controls whether
		/// to display filter clear button in the filter record's record selector or the filter cells or both. Note that
		/// the <i>FilterClearButtonVisibility</i> has higher precedence.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Specifies the visibility of the filter clear button in the filter cell." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<Visibility> ) )]
		public Visibility? FilterClearButtonVisibility
		{
			get
			{
				return (Visibility?)this.GetValue( FilterClearButtonVisibilityProperty );
			}
			set
			{
				this.SetValue( FilterClearButtonVisibilityProperty, value );
			}
		}

				#endregion // FilterClearButtonVisibility

				#region FilterEvaluationTrigger

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterEvaluationTrigger"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterEvaluationTriggerProperty = DependencyProperty.Register(
			"FilterEvaluationTrigger",
			typeof( FilterEvaluationTrigger ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( FilterEvaluationTrigger.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Controls when the filters are evalulated whenever the user modifies filter criteria via filter cell. Default is resolved to
		/// <b>OnCellValueChange</b> which applies the filter as you type contents in the filter cell.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Controls when the filters are evalulated whenever the user modifies filter criteria. Default is resolved to
		/// <b>OnCellValueChange</b> which evaluates the filter as the user types contents in the filter cell.
		/// </para>
		/// <para class="body">
		/// This is only applicable to filter record ui (<see cref="FieldLayoutSettings.FilterUIType"/>). To control 
		/// if filters get re-evaluated on a data record if its data changes, use the 
		/// <seealso cref="FieldLayoutSettings.ReevaluateFiltersOnDataChange"/>
		/// property.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.ReevaluateFiltersOnDataChange"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Controls when the filters are evalulated whenever the user modifies filter criteria via filter cell." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FilterEvaluationTrigger FilterEvaluationTrigger
		{
			get
			{
				return (FilterEvaluationTrigger)this.GetValue( FilterEvaluationTriggerProperty );
			}
			set
			{
				this.SetValue( FilterEvaluationTriggerProperty, value );
			}
		}

				#endregion // FilterEvaluationTrigger

				// AS - NA 11.2 Excel Style Filtering
				#region FilterLabelIconDropDownType

		/// <summary>
		/// Identifies the <see cref="FilterLabelIconDropDownType"/> dependency property.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public static readonly DependencyProperty FilterLabelIconDropDownTypeProperty = DependencyProperty.Register(
			"FilterLabelIconDropDownType",
			typeof( FilterLabelIconDropDownType ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( FilterLabelIconDropDownType.Default )
		);

		/// <summary>
		/// Specifies the type of dropdown to be displayed in the <see cref="FilterButton"/> displayed in the <see cref="LabelPresenter"/> when the <see cref="FilterUIType"/> resolves to <b>LabelIcons</b>. 'Default' is resolved to <b>SingleSelect</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">The <b>FilterLabelIconDropDownType</b> property controls the type of dropdown . 
		/// Default is resolved to <b>Combo</b>. See <see cref="Infragistics.Windows.DataPresenter.FilterLabelIconDropDownType"/>
		/// enum for available options.
		/// </para>
		/// <para class="body">
		/// Also note that <see cref="FieldSettings.FilterOperatorDropDownItems"/> property controls the list of 
		/// operators that are displayed in the operator UI. Setting it to <b>None</b> will hide the operator UI.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FilterLabelIconDropDownType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// <seealso cref="FieldSettings.FilterOperatorDefaultValue"/>
		/// <seealso cref="FieldSettings.FilterEvaluationTrigger"/>
		//[Description( "Specifies a value that is used to determine the type of dropdown that is displayed when the FilterUIType is resolved to LabelIcons." )]
		//[Category( "Behavior" )]
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		[Bindable(true)]
		public FilterLabelIconDropDownType FilterLabelIconDropDownType
		{
			get
			{
				return (FilterLabelIconDropDownType)this.GetValue( FilterLabelIconDropDownTypeProperty );
			}
			set
			{
				this.SetValue( FilterLabelIconDropDownTypeProperty, value );
			}
		}

				#endregion // FilterLabelIconDropDownType

				#region FilterOperandUIType

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterOperandUIType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterOperandUITypeProperty = DependencyProperty.Register(
			"FilterOperandUIType",
			typeof( FilterOperandUIType ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( FilterOperandUIType.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies the type of UI to use for the operand portion in a filter cell. Default is resolved to <b>Combo</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterOperandUIType</b> property controls the type of UI filter cell exposes. 
		/// Default is resolved to <b>Combo</b>. See <see cref="Infragistics.Windows.DataPresenter.FilterOperandUIType"/>
		/// enum for available options.
		/// </para>
		/// <para class="body">
		/// Also note that <see cref="FieldSettings.FilterOperatorDropDownItems"/> property controls the list of 
		/// operators that are displayed in the operator UI. Setting it to <b>None</b> will hide the operator UI.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FilterOperandUIType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// <seealso cref="FieldSettings.FilterOperatorDefaultValue"/>
		/// <seealso cref="FieldSettings.FilterEvaluationTrigger"/>
		//[Description( "Specifies the type of UI to use for the operand portion in a filter cell." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FilterOperandUIType FilterOperandUIType
		{
			get
			{
				return (FilterOperandUIType)this.GetValue( FilterOperandUITypeProperty );
			}
			set
			{
				this.SetValue( FilterOperandUITypeProperty, value );
			}
		}

				#endregion // FilterOperandUIType

				#region FilterOperatorDefaultValue

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterOperatorDefaultValue"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterOperatorDefaultValueProperty = DependencyProperty.Register(
			"FilterOperatorDefaultValue",
			typeof( ComparisonOperator? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies the default filter operator. UI for selecting filter operator in filter cell will be initialized
		/// to this value. Default is resolved to <b>StartsWith</b> except for DateTime, Boolean and numeric fields where
		/// it's resolved to <b>Equals</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterOperatorDefaultValue</b> property specifies the default filter operator. UI for selecting filter operator 
		/// in  filter cell is initialized to this value. The user can then change the operator to a different operator via
		/// the operator drop-down in the filter cell.
		/// </para>
		/// <para class="body">
		/// To control which operators are displayed in the operator drop-down list, set the <see cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// property. To hide the operator UI, set the <i>FilterDropDownItems</i> to <i>None</i>.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that you need to enable the filter record functionality by setting the 
		/// <see cref="FieldLayoutSettings.FilterUIType"/> property to <b>FilterRecord</b> and
		/// <see cref="FieldSettings.AllowRecordFiltering"/> to <b>True</b>. This property controls
		/// the initial value of the operator UI in each filter cell of the filter record. When <i>FilterUIType</i>
		/// is set to <i>LabelIcons</i>, this property has no effect.
		/// </para>
		/// </remarks>
		/// <seealso cref="ComparisonOperator"/>
		/// <seealso cref="FieldSettings.FilterOperandUIType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Specifies the default/initial filter operator." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<ComparisonOperator> ) )]
		public ComparisonOperator? FilterOperatorDefaultValue
		{
			get
			{
				return (ComparisonOperator?)this.GetValue( FilterOperatorDefaultValueProperty );
			}
			set
			{
				this.SetValue( FilterOperatorDefaultValueProperty, value );
			}
		}

				#endregion // FilterOperatorDefaultValue

				#region FilterOperatorDropDownItems

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterOperatorDropDownItems"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterOperatorDropDownItemsProperty = DependencyProperty.Register(
			"FilterOperatorDropDownItems",
			typeof( ComparisonOperatorFlags? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies which operators to display in the filter operator drop-down. Default set of operators are determined
		/// based on the data type of the field.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterOperatorDropDownItems</b> property controls which operators are displayed in the operator 
		/// drop-down list in the filter cell. To hide the operator UI, set the <i>FilterDropDownItems</i> to <i>None</i>.
		/// </para>
		/// <para class="body">
		/// <b>FilterOperatorDefaultValue</b> property specifies the default or the initial filter operator value.  
		/// The user can then change the operator to a different operator via the operator drop-down.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that you need to enable the filter record functionality by setting the 
		/// <see cref="FieldLayoutSettings.FilterUIType"/> property to <b>FilterRecord</b> and
		/// <see cref="FieldSettings.AllowRecordFiltering"/> to <b>True</b>. This property controls
		/// the list of available operators in the filter cell of the filter record. When <i>FilterUIType</i>
		/// is set to <i>LabelIcons</i>, this property has no effect as the filter
		/// record and cells are not displayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ComparisonOperator"/>
		/// <seealso cref="FieldSettings.FilterOperandUIType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Specifies which operators to display in the filter operator drop-down." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<ComparisonOperatorFlags> ) )]
		public ComparisonOperatorFlags? FilterOperatorDropDownItems
		{
			get
			{
				return (ComparisonOperatorFlags?)this.GetValue( FilterOperatorDropDownItemsProperty );
			}
			set
			{
				this.SetValue( FilterOperatorDropDownItemsProperty, value );
			}
		}

				#endregion // FilterOperatorDropDownItems

				#region FilterStringComparisonType

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterStringComparisonType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterStringComparisonTypeProperty = DependencyProperty.Register(
			"FilterStringComparisonType",
			typeof( FieldSortComparisonType ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( FieldSortComparisonType.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether filters will be case-sensitve or case insensitive. Default is <b>CaseInsensitive</b>.
		/// </summary>
		//[Description( "Specifies whether filters will be case-sensitve or case insensitive." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FieldSortComparisonType FilterStringComparisonType
		{
			get
			{
				return (FieldSortComparisonType)this.GetValue( FilterStringComparisonTypeProperty );
			}
			set
			{
				this.SetValue( FilterStringComparisonTypeProperty, value );
			}
		}

				#endregion // FilterStringComparisonType

				#region FilterComparer

        // SSP 5/3/10 TFS25788
		// 

		/// <summary>
		/// Identifies the <see cref="FilterComparer"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterComparerProperty = DependencyProperty.Register(
			"FilterComparer",
			typeof( IComparer ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null )
		);

		/// <summary>
		/// Property used for specifying a custom comparer used to compare values as part of the filtering logic.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterComparer</b> specifies a comparer that is used to compare field values when evaluating
		/// record filter conditions.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the field values are converted to type specified by 
		/// <see cref="FieldSettings.EditAsType"/> before performing comparisons.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="SortComparer"/>
		/// <seealso cref="FieldSettings.EditAsType"/>
		//[Description( "Specifies a custom comparer used for comparing values when evaluating filter conditions." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IComparer FilterComparer
		{
			get
			{
				return (IComparer)this.GetValue( FilterComparerProperty );
			}
			set
			{
				this.SetValue( FilterComparerProperty, value );
			}
		}

				#endregion // FilterComparer

				#region FilterEvaluator

		// SSP 2/29/12 TFS89053
		// 

		/// <summary>
		/// Identifies the <see cref="FilterEvaluator"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterEvaluatorProperty = DependencyProperty.Register(
			"FilterEvaluator",
			typeof( IFilterEvaluator ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null )
		);

		/// <summary>
		/// Property used for specifying a custom evaluator used to evaluate filter comparisons as part of the filtering logic.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterEvaluator</b> specifies a IFilterEvaluator interface that is used to evaluate filter comparisons when
		/// evaluating record filter conditions.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="FilterComparer"/>
		/// <seealso cref="FieldSettings.EditAsType"/>
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IFilterEvaluator FilterEvaluator
		{
			get
			{
				return (IFilterEvaluator)this.GetValue( FilterEvaluatorProperty );
			}
			set
			{
				this.SetValue( FilterEvaluatorProperty, value );
			}
		}

				#endregion // FilterEvaluator

				// AS 3/2/11 TFS66933
				#region ForceCellVirtualization

		/// <summary>
		/// Identifies the <see cref="ForceCellVirtualization"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ForceCellVirtualizationProperty = DependencyProperty.Register("ForceCellVirtualization",
				typeof(Nullable<bool>), typeof(FieldSettings),
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback(OnForceCellVirtualizationChanged)
			));

		private bool? _cachedForceCellVirtualization;

		private static void OnForceCellVirtualizationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			bool? newVal = (bool?)e.NewValue;

			fieldSettings._cachedForceCellVirtualization = newVal;
		}

		/// <summary>
		/// Allows cell elements to be virtualized even if the cell is using a custom CellValuePresenter that does not contain a ValueEditor.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is only evaluated when it is determined that a CellValuePresenter cannot be virtualized. If the 
		/// <see cref="AllowCellVirtualization"/> is set to false then this property will not be used.</p>
		/// <p class="note"><b>Note:</b> the size of the cell element will be based upon the size within a template record that is not associated with an actual dataitem.</p>
		/// </remarks>
		/// <seealso cref="ForceCellVirtualizationProperty"/>
		/// <seealso cref="Field"/>
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? ForceCellVirtualization
		{
			get
			{
				return this._cachedForceCellVirtualization;
			}
			set
			{
				this.SetValue(FieldSettings.ForceCellVirtualizationProperty, value);
			}
		}

				#endregion //ForceCellVirtualization

                #region GroupByComparer

        /// <summary>
        /// Identifies the <see cref="GroupByComparer"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GroupByComparerProperty = DependencyProperty.Register("GroupByComparer",
                typeof(IComparer), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Property used for specifying a custom comparer to sort <see cref="GroupByRecord"/>s.
        /// </summary>
        /// <remarks>
        /// <p class="body">This IComparer instance will be used for sorting group-by records associated with this field if this field were a group-by field.</p>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByEvaluator"/> <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SortComparer"/>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Grouping.html">About Grouping</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
		/// </remarks>
        /// <seealso cref="AllowGroupBy"/>
        /// <seealso cref="Field.AllowGroupByResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.IsGroupBy"/>
        /// <seealso cref="GroupByEvaluator"/>
        /// <seealso cref="GroupByMode"/>
        /// <seealso cref="GroupByRecordPresenterStyle"/>
        /// <seealso cref="GroupByRecordPresenterStyleSelector"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public IComparer GroupByComparer
        {
            get
            {
                return (IComparer)this.GetValue(FieldSettings.GroupByComparerProperty);
            }
            set
            {
                this.SetValue(FieldSettings.GroupByComparerProperty, value);
            }
        }

                #endregion //GroupByComparer

                #region GroupByEvaluator

        /// <summary>
        /// Identifies the <see cref="GroupByEvaluator"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GroupByEvaluatorProperty = DependencyProperty.Register("GroupByEvaluator",
                typeof(IGroupByEvaluator), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Property used for specifying a custom evaluator for group-by records which determines group breaks.
        /// </summary>
        /// <remarks>
		/// <para class="body">The GroupByEvaluator needs to be logically consistent with the sorted order of the <see cref="DataRecord"/>s which can be controlled via the <see cref="GroupByComparer"/> or <see cref="SortComparer"/> properties.</para>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Grouping.html">About Grouping</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
		/// </remarks>
        /// <seealso cref="AllowGroupBy"/>
        /// <seealso cref="Field.AllowGroupByResolved"/>
        /// <seealso cref="FieldSettings"/>
        /// <seealso cref="Field.IsGroupBy"/>
        /// <seealso cref="GroupByComparer"/>
        /// <seealso cref="SortComparer"/>
        /// <seealso cref="GroupByMode"/>
        /// <seealso cref="GroupByRecordPresenterStyle"/>
        /// <seealso cref="GroupByRecordPresenterStyleSelector"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
        /// <seealso cref="IGroupByEvaluator"/>
        /// <seealso cref="SortComparer"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public IGroupByEvaluator GroupByEvaluator
        {
            get
            {
                return (IGroupByEvaluator)this.GetValue(FieldSettings.GroupByEvaluatorProperty);
            }
            set
            {
                this.SetValue(FieldSettings.GroupByEvaluatorProperty, value);
            }
        }

                #endregion //GroupByEvaluator

                #region GroupByMode

        /// <summary>
        /// Identifies the <see cref="GroupByMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty GroupByModeProperty = DependencyProperty.Register("GroupByMode",
                typeof(FieldGroupByMode), typeof(FieldSettings), new FrameworkPropertyMetadata(FieldGroupByMode.Default), new ValidateValueCallback(IsGroupByModeValid));

        private static bool IsGroupByModeValid(object value)
        {
            return Enum.IsDefined(typeof(FieldGroupByMode), value);
        }

        /// <summary>
        /// Determines how <see cref="DataRecord"/>s are grouped
        /// </summary>
        /// <remarks>
		/// <para class="body">If a <see cref="GroupByComparer"/> is specified then this setting is ignored. Otherwise, an internal <see cref="IGroupByEvaluator"/> implementation will be supplied based on the <see cref="FieldGroupByMode"/> value.
		/// </para>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Grouping.html">About Grouping</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
		/// </remarks>
		/// <seealso cref="AllowGroupBy"/>
        /// <seealso cref="Field.AllowGroupByResolved"/>
        /// <seealso cref="FieldGroupByMode"/>
        /// <seealso cref="FieldSettings"/>
        /// <seealso cref="Field.IsGroupBy"/>
        /// <seealso cref="GroupByComparer"/>
        /// <seealso cref="GroupByEvaluator"/>
        /// <seealso cref="FieldSettings.GroupByMode"/>
        /// <seealso cref="GroupByModeProperty"/>
        /// <seealso cref="Field.GroupByModeResolved"/>
        /// <seealso cref="GroupByRecordPresenterStyle"/>
        /// <seealso cref="GroupByRecordPresenterStyleSelector"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
        /// <seealso cref="IGroupByEvaluator"/>
        /// <seealso cref="SortComparer"/>
        //[Description("Determines how DataRecord are grouped")]
        //[Category("Behavior")]
		[Bindable(true)]
		public FieldGroupByMode GroupByMode
        {
            get
            {
                return (FieldGroupByMode)this.GetValue(FieldSettings.GroupByModeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.GroupByModeProperty, value);
            }
        }

                #endregion //GroupByMode

                #region GroupByRecordPresenterStyle

        /// <summary>
        /// Identifies the 'GroupByRecordPresenterStyle' dependency property
        /// </summary>
        public static readonly DependencyProperty GroupByRecordPresenterStyleProperty = DependencyProperty.Register("GroupByRecordPresenterStyle",
                typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// The style for the GroupByRecordPresenter
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="AllowGroupBy"/>
		/// <seealso cref="Field.AllowGroupByResolved"/>
		/// <seealso cref="FieldGroupByMode"/>
		/// <seealso cref="FieldSettings"/>
		/// <seealso cref="Field.IsGroupBy"/>
		/// <seealso cref="GroupByComparer"/>
		/// <seealso cref="GroupByEvaluator"/>
		/// <seealso cref="FieldSettings.GroupByMode"/>
		/// <seealso cref="GroupByModeProperty"/>
		/// <seealso cref="Field.GroupByModeResolved"/>
		/// <seealso cref="GroupByRecordPresenterStyle"/>
		/// <seealso cref="GroupByRecordPresenterStyleSelector"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
		/// <seealso cref="IGroupByEvaluator"/>
		/// <seealso cref="SortComparer"/>
		//[Description("The style for the groupBy")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style GroupByRecordPresenterStyle
        {
            get
            {
                return (Style)this.GetValue(FieldSettings.GroupByRecordPresenterStyleProperty);
            }
            set
            {
                this.SetValue(FieldSettings.GroupByRecordPresenterStyleProperty, value);
            }
        }

                #endregion //GroupByRecordPresenterStyle

                #region GroupByRecordPresenterStyleSelector

        /// <summary>
        /// Identifies the 'GroupByRecordPresenterStyleSelector' dependency property
        /// </summary>
        public static readonly DependencyProperty GroupByRecordPresenterStyleSelectorProperty = DependencyProperty.Register("GroupByRecordPresenterStyleSelector",
                typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// A callback used for supplying styles for groupBys
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="AllowGroupBy"/>
		/// <seealso cref="Field.AllowGroupByResolved"/>
		/// <seealso cref="FieldGroupByMode"/>
		/// <seealso cref="FieldSettings"/>
		/// <seealso cref="Field.IsGroupBy"/>
		/// <seealso cref="GroupByComparer"/>
		/// <seealso cref="GroupByEvaluator"/>
		/// <seealso cref="FieldSettings.GroupByMode"/>
		/// <seealso cref="GroupByModeProperty"/>
		/// <seealso cref="Field.GroupByModeResolved"/>
		/// <seealso cref="GroupByRecordPresenterStyle"/>
		/// <seealso cref="GroupByRecordPresenterStyleSelector"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByArea"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.GroupByAreaStyle"/>
		/// <seealso cref="IGroupByEvaluator"/>
		/// <seealso cref="SortComparer"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        //[Description("A callback used for supplying styles for groupBys")]
        public StyleSelector GroupByRecordPresenterStyleSelector
        {
            get
            {
                return (StyleSelector)this.GetValue(FieldSettings.GroupByRecordPresenterStyleSelectorProperty);
            }
            set
            {
                this.SetValue(FieldSettings.GroupByRecordPresenterStyleSelectorProperty, value);
            }
        }

                #endregion //GroupByRecordPresenterStyleSelector

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region Height

		/// <summary>
		/// Identifies the <see cref="Height"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height",
			typeof(FieldLength?), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a FieldLength instance that represents the default height of the field.
		/// </summary>
		/// <seealso cref="HeightProperty"/>
		/// <seealso cref="Width"/>
		//[Description("Returns or sets a FieldLength instance that represents the default height of the field.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public FieldLength? Height
		{
			get
			{
				return (FieldLength?)this.GetValue(Field.HeightProperty);
			}
			set
			{
				this.SetValue(Field.HeightProperty, value);
			}
		}

				#endregion //Height

				#region InvalidValueBehavior

		/// <summary>
		/// Identifies the <see cref="InvalidValueBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InvalidValueBehaviorProperty = DependencyProperty.Register(
			"InvalidValueBehavior",
			typeof( InvalidValueBehavior ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( InvalidValueBehavior.Default,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnInvalidValueBehaviorChanged )
			) 
		);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private InvalidValueBehavior _cachedInvalidValueBehavior = InvalidValueBehavior.Default;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnInvalidValueBehaviorChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			InvalidValueBehavior newVal = (InvalidValueBehavior)e.NewValue;

			fieldSettings._cachedInvalidValueBehavior = newVal;
		}

		/// <summary>
		/// Determines what action to take when attempting to end edit mode with an invalid value.
		/// </summary>
		//[Description( "Determines what action to take when attempting to end edit mode with an invalid value." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public InvalidValueBehavior InvalidValueBehavior
		{
			get
			{
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (InvalidValueBehavior)this.GetValue( InvalidValueBehaviorProperty );
				return this._cachedInvalidValueBehavior;
			}
			set
			{
				this.SetValue( InvalidValueBehaviorProperty, value );
			}
		}

				#endregion // InvalidValueBehavior

                #region LabelClickAction

        /// <summary>
        /// Identifies the 'LabelClickAction' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelClickActionProperty = DependencyProperty.Register("LabelClickAction",
                typeof(LabelClickAction), typeof(FieldSettings), new FrameworkPropertyMetadata(LabelClickAction.Default), new ValidateValueCallback(IsLabelClickActionValid));

        private static bool IsLabelClickActionValid(object value)
        {
            return Enum.IsDefined(typeof(LabelClickAction), value);
        }

        /// <summary>
        /// Determines what happens when the user clicks on a field label
        /// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Sorting.html">About Sorting</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
        /// <para class="note"><b>Note:</b> when using the SortByOneFieldOnlyTriState or SortByMultipleFieldsTriState settings, if the 'Ctrl' key is pressed during a click, or the <see cref="Field.IsGroupBy"/> property is true, then the Field will not cycle thru the unsorted state. Instead it will cycle thru the ascending and descending states only.</para>
        /// </remarks>
        //[Description("Determines what happens when the user clicks on a field label")]
        //[Category("Behavior")]
		[Bindable(true)]
		public LabelClickAction LabelClickAction
        {
            get
            {
                return (LabelClickAction)this.GetValue(FieldSettings.LabelClickActionProperty);
            }
            set
            {
                this.SetValue(FieldSettings.LabelClickActionProperty, value);
            }
        }

                #endregion //LabelClickAction

                #region LabelHeight

        /// <summary>
        /// Identifies the 'LabelHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelHeightProperty = DependencyProperty.Register("LabelHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelHeight = newVal;
		}

        /// <summary>
        /// The height of the label in device-independent units (1/96th inch per unit)
        /// </summary>
		/// <remarks>This setting is ignored if <see cref="GridViewSettings.Orientation"/> is 'Vertical' and <see cref="FieldLayoutSettings.DataRecordSizingMode"/> is not set to 'Fixed', 'IndividuallySizable' or 'SizableSynchronized'.</remarks>
		/// <seealso cref="LabelHeightProperty"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		/// <seealso cref="DataRecordSizingMode"/>
		//[Description("The height of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (double)this.GetValue(FieldSettings.LabelHeightProperty);
				return this._cachedLabelHeight;
            }
            set
            {
                this.SetValue(FieldSettings.LabelHeightProperty, value);
            }
        }

                #endregion //LabelHeight

                #region LabelMaxHeight

        /// <summary>
        /// Identifies the 'LabelMaxHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelMaxHeightProperty = DependencyProperty.Register("LabelMaxHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelMaxHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelMaxHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelMaxHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelMaxHeight = newVal;
		}

        /// <summary>
        /// The maximum height of the label in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The height of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelMaxHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (double)this.GetValue(FieldSettings.LabelMaxHeightProperty);
				return this._cachedLabelMaxHeight;
            }
            set
            {
                this.SetValue(FieldSettings.LabelMaxHeightProperty, value);
            }
        }

                #endregion //LabelMaxHeight

                #region LabelMaxWidth

        /// <summary>
        /// Identifies the 'LabelMaxWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelMaxWidthProperty = DependencyProperty.Register("LabelMaxWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelMaxWidthChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelMaxWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelMaxWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelMaxWidth = newVal;
		}

        /// <summary>
        /// The maximum width of the label in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The width of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelMaxWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (double)this.GetValue(FieldSettings.LabelMaxWidthProperty);
				return this._cachedLabelMaxWidth;
            }
            set
            {
                this.SetValue(FieldSettings.LabelMaxWidthProperty, value);
            }
        }

                #endregion //LabelMaxWidth

                #region LabelMinHeight

        /// <summary>
        /// Identifies the 'LabelMinHeight' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelMinHeightProperty = DependencyProperty.Register("LabelMinHeight",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelMinHeightChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelMinHeight = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelMinHeightChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelMinHeight = newVal;
		}

        /// <summary>
        /// The minimum height of the label in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The height of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelMinHeight
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (double)this.GetValue(FieldSettings.LabelMinHeightProperty);
				return this._cachedLabelMinHeight;
            }
            set
            {
                this.SetValue(FieldSettings.LabelMinHeightProperty, value);
            }
        }

                #endregion //LabelMinHeight

                #region LabelMinWidth

        /// <summary>
        /// Identifies the 'LabelMinWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelMinWidthProperty = DependencyProperty.Register("LabelMinWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelMinWidthChanged )
				), 
				new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelMinWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelMinWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelMinWidth = newVal;
		}

        /// <summary>
        /// The minimum width of the label in device-independent units (1/96th inch per unit)
        /// </summary>
        //[Description("The width of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelMinWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (double)this.GetValue(FieldSettings.LabelMinWidthProperty);
				return this._cachedLabelMinWidth;
            }
            set
            {
                this.SetValue(FieldSettings.LabelMinWidthProperty, value);
            }
        }

                #endregion //LabelMinWidth

                #region LabelPresenterStyle

        /// <summary>
        /// Identifies the 'LabelPresenterStyle' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelPresenterStyleProperty = DependencyProperty.Register("LabelPresenterStyle",
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
                //typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(), new ValidateValueCallback(ValidateLabelPresenterStyle));
                typeof(Style), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelPresenterStyleChanged)), new ValidateValueCallback(ValidateLabelPresenterStyle));

		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private Style _cachedLabelPresenterStyle;
		
		private static void OnLabelPresenterStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedLabelPresenterStyle = e.NewValue as Style;
		}

		private static bool ValidateLabelPresenterStyle(object value)
		{
			if (value == null)
				return true;

			Style style = value as Style;

			Debug.Assert(style != null);

			if (style == null)
				return false;

            // JJD 4/23/09 - TFS17037
            // Call Utilities.VerifyTargetTypeOfStyle method instead
            //if (style.TargetType == null ||
            //    !typeof(LabelPresenter).IsAssignableFrom(style.TargetType))
            //    throw new ArgumentException( SR.GetString( "LE_ArgumentException_21" ), "LabelPresenterStyle" );
            Utilities.ValidateTargetTypeOfStyle(style, typeof(LabelPresenter), "FieldSettings.LabelPresenterStyle");

			return true;
		}

        /// <summary>
        /// The style for the label
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="Field.Label"/>
		/// <seealso cref="LabelPresenter"/>
        //[Description("The style for the label")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style LabelPresenterStyle
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (Style)this.GetValue(FieldSettings.LabelPresenterStyleProperty);
				return _cachedLabelPresenterStyle;
            }
            set
            {
                this.SetValue(FieldSettings.LabelPresenterStyleProperty, value);
            }
        }

                #endregion //LabelPresenterStyle

                #region LabelPresenterStyleSelector

        /// <summary>
        /// Identifies the 'LabelPresenterStyleSelector' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelPresenterStyleSelectorProperty = DependencyProperty.Register("LabelPresenterStyleSelector",
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
                //typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata());
                typeof(StyleSelector), typeof(FieldSettings), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLabelPresenterStyleSelectorChanged)));
		
		// JJD 6/30/11 - TFS80466 - Optimization 
		// added prop value caching for high volume elements
		private StyleSelector _cachedLabelPresenterStyleSelector;

		private static void OnLabelPresenterStyleSelectorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldSettings instance = target as FieldSettings;

			instance._cachedLabelPresenterStyleSelector = e.NewValue as StyleSelector;
		}

        /// <summary>
        /// A callback used for supplying styles for labels
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="Field.Label"/>
		/// <seealso cref="LabelPresenter"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        //[Description("A callback used for supplying styles for labels")]
        public StyleSelector LabelPresenterStyleSelector
        {
            get
            {
				// JJD 6/30/11 - TFS80466 - Optimization 
				// added prop value caching for high volume elements
				//return (StyleSelector)this.GetValue(FieldSettings.LabelPresenterStyleSelectorProperty);
				return _cachedLabelPresenterStyleSelector;
            }
            set
            {
                this.SetValue(FieldSettings.LabelPresenterStyleSelectorProperty, value);
            }
        }

                #endregion //LabelPresenterStyleSelector

                // JJD 2/7/08 - BR30444 - added
                #region LabelTextAlignment

        /// <summary>
        /// Identifies the <see cref="LabelTextAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelTextAlignmentProperty = DependencyProperty.Register("LabelTextAlignment",
                typeof(TextAlignment?), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback( OnLabelTextAlignmentChanged )
			));

		// JJD 4/26/07
		// Optimization - cache the property locally
        private TextAlignment? _cachedLabelTextAlignment;


		
		
		
		
		
		private static void OnLabelTextAlignmentChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
            TextAlignment? newVal = (TextAlignment?)e.NewValue;

			fieldSettings._cachedLabelTextAlignment = newVal;
		}

        /// <summary>
        /// Determines how text is aligned within the LabelPresenter.
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This setting only applies if the <see cref="Field"/>'s <see cref="Field.Label"/> is a string and the ContentTemplate property of <see cref="LabelPresenter"/> is not set.</p>
		/// </remarks>
		/// <seealso cref="LabelTextAlignmentProperty"/>
        /// <seealso cref="Field.LabelTextAlignmentResolved"/>
        /// <seealso cref="Field"/>
        //[Description("Determines how text is aligned within the LabelPresenter.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<TextAlignment>))] // AS 5/15/08 BR32816
		public TextAlignment? LabelTextAlignment
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool?)this.GetValue(FieldSettings.LabelTextAlignmentProperty);
				return this._cachedLabelTextAlignment;
            }
            set
            {
                this.SetValue(FieldSettings.LabelTextAlignmentProperty, value);
            }
        }

                #endregion //LabelTextAlignment

                // JJD 2/7/08 - BR30444 - added
                #region LabelTextTrimming

        /// <summary>
        /// Identifies the <see cref="LabelTextTrimming"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelTextTrimmingProperty = DependencyProperty.Register("LabelTextTrimming",
                typeof(TextTrimming?), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback( OnLabelTextTrimmingChanged )
			));

		// JJD 4/26/07
		// Optimization - cache the property locally
        private TextTrimming? _cachedLabelTextTrimming;


		
		
		
		
		
		private static void OnLabelTextTrimmingChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
            TextTrimming? newVal = (TextTrimming?)e.NewValue;

			fieldSettings._cachedLabelTextTrimming = newVal;
		}

        /// <summary>
        /// Determines how text is trimmed within the LabelPresenter if there isn't enough space to display it entirely.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This setting only applies if the <see cref="Field"/>'s <see cref="Field.Label"/> is a string and the ContentTemplate property of <see cref="LabelPresenter"/> is not set.</p>
        /// </remarks>
        /// <seealso cref="LabelTextTrimmingProperty"/>
        /// <seealso cref="Field.LabelTextTrimmingResolved"/>
        /// <seealso cref="Field"/>
        //[Description("Determines how text is trimmed within the LabelPresenter if there isn't enough space to display it entirely.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<TextTrimming>))] // AS 5/15/08 BR32816
		public TextTrimming? LabelTextTrimming
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool?)this.GetValue(FieldSettings.LabelTextTrimmingProperty);
				return this._cachedLabelTextTrimming;
            }
            set
            {
                this.SetValue(FieldSettings.LabelTextTrimmingProperty, value);
            }
        }

                #endregion //LabelTextTrimming

                // JJD 2/7/08 - BR30444 - added
                #region LabelTextWrapping

        /// <summary>
        /// Identifies the <see cref="LabelTextWrapping"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelTextWrappingProperty = DependencyProperty.Register("LabelTextWrapping",
                typeof(TextWrapping?), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(null,
				new PropertyChangedCallback( OnLabelTextWrappingChanged )
			));

		// JJD 4/26/07
		// Optimization - cache the property locally
        private TextWrapping? _cachedLabelTextWrapping;


		
		
		
		
		
		private static void OnLabelTextWrappingChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
            TextWrapping? newVal = (TextWrapping?)e.NewValue;

			fieldSettings._cachedLabelTextWrapping = newVal;
		}

        /// <summary>
        /// Determines whether text is wrapped within the LabelPresenter if there isn't enough width to display it on a single line.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This setting only applies if the <see cref="Field"/>'s <see cref="Field.Label"/> is a string and the ContentTemplate property of <see cref="LabelPresenter"/> is not set.</p>
        /// </remarks>
        /// <seealso cref="LabelTextWrappingProperty"/>
        /// <seealso cref="Field.LabelTextWrappingResolved"/>
        /// <seealso cref="Field"/>
        //[Description("Determines whether text is wrapped within the LabelPresenter if there isn't enough width to display it on a single line.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<TextWrapping>))] // AS 5/15/08 BR32816
		public TextWrapping? LabelTextWrapping
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool?)this.GetValue(FieldSettings.LabelTextWrappingProperty);
				return this._cachedLabelTextWrapping;
            }
            set
            {
                this.SetValue(FieldSettings.LabelTextWrappingProperty, value);
            }
        }

                #endregion //LabelTextWrapping

                #region LabelWidth

        /// <summary>
        /// Identifies the 'LabelWidth' dependency property
        /// </summary>
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register("LabelWidth",
                typeof(double), typeof(FieldSettings), 
				new FrameworkPropertyMetadata(double.NaN,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnLabelWidthChanged )
				)
				, new ValidateValueCallback(ValidatePixelValue)
			);

		// JJD 4/26/07
		// Optimization - cache the property locally
		private double _cachedLabelWidth = double.NaN;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelWidthChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldSettings fieldSettings = (FieldSettings)dependencyObject;
			double newVal = (double)e.NewValue;

			fieldSettings._cachedLabelWidth = newVal;
		}

        /// <summary>
        /// The width of the label in device-independent units (1/96th inch per unit)
        /// </summary>
		/// <para class="body">
		/// Note that the label width is restricted to being 6 units or larger. Setting the 
		/// <b>LabelWidth</b> to a smaller value will result in the label width of 6. 
		/// Also the field cannot be resized via UI smaller than 6 units. However 
		/// note that setting <see cref="LabelMaxWidth"/> property to a smaller value will override 
		/// this restriction.
		/// </para>
		/// <remarks>This setting is ignored if <see cref="GridViewSettings.Orientation"/> is 'Horizontal' and <see cref="FieldLayoutSettings.DataRecordSizingMode"/> is not set to 'Fixed', 'IndividuallySizable' or 'SizableSynchronized'.</remarks>
		/// <seealso cref="LabelWidthProperty"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		/// <seealso cref="DataRecordSizingMode"/>
		//[Description("The width of the label in device-independent units (1/96th inch per unit)")]
		[Bindable(true)]
		public double LabelWidth
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
                //return (double)this.GetValue(FieldSettings.LabelWidthProperty);
				return this._cachedLabelWidth;
            }
            set
            {
                this.SetValue(FieldSettings.LabelWidthProperty, value);
            }
        }

                #endregion //LabelWidth

				// JJD 7/15/10 - TFS35815 - added
				#region NonSpecificNotificationBehavior

		/// <summary>
		/// Identifies the <see cref="NonSpecificNotificationBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NonSpecificNotificationBehaviorProperty = DependencyProperty.Register("NonSpecificNotificationBehavior",
			typeof(NonSpecificNotificationBehavior), typeof(FieldSettings), new FrameworkPropertyMetadata(NonSpecificNotificationBehavior.Default));

		/// <summary>
		/// Determines if values are refreshed when a notification is received that a change has occured for a DataRecord but the notification doesn't specify which field value has been changed.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: non-specific notifications can be received in one of 2 ways. The first is if the parent list implements <see cref="IBindingList"/> and raises
		///  a ListChanged event with a <see cref="ListChangedType"/> of 'ItemChanged' and a null PropertyDescriptor. The second way is if the data item implements <see cref="INotifyPropertyChanged"/> and 
		///  raises a PropertyChanged event with a null or empty 'PropertyName'.</para>
		/// </remarks>
		/// <seealso cref="NonSpecificNotificationBehaviorProperty"/>
		/// <seealso cref="Field.NonSpecificNotificationBehaviorResolved"/>
		//[Description("Determines if values are refreshed when a notification is received that a change has occured for a DataRecord but the notification doesn't specify which field values have been changed.")]
		//[Category("Behavior")]
		public NonSpecificNotificationBehavior NonSpecificNotificationBehavior
		{
			get
			{
				return (NonSpecificNotificationBehavior)this.GetValue(FieldSettings.NonSpecificNotificationBehaviorProperty);
			}
			set
			{
				this.SetValue(FieldSettings.NonSpecificNotificationBehaviorProperty, value);
			}
		}

				#endregion //NonSpecificNotificationBehavior

				#region SupportDataErrorInfo

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the <see cref="SupportDataErrorInfo"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName=FeatureInfo.FeatureName_IDataErrorInfo, Version=FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty SupportDataErrorInfoProperty = DependencyProperty.Register(
  			"SupportDataErrorInfo",
			typeof( bool? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) 
		);

		/// <summary>
		/// Specifies whether to display field error information provided by the IDataErrorInfo 
		/// implementation of the underlying data items from the bound data source. Default value 
		/// is resolved based on the FieldLayoutSettings'
		/// <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property setting.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SupportDataErrorInfo</b> property specifies whether to display field error information provided
		/// by the data items associated with data records. Data items provide error information by
		/// implementing <b>IDataErrorInfo</b> interface. The interface provides error information for
		/// the the entire data item as well as for each individual field. This property controls whether
		/// the data presenter displays the field error in the associated cell.
		/// </para>
		/// <para class="body">
		/// Default value of this property is resolved based on the value of FieldLayoutSettings' 
		/// <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property. If it's set to a value
		/// that displays cell error information, then this property will be resolved to <i>True</i>.
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether to display field error information provided by IDataErrorInfo." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName=FeatureInfo.FeatureName_IDataErrorInfo, Version=FeatureInfo.Version_9_2 )]
		public bool? SupportDataErrorInfo
		{
			get
			{
				return (bool?)this.GetValue( SupportDataErrorInfoProperty );
			}
			set
			{
				this.SetValue( SupportDataErrorInfoProperty, value );
			}
		}

				#endregion // SupportDataErrorInfo

                #region SortComparer

        /// <summary>
        /// Identifies the <see cref="SortComparer"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SortComparerProperty = DependencyProperty.Register("SortComparer",
                typeof(IComparer), typeof(FieldSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Property used for specifying a custom comparer used to sort <see cref="DataRecord"/>s.
        /// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Sorting.html">About Sorting</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
        /// <seealso cref="FieldSettings"/>
        /// <seealso cref="GroupByComparer"/>
        /// <seealso cref="FieldSettings.SortComparer"/>
        /// <seealso cref="SortComparerProperty"/>
        /// <seealso cref="Field.SortComparerResolved"/>
        /// <seealso cref="SortComparisonType"/>
        /// <seealso cref="Field.SortStatus"/>
        //[Description("Property used for specifying a custom comparer used to sort DataRecords")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public IComparer SortComparer
        {
            get
            {
                return (IComparer)this.GetValue(FieldSettings.SortComparerProperty);
            }
            set
            {
                this.SetValue(FieldSettings.SortComparerProperty, value);
            }
        }

                #endregion //SortComparer

                #region SortComparisonType

        /// <summary>
        /// Identifies the <see cref="SortComparisonType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SortComparisonTypeProperty = DependencyProperty.Register("SortComparisonType",
                typeof(FieldSortComparisonType), typeof(FieldSettings), new FrameworkPropertyMetadata(FieldSortComparisonType.Default), new ValidateValueCallback(IsSortComparisonTypeValid));

        private static bool IsSortComparisonTypeValid(object value)
        {
            return Enum.IsDefined(typeof(FieldSortComparisonType), value);
        }

        /// <summary>
        /// Determines how DataRecords are sorted
        /// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamDataPresenter_About_Sorting.html">About Sorting</a> topic in the Developer's Guide for an explanation of how this property is used.</p>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
        /// <seealso cref="FieldSettings"/>
        /// <seealso cref="FieldSortComparisonType"/>
        /// <seealso cref="GroupByComparer"/>
        /// <seealso cref="SortComparer"/>
        /// <seealso cref="FieldSettings.SortComparisonType"/>
        /// <seealso cref="SortComparisonTypeProperty"/>
        /// <seealso cref="Field.SortComparisonTypeResolved"/>
        /// <seealso cref="Field.SortStatus"/>
        //[Description("Determines how DataRecords are sorted")]
        //[Category("Behavior")]
		[Bindable(true)]
		public FieldSortComparisonType SortComparisonType
        {
            get
            {
                return (FieldSortComparisonType)this.GetValue(FieldSettings.SortComparisonTypeProperty);
            }
            set
            {
                this.SetValue(FieldSettings.SortComparisonTypeProperty, value);
            }
        }

                #endregion //SortComparisonType

				#region SummaryDisplayArea

		
		

		/// <summary>
		/// Identifies the <see cref="SummaryDisplayArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryDisplayAreaProperty = DependencyProperty.Register(
			"SummaryDisplayArea",
			typeof( SummaryDisplayAreas? ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies if and where summaries associated with this field are displayed. 
		/// Default is resolved to <b>TopLevelOnly</b> and <b>InGroupByRecords</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryDisplayArea</b> property controls if and where summary calculation results for this
		/// field are displayed. You can also specify summary display area for individual summaries by
		/// setting <see cref="SummaryDefinition"/>'s <see cref="SummaryDefinition.DisplayArea"/> property.
		/// </para>
		/// <para class="body">
		/// By default this property is resolved to <b>TopLevelOnly</b> and <b>InGroupByRecords</b>, which 
		/// means that the summaries will be displayed in each group-by record (if there are group-by records)
		/// and for the top level records. See <see cref="SummaryDisplayAreas"/> enum for more information.
		/// </para>
		/// <para class="body">
		/// To enable the UI that lets the user select summary calculations to perform on fields, set the
		/// FieldSettings' <see cref="FieldSettings.AllowSummaries"/> property. See 
		/// <see cref="FieldSettings.AllowSummaries"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryDefinition.DisplayArea"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		/// <seealso cref="FieldSettings.SummaryUIType"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		//[Description( "Specifies if and where summaries associated with this field are displayed." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<SummaryDisplayAreas>))] // AS 5/15/08 BR32816
		public SummaryDisplayAreas? SummaryDisplayArea
		{
			get
			{
				return (SummaryDisplayAreas?)this.GetValue( SummaryDisplayAreaProperty );
			}
			set
			{
				this.SetValue( SummaryDisplayAreaProperty, value );
			}
		}

				#endregion // SummaryDisplayArea

				#region SummaryUIType

		
		

		/// <summary>
		/// Identifies the <see cref="SummaryUIType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryUITypeProperty = DependencyProperty.Register(
			"SummaryUIType",
			typeof( SummaryUIType ),
			typeof( FieldSettings ),
			new FrameworkPropertyMetadata( SummaryUIType.Default )
			);

		/// <summary>
		/// Specifies options for the summary selection user interface.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryUIType</b> property is used to specify various options for summary selection UI.
		/// See <see cref="FieldSettings.AllowSummaries"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		//[Description( "Specifies options for the summary selection user interface." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public SummaryUIType SummaryUIType
		{
			get
			{
				return (SummaryUIType)this.GetValue( SummaryUITypeProperty );
			}
			set
			{
				this.SetValue( SummaryUITypeProperty, value );
			}
		}

				#endregion // SummaryUIType

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region Width

		/// <summary>
		/// Identifies the <see cref="Width"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width",
			typeof(FieldLength?), typeof(FieldSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a FieldLength instance that represents the default width of the field.
		/// </summary>
		/// <seealso cref="WidthProperty"/>
		/// <seealso cref="Height"/>
		//[Description("Returns or sets a FieldLength instance that represents the default width of the field.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public FieldLength? Width
		{
			get
			{
				return (FieldLength?)this.GetValue(Field.WidthProperty);
			}
			set
			{
				this.SetValue(Field.WidthProperty, value);
			}
		}

				#endregion //Width

            #endregion // Public Properties

        #endregion //Properties

        #region Methods

            #region Private Methods

                #region ShouldSerialize

		/// <summary>
		/// Determines if any property value is set to a non-default value.
		/// </summary>
		/// <returns>Returns true if any property value is set to a non-default value.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
        {
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			return GridUtilities.ShouldSerialize(this);
            
            
#region Infragistics Source Cleanup (Region)

























































#endregion // Infragistics Source Cleanup (Region)

        }

                #endregion //ShouldSerialize	

                #region ValidatePixelValue

        private static bool ValidatePixelValue(object value)
        {
            if ( !(value is double) )
                return false;

            if (double.IsNaN((double)value))
                return true;

            return !((double)value < 0);
        }

                #endregion //ValidatePixelValue	
    
            #endregion //Private Methods

            #region Internal Methods

                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                // Created a helper method to remove duplicate code in the DataPresenterReportControl.
                //
                #region ResetExcludedSettings

        // MBS 7/28/09 - NA9.2 Excel Exporting
        //internal void ResetExcludedSettings(ReportViewBase view)
        internal void ResetExcludedSettings(IExportOptions options)
        {
            // MBS 7/28/09 - NA9.2 Excel Exporting
            // We need to be able to reset the properties that are common
            // to the IExportOptions interface.  Also added null checks
            // in the below 'if' statements
            ReportViewBase view = options as ReportViewBase;

            if (view != null && view.ExcludeEditorSettings)
            {
                this.EditorType = typeof(XamTextEditor);
                this.EditorStyle = null;
                this.EditorStyleSelector = null;
            }
            if (view != null && view.ExcludeCellValuePresenterStyles)
            {
                this.CellValuePresenterStyle = null;
                this.CellValuePresenterStyleSelector = null;
            }
            if (view != null && view.ExcludeLabelPresenterStyles)
            {
                this.LabelPresenterStyle = null;
                this.LabelPresenterStyleSelector = null;
            }

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (view.ExcludeSummaries)
            if(options.ExcludeSummaries)
            {
                this.AllowSummaries = false;
                this.SummaryUIType = SummaryUIType.Default;
                this.SummaryDisplayArea = SummaryDisplayAreas.None;
            }
        } 

                #endregion //ResetExcludedSettings
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