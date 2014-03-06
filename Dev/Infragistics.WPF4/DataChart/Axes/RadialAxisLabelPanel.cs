using System;
using System.Collections.Generic;



using System.Linq;

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
    /// Represents a panel control, containing radial axis labels.
    /// </summary>
    public class RadialAxisLabelPanel
        : HorizontalAxisLabelPanelBase
    {
        internal override AxisLabelPanelBaseView CreateView()
        {
            return new RadialAxisLabelPanelView(this);
        }
        internal override void OnViewCreated(AxisLabelPanelBaseView view)
        {
            base.OnViewCreated(view);

            RadialView = (RadialAxisLabelPanelView)view;
        }
        internal RadialAxisLabelPanelView RadialView { get; set; }

        /// <summary>
        /// Sets or gets the rotation center of the panel.
        /// </summary>
        public Point RotationCenter { get; set; }

        /// <summary>
        /// Sets or gets the crossing angle of the panel.
        /// </summary>
        public double CrossingAngle { get; set; }

        /// <summary>
        /// Gets if the axis is external to the plot area.
        /// </summary>
        protected bool AxisIsNotEmbedded
        {
            get
            {
                return Axis.LabelSettings != null && (Axis.LabelSettings.ActualLocation != AxisLabelsLocation.InsideBottom && Axis.LabelSettings.ActualLocation != AxisLabelsLocation.InsideTop);
            }
        }

        /// <summary>
        /// Gets if the axis is embedded in the plot area.
        /// </summary>
        protected bool AxisIsEmbedded
        {
            get { return !AxisIsNotEmbedded; }
        }

        /// <summary>
        /// Returns if staggering should be attempted.
        /// </summary>
        /// <returns>True if staggering should be attempted.</returns>
        protected internal override bool ShouldTryStagger()
        {
            return FoundCollisions && (!UseRotation || GetEffectiveAngle() == -180.0);
        }

        /// <summary>
        /// Returns if vertical alignment should be performed.
        /// </summary>
        /// <returns>True if vertical alignment should be performed.</returns>
        protected override bool ShouldVerticalAlign()
        {
            return !UseStaggering;
        }

        /// <summary>
        /// Applies the rotation to the panel.
        /// </summary>
        /// <param name="finalSize">The final size of the plot area.</param>
        protected internal override void ApplyPanelRotation(Size finalSize)
        {
            base.ApplyPanelRotation(finalSize);

            if (CrossingAngle % 360 == 0 ||
                AxisIsNotEmbedded)
            {
                RadialView.ClearPanelRotation();
            }
            else
            {
                RadialView.ApplyPanelRotation(finalSize);
            }
        }

        /// <summary>
        /// Gets the effective angle for the panel.
        /// </summary>
        /// <returns>The angle.</returns>
        protected internal override double GetEffectiveAngle()
        {
            double angle = base.GetEffectiveAngle();
            if (AxisIsEmbedded)
            {
                angle -= (CrossingAngle * 180.0) / Math.PI;
            }
            return angle;
        }

        /// <summary>
        /// Gets if the panel should rotate.
        /// </summary>
        /// <returns>True if the panel should rotate.</returns>
        protected internal override bool ShouldRotate()
        {
            return GetEffectiveAngle() % 360 != 0;
        }

        /// <summary>
        /// Returns if the panel should clip labels.
        /// </summary>
        /// <returns>True if labels should be clipped.</returns>
        protected override bool ShouldClip()
        {
            if (AxisIsNotEmbedded)
            {
                return true;
            }

            double angle = CrossingAngle * 180.0 / Math.PI;
            if (angle < 30 || angle > 330 ||
                (angle > 150 && angle < 210))
            {
                return true;
            }
            return false;
        }

        private List<int> _toHide = new List<int>();

        /// <summary>
        /// Sets the rotation on a label.
        /// </summary>
        /// <param name="label">The label to rotate.</param>
        /// <param name="angle">The angle to rotate the label.</param>
        protected internal override void SetLabelRotationTransform(
            FrameworkElement label,
            double angle)
        {
            double angleRadians = angle*Math.PI/180.0;
            double yFactor = Math.Abs(Math.Sin(angleRadians));
            if (Axis.LabelSettings != null && Axis.LabelSettings.ActualLocation == AxisLabelsLocation.InsideTop)
            {
                yFactor = yFactor * -1.0;
            }

            RadialView.SetLabelRotationalTransform(label, angle, yFactor);
        }

        /// <summary>
        /// Returns teh minimum label position
        /// </summary>
        /// <returns>minimum</returns>
        protected double MinimumPosition()
        {
            double min = double.MaxValue;
            foreach (var pos in LabelPositions)
            {
                min = Math.Min(pos.Value, min);
            }
            return min;
        }

        /// <summary>
        /// Returns the maximum label position.
        /// </summary>
        /// <returns>maximum</returns>
        protected double MaximumPosition()
        {
            double max = double.MinValue;
            foreach (var pos in LabelPositions)
            {
                max = Math.Max(pos.Value, max);
            }
            return max;
        }

        /// <summary>
        /// Hides any optional labels if collisions are occuring.
        /// </summary>
        /// <param name="rectangles">The label bounds list.</param>
        protected override void HideOptionalLabels(List<Rect> rectangles)
        {
            double val;
            if (Axis.IsInverted)
            {
                val = MinimumPosition();
            }
            else
            {
                val = MaximumPosition();
            }
            var toHide = from pos in LabelPositions
                         where pos.Value == val
                         select LabelPositions.IndexOf(pos);

            _toHide = toHide.ToList();

            FoundCollisions = DetectCollisions(
                rectangles.Where((rect, index) => !_toHide.Contains(index)).ToList());
        }

        /// <summary>
        /// Determins if something should be displayed via its index and bounds.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        protected internal override bool ShouldDisplay(int index, Rect bounds)
        {
            if (_toHide.Contains(index))
            {
                return false;
            }
            else
            {
                return base.ShouldDisplay(index, bounds);
            }
        }

        /// <summary>
        /// Shows any optional labels.
        /// </summary>
        protected override void ShowOptionalLabels()
        {
            _toHide = new List<int>();

            base.ShowOptionalLabels();
        }
        internal override AxisLabelsLocation GetDefaultLabelsLocation()
        {
            return AxisLabelsLocation.InsideBottom;
        }

        internal override bool ValidLocation(AxisLabelsLocation location)
        {
            return location == AxisLabelsLocation.InsideBottom ||
                location == AxisLabelsLocation.InsideTop ||
                location == AxisLabelsLocation.OutsideBottom ||
                location == AxisLabelsLocation.OutsideTop;
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