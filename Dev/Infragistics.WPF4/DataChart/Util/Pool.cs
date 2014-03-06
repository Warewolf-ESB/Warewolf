using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Represents a pool of reusable objects.
    /// </summary>
    /// <typeparam name="T">Pooled object type. Must be nullable</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Pool<T>
        : IIndexedPool<T>
    {
        /// <summary>
        /// Constructs a new Pool instance.
        /// </summary>
        public Pool()
        {
            Active = new List<T>();
            Inactive = new List<T>();
        }

        /// <summary>
        /// Gets or sets the function used to create new items.
        /// </summary>
        public Func<T> Create { get; set; }

        /// <summary>
        /// Gets or sets the function used to disactivate items.
        /// </summary>
        public Action<T> Disactivate { get; set; }

        /// <summary>
        /// Gets or sets the function used to activate items.
        /// </summary>
        public Action<T> Activate { get; set; }

        /// <summary>
        /// Gets or sets the function used to destroy old items.
        /// </summary>
        public Action<T> Destroy { get; set; }

        /// <summary>
        /// Gets the indexed item, extendening the pool and creating the item
        /// if necessary.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Item</returns>
        public T this[int index]
        {
            get
            {
			    if(index>=Count)
			    {
				    Count=index+1;
			    }

			    return Active[index];			
            }
        }

        /// <summary>
        /// Clears the items from the pool.
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }

        /// <summary>
        /// Gets or sets the count of the items that are in the pool.
        /// </summary>
        /// <remarks>
        /// Setting the count to less than the current value will result in pool
        /// items automatically being deactivated.
        /// </remarks>
	    public int Count
	    {
            get { return Active.Count; }

		    set {
			    int activeCount=Active.Count;

			    while(value>Active.Count && Inactive.Count>0)
			    {
				    T item=Inactive[Inactive.Count-1];
                    Active.Add(item);
				    Inactive.RemoveAt(Inactive.Count-1);

                    if (Activate != null)
                    {
                        Activate(item);
                    }
			    }

			    while(value>Active.Count && Inactive.Count==0)
			    {
				    T item=Create();
                    Active.Add(item);

                    if (Activate != null)
                    {
                        Activate(item);
                    }
			    }

                // find the upper limit for inactive items

                int inactiveCount = 2;

                while (activeCount != 0)
                {
                    activeCount >>= 1;
                    inactiveCount <<= 1;
                }

			    // removing items

			    while(value<Active.Count)
			    {
				    T item=Active[Active.Count-1];

				    Active.RemoveAt(Active.Count-1);

                    if (Disactivate != null)
                    {
                        Disactivate(item);
                    }

                    if (Inactive.Count < inactiveCount)
                    {
                        Inactive.Add(item);
                    }
                    else
                    {
                        if (Destroy != null)
                        {
                            Destroy(item);
                        }
                    }
			    }

			    if(inactiveCount<Inactive.Count)
			    {
                    for (int i = inactiveCount; i < Inactive.Count; ++i)
				    {
					    Destroy(Inactive[i]);
				    }

                    Inactive.RemoveRange(inactiveCount, Inactive.Count - inactiveCount);
			    }
		    }
	    }

        /// <summary>
        /// The list of active objects.
        /// </summary>
        public List<T> Active { get; set; }
        /// <summary>
        /// The list of inactive objects.
        /// </summary>
        public List<T> Inactive { get; set; }

        /// <summary>
        /// Perfoms an action on all the items in the pool.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void DoToAll(Action<T> action)
        {
            foreach (var item in Inactive)
            {
                action(item);
            }

            foreach (var item in Active)
            {
                action(item);
            }
        }
    }

    /// <summary>
    /// Represents a pool of reusable objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool.</typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// Gets or sets the function used to create new items.
        /// </summary>
        Func<T> Create { get; set; }

        /// <summary>
        /// Gets or sets the function used to disactivate items.
        /// </summary>
        Action<T> Disactivate { get; set; }

        /// <summary>
        /// Gets or sets the function used to activate items.
        /// </summary>
        Action<T> Activate { get; set; }

        /// <summary>
        /// Gets or sets the function used to destroy old items.
        /// </summary>
        Action<T> Destroy { get; set; }

        /// <summary>
        /// Clear the values from the pool.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// An IPool that is indexed by integer indexes.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool.</typeparam>
    public interface IIndexedPool<T>
        : IPool<T>
    {
        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="index">The index for which to get the item.</param>
        /// <returns>The requested item.</returns>
        T this[int index] { get; }
    }

    /// <summary>
    /// An hash mapped IPool.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys for the pool.</typeparam>
    /// <typeparam name="TValue">The type of the values in the pool.</typeparam>
    public interface IHashPool<TKey, TValue>
        : IPool<TValue>
    {
        /// <summary>
        /// Gets the item for the specified key.
        /// </summary>
        /// <param name="key">The key for which to get the item.</param>
        /// <returns>The requested item.</returns>
        TValue this[TKey key] { get; }
    }

    /// <summary>
    /// An implementation of a hash mapped pool.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys for the pool.</typeparam>
    /// <typeparam name="TValue">The type of the values in the pool.</typeparam>
    public class HashPool<TKey, TValue>
        : IHashPool<TKey, TValue>
    {
        /// <summary>
        /// The inactive members of the pool.
        /// </summary>
        protected List<TValue> Inactive { get; set; }
        /// <summary>
        /// The active members of the pool.
        /// </summary>
        protected Dictionary<TKey, TValue> Active { get; set; }

        /// <summary>
        /// Constructs a new HashPool instance.
        /// </summary>
        public HashPool()
        {
            Inactive = new List<TValue>();
            Active = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Gets or sets the function used to create new items.
        /// </summary>
        public Func<TValue> Create { get; set; }

        /// <summary>
        /// Gets or sets the function used to disactivate items.
        /// </summary>
        public Action<TValue> Disactivate { get; set; }

        /// <summary>
        /// Gets or sets the function used to activate items.
        /// </summary>
        public Action<TValue> Activate { get; set; }

        /// <summary>
        /// Gets or sets the function used to destroy old items.
        /// </summary>
        public Action<TValue> Destroy { get; set; }

        /// <summary>
        /// Gets the requested item, extendening the pool and creating the item
        /// if necessary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Item</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue ret;
                if (!Active.TryGetValue(key, out ret))
                {
                    if (Inactive.Count > 0)
                    {
                        ret = Inactive[Inactive.Count - 1];
                        Inactive.RemoveAt(Inactive.Count - 1);
                    }
                    else
                    {
                        ret = Create();
                    }

                    if (Activate != null)
                    {
                        Activate(ret);
                    }
                    Active[key] = ret;
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets the keys of the active pool objects.
        /// </summary>
        public IEnumerable<TKey> ActiveKeys
        {
            get
            {
                return Active.Keys;
            }
        }

        /// <summary>
        /// Returns whether the provided key is in the active set.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is in the active set.</returns>
        public bool IsActiveKey(TKey key)
        {
            return Active.ContainsKey(key);
        }

        /// <summary>
        /// Removes the item with the provided key from the pool.
        /// </summary>
        /// <param name="key">The key for the item to remove.</param>
        public void Remove(TKey key)
        {
            TValue remove;
            if (Active.TryGetValue(key, out remove))
            {
                Active.Remove(key);

                if (Disactivate != null)
                {
                    Disactivate(remove);
                }

                Inactive.Add(remove);

                int activeCount = Active.Count;
                int inactiveCount = 2;

                while (activeCount != 0)
                {
                    activeCount >>= 1;
                    inactiveCount <<= 1;
                }

                if (inactiveCount < Inactive.Count)
                {
                    for (int i = inactiveCount; i < Inactive.Count; ++i)
                    {
                        Destroy(Inactive[i]);
                    }

                    Inactive.RemoveRange(inactiveCount, Inactive.Count - inactiveCount);
                }
            }
        }

        /// <summary>
        /// Clears the items from the hash pool.
        /// </summary>
        public void Clear()
        {
            List<TKey> deactivate = new List<TKey>();
            foreach (TKey active in Active.Keys)
            {
                deactivate.Add(active);
            }

            foreach (TKey key in deactivate)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// Gets the count of the number of actives.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                return Active.Count;
            }
        }

        /// <summary>
        /// Perfoms an action on all the items in the pool.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void DoToAll(Action<TValue> action)
        {
            foreach (var item in Inactive)
            {
                action(item);
            }

            foreach (var item in Active.Values)
            {
                action(item);
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