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

namespace Infragistics
{
    
    /// <summary>
    /// ISmartPlaceable wrapper class for a FrameworkElement.
    /// For use with the SmartPlacer class.
    /// </summary>
    public class SmartPlaceableWrapper<T> : ISmartPlaceable where T : FrameworkElement
    {
        /// <summary>
        /// SmartPlaceableWrapper constructor.
        /// </summary>
        public SmartPlaceableWrapper()
        {
            NoWiggle = false;
        }

        #region NoWiggle
        /// <summary>
        /// If true, no attempt will be made to shift the position of the smart placeable element.
        /// </summary>
        public bool NoWiggle { get; set; }
        #endregion

        #region Element
        private T _element;
        /// <summary>
        /// Gets or sets the associated FrameworkElement.
        /// </summary>
        public T Element
        {
            get
            {
                return _element;
            }

            set
            {
                _element = value;
            }
        }
        #endregion

        #region ElementLocationResult
        /// <summary>
        /// The resulting location for Element after smart placement.
        /// </summary>
        public Point ElementLocationResult
        {
            get;
            protected set;
        }
        #endregion

        #region OriginalLocation
        /// <summary>
        /// Gets or sets the original location for Element.
        /// </summary>
        public Point OriginalLocation { get; set; }
        #endregion

        #region ISmartPlaceable implementation
        /// <summary>
        /// Gets the array of SmartPositions for this SmartPlaceableWrapper.
        /// </summary>
        /// <returns>The array of SmartPositions for this SmartPlaceableWrapper.</returns>
        public virtual SmartPosition[] GetSmartPositions()
        {
            if (NoWiggle)
            {
                return smartPositionDefault;
            }
            else
            {
                return smartPositions;
            }
        }

        /// <summary>
        /// Dummy array consisting of the default SmartPositions.
        /// </summary>
        private static SmartPosition[] smartPositionDefault = { SmartPosition.CenterCenter };

        /// <summary>
        /// SmartPositions in order of preference.
        /// </summary>
        private static SmartPosition[] smartPositions =
        {
            SmartPosition.CenterCenter,
            SmartPosition.RightCenter,
            SmartPosition.RightTop,
            SmartPosition.CenterTop,
            SmartPosition.RightBottom,
            SmartPosition.CenterBottom,
            SmartPosition.LeftTop,
            SmartPosition.LeftCenter,
            SmartPosition.LeftBottom
        };



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal Size GetSmartElementSize()
        {
            Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double height = Element.DesiredSize.Height;
            double width = Element.DesiredSize.Width;

            // in some cases Actual Height & Width are already set, and are preferable
            if (Element.ActualHeight > 0 && Element.ActualWidth > 0)
            {
                height = Element.ActualHeight;
                width = Element.ActualWidth;
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Gets the smart placement bounds for the SmartPlaceableWrapper using the specified SmartPosition.
        /// </summary>
        /// <param name="position">A SmartPosition value indicating how to place the SmartPlaceableWrapper.</param>
        /// <returns>A Rect representing the calculated bounds for the SmartPlaceableWrapper at the given SmartPosition.</returns>
        public Rect GetSmartBounds(SmartPosition position)
        {
            Size s = GetSmartElementSize();
            double w = s.Width;
            double h = s.Height;
            Point d;


            if (Element.RenderTransform != null && Element.RenderTransform is ScaleTransform)
            {
                var tr = (ScaleTransform)Element.RenderTransform;

                w *= tr.ScaleX;
                h *= tr.ScaleY;

                d = GetOffset(position, w, h);

                d.X += (w - s.Width) / 2.0;
                d.Y += (h - s.Height) / 2.0;
            }
            else

            {
                d = GetOffset(position, w, h);
            }

            return new Rect(OriginalLocation.X + d.X, OriginalLocation.Y + d.Y, w, h);
        }
        /// <summary>
        /// The Opacity of the underlying element.
        /// </summary>
        public double Opacity
        {
            get { return Element.Opacity; }
            set
            {
                Element.Opacity = value;
            }
        }
        /// <summary>
        /// The SmartPosition being used for placement of the SmartPlaceableWrapper.
        /// </summary>
        public SmartPosition SmartPosition
        {
            get { return smartPosition; }
            set
            {
                smartPosition = value;
                Size s = GetSmartElementSize();
                double h = s.Height;
                double w = s.Width;
                Point d;


                if (Element.RenderTransform != null && Element.RenderTransform is ScaleTransform)
                {
                    var tr = (ScaleTransform)Element.RenderTransform;

                    var wt = w * tr.ScaleX;
                    var ht = h * tr.ScaleY;

                    d = GetOffset(smartPosition, wt, ht);

                    d.X += (wt - s.Width) / 2.0;
                    d.Y += (ht - s.Height) / 2.0;
                }
                else

                {
                    d = GetOffset(smartPosition, w, h);
                }

                this.ElementLocationResult = new Point(OriginalLocation.X + d.X, OriginalLocation.Y + d.Y);
            }
        }
        private SmartPosition smartPosition;

        /// <summary>
        /// Returns the offset (from OriginalLocation) representing a particular SmartPosition.
        /// </summary>
        /// <param name="position">A SmartPosition.</param>
        /// <param name="w">The width of the element being positioned.</param>
        /// <param name="h">The height of the element being positioned.</param>
        /// <returns>A Point representing the offset from OriginalLocation.</returns>
        private Point GetOffset(SmartPosition position, double w, double h)
        {
            const double c = 0.25; // try locations with vertical/horizontal distance from origin equal to c * height/width
            switch (position)
            {
                case SmartPosition.LeftTop: return new Point(-w * c, -h * c);
                case SmartPosition.CenterTop: return new Point(0, -h * c);
                case SmartPosition.RightTop: return new Point(w * c, -h * c);
                case SmartPosition.LeftCenter: return new Point(-w * c, 0);
                case SmartPosition.CenterCenter: return new Point(0, 0);
                case SmartPosition.RightCenter: return new Point(w * c, 0);
                case SmartPosition.LeftBottom: return new Point(-w * c, h * c);
                case SmartPosition.CenterBottom: return new Point(0, h * c);
                default: return new Point(w * c, h * c); // SmartPosition.RightBottom
            }
        }

        #endregion
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