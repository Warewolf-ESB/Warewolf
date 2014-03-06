using System;
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
using System.Data;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Text;
//using Infragistics.Windows.Input;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
	/// Used in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> derived control to specify settings to apply to one or more <see cref="FieldLayout"/>s. 
	/// </summary>
	/// <remarks>
	/// <para class="body">This settings object is exposed via the following 2 properties:
	/// <ul>
	/// <li><see cref="DataPresenterBase"/>'s <see cref="DataPresenterBase.FieldLayoutSettings"/> - settings specified here become the default for all <see cref="FieldLayout"/>s in the <see cref="DataPresenterBase.FieldLayouts"/> collection.</li>
	/// <li><see cref="FieldLayout"/>'s <see cref="FieldLayout.Settings"/> - settings specified here apply to only this one specific <see cref="FieldLayout"/>.</li>
	/// </ul>
	/// </para>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Layout.html">Field Layout</a> topic in the Developer's Guide for an explanation of the FieldLayout object.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
    /// <seealso cref="DataPresenterBase.FieldLayoutSettings"/>
    /// <seealso cref="DataPresenterBase.DefaultFieldLayout"/>
    /// <seealso cref="FieldLayout"/>
    /// <seealso cref="FieldLayout.Settings"/>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[StyleTypedProperty(Property = "DataRecordCellAreaStyle", StyleTargetType = typeof(DataRecordCellArea))]	// AS 5/3/07
	[StyleTypedProperty(Property = "DataRecordPresenterStyle", StyleTargetType = typeof(DataRecordPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "HeaderPresenterStyle", StyleTargetType = typeof(HeaderPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "HeaderLabelAreaStyle", StyleTargetType = typeof(HeaderLabelArea))]	// AS 5/3/07
	[StyleTypedProperty(Property = "HeaderPrefixAreaStyle", StyleTargetType = typeof(HeaderPrefixArea))]	// AS 5/3/07
	[StyleTypedProperty(Property = "RecordListControlStyle", StyleTargetType = typeof(RecordListControl))]	// AS 5/3/07
	[StyleTypedProperty(Property = "RecordSelectorStyle", StyleTargetType = typeof(RecordSelector))]	// AS 5/3/07
    // JJD 2/11/09 - TFS10860/TFS13609
    [CloneBehavior(CloneBehavior.CloneObject)]
    public class FieldLayoutSettings : DependencyObjectNotifier
	{
		#region Private Members

		private Grid					_dataRecordCellAreaTemplateGrid;

        #endregion //Private Members

		#region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="FieldLayoutSettings"/> class
        /// </summary>
		public FieldLayoutSettings()
		{
		}

		#endregion //Constructors

        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			if (e.Property == DataRecordCellAreaGridTemplateProperty)
			{
				this._dataRecordCellAreaTemplateGrid = null;

				ItemsPanelTemplate newTemplate = e.NewValue as ItemsPanelTemplate;

				Debug.Assert(e.NewValue == null || newTemplate != null);

				if (newTemplate != null)
				{
					if (!newTemplate.IsSealed)
						newTemplate.Seal();

					this._dataRecordCellAreaTemplateGrid = newTemplate.LoadContent() as Grid;

					if ( this._dataRecordCellAreaTemplateGrid == null )
						throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_9" ) );

					// turn off te IsItemsHost property to prevent an exception in case it's set to true
					this._dataRecordCellAreaTemplateGrid.IsItemsHost = false;
				}
			}
            this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrids	

		#region Properties

			#region Public Properties
		
				#region AddNewRecordLocation

		/// <summary>
		/// Identifies the <see cref="AddNewRecordLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AddNewRecordLocationProperty = DependencyProperty.Register("AddNewRecordLocation",
            typeof(AddNewRecordLocation), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(AddNewRecordLocation.Default), new ValidateValueCallback(IsAddNewRecordLocationValid));

        private static bool IsAddNewRecordLocationValid(object value)
        {
            return Enum.IsDefined(typeof(AddNewRecordLocation), value);
        }

        /// <summary>
        /// Determines how the add record UI is presented to the user.
        /// </summary>
		/// <remarks>The ultimate default value used is 'OnTopFixed'. However, this property is ignored if the <see cref="DataPresenterBase.DataSource"/> does not support adding of records (i.e. does not implement the <see cref="System.ComponentModel.IBindingList"/> interface or that interface's <see cref="System.ComponentModel.IBindingList.AllowNew"/> property returns false).</remarks>
		/// <seealso cref="AllowAddNew"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="AddNewRecordLocationProperty"/>
		/// <seealso cref="FieldLayout.AddNewRecordLocationResolved"/>
        /// <seealso cref="FieldLayout"/>
		//[Description("Determines how the add record UI is presented to the user.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public AddNewRecordLocation AddNewRecordLocation
		{
            get
            {
                return (AddNewRecordLocation)this.GetValue(FieldLayoutSettings.AddNewRecordLocationProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AddNewRecordLocationProperty, value);
            }
        }

				#endregion //AddNewRecordLocation

                #region AllowAddNew

        /// <summary>
        /// Identifies the <see cref="AllowAddNew"/> property
        /// </summary>
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public static readonly DependencyProperty AllowAddNewProperty = DependencyProperty.Register("AllowAddNew",
            typeof(Nullable<bool>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Gets/sets whether the user can add records.
        /// </summary>
		/// <remarks>The ultimate default value used is false. However, this property is ignored if the <see cref="DataPresenterBase.DataSource"/> does not support adding of records (i.e. does not implement the <see cref="System.ComponentModel.IBindingList"/> interface or that interface's <see cref="System.ComponentModel.IBindingList.AllowNew"/> property returns false).</remarks>
		/// <seealso cref="AddNewRecordLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="AllowDelete"/>
		/// <seealso cref="FieldSettings.AllowEdit"/>
		/// <seealso cref="AllowAddNewProperty"/>
        /// <seealso cref="FieldLayout.AllowAddNewResolved"/>
        //[Description("Gets/sets whether the user can add records.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public Nullable<bool> AllowAddNew
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldLayoutSettings.AllowAddNewProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AllowAddNewProperty, value);
            }
        }

                #endregion //AllowAddNew

                #region AllowDelete

        /// <summary>
        /// Identifies the <see cref="AllowDelete"/> property
        /// </summary>
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register("AllowDelete",
            typeof(Nullable<bool>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Gets/sets whether the user can delete records.
        /// </summary>
		/// <remarks>The ultimate default value used is true. However, even if this property is resolved to true it will be ignored if the <see cref="DataPresenterBase.DataSource"/> does not support deletions.</remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="AllowAddNew"/>
        /// <seealso cref="FieldSettings.AllowEdit"/>
        /// <seealso cref="AllowDeleteProperty"/>
        /// <seealso cref="FieldLayout.AllowDeleteResolved"/>
        //[Description("Gets/sets whether the user can delete records")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public Nullable<bool> AllowDelete
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldLayoutSettings.AllowDeleteProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AllowDeleteProperty, value);
            }
        }

                #endregion //AllowDelete

				#region AllowFieldMoving



		
		
		/// <summary>
		/// Identifies the <see cref="AllowFieldMoving"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowFieldMovingProperty = DependencyProperty.Register(
				"AllowFieldMoving",
				typeof( AllowFieldMoving ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( AllowFieldMoving.Default )
			);

		/// <summary>
		/// Specifies whether the user is allowed to re-arrange fields by dragging and dropping fields. Default is resolved to <b>Yes</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default the user is allowed to re-arrange fields. You can use <b>AllowFieldMoving</b> property to prevent
		/// the user from moving fields or enforce restrictions by setting the property to appropriate value. You can
		/// also use <see cref="FieldMovingMaxRows"/> and <see cref="FieldMovingMaxColumns"/> properties to restrict
		/// field arrangement layouts the user can create.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AllowFieldMoving"/>
		/// <seealso cref="FieldMovingMaxRows"/>
		/// <seealso cref="FieldMovingMaxColumns"/>
		//[Description( "Specifies whether the user is allowed to re-arrange fields." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public AllowFieldMoving AllowFieldMoving
		{
			get
			{
				return (AllowFieldMoving)this.GetValue( AllowFieldMovingProperty );
			}
			set
			{
				this.SetValue( AllowFieldMovingProperty, value );
			}
		}



				#endregion // AllowFieldMoving

                // JJD 6/8/09 - NA 2009 vol2 - RecordFixing
				#region AllowRecordFixing

		/// <summary>
		/// Identifies the <see cref="AllowRecordFixing"/> dependency property
		/// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty AllowRecordFixingProperty = DependencyProperty.Register(
				"AllowRecordFixing",
				typeof( AllowRecordFixing ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( AllowRecordFixing.Default )
			);

		/// <summary>
		/// Specifies whether the user is allowed to fix records. Default is resolved to <b>No</b>.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AllowRecordFixing"/>
		/// <seealso cref="FieldLayout.AllowRecordFixingResolved"/>
		/// <seealso cref="Record.FixedLocation"/>
        //[Description("Specifies whether the user is allowed to fix records.")]
		//[Category( "Behavior" )]
		[Bindable( true )]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public AllowRecordFixing AllowRecordFixing
		{
			get
			{
				return (AllowRecordFixing)this.GetValue( AllowRecordFixingProperty );
			}
			set
			{
				this.SetValue( AllowRecordFixingProperty, value );
			}
		}

				#endregion // AllowRecordFixing

                // AS 4/8/09 NA 2009.2 ClipboardSupport
                #region AllowClipboardOperations

        /// <summary>
        /// Identifies the <see cref="AllowClipboardOperations"/> dependency property
        /// </summary>
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
        public static readonly DependencyProperty AllowClipboardOperationsProperty = DependencyProperty.Register("AllowClipboardOperations",
            typeof(AllowClipboardOperations?), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns or sets which clipboard operations can be performed.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property determines which clipboard actions can be performed by the end user on the 
        /// records and/or cells that are selected within the DataPresenter.</p>
        /// <p class="note"><b>Note:</b> These operations have no relation to any clipboard operations that may be 
        /// performed/allowed by an editor that is in edit mode. For example, the value of this property has no bearing 
        /// on whether you can copy values from a XamTextEditor to the clipboard while in edit mode. Also, the clipboard 
        /// operations of the DataPresenter will not be performed while a cell is in edit mode.</p>
        /// </remarks>
        /// <seealso cref="AllowClipboardOperationsProperty"/>
        /// <seealso cref="CopyFieldLabelsToClipboard"/>
		/// <seealso cref="Field.DisallowModificationViaClipboard"/>
        //[Description("Returns or sets which clipboard operations can be performed.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
		[TypeConverter(typeof(NullableConverter<AllowClipboardOperations>))]
        public AllowClipboardOperations? AllowClipboardOperations
        {
            get
            {
                return (AllowClipboardOperations?)this.GetValue(FieldLayoutSettings.AllowClipboardOperationsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AllowClipboardOperationsProperty, value);
            }
        }

                #endregion //AllowClipboardOperations

				#region AllowHidingViaFieldChooser

		
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

				#endregion // AllowHidingViaFieldChooser

				// AS 6/9/09 NA 2009.2 Field Sizing
				#region AutoFitMode

		/// <summary>
		/// Identifies the <see cref="AutoFitMode"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public static readonly DependencyProperty AutoFitModeProperty = DependencyProperty.Register("AutoFitMode",
			typeof(AutoFitMode), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(AutoFitMode.Default));

		/// <summary>
		/// Returns or sets a value indicating which fields should be resized when AutoFit is enabled.
		/// </summary>
		/// <seealso cref="AutoFitModeProperty"/>
		/// <seealso cref="DataPresenterBase.AutoFit"/>
		//[Description("Returns or sets a value indicating which fields should be resized when AutoFit is enabled.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
		public AutoFitMode AutoFitMode
		{
			get
			{
				return (AutoFitMode)this.GetValue(FieldLayoutSettings.AutoFitModeProperty);
			}
			set
			{
				this.SetValue(FieldLayoutSettings.AutoFitModeProperty, value);
			}
		}

				#endregion //AutoFitMode

                #region AutoGenerateFields

        /// <summary>
        /// Identifies the <see cref="AutoGenerateFields"/> property
        /// </summary>
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public static readonly DependencyProperty AutoGenerateFieldsProperty = DependencyProperty.Register("AutoGenerateFields",
            typeof(Nullable<bool>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Gets/sets whether the <see cref="FieldLayout.Fields"/> collection will be automatically populated with <see cref="Field"/>s for every property in the underlying data.
		/// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Assigning_a_FieldLayout.html">Assigning a FieldLayout</a> topic in the Developer's Guide for an explanantion of the auot generation process.</p>
		/// <para class="note"><b>Note: </b>Setting this property to false is ignored if <see cref="DataPresenterBase.BindToSampleData"/> is set to true.</para>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.BindToSampleData"/>
        /// <seealso cref="AutoGenerateFieldsProperty"/>
        /// <seealso cref="FieldLayout.AutoGenerateFieldsResolved"/>
        /// <seealso cref="FieldLayout.Fields"/>
        //[Description("Gets/sets whether fields will be auotmatically generated")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public Nullable<bool> AutoGenerateFields
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldLayoutSettings.AutoGenerateFieldsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AutoGenerateFieldsProperty, value);
            }
        }

                #endregion //AutoGenerateFields
	
				#region AutoArrangeCells

		/// <summary>
		/// Identifies the <see cref="AutoArrangeCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoArrangeCellsProperty = DependencyProperty.Register("AutoArrangeCells",
            typeof(AutoArrangeCells), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(AutoArrangeCells.Default), new ValidateValueCallback(IsAutoArrangeCellsValid));

        private static bool IsAutoArrangeCellsValid(object value)
        {
            return Enum.IsDefined(typeof(AutoArrangeCells), value);
        }

        /// <summary>
        /// Determines hows <see cref="Field"/>s are laid out by default
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AutoArrangeCells"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        /// <seealso cref="FieldLayout.Fields"/>
        //[Description("Determines hows fields are laid out by default")]
		//[Category("Appearance")]
		[Bindable(true)]
		public AutoArrangeCells AutoArrangeCells
        {
            get
            {
                return (AutoArrangeCells)this.GetValue(FieldLayoutSettings.AutoArrangeCellsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AutoArrangeCellsProperty, value);
            }
        }

				#endregion //AutoArrangeCells

				#region AutoArrangeMaxColumns

		/// <summary>
		/// Identifies the <see cref="AutoArrangeMaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoArrangeMaxColumnsProperty = DependencyProperty.Register("AutoArrangeMaxColumns",
				  typeof(int), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(-1));

        /// <summary>
        /// Gets/sets the maximum number of logical columns in the generated templates.
        /// </summary>
		/// <remarks>The property is ignored if <see cref="AutoArrangeCells"/> resolves to 'Never'.</remarks>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="AutoArrangeMaxRows"/>
        /// <seealso cref="AutoArrangeCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AutoArrangeCells"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        /// <seealso cref="AutoArrangeMaxColumnsProperty"/>
        //[Description("Gets/sets the maximum number of logical columns in the generated templates.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public int AutoArrangeMaxColumns
		{
            get
            {
                return (int)this.GetValue(FieldLayoutSettings.AutoArrangeMaxColumnsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AutoArrangeMaxColumnsProperty, value);
            }
        }

				#endregion //AutoArrangeMaxColumns

				#region AutoArrangeMaxRows

		/// <summary>
		/// Identifies the <see cref="AutoArrangeMaxRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoArrangeMaxRowsProperty = DependencyProperty.Register("AutoArrangeMaxRows",
				  typeof(int), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(-1));

        /// <summary>
        /// Gets/sets the maximum number of logical rows in the generated templates.
        /// </summary>
		/// <remarks>The property is ignored if <see cref="AutoArrangeCells"/> resolves to 'Never'.</remarks>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="AutoArrangeMaxColumns"/>
        /// <seealso cref="AutoArrangeCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AutoArrangeCells"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        /// <seealso cref="AutoArrangeMaxRowsProperty"/>
        //[Description("Gets/sets the maximum number of logical rows in the generated templates.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public int AutoArrangeMaxRows
		{
            get
            {
                return (int)this.GetValue(FieldLayoutSettings.AutoArrangeMaxRowsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AutoArrangeMaxRowsProperty, value);
            }
        }

				#endregion //AutoArrangeMaxRows
		
				#region AutoArrangePrimaryFieldReservation

		/// <summary>
		/// Identifies the <see cref="AutoArrangePrimaryFieldReservation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoArrangePrimaryFieldReservationProperty = DependencyProperty.Register("AutoArrangePrimaryFieldReservation",
            typeof(AutoArrangePrimaryFieldReservation), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(AutoArrangePrimaryFieldReservation.Default), new ValidateValueCallback(IsAutoArrangePrimaryFieldReservationValid));

        private static bool IsAutoArrangePrimaryFieldReservationValid(object value)
        {
            return Enum.IsDefined(typeof(AutoArrangePrimaryFieldReservation), value);
        }

        /// <summary>
        /// Determines if the first row, column or cell is reserved for the primary field in the generated templates.
        /// </summary>
		/// <remarks>The property is ignored if <see cref="AutoArrangeCells"/> resolves to 'Never'.</remarks>
        /// <seealso cref="FieldLayout.PrimaryField"/>
        /// <seealso cref="Field.IsPrimary"/>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        /// <seealso cref="HighlightPrimaryField"/>
        /// <seealso cref="FieldLayout.HighlightPrimaryFieldResolved"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AutoArrangePrimaryFieldReservation"/>
        //[Description("Determines if the first row, column or cell is reserved for the primary field in the generated templates.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public AutoArrangePrimaryFieldReservation AutoArrangePrimaryFieldReservation
		{
            get
            {
                return (AutoArrangePrimaryFieldReservation)this.GetValue(FieldLayoutSettings.AutoArrangePrimaryFieldReservationProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.AutoArrangePrimaryFieldReservationProperty, value);
            }
        }

				#endregion //AutoArrangePrimaryFieldReservation

				#region CalculationScope

		
		
		/// <summary>
		/// Identifies the <see cref="CalculationScope"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalculationScopeProperty = DependencyProperty.Register(
			"CalculationScope",
			typeof( CalculationScope ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( CalculationScope.Default )
			);

		/// <summary>
		/// Controls which records and in which order they are traversed when calculating summaries. 
		/// More importantly controls whether visible records are used for calculating summaries or all 
		/// records.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>CalculationScope</b> controls whether to use data from all records or just the visible 
		/// records when calculating summaries. This is particularly impactful when using record 
		/// filtering functionality is enabled. If this property is set to <b>FilteredSortedList</b> then
		/// as records are filtered, summaries will be recalculated based on data from the filtered records 
		/// (visible records). 
		/// </para>
		/// <para class="body">
		/// This property also controls the order in which data is aggregated into a summary calculation.
		/// Note that for all the built-in summaries the order does not result in different calculation results 
		/// since the underlying calculations do not rely upon the order of aggregated values.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryDefinition"/>
		//[Description( "Specifies which records to use for calculation purposes." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public CalculationScope CalculationScope
		{
			get
			{
				return (CalculationScope)this.GetValue( CalculationScopeProperty );
			}
			set
			{
				this.SetValue( CalculationScopeProperty, value );
			}
		}

				#endregion // CalculationScope

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				#region ChildRecordsDisplayOrder

		/// <summary>
		/// Identifies the <see cref="FieldLayoutSettings.ChildRecordsDisplayOrder"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ChildRecordsDisplayOrderProperty = DependencyProperty.Register(
			"ChildRecordsDisplayOrder",
			typeof(ChildRecordsDisplayOrder),
			typeof(FieldLayoutSettings),
			new FrameworkPropertyMetadata(ChildRecordsDisplayOrder.Default)
			);

		/// <summary>
		/// Returns or sets a value that indicates how child records will be displayed relative to their parent record when it is expanded.
		/// </summary>
		[Bindable(true)]
		public ChildRecordsDisplayOrder ChildRecordsDisplayOrder
		{
			get { return (ChildRecordsDisplayOrder)this.GetValue(ChildRecordsDisplayOrderProperty); }
			set { this.SetValue(ChildRecordsDisplayOrderProperty, value); }
		} 

				#endregion // ChildRecordsDisplayOrder

                // AS 4/15/09 NA 2009.2 ClipboardSupport
                #region CopyFieldLabelsToClipboard

        /// <summary>
        /// Identifies the <see cref="CopyFieldLabelsToClipboard"/> dependency property
        /// </summary>
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
        public static readonly DependencyProperty CopyFieldLabelsToClipboardProperty = DependencyProperty.Register("CopyFieldLabelsToClipboard",
            typeof(bool?), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns or sets a value that indicates whether the field labels associated with the cells being copied should be included in the output placed on the clipboard.
        /// </summary>
        /// <seealso cref="CopyFieldLabelsToClipboardProperty"/>
        /// <seealso cref="AllowClipboardOperations"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardCopyingEventArgs.GetLabelValueHolder(Field)"/>
		//[Description("Returns or sets a value that indicates whether the field labels associated with the cells being copied should be included in the output placed on the clipboard.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
        public bool? CopyFieldLabelsToClipboard
        {
            get
            {
                return (bool?)this.GetValue(FieldLayoutSettings.CopyFieldLabelsToClipboardProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.CopyFieldLabelsToClipboardProperty, value);
            }
        }

                #endregion //CopyFieldLabelsToClipboard

				#region DataRecordCellAreaGridTemplate

		/// <summary>
		/// Identifies the <see cref="DataRecordCellAreaGridTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordCellAreaGridTemplateProperty = DependencyProperty.Register("DataRecordCellAreaGridTemplate",
			typeof(ItemsPanelTemplate), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

		/// <summary>
		/// Gets/sets the panel template used to generate the cell area
		/// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Arranging_Cells_within_the_Record.html">Arranging Cells within the Record</a> topic in the Developer's Guide.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="DataRecordCellAreaGridTemplateProperty"/>
		//[Description("Gets/sets the panel template used to generate the cell area")]
		//[Category("Appearance")]
		[Bindable(false)]
		public ItemsPanelTemplate DataRecordCellAreaGridTemplate
		{
			get
			{
				return (ItemsPanelTemplate)this.GetValue(FieldLayoutSettings.DataRecordCellAreaGridTemplateProperty);
			}
			set
			{
				this.SetValue(FieldLayoutSettings.DataRecordCellAreaGridTemplateProperty, value);
			}
		}

				#endregion //DataRecordCellAreaGridTemplate

				#region DataRecordCellAreaStyle

		/// <summary>
		/// Identifies the <see cref="DataRecordCellAreaStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordCellAreaStyleProperty = DependencyProperty.Register("DataRecordCellAreaStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style for a <see cref="Record"/>'s cell area 
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="DataRecordCellAreaStyleSelector"/>
        /// <seealso cref="DataRecordCellAreaStyleProperty"/>
        //[Description("Gets/sets the style for a record's cell area")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style DataRecordCellAreaStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.DataRecordCellAreaStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DataRecordCellAreaStyleProperty, value);
            }
        }

				#endregion //DataRecordCellAreaStyle

				#region DataRecordCellAreaStyleSelector

		/// <summary>
		/// Identifies the <see cref="DataRecordCellAreaStyleSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordCellAreaStyleSelectorProperty = DependencyProperty.Register("DataRecordCellAreaStyleSelector",
            typeof(StyleSelector), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// A callback used for supplying styles for a <see cref="Record"/>'s cell area
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="DataRecordCellAreaStyle"/>
        /// <seealso cref="DataRecordCellAreaStyleSelectorProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		//[Description("A callback used for supplying styles for a record's cell area")]
        //[Category("Appearance")]
		public StyleSelector DataRecordCellAreaStyleSelector
		{
            get
            {
                return (StyleSelector)this.GetValue(FieldLayoutSettings.DataRecordCellAreaStyleSelectorProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DataRecordCellAreaStyleSelectorProperty, value);
            }
        }

				#endregion //DataRecordCellAreaStyleSelector

				#region DataRecordPresenterStyle

		/// <summary>
		/// Identifies the <see cref="DataRecordPresenterStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordPresenterStyleProperty = DependencyProperty.Register("DataRecordPresenterStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style for <see cref="RecordPresenter"/>s
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="DataRecordPresenterStyleSelector"/>
        /// <seealso cref="DataRecordPresenterStyleProperty"/>
        //[Description("Gets/sets the style of the presenter for the record")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style DataRecordPresenterStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.DataRecordPresenterStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DataRecordPresenterStyleProperty, value);
            }
        }

				#endregion //DataRecordPresenterStyle

				#region DataRecordPresenterStyleSelector

		/// <summary>
		/// Identifies the <see cref="DataRecordPresenterStyleSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordPresenterStyleSelectorProperty = DependencyProperty.Register("DataRecordPresenterStyleSelector",
            typeof(StyleSelector), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// A callback used for supplying styles for <see cref="RecordPresenter"/>s
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="DataRecordPresenterStyle"/>
        /// <seealso cref="DataRecordPresenterStyleSelectorProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        //[Description("A callback used for supplying styles for record presenters")]
        //[Category("Appearance")]
		public StyleSelector DataRecordPresenterStyleSelector
		{
            get
            {
                return (StyleSelector)this.GetValue(FieldLayoutSettings.DataRecordPresenterStyleSelectorProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DataRecordPresenterStyleSelectorProperty, value);
            }
        }

				#endregion //DataRecordPresenterStyleSelector

				#region DataRecordSizingMode

		/// <summary>
		/// Identifies the <see cref="DataRecordSizingMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordSizingModeProperty = DependencyProperty.Register("DataRecordSizingMode",
			typeof(DataRecordSizingMode), typeof(FieldLayoutSettings), 
			new FrameworkPropertyMetadata(DataRecordSizingMode.Default,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnDataRecordSizingModeChanged )
			), 
			new ValidateValueCallback(IsDataRecordSizingModeValid)
		);

		private static bool IsDataRecordSizingModeValid(object value)
		{
			return Enum.IsDefined(typeof(DataRecordSizingMode), value);
		}

		// JJD 4/26/07
		// Optimization - cache the property locally
		private DataRecordSizingMode _cachedDataRecordSizingMode = DataRecordSizingMode.Default;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnDataRecordSizingModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings fieldLayoutSettings = (FieldLayoutSettings)dependencyObject;
			DataRecordSizingMode newVal = (DataRecordSizingMode)e.NewValue;

			fieldLayoutSettings._cachedDataRecordSizingMode = newVal;
		}

		/// <summary>
		/// Determines how <see cref="DataRecord"/>s are sized and if they can resized by the user.  The default is SizedToContentAndFixed.
		/// </summary>
		/// <remarks>
		/// <b>NOTE:</b> When using the <b>SizedToContentAndFixed</b> or <b>SizedToContentAndIndividuallySizable</b> values for this property 
		/// there is a negative impact to performance when rendering the grid because the control must calculate the size of each individual cell.
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="DataRecordSizingModeProperty"/>
		/// <seealso cref="DataRecordPresenterStyle"/>
		/// <seealso cref="DataRecordPresenterStyleSelector"/>
		/// <seealso cref="FieldLayout.DataRecordSizingModeResolved"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecordSizingMode"/>
		//[Description("Determines how DataRecords are sized and if they can resized by the user.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DataRecordSizingMode DataRecordSizingMode
		{
			get
			{
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (DataRecordSizingMode)this.GetValue(FieldLayoutSettings.DataRecordSizingModeProperty);
				return this._cachedDataRecordSizingMode;
			}
			set
			{
				this.SetValue(FieldLayoutSettings.DataRecordSizingModeProperty, value);
			}
		}


				#endregion //DataRecordSizingMode

                #region DefaultColumnDefinition

        /// <summary>
        /// Identifies the <see cref="DefaultColumnDefinition"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DefaultColumnDefinitionProperty = DependencyProperty.Register("DefaultColumnDefinition",
            typeof(ColumnDefinition), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
		/// Gets/sets the default column definition used by the generated grid in the cell area
        /// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Arranging_Cells_within_the_Record.html">Arranging Cells within the Record</a> topic in the Developer's Guide.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
		//[Description("Gets/sets the default column definition used by the generated grid in the cell area")]
        //[Category("Appearance")]
		[Bindable(false)]
		public ColumnDefinition DefaultColumnDefinition
        {
            get
            {
                return (ColumnDefinition)this.GetValue(FieldLayoutSettings.DefaultColumnDefinitionProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DefaultColumnDefinitionProperty, value);
            }
        }

                #endregion //DefaultColumnDefinition

                #region DefaultRowDefinition

        /// <summary>
        /// Identifies the <see cref="DefaultRowDefinition"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DefaultRowDefinitionProperty = DependencyProperty.Register("DefaultRowDefinition",
            typeof(RowDefinition), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
		/// Gets/sets the default row definition used by the generated grid in the cell area
        /// </summary>
        /// <seealso cref="FieldLayout"/>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Arranging_Cells_within_the_Record.html">Arranging Cells within the Record</a> topic in the Developer's Guide.</p>
		/// </remarks>
		//[Description("Gets/sets the default row definition used by the generated grid in the cell area")]
        //[Category("Appearance")]
		[Bindable(false)]
		public RowDefinition DefaultRowDefinition
        {
            get
            {
                return (RowDefinition)this.GetValue(FieldLayoutSettings.DefaultRowDefinitionProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.DefaultRowDefinitionProperty, value);
            }
        }

                #endregion //DefaultRowDefinition

				#region DataErrorDisplayMode

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the <see cref="DataErrorDisplayMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty DataErrorDisplayModeProperty = DependencyProperty.Register(
			"DataErrorDisplayMode",
			typeof( DataErrorDisplayMode ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( DataErrorDisplayMode.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies if and how to display cell and record data error information.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default cell and record data errors are not displayed. You can enable this functionality
		/// by setting the <see cref="SupportDataErrorInfo"/> property. When the functionality is enabled,
		/// DataErrorDisplayMode property controls how to display the data error information. By default,
		/// a data error icon is displayed in the cell or the record selector to display cell data error
		/// and record data error respectively. You can use this property to control how the data error
		/// is displayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="SupportDataErrorInfo"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataErrorDisplayMode"/>
		//[Description( "Specifies if and how to display cell and record data error information." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public DataErrorDisplayMode DataErrorDisplayMode
		{
			get
			{
				return (DataErrorDisplayMode)this.GetValue( DataErrorDisplayModeProperty );
			}
			set
			{
				this.SetValue( DataErrorDisplayModeProperty, value );
			}
		}

				#endregion // DataErrorDisplayMode

                // JJD 4/28/08 - BR31406 and BR31707 - added
				#region ExpansionIndicatorDisplayMode

		/// <summary>
		/// Identifies the <see cref="ExpansionIndicatorDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorDisplayModeProperty = DependencyProperty.Register("ExpansionIndicatorDisplayMode",
			typeof(ExpansionIndicatorDisplayMode), typeof(FieldLayoutSettings), 
			new FrameworkPropertyMetadata(ExpansionIndicatorDisplayMode.Default,
				new PropertyChangedCallback( OnExpansionIndicatorDisplayModeChanged )
			), 
			new ValidateValueCallback(IsExpansionIndicatorDisplayModeValid)
		);

		private static bool IsExpansionIndicatorDisplayModeValid(object value)
		{
			return Enum.IsDefined(typeof(ExpansionIndicatorDisplayMode), value);
		}

		private ExpansionIndicatorDisplayMode _cachedExpansionIndicatorDisplayMode = ExpansionIndicatorDisplayMode.Default;

		private static void OnExpansionIndicatorDisplayModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings fieldLayoutSettings = (FieldLayoutSettings)dependencyObject;
			ExpansionIndicatorDisplayMode newVal = (ExpansionIndicatorDisplayMode)e.NewValue;

			fieldLayoutSettings._cachedExpansionIndicatorDisplayMode = newVal;
		}

		/// <summary>
		/// Determines whether an expansion indicator is displayed in each Record.
		/// </summary>
        /// <remarks>
        /// <para class="body">This setting is used in the <see cref="Record"/>'s <see cref="Record.ExpansionIndicatorVisibility"/> 
        /// get accessor to determine the resolved visiblity of the expansion indicator. However, explicitly setting the 
        /// <see cref="Record.ExpansionIndicatorVisibility"/> property on the <see cref="Record"/> will take precedence over this property.</para>
        /// <para class="note"><b>Note:</b> this property is ignored in certain views where expansion indicators are not supported. 
        /// It is also ignored if the <see cref="DataPresenterBase.IsNestedDataDisplayEnabled"/> property is set to false.</para>
        /// </remarks>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="ExpansionIndicatorDisplayModeProperty"/>
		/// <seealso cref="DataRecordPresenterStyle"/>
		/// <seealso cref="DataRecordPresenterStyleSelector"/>
		/// <seealso cref="Record.ExpansionIndicatorVisibility"/>
        /// <seealso cref="DataPresenterBase.IsNestedDataDisplayEnabled"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ExpansionIndicatorDisplayMode"/>
        //[Description("Determines whether an expansion indicator is displayed in each Record.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ExpansionIndicatorDisplayMode ExpansionIndicatorDisplayMode
		{
			get
			{
				return this._cachedExpansionIndicatorDisplayMode;
			}
			set
			{
				this.SetValue(FieldLayoutSettings.ExpansionIndicatorDisplayModeProperty, value);
			}
		}

				#endregion //ExpansionIndicatorDisplayMode

				#region FieldMovingMaxColumns

		
		
		/// <summary>
		/// Identifies the <see cref="FieldMovingMaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldMovingMaxColumnsProperty = DependencyProperty.Register(
				"FieldMovingMaxColumns",
				typeof( int? ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( null ),
				new ValidateValueCallback( ValidateFieldMovingMaxRowColumns )
			);

		
		
		/// <summary>
		/// Specifies the maximum number of logical columns of fields the user is allowed to create
		/// when re-arranging fields.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use <b>FieldMovingMaxColumns</b> and <see cref="FieldMovingMaxRows"/> to properties restrict 
		/// the user from moving fields in such a way that the resultant field layout has more than specified
		/// number of logical columns and rows. Also note that the <see cref="AllowFieldMoving"/> property
		/// can be used to set <i>WithinLogicalRow</i> and <i>WithinLogicalColumn</i> restrictions (see
		/// <see cref="Infragistics.Windows.DataPresenter.AllowFieldMoving"/> enum).
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxRows"/>
		//[Description( "Specifies maximum number of logical columns the user can create." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public int? FieldMovingMaxColumns
		{
			get
			{
				return (int?)this.GetValue( FieldMovingMaxColumnsProperty );
			}
			set
			{
				this.SetValue( FieldMovingMaxColumnsProperty, value );
			}
		}

		private static bool ValidateFieldMovingMaxRowColumns( object objVal )
		{
			int? val = (int?)objVal;
			if ( val.HasValue && val.Value < 0 )
				return false;

			return true;
		}

				#endregion // FieldMovingMaxColumns

				#region FieldMovingMaxRows

		
		
		/// <summary>
		/// Identifies the <see cref="FieldMovingMaxRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldMovingMaxRowsProperty = DependencyProperty.Register(
				"FieldMovingMaxRows",
				typeof( int? ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( null ),
				new ValidateValueCallback( ValidateFieldMovingMaxRowColumns )
			);

		/// <summary>
		/// Specifies the maximum number of logical rows of fields the user is allowed to create
		/// when re-arranging fields.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use <see cref="FieldMovingMaxColumns"/> and <b>FieldMovingMaxRows</b> properties to restrict 
		/// the user from moving fields in such a way that the resultant field layout has more than specified
		/// number of logical columns and rows. Also note that the <see cref="AllowFieldMoving"/> property
		/// can be used to set <i>WithinLogicalRow</i> and <i>WithinLogicalColumn</i> restrictions (see
		/// <see cref="Infragistics.Windows.DataPresenter.AllowFieldMoving"/> enum).
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxColumns"/>
		//[Description( "Specifies maximum number of logical columns the user can create." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public int? FieldMovingMaxRows
		{
			get
			{
				return (int?)this.GetValue( FieldMovingMaxRowsProperty );
			}
			set
			{
				this.SetValue( FieldMovingMaxRowsProperty, value );
			}
		}

				#endregion // FieldMovingMaxRows

				#region FilterAction

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterAction"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterActionProperty = DependencyProperty.Register(
			"FilterAction",
			typeof( RecordFilterAction ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( RecordFilterAction.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies what action to take on records that are filtered out. Default is resolved to <b>Hide</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterAction</b> property specifies what action to take on records that are filtered out. 
		/// By default the records that do not match the filter criteria are hidden. You can set this 
		/// property to <b>Disable</b> to disable such filtered out records instead of hiding them. You can also 
		/// set this property to <b>None</b> in which case the data presenter will take no action on the filtered 
		/// out records. They will remain visible. Records' <see cref="DataRecord.IsFilteredOut"/> property will
		/// however be updated to reflect whether the record actually matches the filter criteria. This option is useful 
		/// if you want to apply some appearance to filtered-out or filtered-in records in the style or template of 
		/// record elements.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordFilterAction"/>
		/// <seealso cref="DataRecord.IsFilteredOut"/>
		/// <seealso cref="GroupByRecord.IsFilteredOut"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Specifies what action to take on records that are filtered out." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public RecordFilterAction FilterAction
		{
			get
			{
				return (RecordFilterAction)this.GetValue( FilterActionProperty );
			}
			set
			{
				this.SetValue( FilterActionProperty, value );
			}
		}

				#endregion // FilterAction

				#region FilterClearButtonLocation

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterClearButtonLocation"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterClearButtonLocationProperty = DependencyProperty.Register(
			"FilterClearButtonLocation",
			typeof( FilterClearButtonLocation ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( FilterClearButtonLocation.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies if and where to display the filter clear button. Default is resolved to <b>RecordSelectorAndFilterCell</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterClearButtonLocation</b> property specifies if and where to display the filter clear button. By default
		/// a filter clear button is displayed in the filter record's record selector and each filter cell. It lets the user
		/// clear filter criteria.
		/// </para>
		/// <para class="body">
		/// You can control the visibility of the filter clear button on a per field basis using the FieldSettings'
		/// <see cref="FieldSettings.FilterClearButtonVisibility"/> property.
		/// </para>
		/// <para class="body">
		/// To manipulate filters, including clear any filters, use the <see cref="RecordFilterCollection"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FilterClearButtonLocation"/>
		/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
		/// <seealso cref="RecordFilterCollection"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		//[Description( "Specifies if and where to display the filter clear button." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FilterClearButtonLocation FilterClearButtonLocation
		{
			get
			{
				return (FilterClearButtonLocation)this.GetValue( FilterClearButtonLocationProperty );
			}
			set
			{
				this.SetValue( FilterClearButtonLocationProperty, value );
			}
		}

				#endregion // FilterClearButtonLocation

				#region FilterEvaluationMode

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 

		/// <summary>
		/// Identifies the <see cref="FilterEvaluationMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public static readonly DependencyProperty FilterEvaluationModeProperty = DependencyPropertyUtilities.Register(
			"FilterEvaluationMode",
			typeof( FilterEvaluationMode ),
			typeof( FieldLayoutSettings ),
			DependencyPropertyUtilities.CreateMetadata( FilterEvaluationMode.Default )
		);

		/// <summary>
		/// Specifies how the data presenter will perform filtering operation.
		/// </summary>
		/// <seealso cref="FilterEvaluationModeProperty"/>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public FilterEvaluationMode FilterEvaluationMode
		{
			get
			{
				return (FilterEvaluationMode)this.GetValue( FilterEvaluationModeProperty );
			}
			set
			{
				this.SetValue( FilterEvaluationModeProperty, value );
			}
		}

				#endregion // FilterEvaluationMode

				#region FilterRecordLocation

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterRecordLocation"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterRecordLocationProperty = DependencyProperty.Register(
			"FilterRecordLocation",
			typeof( FilterRecordLocation ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( FilterRecordLocation.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies the location of the filter record. Default is resolved to <b>OnTopFixed</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterRecordLocation</b> property specifies the location of the filter record - whether
		/// it's at top or bottom of the record collection and also whether it's fixed or scrolling.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that you must set the <see cref="FieldSettings.AllowRecordFiltering"/> property to
		/// true and <see cref="FieldLayoutSettings.FilterUIType"/> property to <b>FilterRecord</b> in order
		/// to actually display the filter record.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FilterRecordLocation"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		//[Description( "Specifies the location of the filter record." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public FilterRecordLocation FilterRecordLocation
		{
			get
			{
				return (FilterRecordLocation)this.GetValue( FilterRecordLocationProperty );
			}
			set
			{
				this.SetValue( FilterRecordLocationProperty, value );
			}
		}

				#endregion // FilterRecordLocation

				#region FilterUIType

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="FilterUIType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FilterUITypeProperty = DependencyProperty.Register(
			"FilterUIType",
			typeof( FilterUIType ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( FilterUIType.Default, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnFilterUITypeChanged ) )
		);

		/// <summary>
		/// Specifies the user interface type for letting the user filter records. Default is resolved to <b>FilterRecord</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FilterUIType</b> property specifies the type of user interface that the data presenter will
		/// use to let the user filter records. See <seealso cref="Infragistics.Windows.DataPresenter.FilterUIType"/> enum
		/// for more information.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> To actually enable the filtering functionality, set the <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// property on the FieldSettings of the FieldLayout (<see cref="FieldLayout.FieldSettings"/>), or
		/// the DataPresenter (<see cref="DataPresenterBase.FieldSettings"/> or individual Field (<see cref="Field.Settings"/>).
		/// Setting <b>AllowRecordFiltering</b> is necessary for <b>FilterUIType</b> property to have any effect.
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
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.FilterRecordLocation"/>
		/// <seealso cref="FieldSettings.FilterOperandUIType"/>
		/// <seealso cref="FieldSettings.FilterOperatorDefaultValue"/>
		/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
		/// <seealso cref="FieldSettings.FilterEvaluationTrigger"/>
		/// <seealso cref="RecordFilterCollection"/>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="RecordManager.RecordFilters"/>
		/// <seealso cref="DataRecord.IsFilteredOut"/>
		/// <seealso cref="GroupByRecord.IsFilteredOut"/>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		/// <seealso cref="FieldSettings.FilterStringComparisonType"/>
		//[Description( "Specifies the user interface type for letting the user filter records." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public FilterUIType FilterUIType
		{
			get
			{
				return (FilterUIType)this.GetValue( FilterUITypeProperty );
			}
			set
			{
				this.SetValue( FilterUITypeProperty, value );
			}
		}

		private static void OnFilterUITypeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{



		}

				#endregion // FilterUIType

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region FixedFieldUIType

        /// <summary>
        /// Identifies the <see cref="FixedFieldUIType"/> property
        /// </summary>
        public static readonly DependencyProperty FixedFieldUITypeProperty = DependencyProperty.Register("FixedFieldUIType",
            typeof(FixedFieldUIType), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(FixedFieldUIType.Default, new PropertyChangedCallback(OnFixedFieldUITypeChanged)));

        private static void OnFixedFieldUITypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {



		}

        /// <summary>
        /// Determines the type of ui displayed to allow the end user to changed the <see cref="Field.FixedLocation"/> of the fields.
		/// </summary>
		/// <remarks>
        /// <p class="note"><b>Note:</b> Even if no ui is displayed, the end user may change the FixedLocation of a field by dragging a column 
        /// into or out of the fixed area assuming that <see cref="Field.AllowFixingResolved"/> allows the state to be changed and the field 
        /// allows moving.</p>
		/// </remarks>
        /// <seealso cref="Field.AllowFixingResolved"/>
        /// <seealso cref="Field"/>
        /// <seealso cref="FixedFieldSplitter"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        /// <seealso cref="Field.FixedLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FixedFieldUIType"/>
        /// <seealso cref="FieldLayout.FixedFieldUITypeResolved"/>
        //[Description("Determines the type of ui displayed to allow the end user to changed the fixed state of the fields.")]
        //[Category("Behavior")]
		[Bindable(true)]
        public FixedFieldUIType FixedFieldUIType
        {
            get
            {
                return (FixedFieldUIType)this.GetValue(FieldLayoutSettings.FixedFieldUITypeProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.FixedFieldUITypeProperty, value);
            }
        }

                #endregion //FixedFieldUIType

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordLimit

        /// <summary>
        /// Identifies the <see cref="FixedRecordLimit"/> property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty FixedRecordLimitProperty = DependencyProperty.Register("FixedRecordLimit",
            typeof(int?), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(null), new ValidateValueCallback(ValidateFixedRecordLimit));

        private static bool ValidateFixedRecordLimit(object value)
        {
            if ( value == null )
                return true;

            int intVal = (int)value;

            return intVal >= 0;
        }

        /// <summary>
        /// Determines how many sibling records can be fixed at a time.
		/// </summary>
        /// <remarks>
        /// <para class="body">It a record is fixed and the limit has been reached then the earliest record that had been fixed previously will be made scrollable.</para>
        /// </remarks>
        /// <seealso cref="Record"/>
        /// <seealso cref="AllowRecordFixing"/>
        /// <seealso cref="Record.FixedLocation"/>
        /// <seealso cref="FieldLayout.FixedRecordLimitResolved"/>
        //[Description("Determines how many sibling records can be fixed at a time.")]
        //[Category("Behavior")]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public int? FixedRecordLimit
        {
            get
            {
                return (int?)this.GetValue(FieldLayoutSettings.FixedRecordLimitProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.FixedRecordLimitProperty, value);
            }
        }

                #endregion //FixedRecordLimit

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordUIType

        /// <summary>
        /// Identifies the <see cref="FixedRecordUIType"/> property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty FixedRecordUITypeProperty = DependencyProperty.Register("FixedRecordUIType",
            typeof(FixedRecordUIType), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(FixedRecordUIType.Default));

        /// <summary>
        /// Determines the type of ui displayed to allow the end user to changed the <see cref="Record.FixedLocation"/> of a record.
		/// </summary>
        /// <seealso cref="AllowRecordFixing"/>
        /// <seealso cref="Record.FixedLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FixedRecordUIType"/>
        /// <seealso cref="FieldLayout.FixedRecordUITypeResolved"/>
        //[Description("Determines the type of ui displayed to allow the end user to changed the fixed state of a record.")]
        //[Category("Behavior")]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public FixedRecordUIType FixedRecordUIType
        {
            get
            {
                return (FixedRecordUIType)this.GetValue(FieldLayoutSettings.FixedRecordUITypeProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.FixedRecordUITypeProperty, value);
            }
        }

		/// <summary>
		/// Determines if the <see cref="FixedRecordUIType"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFixedRecordUIType()
        {
            return Utilities.ShouldSerialize(FixedRecordUITypeProperty, this);
        }

		/// <summary>
		/// Resets the <see cref="FixedRecordUIType"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFixedRecordUIType()
		{
			this.ClearValue(FieldLayoutSettings.FixedRecordUITypeProperty);
		}

                #endregion //FixedRecordUIType

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordSortOrder

        /// <summary>
        /// Identifies the <see cref="FixedRecordSortOrder"/> property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty FixedRecordSortOrderProperty = DependencyProperty.Register("FixedRecordSortOrder",
            typeof(FixedRecordSortOrder), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(FixedRecordSortOrder.Default));

        /// <summary>
        /// Determines the order of fixed records relative to each other.
		/// </summary>
        /// <seealso cref="AllowRecordFixing"/>
        /// <seealso cref="FixedRecordUIType"/>
        /// <seealso cref="Record.FixedLocation"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FixedRecordSortOrder"/>
        /// <seealso cref="FieldLayout.FixedRecordSortOrderResolved"/>
        //[Description("Determines the order of fixed records relative to each other.")]
        //[Category("Behavior")]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public FixedRecordSortOrder FixedRecordSortOrder
        {
            get
            {
                return (FixedRecordSortOrder)this.GetValue(FieldLayoutSettings.FixedRecordSortOrderProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.FixedRecordSortOrderProperty, value);
            }
        }

                #endregion //FixedRecordSortOrder

				#region GroupByEvaluationMode

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 

		/// <summary>
		/// Identifies the <see cref="GroupByEvaluationMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public static readonly DependencyProperty GroupByEvaluationModeProperty = DependencyPropertyUtilities.Register(
			"GroupByEvaluationMode",
			typeof( GroupByEvaluationMode ),
			typeof( FieldLayoutSettings ),
			DependencyPropertyUtilities.CreateMetadata( GroupByEvaluationMode.Default )
		);

		/// <summary>
		/// Specifies how the data presenter will perform grouping operation.
		/// </summary>
		/// <seealso cref="GroupByEvaluationModeProperty"/>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public GroupByEvaluationMode GroupByEvaluationMode
		{
			get
			{
				return (GroupByEvaluationMode)this.GetValue( GroupByEvaluationModeProperty );
			}
			set
			{
				this.SetValue( GroupByEvaluationModeProperty, value );
			}
		}

				#endregion // GroupByEvaluationMode

                // JJD 05/04/10 - TFS31349 - added
				#region GroupByExpansionIndicatorVisibility

		/// <summary>
		/// Identifies the <see cref="GroupByExpansionIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupByExpansionIndicatorVisibilityProperty = DependencyProperty.Register(
			"GroupByExpansionIndicatorVisibility",
			typeof( Visibility? ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata()
			);

		/// <summary>
		/// Determines if expansion indicators will be displayed in groupby records.
		/// </summary>
		/// <seealso cref="GroupByExpansionIndicatorVisibility"/>
        //[Description( "Determines if expansion indicators will be displayed in groupby records." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
        public Visibility? GroupByExpansionIndicatorVisibility
		{
			get
			{
                return (Visibility?)this.GetValue(GroupByExpansionIndicatorVisibilityProperty);
			}
			set
			{
				this.SetValue( GroupByExpansionIndicatorVisibilityProperty, value );
			}
		}

				#endregion // GroupByExpansionIndicatorVisibility

				#region GroupBySummaryDisplayMode

		
		
		/// <summary>
		/// Identifies the <see cref="GroupBySummaryDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupBySummaryDisplayModeProperty = DependencyProperty.Register(
			"GroupBySummaryDisplayMode",
			typeof( GroupBySummaryDisplayMode ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( GroupBySummaryDisplayMode.Default )
			);

		/// <summary>
		/// Specifies how to display summary results inside each group-by record. Default is resolved to <b>SummaryCells</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>GroupBySummaryDisplayMode</b> property controls how summary calculation results are
		/// displayed inside each group-by record. See help for <see cref="GroupBySummaryDisplayMode"/> 
		/// enum for available options.
		/// </para>
		/// </remarks>
		/// <seealso cref="GroupBySummaryDisplayMode"/>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		/// <seealso cref="SummaryDefinition.DisplayArea"/>
		//[Description( "Specifies how to display summaries inside each group-by record." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public GroupBySummaryDisplayMode GroupBySummaryDisplayMode
		{
			get
			{
				return (GroupBySummaryDisplayMode)this.GetValue( GroupBySummaryDisplayModeProperty );
			}
			set
			{
				this.SetValue( GroupBySummaryDisplayModeProperty, value );
			}
		}

				#endregion // GroupBySummaryDisplayMode
	
                // JJD 1/15/09 - NA 2009 vol 1 
				#region HeaderPlacement

		/// <summary>
		/// Identifies the <see cref="HeaderPlacement"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderPlacementProperty = DependencyProperty.Register("HeaderPlacement",
			typeof(HeaderPlacement), typeof(FieldLayoutSettings), 
			new FrameworkPropertyMetadata(HeaderPlacement.Default,
				new PropertyChangedCallback( OnHeaderPlacementChanged )
			)
			, new ValidateValueCallback(IsHeaderPlacementValid)
		);

        private static bool IsHeaderPlacementValid(object value)
        {
            return Enum.IsDefined(typeof(HeaderPlacement), value);
        }

		private HeaderPlacement _cachedHeaderPlacement = HeaderPlacement.Default;
		private static void OnHeaderPlacementChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings fieldLayoutSettings = (FieldLayoutSettings)dependencyObject;
			HeaderPlacement newVal = (HeaderPlacement)e.NewValue;

			fieldLayoutSettings._cachedHeaderPlacement = newVal;
		}

        /// <summary>
        /// Gets/sets the placement of headers
        /// </summary>
        /// <remarks>
        /// <para class="note">
        /// <b>Note:</b> This setting is ignored unless <see cref="LabelLocation"/> resolves to 'SeparateHeader' and the view supports separate headers.
        /// </para>
        /// </remarks>
        /// <seealso cref="FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.HeaderPlacement"/>
        /// <seealso cref="FieldLayout.HasSeparateHeader"/>
        /// <seealso cref="HeaderPlacementProperty"/>
		/// <seealso cref="HeaderPlacementInGroupBy"/>
        //[Description("Gets/sets the placement of headers")]
        //[Category("Behavior")]
		[Bindable(true)]
		public HeaderPlacement HeaderPlacement
        {
            get
            {
				return this._cachedHeaderPlacement;
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HeaderPlacementProperty, value);
            }
        }
				#endregion //HeaderPlacement
	
                // JJD 1/15/09 - NA 2009 vol 1 
				#region HeaderPlacementInGroupBy

		/// <summary>
		/// Identifies the <see cref="HeaderPlacementInGroupBy"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderPlacementInGroupByProperty = DependencyProperty.Register("HeaderPlacementInGroupBy",
			typeof(HeaderPlacementInGroupBy), typeof(FieldLayoutSettings), 
			new FrameworkPropertyMetadata(HeaderPlacementInGroupBy.Default,
				new PropertyChangedCallback( OnHeaderPlacementInGroupByChanged )
			)
			, new ValidateValueCallback(IsHeaderPlacementInGroupByValid)
		);

        private static bool IsHeaderPlacementInGroupByValid(object value)
        {
            return Enum.IsDefined(typeof(HeaderPlacementInGroupBy), value);
        }

		private HeaderPlacementInGroupBy _cachedHeaderPlacementInGroupBy = HeaderPlacementInGroupBy.Default;
		private static void OnHeaderPlacementInGroupByChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings fieldLayoutSettings = (FieldLayoutSettings)dependencyObject;
			HeaderPlacementInGroupBy newVal = (HeaderPlacementInGroupBy)e.NewValue;

			fieldLayoutSettings._cachedHeaderPlacementInGroupBy = newVal;
		}

        /// <summary>
        /// Gets/sets the placement of headers when records are grouped
        /// </summary>
        /// <remarks>
        /// <para class="body">If this property is not explicitly set then <see cref="FieldLayout.HeaderPlacementInGroupByResolved"/> will resolve to 'WithDataRecords' unless <see cref="FieldLayoutSettings.HeaderPlacement"/> is explicitly set to 'OnTopOnly' or <see cref="FieldLayout.FilterUITypeResolved"/> returns 'LabelIcons'.</para>
        /// <para class="note">
        /// <b>Note:</b> This setting is ignored unless <see cref="LabelLocation"/> resolves to 'SeparateHeader' and the view supports separate headers.
        /// </para>
        /// </remarks>
        /// <seealso cref="FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.HeaderPlacementInGroupBy"/>
        /// <seealso cref="FieldLayout.HasSeparateHeader"/>
        /// <seealso cref="HeaderPlacementInGroupByProperty"/>
		/// <seealso cref="HeaderPlacement"/>
        /// <seealso cref="FieldLayoutSettings.FilterUIType"/>
        /// <seealso cref="FieldLayout.FilterUITypeResolved"/>
        //[Description("Gets/sets the placement of headers when records are grouped.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public HeaderPlacementInGroupBy HeaderPlacementInGroupBy
        {
            get
            {
				return this._cachedHeaderPlacementInGroupBy;
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HeaderPlacementInGroupByProperty, value);
            }
        }
				#endregion //HeaderPlacementInGroupBy

                #region HeaderPresenterStyle

        /// <summary>
        /// Identifies the <see cref="HeaderPresenterStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderPresenterStyleProperty = DependencyProperty.Register("HeaderPresenterStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// The style for the header area
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        //[Description("The style for the header area")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style HeaderPresenterStyle
        {
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.HeaderPresenterStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HeaderPresenterStyleProperty, value);
            }
        }

                #endregion //HeaderPresenterStyle

				#region HeaderLabelAreaStyle

		/// <summary>
		/// Identifies the <see cref="HeaderLabelAreaStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderLabelAreaStyleProperty = DependencyProperty.Register("HeaderLabelAreaStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style for a header's label area 
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="HeaderLabelAreaStyleProperty"/>
        //[Description("Gets/sets the style of the for a header's label area")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style HeaderLabelAreaStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.HeaderLabelAreaStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HeaderLabelAreaStyleProperty, value);
            }
        }

				#endregion //HeaderLabelAreaStyle

				#region HeaderPrefixAreaDisplayMode

		// SSP 6/3/09 - NAS9.2 Field Chooser
		// 

		/// <summary>
		/// Identifies the <see cref="HeaderPrefixAreaDisplayMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty HeaderPrefixAreaDisplayModeProperty = DependencyProperty.Register(
			"HeaderPrefixAreaDisplayMode",
			typeof( HeaderPrefixAreaDisplayMode ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( HeaderPrefixAreaDisplayMode.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies what to display in the header prefix area. Header prefix area is the area that is by default
		/// left of field labels, above the record selectors. Default is resolved to <b>None</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HeaderPrefixAreaDisplayMode</b> specifies what to display in the header prefix area. By default nothing
		/// is displayed in this area.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.HeaderPrefixAreaDisplayMode"/>
		/// <seealso cref="DataPresenterBase.ShowFieldChooser(FieldLayout,bool,bool,string)"/>
		//[Description( "Specifies what to display the area that's left of the field labels and over the record selector." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public HeaderPrefixAreaDisplayMode HeaderPrefixAreaDisplayMode
		{
			get
			{
				return (HeaderPrefixAreaDisplayMode)this.GetValue( HeaderPrefixAreaDisplayModeProperty );
			}
			set
			{
				this.SetValue( HeaderPrefixAreaDisplayModeProperty, value );
			}
		}

				#endregion // HeaderPrefixAreaDisplayMode

				#region HeaderPrefixAreaStyle

		/// <summary>
		/// Identifies the <see cref="HeaderPrefixAreaStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderPrefixAreaStyleProperty = DependencyProperty.Register("HeaderPrefixAreaStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style for a header's prefix area 
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="HeaderPrefixAreaStyleProperty"/>
        //[Description("Gets/sets the style of the for a header's prefix area")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style HeaderPrefixAreaStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.HeaderPrefixAreaStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HeaderPrefixAreaStyleProperty, value);
            }
        }

				#endregion //HeaderPrefixAreaStyle

                #region HighlightAlternateRecords

        /// <summary>
        /// Identifies the <see cref="HighlightAlternateRecords"/> dependency property
        /// </summary>
        [TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public static readonly DependencyProperty HighlightAlternateRecordsProperty = DependencyProperty.Register("HighlightAlternateRecords",
                typeof(Nullable<bool>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<bool>()));

        /// <summary>
        /// Gets/sets whether the <see cref="RecordPresenter"/> and <see cref="DataItemPresenter"/> <see cref="RecordPresenter.IsAlternate"/> properties will return true on every other <see cref="Record"/>. 
        /// </summary>
        /// <seealso cref="RecordPresenter.IsAlternate"/>
        /// <seealso cref="DataItemPresenter.IsAlternate"/>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayout.HighlightAlternateRecordsResolved"/>
        /// <remarks>This used from inside cell, label, field and record styles.</remarks>
        //[Description("Gets/sets whether the IsAlternate property will return true on every other record.")]
        //[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(System.Windows.NullableBoolConverter))]
        public Nullable<bool> HighlightAlternateRecords
        {
            get
            {
                return (Nullable<bool>)this.GetValue(FieldLayoutSettings.HighlightAlternateRecordsProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HighlightAlternateRecordsProperty, value);
            }
        }

                #endregion //HighlightAlternateRecords
	
				#region HighlightPrimaryField

		/// <summary>
		/// Identifies the <see cref="HighlightPrimaryField"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HighlightPrimaryFieldProperty = DependencyProperty.Register("HighlightPrimaryField",
			typeof(HighlightPrimaryField), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(HighlightPrimaryField.Default), new ValidateValueCallback(IsHighlightPrimaryFieldValid));

        private static bool IsHighlightPrimaryFieldValid(object value)
        {
            return Enum.IsDefined(typeof(HighlightPrimaryField), value);
        }

        /// <summary>
        /// Gets/sets whether the primary field will be highlighted
        /// </summary>
        /// <seealso cref="DataItemPresenter.HighlightAsPrimary"/>
        /// <seealso cref="Field.IsPrimary"/>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayout.PrimaryField"/>
        /// <seealso cref="FieldLayout.HighlightPrimaryFieldResolved"/>
        /// <remarks>This is used from inside cell, label, field and record styles.</remarks>
        //[Description("Gets/sets whether the primary field will be highlighted")]
        //[Category("Appearance")]
		[Bindable(true)]
		public HighlightPrimaryField HighlightPrimaryField
        {
            get
            {
                return (HighlightPrimaryField)this.GetValue(FieldLayoutSettings.HighlightPrimaryFieldProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.HighlightPrimaryFieldProperty, value);
            }
        }

				#endregion //HighlightPrimaryField
	
				#region LabelLocation

		/// <summary>
		/// Identifies the <see cref="LabelLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LabelLocationProperty = DependencyProperty.Register("LabelLocation",
			typeof(LabelLocation), typeof(FieldLayoutSettings), 
			new FrameworkPropertyMetadata(LabelLocation.Default,
				
				
				
				
				
				
				
				//null, new CoerceValueCallback(CoerceLabelLocation)
				new PropertyChangedCallback( OnLabelLocationChanged )
			)
			, new ValidateValueCallback(IsLabelLocationValid)
		);

        private static bool IsLabelLocationValid(object value)
        {
            return Enum.IsDefined(typeof(LabelLocation), value);
        }

		// JJD 4/26/07
		// Optimization - cache the property locally
		private LabelLocation _cachedLabelLocation = LabelLocation.Default;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnLabelLocationChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings fieldLayoutSettings = (FieldLayoutSettings)dependencyObject;
			LabelLocation newVal = (LabelLocation)e.NewValue;

			fieldLayoutSettings._cachedLabelLocation = newVal;
		}

        /// <summary>
        /// Gets/sets the preferred location of the labels
        /// </summary>
        /// <remarks>Not all panels support a separate header area for the labels. These panels will revert to having the labels with the fields.</remarks>
        /// <seealso cref="FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.LabelLocation"/>
        /// <seealso cref="FieldLayout.HasSeparateHeader"/>
        /// <seealso cref="LabelLocationProperty"/>
        /// <seealso cref="FieldLayout.LabelLocationResolved"/>
        //[Description("Gets/sets the preferred location of the labels")]
        //[Category("Behavior")]
		[Bindable(true)]
		public LabelLocation LabelLocation
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (LabelLocation)this.GetValue(FieldLayoutSettings.LabelLocationProperty);
				return this._cachedLabelLocation;
            }
            set
            {
                this.SetValue(FieldLayoutSettings.LabelLocationProperty, value);
            }
        }

				#endregion //LabelLocation

				#region MaxFieldsToAutoGenerate

		/// <summary>
		/// Identifies the <see cref="MaxFieldsToAutoGenerate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxFieldsToAutoGenerateProperty = DependencyProperty.Register("MaxFieldsToAutoGenerate",
				  typeof(int), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Gets/sets the maximum number of fields to auto-generate in the generated templates.
		/// </summary>
		/// <remarks>The property is ignored if <see cref="AutoArrangeCells"/> resolves to 'Never'.</remarks>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="AutoArrangeCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.AutoArrangeCells"/>
		/// <seealso cref="MaxFieldsToAutoGenerateProperty"/>
		//[Description("Gets/sets the maximum number of fields to auto-generate in the generated templates.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public int MaxFieldsToAutoGenerate
		{
			get
			{
				return (int)this.GetValue(FieldLayoutSettings.MaxFieldsToAutoGenerateProperty);
			}
			set
			{
				this.SetValue(FieldLayoutSettings.MaxFieldsToAutoGenerateProperty, value);
			}
		}

				#endregion //MaxFieldsToAutoGenerate

				#region MaxSelectedCells

		/// <summary>
		/// Identifies the <see cref="MaxSelectedCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxSelectedCellsProperty = DependencyProperty.Register("MaxSelectedCells",
			typeof(Nullable<int>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<int>()), new ValidateValueCallback(OnValidateMaxSelected));

		private static bool OnValidateMaxSelected(object value)
		{
			if (value is Nullable<int>)
			{
				if (!((Nullable<int>)value).HasValue)
					return true;

				return ((Nullable<int>)value).Value >= 0;
			}

			if (value is int)
				return ((int)value) >= 0;

			if (value == null)
				return true;

			return false;
		}

		/// <summary>
		/// Gets/sets the maximum number of cells that can be selected at any time.
		/// </summary>
		/// <seealso cref="MaxSelectedCellsProperty"/>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="FieldLayout.MaxSelectedCellsResolved"/>
		/// <seealso cref="MaxSelectedRecords"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Cells"/>
		//[Description("Gets/sets the maximum number of cells that can be selected at any time.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Nullable<int> MaxSelectedCells
		{
			get
			{
				return (Nullable<int>)this.GetValue(FieldLayoutSettings.MaxSelectedCellsProperty);
			}
			set
			{
				this.SetValue(FieldLayoutSettings.MaxSelectedCellsProperty, value);
			}
		}

				#endregion //MaxSelectedCells

				#region MaxSelectedRecords

		/// <summary>
		/// Identifies the <see cref="MaxSelectedRecords"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxSelectedRecordsProperty = DependencyProperty.Register("MaxSelectedRecords",
			typeof(Nullable<int>), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(new Nullable<int>()), new ValidateValueCallback(OnValidateMaxSelected));

		/// <summary>
		/// Gets/sets the maximum number of records that can be selected at any time.
		/// </summary>
		/// <seealso cref="MaxSelectedRecordsProperty"/>
		/// <seealso cref="FieldLayout"/>
		/// <seealso cref="FieldLayout.MaxSelectedRecordsResolved"/>
		/// <seealso cref="MaxSelectedCells"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Records"/>
		//[Description("Gets/sets the maximum number of records that can be selected at any time.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Nullable<int> MaxSelectedRecords
		{
			get
			{
				return (Nullable<int>)this.GetValue(FieldLayoutSettings.MaxSelectedRecordsProperty);
			}
			set
			{
				this.SetValue(FieldLayoutSettings.MaxSelectedRecordsProperty, value);
			}
		}
				#endregion //MaxSelectedRecords

				#region RecordFilterScope

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="RecordFilterScope"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty RecordFilterScopeProperty = DependencyProperty.Register(
			"RecordFilterScope",
			typeof( RecordFilterScope ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( RecordFilterScope.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Determines whether record filtering is done at the field-layout level or individual record collection level for
		/// child field layouts. Default is resolved to <b>SiblingDataRecords</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecordFilterScope</b> property determines whether record filtering is done at the field-layout level or 
		/// individual record collection level. This only matters for and affects child field layouts. If this property is set to 
		/// <b>AllRecords</b> then when the user enters a 
		/// filter criteria all records associated with the field-layout are applied that filer. If the property is set to 
		/// <b>SiblingDataRecords</b> then the filter criteria is only applied to data records of the particular record collection.
		/// Default is resolved to <i>SiblingDataRecords</i>.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> This property affects how you specify filter criteria in code. If this property is 
		/// set to <b>AllRecords</b> then you must use FieldLayout's <see cref="FieldLayout.RecordFilters"/> property.
		/// If this property is set to <b>SiblingDataRecords</b> then you must use the RecordManager's <see cref="RecordManager.RecordFilters"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordFilterScope"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/> 
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/> 
		/// <seealso cref="FieldLayout.RecordFilters"/> 
		/// <seealso cref="RecordManager.RecordFilters"/> 
		/// <seealso cref="FieldLayoutSettings.FilterAction"/> 
		//[Description( "Specifies whether to filter all records of the field-layout or just the particular record collection." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public RecordFilterScope RecordFilterScope
		{
			get
			{
				return (RecordFilterScope)this.GetValue( RecordFilterScopeProperty );
			}
			set
			{
				this.SetValue( RecordFilterScopeProperty, value );
			}
		}

				#endregion // RecordFilterScope

				#region RecordFiltersLogicalOperator

		// SSP 12/17/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="RecordFiltersLogicalOperator"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty RecordFiltersLogicalOperatorProperty = DependencyProperty.Register(
			"RecordFiltersLogicalOperator",
			typeof( LogicalOperator? ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether filter conditions across fields are to be combined using logical 'Or' or 'And'.
		/// Default is resolved to 'And'.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecordFiltersLogicalOperator</b> property specifies how record filters across fields are to 
		/// be combined. �Or� means only filters of one field are required to pass in order to filter the 
		/// record. �And� means filters of all fields are required to pass in order to filter the record.
		/// The default 'And' which means that filters of all fields are required to pass in order to filter
		/// the record.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordFilterScope"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/> 
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/> 
		/// <seealso cref="FieldLayout.RecordFilters"/> 
		/// <seealso cref="RecordManager.RecordFilters"/> 
		/// <seealso cref="FieldLayoutSettings.FilterAction"/> 
		//[Description( "Specifies whether filter conditions across fields are to be combined using logical 'Or' or 'And'." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<LogicalOperator> ) )]
		public LogicalOperator? RecordFiltersLogicalOperator
		{
			get
			{
				return (LogicalOperator?)this.GetValue( RecordFiltersLogicalOperatorProperty );
			}
			set
			{
				this.SetValue( RecordFiltersLogicalOperatorProperty, value );
			}
		}

				#endregion // RecordFiltersLogicalOperator

				#region RecordListControlStyle

		/// <summary>
		/// Identifies the <see cref="RecordListControlStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordListControlStyleProperty = DependencyProperty.Register("RecordListControlStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style used for RecordListControls created to display objects using this field layout
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <remarks>This is only used with a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> containing non-homogeneous data.</remarks>
        /// <seealso cref="FieldLayout"/>
        //[Description("Gets/sets the style used for RecordListControls created to display objects using this field layout")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style RecordListControlStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.RecordListControlStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.RecordListControlStyleProperty, value);
            }
        }

				#endregion //RecordListControlStyle
	
				#region RecordSelectorLocation

		/// <summary>
		/// Identifies the <see cref="RecordSelectorLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordSelectorLocationProperty = DependencyProperty.Register("RecordSelectorLocation",
			typeof(RecordSelectorLocation), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(RecordSelectorLocation.Default), new ValidateValueCallback(IsRecordSelectorLocationValid));

        private static bool IsRecordSelectorLocationValid(object value)
        {
            return Enum.IsDefined(typeof(RecordSelectorLocation), value);
        }

        /// <summary>
        /// Determines if and where <see cref="RecordSelector"/>s will be displayed relative to a <see cref="Record"/>'s cell area.
        /// </summary>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="RecordSelectorLocationProperty"/>
        /// <seealso cref="RecordSelectorStyle"/>
        /// <seealso cref="RecordSelectorStyleSelector"/>
        /// <seealso cref="FieldLayout.RecordSelectorLocationResolved"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordSelectorLocation"/>
        //[Description("Determines if and where RecordSelectors will be displayed relative to a record's cell area")]
        //[Category("Behavior")]
		[Bindable(true)]
		public RecordSelectorLocation RecordSelectorLocation
        {
            get
            {
                return (RecordSelectorLocation)this.GetValue(FieldLayoutSettings.RecordSelectorLocationProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.RecordSelectorLocationProperty, value);
            }
        }

				#endregion //RecordSelectorLocation

				#region RecordSelectorExtent

		/// <summary>
		/// Identifies the <see cref="RecordSelectorExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordSelectorExtentProperty = DependencyProperty.Register("RecordSelectorExtent",
			typeof(double), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(IsRecordSelectorExtentValid));

		private static bool IsRecordSelectorExtentValid(object value)
		{
			if (!(value is double) )
				return false;

			if ( double.IsNaN((double)value))
				return true;

			return (double)value >= 1.0;
		}

        /// <summary>
        /// Gets/sets the extent for the <see cref="RecordSelector"/>
        /// </summary>
		/// <remarks>
		/// Based on the <see cref="RecordSelectorLocation"/> this will represent its width or its height
		/// </remarks>
        /// <seealso cref="FieldLayout"/>
		/// <seealso cref="RecordSelectorLocation"/>
        /// <seealso cref="RecordSelectorExtentProperty"/>
        /// <seealso cref="FieldLayout.RecordSelectorExtentResolved"/>
		//[Description("Gets/sets the extent for the record selector")]
        //[Category("Appearance")]
		[Bindable(true)]
		public double RecordSelectorExtent
		{
            get
            {
                return (double)this.GetValue(FieldLayoutSettings.RecordSelectorExtentProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.RecordSelectorExtentProperty, value);
            }
        }

				#endregion //RecordSelectorExtent

				#region RecordSelectorStyle

		/// <summary>
		/// Identifies the <see cref="RecordSelectorStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordSelectorStyleProperty = DependencyProperty.Register("RecordSelectorStyle",
            typeof(Style), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets/sets the style for <see cref="RecordSelector"/>s
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="RecordSelectorStyleSelector"/>
        /// <seealso cref="RecordSelectorStyleProperty"/>
        //[Description("Gets/sets the style of the selector for the record")]
        //[Category("Appearance")]
		[Bindable(true)]
		public Style RecordSelectorStyle
		{
            get
            {
                return (Style)this.GetValue(FieldLayoutSettings.RecordSelectorStyleProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.RecordSelectorStyleProperty, value);
            }
        }

				#endregion //RecordSelectorStyle

				#region RecordSelectorStyleSelector

		/// <summary>
		/// Identifies the <see cref="RecordSelectorStyleSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordSelectorStyleSelectorProperty = DependencyProperty.Register("RecordSelectorStyleSelector",
            typeof(StyleSelector), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata());

        /// <summary>
        /// A callback used for supplying styles for <see cref="RecordSelector"/>s
        /// </summary>
		/// <remarks>
        /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
		/// </remarks>
		/// <seealso cref="FieldLayout"/>
        /// <seealso cref="RecordSelectorStyle"/>
        /// <seealso cref="RecordSelectorStyleSelectorProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        //[Description("A callback used for supplying styles for record selectors")]
        //[Category("Appearance")]
		public StyleSelector RecordSelectorStyleSelector
		{
            get
            {
                return (StyleSelector)this.GetValue(FieldLayoutSettings.RecordSelectorStyleSelectorProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.RecordSelectorStyleSelectorProperty, value);
            }
        }

				#endregion //RecordSelectorStyleSelector

				#region RecordSeparatorLocation

		
		
		/// <summary>
		/// Identifies the <see cref="RecordSeparatorLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordSeparatorLocationProperty = DependencyProperty.Register(
				"RecordSeparatorLocation",
				typeof( RecordSeparatorLocation? ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies if and where to display record separators. By default separator is displayed 
		/// between fixed and non-fixed records.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Record seprators are thin, 3d separator bars that are displayed between certain records.
		/// By default record separators are displayed between fixed and non-fixed records. However
		/// you can set this property to display record separators after filter record, add-record and
		/// summary record. The enum is a flagged enum and thus multiple options can be
		/// combined to display record separators at multiple records.
		/// </para>
		/// </remarks>
		//[Description( "Specifies if and where to display record separators" )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<RecordSeparatorLocation>))] // AS 5/15/08 BR32816
		public RecordSeparatorLocation? RecordSeparatorLocation
		{
			get
			{
				return (RecordSeparatorLocation?)this.GetValue( RecordSeparatorLocationProperty );
			}
			set
			{
				this.SetValue( RecordSeparatorLocationProperty, value );
			}
		}

				#endregion // RecordSeparatorLocation

				#region ReevaluateFiltersOnDataChange

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Identifies the <see cref="ReevaluateFiltersOnDataChange"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ReevaluateFiltersOnDataChangeProperty = DependencyProperty.Register(
			"ReevaluateFiltersOnDataChange",
			typeof( bool? ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether to re-evaluate record filters when cell data changes. Default is resolved to <b>True</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default the data presenter will re-evaluate filters on a record if that record's data changes. It will do
		/// so whenever cell data changes, including when data is changed by user input or in code either through the 
		/// data presenter object model or through the data source directly. To prevent this behavior, you can set 
		/// <b>ReevaluateFiltersOnDataChange</b> to false. However note that the record filters will get re-evaluated if the
		/// filter criteria is modified.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataRecord.RefreshFilters"/>
		/// <seealso cref="RecordFilterCollection.Refresh"/>
		/// <seealso cref="DataRecord.IsFilteredOut"/>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		//[Description( "Specifies whether to re-evaluate record filters when cell data changes." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<bool> ) )]
		public bool? ReevaluateFiltersOnDataChange
		{
			get
			{
				return (bool?)this.GetValue( ReevaluateFiltersOnDataChangeProperty );
			}
			set
			{
				this.SetValue( ReevaluateFiltersOnDataChangeProperty, value );
			}
		}

				#endregion // ReevaluateFiltersOnDataChange
	
				#region ResizingMode

		/// <summary>
		/// Identifies the <see cref="ResizingMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizingModeProperty = DependencyProperty.Register("ResizingMode",
			typeof(ResizingMode), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(ResizingMode.Default), new ValidateValueCallback(IsResizingModeValid));

        private static bool IsResizingModeValid(object value)
        {
            return Enum.IsDefined(typeof(ResizingMode), value);
        }

        /// <summary>
        /// Determines if and how cells and labels are resized by the user.
        /// </summary>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayout.HeaderAreaTemplate"/>
        /// <seealso cref="ResizingModeProperty"/>
        /// <seealso cref="FieldLayout.ResizingModeResolved"/>
		/// <seealso cref="Infragistics.Windows.Controls.ResizingMode"/>
        //[Description("Determines if and how cells and labels are resized by the user.")]
        //[Category("Behavior")]
		[Bindable(true)]
		public ResizingMode ResizingMode
        {
            get
            {
                return (ResizingMode)this.GetValue(FieldLayoutSettings.ResizingModeProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.ResizingModeProperty, value);
            }
        }

				#endregion //ResizingMode
	
				#region SelectionTypeCell

		/// <summary>
		/// Identifies the <see cref="SelectionTypeCell"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionTypeCellProperty = DependencyProperty.Register("SelectionTypeCell",
            typeof(SelectionType), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(SelectionType.Default), new ValidateValueCallback(IsSelectionTypeValid));

        /// <summary>
        /// Determines hows <see cref="Cell"/>s can be selected
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayout.SelectionTypeCellResolved"/>
        /// <seealso cref="SelectionTypeRecord"/>
        /// <seealso cref="SelectionTypeField"/>
        //[Description("Determines hows cells can be selected")]
		//[Category("Behavior")]
		[Bindable(true)]
		public SelectionType SelectionTypeCell
        {
            get
            {
                return (SelectionType)this.GetValue(FieldLayoutSettings.SelectionTypeCellProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.SelectionTypeCellProperty, value);
            }
        }

				#endregion //SelectionTypeCell
	
				#region SelectionTypeField

		/// <summary>
		/// Identifies the <see cref="SelectionTypeField"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionTypeFieldProperty = DependencyProperty.Register("SelectionTypeField",
            typeof(SelectionType), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(SelectionType.Default), new ValidateValueCallback(IsSelectionTypeValid));

        /// <summary>
        /// Determines hows <see cref="Field"/>s can be selected
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayout.SelectionTypeFieldResolved"/>
        /// <seealso cref="SelectionTypeRecord"/>
        /// <seealso cref="SelectionTypeCell"/>
        //[Description("Determines hows fields can be selected")]
		//[Category("Behavior")]
		[Bindable(true)]
		public SelectionType SelectionTypeField
        {
            get
            {
                return (SelectionType)this.GetValue(FieldLayoutSettings.SelectionTypeFieldProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.SelectionTypeFieldProperty, value);
            }
        }

				#endregion //SelectionTypeField
	
				#region SelectionTypeRecord

		/// <summary>
		/// Identifies the <see cref="SelectionTypeRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionTypeRecordProperty = DependencyProperty.Register("SelectionTypeRecord",
            typeof(SelectionType), typeof(FieldLayoutSettings), new FrameworkPropertyMetadata(SelectionType.Default), new ValidateValueCallback(IsSelectionTypeValid));

        private static bool IsSelectionTypeValid(object value)
        {
            return Enum.IsDefined(typeof(SelectionType), value);
        }

        /// <summary>
        /// Determines hows <see cref="Record"/>s can be selected
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayout.SelectionTypeRecordResolved"/>
        /// <seealso cref="SelectionTypeField"/>
        /// <seealso cref="SelectionTypeCell"/>
        //[Description("Determines hows records can be selected")]
		//[Category("Behavior")]
		[Bindable(true)]
		public SelectionType SelectionTypeRecord
        {
            get
            {
                return (SelectionType)this.GetValue(FieldLayoutSettings.SelectionTypeRecordProperty);
            }
            set
            {
                this.SetValue(FieldLayoutSettings.SelectionTypeRecordProperty, value);
            }
        }

				#endregion //SelectionTypeRecord

				#region SortEvaluationMode

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 

		/// <summary>
		/// Identifies the <see cref="SortEvaluationMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public static readonly DependencyProperty SortEvaluationModeProperty = DependencyPropertyUtilities.Register(
			"SortEvaluationMode",
			typeof( SortEvaluationMode ),
			typeof( FieldLayoutSettings ),
			DependencyPropertyUtilities.CreateMetadata( SortEvaluationMode.Default )
		);

		/// <summary>
		/// Specifies how the data presenter will perform sorting operation.
		/// </summary>
		/// <seealso cref="SortEvaluationModeProperty"/>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public SortEvaluationMode SortEvaluationMode
		{
			get
			{
				return (SortEvaluationMode)this.GetValue( SortEvaluationModeProperty );
			}
			set
			{
				this.SetValue( SortEvaluationModeProperty, value );
			}
		}

				#endregion // SortEvaluationMode

				#region SpecialRecordOrder

		
		
		/// <summary>
		/// Identifies the <see cref="SpecialRecordOrder"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SpecialRecordOrderProperty = DependencyProperty.Register(
				"SpecialRecordOrder",
				typeof( SpecialRecordOrder ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( null , new PropertyChangedCallback( OnSpecialRecordOrderChanged ) )
			);

		/// <summary>
		/// Specifies the order of special records, like filter record, summary record, add-record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SpecialRecordOrder</b> can be used to specify the order in which special records like filter record,
		/// summary record and add-record are displayed inside each record collection. The associated integer
		/// value on the SpecialRecordOrder object controls the order - special records with lower order value
		/// will be displayed before the special records with higher order value.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the order of special records." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public SpecialRecordOrder SpecialRecordOrder
		{
			get
			{
				return (SpecialRecordOrder)this.GetValue( SpecialRecordOrderProperty );
			}
			set
			{
				this.SetValue( SpecialRecordOrderProperty, value );
			}
		}

		private static void OnSpecialRecordOrderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayoutSettings settings = (FieldLayoutSettings)dependencyObject;
			SpecialRecordOrder oldVal = (SpecialRecordOrder)e.OldValue;
			SpecialRecordOrder newVal = (SpecialRecordOrder)e.NewValue;

			if ( null != oldVal )
				oldVal.PropertyChanged -= new PropertyChangedEventHandler( settings.SpecialRecordOrder_PropertyChanged );

			if ( null != newVal )
				newVal.PropertyChanged += new PropertyChangedEventHandler( settings.SpecialRecordOrder_PropertyChanged );
		}

		private void SpecialRecordOrder_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			this.RaisePropertyChangedEvent( "SpecialRecordOrder" );
		}

				#endregion // SpecialRecordOrder

				#region SummaryDescriptionVisibility

		/// <summary>
		/// Identifies the <see cref="SummaryDescriptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryDescriptionVisibilityProperty = DependencyProperty.Register(
				"SummaryDescriptionVisibility",
				typeof( Visibility? ),
				typeof( FieldLayoutSettings ),
				new FrameworkPropertyMetadata( null )
			);


		/// <summary>
		/// Specifies whether summary records display description. Default is resolved to <b>Collapsed</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can display a description of summaries above the summary record by setting <b>SummaryDescriptionVisibility</b>
		/// to <b>Visible.</b> By default summary description is not displayed. You can specify the description to display
		/// using FieldLayout's <see cref="FieldLayout.SummaryDescriptionMask"/>,
		/// <see cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/> and SummaryResultCollection's
		/// <see cref="SummaryResultCollection.SummaryRecordHeader"/> properties.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDescriptionMask"/>
		/// <seealso cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/>
		/// <seealso cref="SummaryResultCollection.SummaryRecordHeader"/>
		//[Description( "Specifies whether summary records display description." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<Visibility>))] // AS 5/15/08 BR32816
		public Visibility? SummaryDescriptionVisibility
		{
			get
			{
				return (Visibility?)this.GetValue( SummaryDescriptionVisibilityProperty );
			}
			set
			{
				this.SetValue( SummaryDescriptionVisibilityProperty, value );
			}
		}

				#endregion // SummaryDescriptionVisibility

				#region SummaryEvaluationMode

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 

		/// <summary>
		/// Identifies the <see cref="SummaryEvaluationMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public static readonly DependencyProperty SummaryEvaluationModeProperty = DependencyPropertyUtilities.Register(
			"SummaryEvaluationMode",
			typeof( SummaryEvaluationMode ),
			typeof( FieldLayoutSettings ),
			DependencyPropertyUtilities.CreateMetadata( SummaryEvaluationMode.Default )
		);

		/// <summary>
		/// Specifies how summaries are calculated by the data presenter.
		/// </summary>
		/// <seealso cref="SummaryEvaluationModeProperty"/>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryEvaluationMode</b> property is used to control how the data presenter calculates summaries.
		/// By default it calculates the summaries by enumerating all the records (in the process allocating them if 
		/// they are not already allocated) and getting the cell value of each record and aggregating the values
		/// using the <see cref="SummaryDefinition.Calculator"/> associated with the <see cref="SummaryDefinition"/>.
		/// You can set this property to <see cref="Infragistics.Windows.DataPresenter.SummaryEvaluationMode.UseLinq"/>
		/// to have the data presenter use LINQ to perform summary calculations. Note that any converter settings
		/// specified on the Field, like <see cref="Field.Converter"/>, will not be utilized when using LINQ.
		/// </para>
		/// <para class="body">
		/// Also note that the data presenter raises <see cref="DataPresenterBase.QuerySummaryResult"/>
		/// event before it starts the process of calculation for each summary result. You can use the <i>QuerySummaryResult</i> event 
		/// to calculate the summary result and provide the calculated value to the data presenter via the <see cref="Infragistics.Windows.DataPresenter.Events.QuerySummaryResultEventArgs.SetSummaryValue"/>
		/// method of the associated event args. Doing so will cause the data presenter to use the provided value instead of performing its
		/// own calculation. Furthermore, this property's settings will be ignored when you provide your own calculate value via this event.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataPresenterBase.QuerySummaryResult"/>
		[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
		public SummaryEvaluationMode SummaryEvaluationMode
		{
			get
			{
				return (SummaryEvaluationMode)this.GetValue( SummaryEvaluationModeProperty );
			}
			set
			{
				this.SetValue( SummaryEvaluationModeProperty, value );
			}
		}

				#endregion // SummaryEvaluationMode

				#region SupportDataErrorInfo

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the <see cref="SupportDataErrorInfo"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty SupportDataErrorInfoProperty = DependencyProperty.Register(
			"SupportDataErrorInfo",
			typeof( SupportDataErrorInfo ),
			typeof( FieldLayoutSettings ),
			new FrameworkPropertyMetadata( SupportDataErrorInfo.Default, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Specifies whether to display error information provided by the IDataErrorInfo implementation 
		/// of the underlying data items from the bound data source. Default value is resolved to 
		/// <b>None</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SupportDataErrorInfo</b> property specifies whether to display error information provided
		/// by the data items associated with data records. Data items provide error information by
		/// implementing <b>IDataErrorInfo</b> interface. The interface provides error information for
		/// the the entire data item as well as for each individual field. The data presenter displays
		/// the data item error in the record selector of the data record and individual field error in
		/// the associated cell.
		/// </para>
		/// <para class="body">
		/// The error information is displayed in the form of an error icon, which 
		/// displays a tooltip with the error text when the mouse is hovered over.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is resolved to <b>None</b>, which means
		/// that by default the data error information is not displayed. You need to set this property 
		/// to enable this functionality.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the <i>FieldSettings</i> object exposes <see cref="FieldSettings.SupportDataErrorInfo"/>
		/// property. You can use that to show or hide a particular field's error information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SupportDataErrorInfo"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SupportDataErrorInfo"/>
		/// <seealso cref="DataPresenterBase.DataErrorContentTemplateKey"/>
		/// <seealso cref="DataPresenterBase.DataErrorIconStyleKey"/>
		//[Description( "Specifies whether to display error information provided by IDataErrorInfo." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public SupportDataErrorInfo SupportDataErrorInfo
		{
			get
			{
				return (SupportDataErrorInfo)this.GetValue( SupportDataErrorInfoProperty );
			}
			set
			{
				this.SetValue( SupportDataErrorInfoProperty, value );
			}
		}

				#endregion // SupportDataErrorInfo
    
 			#endregion // Public Properties

			#region Internal Properties

				#region DataRecordCellAreaTemplateGrid

		internal Grid DataRecordCellAreaTemplateGrid { get { return this._dataRecordCellAreaTemplateGrid; } }

				#endregion //DataRecordCellAreaTemplateGrid

			#endregion //Internal Properties	
        
		#endregion //Properties

        #region Methods

			#region Public Methods

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

			#endregion // Public Methods

            #region Internal Methods

                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                #region ResetExcludedSettings

        internal void ResetExcludedSettings(ReportViewBase view)        
        {
            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (view.ExcludeRecordSizing)
            if (view != null && view.ExcludeRecordSizing)
            {
                this.ClearValue(DataRecordSizingModeProperty);
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