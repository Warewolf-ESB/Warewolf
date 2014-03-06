
using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics.CodeAnalysis;

namespace Infragistics
{
    
    /// <summary>
    /// Possible positions for smart label placement with respect to the notional rectangle's
    /// origin
    /// </summary>
    public enum SmartPosition
    {
        /// <summary>
        /// Specifies left top as a valid smart placement position.
        /// </summary>
        LeftTop,

        /// <summary>
        /// Specifies center top as a valid smart placement position.
        /// </summary>
        CenterTop,

        /// <summary>
        /// Specifies right top as a valid smart placement position.
        /// </summary>
        RightTop,

        /// <summary>
        /// Specifies left center as a valid smart placement position.
        /// </summary>
        LeftCenter,

        /// <summary>
        /// Specifies center center as a valid smart placement position.
        /// </summary>
        CenterCenter,

        /// <summary>
        /// Specifies right center as a valid smart placement position.
        /// </summary>
        RightCenter,

        /// <summary>
        /// Specifies left bottom as a valid smart placement position.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// Specifies center bottom as a valid smart placement position.
        /// </summary>
        CenterBottom,

        /// <summary>
        /// Specifies right bottom as a valid smart placement position.
        /// </summary>
        RightBottom
    };

    /// <summary>
    /// Interface for objects placed by a smart placer object.
    /// </summary>
    /// <remarks>
    /// Although there is a clear intent to SmartPosition values, implementing classes
    /// are free to interpret the values as they wish. The only restriction being that
    /// the position parameter be interpreted coherently: the bounds set by calling
    /// SetPosition() should be identical to those returned by calling GetPosition()
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Placeable is a word.")]
    public interface ISmartPlaceable
    {
        /// <summary>
        /// Gets an array of valid positions for the current object. Objects can individually
        /// place restrictions on where they can be placed, although in reality it's more
        /// likely that they'll just return a static array.
        /// 
        /// The order of the valid positions has some significance, in that although the
        /// placer will choose the best overall position, it will stop searching as
        /// soon as it finds an ideal one. The upshot of all of this is that you should
        /// return an array sorted in order of preferred placement.
        /// </summary>
        /// <returns>Array of valid smart positions for the current object.</returns>
        SmartPosition[] GetSmartPositions();

        /// <summary>
        /// Gets the bounds of the current object if it were placed at the specified position.
        /// </summary>
        /// <param name="position">Smart position for bounds.</param>
        /// <returns>Bounds for the current object</returns>
        Rect GetSmartBounds(SmartPosition position);

        /// <summary>
        /// Gets or sets the position of the current object.
        /// </summary>
        /// <remarks>
        /// After the smart position is set, the current object's
        /// bounds are assumed to be that returned from GetSmartBounds() if called with the same position.
        /// </remarks>
        SmartPosition SmartPosition { get; set; }

        /// <summary>
        /// Sets or gets the opacity of the current object.
        /// </summary>
        double Opacity { get; set; }
    }

    /// <summary>
    /// Manages the position for each object in a set of managed SmartPositions in
    /// an attempt to minimise overlap.
    /// </summary>
    /// <remarks>
    /// Collision avoidance uses a greedy algorithm running in O(n^2) with O(n) storage.
    /// Although there are no known algorithms running in less time than this, simulated
    /// annealing may produce slightly better results with comparable execution time.
    /// </remarks>
    public class SmartPlacer
    {
        /// <summary>
        /// Creates a new SmartPlacer with the default configuration.
        /// </summary>
        public SmartPlacer()
        {



            Overlap = 0.3;
            Fade = 2.0;
        }

        /// <summary>
        /// Sets or gets the placement bounds for this smart placer. Setting the
        /// placement bounds has no effect on previously placed objects.
        /// </summary>



        public Rect? Bounds

        {
            get;
            set;
        }

        /// <summary>
        /// Sets or gets the maximum permissible placed overlap expressed as a
        /// percentage [0, 1] of each placeable's area. Setting the
        /// placement overlap has no effect on previously placed objects.
        /// </summary>
        public double Overlap
        {
            get;
            set;
        }

        /// <summary>
        /// Sets or gets the fade exponent for ISmartPlaceables which exceed
        /// the current overlap. Setting the
        /// placement fade has no effect on previously placed objects.
        /// </summary>
        public double Fade
        {
            get;
            set;
        }

        /// <summary>
        /// Place the object with respect to currently placed object.
        /// Calling this method causes the object to be immediately updated, but has no effect on previously
        /// placed objects.
        /// 
        /// Placeables which cannot be placed within the placement bounds or 
        /// without overlapping previously placed objects are hidden
        /// </summary>
        /// <param name="smartPlaceable">Object to place.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Placeable is a word.")]
        public void Place(ISmartPlaceable smartPlaceable)
        {
            if (smartPlaceable == null)
            {
                return;
            }

            double minScore = Double.MaxValue;
            Rect minBounds = Rect.Empty;
            SmartPosition minPosition = SmartPosition.CenterBottom;
            bool hasMinPosition = false;
            
            foreach (SmartPosition position in smartPlaceable.GetSmartPositions())
            {
                Rect bounds = smartPlaceable.GetSmartBounds(position);




                if (Bounds == null || Bounds.Value.Contains(bounds))

                {
                    double score = 0.0;

                    foreach (Rect rect in placed)
                    {
                        score += bounds.IntersectionArea(rect);
                    }

                    if (score == 0.0)
                    {
                        minScore = score;
                        minPosition = position;
                        minBounds = bounds;
                        hasMinPosition = true;

                        break;
                    }

                    if (score < minScore)
                    {
                        minScore = score;
                        minPosition = position;
                        minBounds = bounds;
                        hasMinPosition = true;
                    }
                }
            }

            double overlap = 0.0;
            if (hasMinPosition)
            {
                overlap = minScore / minBounds.GetArea();
            }

            if (!hasMinPosition || overlap > Overlap)
            {
                smartPlaceable.Opacity = 0.0;
            }
            else
            {
                if(minScore>0) {
                    smartPlaceable.Opacity = System.Math.Pow(1.0 - MathUtil.Clamp(0.0, overlap, 1.0), Fade);
                }
                else {
                    smartPlaceable.Opacity = 1.0;
                }

                smartPlaceable.SmartPosition=minPosition;
                placed.Add(minBounds);
            }
        }

        private List<Rect> placed = new List<Rect>();
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