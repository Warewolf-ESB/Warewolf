using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
	// JM 6/12/09 NA 2009.2 DataValueChangedEvent - Added
	#region Class DataValueInfoHistory

	// This class maintains a circular list of DataValueInfo instances in a private List<DataValueInfo> member variable while exposing 
	// IList<DataValueInfo> and IList interfaces that return the instances.  The size of the list is fixed to a capacity that is passed 
	// into the constructor.
	//
	// The instances are stored in the circular list in the order they are added but they are returned to callers through the interfaces
	// in the reverse order that they were added - i.e., the indexer will return the newest instance from [0], and the oldest instance 
	// from [Capacity - 1].  
	//
	// Example: If the list already contains 'Capacity' entries, the next new entry is added to the first physical slot in the list, replacing the
	// existing entry in the first physical slot.
	internal class DataValueInfoHistory : IList<DataValueInfo>,
										  IList,
										  INotifyCollectionChanged
	{
		#region Member Variables

		private List<DataValueInfo>			_list;
		private int							_totalItemsAdded;
		private int							_tail = -1;
		private int							_capacity;

		// JJD 3/11/11 - TFS67970 - Optimization
		// In order to prevent fragmentation of the heap we want to cache history objects for later re-use
		[ThreadStatic()]
		private static List<DataValueInfoHistory> _HistoryCache;			


		#endregion //Member Variables

		#region Constructor

		static DataValueInfoHistory()
		{
			// JJD 3/11/11 - TFS67970 - Optimization
			// In order to prevent fragmentation of the heap we want to cache history objects for later re-use
			// allocate the cache list in the static ctro since the member is thread static
			_HistoryCache = new List<DataValueInfoHistory>(100);			
		}

		// JJD 3/11/11 - TFS67970 - Optimization
		// Make ctor private to force the use of the Create static method
		//internal DataValueInfoHistory(int capacity)
		private DataValueInfoHistory(int capacity)
		{
			this._list		= new List<DataValueInfo>(capacity);
			this._capacity	= capacity;
		}

		#endregion //Constructor

		#region Properties

			#region Capacity

		internal int Capacity
		{
			get	{ return this._capacity; }
		}

			#endregion //Capacity

			#region Head

		internal int Head
		{
			get
			{
				if (this._totalItemsAdded <= this.Capacity)
					return 0;
				else
				{
					if (this._tail == this.Capacity - 1)
						return 0;
					else
						return this._tail + 1;
				}
			}
		}

			#endregion //Head

		#endregion //Properties

		#region Methods

			#region Static Methods

				// JJD 3/11/11 - TFS67970 - Optimization
				#region Create

		internal static DataValueInfoHistory Create(int capacity)
		{
			DataValueInfoHistory history;

			int index = _HistoryCache.Count - 1;

			// pop one off the end of the list
			if (index >= 0)
			{
				history = _HistoryCache[index];
				_HistoryCache.RemoveAt(index);

				history._capacity = capacity;
			}
			else
			{
				history = new DataValueInfoHistory(capacity);
			}
			return history;
		}

				#endregion //Create	
    
				#region GetResizedInstance
			
		internal static DataValueInfoHistory GetResizedInstance(DataValueInfoHistory oldDataValueInfoHistory, int newCapacity)
		{
			if (oldDataValueInfoHistory == null)
				throw new ArgumentNullException("oldDataValueInfoHistory");
			if (newCapacity < 1)
				throw new ArgumentOutOfRangeException("newCapacity");

			// If there is no change in the capacity, just return the old instance.
			if (newCapacity == oldDataValueInfoHistory.Capacity)
				return oldDataValueInfoHistory;

			// Allocate a new instance.
			DataValueInfoHistory newDataValueInfoHistory = new DataValueInfoHistory(newCapacity);

			// Since the enumerator will return entries from the old DataValueInfoHistory in REVERSE order (i.e., the most current value change first),
			// add the entries to an array, then iterate through the array in reverse and add the entries to the newDataValueHistory.
			int			totalEntriesCopied	= 0;
			ArrayList	temp				= new ArrayList(newCapacity);
			foreach (DataValueInfo dviOld in oldDataValueInfoHistory)
			{
				temp.Add(dviOld);
				totalEntriesCopied++;

				if (totalEntriesCopied >= newCapacity)
					break;
			}

			temp.TrimToSize();
			DataValueInfo dviNew;
			for(int i = temp.Count - 1; i > -1; i--)
			{
				dviNew = temp[i] as DataValueInfo;
				newDataValueInfoHistory.AddEntry(dviNew.Value, dviNew.TimeStamp, dviNew.Tag);
			}
			

			return newDataValueInfoHistory;
		}

				#endregion //GetResizedInstance
		
				// JJD 3/11/11 - TFS67970 - Optimization
				#region Release

		internal static void Release(DataValueInfoHistory history)
		{
			// cache up to a certain anumber of these objects
			if (history == null || _HistoryCache.Count > 300)
				return;

			int valueCount = history._list.Count;

			// release all the valu entries
			for (int i = 0; i < valueCount; i++)
				DataValueInfo.Release(history._list[i]);

			history._list.Clear();
			history._tail = -1;
			history._totalItemsAdded = 0;

			_HistoryCache.Add(history);
		}

				#endregion //Release	
    
			#endregion //Static Methods

			#region Internal Methods

				#region AddEntry

		internal void AddEntry(object value, DateTime timeStamp, object tag)
		{
			if (this._totalItemsAdded + 1 <= this.Capacity)
			{
				// Only add a new entry if the new value is different than the previous entry's value.
				if (this.GetIsValueChanged(value, this._tail) == true)
				{
					// JJD 3/11/11 - TFS67970 - Optimization
					// Use static Create method instead
					//this._list.Add(new DataValueInfo(value, timeStamp, tag));
					this._list.Add(DataValueInfo.Create(value, timeStamp, tag));
					this._tail = this.GetBumpedTailValue();
				}
				else
					return;
			}
			else
			{
				DataValueInfo	previousDataValueInfo	= this._list[this._tail];
				int				newTail					= this.GetBumpedTailValue();
				DataValueInfo	dataValueInfoToReuse	= this._list[newTail];

				if (dataValueInfoToReuse != null)
				{
					// Only update the entry if the new value is different than the existing entry's value.
					// JM 02-11-10 TFS27538.  Account for previousDataValueInfo.Value == null.
					//if (false == previousDataValueInfo.Value.Equals(value))
					if ((previousDataValueInfo.Value == null && value != null) ||
						(previousDataValueInfo.Value != null && false == previousDataValueInfo.Value.Equals(value)))
					{
						dataValueInfoToReuse.SetValue(value);
						dataValueInfoToReuse.SetTimeStamp(timeStamp);
						dataValueInfoToReuse.Tag = tag;

						this._tail = newTail;
					}
					else
						return;
				}
				else
				{
					// JJD 3/11/11 - TFS67970 - Optimization
					// Use static Create method instead 
					//this._list[newTail] = new DataValueInfo(value, timeStamp, tag);
					this._list[newTail] = DataValueInfo.Create(value, timeStamp, tag);
					this._tail			= newTail;
				}
			}

			this._totalItemsAdded++;

			// Raise change notifications
			if (this.CollectionChanged != null)
				this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

				#endregion //AddEntry

				#region ClearCacheEntries

		internal void ClearCacheEntries(bool keepNewestEntry)
		{
			int count = _list.Count;

			if (keepNewestEntry == false)
			{
				// JJD 3/11/11 - TFS67970 - Optimization
				// Call static RelAse method to cache these info objects for re-use
				for (int i = 0; i < count; i++)
					DataValueInfo.Release(_list[i]);

				this._list.Clear();
				this._tail				= -1;
				this._totalItemsAdded	= 0;
			}
			else
			{
				// Save the newest entry, clear the list, and re-add the entry.
				if (this._totalItemsAdded > 1)
				{
					DataValueInfo dvi = ((IList<DataValueInfo>)this)[0];
					if (dvi != null)
					{
						// JJD 3/11/11 - TFS67970 - Optimization
						// Call static RelAse method to cache these info objects for re-use
						for (int i = 0; i < count; i++)
						{
							DataValueInfo info = _list[i];

							// bypass the one that we will be re-adding below
							if ( info != dvi)
								DataValueInfo.Release(info);
						}

						this._list.Clear();
						this._list.Add(dvi);
						this._tail				= 0;
						this._totalItemsAdded	= 1;
					}
				}
			}

			// Raise change notifications
			if (this.CollectionChanged != null)
				this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

				#endregion //ClearCacheEntries

			#endregion //Internal Methods

			#region Private Methods

				#region GetBumpedTailValue

		private int GetBumpedTailValue()
		{
			// Bump the tail value.
			if (this._tail + 1 >= this.Capacity)
				return 0;
			else
				return this._tail + 1;
		}

				#endregion //GetBumpedTailValue

				#region GetIsValueChanged

		private bool GetIsValueChanged(object newValue, int oldValueIndex)
		{
			if (oldValueIndex < 0)
				return true;

			DataValueInfo dvi = this._list[oldValueIndex];
			if (dvi != null)
			{
				if (dvi.Value == null)
					return newValue != null;

				return false == dvi.Value.Equals(newValue);
			}

			return true;
		}

				#endregion //GetIsValueChanged

				#region GetLogicalIndexFromPhysicalIndex

		private int GetLogicalIndexFromPhysicalIndex(int physicalIndex)
		{
			if (physicalIndex < 0 || physicalIndex > this._totalItemsAdded - 1)
				return -1;


			int totalEntriesProcessed	= 0;
			int currentPhysicalIndex	= this._tail;
			while (totalEntriesProcessed <= this.Capacity)
			{
				if (currentPhysicalIndex == physicalIndex)
					return totalEntriesProcessed;

				totalEntriesProcessed++;
				
				currentPhysicalIndex--;
				if (currentPhysicalIndex < 0)
					currentPhysicalIndex = this.Capacity - 1;
			}

			return -1;
		}

				#endregion //GetLogicalIndexFromPhysicalIndex

				#region GetPhysicalIndexFromLogicalIndex

		private int GetPhysicalIndexFromLogicalIndex(int logicalIndex)
		{
			if (logicalIndex < 0 || logicalIndex > this._totalItemsAdded - 1)
				return -1;



			int totalEntriesProcessed	= 0;
			int currentPhysicalIndex	= this._tail;
			while (totalEntriesProcessed <= this.Capacity)
			{
				if (totalEntriesProcessed == logicalIndex)
					return currentPhysicalIndex;

				totalEntriesProcessed++;

				currentPhysicalIndex--;
				if (currentPhysicalIndex < 0)
					currentPhysicalIndex = this.Capacity - 1;
			}

			return -1;
		}

				#endregion //GetPhysicalIndexFromLogicalIndex

			#endregion //Private Methods

		#endregion //Methods

		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region IList<DataValueInfo> Members

		int IList<DataValueInfo>.IndexOf(DataValueInfo item)
		{
			return ((IList)this).IndexOf(item);
		}

		void IList<DataValueInfo>.Insert(int index, DataValueInfo item)
		{
			throw new NotImplementedException();
		}

		void IList<DataValueInfo>.RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		DataValueInfo IList<DataValueInfo>.this[int index]
		{
			get
			{
				return ((IList)this)[index] as DataValueInfo;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region ICollection<DataValueInfo> Members

		void ICollection<DataValueInfo>.Add(DataValueInfo item)
		{
			throw new NotImplementedException();
		}

		void ICollection<DataValueInfo>.Clear()
		{
			throw new NotImplementedException();
		}

		bool ICollection<DataValueInfo>.Contains(DataValueInfo item)
		{
			return ((IList)this).Contains(item);
		}

		void ICollection<DataValueInfo>.CopyTo(DataValueInfo[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		int ICollection<DataValueInfo>.Count
		{
			get { return ((ICollection)this).Count; }
		}

		bool ICollection<DataValueInfo>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<DataValueInfo>.Remove(DataValueInfo item)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<DataValueInfo> Members

		IEnumerator<DataValueInfo> IEnumerable<DataValueInfo>.GetEnumerator()
		{
			return new DataValueInfoEnumerator(this);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DataValueInfoEnumerator(this);
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			throw new NotImplementedException();
		}

		void IList.Clear()
		{
			throw new NotImplementedException();
		}

		bool IList.Contains(object value)
		{
			return this._list.Contains((DataValueInfo)value);
		}

		int IList.IndexOf(object value)
		{
			int physicalIndex = this._list.IndexOf(value as DataValueInfo);
			if (physicalIndex == -1)
				return -1;

			return this.GetLogicalIndexFromPhysicalIndex(physicalIndex);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		void IList.Remove(object value)
		{
			throw new NotImplementedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		object IList.this[int index]
		{
			get
			{
				int physicalIndex = this.GetPhysicalIndexFromLogicalIndex(index);
				if (physicalIndex == -1)
					return null;

				return this._list[physicalIndex];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		int ICollection.Count
		{
			get { return Math.Min(this.Capacity, this._totalItemsAdded); }
		}

		bool ICollection.IsSynchronized
		{
			get { return ((IList)this._list).IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return ((IList)this._list).SyncRoot; }
		}

		#endregion

		#region DataValueInfoEnumerator private class

		private class DataValueInfoEnumerator : IEnumerator, IEnumerator<DataValueInfo>
		{
			#region Member Variables

			private DataValueInfoHistory	_dataValueInfoHistory;
			private DataValueInfo			_currentDataValueInfo;
			private	int						_currentIndex = -1;
			private int						_totalMoveNextsSoFar;

			#endregion //Member Variables

			#region Constructor

			internal DataValueInfoEnumerator(DataValueInfoHistory dataValueInfoHistory)
			{
				this._dataValueInfoHistory	= dataValueInfoHistory;
			}

			#endregion //Constructor

			#region Methods

				#region Dispose

			public void Dispose()
			{
				this.Reset();
			}

				#endregion //Dispose

			#endregion //Methods

			#region IEnumerator Members

			public bool MoveNext()
			{
				int count = Math.Min(this._dataValueInfoHistory.Capacity, this._dataValueInfoHistory._totalItemsAdded);
				if (this._totalMoveNextsSoFar < count)
				{
					if (this._currentIndex == -1)
						this._currentIndex = 0;
					else
						this._currentIndex++;

					if (this._currentIndex < count)
					{
						this._currentDataValueInfo = ((IList)this._dataValueInfoHistory)[this._currentIndex] as DataValueInfo;
						return true;
					}
				}


				this._currentIndex			= count;
				this._currentDataValueInfo	= null;
				return false;
			}

			public void Reset()
			{
				this._currentDataValueInfo	= null;
				this._currentIndex			= -1;
				this._totalMoveNextsSoFar	= 0;
			}

			object IEnumerator.Current
			{
				get	{ return this.Current; }
			}

			#endregion

			#region IEnumerator<DataValueInfo> Members

			public DataValueInfo Current
			{
				get
				{
					if (this._currentDataValueInfo == null)
					{
					if (this._currentIndex == -1)
						throw new InvalidOperationException("Enumerator has not been started");
					else
						throw new InvalidOperationException("Enumerator has finished");
					}

					return this._currentDataValueInfo;
				}
			}

			#endregion
		}

		#endregion //DataValueInfoEnumerator private class
	}

	#endregion Class DataValueInfoHistory
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