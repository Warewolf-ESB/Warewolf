using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Base class for managing markers for a data series.
    /// </summary>
    public abstract class MarkerManagerBase
    {
        /// <summary>
        /// The function used to get all marker locations.
        /// </summary>
        public Func<Point[]> GetItemLocationsStrategy { get; set; }
        /// <summary>
        /// The function used to provide marker objects given a data item.
        /// </summary>
        public Func<object, Marker> ProvideMarkerStrategy { get; set; }
        /// <summary>
        /// The action used to remove unused markers.
        /// </summary>
        public Action<IDictionary<object, OwnedPoint>> RemoveUnusedMarkers { get; set; }
        /// <summary>
        /// The function used to provide a data item given its index.
        /// </summary>
        public Func<int, object> ProvideItemStrategy { get; set; }
        /// <summary>
        /// The function used to return the indexes of all active markers.
        /// </summary>
        public Func<IList<int>> ActiveMarkerIndexesStrategy { get; set; }
        internal static bool UseDeterministicSelection { get; set; }

        /// <summary>
        /// MarkerManagerBase constructor.
        /// </summary>
        /// <param name="provideMarkerStrategy">The function used to provide marker objects given a data item.</param>
        /// <param name="provideItemStrategy">The function used to provide a data item given its index.</param>
        /// <param name="removeUnusedMarkers">The action used to remove unused markers.</param>
        /// <param name="getItemLocationsStrategy">The function used to get all marker locations.</param>
        /// <param name="activeMarkerIndexesStrategy">The function used to return the indexes of all active markers.</param>
        protected MarkerManagerBase(Func<object, Marker> provideMarkerStrategy,
           Func<int, object> provideItemStrategy,
           Action<IDictionary<object, OwnedPoint>> removeUnusedMarkers,
           Func<Point[]> getItemLocationsStrategy,
           Func<IList<int>> activeMarkerIndexesStrategy
           )
        {
            ProvideMarkerStrategy = provideMarkerStrategy;
            ProvideItemStrategy = provideItemStrategy;
            RemoveUnusedMarkers = removeUnusedMarkers;
            GetItemLocationsStrategy = getItemLocationsStrategy;
            ActiveMarkerIndexesStrategy = activeMarkerIndexesStrategy;
        }
        /// <summary>
        /// Filter out markers that should not be visible.
        /// </summary>
        /// <param name="markers">A dictionary containing the locations of all markers.</param>
        /// <param name="maximumMarkers">The maximum number of markers which can be displayed.</param>
        /// <param name="windowRect">A rectangle representing the scroll window.  Rectangle coordinates are based on the range of zero to one.</param>
        /// <param name="viewportRect">A rectangle representing the viewport, in device coordinates.</param>
        /// <param name="currentResolution">The current Series Resolution.</param>
        public abstract void WinnowMarkers(IDictionary<object, OwnedPoint> markers, int maximumMarkers,
                                              Rect windowRect, Rect viewportRect, double currentResolution);
        
        /// <summary>
        /// Renders the given markers.
        /// </summary>
        /// <param name="markers">A dictionary containing the locations of all markers.</param>
        /// <param name="lightweight">Whether or not to enable a lower-fidelity rendering mode with better performance.</param>
        public abstract void Render(IDictionary<object, OwnedPoint> markers, bool lightweight);
        /// <summary>
        /// Creates a list of indices sorted with priority items in front.
        /// </summary>
        /// <param name="buckets">The buckets in context.</param>
        /// <param name="keys">The keys of all buckets in context.</param>
        /// <returns>The given keys, sorted with priority items first.</returns>
        protected virtual List<int> ActiveFirstKeys(Dictionary<int, MarkerManagerBucket> buckets, List<int> keys)
        {
            List<int> first = new List<int>();
            List<int> second = new List<int>();
            foreach (int key in keys)
            {
                if (buckets[key].PriorityItems.Count > 0)
                {
                    first.Add(key);
                }
                else
                {
                    second.Add(key);
                }
            }

            List<int> ret = new List<int>();
            ret.AddRange(first);
            ret.AddRange(second);
            return ret;
        }
        /// <summary>
        /// Selects a given number of marker buckets, and adds them to a given list of items.
        /// </summary>
        /// <param name="numToSelect">The count of buckets to select.</param>
        /// <param name="buckets">All buckets in context.</param>
        /// <param name="keys">All available keys.</param>
        /// <param name="markerItems">The list of items to add selected markers to.</param>
        protected virtual void SelectMarkerItems(int numToSelect,
            Dictionary<int, MarkerManagerBucket> buckets,
            List<int> keys,
            List<int> markerItems)
        {
            while (numToSelect > 0)
            {
                if (numToSelect < keys.Count)
                {
                    if (!UseDeterministicSelection)
                    {
                        keys.Shuffle();
                    }
                    keys = ActiveFirstKeys(buckets, keys);
                    int count = numToSelect;
                    for (int i = 0; i < count; i++)
                    {
                        int keyIndex = i;
                        MarkerManagerBucket bucket = buckets[keys[keyIndex]];
                        bool wasPriority;
                        int index = bucket.GetItem(out wasPriority);
                        //if (wasPriority)
                        //{
                        //    reusedActives++;
                        //}
                        markerItems.Add(index);
                        numToSelect--;
                        if (bucket.IsEmpty)
                        {
                            buckets.Remove(keys[keyIndex]);
                        }
                    }
                }
                else
                {
                    foreach (int key in keys)
                    {
                        MarkerManagerBucket bucket = buckets[key];
                        bool wasPriority;
                        int index = bucket.GetItem(out wasPriority);
                        //if (wasPriority)
                        //{
                        //    reusedActives++;
                        //}
                        markerItems.Add(index);
                        numToSelect--;
                        if (bucket.IsEmpty)
                        {
                            buckets.Remove(key);
                        }
                    }
                    keys = new List<int>(buckets.Keys);
                }
            }
        }
        /// <summary>
        /// Gets only the visible items and adds them to the given list.
        /// </summary>
        /// <param name="windowRect">The WindowRect in context.</param>
        /// <param name="viewportRect">The Viewport in context.</param>
        /// <param name="itemLocations">The locations of all items in context.</param>
        /// <param name="visibleItems">The list to add all visible items to.</param>
        protected virtual void GetVisibleItems(Rect windowRect, Rect viewportRect,
    IList<Point> itemLocations,
    List<int> visibleItems)
        {
            double left = viewportRect.Left;
            double right = viewportRect.Right;
            double top = viewportRect.Top;
            double bottom = viewportRect.Bottom;

            if (!windowRect.IsEmpty && !viewportRect.IsEmpty)
            {
                for (int i = 0; i < itemLocations.Count; ++i)
                {
                    double x = itemLocations[i].X;

                    if (double.IsNaN(x))
                    {
                        continue;
                    }

                    double y = itemLocations[i].Y;

                    if (double.IsNaN(y))
                    {
                        continue;
                    }

                    if (x < left || x > right)
                    {
                        continue;
                    }

                    if (y < top || y > bottom)
                    {
                        continue;
                    }

                    visibleItems.Add(i);
                }
            }
        }
        /// <summary>
        /// Groups visible items into relevant buckets, based on the viewport size and resolution.
        /// </summary>
        /// <param name="viewportRect">The Viewport in context.</param>
        /// <param name="visibleItems">A list of the indices of all visible markers.</param>
        /// <param name="resolution">The Resolution in context.</param>
        /// <param name="itemLocations">A list of the locations of all markers.</param>
        /// <returns>A Dictionary containing the buckets.</returns>
        protected virtual Dictionary<int, MarkerManagerBucket> GetBuckets(
            Rect viewportRect, List<int> visibleItems,
            double resolution, IList<Point> itemLocations)
        {
            bool[] wasActive = new bool[itemLocations.Count];
            foreach (int index in ActiveMarkerIndexesStrategy())
            {
                if (index != -1)
                {
                    wasActive[index] = true;
                }
            }

            int rowSize = (int)Math.Floor(viewportRect.Width / resolution);

            Dictionary<int, MarkerManagerBucket> buckets = new Dictionary<int, MarkerManagerBucket>();

            foreach (int index in visibleItems)
            {
                double xVal = itemLocations[index].X;
                double yVal = itemLocations[index].Y;

                int rowNumber = (int)Math.Floor(yVal / resolution);
                int colNumber = (int)Math.Floor(xVal / resolution);

                int offset = (rowNumber * rowSize) + colNumber;
                MarkerManagerBucket bucket;
                if (!buckets.TryGetValue(offset, out bucket))
                {
                    bucket = new MarkerManagerBucket();
                    buckets.Add(offset, bucket);
                }
                if (wasActive[index])
                {
                    bucket.PriorityItems.Add(index);
                }
                else
                {
                    bucket.Items.Add(index);
                }
            }
            return buckets;
        }
    }
    /// <summary>
    /// Class representing several markers being consolidated into one.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class MarkerManagerBucket
    {
        private List<int> _items;
        /// <summary>
        /// A list of the indices of items in this bucket.
        /// </summary>
        public List<int> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<int>();
                }
                return _items;
            }
        }

        private List<int> _priorityItems;
        /// <summary>
        /// A list of the indices of priority items in this bucket.
        /// </summary>
        public List<int> PriorityItems
        {
            get
            {
                if (_priorityItems == null)
                {
                    _priorityItems = new List<int>();
                }
                return _priorityItems;
            }
        }
        /// <summary>
        /// Gets the next marker index from the bucket.
        /// </summary>
        /// <param name="wasPriority">True if the index returned is for a priority marker.</param>
        /// <returns>The index of the next marker in the bucket.</returns>
        public int GetItem(out bool wasPriority)
        {
            if (PriorityItems.Count > 0)
            {
                int priorityIndex = PriorityItems[PriorityItems.Count - 1];
                PriorityItems.RemoveAt(PriorityItems.Count - 1);
                wasPriority = true;
                return priorityIndex;
            }

            int index = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            wasPriority = false;
            return index;
        }
        /// <summary>
        /// Boolean indicating whether or not the bucket contains no items.
        /// </summary>
        public bool IsEmpty
        {
            get { return Items.Count == 0 && PriorityItems.Count == 0; }
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