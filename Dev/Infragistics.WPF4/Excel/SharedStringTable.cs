using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;





namespace Infragistics.Documents.Excel
{
	// MD 4/12/11 - TFS67084
	// MD 5/31/11 - TFS75574
	// Marked this as sealed.
	//internal class SharedStringTable : GenericCachedCollection<FormattedStringElement>
	internal sealed class SharedStringTable : GenericCachedCollection<StringElement>
	{
		#region Member Variables

		private uint nextKey = 1;

		// MD 5/31/11 - TFS75574
		// We no longer need a separate lookup table for the keys. If we store the hash code and the key of the string element, we
		// can just as easily find the element in the main element lookup table. This cuts the space for the shared string table in half.
		//private SortedList<uint, FormattedStringElement> values = new SortedList<uint, FormattedStringElement>(); 
		private List<KeyGap> availableKeysSorted = new List<KeyGap>();

		#endregion  // Member Variables

		#region Constructor

		// MD 2/2/12 - TFS100573
		// The default element has been moved to the GenericCachedCollectionEx.
		//public SharedStringTable(StringElement defaultElement, Workbook workbook)
		public SharedStringTable(Workbook workbook)
			// MD 5/31/11 - TFS75574
			// Use a very low load factor because with extremely large numbers of strings, it doesn't seem to affect memory usage too much
			// but it dramatically increases performance.
			//: base(defaultElement, workbook, Int32.MaxValue) { } 
			// MD 1/31/12 - TFS100573
			// Adjusted this slightly for better memory performance.
			//: base(defaultElement, workbook, Int32.MaxValue, 0.01) { } 
			// MD 2/2/12 - TFS100573
			// The default element has been moved to the GenericCachedCollectionEx.
			//: base(defaultElement, workbook, Int32.MaxValue, 0.005) { } 
			: base(workbook, Int32.MaxValue, 0.005) { } 

		#endregion  // Constructor

		#region Base Class Overrides

		// MD 5/31/11 - TFS75574
		// This method is not used by default for adds anymore. Override OnAdded instead.
		#region Removed

		//#region Add
		//
		//public override void Add(FormattedStringElement element)
		//{
		//    base.Add(element);
		//
		//    if (element.Key == 0)
		//        element.Key = Utilities.FindNextSharedElementKey(this.values, ref this.nextKey);
		//
		//    this.values.Add(element.Key, element);
		//}
		//
		//#endregion  // Add

		#endregion  // Removed

		// MD 5/31/11 - TFS75574
		#region FindNextSharedElementKey

		private uint FindNextSharedElementKey(ref uint nextKey)
		{
			if (nextKey == UInt32.MaxValue)
			{
				if (this.availableKeysSorted.Count != 0)
				{
					uint index = this.availableKeysSorted[0].startIndex;
					this.OnKeyUnavialable(index);
					return index;
				}

				Utilities.DebugFail("This should never happen.");
				return 0;
			}
			else
			{
				return nextKey++;
			}
		}

		#endregion  // FindNextSharedElementKey 

		// MD 5/31/11 - TFS75574
		#region OnAdded

		protected override void OnAdded(StringElement element)
		{
			base.OnAdded(element);

			if (element.Key == 0)
				element.Key = this.FindNextSharedElementKey(ref this.nextKey);
		}

		#endregion  // OnAdded

		// MD 5/31/11 - TFS75574
		#region OnKeyAvialable

		private void OnKeyAvialable(uint key)
		{
			KeyGap standAloneGap = new KeyGap(key, key);
			int searchIndex = this.availableKeysSorted.BinarySearch(standAloneGap);

			KeyGap? previousGap = null;
			KeyGap? nextGap = null;

			int previousGapIndex;
			int nextGapIndex;

			if (searchIndex < 0)
			{
				searchIndex = ~searchIndex;
				previousGapIndex = searchIndex - 1;

				if (0 <= previousGapIndex)
				{
					previousGap = this.availableKeysSorted[previousGapIndex];
					Debug.Assert(previousGap.Value.endIndex < key, "This shouldn't have happened.");
				}

				nextGapIndex = searchIndex;
			}
			else
			{
				Utilities.DebugFail("This shouldn't have happened.");
				previousGapIndex = searchIndex;
				previousGap = this.availableKeysSorted[previousGapIndex];

				nextGapIndex = searchIndex + 1;
			}

			if (nextGapIndex < this.availableKeysSorted.Count)
			{
				nextGap = this.availableKeysSorted[nextGapIndex];
				Debug.Assert(key < nextGap.Value.startIndex, "This shouldn't have happened.");
			}

			bool bordersPreviousGap = previousGap.HasValue && previousGap.Value.endIndex + 1 == key;
			bool bordersNextGap = nextGap.HasValue && nextGap.Value.startIndex - 1 == key;

			if (bordersPreviousGap)
			{
				if (bordersNextGap)
				{
					this.availableKeysSorted.RemoveRange(previousGapIndex, 2);
					this.availableKeysSorted.Insert(previousGapIndex, new KeyGap(previousGap.Value.startIndex, nextGap.Value.endIndex));
				}
				else
				{
					this.availableKeysSorted[previousGapIndex] = new KeyGap(previousGap.Value.startIndex, key);
				}
			}
			else if (bordersNextGap)
			{
				this.availableKeysSorted[nextGapIndex] = new KeyGap(key, nextGap.Value.endIndex);
			}
			else
			{
				this.availableKeysSorted.Insert(searchIndex, standAloneGap);
			}
		}

		#endregion  // OnKeyAvialable

		// MD 5/31/11 - TFS75574
		#region OnKeyUnavialable

		private void OnKeyUnavialable(uint key)
		{
			int index = this.availableKeysSorted.BinarySearch(new KeyGap(key, key));

			if (index < 0)
			{
				index = ~index - 1;

				if (index < 0)
					return;

				KeyGap gap = this.availableKeysSorted[index];
				Debug.Assert(gap.startIndex != key, "Not sure how this happened.");

				if (gap.endIndex < key)
					return;

				if (gap.endIndex == key)
				{
					this.availableKeysSorted[index] = new KeyGap(gap.startIndex, gap.endIndex - 1);
				}
				else
				{
					this.availableKeysSorted.RemoveAt(index);
					this.availableKeysSorted.InsertRange(index, new KeyGap[] { 
						new KeyGap(gap.startIndex, key - 1), 
						new KeyGap(key + 1, gap.endIndex) });
				}
			}
			else
			{
				KeyGap gap = this.availableKeysSorted[index];

				if (gap.startIndex == gap.endIndex)
					this.availableKeysSorted.RemoveAt(index);
				else
					this.availableKeysSorted[index] = new KeyGap(gap.startIndex + 1, gap.endIndex);
			}
		}

		#endregion  // OnKeyUnavialable

		#region Remove

		public override bool Remove(StringElement element)
		{
			if (base.Remove(element) == false)
				return false;

			// MD 5/31/11 - TFS75574
			//this.values.Remove(element.Key);
			this.OnKeyAvialable(element.Key);
			element.Key = 0;

			return true;
		}

		#endregion  // Remove

		#endregion  // Base Class Overrides

		#region Find

		// MD 5/31/11 - TFS75574
		// Now we use both the hash code and the key to find the string. By doing this, we can remove the key lookup table and
		// still maintain the same performance.
		//public FormattedStringElement Find(uint key)
		//{
		//    FormattedStringElement element;
		//    if (this.values.TryGetValue(key, out element))
		//        return element;
		//
		//    return null;
		//}
		public StringElement Find(int hashCode, uint key)
		{
			// MD 2/1/12
			// Found while fixing TFS100573
			// We need to get the count so we know when to stop iterating.
			//GenericElementHashSet<StringElement>.Entry[] elements = this.cache.GetEntryList(hashCode);
			int entryListCount;
			GenericElementHashSet<StringElement>.Entry[] entryList = this.cache.GetEntryList(hashCode, out entryListCount);

			if (entryList != null)
			{
				// MD 2/1/12
				// Found while fixing TFS100573
				//for (int i = 0; i < elements.Length; i++)
				for (int i = 0; i < entryListCount; i++)
				{
					// MD 2/1/12
					// Found while fixing TFS100573
					// Skip past the entries which don't have the same hash code, plus we don't have to check for null now that 
					// we have the real count.
					//StringElement element = elements[i].element;
					//
					//if (element == null)
					//    break;

					GenericElementHashSet<StringElement>.Entry entry = entryList[i];
					if (entry.hashCode != hashCode)
						continue;

					StringElement element = entry.element;

					if (element.Key == key)
						return element;
				}
			}

			return null;
		}

		#endregion  // Find

		// MD 2/1/12 - TFS100573
		#region FindStringIndex

		internal int FindStringIndex(StringElement element)
		{
			return this.FindStringIndex(HashHelpers.InternalGetHashCode(element), element.Key);
		}

		internal int FindStringIndex(int hashCode, uint key)
		{
			int entryListCount;
			int cumulativeCount;
			GenericElementHashSet<StringElement>.Entry[] entryList = this.cache.GetEntryList(hashCode, out entryListCount, out cumulativeCount);

			if (entryList != null)
			{
				for (int i = 0; i < entryListCount; i++)
				{
					GenericElementHashSet<StringElement>.Entry entry = entryList[i];
					if (entry.hashCode != hashCode)
						continue;

					StringElement element = entry.element;

					if (element == null)
						break;

					if (element.Key == key)
						return cumulativeCount + i;
				}
			}

			Utilities.DebugFail("This shouldn't happen.");
			return -1;
		}

		#endregion // FindStringIndex

		// MD 5/31/11 - TFS75574
		#region KeyGap struct

		internal struct KeyGap : IComparable<KeyGap>
		{
			internal readonly uint startIndex;
			internal readonly uint endIndex;

			public KeyGap(uint startIndex, uint endIndex)
			{
				this.startIndex = startIndex;
				this.endIndex = endIndex;
			}

			#region IComparable<KeyGap> Members

			public int CompareTo(KeyGap other)
			{
				return this.startIndex.CompareTo(other.startIndex);
			}

			#endregion
		}

		#endregion  // KeyGap struct
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