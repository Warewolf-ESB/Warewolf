using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A control used inside a <see cref="DataRecordPresenter"/> to provide a UI for selecting a record.
	/// </summary>
	//[Description("A control used inside a DataRecordPresenter to provide a UI for selecting a record.")]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,       GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateChanged,         GroupName = VisualStateUtilities.GroupChange)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnchanged,       GroupName = VisualStateUtilities.GroupChange)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateFixed,           GroupName = VisualStateUtilities.GroupFixed)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfixed,         GroupName = VisualStateUtilities.GroupFixed)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateTop,             GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateBottom,          GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,            GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,           GroupName = VisualStateUtilities.GroupLocation)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateAddRecord,       GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecord,      GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilterRecord,    GroupName = VisualStateUtilities.GroupRecord)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateValidEx,         GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidEx,       GroupName = VisualStateUtilities.GroupValidationEx)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RecordSelector : ContentControl, IWeakEventListener
	{
		#region Member Variables

		private int _cachedVersion;
		private bool _versionInitialized;
        private bool _wasActive;
		private Record _record;
		private StyleSelectorHelper _styleSelectorHelper;
        private PropertyValueTracker _tracker;

        // JJD 6/10/09 - NA 2009 vol2 - Record fixing
        private PropertyValueTracker _trackerForFixedButton;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// JM 05-04-10 TFS31055 
		private bool				_activatedEventPending;

		// JJD 08/13/10 - TFS36324
		private bool _verifyActiveStatePending;

		#endregion Member Variables

		#region Constructors

		static RecordSelector()
		{
            // JJD 9/17/09 
            // Register a class handler for the MouseLeave event
            EventManager.RegisterClassHandler(typeof(RecordSelector), Mouse.MouseLeaveEvent, new MouseEventHandler( OnMouseLeave ));

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordSelector), new FrameworkPropertyMetadata(typeof(RecordSelector)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			//KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(RecordSelector), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(RecordSelector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
            DataRecordPresenter.HasDataErrorProperty.OverrideMetadata(typeof(RecordSelector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), DataRecordPresenter.HasDataErrorPropertyKey);

        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordSelector"/> class
		/// </summary>
		public RecordSelector()
		{
			
			// initialize the styleSelectorHelper
            this._styleSelectorHelper = new StyleSelectorHelper(this);

			
            
            
			





        }

        #region Test code



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


        #endregion //Test code

        #endregion Constructors

        #region Base class overrides

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

			// JJD 08/13/10 - TFS36324
			// If we bypassed a verififcation we should perform it now that the template has been applied
			if (this._verifyActiveStatePending)
				this.VerifyActiveState();


            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	
    
			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="RecordSelector"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.RecordSelectorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.RecordSelectorAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer
    
            #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseEnter

            #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseLeave

            #region OnPropertyChanged

        /// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == DataContextProperty)
			{
				Record record = e.NewValue as Record;

				if (record != this._record)
				{
					if (this._record != null)
					{
						// unhook the event listener for the old record
						PropertyChangedEventManager.RemoveListener(this._record, this, string.Empty);
					}

					// JJD 5/29/07
					// Allow element to be reused for other records
					//if ( this._record != null )
					//    this._record = null;
					//else 
						this._record = record;

					this.SetValue(RecordPropertyKey, this._record);

					// JJD 4/1/11 - TFS70662
					// Make sure the trackers are nulled out up front so we know we aren't listening
					// to any aold record's notifications
					this._tracker = null;
					this._trackerForFixedButton = null;

					if (this._record != null)
					{
						this.InitializeVersionInfo();
						this.SetValue(IsActivePropertyKey, KnownBoxes.FromValue(this._record.IsActive));

						DataRecord dr = this._record as DataRecord;

						if (dr != null)
						{
                            // JJD 6/10/09 - NA 2009 vol2 - Record fixing
                            // JJD 7/20/09 - TFS19291
                            // Track fieldlauot's RecordSelectorExtentResolved property which is alffected by changes
                            // to AllowRecordFixing and FixedRecordUIType
                            this._trackerForFixedButton = new PropertyValueTracker(this._record.FieldLayout, "RecordSelectorExtentResolved", this.OnRecordSelectorExtentChanged);
                            this.OnRecordSelectorExtentChanged();

                            // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
                            if (dr.RecordType == RecordType.FilterRecord)
                            {
                                this.SetValue(IsFilterRecordPropertyKey, KnownBoxes.TrueBox);
                                this.OnFilterVisibilityChanged();
                                this._tracker = new PropertyValueTracker(this._record.FieldLayout, "FilterClearButtonLocationResolved", this.OnFilterVisibilityChanged);
                            }
                            else
                            {
                                this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(dr.IsAddRecord));
                                this.SetValue(IsDataChangedPropertyKey, KnownBoxes.FromValue(dr.IsDataChanged));

								// JJD 4/1/11 - TFS70662
								// This was nulled out above
                                //this._tracker = null;

								// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
								// 
								this.UpdateDataError( );
                            }
						}

						// use the weak event manager to hook the event so we don't get rooted
						PropertyChangedEventManager.AddListener(this._record, this, string.Empty);

					}
				}
			}
			else if (e.Property == InternalVersionProperty)
			{
				this.InitializeVersionInfo();
			}
			else if (e.Property == IsVisibleProperty)
			{
				// JJD 3/31/11 - TFS70662 
				// When we are being made visibly re-verify the IsFixd allowed props since 
				// the record parent could have changed to or from null in which case
				// fixing on bottom may be allowed/disallowed. This can happen in a grouping
				// change.
				if ((bool)e.NewValue == true)
					this.VerifyIsFixedAllowedProperties();
			}
		}

			#endregion //OnPropertyChanged

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("RecordSelector: ");

			if (this.Record != null)
				sb.Append(this.Record.ToString());

			return sb.ToString();
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Events

			// JJD 9/16/09 - Added.
			#region ActivatedEvent

		/// <summary>
		/// Event ID for the <see cref="Activated"/> routed event
		/// </summary>
		public static readonly RoutedEvent ActivatedEvent =
			EventManager.RegisterRoutedEvent("Activated", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(RecordSelector));

		/// <summary>
        /// Occurs when the RecordSelector's record is activated.
        /// </summary>
		protected virtual void OnActivated(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseActivated(RoutedEventArgs args)
		{
			args.RoutedEvent	= RecordSelector.ActivatedEvent;
			args.Source			= this;
			this.OnActivated(args);
		}

		/// <summary>
        /// Occurs when the RecordSelector's record is activated.
        /// </summary>
		public event EventHandler<RoutedEventArgs> Activated
		{
			add
			{
				base.AddHandler(RecordSelector.ActivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RecordSelector.ActivatedEvent, value);
			}
		}

			#endregion //ActivatedEvent

			// JJD 9/16/09 - Added.
			#region DeactivatedEvent

		/// <summary>
		/// Event ID for the <see cref="Deactivated"/> routed event
		/// </summary>
		public static readonly RoutedEvent DeactivatedEvent =
			EventManager.RegisterRoutedEvent("Deactivated", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(RecordSelector));

		/// <summary>
		/// Occurs when the RecordSelector's record is de-activated.
		/// </summary>
		protected virtual void OnDeactivated(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDeactivated(RoutedEventArgs args)
		{
			args.RoutedEvent	= RecordSelector.DeactivatedEvent;
			args.Source			= this;
			this.OnDeactivated(args);
		}

		/// <summary>
		/// Occurs when the RecordSelector's record is de-activated.
		/// </summary>
		public event EventHandler<RoutedEventArgs> Deactivated
		{
			add
			{
				base.AddHandler(RecordSelector.DeactivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(RecordSelector.DeactivatedEvent, value);
			}
		}

			#endregion //DeactivatedEvent

		#endregion //Events

		#region Properties

			#region Public Properties

				#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the read-only <see cref="DataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty DataErrorProperty =
			DataRecordPresenter.DataErrorProperty.AddOwner( typeof( RecordSelector ) );

		/// <summary>
		/// Returns the associated data record's data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DataError</b> property returns the value of the associated DataRecord's
		/// <see cref="DataRecord.DataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.HasDataError"/>
		//[Description( "The record data error (IDataErrorInfo.Error)." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object DataError
		{
			get
			{
				return (object)this.GetValue( DataErrorProperty );
			}
		}

		internal void UpdateDataError( )
		{
			bool hasDataError = false;
			object dataError = null;

			DataRecord dr = this.Record as DataRecord;
			if ( null != dr )
			{
				dataError = dr.DataError;
				hasDataError = dr.HasDataError;
			}

			this.SetValue( DataRecordPresenter.HasDataErrorPropertyKey, KnownBoxes.FromValue( hasDataError ) );
			this.SetValue( DataRecordPresenter.DataErrorPropertyKey, dataError );

			UpdateDataErrorDisplayModeProperties( dr, this );
		}

		internal static void UpdateDataErrorDisplayModeProperties( DataRecord dr, DependencyObject elem )
		{
			FieldLayout fl = null != dr ? dr.FieldLayout : null;
			DataErrorDisplayMode displayMode = null != fl ? fl.DataErrorDisplayModeResolved : DataErrorDisplayMode.None;
			elem.SetValue( IsDataErrorDisplayModeIconPropertyKey,
				KnownBoxes.FromValue( DataErrorDisplayMode.ErrorIcon == displayMode
				|| DataErrorDisplayMode.ErrorIconAndHighlight == displayMode ) );

			elem.SetValue( IsDataErrorDisplayModeHighlightPropertyKey,
				KnownBoxes.FromValue( DataErrorDisplayMode.Highlight == displayMode
				|| DataErrorDisplayMode.ErrorIconAndHighlight == displayMode ) );
		}

				#endregion // DataError

				#region FieldLayout

		/// <summary>
		/// Identifies the 'FieldLayout' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutProperty = DependencyProperty.Register("FieldLayout",
				  typeof(FieldLayout), typeof(RecordSelector), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFieldLayoutChanged)));

		private static void OnFieldLayoutChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RecordSelector rs = target as RecordSelector;

			if (rs != null)
			{
				rs._cachedFieldLayout = e.NewValue as FieldLayout;
				rs.InitializeVersionInfo();
			}
		}

		private FieldLayout _cachedFieldLayout = null;

		/// <summary>
		/// Returns the associated field layout
		/// </summary>
		//[Description("Returns the associated field layout")]
		//[Category("Behavior")]
		public FieldLayout FieldLayout
		{
			get
			{
				return this._cachedFieldLayout;
			}
			set
			{
				this.SetValue(RecordSelector.FieldLayoutProperty, value);
			}
		}

				#endregion //FieldLayout

                // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
                #region FilterClearButtonVisibility

        private static readonly DependencyPropertyKey FilterClearButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("FilterClearButtonVisibility",
            typeof(Visibility), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

        /// <summary>
        /// Identifies the <see cref="FilterClearButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterClearButtonVisibilityProperty =
            FilterClearButtonVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Gete the visibility of the filter clear button (read-only)
        /// </summary>
        /// <seealso cref="FilterClearButtonVisibilityProperty"/>
        //[Description("Gete the visibility of the filter clear button (read-only)")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Visibility FilterClearButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(RecordSelector.FilterClearButtonVisibilityProperty);
            }
        }

                #endregion //FilterClearButtonVisibility

                // JJD 6/10/09 NA 2009 Vol 2 - Record Fixing
                #region FixedButtonVisibility

        private static readonly DependencyPropertyKey FixedButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("FixedButtonVisibility",
            typeof(Visibility), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

        /// <summary>
        /// Identifies the <see cref="FixedButtonVisibility"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty FixedButtonVisibilityProperty =
            FixedButtonVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates the visibility of the FixedRecordButton within the element based on the resolved FixedRecordUIType and the AllowRecordFixing of the FieldLayout.
        /// </summary>
        /// <seealso cref="FixedButtonVisibilityProperty"/>
        //[Description("Indicates the visibility of the FixedRecordButton within the element based on the resolved FixedRecordUIType and the AllowRecordFixing of the FieldLayout.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public Visibility FixedButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(RecordSelector.FixedButtonVisibilityProperty);
            }
        }

                #endregion //FixedButtonVisibility

				#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the read-only <see cref="HasDataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty HasDataErrorProperty
			= DataRecordPresenter.HasDataErrorProperty.AddOwner( typeof( RecordSelector ) );

		/// <summary>
		/// Indicates if the associated data record has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property returns the value of the associated DataRecord's
		/// <see cref="DataRecord.HasDataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.HasDataError"/>
		/// <seealso cref="CellValuePresenter.DataError"/>
		//[Description( "Indicates if the record has data error (IDataErrorInfo.Error)." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool HasDataError
		{
			get
			{
				return (bool)this.GetValue( HasDataErrorProperty );
			}
		}

				#endregion // HasDataError

				#region IsActive

		private static readonly DependencyPropertyKey IsActivePropertyKey =
			DependencyProperty.RegisterReadOnly("IsActive",
			typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged)));

        // JJD 9/16/09 - Added.
        private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RecordSelector rs = target as RecordSelector;

            if (rs != null)
            {
                rs.VerifyActiveState();

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                rs.UpdateVisualStates();

            }
        }

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty =
			IsActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the record is the <see cref="DataPresenterBase.ActiveRecord"/> (read-only)
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns true if the record is the active record in the control (read-only)")]
		//[Category("Behavior")]
		[ReadOnly(true)]
		[Bindable(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(RecordSelector.IsActiveProperty);
			}
		}

				#endregion //IsActive

				#region IsAddRecord

		private static readonly DependencyPropertyKey IsAddRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsAddRecord",
			typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

            ));

		/// <summary>
		/// Identifies the <see cref="IsAddRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddRecordProperty =
			IsAddRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the associated <see cref="Record"/> is an add record.
		/// </summary>
		/// <seealso cref="IsAddRecordProperty"/>
		//[Description("Returns true if the associated 'Record' is an add record.")]
		//[Category("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsAddRecord
		{
			get
			{
				return (bool)this.GetValue(RecordSelector.IsAddRecordProperty);
			}
		}

				#endregion //IsAddRecord

				#region IsDataChanged

		private static readonly DependencyPropertyKey IsDataChangedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsDataChanged",
			typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

            ));

		/// <summary>
		/// Identifies the <see cref="IsDataChanged"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDataChangedProperty =
			IsDataChangedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if cell data of the associated <see cref="Record"/> has been edited and have not yet been commited to the data source.
		/// </summary>
		/// <seealso cref="IsDataChangedProperty"/>
		//[Description("Returns true if cell data of the associated 'Record' has been edited and have not yet been commited to the data source.")]
		//[Category("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsDataChanged
		{
			get
			{
				return (bool)this.GetValue(RecordSelector.IsDataChangedProperty);
			}
		}

				#endregion //IsDataChanged

				#region IsDataErrorDisplayModeHighlight

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDataErrorDisplayModeHighlight"/> dependency property.
		/// </summary>
		internal static readonly DependencyPropertyKey IsDataErrorDisplayModeHighlightPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDataErrorDisplayModeHighlight",
			typeof( bool ),
			typeof( RecordSelector ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDataErrorDisplayModeHighlight"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsDataErrorDisplayModeHighlightProperty = IsDataErrorDisplayModeHighlightPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the record selector is to be highlighted if it the record has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property reflects the setting of <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
		/// If it's set to <i>Highlight</i> or <i>ErrorIconAndHighlight</i>, this property will return true.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="DataPresenterBase.DataErrorContentTemplateKey"/>
		/// <seealso cref="DataPresenterBase.DataErrorIconStyleKey"/>
		//[Description( "Indicates if the record selector is to be highlighted if it the record has data error." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool IsDataErrorDisplayModeHighlight
		{
			get
			{
				return (bool)this.GetValue( IsDataErrorDisplayModeHighlightProperty );
			}
		}

				#endregion // IsDataErrorDisplayModeHighlight

				#region IsDataErrorDisplayModeIcon

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDataErrorDisplayModeIcon"/> dependency property.
		/// </summary>
		internal static readonly DependencyPropertyKey IsDataErrorDisplayModeIconPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDataErrorDisplayModeIcon",
			typeof( bool ),
			typeof( RecordSelector ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDataErrorDisplayModeIcon"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsDataErrorDisplayModeIconProperty = IsDataErrorDisplayModeIconPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the record selector is to display an error icon if the record has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property reflects the setting of <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
		/// If it's set to <i>ErrorIcon</i> or <i>ErrorIconAndHighlight</i>, this property will return true.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="DataPresenterBase.DataErrorContentTemplateKey"/>
		/// <seealso cref="DataPresenterBase.DataErrorIconStyleKey"/>
		//[Description( "Indicates if the record selector is to display an error icon if the record has data error." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool IsDataErrorDisplayModeIcon
		{
			get
			{
				return (bool)this.GetValue( IsDataErrorDisplayModeIconProperty );
			}
		}

				#endregion // IsDataErrorDisplayModeIcon

                // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
				#region IsFilterRecord

		private static readonly DependencyPropertyKey IsFilterRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFilterRecord",
			typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsFilterRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFilterRecordProperty =
			IsFilterRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the associated <see cref="Record"/> is a <see cref="FilterRecord"/>.
		/// </summary>
		/// <seealso cref="IsFilterRecordProperty"/>
        //[Description("Returns true if the associated 'Record' is a filter record.")]
        //[Category("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFilterRecord
		{
			get
			{
				return (bool)this.GetValue(RecordSelector.IsFilterRecordProperty);
			}
		}

				#endregion //IsFilterRecord

                // JJD 6/12/09 NA 2009 Vol 2 - Record Fixing
                #region IsFixedOnBottomAllowed

        private static readonly DependencyPropertyKey IsFixedOnBottomAllowedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFixedOnBottomAllowed",
            typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsFixedOnBottomAllowed"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty IsFixedOnBottomAllowedProperty =
            IsFixedOnBottomAllowedPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates whether fixing to the bottom edge is allowed for this record.
        /// </summary>
        /// <seealso cref="IsFixedOnBottomAllowedProperty"/>
        /// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
        //[Description("Indicates whether fixing to the bottom edge is allowed for this record.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public bool IsFixedOnBottomAllowed
        {
            get
            {
                return (bool)this.GetValue(RecordSelector.IsFixedOnBottomAllowedProperty);
            }
        }

                #endregion //IsFixedOnBottomAllowed

                // JJD 6/12/09 NA 2009 Vol 2 - Record Fixing
                #region IsFixedOnTopAllowed

        private static readonly DependencyPropertyKey IsFixedOnTopAllowedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFixedOnTopAllowed",
            typeof(bool), typeof(RecordSelector), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsFixedOnTopAllowed"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty IsFixedOnTopAllowedProperty =
            IsFixedOnTopAllowedPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates whether fixing to the top edge is allowed for this record.
        /// </summary>
        /// <seealso cref="IsFixedOnTopAllowedProperty"/>
        /// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
        //[Description("Indicates whether fixing to the top edge is allowed for this record.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
        public bool IsFixedOnTopAllowed
        {
            get
            {
                return (bool)this.GetValue(RecordSelector.IsFixedOnTopAllowedProperty);
            }
        }

                #endregion //IsFixedOnTopAllowed

				#region Location

		private static readonly DependencyPropertyKey LocationPropertyKey =
			DependencyProperty.RegisterReadOnly("Location",
			typeof(RecordSelectorLocation), typeof(RecordSelector), new FrameworkPropertyMetadata(RecordSelectorLocation.LeftOfCellArea

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="Location"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LocationProperty =
			LocationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the location of the RecordSelector in relation to the cell area (read-only)
		/// </summary>
		/// <seealso cref="LocationProperty"/>
		//[Description("Returns the location of the RecordSelector in relation to the cell area (read-only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public RecordSelectorLocation Location
		{
			get
			{
				return (RecordSelectorLocation)this.GetValue(RecordSelector.LocationProperty);
			}
		}

				#endregion //Location

				#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey =
			DependencyProperty.RegisterReadOnly("Orientation",
			typeof(Orientation), typeof(RecordSelector), new FrameworkPropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the orientation (vertical/horizontal) of the RecordSelectors in the containing Panel.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns the orientation (vertical/horizontal) of the RecordSelectors in the containing Panel.")]
		//[Category("Appearance")]
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(RecordSelector.OrientationProperty); }
		}

				#endregion //Orientation

				#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly("Record",
			typeof(Record), typeof(RecordSelector), new FrameworkPropertyMetadata(

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'Record' dependency property
		/// </summary>
		public static readonly DependencyProperty RecordProperty =
			RecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated <see cref="Record"/> (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		//[Description("Returns the associated record inside a DataPresenterBase (read-only)")]
		//[Category("Data")]
		public Record Record
		{
			get
			{
				return this._record;
			}
		}

				#endregion //Record

			#endregion //Public Properties	

			#region Internal Properties

				#region InternalVersion

		internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
			typeof(int), typeof(RecordSelector), new FrameworkPropertyMetadata(0));

		internal int InternalVersion
		{
			get
			{
				return (int)this.GetValue(RecordSelector.InternalVersionProperty);
			}
			set
			{
				this.SetValue(RecordSelector.InternalVersionProperty, value);
			}
		}

				#endregion //InternalVersion

			#endregion //Internal Properties	
        
		#endregion //Properties	
		
		#region Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // set fixed state
            Record rcd = this.Record;

            // set common state
            if (this.IsEnabled == false || (rcd != null && rcd.IsEnabledResolved == false))
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }

            // set active state
            if ( this.IsActive )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

            // set changed state
            if ( this.IsDataChanged )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateChanged, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnchanged, useTransitions);

            if ( rcd != null && rcd.FixedLocation != FixedRecordLocation.Scrollable )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFixed, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfixed, useTransitions);

            string state = null;

            // set location states
            switch (this.Location)
            {
                case RecordSelectorLocation.AboveCellArea:
                    state = VisualStateUtilities.StateTop;
                    break;
                case RecordSelectorLocation.BelowCellArea:
                    state = VisualStateUtilities.StateBottom;
                    break;
                case RecordSelectorLocation.LeftOfCellArea:
                    state = VisualStateUtilities.StateLeft;
                    break;
                case RecordSelectorLocation.RightOfCellArea:
                    state = VisualStateUtilities.StateRight;
                    break;
            }

            if (state != null)
                VisualStateManager.GoToState(this, state, useTransitions);

            // set record state
            if (this.IsFilterRecord)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilterRecord, useTransitions);
            else
            {
                if (this.IsAddRecord)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateAddRecord, useTransitions);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateDataRecord, useTransitions);
            }

            // set validation state
            if (this.HasDataError)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInvalidEx, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateValidEx, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RecordSelector rs = target as RecordSelector;

            if ( rs != null )
                rs.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected methods

			#region Private Methods

				#region InitializeVersionInfo

		private void InitializeVersionInfo()
		{
			if (this._cachedFieldLayout != null &&
				this._cachedFieldLayout.DataPresenter != null)
			{
				if (this._cachedFieldLayout.StyleGenerator != null)
				{
					int version = this.InternalVersion;

					if (this._cachedVersion != version)
					{
						this._cachedVersion = version;

						if (this._versionInitialized == true)
							this._styleSelectorHelper.InvalidateStyle();

						this.SetValue(RecordSelector.OrientationPropertyKey, KnownBoxes.FromValue(this._cachedFieldLayout.StyleGenerator.LogicalOrientation));
						this.SetValue(LocationPropertyKey, this._cachedFieldLayout.RecordSelectorLocationResolved);
					}

					this._versionInitialized = true;
				}
			}
		}

				#endregion //InitializeVersionInfo

                // JJD 6/10/09 - NA 2009 Vol 2 - Record Fixing
                #region OnRecordSelectorExtentChanged

        private void OnRecordSelectorExtentChanged()
        {
			// JJD 3/31/11 - TFS70662
			// Moved logic to helper method
			VerifyIsFixedAllowedProperties();
        }

   	            #endregion //OnRecordSelectorExtentChanged	

                // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
                #region OnFilterVisibilityChanged

        private void OnFilterVisibilityChanged()
        {
            if (this._record != null)
            {
                switch (this._record.FieldLayout.FilterClearButtonLocationResolved)
                {
                    case FilterClearButtonLocation.RecordSelector:
                    case FilterClearButtonLocation.RecordSelectorAndFilterCell:
                        this.SetValue(FilterClearButtonVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
                        break;
                    default:
                        this.ClearValue(FilterClearButtonVisibilityPropertyKey);
                        break;
                }
            }

        }

   	            #endregion //OnFilterVisibilityChanged	
        
                // JJD 9/17/09 - added 
                #region OnMouseLeave

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            RecordSelector rs = e.OriginalSource as RecordSelector;

            // JJD 9/17/09 
            // If the RecordSelector is Unloaded then mark the event as handled
            // to prevent an possible exception since we are now using event triggers
            // in the default style's template instead of property triggers
            if (rs != null &&
                 !rs.IsLoaded)
                e.Handled = true;
        }

                #endregion //OnMouseLeave	
    
				#region OnRecordPropertyChanged

		private void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DataRecord dr = sender as DataRecord;

			switch (e.PropertyName)
			{
				case "ActiveCell":
				case "IsActive":
					this.SetValue(IsActivePropertyKey, KnownBoxes.FromValue((((Record)sender).IsActive)));
					break;

				case "IsAddRecord":
                    if (dr != null)
                    {
                        this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(dr.IsAddRecord));

                        // JJD 6/24/09 - NA 2009 Vol 2 - RecordFixing
                        // Since we firce the FixedButtonisibility of an add record to hidden 
                        // we need to trigger its initialization be calling OnAllowRecordFixingChanged
						// JJD 3/31/11 - TFS70662 
						// Call the new helper method directly
						//this.OnRecordSelectorExtentChanged();
						this.VerifyIsFixedAllowedProperties();
                    }
					break;

				case "IsDataChanged":
					if ( dr != null )
						this.SetValue(IsDataChangedPropertyKey, KnownBoxes.FromValue(dr.IsDataChanged));
					break;


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                case "IsEnabledResolved":
                    this.UpdateVisualStates();
                    break;


				// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
				// 
				case "DataError":
					this.UpdateDataError( );
					break;

				case "ParentRecord":
					// JJD 4/1/11 - TFS70662
					// When the parent record changes we need to verify the IsFixedAllowed Properties
					this.VerifyIsFixedAllowedProperties();
					break;
			}
		}

				#endregion //OnRecordPropertyChanged	

				// JM 02-19-09 TFS14198 - Added.  The 2 Storyboards in the default Style for the RecordSelector which are kicked off
				//						  when the ControlTemplate Trigger for the IsActive property is hit, was rooting the RecordSelector
				//						  for some reason.  Clearing the value on the Unload (which forces the second storyboard to get
				//						  invoked clears up the problem.  I don't fully understand what's happening here - seems like it
				//						  might be a weakness in the WPF framework.
				#region OnUnloaded, OnLoaded

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded	-= new RoutedEventHandler(OnUnloaded);
			

			// JJD 9/16/09 
            // Set the wasActive flag to false so we don't raise the Deactivated event when we
            // clear the IsActiveProperty below
            bool wasActive = this._wasActive;
            this._wasActive = false;

			this.ClearValue(RecordSelector.IsActivePropertyKey);

            // JJD 05/05/10 - TFS31370
            // reset the flag so when the element gets reloaded it will raise
            // the appropriate active state event
            this._wasActive = wasActive;

			// JJD 9/16/09 
            // only wire the Loaded event if we are the active record
            if ( wasActive )
			    this.Loaded		+= new RoutedEventHandler(OnLoaded);

		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			// JM 05-04-10 TFS31055
			if (this._activatedEventPending)
			{
				this._activatedEventPending = false;
				this.RaiseActivated(new RoutedEventArgs());
			}

            
            
            
			
			this.Loaded		-= new RoutedEventHandler(OnLoaded);

            // JJD 05/05/10 - TFS31370
            // if the record isn't active then call VerifyActiveState so the
            // appropriate event will get raised
            //if (this._record != null && this._record.IsActive)
            if (this._record != null)
            {
                if (this._record.IsActive)
                    this.SetValue(RecordSelector.IsActivePropertyKey, KnownBoxes.TrueBox);
                else
                    this.VerifyActiveState();
            }
		}

				#endregion //OnUnloaded, OnLoaded

			    // JJD 9/16/09 - Added.
                #region VerifyActiveState

        private void VerifyActiveState()
        {
			// JJD 08/13/10 - TFS36324
			// We don't want to raise the event until the templat is applied
			if (VisualTreeHelper.GetChildrenCount(this) == 0)
			{
				this._verifyActiveStatePending = true;
				return;
			}

			this._verifyActiveStatePending = false;

            bool isActiveNow = this.IsActive;

            if (this._wasActive != isActiveNow)
            {
                this._wasActive = isActiveNow;

                if (isActiveNow)
                {
					// JM 05-04-10 TFS31055 - Only fire the Activated event if we are loaded - if we are not loaded, set a flag
					// and fire the event after we are loaded.
					//this.RaiseActivated(new RoutedEventArgs());

					//// for active records we want to wire either the loaded or the
					//// unloaded event
					//if (this.IsLoaded)
					//    this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
					//else
					//    this.Loaded += new RoutedEventHandler(this.OnLoaded);

					// for active records we want to wire either the loaded or the
					// unloaded event
					if (this.IsLoaded)
					{
						this.RaiseActivated(new RoutedEventArgs());
						this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
					}
					else
					{
						this._activatedEventPending = true;
						this.Loaded += new RoutedEventHandler(this.OnLoaded);
					}
				}
                else
                {
                    this.RaiseDeactivated(new RoutedEventArgs());

                    // Make sure we unwire the loaded/unloaded events
                    this.Unloaded -= new RoutedEventHandler(this.OnUnloaded);
                    this.Loaded -= new RoutedEventHandler(this.OnLoaded);
                }
            }
        }

                #endregion //VerifyActiveState	

				// JJD 3/31/11 - TFS70662 - added
				#region VerifyIsFixedAllowedProperties

		private void VerifyIsFixedAllowedProperties()
		{
			if (this._record != null)
			{
				bool isFixedOnTopAllowed = false;
				bool isFixedOnBottomAllowed = false;

				DataPresenterBase dp = this._record.DataPresenter;

				if (dp != null && !dp.IsReportControl)
				{
					if (this._record.RecordType == RecordType.DataRecord)
					{
						if (((DataRecord)this._record).IsAddRecord == false &&
							this._record.FieldLayout.FixedRecordUITypeResolved == FixedRecordUIType.Button)
						{
							switch (this._record.FieldLayout.AllowRecordFixingResolved)
							{
								case AllowRecordFixing.Top:
									isFixedOnTopAllowed = true;
									break;
								case AllowRecordFixing.TopOrBottom:
									isFixedOnTopAllowed = true;
									// We are only supporting fixing records on the bottom for roo records
									isFixedOnBottomAllowed = this._record.ParentRecord == null;
									break;
								case AllowRecordFixing.Bottom:

									
									
									
									isFixedOnBottomAllowed = true;
									break;
							}

							ViewBase view = dp.CurrentViewInternal;

							// JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
							// If the view doesn't support record fixing at this level
							// then set both stack variables to false
							if (view.IsFixedRecordsSupported == false ||
								(view.IsFixingSupportedForNestedRecords == false &&
									this._record.ParentRecord != null))
							{
								isFixedOnTopAllowed = false;
								isFixedOnBottomAllowed = false;
							}
						}
					}
				}

				if (isFixedOnTopAllowed || isFixedOnBottomAllowed)
				{
					
					// If fixing on top isn't allowed and the recrod is not a root record
					// then set visibility to hidden and set isFixedOnBottomAllowed back to false
					if (!isFixedOnTopAllowed && this._record.ParentRecord != null)
					{
						isFixedOnBottomAllowed = false;

						this.SetValue(RecordSelector.FixedButtonVisibilityPropertyKey, KnownBoxes.VisibilityHiddenBox);
					}
					else
					{
						// for filter records we need to reserve the space but we don't want to fix button to show
						Visibility vis = (this._record.RecordType == RecordType.FilterRecord)
							? Visibility.Hidden : Visibility.Visible;

						this.SetValue(RecordSelector.FixedButtonVisibilityPropertyKey, vis);
					}
				}
				else
				{
					this.ClearValue(RecordSelector.FixedButtonVisibilityPropertyKey);
				}

				if (isFixedOnBottomAllowed)
					this.SetValue(IsFixedOnBottomAllowedPropertyKey, KnownBoxes.TrueBox);
				else
					this.ClearValue(IsFixedOnBottomAllowedPropertyKey);

				if (isFixedOnTopAllowed)
					this.SetValue(IsFixedOnTopAllowedPropertyKey, KnownBoxes.TrueBox);
				else
					this.ClearValue(IsFixedOnTopAllowedPropertyKey);

			}
		}

				#endregion //VerifyIsFixedAllowedProperties	
        
			#endregion //Private Methods

		#endregion //Methods

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					this.OnRecordPropertyChanged(sender, args);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordSelector, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for RecordSelector, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private RecordSelector _rs;

			internal StyleSelectorHelper(RecordSelector rs) : base(rs)
			{
				this._rs = rs;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._rs == null)
						return null;

					FieldLayout fl = this._rs.FieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
							return dp.InternalRecordSelectorStyleSelector.SelectStyle(this._rs.DataContext, this._rs);
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class
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