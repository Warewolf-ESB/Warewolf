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
    /// Class containing several properties which are used as parameters passed to scaling operations in a SeriesViewer.
    /// </summary>
    public class ScalerParams
    {
        /// <summary>
        /// ScalerParams constructor.
        /// </summary>
        /// <param name="windowRect">The WindowRect in context.</param>
        /// <param name="viewportRect">The Viewport in context.</param>
        /// <param name="isInverted">True if the axis is inverted, otherwise False.</param>
        public ScalerParams(Rect windowRect, Rect viewportRect, bool isInverted)
        {
            WindowRect = windowRect;
            ViewportRect = viewportRect;
            EffectiveViewportRect = Rect.Empty;
            IsInverted = isInverted;
        }


        /// <summary>
        /// Rect representing the current zoom and pan.
        /// </summary>
        public Rect WindowRect { get; set; } 

        /// <summary>
        /// Rect corresponding roughly to the layout area. The reference frame for scaled coordinates.
        /// </summary>
        public Rect ViewportRect { get; set; }

        /// <summary>
        /// The Rect to which ViewportRect has been coerced for scaling purposes, if such behavior has been defined. Generally a subarea of ViewportRect.
        /// A value of Rect.Empty (by default) implies that the EffectiveViewportRect is the same as the ViewportRect.
        /// </summary>
        public Rect EffectiveViewportRect { get; set; }

        /// <summary>
        /// Bool that determines whether or not to invert the scale.
        /// </summary>
        public bool IsInverted { get; set; }


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

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