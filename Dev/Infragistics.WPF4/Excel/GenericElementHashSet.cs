using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/31/11 - TFS75574
	internal class GenericElementHashSet<T> where T : GenericCacheElement
	{
		#region Member Variables

		private int count;

		// MD 2/1/12 - TFS100573
		// To easily get the element indexes at save time, we will temporarily store cumulative counts of the buckets.
		// This works because we enumerate the buckets in order.
		private int[] cumulativeCountsForSaving;

		private Entry[][] entryLists;
		private int[] entryListsCounts;

		// MD 2/1/12 - TFS100573
		private bool isFrozen;

		private double maxLoadFactor;
		private int version;

		#endregion  // Member Variables

		#region Constructor

		public GenericElementHashSet(double maxLoadFactor)
		{
			this.maxLoadFactor = maxLoadFactor;
			int capacity = (int)(5 / this.maxLoadFactor);

			int prime = HashHelpers.GetPrime(capacity);
			this.entryLists = new Entry[prime][];
			this.entryListsCounts = new int[prime];
		}

		#endregion  // Constructor

		#region Methods

		#region Public Methods

		#region AddIfItemDoesntExist

		public bool AddIfItemDoesntExist(T element, out T existingElement)
		{
			existingElement = default(T);

			// MD 2/1/12 - TFS100573
			if (this.isFrozen)
			{
				Utilities.DebugFail("The collection should not be modified when it is being saved.");
				return false;
			}

			if (element == null)
			{
				Utilities.DebugFail("This should not be null.");
				return false;
			}

			int hashCode = HashHelpers.InternalGetHashCode(element);
			int index = hashCode % this.entryLists.Length;
			Entry[] entryList = this.entryLists[index];
			int entryListCount = this.entryListsCounts[index];
			if (entryList != null)
			{
				for (int i = 0; i < entryListCount; i++)
				{
					Entry entry = entryList[i];
					if (entry.hashCode == hashCode && entry.element.Equals(element))
					{
						existingElement = entry.element;
						return false;
					}
				}
			}
			else
			{
				entryList = new Entry[this.GetDefaultEntryListSize(this.entryLists.Length)];
				this.entryLists[index] = entryList;
			}

			// MD 7/21/11 - TFS82020
			// Instead of ensuring capacity for the next entry after the current entry is added, we should really be ensuring 
			// capacity for the current entry before adding it. Moved the code below to helper method EnsureEntryListCapacity
 			// and called it before adding the entry.
			GenericElementHashSet<T>.EnsureEntryListCapacity(this.entryLists, ref entryList, entryListCount, index);

			entryList[entryListCount++] = new Entry(element, hashCode);
			this.entryListsCounts[index] = entryListCount;

			// MD 7/21/11 - TFS82020
			// Moved this code above. See comment above.
			//if (entryListCount == entryList.Length)
			//{
			//    Entry[] newEntryList = new Entry[entryList.Length * 2];
			//    Array.Copy(entryList, newEntryList, entryListCount);
			//    this.entryLists[index] = newEntryList;
			//}

			this.count++;
			this.version++;

			if (entryListCount > this.entryLists.Length * this.maxLoadFactor)
				this.IncreaseCapacity();

			return true;
		}

		#endregion  // AddIfItemDoesntExist

		#region Contains

		public bool Contains(T element, out T existingElement)
		{
			existingElement = default(T);

			if (this.entryLists == null || element == null)
				return false;

			int hashCode = HashHelpers.InternalGetHashCode(element);
			int index = hashCode % this.entryLists.Length;
			Entry[] entryList = this.entryLists[index];
			if (entryList != null)
			{
				int entryListCount = this.entryListsCounts[index];
				for (int i = 0; i < entryListCount; i++)
				{
					Entry entry = entryList[i];
					if (entry.hashCode == hashCode && entry.element.Equals(element))
					{
						existingElement = entry.element;
						return true;
					}
				}
			}

			return false;
		}

		#endregion  // Contains

		#region CopyTo

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex", SR.GetString("LE_ArgumentOutOfRangeException_CollectionIndex"));

			if (arrayIndex > array.Length || this.count > (array.Length - arrayIndex))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_ArrayTooSmall"), "array");

			if (this.count == 0)
				return;

			int offset = 0;
			for (int i = 0; i < this.entryLists.Length; i++)
			{
				Entry[] entryList = this.entryLists[i];

				if (entryList == null)
					continue;

				int entryListCount = this.entryListsCounts[i];
				for (int j = 0; j < entryListCount; j++)
				{
					array[arrayIndex + offset] = entryList[j].element;
					offset++;
				}
			}
		}

		#endregion  // CopyTo

		#region GetEntryList

		// MD 2/1/12 - TFS100573
		//public Entry[] GetEntryList(int hashCode)
		//{
		//    return this.entryLists[hashCode % this.entryLists.Length];
		//}
		public Entry[] GetEntryList(int hashCode, out int entryListCount)
		{
			int cumulativeCount;
			return this.GetEntryList(hashCode, out entryListCount, out cumulativeCount);
		}

		// MD 2/1/12 - TFS100573
		public Entry[] GetEntryList(int hashCode, out int entryListCount, out int cumulativeCount)
		{
			int index = hashCode % this.entryLists.Length;

			if (this.cumulativeCountsForSaving == null)
				cumulativeCount = 0;
			else
				cumulativeCount = this.cumulativeCountsForSaving[index];

			entryListCount = this.entryListsCounts[index];
			return this.entryLists[index];
		}

		#endregion  // GetEntryList

		#region GetEnumerator

		public IEnumerator<T> GetEnumerator()
		{
			if (this.entryLists == null)
				yield break;

			int originalVersion = this.version;

			for (int i = 0; i < this.entryLists.Length; i++)
			{
				Entry[] entryList = this.entryLists[i];

				if (entryList == null)
					continue;

				int entryListCount = this.entryListsCounts[i];
				for (int j = 0; j < entryListCount; j++)
				{
					yield return entryList[j].element;

					if (originalVersion != this.version)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CollectionModifiedWhileEnumerating"));
				}
			}
		}

		#endregion  // GetEnumerator

		// MD 2/1/12 - TFS100573
		#region OnSaving

		public void OnSaving()
		{
			Debug.Assert(this.isFrozen == false && this.cumulativeCountsForSaving == null, "Something is wrong.");
			this.cumulativeCountsForSaving = new int[this.entryListsCounts.Length];
			this.isFrozen = true;

			int totalCount = 0;
			for (int i = 0; i < this.entryListsCounts.Length; i++)
			{
				this.cumulativeCountsForSaving[i] = totalCount;
				totalCount += this.entryListsCounts[i];
			}
		}

		#endregion // OnSaving

		// MD 2/1/12 - TFS100573
		#region OnSaved

		public void OnSaved()
		{
			Debug.Assert(this.isFrozen && this.cumulativeCountsForSaving != null, "Something is wrong.");
			this.cumulativeCountsForSaving = null;
			this.isFrozen = false;
		}

		#endregion // OnSaved

		#region Remove

		public bool Remove(T element)
		{
			// MD 2/1/12 - TFS100573
			if (this.isFrozen)
			{
				Utilities.DebugFail("The collection should not be modified when it is being saved.");
				return false;
			}

			if (this.entryLists == null)
				return false;

			if (element == null)
			{
				Utilities.DebugFail("This should not be null.");
				return false;
			}

			int hashCode = HashHelpers.InternalGetHashCode(element);
			int index = hashCode % this.entryLists.Length;
			Entry[] entryList = this.entryLists[index];
			if (entryList != null)
			{
				int entryListCount = this.entryListsCounts[index];
				for (int i = 0; i < entryListCount; i++)
				{
					Entry entry = entryList[i];
					if (entry.hashCode == hashCode && entry.element.Equals(element))
					{
						entryListCount--;
						this.entryListsCounts[index] = entryListCount;

						if (i < entryListCount)
							Array.Copy(entryList, i + 1, entryList, i, entryListCount - i);

						entryList[entryListCount] = new Entry();

						this.count--;
						this.version++;
						return true;
					}
				}
			}

			return false;
		}

		#endregion  // Remove

		#endregion  // Public Methods

		#region Private Methods

		// MD 7/21/11 - TFS82020
		#region EnsureEntryListCapacity

		private static void EnsureEntryListCapacity(Entry[][] entryLists, ref Entry[] currentEntryList, int currentEntryListCount, int currentEntryListIndex)
		{
			if (currentEntryListCount < currentEntryList.Length)
				return;

			Entry[] replacementEntryList = new Entry[currentEntryList.Length * 2];
			Array.Copy(currentEntryList, replacementEntryList, currentEntryListCount);
			entryLists[currentEntryListIndex] = replacementEntryList;

			currentEntryList = replacementEntryList;
		}

		#endregion  // EnsureEntryListCapacity

		#region GetDefaultEntryListSize

		private int GetDefaultEntryListSize(int entryListsSize)
		{
			return (int)Math.Ceiling(entryListsSize * this.maxLoadFactor / 2) + 1;
		}

		#endregion  // GetDefaultEntryListSize

		#region IncreaseCapacity

		private void IncreaseCapacity()
		{
			int min = this.entryLists.Length * 2;
			if (min < 0)
				min = this.entryLists.Length + 1;

			int prime = HashHelpers.GetPrime(min);
			if (prime <= this.entryLists.Length)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_HashSetFull"));

			Entry[][] newEntryLists = new Entry[prime][];
			int[] newEntryListCounts = new int[prime];

			for (int i = 0; i < this.entryLists.Length; i++)
			{
				Entry[] entryList = this.entryLists[i];

				if (entryList == null)
					continue;

				int entryListCount = this.entryListsCounts[i];
				for (int j = 0; j < entryListCount; j++)
				{
					Entry entry = entryList[j];

					int index = entry.hashCode % prime;
					Entry[] newEntryList = newEntryLists[index];
					if (newEntryList == null)
					{
						newEntryList = new Entry[this.GetDefaultEntryListSize(newEntryLists.Length)];
						newEntryLists[index] = newEntryList;
					}

					int newEntryListCount = newEntryListCounts[index];

					// MD 7/21/11
					// Found while fixing TFS82020.
					// Make sure the entry list is large enough to hold the new entry.
					GenericElementHashSet<T>.EnsureEntryListCapacity(newEntryLists, ref newEntryList, newEntryListCount, index);

					newEntryList[newEntryListCount++] = entry;
					newEntryListCounts[index] = newEntryListCount;
				}

				// MD 2/1/12 - TFS100573
				// We can allow the current entry list to get garbage collected while we move to the next one by releasing it.
				this.entryLists[i] = null;
			}

			this.entryLists = newEntryLists;
			this.entryListsCounts = newEntryListCounts;
		}

		#endregion  // IncreaseCapacity

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Count

		public int Count
		{
			get { return this.count; }
		}

		#endregion  // Count

		#endregion  // Properties


		#region Entry struct

		internal struct Entry
		{
			public Entry(T element, int hashCode)
			{
				this.element = element;
				this.hashCode = hashCode;
			}

			internal readonly int hashCode;
			internal readonly T element;
		}

		#endregion  // Entry struct
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