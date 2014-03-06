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
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using System.Collections.ObjectModel;
using System.Windows.Interop;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Markup;
using System.Collections.Specialized;

namespace Infragistics.Windows.DataPresenter
{
    #region FieldLayout

    /// <summary>
	/// Used in a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> to define the layout of <see cref="Field"/>s for one or more items. 
	/// </summary>
	/// <remarks>
	/// <para class="body"><see cref="DataRecord"/>s are created lazily as each item in the <see cref="DataPresenterBase.DataSource"/> is requested. 
	/// When a <see cref="DataRecord"/> is created <see cref="DataPresenterBase"/>'s <see cref="DataPresenterBase.FieldLayouts"/> collection is searched for an existing <see cref="FieldLayout"/> 
	/// whose <see cref="FieldLayout.Fields"/> match the <see cref="DataRecord.DataItem"/>'s properties. If one is not found then
	/// a new <see cref="FieldLayout"/> is created, in which case the <see cref="DataPresenterBase.FieldLayoutInitializing"/> and <see cref="DataPresenterBase.FieldLayoutInitialized"/> events will be raised.</para>
	/// <para></para>
	/// <para class="note"><b>Note: </b>If the new <see cref="FieldLayout"/>'s <see cref="FieldLayout.AutoGenerateFieldsResolved"/> property returns true then the <see cref="FieldLayout.Fields"/> collection is automatically populated with a <see cref="Field"/> for every public property on the data item. This is done between the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutInitializing"/> and <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutInitialized"/> events.</para>
	/// <para></para>
	/// <para class="body">In any case the <see cref="DataPresenterBase.AssigningFieldLayoutToItem"/> event is raised to allow a different <see cref="FieldLayout"/> to be assigned to the <see cref="DataRecord"/>. Finally the <see cref="DataPresenterBase.InitializeRecord"/> event is raised.</para>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Layout.html">Field Layout</a> topic in the Developer's Guide for an explanation of the FieldLayout object.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Settings"/>
	/// <seealso cref="FieldSettings"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/>
	[ContentProperty("Fields")]
	[DefaultProperty("Fields")]
	public class FieldLayout : DependencyObjectNotifier
	{
		#region Private Members

        // JJD 5/26/09 - TFS17590
        // Added a DataColumn map to use during field initialization to get each column's caption
        // when initializing from a DataView data source.
        private Dictionary<string, DataColumn> _dataColumnMap;
 
		private FieldCollection                 _fields;
		private Field                           _primaryField;
        private DataPresenterBase               _presenter;
		private bool                            _isdefault;
 		private bool                            _isInitialized;
        // JJD 7/21/08 - BR34098 - flag no longer used
        //private bool _invalidationPending;
		private bool							_areFieldsInitialized;
        private bool                            _keyExplicitlySet;
        private bool                            _initialRecordLoaded;
        private bool                            _isInFieldInitialization;

		// JJD 7/18/07 - BR24617
		// Hold a flag that lets us know if the fields were auto generated during initialization
        private bool                            _areFieldsAutoGenerated;

        // JJD 12/13/07
        // Added a flag so we know if this layout was initialized with an EnumerableObjectWrapper 
        private bool                            _wrapsEnumerableObject;

		// JJD 08/17/12 - TFS119037 - added
		private bool							_wasRemovedFromCollection;

        




		private FieldLayoutTemplateGenerator    _styleGenerator;
        private int                             _totalRowsGenerated;
        private int                             _totalColumnsGenerated;
        private int                             _sortVersion;
        private int                             _groupByVersion;
		private int								_index;
        private FieldLayoutSettings             _settings;
        private FieldSettings                   _cachedFieldSettings = null;
        private FieldSortDescriptionCollection  _sortedFields;
        private SelectionStrategyBase           _selectionStrategyCell;
        private SelectionStrategyBase           _selectionStrategyField;
        private SelectionStrategyBase           _selectionStrategyRecord;
        private string                          _cachedDescription;
        private object                          _cachedKey = null;
		private List<WeakReference>				_propertyDescriptorProviders;
		private List<HeaderPresenter>			_headerPresenterCache = null;
		private int								_primaryFieldVersion;
		private Field                           _scrollTipField;
		private int								_scrollTipFieldVersion;
		private DataRecord						_templateDataRecord;
		private TemplateDataRecordCache			_templateDataRecordCache;
        
        // JJD 4/16/09 - NA 2009 vol 2 
        private FieldLayout                     _parentFieldLayout;
        private Field                           _parentField;
        private bool                            _keyMatchingEnforced;

		// JJD 4/27/11 - TFS73888 - added
		private HashSet							_pendingFieldsToRefresh;

        // AS 1/5/09 NA 2009 Vol 1 - Fixed Fields
		//private bool							_reinitializeSizeManagers;

		// AS 8/25/09 TFS17560
		private GroupByRecord					_templateGroupByRecord;

		
		
		private SummaryDefinitionCollection		_summaryDefinitions;
		private int								_cachedSpecialRecordsVersion = 1;


		
		
		
		// SSP 1/6/09 TFS11860
		// When a field is added or an existing collapsed field is made visible, we need
		// to make sure it doesn't overlap with other fields. Also if the field was collapsed,
		// it should appear close to where it was when it was collapsed. Therefore we need
		// to keep track of where it was before it was collapsed so when its made visible, 
		// we put it close to where it was before.
		// 
		
		// SSP 6/26/09 - NAS9.2 Field Chooser
		// Renamed _dragFieldLayoutInfo to _fieldLayoutInfo and made it private and instead 
		// added GetFieldLayoutInfo and SetFieldLayoutInfo methods.
		// 
		private LayoutInfo _fieldLayoutInfo;

		internal LayoutInfo _autoGeneratedPositions;

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		private RecordFilterCollection _recordFilters;

		// SSP 6/3/09 - TFS17233 - Optimization
		// 
		private RecordFilterAction _cachedRecordFilterActionResolved;
		private LogicalOperator? _cachedRecordFiltersLogicalOperatorResolved;
		private RecordFilterScope? _cachedRecordFilterScopeResolvedDefault;

		// SSP 12/21/11 TFS67264 - Optimizations
		// 
		private bool? _cachedReevaluateFiltersOnDataChange;

        // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
        private FieldGridBagLayoutManager       _cellLayoutManager;
        private FieldGridBagLayoutManager       _labelLayoutManager;
        private int                             _verifiedLayoutManagerVersion = -1;
        private int                             _layoutManagerVersion = 0;
        private int                             _layoutItemVersion = 1;
        private int                             _fixedFieldVersion;
        private FixedFieldLayoutInfo            _fixedFieldInfo;
        
        // JJD 1/15/09 - NA 2009 vol 1 - record filtering
        private Size                            _expansionIndicatorSize;

        // JJD 1/19/09 - NA 2009 vol 1 - record filtering
        private int                             _indentOffsetVersion;

        // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
        private bool                            _createDragFieldLayout;

        // AS 2/20/09 TFS7941
        private DispatcherOperation             _bumpLayoutItemOp;

		// AS 7/7/09 TFS19145
		// SSP 2/4/10 TFS25283
		// Use the InvalidateGeneratedStylesAsnyc instead of having a separate async operation.
		// 
		//private DispatcherOperation             _fieldVisibilityChangeOp;

        // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
        private bool                            _hasBeenInitializedAfterDataSourceChange;

        // JJD 6/2/09 - TFS17867 - added
        private DispatcherOperation             _bumpRecordIndentVersionPending;

		// AS 6/22/09 NA 2009.2 Field Sizing
		private AutoFitState?					_autoFitState;
		private AutoSizeFieldLayoutInfo			_autoSizeInfo;
		private int								_maxRecordManagerDepth;

		// JM 07-29-09 TFS 19241
		private bool							_fieldLayoutInitializedEventRaised;

		// SSP 9/2/09 TFS17893
		// 
		private GridUtilities.InvalidateStylesAsyncInfo _pendingInvalidateGeneratedStyles;

		// AS 10/9/09 NA 2010.1 - CardView
		private bool? _hasStarFieldsX = null;
		private bool? _hasStarFieldsY = null;

		// AS 10/13/09 NA 2010.1 - CardView
		private CellPresentation? _cachedCellPresentation;
		private bool? _cachedIsEmptyCellCollapsingSupportedByView;

		// MD 5/26/10 - ChildRecordsDisplayOrder feature
		// We want to cache the resolved value so we don't have to resolve it all the time.
		private ChildRecordsDisplayOrder? _cachedChildRecordsDisplayOrderResolved;

        // SSP 6/7/10 - Optimizations - TFS34031
        // Cache resolved property value.
        // 
        private SupportDataErrorInfo? _cachedSupportDataErrorInfoResolvedDefault;

		// MD 8/13/10
		// Added a way to bypass the FieldLayout.BumpLayoutManagerVersionRequired() logic. 
		private bool _preventBumpLayoutManagerVersionRequired;

		// AS 11/29/10 TFS60418
		// The IsHorizontal state doesn't change often and could be dependant on a DP
		// which has some overhead when retreiving the value.
		//
		private bool? _cachedIsHorizontal = null;

		// AS 4/12/11 TFS62951
		// We used to cache a strong reference on the Field to the last LabelPresenter 
		// for that field whose ISelectableElement.SelectableItem was requested. The 
		// main issue was that when the label was recycled the field wouldn't return 
		// it anymore. Since we only need it for the purposes of getting to the containing 
		// record presenter we'll cache it on the field layout.
		//
		private WeakReference _lastSelectableItemLabel;

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private SortEvaluationMode? _cachedSortEvaluationModeResolved;
		private FilterEvaluationMode? _cachedFilterEvaluationModeResolved;
		private GroupByEvaluationMode? _cachedGroupByEvaluationModeResolved;
		private SummaryEvaluationMode? _cachedSummaryEvaluationModeResolved;

		#endregion //Private Members

        #region Constants

        private const double DefaultRecordSelectorBaseExtent = 20.0;
        private const double DefaultRecordSelectorErrorIconExtent = 10.0;
        private const double DefaultRecordSelectorFixButtonExtent = 12.0;

        #endregion //Constants	
    
		#region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="FieldLayout"/> class
        /// </summary>
		public FieldLayout()
		{
            // JJD 12/21/07
            // Delay settinbg this property until InitializeOwner is called so we only
            // do it at run time. This will allow the VS2008 designer to work properly 
            //this.SetValue(FieldSettingsProperty, this.FieldSettings);

			
			
			
			
			this._fields = new FieldCollection( this );
			// JJD 3/6/07 - BR20877
			// Listen for changes to the fields collections
			this._fields.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( OnFieldCollectionChanged );

			// AS 7/1/09 NA 2009.2 Field Sizing
			_autoSizeInfo = new AutoSizeFieldLayoutInfo(this);
 		}

		#endregion //Constructors

        #region Base class overrids

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if ( property == DescriptionProperty )
			{
				this._cachedDescription = e.NewValue as string;

				// SSP 3/21/12 Calc
				// 
				GridUtilities.NotifyCalcAdapter( _presenter, this, "Description", null );
			}
			else if ( property == KeyProperty )
			{
				this._cachedKey = e.NewValue;
				this._keyExplicitlySet = true;

				// JJD 4/15/09 
				// Make sure the Description property is initialized which can depend
				// on the Key value set above
				this.CoerceValue( FieldLayout.DescriptionProperty );
			}
			else if ( property == InternalVersionProperty )
			{
				if ( this._presenter != null )
					this._presenter.ClearStyleCache( );

				// make sure the records and template data record cache are updated
				// AS 7/7/09 TFS19145
				//this.TemplateVersion++;
				this.BumpTemplateVersion( true );
			}
			else if ( property == FieldSettingsProperty )
			{
				// unhook old event handler
				if ( e.OldValue != null )
					( (FieldSettings)( e.OldValue ) ).PropertyChanged -= new PropertyChangedEventHandler( this.OnFieldSettingsPropertyChanged );

				// JJD 12/21/07
				// Only hook the PropertyChanged if the NewValue is different from the cached member
				if ( (FieldSettings)( e.NewValue ) != this._cachedFieldSettings )
				{
					this._cachedFieldSettings = (FieldSettings)( e.NewValue );

					// hook into PropertyChanged event
					if ( e.NewValue != null )
						( (FieldSettings)( e.NewValue ) ).PropertyChanged += new PropertyChangedEventHandler( this.OnFieldSettingsPropertyChanged );
				}
			}
            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


            this.RaisePropertyChangedEvent(property.Name);
        }

            #endregion //OnPropertyChanged

			#region ToString

		/// <summary>
		/// Gets a string representation for this object
		/// </summary>
		/// <returns>A string representation for this object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.Description);
			sb.Append(" - ");
			sb.Append(this.Fields.Count.ToString());
			sb.Append(" Fields");

			if (this._presenter != null)
			{
				sb.Append(" - index = ");
				sb.Append(this._presenter.FieldLayouts.IndexOf(this).ToString());
			}






			return sb.ToString();
		}

			#endregion //ToString	
    
        #endregion //Base class overrids	

		#region Properties

			#region Public Properties

                #region AddNewRecordLocationResolved

        /// <summary>
		/// Determines how the add record UI is presented to the user (read-only)
        /// </summary>
		/// <remarks>This property is ignored if the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/> does not support adding of records (i.e. does not implement the <see cref="System.ComponentModel.IBindingList"/> interface or that interface's <see cref="System.ComponentModel.IBindingList.AllowNew"/> property returns false).</remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AddNewRecordLocation"/>
        /// <seealso cref="FieldLayoutSettings.AddNewRecordLocationProperty"/>
        /// <seealso cref="FieldLayoutSettings.AllowAddNew"/>
		/// <seealso cref="AllowAddNewResolved"/>
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public AddNewRecordLocation AddNewRecordLocationResolved
        {
            get
            {
                AddNewRecordLocation setting;

                if (this._settings != null )
                {
                    setting = this._settings.AddNewRecordLocation;
                    if ( setting != AddNewRecordLocation.Default)
                        return setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.AddNewRecordLocation;

					if (setting != AddNewRecordLocation.Default)
						return setting;
				}

                return AddNewRecordLocation.OnTopFixed;
            }
        }

                #endregion //AddNewRecordLocationResolved

                #region AllowAddNewResolved

        /// <summary>
		/// Determines whether the user can add records (read-only).
        /// </summary>
		/// <remarks>This property is ignored if the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/> does not support adding of records (i.e. does not implement the <see cref="System.ComponentModel.IBindingList"/> interface or that interface's <see cref="System.ComponentModel.IBindingList.AllowNew"/> property returns false).</remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AllowAddNew"/>
        /// <seealso cref="FieldLayoutSettings.AllowAddNewProperty"/>
		/// <seealso cref="FieldLayoutSettings.AddNewRecordLocation"/>
		/// <seealso cref="AddNewRecordLocationResolved"/>
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public bool AllowAddNewResolved
        {
            get
            {
                // JJD 9/29/08
                // In a report we neer want to allow add new records to show
                if (this._presenter != null && this._presenter.IsReportControl)
                    return false;

                Nullable<bool> setting;

                if (this._settings != null )
                {
                    setting = this._settings.AllowAddNew;
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.AllowAddNew;
                    
                    if ( setting.HasValue)
                        return (bool)setting;
                }

				// JJD 3/13/07 - BR21067
				// We should not show add new records by default
                return false;
                //return true;
            }
        }

                #endregion //AllowAddNewResolved

                #region AllowDeleteResolved

		/// <summary>
		/// Determines whether the user can delete records (read-only).
		/// </summary>
		/// <remarks>This property is ignored if the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/> does not support deleting of records.</remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AllowDelete"/>
        /// <seealso cref="FieldLayoutSettings.AllowDeleteProperty"/>
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public bool AllowDeleteResolved
        {
            get
            {
                Nullable<bool> setting;

                if (this._settings != null )
                {
                    setting = this._settings.AllowDelete;
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.AllowDelete;
                    
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                return true;
            }
        }

                #endregion //AllowDeleteResolved

				#region AllowFieldMovingResolved


		
		
		/// <summary>
		/// Determines whether the user can re-arrange fields.
		/// </summary>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		public AllowFieldMoving AllowFieldMovingResolved
		{
			get
			{
				AllowFieldMoving val = null != _settings ? _settings.AllowFieldMoving : AllowFieldMoving.Default;

				if ( AllowFieldMoving.Default == val )
				{
					FieldLayoutSettings settings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
					if ( null != settings )
						val = settings.AllowFieldMoving;

					if ( AllowFieldMoving.Default == val )
						val = AllowFieldMoving.Yes;
				}

				return val;
			}
		}

				#endregion // AllowFieldMovingResolved

                // JJD 6/12/09 - NA 2009 Vol 2 - record fixing
				#region AllowRecordFixingResolved

		/// <summary>
		/// Determines whether the user can fix records thru the UI (read-only).
		/// </summary>
		/// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public AllowRecordFixing AllowRecordFixingResolved
		{
			get
			{
				AllowRecordFixing val = null != _settings ? _settings.AllowRecordFixing : AllowRecordFixing.Default;

				if ( AllowRecordFixing.Default == val )
				{
					FieldLayoutSettings settings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
					if ( null != settings )
						val = settings.AllowRecordFixing;

					if ( AllowRecordFixing.Default == val )
						val = AllowRecordFixing.No;
				}

				return val;
			}
		}
				#endregion // AllowRecordFixingResolved

				// AS 11/8/11 TFS88111
				#region AutoFitToHeight
		/// <summary>
		/// Returns a boolean indicating if the layout is auto fit vertically
		/// </summary>
		public bool AutoFitToHeight
		{
			get { return this.IsAutoFitHeight; }
		}
				#endregion // AutoFitToHeight

				// AS 11/8/11 TFS88111
				#region AutoFitToWidth
		/// <summary>
		/// Returns a boolean indicating if the layout is auto fit horizontally
		/// </summary>
		public bool AutoFitToWidth
		{
			get { return this.IsAutoFitWidth; }
		}
				#endregion // AutoFitToWidth

                #region AutoGenerateFieldsResolved

        /// <summary>
        /// Determines whether <see cref="Fields"/> collection will be automatically populated with <see cref="Field"/>s for every property in the underlying data (read-only)
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>If <see cref="DataPresenterBase.BindToSampleData"/> is set to true then this property will always return true.</para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AutoGenerateFields"/>
        /// <seealso cref="FieldLayoutSettings.AutoGenerateFieldsProperty"/>
        /// <seealso cref="Fields"/>
        //[Description("Determines whether fields will be automatically generated (read-only)")]
        //[Category("Behavior")]
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public bool AutoGenerateFieldsResolved
        {
            get
            {
				// [JM/JD 3-28-07]
				if (this._presenter						!= null  &&
					this._presenter.BindToSampleData	== true  &&
					this._presenter.DataSource			== Infragistics.Windows.Internal.DataBindingUtilities.GetSampleData())
					return true;

                Nullable<bool> setting;

                if (this._settings != null )
                {
                    setting = this._settings.AutoGenerateFields;
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.AutoGenerateFields;
                    
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                return true;
            }
        }

                #endregion //AutoGenerateFieldsResolved

				#region CalculationReferenceId


		/// <summary>
		/// Identifies the <see cref="CalculationReferenceId"/> dependency property
		/// </summary>
		[InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]
		public static readonly DependencyProperty CalculationReferenceIdProperty = DependencyProperty.Register("CalculationReferenceId",
			typeof(string), typeof(FieldLayout), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCalculationReferenceIdChanged)));

		private static void OnCalculationReferenceIdChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FieldLayout fl = target as FieldLayout;

			fl._calculationRefernceId = e.NewValue as string;
		}

		private string _calculationRefernceId;

		/// <summary>
		/// Gets/sets the id that will be used to reference <see cref="Field"/>s and <see cref="SummaryDefinition"/>s inside a <b>XamCalculationManager</b> network.
		/// </summary>
		/// <remarks>
		/// <para class="body">If this property is not set then the <see cref="ParentFieldName"/> will be used. If that's not set then the 
		/// <see cref="Description"/> will be used.</para>
		/// <para class="note"><b>Note: </b> this property will be ignored unless <see cref="DataPresenterBase"/>'s  <see cref="DataPresenterBase.CalculationAdapter"/> is set.</para>
		/// </remarks>
		/// <seealso cref="CalculationReferenceIdProperty"/>
		[InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]
		public string CalculationReferenceId
		{
			get
			{
				return _calculationRefernceId;
			}
			set
			{
				this.SetValue(FieldLayout.CalculationReferenceIdProperty, value);
			}
		}

				#endregion //CalculationReferenceId

                #region CellPresentation

		private static readonly DependencyPropertyKey CellPresentationPropertyKey =
            DependencyProperty.RegisterReadOnly("CellPresentation",
            typeof(CellPresentation), typeof(FieldLayout), new FrameworkPropertyMetadata(CellPresentation.CardView));

        /// <summary>
        /// Identifies the <see cref="CellPresentation"/> dependency property
        /// </summary>
		public static readonly DependencyProperty CellPresentationProperty =
			CellPresentationPropertyKey.DependencyProperty;

        /// <summary>
		/// Returns the <see cref="Infragistics.Windows.DataPresenter.CellPresentation"/> of the generated styles
        /// </summary>
        /// <seealso cref="HeaderAreaTemplate"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
		public CellPresentation CellPresentation
        {
            get
            {
				// AS 10/13/09 NA 2010.1 - CardView
				//return (CellPresentation)this.GetValue(FieldLayout.CellPresentationProperty);
				if (_cachedCellPresentation == null)
				{
					_cachedCellPresentation = (CellPresentation)this.GetValue(FieldLayout.CellPresentationProperty);
				}

				return _cachedCellPresentation.Value;
            }
        }

                #endregion //CellPresentation

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				#region ChildRecordsDisplayOrderResolved

		/// <summary>
		/// Gets the resolved value of the <see cref="FieldLayoutSettings.ChildRecordsDisplayOrder"/> for the FieldLayout.
		/// </summary> 
		[ReadOnly(true)]
		[Bindable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ChildRecordsDisplayOrder ChildRecordsDisplayOrderResolved
		{
			get
			{
				if (_cachedChildRecordsDisplayOrderResolved == null)
					_cachedChildRecordsDisplayOrderResolved = this.GetChildRecordsDisplayOrderResolvedInternal();

				return _cachedChildRecordsDisplayOrderResolved.Value;
			}
		}

		private ChildRecordsDisplayOrder GetChildRecordsDisplayOrderResolvedInternal()
		{
			// If the current view does not support setting the child records display order, always use the value of AfterParent.
			if (_presenter != null)
			{
				ViewBase view = _presenter.CurrentViewInternal;
				if (view != null && view.IsChildRecordsDisplayOrderSupported == false)
					return ChildRecordsDisplayOrder.AfterParent;
			}

			ChildRecordsDisplayOrder ret;

			// Use the settings on the FieldLayout
			if (null != _settings)
			{
				ret = _settings.ChildRecordsDisplayOrder;
				if (ChildRecordsDisplayOrder.Default != ret)
					return ret;
			}

			// Use the settings on the DataPresenterBase.
			FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
			if (null != dpSettings)
			{
				ret = dpSettings.ChildRecordsDisplayOrder;
				if (ChildRecordsDisplayOrder.Default != ret)
					return ret;
			}

			// Otherwise, use the default value of AfterParent.
			return ChildRecordsDisplayOrder.AfterParent;
		} 

				#endregion // ChildRecordsDisplayOrderResolved

				#region DataErrorDisplayModeResolved

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
		/// </summary>
		/// <remarks>
		/// <b>DataErrorDisplayModeResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public DataErrorDisplayMode DataErrorDisplayModeResolved
		{
			get
			{
				DataErrorDisplayMode ret;

				if ( null != _settings )
				{
					ret = _settings.DataErrorDisplayMode;
					if ( DataErrorDisplayMode.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.DataErrorDisplayMode;
					if ( DataErrorDisplayMode.Default != ret )
						return ret;
				}

				return DataErrorDisplayMode.ErrorIcon;
			}
		}

				#endregion // DataErrorDisplayModeResolved

                #region DataPresenterBase

        /// <summary>
        /// Returns the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> that owns this <see cref="FieldLayout"/>.
		/// </summary>
        /// <remarks>This property will return null if this FieldLayout is not being used inside a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public DataPresenterBase DataPresenter
		{
			get
			{
				return this._presenter;
			}
		}

				#endregion //DataPresenterBase	

                #region DataRecordSizingModeResolved

        /// <summary>
		/// Determines how <see cref="DataRecord"/>s are sized and if they can resized by the user (read-only).
		/// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
        /// <seealso cref="FieldLayoutSettings.DataRecordSizingModeProperty"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecordSizingMode"/>
        //[Description("Determines how DataRecords are sized and if they can resized by the user (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public DataRecordSizingMode DataRecordSizingModeResolved
        {
            get
            {
                DataRecordSizingMode mode;

                if (this._settings != null )
                {
                    mode = this._settings.DataRecordSizingMode;
                    if (mode != DataRecordSizingMode.Default)
                        return mode;
                }

				if (this._presenter != null)
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						mode = this._presenter.FieldLayoutSettings.DataRecordSizingMode;
						if (mode != DataRecordSizingMode.Default)
							return mode;
					}

					if (this._presenter.CurrentViewInternal != null)
					{
						mode = this._presenter.CurrentViewInternal.DefaultDataRecordSizingMode;
						if (mode != DataRecordSizingMode.Default)
							return mode;
					}
				}

                return DataRecordSizingMode.SizedToContentAndFixed;
            }
        }

                #endregion //DataRecordSizingModeResolved

                #region Description

        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string), typeof(FieldLayout), new FrameworkPropertyMetadata(string.Empty, null, new CoerceValueCallback(CoerceDescription)));

        private static object CoerceDescription(DependencyObject target, object value)
        {
			FieldLayout fl = target as FieldLayout;

			// JM 03-26-10 TFS29944
			if (false == string.IsNullOrEmpty((string)value))
				return value;

			if (fl != null)
				return fl.Description;

            return value;
        }

        /// <summary>
        /// Gets/sets the description for this <see cref="FieldLayout"/>
        /// </summary>
        //[Description("Gets/sets the description for this FieldLayout")]
        //[Category("Behavior")]
		[Bindable(true)]
		public string Description
        {
            get
            {
                if (this._cachedDescription != null && this._cachedDescription.Length > 0)
                    return this._cachedDescription;

                // JJD 4/20/09 - NA 2009 vol 2 - cross band grouping
                // Check the parentField. If it was set then use it to get the default description
                Field parentField = this.ParentField;
                if ( parentField != null )
                {
                    string label = parentField.Label as string;

                    if ( label != null && label.Length > 0 )
                        return label;

                    label = parentField.Name;

                    if ( label != null && label.Length > 0 )
                        return label;
                }

                if ( this._cachedKey != null )
                {

					if (this._propertyDescriptorProviders != null &&
						this._propertyDescriptorProviders.Count > 0)
					{
						LayoutPropertyDescriptorProvider layoutProvider = Utilities.GetWeakReferenceTargetSafe(this._propertyDescriptorProviders[0]) as LayoutPropertyDescriptorProvider;
						if (layoutProvider != null &&
							layoutProvider.Provider != null &&
							layoutProvider.Provider == this._cachedKey)
							return layoutProvider.Provider.ToString();
					}

                    Type type = this._cachedKey as Type;
                    
                    if ( type != null )
                        return type.Name;
            
                    string name = null;

                    // JJD 4/14/09
                    // If the key is an XmlElement then use its Localname
                    XmlElement xmlElement = this._cachedKey as XmlElement;
                    if (xmlElement != null)
                    {
                        name = xmlElement.LocalName;
                    }
                    else
                    {
                        // JJD 4/14/09
                        // For DataTable and DataViews we want to use the table name, if available
                        DataTable dt = this._cachedKey as DataTable;

                        if (dt == null)
                        {
                            DataView dv = this._cachedKey as DataView;

                            if (dv != null)
                                dt = dv.Table;
                        }

                        if (dt != null)
                        {
                            name = dt.TableName;
                        }
                    }

                    if (name != null && name.Length > 0)
                        return name;

                    return this._cachedKey.ToString();
                }

                return string.Empty;
            }
            set
            {
				// JM 09-25-09 TFS22625.
				this._cachedDescription = value;

                this.SetValue(FieldLayout.DescriptionProperty, value);
            }
        }

		/// <summary>
		/// Determines if the <see cref="Description"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDescription()
        {
            return this._cachedDescription != null && this._cachedDescription.Length > 0;
        }

		/// <summary>
		/// Resets the <see cref="Description"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDescription()
		{
			this.ClearValue(DescriptionProperty);
		}

                #endregion //Description

				#region FieldMovingMaxColumnsResolved


		
		
		/// <summary>
		/// Determines the maximum number of logical columns of fields the user is allowed to create
		/// when re-arranging fields.
		/// </summary>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxColumns"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		public int FieldMovingMaxColumnsResolved
		{
			get
			{
				int? val = null != _settings ? _settings.FieldMovingMaxColumns : null;

				if ( ! val.HasValue )
				{
					FieldLayoutSettings settings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
					if ( null != settings )
						val = settings.FieldMovingMaxColumns;

					if ( !val.HasValue )
						val = 0;
				}

				return val.Value;
			}
		}

				#endregion // FieldMovingMaxColumnsResolved

				#region FieldMovingMaxRowsResolved


		
		
		/// <summary>
		/// Determines the maximum number of logical rows of fields the user is allowed to create
		/// when re-arranging fields.
		/// </summary>
		/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.FieldMovingMaxColumns"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		public int FieldMovingMaxRowsResolved
		{
			get
			{
				int? val = null != _settings ? _settings.FieldMovingMaxRows : null;

				if ( ! val.HasValue )
				{
					FieldLayoutSettings settings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
					if ( null != settings )
						val = settings.FieldMovingMaxRows;

					if ( ! val.HasValue )
						val = 0;
				}

				return val.Value;
			}
		}

				#endregion // FieldMovingMaxRowsResolved

				#region Fields

		/// <summary>
		/// Returns the collection of <see cref="Field"/> objects
		/// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
		/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
		/// </remarks>
        /// <seealso cref="Field"/>
        /// <seealso cref="FieldCollection"/>
        /// <seealso cref="SortedFields"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FieldCollection Fields
		{
			get
			{
				
				
				
				
				
				
				
				
				
				

				return this._fields;
			}
		}

		
		
		internal FieldCollection FieldsIfAllocated
		{
			get
			{
				return _fields;
			}
		}

		/// <summary>
		/// Determines if the <see cref="Fields"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFields()
        {
            return this._fields != null && this._fields.ShouldSerialize();
        }

				#endregion //Fields

                #region FieldSettings

        /// <summary>
        /// Identifies the <see cref="FieldSettings"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldSettingsProperty = DataPresenterBase.FieldSettingsProperty;

        private void OnFieldSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			// SSP 4/23/08 - Summaries Functionality
			// Added infrastructure so field, field layout and the data presenter can each process
			// change in field settings of any of these objects.
			// Original code that invalidated generated styles is moved into the Field's
			// new ProcessFieldSettingsPropertyChanged method.
			// ------------------------------------------------------------------------------------
			FieldSettings fieldSettings = this.FieldSettings;

			DataPresenterBase presenter = this.DataPresenter;
			// First process it on each field.
			if ( null != _fields )
			{
				foreach ( Field field in _fields )
					Field.ProcessFieldSettingsPropertyChanged( e, fieldSettings, this, false, false, field, this, presenter );
			}

			Field.ProcessFieldSettingsPropertyChanged( e, fieldSettings, this, null != presenter, true, null, this, presenter );
		
			
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------------

			//this.InvalidateGeneratedStyles(true);
			// AS 5/12/10 Sl-WPF Sharing
			// NestedPropertyChangedEventArgs is obsolete.
			//
			//this.RaisePropertyChangedEvent("FieldSettings", sender, e);
        }

        /// <summary>
        /// Holds the default settings for all fields in the s<see cref="Fields"/> collection.
        /// </summary>
		/// <remarks>
		/// <para class="body"><see cref="FieldSettings"/> are exposed via the following 3 properties:
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
		/// <seealso cref="FieldLayoutSettings"/>
		/// <seealso cref="FieldLayout.FieldSettings"/>
        /// <seealso cref="Fields"/>
        //[Description("Holds the default settings for fields")]
        //[Category("Behavior")]
        public FieldSettings FieldSettings
        {
            get
            {
                if ( this._cachedFieldSettings == null )
                {
                    // JJD 12/21/07
                    // Delay setting this property until InitializeOwner is called so we only
                    // do it at run time. This will allow the VS2008 designer to work properly 
                    if (this._presenter == null ||
                         !DesignerProperties.GetIsInDesignMode(this._presenter))
                    {
                        this._cachedFieldSettings = new FieldSettings();
                        this._cachedFieldSettings.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldSettingsPropertyChanged);
                    }
                }

                return this._cachedFieldSettings;
            }
            set
            {
                this.SetValue(FieldLayout.FieldSettingsProperty, value);
            }
        }

		
		
		internal FieldSettings FieldSettingsIfAllocated
		{
			get
			{
				return _cachedFieldSettings;
			}
		}

		/// <summary>
		/// Determines if the <see cref="FieldSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFieldSettings()
        {
            return this._cachedFieldSettings != null && this._cachedFieldSettings.ShouldSerialize();
        }

		/// <summary>
		/// Resets the <see cref="FieldSettings"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFieldSettings()
		{
			this.ClearValue(FieldSettingsProperty);
		}

                #endregion //FieldSettings

				#region FilterActionResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.FilterAction"/> property.
		/// </summary>
		/// <remarks>
		/// <b>FilterActionResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.FilterAction"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public RecordFilterAction FilterActionResolved
		{
			get
			{
				
				
				
				if ( RecordFilterAction.Default == _cachedRecordFilterActionResolved )
					_cachedRecordFilterActionResolved = this.InternalFilterActionResolved;

				return _cachedRecordFilterActionResolved;
			}
		}

		private RecordFilterAction InternalFilterActionResolved
		{
			get
			{
				RecordFilterAction ret;

				if ( null != _settings )
				{
					ret = _settings.FilterAction;
					if ( RecordFilterAction.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.FilterAction;
					if ( RecordFilterAction.Default != ret )
						return ret;
				}

				return RecordFilterAction.Hide;
			}
		}

				#endregion // FilterActionResolved

				#region FilterClearButtonLocationResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.FilterClearButtonLocation"/> property.
		/// </summary>
		/// <remarks>
		/// <b>FilterClearButtonLocationResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public FilterClearButtonLocation FilterClearButtonLocationResolved
		{
			get
			{
				FilterClearButtonLocation ret;

				if ( null != _settings )
				{
					ret = _settings.FilterClearButtonLocation;
					if ( FilterClearButtonLocation.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.FilterClearButtonLocation;
					if ( FilterClearButtonLocation.Default != ret )
						return ret;
				}

				return FilterClearButtonLocation.RecordSelectorAndFilterCell;
			}
		}
    
				#endregion // FilterClearButtonLocationResolved

				#region FilterRecordLocationResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.FilterRecordLocation"/> property.
		/// </summary>
		/// <remarks>
		/// <b>FilterRecordLocationResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.FilterRecordLocation"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterRecordLocation"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public FilterRecordLocation FilterRecordLocationResolved
		{
			get
			{
				FilterRecordLocation ret;

				if ( null != _settings )
				{
					ret = _settings.FilterRecordLocation;
					if ( FilterRecordLocation.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.FilterRecordLocation;
					if ( FilterRecordLocation.Default != ret )
						return ret;
				}

				return FilterRecordLocation.OnTopFixed;
			}
		}

				#endregion // FilterRecordLocationResolved

				#region FilterUITypeResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.FilterUIType"/> property.
		/// </summary>
		/// <remarks>
		/// <b>FilterUITypeResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.FilterUIType"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public FilterUIType FilterUITypeResolved
		{
			get
			{
                // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
                if (this._presenter != null &&
					// AS 3/21/11 NA 2011.1 Word Writer
					// Putting the original check back in. In theory changing this to use IsFilterRecordSupportedResolved
					// is correct but that results in a change in the ui (in this case during an export). Specifically the 
					// HeaderPlacementInGroupByResolved will default to OnTopOnly when this property returns LabelIcons so 
					// the export results change such that the header is above the group by records instead of being 
					// within.
					//
					//// AS 2/11/11 NA 2011.1 Word Writer
					////this._presenter.IsReportControl)
					//!_presenter.IsFilterRecordSupportedResolved)
					this._presenter.IsReportControl)
                    return FilterUIType.LabelIcons;

				FilterUIType ret;

				if ( null != _settings )
				{
					ret = _settings.FilterUIType;
					if ( FilterUIType.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.FilterUIType;
					if ( FilterUIType.Default != ret )
						return ret;
				}

				return FilterUIType.FilterRecord;
			}
		}

				#endregion // FilterUITypeResolved

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region FixedFieldUITypeResolved

        /// <summary>
        /// Indicates the resolved ui that will be displayed to allow changing the <see cref="Field.FixedLocation"/> of the fields in the layout.
        /// </summary>
        /// <seealso cref="FieldLayoutSettings.FixedFieldUIType"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowFixing"/>
        /// <seealso cref="Field.FixedLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FixedFieldUIType"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public FixedFieldUIType FixedFieldUITypeResolved
        {
            get
            {
                FixedFieldLayoutInfo ffi = this.GetFixedFieldInfo(true);

                return GetFixedFieldUITypeResolved(ffi.HasFixedFields, ffi.HasFixableFields);
            }
        }

                #endregion //FixedFieldUITypeResolved

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordLimitResolved

        /// <summary>
        /// Indicates how many sibling records can be fixed at a time. (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings.FixedRecordLimit"/>
        /// <seealso cref="Record.FixedLocation"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public int FixedRecordLimitResolved
        {
            get
            {
                int? limit = null;
                DataPresenterBase presenter = this._presenter;

                if (null != presenter && presenter.IsFixedRecordsSupportedResolved)
                {
                    if (this._settings != null)
                        limit = this._settings.FixedRecordLimit;

                    if (null == limit && this._presenter.HasFieldLayoutSettings)
                        limit = this._presenter.FieldLayoutSettings.FixedRecordLimit;
                }

                return limit == null
                    ? int.MaxValue
                    : limit.Value;
            }
        }

                #endregion //FixedRecordLimitResolved

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordUITypeResolved

        /// <summary>
        /// Indicates the resolved ui that will be displayed to allow changing the <see cref="Record.FixedLocation"/> of the records in the layout.
        /// </summary>
        /// <seealso cref="FieldLayoutSettings.FixedRecordUIType"/>
        /// <seealso cref="Record.FixedLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FixedRecordUIType"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public FixedRecordUIType FixedRecordUITypeResolved
        {
            get
            {
                FixedRecordUIType uiType = FixedRecordUIType.Default;
                DataPresenterBase presenter = this._presenter;

                if (null != presenter && presenter.IsFixedRecordsSupportedResolved)
                {
                    if (this._settings != null)
                        uiType = this._settings.FixedRecordUIType;

                    if (FixedRecordUIType.Default == uiType && this._presenter.HasFieldLayoutSettings)
                        uiType = this._presenter.FieldLayoutSettings.FixedRecordUIType;
                }

                return uiType == FixedRecordUIType.Default
                    ? FixedRecordUIType.Button
                    : uiType;
            }
        }

                #endregion //FixedRecordUITypeResolved

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedRecordSortOrderResolved

        /// <summary>
        /// Indicates the order of fixed records relative to each other (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings.FixedRecordSortOrder"/>
        /// <seealso cref="Record.FixedLocation"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FixedRecordSortOrder"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public FixedRecordSortOrder FixedRecordSortOrderResolved
        {
            get
            {
                FixedRecordSortOrder sortOrder = FixedRecordSortOrder.Default;
                DataPresenterBase presenter = this._presenter;

                if (null != presenter && presenter.IsFixedRecordsSupportedResolved)
                {
                    if (this._settings != null)
                        sortOrder = this._settings.FixedRecordSortOrder;

                    if (FixedRecordSortOrder.Default == sortOrder && this._presenter.HasFieldLayoutSettings)
                        sortOrder = this._presenter.FieldLayoutSettings.FixedRecordSortOrder;
                }

                return sortOrder == FixedRecordSortOrder.Default
                    ? FixedRecordSortOrder.Sorted
                    : sortOrder;
            }
        }

                #endregion //FixedRecordSortOrderResolved

                // JJD 05/04/10 - TFS31349 - added
                #region GroupByExpansionIndicatorVisibilityResolved

        /// <summary>
        /// Determines if expansion indicators will be displayed in groupby records.
        /// </summary>
        /// <seealso cref="FieldLayoutSettings.GroupByExpansionIndicatorVisibility"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
        public Visibility GroupByExpansionIndicatorVisibilityResolved
        {
            get
            {
                Visibility? vis = null;
                
                if (this._settings != null)
                    vis = this._settings.GroupByExpansionIndicatorVisibility;

                if (vis.HasValue == false && this._presenter != null)
                {
                    if (this._presenter.HasFieldLayoutSettings)
                        vis = this._presenter.FieldLayoutSettings.GroupByExpansionIndicatorVisibility;
                }

                return vis.HasValue
                    ? vis.Value
                    : Visibility.Visible;
            }
        }

                #endregion //GroupByExpansionIndicatorVisibilityResolved

                #region HasFieldSettings

        /// <summary>
        /// Returns true if a <see cref="Infragistics.Windows.DataPresenter.FieldSettings"/> object has been allocated (read-only).
        /// </summary>
        /// <seealso cref="Field"/>
        /// <seealso cref="Fields"/>
        /// <seealso cref="FieldSettings"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public bool HasFieldSettings
        {
            get { return this._cachedFieldSettings != null; }
        }

                #endregion //HasFieldSettings

                #region HasSeparateHeader

        private static readonly DependencyPropertyKey HasSeparateHeaderPropertyKey =
            DependencyProperty.RegisterReadOnly("HasSeparateHeader",
            typeof(bool), typeof(FieldLayout), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the 'HasSeparateHeader' dependency property
        /// </summary>
        public static readonly DependencyProperty HasSeparateHeaderProperty =
            HasSeparateHeaderPropertyKey.DependencyProperty;

		// JJD 4/26/07
		// Optimization - used the locally cached property 
		private bool _cachedHasSeparateHeader;

        /// <summary>
		/// Returns true if a separate header style has been generated for use in a GridViewPanel (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
		public bool HasSeparateHeader
        {
            get
            {
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (bool)this.GetValue(FieldLayout.HasSeparateHeaderProperty);
				return this._cachedHasSeparateHeader;
            }
        }

                #endregion //HasSeparateHeader

                #region HasSettings

        /// <summary>
        /// Returns true if a <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings"/> object has been allocated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public bool HasSettings
        {
            get { return this._settings != null; }
        }

                #endregion //HasSettings	

                #region HasGroupBySortedFields

        /// <summary>
        /// Returns true if there are any <see cref="FieldSortDescription"/>s in the <see cref="SortedFields"/> collection whose <see cref="FieldSortDescription.IsGroupBy"/> property is true (read-only).
        /// </summary>
        /// <seealso cref="Fields"/>
        /// <seealso cref="SortedFields"/>
        /// <seealso cref="FieldSortDescription"/>
        /// <seealso cref="FieldSortDescriptionCollection"/>
        /// <seealso cref="FieldSortDescription.IsGroupBy"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public bool HasGroupBySortFields
        {
            get { return this.HasSortedFields && this.SortedFields.CountOfGroupByFields > 0; }
        }

                #endregion //HasGroupBySortedFields

                #region HasSortedFields

        /// <summary>
        /// Returns true if there are any <see cref="FieldSortDescription"/>s in the <see cref="SortedFields"/> collection (read-only).
        /// </summary>
        /// <seealso cref="Fields"/>
        /// <seealso cref="SortedFields"/>
        /// <seealso cref="FieldSortDescription"/>
        /// <seealso cref="FieldSortDescriptionCollection"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		public bool HasSortedFields
        {
            get { return this._sortedFields != null && this._sortedFields.Count > 0; }
        }

                #endregion //HasSortedFields

                #region HeaderAreaTemplate

        /// <summary>
        /// Returns the generated header template (read-only) 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Bindable(true)]
		public DataTemplate HeaderAreaTemplate
        {
            get
            {
                if (this._styleGenerator != null)
                    return this._styleGenerator.GeneratedHeaderTemplate;

                return null;
            }
        }

                #endregion //HeaderAreaTemplate

                #region HeaderPlacementResolved

		/// <summary>
        /// Determines the placement of headers (read-only).
		/// </summary>
        /// <remarks>
        /// <para class="note">
        /// <b>Note:</b> This setting is ignored unless <see cref="LabelLocationResolved"/> is 'SeparateHeader' and the view supports separate headers.
        /// </para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.HeaderPlacement"/>
        /// <seealso cref="FieldLayoutSettings.HeaderPlacementProperty"/>
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public HeaderPlacement HeaderPlacementResolved
        {
            get
            {
                HeaderPlacement setting = this.HeaderPlacementExplicitResolved;
                
                if ( setting != HeaderPlacement.Default)
                     return setting;

                return HeaderPlacement.OnTopOnly;
            }
        }

        private HeaderPlacement HeaderPlacementExplicitResolved
        {
            get
            {
                HeaderPlacement setting;

                if (this._settings != null )
                {
                    setting = this._settings.HeaderPlacement;
                    if ( setting != HeaderPlacement.Default)
                        return setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.HeaderPlacement;
                    
                    if ( setting != HeaderPlacement.Default)
                         return setting;
                }

                return HeaderPlacement.Default;
            }
        }

                #endregion //HeaderPlacementResolved

                #region HeaderPlacementInGroupByResolved

		/// <summary>
        /// Determines the placement of headers when records are grouped (read-only).
		/// </summary>
        /// <value>
        /// <para class="body">If <see cref="FieldLayoutSettings.HeaderPlacementInGroupBy"/> is not explicitly set then this will return to 'WithDataRecords' unless <see cref="FieldLayoutSettings.HeaderPlacement"/> is explicitly set to 'OnTopOnly' or <see cref="FieldLayout.FilterUITypeResolved"/> returns 'LabelIcons'.</para>
        /// </value>
        /// <remarks>
        /// <para class="note">
        /// <b>Note:</b> This setting is ignored unless <see cref="LabelLocationResolved"/> is 'SeparateHeader' and the view supports separate headers.
        /// </para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.HeaderPlacementInGroupBy"/>
        /// <seealso cref="FieldLayoutSettings.HeaderPlacementInGroupByProperty"/>
		[ReadOnly(true)]
		[Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        public HeaderPlacementInGroupBy HeaderPlacementInGroupByResolved
        {
            get
            {
                // JJD 6/4/09
                // We can't support headers on groupby records in horizontal mode
                if (this.IsHorizontal)
                    return HeaderPlacementInGroupBy.WithDataRecords;

                HeaderPlacementInGroupBy setting;

                if (this._settings != null)
                {
                    setting = this._settings.HeaderPlacementInGroupBy;
                    if (setting != HeaderPlacementInGroupBy.Default)
                        return setting;
                }

                if (this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings)
                {
                    setting = this._presenter.FieldLayoutSettings.HeaderPlacementInGroupBy;

                    if (setting != HeaderPlacementInGroupBy.Default)
                        return setting;
                }

                HeaderPlacement headerPlacement = this.HeaderPlacementExplicitResolved;
                
                // If headerPlacement was explicitly set to 'OnTopOnly' then
                // return the corresponding 'OnTopOnly'
                if ( headerPlacement == HeaderPlacement.OnTopOnly)
                     return HeaderPlacementInGroupBy.OnTopOnly;

                // JJD 1/29/09 
                // If the filter ui type is LabelIcons
                if (this.FilterUITypeResolved == FilterUIType.LabelIcons)
                    return HeaderPlacementInGroupBy.OnTopOnly;

                // return 'WithDataRecords' to maintain existing behavior
                return HeaderPlacementInGroupBy.WithDataRecords;
            }
        }

                #endregion //HeaderPlacementInGroupByResolved

				#region HeaderPrefixAreaDisplayModeResolved

		
		

		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/> property.
		/// </summary>
		/// <remarks>
		/// <b>HeaderPrefixAreaDisplayModeResolved</b> returns the resolved value of the 
		/// <see cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> 
		/// or the DataPresenter's <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/>
		/// <seealso cref="DataPresenterBase.ShowFieldChooser(FieldLayout,bool,bool,string)"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public HeaderPrefixAreaDisplayMode HeaderPrefixAreaDisplayModeResolved
		{
			get
			{
				return this.GetResolvedValue<HeaderPrefixAreaDisplayMode>(
					FieldLayoutSettings.HeaderPrefixAreaDisplayModeProperty,
					HeaderPrefixAreaDisplayMode.Default,
					HeaderPrefixAreaDisplayMode.None );
			}
		}

				#endregion // HeaderPrefixAreaDisplayModeResolved

                #region HighlightAlternateRecordsResolved

        /// <summary>
        /// Determines whether the <see cref="RecordPresenter"/> and <see cref="DataItemPresenter"/> <see cref="RecordPresenter.IsAlternate"/> properties will return true on every other <see cref="Record"/> (read-only). 
        /// </summary>
        /// <seealso cref="RecordPresenter.IsAlternate"/>
        /// <seealso cref="DataItemPresenter.IsAlternate"/>
        /// <seealso cref="FieldLayout"/>
        /// <seealso cref="FieldLayoutSettings.HighlightAlternateRecords"/>
        /// <remarks>This used from inside cell, label, field and record styles.</remarks>
        //[Description("Determines whether the IsAlternate property will return true on every other record.")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public bool HighlightAlternateRecordsResolved
        {
            get
            {
                Nullable<bool> setting;

                if (this._settings != null )
                {
                    setting = this._settings.HighlightAlternateRecords;
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    setting = this._presenter.FieldLayoutSettings.HighlightAlternateRecords;
                    
                    if ( setting.HasValue)
                        return (bool)setting;
                }

                return false;
            }
        }

                #endregion //HighlightAlternateRecordsResolved

                #region HighlightPrimaryFieldResolved

        /// <summary>
        /// Determines whether the primary field will be highlighted (read-only).
        /// </summary>
        /// <seealso cref="DataItemPresenter.HighlightAsPrimary"/>
        /// <seealso cref="Field.IsPrimary"/>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="PrimaryField"/>
        /// <seealso cref="FieldLayoutSettings.HighlightPrimaryField"/>
        /// <remarks>This is used from inside cell, label, field and record styles.</remarks>
        //[Description("Determines whether the primary field will be highlighted (read-only).")]
        //[Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public bool HighlightPrimaryFieldResolved
        {
            get
            {
                if (this._presenter != null)
                    return this._presenter.GetHighlightPrimaryFieldResolved(this);

                return false;
            }
        }

                #endregion //HighlightPrimaryFieldResolved

                #region IsDefault

        /// <summary>
		/// Gets/sets whether this is the default FieldLayout
		/// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		//[ReadOnly(true)]	JM 08-26-09 TFS 21449
		public bool IsDefault
		{
			get
			{
				// If our owner is set rely on it to keep track of the current default
				if ( this._presenter != null )
					return this == this._presenter.DefaultFieldLayout;
			
				// return the cached setting
				return this._isdefault;
			}
			set
			{
				// cache the setting in case we aren't hooked up to the owner yet
				this._isdefault = value;

				if ( this._presenter != null )
				{
					if ( value == true )
						this._presenter.DefaultFieldLayout = this;
					else 
					if ( this._presenter.DefaultFieldLayout == this)
						this._presenter.DefaultFieldLayout = null;
				}
			}
		}

				#endregion //IsDefault	
		
				#region IsInitialized

		/// <summary>
		/// Gets whether this FieldLayout has been initialized (read-only)
		/// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool IsInitialized { get { return this._isInitialized; } }

				#endregion //IsInitialized

				#region Key

	    /// <summary>
		/// Identifies the <see cref="Key"/> dependency property
		/// </summary>
		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key",
				  typeof(object), typeof(FieldLayout), new FrameworkPropertyMetadata());

		/// <summary>
		/// The key into the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/> collection
		/// </summary>
		/// <remarks>
        /// <para class="body">If a key isn't explicitly set then the <see cref="PropertyDescriptorProvider.Key"/> value will be used as the default key.
        /// </para>
        /// <para class="note"><b>Note:</b> in version 9.2 (NA 2009 volume 2) we changed the default value of the Key, initialized from the <see cref="PropertyDescriptorProvider"/>. This was done to make the keys more xaml friendly. The following table describes how the changes affect various data sources:
        /// <list type="bullet">
        /// <item>
        ///		<term><b>DataTable</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the <see cref="System.Data.DataTable"/>  instance. As of v9.2 the Key is now the DataTable.TableName."
        ///		</description>
        /// </item>
        /// <item>
        ///		<term><b>ITypedList (e.g. DataView)</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the data source instance. As of v9.2 the Key is now what is returned from the <see cref="System.ComponentModel.ITypedList"/>.GetListName() method."
        ///		</description>
        /// </item>
        /// <item>
        ///		<term><b>XmlNode</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the <see cref="System.Xml.XmlNode"/> instance. As of v9.2 the Key is now the XmlNode.Name."
        ///		</description>
        /// </item>
        /// </list>
        /// <para class="body">The <see cref="KeyMatchingEnforced"/> property was added to allow the Key to be used to explicitly idenify which data items are compatible when assigning a FieldLayout to a <see cref="DataRecord"/>.</para>
        /// </para>
        /// </remarks>
        /// <seealso cref="KeyMatchingEnforced"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/>
        /// <seealso cref="FieldLayoutCollection"/>
		/// <seealso cref="PropertyDescriptorProvider"/>
		//[Description("The key into the Fieldlayouts collection")]
		//[Category("Behavior")]
		public object Key
		{
			get
			{
				return this._cachedKey;
			}
			set
			{
				this.SetValue(FieldLayout.KeyProperty, value);
			}
		}

        internal void InitializeKey(object value)
        {
            this.Key = value;
            this._keyExplicitlySet = false;
        }

		/// <summary>
		/// Determines if the <see cref="Key"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeKey()
        {
            return this._keyExplicitlySet && this._cachedKey != null;
        }

				#endregion //Key

                // JJD 8/11/09 - NA 2009 vol 2 
                #region KeyMatchingEnforced

        /// <summary>
        /// Identifies the <see cref="KeyMatchingEnforced"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty KeyMatchingEnforcedProperty = DependencyProperty.Register("KeyMatchingEnforced",
            typeof(bool), typeof(FieldLayout), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnKeyMatchingEnforcedChanged)));


        private static void OnKeyMatchingEnforcedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FieldLayout fl = target as FieldLayout;

            if (fl != null)
            {
                fl._keyMatchingEnforced = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// Gets/sets whether the Key property must match the data item provider's key when assiging the FieldLayout to the associated DataRecord. 
        /// </summary>
        /// <remarks>
        /// <para class="body">When assigning a FieldLayout to a <see cref="DataRecord"/> the <see cref="DataPresenterBase.FieldLayouts"/> collection is searched for the most
        /// approprate match to the data item. Setting this property to true will exclude a FieldLayout from matching the data item unless its key is a match for the 
        /// <see cref="PropertyDescriptorProvider"/>'s <see cref="PropertyDescriptorProvider.Key"/>. 
        /// However, even if KeyMatchingEnforced is true and the Key doeesn't match this does not prevent the FieldLayout from being explicitly assigned to a data item in code within the <see cref="DataPresenterBase.AssigningFieldLayoutToItem"/> event.
        /// </para>
        /// <para class="note"><b>Note:</b> in version 9.2 (NA 2009 volume 2) we changed the default value of the Key. Unless explicitly set it is now initialized from the <see cref="PropertyDescriptorProvider"/>'s <see cref="PropertyDescriptorProvider"/>.Key. 
        /// This was done to make the keys more xaml friendly. The following table describes how the changes affect various data sources:
        /// <list type="bullet">
        /// <item>
        ///		<term><b>DataTable</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the <see cref="System.Data.DataTable"/>  instance. As of v9.2 the Key is now the DataTable.TableName."
        ///		</description>
        /// </item>
        /// <item>
        ///		<term><b>ITypedList (e.g. DataView)</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the data source instance. As of v9.2 the Key is now what is returned from the <see cref="System.ComponentModel.ITypedList"/>.GetListName() method."
        ///		</description>
        /// </item>
        /// <item>
        ///		<term><b>XmlNode</b></term>
        ///		<description>
        ///			"Prior to v.9.2 the key was the <see cref="System.Xml.XmlNode"/> instance. As of v9.2 the Key is now the XmlNode.Name."
        ///		</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="KeyMatchingEnforcedProperty"/>
        /// <seealso cref="Key"/>
        //[Description("Gets/sets whether the Key property must match the data item provider's key when assiging the FieldLayout to the associated DataRecord")]
        //[Category("Data")]
        [Bindable(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public bool KeyMatchingEnforced
        {
            get
            {
                return this._keyMatchingEnforced;
            }
            set
            {
                this.SetValue(FieldLayout.KeyMatchingEnforcedProperty, value);
            }
        }

                #endregion //KeyMatchingEnforced
    
                #region LabelLocationResolved

        /// <summary>
        /// Returns the preferred location of the labels (read-only)
        /// </summary>
        /// <remarks>Not all panels support a separate header area for the labels. These panels will revert to having the labels with the fields.</remarks>
        /// <seealso cref="FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.LabelLocation"/>
        /// <seealso cref="HasSeparateHeader"/>
        /// <seealso cref="FieldLayoutSettings.LabelLocation"/>
        /// <seealso cref="FieldLayoutSettings.LabelLocationProperty"/>
        //[Description("Returns the preferred location of the labels (read-only)")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public LabelLocation LabelLocationResolved
        {
            get
            {
                LabelLocation location;

                if (this._settings != null )
                {
                    location = this._settings.LabelLocation;
                    if (location != LabelLocation.Default)
                        return location;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    location = this._presenter.FieldLayoutSettings.LabelLocation;
                    if (location != LabelLocation.Default)
                        return location;

					ViewBase view = this._presenter.CurrentViewInternal;

					if (view != null)
					{
						location = view.DefaultLabelLocation;
						if (location != LabelLocation.Default)
							return location;
					}
				}

                FieldLayoutTemplateGenerator generator = this.StyleGenerator;

                bool separateHeader = generator != null && generator.SupportsLabelHeaders;

                if ( separateHeader )
                    return LabelLocation.SeparateHeader;
                else
                    return LabelLocation.InCells;
            }
        }

                #endregion //LabelLocationResolved

				#region MaxFieldsToAutoGenerateResolved

		/// <summary>
		/// Returns the maximum number of cells that can be selected at any time (read-only).
		/// </summary>
		/// <seealso cref="FieldLayoutSettings"/>
		/// <seealso cref="FieldLayoutSettings.MaxSelectedCells"/>
		/// <seealso cref="FieldLayoutSettings.MaxSelectedCellsProperty"/>
		/// <seealso cref="MaxSelectedRecordsResolved"/>
		//[Description("Returns the maximum number of cells that can be selected at any time (read-only).")]
		//[Category("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public int MaxFieldsToAutoGenerateResolved
		{
			get
			{
				int max;

				if (this._settings != null)
				{
					max = this._settings.MaxFieldsToAutoGenerate;
					if (max > 0)
						return max;
				}

				if (this._presenter != null )
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						max = this._presenter.FieldLayoutSettings.MaxFieldsToAutoGenerate;
						if (max > 0)
							return max;
					}
				}

				return int.MaxValue;
			}
		}

				#endregion //MaxFieldsToAutoGenerateResolved

                #region MaxSelectedCellsResolved

		/// <summary>
		/// Returns the maximum number of cells that can be selected at any time (read-only).
		/// </summary>
		/// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.MaxSelectedCells"/>
        /// <seealso cref="FieldLayoutSettings.MaxSelectedCellsProperty"/>
        /// <seealso cref="MaxSelectedRecordsResolved"/>
		//[Description("Returns the maximum number of cells that can be selected at any time (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public int MaxSelectedCellsResolved
        {
            get
            {
                Nullable<int> max;

                if (this._settings != null )
                {
                    max = this._settings.MaxSelectedCells;
                    if (max.HasValue )
                        return max.Value;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    max = this._presenter.FieldLayoutSettings.MaxSelectedCells;
					if (max.HasValue)
						return max.Value;
				}

                return int.MaxValue;
            }
        }

                #endregion //MaxSelectedCellsResolved

                #region MaxSelectedRecordsResolved

		/// <summary>
		/// Returns the maximum number of records that can be selected at any time (read-only).
		/// </summary>
		/// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.MaxSelectedRecords"/>
        /// <seealso cref="FieldLayoutSettings.MaxSelectedRecordsProperty"/>
        /// <seealso cref="MaxSelectedCellsResolved"/>
		//[Description("Returns the maximum number of records that can be selected at any time (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public int MaxSelectedRecordsResolved
        {
            get
            {
                Nullable<int> max;

                if (this._settings != null )
                {
                    max = this._settings.MaxSelectedRecords;
                    if (max.HasValue )
                        return max.Value;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    max = this._presenter.FieldLayoutSettings.MaxSelectedRecords;
					if (max.HasValue)
						return max.Value;
				}

                return int.MaxValue;
            }
        }

                #endregion //MaxSelectedRecordsResolved

                // JJD 4/16/09 - NA 2009 vol 2 
                #region ParentField

        /// <summary>
        /// Returns the parent Field or null.
        /// </summary>
        /// <seealso cref="ParentFieldName"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public Field ParentField
        {
            get 
            { 
                if ( this._parentField != null )
                    return this._parentField; 

                FieldLayout parentLayout = this.ParentFieldLayout;

                if ( parentLayout == null )
                    return null;

                string fieldName = this.ParentFieldName;

                if (string.IsNullOrEmpty(fieldName))
                    return null;

                int index = parentLayout.Fields.IndexOf(fieldName);

                if ( index >= 0 )
                    this._parentField = parentLayout.Fields[index];

                if (this._parentField != null)
                    this.RaisePropertyChangedEvent("ParentField");

                return this._parentField; 
            }
        }

                #endregion //ParentField

                // JJD 4/07/09 - NA 2009 vol 2 
                #region ParentFieldLayout

        /// <summary>
        /// Returns the parent FieldLayout or null.
        /// </summary>
        /// <seealso cref="ParentFieldLayoutKey"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public FieldLayout ParentFieldLayout
        {
            get 
            { 
                if ( this._parentFieldLayout != null)
                    return this._parentFieldLayout;

                if (this._parentField != null)
                    return this._parentField.Owner;

                object parentLayoutKey = this.ParentFieldLayoutKey;

                if (parentLayoutKey == null)
                    return null;

                int index = this._presenter != null ? this._presenter.FieldLayouts.IndexOfKey(parentLayoutKey) : -1;

                if ( index >= 0 && index != this.Index)
                    this._parentFieldLayout = this._presenter.FieldLayouts[index];

                if (this._parentFieldLayout != null)
                    this.RaisePropertyChangedEvent("ParentFieldLayout");

                return this._parentFieldLayout; 
            }
        }

                #endregion //ParentFieldLayout

                // JJD 4/07/09 - NA 2009 vol 2 
                #region ParentFieldLayoutKey

        /// <summary>
        /// Identifies the <see cref="ParentFieldLayoutKey"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty ParentFieldLayoutKeyProperty = DependencyProperty.Register("ParentFieldLayoutKey",
            typeof(object), typeof(FieldLayout), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnParentFieldLayoutKeyChanged)));

        private static void OnParentFieldLayoutKeyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FieldLayout fl = target as FieldLayout;

            if (fl != null)
            {
                FieldLayout oldParentLayout = fl._parentFieldLayout;

                // clear the cached member 
                fl._parentFieldLayout = null;

                // access the parent field layout property to trigger a possble propertychange notification
                FieldLayout parent = fl.ParentFieldLayout;

                // when the property is cleared we need to raise the event here
                if (oldParentLayout != null &&
                    parent == null)
                    fl.RaisePropertyChangedEvent("ParentFieldLayout");

				// SSP 3/23/12 TFS98845
				// Notify the calc manager of the change in the field-layout hierarchy.
				// 
				if ( oldParentLayout != parent )
					GridUtilities.NotifyCalcAdapter( fl._presenter, fl, "ParentFieldLayout", null );
            }
        }

        /// <summary>
        /// Gets/sets the key of the parent FieldLayout
        /// </summary>
        /// <seealso cref="ParentFieldLayoutKeyProperty"/>
        /// <seealso cref="Key"/>
        /// <seealso cref="ParentFieldLayout"/>
        //[Description("Gets/sets the key of the parent FieldLayout")]
        //[Category("Data")]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public object ParentFieldLayoutKey
        {
            get
            {
                return (object)this.GetValue(FieldLayout.ParentFieldLayoutKeyProperty);
            }
            set
            {
                this.SetValue(FieldLayout.ParentFieldLayoutKeyProperty, value);
            }
        }

                #endregion //ParentFieldLayoutKey

                // JJD 4/16/09 - NA 2009 vol 2 
                #region ParentFieldName

        /// <summary>
        /// Identifies the <see cref="ParentFieldName"/> dependency property
        /// </summary>
        /// <seealso cref="ParentFieldName"/>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty ParentFieldNameProperty = DependencyProperty.Register("ParentFieldName",
            typeof(string), typeof(FieldLayout), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnParentFieldNameChanged)));

        private static void OnParentFieldNameChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FieldLayout fl = target as FieldLayout;

            if (fl != null)
            {
                Field oldParentField = fl._parentField;

                // clear the cached member
                fl._parentField = null;

                // access the parent field layout property to trigger a possble propertychange notification
                Field parentField = fl.ParentField;

                // when the property is cleared we need to raise the event here
                if (oldParentField != null &&
                    parentField == null)
                    fl.RaisePropertyChangedEvent("ParentFieldName");
            }
        }

        /// <summary>
        /// Gets/sets the name of the parent Field
        /// </summary>
        /// <seealso cref="ParentFieldNameProperty"/>
        /// <seealso cref="Field.Name"/>
        /// <seealso cref="ParentField"/>
        /// <seealso cref="ParentFieldLayout"/>
        /// <seealso cref="ParentFieldLayoutKey"/>
        //[Description("Gets/sets the name of the parent Field")]
        //[Category("Data")]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public string ParentFieldName
        {
            get
            {
                return (string)this.GetValue(FieldLayout.ParentFieldNameProperty);
            }
            set
            {
                this.SetValue(FieldLayout.ParentFieldNameProperty, value);
            }
        }

                #endregion //ParentFieldName
		
				#region PrimaryField

		/// <summary>
		/// Returns the primary <see cref="Field"/>
		/// </summary>
        /// <seealso cref="Field"/>
        /// <seealso cref="DataItemPresenter.HighlightAsPrimary"/>
        /// <seealso cref="Field.IsPrimary"/>
        /// <seealso cref="HighlightPrimaryField"/>
        /// <seealso cref="HighlightPrimaryFieldResolved"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public Field PrimaryField
		{
			get
			{
				if (this._fields != null)
				{
					// if the fields have changed...
					if (this._fields.Version != this._primaryFieldVersion)
					{
						// sync the version
						this._primaryFieldVersion = this._fields.Version;

						// if the primary field has been remove, clear the setting
						if (this._primaryField != null &&
							this._fields.Contains(this._primaryField) == false)
						{
							this.PrimaryField = null;
						}
					}

					if (this._primaryField != null)
						return this._primaryField;

                    // SSP 5/7/10 TFS29073
                    // Initialize the _primaryField to the first visible field. Afterwards we should
                    // always be using that as the primary field. If the user hides the field via 
                    // field chooser, we shouldn't pick the next one to be primary. The cause of the
                    // bug was that we weren't sending out notifications when we hide the primary
                    // field because here we simply return the first visible field however we don't
                    // raise the notification.
                    // 
					//return this._fields.GetFirstFieldVisibleInCellArea();
                    return _primaryField = this._fields.GetFirstFieldVisibleInCellArea( );
				}

				return null;
			}
			internal set
			{
				





				// MD 6/4/10 - TFS33586
				// We shouldn't be doing anything when the property is being set to the same value is already has.
				// Otherwise, we will end up setting IsPrimary to False on the value that is being set on this property.
				if (value == this._primaryField)
					return;

				Field oldPrimaryField = this._primaryField;

				this._primaryField = value;


				// JM NA 10.1 CardView
				this.RaisePropertyChangedEvent("PrimaryField");


				// clear the IsPrimary flag on the old field
				if (null != oldPrimaryField)
					// JM 04-20-11 TFS70852 To avoid recursion. don't go through the property accessor. 
					//oldPrimaryField.IsPrimary = false;
					oldPrimaryField.SetIsPrimaryInternal(false);
			}
		}

				#endregion //PrimaryField

				#region RecordFilters

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Specifies the filter criteria with which to filter records.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecordFilters</b> property gets the record filters collection for this field layout. You can use this 
		/// collection to specify filter criteria to filter data records. These filters will be applied to all data 
		/// records associated with this 
		/// field layout. Note that for the child field layouts, RecordManager's <see cref="RecordManager.RecordFilters"/> 
		/// will be used by default unless you set the <see cref="FieldLayoutSettings.RecordFilterScope"/> 
		/// property to <b>AllRecords</b>. See <see cref="FieldLayoutSettings.RecordFilterScope"/> for more information.
		/// </para>
		/// <para class="body">
		/// Also note that you can enable record filtering user interface by setting 
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// and <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.FilterUIType"/> properties.
		/// When the user modifies filter criteria, this collection will be updated to reflect the new criteria.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.RecordFilterScope"/>
		/// <seealso cref="RecordManager.RecordFilters"/>
		//[Description( "Specifies the filter criteria with which to filter records." )]
		//[Category( "Data" )]
		[Bindable( true )]



		// AS 3/18/11 TFS35776
		// The RecordFilters are not serialized without this attribute.
		//
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] 
		public RecordFilterCollection RecordFilters

		{
			get
			{
				if ( null == _recordFilters )
					_recordFilters = new RecordFilterCollection( this );

				return _recordFilters;
			}
		}

		internal RecordFilterCollection RecordFiltersIfAllocated
		{
			get
			{
				return _recordFilters;
			}
		}

		internal bool HasRecordFilters
		{
			get
			{
				return null != _recordFilters && _recordFilters.Count > 0;
			}
		}

		/// <summary>
		/// Returns true if the RecordFilters property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeRecordFilters( )
		{
			return null != _recordFilters && _recordFilters.ShouldSerialize( );
		}

		/// <summary>
		/// Resets the RecordFilters property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetRecordFilters( )
		{
			if ( null != _recordFilters )
				_recordFilters.Clear( );
		}

				#endregion // RecordFilters

				#region RecordFiltersLogicalOperatorResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.RecordFiltersLogicalOperator"/> property.
		/// </summary>
		/// <remarks>
		/// <b>RecordFiltersLogicalOperatorResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.RecordFiltersLogicalOperator"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.RecordFiltersLogicalOperator"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public LogicalOperator RecordFiltersLogicalOperatorResolved
		{
			get
			{
				
				
				
				if ( _cachedRecordFiltersLogicalOperatorResolved.HasValue )
					return _cachedRecordFiltersLogicalOperatorResolved.Value;

				_cachedRecordFiltersLogicalOperatorResolved = this.InternalRecordFiltersLogicalOperatorResolved;
				return _cachedRecordFiltersLogicalOperatorResolved.Value;
			}
		}

		private LogicalOperator InternalRecordFiltersLogicalOperatorResolved
		{
			get
			{
				LogicalOperator? ret;

				if ( null != _settings )
				{
					ret = _settings.RecordFiltersLogicalOperator;
					if ( ret.HasValue )
						return ret.Value;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.RecordFiltersLogicalOperator;
					if ( ret.HasValue )
						return ret.Value;
				}

				return LogicalOperator.And;
			}
		}

				#endregion // RecordFiltersLogicalOperatorResolved

                // JJD 1/19/09 - NA 2009 vol 1 - record filtering
                #region RecordIndentVersion

        /// <summary>
        /// This is used internally to detect (via binding) when the record indents have changed 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable( EditorBrowsableState.Never)]
        [Browsable(false)]
        [ReadOnly(true)]
        [Bindable(true)]
        public int RecordIndentVersion
        {
            get
            {
                return this._indentOffsetVersion;
            }
        }

                #endregion //GroupByVersion

                #region RecordSelectorExtentResolved

		/// <summary>
		/// Gets the extent for the <see cref="RecordSelector"/> (read-only)
		/// </summary>
		/// <remarks>
		/// Based on the <see cref="RecordSelectorLocation"/> this will represent its width or its height
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.RecordSelectorExtent"/>
        /// <seealso cref="FieldLayoutSettings.RecordSelectorExtentProperty"/>
		/// <seealso cref="FieldLayoutSettings.RecordSelectorStyle"/>
		/// <seealso cref="FieldLayoutSettings.RecordSelectorStyleSelector"/>
 		//[Description("Gets/sets the extent for the record selector (read-only)")]
		//[Category("Appearnce")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public double RecordSelectorExtentResolved
        {
            get
            {
                double extent;

                if (this._settings != null )
                {
                    extent = this._settings.RecordSelectorExtent;
                    if (!double.IsNaN(extent))
                        return extent;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    extent = this._presenter.FieldLayoutSettings.RecordSelectorExtent;
					if (!double.IsNaN(extent))
						return extent;
				}

				// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
				// 
				// ------------------------------------------------------------------------------
				//return 20;

				// JJD 3/23/12 - Metro theme support
				// Added properties to supply default RecordSelectorExtents from within a theme's style
				//double ret = DefaultRecordSelectorBaseExtent;
				double ret = this._presenter != null ? _presenter.RecordSelectorExtent : double.NaN;
				if ( double.IsNaN(ret))
					ret = DefaultRecordSelectorBaseExtent;

				SupportDataErrorInfo supportDataErrorInfo = this.SupportDataErrorInfoResolved;
				if (SupportDataErrorInfo.RecordsOnly == supportDataErrorInfo
					|| SupportDataErrorInfo.RecordsAndCells == supportDataErrorInfo)
				{
					// JJD 3/23/12 - Metro theme support
					// Added properties to supply default RecordSelectorExtents from within a theme's style
					//ret += DefaultRecordSelectorErrorIconExtent;
					double defErrorIconExtent = this._presenter != null ? _presenter.RecordSelectorErrorIconExtent : double.NaN;
					if (double.IsNaN(defErrorIconExtent))
						ret += DefaultRecordSelectorErrorIconExtent;
					else
						ret += defErrorIconExtent;
				}

                // JJD 6/10/09 - NA 2009 Vol 2 - RecordFixing
				if (this._presenter != null &&
					this._presenter.IsReportControl == false &&
					this.AllowRecordFixingResolved != AllowRecordFixing.No &&
					this.FixedRecordUITypeResolved == FixedRecordUIType.Button)
				{
					// JJD 3/23/12 - Metro theme support
					// Added properties to supply default RecordSelectorExtents from within a theme's style
					//ret += DefaultRecordSelectorFixButtonExtent;
					double defFixButtonExtent = this._presenter != null ? _presenter.RecordSelectorFixButtonExtent : double.NaN;
					if (double.IsNaN(defFixButtonExtent))
						ret += DefaultRecordSelectorFixButtonExtent;
					else
						ret += defFixButtonExtent;
				}

                return ret;
				// ------------------------------------------------------------------------------
            }
        }

                #endregion //RecordSelectorExtentResolved

                #region RecordSelectorLocationResolved

		/// <summary>
		/// Determines if and where <see cref="RecordSelector"/>s will be displayed relative to a <see cref="Record"/>'s cell area (read-only).
		/// </summary>
		/// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.RecordSelectorLocation"/>
        /// <seealso cref="FieldLayoutSettings.RecordSelectorLocationProperty"/>
		/// <seealso cref="FieldLayoutSettings.RecordSelectorStyle"/>
		/// <seealso cref="FieldLayoutSettings.RecordSelectorStyleSelector"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordSelectorLocation"/>
		//[Description("Determines if and where RecordSelectors will be displayed relative to a record's cell area (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public RecordSelectorLocation RecordSelectorLocationResolved
        {
            get
            {
                RecordSelectorLocation location;

                if (this._settings != null )
                {
                    location = this._settings.RecordSelectorLocation;
                    if (location != RecordSelectorLocation.Default)
                        return location;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    location = this._presenter.FieldLayoutSettings.RecordSelectorLocation;
                    if (location != RecordSelectorLocation.Default)
                        return location;
                }

				if (this._presenter != null)
				{
					if ( this.IsHorizontal )
						return RecordSelectorLocation.AboveCellArea;
					else
						return RecordSelectorLocation.LeftOfCellArea;
				}

                return RecordSelectorLocation.None;
            }
        }

                #endregion //RecordSelectorLocationResolved

				#region ReevaluateFiltersOnDataChangeResolved

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved value of <see cref="FieldLayoutSettings.FilterAction"/> property.
		/// </summary>
		/// <remarks>
		/// <b>FilterActionResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.FilterAction"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		public bool ReevaluateFiltersOnDataChangeResolved
		{
			get
			{
				// SSP 12/21/11 TFS67264 - Optimizations
				// 
				// --------------------------------------------------------------------------------------------
				if ( ! _cachedReevaluateFiltersOnDataChange.HasValue )
					_cachedReevaluateFiltersOnDataChange = this.GetResolvedValue<bool?>( FieldLayoutSettings.ReevaluateFiltersOnDataChangeProperty, null, true );

				return _cachedReevaluateFiltersOnDataChange.Value;
					
				//bool? ret;

				//if ( null != _settings )
				//{
				//    ret = _settings.ReevaluateFiltersOnDataChange;
				//    if ( ret.HasValue )
				//        return ret.Value;
				//}

				//FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				//if ( null != dpSettings )
				//{
				//    ret = dpSettings.ReevaluateFiltersOnDataChange;
				//    if ( ret.HasValue )
				//        return ret.Value;
				//}

				//return true;
				// --------------------------------------------------------------------------------------------
			}
		}

				#endregion // ReevaluateFiltersOnDataChangeResolved

                #region ResizingModeResolved

        /// <summary>
        /// Determines if and how cells and labels are resized by the user (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="HeaderAreaTemplate"/>
        /// <seealso cref="FieldLayoutSettings.ResizingMode"/>
        /// <seealso cref="FieldLayoutSettings.ResizingModeProperty"/>
		/// <seealso cref="Infragistics.Windows.Controls.ResizingMode"/>
        //[Description("Determines if and how cells and labels are resized by the user (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public ResizingMode ResizingModeResolved
        {
            get
            {
                ResizingMode mode;

                if (this._settings != null )
                {
                    mode = this._settings.ResizingMode;
                    if (mode != ResizingMode.Default)
                        return mode;
                }

				if (this._presenter != null)
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						mode = this._presenter.FieldLayoutSettings.ResizingMode;
						if (mode != ResizingMode.Default)
							return mode;
					}
				}

                return ResizingMode.Deferred;
            }
        }

                #endregion //ResizingModeResolved

				#region ScrollTipField

		/// <summary>
		/// Returns the <see cref="Field"/> whose information is displayed in a scroll tip when the GridViewSettings.ScrollingMode is set to DeferredWithScrollTips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Returns the <see cref="Field"/> whose information is displayed in a scroll tip when the <see cref="DataPresenterBase.ScrollingMode"/> is set to <b>DeferredWithScrollTips</b>.</p>
		/// </remarks>
		/// <value>Returns the <see cref="Field"/> whose <see cref="Field.IsScrollTipField"/> was set to true or the 
		/// <see cref="PrimaryField"/> if no field was designated as the scroll tip field.</value>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.IsScrollTipField"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public Field ScrollTipField
		{
			get
			{
				if (this._fields != null)
				{
					// if the fields have changed...
					if (this._fields.Version != this._scrollTipFieldVersion)
					{
						// sync the version
						this._scrollTipFieldVersion = this._fields.Version;

						// if the scrollTip field has been remove, clear the setting
						if (this._scrollTipField != null &&
							this._fields.Contains(this._scrollTipField) == false)
						{
							this.ScrollTipField = null;
						}
					}

					if (this._scrollTipField != null)
						return this._scrollTipField;

					return this.PrimaryField;
				}

				return null;
			}
			internal set
			{
				





				Field oldScrollTipField = this._scrollTipField;

				this._scrollTipField = value;

				// clear the IsScrollTipField flag on the old field
				if (null != oldScrollTipField)
					oldScrollTipField.IsScrollTipField = false;
			}
		}

				#endregion //ScrollTipField

                #region SelectionTypeCellResolved

        /// <summary>
        /// Determines hows <see cref="Cell"/>s can be selected (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayoutSettings.SelectionTypeCell"/>
        /// <seealso cref="SelectionTypeFieldResolved"/>
        /// <seealso cref="SelectionTypeRecordResolved"/>
        //[Description("Determines how cells can be selected (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public SelectionType SelectionTypeCellResolved
        {
            get
            {
                SelectionType selectionType;

                if (this._settings != null )
                {
                    selectionType = this._settings.SelectionTypeCell;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    selectionType = this._presenter.FieldLayoutSettings.SelectionTypeCell;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                return SelectionType.Extended;
            }
        }

                #endregion //SelectionTypeCellResolved

                #region SelectionTypeFieldResolved

        /// <summary>
        /// Determines hows <see cref="Field"/>s can be selected (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayoutSettings.SelectionTypeField"/>
        /// <seealso cref="SelectionTypeCellResolved"/>
        /// <seealso cref="SelectionTypeRecordResolved"/>
        //[Description("Determines how fields can be selected (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public SelectionType SelectionTypeFieldResolved
        {
            get
            {
                SelectionType selectionType;

                if (this._settings != null )
                {
                    selectionType = this._settings.SelectionTypeField;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    selectionType = this._presenter.FieldLayoutSettings.SelectionTypeField;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                return SelectionType.Extended;
            }
        }

                #endregion //SelectionTypeFieldResolved

                #region SelectionTypeRecordResolved

        /// <summary>
        /// Determines hows <see cref="Record"/>s can be selected (read-only).
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
        /// <seealso cref="FieldLayoutSettings.SelectionTypeRecord"/>
        /// <seealso cref="SelectionTypeFieldResolved"/>
        /// <seealso cref="SelectionTypeCellResolved"/>
        //[Description("Determines how records can be selected (read-only).")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(true)]
		public SelectionType SelectionTypeRecordResolved
        {
            get
            {
                SelectionType selectionType;

                if (this._settings != null )
                {
                    selectionType = this._settings.SelectionTypeRecord;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                if ( this._presenter != null &&
                     this._presenter.HasFieldLayoutSettings )
                {
                    selectionType = this._presenter.FieldLayoutSettings.SelectionTypeRecord;
                    if (selectionType != SelectionType.Default)
                        return selectionType;
                }

                return SelectionType.Extended;
            }
        }

                #endregion //SelectionTypeRecordResolved

                #region Settings

        /// <summary>
        /// Gets/sets an object that holds this FieldLayout's specific settings 
        /// </summary>
		/// <remarks>
		/// <para class="body"><see cref="FieldLayoutSettings"/> objects are exposed via the following 2 properties:
		/// <ul>
		/// <li><see cref="DataPresenterBase"/>'s <see cref="DataPresenterBase.FieldLayoutSettings"/> - settings specified here become the default for all <see cref="FieldLayout"/>s in the <see cref="DataPresenterBase.FieldLayouts"/> collection.</li>
		/// <li><see cref="FieldLayout"/>'s <see cref="FieldLayout.Settings"/> - settings specified here apply to only this one specific <see cref="FieldLayout"/>.</li>
		/// </ul>
		/// </para>
		/// </remarks>
        //[Description("Gets/sets an object that holds FieldLayout specific settings")]
        //[Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public FieldLayoutSettings Settings
        {
            get
            {
                if (this._settings == null)
                {
                     // JJD 12/21/07
                    // Delay setting this property until InitializeOwner is called so we only
                    // do it at run time. This will allow the VS2008 designer to work properly 
                    if (this._presenter == null ||
                         !DesignerProperties.GetIsInDesignMode(this._presenter))
                    {
                        this._settings = new FieldLayoutSettings();

                        // listen for property changes
                        this._settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsPropertyChanged);
                    }
                }

                return this._settings;
            }
            set
            {
                if (value != this._settings)
                {
                    // remove property change listener for old settings
                    if (this._settings != null)
                        this._settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsPropertyChanged);

                    this._settings = value;

                    if (this._settings != null)
                        this._settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsPropertyChanged);

                    this.RaisePropertyChangedEvent("Settings");
                }
            }
        }

		
		
		internal FieldLayoutSettings SettingsIfAllocated
		{
			get
			{
				return _settings;
			}
		}

		/// <summary>
		/// Determines if the <see cref="Settings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSettings()
        {
            return this._settings != null && this._settings.ShouldSerialize();
        }

		/// <summary>
		/// Resets the <see cref="Settings"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSettings()
		{
			this._settings = null;
		}

                #endregion //Settings	

                #region SortedFields

        /// <summary>
        /// Returns the collection of <see cref="FieldSortDescription"/> objects that determine sort order and groupby status (read-only).
        /// </summary>
		/// <remarks>
		/// <remarks>
        /// <p class="body">Refer to the <a href="xamDataPresenter_Sorting_and_Grouping.html">Sorting and Grouping</a> topic in the Developer's Guide for an explanation of how this collection is used.</p>
		/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
		/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
		/// </remarks>
		/// </remarks>
        /// <seealso cref="FieldSortDescription"/>
        /// <seealso cref="FieldSortDescription.IsGroupBy"/>
        /// <seealso cref="FieldSortDescriptionCollection"/>
        /// <seealso cref="Field.SortStatus"/>
        //[Description("Returns the collection of FieldSortDescription objects that determine sort order and groupby status (read-only).")]
        //[Category("Behavior")]
        public FieldSortDescriptionCollection SortedFields
        {
            get
            {
                if ( this._sortedFields == null )
                {
                    this._sortedFields = new FieldSortDescriptionCollection(this, new List<FieldSortDescription>());
                    this._sortedFields.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnSortedFieldsCollectionChanged);
                    ((INotifyPropertyChanged)(this._sortedFields)).PropertyChanged += new PropertyChangedEventHandler(OnSortedFieldsPropertyChanged);
                }
                return this._sortedFields;
            }
        }

        void OnSortedFieldsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CountOfGroupByFields")
                this.BumpGroupByVersion();

			// SSP 4/30/09 TFS17233
			// Since we are bumping the sort version in OnSortedFieldsCollectionChanged, this is
			// not necessary. This causes performance issue because recently a change was made to
			// sort records syncrhonously whenever sort version is bumped, which means that we 
			// would end up sorting records multiple times.
			// 
            //this.BumpSortVersion();
        }

        private void OnSortedFieldsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.BumpSortVersion();
        }

                #endregion //SortedFields

				#region SummaryDefinitions

		
		

		/// <summary>
		/// Gets the summaries collection for this field layout. These summaries will be calculated for each
		/// RecordCollection associated with the field layout.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can calculate summaries (like sum, average, maximum, minimum or any custom calculation logic that
		/// you implement) of a field by adding a <see cref="SummaryDefinition"/> instance to this collection. 
		/// The calculated summaries by default are displayed in a summary record for each record 
		/// collection. The calculation results can also be accessed via 
		/// <see cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.SummaryResults"/> collection of a 
		/// record collection. 
		/// </para>
		/// <para class="body">
		/// DataGrid also has a built-in user interface that lets the user select summary calculations for a field.
		/// To enable summary selection UI, set the FieldSettings' <see cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowSummaries"/> and
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryUIType"/> properties. FieldSettings' 
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryDisplayArea"/> property controls if and where the summaries will be 
		/// displayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowSummaries"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryUIType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinitionCollection"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryDefinition"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.SummaryDisplayArea"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.SummaryCalculator"/>
		//[Description( "Specifies field data summaries for the field layout." )]
		//[Category( "Data" )]
		[Bindable( true )]
		// AS 2/11/11 NA 2011.1 Word Writer
		// Because we didn't have this attribute, the MarkupObject (and other serialization mechanisms) 
		// wouldn't evaluate the contents of the property since it looks like a readonly property.
		//
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SummaryDefinitionCollection SummaryDefinitions
		{
			get
			{
				if ( null == _summaryDefinitions )
					_summaryDefinitions = new SummaryDefinitionCollection( this );

				return _summaryDefinitions;
			}
		}

		internal SummaryDefinitionCollection SummaryDefinitionsIfAllocated
		{
			get
			{
				return _summaryDefinitions;
			}
		}

		/// <summary>
		/// Returns true if the SummaryDefinitions property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSummaryDefinitions( )
		{
			return null != _summaryDefinitions && _summaryDefinitions.Count > 0;
		}

		/// <summary>
		/// Resets the SummaryDefinitions property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSummaryDefinitions( )
		{
			if ( null != _summaryDefinitions )
				_summaryDefinitions.Clear( );
		}

				#endregion // SummaryDefinitions

				#region SummaryDescriptionMask

		
		

		/// <summary>
		/// Identifies the <see cref="SummaryDescriptionMask"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryDescriptionMaskProperty = DependencyProperty.Register(
				"SummaryDescriptionMask",
				typeof( string ),
				typeof( FieldLayout ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies what to display in the header of the summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Each summary record by default displays a header above it. You can use 
		/// <b>SummaryDescriptionMask</b> and <see cref="SummaryDescriptionMaskInGroupBy"/> properties 
		/// to specify what's displayed inside the header. <b>SummaryDescriptionMaskInGroupBy</b> is 
		/// used for any summary record whose parent record is a <see cref="GroupByRecord"/>. Otherwise 
		/// <b>SummaryDescriptionMask</b> is used.
		/// </para>
		/// <para class="body">
		/// Summary description mask is a replacement string with specific tokens that will be replaced 
		/// by their corresponding values. The following is a list of supported tokens:
		/// <br/>
		/// <list type="table">
		/// <listheader>
		///		<term>Token</term>
		///		<description>Replacement Value</description>
		/// </listheader>
		/// <item>
		///		<term>[GROUP_BY_VALUE]</term>
		///		<description>Parent group-by record's value.</description>
		/// </item>
		/// <item>
		///		<term>[GROUP_BY_VALUES]</term>
		///		<description>Values of all the ancestor group-by records separated by ','.</description>
		/// </item>
		/// <item>
		///		<term>[PRIMARY_FIELD]</term>
		///		<description>
		///			Value of the primary field (specified using Field's 
		///			<see cref="Field.IsPrimary"/> property) in the parent data record.
		///		</description>
		/// </item>
		/// <item>
		///		<term>[SCROLLTIP_FIELD]</term>
		///		<description>
		///			Value of the scroll-tip field (specified using the Field's 
		///			<see cref="Field.IsScrollTipField"/> property) in the parent data record.
		///		</description>
		/// </item>
		/// <item>
		///		<term>[PARENT_FIELD_NAME]</term>
		///		<description>
		///			If the summary record is associated with a child record collection, this
		///			token will be replaced with the name of the parent expandable field.
		///		</description>
		/// </item>
		/// <item>
		///		<term>[FieldName]</term>
		///		<description>
		///			Here the <i>FieldName</i> can be any field in the parent data record. This
		///			will be replaced with the value of that field in the parent data record.
		///		</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para class="body">
		/// If not explicitly set, this property is resolved as follows: <br/>
		/// <list type="bullet">
		/// <item>
		///		<term>Summaries of root data records</term>
		///		<description>
		///			<b>"Grand Summaries"</b>
		///		</description>
		/// </item>
		/// <item>
		///		<term>Summaries of child records</term>
		///		<description>
		///			<b>"Summaries for [PRIMARY_FIELD]"</b>
		///			<br/><b>Note</b> that when you have group-by records, <see cref="SummaryDescriptionMaskInGroupBy"/>
		///			will be used instead of this property.
		///		</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> To hide the summary record header, use the FieldLayoutSetting's 
		/// <see cref="FieldLayoutSettings.SummaryDescriptionVisibility"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryDescriptionMaskInGroupBy"/>
		/// <seealso cref="FieldLayoutSettings.SummaryDescriptionVisibility"/>
		/// <seealso cref="SummaryResultCollection.SummaryRecordHeader"/>
		/// <seealso cref="SummaryRecord.SummaryRecordHeaderResolved"/>
		//[Description( "Specifies the what to dispaly in the header of the summary record." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string SummaryDescriptionMask
		{
			get
			{
				return (string)this.GetValue( SummaryDescriptionMaskProperty );
			}
			set
			{
				this.SetValue( SummaryDescriptionMaskProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the SummaryDescriptionMask property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSummaryDescriptionMask( )
		{
			return Utilities.ShouldSerialize( SummaryDescriptionMaskProperty, this );
		}

		/// <summary>
		/// Resets the SummaryDescriptionMask property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSummaryDescriptionMask( )
		{
			this.ClearValue( SummaryDescriptionMaskProperty );
		}

				#endregion // SummaryDescriptionMask

				#region SummaryDescriptionMaskInGroupBy

		
		

		/// <summary>
		/// Identifies the <see cref="SummaryDescriptionMaskInGroupBy"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SummaryDescriptionMaskInGroupByProperty = DependencyProperty.Register(
				"SummaryDescriptionMaskInGroupBy",
				typeof( string ),
				typeof( FieldLayout ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the what to display in the header of the summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// See <see cref="SummaryDescriptionMask"/> for more information.
		/// </para>
		/// <para class="body">
		/// If not explicitly set, this property is resolved as "Summaries for [GROUP_BY_VALUE]".
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> To hide the summary record header, use the FieldLayoutSetting's 
		/// <see cref="FieldLayoutSettings.SummaryDescriptionVisibility"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryDescriptionMask"/>
		/// <seealso cref="FieldLayoutSettings.SummaryDescriptionVisibility"/>
		/// <seealso cref="SummaryResultCollection.SummaryRecordHeader"/>
		/// <seealso cref="SummaryRecord.SummaryRecordHeaderResolved"/>
		//[Description( "Specifies the what to dispaly in the header of the summary record when records are grouped." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string SummaryDescriptionMaskInGroupBy
		{
			get
			{
				return (string)this.GetValue( SummaryDescriptionMaskInGroupByProperty );
			}
			set
			{
				this.SetValue( SummaryDescriptionMaskInGroupByProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the SummaryDescriptionMaskInGroupBy property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSummaryDescriptionMaskInGroupBy( )
		{
			return Utilities.ShouldSerialize( SummaryDescriptionMaskInGroupByProperty, this );
		}

		/// <summary>
		/// Resets the SummaryDescriptionMaskInGroupBy property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSummaryDescriptionMaskInGroupBy( )
		{
			this.ClearValue( SummaryDescriptionMaskInGroupByProperty );
		}

				#endregion // SummaryDescriptionMaskInGroupBy

                // JJD 2/20/08
                // Added Tag property
                #region Tag

        /// <summary>
        /// Identifies the <see cref="Tag"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TagProperty = FrameworkElement.TagProperty.AddOwner(typeof(FieldLayout));

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this object.
        /// </summary>
        [Localizability(LocalizationCategory.NeverLocalize)]
        // JJD 2/11/09 - TFS10860/TFS13609
        [CloneBehavior(CloneBehavior.ShareInstance)]
        public object Tag
        {
            get 
            { 
                return this.GetValue(TagProperty); 
            }
            set
            {
                this.SetValue(TagProperty, value);
            }
        }

                #endregion //Tag	

				#region ToolTip

		// SSP 6/3/09 - NAS9.2 Field Chooser
		// 

		/// <summary>
		/// Identifies the <see cref="ToolTip"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
			"ToolTip",
			typeof( object ),
			typeof( FieldLayout ),
			new FrameworkPropertyMetadata( null )
		);

		/// <summary>
		/// Specifies the tooltip for the field layout. It's displayed when the user hovers the mouse over the 
		/// field layout label.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Field layout label is displayed by the element for the <see cref="ExpandableFieldRecord"/> that represents field layout.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the tooltip that's displayed when the mouse is hovered over the field-layout label." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public object ToolTip
		{
			get
			{
				return (object)this.GetValue( ToolTipProperty );
			}
			set
			{
				this.SetValue( ToolTipProperty, value );
			}
		}

				#endregion // ToolTip

                #region TotalColumnsGenerated

        /// <summary>
        /// Return the total number of columns generated 
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AutoArrangeMaxColumns"/>
        /// <seealso cref="FieldLayoutSettings.AutoArrangeMaxRows"/>
        /// <seealso cref="HeaderAreaTemplate"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
        // AS 2/27/09 TFS14730
        // The backing member is only updated by the style generator. We originally were 
        // going to change this to that it forces a verification but that could impact
        // performance. 
        //
        //public int TotalColumnsGenerated { get { return this._totalColumnsGenerated; } }
		public int TotalColumnsGenerated 
        { 
            get 
            {
                Debug.Assert(null == _presenter || (null != _styleGenerator && _styleGenerator.IsDirty == false));

                return this._totalColumnsGenerated; 
            }
        }

                #endregion //TotalColumnsGenerated	

                #region TotalRowsGenerated

        /// <summary>
        /// Return the total number of rows generated 
        /// </summary>
        /// <seealso cref="FieldLayoutSettings"/>
        /// <seealso cref="FieldLayoutSettings.AutoArrangeCells"/>
        /// <seealso cref="FieldLayoutSettings.AutoArrangeMaxColumns"/>
        /// <seealso cref="FieldLayoutSettings.AutoArrangeMaxRows"/>
        /// <seealso cref="HeaderAreaTemplate"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
        // AS 2/27/09 TFS14730
        // The backing member is only updated by the style generator. We originally were 
        // going to change this to that it forces a verification but that could impact
        // performance. 
        //
        //public int TotalRowsGenerated { get { return this._totalRowsGenerated; } }
		public int TotalRowsGenerated 
        { 
            get 
            {
                Debug.Assert(null == _presenter || (null != _styleGenerator && _styleGenerator.IsDirty == false));

                return this._totalRowsGenerated; 
            } 
        }

                #endregion //TotalRowsGenerated	
    
 			#endregion // Public Properties

			#region Internal Properties

                // AS 4/8/09 NA 2009.2 ClipboardSupport
				#region AllowClipboardOperationsResolved

        internal AllowClipboardOperations AllowClipboardOperationsResolved
		{
			get
			{
                return GetResolvedValue<AllowClipboardOperations?>(FieldLayoutSettings.AllowClipboardOperationsProperty, null, AllowClipboardOperations.None).Value;
			}
		}

				#endregion // AllowClipboardOperationsResolved

				#region AllowHidingViaFieldChooserResolved

		
		
		
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion // AllowHidingViaFieldChooserResolved

				#region AllowRecordFilteringResolvedDefault

		// SSP 9/8/09 TFS21710
		// 
		/// <summary>
		/// Returns the partially resolved value based on the field settings of the field layout
		/// and the data presenter.
		/// </summary>
		internal bool AllowRecordFilteringResolvedDefault
		{
			get
			{
				// JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
				DataPresenterBase dp = _presenter;
				if ( dp != null && dp.IsReportControl )
					return false;

				bool? ret;

				// If field doesn't have it set, then use the field layout's setting.
				// 
				FieldSettings settings = this.FieldSettingsIfAllocated;
				if ( null != settings )
				{
					ret = settings.AllowRecordFiltering;
					if ( ret.HasValue )
						return ret.Value;
				}

				// If the field layout doesn't have it set then use the data presenter's setting.
				// 
				settings = null != dp ? dp.FieldSettingsIfAllocated : null;
				if ( null != settings )
				{
					ret = settings.AllowRecordFiltering;
					if ( ret.HasValue )
						return ret.Value;
				}

				// Resolve default to false.
				// 
				return false;
			}
		}

				#endregion // AllowRecordFilteringResolvedDefault

                #region AreFieldsInitialized

        internal bool AreFieldsInitialized
        {
            get { return this._areFieldsInitialized; }
        }

                #endregion //AreFieldsInitialized	

				#region AutoArrangeCellsResolved

		internal AutoArrangeCells AutoArrangeCellsResolved
		{
			get
			{
				// get the layout mode setting 1st from the field layout
				AutoArrangeCells mode = AutoArrangeCells.Default;

				if (this._settings != null)
					mode = this._settings.AutoArrangeCells;

                // JJD 12/21/07
                // Check HasFieldLayoutSettings
				// if not explicitly set then get the setting from the layout's owner
				//if (mode == AutoArrangeCells.Default)
				if (mode == AutoArrangeCells.Default && this.DataPresenter.HasFieldLayoutSettings )
					mode = this.DataPresenter.FieldLayoutSettings.AutoArrangeCells;

				// if still not set then use the style generator's default
				if (mode == AutoArrangeCells.Default)
				{
					ViewBase currentView = this.DataPresenter.CurrentViewInternal;

					mode = currentView.DefaultAutoArrangeCells;

					if (mode == AutoArrangeCells.Default)
					{
						if (currentView.HasLogicalOrientation)
						{
							if (currentView.LogicalOrientation == Orientation.Vertical)
								mode = AutoArrangeCells.LeftToRight;
							else
								mode = AutoArrangeCells.TopToBottom;
						}
						else
							mode = AutoArrangeCells.TopToBottom;
					}
				}

				return mode;
			}
		}

				#endregion //AutoArrangeCellsResolved	

				// AS 6/9/09 NA 2009.2 Field Sizing
				#region AutoFitModeResolved

		internal AutoFitMode AutoFitModeResolved
		{
			get
			{
                return GetResolvedValue<AutoFitMode>(FieldLayoutSettings.AutoFitModeProperty, AutoFitMode.Default, AutoFitMode.Default);
			}
		}

				#endregion // AutoFitModeResolved

				// AS 6/26/09 NA 2009.2 Field Sizing
				#region AutoSizeInfo
		internal AutoSizeFieldLayoutInfo AutoSizeInfo
		{
			get { return _autoSizeInfo; }
		}
				#endregion //AutoSizeInfo

				#region CalculationScopeResolved

		
		
		/// <summary>
		/// Resolved calculation scope.
		/// </summary>
		internal CalculationScope CalculationScopeResolved
		{
			get
			{
				CalculationScope ret;

				if ( null != _settings )
				{
					ret = _settings.CalculationScope;
					if ( CalculationScope.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.CalculationScope;
					if ( CalculationScope.Default != ret )
						return ret;
				}

				return CalculationScope.FilteredSortedList;
			}
		}

				#endregion // CalculationScopeResolved
	
				#region CellAreaRecordSizeManager

        
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

				#endregion //CellAreaRecordSizeManager	

                // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
                #region CellLayoutManager
        internal FieldGridBagLayoutManager CellLayoutManager
        {
            get
            {
                this.VerifyLayoutManagers();

                return this._cellLayoutManager;
            }
        } 
                #endregion //CellLayoutManager

                // AS 4/8/09 NA 2009.2 ClipboardSupport
				#region CopyFieldLabelsToClipboardResolved

        internal bool CopyFieldLabelsToClipboardResolved
		{
			get
			{
                return GetResolvedValue(FieldLayoutSettings.CopyFieldLabelsToClipboardProperty, (bool?)null, (bool?)false).Value;
			}
		}

				#endregion // CopyFieldLabelsToClipboardResolved

                // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
                #region ShouldCreateDragFieldLayout
        internal bool ShouldCreateDragFieldLayout
        {
            get { return _createDragFieldLayout; }
            set { _createDragFieldLayout = value; }
        } 
                #endregion //ShouldCreateDragFieldLayout

				#region DataRecordCellAreaTemplateGridResolved

		internal Grid DataRecordCellAreaTemplateGridResolved
		{
			get
			{
				Grid grid = null;

				if (this._settings != null)
				{
					grid = this._settings.DataRecordCellAreaTemplateGrid;
					if (grid != null)
						return grid;
				}

				if (this._presenter != null &&
					 this._presenter.HasFieldLayoutSettings)
					grid = this._presenter.FieldLayoutSettings.DataRecordCellAreaTemplateGrid;

				return grid;
			}
		}

				#endregion //DataRecordCellAreaTemplateGridResolved	

				#region DefaultColumnDefinitionResolved

		[ThreadStatic]	// JM 11-05-08 TFS9562
		private static ColumnDefinition g_fallbackColumnDefinition = null;
		private static ColumnDefinition FallbackColumnDefinition
		{
			get
			{
				if (g_fallbackColumnDefinition == null)
				{
					lock (typeof(FieldLayout))
					{
						if (g_fallbackColumnDefinition == null)
						{
							g_fallbackColumnDefinition = new ColumnDefinition();
							g_fallbackColumnDefinition.Width = new GridLength(0.0, GridUnitType.Auto);
						}
					}
				}
				return g_fallbackColumnDefinition;
			}
		}

		[ThreadStatic]	// JM 11-05-08 TFS9562
		private static ColumnDefinition g_fallbackColumnDefinitionAutoFit = null;
		private static ColumnDefinition FallbackColumnDefinitionAutoFit
		{
			get
			{
				if (g_fallbackColumnDefinitionAutoFit == null)
				{
					lock (typeof(FieldLayout))
					{
						if (g_fallbackColumnDefinitionAutoFit == null)
						{
							g_fallbackColumnDefinitionAutoFit = new ColumnDefinition();
							g_fallbackColumnDefinitionAutoFit.Width = new GridLength(1.0, GridUnitType.Star);
						}
					}
				}
				return g_fallbackColumnDefinitionAutoFit;
			}
		}

		internal ColumnDefinition DefaultColumnDefinitionResolved
		{
			get
			{
				ColumnDefinition def;

				if (this._settings != null)
				{
					def = this._settings.DefaultColumnDefinition;
					if (def != null)
						return def;
				}

				if (this._presenter != null)
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						def = this._presenter.FieldLayoutSettings.DefaultColumnDefinition;
						if (def != null)
							return def;
					}

					// AS 6/22/09 NA 2009.2 Field Sizing
					//if (this._presenter.IsAutoFitWidth)
					if (this.IsAutoFitWidth)
						return FallbackColumnDefinitionAutoFit;

					Grid templatGrid = this.DataRecordCellAreaTemplateGridResolved;

					if ( templatGrid != null &&
						!double.IsNaN( templatGrid.Width ))
						return FallbackColumnDefinitionAutoFit;

				}

				return FallbackColumnDefinition;
			}
		}

				#endregion //DefaultColumnDefinitionResolved	

				#region DefaultRowDefinitionResolved

		[ThreadStatic]	// JM 11-05-08 TFS9562
		private static RowDefinition g_fallbackRowDefinition = null;
		private static RowDefinition FallbackRowDefinition
		{
			get
			{
				if (g_fallbackRowDefinition == null)
				{
					lock (typeof(FieldLayout))
					{
						if (g_fallbackRowDefinition == null)
						{
							g_fallbackRowDefinition = new RowDefinition();
							g_fallbackRowDefinition.Height = new GridLength(0.0, GridUnitType.Auto);
						}
					}
				}
				return g_fallbackRowDefinition;
			}
		}

		[ThreadStatic]	// JM 11-05-08 TFS9562
		private static RowDefinition g_fallbackRowDefinitionAutoFit = null;
		private static RowDefinition FallbackRowDefinitionAutoFit
		{
			get
			{
				if (g_fallbackRowDefinitionAutoFit == null)
				{
					lock (typeof(FieldLayout))
					{
						if (g_fallbackRowDefinitionAutoFit == null)
						{
							g_fallbackRowDefinitionAutoFit = new RowDefinition();
							g_fallbackRowDefinitionAutoFit.Height = new GridLength(1.0, GridUnitType.Star);
						}
					}
				}
				return g_fallbackRowDefinitionAutoFit;
			}
		}

		internal RowDefinition DefaultRowDefinitionResolved
		{
			get
			{
				RowDefinition def;

				if (this._settings != null)
				{
					def = this._settings.DefaultRowDefinition;
					if (def != null)
						return def;
				}

				if (this._presenter != null)
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						def = this._presenter.FieldLayoutSettings.DefaultRowDefinition;
						if (def != null)
							return def;
					}

					// AS 6/22/09 NA 2009.2 Field Sizing
					//if (this._presenter.IsAutoFitHeight)
					if (this.IsAutoFitHeight)
						return FallbackRowDefinitionAutoFit;

					Grid templatGrid = this.DataRecordCellAreaTemplateGridResolved;

					if ( templatGrid != null &&
						!double.IsNaN( templatGrid.Height ))
						return FallbackRowDefinitionAutoFit;
				}

				return FallbackRowDefinition;
			}
		}

				#endregion //DefaultRowDefinitionResolved	

				// JM 07-29-09 TFS 19241 - Added
				#region FieldLayoutInitializedEventRaised

		internal bool FieldLayoutInitializedEventRaised
		{
			get { return this._fieldLayoutInitializedEventRaised; }
		}

				#endregion //FieldLayoutInitializedEventRaised

				#region FilterEvaluationModeResolved

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal FilterEvaluationMode FilterEvaluationModeResolved
		{
			get
			{
				if ( !_cachedFilterEvaluationModeResolved.HasValue )
				{
					_cachedFilterEvaluationModeResolved = this.GetResolvedValue<FilterEvaluationMode>(
						FieldLayoutSettings.FilterEvaluationModeProperty,
						FilterEvaluationMode.Default,
						FilterEvaluationMode.Auto
					);
				}

				return _cachedFilterEvaluationModeResolved.Value;
			}
		}

				#endregion // FilterEvaluationModeResolved

                // JJD 1/15/09 - NA 2009 vol 1 - record filtering
                #region ExpansionIndicatorSize

        internal Size ExpansionIndicatorSize
        {
            get { return this._expansionIndicatorSize; }
            set { this._expansionIndicatorSize = value; }
        }

                #endregion //ExpansionIndicatorSize	
        
                // AS 1/6/09 NA 2009 Vol 1 - Fixed Fields
                #region FixedFieldVersion
        internal int FixedFieldVersion
        {
            get
            {
                return this._fixedFieldVersion + this.TemplateVersion;
            }
        } 
                #endregion //FixedFieldVersion

				#region CanUseVirtualizedCellAreaTemplate
        
#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)





				#endregion //CanUseVirtualizedCellAreaTemplate

                // JJD 4/28/08 - BR31406 and BR31707 - added
                #region ExpansionIndicatorDisplayModeResolved

		internal ExpansionIndicatorDisplayMode ExpansionIndicatorDisplayModeResolved
        {
            get
            {
                ExpansionIndicatorDisplayMode mode;

                if (this._settings != null )
                {
                    mode = this._settings.ExpansionIndicatorDisplayMode;
                    if (mode != ExpansionIndicatorDisplayMode.Default)
                        return mode;
                }

				if (this._presenter != null)
				{
					if (this._presenter.HasFieldLayoutSettings)
					{
						mode = this._presenter.FieldLayoutSettings.ExpansionIndicatorDisplayMode;
						if (mode != ExpansionIndicatorDisplayMode.Default)
							return mode;
					}
    			}

                 return ExpansionIndicatorDisplayMode.CheckOnExpand;
            }
        }

                #endregion //ExpansionIndicatorDisplayModeResolved

				#region GroupByEvaluationModeResolved

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal GroupByEvaluationMode GroupByEvaluationModeResolvedDefault
		{
			get
			{
				if ( !_cachedGroupByEvaluationModeResolved.HasValue )
				{
					_cachedGroupByEvaluationModeResolved = this.GetResolvedValue<GroupByEvaluationMode>(
						FieldLayoutSettings.GroupByEvaluationModeProperty,
						GroupByEvaluationMode.Default,
						GroupByEvaluationMode.Auto
					);
				}

				return _cachedGroupByEvaluationModeResolved.Value;
			}
		}

				#endregion // GroupByEvaluationModeResolved

				#region GroupBySummaryDisplayModeResolved

		
		
		/// <summary>
		/// Determines whether the user can add records (read-only).
		/// </summary>
		/// <remarks>This property is ignored if the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/> does not support adding of records (i.e. does not implement the <see cref="System.ComponentModel.IBindingList"/> interface or that interface's <see cref="System.ComponentModel.IBindingList.AllowNew"/> property returns false).</remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="FieldLayoutSettings.AllowAddNew"/>
		/// <seealso cref="FieldLayoutSettings.AllowAddNewProperty"/>
		/// <seealso cref="FieldLayoutSettings.AddNewRecordLocation"/>
		/// <seealso cref="AddNewRecordLocationResolved"/>
		[ReadOnly( true )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		public GroupBySummaryDisplayMode GroupBySummaryDisplayModeResolved
		{
			get
			{
				GroupBySummaryDisplayMode ret;

				if ( null != _settings )
				{
					ret = _settings.GroupBySummaryDisplayMode;
					if ( GroupBySummaryDisplayMode.Default != ret )
						return ret;
				}

				if ( null != _presenter )
				{
					FieldLayoutSettings dpSettings = _presenter.FieldLayoutSettingsIfAllocated;
					if ( null != dpSettings )
					{
						ret = dpSettings.GroupBySummaryDisplayMode;
						if ( GroupBySummaryDisplayMode.Default != ret )
							return ret;
					}
				}

				return GroupBySummaryDisplayMode.Text;
			}
		}

				#endregion // GroupBySummaryDisplayModeResolved

                #region GroupByVersion

        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping
        // Made public so RecordManager could use a PropertyValueTracker 
        //internal int GroupByVersion
        /// <summary>
        /// For internal use only
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
        public int GroupByVersion
        {
            get
            {
                return this._groupByVersion;
            }
        }

                #endregion //GroupByVersion

				// AS 12/13/07 BR25223
				#region HasCellAreaRecordSizeManager
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //HasCellAreaRecordSizeManager

				// AS 12/13/07 BR25223
				#region HasLogicalColumnSizeManager
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //HasLogicalColumnSizeManager

				// AS 10/9/09 NA 2010.1 - CardView
				#region HasStarFieldsX
		internal bool HasStarFieldsX
		{
			get
			{
				if (null == _hasStarFieldsX)
				{
					_hasStarFieldsX = this.HasStarFields(true);
				}

				return _hasStarFieldsX.Value;
			}
		} 
				#endregion //HasStarFieldsX

				// AS 10/9/09 NA 2010.1 - CardView
				#region HasStarFieldsY
		internal bool HasStarFieldsY
		{
			get
			{
				if (null == _hasStarFieldsY)
				{
					_hasStarFieldsY = this.HasStarFields(false);
				}

				return _hasStarFieldsY.Value;
			}
		} 
				#endregion //HasStarFieldsY

				#region HeaderAreaRecordSizeManager

        
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				#endregion //HeaderAreaRecordSizeManager	

				#region GridColumnWidthVersion

		// AS 11/29/10 TFS60418
		// Previously we were not reading this value other than the bindings to this DP. However 
		// now we are storing this value in the FieldGridBagLayoutManager so its better to have a
		// cached member than access the dp's value.
		//
		private int _cachedGridColumnWidthVersion;

        internal static readonly DependencyProperty GridColumnWidthVersionProperty = DependencyProperty.Register("GridColumnWidthVersion",
            typeof(int), typeof(FieldLayout), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnGridColumnWidthVersionChanged)));

		private static void OnGridColumnWidthVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FieldLayout)d)._cachedGridColumnWidthVersion = (int)e.NewValue;
		}

        internal int GridColumnWidthVersion
        {
            get
            {
				// AS 11/29/10 TFS60418
				//return (int)this.GetValue(FieldLayout.GridColumnWidthVersionProperty);
				return _cachedGridColumnWidthVersion;
            }
            set
            {
                this.SetValue(FieldLayout.GridColumnWidthVersionProperty, value);
            }
        }

                #endregion //GridColumnWidthVersion

				#region SizeToContentManagerVersion

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				#endregion //SizeToContentManagerVersion

				#region GridRowHeightVersion

        
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                #endregion //GridRowHeightVersion

                // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
                #region HasBeenInitializedAfterDataSourceChange

        internal bool HasBeenInitializedAfterDataSourceChange { get { return this._hasBeenInitializedAfterDataSourceChange; } }

                #endregion //HasBeenInitializedAfterDataSourceChange	
    
                #region Index

        internal int Index
        {
            get
            {
                if (this._presenter == null)
                    return -1;

				FieldLayoutCollection layouts = this._presenter.FieldLayouts;

                // Optimization: see if the cached index still represents this field.
                // If not, cache the returned value from the IndexOf method
                if (this._index < 0 ||
					this._index >= layouts.Count ||
					layouts[this._index] != this)
					this._index = layouts.IndexOf(this);

                return this._index;
            }
        }

                #endregion //AreFieldsInitialized	

                #region InternalVersion

        internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
            typeof(int), typeof(FieldLayout), new FrameworkPropertyMetadata(0));

        internal int InternalVersion
        {
            get
            {
                return (int)this.GetValue(FieldLayout.InternalVersionProperty);
            }
			set
			{
				this.SetValue(FieldLayout.InternalVersionProperty, value);
			}
        }

                #endregion //InternalVersion

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region IsAutoFit(Width|Height)
		internal bool IsAutoFit
		{
			get { return this.IsAutoFitWidth || this.IsAutoFitHeight; }
		}

		internal bool IsAutoFitWidth
		{
			get
			{
				this.VerifyAutoFitSettings(false);
				return (_autoFitState.Value & AutoFitState.Width) != 0;
			}
		}

		internal bool IsAutoFitHeight
		{
			get
			{
				this.VerifyAutoFitSettings(false);
				return (_autoFitState.Value & AutoFitState.Height) != 0;
			}
		}

		// AS 6/29/10 TFS32094
		/// <summary>
		/// Indicates if the grid bag panel should consider this field layout to be autofit. ExtendLastField is not autofit with regards to layout since we only ever increase the size of the fields to fill the available area.
		/// </summary>
		internal bool IsGridBagAutoFitWidth
		{
			get { return this.IsAutoFitWidth && this.AutoFitModeResolved != AutoFitMode.ExtendLastField; }
		}

		/// <summary>
		/// Indicates if the grid bag panel should consider this field layout to be autofit. ExtendLastField is not autofit with regards to layout since we only ever increase the size of the fields to fill the available area.
		/// </summary>
		internal bool IsGridBagAutoFitHeight
		{
			get { return this.IsAutoFitHeight && this.AutoFitModeResolved != AutoFitMode.ExtendLastField; }
		}
				#endregion //IsAutoFit(Width|Height)

				#region IsDataRecordSizedToContent

		internal bool IsDataRecordSizedToContent
		{
			get
			{
				switch (this.DataRecordSizingModeResolved)
				{
					case DataRecordSizingMode.SizedToContentAndFixed:
					case DataRecordSizingMode.SizedToContentAndIndividuallySizable:
						return true;
				}

				return false;
			}
		}

				#endregion //IsDataRecordSizedToContent	

				#region IsFieldChooserUIEnabled

		
		

		/// <summary>
		/// Indicates if the field chooser user interface is enabled.
		/// </summary>
		internal bool IsFieldChooserUIEnabled
		{
			get
			{
				DataPresenterBase dp = this.DataPresenter;

				return HeaderPrefixAreaDisplayMode.FieldChooserButton == this.HeaderPrefixAreaDisplayModeResolved
					|| null != dp && dp.HasRegisteredFieldChoosers( );
			}
		}

				#endregion // IsFieldChooserUIEnabled
    
                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                #region IsFixedFieldsEnabled
        internal bool IsFixedFieldsEnabled
        {
            get
            {
                DataPresenterBase dp = this._presenter;

                if (null != dp && dp.IsFixedFieldsSupportedResolved)
                {
                    FixedFieldLayoutInfo ff = this.GetFixedFieldInfo(true);

                    return ff.HasFixableFields || ff.HasFixedFields;
                }

                return false;
            }
        } 
                #endregion //IsFixedFieldsEnabled

				#region IsHorizontal

    		internal bool IsHorizontal
			{
				get
				{
					// AS 11/29/10 TFS60418
					//if (this._presenter != null)
					//{
					//    ViewBase currentView = this._presenter.CurrentViewInternal;
					//    if (currentView.HasLogicalOrientation)
					//        return currentView.LogicalOrientation == Orientation.Horizontal;
					//}
					//
					//return false;
					if (_cachedIsHorizontal == null)
					{
						if (_presenter != null)
						{
							ViewBase currentView = _presenter.CurrentViewInternal;
							_cachedIsHorizontal = currentView.HasLogicalOrientation && currentView.LogicalOrientation == Orientation.Horizontal;
						}
					}

					return _cachedIsHorizontal == true;
				}
			}

				#endregion //IsHorizontal	
    
                // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                #region IsInFieldInitialization
            internal bool IsInFieldInitialization
            {
                get { return _isInFieldInitialization; }
            } 
                #endregion //IsInFieldInitialization

                #region IsInitialRecordLoaded

		internal bool IsInitialRecordLoaded
        {
            get { return this._initialRecordLoaded; }
        }

                #endregion //IsInitialRecordLoaded	

                // AS 2/4/09 NA 2009 Vol 1 - Fixed Fields
                // We added an IsHighlighted property to the FixedFieldSplitter so all 
                // splitters for a given type (near/far) can appear to hottrack when 
                // you are over one of them.
                //
                #region IsOverNearSplitter

        /// <summary>
        /// Identifies the <see cref="IsOverNearSplitter"/> dependency property
        /// </summary>
        internal static readonly DependencyProperty IsOverNearSplitterProperty = DependencyProperty.Register("IsOverNearSplitter",
            typeof(bool), typeof(FieldLayout), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        internal bool IsOverNearSplitter
        {
            get
            {
                return (bool)this.GetValue(FieldLayout.IsOverNearSplitterProperty);
            }
            set
            {
                this.SetValue(FieldLayout.IsOverNearSplitterProperty, value);
            }
        }

                #endregion //IsOverNearSplitter

                // AS 2/4/09 NA 2009 Vol 1 - Fixed Fields
                #region IsOverFarSplitter

        /// <summary>
        /// Identifies the <see cref="IsOverFarSplitter"/> dependency property
        /// </summary>
        internal static readonly DependencyProperty IsOverFarSplitterProperty = DependencyProperty.Register("IsOverFarSplitter",
            typeof(bool), typeof(FieldLayout), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        internal bool IsOverFarSplitter
        {
            get
            {
                return (bool)this.GetValue(FieldLayout.IsOverFarSplitterProperty);
            }
            set
            {
                this.SetValue(FieldLayout.IsOverFarSplitterProperty, value);
            }
        }

                #endregion //IsOverFarSplitter

				// AS 10/13/09 NA 2010.1 - CardView
				#region IsEmptyCellCollapsingSupportedByView
		internal bool IsEmptyCellCollapsingSupportedByView
		{
			get
			{
				if (_cachedIsEmptyCellCollapsingSupportedByView == null)
				{
					_cachedIsEmptyCellCollapsingSupportedByView = false;

					ViewBase view = _presenter != null ? _presenter.CurrentViewInternal : null;
					if (view != null)
						_cachedIsEmptyCellCollapsingSupportedByView = view.IsEmptyCellCollapsingSupported;
				}

				return _cachedIsEmptyCellCollapsingSupportedByView.Value;
			}
		}
				#endregion //IsEmptyCellCollapsingSupportedByView

				// AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
                #region LabelLayoutManager
        internal FieldGridBagLayoutManager LabelLayoutManager
        {
            get
            {
                this.VerifyLayoutManagers();

                return this._labelLayoutManager;
            }
        }
                #endregion //LabelLayoutManager

				// AS 4/12/11 TFS62951
				#region LastSelectableItemLabel
		internal LabelPresenter LastSelectableItemLabel
		{
			get { return Utilities.GetWeakReferenceTargetSafe(_lastSelectableItemLabel) as LabelPresenter; }
			set
			{
				if (value == null)
					_lastSelectableItemLabel = null;
				else
					_lastSelectableItemLabel = new WeakReference(value);
			}
		}
				#endregion //LastSelectableItemLabel

                // AS 12/18/08 NA 2009 Vol 1 - Fixed Fields
                #region LayoutItemVersion
        /// <summary>
        /// Returns a version number used to determine when the layout settings for the fields/constraints has changed.
        /// </summary>
        internal int LayoutItemVersion
        {
            get { return this._layoutItemVersion; }
        }
                #endregion //LayoutItemVersion

        // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
                #region LayoutManagerVersion
        /// <summary>
        /// Returns a version number used to determine when the layoutmanager items has changed.
        /// </summary>
        internal int LayoutManagerVersion
        {
            get { return this._layoutManagerVersion + this.TemplateVersion; }
        }
                #endregion //LayoutManagerVersion

				#region LogicalColumnSizeManager

        
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

				#endregion //LogicalColumnSizeManager	

				// AS 7/21/09 NA 2009.2 Field Sizing
				#region MaxRecordManagerDepth
		internal int MaxRecordManagerDepth
		{
			get { return _maxRecordManagerDepth; }
			set
			{
				if (value > _maxRecordManagerDepth)
				{
					_maxRecordManagerDepth = value;
					this.AutoSizeInfo.OnMaxDepthChanged();
				}
			}
		} 
				#endregion //MaxRecordManagerDepth

                // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
                #region NestingDepth

        internal int NestingDepth
        {
            get
            {
                FieldLayout parentLayout = this.ParentFieldLayout;

                if (parentLayout != null)
                    return 1 + parentLayout.NestingDepth;

                return 0;
            }
        }

                #endregion //NestingDepth	

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region OnFieldWidthHeightChanged
		internal void OnFieldWidthHeightChanged()
		{
			if (_autoFitState != null)
				this.VerifyAutoFitSettings(true);

			// AS 10/9/09 NA 2010.1 - CardView
			_hasStarFieldsX = _hasStarFieldsY = null;

			this.BumpLayoutManagerVersion();
		} 
				#endregion //OnFieldWidthHeightChanged

				// MD 8/13/10
				// Added a way to bypass the FieldLayout.BumpLayoutManagerVersionRequired() logic.
				#region PreventBumpLayoutManagerVersionRequired

		internal bool PreventBumpLayoutManagerVersionRequired
		{
			get { return _preventBumpLayoutManagerVersionRequired; }
			set { _preventBumpLayoutManagerVersionRequired = value; }
		}

				#endregion // PreventBumpLayoutManagerVersionRequired

				#region RecordFilterScopeResolvedDefault

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Gets the resolved default value of <see cref="FieldLayoutSettings.RecordFilterScope"/> property.
		/// </summary>
		/// <remarks>
		/// <b>RecordFilterScopeResolved</b> returns the resolved value of the <see cref="FieldLayoutSettings.RecordFilterScope"/> 
		/// property which can be set on the FieldLayout's <see cref="FieldLayout.Settings"/> or the DataPresenter's
		/// <see cref="DataPresenterBase.FieldLayoutSettings"/>.
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Browsable( false )]
		[ReadOnly( true )]
		[Bindable( true )]
		internal RecordFilterScope RecordFilterScopeResolvedDefault
		{
			get
			{
				
				
				
				if ( _cachedRecordFilterScopeResolvedDefault.HasValue )
					return _cachedRecordFilterScopeResolvedDefault.Value;

				_cachedRecordFilterScopeResolvedDefault = this.InternalRecordFilterScopeResolvedDefault;
				return _cachedRecordFilterScopeResolvedDefault.Value;
			}
		}

		private RecordFilterScope InternalRecordFilterScopeResolvedDefault
		{
			get
			{
				RecordFilterScope ret;

				if ( null != _settings )
				{
					ret = _settings.RecordFilterScope;
					if ( RecordFilterScope.Default != ret )
						return ret;
				}

				FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
				if ( null != dpSettings )
				{
					ret = dpSettings.RecordFilterScope;
					if ( RecordFilterScope.Default != ret )
						return ret;
				}

				return RecordFilterScope.Default;
			}
		}

				#endregion // RecordFilterScopeResolvedDefault

				#region RecordSeparatorLocationResolved

		// SSP 5/6/08 - Summaries Feature
		// Added RecordSeparatorLocation.
		// 
		internal RecordSeparatorLocation RecordSeparatorLocationResolved
		{
			get
			{
				RecordSeparatorLocation? ret = null;

				if ( null != _settings )
				{
					ret = _settings.RecordSeparatorLocation;
					if ( ret.HasValue )
						return ret.Value;
				}

				if ( null != _presenter )
				{
					FieldLayoutSettings dpSettings = _presenter.FieldLayoutSettingsIfAllocated;
					if ( null != dpSettings )
					{
						ret = dpSettings.RecordSeparatorLocation;
						if ( ret.HasValue )
							return ret.Value;
					}
				}

				return ret ?? RecordSeparatorLocation.FixedRecords;
			}
		}

				#endregion // RecordSeparatorLocationResolved

                // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
                #region RootFieldLayout

        internal FieldLayout RootFieldLayout
        {
            get
            {
                FieldLayout parentLayout = this.ParentFieldLayout;

                if (parentLayout != null)
                    return parentLayout.RootFieldLayout;

                return this;
            }
        }

                #endregion //RootFieldLayout	

				#region SortEvaluationModeResolvedDefault

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal SortEvaluationMode SortEvaluationModeResolvedDefault
		{
			get
			{
				if ( !_cachedSortEvaluationModeResolved.HasValue )
				{
					_cachedSortEvaluationModeResolved = this.GetResolvedValue<SortEvaluationMode>(
						FieldLayoutSettings.SortEvaluationModeProperty,
						SortEvaluationMode.Default,
						SortEvaluationMode.Auto
					);
				}

				return _cachedSortEvaluationModeResolved.Value;
			}
		} 

				#endregion // SortEvaluationModeResolvedDefault
    
                #region SortVersion

        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping
        // Made public so RecordManager could use a PropertyValueTracker 
        //internal int SortVersion
        /// <summary>
        /// For internal use only
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable( false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
        public int SortVersion
		{
			get
			{
				return this._sortVersion;
			}
		}

                #endregion //SortVersion

				// JM 11-13-07 BR27986
				#region SortOperationVersion

		internal static readonly DependencyProperty SortOperationVersionProperty = DependencyProperty.Register("SortOperationVersion",
			typeof(int), typeof(FieldLayout), new FrameworkPropertyMetadata(0));

		internal int SortOperationVersion
		{
			get
			{
				return (int)this.GetValue(FieldLayout.SortOperationVersionProperty);
			}
			set
			{
				this.SetValue(FieldLayout.SortOperationVersionProperty, value);
			}
		}

				#endregion //SortOperationVersion

				#region SpecialRecordsVersion
		
		// SSP 4/14/08 - Summaries Functionality
		// 

		/// <summary>
		/// Identifies the <see cref="SpecialRecordsVersion"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty SpecialRecordsVersionProperty = DependencyProperty.Register(
				"SpecialRecordsVersion",
				typeof( int ),
				typeof( FieldLayout ),
				new FrameworkPropertyMetadata( 1, new PropertyChangedCallback( OnSpecialRecordsVersionChanged ) )
			);

		/// <summary>
		/// This version number is incremented whenever any settings that affect special records
		/// in viewable records collection. For example, when a summary is added/removed that can
		/// potentially cause summary record to be added/removed from the viewable record 
		/// collection, this version number will be incremented. Viewable record collection will
		/// use it for verification.
		/// </summary>
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		internal int SpecialRecordsVersion
		{
			get
			{
				return _cachedSpecialRecordsVersion;
			}
			set
			{
				this.SetValue( SpecialRecordsVersionProperty, value );
			}
		}

		private static void OnSpecialRecordsVersionChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			FieldLayout fieldLayout = (FieldLayout)dependencyObject;
			fieldLayout._cachedSpecialRecordsVersion = (int)e.NewValue;
		}

				#endregion // SpecialRecordsVersion

				#region SummaryDescriptionVisibilityResolved

		
		
		internal Visibility SummaryDescriptionVisibilityResolved
		{
			get
			{
				Visibility? ret;

				if ( null != _settings )
				{
					ret = _settings.SummaryDescriptionVisibility;
					if ( ret.HasValue )
						return ret.Value;
				}

				if ( null != _presenter )
				{
					FieldLayoutSettings dpSettings = _presenter.FieldLayoutSettingsIfAllocated;
					if ( null != dpSettings )
					{
						ret = dpSettings.SummaryDescriptionVisibility;
						if ( ret.HasValue )
							return ret.Value;
					}
				}

				return Visibility.Collapsed;
			}
		}

				#endregion // SummaryDescriptionVisibilityResolved

				#region SummaryEvaluationModeResolved

		// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal SummaryEvaluationMode SummaryEvaluationModeResolved
		{
			get
			{
				if ( !_cachedSummaryEvaluationModeResolved.HasValue )
				{
					_cachedSummaryEvaluationModeResolved = this.GetResolvedValue<SummaryEvaluationMode>(
						FieldLayoutSettings.SummaryEvaluationModeProperty,
						SummaryEvaluationMode.Default,
						SummaryEvaluationMode.Auto
					);
				}

				return _cachedSummaryEvaluationModeResolved.Value;
			}
		}

				#endregion // SummaryEvaluationModeResolved

				#region SupportDataErrorInfoResolved

		
		
		internal SupportDataErrorInfo SupportDataErrorInfoResolved
		{
			get
			{
				SupportDataErrorInfo ret = this.SupportDataErrorInfoResolvedDefault;
				if ( SupportDataErrorInfo.Default != ret )
					return ret;

				return SupportDataErrorInfo.None;
			}
		}

				#endregion // SupportDataErrorInfoResolved

				#region SupportDataErrorInfoResolvedDefault

		
		
		internal SupportDataErrorInfo SupportDataErrorInfoResolvedDefault
		{
			get
			{
                
                
                
                
                if ( !_cachedSupportDataErrorInfoResolvedDefault.HasValue )
                    _cachedSupportDataErrorInfoResolvedDefault = this.GetResolvedValue<SupportDataErrorInfo>( 
                        FieldLayoutSettings.SupportDataErrorInfoProperty, SupportDataErrorInfo.Default, SupportDataErrorInfo.Default );

                return _cachedSupportDataErrorInfoResolvedDefault.Value;

                
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

                
			}
		}

				#endregion // SupportDataErrorInfoResolvedDefault

				#region StyleGenerator

		internal FieldLayoutTemplateGenerator StyleGenerator { get { return this._styleGenerator; } }

				#endregion //StyleGenerator	

				#region TemplateDataRecord





		internal DataRecord TemplateDataRecord
		{
			get
			{
				if (null == this._templateDataRecord)
					// SSP 12/17/08 - NAS9.1 Record Filtering
					// Changed the method to take in out param for returning the data record instead of returning it.
					// We need to initialize FieldLayout's _templateDataRecord member variable before call to 
					// templateDataRecords.AddRecord in this method to prevent recursion.
					// 
					//this._templateDataRecord = DataRecord.CreateTemplateDataRecord(this);
					DataRecord.CreateTemplateDataRecord( this, out _templateDataRecord );

				return this._templateDataRecord;
			}
		} 
				#endregion //TemplateDataRecord

				// AS 8/25/09 TFS17560
				#region TemplateGroupByRecord





		internal GroupByRecord TemplateGroupByRecord
		{
			get
			{
				if (null == this._templateGroupByRecord && _presenter != null)
				{
                    RecordCollectionBase rcds = _presenter.FieldLayouts.TemplateGroupByRecords;
					_templateGroupByRecord = new GroupByRecord(this, rcds);
					_templateGroupByRecord.SetIsExpanded(true, null);

					// JJD 9/29/11 - TFS87297
					// Call BeginUpdate to prevent a recursive verification of the scroll counts
					rcds.BeginUpdate();

                    // JJD 9/22/09 - TFS18162 
                    // Add the template groupby rcd to the collection
                    rcds.AddRecord(_templateGroupByRecord);

					// JJD 9/29/11 - TFS87297
					// Call EndUpdate to match the call to BeginUpdate above
					rcds.EndUpdate(true);
                }

				return this._templateGroupByRecord;
			}
		} 
				#endregion //TemplateGroupByRecord

				#region TemplateDataRecordCache
		internal TemplateDataRecordCache TemplateDataRecordCache
		{
			get
			{
				if (null == this._templateDataRecordCache)
					this._templateDataRecordCache = new TemplateDataRecordCache(this);

				return this._templateDataRecordCache;
			}
		} 
				#endregion //TemplateDataRecordCache

				#region TemplateVersion

        // AS 2/10/09
        // I had removed this since the virtualizing cell panel wasn't using it but we 
        // do need to notify the virtualizing panel when the template cache has been 
        // cleared.
        //
        internal static readonly DependencyProperty TemplateVersionProperty = DependencyProperty.Register("TemplateVersion",
            typeof(int), typeof(FieldLayout), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnTemplateVersion)));

        private static void OnTemplateVersion(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FieldLayout fl = (FieldLayout)d;

            fl._templateVersion = (int)e.NewValue;

			// AS 7/7/09 TFS19145
			// This will now be handled by the BumpTemplateVersion method so we can 
			// selectively release the entire cache.
			//
            //if (null != fl._templateDataRecordCache)
            //    fl._templateDataRecordCache.ReleaseCache();
        }

        private int _templateVersion;

        internal int TemplateVersion
        {
            get
            {
                return _templateVersion;
            }
			// AS 7/7/09 TFS19145
			// Removed so we have to use the BumpTemplateVersion method.
			//
            //set
            //{
            //    this.SetValue(FieldLayout.TemplateVersionProperty, value);
            //}
        }
                #endregion //TemplateVersion

				#region UseCellPresenters

		internal bool UseCellPresenters
		{
			get
			{
				if (this._styleGenerator != null)
					return this._styleGenerator.UseCellPresenters;

				return !this.HasSeparateHeader && this.LabelLocationResolved == LabelLocation.InCells;
			}
		}

				#endregion //UseCellPresenters	

				// JJD 08/17/12 - TFS119037 - added
				#region WasRemovedFromCollection

		internal bool WasRemovedFromCollection
		{
			get
			{
				if (_wasRemovedFromCollection)
				{
					// if it was re-added to the colleciton then clear the flag
					if (this.Index >= 0)
						_wasRemovedFromCollection = false;
				}

				return _wasRemovedFromCollection;
			}
		}

				#endregion //WasRemovedFromCollection	
    
                // JJD 12/13/07
                // Added a flag so we know if this layout was initialized with an EnumerableObjectWrapper
                #region WrapsEnumerableObject

        internal bool WrapsEnumerableObject { get { return this._wrapsEnumerableObject; } }

                #endregion //WrapsEnumerableObject	
        
			#endregion // Internal Properties

			#region Private Properties

				#region HeaderPresenterCache

		private List<HeaderPresenter> HeaderPresenterCache
		{
			get
			{
				if (this._headerPresenterCache == null)
					this._headerPresenterCache = new List<HeaderPresenter>(3);

				return this._headerPresenterCache;
			}
		}

				#endregion //HeaderPresenterCache

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

                // JJD 2/18/09 - TFS14057 - added
				#region BumpGridColumnWidthVersion

        internal void BumpRecordFilterVersion()
		{
            // This is a way to notify the ResolvedRecordFilterCollection that it needs to
            // re-evaluate whether to use FieldLayout's RecordFilters or RecordManager's 
            // RecordFilters. We are taking this approach because there's no public 
            // RecordFilterScopeResolved that ResolvedRecordFilterCollection can hook 
            // into to detect change in this property's setting.
            // 
            if (null != this._recordFilters)
                this._recordFilters.BumpVersion();
        } 

				#endregion //BumpRecordFilterVersion

				#region BumpGridColumnWidthVersion

		internal void BumpGridColumnWidthVersion()
		{
			this.GridColumnWidthVersion++;
		} 
				#endregion //BumpGridColumnWidthVersion

				#region BumpGridRowHeightVersion
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //BumpGridRowHeightVersion

				#region BumpGroupByVersion

		internal void BumpGroupByVersion()
        {
            this._groupByVersion++;

            this.RaisePropertyChangedEvent("GroupByVersion");

            if (this._presenter != null)
            {
				// MD 3/17/11 - TFS34785
				// Call the new overload and pass False for the regenerateTemplates value so we can reuse elements.
                //this._presenter.InvalidateGeneratedStyles(true, false);
				this._presenter.InvalidateGeneratedStyles(true, false, false);

                this._presenter.InvalidateItemsHost();

				// AS 9/10/09 TFS21581
				// Moved into a common helper routine for bumping the record version 
				// asynchronously.
				//
				//// JJD 6/2/09 - TFS17867
				//// If we are not in a report control then bump
				//// the RecordIndentVersion asynchronously to handle
				//// group by support in child record islands
				////
				//// MBS 7/29/09 - NA9.2 Excel Exporting
				////if (this._presenter.IsReportControl)
				//if(this._presenter.IsSynchronousControl)
				//    this.BumpRecordIndentVersion();
				//else
				//{
				//    if (this._bumpRecordIndentVersionPending == null)
				//        this._bumpRecordIndentVersionPending = this._presenter.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(this.BumpRecordIndentVersion));
				//}
				this.BumpRecordIndentVersionAsync();
            }
        }

                #endregion //BumpGroupByVersion	

                // JJD 1/19/09 - NA 2009 vol 1 - record filtering
                #region BumpRecordIndentVersion

        internal void BumpRecordIndentVersion()
        {
            // JJD 6/2/09 - TFS17867
            // clear the pending operation
            this._bumpRecordIndentVersionPending = null;

            this._indentOffsetVersion++;

            this.RaisePropertyChangedEvent("RecordIndentVersion");

			// AS 10/21/11 TFS21581
			// While running some unit tests Joe found this one was failing. It started failing 
			// I believe when we started asynchronously processing (and batching) the Invalidate
			// Meassure calls. In any case what is happening is that the RecordIndentVersion is 
			// bumped but the VDRCP doesn't know so it's layout is out of sync. So we'll just 
			// dirty it when the record indent changes.
			//
			this.BumpGridColumnWidthVersion();
        }

                #endregion //BumpRecordIndentVersion

				// AS 9/10/09 TFS21581
				#region BumpRecordIndentVersionAsync
		internal void BumpRecordIndentVersionAsync()
		{
			// AS 10/21/11 TFS21581
			// This method could be called before the FieldLayout is associated with a DataPresenter.
			//
			if (_presenter == null)
				return;

			// JJD 6/2/09 - TFS17867
			// If we are not in a report control then bump
			// the RecordIndentVersion asynchronously to handle
			// group by support in child record islands
			//
			// MBS 7/29/09 - NA9.2 Excel Exporting
			//if (this._presenter.IsReportControl)
			if (this._presenter.IsSynchronousControl)
				this.BumpRecordIndentVersion();
			else
			{
				if (this._bumpRecordIndentVersionPending == null)
					this._bumpRecordIndentVersionPending = this._presenter.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(this.BumpRecordIndentVersion));
			}
		} 
				#endregion //BumpRecordIndentVersionAsync

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
                #region BumpLayoutItemVersion
		internal void BumpLayoutItemVersion()
		{
			// AS 11/29/10 TFS60418
			this.BumpLayoutItemVersion(true);
		}

		// AS 11/29/10 TFS60418
		// Added an overload so the field could cause a change notification that would
		// update the layout without making every field consider its cached information 
		// was dirty.
		//
		internal void BumpLayoutItemVersion(bool bumpFieldLayoutVersion)
        {
            // AS 2/20/09 TFS7941
            if (null != _bumpLayoutItemOp)
            {
                if (_bumpLayoutItemOp.Status == DispatcherOperationStatus.Pending)
                    _bumpLayoutItemOp.Abort();

                _bumpLayoutItemOp = null;
            }

			// AS 11/29/10 TFS60418
			// Added if block to only bump the gross version (i.e. indicate that 
			// all fields have been dirtied).
			//
			if (bumpFieldLayoutVersion)
	            this._layoutItemVersion++;

            this.BumpGridColumnWidthVersion();
            // AS 1/5/09
            //this.BumpGridRowHeightVersion();

			// SSP 2/2/10
			// Added CellsInViewChanged event to the DataPresenterBase.
			// 
			if ( null != _presenter )
				_presenter.RaiseCellsInViewChangedAsyncHelper( );
        } 
                #endregion //BumpLayoutItemVersion

                // AS 2/20/09 TFS7941
                #region BumpLayoutItemVersionAsync
        internal void BumpLayoutItemVersionAsync()
        {
            if (null == _bumpLayoutItemOp)
            {
                _bumpLayoutItemOp = this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(BumpLayoutItemVersion));
            }
        } 
                #endregion //BumpLayoutItemVersionAsync

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
                #region BumpLayoutManagerVersion
        internal void BumpLayoutManagerVersion()
        {
			// MD 8/13/10
			// Added a way to bypass the FieldLayout.BumpLayoutManagerVersionRequired() logic.
			if (this.PreventBumpLayoutManagerVersionRequired)
				return;

            this._layoutManagerVersion++;
            this.BumpLayoutItemVersion();
        } 
                #endregion //BumpLayoutManagerVersion

                #region BumpSortVersion

        internal void BumpSortVersion()
        {
			this._sortVersion++;

			
			
			if ( null != _summaryDefinitions )
				_summaryDefinitions.RefreshSummariesAffectedBySort( );

            this.RaisePropertyChangedEvent("SortVersion");

            if (this.DataPresenter != null)
                this.DataPresenter.OnSortCriteriaChanged(this);
		}

                #endregion //BumpSortVersion	

				// JM 11-13-07 BR27986
				#region BumpSortOperationVersion

		internal void BumpSortOperationVersion()
		{
			this.SortOperationVersion++;

            
            
            
            
		}

				#endregion //BumpSortOperationVersion

				#region BumpSpecialRecordsVersion

		
		
		internal void BumpSpecialRecordsVersion( )
		{
			this.SpecialRecordsVersion++;
		}
				
				#endregion // BumpSpecialRecordsVersion

				// AS 7/7/09 TFS19145
				#region BumpTemplateVersion
		internal void BumpTemplateVersion(bool releaseTemplateCache)
		{
			this.SetValue(TemplateVersionProperty, _templateVersion + 1);

			// selectively release the entire cache or just bump the cache version
			// so the cell placeholders can be fixed up/reverified
			this.TemplateDataRecordCache.BumpCacheVersion(releaseTemplateCache);
		} 
				#endregion //BumpTemplateVersion

                // JJD 09/29/08 - added
                #region CloneDragFieldLayoutInfo


        // Clone the DragFieldLayoutInfo in case the user moved the fields around
        internal void CloneDragFieldLayoutInfo(FieldLayout clonedLayout)
        {
            if (this._fieldLayoutInfo == null ||
				 this._fieldLayoutInfo.Count == 0 )
                return;

            FieldCollection fields = this.Fields;
            FieldCollection clonedFields = clonedLayout.Fields;

            int count = fields.Count;

            Debug.Assert(clonedFields.Count == count, "Cloned layout field counts doesn't match");

            if (count != clonedFields.Count)
                return;

			clonedLayout._fieldLayoutInfo = new LayoutInfo( clonedLayout );

            for (int i = 0; i < count; i++)
            {
                Field field = fields[i];

				if ( this._fieldLayoutInfo.ContainsKey( field ) )
                {
                    Field clonedField = clonedFields[i];

                    Debug.Assert(field.Name == clonedField.Name, "field collections should match");

					clonedLayout._fieldLayoutInfo.Add( clonedField, this._fieldLayoutInfo[field].Clone( ) );
                }
            }
        }

                #endregion //CloneDragFieldLayoutInfo	
    
                #region ClearCustomizations

		
		
		/// <summary>
		/// Clears current user customizations based on the customizations parameter.
		/// </summary>
		/// <param name="customizations">Customizations to clear.</param>
		/// <seealso cref="CustomizationType"/>
		/// <seealso cref="DataPresenterBase.SaveCustomizations()"/>
		/// <seealso cref="DataPresenterBase.LoadCustomizations(string)"/>
		internal void ClearCustomizations( CustomizationType customizations )
		{
			bool invalidateGeneratedStyles = false;

			// Clear Field specific customizations.
			// 
			FieldCollection fields = _fields;
			if ( null != fields )
			{
				if ( 0 != ( CustomizationType.FieldExtent & customizations ) )
				{
				    foreach ( Field field in fields )
				    {
					    field.ClearUserResize( );
					    invalidateGeneratedStyles = true;
				    }
                }
			}

			if ( 0 != ( CustomizationType.FieldPosition & customizations ) )
			{

				// JM 01-21-09 NA 9.1 Fixed Fields
                if (null != fields)
                {
					foreach ( Field field in fields )
					{
						field.FixedLocation = FixedFieldLocation.Scrollable;

						// SSP 6/23/09 - NAS9.2 Field Chooser
						// This is used by the field chooser to force a field to be visible even if it's
						// a group-by field and CellVisibilityWhenGrouped is set to a value other than Visible.
						// 
						field.IgnoreFieldVisibilityOverrides = false;
					}
                }

                // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
                // When setting the fixed location above, the following flag
                // may have been set to indicate that we need to create a 
                // snapshot when the layout is generated.
                //
                _createDragFieldLayout = false;

				_fieldLayoutInfo = null;
				
				// SSP 2/23/10 - TFS25122 TFS28016
				// Also null out auto-generated positions. Otherwise collapsed fields will assume those
				// positions once they are made visible after a drag-and-drop operation.
				// 
				_autoGeneratedPositions = null;

				invalidateGeneratedStyles = true;

			}

			if ( 0 != ( CustomizationType.GroupingAndSorting & customizations ) )
			{
				if ( null != _sortedFields )
					_sortedFields.Clear( );
			}

			// SSP 2/4/09 - NAS9.1 Record Filtering
			// 
			if ( 0 != ( CustomizationType.RecordFilters & customizations ) )
			{
				if ( null != _recordFilters )
					_recordFilters.Clear( );
			}

			// SSP 9/8/09 TFS18172
			// 
			if ( 0 != ( CustomizationType.Summaries & customizations ) )
			{
				if ( null != _summaryDefinitions )
					_summaryDefinitions.Clear( );
			}

			// AS 6/4/09 NA 2009.2 Undo/Redo
			if (null != _presenter)
				_presenter.History.OnCustomizationsChanged(this, customizations);

            if (invalidateGeneratedStyles && null != _presenter)
            {
                // JJD 11/12/08 - TFS7858
                // Call the fieldlayout's PostDelayedInvalidation instead so we only invalidate
                // the styles if we aren't in the the middle of initializing all the fields 
                //_presenter.InvalidateGeneratedStylesAsync();
                this.PostDelayedInvalidation();
            }
		}

				#endregion // ClearCustomizations

				#region CreateSizeAreaManager
        
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

				#endregion //CreateSizeAreaManager

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region DirtyFixedState
        internal void DirtyFixedState()
        {
            this._fixedFieldVersion++;

            // the splitter elements may need to be included or not
            this.BumpLayoutManagerVersion();
        }
                #endregion //DirtyFixedState
        
                // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                #region DoesParentFieldMatch

        internal bool DoesParentFieldMatch(Field fieldToTest)
        {
            if (fieldToTest != null)
            {
                if (fieldToTest.Owner == this)
                    return false;

                return fieldToTest == this.ParentField;
            }
            else
            {
                if (this._parentFieldLayout != null)
                    return false;

                return this.ParentFieldLayoutKey == null;
            }
        }

                #endregion //DoesParentFieldMatch	
        
                // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                #region DoesParentFieldLayoutMatch

        internal bool DoesParentFieldLayoutMatch(FieldLayout layoutToTest)
        {
            if (layoutToTest != null)
            {
                if (layoutToTest == this)
                    return false;

                return layoutToTest == this.ParentFieldLayout;
            }
            else
            {
                if (this._parentFieldLayout != null)
                    return false;

                return this.ParentFieldLayoutKey == null;
            }
        }

                #endregion //DoesParentFieldLayoutMatch	
    
				#region DoesProviderMatchKeyByName

		internal bool DoesProviderMatchKeyByName(PropertyDescriptorProvider propertyDescriptorProvider)
		{
			string strKey = this.Key as string;

			if (strKey == null ||
				strKey.Length == 0 ||
				propertyDescriptorProvider == null)
				return false;

			string fpName = propertyDescriptorProvider.Name;

			if (fpName == null)
				return false;

			// do a case sensitive compare
			if ( fpName != strKey )
				return false;

			return true;
		}

				#endregion //DoesProviderMatchKeyExactly

				#region DoesProviderMatchKeyExactly

		internal bool DoesProviderMatchKeyExactly(PropertyDescriptorProvider propertyDescriptorProvider)
		{
			object key = this.Key;

			if (key == null ||
				propertyDescriptorProvider == null)
				return false;

			if (key == propertyDescriptorProvider.Source)
				return true;

			XmlNode node1 = key as XmlNode;
			XmlNode node2 = propertyDescriptorProvider.Source as XmlNode;
			if (node1 != null && node2 != null)
			{
				if (node1.SchemaInfo != null ||
					 node2.SchemaInfo != null)
				{
					if (node1.SchemaInfo == node2.SchemaInfo)
						return true;
				}

				return node1.LocalName == node2.LocalName;
			}

			return false;
		}

				#endregion //DoesProviderMatchKeyExactly

                // MBS 7/30/09 - NA9.2 Excel Exporting
                #region EnsureStyleGeneratorInitialized

                internal void EnsureStyleGeneratorInitialized()
                {
                    if (this.StyleGenerator == null)
                        this.Initialize(this.DataPresenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator(this));
                }
                #endregion //EnsureStyleGeneratorInitialized

				#region GetCellAreaColumnDefinitions

		// Returns the column definitions used for this column when
		// the template was last generated 
		internal ColumnDefinition[] GetCellAreaColumnDefinitions()
		{
			if (this._styleGenerator == null)
				return new ColumnDefinition[0];

			return this._styleGenerator.GetCellAreaColumnDefinitions();
		}

				#endregion //GetCellAreaColumnDefinitions
	
				#region GetCellAreaRowDefinitions

		// Returns the row definitions used for this column when
		// the template was last generated 
		internal RowDefinition[] GetCellAreaRowDefinitions()
		{
			if (this._styleGenerator == null)
				return new RowDefinition[0];

			return this._styleGenerator.GetCellAreaRowDefinitions();
		}

				#endregion //GetCellAreaRowDefinitions	
    
                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
				#region GetDefaultAllowFieldFixing

        internal AllowFieldFixing GetDefaultAllowFieldFixing(Field field)
		{
            AllowFieldFixing ret = AllowFieldFixing.Default;

			if ( null != _cachedFieldSettings )
				ret = _cachedFieldSettings.AllowFixing;

            if (AllowFieldFixing.Default == ret && null != _presenter)
            {
                FieldSettings dpSettings = _presenter.FieldSettingsIfAllocated;

                if (null != dpSettings)
                    ret = dpSettings.AllowFixing;
            }

			return ret;
		}

				#endregion // GetDefaultAllowFieldFixing

                #region GetDefaultAllowGroupBy

        // JJD 8/19/09 - NA 2009 Vol 2 - Cross Band grouping
        
        // added default value
        //internal bool GetDefaultAllowGroupBy(Field field )
        internal bool GetDefaultAllowGroupBy(Field field, bool? defaultValue )
        {
            Nullable<bool> allowGroupBy = new Nullable<bool>();

            if (this._cachedFieldSettings != null)
            {
                allowGroupBy = this._cachedFieldSettings.AllowGroupBy;

                if (allowGroupBy.HasValue)
                    return allowGroupBy.Value;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    allowGroupBy = this._presenter.FieldSettings.AllowGroupBy;

                    if (allowGroupBy.HasValue)
                        return allowGroupBy.Value;
                }
            }

            // JJD 8/19/09 - NA 2009 Vol 2 - Cross Band grouping
            
            // if default value was passed in then return it
            if (defaultValue.HasValue)
                return defaultValue.Value;

            if (field != null)
            {
                // the a specific sort comparer isn't suplied for this field
                // and its data type doesn't support IComparable then return No
                if ( ( field.HasSettings == false || field.Settings.SortComparer == null ) &&
					// SSP 5/23/07
					// Take into account the Nullable types.
					// 
                    //!typeof(IComparable).IsAssignableFrom( field.DataType ) 
					!typeof( IComparable ).IsAssignableFrom( field.DataTypeUnderlying ) 
					)
                    return false;
            }

            return true;
        }

                #endregion //GetDefaultAllowGroupBy
    
                #region GetDefaultAllowResize

        internal bool GetDefaultAllowResize(Field field )
        {
            Nullable<bool> allowResize = new Nullable<bool>();

            if (this._cachedFieldSettings != null)
            {
                allowResize = this._cachedFieldSettings.AllowResize;

                if (allowResize.HasValue)
                    return allowResize.Value;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    allowResize = this._presenter.FieldSettings.AllowResize;

                    if (allowResize.HasValue)
                        return allowResize.Value;
                }
			}

            return true;
        }

                #endregion //GetDefaultAllowResize

				#region GetDefaultAllowSummaries

		
		
		internal bool GetDefaultAllowSummaries( Field field )
		{
			bool? ret;

			SummaryUIType summaryUIType = field.SummaryUITypeResolved;
			if ( SummaryUIType.MultiSelectForNumericsOnly == summaryUIType
				|| SummaryUIType.SingleSelectForNumericsOnly == summaryUIType )
			{
				
				
				
				
				if ( ! field.IsEditAsTypeNumeric )
					return false;
				




				
			}

			if ( null != _cachedFieldSettings )
			{
				ret = _cachedFieldSettings.AllowSummaries;

				if ( ret.HasValue )
					return ret.Value;
			}

			FieldSettings dpSettings = null != _presenter ? _presenter.FieldSettingsIfAllocated : null;
			if ( null != dpSettings )
			{
				ret = dpSettings.AllowSummaries;

				if ( ret.HasValue )
					return ret.Value;
			}

			return false;
		}

				#endregion // GetDefaultAllowSummaries

				#region GetDefaultCellClickAction






        internal CellClickAction GetDefaultCellClickAction(Field field )
        {
            CellClickAction clickAction = CellClickAction.Default;

            if (this._cachedFieldSettings != null)
            {
                clickAction = this._cachedFieldSettings.CellClickAction;

                if (clickAction != CellClickAction.Default)
                    return clickAction;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    clickAction = this._presenter.FieldSettings.CellClickAction;

                    if (clickAction != CellClickAction.Default)
                        return clickAction;
                }

				// Use the DefaultCellClickAction from the view if it is not set to default.
				if (this._presenter.CurrentViewInternal.DefaultCellClickAction != CellClickAction.Default)
					return this._presenter.CurrentViewInternal.DefaultCellClickAction;
            }

            return Field.DefaultCellClickAction;
        }

                #endregion //GetDefaultCellClickAction

                #region GetDefaultCellContentAlignment






        internal CellContentAlignment GetDefaultCellContentAlignment(Field field )
        {
            CellContentAlignment position = CellContentAlignment.Default;

            if (this._cachedFieldSettings != null)
            {
                position = this._cachedFieldSettings.CellContentAlignment;

                if (position != CellContentAlignment.Default)
                    return position;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    position = this._presenter.FieldSettings.CellContentAlignment;

                    if (position != CellContentAlignment.Default)
                        return position;
                }

				// JJD 11/21/11 - TFS26381
				// Instead of relying on the explicit setting at the dp level (which ignores
				// an override for this field layout) use LabelLocationResolved which
				// takes settings at both levels into account.
				//if (this._presenter.HasFieldLayoutSettings &&
				//    this._presenter.FieldLayoutSettings.LabelLocation == LabelLocation.Hidden)
                if (this.LabelLocationResolved == LabelLocation.Hidden)
                    return CellContentAlignment.ValueOnly;

				// JM NA 10.1 CardView
				// Use the DefaultCellContentAlignment from the view if it is not set to default.
				if (this._presenter.CurrentViewInternal.DefaultCellContentAlignment != CellContentAlignment.Default)
					return this._presenter.CurrentViewInternal.DefaultCellContentAlignment;
			}

            if (this._styleGenerator != null)
            {
                bool isPrimary = field != null && field.IsPrimary;
                if (isPrimary)
                    return this._styleGenerator.PrimaryFieldDefaultCellContentAlignment;
                else
                    return this._styleGenerator.DefaultCellContentAlignment;
            }

            return CellContentAlignment.LabelAboveValueStretch;
        }

                #endregion //GetDefaultCellContentAlignment

                #region GetDefaultExpandableFieldRecordExpansionMode






        internal ExpandableFieldRecordExpansionMode GetDefaultExpandableFieldRecordExpansionMode(Field field )
        {
            ExpandableFieldRecordExpansionMode displayMode = ExpandableFieldRecordExpansionMode.Default;

            if (this._cachedFieldSettings != null)
            {
                displayMode = this._cachedFieldSettings.ExpandableFieldRecordExpansionMode;

                if (displayMode != ExpandableFieldRecordExpansionMode.Default)
                    return displayMode;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    displayMode = this._presenter.FieldSettings.ExpandableFieldRecordExpansionMode;

                    if (displayMode != ExpandableFieldRecordExpansionMode.Default)
                        return displayMode;
                }
            }

            return ExpandableFieldRecordExpansionMode.ShowExpansionIndicatorIfSiblingsExist;
        }

                #endregion //GetDefaultExpandableFieldRecordExpansionMode

                #region GetDefaultExpandableFieldRecordHeaderDisplayMode






        internal ExpandableFieldRecordHeaderDisplayMode GetDefaultExpandableFieldRecordHeaderDisplayMode(Field field )
        {
            ExpandableFieldRecordHeaderDisplayMode displayMode = ExpandableFieldRecordHeaderDisplayMode.Default;

            if (this._cachedFieldSettings != null)
            {
                displayMode = this._cachedFieldSettings.ExpandableFieldRecordHeaderDisplayMode;

                if (displayMode != ExpandableFieldRecordHeaderDisplayMode.Default)
                    return displayMode;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    displayMode = this._presenter.FieldSettings.ExpandableFieldRecordHeaderDisplayMode;

                    if (displayMode != ExpandableFieldRecordHeaderDisplayMode.Default)
                        return displayMode;
                }
            }

			if ( this.Fields.ExpandableFieldsCount < 2 )
				return ExpandableFieldRecordHeaderDisplayMode.NeverDisplayHeader;
			else
				return ExpandableFieldRecordHeaderDisplayMode.AlwaysDisplayHeader;
        }

                #endregion //GetDefaultExpandableFieldRecordHeaderDisplayMode

                #region GetDefaultLabelClickAction






        internal LabelClickAction GetDefaultLabelClickAction(Field field )
        {
            LabelClickAction clickAction = LabelClickAction.Default;

            if (this._cachedFieldSettings != null)
            {
                clickAction = this._cachedFieldSettings.LabelClickAction;

                if (clickAction != LabelClickAction.Default)
                    return clickAction;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    clickAction = this._presenter.FieldSettings.LabelClickAction;

                    if (clickAction != LabelClickAction.Default)
                        return clickAction;
                }
            }

            // the a specific sort comparer isn't suplied for this field
            // and its data type doesn't support IComparable then return No
            if (field != null &&
                (field.HasSettings == false || field.Settings.SortComparer == null) &&
				// SSP 5/23/07
				// Take into account the Nullable types.
				// 
                //!typeof(IComparable).IsAssignableFrom(field.DataType)
				!typeof( IComparable ).IsAssignableFrom( field.DataTypeUnderlying )
				)
                return LabelClickAction.Nothing;

			// Use the DefaultLabelClickAction from the View if it is not set to default.
			if (this._presenter != null && 
				this._presenter.CurrentViewInternal.DefaultLabelClickAction != LabelClickAction.Default)
				return this._presenter.CurrentViewInternal.DefaultLabelClickAction;

            return Field.DefaultLabelClickAction;
        }

                #endregion //GetDefaultLabelClickAction

                #region GetDefaultLayoutDisplayMode

		// JJD 1/25/07 
		// Since we pulled the XamDataPresenter control we decided to makr this property internal
//#if DEBUG
//        /// <summary>
//        /// Gets the resolved LayoutDisplayMode setting for this field (read-only)
//        /// </summary>
//#endif
//        internal FieldLayoutDisplayMode GetDefaultLayoutDisplayMode(Field field )
//        {
//            FieldLayoutDisplayMode displayMode = FieldLayoutDisplayMode.Default;

//            if (this._cachedFieldSettings != null)
//            {
//                displayMode = this._cachedFieldSettings.LayoutDisplayMode;

//                if (displayMode != FieldLayoutDisplayMode.Default)
//                    return displayMode;
//            }

//            if (this._presenter != null)
//            {
//                if (this._presenter.HasFieldSettings)
//                {
//                    displayMode = this._presenter.FieldSettings.LayoutDisplayMode;

//                    if (displayMode != FieldLayoutDisplayMode.Default)
//                        return displayMode;
//                }
//            }

//            return Field.DefaultLayoutDisplayMode;
//        }

                #endregion //GetDefaultLayoutDisplayMode
    
                // JJD 2/7/08 - BR30444 - added
                #region GetDefaultLabelTextAlignment

        internal TextAlignment GetDefaultLabelTextAlignment(Field field )
        {
            Nullable<TextAlignment> textAlignment = new Nullable<TextAlignment>();

            if (this._cachedFieldSettings != null)
            {
                textAlignment = this._cachedFieldSettings.LabelTextAlignment;

                if (textAlignment.HasValue)
                    return textAlignment.Value;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    textAlignment = this._presenter.FieldSettings.LabelTextAlignment;

                    if (textAlignment.HasValue)
                        return textAlignment.Value;
                }
			}

            return TextAlignment.Left;
        }

                #endregion //GetDefaultLabelTextAlignment
    
                // JJD 2/7/08 - BR30444 - added
                #region GetDefaultLabelTextTrimming

        internal TextTrimming GetDefaultLabelTextTrimming(Field field )
        {
            Nullable<TextTrimming> textTrimming = new Nullable<TextTrimming>();

            if (this._cachedFieldSettings != null)
            {
                textTrimming = this._cachedFieldSettings.LabelTextTrimming;

                if (textTrimming.HasValue)
                    return textTrimming.Value;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    textTrimming = this._presenter.FieldSettings.LabelTextTrimming;

                    if (textTrimming.HasValue)
                        return textTrimming.Value;
                }
			}

            return TextTrimming.None;
        }

                #endregion //GetDefaultLabelTextTrimming
    
                // JJD 2/7/08 - BR30444 - added
                #region GetDefaultLabelTextWrapping

        internal TextWrapping GetDefaultLabelTextWrapping(Field field )
        {
            Nullable<TextWrapping> textWrapping = new Nullable<TextWrapping>();

            if (this._cachedFieldSettings != null)
            {
                textWrapping = this._cachedFieldSettings.LabelTextWrapping;

                if (textWrapping.HasValue)
                    return textWrapping.Value;
            }

            if (this._presenter != null)
            {
                if (this._presenter.HasFieldSettings)
                {
                    textWrapping = this._presenter.FieldSettings.LabelTextWrapping;

                    if (textWrapping.HasValue)
                        return textWrapping.Value;
                }

                // JJD 9/20/08 - for printing resolve to wrapping.
                if (this._presenter.IsReportControl)
                    return TextWrapping.Wrap;
			}

            return TextWrapping.NoWrap;
        }

                #endregion //GetDefaultLabelTextWrapping

				#region GetDefaultSummaryUIType

		
		
		internal SummaryUIType GetDefaultSummaryUIType( Field field )
		{
			SummaryUIType ret;

			if ( null != _cachedFieldSettings )
			{
				ret = _cachedFieldSettings.SummaryUIType;

				if ( SummaryUIType.Default != ret )
					return ret;
			}

			FieldSettings dpSettings = null != _presenter ? _presenter.FieldSettingsIfAllocated : null;
			if ( null != dpSettings )
			{
				ret = dpSettings.SummaryUIType;

				if ( SummaryUIType.Default != ret )
					return ret;
			}

			return SummaryUIType.MultiSelectForNumericsOnly;
		}

				#endregion // GetDefaultSummaryUIType

				#region GetFieldLayoutInfo

		// SSP 6/26/09 - NAS9.2 Field Chooser
		// Made _dragFieldLayoutInfo private and instead added GetFieldLayoutInfo and 
		// SetDragFieldLayoutInfo methods.
		// 
		internal LayoutInfo GetFieldLayoutInfo( bool fallbackToAutogenerated, bool createNewIfNecessary )
		{
			if ( null != _fieldLayoutInfo )
				return _fieldLayoutInfo;

			if ( fallbackToAutogenerated && null != _autoGeneratedPositions )
				return _autoGeneratedPositions;

			if ( createNewIfNecessary )
			{
				// SSP 8/21/09 - TFS19187, TFS19273
				// Added this assert.
				// 
				Debug.Assert( false, "This is going to cause problems since layout info will be created "
					+ "using default grid column, row values of 0, 0 for all fields which will overlap all "
					+ "the fields. Correct thing to do here is to auto-generate positions using the logic "
					+ "in our style generator and use the auto-generated values for row, col, spans etc..." );

				return LayoutInfo.Create( this );
			}

			return null;
		}

				#endregion // GetFieldLayoutInfo

                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                // Changed from property to method since we may need to verify the templates before
                // returning since the fixed/fixable count is based on what is in the layout.
                //
                #region GetFixedFieldInfo
        /// <summary>
        /// Helper method for getting the object that provides fixed information for the field layout.
        /// </summary>
        /// <param name="verifyTemplate">The information in the FixedFieldLayoutInfo is based at least partially on what fields are in the layout and therefore requires that the templates have been generated. If true, the method will ensure the templates are generated before returning the object.</param>
        /// <returns></returns>
        internal FixedFieldLayoutInfo GetFixedFieldInfo(bool verifyTemplate)
        {
            if (null == this._fixedFieldInfo)
                this._fixedFieldInfo = new FixedFieldLayoutInfo(this);

            if (verifyTemplate)
            {
                FieldLayoutTemplateGenerator generator = this.StyleGenerator;

                Debug.Assert(null != generator || _presenter == null);
                Debug.Assert(null == generator || !generator.IsGeneratingTemplates);

                if (null != generator && !generator.IsGeneratingTemplates)
                    generator.GenerateTemplates();
            }

            return this._fixedFieldInfo;
        }
                #endregion //GetFixedFieldInfo

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region GetFixedFieldUITypeResolved
        internal FixedFieldUIType GetFixedFieldUITypeResolved(bool hasFixed, bool hasFixable)
        {
            FixedFieldUIType mode = FixedFieldUIType.Default;
            DataPresenterBase presenter = this._presenter;

            if (null != presenter && presenter.IsFixedFieldsSupportedResolved)
            {
                if (this._settings != null)
                    mode = this._settings.FixedFieldUIType;

                if (FixedFieldUIType.Default == mode && this._presenter.HasFieldLayoutSettings)
                    mode = this._presenter.FieldLayoutSettings.FixedFieldUIType;

                if (FixedFieldUIType.Default == mode)
                {
                    if (hasFixable)
                        mode = FixedFieldUIType.ButtonAndSplitter;
                    else if (hasFixed)
                        mode = FixedFieldUIType.Splitter;
                }
            }

            return mode == FixedFieldUIType.Default 
                ? FixedFieldUIType.None 
                : mode;
        }
                #endregion //GetFixedFieldUITypeResolved

				#region GetHeaderAreaColumnDefinitions

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetHeaderAreaColumnDefinition
	
				#region GetHeaderAreaRowDefinitions

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetHeaderAreaRowDefinitions	

				#region GetHeaderPresenterFromCache

		internal HeaderPresenter GetHeaderPresenterFromCache()
		{
			HeaderPresenter hp = null;

			if (this._headerPresenterCache != null && this._headerPresenterCache.Count > 0)
			{
				hp = this._headerPresenterCache[0];
				this._headerPresenterCache.RemoveAt(0);

				this.InitializeHeaderPresenter(hp);

				return hp;
			}

			hp	= new HeaderPresenter();
			this.InitializeHeaderPresenter(hp);

			return hp;
		}

				#endregion //GetHeaderPresenterFromCache

                // JJD 5/26/09 - TFS17590 - added
                #region GetLabelFromPropertyDescriptor

        internal string GetLabelFromPropertyDescriptor(PropertyDescriptor pd, object pdSource)
        {
            string label = null;

            DataColumn column = null;
            DataColumnPropertyDescriptor dcpd = pd as DataColumnPropertyDescriptor;

            if (dcpd != null)
                column = dcpd.Column;
            else
            {
                // if a column map has been initialized (i.e we are in field initialization)
                // then use it to get the associated column
                if (this._dataColumnMap != null)
                    this._dataColumnMap.TryGetValue(pd.Name, out column);
                else
                {
                    // Otherwise use the less efficient IndexOf method if the
                    // source is a DataView
                    DataView dv = pdSource as DataView;

                    if (dv != null && dv.Table != null)
                    {
                        int index = dv.Table.Columns.IndexOf(pd.Name);

                        if (index >= 0)
                            column = dv.Table.Columns[index];
                    }
                }
            }

            if (column != null)
                label = column.Caption;

            if (label == null)
                label = pd.DisplayName;

            return label;
        }

                #endregion //GetLabelFromPropertyDescriptor	
    
				#region GetProvider

		internal LayoutPropertyDescriptorProvider GetProvider(object listObject, IEnumerable containingCollection)
		{
			// get the provider
			PropertyDescriptorProvider pp = this._presenter.FieldLayouts.GetPropertyDescriptorProvider(listObject, containingCollection);

			if (pp == null)
				return null;

			return GetProvider(pp);
		}

		// JJD 8/1/07 - Optimization
		// Added overload that takes PropertyDescriptorProvider
		internal LayoutPropertyDescriptorProvider GetProvider(PropertyDescriptorProvider propDescProvider)
		{

			if (this._propertyDescriptorProviders == null)
				this._propertyDescriptorProviders = new List<WeakReference>();
			else
			{
				LayoutPropertyDescriptorProvider existingLayoutProvider = null;

				// loop over the cached layout providers backwards so that
				// we can optimize the removal of references that are no longer alive
				for (int i = this._propertyDescriptorProviders.Count - 1; i >= 0; i-- )
				{
				    WeakReference reference = this._propertyDescriptorProviders[i];

				    LayoutPropertyDescriptorProvider layoutProvider = Utilities.GetWeakReferenceTargetSafe(reference) as LayoutPropertyDescriptorProvider;

				    // if the weakrefernce isn't alive then remove it
				    if (layoutProvider == null)
				        this._propertyDescriptorProviders.RemoveAt(i);
				    else
				    if (existingLayoutProvider == null &&
				        layoutProvider.Provider.IsCompatibleProvider(propDescProvider))
				        existingLayoutProvider = layoutProvider;
				}

				// if we found an existing layout provider then return it
				if ( existingLayoutProvider != null )
				    return existingLayoutProvider;
			}

			// since we didn't find an existing layout provider we need to create a new one
			LayoutPropertyDescriptorProvider newLayoutProvider = new LayoutPropertyDescriptorProvider(this, propDescProvider);

			// add it to the cached list
			this._propertyDescriptorProviders.Add(new WeakReference(newLayoutProvider));

			return newLayoutProvider;
		}

				#endregion //GetProvider	

				#region GetSelectionStrategyForItem

		internal SelectionStrategyBase GetSelectionStrategyForItem(ISelectableItem item)
        {
            if (item is Record)
            {
                this._selectionStrategyRecord = this.GetSelectionStrategyHelper(this.SelectionTypeRecordResolved, this._selectionStrategyRecord);
                return this._selectionStrategyRecord;
            }

            if (item is Cell)
            {
                this._selectionStrategyCell = this.GetSelectionStrategyHelper(this.SelectionTypeCellResolved, this._selectionStrategyCell);
                return this._selectionStrategyCell;
            }

            if (item is Field)
            {
                this._selectionStrategyField = this.GetSelectionStrategyHelper(this.SelectionTypeFieldResolved, this._selectionStrategyField);
                return this._selectionStrategyField;
            }

            Debug.Fail("We shouldn't get here - unknown item type");

            return null;
        }

        private SelectionStrategyBase GetSelectionStrategyHelper(SelectionType selectionType, SelectionStrategyBase oldStrategy)
        {
			
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

			return SelectionStrategyBase.GetSelectionStrategy(selectionType, this.DataPresenter, oldStrategy);
        }

                #endregion //GetSelectionStrategyForItem	

				#region GetSummaryCellAreaColumnDefinitions

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion // GetSummaryCellAreaColumnDefinitions

				#region GetSummaryCellAreaRowDefinitions

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion // GetSummaryCellAreaRowDefinitions

				#region Initialize

		internal void Initialize(FieldLayoutTemplateGenerator styleGenerator)
		{
			this._styleGenerator = styleGenerator;

			if ( styleGenerator != null )
			{
				// AS 10/13/09 NA 2010.1 - CardView
				_cachedCellPresentation = null;

				this.SetValue(FieldLayout.CellPresentationPropertyKey, styleGenerator.CellPresentation);
			}

			// AS 8/25/11 TFS84612
			this.InvalidateCellContentAlignmentResolved();

			this._isInitialized = true;

            // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
            // Initialize this flag in OnRecordLoaded
            // instead set _hasBeenInitializeAfterDataSourceChange flag to true
			//this._initialRecordLoaded = true;
            this._hasBeenInitializedAfterDataSourceChange = true;

            // Bumping the version # is required as a workaround for the lack of a lazy 
            // property initialization story in the framework
			if (styleGenerator != null)
				this.InternalVersion++;
        }

				#endregion //Initialize

                #region InitializeFields

		// JJD 8/1/07 - Optimization
		// Added overload that passes in the propertyDescriptorProvider
        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
        // Added parentRecordCollection parameter
        //internal void InitializeFields(object listObject, IEnumerable containingCollection)
		internal void InitializeFields(object listObject, IEnumerable containingCollection, RecordCollectionBase parentRecordCollection)
		{
			this.InitializeFields(listObject, containingCollection, parentRecordCollection, null);
		}

        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
        // Added parentRecordCollection parameter
		//internal void InitializeFields(object listObject, IEnumerable containingCollection, PropertyDescriptorProvider propertyDescriptorProvider)
		internal void InitializeFields(object listObject, IEnumerable containingCollection, RecordCollectionBase parentRecordCollection, PropertyDescriptorProvider propertyDescriptorProvider)
        {
			Debug.Assert( this._areFieldsInitialized == false);
 			Debug.Assert( this._presenter != null);

			if (this._presenter == null || this._areFieldsInitialized)
				return;

			// JJD 3/06/07 - BR20877
			// Set a flag so we know we are in the initialization routine
			this._isInFieldInitialization = true;

			try
			{
				this._areFieldsInitialized = true;
                
                // JJD 12/13/07
                // Added a flag so we know if this layout was initialized with an EnumerableObjectWrapper 
                this._wrapsEnumerableObject = listObject is EnumerableObjectWrapper;

				this._presenter.RaiseFieldLayoutInitializingEvent(this);

				bool autoGenerateFields = this.AutoGenerateFieldsResolved;

				Field[] existingFields = new Field[this.Fields.Count];

				// copy the existing fields into an array
				if (this.Fields.Count > 0)
					this.Fields.CopyTo(existingFields, 0);

				// get the provider
				// JJD 8/1/07 - Optimization
				// Only get the provider if it wasn't passed in
				//PropertyDescriptorProvider propertyDescriptorProvider = this._presenter.FieldLayouts.GetPropertyDescriptorProvider(listObject, containingCollection);
				if ( propertyDescriptorProvider == null )
					propertyDescriptorProvider = this._presenter.FieldLayouts.GetPropertyDescriptorProvider(listObject, containingCollection);
                
				// if the key is null then use the provider as the key
                if (this.Key == null)
                {
                    
                    
                    
                    this.InitializeKey(propertyDescriptorProvider.Key);
                }

                // JJD 5/26/09 - TFS17590
                // Get the propertyDescriptor source
                object source = propertyDescriptorProvider.Source;

                DataView dv = source as DataView;

                // JJD 5/26/09 - TFS17590
                // If the datasource is a DataView then initialize a map of its columns.
                if (dv != null && dv.Table != null )
                {
                    this._dataColumnMap = new Dictionary<string,DataColumn>();

                    foreach (DataColumn column in dv.Table.Columns)
                        this._dataColumnMap.Add(column.ColumnName, column);
                }

				PropertyDescriptorCollection pds = propertyDescriptorProvider.GetProperties();

				Debug.Assert(pds != null);

				int totalFieldsGenerated = 0;

				int i;
				int pdCount = pds.Count;

				// JJD 4/30/07
				// Optimization - use for loop instead of foreach
                for (i = 0; i < pdCount; i++)
                {
                    // JJD 5/26/09 - TFS17590
                    // Pass the propertyDescriptor source into InitializeFieldHelper
                    //this.InitializeFieldHelper(pds[i], existingFields, autoGenerateFields, ref totalFieldsGenerated);
                    this.InitializeFieldHelper(pds[i], source, existingFields, autoGenerateFields, ref totalFieldsGenerated);
                }

				for (i = 0; i < existingFields.Length; i++)
				{
					Field existingField = existingFields[i];

					// bypass slots that we already used or that are unbound
					if (existingField == null)
						continue;

					if (existingField.IsUnbound)
					{
						// initialize the label for the unbound field if it
						// wasn't already set
						if (existingField.Label == null &&
							existingField.Name != null)
							existingField.InitializeDefaultLabel(existingField.Name);

						continue;
					}

                    // JJD 5/18/09
                    // For xmlnodes it is okay to have fields that haven't been encountered yet
                    if (propertyDescriptorProvider is XmlNodePropertyDescriptorProvider)
                        continue;

					// throw an error
					throw new NotSupportedException(DataPresenterBase.GetString("LE_NotSupportedException_8", new object[] { existingField.Name, propertyDescriptorProvider.Source.ToString() }));
				}

				FieldLayoutTemplateGenerator fli = this._presenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator(this);

				if (fli != null && !this.IsInitialized)
					this.Initialize(fli);

				//this._presenter.RaiseFieldLayoutInitializedEvent(this);

				if (this._sortedFields != null)
					this._sortedFields.OnOwnerInitialized();

				// JJD 7/18/07 - BR24617
				// Cache a flag that lets us know if the fields were auto generated during initialization
				this._areFieldsAutoGenerated = autoGenerateFields;

                // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                #region Initialize ParentFieldLayout and ParentField if not already set

                ExpandableFieldRecord parentRecord = parentRecordCollection != null ? parentRecordCollection.ParentRecord as ExpandableFieldRecord : null;
                Field parentField = parentRecord != null ? parentRecord.Field : null;

                if (parentField != null)
                {
                    if (this.ParentFieldLayoutKey == null &&
                        this.ParentFieldName == null)
                    {
                        this.InitializeParentInfo(parentField.Owner, parentField);
                    }
                    else
                    {
                        FieldLayout parentFieldLayout = parentField.Owner;

                        if (this.ParentFieldLayoutKey == null)
                        {
                            this._parentFieldLayout = parentFieldLayout;
                            this.ParentFieldLayoutKey = parentFieldLayout.Key;
                        }

                        if (this.ParentFieldName == null)
                        {
                            this._parentField = null;
                            this.ParentFieldName = parentField.Name;
                        }
                    }
                }

                #endregion //Initialize ParentFieldLayout and ParentField if not already set	
    
				// JM 07-29-09 TFS 19241 - Force our list of PropertyDescriptorProvidersto be created if it hasn't been.
				if (this._propertyDescriptorProviders == null && propertyDescriptorProvider != null)
					this.GetProvider(propertyDescriptorProvider);

				// JM 07-29-09 TFS 19241
				this._fieldLayoutInitializedEventRaised = true;

				// JJD 9/9/10 - TFS37596
				// Take a snapshot of the fields collection before we raise the FieldLayoutInitialize event
				HashSet beforeEventSnapshot = new HashSet();
				beforeEventSnapshot.AddItems(this._fields);

				// [BR20144] JM 2-26-07 - Move here from above.
				this._presenter.RaiseFieldLayoutInitializedEvent(this);

				// JJD 9/9/10 - TFS37596
				#region Initialize any non-unbound fields that were added in the FieldLayoutInitialized event

				// JJD 9/9/10 - TFS37596
				// Take another snapshot of the fields collection after we raised the FieldLayoutInitialize event
				HashSet afterEventSnapshot = new HashSet();
				afterEventSnapshot.AddItems(this._fields);

				// see if the snapshots are different
				if (!HashSet.AreEqual(beforeEventSnapshot, afterEventSnapshot))
				{
					// Create a dictionary of all the added (non-unbound) fields keyed by field name
					Dictionary<string, Field> addedFlds = new Dictionary<string, Field>();
					foreach (Field fld in afterEventSnapshot)
					{
						if (!fld.IsUnbound && !beforeEventSnapshot.Exists(fld))
							addedFlds[fld.Name] = fld;
					}

					if (addedFlds.Count > 0)
					{
						// walk over the property descriptors looking for matches
						int dummy = 0;
						for (i = 0; i < pdCount; i++)
						{
							PropertyDescriptor pd = pds[i];

							Field fld;
							if (pd != null && addedFlds.TryGetValue(pd.Name, out fld))
							{
								// call InitializeFieldHelper to set the datatype and pd
								this.InitializeFieldHelper(pd, source, new Field[] { fld }, false, ref dummy);

								// renmove the entry from the map
								addedFlds.Remove(pd.Name);

								// if the map is empty then break out 
								if (addedFlds.Count == 0)
									break;
							}

						}
					}
				}

				#endregion //Initialize any non-unbound fields that were added in the FieldLayoutInitialized event	
    
				// JM 08-04-08 Load/Save Customizations
				this._presenter.CustomizationsManager.ApplyCustomizations();

                // JJD 4/15/09 - NA 2009 vol 2 - Cross band grouping
                // Since we are supporting cross band grouping we need to
                // notify the groupbyarea that a FieldLayout has been initialized
                this._presenter.BumpGroupByAreaStyleVersion();
			}
			finally
			{
				// JJD 03/29/12 - TFS106889 
				// Call OnFieldLayoutInitialized on the RecordFilter collection
				// so they can initialize any pending conditions and raise
				// the RecordFilterDropDownItemInitializing event
				if (_recordFilters != null)
					_recordFilters.OnFieldLayoutInitialized();

                // JJD 5/26/09 - TFS17590
                // Clear the map created above
                this._dataColumnMap = null;

				// JJD 3/06/07 - BR20877
				// Reset the flag 
				this._isInFieldInitialization = false;
            }
		}

        // JJD 5/26/09 - TFS17590
        // Added the propertyDescriptor source paramater
        //private void InitializeFieldHelper(PropertyDescriptor pd, Field[] existingFields, bool autoGenerate, ref int totalFieldsGenerated)
        private void InitializeFieldHelper(PropertyDescriptor pd, object pdsource, Field[] existingFields, bool autoGenerate, ref int totalFieldsGenerated)
        {
			string key = pd.Name;
			Type dataType = pd.PropertyType;

            
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


            // JJD 5/26/09 - TFS17590
            // Moved logic to GetLabelFromPropertyDescriptor
			string label = this.GetLabelFromPropertyDescriptor(pd, pdsource);

			Field field = null;

            // Loop over the existing field array
            for (int i = 0; i < existingFields.Length; i++)
            {
				field = existingFields[i];

				// bypass slots that we already used 
				if ( field == null)
					continue;

                // If the key already exists then initialize and return
                if (field.DoesPropertyMatch(pd))
                {
					// JJD 12/2/11 - TFS21317
					// Only set the DataType if it hasn't already been set.
					// If we don't check the IsDataTypeExplicitySet flag we
					// will step on the explicit DataType setting.
					if ( !field.IsDataTypeExplicitySet )
						field.DataType = dataType;

					// SSP 9/26/11 TFS86720
					// Added reinitialize parameter. Pass true for it.
					// 
                    //field.InitializePropertyDescriptor(pd);
					field.InitializePropertyDescriptor( pd, false );

                    // JJD 9/21/09 - TFS22404
                    // We should always initialize the default label
                    //if (field.Label == null)
                        field.InitializeDefaultLabel(label);

					existingFields[i] = null;
                    return;
                }
            }

            if (autoGenerate)
            {
                field = new Field(dataType, key, label);

				// SSP 9/26/11 TFS86720
				// Added reinitialize parameter. Pass true for it.
				// 
                //field.InitializePropertyDescriptor(pd);
				field.InitializePropertyDescriptor( pd, false );

				// To make sure we always include expandable fields, check IsExpandableByDefault 
				// and if true, add the field without checking to see if we've exceeded
				// MaxFieldsToAutoGenerateResolved.
				if (field.IsExpandableByDefault)
					this.Fields.Add(field);
				else if (totalFieldsGenerated < this.MaxFieldsToAutoGenerateResolved)
				{
					this.Fields.Add(field);
					totalFieldsGenerated++;
				}
            }
        }

                #endregion //InitializeFields

				#region InitializeOwner

		internal void InitializeOwner(DataPresenterBase presenter)
		{
            if (this._presenter != null && this._presenter != presenter)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_10" ) );

            this._presenter = presenter;

            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            this.DirtyFixedState();

            // JJD 12/21/07
            // Delay setting this property until InitializeOwner is called so we only
            // do it at run time. This will allow the VS2008 designer to work properly 
            if (this._presenter == null ||
                 !DesignerProperties.GetIsInDesignMode(this._presenter))
            {
                FieldSettings fs = this.GetValue(FieldSettingsProperty) as FieldSettings;

                if (fs == null)
                    this.SetValue(FieldSettingsProperty, this.FieldSettings);
            }

		}

				#endregion //InitializeOwner

                // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
                #region InitializeParentInfo

        internal void InitializeParentInfo(FieldLayout parentFieldLayout, Field parentField)
        {
            Debug.Assert(parentField == null || parentField.Owner == parentFieldLayout, "The parent field should come from the parent layout");

            this.ParentFieldLayoutKey = parentFieldLayout.Key;
            this.ParentFieldName = parentField != null ? parentField.Name : null;

            this._parentFieldLayout = parentFieldLayout;
            this._parentField = parentField;
        }

                #endregion //InitializeParentInfo	
    
				#region InitializeSizeAreaManager

				#region Commented out
		
#region Infragistics Source Cleanup (Region)

















































































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //Commented out

        
#region Infragistics Source Cleanup (Region)










































































































































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //InitializeSizeAreaManager	

                #region InvalidateGeneratedStyles

        internal void InvalidateGeneratedStyles(bool bumpVersion)
        {
            // AS 3/3/09 Optimization
            // Maintain previous behavior and regenerate the templates when this overload is used.
            //
            this.InvalidateGeneratedStyles(bumpVersion, true);
        }

        // AS 3/3/09 Optimization
        // Added an overload so we can skip the recreation of the templates
        //
		internal void InvalidateGeneratedStyles(bool bumpVersion, bool regenerateTemplates)
		{
			this.InvalidateGeneratedStyles(bumpVersion, regenerateTemplates, false);
		}

		// AS 6/22/11 TFS75274
		// Only the async would bump the special records but since we may not be able to use the async 
		// method we need an overload that takes the bump special records version.
		//
        internal void InvalidateGeneratedStyles(bool bumpVersion, bool regenerateTemplates, bool bumpSpecialRecords)
        {
			// SSP 2/4/10 TFS25283
			// Use the InvalidateGeneratedStylesAsnyc instead of having a separate async operation.
			// 
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			// SSP 9/2/09 TFS17893
			// Added InvalidateGeneratedStylesAsnyc method. If there's a pending invalidate
			// operation then cancel it.
			// 
			// ------------------------------------------------------------------------------------
			GridUtilities.InvalidateStylesAsyncInfo pendingOp = _pendingInvalidateGeneratedStyles;
			_pendingInvalidateGeneratedStyles = null;
			if ( null != pendingOp )
			{
				bumpVersion = bumpVersion || pendingOp._bumpVersion;
				regenerateTemplates = regenerateTemplates || pendingOp._regenerateTemplates;

				if ( null != pendingOp._dispatcherOperation )
					pendingOp._dispatcherOperation.Abort( );

				// SSP 2/4/10 TFS25283
				// 
				// AS 6/22/11 TFS75274
				//if ( pendingOp._bumpSpecialRecordsVersion )
				//    this.BumpSpecialRecordsVersion( );
				bumpSpecialRecords = bumpSpecialRecords || pendingOp._bumpSpecialRecordsVersion;
			}
			// ------------------------------------------------------------------------------------

			// AS 6/22/11 TFS75274
			if (bumpSpecialRecords)
				this.BumpSpecialRecordsVersion( );

			// JJD 12/4/06 - BR17605
			// Force an exit of edit mode when the styles are being invalidated
			// AS 7/27/09 NA 2009.2 Field Sizing
			//if (this._presenter != null &&
			//    this._presenter.CellValuePresenterInEdit != null)
			//    this._presenter.CellValuePresenterInEdit.EndEditMode(true, true);
			if (_presenter != null)
				_presenter.EditHelper.EndEditMode(true, true);

			// AS 6/22/09 NA 2009.2 Field Sizing
			if (regenerateTemplates)
				_autoFitState = null;

            if (this._styleGenerator != null)
            {
                // AS 3/3/09 Optimization
                //this._styleGenerator.InvalidateGeneratedTemplates();
                this._styleGenerator.InvalidateGeneratedTemplates(regenerateTemplates);

				// AS 4/26/07 Performance
				// Sending this now will cause the template record cache to be requested.
				// Instead, we'll wait until the end of the routine (i.e. after the
				// cache has been released and the version number bumped).
				//
                //this.RaisePropertyChangedEvent("HeaderAreaTemplate");
            }

			// clear the template record cache
			if (null != this._templateDataRecordCache)
			{
				// AS 7/7/09 TFS19145
				// I was thinking about removing this since it will be handled 
				// when the templateversion is bumped via the InvalidateGeneratedTemplates
				// call above but just in case we'll still do this. However, we don't 
				// want to release the entire cache when we are not regenerating the 
				// templates.
				//
				//this._templateDataRecordCache.ReleaseCache();
				_templateDataRecordCache.BumpCacheVersion(regenerateTemplates);
			}

			// JJD 4/25/07
			// Optimization - there is no need to clear the template data record
			// release the template record
			//if (null != this._templateDataRecord)
			//{
			//    this._templateDataRecord.ParentCollection.RemoveRecord(this._templateDataRecord);
			//    this._templateDataRecord = null;
			//}

			if (bumpVersion)
				this.InternalVersion++;

			// AS 4/26/07 Performance
			// Moved down from above since this could cause the template data record's cache to be
			// updated only to have it then dirtied thereafter leading to the rp for the template
			// record to be created/initialized twice.
			//
            // AS 3/3/09 Optimization
            // If we're not regenerating the templates then don't send a change
            // notification for the HeaderAreaTemplate.
            //
            //if (this._styleGenerator != null)
            if (this._styleGenerator != null && regenerateTemplates)
                this.RaisePropertyChangedEvent("HeaderAreaTemplate");

		}

                #endregion //InvalidateGeneratedStyles	

				#region InvalidateGeneratedStylesAsnyc

		// SSP 9/2/09 TFS17893
		// Added InvalidateGeneratedStylesAsnyc method.
		// 
		internal void InvalidateGeneratedStylesAsnyc( bool bumpVersion, bool regenerateTemplates )
		{
			// AS 6/22/11 TFS75274
			this.InvalidateGeneratedStylesAsnyc(bumpVersion, regenerateTemplates, false);
		}

		// AS 6/22/11 TFS75274
		// Instead of the caller assuming this will be async and then set the bump special records 
		// we need to take it as a param.
		//
		internal void InvalidateGeneratedStylesAsnyc( bool bumpVersion, bool regenerateTemplates, bool bumpSpecialRecords )
		{
			Dispatcher dispatcher = this.Dispatcher;

			// AS 6/22/11 TFS75274
			var dp = this.DataPresenter;
			bool allowAsync = dp != null && !dp.IsSynchronousControl;

			if ( null == dispatcher || !allowAsync  )
			{
				this.InvalidateGeneratedStyles( bumpVersion, regenerateTemplates, bumpSpecialRecords  );
				return;
			}

			if ( null == _pendingInvalidateGeneratedStyles )
			{
				_pendingInvalidateGeneratedStyles = new GridUtilities.InvalidateStylesAsyncInfo( );
				_pendingInvalidateGeneratedStyles._dispatcherOperation = dispatcher.BeginInvoke( 
					DispatcherPriority.Send, new GridUtilities.MethodDelegate( InvalidateGeneratedStylesAsnycHandler ) );
			}

			_pendingInvalidateGeneratedStyles._bumpVersion = bumpVersion || _pendingInvalidateGeneratedStyles._bumpVersion;
			_pendingInvalidateGeneratedStyles._regenerateTemplates = regenerateTemplates || _pendingInvalidateGeneratedStyles._regenerateTemplates;
			_pendingInvalidateGeneratedStyles._bumpSpecialRecordsVersion = bumpSpecialRecords || _pendingInvalidateGeneratedStyles._bumpSpecialRecordsVersion; // AS 6/22/11 TFS75274
		}

		private void InvalidateGeneratedStylesAsnycHandler( )
		{
			GridUtilities.InvalidateStylesAsyncInfo info = _pendingInvalidateGeneratedStyles;
			_pendingInvalidateGeneratedStyles = null;
			if ( null != info )
			{
				// SSP 2/4/10 TFS25283
				// 
				if ( info._bumpSpecialRecordsVersion )
					this.BumpSpecialRecordsVersion( );

				this.InvalidateGeneratedStyles( info._bumpVersion, info._regenerateTemplates );
			}
		}

				#endregion // InvalidateGeneratedStylesAsnyc

				#region IsProviderCached

		internal bool IsProviderCached(PropertyDescriptorProvider propertyDescriptorProvider)
		{
			if ( this._propertyDescriptorProviders == null ||
				 this._propertyDescriptorProviders.Count == 0)
				return false;

			// loop over the currently cached providers to look for a match.
			// This is done backwards to optimize the removal of weak references
			// that are no longer alive
			for (int i = this._propertyDescriptorProviders.Count - 1; i >= 0; i--)
			{
				WeakReference weakReference = this._propertyDescriptorProviders[i];
				
				LayoutPropertyDescriptorProvider layoutProvider = Utilities.GetWeakReferenceTargetSafe(weakReference) as LayoutPropertyDescriptorProvider;

				// if the weak reference is not alive then remove it from the collection
				if (layoutProvider == null)
					this._propertyDescriptorProviders.RemoveAt(i);
				else
					if(	layoutProvider.Provider == propertyDescriptorProvider)
						return true;
			}

			return false;
		}

				#endregion //IsProviderCached

				#region IsListObjectCompatible

		// JJD 7/18/07 - BR24617
		// added flag to ignore auto gen flag
		//internal bool IsListObjectCompatible(object listObject, IEnumerable containingCollection)
		// JJD 8/2/07 - Optimization
		// Added PropertyDescriptorProvider param so we don't have to re-get it 
		//internal bool IsListObjectCompatible(object listObject, IEnumerable containingCollection, bool ignoreAutoGenerateFlag)
		internal bool IsListObjectCompatible(object listObject, IEnumerable containingCollection, bool ignoreAutoGenerateFlag, PropertyDescriptorProvider pp)
		{
			Debug.Assert(this._presenter != null);

			if (this._presenter == null)
				return false;

            // for XmlNodes we want to return true as long as the key is either null or
            // a string that matches the local name of the node
            XmlNode node = listObject as XmlNode;
            if (node != null && pp is XmlNodePropertyDescriptorProvider)
            {
                // if the ignoreAutoGenerateFlag is true that means we were called after
                // the AssigningFieldLayoutToItem event was raised so in that case
                // we want to return true
                if (ignoreAutoGenerateFlag)
                    return true;

                object key = this.Key;

                string keyAsLocalName = key as string;

                if (keyAsLocalName != null && keyAsLocalName.Length > 0)
                    return string.Compare(keyAsLocalName, node.LocalName, true) == 0;

                return key == null;    
            }

			if (this._fields == null )
				return true;

			int fieldCount = this._fields.Count;
			
			// JJD 7/18/07 - BR24617
			// Determine if we should do an exact match (returns false if the # of fields is different) 
			bool enforceExactMatch = this._areFieldsAutoGenerated && ignoreAutoGenerateFlag == false;

			// JJD 7/18/07 - BR24617
			// Only return true if the fields weren't auto generated
			//if (fieldCount < 1)
			if (fieldCount < 1 && !enforceExactMatch)
				return true;

			// JJD 8/2/07 - Optimization
			// Only get PropertyDescriptorProvider if it wasn't passed in
			//PropertyDescriptorProvider pp = this._presenter.FieldLayouts.GetPropertyDescriptorProvider(listObject, containingCollection);
			if ( pp == null )
				pp = this._presenter.FieldLayouts.GetPropertyDescriptorProvider(listObject, containingCollection);

			PropertyDescriptorCollection props = pp.GetProperties();

            int propsCount = props.Count;

			// JJD 7/18/07 - BR24617
			// Return false if field count doesn't match and the fields were auto generated
			if (enforceExactMatch)
			{
				if (fieldCount != this._fields.UnboundFieldsCount + propsCount )
					return false;
			}

			// JJD 4/30/07
			// Optimization - use for loop instead of foreach
			//foreach (Field fld in this.Fields)
			for (int i = 0; i < fieldCount; i++)
			{
				Field fld = this._fields[i];

				//bypass unbound fields
				if (fld.IsUnbound)
					continue;

				// do a case-sensitive search on the name
				PropertyDescriptor pd = props.Find(fld.Name, false);

				if (pd == null)
					return false;

				// if the data types don't match then return false
				Type dataType = fld.DataType;
				if (dataType != null)
				{
                    // JJD 4/17/08
                    // Special case the situation where we are using ValuePropertyDescriptor for a null item
                    // in the list (i.e. the list didn't implement ITypedList. In this case, we
                    // don't want to automatically match other nnon-null items in the list.
                    if (propsCount == 1 &&
                        ignoreAutoGenerateFlag == false &&
                        pd is ValuePropertyDescriptor &&
                        dataType == typeof(object) &&
                        listObject != null &&
                        listObject.GetType() != typeof(object))
                        return false;

                    if (!(pd.PropertyType == dataType ||
                          dataType.IsAssignableFrom(pd.PropertyType)))
						return false;
				}
			}
			
			return true;
		}

				#endregion //IsListObjectCompatible	

                // JJD 9/22/09 - TFS18162 - added
                #region OnAddedToCollection

        internal void OnAddedToCollection()
        {
            // release the cache sto make sure everything initializes properly
            // the next time someone accesses the cache
            if (this._templateDataRecordCache != null)
                this._templateDataRecordCache.ReleaseCache();

			// JJD 08/17/12 - TFS119037 
			// Reset the flag so we know the field layout was re-added
			_wasRemovedFromCollection = false;
        }

                #endregion //OnAddedToCollection	

                // JJD 9/22/09 - TFS18162 - added
                #region OnRemovedFromCollection

        internal void OnRemovedFromCollection()
        {
            // remove the templateDataRecord from the collection that is maintained
            // off the fieldlayouts collection
            if (this._templateDataRecord != null)
            {
                RecordCollectionBase templateDataRecords = this._presenter.FieldLayouts.TemplateDataRecords;

                int index = templateDataRecords.IndexOf(this._templateDataRecord);

                if (index >= 0)
                    templateDataRecords.RemoveAt(index);

                this._templateDataRecord = null;
            }

            // remove the templateGroupByRecord from the collection that is maintained
            // off the fieldlayouts collection
            if (this._templateGroupByRecord != null)
            {
                RecordCollectionBase templateGroupByRecords = this._presenter.FieldLayouts.TemplateGroupByRecords;

                int index = templateGroupByRecords.IndexOf(this._templateGroupByRecord);

                if (index >= 0)
                    templateGroupByRecords.RemoveAt(index);

                this._templateGroupByRecord = null;
            }
            
            // let the cache know so that it can remove any cached presenters
            // from the DP's logical tree
            if (this._templateDataRecordCache != null)
                this._templateDataRecordCache.OnFieldLayoutRemoved();


			// JJD 08/17/12 - TFS119037 
			// Set a flag so we know the field layout has been removed
			_wasRemovedFromCollection = true;

        }

                #endregion //OnRemovedFromCollection	

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                #region OnCurrentViewChanged
		// AS 10/13/09 NA 2010.1 - CardView
		// Added format settings changed parameter.
		//
		internal void OnCurrentViewChanged(bool formatSettingsChanged)
        {
			// AS 10/13/09 NA 2010.1 - CardView
			_cachedIsEmptyCellCollapsingSupportedByView = null;
			_cachedCellPresentation = null;

			// AS 11/29/10 TFS60418
			_cachedIsHorizontal = null;

            this.DirtyFixedState();

            if (null != this._fields)
            {
                foreach (Field field in this._fields)
                {
                    field.OnCurrentViewChanged(formatSettingsChanged);

					// AS 8/25/11 TFS84612
					field.DirtyCellContentAlignmentCache();
                }
            }
        }
                #endregion //OnCurrentViewChanged

                // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
                #region OnDataRecordLoaded

        internal void OnDataRecordLoaded()
        {
            if (this._initialRecordLoaded == false)
            {
                this._initialRecordLoaded = true;

                if (this._presenter != null)
                    this._presenter.BumpGroupByAreaStyleVersion();
            }
        }

                #endregion //OnDataRecordLoaded	
    
				#region OnDataSourceChanged

		internal void OnDataSourceChanged()
		{
            // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
            // reset both the _initialRecordLoaded and the _hasBeenInitializeAfterDataSourceChange flag to false
            this._initialRecordLoaded = false;

			// AS 7/21/09 NA 2009.2 Field Sizing
			_maxRecordManagerDepth = -1;

			// AS 8/5/09 NA 2009.2 Field Sizing
			// Whenever the datasource changes, we will clear the count of autosize viewable records and 
			// recalculate that as field layouts are used.
			//
			this.AutoSizeInfo.HasViewableRecordFields = false;

			// AS 2/26/10 TFS28159
			this.AutoSizeInfo.HasRecordsInViewFields = false;

            //if (this._initialRecordLoaded)
            if (this._hasBeenInitializedAfterDataSourceChange)
			{
				this._hasBeenInitializedAfterDataSourceChange = false;

                // JJD 1/4/08 - BR25487
                // We don't want to clear the sorted fields collection when the datasource changes
                // so we preserve the existing sorting/grouping criteria 
                //if (this.HasSortedFields)
                //    this.SortedFields.Clear();
			}
		}

				#endregion //OnDataSourceChanged
			
				// JJD 7/18/07 - BR24617
				#region OnFieldAddedRemovedFromCollection

		internal void OnFieldAddedRemovedFromCollection()
		{
			// JJD 7/18/07 - BR24617
			// Clear the flag since there was a change to the fields collection
			this._areFieldsAutoGenerated = false;
		}

				#endregion //OnFieldAddedRemovedFromCollection	

				// JJD 4/27/11 - TFS73888 - added
				#region OnFieldConverterChanged
		internal void OnFieldConverterChanged(Field field)
		{
			// if we aren't initialized or the field is collapsed then we can bail out
			if (this._areFieldsInitialized == false ||
				_hasBeenInitializedAfterDataSourceChange == false ||
				_isInFieldInitialization == true ||
				field.VisibilityResolved == Visibility.Collapsed)
				return;

			if (_pendingFieldsToRefresh == null)
				_pendingFieldsToRefresh = new HashSet();

			int oldCount = _pendingFieldsToRefresh.Count;
			
			_pendingFieldsToRefresh.Add(field);

			int newCount = _pendingFieldsToRefresh.Count;

			// process the set of field asynchronously
			if (newCount == 1 && newCount != oldCount)
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GridUtilities.MethodDelegate(this.ProcessPendingFieldsToRefresh));
				
		}
				#endregion //OnFieldConverterChanged

				// AS 7/7/09 TFS19145
				#region OnFieldVisibilityChanged
		internal void OnFieldVisibilityChanged(Field field)
		{
			// SSP 2/4/10 TFS25283
			// Use the InvalidateGeneratedStylesAsnyc instead of having a separate async operation.
			// 
			// ------------------------------------------------------------------------------------
			this.InvalidateGeneratedStylesAsnyc( false, false, true  );

			// AS 6/22/11 TFS75274
			//Debug.Assert( null != _pendingInvalidateGeneratedStyles );
			//if ( null != _pendingInvalidateGeneratedStyles )
			//    _pendingInvalidateGeneratedStyles._bumpSpecialRecordsVersion = true;

			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------------
		}

		// SSP 2/4/10 TFS25283
		// 
		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnFieldVisibilityChanged
    
				#region OnPropertyDescriptorAdded

		internal void OnPropertyDescriptorAdded(PropertyDescriptor propertyDescriptor, PropertyDescriptorProvider provider)
		{
            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// if the provider isn't cached then exit
			if (!this.IsProviderCached(provider))
				return;

            
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


            this.PropertyDescriptorAddedHelper(propertyDescriptor, provider, this.AutoGenerateFieldsResolved, true);
		}

				#endregion //OnPropertyDescriptorAdded	

				#region OnPropertyDescriptorChanged

		internal void OnPropertyDescriptorChanged(PropertyDescriptor propertyDescriptor, PropertyDescriptorProvider provider)
		{
            // if the provider isn't cached then exit
            if (!this.IsProviderCached(provider))
                return;

            this.PropertyDescriptorChangedHelper(propertyDescriptor, provider, false, true);
		}

				#endregion //OnPropertyDescriptorChanged	

				#region OnPropertyDescriptorDeleted

		internal void OnPropertyDescriptorDeleted(PropertyDescriptor propertyDescriptor, PropertyDescriptorProvider provider)
		{
            // if the provider isn't cached then exit
            if (!this.IsProviderCached(provider))
                return;

            this.PropertyDescriptorChangedHelper(propertyDescriptor, provider, true, true);
		}
    
				#endregion //OnPropertyDescriptorDeleted	
        
                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
                #region OnProviderPropertyDescriptorsChanged

        internal void OnProviderPropertyDescriptorsChanged(PropertyDescriptorsChangedEventArgs e)
        {
            PropertyDescriptorProvider provider = e.Provider;

            // if the provider isn't cached then exit
            if (!this.IsProviderCached(provider))
                return;

            bool wereChangesMade = false;

            #region Process PropertyDescriptors that were removed

            if (e.PropertiesRemoved != null &&
                e.PropertiesRemoved.Length > 0)
            {
                foreach (PropertyDescriptor pd in e.PropertiesRemoved)
                {
                    this.PropertyDescriptorChangedHelper(pd, provider, true, false);

                    wereChangesMade = true;
                }
            }

            #endregion //Process PropertyDescriptors that were removed	

            #region Process PropertyDescriptors that were changed

            if (e.PropertiesChanged != null &&
                e.PropertiesChanged.Length > 0)
            {
                foreach (PropertyDescriptor pd in e.PropertiesChanged)
                {
                    this.PropertyDescriptorChangedHelper(pd, provider, false, false);

                    wereChangesMade = true;
                }
            }

            #endregion //Process PropertyDescriptors that were changed	
    
            #region Process PropertyDescriptors that were added

            bool autoGenerate = this.AutoGenerateFieldsResolved;

            // Process adds 
            if ( e.PropertiesAdded != null &&
                e.PropertiesAdded.Length > 0)
            {
                foreach (PropertyDescriptor pd in e.PropertiesAdded)
                {
                    this.PropertyDescriptorAddedHelper(pd, e.Provider, autoGenerate, false);

                    wereChangesMade = true;
                }
            }

            #endregion //Process PropertyDescriptors that were added	
    
            // post a delayed invalidation so we don't do multiple synchronous
            // invalidations for a single change
            if (wereChangesMade)
            {
                this.PostDelayedInvalidation();
            }
        }

                #endregion //OnProviderPropertyDescriptorsChanged	

				#region OnTemplatesGenerated
		internal void OnTemplatesGenerated()
		{
			// AS 10/9/09 NA 2010.1 - CardView
			_hasStarFieldsY = _hasStarFieldsX = null;

            // AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
            this.DirtyFixedState();
            this.BumpLayoutManagerVersion();

            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			// JM 1/14/09 NA 2009 Vol 1 - Fixed Fields
			// Give our fields the opportunity to initialize state now that the Templates have been generated.
			foreach (Field field in this.Fields)
				field.OnFieldLayoutTemplatesGenerated();

			// AS 7/7/09 TFS19145/NA 2009.2 Field Sizing
			// When autofit mode resolves to OnlyWhenStarFieldsVisible, then the 
			// autofit state may change as a result of a layout being calculated 
			// because we don't know until that point that the layout is in fact 
			// in the layout. So when you hide the last star field or show the 
			// first star field, we will have to bump the internal version.
			//
			this.VerifyAutoFitSettings(true);
		}
				#endregion //OnTemplatesGenerated

				#region ProcessFieldLayoutSettingsPropertyChanged

		// SSP 4/23/08 - Summaries Functionality
		// Added infrastructure so field, field layout and the data presenter can each process
		// change in field settings of any of these objects.
		// 
		/// <summary>
		/// This method is called whenever a property on FieldLayoutSettings belonging to this FieldLayout or the DataPresenterBase
		/// changes. When DataPresenter's FieldLayoutSettings changes, this method is called on all FieldLayouts, otherwise
		/// its called on just the associated FieldLayout.
		/// </summary>
		/// <param name="e">Property change notification event args.</param>
		/// <param name="settings">Settings whose property changed.</param>
		/// <param name="settingsOwner">The owner of the settings object - either DataPresenterBase or FieldLayout that the
		/// settings object belongs to.</param>
		/// <param name="firstPassForPresenterFlag"></param>
		/// <param name="fieldLayout"></param>
		/// <param name="presenter"></param>
		internal static void ProcessFieldLayoutSettingsPropertyChanged(
				PropertyChangedEventArgs e,
				FieldLayoutSettings settings,
				object settingsOwner,
				bool firstPassForPresenterFlag,
				FieldLayout fieldLayout,
				DataPresenterBase presenter
			)
		{
			bool dpInitialized = null != presenter && presenter.IsInitialized;

            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            string propName = e.PropertyName;
            bool raiseResolved = false;

			// AS 9/9/09 TFS21581
			bool invalidateGeneratedStyles = false;

			switch ( propName )
			{
                // these properties do not require a resolved notification and
                //  do not require invalidation of the styles
                case "CopyFieldLabelsToClipboard": // AS 4/8/09 NA 2009.2 ClipboardSupport
                case "AllowClipboardOperations": // AS 4/8/09 NA 2009.2 ClipboardSupport
                    break;

				case "CalculationScope":
					
					raiseResolved = true;
					if ( null != fieldLayout && null != fieldLayout._summaryDefinitions )
						fieldLayout._summaryDefinitions.BumpDataVersion( );
					break;
				case "SummaryDescriptionVisibility":
				case "GroupBySummaryDisplayMode":
					
					raiseResolved = true;
					if ( null != fieldLayout && null != fieldLayout._summaryDefinitions )
						fieldLayout._summaryDefinitions.BumpSummariesVersion( );
					break;
				case "AddNewRecordLocation":
				case "AllowAddNew":
				case "SpecialRecordOrder":
				case "RecordSeparatorLocation":
					
					raiseResolved = "SpecialRecordOrder" != propName;
						
					if ( null != fieldLayout )
					{
						fieldLayout.BumpSpecialRecordsVersion( );
						
						// JJD 8/18/10 - TFS36307
						// We need to bump the ScrollCountRecalcVersion so
						// that DataRecord's that are expanded already that
						// don't have any children will be updated so that
						// their add new record will get displayed
						if (presenter != null && propName == "AllowAddNew")
							presenter.BumpScrollCountRecalcVersion();
					}
					break;
				// AS 6/9/09 NA 2009.2 Field Sizing
                case "AutoFitMode":
					if (null != fieldLayout)
						fieldLayout.OnFieldWidthHeightChanged();
					break;
                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                case "FixedFieldUIType":
                    if (null != fieldLayout)
                    {
                        raiseResolved = true;
                        fieldLayout.DirtyFixedState();

                        if (fieldLayout._fields != null)
                        {
                            foreach (Field field in fieldLayout._fields)
                                field.VerifyFixedState();
                        }
                    }
                    break;
                
                // JJD 2/18/09 - TFS14057 
                case "RecordFiltersLogicalOperator":
                    raiseResolved = true;

                    // Bump the filter version so any existing filters will get re-applied
					if ( null != fieldLayout )
					{
						
						
						
						fieldLayout._cachedRecordFiltersLogicalOperatorResolved = null;

						fieldLayout.BumpRecordFilterVersion( );
					}
                    break;
                
                // JJD 6/22/09 - NA 2009 Vol 2 - Record Fixing - added
                case "AllowRecordFixing":
                case "FixedRecordUIType":
                    raiseResolved = true;

                    // Also raise the property change for RecordSelectorExtentResolved
                    if ( null != fieldLayout)
                        fieldLayout.RaisePropertyChangedEvent("RecordSelectorExtentResolved");
                    
                    break;

                // JJD 6/22/09 - NA 2009 Vol 2 - Record Fixing - added
                case "FixedRecordLimit":
                case "FixedRecordSortOrder":
                    raiseResolved = true;
                    break;

                // SSP 12/11/08 - NAS9.1 Record Filtering
				// 
				case "FilterAction":
				case "FilterClearButtonLocation":
				case "FilterRecordLocation":
				case "FilterUIType":
				case "RecordFilterScope":
				case "ReevaluateFiltersOnDataChange":
					raiseResolved = true;

					// Field's FilterClearButtonVisibilityResolved uses FilterClearButtonLocation for
					// resolution and therefore we need to raise its property changed.
					// 
					if ( null != fieldLayout )
					{
						// SSP 12/21/11 TFS67264 - Optimizations
						// 
						fieldLayout._cachedReevaluateFiltersOnDataChange = null;

						if ( "RecordFilterScope" == propName )
						{
							// There's no corresponding RecordFilterScopeResolved property.
							// 
							raiseResolved = false;

							
							
							
							fieldLayout._cachedRecordFilterScopeResolvedDefault = null;

                            // JJD 2/18/09 - TFS14057 
                            // Moved logic into BumpRecordFilterVersion method
                            fieldLayout.BumpRecordFilterVersion();
						}
							// SSP 4/10/09 TFS16485 TFS16490
							// 
						else if ( "FilterAction" == propName )
						{
							
							
							
							fieldLayout._cachedRecordFilterActionResolved = RecordFilterAction.Default;

							fieldLayout.BumpRecordFilterVersion( );
						}

						// Bump special records version to account for potential change in the visibility
						// of filter record as a result of change in the property setting of FilterUIType
						// or the FilterRecordLocation.
						// 
						fieldLayout.BumpSpecialRecordsVersion( );

						FieldCollection fields = fieldLayout.FieldsIfAllocated;
						if ( null != fields )
						{
							
							
							
							string raiseFieldPropChange = propName == "FilterUIType"
								? "AllowRecordFiltering"
								: "FilterClearButtonLocation" == propName
								? "FilterClearButtonVisibility"
								: null;

                            foreach (Field field in fields)
                            {
                                field.DirtyRecordFilterRelatedCache(true); 

                                // JJD 1/7/09 
                                // When the FielterUIType changes we need to let the field's know so they can 
                                // raise the appropriate notifications
								
								
								
                                
                                
								if ( null != raiseFieldPropChange )
									Field.ProcessFieldSettingsPropertyChanged( 
										new PropertyChangedEventArgs( raiseFieldPropChange ), 
										fieldLayout.FieldSettings, fieldLayout, firstPassForPresenterFlag, 
										firstPassForPresenterFlag, field, fieldLayout, fieldLayout.DataPresenter );
								
                            }
						}
					}
					break;

                // JJD 05/05/10 - TFS31349
                case "GroupByExpansionIndicatorVisibility":
                // AS 9/10/09 TFS21581
				case "RecordSelectorLocation":
				case "RecordSelectorExtent":
					// when UseNestedPanels is false we could end up recycling a groupbyrecordpresenter, etc.
					// and so we need to make sure the record indent version is dirtied so the margins are 
					// updated
					if (null != fieldLayout)
						fieldLayout.BumpRecordIndentVersionAsync();

					// we used to invalidate the generated styles before so continue to do that 
					invalidateGeneratedStyles = true;

                    // JJD 05/05/10 - TFS31349
                    if (propName == "GroupByExpansionIndicatorVisibility")
                        raiseResolved = true;

					break;

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				case "ChildRecordsDisplayOrder":
					// Reset the resolved value cache
					if (fieldLayout != null)
						fieldLayout.ResetCachedChildRecordsDisplayOrderResolvedValue();

					// We need to display to update when the value changes, but we only want to do this once for 
					// each change, so check the firstPassForPresenterFlag. Also, we should only do this when the
					// DataPresenter is initialized.
					if (firstPassForPresenterFlag && dpInitialized && presenter != null)
						presenter.OnChildRecordsDisplayOrderChanged();

					raiseResolved = true;

					break;

                // SSP 6/7/10 - Optimizations - TFS34031
                // Cache resolved property value. Added case for SupportDataErrorInfo.
                // 
                case "SupportDataErrorInfo":
                    invalidateGeneratedStyles = true;

                    if ( null != fieldLayout )
                        fieldLayout._cachedSupportDataErrorInfoResolvedDefault = null;

                    break;

				// AS 8/25/11 TFS84612
				case "LabelLocation":
					invalidateGeneratedStyles = true;

					if ( null != fieldLayout )
						fieldLayout.InvalidateCellContentAlignmentResolved();
					break;
				// SSP 1/31/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// 
				case "SortEvaluationMode":
				case "FilterEvaluationMode":
				case "SummaryEvaluationMode":
				case "GroupByEvaluationMode":
					{
						if ( null != fieldLayout )
						{
							fieldLayout._cachedSortEvaluationModeResolved = null;
							fieldLayout._cachedFilterEvaluationModeResolved = null;
							fieldLayout._cachedGroupByEvaluationModeResolved = null;
							fieldLayout._cachedSummaryEvaluationModeResolved = null;

							switch ( propName )
							{
								case "SortEvaluationMode":
								case "GroupByEvaluationMode":
									fieldLayout.BumpSortVersion( );
									break;
								case "FilterEvaluationMode":
									fieldLayout.BumpRecordFilterVersion( );
									break;
								case "SummaryEvaluationMode":
									if ( null != fieldLayout.SummaryDefinitionsIfAllocated )
										fieldLayout.SummaryDefinitionsIfAllocated.Refresh( );
									break;
							}
						}
					}
					break;

				default:
					{
						// AS 9/9/09 TFS21581
						// Moved out switch so properties we do handle can 
						// still invalidate generated styles.
						//
						//// This is to maintain what we were doing before whenever field layout's or data presenter's
						//// settings changed.
						//// 
						//
						//// SSP 7/17/07 BR22919
						//// Don't clear the group-by fields.
						//// 
						////this.InvalidateGeneratedStyles(true, true);
						//if (dpInitialized)
						//{
						//    // JJD 7/21/08 - BR34098 - Optimization
						//    // Invalidate the generated styles asynchronously so that if alot of
						//    // changes are made in a tight loop that only one invalidation will take place
						//    //presenter.InvalidateGeneratedStyles(true, false);
						//    // JJD 11/12/08 - TFS7858
						//    // Call the fieldlayout's PostDelayedInvalidation instead so we only invalidate
						//    // the styles if we aren't in the the middle of initializing all the fields 
						//    //presenter.InvalidateGeneratedStylesAsync();
						//    if (fieldLayout == null)
						//        presenter.InvalidateGeneratedStylesAsync();
						//    else
						//        fieldLayout.PostDelayedInvalidation();
						//}
						////this.SetValue(DataPresenterBase.GroupByAreaStyleResolvedPropertyKey, this.GroupByAreaStyleResolved);
						////this.GroupByArea.StyleVersionNumber++;
						invalidateGeneratedStyles = true;
					}
					break;
			}

			// AS 9/9/09 TFS21581
			// Moved out of default block above.
			//
			if (invalidateGeneratedStyles)
			{
				// This is to maintain what we were doing before whenever field layout's or data presenter's
				// settings changed.
				// 

				// SSP 7/17/07 BR22919
				// Don't clear the group-by fields.
				// 
				//this.InvalidateGeneratedStyles(true, true);
				if (dpInitialized)
				{
					// JJD 7/21/08 - BR34098 - Optimization
					// Invalidate the generated styles asynchronously so that if alot of
					// changes are made in a tight loop that only one invalidation will take place
					//presenter.InvalidateGeneratedStyles(true, false);
					// JJD 11/12/08 - TFS7858
					// Call the fieldlayout's PostDelayedInvalidation instead so we only invalidate
					// the styles if we aren't in the the middle of initializing all the fields 
					//presenter.InvalidateGeneratedStylesAsync();
					if (fieldLayout == null)
						presenter.InvalidateGeneratedStylesAsync();
					else
						fieldLayout.PostDelayedInvalidation();
				}
			}

            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            // Start raising property change for resolved properties like we do in the Field
            //
            if (raiseResolved && null != fieldLayout)
                fieldLayout.RaisePropertyChangedEvent(propName + "Resolved");
		}

				#endregion // ProcessFieldLayoutSettingsPropertyChanged

                #region Reset

        internal void Reset()
        {
            this._styleGenerator = null;
            this._isInitialized = false;

            // JJD 8/7/09
            // Clear the autofit state
            _autoFitState = null;

            // JJD 8/7/09
            // clear the template record cache
            if (null != this._templateDataRecordCache)
            {
                _templateDataRecordCache.BumpCacheVersion(true);
            }
        }

                #endregion //Reset	

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				#region ResetCachedChildRecordsDisplayOrderResolvedValue

		internal void ResetCachedChildRecordsDisplayOrderResolvedValue()
		{
			_cachedChildRecordsDisplayOrderResolved = null;
		} 

				#endregion // ResetCachedChildRecordsDisplayOrderResolvedValue

				#region ReturnHeaderPresenterToCache

		internal void ReturnHeaderPresenterToCache(HeaderPresenter headerPresenter)
		{
			this.HeaderPresenterCache.Add(headerPresenter);
		}

				#endregion //ReturnHeaderPresenterToCache

				#region SetFieldLayoutInfo

		// SSP 6/26/09 - NAS9.2 Field Chooser
		// Made _dragFieldLayoutInfo private and instead added GetFieldLayoutInfo and 
		// SetFieldLayoutInfo methods.
		// 
		/// <summary>
		/// Sets new field layout information.
		/// </summary>
		/// <param name="newLayoutInfo">New field layout information.</param>
		/// <param name="mergeWithCurrent">Whether to merge with the current field layout information
		/// instead of replacing it. This will copy over all entries from the new layout info
		/// into the existing layout info and mark entries that don't exist in the new layout info
		/// as collapsed. Essentially this is used to maintain collapsed states of the fields and
		/// ensure they don't overlap with other fields when they are made visible again.</param>
		/// <param name="syncFieldVisibility">Specifies whether to set the Visibility property
		/// of the fields to the associated ItemLayoutInfo's _isCollapsed state.</param>
		internal void SetFieldLayoutInfo( LayoutInfo newLayoutInfo, bool mergeWithCurrent, bool syncFieldVisibility )
		{
			// SSP 1/12/10 TFS25122
			// Raise ActualPosition property change notifications.
			// 
			List<Field> notifyFieldList = new List<Field>( );

			if ( _fieldLayoutInfo != newLayoutInfo )
			{
				// SSP 1/12/10 TFS25122
				// Raise ActualPosition property change notifications.
				// 
				// ------------------------------------------------------------------
				foreach ( Field field in _fields )
				{
					ItemLayoutInfo x = null != _fieldLayoutInfo ? _fieldLayoutInfo[field] : null;
					ItemLayoutInfo y = null != newLayoutInfo ? newLayoutInfo[field] : null;
					if ( x != y && ( null == x || null == y || ! ItemLayoutInfo.HasSamePosition( x, y ) ) )
						notifyFieldList.Add( field );
				}
				// ------------------------------------------------------------------

				if ( null != _fieldLayoutInfo && mergeWithCurrent )
					// SSP 10/16/09 TFS23582
					// We need to mark the fields that are not part of the new layout as collapsed so when 
					// those fields are made visible, we know to make sure that they don't overlap with other
					// fields. Pass true for the new markCollapsedFieldsNotInSource parameter.
					// 
					//_fieldLayoutInfo.Merge( newLayoutInfo );
					_fieldLayoutInfo.Merge( newLayoutInfo, true );
				else
					_fieldLayoutInfo = newLayoutInfo;
			}

			// SSP 1/12/10 TFS25122
			// Raise ActualPosition property change notifications.
			// 
			Field.RaisePropertyChangedEventHelper( notifyFieldList, "ActualPosition" );

			if ( syncFieldVisibility && null != newLayoutInfo )
				newLayoutInfo.SynchronizeFieldVisibility( );
		}

				#endregion // SetFieldLayoutInfo

				#region SetGridCounts

		internal void SetGridCounts(int totalColumnsGenerated, int totalRowsGenerated)
        {
            this._totalColumnsGenerated = totalColumnsGenerated;
            this._totalRowsGenerated = totalRowsGenerated;
            




        }
                #endregion //SetGridCounts	

                #region SetHasSeparateHeader

        internal void SetHasSeparateHeader(bool separateHeader)
        {
			// JJD 4/26/07
			// Optimization - cache the property locally 
			this._cachedHasSeparateHeader = separateHeader;

			this.SetValue(FieldLayout.HasSeparateHeaderPropertyKey, KnownBoxes.FromValue(separateHeader));
        }

                #endregion //SetHasSeparateHeader	

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

			if (this.ShouldSerializeSettings() ||
				this.ShouldSerializeDescription() ||
				this.ShouldSerializeFields() ||
				this.ShouldSerializeFieldSettings() ||
				this.ShouldSerializeRecordFilters() ||
				this.ShouldSerializeSummaryDefinitions() ||
				this.ShouldSerializeKey())
				return true;

			return GridUtilities.ShouldSerialize(this);
		}

                #endregion //ShouldSerialize	

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region VerifyAutoFitSettings
		internal void VerifyAutoFitSettings(bool force)
		{
			if (_autoFitState == null || force)
			{
				Debug.Assert(_styleGenerator == null || !_styleGenerator.IsGeneratingTemplates);

				// AS 1/3/12 TFS98355
				// The AutoFitMode was requested once early in the template usage while the template record 
				// cache was being initialized. This was happening before the style generator created the 
				// templates. Since we don't know about the fields that are shown we don't know if there are 
				// any star columns so it first resolved to no autosizing. Then while still in the template 
				// record initialization we ended up generating the templates and then we had star columns so 
				// the autofit mode was now width so we tried to invalidate the style generator and template 
				// data record cache (which was still updating). So if we get here while the template record 
				// is initializing then weneed to make sure the templates are generated before calculating the 
				// auto fit mode.
				//
				if (_styleGenerator != null && !_styleGenerator.IsGeneratingTemplates && this.TemplateDataRecordCache.IsInitializingCache)
					_styleGenerator.GenerateTemplates();

				AutoFitState state = ResolveAutoFit(this);

				if (state != _autoFitState)
				{
					bool invalidate = _autoFitState != null;
					var oldState = _autoFitState;
					_autoFitState = state;

					if (invalidate)
						this.InvalidateGeneratedStyles(true, true);

					// AS 11/8/11 TFS88111
					// Since its public we should send change notifications.
					//
					if ((_autoFitState & AutoFitState.Width) != (oldState & AutoFitState.Width))
						this.OnPropertyChanged("AutoFitToWidth");

					if ((_autoFitState & AutoFitState.Height) != (oldState & AutoFitState.Height))
						this.OnPropertyChanged("AutoFitToHeight");
				}
			}
		}
				#endregion //VerifyAutoFitSettings

				// JM 07-29-09 TFS 19241
				#region VerifyAllPropertyDescriptorProviders

		internal void VerifyAllPropertyDescriptorProviders()
		{
			if (this._propertyDescriptorProviders == null)
				return;

			for (int i = this._propertyDescriptorProviders.Count - 1; i >= 0; i--)
			{
				WeakReference						weakReference	= this._propertyDescriptorProviders[i];
				LayoutPropertyDescriptorProvider	layoutProvider	= Utilities.GetWeakReferenceTargetSafe(weakReference) as LayoutPropertyDescriptorProvider;

				if (layoutProvider != null)
					layoutProvider.VerifyFieldDescriptors();
			}
		}

				#endregion //VerifyAllPropertyDescriptorProviders

                // AS 2/27/09 TFS14730
                // Added helper method so we can ensure that the templates
                // are up to date.
                //
                #region VerifyStyleGeneratorTemplates
        
        internal void VerifyStyleGeneratorTemplates()
        {
            FieldLayoutTemplateGenerator gen = this._styleGenerator;
            Debug.Assert(null != gen);

            if (null != gen)
            {
                Debug.Assert(!gen.IsGeneratingTemplates);

                if (false == gen.IsGeneratingTemplates)
                    gen.GenerateTemplates();
            }
        } 
                #endregion //VerifyStyleGeneratorTemplates

            #endregion //Internal Methods

            #region Private Methods

				#region ApportionSpannedSize

        
#region Infragistics Source Cleanup (Region)











































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //ApportionSpannedSize	
    
				#region CalculateCellLabelSize

        
#region Infragistics Source Cleanup (Region)





































































































































































































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //CalculateCellLabelSize	

				#region CalculateFieldSize

        
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

				#endregion //CalculateFieldSize	

                // AS 4/8/09 NA 2009.2 ClipboardSupport
                #region GetResolvedValue
        private T GetResolvedValue<T>(DependencyProperty property, T defaultValue, T defaultResolvedValue)
        {
            Debug.Assert(null != property && property.OwnerType == typeof(FieldLayoutSettings));

            T value;

            if (null != _settings)
            {
                value = (T)_settings.GetValue(property);

                if (!object.Equals(defaultValue, value))
                    return value;
            }

            FieldLayoutSettings dpSettings = null != _presenter ? _presenter.FieldLayoutSettingsIfAllocated : null;
            if (null != dpSettings)
            {
                value = (T)dpSettings.GetValue(property);

                if (!object.Equals(defaultValue, value))
                    return value;
            }

            return defaultResolvedValue;

        }
                #endregion //GetResolvedValue

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region HasStarFields
		// MD 3/28/12 - TFS106849
		// Made this internal.
		//private bool HasStarFields(bool width)
		internal bool HasStarFields(bool width)
		{
			foreach (Field f in this.Fields)
			{
				if (!f.IsInLayout)
					continue;

				FieldLength len = f.GetWidthOrHeight(width);

				if (len.IsStar)
					return true;
			}

			return false;
		}
				#endregion //HasStarFields

				#region InitializeHeaderPresenter

		private void InitializeHeaderPresenter(HeaderPresenter headerPresenter)
		{
			headerPresenter.Content = this;
			headerPresenter.SetBinding(HeaderPresenter.ContentTemplateProperty, Utilities.CreateBindingObject("HeaderAreaTemplate", BindingMode.OneWay, this));
			headerPresenter.SetBinding(HeaderPresenter.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this));
		}

				#endregion //InitializeHeaderPresenter
    
				#region InitializeAreaSizeManagers

        
#region Infragistics Source Cleanup (Region)


































#endregion // Infragistics Source Cleanup (Region)

				#endregion //InitializeAreaSizeManagers	

				// AS 8/25/11 TFS84612
				#region InvalidateCellContentAlignmentResolved
		private void InvalidateCellContentAlignmentResolved()
		{
			var fields = _fields;

			if (null != fields)
			{
				for (int i = 0, count = fields.Count; i < count; i++)
					fields[i].DirtyCellContentAlignmentCache();
			}
		}
				#endregion //InvalidateCellContentAlignmentResolved

				// JM 12-12-08 Added for RecordFiltering feature

				#region OnCustomFilterDialogClosed

		private void OnCustomFilterDialogClosed(ToolWindow toolWindow, bool? dialogResult)
		{
			toolWindow.Resources = null;
		}

				#endregion //OnCustomFilterDialogClosed	

        	
                // JJD 7/21/08 - BR34098 - commented out, no longer used to do asynchrous invalidation
				#region OnDelayedInvalidation

        //private void OnDelayedInvalidation()
        //{
        //    // check flag to prevent redundant invalidations
        //    if (!this._invalidationPending)
        //        return;

        //    this._invalidationPending = false;

        //    // JM 02-22-08 - BR30563
        //    // Call the InvalidateGeneratedStyles method off the datapresenter instead so that we
        //    // trigger a refresh of all the elements currently in the visual tree
        //    //this.InvalidateGeneratedStyles(true);
        //    this.DataPresenter.InvalidateGeneratedStyles(true, false);
        //}

				#endregion //OnDelayedInvalidation	

				#region OnFieldCollectionChanged

		private void OnFieldCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// JJD 3/06/07 - BR20877
			// Ignore notifications generated during initialization.
			if (this._isInFieldInitialization)
				return;


			// [JM 05-31-07 BR23359] Reset the AreFieldsInitialized flag if we get a reset or if the count goes to zero as a 
			// result of the change to the collection.
			if (((FieldCollection)sender).Count < 1  ||  e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				this._areFieldsInitialized = false;

            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            this.DirtyFixedState();

			// AS 9/17/09 TFS22285
			// We need to proactively clear the template cache since the 
			// field lists it maintains may be invalid and we may try to 
			// use it before the delayed invalidation takes place.
			//
			this.TemplateDataRecordCache.BumpCacheVersion(true);
            
			// invalidate the generated styles

			// JJD 8/1/07 - BR25088
			// Call the InvalidateGeneratedStyles method off the datapresenter instead so that we
			// trigger a refresh of all the elements currently in the visual tree
			//this.InvalidateGeneratedStyles(true);
            if (this._presenter != null && this._presenter.IsInitialized)
            {
                // JJD 2/4/09 - TFS11575
                // Do an asynchronously invalidation of the generate styles instead of a synchronous
                //this._presenter.InvalidateGeneratedStyles(true, false);

                // JJD 06/01/10 - TF33273
                // Instead of calling PostDelayedInvalidation which will re-create the templates
                // call InvalidateGeneratedStylesAsnyc and pass true as the 2nd param - regenerateTemplates,
                // only if this is a Reset. Otherwise, preserve the templates so we can re-use any
                // existing CellValuePresenters
                //    this.PostDelayedInvalidation();
                this.InvalidateGeneratedStylesAsnyc(true, e.Action == NotifyCollectionChangedAction.Reset);

				// SSP 4/5/12 TFS107483
				// 
				GridUtilities.NotifyCalcAdapter( _presenter, this, "Fields", null );
             }
		}

				#endregion //OnFieldCollectionChanged	
        
				#region OnSettingsPropertyChanged

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			// SSP 4/23/08 - Summaries Functionality
			// Added infrastructure so field, field layout and the data presenter can each process
			// change in field settings of any of these objects. Previous code is moved into the 
			// new ProcessFieldLayoutSettingsPropertyChanged method.
			// ------------------------------------------------------------------------------------------
			DataPresenterBase presenter = this.DataPresenter;
			FieldLayout.ProcessFieldLayoutSettingsPropertyChanged( e, _settings, this, null != presenter, this, presenter );
			
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------------------

			// AS 5/12/10 Sl-WPF Sharing
			// NestedPropertyChangedEventArgs is obsolete.
			//
			//this.RaisePropertyChangedEvent("Settings", sender, e);
        }

                #endregion //OnSettingsPropertyChanged	
    
				#region PostDelayedInvalidation

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

        // JJD 10/30/08 - TFS7858
        // Changed to internal 
		//private void PostDelayedInvalidation()
		internal void PostDelayedInvalidation()
		{
            // JJD 10/30/08 - TFS7858
            // Only invalidate the styles if we aren't in the the middle of
            // initializing the fields 
            //if (this._presenter != null)
			if (this._presenter != null && this._isInFieldInitialization == false)
			{
                // JJD 7/21/08 - BR34098 - Optimization
                // Call the new InvalidateGeneratedStylesAsync method instead so we centtralize the asynch refresh logic
                //this._invalidationPending = true;
                //this._presenter.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MethodDelegate(OnDelayedInvalidation));
                this._presenter.InvalidateGeneratedStylesAsync();
			}
		}

				#endregion //PostDelayedInvalidation
		
				// JJD 4/27/11 - TFS73888 - added
				#region ProcessPendingFieldsToRefresh

		private void ProcessPendingFieldsToRefresh()
		{
			List<Field> fields = new List<Field>();

			// copy the pending fields into a stack array
			foreach (Field field in _pendingFieldsToRefresh)
				fields.Add(field);

			// null out the _pendingFieldsToRefresh
			_pendingFieldsToRefresh = null;

			DataPresenterBase dp = this.DataPresenter;

			if (dp == null)
				return;

			// get the records in view
			Record[] rcdsInView = dp.GetRecordsInView(true);

			HashSet rcdsToRefresh = new HashSet();

			// add all the datarecords in view from this field layout
			// to a hashset
			foreach (Record rcd in rcdsInView)
			{
				DataRecord dr = rcd as DataRecord;

				if (dr == null || dr.FieldLayout != this)
					continue;

				rcdsToRefresh.Add(dr);

			}

			DataRecord activeRecord = dp.ActiveRecordInternal as DataRecord;

			// make sure the active record is included 
			if (activeRecord != null && activeRecord.AssociatedRecordPresenter != null)
				rcdsToRefresh.Add(activeRecord);

			// refresh the set of fields on each data record
			foreach (DataRecord drToRefresh in rcdsToRefresh)
				drToRefresh.RefreshCellValues(fields, true);

			// bump the filter version
			this.BumpRecordFilterVersion();

			// bump the data version on the summary definitions if they exist
			if (this._summaryDefinitions != null)
				_summaryDefinitions.BumpDataVersion();
		}

				#endregion //ProcessPendingFieldsToRefresh	

				#region ProcessPropertyDescriptorsChanged

		// SSP 9/26/11 TFS86720
		// Added ProcessPropertyDescriptorsChanged method.
		// 
		private void ProcessPropertyDescriptorsChanged( PropertyDescriptorProvider provider, bool postInvalidation )
		{
			if ( !this.AutoGenerateFieldsResolved )
				return;

			FieldCollection fieldCollection = this.Fields;

			Dictionary<string, PropertyDescriptor> props = new Dictionary<string, PropertyDescriptor>( );
			foreach ( PropertyDescriptor ii in provider.GetProperties( ) )
			{
				props[ii.Name] = ii;
			}

			Dictionary<string, Field> fields = new Dictionary<string, Field>( );
			List<Field> fieldsToRemove = new List<Field>( );

			int totalAutogeneratedFields = 0;

			foreach ( Field ii in fieldCollection )
			{
				if ( !ii.IsUnbound )
				{
					PropertyDescriptor pd;
					if ( props.TryGetValue( ii.Name, out pd ) )
					{
						ii.OnPropertyDescriptorChanged( pd, provider );

						fields[ii.Name] = ii;

						if ( ii.AutoGenerated )
							totalAutogeneratedFields++;
					}
					else
					{
						fieldsToRemove.Add( ii );
					}
				}
			}

			foreach ( KeyValuePair<string, PropertyDescriptor> ii in props )
			{
				if ( !fields.ContainsKey( ii.Key ) && totalAutogeneratedFields <= this.MaxFieldsToAutoGenerateResolved )
				{
					this.OnPropertyDescriptorAdded( ii.Value, provider );
					totalAutogeneratedFields++;
				}
			}

			foreach ( Field ii in fieldsToRemove )
			{
				fieldCollection.Remove( ii );
			}
		}

				#endregion // ProcessPropertyDescriptorsChanged
    
                // JJD 5/18/09 - NA 2009 vol 2 - Cross band grouping - added
                #region PropertyDescriptorAddedHelper

        internal void PropertyDescriptorAddedHelper(PropertyDescriptor propertyDescriptor,
                                                PropertyDescriptorProvider provider,
                                                bool autoGenerate,
                                                bool postInvalidation)
        {
            // see if we already have a field for this descriptor
            Field field = this.Fields.GetFieldFromPropertyDescriptor(propertyDescriptor);

            // if we do then treat the notificatuon as a change and return
            if (field != null)
            {
                this.PropertyDescriptorChangedHelper(propertyDescriptor, provider, false, postInvalidation);
                return;
            }

            if (autoGenerate == false)
                return;

			// JJD 6/23/11 - TFS36572
			// Before we add a field that is an IEnumrable we need to verify that the provider contains
			// the property descriptor. This is to handle a stiuation when dealing with a DataView where
			// a new relation is being added. Their implementation sends out an add notification on every
			// descendant relation. We need to ignore these extra notifications.
			Type propType = propertyDescriptor.PropertyType;
			if (propType != null)
			{
				Type upropType = Utilities.GetUnderlyingType(propType);

				if (upropType != typeof(string) && 
					typeof(IEnumerable).IsAssignableFrom(upropType))
				{
					PropertyDescriptorCollection props = provider.GetProperties();

					// JJD 6/23/11 - TFS36572
					// if the prop descriptor is not in the providers collection of props then return
					if (!props.Contains(propertyDescriptor))
						return;
				}
			}


            int totalFieldsGenerated = 0;

            this.InitializeFieldHelper(propertyDescriptor, provider.Source, new Field[0], true, ref totalFieldsGenerated);

            // post a delayed invalidation so we don't do multiple synchronous
            // invalidations for a single change
            if ( postInvalidation )
                this.PostDelayedInvalidation();
        }

                #endregion //PropertyDescriptorAddedHelper	

				#region PropertyDescriptorChangedHelper

        // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
        // Added postInvalidation param
		private void PropertyDescriptorChangedHelper(PropertyDescriptor propertyDescriptor,
													PropertyDescriptorProvider provider,
													bool deleted,
                                                    bool postInvalidation)
		{
			bool fieldUsedByOtherProviders = false;

			// SSP 9/26/11 TFS86720
			// Apparently DataTable raises PropertyDescriptorChanged with null e.PropertyDescriptor. In which case
			// we need to process it as if anything could've changed.
			// 
			if ( null == propertyDescriptor )
			{
				this.ProcessPropertyDescriptorsChanged( provider, postInvalidation );
				return;
			}

			// find the matching field
			Field field = this.Fields.GetFieldFromPropertyDescriptor(propertyDescriptor);

            // JJD 5/18/09
            // There may not be a field for this descriptor. If not then return
			if ( field == null )
				return;
            
			bool dataTypeChanged = false;

			// find out if the data type has changed if this is not a deletion
			if (deleted == false)
			{
				Type existingDataType = field != null ? field.DataType : null;

				dataTypeChanged = (existingDataType != null &&
										 existingDataType != typeof(object) &&
										 existingDataType != propertyDescriptor.PropertyType);

			}

			// loop over the currently cached providers to look for a match.
			// This is done backwards to optimize the removal of weak references
			// that are no longer alive
			for (int i = this._propertyDescriptorProviders.Count - 1; i >= 0; i--)
			{
				WeakReference weakReference = this._propertyDescriptorProviders[i];

				LayoutPropertyDescriptorProvider layoutProvider = Utilities.GetWeakReferenceTargetSafe(weakReference) as LayoutPropertyDescriptorProvider;

				// if the weak reference is not alive then remove it from the collection
				if (layoutProvider == null)
					this._propertyDescriptorProviders.RemoveAt(i);
				else
				{
					if (layoutProvider.Provider == provider)
					{
						layoutProvider.Dirty();

						if (deleted == true)
						{
							// if we didn't find one or the field wasn't auto generated the 
							if (field == null ||
								field.AutoGenerated == false)
								return;
						}
					}
					else
					{
						if (field != null &&
							field.AutoGenerated == true)
						{
							FieldDescriptor fd = layoutProvider.GetFieldDescriptor(field);
							if (fd != null)
							{
								fieldUsedByOtherProviders = true;
								break;
							}
						}
					}
				}
			}

			if (deleted == true)
			{
				// if it is still being used by another provider then leave it alone
				if (!fieldUsedByOtherProviders)
				{
					// remove the field from the collection
					this.Fields.Remove(field);
				}

				// post a delayed invalidation so we don't do multiple synchronous
				// invalidations for a single change
				this.PostDelayedInvalidation();
				
				return;
			}

            if (dataTypeChanged && fieldUsedByOtherProviders)
            {
                // since we can't modify it when that would conflict with the other
                // providers we need to clone it
                Field clone = field.Clone(true);
                clone.IsPrimary = false;
                clone.OnPropertyDescriptorChanged(propertyDescriptor, provider);
                this.Fields.Add(clone);
            }
            else
            {
                field.OnPropertyDescriptorChanged(propertyDescriptor, provider);

				// JJD 6/27/11 - TFS36572
				// Bump the PropertyDescriptorVersion on the field
				field.BumpPropertyDescriptorVersion();
            }

			// post a delayed invalidation so we don't do multiple synchronous
			// invalidations for a single change
            // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
            // Added postInvalidation param
            if ( postInvalidation )
                this.PostDelayedInvalidation();
		}

				#endregion //PropertyDescriptorChangedHelper	

				// AS 6/22/09 NA 2009.2 Field Sizing
				#region ResolveAutoFit
		private static AutoFitState ResolveAutoFit(FieldLayout fl)
		{
			DataPresenterBase dp = fl._presenter;

			if (null == dp)
				return AutoFitState.None;

			AutoFitState state = AutoFitState.None;
			ViewBase view = dp.CurrentViewInternal;

			// if the records are horizontal (and therefore the fields vertically) and we are 
			// being asked if the height is autofit - or the records are not horizontal and we are
			// being asked if the width is autofit then we need to check the resolved autofitmode
			AutoFitMode mode = fl.GetResolvedValue<AutoFitMode>(FieldLayoutSettings.AutoFitModeProperty, AutoFitMode.Default, AutoFitMode.Default);

			if (mode == AutoFitMode.Default)
			{
				if (dp.AutoFitResolved)
					state = AutoFitState.WidthAndHeight;
				else if (dp.AutoFit == null)
					mode = AutoFitMode.OnlyWithVisibleStarFields;
				else
					mode = AutoFitMode.Never;
			}

			switch (mode)
			{
				case AutoFitMode.OnlyWithVisibleStarFields:
					if (fl.HasStarFields(true))
						state |= AutoFitState.Width;

					if (fl.HasStarFields(false))
						state |= AutoFitState.Height;
					break;
				case AutoFitMode.Never:
					state = AutoFitState.None;
					break;
				case AutoFitMode.Always:
				case AutoFitMode.ExtendLastField:
					state = AutoFitState.WidthAndHeight;
					break;
			}

			// make sure the view supports it at all
			if (!view.IsAutoFitWidthSupported)
				state &= ~AutoFitState.Width;

			if (!view.IsAutoFitHeightSupported)
				state &= ~AutoFitState.Height;

			return state;
		}
				#endregion //ResolveAutoFit

                // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
                #region VerifyLayoutManagers
        private void VerifyLayoutManagers()
        {
            if (this._verifiedLayoutManagerVersion != this.LayoutManagerVersion)
            {
                this._verifiedLayoutManagerVersion = this.LayoutManagerVersion;

                if (null == this._cellLayoutManager)
                    this._cellLayoutManager = GridUtilities.CreateGridBagLayoutManager(this, LayoutManagerType.Record);

                if (null == this._labelLayoutManager)
                    this._labelLayoutManager = GridUtilities.CreateGridBagLayoutManager(this, LayoutManagerType.Header);

            }

            // we should always make sure the manager we hand back is up to date
            this._labelLayoutManager.VerifyLayout();
            this._cellLayoutManager.VerifyLayout();
        } 
                #endregion //VerifyLayoutManagers

			#endregion //Private Methods

			#region Public Methods

				#region EnsureUniqueFieldPositions

		// SSP 9/2/09 TFS17893
		// Made ActualPosition on the Field settable and also added EnsureUniqueFieldPositions 
		// method here.
		// 
		/// <summary>
		/// Ensures fields have unique positions so they don't overlap.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>EnsureUniqueFieldPositions</b> method makes sure that fields don't overlap 
		/// by moving overlapping fields as necessary. You would typically use this method
		/// after changing a field's position in code via field's <see cref="Field.ActualPosition"/>
		/// property.
		/// </para>
		/// </remarks>
		/// <seealso cref="Field.ActualPosition"/>
		public void EnsureUniqueFieldPositions( )
		{
			LayoutInfo layoutInfo = this.GetFieldLayoutInfo( false, false );
			if ( null != layoutInfo )
			{
				bool changed = layoutInfo.EnsureItemsDontOverlap( );
				this.SetFieldLayoutInfo( layoutInfo, true, false );

				if ( changed )
					this.InvalidateGeneratedStylesAsnyc( false, false );
			}
		}

				#endregion // EnsureUniqueFieldPositions

				// JM 12-08 Added for RecordFiltering feature.
				#region ShowCustomFilterSelectionControl

		/// <summary>
		/// Displays the <see cref="CustomFilterSelectionControl"/> inside an appropriate container depending on whether the application is running
		/// locally or inside a browser as an XBAP.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When running locally a modal top level window will be created as the container to host the <see cref="CustomFilterSelectionControl"/>.  
		/// When running in the browser as an XBAP application, a special control that simulates a window is created as the container
		/// to get around the lack of top level window support in XBAP applications (modality is also simulated in this case).
		/// </para>
		/// </remarks>
		/// <param name="recordFilter">The Field that the filters are being selected for (required)</param>
		/// <param name="recordManager">The <see cref="RecordManager"/> that should be used to determine the unique values to display for the current <see cref="Field"/> (optional).  but should be specified if the associated FieldLayout’s FieldLayoutSettings.RecordFilterScope property is set to SiblingDataRecords.  This allows the <see cref="CustomFilterSelectionControl"/> to properly display the unique data for the <see cref="Field"/> from the proper scope.  If the <see cref="RecordManager"/> is not specified unique values for the <see cref="Field"/> across all data islands will be displayed.</param>
		/// <seealso cref="CustomFilterSelectionControl"/>
		/// <seealso cref="Field"/>
		/// <seealso cref="RecordManager"/>
		public void ShowCustomFilterSelectionControl(RecordFilter recordFilter, RecordManager recordManager)
		{





            GridUtilities.ValidateNotNull(recordFilter, "recordFilter");

			if (this.Fields.Contains(recordFilter.Field) == false)
                throw new ArgumentException(DataPresenterBase.GetString("LE_MismatchedFieldLayoutInRecordFilter", recordFilter.Field.Name));


			// JM 01-21-09 TFS12702 - Raise the CustoFilterSelectionControlOpening event.
			CustomFilterSelectionControl					cfsc	= new CustomFilterSelectionControl(recordFilter, recordManager);
			CustomFilterSelectionControlOpeningEventArgs	args	= new CustomFilterSelectionControlOpeningEventArgs(recordFilter, cfsc);
			this.DataPresenter.RaiseCustomFilterSelectionControlOpening(args);
			if (args.Cancel)
				return;


			ToolWindow			toolWindow		= new ToolWindow();
			FrameworkElement	feOwner			= this.DataPresenter;
			toolWindow.Content					= cfsc;

            
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


            // JM 02-17-09 TFS 13977
			//toolWindow.VerticalAlignmentMode	= ToolWindowAlignmentMode.UseAlignment;
			//toolWindow.VerticalAlignment		= VerticalAlignment.Center;
			//toolWindow.HorizontalAlignmentMode= ToolWindowAlignmentMode.UseAlignment;
			//toolWindow.HorizontalAlignment	= HorizontalAlignment.Center;

			// JM 6-2-09 TFS 17793 - Allow resizing.
			//toolWindow.ResizeMode				= ResizeMode.NoResize;
			toolWindow.ResizeMode				= ResizeMode.CanResizeWithGrip;
			toolWindow.Height					= 380;

            // AS 3/9/09 TFS13972
            // Get the title from the control's publicly exposed property.
            //
            //toolWindow.Title					= "Custom Filter Selection";
            toolWindow.SetBinding(ToolWindow.TitleProperty, Utilities.CreateBindingObject(CustomFilterSelectionControl.TitleDescriptionProperty, BindingMode.OneWay, cfsc));

			toolWindow.Resources				= this.DataPresenter.Resources;

            // AS 3/12/09 TFS15327
            // This isn't specific to this issue but we should focus the first button
            // automatically.
            //
            toolWindow.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                DispatcherOperationCallback callback = delegate(object param)
                {
                    ((ToolWindow)param).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    return null;
                };

                // loaded is too early but we can delay setting focus to the first element
                ((ToolWindow)sender).Dispatcher.BeginInvoke(DispatcherPriority.Loaded, callback, sender);
            };

            // AS 3/12/09 TFS15327
            // This isn't specific to this issue but the CustomFilterSelectionControl shouldn't 
            // assume it should cancel the closing of a containing toolwindow. Instead we'll make
            // that specific to our showing of this control.
            //
            toolWindow.Closing += delegate(object sender, CancelEventArgs e)
            {
                ToolWindow tw = (ToolWindow)sender;
                CustomFilterSelectionControl filterCtrl = toolWindow.Content as CustomFilterSelectionControl;

                // If the sender is a ToolWindow and we haven't already processed a DialogResult generated by the OK or Cancel button (i.e., the ToolWindow's
                // DialogResult is null), and the dialog is 'dirty', display a MessageBox asking the user to confirm the exit.
                if (tw.DialogResult == null &&
                    filterCtrl != null &&
                    filterCtrl.IsDirty == true)
                {
                    MessageBoxResult result = Utilities.ShowMessageBox(this,
                        filterCtrl.CancelMessageText,
                        filterCtrl.CancelMessageTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        e.Cancel = true;
                }
            };

			toolWindow.ShowDialog(feOwner, new ToolWindow.ShowDialogCallback(OnCustomFilterDialogClosed));

		}

				#endregion //ShowCustomFilterSelectionControl	
    
			#endregion //Public Methods
		
		#endregion //Methods

		#region LayoutPropertyDescriptorProvider internal class

		// this object keeps track of the FieldDescriptors for a specific
		// FieldLayout and a PropertyDescriptorProvider
		internal class LayoutPropertyDescriptorProvider
		{
			#region Private Members

			private FieldLayout _fieldLayout;
			private PropertyDescriptorProvider _provider;
			private FieldDescriptor[] _fieldDescriptors;
			private int _fieldDescriptorVersion;

			// SSP 3/10/10 TFS26510
			// 
			private string[] _unboundFieldsWithPropertyDescriptor;
			private Dictionary<string, UnboundField> _unboundFieldsWithPropertyDescriptorMap;

			#endregion //Private Members	
    
			#region Constructor

			internal LayoutPropertyDescriptorProvider(FieldLayout fieldLayout, PropertyDescriptorProvider provider)
			{
				this._fieldLayout = fieldLayout;
				this._provider = provider;
 			}

			#endregion //Constructor	
    
			#region Properties

				#region Internal Properties

			internal FieldLayout FieldLayout { get { return this._fieldLayout; } }
			internal PropertyDescriptorProvider Provider { get { return this._provider; } }

				#endregion //Internal Properties	
    
				#region Private Properties

			private FieldDescriptor[] FieldDescriptors
			{
				get
				{
					this.VerifyFieldDescriptors();
					return this._fieldDescriptors;
				}
			}

				#endregion //Private Properties	
    
			#endregion //Properties	
    
			#region Methods

				#region Internal Methods

					#region Dirty

			internal void Dirty()
			{
				this._fieldDescriptorVersion--;
			}

					#endregion //Dirty	
    
					#region GetFieldDescriptor

			internal FieldDescriptor GetFieldDescriptor(Field field)
			{
				int index = field.Index;

				Debug.Assert(index >= 0);
				Debug.Assert(field.Owner == this._fieldLayout);

				if (index < 0 ||
					 field.Owner != this._fieldLayout)
					return null;

				return this.FieldDescriptors[index];
			}

					#endregion //GetFieldDescriptor	

					#region HasPropertyDescriptorFor

			// SSP 3/10/10 TFS26510
			// If there's a summary for unbound field then we need to maintain the bindings for
			// getting values so we get notified when the value changes. We can optimize this
			// by checking to see if there's an underlying property that we'll get a property
			// change notification for in which case we don't need to maintain the bindings
			// for unbound cells.
			// 

			internal bool HasPropertyDescriptorFor( UnboundField field )
			{
				this.VerifyFieldDescriptors( );

				int index = field.Index;

				Debug.Assert( null == _unboundFieldsWithPropertyDescriptor 
					|| index >= 0 && index < _unboundFieldsWithPropertyDescriptor.Length );

				if ( null != _unboundFieldsWithPropertyDescriptor
					&& index >= 0 && index < _unboundFieldsWithPropertyDescriptor.Length )
					return null != _unboundFieldsWithPropertyDescriptor[index];

				return false;
			}

			internal static bool HasPropertyDescriptorFor( UnboundCell cell )
			{
				UnboundField field = cell.Field as UnboundField;
				DataRecord record = cell.Record;

				LayoutPropertyDescriptorProvider provider = null != record ? record.PropertyDescriptorProvider : null;

				return null != provider && provider.HasPropertyDescriptorFor( field );
			}

			internal static UnboundField GetUnboundFieldWithPropertyDescriptor( DataRecord record, string propertyDescriptorName )
			{
				LayoutPropertyDescriptorProvider provider = record.PropertyDescriptorProvider;
				if ( null != provider )
				{
					provider.VerifyFieldDescriptors( );

					Dictionary<string, UnboundField> map = provider._unboundFieldsWithPropertyDescriptorMap;
					
					UnboundField ret;
					if ( null != map && map.TryGetValue( propertyDescriptorName, out ret ) )
						return ret;
				}

				return null;
			}

			internal static bool HasUnboundFieldWithPropertyDescriptor( DataRecord record )
			{
				LayoutPropertyDescriptorProvider provider = record.PropertyDescriptorProvider;

				if ( null != provider )
				{
					provider.VerifyFieldDescriptors( );
					
					Dictionary<string, UnboundField> map = provider._unboundFieldsWithPropertyDescriptorMap;
					
					return null != map && map.Count > 0;
				}

				return false;
			}
    
					#endregion // HasPropertyDescriptorFor

				#endregion //Internal Methods	
    
				#region Private Methods

					#region VerifyFieldDescriptors

			// JM 07-29-09 TFS 19241 - Make this internal so we can call it from the new FieldLayout.VerifyAllPropertyDescriptorProviders
			//private void VerifyFieldDescriptors()
			internal void VerifyFieldDescriptors()
			{

				FieldCollection fieldCollections = this._fieldLayout.Fields;

				// if the version of the fields collection hasn't changed then return

				// JJD 3/09/07 BR20924
				// Changed for 'or' to 'and' so we actually undate the descriptors a fields collection change
				//if (this._fieldDescriptors != null ||
				if (this._fieldDescriptors != null &&
					this._fieldDescriptorVersion == fieldCollections.Version)
					return;

				// cache the Field collection's version number
				this._fieldDescriptorVersion = fieldCollections.Version;

				// get the count of the fields
				int fieldCount = fieldCollections.Count;

				// copy the field into an array
				Field[] fields = new Field[fieldCount];
				((IList<Field>)(fieldCollections)).CopyTo(fields, 0);

				// create a stack variable to hold the old FieldDescriptors
				FieldDescriptor[] oldFieldDescriptors = this._fieldDescriptors;

				// allocate an array to hold the new field descriptors
				// based on the fiel count since they are mapped based on
				// the field's index in its collection
				this._fieldDescriptors = new FieldDescriptor[fieldCount];

				// JJD 06/22/11 - TFS34793
				// Added a stack bool to keep track if we have initialized a 
				// property descriptor for any field
				bool propertyDescriptorInitialized = false;

				// JJD 11/4/11 - TFS85469
				// Wrap the call to GetProperties in a try/catch incase
				// the list has been disposed and whoever is supplying the
				// list of property descriptors decides to throw an exception
				//PropertyDescriptorCollection pds = this._provider.GetProperties();
				PropertyDescriptorCollection pds = null;
				try
				{
					pds = this._provider.GetProperties();
				}
				catch (Exception)
				{
					pds = new PropertyDescriptorCollection(null);
				}

				PropertyDescriptor pd;
				FieldDescriptor fieldDescriptor, oldFieldDescriptor;
				Field field;
				int pdCount = pds.Count;
				for (int i = 0; i < pdCount; i++)
				{
					pd = pds[i];

					// see if we can reuse the old field descriptor at this index

                    // JJD 7/23/08 - BR34821
                    // Use a stack variable to hold the old field descriptor so
                    // we don't have to repeatedly use the indexer
					if (oldFieldDescriptors != null &&
						 oldFieldDescriptors.Length > i )
                        oldFieldDescriptor = oldFieldDescriptors[i];
                    else
                        oldFieldDescriptor = null;

                    // JJD 7/23/08 - BR34821
                    // do a null ref check on oldFieldDescriptor.Field so an exception isn't thrown.
                    // This can happen after fields are removed from the Fields collection
                    //if (oldFieldDescriptors != null &&
                    //     oldFieldDescriptors.Length > i &&
                    //     oldFieldDescriptors[i].PropertyDescriptor == pd &&
                    //     oldFieldDescriptors[i].Field.Index >= 0)
                    if (oldFieldDescriptor != null &&
						 oldFieldDescriptor.PropertyDescriptor == pd &&
                         oldFieldDescriptor.Field != null &&
						 oldFieldDescriptor.Field.Index >= 0)
					{
						fieldDescriptor = oldFieldDescriptors[i];
						field = fieldDescriptor.Field;
						// clear the array slot so we don't reuse the field and
						// can optimize the search for trailing descriptors
						fields[field.Index] = null;
					}
					else
					{
						fieldDescriptor = null;
						field = null;

						// look for a field whose name matches exactly (case sensitive)
						for (int j = 0; j < fieldCount; j++)
						{
                            // JJD 7/23/08 - BR34821
                            // Use a stack variable to hold the field so
                            // we don't have to repeatedly use the indexer
                            //if (fields[j] != null &&
                            //    fields[j].DoesPropertyMatch(pd))
                            //{
                            //    field = fields[j];
                            Field fieldToTest = fields[j];

                            if (fieldToTest != null &&
                                fieldToTest.DoesPropertyMatch(pd))
							{
                                field = fieldToTest;

								// clear the array slot so we don't reuse the field and
								// can optimize the search for trailing descriptors
								fields[j] = null;
								break;
							}
						}
					}

					// if we aren't reusing a old descriptor and we found a matching
					// field then create a FieldDescriptor now
					if (fieldDescriptor == null && field != null)
					{
						fieldDescriptor = new FieldDescriptor(field, pd, this._provider);

						// JM 07-29-09 TFS 19241
						if (this._fieldLayout.AutoGenerateFieldsResolved			== false	&&
							this._fieldLayout._fieldLayoutInitializedEventRaised	== true		&&
							field.IsUnbound											== false	&&
							field.IsPropertyDescriptorInitialized					== false)
						{
							field.DataType = pd.PropertyType;

							// SSP 9/26/11 TFS86720
							// Added reinitialize parameter. Pass true for it.
							// 
							//field.InitializePropertyDescriptor(pd);
							field.InitializePropertyDescriptor( pd, true );

							// JJD 06/22/11 - TFS34793
							// Set the flag so we know that we have intialized a 
							// property descriptor for a field
							propertyDescriptorInitialized = true;

                            
                            
                            
							// JJD 1/12/11 - TFS60770
							// Moved below so the default label is always initialized
							//field.InitializeDefaultLabel(this._fieldLayout.GetLabelFromPropertyDescriptor(pd, this.Provider.Source));
						}
						// JJD 1/12/11 - TFS60770
						// Always InitializeDefaultLabel
						field.InitializeDefaultLabel(this._fieldLayout.GetLabelFromPropertyDescriptor(pd, this.Provider.Source));
					}

					// add it to the array so that its index matches the field's
					// index in the Fields collection
					if (fieldDescriptor != null)
						this._fieldDescriptors[field.Index] = fieldDescriptor;
				}

				// SSP 3/10/10 TFS26510
				// 
				
				_unboundFieldsWithPropertyDescriptor = null;
				_unboundFieldsWithPropertyDescriptorMap = null;

				for ( int i = 0; i < fieldCount; i++ )
				{
					UnboundField ubField = fields[i] as UnboundField;
					if ( null != ubField )
					{
						PropertyPath bindingPath = ubField.BindingPath;
						if ( null != bindingPath && null == ubField.Binding )
						{
							string path = bindingPath.Path;
							pd = pds.Find( path, false );

							if ( null != pd )
							{
								// Check the map to see if there are multiple unbound fields with the same
								// property path in which case only use the first unbound field. Other unbound
								// fields will not use this optimization and will have to maintain bindings.
								// This is because the existing logic for responding to property change 
								// notifications expects a single field associated with a property.
								// 
								if ( ( null == _unboundFieldsWithPropertyDescriptorMap || ! _unboundFieldsWithPropertyDescriptorMap.ContainsKey( path ) )
									&& fieldCollections.IndexOf( path ) < 0 )
								{
									if ( null == _unboundFieldsWithPropertyDescriptorMap )
									{
										_unboundFieldsWithPropertyDescriptor = new string[fieldCount];
										_unboundFieldsWithPropertyDescriptorMap = new Dictionary<string, UnboundField>( );
									}

									_unboundFieldsWithPropertyDescriptor[i] = path;
									_unboundFieldsWithPropertyDescriptorMap[path] = ubField;
								}
							}
						}
					}
				}
				// JJD 06/22/11 - TFS34793
				// If we have intialized any field then call ApplyCustomizations 
				if (propertyDescriptorInitialized)
				{
					DataPresenterBase dp = _fieldLayout != null ? _fieldLayout.DataPresenter : null;

					if (dp != null)
						dp.CustomizationsManager.ApplyCustomizations();
				}
				
			}

					#endregion //VerifyFieldDescriptors

				#endregion //Private Methods	
        
			#endregion //Methods	

		}

		#endregion //LayoutPropertyDescriptorProvider internal class	

		#region FieldDescriptor internal class

		// this object maps a field with a property descriptor
		internal class FieldDescriptor
		{
			#region Private Members

			private Field _field;
			private PropertyDescriptor _propertyDescriptor;
			private DataColumn _column;

			#endregion //Private Members	
    
			#region Constructor

			internal FieldDescriptor(Field field, PropertyDescriptor propertyDescriptor, PropertyDescriptorProvider provider)
			{
				this._field = field;
				this._propertyDescriptor = propertyDescriptor;

				// JJD 3/17/07
				// Cache the underlying DataColumn if it exists
				DataColumnPropertyDescriptor dcpd = this._propertyDescriptor as DataColumnPropertyDescriptor;
				if (dcpd != null)
					this._column = dcpd.Column;
				else
				{
					DataViewPropertyDescriptorProvider dataViewProvider = provider as DataViewPropertyDescriptorProvider;

					if (dataViewProvider != null)
					{
						if (dataViewProvider.View != null &&
							 dataViewProvider.View.Table != null)
						{
							DataColumnCollection dataColumns = dataViewProvider.View.Table.Columns;

							if (dataColumns != null &&
								 dataColumns.Contains(propertyDescriptor.Name))
								this._column = dataColumns[propertyDescriptor.Name];
						}
					}
				}
			}

			#endregion //Constructor	
    
			#region Properties

				#region DataColumn

			internal DataColumn DataColumn
			{
				get
				{
					return this._column;
				}
			}

				#endregion //DataColumn

				#region Field

			internal Field Field { get { return this._field; } }

				#endregion //Field	
	    
				#region PropertyDescriptor

			internal PropertyDescriptor PropertyDescriptor { get { return this._propertyDescriptor; } }

				#endregion //PropertyDescriptor	
    
			#endregion //Properties	
    
		}

		#endregion //FieldDescriptor internal class

		#region SizeHolderManager internal class

        
#region Infragistics Source Cleanup (Region)

























































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //SizeHolderManager internal class
	}

    #endregion FieldLayout
}

namespace Infragistics.Windows.DataPresenter.Internal
{
	#region SizeHolder public class

    
#region Infragistics Source Cleanup (Region)


































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	#endregion //SizeHolder public class

	#region SpannedSizeHolder internal class

    
#region Infragistics Source Cleanup (Region)
























































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	#endregion //SpannedSizeHolder internal class

	#region MergedSizeHolder internal class

    
#region Infragistics Source Cleanup (Region)
































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	#endregion //MergedSizeHolder internal class

	#region TandemCellValueAndLabelSizeHolder internal class

    
#region Infragistics Source Cleanup (Region)





















































































































































#endregion // Infragistics Source Cleanup (Region)

	#endregion //TandemCellValueAndLabelSizeHolder internal class

	#region TemplateDataRecordCache
	internal class TemplateDataRecordCache
	{
		#region Member Variables

		private FieldLayout _fieldLayout;

		private DataRecordPresenter _lastRecordPresenter;
		private DataRecordCellArea _lastCellArea;
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        // We are no longer using a Grid so just look for any panel.
        //
		//private Grid _lastCellGrid;
        private Panel _lastCellPanel;

		// AS 7/7/09 TFS19145
		private Panel _lastLabelPanel;

		private List<CellPlaceholder> _cellPlaceHolders;
		private Dictionary<CellElementKey, Control> _cellElements;

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
		private Dictionary<LabelElementKey, Control> _labelElements;

		// AS 5/1/07 Performance
		//private Dictionary<Field, int> _fieldIndexes;
		private bool _isInitializingCache;
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        // We don't need this info any more.
        //
		//private bool? _hasColumnWithStarSize;
		//private bool? _hasRowWithStarSize;

		// AS 5/4/07 Optimization
		private List<Field> _virtualizedCellFields;
		private List<Field> _unVirtualizedCellFields;

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
		private List<Field> _virtualizedFilterCellFields;
		private List<Field> _unVirtualizedFilterCellFields;
		private Dictionary<FilterCellElementKey, Field> _filterCellFieldMap;

		// JJD 5/4/07 - Optimization
		// Added label virtualization
		private List<Field> _virtualizedLabelFields;
		private List<Field> _unVirtualizedLabelFields;
        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
		//private List<Field> _virtualizedLabelFieldsToMeasure;

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        private List<GridDefinitionLayoutItem> _rowLayoutItems;
        private List<GridDefinitionLayoutItem> _colLayoutItems;
        private Dictionary<Field, Size> _gridDefinitionSizes;
        internal static readonly Size NaNSize = new Size(double.NaN, double.NaN);
        private Size _templateGridSize = NaNSize;

        // AS 1/17/09 NA 2009 Vol 1 - Fixed Fields
        private List<Field> _allFields;

        // JJD 1/19/09 - NA 2009 vol 1 - record filtering
        private double _nearOffsetDataRecord;
        private double _nearOffsetGroupByRecord             = double.NaN;
        private double _nearOffsetExpandableFieldRecord     = double.NaN;
        private double _farOffsetDataRecord;
        private double _farOffsetGroupByRecord              = double.NaN;
        private double _farOffsetExpandableFieldRecord      = double.NaN;

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        private double _nearOffsetVirtualizingPanel;
        private double _farOffsetVirtualizingPanel;

        // AS 3/25/09 TFS15801
        // We need to cache the space between the itemspresenter within the 
        // nestedcontent's recordlistcontrol and the nested content site itself.
        //
        private double _nearOffsetGroupByChrome = double.NaN;
        private double _nearOffsetExpandableFieldChrome = double.NaN;
        private double _nearOffsetDataRecordChrome = double.NaN;
        private double _farOffsetDataRecordChrome = double.NaN;
        private double _farOffsetGroupByChrome = double.NaN;
        private double _farOffsetExpandableFieldChrome = double.NaN;


        // JJD 9/12/08 - added supoport for printing
		private List<Control> _cellElementsInReportOrder;

        // JJD 9/15/08 - added print support
        private Dictionary<double, ReportLayoutInfo> _reportLayoutInfos;
        private Dictionary<int, double> _reportLayoutNestingDepthOffsetCache;
        private double _lastPanelExtent;
        private bool _lastIsHorizontal;
        private CellPageSpanStrategy _lastCellPageSpanStrategy;
        [ThreadStatic()]
        private static double s_lastZeroIndentOffset = 20;


        // AS 2/10/09
        private int _cacheVersion = 1;

        // AS 2/26/09 CellPresenter Chrome
        private Dictionary<CellElementKey, Thickness> _cellPresenterMargins;

		// AS 7/7/09 TFS19145
		private int _verifiedCacheVersion = -1;

		// AS 8/25/09 TFS17560
		private GroupByRecordPresenter _lastGroupByPresenter;

		// JJD 10/21/11 - TFS86028 - Optimization
		// Maintain a cache of auto-fit extents for cell area keyed by indent level 
		// so we can eliminate unnecessary multiple layout passes
		private Dictionary<int, double> _autoFitCellAreaWidthCache;
		private Dictionary<int, double> _autoFitCellAreaHeightCache;

        #endregion //Member Variables

        #region Constructor
        internal TemplateDataRecordCache(FieldLayout fieldLayout)
		{
			this._fieldLayout = fieldLayout;
		}
		#endregion //Constructor

		#region Properties

        // AS 1/17/09 NA 2009 Vol 1 - Fixed Fields
        #region AllFields
        internal List<Field> AllFields
        {
            get
            {
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

                return this._allFields;
            }
        }
        #endregion //AllFields

        // AS 2/10/09
        #region CacheVersion
        internal int CacheVersion
        {
            get
            {
                return _cacheVersion;
            }
        } 
        #endregion //CacheVersion

        // JJD 9/12/08 - added support for printing
        #region CellElementInReportOrder


        public List<Control> CellElementsInReportOrder
        {
            get
            {
                Debug.Assert(this._fieldLayout.DataPresenter.IsReportControl, "CellElementsInReportOrder is only used while printing");

                if (this._cellElementsInReportOrder == null && this._fieldLayout.DataPresenter.IsReportControl)
                {
					
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

					this.VerifyCache();

                    Control[] elements = new Control[this._cellElements.Count];
                    
                    // JJD 10/16/08 - TFS8587
                    // Always copy the elements into the area if there are any
                    if (elements.Length > 0)
                    {
                        this._cellElements.Values.CopyTo(elements, 0);

                        if (elements.Length > 1)
                        {
                            // JJD 10/16/08 - TFS8587
                            // Moved above
                            //this._cellElements.Values.CopyTo(elements, 0);

                            Utilities.SortMerge(elements, new ReportElementComparer(this._fieldLayout.DataPresenter.CurrentViewInternal.LogicalOrientation == Orientation.Vertical));
                        }
                    }

                    this._cellElementsInReportOrder = new List<Control>(elements);

                }

                return this._cellElementsInReportOrder;
            }
        }

        #endregion //CellElementInReportOrder

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region GridColumnLayoutItems
        internal IList<GridDefinitionLayoutItem> GridColumnLayoutItems
        {
            get
            {
                if (this._colLayoutItems == null)
                    this.InitializeGridDefinitionItems();

                return this._colLayoutItems;
            }
        } 
        #endregion //GridColumnLayoutItems

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region GridRowLayoutItems
        internal IList<GridDefinitionLayoutItem> GridRowLayoutItems
        {
            get
            {
                if (this._rowLayoutItems == null)
                    this.InitializeGridDefinitionItems();

                return this._rowLayoutItems;
            }
        }
        #endregion //GridRowLayoutItems

        #region HasColumnDefinitionWithStar
        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        #endregion //HasColumnDefinitionWithStar

        #region HasRowDefinitionWithStar
        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        #endregion //HasRowDefinitionWithStar

        #region IsInitializingCache
        internal bool IsInitializingCache
		{
			get { return this._isInitializingCache; }
		}
		#endregion //IsInitializingCache

		// AS 10/9/09 TFS22990
		#region IsTemplateRecordPanel

		/// <summary>
		/// IsTemplateRecordPanel Attached Dependency Property
		/// </summary>
		internal static readonly DependencyProperty IsTemplateRecordPanelProperty =
			DependencyProperty.RegisterAttached("IsTemplateRecordPanel", typeof(bool), typeof(TemplateDataRecordCache),
				new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
					new PropertyChangedCallback(OnIsTemplateRecordPanelChanged)));

		/// <summary>
		/// Gets the IsTemplateRecordPanel property.  This dependency property 
		/// indicates ....
		/// </summary>
		private static bool GetIsTemplateRecordPanel(DependencyObject d)
		{
			return (bool)d.GetValue(IsTemplateRecordPanelProperty);
		}

		/// <summary>
		/// Sets the IsTemplateRecordPanel property.  This dependency property 
		/// indicates ....
		/// </summary>
		private static void SetIsTemplateRecordPanel(DependencyObject d, bool value)
		{
			d.SetValue(IsTemplateRecordPanelProperty, value);
		}

		/// <summary>
		/// Handles changes to the IsTemplateRecordPanel property.
		/// </summary>
		private static void OnIsTemplateRecordPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				Debug.Assert(d is Panel);
				d.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DispatcherOperationCallback(VerifyTemplateCache), d);
			}
		}

		private static object VerifyTemplateCache(object param)
		{
			Panel panel = param as Panel;

			if (null != panel)
			{
				RecordPresenter rp = Utilities.GetAncestorFromType(panel, typeof(RecordPresenter), true) as RecordPresenter;
				FieldLayout fl = null != rp ? rp.FieldLayout : null;

				if (null != fl)
				{
					fl.TemplateDataRecordCache.VerifyTemplateRecordPanel(panel, rp);
				}
			}

			return null;
		}

		#endregion //IsTemplateRecordPanel

		// AS 5/4/07 Optimization
		#region NonVirtualizedCellFields
		internal List<Field> NonVirtualizedCellFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

				return this._unVirtualizedCellFields;
			}
		}
		#endregion //NonVirtualizedCellFields

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
		#region NonVirtualizedFilterCellFields
		internal List<Field> NonVirtualizedFilterCellFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

				return this._unVirtualizedFilterCellFields;
			}
		}
		#endregion //NonVirtualizedFilterCellFields

		// JJD 5/4/07 - Optimization
		// Added label virtualization
		#region NonVirtualizedLabelFields
		internal List<Field> NonVirtualizedLabelFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

				return this._unVirtualizedLabelFields;
			}
		}
		#endregion //NonVirtualizedLabelFields

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region TemplateGridSize
        internal Size TemplateGridSize
        {
            get
            {
                if (this._rowLayoutItems == null)
                    this.InitializeGridDefinitionItems();

                return this._templateGridSize;
            }
        } 
        #endregion //TemplateGridSize

		// AS 5/4/07 Optimization
		#region VirtualizedCellFields
		internal List<Field> VirtualizedCellFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

				return this._virtualizedCellFields;
			}
		}
		#endregion //VirtualizedCellFields

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        #region VirtualizedFilterCellFields
        internal List<Field> VirtualizedFilterCellFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

                return this._virtualizedFilterCellFields;
			}
        }
        #endregion //VirtualizedFilterCellFields

        // JJD 5/4/07 - Optimization
		// Added label virtualization
		#region VirtualizedLabelFields
		internal List<Field> VirtualizedLabelFields
		{
			get
			{
				// AS 7/7/09 TFS19145
				//if (null == this._lastCellArea)
				//	this.InitializeCachedCellArea();
				this.VerifyCache();

				return this._virtualizedLabelFields;
			}
		}
		#endregion //VirtualizedLabelFields

		// JJD 5/7/07 - Optimization
		// Added label virtualization
		#region VirtualizedLabelFieldsToMeasure
        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		#endregion //VirtualizedLabelFieldsToMeasure

		#endregion //Properties

		#region Methods

		#region Internal

		// AS 7/7/09 TFS19145
		#region BumpCacheVersion
		internal void BumpCacheVersion(bool releaseCache)
		{
			if (releaseCache)
				this.ReleaseCache();
			else
				_cacheVersion++;
		} 
		#endregion //BumpCacheVersion

		// JJD 5/3/07 - Optimization
		#region CacheCellElement
        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        // Added caching of label presenters.
        //
		//internal void CacheCellElement(Field field, Control control)
		internal void CacheCellElement(Field field, Control control, bool label)
		{
            if (label)
            {
                if (null == _labelElements)
                    _labelElements = new Dictionary<LabelElementKey, Control>();

                _labelElements[field.LabelElementKey] = control;
                return;
            }

			if (null == this._cellElements)
				this._cellElements = new Dictionary<CellElementKey, Control>();

			// AS 10/12/09
			// Its possible that we have multiple placeholders for the same cell key.
			//
			//this._cellElements.Add(field.CellElementKey, control);
			_cellElements[field.CellElementKey] = control;


            // JJD 9/12/08 - added printing support
            // null the sorted list out since it will get recreated lazily
            this._cellElementsInReportOrder = null;

        } 
		#endregion //CacheCellElement

		#region EditorAllowsVirtualization
		internal bool EditorAllowsVirtualization(Field field, Orientation? orientation)
		{
			// JJD 5/03/07 - Optimization
			// Changed logic to use cached shared cell elements
			#region Obsolete code

			// AS 5/1/07 Performance
			// This will be done by the call to GetFieldIndex.
			//
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			//
			//int index;
			//
			//if (this._fieldIndexes.TryGetValue(field, out index))
			//int index = this.GetFieldIndex(field);

			//if (index >= 0)
			//{
			//    CellPlaceholder ctrl = this._cellPlaceHolders[index];

			//    // TODO: get cached editor
			//    return true;

			#endregion //Obsolete code	
    
			// AS 5/23/07 BR23119
			// The cache may not have been created yet.
			//
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

			Control ctrl = this.GetCellElement(field);

			if ( ctrl != null )
			{
				CellValuePresenter cvp;

				if (ctrl is CellValuePresenter)
					cvp = (CellValuePresenter)ctrl;
				else if (ctrl is CellPresenter)
					cvp = ((CellPresenter)ctrl).CellValuePresenter;
				else
				{
					Debug.Fail("Unexpected child type!");
					cvp = null;
				}

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                // The Editor may not have been hydrated yet so make sure
                // the template has been applied.
                //
                if (null != cvp)
                {
                    cvp.ApplyTemplate();

                    Debug.Assert(null != cvp.Editor 
                        || field.EditorTypeResolved == null 
                        // added check for missing editor site in case it was retemplated
                        || Utilities.GetTemplateChild<ContentPresenter>(cvp, delegate(ContentPresenter cp) { return cp.Name == "PART_EditorSite"; }) == null
                        );
                }

				if (null != cvp && null != cvp.Editor)
				{
					orientation = orientation ?? (this._fieldLayout.IsHorizontal ? Orientation.Horizontal : Orientation.Vertical);
					return false == cvp.Editor.IsExtentBasedOnValue(orientation.Value);
				}

				// AS 3/14/07
				// Since we're now going to return true if the cell is not
				// in the layout, we want to continue to return false if the cell
				// is in the layout but doesn't have an editor.
				//
				return false;
			}

			// AS 3/14/07
			// If we return false then we're going to end up returning the default
			// cell height. If we don't have the cell we'll consider it to be virtualizable.
			//
			//return false;
			return true;
		}
		#endregion //EditorAllowsVirtualization

		#region GetArrangeSize
        
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetArrangeSize

		// JJD 10/21/11 - TFS86028 - Optimization
		#region GetCachedAutoFitHeight

		// JJD 10/21/11 - TFS86028 - Optimization
		// Maintain a cache of auto-fit Heights for cell area keyed by indent level 
		// so we can eliminate unnecessary multiple layout passes
		internal double? GetCachedAutoFitHeight(Record rcd)
		{
			double height = double.NaN;

			if (_autoFitCellAreaHeightCache != null &&
				_autoFitCellAreaHeightCache.TryGetValue(rcd.NestingDepth, out height) &&
				!double.IsNaN(height))
				return height;

			return null;
		}

		#endregion //GetCachedAutoFitHeight	

		// JJD 10/21/11 - TFS86028 - Optimization
		#region GetCachedAutoFitWidth

		// JJD 10/21/11 - TFS86028 - Optimization
		// Maintain a cache of auto-fit Widths for cell area keyed by indent level 
		// so we can eliminate unnecessary multiple layout passes
		internal double? GetCachedAutoFitWidth(Record rcd)
		{
			double width = double.NaN;

			if (_autoFitCellAreaWidthCache != null &&
				_autoFitCellAreaWidthCache.TryGetValue(rcd.NestingDepth, out width) &&
				!double.IsNaN(width))
				return width;

			return null;
		}

		#endregion //GetCachedAutoFitWidth	
    
		#region GetCellCount
		internal int GetCellCount()
		{
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

			// AS 10/26/10 TFS58064
			if (_cellPlaceHolders == null)
				return 0;

			return this._cellPlaceHolders.Count;
		}
		#endregion //GetCellCount

		// JJD 5/3/07 - Optimization
		#region GetCellElement
        internal Control GetCellElement(Field field)
        {
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            return GetCellElement(field, false);
        }

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        // Added overload so we can cache labels as well.
        //
        internal Control GetCellElement(Field field, bool label)
		{
            if ((!label && null == _cellElements) ||
                (label && null == _labelElements))
				return null;

			Control control;

            if (label)
                _labelElements.TryGetValue(field.LabelElementKey, out control);
            else
    			_cellElements.TryGetValue(field.CellElementKey, out control);

			return control;
		} 
		#endregion //GetCellElement

		#region GetCellPlaceHolder
		internal CellPlaceholder GetCellPlaceHolder(int index)
		{
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

			return this._cellPlaceHolders[index];
		}
		#endregion //GetCellPlaceHolder

		#region GetCellGrid
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetCellGrid

		// AS 12/18/07 BR25223
		// Added helper method so we don't have to do this in several different places.
		//
		#region GetCellPresenter
		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

		// SSP 4/1/09 - Cell Text
		// Made GetCellValuePresenter method internal from private.
		// 
		//private CellValuePresenter GetCellValuePresenter(Field field)
		internal CellValuePresenter GetCellValuePresenter(Field field)
		{
			return GetCellValuePresenter(field, false);
		}

		// AS 7/7/09 TFS19145
		// Added an overload so we can get a template cell for a field that was hidden.
		//
		internal CellValuePresenter GetCellValuePresenter(Field field, bool createIfNull)
		{
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

			FrameworkElement element = this.GetCellElement(field);

			// AS 7/7/09 TFS19145
			// There are certain operations that need to get to the editor of a cell
			// even when the cell is not in view. In these situations we will create a 
			// placeholder. When that placeholder is initialized it will register its 
			// cell element which we can then return.
			//
			if (createIfNull && element == null && _lastCellPanel != null
                // SSP 6/14/10 TFS32985
                // If the field is not part of a field layout then cell value presenter will
                // cause an exception as it tries to access the cell from the record's cells
                // collection for the field. We should not create any element if the field
                // is invalid.
                // 
                && null != field && field.Index >= 0
                )
			{
				CellPlaceholder placeholder = AddPlaceholder(_lastCellPanel, field, false);
				placeholder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				return GetCellValuePresenter(field, false);
			}

			if (element is CellPresenter)
				return ((CellPresenter)element).CellValuePresenter;

			return element as CellValuePresenter;
		}

		private LabelPresenter GetLabelPresenter(Field field)
		{
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

            
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

            LabelPresenter lp = GetCellElement(field, true) as LabelPresenter;

            if (null != lp)
                return lp;

			FrameworkElement element = this.GetCellElement(field);

			if (element is CellPresenter)
				return ((CellPresenter)element).LabelPresenter;

			return element as LabelPresenter;
		}
		#endregion //GetCellPresenter

        // AS 2/26/09 CellPresenter Chrome
        #region GetCellPresenterMargin
        internal Thickness GetCellPresenterMargin(Field field)
        {
            // note we will get into here while the cache is initializing
            // because the cellpresenterlayoutelement's layoutmanager uses 
            // the field's layout item which when initializing its gc will 
            // ask for the margin. since the layout manager used by the 
            // cp does not need (or want) the margin then we can safely
            // return an empty thickness. once the cellareacache is calculated
            // the layout item version is bumped and the real layout items
            // will get back in here to get the updated margin
			if (null == this._cellPresenterMargins && !_isInitializingCache)
			{
				// AS 7/7/09 TFS19145
				//this.InitializeCachedCellArea();
				this.VerifyCache();
			}

            Thickness thickness;

            if (null == this._cellPresenterMargins || !_cellPresenterMargins.TryGetValue(field.CellElementKey, out thickness))
                thickness = new Thickness();

            return thickness;
        } 
        #endregion //GetCellPresenterMargin

		// AS 3/13/07 BR21065
		// We actually need to separate out the cell value presenter and label heights.
		//
		#region GetCellValuePresenterHeight
		internal double GetCellValuePresenterHeight(Field field)
		{
			// JJD 5/03/07 - Optimization
			// Changed logic to use cached shared cell elements
			#region Obsolete code

			//int index = this.GetFieldIndex(field);

			//// AS 3/14/07 BR21115
			//if (index >= 0)
			//{
			//CellPresenter cp = this._cellElements[index] as CellPresenter;

			#endregion //Obsolete code	
    
			
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

			Size size;
            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            // Use the DesiredSize instead of the RenderSize since 
            // the RenderSize may not be updated by the wpf layout 
            // engine yet and really we are using the template to 
            // provide preferred default sizes.
            //
			//if (GetCellValuePresenterSize(field, out size))
			if (GetCellValuePresenterDesiredSize(field, out size))
				return size.Height;

			return double.NaN;
		}
		#endregion //GetCellValuePresenterHeight

		// AS 12/18/07 BR25223
		#region GetCellValuePresenterDesiredSize
		/// <summary>
		/// Returns the desired size of the cell value presenter within a CellPresenter.
		/// </summary>
		internal bool GetCellValuePresenterDesiredSize(Field field, out Size size)
		{
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			CellValuePresenter cvp = this.GetCellValuePresenter(field);

			if (null != cvp)
			{
				size = cvp.DesiredSize;
				return true;
			}

			size = Size.Empty;
			return false;
		}
		#endregion //GetCellValuePresenterDesiredSize

		// AS 12/18/07 BR25223
		#region GetCellValuePresenterSize
        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetCellValuePresenterSize

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region GetDefaultCellWidth
        internal double GetDefaultCellWidth(Field field, double extent)
        {
            if (this._gridDefinitionSizes == null)
                this.InitializeGridDefinitionItems();

            Size gridSize;

            if (this._gridDefinitionSizes.TryGetValue(field, out gridSize) &&
                double.IsNaN(gridSize.Width) == false)
                extent = gridSize.Width;

            return extent;
        }
        #endregion //GetDefaultCellWidth

		#region GetDesiredSize
        
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetDesiredSize

		#region GetField
		internal Field GetField(int index)
		{
			return this.GetCellPlaceHolder(index).Field;
		}
		#endregion //GetField
    
		#region GetFieldIndex
		internal int GetFieldIndex(Field field)
		{
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

			// AS 3/14/07 BR21115
			// Some cells are "visible" but not included in a record by 
			// the view. We need to skip any cells/fields that are not
			// in the template.
			//
			//return this._fieldIndexes[field];
			//int index;

			// AS 5/1/07 Performance
			// Store the index on the field instead of doing a hashtable lookup.
			//
			//if (this._fieldIndexes.TryGetValue(field, out index))
			//	return index;
			//
			//return -1;
			return field.TemplateCellIndex;
		}
		#endregion //GetFieldIndex

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        #region GetFilterFieldToMeasure
        internal Field GetFilterFieldToMeasure(Field field)
        {
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

            Field measureField;
            _filterCellFieldMap.TryGetValue(field.FilterCellElementKey, out measureField);
            return measureField;
        }
        #endregion //GetFilterFieldToMeasure

        // AS 3/13/07 BR21065
		// We actually need to separate out the cell value presenter and label heights.
		//
		#region GetLabelPresenterHeight
		internal double GetLabelPresenterHeight(Field field)
		{
			// JJD 5/03/07 - Optimization
			// Changed logic to use cached shared cell elements
			#region Obsolete code

			//int index = this.GetFieldIndex(field);

			//// AS 3/14/07 BR21115
			//if (index >= 0)
			//{
			//    //CellPresenter cp = this._cellElements[index] as CellPresenter;

			//    //if (null != cp)
			//    //{
			//    //    Debug.Assert(cp.LabelPresenter != null);

			//    //    if (null != cp.LabelPresenter)
			//    //        return cp.LabelPresenter.ActualHeight;
			//    //}
			//}

			#endregion //Obsolete code

			
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

			Size size;

            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            // Use the DesiredSize instead of the RenderSize since 
            // the RenderSize may not be updated by the wpf layout 
            // engine yet and really we are using the template to 
            // provide preferred default sizes.
            //
            //if (GetLabelPresenterSize(field, out size))
			if (GetLabelPresenterDesiredSize(field, out size))
				return size.Height;

			return double.NaN;
		}
		#endregion //GetLabelPresenterHeight

		// AS 12/18/07 BR25223
		#region GetLabelPresenterDesiredSize

		/// <summary>
		/// Returns the desired size of the label presenter within a CellPresenter.
		/// </summary>
		// AS 10/9/09 NA 2010.1 - CardView
		//internal bool GetLabelPresenterDesiredSize(Field field, out Size size)
		private bool GetLabelPresenterDesiredSize(Field field, out Size size)
		{
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			LabelPresenter lp = this.GetLabelPresenter(field);

			if (null != lp)
			{
				// AS 10/9/09 NA 2010.1 - CardView
				// Make sure the label has been measured.
				//
				if (!lp.IsMeasureValid)
					lp.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

				size = lp.DesiredSize;
				return true;
			}

			size = Size.Empty;
			return false;
		}
		#endregion //GetLabelPresenterDesiredSize

		// AS 10/9/09 NA 2010.1 - CardView
		#region GetLabelPresenterWidth
		internal double GetLabelPresenterWidth(Field field)
		{
			// ensure that we have an element specific to this field
			CellPlaceholder placeholder = this.GetPlaceHolder(field, true, true);

			if (null != placeholder)
			{
				Control ctrl = placeholder.ChildCellElement;

				Debug.Assert(null != ctrl);

				if (!ctrl.IsMeasureValid)
				{
					VirtualizingDataRecordCellPanel.ApplyTemplateRecursively(ctrl);
					ctrl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				}

				LabelPresenter lp = AutoSizeFieldHelper.GetCellElement(ctrl, true) as LabelPresenter;

				if (null != lp)
					return lp.DesiredSize.Width;
			}

			return double.NaN;
		} 
		#endregion //GetLabelPresenterWidth

		// AS 12/18/07 BR25223
		#region GetLabelPresenterSize
        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetLabelPresenterSize

        // AS 3/25/09 TFS15801
        #region GetNestedContentChrome
        internal double GetNestedContentChrome(Record record, bool near)
        {
            double chromeOffset = double.NaN;

            if (record is DataRecord)
            {
                chromeOffset = near ? _nearOffsetDataRecordChrome : _farOffsetDataRecordChrome;
            }
            else if (record is GroupByRecord)
            {
                chromeOffset = near ? _nearOffsetGroupByChrome : _farOffsetGroupByChrome;
            }
            else if (record is ExpandableFieldRecord)
            {
                chromeOffset = near ? _nearOffsetExpandableFieldChrome : _farOffsetExpandableFieldChrome;
            }
            else
            {
                Debug.Fail("Unexpected record type");
                return 0;
            }

            if (double.IsNaN(chromeOffset) == false)
                return chromeOffset;

            // get the recrod's associated record presenter
            RecordPresenter rp = record.AssociatedRecordPresenter;

            // If the rp hasn't been arranged yet then return the default since
            // we can't do our point translations accurateyl until it is 
            if (rp == null || rp.NestedContent == null)
                return 0;

            // get the content site
            FrameworkElement feNestedContent = rp.GetNestedContentSite();

            double nearOffset = 0;
            double farOffset = 0;

            if (feNestedContent != null)
            {
                Debug.Assert(rp.NestedContent is RecordListControl);
                RecordListControl rlc = rp.NestedContent as RecordListControl;
                ItemsPresenter ip = rlc != null ? Utilities.GetDescendantFromType(rlc, typeof(ItemsPresenter), true) as ItemsPresenter : null;

                //Debug.Assert(null != ip);

                if (null != ip && ip.IsArrangeValid && feNestedContent.IsArrangeValid)
                {
                    Size sz = ip.DesiredSize;
                    Size feNestedContentSize = feNestedContent.DesiredSize;

                    Point ptTopLeft = ip.TranslatePoint(new Point(), feNestedContent);
                    Point ptBottomRight = ip.TranslatePoint(new Point(sz.Width, sz.Height), feNestedContent);

                    // if the nested content site is fixed and we have scrolled over then 
                    // we need to shift the points back or else we will have adjusted points
                    GeneralTransform tt = feNestedContent.RenderTransform;

                    if (tt != Transform.Identity && tt != null)
                    {
                        ptTopLeft = tt.Transform(ptTopLeft);
                        ptBottomRight = tt.Transform(ptBottomRight);
                    }

                    // In horizonatl mode we need to take the full height of the
                    // groupbyrecord and expandableField record since they are always rendered
                    // horizonatlly
                    if (_fieldLayout.IsHorizontal)
                    {
                        nearOffset = ptTopLeft.Y;
                        farOffset = feNestedContentSize.Height - ptBottomRight.Y;
                    }
                    else
                    {
                        nearOffset = ptTopLeft.X;
                        farOffset = feNestedContentSize.Width - ptBottomRight.X;
                    }

					// AS 6/26/09
					// I got an assert here because the far offset was near 0.
					//
					if (GridUtilities.AreClose(0, nearOffset))
						nearOffset = 0;
					if (GridUtilities.AreClose(0, farOffset))
						farOffset = 0;

                    Debug.Assert(farOffset >= 0 && nearOffset >= 0);

                    if (record is DataRecord)
                    {
                        _nearOffsetDataRecordChrome = nearOffset;
                        _farOffsetDataRecordChrome = farOffset;
                    }
                    else if (record is GroupByRecord)
                    {
                        _nearOffsetGroupByChrome = nearOffset;
                        _farOffsetGroupByChrome = farOffset;
                    }
                    else
                    {
                        _nearOffsetExpandableFieldChrome = nearOffset;
                        _farOffsetExpandableFieldChrome = farOffset;
                    }

                    // bump the indentoffsetversion since this value has 
                    // changed and could effect other records
                    this._fieldLayout.BumpRecordIndentVersion();
                }
            }

            return near ? nearOffset : farOffset;
        }

        #endregion //GetNestedContentChrome

		// AS 6/18/09 NA 2009.2 Field Sizing
		#region GetPlaceholder
		internal CellPlaceholder GetPlaceHolder(Field f, bool label, bool requireMatchingField)
		{
			if (null == this._lastCellArea)
				this.InitializeCachedCellArea();

			CellPlaceholder placeholder = null;

			// if its a label then get into the label header panel first
			if (label && null != _lastLabelPanel)
				placeholder = GetPlacedholder(_lastLabelPanel, f);

			if (null == placeholder && f.TemplateCellIndex >= 0)
				placeholder = _cellPlaceHolders[f.TemplateCellIndex];

			//Debug.Assert(null != placeholder && null != placeholder.ChildCellElement);

			if (null != placeholder && null != placeholder.ChildCellElement)
			{
				if (requireMatchingField && f != GridUtilities.GetFieldFromControl(placeholder.ChildCellElement))
					placeholder.EnsureHasOwnControl();
			}

			return placeholder;
		}

		private static CellPlaceholder GetPlacedholder(Panel parent, Field field)
		{
			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(parent); i < count; i++)
			{
				CellPlaceholder placeholder = VisualTreeHelper.GetChild(parent, i) as CellPlaceholder;

				if (null != placeholder && placeholder.Field == field)
					return placeholder;
			}

			return null;
		}
		#endregion //GetPlaceholder

		// AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
		#region GetVirtPanelOffset
		internal double GetVirtPanelOffset(bool near, bool far)
        {
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

            double offset = 0;

            if (near)
                offset += _nearOffsetVirtualizingPanel;

            if (far)
                offset += _farOffsetVirtualizingPanel;

            return offset;
        } 
        #endregion //GetVirtPanelOffset

        // JJD 1/19/09 - NA 2009 vol 1 
        #region GetRecordOffset

        internal double GetRecordOffset(Record record, bool near)
        {
            Debug.Assert(record != null);

            if (record == null)
                return 0d;

			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

            double dataRecordOffset;
            if (near)
                dataRecordOffset = this._nearOffsetDataRecord;
            else
                dataRecordOffset = this._farOffsetDataRecord;

            // for Datarecords and summary record we use the offset that was 
            // cacluated when we created the template record in InitializeCachedCellArea
            if (record is DataRecord ||
                record is SummaryRecord)
                return dataRecordOffset;

            if (record is GroupByRecord)
            {
                // see if we have already cached a valid value
                if (!double.IsNaN(this._nearOffsetGroupByRecord))
                {
                    if (near)
                        return this._nearOffsetGroupByRecord;
                    else
                        return this._farOffsetGroupByRecord;
                }
            }
            else if (record is ExpandableFieldRecord)
            {
                // see if we have already cached a valid value
                if (!double.IsNaN(this._nearOffsetExpandableFieldRecord))
                {
                    if (near)
                        return this._nearOffsetExpandableFieldRecord;
                    else
                        return this._farOffsetExpandableFieldRecord;
                }
            }
            else
            {
                Debug.Fail("Unknown record type: " + record.GetType().ToString());
                return dataRecordOffset;
            }

            // get the recrod's associated record presenter
            RecordPresenter rp = record.AssociatedRecordPresenter;

            // If the rp hasn't been arranged yet then return the default since
            // we can't do our point translations accurateyl until it is 
            if (rp == null ||
                 !rp.IsInitialized ||
                !rp.IsArrangeValid)
                return dataRecordOffset;

            // AS 3/25/09 TFS15801
            // If we didn't have a content site or if we did but we calculated 
            // no near or far offset, we were still returning the 
            // _(near|far)OffsetDataRecord stored above. We should return 0 to 
            // indicate there is no indent. This would happen for a record that 
            // didn't occupy a scroll position.
            //
            dataRecordOffset = 0;

			
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)
































































#endregion // Infragistics Source Cleanup (Region)


			double nearOffset;
			double farOffset;

			if (GetRecordOffset(rp, out nearOffset, out farOffset))
			{
				if (nearOffset != 0 ||
					 farOffset != 0)
				{
					if (record is GroupByRecord)
					{
						this._nearOffsetGroupByRecord = nearOffset;
						this._farOffsetGroupByRecord = farOffset;
					}
					else
					{
						this._nearOffsetExpandableFieldRecord = nearOffset;
						this._farOffsetExpandableFieldRecord = farOffset;
					}

					// bump the indentoffsetversion since this value has 
					// changed and could effect other records
					this._fieldLayout.BumpRecordIndentVersion();

					// AS 3/25/09 TFS15801
					// Now that we cached the value the next time this method was 
					// called we would have returned the correct value but this time 
					// we would have returned the value calculated above - i.e. the 
					// _(near|far)OffsetDataRecord instead of the calculated near/far 
					// values.
					//
					dataRecordOffset += near ? nearOffset : farOffset;
				}
			}

            return dataRecordOffset;
        }

		// AS 8/25/09 TFS17560
		// Moved the implementation from the other GetRecordOffset overload into this method.
		//
		private static bool GetRecordOffset(RecordPresenter rp, out double nearOffset, out double farOffset)
		{
			nearOffset = double.NaN;
			farOffset = double.NaN;

			// get the content site
			FrameworkElement recordContentSite = rp == null ? null : rp.GetRecordContentSite();

			if (recordContentSite == null)
				return false;

			FieldLayout fl = rp.FieldLayout;

			if (null == fl)
				return false;

			Size sz = recordContentSite.DesiredSize;

			Size szNestedContent;

			FrameworkElement feNestedContent = rp.GetNestedContentSite();

			if (feNestedContent != null)
				szNestedContent = feNestedContent.DesiredSize;
			else
				szNestedContent = Size.Empty;

			// JJD 05/07/12 - TFS107490
			// For groupby records calculate the offset by translating points between the record presenter
			// and the nest content instead of the record content so that when we calculate the header margins
			// they will line up with the data record cells
			//Point ptTopLeft = recordContentSite.TranslatePoint(new Point(), rp);
			//Point ptBottomRight = recordContentSite.TranslatePoint(new Point(Math.Max(sz.Width, szNestedContent.Width), Math.Max(sz.Height, szNestedContent.Height)), rp);
			Point ptTopLeft;
			Point ptBottomRight;

			if (feNestedContent != null && rp is GroupByRecordPresenter)
			{
				ptTopLeft = feNestedContent.TranslatePoint(new Point(), rp);
				ptBottomRight = feNestedContent.TranslatePoint(new Point(Math.Max(sz.Width, szNestedContent.Width), Math.Max(sz.Height, szNestedContent.Height)), rp);
			}
			else
			{
				ptTopLeft = recordContentSite.TranslatePoint(new Point(), rp);
				ptBottomRight = recordContentSite.TranslatePoint(new Point(Math.Max(sz.Width, szNestedContent.Width), Math.Max(sz.Height, szNestedContent.Height)), rp);
			}

			
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


			// In horizonatl mode we need to take the full height of the
			// groupbyrecord and expandableField record since they are always rendered
			// horizonatlly
			if (fl.IsHorizontal)
			{
				nearOffset = ptTopLeft.Y + sz.Height;
				farOffset = rp.DesiredSize.Height - ptBottomRight.Y;

				// AS 3/25/09 TFS15801
				// The subtraction could result in rounding errors which cause 
				// us to think the value isn't 0 when it really is.
				//
				if (GridUtilities.AreClose(farOffset, 0))
					farOffset = 0;

				if (nearOffset != 0 || farOffset != 0)
				{
					Thickness margin = rp.Margin;
					nearOffset += margin.Top + margin.Bottom;
				}
			}
			else
			{
				nearOffset = ptTopLeft.X;
				farOffset = rp.DesiredSize.Width - ptBottomRight.X;

				// AS 3/25/09 TFS15801
				// The subtraction could result in rounding errors which cause 
				// us to think the value isn't 0 when it really is.
				//
				if (GridUtilities.AreClose(farOffset, 0))
					farOffset = 0;
			}

			return true;
		} 
		#endregion //GetRecordOffset
    
        // JJD 9/18/08 -- added suppoert for printing
        #region GetReportLayoutInfoForRecord


        internal ReportLayoutInfo GetReportLayoutInfoForRecord(Record record, double? recordOffset)
        {
            DataPresenterReportControl dprc = this._fieldLayout.DataPresenter as DataPresenterReportControl;
            ReportViewBase reportView = null;
            ReportSection section = null;

            if (dprc != null)
            {
                reportView = dprc.CurrentViewInternal as ReportViewBase;
                section = dprc.Section;
            }

            if (dprc == null ||
                reportView == null ||
                section == null ||
                dprc.CurrentPanel == null ||
				// AS 6/22/09 NA 2009.2 Field Sizing
                //dprc.AutoFitResolved ||
				_fieldLayout.IsAutoFit ||
                section.Report.ReportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale ||
                reportView.CellPageSpanStrategy == CellPageSpanStrategy.Continue)
            {
                this._reportLayoutInfos = null;
                this._reportLayoutNestingDepthOffsetCache = null;

                return null;
            }

            double panelExtent;

            CellPageSpanStrategy cellPageSpanStrategy = reportView.CellPageSpanStrategy;

            bool isHorizontal = this._fieldLayout.IsHorizontal;

            if (isHorizontal)
                panelExtent = dprc.CurrentPanel.ActualHeight;
            else
                panelExtent = dprc.CurrentPanel.ActualWidth;

            if (panelExtent != this._lastPanelExtent ||
                isHorizontal != this._lastIsHorizontal ||
                cellPageSpanStrategy != this._lastCellPageSpanStrategy)
            {
                this._lastPanelExtent                       = panelExtent;
                this._lastIsHorizontal                      = isHorizontal;
                this._lastCellPageSpanStrategy              = cellPageSpanStrategy;
                this._reportLayoutInfos                     = null;
                this._reportLayoutNestingDepthOffsetCache   = null;
            }

            double offset;
            int nestingDepth = record != null ? record.NestingDepth : 0;

            if (recordOffset.HasValue)
            {
                offset = recordOffset.Value;

                if (nestingDepth == 0)
                    s_lastZeroIndentOffset = offset;
            }
            else
            {
                if (nestingDepth > 0 &&
                    this._reportLayoutNestingDepthOffsetCache != null &&
                    this._reportLayoutNestingDepthOffsetCache.ContainsKey(nestingDepth))
                    offset = this._reportLayoutNestingDepthOffsetCache[nestingDepth];
                else
                {
                    double levelIndentation;

                    if (reportView is TabularReportView)
                        levelIndentation = ((TabularReportView)reportView).LevelIndentation;
                    else
                        levelIndentation = TabularReportView.DEFAULT_LEVELINDENTATION;

                    // calculate a best guess based on indentation level
                    offset = (nestingDepth * levelIndentation) + s_lastZeroIndentOffset;
                }
            }

            ReportLayoutInfo pli = null;

            if (this._reportLayoutInfos == null)
                this._reportLayoutInfos = new Dictionary<double, ReportLayoutInfo>();
            else
                this._reportLayoutInfos.TryGetValue(offset, out pli);

            if (pli == null)
            {
                pli = new ReportLayoutInfo(offset, panelExtent, this._fieldLayout);
                this._reportLayoutInfos[offset] = pli;
            }

            if (this._reportLayoutNestingDepthOffsetCache == null)
                this._reportLayoutNestingDepthOffsetCache = new Dictionary<int, double>();

            this._reportLayoutNestingDepthOffsetCache[nestingDepth] = offset;

            return pli;

        }

        #endregion //GetReportLayoutInfoForRecord

        #region InvalidateDefinitionStarInfo
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		#endregion //InvalidateDefinitionStarInfo

        // AS 3/6/09 TFS15025
        #region IsTemplateRecordPresenter
		internal bool IsTemplateRecordPresenter(DataRecordPresenter rp)
        {
			return null != rp && rp == _lastRecordPresenter;
        }
        #endregion //IsTemplateRecordPresenter

		#region OnFieldLayoutRemoved
		// JJD 9/22/09 - TFS18162 - added
		internal void OnFieldLayoutRemoved()
		{
			DataPresenterBase presenter = this._fieldLayout.DataPresenter;

			if (presenter != null)
			{
				// since the FieldLayout has been removed from the collection take
				// the record prenter out of the DP's logical tree
				if (null != this._lastRecordPresenter)
					presenter.InternalRemoveLogicalChild(this._lastRecordPresenter);
				if (null != this._lastGroupByPresenter)
					presenter.InternalRemoveLogicalChild(this._lastGroupByPresenter);
			}
		} 
		#endregion //OnFieldLayoutRemoved

		// AS 10/9/09 TFS22990
		#region OnTemplateRecordTemplateChanged
		internal void OnTemplateRecordTemplateChanged(FrameworkElement fe)
		{
			if (this.IsInitializingCache)
				return;

			bool releaseCache = false;

			if (fe == _lastGroupByPresenter)
			{
				releaseCache = true;
			}

			if (releaseCache)
			{
				this.ReleaseCache();
				_fieldLayout.BumpLayoutItemVersion();
			}
		}
		#endregion //OnTemplateRecordTemplateChanged

        #region ReleaseCache
        internal void ReleaseCache()
		{
			if (null != this._lastRecordPresenter)
			{
                Debug.Assert(_isInitializingCache == false, "We shouldn't be releasing the cache while initializing it");

                // AS 2/10/09
                _cacheVersion++;

				DataPresenterBase presenter = this._fieldLayout.DataPresenter;

				Debug.Assert(null != presenter, "Unable to access datapresenter to properly clean up !");

				if (null != presenter)
				{
					presenter.InternalRemoveLogicalChild(this._lastRecordPresenter);

					// AS 8/25/09 TFS17560
					presenter.InternalRemoveLogicalChild(this._lastGroupByPresenter);
				}

				// JJD 2/16/12 - TFS101387
				// Cache the old record presenters in stack variables so we can clear them
 				// at the very end of this logic.
				var oldRp	= _lastRecordPresenter;
				var oldGBRP = _lastGroupByPresenter;

				this._lastRecordPresenter = null;
				this._lastCellArea = null;
				this._cellPlaceHolders = null;

				// AS 8/25/09 TFS17560
				this._lastGroupByPresenter = null;

				// JJD 5/1/07
				// Clear the cached cell elements
				this._cellElements = null;

                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                this._labelElements = null;

				// AS 5/4/07 Optimization
				this._virtualizedCellFields = null;
				this._unVirtualizedCellFields = null;

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                this._virtualizedFilterCellFields = null;
                this._unVirtualizedFilterCellFields = null;
                this._filterCellFieldMap = null;

                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                this._rowLayoutItems = null;
                this._colLayoutItems = null;
                this._gridDefinitionSizes = null;
                this._templateGridSize = NaNSize;

				// JJD 5/4/07 - Optimization
				// Added label virtualization
				this._virtualizedLabelFields = null;
				this._unVirtualizedLabelFields = null;
                // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
				//this._virtualizedLabelFieldsToMeasure = null;

                // AS 1/17/09 NA 2009 Vol 1 - Fixed Fields
                this._allFields = null;

                // AS 2/26/09 CellPresenter Chrome
                this._cellPresenterMargins = null;

                // AS 3/6/09 TFS15025
                // This isn't specific to the issue but we should clear the 
                // reference to the cell panel as well.
                //
                this._lastCellPanel = null;

				// AS 7/7/09 TFS19145
				this._lastLabelPanel = null;

				// AS 3/25/09 TFS15801
                // Reset/clear the cached values so we calculated them again.
                // In addition to the new fields, we need to do this for a 
                // couple of previously added fields as well.
                //
                _nearOffsetGroupByRecord = double.NaN;
                _nearOffsetExpandableFieldRecord = double.NaN;
                _farOffsetGroupByRecord = double.NaN;
                _farOffsetExpandableFieldRecord = double.NaN;
                _nearOffsetGroupByChrome = double.NaN;
                _nearOffsetExpandableFieldChrome = double.NaN;
                _nearOffsetDataRecordChrome = double.NaN;
                _farOffsetDataRecordChrome = double.NaN;
                _farOffsetGroupByChrome = double.NaN;
                _farOffsetExpandableFieldChrome = double.NaN;

				// AS 5/1/07 Performance
				//this._fieldIndexes = null;
				FieldCollection fields = this._fieldLayout.Fields;
				for (int i = 0, count = fields.Count; i < count; i++)
					fields[i].TemplateCellIndex = -1;

				// JJD 2/16/12 - TFS101387
				// Clear the DataContext on the old cached record presenters so we can 
				// use the lack of a DataContext to indicate that they are no longer active
				// This fixes a memory leak by preventing old presenters from being rooted
				// in certain cases.
				oldRp.ClearValue(FrameworkElement.DataContextProperty);
				oldGBRP.ClearValue(FrameworkElement.DataContextProperty);
			}
		}
		#endregion //ReleaseCache

		// JJD 10/21/11 - TFS86028 - Optimization
		#region SetCachedAutoFitHeight

		// JJD 10/21/11 - TFS86028 - Optimization
		// Maintain a cache of auto-fit Heights for cell area keyed by indent level 
		// so we can eliminate unnecessary multiple layout passes
		internal void SetCachedAutoFitHeight(Record rcd, double height)
		{
			if (_autoFitCellAreaHeightCache == null)
				_autoFitCellAreaHeightCache = new Dictionary<int, double>();

			_autoFitCellAreaHeightCache[rcd.NestingDepth] = height;
		}

		#endregion //SetCachedAutoFitHeight	

		// JJD 10/21/11 - TFS86028 - Optimization
		#region SetCachedAutoFitWidth

		// JJD 10/21/11 - TFS86028 - Optimization
		// Maintain a cache of auto-fit Widths for cell area keyed by indent level 
		// so we can eliminate unnecessary multiple layout passes
		internal void SetCachedAutoFitWidth(Record rcd, double width)
		{
			if (_autoFitCellAreaWidthCache == null)
				_autoFitCellAreaWidthCache = new Dictionary<int, double>();

			_autoFitCellAreaWidthCache[rcd.NestingDepth] = width;
		}

		#endregion //SetCachedAutoFitWidth	

		#region Verify
		internal void Verify()
		{
			// AS 2/19/10 TFS28036
			// We should verify the entire cache which means fixing up 
			// the cell panels too if they are out of sync.
			//
			//if (null == this._lastCellPanel)
			//    this.InitializeCachedCellArea();
			this.VerifyCache();
		} 
		#endregion //Verify

		#endregion //Internal

		#region Private

		// AS 7/7/09 TFS19145
		#region AddPlaceholder
		private CellPlaceholder AddPlaceholder(Panel placeholderPanel, Field field, bool isLabel)
		{
			// AS 10/9/09 NA 2010.1 - CardView
			// When hosting cards we want the label to be as wide as the 
			// caption which means needing 
			//CellPlaceholder placeholder = new CellPlaceholder();
			bool alwaysCreate = _fieldLayout.CellPresentation == CellPresentation.CardView && _fieldLayout.UseCellPresenters;
			CellPlaceholder placeholder = new CellPlaceholder(alwaysCreate);

			placeholder.BeginInit();
			placeholder.SetValue(CellPlaceholder.IsLabelProperty, isLabel);
			placeholder.SetValue(CellPlaceholder.FieldProperty, field);
			placeholderPanel.Children.Add(placeholder);
			placeholder.EndInit();

			return placeholder;
		}
		#endregion //AddPlaceholder

		#region GetCellPanel
		internal Panel GetCellPanel()
        {
			// AS 7/7/09 TFS19145
			//if (null == this._lastCellArea)
			//	this.InitializeCachedCellArea();
			this.VerifyCache();

            return this._lastCellPanel;
        }
        #endregion //GetCellPanel

        #region InitializeCachedCellArea
        private void InitializeCachedCellArea()
		{
			if (this._lastCellArea == null)
			{
				// AS 7/7/09 TFS19145
				_verifiedCacheVersion = _cacheVersion;

				Debug.Assert(this._fieldLayout.StyleGenerator == null || this._fieldLayout.StyleGenerator.IsGeneratingTemplates == false);

                // AS 2/26/09 CellPresenter Chrome
                Debug.Assert(_isInitializingCache == false, "Should we get into InitializeCachedCellArea recursively? If we do then we'll end up creating a different RP while we are in the process of creating/initializing from a different one.");

				bool wasInitializing = this._isInitializingCache;
				try
				{
					this._isInitializingCache = true;
					DataPresenterBase presenter = this._fieldLayout.DataPresenter;

					if (null != presenter)
					{
						FieldLayoutTemplateGenerator generator = this._fieldLayout.StyleGenerator;

						if (null != generator)
						{
							// create a record since the data record cell area has things that 
							// expect it to be in the parent chain
							DataRecordPresenter rp = new DataRecordPresenter();
							this._lastRecordPresenter = rp;

                            // JJD 6/4/09 
                            // Add a temporary handler for the CanExecute command to
                            // prevent it bubbling up to the DataPresenter whose CanExecute logic
                            // can cause a re-entrancy problem 
                            CanExecuteRoutedEventHandler handler = delegate(object sender, CanExecuteRoutedEventArgs e)
                            {
                                e.CanExecute = true;
                                e.Handled = true;
                            };
                            CommandManager.AddCanExecuteHandler(rp, handler);

                            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                            // We need to be able to uniquely identify the rp used by the 
                            // cache since we want its header template to be a special panel.
                            //
                            rp.Tag = RecordPresenter.TemplateRecordPresenterId;

							// SSP 9/15/09 - Enhanged grid-view
							// Moved this here freom below. We should set the DataContext before adding the
							// rp to the logical child otherwise we get binding warnings if there's a binding
							// in the rp template that expects DataContext to be a Record. If we don't set
							// the DataContext before adding the rp as the logical child of the data presenter,
							// it may inherit the data context of the data presenter.
							// 
							DataRecord record = this._fieldLayout.TemplateDataRecord;
							rp.DataContext = record;

							// JJD 08/15/12 - TFS119037
							// If the FieldLayout is no longer in the FieldLayouts collection then log a warning
							if (_fieldLayout.WasRemovedFromCollection)
								GridUtilities.LogDebuggerWarning("Should not be initializing a TemplateDataRecordCache for a FieldLayout that was removed from the FieldLayouts collection.");

							// initialize the record after adding it to the logical tree
							presenter.InternalAddLogicalChild(rp);

							rp.PrepareContainerForItem(record);

                            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                            // We want to be able to get the labelpresenter sizes from 
                            // the template record.
                            //
                            rp.InitializeHeaderContent(true);

							// SSP 9/15/09 - Enhanged grid-view
							// Moved this above, before we add the rp to the data presenter.
							// 
							//rp.DataContext = record;

                            //presenter.TemplateContainer.Children.Add(this._lastRecordPresenter);

							// force the templates to be applied
							VirtualizingDataRecordCellPanel.ApplyTemplateRecursively(rp);
                            
                            // JJD 1/15/09 - NA 2009 vol 1 - record filtering
                            // Make sure the expansionindicator is visible and cache
                            // its size on the fieldlayout
							// AS 8/25/09 TFS17560
							// Moved into a helper method.
							//
                            //ExpansionIndicator expInd = Utilities.GetTemplateChild<ExpansionIndicator>(this._lastRecordPresenter);
							//
                            //if (expInd != null && expInd.TemplatedParent == this._lastRecordPresenter)
							ExpansionIndicator expInd = GridUtilities.GetExpansionIndicator(_lastRecordPresenter);

							if (null != expInd)
                            {
								// AS 9/3/09 TFS21581
								// Reverting to the original behavior. We conditionally use the ExpansionIndicatorSize
								// and since GroupByRecord's need this even when DataRecord's wouldn't because of the 
								// ExpansionIndicatorDisplayModeResolved, we need store the ExpansionIndicatorSize always.
								//
								//// AS 1/22/09
								//// We don't want an expansion indicator size to be stored if the 
								//// field layout cannot show one.
								////
								////expInd.Visibility = Visibility.Visible;
								//expInd.Visibility = _fieldLayout.ExpansionIndicatorDisplayModeResolved == ExpansionIndicatorDisplayMode.Never ? Visibility.Collapsed : Visibility.Visible;
								expInd.Visibility = Visibility.Visible;
                                expInd.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                expInd.Arrange(new Rect(expInd.DesiredSize));
                                Size sz = expInd.DesiredSize;

                                if (!sz.IsEmpty)
                                    this._fieldLayout.ExpansionIndicatorSize = sz;
                            }

                            // JJD 1/19/09 - NA 2009 vol 1 
                            // Measure and arrange the entire record
							// AS 7/7/09 TFS19145
							// Moved this down after we have initialized the cell panels.
							//
							//this._lastRecordPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            //this._lastRecordPresenter.Arrange(new Rect(rp.DesiredSize));

                            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                            HeaderLabelArea labelArea = Utilities.GetDescendantFromType<HeaderLabelArea>(rp, true, null);

							// AS 7/7/09 TFS19145
							if (null != labelArea)
							{
								this._lastLabelPanel = Utilities.GetDescendantFromName(labelArea, FieldLayoutTemplateGenerator.HeaderAreaItemGridName) as Panel;

								// AS 7/7/09 TFS19145
								this.InitializeCellPanel(_lastLabelPanel, true);
							}

							// then store the record cell area within it
							this._lastCellArea = Utilities.GetDescendantFromType<DataRecordCellArea>(rp, true, null);

                            // JJD 3/31/08 - check for null _lastCellArea
                            if (this._lastCellArea != null)
                            {
                                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                                // Changed from _lastCellGrid to _lastCellPanel since we're not using
                                // a Grid element anymore.
                                //
                                //this._lastCellGrid = Utilities.GetDescendantFromName(this._lastCellArea, FieldLayoutTemplateGenerator.CellAreaItemGridName) as Grid;
                                this._lastCellPanel = Utilities.GetDescendantFromName(this._lastCellArea, FieldLayoutTemplateGenerator.CellAreaItemGridName) as Panel;

								// AS 7/7/09 TFS19145
								this.InitializeCellPanel(_lastCellPanel, false);

								// AS 7/7/09 TFS19145
								// Moved this outside the if block so we can measure/arrange the rp first.
								// Previously the RP was measured above but now that we need to wait until the 
								// placeholders are created, I had to move the call to InitializeCellInfo down 
								// as well.
								//
								//// force the grid to be measured and arranged so we can access field cell heights
								//// if necessary
								//// JJD 3/31/08 - check for null _lastCellGrid
								//if (this._lastCellPanel != null)
								//{
								//    // JJD 1/19/09 - NA 2009 vol 1 
								//    // Not needed since the recordpresenter was measured and arranged above
								//    //this._lastCellPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
								//    //this._lastCellPanel.Arrange(new Rect(this._lastCellPanel.DesiredSize));

								//    this.InitializeCellInfo();
								//}
                            }

							// AS 7/7/09 TFS19145
							// Moved this from above. We need to initialize the cell/label panel with its 
							// CellPlaceholder instances before we measure so all the elements are measured together.
							// Otherwise I found that the _farOffsetVirtualizingPanel was getting an incorrect negative value.
							//
							this._lastRecordPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
							this._lastRecordPresenter.Arrange(new Rect(rp.DesiredSize));

							// AS 7/7/09 TFS19145
							// Moved this down.
							//
							// force the grid to be measured and arranged so we can access field cell heights
							// if necessary
							// JJD 3/31/08 - check for null _lastCellGrid
							if (this._lastCellPanel != null)
							{
								// JJD 1/19/09 - NA 2009 vol 1 
								// Not needed since the recordpresenter was measured and arranged above
								//this._lastCellPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
								//this._lastCellPanel.Arrange(new Rect(this._lastCellPanel.DesiredSize));

								this.InitializeCellInfo();
							}

                            FrameworkElement recordContentSite = this._lastRecordPresenter.GetRecordContentSite();

                            Debug.Assert(recordContentSite != null);

                            // JJD 1/19/09 - NA 2009 vol 1 
                            // Calculate and cache the near and far offsets
                            if (recordContentSite != null)
                            {
                                bool isHorizontal = _fieldLayout.IsHorizontal;
                                Size sz = recordContentSite.RenderSize;
                                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                                // Actually we want to use the label area and not the panel within it
                                // since there may be chrome around the panel and we don't want to include
                                // that since we're only using the labelPanel in case its wider than the 
                                // recordcontentsite and therefore we want the chrome between it and the 
                                // recordpresenter.
                                //
                                //Size szLabels = this._lastLabelPanel.RenderSize;
                                Size szLabels = null != labelArea ? labelArea.RenderSize : new Size();
                                Point ptTopLeft = recordContentSite.TranslatePoint(new Point(), rp);
                                Point ptBottomRight = recordContentSite.TranslatePoint(new Point(Math.Max(sz.Width, szLabels.Width), Math.Max(sz.Height, szLabels.Height)), rp);

                                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                                // we need to calculate the amount of chrome on the near and far side 
                                // of the virtualizing cell panel within the record cell area.
                                //
                                FrameworkElement cellPanel = Utilities.GetDescendantFromName(recordContentSite, FieldLayoutTemplateGenerator.CellAreaItemGridName) as FrameworkElement;

                                if (null != cellPanel)
                                {
                                    Size szPanel = cellPanel.RenderSize;
                                    Size szRecordContentSize = recordContentSite.RenderSize;
                                    Point ptPanelTopLeft = cellPanel.TranslatePoint(new Point(), recordContentSite);
                                    Point ptPanelBottomRight = cellPanel.TranslatePoint(new Point(szPanel.Width, szPanel.Height), recordContentSite);

                                    _nearOffsetVirtualizingPanel = isHorizontal ? ptPanelTopLeft.Y : ptPanelTopLeft.X;

                                    if (isHorizontal)
                                    {
                                        _nearOffsetVirtualizingPanel = ptPanelTopLeft.Y;
                                        _farOffsetVirtualizingPanel = szRecordContentSize.Height - ptPanelBottomRight.Y;
                                    }
                                    else
                                    {
                                        _nearOffsetVirtualizingPanel = ptPanelTopLeft.X;
                                        _farOffsetVirtualizingPanel = szRecordContentSize.Width - ptPanelBottomRight.X;
                                    }
                                }

                                if (isHorizontal)
                                {
                                    this._nearOffsetDataRecord  = ptTopLeft.Y;
                                    this._farOffsetDataRecord   = this._lastRecordPresenter.RenderSize.Height - ptBottomRight.Y;
                                }
                                else
                                {
                                    this._nearOffsetDataRecord  = ptTopLeft.X;
                                    this._farOffsetDataRecord   = this._lastRecordPresenter.RenderSize.Width - ptBottomRight.X;
                                }
                            }

							// AS 8/25/09 TFS17560
							this.InitializeGroupByRecordCache();

                            //Debug.Assert(this._lastCellArea != null);
							//Debug.Assert(this._lastCellGrid	!= null);

                            // JJD 6/4/09 
                            // Remove the temporary handler for the CanExecute command added above
                            CommandManager.RemoveCanExecuteHandler(rp, handler);

						}
					}
				}
				finally
				{
					this._isInitializingCache = wasInitializing;

                    // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                    // Any preferred sizes that may have been gathered while initializing
                    // could be wrong because we wouldn't have been able to get to info
                    // while initializing - e.g. cell elements to get sizes.
                    //
                    this._fieldLayout.BumpLayoutItemVersion();
				}
			}
		}

		#endregion //InitializeCachedCellArea

		#region InitializeDefinitionStarInfo
        
#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

		#endregion //InitializeDefinitionStarInfo

		#region InitializeCellInfo
		private void InitializeCellInfo()
		{
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//Grid itemGrid = this.GetCellGrid();
            Panel itemGrid = this.GetCellPanel();
			List<CellPlaceholder> cells = new List<CellPlaceholder>();
			// AS 5/1/07 Performance
			//Dictionary<Field, int> fieldIndexes = new Dictionary<Field, int>();

			// AS 5/4/07 Optimization
			this._virtualizedCellFields = new List<Field>();
			this._unVirtualizedCellFields = new List<Field>();

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            this._virtualizedFilterCellFields = new List<Field>();
            this._unVirtualizedFilterCellFields = new List<Field>();
            this._filterCellFieldMap = new Dictionary<FilterCellElementKey, Field>();

			// JJD 5/4/07 - Optimization
			// Added label virtualization
			this._virtualizedLabelFields = new List<Field>();
			this._unVirtualizedLabelFields = new List<Field>();
            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
			//this._virtualizedLabelFieldsToMeasure = new List<Field>();

            // AS 1/17/09 NA 2009 Vol 1 - Fixed Fields
            this._allFields = new List<Field>();

            // AS 2/26/09 CellPresenter Chrome
            this._cellPresenterMargins = new Dictionary<CellElementKey, Thickness>();

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            // When using preload all fields will be considered non-virtualized.
            //
            bool preLoadCells = this._fieldLayout.DataPresenter != null
                ? this._fieldLayout.DataPresenter.CellContainerGenerationMode == CellContainerGenerationMode.PreLoad
                : false;
            bool useCellPresenters = this._fieldLayout.UseCellPresenters;

            // AS 12/11/08 TFS11518
            // This isn't needed anymore since we need to know the field of the 
            // position we accounted for so we may as well use the _virtualizedLabelFieldsToMeasure
            // and just get its GridPosition when comparing.
            //
            //List<Field.FieldGridPosition> labelPositionsAccountedFor = new List<Field.FieldGridPosition>();

			for (int i = 0, count = itemGrid.Children.Count; i < count; i++)
			{
				CellPlaceholder child = itemGrid.Children[i] as CellPlaceholder;

				// JJD 5/03/07 - Optimization
				// Changed logic to use cell place holders
				#region Obsolete code

				//if (child is CellValuePresenter || child is CellPresenter)
				//{
				//    cells.Add(child);
				//    // AS 5/1/07 Performance
				//    //fieldIndexes.Add((Field)child.GetValue(CellValuePresenter.FieldProperty), cells.Count - 1);
				//    ((Field)child.GetValue(CellValuePresenter.FieldProperty)).TemplateCellIndex = cells.Count - 1;
				//}

				#endregion //Obsolete code	
    
				if (child != null)
				{
					cells.Add(child);
                }
            }

            // AS 3/9/09 Optimization
            // I had to modify my optimization on 3/3/09. The way I set that up I sorted the 
            // resulting field lists so that we didn't consider the lists to be different when 
            // the VDRCP compared the lists but the order of the cell elements may have been 
            // different which could cause a problem when we reused the elements. Instead we 
            // will now sort the CellPlaceholders which will essentially sort the resulting 
            // lists as well.
            //
            Comparison<CellPlaceholder> placeholderComparison = delegate(CellPlaceholder c1, CellPlaceholder c2)
            {
                Field f1 = c1.Field;
                Field f2 = c2.Field;

                Debug.Assert(null != f1 && null != f2);

                int idx1 = f1 != null ? f1.Index : -1;
                int idx2 = f2 != null ? f2.Index : -1;

                return idx1.CompareTo(idx2);
            };
            cells.Sort(Utilities.CreateComparer(placeholderComparison));
            int nextCellIndex = 0;

            for (int i = 0, count = cells.Count; i < count; i++)
            {
                CellPlaceholder child = cells[i];

                if (null != child)
                {
					Field field = child.Field;

					Debug.Assert(field.IsInLayout);

                    // AS 3/9/09 Optimization
                    //field.TemplateCellIndex = cells.Count - 1;
                    field.TemplateCellIndex = nextCellIndex;
                    nextCellIndex++;

					// AS 5/4/07 Optimization
                    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
					//if (field.AllowCellVirtualizationResolved)
                    bool virtualizeCell = false == preLoadCells &&
                        field.AllowCellVirtualizationResolved &&
                        // AS 3/3/09 Optimization
                        // The fixed field doesn't have to be non-virt. Making it so will
                        // make the VDRCP have to remove the cell elements because the field 
                        // list will change.
                        //
                        //// AS 2/19/09 TFS14038
                        //field.FixedLocation == FixedFieldLocation.Scrollable &&
                        // AS 1/26/09
                        // When using cell presenters we should not virtualize a cell when 
                        // the label presenter cannot be virtualized or else we could end up
                        // measuring the label using a cellpresenter with different label 
                        // settings.
                        //
                        (!useCellPresenters || field.AllowLabelVirtualizationResolved);
                    field.IsCellVirtualized = virtualizeCell;

                    if (virtualizeCell)
                        this._virtualizedCellFields.Add(field);
                    else
                    {
                        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                        // If the regular cell is not to be virtualized then we won't
                        // virtualize the filter cell either. In theory we may be able to 
                        // but we won't have the actual filter cell editor.
                        //
                        this._unVirtualizedFilterCellFields.Add(field);
                        field.IsFilterCellVirtualized = false;

                        this._unVirtualizedCellFields.Add(field);
                    }

					// JJD 5/4/07 - Optimization
					// Added label virtualization
					//if (field.AllowLabelVirtualizationResolved)
                    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                    bool virtualizeLabel = useCellPresenters
                        ? virtualizeCell
                        : false == preLoadCells && field.AllowLabelVirtualizationResolved
                            // AS 3/3/09 Optimization
                            // The fixed field doesn't have to be non-virt. Making it so will
                            // make the VDRCP have to remove the cell elements because the field 
                            // list will change.
                            //
                            //// AS 2/19/09 TFS14038
                            //&& field.FixedLocation == FixedFieldLocation.Scrollable;
                            ;
                    field.IsLabelVirtualized = virtualizeLabel;

                    if (virtualizeLabel)
					{
						this._virtualizedLabelFields.Add(field);

                        #region Commented out
                        
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

                        #endregion //Commented out
					}
					else
						this._unVirtualizedLabelFields.Add(field);

					#region CellPresenter Margins
					
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

					#endregion //CellPresenter Margins
                }
			}

            bool isHorizontal = _fieldLayout.IsHorizontal;

			// AS 5/4/07 Optimization
			// keep the unvirtualized fields sorted by their grid position
            // AS 3/3/09 Optimization
            // Sort by index so it doesn't look like the field list has changed when compared in the VDRCP.
            //
            //if (this._unVirtualizedCellFields.Count > 1)
            //    this._unVirtualizedCellFields.Sort(new FieldLayoutTemplateGenerator.GridSlotComparer(isHorizontal));

			// JJD 5/4/07 - Optimization
			// Added label virtualization
			// keep the unvirtualized fields sorted by their grid position
            // AS 3/3/09 Optimization
            // Sort by index so it doesn't look like the field list has changed when compared in the VDRCP.
            //
            //if (this._unVirtualizedLabelFields.Count > 1)
            //    this._unVirtualizedLabelFields.Sort(new FieldLayoutTemplateGenerator.GridSlotComparer(isHorizontal));

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            foreach (Field field in _virtualizedCellFields)
            {
                // AS 2/10/09
                // We decided that an operand type of None would mean that the cell
                // isn't to be included in the filter row. To simplify things we'll 
                // consider the cell to be non-virtualized. Note, I'm not adding it
                // to the _unVirtualizedFilterCellFields because we're going to remove
                // the None operand filter cells below anyway.
                //
                if (field.FilterOperandUITypeResolved == FilterOperandUIType.None)
                {
                    field.IsFilterCellVirtualized = false;
                    continue;
                }

                Field measureField;
                _filterCellFieldMap.TryGetValue(field.FilterCellElementKey, out measureField);

                if (null == measureField)
                {
                    _filterCellFieldMap[field.FilterCellElementKey] = field;
                    field.IsFilterCellVirtualized = false;
                    _unVirtualizedFilterCellFields.Add(field);
                }
                else
                {
                    field.IsFilterCellVirtualized = true;
                    _virtualizedFilterCellFields.Add(field);
                }
            }

            // AS 2/10/09
            // We decided that an operand type of None would mean that the cell
            // isn't to be included in the filter row so remove any nonvirt filter 
            // cell fields with a value of none.
            //
            for (int i = _unVirtualizedFilterCellFields.Count - 1; i >= 0; i--)
            {
                if (_unVirtualizedFilterCellFields[i].FilterOperandUITypeResolved == FilterOperandUIType.None)
                    _unVirtualizedFilterCellFields.RemoveAt(i);
            }

            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            // AS 1/17/09 NA 2009 Vol 1 - Fixed Fields
            _allFields.AddRange(_unVirtualizedCellFields);
            _allFields.AddRange(_virtualizedCellFields);
            // AS 3/3/09 Optimization
            // Sort by index so it doesn't look like the field list has changed when compared in the VDRCP.
            //
            //_allFields.Sort(new FieldLayoutTemplateGenerator.GridSlotComparer(isHorizontal));

            // AS 3/3/09 Optimization
            // Previously we sorted some of these field collections based on their Row/Column position.
            // However the VDRCP caches the field lists and if they change it will remove the old cell 
            // elements and recreate new ones. This causes a performance hit when dragging fields or  
            // changing the fixed location. Since sorting by the row/column really doesn't help we can 
            // just sort by the Field's Index so the order is the same regardless of where the field 
            // is moved to and therefore the comparison of the new<=>old field lists will come back as 
            // equal.
            //
            Comparison<Field> indexComparison = delegate(Field f1, Field f2)
            {
                return f1.Index.CompareTo(f2.Index);
            };
            IComparer<Field> indexComparer = Utilities.CreateComparer(indexComparison);
            _allFields.Sort(indexComparer);
            _virtualizedCellFields.Sort(indexComparer);
            _virtualizedFilterCellFields.Sort(indexComparer);
            _virtualizedLabelFields.Sort(indexComparer);
            _unVirtualizedCellFields.Sort(indexComparer);
            _unVirtualizedFilterCellFields.Sort(indexComparer);
            _unVirtualizedLabelFields.Sort(indexComparer);

			this._cellPlaceHolders = cells;
			// AS 5/1/07 Performance
			//this._fieldIndexes = fieldIndexes;

			// AS 2/25/10 TFS28494
			// See comments in the loop above.
			//
			for (int i = 0, count = cells.Count; i < count; i++)
			{
				CellPlaceholder child = cells[i];

				if (null != child)
				{
					Field field = child.Field;

                    // AS 2/26/09 CellPresenter Chrome
                    // The CellPresenter could be styled such that there is chrome around the 
                    // CellPresenterLayoutElement. Since the layout uses a layout item for the 
                    // CVP & Label, we need to have those layout items include the necessary 
                    // margins so that the element is large enough to display its specified 
                    // size and the chrome.
                    //
                    if (useCellPresenters)
                    {
                        if (false == _cellPresenterMargins.ContainsKey(field.CellElementKey) &&
                            VisualTreeHelper.GetChildrenCount(child) > 0)
                        {
                            CellPresenter cp = VisualTreeHelper.GetChild(child, 0) as CellPresenter;

                            if (null != cp)
                            {
                                cp.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                cp.Arrange(new Rect(cp.DesiredSize));

                                CellPresenterLayoutElementBase layoutElement = Utilities.GetDescendantFromType(cp, typeof(CellPresenterLayoutElementBase), true) as CellPresenterLayoutElementBase;

                                if (null != layoutElement)
                                {
                                    // we need to find out where the layout element is within the 
                                    // cell presenter
                                    Size leSize = layoutElement.RenderSize;
                                    Point ptLETopLeft = layoutElement.TranslatePoint(new Point(), cp);
                                    Point ptLEBottomRight = layoutElement.TranslatePoint(new Point(leSize.Width, leSize.Height), cp);

                                    Thickness t = new Thickness(ptLETopLeft.X, ptLETopLeft.Y, cp.ActualWidth - ptLEBottomRight.X, cp.ActualHeight - ptLEBottomRight.Y);
                                    _cellPresenterMargins[field.CellElementKey] = t;

                                    // since the CellPresenterLayoutElementBase will not be measuring
                                    // its children while initializing cache is true, we want to invalidate
                                    // the measure of the element so it will measure/arrange the cvp/label
                                    // later
                                    cp.InvalidateLayoutElement();
                                    cp.InvalidateMeasure();
                                }
                            }
                        }
                    }
				}
			}
		}
		#endregion //InitializeCellInfo

		// AS 7/7/09 TFS19145
		#region InitializeCellPanel
		private void InitializeCellPanel(Panel placeholderPanel, bool isLabelPanel)
		{
			if (null == placeholderPanel)
				return;

			foreach (Field field in _fieldLayout.Fields)
			{
				if (!field.IsInLayout)
					continue;

				AddPlaceholder(placeholderPanel, field, isLabelPanel);
			}
		}
		#endregion //InitializeCellPanel

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region InitializeGridDefinitionItems
        private void InitializeGridDefinitionItems()
        {
            this._colLayoutItems = new List<GridDefinitionLayoutItem>();
            this._rowLayoutItems = new List<GridDefinitionLayoutItem>();
            this._gridDefinitionSizes = new Dictionary<Field, Size>();

            FieldLayoutTemplateGenerator styleGenerator = this._fieldLayout.StyleGenerator;

            if (null == styleGenerator)
                return;

            // make sure the templates are created...
            if (styleGenerator.IsGeneratingTemplates == false)
                styleGenerator.GenerateTemplates();

            ColumnDefinition defaultColDef = styleGenerator.DefaultColumnDefinition;
            RowDefinition defaultRowDef = styleGenerator.DefaultRowDefinition;
            Grid templateGrid = styleGenerator.TemplateGrid;
            bool usingCellPresenters = this._fieldLayout.UseCellPresenters;

            if (null != templateGrid)
                this._templateGridSize = new Size(templateGrid.Width, templateGrid.Height);

            #region Build List of Row/Column Definitions
            List<ColumnDefinition> columns = new List<ColumnDefinition>();
            List<RowDefinition> rows = new List<RowDefinition>();

            for (int i = 0, count = this._fieldLayout.TotalColumnsGenerated; i < count; i++)
            {
                ColumnDefinition col = templateGrid == null || i >= templateGrid.ColumnDefinitions.Count
                    ? defaultColDef
                    : templateGrid.ColumnDefinitions[i];

                columns.Add(col);
            }

            for (int i = 0, count = this._fieldLayout.TotalRowsGenerated; i < count; i++)
            {
                RowDefinition row = templateGrid == null || i >= templateGrid.RowDefinitions.Count
                    ? defaultRowDef
                    : templateGrid.RowDefinitions[i];

                rows.Add(row);
            } 
            #endregion //Build List of Row/Column Definitions

            #region See If All Are AutoFit

            DataPresenterBase dp = this._fieldLayout.DataPresenter;

            // if we're autofitting and all the column/row definitions are 
            // the same default width then don't bother creating the additional
            // layout items since the grid bag will automatically distribute 
            // all the columns
			// AS 6/22/09 NA 2009.2 Field Sizing
			//bool isAutoFitWidth = dp != null && dp.IsAutoFitWidth;
			//bool isAutoFitHeight = dp != null && dp.IsAutoFitHeight;
            bool isAutoFitWidth = _fieldLayout.IsAutoFitWidth;
            bool isAutoFitHeight = _fieldLayout.IsAutoFitHeight;
            bool areColumnsNeeded = false;
            bool areRowsNeeded = false;

            if (isAutoFitWidth)
            {
                for (int i = 0, count = columns.Count; i < count; i++)
                {
                    if (columns[i].Width.IsStar == false ||
                        !GridUtilities.AreClose(1, columns[i].Width.Value))
                    {
                        areColumnsNeeded = true;
                        break;
                    }
                }
            }
            else
                areColumnsNeeded = true;

            if (isAutoFitHeight)
            {
                for (int i = 0, count = rows.Count; i < count; i++)
                {
                    if (rows[i].Height.IsStar == false ||
                        !GridUtilities.AreClose(1, rows[i].Height.Value))
                    {
                        areRowsNeeded = true;
                        break;
                    }
                }
            }
            else
                areRowsNeeded = true;
            #endregion //See If All Are AutoFit

            #region Add If Needed
            if (areColumnsNeeded)
            {
                for (int i = 0, count = columns.Count; i < count; i++)
                {
                    ColumnDefinition col = columns[i];

                    if (GridDefinitionLayoutItem.IsItemNeeded(col))
                        this._colLayoutItems.Add(new GridDefinitionLayoutItem(col, i, usingCellPresenters));
                }
            }

            if (areRowsNeeded)
            {
                for (int i = 0, count = rows.Count; i < count; i++)
                {
                    RowDefinition row = rows[i];

                    if (GridDefinitionLayoutItem.IsItemNeeded(row))
                        this._rowLayoutItems.Add(new GridDefinitionLayoutItem(row, i, usingCellPresenters));
                }
            } 
            #endregion //Add If Needed

            #region Store Default Width/Height from DefinitionBase
            // for fields that occupy a single logical column/row, store the 
            // width/height of the definitionbase should it be an absolute size
            for (int i = 0, count = this._fieldLayout.Fields.Count; i < count; i++)
            {
                Field field = this._fieldLayout.Fields[i];

                if (field.IsInLayout == false)
                    continue;

                Field.FieldGridPosition gridPos = field.GridPosition;
                Size gridCellSize = new Size(double.NaN, double.NaN);

                if (gridPos.ColumnSpan == 1)
                {
                    Debug.Assert(gridPos.Column >= 0 && gridPos.Column < columns.Count);

                    ColumnDefinition col = columns[gridPos.Column];

                    if (col.Width.IsAbsolute)
                        gridCellSize.Width = col.Width.Value;
                }

                if (gridPos.RowSpan == 1)
                {
                    Debug.Assert(gridPos.Row >= 0 && gridPos.Row < rows.Count);

                    RowDefinition row = rows[gridPos.Row];

                    if (row.Height.IsAbsolute)
                        gridCellSize.Height = row.Height.Value;
                }

                if (!double.IsNaN(gridCellSize.Width) || !double.IsNaN(gridCellSize.Height))
                    _gridDefinitionSizes.Add(field, gridCellSize);
            } 
            #endregion //Store Default Width/Height from DefinitionBase
        }
        #endregion //InitializeGridDefinitionItems

		// AS 8/25/09 TFS17560
		#region InitializeGroupByRecordCache
		private void InitializeGroupByRecordCache()
		{
			GroupByRecordPresenter rp = new GroupByRecordPresenter();
			_lastGroupByPresenter = rp;

			// make sure we know this is a template record to avoid certain actions (e.g. binding
			// to fixed field info, etc.)
			rp.Tag = RecordPresenter.TemplateRecordPresenterId;

			_fieldLayout.DataPresenter.InternalAddLogicalChild(rp);

			// initialize the record after adding it to the logical tree
			rp.PrepareContainerForItem(_fieldLayout.TemplateGroupByRecord);
			rp.DataContext = _fieldLayout.TemplateGroupByRecord;

			rp.InitializeGroupByRecordContentVisibility(true);

			// just use a dummy element as the nested content
			FrameworkElement nestedContent = new FrameworkElement();
			rp.InitializeNestedContent(nestedContent);

			// force the templates to be applied
			VirtualizingDataRecordCellPanel.ApplyTemplateRecursively(rp);

			ExpansionIndicator expInd = GridUtilities.GetExpansionIndicator(rp);

			// force the expansion indicator visibility based on whether we can show one
			if (expInd != null)
			{
				// AS 9/3/09 TFS21581
				//expInd.Visibility = _fieldLayout.ExpansionIndicatorDisplayModeResolved == ExpansionIndicatorDisplayMode.Never ? Visibility.Collapsed : Visibility.Visible;
				expInd.Visibility = Visibility.Visible;
			}

			rp.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			rp.Arrange(new Rect(_lastGroupByPresenter.DesiredSize));

			bool isHorizontal = _fieldLayout.IsHorizontal;

			FrameworkElement ncs = rp.GetNestedContentSite();

			if (null != ncs)
			{
				Size sz = nestedContent.DesiredSize;
				Size feNestedContentSize = ncs.DesiredSize;

				Point ptTopLeft = nestedContent.TranslatePoint(new Point(), ncs);
				Point ptBottomRight = nestedContent.TranslatePoint(new Point(sz.Width, sz.Height), ncs);

				if (isHorizontal)
				{
					_nearOffsetGroupByChrome = ptTopLeft.Y;
					_farOffsetGroupByChrome = feNestedContentSize.Height - ptBottomRight.Y;
				}
				else
				{
					_nearOffsetGroupByChrome = ptTopLeft.X;
					_farOffsetGroupByChrome = feNestedContentSize.Width - ptBottomRight.X;
				}
			}

			// AS 8/26/09 TFS21427
			//GetRecordOffset(_lastGroupByPresenter, out _nearOffsetGroupByRecord, out _farOffsetGroupByChrome);
			GetRecordOffset(_lastGroupByPresenter, out _nearOffsetGroupByRecord, out _farOffsetGroupByRecord);
		} 
		#endregion //InitializeGroupByRecordCache

		// AS 7/7/09 TFS19145
		#region ReinitializeCellInfo
		private void ReinitializeCellInfo()
		{
			// AS 2/25/10 TFS28494
			// We should considering ourselves initializing the cache 
			// even if we're just fixing up the panels.
			//
			bool wasInitializing = this._isInitializingCache;
			try
			{
				this._isInitializingCache = true;

				this.VerifyCellPanel(_lastCellPanel, false);
				this.VerifyCellPanel(_lastLabelPanel, true);

				this.InitializeCellInfo();
			}
			finally
			{
				// AS 2/25/10 TFS28494
				// Just as we need to bump the layout version when the full 
				// cache is being initialed we need to do that here too. We
				// should also ensure we know we're initializing.
				//

				_isInitializingCache = wasInitializing;

				// AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
				// Any preferred sizes that may have been gathered while initializing
				// could be wrong because we wouldn't have been able to get to info
				// while initializing - e.g. cell elements to get sizes.
				//
				if (!_isInitializingCache)
					_fieldLayout.BumpLayoutItemVersion();
			}
		}

		#endregion //ReinitializeCellInfo

		// AS 7/7/09 TFS19145
		#region VerifyCache
		private void VerifyCache()
		{
			// moved this first if block from the various internal members to here
			if (null == _lastCellArea)
			{
				this.InitializeCachedCellArea();
			}
			else if (_cacheVersion != _verifiedCacheVersion)
			{
				// AS 2/19/10 TFS28036
				Debug.Assert(CellPlaceholder.IsInMeasure == false);

				// we cannot manipulate the cell panels while the placeholders are being measured 
				// since the containing stackpanel could be enumerating the collection.
				if (CellPlaceholder.IsInMeasure)
					return;

				_verifiedCacheVersion = _cacheVersion;

				_fieldLayout.VerifyStyleGeneratorTemplates();

				// AS 2/25/10 TFS28494
				// Now that I set the _isInitializingCache flag I found that 
				// the above call to verify the templates could have released 
				// the cache. If that happens then we should verify the full 
				// cached area instead of calling into the ReinitializeCellInfo.
				//
				//this.ReinitializeCellInfo();
				if (null == _lastCellArea)
					this.InitializeCachedCellArea();
				else
					this.ReinitializeCellInfo();
			}
		}

		#endregion //VerifyCache

		#region VerifyCellPanel
		private void VerifyCellPanel(Panel cellPanel, bool isLabel)
		{
			if (cellPanel == null)
				return;

			// AS 9/3/09 TFS21432
			// We cache the actual label/cell element from the cellplaceholder so we need to 
			// fix that up as well.
			//
			// AS 9/9/09 TFS21965
			// The cache may not be allocated.
			//
			//if (isLabel)
			//    _labelElements.Clear();
			//else
			//    _cellElements.Clear();
			IDictionary cache = isLabel ? (IDictionary)_labelElements : _cellElements;

			if (null != cache)
				cache.Clear();
			

			Dictionary<Field, CellPlaceholder> placeholders = new Dictionary<Field, CellPlaceholder>();

			// AS 9/3/09 TFS21432
			//foreach (UIElement element in cellPanel.Children)
			for (int i = cellPanel.Children.Count - 1; i >= 0; i--)
			{
				// AS 9/3/09 TFS21432
				//CellPlaceholder placeholder = element as CellPlaceholder;
				CellPlaceholder placeholder = cellPanel.Children[i] as CellPlaceholder;
				Debug.Assert(null != placeholder);

				if (null != placeholder)
				{
					// AS 9/3/09 TFS21432
					// There are two scenarios to deal with here. First, if a field is removed then 
					// we wouldn't have removed the placeholder in the next block. Also and more likely 
					// is that a placeholder could be holding a reference to a cell element for a field 
					// that is no longer in the layout in which case we want to release that placeholder.
					//
					//placeholders[placeholder.Field] = placeholder;

					Field placeholderField = placeholder.Field;

					if (placeholder.ChildCellElement != null)
					{
						Field elementField = GridUtilities.GetFieldFromControl(placeholder.ChildCellElement);

						if (elementField == null || elementField.Index < 0 || elementField.IsInLayout == false)
						{
							cellPanel.Children.RemoveAt(i);
							continue;
						}

						// if we're keeping the child element and it belongs to this placeholder's field 
						// then register it as it would have when it first created it
						if (elementField == placeholderField)
							CacheCellElement(placeholderField, placeholder.ChildCellElement, isLabel);
					}

					placeholders[placeholderField] = placeholder;
				}
			}

			foreach (Field field in _fieldLayout.Fields)
			{
				CellPlaceholder placeholder;

				// get the placeholder we have currently for the field
				if (!placeholders.TryGetValue(field, out placeholder))
				{
					// if we didn't have a placeholder and its still not in the layout do nothing
					if (!field.IsInLayout)
					{
						field.TemplateCellIndex = -1;
						continue;
					}

					// otherwise we need to create a placeholder
					AddPlaceholder(cellPanel, field, isLabel);
				}
				else
				{
					// we had a placeholder and the field is still in the layout do nothing
					if (field.IsInLayout)
						continue;

					// clear the template cell index
					field.TemplateCellIndex = -1;

					// we need to remove the placeholder
					cellPanel.Children.Remove(placeholder);
				}
			}

			// AS 10/1/09 TFS22650
			cellPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		} 
		#endregion //VerifyCellPanel

		// AS 10/9/09 TFS22990
		#region VerifyTemplateRecordPanel
		internal void VerifyTemplateRecordPanel(Panel panel, RecordPresenter rp)
		{
			if (rp == _lastRecordPresenter)
			{
				if (this.IsInitializingCache)
					return;

				if (panel == _lastCellPanel ||
					panel == _lastLabelPanel)
					return;

				// if the panel created is not one of the panels we have stored 
				// then an ancestor template must have changed and caused a new 
				// record content area panel to be generated so we need to release
				// the current cache
				this.ReleaseCache();
				_fieldLayout.BumpLayoutItemVersion();
			}
		}
		#endregion //VerifyTemplateRecordPanel

		#endregion //Private

		#endregion //Methods

		// JJD 9/12/08 - added support for printing
        #region ReportElementComparer private class

        // sorts by layout position
        private class ReportElementComparer : IComparer
        {
            private bool _isVertical;


            internal ReportElementComparer(bool isVertical)
            {
                this._isVertical = isVertical;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                Control xControl = x as Control;
                Control yControl = y as Control;

                if (xControl == yControl)
                    return 0;

                if (yControl == null)
                    return 1;

                if (xControl == null)
                    return -1;

                Field xFld = GridUtilities.GetFieldFromControl(xControl);
                Field yFld = GridUtilities.GetFieldFromControl(yControl);

                if (xFld == yFld)
                    return 0;

                if (yFld == null)
                    return 1;

                if (xFld == null)
                    return -1;

                Field.FieldGridPosition xpos = xFld.GridPosition;
                Field.FieldGridPosition ypos = yFld.GridPosition;

                int xBeginSlot;
                int xSpan;
                int yBeginSlot;
                int ySpan;
 
                if (this._isVertical)
                {
                    xBeginSlot  = xpos.Column;
                    xSpan       = xpos.ColumnSpan;
                    yBeginSlot  = ypos.Column;
                    ySpan       = ypos.ColumnSpan;
                }
                else
                {
                    xBeginSlot  = xpos.Row;
                    xSpan       = xpos.RowSpan;
                    yBeginSlot  = ypos.Row;
                    ySpan       = ypos.RowSpan;
                }

                int xEndSlot = xBeginSlot + xSpan;
                int yEndSlot = yBeginSlot + ySpan;

                if (xBeginSlot < yBeginSlot)
                    return -1;

                if (xBeginSlot > yBeginSlot)
                    return 1;

                if (xEndSlot < yEndSlot)
                    return -1;

                if (xEndSlot > yEndSlot)
                    return 1;

                return 0;
            }

            #endregion
        }

        #endregion //ReportElementComparer private class
	} 
	#endregion //TemplateDataRecordCache

    #region ReportLayoutInfo internal class


    internal class ReportLayoutInfo
    {
    #region Private Members

        private List<PageInfo> _pages;
        private Dictionary<Field, PageFieldInfo> _fieldsTooLargeToFit;
        private Dictionary<Field, PageFieldInfo> _allFields;
        private double _overallExtent;
        private double _offsetWithinPanel;

        #endregion //Private Members

    #region Constructor

        internal ReportLayoutInfo(double offsetWithinPanel, double panelExtent, FieldLayout fieldLayout)
        {
            this._offsetWithinPanel = offsetWithinPanel;
            this._allFields = new Dictionary<Field, PageFieldInfo>();

            TemplateDataRecordCache templateCache = fieldLayout.TemplateDataRecordCache;

            DataPresenterReportControl dp = fieldLayout.DataPresenter as DataPresenterReportControl; ;

            Debug.Assert(dp != null, "ReportLayoutInfo requires a DataPresenterReportControl");

            if (dp == null)
                return;

            ReportViewBase reportView = dp.CurrentViewInternal as ReportViewBase;

            Debug.Assert(reportView != null, "ReportLayoutInfo requires a ReportViewBase");

            if (reportView == null)
                return;

            this._fieldsTooLargeToFit = new Dictionary<Field, PageFieldInfo>();

            CellPageSpanStrategy pageSpanStrategy = reportView.CellPageSpanStrategy;

            Debug.Assert(pageSpanStrategy != CellPageSpanStrategy.Continue, "we should'ne e in here with CellPageSpanStratgey set to 'Continue'");

            bool isVertical = reportView.LogicalOrientation == Orientation.Vertical;

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //FieldLayout.SizeHolderManager shm = fieldLayout.LogicalColumnSizeManager;
            FieldGridBagLayoutManager lm = fieldLayout.CellLayoutManager;
            lm.VerifyLayout();
            lm.CalculatePreferredSize();

            List<Control> cellElements = templateCache.CellElementsInReportOrder;

            double firstPageCellExtent = Math.Max(panelExtent - offsetWithinPanel, 0);

            List<PageFieldInfo> pageBreakCandidates = new List<PageFieldInfo>();

            this._pages = new List<PageInfo>();
            this._pages.Add(new PageInfo(firstPageCellExtent));

            // First loop over the cells to determine which are candidates to be moved
            // from page to page based on their spans
            foreach (Control cellElement in cellElements)
            {
                Field fld = GridUtilities.GetFieldFromControl(cellElement);

                Debug.Assert(fld != null, "The cell element should be associated with a field");

                if (fld != null)
                {
                    #region Get beginslot, endslot and span

                    Field.FieldGridPosition pos = fld.GridPosition;

                    int beginSlot;
                    int span;

                    if (isVertical)
                    {
                        beginSlot = pos.Column;
                        span = pos.ColumnSpan;
                    }
                    else
                    {
                        beginSlot = pos.Row;
                        span = pos.RowSpan;
                    }

                    int endSlot = beginSlot + span - 1;

                    #endregion //Get beginslot, endslot and span

                    // get the size holder for this slot and span
                    // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                    //SizeHolder shCell = shm.GetSizeHolder(beginSlot, span);
                    //
                    //double cellExtent = shCell.Size;
                    double cellExtent = lm.GetPreferredExtent(beginSlot, span, isVertical);

                    PageFieldInfo fieldInfo = new PageFieldInfo(cellExtent, beginSlot, endSlot, fld);

                    // if the cell span is such that it won't fit on a page then keep track of
                    // it so we can cause it to be continued over multiple _pages
                    if (cellExtent > panelExtent ||
                        (cellExtent > firstPageCellExtent && beginSlot == 0))
                        this._fieldsTooLargeToFit.Add(fld, fieldInfo);
                    // JJD 1/14/09 - TFS10926
                    // Always add the file to pagebreakCandidates.
                    // Note: There is addition logic below that will remove the field from 
                    // either the pageBreakCandidates or _fieldsTooLargeToFit collections
                    // based on whether or not there are other fields that occupy the same begin slot
                    //else
                        pageBreakCandidates.Add(fieldInfo);
                }
            }

            #region Make sure the field is in the proper collection (pageBreakCandidates or fieldsTooLargeToFit)

            // JJD 1017/08 - TFS9231
            // If all of the fields are too large to fit then move them into pageBreakCandidates.
            // Otherwise, we won't have any pages generated below.
            if (this._fieldsTooLargeToFit.Count > 0)
            {
                if (pageBreakCandidates.Count == 0)
                {
                    foreach (PageFieldInfo pfi in this._fieldsTooLargeToFit.Values)
                        pageBreakCandidates.Add(pfi);

                    this._fieldsTooLargeToFit.Clear();
                }
                else
                {
                    // JJD 1/14/09 - TFS10926
                    // copy the large fiels into a temp array
                    List<PageFieldInfo> tempLargeFields = new List<PageFieldInfo>(this._fieldsTooLargeToFit.Values);

                    // JJD 1/14/09 - TFS10926
                    // loop over the temp list of large fields (i.e. too bigg to fit on a page)
                    // and determine if any other fields occupy their begin slot. If so they can
                    // be left in the toolargerfields collection and remvoed from the page break candidate list.
                    // Otherwise they need to be remved from the too large fields list since someone
                    // need to occupy that slot
                    foreach (PageFieldInfo pfi in tempLargeFields)
                    {
                        bool isSlotOccupied = false;

                        for (int i = 0, count = pageBreakCandidates.Count; i < count; i++)
                        {
                            PageFieldInfo candidate = pageBreakCandidates[i];

                            if (candidate == pfi)
                                continue;

                            if (candidate.BeginSlot > pfi.BeginSlot)
                                break;

                            if (candidate.BeginSlot == pfi.BeginSlot)
                            {
                                isSlotOccupied = true;
                                break;
                            }
                        }

                        // JJD 1/14/09 - TFS10926
                        // Since the field can only be in one of the collections we need
                        // to remove it from the other
                        if (isSlotOccupied == false)
                            this._fieldsTooLargeToFit.Remove(pfi.Field);
                        else
                            pageBreakCandidates.Remove(pfi);

                    }
                }
            }

            #endregion //Make sure the field is in the proper collection (pageBreakCandidates or fieldsTooLargeToFit)	
    
            int currentPageIndex = 0;
            int pageBreakIndex = 0;

            for (int i = 0, count = pageBreakCandidates.Count; i < count; i++)
            {
                PageFieldInfo pfi = pageBreakCandidates[i];

                PageInfo currentPage = this._pages[currentPageIndex];

                if (currentPage.FieldCount < 1)
                {
                    currentPage.AddFieldInfo(pfi);
                    pageBreakIndex = i;
                }
                else
                {
                    if (pfi.BeginSlot >= currentPage.FirstSlot &&
                         pfi.EndSlot <= currentPage.LastSlot)
                        currentPage.AddFieldInfo(pfi);
                    else
                    {
                        double offsetFromPage = 0;

                        if (pfi.BeginSlot > currentPage.FirstSlot)
                            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                            //offsetFromPage = shm.GetSizeHolder(currentPage.FirstSlot, pfi.BeginSlot - currentPage.FirstSlot).Size;
                            offsetFromPage = lm.GetPreferredExtent(currentPage.FirstSlot, pfi.BeginSlot - currentPage.FirstSlot, isVertical);

                        if (offsetFromPage + pfi.Extent > currentPage.OverallPanelExent)
                        {
                            int moveFromSlot = pfi.BeginSlot;

                            PageInfo nextPage;

                            Debug.Assert(currentPageIndex < this._pages.Count, "This is a forward only process so we shouldn't have more pages than the current page");

                            if (currentPageIndex < this._pages.Count)
                            {
                                nextPage = new PageInfo(panelExtent);

                                this._pages.Add(nextPage);
                            }
                            else
                            {
                                nextPage = this._pages[currentPageIndex + 1];
                            }

                            bool fieldsWereMoved = false;

                            for (int j = pageBreakIndex; j < i; j++)
                            {
                                PageFieldInfo pfiToMove = pageBreakCandidates[j];

                                if (pfiToMove.BeginSlot >= pfi.BeginSlot)
                                {
                                    currentPage.RemoveFieldInfo(pfiToMove);
                                    nextPage.AddFieldInfo(pfiToMove);
                                    fieldsWereMoved = true;
                                }
                            }

                            nextPage.AddFieldInfo(pfi);

                            pageBreakIndex = i + 1;

                            // since we don't try to maintain the ast and last slot info on each remove
                            // that was done above we need to refresh that info 
                            if ( fieldsWereMoved )
                                currentPage.RefreshFirstAndLastSlots();

                            // bump the current page index since we have moved on to the next page
                            currentPageIndex++;

                        }
                        else
                            currentPage.AddFieldInfo(pfi);
                    }
                }
            }

            this._overallExtent = 0;

            for (int i = 0, count = this._pages.Count; i < count; i++)
            {
                bool isLastPage = i == count - 1;

                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                //this._pages[i].InitializeFieldOffsets(shm, this._fieldsTooLargeToFit.Values, this._allFields, pageSpanStrategy == CellPageSpanStrategy.NextPageFillWithPreviousCell && !isLastPage);
                this._pages[i].InitializeFieldOffsets(lm, isVertical, this._fieldsTooLargeToFit.Values, this._allFields, pageSpanStrategy == CellPageSpanStrategy.NextPageFillWithPreviousCell && !isLastPage);

                if (isLastPage)
                    this._overallExtent += this._pages[i].ExtentUsed;
                else
                    this._overallExtent += this._pages[i].OverallPanelExent;
            }

        }

        #endregion //Constructor

    #region Properties

        internal double OffsetWithinPanel { get { return this._offsetWithinPanel; } }
        internal double OverallExtent { get { return this._overallExtent; } }
        internal int PageCount { get { return this._pages.Count; } }

        #endregion //Properties

    #region Methods

    #region GetExtent

        internal double GetExtent(Field field, int beginSlot, int span)
        {
            int endSlot = beginSlot + span - 1;
            double extent = 0;
            bool wasFieldFound = false;

            for (int i = 0, count = this._pages.Count; i < count; i++)
            {
                PageInfo pi = this._pages[i];

                if (pi.LastSlot < beginSlot)
                    continue;

                bool isFieldOnPage = pi.GetPageFieldInfo(field) != null;

                if (isFieldOnPage == true)
                    wasFieldFound = true;

                if (pi.FirstSlot <= endSlot)
                {
					if (isFieldOnPage)
					{
						// JJD 1/10/12 - TFS22013
						// Special case the situation where there is a single
						// field on a page and it is wider than the panel extent.
						// KIn this case we want to return the smaller panel
						// extent because we don't support having single fields
						// that span to multiple pages
						//extent += pi.GetExtent(beginSlot, endSlot);
						double slotExtent = pi.GetExtent(beginSlot, endSlot);

						if (beginSlot == endSlot &&
							 pi.FirstSlot == pi.LastSlot)
							slotExtent = Math.Min(slotExtent, pi.OverallPanelExent);

						extent += slotExtent;
					}
					else
					{
						if (wasFieldFound)
							extent += pi.OverallPanelExent;
					}
                }
            }

            return extent;
        }

            #endregion //GetExtent	

    #region GetOffset

        internal double GetOffset(Field field, int slot)
        {
            double offset = 0;

            for (int i = 0, count = this._pages.Count; i < count; i++)
            {
                PageInfo pi = this._pages[i];

                bool isFieldOnPage = pi.GetPageFieldInfo(field) != null;

                if (slot > pi.LastSlot || isFieldOnPage == false)
                {
                    offset += pi.OverallPanelExent;
                    continue;
                }

                if (slot < pi.FirstSlot)
                    return offset;

                return offset + pi.GetOffset(slot);
            }

            return 0;
        }

            #endregion //GetOffsetonPage	

        #endregion //Methods

    #region PageInfo internal class

        internal class PageInfo
        {
    #region Private Members

            private Dictionary<Field, PageFieldInfo> _finalFieldInfos;
            private IList<PageFieldInfo> _fieldInfos;
            private double _overallPanelExtent;
            private int _firstSlot;
            private int _lastSlot;
            private double _extentUsed;
            private double[] _slotExtents;

            #endregion //Private Members

    #region Constructors

            internal PageInfo(double overallPanelExtent)
            {
                this._overallPanelExtent = overallPanelExtent;
                this._fieldInfos = new List<PageFieldInfo>();
                this._firstSlot = -1;
                this._lastSlot = -1;
            }

            #endregion //Constructors

    #region Methods

    #region AddFieldInfo

            internal void AddFieldInfo(PageFieldInfo pfi)
            {
                this._fieldInfos.Add(pfi);
                this.UpdateFirstAndLastSlots(pfi);
            }

            #endregion //AddFieldInfo

    #region GetPageFieldInfo

            internal PageFieldInfo GetPageFieldInfo(Field fld)
            {
                PageFieldInfo fpi;

                this._finalFieldInfos.TryGetValue(fld, out fpi);

                return fpi;
            }

            #endregion //GetPageFieldInfo

    #region GetExtent

            internal double GetExtent(int beginSlot, int endSlot)
            {
                int startSlotIndex = Math.Max(0, beginSlot - this._firstSlot);
                int endSlotIndex = Math.Min(this._slotExtents.Length - 1, endSlot - this._firstSlot);

                double extent = 0;

                for (int i = startSlotIndex; i <= endSlotIndex; i++)
                    extent += this._slotExtents[i];

                return extent;
            }

            #endregion //GetExtent	

    #region GetOffset

            internal double GetOffset(int slot)
            {
                int slotIndex = Math.Min(this._slotExtents.Length - 1, slot - this._firstSlot);

                double offset = 0;

                for (int i = 0; i < slotIndex; i++)
                    offset += this._slotExtents[i];

                return offset;
            }

            #endregion //GetExtent	
    
    #region InitializeFieldOffsets

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //internal void InitializeFieldOffsets(FieldLayout.SizeHolderManager shm,
            internal void InitializeFieldOffsets(FieldGridBagLayoutManager lm,
                                                bool isVertical,
                                                IEnumerable<PageFieldInfo> fieldsTooLargeToFit,
                                                Dictionary<Field, PageFieldInfo> allFields,
                                                bool extendEndingFieldsToFill)
            {
                if (this._finalFieldInfos != null)
                    throw new InvalidOperationException("PageInfo is already initialized");

                this._finalFieldInfos = new Dictionary<Field, PageFieldInfo>(this._fieldInfos.Count);

                int lastBeginSlot = -1;
                double offset = 0;
                this._extentUsed = 0;

                foreach (PageFieldInfo pfi in this._fieldInfos)
                {
                    int beginSlot = pfi.BeginSlot;

                    if (beginSlot != lastBeginSlot)
                    {
                        lastBeginSlot = beginSlot;

                        if (beginSlot > this._firstSlot)
                            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                            //offset = shm.GetSizeHolder(this._firstSlot, beginSlot - this._firstSlot).Size;
                            offset = lm.GetPreferredExtent(this._firstSlot, beginSlot - this._firstSlot, isVertical);
                        else
                            offset = 0;

                        if (extendEndingFieldsToFill && pfi.EndSlot == this._lastSlot)
                            pfi.InitializeExtent(this._overallPanelExtent - offset);

                        pfi.InitializeOffset(offset);

                        this._extentUsed = Math.Max(this._extentUsed, offset + pfi.Extent);
                    }

                    // add to this page's dictionary
                    this._finalFieldInfos.Add(pfi.Field, pfi);

                    // add to the 'allFields' dictionary.
                    // Note: we don't add the field's we clone below to this dictionary
                    allFields.Add(pfi.Field, pfi);
                }

                // allocate the slot extent array
                this._slotExtents = new double[this._lastSlot - this._firstSlot + 1];

                double totalCalulatedExtent = 0;

                // initialize the slot extent array
                for (int i = 0, count = this._slotExtents.Length; i < count; i++)
                {
                    // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                    //double extent = shm.GetSizeHolder(this._firstSlot + i, 1).Size;
                    double extent = lm.GetPreferredExtent(this._firstSlot + i, 1, isVertical);
                    this._slotExtents[i] = extent;
                    totalCalulatedExtent += extent;
                }

                if ( extendEndingFieldsToFill )
                    this._slotExtents[this._slotExtents.Length - 1] += Math.Max(this._overallPanelExtent - totalCalulatedExtent, 0);
 
                // loop over the field too large for each page looking for
                // ones that overlap this page
                foreach (PageFieldInfo pfi in fieldsTooLargeToFit)
                {
                    int beginSlot = pfi.BeginSlot;

                    if (beginSlot == this._firstSlot)
                        offset = 0;
                    else
                        if (beginSlot > this._firstSlot)
                        {
                            // if the bengin slot is > than the last slot on this page
                            // then we can continue
                            if (beginSlot > this._lastSlot)
                                continue;

                            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                            //offset = shm.GetSizeHolder(this._firstSlot, beginSlot - this._firstSlot).Size;
                            offset = lm.GetPreferredExtent(this._firstSlot, beginSlot - this._firstSlot, isVertical);
                        }
                        else
                        {
                            // if the end slot is < than the first slot on this page
                            // then we can continue
                            if (pfi.EndSlot < this._firstSlot)
                                continue;

                            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                            //offset = -shm.GetSizeHolder(beginSlot, this._firstSlot - beginSlot).Size;
                            offset = -lm.GetPreferredExtent(beginSlot, this._firstSlot - beginSlot, isVertical);
                        }

                    // clone the PageFieldInfo since we need a unique osffset value for each page
                    PageFieldInfo clone = new PageFieldInfo(pfi.Extent, pfi.BeginSlot, pfi.EndSlot, pfi.Field);

                    clone.InitializeOffset(offset);

                    this._finalFieldInfos.Add(pfi.Field, clone);
                    this._fieldInfos.Add(clone);
                    
                    this._extentUsed = Math.Max(this._extentUsed, offset + clone.Extent);
                }

                // convert the collection to be read-only
                this._fieldInfos = new ReadOnlyCollection<PageFieldInfo>(this._fieldInfos);
            }

            #endregion //InitializeFieldOffsets

    #region RemoveFieldInfo

            internal void RemoveFieldInfo(PageFieldInfo pfi)
            {
                this._fieldInfos.Remove(pfi);
            }

            #endregion //RemoveFieldInfo

    #region RefreshFirstAndLastSlots

            internal void RefreshFirstAndLastSlots()
            {
                this._firstSlot = -1;
                this._lastSlot = -1;

                int count = this._fieldInfos.Count;

                for (int i = 0; i < count; i++)
                    this.UpdateFirstAndLastSlots(this._fieldInfos[i]);
            }

            #endregion //RefreshFirstAndLastSlots

    #region UpdateFirstAndLastSlots

            private void UpdateFirstAndLastSlots(PageFieldInfo pfi)
            {
                if (this._firstSlot < 0 ||
                    this._firstSlot > pfi.BeginSlot)
                    this._firstSlot = pfi.BeginSlot;

                if (pfi.EndSlot > this._lastSlot)
                    this._lastSlot = pfi.EndSlot;
            }

            #endregion //UpdateFirstAndLastSlots

            #endregion //Methods

    #region Properties

            internal double ExtentUsed { get { return this._extentUsed; } } // Note: this property not updated until InitializeFieldOffsets is called

            internal int FieldCount { get { return this._fieldInfos.Count; } }
            internal int FirstSlot { get { return this._firstSlot; } }
            internal int LastSlot { get { return this._lastSlot; } }
            internal double OverallPanelExent { get { return this._overallPanelExtent; } }
            internal IList<PageFieldInfo> FieldInfos { get { return this._fieldInfos; } }

            #endregion //Properties

        }

        #endregion //PageInfo internal class

    #region PageFieldInfo internal class

        internal class PageFieldInfo
        {
    #region Private members

            private double _extent;
            private double _offset = double.NaN;
            private Field _field;
            private int _beginSlot;
            private int _endSlot;

            #endregion //Private members

    #region Constructor

            internal PageFieldInfo(double extent, int beginSlot, int endSlot, Field field)
            {
                this._extent = extent;
                this._beginSlot = beginSlot;
                this._endSlot = endSlot;
                this._field = field;
            }

            #endregion //Constructor

    #region Methods

            internal void InitializeOffset(double offset)
            {
                if (!double.IsNaN(this._offset))
                    throw new InvalidOperationException();

                this._offset = offset;

            }

            internal void InitializeExtent(double extent)
            {
                if (!double.IsNaN(this._offset))
                    throw new InvalidOperationException();

                this._extent = extent;

            }

            #endregion //Methods

    #region Properties

            internal int BeginSlot { get { return this._beginSlot; } }

            internal int EndSlot { get { return this._endSlot; } }

            internal double Extent { get { return this._extent; } }

            internal Field Field { get { return this._field; } }

            internal double Offset { get { return this._offset; } }

            #endregion //Properties

        }

        #endregion //PageFieldInfo internal class
    }

    #endregion //ReportLayoutInfo internal class

    // AS 1/6/09 NA 2009 Vol 1 - Fixed Fields
    #region FixedFieldLayoutInfo
    internal class FixedFieldLayoutInfo
    {
        #region Member Variables

        private FieldLayout _owner;
        private int _verifiedFixedFieldVersion = -1;

        private int _fixableNearFieldCount;
        private int _fixableFarFieldCount;
        private FixedFieldUIType _fixedUIType;
        private List<Field> _nearFixedFields = new List<Field>();
        private List<Field> _farFixedFields = new List<Field>();
        private int _nearFixedOrigin = 0;
        private int _nearFixedSpan = 0;
        private int _farFixedOrigin = 0;
        private int _farFixedSpan = 0;
        private int _rowCount = 0;
        private int _columnCount = 0;
        private int _farUnfixableCount;
        private int _nearUnfixableCount;

        // AS 2/2/09
        // Added the ability to provide a LayoutInfo that would be used to provide the 
        // origin column/row values. This allowed me to share the same code when calculating
        // the resulting layout when dragging a splitter.
        //
        private LayoutInfo _layoutInfo;

        #endregion //Member Variables

        #region Constructor
        internal FixedFieldLayoutInfo(FieldLayout owner) : this(owner, null)
        {
        }

        internal FixedFieldLayoutInfo(FieldLayout owner, LayoutInfo layoutInfo)
        {
            GridUtilities.ValidateNotNull(owner);
            this._owner = owner;
            this._layoutInfo = layoutInfo;
        }
        #endregion //Constructor

        #region Properties

        #region ColumnCount
        internal int ColumnCount
        {
            get
            {
                VerifyFixedInfo();
                return _columnCount;
            }
        }
        #endregion //ColumnCount

        #region FarFixedFields
        internal IList<Field> FarFixedFields
        {
            get
            {
                VerifyFixedInfo();
                return _farFixedFields;
            }
        }
        #endregion //FarFixedFields

        #region FarFixedOrigin
        internal int FarFixedOrigin
        {
            get
            {
                VerifyFixedInfo();
                return _farFixedOrigin;
            }
        } 
        #endregion //FarFixedOrigin

        #region FarFixedSpan
        internal int FarFixedSpan
        {
            get
            {
                VerifyFixedInfo();
                return _farFixedSpan;
            }
        } 
        #endregion //FarFixedSpan

        #region FarSplitterVisibility
        internal Visibility FarSplitterVisibility
        {
            get
            {
                VerifyFixedInfo();
                return (_fixableFarFieldCount > 0 || _farFixedFields.Count > 0) && IncludeSplitter(_fixedUIType)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        } 
        #endregion //FarSplitterVisibility

        #region FieldLayout
        internal FieldLayout FieldLayout
        {
            get { return _owner; }
        } 
        #endregion //FieldLayout

        #region HasFixableFields
        internal bool HasFixableFields
        {
            get
            {
                this.VerifyFixedInfo();
                return _fixableNearFieldCount > 0 || _fixableFarFieldCount > 0;
            }
        }

        #endregion //HasFixableFields

        #region HasFixedFields
        internal bool HasFixedFields
        {
            get
            {
                this.VerifyFixedInfo();
                return _nearFixedFields.Count > 0 || _farFixedFields.Count > 0;
            }
        }

        #endregion //HasFixedFields

        #region IncludeSplitterElements
        internal bool IncludeSplitterElements
        {
            get
            {
                VerifyFixedInfo();
                return IncludeSplitter(_fixedUIType);
            }
        } 
        #endregion //IncludeSplitterElements

        #region IsFarSplitterEnabled
        /// <summary>
        /// Indicates if the splitter should be enabled. This is true if there are fields 
        /// that may be fixed to the far side or there are far fixed fields that may be 
        /// unfixed.
        /// </summary>
        internal bool IsFarSplitterEnabled
        {
            get
            {
                VerifyFixedInfo();
                return _fixableFarFieldCount > 0 ||
                    _farUnfixableCount > 0;
            }
        } 
        #endregion //IsFarSplitterEnabled

        #region IsNearSplitterEnabled
        /// <summary>
        /// Indicates if the splitter should be enabled. This is true if there are fields 
        /// that may be fixed to the near side or there are near fixed fields that may be 
        /// unfixed.
        /// </summary>
        internal bool IsNearSplitterEnabled
        {
            get
            {
                VerifyFixedInfo();
                return _fixableNearFieldCount > 0 ||
                    _nearUnfixableCount > 0;
            }
        }
        #endregion //IsNearSplitterEnabled

        #region NearFixedFields
        internal IList<Field> NearFixedFields
        {
            get
            {
                VerifyFixedInfo();
                return _nearFixedFields;
            }
        } 
        #endregion //NearFixedFields

        #region NearFixedOrigin
        internal int NearFixedOrigin
        {
            get
            {
                VerifyFixedInfo();
                return _nearFixedOrigin;
            }
        }
        #endregion //NearFixedOrigin

        #region NearFixedSpan
        internal int NearFixedSpan
        {
            get
            {
                VerifyFixedInfo();
                return _nearFixedSpan;
            }
        }
        #endregion //NearFixedSpan

        #region NearSplitterVisibility
        internal Visibility NearSplitterVisibility
        {
            get
            {
                VerifyFixedInfo();
                return (_fixableNearFieldCount > 0 || _nearFixedFields.Count > 0) && IncludeSplitter(_fixedUIType)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
        #endregion //NearSplitterVisibility

        #region RowCount
        internal int RowCount
        {
            get
            {
                VerifyFixedInfo();
                return _rowCount;
            }
        }
        #endregion //RowCount

        #endregion //Properties

        #region Methods

        // AS 2/2/09
        // Added a way to override the fixed location & row/column origins based on a given LayoutInfo.
        //
        #region GetFieldInfo
        private void GetFieldInfo(Field field, out int column, out int row, out int columnSpan, out int rowSpan, out FixedFieldLocation fixedLocation)
        {
            if (null != _layoutInfo)
            {
                ItemLayoutInfo layoutItemInfo = _layoutInfo[field];

                Debug.Assert(null != layoutItemInfo);

                if (null != layoutItemInfo)
                {
                    column = layoutItemInfo.Column;
                    row = layoutItemInfo.Row;
                    columnSpan = layoutItemInfo.ColumnSpan;
                    rowSpan = layoutItemInfo.RowSpan;
                    fixedLocation = layoutItemInfo._fixedLocation;
                    return;
                }
            }

            GridBagConstraint gcField = field.LayoutConstraint;

            column = gcField.Column;
            columnSpan = gcField.ColumnSpan;
            row = gcField.Row;
            rowSpan = gcField.RowSpan;
            fixedLocation = field.FixedLocation;
        } 
        #endregion //GetFieldInfo

        #region IncludeSplitter
        internal static bool IncludeSplitter(FixedFieldUIType type)
        {
            return type == FixedFieldUIType.ButtonAndSplitter || type == FixedFieldUIType.Splitter;
        }
        #endregion //IncludeSplitter

        #region UpdateFixedInfo
        private void UpdateFixedInfo()
        {
            _verifiedFixedFieldVersion = _owner.FixedFieldVersion;

            DataPresenterBase dp = _owner.DataPresenter;

            _fixableNearFieldCount = 0;
            _fixableFarFieldCount = 0;
            _nearFixedFields.Clear();
            _farFixedFields.Clear();
            _nearFixedOrigin = _nearFixedSpan = 0;
            _farFixedOrigin = _farFixedSpan = 0;
            _columnCount = 0;
            _rowCount = 0;
            _farUnfixableCount = 0;
            _nearUnfixableCount = 0;

            if (null != dp && dp.IsFixedFieldsSupportedResolved)
            {
                bool isHorizontal = _owner.IsHorizontal;
                int maxFieldEnd = 0;
                int colCount = 0;
                int rowCount = 0;

                // see if we have fixed/fixable fields
                foreach (Field field in _owner.Fields)
                {
                    if (field.IsInLayout == false)
                        continue;

                    AllowFieldFixing allowFixing = field.AllowFixingResolved;

                    switch (allowFixing)
                    {
                        case AllowFieldFixing.Far:
                            _fixableFarFieldCount++;
                            break;
                        case AllowFieldFixing.Near:
                            _fixableNearFieldCount++;
                            break;
                        case AllowFieldFixing.NearOrFar:
                            _fixableNearFieldCount++;
                            _fixableFarFieldCount++;
                            break;
                    }

                    int row, column, rowSpan, columnSpan;
                    FixedFieldLocation fixedLocation;
                    this.GetFieldInfo(field, out column, out row, out columnSpan, out rowSpan, out fixedLocation);

                    int fieldStart = isHorizontal ? row : column;
                    int fieldEnd = fieldStart + (isHorizontal ? rowSpan : columnSpan);

                    maxFieldEnd = Math.Max(maxFieldEnd, fieldEnd);

                    colCount = Math.Max(colCount, column + columnSpan);
                    rowCount = Math.Max(rowCount, row + rowSpan);

                    switch (fixedLocation)
                    {
                        case FixedFieldLocation.FixedToNearEdge:
                            _nearFixedFields.Add(field);

                            // the near fixed area ends after the last near fixed
                            _nearFixedSpan = Math.Max(_nearFixedSpan, fieldEnd);
                            _farFixedOrigin = Math.Max(_farFixedOrigin, fieldEnd);

                            if (allowFixing != AllowFieldFixing.No)
                                _nearUnfixableCount++;
                            break;

                        case FixedFieldLocation.Scrollable:
                            // if there are not far fixed, then the fixed area
                            // starts after the last scrollable
                            _farFixedOrigin = Math.Max(_farFixedOrigin, fieldEnd);
                            break;
                        case FixedFieldLocation.FixedToFarEdge:
                            _farFixedFields.Add(field);

                            // if there are far fixed, then the fixed area starts before the first 
                            _farFixedOrigin = Math.Min(_farFixedOrigin, fieldStart);

                            if (allowFixing != AllowFieldFixing.No)
                                _farUnfixableCount++;
                            break;
                    }
                }

                _farFixedSpan = maxFieldEnd - _farFixedOrigin;
                _columnCount = colCount;
                _rowCount = rowCount;
            }

            // we need to do this last since the default is based on whether
            // we have fixed/fixable fields
            _fixedUIType = this._owner.GetFixedFieldUITypeResolved(this.HasFixedFields, this.HasFixableFields);
        }
        #endregion //UpdateFixedInfo

        #region VerifyFixedInfo
        private void VerifyFixedInfo()
        {
            if (this._verifiedFixedFieldVersion != _owner.FixedFieldVersion)
                this.UpdateFixedInfo();
        }
        #endregion //VerifyFixedInfo

        #endregion //Methods
    } 
    #endregion //FixedFieldLayoutInfo

	// AS 7/31/09 NA 2009.2 Excel Exporting
	#region ExportLayoutInformation
	/// <summary>
	/// For internal use. Provides layout information regarding the fields of a given field layout.
	/// </summary>
	public class ExportLayoutInformation
	{
		#region Member Variables

		private FieldLayout _fieldLayout;
		private Dictionary<Field, FieldPosition> _labelPositions;
		private Dictionary<Field, FieldPosition> _cellPositions;
		private double[] _columnWidths;
		private double[] _rowHeights;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExportLayoutInformation"/>
		/// </summary>
		/// <param name="fieldLayout">The FieldLayout whose information is to be calculated</param>
		/// <param name="header">True to get the layout information for the header area.</param>
		public ExportLayoutInformation(FieldLayout fieldLayout, bool header)
		{
			GridUtilities.ValidateNotNull(fieldLayout, "fieldLayout");

			_fieldLayout = fieldLayout;

			// make sure the cache information is up to date
			fieldLayout.TemplateDataRecordCache.Verify();

			LayoutManagerType type = header ? LayoutManagerType.Header : LayoutManagerType.Record;
			FieldGridBagLayoutManager lm = GridUtilities.CreateGridBagLayoutManager(fieldLayout, type);
			lm.VerifyLayout();

			Size preferredSize = lm.CalculatePreferredSize();
			_columnWidths = lm.ColumnWidths ?? new double[0];
			_rowHeights = lm.RowHeights ?? new double[0];
			_cellPositions = new Dictionary<Field, FieldPosition>();
			_labelPositions = new Dictionary<Field, FieldPosition>();

			int[] colDeltas = GetDeltas(_columnWidths);
			int[] rowDeltas = GetDeltas(_rowHeights);

			Dictionary<FieldLayoutItemBase, FieldPosition> itemPositions = new Dictionary<FieldLayoutItemBase, FieldPosition>();

			// MD 3/28/12 - TFS106849
			// If there are any star sized fields, we need to size the layout manager with the data presenter's size so we don't
			// just use the minimum columns widths.
			if (fieldLayout.HasStarFields(true))
			{
				DataPresenterExportControlBase exportControl = fieldLayout.DataPresenter as DataPresenterExportControlBase;
				if (exportControl != null)
				{
					DataPresenterBase sourceDataPresenter = exportControl.SourceDataPresenter;
					if (sourceDataPresenter != null)
					{
						FieldRectsLayoutContainer lc = new FieldRectsLayoutContainer();

						
						
						
						var layoutDimensions = lm.GetLayoutItemDimensionsCached(lc, new Rect(sourceDataPresenter.RenderSize));
						var columnDims = layoutDimensions.ColumnDims;

						for (int i = 1; i < columnDims.Length; i++)
							_columnWidths[i - 1] = columnDims[i] - columnDims[i - 1];
					}
				}
			}

			foreach (ILayoutItem li in lm.LayoutItems)
			{
				FieldLayoutItemBase fieldItem = li as FieldLayoutItemBase;

				if (null != fieldItem)
				{
					Field field = fieldItem.Field;
					IGridBagConstraint gc = (IGridBagConstraint)lm.LayoutItems.GetConstraint(li);

					if (li.Visibility == Visibility.Collapsed)
						continue;

					FieldPosition? fieldPos = GetPosition(gc, ref _columnWidths, ref _rowHeights, ref colDeltas, ref rowDeltas);

					if (fieldPos == null)
						continue;

					if (fieldItem.IsLabel)
						_labelPositions[field] = fieldPos.Value;
					else
						_cellPositions[field] = fieldPos.Value;
				}
			}

			_columnWidths = RemoveZeroWidths(_columnWidths);
			_rowHeights = RemoveZeroWidths(_rowHeights);
		}
		#endregion //Constructor

		#region Methods

		#region Public

		#region GetColumnWidths
		/// <summary>
		/// Returns an array of the widths for the logical columns that the fields occupy.
		/// </summary>
		/// <returns>An array of doubles represents the widths of the logical columns</returns>
		public double[] GetColumnWidths()
		{
			return _columnWidths;
		}
		#endregion //GetColumnWidths

		#region GetRowHeights
		/// <summary>
		/// Returns an array of the heights for the logical rows that the fields occupy.
		/// </summary>
		/// <returns>An array of doubles represents the heights of the logical rows</returns>
		public double[] GetRowHeights()
		{
			return _rowHeights;
		}
		#endregion //GetRowHeights

		#region GetFieldPosition
		/// <summary>
		/// Returns the resolved position of the specified field.
		/// </summary>
		/// <param name="field">The <see cref="Field"/> whose position information is to be returned</param>
		/// <param name="label">True to return the position information for a label; otherwise false to return the information for a cell.</param>
		/// <returns>A FieldPosition that indicates the position of the specified element type with respect to the other fields in the field layout or null if the item doesn't exist within the associated layout type.</returns>
		public FieldPosition? GetFieldPosition(Field field, bool label)
		{
			FieldPosition pos;

			Dictionary<Field, FieldPosition> table = label ? _labelPositions : _cellPositions;

			if (!table.TryGetValue(field, out pos))
				return null;

			return pos;
		}
		#endregion //GetFieldPosition

		#endregion //Public

		#region Private

		#region AdjustOriginAndSpan
		private static bool AdjustOriginAndSpan(ref int origin, ref int span, ref double[] extents, ref int[] deltas)
		{
			Debug.Assert(origin >= 0);

			if (span <= 0)
				return false;

			// AS 9/18/09
			// Since we could shift over an item we need to calculate the original after we shift.
			//
			//int originDelta = origin >= 0 && origin < deltas.Length ? deltas[origin] : 0;

			// reduce any col/row spans based on occupying a 0 width row/col
			for (int i = origin, end = Math.Min(extents.Length, origin + span); i < end; i++)
			{
				if (IsZero(extents[i]))
				{
					// if the zero length column is at the head shift over. 
					// note the next column could be 0 too
					if (i == origin)
						origin++;

					span--;

					if (span <= 0)
						return false;
				}
			}

			int originDelta = origin >= 0 && origin < deltas.Length ? deltas[origin] : 0; 
			origin += originDelta;

			Debug.Assert(origin >= 0);

			return true;
		}
		#endregion //AdjustOriginAndSpan

		#region GetDeltas
		private static int[] GetDeltas(double[] extents)
		{
			int currentDelta = 0;
			int[] deltas = new int[extents.Length];

			for (int i = 0; i < extents.Length; i++)
			{
				if (IsZero(extents[i]))
					currentDelta--;

				deltas[i] = currentDelta;
			}

			return deltas;
		} 
		#endregion //GetDeltas

		#region GetPosition
		private static FieldPosition? GetPosition(IGridBagConstraint gc, ref double[] columns, ref double[] rows, ref int[] colDeltas, ref int[] rowDeltas)
		{
			int col = gc.Column;
			int row = gc.Row;
			int colSpan = gc.ColumnSpan;
			int rowSpan = gc.RowSpan;

			if (!AdjustOriginAndSpan(ref col, ref colSpan, ref columns, ref colDeltas))
				return null;

			if (!AdjustOriginAndSpan(ref row, ref rowSpan, ref rows, ref rowDeltas))
				return null;

			return new FieldPosition(col, row, colSpan, rowSpan);
		}
		#endregion //GetPosition

		#region IsZero
		private static bool IsZero(double dbl)
		{
			return GridUtilities.AreClose(0, dbl);
		}
		#endregion //IsZero

		#region RemoveZeroWidths
		private double[] RemoveZeroWidths(double[] extents)
		{
			List<double> newExtents = new List<double>();

			foreach (double dbl in extents)
			{
				if (!IsZero(dbl))
					newExtents.Add(dbl);
			}

			return newExtents.ToArray();
		}
		#endregion //RemoveZeroWidths

		#endregion //Private

		#endregion //Methods
	} 
	#endregion //ExportLayoutInformation
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