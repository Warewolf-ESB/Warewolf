//#define VERIFY_MAPS
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using Infragistics.Windows.Selection;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using Infragistics.Shared;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.Virtualization
{
	/// <summary>
	/// An alternative generator used by Panels interested in recycling elements. 
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> Using this generator instead of ItemControl's ItemContainerGenerator will require that the 
	/// <see cref="RecyclingItemsControl"/> derived class implement its own navigation logic. This generator will also not support grouping thru the Items collection.</para>
	/// </remarks>
	/// <seealso cref="RecyclingItemsControl"/>
	/// <seealso cref="RecyclingItemsControl.RecyclingItemContainerGenerator"/>
	/// <seealso cref="RecyclingItemsControl.ItemContainerGenerationMode"/>
	/// <seealso cref="RecyclingItemsPanel"/>
	public sealed class RecyclingItemContainerGenerator : IItemContainerGenerator
	{
		#region Private Members

        // JJD 05/07/10 - TFS31643 
        private DispatcherOperation _resetOperationPending;

		private RecyclingItemsControl _recyclingItemsControl;
		private List<ContainerReference> _deactivatedContainers;
		private List<ContainerReference> _activeContainersInUse;
		private List<ContainerReference> _activeContainersInUseByZorder;
        
        // JJD 3/19/10 - TFS28705 - Optimization 
		
        // _recyclableContainers is only used during a generation and contains all active and deactivated containers
        // It is indexed by item for quick matching
        private Dictionary<object, ContainerReference> _recyclableContainers;
        // JJD 3/19/10 - TFS28705 - Optimization 
        // _recyclableContainersList is only used during a generation and contains both active and deactivated containers
        private List<ContainerReference> _recyclableContainersList;
        // JJD 3/19/10 - TFS28705 - Optimization 
        // _recyclableContainersListFiltered is only used during a generation and contains both active and deactivated containers
        // sequenced by the order they should be recycled excluding any rcds that were expected to be generated
        private List<ContainerReference> _recyclableContainersListFiltered;
        // JJD 3/19/10 - TFS28705 - Optimization 
        // _recyclableContainersDups is only used during a generation and contains container refs with
        // null or duplicate items
        private HashSet _recyclableContainersDups;

		private SparseArray _indexMap;

		private Dictionary<object, ContainerReference> _referencesBeforeLastReset;

        // JJD 11/04/08 - TFS7939
        // Create a list for the referencesBeforeLastReset so we can index into them 
        // from GetContainer so they remain in the visual tree
        private List<ContainerReference> _referencesBeforeLastResetList;

		private RecyclingGenerator _currentGenerator;
		private GeneratorStatus _status = GeneratorStatus.NotStarted;
		private ScrollDirection _scrollDirection;
		private bool _isGenerating;
		private bool _isZOrderDirty;

		// JM 10-21-10 TFS57364
		private bool		_isInRemoveAll;

		// MD 3/16/11 - TFS24163
		private List<ContainerReference> _deactivatedContainersNotInIndexMap;

		#endregion //Private Members	
    
		#region Constructors



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal RecyclingItemContainerGenerator(RecyclingItemsControl recyclingItemsControl)
		{
			if (recyclingItemsControl == null)
                throw new ArgumentNullException("recyclingItemsControl", SR.GetString("LE_RecyclingGenerator_1"));

			this._recyclingItemsControl = recyclingItemsControl;
			this._deactivatedContainers = new List<ContainerReference>();
			this._activeContainersInUse = new List<ContainerReference>();
            
            
            

			this.InitializeIndexMap();

			// wire up the CollectionChanged event so we can keep the map in sync
			// with the Items collection
			((INotifyCollectionChanged)(this.Items)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemCollectionChanged);

		}

		#endregion //Constructors

		#region Event Handlers

			#region OnItemCollectionChanged

		// used to keep index map in sync with the Items collection
		private void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				#region Reset
    
    				case NotifyCollectionChangedAction.Reset:

						// cache the existing refernces in a Dictionary keyed by the Item
                        if (this._referencesBeforeLastReset == null)
                            this._referencesBeforeLastReset = new Dictionary<object, ContainerReference>();

                        foreach (ContainerReference cr in this._indexMap.NonNullItems)
                        {
                            // JJD 6/2/08 - BR33524
                            // Check for null item and use indexer set instead
                            // of calling the Add method. This is safer because the
                            // indexer set will add items that don't exist and
                            // overlay items that do. The add method on the other
                            // hand will throw an ArgumentException when trying
                            // to add an item will a key that already exists in the
                            // dictionary.
                            //this._referencesBeforeLastReset.Add(cr.Item, cr);
                            object item = cr.Item;
                            if (item != null)
                                this._referencesBeforeLastReset[item] = cr;
                        }

						// MD 3/16/11 - TFS24163
						// We were only adding in items from the index map before. But we now also have deactivated containers
						// which are not in the index map, we need to add them in as well.
						if (_deactivatedContainersNotInIndexMap != null)
						{
							foreach (ContainerReference cr in _deactivatedContainersNotInIndexMap)
							{
								object item = cr.Item;
								if (item != null)
									this._referencesBeforeLastReset[item] = cr;
							}

							_deactivatedContainersNotInIndexMap = null;
						}

						this._indexMap.Clear();

						this._indexMap.Expand(this.Items.Count);

                        
                        
                        
						this._activeContainersInUse.Clear();
                        this._deactivatedContainers.Clear();

						if (this._activeContainersInUseByZorder != null)
							this._activeContainersInUseByZorder.Clear();

                        // JJD 05/07/10 - TFS31643 
                        // In case we never get called on the next measure pass (e.g. we are unloaded)
                        // call BeginInvoke so we can preform the appropriate cleanup operation
                        if (this._resetOperationPending == null && this._recyclingItemsControl != null)
                            this._resetOperationPending = this._recyclingItemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Input, new PropertyValueTracker.MethodInvoker(this.ProcessPendingReset));

						break;

   				#endregion //Reset	
    
				#region Add
    
    				case NotifyCollectionChangedAction.Add:
					{
						// insert that number of slots into the sparse array map
						object[] nullArray = new object[e.NewItems.Count];

						// JJD 8/1/07 - BR25221
						// Make sure the index map has enough slots allocated to handle the additions
						
						
						
						
						
						
						
						



						if ( e.NewStartingIndex > this._indexMap.Count )
							this._indexMap.Expand( e.NewStartingIndex );
						

						this._indexMap.InsertRange(e.NewStartingIndex, nullArray);

                        //Debug.Assert(this._indexMap.Count == this.Items.Count, "Counts don't match on add in RecyclingItemContainerGenerator.OnItemCollectionChanged");
					}
					break;

   				#endregion //Add	
    
				#region Replace

				case NotifyCollectionChangedAction.Replace:
					{
						// insert that number of slots into the sparse array map
						int endIndex = e.NewStartingIndex + e.NewItems.Count - 1;

						// loop over the affected slots and cache any ContainerReferences into the
						// _connectedContainersInLimbo cache
						for (int i = e.NewStartingIndex; i <= endIndex; i++)
						{
							ContainerReference cr = this._indexMap[i] as ContainerReference;

							if (cr != null)
							{
								// clear the ContainerReference
								this.ClearContainerReference(cr, false, true, true);

								// null out the map slot
								this._indexMap[i] = null;
							}
						}

                        //Debug.Assert(this._indexMap.Count == this.Items.Count, "Counts don't match on replace in RecyclingItemContainerGenerator.OnItemCollectionChanged");
                    }
					break;

				#endregion //Replace	
    
				#region Remove

				case NotifyCollectionChangedAction.Remove:
					{
						int startIndex;
						IList list;

						if (e.NewItems != null)
						{
							startIndex = e.NewStartingIndex;
							list = e.NewItems;
						}
						else
						{
							startIndex = e.OldStartingIndex;
							list = e.OldItems;
						}

						int endIndex = startIndex + list.Count - 1;

						// SSP 3/5/09 TFS5842
						// If the _indexMap data structure is out of sync for some reason then process the
						// notification as reset.
						// 
						if ( endIndex >= _indexMap.Count )
						{
							this.OnItemCollectionChanged( sender, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
							return;
						}

						// loop over the affected slots and cache any ContainerReferences into the
						// _connectedContainersInLimbo cache
						for (int i = startIndex; i <= endIndex; i++)
						{
							ContainerReference cr = this._indexMap[i] as ContainerReference;

							if (cr != null)
							{
								// clear the ContainerReference
								this.ClearContainerReference(cr, false, true, true);
							}
						}

						// remove the range from the index map
						this._indexMap.RemoveRange(startIndex, list.Count);

                        //Debug.Assert(this._indexMap.Count == this.Items.Count, "Counts don't match on remove in RecyclingItemContainerGenerator.OnItemCollectionChanged");
                    }
					break;

				#endregion //Remove	
    
				#region Move

				case NotifyCollectionChangedAction.Move:
					{
						IList list;

						if (e.NewItems != null)
							list = e.NewItems;
						else
							list = e.OldItems;

						// allocate an array for each item moved
						object[] slotArray = new object[list.Count];

						// loop over the old index slots and cache any ContainerReferences into the
						// slotArray allocated above
						for (int i = 0; i < slotArray.Length; i++)
						{
							ContainerReference cr = this._indexMap[i + e.OldStartingIndex] as ContainerReference;

							if (cr != null)
								slotArray[i] = cr;
						}

                        //Debug.Assert(this._indexMap.Count == this.Items.Count, "Counts don't match on move in RecyclingItemContainerGenerator.OnItemCollectionChanged");

						// remove the old range from the index map
						this._indexMap.RemoveRange(e.OldStartingIndex, list.Count);

						// insert the cached removed slots into the new index
						this._indexMap.InsertRange(e.NewStartingIndex, slotArray);

                        //Debug.Assert(this._indexMap.Count == this.Items.Count, "Counts don't match on move in RecyclingItemContainerGenerator.OnItemCollectionChanged");
                    }
					break;

				#endregion //Move
			}




        }

			#endregion //OnItemCollectionChanged	

			#region OnZorderChanged

		
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

		private void OnZorderChanged()
		{
			// set our dirty flag
			this._isZOrderDirty = true;

			if (this.ZOrderChanged != null)
				this.ZOrderChanged(this, EventArgs.Empty);
		}
			#endregion //OnZorderChanged	
        
		#endregion //Event Handlers	

		#region Events
    
			#region ContainerDiscarded

		/// <summary>
		/// Occurs when a container has been discarded.
		/// </summary>
		public event EventHandler<ContainerDiscardedEventArgs> ContainerDiscarded;

			#endregion //ContainerDiscarded	

			#region StatusChanged

		/// <summary>
		/// Occurs when the Status has changed.
		/// </summary>
		/// <seealso cref="Status"/>
		public event EventHandler StatusChanged;

			#endregion //StatusChanged	
    
			#region ZOrderChanged

		internal event EventHandler ZOrderChanged;

			#endregion //ZOrderChanged	
    
		#endregion //Events	
    
		#region Properties

			#region Public Properties

				#region CountOfAllContainers

		/// <summary>
		/// Returns the total number of containers, both active and deactived (read-only).
		/// </summary>
		/// <seealso cref="CountOfActiveContainers"/>
		/// <seealso cref="GetContainer"/>
        // JJD 11/04/08 - TFS7939
        // Add in the referencesBeforeLastReset so they remain in the visual tree
		//public int CountOfAllContainers { get { return this.CountOfActiveContainers + this._deactivatedContainers.Count + this._activeContainersInLimbo.Count; } }
		public int CountOfAllContainers 
        { 
            get 
            { 
                //int count =  this.CountOfActiveContainers 
                //            + this._deactivatedContainers.Count 
                //            + this._activeContainersInLimbo.Count;
                int count = this.CountOfActiveContainers;

                // JJD 3/19/10 - TFS28705 - Optimization 
                // If we are in the mifddle of a generation then add in those the cached containers
                if (this._recyclableContainersList != null)
                    count += this._recyclableContainersList.Count;
                else
                {
                    count += this._deactivatedContainers.Count;

                    // JJD 11/04/08 - TFS7939
                    // Add in the referencesBeforeLastReset so they remain in the visual tree
                    if (this._referencesBeforeLastReset != null)
                        count += this._referencesBeforeLastReset.Count;
                }

                return count;
            } 
        }

				#endregion //CountOfAllContainers	

				#region CountOfActiveContainers

		/// <summary>
		/// Returns the number of active containers (read-only)..
		/// </summary>
		/// <seealso cref="CountOfAllContainers"/>
		/// <seealso cref="GetContainer"/>
		public int CountOfActiveContainers 
		{ 
			get 
			{ 
				int count = this._activeContainersInUse.Count;

				if (this._currentGenerator != null)
					count += this._currentGenerator.CountOfGeneratedItems;

				return count;
			} 
		}

				#endregion //CountOfActiveContainers	
    
				#region Status

		/// <summary>
		/// Returns the current status of the generator (read-only).
		/// </summary>
		/// <seealso cref="StatusChanged"/>
		public GeneratorStatus Status { get { return this._status; } }

				#endregion //Status

			#endregion //Public Properties	
        
			#region Private Properties

				#region ActiveContainersInUseByZorder

		private List<ContainerReference> ActiveContainersInUseByZorder
		{
			get
			{
				if (this._activeContainersInUseByZorder == null)
					this._activeContainersInUseByZorder = new List<ContainerReference>();
				else
				{
					if (this._isZOrderDirty || this._activeContainersInUse.Count != this._activeContainersInUseByZorder.Count)
						this._activeContainersInUseByZorder.Clear();
				}

				if (this._activeContainersInUseByZorder.Count == 0)
				{
					this._isZOrderDirty = false;

					int count = this._activeContainersInUse.Count;

					if (count > 0)
					{
						if (count == 1)
							this._activeContainersInUseByZorder.Add(this._activeContainersInUse[0]);
						else
						{
							// initialize the original index of each entry to use inside the sort comparer
							for (int i = 0; i < count; i++)
								this._activeContainersInUse[i]._originalIndex = i;

							ContainerReference[] tempArray = new ContainerReference[count];
							this._activeContainersInUse.CopyTo(tempArray);

							// sort the entries by zorder
							Utilities.SortMerge(tempArray, new ZOrderComparer());

							this._activeContainersInUseByZorder.AddRange(tempArray);
						}
					}
				}

				return this._activeContainersInUseByZorder;
			}
		}

				#endregion //ActiveContainersInUseByZorder	

				#region ContainerReference

		private static readonly DependencyProperty ContainerReferenceProperty = DependencyProperty.RegisterAttached("ContainerReference",
			typeof(ContainerReference), typeof(RecyclingItemContainerGenerator), new FrameworkPropertyMetadata(null));

		private static ContainerReference GetContainerReference(DependencyObject d)
		{
			return (ContainerReference)d.GetValue(RecyclingItemContainerGenerator.ContainerReferenceProperty);
		}

		
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


				#endregion //ContainerReference

				// JJD 8/24/11 - TFS84215 - added
				#region IsStyleSetFromItemContainerStyle

		private static readonly DependencyProperty IsStyleSetFromItemContainerStyleProperty = DependencyProperty.RegisterAttached("IsStyleSetFromItemContainerStyle",
			typeof(bool), typeof(RecyclingItemContainerGenerator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

				#endregion //IsStyleSetFromItemContainerStyle

				#region Items

		private ItemCollection Items { get { return ((ItemsControl)this._recyclingItemsControl).Items; } }

				#endregion //Items	
    
			#endregion //Private Properties	
    
			#region Internal Properties

				// JM 10-21-10 TFS57364 - Added
				#region IsInRemoveAll
		internal bool IsInRemoveAll
		{
			get { return this._isInRemoveAll; }
		}
				#endregion //IsInRemoveAll

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region BeginGeneration

		/// <summary>
		/// Called at the beginning of the generation process.
		/// </summary>
		/// <remarks>
		/// <para class="body">This method is usually called at the beginnning of a <see cref="RecyclingItemsPanel"/> derived class's <b>MeasureOverride</b> logic. 
		/// This is normally followed by one or more <see cref="StartAt(GeneratorPosition, GeneratorDirection, bool)"/>/<see cref="GenerateNext(out bool)"/> loops to generate all 'active' containers. Finally, the <see cref="EndGeneration"/> method is called at the end of the measure logic.</para>
		/// </remarks>
		/// <param name="scrollDirection">Relative to the last generation to let the generator optimize the recycling logic.</param>
		/// <seealso cref="EndGeneration"/>
        public void BeginGeneration(ScrollDirection scrollDirection)
        {
            // JJD 3/10/10 - TFS28705 - Optimization
            // Call new overload  
            this.BeginGeneration(scrollDirection, null);
        }
		/// <summary>
		/// Called at the beginning of the generation process.
		/// </summary>
		/// <remarks>
		/// <para class="body">This method is usually called at the beginnning of a <see cref="RecyclingItemsPanel"/> derived class's <b>MeasureOverride</b> logic. 
		/// This is normally followed by one or more <see cref="StartAt(GeneratorPosition, GeneratorDirection, bool)"/>/<see cref="GenerateNext(out bool)"/> loops to generate all 'active' containers. Finally, the <see cref="EndGeneration"/> method is called at the end of the measure logic.</para>
		/// </remarks>
		/// <param name="scrollDirection">Relative to the last generation to let the generator optimize the recycling logic.</param>
        /// <param name="itemsExpectedToBeGenerated">An optional list of items are are expected to be generated. This is used to optimize eleme nt recycling.</param>
		/// <seealso cref="EndGeneration"/>
        // JJD 3/10/10 - TFS28705 - Optimization
        // Added new overload that takes itemsExpectedToBeGenerated param
		public void BeginGeneration(ScrollDirection scrollDirection, IEnumerable itemsExpectedToBeGenerated)
		{
			// JM 10-08-08 TFS6272 - In cases where our code has called BeginGeneration and then an exception occurs in the user's code, 
			//						 it is possible for us to get another BeginGeneration call before EndGeneation is called.  I have put
			//						 an Assert here which should never be hit in our testing, in case we mistakenly call BeginGeneration
			//						 without calling EndGeneration during development.
			if (this._isGenerating == true)
			{
				Debug.Assert(false, "Our code should never cause us to hit this assert!");
				this.EndGeneration();
			}

			if (this._isGenerating == true)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_2"));

            // JJD 05/07/10 - TFS31643 
            // Abort the _resetOperationPending since BeginGeneration has been called  
            // and we will perform the normal cleanup in EndGeneration
            if (this._resetOperationPending != null)
            {
                this._resetOperationPending.Abort();
                this._resetOperationPending = null;
            }

            this._scrollDirection = scrollDirection;






			// Move all the connected containers for the 'inuse' to the 'limbo' cache
            // JJD 3/10/10 - TFS28705 - Optimization
            // Added itemsExpectedToBeGenerated parameter
			this.PlaceAllActiveContainersInLimbo(itemsExpectedToBeGenerated);

			this._isGenerating = true;




        }

				#endregion //BeginGeneration	

				#region ContainerFromIndex

		/// <summary>
		/// Returns the element corresponding to the item at the given index within the ItemCollection.
		/// </summary>
		/// <param name="index">The index of the desired item.</param>
		/// <returns>Returns the element corresponding to the item at the given index within the ItemCollection or returns null if the item is not realized.</returns>
		public DependencyObject ContainerFromIndex(int index)
		{
			if (index < 0 || index >= this._indexMap.Count)
				return null;

			ContainerReference cr = this._indexMap[index] as ContainerReference;

			// AS 6/5/07
			//if (cr.IsDeactivated)
			if (null == cr || cr.IsDeactivated)
				return null;

			return cr.Container;
		}

				#endregion //ContainerFromIndex	

				#region ContainerFromItem

		/// <summary>
		/// Returns the UIElement container corresponding to the given item.
		/// </summary>
		/// <param name="item">The Object item to find the UIElement container for.</param>
		/// <returns>A UIElement that corresponds to the given item. Returns null if the item does not belong to the item collection, or if an UIElement has not been generated for it.</returns>
		public DependencyObject ContainerFromItem(object item)
		{
			// AS 6/21/11 TFS79160
			// If we got a reset then the _indexMap will be clear until the next generation but 
			// in the interim should someone ask for the container for an item then we should 
			// look at the cache of containerreferences we had before the reset.
			//
			//return this.ContainerFromIndex( this._recyclingItemsControl.Items.IndexOf(item) );
			var d = this.ContainerFromIndex(this._recyclingItemsControl.Items.IndexOf(item));

			if (null == d && _referencesBeforeLastReset != null)
			{
				ContainerReference cr;

				if (_referencesBeforeLastReset.TryGetValue(item, out cr))
					d = cr.Container;
			}

			return d;
		}

				#endregion //ContainerFromItem	
    
				#region DeactivateAllContainers
		
		/// <summary>
		/// Deactivates all containers.
		/// </summary>
		/// <remarks>
		/// <para class="body"> This will result in the <see cref="RecyclingItemsControl"/>'s <see cref="RecyclingItemsControl.DeactivateContainer"/>virtual method being called for each active container.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecyclingItemsControl.DeactivateContainer"/>
		public void DeactivateAllContainers()
		{
			this.DeactivateAllContainersInUse();
			this.DeactivateAllContainersInLimbo();
		}

				#endregion //DeactivateAllContainers	

				#region DeactivateContainer

		/// <summary>
		/// Deactivates a specific container
		/// </summary>
		/// <param name="container">The container to be deactivated</param>
		/// <remarks>
		/// <para class="body"> This will result in the <see cref="RecyclingItemsControl"/>'s <see cref="RecyclingItemsControl.DeactivateContainer"/> virtual method being called.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecyclingItemsControl.DeactivateContainer"/>
        public void DeactivateContainer(DependencyObject container)
        {
            this.DeactivateContainerHelper(container, true);

        }

        private void DeactivateContainerHelper(DependencyObject container, bool removeFromRecyclableCache)
		{
			ContainerReference cr = GetContainerReference(container);

			if (cr == null)
				return;

			this.DeactivateContainerHelper(cr);

			int indexInCache = this._activeContainersInUse.IndexOf(cr);

            if (indexInCache >= 0)
                this._activeContainersInUse.RemoveAt(indexInCache);
            else
            {
                // JJD 3/19/10 - TFS28705 - Optimization
                // Check and remove from recyclable cache
                //this.RemoveContainerFromCache(cr, this._activeContainersInLimbo);
                if ( removeFromRecyclableCache )
                    this.RemoveContainerFromRecyclableCache(cr);
            }
            
            // JJD 7/31/09 - TFS18162
            // Also maintain the activeContainersInUseByZorder collection
            if (this._activeContainersInUseByZorder != null)
            {
		        indexInCache = this._activeContainersInUseByZorder.IndexOf(cr);

                if (indexInCache >= 0)
                    this._activeContainersInUseByZorder.RemoveAt(indexInCache);
            }
		}

		private void DeactivateContainerHelper(ContainerReference cr)
		{
			cr.IsDeactivated = true;

			this._recyclingItemsControl.DeactivateContainer(cr.Container, cr.Item);

			this._deactivatedContainers.Add(cr);

		}

				#endregion //DeactivateContainer	
    
				#region EndGeneration

		/// <summary>
		/// Called at the end of the generation process.
		/// </summary>
		/// <seealso cref="BeginGeneration(ScrollDirection, IEnumerable)"/>
		public void EndGeneration()
		{
			this._isGenerating = false;



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			if (this._activeContainersInUseByZorder != null)
				this._activeContainersInUseByZorder.Clear();

			// Try to deactivate all containers in the 'limbo' cache
			this.DeactivateAllContainersInLimbo();

            #region Old code

            
#region Infragistics Source Cleanup (Region)














































#endregion // Infragistics Source Cleanup (Region)


            #endregion //Old code	
    
            // JJD 3/19/10 - TFS28705 - Optimization 
            // Clear out the cached lists we used during the generation cycle
            this._referencesBeforeLastReset         = null;
            this._referencesBeforeLastResetList     = null;
            this._recyclableContainers              = null;
            this._recyclableContainersList          = null;
            this._recyclableContainersListFiltered  = null;
            this._recyclableContainersDups          = null;

			this._indexMap.Compact();





        }

				#endregion //EndGeneration	

				#region IndexFromContainer

		/// <summary>
		/// Returns the index to an item that corresponds to the specified, generated UIElement. 
		/// </summary>
		/// <param name="container">The DependencyObject that corresponds to the item to the index to be returned.</param>
		/// <returns>An Int32 index to an item that corresponds to the specified, generated UIElement.</returns>
		/// <exception cref="ArgumentNullException">container is null</exception>
		public int IndexFromContainer(DependencyObject container)
		{
			if (container == null)
				throw new ArgumentNullException("container");

			ContainerReference cr = GetContainerReference(container);

			if (cr == null || cr.IsDeactivated)
				return -1;

			return this._indexMap.IndexOf(cr);
		}

				#endregion //IndexFromContainer	

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

			ContainerReference cr = GetContainerReference(container);

			if (cr == null || cr.IsDeactivated)
				return DependencyProperty.UnsetValue;

			int index = this._indexMap.IndexOf(cr);

			if ( index < 0 )
				return DependencyProperty.UnsetValue;

			return cr.Item;
		}

				#endregion //ItemFromContainer	
    
				#region RemoveAllDeactivatedContainers

		/// <summary>
		/// Removes all generated containers that have previously been deactivated.
		/// </summary>
		public void RemoveAllDeactivatedContainers()
		{
			// MD 3/16/11 - TFS24163
			// Clear this list. We don't have to do any cleanup work here because all items from the 
			// _deactivatedContainersNotInIndexMap collection are also in the _deactivatedContainers
			// collection, so we will clean up those items below.
			_deactivatedContainersNotInIndexMap = null;

			int i = this._deactivatedContainers.Count - 1;
			
			// Loop over the _deactivatedContainers backwards to remove any deactivated containers
			while (i >= 0 )
			{
				ContainerReference cr = this._deactivatedContainers[i];

				this._deactivatedContainers.RemoveAt(i);

                // JJD 3/18/10 - TFS28705 
                // Pass true in for both params to make sure we remove it from the cahes as well
				//this.ClearContainerReference(cr, true, false);
				this.ClearContainerReference(cr, true, true, true);

				// set the index to the last item in the list
				// this is a precaution in case the clear triggers some other
				// user code that could impact this collection
				i = this._deactivatedContainers.Count - 1;
			}
		}

				#endregion //RemoveAllDeactivatedContainers	
    
				// JJD 4/09/12 - TFS108549 - Added
				#region VerifyAllDeactivatedContainers

		/// <summary>
		/// Removes any generated containers that have previously been deactivated but are no longer valid.
		/// </summary>
		public void VerifyAllDeactivatedContainers()
		{
			if (_deactivatedContainers.Count < 1)
				return;

			List<ContainerReference> refsToRemove = null;

			foreach (ContainerReference cr in _deactivatedContainers)
			{
				bool? isStillValid = _recyclingItemsControl.IsStillValid(cr.Container, cr.Item);

				if (isStillValid.HasValue && false == isStillValid.Value)
				{
					if (refsToRemove == null)
						refsToRemove = new List<ContainerReference>();

					refsToRemove.Add(cr);
				}
			}

			if (refsToRemove == null)
				return;

			if (refsToRemove.Count == _deactivatedContainers.Count)
			{
				// Clear this list. We don't have to do any cleanup work here because all items from the 
				// _deactivatedContainersNotInIndexMap collection are also in the _deactivatedContainers
				// collection, so we will clean up those items below.
				_deactivatedContainersNotInIndexMap = null;
			}
			
			// Loop over refs to remove and remove them
			foreach (ContainerReference cr in refsToRemove)
			{
				this._deactivatedContainers.Remove(cr);

				if (_deactivatedContainersNotInIndexMap != null)
					_deactivatedContainersNotInIndexMap.Remove(cr);

                // Pass true in for all 3 trailing params to make sure we remove it from all the caches as well
				this.ClearContainerReference(cr, true, true, true);
			}
		}

				#endregion //VerifyAllDeactivatedContainers	
    
   			#endregion //Public Methods	
    
			#region Internal Methods

				#region GetContainer



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal DependencyObject GetContainer(int index, bool isIndexBasedOnZOrder)
		{
			ContainerReference cr = this.GetContainerReference(index, isIndexBasedOnZOrder);

			if (cr != null)
				return cr.Container;

			return null;

		}

		private ContainerReference GetContainerReference(int index, bool isIndexBasedOnZOrder)
		{
			int count = this.CountOfActiveContainers;

			if (index < count)
			{
				if (this._currentGenerator != null)
					return this._currentGenerator.GetActiveContainerReference(index, isIndexBasedOnZOrder);

				if ( isIndexBasedOnZOrder )
					return this.ActiveContainersInUseByZorder[index];
				else
					return this._activeContainersInUse[index];
			}

			index -= count;

			count = this._deactivatedContainers.Count;
			
			if (index < count)
				return this._deactivatedContainers[index];

			index -= count;

            // JJD 3/19/10 - TFS28705 - Optimization
            // If we have a _recyclableContainersList then index into that
            if (this._recyclableContainersList != null)
            {
                //count = this._activeContainersInLimbo.Count;
                count = this._recyclableContainersList.Count;

                if (index < count)
                    return this._recyclableContainersList[index];
            }
            else
            {
                // JJD 11/04/08 - TFS7939
                // Index into the referencesBeforeLastReset so they remain in the visual tree
                
                
                if (this._referencesBeforeLastReset != null)
                {
                    int lastRefCount = this._referencesBeforeLastReset.Count;

                    if (index < lastRefCount)
                    {
                        // JJD 11/04/08 - TFS7939
                        // Since we can't index into a dictionary we need to create and cache a list
                        // from the dictionaries Values collection
                        if (this._referencesBeforeLastResetList == null ||
                             this._referencesBeforeLastResetList.Count != lastRefCount)
                            this._referencesBeforeLastResetList = new List<ContainerReference>(this._referencesBeforeLastReset.Values);

                        return this._referencesBeforeLastResetList[index];
                    }
                }
                
            }

			return null;

		}

		private ContainerReference GetContainerReferenceHelper(int index, LinkedList<ContainerReference> llist)
		{
			LinkedListNode<ContainerReference> node = llist.First;

			while (index > 0)
			{
				node = node.Next;
				index--;
			}

			return node.Value;
		}

				#endregion //GetContainer	

				#region GetIndexInActiveList

		internal int GetIndexOfContainerInActiveList(DependencyObject container, bool isIndexBasedOnZOrder)
		{
			ContainerReference cr = GetContainerReference(container);

			if (cr != null)
				return -1;

			if (this._currentGenerator != null)
				return this._currentGenerator.GetIndexOfActiveContainerReference(cr, isIndexBasedOnZOrder);

			if (isIndexBasedOnZOrder)
				return this.ActiveContainersInUseByZorder.IndexOf(cr);
			else
				return this._activeContainersInUse.IndexOf(cr);

		}

				#endregion //GetIndexInActiveList	
        
				#region OnContainerAddedToVisualTree

		internal void OnContainerAddedToVisualTree(DependencyObject container)
		{
			if (this._currentGenerator == null)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_3"));

			ContainerReference cr = GetContainerReference(container);

			if (cr == null)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_4"));

			this._currentGenerator.OnContainerAddedToVisualTree(cr);
		}

				#endregion //OnContainerAddedToVisualTree	
    
   			#endregion //Internal Methods	
    
			#region Private Methods

				#region ClearContainerReference

		// JJD 6/5/07 - added removeFromCache flag
		private void ClearContainerReference(ContainerReference cr, bool removeFromMap, bool removeFromCache, bool removeFromRecyclingCache)
		{
			int index;

			if (removeFromMap)
			{
				index = this._indexMap.IndexOf(cr);

				if (index >= 0)
					this._indexMap[index] = null;
			}

			// JJD 6/5/07 - added removeFromCache flag
			if (removeFromCache)
			{
				index = this._activeContainersInUse.IndexOf(cr);

				if (index >= 0)
					this._activeContainersInUse.RemoveAt(index);
				else
				{
					index = this._deactivatedContainers.IndexOf(cr);

					if (index >= 0)
						this._deactivatedContainers.RemoveAt(index);

					// MD 3/16/11 - TFS24163
					// Also remove the container from the _deactivatedContainersNotInIndexMap collection.
					if (_deactivatedContainersNotInIndexMap != null)
					{
						index = _deactivatedContainersNotInIndexMap.IndexOf(cr);
						if (index >= 0)
							_deactivatedContainersNotInIndexMap.RemoveAt(index);
					}

                    // JJD 3/19/10 - TFS28705 - Optimization
                    // Remove from Recyclable Cache
                    
                    
                    
                    if (removeFromRecyclingCache)
                    {
                        this.RemoveContainerFromRecyclableCache(cr);
                    }
				}

                // JJD 7/31/09 - TFS18162
                // Also maintain the activeContainersInUseByZorder collection
                if (this._activeContainersInUseByZorder != null)
                {
                    index = this._activeContainersInUseByZorder.IndexOf(cr);

                    if (index >= 0)
                        this._activeContainersInUseByZorder.RemoveAt(index);
                }
            }

			// AS 6/1/07
			cr.Generator = null;

			cr.Container.ClearValue(ContainerReferenceProperty);

			// JM/JD 09-26-07
			//cr.Container.ClearValue(FrameworkElement.DataContextProperty);

            // JJD 1/22/10 - TFS26513
            // Check IsItemItsOwnContainer property
            if (!cr.IsItemItsOwnContainer)
                cr.Container.SetValue(FrameworkElement.DataContextProperty, null);


            this._recyclingItemsControl.ClearContainerForItemInternal(cr.Container, cr.Item);

            // unwire the zoder changed event
            //DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(Panel.ZIndexProperty, cr.Container.GetType());
            //dpd.RemoveValueChanged(cr.Container, new EventHandler(this.OnZorderChanged));

            if (this.ContainerDiscarded != null)
                this.ContainerDiscarded(this, new ContainerDiscardedEventArgs(cr.Container));

		}

				#endregion //ClearContainerReference

				#region CreateNewContainerReference

		private ContainerReference CreateNewContainerReference(object item)
		{
			
			
			
			
			
			
			
			
			

            // JJD 1/22/10 - TFS26513
            // See if item is its own container
			//ContainerReference cr = new ContainerReference( this._recyclingItemsControl.GetContainerForItemInternal( item ) );
            bool isItemItsOwnContainer = this._recyclingItemsControl.IsItemItsOwnContainerInternal(item);
            DependencyObject container;

            if (isItemItsOwnContainer)
                container = item as DependencyObject;
            else
                container = this._recyclingItemsControl.GetContainerForItemInternal( item );

			ContainerReference cr = new ContainerReference( container, isItemItsOwnContainer );

            // JJD 1/22/10 - TFS26513
            if (!isItemItsOwnContainer)
            {
                cr.Item = item;
            }

			this.ConnectContainerToItem(cr);

			// wire up the zorder changed venet handler
			//DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(Panel.ZIndexProperty, cr.Container.GetType());
			//dpd.AddValueChanged(cr.Container, new EventHandler(this.OnZorderChanged));

			return cr;
		}

				#endregion //CreateNewContainerReference	

				#region ConnectContainerToItem

		private void ConnectContainerToItem(ContainerReference cr)
		{
			DependencyObject container = cr.Container;

			container.SetValue(ContainerReferenceProperty, cr);

			// AS 6/1/07
			cr.Generator = this;

            // JJD 1/22/10 - TFS26513
            // Check IsItemItsOwnContainer property
            
            if (!cr.IsItemItsOwnContainer)
				container.SetValue(FrameworkElement.DataContextProperty, cr.Item);
		}

				#endregion //ConnectContainerToItem	

				#region DeactivateContainerFromItem

		
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


				#endregion //DeactivateContainerFromItem	

				#region DeactivateAllContainersInLimbo

        private void DeactivateAllContainersInLimbo()
        {
            
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


            if (this._recyclableContainersList == null )
                return;

            int count = this._recyclableContainersList.Count;
            
            if ( count < 1)
                return;

            int itemCount = this._recyclingItemsControl.Items.Count;

            // clear any unused references 
            for (int i = 0; i < count; i++)
            {
                ContainerReference cr = this._recyclableContainersList[i];
                
                this.DeactivateLimboedContainer(itemCount, cr);
            }
        }

				#endregion //DeactivateAllContainersInLimbo	

                // JJD 3/19/10 - TFS28705 - Optimization - added
                #region DeactivateLimboedContainer

        private void DeactivateLimboedContainer(int itemCount, ContainerReference cr)
        {
            if (this._referencesBeforeLastReset == null)
            {
                if ( cr.IsDeactivated == false )
                    this.DeactivateContainerHelper(cr);

                Debug.Assert(cr.IsDeactivated == true, "Container should be deactivated");
                Debug.Assert(this._deactivatedContainers.Contains(cr), "Container should be in deactivated containers list");
            }
            else
            {
                // Instead of clearing the container just deactivate it if
                // ShouldDeactivateContainer returns true
                if (itemCount > 0 &&
                    this.HasContainerBeenCleared(cr) == false &&
                    this._recyclingItemsControl.ShouldDeactivateContainer(cr.Container, cr.Item))
                {
                    bool isIndexInMap = this._indexMap.Contains(cr);

                    // JJD 3/17/10 - TFS28705 - Optimization 
                    //see if it is in the map
                    if (!isIndexInMap)
                    {
                        int index = this._recyclingItemsControl.Items.IndexOf(cr.Item);

                        // if the map entry is empty then update the map
                        if (index >= 0 && this._indexMap[index] == null)
                        {
                            this._indexMap[index] = cr;
                            isIndexInMap = true;
                        }
                    }

					// MD 3/16/11 - TFS24163
					// If the item is not in the index map, we can still deactivate it, but only if we haven't hit our limit of
					// deactivated containers without indexes.
					//if (isIndexInMap)
					//    this.DeactivateContainerHelper(cr);
					//else
					//    this.ClearContainerReference(cr, true, true, false);
					bool canBeDeactivated = isIndexInMap;
					if (isIndexInMap == false)
					{
						int maxDeactivatedContainersWithoutIndexes = _recyclingItemsControl.MaxDeactivatedContainersWithoutIndexes;

						if (0 < maxDeactivatedContainersWithoutIndexes)
						{
							if (_deactivatedContainersNotInIndexMap == null)
								_deactivatedContainersNotInIndexMap = new List<ContainerReference>();

							if (_deactivatedContainersNotInIndexMap.Count < maxDeactivatedContainersWithoutIndexes)
							{
								Debug.Assert(_deactivatedContainersNotInIndexMap.Contains(cr) == false, "The deactivated container should not be in the collection already.");
								_deactivatedContainersNotInIndexMap.Add(cr);
								canBeDeactivated = true;
							}
						}
					}

					if (canBeDeactivated)
						this.DeactivateContainerHelper(cr);
					else
						this.ClearContainerReference(cr, true, true, false);
                }
                else
                    this.ClearContainerReference(cr, true, true, false);
            }
        }

                #endregion //DeactivateLimboedContainer	
        
				#region DeactivateAllContainersInUse

		private void DeactivateAllContainersInUse()
		{
			int count = this._activeContainersInUse.Count;

			if (count < 1)
				return;

			// walk over every node (to deactivate each one
			for (int i = 0; i < count; i++)
				this.DeactivateContainerHelper(this._activeContainersInUse[i]);

			this._activeContainersInUse.Clear();

            // JJD 7/31/09 - TFS18162
            // Also clear the activeContainersInUseByZorder collection
            if ( this._activeContainersInUseByZorder != null )
                this._activeContainersInUseByZorder.Clear();
		}

				#endregion //DeactivateAllContainersInUse	
    
				#region GetItem

		private object GetItem(DependencyObject container)
		{
			ContainerReference cr = GetContainerReference(container);

			if ( cr == null )
				return null;

			return cr.Item;
		}

		private object GetItem(DependencyObject container, out int index)
		{
			ContainerReference cr = GetContainerReference(container);

			if (cr != null)
				return this.GetItem(cr, out index);

			index = -1;

			return null;
		}

		private object GetItem(ContainerReference cr, out int index)
		{
			Debug.Assert(this._indexMap.Count == this.Items.Count);

			index = this._indexMap.IndexOf(cr);

			if (index < 0)
				return null;

			object item = this.Items[index];

			// assert we are in sync
			Debug.Assert(item == cr.Item);

			return item;
		}

				#endregion //GetItem	

                // JJD 3/18/10 - TFS28705 - added 
				#region HasContainerBeenCleared

        private bool HasContainerBeenCleared(ContainerReference cr)
		{
            return cr.Container == null || cr.Container.GetValue(ContainerReferenceProperty) == null;
		}

				#endregion //HasContainerBeenCleared

				#region InitializeIndexMap

		private void InitializeIndexMap()
		{
			this._indexMap = new SparseArray(20, 1.0f, true);

			// initialize the map size so that it has the same number of slots
			// as the count from the Items collection
			this._indexMap.Expand(this.Items.Count);
		}

				#endregion //InitializeIndexMap	
 
                // JJD 1/22/10 - TFS26513 - added
                #region IsContainerComptibleWithItem

        private bool IsContainerCompatibleWithItem(ContainerReference cr, object item)
        {
            if (cr.IsItemItsOwnContainer)
                return item == cr.Item;

            return this._recyclingItemsControl.IsContainerCompatibleWithItem(cr.Container, item);
        }

                #endregion //IsContainerComptibleWithItem	
    
				#region PlaceAllConnectedContainersInLimbo

        // JJD 3/19/10 - TFS28705 - Optimization
        // Added itemsExpectedToBeGenerated parameter
		private void PlaceAllActiveContainersInLimbo(IEnumerable itemsExpectedToBeGenerated)
		{
            HashSet expectedItemsHash = null;

            // JJD 3/19/10 - TFS28705 - Optimization
            // If itemsExpectedToBeGenerated was provided then populate a HashSet for
            // quick access below
            if (itemsExpectedToBeGenerated != null)
            {
                expectedItemsHash = new HashSet();
                expectedItemsHash.AddItems(itemsExpectedToBeGenerated);

                if (expectedItemsHash.Count == 0)
                    expectedItemsHash = null;
            }

            int countOfActive       = this._activeContainersInUse.Count;
            int countOfDeactivated  = this._deactivatedContainers.Count;
            int countOfOldRefs      = this._referencesBeforeLastReset != null ? this._referencesBeforeLastReset.Count : 0;

            // JJD 3/19/10 - TFS28705 - Optimization
            // Allocate the caches we will use during generation
            this._recyclableContainers              = new Dictionary<object, ContainerReference>();
            this._recyclableContainersList          = new List<ContainerReference>(countOfActive + countOfDeactivated + countOfOldRefs);
            this._recyclableContainersListFiltered  = new List<ContainerReference>(countOfActive + countOfDeactivated + countOfOldRefs);

            List<ContainerReference> deactivedItems = new List<ContainerReference>();

            // JJD 3/19/10 - TFS28705 - Optimization
            // Populate the _recyclableContainersList with everything
            if (countOfActive > 0)
                this._recyclableContainersList.AddRange(this._activeContainersInUse);
            if (countOfOldRefs > 0)
                this._recyclableContainersList.AddRange(this._referencesBeforeLastReset.Values);
            if (countOfDeactivated > 0)
                this._recyclableContainersList.AddRange(this._deactivatedContainers);

            int count = this._recyclableContainersList.Count;

            // JJD 3/19/10 - TFS28705 - Optimization
            // Loop over all the containers so we can popluate the filtered lists
            for (int i = 0; i < count; i++)
            {
                ContainerReference cr = this._recyclableContainersList[i];

                object item = cr.Item;

                // JJD 3/19/10 - TFS28705 - Optimization
                // If the item is null or it is a duplicate then add it to the
                // _recyclableContainersDups hashset
                if (item == null ||
                    this._recyclableContainers.ContainsKey(item))
                {
                    if (this._recyclableContainersDups == null)
                        this._recyclableContainersDups = new HashSet();

                    this._recyclableContainersDups.Add(cr);
                }
                else
                {
                    // JJD 3/19/10 - TFS28705 - Optimization
                    //  Update the dictinary which is used for quick access using the Item as the key
                    this._recyclableContainers[item] = cr;
                }

                // JJD 3/19/10 - TFS28705 - Optimization
                // We want to keep deactivated containers separate so put them
                // into a temporay list of filtered items that we wil insert into
                // the _recyclableContainersListFiltered below, either at the beginning or
                // the end of the list (see notes below).
                if (cr.IsDeactivated)
                    deactivedItems.Add(cr);
                else
                {
                    // JJD 3/19/10 - TFS28705 - Optimization
                    // Check to see if the item is in the passed in expected collection of items
                    if (expectedItemsHash == null ||
                         !expectedItemsHash.Exists(item))
                    {
                        this._recyclableContainersListFiltered.Add(cr);
                    }
                }
            }

            // At this point the _recyclableContainersListFiltered contains only
            // active containers.

            // if the scroll direction is up and 'expected' items weren't supplied
            // then reverse the order of active items so that we try to recycle from
            // the bottom first.
            // Otherwise, randomize the list to minimize the chance that we recycle a
            // container for an item that might be subsequently generated
            if (this._recyclableContainersListFiltered.Count > 1)
            {
                if (expectedItemsHash == null &&
                     this._scrollDirection == ScrollDirection.Decrement)
                    this._recyclableContainersListFiltered.Reverse();
                else
                    Utilities.RandomizeList<ContainerReference>(this._recyclableContainersListFiltered);
            }

            // finally insert the deactivated items into the list at 
            // the beginning or the end depending on whether 'expected' items were supplied.
            // In that case we can assume that it is more efficient to re-use an active
            // container than to activate  de-activated one. Otherwise, inserting them
            // at the end has the potential to re-use a container for an item
            // that will be subsequently generated
            if (deactivedItems.Count > 0)
            {
                if (expectedItemsHash == null)
                    this._recyclableContainersListFiltered.InsertRange(0, deactivedItems);
                else
                    this._recyclableContainersListFiltered.AddRange(deactivedItems);
            }

            
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			// clear the connected containers 'in use' cache
			this._activeContainersInUse.Clear();

            // JJD 7/31/09 - TFS18162
            // Also clear the activeContainersInUseByZorder collection
            if (this._activeContainersInUseByZorder != null)
                this._activeContainersInUseByZorder.Clear();
        }

				#endregion //PlaceAllConnectedContainersInLimbo	

                // JJD 05/07/10 - TFS31643 - added
                #region ProcessPendingReset

        private void ProcessPendingReset()
        {
            // JJD 05/07/10 - TFS31643 
            // If the operation is already nulled out then return
            if (this._resetOperationPending == null)
                return;

            // null out _resetOperationPending
            this._resetOperationPending = null;

            if (this._referencesBeforeLastReset != null)
            {
                List<ContainerReference> crsToRemove = new List<ContainerReference>();
                
                // loop over all container refs to see it they are still valid
                // to populate a list of ones to remove
                foreach (ContainerReference cr in this._referencesBeforeLastReset.Values)
                {
                    bool? isStillValid = this._recyclingItemsControl.IsStillValid(cr.Container, cr.Item);

                    // if the method returns null (the default) then don't do anything
                    if (isStillValid.HasValue && isStillValid.Value == false )
                        crsToRemove.Add(cr);
                }

                foreach (ContainerReference cr in crsToRemove)
                {
                    // remove the reference from the list
                    if (cr.Item != null)
                    {
                        this._referencesBeforeLastReset.Remove(cr.Item);

                        // clear the container ref
                        this.ClearContainerReference(cr, false, false, false);
                    }
                }
            }
        }

                #endregion //ProcessPendingReset	
    		
				#region RemoveContainerFromCache

		private bool RemoveContainerFromCache(ContainerReference cr)
		{
            // JJD 3/19/10 - TFS28705 - Optimization 
            // Remove from recyclable cache first and if successful return true
            // since if the container is in the recycable cahe it can't be anywhere else
            //if (this.RemoveContainerFromCache(cr, this._activeContainersInLimbo) )
			bool removedFromRecycableCache = this.RemoveContainerFromRecyclableCache(cr);

            // JJD 3/19/10 - TFS28705 - Optimization 
            // Even if it was removed from the recycling cach check if its deactivated and
            // remove it from thsat cache
            if (removedFromRecycableCache == false ||
                cr.IsDeactivated)
            {
                if (this._deactivatedContainers.Remove(cr) )
				{
					// MD 3/16/11 - TFS24163
					// If we removed the item from the _deactivatedContainers collection, it might also exist in the 
					// _deactivatedContainersNotInIndexMap collection, so remove it from there as well.
					if (_deactivatedContainersNotInIndexMap != null)
						_deactivatedContainersNotInIndexMap.Remove(cr);

                    return true;
				}
            }

            // JJD 3/19/10 - TFS28705 - Optimization 
            // At this point if we removed it from the recycling cache we can return
            if (removedFromRecycableCache)
                return true;

            bool successful = this._activeContainersInUse.Remove(cr);

            // JJD 7/31/09 - TFS18162
            // Also maintain the activeContainersInUseByZorder collection
            if (successful && this._activeContainersInUseByZorder != null)
            {
                this._activeContainersInUseByZorder.Remove(cr);
            }

			return successful;
		}

        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


        // JJD 3/19/10 - TFS28705 - Optimization - added
        private bool RemoveContainerFromRecyclableCache(ContainerReference cr)
        {
            if (this._recyclableContainers == null ||
                 !this._recyclableContainers.Remove(cr.Item))
            {
                if (this._recyclableContainersDups != null &&
                    this._recyclableContainersDups.Exists(cr))
                {
                    this._recyclableContainersDups.Remove(cr);

                    if (this._recyclableContainersDups.Count == 0)
                        this._recyclableContainersDups = null;
                }
                else
                    return false;
            }

            this._recyclableContainersListFiltered.Remove(cr);
            this._recyclableContainersList.Remove(cr);

            return true;
        }

                #endregion //RemoveContainerFromCache

				// JJD 8/24/11 - TFS84215 - added
				#region SetItemContainerStyle

		private void SetItemContainerStyle(DependencyObject container, object item)
		{
			FrameworkElement feContainer = container as FrameworkElement;

			if (feContainer == null)
				return;

			bool isStyleAlereadySet = (bool)container.GetValue(RecyclingItemContainerGenerator.IsStyleSetFromItemContainerStyleProperty);

			if (false == isStyleAlereadySet && DependencyProperty.UnsetValue != container.ReadLocalValue(FrameworkElement.StyleProperty))
				return;

			Style style = this._recyclingItemsControl.ItemContainerStyle;

			if (style == null)
			{
				StyleSelector selector = this._recyclingItemsControl.ItemContainerStyleSelector;

				if (selector != null)
					style = selector.SelectStyle(item, container);
			}

			if (style != null)
			{
				// make sure the style's type is appropriate
				if (style.TargetType != null &&
					 !style.TargetType.IsInstanceOfType(item))
					throw new InvalidOperationException(SR.GetString("LE_InvalidStyleTargetType", style.TargetType, container.GetType()));

				container.SetValue(FrameworkElement.StyleProperty, style);
			}
			else
				container.ClearValue(FrameworkElement.StyleProperty);

			container.SetValue(RecyclingItemContainerGenerator.IsStyleSetFromItemContainerStyleProperty, KnownBoxes.FromValue(style != null));
		}

				#endregion //SetItemContainerStyle	
    
        #region SetStatus

        private void SetStatus(GeneratorStatus status)
		{
			if (this._status != status)
			{
				this._status = status;

				if (this.StatusChanged != null)
					this.StatusChanged(this, EventArgs.Empty);
			}
		}

				#endregion //SetStatus



#region Infragistics Source Cleanup (Region)









































































































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Private Methods	
    
   		#endregion //Methods	
        
		#region IItemContainerGenerator Members

		#region GenerateNext

		/// <summary>
		/// Returns the container element used to display the next item.
		/// </summary>
		/// <returns>The container element or null if the container has already been generated for that item.</returns>
		public DependencyObject GenerateNext()
		{
			if (this._currentGenerator == null)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_5"));

			bool isNewlyRealized;

			return this._currentGenerator.GenerateNext(true, out isNewlyRealized);
		}

		/// <summary>
		/// Returns the container element used to display the next item, and whether the container element has been newly generated (realized).
		/// </summary>
		/// <param name="isNewlyRealized">Is true is the returned container element is newly generated (realized); otherwise, false.</param>
		/// <returns>The container element. If the container was newly realized in the menthod then the isNewlyRealized out paramater will be set to true.</returns>
		public DependencyObject GenerateNext(out bool isNewlyRealized)
		{
			if (this._currentGenerator == null)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_5"));

			return this._currentGenerator.GenerateNext(false, out isNewlyRealized);
		}

		#endregion //GenerateNext	
    
		#region GeneratorPositionFromIndex

		/// <summary>
		/// Returns the GeneratorPosition object that maps to the item at the specified index.
		/// </summary>
		/// <param name="itemIndex">The index of the desired item.</param>
		/// <returns>The GeneratorPosition object that maps to the item.</returns>
		public GeneratorPosition GeneratorPositionFromIndex(int itemIndex)
		{
			if ( itemIndex < 0 || itemIndex >= this.Items.Count)
				// [JM BR23281 05-29-07]
				//return new GeneratorPosition(-1, itemIndex + 1);
				return new GeneratorPosition(-1, Math.Max(0, itemIndex + 1));

			int count		= this._activeContainersInUse.Count;
			int baseIndex	= -1;
			int offset		= itemIndex + 1;
			int mapIndex;
			int lastMapIndex	= -1;

			for (int i = 0; i < count; i++)
			{
				ContainerReference cr = this._activeContainersInUse[i];

				Debug.Assert(cr != null);
				if (cr == null)
					continue;

				// get the mapped index for this item in the list
				mapIndex = this._indexMap.IndexOf( cr );

				// this should always be >= 0
				Debug.Assert(mapIndex >= 0);

				if (mapIndex < 0)
					break;

				// if we hit an exact index match then return this position with an offset of zero
				if (mapIndex == itemIndex)
					return new GeneratorPosition(i, 0);

				// if the mapped index is > that the requested itemIndex then
				// break out so we use the baseIndex cached so far
				if (mapIndex > itemIndex)
					break;

				lastMapIndex = mapIndex;

				// cached the index in the list as the base index since the map
				// index is still less than the requested item index.
				baseIndex = i;
			}

			// return a position using the last valid base index and caclulate the
			// offet from it
			// [JM BR23281 05-29-07]
			//return new GeneratorPosition(baseIndex, itemIndex - lastMapIndex);
			return new GeneratorPosition(baseIndex, Math.Max(0, itemIndex - lastMapIndex));
		}

		#endregion //GeneratorPositionFromIndex	
    
		#region GetItemContainerGeneratorForPanel

		/// <summary>
		/// Not supported
		/// </summary>
		/// <param name="panel"></param>
		/// <exception cref="NotSupportedException">Always throws this exception.</exception>
		ItemContainerGenerator IItemContainerGenerator.GetItemContainerGeneratorForPanel(Panel panel)
		{
			throw new NotSupportedException();
		}

		#endregion //GetItemContainerGeneratorForPanel	
    
		#region IndexFromGeneratorPosition

		/// <summary>
		/// Returns the index that maps to the specified GeneratorPosition.
		/// </summary>
		/// <param name="position">The GeneratorPosition for the desired index</param>
		/// <returns>The index that maps to the GeneratorPosition.</returns>
		public int IndexFromGeneratorPosition(GeneratorPosition position)
		{
			int baseIndex = position.Index;

			if (baseIndex < 0)
				return baseIndex + position.Offset;
			
			int count = this.CountOfAllContainers;

			Debug.Assert(baseIndex < count);

			if (baseIndex >= count)
				return baseIndex + position.Offset;

			ContainerReference cr = this.GetContainerReference(baseIndex, false);

			Debug.Assert(cr != null);

			if ( cr == null )
				return baseIndex + position.Offset;

			int mapIndex = this._indexMap.IndexOf(cr);

			Debug.Assert(mapIndex >= 0);

			if (mapIndex < 0)
				return baseIndex + position.Offset;

			return mapIndex + position.Offset;
		}

		#endregion //IndexFromGeneratorPosition	
    
		#region PrepareItemContainer

		/// <summary>
		/// Prepares the specified element as the container for the corresponding item.
		/// </summary>
		/// <param name="container">The container to prepare. Normally, container is the result of the previous call to <see cref="GenerateNext(out bool)"/>.</param>
		public void PrepareItemContainer(DependencyObject container)
		{
			object item = this.GetItem(container);

			// JJD 8/24/11 - TFS84215
			// Set the ItemContainerStyle if specified
			if (this._recyclingItemsControl.ShouldApplyItemContainerStyleInternal(container, item))
				this.SetItemContainerStyle(container, item);

			this._recyclingItemsControl.PrepareContainerForItemInternal(container, item);
		}

		#endregion //PrepareItemContainer	

		#region Remove

		/// <summary>
		/// Removes one or more generated (realized) items.
		/// </summary>
		/// <param name="position">The index of the element to remove. position must refer to a previously generated (realized) item, which means its offset must be zero.</param>
		/// <param name="count">The number of elements to remove, starting at position.</param>
		public void Remove(GeneratorPosition position, int count)
		{
			if (position.Offset != 0)
				throw new ArgumentException(SR.GetString("LE_RecyclingGenerator_6"), "position");

			if (count < 1)
				throw new ArgumentException(SR.GetString("LE_RecyclingGenerator_7"), "count");

			int startIndex = Math.Max(0, position.Index);
			int endIndex = startIndex + count - 1;

			int totalItemCount = this._indexMap.Count;

			if (endIndex >= totalItemCount)
				endIndex = totalItemCount - 1;

			for (int i = startIndex; i <= endIndex; i++)
			{
				ContainerReference cr = this._indexMap[i] as ContainerReference;

				if (cr != null)
				{
					this.ClearContainerReference(cr, false, true, true);
					this._indexMap[i] = null;
				}
			}
		}

		#endregion //Remove	
    
		#region RemoveAll

		/// <summary>
		/// Removes all generated (realized) items.
		/// </summary>
		public void RemoveAll()
		{
			// JM 10-21-10 TFS57364 - Add Try/Finally to manage the new _isInRemoveAll flag.
			this._isInRemoveAll = true;
			try
			{
				foreach (ContainerReference cr in this._indexMap.NonNullItems)
					this.ClearContainerReference(cr, false, false, false);


				// JJD 3/19/10 - TFS28705 - Optimization 
				// if we are in the middle of a gen then clear the cache
				if (this._recyclableContainers != null)
				{
					foreach (ContainerReference cr in this._recyclableContainersList)
						this.ClearContainerReference(cr, false, false, false);

					this._recyclableContainers = null;
					this._recyclableContainersList = null;
					this._recyclableContainersListFiltered = null;
					this._recyclableContainersDups = null;
				}
				else
				{
					// JJD 6/5/07
					// Also process any containers in the _referencesBeforeLastReset cache
					if (this._referencesBeforeLastReset != null)
					{
						foreach (ContainerReference cr in this._referencesBeforeLastReset.Values)
							this.ClearContainerReference(cr, false, false, false);
					}
				}

				// JJD 6/5/07
				// clear the cache
				this._referencesBeforeLastReset = null;

				// JJD 11/04/08 - TFS7939
				// null out the referencesBeforeLastReset 
				this._referencesBeforeLastResetList = null;

				this._activeContainersInUse.Clear();
				
				
				this._deactivatedContainers.Clear();

				// MD 3/16/11 - TFS24163
				this._deactivatedContainersNotInIndexMap = null;

				// JJD 7/31/09 - TFS18162
				// Also maintain the activeContainersInUseByZorder collection
				if (this._activeContainersInUseByZorder != null)
					this._activeContainersInUseByZorder.Clear();

				this._indexMap.Clear();
				this._indexMap.Expand(this.Items.Count);
			}
			finally
			{
				this._isInRemoveAll = false;
			}
		}

		#endregion //RemoveAll	
    
		#region StartAt

		/// <summary>
		/// Prepares the generator to generate items, starting at the specified GeneratorPosition, and in the specified GeneratorDirection.
		/// </summary>
		/// <param name="position">A <see cref="GeneratorPosition"/>, that specifies the position of the item to start generating items at.</param>
		/// <param name="direction">A <see cref="GeneratorDirection"/> that specifies the direction which to generate items.</param>
		/// <param name="allowStartAtRealizedItem">Specifies whether to start at a generated (realized) item.</param>
		/// <returns></returns>
		public IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem)
		{
			if (this._currentGenerator != null)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_8"));

			if (this._isGenerating == false)
				throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_9"));





            this._currentGenerator = new RecyclingGenerator(this, position, direction, allowStartAtRealizedItem);





			return this._currentGenerator;
		}

		/// <summary>
		/// Prepares the generator to generate items, starting at the specified GeneratorPosition, and in the specified GeneratorDirection.
		/// </summary>
		/// <param name="position">A <see cref="GeneratorPosition"/>, that specifies the position of the item to start generating items at.</param>
		/// <param name="direction">A <see cref="GeneratorDirection"/> that specifies the direction which to generate items.</param>
		public IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction)
		{
			if (this._isGenerating == false)
				throw new InvalidOperationException(SR.GetString("LE_BeginGenerationNotCalled"));

			return ((IItemContainerGenerator)this).StartAt(position, direction, false);
		}

		#endregion //StartAt	
    
		#endregion

		#region ContainerReference class

		// JM 01-28-08 BR30138 - Derive from DependencyObject so we can make the ZIndex property a dependency property.
		//private class ContainerReference : ISparseArrayItem
		private class ContainerReference : DependencyObject, ISparseArrayItem
		{
			#region Private Members

			private object _sparseArrayData;
			private DependencyObject _container;
			private object _item;
			private object _cachedVisibilityLocalValue;
			private bool _isDeactivated;
			private bool _isInVisualTree;
            private bool _isItemItsOwnContainer; // JJD 1/22/10 - TFS26513 - added

			// JM 01-28-08 BR30138 - No longer needed since the ZIndex property has been changed to a dependency property.
			//private int _zIndex;

			internal int _originalIndex;

			// AS 6/1/07
			private RecyclingItemContainerGenerator _generator;

			#endregion //Private Members

			#region Constructor

            // JJD 1/22/10 - TFS26513
            // Added isItemItsOwnContainer param
            //internal ContainerReference(DependencyObject container)
			internal ContainerReference(DependencyObject container, bool isItemItsOwnContainer)
			{
				this._container = container;

                // JJD 1/22/10 - TFS26513
                // Added isItemItsOwnContainer param
                this._isItemItsOwnContainer = isItemItsOwnContainer;

                if (this._isItemItsOwnContainer)
                {
                    this._item = this._container;
                }

				if (container is UIElement)
				{
					// JM 01-28-08 BR30138 - Change the binding to a OneWay binding and reverse the direction of the binding.
					//Binding binding = new Binding();
					//binding.Mode = BindingMode.OneWayToSource;
					//binding.Path = new PropertyPath("ZIndex");
					//binding.Source = this;
					//BindingOperations.SetBinding(container, Panel.ZIndexProperty, binding);
					Binding binding = new Binding();
					binding.Mode = BindingMode.OneWay;
					binding.Path = new PropertyPath(Panel.ZIndexProperty);
					binding.Source = container;
					BindingOperations.SetBinding(this, ContainerReference.ZIndexProperty, binding);

					// JM 01-28-08 BR30138 - The ZIndex proeprty will now be initialized by the binding.
					//this._zIndex = Panel.GetZIndex(container as UIElement);
				}
			}

			#endregion //Constructor

			#region Base class overrides

			#region ToString
			public override string ToString()
			{
				return string.Format("Item:{0} Container:{1}", this.Item, this.Container);
			}
			#endregion //ToString 

			#endregion //Base class overrides

			#region Properties

				#region Container

			internal DependencyObject Container
			{
				get { return this._container; }
			}

				#endregion //Container

				// AS 6/1/07
				#region Generator
			internal RecyclingItemContainerGenerator Generator
			{
				get { return this._generator; }
				set { this._generator = value; }
			} 
				#endregion //Generator

				#region IsDeactivated

			internal bool IsDeactivated
			{
				get { return this._isDeactivated; }
				set 
				{
					if (this._isDeactivated != value)
					{
						this._isDeactivated = value;
                        
                        // JJD 1/22/10 - TFS26513
                        // Check IsItemItsOwnContainer property
                        if (this._isItemItsOwnContainer)
                            return;

						UIElement element = this._container as UIElement;

						if (element != null)
						{
							if (this._isDeactivated)
							{
								// cache the locally set value of the Visibility property so we can restore it later
								this._cachedVisibilityLocalValue = element.ReadLocalValue(UIElement.VisibilityProperty);

								element.SetValue(UIElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

								// If its a binding expession then don't bother caching the binding expression
								// instead cache its ParentBinding.
								if (this._cachedVisibilityLocalValue is BindingExpressionBase)
									this._cachedVisibilityLocalValue =((BindingExpressionBase)(this._cachedVisibilityLocalValue)).ParentBindingBase;
							}
							else
							{
								if (this._cachedVisibilityLocalValue is Visibility)
									element.SetValue(UIElement.VisibilityProperty, this._cachedVisibilityLocalValue);
								else
								if (this._cachedVisibilityLocalValue is BindingBase)
								{
									FrameworkElement fe = element as FrameworkElement;
									if (fe != null)
										fe.SetBinding(UIElement.VisibilityProperty, (BindingBase)(this._cachedVisibilityLocalValue)); 
									else
										element.ClearValue(UIElement.VisibilityProperty);
								}
								else
									element.ClearValue(UIElement.VisibilityProperty);

								this._cachedVisibilityLocalValue = null;
							}
						}
					}
				}
			}

				#endregion //IsDeactivated

				#region IsInVisualTree

			internal bool IsInVisualTree
			{
				get { return this._isInVisualTree; }
				set 
				{
					if (this._isInVisualTree != value)
					{
						this._isInVisualTree = value;
					}
				}
			}

				#endregion //IsInVisualTree

                // JJD 1/22/10 - TFS26513 - added
                #region IsItemItsOwnContainer

            internal bool IsItemItsOwnContainer { get { return this._isItemItsOwnContainer; } }

                #endregion //IsItemItsOwnContainer	
    
				#region Item

			internal object Item
			{
				get { return this._item; }
				set { this._item = value; }
			}

				#endregion //Item

				#region ZIndex

			// JM 01-28-08 BR30138 - Make the ZIndex property a dependency property with a property changed callback to process a ZOrder change.
			public static readonly DependencyProperty ZIndexProperty = DependencyProperty.Register("ZIndex",
				typeof(int), typeof(ContainerReference), new FrameworkPropertyMetadata((int)0, new PropertyChangedCallback(OnZIndexPropertyChanged)));

			static void OnZIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				ContainerReference cr = d as ContainerReference;

				if (cr != null && cr._generator != null)
					cr._generator.OnZorderChanged();

			}

			// AS 6/1/07
			// Using a DependencyPropertyDescriptor will actually root this class. Instead
			// we will use a OneWayToSource binding to update the container reference.
			//
			//internal int ZIndex { get { return this._zIndex; } }
			public int ZIndex 
			{
				// JM 01-28-08 BR30138 - Since the ZIndex property is now a dependency property, use GetValue/SetValue and
				// rely on the PropertyChangedCallback to process a ZOrder change.
				//get { return this._zIndex; } 
				//set 
				//{ 
				//    this._zIndex = value;

				//    if (this._generator != null)
				//        this._generator.OnZorderChanged();
				//} 
				get { return (int)this.GetValue(ContainerReference.ZIndexProperty); } 
				set	{ this.SetValue(ContainerReference.ZIndexProperty, value); } 
			}

				#endregion //ZIndex	
    
			#endregion //Properties

			#region Methods

				#region OnZorderChanged

			// AS 6/1/07
			// This isn't needed since we're using a binding.
			//
			//internal void OnZorderChanged()
			//{
			//	this._zIndex = Panel.GetZIndex(this._container as UIElement);
			//}

				#endregion //OnZorderChanged

			#endregion //Methods	
        
			#region ISparseArrayItem Members

			object ISparseArrayItem.GetOwnerData(SparseArray context)
			{
				return this._sparseArrayData;
			}

			void ISparseArrayItem.SetOwnerData(object ownerData, SparseArray context)
			{
				this._sparseArrayData = ownerData;
			}

			#endregion
		}

		#endregion //ContainerReference class

		#region RecyclingGenerator class

		private class RecyclingGenerator : IDisposable
		{
			#region Private Members

			private RecyclingItemContainerGenerator _owner;
			private GeneratorPosition _position;
			private GeneratorDirection _direction;
			private ContainerReference _newlyCreatedContainerNotInVisualTree;
			private int _currentIndex;
			private int _startIndex;
			private List<ContainerReference> _generatedContainers;

			#endregion //Private Members

			#region Constructor

			internal RecyclingGenerator(RecyclingItemContainerGenerator owner, GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem)
			{
				this._owner = owner;
				this._position = position;
				this._direction = direction;
				this._currentIndex = position.Index;
				int index = position.Index;
				int offset = position.Offset;

				this._generatedContainers = new List<ContainerReference>();

				if (index >= 0)
				{
					int activeContainerCount = this._owner.CountOfActiveContainers;

					// if the index is a valid index of an active container then
					// gets its mapped index into the list
					if ( index < activeContainerCount)
					{
						ContainerReference cr = this._owner.GetContainerReference(index, false);

						Debug.Assert(cr != null, "RecyclingGenerator ctor");

						if (cr != null)
						{
							this._currentIndex = this._owner._indexMap.IndexOf(cr);

							Debug.Assert(this._currentIndex >= 0, "RecyclingGenerator ctor - assert 2");
						}
					}
				}

				if (offset == 0)
				{
					if (this._currentIndex == -1)
					{
						if (direction == GeneratorDirection.Backward)
							this._currentIndex = this._owner.Items.Count - 1;
						else
							this._currentIndex = 0;
					}

					if (!allowStartAtRealizedItem)
					{
						if (this._owner._indexMap[this._currentIndex] != null)
						{
							if (direction == GeneratorDirection.Backward)
								offset = -1;
							else
								offset = 1;
						}
					}
				}
				else
				if (offset < 0 && this._currentIndex < 0)
				{
					if (direction == GeneratorDirection.Backward)
						this._currentIndex = this._owner.Items.Count;
					else
						this._currentIndex = -1;
				}

				this._currentIndex += offset;

				this._startIndex = this._currentIndex;

				this._owner.SetStatus( GeneratorStatus.GeneratingContainers );
			}

			#endregion //Constructor
			
			#region Properties

				#region CountOfGeneratedItems

			internal int CountOfGeneratedItems 
			{ 
				get 
				{ 
					int count = this._generatedContainers.Count;

					if (this._newlyCreatedContainerNotInVisualTree != null)
						count--;

					return count;
				} 
			}

				#endregion //CountOfGeneratedItems	

				#region InsertAtIndexInActiveList

			private int InsertAtIndexInActiveList
			{
				get
				{
					int countofExistingActiveContainers = this._owner._activeContainersInUse.Count;
					int i;

					// find the appropriate index to insert this block of items 
					for (i = 0; i < countofExistingActiveContainers; i++)
					{
						if (this._owner._indexMap.IndexOf(this._owner._activeContainersInUse[i]) > this._startIndex)
							break;
					}

					return i;
				}
			}

				#endregion //InsertAtIndexInActiveList	
    
			#endregion //Properties

			#region Methods

				#region CalculateNextIndex

			private void CalculateNextIndex()
			{
				if (this._direction == GeneratorDirection.Backward)
					this._currentIndex--;
				else
					this._currentIndex++;
			}

				#endregion //CalculateNextIndex
    
				#region GenerateNext

			internal DependencyObject GenerateNext(bool returnNullIfAlreadyRealized, out bool isNewlyRealized)
			{
				if (this._newlyCreatedContainerNotInVisualTree != null)
					throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_10"));



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


				isNewlyRealized = false;

                // JJD 1/15/08 
                // Removed unwarranted assert 
				//Debug.Assert(this._owner.Items.Count == this._owner._indexMap.Count);

				// make sure the index is in range
				if (this._currentIndex < 0 || this._currentIndex >= this._owner.Items.Count)
					return null;

				if (this._currentIndex >= this._owner._indexMap.Count)
					this._owner._indexMap.Expand(this._currentIndex + 1);

				ContainerReference cr = this._owner._indexMap[this._currentIndex] as ContainerReference;

				if (cr != null)
				{
                    Debug.Assert(Object.Equals(this._owner.Items[this._currentIndex], cr.Item) == true, "Container item doesn't match in RecyclingGenerator.GenerateNext.");




                    
                    this._owner.RemoveContainerFromCache(cr);

                    
                    Debug.Assert(this._owner.HasContainerBeenCleared(cr) == false, "Cleared containers should not be in the map");

					if (cr.IsDeactivated)
					{
						cr.IsDeactivated = false;
						this._owner._recyclingItemsControl.ReactivateContainer(cr.Container, cr.Item);
					}

					if (returnNullIfAlreadyRealized)
					{
						// keep track of all the containers that were generated
						this._generatedContainers.Add(cr);



                        // set up index for the next call to this method
						this.CalculateNextIndex();
                        return null;
					}
				}
				else
				{
					bool needsReactivation = false;
					bool hasItemChanged = false;
                    
                    
                    

					// get the underlying item at the current index
					object item = this._owner.Items[this._currentIndex];

					// SSP 1/14/08 BR29653
					// Returned item from the ViewableRecordCollection can be null. This situation arises
					// when the collection had a null slot (virtualization of records) and then a new record
					// was created to fill it. If that created record's Visibility is set to Collapsed then
					// it will not be considered part of the viewable record collection and the next visible
					// record will be returned. However if there is no next visible record (because the entire
					// collection is exhausted), then it will return null. In that case the viewable record
					// collection will have a count that's less than what's required for the _currentIndex to
					// be valid.
					// 
					if ( null == item )
						return null;

                    
                    #region Old code

                    
#region Infragistics Source Cleanup (Region)















































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

                    #endregion //Old code	
    
                    // JJD 3/19/10 - TFS28705 - Optimization 
                    // Check the cached dictionary for a key (item) match first
                    // If found use it.
                    if (this._owner._recyclableContainers != null)
                    {
                        ContainerReference crTemp;
                        if (this._owner._recyclableContainers.TryGetValue(item, out crTemp))
                        {
                            cr = crTemp;
                        }
                        else
                        {
                            // JJD 3/19/10 - TFS28705 - Optimization 
                            // Since we didn't find a match for the item loop over any duplicate items
                            // looking for an exact match
                            if (this._owner._recyclableContainersDups != null)
                            {
                                foreach (ContainerReference duplicate in this._owner._recyclableContainersDups)
                                {
                                    if (Object.Equals(item, duplicate.Item))
                                    {
                                        cr = duplicate;
                                        break;
                                    }
                                }
                            }

                            if (cr == null)
                            {
                                // JJD 3/19/10 - TFS28705 - Optimization 
                                // Since we didn't find a match for the item loop over the
                                // _recyclableContainersListFiltered list looking for a compatible
                                // container that we can recycle.
                                int count = this._owner._recyclableContainersListFiltered.Count;

                                for (int i = 0; i < count; i++)
                                {
                                    crTemp = this._owner._recyclableContainersListFiltered[i];

                                    if (this._owner.IsContainerCompatibleWithItem(crTemp, item))
                                    {
                                        cr = crTemp;
                                        hasItemChanged = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

					#region Create container if necessary

					if (cr == null)
					{
						cr = this._owner.CreateNewContainerReference(item);

						isNewlyRealized = true;
                        
                        
                        this._newlyCreatedContainerNotInVisualTree = cr;
					}
                    else
                        this._owner.RemoveContainerFromCache(cr);


					#endregion //Create container if necessary

					#region Update the index map 

                    // JJD 3/17/10 - TFS28705 - Optimization
                    // Always check the map to see it it contains an entry 
                    //if (hasItemChanged == true)
					//{
					int oldIndex = this._owner._indexMap.IndexOf(cr);

                    // JJD 3/17/10 - TFS28705 - Optimization
                    // Only null out the map entry if it is the wrong index
					
                    if (oldIndex >= 0 && oldIndex != this._currentIndex)
                        this._owner._indexMap[oldIndex] = null;

                    // JJD 3/17/10 - TFS28705 - Optimization
                    // Always update the map entry
                    // update the map at the appropriate index
                    
                    
                    Debug.Assert(this._owner._indexMap[this._currentIndex] == null ||
                                 this._owner._indexMap[this._currentIndex] == cr, "Overlaying index map entry");

                    this._owner._indexMap[this._currentIndex] = cr;
                    
                    
                    #endregion //Update the index map

                    // JJD 3/17/10 - TFS28705 - Optimization
                    // Set the flag so we call ReactivateContainer below
                    if (cr.IsDeactivated)
                    {
                        if ( hasItemChanged == false)
                            needsReactivation = true;
					    
                        cr.IsDeactivated = false;
                   }

					// set the Item reference and clear the IsDeactivated flag
					cr.Item = item;

                    if (needsReactivation)
                        this._owner._recyclingItemsControl.ReactivateContainer(cr.Container, cr.Item);
                    else
                        if (hasItemChanged)
                        {
                            this._owner.ConnectContainerToItem(cr);
                            this._owner._recyclingItemsControl.ReuseContainerForNewItem(cr.Container, cr.Item);
                        }
				}

				// keep track of all the containers that were generated
				this._generatedContainers.Add(cr);





				// set up index for the next call to this method
				this.CalculateNextIndex();

				return cr.Container;
			}

				#endregion //GenerateNext

				#region GetActiveContainerReference

			internal ContainerReference GetActiveContainerReference(int index, bool isIndexBasedOnZOrder)
			{
				int count						= this._owner.CountOfActiveContainers;
				int countOfGeneratedItems		= this._generatedContainers.Count;
				int insertGeneratedItemsAtIndex = this.InsertAtIndexInActiveList;

				List<ContainerReference> activeContainers;

				if (isIndexBasedOnZOrder)
					activeContainers = this._owner.ActiveContainersInUseByZorder;
				else
					activeContainers = this._owner._activeContainersInUse;

				// if the requested index is before the insertion point in the active
				// list we can just use the index as is into the _activeContainersInUse list
				if (countOfGeneratedItems == 0 ||
					index < insertGeneratedItemsAtIndex )
					return activeContainers[index];

				// if the requested index is after the generated items pending insertion
				// then we can index into the _activeContainersInUse list by just
				// decremeneting the index by the countOfGeneratedItems.
				if (index >= insertGeneratedItemsAtIndex + countOfGeneratedItems)
					return activeContainers[index - countOfGeneratedItems];

				// decrement the index by the insertion index
				index -= insertGeneratedItemsAtIndex;

				// if the direction of the generation was backward then 
				// re-calculate the index so it is logically flipped
				if (this._direction == GeneratorDirection.Backward &&
					countOfGeneratedItems > 1)
					index = countOfGeneratedItems - (1 + index);

				return this._generatedContainers[index];

			}

				#endregion //GetActiveContainerReference	

				#region GetIndexOfActiveContainerReference

			internal int GetIndexOfActiveContainerReference( ContainerReference cr, bool isIndexBasedOnZOrder)
			{
				int count						= this._owner.CountOfActiveContainers;
				int countOfGeneratedItems		= this._generatedContainers.Count;
				int insertGeneratedItemsAtIndex = this.InsertAtIndexInActiveList;

				List<ContainerReference> activeContainers;

				if (isIndexBasedOnZOrder)
					activeContainers = this._owner.ActiveContainersInUseByZorder;
				else
					activeContainers = this._owner._activeContainersInUse;

				int index = activeContainers.IndexOf(cr);

				// see if the cr is in the active list
				if (index >= 0)
				{
					// if the index is >= the insert point then add the generated item count
					if ( index >= insertGeneratedItemsAtIndex )
						index += countOfGeneratedItems;

					return index;
				}

				index = activeContainers.IndexOf(cr);

				if (index < 0 )
					return -1;

				// if the direction of the generation was backward then 
				// re-calculate the index so it is logically flipped
				if (this._direction == GeneratorDirection.Backward &&
					countOfGeneratedItems > 1)
					index = countOfGeneratedItems - (1 + index);

				return index + insertGeneratedItemsAtIndex;

			}

				#endregion //GetIndexOfActiveContainerReference	



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


				#region OnContainerAddedToVisualTree

			internal void OnContainerAddedToVisualTree(ContainerReference cr)
			{
				if (cr.IsInVisualTree)
					throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_11"));

				if (cr != this._newlyCreatedContainerNotInVisualTree)
					throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_12"));

				cr.IsInVisualTree = true;

				this._newlyCreatedContainerNotInVisualTree = null;
			}

				#endregion //OnContainerAddedToVisualTree	

			#endregion //Methods	
        
			#region IDisposable Members

			public void Dispose()
			{
				if (this._newlyCreatedContainerNotInVisualTree != null)
					throw new InvalidOperationException(SR.GetString("LE_RecyclingGenerator_10"));

				this._owner._currentGenerator = null;
				this._owner.SetStatus( GeneratorStatus.ContainersGenerated );

				int count = this._generatedContainers.Count;

				if (count > 0)
				{
                    // JJD 3/17/10 - TFS28705 - Optimization
                    // Strip out generated items already in the active list
                    if (count == 1)
                    {
                        if (this._owner._activeContainersInUse.Contains(this._generatedContainers[0]))
                        {
                            this._generatedContainers.Clear();
                            count = 0;
                        }
                    }
                    else if (this._owner._activeContainersInUse.Count > 0)
                    {
                        HashSet existingActiveContainers = new HashSet();

                        existingActiveContainers.AddItems(this._owner._activeContainersInUse);

                        for (int i = count - 1; i >= 0; i--)
                        {
                            if (existingActiveContainers.Exists(this._generatedContainers[i]))
                            {
                                this._generatedContainers.RemoveAt(i);
                                count--;
                            }
                        }
                    }

					// if the direction of the generation was backward then reverse the 
					// generated items
					if (this._direction == GeneratorDirection.Backward &&
						count > 1)
						this._generatedContainers.Reverse();

					// insert the generated items at the appropriate index
                    if ( count > 0 )
					    this._owner._activeContainersInUse.InsertRange(this.InsertAtIndexInActiveList, this._generatedContainers);
				}



            }

			#endregion
		}

		#endregion //Generator class

		#region ZOrderComparer private class

		private class ZOrderComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				ContainerReference cr1 = x as ContainerReference;
				ContainerReference cr2 = y as ContainerReference;

				if (cr1 == cr2)
					return 0;

				if (cr1 == null || cr2 == null)
				{
					if (cr1 == null)
						return -1;
					else
						return 1;
				}

				int zIndex1 = cr1.ZIndex;
				int zIndex2 = cr2.ZIndex;

				if (zIndex1 < zIndex2)
					return -1;
				else
					if (zIndex1 > zIndex2)
						return 1;

				if (cr1._originalIndex < cr2._originalIndex)
					return -1;

				return 1;
			}

			#endregion
		}

		#endregion //ZOrderComparer private class	
    
		#region ContainerDiscardedEventArgs

		/// <summary>
		/// Arguments passed into the RecyclingItemContainerGenerator's ContainerDiscarded event.
		/// </summary>
		/// <seealso cref="RecyclingItemContainerGenerator"/>
		/// <seealso cref="RecyclingItemContainerGenerator.ContainerDiscarded"/>
		public class ContainerDiscardedEventArgs : RoutedEventArgs
		{
			private DependencyObject _container;

			internal ContainerDiscardedEventArgs(DependencyObject container)
			{
				this._container = container;
			}

			/// <summary>
			/// Returns the container that is being discared (read-only).
			/// </summary>
			public DependencyObject Container { get { return this._container; } }
		}

		#endregion //ContainerDiscardedEventArgs
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