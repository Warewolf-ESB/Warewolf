using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Infragistics.Controls.Charts.Util;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Enum defining methods of collision avoidance for data series markers.
    /// </summary>
    public enum CollisionAvoidanceType 
    { 
        /// <summary>
        /// Collision avoidance is disabled.
        /// </summary>
        None, 
        /// <summary>
        /// Items colliding with other items will be hidden from view.
        /// </summary>
        Omit, 
        /// <summary>
        /// Items colliding with other items will be partially hidden from view by reducing their opacity.
        /// </summary>
        Fade, 
        /// <summary>
        /// Items colliding with other items will be either hidden from view or moved to new positions.
        /// </summary>
        OmitAndShift, 
        /// <summary>
        /// Items colliding with other items will be either partially hidden from view by reducing their opacity, or moved to new positions, or a combination of both.
        /// </summary>
        FadeAndShift 
    };

    /// <summary>
    /// Marker manager class used by data series with numeric dimensions X and Y.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class NumericMarkerManager : MarkerManagerBase
    {
        /// <summary>
        /// NumericMarkerManager constructor.
        /// </summary>
        /// <param name="provideMarkerStrategy">The function used to provide marker objects given a data item.</param>
        /// <param name="provideItemStrategy">The function used to provide a data item given its index.</param>
        /// <param name="removeUnusedMarkers">The action used to remove unused markers.</param>
        /// <param name="getItemLocationsStrategy">The function used to get all marker locations.</param>
        /// <param name="activeMarkerIndexesStrategy">The function used to return the indexes of all active markers.</param>
        public NumericMarkerManager(Func<object, Marker> provideMarkerStrategy,
            Func<int, object> provideItemStrategy,
            Action<IDictionary<object, OwnedPoint>> removeUnusedMarkers,
            Func<Point[]> getItemLocationsStrategy,
            Func<IList<int>> activeMarkerIndexesStrategy)
            : this(provideMarkerStrategy, provideItemStrategy, removeUnusedMarkers, getItemLocationsStrategy, activeMarkerIndexesStrategy, () => { return CollisionAvoidanceType.None; })
        {
        }
        /// <summary>
        /// NumericMarkerManager constructor.
        /// </summary>
        /// <param name="provideMarkerStrategy">The function used to provide marker objects given a data item.</param>
        /// <param name="provideItemStrategy">The function used to provide a data item given its index.</param>
        /// <param name="removeUnusedMarkers">The action used to remove unused markers.</param>
        /// <param name="getItemLocationsStrategy">The function used to get all marker locations.</param>
        /// <param name="activeMarkerIndexesStrategy">The function used to return the indexes of all active markers.</param>
        /// <param name="getCollisionAvoidanceStrategy">The function used to get the method of collision avoidance to be used.</param>
        public NumericMarkerManager(Func<object, Marker> provideMarkerStrategy,
            Func<int, object> provideItemStrategy,
            Action<IDictionary<object, OwnedPoint>> removeUnusedMarkers,
            Func<Point[]> getItemLocationsStrategy,
            Func<IList<int>> activeMarkerIndexesStrategy,
            Func<CollisionAvoidanceType> getCollisionAvoidanceStrategy)
            : base(provideMarkerStrategy, provideItemStrategy, removeUnusedMarkers, getItemLocationsStrategy, activeMarkerIndexesStrategy)
        {
            PopulateColumnValues = false;
            GetColumnValues = (i) => new Point(0,0);
            GetCollisionAvoidanceStrategy = getCollisionAvoidanceStrategy;
        }
        /// <summary>
        /// Boolean indicating whether or not the ColumnValues of OwnedPoints should be populated during marker assignment.
        /// </summary>
        public bool PopulateColumnValues { get; set; }
        /// <summary>
        /// A reference to a function providing column values for a given index.
        /// </summary>
        public Func<int, Point> GetColumnValues { get; set; }
        Func<CollisionAvoidanceType> GetCollisionAvoidanceStrategy { get; set; }



        /// <summary>
        /// Filter out markers that should not be visible.
        /// </summary>
        /// <param name="markers">A dictionary containing the locations of all markers.</param>
        /// <param name="maximumMarkers">The maximum number of markers which can be displayed.</param>
        /// <param name="windowRect">A rectangle representing the scroll window.  Rectangle coordinates are based on the range of zero to one.</param>
        /// <param name="viewportRect">A rectangle representing the viewport, in device coordinates.</param>
        /// <param name="currentResolution">The current Series Resolution.</param>
        public override void WinnowMarkers(IDictionary<object, OwnedPoint> markers, int maximumMarkers,
           Rect windowRect, Rect viewportRect, double currentResolution)
        {
            Point[] itemLocations = GetItemLocationsStrategy();

            //reusedActives = 0;
            markers.Clear();
            
            List<int> visibleItems = new List<int>();
            maximumMarkers = Math.Max(0, maximumMarkers);

            List<int> markerItems = null;
            GetVisibleItems(windowRect, viewportRect, itemLocations, visibleItems);

            if (maximumMarkers >= visibleItems.Count)
            {
                markerItems = visibleItems;
            }
            else
            {
                markerItems = new List<int>();
                double resolution = Math.Max(8.0, currentResolution);

                Dictionary<int, MarkerManagerBucket> buckets = GetBuckets(viewportRect, visibleItems, resolution, itemLocations);
                List<int> keys = new List<int>(buckets.Keys);
                if (UseDeterministicSelection)
                {
                    keys.Sort();
                }
                SelectMarkerItems(maximumMarkers, buckets, keys, markerItems);
            }

            AssignMarkers(markers, itemLocations, markerItems);

            //Debug.WriteLine("Reused Actives: " + reusedActives);
        }

        private void AssignMarkers(IDictionary<object, OwnedPoint> markers, Point[] itemLocations, List<int> markerItems)
        {
            for (int i = 0; i < markerItems.Count; ++i)
            {
                int index = markerItems[i];
                Point point = itemLocations[index];

                object item = ProvideItemStrategy(index);
                Marker marker = ProvideMarkerStrategy(item);
                if (marker.Content != null)
                {
                    (marker.Content as DataContext).Item = item;
                }
                
                OwnedPoint mp = new OwnedPoint();
                if (PopulateColumnValues)
                {
                    mp.ColumnValues = GetColumnValues(index);
                }

                mp.OwnerItem = item;
                mp.Point = new Point(point.X, point.Y);

                if (!markers.ContainsKey(item))
                {
                    markers.Add(item, mp);
                }
            }
        }
        /// <summary>
        /// Renders the given markers.
        /// </summary>
        /// <param name="markers">A dictionary containing the locations of all markers.</param>
        /// <param name="lightweight">Whether or not to enable a lower-fidelity rendering mode with better performance.</param>
        public override void Render(IDictionary<object, OwnedPoint> markers, bool lightweight)
        {
            #region render the markers
            IEnumerable<object> keys = markers.Keys;
            if (UseDeterministicSelection)
            {
                List<object> keysList = new List<object>(markers.Keys);
                keysList.Sort((o1, o2) =>
                {
                    OwnedPoint point1 = markers[o1];
                    OwnedPoint point2 = markers[o2];
                    double dist1 =
                        Math.Pow(point1.Point.X, 2) +
                        Math.Pow(point1.Point.Y, 2);
                    double dist2 =
                        Math.Pow(point2.Point.X, 2) +
                        Math.Pow(point2.Point.Y, 2);
                    return dist1.CompareTo(dist2);
                });
                keys = keysList;
            }


            // initialize smart placement helpers, if necessary
            SmartPlacer smartPlacer = null;
            SmartPlaceableWrapper<Marker> wrapper = null;
            switch (GetCollisionAvoidanceStrategy())
            {
                case CollisionAvoidanceType.None:
                    break;
                case CollisionAvoidanceType.Omit:
                    smartPlacer = new SmartPlacer() { Overlap = 0.3, Fade = 0.0 };
                    wrapper = new SmartPlaceableWrapper<Marker>();
                    wrapper.NoWiggle = true;
                    break;
                case CollisionAvoidanceType.Fade:
                    smartPlacer = new SmartPlacer() { Overlap = 0.6, Fade = 2.0 };
                    wrapper = new SmartPlaceableWrapper<Marker>();
                    wrapper.NoWiggle = true;
                    break;
                case CollisionAvoidanceType.OmitAndShift:
                    smartPlacer = new SmartPlacer() { Overlap = 0.3, Fade = 0.0 };
                    wrapper = new SmartPlaceableWrapper<Marker>();
                    break;
                case CollisionAvoidanceType.FadeAndShift:
                    smartPlacer = new SmartPlacer() { Overlap = 0.6, Fade = 2.0 };
                    wrapper = new SmartPlaceableWrapper<Marker>();
                    break;
            }


            foreach (object key in keys)
            {
                OwnedPoint point = markers[key];
                Marker marker = ProvideMarkerStrategy(point.OwnerItem);

                // [DN Apr 9, 2012 : 108517] resetting IsHitTestVisible which might have been set to False by the other line of code marked 108517
                marker.IsHitTestVisible = true;



                if (smartPlacer != null && wrapper != null) // if helpers initialized, do smart placement
                {
                    // initialize wrapper
                    wrapper.Element = marker;



                    wrapper.OriginalLocation = point.Point;

                    // apply smart placement via wrapper
                    smartPlacer.Place(wrapper);

                    if (wrapper.Opacity == 0.0)
                    {
                        // [DN Apr 6, 2012 : 108355] surgical fix applied during code freeze.  the SmartPosition setter has some code which is essential to update the ElementLocationResult property.  in the case where smartPlacer.Place was not successful, the Opacity is set to 0 and the SmartPosition setter is never accessed.  Therefore, ElementLocationResult will be invalid (possibly an old value).  here, we detect this case clumsily by looking at the Opacity property and if it's zero, we set the SmartPosition property to itself so the setter gets accessed.  TODO: apply this fix in a more elegant way when not in code freeze.
                        wrapper.SmartPosition = wrapper.SmartPosition;

                        // [DN Apr 9, 2012 : 108517] it's still going to interact with the mouse with Opacity=0, so use IsHitTestVisible here.
                        marker.IsHitTestVisible = false;

                    }

                    // save the result
                    point.Point = wrapper.ElementLocationResult;
                }
                else

                {
                    marker.Opacity = 1.0;
                    marker.Visibility = Visibility.Visible;
                }

                UpdateMarkerPosition(marker, point, lightweight);
            }

            RemoveUnusedMarkers(markers);
            #endregion
        }

        private void UpdateMarkerPosition(Marker marker, OwnedPoint point, bool lightweight)
        {




            if (!lightweight)
            {
                marker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            TranslateTransform current = marker.RenderTransform as TranslateTransform;

            double xOffset = point.Point.X;
            double yOffset = point.Point.Y;

            if (!lightweight)
            {
                xOffset -= 0.5 * marker.DesiredSize.Width;
                yOffset -= 0.5 * marker.DesiredSize.Height;
            }

            if (current != null && current.X == xOffset && current.Y == yOffset)
            {
                return;
            }
            //else if (current != null)
            //{
            //    current.X = xOffset;
            //    current.Y = yOffset;
            //    continue;
            //}

            marker.RenderTransform = new TranslateTransform()
            {
                X = xOffset,
                Y = yOffset
            };

        }

    }

    internal static class CategoryMarkerManager
    {
        internal static void RasterizeMarkers(MarkerSeries series, List<Point> markerLocations, Pool<Marker> markers, bool lightweight)
        {
            bool hasMarkers = series.ShouldDisplayMarkers();
            if (markers == null)
            {
                return;
            }

            if (hasMarkers)
            {
                for (int i = 0; i < markerLocations.Count; ++i)
                {
                    PositionMarker(markers, i, markerLocations, lightweight);
                }
            }
        }

        private static void PositionMarker(Pool<Marker> markers, int i, List<Point> markerLocations, bool lightweight)
        {




            double xOffset = 0;
            double yOffset = 0;

            xOffset = markerLocations[i].X;
            yOffset = markerLocations[i].Y;

            if (!lightweight)
            {
                markers[i].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                xOffset -= 0.5 * markers[i].DesiredSize.Width;
                yOffset -= 0.5 * markers[i].DesiredSize.Height;
            }

            markers[i].RenderTransform = new TranslateTransform()
            {
                X = xOffset,
                Y = yOffset
            };

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