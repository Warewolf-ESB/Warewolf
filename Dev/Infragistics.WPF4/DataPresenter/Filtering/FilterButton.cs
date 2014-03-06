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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Used for displaying a filter button inside each field's label. 
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// To actually show filter buttons inside field labels, set the <see cref="FieldLayoutSettings"/>'s <see cref="FieldLayoutSettings.FilterUIType"/>
    /// property to 'LabelIcons'.
    /// </para>
    /// </remarks>
    /// <seealso cref="FieldLayoutSettings.FilterUIType"/>
    /// <seealso cref="DataPresenterBase.RecordFilterDropDownPopulating"/>
    /// <seealso cref="DataPresenterBase.RecordFilterDropDownOpening"/>
    //[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	[TemplatePart(Name = "PART_FilterMenu", Type = typeof(MenuItem))] // AS - NA 11.2 Excel Style Filtering
	public class FilterButton : ToggleButton
		, IWeakEventListener // AS - NA 11.2 Excel Style Filtering
    {
		#region Private Vars

        private PropertyValueTracker _tracker;
        private ObservableCollectionExtended<FilterDropDownItem> _operands;
        private ResolvedRecordFilterCollection.FieldFilterInfo _fieldFilterInfo;

        // JJD 5/27/09 - TFS17583
        // Added flag so we know to bypass SelectedOperandChanged logic while we are
        // initializing the dropdown list
        private bool _bypassSelectedOperandChanged;

        // JJD 6/5/09 - TFS18044
        // Keep track of the the active record or cell
        private object _previouslyActiveElement;

		// JJD 06/29/10 - TFS32174 - added
		private ResolvedRecordFilterCollection.FilterDropDownItemLoader _loader;

		// AS - NA 11.2 Excel Style Filtering
		private MenuItem _filterMenuItem;

		#endregion // Private Vars

		#region Constructor

		static FilterButton( )
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( FilterButton ), new FrameworkPropertyMetadata( typeof( FilterButton ) ) );

            FrameworkElement.ToolTipProperty.OverrideMetadata(typeof(FilterButton), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceTooltip)));
            Control.IsTabStopProperty.OverrideMetadata(typeof(FilterButton), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
            FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(FilterButton), new FrameworkPropertyMetadata(null));
            
            // JJD 1/28/09 - TFS12533
            // If the filter button's Visibility changes to visible we want to call OnFilterVersionChanged
            // so we can initialize the HasActiveFilters property
            FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(FilterButton), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged)));

			// JJD 2/9/11 - TFS63916 
			// If the HeaderPresenter's inherited RecordManager changes we need to release and reverify the filer info
			HeaderPresenter.RecordManagerProperty.OverrideMetadata(typeof(FilterButton), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRecordManagerChanged)), HeaderPresenter.RecordManagerPropertyKey);

		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterButton"/>.
		/// </summary>
		public FilterButton( )
		{
		}

		#endregion // Constructor

        #region Base class overrides

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

			// AS - NA 11.2 Excel Style Filtering
			// Added menu item part.
			//
			_filterMenuItem = this.GetTemplateChild("PART_FilterMenu") as MenuItem;
			this.ClearClosedFilterMenu();

            // JJD 1/28/09 - TFS12533
            // If the filter button is visible then call OnFilterVersionChanged
            // so we can initialize the HasActiveFilters property
			if (this.Visibility == Visibility.Visible)
			{
				// Moved logic into VerifyFilterInfo
				VerifyFilterInfo();
			}
        }

            #endregion //OnApplyTemplate	
    
            #region OnChecked


        /// <summary>
        /// Called when the button is checked
        /// </summary>
        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);

            ResolvedRecordFilterCollection.FieldFilterInfo fi = this.FilterInfo;

            if (fi == null)
                return;

            Field fld = this.Field;

            if ( fld == null )
                return;

            DataPresenterBase dp = fld.DataPresenter;

            if ( dp == null )
                return;

            // JJD 6/8/09 - TFS18044
            // Keep track of the activecell/record so we know where
            // to shift focus to when the button is unchecked
            Cell activeCell = dp.ActiveCell;
            Record activeRecord = dp.ActiveRecord;
            this._previouslyActiveElement = activeCell != null ? (object)activeCell : (object)activeRecord;

            ObservableCollectionExtended<FilterDropDownItem> operands = this.Operands;

            // JJD 4/13/09 - TFS16570
            // Always re-populate the dropdown list in case any other field filters have
            // modified the list of potential operands
            //if (operands.Count == 0)
            
            // JJD 5/27/09 - TFS17583
            // Initialize flag so we know to bypass SelectedOperandChanged logic while we are
            // initializing the dropdown list
            this._bypassSelectedOperandChanged = true;

            try
            {
                bool includeFilteredOutValues = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ||
                    fi.Field.Owner.RecordFiltersLogicalOperatorResolved == LogicalOperator.Or;

				// JJD 06/29/10 - TFS32174 
				// Use the new loader instead

				// Call GetFilterDropDownItems off FieldFilterInfo
                // Note: this will raise the RecordFilterDropDownPopulating
				//List<FilterDropDownItem> list = this._fieldFilterInfo.GetFilterDropDownItems(false, includeFilteredOutValues);

				//// update the operands list we expose
				//operands.BeginUpdate();

				//// JJD 4/13/09 - TFS16570
				//// since we care re-populating the list make sure we clear it first
				//operands.Clear();

				//operands.AddRange(list);
				//operands.EndUpdate();
				if (this._loader != null)
					this._loader.Abort();

				// AS - NA 11.2 Excel Style Filtering
				//this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(Utilities.GetDescendantFromType<XamComboEditor>(this, true, null), this._fieldFilterInfo, operands, false);
				if (this.DropDownType == FilterLabelIconDropDownType.MultiSelectExcelStyle)
					this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(null, _fieldFilterInfo, null, false);
				else
					this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(Utilities.GetDescendantFromType<XamComboEditor>(this, true, null), this._fieldFilterInfo, operands, false);

				this._loader.PopulatePhase1();
				this._loader.PopulatePhase2(includeFilteredOutValues);

				// AS - NA 11.2 Excel Style Filtering
				var loader = _loader;
				if (_filterMenuItem != null && _loader.RootMenuItem != null)
				{
					_filterMenuItem.ClearValue(ItemsControl.ItemsSourceProperty);
					_filterMenuItem.DataContext = _loader.RootMenuItem;
				}

				// JJD 06/29/10 - TFS32174 
				// If the loader isn't done wire the Phase2Completed event
				if (this._loader.EndReached)
					this._loader = null;
				else
					this._loader.Phase2Completed += new EventHandler(OnDropdownLoadCompleted);

				// AS - NA 11.2 Excel Style Filtering
				if (loader.RootMenuItem != null)
				{
					this.InitializeMenuIsCheckedState(loader.RootMenuItem);

					// we want the mnemonics to always be visible in the filter menu...
					if (FieldMenuItem._showKeyboardCuesProperty != null && _filterMenuItem != null)
						_filterMenuItem.SetValue(FieldMenuItem._showKeyboardCuesProperty, KnownBoxes.TrueBox);
				}

                // Raise the RecordFilterDropDownOpening event
				// AS - NA 11.2 Excel Style Filtering
				//RecordFilterDropDownOpeningEventArgs args = new RecordFilterDropDownOpeningEventArgs(fi.Field, fi.RecordManager, operands, false);
				RecordFilterDropDownOpeningEventArgs args;
				
				if (this.DropDownType == FilterLabelIconDropDownType.MultiSelectExcelStyle)
					args = new RecordFilterDropDownOpeningEventArgs(fi.Field, fi.RecordManager, loader.RootMenuItem.Items, false);
				else
					args = new RecordFilterDropDownOpeningEventArgs(fi.Field, fi.RecordManager, operands, false);

                fi.RecordManager.DataPresenter.RaiseRecordFilterDropDownOpening(args);

				if (this._fieldFilterInfo != null && this._fieldFilterInfo.RecordFilter.CurrentUIOperand != null)
					this.SelectedOperand = fi.RecordFilter.CurrentUIOperand;
				else
				{
					// JJD 11/11/11 - TFS91364
					// Clear the SelectedOperand property since the RecordFilter doesn't have a CurrentUIOperand
					this.ClearValue(SelectedOperandProperty);
				}
            }
            finally
            {
                // JJD 5/27/09 - TFS17583
                // Reset the flag 
                this._bypassSelectedOperandChanged = false;
            }
        }


            #endregion //OnChecked

            #region OnPreviewMouseLeftButtonDown

        /// <summary>
        /// Called when the left mouse button is pressed
        /// </summary>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.IsChecked == false)
            {
                Field fld = this.Field;

                if (fld != null)
                {
                    DataPresenterBase dp = fld.DataPresenter;

                    if (dp != null)
                    {
                        // If the active cell is in edit mode then try to exit
                        // if the exit was cncelled eat the mouse message
                        if (dp.EndEditMode(true, false) == false)
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

            #endregion //OnPreviewMouseLeftButtonDown	
    
			// JJD 2/7/11 - TFS35853 - added
			#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if (property == IsVisibleProperty)
			{
				// JJD 2/7/11 - TFS35853
				// If the burron is being made visible then call VerifyFilterInfo
				// to make sure we are correctly initialized
				if (true == (bool)e.NewValue)
					this.VerifyFilterInfo();
			}
		}
    
			#endregion //OnPropertyChanged
 
            // JJD 6/8/09 - TFS18044 - added
            #region OnUnchecked


        /// <summary>
        /// Called when the button is unchecked
        /// </summary>
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);

			if (this._loader != null)
				this._loader.Abort();

			// AS - NA 11.2 Excel Style Filtering
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new GridUtilities.MethodDelegate(this.ClearClosedFilterMenu));

            // JJD 6/8/09 - TFS18044
            // If the keyboard focu is till within us we need to shift that focus 
            // away from any contained element in our tree
            if (this.IsKeyboardFocusWithin)
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GridUtilities.MethodDelegate(this.ShiftFocusAway));
        }


            #endregion //OnUnchecked	
    
        #endregion //Base class overrides	
    
        #region Properties

		    #region Public Properties

				// AS - NA 11.2 Excel Style Filtering
				#region DropDownType

		private static readonly DependencyPropertyKey DropDownTypePropertyKey =
			DependencyProperty.RegisterReadOnly("DropDownType",
			typeof(FilterLabelIconDropDownType), typeof(FilterButton), new FrameworkPropertyMetadata(FilterLabelIconDropDownType.Default));

		/// <summary>
		/// Identifies the <see cref="DropDownType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownTypeProperty =
			DropDownTypePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an enumeration indicating the type of dropdown that should be displayed by the FilterButton
		/// </summary>
		/// <seealso cref="DropDownTypeProperty"/>
		[Description("Returns an enumeration indicating the type of dropdown that should be displayed by the FilterButton")]
		[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public FilterLabelIconDropDownType DropDownType
		{
			get
			{
				return (FilterLabelIconDropDownType)this.GetValue(FilterButton.DropDownTypeProperty);
			}
		}

				#endregion //DropDownType

		        #region Field

		/// <summary>
		/// Identifies the <see cref="Field"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldProperty = DependencyProperty.Register(
			"Field",
			typeof( Field ),
			typeof( FilterButton ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback(OnFieldChanged) )
			);

        private static void OnFieldChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FilterButton fb = target as FilterButton;

            if (fb != null)
            {
				Field oldField = e.OldValue as Field;
				Field newField = e.NewValue as Field;

				// AS - NA 11.2 Excel Style Filtering
				if (null != oldField)
					PropertyChangedEventManager.RemoveListener(oldField, fb, "FilterLabelIconDropDownTypeResolved");

				if (null != newField)
					PropertyChangedEventManager.AddListener(newField, fb, "FilterLabelIconDropDownTypeResolved");

				// AS - NA 11.2 Excel Style Filtering
				fb.VerifyDropDownType();

				
				
				
				
				
				

				// JJD 2/9/11 - TFS35853
				// Moved logic to ReleaseFilterInfo
				//fb._fieldFilterInfo = null;
				//fb._tracker = null;
				//fb.Operands.Clear();
				fb.ReleaseFilterInfo();
				
				// JJD 5/24/11 - TFS76604
				// Make sure we verify filter info for the new Field if we are 
				// recycling (i.e the OldValue != null and we are visible.
				// Othwerwise VerifyFilterInfo will be called when we are made visible
				if (e.OldValue != null && fb.IsVisible)
					fb.VerifyFilterInfo();
			}
        }

		/// <summary>
		/// Gets the associated field whose label is displaying this filter button inside it.
		/// </summary>
        //[Description("Gets or sets the associated field whose label is displaying this filter button inside it.")]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Field Field
		{
			get
			{
				return (Field)this.GetValue( FieldProperty );
			}
			set
			{
				this.SetValue( FieldProperty, value );
			}
		}

		        #endregion // Field

                #region HasActiveFilters

        internal static readonly DependencyPropertyKey HasActiveFiltersPropertyKey =
            DependencyProperty.RegisterReadOnly("HasActiveFilters",
            typeof(bool), typeof(FilterButton), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="HasActiveFilters"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HasActiveFiltersProperty =
            HasActiveFiltersPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if there are any active filters applied to this field (read-only)
        /// </summary>
        /// <seealso cref="HasActiveFiltersProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasActiveFilters
        {
            get
            {
                return (bool)this.GetValue(FilterButton.HasActiveFiltersProperty);
            }
        }

                #endregion //HasActiveFilters

                #region Operands

        /// <summary>
        /// Returns a list of possible operands (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> The Operands list gets lazily populated when the when the button is first checked. Populating the list raises the <see cref="DataPresenterBase.RecordFilterDropDownPopulating"/> event. 
		/// <para class="note"><b>Note:</b> The Operands are not used or populated when the <see cref="DropDownType"/> is <b>MultiSelectExcelStyle</b>.</para>
        /// </para>
        /// </remarks>
        /// <seealso cref="DataPresenterBase.RecordFilterDropDownPopulating"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(true)]
        public ObservableCollectionExtended<FilterDropDownItem> Operands
        {
            get
            {
                if (this._operands == null)
                    this._operands = new ObservableCollectionExtended<FilterDropDownItem>();

                return this._operands;
            }
        }

                #endregion //Operands

                #region SelectedOperand

        /// <summary>
        /// Identifies the <see cref="SelectedOperand"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedOperandProperty = DependencyProperty.Register("SelectedOperand",
            typeof(object), typeof(FilterButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedOperandChanged)));

        private static void OnSelectedOperandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FilterButton fb = target as FilterButton;

            // JJD 5/27/09 - TFS17583
            // Added flag so we know to bypass SelectedOperandChanged logic while we are
            // initializing the dropdown list
            //if (fb != null)
            if (fb != null && fb._bypassSelectedOperandChanged == false)
            {
                if (fb._fieldFilterInfo != null)
                {
                    RecordFilter rf = fb._fieldFilterInfo.RecordFilter;

                    // JJD 11/20/09 - TFS24625
                    // Only update the filter criteria if the new value is not null
                    if (e.NewValue != null)
                    {
                        FilterDropDownItem ddi = e.NewValue as FilterDropDownItem;

                        // JJD 11/20/09 - TFS24625
                        // If the selected item is a cellvalue or special operand
                        // then force the operator to be 'Equals'.
                        if ( ddi == null || ddi.IsCellValue || ddi.IsSpecialOperand )
                            rf.CurrentUIOperator = ComparisonOperator.Equals;

                        rf.CurrentUIOperand = e.NewValue;
                        // AS 5/28/09 NA 2009.2 Undo/Redo
                        // Applying via the filter should always create an undo action.
                        //
                        //rf.ApplyPendingFilter();
                        rf.ApplyPendingFilter(true, true);
                        rf.CurrentFilterDropDownItem = e.NewValue as FilterDropDownItem;

						// JJD 07/19/12 - TFS117413
						// Now that we have set the CurrentFilterDropDownItem on the 
						// RecordFilter re-coerce the ToolTip property to pick up
						// its display text
						fb.CoerceValue(ToolTipProperty);
                    }


                    // JJD 2/18/09 - TFS14161
                    // If a command was selected then the CurrentUIOperand will return
                    // null. In this case we want to asynchronously clear the SelectedOperand
                    // property.
                    // JJD 11/20/09 - TFS24625
                    // Always post the message and check in the ClearCommandOperand mtehod
                    // to see if the slected operand is a command
                    //if ( rf.CurrentUIOperand == null)

					// JJD 07/06/10 - TFS32174 
					// If we are in the middle of an asynchronous load then process the action synchrpnously
					if (fb._loader != null && fb._loader.EndReached == false)
						fb.ClearCommandOperand();
					else
                        fb.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(fb.ClearCommandOperand));
                }
            }
        }

        /// <summary>
        /// Gets/sets the selected item from the Operands collection
        /// </summary>
        /// <seealso cref="SelectedOperandProperty"/>
        //[Description("Gets/sets the selected item from the Operands collection")]
        //[Category("Behavior")]
        public object SelectedOperand
        {
            get
            {
                return (object)this.GetValue(FilterButton.SelectedOperandProperty);
            }
            set
            {
                this.SetValue(FilterButton.SelectedOperandProperty, value);
            }
        }

                #endregion //SelectedOperand

            #endregion // Public Properties

            #region Private Properties

                #region FilterInfo

        private ResolvedRecordFilterCollection.FieldFilterInfo FilterInfo
        {
            get
            {
                if (this._fieldFilterInfo != null)
                    return this._fieldFilterInfo;

                Field field = this.Field;

				
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


				RecordManager rm = GridUtilities.GetFieldRecordManager(this, field, true);

                if (rm == null || rm.DataPresenter == null)
                    return null;

                this._fieldFilterInfo = new ResolvedRecordFilterCollection.FieldFilterInfo(rm.RecordFiltersResolved, field);

                // JJD 4/13/09 - TFS16570
                // The version on the RecordFilter won't get changed if the RecordFilterCollection is cleared.
                // So instead track the HasActiveFilters property of the FieldFilterInfo which will get 
                // updated properly.

                // JJD 11/20/09 - TFS24625
                // we need to track both the Version and the HasActiveFilters property changes
                this._tracker = new PropertyValueTracker(this._fieldFilterInfo.RecordFilter, "Version", this.OnFilterVersionChanged);
                
                // JJD 12/04/09 - TFS24625
                // Call the nenamed callback since the OnFilterVersionChanged implementation
                // was stripped down to just coerce the tooltip. Otherwise, we could
                // end up clearing the SelectedOperand if the version changed before the
                // HasActiveFilters property was set
                //this._tracker.Tag = new PropertyValueTracker(this._fieldFilterInfo, "HasActiveFilters", this.OnFilterVersionChanged);
                this._tracker.Tag = new PropertyValueTracker(this._fieldFilterInfo, "HasActiveFilters", this.OnHasActiveFiltersChanged);

                return this._fieldFilterInfo;
            }
        }

                #endregion //FilterInfo

            #endregion //Private Properties	
        
        #endregion // Properties

        #region Methods

            #region Public Methods

            #endregion // Public Methods

            #region Private/Internal Methods

				// AS - NA 11.2 Excel Style Filtering
				#region ClearClosedFilterMenu
		private void ClearClosedFilterMenu()
		{
			if (_filterMenuItem == null ||
				_filterMenuItem.IsSubmenuOpen)
				return;

			// clear the datacontent we set on the element
			var tempMenuItem = new FieldMenuDataItem();
			tempMenuItem.Items.Add(new FieldMenuDataItem());
			_filterMenuItem.DataContext = tempMenuItem;
		}
				#endregion //ClearClosedFilterMenu

                // JJD 2/18/09 - TFS14161 - added
                #region ClearCommandOperand

        private void ClearCommandOperand()
        {
            ResolvedRecordFilterCollection.FieldFilterInfo info = this.FilterInfo;

            // JJD 2/18/09 - TFS14161
            // If a command was selected then the CurrentUIOperand will return
            // null. In this case we want to clear the SelectedOperand property.
            //if (info == null || info.RecordFilter.CurrentUIOperand == null )
                //this.ClearValue(SelectedOperandProperty);

            // JJD 11/20/09 - TFS24625
            // If the selected operand is an action then clear it 
            FilterDropDownItem ddi = this.SelectedOperand as FilterDropDownItem;
            if (info == null || (ddi != null && ddi.IsAction) )
                this.ClearValue(SelectedOperandProperty);
           
        }

                #endregion //ClearCommandOperand	
    
                #region CoerceTooltip

        private static object CoerceTooltip(DependencyObject target, object value)
        {
            if (value == null)
            {
                FilterButton fb = target as FilterButton;

                if (fb != null && fb._fieldFilterInfo != null)
                {
                    return fb._fieldFilterInfo.RecordFilter.Tooltip;
                }
            }

            return value;
        }

                #endregion //CoerceTooltip	

				// AS - NA 11.2 Excel Style Filtering
				#region InitializeMenuIsCheckedState
		private void InitializeMenuIsCheckedState(FieldMenuDataItem parentItem)
		{
			if (_fieldFilterInfo == null)
				return;

			var field = _fieldFilterInfo.Field;
			var rm = _fieldFilterInfo.RecordManager;
			var filter = _fieldFilterInfo.Filters[field];

			// if there was no filter then don't bother enumerating
			if (!filter.HasConditions)
				return;

			// try to match comparing the operands first
			if (!InitializeMenuIsCheckedState(parentItem, field, rm, filter, true))
			{
				// if we still don't find one then it could be one of the between..., 
				// before..., etc. items so we need to ignore the operands and just 
				// compare the conditions
				if (!InitializeMenuIsCheckedState(parentItem, field, rm, filter, false))
				{
					List<FieldMenuDataItem> ancestors;

					// see it's a match for filter tree
					Predicate<FieldMenuDataItem> findTreeCallback = delegate(FieldMenuDataItem item)
					{
						return item != null && item.Header is RecordFilterTreeControl;
					};

					ancestors = new List<FieldMenuDataItem>();
					var treeItem = parentItem.Find(findTreeCallback, true, ancestors);

					if (null != treeItem)
					{
						#region RecordFilterTreeControl

						bool hasAllEquals = true;

						var firstCondition = filter.Conditions[0] as ComparisonCondition;

						if (filter.Conditions.Count == 1 &&
							firstCondition != null &&
							firstCondition.Operator == ComparisonOperator.NotEquals &&
							firstCondition.Value == SpecialFilterOperands.Blanks)
						{
							// != Blanks is the tree control
						}
						else
						{
							// for all else we need equals operators
							foreach (ICondition condition in filter.Conditions)
							{
								var cc = condition as ComparisonCondition;

								if (cc == null || cc.Operator != ComparisonOperator.Equals)
								{
									hasAllEquals = false;
									break;
								}
							}
						}

						if (hasAllEquals)
						{
							treeItem.IsChecked = true;

							foreach (var ancestor in ancestors)
								ancestor.IsChecked = true;
						} 

						#endregion //RecordFilterTreeControl
					}

					// finally use custom since we have conditions
					if (null == treeItem || treeItem.IsChecked == false)
					{
						#region Custom Filter...
						Predicate<FieldMenuDataItem> findCustomCallback = delegate(FieldMenuDataItem item)
						{
							var cfCmd = item.Command as ConditionFilterCommand;
							return cfCmd != null && cfCmd.ShowCustomFilterDialog && cfCmd.Condition == null;
						};

						ancestors = new List<FieldMenuDataItem>();
						var customItem = parentItem.Find(findCustomCallback, true, ancestors);

						if (null != customItem)
						{
							customItem.IsChecked = true;

							foreach (var ancestor in ancestors)
								ancestor.IsChecked = true;
						} 
						#endregion //Custom Filter...
					}
				}
			}
		}

		private bool InitializeMenuIsCheckedState(FieldMenuDataItem parentItem, Field field, RecordManager rm, RecordFilter filter, bool matchOperands)
		{
			foreach (FieldMenuDataItem child in parentItem.Items)
			{
				RecordFilterCommandBase filterCommand = child.Command as RecordFilterCommandBase;

				if (null != filterCommand)
				{
					// if we're in the pass where we don't want to match operands then 
					// skip ones that we wouldn't be prompting for more info on the 
					// assumption that those have operands values that make them 
					// unique. ones that would show the dialog (e.g. before...) would 
					// have required they provide the value so the command wouldn't 
					// have the operand to compare
					if (!matchOperands && !filterCommand.ShowCustomFilterDialog)
						continue;

					if (filterCommand.HasSameConditions(filter, matchOperands, field, rm))
					{
						child.IsChecked = true;
						return true;
					}
				}
				else if (InitializeMenuIsCheckedState(child, field, rm, filter, matchOperands))
				{
					child.IsChecked = true;
					return true;
				}
			}

			return false;
		}
				#endregion //InitializeMenuIsCheckedState

				// JJD 06/29/10 - TFS32174 - added
				#region OnDropdownLoadCompleted

		private void OnDropdownLoadCompleted(object sender, EventArgs e)
		{
			Debug.Assert(this._loader == sender, "FilterButton.OnDropdownLoadCompleted");
			if (this._loader == sender)
			{
				this._loader.Phase2Completed -= new EventHandler(OnDropdownLoadCompleted);
				this._loader = null;
			}
		}

				#endregion //OnDropdownLoadCompleted	

				// AS - NA 11.2 Excel Style Filtering
				#region OnFieldPropertyChanged
		private void OnFieldPropertyChanged(PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "FilterLabelIconDropDownTypeResolved":
					this.VerifyDropDownType();
					break;
			}
		}
				#endregion //OnFieldPropertyChanged
    
                // JJD 12/3/09 - TFS24625 - added new OnFilterVersionChanged
                #region OnFilterVersionChanged

        private void OnFilterVersionChanged()
        {
            this.CoerceValue(ToolTipProperty);
        }

   	            #endregion //OnFilterVersionChanged	

                // JJD 12/3/09 - TFS24625 - renamed OnFilterVersionChanged
                #region OnHasActiveFiltersChanged

        private void OnHasActiveFiltersChanged()
        {
            ResolvedRecordFilterCollection.FieldFilterInfo info = this.FilterInfo;

            if (info != null)
            {
                if (info.HasActiveFilters)
                    this.SetValue(HasActiveFiltersPropertyKey, KnownBoxes.TrueBox);
                else
                {
                    this.ClearValue(HasActiveFiltersPropertyKey);

                    // JJD 4/13/09 - TFS16570
                    // Since we don't have active filters make sure we clear the
                    // SelectedOperand property. Otherwise, is the user subsequently
                    // drops down the operand list and selects the same operand
                    // then nothing will happen.
                    if (this.SelectedOperand != null)
                        this.ClearValue(SelectedOperandProperty);
                 }
            }

            // JJD 11/20/09 - TFS24625
            // Moved from top of method
            this.CoerceValue(ToolTipProperty);

        }

   	            #endregion //OnHasActiveFiltersChanged	
        
				// JJD 2/9/11 - TFS35853 - added
				#region OnRecordManagerChanged

		private static void OnRecordManagerChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FilterButton fb = target as FilterButton;

			if (fb != null)
			{
				fb.ReleaseFilterInfo();
				fb.VerifyFilterInfo();
			}
		}

				#endregion //OnRecordManagerChanged	
    
                // JJD 1/28/09 - TFS12533
                #region OnVisibilityChanged

        private static void OnVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FilterButton fb = target as FilterButton;

            if (fb != null)
            {
                // JJD 1/28/09 - TFS12533
                // If the filter button is being made visible then call OnFilterVersionChanged
                // so we can initialize the HasActiveFilters property
				if (((Visibility)(e.NewValue)) == Visibility.Visible)
				{
					// JJD 2/7/11 - TFS35853
					// Moved logic into helper method
					fb.VerifyFilterInfo();
				}
            }
        }

                #endregion //OnVisibilityChanged	

				// JJD 2/9/11 - TFS35853 - added
				#region ReleaseFilterInfo

		private void ReleaseFilterInfo()
		{
			this._tracker = null;
			this._fieldFilterInfo = null;
			if (this._operands != null)
				this._operands.Clear();
		}

				#endregion //ReleaseFilterInfo	
        
                // JJD 6/8/09 - TFS18044 - added
                #region ShiftFocusOut

        private void ShiftFocusAway()
        {
            // Make sure we still have focus within and we aren't checked
            if (this.IsKeyboardFocusWithin && this.IsChecked == false)
            {
                Field fld = this.Field;

                if (fld == null)
                    return;

                DataPresenterBase dp = fld.DataPresenter;

                if (dp == null)
                    return;

                Record previousActiveRecord = null;

                // see if we had an previous active cell
                Cell previousActiveCell = this._previouslyActiveElement as Cell;

                // determine the previous active record
                if (previousActiveCell != null)
                    previousActiveRecord = previousActiveCell.Record;
                else
                    previousActiveRecord = this._previouslyActiveElement as Record;

                if (previousActiveRecord != null)
                {
                    Record rcdToFocus = null;

                    // if the prevoius rcd is still in view and still focusable then
                    // we will use it
                    if (previousActiveRecord.VisibilityResolved == Visibility.Visible &&
                        previousActiveRecord.IsEnabledResolved)
                        rcdToFocus = previousActiveRecord;
                    else
                    {
                        // look for a sibling rcd to use
                        RecordManager rm = previousActiveRecord.RecordManager;

                        if (rm != null)
                        {
                            ViewableRecordCollection vr = rm.ViewableRecords;

                            // loop over all sibling rcds looking for a data record
                            // that is enabled
                            foreach (Record sibling in vr)
                            {
                                if (sibling is DataRecord &&
                                    sibling.IsEnabledResolved)
                                {
									// JJD 10/26/11 - TFS91364 
									// Ignore HeaderReords 
									if (!(sibling is HeaderRecord))
									{
										rcdToFocus = sibling;
										break;
									}
                                }
                            }
                        }
                    }

                    if (rcdToFocus != null)
                    {
                        // If the field layout is same then activate the associated cell
                        // but only is it isn't an add record
                        if (previousActiveCell != null &&
                            rcdToFocus is DataRecord &&
							// JJD 10/26/11 - TFS91364 
							// Ignore HeaderReords 
                            !(rcdToFocus is HeaderRecord) &&
							((DataRecord)rcdToFocus).IsAddRecord == false &&
                            previousActiveCell.Field.Owner == rcdToFocus.FieldLayout)
                        {
                            dp.ActiveCell = ((DataRecord)rcdToFocus).Cells[previousActiveCell.Field];
                        }
                        else
                            dp.ActiveRecord = rcdToFocus;

						// JM 03-05-10 TFS29059.  Force filtering changes to happen synchronously.
						if (dp.CurrentPanel != null)
							dp.CurrentPanel.InvalidateMeasure();
						
                        dp.UpdateLayout();

                        RecordPresenter rp = RecordPresenter.FromRecord(dp.ActiveRecord);

                        // if the record isn't in view then try to bring it into view
                        if (rp == null)
                        {
                            dp.BringRecordIntoView(dp.ActiveRecord);

                            dp.UpdateLayout();

                            rp = RecordPresenter.FromRecord(dp.ActiveRecord);
                        }

                        if (rp != null)
                        {
                            CellValuePresenter cvp = null;

                            if (dp.ActiveCell != null)
                            {
                                cvp = CellValuePresenter.FromCell(dp.ActiveCell);

                                // if the cell isn't in view then try to bring it into view
                                if (cvp == null)
                                {
                                    dp.BringCellIntoView(dp.ActiveCell);

                                    dp.UpdateLayout();

                                    cvp = CellValuePresenter.FromCell(dp.ActiveCell);
                                }
                            }

                            // move focus to the cell or record
                            if (cvp != null)
                                cvp.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                            else
                                rp.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

                            return;
                        }
                    }
                }

                // finally just focus the datapresenter
                if (dp.Focusable)
                    dp.Focus();
            }

            this._previouslyActiveElement = null;
        }

                #endregion //ShiftFocusOut	

				// AS - NA 11.2 Excel Style Filtering
				#region VerifyDropDownType
		private void VerifyDropDownType()
		{
			Field field = this.Field;

			if (null == field)
				return;

			FilterLabelIconDropDownType ddType = field.FilterLabelIconDropDownTypeResolved;

			if (ddType != this.DropDownType)
				this.SetValue(DropDownTypePropertyKey, ddType);
		}
				#endregion //VerifyDropDownType
		
				// JJD 2/7/11 - TFS35853 - added
				#region VerifyFilterInfo

		private void VerifyFilterInfo()
		{
			// JJD 2/4/11 - TFS64119
			// Access the FilterInfo property which will ensure that we create trackers
			// so we know when the filter version and HasActiveFileters state changes so
			// that we can up the UI.
			ResolvedRecordFilterCollection.FieldFilterInfo filterInfo = this.FilterInfo;

			this.OnFilterVersionChanged();

			// JJD 2/7/11 - TFS35853
			// Call OnHasActiveFiltersChanged to ensute that property is set correctly
			this.OnHasActiveFiltersChanged();
		}

				#endregion //VerifyFilterInfo	
     
            #endregion // Private/Internal Methods

        #endregion // Methods

		// AS - NA 11.2 Excel Style Filtering
		#region IWeakEventListener Members
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					this.OnFieldPropertyChanged(args);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for DataItemSelector, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for DataItemSelector, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		} 
		#endregion //IWeakEventListener Members
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