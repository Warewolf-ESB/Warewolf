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

namespace Infragistics.Controls.Charts
{
    internal class BubbleSeriesView
        : ScatterBaseView
    {
        protected BubbleSeries BubbleModel { get; set; }
        public BubbleSeriesView(BubbleSeries model)
            : base(model)
        {
            BubbleModel = model;
        }

        public override void OnInit()
        {
            base.OnInit();
        }

        protected override MarkerManagerBase CreateMarkerManager()
        {
            return new BubbleMarkerManager(
               o => this.Markers[o],
               i => this.Model.FastItemsSource[i],
               RemoveUnusedMarkers,
               GetMarkerLocations,
               GetActiveIndexes);
        }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }
        /// <summary>
        /// Gets or sets the list of scaled radius values.
        /// </summary>
        internal List<double> RadiusValues { get; set; }
        /// <summary>
        /// Populates the list of actual marker radii based on the scaling algorithm.
        /// </summary>
        internal void CreateMarkerSizes()
        {
            var bubbleManager = (BubbleMarkerManager)MarkerManager;
            BubbleModel.SizeBubbles(
                bubbleManager.ActualMarkers,
                bubbleManager.ActualRadiusColumn,
                Viewport, this == Model.ThumbnailView);
        }

        /// <summary>
        /// Sets a color on each bubble based on the existing color axis.
        /// </summary>
        internal void SetMarkerColors()
        {
            var bubbleManager = (BubbleMarkerManager)MarkerManager;
            BubbleModel.SetMarkerColors(bubbleManager.ActualMarkers);
        }
        internal void ClearMarkerBrushes()
        {
            var bubbleManager = (BubbleMarkerManager)MarkerManager;
            foreach (var marker in bubbleManager.ActualMarkers)
            {
                DataContext markerContext = marker.Content as DataContext;
                if (markerContext != null)
                {
                    markerContext.ItemBrush = null;
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