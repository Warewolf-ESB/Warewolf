using System;
using System.Collections.Generic;

namespace Infragistics
{
    /// <summary>
    /// Represents a list that store weak references to values. This list allows to
    /// store values while still allowing that the values can be reclaimed by garbage 
    /// collection and avoid memory leaks.
    /// </summary>
    /// <typeparam name="T">Type of the values.</typeparam>
    /// <remarks>
    /// If a value is added to the list and after that it is released by the
    /// garbage collector, it is removed from the list automatically when the
    /// it detects that the weak reference is dead.    
    /// </remarks>
    public class WeakReferenceHelper<T>
        where T : class
    {
        private readonly List<WeakReference> _values;

		/// <summary>
		/// A List of WeakReferences for the RecyclingManager
		/// </summary>
		public List<WeakReference> Values
		{
			get
			{
				return this._values;
			}			
		}

        /// <summary>
        /// Initialize a new instance of the WeakReferenceHelper class.
        /// </summary>
        public WeakReferenceHelper()
        {
            this._values = new List<WeakReference>();
        }

        /// <summary>
        /// Determines whether the access control list contains a specific 
        /// access control entry.
        /// </summary>
        /// <param name="item">The item to locate in the list.</param>
        /// <returns>true if the item is found in the list; otherwise, false.</returns>
        public bool Contains(T item)
        {
            for (int i = 0; i < this._values.Count; i++)
            {
                if (this._values[i].Target == item)
                {                    
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserts an item at the end of the list.
        /// </summary>
        /// <param name="item">Item.</param>
        public void Add(T item)
        {
            if (this._values.Count == this._values.Capacity)
            {
                this.CleanValues();
            }

            this._values.Add(new WeakReference(item));
        }

		/// <summary>
		/// Adds a range of items to the list
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(IEnumerable<WeakReference> items)
		{
			if (items != null)
			{
				this._values.AddRange(items);
			}
		}

        /// <summary>
        /// Removes and returns the last item of the list.
        /// </summary>
        /// <returns>The item removed from the list. null if the list is empty.</returns>
        public T RemoveLast()
        {
            int k = this._values.Count - 1;
            while (k >= 0)
            {
                WeakReference value = this._values[k];
                this._values.RemoveAt(k);

                var valueT = value.Target as T;
                if (valueT != null)
                {
                    // The object is alive, return it
                    return valueT;
                }
            }

            return null;
        }

        /// <summary>
        /// Remove the first occurrence of the specified item from the list.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>true if the item was successfully removed; false if item was not found in the list.</returns>
        public bool Remove(T item)
        {
            for (int i = 0; i < this._values.Count; i++)
            {
                if (this._values[i].Target == item)
                {
                    this._values.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs the specified action on each item of the list.
        /// </summary>
        /// <param name="action">
        /// Action that is invoked with the item as argument. If it returns true the
        /// for each continues with the next item, if it returns false the for each
        /// stops (like including a break in code).
        /// </param>
        public void ForEach(Func<T, bool> action)
        {
            int k = 0;
            while (k < this._values.Count)
            {
                WeakReference value = this._values[k];
                
                var valueT = value.Target as T;
                if (valueT != null)
                {
                    // The object is alive, execute the action
                    if (!action(valueT))
                        break;
                    k++;
                }
                else
                {
                    // The object is not alive, remove it from the list
                    this._values.RemoveAt(k);
                }
            }
        }

		/// <summary>
		/// Gets the total amount of objects from the collection. 
		/// </summary>
		public int Count
		{
			get 
			{
				int count = 0;

				foreach (WeakReference obj in this._values)
				{
					if (obj.IsAlive)
						count++;
				}

				return count;
			}
		}

		/// <summary>
		/// Gets the item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{
				int count = 0;
				foreach (WeakReference obj in this._values)
				{
					if (obj.IsAlive)
						count++;

					if ((count - 1) == index)
						return (T)obj.Target;
				}

				return null;
			}
		}

        /// <summary>
        /// Remove the items from the list that were reclaimed by the garbage collector.
        /// </summary>
        private void CleanValues()
        {
            int k = this._values.Count - 1;
            while (k >= 0)
            {
                WeakReference value = this._values[k];

                var valueT = value.Target as T;
                if (valueT == null)
                {
                    // The object is not alive, remove it from the list
                    this._values.RemoveAt(k);
                }

                k--;
            }
        }
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