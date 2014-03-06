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

namespace Infragistics.Controls
{

    /// <summary>
    /// Interface for controls which can host the OverviewPlusDetailPane.
    /// </summary>
    public interface IOverviewPlusDetailControl
    {
        /// <summary>
        /// Zoom the IOverviewPlusDetailControl to 100%.
        /// </summary>
        /// <remarks>
        /// This method will be called when the OverviewPlusDetailPane's "Zoom to 100%" action is triggered, typically when the "Zoom to 100%" button is clicked.  When this method is called, the IOverviewPlusDetailControl should zoom to a state which the user would recognize as "100% zoom."
        /// </remarks>
        void ZoomTo100();


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Scale the IOverviewPlusDetailControl to fit all its contents in view.
        /// </summary>
        /// <remarks>
        /// This method will be called when the OverviewPlusDetailPane's "Scale to Fit" action is triggered, typically when the "Scale to fit" button is clicked.  When this method is called, the IOverviewPlusDetailControl should zoom so that all its contents are in view.
        /// </remarks>
        void ScaleToFit();
        /// <summary>
        /// Render preview content to the OverviewPlusDetailPane.
        /// </summary>
        void RenderPreview();
        /// <summary>
        /// A rectangle representing the full bounds of the contents of the IOverviewPlusDetailControl.
        /// </summary>
        /// <remarks>
        /// WorldRect can be made of abstract coordinates, not related to screen space.
        /// </remarks>
        Rect WorldRect { get; }
        /// <summary>
        /// A rectangle representing the bounds of the contents of the IOverviewPlusDetailControl.
        /// </summary>
        Rect ViewportRect { get; }
        /// <summary>
        /// The minimum zoom value to be allowed by the OverviewPlusDetailPane.
        /// </summary>
        double MinimumZoomLevel { get; }
        /// <summary>
        /// The maximum zoom value to be allowed by the OverviewPlusDetailPane.
        /// </summary>
        double MaximumZoomLevel { get; }
        /// <summary>
        /// The text to display to represent the zoom level on the OverviewPlusDetailPane.
        /// </summary>
        string ZoomLevelDisplayText { get; }

        /// <summary>
        /// Gets or sets the default interaction state.
        /// </summary>
        /// <value>The default interaction.</value>
        InteractionState DefaultInteraction { get; set; }
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