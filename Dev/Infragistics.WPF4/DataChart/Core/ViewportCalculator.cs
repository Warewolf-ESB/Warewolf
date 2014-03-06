using System;
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
    /// Represents a basic set of methods and properties to calculate the view port of the chart.
    /// </summary>
    public abstract class ViewportCalculator
    {
        /// <summary>
        /// Calculates the bounds of the viewport.
        /// </summary>
        /// <param name="target">Target element</param>
        /// <param name="xReference">X-reference</param>
        /// <param name="yReference">Y-reference</param>
        /// <returns>Viewport rectangle</returns>
        protected virtual Rect CalculateViewportOverride(
            UIElement target,
            FrameworkElement xReference,
            FrameworkElement yReference)
        {
            if (xReference == null || yReference == null)
            {
                return Rect.Empty;
            }
            Rect ret = new Rect(0, 0, xReference.ActualWidth, yReference.ActualHeight);
            if (ret.Width < 1 || ret.Height < 1 
                || xReference.Parent == null 
                || yReference.Parent == null)
            {
                return Rect.Empty;
            }
            return ret;
        }

        /// <summary>
        /// Returns whether or not there is an alternate reference.
        /// </summary>
        /// <param name="target">Target element</param>
        /// <param name="xReference">X-reference</param>
        /// <param name="yReference">Y-reference</param>
        /// <returns>Whether or not the reference was located</returns>
        public virtual bool LocateAlternateReference(UIElement target,
            ref FrameworkElement xReference,
            ref FrameworkElement yReference)
        {
            return false;
        }

        /// <summary>
        /// Returns the bounds of the viewport.
        /// </summary>
        /// <param name="target">Target element</param>
        /// <param name="xReference">X-reference</param>
        /// <param name="yReference">Y-reference</param>
        /// <returns>Bounds of the viewport</returns>
        public virtual Rect CalculateViewport(UIElement target,
            FrameworkElement xReference,
            FrameworkElement yReference)
        {
            Rect ret = CalculateViewportOverride(target, xReference, yReference);

            while (ret == Rect.Empty &&
                LocateAlternateReference(target, ref xReference, ref yReference))
            {
                ret = CalculateViewportOverride(target, xReference, yReference);
            }

            return ret;
        }
    }

    internal class AxisBasedViewportCalculator
        : ViewportCalculator
    {
        public override bool LocateAlternateReference(UIElement target,
            ref FrameworkElement xReference,
            ref FrameworkElement yReference)
        {
            Axis xAxis = xReference as Axis;
            Axis yAxis = yReference as Axis;

            if (xAxis == null ||
                yAxis == null)
            {
                return false;
            }

            Grid gridPanel = xAxis.Parent as Grid;
            if (gridPanel != null)
            {
                Grid plotArea = gridPanel.Parent as Grid;
                if (plotArea != null)
                {
                    xReference = plotArea;
                    yReference = plotArea;

                    return true;
                }
            }

            return false;
        }
    }

    internal class PolarAxisBasedViewportCalculator
        : AxisBasedViewportCalculator
    {
        protected override Rect CalculateViewportOverride(
            UIElement target,
            FrameworkElement xReference,
            FrameworkElement yReference)
        {
            if (target is Series &&
                xReference is Axis &&
                yReference is Axis)
            {
                var series = target as Series;
                var angleAxis = xReference as Axis;
                var radiusAxis = yReference as Axis;

                //if one of the axes is in another chart,
                //prefer the one in the current chart,
                //if both are in another chart, look for another viewport.
                if (angleAxis.SeriesViewer != series.SeriesViewer ||
                    radiusAxis.SeriesViewer != series.SeriesViewer)
                {
                    Axis targetAxis = null;
                    if (angleAxis.SeriesViewer == series.SeriesViewer)
                    {
                        targetAxis = angleAxis;
                    }
                    if (radiusAxis.SeriesViewer == series.SeriesViewer)
                    {
                        targetAxis = radiusAxis;
                    }
                    if (targetAxis == null)
                    {
                        return Rect.Empty;
                    }

                    return base.CalculateViewportOverride(target, targetAxis, targetAxis);
                }
            }

            return base.CalculateViewportOverride(target, xReference, yReference);
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