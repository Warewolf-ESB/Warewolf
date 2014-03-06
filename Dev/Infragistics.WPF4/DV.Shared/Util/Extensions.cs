using System;



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
using System.Collections.Generic;

namespace Infragistics
{
    /// <summary>
    /// Class containing extension methods used by Infragistics Data Visualization.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Resets the given PathGeometry.
        /// </summary>
        /// <param name="geom">The PathGeometry to reset.</param>
        public static void Reset(this PathGeometry geom)
        {
            
            //isn't there a better way to do this?
            geom.Figures.Clear();
            geom.Figures.Add(new PathFigure());
            geom.Figures.RemoveAt(0);
        }
        /// <summary>
        /// Resets the given GeometryGroup.
        /// </summary>
        /// <param name="geom">The GeometryGroup to reset.</param>
        public static void Reset(this GeometryGroup geom)
        {
            
            //isn't there a better way to do this?
            geom.Children.Clear();
            geom.Children.Add(new PathGeometry());
            geom.Children.RemoveAt(0);
        }
        /// <summary>
        /// Detaches the given FrameworkElement from the visual tree.
        /// </summary>
        /// <param name="child">The FrameworkElement to detach from the visual tree.</param>
        public static void Detach(this FrameworkElement child)
        {
            if (child == null)
            {
                return;
            }
            Panel parent = child.Parent as Panel;
            if (parent == null)
            {
                return;
            }
            parent.Children.Remove(child);
        }
        /// <summary>
        /// Transfers all of a Panel's Children to another Panel.
        /// </summary>
        /// <param name="from">The Panel to transfer Children from.</param>
        /// <param name="to">The Panel to transfer Children to.</param>
        public static void TransferChildrenTo(this Panel from, Panel to)
        {
            List<UIElement> transfer = new List<UIElement>();
            foreach (var child in from.Children.OfType<UIElement>())
            {
                transfer.Add(child);
            }
            foreach (var child in transfer)
            {
                from.Children.Remove(child);
                to.Children.Add(child);
            }
        }


        /// <summary>
        /// Determines if a numeric value is within the range of plottable values.
        /// </summary>
        /// <param name="num">The number under observation.</param>
        /// <returns>True if the given number is plottable, otherwise False.</returns>
        public static bool IsPlottable(this double num)
        {
            return !double.IsNaN(num) &&
                !double.IsInfinity(num);
        }

        /// <summary>
        /// Determines if a Point is within the range of plottable points.
        /// </summary>
        /// <param name="point">The Point under observation.</param>
        /// <returns>True if the given point is plottable, otherwise False.</returns>
        public static bool IsPlottable(this Point point)
        {






            return point.X.IsPlottable() &&
                point.Y.IsPlottable();

        }
        /// <summary>
        /// Determines if a Rect is within plottable range.
        /// </summary>
        /// <param name="rect">The Rect under observation.</param>
        /// <returns>True if the Rect is plottable, otherwise False.</returns>
        public static bool IsPlottable(this Rect rect)
        {
            return !double.IsNaN(rect.Left) &&
                !double.IsNaN(rect.Right) &&
                !double.IsNaN(rect.Top) &&
                !double.IsNaN(rect.Bottom) &&
                !double.IsInfinity(rect.Left) &&
                !double.IsInfinity(rect.Right) &&
                !double.IsInfinity(rect.Top) &&
                !double.IsInfinity(rect.Bottom);
        }


       

        public static IEnumerable<T> LogicalDescendantsOfType<T>(this FrameworkElement ele)
            where T : FrameworkElement
        {
            foreach (var child in LogicalTreeHelper.GetChildren(ele).OfType<FrameworkElement>())
            {
                if (child is T)
                {
                    yield return (T)child;
                }
                foreach (var subChild in LogicalDescendantsOfType<T>(child))
                {
                    yield return (T)subChild;
                }

            }
        }

        /// <summary>
        /// Gets all the visual descendants of a certain Type under the given parent FrameworkElement.
        /// </summary>
        /// <typeparam name="T">The Type of visual descendants to search for.</typeparam>
        /// <param name="ele">The parent FrameworkElement to search under.</param>
        /// <returns>An enumerable list of all visual descendants of the given type under the given parent FrameworkElement.</returns>
        public static IEnumerable<T> VisualDescendantsOfType<T>(this FrameworkElement ele)
            where T : FrameworkElement
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(ele); i++)
            {
                var child = VisualTreeHelper.GetChild(ele, i) as FrameworkElement;
                if (child == null)
                {
                    continue;
                }
                if (child is T)
                {
                    yield return (T)child;
                }
                foreach (var subChild in VisualDescendantsOfType<T>(child))
                {
                    yield return (T)subChild;
                }

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