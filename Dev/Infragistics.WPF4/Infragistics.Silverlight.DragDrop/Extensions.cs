using System;
using System.Windows;

namespace Infragistics.DragDrop
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class WeakReferenceExtensions
    {
        #region IList<WeakReference> Extensions

        public static void Add(this IList<WeakReference> list, DependencyObject item)
        {
            list.Add(new WeakReference(item));
        }

        public static bool Contains(this IList<WeakReference> list, DependencyObject item)
        {
            for (int i = list.Count - 1; i > -1; i--)
            {
                if (list[i].Target == null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    if (list[i].Target == item)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void Remove(this IList<WeakReference> list, DependencyObject item)
        {
            for (int i = list.Count - 1; i > -1; i--)
            {
                if (list[i].Target == null || list[i].Target == item)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static IList<T> GetTargets<T>(this IList<WeakReference> list)
        {
            IList<T> resultList = new List<T>();
            for (int i = list.Count - 1; i > -1; i--)
            {
                object target = list[i].Target;
                if (target == null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    resultList.Add((T)target);
                }
            }

            return resultList;
        }

        #endregion // IList<WeakReference> Extensions

        #region IDictionary<WeakReference, IList<WeakReference>> Extensions

        public static void Add(this Dictionary<WeakReference, IList<WeakReference>> list, DependencyObject key, IList<DependencyObject> value)
        {
            List<WeakReference> weakReferences = new List<WeakReference>();
            foreach (DependencyObject DependencyObject in value)
            {
                weakReferences.Add(new WeakReference(DependencyObject));
            }

            list.Add(new WeakReference(key), weakReferences);
        }

        public static void Remove(this Dictionary<WeakReference, IList<WeakReference>> list, DependencyObject key)
        {
            IList<WeakReference> keys = list.Keys.ToList();

            for (int i = keys.Count - 1; i > -1; i--)
            {
                if (keys[i].Target == null || keys[i].Target == key)
                {
                    list.Remove(keys[i]);
                }
            }
        }

        public static bool ContainsKey(this Dictionary<WeakReference, IList<WeakReference>> list, DependencyObject key)
        {
            IList<WeakReference> keys = list.Keys.ToList();

            for (int i = keys.Count - 1; i > -1; i--)
            {
                if (keys[i].Target == null)
                {
                    list.Remove(keys[i]);
                }
                else
                {
                    if (keys[i].Target == key)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryGetValue(this Dictionary<WeakReference, IList<WeakReference>> list, DependencyObject key, out IList<WeakReference> items)
        {
            items = null;
            IList<WeakReference> keys = list.Keys.ToList();

            for (int i = keys.Count - 1; i > -1; i--)
            {
                if (keys[i].Target == null)
                {
                    list.Remove(keys[i]);
                }
                else
                {
                    if (keys[i].Target == key)
                    {
                        items = list[keys[i]];
                        if (items != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion // IDictionary<WeakReference, IList<WeakReference>> Extensions
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