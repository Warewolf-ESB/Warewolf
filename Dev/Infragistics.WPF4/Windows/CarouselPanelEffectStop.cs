using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	//EffectStop collections.
	#region EffectStopCollection<T> Class

	/// <summary>
	/// Abstract base class for all <see cref="EffectStop"/> collections.
	/// </summary>
	/// <remarks>
	/// <p class="body">All EffectStopCollections are derived from this abstract generic base class which provides implementations for functionality required by all derived effect-specific classes.
    /// Refer to the documentation for <see cref="EffectStop"/> for a full discussion of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/></p>
	/// <p class="body">There are 5 supported effect stop types for this abstract base generic collection:
	///		<ul>
	///			<li><see cref="OpacityEffectStop"/> - Defines an EffectStop used to apply Opacity effects to items.</li>
	///			<li><see cref="ScalingEffectStop"/> - Defines an EffectStop used to apply Scaling effects to items.</li>
	///			<li><see cref="SkewAngleXEffectStop"/> - Defines an EffectStop used to apply Skewing effects about the X-axis to items.</li>
	///			<li><see cref="SkewAngleYEffectStop"/> - Defines an EffectStop used to apply Skewing effects about the Y-axis to items.</li>
	///			<li><see cref="ZOrderEffectStop"/> - Defines an EffectStop used to apply ZOrder effects to items to control their positions in the Z-space.</li>
	///		</ul>
	/// </p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <typeparam name="T">The type for which the EffectStopCollection is being created.</typeparam>
	/// <seealso cref="OpacityEffectStopCollection"/>
	/// <seealso cref="OpacityEffectStop"/>
	/// <seealso cref="SkewAngleXEffectStopCollection"/>
	/// <seealso cref="SkewAngleXEffectStop"/>
	/// <seealso cref="SkewAngleYEffectStopCollection"/>
	/// <seealso cref="SkewAngleYEffectStop"/>
	/// <seealso cref="ScalingEffectStopCollection"/>
	/// <seealso cref="ScalingEffectStop"/>
	/// <seealso cref="ZOrderEffectStopCollection"/>
	/// <seealso cref="ZOrderEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public abstract class EffectStopCollection<T> : Object,
													INotifyPropertyChanged, 
													INotifyCollectionChanged, 
													IList<T>, 
													IList 
													where T : EffectStop 
	{
		#region Member Variables

		private List<T>									_list = new List<T>(3);
		private int										_listVersion = 0;
		private int										_listSortVersion = -1;
		private List<T>									_sortedList = null;

		private double									_stopValueForBottomOfRange = 0;
		private int										_stopValueForBottomOfRangeVersion = -1;
		private double									_stopValueForTopOfRange = 1;
		private int										_stopValueForTopOfRangeVersion = -1;

		private bool									_changeNotificationsSuspended = false;
		private bool									_changesMadeWhileNotificationsSuspended = false;

		private PropertyChangedEventHandler				_propertyChangedHandler;
		private NotifyCollectionChangedEventHandler		_collectionChangedHandler;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of the EffectStopCollection.  
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="CarouselViewSettings"/> object automatically creates an instance of this class when necessary.  You do not ordinarily need to create an instance of this class directly.</p>
		/// </remarks>
		/// <seealso cref="CarouselViewSettings"/>
		protected EffectStopCollection()
		{
		}

		#endregion //Constructor	

		#region Methods

			#region Public Methods

				#region ReplaceAll

		/// <summary>
		/// Clears then replaces all the elements in the collection with the elements from the specified collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method provides a mechanism for replacing the entire contents of the collection.  To generate a single property changed notification as a result of this action, bracket the call to this method with calls to <see cref="SuspendChangeNotifications"/> and <see cref="ResumeChangeNotifications"/>.</p>
		/// </remarks>
		/// <param name="newEffectStopCollection">The collection that contains the elements to be inserted into the current collection after it is cleared.</param>
		/// <seealso cref="SuspendChangeNotifications"/>
		/// <seealso cref="ResumeChangeNotifications"/>
		public void ReplaceAll(EffectStopCollection<T> newEffectStopCollection)
		{
			this._list.Clear();
			this._list.AddRange(newEffectStopCollection);

			this.BumpListVersion(true);
		}

				#endregion //ReplaceAll

				#region ResumeChangeNotifications

		/// <summary>
		/// Resumes change notifications that were suspended by a call to <see cref="SuspendChangeNotifications"/>.  
		/// </summary>
		/// <param name="fullReset">True to also fire a Reset notification on the collection.</param>
		/// <seealso cref="SuspendChangeNotifications"/>
		/// <seealso cref="ReplaceAll"/>
		public void ResumeChangeNotifications(bool fullReset)
		{
			if (this._changeNotificationsSuspended)
			{
				this._changeNotificationsSuspended = false;

				if (this._changesMadeWhileNotificationsSuspended == true)
				{
					// JM 03-10-09 TFS6689 - Bump the list version.
					this._listVersion++;

					this.RaiseChangeEvents(fullReset);
					this._changesMadeWhileNotificationsSuspended = false;
				}
			}
		}

				#endregion //ResumeChangedNotifications

				#region ShouldSerialize

		/// <summary>
		/// Returns a boolean that indicates whether the collection has changed and should be serialized.
		/// </summary>
		/// <returns>Returns true if the collection contains at least 1 entry.  Otherwise returns false.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			return this._list.Count > 0;
		}

				#endregion //ShouldSerialize	

				#region SuspendChangeNotifications

		/// <summary>
		/// Suspends all change notifications for the collection until <see cref="ResumeChangeNotifications "/> is called.
		/// </summary>
		/// <seealso cref="ResumeChangeNotifications"/>
		/// <seealso cref="ReplaceAll"/>
		public void SuspendChangeNotifications()
		{
			this._changeNotificationsSuspended				= true;
			this._changesMadeWhileNotificationsSuspended	= true;
		}

				#endregion //SuspendChangeNotifications	

			#endregion //Public Methods

			#region Private Methods

				#region BumpListVersion

		private void BumpListVersion(bool fullReset)
		{
			if (this._changeNotificationsSuspended)
			{
				this._changesMadeWhileNotificationsSuspended = true;
				return;
			}

			this.RaiseChangeEvents(fullReset);
			this._listVersion++;
		}

				#endregion //BumpListVersion	

				#region GetStopValueForTopOfRange

		private double GetStopValueForTopOfRange()
		{
			this.VerifyIsSorted();

			if (this._stopValueForTopOfRangeVersion != this._listVersion)
			{
				if (this._sortedList.Count == 0)
					this._stopValueForTopOfRange	= 0;
				else
					this._stopValueForTopOfRange	= this._sortedList[this._sortedList.Count - 1].Value;

				this._stopValueForTopOfRangeVersion	= this._listVersion;
			}

			return this._stopValueForTopOfRange;
		}

				#endregion //GetStopValueForTopOfRange

				#region GetStopValueForBottomOfRange

		private double GetStopValueForBottomOfRange()
		{
			this.VerifyIsSorted();

			if (this._stopValueForBottomOfRangeVersion != this._listVersion)
			{
				if (this._sortedList.Count == 0)
					this._stopValueForBottomOfRange		= 0;
				else
					this._stopValueForBottomOfRange		= this._sortedList[0].Value;

				this._stopValueForBottomOfRangeVersion	= this._listVersion;
			}

			return this._stopValueForBottomOfRange;
		}

				#endregion //GetStopValueForBottomOfRange

				#region OnCollectionChanged

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
		{
			if (this._collectionChangedHandler != null)
			{
				if (item is IList)
					this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, (IList)item, index));
				else
					this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, item, index));
			}

		}

				#endregion //OnCollectionChanged

				#region OnEffectStopPropertyChanged

		void OnEffectStopPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnPropertyChanged("Item[]");
		}

				#endregion //OnEffectStopPropertyChanged	
    
				#region OnPropertyChanged

		private void OnPropertyChanged(string propertyName)
		{
			if (this._propertyChangedHandler != null)
				this._propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
		}

				#endregion //OnPropertyChanged

				#region RaiseChangeEvents

		internal void RaiseChangeEvents(bool fullReset)
		{
			this.OnPropertyChanged("Count");
			this.OnPropertyChanged("Item[]");
			if (fullReset)
				this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, -1);
		}

				#endregion //RaiseChangeEvents	
    
				#region VerifyIsSorted

		private void VerifyIsSorted()
		{
			if (this._listSortVersion != this._listVersion)
			{
				// Allocate a sorted list then copy the contents of our list to our sorted list and sort it.
				int listCount		= this._list.Count;
				this._sortedList	= new List<T>(listCount);
				for (int i = 0; i < listCount; i++)
					this._sortedList.Add(this._list[i]);

				this._sortedList.Sort();

				this._listSortVersion = this._listVersion;
			}
		}

				#endregion //VerifyIsSorted	
    
			#endregion //Private Methods	
    
			#region Internal Methods

				#region GetStopValueFromOffset





		internal double GetStopValueFromOffset(double offset)
		{
			// Sort the list if necessary.
			this.VerifyIsSorted();


			List<T>		list				= this._sortedList;
			int			listCount			= list.Count;
			EffectStop	firstOverallStop	= list[0];
			EffectStop	lastOverallStop		= list[listCount - 1];

			if (offset <= firstOverallStop.Offset)
				return firstOverallStop.Value;
			if (offset >= lastOverallStop.Offset)
				return lastOverallStop.Value;


			// Calculate the correct value depending on where the offset falls within the defined stops.
			EffectStop thisStop, previousStop;
			for (int i = 0; i < listCount; i++)
			{
				thisStop = list[i];

				if (offset <= thisStop.Offset)
				{
					previousStop	= list[i - 1];
					double lowPct	= previousStop.Offset;
					double hiPct	= thisStop.Offset;
					double newPct	= (offset - lowPct) / (hiPct - lowPct);

					double lowVal	= previousStop.Value;
					double hiVal	= thisStop.Value;

					return lowVal + (newPct * (hiVal - lowVal));
				}
			}

			return lastOverallStop.Value;
		}

				#endregion //GetStopValueFromOffset

				#region GetStopValueFromRange



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal double GetStopValueFromRange(double rangeLowValue, double rangeHighValue, double valueToEvaluate)
		{
			// Verify that the low and high range values are valid.
			Debug.Assert(rangeHighValue >= rangeLowValue, "rangeHighValue not greater than rangeLowValue!");
			if (rangeHighValue <= rangeLowValue)
				return 0;


			// Verify that the valueToEvaluate is within the specified range.  Force the value to be within
			// range if it is not.
			if (valueToEvaluate < rangeLowValue)
				valueToEvaluate = rangeLowValue;
			else
			if (valueToEvaluate > rangeHighValue)
				valueToEvaluate = rangeHighValue;


			// If the valueToEvaluate is equal to the low or high end of the range, return the lowest/highest
			// effect value.
			if (valueToEvaluate == rangeLowValue)
				return this.GetStopValueForBottomOfRange();

			if (valueToEvaluate == rangeHighValue)
				return this.GetStopValueForTopOfRange();


			// If no stops have been defined, return the lowest stop value
			List<T> list		= this._list;
			int		listCount	= list.Count;
			if (listCount < 1)
				return 0;


			// If there is only 1 entry in the list, return that value.
			if (listCount == 1)
				return list[0].Value;


			return this.GetStopValueFromOffset((valueToEvaluate - rangeLowValue) / (rangeHighValue - rangeLowValue));
		}

				#endregion //GetStopValueFromRange	
    
			#endregion //Internal Methods	
    
		#endregion //Methods

		#region INotifyCollectionChanged Members

		/// <summary>
		/// Notifies listeners when a change is made to the colleciton
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this._collectionChangedHandler = System.Delegate.Combine(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this._collectionChangedHandler = System.Delegate.Remove(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
			}
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				this._propertyChangedHandler = System.Delegate.Combine(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this._propertyChangedHandler = System.Delegate.Remove(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
			}
		}

		#endregion

		#region IList<T> Members

		/// <summary>
		/// Returns the index of the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The index of the specified item.</returns>
		public int IndexOf(T item)
		{
			return this._list.IndexOf(item);
		}

		/// <summary>
		/// Inserts the specified item at them specified index.
		/// </summary>
		/// <param name="index">The index at which to insert the item.</param>
		/// <param name="item">The item to insert.</param>
		public void Insert(int index, T item)
		{
			this._list.Insert(index, item);
			this.BumpListVersion(false);
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item to remove</param>
		public void RemoveAt(int index)
		{
			this._list.RemoveAt(index);
			this.BumpListVersion(false);
		}

		/// <summary>
		/// Returns the item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item ro return.</param>
		/// <returns>The item at the specified index.</returns>
		public T this[int index]
		{
			get
			{
				return this._list[index];
			}
			set
			{
				this._list[index] = value;
				this.BumpListVersion(false);
			}
		}

		#endregion

		#region ICollection<EffectStop> Members

		/// <summary>
		/// Add the specified item to the collection.
		/// </summary>
		/// <param name="item">The item to be added.</param>
		public void Add(T item)
		{
			this._list.Add(item);
			this.BumpListVersion(false);

			item.PropertyChanged += new PropertyChangedEventHandler(OnEffectStopPropertyChanged);
		}

		/// <summary>
		/// Clear the collection and removes all items.
		/// </summary>
		public void Clear()
		{
			foreach(T t in this._list)
				t.PropertyChanged -= new PropertyChangedEventHandler(OnEffectStopPropertyChanged);;

			this._list.Clear();
			this.BumpListVersion(true);
		}

		/// <summary>
		/// Returns whether the list contains the specified item.
		/// </summary>
		/// <param name="item">The item to check for the existence of.</param>
		/// <returns>True if the list contains the specified, otherwise false.</returns>
		public bool Contains(T item)
		{
			return this._list.Contains(item);
		}

		/// <summary>
		/// Copies the contents of the list to a compatible 1 dimensional array.
		/// </summary>
		/// <param name="array">The target array to which the contents of the list will be copied.</param>
		/// <param name="arrayIndex">The zero-based index in the target array at which the copying begins.</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			this._list.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Returns the number of items in the list.
		/// </summary>
		/// <returns>The number of items in the list.</returns>
		public int Count
		{
			get { return this._list.Count; }
		}

		/// <summary>
		/// Returns whether the list is read only.
		/// </summary>
		/// <returns>True if the list is read-only, otherwise false.</returns>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes the first occurrence of a specified object from the list.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if the item was found and removed, otherwise false.</returns>
		public bool Remove(T item)
		{
			this.BumpListVersion(false);
			item.PropertyChanged -= new PropertyChangedEventHandler(OnEffectStopPropertyChanged);
			
			return this._list.Remove(item);
		}

		#endregion

		#region IEnumerable<EffectStop> Members

		/// <summary>
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An IEnumerator interface that can be used to enumerate through the list.</returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this._list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An IEnumerator interface that can be used to enumerate through the list.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._list.GetEnumerator();
		}

		#endregion

		#region IList Members

		/// <summary>
		/// Adds the specified object to the list.
		/// </summary>
		/// <param name="value">The object to add to the list.</param>
		/// <returns>The index at which the object was added or -1 if the object could not be added.</returns>
		int IList.Add(object value)
		{
			T valueT = value as T;

			this.Add(valueT);

			return this._list.IndexOf(valueT);
		}

		/// <summary>
		/// Clears all items from the list.
		/// </summary>
		void IList.Clear()
		{
			this.Clear();
		}

		/// <summary>
		/// Returns whether the list contains the specified object.
		/// </summary>
		/// <param name="value">The object to check for the existence of.</param>
		/// <returns>True if the list contains the specified object, otherwise false.</returns>
		bool IList.Contains(object value)
		{
			return this.Contains(value as T);
		}

		/// <summary>
		/// Returns the index of the specified object.
		/// </summary>
		/// <param name="value">The object to locate in the list.</param>
		/// <returns>The index of the specified object or -1 if the object was not found in the list.</returns>
		int IList.IndexOf(object value)
		{
			return this._list.IndexOf(value as T);
		}

		/// <summary>
		/// Inserts the specified item at the specified index.
		/// </summary>
		/// <param name="index">The index at which to insert the specified item.</param>
		/// <param name="value">The object to insert into the list.</param>
		void IList.Insert(int index, object value)
		{
			T valueT = value as T;
			this.Insert(index, valueT);
			this.BumpListVersion(false);
			valueT.PropertyChanged += new PropertyChangedEventHandler(OnEffectStopPropertyChanged);
		}

		/// <summary>
		/// Returns whether the list is a fixed size.
		/// </summary>
		/// <returns>True if the list size is fixed, otherwise false.</returns>
		bool IList.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Returns whether the list is read only.
		/// </summary>
		/// <returns>True if the list is read only, otherwise false.</returns>
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		void IList.Remove(object value)
		{
			this.Remove(value as T);
		}

		/// <summary>
		/// Removes the object at the specified index.
		/// </summary>
		/// <param name="index">The index of the object to remove.</param>
		void IList.RemoveAt(int index)
		{
			if (index < this._list.Count)
				this._list[index].PropertyChanged -= new PropertyChangedEventHandler(OnEffectStopPropertyChanged);

			this._list.RemoveAt(index);
			this.BumpListVersion(false);
		}

		/// <summary>
		/// Returns the object at the specified index.
		/// </summary>
		/// <param name="index">The index of the object to return.</param>
		/// <returns>The object at the specified index or null if no object exists at the specified index or of the index is out of bounds.</returns>
		object IList.this[int index]
		{
			get
			{
				return this._list[index];
			}
			set
			{
				if (index < this._list.Count)
				{
					this._list[index].PropertyChanged -= new PropertyChangedEventHandler(OnEffectStopPropertyChanged);
					this._list[index] = value as T;
					this._list[index].PropertyChanged += new PropertyChangedEventHandler(OnEffectStopPropertyChanged);
					this.BumpListVersion(false);
				}
			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// Copies the contents of the list to a compatible 1 dimensional array.
		/// </summary>
		/// <param name="array">The target array to which the contents of the list will be copied.</param>
		/// <param name="index">The zero-based index in the target array at which the copying begins.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			((IList)(this._list)).CopyTo(array, index);
		}

		/// <summary>
		/// Returns the number of items in the list.
		/// </summary>
		/// <returns>The number of items in the list.</returns>
		int ICollection.Count
		{
			get { return this._list.Count; }
		}

		/// <summary>
		/// Returns whether access to the System.Collections.ICollection is synchronized (thread safe).
		/// </summary>
		/// <returns>True if the collection is synchronized and thread safe, otherwise false.</returns>
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the System.Collections.ICollection.
		/// </summary>
		object ICollection.SyncRoot
		{
			get { return this; }
		}

		#endregion
	}

	#endregion //EffectStopCollection<T> Class

	#region OpacityEffectStopCollection Class

	/// <summary>
    /// A collection of <see cref="OpacityEffectStop"/> objects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="OpacityEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public class OpacityEffectStopCollection : EffectStopCollection<OpacityEffectStop>
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="OpacityEffectStopCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the collection is created it will contain zero items.</p>
		/// </remarks>
		public OpacityEffectStopCollection()
		{
		}

		#endregion //Constructor
	}

	#endregion //OpacityEffectStopCollection Class

	#region ScalingEffectStopCollection Class

	/// <summary>
    /// A collection of <see cref="ScalingEffectStop"/> objects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="ScalingEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public class ScalingEffectStopCollection : EffectStopCollection<ScalingEffectStop>
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="ScalingEffectStopCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the collection is created it will contain zero items.</p>
		/// </remarks>
		public ScalingEffectStopCollection()
		{
		}

		#endregion //Constructor	
	}

	#endregion //ScalingEffectStopCollection Class

	#region ZOrderEffectStopCollection Class

	/// <summary>
    /// A collection of <see cref="ZOrderEffectStop"/> objects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="ZOrderEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public class ZOrderEffectStopCollection : EffectStopCollection<ZOrderEffectStop>
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="ZOrderEffectStopCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the collection is created it will contain zero items.</p>
		/// </remarks>
		public ZOrderEffectStopCollection()
		{
		}

		#endregion //Constructor
	}

	#endregion //ZOrderEffectStopCollection Class

	#region SkewAngleXEffectStopCollection Class

	/// <summary>
    /// A collection of <see cref="SkewAngleXEffectStop"/> objects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="SkewAngleXEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public class SkewAngleXEffectStopCollection : EffectStopCollection<SkewAngleXEffectStop>
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="SkewAngleXEffectStopCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the collection is created it will contain zero items.</p>
		/// </remarks>
		public SkewAngleXEffectStopCollection()
		{
		}

		#endregion //Constructor
	}

	#endregion //SkewAngleXEffectStopCollection Class

	#region SkewAngleYEffectStopCollection Class

	/// <summary>
    /// A collection of <see cref="SkewAngleYEffectStop"/> objects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="SkewAngleYEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public class SkewAngleYEffectStopCollection : EffectStopCollection<SkewAngleYEffectStop>
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="SkewAngleYEffectStopCollection"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">When the collection is created it will contain zero items.</p>
		/// </remarks>
		public SkewAngleYEffectStopCollection()
		{
		}

		#endregion //Constructor
	}

	#endregion //SkewAngleYEffectStopCollection Class


	//EffectStops
	#region EffectStop Class

	/// <summary>
	/// Abstract base class that represents an <see cref="EffectStop"/> and contains an Offset and a Value.
	/// </summary>
	/// <remarks>
	/// <p class="body">There are 5 specific effect stop types derived from this base class:
	///		<ul>
	///			<li><see cref="OpacityEffectStop"/> - Defines an EffectStop used to apply Opacity effects to items.</li>
	///			<li><see cref="ScalingEffectStop"/> - Defines an EffectStop used to apply Scaling effects to items.</li>
	///			<li><see cref="SkewAngleXEffectStop"/> - Defines an EffectStop used to apply Skewing effects along the X-axis to items.</li>
	///			<li><see cref="SkewAngleYEffectStop"/> - Defines an EffectStop used to apply Skewing effects along the Y-axis to items.</li>
	///			<li><see cref="ZOrderEffectStop"/> - Defines an EffectStop used to apply ZOrder effects to items to control their positions in the Z-space.</li>
	///		</ul>
	/// </p>
    /// <p class="body">An EffectStop contains an Offset and a Value which define how a particular parent effect is applied to items in the <see cref="XamCarouselPanel"/>
    /// The <see cref="XamCarouselPanel"/> supports 5 parent effects the: Opacity, Scaling, Skewing along the X-axis, Skewing along the Y-Axis and ZOrder.  Each of the 5 effects has a 
    /// corresponding EffectStop-derived class that defines an offset and value for applying that effect.</p>
    /// <p class="body">You can define 1 or more EffectStops for each parent effect by
    /// creating the appropriate derived EffectStop (e.g., <see cref="OpacityEffectStop"/>) and adding it to the appropriate derived EffectStopCollection (e.g., <see cref="OpacityEffectStopCollection"/>).</p>
    /// <p class="body">The 5 effect stop collections are exposed as properties on the <see cref="CarouselViewSettings"/> object (<see cref="CarouselViewSettings.OpacityEffectStops"/>, <see cref="CarouselViewSettings.ScalingEffectStops"/>,
    /// <see cref="CarouselViewSettings.SkewAngleXEffectStops"/>, <see cref="CarouselViewSettings.SkewAngleYEffectStops"/>, <see cref="CarouselViewSettings.ZOrderEffectStops"/>).  The <see cref="CarouselViewSettings"/> object is exposed via 
    /// the ViewSettings properties on both the <see cref="XamCarouselPanel"/>.<see cref="XamCarouselPanel.ViewSettings"/> and <see cref="XamCarouselListBox"/>.<see cref="XamCarouselListBox.ViewSettings"/>.  
    /// </p>
    /// <p class="body">When applying a parent effect to an item in the list, the XamCarouselPanel considers 4 pieces of information.  For example, when the XamCarouselPanel is
    /// deciding how to apply an Opacity effect to an item it considers the following 4 pieces of information:
    ///		<ul>
    ///			<li>The setting of the <see cref="CarouselViewSettings.UseOpacity"/> property to determine if opacity effects are being used.  You can use this property to turn Opacity effects on or off.  If this
    /// property is set to false, the <see cref="XamCarouselPanel"/> stops its evaluation and does not apply an opacity effect to the item.</li>
    ///			<li>The list of <see cref="CarouselViewSettings.OpacityEffectStops"/> that have been defined.  The <see cref="XamCarouselPanel"/> uses linear interpolation to establish Values for Offsets that fall between defined stops.</li>
    ///			<li>The value of the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/> property which determines how the '<see cref="EffectStop.Offset"/>' of each <see cref="OpacityEffectStop"/> is evaluated. For example, if the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/>
    /// is set to <see cref="EffectStopDirection"/>.UseItemPath, and the item's position is 50 percent along the <see cref="CarouselViewSettings.ItemPath"/>, the <see cref="EffectStop.Value"/> applied to the item's Opacity would be based on an offset of .5.  If however, the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/>
    /// is set to <see cref="EffectStopDirection"/>.Vertical, and the item's Y-position in the <see cref="XamCarouselPanel"/>'s display area is one third of the way down from the top of the <see cref="XamCarouselPanel"/>'s display area, the value applied to the item's Opacity would be based on an offset of .3.</li>
    ///			<li>The position of the item in the <see cref="XamCarouselPanel"/>'s display area (used as described in the previous bullet).</li>
    ///		</ul>
    /// The other types of parent effects are evaluated in exactly the same way using correspondingly named properties on the <see cref="CarouselViewSettings"/> object.
    /// </p>
    /// <p class="body">By applying different combinations of <see cref="EffectStop"/>s you can achieve some interesting layout effects in the <see cref="XamCarouselPanel"/>.  </p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="OpacityEffectStopCollection"/>
	/// <seealso cref="OpacityEffectStop"/>
	/// <seealso cref="SkewAngleXEffectStopCollection"/>
	/// <seealso cref="SkewAngleXEffectStop"/>
	/// <seealso cref="SkewAngleYEffectStopCollection"/>
	/// <seealso cref="SkewAngleYEffectStop"/>
	/// <seealso cref="ScalingEffectStopCollection"/>
	/// <seealso cref="ScalingEffectStop"/>
	/// <seealso cref="ZOrderEffectStopCollection"/>
	/// <seealso cref="ZOrderEffectStop"/>
	/// <seealso cref="EffectStopDirection"/>
	public abstract class EffectStop : DependencyObjectNotifier, IComparable
	{
		#region Member Variables

		private	double									_cachedOffset;
		private	double									_cachedValue;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Creates an instance of an EffectStop with a default Offset and a default Value (0, 0).
		/// </summary>
		protected EffectStop()
		{
		}

		/// <summary>
		/// Creates an instance of an EffectStop with the specified Offset and Value.
		/// </summary>
		protected EffectStop(double offset, double value)
		{
			this.SetValue(EffectStop.OffsetProperty, Math.Max(Math.Min(offset, 1), 0d));
			this.SetValue(EffectStop.ValueProperty, value);
		}

		#endregion //Constructors	

		#region Base Class Overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes.
		/// </summary>
		/// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
		}

			#endregion //OnPropertyChanged	
    
		#endregion //Base Class Overrides	
    
		#region Properties

			#region Offset

		/// <summary>
		/// Identifies the <see cref="EffectStop.Offset"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset",
			typeof(double), typeof(EffectStop), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnOffsetChanged)), new ValidateValueCallback(OnValidateOffset));

		private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			EffectStop effectStop = d as EffectStop;
			if (effectStop != null)
				effectStop._cachedOffset = (double)e.NewValue;
		}

		private static bool OnValidateOffset(object value)
		{
			if ((double)value < 0.0 || (double)value > 1.0)
				return false;

			return true;
		}

		/// <summary>
		/// A value between 0 and 1 that represents the percentage offset into the overall range for the effect.
		/// </summary>
        /// <remarks>
        /// <p class="body">The overall range for the effect is determined by the setting of the related <see cref="EffectStopDirection"/> property and the extent of the corresponding control dimension.  
        /// For example, the overall range for an <see cref="OpacityEffectStop"/> is determined by the setting of the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/> property and the
        /// corresponding extent of the <see cref="XamCarouselPanel"/>'s display area.  So if the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/> is set to <see cref="EffectStopDirection"/>.Vertical
        /// the overall range for the effect would be the height of the <see cref="XamCarouselPanel"/>'s display area.  If the Offset of the EffectStop is set to .75 then the EffectStop's
        /// Value would apply to items whose vertical position in the <see cref="XamCarouselPanel"/>'s display area is three quarters of the way down from the top (<see cref="EffectStopDirection"/>.Vertical 
        /// is always evaluated from the top down)</p>
        /// <p class="body">A similar evaluation is done with respect to the control's horizontal extent if the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/> is set to <see cref="EffectStopDirection"/>.Horizontal, with evaluation
        /// of the Offset done from left to right.  </p>
        /// <p class="body">The example provided above for an <see cref="OpacityEffectStop"/> applies to the EffectStop types as well.</p>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
		/// </remarks>
		/// </remarks>
        /// <seealso cref="OffsetProperty"/>
		/// <seealso cref="EffectStop.Value"/>
		//[Description("A value between 0 and 1 that represents the percentage offset in the overall range for the effect")]
		//[Category("Data")]
		[Bindable(true)]
        public double Offset
		{
			get
			{
				return this._cachedOffset;
			}
			set
			{
				this.SetValue(EffectStop.OffsetProperty, value);
			}
		}

			#endregion //Offset

			#region Value

		/// <summary>
		/// Identifies the <see cref="EffectStop.Value"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
			typeof(double), typeof(EffectStop), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(OnCoerceValue)));

		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			EffectStop effectStop = d as EffectStop;
			if (effectStop != null)
				effectStop._cachedValue = (double)e.NewValue;
		}

		private static object OnCoerceValue(DependencyObject d, object value)
		{
			if (d is ScalingEffectStop)
			{
				if ((double)value < 0 || (double)value > 10)
					return (double)1;

				return value;
			}

			if (d is ZOrderEffectStop)
			{
				if ((double)value < 0 || (double)value > 1)
					return (double)0;

				return value;
			}

			if (d is SkewAngleXEffectStop)
			{
				if ((double)value < -360 || (double)value > 360)
					return (double)0;

				return value;
			}

			if (d is SkewAngleYEffectStop)
			{
				if ((double)value < -360 || (double)value > 360)
					return (double)0;

				return value;
			}

			if (d is OpacityEffectStop)
			{
				if ((double)value < 0 || (double)value > 1)
					return (double)0;

				return value;
			}

			return (double)0;
		}

		/// <summary>
		/// The value that should be applied at the <see cref="EffectStop.Offset"/> associated with this <see cref="EffectStop"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
		/// <p class="note"><b>Note:</b> The range of valid values for this property depends on the derived EffectStop type.</p>
		/// </remarks>
		/// <seealso cref="ValueProperty"/>
		/// <seealso cref="EffectStop.Offset"/>
		//[Description("The value that should be returned for the associated offset.")]
		//[Category("Data")]
		[Bindable(true)]
		public double Value
		{
			get
			{
				return this._cachedValue;
			}
			set
			{
				this.SetValue(EffectStop.ValueProperty, value);
			}
		}

			#endregion //Value

		#endregion //Properties

		#region IComparable Members

		/// <summary>
		/// Compares the specified object to this instance.
		/// </summary>
		/// <param name="obj">The object to compare to this instance</param>
		/// <returns>A signed number that indicates the relative values of the instance and the specified object. If this instance is less than the specified object a number less than zero is returned. If this instance is the same as the specified object then zero is returned. If this instance is greater than the specified object -or- the specified object is a null reference a number greater than zero is returned.</returns>
		int IComparable.CompareTo(object obj)
		{
			EffectStop stop = (EffectStop)obj;

			if (this.Offset == stop.Offset)
				return 0;

			if (this.Offset < stop.Offset)
				return -1;
			else
				return 1;
		}

		#endregion //IComparable Members
	}

	#endregion //EffectStop Class

	#region OpacityEffectStop Class

	/// <summary>
    /// A strongly typed <see cref="EffectStop"/> used for Opacity effects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>.
	/// </summary>
	/// <remarks>
    /// <p class="body">Opacity effects give you control over the opacity of items within the <see cref="XamCarouselPanel"/> based on their position in the <see cref="XamCarouselPanel"/>'s display
    /// area.  For example, you can use an Opacity effect in conjunction with a Scaling effect to simulate perspective and make items appear to be fading into the distance.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>This derived <see cref="EffectStop"/> class provides range validation for the <see cref="EffectStop.Value"/> property.  
    /// The valid range for an OpacityEffectStop Value is from 0 to 1.</p>
	/// </remarks>
	/// <seealso cref="EffectStop"/>
	/// <seealso cref="OpacityEffectStopCollection"/>
	/// <seealso cref="EffectStopDirection"/>
    /// <seealso cref="XamCarouselPanel"/>
	public class OpacityEffectStop : EffectStop
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of an OpacityEffectStop with default values for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The default value for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties is zero.</p>
		/// </remarks>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public OpacityEffectStop()
			: base()
		{
		}

		/// <summary>
		/// Creates an instance of an <see cref="OpacityEffectStop"/> using the specified <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/>.
		/// </summary>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public OpacityEffectStop(double offset, double value)
			: base(offset, value)
		{
		}

		#endregion //Constructors
	}

	#endregion //OpacityEffectStop Class

	#region ScalingEffectStop Class

    /// <summary>
    /// A strongly typed <see cref="EffectStop"/> used for Scaling effects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>.
    /// </summary>
    /// <remarks>
    /// <p class="body">Scaling effects give you control over the size of items within the <see cref="XamCarouselPanel"/> based on their position in the <see cref="XamCarouselPanel"/>'s display
    /// area.  For example, you can use a Scaling effect in conjunction with an Opacity effect to simulate perspective and make items appear to be fading into the distance.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>This derived <see cref="EffectStop"/> class provides range validation for the <see cref="EffectStop.Value"/> property.  
    /// The valid range for a ScalingEffectStop Value is from 0 and 10.</p>
    /// </remarks>
    /// <seealso cref="EffectStop"/>
	/// <seealso cref="ScalingEffectStopCollection"/>
	/// <seealso cref="EffectStopDirection"/>
    /// <seealso cref="XamCarouselPanel"/>
    public class ScalingEffectStop : EffectStop
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a ScalingEffectStop with default values for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The default value for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties is zero.</p>
		/// </remarks>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public ScalingEffectStop()
			: base()
		{
		}

		/// <summary>
		/// Creates an instance of a <see cref="ScalingEffectStop"/> using the specified <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/>.
		/// </summary>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public ScalingEffectStop(double offset, double value)
			: base(offset, value)
		{
		}

		#endregion //Constructors
	}

	#endregion //ScalingEffectStop Class

	#region ZOrderEffectStop Class

    /// <summary>
    /// A strongly typed <see cref="EffectStop"/> used for ZOrder effects.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>.
    /// </summary>
    /// <remarks>
    /// <p class="body">ZOrder effects give you control over the z-index of items based on their position in the <see cref="XamCarouselPanel"/>'s display
    /// area.  For example, you can use a ZOrder effect for items on a circular path to ensure that items at the bottom of the circle (i.e., logically closer to the viewer's
    /// eye) are topmost, with items to the right and left further down in the zorder.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>This derived <see cref="EffectStop"/> class provides range validation for the <see cref="EffectStop.Value"/> property.  
    /// The valid range for a ZOrderEffectStop Value is from 0 to 1 (with 0 representing 'bottommost' and 1 representing 'topmost')</p>
    /// </remarks>
    /// <seealso cref="EffectStop"/>
	/// <seealso cref="ZOrderEffectStopCollection"/>
	/// <seealso cref="EffectStopDirection"/>
    /// <seealso cref="XamCarouselPanel"/>
    public class ZOrderEffectStop : EffectStop
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a ZOrderEffectStop with default values for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The default value for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties is zero.</p>
		/// </remarks>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public ZOrderEffectStop()
			: base()
		{
		}

		/// <summary>
		/// Creates an instance of a <see cref="ZOrderEffectStop"/> using the specified <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/>.
		/// </summary>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public ZOrderEffectStop(double offset, double value)
			: base(offset, value)
		{
		}

		#endregion //Constructors
	}

	#endregion //ZOrderEffectStop Class

	#region SkewAngleXEffectStop Class

    /// <summary>
    /// A strongly typed <see cref="EffectStop"/> used for effects that Skew items about the X-axis.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>.
    /// </summary>
    /// <remarks>
    /// <p class="body">Skewing effects give you control over the rotation of items about their X or Y axis, based on their position in the <see cref="XamCarouselPanel"/>'s display
    /// area.  For example, you can use a Skewing effects with a curved path to make items appear to be wrapping around the curve.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>This derived <see cref="EffectStop"/> class provides range validation for the <see cref="EffectStop.Value"/> property.  
    /// The valid range for a SkewAngleXEffectStop Value is from -360 to +360.</p>
    /// </remarks>
    /// <seealso cref="EffectStop"/>
	/// <seealso cref="SkewAngleXEffectStopCollection"/>
	/// <seealso cref="EffectStopDirection"/>
    /// <seealso cref="XamCarouselPanel"/>
    public class SkewAngleXEffectStop : EffectStop
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a SkewAngleXEffectStop with default values for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The default value for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties is zero.</p>
		/// </remarks>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public SkewAngleXEffectStop()
			: base()
		{
		}

		/// <summary>
		/// Creates an instance of a <see cref="SkewAngleXEffectStop"/> using the specified <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/>.
		/// </summary>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public SkewAngleXEffectStop(double offset, double value)
			: base(offset, value)
		{
		}

		#endregion //Constructors
	}

	#endregion //SkewAngleXEffectStop Class

	#region SkewAngleYEffectStop Class

    /// <summary>
    /// A strongly typed <see cref="EffectStop"/> used for effects that Skew items about the Y-axis.  Refer to the documentation for <see cref="EffectStop"/> for a full discussion
    /// of how to use EffectStops to apply parent effects to items in a <see cref="XamCarouselPanel"/>.
    /// </summary>
    /// <remarks>
    /// <p class="body">Skewing effects give you control over the rotation of items about their X or Y axis, based on their position in the <see cref="XamCarouselPanel"/>'s display
    /// area.  For example, you can use a Skewing effects with a curved path to make items appear to be wrapping around the curve.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>This derived <see cref="EffectStop"/> class provides range validation for the <see cref="EffectStop.Value"/> property.  
    /// The valid range for a SkewAngleYEffectStop Value is from -360 to +360.</p>
    /// </remarks>
    /// <seealso cref="EffectStop"/>
	/// <seealso cref="SkewAngleYEffectStopCollection"/>
	/// <seealso cref="EffectStopDirection"/>
    /// <seealso cref="XamCarouselPanel"/>
    public class SkewAngleYEffectStop : EffectStop
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of a SkewAngleYEffectStop with default values for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The default value for the <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/> properties is zero.</p>
		/// </remarks>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public SkewAngleYEffectStop()
			: base()
		{
		}

		/// <summary>
		/// Creates an instance of a <see cref="SkewAngleYEffectStop"/> using the specified <see cref="EffectStop.Offset"/> and <see cref="EffectStop.Value"/>.
		/// </summary>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStop.Offset"/>
		/// <seealso cref="EffectStop.Value"/>
		public SkewAngleYEffectStop(double offset, double value)
			: base(offset, value)
		{
		}

		#endregion //Constructors
	}

	#endregion //SkewAngleYEffectStop Class
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