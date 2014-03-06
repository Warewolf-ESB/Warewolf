using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.DataPresenter.Internal;
using System.Windows.Data;
using Infragistics.Windows.Reporting;
using System.Windows.Input;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// This is an ItemsControl that is created internally by a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> to hold a list of sibling <see cref="Record"/>s
	/// </summary>
	// JJD 5/22/07 - Optimization
	// RecordListControl now derives from RecyclingItemsControl 
	//public sealed class RecordListControl : ItemsControl
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public sealed class RecordListControl : RecyclingItemsControl

		, ICarouselPanelSelectionHost

	{
		#region Member Variables

		private DataPresenterBase					_dataPresenter;
		private Panel								_panel;

		// JJD 11/21/11 - TFS69163
		// Added logic to temporarily coerce the ItemsSource property to null for performance reasons
		// during mass expand/collapse operations
		private int									_coerceItemsSourceToNullCount;

		#endregion //Member Variables

		#region Constructor

		static RecordListControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordListControl), new FrameworkPropertyMetadata(typeof(RecordListControl)));
			
			// JJD 11/21/11 - TFS69163
			// Override ItemsSource to add a Coerce callback
			ItemsControl.ItemsSourceProperty.OverrideMetadata(typeof(RecordListControl), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceItemsSource)));

			// AS 8/19/09 TFS17864
			// This is basically the same situation that we had for the DataRecordCellArea. In this case the 
			// element can get focus if focus was within a cell and gets shifted up to an ancestor.
			//
			GridUtilities.SuppressBringIntoView(typeof(RecordListControl));
		}

		// JJD 2/22/12 - TFS101199 - added IsRoot parameter
		//internal RecordListControl(DataPresenterBase dataPresenter)
		internal RecordListControl(DataPresenterBase dataPresenter, bool isRoot)
		{
			if (dataPresenter == null)
				throw new ArgumentNullException( "dataPresenter", DataPresenterBase.GetString( "LE_ArgumentNullException_5" ) );

			this._dataPresenter = dataPresenter;
			
			// JJD 2/22/12 - TFS101199 - added IsRoot parameter
			if (isRoot)
			{
				this.SetValue(IsRootPropertyKey, KnownBoxes.TrueBox);
			}


			// JJD 5/25/07 - Optimization
			// Bind the ItemContainerGenerationMode property to the dp's RecordContainerGenerationMode
			this.SetBinding(ItemContainerGenerationModeProperty, Utilities.CreateBindingObject(DataPresenterBase.RecordContainerGenerationModeProperty, BindingMode.OneWay, this._dataPresenter));
		}

		#endregion //Constructor

		#region Base Class Overrides
        
            // JJD 05/07/10 - TFS31643 - added
            #region IsStillValid

        /// <summary>
        /// Deterimines whether the container and item are still valid
        /// </summary>
        /// <param name="container">The container associated with the item.</param>
        /// <param name="item">Its associated item.</param>
        /// <returns>True if still valid. The default is null.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal protected override bool? IsStillValid(DependencyObject container, object item) 
        {
            Record rcd = item as Record;

            Debug.Assert(rcd != null, "item is not a Record in RecordListControl.IsStillValid");

            FlatScrollRecordsCollection flatColl = this.ItemsSource as FlatScrollRecordsCollection;

            if (flatColl != null)
                return flatColl.IndexOf(rcd) >= 0;

            return rcd != null && rcd.OverallScrollPosition >= 0; 
        }

            #endregion //IsStillValid

			// JJD 2/28/11 - TFS66934 - Optimization - added
			#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// JJD 2/28/11 - TFS66934 - Optimization 
			// Having a RoutedCommand associated with a command source like a button means that the 
			// CanExecute will be invoked every time the mouse or key is pressed down. That results 
			// in 2 routed events (previewcanexecute and canexecute). This has a serious impact on 
			// scrolling via the arrow keys and typing in general.
			//
			// The logic below will look for buttons inside the scrollbars and replace them
			// with RoutedCommandProxy objects (which implement ICommand but are not RoutedCommands
			// and always return true from CanExecute).
			// This eliminates the overhead of the ...CanEcecute routed events being generated on every key down. 
			//
			ScrollViewer sv = Utilities.GetTemplateChild<ScrollViewer>(this, null);

			if (sv != null)
			{
				// Make sure the ScrollViewer's template is applied
				sv.ApplyTemplate();

				List<ScrollBar> scrollbars = new List<ScrollBar>();
				Utilities.DependencyObjectSearchCallback<ScrollBar> scrollbarCallback = delegate(ScrollBar scrollbar)
				{
					scrollbars.Add(scrollbar);
					return false;
				};

				// Find all scrollbars
				Utilities.GetTemplateChild<ScrollBar>(sv, scrollbarCallback);

				foreach (ScrollBar sb in scrollbars)
				{
					// Make sure the Scrollbar's template is applied
					sb.ApplyTemplate();

					List<ButtonBase> buttons = new List<ButtonBase>();
					Utilities.DependencyObjectSearchCallback<ButtonBase> buttonCallback = delegate(ButtonBase button)
					{
						buttons.Add(button);
						return false;
					};

					// Find all buttons within the scrollbar
					Utilities.GetTemplateChild<ButtonBase>(sb, buttonCallback);

					// For each button call RoutedCommandProxy.WireProxy which will check if the Command
					// property is set to a routed command. If so it will replace it with a proxy that
					// doesn't incur the overhead of a routed command by assuming the the CanExecute
					// always returns true (which it does for all the commands in a scrollbar
					foreach (ButtonBase button in buttons)
						RoutedCommandProxy.WireProxy(button);

				}
			}
			
		}

			#endregion //OnApplyTemplate	
    
			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="RecordListControl"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.RecordListControlAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - return a different peer if we are in flat view
			//return new Infragistics.Windows.Automation.Peers.DataPresenter.RecordListControlAutomationPeer(this);
			if (this.DataPresenter != null && this.DataPresenter.IsFlatView)
				return new ViewableRecordCollectionAutomationPeer(this, this.DataPresenter.ViewableRecords, true);
			else
				return new Infragistics.Windows.Automation.Peers.DataPresenter.RecordListControlAutomationPeer(this);
		} 
			#endregion //OnCreateAutomationPeer

			#region OnItemsChanged
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		// JM 12-21-09 TFS22151
		/// <summary>
		/// Overridden. Invoked when the contents of the items collection has changed.
		/// </summary>
		/// <param name="e">Event arguments indicating the change that occurred.</param>
		protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(this);
			if (automationPeer != null)
			{
				if (this.DataPresenter != null)
				{
					if (this.DataPresenter.IsFlatView == true	&& automationPeer is ViewableRecordCollectionAutomationPeer ||
						this.DataPresenter.IsFlatView == false	&& automationPeer is RecordListControlAutomationPeer)
					{
						if (automationPeer is ViewableRecordCollectionAutomationPeer)
							((ViewableRecordCollectionAutomationPeer)automationPeer).InitializeViewableRecordsCollection(this.DataPresenter.ViewableRecords);

						// AS 4/13/11 TFS72669
						// I would think we would want to raise this after invalidating.
						//
						//automationPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
						automationPeer.InvalidatePeer();

						// AS 4/13/11 TFS72669
						AutomationPeerHelper.InvalidateChildren(automationPeer);

						automationPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
					}
				}
			}
		}
			#endregion //OnItemsChanged

		// AS 6/5/07
			#region OnItemsPanelChanged
		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			#endregion //OnItemsPanelChanged

			#region OnItemsSourceChanged
		/// <summary>
		/// Overridden. Invoked when the <b>ItemsSource</b> of the record list has changed.
		/// </summary>
		/// <param name="oldValue">Old items source</param>
		/// <param name="newValue">New items source</param>
		protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);

			ViewableRecordCollection oldVisibleRecords = oldValue as ViewableRecordCollection;
			RecordCollectionBase oldRecords = oldValue as RecordCollectionBase;

			if (oldVisibleRecords != null && oldVisibleRecords.LastRecordList == this)
				oldVisibleRecords.LastRecordList = null;
			else if (oldRecords != null && oldRecords.LastRecordList == this)
				oldRecords.LastRecordList = null;
			
			ViewableRecordCollection newVisibleRecords = newValue as ViewableRecordCollection;
			RecordCollectionBase newRecords = newValue as RecordCollectionBase;

			if (newVisibleRecords != null)
				newVisibleRecords.LastRecordList = this;
			else if (newRecords != null)
				newRecords.LastRecordList = this;
		} 
			#endregion //OnItemsSourceChanged
    
			#region Properties

				#region HandlesScrolling

		/// <summary>
		/// Returns true since the RecordListControls handles scrolling
		/// </summary>
		protected override bool HandlesScrolling
		{
			get	{ return true; }
		}

				#endregion //HandlesScrolling

				// MD 3/16/11 - TFS24163
				#region MaxDeactivatedContainersWithoutIndexes

		/// <summary>
		/// Gets the maximum number of deeactivated containers without indexes which can remain in the control.
		/// </summary>
		protected internal override int MaxDeactivatedContainersWithoutIndexes
		{
			get
			{
				if (this.ItemsSource is FlatScrollRecordsCollection)
					return 150;

				return base.MaxDeactivatedContainersWithoutIndexes;
			}
		} 

				#endregion // MaxDeactivatedContainersWithoutIndexes

                // JJD 05/07/10 - TFS31643 - added
				#region UnderlyingItemsCount

		/// <summary>
		/// Returns the number of items in the underlying source. 
		/// </summary>
		/// <remarks>
        /// <para class="note"><b>Note:</b> derived classes that append additional items (e.g. header records) can override this property to return the number of items not counting the appended ones.</para>
		/// </remarks>
        /// <value>The default implementation returns the count of the Items collection.</value>
		internal protected override int UnderlyingItemsCount 
        { 
            get 
            {
                FlatScrollRecordsCollection flatColl = this.ItemsSource as FlatScrollRecordsCollection;

                // JJD 05/07/10 - TFS31643 
                // Just return the count of records (not including appended headers)
                if (flatColl != null)
                    return flatColl.CountOfNonHeaderRecords;

                return this.Items.Count; 
            } 
        }

				#endregion //UnderlyingItemsCount

			#endregion //Properties

			#region Methods

				#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes any preparation done by the container to 'host' the item.
		/// </summary>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			//base.ClearContainerForItemOverride(element, item);

			// JJD 5/22/07 check for the IRecordPresenterContainer interface that is now implemented 
			// by the CarouselItem
			
			//RecordPresenter recordPresenter = null;

//            // JM 07-18-06 - Be on the lookout for CarouselItem elements too.
//            if (element is RecordPresenter)
//                recordPresenter = element as RecordPresenter;

//#if !EXPRESS
//            else if (element is CarouselItem)
//                recordPresenter = ((CarouselItem)element).Content as RecordPresenter;
//#endif
			RecordPresenter recordPresenter = GetRecordPresenterFromContainer(element);

            if (recordPresenter != null)
            {
                bool clearContainer = true;

                // JJD 11/11/09 - TFS24665 
                // If the record presenter contains the active cell and it is
                // in edit mode and if we aren't in recycle mode then we
                // should cache the record presenter for possible re-use and remove it from
                // the visual tree
                if (recordPresenter.HasCellInEditMode)
                {
                    if (this._dataPresenter != null &&
                        this._dataPresenter.RecordContainerGenerationMode != ItemContainerGenerationMode.Recycle)
                    {
                        RecordListItemContainer rlic = element as RecordListItemContainer;

                        if (rlic != null)
                        {
							// SSP 3/26/10 TFS26525
							// Since we are pulling the record presenter out of the element tree, it's
							// DataContext will get set to null. We need to maintain the data context
							// on the record presenter.
							// 
							recordPresenter.DataContext = rlic.DataContext;

                            // set the Child property to null which will remove it from the visual tree
                            rlic.Child = null;

                            // cache a ref to the rp on the dp for later use
                            this._dataPresenter.PendingActiveRecordPresenter = recordPresenter;

                            // wire the layoutUpdated event. If the rp isn't reused
                            // by then we can safely clear the cache
	
							// JJD 3/15/11 - TFS65143 - Optimization
							// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
							// and just wire LayoutUpdated on the DP
							//this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
                            this._dataPresenter.WireLayoutUpdated(new GridUtilities.MethodDelegate(this.OnLayoutUpdated));

                            // set the flag so we don't call ClearContainerForItem below
                            clearContainer = false;
                        }
                    }
                }
                
                if ( clearContainer )
                    recordPresenter.ClearContainerForItem(item);
            }
			
			base.ClearContainerForItemOverride(element, item);
		}

				#endregion //ClearContainerForItemOverride

				#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			
			
			
			
			
			
			return this.GetContainerForItemOverrideHelper( null );
			
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

			
		}

		
		
		
		
		
		
		
		
		
		
		
		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <param name="item">Item for which to create the container.</param>
		/// <returns>The newly created container</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride( object item )
		{
			return this.GetContainerForItemOverrideHelper( item );
		}

				#endregion //GetContainerForItemOverride

				// JJD 5/22/07 - Optimization
				// RecordListControl now derives from RecyclingItemsControl 
				#region IsContainerCompatibleWithItem

		/// <summary>
		/// Determines if a container can be reused for a specific item.
		/// </summary>
		/// <param name="container">The container to be reused.</param>
		/// <param name="item">The potential new item.</param>
		/// <returns>True if the container can be reused for the item</returns>
		/// <remarks>
		/// <para class="body">When looking for a suitable container for an item the generator will search its cache and call this method to see 
		/// if one of its cached containers is compatible with the item. If this method returns true then the container is assigned to the item and 
		/// the <see cref="ReuseContainerForNewItem"/> method is called.
		/// </para>
		/// <para class="note"><b>Note:</b> the default implementation always returns true.</para>
		/// </remarks>
		internal protected override bool IsContainerCompatibleWithItem(DependencyObject container, object item)
		{
			RecordPresenter rp = GetRecordPresenterFromContainer(container);
			Record rcd = item as Record;

			Debug.Assert(rp != null, "Unknown container in RecordListControl.IsContainerCompatibleWithItem");
			Debug.Assert(rcd != null, "Unknown item in RecordListControl.IsContainerCompatibleWithItem");

			if (rp == null ||
				rcd == null)
				return false;

			if (rp.IsActive)
				return false;

            // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
            bool isFlattenedList = this.IsFlattenedList;

            // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
            // only check expanded state with a non-flattened list (i.e. nestedpanels)
            if (isFlattenedList == false)
            {
                bool isExpanded = rp.IsExpanded;

                if (isExpanded && rp.HasNestedContent)
                    return false;

                if (isExpanded != rcd.IsExpanded)
                    return false;
            }

			// [JM/JD 06-15-07]
            Record rpRecord = rp.Record;
			if (rpRecord != null)
			{
                // JJD 12/22/08 
                // Make sure the RecordTypes match
                if (rpRecord.RecordType != rcd.RecordType)
                    return false;

                // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
                // only check for a parent record match with a non-flattened list (i.e. nestedpanels).
                if (isFlattenedList == false)
                {
                    if (rpRecord.ParentRecord != rcd.ParentRecord)
                        return false;
                }
			}


			// [JM BR25650 08-13-07]
			//DataRecordPresenter drp = container as DataRecordPresenter;
			DataRecordPresenter drp = rp as DataRecordPresenter;

			if (drp != null)
			{
				DataRecord dr = item as DataRecord;

				// AS 5/24/07
				// This is a valid state when transitioning from non-groupby to groupby or vice versa.
				//
				//Debug.Assert(dr != null);

				if (dr == null)
					return false;

				Record drpRecord = rpRecord;

				if (dr.IsSpecialRecord)
				{
                    // JJD 12/01/08 - TFS6743/BR35763
                    // Do an 'and' instead of an 'or', otherwise it could throw a null ref exception
                    //if (drpRecord != null ||
                    if (drpRecord != null &&
                        drpRecord.IsSpecialRecord == false)
						return false;
				}

				// JJD 11/11/11 - TFS91364
				// For special header record placeholders don't allow recycling for any other record
				HeaderRecord hr = rpRecord as HeaderRecord;
				if (hr != null &&
					hr._vrcPlaceholderRecord != null)
				{
					return hr._vrcPlaceholderRecord == dr;
				}

				return drp.FieldLayout == dr.FieldLayout; 
			}

			// [JM BR25650 08-13-07]
			//GroupByRecordPresenter grp = container as GroupByRecordPresenter;
			GroupByRecordPresenter grp = rp as GroupByRecordPresenter;

			if (grp != null)
			{
				GroupByRecord gr = item as GroupByRecord;

				// AS 5/24/07
				// This is a valid state when transitioning from non-groupby to groupby or vice versa.
				//
				//Debug.Assert(gr != null);

				if (gr == null)
					return false;

				return grp.FieldLayout == gr.FieldLayout;
			}

			// [JM BR25650 08-13-07]
			//ExpandableFieldRecordPresenter xrp = container as ExpandableFieldRecordPresenter;
			ExpandableFieldRecordPresenter xrp = rp as ExpandableFieldRecordPresenter;

			if (xrp != null)
			{
				ExpandableFieldRecord xr = item as ExpandableFieldRecord;

				//Debug.Assert(xr != null);

				if (xr == null)
					return false;

				return xrp.FieldLayout == xr.FieldLayout;
			}

			
			
			SummaryRecordPresenter summaryRP = rp as SummaryRecordPresenter;
			if ( null != summaryRP )
			{
				SummaryRecord summaryRecord = item as SummaryRecord;

				// JJD 04/09/12 - TFS108549 - Optimization
				// As long and the FieldLayout's match then return true so that this
				// SummaryRecordPresenter can be reused for a new SummaryRecord
				//return null != summaryRecord && summaryRP.SummaryRecord == summaryRecord;
				SummaryRecord rpSummaryRecord = summaryRP.SummaryRecord;
				return null != summaryRecord && rpSummaryRecord.FieldLayout == summaryRecord.FieldLayout
					// SSP 4/10/12 TFS108549 - Optimizations
					// We also need to check the context.
					// 
					&& rpSummaryRecord.SummaryDisplayAreaContext.Equals( summaryRecord.SummaryDisplayAreaContext )
					&& rpSummaryRecord.NestingDepth == summaryRecord.NestingDepth;
			}

			return true;

		}

				#endregion //IsContainerCompatibleWithItem	
    
				#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			// JJD 3/05/07
			// Make sure the item is not null
			if ( item == null )
				return false;

			Type containerType = this.DataPresenter.CurrentViewInternal.RecordPresenterContainerType;

			return (item is RecordPresenter || item.GetType() == containerType);
		}

				#endregion //IsItemItsOwnContainerOverride

				#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size baseMeasureSize = base.MeasureOverride(availableSize);

            
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

            this.FindPanel();

			return baseMeasureSize;
		}

				#endregion //MeasureOverride	

				#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
		{
			// SSP 4/23/08
			// Moved the call to base implementation at the end of the method. We should create the
			// element hierarchy as much as possible before calling the base implementation. Without this
			// the card-view tester application doesn't work.
			// 
			//base.PrepareContainerForItemOverride(element, item);

			RecordPresenter recordPresenter = null;

			// JM 10-27-09 NA 10.1 CardView
			bool recordPresenterPrepareContainerCalled = false;

			// If the container is a RecordPresenter, we're done.
			if ( element is RecordPresenter )
			{
				recordPresenter = element as RecordPresenter;
			}
			// SSP 3/27/08 - Summaries Functionality
			// Now the element could be a RecordListItemContainer. If the element is 
			// RecordListItemContainer then create the right record presenter based on the item.
			// Added the following else-if block.
			// 
			else if ( element is RecordListItemContainer )
			{
				RecordListItemContainer recordListItemContainer = element as RecordListItemContainer;
				recordPresenter = CreateRecordPresenterForItem( item );
				recordListItemContainer.InitializeWithItem( recordPresenter );
			}
			else
			{
				// SSP 4/22/08 - Summaries Feature
				// Took out RecordPresenter parameter from ViewBase's
				// GetContainerForRecordPresenter because in GetContainerForItemOverride we
				// don't have the item and thus don't know which record presenter to create,
				// and thus we cannot pass that along into this method. Instead we added
				// ViewBase's PrepareContainerForRecordPresenter method that will allow the
				// view to associated its wrapper to the record presenter. With this,
				// GetContainerForItemOverride simply creates the view container and here we
				// are supposed to associate it with a record presenter using ViewBase's
				// PrepareContainerForRecordPresenter method.
				// 
				// --------------------------------------------------------------------------
				recordPresenter = CreateRecordPresenterForItem( item );

				// JM 10-27-09 NA 10.1 CardView
				if (recordPresenter != null)
				{
					recordPresenter.PrepareContainerForItem(item);
					recordPresenterPrepareContainerCalled = true;
				}

				this.PrepareViewContainerForRecordPresenter(element, recordPresenter);
				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				// --------------------------------------------------------------------------
			}

			// If we got a RecordPresenter, call PrepareContainerForItem.
			// JM 10-27-09 NA 10.1 CardView
			//if (recordPresenter != null)
			if (recordPresenter != null && recordPresenterPrepareContainerCalled == false)
				recordPresenter.PrepareContainerForItem(item);

			// SSP 4/23/08
			// Moved the call to base implementation here from the beginning of the method. See
			// notes above for more info.
			// 
			base.PrepareContainerForItemOverride( element, item );
		}

				#endregion //PrepareContainerForItemOverride

				// JJD 5/22/07 - Optimization
				// RecordListControl now derives from RecyclingItemsControl 
				#region ReuseContainerForNewItem

		/// <summary>
		/// Called when a container is being reused, i.e. recycled, or a different item.
		/// </summary>
		/// <param name="container">The container being reused/recycled.</param>
		/// <param name="item">The new item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the container had previously been deactivated then the original setting for the Visibility property, prior to its deactivation (refer to <see cref="RecyclingItemsControl.DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		internal protected override void ReuseContainerForNewItem(DependencyObject container, object item)
		{
			RecordPresenter rp = GetRecordPresenterFromContainer(container);

			Debug.Assert(rp != null);

			if (rp == null)
			{
				base.ReuseContainerForNewItem(container, item);
				return;
			}

			rp.PrepareContainerForItem(item);

			// JM 12-3-09
			this.PrepareViewContainerForRecordPresenter(container, rp);
		}

				#endregion //ReuseContainerForNewItem	

                #region ShouldDeactivateContainer

        // JJD 3/11/10 - TFS28705 - Optimization
        /// <summary>
        /// Called after a Reset notification to determine if the Container should be de-activated instead of cleared.
        /// </summary>
        /// <param name="container">The container to be deactivated or cleared.</param>
        /// <param name="item">Its associated item.</param>
        /// <returns>True to de-activate the cointainer after a reset or false to clear it. The default is false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal protected override bool ShouldDeactivateContainer(DependencyObject container, object item)
        {
            FlatScrollRecordsCollection flatCollection = this.ItemsSource as FlatScrollRecordsCollection;

            if (flatCollection != null && !flatCollection.HasPendingNotifications)
            {
                DataRecord rcd = item as DataRecord;

				if (rcd != null &&

					// MD 3/16/11 - TFS24163
					// We will now allow this for all records, not just root records. But we should always make sure that the field layout is still valid.
					//rcd.ParentRecord == null &&
					rcd.FieldLayout.HasBeenInitializedAfterDataSourceChange &&

                    rcd.FieldLayout.IsInitialRecordLoaded &&
                    !rcd.IsSpecialRecord &&
                    rcd.IsStillValid)
                {
                    return true;
                }
            }

            return false;
        }

                #endregion //ShouldDeactivateContainer	
    
			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

                // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
                #region FixedNearElementTransform

        /// <summary>
        /// Identifies the <see cref="FixedNearElementTransform"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixedNearElementTransformProperty =
            RecordPresenter.FixedNearElementTransformProperty.AddOwner(typeof(RecordListControl));

        /// <summary>
        /// Returns a transform object that can be used to scroll an element back into view when using fixed fields.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This property will only returning a usable object when fixed fields are supported by 
        /// the view and fixed fields are being used - either having fixed one or more fields or setting the <see cref="FieldSettings.AllowFixing"/> 
        /// such that fixing is allowed.</p>
        /// </remarks>
        /// <seealso cref="FixedNearElementTransformProperty"/>
        /// <seealso cref="ScrollableElementTransform"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
        /// <seealso cref="ViewBase.IsFixedFieldsSupported"/>
        //[Description("Returns a transform object that can be used to scroll an element back into view when using fixed fields.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Transform FixedNearElementTransform
        {
            get
            {
                return (Transform)this.GetValue(RecordListControl.FixedNearElementTransformProperty);
            }
        }

                #endregion //FixedNearElementTransform

				#region HorizontalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="HorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(RecordListControl), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox));

		/// <summary>
		/// Determines if a horizontal scrollbar is visible.
		/// </summary>
		/// <seealso cref="HorizontalScrollBarVisibilityProperty"/>
		//[Description("Determines if a horizontal scrollbar is visible.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(RecordListControl.HorizontalScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(RecordListControl.HorizontalScrollBarVisibilityProperty, value);
			}
		}

				#endregion //HorizontalScrollBarVisibility

				// JJD 2/22/12 - TFS101199 - added 
				#region IsRoot

		private static readonly DependencyPropertyKey IsRootPropertyKey =
			DependencyProperty.RegisterReadOnly("IsRoot",
			typeof(bool), typeof(RecordListControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsRoot"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRootProperty =
			IsRootPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this is the root level RecordListControl (read-only).
		/// </summary>
		/// <seealso cref="IsRootProperty"/>
		public bool IsRoot
		{
			get
			{
				return (bool)this.GetValue(RecordListControl.IsRootProperty);
			}
		}

				#endregion //IsRoot
   
                // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
                #region ScrollableElementTransform

        /// <summary>
        /// Identifies the <see cref="ScrollableElementTransform"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScrollableElementTransformProperty =
            RecordPresenter.ScrollableElementTransformProperty.AddOwner(typeof(RecordListControl));

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
                return (Transform)this.GetValue(RecordListControl.ScrollableElementTransformProperty);
            }
        }

                #endregion //ScrollableElementTransform

				#region VerticalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(RecordListControl), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox));

		/// <summary>
		/// Determines if a vertical scrollbar is visible.
		/// </summary>
		/// <seealso cref="VerticalScrollBarVisibilityProperty"/>
		//[Description("Determines if a vertical scrollbar is visible.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(RecordListControl.VerticalScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(RecordListControl.VerticalScrollBarVisibilityProperty, value);
			}
		}

				#endregion //VerticalScrollBarVisibility

			#endregion //Public Properties	
    
			#region Internal Properties

				#region DataPresenterBase

		internal DataPresenterBase DataPresenter
		{
			get { return this._dataPresenter; }
		}

				#endregion //DataPresenterBase
        
                // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
                #region IsFlatList

        internal bool IsFlattenedList { get { return this.ItemsSource is FlatScrollRecordsCollection; } }

                #endregion //IsFlatList	
    
				#region Panel

		internal Panel Panel
		{
			get 
            {
                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                // The panel is initialized in the measureoverride after calling the base
                // so if someone asks for it during that call to the base then they will
                // not get it.
                //
                if (null == this._panel)
                    this.FindPanel();

                return this._panel; 
            }
		}

				#endregion //Panel

				#region ScrollInfo






		internal IScrollInfo ScrollInfo
		{
			get { return this._panel as IScrollInfo; }
		}

				#endregion //ScrollInfo	

                // JJD 8/7/09 - NA 2009 Vol 2 - Enhanced grid view
                #region ViewableRecords

        internal ViewableRecordCollection ViewableRecords
        {
            get
            {
                object itemSource = this.ItemsSource;

                ViewableRecordCollection vrc = itemSource as ViewableRecordCollection;

                if (vrc != null)
                    return vrc;

                FlatScrollRecordsCollection flatList = itemSource as FlatScrollRecordsCollection;

                if (flatList != null)
                    return flatList.RootViewableRecords;

                return null;
            }
        }

                #endregion //ViewableRecords	
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				// JJD 11/21/11 - TFS69163 - added
				#region BeginCoerceItemsSourceToNull

		// Added logic to temporarily coerce the ItemsSource property to null for performance reasons
		// during mass expand/collapse operations
		internal void BeginCoerceItemsSourceToNull()
		{
			_coerceItemsSourceToNullCount++;
			this.CoerceValue(ItemsSourceProperty);
		}

				#endregion //BeginCoerceItemsSourceToNull	
    
				// JJD 11/21/11 - TFS69163 - added
				#region EndCoerceItemsSourceToNull

		// Added logic to temporarily coerce the ItemsSource property to null for performance reasons
		// during mass expand/collapse operations
		internal void EndCoerceItemsSourceToNull()
		{
			_coerceItemsSourceToNullCount--;
			this.CoerceValue(ItemsSourceProperty);
		}

				#endregion //EndCoerceItemsSourceToNull	
    
				// JJD 5/22/07 - Added GetRecordPresenterFromContainer
				#region GetRecordPresenterFromContainer

		
		
		
		
		/// <summary>
		/// Returns the record presenter from the specified item container. If the container
		/// itself is a record presenter then returns it.
		/// </summary>
		/// <param name="container">Item container from which to get the record presenter.</param>
		/// <returns>Returns the record presenter</returns>
		internal static RecordPresenter GetRecordPresenterFromContainer( DependencyObject container )
		{
			RecordPresenter rp = container as RecordPresenter;
			if ( null != rp )
				return rp;

			// If the container implements the IRecordPresenterContainer interface then get the
			// contained RecordPresenter from the interface's RecordPresenter property.
			IRecordPresenterContainer rpc = container as IRecordPresenterContainer;
			if ( null != rpc )
			{
				rp = rpc.RecordPresenter;
			}
			else
			{
				// If the container is derived from ContentControl then use the content property
				ContentControl cc = container as ContentControl;
				if ( null != cc )
					rp = cc.Content as RecordPresenter;
			}

			Debug.Assert( null != rp, string.Format( "Container is invalid type: {0}", container != null ? container.GetType( ).ToString( ) : "<null>" ) );

			return rp;
		}
		
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)


				#endregion //GetRecordPresenterFromContainer	

			#endregion //Internal Methods

			#region Private Methods

				// JJD 11/21/11 - TFS69163
				#region CoerceItemsSource

		// Added logic to temporarily coerce the ItemsSource property to null for performance reasons
		// during mass expand/collapse operations
		private static object CoerceItemsSource(DependencyObject target, object value)
		{
			RecordListControl instance = target as RecordListControl;

			// if the coerce count is > 0 then return null
			if (instance._coerceItemsSourceToNullCount > 0)
				return null;

			return value;
		}

				#endregion //CoerceItemsSource	

				#region CreateRecordPresenterForItem

		// SSP 4/22/08 - Summaries Feature
		// Added CreateRecordPresenterForItem helper method.
		// 
		private static RecordPresenter CreateRecordPresenterForItem( object item )
		{
            Record record = item as Record;


            // JJD 11/11/09 - TFS24665 
            // If the record is active see if we have cached the active record presenter
            // If so then re-use it
            if (record != null && record.IsActive)
            {
                DataPresenterBase dp = record.DataPresenter;

                if (dp != null)
                {
                    RecordPresenter rp = dp.PendingActiveRecordPresenter;

                    if (rp != null && rp.Record == record )
                    {
                        Debug.Assert(rp.Parent == null, "The parent should be null here");

                        if (rp.Parent == null)
                        {
                            // clear the cached ref on dp
                            dp.PendingActiveRecordPresenter = null;

                            return rp;
                        }
                    }
                }
            }

			RecordPresenter recordPresenter = null != record ? record.CreateRecordPresenter( ) : null;

			Debug.Assert( null != recordPresenter );
			return recordPresenter;
		}

				#endregion // CreateRecordPresenterForItem

                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                #region FindPanel
        private void FindPanel()
        {
            // Look for our Panel and cache a reference to it.
            if (this._panel == null && this.DataPresenter != null)
            {
                this._panel = Utilities.GetDescendantFromType(this, this.DataPresenter.CurrentViewInternal.ItemsPanelType, true) as Panel;


                CarouselViewPanel carouselPanel = this._panel as CarouselViewPanel;
                if (carouselPanel != null)
                    carouselPanel.SelectionHost = this;

            }
        }
                #endregion //FindPanel

				#region GetContainerForItemOverrideHelper

		
		
		/// <summary>
		/// Returns the container for specified item. Item can be null in which case 
		/// a RecordListItemContainer is returned for purposes of creating the right
		/// record presenter element when it's associated with a record object.
		/// </summary>
		/// <param name="item">Item for which to get the container</param>
		/// <returns>Container element for the item</returns>
		private System.Windows.DependencyObject GetContainerForItemOverrideHelper( object item )
		{
			DependencyObject viewContainer = this.GetViewContainerForRecordPresenter( );
			if ( null != viewContainer )
				return viewContainer;

			Record record = item as Record;
			if ( null != record )
			{
				return CreateRecordPresenterForItem( item );
			}
			else
			{
				// If we don't know the record for which this container is being created, we
				// have to create a generic container that will create the right record presenter
				// based on the record it gets associated with in the PrepareContainerForItemOverride.
				// 
				return new RecordListItemContainer( );
			}
		}

				#endregion // GetContainerForItemOverrideHelper

				#region GetViewContainerForRecordPresenter

		
		
		/// <summary>
		/// Gets the container for record presenter as returned by the view via
		/// its GetContainerForRecordPresenter method.
		/// </summary>
		/// <returns>Returns the view provided record presenter container. 
		/// If none was provided by the view, then returns null.</returns>
		private DependencyObject GetViewContainerForRecordPresenter( )
		{
			DataPresenterBase dp = this.DataPresenter;
			ViewBase view = dp.CurrentViewInternal;

			DependencyObject container = view.GetContainerForRecordPresenter( this._panel );
			if ( container != null )
			{
				if ( !( container is RecordPresenter ) )
					this.ValidateRecordPresenterContainer( view, container );

				return container;
			}

			return null;
		}

				#endregion // GetViewContainerForRecordPresenter

                // JJD 11/11/09 - TFS24665 - added
                #region OnLayoutUpdated

		// JJD 3/15/11 - TFS65143 - Optimization
		// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
		// and just wire LayoutUpdated on the DP
		//private void OnLayoutUpdated(object sender, EventArgs e)
        private void OnLayoutUpdated()
        {
			// JJD 3/15/11 - TFS65143 - Optimization
			// No need to unhook since the DP is maintaining the callback list and automatically removes the entry
			// unhook the event
            //this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

            // clear the cached rp since it wasn't re-used during this 
            // measure pass
            if (this._dataPresenter != null)
                this._dataPresenter.ClearPendingActiveRecordPresenter();
        }

                #endregion //OnLayoutUpdated	
    
				#region PrepareViewContainerForRecordPresenter

		// SSP 4/22/08 - Summaries Feature
		// Took out RecordPresenter parameter from ViewBase.GetContainerForRecordPresenter because in GetContainerForItemOverride
		// we don't have the item and thus don't know which record presenter to create, and thus we cannot pass that
		// along into this method. Instead we added ViewBase.PrepareContainerForRecordPresenter method that will allow the view
		// to associated its wrapper to the record presenter.
		// 
		private void PrepareViewContainerForRecordPresenter( DependencyObject viewContainer, RecordPresenter recordPresenter )
		{
			Debug.Assert( null != viewContainer && null != recordPresenter );
			this.DataPresenter.CurrentViewInternal.PrepareContainerForRecordPresenter( _panel, viewContainer, recordPresenter );
		}

				#endregion // PrepareViewContainerForRecordPresenter

				#region ValidateRecordPresenterContainer

		
		
		/// <summary>
		/// Validates the container to make sure it's a valid container for record presenter elements.
		/// Containers that are ContentControl or those that implement IRecordPresenterContainer interface
		/// are considered valid. Throws an exception when the container is invalid.
		/// </summary>
		/// <param name="view">Data presenter view</param>
		/// <param name="container">Record presenter container to validate</param>
		private void ValidateRecordPresenterContainer( ViewBase view, DependencyObject container )
		{
			Type containerType = container.GetType( );

			Type viewRecordPresenterContainerType = view.RecordPresenterContainerType;
			Debug.Assert( viewRecordPresenterContainerType != null &&
					viewRecordPresenterContainerType.IsAssignableFrom( containerType ) );

			if ( !typeof( IRecordPresenterContainer ).IsAssignableFrom( containerType ) &&
				!typeof( ContentControl ).IsAssignableFrom( containerType ) )
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_15" ) );
		}

				#endregion // ValidateRecordPresenterContainer

			#endregion //Private Methods

		#endregion //Methods

		#region ICarouselPanelSelectionHost Members

		#region SelectedItemIndex

		/// <summary>
		/// Returns/sets the index of the currently selected item.  If more than 1 item is selected this property returns/sets the first selected item.
		/// </summary>
		int ICarouselPanelSelectionHost.SelectedItemIndex
		{
			get 
			{
				if (this.DataPresenter == null || this.DataPresenter.SelectedItems.Records.Count == 0)
					return -1; 

				return this.DataPresenter.SelectedItems.Records[0].Index;
			}
			set 
			{ 
				IList records = this.ItemsSource as IList;
				if (records != null)
				{
					Record record = records[value] as Record;

					if (record != null  &&  this.DataPresenter != null)
						this.DataPresenter.InternalSelectItem(record, true, true);
				}
			}
		}

			#endregion //SelectedItemIndex	

		#endregion //ICarouselPanelSelectionHost Members
	}
}

namespace Infragistics.Windows.DataPresenter.Internal
{
	// JJD 2/28/11 - TFS66934 - Optimization
	#region RoutedCommandProxy class

	internal class RoutedCommandProxy : ICommand
	{
		#region Private Members

		private ButtonBase _button;
		private RoutedCommand _command;

		#endregion //Private Members	
    
		#region Constructor

		private RoutedCommandProxy(ButtonBase button, RoutedCommand command)
		{
			this._button = button;
			this._command = command;
		}

		#endregion //Constructor	
   
		internal static void WireProxy(ButtonBase button)
		{
			GridUtilities.ValidateNotNull(button);
			RoutedCommand command = button.Command as RoutedCommand;

			if (command != null)
			{
				button.Command = new RoutedCommandProxy(button, command);
			}
		}

		#region ICommand Members

		bool ICommand.CanExecute(object parameter)
		{
			// We always return true, which is what the ScrollBar RepeatButton commands always return anyway.
			// This prevents the extra routed events from being generated
			return true;
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add
			{
				// Do nothing since we will never raise this event
			}
			remove
			{
				// Do nothing since we will never raise this event
			}
		}

		void ICommand.Execute(object parameter)
		{
			if (this._button.Command == this)
			{
				IInputElement target = this._button.CommandTarget;

				if (target == null)
					target = this._button;

				// JJD 3/17/11
				// Call CanEecute first. This will allow wiring of the PreviewCanExecute and CanExecute
				// routed events
				if (this._command.CanExecute(parameter, target))
					this._command.Execute(parameter, target);
			}
		}

		#endregion
	}

	#endregion //RoutedCommandProxy class	
    
	#region RecordListItemContainer Class

	
	
	/// <summary>
	/// For internal use only. Under certain situations where recycling is not enabled, this element 
	/// will be used will be used as a item container for record list control.
	/// </summary>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class RecordListItemContainer : Decorator, IRecordPresenterContainer
	{
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal RecordListItemContainer( )
		{
		}

		#endregion // Constructor

		#region InitializeWithItem

		/// <summary>
		/// Initializes the container with the record.
		/// </summary>
		/// <param name="recordPresenter">RecordPresenter that's going to be displayed inside this container.</param>
		internal void InitializeWithItem( RecordPresenter recordPresenter )
		{
			this.Child = recordPresenter;
		}

		#endregion // InitializeWithItem

		#region Base Overrides

		#region Child

		/// <summary>
		/// Overridden. Gets or sets the child element.
		/// </summary>
		public override UIElement Child
		{
			get
			{
				return base.Child;
			}
			set
			{
				UIElement oldChild = this.Child;
				if ( oldChild != value )
				{
					base.Child = value;

					// Bind horizontal and vertical alignment properties to 
					if ( null != value )
					{
						// SSP 3/26/10 TFS26525
						// We temporarily explicitly set the DataContext on the record presenter while
						// its pulled out of element tree. When putting it back in, clear the DataContext.
						// 
						value.ClearValue( DataContextProperty );

						Binding horizAlignBinding = Utilities.CreateBindingObject( HorizontalAlignmentProperty, BindingMode.TwoWay, value );
						Binding vertAlignBinding = Utilities.CreateBindingObject( VerticalAlignmentProperty, BindingMode.TwoWay, value );

						BindingOperations.SetBinding( this, HorizontalAlignmentProperty, horizAlignBinding );
						BindingOperations.SetBinding( this, VerticalAlignmentProperty, vertAlignBinding );

                        // JJD 10/15/09 - TFS23738
                        // Bind the Visibility property as well so we don't get unnecessary
                        // size change notifications that can cause perpertual measure invalidations
                        // on the panel in certain situations
                        BindingOperations.SetBinding(this, VisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.TwoWay, value));
                    }
                    else
                    {
                        // JJD 10/15/09 - TFS23738
                        // Clear the bindings if value is null
                        BindingOperations.ClearAllBindings(this);
                    }
                }
			}
		}

		#endregion // Child

		#endregion // Base Overrides
    
		#region IRecordPresenterContainer Members

		/// <summary>
		/// Returns the associated record presenter element.
		/// </summary>
		public RecordPresenter RecordPresenter
		{
			get 
			{
				RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer( this.Child );
				Debug.Assert( null != rp );
				return rp;
			}
		}

		#endregion

	}

	#endregion // RecordListItemContainer Class
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