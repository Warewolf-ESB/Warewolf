using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Represents a pool of reusable objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StackPool<T>
    {
        /// <summary>
        /// Gets an object from the pool.
        /// </summary>
        /// <remarks>
        /// The pool will either Create a new object or Activate one which was previously returned
        /// and Disactivated. If DeferDisactivate is set there may be pending active objects
        /// available which will be reused directly).
        /// </remarks>
        /// <returns>An object.</returns>
        public T Pop()
        {
            T t;

            if (limbo.Count != 0)
            {
                t = limbo.Pop();
            }
            else
            {
                t = inactive.Count != 0 ? inactive.Pop() : Create();
                Activate(t);
            }

            active.Add(t, null);
            return t;
        }

        /// <summary>
        /// Returns an object to the pool for recycling.
        /// </summary>
        /// <remarks>
        /// All references to objects which have been returned to the pool should be destroyed. 
        /// <para>
        /// When an object is returned to the pool it will be immediately Disactivated (unless
        /// DeferDisactivate is set) and may also be Destroyed either immediately or some time
        /// later. 
        /// </para>
        /// </remarks>
        /// <param name="t"></param>
        public void Push(T t)
        {
            active.Remove(t);

            if (DeferDisactivate)
            {
                limbo.Push(t);
            }
            else
            {
                Deactivate(t);

                int inactiveCount = RoundUp(active.Count);

                if (inactive.Count < inactiveCount)
                {
                    Destroy(t);
                }
                else
                {
                    inactive.Push(t);
                }
            }
        }

        /// <summary>
        /// Sets or gets the DeferDisactivate flag.
        /// </summary>
        /// <remarks>
        /// When the pool is marked to defer disactivation, objects returned to the pool are
        /// not immediately disactivated, instead remaining in a limbo state where they are
        /// available for reuse without Activation. Resetting DeferDisactivate causes all of 
        /// these limbo objects to be disactivated and potentially destroyed.
        /// <para>
        /// Deferred disactivation is useful where the activation/disactivation cycle is costle
        /// (such as add/remove a VisualElement from a ParentItem Panel).
        /// </para>
        /// </remarks>
        public bool DeferDisactivate
        {
            get { return deferDisactivate; }
            set
            {
                if (deferDisactivate != value)
                {
                    deferDisactivate = value;

                    if (!deferDisactivate)
                    {
                        int inactiveCount = RoundUp(active.Count);

                        while (limbo.Count > 0 && inactive.Count <= inactiveCount)
                        {
                            T t = limbo.Pop();

                            Deactivate(t);
                            inactive.Push(t);
                        }

                        while (limbo.Count > 0)
                        {
                            T t = limbo.Pop();

                            Deactivate(t);
                            Destroy(t);
                        }

                        while (inactive.Count > inactiveCount)
                        {
                            Destroy(inactive.Pop());
                        }
                    }
                }
            }
        }
        private bool deferDisactivate = false;

        /// <summary>
        /// Gets the number of active items in the current StackPool object.
        /// </summary>
        public int ActiveCount { get { return active.Count; } }

        /// <summary>
        /// Gets the number of inactive (not including limbo) items in the
        /// current StackPool object.
        /// </summary>
        public int InactiveCount { get { return inactive.Count; } }

         /// <summary>
        /// Gets or sets the function used to create new items.
        /// </summary>
        public Func<T> Create { get; set; }

        /// <summary>
        /// Gets or sets the function used to disactivate items.
        /// </summary>
        public Action<T> Deactivate { get; set; }

        /// <summary>
        /// Gets or sets the function used to activate items.
        /// </summary>
        public Action<T> Activate { get; set; }

        /// <summary>
        /// Gets or sets the function used to destroy old items.
        /// </summary>
        public Action<T> Destroy { get; set; }

        private static int RoundUp(int a)
        {
            int p = 2;

            while (a > p)
            {
                p = p << 1;
            }

            return p;
        }

        /// <summary>
        /// The active object collection.
        /// </summary>
        Dictionary<T, object> active = new Dictionary<T, object>();

        /// <summary>
        /// The limbo object collection.
        /// </summary>
        Stack<T> limbo = new Stack<T>();

        /// <summary>
        /// The inactive object collection.
        /// </summary>
        Stack<T> inactive = new Stack<T>();
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