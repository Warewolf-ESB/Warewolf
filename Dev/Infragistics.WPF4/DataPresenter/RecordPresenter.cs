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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using System.Windows.Threading;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Commands;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Internal;


namespace Infragistics.Windows.DataPresenter
{
	#region RecordPresenter base class

	/// <summary>
	/// Base class used to represent a Record in a XamDataGrid, XamDataCarousel or XamDataPresenter.
	/// </summary>
	/// <remarks>
	/// <para class="body">One of these elements is created to represent a corresponding <see cref="Record"/> in the UI. There are corresponding derived classes to represent  
	/// each of the <see cref="Record"/> derived classes - <see cref="DataRecord"/>, <see cref="ExpandableFieldRecord"/> and <see cref="GroupByRecord"/>.</para>
	/// <para></para>
	/// <para class="body">In addition to the standard <b>Template</b> property there are 2 other properties of type <see cref="System.Windows.Controls.ControlTemplate"/>, <see cref="TemplateGridView"/> and <see cref="TemplateCardView"/> that can be used 
	/// to define different visual trees based on the <see cref="XamDataPresenter.View"/>'s <see cref="ViewBase.CellPresentation"/>. 
	/// These visual trees should include <see cref="System.Windows.FrameworkElement"/> derived classes for the following 3 logical areas:
	/// <ul>
	/// <li>Record Content (must be named 'PART_RecordContentSite') - this is normally a <see cref="System.Windows.Controls.ContentControl"/> or <see cref="System.Windows.Controls.ContentPresenter"/> which contains the <see cref="DataRecordCellArea"/> for <see cref="DataRecord"/>s or the associated <see cref="Record"/>'s <see cref="Infragistics.Windows.DataPresenter.Record.Description"/> for <see cref="ExpandableFieldRecord"/>s and <see cref="GroupByRecord"/>s.</li>
	/// <li>Header Content (must be named 'PART_HeaderContentSite') - this is normally a <see cref="System.Windows.Controls.ContentControl"/> or <see cref="System.Windows.Controls.ContentPresenter"/> which contains the <see cref="HeaderContent"/> for <see cref="DataRecord"/>s. It is not used for either <see cref="ExpandableFieldRecord"/>s or <see cref="GroupByRecord"/>s.</li>
	/// <li>Nested Content (must be named 'PART_NestedContentSite') - this is normally a <see cref="System.Windows.Controls.ContentControl"/> or <see cref="System.Windows.Controls.ContentPresenter"/> which contains the <see cref="NestedContent"/> if the <see cref="IsExpanded"/> property is set to true, otherwise it is empty.</li>
	/// </ul>
	/// </para>
	/// <para></para>
	/// <para class="note"><b>Note: </b>These elements are normally virtualized by the view panels, <see cref="System.Windows.Controls.Panel"/> derived classes that implement the <see cref="Infragistics.Windows.DataPresenter.IViewPanel"/> interface). 
	/// That means that there should only be enough of these elements created to represent the records currently in view plus some additional number that the panel might cache for scrolling performance reasons.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="HeaderContent"/>
	/// <seealso cref="NestedContent"/>
	/// <seealso cref="HasHeaderContent"/>
	/// <seealso cref="HasNestedContent"/>
	/// <seealso cref="ShouldDisplayRecordContent"/>
	/// <seealso cref="Record"/>
	/// <seealso cref="DataRecord"/>
	/// <seealso cref="DataRecordPresenter"/>
	/// <seealso cref="GroupByRecord"/>
	/// <seealso cref="GroupByRecordPresenter"/>
	/// <seealso cref="ExpandableFieldRecord"/>
	/// <seealso cref="ExpandableFieldRecordPresenter"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataPresenter"/>

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateFilteredIn,      GroupName = VisualStateUtilities.GroupFilter)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilteredOut,     GroupName = VisualStateUtilities.GroupFilter)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateFixed,           GroupName = VisualStateUtilities.GroupFixed)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfixed,         GroupName = VisualStateUtilities.GroupFixed)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,        GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,      GroupName = VisualStateUtilities.GroupSelection)]

	[TemplatePart(Name = "PART_RecordContentSite", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_NestedContentSite", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_HeaderContentSite", Type = typeof(FrameworkElement))]
	public abstract class RecordPresenter : Control, ISelectableElement, IWeakEventListener
	{
		#region Member Variables

		private int _cachedVersion;
		private bool _versionInitialized;
		private bool _isAlternateInitialized;
		private bool _isArrangedInView = true;
		private Record _record;
		private object _nestedContent;
		private object _headerContent;
		private StyleSelectorHelper _styleSelectorHelper;
		private bool _isBoundaryRecord = false;
		private bool _ignoreNextRecordsInViewVersionPropertyChange = false;
		private bool _preparingItemForContainer;
		private bool _hasBeenPrepared;
		private bool _shouldDisplayExpandableRecordContent = true;
		private bool _shouldDisplayGroupByRecordContent = true;
		private bool _shouldDisplayRecordContent = true;
		private bool _isSwappingTemplate;
		private bool _isTemplateDirty = true;
		private bool _isClearingBindings;
		private bool _hasRecordEverBeenInitialized;
		
		// JJD 11/17/11 - TFS78651 - Optimization
		private bool _descSyncPending;
		
		private ControlTemplate _cachedTemplate;
		private FrameworkElement _parentContentPresenter;

		// AS 6/22/09 NA 2009.2 Field Sizing
		//// AS 6/11/09 TFS18382
		//private PropertyValueTracker _parentCPExtentTracker;
		private PropertyValueTracker _parentCPWidthTracker;
		private PropertyValueTracker _parentCPHeightTracker;

		private bool			_isActiveFixedRecord = false;
		private bool			_isActiveHeaderRecord = false;
		private bool			_treatAsCollapsed = false;

		// JJD 2/16/12 - TFS101387
		// Use a bool to indicate the LayoutUpdated was wired since we are noe using the 
		// DataPresenterBase's WireLayoutUpdated infrastucture
		//private EventHandler	_layoutUpdatedHandler;
		private bool			_layoutUpdatedWired;

        // JJD 2/11/09 - added
        private DispatcherOperation _asyncAutoFit;

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        private WeakReference   _cellArea;

		// JJD 3/11/11 - TFS67970 - Optimization
		// Cache a weak reference to this RecordPresenter to be used by its associated Record
		// This prevents heap fragmentation when this object is recycled
		private WeakReference _weakRef;		

        // JJD 03/16/09 - Optimization
        // Keep a flag so we know if the nested data needs to be initialized
        private bool _nestedDataRequiresInitalization;
        
        // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
        // Added
        private Geometry _internalClip;
        private Geometry _explicitClip;
        private CombinedGeometry _combinedClip;

		// AS 2/25/11 TFS66934
		private ExpansionIndicator _expansionIndicator;

		// AS 5/3/11 TFS73495
		private bool _isInitializeFixedFieldInfoPending;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion Member Variables

		#region Constructors

		static RecordPresenter()
		{
			// This will manage FocusWithinManager.IsFocusWithin property for this type.
			// 
			FocusWithinManager.RegisterType( typeof( RecordPresenter ) );

            // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
            CommandBinding commandBindingClearFilters = new CommandBinding(DataPresenterCommands.ClearActiveRecordCellFilters,
                RecordPresenter.ExecuteCommandHandler, RecordPresenter.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(RecordPresenter), commandBindingClearFilters);

            // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
            // Added coerce callback for ClipProperty
            ClipProperty.OverrideMetadata(typeof(RecordPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceClip)));

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(RecordPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordPresenter"/> class
		/// </summary>
		protected RecordPresenter()
		{
			// initialize the styleSelectorHelper
			this._styleSelectorHelper = new StyleSelectorHelper(this);
			
			// JJD 5/22/07
			// Bind the Visibility to the new VisibilityResolved readonly property
			// We need to use a binding now so the new Recycling logic doesn't conflict
			// with our internal settings
			this.SetBinding(RecordPresenter.VisibilityProperty, Utilities.CreateBindingObject(VisibilityResolvedProperty, BindingMode.OneWay, this));
		}

		#endregion Constructors

		#region CommandHandlers

            // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
			#region CanExecuteCommandHandler

		private static void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
		{
            RecordPresenter rp = sender as RecordPresenter;

			e.CanExecute = false;

            if (e.Command == DataPresenterCommands.ClearActiveRecordCellFilters)
            {
                if (rp != null && rp.Record is FilterRecord)
                    e.CanExecute = ((FilterRecord)(rp.Record)).HasActiveFilters;

                e.Handled = true;
            }
		}

			#endregion //CanExecuteCommandHandler	

            // JJD 12/31/08 - NA 2009 Vol 1 - Record Filtering
			#region ExecuteCommandHandler

		private static void ExecuteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
            RecordPresenter rp = sender as RecordPresenter;
			if (rp != null)
				rp.ExecuteCommandImpl(e.Command as RoutedCommand, e.Parameter, e.Source, e.OriginalSource);
		}

			#endregion //ExecuteCommandHandler

			#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(RoutedCommand command, object parameter, object source, object originalSource)
		{
			// Make sure we have a command to execute.
			if (null == command)
				throw new ArgumentNullException("command");

			bool handled = false;

            if (command == DataPresenterCommands.ClearActiveRecordCellFilters)
            {
                FilterRecord fr = this.Record as FilterRecord;

                if (fr != null)
					fr.ClearActiveFilters(true );
 
                handled = true;
            }

			return handled;
		}

			#endregion //ExecuteCommandImpl

		#endregion //CommandHandlers

		#region Base class overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size size = base.ArrangeOverride(finalSize);

			DataPresenterBase dp = this.DataPresenter;
			FieldLayout fl = this.FieldLayout;

			// AS 6/22/09 NA 2009.2 Field Sizing
			//if (dp != null && dp.AutoFitResolved)
			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			//if (dp != null && fl != null && fl.IsAutoFit)
			if (dp != null && fl != null && fl.IsAutoFit && (this.IsVisible || this.DataContext != null ) )
			{
				// update the autfit properties in case anything has shifted

				// JJD 3/27/07
				// Hook the LayoutUpdated event to process the UpdateAutoFitProperties after everything is arranged
				//this.UpdateAutoFitProperties();
				// JJD 2/16/12 - TFS101387
				// Check the _layoutUpdatedWired flag. If false set it to true and call the 
				// DataPresenterBase's WireLayoutUpdated method
				//if (this._layoutUpdatedHandler == null)
				//{
				//    this._layoutUpdatedHandler = new EventHandler(OnLayoutUpdated);
				//    this.LayoutUpdated += this._layoutUpdatedHandler;
				//}
				if ( false == _layoutUpdatedWired )
				{
					_layoutUpdatedWired = true;
					dp.WireLayoutUpdated(new GridUtilities.MethodDelegate(this.OnLayoutUpdated));
				}

                // JJD 2/11/09 
                // There is the potential to get into a situation where the LayoutUpdated event never
                // gets raised. This can happen if element's measure and/or arrange are invalidated
                // endlessly. Therefore in addition to hooking the LayoutUpdated event above we
                // want to asynchronously call a method that we know will eventally get called
                // as insurance.
                // JJD 2/19/09 - support for printing.
                // We can't do asynchronous operations during a report operation
                //if (this._asyncAutoFit == null)
                if (this._asyncAutoFit == null && dp.IsReportControl == false)
                    this._asyncAutoFit = this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(this.OnUpdateAutoFitAsync));
			}

			return size;
		}

			#endregion //ArrangeOverride	

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			DataPresenterBase dp = this.DataPresenter;

			// AS 2/19/10 TFS28036
			// Make sure the template cache has been verified before we process any cells.
			//
			if (_record is TemplateDataRecord)
			{
				// JJD 08/15/12 - TFS119037
				// Verify that the fieldlayout is still valid. If not return the cached DesiredSize.
				// This prevents causing the TemplateDataRecordCache for an old (disposed)
				// FieldLayout from getting re-initialized and added to the logical tree
				// of the DP which will leak memory since it will never be removed.
				//_record.FieldLayout.TemplateDataRecordCache.Verify();
				FieldLayout flayout = _record.FieldLayout;

				if (flayout.WasRemovedFromCollection)
					return this.DesiredSize;

				flayout.TemplateDataRecordCache.Verify();
			}

			if (dp == null && this._hasRecordEverBeenInitialized == false)
			{
				// if we don't have a dp at this point then we are probably being used in a 
				// designer tool like Blend to get at this styling point so set the DataContext 
				// to a descriptive string
				this.DataContext = this.GetType().Name + " RecordContentArea";

				if (this.CellPresentation == CellPresentation.GridView)
				{
					this.SetValue(HasHeaderContentPropertyKey, KnownBoxes.TrueBox);
					this.SetValue(HasNestedContentPropertyKey, KnownBoxes.TrueBox);

					TextBlock headerText = new TextBlock();

					headerText.Text = "HeaderContent (GridView format only)";
					headerText.Padding = new Thickness(2);
					headerText.Foreground = new SolidColorBrush(Colors.Gray);
					headerText.FontStyle = FontStyles.Normal;
					headerText.FontWeight = FontWeights.Normal;

					this.SetValue(ExpansionIndicatorVisibilityProperty, KnownBoxes.VisibilityVisibleBox);

					this.SetValue(HeaderContentPropertyKey, headerText);
					this.SetValue(NestedContentPropertyKey, "NestedContent (GridView format only)");
				}
				else
				{
					this.SetValue(HasHeaderContentPropertyKey, KnownBoxes.FalseBox);
					this.SetValue(HasNestedContentPropertyKey, KnownBoxes.FalseBox);
					this.SetValue(ExpansionIndicatorVisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
					this.ClearValue(HeaderContentPropertyKey);
					this.ClearValue(NestedContentPropertyKey);
				}
			}

			// make sure we have the proper template
            if (this._isTemplateDirty == true)
            {
                this.SetTemplateBasedOnTemplateType();

                // AS 3/11/09 TFS11010
                // I noticed that when you changed the theme multiple times (e.g.
                // on vista, change to lunanormal to default to lunaolive would result
                // in some or all of the cards disappearing. This happened because 
                // we "fixed up" the template of the recordpresenter in the measure.
                // The FrameworkElement.MeasureCore had already called ApplyTemplate 
                // before calling MeasureOverride so we had visual children. But then 
                // since our template is dirty flag was true, we changed the template 
                // which caused us to have 0 visual children because that template wasn't 
                // applied. Since there were no visual children the call to base.MeasureOverride 
                // returned 0,0 (since the base Control class will return 0,0 if it has no 
                // visual children. So if we have changed the template we will force the 
                // template to be applied before calling the base measure.
                //
                if (false == _isTemplateDirty)
                    this.ApplyTemplate();
            }

			FieldLayout fl = this.FieldLayout;

			// AS 6/22/09 NA 2009.2 Field Sizing
			// Check the property on the field layout itself
			//
			//// AS 3/22/07 AutoFit
			////if (dp != null && dp.AutoFit)
			//if (dp != null && dp.AutoFitResolved &&
			//    this._record != null)
			FrameworkElement oldParentCp = _parentContentPresenter;

			if (null != fl && fl.IsAutoFit && null != _record)
			{
				// JJD 3/27/07
				// For nested records bind the ParentRecordAutoFitVersionProperty to the AutoFitVersionProperty of the parent
				if (this._record.ParentDataRecord != null && !this.IsHeaderRecord)
				{
					DataRecordPresenter drp = Utilities.GetAncestorFromType(this, typeof(DataRecordPresenter), true, dp) as DataRecordPresenter;

					if (drp != null)
						this.SetBinding(ParentRecordAutoFitVersionProperty, Utilities.CreateBindingObject(AutoFitVersionProperty, BindingMode.OneWay, drp));

				}
				else
				{
					// find the ancestor ScrollContentPresenter which will determine this record size
					FrameworkElement parentCP;

					Type stopAtType = null;
					ViewBase view = dp.CurrentViewInternal;

					if (view.RecordPresenterContainerType == null)
					{
						// JJD 4/6/07
						// only do the walk up the ancestor chain if AutoFitToRecord is false
						if (view.AutoFitToRecord == false)
							stopAtType = typeof(RecordListControl);
					}
					else
						stopAtType = dp.CurrentViewInternal.RecordPresenterContainerType;

					if (stopAtType != null)
					{
						parentCP = Utilities.GetAncestorFromType(this, typeof(ScrollContentPresenter), false, null, stopAtType) as FrameworkElement;

						// JJD 3/27/07
						// We shouldn't call GetType() since stopAtType is already the type to compare
						//if (stopAtType.GetType() == typeof(RecordListControl))
						if (stopAtType == typeof(RecordListControl))
						{
							RecordPresenter rp = this;

							// walk up the parent chain to get the highest level scrollContentPresenter
							while (parentCP != null && rp != null && rp.Record != null && rp.Record.ParentRecord != null)
							{
								rp = Utilities.GetAncestorFromType(parentCP, typeof(RecordPresenter), true, dp) as RecordPresenter;

								if (rp != null)
									parentCP = Utilities.GetAncestorFromType(rp, typeof(ScrollContentPresenter), false, null, stopAtType) as FrameworkElement;
							}
						}
						// cache the element
						this._parentContentPresenter = parentCP;
					}
					else
						this._parentContentPresenter = null;
				}
			}
			else
			{
				// clear and unhook from the old element's SizeChanged event
				this._parentContentPresenter = null;
			}

			// AS 6/11/09 TFS18382
			if (_parentContentPresenter == null)
			{
				// AS 6/22/09 NA 2009.2 Field Sizing
				//_parentCPExtentTracker = null;
				_parentCPWidthTracker = _parentCPHeightTracker = null;
			}
			else
			{
				// The recordpresenter was only hooking the layout updated from within the 
				// arrange. However, since we do not explicitly arrange the root records based 
				// on the viewport extent (because we want to support scrolling should the 
				// record need to be bigger than the viewport because of the minimums of the 
				// fields) the rp could have been measured/arranged and then when the grid was 
				// resized the rp did not have its arrange dirtied because it was already 
				// sized based on what it reported. The autofit calculation relies upon the 
				// ActualWidth of the root contentpresenter which would have changed. So we 
				// need to know when that changes as well and deal with the autofit props.
				//
				// AS 6/22/09 NA 2009.2 Field Sizing
				// We should track both the height and width changes. Also, we probably shouldn't create
				// new trackers if the contentpresenter hasn't changed.
				//
				//DependencyProperty prop = dp.IsAutoFitHeight ? FrameworkElement.ActualHeightProperty : FrameworkElement.ActualWidthProperty;
				//_parentCPExtentTracker = new PropertyValueTracker(_parentContentPresenter, prop, new PropertyValueTracker.PropertyValueChangedHandler(OnAutoFitExtentChanged));

				if (fl.IsAutoFitWidth)
				{
					if (_parentCPWidthTracker == null || _parentContentPresenter != oldParentCp)
						_parentCPWidthTracker = new PropertyValueTracker(_parentContentPresenter, FrameworkElement.ActualWidthProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnAutoFitExtentChanged));
				}
				else
					_parentCPWidthTracker = null;

				if (fl.IsAutoFitHeight)
				{
					if (_parentCPHeightTracker == null || _parentContentPresenter != oldParentCp)
						_parentCPHeightTracker = new PropertyValueTracker(_parentContentPresenter, FrameworkElement.ActualHeightProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnAutoFitExtentChanged));
				}
				else
					_parentCPHeightTracker = null;
			}

			return base.MeasureOverride(availableSize);
		}

			#endregion //MeasureOverride	

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
			// AS 2/25/11 TFS66934
			if (null != _expansionIndicator)
			{
				_expansionIndicator.Click -= new RoutedEventHandler(OnExpansionIndicatorClick);
				_expansionIndicator = null;
			}

            base.OnApplyTemplate();

			// AS 2/25/11 TFS66934
			// Having a command associated with a command source like a button means that the 
			// CanExecute will be invoked every time the mouse or key is pressed down. That results 
			// in 2 routed events (previewcanexecute and canexecute). This has a serious impact on 
			// scrolling via the arrow keys and typing in general.
			//
			Utilities.DependencyObjectSearchCallback<ExpansionIndicator> callback = delegate(ExpansionIndicator indicator)
			{
				return indicator.Command == DataPresenterCommands.ToggleRecordIsExpanded;
			};
			_expansionIndicator = Utilities.GetTemplateChild<ExpansionIndicator>(this, callback);

			if (null != _expansionIndicator)
			{
				_expansionIndicator.Command = null;
				_expansionIndicator.Click += new RoutedEventHandler(OnExpansionIndicatorClick);
			}

            FrameworkElement fe = this.GetRecordContentSite();

            // JJD 1/8/09 - NA 2009 vol 1 - Record filtering
            //  Bind the record conten area's IsEnabled to the IsRecordEnabledProperty
            if ( fe != null )
                fe.SetBinding(FrameworkElement.IsEnabledProperty, Utilities.CreateBindingObject(IsRecordEnabledProperty, BindingMode.OneWay, this));

			// JM 06-04-09 TFS14198
			if (this.IsLoaded)
				this.RaiseNestedContentEvent(this.HasNestedContent);

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	
		
			// JM 02-18-10 TFS27652 - Added.
			#region OnChildDesiredSizeChanged

		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			CardViewCard parentCard = this.Parent as CardViewCard;

			// If this RecordPresenter is inside of a Card, and the Card's Height/Width have not been explicitly set, we need to
			// invalidate the measure of all the elements in the parent chain up to the parent of the Card (i.e., up to the 
			// CardViewPanel).  We need to do this because when the Card size is not explicitly set (via CardViewSettings.CardHeight
			// or CardViewSettings.CardWidth), the ScrollViewer in the Card's template will prevent elements within it from re-measuring
			// when a previously collapsed cell is uncollapsed. 
			if (null != parentCard)
			{
				CardViewSettings viewSettings = (this.DataPresenter != null && this.DataPresenter.CurrentViewInternal is CardView) ? ((CardView)this.DataPresenter.CurrentViewInternal).ViewSettings : null;
				if (viewSettings != null					&& 
					double.IsNaN(viewSettings.CardWidth)	&& 
					double.IsNaN(viewSettings.CardHeight))
				{
					UIElement parent	= VisualTreeHelper.GetParent(parentCard) as UIElement;
					UIElement element	= this;

					do
					{
						element.InvalidateMeasure();

						if (element == parent)
							break;

						element = VisualTreeHelper.GetParent(element) as UIElement;
					} while (element != null);
				}
			}

			base.OnChildDesiredSizeChanged(child);
		}

			#endregion //OnChildDesiredSizeChanged

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="RecordPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.RecordPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.RecordPresenterAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer

			#region OnKeyDown
		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

			#endregion //OnKeyDown

			#region OnGotFocus

		/// <summary>
		/// Called when focus is set on this element.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			if (this.ShouldDisplayRecordContent)
			{
				FrameworkElement recordContentSite = this.GetRecordContentSite();

				if (recordContentSite != null)
				{
					FrameworkElement elementToFocus = FindFocusableDescendant(recordContentSite);
					if (elementToFocus != null)
					{
						DataPresenterBase dp = this.DataPresenter;

						if (dp != null)
						{
							// JJD 3/12/07
							// if the datapresenter has keyboard focus within we want to take keyboard focus
							if (dp.IsKeyboardFocusWithin)
								elementToFocus.Focus();
							else
								// JJD 3/14/07
								// Otherwise if the datapresenter has logical focus within we want to take logical focus
								if (true == (bool)dp.GetValue(FocusWithinManager.IsFocusWithinProperty))
								{
									DependencyObject scope = FocusManager.GetFocusScope(elementToFocus);

									if (scope != null)
									{
										// AS 5/11/12 TFS104724
										//FocusManager.SetFocusedElement(scope, elementToFocus);
										Utilities.SetFocusedElement(scope, elementToFocus);
									}
								}
						}
						return;
					}
					return;
				}
			}

			base.OnGotFocus(e);
		}

			#endregion //OnGotFocus	
    
			#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// if this record presenter is being cleared then ignore any changes
			if (this._isClearingBindings)
				return;

			DependencyProperty property = e.Property;

			if (property == StyleProperty)
			{
				if (!this._preparingItemForContainer)
				{
					this._isTemplateDirty = true;
					this.ClearValue(TemplateProperty);
					this.ClearValue(TemplateCardViewProperty);
					this.ClearValue(TemplateGridViewProperty);
					this.InitializeMinWidth();
					//this.SetTemplateBasedOnTemplateType();
				}
			}
			else if (property == RecordPresenter.TemplateProperty)
			{

				// if we aren't swapping out the template then cache the one that is being set
				if (!this._isSwappingTemplate)
				{
					this._isTemplateDirty = true;
					this._cachedTemplate = this.Template;
				}
			}
			else if (property == InternalVersionProperty)
			{
				if (!this._isClearingBindings)
				{
					this.InitializeVersionInfo();
					this.SetTemplateBasedOnTemplateType();
				}
			}
			//else if (property == InternalThemeProperty)
			//{
			//    if (!this._preparingItemForContainer &&
			//        !this._isClearingBindings)
			//    {
			//        this.Style = new Style();
			//        this._styleSelectorHelper.InvalidateStyle();
			//    }
			//}
			else if (property == RecordsInViewVersionProperty)
			{
				if (!this._preparingItemForContainer)
				{
					if (this._ignoreNextRecordsInViewVersionPropertyChange)
						this._ignoreNextRecordsInViewVersionPropertyChange = false;
					else
					{
						this.InvalidateMeasure();

						// AS 5/9/07
						// We were previously setting the framework metadata to invalidate the parent but we
						// don't want to do that unless necessary. e.g. It is not necessary when using flat
						// data.
						//
						if (this.Record.ParentRecord != null)
						{
							for (DependencyObject parent = VisualTreeHelper.GetParent(this); parent != null; parent = VisualTreeHelper.GetParent(parent))
							{
                                // JJD 1/13/09 - TFS11924
                                // Make sure we skip RecordListItemContainer decorator and invalidate
                                // the measure of its parent panel. 
                                // Note: the RecordListItemContainer it only used if the RecordContainerGenerationMode
                                // is no set to 'Recycle'
                                //if (parent is UIElement)
                                if (parent is UIElement && !(parent is RecordListItemContainer))
								{
									((UIElement)parent).InvalidateMeasure();
									break;
								}
							}
						}
					}
				}
			}
			else if (property == CellPresentationProperty)
			{
				if (!this._preparingItemForContainer)
				{
					this._isTemplateDirty = true;
					this.InvalidateMeasure();
				}
			}
			else if ( e.Property == FocusWithinManager.IsFocusWithinProperty )
			{
				this.OnIsFocusWithinChanged( (bool)e.NewValue );
			}
            else if (e.Property == FrameworkElement.IsVisibleProperty)
            {
                DataPresenterBase dp = this.DataPresenter;

                // JJD 03/16/09 - Optimization
                // When expanded record prsesenters become visible and they are
                // waiting for their nested content to be initialized we need
                // to queue a request (which will get processed on the next LayoutUpdated 
                // event.
                if (dp != null && this.IsExpanded)
                {
                    if (true == (bool)e.NewValue)
                    {
                        if (this._nestedDataRequiresInitalization)
                        {
                            dp.QueueNestedContentInitializationRequest(this);
                        }
                    }
                    else
                    {
                        // JJD 11/19/09 - TFS24314
                        // If IsVisible is going to false we need to set the flag
                        // so that when IsVisible is set back to true we will queue
                        // a request to re-initialize the nested content. This ensures
                        // that all appropriate props are initialized and the
                        // Show/HideNetsedContent events are raised so that any event triggers
                        // are invoked. This is important e.g. after a theme change.
                        this._nestedDataRequiresInitalization = true;
                    }
                }
            }
            // JJD 3/27/07
			// Invalidate the measure and bump the autofitversion when an autofit property changes
			else if (e.Property == RecordPresenter.ParentRecordAutoFitVersionProperty ||
					  e.Property == RecordPresenter.AutoFitWidthProperty ||
					  e.Property == RecordPresenter.AutoFitHeightProperty)
			{
				// JJD 4/21/11 - TFS73048 - Optimaztion - added
				// Instead of calling InvalidateMeasure here, do it asynchronously
				// so we don't interrupt a layout updated pass.
				//this.InvalidateMeasure();
				// JJD 5/31/11 - TFS76852
				// Only call InvalidateMeasureAsynch if the dp is 
				// not a synchronuous control, i.e. it supports asynchrous processing.
				//GridUtilities.InvalidateMeasureAsynch(this);
				DataPresenterBase dp = this.DataPresenter;
				if (dp == null || !dp.IsSynchronousControl)
					GridUtilities.InvalidateMeasureAsynch(this);
				else
					this.InvalidateMeasure();

				this.SetValue(AutoFitVersionPropertyKey, this.AutoFitVersion + 1);
			}
			// JM 06-04-09 TFS14198
			else
			if (e.Property == HasNestedContentProperty)
			{
				if (this.IsLoaded)
					this.RaiseNestedContentEvent((bool)e.NewValue);
				else
					// JM 06-09-09 - Wait for Loaded instead of trying to fire the event asynchronously.
					//this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new RaiseNestedContentEventDelegate(this.RaiseNestedContentEvent), (bool)e.NewValue);
					this.Loaded += new RoutedEventHandler(OnLoaded);
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

			sb.Append("RecordPresenter: ");

			if (this.Record != null)
				sb.Append(this.Record.ToString());

			return sb.ToString();
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Events

			// JM 06-04-09 TFS14198 - Added.
			#region HideNestedContentEvent

		/// <summary>
		/// Event ID for the <see cref="HideNestedContent"/> routed event
		/// </summary>
		public static readonly RoutedEvent HideNestedContentEvent =
			EventManager.RegisterRoutedEvent("HideNestedContent", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(RecordPresenter));

		/// <summary>
		/// Occurs when the RecordPresenter's NestedContent should be hidden.
		/// </summary>
		protected virtual void OnHideNestedContent(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseHideNestedContent(RoutedEventArgs args)
		{
			args.RoutedEvent	= RecordPresenter.HideNestedContentEvent;
			args.Source			= this;
			this.OnHideNestedContent(args);
		}

		/// <summary>
		/// Occurs when the RecordPresenter's NestedContent should be hidden.
		/// </summary>
		public event EventHandler<RoutedEventArgs> HideNestedContent
		{
			add
			{
				base.AddHandler(RecordPresenter.HideNestedContentEvent, value);
			}
			remove
			{
				base.RemoveHandler(RecordPresenter.HideNestedContentEvent, value);
			}
		}

			#endregion //HideNestedContentEvent

			// JM 06-04-09 TFS14198 - Added.
			#region ShowNestedContentEvent

		/// <summary>
		/// Event ID for the <see cref="ShowNestedContent"/> routed event
		/// </summary>
		public static readonly RoutedEvent ShowNestedContentEvent =
			EventManager.RegisterRoutedEvent("ShowNestedContent", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(RecordPresenter));

		/// <summary>
		/// Occurs when the RecordPresenter's NestedContent should be shown.
		/// </summary>
		protected virtual void OnShowNestedContent(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseShowNestedContent(RoutedEventArgs args)
		{
			args.RoutedEvent	= RecordPresenter.ShowNestedContentEvent;
			args.Source			= this;
			this.OnShowNestedContent(args);
		}

		/// <summary>
		/// Occurs when the RecordPresenter's NestedContent should be shown.
		/// </summary>
		public event EventHandler<RoutedEventArgs> ShowNestedContent
		{
			add
			{
				base.AddHandler(RecordPresenter.ShowNestedContentEvent, value);
			}
			remove
			{
				base.RemoveHandler(RecordPresenter.ShowNestedContentEvent, value);
			}
		}

			#endregion //ShowNestedContentEvent

		#endregion //Events

		#region Properties

		#region Public Properties

		#region CellPresentation

		/// <summary>
		/// Identifies the 'CellPresentation' dependency property
		/// </summary>
		public static readonly DependencyProperty CellPresentationProperty = DependencyProperty.Register("CellPresentation",
			typeof(CellPresentation), typeof(RecordPresenter), new FrameworkPropertyMetadata(CellPresentation.GridView, null, new CoerceValueCallback(CoerceCellPresentation)));

		private static object CoerceCellPresentation(DependencyObject target, object value)
		{
			RecordPresenter rp = target as RecordPresenter;

			if (rp != null)
			{
				DataPresenterBase dp = rp.DataPresenter;

				if (dp != null)
				{
					// Always return the value based on the View so they can't mess with it if the rp is inside a DP 
					return dp.CurrentViewInternal.CellPresentation;
				}
			}

			return value;
		}
		/// <summary>
		/// Returns the layout mode of the generated styles
		/// </summary>
		[DefaultValue(CellPresentation.GridView)]
		//[Description("Returns the layout mode of the generated styles")]
		public CellPresentation CellPresentation
		{
			get
			{
				return (CellPresentation)this.GetValue(RecordPresenter.CellPresentationProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.CellPresentationProperty, value);
			}
		}

		#endregion //CellPresentation

		#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		public DataPresenterBase DataPresenter
		{
			get
			{
				Record rcd = this.Record;

				if (rcd != null)
					return rcd.DataPresenter;

				return null;
			}
		}

		#endregion //DataPresenterBase

		#region Description

		private static readonly DependencyPropertyKey DescriptionPropertyKey =
			DependencyProperty.RegisterReadOnly("Description",
			typeof(string), typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Description"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DescriptionProperty =
			DescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="Record"/>'s description property value (read-only)
		/// </summary>
		/// <seealso cref="DescriptionProperty"/>
		//[Description("Returns the record's description property value (read-only)")]
		//[Category("Behavior")]
		public string Description
		{
			get
			{
				// JJD 11/17/11 - TFS78651 - Optimization
				// This is kind of non-standard to do special logic in the CLR getter of a DependencyProperty
				// since if the caller used the GetValue(DescriptionProperty) method instead this logic
				// would not be performed.
				// However, since the optimization (which can be sgnificant in certain real-time updating
				// scenarios) requires us to process Description change notifications asynchronously 
				// we felt that the benefits outweighed the non-conformity.
				if (_descSyncPending)
					this.SynchronizeDescriptionProperty();

				return (string)this.GetValue(RecordPresenter.DescriptionProperty);
			}
		}

		#endregion //Description

		#region ExpansionIndicatorVisibility

		/// <summary>
		/// Identifies the 'ExpansionIndicatorVisibility' dependency property
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorVisibilityProperty = DependencyProperty.Register("ExpansionIndicatorVisibility",
			typeof(Visibility), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, null, new CoerceValueCallback(CoerceExpansionIndicatorVisibility)));

		// AS 9/3/09 TFS21581
		// Added CoerceValueCallback. The header on top of group by records was setting the ExpansionIndicator's Visibility
		// to Hidden which caused the headers on top to not line up with the records. I saw this come up when I grouped by 
		// a field, changed the datasource and then changed it back or when I toggled the allow record filtering. Talked to 
		// Joe and we determined the expansion indicator should never be visible for the header record.
		//
		private static object CoerceExpansionIndicatorVisibility(DependencyObject d, object newValue)
		{
			RecordPresenter rp = (RecordPresenter)d;

			// AS 9/9/09 TFS21581
			// There are cases where we want the expansion indicator to occupy space.
			//
			//if (rp.IsHeaderRecord)
			//    return KnownBoxes.VisibilityCollapsedBox;
			if (rp.IsHeaderRecord)
			{
                Record rcd = rp.Record;

                // JJD 9/23/09 - TFS18599
                // Also check special records (i.e. filter or summary rcds) that are
                // in a GroupByRecord collection
				if (rcd is GroupByRecord ||
                    (rcd.IsSpecialRecord && rcd.ParentCollection.RecordsType == RecordType.GroupByField))
				{
					RecordManager rm = rcd.RecordManager;

					// if the associated data records have no expansion indicator then we should
					// exclude the expansion indicator in the header record
					if (null == rm || rm.Sorted.Count == 0 || rm.Sorted[0].ExpansionIndicatorVisibility == Visibility.Collapsed)
						return KnownBoxes.VisibilityCollapsedBox;
				}
			}

			return newValue;
		}

		/// <summary>
		/// Gets/sets whether an expansion indicator is shown for the Record
		/// </summary>
		//[Description("Gets/sets whether an expansion indicator is shown for the Record")]
		//[Category("Behavior")]
		public Visibility ExpansionIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(RecordPresenter.ExpansionIndicatorVisibilityProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.ExpansionIndicatorVisibilityProperty, value);
			}
		}

		#endregion //ExpansionIndicatorVisibility

		#region FieldLayout

		/// <summary>
		/// Identifies the 'FieldLayout' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutProperty = DependencyProperty.Register("FieldLayout",
				  typeof(FieldLayout), typeof(RecordPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFieldLayoutChanged)));

		private static void OnFieldLayoutChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RecordPresenter rp = target as RecordPresenter;

			if (rp != null)
			{
				rp._cachedFieldLayout = e.NewValue as FieldLayout;

				// AS 5/3/11 TFS73495
				if (rp._cachedFieldLayout == null)
					BindingOperations.ClearBinding(rp, TemplateVersionProperty);
				else
					BindingOperations.SetBinding(rp, TemplateVersionProperty, Utilities.CreateBindingObject(FieldLayout.TemplateVersionProperty, BindingMode.OneWay, rp._cachedFieldLayout));
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
				this.SetValue(RecordPresenter.FieldLayoutProperty, value);
			}
		}

		#endregion //FieldLayout

        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedNearElementTransform

        // JJD 9/3/09 - TFS20581
        // Made internal
        //private static readonly DependencyPropertyKey FixedNearElementTransformPropertyKey =
        internal static readonly DependencyPropertyKey FixedNearElementTransformPropertyKey =
            DependencyProperty.RegisterReadOnly("FixedNearElementTransform",
            typeof(Transform), typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="FixedNearElementTransform"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixedNearElementTransformProperty =
            FixedNearElementTransformPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a transform object that can be used to scroll an element back into view if it were fixed on the near edge when using fixed fields.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This property will only returning a usable object when fixed fields are supported by 
        /// the view and fixed fields are being used - either having fixed one or more fields or setting the <see cref="FieldSettings.AllowFixing"/> 
        /// such that fixing is allowed.</p>
        /// </remarks>
        /// <seealso cref="FixedNearElementTransformProperty"/>
        /// <seealso cref="ScrollableElementTransform"/>
        /// <seealso cref="FixedFarElementTransform"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        /// <seealso cref="ViewBase.IsFixedFieldsSupported"/>
        //[Description("Returns a transform object that can be used to scroll an element back into view if it were fixed on the near edge when using fixed fields.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Transform FixedNearElementTransform
        {
            get
            {
                return (Transform)this.GetValue(RecordPresenter.FixedNearElementTransformProperty);
            }
        }

        #endregion //FixedNearElementTransform

        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFarElementTransform

        private static readonly DependencyPropertyKey FixedFarElementTransformPropertyKey =
            DependencyProperty.RegisterReadOnly("FixedFarElementTransform",
            typeof(Transform), typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="FixedFarElementTransform"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixedFarElementTransformProperty =
            FixedFarElementTransformPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a transform object that can be used to scroll an element back into view if it were fixed on the far edge when using fixed fields.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This property will only returning a usable object when fixed fields are supported by 
        /// the view and fixed fields are being used - either having fixed one or more fields or setting the <see cref="FieldSettings.AllowFixing"/> 
        /// such that fixing is allowed.</p>
        /// </remarks>
        /// <seealso cref="FixedFarElementTransformProperty"/>
        /// <seealso cref="ScrollableElementTransform"/>
        /// <seealso cref="FixedNearElementTransform"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        /// <seealso cref="ViewBase.IsFixedFieldsSupported"/>
        //[Description("Returns a transform object that can be used to scroll an element back into view if it were fixed on the far edge when using fixed fields.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Transform FixedFarElementTransform
        {
            get
            {
                return (Transform)this.GetValue(RecordPresenter.FixedFarElementTransformProperty);
            }
        }

        #endregion //FixedFarElementTransform

		#region HasChildData

		private static readonly DependencyPropertyKey HasChildDataPropertyKey =
			DependencyProperty.RegisterReadOnly("HasChildData",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasChildData"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasChildDataProperty =
			HasChildDataPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true is this record has child records to display
		/// </summary>
		/// <seealso cref="HasChildDataProperty"/>
		//[Description("Returns true is this record child records to display")]
		//[Category("Behavior")]
		public bool HasChildData
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.HasChildDataProperty);
			}
		}

		#endregion //HasChildData

		#region HasHeaderContent

		private static readonly DependencyPropertyKey HasHeaderContentPropertyKey =
			DependencyProperty.RegisterReadOnly("HasHeaderContent",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the 'HasHeaderContent' dependency property
		/// </summary>
		public static readonly DependencyProperty HasHeaderContentProperty =
			HasHeaderContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if there is header content (read-only)
		/// </summary>
		//[Description("Returns true if there is nested content (read-only)")]
		//[Category("Behavior")]
		public bool HasHeaderContent
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.HasHeaderContentProperty);
			}
		}

		#endregion //HasHeaderContent

		#region HasNestedContent

		private static readonly DependencyPropertyKey HasNestedContentPropertyKey =
			DependencyProperty.RegisterReadOnly("HasNestedContent",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the 'HasNestedContent' dependency property
		/// </summary>
		public static readonly DependencyProperty HasNestedContentProperty =
			HasNestedContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if there is nested content (read-only)
		/// </summary>
		//[Description("Returns true if there is nested content (read-only)")]
		//[Category("Behavior")]
		public bool HasNestedContent
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.HasNestedContentProperty);
			}
		}

		#endregion //HasNestedContent

		#region HasSeparatorAfter

		// SSP 5/6/08 - Summaries Feature
		// Added HasRecordSeparatorAfter and HasRecordSeparatorBefore properties to support
		// record separators.
		// 
		private static readonly DependencyPropertyKey HasSeparatorAfterPropertyKey = DependencyProperty.RegisterReadOnly(
				"HasSeparatorAfter",
				typeof( bool ),
				typeof( RecordPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="HasSeparatorAfter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSeparatorAfterProperty = HasSeparatorAfterPropertyKey.DependencyProperty;


		/// <summary>
		/// Indicates if the record displays a record separator after it.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasRecordSeparatorAfter</b> indicates if the record displays a record separator after it.
		/// The separator is displayed below or right of the record based on whether the orientation 
		/// (see <see cref="GridViewSettings.Orientation"/>) is vertical or horizontal respectively.
		/// </para>
		/// <para class="body">
		/// To enable separators, set the FieldLayoutSettings's <see cref="FieldLayoutSettings.RecordSeparatorLocation"/> property.
		/// </para>
		/// <seealso cref="HasSeparatorBefore"/>
		/// <seealso cref="FieldLayoutSettings.RecordSeparatorLocation"/>
		/// </remarks>
		//[Description( "Indicates if the record displays a record separator after it." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool HasSeparatorAfter
		{
			get
			{
				return (bool)this.GetValue( HasSeparatorAfterProperty );
			}
		}

		#endregion // HasSeparatorAfter

		#region HasSeparatorBefore

		// SSP 5/6/08 - Summaries Feature
		// Added HasRecordSeparatorAfter and HasRecordSeparatorBefore properties to support
		// record separators.
		// 

		private static readonly DependencyPropertyKey HasSeparatorBeforePropertyKey = DependencyProperty.RegisterReadOnly(
				"HasSeparatorBefore",
				typeof( bool ),
				typeof( RecordPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="HasSeparatorBefore"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasSeparatorBeforeProperty = HasSeparatorBeforePropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the record displays a record separator before it.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasRecordSeparatorBefore</b> indicates if the record displays a record separator after it.
		/// The separator is displayed above or left of the record based on whether the orientation 
		/// (see <see cref="GridViewSettings.Orientation"/>) is vertical or horizontal respectively.
		/// </para>
		/// <para class="body">
		/// To enable separators, set the FieldLayoutSettings's <see cref="FieldLayoutSettings.RecordSeparatorLocation"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="HasSeparatorBefore"/>
		/// <seealso cref="FieldLayoutSettings.RecordSeparatorLocation"/>
		//[Description( "Indicates if the record displays a record separator before it." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool HasSeparatorBefore
		{
			get
			{
				return (bool)this.GetValue( HasSeparatorBeforeProperty );
			}
		}

		#endregion // HasSeparatorBefore

		#region HeaderContent

		private static readonly DependencyPropertyKey HeaderContentPropertyKey =
			DependencyProperty.RegisterReadOnly("HeaderContent",
			typeof(object), typeof(RecordPresenter), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the 'HeaderContent' dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderContentProperty =
			HeaderContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the header content for a <see cref="DataRecordPresenter"/>.
		/// </summary>
		/// <remarks>
		/// <para class="body">Header content is only used by <see cref="DataRecordPresenter"/>s when the <see cref="FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.LabelLocationResolved"/> property returns 'SeparateHeader'.</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>Not all views support separate headers, e.g. <see cref="CarouselView"/>. The <see cref="FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.HasSeparateHeader"/> returns true if a separate header has been generated.</para>
		/// </remarks>
		/// <seealso cref="HeaderPresenter"/>
		/// <seealso cref="HasHeaderContent"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.LabelLocationResolved"/>
		//[Description("Returns the header content for the RecordPresenter.")]
		//[Category("Behavior")]
		public object HeaderContent
		{
			get { return this._headerContent; }
		}

		#endregion //HeaderContent

		#region IsAlternate

		internal static readonly DependencyPropertyKey IsAlternatePropertyKey =
			DependencyProperty.RegisterReadOnly("IsAlternate",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'IsAlternate' dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlternateProperty =
			IsAlternatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true for every other row in the list (readonly)
		/// </summary>
		public bool IsAlternate
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.IsAlternateProperty);
			}
		}

		#endregion //IsAlternate

		#region IsActive

		/// <summary>
		/// Identifies the 'IsActive' dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
				typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsActiveChanged)));

		private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RecordPresenter rp = target as RecordPresenter;

			if (rp != null)
			{
				bool newValue = (bool)(e.NewValue);

				if (newValue != rp._cachedIsActive)
					rp._cachedIsActive = newValue;

				if (rp._record != null &&
					rp._record.DataPresenter != null)
				{
					if (newValue)
					{
						if (rp._record.DataPresenter.ActiveRecord != rp._record)
							rp._record.DataPresenter.ActiveRecord = rp._record;
					}
					else
					{
						if (rp._record.DataPresenter.ActiveRecord == rp._record)
							rp._record.DataPresenter.ActiveRecord = null;
					}
				}

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                rp.UpdateVisualStates();

            }
		}

		private bool _cachedIsActive = false;

		/// <summary>
		/// Determines if this is the active record
		/// </summary>
		//[Description("Determines if this is the active record")]
		//[Category("Behavior")]
		public bool IsActive
		{
			get
			{
				return this._cachedIsActive;
			}
			set
			{
				this.SetValue(RecordPresenter.IsActiveProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsActive

		#region IsExpanded

		/// <summary>
		/// Identifies the 'IsExpanded' dependency property
		/// </summary>
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
				typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, null, new CoerceValueCallback(CoerceIsExpanded)));

		private static object CoerceIsExpanded(DependencyObject target, object value)
		{
			RecordPresenter rp = target as RecordPresenter;

			Debug.Assert(rp != null);
			Debug.Assert(value is bool);

			if (rp != null)
			{
				Record dr = rp.Record;

				if (dr != null && rp.DataPresenter != null)
				{
					// sync the value on the record
					dr.IsExpanded = (bool)value;

					// if the operation was cancelled we need to
					// allow the passed in value to be retunred from the corece
					// so any 2way bindings get maintained ut we have to resync
					// the expanded state asynchronously 
                    if (dr.IsExpanded != (bool)value)
                    {
                        // JJD 3/29/08 - added support for printing.
                        // We can't do asynchronous operations during a print
                        //
                        // MBS 7/29/09 - NA9.2 Excel Exporting
                        //if (dr.DataPresenter.IsReportControl)
                        if(dr.DataPresenter.IsSynchronousControl)
                            rp.ResyncIsExpandedState();
                        else
                            rp.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(rp.ResyncIsExpandedState));
                    }
				}
			}

			return (bool)value;
		}
        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

		private void ResyncIsExpandedState()
		{
			if ( this._record != null )
				this.IsExpanded = this._record.IsExpanded;
		}

		/// <summary>
		/// Gets/sets if the record will show its nested content.
		/// </summary>
		/// <seealso cref="NestedContent"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
		//[Description("Determines if the record is expanded")]
		//[Category("Behavior")]
		public bool IsExpanded
		{
			get
			{
				Record rcd = this.Record;

				if (rcd == null)
					return false;

				return rcd.IsExpanded;
			}
			set
			{
				this.SetValue(RecordPresenter.IsExpandedProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsExpanded

        // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
        #region IsFilterRecord

        internal static readonly DependencyPropertyKey IsFilterRecordPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFilterRecord",
            typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

        /// <summary>
        /// Identifies the 'IsFilterRecord' dependency property
        /// </summary>
        public static readonly DependencyProperty IsFilterRecordProperty =
            IsFilterRecordPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if this is a filter record (readonly)
        /// </summary>
        public bool IsFilterRecord
        {
            get
            {
                return (bool)this.GetValue(RecordPresenter.IsFilterRecordProperty);
            }
        }

        #endregion //IsFilterRecord

        // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
        #region IsFilteredOut

        internal static readonly DependencyPropertyKey IsFilteredOutPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFilteredOut",
            typeof(bool?), typeof(RecordPresenter), new FrameworkPropertyMetadata(null

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

        /// <summary>
        /// Identifies the 'IsFilteredOut' dependency property
        /// </summary>
        /// <seealso cref="IsFilteredOut"/>
        public static readonly DependencyProperty IsFilteredOutProperty =
            IsFilteredOutPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if this is record is filtered out (read-only)
        /// </summary>
        /// <value>
        /// <para class="body"><b>True</b> if the record fails to meet the current effective record filters or <b>false</b> if it does not. 
        /// However, if there are no active record filters then this property returns <b>null</b></para>
        /// </value>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        /// <seealso cref="IsFilteredOutProperty"/>
        public bool? IsFilteredOut
        {
            get
            {
                return (bool?)this.GetValue(RecordPresenter.IsFilteredOutProperty);
            }
        }

        #endregion //IsFilteredOut

		#region IsFixedOnBottom

		internal static readonly DependencyPropertyKey IsFixedOnBottomPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFixedOnBottom",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsFixedOnBottom"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFixedOnBottomProperty =
			IsFixedOnBottomPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the record is fixed at the bottom of the list of rows.
		/// </summary>
		/// <seealso cref="IsFixedOnBottomProperty"/>
		//[Description("Returns true if the record is fixed at the bottom of the list of rows.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFixedOnBottom
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.IsFixedOnBottomProperty);
			}
		}

		#endregion //IsFixedOnBottom

		#region IsFixedOnTop

		internal static readonly DependencyPropertyKey IsFixedOnTopPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFixedOnTop",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsFixedOnTop"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFixedOnTopProperty =
			IsFixedOnTopPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the record is fixed at the top of the list of rows.
		/// </summary>
		/// <seealso cref="IsFixedOnTopProperty"/>
		//[Description("Returns true if the record is fixed at the top of the list of rows.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFixedOnTop
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.IsFixedOnTopProperty);
			}
		}

		#endregion //IsFixedOnTop

		#region IsHeaderRecord

        // JJD 4/25/08 - BR31538
        // This no longer needs to be internal since it is now initialized inside 
        // the PrepareContainerForItem methid
        //internal static readonly DependencyPropertyKey IsHeaderRecordPropertyKey =
		private static readonly DependencyPropertyKey IsHeaderRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsHeaderRecord",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsHeaderRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsHeaderRecordProperty =
			IsHeaderRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the record is fixed at the top of the list of rows.
		/// </summary>
		/// <seealso cref="IsHeaderRecordProperty"/>
		//[Description("Returns true if the record is fixed at the top of the list of rows.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsHeaderRecord
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.IsHeaderRecordProperty);
			}
		}

		#endregion //IsHeaderRecord

		// JM NA 10.1 CardView - Added.
		#region IsInCard

		private static readonly DependencyPropertyKey IsInCardPropertyKey =
			DependencyProperty.RegisterReadOnly("IsInCard",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsInCard"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInCardProperty =
			IsInCardPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the RecordPresenter is being hosted by a <see cref="CardViewCard"/> in <see cref="CardView"/> (read only).
		/// </summary>
		/// <seealso cref="IsInCardProperty"/>
		//[Description("Returns true if the RecordPresenter is being hosted by a CardViewCard in CardView")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsInCard
		{
			get
			{
				return (bool)this.GetValue(RecordPresenter.IsInCardProperty);
			}
		}

		#endregion //IsInCard

        // JJD 1/8/09 - NA 2009 vol 1 - Record filtering
        #region IsRecordEnabled

        internal static readonly DependencyPropertyKey IsRecordEnabledPropertyKey =
            DependencyProperty.RegisterReadOnly("IsRecordEnabled",
            typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

        /// <summary>
        /// Identifies the 'IsRecordEnabled' dependency property
        /// </summary>
        public static readonly DependencyProperty IsRecordEnabledProperty =
            IsRecordEnabledPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns whether the record is enabled (readonly)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsEnabled"/>
        /// <seealso cref="FieldLayoutSettings.FilterAction"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.FilterActionResolved"/>
        public bool IsRecordEnabled
        {
            get
            {
                return (bool)this.GetValue(RecordPresenter.IsRecordEnabledProperty);
            }
        }

        #endregion //IsRecordEnabled

		#region IsSelected

		/// <summary>
		/// Identifies the 'IsSelected' dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
				typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, 

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            new PropertyChangedCallback(OnVisualStatePropertyChanged),



            new CoerceValueCallback(CoerceIsSelected)));

		private static object CoerceIsSelected(DependencyObject target, object value)
		{
			RecordPresenter rp = target as RecordPresenter;

			Debug.Assert(rp != null);
			Debug.Assert(value is bool);

			if (rp != null)
			{
				Record dr = rp.Record;

				if (dr != null && rp.DataPresenter != null)
				{
					// sync the value on the record
					dr.IsSelected = (bool)value;

					return dr.IsSelected;
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Determines if the record is selected
		/// </summary>
		//[Description("Determines if the record is selected")]
		//[Category("Behavior")]
		public bool IsSelected
		{
			get
			{
				Record rcd = this.Record;

				if (rcd == null)
					return false;

				return rcd.IsSelected;
			}
			set
			{
				this.SetValue(RecordPresenter.IsSelectedProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsSelected

		#region NestedContent

		private static readonly DependencyPropertyKey NestedContentPropertyKey =
			DependencyProperty.RegisterReadOnly("NestedContent",
			typeof(object), typeof(RecordPresenter), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the 'NestedContent' dependency property
		/// </summary>
		public static readonly DependencyProperty NestedContentProperty =
			NestedContentPropertyKey.DependencyProperty;

		internal void InitializeNestedContent(object nestedContent)
		{
			// AS 5/2/11 TFS63930/TFS30211
			// The _nestedContent may have been cleared (e.g. in the NestedContent getter or 
			// in the processing of the InitializeNestedDataContent) but the DP value is still 
			// set so we can't do this optimization.
			//
			//// JJD 3/11/10 - TFS28705 - Optimization 
			//// If the value hasn't changed then bail out
			//if (this._nestedContent == nestedContent)
			//    return;

			this._nestedContent = nestedContent;
			this.SetValue(NestedContentPropertyKey, this._nestedContent);
			this.SetValue(HasNestedContentPropertyKey, KnownBoxes.FromValue(this._nestedContent != null));

			Debug.Assert(nestedContent == null || nestedContent is FrameworkElement);

            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }

		/// <summary>
		/// Returns the nested content (i.e., child data) for the record based on its expanded state (read-only)
		/// </summary>
		/// <value>The nested content or null is the <see cref="IsExpanded"/> property is false.</value>
		/// <remarks>
		/// <para class="body">
		/// When expanded this will, in most cases, return a <see cref="RecordListControl"/> containing the <see cref="Record"/>'s <see cref="Infragistics.Windows.DataPresenter.Record.ViewableChildRecords"/>. However, for <see cref="ExpandableFieldRecordPresenter"/>s, if the associated <see cref="Field"/>'s <see cref="Field.DataType"/> does not implement the <see cref="System.Collections.IEnumerable"/> interface then the nested content will contain an <see cref="ExpandedCellPresenter"/> with the actual value of the <see cref="Cell"/>.</para>
		/// </remarks>
		/// <seealso cref="IsExpanded"/>
		/// <seealso cref="HasNestedContent"/>
		//[Description("Returns the nested content (i.e., child data) for the record based on its expanded state (read-only)")]
		//[Category("Behavior")]
		public object NestedContent
		{
			get
			{
				if (this._record == null || !this._record.IsExpanded)
				{
					this._nestedContent = null;
					return null;
				}

				return this._nestedContent;
			}
		}

		#endregion //NestedContent

		#region NestedContentMargin

		private static readonly DependencyPropertyKey NestedContentMarginPropertyKey =
			DependencyProperty.RegisterReadOnly("NestedContentMargin",
			typeof(Thickness), typeof(RecordPresenter), new FrameworkPropertyMetadata(new Thickness()));

		/// <summary>
		/// Identifies the 'NestedContentMargin' dependency property
		/// </summary>
		public static readonly DependencyProperty NestedContentMarginProperty =
			NestedContentMarginPropertyKey.DependencyProperty;

		/// <summary>
		/// Provides extra margins around the nested content (read-only)
		/// </summary>
		//[Description("Provides extra margins around the nested content readonly)")]
		//[Category("Behavior")]
		public Thickness NestedContentMargin
		{
			get
			{
				return (Thickness)this.GetValue(RecordPresenter.NestedContentMarginProperty);
			}
		}

		#endregion //NestedContentMargin

		#region NestedContentBackground

		/// <summary>
		/// Identifies the <see cref="NestedContentBackground"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty NestedContentBackgroundProperty = DependencyProperty.Register("NestedContentBackground",
			typeof(Brush), typeof(RecordPresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied by default templates as the background of the nested content area.
		/// </summary>
		/// <seealso cref="NestedContentBackgroundProperty"/>	
		//[Description("The brush applied by default templates as the background of the nested content area")]
		//[Category("Brushes")]
		public Brush NestedContentBackground
		{
			get
			{
				return (Brush)this.GetValue(RecordPresenter.NestedContentBackgroundProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.NestedContentBackgroundProperty, value);
			}
		}

		#endregion NestedContentBackground		

		#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey =
			DependencyProperty.RegisterReadOnly("Orientation",
			typeof(Orientation), typeof(RecordPresenter), new FrameworkPropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the orientation (vertical/horizontal) of the RecordPresenters in the containing Panel.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns the orientation (vertical/horizontal) of the RecordPresenters in the containing Panel.")]
		//[Category("Appearance")]
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(RecordPresenter.OrientationProperty); }
		}

		#endregion //Orientation

		#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly("Record",
			typeof(Record), typeof(RecordPresenter), new FrameworkPropertyMetadata());

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

		#region RecordContentAreaTemplate

		private static readonly DependencyPropertyKey RecordContentAreaTemplatePropertyKey =
			DependencyProperty.RegisterReadOnly("RecordContentAreaTemplate",
			typeof(DataTemplate), typeof(RecordPresenter), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the 'RecordContentAreaTemplate' dependency property
		/// </summary>
		public static readonly DependencyProperty RecordContentAreaTemplateProperty =
			RecordContentAreaTemplatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the style for the cell area (read-only).
		/// </summary>
		public DataTemplate RecordContentAreaTemplate
		{
			get
			{
				if ( this._cachedFieldLayout != null )
				{
					Record record = this.Record;

					// SSP 4/7/08 - Summaries Functionality
					// It's also applicable to summary records now. This is taken care of by 
					// GetRecordContentAreaTemplate implementation.
					// 
					//if ( record != null && !( record is DataRecord ) )
					//	return null;

					FieldLayoutTemplateGenerator generator = this._cachedFieldLayout.StyleGenerator;

					// if the generator hasn't been created yet then do it now
					if ( generator == null )
					{
						generator = this.DataPresenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator( this._cachedFieldLayout );

						Debug.Assert( generator != null );

						if ( generator != null )
							this._cachedFieldLayout.Initialize( generator );
					}

					if ( generator != null )
					{
						// SSP 4/7/08 - Summaries Functionality
						// Added GetRecordContentAreaTemplate method. Original code from here is moved into
						// that method.
						// 
						// --------------------------------------------------------------------------------
						return this.GetRecordContentAreaTemplate( generator );
						
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

						// --------------------------------------------------------------------------------
					}
				}

				return null;
			}
		}

		#endregion //RecordContentAreaTemplate

        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region ScrollableElementTransform

        // JJD 9/3/09 - TFS20581
        // Made internal
        //private static readonly DependencyPropertyKey ScrollableElementTransformPropertyKey =
        internal static readonly DependencyPropertyKey ScrollableElementTransformPropertyKey =
            DependencyProperty.RegisterReadOnly("ScrollableElementTransform",
            typeof(Transform), typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ScrollableElementTransform"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScrollableElementTransformProperty =
            ScrollableElementTransformPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a transform object that can be used to scroll an element back into view when using fixed fields.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This property will only returning a usable object when fixed fields are supported by 
        /// the view and fixed fields are being used - either having fixed one or more fields or setting the <see cref="FieldSettings.AllowFixing"/> 
        /// such that fixing is allowed.</p>
        /// </remarks>
        /// <seealso cref="ScrollableElementTransformProperty"/>
        /// <seealso cref="FixedNearElementTransform"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        /// <seealso cref="ViewBase.IsFixedFieldsSupported"/>
        //[Description("Returns a transform object that can be used to scroll an element back into view when using fixed fields.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Transform ScrollableElementTransform
        {
            get
            {
                return (Transform)this.GetValue(RecordPresenter.ScrollableElementTransformProperty);
            }
        }

        #endregion //ScrollableElementTransform

		#region ShouldDisplayExpandableRecordContent

		private static readonly DependencyPropertyKey ShouldDisplayExpandableRecordContentPropertyKey =
			DependencyProperty.RegisterReadOnly("ShouldDisplayExpandableRecordContent",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Identifies the 'ShouldDisplayExpandableRecordContent' dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldDisplayExpandableRecordContentProperty =
			ShouldDisplayExpandableRecordContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if ExpandableRecordContent (i.e., expansion indicator and description) should be displayed (read-only)
		/// </summary>
		//[Description("Returns true if ExpandableRecordContent (i.e., expansion indicator and description) should be displayed (read-only)")]
		//[Category("Appearance")]
		public bool ShouldDisplayExpandableRecordContent
		{
			get
			{
				return this._shouldDisplayExpandableRecordContent;
			}
		}

		#endregion //ShouldDisplayExpandableRecordContent

		#region ShouldDisplayGroupByRecordContent

		private static readonly DependencyPropertyKey ShouldDisplayGroupByRecordContentPropertyKey =
			DependencyProperty.RegisterReadOnly("ShouldDisplayGroupByRecordContent",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Identifies the 'ShouldDisplayGroupByRecordContent' dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldDisplayGroupByRecordContentProperty =
			ShouldDisplayGroupByRecordContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if GroupByRecordContent (i.e., expansion indicator and description) should be displayed (read-only)
		/// </summary>
		//[Description("Returns true if GroupByRecordContent (i.e., expansion indicator and description) should be displayed (read-only)")]
		//[Category("Appearance")]
		public bool ShouldDisplayGroupByRecordContent
		{
			get
			{
				return this._shouldDisplayGroupByRecordContent;
			}
		}

		#endregion //ShouldDisplayGroupByRecordContent

		#region ShouldDisplayRecordContent

		private static readonly DependencyPropertyKey ShouldDisplayRecordContentPropertyKey =
			DependencyProperty.RegisterReadOnly("ShouldDisplayRecordContent",
			typeof(bool), typeof(RecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Identifies the 'ShouldDisplayRecordContent' dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldDisplayRecordContentProperty =
			ShouldDisplayRecordContentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if record content should be displayed (read-only)
		/// </summary>
		//[Description("Returns true if there is nested content (read-only)")]
		//[Category("Appearance")]
		public bool ShouldDisplayRecordContent
		{
			get
			{
				return this._shouldDisplayRecordContent;
			}
		}

		#endregion //ShouldDisplayRecordContent

		#region TemplateCardView

		private static void OnTargetedTemplateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RecordPresenter rp = target as RecordPresenter;

			Debug.Assert(rp != null);

			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			//if (rp != null)
			if (rp != null && (e.OldValue == null || rp.IsVisible || rp.DataContext != null))
			{
				rp._isTemplateDirty = true;
				// JJD 4/21/11 - TFS73048 - Optimaztion - added
				// Instead of calling InvalidateMeasure here, do it asynchronously
				// so we don't interrupt a layout updated pass.
				//rp.InvalidateMeasure();
				// JJD 5/31/11 - TFS76852
				// Only call InvalidateMeasureAsynch if the dp is 
				// not a synchronuous control, i.e. it supports asynchrous processing.
				//GridUtilities.InvalidateMeasureAsynch(rp);
				DataPresenterBase dp = rp.DataPresenter;
				if (dp == null || !dp.IsSynchronousControl)
					GridUtilities.InvalidateMeasureAsynch(rp);
				else
					rp.InvalidateMeasure();
			}
		}

		/// <summary>
		/// Identifies the <see cref="TemplateCardView"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TemplateCardViewProperty = DependencyProperty.Register("TemplateCardView",
			typeof(ControlTemplate), typeof(RecordPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTargetedTemplateChanged)));

		/// <summary>
		/// Gets/sets the template that is used for views whose <see cref="ViewBase.CellPresentation"/> is <b>CardView</b>.
		/// </summary>
		/// <seealso cref="TemplateCardViewProperty"/>
		//[Description("Gets/sets the template that is used for views whose 'CellPresentation' is 'CardView'.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public ControlTemplate TemplateCardView
		{
			get
			{
				return (ControlTemplate)this.GetValue(RecordPresenter.TemplateCardViewProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.TemplateCardViewProperty, value);
			}
		}

		#endregion //TemplateCardView

		#region TemplateGridView

		/// <summary>
		/// Identifies the <see cref="TemplateGridView"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TemplateGridViewProperty = DependencyProperty.Register("TemplateGridView",
			typeof(ControlTemplate), typeof(RecordPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTargetedTemplateChanged)));

		/// <summary>
		/// Gets/sets the template that is used in the <see cref="XamDataGrid"/> control.
		/// </summary>
		/// <seealso cref="TemplateGridViewProperty"/>
		//[Description("Gets/sets the template that is used in the XamDataGrid")]
		//[Category("Appearance")]
		[Bindable(true)]
		public ControlTemplate TemplateGridView
		{
			get
			{
				return (ControlTemplate)this.GetValue(RecordPresenter.TemplateGridViewProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.TemplateGridViewProperty, value);
			}
		}

		#endregion //TemplateGridView

		#endregion //Public Properties

		#region Internal Properties

		#region AutoFitHeight

		private static readonly DependencyPropertyKey AutoFitHeightPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoFitHeight",
			typeof(double), typeof(RecordPresenter), new FrameworkPropertyMetadata(double.NaN));

		internal static readonly DependencyProperty AutoFitHeightProperty =
			AutoFitHeightPropertyKey.DependencyProperty;

		internal double AutoFitHeight
		{
			get
			{
				return this.CalculateAutoFitExtent(false); 
			}
		}

		#endregion //AutoFitHeight

		#region AutoFitWidth

		private static readonly DependencyPropertyKey AutoFitWidthPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoFitWidth",
			typeof(double), typeof(RecordPresenter), new FrameworkPropertyMetadata(double.NaN));

		internal static readonly DependencyProperty AutoFitWidthProperty =
			AutoFitWidthPropertyKey.DependencyProperty;

		internal double AutoFitWidth
		{
			get
			{
				return this.CalculateAutoFitExtent(true); 
			}
		}

		#endregion //AutoFitWidth

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        #region CellArea
        internal RecordCellAreaBase CellArea
        {
            get
            {
                return null != _cellArea
                    ? (RecordCellAreaBase)Utilities.GetWeakReferenceTargetSafe(_cellArea)
                    : null;
            }
            set
            {
                bool useValue = value == null ||
                    (_record is DataRecord && value is DataRecordCellArea) ||
                    (_record is SummaryRecord && value is SummaryRecordCellArea) ||
                    (_record == null && value is HeaderLabelArea);

                if (useValue)
                {
                    RecordCellAreaBase oldCellArea = this.CellArea;

                    if (value != oldCellArea)
                    {
						_cellArea = value != null ? new WeakReference(value) : null;

                        this.VerifyFixedFarTransform();
                    }
                }
            }
        } 
        #endregion //CellArea

        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFieldInfo

        /// <summary>
        /// Identifies the <see cref="FixedFieldInfo"/> dependency property
        /// </summary>
        internal static readonly DependencyProperty FixedFieldInfoProperty = VirtualizingDataRecordCellPanel.FixedFieldInfoProperty.AddOwner(typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Returns an object that provides scroll information.
        /// </summary>
        internal FixedFieldInfo FixedFieldInfo
        {
            get
            {
                return (FixedFieldInfo)this.GetValue(RecordPresenter.FixedFieldInfoProperty);
            }
            set
            {
                this.SetValue(RecordPresenter.FixedFieldInfoProperty, value);
            }
        }

        #endregion //FixedFieldInfo

        // JJD 11/11/09 - TFS24665 - added
        #region HasCellInEditMode
    
        internal bool HasCellInEditMode
        {
            get
            {
                DataRecord dr = this._record as DataRecord;

                if (dr == null || !dr.IsActive)
                    return false;

                DataPresenterBase dp = this.DataPresenter;

                Cell activeCell = dp != null ? dp.ActiveCell :  null;

                return (activeCell != null &&
                        activeCell.IsInEditMode &&
                        activeCell.Record == dr);
            }
        }

   	    #endregion //HasCellInEditMode	
    
        #region RecordsInViewVersion

		internal static readonly DependencyProperty RecordsInViewVersionProperty = DependencyProperty.Register("RecordsInViewVersion",
			// AS 5/9/07
			// We should not assume that we need to invalidate the measure of the parent element. Doing so was
			// causing an invalidation of the gridviewpanel measure while in the arrange because this property
			// was bumped in that routine.
			//
			//typeof(int), typeof(RecordPresenter), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.AffectsMeasure |
			//																			 FrameworkPropertyMetadataOptions.AffectsParentMeasure));
			typeof(int), typeof(RecordPresenter), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.None));






		internal int RecordsInViewVersion
		{
			get
			{
				return (int)this.GetValue(RecordPresenter.RecordsInViewVersionProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.RecordsInViewVersionProperty, value);
			}
		}

		#endregion //RecordsInViewVersion

        // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
        // Added
        #region InternalClip

        internal Geometry InternalClip
        {
            get { return this._internalClip; }
            set
            {
                if (this._internalClip != value)
                {
                    this._explicitClip = null;
                    this._combinedClip = null;
                    this._internalClip = value;
                    this.CoerceValue(ClipProperty);
                }
            }
        }

        #endregion //InternalClip	
    
		#region InternalTheme

		///// <summary>
		///// Identifies the <see cref="InternalTheme"/> dependency property
		///// </summary>
		//internal static readonly DependencyProperty InternalThemeProperty = DependencyProperty.Register("InternalTheme",
		//    typeof(string), typeof(RecordPresenter), new FrameworkPropertyMetadata(string.Empty));

		//internal string InternalTheme
		//{
		//    get
		//    {
		//        return (string)this.GetValue(RecordPresenter.InternalThemeProperty);
		//    }
		//    set
		//    {
		//        this.SetValue(RecordPresenter.InternalThemeProperty, value);
		//    }
		//}

		#endregion //InternalTheme

		#region InternalVersion

		internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
			typeof(int), typeof(RecordPresenter), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		internal int InternalVersion
		{
			get
			{
				return (int)this.GetValue(RecordPresenter.InternalVersionProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.InternalVersionProperty, value);
			}
		}

		#endregion //InternalVersion

		#region IsActiveFixedRecord



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IsActiveFixedRecord
		{
			get { return this._isActiveFixedRecord; }
			set { this._isActiveFixedRecord = value; }
		}

		#endregion //IsActiveFixedRecord	

		#region IsActiveHeaderRecord



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IsActiveHeaderRecord
		{
			get { return this._isActiveHeaderRecord; }
			set { this._isActiveHeaderRecord = value; }
		}

		#endregion //IsActiveHeaderRecord	

		#region IsArrangedInView

		internal bool IsArrangedInView
		{
			get { return this._isArrangedInView; }
			set { this._isArrangedInView = value; }
		}

		#endregion //IsArrangedInView	
    	
		#region IsBoundaryRecord







		internal bool IsBoundaryRecord
		{
			get { return this._isBoundaryRecord; }
			set
			{
				if (value == this._isBoundaryRecord)
					return;

				this._isBoundaryRecord = value;
				this._ignoreNextRecordsInViewVersionPropertyChange = true;

				if (this._isBoundaryRecord == false)
				{
					BindingOperations.ClearBinding(this, RecordPresenter.RecordsInViewVersionProperty);
				}
				else
				{
					this.SetBinding(RecordPresenter.RecordsInViewVersionProperty,
									Utilities.CreateBindingObject(DataPresenterBase.RecordsInViewVersionProperty, BindingMode.OneWay, this.DataPresenter));
				}
			}
		}

		#endregion //IsBoundaryRecord

		#region IsClearingBindings

		internal bool IsClearingBindings { get { return this._isClearingBindings; } }

		#endregion //IsClearingBindings	

		// JJD 04/12/12 - TFS108549 - Optimization
		#region IsDeactivated

		internal bool IsDeactivated
		{
			get
			{
				// get the rp's parent because there are scenarios where we create a wrapper
				// control for the rp that is then the immediate child of the panel and
				// the element whose Visibility is set to 'Collapsed' when the
				// record is deactivated by the recycling logic
				FrameworkElement parent = VisualTreeHelper.GetParent(this) as FrameworkElement;

				// if the immediate parent of the rp is a Panel then
				// check the Visibility property of the rp.
				if (parent is Panel || parent == null)
					return this.Visibility == Visibility.Collapsed;

				// otherwise check the Visibility property of the parent element
				return parent.Visibility == Visibility.Collapsed;
			}
		}

		#endregion //IsDeactivated	

		// [JM 04-18-07 BR21391]
		#region IsPreparingItemForContainer

		internal bool IsPreparingItemForContainer
		{
			get { return this._preparingItemForContainer; }
		}

		#endregion //IsPreparingItemForContainer	
    
		#region IsFocusWithinContentArea

		internal bool IsFocusWithinContentArea
		{
			get
			{
				// SSP 10/22/08 BR35625
				// Commented this out. Currently IsFocusWithinProperty property is set on the focused
				// element and then its ancestor elements recursively from bottom to top. So a
				// descendant element could have focus and this property on this element would still
				// return false. So don't use it. Instead we are already making use of CheckFocusWithinHelper
				// method below which does the right checks to find out if the focus is actually within.
				// 
				//if (false == (bool)this.GetValue(FocusWithinManager.IsFocusWithinProperty))
				//	return false;

				FrameworkElement contentSite = this.GetRecordContentSite();

				if (contentSite == null)
					return false;

                
                
                
                
                
                
                return FocusWithinManager.CheckFocusWithinHelper(contentSite);
                
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion //IsFocusWithinContentArea	

		// JJD 2/9/11 - TFS63916 - added
		#region RecordManagerForHeader

		private static readonly DependencyPropertyKey RecordManagerForHeaderPropertyKey =
			DependencyProperty.RegisterReadOnly("RecordManagerForHeader",
			typeof(RecordManager), typeof(RecordPresenter), new FrameworkPropertyMetadata(null));

		internal static readonly DependencyProperty RecordManagerForHeaderProperty =
			RecordManagerForHeaderPropertyKey.DependencyProperty;

		internal RecordManager RecordManagerForHeader
		{
			get
			{
				return (RecordManager)this.GetValue(RecordPresenter.RecordManagerForHeaderProperty);
			}
		}

		#endregion //RecordManagerForHeader

		#region TreatAsCollapsed







		internal bool TreatAsCollapsed
		{
			get { return this._treatAsCollapsed; }
			set 
			{
				if (value != this._treatAsCollapsed)
				{
					this._treatAsCollapsed = value;
					// JJD 5/22/07
					// Set the the new VisibilityResolved readonly property which the Visibility property is bound to.
					// We need to use a binding now so the new Recycling logic doesn't conflict
					// with our internal settings
//					this.SetValue(RecordPresenter.VisibilityProperty, KnownBoxes.FromValue(this.VisibilityToUse));
					this.SetValue(RecordPresenter.VisibilityResolvedPropertyKey, KnownBoxes.FromValue(this.VisibilityToUse));
				}
			}
		}

		#endregion //TreatAsCollapsed	

		// JJD 3/11/11 - TFS67970 - Optimization
		#region WeakRef

		// JJD 3/11/11 - TFS67970 - Optimization
		// Cache a weak reference to this RecordPresenter to be used by its associated Record
		// This prevents heap fragmentation when this object is recycled
		internal WeakReference WeakRef
		{
			get
			{
				if (_weakRef == null)
					_weakRef = new WeakReference(this);

				return _weakRef;
			}
		}

		#endregion //WeakRef	

		#endregion //Internal Properties

		#region Private Properties

			#region AutoFitVersion

		private static readonly DependencyPropertyKey AutoFitVersionPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoFitVersion",
			typeof(int), typeof(RecordPresenter), new FrameworkPropertyMetadata(0));

		private static readonly DependencyProperty AutoFitVersionProperty =
			AutoFitVersionPropertyKey.DependencyProperty;

		private int AutoFitVersion
		{
			get
			{
				return (int)this.GetValue(RecordPresenter.AutoFitVersionProperty);
			}
		}

			#endregion //AutoFitVersion

            // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
            #region FixedFieldOffset

        internal static readonly DependencyProperty FixedFieldOffsetProperty = VirtualizingDataRecordCellPanel.FixedFieldOffsetProperty.AddOwner(
            typeof(RecordPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        private static void OnFixedFieldOffsetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RecordPresenter)d).VerifyFixedFarTransform();
        }

            #endregion //FixedFieldOffset

            // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
            #region FixedFieldViewportExtent

        internal static readonly DependencyProperty FixedFieldViewportExtentProperty = VirtualizingDataRecordCellPanel.FixedFieldViewportExtentProperty.AddOwner(
            typeof(RecordPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

            #endregion //FixedFieldViewportExtent

			#region ParentRecordAutoFitVersion

		private static readonly DependencyProperty ParentRecordAutoFitVersionProperty = DependencyProperty.Register("ParentRecordAutoFitVersion",
			typeof(int), typeof(RecordPresenter), new FrameworkPropertyMetadata(0));

		private int ParentRecordAutoFitVersion
		{
			get
			{
				return (int)this.GetValue(RecordPresenter.ParentRecordAutoFitVersionProperty);
			}
			set
			{
				this.SetValue(RecordPresenter.ParentRecordAutoFitVersionProperty, value);
			}
		}

			#endregion //ParentRecordAutoFitVersion

			// AS 5/3/11 TFS73495
			#region TemplateVersionProperty
		private static DependencyProperty TemplateVersionProperty = FieldLayout.TemplateVersionProperty.AddOwner(typeof(RecordPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTemplateVersionChanged)));

		private static void OnTemplateVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RecordPresenter)d).InitializeFixedFieldInfo();
		}
			#endregion //TemplateVersionProperty

			#region VisibilityToUse

		private Visibility VisibilityToUse
		{
			get
			{
				if (this.TreatAsCollapsed == true)
					return Visibility.Collapsed;

				if (this.Record != null)
					return this.Record.VisibilityResolved;

				Debug.Assert(this.Record != null, "Record is null!");

				return Visibility.Collapsed;
			}
		}

			#endregion //VisibilityToUse	

			#region VisibilityResolved

		private static readonly DependencyPropertyKey VisibilityResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("VisibilityResolved",
			typeof(Visibility), typeof(RecordPresenter), new FrameworkPropertyMetadata(Visibility.Visible));

		private static readonly DependencyProperty VisibilityResolvedProperty =
			VisibilityResolvedPropertyKey.DependencyProperty;

		private Visibility VisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue(RecordPresenter.VisibilityResolvedProperty);
			}
		}

		#endregion //VisibilityResolved
    
		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

			#region FromRecord
		
		/// <summary>
		/// Returns the <see cref="RecordPresenter"/> that represents a specific record.
		/// </summary>
		/// <param name="record">The associated record.</param>
		/// <returns>The <see cref="RecordPresenter"/> that represents the record or null if not found in the visual tree.</returns>
		/// <exception cref="ArgumentNullException">If record is null.</exception>
		/// <seealso cref="Record"/>
		/// <seealso cref="CellValuePresenter.FromCell(Cell)"/>
		/// <seealso cref="CellValuePresenter.FromRecordAndField(DataRecord,Field)"/>
		/// <seealso cref="DataPresenterBase.RecordsInViewChanged"/>
		/// <seealso cref="DataPresenterBase.GetRecordsInView(bool)"/>
		/// <seealso cref="DataPresenterBase.BringRecordIntoView(Record)"/>
		/// <seealso cref="DataPresenterBase.BringCellIntoView(Cell)"/>
		public static RecordPresenter FromRecord(Record record)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			return record.AssociatedRecordPresenter;
		}

			#endregion //FromRecord

		#endregion //Public Methods

		#region Internal Methods

		#region CleanUp






		internal void CleanUp()
		{
			if (this.IsBoundaryRecord == true)
				this.IsBoundaryRecord = false;
		}

		#endregion //CleanUp

		#region ClearContainerForItem







		internal void ClearContainerForItem(object item)
		{
			this.ClearContainerForItem(item, false);
		}

		internal void ClearContainerForItem(object item, bool isUsedForHeaderOnly)
		{
			this._isClearingBindings = true;

			try
			{
				this._parentContentPresenter = null;

				// AS 6/22/09 NA 2009.2 Field Sizing
				//// AS 6/11/09 TFS18382
				//this._parentCPExtentTracker = null;
				_parentCPHeightTracker = _parentCPWidthTracker = null;

				// JJD 3/26/07
				// unhook the LayoutUpdated event 
				// JJD 2/16/12 - TFS101387
				// Instead of unhooking just set _layoutUpdatedWired to false so we will bypass the processing logic
				// in the OnLayoutUpdated callback passed into the DataPresenterBase's WireLayoutUpdated method
				//if ( this._layoutUpdatedHandler != null )
				//{
				//    this.LayoutUpdated -= this._layoutUpdatedHandler;
				//    this._layoutUpdatedHandler = null;
				//}
				_layoutUpdatedWired = false;

				// ser the FieldLayout to null
				this.FieldLayout = null;

				BindingOperations.ClearAllBindings(this);

				// call InitializeRecord with null which will unhook from the record's events 
				this.InitializeRecord(null, isUsedForHeaderOnly);
			}
			finally
			{
				this._isClearingBindings = false;
			}

			// AS 6/21/11 TFS79160
			// Since the record peer includes the peers of the element it was associated with we should 
			// dirty the peer so its children can be requiried.
			//
			if (item is Record)
				((Record)item).InvalidatePeer();
		}

		#endregion //ClearContainerForItem

		#region FocusIfAppropriate

		internal void FocusIfAppropriate()
		{
			DataPresenterBase dp = this.DataPresenter;

			if (dp == null)
				return;

			if (false == this.IsFocusWithinContentArea)
			{
				// JJD 3/12/07
				// if the datapresenter has keyboard focus within we want to take keyboard focus
				if (dp.IsKeyboardFocusWithin)
					this.Focus();
				else
					// JJD 3/14/07
					// Otherwise if the datapresenter has logial focus within we want to take logical focus
					if (true == (bool)dp.GetValue(FocusWithinManager.IsFocusWithinProperty))
					{
						DependencyObject scope = FocusManager.GetFocusScope(this);

						if (scope != null)
						{
							// AS 5/11/12 TFS104724
							//FocusManager.SetFocusedElement(scope, this);
							Utilities.SetFocusedElement(scope, this);
						}
					}
			}
		}

		#endregion //FocusIfAppropriate	
    
		#region GetRecordContentSite

		internal FrameworkElement GetRecordContentSite()
		{
			if (this.ShouldDisplayRecordContent == false)
				return null;

			DependencyObject contentSite = base.GetTemplateChild("PART_RecordContentSite");

			if (contentSite == null)
			{
				// AS 8/26/09
				// Added IsLoaded check since we could get in here while the template is being applied
				// during the initial measure call by the containing panel.
				//
				// SSP 9/11/09
				// After discussing this with Andrew
				//Debug.Assert(!this.IsLoaded || VisualTreeHelper.GetChildrenCount(this) == 0, "There is no PART_RecordContentSite so we're going to do a more expensive descendant walk");

				return Utilities.GetDescendantFromType(this, typeof(DataRecordCellArea), true) as FrameworkElement;
			}
 
			//Debug.Assert( contentSite is FrameworkElement);

			return contentSite as FrameworkElement;
		}

		#endregion //GetRecordContentSite	

		#region GetHeaderContentSite

		internal FrameworkElement GetHeaderContentSite()
		{
			DependencyObject contentSite = base.GetTemplateChild("PART_HeaderContentSite");

			return contentSite as FrameworkElement;
		}

		#endregion //GetHeaderContentSite	

		#region GetNestedContentSite

		internal FrameworkElement GetNestedContentSite()
		{
			DependencyObject contentSite = base.GetTemplateChild("PART_NestedContentSite");

			//Debug.Assert( contentSite is FrameworkElement );

			return contentSite as FrameworkElement;
		}

		#endregion //GetNestedContentSite	

		#region GetRecordContentAreaTemplate

		// SSP 4/7/08 - Summaries Functionality
		// Added GetRecordContentAreaTemplate method.
		// 
		/// <summary>
		/// Gets the record content area template from the generator. For data presenter, this returns
		/// a template that creates data cell area.
		/// </summary>
		/// <param name="generator">Field layout generator from which to get the template.</param>
		/// <returns>The data template</returns>
		internal virtual DataTemplate GetRecordContentAreaTemplate( FieldLayoutTemplateGenerator generator )
		{
			return null;
		}

		#endregion // GetRecordContentAreaTemplate

		#region GetRecordDescription

		// SSP 6/2/08 - Summaries Functionality
		// Added DescriptionWithSummaries property on the GroupByRecord.
		// 
		internal virtual string GetRecordDescription( )
		{
			Record record = this.Record;
			return null != record ? record.Description : null;
		}

		#endregion // GetRecordDescription

		// AS 8/21/09 TFS19388
		// The VirtualizingDataRecordCellPanel was using the Record property of the 
		// record presenter to obtain the indent level. Well for a header record that 
		// will always be a data record so if you have group by you end up picking up 
		// more indent than needed.
		//
		#region GetRecordForMeasure
		internal virtual Record GetRecordForMeasure()
		{
			return this.Record;
		} 
		#endregion //GetRecordForMeasure

		#region GetDefaultTemplateProperty

		internal abstract ControlTemplate GetDefaultTemplateProperty(DependencyProperty templateProperty);

		#endregion //GetDefaultTemplateProperty	
    
		#region InitializeExpandableRecordContentVisibility







		internal void InitializeExpandableRecordContentVisibility(bool showExpandableRecordContent)
		{
			if (showExpandableRecordContent != this._shouldDisplayExpandableRecordContent)
			{
				this._shouldDisplayExpandableRecordContent = showExpandableRecordContent;
				this.SetValue(RecordPresenter.ShouldDisplayExpandableRecordContentPropertyKey, KnownBoxes.FromValue(showExpandableRecordContent));
			}
		}

		#endregion InitializeExpandableRecordContentVisibility

		#region InitializeGroupByRecordContentVisibility







		internal void InitializeGroupByRecordContentVisibility(bool showGroupByRecordContent)
		{
			if (showGroupByRecordContent != this._shouldDisplayGroupByRecordContent)
			{
				this._shouldDisplayGroupByRecordContent = showGroupByRecordContent;
				this.SetValue(RecordPresenter.ShouldDisplayGroupByRecordContentPropertyKey, KnownBoxes.FromValue(showGroupByRecordContent));
			}
		}

		#endregion InitializeGroupByRecordContentVisibility

		#region InitializeHeaderContent

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        internal static readonly object TemplateRecordPresenterId = new object();

		internal void InitializeHeaderContent(bool addHeader)
		{
			if (addHeader == false || (this.FieldLayout != null &&
										 this.FieldLayout.StyleGenerator != null &&
										 this.FieldLayout.StyleGenerator.HasSeparateHeader == false))
			{
				// JJD 10/26/11 - TFS91364
				// If this is a HeaderRecord then it always needs HeaderContent so let
				// them fall thru.
				if (!(this.Record is HeaderRecord))
				{
					if (this._headerContent != null)
					{
						// AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
						//if (this._headerContent is HeaderPresenter)
						//	this.FieldLayout.ReturnHeaderPresenterToCache(this._headerContent as HeaderPresenter);
						HeaderPresenter oldHeader = this._headerContent as HeaderPresenter;

						if (null != oldHeader && this.Tag != TemplateRecordPresenterId)
							this.FieldLayout.ReturnHeaderPresenterToCache(oldHeader);

						this._headerContent = null;
						this.SetValue(RecordPresenter.HeaderContentPropertyKey, null);
						this.SetValue(RecordPresenter.HasHeaderContentPropertyKey, KnownBoxes.FalseBox);
					}

					return;
				}
			}

			// JJD 2/9/11 - TFS63916 
			// Get the record manager for the header
			RecordManager rm = null;

			DataPresenterBase dp = this.DataPresenter;

			if ( dp != null && dp.IsExportControl == false && dp.IsReportControl == false )
				rm = GridUtilities.GetRPRecordManager(this, this.FieldLayout, true);

			// If we already have header content, return;
			if (this._headerContent != null)
			{
				// JJD 2/9/11 - TFS63916 
				// Make sure our new internal property that holds the record manager for the header
				// is in sync before returning
				if (rm != null)
					this.SetValue(RecordManagerForHeaderPropertyKey, rm);
				else
					this.ClearValue(RecordManagerForHeaderPropertyKey);

				return;
			}

			// Get a HeaderPresenter from the FieldLayout's cache of HeaderPresenters.
            // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
            // We want the template record to include the label presenters so we can 
            // get their preferred size.
            //
			//this._headerContent = this.FieldLayout.GetHeaderPresenterFromCache();
            if (this.Tag == TemplateRecordPresenterId)
            {
                HeaderPresenter hp = new HeaderPresenter();
                hp.Content = this._cachedFieldLayout;
                hp.SetValue(HeaderPresenter.ContentTemplateProperty, this._cachedFieldLayout.StyleGenerator.TemplateDataRecordHeaderAreaTemplate);
                hp.SetBinding(HeaderPresenter.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._cachedFieldLayout));
                this._headerContent = hp;
            }
            else
                this._headerContent = this.FieldLayout.GetHeaderPresenterFromCache();

			this.SetValue(RecordPresenter.HeaderContentPropertyKey, this._headerContent);
			this.SetValue(RecordPresenter.HasHeaderContentPropertyKey, KnownBoxes.TrueBox);

			// JJD 2/9/11 - TFS63916 
			// Set our new internal property that holds the record manager for the header
			if (rm != null)
				this.SetValue(RecordManagerForHeaderPropertyKey, rm);
			else
				this.ClearValue(RecordManagerForHeaderPropertyKey);

			HeaderPresenter header = this._headerContent as HeaderPresenter;

			// JJD 2/9/11 - TFS63916 
			// Set the RecordPresenter property on the header so it can keep track of changes
			// to the RecordManagerForHeader property
			if (header != null)
				header.RecordPresenter = this;
		}

		#endregion //InitializeHeaderContent

		// JM 10-14-08 TFS7711 
		#region InitializeIsAlternate

		internal void InitializeIsAlternate(bool force)
		{
			if (force)
				this._isAlternateInitialized = false;

			this.InitializeIsAlternate();
		}

		#endregion //InitializeIsAlternate

		#region InitializeRecordContentVisibility

		internal void InitializeRecordContentVisibility(bool showRecordContent)
		{
			// JJD 10/26/11 - TFS91364
			// We never want to show record content for a header record
			if (showRecordContent && this.Record is HeaderRecord)
				showRecordContent = false;

			if (showRecordContent != this._shouldDisplayRecordContent)
			{
				this._shouldDisplayRecordContent = showRecordContent;
				this.SetValue(RecordPresenter.ShouldDisplayRecordContentPropertyKey, KnownBoxes.FromValue(showRecordContent));

				
				
				this.SynchronizeSeparatorVisibility( );
			}

			// JJD 11/23/11 - TFS13486
			// If we are showing the record content then make sure we set
			// the IsFixedOnTop and IsFixedOnBottom properties approriately
			if (showRecordContent)
				this.InitializeIsFixedOnTopOrBottom();
		}

		#endregion InitializeRecordContentVisibility

		// JJD 4/29/11 - TFS74075 - added
		#region InvalidateStyle

		internal void InvalidateStyle()
		{
			this._styleSelectorHelper.InvalidateStyle();
		}

		#endregion //InvalidateStyle	
    
		// JJD 2/7/11 - TFS35853 - added
		#region InvalidateGridViewPanelFlat

		internal void InvalidateGridViewPanelFlat()
		{
			DataPresenterBase dp = this.DataPresenter;

			if ( dp == null )
				return;

			ViewBase view = dp.CurrentViewInternal;

			if (view == null)
				return;

			Type panelType = view.ItemsPanelType;

			if (panelType != null && typeof(GridViewPanelFlat).IsAssignableFrom(panelType))
			{
				GridViewPanelFlat gvp = Utilities.GetAncestorFromType(this, typeof(GridViewPanelFlat), true) as GridViewPanelFlat;

				if (gvp != null)
					gvp.InvalidateMeasure();
			}
		}

		#endregion //InvalidateGridViewPanelFlat	
    
		#region OnIsFocusWithinChanged

		/// <summary>
		/// Called when the element either recieves or loses the logical focus.
		/// </summary>
		/// <param name="gotFocus"></param>
		internal void OnIsFocusWithinChanged( bool gotFocus )
		{
			if (gotFocus)
			{
				// try to activate the record
				if (this.Record != null &&
                    // JJD 1/4/08 - BR22682 - make sure the Record equals the DataContext so we don't activate the wrong record
                    // while in the middle of a RecordPresenter recycling operation
                    this.Record == this.DataContext &&
					!this.IsActive &&
					// JM BR29242 12-20-07 - Also check to make sure this is not a RecordPresenter for a Header record
					this.IsHeaderRecord == false &&
					this.Record.IsAncestorOf(this.DataPresenter.ActiveRecord) == false)
					this.IsActive = true;
			}
            
            
            
            
            
            
            
            
            
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // OnIsFocusWithinChanged

		#region PrepareContainerForItem







		internal void PrepareContainerForItem(object item)
		{
            // JJD 7/30/09 - NA 2009 Vol 2 - Enhanced grid view
            // If the item is a HeaderRecord then pass in true for isUsedForHeaderOnly
			//this.PrepareContainerForItem(item, false);
			this.PrepareContainerForItem(item, item is HeaderRecord);
		}

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="item">The Record to be contained.</param>
		/// <param name="isUsedForHeaderOnly">Indicates this is a special presenter used to display headers only.</param>
		internal virtual void PrepareContainerForItem(object item, bool isUsedForHeaderOnly)
		{

			// JJD 4/14/07
			// If a null item gets passed in then just set this visibility to collapsed and return.
			// Note: This can happen if the generate caused a lazy creation of the Record and
			// its Visibility was set to Collapsed in the InitializeRecord event
			if (item == null)
			{
				this.Visibility = Visibility.Collapsed;
				return;
			}

			// JJD 07/10/12 - TFS115647
			// Verify that the VisibilityProperty binding that was set in the constructor is still valid
			// by checking to see if the local value is a BindingExpression. If it isn't then reset it here.
 			// This covers a situation where RecordPresenters that we discarded were cached by the application
			// and then returned from the View's GetContainerForRecordPresenter method (i.e. raising them from the dead).
			object localVal = this.ReadLocalValue(VisibilityProperty);
			if (!(localVal is BindingExpression))
				this.SetBinding(RecordPresenter.VisibilityProperty, Utilities.CreateBindingObject(VisibilityResolvedProperty, BindingMode.OneWay, this));

			// set a flag so we can bypass style invalidations while in this method
			this._preparingItemForContainer = true;

			try
			{
				// JJD 10/26/11 - TFS91364
				// If the item is a header record then we should set the isUsedForHeaderOnly to true
				if (item is HeaderRecord)
					isUsedForHeaderOnly = true;

				// JJD 4/25/08 - BR31538
                // Make sure we initialize the IsHeaderRecord property before we call InitializeRecord
                if ( isUsedForHeaderOnly )
                    this.SetValue(RecordPresenter.IsHeaderRecordPropertyKey, KnownBoxes.TrueBox);

				this.InitializeRecord(item, isUsedForHeaderOnly);

				Record record = this.Record;

				Debug.Assert(record != null, "Record is null in RecordPresenter.PrepareContainerForItem!");
				if (this.Record == null)
					return;

				Debug.Assert(record.FieldLayout != null, "Record.FieldLayout is null in RecordPresenter.PrepareContainerForItem!");
				if (record.FieldLayout == null)
					return;

				DataPresenterBase dp = this.DataPresenter;
				ViewBase currentView = dp.CurrentViewInternal;

				Debug.Assert(dp != null, "DataPresenterBase is null in RecordPresenter.PrepareContainerForItem!");
				if (dp == null)
					return;

				this.FieldLayout = record.FieldLayout;
				this.CellPresentation = currentView.CellPresentation;
				this.SetBinding(RecordPresenter.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this.FieldLayout));
				//this.SetBinding(RecordPresenter.InternalThemeProperty, Utilities.CreateBindingObject(DataPresenterBase.ThemeProperty, BindingMode.OneWay, dp));

				if (isUsedForHeaderOnly == false)
				{
					// JJD 11/23/11 - TFS13486
					// Refactored logic into helper method so we can call it for existing record presenters
					this.InitializeIsFixedOnTopOrBottom();
				}

				// Set the Visibility property to our VisibilityToUse property which takes the
				// TreatAsCollaped property into account.
				// JJD 5/22/07
				// Set the the new VisibilityResolved readonly property which the Visibility property is bound to.
				// We need to use a binding now so the new Recycling logic doesn't conflict
				// with our internal settings
				//this.SetValue(RecordPresenter.VisibilityProperty, KnownBoxes.FromValue(this.VisibilityToUse));
				this.SetValue(RecordPresenter.VisibilityResolvedPropertyKey, KnownBoxes.FromValue(this.VisibilityToUse));


				// Bind the ExpansionIndicatorVisibility 
				Binding binding = new Binding();
				binding.Path = new PropertyPath("ExpansionIndicatorVisibility");
				binding.Mode = BindingMode.OneWay;
				binding.Source = this.Record;
				this.SetBinding(RecordPresenter.ExpansionIndicatorVisibilityProperty, binding);


				this.SetValue(RecordPresenter.RecordContentAreaTemplatePropertyKey, this.RecordContentAreaTemplate);
				//this.InitializeMinWidth();

				this.InitializeNestedDataContent();

				
                
                
                
				

				this._styleSelectorHelper.InvalidateStyle();

				this._cachedTemplate = this.Template;
				this._hasBeenPrepared = true;
				this._isTemplateDirty = true;
				this.SetTemplateBasedOnTemplateType();

				// synchronize the IsExpanded state
				if ((bool)(this.GetValue(IsExpandedProperty)) == true)
					this._record.IsExpanded = true;

				// JM 08-14-08 - BR31526, BR31531 - If our isAlternate status has been previously initialized, make sure it gets set/reset properly.
				if (this._isAlternateInitialized == true)
				{
					this._isAlternateInitialized = false;
					this.InitializeIsAlternate();
				}

				// JM NA 10.1 CardView.
				this.SetValue(RecordPresenter.IsInCardPropertyKey, (currentView is CardView) ? KnownBoxes.TrueBox : KnownBoxes.FalseBox);
			}
			finally
			{
				// reset the flags
				this._preparingItemForContainer = false;
			}

			// AS 6/21/11 TFS79160
			// Since the record peer includes the peers of the element it was associated with we should 
			// dirty the peer so its children can be requiried.
			//
			if (null != _record)
				_record.InvalidatePeer();
		}

		#endregion //PrepareContainerForItem

        // JJD 2/2/09 - NA 2009 vol 1 - record filtering
        #region SetIsFilteredOutPropertyHelper

        internal static void SetIsFilteredOutPropertyHelper(FrameworkElement fe, Record record)
        {
            bool? isFilteredOut = record != null ? record.InternalIsFilteredOutNullable_NoVerify : null;

            if (isFilteredOut != null)
                fe.SetValue(RecordPresenter.IsFilteredOutPropertyKey, KnownBoxes.FromValue(isFilteredOut.Value));
            else
                fe.ClearValue(IsFilteredOutPropertyKey);
        }

        #endregion //SetIsFilteredOutPropertyHelper	
    
		#region SynchronizeDescriptionProperty

		// SSP 6/2/08 - Summaries Functionality
		// Added DescriptionWithSummaries property on the GroupByRecord.
		// 
		internal void SynchronizeDescriptionProperty( )
		{
			// JJD 11/17/11 - TFS78651 - Optimization
			// clear the pending flag
			_descSyncPending = false;

			this.SetValue( DescriptionPropertyKey, this.GetRecordDescription( ) );
		}

		#endregion // SynchronizeDescriptionProperty
    
		// JJD 11/17/11 - TFS78651 - Optimization
		#region SynchronizeDescriptionPropertyAsync

		internal void SynchronizeDescriptionPropertyAsync()
		{
			// if a desc sync is already pendinf then return
			if (_descSyncPending)
				return;

			// set the pendi ng flag to true
			_descSyncPending = true;

			// Synchronize the desciprion asynchronously in case we get multiple
			// notifications in a tight loop. This is possible especially with
			// GroupByRecords in real time updating scenarios
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new GridUtilities.MethodDelegate(this.SynchronizeDescriptionProperty));
		}

		#endregion // SynchronizeDescriptionPropertyAsync

		#region SynchronizeSeparatorVisibility

		
		
		/// <summary>
		/// Syncrhonizes RecordPresenter's HasSeparatorBefore and HasSeparatorAfter with the record's
		/// SeparatorVisibility property.
		/// </summary>
		private void SynchronizeSeparatorVisibility( )
		{
			Record record = _record;
			bool shouldDisplayRecordContent = this.ShouldDisplayRecordContent;
			RecordSeparatorVisibility separatorLocation = null != record && this.ShouldDisplayRecordContent
				? record.SeparatorVisibility : RecordSeparatorVisibility.None;

			this.SetValue( HasSeparatorAfterPropertyKey, RecordSeparatorVisibility.After == separatorLocation );
			this.SetValue( HasSeparatorBeforePropertyKey, RecordSeparatorVisibility.Before == separatorLocation );
		}

		#endregion // SynchronizeSeparatorVisibility

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        #region VerifyFixedFarTransform
        internal void VerifyFixedFarTransform()
        {
            TranslateTransform tt = (TranslateTransform)this.GetValue(FixedFarElementTransformProperty);
            FixedFieldInfo ffi = (FixedFieldInfo)this.GetValue(FixedFieldInfoProperty);
            RecordCellAreaBase cellArea = this.CellArea;
            FieldLayout fl = this.FieldLayout;

            if (null != ffi && null != cellArea && null != fl)
            {
                bool isHorizontal = fl.IsHorizontal;
                double cellAreaExtent = isHorizontal ? cellArea.ActualHeight : cellArea.ActualWidth;
				double farEdgeExtent = Math.Min(ffi.Extent, VirtualizingDataRecordCellPanel.CalculateFixedRecordAreaEdge(isHorizontal, cellAreaExtent, this,  null, this, false));
                double fixedAreaExtent = ffi.ViewportExtent;
                double offset = ffi.Offset;
                double renderOffset = isHorizontal ? cellArea.RenderOffset.Y : cellArea.RenderOffset.X;
                double farEdge = -Math.Min(cellAreaExtent, Math.Max(farEdgeExtent - fixedAreaExtent - offset, 0));

                if (tt == null)
                {
                    tt = new TranslateTransform();
                    this.SetValue(FixedFarElementTransformPropertyKey, tt);
                }
                else if (isHorizontal)
                    tt.X = 0;
                else
                    tt.Y = 0;

                if (isHorizontal)
                    tt.Y = farEdge - renderOffset;
                else
                    tt.X = farEdge - renderOffset;
            }
            else
            {
                this.ClearValue(FixedFarElementTransformPropertyKey);
            }
        }
        #endregion //VerifyFixedFarTransform

        #endregion //Internal Methods

		#region Protected Methods

		    #region OnRecordChanged

		
		
		
		/// <summary>
		/// Called when Record property's value has changed.
		/// </summary>
		/// <param name="oldRecord">Old record if any.</param>
		/// <param name="newRecord">New record if any.</param>
		protected virtual void OnRecordChanged( Record oldRecord, Record newRecord )
		{
			this.SynchronizeSeparatorVisibility( );


            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

			#endregion // OnRecordChanged

			#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property of the associated record has changed
		/// </summary>
		/// <param name="e">The event arguments that contain the name of the property that was changed</param>
		protected virtual void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			this.VerifyHasChildData();

			switch (e.PropertyName)
			{
				case "Description":
				case "DescriptionWithSummaries":
					// SSP 6/2/08 - Summaries Functionality
					// Added DescriptionWithSummaries property on the GroupByRecord. Use the new
					// SynchronizeDescriptionProperty method which takes that into account.
					// 
					//this.SetValue(DescriptionPropertyKey, this.Record.Description);
					// JJD 11/17/11 - TFS78651 - Optimization
					// Synchronize the desciprion asynchronously in case we get multiple
					// notifications in a tight loop. This is possible especially with
					// GroupByRecords in real time updating scenarios
					//this.SynchronizeDescriptionProperty( );
					this.SynchronizeDescriptionPropertyAsync( );
					break;

				case "ActiveCell":
				case "IsActive":
					this.IsActive = this.Record.IsActive;
					break;

                // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
                case "IsEnabledResolved":
                    this.SetValue(IsRecordEnabledPropertyKey, KnownBoxes.FromValue(this.Record.IsEnabledResolved));
                    break;

				case "IsExpanded":
					this.IsExpanded = this.Record.IsExpanded;
					this.InitializeNestedDataContent();

                    // JJD 03/16/09 - Optimization
                    // If the record is expanded then we need to invaidate the measue
                    // jup to its parent RecordListControl
                    if (this.IsExpanded)
                    {
						// SSP 8/12/09 - NAS9.2 Enhanced grid-view - Optimizations
						// Don't bother invalidating the measure on the parent record list control
						// if we are in flat view. Enclosed the existing code into the if block.
						// 
						DataPresenterBase dp = this.DataPresenter;
						bool isFlatView = null != dp && dp.IsFlatView;
						if ( !isFlatView )
						{
							FrameworkElement fe = this;

							while ( fe != null )
							{
								fe.InvalidateMeasure( );
								fe = VisualTreeHelper.GetParent( fe ) as FrameworkElement;

								if ( fe is RecordListControl )
									break;
							}
						}
                    }
                    break;

                    // JJD 2/9/09 - TFS13685/TFS13781
                    // Initialize the nested content when the 'HasChildren' state changes
                case "HasChildren":
                    // JJD 03/16/09 - Optimization
                    // Only initialize the nested content if the state has actually changed
                    if (this.HasChildData != this._record.HasChildData ||
                         this.HasChildData != this.HasNestedContent)
                        this.InitializeNestedDataContent();
					break;

                // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
                case "IsFilteredOut":
                    {
                        SetIsFilteredOutPropertyHelper(this, this.Record);
                        break;
                    }

				case "IsFixed":
					if (this.Record.IsFixed)
					{
						if (this.Record.IsOnTopWhenFixed)
						{
							this.SetValue(IsFixedOnTopPropertyKey, KnownBoxes.TrueBox);
							this.ClearValue(IsFixedOnBottomPropertyKey);
						}
						else
						{
							this.ClearValue(IsFixedOnTopPropertyKey);
							this.SetValue(IsFixedOnBottomPropertyKey, KnownBoxes.TrueBox);
						}
					}
					else
					{
						this.ClearValue(IsFixedOnTopPropertyKey);
						this.ClearValue(IsFixedOnBottomPropertyKey);
					}
					break;

				case "IsSelected":
					this.IsSelected = this.Record.IsSelected;
					break;

				
				
				case "SeparatorVisibility":
					this.SynchronizeSeparatorVisibility( );
					break;

				// JM 02-04-09 TFS12056
				//case "Visiblity":
				case "Visibility":
					// JJD 5/22/07
					// Set the the new VisibilityResolved readonly property which the Visibility property is bound to.
					// We need to use a binding now so the new Recycling logic doesn't conflict
					// with our internal settings
					//this.SetValue(RecordPresenter.VisibilityProperty, KnownBoxes.FromValue(this.VisibilityToUse));
					this.SetValue(RecordPresenter.VisibilityResolvedPropertyKey, KnownBoxes.FromValue(this.VisibilityToUse));
					break;
			}

			this.InitializeMinWidth();
		}

			#endregion //OnRecordPropertyChanged

            #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // set common state
            if (this.IsEnabled == false || this.IsRecordEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            // set active state
            if ( this.IsActive )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

            // set filter state
            bool? isFilteredOut = this.IsFilteredOut;

            if (isFilteredOut.HasValue && isFilteredOut.Value == true)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilteredOut, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilteredIn, useTransitions);

            // set fixed state
            Record rcd = this.Record;

            if ( rcd != null && rcd.FixedLocation != FixedRecordLocation.Scrollable )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFixed, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfixed, useTransitions);

            // set active state
            if ( this.IsSelected )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RecordPresenter rp = target as RecordPresenter;

            if ( rp != null )
                rp.UpdateVisualStates();
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

		#endregion //Protected Methods	

		#region Private Methods

		#region CalculateAutoFitExtent

		private double CalculateAutoFitExtent(bool isAutoFitWidth)
		{
			// AS 6/22/09 NA 2009.2 Field Sizing
			//DataPresenterBase dp = this.DataPresenter;
			FieldLayout fl = this.FieldLayout;

			if (fl == null)
				return double.NaN;

			if (isAutoFitWidth)
			{
				if (!fl.IsAutoFitWidth)
					return double.NaN;
			}
			else
			{
				if (!fl.IsAutoFitHeight)
					return double.NaN;
			}

			if (this._parentContentPresenter == null)
				return double.NaN;

			double extent;

			if (isAutoFitWidth)
				extent = this._parentContentPresenter.ActualWidth;
			else
				extent = this._parentContentPresenter.ActualHeight;

            if (double.IsNaN(extent) || extent < 1)
                // AS 1/26/09
                //return extent;
                return double.NaN;

			Thickness margin = this.Margin;

			// JM 07-01-08 BR32641 - Offset the calculated extent based on our offset within the parent content presenter.
			//if (isAutoFitWidth)
			//    return Math.Max(0,  extent - (margin.Left + margin.Right));
			//else
			//    return Math.Max(0, extent - (margin.Top + margin.Bottom));
			Point pt = this.TranslatePoint(new Point(0, 0), this._parentContentPresenter);
			if (isAutoFitWidth)
				return Math.Max(extent - (margin.Left + margin.Right + pt.X), 0);
			else
				return Math.Max(extent - (margin.Top + margin.Bottom + pt.Y), 0);
		}

		#endregion //CalculateAutoFitExtent
        
        // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
        // Added coerce callback for ClipProperty
        #region CoerceClip

        private static object CoerceClip(DependencyObject target, object value)
        {
            RecordPresenter rp = target as RecordPresenter;

            if (rp != null)
            {
                if (rp._internalClip != null)
                {
                    // if there is no clip explicitly set then clear the members
                    if (value == null)
                    {
                        rp._combinedClip = null;
                        rp._explicitClip = null;
                    }
                    else
                    {
                        // see if the explicitly set clip as changed and
                        // if so create a CombinedGeometry wiht the explicit one
                        // the user set with our internal clip
                        if (value != rp._explicitClip)
                        {
                            rp._explicitClip = value as Geometry;
                            rp._combinedClip = rp._explicitClip == null ? null : new CombinedGeometry(rp._explicitClip, rp._internalClip);
                        }

                        // if we have a combined one then return it
                        if (rp._combinedClip != null)
                            return rp._combinedClip;
                    }

                    return rp._internalClip;
                }
            }

            return value;
        }

        #endregion //CoerceClip	
    
		#region FindFocusableDescendant

		private static FrameworkElement FindFocusableDescendant(FrameworkElement parent)
		{
			if (parent.Focusable)
				return parent;

			int childCount = VisualTreeHelper.GetChildrenCount(parent);

			if (childCount == 0)
				return null;

			Queue<FrameworkElement> queue = null;

			for (int i = 0; i < childCount; i++)
			{
				FrameworkElement child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

				if (child != null)
				{
					if (child.Focusable)
						return child;

					if (queue == null)
						queue = new Queue<FrameworkElement>();

					queue.Enqueue(child);
				}
			}

			if (queue != null)
			{
				foreach (FrameworkElement fe in queue)
				{
					FrameworkElement focusableChild = FindFocusableDescendant(fe);

					if (focusableChild != null)
						return focusableChild;
				}
			}
			return null;

		}

		#endregion //FindFocusableDescendant	

		// JM 08-14-08 - BR31526, BR31531 - Created this routine from code that was in the InitializeVersionInfo method - a call to
		// this method has been inserted in its place.  This method is also called from PrepareContainerForItemOverride.
		#region InitializeIsAlternate

		private void InitializeIsAlternate()
		{
			if (this._isAlternateInitialized == false)
			{
				// JJD 10/13/10 - TFS57200
				// Create a stack variable so we can check it below
				bool isAlternate = false;

				if (this._cachedFieldLayout.HighlightAlternateRecordsResolved == false)
				{
					this._isAlternateInitialized = true;
					this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FalseBox);
				}
				else
				{
					object context = this.DataContext;

					if (context != null)
					{
						int index = -1;

						Record record = context as Record;

						if (record != null)
						{
							// JJD 10/26/11 - TFS91364 
							// Ignore HeaderReords 
							//if (record is DataRecord)
							if (record is DataRecord &&
								!(record is HeaderRecord))
								// JM 10-03-08 [BR34631 TFS6222]
								//index = ((DataRecord)record).OverallScrollPosition;
								index = ((DataRecord)record).VisibleIndex;
						}
						else
						{
                            // JJD 12/12/07
                            // The TemplateDataRecord will not be in the ItemsControl so bypass
                            // trying to get its index
							// AS 6/24/09 NA 2009.2 Field Sizing
                            //if (this.Record != this._cachedFieldLayout.TemplateDataRecord)
                            // JJD/AS 8/27/09 
                            // Since this could be a template GroupBy record check the tag instead 
                            //if (this.Record is TemplateDataRecord == false)
                            if (this.Tag != TemplateRecordPresenterId)
                            {
                                ItemsControl ic = Utilities.GetAncestorFromType(this, typeof(ItemsControl), true) as ItemsControl;
                                if (ic != null)
                                {
                                    index = ic.Items.IndexOf(context);

                                    Debug.Assert(index >= 0);
                                 }
                            }
						}

						if (index >= 0)
						{
							isAlternate = index % 2 > 0;
							this._isAlternateInitialized = true;
							if (isAlternate)
								this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.TrueBox);
							else
								this.ClearValue(RecordPresenter.IsAlternatePropertyKey);
						}
					}
				}

				// JJD 10/13/10 - TFS57200
				// If the DataRecordCellArea's IsAlternate doesn't match then set it now
				DataRecordCellArea drca = Utilities.GetWeakReferenceTargetSafe(this._cellArea) as DataRecordCellArea;

				if (drca != null && drca.IsAlternate != isAlternate)
					drca.SetIsAlternate(isAlternate);

			}
		}

		#endregion //InitializeIsAlternate

		// JJD 11/23/11 - TFS13486
		#region InitializeIsFixedOnTopOrBottom

		private void InitializeIsFixedOnTopOrBottom()
		{
			Record record = this.Record;

			if (record != null && record.IsFixed)
			{
				if (record.IsOnTopWhenFixed)
				{
					this.SetValue(IsFixedOnTopPropertyKey, KnownBoxes.TrueBox);
					this.ClearValue(IsFixedOnBottomPropertyKey);
				}
				else
				{
					this.ClearValue(IsFixedOnTopPropertyKey);
					this.SetValue(IsFixedOnBottomPropertyKey, KnownBoxes.TrueBox);
				}
			}
			else
			{
				this.ClearValue(IsFixedOnTopPropertyKey);
				this.ClearValue(IsFixedOnBottomPropertyKey);
			}
		}

		#endregion //InitializeIsFixedOnTopOrBottom	
    
        // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
        #region InitializeFixedFieldInfo
        private void InitializeFixedFieldInfo()
        {
            // do not do this for a template data record
            if (this.Tag == RecordPresenter.TemplateRecordPresenterId)
                return;

			// AS 5/3/11 TFS73495
			// Since the interalversion and the template version could be bumped and 
			// the latter multiple times we'll defer processing of this.
			//
			if (_isInitializeFixedFieldInfoPending)
				return;

			DataPresenterBase dp = this.DataPresenter;

			if (null != dp && !dp.IsSynchronousControl)
			{
				_isInitializeFixedFieldInfoPending = true;
				this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(InitializeFixedFieldInfoImpl));
			}
			else
			{
				this.InitializeFixedFieldInfoImpl();
			}
		}

		// AS 5/3/11 TFS73495
		// Moved implementation into a separate routine so we can defer processing it.
		//
		private void InitializeFixedFieldInfoImpl()
		{
			// AS 5/3/11 TFS73495
			_isInitializeFixedFieldInfoPending = false;

            DataPresenterBase dp = this.DataPresenter;
            FixedFieldInfo ffi = null != dp ? dp.GetFixedFieldInfo(this) : null;
            RecordListControl rlc = _nestedContent as RecordListControl;

            if (null != ffi)
            {
                bool isFixedAreaVertical = dp.CurrentViewInternal.LogicalOrientation == Orientation.Horizontal;

                this.SetValue(FixedFieldInfoProperty, ffi);
                this.SetValue(FixedNearElementTransformPropertyKey, ffi.GetTransform(FixedFieldLocation.FixedToNearEdge, isFixedAreaVertical));
                this.SetValue(ScrollableElementTransformPropertyKey, ffi.GetTransform(FixedFieldLocation.Scrollable, isFixedAreaVertical));
                this.SetBinding(FixedFieldOffsetProperty, Utilities.CreateBindingObject(FixedFieldInfo.OffsetProperty, BindingMode.OneWay, ffi));
                this.SetBinding(FixedFieldViewportExtentProperty, Utilities.CreateBindingObject(FixedFieldInfo.ViewportExtentProperty, BindingMode.OneWay, ffi));
            }
            else
            {
                this.ClearValue(FixedFieldInfoProperty);
                this.ClearValue(FixedNearElementTransformPropertyKey);
                this.ClearValue(ScrollableElementTransformPropertyKey);
                this.ClearValue(FixedFieldOffsetProperty);
                this.ClearValue(FixedFieldViewportExtentProperty);
                this.ClearValue(FixedFarElementTransformPropertyKey);
            }

            this.VerifyFixedFarTransform();

            if (null != rlc)
            {
                rlc.SetValue(FixedNearElementTransformPropertyKey, this.FixedNearElementTransform);
                rlc.SetValue(ScrollableElementTransformPropertyKey, this.ScrollableElementTransform);
            }
        }
        #endregion //InitializeFixedFieldInfo

        #region InitializeMinWidth

        private void InitializeMinWidth()
		{
			
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //InitializeMinWidth

		#region InitializeNestedDataContent

        // JJD 03/16/09 
        // Made internal so we could call it from DataPresenterBase.OnLayoutUpdated
        //private void InitializeNestedDataContent()
		internal void InitializeNestedDataContent()
		{
            // JJD 03/16/09 
            // make sure the record has been initialized
            if (this._record == null)
                return;

			// SSP 8/12/09 - NAS9.2 Enhanced grid-view - Optimizations
			// 
			DataPresenterBase dp = this.DataPresenter;
			bool isFlatView = null != dp && dp.IsFlatView;
			if ( isFlatView && null == _nestedContent )
			{
				ExpandableFieldRecord expandableRecord = _record as ExpandableFieldRecord;
				if ( null == expandableRecord || expandableRecord.Field.IsExpandableByDefault )
					return;
			}

			// JM 08-06-09 TFS 19866 - Header records cannot have nested content.
			if (this.IsHeaderRecord)
				return;

            // JJD 03/16/09 - Optimization
            // If the record is expanded but not visible then just set and flag
            // and wait until its IsVisible property is set to true
            if (this.IsExpanded == true &&
                this.IsVisible == false)
            {
                this._nestedDataRequiresInitalization = true;
                return;
            }

            // JJD 03/16/09 - Optimization
            // Clear the flag and initialize the nested content now
            this._nestedDataRequiresInitalization = false;

            this.VerifyHasChildData();

			// If we are not expanded, remove existing nested content (if any).
			if (this._record.IsExpanded == false)
			{
				if (this._nestedContent != null)
				{
					RecordListControl nestedRlc = this._nestedContent as RecordListControl;
					if (nestedRlc != null)
						BindingOperations.ClearAllBindings(nestedRlc);

					this._nestedContent = null;
				}

				this.InitializeNestedContent(null);

				return;
			}

            // JJD 3/29/08 - added support for printing
            if (this.DataPresenter.IsReportControl)
            {
                
                return;
            }

			// if the dp doesn't allow nested data to display then we
			// can stop here and return
			if (this.DataPresenter.IsNestedDataDisplayEnabled == false)
				return;

			// JM 8-24-06 If the layout mode is not gridview, just exit.
			if (this.CellPresentation != CellPresentation.GridView)
				return;

			// JJD 2/7/11 - TFS35853 - added flag
			bool forceRecordListControl = false;
			
			// If we have no child records, just exit.
			if (!this.HasChildData)
			{
				// JJD 2/7/11 - TFS35853
				// If the is a GroupByFieldLayout we always want to create a child
				// RecordListControl so that we can display the header.
				// This can happen when all descendant records are filtered out.
				switch (_record.RecordType )
				{
					case RecordType.GroupByFieldLayout:
						forceRecordListControl = true;
						break;
				}

				if (!forceRecordListControl )
					return;
			}
			else
			{
				if (_record.RecordType == RecordType.ExpandableFieldRecord &&
					_record.HasChildrenInternal)
					forceRecordListControl = true;
			}

			FrameworkElement nestedContent;

            // JJD 3/9/10 = TFS25465
            // Use HasVisibleChildren instead since HasChildren can sometimes trigger the load
            // of grand child DataRecords in certain scenarios
            //if (this._record.HasChildren)
			// JJD 2/7/11 - TFS35853
			// If the is a GroupByFieldLayout we always want to create a cj=hild
			// RecordListControl so that we can display the header.
			// This can happen when all descenadat records are filtered out.
			//if (this._record.HasVisibleChildren)
			if (this._record.HasVisibleChildren || forceRecordListControl)
			{
                // JJD 7/30/09 - NA 2009 Vol 2 - Enhanced grid view
                // If we are using a flattened list then ste the NestedContent to null
                if (this.DataPresenter.IsFlatView)
                {
                    this.InitializeNestedContent(null);
                    return;
                }

				// Create a RecordListControl as our nested content to display our child records.
				RecordListControl nestedRecordListControl = this._nestedContent as RecordListControl;

				if (nestedRecordListControl == null)
				{
					// JJD 2/22/12 - TFS101199 
					// Pass false in for the isRoot parameter
					//nestedRecordListControl = new RecordListControl(this.DataPresenter);
					nestedRecordListControl = new RecordListControl(this.DataPresenter, false);
				}

				// JM 01-29-09 BR33445 [TFS6021]
				//nestedRecordListControl.SetBinding(ItemsControl.BackgroundProperty, Utilities.CreateBindingObject(DataPresenterBase.BackgroundProperty, BindingMode.OneWay, this.DataPresenter));
				
				nestedRecordListControl.SetBinding(ItemsControl.ItemsPanelProperty, Utilities.CreateBindingObject(DataPresenterBase.ItemsPanelProperty, BindingMode.OneWay, this.DataPresenter));

                // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
                nestedRecordListControl.SetValue(FixedNearElementTransformPropertyKey, this.FixedNearElementTransform);
                nestedRecordListControl.SetValue(ScrollableElementTransformPropertyKey, this.ScrollableElementTransform);

				// Get and apply the appropriate style for this RecordListControl
				Style style = this.DataPresenter.GetRecordListControlStyle(this._record.FieldLayout);
				if (style != null)
					nestedRecordListControl.Style = style;


				// Bind the RecordListControl's ItemsSource property to the ViewableChildRecords
				Binding binding = new Binding();
				binding.Mode = BindingMode.OneWay;
				binding.Source = this._record;
				binding.Path = new PropertyPath("ViewableChildRecords");

				nestedRecordListControl.SetBinding(ItemsControl.ItemsSourceProperty, binding);

				nestedContent = nestedRecordListControl;
			}
			else
			{
				ExpandableFieldRecord expandableRecord = this._record as ExpandableFieldRecord;

				// JJD 7/17/07
				// Unnecessary assert this condition can be valid
				//Debug.Assert(expandableRecord != null);

				if (expandableRecord == null)
					return;

				ExpandedCellPresenter expandedCell = this._nestedContent as ExpandedCellPresenter;
				
				if ( expandedCell == null )
					expandedCell = new ExpandedCellPresenter(this.DataPresenter, expandableRecord);

				expandedCell.DataContext = expandableRecord.ParentDataRecord;
				expandedCell.Field = expandableRecord.Field;
				expandedCell.Orientation = this.Orientation;

				expandedCell.Content = expandableRecord.ParentDataRecord.GetCellValue(expandableRecord.Field, true);

				nestedContent = expandedCell;
			}

			this.InitializeNestedContent(nestedContent);
		}

		#endregion //InitializeNestedDataContent

		#region InitializeRecord

		private void InitializeRecord(object item, bool isUsedForHeaderOnly)
		{
			Record record = item as Record;

			if (record != this._record)
			{
				Record oldRecord = _record;

				// Unhook from the old record's property change notification 
				if (this._record != null)
				{
					// unhook the event listener for the old record
					if (isUsedForHeaderOnly == false)
					{
						PropertyChangedEventManager.RemoveListener(this._record, this, string.Empty);

                        // JJD 1/26/08 - BR30085
                        // null out the reference the record has to this presenter
                        // JJD 4/25/08 
                        // Only null it out if it is pointing to this recordpresenter
                        if ( this._record.AssociatedRecordPresenter == this )
                            this._record.AssociatedRecordPresenter = null;
					}
				}

				this._record = record;

				// Hook the new record's property change notification 
				if (this._record != null)
				{
					this._hasRecordEverBeenInitialized = true;

					// Set the back reference to this object on the record
					if (isUsedForHeaderOnly == false)
					{
						// use the weak event manager to hook the event so we don't get rooted
						PropertyChangedEventManager.AddListener(this._record, this, string.Empty);

						this._record.AssociatedRecordPresenter = this;

						// initialize the Description property
						// SSP 6/2/08 - Summaries Functionality
						// Added DescriptionWithSummaries property on the GroupByRecord. Use the new
						// SynchronizeDescriptionProperty method which takes that into account.
						// 
						//this.SetValue(DescriptionPropertyKey, this._record.Description);
						this.SynchronizeDescriptionProperty( );

						// JJD 5/29/07 -
						// Always set the value explicitly so we can reuse the rp for different records
						// If this is the active record then initialize its IsActive property
						//if (this._record.IsActive)
						//    this.SetValue(IsActiveProperty, KnownBoxes.TrueBox);
						this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this._record.IsActive));

						// JJD 5/29/07 -
						// Always set the value explicitly so we can reuse the rp for different records
						// If the record is expanded then initialize its IsExpanded property
						//if (this._record.IsExpanded)
						//    this.SetValue(IsExpandedProperty, KnownBoxes.TrueBox);
						this.SetValue(IsExpandedProperty, KnownBoxes.FromValue(this._record.IsExpanded));

						// JJD 5/29/07 -
						// Always set the value explicitly so we can reuse the rp for different records
						// If the record is selected then initialize its IsSelected property
						//if (this._record.IsSelected)
						//    this.SetValue(IsSelectedProperty, KnownBoxes.TrueBox);
						this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(this._record.IsSelected));
                        
                        // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
                        this.SetValue(IsFilterRecordPropertyKey, KnownBoxes.FromValue((bool)(this._record is FilterRecord)));
                        this.SetValue(IsRecordEnabledPropertyKey, KnownBoxes.FromValue(this._record.IsEnabledResolved));
                        SetIsFilteredOutPropertyHelper(this, this._record);
                    }
                    else
                    {
                        // JJD 8/10/09 - NA 2009 Vol 2 - Enhanced grid view 
                        // For HeaderRecords we still need to set the AssociatedRecordPresenter
                        if ( this._record is HeaderRecord )
                            this._record.AssociatedRecordPresenter = this;
                    }

					// AS 9/9/09 TFS21581
					if (isUsedForHeaderOnly)
						this.CoerceValue(ExpansionIndicatorVisibilityProperty);

					this.SetValue(RecordPropertyKey, this._record);
				}

				
				
				
				this.OnRecordChanged( oldRecord, _record );
			}
		}

		#endregion //InitializeRecord

		#region InitializeVersionInfo

		private void InitializeVersionInfo()
		{
			if (this._cachedFieldLayout != null &&
				this._cachedFieldLayout.DataPresenter != null)
			{
				// AS 6/28/11 TFS77655
				// Do not release the header of a RP associated with a HeaderRecord. This RP will 
				// not be recycled for a DataRecord or GroupByRecord. In this case what was happening 
				// is the HeaderAutomationPeer found this RP but wouldn't use it because its HasHeaderContent 
				// was false.
				//
				//if (this._headerContent != null)
				if (this._headerContent != null && this.Record is HeaderRecord == false)
				{
					this._headerContent = null;
					this.SetValue(RecordPresenter.HeaderContentPropertyKey, null);
					this.SetValue(RecordPresenter.HasHeaderContentPropertyKey, KnownBoxes.FalseBox);
				}


				if (this._cachedFieldLayout.StyleGenerator != null)
				{
					int version = this.InternalVersion;

					if (this._cachedVersion != version)
					{
						bool firstTimeThru = this._cachedVersion == 0;
						this.ClearValue(WidthProperty);
						this.ClearValue(HeightProperty);

						this.InitializeMinWidth();

						this._isAlternateInitialized = false;

						this._cachedVersion = version;

						if (this._versionInitialized == true)
						{
							if ( this._preparingItemForContainer == false)
								this._styleSelectorHelper.InvalidateStyle();

							// SSP 6/11/08 BR32836
							// We need update the content area template in summary record as well.
							// 
							if ( this is DataRecordPresenter || this is SummaryRecordPresenter )
								this.SetValue(RecordPresenter.RecordContentAreaTemplatePropertyKey, this.RecordContentAreaTemplate);
						}

						this.SetValue(RecordPresenter.OrientationPropertyKey, KnownBoxes.FromValue(this.FieldLayout.StyleGenerator.LogicalOrientation));

						ViewBase currentView = this.DataPresenter.CurrentViewInternal;
						this.CellPresentation = currentView.CellPresentation;

                        
                        
                        
                        

						Record record = this.Record;

						// JJD 2/22/07 
						// Access the IsExpanded property which may triiger a the AllowAddNew setting
						// for ExpandableFieldRecords
						int scrollCount = record.ScrollCountInternal;
						Visibility indicatorVisibility = record.ExpansionIndicatorVisibility;

						// if this is a datarecord expanded in cardview collapse it
						// since we don't have a way to support this yet
						// [JM BR25648 08-13-07]
						//if (this is DataRecordPresenter && currentView.CellPresentation == CellPresentation.CardView)
						if ((this is DataRecordPresenter || this is GroupByRecordPresenter) && currentView.CellPresentation == CellPresentation.CardView)
							record.IsExpanded = false;
					}

					// initialize the IsAlternate property setting
					// JM 08-14-08 - BR31526, BR31531 - Comment out the code that initializes the IsAlternate flag into a separate so it can be called 
					// from here AND PrepareContainerForItemOverride.
					#region Commented-out code
					//if (this._isAlternateInitialized == false)
					//{
					//    if (this._cachedFieldLayout.HighlightAlternateRecordsResolved == false)
					//    {
					//        this._isAlternateInitialized = true;
					//        this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FalseBox);
					//    }
					//    else
					//    {
					//        object context = this.DataContext;

					//        if (context != null)
					//        {
					//            int index = -1;

					//            Record record = context as Record;

					//            if (record != null)
					//            {
					//                if (record is DataRecord)
					//                    index = ((DataRecord)record).OverallScrollPosition;
					//            }
					//            else
					//            {
					//                // JJD 12/12/07
					//                // The TemplateDataRecord will not be in the ItemsControl so bypass
					//                // trying to get its index
					//                if (this.Record != this._cachedFieldLayout.TemplateDataRecord)
					//                {
					//                    ItemsControl ic = Utilities.GetAncestorFromType(this, typeof(ItemsControl), true) as ItemsControl;
					//                    if (ic != null)
					//                    {
					//                        index = ic.Items.IndexOf(context);

					//                        Debug.Assert(index >= 0);
					//                     }
					//                }
					//            }

					//            if (index >= 0)
					//            {
					//                bool alternate = index % 2 > 0;
					//                this._isAlternateInitialized = true;
					//                this.SetValue(RecordPresenter.IsAlternatePropertyKey, KnownBoxes.FromValue(alternate));
					//            }
					//        }
					//    }
					//}
					#endregion //Commented-out code
					this.InitializeIsAlternate();

					this.InitializeMinWidth();

                    // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
                    this.InitializeFixedFieldInfo();

                    this._versionInitialized = true;
				}
			}
		}

		#endregion //InitializeVersionInfo

		#region OnAutoFitExtentChanged
		private void OnAutoFitExtentChanged()
		{
			// this should be invoked when the ActualWidth changes. that should happen 
			// when the contextlayoutmanager raises the size changed events. right after 
			// that it will raise the layout updated. we want to just hook the layout 
			// updated as we would have in the arrange

			// JJD 2/16/12 - TFS101387
			// Check the _layoutUpdatedWired flag. If false set it to true and call the 
			// DataPresenterBase's WireLayoutUpdated method.
			// But only if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			//if (this._layoutUpdatedHandler == null)
			//{
			//    this._layoutUpdatedHandler = new EventHandler(OnLayoutUpdated);
			//    this.LayoutUpdated += this._layoutUpdatedHandler;
			//}
			if (false ==_layoutUpdatedWired && (this.IsVisible || this.DataContext != null ))
			{
				DataPresenterBase dp = this.DataPresenter;

				if (dp != null)
				{
					_layoutUpdatedWired = true;
					dp.WireLayoutUpdated(new GridUtilities.MethodDelegate(this.OnLayoutUpdated));
				}
			}
		} 
		#endregion //OnAutoFitExtentChanged

		// AS 2/25/11 TFS66934
		#region OnExpansionIndicatorClick
		private void OnExpansionIndicatorClick(object sender, RoutedEventArgs e)
		{
			// to maintain the existing behavior we'll use the command to perform the action
			// JJD 3/17/11
			// Call CanEecute first. This will allow wiring of the PreviewCanExecute and CanExecute
			// routed events
			if (DataPresenterCommands.ToggleRecordIsExpanded.CanExecute(this.Record, this))
				DataPresenterCommands.ToggleRecordIsExpanded.Execute(this.Record, this);
		}
		#endregion //OnExpansionIndicatorClick

        #region OnLayoutUpdated
		//private void OnLayoutUpdated(object sender, EventArgs e)
		private void OnLayoutUpdated()
		{
			// JJD 2/16/12 - TFS101387
			// Check the _layoutUpdatedWired flag instead. 
			//if ( this._layoutUpdatedHandler != null )
			if ( this._layoutUpdatedWired )
			{
                // JJD 2/11/09 
                // if the this._asyncAutoFit is still pending
                // abort it and null out the member so we don't
                // end up processing UpdateAutoFitProperties twice
                if (this._asyncAutoFit != null)
                {
                    this._asyncAutoFit.Abort();
                    this._asyncAutoFit = null;
                }

				// JJD 3/27/07
				// unhook the event
				//this.LayoutUpdated -= this._layoutUpdatedHandler;
				//this._layoutUpdatedHandler = null;

				// JJD 2/16/12 - TFS101387
				// Reset the _layoutUpdatedWired flag. 
				_layoutUpdatedWired = false;

				// call UpdateAutoFitProperties
				this.UpdateAutoFitProperties();

			}
		}
		#endregion //OnLayoutUpdated

		// JM 06-09-09 Added
		#region OnLoaded

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(OnLoaded);
			this.RaiseNestedContentEvent(this.HasNestedContent);
		}

		#endregion //OnLoaded

		// JM 06-04-09 TFS14198 - Added.
		#region RaiseNestedContentEvent

		// JM 06-09-09 - No longer need this delegate since we are now hooking the Loaded event
		//delegate void RaiseNestedContentEventDelegate(bool show);
		private void RaiseNestedContentEvent(bool show)
		{
			// AS 2/11/10 TFS27515
			// If we raise the event when the template has not been applied 
			// then an exception could be generated if an animation tries to 
			// target an element in the template.
			//
			if (this.VisualChildrenCount == 0)
				return;

			if (show)
				this.RaiseShowNestedContent(new RoutedEventArgs());
			else
				this.RaiseHideNestedContent(new RoutedEventArgs());
		}


		#endregion //RaiseNestedContentEvent

        // JJD 2/11/09 - added
        #region OnUpdateAutoFitAsync

        private void OnUpdateAutoFitAsync()
        {
            if (this._asyncAutoFit != null)
            {
                // null out the member
                this._asyncAutoFit = null;

                // call UpdateAutoFitProperties
                this.UpdateAutoFitProperties();

            }
        }

        #endregion //OnUpdateAutoFitAsync	
        
		#region SetAutoFitBindings

        
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

        #endregion //SetAutoFitBindings

        #region SetTemplateBasedOnTemplateType

        private void SetTemplateBasedOnTemplateType()
		{
			DataPresenterBase dp = this.DataPresenter;

			if (dp != null)
			{
				if (this._record == null || !this._hasBeenPrepared || !this.IsInitialized)
					return;
			}

			this._isTemplateDirty = false;

			DependencyProperty templatePropertyToUse = null;
			ControlTemplate templateToUse = null;

			// get the specific template property 
			if ( this.CellPresentation == CellPresentation.GridView )
				templatePropertyToUse = RecordPresenter.TemplateGridViewProperty;
			else
				templatePropertyToUse = RecordPresenter.TemplateCardViewProperty;

			templateToUse = (ControlTemplate)this.GetValue(templatePropertyToUse);

			if ( templateToUse != null &&
				 templateToUse == this.GetDefaultTemplateProperty(templatePropertyToUse) )
			{
				if (this._cachedTemplate != null &&
					this._cachedTemplate != this.GetDefaultTemplateProperty(TemplateProperty))
					templateToUse = this._cachedTemplate;
			}

			try
			{
				// set a flag so we don't cache the specific template
				this._isSwappingTemplate = true;

				if (templateToUse == null)
				{
					#region Use the default
					// since a specific one wasn't specified then use the cached template is not the default
					if (this._cachedTemplate != null &&
						 this._cachedTemplate != this.GetDefaultTemplateProperty(TemplateProperty))
						templateToUse = this._cachedTemplate;

					// if the cached template is null then get it from the default value based on the theme
					if (templateToUse == null)
					{
						DataPresenterBase.SetDefaultValue(dp, this, Control.TemplateProperty, templatePropertyToUse, true);
						return;
					}
					#endregion //Use the default
				}

				// set the specific template
				this.Template = templateToUse;
			}
			finally
			{
				// reset the flag
				this._isSwappingTemplate = false;
			}
		}

		#endregion //SetTemplateBasedOnTemplateType	

		#region UpdateAutoFitProperties

		internal virtual void UpdateAutoFitProperties()
		{
			// JJD 3/27/07
			// get out if not arranged in view
			if (!this._isArrangedInView)
			{
				this.ClearValue(AutoFitHeightPropertyKey);
				this.ClearValue(AutoFitVersionPropertyKey);
				return;
			}

			if (this.IsHeaderRecord)
			{
				// JJD 3/27/07
				// get out if no content
				if (this.HasHeaderContent == false)
					return;
			}
			else
			{
				Record rcd = this.Record;

				// JJD 3/27/07
				// get out if the record doesn't occupy a scroll position
				if (rcd == null ||
					 rcd.OccupiesScrollPosition == false)
					return;
			}

			// JJD 3/27/07
			// get out if collapsed
            // JJD 2/3/09
            // Even if the element is collapsed let this process. Otherwise when we are in a recycling record presenters
            // our Visibility can be set toe collapsed during a reccyling deactivation of the container element. This
            // ca sometimes cause a situation when the horizontal scrollbar is shown and hidden endlessly because the
            // record presnter at the cusp never calculates it auotsize property because its Visibility is set to Collapsed.
            //if (this.Visibility == Visibility.Collapsed)
            //    return;

			// AS 6/22/09 NA 2009.2 Field Sizing
			//DataPresenterBase dp = this.DataPresenter;
			FieldLayout fl = this.FieldLayout;

			if (fl == null)
				return;

			if (fl.IsAutoFitWidth)
				this.SetValue(AutoFitWidthPropertyKey, this.AutoFitWidth);
			else
				this.ClearValue(AutoFitWidthPropertyKey);

			if (fl.IsAutoFitHeight)
				this.SetValue(AutoFitHeightPropertyKey, this.AutoFitHeight);
			else
				this.ClearValue(AutoFitHeightPropertyKey);
		}

		#endregion //UpdateAutoFitProperties

        #region VerifyHasChildData

		private void VerifyHasChildData()
		{
            // JJD 03/16/09 
            // check of dp == null
            DataPresenterBase dp = this.DataPresenter;
            if (dp == null || dp.IsNestedDataDisplayEnabled == false)
			{
				this.SetValue(RecordPresenter.HasChildDataPropertyKey, KnownBoxes.FalseBox);
				return;
			}

			Record rcd = this.Record;

			if (rcd == null)
				return;

			// JJD 2/24/07 
			// Access the expansion indicator visibility property which can change when
			// a data record is expanded. This may trigger the appropriate notification
			Visibility expansionIndicatorVisibiliy = rcd.ExpansionIndicatorVisibility;

			this.SetValue(RecordPresenter.HasChildDataPropertyKey, KnownBoxes.FromValue(rcd.HasChildData));
		}

		#endregion //VerifyHasChildData

		#endregion //Private Methods

		#endregion //Methods

		#region MinWidthConverter private class

		private class MinWidthConverter : IValueConverter
		{
			/// <summary>
			/// Constructor
			/// </summary>
			internal MinWidthConverter()
			{
			}

			object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is double)
				{
					double newValue = (double)value;

					// shrink the minwidth by a few pixels to prevent a horizontal scrollbar 
					newValue -= 6;

					// assume there is a vertical scrollbar and subtract its width
					newValue -= SystemParameters.VerticalScrollBarWidth;

					newValue = Math.Max(newValue, 0);

					return newValue;
				}

				return value;
			}

			object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return value;
			}

		}

		#endregion MinWidthConverter private class

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private RecordPresenter _rp;

			internal StyleSelectorHelper(RecordPresenter rp) : base(rp)
			{
				this._rp = rp;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._rp == null)
						return null;

					FieldLayout fl = this._rp.FieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
						{
							// JM 08-04-09 TFS 20237
							//return dp.InternalRecordStyleSelector.SelectStyle(this._rp.DataContext, this._rp);
							return dp.InternalRecordStyleSelector.SelectStyle(this._rp.Record, this._rp);
						}
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class

		#region ISelectableElement Members

		ISelectableItem ISelectableElement.SelectableItem
		{
			get
			{
				// JJD 3/14/11 - TFS67147
				// Since we don't want to be selecting the filter record return null
				if (this._record is FilterRecord)
					return null;

				return this._record as ISelectableItem;
			}
		}

		#endregion

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if ( managerType == typeof(PropertyChangedEventManager) )
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					this.OnRecordPropertyChanged(args);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordPresenter, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for RecordPresenter, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion
	}

	#endregion //RecordPresenter base class

	#region DataRecordPresenter

	/// <summary>
	/// An element that represents a <see cref="DataRecord"/> in the UI of a XamDataGrid, XamDataCarousel or XamDataPresenter.
	/// </summary>
	/// <remarks>
	/// <para class="body">Refer to the remarks for the <see cref="RecordPresenter"/> base class.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="DataRecord"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataPresenter"/>
	//[Description("A control that displays the contents of a 'DataRecord' in the 'DataPresenterBase' derived controls such as 'XamDataGrid'.")]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateChanged,         GroupName = VisualStateUtilities.GroupChange)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnchanged,       GroupName = VisualStateUtilities.GroupChange)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateAddRecord,       GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecord,      GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecordAlternateRow, GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilterRecord,    GroupName = VisualStateUtilities.GroupRecord)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateValidEx,         GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidEx,       GroupName = VisualStateUtilities.GroupValidationEx)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DataRecordPresenter : RecordPresenter
	{
		#region Member Variables

		// MD 8/19/10
		// The record presenter will now cache it's associated cell panel.
		private WeakReference associatedVirtualizingDataRecordCellPanel;

		// AS 8/21/09 TFS19388
		private WeakReference _headerRecordCollection;

		// JJD 10/21/11 - TFS86028 - Optimization 
		// Added member to temporarily hold cached autofit cell area width and/ot height from previous sibling records
		private CachedAutoFitExtent _cachedAutoFitExtent;

		#endregion //Member Variables

		#region Constructors

		static DataRecordPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataRecordPresenter), new FrameworkPropertyMetadata(typeof(DataRecordPresenter)));
		}

		#endregion //Constructors

		#region Base class overrides

			#region GetDefaultTemplateProperty

		private static bool s_DefaultTemplatesCached = false;
		private static ControlTemplate s_DefaultTemplate = null;
		private static ControlTemplate s_DefaultTemplateTabluar = null;
		private static ControlTemplate s_DefaultTemplateCardView = null;

		internal override ControlTemplate GetDefaultTemplateProperty(DependencyProperty templateProperty)
		{
			if (s_DefaultTemplatesCached == false)
			{
				lock (typeof(DataRecordPresenter))
				{
					if (s_DefaultTemplatesCached == false)
					{
						s_DefaultTemplatesCached = true;

						Style style = Infragistics.Windows.Themes.DataPresenterGeneric.DataRecordPresenter;
						Debug.Assert(style != null);
						if (style != null)
						{
							s_DefaultTemplate = Utilities.GetPropertyValueFromStyle(style, TemplateProperty, true, false) as ControlTemplate;
							s_DefaultTemplateTabluar = Utilities.GetPropertyValueFromStyle(style, TemplateGridViewProperty, true, false) as ControlTemplate;
							s_DefaultTemplateCardView = Utilities.GetPropertyValueFromStyle(style, TemplateCardViewProperty, true, false) as ControlTemplate;
						}
					}
				}
			}

			if ( templateProperty == TemplateProperty )
				return s_DefaultTemplate;

			if ( templateProperty == TemplateGridViewProperty )
				return s_DefaultTemplateTabluar;

			if ( templateProperty == TemplateCardViewProperty )
				return s_DefaultTemplateCardView;

			Debug.Fail("What are WeakEventManager doing here");
			return null;
		}

			#endregion //GetDefaultTemplateProperty	

			#region GetRecordContentAreaTemplate

		// SSP 4/7/08 - Summaries Functionality
		// Added GetRecordContentAreaTemplate method to RecordPresenter and overrode it here. 
		// Code in there is moved from the RecordPresenter's RecordContentAreaTemplate property's get.
		// 
		/// <summary>
		/// Gets the record content area template from the generator. For data presenter, this returns
		/// a template that creates data cell area.
		/// </summary>
		/// <param name="generator">Field layout generator from which to get the template.</param>
		/// <returns>The data template</returns>
		internal override DataTemplate GetRecordContentAreaTemplate( FieldLayoutTemplateGenerator generator )
		{
			// AS 6/24/09 NA 2009.2 Field Sizing
			//Record record = this.Record;
			//FieldLayout fieldLayout = this.FieldLayout;

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            // 
			//if ( record != fieldLayout.TemplateDataRecord &&
			//	fieldLayout.CanUseVirtualizedCellAreaTemplate )
			//	return generator.GeneratedVirtualRecordContentAreaTemplate;
            //
			//return generator.GeneratedRecordContentAreaTemplate;
			// AS 6/24/09 NA 2009.2 Field Sizing
            //if (record == fieldLayout.TemplateDataRecord)
            if (this.Record is TemplateDataRecord)
                return generator.TemplateDataRecordContentAreaTemplate;

            return generator.GeneratedVirtualRecordContentAreaTemplate;
		}

			#endregion // GetRecordContentAreaTemplate

			// AS 8/21/09 TFS19388
			#region GetRecordForMeasure
		internal override Record GetRecordForMeasure()
		{
			if (null != _headerRecordCollection)
			{
				ViewableRecordCollection records = Utilities.GetWeakReferenceTargetSafe(_headerRecordCollection) as ViewableRecordCollection;

				if (null != records && records.Count > 0)
					return records[0];
			}

			return this.Record;
		} 
			#endregion //GetRecordForMeasure

            #region OnApplyTemplate
        /// <summary>
        /// Invoked when the template has been applied for the element.
        /// </summary>
        public override void OnApplyTemplate()
        {
            
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


            base.OnApplyTemplate();
        } 
            #endregion //OnApplyTemplate

			// JM BR27463 12-3-07
			#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse buton is pressed
		/// </summary>
		/// <param name="e">Contains additional information about the event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (e.Handled								== false &&
				this.IsHeaderRecord						== true &&
				this.ShouldDisplayRecordContent			== false &&
				this.ShouldDisplayGroupByRecordContent	== false)
			{
                // JJD 1/9/08 - BR29519
                // The fix for BR27463 caused a regression issue that prevented
                // fields from being selected by clicking on their LabelPresenters.
                // The code below prevents e.Handled from being set to true if a LabelPresenter
                // was clicked on and the LabelClickActionResolved is not 'Nothing'.
                DependencyObject source = e.OriginalSource as DependencyObject;

                if (source != null)
                {
                    LabelPresenter lp = source as LabelPresenter;

                    if (lp == null)
                        lp = Utilities.GetAncestorFromType(source, typeof(LabelPresenter), true, this) as LabelPresenter;

                    if ( lp != null &&
                         lp.Field != null &&
                         lp.Field.LabelClickActionResolved != LabelClickAction.Nothing)
                        return;
                }
           
				e.Handled = true;
				return;
			}

		}

			#endregion //OnMouseLeftButtonDown	
    
			#region OnPropertyChanged

		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// if this record presenter is being cleared then ignore any changes
			if (this.IsClearingBindings)
				return;

			if (e.Property == RecordProperty)
			{
				DataRecord dr = e.NewValue as DataRecord;
				
				this.SetValue(DataRecordPropertyKey, dr);

                // JJD 4/25/08 - BR31538
                // Don't set the IsAddRecord or IsDataChanged record if this is a header record
				//if (dr != null)
				if (dr != null && !this.IsHeaderRecord)
				{
					this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(dr.IsAddRecord));
					this.SetValue(IsDataChangedPropertyKey, KnownBoxes.FromValue(dr.IsDataChanged));
				}
			}
			else if (e.Property == VisibilityProperty)
			{
				// JJD 12/7/11 - TFS97186 
				// when we are being un-collapsed. e.g. from the
				// recycling infrastructure we need to call RefreshAutoFitExtent
				// so we don't end up using a stale value.
				if (Visibility.Collapsed.Equals( e.OldValue) )
					this.RefreshAutoFitExtent(false);
			}
			else
				if (e.Property == ActualHeightProperty ||
					 e.Property == ActualWidthProperty)
				{
					// AS 6/22/09 NA 2009.2 Field Sizing
					//DataPresenterBase dp = this.DataPresenter;
					FieldLayout fl = this.FieldLayout;

					if (fl != null)
					{
						if (e.Property == ActualHeightProperty &&
							fl.IsAutoFitHeight)
							this.UpdateAutoFitProperties();

						if (e.Property == ActualWidthProperty &&
							fl.IsAutoFitWidth)
							this.UpdateAutoFitProperties();
					}
				}
		}

			#endregion //OnPropertyChanged	

			#region OnRecordChanged

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Overridden. Called when Record property's value has changed.
		/// </summary>
		/// <param name="oldRecord">Old record if any.</param>
		/// <param name="newRecord">New record if any.</param>
		protected override void OnRecordChanged( Record oldRecord, Record newRecord )
		{
			base.OnRecordChanged( oldRecord, newRecord );

			this.UpdateDataError( );
		}

			#endregion // OnRecordChanged

			#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property of the assocuiated record has changed
		/// </summary>
		/// <param name="e">The event arguments that contain the name of the property that was changed</param>
		protected override void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnRecordPropertyChanged( e );

            switch (e.PropertyName)
            {
                case "IsAddRecord":
                    this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(((DataRecord)this.Record).IsAddRecord));
                    break;

                case "IsDataChanged":
                    this.SetValue(IsDataChangedPropertyKey, KnownBoxes.FromValue(((DataRecord)this.Record).IsDataChanged));
                    break;

				// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
				// 
				case "DataError":
					this.UpdateDataError( );
					break;
            }
		}

			#endregion //OnRecordPropertyChanged

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            #region SetVisualState


       /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            // set record state
            if (this.IsFilterRecord)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilterRecord, useTransitions);
            else
            {
                if (this.IsAddRecord)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateAddRecord, useTransitions);
                else
                {
                    if (this.IsAlternate)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDataRecordAlternateRow, VisualStateUtilities.StateDataRecord);
                    else
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateDataRecord, useTransitions);
                }
            }

            // set validation state
            if (this.HasDataError)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInvalidEx, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateValidEx, useTransitions);
        }


            #endregion //SetVisualState

        #endregion //Base class overrides	
    
		#region Properties

			// MD 8/19/10
			// The record presenter will now cache it's associated cell panel.
			#region AssociatedVirtualizingDataRecordCellPanel

		internal VirtualizingDataRecordCellPanel AssociatedVirtualizingDataRecordCellPanel
		{
			get
			{
				if (this.associatedVirtualizingDataRecordCellPanel == null)
					return null;

				VirtualizingDataRecordCellPanel associatedVirtualizingDataRecordCellPanelTarget =
					Utilities.GetWeakReferenceTargetSafe(this.associatedVirtualizingDataRecordCellPanel) as VirtualizingDataRecordCellPanel;

				if (associatedVirtualizingDataRecordCellPanelTarget == null)
					this.associatedVirtualizingDataRecordCellPanel = null;

				return associatedVirtualizingDataRecordCellPanelTarget;
			}
			set
			{
				if (value == null)
					this.associatedVirtualizingDataRecordCellPanel = null;
				else
					this.associatedVirtualizingDataRecordCellPanel = new WeakReference(value);
			}
		}

			#endregion // AssociatedVirtualizingDataRecordCellPanel

			#region AutoFitCellAreaHeight

		private static readonly DependencyPropertyKey AutoFitCellAreaHeightPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoFitCellAreaHeight",
			// JJD 10/21/11 - TFS86028 - Optimization - added OnAutoFitExtentChanged callback
			//typeof(double), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(double.NaN));
			typeof(double), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAutoFitExtentChanged)));

		internal static readonly DependencyProperty AutoFitCellAreaHeightProperty =
			AutoFitCellAreaHeightPropertyKey.DependencyProperty;

		// JJD 10/21/11 - TFS86028 - Optimization
		// Called when a AutoFitCellArea... property is changed
		// so we can update the cached value for use on the next sibling DRP
		private static void OnAutoFitExtentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataRecordPresenter drp = target as DataRecordPresenter;

			if (drp == null || drp.TreatAsCollapsed || !drp.IsVisible)
				return;

			DataRecord dr = drp.Record as DataRecord;

			if (dr != null && dr.RecordType == RecordType.DataRecord)
			{
				FieldLayout fl = dr.FieldLayout;

				if (fl != null)
				{
					if (fl.IsAutoFitWidth && e.Property == AutoFitCellAreaWidthProperty)
						fl.TemplateDataRecordCache.SetCachedAutoFitWidth(dr, (double)e.NewValue);

					if (fl.IsAutoFitHeight && e.Property == AutoFitCellAreaHeightProperty)
						fl.TemplateDataRecordCache.SetCachedAutoFitHeight(dr, (double)e.NewValue);
				}
			}
		}

		internal double AutoFitCellAreaHeight
		{
			get
			{
				// SSP 5/23/08 BR33108
				// Made CalculateCellAreaAutoFitExtent static and added rp parameter to it.
				// 
				//return this.CalculateCellAreaAutoFitExtent(false);
				return CalculateCellAreaAutoFitExtent( this, false );
			}
		}

			#endregion //AutoFitCellAreaHeight

			#region AutoFitCellAreaWidth

		private static readonly DependencyPropertyKey AutoFitCellAreaWidthPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoFitCellAreaWidth",
			// JJD 10/21/11 - TFS86028 - Optimization - added OnAutoFitExtentChanged callback
			//typeof(double), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(double.NaN));
			typeof(double), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAutoFitExtentChanged)));

		internal static readonly DependencyProperty AutoFitCellAreaWidthProperty =
			AutoFitCellAreaWidthPropertyKey.DependencyProperty;

		internal double AutoFitCellAreaWidth
		{
			get
			{
				// SSP 5/23/08 BR33108
				// Made CalculateCellAreaAutoFitExtent static and added rp parameter to it.
				// 
				//return this.CalculateCellAreaAutoFitExtent(true);
				return CalculateCellAreaAutoFitExtent( this, true );
			}
		}

			#endregion //AutoFitCellAreaWidth

			#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the property key for read-only <see cref="DataError"/> dependency property.
		/// </summary>
		internal static readonly DependencyPropertyKey DataErrorPropertyKey = DependencyProperty.RegisterReadOnly(
			"DataError",
			typeof( object ),
			typeof( DataRecordPresenter ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="DataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty DataErrorProperty = DataErrorPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated data record's data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DataError</b> property returns the value of the associated DataRecord's
		/// <see cref="Infragistics.Windows.DataPresenter.DataRecord.DataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.DataError"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.HasDataError"/>
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

			DataRecord dr = this.DataRecord;
			if ( null != dr )
			{
				dataError = dr.DataError;
				hasDataError = dr.HasDataError;
			}

			this.SetValue( DataErrorPropertyKey, dataError );
			this.SetValue( HasDataErrorPropertyKey, hasDataError );
		}

			#endregion // DataError

			#region DataRecord

		private static readonly DependencyPropertyKey DataRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("DataRecord",
			typeof(DataRecord), typeof(DataRecordPresenter), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the 'DataRecord' dependency property
		/// </summary>
		public static readonly DependencyProperty DataRecordProperty =
			DataRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated <see cref="DataRecord"/> inside a DataPresenterBase (read-only)
		/// </summary>
		//[Description("Returns the associated DataRecord inside a DataPresenterBase (read-only)")]
		//[Category("Data")]
		public DataRecord DataRecord
		{
			get
			{
				return this.Record as DataRecord;
			}
		}

			#endregion //DataRecord

			#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the property key for read-only <see cref="HasDataError"/> dependency property.
		/// </summary>
        internal static readonly DependencyPropertyKey HasDataErrorPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasDataError",
            typeof(bool),
            typeof(DataRecordPresenter),
            new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));


		/// <summary>
		/// Identifies the read-only <see cref="HasDataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty HasDataErrorProperty = HasDataErrorPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the associated data record has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property returns the value of the associated DataRecord's
		/// <see cref="Infragistics.Windows.DataPresenter.DataRecord.HasDataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.HasDataError"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord.DataError"/>
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

			#region IsAddRecord

		private static readonly DependencyPropertyKey IsAddRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsAddRecord",
			typeof(bool), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsAddRecord"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddRecordProperty =
			IsAddRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this is a template add record
		/// </summary>
		/// <seealso cref="IsAddRecordProperty"/>
		//[Description("Returns true if this is a template add record")]
		//[Category("Behavior")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsAddRecord
		{
			get
			{
				return (bool)this.GetValue(DataRecordPresenter.IsAddRecordProperty);
			}
		}

			#endregion //IsAddRecord

			#region IsDataChanged

		private static readonly DependencyPropertyKey IsDataChangedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsDataChanged",
			typeof(bool), typeof(DataRecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsDataChanged"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDataChangedProperty =
			IsDataChangedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this is a template add record
		/// </summary>
		/// <seealso cref="IsDataChangedProperty"/>
		//[Description("Returns true if this is a template add record")]
		//[Category("Behavior")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsDataChanged
		{
			get
			{
				return (bool)this.GetValue(DataRecordPresenter.IsDataChangedProperty);
			}
		}

			#endregion //IsDataChanged

			#region HeaderAreaBackground

			/// <summary>
			/// Identifies the <see cref="HeaderAreaBackground"/> dependency property
			/// </summary>		
			public static readonly DependencyProperty HeaderAreaBackgroundProperty = DependencyProperty.Register("HeaderAreaBackground",
				typeof(Brush), typeof(DataRecordPresenter), new FrameworkPropertyMetadata((object)null));

			/// <summary>
			/// The brush applied by default templates as the background in the HeaderArea. This is the area behind the LabelPresenters .
			/// </summary>
			/// <seealso cref="HeaderAreaBackgroundProperty"/>	
			//[Description("The brush applied by default templates as the background in the HeaderArea. This is the area behind the LabelPresenters ")]
			//[Category("Brushes")]
			public Brush HeaderAreaBackground
			{
				get
				{
					return (Brush)this.GetValue(DataRecordPresenter.HeaderAreaBackgroundProperty);
				}
				set
				{
					this.SetValue(DataRecordPresenter.HeaderAreaBackgroundProperty, value);
				}
			}

			#endregion HeaderAreaBackground		

			// JJD 10/21/11 - TFS86028 - Optimization
			#region PrepareContainerForItem

			internal override void PrepareContainerForItem(object item, bool isUsedForHeaderOnly)
			{
				base.PrepareContainerForItem(item, isUsedForHeaderOnly);

				// If a null item gets passed in then just return.
				// Note: This can happen if the generate caused a lazy creation of the Record and
				// its Visibility was set to Collapsed in the InitializeRecord event
				if (item == null)
					return;

				// JJD 12/7/11 - TFS97186
				// Refactor - Moved logic for setting the autofit extent into a helper method
				this.RefreshAutoFitExtent(true);
			}

			#endregion //PrepareContainerForItem	
    			
		#endregion //Properties	
    
		#region Methods

			#region Public Methods

				#region GetChildCellValuePresenters

		/// <summary>
		/// Gets the child <see cref="CellValuePresenter"/>s
		/// </summary>
		/// <returns>An array of all the child <see cref="CellValuePresenter"/>s</returns>
		public CellValuePresenter[] GetChildCellValuePresenters()
		{
			List<CellValuePresenter> list = this.GetChildCellValuePresenterList();

			CellValuePresenter[] array = new CellValuePresenter[list.Count];

			if (list.Count > 0)
				list.CopyTo(array);

			return array;

		}

				#endregion //GetChildCellValuePresenters	
    
			#endregion //Public Methods	

			#region Internal Methods

				#region OnRecordResized

		internal void OnRecordResized()
		{
			this.InvalidateMeasure();

			UIElement cvpParent = null;
			List<CellPresenter> cellPresenters = this.GetChildCellPresenterList();

			if (cellPresenters.Count > 0)
			{
				foreach (CellPresenter cp in cellPresenters)
					cp.OnRecordResized();

				cvpParent = VisualTreeHelper.GetParent(cellPresenters[0]) as UIElement;
			}
			else
			{
				List<CellValuePresenter> cvps = this.GetChildCellValuePresenterList();

				foreach (CellValuePresenter cvp in cvps)
					cvp.InvalidateMeasure();

				cvpParent = cvps.Count > 0
					? VisualTreeHelper.GetParent(cvps[0]) as UIElement
					: null;
			}

			if (cvpParent is VirtualizingDataRecordCellPanel)
				cvpParent.InvalidateMeasure();
		}

				#endregion //OnRecordResized	

				// AS 8/21/09 TFS19388
				#region SetHeaderRecordCollection
		internal void SetHeaderRecordCollection(ViewableRecordCollection records)
		{
			_headerRecordCollection = records == null ? null : new WeakReference(records);
		}
				#endregion //SetHeaderRecordCollection
    
			#endregion //Internal Methods	
    
			#region Private Methods

				#region CalculateCellAreaAutoFitExtent

		// SSP 5/23/08 BR33108
		// Need to call this from SummaryRecordPresenter as well. Made it static and added rp parameter.
		// 
		//private double CalculateCellAreaAutoFitExtent(bool isAutoFitWidth)
		internal static double CalculateCellAreaAutoFitExtent( RecordPresenter rp, bool isAutoFitWidth )
		{
			// SSP 5/23/08 BR33108
			// 
			//DataPresenterBase dp = this.DataPresenter;
			// AS 6/22/09 NA 2009.2 Field Sizing
			//DataPresenterBase dp = rp.DataPresenter;
			FieldLayout fl = rp.FieldLayout;

			if (fl == null)
				return double.NaN;

			if (isAutoFitWidth)
			{
				if (!fl.IsAutoFitWidth)
					return double.NaN;

				// JJD 10/21/11 - TFS86028 - Optimization 
				// Use cached extent if available
				DataRecordPresenter drp = rp as DataRecordPresenter;
				if (drp != null && drp._cachedAutoFitExtent != null && drp._cachedAutoFitExtent._width.HasValue)
					return drp._cachedAutoFitExtent._width.Value;
			}
			else
			{
				if (!fl.IsAutoFitHeight)
					return double.NaN;

				// JJD 10/21/11 - TFS86028 - Optimization 
				// Use cached extent if available
				DataRecordPresenter drp = rp as DataRecordPresenter;
				if (drp != null && drp._cachedAutoFitExtent != null && drp._cachedAutoFitExtent._height.HasValue)
					return drp._cachedAutoFitExtent._height.Value;
			}

			
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			FrameworkElement clipToElement = null;
			FrameworkElement cellPanel = null;

			// AS 5/7/07 BR22294
			//this.GetCellAreaAndRestrictToElement(ref contentArea, ref clipToElement);
			bool isScaled = false;
			// SSP 5/23/08 BR33108
			// Made GetCellAreaAndRestrictToElement static and added rp parameter to it.
			// 
			//this.GetCellAreaAndRestrictToElement(ref contentArea, ref clipToElement, out isScaled);
			GetCellAreaAndRestrictToElement( rp, ref cellPanel, ref clipToElement, out isScaled );

			if (clipToElement == null ||
                // AS 1/26/09
                // If the element doesn't have an actual width/height then 
                // do not try to return an auto fit extent yet.
                //
                (isAutoFitWidth && clipToElement.ActualWidth < 1) ||
                (!isAutoFitWidth && clipToElement.ActualHeight < 1) ||
                 cellPanel == null)
				return double.NaN;

			// AS 5/7/07 BR22294
			// If the content area is within a scaled view box then we cannot rely on location of the 
			// content area with respect to this record. In this case, the recordpresenter is 150 pixels wide.
			// The content area is also 150 pixels wide. However, the translation of 0,0 of the content 
			// area with respect to the clip to element - the scrollcontentpresenter in this case - is
			// 12 because the content is within a viewbox that is within the rp since the viewbox is 
			// scaling the content area such that it horizontally and vertically fits in view.
			//
			//Point pt = contentArea.TranslatePoint(new Point(0, 0), clipToElement);
			Point pt = isScaled ? new Point() : cellPanel.TranslatePoint(new Point(0, 0), clipToElement);

			// AS 6/11/09 TFS18382
			// If we had scrolled over (presumably because the offset was updated before 
			// the control was resized smaller) then the cell area would have shifted 
			// its content into view. To account for that we need to get that element 
			// and take away any translation it may have done.
			//
			if (!isScaled)
			{
				RecordCellAreaBase cellArea = Utilities.GetAncestorFromType(cellPanel, typeof(RecordCellAreaBase), true, rp) as RecordCellAreaBase;

				if (null != cellArea)
				{
					GeneralTransform tt = cellArea.RenderTransform;
					if (tt != Transform.Identity && tt != null)
					{
						tt = tt.Inverse;
						pt = tt.Transform(pt);
					}
				}
			}
			
			// AS 4/16/09 TFS16289
            // We should not assume any initial offset. Otherwise in a situation where the window 
            // is size to content we will continually reduce the size that we return from measure
            // causing the window to reduce in size. The issue that led to the inclusion of this 1
            // pixel was a case where we were not accounting for the chrome around the nested content 
            // site and it just happened to be 1 pixel. I addressed this issue by accounting for the 
            // far nested content chrome within the loop below.
            //
            //// JJD 1/22/09 - NA 2009 vol 1
            //// start out with an adjustmetn of 1 pixel to allow for sub pixel rounding errors that
            //// might trigger a scrolbar when we don't need one
            //double adjustment = 1;
            double adjustment = 0;

			// AS 6/22/09 NA 2009.2 Field Sizing
			// Moved up since we need it to determine if autofit is enabled.
			//
            //FieldLayout fl = rp.FieldLayout;

            if (fl != null)
            {
                Record rcd = rp.Record;

                // adjust from any chrome around the virtualizing panel
                if (rcd != null)
                    adjustment += rcd.FieldLayout.TemplateDataRecordCache.GetVirtPanelOffset(true, true);

                // JJD 1/22/09 - NA 2009 vol 1
                // adjust for the cumulative far offsets of every rcd up the parent chaning
                while (rcd != null)
                {
                    adjustment += rcd.FarOffset;
                    rcd = rcd.ParentRecord;

                    // AS 4/16/09 TFS16289
                    // Add in nested content site chrome.
                    if (null != rcd)
                        adjustment += rcd.FieldLayout.TemplateDataRecordCache.GetNestedContentChrome(rcd, true);
                }
            }


            ScrollContentPresenter scp = clipToElement as ScrollContentPresenter;

            // JJD 1/22/09 - NA 2009 vol 1
            // adjust for any scrolling offset
            if (scp != null)
            {
				// AS 6/24/09
				// I found this while implementing some features in 9.2. Essentially the 
				// ScrollViewer does not update its Horizontal/VerticalOffset until its 
				// layoutupdated so the value we get here may or may not be correct depending 
				// on whether the rp's layoutupdated or the scrollviewer's layoutupdated
				// handler is called first. Since the scrollviewer just updates its info
				// from its scrollinfo, we can use that object's information. Unfortunately 
				// that's not public on the ScrollViewer or ScrollContentPresenter so we 
				// need to calculate it the way that the ScrollContentPresenter does.
				//
                //ScrollViewer sv = scp.TemplatedParent as ScrollViewer;
				IScrollInfo sv = null;

				if (scp.CanContentScroll)
				{
					sv = scp.Content as IScrollInfo;

					if (null == sv)
					{
						ItemsPresenter ip = scp.Content as ItemsPresenter;

						if (null != ip && VisualTreeHelper.GetChildrenCount(ip) > 0)
							sv = VisualTreeHelper.GetChild(ip, 0) as IScrollInfo;
					}
				}

				if (null == sv)
					sv = scp;

                if (sv != null)
                {
                    if (isAutoFitWidth)
                        adjustment += sv.HorizontalOffset;
                    else
                        adjustment += sv.VerticalOffset;
                }
            }

			// AS 5/5/10 TFS29508
			// Removed the Math.Floor call that seemed to be causing the previous issue.
			//
			//// JJD 1/22/09 - NA 2009 vol 1
			//// apply the adjustment calculate above
			////if (isAutoFitWidth)
			////    return Math.Max(1.0d, clipToElement.ActualWidth - Math.Max(0, pt.X));
			////else
			////    return Math.Max(1.0d, clipToElement.ActualHeight - Math.Max(0, pt.Y));
			//// AS 6/11/09 TFS18382
			//// It seems that the Floor call here is leading to some cycling between values
			//// when trying to compare the gridviewpanel adorner's drp size to that of the 
			//// size that it was measured with. However, we were unsure of what not doing 
			//// this could cause so we put some extra tolerance into the OnHeaderExtentChanged.
			////
			//if (isAutoFitWidth)
			//    return Math.Floor(Math.Max(clipToElement.ActualWidth - (adjustment + pt.X), 1d));
			//else
			//    return Math.Floor(Math.Max(clipToElement.ActualHeight - (adjustment + pt.Y), 1d));
			if (isAutoFitWidth)
				return Math.Max(clipToElement.ActualWidth - (adjustment + pt.X), 1d);
			else
				return Math.Max(clipToElement.ActualHeight - (adjustment + pt.Y), 1d);
		}
    
				#endregion //CalculateCellAreaAutoFitExtent

				#region GetCellAreaAndScrollContentPresenter 


		// AS 3/22/07 AutoFit
		// We were assuming that a card record contained a ScrollContentPresenter
		// but in the case of a cardview, there isn't one. Instead at that point we 
		// needed to clip to the data record presenter.
		//
		//private void GetCellAreaAndScrollContentPresenter(ref DataRecordCellArea cellarea, ref ScrollContentPresenter scp)
		// AS 5/7/07 BR22294
		//private void GetCellAreaAndRestrictToElement(ref FrameworkElement contentArea, ref FrameworkElement clipToElement)
		// SSP 5/23/08 BR33108
		// Need to call this from SummaryRecordPresenter as well. Made GetCellAreaAndRestrictToElement static and added rp parameter.
		// 
		//private void GetCellAreaAndRestrictToElement(ref FrameworkElement contentArea, ref FrameworkElement clipToElement, out bool isScaled)
		private static void GetCellAreaAndRestrictToElement( RecordPresenter rp, ref FrameworkElement cellPanel, ref FrameworkElement clipToElement, out bool isScaled )
		{
			
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			// SSP 5/23/08 BR33108
			// Need to call this from SummaryRecordPresenter as well. Made it static and added rp parameter.
			// 
			//ViewBase currentView = this.DataPresenter == null ? null : this.DataPresenter.CurrentViewInternal;
			DataPresenterBase dp = rp.DataPresenter;
			ViewBase currentView = dp == null ? null : dp.CurrentViewInternal;

			Type stopAtType;
			if (currentView != null && currentView.AutoFitToRecord)
			{
				// JJD 4/6/07
				// If the RecordPresenterContainerType returns null we should never do the ancestor walk
				//stopAtType = currentView.RecordPresenterContainerType ?? typeof(DataRecordPresenter);
				stopAtType = currentView.RecordPresenterContainerType;
			}
			else
				stopAtType = typeof(RecordListControl);

			if ( stopAtType != null )
			{
				// AS 5/7/07 BR22294
				//clipToElement = Utilities.GetAncestorFromType(this, typeof(ScrollContentPresenter), true, null, stopAtType) as ScrollContentPresenter;
				// SSP 5/23/08 BR33108
				// Made this method static and added rp parameter.
				// 
				//clipToElement = GetAutoFitScrollPresenter(this, stopAtType, out isScaled) as ScrollContentPresenter;
				clipToElement = GetAutoFitScrollPresenter( rp, stopAtType, out isScaled ) as ScrollContentPresenter;
			}
			else
			{
				// AS 5/7/07 BR22294
				isScaled = false;
			}

			if (clipToElement == null && stopAtType != typeof(RecordListControl))
				// SSP 5/23/08 BR33108
				// Made this method static and added rp parameter.
				// 
				//clipToElement = this;
				clipToElement = rp;

			if (clipToElement == null)
				return;

            // JJD 1/22/09 - NA 2009 vol 1
            // Since we should always be using a cellPanel get that now
			// MD 8/19/10
			// If the associated cell panel is cached, use it instead of searching for it.
            //cellPanel = Utilities.GetDescendantFromType(rp, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
			DataRecordPresenter drp = rp as DataRecordPresenter;

			if (drp != null)
				cellPanel = drp.AssociatedVirtualizingDataRecordCellPanel;

			if (cellPanel == null)
				cellPanel = Utilities.GetDescendantFromType(rp, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;

            //Debug.Assert(cellPanel != null, "We should always have a VirtualizingDataRecordCellPanel");
            if (cellPanel == null)
            {
                // SSP 5/23/08 BR33108
                // Added support for SummaryRecordPresenter.
                // 
                //cellPanel = Utilities.GetDescendantFromType(this, typeof(DataRecordCellArea), true) as DataRecordCellArea;
                if (rp is SummaryRecordPresenter)
                    cellPanel = Utilities.GetDescendantFromType(rp, typeof(SummaryRecordCellArea), true) as SummaryRecordCellArea;
                else
                    cellPanel = Utilities.GetDescendantFromType(rp, typeof(DataRecordCellArea), true) as DataRecordCellArea;

                if (cellPanel == null)
                {
                    // SSP 5/23/08 BR33108
                    // Made this method static and added rp parameter.
                    // 
                    //contentArea = Utilities.GetDescendantFromType(this, typeof(HeaderLabelArea), true) as HeaderLabelArea;
                    cellPanel = Utilities.GetDescendantFromType(rp, typeof(HeaderLabelArea), true) as HeaderLabelArea;
                    return;
                }
            }

			// Stop here if the view does not support nested panels for hierarchical data display.
			if (currentView != null && currentView.IsNestedPanelsSupported == false)
				return;

            // JJD 2/18/09 - TFS13976
            // Since this is a nested panel situation we can get the top level panel
            // from the grid and walk up its ancestor chain to get its ScrollContentPresenter
            // and use that
            Panel panel = dp.CurrentPanel;
            if (panel != null)
            {
				clipToElement = Utilities.GetAncestorFromType(panel, typeof(ScrollContentPresenter), true, null, stopAtType) as ScrollContentPresenter;

                if (clipToElement != null)
                    return;
            }

			// SSP 5/23/08 BR33108
			// Made this method static and added rp parameter. This method can apply to summary record as well.
			// 
			//DataRecordPresenter drp = this;
			// MD 8/19/10
			// We don't need to store the record in a separate variable. We can just update the rp variable.
			//RecordPresenter drp = rp;

			// walk up until we get the scrollconentpresenter for the top level DataRecord
			// SSP 5/23/08 BR33108
			// Made this method static and added rp parameter. This method can apply to summary record as well.
			// 
			//while (clipToElement != null && drp.DataRecord.ParentDataRecord != null)
			while ( clipToElement != null && null != rp.Record && rp.Record.ParentDataRecord != null )
			{
				// get the DataRecordPresenter for the parent DataRecord
				// MD 8/19/10
				// We don't need to store the record in a separate variable. We can just update the rp variable.
				//drp = Utilities.GetAncestorFromType(clipToElement, typeof(DataRecordPresenter), true, dp) as DataRecordPresenter;
				rp = Utilities.GetAncestorFromType(clipToElement, typeof(DataRecordPresenter), true, dp) as RecordPresenter;

				//Debug.Assert(drp != null);

				// get the scroll content presenter for the parent data record
				// MD 8/19/10
				// We don't need to store the record in a separate variable. We can just update the rp variable.
				//if (drp != null)
				//    clipToElement = Utilities.GetAncestorFromType(drp, typeof(ScrollContentPresenter), true, null, stopAtType) as ScrollContentPresenter;
				if (rp != null)
					clipToElement = Utilities.GetAncestorFromType(rp, typeof(ScrollContentPresenter), true, null, stopAtType) as ScrollContentPresenter;
				else
					return;
			}

		}

		// AS 5/7/07 BR22294
		// AS 5/8/07
		// We weren't returning the stop at element  if we didn't find a scroll content presenter.
		//
		// SSP 5/23/08 BR33108
		// Need to call this from SummaryRecordPresenter as well. Made GetAutoFitScrollPresenter static.
		// 
		private static DependencyObject GetAutoFitScrollPresenter(FrameworkElement descendant, Type stopAtType, out bool isScaled)
		{
			isScaled = false;
			DependencyObject ancestor = descendant;

			do
			{
				// start with the ancestor
				ancestor = VisualTreeHelper.GetParent(ancestor);

				if (ancestor is Viewbox && ((Viewbox)ancestor).Stretch != Stretch.None)
					isScaled = true;
				else if (ancestor is ScrollContentPresenter)
					return (ScrollContentPresenter)ancestor;

			} while (ancestor != null && stopAtType.IsAssignableFrom(ancestor.GetType()) == false);

			return ancestor;
		}

				#endregion //GetCellAreaAndScrollContentPresenter

				#region GetChildCellValuePresenterList

		private List<CellValuePresenter> GetChildCellValuePresenterList()
		{
			if (this.FieldLayout == null)
				return new List<CellValuePresenter>();

			List<CellValuePresenter> list = new List<CellValuePresenter>(this.FieldLayout.Fields.Count);

			DataRecordCellArea cellArea = Utilities.GetDescendantFromType(this, typeof(DataRecordCellArea), false) as DataRecordCellArea;

			if (cellArea == null)
				return list;

			UIElement cellAreaContainer = cellArea.Content as UIElement;

			if (cellAreaContainer is Grid == false && cellAreaContainer is VirtualizingDataRecordCellPanel == false)
				return list;

			int count = VisualTreeHelper.GetChildrenCount(cellAreaContainer);

			for (int i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(cellAreaContainer, i);

				if (child is CellValuePresenter)
					list.Add(child as CellValuePresenter);
				else
				{
					CellValuePresenter fc = Utilities.GetDescendantFromType(child, typeof(CellValuePresenter), true) as CellValuePresenter;

					if (fc != null)
						list.Add(fc);
				}
			}

			return list;
		}

				#endregion //GetChildCellValuePresenterList	

				#region GetChildCellPresenterList

		private List<CellPresenter> GetChildCellPresenterList()
		{
			if (this.FieldLayout == null)
				return new List<CellPresenter>();

			List<CellPresenter> list = new List<CellPresenter>(this.FieldLayout.Fields.Count);

			DataRecordCellArea cellArea = Utilities.GetDescendantFromType(this, typeof(DataRecordCellArea), false) as DataRecordCellArea;

			if (cellArea == null)
				return list;

			UIElement cellAreaContainer = cellArea.Content as UIElement;

			if (cellAreaContainer is Grid == false && cellAreaContainer is VirtualizingDataRecordCellPanel == false)
				return list;

			int count = VisualTreeHelper.GetChildrenCount(cellAreaContainer);

			for (int i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(cellAreaContainer, i);

				if (child is CellPresenter)
					list.Add(child as CellPresenter);
			}

			return list;
		}

				#endregion //GetChildCellPresenterList	

				// JJD 12/7/11 - TFS97186 - Refactored - added
				#region RefreshAutoFitExtent

		private void RefreshAutoFitExtent(bool calledFromPrepare)
		{
			_cachedAutoFitExtent = null;

			DataRecord dr = this.Record as DataRecord;

			// JJD 10/21/11 - TFS86028 - Optimization 
			// for data records that will be auto fit see is a previous
			// sibling record already calculated these. If so cache the calculated
			// cell area width and/or height to be used by this records until
			// it completely laid out. This optimization assumes that all
			// sibling records at the same nesting depth will calculate the same
			// value. In that case, the optimization will avoid another layout pass.
			if (dr != null && dr.RecordType == RecordType.DataRecord)
			{
				FieldLayout fl = dr.FieldLayout;

				if (fl != null && fl.IsAutoFit)
				{
					double? cachedWidth = null;
					double? cachedHeight = null;

					if (fl.IsAutoFitWidth)
					{
						cachedWidth = fl.TemplateDataRecordCache.GetCachedAutoFitWidth(dr);

						if (cachedWidth.HasValue)
							this.SetValue(AutoFitCellAreaWidthPropertyKey, cachedWidth.Value);
					}

					if (fl.IsAutoFitHeight)
					{
						cachedHeight = fl.TemplateDataRecordCache.GetCachedAutoFitHeight(dr);

						if (cachedHeight.HasValue)
							this.SetValue(AutoFitCellAreaHeightPropertyKey, cachedHeight.Value);
					}

					if (cachedWidth.HasValue || cachedHeight.HasValue)
					{
						_cachedAutoFitExtent = new CachedAutoFitExtent();
						_cachedAutoFitExtent._width = cachedWidth;
						_cachedAutoFitExtent._height = cachedHeight;

						// JJD 12/7/11 - TFS97186 
						// If we aren't called from the prepare then we need to
						// invalidate the measure of our descendant VirtualizingDataRecordCellPanel
						// and all of its ancestors up this element since this method was called
						// as the result of an existing DRP being un-collapsed by
						// the recycling infrastructure and we need to make sure any cached measure
						// sizes are ignored.
						if (calledFromPrepare == false)
						{
							var vdrcp = CoreUtilities.GetWeakReferenceTargetSafe(associatedVirtualizingDataRecordCellPanel) as UIElement;

							if (vdrcp != null)
							{
								Utilities.InvalidateMeasure((UIElement)vdrcp, this);
								vdrcp.InvalidateMeasure();
							}
						}
					}

				}
			}
		}

				#endregion //RefreshAutoFitExtent	
    
				#region UpdateAutoFitPropertiesForCellArea

		internal override void UpdateAutoFitProperties()
		{
			// JJD 3/27/07
			// Get out if we aren't arranged in view
			if (!this.IsArrangedInView)
			{
				// SSP 5/23/08 BR33108
				// Use the new ClearAutoFitCellAreaProperties method instead.
				// 
				//this.ClearValue(AutoFitCellAreaHeightPropertyKey);
				//this.ClearValue(AutoFitCellAreaWidthPropertyKey);
				ClearAutoFitCellAreaProperties( this );

				return;
			}

			// JJD 3/27/07
			// call the base implementation
			base.UpdateAutoFitProperties();

			// SSP 5/23/08 BR33108
			// Moved the logic into the new UpdateAutoFitPropertiesHelper static method.
			// 
			// --------------------------------------------------------------------------
			UpdateAutoFitPropertiesHelper( this );
			
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------
		}

		// SSP 5/23/08 BR33108
		// 
		internal static void ClearAutoFitCellAreaProperties( RecordPresenter rp )
		{
			rp.ClearValue( AutoFitCellAreaWidthPropertyKey );
			rp.ClearValue( AutoFitCellAreaHeightPropertyKey );
		}

		// SSP 5/23/08 BR33108
		// Added static UpdateAutoFitPropertiesHelper so summary rp can call this. Logic in 
		// the method is moved from the existing UpdateAutoFitProperties method.
		// 
		internal static void UpdateAutoFitPropertiesHelper( RecordPresenter rp )
		{
			if ( rp.IsHeaderRecord )
			{
				// JJD 3/27/07
				// Get out if we don't have content
				if ( rp.HasHeaderContent == false )
					return;
			}
			else
			{
				// JJD 3/27/07
				// Get out if we aren't displaying thce cell area
				if ( rp.ShouldDisplayRecordContent == false )
					return;
			}

            // JJD 3/27/07
            // Get out if we are collapsed
            // JJD 2/3/09
            // Even if the element is collapsed let this process. Otherwise when we are in a recycling record presenters
            // our Visibility can be set toe collapsed during a reccyling deactivation of the container element. This
            // ca sometimes cause a situation when the horizontal scrollbar is shown and hidden endlessly because the
            // record preesnter at the cusp never calculates it auotsize property because its Visibility is set to Collapsed.
            //if ( rp.Visibility == Visibility.Collapsed )
            //    return;

			// AS 6/22/09 NA 2009.2 Field Sizing
			//DataPresenterBase dp = rp.DataPresenter;
			FieldLayout fl = rp.FieldLayout;

			if (fl == null)
				return;

			// JJD 10/21/11 - TFS86028 - Optimization 
			// Clear the cached values
			DataRecordPresenter drp = rp as DataRecordPresenter;
			if (drp != null && drp._cachedAutoFitExtent != null)
				drp._cachedAutoFitExtent = null;

			if (fl.IsAutoFitWidth)
			{
				double width =  CalculateCellAreaAutoFitExtent(rp, true);
				
				if (drp != null )
					fl.TemplateDataRecordCache.SetCachedAutoFitWidth(rp.Record, width);

				// rp.SetValue( AutoFitCellAreaWidthPropertyKey, CalculateCellAreaAutoFitExtent( rp, true ) );
				rp.SetValue(AutoFitCellAreaWidthPropertyKey,width);
			}
			else
				rp.ClearValue(AutoFitCellAreaWidthPropertyKey);

			if (fl.IsAutoFitHeight)
			{
				double height =  CalculateCellAreaAutoFitExtent(rp, false);
				
				if (drp != null )
					fl.TemplateDataRecordCache.SetCachedAutoFitHeight(rp.Record, height);

				// rp.SetValue( AutoFitCellAreaWidthPropertyKey, CalculateCellAreaAutoFitExtent( rp, false ) );
				rp.SetValue(AutoFitCellAreaHeightPropertyKey, height);
			}
			else
				rp.ClearValue(AutoFitCellAreaHeightPropertyKey);

		}

				#endregion //UpdateAutoFitPropertiesForCellArea
    
			#endregion //Private Methods	
    
		#endregion //Methods

			// JJD 10/21/11 - TFS86028 - Optimization
			#region CachedAutoFitExtent private class
    
    		private class CachedAutoFitExtent 
			{
				internal double? _height;
				internal double? _width;
			}

   			#endregion //CachedAutoFitExtent private class	
    	}

	#endregion //DataRecordPresenter

	#region ExpandableFieldRecordPresenter

	/// <summary>
	/// An element that represents an <see cref="ExpandableFieldRecord"/> in the UI of a XamDataGrid, XamDataCarousel or XamDataPresenter.
	/// </summary>
	/// <remarks>
	/// <para class="body"><see cref="ExpandableFieldRecord"/>s are child records of <see cref="DataRecord"/>s. 
	/// The <see cref="RecordPresenter.NestedContent"/> of these elements contain either a <see cref="RecordListControl"/> with the <see cref="ExpandableFieldRecord"/>'s <see cref="Infragistics.Windows.DataPresenter.Record.ViewableChildRecords"/>, if the associated <see cref="Field"/>'s <see cref="Field.DataType"/> implement's the <see cref="System.Collections.IEnumerable"/> interface, or an <see cref="ExpandedCellPresenter"/> containing the actual value of the <see cref="Cell"/>.</para>
	/// <para></para>
	/// <para class="body">Refer to the remarks for the <see cref="RecordPresenter"/> base class.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="ExpandableFieldRecord"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataPresenter"/>
	//[Description("A control that displays the contents of a 'ExpandableFieldRecord' in the 'DataPresenterBase' derived controls such as 'XamDataGrid'. ExpandableFieldRecords are used to group records in child bands.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ExpandableFieldRecordPresenter : RecordPresenter
	{
		#region Constructors

		static ExpandableFieldRecordPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpandableFieldRecordPresenter), new FrameworkPropertyMetadata(typeof(ExpandableFieldRecordPresenter)));

			// JJD 1/25/07 - BR18963
			// Default to not stretching
			RecordPresenter.HorizontalAlignmentProperty.OverrideMetadata(typeof(ExpandableFieldRecordPresenter), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
			RecordPresenter.VerticalAlignmentProperty.OverrideMetadata(typeof(ExpandableFieldRecordPresenter), new FrameworkPropertyMetadata(VerticalAlignment.Top));
		}

		#endregion //Constructors	
        
		#region Base class overrides

			#region GetDefaultTemplateProperty

		private static bool s_DefaultTemplatesCached = false;
		private static ControlTemplate s_DefaultTemplate = null;
		private static ControlTemplate s_DefaultTemplateTabluar = null;
		private static ControlTemplate s_DefaultTemplateCardView = null;

		internal override ControlTemplate GetDefaultTemplateProperty(DependencyProperty templateProperty)
		{
			if (s_DefaultTemplatesCached == false)
			{
				lock (typeof(ExpandableFieldRecordPresenter))
				{
					if (s_DefaultTemplatesCached == false)
					{
						s_DefaultTemplatesCached = true;

						Style style = Infragistics.Windows.Themes.DataPresenterGeneric.ExpandableFieldRecordPresenter;
						Debug.Assert(style != null);
						if (style != null)
						{
							s_DefaultTemplate = Utilities.GetPropertyValueFromStyle(style, TemplateProperty, true, false) as ControlTemplate;
							s_DefaultTemplateTabluar = Utilities.GetPropertyValueFromStyle(style, TemplateGridViewProperty, true, false) as ControlTemplate;
							s_DefaultTemplateCardView = Utilities.GetPropertyValueFromStyle(style, TemplateCardViewProperty, true, false) as ControlTemplate;
						}
					}
				}
			}

			if (templateProperty == TemplateProperty)
				return s_DefaultTemplate;

			if (templateProperty == TemplateGridViewProperty)
				return s_DefaultTemplateTabluar;

			if (templateProperty == TemplateCardViewProperty)
				return s_DefaultTemplateCardView;

			Debug.Fail("What are WeakEventManager doing here");
			return null;
		}

			#endregion //GetDefaultTemplateProperty

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// if this record presenter is being cleared then ignore any changes
			if (this.IsClearingBindings)
				return;

			if (e.Property == IsExpandedProperty)
				this.SetHeaderVisibility();
		}

			#endregion //OnPropertyChanged	

			#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property of the associated record has changed
		/// </summary>
		/// <param name="e">The event arguments that contain the name of the property that was changed</param>
		protected override void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnRecordPropertyChanged(e);

			switch (e.PropertyName)
			{
				case "IsExpanded":
					{
						// JJD 2/7/11 - TFS35853
						// If the expanded state is changing on a GroupByFieldLayout's record whose
						// scroll count is 1 then we need to invalidate the measure of the containing GridViewPanel.
						// The reason for this is to show/hide the attached header rcd for the filtered out
						// descendant data rcds
						ExpandableFieldRecord efr = this.Record as ExpandableFieldRecord;

						if (efr != null && efr.HasVisibleChildren == false)
							this.InvalidateGridViewPanelFlat();
					}
					break;
			}
		}

		#endregion //OnRecordPropertyChanged

			#region PrepareContainerForItem

		internal override void PrepareContainerForItem(object item, bool isUsedForHeaderOnly)
		{
			base.PrepareContainerForItem(item, isUsedForHeaderOnly);

			// JJD 4/14/07
			// If a null item gets passed in then just retirn.
			// Note: This can happen if the generate caused a lazy creation of the Record and
			// its Visibility was set to Collapsed in the InitializeRecord event
			if (item == null)
				return;

			ExpandableFieldRecord rcd = this.Record as ExpandableFieldRecord;

			if (rcd != null)
			{
				// Bind the ExpansionIndicatorVisibility 
				Binding binding = new Binding();
				binding.Path = new PropertyPath("ExpansionIndicatorVisibility");
				binding.Mode = BindingMode.OneWay;
				binding.Source = rcd;
				this.SetBinding(ExpansionIndicatorVisibilityProperty, binding);

				this.SetHeaderVisibility();
			}
		}

			#endregion //PrepareContainerForItem	
    
		#endregion //Base class overrides

		#region Properties

			#region HeaderVisibility

		private static readonly DependencyPropertyKey HeaderVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("HeaderVisibility",
			typeof(Visibility), typeof(ExpandableFieldRecordPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty =
			HeaderVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Determines if the visibility of the header (read-only).
		/// </summary>
		/// <seealso cref="HeaderVisibilityProperty"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility HeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ExpandableFieldRecordPresenter.HeaderVisibilityProperty);
			}
		}

			#endregion //HeaderVisibility

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region SetHeaderVisibility

		private void SetHeaderVisibility()
		{
			ExpandableFieldRecord rcd = this.Record as ExpandableFieldRecord;

			if (rcd != null)
			{
				// SSP 8/10/09 - NAS9.2 Enhanced grid-view
				// Moved the existing logic from here into the IsHeaderVisible property on the
				// ExpandableFieldRecord.
				// 
				bool displayHeader = rcd.IsHeaderVisible;

				this.SetValue(HeaderVisibilityPropertyKey, displayHeader ? KnownBoxes.VisibilityVisibleBox : KnownBoxes.VisibilityCollapsedBox);
			}
		}

				#endregion //SetHeaderVisibility

			#endregion //Private Methods	

		#endregion //Methods
	}

	#endregion //ExpandableFieldRecordPresenter

	#region GroupByRecordPresenter

	/// <summary>
	/// An element that represents a GroupByRecord in the UI of a XamDataGrid, XamDataCarousel or XamDataPresenter.
	/// </summary>
	/// <remarks>
	/// <para class="body"><see cref="GroupByRecord"/>s are used as parent records for a set of logically related <see cref="DataRecord"/>s (or nested <see cref="GroupByRecord"/>s) created by a Grouping operation.</para>
	/// <para></para>
	/// <para class="body">Refer to the remarks for the <see cref="RecordPresenter"/> base class.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="GroupByRecord"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataGrid"/>
	/// <seealso cref="XamDataPresenter"/>
	//[Description("A control that displays the contents of a 'GroupByRecord' in the 'DataPresenterBase' derived controls such as 'XamDataGrid'. GroupByRecords are used as parent nodes for a set of logically related records created by a Grouping operation.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GroupByRecordPresenter : RecordPresenter
	{
		#region Private Members

		// JJD 4/29/11 - TFS74075
		private PropertyValueTracker _styleTracker;

		#endregion //Private Members	
    
		#region Constructor

		static GroupByRecordPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupByRecordPresenter), new FrameworkPropertyMetadata(typeof(GroupByRecordPresenter)));
		}

		#endregion // Constructor

		#region Base Overrides

		#region GetDefaultTemplateProperty

		private static bool s_DefaultTemplatesCached = false;
		private static ControlTemplate s_DefaultTemplate = null;
		private static ControlTemplate s_DefaultTemplateTabluar = null;
		private static ControlTemplate s_DefaultTemplateCardView = null;

		internal override ControlTemplate GetDefaultTemplateProperty(DependencyProperty templateProperty)
		{
			if (s_DefaultTemplatesCached == false)
			{
				lock (typeof(GroupByRecordPresenter))
				{
					if (s_DefaultTemplatesCached == false)
					{
						s_DefaultTemplatesCached = true;

						Style style = Infragistics.Windows.Themes.DataPresenterGeneric.GroupByRecordPresenter;
						Debug.Assert(style != null);
						if (style != null)
						{
							s_DefaultTemplate = Utilities.GetPropertyValueFromStyle(style, TemplateProperty, true, false) as ControlTemplate;
							s_DefaultTemplateTabluar = Utilities.GetPropertyValueFromStyle(style, TemplateGridViewProperty, true, false) as ControlTemplate;
							s_DefaultTemplateCardView = Utilities.GetPropertyValueFromStyle(style, TemplateCardViewProperty, true, false) as ControlTemplate;
						}
					}
				}
			}

			if (templateProperty == TemplateProperty)
				return s_DefaultTemplate;

			if (templateProperty == TemplateGridViewProperty)
				return s_DefaultTemplateTabluar;

			if (templateProperty == TemplateCardViewProperty)
				return s_DefaultTemplateCardView;

			Debug.Fail("What are WeakEventManager doing here");
			return null;
		}

		#endregion //GetDefaultTemplateProperty

		#region GetRecordDescription

		// SSP 6/2/08 - Summaries Functionality
		// Added DescriptionWithSummaries property on the GroupByRecord.
		// 
		internal override string GetRecordDescription( )
		{
			GroupByRecord gr = this.Record as GroupByRecord;
			return null != gr ? gr.DescriptionWithSummaries : null;
		}

		#endregion // GetRecordDescription

		#region OnApplyTemplate
		/// <summary>
		/// Called after the template has been applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			// AS 10/9/09 TFS22990
			if (this.Tag == RecordPresenter.TemplateRecordPresenterId)
			{
				FieldLayout fl = this.FieldLayout;

				if (null != fl)
					fl.TemplateDataRecordCache.OnTemplateRecordTemplateChanged(this);
			}

			base.OnApplyTemplate();
		} 
		#endregion //OnApplyTemplate

		#region OnRecordChanged

		/// <summary>
		/// Overridden. Called when Record property's value has changed.
		/// </summary>
		/// <param name="oldRecord">Old record if any.</param>
		/// <param name="newRecord">New record if any.</param>
		protected override void OnRecordChanged( Record oldRecord, Record newRecord )
		{
			base.OnRecordChanged( oldRecord, newRecord );

			this.UpdateShouldDisplaySummaries( );

			// JJD 4/29/11 - TFS74075
			// Listen for changes to the GroupByRecordPresenterStyle settings
			GroupByRecord grp = newRecord as GroupByRecord;
			Field fld = grp != null ? grp.GroupByField : null;
			if (fld == null)
				_styleTracker = null;
			else
				_styleTracker = new PropertyValueTracker(fld, "GroupByRecordPresenterStyleResolved", new PropertyValueTracker.PropertyValueChangedHandler(this.OnStyleSettingChanged), true);
		}
		#endregion // OnRecordChanged

		#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property of the associated record has changed
		/// </summary>
		/// <param name="e">The event arguments that contain the name of the property that was changed</param>
		protected override void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnRecordPropertyChanged(e);

			switch (e.PropertyName)
			{
				case "IsExpanded":
					{
						// JJD 2/7/11 - TFS35853
						// If the expanded state is changing on a GroupByFieldLayout's record whose
						// scroll count is 1 then we need to invalidate the measure of the containing GridViewPanel.
						// The reason for this is to show/hide the attached header rcd for the filtered out
						// descendant data rcds
						GroupByRecord gbr = this.Record as GroupByRecord;

						if ( gbr != null && gbr.RecordType == RecordType.GroupByFieldLayout &&
							 gbr.ScrollCountInternal == 1 )
							this.InvalidateGridViewPanelFlat();
					}
					break;
			}
		}

		#endregion //OnRecordPropertyChanged

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region ShouldDisplaySummaryCells

		private static readonly DependencyPropertyKey ShouldDisplaySummaryCellsPropertyKey = DependencyProperty.RegisterReadOnly(
				"ShouldDisplaySummaryCells",
				typeof( bool ),
				typeof( GroupByRecordPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="ShouldDisplaySummaryCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldDisplaySummaryCellsProperty = ShouldDisplaySummaryCellsPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether to display summary cells inside the group-by record.
		/// </summary>
		//[Description( "Indicates whether to display summary results inside the group-by record." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool ShouldDisplaySummaryCells
		{
			get
			{
				return (bool)this.GetValue( ShouldDisplaySummaryCellsProperty );
			}
		}

		#endregion // ShouldDisplaySummaryCells

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		// JJD 4/29/11 - TFS74075 - added
		#region OnStyleSettingChanged

		private void OnStyleSettingChanged()
		{
			this.InvalidateStyle();
		}

		#endregion //OnStyleSettingChanged	
    
		#region UpdateShouldDisplaySummaries

		internal void UpdateShouldDisplaySummaries( )
		{
			GroupByRecord groupByRecord = this.Record as GroupByRecord;
			// AS 6/18/09 NA 2009.2 Field Sizing
			// Moved to a helper method.
			//
			//FieldLayout fl = null != groupByRecord ? groupByRecord.FieldLayout : null;
			//GroupBySummaryDisplayMode groupBySummaryDisplayMode = null != fl
			//    ? fl.GroupBySummaryDisplayModeResolved : GroupBySummaryDisplayMode.Default;
			//
			//bool shouldDisplaySummaries = //GroupBySummaryDisplayMode.SummaryCells == groupBySummaryDisplayMode || 
			//    GroupBySummaryDisplayMode.SummaryCellsAlwaysBelowDescription == groupBySummaryDisplayMode;
			//
			//if (shouldDisplaySummaries)
			//    shouldDisplaySummaries = null != groupByRecord && groupByRecord.HasSummaryResults();
			bool shouldDisplaySummaries = GroupByRecord.ShouldDisplaySummaries(groupByRecord);

			this.SetValue( ShouldDisplaySummaryCellsPropertyKey, shouldDisplaySummaries );
		}

		#endregion // UpdateShouldDisplaySummaries

		#endregion // Private/Internal Methods

		#endregion // Methods

	}

	#endregion //GroupByRecordPresenter
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