using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Windows.Selection;
using System.Windows.Threading;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Reporting;

namespace Infragistics.Windows.Virtualization
{
	/// <summary>
	/// Abstract base class for panels that want to take advantage of recycling item containers.
	/// </summary>
	/// <remarks>
	/// <para class="body">In order to recycle item containers, the panel must be the ItemsHost for a <see cref="RecyclingItemsControl"/>. Otherwise, one of the other <see cref="ItemContainerGenerationMode"/>s will be used.</para>
	/// </remarks>
	/// <seealso cref="RecyclingItemsControl"/>
	/// <seealso cref="ItemContainerGenerationMode"/>
	abstract public class RecyclingItemsPanel : VirtualizingPanel
	{
		#region Private Members

		private bool				_hasBeenPreloaded;
		private bool				_recyclingEventsWired;
		private DispatcherOperation _pendingAsyncCleanup;
		private DispatcherOperation _pendingPostResetVerification;
		private DispatcherTimer		_cleanupDelayTimer;
		private ItemsControl		_itemsControl;
		private bool				_zOrderDirty;
		private bool				_wasRecyclingOnLastGeneration;
		private ItemContainerGenerationMode
									_cachedItemContainerGenerationMode = ItemContainerGenerationMode.PreLoad;
		private RecycleableContainerCollection
									_children;

		// JM 03-25-09 TFS 15539 
		private bool				_isInCleanup;

        // JJD 3/4/10 - added
        private bool                _removeDeactivatedContainersOnCleanup;

		#endregion //Private Members	
		
		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		protected RecyclingItemsPanel() : this(false)
		{
		}

        // JJD 3/4/10
        // Added removeDeactivatedContainersOnCleanup ctor
        /// <summary>
		/// Constructor
		/// </summary>
        /// <param name="removeDeactivatedContainersOnCleanup">If true then when recyling will remove deactivated containers during a cleanup operation.</param>
		protected RecyclingItemsPanel(bool removeDeactivatedContainersOnCleanup)
		{
            this._removeDeactivatedContainersOnCleanup = removeDeactivatedContainersOnCleanup;
		}

		#endregion //Constructor	
    
		#region Constants

		private const int MAX_TICKS_IN_CLEANUP = 200;
		private const int MAXIMUM_UNUSED_GENERATED_ITEMS_TO_KEEP = 50;

		#endregion //Constants

		#region Event Handlers

			#region OnContainerDiscarded

		private void OnContainerDiscarded(object sender, RecyclingItemContainerGenerator.ContainerDiscardedEventArgs e)
		{
			this._zOrderDirty = true;

            // JJD 1/22/10 - TFS26513
            // Check to make sure this is the logical parent before trying
            // to remove it
            if (LogicalTreeHelper.GetParent(e.Container) == this)
			    this.RemoveLogicalChild(e.Container);

			if ( e.Container is Visual)
				this.RemoveVisualChild(e.Container as Visual);

			// JM 10-21-10 TFS57364 - If the generator is in the middle of a RemoveAll operation, then
			// invalidate our measure.
			RecyclingItemContainerGenerator generator = sender as RecyclingItemContainerGenerator;
			if (null != generator && generator.IsInRemoveAll)
				this.InvalidateMeasure();

			// AS 1/6/12 TFS26281
			// If the container we have is being removed and it has the keyboard focus then 
			// we should shift it to an ancestor focusable element so it won't get shifted to 
			// the window itself.
			//
			IInputElement inputElement = e.Container as IInputElement;

			if (null != inputElement && inputElement.IsKeyboardFocusWithin)
			{
				var ancestor = Utilities.GetParent(this, true);

				while (ancestor != null)
				{
					IInputElement ancestorInput = ancestor as IInputElement;

					if (ancestorInput != null && ancestorInput.Focusable)
					{
						ancestorInput.Focus();
						break;
					}

					ancestor = Utilities.GetParent(ancestor, true);
				}
			}
		}

			#endregion //OnContainerDiscarded	

			#region OnItemContainerGenerationModeChanged

        // SSP 5/3/10 TFS26525
        // Rewrote this method as a static method so it can be used as a value change callback for
        // dp property. Original method is commented out below.
        // 
        private static void OnItemContainerGenerationModeChanged(DependencyObject item, DependencyPropertyChangedEventArgs e)
        {
            RecyclingItemsPanel panel = item as RecyclingItemsPanel;

            panel._cachedItemContainerGenerationMode = (ItemContainerGenerationMode)e.NewValue;

            panel._hasBeenPreloaded = false;
            panel.InvalidateMeasure();

            bool isReCycling = panel._cachedItemContainerGenerationMode == ItemContainerGenerationMode.Recycle;

            if (isReCycling != panel._wasRecyclingOnLastGeneration)
                panel.OnIsRecyclingStateChanged();
        }

        
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


			#endregion //OnItemContainerGenerationModeChanged	
    
			#region OnZOrderChanged

		private void OnZOrderChanged(object sender, EventArgs e)
		{
			this._zOrderDirty = true;
		}

			#endregion //OnZOrderChanged	
    
		#endregion //Event Handlers	
    
		#region Base class overrides

			#region AddInternalChild (new)

		/// <summary>
		/// Adds the specified UIElement to the InternalChildren collection of a VirtualizingPanel element.
		/// </summary>
		/// <param name="child">The UIElement child to add to the collection.</param>
		new protected void AddInternalChild(UIElement child)
		{
			if (this.PerformRecyclingAdd(child) == false )
				base.AddInternalChild(child);
		}

			#endregion //AddInternalChild (new)	

			#region Children
		
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
		/// Returns the collection of child elements for this panel when the <see cref="ItemContainerGenerationModeResolved"/> is set to something other than <b>Recycle</b>.
		/// </summary>
		/// <remarks>
		/// <p class="note">The <see cref="Children"/> property is not used when the <see cref="ItemContainerGenerationModeResolved"/> is set to <b>Recycle</b>. Instead, the 
		/// <see cref="ChildElements"/> property should be used.</p>
		/// </remarks>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		new public UIElementCollection Children
		{
			get { return base.Children; }
		}

		/// <summary>
		/// Returns the collection of child elements for this panel.
		/// </summary>
		public IList ChildElements
		{
			get
			{
				if (this._children == null)
					this._children = new RecycleableContainerCollection(this);

				return this._children;
			}
		}

			#endregion //Children	
    
			#region GetVisualChild

		/// <summary>
		/// Gets a child element at the specied index.
		/// </summary>
		/// <param name="index">The zero-based index of the child element</param>
		/// <returns>The child element.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (this._wasRecyclingOnLastGeneration)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
				{
					if (this._zOrderDirty)
					{
						if (this.InternalChildren.Count > 0)
						{
							this._zOrderDirty = false;

							// Call the base implementation of GetVisualChild to trigger a refresh of the 
							// ZOrder logic in the framework
							Visual visual = base.GetVisualChild(0);
						}
					}
					return generator.GetContainer(index, true) as Visual;
				}
			}
			return base.GetVisualChild(index);
		}

			#endregion //GetVisualChild	

			#region InsertInternalChild (new)

		/// <summary>
		/// Adds the specified UIElement to the InternalChildren collection of a VirtualizingPanel element at the specified index position.
		/// </summary>
		/// <param name="index">The index position within the collection at which the child element is inserted.</param>
		/// <param name="child">The UIElement child to add to the collection.</param>
		new protected void InsertInternalChild(int index, UIElement child)
		{
			if (this.PerformRecyclingAdd(child) == false)
				base.InsertInternalChild(index, child);
		}

			#endregion //InsertInternalChild (new)	

			#region LogicalChildren

		/// <summary>
		/// Gets an enumerator that can iterate the logical child elements of this element.
		/// </summary>
		/// <value>An IEnumerator. This property has no default value.</value>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
					return new ContainerEnumerator(this, false);

				return base.LogicalChildren;
			}
		}

			#endregion //LogicalChildren	
    
            #region OnIsItemsHostChanged
        /// <summary>
        /// Invoked when the <see cref="Panel.IsItemsHost"/> has changed.
        /// </summary>
        /// <param name="oldIsItemsHost">The old property value</param>
        /// <param name="newIsItemsHost">The new property value</param>
        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            // AS 10/14/08 TFS8200
            // If the template of the parent ItemsControl changes then the panel will 
            // no longer be the itemshost of the itemscontrol but its elements will 
            // still be children of it so we need to clean up all the children and 
            // unhook the generated events. What was happening is that the new panel
            // was returning the containers of the itemscontrol's generator but those 
            // were still visual children of the old panel and never added as visual
            // children of the new panel. The old panel needs to clean up the generator 
            // when it is no longer the items host and unhook from its events.
            //
            if (newIsItemsHost == false && oldIsItemsHost == true)
            {
                if (this._recyclingEventsWired)
                {
                    RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

                    if (null != generator)
                    {
                        this._recyclingEventsWired = false;

                        generator.DeactivateAllContainers();
                        generator.RemoveAllDeactivatedContainers();

                        generator.ContainerDiscarded -= new EventHandler<RecyclingItemContainerGenerator.ContainerDiscardedEventArgs>(OnContainerDiscarded);
                        generator.ZOrderChanged -= new EventHandler(OnZOrderChanged);
                    }
                }
            }

            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
        } 
            #endregion //OnIsItemsHostChanged

			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					{
						this._hasBeenPreloaded = false;

						this._zOrderDirty = true;

						// JJD 6/1/07
						// If we are recycling make sure that everything is cleaned up even if we don't receive a subsequent Measure
                        if (this._pendingPostResetVerification == null &&
                            this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
                        {
                            // JJD 3/29/08 - added support for printing.
                            // We can't do asynchronous operations during a print
                            if (Utilities.AllowsAsyncOperations(this))
                                this.OnPostResetVerification(null);
                            else
                                this._pendingPostResetVerification = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(this.OnPostResetVerification), null);
                        }

						break;
					}
			}

			base.OnItemsChanged(sender, args);
		}

			#endregion //OnItemsChanged	

			#region RemoveInternalChildRange (new)

		/// <summary>
		/// Removes child elements from the InternalChildren collection.
		/// </summary>
		/// <param name="index">The beginning index position within the collection at which the first child element is removed.</param>
		/// <param name="range">The total number of child elements to remove from the collection.</param>
		new protected void RemoveInternalChildRange(int index, int range)
		{
			if (this.ItemContainerGenerationModeResolved != ItemContainerGenerationMode.Recycle)
				base.RemoveInternalChildRange(index, range);
		}

			#endregion //RemoveInternalChildRange (new)	
    
			#region VisualChildrenCount

		/// <summary>
		/// Returns the count of the parent child elements
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				if (this._wasRecyclingOnLastGeneration)
				{
					RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

					if (generator != null)
						return generator.CountOfAllContainers;
				}

				return base.VisualChildrenCount;
			}
		}

			#endregion //VisualChildrenCount	
    
		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ActiveItemContainerGenerator

		/// <summary>
		/// Returns the active item generator (read-only)
		/// </summary>
		/// <value>If the <see cref="ItemContainerGenerationModeResolved"/> property returns 'Recycle' this will return the <see cref="RecyclingItemsControl"/>'s <see cref="RecyclingItemsControl.RecyclingItemContainerGenerator"/>, otherwise it will return the base ItemsControl's ItemContainerGenerator.</value>
		[Browsable(false)]
		[Bindable(false)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[EditorBrowsable( EditorBrowsableState.Advanced)]
		public IItemContainerGenerator ActiveItemContainerGenerator
		{
			get
			{
				ItemsControl owner = this.ItemsControl;

				if (owner == null)
					return null;

				if (this._wasRecyclingOnLastGeneration)
				{
					if (owner is RecyclingItemsControl)
						return ((RecyclingItemsControl)owner).RecyclingItemContainerGenerator;
				}

				return owner.ItemContainerGenerator;
			}
		}

				#endregion //ActiveItemContainerGenerator

				#region ItemContainerGenerationModeResolved

		/// <summary>
		/// Returns the resolved value for the ItemContainerGenerationMode property (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>The default value of 'Recycle' only has meaning when the <see cref="RecyclingItemsPanel"/> is used within a <see cref="RecyclingItemsControl"/>. When used inside other ItemsControls this property will return 'Virtualize' place of 'Recycle'.</para>
		/// </remarks>
		/// <seealso cref="RecyclingItemsControl"/>
		/// <seealso cref="RecyclingItemsControl.ItemContainerGenerationMode"/>
		[Browsable(false)]
		[Bindable(false)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public ItemContainerGenerationMode ItemContainerGenerationModeResolved
		{
			get
			{
				return this._cachedItemContainerGenerationMode;
			}
		}

				#endregion //ItemContainerGenerationModeResolved	

			#endregion //Public Properties	

			#region Propected Properties
    
			#endregion //Propected Properties	

			#region Protected Virtual Properties

				#region IsOkToCleanupUnusedGeneratedElements

		/// <summary>
		/// Called when elements are about to be cleaned up.  Return true to allow cleanup, false to prevent cleanup.
		/// </summary>
		protected virtual bool IsOkToCleanupUnusedGeneratedElements
		{
			get { return true; }
		}

				#endregion IsOkToCleanupUnusedGeneratedElements

				#region TotalVisibleGeneratedItems [Abstract]

		/// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected abstract int TotalVisibleGeneratedItems
		{
			get;
		}

				#endregion //TotalVisibleGeneratedItems [Abstract]

				#region MaximumUnusedGeneratedItemsToKeep

		/// <summary>
		/// Returns the maximum number of unused generated items that should be kept around at any given time.
		/// </summary>
		protected virtual int MaximumUnusedGeneratedItemsToKeep
		{
			get { return RecyclingItemsPanel.MAXIMUM_UNUSED_GENERATED_ITEMS_TO_KEEP; }
		}

				#endregion //MaximumUnusedGeneratedItemsToKeep

			#endregion //Protected Virtual Properties

			#region Internal Properties

				#region ItemsControl






		internal ItemsControl ItemsControl
		{
			get
			{
				if (this._itemsControl == null)
				{
					this._itemsControl = ItemsControl.GetItemsOwner(this) as ItemsControl;

					if (this._itemsControl is RecyclingItemsControl)
					{
						// AS 6/1/07
						// Using a DependencyPropertyDescriptor will actually root this class.
						//
						//DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(RecyclingItemsControl.ItemContainerGenerationModeProperty, typeof(RecyclingItemsControl));
						//dpd.AddValueChanged(this._itemsControl, new EventHandler(this.OnItemContainerGenerationModeChanged));
						this.SetBinding(RecyclingItemsPanel.ItemContainerGenerationModeProperty, Utilities.CreateBindingObject(RecyclingItemsControl.ItemContainerGenerationModeProperty, System.Windows.Data.BindingMode.OneWay, this._itemsControl));

						this._cachedItemContainerGenerationMode = ((RecyclingItemsControl)this._itemsControl).ItemContainerGenerationMode;
					}
					else
					{
						// If null initialize gen mode to 'PreLoad', otherwise to 'Virtualize'
						if (this._itemsControl == null)
							this._cachedItemContainerGenerationMode = ItemContainerGenerationMode.PreLoad;
						else
							this._cachedItemContainerGenerationMode = ItemContainerGenerationMode.Virtualize;
					}
				}

				return this._itemsControl;
			}
		}

				#endregion //ItemsControl

				#region TotalItems

		internal int TotalItems
		{
			get
			{
				if (this.ItemsControl != null && this.ItemsControl.HasItems)
					return this.ItemsControl.Items.Count;


				return 0;
			}
		}

				#endregion //TotalItems

			#endregion //Internal Properties

			#region Private Properties

				#region CountOfActiveContainers

		///// <summary>
		///// Returns the number of active containers (read-only).
		///// </summary>
		//[Browsable(false)]
		//[Bindable(false)]
		//[ReadOnly(true)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		private int CountOfActiveContainers 
		{ 
			get 
			{
				if (this._wasRecyclingOnLastGeneration)
				{
					RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

					if (generator != null)
						return generator.CountOfActiveContainers;
				}

				return this.InternalChildren.Count; 
			} 
		}

				#endregion //CountOfActiveContainers	

				#region ItemContainerGenerationMode
		// AS 6/1/07
		private static DependencyProperty ItemContainerGenerationModeProperty = 
            RecyclingItemsControl.ItemContainerGenerationModeProperty.AddOwner( typeof(RecyclingItemsPanel),
            // SSP 5/3/10 TFS26525
            // We need to update the _cachedItemContainerGenerationMode member variable. Added 
            // OnItemContainerGenerationModeChanged method as the value change callback.
            // 
            new FrameworkPropertyMetadata( OnItemContainerGenerationModeChanged ) );


				#endregion //ItemContainerGenerationMode

				#region TotalGeneratedChildrenCount

		private int TotalGeneratedChildrenCount
		{
			get { return (base.IsItemsHost ? base.InternalChildren.Count : 0); }
		}

				#endregion //TotalGeneratedChildrenCount
                
                // JJD 05/07/10 - TFS31643 -added
				#region UnderlyingItemsCount

		private int UnderlyingItemsCount 
        { 
            get 
            {
                RecyclingItemsControl rlc = this.ItemsControl as RecyclingItemsControl;

                if ( rlc != null )
                    return rlc.UnderlyingItemsCount;

                if (this._itemsControl != null)
                    return this._itemsControl.Items.Count;

                return 0; 
            } 
        }

				#endregion //UnderlyingItemsCount


			#endregion //Private Properties	
            
		#endregion //Properties	

		#region Methods

			#region Public Methods

                // JJD 10/06/09 - NA 2010 vol 1 - Tiles Control added
				#region ContainerFromIndex

		/// <summary>
		/// Returns the element corresponding to the item at the given index within the ItemCollection.
		/// </summary>
		/// <param name="index">The index of the desired item.</param>
		/// <returns>Returns the element corresponding to the item at the given index within the ItemCollection or returns null if the item is not realized.</returns>
		public DependencyObject ContainerFromIndex(int index)
		{
            if (this.IsItemsHost == false)
                return this.ChildElements[index] as DependencyObject;

            IItemContainerGenerator activeGenerator = this.ActiveItemContainerGenerator;

            RecyclingItemContainerGenerator recyclingGenerator = activeGenerator as RecyclingItemContainerGenerator;

            if (recyclingGenerator != null)
                return recyclingGenerator.ContainerFromIndex(index);

            ItemContainerGenerator generator = activeGenerator as ItemContainerGenerator;

			if (generator != null)
			{
				// JJD 4/20/11 - TFS73180
				// call ContainerFromIndex instead
				//return generator.ContainerFromItem(index);
				return generator.ContainerFromIndex(index);
			}

            Debug.Fail("There should always be a generator for an ItemsHost");

            return this.ChildElements[index] as DependencyObject;
        }

				#endregion //ContainerFromIndex	

                // JJD 10/06/09 - NA 2010 vol 1 - Tiles Control added
				#region ContainerFromItem

		/// <summary>
		/// Returns the UIElement container corresponding to the given item.
		/// </summary>
		/// <param name="item">The Object item to find the UIElement container for.</param>
		/// <returns>A UIElement that corresponds to the given item. Returns null if the item does not belong to the item collection, or if an UIElement has not been generated for it.</returns>
		public DependencyObject ContainerFromItem(object item)
		{
            if (this.IsItemsHost == false)
                return item as DependencyObject;

            IItemContainerGenerator activeGenerator = this.ActiveItemContainerGenerator;

            RecyclingItemContainerGenerator recyclingGenerator = activeGenerator as RecyclingItemContainerGenerator;

            if (recyclingGenerator != null)
                return recyclingGenerator.ContainerFromItem(item);

            ItemContainerGenerator generator = activeGenerator as ItemContainerGenerator;

            if (generator != null)
                return generator.ContainerFromItem(item);

            Debug.Fail("There should always be a generator for an ItemsHost");

            return item as DependencyObject;
        }

				#endregion //ContainerFromItem	

                // JJD 10/06/09 - NA 2010 vol 1 - Tiles Control added
                #region ItemFromContainer

        /// <summary>
        /// Returns the item that corresponds to the specified, generated UIElement. 
        /// </summary>
        /// <param name="container">The DependencyObject that corresponds to the item to be returned.</param>
        /// <returns>A DependencyObject that is the item which corresponds to the specified, generated UIElement. If the UIElement has not been generated, UnsetValue is returned.</returns>
        /// <exception cref="ArgumentNullException">container is null</exception>
        public object ItemFromContainer(DependencyObject container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (this.IsItemsHost == false)
                return container;

            IItemContainerGenerator activeGenerator = this.ActiveItemContainerGenerator;

            RecyclingItemContainerGenerator recyclingGenerator = activeGenerator as RecyclingItemContainerGenerator;

            if (recyclingGenerator != null)
                return recyclingGenerator.ItemFromContainer(container);

            ItemContainerGenerator generator = activeGenerator as ItemContainerGenerator;

            if (generator != null)
                return generator.ItemFromContainer(container);

            Debug.Fail("There should always be a generator for an ItemsHost");

            return container;
        }

                #endregion //ItemFromContainer	
    
				#region RemoveAllDeactivatedContainers

		/// <summary>
		/// Removes all generated containers that have previously been deactivated.
		/// </summary>
		/// <remarks class="note"><b>Note:</b> This method does nothing unless the <see cref="ItemContainerGenerationModeResolved"/> property returns 'Recycle'.</remarks>
		public void RemoveAllDeactivatedContainers()
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
					generator.RemoveAllDeactivatedContainers();
			}
		}

				#endregion //RemoveAllDeactivatedContainers	
    
				// JJD 4/09/12 - TFS108549 - Added
				#region VerifyAllDeactivatedContainers

		/// <summary>
		/// Removes any generated containers that have previously been deactivated and but no longer valid..
		/// </summary>
		/// <remarks class="note"><b>Note:</b> This method does nothing unless the <see cref="ItemContainerGenerationModeResolved"/> property returns 'Recycle'.</remarks>
		public void VerifyAllDeactivatedContainers()
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
					generator.VerifyAllDeactivatedContainers();
			}
		}

				#endregion //VerifyAllDeactivatedContainers	

			#endregion //Public Methods

			#region Protected Methods

				#region BeginGeneration

		/// <summary>
		/// Called at the beginning of the generation process.
		/// </summary>
		/// <remarks>
		/// <para class="body">This method is usually called at the beginnning of a <see cref="RecyclingItemsPanel"/> derived class's <b>MeasureOverride</b> logic. 
		/// This is normally followed by one or more <see cref="RecyclingItemContainerGenerator.StartAt(GeneratorPosition, GeneratorDirection, bool)"/>/<see cref="RecyclingItemContainerGenerator.GenerateNext(out bool)"/> loops to generate all 'active' containers. Finally, the <see cref="EndGeneration"/> method is called at the end of the measure logic.</para>
		/// </remarks>
		/// <param name="scrollDirection">Relative to the last generation to let the generator optimize the recycling logic.</param>
		/// <seealso cref="EndGeneration"/>
        protected void BeginGeneration(ScrollDirection scrollDirection)
        {
            this.BeginGeneration(scrollDirection, null);
        }

		/// <summary>
		/// Called at the beginning of the generation process.
		/// </summary>
		/// <remarks>
		/// <para class="body">This method is usually called at the beginnning of a <see cref="RecyclingItemsPanel"/> derived class's <b>MeasureOverride</b> logic. 
		/// This is normally followed by one or more <see cref="RecyclingItemContainerGenerator.StartAt(GeneratorPosition, GeneratorDirection, bool)"/>/<see cref="RecyclingItemContainerGenerator.GenerateNext(out bool)"/> loops to generate all 'active' containers. Finally, the <see cref="EndGeneration"/> method is called at the end of the measure logic.</para>
		/// </remarks>
		/// <param name="scrollDirection">Relative to the last generation to let the generator optimize the recycling logic.</param>
        /// <param name="itemsExpectedToBeGenerated">An optional list of items are are expected to be generated. This is used to optimize eleme nt recycling.</param>
		/// <seealso cref="EndGeneration"/>
        // JJD 3/10/10 - TFS28705 - Optimization
        // Added new overload that takes itemsExpectedToBeGenerated param
		protected void BeginGeneration(ScrollDirection scrollDirection, IEnumerable itemsExpectedToBeGenerated)
		{
			// Initialize flag
			this._wasRecyclingOnLastGeneration = false;
			
			// JJD 6/1/07
			// Null out the  _pendingPostResetVerification member 
			this._pendingPostResetVerification = null;

			// Access the ItemsControl property that will ensure that the cached ItemContainerGenerationMode
			// member has been initialized
			ItemsControl ic = this.ItemsControl;
			
			// If null return 
			if (ic == null)
				return;

			int count = this.InternalChildren.Count;

			switch (this.ItemContainerGenerationModeResolved)
			{
				case ItemContainerGenerationMode.Recycle:
				{
					this._wasRecyclingOnLastGeneration = true;

					RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

					if (generator != null)
					{
						if (this._recyclingEventsWired == false)
						{
							this._recyclingEventsWired = true;
							generator.ContainerDiscarded += new EventHandler<RecyclingItemContainerGenerator.ContainerDiscardedEventArgs>(OnContainerDiscarded);
							generator.ZOrderChanged += new EventHandler(OnZOrderChanged);
						}

                        // JJD 3/10/10 - TFS28705 - Optimization
                        // Pass in the itemsExpectedToBeGenerated
                        //generator.BeginGeneration(scrollDirection);
						generator.BeginGeneration(scrollDirection, itemsExpectedToBeGenerated);
					}
					else
						this._wasRecyclingOnLastGeneration = false;

					
					break;
				}
				case ItemContainerGenerationMode.PreLoad:
				{
					bool waschildCollectionEmpty = this.InternalChildren.Count == 0;

					if (this._hasBeenPreloaded == true && 
						waschildCollectionEmpty == false)
						break;

					IItemContainerGenerator generator = this.ActiveItemContainerGenerator;

					// preload/generate all items in list
					using (generator.StartAt(generator.GeneratorPositionFromIndex(0), GeneratorDirection.Forward, true))
					{
						bool isNewlyRealized;
						int index = 0;

                        // JJD 05/07/10 - TFS31643
                        // Get the underlying item count
                        int totalRecordsToPreload = this.UnderlyingItemsCount;

                        while (true)
						{
							FrameworkElement fe = generator.GenerateNext(out isNewlyRealized) as FrameworkElement;

							if (fe == null)
								break;

							if (isNewlyRealized)
							{
								base.InsertInternalChild(index, fe);
								generator.PrepareItemContainer(fe);
							}

							index++;

                            // JJD 05/07/10 - TFS31643
                            // once we reach the count stop preloading
                            if (index >= totalRecordsToPreload)
                                break;
                        }
					}

					this._hasBeenPreloaded = true;

					break;
				}
			}
		}

				#endregion //BeginGeneration

				#region CleanupUnusedGeneratedElements

		/// <summary>
		/// Causes previously generated elements that are no longer in view to be 'un-generated'
		/// and removed from our parent children collection.
		/// </summary>
		/// <returns>True if more unused (i.e., invisible) elements remain to be cleaned up, false if all unused elements have been cleaned up</returns>
		protected bool CleanupUnusedGeneratedElements()
		{
			// JM 03-25-09 TFS 15539 
			if (this._isInCleanup)
				return false;

            if (this.IsItemsHost == false || this.ItemContainerGenerationModeResolved != ItemContainerGenerationMode.Virtualize)
            {
                // JJD 3/4/10 
                // if flag is set call RemoveAllDeactivatedContainers to conver the recycling case
                if (this._removeDeactivatedContainersOnCleanup)
                    this.RemoveAllDeactivatedContainers();

                return false;
            }


			// JM 03-25-09 TFS 15539 - wrap in try/finally and set isInCleanup flag
			this._isInCleanup = true;
			try
			{
			int startTick						= Environment.TickCount;
			int totalGeneratedChildrenCount		= this.TotalGeneratedChildrenCount;
			int totalVisibleGeneratedItems		= this.TotalVisibleGeneratedItems;
			int	totalUnusedGeneratedElements	= totalGeneratedChildrenCount - totalVisibleGeneratedItems;

			if (totalUnusedGeneratedElements > this.MaximumUnusedGeneratedItemsToKeep)
			{
				UIElementCollection		internalChildren	= base.InternalChildren;
				IItemContainerGenerator generator			= this.ActiveItemContainerGenerator;

				for (int i = 0; i < totalGeneratedChildrenCount; i++)
				{
					// JM 03-25-09 TFS 15539 
					//if (this.ShouldAbortCleanup(startTick))
					if (this.ShouldAbortCleanup(startTick) != AbortAction.NoAbort)
					{
						// JJD 4/20/11 - TFS73180
						// Only return true is the totalUnusedGeneratedElements is > 
						// the MaximumUnusedGeneratedItemsToKeep.
						// Otherwise, the cleanup timer will continue to run
						//return totalUnusedGeneratedElements > 0;
						return totalUnusedGeneratedElements > this.MaximumUnusedGeneratedItemsToKeep;
					}

					// JJD 5/21/07
					// Bypass the child element if it has input focus
					if (internalChildren[i].IsKeyboardFocusWithin)
						continue;

					// JM 03-25-09 TFS 15539
					//if (this.GetCanCleanupItem(this.GetItemIndexFromChildIndex(i)) == true) 
					int lastItemIndexCleanupCandidate = -1;
					int itemIndex = this.GetItemIndexFromChildIndex(i);
					if (this.GetCanCleanupItem(itemIndex) == true)
					{
						// JM 03-25-09 TFS 15539
						// Keep track of the itemIndex of the last item that is a candidate for cleanup.  We will check and update this
						// below as we loop through subsequent children to ensure that we only cleanup a contiguous group of items.  As
						// soon as we hit a gap in item index, we break out of the loop and cleanup what we have.
						lastItemIndexCleanupCandidate = itemIndex;

						int cleanupCount = 1;
						for (int j = i + 1; j < totalGeneratedChildrenCount; j++)
						{
							// JJD 5/21/07
							// Break out of loop if the child element has input focus
							if (internalChildren[j].IsKeyboardFocusWithin)
								break;

							// JM 03-25-09 TFS 15539 
							//if (this.ShouldAbortCleanup(startTick))
							AbortAction abortAction = this.ShouldAbortCleanup(startTick);
							if (abortAction != AbortAction.NoAbort)
							{
								// JM 03-25-09 TFS 15539 - Cleanup what we have so far if AbortAction = AbortWithCleanup.
								if (abortAction == AbortAction.AbortWithCleanup)
								{
									generator.Remove(new GeneratorPosition(i, 0), cleanupCount);
									this.RemoveInternalChildRange(i, cleanupCount);

									totalUnusedGeneratedElements	-= cleanupCount;
									totalGeneratedChildrenCount		-= cleanupCount;
								}

								// JJD 4/20/11 - TFS73180
								// Only return true is the totalUnusedGeneratedElements is > 
								// the MaximumUnusedGeneratedItemsToKeep.
								// Otherwise, the cleanup timer will continue to run
								//return totalUnusedGeneratedElements > 0;
								return totalUnusedGeneratedElements > this.MaximumUnusedGeneratedItemsToKeep;
							}

							// JM 03-25-09 TFS 15539
							//if (this.GetCanCleanupItem(this.GetItemIndexFromChildIndex(j)) &&
							itemIndex = this.GetItemIndexFromChildIndex(j);
							if (this.GetCanCleanupItem(itemIndex)				&&
								itemIndex == lastItemIndexCleanupCandidate + 1	&&
								(totalUnusedGeneratedElements - cleanupCount)	> this.MaximumUnusedGeneratedItemsToKeep)
							{
								lastItemIndexCleanupCandidate = itemIndex;
								cleanupCount++;
							}
							else
								break;
						}

				        generator.Remove(new GeneratorPosition(i, 0), cleanupCount);
				        this.RemoveInternalChildRange(i, cleanupCount);

						totalUnusedGeneratedElements	-= cleanupCount;
						totalGeneratedChildrenCount		-= cleanupCount;
				        i--;	// re-process item at same index
					}

					if (totalUnusedGeneratedElements <= this.MaximumUnusedGeneratedItemsToKeep)
						break;
				}
			}
			}
			finally
			{
				this._isInCleanup = false;
			}

			return false;
		}

				#endregion //CleanupUnusedGeneratedElements

				#region DeactivateAllContainers

		/// <summary>
		/// Deactivates all containers.
		/// </summary>
		public void DeactivateAllContainers()
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
					generator.DeactivateAllContainers();
			}
		}

				#endregion //DeactivateAllContainers	

				#region DeactivateContainer

		/// <summary>
		/// Deactivates a specific container.
		/// </summary>
		/// <param name="container">The container to deactivate.</param>
		public void DeactivateContainer(DependencyObject container)
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
					generator.DeactivateContainer(container);
			}
		}

				#endregion //DeactivateContainer	

				#region EndGeneration

		/// <summary>
		/// Called at the end of the generation process.
		/// </summary>
        /// <seealso cref="BeginGeneration(ScrollDirection, IEnumerable)"/>
		protected void EndGeneration()
		{
			switch (this.ItemContainerGenerationModeResolved)
			{
				case ItemContainerGenerationMode.Recycle:
				{
					RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

					if (generator != null)
					{
						generator.EndGeneration();
						this._zOrderDirty = true;

						// JJD 6/4/07
						// make sure there is at least one element in the base InternalChildren collection which will allow
						// zindex processing to work
						if (this.InternalChildren.Count == 0 && generator.CountOfActiveContainers > 0)
							base.AddInternalChild(new FrameworkElement());
					}
					
					break;
				}
				case ItemContainerGenerationMode.Virtualize:
					// Queue a request to cleanup unused generated elements.
					this.TriggerCleanupOfUnusedGeneratedItems(true);
					break;
			}
		}

				#endregion //EndGeneration

				#region InvalidateZOrder

		/// <summary>
		/// Invalidates the Zorder of the elements.
		/// </summary>
		/// <remarks class="note"><b>Note:</b> This method does nothing if the <see cref="ItemContainerGenerationModeResolved"/> is not equal to 'Recycle' or the <see cref="CountOfActiveContainers"/> is less than 2.</remarks>
		protected void InvalidateZOrder()
		{
			if (this.ItemContainerGenerationModeResolved != ItemContainerGenerationMode.Recycle ||
				this.CountOfActiveContainers < 2)
				return;

			this._zOrderDirty = true;
			
			// JJD 6/4/07
			// make sure there is at least one element in the base InternalChildren collection which will allow
			// zindex processing to work
			if (base.InternalChildren.Count == 0)
				base.AddInternalChild(new FrameworkElement());

			if (base.InternalChildren.Count > 0)
			{
				UIElement element = base.InternalChildren[0];
				int zindex = (int)element.GetValue(Panel.ZIndexProperty);
				element.SetValue(Panel.ZIndexProperty, zindex + 1);
			}

		}

				#endregion //InvalidateZOrder	
    
				#region TriggerCleanupOfUnusedGeneratedItems

		/// <summary>
		/// Called to trigger a cleanup of unused generated items.
		/// </summary>
		/// <param name="useDelay">True to cleanup items after a delay, false to cleanup items asynchronously in the background.</param>
		protected void TriggerCleanupOfUnusedGeneratedItems(bool useDelay)
		{
			if (this.IsItemsHost == false)
				return;

            // JJD 3/29/08 - added support for printing.
            // We can't do asynchronous operations during a print
			// JM 03-25-09 TFS 15539 - Should be checking for 'false' rather than 'true'
			//if (Utilities.AllowsAsyncOperations(this))
			if (false == Utilities.AllowsAsyncOperations(this))
			{
				this.OnAsyncCleanUp(null);
                return;
            }


			if (useDelay)
			{
				bool pendingAsyncCleanupAborted = true;
				if (this._pendingAsyncCleanup != null)
				{
					pendingAsyncCleanupAborted = this._pendingAsyncCleanup.Abort();
					if (pendingAsyncCleanupAborted)
						this._pendingAsyncCleanup = null;
				}

                
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

                if (pendingAsyncCleanupAborted)
                {
                    if (this._cleanupDelayTimer == null)
                    {
                        this._cleanupDelayTimer = new DispatcherTimer();
                        this._cleanupDelayTimer.Tick += new EventHandler(this.OnCleanupDelayExpired);
                        this._cleanupDelayTimer.Interval = TimeSpan.FromMilliseconds(500);
                        this._cleanupDelayTimer.Start();

                        
                        // Wire the Unloaded event so we can kill the timer
                        this.Unloaded += new RoutedEventHandler(OnUnloaded);
                    }
                    else
                    {
                        
                        // If we are recycling then restart the timer so that a cleanup doesn't 
                        // happen during multiple scroll operations 
                        if (this._removeDeactivatedContainersOnCleanup &&
                            this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
                        {
                            this._cleanupDelayTimer.Stop();
                            this._cleanupDelayTimer.Start();
                        }
                    }
                }

			}
			else
			{
				// JJD 10/18/10 - TFS30715
				// Only call BeginInvoke if one isn't already pending
				if ( this._pendingAsyncCleanup == null )
					this._pendingAsyncCleanup = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(this.OnAsyncCleanUp), null);
			}
		}

				#endregion //TriggerCleanupOfUnusedGeneratedItems	
    
			#endregion //Protected Methods	

			#region Protected Virtual Methods	

				#region GetCanCleanupItem

		/// <summary>
		/// Derived classes must return whether the item at the specified index can be cleaned up.
		/// </summary>
		/// <param name="itemIndex">The index of the item to be cleaned up.</param>
		/// <returns>True if the item at the specified index can be cleaned up, false if it cannot.</returns>
		protected abstract bool GetCanCleanupItem(int itemIndex);

				#endregion //GetCanCleanupItem

				#region GetChildIndexFromItemIndex

		/// <summary>
		/// Returns the child index associated with the specified item index.
		/// </summary>
		/// <param name="itemIndex">The index of an item in the <see cref="System.Windows.Controls.ItemsControl"/>'s list.</param>
		/// <returns>The index of the child element that contains the specified item.</returns>
		/// <seealso cref="System.Windows.Controls.ItemsControl"/>
		protected int GetChildIndexFromItemIndex(int itemIndex)
		{
			if (this.IsItemsHost != true)
				return itemIndex;

			IItemContainerGenerator generator			= this.ActiveItemContainerGenerator;
			if (generator == null)
				return itemIndex;

			// Must access the internal children collection before using the generator (this is probably a bug in the framework).
			UIElementCollection		children			= base.InternalChildren;
			
			GeneratorPosition		generatorPosition	= generator.GeneratorPositionFromIndex(itemIndex);
			return (generatorPosition.Offset == 0) ? generatorPosition.Index :
													 generatorPosition.Index + 1;
		}

				#endregion //GetChildIndexFromItemIndex

				#region GetItemIndexFromChildIndex

		/// <summary>
		/// Returns the item index associated with the specified child index.
		/// </summary>
		/// <param name="childIndex">The index of the child element that contains the specified item.</param>
		/// <returns>The index of the item in the <see cref="System.Windows.Controls.ItemsControl"/>'s list.</returns>
		/// <seealso cref="System.Windows.Controls.ItemsControl"/>
		protected int GetItemIndexFromChildIndex(int childIndex)
		{
			if (this.IsItemsHost != true)
				return childIndex;

			IItemContainerGenerator generator = this.ActiveItemContainerGenerator;
			if (generator == null)
				return childIndex;

			// Must access the internal children collection before using the generator (this is probably a bug in the framework).
			UIElementCollection children = base.InternalChildren;

			return generator.IndexFromGeneratorPosition(new GeneratorPosition(childIndex, 0));
		}

				#endregion //GetItemIndexFromChildIndex

				#region OnNewElementRealized

		/// <summary>
		/// Called after a newly realized element is generated, added to the children collection and 'prepared'.
		/// </summary>
		/// <param name="element">The newly realized element</param>
		/// <param name="index">The position at which the element was added to the children collection</param>
		protected virtual void OnNewElementRealized(UIElement element, int index)
		{
		}

				#endregion OnNewElementRealized

			#endregion //Protected Virtual Methods	

			#region Private Methods

				#region GetChildElement



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		private Visual GetChildElement(int index)
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

				if (generator != null)
					return generator.GetContainer(index, false) as Visual;
			}

			return base.Children[index];
		}

				#endregion //GetChildElement

				#region OnAsyncCleanUp

		private object OnAsyncCleanUp(object args)
		{
			bool moreCleanupRequired = false;

			try
			{
				moreCleanupRequired = this.CleanupUnusedGeneratedElements();
			}
			finally
			{
				this._pendingAsyncCleanup = null;
			}


			if (moreCleanupRequired)
				this.TriggerCleanupOfUnusedGeneratedItems(true);

			return null;
		}

				#endregion //OnAsyncCleanUp

				#region OnCleanupDelayExpired

		private void OnCleanupDelayExpired(object sender, EventArgs e)
		{
			bool moreCleanupRequired = false;

			try
			{
				moreCleanupRequired = this.CleanupUnusedGeneratedElements();
			}
			finally
			{
				if (moreCleanupRequired == false)
				{
                    
                    // Unwire the Unloaded Event
                    this.Unloaded -= new RoutedEventHandler(OnUnloaded);

                    this._cleanupDelayTimer.Stop();
					this._cleanupDelayTimer = null;
				}
			}
		}

				#endregion //OnCleanupDelayExpired	

				#region OnIsRecyclingStateChanged

		private void OnIsRecyclingStateChanged()
		{
			int count = this.InternalChildren.Count;

			if (count > 0)
				base.RemoveInternalChildRange(0, count);

			if (this._wasRecyclingOnLastGeneration)
			{
				RecyclingItemsControl owner = this.ItemsControl as RecyclingItemsControl;

				if (owner != null)
					owner.RecyclingItemContainerGenerator.RemoveAll();
			}
		}

				#endregion //OnIsRecyclingStateChanged	

				#region OnPostResetVerification

		private object OnPostResetVerification(object args)
		{
			// JJD 6/1/07
			// If the  _pendingPostResetVerification member is non-null it means that 
			// the BeginBeginGeneration/EndGeneration was not callled since the last
			// reset notification which means that we should force a cleanup now
			if (this._pendingPostResetVerification != null)
			{
				// Call BeginGeneration which will null out the _pendingPostResetVerification member
				this.BeginGeneration(ScrollDirection.Increment);

				// Call EndGeneration which will cleanup any containers
				this.EndGeneration();
			}

			return null;
		}

				#endregion //OnPostResetVerification

                
                #region OnUnloaded

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // unwire the event
            this.Unloaded -= new RoutedEventHandler(OnUnloaded);

            // If we have an active cleanup timer stop it and perform the cleanup
            if (this._cleanupDelayTimer != null)
            {
                this._cleanupDelayTimer.Stop();
                this._cleanupDelayTimer = null;
                this.CleanupUnusedGeneratedElements();
            }
        }

            #endregion //OnUnloaded	
    
				#region PerformRecyclingAdd

		private bool PerformRecyclingAdd(UIElement child)
		{
			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
			{
				RecyclingItemContainerGenerator generator = this.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;
				if (generator != null)
				{
                    // JJD 1/22/10 - TFS26513 
                    // only add the the child to the logical tree if it doesn't already have a parent
                    if (LogicalTreeHelper.GetParent(child) == null)
					    this.AddLogicalChild(child);

					this.AddVisualChild(child);
					generator.OnContainerAddedToVisualTree(child);
					this._zOrderDirty = true;

					// JJD 6/4/07
					// make sure there is at least one element in the base InternalChildren collection which will allow
					// zindex processing to work
					if (this.InternalChildren.Count == 0)
						base.AddInternalChild(new FrameworkElement());

					return true;
				}
			}

			return false;
		}

				#endregion //PerformRecyclingAdd	
            
				#region ShouldAbortCleanup

		// JM 03-25-09 TFS 15539 
		//private bool ShouldAbortCleanup(int tickAtStartOfCleanup)
		private AbortAction ShouldAbortCleanup(int tickAtStartOfCleanup)
		{
			if (Environment.TickCount - tickAtStartOfCleanup > MAX_TICKS_IN_CLEANUP)
				// JM 03-25-09 TFS 15539 
				//return true;
				return AbortAction.AbortWithCleanup;

			if (this.IsOkToCleanupUnusedGeneratedElements == false)
				// JM 03-25-09 TFS 15539 
				//return true;
				return AbortAction.AbortWithNoCleanup;

			// JM 03-25-09 TFS 15539 
			//return false;
			return AbortAction.NoAbort;
		}

				#endregion //ShouldAbortCleanup

			#endregion //Private Methods	
        
		#endregion //Methods

		#region ContainerEnumerator private class

		private class ContainerEnumerator : IEnumerator
		{
			#region Private members

			private RecyclingItemsPanel _panel;
			private bool _activeContainersOnly;
			private int _currentPosition;
			private object _currentItem;
			static object UnsetObjectMarker = new object();

			#endregion //Private members	
    
			#region Constructor

			internal ContainerEnumerator(RecyclingItemsPanel panel, bool activeContainersOnly)
			{
				this._panel					= panel;
				this._activeContainersOnly	= activeContainersOnly;
				this._currentPosition		= -1;
				this._currentItem			= UnsetObjectMarker;
			}

			#endregion //Constructor	
    
			#region IEnumerator Members

			public bool MoveNext()
			{
				int count;
				
				if ( this._activeContainersOnly )
					count = this._panel.CountOfActiveContainers;
				else
					count = this._panel.VisualChildrenCount;

                // JJD 1/22/10 - TFS26513
                // keep looking until we find a child we want to return from this
                // enumerator
                //if (this._currentPosition < count - 1)
                while(this._currentPosition < count - 1)
				{
					this._currentPosition++;
					this._currentItem = this._panel.GetVisualChild(this._currentPosition);

                    // JJD 1/22/10 - TFS26513
                    // If _activeContainersOnly is false we are returning logical children.
                    // In that case make sure the visual is a logical child before
                    // returning true. Otherwise, we will just stay in the while loop
                    // t get the next child
                    if (this._activeContainersOnly ||
						// JJD 10/28/10 - TFS26513
						// Check to make sure that GetVisualChild returned a visual
                        //LogicalTreeHelper.GetParent(this._currentItem as Visual) == this._panel )
                        ((this._currentItem is Visual) && LogicalTreeHelper.GetParent(this._currentItem as Visual) == this._panel ))
					    return true;
				}

				this._currentPosition = count;
				this._currentItem = UnsetObjectMarker;
				return false;
			}

			public void Reset()
			{
				this._currentPosition = -1;
				this._currentItem = UnsetObjectMarker;
			}

			object IEnumerator.Current
			{
				get
				{
					if (this._currentItem == UnsetObjectMarker)
					{
						if (this._currentPosition == -1)
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_3"));
						else
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4"));
					}

					return this._currentItem;
				}
			}

			#endregion
		}

		#endregion //ContainerEnumerator private class

		#region RecycleableContainerCollection private class

		private class RecycleableContainerCollection : IList
		{
			#region Private members

			private RecyclingItemsPanel _panel;

			#endregion //Private members

			#region Constructor

			internal RecycleableContainerCollection(RecyclingItemsPanel panel)
			{
				this._panel = panel;
			}

			#endregion //Constructor

			#region IList Members

			int IList.Add(object value)
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));

				return this._panel.InternalChildren.Add(value as UIElement);
			}

			void IList.Clear()
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));
				
				this._panel.InternalChildren.Clear();
			}

			bool IList.Contains(object value)
			{
				return ((IList)this).IndexOf(value) >= 0;
			}

			int IList.IndexOf(object value)
			{
				if (!(value is UIElement))
					return -1;

				if (this._panel._wasRecyclingOnLastGeneration)
				{
					RecyclingItemContainerGenerator generator = this._panel.ActiveItemContainerGenerator as RecyclingItemContainerGenerator;

					Debug.Assert(generator != null);

					if (generator != null)
						return generator.GetIndexOfContainerInActiveList(value as DependencyObject, false);
				}

				return this._panel.InternalChildren.IndexOf(value as UIElement);
			}

			void IList.Insert(int index, object value)
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));
				
				this._panel.InternalChildren.Insert(index, value as UIElement);

			}

			bool IList.IsFixedSize
			{
				get 
				{ 
					return false; 
				}
			}

			bool IList.IsReadOnly
			{
				get 
				{
					return this._panel._wasRecyclingOnLastGeneration;
				}
			}

			void IList.Remove(object value)
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));

				this._panel.InternalChildren.Remove( value as UIElement);
			}

			void IList.RemoveAt(int index)
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));
				
				this._panel.InternalChildren.RemoveAt( index );
			}

			object IList.this[int index]
			{
				get
				{
					if (this._panel._wasRecyclingOnLastGeneration)
					{
						if (index < 0 || index >= this._panel.CountOfActiveContainers)
							throw new IndexOutOfRangeException();

						return this._panel.GetChildElement(index);
					}

					return this._panel.InternalChildren[index];
				}
				set
				{
					if (this._panel._wasRecyclingOnLastGeneration)
						throw new NotSupportedException(SR.GetString("LE_NotSupportedException_11"));
					
					this._panel.InternalChildren[index] = value as UIElement;
				}
			}

			#endregion

			#region ICollection Members

			void ICollection.CopyTo(Array array, int index)
			{
				int count = ((ICollection)this).Count;

				int j = index;
				for (int i = 0; i < count; i++, j++)
				{
					if (j >= array.Length)
						break;

					array.SetValue(((IList)this)[i], j );
				}
			}

			int ICollection.Count
			{
				get { return this._panel.CountOfActiveContainers; }
			}

			bool ICollection.IsSynchronized
			{
				get 
				{
					if (this._panel._wasRecyclingOnLastGeneration)
						return false;

					return this._panel.InternalChildren.IsSynchronized;
				}
			}

			object ICollection.SyncRoot
			{
				get 
				{ 
					if (this._panel._wasRecyclingOnLastGeneration)
						return this; 
					
					return this._panel.InternalChildren.SyncRoot;
				}
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				if (this._panel._wasRecyclingOnLastGeneration)
					return new ContainerEnumerator(this._panel, true);
				
				return this._panel.InternalChildren.GetEnumerator();
			}

			#endregion
		}

		#endregion //RecycleableContainerCollection private class

		// JM 03-25-09 TFS 15539 
		#region AbortAction Private Enumeration

		private enum AbortAction
		{
			NoAbort,
			AbortWithCleanup,
			AbortWithNoCleanup
		}

		#endregion //AbortAction Private Enumeration
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