using System;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the panel control, containing angle axis labels.
    /// </summary>
    [WidgetIgnoreDepends("NumericAngleAxis")]
    [WidgetIgnoreDepends("CategoryAngleAxis")]
    public class AngleAxisLabelPanel
        : AxisLabelPanelBase
    {
        internal override AxisLabelPanelBaseView CreateView()
        {
            return new AngleAxisLabelPanelView(this);
        }
        internal override void OnViewCreated(AxisLabelPanelBaseView view)
        {
            base.OnViewCreated(view);

            AngleView = (AngleAxisLabelPanelView)view;
        }
        internal AngleAxisLabelPanelView AngleView { get; set; }

        /// <summary>
        /// Initializes the AngleAxis label panel.
        /// </summary>
        public AngleAxisLabelPanel()
        {
            
        }

        /// <summary>
        /// Gets or sets whether labels should be clipped to bounds.
        /// </summary>
        public bool ClipLabelsToBounds { get; set; }

        /// <summary>
        /// Gets or sets the function used to calculate point locations.
        /// </summary>
        public Func<double, Point> GetPoint { get; set; }

        /// <summary>
        /// Determines if the label should be displayed.
        /// </summary>
        /// <param name="index">label index</param>
        /// <param name="bounds">label bounds</param>
        /// <returns>True if the label should be displayed; otherwise false</returns>
        protected internal override bool ShouldDisplay(int index, Rect bounds)
        {
            if (!bounds.IsPlottable())
            {
                return false;
            }

            if (this.Axis == null 
                || this.Axis.ViewportRect.IsEmpty)
            {
                return base.ShouldDisplay(index, bounds);
            }

            return true;
        }

        internal double LargestWidth { get; set; }
        internal double LargestHeight { get; set; }

        /// <summary>
        /// Creates a list of label bounds.
        /// </summary>
        /// <returns>List of label rectangles</returns>
        protected internal override List<Rect> CreateBoundsRectangles()
        {
            List<Rect> rectangles = new List<Rect>();

            //find the widest label and create the initial list of label bounds.

            if (TextBlocks.Count != LabelPositions.Count)
            {
                return rectangles;
            }

            AngleView.DetermineLargestLabels(rectangles);

            bool extentChanged = false;
            FoundCollisions = DetectCollisions(rectangles);

            //The extent hasn't been set by the user, so we auto-calculate it.
            if (!this.Axis.HasUserExtent())
            {
                extentChanged = true;
                //the simple case: labels fit without collisions. Use the textblock height as the extent.
                this.Extent = LargestWidth / 2.0;

                this.Extent = this.Extent + OtherExtentValues();
            }
            else
            {
                extentChanged = true;
                AngleView.BindExtentToSettings();
            }

            if (extentChanged)
            {
                for (int i = 0; i < rectangles.Count; i++)
                {
                    FrameworkElement currentTextBlock = TextBlocks[i];
                    LabelPosition position = LabelPositions[i];
                    Rect currentRect = rectangles[i];

                    Point point = GetPoint(position.Value);

                    double x = point.X - GetDesiredWidth(currentTextBlock) / 2.0;
                    double y = point.Y - GetDesiredHeight(currentTextBlock) / 2.0;

                    currentRect.X = x;
                    currentRect.Y = y;

                    rectangles[i] = currentRect;
                }
            }

            
            FoundCollisions = DetectCollisions(rectangles);

            if (FoundCollisions)
            {

            }
           
            return rectangles;
        }

        //private IEnumerable<FrameworkElement> Ancestors(FrameworkElement obj)
        //{
        //    FrameworkElement curr = obj;
        //    while (curr != null)
        //    {
        //        FrameworkElement next = curr.Parent as FrameworkElement;
        //        if (next == null)
        //        {
        //            curr = VisualTreeHelper.GetParent(curr) as FrameworkElement;
        //        }
        //        else
        //        {
        //            curr = next;
        //        }
        //        if (curr != null)
        //        {
        //            yield return curr;
        //        }
        //    }
        //}

        private double OtherExtentValues()
        {
            XamDataChart owningChart;
            Axis axis;

            axis = Axis;
            owningChart = null;
            if (axis != null)
            {
                owningChart = axis.SeriesViewer as XamDataChart; // Ancestors(this).OfType<XamDataChart>().FirstOrDefault();
            }

            if (owningChart == null ||
                axis == null)
            {
                return 0;
            }

            var angleAxes = (from a in owningChart.Axes
                where (a is NumericAngleAxis || a is CategoryAngleAxis) &&
                a.LabelPanel is AngleAxisLabelPanel &&
                (a.LabelPanel as AngleAxisLabelPanel).TextBlocks.Count > 0 &&
                (!a.HasUserExtent()) &&
                !a.HasCrossingValue() &&
                (a.CrossingAxis == null || Axis.CrossingAxis == null ||
                (a.CrossingAxis as NumericRadiusAxis).ActualRadiusExtentScale ==
                (axis.CrossingAxis as NumericRadiusAxis).ActualRadiusExtentScale)
                select a)
                .ToList();

            int index = angleAxes.IndexOf(axis);

            if (index == -1)
            {
                return 0;
            }

            double extent = 0;
            for (int i = 0; i < index; i++)
            {
                extent += angleAxes[i].LabelPanel.Extent * 2.0 + 5;
            }

            return extent;
        }
        internal override AxisLabelsLocation GetDefaultLabelsLocation()
        {
            return AxisLabelsLocation.InsideTop;
        }

        internal override bool ValidLocation(AxisLabelsLocation location)
        {
            return location == AxisLabelsLocation.InsideTop ||
                location == AxisLabelsLocation.InsideBottom;
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