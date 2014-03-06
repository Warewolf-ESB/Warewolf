using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Infragistics;
using Infragistics.Collections;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Represents a collection of <see cref="CalendarGroup"/> instances
	/// </summary>
	public class CalendarGroupCollection : ObservableCollectionExtended<CalendarGroup>
	{
		#region Member Variables

		private WeakReference _owner;
		private WeakReference _resources;
		private PropertyChangeListener<CalendarGroupCollection> _listener;
		private bool _hasCalendars;

		private HashSet<Resource> _hookedResources;
		private Dictionary<ResourceCalendar, object> _hookedCalendars;
		private InternalFlags _flags;
		private DeferredOperation _deferredOperation;
		private SingleItemCalendarGroupCollection _flatCalendarGroups;
		private CoreUtilities.ObservableTypedList<CalendarGroupBase> _baseCollectionWrapper;
		private ReadOnlyNotifyCollection<CalendarGroupBase> _mergedCalendarGroups;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupCollection"/>
		/// </summary>
		public CalendarGroupCollection()
            : base( false, true )
		{
			_deferredOperation = new DeferredOperation(new Action(this.ProcessPendingChanges));

			_hookedResources = new HashSet<Resource>();
			_hookedCalendars = new Dictionary<ResourceCalendar, object>();
			_listener = new PropertyChangeListener<CalendarGroupCollection>(this, OnSubObjectPropertyChanged);
			this.PropChangeListeners.Add(_listener, false);
		} 
		#endregion //Constructor

		#region Base class overrides

		#region NotifyItemsChanged
		/// <summary>
		/// Returns true to indicate that the collection wants to receive notifications as items are added and removed.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		} 
		#endregion // NotifyItemsChanged

		#region OnItemAdding
		/// <summary>
		/// Invoked when an item is about to be added to the collection.
		/// </summary>
		/// <param name="itemAdded">The item being added</param>
		protected override void OnItemAdding(CalendarGroup itemAdded)
		{
			if (itemAdded == null)
				throw new ArgumentNullException();

			base.OnItemAdding(itemAdded);
		} 
		#endregion // OnItemAdding

		#region OnItemAdded
		/// <summary>
		/// Invoked when an item is added to the collection
		/// </summary>
		/// <param name="itemAdded">The item added</param>
		protected override void OnItemAdded(CalendarGroup itemAdded)
		{
			if (null != this.Resources)
			{
				if (!this.GetFlag(InternalFlags.UpdateChildResources))
				{
					// make sure that the group updates its calendars 
					// based on the resources collection
					itemAdded.OnResourcesChanged(this.Resources, false);
				}

				// if during initialization we added calendars then set the flag
				if (itemAdded.Calendars.Count > 0)
					_hasCalendars = true;
			}

			base.OnItemAdded(itemAdded);
		} 
		#endregion // OnItemAdded

		#region OnItemRemoved
		/// <summary>
		/// Invoked when an item is removed from the collection
		/// </summary>
		/// <param name="itemRemoved">The item removed</param>
		protected override void OnItemRemoved(CalendarGroup itemRemoved)
		{
			// remove the references to the calendars so they are not rooted
			itemRemoved.OnResourcesChanged(null, true);

			base.OnItemRemoved(itemRemoved);
		} 
		#endregion // OnItemRemoved

		#endregion // Base class overrides

		#region Properties

		#region Internal

		#region BaseCollectionWrapper
		internal IList<CalendarGroupBase> BaseCollectionWrapper
		{
			get
			{
				if (_baseCollectionWrapper == null)
					_baseCollectionWrapper = new CoreUtilities.ObservableTypedList<CalendarGroupBase>(this);

				return _baseCollectionWrapper;
			}
		} 
		#endregion // BaseCollectionWrapper

		#region FlatCalendarGroups
		internal SingleItemCalendarGroupCollection FlatCalendarGroups
		{
			get 
			{
				if (_flatCalendarGroups == null)
				{
					_flatCalendarGroups = new SingleItemCalendarGroupCollection(this);

					// if the collections are not dirty then make sure this collection processes its 
					// state now
					if (!this.GetFlag(InternalFlags.UpdateChildResources | InternalFlags.VerifyResourcesAndCalendars))
						_flatCalendarGroups.VerifyState();
				}

				return _flatCalendarGroups; 
			}
		} 
		#endregion // FlatCalendarGroups

		#region IsProcessingPendingOperations
		internal bool IsProcessingPendingOperations
		{
			get
			{
				return GetFlag(InternalFlags.ProcessingPendingChanges);
			}
		}
		#endregion // IsProcessingPendingOperations

		#region MergedCalendarGroups
		internal IList<CalendarGroupBase> MergedCalendarGroups
		{
			get
			{
				if (_mergedCalendarGroups == null)
				{
					MergedCalendarGroup group = new MergedCalendarGroup(this);
					_mergedCalendarGroups = new ReadOnlyNotifyCollection<CalendarGroupBase>(new CalendarGroupBase[] { group });

					// if the collections are not dirty then make sure this collection processes its 
					// state now
					if (!this.GetFlag(InternalFlags.UpdateChildResources | InternalFlags.VerifyResourcesAndCalendars))
						group.VerifyState();
				}

				return _mergedCalendarGroups;
			}
		}
		#endregion // MergedCalendarGroups

		#region Owner
		internal XamScheduleDataManager Owner
		{
			get { return ScheduleUtilities.GetWeakReferenceTargetSafe(_owner) as XamScheduleDataManager; }
			set
			{
				XamScheduleDataManager current = this.Owner;

				if (current != value)
				{
					// keep a weak reference to the datamanager
					_owner = null == value ? null : new WeakReference(value);

					// if we have an old owner make sure we're not listening to its changes any more
					if (null != current)
						current.PropChangeListeners.Remove(_listener);

					// if we have a new one then hook into its changes
					if (null != value)
						value.PropChangeListeners.Add(_listener, true);

					this.Resources = value == null ? null : value.ResourceItems;
				}
			}
		}
		#endregion // Owner

		#region VisibleCalendars
		/// <summary>
		/// Returns a list of the visible ResourceCalendar instances
		/// </summary>
		internal ICollection<ResourceCalendar> VisibleCalendars
		{
			get
			{
				this.ProcessPendingChanges();
				return _hookedCalendars.Keys;
			}
		}
		#endregion // VisibleCalendars

		#region VisibleResources
		/// <summary>
		/// Returns a list of the Resource instances associated with the visible calendars
		/// </summary>
		internal HashSet<Resource> VisibleResources
		{
			get
			{
				this.ProcessPendingChanges();
				return _hookedResources;
			}
		} 
		#endregion // VisibleResources

		#endregion // Internal

		#region Private

		#region Resources
		private ResourceCollection Resources
		{
			get { return ScheduleUtilities.GetWeakReferenceTargetSafe(_resources) as ResourceCollection; }
			set
			{
				ResourceCollection resources = this.Resources;

				if (resources != value)
				{
					// store a reference to this
					_resources = null == value ? null : new WeakReference(value);

					// update the groups asynchronously
					this.EnqueueDeferredOperation(InternalFlags.UpdateChildResources);
				}
			}
		}
		#endregion // Resources

		#endregion // Private

		#endregion // Properties

		#region Methods

		#region Internal

		#region ProcessPendingChanges
		internal void ProcessPendingChanges()
		{
			// if we're already processing operations then exit
			if (this.IsProcessingPendingOperations)
				return;

			// cancel the pending operation if this is being invoked explicitly
			_deferredOperation.CancelPendingOperation();

			this.SetFlag(InternalFlags.ProcessingPendingChanges, true);

			try
			{
				if (this.GetFlag(InternalFlags.UpdateChildResources))
					this.UpdateChildResources(this.Resources, false);
				else if (this.GetFlag(InternalFlags.VerifyResourcesAndCalendars))
					this.UpdateChildResources(this.Resources, true);
			}
			finally
			{
				this.SetFlag(InternalFlags.ProcessingPendingChanges, false);
			}

			// now that the calendar groups are up to date let's refresh our list of visible resources
			this.VerifyResourceList();

			Debug.Assert(_flags == 0, "Some flags still set?");
		}
		#endregion // ProcessPendingChanges

		#endregion // Internal

		#region Private

		#region EnqueueDeferredOperation
		/// <summary>
		/// Helper routine that returns false if the operation could not/should not be deferred. Otherwise enqueues an async operation and returns true that the current operation should be deferred.
		/// </summary>
		/// <param name="flag">Indicates the operation to be enqueued</param>
		/// <returns></returns>
		private bool EnqueueDeferredOperation(InternalFlags flag)
		{
			Debug.Assert(!this.IsProcessingPendingOperations, "We're processing an pending operations and something wants to enqueue another change?");

			// if we're processing the pending changes then don't delay the processing
			if (this.IsProcessingPendingOperations)
				return false;

			// set the flag for the operation we are waiting for
			this.SetFlag(flag, true);

			_deferredOperation.StartAsyncOperation();

			return true;
		}
		#endregion // EnqueueDeferredOperation

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag(InternalFlags flag)
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		#region GetVisibleCalendars
		private IList<ResourceCalendar> GetVisibleCalendars(out HashSet<Resource> resources)
		{
			resources = new HashSet<Resource>();
			List<ResourceCalendar> calendars = new List<ResourceCalendar>();

			foreach (CalendarGroup group in this)
			{
				foreach (ResourceCalendar calendar in group.VisibleCalendars)
				{
					calendars.Add(calendar);

					Resource res = calendar.OwningResource;

					if (res != null)
					{
						resources.Add(res);
					}
				}
			}

			return calendars;
		}
		#endregion // GetVisibleCalendars

		#region OnSubObjectPropertyChanged

		private static void OnSubObjectPropertyChanged(CalendarGroupCollection collection, object sender, string propName, object extraInfo)
		{
			if (sender is XamScheduleDataManager && propName == "ResourceItems")
			{
				XamScheduleDataManager dm = sender as XamScheduleDataManager;
				Debug.Assert(dm == collection.Owner);
				collection.Resources = dm.ResourceItems;
			}
			else if ( sender is ResourceCollection || sender is ObservableCollectionExtended<ResourceCalendar> )
			{
				// AS 10/5/10 TFS55791
				// A calendar was added/removed from the calendars collection of a calendar group so cache that 
				// we need to rebuild/verify the resource list and that we need to check if there are any dup
				// calendars.
				//
				if ( sender is CalendarGroupCalendarCollection )
					collection.SetFlag(InternalFlags.VerifyResourceList | InternalFlags.CalendarsChanged, true);

				// if we haven't found any calendars yet then reinitialize (i.e. rebuild from the 
				// calendar ids). but if we have then just make sure we're not referencing a calendar or 
				// resource that was removed
				if ( !collection.IsProcessingPendingOperations )
					collection.EnqueueDeferredOperation(collection._hasCalendars ? InternalFlags.VerifyResourcesAndCalendars : InternalFlags.UpdateChildResources);
			}
			else if ( sender is ResourceCalendar )
			{
				if ( string.IsNullOrEmpty(propName) || propName == "IsVisibleResolved" )
				{
					collection.EnqueueDeferredOperation(InternalFlags.VerifyResourcesAndCalendars);
				}
				else if ( propName == "BaseColor" )
				{
					// AS 10/5/10 TFS55791
					// We don't need to reverify the calendars when the color changes - just 
					// reverify the brush providers which happens in the verifyresources.
					//
					// JJD 9/1/10 - TFS37171
					// Re-verify resources if the base color changed also
					collection.EnqueueDeferredOperation(InternalFlags.VerifyResourceList);
				}
			}
		}
		#endregion //OnSubObjectPropertyChanged

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region UpdateChildResources
		/// <summary>
		/// Updates the CalendarGroup instances in the collection based on the resource collection.
		/// </summary>
		/// <param name="resources">The collection used to update the CalendarGroups</param>
		/// <param name="verifyCalendarsOnly">True if the calendars should not be cleared and reobtained based on the calendar ids</param>
		private void UpdateChildResources(ResourceCollection resources, bool verifyCalendarsOnly)
		{
			InternalFlags flag = verifyCalendarsOnly ? InternalFlags.VerifyResourcesAndCalendars : InternalFlags.UpdateChildResources;

			this.SetFlag(flag, false);

			// if we are reinitializing and not just verifying then we can disable the verification
			// since we'll do that for any group that doesn't have an initialcalendarids and any one 
			// that does will automatically do the verification
			if (flag == InternalFlags.UpdateChildResources)
				this.SetFlag(InternalFlags.VerifyResourcesAndCalendars, false);

			// we only want to clear the hasCalendars state when reinitializing
			// and not when verifying since the calendars could have been 
			// explicitly removed
			if (!verifyCalendarsOnly)
				_hasCalendars = false;

			foreach (CalendarGroup group in this)
			{
				group.OnResourcesChanged(resources, verifyCalendarsOnly);

				if (group.Calendars.Count > 0)
					_hasCalendars = true;
			}

			// AS 10/5/10 TFS55791
			if ( this.GetFlag(InternalFlags.CalendarsChanged) )
			{
				// moved this down since the groups may have just calendar ids in which case they
				// could still have duplicates before we have them process
				#region Remove duplicate calendars

				// we cannot allow the same calendar to be in multiple groups
				var calendarToGroup = new Dictionary<ResourceCalendar, CalendarGroup>();

				// 1st pass - we want to give preference to keeping the calendar in the 
				// group that had it already so walk over all and build a list of them
				foreach ( CalendarGroup group in this )
				{
					var calendars = group.Calendars;

					// originally we were going to try and give preference to keeping 
					// calendars that were already in the visible calendars of a group
					// in thinking that those were displayed to the end user and should
					// stay where they are but that would not be consistent since the 
					// visible calendars could have been populated by virtue of someone 
					// requesting the visible calendars collection so we wouldn't know 
					// which group had shown it previously unless we tracked that here 
					// in the group which we could do in the future if this becomes an 
					// issue
					for ( int i = calendars.Count - 1; i >= 0; i-- )
					{
						var calendar = calendars[i];

						if ( calendarToGroup.ContainsKey(calendar) )
							group.Calendars.RemoveAt(i);
						else
							calendarToGroup[calendar] = group;
					}
				}
				#endregion // Remove duplicate calendars

				this.SetFlag(InternalFlags.CalendarsChanged, false);
			}

			Debug.Assert(!_hasCalendars || resources != null, "Don't think we should have calendars if the resources are null");

			if (resources == null)
				_hasCalendars = false;

			// if we have flattened calendars then verify its state
			if (null != _flatCalendarGroups)
				_flatCalendarGroups.VerifyState();

			if (null != _mergedCalendarGroups)
				((MergedCalendarGroup)_mergedCalendarGroups[0]).VerifyState();
		}
		#endregion // UpdateChildResources

		#region VerifyResourceList
		/// <summary>
		/// Helper routine to maintain a hashset of the Resources for which a VisibleCalendar is displayed.
		/// </summary>
		private void VerifyResourceList()
		{
			// if changes are happening while we are processing the pending operations then
			// just ignore them. we will verify this after the pending operation processing 
			// is complete
			if (this.IsProcessingPendingOperations)
				return;

			this.SetFlag(InternalFlags.VerifyResourceList, false);

			var oldResources = _hookedResources;
			var oldCalendars = _hookedCalendars;
			XamScheduleDataManager dm = this.Owner;

			HashSet<Resource> newResources;
			var newCalendars = this.GetVisibleCalendars(out newResources);

			_hookedResources = newResources;
			var newCalendarTable = _hookedCalendars = new Dictionary<ResourceCalendar,object>();

			#region Process Added/Removed Calendars

			bool calendarsChanged = false;

			#region Reuse previous calendar info
			for (int i = newCalendars.Count - 1; i >= 0; i--)
			{
				ResourceCalendar calendar = newCalendars[i];

				object oldValue;

				// if we were already using it before then just reuse the same brush manager 
				// instance unless the base color has changed
				bool hasOldCalendar = oldCalendars.TryGetValue(calendar, out oldValue);

				if (hasOldCalendar && !calendar.NeedsNewBrushProvider)
				{
					// remove it so the only thing left in the old calendars are
					// the ones that are no longer being used
					oldCalendars.Remove(calendar);

					// reuse the same brush provider
					newCalendarTable[calendar] = oldValue;

					// clear the slot in the new calendar list so we don't
					// try to register it as a new calendar
					newCalendars[i] = null;
				}
				else if (newCalendarTable.ContainsKey(calendar))
				{
					// it's not in the old but if its in the new table then 
					// this must be a dupe so don't do anything with it
					newCalendars[i] = null;
				}
				else
				{
					// this must be a new calendar since it wasn't in the 
					// old calendars and wasn't one of the old calendars that
					// were already processed
					if (!hasOldCalendar)
					{
						calendarsChanged = true;
					}
				}
			} 
			#endregion // Reuse previous calendar info

			if (dm != null)
			{
                CalendarColorScheme colorScheme = dm.ColorSchemeResolved;

                bool wasBeginCalled = false;

                try
                {
                    #region Unregister old calendars

                    if (oldCalendars.Count > 0)
                    {
                        if (wasBeginCalled == false)
                        {
                            wasBeginCalled = true;
                            colorScheme.BeginProviderAssigments(this);
                        }
                        foreach (ResourceCalendar resourceCalendar in oldCalendars.Keys)
                        {
                            // unregister the brush provider for the calendars no longer used
                            colorScheme.UnassignProvider(resourceCalendar);
                        }
                    }
                    #endregion // Unregister old calendars

                    #region Register new calendars
                    for (int i = 0, count = newCalendars.Count; i < count; i++)
                    {
                        ResourceCalendar calendar = newCalendars[i];

                        if (null != calendar)
                        {
							// we could have a calendar still in the old collection because 
							// its provider assignment has changed so remove it from the 
							// old calendars so we don't think we had old calendar left over
							oldCalendars.Remove(calendar);

							object token = calendar.BrushProviderToken;

                            if (wasBeginCalled == false)
                            {
                                wasBeginCalled = true;
                                colorScheme.BeginProviderAssigments(this);
                            }

                            //assign a brush provider to the calendar
                            colorScheme.AssignProvider(calendar);

                            // set the token in the dictionary so we have a hard reference to it.
                            // This keeps it alive until clear it out
                            newCalendarTable[calendar] = token;
                        }
                    }
                    #endregion // Register new calendars
                }
                finally
                {
                    if (wasBeginCalled )
                        colorScheme.EndProviderAssigments();
                }
			}

			// if we have any that were removed then the collection changed
			if (oldCalendars.Count > 0)
				calendarsChanged = true;

			#endregion // Process Added/Removed Calendars

			#region Process Added/Removed Resources

			bool resourcesChanged = false;

			foreach (Resource resource in newResources)
			{
				// it wasn't in the old list so it must be new
				if (!oldResources.Remove(resource))
				{
					resourcesChanged = true;
					resource.PropChangeListeners.Add(this.PropChangeListeners, true);
				}
			}

			if (oldResources.Count > 0)
				resourcesChanged = true;

			// whatever is left is what was removed
			foreach (Resource resource in oldResources)
			{
				resource.PropChangeListeners.Remove(this.PropChangeListeners);
			}

			#endregion // Process Added/Removed Resources

			#region Change Notifications

			// if we removed/added some then send a change notification so the controls can know
			if (resourcesChanged)
				this.PropChangeListeners.OnPropertyValueChanged(this, "VisibleResources", null);

			// if we removed/added some then send a change notification so the controls can know
			if (calendarsChanged)
				this.PropChangeListeners.OnPropertyValueChanged(this, "VisibleCalendars", null);

			#endregion // Change Notifications
		}
		#endregion // VerifyResourceList

		#endregion // Private

		#endregion // Methods

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : byte
		{
			/// <summary>
			/// in the middle of processing deferred changes
			/// </summary>
			ProcessingPendingChanges = 0x1,

			/// <summary>
			/// we need to rebuild the list of resources
			/// </summary>
			VerifyResourceList = 0x2,

			/// <summary>
			/// We need to reinitialize all the groups with the resources
			/// </summary>
			UpdateChildResources = 0x4,

			/// <summary>
			/// We need to check for any resources/calendars that have been removed.
			/// </summary>
			VerifyResourcesAndCalendars = 0x8,

			/// <summary>
			/// Notification that the Resources collection received a reset.
			/// </summary>
			CalendarsChanged = 0x10,
		}
		#endregion // InternalFlags enum
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