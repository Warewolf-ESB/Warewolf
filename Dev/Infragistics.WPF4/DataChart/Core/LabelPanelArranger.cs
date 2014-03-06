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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    [WidgetIgnoreDepends("NumericAngleAxis")]
    [WidgetIgnoreDepends("NumericRadiusAxis")]
    [WidgetIgnoreDepends("CategoryAngleAxis")]
    [WidgetIgnoreDepends("RadialAxisLabelPanel")]
    [WidgetIgnoreDepends("AngleAxisLabelPanel")]
    internal class LabelPanelArranger
    {
        public static Rect PreparePanels(List<AxisLabelPanelBase> labelPanels, Rect gridAreaRect)
        {
            foreach (AxisLabelPanelBase panel in labelPanels)
            {
                Axis axis = panel.Axis;
                Axis crossingAxis = axis.CrossingAxis;
                crossingAxis = EnsurePolarCrossing(panel, axis, crossingAxis);
                AxisLabelsLocation labelLocation = ResolveLabelLocation(panel);


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                double desiredWidth = panel.DesiredSize.Width;
                double desiredHeight = panel.DesiredSize.Height;


                if (panel is HorizontalAxisLabelPanelBase)
                {
                    switch (labelLocation)
                    {
                        case AxisLabelsLocation.OutsideTop:
                            gridAreaRect.Y = Math.Max(0, gridAreaRect.Top + desiredHeight);
                            gridAreaRect.Height = Math.Max(0, gridAreaRect.Height - desiredHeight);
                            axis.LabelSettings.ActualLocation = AxisLabelsLocation.OutsideTop;
                            break;
                        case AxisLabelsLocation.InsideTop:
                            if (crossingAxis != null &&
                                (panel.CrossingValue - gridAreaRect.Top < desiredHeight &&
                                panel is HorizontalAxisLabelPanel))
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideBottom;
                            }
                            else
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideTop;
                            }
                            break;
                        case AxisLabelsLocation.InsideBottom:
                            if (crossingAxis != null &&
                                (gridAreaRect.Bottom - panel.CrossingValue < desiredHeight &&
                                panel is HorizontalAxisLabelPanel))
                            {
                                if (axis.LabelSettings != null)
                                {
                                    axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideTop;
                                }
                            }
                            else
                            {
                                if (axis.LabelSettings != null)
                                {
                                    axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideBottom;
                                }
                            }
                            break;
                        case AxisLabelsLocation.OutsideBottom:
                        default:
                            if (axis.LabelSettings != null)
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.OutsideBottom;
                            }
                            gridAreaRect.Height = Math.Max(0, gridAreaRect.Height - desiredHeight);
                            break;
                    }
                }
                if (panel is VerticalAxisLabelPanel)
                {
                    switch (labelLocation)
                    {
                        case AxisLabelsLocation.OutsideRight:
                            gridAreaRect.Width = Math.Max(0, gridAreaRect.Width - desiredWidth);
                            axis.LabelSettings.ActualLocation = AxisLabelsLocation.OutsideRight;
                            break;
                        case AxisLabelsLocation.InsideLeft:
                            if (crossingAxis != null && panel.CrossingValue - gridAreaRect.Left < desiredWidth)
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideRight;
                            }
                            else
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideLeft;
                            }
                            break;
                        case AxisLabelsLocation.InsideRight:
                            if (crossingAxis != null && gridAreaRect.Right - panel.CrossingValue < desiredWidth)
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideLeft;
                            }
                            else
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.InsideRight;
                            }
                            break;
                        case AxisLabelsLocation.OutsideLeft:
                        default:
                            //if this falls through and location is set to outside left, we set it to outside left explicitly.
                            if (axis.LabelSettings != null)
                            {
                                axis.LabelSettings.ActualLocation = AxisLabelsLocation.OutsideLeft;
                            }
                            gridAreaRect.X = Math.Max(0, gridAreaRect.Left + desiredWidth);
                            gridAreaRect.Width = Math.Max(0, gridAreaRect.Width - desiredWidth);
                            break;
                    }
                }

                if (panel is AngleAxisLabelPanel)
                {
                    if (axis.LabelSettings != null)
                    {
                        axis.LabelSettings.ActualLocation = labelLocation;
                    }
                }
            }

            return gridAreaRect;
        }

        private static Axis EnsurePolarCrossing(AxisLabelPanelBase panel, Axis axis, Axis crossingAxis)
        {
            if (panel is RadialAxisLabelPanel &&
                    axis is NumericRadiusAxis)
            {
                crossingAxis = (axis as NumericRadiusAxis).AngleAxis;
            }
            if (panel is AngleAxisLabelPanel &&
                axis is NumericAngleAxis)
            {
                crossingAxis = (axis as NumericAngleAxis).RadiusAxis;
            }
            if (panel is AngleAxisLabelPanel &&
                axis is CategoryAngleAxis)
            {
                crossingAxis = (axis as CategoryAngleAxis).RadiusAxis;
            }
            return crossingAxis;
        }

        internal static AxisLabelsLocation ResolveLabelLocation(AxisLabelPanelBase panel)
        {
            if (panel == null)
            {
                return AxisLabelsLocation.OutsideBottom;
            }
            else
            {
                Axis axis = panel.Axis;
                if (axis != null && axis.LabelSettings != null)
                {
                    AxisLabelsLocation location = axis.LabelSettings.ActualLocation;
                    if (!panel.ValidLocation(location))
                    {
                        return panel.GetDefaultLabelsLocation();
                    }
                    return location;
                }
                else
                {
                    return panel.GetDefaultLabelsLocation();
                }
            }
        }

        private static void ArrangeLabelPanel(AxisLabelPanelBase panel, LabelPanelsArrangeState arrangeState, Rect gridAreaRect, Action<AxisLabelPanelBase, Rect> setBounds)
        {
            Axis axis = panel.Axis;
            Axis crossingAxis = axis.CrossingAxis;
            crossingAxis = EnsurePolarCrossing(panel, axis, crossingAxis);
            AxisLabelsLocation labelLocation = ResolveLabelLocation(panel);

            Rect bounds;




            double panelHeight = panel.DesiredSize.Height;
            double panelWidth = panel.DesiredSize.Width;

            switch (labelLocation)
            {
                case AxisLabelsLocation.OutsideRight:
                    arrangeState.Right = arrangeState.Right - panelWidth;
                    arrangeState.InsideRight = arrangeState.InsideRight - panelWidth;
                    bounds = new Rect(arrangeState.Right, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                    setBounds(panel, bounds);
                    break;
                case AxisLabelsLocation.OutsideLeft:
                    bounds = new Rect(arrangeState.Left, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                    setBounds(panel, bounds);
                    arrangeState.Left = arrangeState.Left + panelWidth;
                    arrangeState.InsideLeft = arrangeState.InsideLeft + panelWidth;
                    break;
                case AxisLabelsLocation.InsideRight:
                    if (crossingAxis != null)
                    {
                        bounds = new Rect(arrangeState.Left + panel.CrossingValue, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                    }
                    else
                    {
                        arrangeState.InsideRight = arrangeState.InsideRight - panelWidth;
                        bounds = new Rect(arrangeState.InsideRight, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                    }
                    setBounds(panel, bounds);
                    break;
                case AxisLabelsLocation.InsideLeft:
                    if (crossingAxis != null)
                    {
                        bounds = new Rect(arrangeState.Left + panel.CrossingValue - panelWidth, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                    }
                    else
                    {
                        bounds = new Rect(arrangeState.InsideLeft, gridAreaRect.Top, panelWidth, gridAreaRect.Height);
                        arrangeState.InsideLeft = arrangeState.InsideLeft + panelWidth;
                    }
                    setBounds(panel, bounds);
                    break;
                case AxisLabelsLocation.OutsideBottom:
                    arrangeState.Bottom = arrangeState.Bottom - panelHeight;
                    arrangeState.InsideBottom = arrangeState.InsideBottom - panelHeight;
                    bounds = new Rect(gridAreaRect.Left, arrangeState.Bottom, gridAreaRect.Width, panelHeight);
                    setBounds(panel, bounds);
                    break;
                case AxisLabelsLocation.OutsideTop:
                    bounds = new Rect(gridAreaRect.Left, arrangeState.Top, gridAreaRect.Width, panelHeight);
                    setBounds(panel, bounds);
                    arrangeState.Top = arrangeState.Top + panelHeight;
                    arrangeState.InsideTop = arrangeState.InsideTop + panelHeight;
                    break;
                case AxisLabelsLocation.InsideBottom:
                    if (crossingAxis != null)
                    {
                        bounds = new Rect(gridAreaRect.Left, arrangeState.Top + panel.CrossingValue, gridAreaRect.Width, panelHeight);
                    }
                    else
                    {
                        arrangeState.InsideBottom = arrangeState.InsideBottom - panelHeight;
                        bounds = new Rect(gridAreaRect.Left, arrangeState.InsideBottom, gridAreaRect.Width, panelHeight);
                    }
                    setBounds(panel, bounds);
                    break;
                case AxisLabelsLocation.InsideTop:
                    if (crossingAxis != null)
                    {
                        bounds = new Rect(gridAreaRect.Left, arrangeState.Top + panel.CrossingValue - panelHeight, gridAreaRect.Width, panelHeight);
                    }
                    else
                    {
                        bounds = new Rect(gridAreaRect.Left, arrangeState.InsideTop, gridAreaRect.Width, panelHeight);
                        arrangeState.InsideTop = arrangeState.InsideTop + panelHeight;
                    }
                    setBounds(panel, bounds);
                    break;
            }

            if (panel is AngleAxisLabelPanel)
            {
                bounds = new Rect(gridAreaRect.Left, gridAreaRect.Top, gridAreaRect.Width, gridAreaRect.Height);
                setBounds(panel, bounds);
            }
        }

        internal static void ArrangePanels(List<AxisLabelPanelBase> labelPanels, LabelPanelsArrangeState arrangeState, Rect gridAreaRect, Action<AxisLabelPanelBase, Rect> setBounds)
        {
            // first the 'outside' panels, then the 'inside' panels
            List<AxisLabelPanelBase> insidePanels = new List<AxisLabelPanelBase>();
            List<AxisLabelPanelBase> outsidePanels = new List<AxisLabelPanelBase>();

            foreach (AxisLabelPanelBase panel in labelPanels)
            {
                AxisLabelsLocation labelLocation = ResolveLabelLocation(panel);
                switch (labelLocation)
                {
                    case AxisLabelsLocation.OutsideBottom:
                    case AxisLabelsLocation.OutsideLeft:
                    case AxisLabelsLocation.OutsideTop:
                    case AxisLabelsLocation.OutsideRight:
                        outsidePanels.Add(panel);
                        break;
                    case AxisLabelsLocation.InsideBottom:
                    case AxisLabelsLocation.InsideLeft:
                    case AxisLabelsLocation.InsideTop:
                    case AxisLabelsLocation.InsideRight:
                        insidePanels.Add(panel);
                        break;

                }
            }

            foreach (AxisLabelPanelBase panel in outsidePanels)
            {
                ArrangeLabelPanel(panel, arrangeState, gridAreaRect, setBounds);
            }
            foreach (AxisLabelPanelBase panel in insidePanels)
            {
                ArrangeLabelPanel(panel, arrangeState, gridAreaRect, setBounds);
            }
        }
    }

    internal class LabelPanelsArrangeState
    {
        internal double Bottom { get; set; }
        internal double Top { get; set; }
        internal double InsideBottom { get; set; }
        internal double InsideTop { get; set; }
        internal double Left { get; set; }
        internal double Right { get; set; }
        internal double InsideLeft { get; set; }
        internal double InsideRight { get; set; }
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