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
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Resizing;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Editors;
using System.Xml;
using Infragistics.Collections;
using Infragistics.AutomationPeers;
using System.Collections.Specialized;

namespace Infragistics.Windows.DataPresenter
{
    #region Record

    /// <summary>
    /// Abstract base class used by a XamDataGrid, XamDataCarousel or XamDataPresenter to represent a record 
    /// </summary>
	/// <remarks>
	/// <para class="note"><b>Note: </b>It is represented in the UI via a corresponding <see cref="DataRecordPresenter"/>, <see cref="GroupByRecordPresenter"/> or <see cref="ExpandableFieldRecordPresenter"/> element.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
    /// <seealso cref="DataRecord"/>
    /// <seealso cref="GroupByRecord"/>
    /// <seealso cref="ExpandableFieldRecord"/>
    /// <seealso cref="RecordManager"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Records"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataPresenter"/>
	public abstract class Record : PropertyChangeNotifier, ISparseArrayMultiItem, ISelectableItem
	{
		#region Constants

		
		
		
		
		internal const FilterState FILTERED_IN_OUT_FLAGS = FilterState.FilteredIn | FilterState.FilteredOut;
		internal const FilterState FILTER_ACTION_FLAGS = FilterState.Disable | FilterState.NoAction | FilterState.Hide;
		internal const FilterState FILTER_IN_OUT_AND_ACTION_FLAGS = FILTERED_IN_OUT_FLAGS | FILTER_ACTION_FLAGS;

		#endregion // Constants

		#region Private Members

		private FieldLayout _fieldLayout;
        private RecordCollectionBase _parentCollection;
        private string _description;
        private bool _isExpanded;

        
        // No longer needed
        

        private bool _isFixedLastReturned;
		private bool _enabled = true;
        private bool _selected;
		private WeakReference _associatedRecordPresenter;
		private Visibility _expansionIndicatorVisibilityLastReturned = Visibility.Collapsed;
		private Visibility _visibility = Visibility.Visible;
		private Nullable<Visibility> _explicitExpansionIndicatorVisibility;

        internal object _sparseArrayOwnerData_Sorted = null;
        internal object _sparseArrayOwnerData_Unsorted = null;
        internal object _sparseArrayOwnerData_Grouped = null;

		private int _index = -1;	// JM 6-28-10 TFS33366. Initialize to -1;

        // JJD 2/20/08
        // Added Tag property
        private object _tag;

		private WeakReference _automationPeer;

		
		
		private FixedRecordLocationInternal _fixedRecordLocationOverride;
		private RecordSeparatorVisibility _separatorVisibility;

        // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
		// JJD 1/12/12 - TFS23607 - made internal
		//private FixedRecordLocation _fixedLocation;
        internal FixedRecordLocation _fixedLocation;

        
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		[Flags]
		internal enum FilterState : byte
		{
			NeedsToRefilter	= 1,
			FilteredOut		= 2,
			FilteredIn		= 4,
			NeverFilter		= 8,

			// SSP 4/10/09 TFS16485 TFS16490
			// We need to raise property changed for VisibilityResolved and/or IsEnabled whenever
			// FitlerAction is changed on the field layout settings.
			// 
			Hide			= 0x10,
			Disable			= 0x20,
			NoAction		= 0x40,

			// AS 8/20/09 TFS20920
			// We need a separate flag to know that filtering is suspended so 
			// we don't set the NeedsToRefilter flag until filtering is 
			// resumed for the record.
			//
			FilterSuspended = 0x80
		}

		internal FilterState _cachedFilterState = FilterState.NeedsToRefilter;

        // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
        private FieldGridBagLayoutManager _layoutManager;

		// JM 10-20-09 NA 10.1 CardView
		private bool? _isContainingCardCollapsed;
		private bool? _shouldCollapseEmptyCells;

		// JM 6-28-10 TFS33366
		private PropertyValueTracker		_parentCollectionVersionTracker;

        #endregion //Private Members

        #region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="Record"/> class
        /// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/></param>
		/// <param name="parentCollection">The containing <see cref="RecordCollectionBase"/></param>
        protected Record(FieldLayout layout, RecordCollectionBase parentCollection)
        {
            if (null == layout)
                throw new ArgumentNullException("layout");

            if (null == parentCollection)
                throw new ArgumentNullException("parentCollection");

            this._fieldLayout = layout;
            this._parentCollection = parentCollection;

			// AS 7/21/09 NA 2009.2 Field Sizing
			if (this is TemplateDataRecord == false)
				layout.MaxRecordManagerDepth = parentCollection.ParentRecordManager.NestingDepth;
        }

        #endregion //Constructors

		#region Base Class Overrides

			// JM 6-28-10 TFS33366 - Added.
			#region OnHasListenersChanged

		/// <summary>
		/// Called when th number of PropertyChanged listeners changes to or from zero.
		/// </summary>
		protected override void OnHasListenersChanged()
		{
			if (base.HasListeners	&& 
				this._index != -1	&&
				this._parentCollectionVersionTracker == null)
				this._parentCollectionVersionTracker = new PropertyValueTracker(this._parentCollection, "CollectionVersion", this.OnParentCollectionVersionChanged);
			else
				this._parentCollectionVersionTracker = null;

			base.OnHasListenersChanged();
		}

			#endregion //OnHasListenersChanged

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region DataPresenterBase

		/// <summary>
        /// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataPresenterBase DataPresenter 
        { 
            get 
            { 
                return this._parentCollection.ParentRecordManager.DataPresenter; 
            } 
        }

                #endregion //DataPresenterBase

                #region Description

        /// <summary>
        /// Gets/sets the description for the record
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Description 
        { 
            get 
            {
                if ( this._description != null && this._description.Length > 0 )
                    return this._description;

                return string.Empty;
            }
            set
            {
                if (this._description != value)
                {
                    this._description = value;
                    this.RaisePropertyChangedEvent("Description");
                }
            }
        }

                #endregion //Description

                #region ExpansionIndicatorVisibility

        /// <summary>
        /// Gets/sets if an expansion indicator is shown for this record
        /// </summary>
		/// <remarks>
		/// <para>If the <see cref="ExpansionIndicatorVisibility"/> property was not explicitly set to a value then it returns an appropriate visibility based on whether this record has children.</para>
		/// <para></para>
		/// <para>By default the property returns 'Collapsed' if there are no expandable fields. Otherwise it returns 'Visible' if there are any child records or 'Hidden' if there are none.</para>
		/// </remarks>
		/// <seealso cref="HasChildren"/>
		/// <seealso cref="ResetExpansionIndicatorVisibility"/>
		public Visibility ExpansionIndicatorVisibility
        {
            get
            {

                // JJD 4/1/08
                // Always collapse the expansion indictaor when printing
                if (this.DataPresenter is DataPresenterReportControl)
                    return Visibility.Collapsed;


                Visibility? visibility = new Visibility?();

                if (!this.CanExpand)
                {
                    visibility = Visibility.Collapsed;
                }
                // JJD 05/04/10 - TFS31349 
                // Handle GroupByRecords separately
                else if (this is GroupByRecord)
                {
                    // if it was explicitly set then return the explicit setting
                    if (this._explicitExpansionIndicatorVisibility.HasValue)
                        visibility = this._explicitExpansionIndicatorVisibility.Value;
                    else
                    {
                        // JJD 05/04/10 - TFS31349 
                        // For GroupByRecords get the GroupByExpansionIndicatorVisibilityResolved value
                        visibility = this.FieldLayout.GroupByExpansionIndicatorVisibilityResolved;

                        // JJD 05/05/10 - TFS31349
                        // If the  GroupByExpansionIndicatorVisibility is set to Collapsed 
                        // but this is is anested groupby rcd return 'Hidden' instead
                        // to enable proper record indenting.
                        if (visibility.Value == Visibility.Collapsed &&
                            this.ParentRecord is GroupByRecord)
                            visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    // if it was explicitly set then return the explicit setting
                    if (this._explicitExpansionIndicatorVisibility.HasValue)
                        visibility = this._explicitExpansionIndicatorVisibility.Value;

                    if ((!visibility.HasValue) && this is ExpandableFieldRecord)
                    {
                        if (((ExpandableFieldRecord)this).IsExpandedAlways)
                        {
                            // JJD 7/12/07
                            // The expansion indicator should be collapsed if the efr is always open
                            //visibility = Visibility.Hidden;
                            visibility = Visibility.Collapsed;
                        }
                        // JJD 4/28/08 - BR31406 and BR31707 
                        // Handle ExpandableFieldRecords in common logic below
                        //else
                        //    visibility = Visibility.Visible;
                    }

                    // JJD 4/28/08 - BR31406 and BR31707 
                    // Added ExpansionIndicatorDisplayMode property
                    ExpansionIndicatorDisplayMode displayMode;
                    DataRecord dr = this as DataRecord;
                    ExpandableFieldRecord efr = this as ExpandableFieldRecord;

                    // SSP 4/30/08 - Summaries Functionality
                    // 
                    bool recordHidesExpansionIndicator = null != dr && dr.IsAddRecord || this is SummaryRecord
                        // SSP 1/6/08 - NAS9.1 Record Filtering
                        // 
                        || this is FilterRecord;
                    bool emulateDataRecordVisibility = RecordType.DataRecord == this.ParentCollection.RecordsType;

                    // JJD 4/28/08 - BR31406 and BR31707 
                    // If ExpansionIndicatorDisplayMode is set to Never or Always then
                    // return the appropriate value for DataRecords and ExpandableFieldRecords.
                    // SSP 4/30/08 - Summaries Functionality
                    // 
                    //if (!visibility.HasValue && ( dr != null || efr != null))
                    if (!visibility.HasValue && (emulateDataRecordVisibility || efr != null))
                    {
                        displayMode = this.FieldLayout.ExpansionIndicatorDisplayModeResolved;
                        switch (displayMode)
                        {
                            case ExpansionIndicatorDisplayMode.Never:
                                visibility = Visibility.Collapsed;
                                break;
                            case ExpansionIndicatorDisplayMode.Always:
                                visibility = Visibility.Visible;
                                break;
                        }
                    }
                    else
                        displayMode = ExpansionIndicatorDisplayMode.Default;

                    // JJD 4/28/08 - BR31406 and BR31707
                    // Modified logic below to deal with DataRecords and ExpandableFieldRecords
                    //if ((!visibility.HasValue) && this is DataRecord || this is SummaryRecord)
                    // SSP 4/30/08 - Summaries Functionality
                    // 
                    //if ((!visibility.HasValue) && (dr != null || efr != null || this is SummaryRecord))
                    if ((!visibility.HasValue) && (emulateDataRecordVisibility || efr != null))
                    {
                        if (
                            // SSP 4/30/08 - Summaries Functionality
                            // Commented out below condition and added emulateDataRecordVisibility condition.
                            // 
                            //dr != null &&
                            // JJD 12/11/08 - TFS11474
                            // Use NotCollapsedExpandableFieldsCount instead so we don't get
                            // an expansion indicator when all expandable fields are collapsed
                            emulateDataRecordVisibility &&
                            (this.DataPresenter.IsNestedDataDisplayEnabled == false ||
                            //this.FieldLayout.Fields.ExpandableFieldsCount == 0))
                            this.FieldLayout.Fields.NotCollapsedExpandableFieldsCount == 0))
                            visibility = Visibility.Collapsed;
                        // SSP 4/16/08 - Summaries Functionality
                        // 
                        //else if (dr != null && dr.IsAddRecord)
                        else if (recordHidesExpansionIndicator)
                            visibility = Visibility.Hidden;
                        else
                        {
                            // JJD 4/28/08 - BR31406 and BR31707 
                            // Moved logic into helper method GetExpansionIndicatorVisibilityBasedOnContent
                            if (displayMode == ExpansionIndicatorDisplayMode.CheckOnDisplay || this.IsExpanded)
                                visibility = this.GetExpansionIndicatorVisibilityBasedOnContent();
                            else
                                visibility = Visibility.Visible;
                        }
                    }

                    // SSP 4/30/08 - Summaries Functionality
                    // 
                    if (Visibility.Collapsed != visibility && recordHidesExpansionIndicator)
                        visibility = Visibility.Hidden;

                    //if ((!visibility.HasValue) && this.HasChildren)
                    //{
                    //    if (this is DataRecord && this.ChildRecordsInternal.Count == 1)
                    //    {
                    //        ViewableRecordCollection children = this.ViewableChildRecords;

                    //        if (children.Count == 0)
                    //            visibility = Visibility.Hidden;
                    //        else
                    //        {
                    //            ExpandableFieldRecord firstChild = children[0] as ExpandableFieldRecord;

                    //            if (firstChild != null && firstChild.IsExpandedAlways)
                    //            {
                    //                if (!firstChild.HasChildren)
                    //                    visibility = Visibility.Hidden;
                    //            }
                    //        }
                    //    }

                    //    if (!visibility.HasValue) 
                    //        visibility = Visibility.Visible;
                    //}
                }

				if (!visibility.HasValue)
					visibility = Visibility.Visible;
	//				visibility = Visibility.Hidden;

				if (visibility.Value != this._expansionIndicatorVisibilityLastReturned)
				{
					this._expansionIndicatorVisibilityLastReturned = visibility.Value;

					this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");
				}
				
				return visibility.Value;
            }
			set
			{
				if (!this._explicitExpansionIndicatorVisibility.HasValue ||
					this._explicitExpansionIndicatorVisibility.Value != value)
				{
					this._explicitExpansionIndicatorVisibility = new Nullable<Visibility>( value );
					this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");
				}
			}
        }

                #endregion //ExpansionIndicatorVisibility	

                #region FieldLayout

        /// <summary>
        /// Returns the associated <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/> 
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/> 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FieldLayout FieldLayout { get { return this._fieldLayout; } }

                #endregion //FieldLayout

                // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
                #region FixedLocation

		/// <summary>
		/// Indicates whether a <see cref="Record"/> is fixed and if so to which edge.
		/// </summary>
		/// <seealso cref="IsFixed"/> 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public FixedRecordLocation FixedLocation 
        { 
            get 
            { 
                return this._fixedLocation; 
            } 
            set 
            {
                if (this._fixedLocation != value)
                {
                    if (!this.IsFixedLocationAllowed(value))
                        throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_14", this.ToString()));

                    ViewableRecordCollection vrc = this._parentCollection != null ? this._parentCollection.ViewableRecords : null;

                    Debug.Assert(vrc != null, "ViewableChildRecords should not be null");

                    this._fixedRecordLocationOverride = FixedRecordLocationInternal.Default;
                    this._fixedLocation = value;

                    if (vrc != null)
                        vrc.OnFixedStatusChanged(this);

                    this.RaisePropertyChangedEvent("IsFixed");
                    this.RaisePropertyChangedEvent("FixedLocation");

                    // JJD 6/30/09 - NA 2009 Vol 2 - Record fixing
                    this.CheckFixedAndExpandedStates();
                }
            } 
        }

                #endregion //FixedLocation

                #region HasChildren

        /// <summary>
        /// Indicates if this record has any child records (read-only)
        /// </summary>
		/// <value>True if this record has any child records</value>
		/// <remarks>
		/// <p class="note"><b>Note:</b>For <see cref="DataRecord"/>s this property will return false if its direct child <see cref="ExpandableFieldRecord"/>s have no child <see cref="DataRecord"/>s.</p>
		/// </remarks>
		public virtual bool HasChildren { get { return this.HasChildrenInternal; } }

                #endregion //HasChildren	

				#region Index

		/// <summary>
        /// The zero-based index of the record in its <see cref="ParentCollection"/> (read-only).
        /// </summary>
        public int Index
        {
            get
            {
                if (this._parentCollection == null)
                    return -1;

				if (this._index < 0 ||
					this._index >= this._parentCollection.Count ||
					this._parentCollection[this._index] != this)
				{
					// JM 6-28-10 TFS33366.
					if (this._index < 0		&& 
						base.HasListeners	&&
						this._parentCollectionVersionTracker == null)
						this._parentCollectionVersionTracker = new PropertyValueTracker(this._parentCollection, "CollectionVersion", this.OnParentCollectionVersionChanged);

					this._index = this._parentCollection.IndexOf(this);
				}

				return this._index;
            }
        }

                #endregion //Index	
     
                #region IsActive

        /// <summary>
        /// Gets/sets whether this record is set as the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>'s <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/>
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/> 
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivating"/> 
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivated"/> 
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordDeactivating"/> 
        public bool IsActive
        {
            get
            {
                return this.DataPresenter.ActiveRecord == this;
            }
            set
            {
                if (this.IsActive != value)
                {
                    if (value == true)
                        this.DataPresenter.SetActiveRecord(this, false);
                    else
                    {
                        // JJD 11/24/08 - TFS6743/BR35763
                        // Set the active record to null
                        //this.DataPresenter.SetActiveRecord(this, false);
                        this.DataPresenter.SetActiveRecord(null, false);
                    }
                }
            }
        }

                #endregion //IsActive	

				// JM 10-20-09 NA 10.1 CardView
                #region IsContainingCardCollapsed

        /// <summary>
        /// Returns/sets whether the containing <see cref="CardViewCard"/> element is collapsed to only show its Header and not its Content.
        /// </summary>
		/// <seealso cref="IsContainingCardCollapsedResolved"/>
		/// <seealso cref="CardView"/>
        /// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.CollapseCardButtonVisibility"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseCards"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public bool? IsContainingCardCollapsed
        {
            get
            {
				return this._isContainingCardCollapsed;
            }
            set
            {
				if (this._isContainingCardCollapsed != value)
                {
					this._isContainingCardCollapsed = value;
					this.RaisePropertyChangedEvent("IsContainingCardCollapsed");
					this.RaisePropertyChangedEvent("IsContainingCardCollapsedResolved");
				}
            }
		}

				#endregion //IsContainingCardCollapsed

				// JM 01-14-10 TFS25927
				#region IsContainingCardCollapsedResolved

		/// <summary>
		/// Returns whether the containing <see cref="CardViewCard"/> element is collapsed taking into account the setting of the <see cref="IsContainingCardCollapsed"/>
		/// property as well as the <see cref="CardViewSettings.ShouldCollapseCards"/> property.
		/// </summary>
		/// <seealso cref="IsContainingCardCollapsed"/>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.CollapseCardButtonVisibility"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseCards"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public bool IsContainingCardCollapsedResolved
		{
			get
			{
				if (this._isContainingCardCollapsed.HasValue)
					return this._isContainingCardCollapsed.Value;

				if (this.FieldLayout									!= null &&
					this.FieldLayout.DataPresenter						!= null &&
					this.FieldLayout.DataPresenter.CurrentViewInternal	!= null)
					return this.FieldLayout.DataPresenter.CurrentViewInternal.DefaultShouldCollapseCards;

				return false;
			}
		}

				#endregion //IsContainingCardCollapsedResolved

				#region IsEnabled

		// JJD 1/8/09 
        // Deprecate the Enabled property and replace it with IsEnabled
        /// <summary>
        /// Obsolete - use IsEnabled property instead
        /// </summary>
        /// <seealso cref="IsEnabled"/>
        /// <seealso cref="IsEnabledResolved"/>
        [Obsolete("Use IsEnabled property instead", false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Enabled
        {
            get { return this.IsEnabled; }
            set { this.IsEnabled = value; }
        }

        /// <summary>
        /// Gets/sets whether this is record is enabled
        /// </summary>
        /// <seealso cref="IsEnabledResolved"/>
        public  bool IsEnabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;
                    this.RaisePropertyChangedEvent("Enabled");
                    this.RaisePropertyChangedEvent("IsEnabled");
                    this.RaisePropertyChangedEvent("IsEnabledResolved");
                }
            }
        }

                #endregion //IsEnabled	

                // JJD 1/8/09 - NA 2009 vol 1 - record filtering
                #region IsEnabledResolved

        /// <summary>
        /// Gets whether this is record is enabled (read-only)
        /// </summary>
        /// <value>False if the <see cref="IsEnabled"/> is set to false. Otherwise true inless the record is filtered out and 
        /// the FilterAction is set to 'Disable'. 
        /// </value>
        /// <seealso cref="IsEnabled"/>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        /// <seealso cref="FieldLayoutSettings.FilterAction"/>
        public bool IsEnabledResolved
        {
            get
            {
                if (this._enabled == false)
                    return false;

				// SSP 4/10/09 TFS16485 TFS16490 - Optimizations
				// Sice now we are keeping filter action setting as part of the filter state,
				// use that.
				// 
				// ------------------------------------------------------------------------------
				if ( 0 != ( FilterState.Disable & _cachedFilterState )
					&& 0 != ( FilterState.FilteredOut & _cachedFilterState ) )
					return false;
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				// ------------------------------------------------------------------------------

                return true;
            }
        }

                #endregion //IsEnabledResolved	

                #region IsExpanded

        /// <summary>
        /// Gets/sets whether this is record is expanded to reveal its children
        /// </summary>
        public virtual bool IsExpanded
        {
            get
            {
				// SSP 6/3/09 - TFS17233 - Optimization
				// Don't bother checking CanExpand if _isExpanded is false.
				// Commented out the old code below.
				// 
				return _isExpanded && this.CanExpand;

				//if (!this.CanExpand)
				//	return false;

				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

 
                //return this._isExpanded;
            }
            set
            {

				// AS 6/3/09 NA 2009.2 ClipboardSupport
				// Moved to a helper method so we can optionally add the action to the undo stack.
				//
				//if (this.IsExpanded != value)
				//{
				//    Debug.Assert(null != this.DataPresenter);
				//    this.DataPresenter.OnRecordExpandStateChanged(this, value);
				//}
				this.ExpandCollapse(value, false);
            }
        }

                #endregion //IsExpanded	

				#region IsFixed

		
		
		/// <summary>
		/// Gets/sets whether this record should be fixed, i.e. not scrollable.
		/// </summary>
		/// <value>True is the record is fixed or false if it is not.</value>
		/// <remarks>
		/// Fixed records don't scroll until all of their scrollable sibling records have been scrolled out of view. Root level fixed records never scroll if there is enough space to display them all.
		/// <para></para>
		/// <p class="note"><b>Note:</b>Not all views support the fixing of records visually. For those views that don't support fixing, e.g. carousel view, fixing a record will just determine its order in the list.</p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Certain records' fixed status can not be changed, e.g. add records.</exception>
		/// <seealso cref="ViewableRecordCollection.CountOfFixedRecordsOnTop"/>
		/// <seealso cref="ViewableRecordCollection.CountOfFixedRecordsOnBottom"/>
		/// <seealso cref="DataRecord.IsAddRecord"/>
		public bool IsFixed
		{
			get
			{
				bool? isFixed = null;

				if ( !this.DataPresenter.CurrentViewInternal.IsFixedRecordsSupported )
					isFixed = false;

				if ( !isFixed.HasValue )
				{
					if ( FixedRecordLocationInternal.Default != _fixedRecordLocationOverride )
					{
						isFixed = FixedRecordLocationInternal.Top == _fixedRecordLocationOverride
							|| FixedRecordLocationInternal.Bottom == _fixedRecordLocationOverride;
					}
				}

                if (!isFixed.HasValue)
                {
                    // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
                    isFixed = this._fixedLocation != FixedRecordLocation.Scrollable;
                 }

				if ( isFixed != this._isFixedLastReturned )
				{
					this._isFixedLastReturned = isFixed.Value;
					this.RaisePropertyChangedEvent( "IsFixed" );
				}

				return this._isFixedLastReturned;
			}
			set
			{
                // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
                if (value == (this._fixedLocation == FixedRecordLocation.Scrollable))
				{
					if ( !this.CanFixedStatusBeChanged )
						throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_14", this.ToString( ) ) );

                    
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


                    if (this._fieldLayout != null)
                    {
                        FixedRecordLocation location = FixedRecordLocation.Scrollable;

                        if (value == true)
                        {
                            if (this._fieldLayout.AllowRecordFixingResolved == AllowRecordFixing.Bottom)
                                location = FixedRecordLocation.FixedToBottom;
                            else
                                location = FixedRecordLocation.FixedToTop;
                        }

                        this.FixedLocation = location;
                    }
                               
				}
			}
		}

		
#region Infragistics Source Cleanup (Region)




















































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //IsFixed

				#region IsSelected

		/// <summary>  
		/// Property: gets/sets whether the record is selected.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> maintains selected records, cells and fields in 
		/// <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/> object. The 
		/// <b>SelectedItem</b> object exposes <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Records"/>,
		/// <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Cells"/> and
		/// <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder.Fields"/> properties that
		/// return selected records, cells and fields respectively. When you set the 
		/// <b>Selected</b> property of a record, that record gets added
		/// to the selected records collection of the <b>SelectedItems</b> object. You can also select
		/// a record by calling the <see cref="SelectedRecordCollection.Add"/> method of the
		/// selected records collection.
		/// </p>
		/// <p class="body">
		/// <b>NOTE:</b> If you want to select a lot of records at once then use the
		/// <see cref="SelectedRecordCollection.AddRange(Record[])"/> method of the 
		/// <see cref="SelectedRecordCollection"/> for greater efficiency.
		/// </p>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="SelectedRecordCollection.AddRange(Record[])"/>
		/// </remarks>
		public bool IsSelected
		{
			get
			{
				return this._selected;
			}
			set
			{
				if (this._selected != value)
				{
					DataPresenterBase dp = this.DataPresenter;

					Debug.Assert(dp != null);

					if (null != dp)
					{
						FieldLayout fl = this.FieldLayout;
						Debug.Assert(fl != null);

						if (fl != null)
						{
							// Don't clear the existing selection when selected
							// property is set. Look at the selection strategy to
							// determine whether to clear existing selected
							// items.
							//
							bool clearExistingItems = true;

							SelectionStrategyBase selectionStrategy = fl.GetSelectionStrategyForItem(this);

							if (null != selectionStrategy)
								clearExistingItems = selectionStrategy.IsSingleSelect;

							// Also clear the selected cells as rows and cells are mutually 
							// exclusive for selection. If the user cancels the selection
							// then return without selecting the row.
							//
							if (!dp.ClearSelectedCells())
								return;

							// JM 02-05-09 TFS12744
							//dp.InternalSelectItem(this, clearExistingItems, value);
							bool selected = dp.InternalSelectItem(this, clearExistingItems, value);
							if (selected && dp.SelectedItems.Records.Count == 1)
							    ((ISelectionHost)dp).SetPivotItem(this, false);
						}
					}
				}
			}
		}

				#endregion //IsSelected	
    
				#region NestingDepth

		/// <summary>
		/// Calculates the number of parent records in this record's ancestor chain (read-only)
		/// </summary>
		/// <seealso cref="ParentRecord"/>
		public int NestingDepth
		{
			get
			{
				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				int depth = 0;
				Record parent = this.ParentRecord;

				while (parent != null)
				{
					depth++;
					parent = parent.ParentRecord;
				}

				return depth;
			}
		}

				#endregion //NestingDepth	
    
                #region ParentDataRecord

        /// <summary>
        /// Returns the parent <see cref="DataRecord"/> or null (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRecord ParentDataRecord
        {
            get
            {
                return this._parentCollection.ParentDataRecord;
            }
        }

                #endregion //ParentDataRecord

                #region ParentRecord

        /// <summary>
        /// Returns the parent record or null (read-only)
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Record ParentRecord 
        { 
            get 
            { 
                return this._parentCollection.ParentRecord; 
            } 
        }

                #endregion //ParentRecord

                #region ParentCollection

        /// <summary>
        /// Returns the collection that this record belongs to. 
        /// </summary>
        /// <seealso cref="RecordCollectionBase"/>
        public RecordCollectionBase ParentCollection { get { return this._parentCollection; } }

                #endregion //ParentCollection	
    
                #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordType"/>
        /// <seealso cref="DataRecord"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="ExpandableFieldRecord"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public abstract RecordType RecordType { get; }

                #endregion //RecordType

				// JM 10-20-09 NA 10.1 CardView
                #region ShouldCollapseEmptyCells

        /// <summary>
        /// Returns/sets whether <see cref="Cell"/>s contained in this <see cref="Record"/> should be collapsed if they contain empty values.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>If this property is set to null, <see cref="Record.ShouldCollapseEmptyCellsResolved"/> will return the value specified by <see cref="CardViewSettings.ShouldCollapseEmptyCells"/>. 
		/// This property is only honored in <see cref="CardView"/>.</para>
		/// </remarks>
		/// <seealso cref="CardView"/>
        /// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseEmptyCells"/>
		/// <seealso cref="Record.ShouldCollapseEmptyCellsResolved"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public bool? ShouldCollapseEmptyCells
        {
            get
            {
				return this._shouldCollapseEmptyCells;
            }
            set
            {
				if (this._shouldCollapseEmptyCells != value)
                {
					this._shouldCollapseEmptyCells = value;
					this.RaisePropertyChangedEvent("ShouldCollapseEmptyCells");
					this.RaisePropertyChangedEvent("ShouldCollapseEmptyCellsResolved");

					this.BumpCellLayoutVersion();
                }
            }
		}

				#endregion //ShouldCollapseEmptyCells

				// JM 10/20/09 NA 2010.1 - CardView
				#region ShouldCollapseEmptyCellsResolved
		/// <summary>
		/// Returns whether <see cref="Cell"/>s contained in this <see cref="Record"/> should be collapsed if they contain empty values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>The value returned by this property resolves (i.e., combines) the values of <see cref="CardViewSettings.ShouldCollapseEmptyCells"/> and the nullable <see cref="Record.ShouldCollapseEmptyCells"/>.  
		/// This property only has meaning in <see cref="CardView"/>.</para>
		/// </remarks>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseEmptyCells"/>
		/// <seealso cref="Record.ShouldCollapseEmptyCells"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public virtual bool ShouldCollapseEmptyCellsResolved
		{
			get { return false; }
		}
				#endregion //ShouldCollapseEmptyCellsResolved

				// JJD 2/20/08
                // Added Tag property
                #region Tag

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this object.
        /// </summary>
        public object Tag
        {
            get { return this._tag; }
            set
            {
                if (this._tag != value)
                {
                    this._tag = value;
                    this.RaisePropertyChangedEvent("Tag");
                }
            }
        }

                #endregion //Tag	
        
                #region TopLevelGroup

        /// <summary>
        /// Returns the top level group if this record is nested in <see cref="GroupByRecord"/> parent records.
        /// </summary>
        /// <remarks>Will stop walking up the parent chain when it reaches a parent record with a RecordType of DataRecord. This property may return null (read-only).</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GroupByRecord TopLevelGroup
        {
            get
            {
				Record parent = this.ParentRecord;

				// if this is the top level record then return it if it is a 
                // group by record. Otherwise return null
				if (parent == null)
                    return this as GroupByRecord;

                // If the parent record is a group by record return its TopGroup
                // which will effectively walk up the parent chain until it reaches
                // the top or encounters a data record
				if (parent is GroupByRecord)
					return parent.TopLevelGroup;

                // Return this record if it is a group by record. Otherwise return null
                return this as GroupByRecord;
            }
        }

                #endregion //TopLevelGroup

				#region UltimateParentRecord

		/// <summary>
		/// Returns the top level parent record (i.e. the record with no parent)
        /// </summary>
        /// <remarks>
		/// <para class="body">Will stop walking up the parent chain when it reaches a parent record with no parent. This property may return null (read-only).</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Record UltimateParentRecord
        {
            get
            {
				Record parent = this.ParentRecord;

				// If we have a parent return its UltimateParentRecord
                // which will effectively walk up the parent chain until it reaches
                // the top record (i.e. the record with no parent)
				if (parent != null)
					return parent.UltimateParentRecord;

                // Return this record since it doesn't have a parent record
				return this;
            }
		}

				#endregion //UltimateParentRecord

				#region ViewableChildRecords

		/// <summary>
		/// Returns a read only collection of this record's direct child records whose visibility is not set to 'Collapsed'. 
		/// </summary>
		/// <value>A <see cref="ViewableRecordCollection"/> containing all records whose <see cref="Record.Visibility"/> property is not set to 'Collapsed'.</value>
		/// <remarks>This collection also includes any special records (e.g. add records)
		/// <para></para>
		/// <p class="note"><b>Note:</b>The records are ordered exactly as they are presented in the UI.</p>
		/// </remarks>
		/// <seealso cref="ViewableRecordCollection"/>
		/// <seealso cref="DataPresenterBase.ViewableRecords"/>
		public ViewableRecordCollection ViewableChildRecords
		{
			get
			{
				RecordCollectionBase childRecords = this.ChildRecordsInternal;
				
				if ( childRecords != null )
					return childRecords.ViewableRecords;

				return null;
			}
		}

				#endregion //ViewableChildRecords

				#region Visibility

		/// <summary>
        /// Gets/sets the visibility of the record
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return this._visibility;
            }
            set
            {
                if (this._visibility != value)
                {
					// SSP 12/12/08 - NAS9.1 Record Filtering
					// Refactoring. Moved the code into the new ChangeVisibilityHelper method.
					// 
					this.ChangeVisibilityHelper( value, null );
                }
            }
        }

                #endregion //Visibility	

                #region VisibilityResolved

        /// <summary>
        /// Gets the resolved visibility of the record (read-only)
        /// </summary>
        public virtual Visibility VisibilityResolved
        {
            get
            {
				// SSP 12/12/08 - NAS9.1 Record Filtering
				// 
				//return this._visibility;
				return this.GetVisibilityResolved( true );
            }
        }

                #endregion //VisibilityResolved	

                #region VisibleIndex

        /// <summary>
        /// Returns the visible index in the collection (read-only)
        /// </summary>
        public int VisibleIndex
        {
            get
            {
				ViewableRecordCollection vrc = this._parentCollection != null ? this._parentCollection.ViewableRecords : null;

				Debug.Assert(vrc != null, "No ViewableRecordCollection for record");

				if (vrc == null)
					return -1;

				return vrc.IndexOf(this);
            }
        }

                #endregion //VisibleIndex	
    
            #endregion //Public Properties
    
            #region Internal Properties

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				#region AreChildrenAfterParent






		internal bool AreChildrenAfterParent
		{
			get { return this.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.AfterParent; }
		} 

				#endregion // AreChildrenAfterParent

                #region AssociatedField

        internal virtual Field AssociatedField { get { return null; } }

                #endregion //AssociatedField	

				#region AssociatedRecordPresenter

		internal RecordPresenter AssociatedRecordPresenter
		{
			get
			{
				if (this._associatedRecordPresenter == null)
					return null;

				RecordPresenter rp = Utilities.GetWeakReferenceTargetSafe(this._associatedRecordPresenter) as RecordPresenter;

				// verify the RecordPresenter is still valid
				if (rp == null || rp.Record != this)
                {
					// clear the weak reference
					this._associatedRecordPresenter = null;

                    // JJD 1/26/08 - BR30085
                    // Let the derived clases know that a change has happened
                    this.OnAssociatedRecordPresenterChanged();
                    return null;
                }

				return rp;
			}
			set
			{
                if (value != this.AssociatedRecordPresenter)
                {
					// AS 6/9/10 TFS34057
					// If an element is generated after the peer then unless we get into 
					// the GetChildrenCore that would not be associated with the peer so 
					// we will get/create the peer for the element if we have a peer for 
					// the item (i.e. the record) and set the element's peer->EventSource 
					// to be the record as would happen when the record peer gets its 
					// underlyingpeer.
					//
					if (value != null)
					{
						RecordAutomationPeer peer = this.AutomationPeer;

						if (null != peer)
						{
							AutomationPeer valuePeer = UIElementAutomationPeer.CreatePeerForElement(value);

							if (null != valuePeer)
								valuePeer.EventsSource = peer;
						}
					}
				

                    // JJD 1/26/08 - BR30085
                    // if the value is null then just set the _associatedRecordPresenter to null
					if (value == null)
						this._associatedRecordPresenter = null;
					else
					{
						// JJD 3/11/11 - TFS67970 - Optimization
						// Use the cvp's cached WeakRef instead
						// This prevents heap fragmentation when the cvp is recycled
						//this._associatedRecordPresenter = new WeakReference(value);
						this._associatedRecordPresenter = value.WeakRef;
					}
                    
                    // JJD 1/26/08 - BR30085
                    // Let the derived clases know that a change has happened
                    this.OnAssociatedRecordPresenterChanged();
                }
			}
		}

				#endregion //AssociatedRecordPresenter

				#region AutomationPeer
		internal RecordAutomationPeer AutomationPeer
		{
			get 
			{ 
				return this._automationPeer != null
					? (RecordAutomationPeer)Utilities.GetWeakReferenceTargetSafe(this._automationPeer)
					: null; 
			}
		} 
				#endregion //AutomationPeer

				#region CanDelete

		internal virtual bool CanDelete
		{
			get
			{
				return false;
			}
		}

				#endregion //CanDelete	

				#region CanExpand

		internal bool CanExpand
		{
			get
			{
				DataPresenterBase dp = this.DataPresenter;

				if (dp == null)
					return false;

				if (this is DataRecord &&
					dp.IsNestedDataDisplayEnabled == false)
					return false;
					
				ViewBase view = dp.CurrentViewInternal;

                // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
                //return view != null && view.SupportedDataDisplayMode == DataDisplayMode.Hierarchical;
                return view != null && view.SupportedDataDisplayMode != DataDisplayMode.Flat;
			}
		}

				#endregion //CanExpand	

				#region CanFixedStatusBeChanged

		internal virtual bool CanFixedStatusBeChanged
		{
			get
			{
				return true;
			}
		}

			   #endregion // CanFixedStatusBeChanged

				// JJD 09/22/11  - TFS84708 - Optimization
                #region ChildRecordsIfNeeded

		internal virtual RecordCollectionBase ChildRecordsIfNeeded { get { return this.ChildRecordsInternal; } }

				#endregion //ChildRecordsIfNeeded

				#region ChildRecordsInternal

		internal abstract RecordCollectionBase ChildRecordsInternal { get; }

                #endregion //ChildRecordsInternal	

                // JJD 1/19/09 - NA 2009 vol 1
                #region FarOffset

        internal double FarOffset
        {
            get
            {
                return this._fieldLayout.TemplateDataRecordCache.GetRecordOffset(this, false);
            }
        }

                #endregion //FarOffset	
    
				#region FixedRecordLocationOverride

		
		
		/// <summary>
		/// Used for overriding whether the record is fixed or not. Certain records need to
		/// be fixed even if they are not explicitly setup to be fixed. For example, all special
		/// records need to be fixed if there are any data records that are fixed. We set this
		/// property when we resolve all the special records in viewable record collection's
		/// verification logic.
		/// </summary>
		internal FixedRecordLocationInternal FixedRecordLocationOverride
		{
			get
			{
				return _fixedRecordLocationOverride;
			}
			set
			{
				_fixedRecordLocationOverride = value;
			}
		}

				#endregion // FixedRecordLocationOverride

				// JJD 3/01/ 07 - BR17658
                #region HasChildrenInternal

        internal abstract bool HasChildrenInternal { get; }

                #endregion //HasChildrenInternal	

                #region HasVisibleChildren

		// JJD 3/01/ 07 - BR17658
		// Made internal since they can get the information thru the ViewableChildRecords collection





		internal virtual bool HasVisibleChildren 
		{ 
			get 
			{
				if (!this.IsExpanded)
					return false;

				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//ViewableRecordCollection children = this.ViewableChildRecords;
				ViewableRecordCollection children = this.ViewableChildRecordsIfNeeded;

				if (children == null)
					return false;

				return children.Count > 0; 
			} 
		}

                #endregion //HasVisibleChildren	

				#region HasChildData

		// JJD 3/01/ 07 - BR17658
		// Made internal since they can get the information thru the ViewableChildRecords collection





		internal virtual bool HasChildData 
		{ 
			get 
			{
				return this.HasVisibleChildren; 
			}
		}

				#endregion //HasChildData

				// AS 6/4/07
				#region HasCollapsedAncestor
		internal bool HasCollapsedAncestor
		{
			get
			{
				Record ancestor = this.ParentRecord;

				while (ancestor != null)
				{
					if (ancestor.IsExpanded == false)
						return true;

					ancestor = ancestor.ParentRecord;
				}

				return false;
			}
		} 
				#endregion //HasCollapsedAncestor

				#region HasFilterCriteriaBeenApplied

		// SSP 12/22/11 TFS67264 - Optimizations
		// 
		/// <summary>
		/// Indicates if the filter conditions have been applied to this record.
		/// </summary>
		internal bool HasFilterCriteriaBeenApplied
		{
			get
			{
				return 0 != ( FILTERED_IN_OUT_FLAGS & _cachedFilterState );
			}
		}

				#endregion // HasFilterCriteriaBeenApplied

				// JJD 7/17/07 - BR29924
				#region IsAncestorChainExpanded

		// Returns true if the ancestor chain is expanded
		internal bool IsAncestorChainExpanded
		{
			get
			{
				Record parentRecord = this.ParentRecord;

				// if we have a opaerent record return its IsAncestorChainExpanded
				// which will effectively walk up the parent chain.
				return parentRecord == null || ( parentRecord.IsExpanded && parentRecord.IsAncestorChainExpanded);
			}
		}

				#endregion //IsAncestorChainExpanded	

				#region InternalIsFilteredOut_NoVerify

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns the filtered out state of the record without performing any verification.
		/// </summary>
		internal bool InternalIsFilteredOut_NoVerify
		{
			get
			{
				return this.InternalIsFilteredOutHelper( false );
			}
		}

				#endregion // InternalIsFilteredOut_NoVerify

				#region InternalIsFilteredOut

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns the filtered out state of the record. If filter state is dirty then
		/// it re-evaluates the filters.
		/// </summary>
		internal bool InternalIsFilteredOut_Verify
		{
			get
			{
				return this.InternalIsFilteredOutHelper( true );
			}
		}

				#endregion // InternalIsFilteredOut

				#region InternalIsFilteredOutNullable_NoVerify

		// JJD 2/2/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns the filtered out state of the record without performing any verification.
		/// </summary>
		internal bool? InternalIsFilteredOutNullable_NoVerify
		{
			get
			{
				return this.InternalIsFilteredOutNullableHelper( false );
			}
		}

				#endregion // InternalIsFilteredOutNullable_NoVerify

				#region InternalIsFilteredOutNullable

		// JJD 2/2/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns the filtered out state of the record. If filter state is dirty then
		/// it re-evaluates the filters.
		/// </summary>
		internal bool? InternalIsFilteredOutNullable_Verify
		{
			get
			{
				return this.InternalIsFilteredOutNullableHelper( true );
			}
		}

				#endregion // InternalIsFilteredOutNullable

                // JJD 05/06/10 - TFS27757 added
				#region IsActivatable

        /// <summary>
        /// Property: Returns true only if the record can be activated
        /// </summary>
        internal protected virtual bool IsActivatable
        {
            get
            {
                // Check if the record is disabled.
                //
                if (!this.IsEnabledResolved)
                    return false;

                return this.VisibilityResolved == Visibility.Visible;
            }
        }

                #endregion // IsActivatable

				#region IsDataRecord

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Indicates if this is a data record with a data item from the data source.
		/// </summary>
		// JJD 10/26/11 - TFS91364 - Make property public
		//internal virtual bool IsDataRecord
		public virtual bool IsDataRecord
		{
			get
			{
				return false;
			}
		}

				#endregion // IsDataRecord
    
				// AS 5/19/09 TFS17455
				#region IsIndexValid
		internal virtual bool IsIndexValid
		{
			get
			{
				if (this.IsSpecialRecord)
					return true;

				return this.Index >= 0;
			}
		} 
				#endregion //IsIndexValid

				#region IsOnTopWhenFixed

		internal virtual bool IsOnTopWhenFixed
		{
			get
			{
				// SSP 4/25/08 - Summaries/Fixed Records
				// 
				if ( FixedRecordLocationInternal.Top == _fixedRecordLocationOverride )
					return true;
				else if ( FixedRecordLocationInternal.Bottom == _fixedRecordLocationOverride )
					return false;

				DataRecord dr = this as DataRecord;

                if (dr != null)
                {
                    if (dr.IsAddRecord)
                    {
                        switch (this.ParentCollection.ParentRecordManager.AddNewRecordLocation)
                        {
                            case AddNewRecordLocation.OnTopFixed:
                            case AddNewRecordLocation.OnTop:
                                return true;
                        }
                    }
                    else
                    {
                        // JJD 6/10/09 - NA 2009 Vol 2 - Record fixing
                        // take into account explicit _fixedLocation setting
                        switch (this._fixedLocation)
                        {
                            case FixedRecordLocation.FixedToTop:
                                return true;
                            case FixedRecordLocation.FixedToBottom:
                                return false;
                        }

                        // JJD 6/10/09 - NA 2009 Vol 2 - Record fixing
                        // Return false if AllowRecordFixingResolved is Bottom
                        if (this._fieldLayout != null &&
                            this._fieldLayout.AllowRecordFixingResolved == AllowRecordFixing.Bottom)
                            return false;

                    }
                }
				return true;
			}
		}

				#endregion //IsOnTopWhenFixed

				#region IsSelectable

        /// <summary>
        /// Property: Returns true only if the record can be selected
        /// </summary>
        internal protected virtual bool IsSelectable
        {
            get
            {
                SelectionStrategyBase strategy = this.SelectionStrategyDefault;
                if (null == strategy || strategy is SelectionStrategyNone)
                    return false;

                // Also check if the Activation is set to Disabled.
                //
                if (!this.IsEnabledResolved)
                    return false;

                return true;
            }
        }

                #endregion // IsSelectable

				#region IsSpecialRecord

		// SSP 8/5/09 - NAS9.2 Enhanced grid view
		// Made public.
		// 
		//internal virtual bool IsSpecialRecord
		/// <summary>
		/// Indicates if this record is a special record (filter record, summary record, template add-record etc...).
		/// </summary>
		public virtual bool IsSpecialRecord
		{
			get
			{
				return false;
			}
		}

			   #endregion // IsSpecialRecord

				// AS 5/19/09 TFS17455
				#region IsStillValid
		/// <summary>
		/// Indicates if the record is still within the associated <see cref="DataPresenter"/>
		/// </summary>
		internal virtual bool IsStillValid
		{
			get
			{
				if (!IsIndexValid)
					return false;

				Record parentRecord = this.ParentRecord;

				if (parentRecord != null)
					return parentRecord.IsStillValid;

				DataPresenterBase dp = this.DataPresenter;

				if (null == dp)
					return false;

				RecordCollectionBase parentCollection = this.ParentCollection;

				if (null == parentCollection)
					return false;

				return parentCollection.ParentRecordManager == dp.RecordManager;
			}
		} 
				#endregion //IsStillValid

				// AS 5/12/09
				#region IsTemplateDataRecord
		/// <summary>
		/// Returns true if the record is the template record created by the FieldLayout for measurement, etc.
		/// </summary>
		// AS 6/24/09 NA 2009.2 Field Sizing
		//internal bool IsTemplateDataRecord
		internal virtual bool IsTemplateDataRecord
		{
			get
			{
				// AS 6/24/09 NA 2009.2 Field Sizing
				//return null != _fieldLayout && _fieldLayout.TemplateDataRecord == this;
				return false;
			}
		}
				#endregion //IsTemplateDataRecord

                #region NestedContent

        // need to expose this dummy property so that we can use the same GroupByRecordStyle
        // without generating binding erros
        internal FrameworkElement NestedContent { get { return null; } }

                #endregion //NestedContent	
 
                // JJD 9/26/08 - added
				#region NestingDepthOfGroupByRcds

        internal int NestingDepthOfGroupByRcds
		{
			get
			{
				GroupByRecord parent = this.ParentRecord as GroupByRecord;

				// If we have a parent groupby record then return its nesting depth plus 1
				if (parent != null)
					return parent.NestingDepthOfGroupByRcds + 1;

				//Otherwise, return 0 for a root record
				return 0;
			}
		}

				#endregion //NestingDepthOfGroupByRcds	
 
				#region OccupiesScrollPosition

		internal virtual bool OccupiesScrollPosition { get { return true; } }

				#endregion //OccupiesScrollPosition	
    
                #region OverallScrollPosition

        internal int OverallScrollPosition
        {
            get
            {
                // JJD 05/06/10 - TFS27757
                // Moved logic into helper method
                bool dummy = false;
                return GetOverallScrollPosition(false, ref dummy);
            }
        }

                #endregion //OverallScrollPosition	
    
                #region OverallSelectionPosition







        internal long OverallSelectionPosition
        {
            get
            {
				// JJD 1/10/12 - TFS99025
				// First check if the OverallScrollPosition is negative. 
				// This can happen if one of its ancestor records is collapsed.
				//return ((long)this.OverallScrollPosition << 32) + this.Index;
				long position = this.OverallScrollPosition;

				if (position < 0)
				{
					// JJD 1/10/12 - TFS99025
					// Use 1 plus the parent's OverallSelectionPosition.
					// Note: this will effectivels walk up the parent chain until
					// it finds an un-collapsed ancestor.
					Record parent = this.ParentRecord;

					if (parent != null)
						position = 1 + parent.OverallSelectionPosition;
				}
				else
				{
					// bit shift the scroll position
					position <<= 32;
				}

				return position + this.Index;
            }
        }

                #endregion //OverallSelectionPosition	

				#region ParentRecordList





		internal RecordListControl ParentRecordList
		{
			get
			{
				RecordListControl rl = null;

				if (this.ParentCollection != null)
				{
					rl = this.ParentCollection.LastRecordList;

					if (null == rl &&
						this.ParentCollection.ParentRecordManager != null)
					{
						ViewableRecordCollection visibleRecords = this.ParentCollection.ParentRecordManager.ViewableRecords as ViewableRecordCollection;

						if (null != visibleRecords)
							rl = visibleRecords.LastRecordList;
					}
				}

				return rl;
			}
		} 
				#endregion //ParentRecordList

                // JJD 8/13/09 - NA 2009 Vol 2 - Enhanced grid view
                #region RecordForLayoutCalculations

        internal virtual Record RecordForLayoutCalculations
        {
            get { return this; }
        }

                #endregion //RecordForLayoutCalculations	
    
				#region RecordManager

		
		
		/// <summary>
		/// Returns the associated <see cref="RecordManager"/>
		/// </summary>
		// AS 5/13/09 NA 2009.2 Undo/Redo
		//internal RecordManager RecordManager
		public RecordManager RecordManager
		{
			get
			{
				RecordCollectionBase parentCollection = _parentCollection;
				return null != parentCollection ? parentCollection.ParentRecordManager : null;
			}
		}

				#endregion // RecordManager

				// AS 7/21/09 NA 2009.2 Field Sizing
				#region RecordManagerNestingDepth
		internal int RecordManagerNestingDepth
		{
			get
			{
				RecordManager rm = this.RecordManager;

				return null != rm ? rm.NestingDepth : -1;
			}
		} 
				#endregion //RecordManagerNestingDepth

				#region SelectionStrategyDefault

		internal SelectionStrategyBase SelectionStrategyDefault
		{
			get
			{
				if (this._fieldLayout == null)
					return new SelectionStrategyNone(this.DataPresenter as ISelectionHost);

				return this._fieldLayout.GetSelectionStrategyForItem(this);
			}
		}

				#endregion //SelectionStrategyDefault	
        
                #region ScrollCountInternal





        internal int ScrollCountInternal
        {
            get
            {
				// special records need to return 0 so they don't appear twice
				// in the ViewableRecordsCollection
				if (this.IsSpecialRecord)
					return 0;

				// if the visibility is set to collapsed return 0
				if (this.VisibilityResolved == Visibility.Collapsed)
					return 0;
 
				int count = 0;

				if ( this.OccupiesScrollPosition == true )
					count++;

				// if this record has children and is expanded
				// add in the descendant count 
				if (this.IsExpanded && 
					this.HasChildrenInternal)
				{
					// JJD 2/26/07 - BR20623
					// Add to the count don't replace it
					//count = this.ViewableChildRecords.ScrollCount;
					// JJD 09/22/11  - TFS84708 - Optimization
					// Use ViewableChildRecordsIfNeeded instead
					//count += this.ViewableChildRecords.ScrollCount;
					ViewableRecordCollection vcr = this.ViewableChildRecordsIfNeeded;

					if ( vcr != null )
						count += vcr.ScrollCount;
				}



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


				return count;
            }
        }

                #endregion // ScrollCountInternal

				#region SeparatorVisibility

		// SSP 5/6/08 - Summaries Feature
		// 
		internal RecordSeparatorVisibility SeparatorVisibility
		{
			get
			{
				return _separatorVisibility;
			}
			set
			{
				if ( _separatorVisibility != value )
				{
					_separatorVisibility = value;
					this.RaisePropertyChangedEvent( "SeparatorVisibility" );
				}
			}
		}

				#endregion // SeparatorVisibility

                // JJD 12/08/08 - Added 
                #region TopLevelDataRecord

        internal DataRecord TopLevelDataRecord
        {
            get
            {
                DataRecord parentDataRecord = this.ParentDataRecord;

                if (parentDataRecord != null)
                    return parentDataRecord.TopLevelDataRecord;

                return this as DataRecord;
            }
        }

                #endregion //TopLevelDataRecord	

                // JJD 8/13/09 - NA 20009 Vol 2 - Enhanced grid view - added
                #region TopLevelGroupByFieldRecord

        internal GroupByRecord TopLevelGroupByFieldRecord
        {
            get
            {
				Record parent = this.ParentRecord;

				// if this is the top level record then return it if it is a 
                // group by record. Otherwise return null
                if (parent == null)
                {
                    if (this.RecordType == RecordType.GroupByField)
                        return this as GroupByRecord;
                    else
                        return null;
                }

                // If the parent record is a group by field record return its TopGroup
                // which will effectively walk up the parent chain until it reaches
                // the top or encounters a data record
				if (parent.RecordType == RecordType.GroupByField)
					return parent.TopLevelGroupByFieldRecord;

                // Return this record if it is a group by field record. Otherwise return null
                if (this.RecordType == RecordType.GroupByField)
                    return this as GroupByRecord;

                return null;
            }
        }

                #endregion //TopLevelGroupByFieldRecord

				// AS 2/24/11 NA 2011.1 Word Writer
				#region ViewableChildRecordScrollCount
		internal int ViewableChildRecordScrollCount
		{
			get
			{
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//ViewableRecordCollection viewableRecords = this.ViewableChildRecords;
				ViewableRecordCollection viewableRecords = this.ViewableChildRecordsIfNeeded;
				return viewableRecords == null ? 0 : viewableRecords.ScrollCount;
			}
		} 
				#endregion //ViewableChildRecordScrollCount

				// JJD 10/20/11  - TFS84708 - Optimization
				#region ViewableChildRecordsIfNeeded

		// JJD 10/20/11  - TFS84708 - Optimization
		// added virtual property for getting the ViewableChildRecords only if they are required
		internal virtual ViewableRecordCollection ViewableChildRecordsIfNeeded
		{
			get
			{
				RecordCollectionBase childRecords = this.ChildRecordsIfNeeded;

				if (childRecords != null)
					return childRecords.ViewableRecords;

				return null;
			}
		}

				#endregion //ViewableChildRecordsIfNeeded	
    
				#region VisibilityResolved_NoFiltering

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns the resolved visibility of the record without taking into account the filtering criteria.
		/// That is even if the record is filtered out, this property may return true if nothing else hides this
		/// record.
		/// </summary>
		internal Visibility VisibilityResolved_NoFiltering
		{
			get
			{
				return _visibility;
			}
		}

				#endregion // VisibilityResolved_NoFiltering
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region IsAncestorOf

        /// <summary>
        /// Determines if this record is anywhere in the parent chain of the record being tested.
        /// </summary>
        /// <param name="record">The record being tested</param>
        /// <returns>Trus if this record in anywhere in the parent chain of the passed in record. Otherwise it returns false.</returns>
        public bool IsAncestorOf(Record record)
        {
			
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			if (record == this)
				return false;

			while (null != record)
			{
				Record parentRecord = record.ParentRecord;

				if (parentRecord == this)
					return true;

				record = parentRecord;
			}

			return false;
		}

                #endregion //IsAncestorOf

				#region RefreshSortPosition

		// SSP 7/29/08 
		// Added this method to position the row according to the current
		// sort criteria without having to resort the whole rows collection.
		// 
		/// <summary>
		/// If the record is not at correct sort position in the record collection,
		/// this method will reposition it at the correct sort position based on
		/// the current sort criteria. Also if the record is in the wrong group record, 
		/// it will be moved to the correct group record.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method can be useful in situations where a new record is added,
		/// which by default is appended at the end of the collection, and you want 
		/// to ensure the record is positioned at the correct location in the 
		/// collection based on the sort criteria without having to resort the whole 
		/// record collection. This method should not be used if the sort criteria 
		/// itself changes which affects the whole record collection.
		/// </p>
		/// <p class="body">
		/// To resort all the records of a record collection, call RecordManager's
		/// <see cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.RefreshSort"/> method.
		/// </p>
		/// <p class="note">
		/// <b>Note: </b>This method should not be called directly from within the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeRecordEvent"/>.
		/// If the method needs to be called from within this event, you should do so asynchronously using Dispatcher.BeginInvoke
		/// </p>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordCollectionBase.RefreshSort"/>
		/// </remarks>
		public virtual void RefreshSortPosition( )
		{
 			// If this row is not in the correct group, then reposition it
			// in the correct group.
			//
			RecordCollectionBase parentCollection = this.ParentCollection;
			RecordManager recordManager = null != parentCollection ? parentCollection.ParentRecordManager : null;

            // JJD 12/02/08 - TFS6743/BR35763
            // If the rm is in Reset then we can ignore this request since 
            // all the records will be sorted in one shot
            if (recordManager == null ||
                 recordManager.IsInReset)
                return;
            
            GroupByRecord parentGroupByRecord = this.ParentRecord as GroupByRecord;
			if ( null != parentGroupByRecord )
			{
				if ( this is DataRecord && !parentGroupByRecord.DoesRecordMatchGroup( (DataRecord)this ) )
				{
					// Remove the record and re-add it to position it in the correct group.

					parentGroupByRecord.ChildRecords.ViewableRecords.RemoveRecord( this, true );

					recordManager.Groups.ViewableRecords.InsertDataRecordInGroupBySlot( (DataRecord)this );
				}

				parentGroupByRecord = this.ParentRecord as GroupByRecord;

				// Since the data record was part of a group, after above operation, it should be
				// part of a group as well, although the group might have changed.
				// 
				Debug.Assert( null != parentGroupByRecord );
			}

            // JJD 7/1/09 - NA 2009 Vol 2 - Record fixing
            // Call OnRecordRefreshSortPosition off viewableRecordscollection
			
			
            
            ViewableRecordCollection vrc = this.ParentCollection.ViewableRecords;

            if (vrc != null)
                vrc.OnRecordRefreshSortPosition(this);

			// SSP 9/19/2011 TFS88364
			// If the record was moved from one group to another then re-calc the summaries
			// of the affected groups.
			// 
			// ------------------------------------------------------------------------------
			RecordCollectionBase newParentCollection = this.ParentCollection;
			if ( newParentCollection != parentCollection )
			{
				RecordManager.DirtySummaryResults( null, null, parentCollection, null );
				RecordManager.DirtySummaryResults( null, null, newParentCollection, null );
			}
			// ------------------------------------------------------------------------------
		}

				#endregion // RefreshSortPosition

				#region ResetExpansionIndicatorVisibility

		/// <summary>
		/// Resets the value of the <see cref="ExpansionIndicatorVisibility"/> property.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="ExpansionIndicatorVisibility"/> property was explicitly set to a value then this method removes that explicit setting and allows the property to return the appropriate visibility based on whether this record has children.</para>
		/// </remarks>
		/// <seealso cref="ExpansionIndicatorVisibility"/>
		/// <seealso cref="HasChildren"/>
		public void ResetExpansionIndicatorVisibility()
		{
			if (this._explicitExpansionIndicatorVisibility.HasValue)
			{
				// getbthe old value
				Visibility oldValue = this._explicitExpansionIndicatorVisibility.Value;

				// reset the explicit member
				this._explicitExpansionIndicatorVisibility = new Nullable<Visibility>();

				// if the value has changed then raise the property changed event
				if (oldValue != this.ExpansionIndicatorVisibility)
					this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");
			}
		}

				#endregion //ResetExpansionIndicatorVisibility	
    
            #endregion //Public Methods

            #region Internal Methods

				// AS 10/27/09 NA 2010.1 - CardView
				#region BumpCellLayoutVersion
		internal void BumpCellLayoutVersion()
		{
			if (_associatedRecordPresenter != null)
			{
				this.RaisePropertyChangedEvent("CellLayoutVersion");
			}
		} 
				#endregion //BumpCellLayoutVersion
        
                // JJD 1/19/09 - NA 2009 vol 1
                #region CalculateExtraOffsetInGroupBy

        internal double CalculateExtraOffsetInGroupBy(bool isHeader)
        {
            HeaderRecord hr = this as HeaderRecord;

            // JJD 8/13/09 - NA 2009 Vol 2 - Enhanced grid view
            // If this is a HeaderRecord then calcluate it based
            // on its attacted to record
            if (hr != null)
            {
                // JJD 11/10/09 - TFS24243
                // Get the logical rcd to use for calculations instead
                //Record rcd = hr.AttachedToRecord;
                Record rcd = this.RecordForLayoutCalculations;

                if (rcd != null && rcd != this)
                    return rcd.CalculateExtraOffsetInGroupBy(isHeader);

                return 0;
            }

            if (this._fieldLayout.HasGroupBySortFields)
            {
                // JJD 6/4/09 - TFS17060
                #region Special case logic when we are in a ReportControl

                DataPresenterBase dp = this.DataPresenter;

                if (dp == null)
                    return 0;

                // JJD 6/4/09 - TFS17060
                // see if we are in a DataPresenterReportControl
                if (dp.IsReportControl)
                {
                    TabularReportView view = dp.CurrentViewInternal as TabularReportView;

                    if (view == null)
                        return 0;

                    // JJD 6/4/09 - TFS17060
                    // get the level indentation setting from the viwe
                    double levelIndentation = view.LevelIndentation;

                    // JJD 6/4/09 - TFS17060
                    // return the level indenation times the # of groupby fields
					// JJD 4/14/11 - TFS70716
					// Net out the groupby nesting depth of this record
					//return levelIndentation * this._fieldLayout.SortedFields.CountOfGroupByFields;
                    return levelIndentation * Math.Max( this._fieldLayout.SortedFields.CountOfGroupByFields - this.NestingDepthOfGroupByRcds, 0);
                }

                #endregion //Special case logic when we are in a ReportControl	
    
                GroupByRecord parentGroupBy = this.ParentRecord as GroupByRecord;

                RecordManager rm = this.RecordManager;

                bool isSiblingOfGroupByRootRcds = false;

                if ( parentGroupBy == null )
                {
                    // AS 4/17/09 TFS16573
                    // Added isHeader checks since the header record presenter may not
                    // be associated with a record but may still be displayed at the top
                    // sibling to the group by records.
                    //
                    if (this is FilterRecord ||
                        this is SummaryRecord ||
						// AS 9/3/09 TFS21581
						// The RecordContentMarginConverter Convert method gets the first 
						// record in the viewable record collection. When that is a group 
						// by record, the summaries for the root group by record were not 
						// lining up because the parentGroupBy we obtain here is null.
						//
                        //(this is GroupByRecord && isHeader) ||
                        //(isHeader && this._fieldLayout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly)
						this is GroupByRecord
                        )
                    {
                        if (rm != null &&
                             rm.Groups.Count > 0)
                        {
                            parentGroupBy = rm.Groups[0];
                            isSiblingOfGroupByRootRcds = true;
                        }
                    }
                }

                if ( parentGroupBy == null )
                    return 0d;

                double offsetPerLevel = this._fieldLayout.TemplateDataRecordCache.GetRecordOffset(parentGroupBy, true);

                int count = this._fieldLayout.SortedFields.CountOfGroupByFields;

                // JJD 10/19/09 - TFS23161
                // If we are also grouping by FieldLayout (i.e. heterogenous data) then add one to the count
                // to allow for the extra level of grouping
                if (rm.HasGroups && rm.Groups[0].RecordType == RecordType.GroupByFieldLayout)
                    count++;

                if (isSiblingOfGroupByRootRcds == false)
                    count = Math.Max(0, count - this.NestingDepthOfGroupByRcds);

                double offset = count * offsetPerLevel;
                
                // JJD 05/05/10 - TFS31349
                // If GroupByExpansionIndicatorVisibility is set to Collapsed 
                // adjust the offset value back by one level to account for the collapsed
                // expandion indicator in the root group by rcd.
                // Note: nested groupby rcds will set there expansion indicator visibility to
                // 'Hidden' in this case to enable proper indenting
                if (count > 0 && this._fieldLayout.GroupByExpansionIndicatorVisibilityResolved == Visibility.Collapsed)
                    offset -= offsetPerLevel;

                // AS 3/25/09 TFS15801
                // We were not accounting for the chrome between the nested content's items presenter
                // and the nested content site.
                //
                offset += count * this._fieldLayout.TemplateDataRecordCache.GetNestedContentChrome(parentGroupBy, true);

                
                // JJD 8/25/09 - TFS20982 
                // If we are in a child record island then we need to adjust for 
                // the ExpansionIndicator of our child records 
                //if (isHeader == false && rm.Sorted.Count > 0)
                if (rm.Sorted.Count > 0)
                {
					// AS 9/9/09 TFS21581
					// We're handling collapsing the expansion indicator in the record presenter itself.
					//
                    //if (isHeader == false ||
                    //    (isSiblingOfGroupByRootRcds == false && this.ParentDataRecord != null) ||
					//	// AS 9/3/09 TFS21581
					//	// After talking to Joe about this, it seems that we should not get 
					//	// in here for a header that is attached to the root group by record.
					//	//
					//	//(this.RecordType == RecordType.GroupByField && !this.DataPresenter.IsFlatView))
					//	(this.RecordType == RecordType.GroupByField && !this.DataPresenter.IsFlatView && parentGroupBy.ParentCollection != this.ParentCollection))
					if (isHeader == false)
                    {
                        Record rcd = rm.Sorted[0];

                        if (rcd.ExpansionIndicatorVisibility == Visibility.Collapsed)
                        {
                            if (this._fieldLayout.IsHorizontal)
                                offset -= this._fieldLayout.ExpansionIndicatorSize.Height;
                            else
                                offset -= this._fieldLayout.ExpansionIndicatorSize.Width;
                        }
                    }
                }

                return offset;
            }

            return 0d;
        }

                #endregion //CalculateExtraOffsetInGroupBy	

                #region CalculateFlatViewIndent

        internal double CalculateFlatViewIndent()
        {
            HeaderRecord hr = this as HeaderRecord;

            if (hr != null)
            {
				
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)


				// JJD 11/10/09 - TFS24243
				// Get the logical rcd to use for calculations
				Record logicalRecord = this.RecordForLayoutCalculations;

                if (logicalRecord == this || logicalRecord == null)
                    return 0;

                return logicalRecord.CalculateFlatViewIndent();
            }

			Record parent = this.ParentRecord;

			if (parent == null)
				return 0;

			double indent = 0;

            // AS 9/9/09 TFS21581
            // In a flat situation we need to account for the chrome that would have been there 
            // if this record's elements were actually nested within the parent's record element.
            //
            indent += parent.FieldLayout.TemplateDataRecordCache.GetNestedContentChrome(parent, true);

			FieldLayout fl = parent.FieldLayout;

			if (parent.ExpansionIndicatorVisibility != Visibility.Collapsed)
			{
				indent += fl.IsHorizontal ? fl.ExpansionIndicatorSize.Height : fl.ExpansionIndicatorSize.Width;
			}

			indent += parent.CalculateFlatViewIndent();

			return indent;
		} 

                #endregion //CalculateFlatViewIndent	
    
                // JJD 1/19/09 - NA 2009 vol 1
                #region CalculateNestedRecordContentNearOffset

        internal double CalculateNestedRecordContentNearOffset()
        {
            
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


            // JJD 8/13/09 - NA 2009 Vol 2 - Enhanced grid view
            // Use RecordForLayoutCalculations property instead
            //Record record = this;
            Record record = this.RecordForLayoutCalculations;

            DataPresenterBase dp = record.DataPresenter;

            double offset = 0;

            while (null != record)
            {
                FieldLayout fl = record.FieldLayout;

                double recordOffset = fl.TemplateDataRecordCache.GetRecordOffset(record, true);
                Size expansionSize = fl.ExpansionIndicatorSize;
                double expansionIndicatorOffset = fl.IsHorizontal ? expansionSize.Height : expansionSize.Width;

                if (recordOffset >= expansionIndicatorOffset)
                {
                    if (record.ExpansionIndicatorVisibility == Visibility.Collapsed)
                        recordOffset -= expansionIndicatorOffset;
                }

                // JJD 8/13/09 - NA 2009 Vol 2 - Enhanced grid view
                // for flat view just return the record indent
                if (dp != null && dp.IsFlatView)
                    return recordOffset + this.CalculateFlatViewIndent();

                offset += recordOffset;

                // get the parent's parent walking up the parent chain
                Record parentRecord = record.ParentRecord;

                if (null != parentRecord)
                {
                    // add in nested content site chrome
                    offset += parentRecord.FieldLayout.TemplateDataRecordCache.GetNestedContentChrome(parentRecord, true);
                }

                record = parentRecord;
            }

            return offset;
        }

                #endregion //CalculateNestedRecordContentNearOffset	
    
				#region ChangeVisibilityHelper

		// SSP 12/12/08 - NAS9.1 Record Filtering
		// Refactoring. Code in this method is moved from the Visibility property setter.
		// 
		/// <summary>
		/// Called when the Visibility or VisibilityResolved property value changes. Raises 
		/// appropriate property change notifications. If newVisibility is non-null, then 
		/// the _visibility member variable is updated to that value. Otherwise 
		/// VisibilityResolved is assumed to be the property that's affected.
		/// </summary>
		/// <param name="newVisibility">If the change in VisibilityResolved is due to Visibility property set, this will be new value of the property.</param>
		/// <param name="newFilterState">If the change in VisibilityResolved is due to new filter state, this will be the new filter state.</param>
		internal void ChangeVisibilityHelper( Visibility? newVisibility, FilterState? newFilterState )
		{
			RecordCollectionBase parentCollection = this.ParentCollection;
			ViewableRecordCollection viewableSiblings = parentCollection.ViewableRecords;

			int oldVisibleIndex = -1;

			// SSP 1/12/09 - NAS9.1 Record Filtering
			// Pass false for verify. We don't want any verification to occur during the process
			// and potentially recursively have this called.
			// 
			// ----------------------------------------------------------------------------------
			bool vrcNotificationsSuspended = null == viewableSiblings || !viewableSiblings.ShouldRaiseCollectionNotifications;
			Visibility oldVisibilityResolved = Visibility.Visible;
			if ( !vrcNotificationsSuspended && viewableSiblings != null )
			{
				oldVisibilityResolved = this.GetVisibilityResolved( false );
				if ( oldVisibilityResolved != Visibility.Collapsed )
					oldVisibleIndex = viewableSiblings.GetIndexOf( this, false );
			}
			



			// ----------------------------------------------------------------------------------

			if ( newVisibility.HasValue )
				this._visibility = newVisibility.Value;

			// SSP 1/10/09 NAS9.1 Record Filtering
			// 
			if ( newFilterState.HasValue )
				_cachedFilterState = newFilterState.Value;

			// SSP 2/12/09 TFS12467
			// Moved this here from below.
			// 
			this.DirtyScrollCount( );

			// SSP 5/5/08 - Summaries Feature
			// If a visibility of a record changes, it may impact the summary calculations
			// if calculation scope is to include only visible records.
			// 
			FieldLayout fieldLayout = _fieldLayout;
			bool notifyCalcManager = false;
			if ( null != fieldLayout && CalculationScope.FilteredSortedList == fieldLayout.CalculationScopeResolved )
			{
				RecordManager.DirtySummaryResults( this, null, null, null );

				// SSP 4/23/12 TFS107881
				// 
				notifyCalcManager = true;
			}

			// AS 8/3/09 NA 2009.2 Field Sizing
			if (null != _fieldLayout)
				_fieldLayout.AutoSizeInfo.OnRecordVisibilityChanged(this);

			// JJD 4/14/07
			// let the parent's ViewableRecordCollection know so that it can adjust its 
			// counts appropriately
			// SSP 1/12/09 - NAS9.1 Record Filtering
			// 
			//if ( viewableSiblings != null )
			if ( ! vrcNotificationsSuspended && viewableSiblings != null )
				viewableSiblings.OnRecordVisibilityChanged( this, oldVisibilityResolved, oldVisibleIndex );

			// SSP 2/12/09 TFS12467
			// Moved this above and also changed it to DirtyScrollCount call which does the same thing.
			// 
			//parentCollection.SparseArray.NotifyItemScrollCountChanged( this );

			if ( newVisibility.HasValue )
				this.RaisePropertyChangedEvent( "Visibility" );

			// SSP 1/10/09 NAS9.1 Record Filtering
			// 
			// SSP 4/10/09 TFS16485 TFS16490
			// Moved the following code into the FilterStateChanged_RaiseNotificationsHelper.
			// 
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			this.RaisePropertyChangedEvent( "VisibilityResolved" );

			// SSP 4/23/12 TFS107881
			// 
			if ( notifyCalcManager )
				GridUtilities.NotifyCalcAdapter( this.DataPresenter, this, "Visibility", null );
        }

				#endregion // ChangeVisibilityHelper

				#region ClearSizeToContentManager

		
		
		
		internal void ClearSizeToContentManager( )
		{
			this.ClearSizeToContentManager( null );
		}

		internal void ClearSizeToContentManager( Cell cell )
		{
            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			if (null != this._layoutManager)
			{
				this._layoutManager.InvalidateLayout();

				// AS 10/27/09 NA 2010.1 - CardView
				this.BumpCellLayoutVersion();
			}
		}
				#endregion //ClearSizeToContentManager

                // JJD 4/3/08 - added support for printing
                #region CloneAssociatedRecordSettings


        // MBS 7/28/09 - NA9.2 Excel Exporting
        //internal virtual void CloneAssociatedRecordSettings(Record associatedRecord, ReportViewBase reportView)
        internal virtual void CloneAssociatedRecordSettings(Record associatedRecord, IExportOptions options)
        {
            // MBS 7/28/09 - NA9.2 Excel Exporting
            Debug.Assert(options != null, "Expected to have a non-null IExportOptions");

            this._description           = associatedRecord._description;

            // JJD 6/17/09 - NA 2009 Vol 2 - Record fixing
            // Copy location and searator visibility
            //this._isFixed = associatedRecord._isFixed;
            this._fixedLocation         = associatedRecord._fixedLocation;
            this._separatorVisibility   = associatedRecord._separatorVisibility;

            this._enabled               = associatedRecord._enabled;
            this._tag                   = associatedRecord._tag;

			// JM 06-30-10 TFS23300
			this._selected				= associatedRecord._selected;

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if ( !reportView.ExcludeExpandedState )
            if(options != null && !options.ExcludeExpandedState)
                this._isExpanded        = associatedRecord._isExpanded;
            else
                this._isExpanded        = true;

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (!reportView.ExcludeRecordVisibility)
            if(options == null || !options.ExcludeRecordVisibility)
            {
                // JJD 10/16/08 - TFS8092
                // Set the public property instead of the meber since that
                // will update the scroll count's correctly
                //this._visibility = associatedRecord._visibility;
                this.Visibility = associatedRecord._visibility;
            }
  
            // JJD 1/16/09 - NA 2009 col 1 - Record filtering
            //
            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (!reportView.ExcludeRecordFilters)
            if(options == null || !options.ExcludeRecordFilters)
                this._cachedFilterState = associatedRecord._cachedFilterState;

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            #region Clone Record Sizing

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (associatedRecord._layoutManager != null && !reportView.ExcludeRecordSizing)
            ReportViewBase reportView = options as ReportViewBase;
            if(associatedRecord._layoutManager != null && reportView != null && !reportView.ExcludeRecordSizing)
            {
                FieldGridBagLayoutManager destLm = this.GetLayoutManager(true);

                // MBS 7/31/09 - NA9.2 Excel Exporting
                //DataPresenterReportControl rc = this.DataPresenter as DataPresenterReportControl;
                DataPresenterExportControlBase rc = this.DataPresenter as DataPresenterExportControlBase;

                Debug.Assert(null != rc);

                if (null != rc)
                {
                    foreach (Infragistics.Controls.Layouts.Primitives.ILayoutItem item in associatedRecord._layoutManager.LayoutItems)
                    {
                        CellLayoutItem srcItem = item as CellLayoutItem;

                        if (null != srcItem)
                        {
                            Field destField = rc.GetClonedField(srcItem.Field);
                            CellLayoutItem destItem = destLm.GetLayoutItem(destField, srcItem.IsLabel) as CellLayoutItem;

                            if (null != destItem)
                                destItem.InitializeFrom(srcItem);
                        }
                    }
                }
            } 
            #endregion //Clone Record Sizing

        }

                #endregion //CloneAssociatedRecordSettings	

				#region CreateRecordPresenter

		
		
		/// <summary>
		/// Creates a new element to represent this record in a record list control.
		/// </summary>
		/// <returns>Returns a new element to be used for representing this record in a record list control.</returns>
		internal abstract RecordPresenter CreateRecordPresenter( );

				#endregion // CreateRecordPresenter

				#region DirtyFilterState

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		internal void DirtyFilterState( )
		{
			this.DirtyFilterState( true );
		}

		// SSP 5/12/09 TFS16576
		// Added reevaluate parameter.
		// 
		internal void DirtyFilterState( bool reevaluate )
		{
			if ( FilterState.NeverFilter != _cachedFilterState && 0 == ( FilterState.NeedsToRefilter & _cachedFilterState ) )
			{
				// AS 8/20/09 TFS20920
				// Added if block - do not set the flag while the filtering is suspended.
				//
				if (0 == (FilterState.FilterSuspended & _cachedFilterState))
					_cachedFilterState |= FilterState.NeedsToRefilter;

				// SSP 5/12/09 TFS16576
				// Added reevaluate parameter.
				// 
				// --------------------------------------------------------------------------
				//this.InternalIsFilteredOutHelper( true );
				if ( reevaluate )
				{
					// If reevaluate is true then synchronously re-evaluate the filters.
					// 
					this.InternalIsFilteredOutHelper( true );
				}
				// SSP 8/25/09 TFS18934
				// Changed from else-if to if. Above call to InternalIsFilteredOutHelper could be
				// a NOOP in which case we need to dirty the scroll count.
				// 
				//else if ( 0 != ( FilterState.Hide & _cachedFilterState ) )
				if ( 0 != ( FilterState.Hide & _cachedFilterState ) && 0 != ( FilterState.NeedsToRefilter & _cachedFilterState ) )
				{
					// Otherwise notify the sparse array of potential change the visibility
					// of this record.
					// 
					this.DirtyScrollCount( );
				}
				// --------------------------------------------------------------------------
			}
		}

				#endregion // DirtyFilterState

				#region DirtyScrollCount

		internal void DirtyScrollCount()
		{
			// SSP 2/11/09 TFS12467
			// All we need to do here is notify the sparse array containing parent 
			// record that it's scroll count has changed. The sparse array then will
			// call its OnScrollCountChanged which will in turn notify its parent
			// record of change in scroll count.
			// 
			// ----------------------------------------------------------------------
			if ( null != _parentCollection )
				_parentCollection.SparseArray.NotifyItemScrollCountChanged( this );
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------------------
		}

				#endregion //DirtyScrollCount	

				#region EnsureFiltersEvaluated

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Ensures that record filters are applied to this record.
		/// </summary>
		internal virtual void EnsureFiltersEvaluated( )
		{
			// Default implementation does nothing. DataRecord and GroupByRecord override this method.
			// 
		}

				#endregion // EnsureFiltersEvaluated

				// AS 6/3/09 NA 2009.2 ClipboardSupport
				#region ExpandCollapse
		internal bool ExpandCollapse(bool expand, bool addToUndo)
		{
			// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// 
			if ( expand )
				this.LoadVirtualData( );

			if (this.IsExpanded == expand)
			{
				// JJD 10/21/11 
				// Make sure the _isExpanded member is synced up. This solves an
				// issue when setting IsExpanded to true on an ExpandableFieldRecord
				// whose IsExpandedAlways property returns true because at the
				// time of the set it was the only ExpandableFieldRecord child in the DataRecord
				// and therefore was implicitly expanded. However, at a later time when there
				// is more than one ExpandableFieldRecord and it is no longer implicitly
				// expanded it would then fall back to relying on the state of the _isExpanded
				// member.
				// This scenario can happen when setting the IsExpanded to true of
				// an ExpandableFieldRecord in the InitializeRecord event where there is more than one 
				// expandable field. The first one will not pick up the setting unless we sync
				// the member here.
				_isExpanded = expand;
				return false;
			}

			DataPresenterBase dp = this.DataPresenter;

			Debug.Assert(null != dp);

			if (dp == null)
				return false;

			return dp.OnRecordExpandStateChanged(this, expand, addToUndo);
		}
				#endregion //ExpandCollapse
    
                #region FireInitializeRecord

		internal void FireInitializeRecord( )
		{
			// JJD 11/17/11 - TFS78651 
			// Added sortValueChanged parameter
			//this.FireInitializeRecord( false );
			this.FireInitializeRecord( false, false );
		}

		// SSP 3/3/09 TFS11407
		// Added an overload that takes in reInitialize parameter.
		// 
		// JJD 11/17/11 - TFS78651 
		// Added sortValueChanged parameter
		//internal void FireInitializeRecord(bool reInitialize)
		internal void FireInitializeRecord(bool reInitialize, bool sortValueChanged)
		{
			// JJD 10/26/11 - TFS91364 
			// Ignore HeaderReords 
			if (this is HeaderRecord)
				return;

			// SSP 3/3/09 TFS11407
			// 
			//this.DataPresenter.RaiseInitializeRecord(this);
			// JJD 11/17/11 - TFS78651 
			// Added sortValueChanged parameter
			//this.DataPresenter.RaiseInitializeRecord( this, reInitialize );
			this.DataPresenter.RaiseInitializeRecord( this, reInitialize, sortValueChanged );


			// JM 6/12/09 NA 2009.2 DataValueChangedEvent
			// Initialize the DataValueChanged history for all cells in the record that are associated with Fields that have
			// DataValueChangedNotificationsActiveResolved set to true and DataValueChangedScopeResolved set to AllAllocatedRecords
			if (this is DataRecord				&& 
				reInitialize		== false	&& 
				this.FieldLayout	!= null)
			{
				List<Field> fields = this.FieldLayout.Fields.FieldsWithDataValueChangedScopeSetToAllRecords;
				foreach (Field field in fields)
				{
					((DataRecord)this).Cells[field].AddDataValueChangedHistoryEntryForCurrentValue();
				}
			}
		}

                #endregion //FireInitializeRecord	

				#region FocusIfActive

		internal void FocusIfActive()
		{
			if (this.IsActive)
			{
				RecordPresenter rp = this.AssociatedRecordPresenter;

				if ( rp != null)
					rp.FocusIfAppropriate();
			}
		}

				#endregion //FocusIfActive

                // JJD 4/1/08 - added support for printing
                #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal abstract Record GetAssociatedRecord();
        public abstract Record GetAssociatedRecord();

                #endregion //GetAssociatedRecord

                // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
                #region GetLayoutManager
        internal FieldGridBagLayoutManager GetLayoutManager(bool createIfNull)
        {
            if (createIfNull && this._layoutManager == null)
            {
                FieldLayout fl = this._fieldLayout;

                if (null != fl)
                {
                    this._layoutManager = fl.CellLayoutManager.Clone();

					// AS 10/13/09 NA 2010.1 - CardView
					// For the layout manager maintained by a VDRCP & record, we'll 
					// store a reference to the associated record so we can get back
					// to it to query if the cell should be collapsed.
					//
					_layoutManager.Record = this;
                }
            }

            if (null != this._layoutManager)
                this._layoutManager.VerifyLayout();

            return this._layoutManager;
        } 
                #endregion //GetLayoutManager

				// JJD 1/10/12 - TFS99025 - added
				#region GetCrossParentSibling

		// JJD 1/10/12 - TFS99025 - added
		// If the 'next' parameter is true will get the cross paent sibling record
		// that follows this record. Otherwise it will get the previous cross parent sibling
		internal Record GetCrossParentSibling(bool next)
		{
			// get the parent rcd
			Record parent = this.ParentRecord;

			if (parent == null)
				return null;

			// get the collection the parent belongs to as well as its index
			RecordCollectionBase ancestorCollection = parent.ParentCollection;
			int parentIndex = parent.Index;
			int step = next ? 1 : -1;

			Record parentSibling = null;

			while (true)
			{
				// adjust the index to get the sibling before or after this recds parent
				parentIndex += step;

				// JJD 07/17/12 - TFS117320
				// Hold the previous parent sibling for comparison below
				Record previousParentSibling = parentSibling;

				// make sure the index is in range
				if (parentIndex >= 0 && parentIndex < ancestorCollection.Count)
				{
					parentSibling = ancestorCollection[parentIndex];
				}
				else
				{
					// since the index wasn't in range get the parent's
					// cross parent sibling
					parentSibling = parent.GetCrossParentSibling(next);

					if (parentSibling == null)
						break;
				}

				// JJD 07/17/12 - TFS117320
				// If the parentSibling hasn't changed then break out of the wuile loop.
				// Otherwise, there are scenarios where we could end up in an infinte loop.
				if (parentSibling == previousParentSibling)
					break;

				// if the sibling has children then return its first or
				// last child based on the 'next' parameter
				if (parentSibling.HasChildren)
				{
					RecordCollectionBase parentSiblingChildren = parentSibling.ChildRecordsInternal;
					if (next)
						return parentSiblingChildren[0];
					else
						return parentSiblingChildren[parentSiblingChildren.Count - 1];
				}
			}

			return null;
		}

				#endregion //GetCrossParentSibling	
    
                // JJD 05/06/10 - TFS27757 - added
                #region GetOverallScrollPosition

        internal int GetOverallScrollPosition(bool ignoreHiddenItemState, ref bool isRecordHidden)
        {
			// MD 5/26/10 - ChildRecordsDisplayOrder feature
			// Moved all code to a new overload. Pass in False for the new getMinOfRecordAndFirstChild because we need the
			// actual scroll position of the record, regardless of its children.
			return this.GetOverallScrollPosition(ignoreHiddenItemState, false, ref isRecordHidden);
		}

		// MD 5/26/10 - ChildRecordsDisplayOrder feature
		// Added a new overload with a getMinOfRecordAndFirstChild parameter. When this is True, the method will
		// return the scroll position of the first child record insted if child are being displayed above the parent.
		internal int GetOverallScrollPosition(bool ignoreHiddenItemState, bool getMinOfRecordAndFirstChild, ref bool isRecordHidden)
		{
            if (this._parentCollection == null)
                return -1;

            // JJD 05/06/10 - TFS27757 
            // Only bail out on a hidden record if ignoreHiddenItemState is 'False'
            //if (this._visibility == Visibility.Collapsed)

            
            
            //if (ignoreHiddenItemState == false && this._visibility == Visibility.Collapsed)
            Visibility vis = this.VisibilityResolved;

            if (ignoreHiddenItemState == false && vis == Visibility.Collapsed)
                return -1;

            ViewableRecordCollection vrc = this._parentCollection.ViewableRecords;

            Debug.Assert(vrc != null);

            if (vrc == null)
                return -1;

            // JJD 3/17/10 - TFS28705 
            // If this is a special record and its parent collection
            // is either toe sorted or groups collection off its
            // RecordManager check to make sure that its parent collection
            // is the Current collection off the rm. This can happen
            // during grouping and ungrouping where the special record
            // is logically orphaned
            if (this.IsSpecialRecord)
            {
                RecordManager rm = this.RecordManager;

                if (rm == null)
                    return -1;

                if (rm.SortedInternal == this._parentCollection ||
                     rm.GroupsInternal == this._parentCollection)
                {

                    if (this._parentCollection != rm.CurrentInternal)
                        return -1;
                }
            }

            int scrollPos = 0;

            Record parent = this.ParentRecord;

            if (parent != null)
            {
                // JJD 3/17/10 - TFS28705 
                // If the parent record is not expanded or it is a special rcd
                // (which can't be expanded in the ui) return -1
                if (parent.IsExpanded == false || parent.IsSpecialRecord)
                {
                    // JJD 05/06/10 - TFS27757 
                    // If ignoreHiddenItemState is true then just set isRecordHidden to true
                    // and fall thru so we get the parent's scrollpos and return it
                    if (ignoreHiddenItemState)
                        isRecordHidden = true;
                    else
                        return -1;
                }

                // JJD 05/06/10 - TFS27757 
                // Call this method on the parent
                //scrollPos = parent.OverallScrollPosition;
				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				// Pass in True for the new getMinOfRecordAndFirstChild because we need the resolve the scroll index of which the
				// which needs to be offset to get the child record's absolute scroll index. When the child records are being displayed
				// above the parent, this starting scroll index needs to be the first child record. When the child records are being
				// displayed after the parent, this starting scroll index needs to be the parent record.
                //scrollPos = parent.GetOverallScrollPosition(ignoreHiddenItemState, ref isRecordHidden);
				scrollPos = parent.GetOverallScrollPosition(ignoreHiddenItemState, true, ref isRecordHidden);

                // JJD 05/06/10 - TFS27757 
                // If the parent is hidden then return the scrollPos as is
                if (isRecordHidden)
                    return scrollPos;

                if (scrollPos < 0)
                    return -1;

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				// We only need to increment the starting scroll index when parent occupies and scroll position and it is before the child 
				// record. If the parent is after the child records, our starting scroll index is the scroll index of the first child in the 
				// collection, and therefore we do not need to bump the value.
                //if (parent.OccupiesScrollPosition == true)
				if (parent.OccupiesScrollPosition == true && this.AreChildrenAfterParent)
                    scrollPos++;
            }

            // JJD 05/06/10 - TFS27757 
            // If this record is collapsed then set the isRecordHidden flag
            isRecordHidden = isRecordHidden || vis == Visibility.Collapsed;


            
            
            
            
            //int tmp = vrc.GetScopedScrollIndexOfRecord(this);
			// MD 8/6/10 - TFS36611
			// Now the GetScopedScrollIndexOfRecord will handle the getMinOfRecordAndFirstChild parameter, so pass it in.
            //int tmp = vrc.GetScopedScrollIndexOfRecord(this, ignoreHiddenItemState);
			int tmp = vrc.GetScopedScrollIndexOfRecord(this, ignoreHiddenItemState, getMinOfRecordAndFirstChild);
            if (tmp < 0)
                return -1;

            scrollPos += tmp;
            

			// MD 8/6/10 - TFS36611
			// Now the GetScopedScrollIndexOfRecord will handle the getMinOfRecordAndFirstChild parameter, so we don't have to 
			// do anything about it here.
			//// MD 5/26/10 - ChildRecordsDisplayOrder feature
			//// If we need to get the actual scroll position of the record, but the record is expanded and its child records are displaying 
			//// before it, we need to add in the scroll count of the child records because this record appears after them.
			//if (getMinOfRecordAndFirstChild == false && this.AreChildrenAfterParent == false && this.IsExpanded)
			//    scrollPos += this.ViewableChildRecords.ScrollCount;

            return scrollPos;
        }

                #endregion //GetOverallScrollPosition	
    
                #region GetRecordInParentChainContainedInCollection

        internal Record GetRecordInParentChainContainedInCollection(RecordCollectionBase collection)
		{
			if (collection == this._parentCollection)
				return this;

			// if we have a parent record then call this method on the parent
			// this will effectively walk up the parent chain
			Record parent = this.ParentRecord;
			if (parent != null)
				return parent.GetRecordInParentChainContainedInCollection(collection);

			return null;
		}

				#endregion //GetRecordInParentChainContainedInCollection

				#region GetRecordResizeManager

        
#region Infragistics Source Cleanup (Region)




















































































































































































#endregion // Infragistics Source Cleanup (Region)


			
#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetRecordResizeManager

				#region GetRecordResizeManager

        
#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetRecordResizeManager

				#region GetSizeManagerForDimension

        
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetSizeManagerForDimension

				#region GetVisibilityResolved

		// SSP 1/10/09 - NAS9.1 Record Filtering
		// 
		internal Visibility GetVisibilityResolved( bool verifyFiltering )
		{
			if ( this.InternalIsFilteredOutHelper( verifyFiltering ) )
			{
				// SSP 4/10/09 TFS16485 TFS16490 - Optimizations
				// Sice now we are keeping filter action setting as part of the filter state,
				// use that.
				// 
				// ----------------------------------------------------------------------------
				if ( 0 != ( FilterState.Hide & _cachedFilterState ) )
					return Visibility.Collapsed;
				




				// ----------------------------------------------------------------------------
			}

			return _visibility;
		}

				#endregion // GetVisibilityResolved

				#region InternalSetFilterState

		// SSP 4/10/09 TFS16485 TFS16490
		// 
		private void FilterStateChanged_RaiseNotificationsHelper( 
			FilterState oldFilterState, FilterState newFilterState, bool raiseVisibilityResolvedNotification )
		{
			if ( oldFilterState != newFilterState )
			{
				// filteredInOutStateChanged is set to true when IsFilteredOut property's value changes.
				bool filteredInOutStateChanged = ( FILTERED_IN_OUT_FLAGS & oldFilterState ) != ( FILTERED_IN_OUT_FLAGS & newFilterState );

				// filterActionChanged is set to true when the FilterAction setting changes.
				bool filterActionChanged = ( FILTER_ACTION_FLAGS & oldFilterState ) != ( FILTER_ACTION_FLAGS & newFilterState );

				// isFilteredOutChanged is set to true when IsFilteredOut property goes to true from a non-true
				// value or from a non-true value to true.
				bool isFilteredOutChanged = ( FilterState.FilteredOut & oldFilterState ) != ( FilterState.FilteredOut & newFilterState );

				if ( raiseVisibilityResolvedNotification
					// We also need to raise VisibilityResolved prop change notification when the filter
					// action is changed from or to to Hide even if filtered-out state remains the same.
					&& ( isFilteredOutChanged || filterActionChanged )
					&& ( 0 != ( FilterState.Hide & oldFilterState ) || 0 != ( FilterState.Hide & newFilterState ) ) )
				{
					_cachedFilterState &= ~FilterState.NeedsToRefilter;
					this.ChangeVisibilityHelper( null, newFilterState );
				}

				// Set the _cachedFilterState before raise change notifications on the properties so the
				// properies return the correct value from their getter in case the property change listener
				// accesses the value.
				// 
				_cachedFilterState = newFilterState;

				if ( filteredInOutStateChanged )
					this.RaisePropertyChangedEvent( "IsFilteredOut" );

				if ( ( isFilteredOutChanged || filterActionChanged )
					&& ( 0 != ( FilterState.Disable & oldFilterState ) || 0 != ( FilterState.Disable & newFilterState ) ) )
					this.RaisePropertyChangedEvent( "IsEnabledResolved" );

				// Send necessary add/remove collection change notifications from FilteredInDataItemCollection.
				// 
				if ( isFilteredOutChanged )
				{
					// AS 8/6/09 NA 2009.2 Field Sizing
					if (_fieldLayout != null)
						_fieldLayout.AutoSizeInfo.OnRecordVisibilityChanged(this);

					RecordManager rm = this.RecordManager;
					FilteredDataItemsCollection filteredInDataItems = null != rm ? rm.FilteredInDataItemsIfAllocated : null;
					if ( null != filteredInDataItems && this.IsDataRecord )
						filteredInDataItems.OnRecordFilteredOutStateChanged( (DataRecord)this );
				}
			}
		}

		// SSP 12/12/08 - NAS9.1 Record Filtering
		// 
		internal void InternalSetFilterState( FilterState newState, bool raiseVisibilityResolvedNotification )
		{
			// SSP 4/10/09 TFS16485 TFS16490
			// We need to raise property changed for VisibilityResolved and/or IsEnabled whenever
			// FitlerAction is changed on the field layout settings.
			// 
			if ( FilterState.NeverFilter != newState )
			{
				RecordFilterAction filterAction = _fieldLayout.FilterActionResolved;
				FilterState actionState;

				if ( RecordFilterAction.Hide == filterAction )
					actionState = FilterState.Hide;
				else if ( RecordFilterAction.Disable == filterAction )
					actionState = FilterState.Disable;
				else if ( RecordFilterAction.None == filterAction )
					actionState = FilterState.NoAction;
				else
					actionState = 0;

				newState |= actionState;

				// SSP 3/1/10 - Optimizations
				// If this is a new record and _cachedFilterState has not been initialized previously then
				// set its action flag otherwise the invalidation logic will end up raising visibility changed
				// notification even when there's no need for it.
				// 
				if ( 0 == ( _cachedFilterState & FILTER_ACTION_FLAGS ) )
					_cachedFilterState |= actionState;
			}

			Debug.Assert( FilterState.NeverFilter != _cachedFilterState, "Special records are never filtered." );
			if ( _cachedFilterState != newState && FilterState.NeverFilter != _cachedFilterState )
			{
				// SSP 4/10/09 TFS16485 TFS16490
				// We need to raise property changed for VisibilityResolved and/or IsEnabled whenever
				// FitlerAction is changed on the field layout settings.
				// Moved the logic into the new FilterStateChanged_RaiseNotificationsHelper method
				// and modified to take into account change in the value of FilterAction setting.
				// 
				// ----------------------------------------------------------------------------------
				this.FilterStateChanged_RaiseNotificationsHelper( _cachedFilterState, newState, raiseVisibilityResolvedNotification );
				
#region Infragistics Source Cleanup (Region)










































#endregion // Infragistics Source Cleanup (Region)

				// ----------------------------------------------------------------------------------
			}
		}

				#endregion // InternalSetFilterState

				#region InitializeAutomationPeer
		internal void InitializeAutomationPeer(RecordAutomationPeer peer)
		{
			// AS 1/22/08
			// This can happen with recycling.
			//
			//Debug.Assert(this._automationPeer == null, "The record already has a peer associated with it!");

			this._automationPeer = new WeakReference(peer);
		} 
				#endregion //InitializeAutomationPeer

				#region InitializeParentCollection

        internal void InitializeParentCollection(RecordCollectionBase parentCollection)
        {
            if (null == parentCollection)
                throw new ArgumentNullException("parentCollection");

			// JJD 4/1/11 - TFS70662
			// confirm that the value has changed
			if (parentCollection != _parentCollection)
			{
				this._parentCollection = parentCollection;

				// JJD 4/1/11 - TFS70662
				// Raise a notification that the ParentRecord has changed
				this.RaisePropertyChangedEvent("ParentRecord");
			}
        }

                #endregion //InitializeParentCollection	
        
                #region InternalSelect







        internal void InternalSelect(bool value)
        {
            if (this._selected != value)
            {
                this._selected = value;

                Debug.Assert(this._selected == false || this.IsSelectable, "We are selecting a record that is not selectable");

				this.RaiseAutomationIsSelectedChanged();

                this.RaisePropertyChangedEvent("IsSelected");
            }
        }

                #endregion //InternalSelect	

				#region InternalIsFilteredOutHelper

		// SSP 1/10/09 - NAS9.1 Record Filtering
		// 
		internal bool InternalIsFilteredOutHelper( bool verify )
		{
			if ( verify )
				this.EnsureFiltersEvaluated( );

			return 0 != ( FilterState.FilteredOut & _cachedFilterState );
		}
		
				#endregion // InternalIsFilteredOutHelper

				#region InternalIsFilteredOutNullableHelper

		// JJD 2/02/09 - NAS9.1 Record Filtering
		// 
		internal bool? InternalIsFilteredOutNullableHelper( bool verify )
		{
			if ( verify )
				this.EnsureFiltersEvaluated( );

            if (0 != (FilterState.FilteredOut & _cachedFilterState))
                return true;

            if (0 != (FilterState.FilteredIn & _cachedFilterState))
                return false;

            return null;
		}
		
				#endregion // InternalIsFilteredOutNullableHelper
 

                // JJD 2/9/09 - TFS13685/TFS13781 - added
                #region InvalidateExpansionIndicatorVisibility

        internal void InvalidateExpansionIndicatorVisibility()
        {
            this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");

            
            
            
			
        }

                #endregion //InvalidateExpansionIndicatorVisibility	
            
                // JJD 2/9/09 - TFS13685/TFS13781 - added
                #region InvalidateHasChildren

        internal void InvalidateHasChildren()
        {
            this.RaisePropertyChangedEvent("HasChildren");
        }

              #endregion //InvalidateHasChildren	

				// AS 6/21/11 TFS79160
				#region InvalidatePeer
		internal void InvalidatePeer()
		{
			var peer = this.AutomationPeer;

			if (null != peer)
				AutomationPeerHelper.InvalidateChildren(peer);
		}
				#endregion //InvalidatePeer
  
                // JJD /10/09 - NA 2009 Vol 2 - Record fixing
                #region IsFixedLocationAllowed

        internal bool IsFixedLocationAllowed(FixedRecordLocation location)
        {
            
            return true;
        }

                #endregion //IsFixedLocationAllowed	
    
				#region IsSiblingOf

		internal bool IsSiblingOf(Record record)
		{
			if (record == null)
				return false;

			return this._parentCollection == record._parentCollection;
		}

				#endregion //IsSiblingOf	

				#region IsTrivialAncestorOf

		// SSP 8/20/09 TFS20765
		// Added IsTrivialAncestorOf method.
		// 
		internal bool IsTrivialAncestorOf( Record record )
		{
			if ( this == record )
				return true;

			Record parentRecord = null != record ? record.ParentRecord : null;
			return null != parentRecord && this.IsTrivialAncestorOf( parentRecord );
		}
    
				#endregion // IsTrivialAncestorOf

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal void LoadVirtualData( )
		{
			RecordManager rm = this.RecordManager;

			GroupByRecord gr = this as GroupByRecord;
			if ( null != gr )
			{
				GroupInfo groupInfo = gr.CommonValueInternal as GroupInfo;
				if ( null != groupInfo )
				{
					CollectionViewGroup cvg = groupInfo.Tag as CollectionViewGroup;
					if ( null != cvg )
					{
						if ( !cvg.IsBottomLevel )
						{
							VUtils.CreateGroupByRecordsSynchronizerHelper( gr.ChildRecords, cvg.Items );
						}
						else
						{
							VUtils.CreateDataRecordsSynchronizerHelper( gr.ChildRecords, cvg.Items );
						}
					}
				}
			}
		}

                #region OnActiveCellChanged

        internal void OnActiveCellChanged()
        {
            this.RaisePropertyChangedEvent("ActiveCell");
        }

                #endregion //OnActiveCellChanged	
        
                #region OnActiveRecordChanged

        internal virtual void OnActiveRecordChanged()
        {
            this.RaisePropertyChangedEvent("IsActive");
        }

                #endregion //OnActiveRecordChanged	

                // JJD 1/26/08 - BR30085 - added
                #region OnAssociatedRecordPresenter

        internal virtual void OnAssociatedRecordPresenterChanged() { }

                #endregion //OnAssociatedRecordPresenter	
    
                #region OnCellSelectionChange

		internal void OnCellSelectionChange()
        {
			this.RaisePropertyChangedEvent("CellSelection");
		}

				#endregion //OnCellSelectionChange
        
                #region OnViewableChildRecordsChanged

        internal void OnViewableChildRecordsChanged()
        {
            this.RaisePropertyChangedEvent("ViewableChildRecords");

			// JM 01-05-10 TFS22151
			if (this._automationPeer		!= null &&
				this._automationPeer.IsAlive		&& 
				this._automationPeer.Target != null)
			{
				AutomationPeer peer = this._automationPeer.Target as AutomationPeer;
				if (peer != null)
				{
					if (peer is RecordAutomationPeer)
						((RecordAutomationPeer)peer).ClearChildRecordsPeer();

					peer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
					peer.InvalidatePeer();
					peer.ResetChildrenCache();
				}
			}
        }

                #endregion //OnViewableChildRecordsChanged	

				#region RaiseAutomationIsSelectedChanged
		internal void RaiseAutomationIsSelectedChanged()
		{
			AutomationPeer peer = this.AutomationPeer;

			if (null != peer)
				peer.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !this._selected, this._selected);
		} 
				#endregion //RaiseAutomationIsSelectedChanged

				#region RaiseAutomationSelectedChange
		internal bool RaiseAutomationSelectedChange(AutomationEvents eventId)
		{
			AutomationPeer peer = this.AutomationPeer;

			if (null != peer)
			{
				peer.RaiseAutomationEvent(eventId);
				return true;
			}

			return false;
		} 
				#endregion //RaiseAutomationSelectedChange

				// JM 02-03-10 NA 10.1 CardView
				#region RaisePropertyChangedEventInternal

		internal void RaisePropertyChangedEventInternal(string propertyName)
		{
			this.RaisePropertyChangedEvent(propertyName);
		}

				#endregion //RaisePropertyChangedEventInternal

				#region ResumeFiltering

		
		
		internal void ResumeFiltering( bool verify )
		{
			_cachedFilterState |= FilterState.NeedsToRefilter;

			// AS 8/20/09 TFS20920
			Debug.Assert((_cachedFilterState & FilterState.FilterSuspended) == FilterState.FilterSuspended);
			_cachedFilterState &= ~FilterState.FilterSuspended;

			if ( verify )
				this.InternalIsFilteredOutHelper( verify );
		}

				#endregion // ResumeFiltering

				#region SetIsExpanded

		// AS 6/3/09 NA 2009.2 Undo/Redo
		//internal void SetIsExpanded( bool isExpanded )
		internal void SetIsExpanded( bool isExpanded, List<Record> recordsChanged )
        {
            this._isExpanded = isExpanded;

			// AS 6/3/09 NA 2009.2 Undo/Redo
			if (null != recordsChanged)
				recordsChanged.Add(this);

            // JJD 4/29/08
            // Note: I left this alone to avoid making a breaking change but we probably
            // shouldn't expand all parent records by default. We don't do this in the
            // winforms grid and that allows for some additional flexibility.
            //
            // If this record is expanded than all of its ancestors must
            // be expanded as well
			if (isExpanded &&
				this.ParentRecord != null &&
				!this.ParentRecord._isExpanded)
			{
				// AS 6/3/09 NA 2009.2 Undo/Redo
				//this.ParentRecord.SetIsExpanded(isExpanded);
				this.ParentRecord.SetIsExpanded(isExpanded, recordsChanged);
			}
			// AS 8/4/09 NA 2009.2 Field Sizing
			// Register the "root" record being expanded to process auto sizing measurements.
			//
			else if (isExpanded && null != _fieldLayout)
			{
				_fieldLayout.AutoSizeInfo.OnRecordExpanded(this);
			}

			// SSP 2/11/09 TFS12467
			// DirtyScrollCount call is not necessary here since OnIsExpandedStateChanged 
			// method does it.
			// 
			//this.DirtyScrollCount();

            this.OnIsExpandedStateChanged();
        }

                #endregion //SetIsExpanded	

				// AS 10/13/09 NA 2010.1 - CardView
				#region ShouldCollapseCell
		internal virtual bool ShouldCollapseCell(Field field)
		{
			return false;
		} 
				#endregion //ShouldCollapseCell

				#region SortChildren

		internal abstract void SortChildren(); 

                #endregion //SortChildren	

				// AS 8/20/09 TFS20920
				// We need to do something similar to what the RecordManager.OnDataChanged_DirtyFiltersHelper
				// does whereby we don't set the flag on the filter but instead we 
				#region SuspendActiveRecordFiltering
		internal void SuspendActiveRecordFiltering()
		{
			if (0 != (_cachedFilterState & FilterState.NeedsToRefilter))
			{
				DataPresenterBase dp = this.DataPresenter;

				if (null != dp && dp.ActiveRecord == this)
				{
					FieldLayout fl = this.FieldLayout;

					if (null != fl && fl.ReevaluateFiltersOnDataChangeResolved)
					{
						Debug.Assert(dp._activeRecordWithPendingDirtyFilterState == this || dp._activeRecordWithPendingDirtyFilterState == null);

						_cachedFilterState ^= FilterState.NeedsToRefilter;
						dp._activeRecordWithPendingDirtyFilterState = this;
					}
				}
			}
		} 
				#endregion //SuspendActiveRecordFiltering

				#region SuspendFiltering

		
		
		internal void SuspendFiltering( )
		{
			_cachedFilterState &= ~FilterState.NeedsToRefilter;

			// AS 8/20/09 TFS20920
			Debug.Assert((_cachedFilterState & FilterState.FilterSuspended) == 0);
			_cachedFilterState |= FilterState.FilterSuspended;
		}

				#endregion // SuspendFiltering

				// JM 11/5/09 NA 2010.1 CardView
				#region ToggleContainingCardCollapsedState
		internal virtual bool ToggleContainingCardCollapsedState(bool addToUndo)
		{
			return false;
		}
				#endregion //ToggleContainingCardCollapsedState

				// JM 11/5/09 NA 2010.1 CardView
				#region ToggleContainingCardEmptyCellsCollapsedState
		internal virtual bool ToggleContainingCardEmptyCellsCollapsedState(bool addToUndo)
		{
			return false;
		}
				#endregion //ToggleContainingCardEmptyCellsCollapsedState

				#region VerifySortOrderOfChildren

		internal virtual void VerifySortOrderOfChildren() { }

                #endregion //VerifySortOrderOfChildren	
                
            #endregion //Internal Methods

            #region Protected Methods

				#region InitializeFieldLayout

		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				#endregion //InitializeFieldLayout	
            
            #endregion //Protected Methods

            #region Private Methods

                // JJD 6/30/09 - NA 2009 Vol 2 - Record fixing
                #region CheckFixedAndExpandedStates

        private void CheckFixedAndExpandedStates()
        {
            // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
            // when we expand a root record that is fixed to the top we need
            // to scroll back to the top of the list so that the
            // child records are visible when nested display is
            // enabled.
            if (this.IsExpanded && this.FixedLocation != FixedRecordLocation.Scrollable)
            {
                DataPresenterBase dp = this.DataPresenter;

                if (dp != null && dp.IsNestedDataDisplayEnabled &&
                    dp.CurrentViewInternal.SupportedDataDisplayMode != DataDisplayMode.Flat &&
                     this.HasVisibleChildren)
                {
                    IViewPanelInfo info = dp as IViewPanelInfo;

                    if (this.ParentRecord == null && this.FixedLocation == FixedRecordLocation.FixedToTop)
                    {
                        info.OverallScrollPosition = 0;
                    }
                    else
                    {
                        Record rcdToBringIntoView;

                        if (this.FixedLocation == FixedRecordLocation.FixedToTop)
                            rcdToBringIntoView = info.GetRecordAtOverallScrollPosition(Math.Min(info.OverallScrollCount - 1, this.OverallScrollPosition + 1));
                        else
                            rcdToBringIntoView = this;

                        dp.BringRecordIntoView(rcdToBringIntoView);
                    }
                }
            }
        }

                #endregion //CheckFixedAndExpandedStates	
    
                // JJD 4/28/08 - BR31406 and BR31707 - added
                #region GetExpansionIndicatorVisibilityBasedOnContent

        private Visibility GetExpansionIndicatorVisibilityBasedOnContent()
        {
            ExpandableFieldRecord efr = this as ExpandableFieldRecord;

            if (efr != null)
            {
                if (efr.CheckHasChildData())
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }

			// SSP 4/30/08 - Summaries Feature
			// Check for null.
			// 
			//int childCount = this.ViewableChildRecords.Count;
			// JJD 09/22/11  - TFS84708 - Optimization
			// Use ViewableChildRecordsIfNeeded instead
			//ViewableRecordCollection viewableChildRecords = this.ViewableChildRecords;
			ViewableRecordCollection viewableChildRecords = this.ViewableChildRecordsIfNeeded;
            int childCount = null != viewableChildRecords ? viewableChildRecords.Count : 0;

            if (childCount == 0)
                return Visibility.Hidden;

            // If there is only one child record and it is an ExpandableFieldRecord
            // then check to make sure it has child data
            if (childCount == 1)
            {
				ExpandableFieldRecord childEfr = viewableChildRecords[0] as ExpandableFieldRecord;

                if (childEfr != null && childEfr.CheckHasChildData() == false)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }

            return Visibility.Visible;
        }

                #endregion //GetExpansionIndicatorVisibilityBasedOnContent	
        
                #region OnIsExpandedStateChanged






        private void OnIsExpandedStateChanged()
        {
			// SSP 2/12/09 TFS12467
			// Moved this here from below.
			// 
			this.DirtyScrollCount( );

            if (this.IsExpanded)
                this.VerifySortOrderOfChildren();

            // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
            // The expanded state affects the resolved IsFixed state of trailing
            // fixed records so we need to dirty the special records of the
            // vrc if the fixedlocation is not scrollable
            if (this.FixedLocation != FixedRecordLocation.Scrollable)
            {
                ViewableRecordCollection vrc = this.ParentCollection.ViewableRecordsIfAllocated;

                if (vrc != null)
                    vrc.DirtySpecialRecords(false);
            }

			AutomationPeer peer = this.AutomationPeer;

			if (null != peer)
			{
				peer.InvalidatePeer();

				peer.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
					!this.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
					this.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
			}

			// SSP 2/11/09 TFS12467
			// Commented this out and instead added DirtyScrollCount call above, which 
			// essentially does the same thing.
			// 
			//this.ParentCollection.SparseArray.NotifyItemScrollCountChanged(this);

            this.RaisePropertyChangedEvent("IsExpanded");

            // JJD 6/30/09 - NA 2009 Vol 2 - Record fixing
            this.CheckFixedAndExpandedStates();
        }

                #endregion //OnIsExpandedStateChanged	
            
				// JM 6-28-10 TFS33366 - Added.
				#region OnParentCollectionVersionChanged

		private void OnParentCollectionVersionChanged()
		{
			this.RaisePropertyChangedEvent("Index");
			this.RaisePropertyChangedEvent("VisibleIndex");
		}

				#endregion //OnParentCollectionVersionChanged

			#endregion //Private Methods

		#endregion //Methods

		#region ISparseArrayMultiItem Members

		object ISparseArrayMultiItem.GetItemAtScrollIndex(int scrollIndex)
        {
			bool occupiesScrollPosition = this.OccupiesScrollPosition;

			// MD 5/26/10 - ChildRecordsDisplayOrder feature
			// Cache the value of AreChildrenAfterParent so we don't have to get it multiple times.
			bool areChildrenAfterParent = this.AreChildrenAfterParent;

			// MD 5/26/10 - ChildRecordsDisplayOrder feature
			// When the index is zero, we only want to return this record when its child records are displayed after it.
            //if (0 == scrollIndex && occupiesScrollPosition)
			if (0 == scrollIndex && occupiesScrollPosition && areChildrenAfterParent)
                return this;
            else
            {
				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				// Only decrement the scroll index when this record is displayed before its child records. Otherwise, the
				// children come first and the scroll index first points to them.
				//if (occupiesScrollPosition == true)
				if (occupiesScrollPosition && areChildrenAfterParent)
					scrollIndex--;

				if (this.IsExpanded)
				{
					// MD 5/26/10 - ChildRecordsDisplayOrder feature
					// This assert may not be valid when children are displayed before the parent. We wouldn't have returned 
					// this record above even if the scrollIndex were zero and it was expanded with no children. We will return 
					// it below.
					//Debug.Assert(this.HasChildrenInternal);
					Debug.Assert(this.HasChildrenInternal || areChildrenAfterParent == false);

					if (this.HasChildrenInternal)
					{
						ViewableRecordCollection children = this.ViewableChildRecords;

						Debug.Assert(children != null, "ViewableChildRecords should not be null");

						int childrenScrollCount = children.ScrollCount;

						if (children != null && scrollIndex < childrenScrollCount)
							return children.GetRecordAtScrollPosition(scrollIndex);

						// adjust the scroll index
						scrollIndex -= childrenScrollCount;

					}
				}

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				// If the srollIndex is now zero and this record is displayed after its children, we can now return it.
				if (scrollIndex == 0 && occupiesScrollPosition && areChildrenAfterParent == false)
					return this;

                return null;
            }
        }

        int ISparseArrayMultiItem.ScrollCount
        {
            get { return this.ScrollCountInternal; }
        }

        #endregion

        #region ISparseArrayItem Members

        object ISparseArrayItem.GetOwnerData(SparseArray context)
        {
			throw new NotSupportedException(DataPresenterBase.GetString("LE_NotSupportedException_6"));
        }

        void ISparseArrayItem.SetOwnerData(object ownerData, SparseArray context)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_7" ) );
        }

        #endregion

        #region ISelectableItem Members

        bool ISelectableItem.IsDraggable
        {
            get { return false; }
        }

        bool ISelectableItem.IsSelectable
        {
            get { return this.IsSelectable; }
        }

        bool ISelectableItem.IsSelected
        {
            get { return this.IsSelected; }
        }

        bool ISelectableItem.IsTabStop
        {
            get { return this.IsEnabledResolved; }
        }

        #endregion
	}

    #endregion //Record

    #region DataRecord

    /// <summary>
    /// Used by a XamDataGrid, XamDataCarousel or XamDataPresenter to represent an item in the DataSource. 
    /// </summary>
	/// <remarks>
	/// <para class="body">This object wraps an item from a <see cref="DataPresenterBase.DataSource"/>. The underlying data item is 
	/// exposed via the read-only <see cref="DataItem"/> property and its associated index in the DataSource is returned via the <see cref="DataItemIndex"/> property.</para>
	/// <para class="note"><b>Note: </b>This is not a UIElement but is a lightweight wrapper around the data item. It is represented in the UI via a corresponding <see cref="DataRecordPresenter"/> element.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Records"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
    /// <seealso cref="RecordManager"/>
    /// <seealso cref="DataRecordCollection"/>
    /// <seealso cref="DataRecordPresenter"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataPresenter"/>
	// JJD 5/9/07 - BR22698
	// Added support for listening to data item's PropertyChanged event
	// if the datasource doesn't supply cell value change notifications thru IBindlingList
	//public class DataRecord : Record
	public class DataRecord : Record, IWeakEventListener
    {
        #region Private Members

		private FieldLayout.LayoutPropertyDescriptorProvider _propertyDescriptorProvider;
        private ExpandableFieldRecordCollection _expandableFieldRecords;
        private CellCollection _cells;
        private object _dataItem;
        private object _dataItemForComparison;
		private int _underlyingDataVersion;
		private int _dataItemIndex;
		private bool _isAddRecord;
		private bool _isDataChanged;
		private bool _isDataChangedSinceLastCommitAttempt;
		private bool _isDeleted;
		// SSP 3/23/11 TFS69903
		// 
		//private bool _isCommittingChanges;
		private bool _beginEditCalled;		
		
		// JJD 5/9/07 - BR22698
		// Added support for listening to data item's PropertyChanged event
		// if the datasource doesn't supply cell value change notifications thru IBindlingList
		private bool _isHookedToDataItemPropertyChangedEvent;

        // JJD 3/7/08 
        // cache the field during a cell value set so we know to
        // ignore value change notifications for that cell
        private Field _ignoreChangeNotificationsForField;

        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping
        // The DataRecord hold a reference to the _sortGroupVersionTracker that
        // was passed into its constructor. This is because the RecordManager
        // maintains a weak reference dictonary. Our reference root the sortGroupVersionTracker
        // until all records with the same fieldlayout have been removed.
        // At that point the WeakDictionary will allow the sortGroupVersionTracker to be
        // cleaned up automatically
        private RecordManager.SortGroupVersionTracker _sortGroupVersionTracker;
		
		// JJD 09/22/11  - TFS84708 - Optimization
		ExpandableFieldRecord.ChildDataTrackingInfo[] _trackingInfos;

		#endregion //Private Members

        #region Constructors

        // JJD 10/02/08 added index param to support printing so we can default to
        // the associated record's FieldLayout  
		//internal static DataRecord Create(RecordCollectionBase parentCollection, object listObject, bool isAddRecord)
		internal static DataRecord Create(RecordCollectionBase parentCollection, object listObject, bool isAddRecord, int listIndex)
		{
			RecordManager rm = parentCollection.ParentRecordManager;

			Debug.Assert(rm != null);

			IEnumerable dataSource = rm.DataSource;
			DataPresenterBase dp = parentCollection.DataPresenter;

			Debug.Assert(dp != null);
			Debug.Assert(dataSource != null);

            // JJD 10/17/08
            // We need to return if we don't have a datapresenter or data source
            if (dp == null ||
                dataSource == null)
                return null;

			// JJD 7/9/07
			// Added support for handling change notifications on another thread 
			// by making sure we are on the DP's thread 
			dp.VerifyAccess();

			// JJD 8/1/07 - Optimization
			// Added support for lazing searching for field layout
			#region Old code

			//FieldLayout	fl = dp.FieldLayouts.GetDefaultLayoutForItem(listObject, dataSource);

			//// JJD 2/22/07 - BR20439
			//// for add records use the collection's fl if necessary
			//if (fl == null && isAddRecord)
			//    fl = parentCollection.FieldLayout;

			#endregion //Old code
			
			FieldLayout fl = null;
			FieldLayoutCollection fieldLayouts = dp.FieldLayouts;

            // JJD 12/13/07
            // If a root data item is an IEnumerable then wrap it in an object that
            // exposes 2 properties, the list name and the items 
            bool createItemsWrapper = false;

            if (isAddRecord == false &&
                 parentCollection.ParentDataRecord == null &&
                 listObject is IEnumerable &&
                 !(listObject is string) &&
				 // AS 12/17/07
				 // Also exclude xmlnode
				 //
				 false == listObject is System.Xml.XmlNode)
            {
                createItemsWrapper = true;
            }

            if (createItemsWrapper)
                listObject = new EnumerableObjectWrapper(listObject as IEnumerable);

            // JJD 2/6/09 - TFS13615
            // If we don't have a list object and this is the add record and we have at least 
            // one data record and we can't get properties from the datasource if its empty 
            // then use the DataItem from that data record to use when we search for the appropriate 
            // property descriptor provider
            object objectToUseForPropDescriptor = listObject;

            if (objectToUseForPropDescriptor == null &&
                isAddRecord == true &&
                rm != null &&
                rm.CanGetPropertiesFromDataSourceWithoutItems == false &&
                rm.Unsorted.Count > 0)
            {
                objectToUseForPropDescriptor = rm.Unsorted[0].DataItem;
            }       
          

			// JJD 8/1/07 - Optimization
			// Get the provider

            // JJD 2/6/09 - TFS13615
            // Use the objectToUseForPropDescriptor stack variable initialized above
			//PropertyDescriptorProvider propertyDescProvider = fieldLayouts.GetPropertyDescriptorProvider(listObject, dataSource);
			PropertyDescriptorProvider propertyDescProvider = fieldLayouts.GetPropertyDescriptorProvider(objectToUseForPropDescriptor, dataSource);

			Debug.Assert(propertyDescProvider != null);

			if (propertyDescProvider == null)
				return null;
			
			// JJD 8/1/07 - Optimization
			// Only pre-get the field layout if there aren't any in the collection.
			// This will ensure that the DefaultFieldLayout property of DataPresenterBase
			// won't return null inside the AssigningFieldLayoutToItem event which would
			// possibly cause a regression issue.
			if (fieldLayouts.Count == 0)
			{
				// JJD 8/2/07 - Optimization
				// Added overload that takes a PropertyDescriptorProvider
				//fl = fieldLayouts.GetDefaultLayoutForItem(listObject, dataSource);
                // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                // Added parentCollection parameter
                //fl = fieldLayouts.GetDefaultLayoutForItem(listObject, dataSource, propertyDescProvider);
                fl = fieldLayouts.GetDefaultLayoutForItem(listObject, dataSource, parentCollection, propertyDescProvider);

				// JJD 2/22/07 - BR20439
				// for add records use the collection's fl if necessary
				if (fl == null && isAddRecord)
					fl = parentCollection.FieldLayout;
			}

			// JJD 8/2/07 - Optimization
			// Added overload that takes parameters to allow lazy creation of FieldLayout
			//AssigningFieldLayoutToItemEventArgs args = new AssigningFieldLayoutToItemEventArgs(listObject, dataSource, fl, isAddRecord);
            // JJD 10/02/08 added index param to support printing so we can default to
            // the associated record's FieldLayout  
            //AssigningFieldLayoutToItemEventArgs args = new AssigningFieldLayoutToItemEventArgs(listObject, dataSource, fl, isAddRecord, dp, parentCollection, propertyDescProvider);
			AssigningFieldLayoutToItemEventArgs args = new AssigningFieldLayoutToItemEventArgs(listObject, dataSource, fl, isAddRecord, dp, parentCollection, propertyDescProvider, listIndex);

			// raise the AssigningFieldLayoutToItem event
			dp.RaiseAssigningFieldLayoutToItem(args);

			// JJD 8/1/07 - Optimization
			// Moved this code below until after we get the provider. If the provoder is already acached on
			// the field layout we can eliminate the call to IsListObjectCompatible completely
			#region Moved below
    
    			// if they changed the FieldLayout validate that it is a compatible layout
			// JJD 7/18/07 - BR24617
			// Pass true in as the 3rd param so we allow a partial match since it was set explicitly in the event
			//if (args.FieldLayout != fl &&
			//    !args.FieldLayout.IsListObjectCompatible(listObject, dataSource))

   			#endregion //Moved below	
    
			fl = args.FieldLayout;

			// JJD 2/22/07 - BR20439
			// return if still null at this point
			if (fl == null)
				return null;

			if (!fl.AreFieldsInitialized)
			{
				// JJD 8/1/07 - Optimization
				// Added overload that passes in the propertyDescriptorProvider
				//fl.InitializeFields(listObject, dataSource);

                // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                // Added parentRecordCollection parameter
                //fl.InitializeFields(listObject, dataSource, propertyDescProvider);
                fl.InitializeFields(listObject, dataSource, parentCollection, propertyDescProvider);
			}

			// JJD 8/1/07 - Optimization
			// Added overload that passes in the propertyDescriptorProvider
			//FieldLayout.LayoutPropertyDescriptorProvider layoutProvider = fl.GetProvider(listObject, dataSource);
			FieldLayout.LayoutPropertyDescriptorProvider layoutProvider = fl.GetProvider(propertyDescProvider);

			// JJD 2/22/07 - BR20439
			//if we don't have a provider for add records use the first sibling record's provider
			if (layoutProvider == null && isAddRecord &&
				 parentCollection.SparseArray.Count > 0)
			{
				DataRecord firstSibling = parentCollection.SparseArray.GetItem(0, true) as DataRecord;

				Debug.Assert(firstSibling != null);

				if (firstSibling != null)
					layoutProvider = firstSibling._propertyDescriptorProvider;
			}

			Debug.Assert(layoutProvider != null);

			// JJD 2/22/07 - BR20439
			// return null if we don't have a provider
			if (layoutProvider == null)
				return null;

			// if they changed the FieldLayout validate that it is a compatible layout

			// JJD 7/18/07 - BR24617
			// Pass true in as the 3rd param so we allow a partial match since it was set explicitly in the event

			// JJD 8/1/07 - Optimization
			// Moved from above. Now that we have the provider we can eliminate the call to IsListObjectCompatible if
			// the provider is already cached on the field layout
			// JJD 8/2/07 - Optimization
			// Added PropertyDescriptorProvider param so we don't have to re-get it 
			//if (args.FieldLayout != fl &&
			//!args.FieldLayout.IsListObjectCompatible(listObject, dataSource))
			if (args.IsFieldLayoutExplicitlySet &&
				!fl.IsProviderCached(propertyDescProvider)	&&
				!args.FieldLayout.IsListObjectCompatible(listObject, dataSource, true, propertyDescProvider))
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_27" ), "FieldLayout" );

            // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
            // Let the fieldlayout know that a record has been created
            fl.OnDataRecordLoaded();

            // JJD 5/18/09
            // If the provider is a XmlNodePropertyDescriptorProvider then merge in any additional properties
            // that we find on the XmlNode
            XmlNodePropertyDescriptorProvider xmlProvider = layoutProvider.Provider as XmlNodePropertyDescriptorProvider;
            if (xmlProvider != null)
            {
                XmlNode node = listObject as XmlNode;

                Debug.Assert(node != null, "If the provider is an XmlNodePropertyDescriptorProvider then the listObject must be an XmlNode");

                if (node != null)
                    xmlProvider.MergePropertiesFromNode(node);
            }

            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
            // make sure we track sort/groupby version number changes
            // for this fieldlayout
            RecordManager.SortGroupVersionTracker sortGroupVersionTracker = rm.TrackSortGroupVersionNumberChanges(fl);

            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
            // Pass the tracker return above into the DataRecord's constructor so that
            // the DataRecord can keep a reference to it, which will keep the tracker alive
            // as long as there is at least one record, managed by the record manager,
            // with the same fieldlayout.
			DataRecord newRcd = new DataRecord(fl, layoutProvider, parentCollection, listObject, isAddRecord, sortGroupVersionTracker);

            if (isAddRecord)
                dp.RaiseInitializeTemplateAddRecord(newRcd);
            
			return newRcd;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// SSP 12/17/08 - NAS9.1 Record Filtering
		// Changed the method to take in out param for returning the data record instead of returning it.
		// We need to initialize FieldLayout's _templateDataRecord member variable before call to 
		// templateDataRecords.AddRecord in this method to prevent recursion.
		// 
		//internal static DataRecord CreateTemplateDataRecord(FieldLayout fieldLayout)
		internal static void CreateTemplateDataRecord( FieldLayout fieldLayout, out DataRecord createdTemplateRecord )
		{
			Debug.Assert(fieldLayout != null, "Expected a valid field layout!");
			Debug.Assert(fieldLayout == null || fieldLayout.DataPresenter != null, "Expected the field layout to have a ptr to the DataPresenter!");

			RecordCollectionBase templateDataRecords = fieldLayout.DataPresenter.FieldLayouts.TemplateDataRecords;
			// SSP 12/17/08 - NAS9.1 Record Filtering
			// Also dummy template records should never get filtered out so set the filter state to
			// NeverFilter.
			// ------------------------------------------------------------------------------------------
			// AS 6/24/09 NA 2009.2 Field Sizing
			//createdTemplateRecord = new DataRecord(fieldLayout, null, templateDataRecords, null, false, null);
			createdTemplateRecord = new TemplateDataRecord(fieldLayout, templateDataRecords);

			createdTemplateRecord._cachedFilterState = FilterState.NeverFilter;
			templateDataRecords.AddRecord( createdTemplateRecord );
			




			// ------------------------------------------------------------------------------------------
		}

        private DataRecord(FieldLayout layout, FieldLayout.LayoutPropertyDescriptorProvider provider, RecordCollectionBase parentCollection, object listObject, bool isAddRecord, 
            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added sortGroupVersionTracker param (see note below)
            RecordManager.SortGroupVersionTracker sortGroupVersionTracker)
            : base(layout, parentCollection)
        {
			
			
			
			
			

			this._underlyingDataVersion = this.ParentCollection.ParentRecordManager.UnderlyingDataVersion;
			this._propertyDescriptorProvider = provider;
			this._isAddRecord = isAddRecord;

            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping
            // The DataRecord holds a reference to the _sortGroupVersionTracker that
            // was passed into its constructor. This is because the RecordManager
            // maintains a weak reference dictonary. Our reference root the sortGroupVersionTracker
            // until all records with the same fieldlayout have been removed.
            // At that point the WeakDictionary will allow the sortGroupVersionTracker to be
            // cleaned up automatically
            this._sortGroupVersionTracker = sortGroupVersionTracker;

			
			
			
			this.SetDataItemHelper(listObject);
        }

		// SSP 12/10/08 - NAS9.1 Record Filtering
		// Since FilterRecord derives from DataRecord, needed an overload accessible from the derived class.
		// 
		internal DataRecord( FieldLayout layout, RecordCollectionBase parentCollection )
			: this( layout, null, parentCollection, null, false, null )
		{
		}

        #endregion //Constructors

        #region Base class overrides

			#region CanDelete

		internal override bool CanDelete
		{
			get
			{
				if (this.IsAddRecord)
					return this._dataItem != null;

				FieldLayout fl = this.FieldLayout;

				if (fl == null)
					return false;

				RecordManager rm = null;

				if (this.ParentCollection != null)
					rm = this.ParentCollection.ParentRecordManager;

				if (rm == null)
					return false;

				return fl.AllowDeleteResolved && rm.DataSourceAllowsDelete;
			}
		}

			#endregion //CanDelete	

			// JJD 09/22/11  - TFS84708 - Optimization
			#region ChildRecordsIfNeeded

		internal override RecordCollectionBase ChildRecordsIfNeeded
		{
			get
			{
				return this.GetChildRcdCollection(false);
			}
		}

			#endregion //ChildRecordsIfNeeded

            #region ChildRecordsInternal

        internal override RecordCollectionBase ChildRecordsInternal { get { return this.ChildRecords; } }

            #endregion //ChildRecordsInternal	
        
            // JJD 4/3/08 - added support for printing
            #region CloneAssociatedRecordSettings


        // MBS 7/28/09 - NA9.2 Excel Exporting
        //internal override void CloneAssociatedRecordSettings(Record associatedRecord, ReportViewBase reportView)
        internal override void CloneAssociatedRecordSettings(Record associatedRecord, IExportOptions options)
        {
            base.CloneAssociatedRecordSettings(associatedRecord, options);

            DataRecord associatedDr = associatedRecord as DataRecord;

            Debug.Assert(associatedDr != null);

            if (associatedDr != null && associatedDr._cells != null)
                this.Cells.CloneAssociatedCellSettings(associatedDr.Cells);
 

        }


            #endregion //CloneAssociatedRecordSettings	

			#region CreateRecordPresenter

		
		
		/// <summary>
		/// Creates a new element to represent this record in a record list control.
		/// </summary>
		/// <returns>Returns a new element to be used for representing this record in a record list control.</returns>
		internal override RecordPresenter CreateRecordPresenter( )
		{
			return new DataRecordPresenter( );
		}

			#endregion // CreateRecordPresenter

			#region EnsureFiltersEvaluated

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Ensures that record filters are applied to this record.
		/// </summary>
		internal override void EnsureFiltersEvaluated( )
		{
			if ( FilterState.NeverFilter != _cachedFilterState )
			{
				RecordCollectionBase parentColl = this.ParentCollection;
				if ( null != parentColl )
				{
					ViewableRecordCollection vrc = parentColl.ViewableRecords;
					if ( null != vrc && vrc.EnsureFiltersEvaluated( ) )
					{
						if ( 0 != ( FilterState.NeedsToRefilter & _cachedFilterState ) )
						{
							this.ApplyFiltersHelper( );
						}
					}
				}
			}
		}

			#endregion // EnsureFiltersEvaluated

            // JJD 4/1/08 - added support for printing
            #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
            return this.ParentCollection.ParentRecordManager.GetAssociatedRecordInternal(this);
        }

            #endregion //GetAssociatedRecord

			#region HasChildren

        /// <summary>
        /// Indicates if this record has any child records (read-only)
        /// </summary>
		/// <value>True if this record has any child records</value>
		/// <remarks>
		/// <p class="note"><b>Note:</b>This property will return false if its direct child <see cref="ExpandableFieldRecord"/>s have no child <see cref="DataRecord"/>s.</p>
		/// </remarks>
        public override bool HasChildren 
        { 
            get
            {
                if ( this.HasExpandableFieldRecords == false)
					return false;

				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ChildRecordsIfNeeded instead
				//ExpandableFieldRecordCollection children = this.ChildRecords;
				ExpandableFieldRecordCollection children = this.ChildRecordsIfNeeded as ExpandableFieldRecordCollection;

				if (children == null)
					return false;

				// JJD 3/01/07 - BR20214
				// only return true if one of our children returns true
				for (int i = 0; i < children.Count; i++)
				{
					// JJD 8/8/07 - BR25572
					// Always consider a child ExpandableFieldRecord as a child if its Field
					// is not expandable by default.
					//if (children[i].HasChildrenInternal)
					ExpandableFieldRecord child = children[i];

					if (child.Field.IsExpandableByDefault == false ||
						child.HasChildrenInternal)
						return true;
				}

				return false;
            } 
        }

			#endregion //HasChildren

            #region HasChildrenInternal

			// JJD 3/01/ 07 - BR17658
			// The new HasChildrenInternal will return true if there are any ExpandableFieldRecords





		internal override bool HasChildrenInternal 
        { 
            get
            {
                return this.HasExpandableFieldRecords;
            } 
        }

			#endregion //HasChildrenInternal

			#region HasChildData

			// JJD 3/01/ 07 - BR17658
			// Made internal since they can get the information thru the ViewableChildRecords collection





			internal override bool HasChildData
		{
			get
			{
				if (this.IsExpanded)
					return base.HasChildData;

				return this.HasExpandableFieldRecords;
			}
		}

			#endregion //HasChildData

			#region IsDataRecord

			// SSP 12/11/08 - NAS9.1 Record Filtering
			// 
			/// <summary>
			/// Indicates if this is a data record with a data item from the data source.
			/// </summary>
			// JJD 10/26/11 - TFS91364 - Make property public
			//internal override bool IsDataRecord
			public override bool IsDataRecord
			{
				get
				{
					// DataRecord objects are data records except for the template add-record.
					// Other record classes that derive from DataRecord, like FilterRecord, must
					// override this method and return false.
					// 
					return ! this.IsAddRecordTemplate;
				}
			}

			#endregion // IsDataRecord

            #region IsExpanded

		
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


                #endregion //IsExpanded	

			// AS 5/19/09 TFS17455
			#region IsIndexValid
		internal override bool IsIndexValid
		{
			get
			{
				if (this.IsAddRecordTemplate)
					return true;

				return base.IsIndexValid;
			}
		}
			#endregion //IsIndexValid

			#region IsOnTopWhenFixed

		internal override bool IsOnTopWhenFixed
		{
			get
			{
				if (this.IsAddRecord)
					return this.ParentCollection.ViewableRecords.IsAddRecordOnTop;

				return base.IsOnTopWhenFixed;
			}
		}

			#endregion //IsOnTopWhenFixed

			#region IsSelectable

		/// <summary>
		/// Property: Returns true only if the record can be selected
		/// </summary>
		internal protected override bool IsSelectable
		{
			get
			{
				if (this.IsAddRecordTemplate)
					return false;

				return base.IsSelectable;
			}
		}

			#endregion // IsSelectable

			#region IsSpecialRecord

		// SSP 8/5/09 - NAS9.2 Enhanced grid view
		// Made public.
		// 
		//internal override bool IsSpecialRecord
		/// <summary>
		/// Overridden. Indicates if this record is a special record (filter record, summary record, template add-record etc...).
		/// </summary>
		public override bool IsSpecialRecord
		{
			get
			{
				// SSP 3/23/11 TFS69903
				// _isCommittingChanges causes an issue where if the updating of a record is cancelled,
				// for example due to the EndEdit call throwing an exception, then the record will remain
				// an add-record. In that case, checking _isCommittingChanges here will cause the record
				// to be not considered special record temporarily. If we do verification of special 
				// records in the mean time, that may cause the special records structures on the 
				// viewable record collection to erronously not contain this record when it should.
				// When we reset the _isCommittingChanges flag back to false in the CommitChanges call,
				// we don't dirty special records so the structures will remain like that.
				// 
				//return this._isAddRecord && this._isCommittingChanges == false;
				return this._isAddRecord;
			}
		}

			#endregion // IsSpecialRecord
        
			// AS 5/7/09 TFS17455
			#region IsStillValid
		internal override bool IsStillValid
		{
			get
			{
				if (this._isDeleted)
					return false;

				return base.IsStillValid;
			}
		} 
			#endregion //IsStillValid

            // JJD 1/26/08 - BR30085 - added
            #region OnAssociatedRecordPresenter

        internal override void OnAssociatedRecordPresenterChanged()
        {
			// SSP 2/25/09 TFS14608
			// Commented out the following code because the we need to hook into the record
			// regardless of whether it's in view or not because we still need to recalculate
			// any summaries as well as re-evaluate any filters on the record.
			// 
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }

            #endregion //OnAssociatedRecordPresenter	
    
            #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RecordType RecordType
        {
            get
            {
                return RecordType.DataRecord;
            }
        }

            #endregion //RecordType

			// AS 10/13/09 NA 2010.1 - CardView
			#region ShouldCollapseCell
		internal override bool ShouldCollapseCell(Field field)
		{
			if (field == null || field.Index < 0)
				return false;

			
			if (this.IsAddRecord)
				return false;

			FieldLayout fl = field.Owner;

			if (fl == null || !fl.IsEmptyCellCollapsingSupportedByView)
				return false;

			if (this.ShouldCollapseEmptyCellsResolved == false)
				return false;

			if (field.CollapseWhenEmptyResolved == false)
				return false;

			bool isEmpty = false;

			object dataValue = this.GetCellValue(field, CellValueType.EditAsType);

			if (dataValue == null || dataValue is DBNull)
				isEmpty = true;
			else
			{
				Type dataValueType = dataValue.GetType();
				Type underlyingType = Nullable.GetUnderlyingType(dataValueType) ?? dataValueType;

				if (dataValueType != underlyingType)
				{
					// if the value is a nullable but not actually null we should not consider it empty
				}
				else if (underlyingType == typeof(double) || underlyingType == typeof(float))
				{
					double dbl = (double)Utilities.ConvertDataValue(dataValue, typeof(double), null, null);

					isEmpty = dbl == 0 || double.IsNaN(dbl);
				}
				else if (Utilities.IsNumericType(underlyingType))
				{
					isEmpty = object.Equals(0m, Utilities.ConvertDataValue(dataValue, typeof(Decimal), null, null));
				}
				else
				{
					string strDataValue = dataValue as string;

					
					if (strDataValue == null)
						strDataValue = dataValue.ToString();

					if (strDataValue != null)
						isEmpty = strDataValue.Trim().Length == 0;
				}
			}

			return isEmpty;
		} 
			#endregion //ShouldCollapseCell

			// JM 10/20/09 NA 2010.1 - CardView
			#region ShouldCollapseEmptyCellsResolved
		/// <summary>
		/// Returns whether <see cref="Cell"/>s contained in this <see cref="Record"/> should be collapsed if they contain empty values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>The value returned by this property resolves (i.e., combines) the values of <see cref="CardViewSettings.ShouldCollapseEmptyCells"/> and the nullable <see cref="Record.ShouldCollapseEmptyCells"/>.  
		/// This property only has meaning in <see cref="CardView"/>.</para>
		/// </remarks>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseEmptyCells"/>
		/// <seealso cref="Record.ShouldCollapseEmptyCells"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public override bool ShouldCollapseEmptyCellsResolved
		{
			get 
			{
				FieldLayout fl = this.FieldLayout;

				if (fl == null)
					return false;

				if (fl.IsEmptyCellCollapsingSupportedByView == false)
					return false;

				if (this.IsSpecialRecord)
					return false;

				bool shouldCollapseEmptyCellsResolved = false;
				// AS 12/2/10 TFS61073
				// Reorganized this since its not necessary to check the view if we have a value on the record.
				//
				//if (this.FieldLayout.DataPresenter != null && this.FieldLayout.DataPresenter.CurrentViewInternal != null)
				//    shouldCollapseEmptyCellsResolved = this.FieldLayout.DataPresenter.CurrentViewInternal.DefaultShouldCollapseEmptyCells;
				//
				//if (this.ShouldCollapseEmptyCells.HasValue)
				//    shouldCollapseEmptyCellsResolved = this.ShouldCollapseEmptyCells.Value;
				if (this.ShouldCollapseEmptyCells.HasValue)
					shouldCollapseEmptyCellsResolved = this.ShouldCollapseEmptyCells.Value;
				else if (fl.DataPresenter != null)
				{
					ViewBase view = fl.DataPresenter.CurrentViewInternal;

					if (view != null)
						shouldCollapseEmptyCellsResolved = view.DefaultShouldCollapseEmptyCells;
				}

				return shouldCollapseEmptyCellsResolved;
			}
		}
			#endregion //ShouldCollapseEmptyCells

            #region SortChildren

        internal override void SortChildren()
        {
            
        }
            #endregion //SortChildren	

			// JM 11/5/09 NA 2010.1 CardView
			#region ToggleContainingCardCollapsedState
		internal override bool ToggleContainingCardCollapsedState(bool addToUndo)
		{
			DataPresenterBase dp = this.DataPresenter;
			Debug.Assert(null != dp);

			if (dp == null)
				return false;

			if (this.IsContainingCardCollapsed != null)
				// JM 12-9-10 TFS 61460
				//this.IsContainingCardCollapsed = null;
				this.IsContainingCardCollapsed = !this.IsContainingCardCollapsed;
			else
			{
				if (this.FieldLayout									!= null &&
					this.FieldLayout.DataPresenter						!= null && 
					this.FieldLayout.DataPresenter.CurrentViewInternal	!= null)
					this.IsContainingCardCollapsed = !this.FieldLayout.DataPresenter.CurrentViewInternal.DefaultShouldCollapseCards;
				else
					this.IsContainingCardCollapsed = null;
			}

			

			return true;
		}
			#endregion //ToggleContainingCardCollapsedState

			// JM 11/5/09 NA 2010.1 CardView
			#region ToggleContainingCardEmptyCellsCollapsedState
		internal override bool ToggleContainingCardEmptyCellsCollapsedState(bool addToUndo)
		{
			DataPresenterBase dp = this.DataPresenter;
			Debug.Assert(null != dp);

			if (dp == null)
				return false;

			if (this.ShouldCollapseEmptyCells != null)
				this.ShouldCollapseEmptyCells = null;
			else
			{
				if (this.FieldLayout									!= null &&
					this.FieldLayout.DataPresenter						!= null &&
					this.FieldLayout.DataPresenter.CurrentViewInternal	!= null)
					this.ShouldCollapseEmptyCells = !this.FieldLayout.DataPresenter.CurrentViewInternal.DefaultShouldCollapseEmptyCells;
				else
					this.ShouldCollapseEmptyCells = null;
			}

			

			return true;
		}
			#endregion //ToggleContainingCardEmptyCellsCollapsedState

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
    	public override string ToString()
		{

			StringBuilder sb = new StringBuilder();

			sb.Append("DataRecord- ");
			sb.Append(this.DataItemIndex.ToString());






			if (this._isDeleted == true)
			{
				sb.Append(" --- Deleted --- ");
			}
			else
			{
				Field primary = null;
				FieldLayout fl = this.FieldLayout;

				if (fl != null)
					primary = fl.PrimaryField;

				if (primary != null)
				{
					sb.Append(" ");
					sb.Append(primary.Name);
					sb.Append(": ");

					object cellvalue = this.GetCellValue(primary, true);

					if (cellvalue == null)
						sb.Append("[null]");
					else
						sb.Append(cellvalue.ToString());
				}
			}

			return sb.ToString();
		}

   			#endregion //ToString	
 
			// JJD 10/20/11  - TFS84708 - added
			#region ViewableChildRecordsIfNeeded

		internal override ViewableRecordCollection ViewableChildRecordsIfNeeded
		{
			get
			{
				if (this._expandableFieldRecords == null)
				{
					FieldCollection fields = this.FieldLayout.Fields;

					int enumerableCount = fields.ExpandableFieldsCount;

					// if there are no enumerable fields then return null
					if (enumerableCount < 1)
						return null;
				}

				// for DataRecords that have Expandable Fields we want to always
				// allocate and return the ViewableChildRecords collection
				return this.ViewableChildRecords;
			}
		}

			#endregion //ViewableChildRecordsIfNeeded	
    
		#endregion Base class overrides

        #region Properties

            #region Public Properties

                #region Cells

        /// <summary>
        /// Returns a collection of <see cref="Cell"/> objects.
        /// </summary>
        /// <remarks>The cell objects in this collection are maintained in the same order as their associated <see cref="Field"/>s in the <see cref="FieldLayout.Fields"/> collection.</remarks>
        public CellCollection Cells
        {
            get
            {
				if ( this._cells == null )
					// SSP 12/16/08 - NAS9.1 Record Filtering
					// Added CreateCellCollection method. FilterRecord overrides it to create FilterCellCollection.
					// 
					//this._cells = new CellCollection(this);
					this._cells = this.CreateCellCollection( );

                return this._cells;
            }
        }

                #endregion //Cells	
    
                #region ChildRecords

        /// <summary>
        /// Returns a <see cref="ExpandableFieldRecordCollection"/> for the child records (read-only)
        /// </summary>
        /// <remarks>This collection is keyed by field. It contains only fields that return enumerators.</remarks>
        /// <seealso cref="ExpandableFieldRecord"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ExpandableFieldRecordCollection ChildRecords
        {
            get
            {
				// JJD 09/22/11  - TFS84708 - Optimization
				// MOved logic into helper method
				return GetChildRcdCollection(true);
            }
        }

                #endregion //ChildRecords

                #region DataItem

        /// <summary>
        /// Returns the underlying data from the data source (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataItem 
        { 
            get 
            {
				// JJD 3/13/11 - TFS67970 - Optimization
				// Reverse these tests since checking for a null _dataItem is less expensive for
				// the most common scenarion (since most of the time this will be non-null and
				// normally the version maches as well). We can therefore avoid the overhead of
				// checking IsAddRecord and IsDeleted for the majority os cases.
				// we can avoid the other checks
				//if (!this.IsAddRecord && !this._isDeleted)
				//{
				//    if (this._dataItem == null ||
				//        this._underlyingDataVersion != this.ParentCollection.ParentRecordManager.UnderlyingDataVersion )
				if (this._dataItem == null ||
					this._underlyingDataVersion != this.ParentCollection.ParentRecordManager.UnderlyingDataVersion )
				{
					if (!this._isAddRecord && !this._isDeleted)
					{
						IList sourceList = this.ParentCollection.ParentRecordManager.List;

						if (sourceList != null)
						{
							int index = this.DataItemIndex;

							//Debug.Assert(index >= 0, "DataItemIndex < 0");

							
							
							
							this._underlyingDataVersion = this.ParentCollection.ParentRecordManager.UnderlyingDataVersion;

							if ( index >= 0 && index < sourceList.Count )
								this.SetDataItemHelper(sourceList[index]);
						
							
							
							
							
						}
					}
				}

                return this._dataItem; 
            } 
        }

                #endregion //DataItem

                #region DataItemIndex

        /// <summary>
        /// Returns the associated index in the underlying data source (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int DataItemIndex 
        { 
            get 
            {
				if (this._isDeleted)
					return -1;

				if (this.IsAddRecord && this._dataItem == null)
					return -1;

				// SSP 11/16/10 TFS59686
				// 
				//IList<DataRecord> unsorted = this.ParentCollection.ParentRecordManager.Unsorted;
				DataRecordCollection unsorted = this.ParentCollection.ParentRecordManager.UnsortedInternalVerify;

				// cache the data item index if the old cached value is not valid
				if ( this._dataItemIndex < 0 ||
					 this._dataItemIndex >= unsorted.Count ||
					// SSP 11/16/10 TFS59686
					// Don't allocate item at _dataItemIndex if it's not already allocated.
					//
					//this != unsorted[this._dataItemIndex]
					this != unsorted.SparseArray.GetItem( this._dataItemIndex, false )
					)
					this._dataItemIndex = unsorted.IndexOf(this); 

				return this._dataItemIndex;
            } 
        }

                #endregion //DataItemIndex

				#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Returns the data error associated with the underlying data item (IDataErrorInfo's Error property).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// A data item can provide error information regarding the data item and individual fields
		/// of the data item by implementing IDataErrorInfo interface. If the data item associated with
		/// this record implements IDataErrorInfo, this property returns the data item error - the value
		/// returned by the IDataErrorInfo's Error property.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the corresponding <see cref="DataRecord.HasDataError"/> property returns the
		/// value indicating whether the data item has a data error.
		/// </para>
		/// <para class="body">
		/// Also <b>Note</b> that by default the data presenter doesn't display the data error information. 
		/// To have the data presenter display the data error information, set the
		/// FieldLayoutSettings' <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property.
		/// </para>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecord.GetCellDataError"/>
		/// <seealso cref="DataRecord.GetCellHasDataError"/>
		/// <seealso cref="Cell.HasDataError"/>
		/// <seealso cref="Cell.DataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object DataError
		{
			get
			{
				
				
				
				
				if ( this.IsRecordDataErrorInfoSupported )
				{
					IDataErrorInfo errorInfo = this.GetDataErrorInfo( );
					if ( null != errorInfo )
						return errorInfo.Error;
				}
				
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

				

				return null;
			}
		}

				#endregion // DataError

				#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Indicates if the data record has a data error (IDataErrorInfo's Error property).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property indicates whether the data record has data error. 
		/// DataRecord's <see cref="DataRecord.DataError"/> property returns the actual data error if any.
		/// </para>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.GetCellDataError"/>
		/// <seealso cref="DataRecord.GetCellHasDataError"/>
		/// <seealso cref="Cell.DataError"/>
		/// <seealso cref="Cell.HasDataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool HasDataError
		{
			get
			{
				return ! GridUtilities.IsNullOrEmpty( this.DataError );
			}
		}
  
				#endregion // HasDataError

                #region HasExpandableFieldRecords

        /// <summary>
        /// Returns true if there are any enumerable fields (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasExpandableFieldRecords 
        { 
            get 
            {
                if (this._expandableFieldRecords != null && this._expandableFieldRecords.Count > 0)
                    return true;

                return this.FieldLayout.Fields.ExpandableFieldsCount > 0; 
            } 
        }

                #endregion //HasExpandableFieldRecords

				#region IsAddRecord

		/// <summary>
		/// Returns true if this is an add record that hasn't been commited to the underlying <see cref="DataPresenterBase.DataSource"/> yet.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> add records are included in <see cref="ViewableRecordCollection"/>s. However they are not included in the <see cref="RecordManager"/>'s <see cref="RecordManager.Unsorted"/> or <see cref="RecordManager.Sorted"/> collections until they have a <see cref="DataItem"/> assigned from the <see cref="DataPresenterBase.DataSource"/>.</para>
		/// </remarks>
		/// <seealso cref="DataPresenterBase.InitializeTemplateAddRecord"/>
		/// <seealso cref="DataPresenterBase.RecordAdding"/>
		/// <seealso cref="DataPresenterBase.RecordAdded"/>
		/// <seealso cref="DataPresenterBase.ViewableRecords"/>
		/// <seealso cref="ViewableRecordCollection"/>
		/// <seealso cref="Record.ViewableChildRecords"/>
		public bool IsAddRecord { get { return this._isAddRecord; } }

				#endregion //IsAddRecord	

                // JJD 1/26/08 - Optimization
				#region IsCellAllocated

		/// <summary>
		/// Determines if the cell has been allocated
		/// </summary>
		/// <param name="field">The associated field.</param>
		/// <returns>True if allocated</returns>
		public bool IsCellAllocated(Field field)
		{
            if (this._cells == null)
                return false;

			return this._cells.IsCellAllocated(field);
		}

		/// <summary>
		/// Determines if the cell has been allocated
		/// </summary>
		/// <param name="index">The zero based index of the cell.</param>
		/// <returns>True if allocated</returns>
		public bool IsCellAllocated(int index)
		{
            if (this._cells == null)
                return false;

            return this._cells.IsCellAllocated(index);
		}

				#endregion //IsCellAllocated	

				#region IsDataChanged

		/// <summary>
		/// Returns true if cell data has been edited and have not yet been committed to the <see cref="DataPresenterBase.DataSource"/>.
		/// </summary>
		/// <remarks>
		/// <para class="body">To commit the </para>
		/// </remarks>
		/// <seealso cref="DataPresenterBase.UpdateMode"/>
		public bool IsDataChanged 
		{ 
			get 
			{
				if ( this._isDataChanged )
					return true;

				Cell activeCell = this.DataPresenter.ActiveCell;

				// if the active cell is in edit mode it will return true for
				// IsDataChanged even if the record's flag has been set
				if (activeCell != null &&
					activeCell.Record == this )
					return activeCell.IsDataChanged;

				return false;
			} 
		}

				#endregion //IsDataChanged	

				#region IsDeleted

		/// <summary>
		/// Returns true if this record has been deleted (read-only).
		/// </summary>
		/// <seealso cref="DataPresenterBase.RecordsDeleting"/>
		/// <seealso cref="DataPresenterBase.RecordsDeleted"/>
		/// <seealso cref="DataPresenterCommands.DeleteSelectedDataRecords"/>
		/// <seealso cref="FieldLayoutSettings.AllowDelete"/>
		/// <seealso cref="FieldLayout.AllowDeleteResolved"/>
		/// <seealso cref="RecordManager.DataSourceAllowsDelete"/>
		public bool IsDeleted { get { return this._isDeleted; } }

				#endregion //IsDeleted

				#region IsFilteredOut

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Indicates whether the record is filtered out. Returns true if the record fails to meet filter criteria.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsFilteredOut</b> property indicates whether the record is filtered out. If the record fails to meet
		/// the current effective record filters, this property returns <b>True</b>. Otherwise it returns <b>False</b>.
		/// However, if there are no active record filters then this property returns <b>null</b>.
		/// </para>
		/// <para class="body">
		/// Current effective record filters are either RecordManager's <see cref="RecordManager.RecordFilters"/> 
		/// or the FieldLayout�s <see cref="FieldLayout.RecordFilters"/> depending on whether the FilterLayoutSettings� 
		/// <see cref="FieldLayoutSettings.RecordFilterScope"/> property is set to <b>SiblingDataRecords</b> or 
		/// <b>AllRecords</b> respectively. <b>SiblingDataRecords</b> is the default.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> To enable the record filtering user interface, set the <see cref="FieldSettings.AllowRecordFiltering"/>
		/// and <see cref="FieldLayoutSettings.FilterUIType"/> properties.
		/// </para>
		/// </remarks>
        /// <value>
        /// <para class="body"><b>True</b> if the record fails to meet the current effective record filters or <b>false</b> if it does not. 
        /// However, if there are no active record filters then this property returns <b>null</b></para>
        /// </value>
		/// <seealso cref="RecordManager.RecordFilters"/>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="GroupByRecord.IsFilteredOut"/>
		/// <seealso cref="FieldLayoutSettings.FilterAction"/>
		public bool? IsFilteredOut
		{
			get
			{
				return this.InternalIsFilteredOutNullable_Verify;
			}
		}

				#endregion // IsFilteredOut
    
            #endregion //Public Properties

			#region Internal Properties

				#region CellsIfAllocated

		
		
		/// <summary>
		/// Returns the cells collection if it has been allocated.
		/// </summary>
		internal CellCollection CellsIfAllocated
		{
			get
			{
				return _cells;
			}
		}

				#endregion // CellsIfAllocated

				#region ChildRecordsIfAllocated

		// SSP 8/28/09 TFS21591
		// 
		/// <summary>
		/// Returns the child expandable field records if allocated.
		/// </summary>
		internal ExpandableFieldRecordCollection ChildRecordsIfAllocated
		{
			get
			{
				return _expandableFieldRecords;
			}
		}

				#endregion // ChildRecordsIfAllocated

				#region DataItemForComparison

		internal object DataItemForComparison 
        { 
            get 
            {
				if (this._dataItem == null)
					return null;

				if (this._dataItemForComparison != null)
					return this._dataItemForComparison;

				this._dataItemForComparison = DataRecord.GetObjectForRecordComparision(this._dataItem);

				return this._dataItemForComparison; 
            } 
        }

                #endregion //DataItem

				#region DataItemInternal

		// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
		// 
		internal object DataItemInternal
		{
			get
			{
				return _dataItem;
			}
		}

				#endregion // DataItemInternal

                // JJD 3/7/08 - added
                #region IgnoreChangeNotificationsForField

        // JJD 3/7/08 
        // cache the field during a cell value set so we know to
        // ignore value change notifications for that cell
        internal Field IgnoreChangeNotificationsForField
        {
            set { this._ignoreChangeNotificationsForField = value; }
        }

                #endregion //IgnoreChangeNotificationsForField	
    
				#region IsAddRecordTemplate

		internal bool IsAddRecordTemplate 
		{ 
			get 
			{
				if (this._isAddRecord == false)
					return false;

				return this._dataItem == null;
			} 
		}

				#endregion //IsAddRecordTemplate	

				#region IsRecordDataErrorInfoSupported

		
		
		
		internal bool IsRecordDataErrorInfoSupported
		{
			get
			{
				FieldLayout fl = this.FieldLayout;
				if ( null != fl )
				{
					SupportDataErrorInfo supportDataErrorInfo = fl.SupportDataErrorInfoResolved;

					if ( SupportDataErrorInfo.RecordsOnly == supportDataErrorInfo
						|| SupportDataErrorInfo.RecordsAndCells == supportDataErrorInfo )
					{
						return true;
					}
				}

				return false;
			}
		}

				#endregion // IsRecordDataErrorInfoSupported

				#region FirstDisplayedCell







		internal Cell FirstDisplayedCell
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

                Field field = this.FirstDisplayedField;
                return null != field ? this.Cells[field] : null;
            }
		}

				#endregion //FirstDisplayedCell

                // AS 2/27/09 TFS14730/Optimization
				#region FirstDisplayedField

		internal Field FirstDisplayedField
		{
			get
			{
                FieldLayoutTemplateGenerator generator = this.FieldLayout.StyleGenerator;
                return null != generator ? generator.GridFieldMap.GetFirstField() : null;
            }
		}

				#endregion //FirstDisplayedField

				#region LastDisplayedCell







		internal Cell LastDisplayedCell
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                Field field = this.LastDisplayedField;
                return null != field ? this.Cells[field] : null;
            }
		}

				#endregion //LastDisplayedCell

                // AS 2/27/09 TFS14730/Optimization
				#region LastDisplayedField

		internal Field LastDisplayedField
		{
			get
			{
                FieldLayoutTemplateGenerator generator = this.FieldLayout.StyleGenerator;
                return null != generator ? generator.GridFieldMap.GetLastField() : null;
            }
		}

				#endregion //LastDisplayedField

				#region PropertyDescriptorProvider

		internal FieldLayout.LayoutPropertyDescriptorProvider PropertyDescriptorProvider
		{
			get
			{
				return this._propertyDescriptorProvider;
			}
		}

				#endregion //PropertyDescriptorProvider	

				#region SortIndex

		internal int SortIndex
		{
			get
			{
				return this.ParentCollection.ParentRecordManager.Sorted.IndexOf(this);
			}
		}

				#endregion //SortIndex	
    
			#endregion // Internal Properties

		#endregion //Properties

		#region Methods

			#region Public Methods
    
				#region CancelUpdate

		/// <summary>
		/// Cancels the update of the record when data has been changed (similar to pressing ESC).
		/// </summary>
		/// <remarks>
		/// <p class="body">When the <b>CancelUpdate</b> method is invoked for a record, any changes made to the cells of record are backed out. The cells then display their original values, and the record is taken out of edit mode. The record selector indicator changes from the "Write" image back to the "Current" image. The <b>IsDataChanged</b> property will be set to false.</p>
		/// </remarks>
		public bool CancelUpdate()
		{
			if (!this.IsDataChanged)
				return true;

			// raise the before record update canceled event
			RecordUpdateCancelingEventArgs args = new RecordUpdateCancelingEventArgs(this);

			this.DataPresenter.RaiseRecordUpdateCanceling(args);

			// if cancelled then exit
			if (args.Cancel == true)
				return false;

			this.CancelEdit();

			// raise the after record update canceled event
			this.DataPresenter.RaiseRecordUpdateCanceled(new RecordUpdateCanceledEventArgs(this));

			return true;
		}

				#endregion //CancelUpdate	

				// JJD 1/26/08 - Optimization
				#region GetCellIfAllocated

		/// <summary>
		/// Gets a <see cref="Cell"/> only if it has been previously allocated.
		/// </summary>
		/// <param name="field">The field that identifies the cell in question.</param>
		/// <returns>The <see cref="Cell"/> if it had been previously allocated or null.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> <see cref="Cell"/>s are alloocated lazily except for <see cref="UnboundCell"/>s whose
		/// associated <see cref="UnboundField"/>'s <see cref="UnboundField.BindingPath"/> property is set to a non-null value.</para>
		/// </remarks>
		public Cell GetCellIfAllocated( Field field )
		{
			if ( this._cells == null )
				return null;

			return this._cells.GetCellIfAllocated( field );
		}

				#endregion //GetCellIfAllocated	

				#region GetCellText

		/// <summary>
        /// Returns the text of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Field"/>'s <see cref="Field.Converter"/> is set then it will be used to convert the underlying data value.</para>
        /// </remarks>
        /// <seealso cref="Field"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public string GetCellText(Field field)
        {
            // JJD 5/29/09 - TFS18063 
            // Use the new overload to GetCellValue which will return the value 
            // converted into EditAsType
            //object value = this.GetCellValue(field, true);
            //object value = this.GetCellValue(field, CellValueType.EditAsType);

			
			
			
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			// SSP 4/1/09 - Cell Text
			// Added logic to be able to convert the cell values to display texts.
			// 
			// ----------------------------------------------------------------------
			return CellTextConverterInfo.ConvertCellValue( this, field );
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------------------
			
        }

                #endregion // GetCellText	

                #region GetCellValue

        /// <summary>
        /// Returns the value of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this method returns the unconverted data value, ignoring the <see cref="Field"/>'s <see cref="Field.Converter"/> setting.</para>
        /// </remarks>
        /// <seealso cref="Field"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public object GetCellValue(Field field)
        {
            return this.GetCellValue(field, false);
        }

        // JJD 3/5/08 - Added overload to support Field.Converter properties
        /// <summary>
        /// Returns the value of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <param name="useConverter">If true will use the Converter specified on the Field</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
		// SSP 12/16/08 - NAS9.1 Record Filtering
		// Made GetCellValue method virtual so FilterRecord can override it.
		// 
        //public object GetCellValue(Field field, bool useConverter)
		public virtual object GetCellValue( Field field, bool useConverter )
        {
            if (field == null)
                throw new ArgumentNullException("Field");

			
			
            
			FieldLayout fieldLayout = this.FieldLayout;
			if ( field.Owner != fieldLayout )
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_28" ) );

            // AS 4/15/09 Field.(Get|Set)CellValue
            // We decided to move the implementation from the Field for the Get|SetCellValue
            // into the record since its methods are virtual. The code below is based on what 
            // was done previously in the field's GetCellValue.
            //
            //return field.GetCellValue(this, useConverter);
            //
            // JJD 2/20/09
            // Moved the following if block from GetCellValueHelper to here
            // since we don't want to call the converter with the dummy
            // data we are supplying for the template record.
            //
            // if this is the template data record that we use for virtualization
            // then use sample data based on the data type
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


            object rawValue = this.GetCellValueHelper(field);

            if (useConverter)
				// SSP 3/25/10 TFS26315
				// If there are any cell specific settings then we need to use them so pass along
				// the data record.
				// 
                //return field.ConvertValue(rawValue);
				return field.ConvertValue( rawValue, this );

            return rawValue;
        }

        // AS 4/15/09 Field.(Get|Set)CellValue
        // This routine was moved from the Field class and altered to take a field.
        //
		private object GetCellValueHelper(Field field)
		{
            try
            {
                // JJD 2/20/09
                #region Moved up into the GetCellValue calling method

                //// if this is the template data record that we use for virtualization
                //// then use sample data based on the data type
                //if (record.FieldLayout != null && record.FieldLayout.TemplateDataRecord == record)
                //{
                //    // AS 2/19/09 TFS14301
                //    //return GetDefaultDataForType(this.DataType);
                //    return GetDefaultDataForType(this.EditAsTypeResolved);
                //}

                #endregion //Moved up into the GetCellValue calling method	
    
                // JJD 2/17/09 - TFS14029
                // Move the unbound cell logic after the check for IsAddRecordTemplate since
                // we want to treat unbound cells the same and use the caching logic
                // on the RecordManaer for cell values in the template add record
                //if (this.IsUnbound)
                //    return record.Cells[this].Value;

				// if this is an add record but we haven't called AddNew on the IBindingList
				// yet and therefore don't have a data tiem then use the cached cell values
				// mainatained in the RecordManager for this situation.
				if (this.IsAddRecordTemplate)
					return this.ParentCollection.ParentRecordManager.GetAddRecordCellData( field );

                
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


                // JJD 2/17/09 - TFS14029
                // Moved unbound logic here from above (see note above)
                if (field.IsUnbound)
                {
                    // JJD 2/17/09 - TFS14029
                    // Use the new ValueInternal property instead of Value since that will now
                    // simply call back into this method and cause a stack overflow
                    //return record.Cells[this].Value;
                    return ((UnboundCell)this.Cells[field]).ValueInternal;
               }

				FieldLayout.LayoutPropertyDescriptorProvider provider = this.PropertyDescriptorProvider;

				Debug.Assert(provider != null);
				
				FieldLayout.FieldDescriptor fieldDescriptor = null;

				if ( provider != null )
					fieldDescriptor = provider.GetFieldDescriptor(field);

				if (fieldDescriptor != null)
					return fieldDescriptor.PropertyDescriptor.GetValue(this.DataItem);

                
                
                
				
				return null;
			}
            catch (Exception exception)
            {
				// JJD 5/9/07 - BR22686
				// Pass true in as the last param so that we default the cancel param to true on the event args
				// so we don't display an error message by default. This is required since we there are situations
				// where the underlying data item is invalid and we haven't received the change notification yet.
				//this.Owner.DataPresenter.RaiseDataError(record, this, exception, DataErrorOperation.CellValueGet, "Unable to retrieve cell value.");
				
				
				
				
				
				
				this.DataPresenter.RaiseDataError( this, field, exception, DataErrorOperation.CellValueGet, DataPresenterBase.GetString( "DataError_GetCellValue" ), true );

				// JJD 06/21/12 - TFS113886 
				// If the debugger is attached and logging then log an error
				if (Debugger.IsAttached && Debugger.IsLogging())
				{
					string errorMsg;
					if (this.DataItem == null)
						errorMsg = string.Format("GetCellValue failed because DataItem was null, can not bind to a list that contains null items, item index: {0}{1}", this.Index, Environment.NewLine);
					else
						errorMsg = string.Format("GetCellValue failed, exception type: {0}, exception msg: {1}, item index: {2}, DataItem: {3}{4}", exception.GetType(), exception.Message, this.Index, this.DataItem, Environment.NewLine);

					GridUtilities.LogDebuggerError(errorMsg);
				}

                return null;
            }
        }


        // JJD 5/29/09 - TFS18063 - added
        /// <summary>
        /// Returns the value of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <param name="cellValueType">The type of value to return</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public virtual object GetCellValue(Field field, CellValueType cellValueType)
        {
            if (field == null)
                throw new ArgumentNullException("Field");

            if ( field.Owner != this.FieldLayout )
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_28" ) );

            switch (cellValueType)
            {
                case CellValueType.Raw:
                    return this.GetCellValue(field, false);

                case CellValueType.Converted:
                    return this.GetCellValue(field, true);

                case CellValueType.EditAsType:
                {
                    Type editAsType = field.EditAsTypeResolvedUnderlying;

                    object value = this.GetCellValue(field, true);

                    if (editAsType == field.DataTypeUnderlying)
                        return value;

                    return Utilities.ConvertDataValue(value, editAsType, field.ConverterCultureResolved, null);
                }
            }

    		throw new ArgumentOutOfRangeException("cellValueType");
        }

                #endregion //GetCellValue	

				#region GetCellDataError

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Returns the field has data error as indicated by the underlying data item's IDataErrorInfo implementation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// A data item can provide error information regarding the data item and individual fields
		/// of the data item by implementing IDataErrorInfo interface. If the data item associated with
		/// the data record implements IDataErrorInfo, this method returns the field error if any 
		/// - basically the value returned by the IDataErrorInfo's Item[fieldName] indexer.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the corresponding <see cref="GetCellHasDataError"/> method returns the
		/// value indicating whether there's a data error on the field.
		/// </para>
		/// <para class="body">
		/// Also <b>Note</b> that by default the data presenter doesn't display the data error information. 
		/// To have the data presenter display the data error information, set the
		/// FieldLayoutSettings' <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property.
		/// </para>
		/// <seealso cref="DataRecord.GetCellHasDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="Cell.HasDataError"/>
		/// <seealso cref="Cell.DataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object GetCellDataError( Field field )
		{
			if ( field.SupportDataErrorInfoResolved )
			{
				if ( ! field.IsUnbound )
				{
					IDataErrorInfo errorInfo = this.GetDataErrorInfo( );
					if ( null != errorInfo )
						return errorInfo[field.Name];
				}
				// SSP 6/28/10 TFS23257
				// 
				// SSP 4/27/11 73511
				// Check to see if this is a data record.
				// 
				//else
				else if ( this.IsDataRecord )
				{
					UnboundCell ubCell = this.Cells[field] as UnboundCell;
					Debug.Assert( null != ubCell );
					if ( null != ubCell )
						return ubCell.DataErrorInternal;
				}
			}

			return null;
		}

				#endregion // GetCellDataError

				#region GetCellHasDataError

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Indicates if the field has a data error as indicated by the underlying data item's 
		/// IDataErrorInfo implementation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>GetCellHasDataError</b> method returns a value indicating whether the field has data error. 
		/// <see cref="GetCellDataError"/> method returns the actual data error if any.
		/// </para>
		/// <seealso cref="DataRecord.GetCellDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="Cell.HasDataError"/>
		/// <seealso cref="Cell.DataError"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldSettings.SupportDataErrorInfo"/>
		/// </remarks>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool GetCellHasDataError( Field field )
		{
			object error = this.GetCellDataError( field );
			return ! GridUtilities.IsNullOrEmpty( error );
		}

				#endregion // GetCellHasDataError

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region GetDataValueHistory

		/// <summary>
		/// Returns an IList of <see cref="DataValueInfo"/> instances that contains the history of data values for the <see cref="Field"/> with the specified name in the current DataRecord.
		/// </summary>
		/// <param name="fieldName">The name of the Field to retrieve the data value history for.</param>
		/// <returns>IList of <see cref="DataValueInfo"/> instances or null if DataValueChanged notifications are not active for the <see cref="Field"/>.</returns>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.DataValueChangedNotificationsActive"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public IList<DataValueInfo> GetDataValueHistory(string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException("fieldName");


			if (this.FieldLayout == null)
				return null;

			Field field = this.FieldLayout.Fields[fieldName];
			if (field != null)
				return this.GetDataValueHistory(field);

			return null;
		}

		/// <summary>
		/// Returns an IList of <see cref="DataValueInfo"/> instances that contains the history of data values for the specified <see cref="Field"/> in the current DataRecord.
		/// </summary>
		/// <param name="field">The Field to retrieve the data value history for.</param>
		/// <returns>IList of <see cref="DataValueInfo"/> instances or null if DataValueChanged notifications are not active for the <see cref="Field"/>.</returns>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.DataValueChangedNotificationsActive"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public IList<DataValueInfo> GetDataValueHistory(Field field)
		{
			if (field == null)
				throw new ArgumentNullException("field");


			Cell cell = this.GetCellIfAllocated(field);
			if (cell != null)
			{
				if (field.DataValueChangedNotificationsActiveResolved == true)
					return cell.DataValueInfoHistoryCache as IList<DataValueInfo>;
			}

			return null;
		}

				#endregion GetDataValueHistory

				#region GetExportValue

		// SSP 10/22/09 TFS24061
		// Added GetExportValue method to be used by the excel exporter to cover the scenario
		// where XamComboEditor is being used to map values to display texts in which case
		// we need to export the mapped display texts to excel document so the end user sees 
		// the same content in the exported document as what's in the data presenter.
		// 
		/// <summary>
		/// Returns the value that should be used when exporting, for example to excel exporter.
		/// </summary>
		/// <param name="field">Field whose export value will be returned.</param>
		/// <returns>The export value.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically the cell's value converted using the field converter if any is used when exporting
		/// to external documents, for example to excel exporter. However if the field's value
		/// is mapped to display text (using XamComboEditor for example) then the display text 
		/// should be exported to ensure that the same values are in the exported document as
		/// what the user sees in the data presenter.
		/// </para>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public object GetExportValue( Field field )
		{
			CellTextConverterInfo info = CellTextConverterInfo.GetCachedConverter( field );

			if ( null != info && info.ShouldExportText )
				return info.ConvertCellValue( this );

			return this.GetCellValue( field, CellValueType.EditAsType );
		}

				#endregion // GetExportValue

				// JJD 4/27/11 - TFS73888 - added 
				#region RefreshCellValue

		/// <summary>
		/// Gets the cell's value and updates the display if the cell is in view
		/// </summary>
		/// <param name="field">The field that identifies the cell</param>
		public void RefreshCellValue(Field field)
		{
			GridUtilities.ValidateNotNull(field, "field");

			if (field.Owner == null ||
				 field.Owner != this.FieldLayout)
				return;

			// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
			// Since we are called thru a public method pass in false for isRecordPresenterDeactivated 
			// paramter so we do update the cvp's value 
			//this.RefreshCellValue(field, false);
			this.RefreshCellValue(field, false, false);
		}

				#endregion //RefreshCellValue	
    
				#region RefreshFilters

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Re-evaluates filters on the record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Re-evaluates filter conditions on the record to ensure the record's filtered state is up-to-date.
		/// </para>
		/// <para class="body">
		/// Typically there is no need for you to call this method as filters are automatically re-evaluated whenever
		/// record data or filter criteria changes. However there may be times when you may want to re-evaluate filters
		/// on a record manually (for example if the data source doesn't support data notifications on a record or
		/// your custom filter condition logic changes).
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that you can re-evaluate filters on all records by calling the <see cref="RecordFilterCollection"/>'s
		/// <see cref="RecordFilterCollection.Refresh"/> method.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecordFilterCollection.Refresh"/>
		/// <seealso cref="FieldLayout.RecordFilters"/>
		/// <seealso cref="RecordManager.RecordFilters"/>
		public void RefreshFilters( )
		{
			this.ApplyFiltersHelper( );
		}

				#endregion // RefreshFilters

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region ResetDataValueHistory

		/// <summary>
		/// Clears the history of data values (except for the most current value) for all <see cref="Cells"/> in the current DataRecord.
		/// </summary>
		/// <param name="recursive">If true, then this method is recursively called on child <see cref="DataRecord"/>s</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public void ResetDataValueHistory(bool recursive)
		{
			if (this.FieldLayout == null)
				return;

			// Reset the history for all cells in this record
			FieldCollection fields = this.FieldLayout.Fields;
			foreach (Field field in fields)
			{
				this.ResetDataValueHistory(field);
			}

			// If asked, reset the history for all child data records.
			if (recursive)
			{
				if (this.ChildRecords != null)
				{
					ExpandableFieldRecordCollection efrc = this.ChildRecords;
					foreach (ExpandableFieldRecord efr in efrc)
					{
						// JJD 09/22/11  - TFS84708 - Optimization
						// Use the ChildRecordManagerIfNeeded instead which won't create
						// child rcd managers for leaf records
						//efr.ChildRecordManager.ResetDataValueHistory(recursive);
						RecordManager crm = efr.ChildRecordManagerIfAllocated;

						if ( crm != null )
							crm.ResetDataValueHistory(recursive);
					}
				}
			}
		}

		/// <summary>
		/// Clears the history of data values (except for the most current value) for the <see cref="Field"/> with the specified name in the current DataRecord.
		/// </summary>
		/// <param name="fieldName">The name of the Field whose data value history should be cleared.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public void ResetDataValueHistory(string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException("fieldName");

			if (this.FieldLayout == null)
				return;

			Field field = this.FieldLayout.Fields[fieldName];
			if (field != null)
				this.ResetDataValueHistory(field);
		}

		/// <summary>
		/// Clears the history of data values (except for the most current value) for the specified <see cref="Field"/> in the current DataRecord.
		/// </summary>
		/// <param name="field">The Field  whose data value history should be cleared.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public void ResetDataValueHistory(Field field)
		{
			if (field == null)
				throw new ArgumentNullException("field");

			Cell cell = this.GetCellIfAllocated(field);
			if (cell != null)
			{
				// If DataValueChangedNotifications are no longer active for the associated Field, then destroy the
				// cache and all its entries.  Otherwise, remove all entries except the newest one.
				cell.ClearDataValueChangedHistory(field.DataValueChangedNotificationsActiveResolved == false);
			}
		}

				#endregion ResetDataValueHistory
    
                #region SetCellValue

        /// <summary>
        /// Sets the value of a specific cell
        /// </summary>
        /// <param name="field">The field whose value to set.</param>
        /// <param name="value">The new unconverted value.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this method sets the unconverted data value, ignoring the <see cref="Field"/>'s <see cref="Field.Converter"/> setting.</para>
        /// </remarks>
        /// <seealso cref="Field"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public void SetCellValue(Field field, object value)
        {
             this.SetCellValue(field, value, false);
        }

        // JJD 3/5/08 - Added overload to support Field.Converter properties
        /// <summary>
        /// Sets the value of a specific cell
        /// </summary>
        /// <param name="field">The field whose value to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="useConverter">If true will use the Converter specified on the Field</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        // AS 4/15/09 NA 2009.2 ClipboardSupport
        // Added an overload to determine if the operation should be added to 
        // the undo stack. I also changed the method from virtual to non-virtual.
        // This shouldn't break anyone since there is no public means of providing 
        // a more derived record class.
        //
        //// SSP 12/16/08 - NAS9.1 Record Filtering
        //// Made SetCellValue method virtual so FilterRecord can override it.
        //// 
        ////public void SetCellValue(Field field, object value, bool useConverter)
        //// AS 4/15/09 Field.(Get|Set)CellValue
        //// Needed to return a boolean value.
        ////
		//public virtual void SetCellValue( Field field, object value, bool useConverter )
		public bool SetCellValue( Field field, object value, bool useConverter )
        {
            return SetCellValue(field, value, useConverter, false);
        }

        // AS 4/15/09 NA 2009.2 ClipboardSupport
        // Add addToUndo parameter.
        //
        /// <summary>
        /// Sets the value of a specific cell
        /// </summary>
        /// <param name="field">The field whose value to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="useConverter">If true will use the Converter specified on the Field</param>
        /// <param name="addToUndo">If true, the value will be added to the undo stack.</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
		// AS 5/5/09 NA 2009.2 ClipboardSupport
		// Changed to non-virtual to add a new virtual overload that returns the dataerrorinfo.
		//
		//public virtual bool SetCellValue(Field field, object value, bool useConverter, bool addToUndo)
        public bool SetCellValue(Field field, object value, bool useConverter, bool addToUndo)
        {
			return SetCellValue(field, value, useConverter, addToUndo, false, UndoValueMissing, false);
		}

		// AS 5/22/09 NA 2009.2 Undo/Redo
		internal static object UndoValueMissing = new object();

		// AS 5/5/09 NA 2009.2 ClipboardSupport
		// Also added an overload we can use when reverting the values so that we can 
		// prevent any error dialog from being shown during the revert.
		//
		// AS 5/22/09 NA 2009.2 Undo/Redo
		// I had to add a parameter for the undo value. This was needed because the current value, 
		// which is what would have been stored previously as the original value, may already have 
		// been edited while in edit mode. So when we call this method from within the CommitEditValue
		// we can use the same value that the editor would have used when a cancel occurred - that is 
		// the OriginalValue of the Editor.
		//
		// AS 7/30/09 NA 2009.2 Field Sizing
		// Added the isCommittingEdit. As a result of an edit, the extent of the field in an autosized field 
		// may be changed. In that case it would be best if when the edit was undone that the extent of the 
		// fields was also undone.
		//
		internal bool SetCellValue( Field field, object value, bool useConverter, bool addToUndo, bool suppressErrorDialog, object undoValue, bool isCommittingEdit )
		{
			return this.SetCellValue( field, value, useConverter, addToUndo, suppressErrorDialog, undoValue, isCommittingEdit, false );
		}

		// SSP 3/19/12 - Calc manager support
		// Added an overload that takes 'dontSetDataChanged' flag.
		// 
		internal bool SetCellValue( Field field, object value, bool useConverter, bool addToUndo, bool suppressErrorDialog, object undoValue, bool isCommittingEdit, bool dontSetDataChanged )
		{
			// AS 5/12/09 NA 2009.2 Undo/Redo
			if (addToUndo)
			{
				DataPresenterBase dp = this.DataPresenter;

				if (null != dp && dp.IsUndoEnabled)
				{
					Debug.Assert(suppressErrorDialog == false, "The undo action doesn't currently know not to raise the error dialog");
					SingleCellValueAction undoAction = new SingleCellValueAction(value, useConverter, this, field, undoValue, isCommittingEdit);

					return dp.History.PerformAction(undoAction, null);
				}
			}

			DataErrorInfo errorInfo;
			// SSP 3/19/12 - Calc manager support
			// Added an overload that takes 'dontSetDataChanged' flag.
			// 
			//bool result = SetCellValue(field, value, useConverter, out errorInfo);
			bool result = SetCellValue( field, value, useConverter, out errorInfo, dontSetDataChanged );

			if (null != errorInfo)
			{
				DataPresenterBase dp = this.DataPresenter;

				Debug.Assert(null != dp);

				if (null != dp)
					dp.RaiseDataError(errorInfo, suppressErrorDialog);
			}

			return result;
		}

		// AS 5/5/09 NA 2009.2 ClipboardSupport
		// Added overload to return a DataErrorInfo so the clipboard operation can raise
		// its own event.
		//
		internal bool SetCellValue(Field field, object value, bool useConverter, out DataErrorInfo errorInfo)
		{
			return this.SetCellValue( field, value, useConverter, out errorInfo, false );
		}

		// SSP 3/19/12 - Calc manager support
		// Added an overload that takes 'dontSetDataChanged' flag.
		// 
		internal virtual bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo, bool dontSetDataChanged )
        {
            if (field == null)
                throw new ArgumentNullException("Field");

            if ( field.Owner != this.FieldLayout )
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_28" ) );

			// AS 5/5/09 NA 2009.2 ClipboardSupport
			errorInfo = null;

            // AS 4/15/09 Field.(Get|Set)CellValue
            // Moved the implementation from the Field's SetCellValue here since we 
            // were sometimes calling directly into that method whereas this method
            // was virtual and overriden by things like the FilterRecord.
            //
            //field.SetCellValue(this, value, useConverter);
            //
            //Debug.Assert(this.RecordType == RecordType.DataRecord);

            object newRawValue = value;

            // JJD 10/22/09 - TFS24092
            // Moved converter logic below
            //// JJD 3/5/08 - Added support for Converter properties
            //// If the passed in value is the converted value then we need to convert it back
            //// to a raw value
            //if (useConverter && field.Converter != null)
            //     newRawValue = field.Converter.ConvertBack(value, field.DataType, field.ConverterParameter, field.ConverterCultureResolved);

			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


            // JJD 10/22/09 - TFS24092
            // Moved the logic to call the converter here from above because if we are calling the converter
            // we don't want to then call Utilities.ConvertDataValue. We should use the converter's
            // ConvertBack value as is.
            //
            // JJD 3/5/08 - Added support for Converter properties
            // If the passed in value is the converted value then we need to convert it back
            // to a raw value
            if (useConverter && field.Converter != null)
				// SSP 3/25/10 TFS26315
				// Pass along record for the converter parameter if none has been specified.
				// 
                //newRawValue = field.Converter.ConvertBack(value, field.DataType, field.ConverterParameter, field.ConverterCultureResolved);
				newRawValue = field.Converter.ConvertBack( value, field.DataType, field.GetConverterParameterResolved( this ), field.ConverterCultureResolved );
            else
            {
                // JJD 6/3/09 - TFS18063
                // Make sure that the new value matches the DataType expected
                newRawValue = Utilities.ConvertDataValue(newRawValue, field.DataType, field.ConverterCultureResolved, null);
            }

            // JJD 3/5/08 - Added support for Converter properties
            // cache the old raw value in a stack variable
            // AS 4/15/09 Field.(Get|Set)CellValue
            //object oldRawValue = field.GetCellValue(this, false);
            object oldRawValue = this.GetCellValue(field, false);

			// if the values are the same then return
			//if (Object.Equals(value, field.GetCellValue(this)))
			if (Object.Equals(newRawValue, oldRawValue))
				return true;

			Cell cell = this.GetCellIfAllocated(field);
            
            // JJD 3/5/08 - Added support for Converter properties
            // convert the oldRawValue 
            object oldConvertedValue = oldRawValue;

			// SSP 3/3/09 TFS11407
			// Added code to raise InitializeRecord event whenever a cell value changes.
			// 
			bool raiseInitializeRecord = false;
			DataPresenterBase dataPresenter = field.Owner.DataPresenter;

            try
            {
                if (this.DataItem != null)
                {
                    // raise the before cell update event
                    CellUpdatingEventArgs args = new CellUpdatingEventArgs(this, field);

					
					
                    
					dataPresenter.RaiseCellUpdating( args );

                    // if cancelled then exit
                    if (args.Cancel == true)
                        return false;
                }

                // JJD 3/7/08
                // flag the field on the this so the this knows to ignore
                // change notifications during the set
                this.IgnoreChangeNotificationsForField = field;

                // JJD 2/17/09 - TFS14029
                // Move the unbound cell logic after the check for IsAddRecordTemplate since
                // we want to treat unbound cells the same and use the caching logic
                // on the RecordManaer for cell values in the template add this
                //if (field.IsUnbound)
                //{
                //    if (cell == null)
                //        cell = this.Cells[field];

                //    cell.Value = newRawValue;
                //}
                // if field is an add this but we haven't called AddNew on the IBindingList
                // yet and therefore don't have a data tiem then use the cached cell values
                // mainatained in the RecordManager for field situation.
                //else if (this.IsAddRecord && this.DataItem == null)
                if (this.IsAddRecordTemplate)
                {
                    this.ParentCollection.ParentRecordManager.SetAddRecordCellData(field, newRawValue);
                }
				else
				{
					// SSP 3/3/09 TFS11407
					// Added code to raise InitializeRecord event whenever a cell value changes. Suspend 
					// InitializeRecord event for field this. We are raising it at the end of the 
					// process in finally block based on raiseInitializeRecord flag.
					// 
					bool initializeRecordAlreadySuspended = dataPresenter.SuspendInitializeRecordFor( this );
					if ( !initializeRecordAlreadySuspended )
						raiseInitializeRecord = true;

					// JJD 2/17/09 - TFS14029
					// Moved unbound logic here from above (see note above)
					if ( field.IsUnbound )
					{
						if ( cell == null )
							cell = this.Cells[field];

						// JJD 2/17/09 - TFS14029
						// Use the new ValueInternal property instead of Value since that will now
						// simply call back into field method and cause a stack overflow
						//cell.Value = newRawValue;
						( (UnboundCell)cell ).ValueInternal = newRawValue;
					}
					else
					{
						FieldLayout.FieldDescriptor fieldDescriptor = this.PropertyDescriptorProvider.GetFieldDescriptor( field );

						if ( fieldDescriptor != null )
						{
							this.BeginEdit( );

							
							
							
							
							PropertyDescriptor propertyDescriptor = fieldDescriptor.PropertyDescriptor;
							if ( propertyDescriptor is ValuePropertyDescriptor )
							{
								IList list = this.ParentCollection.ParentRecordManager.List;
								if ( null != list && !list.IsReadOnly )
								{
									int itemIndex = this.DataItemIndex;
									if ( itemIndex >= 0 && itemIndex < list.Count )
									{
										list[itemIndex] = newRawValue;

										// Make sure the this re-fetches the data item from the list.
										this.RefreshDataItem( );
									}
									else
									{
										Debug.Assert( false, "DataItemIndex invalid!" );

										// AS 8/20/09 TFS19091
										// We're going to consider this an error condition for which we want to display
										// an error message.
										//
										errorInfo = new DataErrorInfo(this, field, null, DataErrorOperation.CellValueSet, DataPresenterBase.GetString("LE_ValueProperty_InvalidIndex"));

										return false;
									}
								}
								else
								{
									// AS 8/20/09 TFS19091
									// We're going to consider this an error condition for which we want to display
									// an error message.
									//
									if (null != list && list.IsReadOnly)
										errorInfo = new DataErrorInfo(this, field, null, DataErrorOperation.CellValueSet, DataPresenterBase.GetString("LE_ValueProperty_ListIsReadOnly"));

									return false;
								}
							}
							else
							{
								propertyDescriptor.SetValue( this.DataItem, newRawValue );
							}
							

							// SSP 3/19/12 - Calc manager support
							// Added an overload that takes 'dontSetDataChanged' flag. Enclosed the existing code into the if block.
							// 
							if ( !dontSetDataChanged )
							{
								if ( cell != null )
									cell.InternalSetDataChanged( true );
								else
									this.InternalSetDataChanged( true );
							}
						}
						else
						{
							Debug.Fail( "Shouldn't get here in Field.GetCellValueHelper" );
							return false;
						}
					}
				}

				
				
				
				
				
				
				
				RecordManager thisManager = this.RecordManager;
				if ( null != thisManager )
					thisManager.OnDataChanged( DataChangeType.CellDataChange, this, field );

                // if a cell was allocated resync the value on the cell value presenter
                if (cell != null)
                {
                    // JJD 3/5/08 - Raise the cell's property changed events
                    cell.RaiseAutomationValueChanged(oldConvertedValue, cell.ConvertedValue);

                    // JJD 3/7/08
                    // do raise redundant notifications on unbound cells
                    if (!cell.IsUnbound)
                        cell.RaiseValueChangedNotifications();

					// AS 4/29/09 TFS17168
					// Moved outside of this loop since we may have a CVP without having
					// allocated the cell.
					//
					//if (cell.AssociatedCellValuePresenter != null)
					//{
					//    // JJD 3/5/08 - Added support for Converter properties
					//    //cell.AssociatedCellValuePresenter.Value = cell.Value;
					//    // SSP 1/21/09 TFS12327
					//    // This is to prevent recursion where CellValuePresenter's
					//    // ValueProperty set will call back field method to update
					//    // the this with the set value. See notes on 
					//    // CellValuePresenter.CVPValueWrapper class for more info.
					//    // 
					//    //cell.AssociatedCellValuePresenter.Value = cell.ConvertedValue;
					//    cell.AssociatedCellValuePresenter.SetValueInternal( cell.ConvertedValue, false );
					//}
                }

				// AS 4/29/09 TFS17168
				// AssociatedCellValuePresenter basically just calls into FromCell,
				// which then calls into FromRecordAndField. We can just call directly
				// into that method with the record and field regardless of whether we
				// have a cell.
				//
				CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(this, field);

				if (null != cvp)
				{
					// SSP 1/21/09 TFS12327 / AS 4/29/09 TFS17168
					// This is to prevent recursion where CellValuePresenter's
					// ValueProperty set will call back this method to update
					// the record with the set value. See notes on 
					// CellValuePresenter.CVPValueWrapper class for more info.
					// 
					//cvp.Value = this.GetCellValue(record, true);

                    // JJD 5/29/09 - TFS18063 
                    // Use the new overload to GetCellValue which will return the value 
                    // converted into EditAsType
                    //cvp.SetValueInternal(this.GetCellValue(field, true), false);
					cvp.SetValueInternal(this.GetCellValue(field, CellValueType.EditAsType), false);
				}

				// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
				// 
				// SSP 6/28/10 TFS23257
				// 
				//this.RefreshDataErrorInfo( field, true );
				// SSP 1/17/10 TFS61443
				// Since IDataErrorInfo doesn't support notifications, it's possible that the IDataErrorInfo.Error
				// property has changed as a result of a cell value change. In order to minimize performance impact,
				// we'll only dirty IDataErrorInfo.Error when the cell value is modified through our API and will 
				// not consider it dirty when we get a property change notification from the data source.
				// 
				//this.RefreshDataErrorInfo( field, false );
				this.RefreshDataErrorInfo( field, false, true );

                if (this.DataItem != null)
                {
                    // raise the after cell update event
					
					
                    
					dataPresenter.RaiseCellUpdated( new CellUpdatedEventArgs( this, field ) );
                }

                return true;
            }
            catch (Exception exception)
            {
                // JJD 3/7/08
                // reset the ignore field on the this
                this.IgnoreChangeNotificationsForField = null;

				// SSP 3/3/09 TFS11407
				// If we had suspended InitializeRecord for the this, resume it.
				// However don't raise the event since setting the value failed and thus 
				// there's no need to raise the event.
				// 
				if ( raiseInitializeRecord )
				{
					dataPresenter.ResumeInitializeRecordFor( this );
					raiseInitializeRecord = false;
				}

                
                
                
				
				
                
				// AS 5/5/09 NA 2009.2 ClipboardSupport
				//dataPresenter.RaiseDataError( this, field, exception, DataErrorOperation.CellValueSet, SR.GetString( "DataError_UpdateCellValue" ) );
				errorInfo = new DataErrorInfo( this, field, exception, DataErrorOperation.CellValueSet, DataPresenterBase.GetString( "DataError_UpdateCellValue" ) );

                return false;
            }
            finally
            {
                // JJD 3/7/08
                // reset the ignore field on the this
                this.IgnoreChangeNotificationsForField = null;

				// SSP 3/3/09 TFS11407
				// 
				if ( raiseInitializeRecord )
				{
					dataPresenter.ResumeInitializeRecordFor( this );
					// JJD 11/17/11 - TFS78651 
					// Added sortValueChanged parameter
					//this.FireInitializeRecord(true);
					this.FireInitializeRecord(true, field.SortStatus != SortStatus.NotSorted);
				}
            }

        }

                #endregion //SetCellValue	

				#region Update

		/// <summary>
		///  Updates the data source with any modified cell values fopr this record.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>Update</b> method updates any modified information in the record, sending it to the data provider. When the update is complete the <b>DataChanged</b> property will be set to False.</p>
		/// <p class="body">Normally, this is handled automatically, so there will be few situations in which you will need to invoke this method. The major exception is when you have set the <b>UpdateMode</b> property to 'OnUpdate'. When using that setting, no automatic updates will be sent to the data provider until you invoke the <b>Update</b> method. You must use the method to manually update the data provider whenever data has been changed and you are ready to commit the changes.</p>
		/// </remarks>
		// AS 5/5/09 NA 2009.2 ClipboardSupport
		// Added an overload so the clipboard actions could get the dataerror info that arises
		// from the update and raise its own event.
		//
		//public virtual bool Update()
		public bool Update()
		{
			DataErrorInfo errorInfo;

			bool result = this.Update(out errorInfo);

			if (null != errorInfo)
			{
				DataPresenterBase dp = this.DataPresenter;

				if (null != dp)
					dp.RaiseDataError(errorInfo);
			}

			return result;
		}

		internal virtual bool Update(out DataErrorInfo errorInfo)
		{
			errorInfo = null;

			if (!this.IsDataChanged)
				return true;

			// raise the before record update canceled event
			RecordUpdatingEventArgs args = new RecordUpdatingEventArgs(this);

			this.DataPresenter.RaiseRecordUpdating(args);

			// if cancelled then exit
			switch (args.Action)
			{
				case RecordUpdatingAction.CancelUpdateRetainChanges:
					return false;

				case RecordUpdatingAction.CancelUpdateDiscardChanges:
					// call CancelUpdate to revert to the original values
					this.CancelUpdate();
					return false;
			}

			// JJD 5/23/07 - BR23169
			// Only raise the RecordUpdated event if CommitChanges was successful
			//this.CommitChanges();
			// raise the after record update canceled event
			//
			//this.DataPresenter.RaiseRecordUpdated(new RecordUpdatedEventArgs(this));
			// AS 5/5/09 NA 2009.2 ClipboardSupport
			//bool rtn = this.CommitChanges();
			bool rtn = this.CommitChanges(out errorInfo);

			// raise the after record update canceled event
			if (rtn == true)
			{
				this.DataPresenter.RaiseRecordUpdated(new RecordUpdatedEventArgs(this));
			}

			//return true;
			return rtn;
		}

				#endregion //Update

			#endregion //Public Methods

            #region Internal Methods

				#region ApplyFilters

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 

		internal void ApplyFiltersHelper( )
		{
			RecordManager rm = this.RecordManager;
			Debug.Assert( null != rm );
			this.ApplyFiltersHelper( null != rm ? rm.RecordFiltersResolved : null, true );
		}
		
		internal void ApplyFiltersHelper( ResolvedRecordFilterCollection filters, bool raiseNotifications )
		{
			if ( FilterState.NeverFilter != _cachedFilterState )
			{
				if ( this.IsAddRecord || this.IsAddRecordTemplate )
				{
					// Don't filter out add-record until it's committed. Leave 
					// _cachedFilterState to NotFilteredYet.
					// 
				}
				else if ( this.IsDataRecord )
				{
					// MeetsCriteria returns null if there are no record filters, in which case
					// the filter state should be set to 0 to indicate that there are no filters.
					// 
					
					
					
					bool? meetsCriteria = null != filters ? filters.MeetsCriteria( this ) : null;
					FilterState state = ! meetsCriteria.HasValue ? 0
						: ( meetsCriteria.Value ? FilterState.FilteredIn : FilterState.FilteredOut );

					this.InternalSetFilterState( state, raiseNotifications );
				}
				else
				{
					// Records that are not data records never get filtered out.
					// 
					_cachedFilterState = FilterState.NeverFilter;
				}
			}
		}

				#endregion // ApplyFilters

				#region BeginEdit

		internal void BeginEdit()
		{
            object dataItem = this.DataItem;

            if (dataItem != null)
            {
                // JJD 11/17/08 - TFS6743/BR35763 
                // If we have an EditableCollectionView then call its EditItem method
                // instead of calling IEditableObject's BeginEdit method
                EditableCollectionViewProxy editableCollectionView = this.ParentCollection.ParentRecordManager.EditableCollectionView;

                if (editableCollectionView != null)
                    editableCollectionView.EditItem(dataItem);
                else
                {
                    IEditableObject editableObject = dataItem as IEditableObject;
                    if (null != editableObject)
                    {
                        editableObject.BeginEdit();
                    }
                }
            }
			this._beginEditCalled = true;
		}

				#endregion //BeginEdit	
    
				#region CancelEdit

		internal void CancelEdit()
		{
			bool wasActive = this.IsActive;
			
			if (wasActive)
			{
				Cell cell = this.DataPresenter.ActiveCell;

				if (cell != null)
				{
					// force exit of edit mode
					if (cell.IsInEditMode)
						cell.EndEditMode(false, true);
				}
			}

			// JJD 2/22/07 - BR20439
			// hold the dataitem on the stack
			object dataItem = this._dataItem;

			bool isAddReordWithDataItem = this._isAddRecord && this._dataItem != null;

			IEditableObject editableObject = this.DataItem as IEditableObject;

			// SSP 10/12/07 BR26397
			// Store the dataItemIndex so we can use it further below. The DataItemIndex will 
			// return -1 when we null out the data item below.
			// 
			int dataItemIndex = this.DataItemIndex;

			// clear out the cached data item before we call CancelEdit so this
			// add record can go back to being a template add record prior to
			// the notifications that get triggered during CancelEdit
			if (isAddReordWithDataItem)
			{
			    this.SetDataItemHelper(null);
				//this.ParentCollection.ViewableRecords.OnAddRecordCanceled(this);
			}

            // JJD 11/17/08 - TFS6743/BR35763 
            // If we have an EditableCollectionView then call its CancelEdit method
            // instead of calling IEditableObject's CancelEdit method
            EditableCollectionViewProxy editableCollectionView = this.ParentCollection.ParentRecordManager.EditableCollectionView;
            if (editableCollectionView != null)
            {
                if (editableCollectionView.IsAddingNew &&
                    editableCollectionView.CurrentAddItem == dataItem)
                    editableCollectionView.CancelNew();
                else
                {
                    if (editableCollectionView.CanCancelEdit &&
                         editableCollectionView.IsEditingItem &&
                        editableCollectionView.CurrentEditItem == dataItem)
                        editableCollectionView.CancelEdit();
                }
            }
			// JJD 8/23/11 - TFS84602
			// Moved editableObject logic below to we
			// can check if list implements ICancelAddNew first and call
			// CancelNew on it instead
			//else if (null != editableObject)
			//{
			//    // JJD 6/17/09 - TFS12228 
			//    // Don't call CancelEdit unless BeginEdit was called
			//    if ( this._beginEditCalled == true )
			//        editableObject.CancelEdit();
			//}
			// SSP 10/12/07 BR26397
			// If the list object doesn't implement IEditableObject then see if the
			// list implements ICancelAddNew. If so then call EndNew on that if this
			// row is an add-row (ICancelAddNew is only meant for add-rows as its
			// name implies).
			// 
			// JJD 8/23/11 - TFS84602
			// next the editableObject object logic from above in the else block
			//else if ( isAddReordWithDataItem )
			else
			{
				// JJD 8/23/11 - TFS84602
				// Keep track of whether we call CancelNew below
				bool cancelNewCalled = false;

				if (isAddReordWithDataItem)
				{
					IList dataList = this.ParentCollection.ParentRecordManager.List;
					ICancelAddNew cancelAddNew = dataList as ICancelAddNew;
					if (null != cancelAddNew)
					{
						int listIndex = dataItemIndex;
						if (listIndex >= 0 && listIndex < dataList.Count)
						{
							cancelAddNew.CancelNew(listIndex);

							// JJD 8/23/11 - TFS84602
							// set a flag so we know to bypass the call to
							// editableObject.CancelEdit() below
							cancelNewCalled = true;
						}
					}
				}

				// JJD 8/23/11 - TFS84602
				// Only call editableObject.CancelEdit() if cancelAddNew.CancelNew
				// was not called above
				if (false == cancelNewCalled && null != editableObject)
				{
					// JJD 6/17/09 - TFS12228 
					// Don't call CancelEdit unless BeginEdit was called
					if ( this._beginEditCalled == true )
						editableObject.CancelEdit();
				}
			}

			// JJD 11/18/11 - TFS79001
			// Moved from below
			RecordManager rm = this.RecordManager;

			// JJD 2/22/07 - BR20439
			// if we have gotten to this point and we are still in the collection
			// then we need to do an explicit delete for add records
			if (isAddReordWithDataItem &&
				dataItem != null &&
				(null == editableObject || this.ParentCollection.SparseArray.Contains(this)))
			{
				IList underlyingList = this.ParentCollection.ParentRecordManager.List;

				Debug.Assert(underlyingList != null);

				if (underlyingList != null)
				{
					underlyingList.Remove(dataItem);

					// JJD 11/18/11 - TFS79001
					// Let the RecordManager know that we have removed the item from the list
					if (rm != null)
						rm.OnDataItemRemoved(this, dataItem);
				}

			}

			// SSP 6/9/08 BR33523
			// If add-record is being removed then call PrepareForNewCurrentAddRecord which takes
			// the necessary steps of dirtying the the special records list.
			// 
			// JJD 11/18/11 - TFS79001 
			// Moved rm stack variable above
			//RecordManager rm = this.RecordManager;
			if ( isAddReordWithDataItem && null != rm )
				rm.PrepareForNewCurrentAddRecord( );

			//// let the record manager know the add record edit was canceled
			//if (isAddReordWithDataItem)
			//{
			//    this.SetDataItemHelper(null);
			//    //this.ParentCollection.ParentRecordManager.OnAddRecordCanceled(this);
			//}

			this._beginEditCalled = false;

			this.InternalSetDataChanged(false);

			// AS 5/11/09 TFS17514
			// Setting the DataChanged to false only updates cells that have been allocated
			// but we could have modified cells that were not allocated using the SetCellValue
			// method. Since setting the cell value would be handled we don't really need to
			// deal with the case where we are setting data changed to false because we are 
			// committing the record.
			//
			this.RefreshCellValues(false, false, false);

			if (isAddReordWithDataItem)
			{
				// SSP 2/11/09 TFS12467
				// Dirtying parent records's scroll count won't help if the this record's parent 
				// viewable record collection's scroll count is not re-calculated properly. So 
				// dirty that instead by dirtying the scroll count of this record which will 
				// cause its parent viewable record collection to recalculate its scroll count 
				// which in turn will dirty the parent record's scroll count.
				// 
				// ------------------------------------------------------------------------------
				this.DirtyScrollCount( );
				//if (this.ParentRecord != null)
				//	this.ParentRecord.DirtyScrollCount();
				// ------------------------------------------------------------------------------

				// SSP 7/22/11 TFS77482
				// If the template add-record was canceled then activate the new template 
				// add-record that gets created in its place.
				// 
				// --------------------------------------------------------------------------
				DataPresenterBase dp = this.DataPresenter;
				if ( isAddReordWithDataItem && wasActive && null != rm 
					&& ( null == dp.ActiveRecordInternal || dp.ActiveRecordInternal == this ) )
				{
					DataRecord newAddRecord = rm.CurrentAddRecordInternal;
					if ( null != newAddRecord )
						newAddRecord.IsActive = true;
				}
				// --------------------------------------------------------------------------
			}
		}

				#endregion //CancelEdit	
    
				// AS 4/29/09 NA 2009.2 ClipboardSupport
				// Moved this from the CellValuePresenter's CommitEditValue method.
				//
				#region CoerceNullEditValue
		/// <summary>
		/// Adjusts the specified value to the appropriate null representation for a given field if the <paramref name="editedValue"/> is considered null.
		/// </summary>
		/// <param name="field">Field whose cell value is being evaluated</param>
		/// <param name="editedValue">The cell value to evaluate</param>
		internal void CoerceNullEditValue(Field field, ref object editedValue)
		{
			// SSP 2/20/09 TFS13093
			// Moved the following logic into the new GetDataSourceNullValue and also
			// added logic to handle other cases in that method.
			// 
			// ----------------------------------------------------------------------
			if (null == editedValue || DBNull.Value == editedValue)
			{
				PropertyDescriptor propertyDescriptor = this.GetPropertyDescriptor(field);
				object nullValue;
				if (null != propertyDescriptor && DataBindingUtilities.GetDataSourceNullValue(propertyDescriptor, out nullValue))
				{
					editedValue = nullValue;
				}
				// If field's data type is nullable then use null instead of DBNull.
				// 
				else if (DBNull.Value == editedValue)
				{
					bool isNullable = field.DataType != field.DataTypeUnderlying;
					if (isNullable)
						editedValue = null;
				}
			}
		} 
				#endregion //CoerceNullEditValue

				#region CommitChanges

		// AS 5/5/09 NA 2009.2 ClipboardSupport
		// Added DataErrorInfo out param so we can get the errorInfo in the clipboard handling
		// and raise its own event.
		//
		//internal bool CommitChanges()
		internal bool CommitChanges(out DataErrorInfo errorInfo)
		{
			errorInfo = null;

			// SSP 3/23/11 TFS69903
			// This flag is not used anymore.
			// 
			//this._isCommittingChanges = true;

			try
			{
				if (!this.ParentCollection.ParentRecordManager.CommitRecord(this))
					return false;
			}
			// SSP 8/10/07 BR25635
			// Added the following catch block. The code was actually moved from the CommitRecord.
			// The purpose of the move was to raise the data error after resetting the 
			// _isCommittingChanges flag.
			// 
			catch ( Exception exception )
			{
				// SSP 3/23/11 TFS69903
				// This flag is not used anymore.
				// 
				//this._isCommittingChanges = false;
				
				
				
				// AS 5/5/09 NA 2009.2 ClipboardSupport
				//this.DataPresenter.RaiseDataError( this, null, exception, DataErrorOperation.RecordUpdate, "DataError_UpdateRecord" );
				
				
				
				
				errorInfo = new DataErrorInfo( this, null, exception, DataErrorOperation.RecordUpdate, DataPresenterBase.GetString( "DataError_UpdateRecord" ) );

				return false;
			}
			finally
			{
				// SSP 3/23/11 TFS69903
				// This flag is not used anymore.
				// 
				//this._isCommittingChanges = false;
			}

			this._beginEditCalled = false;

			bool isAddReordWithDataItem = this._isAddRecord && this._dataItem != null;

			if (isAddReordWithDataItem)
			{
				// reset the IsAddRecord property since this is now a bona fide record
				this._isAddRecord = false;

				// SSP 4/30/08 BR32427
				// Moved this here from below.
				// 
				// let the record manager know the add record edit was committed
				this.ParentCollection.ParentRecordManager.PrepareForNewCurrentAddRecord();

				// now that we have a bona fide record we need to initialize the DataSource
				// properties of any child expandable field records that were created
				if (this.HasExpandableFieldRecords)
				{
					foreach (ExpandableFieldRecord child in this.ChildRecords)
					{
						// JJD 09/22/11  - TFS84708 - Optimization
						// Use the ChildRecordManagerIfNeeded instead which won't create
						// child rcd managers for leaf records
						//RecordManager rm = child.ChildRecordManager;
						RecordManager rm = child.ChildRecordManagerIfNeeded;

						if (rm != null &&
							 rm.DataSource == null)
						{
							// set its datasource to the cell value of the field
							rm.DataSource = this.GetCellValue(child.Field, true) as IEnumerable;
						}
					}
				}

				this.ParentCollection.ViewableRecords.RecordCollectionSparseArray.NotifyItemScrollCountChanged(this);

				// SSP 2/11/09 TFS12467
				// Dirtying parent records's scroll count won't help if the this record's parent 
				// viewable record collection's scroll count is not re-calculated properly. So 
				// dirty that instead by dirtying the scroll count of this record which will 
				// cause its parent viewable record collection to recalculate its scroll count 
				// which in turn will dirty the parent record's scroll count.
				// 
				// ------------------------------------------------------------------------------
				//if (this.ParentRecord != null)
				//	this.ParentRecord.DirtyScrollCount();
				this.DirtyScrollCount( );
				// ------------------------------------------------------------------------------

				// SSP 4/30/08 BR32427
				// Moved this up above where we set reset the _isAddRecord flag.
				// 
				// let the record manager know the add record edit was committed
				//this.ParentCollection.ParentRecordManager.PrepareForNewCurrentAddRecord();

				this.RaisePropertyChangedEvent("IsAddRecord");
				this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");

                // JJD 12/17/09 - TFS25407
                // Since the record was just changed from an add record to a normal record
                // we need to raise a notification that the IsSpecialRecord property has
                // changed and will now return false
				this.RaisePropertyChangedEvent("IsSpecialRecord");
			}

			this.InternalSetDataChanged(false);

            // JJD 12/08/08
            // Added support for IsSynchronizedWithCurrtentItem
            this.ParentCollection.ParentRecordManager.SetCurrentItemFromActiveRecord();

            // JJD 12/2/06 - TFS6743/BR35763
            // If the addrecord has a null dataitem then return false.
            // This can happen when the committed record is filtered out by the
            // data source and is deleted during the commit.
            if (this._isAddRecord && this._dataItem == null)
                return false;

			// SSP 12/17/08 - NAS9.1 Record Filtering
			// When a record is committed, re-evaluate filters on it if its data has changed.
			// 
			DataPresenterBase dp = this.DataPresenter;
			if ( null != dp )
			{
				dp.ProcessRecordWithPendingFilterHelper( this );

				// SSP 4/23/12 TFS107881
				// 
				if ( isAddReordWithDataItem )
					GridUtilities.NotifyCalcAdapter( dp, this, "IsAddRecord", null );
			}

			return true;
		}

				#endregion //CommitChanges

				#region CreateCellCollection

		// SSP 12/16/08 - NAS9.1 Record Filtering
		// Added CreateCellCollection method. FilterRecord overrides it to create FilterCellCollection.
		// 
		/// <summary>
		/// Creates a new CellCollection to be used by the <see cref="Cells"/> property of this DataRecord.
		/// </summary>
		/// <returns>A new CellCollection instance.</returns>
		internal virtual CellCollection CreateCellCollection( )
		{
			return new CellCollection( this );
		}

				#endregion // CreateCellCollection

				#region DeleteHelper






		internal void DeleteHelper()
		{
			if (null == this.ParentCollection)
				return;

			this.ParentCollection.ParentRecordManager.DeleteRecord(this);
		}

				#endregion //DeleteHelper	

				#region GetCellClickActionResolved

		// SSP 9/9/09 TFS19158
		// Added GetCellClickActionResolved virtual method on the DataRecord.
		// 
		/// <summary>
		/// Returns the resolved cell click action for the cell associated with
		/// this record and the specified field.
		/// </summary>
		/// <param name="field">Returned cell click action is for the cell of this field.</param>
		/// <returns>A resolved CellClickAction value.</returns>
		internal virtual CellClickAction GetCellClickActionResolved( Field field )
		{
			return field.CellClickActionResolved;
		}

				#endregion // GetCellClickActionResolved

				// AS 5/1/09
				// I took part of the GetIsEditingAllowed from the Field/Record and 
				// moved it into this helper method.
				//
				#region GetEditAsTypeResolved
		internal virtual Type GetEditAsTypeResolved(Field field)
		{
			if (null == field)
				return null;

			Cell cell = this.GetCellIfAllocated(field);

			if (null != cell)
				return cell.EditAsTypeResolved;

			return field.EditAsTypeResolved;
		}
				#endregion //GetEditAsTypeResolved 

				// AS 5/1/09
				// I took part of the GetIsEditingAllowed from the Field/Record and 
				// moved it into this helper method.
				//
				#region GetEditorTypeResolved
		internal virtual Type GetEditorTypeResolved(Field field)
		{
			if (null == field)
				return null;

			Cell cell = this.GetCellIfAllocated(field);

			if (null != cell)
				return cell.EditorTypeResolved;

			return field.EditorTypeResolved;
		}
				#endregion //GetEditorTypeResolved 

				#region GetFieldForPropertyDescriptor

		// SSP 3/10/10 TFS26510
		// 
		internal Field GetFieldForPropertyDescriptor( string propertyDescriptorName )
		{
			FieldLayout fl = this.FieldLayout;
			FieldCollection fields = GridUtilities.GetFields( fl );
			if ( null != fields )
			{
				int fldIndex = fields.IndexOf( propertyDescriptorName );

				if ( fldIndex >= 0 )
					return fields[fldIndex];

				return FieldLayout.LayoutPropertyDescriptorProvider.GetUnboundFieldWithPropertyDescriptor( this, propertyDescriptorName );
			}

			return null;
		}

				#endregion // GetFieldForPropertyDescriptor

				// AS 5/1/09
				// I noticed this while implementing clipboard support. Essentially disabling
				// AllowEdit would disable filtering for the field. We really want to 
				#region GetIsEditingAllowed
		internal virtual bool GetIsEditingAllowed(Field field)
		{
			if (null == field)
				return false;

			if (field.AllowEditResolved == false)
				return false;

			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

			Type editorType = this.GetEditorTypeResolved(field);

			if (null == editorType)
				return false;

			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// AS 5/1/09
			// While the this presenter is likely disabled, we should still be checking
			// to see if its disabled for any callers of field method.
			// 
			if (!this.IsEnabledResolved)
				return false;

			// JJD 3/13/07
			// If the property descriptor representsa DataColumn that has an expression we need 
			// to return false to prevent editing
			#region Check for DataColumn with expression

			FieldLayout.LayoutPropertyDescriptorProvider provider = this.PropertyDescriptorProvider;

			// AS 6/24/09 NA 2009.2 Field Sizing
			//Debug.Assert(provider != null || (field.Owner != null && field.Owner.TemplateDataRecord == this));
			Debug.Assert(provider != null || this.IsTemplateDataRecord);

			FieldLayout.FieldDescriptor fieldDescriptor = null;

			if (provider != null)
			{
				fieldDescriptor = provider.GetFieldDescriptor(field);

				if (fieldDescriptor != null)
				{
					
					// If the property descriptor indicates that the field is read-only then
					// return false.
					PropertyDescriptor propertyDescriptor = fieldDescriptor.PropertyDescriptor;
					if (null != propertyDescriptor)
					{
						if (propertyDescriptor is ValuePropertyDescriptor)
						{
							ValuePropertyDescriptor vpd = propertyDescriptor as ValuePropertyDescriptor;

							// JJD 05/08/12 - TFS110865
							// Instead of checking IsReadOnlyList use the appropriate property on
							// the parent RecordManager (i.e. DataSourceAllowsAddNew or DataSourceAllowsEdit)
							// to determine if editing is allowed for known types.
							// Note: these properties will now return false for known types if the
							// underlying data source doesn't support IList or its IList.IsReadOnly returns true;
							//if (vpd.IsReadOnlyList(this.ParentCollection.ParentRecordManager.List))
							RecordManager rm = this.ParentCollection.ParentRecordManager;

							if (this.IsAddRecordTemplate || this.IsAddRecord)
							{
								if (!rm.DataSourceAllowsAddNew)
									return false;
							}
							else
							{
								if (!rm.DataSourceAllowsEdit)
									return false;
							}
						}
						else if (propertyDescriptor.IsReadOnly)
							return false;
					}

					DataColumn column = fieldDescriptor.DataColumn;

					if (column != null &&
						 column.Expression != null &&
						 column.Expression.Length > 0)
						return false;
				}
			}

			#endregion //Check for DataColumn with expression

			if (this.IsAddRecord)
				return true;

			return this.ParentCollection.ParentRecordManager.DataSourceAllowsEdit;
		}

				#endregion //GetIsEditingAllowed
        
                #region GetObjectForRecordComparision



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static object GetObjectForRecordComparision(object listObject)
        {
			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

			return DataBindingUtilities.GetObjectForComparison(listObject);
        }

                #endregion // GetObjectForRecordComparision

				#region GetPropertyDescriptor

		// SSP 2/20/09 TFS13093
		// Added GetPropertyDescriptor method.
		// 
		internal PropertyDescriptor GetPropertyDescriptor( Field field )
		{
			FieldLayout.LayoutPropertyDescriptorProvider provider = this.PropertyDescriptorProvider;

			if ( null != provider )
			{
				FieldLayout.FieldDescriptor fieldDescriptor = provider.GetFieldDescriptor( field );

				if ( null != fieldDescriptor )
					return fieldDescriptor.PropertyDescriptor;
			}

			return null;
		}

				#endregion // GetPropertyDescriptor

				#region InitializeDataItem

		internal void InitializeDataItem(object dataItem, int dataItemIndex)
		{
			this._dataItemIndex = dataItemIndex;
			this.SetDataItemHelper(dataItem);
		}

				#endregion //InitializeDataItem	
    
				#region InternalSetDataChanged







		internal void InternalSetDataChanged(bool value)
		{
			if (this._isDataChangedSinceLastCommitAttempt != value)
				this._isDataChangedSinceLastCommitAttempt = value;

			if (this._isDataChanged != value)
			{
				this._isDataChanged = value;

				// if the flag is being set fals then loop over the allocated cells and 
				// reset their flags as well
				if ( value == false && this._cells != null)
				{
					int count = this._cells.Count;

					for (int i = 0; i < count; i++)
					{
						// AS 5/11/09 TFS17514
						// Optimization - get the cell once.
						//
						//if (this._cells.IsCellAllocated(i))
						//	this._cells[i].InternalSetDataChanged(false);
						Cell cell = this._cells.GetCellIfAllocated(i);

						if (null != cell)
							cell.InternalSetDataChanged(false);
					}
				}

				this.RaisePropertyChangedEvent("IsDataChanged");
			}
		}

				#endregion //InternalSetDataChanged

				#region InternalSetIsDeleted

		internal void InternalSetIsDeleted()
		{
			this._isDeleted = true;
		}

				#endregion //InternalSetIsDeleted

				#region IsDataChangedSinceLastCommitAttempt

		internal bool IsDataChangedSinceLastCommitAttempt 
		{ 
			get 
			{
				return this._isDataChangedSinceLastCommitAttempt;
			} 
			set 
			{
				this._isDataChangedSinceLastCommitAttempt = value;
			} 
		}

				#endregion //IsDataChangedSinceLastCommitAttempt	

				#region IsDataItemEqual

		internal bool IsDataItemEqual(object dataItem)
		{
			return this.DataItemForComparison == DataRecord.GetObjectForRecordComparision(dataItem);
		}

				#endregion //IsDataItemEqual

                // AS 2/27/09 TFS14730/Optimization
                #region IsFirstDisplayedCell
        internal bool IsFirstDisplayedCell(Cell cell)
        {
            return null != cell &&      // a cell was specified
                null != _cells &&       // we've allocated any cells
                cell.Record == this &&  // its in this record
                cell.Field == this.FirstDisplayedField; // they have the same field
        } 
                #endregion //IsFirstDisplayedCell

                // AS 2/27/09 TFS14730/Optimization
                #region IsLastDisplayedCell
        internal bool IsLastDisplayedCell(Cell cell)
        {
            return null != cell &&      // a cell was specified
                null != _cells &&       // we've allocated any cells
                cell.Record == this &&  // its in this record
                cell.Field == this.LastDisplayedField; // they have the same field
        } 
                #endregion //IsLastDisplayedCell

				#region OnEditValueChanged

		// AS 5/7/09 NA 2009.2 ClipboardSupport
		// Added boolean return parameter. When the clipboard action invokes this we want to 
		// know if the add record addition was successful.
		// AS 5/15/09 NA 2009.2 ClipboardSupport
		// Added addToUndo parameter.
		//
		internal bool OnEditValueChanged(bool addToUndo)
		{
			return OnEditValueChanged(addToUndo, 0);
		}

		// AS 5/7/09 NA 2009.2 ClipboardSupport
		// Added an overload that takes an offset so that when we insert multiple new records
		// during a paste operation we can control the position of that record so that the 
		// order from the paste is maintained.
		//
		// AS 5/15/09 NA 2009.2 ClipboardSupport
		// Added addToUndo parameter.
		//
		internal bool OnEditValueChanged(bool addToUndo, int recordOnTopOffset)
		{
			if (this._isDataChanged == false)
			{
				// SSP 3/24/10 TFS26271
				// Call to EndEditMode below can cause us to get back in here recursively because 
				// the EndEditMode will raise OnValueChanged, which calls this method, when reverting 
				// back to original value. Enclosed the existing code in try-finally block and added
				// anti-recursion logic.
				// 
				if ( !GridUtilities.Antirecursion.Enter( this, "OnEditValueChanged", true ) )
					return false;

				try
				{
					bool recordAdded = false;

					// on the first change call AddNew on the binding list
					if ( this.IsAddRecord && this._dataItem == null )
					{
						// raise the before record add event
						RecordAddingEventArgs args = new RecordAddingEventArgs( this );

						DataPresenterBase dp = this.DataPresenter;
						dp.RaiseRecordAdding( args );

						// if cancelled then exit
						if ( args.Cancel == true )
						{
							Cell cell = dp.ActiveCell;

							// since the record adding event was cancelled we
							// need to force out of edit mode and lose the changes
							if ( cell != null &&
								 cell.Record == this &&
								 cell.IsInEditMode )
								cell.EndEditMode( false, true );

							// AS 5/7/09 NA 2009.2 ClipboardSupport
							// Return false to indicate the edit was not successful.
							//
							return false;
						}

						// get the record manager
						RecordManager manager = this.ParentCollection.ParentRecordManager;

						// SSP 3/3/09 TFS11407
						// Added code to raise InitializeRecord event whenever a cell value changes.
						// Enclosed the existing code into try-finally block and added 
						// Suspend/ResumeInitializeRecordFor calls as well as the call to 
						// FireInitializeRecord in the finally block.
						// 
						bool initializeRecordAlreadySuspended = dp.SuspendInitializeRecordFor( this );
						try
						{
							// call add new to get a data item from the IBindingList
							// AS 5/7/09 NA 2009.2 ClipboardSupport
							//this.SetDataItemHelper( manager.AddNew( ) );
							this.SetDataItemHelper( manager.AddNew( recordOnTopOffset ) );

							// initialiize the cell values with the cached values
							manager.InitializeAddRecordCellValuesFromCache( this );

							// AS 5/15/09 NA 2009.2 ClipboardSupport
							// If the user just starts typing in a cell in the add record
							// then we want to be able to delete that record when they undo. I 
							// could have tried to break this up such that we don't add the 
							// undo action until it returns but that leaves open the possibility 
							// (or a greater possibility) that other stuff could be put into 
							// the undo stack ahead of the delete record action.
							//
							if ( addToUndo && dp.IsUndoEnabled )
								dp.History.AddUndoActionInternal( new DeleteRecordsAction( dp, new Record[] { this }, false ) );

							// JJD 5/27/08 - BR32640
							// Refresh the cell values to pick up default values
							// SSP 3/3/09 TFS11407
							// Added raiseInitializeRecord parameter. This particular change shouldn't change
							// the behavior.
							// 
							//this.RefreshCellValues( );
							this.RefreshCellValues( true, false );

							// sync the version numbers
							this._underlyingDataVersion = manager.UnderlyingDataVersion;
						}
						finally
						{
							if ( !initializeRecordAlreadySuspended )
							{
								dp.ResumeInitializeRecordFor( this );
								// JJD 11/17/11 - TFS78651 
								// Added sortValueChanged parameter
								//this.FireInitializeRecord(false);
								this.FireInitializeRecord(false, false);
							}
						}

						// set the flag so we know to raise the after event below
						recordAdded = true;
					}

					this.InternalSetDataChanged( true );

					// raise the after record add event
					if ( recordAdded == true )
					{
						// dirty the parent record's scroll count
						// SSP 2/11/09 TFS12467
						// Dirtying parent records's scroll count won't help if the this record's parent 
						// viewable record collection's scroll count is not re-calculated properly. So 
						// dirty that instead by dirtying the scroll count of this record which will 
						// cause its parent viewable record collection to recalculate its scroll count 
						// which in turn will dirty the parent record's scroll count.
						// 
						// ------------------------------------------------------------------------------
						//if (this.ParentRecord != null)
						//	this.ParentRecord.DirtyScrollCount();
						this.DirtyScrollCount( );
						// ------------------------------------------------------------------------------

						this.DataPresenter.RaiseRecordAdded( new RecordAddedEventArgs( this ) );
					}
				}
				finally
				{
					// SSP 3/24/10 TFS26271
					// 
					GridUtilities.Antirecursion.Exit( this, "OnEditValueChanged" );
				}
			}

			return true;
		}

				#endregion //OnEditValueChanged	

				#region OnIsNestedDataDisplayEnabledChanged

		internal void OnIsNestedDataDisplayEnabledChanged()
		{
			if (this.IsExpanded == true &&
				 this.DataPresenter.IsNestedDataDisplayEnabled == false)
				this.IsExpanded = false;

			this.DirtyScrollCount();
			this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");
			this.RaisePropertyChangedEvent("IsExpanded");
		}

				#endregion //OnIsNestedDataDisplayEnabledChanged	

				#region ProcessPendingChanges

		internal void ProcessPendingChanges(bool discardChanges, bool recursive)
		{
			// if BeginEdit method was called without a subsequent commit or cancel
			// then call one of those methods now
			if (this._beginEditCalled == true ||
				(this._isAddRecord && this._dataItem != null ))
			{
				if (discardChanges == true)
					this.CancelUpdate();
				else
					this.Update();
			}

			if (recursive == false || 
				this.HasExpandableFieldRecords == false)
				return;

			// since the recursive flag is true call the UpdateRecordsWithPendingChanges
			// on each child record manager
			foreach (ExpandableFieldRecord efr in this.ChildRecords.SparseArray.NonNullItems)
			{
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use the ChildRecordManagerIfNeeded instead which won't create
				// child rcd managers for leaf records
				//RecordManager rm = efr.ChildRecordManager;
				RecordManager rm = efr.ChildRecordManagerIfNeeded;

				if (rm != null)
					rm.UpdateRecordsWithPendingChanges(discardChanges, true);
			}
		}

				#endregion //ProcessPendingChanges	
 
				// JJD 5/9/07 - BR22698
				// Added support for listening to data item's PropertyChanged event
				// if the datasource doesn't supply cell value change notifications thru IBindlingList
				#region RefreshCellValue

		// Centralized logic for refreshing a cell's value on a change notification
		// SSP 3/3/09 TFS11407
		// Added raiseInitializeRecord parameter.
		// 
		//internal void RefreshCellValue(string fieldName)
		
		
		
		
		//internal void RefreshCellValue( string fieldName, bool raiseInitializeRecord )
		// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
		//internal void RefreshCellValue(string propertyDescriptorName, bool raiseInitializeRecord)
		internal void RefreshCellValue(string propertyDescriptorName, bool raiseInitializeRecord, bool isRecordPresenterDeactivated)
		{
			// JJD 4/27/11 - TFS73888 - added
			// Moved logic into overload that takes a field
			//FieldLayout fl = this.FieldLayout;

			//if (fl == null)
			//    return;

			// SSP 3/10/10 TFS26510
			// 
			// --------------------------------------------------------------------------
			Field field = this.GetFieldForPropertyDescriptor(propertyDescriptorName);
			if (null == field)
				return;

			// JJD 4/27/11 - TFS73888 
			// Moved logic into overload that takes a field
			// JJD 04/12/12 - TFS108549 - Optimization - pass along isRecordPresenterDeactivated parameter
			//this.RefreshCellValue(field, raiseInitializeRecord);
			this.RefreshCellValue(field, raiseInitializeRecord, isRecordPresenterDeactivated);
		}

		// JJD 4/27/11 - TFS73888 - added overload that takes a Field
		// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
		//internal void RefreshCellValue(Field field, bool raiseInitializeRecord)
		internal void RefreshCellValue(Field field, bool raiseInitializeRecord, bool isRecordPresenterDeactivated)
		{
			FieldLayout fl = this.FieldLayout;

			if (fl == null)
				return;
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			// JJD 11/8/11 - TFS95675
			// For UnboundFields get the corresponding UnboundCell and 
			// call RefreshCellValue which will call UpdateTarget on any binding that was applied
			if (field.IsUnbound)
			{
				UnboundCell ucell = this.GetCellIfAllocated(field) as UnboundCell;

				if (ucell != null)
					ucell.RefreshCellValue();
			}
			// --------------------------------------------------------------------------
			
			// SSP 3/31/08 - Summaries Functionality
			// Call OnDataChanged so any functionalities affected by data value get dirtied,
			// like summaries functionality.
			// 
			RecordManager recordManager = this.RecordManager;
			if ( null != recordManager )
				recordManager.OnDataChanged( DataChangeType.CellDataChange, this, field );

			CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(this, field);

            // JJD 5/1/08 - BR31742
            // Added stack variable to hold the cell value
            object cellValue = null;

			// synchronize the cell value presenter's value property
            if (cvp != null)
            {
                // JJD 5/1/08 - BR31742
                // cache the value on the stack variable
                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //cellValue = field.GetCellValue(this, true);
                cellValue = this.GetCellValue(field, CellValueType.EditAsType);

				// JJD 04/12/12 - TFS108549 - Optimization
				// If the record presenter is deactivated (i.e. unused)
				// then we want to just mark it dirty. When the cvp becomes visible again we will
				// get the value
				if (isRecordPresenterDeactivated)
					cvp.MarkValueDirty();
				else
				{
					// SSP 1/21/09 TFS12327
					// See notes on CellValuePresenter.CVPValueWrapper class for more info.
					// 
					//cvp.Value = cellValue;
					cvp.SetValueInternal(cellValue, false);
				}
            }

            // JJD 3/7/08
            // For normal field's, i.e. not unbound, if the cell has been allocated then raise its property change notifications 
            
            // JJD 5/1/08 - BR31742
            // Added logic to update child ExpandableFieldRecord DataSources
            //if (!field.IsUnbound && field != this._ignoreChangeNotificationsForField)
            if (field != this._ignoreChangeNotificationsForField)
            {
                // JJD 6/26/08 - BR33656
                // Moved logic into helper method so that it could be also called
                // when an UnboundCell's value changed
                this.RefreshCellValueHelper(field, cellValue);

				// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
				// 
				// SSP 6/28/10 TFS23257
				// 
				//this.RefreshDataErrorInfo( field, true );
				// SSP 4/28/11 TFS73511
				// Decided to raise the record error change notifications as well because there's no specific
				// notification mechanism that a data item could notify that the row error changed and therefore
				// whenever a field value changes, we should requry the row error in case it changed. Pass true
				// for the raiseRecordError parameter.
				// 
				//this.RefreshDataErrorInfo( field, false );
				this.RefreshDataErrorInfo( field, false, true );
            }

			// SSP 3/3/09 TFS11407
			// Added raiseInitializeRecord parameter.
			// 
			if (raiseInitializeRecord)
			{
				// JJD 11/17/11 - TFS78651 
				// Added sortValueChanged parameter
				//this.FireInitializeRecord(true);
				this.FireInitializeRecord(true, field.SortStatus != SortStatus.NotSorted);
			}
		}

				#endregion //RefreshCellValue	
 
                // JJD 6/26/08 - BR33656 - added
                #region RefreshCellValueHelper

        // JJD 6/26/08 - BR33656
        // Added helper method so that it could be called from RefreshCellValue and
        // when an UnboundCell's value changed
        internal void RefreshCellValueHelper(Field field, object cellValue)
        {
            // JJD 5/1/08 - BR31742
            // If the Field is expandable and contains a child list then update its
            // child RecordManager's DataSource with the new value
			// JJD 7/15/10 - TFS35815 - Optimization - don't allocate the child records collection
			//if (field.IsExpandableByDefault && field.IsExpandableResolved)
			// JJD 09/22/11  - TFS84708 - Optimization
			// Added else block if ChildRecordsIfAllocated returns null
			//if (field.IsExpandableByDefault && field.IsExpandableResolved && this.ChildRecordsIfAllocated != null)
            if (field.IsExpandableByDefault && field.IsExpandableResolved )
            {
				if (this.ChildRecordsIfAllocated != null)
				{
					ExpandableFieldRecordCollection children = this.ChildRecords;

					int index = children.IndexOf(field);

					Debug.Assert(index >= 0, "ExpandableFieldRecord is not in collection.");

					if (index >= 0)
					{
						ExpandableFieldRecord child = children[index];

						// JJD 7/15/10 - TFS35815 - Optimization - don't allocate the child record manager
						//RecordManager rm = child.ChildRecordManager;
						RecordManager rm = child.ChildRecordManagerIfAllocated;

						if (rm != null)
						{
							if (cellValue == null)
								cellValue = this.GetCellValue(child.Field, true);

							Debug.Assert(cellValue == null || cellValue is IEnumerable, "Invalid cell value for an IEnumerable field");

							// JJD 5/1/08 - BR31742
							// update the datasource with the cell value of the field
							rm.DataSource = cellValue as IEnumerable;
						}
						else
						{
							// JJD 09/22/11  - TFS84708 - Optimization
							// Let the expandable field rcd know that its underlying data has changed
							child.OnDataChanged();
						}
					}
				}
				else
				{
					// JJD 09/22/11  - TFS84708 - Optimization
					// If we have created trackers for enumerable fields then clear then now
					this.OnEnumerableCellDataChanged();
				}
            }
			else //if (!field.IsUnbound)	// JM 08-18-09 TFS20916 - Move this check below so we can support DataValueChangedNotifications for Unbound Fields.
			{
				if (!field.IsUnbound)
				{
					Cell cell = this.GetCellIfAllocated(field);

					if (cell != null)
						cell.RaiseValueChangedNotifications();
				}

				// JM 6/17/09 NA 2009.2 DataValueChangedEvent
				this.DataValueChangedHelper(field);
			}
        }

                #endregion //RefreshCellValueHelper	
    
				#region RefreshCellValues

		
		
		
		
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


		// JM 08-29-08 BR35908
		//internal void RefreshCellValues()
		// SSP 3/3/09 TFS11407
		// Added raiseInitializeRecord parameter.
		// 
		//internal void RefreshCellValues(bool excludeUnboundFields)
		internal void RefreshCellValues( bool excludeUnboundFields, bool raiseInitializeRecord )
		{
			// AS 5/11/09 TFS17514
			RefreshCellValues(excludeUnboundFields, raiseInitializeRecord, true);
		}

		// JJD 4/27/11 - TFS73888 - added
		internal void RefreshCellValues(List<Field> fields, bool raiseInitializeRecord)
		{
			// JJD 11/17/11 - TFS78651 
			bool sortValueChanged = false;

			// JJD 04/12/12 - TFS108549 - Optimization 
			// Determine if presenter is deactivatewd
			RecordPresenter rp = this.AssociatedRecordPresenter;
			bool isRecordPresenterDeactivated = rp != null && rp.IsDeactivated;

			foreach (Field field in fields)
			{
				// JJD 11/17/11 - TFS78651 
				sortValueChanged = sortValueChanged || field.SortStatus != SortStatus.NotSorted;

				// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
				//this.RefreshCellValue(field, false);
				this.RefreshCellValue(field, false, isRecordPresenterDeactivated);
			}

			if ( raiseInitializeRecord )
			{
				// JJD 11/17/11 - TFS78651 
				// Added sortValueChanged parameter
				//this.FireInitializeRecord(true);
				this.FireInitializeRecord(true, sortValueChanged);
			}
		}

		// AS 5/11/09 TFS17514
		// Added an overload because we want to be able to avoid regetting the child list since 
		// at least in a datatable that would result in a new list being provided which wouldn't
		// have changed while the record was being updated.
		//
		internal void RefreshCellValues( bool excludeUnboundFields, bool raiseInitializeRecord, bool reinitializeExpandableFields )
		{
			this.RefreshCellValues( excludeUnboundFields, raiseInitializeRecord, reinitializeExpandableFields, false );
		}

		// SSP 2/2/10 TFS62391
		// Added an overload that takes fromDataSourceReset parameter.
		// 
		internal void RefreshCellValues( bool excludeUnboundFields, bool raiseInitializeRecord, bool reinitializeExpandableFields, bool fromDataSourceReset )
		{
			// JJD 04/12/12 - TFS108549 - Optimization 
			// Determine if the associated presenter is deactivated (i.e. usused).
			//bool hasRecordPresenter = this.AssociatedRecordPresenter != null;
			RecordPresenter rp = this.AssociatedRecordPresenter;
			bool isRecordPresenterDeactivated = rp != null && rp.IsDeactivated;

			bool hasRecordPresenter = rp != null;
			bool expandableFieldsFound = false;
			// JJD 11/17/11 - TFS78651 
			bool sortValueChanged = false;

			// JM 10-10-11 TFS91236 - Backed out the change for TFS85685 in this.DataValueChangedHelper and added this code
			// as well as a check for shouldRaiseDataValueChanged below where we call this.DataValueChangedHelper. 
			DataPresenterBase dp = this.DataPresenter;
			bool shouldRaiseDataValueChanged = dp == null ? false : this != dp._recordWhoseChangesAreBeingDiscarded;

			foreach (Field field in this.FieldLayout.Fields)
			{

                // bypass fields that aren't visible 
				if (field.VisibilityResolved == Visibility.Collapsed)
					continue;

				// bypass expandable fields
				if (field.IsExpandableResolved == true)
				{
					if ( field.IsExpandableByDefault )
						expandableFieldsFound = true;
					continue;
				}

				// JM 08-29-08 BR35908 - Only bypass the field if it is an unbound field and
				//						 the new parameter to this method is set to true.
                // JJD 1/28/08 - Optimization
                // bypass unbound fields since the unboundcells use databinding to synchronize 
                // their values
                //if (field is UnboundField)
				if (field is UnboundField )
				{
					// SSP 2/2/10 TFS62391
					// Added an overload that takes fromDataSourceReset parameter.
					// 
					if ( fromDataSourceReset || excludeUnboundFields )
					{
						// JM 11-12-08 TFS10196 - Need to refresh the bindings on the Unbound cell to pick up data values in the event that the data record 
						// was set after the bindings were set.  This can happen in an AddRow containing unbound fields.
						UnboundCell unboundCell = this.GetCellIfAllocated( field ) as UnboundCell;
						if ( unboundCell != null )
						{
							unboundCell.RefreshBindings( );
							// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
							//CellValuePresenter.SyncValueWithCellValue(this, field);
							CellValuePresenter.SyncValueWithCellValue(this, field, isRecordPresenterDeactivated);
						}
					}

					if ( excludeUnboundFields )
						continue;
				}

                // JJD 3/7/08
                // If the cell has been allocated then raise its property change notifications 
                if (field != this._ignoreChangeNotificationsForField)
                {
                    Cell cell = this.GetCellIfAllocated(field);

                    if (cell != null)
                        cell.RaiseValueChangedNotifications();
                }

				if (hasRecordPresenter)
				{
					// JM 11-12-08 TFS10196 - Moved the following code into a static method on CellValuePresenter called SyncValueWithCellValue
					// and replaced it with a call to that method.  I did this because I needed to call this code from above (approx 15 lines up)
					//// get the cellvaluepresenter for the field
					//CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(this, field);

					//// synchronize the cell value presenter's value property
					//// JJD 5/27/08 - BR32640
					//// Bypass updating the cell value if the cell is in edit mode and
					//// the user has changed the value
					//// if (cvp != null)
					//if (cvp != null && (!cvp.IsInEditMode || !cvp.Editor.HasValueChanged))
					//    cvp.Value = field.GetCellValue(this, true);
					// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
					//CellValuePresenter.SyncValueWithCellValue(this, field);
					CellValuePresenter.SyncValueWithCellValue(this, field, isRecordPresenterDeactivated);
				}

				// JM 6/17/09 NA 2009.2 DataValueChangedEvent
				//if (field is UnboundField == false)	// JM 08-18-09 TFS20916 - Support Unbound Fields too
				if (shouldRaiseDataValueChanged)	// JM 10-10-11 TFS91236
					this.DataValueChangedHelper(field);

				// JJD 11/17/11 - TFS78651 
				sortValueChanged = sortValueChanged || field.SortStatus != SortStatus.NotSorted;
			}

			if (expandableFieldsFound &&
				reinitializeExpandableFields && // AS 5/11/09 TFS17514
				this.ChildRecordsIfAllocated != null && // JJD 7/15/10 - TFS35815 - Optimization - don't allocate the child records collection
				 this.DataPresenter.IsNestedDataDisplayEnabled)
			{
				foreach (ExpandableFieldRecord child in this.ChildRecords )
				{
					// JJD 7/15/10 - TFS35815 - Optimization
					// Don't allocate the child record manager
					//RecordManager rm = child.ChildRecordManager;
					RecordManager rm = child.ChildRecordManagerIfAllocated;

					// update the data source
					if (rm != null)
					{
						// JJD 7/15/10 - TFS35815 - Optimization
						// Check the new NonSpecificNotificationBehaviorResolved 
						// and bypass refreshing the child datasource based on
						// the setting
						switch (child.Field.NonSpecificNotificationBehaviorResolved)
						{
							case NonSpecificNotificationBehavior.RefreshValue:
								break;
							case NonSpecificNotificationBehavior.BypassIfEnumerable:
								continue;
							case NonSpecificNotificationBehavior.BypassIfBindingList:
								if (rm.BindingList != null)
									continue;
								break;
							case NonSpecificNotificationBehavior.BypassIfObservable:
								if (rm.IsObservable)
									continue;
								break;
						}

						// AS 4/15/09 Field.(Get|Set)CellValue
						//rm.DataSource = child.Field.GetCellValue(this, true) as IEnumerable;
						rm.DataSource = this.GetCellValue(child.Field, true) as IEnumerable;
					}
					else
					{
						// JJD 09/22/11  - TFS84708 - Optimization
						// since we haven't allocated a child record manager yet we
						// need to notify the ExpandableFieldRecord that its data has changed
						child.OnDataChanged();
					}
				}
			}
			else
			{
				// JJD 09/22/11  - TFS84708 - Optimization
				// If we have created trackers for enumerable fields then clear then now
				this.OnEnumerableCellDataChanged();
			}

			// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
			// 
			// SSP 6/28/10 TFS23257
			// 
			//this.RefreshDataErrorInfo( null, true );
			this.RefreshDataErrorInfo( null, false );

			// release the record size manager if it was created to maintain
			// the size to content sizes.
			this.ClearSizeToContentManager(null);

			// SSP 3/3/09 TFS11407
			// Added raiseInitializeRecord parameter.
			// 
			if (raiseInitializeRecord)
			{
				// JJD 11/17/11 - TFS78651 
				// Added sortValueChanged parameter
				//this.FireInitializeRecord(true);
				this.FireInitializeRecord(true, sortValueChanged);
			}
		}

				#endregion //RefreshCellValues	

                // JJD 6/17/09 - TFS12228 - added
                #region WasBeginEditCalled

        internal bool WasBeginEditCalled { get { return this._beginEditCalled; } }

                #endregion //WasBeginEditCalled	
       
            #endregion //Internal Methods

            #region Private Methods

				#region ClearChildDataCache

		// JJD 09/22/11  - TFS84708 - Optimization
		private void ClearChildDataCache(bool unwireEvents)
		{
			if (_trackingInfos == null)
				return;

			if (unwireEvents)
			{
				foreach (ExpandableFieldRecord.ChildDataTrackingInfo trInfo in _trackingInfos)
					trInfo.Unwire(this);
			}

			_trackingInfos = null;
		}

				#endregion //ClearChildDataCache	

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region DataValueChangedHelper

		private void DataValueChangedHelper(Field field)
		{
			if (field.DataValueChangedNotificationsActiveResolved == false)
				return;

			// Check the DataValueChangedScope.
			if (field.DataValueChangedScopeResolved == DataValueChangedScope.OnlyRecordsInView && 
				this.AssociatedRecordPresenter		== null)
				return;

			// Get the cell associated with the Field in this record.
			Cell cell = this.Cells[field];

			// JM 10-10-11 TFS91236 - Backing out this change an instead adding code to RefreshCellValues to conditionally call this method
			//// JM 09-14-11 TFS85685
			//if (cell.IsDataChanged == false)
			//    return;
			DataPresenterBase dp = this.DataPresenter;	// JM 10-10-11 TFS91236 - Efficiency.

			cell.AddDataValueChangedHistoryEntryForCurrentValue();
			if (dp != null)	// JM 10-10-11 TFS91236 - Use new stack variable defined above
			{
				DataValueChangedEventArgs args = new DataValueChangedEventArgs(this, field, cell.AssociatedCellValuePresenter, cell.DataValueInfoHistoryCache as IList<DataValueInfo>);
				dp.RaiseDataValueChanged(args);	// JM 10-10-11 TFS91236 - Use new stack variable defined above
			}
		}

				#endregion DataValueChangedHelper

				// JJD 09/22/11  - TFS84708 - Optimization
				#region GetChildRcdCollection

		private ExpandableFieldRecordCollection GetChildRcdCollection(bool alwaysCreateIfEnumerable)
		{
			if (this._expandableFieldRecords == null)
			{
				FieldCollection fields = this.FieldLayout.Fields;

				int enumerableCount = fields.ExpandableFieldsCount;

				// JJD 09/22/11  - TFS84708 - Optimization
				// if there are no enumerable fields then return
				if (enumerableCount < 1 && false == alwaysCreateIfEnumerable)
					return null;

				bool wireEvents = false;
				bool create = alwaysCreateIfEnumerable;

				// JJD 09/22/11  - TFS84708 - Optimization
				// for DataRowView's we always want to create the ExpandableFieldRecordCollection since
				// getting the enumerable value from the data source is so expensive
				if (alwaysCreateIfEnumerable == false && !(this._dataItem is DataRowVersion))
				{
					if (_trackingInfos != null )
					{
						if (_trackingInfos.Length == enumerableCount)
							return null;

						this.ClearChildDataCache(true);
					}

					if (_trackingInfos == null)
					{
						_trackingInfos = new ExpandableFieldRecord.ChildDataTrackingInfo[enumerableCount];
						int processed = 0;
						int count = this.FieldLayout.Fields.Count;

						for (int i = 0; i < count; i++)
						{
							if (fields[i].IsExpandableResolved)
							{
								ExpandableFieldRecord.ChildDataTrackingInfo trInfo = new ExpandableFieldRecord.ChildDataTrackingInfo(this.GetCellValue( fields[i], true) as IEnumerable);

								if (trInfo._childData != null)
								{
									wireEvents = true;
									create = trInfo.Initialize(this.DataPresenter, alwaysCreateIfEnumerable) || create;
								}

								// init the array
								_trackingInfos[processed] = trInfo;

								// bump the index
								processed++;

								// when we reach the total number of enumerable fields we can stop  
								if (processed == enumerableCount)
									break;
							}
						}
					}
				}

				if (create)
				{
					this._expandableFieldRecords = new ExpandableFieldRecordCollection(this, this.ParentCollection.ParentRecordManager, this.FieldLayout);
					
					// since we created a child record manager we can clear any cached child date members
					this.ClearChildDataCache(!wireEvents);
				}
				else 
				{
					if (wireEvents)
					{
						// Listen for collection changed notifications
						foreach (ExpandableFieldRecord.ChildDataTrackingInfo trInfo in _trackingInfos)
							trInfo.Wire(this);
					}
				}

				
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)


				if ( _expandableFieldRecords != null )
					this._expandableFieldRecords.VerifyChildren();
			}

			return this._expandableFieldRecords;
		}

				#endregion //GetChildRcdCollection	
    
				#region GetDataErrorInfo

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Returns the IDataErrorInfo that is to be used for record and cell data error information.
		/// </summary>
		/// <returns></returns>
		private IDataErrorInfo GetDataErrorInfo( )
		{
			return this.DataItem as IDataErrorInfo;
		}

				#endregion // GetDataErrorInfo

				// JJD 09/22/11  - TFS84708 - Optimization
				#region OnEnumerableCellDataChanged

		internal void OnEnumerableCellDataChanged()
		{
			if (_trackingInfos != null)
			{
				// clear the existing cache 
				this.ClearChildDataCache(true);

				// If we are expanded then access the child ViewableRecordCollection
				// so we can update our scroll count
				if (this.IsExpanded)
				{
					ViewableRecordCollection vcr = this.ViewableChildRecordsIfNeeded;

					if (vcr != null)
						this.DirtyScrollCount();
				}
			}
		}

				#endregion //OnEnumerableCellDataChanged	

				#region SetDataItemHelper

        // JJD 11/24/08 - TFS6743/BR35763 - make internal
        //private void SetDataItemHelper(object dataItem)
		internal void SetDataItemHelper(object dataItem)
		{
            // JJD 12/13/07
            // If the old dataitem is an EnumerableObjectWrapper then check
            // if the new dataitem is equal to its underlying data. If so return
            if (this._dataItem is EnumerableObjectWrapper)
            {
                if (((EnumerableObjectWrapper)(this._dataItem)).Items == dataItem)
                    return;

                // If the new dataitem is IEnumerable then wrap it
                if (dataItem is IEnumerable &&
                     !(dataItem is string))
                    dataItem = new EnumerableObjectWrapper(dataItem as IEnumerable);
            }


			if (this._dataItem != dataItem)
			{
				bool hadOldDataItem = this._dataItem != null;

				// JJD 5/9/07 - BR22698
				// Added support for listening to data item's PropertyChanged event
				// if the datasource doesn't supply cell value change notifications thru IBindlingList
				if (hadOldDataItem && this._isHookedToDataItemPropertyChangedEvent)
				{
					// Unhook from the old dataitem's event
					this._isHookedToDataItemPropertyChangedEvent = false;
					PropertyChangedEventManager.RemoveListener((INotifyPropertyChanged)this._dataItem, this, string.Empty);
				}

				this._dataItem = dataItem;

                // JJD 1/26/08 - BR30085
                // Moved logic for hooking the data item's PropertyChanged event
                // into VerifyDataItemNotifyChangeStatus method
                #region Old code

                //// JJD 5/9/07 - BR22698
                //// Added support for listening to data item's PropertyChanged event
                //// if the datasource doesn't supply cell value change notifications thru IBindlingList
                //// See if the new dataitem implements the INotifyPropertyChanged event
                //INotifyPropertyChanged notifyPropertyChanged = this._dataItem as INotifyPropertyChanged;

                //if (notifyPropertyChanged != null)
                //{
                //    RecordCollectionBase parentCollection = this.ParentCollection;
                //    RecordManager rm = parentCollection == null ? null : parentCollection.ParentRecordManager;

                //    Debug.Assert(rm != null);

                //    // Only hook the event if the datasource doesn't ssupport raising cell change events
                //    if (rm != null &&
                //         rm.DataSourceRaisesCellValueChangeEvents == false)
                //    {
                //        this._isHookedToDataItemPropertyChangedEvent = true;
                //        PropertyChangedEventManager.AddListener(notifyPropertyChanged, this, string.Empty);
                //    }
                //}
                #endregion //Old code	
                this.VerifyDataItemNotifyChangeStatus();

				// raise a notify event if the DataItem has changed
				if (hadOldDataItem)
				{
					// JM 08-29-08 BR35908 - Call the new overloa of RefreshCellValues that lets us specify
					//						 whether to exclude unbound fields.  Since the data item has changed
					//						 we do not want to exclude unbound fields so that the binding (if any)
					//						 on the unbound field gets recreated.
					//this.RefreshCellValues();
					// SSP 3/3/09 TFS11407
					// Added raiseInitializeRecord parameter.
					// 
					//this.RefreshCellValues(false);
					this.RefreshCellValues( false, false );

					this.RaisePropertyChangedEvent("DataItem");

					// SSP 3/31/08 - Summaries Functionality
					// Call OnDataChanged so any functionalities affected by data value get dirtied,
					// like summaries functionality.
					// 
					RecordManager recordManager = this.RecordManager;
					if ( null != recordManager )
						recordManager.OnDataChanged( DataChangeType.RecordDataChange, this, null );

					// JJD 6/13/11 - TFS77755
					// If this this the active record then we need to synch up the
					// ActiveDataItem value
					if (this.IsActive)
					{
						// initialize the _dataItemForComparison since that is used inside the ActiveDataItem
						// coercion logic.
						this._dataItemForComparison = DataRecord.GetObjectForRecordComparision(dataItem);

						DataPresenterBase dp = this.DataPresenter;

						if (dp != null)
							dp.ActiveDataItem = dataItem;

						return;
					}
				}
                else
                {
                    // JJD 5/12/08 - BR32619
                    // If this is the add record then raise the PropertyChanged
                    // event for the DataItem property
                    if (this.IsAddRecord)
                        this.RaisePropertyChangedEvent("DataItem");
                }
			}

			this._dataItemForComparison = DataRecord.GetObjectForRecordComparision(dataItem);
		}

				#endregion //SetDataItemHelper

				#region RefreshDataErrorInfo

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		internal void RefreshDataErrorInfo( Field field, bool excludeUnboundFields )
		{
			this.RefreshDataErrorInfo( field, excludeUnboundFields, null );
		}

		// SSP 1/17/10 TFS61443
		// Added an overload of RefreshDataErrorInfo that takes in raiseRecordError parameter.
		// 
		internal void RefreshDataErrorInfo( Field field, bool excludeUnboundFields, bool? raiseRecordError )
		{
			if ( null != field )
			{
				if ( !excludeUnboundFields || !field.IsUnbound )
				{
					Cell cell = this.GetCellIfAllocated( field );
					if ( null != cell )
					{
						cell.RaiseDataErrorNotifications( );
					}
					else
					{
						CellValuePresenter cvp = CellValuePresenter.FromRecordAndField( this, field );
						if ( null != cvp )
							cvp.UpdateDataError( );
					}
				}
			}

			// SSP 1/17/10 TFS61443
			// Commented out the else keyword and instead added the if condition.
			// 
			//else
			//{
			if ( raiseRecordError ?? null == field )
			{
				// SSP 12/22/11 TFS67264 - Optimizations
				// Only raise the events if the SupportDataErrorInfo has been enabled.
				// 
				//if ( this.HasListeners )
				if ( this.HasListeners && this.IsRecordDataErrorInfoSupported )
				{
					this.RaisePropertyChangedEvent( "DataError" );
					this.RaisePropertyChangedEvent( "HasDataError" );
				}

				DataRecordPresenter rp = this.AssociatedRecordPresenter as DataRecordPresenter;
				if ( null != rp )
				{
					// SSP 12/22/11 TFS67264 - Optimizations
					// Only raise the events if the SupportDataErrorInfo has been enabled.
					// 
					//rp.UpdateDataError( );
					if ( this.IsRecordDataErrorInfoSupported )
						rp.UpdateDataError( );

					// SSP 1/17/10 TFS61443
					// Related to the change above. Enclosed existing code in the if block.
					// 
					if ( null == field )
					{
						FieldCollection fields = GridUtilities.GetFields( this.FieldLayout );
						Debug.Assert( null != fields );
						if ( null != fields )
						{
							foreach ( Field ii in fields )
							{
								if ( null != ii )
									this.RefreshDataErrorInfo( ii, excludeUnboundFields );
							}
						}
					}
				}
			}
			//}
		}

				#endregion // RefreshDataErrorInfo

				#region RefreshDataItem

		/// <summary>
		/// Causes the Record to re-get the data item from the list the next time DataItem is accessed.
		/// </summary>
		internal void RefreshDataItem( )
		{
			_underlyingDataVersion--;
		}

				#endregion RefreshDataItem

                // JJD 1/26/08 - BR30085 - added
                #region VerifyDataItemNotifyChangeStatus

        private void VerifyDataItemNotifyChangeStatus()
        {
            bool shouldHookPropertyChangeEvent = false;

            FieldLayout fl = this.FieldLayout;

            // JJD 5/9/07 - BR22698
            // Added support for listening to data item's PropertyChanged event
            // if the datasource doesn't supply cell value change notifications thru IBindlingList
            // See if the new dataitem implements the INotifyPropertyChanged event
            INotifyPropertyChanged notifyPropertyChanged = this._dataItem as INotifyPropertyChanged;

            if (notifyPropertyChanged != null && fl != null
				// SSP 2/25/09 TFS14608
				// Commented out check for AssociatedRecordPresenter. We need to hook into the record
				// regardless of whether it's in view or not because we still need to recalculate
				// any summaries as well as re-evaluate any filters on the record.
				// 
                // && this.AssociatedRecordPresenter != null
				)
            {
                FieldCollection fields = fl.Fields;

                // If we only have unbound fields then we don't need to hook into
                // the event because unbounccells synchronize their values by
                // using a binding
                if (fields.Count > fields.UnboundFieldsCount
					// SSP 3/10/10 TFS26510
					// We have an optimization where instead of maintanining bindings for all unbound cells
					// of a field that has summary, we rely on the property change notification from the
					// data item if the unbound field's binding path points to a property on the data item.
					// 
					|| FieldLayout.LayoutPropertyDescriptorProvider.HasUnboundFieldWithPropertyDescriptor( this )
					)
                {
                    RecordCollectionBase parentCollection = this.ParentCollection;
                    RecordManager rm = parentCollection == null ? null : parentCollection.ParentRecordManager;

                    Debug.Assert(rm != null);

                    // Only hook the event if the datasource doesn't support raising cell change events
                    if (rm != null &&
                         rm.DataSourceRaisesCellValueChangeEvents == false)
                        shouldHookPropertyChangeEvent = true;
                }
            }


            // if the hook state hasn't changed then we can just return
            if (shouldHookPropertyChangeEvent == this._isHookedToDataItemPropertyChangedEvent)
                return;

            this._isHookedToDataItemPropertyChangedEvent = shouldHookPropertyChangeEvent;

            if (shouldHookPropertyChangeEvent)
                PropertyChangedEventManager.AddListener(notifyPropertyChanged, this, string.Empty);
            else
                PropertyChangedEventManager.RemoveListener(notifyPropertyChanged, this, string.Empty);

        }

                #endregion //VerifyDataItemNotifyChangeStatus	
    
			#endregion //Private Methods

		#endregion //Methods

		#region IWeakEventListener Members

		// JJD 5/9/07 - BR22698
		// Added support for listening to data item's PropertyChanged event
		// if the datasource doesn't supply cell value change notifications thru IBindlingList
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					// JJD 7/9/07
					// Added support for handling change notifications on another thread 
					//this.RefreshCellValue(args.PropertyName);
					RecordCollectionBase collection = this.ParentCollection;
					if (collection != null)
					{
						RecordManager rm = collection.ParentRecordManager;

						if (rm != null)
							rm.OnCellValueChangeNotification(this, args);
					}
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for DataRecord, arg type: " + e != null ? e.ToString() : "null");
			}

			// JJD 09/22/11  - TFS84708 - Optimization
			if (managerType == typeof(CollectionChangedEventManager) ||
				managerType == typeof(BindingListChangedEventManager))
			{
				this.OnEnumerableCellDataChanged();
				return true;
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for DataRecord, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion
	}

    #endregion //DataRecord

    #region ExpandableFieldRecord

	/// <summary>
	/// Used by a XamDataGrid, XamDataCarousel or XamDataPresenter to represent the child records of a DataRecord from an expandable field. 
	/// </summary>
	/// <remarks>
	/// <para class="body"><see cref="ExpandableFieldRecord"/>s are child records of <see cref="DataRecord"/>s. 
	/// The <see cref="RecordPresenter.NestedContent"/> of these elements contain either a <see cref="RecordListControl"/> with the <see cref="ExpandableFieldRecord"/>'s <see cref="Infragistics.Windows.DataPresenter.Record.ViewableChildRecords"/>, if the associated <see cref="Field"/>'s <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> implement's the <see cref="System.Collections.IEnumerable"/> interface, or an <see cref="ExpandedCellPresenter"/> containing the actual value of the <see cref="Cell"/>.</para>
	/// <para class="note"><b>Note: </b>This is not a UIElement but is a lightweight wrapper around a data item. It is represented in the UI via a corresponding <see cref="ExpandableFieldRecordPresenter"/> element.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="RecordManager"/>
	/// <seealso cref="RecordManager.Groups"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsExpandable"/>
	/// <seealso cref="ExpandableFieldRecordPresenter"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataPresenter"/>
	/// <seealso cref="DataRecord"/>
    /// <seealso cref="DataRecord.ChildRecords"/>
    // JJD 10/30/08 - TFS7145
    // Listen for Visibility changes from the Field
    //public class ExpandableFieldRecord : Record
    public class ExpandableFieldRecord : Record, IWeakEventListener
    {
        #region Private Members

        private Field _field;
        private RecordManager _records;
		
		// JJD 09/22/11  - TFS84708 - Optimization
		// Added member for child enumerable
		private ChildDataTrackingInfo _trackingInfo;

		private bool _isExpandedLastReturned;
		private bool _isExpandedAlwaysLastReturned;

        #endregion //Private Members

        #region Constructors

        internal ExpandableFieldRecord(RecordCollectionBase parentCollection, Field field)
            : base(field.Owner, parentCollection)
        {
            if (field == null)
				throw new ArgumentNullException( "field", DataPresenterBase.GetString( "LE_ArgumentNullException_4" ) );
            
            this._field   = field;

            // JJD 10/30/08 - TFS7145
            // Moved the initialization of the visibility property out of the ctor and
            // into the caller logic after the record is added to the sparse array.
            // Otherwise, we end up with assertions in the viewable records collection
            // when the visibility of the field is collapsed
			//this.Visibility = field.VisibilityResolved;

            // JJD 10/30/08 - TFS7145
            // Listen for Visibility changes from the Field
			// JJD 6/27/11 - TFS36572
			
			
			
			// Since we also want to be notified when the PropertyDescriptorVersion has changed we
			// need to add another listener for that
            
            
            PropertyChangedEventManager.AddListener(field, this, "VisibilityResolved");
			PropertyChangedEventManager.AddListener(field, this, "PropertyDescriptorVersion");
        }

        #endregion //Constructors

        #region Base class overrides

            #region AssociatedField

        internal override Field AssociatedField { get { return this._field; } }

            #endregion //AssociatedField	

			// JJD 09/22/11  - TFS84708 - Optimization
			#region ChildRecordsIfNeeded

		internal override RecordCollectionBase ChildRecordsIfNeeded 
		{ 
			get 
			{ 
				return this.GetChildRcdCollection(false); 
			} 
		}

			#endregion //ChildRecordsIfNeeded

            #region ChildRecordsInternal

        internal override RecordCollectionBase ChildRecordsInternal { get { return this.ChildRecords; } }

            #endregion //ChildRecordsInternal	
        
            // JJD 4/3/08 - added support for printing
            #region CloneAssociatedRecordSettings


        // MBS 7/28/09 - NA9.2 Excel Exporting
        //internal override void CloneAssociatedRecordSettings(Record associatedRecord, ReportViewBase reportView)
        internal override void CloneAssociatedRecordSettings(Record associatedRecord, IExportOptions options)
        {
            base.CloneAssociatedRecordSettings(associatedRecord, options);

            ExpandableFieldRecord associatedExRcd = associatedRecord as ExpandableFieldRecord;

            Debug.Assert(associatedExRcd != null);

            if (associatedExRcd != null)
            {
                this._isExpandedAlwaysLastReturned = associatedExRcd._isExpandedAlwaysLastReturned;
                this._isExpandedLastReturned = associatedExRcd._isExpandedLastReturned;
            }
        }

            #endregion //CloneAssociatedRecordSettings	

			#region CreateRecordPresenter

		
		
		/// <summary>
		/// Creates a new element to represent this record in a record list control.
		/// </summary>
		/// <returns>Returns a new element to be used for representing this record in a record list control.</returns>
		internal override RecordPresenter CreateRecordPresenter( )
		{
			return new ExpandableFieldRecordPresenter( );
		}

			#endregion // CreateRecordPresenter

			#region Description

		/// <summary>
		/// Gets/sets the description for the record
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Description
		{
			get
			{
				string desc = base.Description;
				if (desc != null && desc.Length > 0)
					return desc;

				if (this.AssociatedField != null)
				{
					FieldLayout.FieldDescriptor fieldDescriptor = this.ParentDataRecord == null ? null : this.ParentDataRecord.PropertyDescriptorProvider.GetFieldDescriptor(this.AssociatedField);

					if (fieldDescriptor != null && fieldDescriptor.DataColumn != null)
						return fieldDescriptor.DataColumn.Caption;
					else
					if ( this.AssociatedField.Label is string )
						return this.AssociatedField.Label as string;
					else
						return this.AssociatedField.Name;
				}
				else
					return string.Empty;
			}
			set
			{
				base.Description = value;
			}
		}

		#endregion //Description

            // JJD 4/1/08 - added support for printing
            #region GetAssociatedRecordHelper


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
            DataRecord associatedParentRecord = this.ParentRecord.GetAssociatedRecord() as DataRecord;

            Debug.Assert(associatedParentRecord != null);

            if ( associatedParentRecord == null )
                return null;

			// AS 2/24/11 NA 2011.1 Word Writer
			// The ChildRecords doesn't include the ExpandableFieldRecord instances that represents 
			// Fields whose IsExpandable is true but whose IsExpandableByDefault is false.
			//
			//foreach (ExpandableFieldRecord expandableFieldRcd in associatedParentRecord.ChildRecords)
			foreach (ExpandableFieldRecord expandableFieldRcd in associatedParentRecord.ViewableChildRecords)
			{
                if (expandableFieldRcd != null && 
                    expandableFieldRcd.Field.Name == this.Field.Name)
                    return expandableFieldRcd;
            }

            Debug.Fail("No associated expandableFieldRcd found");

            return null;

        }

            #endregion //GetAssociatedRecordHelper

            #region HasChildrenInternal

        internal override bool HasChildrenInternal 
        { 
            get
            {
				// if the field isn't expandable by default that means that
				// it doesn't support IEnumerable so return false
				if (!this.Field.IsExpandableByDefault)
					return false;

				//if (this.ChildRecords.Count > 0)
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ChildRecordsIfNeeded instead
				//RecordCollectionBase childRcds = this.ChildRecords;
				RecordCollectionBase childRcds = this.ChildRecordsIfNeeded;
				if (childRcds == null)
					return false;

				//if (childRcds != null && childRcds .Count > 0)
				if (childRcds .Count > 0)
					return true;

				ViewableRecordCollection vrc = this.ViewableChildRecords;

				return vrc != null && vrc.Count > 0;
            } 
        }

            #endregion //HasChildrenInternal	

			#region HasChildData

		// JJD 3/01/ 07 - BR17658
		// Made internal since they can get the information thru the ViewableChildRecords collection





		internal override bool HasChildData
		{
			get
			{
				if (!this.IsExpanded)
					return true;

                // JJD 4/28/08 - BR31406 and BR31707 
                // Moved logic into helper method
                //// if the field isn't expandable by default that means that
                //// it doesn't support IEnumerable so return true if the 
                //// call has any non-null value
                //if ( !this.Field.IsExpandableByDefault )
                //{
                //    object value = this.ParentDataRecord.GetCellValue(this.Field, true);

                //    return (value != null && !(value is DBNull));
                //}

                //return this.HasVisibleChildren;
                return CheckHasChildData();
			}
		}

			#endregion //HasChildData

			#region HasVisibleChildren

		// JJD 3/01/ 07 - BR17658
		// Made internal since they can get the information thru the ViewableChildRecords collection





        internal override bool HasVisibleChildren
		{
			get
			{
				if (base.HasVisibleChildren)
					return true;

				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//ViewableRecordCollection vrc = this.ViewableChildRecords;
				ViewableRecordCollection vrc = this.ViewableChildRecordsIfNeeded;

				return vrc != null && vrc.Count > 0;
			}
		}

			#endregion //HasVisibleChildren	

			#region IsExpanded

		/// <summary>
		/// Gets/sets whether this is record is expanded
		/// </summary>
		public override bool IsExpanded
		{
			get
			{
				bool isExpanded = false;

				if (this.CanExpand)
				{
					if (this.IsExpandedAlways)
						isExpanded = true;
					else
						isExpanded = base.IsExpanded;
				}

				if (isExpanded != this._isExpandedLastReturned)
				{
					this._isExpandedLastReturned = isExpanded;
					this.DirtyScrollCount();
					this.RaisePropertyChangedEvent("IsExpanded");
				}

				return isExpanded;
			}
			set
			{
				base.IsExpanded = value;
			}
		}

			#endregion //IsExpanded	

			#region OccupiesScrollPosition

		internal override bool OccupiesScrollPosition
		{
			get
			{
				// JJD 4/14/07
				// Call the bas implementation of VisibilityResolved since
				// this class's implemnation now references this property
				// which would cause a stack overflow.
				//if (this.VisibilityResolved == Visibility.Collapsed)
				if (base.VisibilityResolved == Visibility.Collapsed)
					return false;

				// SSP 8/4/09 - NAS9.2 Enhanced grid view
				// 
				DataPresenterBase dp = this.DataPresenter;
				bool isFlatView = null != dp && dp.IsFlatView;				

				// if we are expanded and there are child records then
				// we don't want to take up a scroll position
				bool isExpanded = this.IsExpanded;
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//if (isExpanded &&
				//    this.Field.IsExpandableByDefault &&
				//    // SSP 8/4/09 - NAS9.2 Enhanced grid view
				//    // Added ! isFlatView condition.
				//    // 
				//    ! isFlatView &&
				//    // JJD 9/18/09 - TFS20567
				//    // if the header is not visible return false as well
				//    //this.ViewableChildRecords.Count > 0)
				//    (this.ViewableChildRecords.Count > 0 || this.IsHeaderVisible == false))
				//    return false;
                if (isExpanded &&
                    this.Field.IsExpandableByDefault &&
					// SSP 8/4/09 - NAS9.2 Enhanced grid view
					// Added ! isFlatView condition.
					// 
					! isFlatView  )
				{
					// JJD 09/22/11  - TFS84708 - Optimization
					// Use ViewableChildRecordsIfNeeded instead
					ViewableRecordCollection vcr = this.ViewableChildRecordsIfNeeded;
                     if ( (vcr != null && vcr.Count > 0) || this.IsHeaderVisible == false)
						return false;
				}

				// SSP 8/4/09 - NAS9.2 Enhanced grid view
				// NOTE: It's necessary for IsExpanded call to be made above since
				// that can affect the value of IsHeaderVisible.
				// 
				if ( isFlatView && isExpanded && ! this.IsHeaderVisible
					&& this.Field.IsExpandableByDefault
					&& Visibility.Collapsed == this.ExpansionIndicatorVisibility )
					return false;

                Record parent = this.ParentRecord;
               
                // JJD 3/17/10 - TFS28705 
                // Since special records can't be expanded in the UI return false if the 
                // parent is a special record
                if (parent == null || parent.IsSpecialRecord)
                    return false;

				return true;
			}
		}

			#endregion //OccupiesScrollPosition	
    
            #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RecordType RecordType
        {
            get
            {
                return RecordType.ExpandableFieldRecord;
            }
        }

            #endregion //RecordType

			#region IsSelectable

        /// <summary>
        /// Property: Returns false since an expandable field record cannot be selected.
        /// </summary>
        internal protected override bool IsSelectable
        {
            get
            {
                return false;
            }
        }

			#endregion // IsSelectable

            #region SortChildren

        internal override void SortChildren()
        {
			// JJD 09/22/11  - TFS84708 - Optimization
			// Use the ChildRecordManagerIfNeeded instead which won't create
			// child rcd managers for leaf records
			//if (this.ChildRecordManager != null)
			//	this.ChildRecordManager.VerifySort();
			RecordManager crm = this.ChildRecordManagerIfNeeded;
			if (crm != null)
				crm.VerifySort();
        }

            #endregion //SortChildren	

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			Field fld = this.Field;
			DataRecord parent = this.ParentDataRecord;

			StringBuilder sb = new StringBuilder();

			sb.Append("ExpandableFieldRecord: ");
			
			if ( fld != null )
				sb.Append(fld);

			if (parent != null)
			{
				sb.Append(", ");
				sb.Append(parent.ToString());
			}

			return sb.ToString();
		}

			#endregion //ToString	

            #region VerifySortOrderOfChildren

        internal override void VerifySortOrderOfChildren()
        {
            if (this._records == null || this._records.Unsorted.Count < 1)
                return;

            this.SortChildren();
        }

            #endregion //VerifySortOrderOfChildren	

			#region VisibilityResolved

		/// <summary>
		/// Gets the resolved visibility of the record (read-only)
		/// </summary>
		public override Visibility VisibilityResolved
		{
			get
			{
				Visibility visibility = base.VisibilityResolved;

				// if explicitly collapsed then return collapsed
				if (visibility == Visibility.Collapsed)
					return visibility;

				// if there are no child records then return collapsed

				// JJD 4/14/07
				// Only return collapsed if the record doesn't occupy a scroll
				// position as well as have no children
				//if (!this.HasChildData)
                if (this.OccupiesScrollPosition == false &&
                    !this.HasChildData)
					return Visibility.Collapsed;

				return visibility;
			}
		}

			#endregion //VisibilityResolved	

        #endregion Base class overrides

        #region Properties

            #region Public Properties

                #region ChildRecordManager

        /// <summary>
        /// Returns a RecordManager for the child records (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RecordManager ChildRecordManager
        {
            get
            {
				// JJD 09/22/11  - TFS84708 - Optimization
				// Moved logic to helper method
				this.VerifyChildRecordManager(true);

                return this._records;
            }
        }

                #endregion //ChildRecordManager

                #region ChildRecords

        /// <summary>
        /// Returns a collection of child records (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RecordCollectionBase ChildRecords
        {
            get
            {
				// JJD 09/22/11  - TFS84708 - Optimization
				// MOved logic into helper method
				return GetChildRcdCollection(true);
            }
        }

                #endregion //ChildRecords

                #region Field

        /// <summary>
        /// Returns the parent field for child records only (read-only)
        /// </summary>
        /// <remarks>Returns null for root level (i.e. non-child) records.</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Field Field { get { return this._field; } }

                #endregion //Field

            #endregion //Public Properties

			#region Internal Properties

				// JJD 7/15/10 - TFS35815 - Optimization
				#region ChildRecordManagerIfAllocated

		internal RecordManager ChildRecordManagerIfAllocated { get { return this._records; } }

				#endregion //ChildRecordManagerIfAllocated	

				// JJD 09/22/11  - TFS84708 - Optimization
				// Added internal property to only create child record manager if there were child records
				#region ChildRecordManagerIfNeeded

		internal RecordManager ChildRecordManagerIfNeeded
		{
			get
			{
				this.VerifyChildRecordManager(false);

				return this._records;
			}
		}

				#endregion //ChildRecordManagerIfNeeded	
        
				#region IsExpandedAlways

		internal bool IsExpandedAlways
		{
			get
			{
				bool isExpandedAlways = false;

				switch (this.Field.ExpandableFieldRecordExpansionModeResolved)
				{
					case ExpandableFieldRecordExpansionMode.ExpandAlways:
						isExpandedAlways = true;
						break;
					case ExpandableFieldRecordExpansionMode.ShowExpansionIndicatorIfSiblingsExist:
						if (this.ParentDataRecord.ChildRecords.Count < 2)
							isExpandedAlways = true;
						break;
				}

				if (isExpandedAlways != this._isExpandedAlwaysLastReturned)
				{
					this._isExpandedAlwaysLastReturned = isExpandedAlways;
					this.DirtyScrollCount();
					this.RaisePropertyChangedEvent("ExpansionIndicatorVisibility");
				}

				return isExpandedAlways;
			}
		}

				#endregion //IsExpandedAlways	

				#region IsHeaderVisible

		// SSP 8/10/09 - NAS9.2 Enhanced grid-view
		// Added IsHeaderVisible. Logic in there is moved from ExpandableFieldRecordPresenter's
		// SetHeaderVisibility method.
		// 
		/// <summary>
		/// Indicates if the expandable field record's header (label) is visible.
		/// </summary>
		internal bool IsHeaderVisible
		{
			get
			{
				bool displayHeader = true;

				switch ( this.Field.ExpandableFieldRecordHeaderDisplayModeResolved )
				{
					case ExpandableFieldRecordHeaderDisplayMode.DisplayHeaderOnlyWhenCollapsed:
						displayHeader = !this.IsExpanded;
						break;
					case ExpandableFieldRecordHeaderDisplayMode.DisplayHeaderOnlyWhenExpanded:
						displayHeader = this.IsExpanded;
						break;
					case ExpandableFieldRecordHeaderDisplayMode.NeverDisplayHeader:
						displayHeader = false;
						break;
				}

				return displayHeader;
			}
		}

				#endregion // IsHeaderVisible
    
			#endregion //Internal Properties
		
		#endregion //Properties

        #region Methods

            #region Public Methods

            #endregion //Public Methods

            #region Internal Methods

                // JJD 4/28/08 - BR31406 and BR31707 - added helper method
                #region CheckHasChildData

        internal bool CheckHasChildData()
        {
            // if the field isn't expandable by default that means that
            // it doesn't support IEnumerable so return true if the 
            // call has any non-null value
            if (!this.Field.IsExpandableByDefault)
            {
                object value = this.ParentDataRecord.GetCellValue(this.Field, true);

                return (value != null && !(value is DBNull));
            }

            // JJD 1/29/09 - NA 2009 vol 1 - Record filtering
            // Even if there are no visible records we may still need to return
            // true from this method. This is to cover the situation where all of the 
            // child records have been filtered out but we still need to show the header
            // so there is some Ui (i.e LabelIcons) in the header to change
            // the filter criteria. 
            //return this.HasVisibleChildren;
            if (this.HasVisibleChildren)
                return true;

			// JJD 09/22/11  - TFS84708 - Optimization
			// Use the ChildRecordManagerIfNeeded instead which won't create
			// child rcd managers for leaf records
			//RecordManager rm = this.ChildRecordManager;
            RecordManager rm = this.ChildRecordManagerIfNeeded;

            if (rm == null )
                return false;

            if (rm.Unsorted.Count > 0)
            {
                FieldLayout fl = rm.FieldLayout;

                // JJD 1/29/09 - NA 2009 vol 1 - Record filtering
                // If there is a separateHeader and filteruitype
                // resolves to LabelIcons then return true
                if (fl.HasSeparateHeader &&
                    fl.FilterUITypeResolved == FilterUIType.LabelIcons)
                {
                    return true;
                }
            }

            return false;
        }

                #endregion //CheckHasChildData	

                // JJD 6/3/09 - TFS18108 - added
                #region Initialize

        internal void Initialize()
        {
            // JJD 10/30/08 - TFS7145
            // Moved the initialization of the visibility property out of the ctor and
            // into here so that we can do it after the record is added to the sparse array.
            // Otherwise, we end up with assertions in the viewable records collection
            // when the visibility of the field is collapsed
            this.Visibility = this.Field.VisibilityResolved;

            // JJD 4/14/07
            // raise InitializeRecord event
            this.FireInitializeRecord();
        }

                #endregion //Initialize
				
				// JJD 09/22/11  - TFS84708 - Optimization
				#region OnDataChanged

		internal void OnDataChanged()
		{
			if (_trackingInfo != null)
			{
				// clear the existing cache 
				this.ClearChildDataCache(true);

				// If we are expanded then access the child ViewableRecordCollection
				// so we can update our scroll count
				if (this.IsExpanded)
				{
					ViewableRecordCollection vcr = this.ViewableChildRecordsIfNeeded;

					if (vcr != null)
						this.DirtyScrollCount();
				}
			}
		}

				#endregion //OnDataChanged	
    
            #endregion //Internal Methods

			#region Private Methods

				#region ClearChildDataCache

		// JJD 09/22/11  - TFS84708 - Optimization
		private void ClearChildDataCache(bool unwireEvents)
		{
			if (_trackingInfo == null)
				return;

			if (unwireEvents)
				_trackingInfo.Unwire(this);

			_trackingInfo = null;
		}

				#endregion //ClearChildDataCache	

				#region GetChildRcdCollection

		// JJD 09/22/11  - TFS84708 - Optimization
		private RecordCollectionBase GetChildRcdCollection(bool alwaysCreateifEnumerable)
		{
			RecordManager rm;

			if (alwaysCreateifEnumerable)
				rm = this.ChildRecordManager;
			else
				rm = this.ChildRecordManagerIfNeeded;

			if (rm == null)
				return null;

			return rm.Current;
		}

				#endregion //GetChildRcdCollection	
        
				// JJD 6/27/11 - TFS36572 added
				#region OnPropertyDescriptorChanged

		private void OnPropertyDescriptorChanged()
		{
			if (_records == null)
				return;

			// Since the PropertyDescriptor changed we need to re-get the cell value
			// which contains the list on child data items
			if (_field.IsExpandableByDefault)
			{
				// set its datasource to the cell value of the field
				this._records.DataSource = this.ParentDataRecord.GetCellValue(this._field, true) as IEnumerable;
			}
			else
			{
				this._records.DataSource = null;
			}
		}

				#endregion //OnPropertyDescriptorChanged

				// JJD 09/22/11  - TFS84708 - Optimization
				#region VerifyChildRecordManager

		// Added helper method to optionally only create child record manager if there were child records
		private void VerifyChildRecordManager(bool alwaysCreateIfEnumerable)
		{
			if (_records != null)
				return;

			// only create a child record manager if the field is expandable by
			// default (i.e. its data type implements IEnumerable)
			if (false == this.Field.IsExpandableByDefault)
				return;

			// create the record manager
			//this._records = new RecordManager(this.DataPresenter, this, this._field);

			//// set its data source to the cell value of the field
			//this._records.DataSource = this.ParentDataRecord.GetCellValue(this._field, true) as IEnumerable;
			bool wireEvents = false;
			bool create = alwaysCreateIfEnumerable;

			// set its data source to the cell value of the field
			if (_trackingInfo == null)
			{
				_trackingInfo = new ChildDataTrackingInfo(this.ParentDataRecord.GetCellValue(this._field, true) as IEnumerable);
				if (_trackingInfo._childData != null)
				{
					wireEvents = true;
					create = _trackingInfo.Initialize(this.DataPresenter, alwaysCreateIfEnumerable);
				}
			}

			if (create)
			{
				this._records = new RecordManager(this.DataPresenter, this, this._field);
				this._records.DataSource = _trackingInfo._childData;

				// since we created a child record manager we can clear any cached child date members
				this.ClearChildDataCache(!wireEvents);
			}
			else 
			{
				// Listen for collection changed notifications
				if (wireEvents)
					_trackingInfo.Wire(this);
			}

		}

				#endregion //VerifyChildRecordManager	
    
			#endregion //Private Methods	
        
        #endregion //Methods

        #region IWeakEventListener Members

        // JJD 10/30/08 - TFS7145
        // Sync up the record Visibility with the field's when it changes
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
				// JJD 6/27/11 - TFS36572
				// Since we are now getting notifications when any field property changes
				// we need to switch on the PropertyName
				//this.Visibility = this._field.VisibilityResolved;
				switch(((PropertyChangedEventArgs)e).PropertyName)
				{
					case "VisibilityResolved":
						this.Visibility = this._field.VisibilityResolved;
						break;

					// JJD 6/27/11 - TFS36572 added
					case "PropertyDescriptorVersion":
						this.OnPropertyDescriptorChanged();
						break;
				}

                return true;
            }

			// JJD 09/22/11  - TFS84708 - Optimization
			if (managerType == typeof(CollectionChangedEventManager) ||
				managerType == typeof(BindingListChangedEventManager))
            {
				this.OnDataChanged();
                return true;
            }

            return false;
        }

        #endregion

		// JJD 09/22/11  - TFS84708 - Optimization
		#region ChildDataTrackingInfo private class

		internal class ChildDataTrackingInfo
		{
			#region Members

			internal IEnumerable _childData;
			internal IEnumerable _underlyingData;
			internal EditableCollectionViewProxy _proxy;

			#endregion //Members	
    
			#region Constructor

			internal ChildDataTrackingInfo(IEnumerable childData)
			{
				_childData = childData;

				if (_childData != null)
				{
					// JJD 9/28/11 - TFS89531/TFS84708
					// Pass false into returnCollectionViewsSourceCollection 
					// so we don't get the collection view's source returned
					_underlyingData = DataBindingUtilities.GetUnderlyingItemSource(_childData, false);
				}
			}

			#endregion //Constructor	
    
			#region Methods

			#region Initialize

			internal bool Initialize(DataPresenterBase dp, bool bypassAddNewCheck)
			{
				if (bypassAddNewCheck)
					return true;

				if (_underlyingData == null)
					return false;

				bool create = false;

				ICollection childColl = _underlyingData as ICollection;

				if (childColl != null)
					create = childColl.Count > 0;
				else
					create = _underlyingData.GetEnumerator().MoveNext();

				if (create == false)
				{
					bool canCreate = false;

					_proxy = RecordManager.GetEditableCollectionViewProxy(_underlyingData, _proxy);
					if (_proxy != null)
						canCreate = _proxy.CanAddNew;
					else
					{
						IBindingList blist = _underlyingData as IBindingList;

						if (blist != null && blist.AllowNew)
						{
							Type itemType;

							// See if we can get the property descriptors to show the add record  
							ITypedList typedList = PropertyDescriptorProvider.GetTypedListFromContainingList(_underlyingData, out itemType);

							canCreate = typedList != null || itemType != null;
						}
					}

					if (canCreate == true)
					{
						// JJD 09/26/11  - TFS84708 
						// If we can create asume that we should create unless
						// AllowAddNew is explicitly set to false on the DP's FieldLayoutSettings
						create = true;

						if (dp != null && dp.FieldLayoutSettingsIfAllocated != null)
						{
							bool? allowAddNew = dp.FieldLayoutSettingsIfAllocated.AllowAddNew;

							// Only create a child record manager if the DP's FieldLayoutSettings.AllowAddNew
							// is explicitly set to true
							if (allowAddNew.HasValue)
								create = allowAddNew.Value;
						}
					}
				}

				return create;
			}

			#endregion //Initialize	
 
			#region Unwire

			internal void Unwire(IWeakEventListener listener)
			{
				if (_proxy != null)
				{
					CollectionChangedEventManager.RemoveListener(_proxy.View, listener);
				}
				else
				{
					IBindingList blist = _underlyingData as IBindingList;

					if (blist != null)
						BindingListChangedEventManager.RemoveListener(blist, listener);
					else
					{
						INotifyCollectionChanged notifyCollChanged = _underlyingData as INotifyCollectionChanged;

						if (notifyCollChanged != null)
							CollectionChangedEventManager.RemoveListener(notifyCollChanged, listener);
					}
				}
			}

			#endregion //Unwire	
 
			#region Wire

			internal void Wire(IWeakEventListener listener)
			{
				if (_proxy != null)
				{
					CollectionChangedEventManager.AddListener(_proxy.View, listener);
				}
				else
				{
					IBindingList blist = _underlyingData as IBindingList;

					if (blist != null)
						BindingListChangedEventManager.AddListener(blist, listener);
					else
					{
						INotifyCollectionChanged notifyCollChanged = _underlyingData as INotifyCollectionChanged;

						if (notifyCollChanged != null)
							CollectionChangedEventManager.AddListener(notifyCollChanged, listener);
					}
				}
			}

			#endregion //Wire	
    
			#endregion //Methods
		}

		#endregion //ChildDataTrackingInfo private class
	}

    #endregion //ExpandableFieldRecord

    #region GroupByRecord class

	/// <summary>
	/// Used by a XamDataGrid, XamDataCarousel or XamDataPresenter to represent a grouping of records. 
	/// </summary>
	/// <remarks>
	/// <para class="body">This object is used as a parent record for a set of logically related <see cref="DataRecord"/>s (or nested GroupByRecords) created by a Grouping operation related to a specific <see cref="GroupByField"/>.</para>
	/// <para class="note"><b>Note: </b>This is not a UIElement but is a lightweight wrapper around the data item. It is represented in the UI via a corresponding <see cref="GroupByRecordPresenter"/> element.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
    /// <seealso cref="RecordManager"/>
    /// <seealso cref="RecordManager.Groups"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Records"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsGroupBy"/>
	/// <seealso cref="GroupByRecordPresenter"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataPresenter"/>
    public class GroupByRecord : Record
    {
        #region Private Members

        private object _commonValue;
        private Field _field;
        private RecordCollection _records;

        // JJD 4/10/08
        // Added support for caching a dynamicDesriptionString to support OutlookDataEvaluator
        private DynamicResourceString _dynamicDesriptionString;

        #endregion //Private Members	
    
        #region Constructors

        internal GroupByRecord(Field field, RecordCollectionBase parentCollection, object commonValue)
            : base(field.Owner, parentCollection)
        {
            this._field = field;

            this._commonValue = commonValue;
        }

        internal GroupByRecord(FieldLayout layout, RecordCollectionBase parentCollection)
            : base(layout, parentCollection)
        {
            this._commonValue = layout;
        }

        #endregion //Constructors

        #region Base class overrides

            #region AssociatedField

        internal override Field AssociatedField { get { return this._field; } }

            #endregion //AssociatedField	

            #region ChildRecordsInternal

        internal override RecordCollectionBase ChildRecordsInternal { get { return this.ChildRecords; } }

            #endregion //ChildRecordsInternal	

			#region CreateRecordPresenter

		
		
		/// <summary>
		/// Creates a new element to represent this record in a record list control.
		/// </summary>
		/// <returns>Returns a new element to be used for representing this record in a record list control.</returns>
		internal override RecordPresenter CreateRecordPresenter( )
		{
			return new GroupByRecordPresenter( );
		}

			#endregion // CreateRecordPresenter

            #region Description

        /// <summary>
        /// Gets/sets the description for the row
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Description
        {
            get
            {
                
                
                
                return this.GetDescriptionWithoutSummariesHelper();
            }
            set
            {
                base.Description = value;

				// SSP 6/2/08 - Summaries Functionality
				// Added DescriptionWithSummaries property.
				// 
				this.DirtyDescriptionWithSummaries( );
            }
        }

            #endregion //Description

			#region DescriptionWithSummaries

		// SSP 6/2/08 - Summaries Functionality
		// Added DescriptionWithSummaries property.
		// 
		/// <summary>
		/// Returns the description of the group-by record, with any summaries appended to it.
		/// </summary>
		public string DescriptionWithSummaries
		{
			get
			{
				string val = this.GetDescriptionWithSummaries( );
				if ( val != _cachedDescriptionWithSummaries )
				{
					bool wasNull = null == _cachedDescriptionWithSummaries;
					_cachedDescriptionWithSummaries = val;

					// Don't raise prop change initially as well as if we have already raised
					// it in DirtyDescriptionWithSummaries.
					// 
					if ( ! wasNull )
						this.RaisePropertyChangedEvent( "DescriptionWithSummaries" );
				}

				return _cachedDescriptionWithSummaries;
			}
		}

		private string _cachedDescriptionWithSummaries;

		internal void DirtyDescriptionWithSummaries( )
		{
			_cachedDescriptionWithSummaries = null;
			this.RaisePropertyChangedEvent( "DescriptionWithSummaries" );
		}

			#endregion DescriptionWithSummaries

			#region EnsureFiltersEvaluated

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Ensures that record filters are applied to this record.
		/// </summary>
		internal override void EnsureFiltersEvaluated( )
		{
			// SSP 5/15/09 TFS16576
			// We need to actively maintain the group-by record's filter state whenever
			// a child record gets filtered out/in or deleted/added since a group-by
			// record is considered filtered out when all of its children are filtered
			// out. So whenever a record gets filtered out/in or deleted etc..., we 
			// need to re-evaluate the parent group-by record's filter state.
			// 
			// ------------------------------------------------------------------------
			if ( FilterState.NeverFilter != _cachedFilterState )
			{
				RecordCollectionBase parentColl = this.ParentCollection;
				if ( null != parentColl )
				{
					ViewableRecordCollection vrc = parentColl.ViewableRecords;
					if ( null != vrc && vrc.EnsureFiltersEvaluated( ) )
					{
						if ( 0 != ( FilterState.NeedsToRefilter & _cachedFilterState ) )
							this.ApplyFiltersHelper( true );
					}
				}
			}
			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------
		}

		// SSP 5/15/09 TFS16576
		// Added ApplyFiltersHelper method. Logic in there was moved from the 
		// ViewableRecordCollection.EnsureFiltersEvaluated.
		// 
		internal void ApplyFiltersHelper( bool raiseNotifications )
		{
			RecordCollection childRecords = this.ChildRecords;
			if ( null != childRecords )
			{
				ViewableRecordCollection childVRC = childRecords.ViewableRecords;
				if ( null != childVRC && childVRC.EnsureFiltersEvaluated( ) )
				{
					bool allChildrenFilteredOut = 0 == childVRC.CountOfNonSpecialRecords;
					this.ApplyFiltersHelper( allChildrenFilteredOut, raiseNotifications );
				}
			}
		}

		// SSP 5/15/09 TFS16576
		// Added ApplyFiltersHelper method. Logic in there was moved from the 
		// ViewableRecordCollection.EnsureFiltersEvaluated.
		// 
		internal void ApplyFiltersHelper( bool allChildrenFilteredOut, bool raiseNotifications )
		{
			if ( RecordType.GroupByField == this.RecordType )
			{
				Record.FilterState state;
				FieldLayout fieldLayout = this.FieldLayout;

				RecordManager rm = this.RecordManager;
				FieldLayout fl = this.FieldLayout;
				ResolvedRecordFilterCollection filters = null != rm ? rm.RecordFiltersResolved : null;
				bool hasFilters = null != filters && null != fl && filters.HasActiveFilters( fl );

				if ( ! hasFilters )
					// If there are no filters then the filter state should be set to 0 
					// to indicate that there are no filters.
					// 
					state = 0;
				else
					state = allChildrenFilteredOut ? Record.FilterState.FilteredOut : Record.FilterState.FilteredIn;

				this.InternalSetFilterState( state, raiseNotifications );

				// JJD 2/7/11 - TFS35853
				// Move call to OnGroupingChanged out of if block so it always gets called.
				// This is to cover the situation where this is a GroupByFieldLayout record.
				//// SSP 5/12/09 TFS16576
				//// Made a change to display the count of visible child records instead 
				//// of all child records in the group-by record description. This however
				//// means that whenever filter is applied, we need to update the 
				//// description with the correct visible child records count.
				//// 
				//this.OnGroupingChanged( );
			}

			// JJD 2/7/11 - TFS35853
			// Move call to OnGroupingChanged out of if block above so it always gets called.
			// This is to cover the situation where this is a GroupByFieldLayout record.
			// SSP 5/12/09 TFS16576
			// Made a change to display the count of visible child records instead 
			// of all child records in the group-by record description. This however
			// means that whenever filter is applied, we need to update the 
			// description with the correct visible child records count.
			// 
			this.OnGroupingChanged( );
		}

			#endregion // EnsureFiltersEvaluated

            // JJD 4/1/08 - added support for printing
            #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
            return this.ParentCollection.ParentRecordManager.GetAssociatedRecordInternal(this);
        }

            #endregion //GetAssociatedRecord

            #region HasChildrenInternal

        internal override bool HasChildrenInternal 
        { 
            get
            {
                return true;
            } 
        }

            #endregion //HasChildrenInternal	
    
			// AS 5/19/09 TFS17455
			#region IsStillValid
		internal override bool IsStillValid
		{
			get
			{
				if (this.AssociatedField == null ||
					!this.AssociatedField.IsGroupBy)
					return false;

				return base.IsStillValid;
			}
		} 
			#endregion //IsStillValid

            #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RecordType RecordType 
        { 
            get 
            {
                if ( this._field == null )
                    return RecordType.GroupByFieldLayout;
                else
                    return RecordType.GroupByField;
            } 
        }

            #endregion //RecordType

            #region SortChildren

        internal override void SortChildren()
        {
            if (this._records == null)
                return;

            this._records.VerifySortOrder(true);
        }

            #endregion //SortChildren	

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{

			StringBuilder sb = new StringBuilder();

			sb.Append("GroupByRecord: ");
			sb.Append(this.Description);

			return sb.ToString();
		}

			#endregion //ToString	

            #region VerifySortOrderOfChildren

        internal override void VerifySortOrderOfChildren() 
        {
            // JJD 10/16/09 - TFS23163
            // If this record has a RecordZType of GroupByFieldLayout then
            // it will not have a field but its child records may still need to be sorted
            //if (this._field == null)
            if (this._field == null && this.RecordType != RecordType.GroupByFieldLayout)
                return;

            if ( this.HasChildrenInternal == false )
                return;

            this.SortChildren();
        }

            #endregion //VerifySortOrderOfChildren	

        #endregion Base class overrides

        #region Properties

            #region Public Properties

                #region ChildRecords

        /// <summary>
        /// Returns a read-only collectionn of <see cref="Record"/>s this group contains
        /// </summary>
        /// <remarks>This collection will either contain all <b>GroupByRecord</b>s or all <see cref="DataRecord"/>s, never both.</remarks>
        /// <seealso cref="DataRecord"/>
        /// <seealso cref="RecordManager"/>
        /// <seealso cref="RecordManager.Groups"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RecordCollection ChildRecords
        {
            get
            {
                if (this._records == null)
                    this._records = new RecordCollection( this, this.ParentCollection.ParentRecordManager, this.FieldLayout);

                return this._records;
            }
        }

                #endregion //ChildRecords

                #region GroupByField

        /// <summary>
        /// Returns the <see cref="Field"/> used for grouping this group (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
        /// <seealso cref="FieldSettings.GroupByComparer"/>
        /// <seealso cref="FieldSettings.GroupByEvaluator"/>
        /// <seealso cref="FieldSettings.GroupByMode"/>
        /// <seealso cref="FieldSettings.GroupByRecordPresenterStyle"/>
        /// <seealso cref="Field.IsGroupBy"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Field GroupByField
        {
            get
            {
                return this._field;
            }
        }

                #endregion //GroupByField	

				#region IsFilteredOut

		// SSP 12/9/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Indicates whether all the data records of this group-by record are filtered out.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsFilteredOut</b> of a GroupByRecord indicates whether all the data records of this group-by record
		/// are filtered out.
		/// </para>
		/// </remarks>
        /// <value>
        /// <para class="body"><b>True</b> all the data records of this group-by record are filtered out or <b>false</b> if they are not. 
        /// However, if there are no active record filters then this property returns <b>null</b></para>
        /// </value>
		/// <seealso cref="DataRecord.IsFilteredOut"/>
		public bool? IsFilteredOut
		{
			get
			{
				return this.InternalIsFilteredOutNullable_Verify;
			}
		}

				#endregion // IsFilteredOut
    
                #region Value

        /// <summary>
        /// Returns the common value for this group (read-only)
        /// </summary>
        /// <seealso cref="DataRecord.GetCellValue(Field, bool)"/>
        /// <seealso cref="FieldSettings.GroupByComparer"/>
        /// <seealso cref="FieldSettings.GroupByEvaluator"/>
        /// <seealso cref="FieldSettings.GroupByMode"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Value
        {
            get
            {
				// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// If value is GroupInfo then unwrap it.
				// 
                //return this._commonValue;
				GroupInfo gi = _commonValue as GroupInfo;
				return null != gi ? gi.Value : _commonValue;
            }
        }

                #endregion //Value	
    
            #endregion //Public Properties

            #region Internal Properties

                #region DynamicDesriptionString

        // JJD 4/10/08
        // Added support for caching a dynamicDesriptionString to support OutlookDataEvaluator
        internal DynamicResourceString DynamicDesriptionString
        {
            get
            {
                return this._dynamicDesriptionString;
            }
            set
            {
                if (value != this._dynamicDesriptionString)
                {
                    this._dynamicDesriptionString = value;
                    this.RaisePropertyChangedEvent("Description");
					// SSP 6/2/08 - Summaries Functionality
					// Added DescriptionWithSummaries property.
					// 
					this.DirtyDescriptionWithSummaries( );
                }
            }
        }

                #endregion //DynamicDesriptionString

				#region CommonValueInternal

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		/// <summary>
		/// Returns the value of the _commonValue member variable. The <see cref="Value"/> property returns
		/// the _commonValue as well except when _commonValue is a <b>GroupInfo</b> instance in which case
		/// it unwraps it and returns the GroupInfo.Value where as this property returns whatever the 
		/// _commonValue is.
		/// </summary>
		internal object CommonValueInternal
		{
			get
			{
				return _commonValue;
			}
		} 

				#endregion // CommonValueInternal

            #endregion //Internal Properties	

        #endregion //Properties

        #region Methods

            #region Internal Methods

				// AS 2/9/11 NA 2011.1 Word Writer
				#region CreateSummaryRecord
		internal SummaryRecord CreateSummaryRecord()
		{
			return new SummaryRecord(this.FieldLayout, this.ChildRecords, SummaryDisplayAreaContext.InGroupByRecordsSummariesContext);
		} 
				#endregion //CreateSummaryRecord

                #region DoesRecordMatchGroup

        private bool DoesRecordMatchGroupHelper(DataRecord record, IGroupByEvaluator evaluator)
        {
            if (this.FieldLayout != record.FieldLayout)
                return false;

            // For GroupByFieldLayout records only the FieldLayout has to match
            if (this.RecordType == RecordType.GroupByFieldLayout)
                return true;

            Field field = this._field;

            //IGroupByEvaluator evaluator = field.GroupByEvaluatorResolved;

            // If they have specified an evaulator then call it
            //
            if (evaluator != null)
                return evaluator.DoesGroupContainRecord(this, record);

            IComparer comparer = field.SortComparerResolved;

            // If they have specified a sort comparer then get the cell from
            // the first record in the collection and use that for comparison.
            //
            if (null != comparer)
            {
                DataRecord firstRecord = this.FindFirstDataRecord();

                Debug.Assert(null != firstRecord,
                    "There must have been a firstRecord.",
                    "This method should not have been called when the descendent hierarchy of " +
                    "this group by record does not contain at least one regular record.");

                if (null != firstRecord)
                {
                    // JJD 5/29/09 - TFS18063 
                    // Use the new overload to GetCellValue which will return the value 
                    // converted into EditAsType
                    //return comparer.Compare(firstRecord.GetCellValue(field, true), record.GetCellValue(field, true)) == 0;
                    return comparer.Compare(firstRecord.GetCellValue(field, CellValueType.EditAsType), record.GetCellValue(field, CellValueType.EditAsType)) == 0;
                }
            }

            object x = this.Value;
            // JJD 5/29/09 - TFS18063 
            // Use the new overload to GetCellValue which will return the value 
            // converted into EditAsType
            //object y = record.GetCellValue(field, true);
            object y = record.GetCellValue(field, CellValueType.EditAsType);

            if (x == null)
            {
                return (y == null);
            }

            if (y == null)
            {
                return false;
            }

            return RecordManager.RecordsSortComparer.DefaultCompare(x, y, false, field) == 0;
        }


        internal bool DoesRecordMatchGroup(DataRecord record, bool recursive, IGroupByEvaluator evaluator)
        {
            // If the record does not match this group by record's criteria return false
            if (!this.DoesRecordMatchGroupHelper(record, evaluator))
                return false;

            if (recursive)
            {
                GroupByRecord parent = this.ParentRecord as GroupByRecord;

                // If we have a parent groupByRecord, for the record to belong to this
                // group by record, it also has to belong to any parent groups. So
                // make sure that it does.
                //
                if (null != parent)
                    return parent.DoesRecordMatchGroup(record);
            }

            return true;
        }

        /// <summary>
        /// Returns true if the record belongs in this group by record
        /// by walking up the parent group by records if any and making
        /// sure that the record also matches them.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        internal bool DoesRecordMatchGroup(DataRecord record)
        {
            Field field = this._field;

            IGroupByEvaluator evaluator;

            if (field != null)
                evaluator = field.GroupByEvaluatorResolved;
            else
                evaluator = null;

            return this.DoesRecordMatchGroup(record, true, evaluator);
        }

                #endregion //DoesRecordMatchGroup	

                #region FindFirstDataRecord



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal DataRecord FindFirstDataRecord()
        {
            if (this.ChildRecords.Count < 1)
                return null;

            Record firstRecord = this.ChildRecords[0];

            if (firstRecord is GroupByRecord)
                return ((GroupByRecord)firstRecord).FindFirstDataRecord();

            return firstRecord as DataRecord;
        }

                #endregion //FindFirstDataRecord	

				#region GetDescriptionWithoutSummariesHelper

		
		
		
		private string GetDescriptionWithoutSummariesHelper( )
		{
            string desc = base.Description;
            if (desc != null && desc.Length > 0)
                return desc;

            // JJD 4/10/08
            // Added support for caching a dynamicDesriptionString to support OutlookDataEvaluator
            if (this._dynamicDesriptionString != null)
                return this._dynamicDesriptionString.Value;

            // JJD 4/10/08
            // Use the DP's culture
            CultureInfo culture = this.DataPresenter.DefaultConverterCulture;

            // JJD 4/10/08
            // change baseText to baseValue
            //string baseText = null;
            object baseValue = null;

			// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// 
			GroupInfo groupInfo = _commonValue as GroupInfo;

            switch (this.RecordType)
            {
                case RecordType.GroupByFieldLayout:
                    baseValue = this.FieldLayout.Description;
                    break;

                case RecordType.GroupByField:
					// SSP 4/1/09 - Cell Text
					// 
					// ----------------------------------------------------------------------------------
					object value = _commonValue;

					// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// 
					if ( null != groupInfo )
					{
						if ( !string.IsNullOrEmpty( groupInfo.Description ) )
							return groupInfo.Description;

						value = groupInfo.Value;
					}

					baseValue = null != _field
						? CellTextConverterInfo.ConvertCellValue( value, _field )
						: value;

					// If the _commonValue is null then use GroupByDescription_NullValue_Literal
					// unless the editor converts null into a non-empty string.
					// 
					if ( null == value && string.IsNullOrEmpty( baseValue as string ) )
						baseValue = DataPresenterBase.GetString( culture, "GroupByDescription_NullValue_Literal" );

					
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

					// ----------------------------------------------------------------------------------
                    break;
            }

            if (baseValue != null)
            {
                // JJD 7/19/07
                // localize the string

                //StringBuilder sb = new StringBuilder();

                //sb.Append(baseText);
                //sb.Append(" (");
                //sb.Append(this.ChildRecords.Count.ToString());

                //if ( this.ChildRecords.Count == 1)
                //    sb.Append(" item)");
                //else
                //    sb.Append(" items)");

                //return sb.ToString();

                string format;

				// SSP 5/12/09 TFS16576
				// 
                //int count = this.ChildRecords.Count;
				// SSP 5/12/09 TFS16576
				// Display the count of visible child records instead of all child records.
				// 
				//int count = this.ChildRecords.Count;
				int count;
				// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// Added the if block and enclosed the existing code into the else block.
				// 
				if ( null != groupInfo )
				{
					count = groupInfo.ChildCount;
				}
				else
				{
					ViewableRecordCollection vrcColl = this.ChildRecords.ViewableRecords;
					count = null != vrcColl ? vrcColl.CountOfNonSpecialRecords : this.ChildRecords.Count;
				}

                // JJD 4/10/08
                // Use the DP's culture
                //if ( count == 1)
                //    format = SR.GetString("GroupByDescription_Format_OneChild");
                //else
                //    format = SR.GetString("GroupByDescription_Format_NotOneChild");
                if (count == 1)
                    format = DataPresenterBase.GetString(culture, "GroupByDescription_Format_OneChild");
                else
                    format = DataPresenterBase.GetString(culture, "GroupByDescription_Format_NotOneChild");

                try
                {
                    // JJD 4/10/08
                    // Use the DP's culture
                    //return string.Format(format, new object[] { baseValue, count.ToString() });
                    return string.Format(culture, format, new object[] { baseValue, count });
                }
                catch (FormatException)
                {
                    return format;
                }
            }

            return string.Empty;
        }

				#endregion // GetDescriptionWithoutSummariesHelper

				#region GetDescriptionWithSummaries

		internal string GetDescriptionWithSummaries( )
		{
			string description = this.Description;

			FieldLayout fl = this.FieldLayout;
			if ( null != fl && GroupBySummaryDisplayMode.Text == fl.GroupBySummaryDisplayModeResolved )
			{
				string summariesText = this.GetSummariesText( ", " );
				if ( !string.IsNullOrEmpty( summariesText ) )
					description += " " + summariesText;
			}

			return description;
		}

				#endregion // GetDescriptionWithSummaries

				#region GetSummaries

		
		
		/// <summary>
		/// Returns summary results that are to be displayed inside the group-by record.
		/// </summary>
		/// <returns>Returns summary results enumerable, can be null if no matching items found.</returns>
		internal IEnumerable<SummaryResult> GetSummaryResults( )
		{
			RecordCollection childRecords = this.ChildRecords;
			if ( null != childRecords )
			{
				SummaryResultCollection summaries = childRecords.SummaryResults;
				if ( null != summaries )
					return SummaryDisplayAreaContext.InGroupByRecordsSummariesContext.Filter( summaries );
			}

			return null;
		}

				#endregion // GetSummaries

				#region GetSummariesText

		
		
		/// <summary>
		/// Returns the summary results that are to be displayed in the group-by record in the form of text.
		/// Multiple summary results will be concatenated using the specified separator.
		/// </summary>
		/// <param name="separator">Separator to use to concatenate multiple summary results.</param>
		/// <returns>Returns the summary results as text.</returns>
		internal string GetSummariesText( string separator )
		{
			IEnumerable<SummaryResult> gbrResults = this.GetSummaryResults( );
			if ( null != gbrResults )
			{
				StringBuilder sb = null;

				foreach ( SummaryResult result in gbrResults )
				{
					string displayText = result.DisplayText;
					Field sourceField = result.SourceField;

					
					
					
					string iiSeparator = separator;
					if ( null == sb )
					{
						sb = new StringBuilder( );
						iiSeparator = string.Empty;
					}
					
					if ( null != sourceField && !string.IsNullOrEmpty( displayText ) )
					{
						
						
						
						
						
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

						sb.Append( iiSeparator );
						

						string sourceFieldLabel = GridUtilities.ToString( sourceField.Label, sourceField.Name );
						sb.Append( sourceFieldLabel ).Append( " " ).Append( displayText );
					}
					else
					{
						// SSP 9/29/11 Calc
						// For formula summaries there's no source field or calculator.
						// 
						//Debug.Assert( false );
						if ( null != displayText )
							sb.Append( iiSeparator ).Append( displayText );
					}
				}

				if ( null != sb )
				{
					// Replace the new line characters with space for group by row's captions.
					//
					sb.Replace( "\r\n", " " );
					sb.Replace( '\n', ' ' );
					sb.Replace( '\r', ' ' );

					return sb.ToString( );
				}
			}

			return null;
		}
 
				#endregion // GetSummariesText

                // JJD 2/29/08 - added
                #region GetFirstDescendantDataRecord

        internal DataRecord GetFirstDescendantDataRecord()
        {
            if (this.ChildRecords.Count == 0)
            {
                Debug.Fail("A GroupbyRecord should always have at least 1 child record");
                return null;
            }
            Record record = this.ChildRecords[0];

            if (record is DataRecord)
                return (DataRecord)record;

            GroupByRecord gbr = record as GroupByRecord;

            if (gbr != null)
                return gbr.GetFirstDescendantDataRecord();

            Debug.Fail(string.Format("A GroupbyRecord can contain only DataRecords or GroupByrecords, not {0}", record.GetType().ToString()));

            return null;
        }

                #endregion //GetFirstDescendantDataRecord	

				#region GetSummaries

		
		
		/// <summary>
		/// Returns true if there are any summary results that will be displayed in the group-by record.
		/// </summary>
		/// <returns>Returns true if there are any summary results that will be displayed in the group-by record.</returns>
		internal bool HasSummaryResults( )
		{
			return GridUtilities.HasItems( this.GetSummaryResults( ) );
		}

				#endregion // GetSummaries
    
                #region InitializeCommonValue

        internal void InitializeCommonValue(object commonValue)
        {
            this._commonValue = commonValue;
        }

                #endregion //InitializeCommonValue	

				// JM 05-30-08 BR33286 - Raise a PropertyChanged notification for our Description property when
				// the grouping changes.
				#region OnGroupingChanged

		internal void OnGroupingChanged()
		{
			if ( this._dynamicDesriptionString == null && this.HasListeners )
			{
				this.RaisePropertyChangedEvent( "Description" );
				// SSP 6/2/08 - Summaries Functionality
				// Added DescriptionWithSummaries property.
				// 
				this.DirtyDescriptionWithSummaries( );
			}
		}

				#endregion //OnGroupingChanged

				// AS 6/18/09 NA 2009.2 Field Sizing
				#region ShouldDisplaySummaries
		internal static bool ShouldDisplaySummaries(GroupByRecord groupByRecord)
		{
			FieldLayout fl = null != groupByRecord ? groupByRecord.FieldLayout : null;
			GroupBySummaryDisplayMode groupBySummaryDisplayMode = null != fl
				? fl.GroupBySummaryDisplayModeResolved : GroupBySummaryDisplayMode.Default;

			bool shouldDisplaySummaries = //GroupBySummaryDisplayMode.SummaryCells == groupBySummaryDisplayMode || 
				GroupBySummaryDisplayMode.SummaryCellsAlwaysBelowDescription == groupBySummaryDisplayMode;

			if (shouldDisplaySummaries)
				shouldDisplaySummaries = null != groupByRecord && groupByRecord.HasSummaryResults();

			return shouldDisplaySummaries;
		}
				#endregion //ShouldDisplaySummaries

			#endregion //Internal Methods

		#endregion //Methods
	}

    #endregion //GroupByRecord class

	// AS 6/24/09 NA 2009.2 Field Sizing
	// Created separate class for the template data record to make it easier to store/provide temporary
	// data. Plus it makes it more effecient to not have to check for templatedatarecord within the 
	// datarecord class.
	//
	#region TemplateDataRecord
	internal class TemplateDataRecord : DataRecord
	{
		#region Member Variables

		private Dictionary<Field, object> _values;
		private int _lastCacheVersion;

		#endregion //Member Variables

		#region Constructor

		internal TemplateDataRecord(FieldLayout fieldLayout, RecordCollectionBase parentCollection)
			: base(fieldLayout, parentCollection)
		{
			_values = new Dictionary<Field, object>();
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetCellValue
		public override object GetCellValue(Field field, bool useConverter)
		{
			GridUtilities.ValidateNotNull(field);

			if (field.Owner != this.FieldLayout)
				throw new ArgumentException();

			object value;
			
			if (!_values.TryGetValue(field, out value))
				// JJD 1/4/12 - TFS98294 - Optimization
				// Added dataType paramater
				//value = Field.GetDefaultDataForType(field.EditAsTypeResolved);
				value = Field.GetDefaultDataForType(field.EditAsTypeResolved, field.DataType);

			return value;
		}
		#endregion //GetCellValue

		#region IsTemplateDataRecord
		internal override bool IsTemplateDataRecord
		{
			get
			{
				return null != FieldLayout && FieldLayout.TemplateDataRecord == this;
			}
		}
		#endregion //IsTemplateDataRecord

		#region SetCellValue
		// SSP 3/19/12 - Calc manager support
		// Added an overload that takes 'dontSetDataChanged' flag.
		// 
		//internal override bool SetCellValue(Field field, object value, bool useConverter, out DataErrorInfo errorInfo)
		internal override bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo, bool dontSetDataChanged )
		{
			errorInfo = null;
			return false;
		}
		#endregion //SetCellValue

		// JJD 10/20/11  - TFS84708 - added
		#region ViewableChildRecordsIfNeeded

		internal override ViewableRecordCollection ViewableChildRecordsIfNeeded
		{
			get
			{
				return null;
			}
		}

		#endregion //ViewableChildRecordsIfNeeded	

		#endregion //Base class overrides

		#region Methods
		internal void ClearAll()
		{
			Field[] fields = new Field[_values.Keys.Count];
			_values.Keys.CopyTo(fields, 0);

			foreach (Field field in fields)
			    ClearValue(field);
		}

		internal void ClearValue(Field field)
		{
			if (_values.ContainsKey(field))
			{
				_values.Remove(field);
				UpdateCell(field);
			}
		}

		internal void SetValue(Field field, object value)
		{
			_values[field] = value;
			UpdateCell(field);
		}

		private void UpdateCell(Field field)
		{
			Cell cell = this.Cells[field];
			CellValuePresenter cvp = cell.AssociatedCellValuePresenterInternal;

			if (null == cvp)
			{
				cvp = AutoSizeFieldHelper.GetCellPresenter(field, this.FieldLayout.TemplateDataRecordCache);
				Debug.Assert(null != cvp && cvp.Field == field);
				cell.AssociatedCellValuePresenterInternal = cvp;
			}

			if (null != cvp)
				cvp.SetValueInternal(this.GetCellValue(field), false);
		}

		#region VerifyCachedCellElements
		internal void VerifyCachedCellElements()
		{
			FieldLayout fl = this.FieldLayout;

			// if the template cache has changed then make sure we release 
			// our references to any CVPs since they may be "old"
			//
			if (_lastCacheVersion != fl.TemplateDataRecordCache.CacheVersion)
			{
				_lastCacheVersion = fl.TemplateDataRecordCache.CacheVersion;

				FieldCollection fields = fl.Fields;

				for (int i = 0, count = fields.Count; i < count; i++)
				{
					Cell c = this.GetCellIfAllocated(fields[i]);

					if (c != null)
						c.AssociatedCellValuePresenterInternal = null;
				}
			}
		}
		#endregion //VerifyCachedCellElements
		#endregion //Methods
	} 
	#endregion //TemplateDataRecord

	// SSP 7/23/09 - NAS9.2
	// Added HeaderRecord class.
	// 
	#region HeaderRecord Class

	/// <summary>
	/// A record that displays field labels.
	/// </summary>
	public class HeaderRecord : DataRecord
	{
		#region Member Vars

        private WeakReference _attachedToRecord;
        private WeakReference _attachedToRecordPrevious;
        private int             _attachedNestingDepth; 

		internal bool _isPlaceHolder;

		// JJD 11/11/11 - TFS91364
		// In the case where this header is a placeholder inside a ViewableRecordCollection whose
		// records are all filtered out we need to hold a hard reference on the TemplateDataRecord
		// placeholder since the _attachedToRecord is a weak reference
		
		
		internal TemplateDataRecord _vrcPlaceholderRecord;

		#endregion // Member Vars

		#region Constructors

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="HeaderRecord"/> class.
		/// </summary>
		/// <param name="fieldLayout">Associated field layout.</param>
		/// <param name="parentCollection">Parent record collection.</param>
		internal HeaderRecord( FieldLayout fieldLayout, RecordCollectionBase parentCollection )
			: base( fieldLayout, parentCollection )
        {
        }

        #endregion //Constructors

        #region Base class overrides

		// JJD 09/22/11  - TFS84708 - Optimization
		#region ChildRecordsIfNeeded

		internal override RecordCollectionBase ChildRecordsIfNeeded { get { return null; } }

		#endregion //ChildRecordsIfNeeded

        #region ChildRecordsInternal

        internal override RecordCollectionBase ChildRecordsInternal { get { return null; } }

        #endregion //ChildRecordsInternal	

		#region CreateCellCollection

		/// <summary>
		/// Overridden. Creates a new FilterCellCollection.
		/// </summary>
		/// <returns>A new FilterCellCollection instance.</returns>
		internal override CellCollection CreateCellCollection( )
		{
			return null;
		}

		#endregion // CreateCellCollection

        #region Description

        /// <summary>
        /// Gets/sets the description for the row
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Description
        {
            get
            {
				if ( ! string.IsNullOrEmpty( base.Description ) )
					return base.Description;

				return "Header record";
            }
            set
            {
                base.Description = value;
            }
        }

        #endregion // Description

        #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// HeaderRecords will always return null from this method.
        /// </summary>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
			Debug.Assert( false );
			return null;
        }

        #endregion // GetAssociatedRecord

		#region GetCellValue

		/// <summary>
		/// Returns the value of a filter cell.
		/// </summary>
		/// <param name="field">Value of the filter cell associated with this field will be returned.</param>
		/// <param name="useConverter">Not used by the FilterRecord.</param>
		public override object GetCellValue( Field field, bool useConverter )
		{
			return this.GetCellValue( field, useConverter ? CellValueType.Converted : CellValueType.Raw );
		}

        /// <summary>
        /// Returns the value of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <param name="cellValueType">The type of value to return</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public override object GetCellValue(Field field, CellValueType cellValueType)
        {
			return field.Label;
        }

		#endregion // GetCellValue

		#region GetEditorTypeResolved

		internal override Type GetEditorTypeResolved(Field field)
		{
			return null;
		}

		#endregion // GetEditorTypeResolved 

		#region GetIsEditingAllowed

		internal override bool GetIsEditingAllowed(Field field)
		{
			return false;
		} 

		#endregion // GetIsEditingAllowed

		#region HasChildrenInternal

        internal override bool HasChildrenInternal 
        { 
            get
            {
                return false;
            } 
        }

        #endregion //HasChildrenInternal	

		#region IsDataRecord

		/// <summary>
		/// Overridden. Returns false since this is not a data record.
		/// </summary>
		// JJD 10/26/11 - TFS91364 - Make property public
		//internal override bool IsDataRecord
		public override bool IsDataRecord
		{
			get
			{
				return false;
			}
		}

		#endregion // IsDataRecord

		#region IsSelectable

        /// <summary>
        /// Property: Returns true only if the record can be selected
        /// </summary>
        internal protected override bool IsSelectable
        {
            get
            {
                 return false;
            }
        }

        #endregion // IsSelectable

		#region IsSpecialRecord

		// SSP 8/5/09 - NAS9.2 Enhanced grid view
		// Made public.
		// 
		//internal override bool IsSpecialRecord
		/// <summary>
		/// Overridden. Returns true since the HeaderRecord is a special record.
		/// </summary>
		public override bool IsSpecialRecord
		{
			get
			{
				return true;
			}
		}

		#endregion // IsSpecialRecord

        // JJD 8/13/09 - NA 2009 Vol 2 - Enhanced grid view
        #region RecordForLayoutCalculations

        internal override Record RecordForLayoutCalculations
        {
            get 
            { 
                Record attactedToRcd = this.AttachedToRecord;

                // JJD 11/10/09 - TFS24243
                // Get the correct logical record instead
                //return attactedToRcd != null ? attactedToRcd : this;
                if (attactedToRcd == null)
                    return this;

                Record record = attactedToRcd;

                ExpandableFieldRecord efr = record as ExpandableFieldRecord;

                // For ExpandableFieldRecords use their firat child record instead
                if (efr != null)
                {
					// JJD 09/22/11  - TFS84708 - Optimization
					// Use the ChildRecordManagerIfNeeded instead which won't create
					// child rcd managers for leaf records
					//RecordManager rm = efr.ChildRecordManager;
                    RecordManager rm = efr.ChildRecordManagerIfNeeded;

                    if (rm != null &&
                        rm.Unsorted.Count > 0)
                        record = rm.Unsorted[0];
                }

                // if the attached to record is a DataRecord or GroupByRecord and it is inside a groupby and
                // the HeaderPlacementInGroupBy is OnTopOnly then we want to get the indent
                // of this cdr'd top level group. This can happen when the top level group is scrolled
                // out of view
                switch (record.RecordType)
                {
					// JJD 2/7/11 - TFS35853
					// added case for when a headr rcd os attached to the GroupByFieldLayout record
					case RecordType.GroupByFieldLayout:
						{
							RecordCollectionBase children = record.ChildRecordsInternal;

							// JJD 2/7/11 - TFS35853
							// return the first 'non groupby' descendant
							while (children != null && children.Count > 0)
							{
								if (children[0] is GroupByRecord)
								{
									children = children[0].ChildRecordsInternal;
									continue;
								}

								return children[0];
							}

						}
						break;

                    case RecordType.DataRecord:
                    case RecordType.GroupByField:
                         {
                            FieldLayout fieldLayout = record.FieldLayout;

                            if (fieldLayout.HasGroupBySortFields &&
                                 fieldLayout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly)
                            {
                                Record topLevelGroup = record.TopLevelGroupByFieldRecord;

                                if (topLevelGroup != null)
                                    record = topLevelGroup;
                            }
                        }
                        break;

					// JJD 1/13/12 - TFS95800 - added case for summary records
 					case RecordType.SummaryRecord: 
                         {
                            FieldLayout fieldLayout = record.FieldLayout;

							if (fieldLayout.HasGroupBySortFields)
							{
								// if the HeaderPlacementInGroupBy is 'OnTopOnly' then
								// use the top level group by field record
								if (fieldLayout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly)
								{
									Record topLevelGroup = record.TopLevelGroupByFieldRecord;

									if (topLevelGroup != null)
										record = topLevelGroup;
								}
								else
								{
									// otherwise use any data record from the parent record manager
									// since we want the header to align the same way as the data records
									RecordManager rm = record.ParentCollection.ParentRecordManager;

									if (rm != null && rm.Unsorted.Count > 0)
										return rm.Unsorted[0];
								}
							}
                        }
                        break;
                }

                return record;
            }
        }

        #endregion //RecordForLayoutCalculations	
    
        #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RecordType RecordType 
        { 
            get 
            {
				return RecordType.HeaderRecord;
            } 
        }

        #endregion //RecordType

		#region SetCellValue

		/// <summary>
		/// Overridden. Sets the value of the filter cell.
		/// </summary>
		/// <param name="field">Value on the filter cell of this field will be set.</param>
		/// <param name="value">The new value.</param>
		/// <param name="useConverter">Not used for filter record.</param>
		/// <param name="errorInfo">Set to null since the filter record doesn't generated data errors</param>
		/// <param name="dontSetDataChanged">True to avoid setting the data changed flag</param>
		// SSP 3/19/12 - Calc manager support
		// Added an overload that takes 'dontSetDataChanged' flag.
		// 
		//internal override bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo )
		internal override bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo, bool dontSetDataChanged )
		{
			throw new InvalidOperationException( "Header record doesn't support setting values." );
		}

		#endregion //SetCellValue	

		// AS 10/13/09 NA 2010.1 - CardView
		#region ShouldCollapseCell
		internal override bool ShouldCollapseCell(Field field)
		{
			return false;
		}
		#endregion //ShouldCollapseCell

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			return this.Description;
		}

		#endregion //ToString	
 
		// JJD 10/20/11  - TFS84708 - added
		#region ViewableChildRecordsIfNeeded

		internal override ViewableRecordCollection ViewableChildRecordsIfNeeded
		{
			get
			{
				return null;
			}
		}

		#endregion //ViewableChildRecordsIfNeeded	

        #endregion Base class overrides

        #region Properties

        #region Public Properties
        
        #endregion //Public Properties

        #region Internal Properties

        #region AttachedToRecord

        internal int AttachedNestingDepth
        {
            get { return this._attachedNestingDepth; }
            set 
            {
                if (this._attachedNestingDepth != value)
                {
                    this._attachedNestingDepth = value;

                    // since the nesting depth changed we need 
                    // to clear the AttachedToRecordPrevious so 
                    // we know to trigger a re-calulation of the record
                    // contenjt margins
                    this.AttachedToRecordPrevious = null;
                }
            }
        }


        // JJD 11/10/09 - Made getter public
        //internal Record AttachedToRecord
        /// <summary>
        /// Returns the record that this header is attached to (read-only)
        /// </summary>
        public Record AttachedToRecord
        {
            get
            {
                if (this._attachedToRecord == null)
                    return null;

                Record rcd = Utilities.GetWeakReferenceTargetSafe(this._attachedToRecord) as Record;

                if (rcd == null)
                {
                    // clear the weak reference
                    this._attachedToRecord = null;

					// JJD 10/18/11 - TFS92271
					// Raise property changed notification asynchronously since the record
					// was garbage collected
					DataPresenterBase dp = this.DataPresenter;
					if (dp != null)
						dp.Dispatcher.BeginInvoke(new GridUtilities.MethodDelegate(this.OnAttachedToRecordChanged));

					return null;
                }

                return rcd;
            }
            internal set
            {
                if (value != this.AttachedToRecord)
                {
                    this._attachedToRecordPrevious = this._attachedToRecord;

					if (value == null)
					{
						this._attachedToRecord = null;
					}
					else
					{
						this._attachedToRecord = new WeakReference(value);
					}

					// JJD 10/17/11 - TFS92271
					// Raise property changed notification
					this.OnAttachedToRecordChanged();
                }
            }
        }
        
        internal Record AttachedToRecordPrevious
        {
            get
            {
                if (this._attachedToRecordPrevious == null)
                    return null;

                Record rcd = Utilities.GetWeakReferenceTargetSafe(this._attachedToRecordPrevious) as Record;

                if (rcd == null)
                {
					// clear the weak reference
                    this._attachedToRecordPrevious = null;
                    return null;
                }

                return rcd;
            }
            set
            {
                if (value != this.AttachedToRecordPrevious)
                {
					if (value == null)
					{
						this._attachedToRecordPrevious = null;
					}
					else
					{
						this._attachedToRecordPrevious = new WeakReference(value);
					}
                }
            }
        }

        #endregion //AttachedToRecord	
    
        #endregion //Internal Properties	

        #endregion //Properties

        #region Methods

        #region Internal Methods

		#endregion //Internal Methods

		#region Private Methods

		// JJD 10/17/11 - TFS92271 - added
		#region OnAttachedToRecordChanged

		private void OnAttachedToRecordChanged()
		{
			// JJD 10/17/11 - TFS92271
			// Raise property changed notification
			this.RaisePropertyChangedEvent("AttachedToRecord");
		}

		#endregion //OnAttachedToRecordChanged

		#endregion //Private Methods	
        
		#endregion //Methods
	}

	#endregion // HeaderRecord Class
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