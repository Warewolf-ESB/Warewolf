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

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// This class is responsible for creating and managing the markers of a bubble series.
    /// </summary>
    internal class BubbleMarkerManager : MarkerManagerBase
    {
        /// <summary>
        /// Gets or sets which column from the datasource is used to map radius values.
        /// </summary>
        internal IFastItemColumn<double> RadiusColumn{ get; set;}

        /// <summary>
        /// Gets or sets the list of scaled radius values.
        /// </summary>
        internal List<double> ActualRadiusColumn { get; set; }

        /// <summary>
        /// Gets or sets the list of displayed markers.
        /// </summary>
        internal List<Marker> ActualMarkers { get; set; }

        /// <summary>
        /// creates a new instance of the BubbleMarkerManager.
        /// </summary>
        public BubbleMarkerManager(Func<object, Marker> provideMarkerStrategy,
            Func<int, object> provideItemStrategy,
            Action<IDictionary<object, OwnedPoint>> removeUnusedMarkers,
            Func<Point[]> getItemLocationsStrategy,
            Func<IList<int>> activeMarkerIndexesStrategy
            )
            : base(provideMarkerStrategy, provideItemStrategy, removeUnusedMarkers, getItemLocationsStrategy, activeMarkerIndexesStrategy)
        {
            ActualRadiusColumn = new List<double>();
            ActualMarkers = new List<Marker>();
        }

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
            ActualRadiusColumn.Clear();
            ActualMarkers.Clear();

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

            for (int i = 0; i < markerItems.Count; ++i)
            {
                double x = itemLocations[markerItems[i]].X;
                double y = itemLocations[markerItems[i]].Y;
                double radius = RadiusColumn[markerItems[i]];
                
                ActualRadiusColumn.Add(radius);

                Marker marker = ProvideMarkerStrategy(ProvideItemStrategy(markerItems[i]));
                (marker.Content as DataContext).Item = ProvideItemStrategy(markerItems[i]);
                OwnedPoint mp = new OwnedPoint();

                mp.OwnerItem = (marker.Content as DataContext).Item;
                mp.Point = new Point(x, y);

                if (!markers.ContainsKey(mp.OwnerItem))
                {
                    markers.Add(mp.OwnerItem, mp);
                    ActualMarkers.Add(marker);
                }
            }

            //Debug.WriteLine("Reused Actives: " + reusedActives);
        }

        /// <summary>
        /// Renders the given markers.
        /// </summary>
        /// <param name="markers">A dictionary containing the locations of all markers.</param>
        /// <param name="lightweight">Whether or not to enable a lower-fidelity rendering mode with better performance.</param>
        public override void Render(IDictionary<object, OwnedPoint> markers, bool lightweight)
        {
            #region render the markers
            List<object> keys = new List<object>(markers.Keys);
            if (UseDeterministicSelection)
            {
                keys.Sort((o1, o2) =>
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
            }

            foreach (object key in keys)
            {
                OwnedPoint point = markers[key];
                Marker marker = ProvideMarkerStrategy(point.OwnerItem);





                Canvas.SetZIndex(marker, keys.IndexOf(key));
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
                    continue;
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

            RemoveUnusedMarkers(markers);
            #endregion
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