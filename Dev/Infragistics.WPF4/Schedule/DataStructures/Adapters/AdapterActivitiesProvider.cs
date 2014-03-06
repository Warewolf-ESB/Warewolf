//#define DEBUG_ACTIVITY





using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using Infragistics.Collections;
using System.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class AdapterActivitiesProvider : object
		, IPropertyChangeListener
		, ISupportPropertyChangeNotifications
		, IList<ActivityBase>
	{
		#region Member Variables

		private static NotifyCollectionChangedEventArgs ResetArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

		private CalendarGroupBase _activityOwner;
		private ISupportRecycling _adapter;

		private Predicate<ActivityBase> _filter;
		private DateRange[] _ranges;
		private bool _isEnabled = true;
		private bool _isSortDirty;

		private bool _isVerified;
		private PropertyChangeListenerList _listenerList;
		private int _listenerCount;
		private ActivityQueryResult _queryResult;
		private IComparer<ActivityBase> _sortComparer;

		private IList _activities;
		private IList<ActivityBase> _typedActivities;

		private ActivityTypes _activityTypes;
		private ObservableCollectionExtended<ActivityBase> _extraItems;
		private ObservableCollectionExtended<ActivityBase> _filteredOutItems;
		private PropertyChangeListener<AdapterActivitiesProvider> _filterListener;

		private TimeZoneInfoProvider _tzProvider;
		private bool _isProcessingCollectionChange;
		private Dictionary<ActivityBase, object> _pendingActivityChangeNotifications;

		private HashSet<ActivityBase> _pendingResort;
		private DeferredOperation _deferredOperation;

		#endregion // Member Variables

		#region Constructor
		internal AdapterActivitiesProvider(ScheduleControlBase control, ISupportRecycling adapter, CalendarGroupBase calendarGroup)
		{
			_activityOwner = calendarGroup;
			_adapter = adapter;
			_extraItems = control.ExtraActivities;
			_filteredOutItems = control.FilteredOutActivities;
			_tzProvider = control.TimeZoneInfoProviderResolved;
		} 
		#endregion // Constructor

		#region Properties

		#region ActivityFilter
		internal Predicate<ActivityBase> ActivityFilter
		{
			get { return _filter; }
			set
			{
				if (value != _filter)
				{
					_filter = value;
					this.DirtyActivities();
					this.NotifyActivitiesChanged();
				}
			}
		} 
		#endregion // ActivityFilter

		#region ActivityOwner
		internal CalendarGroupBase ActivityOwner
		{
			get { return _activityOwner; }
		}
		#endregion // ActivityOwner

		#region ActivityTypes
		internal ActivityTypes ActivityTypes
		{
			get { return _activityTypes; }
			set
			{
				if (value != _activityTypes)
				{
					_activityTypes = value;
					this.ReleaseActivities();
				}
			}
		} 
		#endregion // ActivityTypes

		#region IsEnabled
		internal bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (value != _isEnabled)
				{
					_isEnabled = value;
					this.DirtyActivities();

					if (!value)
						this.ReleaseActivities();
					else
						this.NotifyActivitiesChanged();
				}
			}
		} 
		#endregion // IsEnabled

		#region Ranges
		internal DateRange[] Ranges
		{
			get { return _ranges; }
			set
			{
				if (value != _ranges)
				{
					_ranges = value;
					this.ReleaseActivities();
				}
			}
		}
		#endregion // Ranges

		#region SortComparer
		internal IComparer<ActivityBase> SortComparer
		{
			get { return _sortComparer; }
			set
			{
				if (value != _sortComparer)
				{
					_sortComparer = value;
					this.InvalidateSort(null);
				}
			}
		} 
		#endregion // SortComparer

		#region TypedActivities
		private IList<ActivityBase> TypedActivities
		{
			get
			{
				if (_typedActivities == null)
				{
					Debug.Assert(null != _activities);
					_typedActivities = new CoreUtilities.TypedList<ActivityBase>(_activities);
				}

				return _typedActivities;
			}
		}
		#endregion // TypedActivities

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region CompareCalendar
		internal int CompareCalendar(ResourceCalendar cal1, ResourceCalendar cal2)
		{
			IList<ResourceCalendar> calendars = _activityOwner.VisibleCalendars;

			return calendars.IndexOf(cal1).CompareTo(calendars.IndexOf(cal2));
		}
		#endregion // CompareCalendar

		#region CouldContainActivity
		internal bool CouldContainActivity(ActivityBase activity)
		{
			return this.CouldContainActivity(activity, false);
		}

		private bool CouldContainActivity(ActivityBase activity, bool calledFromFilter)
		{
			if (_ranges == null || _activityTypes == 0 || _activityOwner == null || activity == null)
				return false;

			if ((ScheduleUtilities.GetActivityTypes(activity.ActivityType) & _activityTypes) == 0)
				return false;

			if (!calledFromFilter && !this.Filter(activity, null))
				return false;

			DateRange activityRange = new DateRange(activity.Start, activity.End);

			// AS 9/20/10 TFS42810
			// if the activity is timezoneneutral then these values are actually
			// relative to the local zone but we're comparing to utc time ranges that 
			// the activity provider represents
			if (activity.IsTimeZoneNeutral)
			{
				activityRange = ScheduleUtilities.ConvertToUtc(_tzProvider.LocalToken, activityRange);
			}

			bool intersectsRange = false;

			for (int i = 0; i < _ranges.Length; i++)
			{
				DateRange range = _ranges[i];
				range.End = ScheduleUtilities.GetNonInclusiveEnd(range.End);

				if (range.IntersectsWith(activityRange))
				{
					intersectsRange = true;
					break;
				}
			}

			if (!intersectsRange)
				return false;

			if (!_activityOwner.Contains(activity.OwningCalendar))
				return false;

			return true;
		}
		#endregion // CouldContainActivity

		#region InvalidateSort
		internal void InvalidateSort(ActivityBase activity)
		{
			this.InvalidateSort(activity, true);
		}

		private void InvalidateSort(ActivityBase activity, bool defer)
		{
			if (_isVerified && !_isSortDirty)
			{
				if (activity != null && _sortComparer != null)
				{
					int oldIndex = _activities.IndexOf(activity);

					Debug.Assert(oldIndex >= 0, "Why are we being asked to invalidate the sort for an item that isn't in the list?");

					// if its still in the list
					if (oldIndex >= 0)
					{
						if (defer)
						{
							if (_pendingResort == null)
							{
								_pendingResort = new HashSet<ActivityBase>();
								this.BeginAsyncOperation();
							}

							_pendingResort.Add(activity);
							return;
						}

						int newIndex = ScheduleUtilities.BinarySearch(this.TypedActivities, activity, _sortComparer, true);

						// if it can stay where it is then just exit
						if (newIndex == oldIndex)
							return;

						if (newIndex < 0)
							newIndex = ~newIndex;

						_activities.RemoveAt(oldIndex);

						// if the calculated index was after the existing spot then decrement to account for the remove
						if (newIndex > oldIndex)
							newIndex--;

						_activities.Insert(newIndex, activity);

						// don't dirty the sort if we processed it synchronously
						return;
					}
				}

				_pendingResort = null;
				_pendingActivityChangeNotifications = null;
				_isSortDirty = true;
			}
		}
		#endregion // InvalidateSort

		#region OnAttachedElementChanged
		internal void OnAttachedElementChanged()
		{
			// the query result is only used when we have an element attached with the adapter
			// so if the adapter is attached we need to mark the collection dirty so we reget
			// the query and if the element is released we should release the activities
			if (_adapter.AttachedElement == null)
				this.ReleaseActivities();
			else
				this.DirtyActivities();
		} 
		#endregion // OnAttachedElementChanged

		#region ProcessPendingOperations
		internal void ProcessPendingOperations()
		{
			if ( _deferredOperation != null && !_isProcessingCollectionChange )
			{
				_deferredOperation.InvokePendingOperation();
			}
		} 
		#endregion // ProcessPendingOperations

		#region ReferencesCalendar
		internal bool ReferencesCalendar(ResourceCalendar calendar)
		{
			if (_activityOwner != null)
				return _activityOwner.Contains(calendar);

			return false;
		}
		#endregion // ReferencesCalendar

		#region VerifySort
		internal void VerifySort()
		{
			this.VerifyActivities();

			this.ProcessAsyncOperation();

			if (_isSortDirty)
			{
				_isSortDirty = false;

				if (_activities.Count > 1 && _sortComparer != null)
				{
					HashSparseArray sparse = _activities as HashSparseArray;
					sparse.SortGeneric(_sortComparer);
				}
			}
		}
		#endregion // VerifySort

		#endregion // Internal Methods

		#region Private Methods

		#region AddPendingActivityChanged
		private void AddPendingActivityChanged(ActivityBase activity, string property)
		{
			if (_pendingActivityChangeNotifications == null)
			{
				_pendingActivityChangeNotifications = new Dictionary<ActivityBase, object>();
				this.BeginAsyncOperation();
			}

			object props;

			if (!_pendingActivityChangeNotifications.TryGetValue(activity, out props))
			{
				_pendingActivityChangeNotifications[activity] = string.IsNullOrEmpty(property) ? null : property;
			}
			else
			{
				if (props != null)
				{
					if (string.IsNullOrEmpty(property))
					{
						_pendingActivityChangeNotifications[activity] = null;
					}
					else
					{
						List<string> properties = props as List<string>;

						if (null != properties)
							properties.Add(property);
						else
						{
							Debug.Assert(property is string);
							properties = new List<string>();
							properties.Add(props as string);
							properties.Add(property);

							_pendingActivityChangeNotifications[activity] = properties;
						}
					}
				}
			}
		}
		#endregion // AddPendingActivityChanged 

		#region BeginAsyncOperation
		private void BeginAsyncOperation()
		{
			if (_deferredOperation == null)
				_deferredOperation = new DeferredOperation(this.ProcessAsyncOperation);

			_deferredOperation.StartAsyncOperation();
		}
		#endregion // BeginAsyncOperation

		#region DirtyActivities
		private void DirtyActivities()
		{
			_isVerified = false;
			_pendingResort = null;
			_pendingActivityChangeNotifications = null;
		}
		#endregion // DirtyActivities

		#region Filter
		private bool Filter(ActivityBase activity, bool? isExtraItem, bool ignoreFilteredItems = false)
		{
			Debug.Assert(null != activity, "A null activity?");

			if (activity == null)
				return false;

			if (! activity.IsVisibleResolved )
				return false;

			if (null != _filter && !_filter(activity))
				return false;

			// if this is an extra item (or we're not sure) then check its calendar, 
			// date range and activity type
			if (isExtraItem != false && !this.CouldContainActivity(activity, true))
			{
				return false;
			}

			if ( !ignoreFilteredItems && _filteredOutItems.Contains(activity) )
				return false;

			return true;
		} 
		#endregion // Filter

		#region InitializeQueryResult
		private void InitializeQueryResult()
		{
			// if we haven't obtained the query and we have an activity owner then query now
			if (_queryResult == null && _activityOwner != null && _ranges != null && _isEnabled)
			{
				FrameworkElement element = _adapter.AttachedElement;

				if (null != element)
				{
					XamScheduleDataManager dm = ScheduleUtilities.GetDataManager(element);
					Debug.Assert(dm != null, "Cannot obtain the query result without a reference to the containing control");

					if (null != dm)
					{
						var query = new ActivityQuery( _activityTypes, _ranges, new ReadOnlyCollection<ResourceCalendar>( _activityOwner.VisibleCalendars ) );
						_queryResult = dm.GetActivities(query);

						ScheduleUtilities.AddListener(_queryResult, this, true);

						// when its reenabled then we can start listening again
						ScheduleUtilities.AddListener(_extraItems, this, true);

						if ( null == _filterListener )
							_filterListener = new PropertyChangeListener<AdapterActivitiesProvider>(this, OnFilteredItemChange, false);

						ScheduleUtilities.AddListener(_filteredOutItems, _filterListener, true);

						ISupportPropertyChangeNotifications notifier = _activityOwner as ISupportPropertyChangeNotifications;

						if (notifier != null)
							notifier.AddListener(this, true);
					}
				}
			}
		}
		#endregion // InitializeQueryResult

		#region InsertItemHelper
		private int InsertItemHelper(ActivityBase activity)
		{
			int newIndex;

			this.ProcessAsyncOperation();

			// if the sort is dirty or we don't have a sort comparer then just stick it at the end
			if (_isSortDirty || _sortComparer == null)
			{
				newIndex = _activities.Count;
				_activities.Add(activity);
			}
			else
			{
				newIndex = ScheduleUtilities.BinarySearch(this.TypedActivities, activity, _sortComparer, false);

				if (newIndex < 0)
					newIndex = ~newIndex;

				_activities.Insert(newIndex, activity);
			}

			return newIndex;
		}
		#endregion // InsertItemHelper

		#region NotifyActivitiesChanged
		private void NotifyActivitiesChanged()
		{
			if (_listenerCount > 0)
				_listenerList.OnCollectionChanged(this, ResetArgs);
		}
		#endregion // NotifyActivitiesChanged

		#region OnActivityCollectionChange
		private void OnActivityCollectionChange(NotifyCollectionChangedEventArgs collectionArgs, bool isExtraItem)
		{
			if (collectionArgs != null)
			{
				// if we're already dirty or we need to dirty the collection based on the 
				// the event args (and what item was added/removed)...
				if (!_isVerified || ProcessCollectionChange(collectionArgs, isExtraItem))
				{
					this.NotifyActivitiesChanged();
				}

				if (null != _pendingActivityChangeNotifications)
					this.ProcessPendingActivityChanges();
			}
		} 
		#endregion // OnActivityCollectionChange

		#region OnActivityRemoved
		/// <summary>
		/// Invoked when an activity has been removed from the exposed list of activities.
		/// </summary>
		/// <param name="activity">Activity being removed</param>
		private void OnActivityRemoved(ActivityBase activity)
		{
			if (null != _pendingResort)
				_pendingResort.Remove(activity);

			if (null != _pendingActivityChangeNotifications)
				_pendingActivityChangeNotifications.Remove(activity);
		}
		#endregion // OnActivityRemoved

		#region OnFilteredItemChange
		private static void OnFilteredItemChange( AdapterActivitiesProvider owner, object dataItem, string property, object extraInfo )
		{
			owner.OnFilteredItemChange(dataItem, property, extraInfo);
		}

		private void OnFilteredItemChange( object dataItem, string property, object extraInfo )
		{
			if ( dataItem == _filteredOutItems )
			{
				var collectionArgs = extraInfo as NotifyCollectionChangedEventArgs;

				if ( null != collectionArgs )
				{
					var action = collectionArgs.Action;
					Debug.Assert(action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Remove, "Only expecting adds or removes");

					bool notifyChange = false;

					if ( !_isVerified || action == NotifyCollectionChangedAction.Reset )
					{
						this.DirtyActivities();
						notifyChange = true;
					}
					else
					{
						bool wasProcessing = _isProcessingCollectionChange;

						try
						{
							_isProcessingCollectionChange = true;

							if ( action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Replace )
							{
								foreach ( object item in collectionArgs.NewItems )
								{
									var activity = item as ActivityBase;

									if ( this.RemoveItemHelper(activity) )
										notifyChange = true;
								}
							}

							if ( action == NotifyCollectionChangedAction.Remove || action == NotifyCollectionChangedAction.Replace )
							{
								foreach ( object item in collectionArgs.OldItems )
								{
									var activity = item as ActivityBase;

									// it seems like it should be included and currently isn't. however it is possible that 
									// during the propogation of this change the item has been removed from the source 
									// collection and we pulled it out of our list. so we need to verify that its 
									// still in the one of the source collections
									bool isInSource = _extraItems.Contains(activity) || (_queryResult != null && _queryResult.Activities.Contains(activity));

									if ( !isInSource )
										continue;

									// if it should be in the list then add it in now
									if ( this.Filter(activity, null, true) )
									{
										this.InsertItemHelper(activity);
										notifyChange = true;
									}
								}
							}
						}
						finally
						{
							_isProcessingCollectionChange = wasProcessing;
						}
					}

					if ( notifyChange )
						this.NotifyActivitiesChanged();
 				}
			}
		}
		#endregion // OnFilteredItemChange

		#region ProcessActivityChange
		private bool ProcessActivityChange(ActivityBase activity)
		{
			bool notifyItemChange = true;

			if (_isVerified && activity != null && _queryResult != null)
			{
				// the extra items collection should be relatively small
				bool? isExtraItem = _extraItems.IndexOf(activity) >= 0;
				bool isIn = this.Filter(activity, isExtraItem);

				int index = _activities.IndexOf(activity);

				// if the item was in the list and now isn't or is in the list and wasn't then...
				if (isIn != index >= 0)
				{
					// if its not supposed to be in then just remove it
					if (!isIn)
					{
						_activities.RemoveAt(index);

						this.OnActivityRemoved(activity);

						if (_listenerCount > 0)
							_listenerList.OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, activity, index));
					}
					else
					{
						// it seems like it should be included and currently isn't. however it is possible that 
						// during the propogation of this change the item has been removed from the source 
						// collection and we pulled it out of our list. so we need to verify that its 
						// still in the one of the source collections
						bool isInSource = _extraItems.Contains(activity) || (_queryResult != null && _queryResult.Activities.Contains(activity));

						if (isInSource)
						{
							int newIndex = this.InsertItemHelper(activity);

							if (_listenerCount > 0)
								_listenerList.OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, activity, newIndex));
						}
					}

					notifyItemChange = false;
				}
				else if (!isIn)
				{
					// if it's not in the list (and wasn't) then don't send a notification
					notifyItemChange = false;
				}
			}

			return notifyItemChange;
		}
		#endregion // ProcessActivityChange

		#region ProcessAsyncOperation
		private void ProcessAsyncOperation()
		{
			this.ProcessPendingActivityChanges();

			this.ProcessPendingResort();
		}
		#endregion // ProcessAsyncOperation

		#region ProcessCollectionChange
		/// <summary>
		/// Processes a collection change and indicates if the caller should send a reset notification.
		/// </summary>
		/// <param name="args">Collection change information</param>
		/// <param name="isExtraItem">A collection change for the extra items</param>
		/// <returns></returns>
		private bool ProcessCollectionChange(NotifyCollectionChangedEventArgs args, bool isExtraItem)
		{
			Debug.Assert(_activities != null);

			if (args == null)
				return false;

			if (!_isVerified)
				return true;

			NotifyCollectionChangedAction action = args.Action;

			if (action == NotifyCollectionChangedAction.Reset)
			{
				this.DirtyActivities();
				return true;
			}

			bool changed = false;
			bool wasProcessing = _isProcessingCollectionChange;

			try
			{
				_isProcessingCollectionChange = true;

				if (action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Replace)
				{
					foreach (object item in args.NewItems)
					{
						ActivityBase activity = item as ActivityBase;

						if (activity == null)
						{
							this.DirtyActivities();
							return true;
						}

						// if it should be in the list then add it in now
						if (this.Filter(activity, isExtraItem))
						{
							this.InsertItemHelper(activity);
							changed = true;
						}
					}
				}

				if (action == NotifyCollectionChangedAction.Remove || action == NotifyCollectionChangedAction.Replace)
				{
					foreach (object item in args.OldItems)
					{
						ActivityBase activity = item as ActivityBase;

						if (activity == null)
						{
							this.DirtyActivities();
							return true;
						}

						if ( this.RemoveItemHelper(activity) )
							changed = true;
					}
				}
			}
			finally
			{
				_isProcessingCollectionChange = wasProcessing;
			}

			return changed;
		}
		#endregion // ProcessCollectionChange

		#region ProcessPendingActivityChanges
		private void ProcessPendingActivityChanges()
		{
			if (!_isProcessingCollectionChange && _pendingActivityChangeNotifications != null)
			{
				var activities = _pendingActivityChangeNotifications;
				_pendingActivityChangeNotifications = null;

				foreach (var item in activities)
				{
					ActivityBase activity = item.Key;
					bool notifyItemChange = this.ProcessActivityChange(activity);

					if (notifyItemChange && _listenerCount > 0)
					{
						if (item.Value == null || item.Value is string)
							_listenerList.OnPropertyValueChanged(activity, item.Value as string, null);
						else
						{
							var props = item.Value as ICollection<string>;
							Debug.Assert(null != props);

							foreach (string prop in props)
								_listenerList.OnPropertyValueChanged(activity, prop, null);
						}
					}
				}
			}
		}
		#endregion // ProcessPendingActivityChanges

		#region ProcessPendingResort
		private void ProcessPendingResort()
		{
			if (_pendingResort != null)
			{
				ActivityBase[] activities = _pendingResort.ToArray();
				_pendingResort = null;

				if (_isSortDirty == false)
				{
					foreach (ActivityBase activity in activities)
					{
						this.InvalidateSort(activity, false);
					}
				}
			}
		}
		#endregion // ProcessPendingResort

		#region RebuildActivityList
		private void RebuildActivityList()
		{
			_pendingResort = null;
			_pendingActivityChangeNotifications = null;

			ActivityQueryResult queryResult = _queryResult;

			if (queryResult == null && _isEnabled)
			{
				this.InitializeQueryResult();
				queryResult = _queryResult;
			}

			if (queryResult == null)
			{
				_isSortDirty = false;
				_activities = new ActivityBase[0];
			}
			else
			{
				HashSparseArray activities = new HashSparseArray();

				foreach (ActivityBase activity in queryResult.Activities)
				{
					if (this.Filter(activity, false))
						activities.Add(activity);
				}

				foreach (ActivityBase activity in _extraItems)
				{
					if (this.Filter(activity, true))
						activities.Add(activity);
				}

				_activities = activities;
				_isSortDirty = activities.Count > 1;
			}

			_typedActivities = null;
			_isVerified = true;
		}
		#endregion // RebuildActivityList

		#region ReleaseActivities
		private void ReleaseActivities()
		{
			if (null != _queryResult)
			{
				this.DirtyActivities();
				_activities.Clear();

				// if we got here then we have generated the activity query result and were watching the owner
				// for changes in its calendars. if the calendars change then we want to unhook and clear the 
				// results so we requery on the next request
				ISupportPropertyChangeNotifications notifier = _activityOwner as ISupportPropertyChangeNotifications;

				if (null != notifier)
					notifier.RemoveListener(this);

				// we don't need to listen to extra items changes until this object is being used
				ScheduleUtilities.RemoveListener(_extraItems, this);

				if (null != _filterListener)
					ScheduleUtilities.RemoveListener(_filteredOutItems, _filterListener);

				ScheduleUtilities.RemoveListener(_queryResult, this);
				_queryResult = null;

				// raise reset notification to let any listeners know
				this.NotifyActivitiesChanged();
			}
		}
		#endregion // ReleaseActivities

		#region RemoveItemHelper
		private bool RemoveItemHelper( ActivityBase activity )
		{
			int index = _activities.IndexOf(activity);

			if ( index >= 0 )
			{
				_activities.RemoveAt(index);

				this.OnActivityRemoved(activity);
				return true;
			}

			return false;
		}
		#endregion // RemoveItemHelper

		#region VerifyActivities
		private void VerifyActivities()
		{
			if (!_isVerified)
				this.RebuildActivityList();
		}
		#endregion // VerifyActivities

		#endregion // Private Methods

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			var calendars = dataItem as CalendarGroupCalendarCollection;





			if (calendars != null)
			{
				if (calendars.IsVisibleCalendars)
				{
					// if we got here then we have generated the activity query result and were watching the owner
					// for changes in its calendars. if the calendars change then we want to unhook and clear the 
					// results so we requery on the next request
					this.ReleaseActivities();
				}
			}
			else
			{
				ActivityBase activity = dataItem as ActivityBase;

				if (activity != null)
				{
					if (_isVerified)
					{
						// if we're in a clean state but we're processing add/remove notifications
						// then do not synchronously process the property changes since we could 
						// inadvertantly add an item into the collection while we were adding/removing 
						// items
						// also since several properties could be changed - e.g. start then end - we 
						// need to defer all processing of the activity property changes
						this.AddPendingActivityChanged(activity, property);
					}
					else
					{
						if (_listenerCount > 0)
							_listenerList.OnPropertyValueChanged(dataItem, property, extraInfo);
					}
				}
				else 
				{
					var collectionArgs = extraInfo as NotifyCollectionChangedEventArgs;

					if (null != collectionArgs)
					{
						bool? isExtraItems;

						if (_queryResult != null && dataItem == _queryResult.Activities)
							isExtraItems = false;
						else if (dataItem == _extraItems)
							isExtraItems = true;
						else
							isExtraItems = null;
						
						if (isExtraItems.HasValue)
							OnActivityCollectionChange(collectionArgs, isExtraItems.Value);
					}
				}
			}
		}
		#endregion //ITypedPropertyChangeListener<object,string> Members

		#region ISupportPropertyChangeNotifications Interface Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			if (null == _listenerList)
				_listenerList = new PropertyChangeListenerList();

			_listenerCount++;
			_listenerList.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			_listenerCount--;
			if (null != _listenerList)
				_listenerList.Remove(listener);
		}
		#endregion // ISupportPropertyChangeNotifications Interface Implementation

		#region IList<ActivityBase> Members

		int IList<ActivityBase>.IndexOf(ActivityBase item)
		{
			this.VerifySort();
			return _activities.IndexOf(item);
		}

		void IList<ActivityBase>.Insert(int index, ActivityBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void IList<ActivityBase>.RemoveAt(int index)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		ActivityBase IList<ActivityBase>.this[int index]
		{
			get
			{
				this.VerifySort();
				return _activities[index] as ActivityBase;
			}
			set
			{
				CoreUtilities.RaiseReadOnlyCollectionException();
			}
		}

		#endregion //IList<ActivityBase> Members

		#region ICollection<ActivityBase> Members

		void ICollection<ActivityBase>.Add(ActivityBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void ICollection<ActivityBase>.Clear()
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		bool ICollection<ActivityBase>.Contains(ActivityBase item)
		{
			this.VerifyActivities();
			return _activities.Contains(item);
		}

		void ICollection<ActivityBase>.CopyTo(ActivityBase[] array, int arrayIndex)
		{
			this.VerifyActivities();
			_activities.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get 
			{
				this.VerifyActivities();
				return _activities.Count; 
			}
		}

		bool ICollection<ActivityBase>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<ActivityBase>.Remove(ActivityBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
			return false;
		}

		#endregion //ICollection<ActivityBase> Members

		#region IEnumerable<ActivityBase> Members

		IEnumerator<ActivityBase> IEnumerable<ActivityBase>.GetEnumerator()
		{
			this.VerifyActivities();
			return new TypedEnumerable<ActivityBase>.Enumerator(((IEnumerable)this).GetEnumerator());
		}

		#endregion //IEnumerable<ActivityBase> Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			this.VerifyActivities();
			return _activities.GetEnumerator();
		}

		#endregion //Enumerable Members

		#region HashSparseArray class
		private class HashSparseArray : SparseArray
		{
			#region Member Variables

			private Dictionary<object, object> _ownerData = new Dictionary<object, object>();

			#endregion // Member Variables

			#region Constructor
			internal HashSparseArray()
			{
			}
			#endregion // Constructor

			#region Base class overrides
			protected override object GetOwnerData(object item)
			{
				object data;
				_ownerData.TryGetValue(item, out data);
				return data;
			}

			protected override void SetOwnerData(object item, object ownerData)
			{
				_ownerData[item] = ownerData;
			}
			#endregion // Base class overrides
		} 
		#endregion // HashSparseArray class
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