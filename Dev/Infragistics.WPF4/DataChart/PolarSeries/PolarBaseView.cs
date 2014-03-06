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
using System.Windows.Data;
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    internal class PolarBaseView
        : MarkerSeriesView
    {
        protected PolarBase PolarModel { get; set; }
        internal MarkerManagerBase MarkerManager { get; set; }

        public PolarBaseView(PolarBase model)
            : base(model)
        {
            PolarModel = model;
            this.Markers = new HashPool<object, Marker>();
            InitMarkers(this.Markers);

            TrendLineManager = new PolarTrendLineManager();
        }

        public override void OnInit()
        {
            base.OnInit();

            MarkerManager = CreateMarkerManager();
        }

        protected virtual MarkerManagerBase CreateMarkerManager()
        {
            var m = new NumericMarkerManager(
             (o) => this.Markers[o],
             (i) => this.PolarModel.AxisInfoCache.FastItemsSource[i],
             RemoveUnusedMarkers,
             GetMarkerLocations,
             GetActiveIndexes);
            m.PopulateColumnValues = true;
            m.GetColumnValues = PolarModel.GetColumnValues;

            return m;
        }

        protected virtual void RemoveUnusedMarkers(IDictionary<object, OwnedPoint> list)
        {
            PolarModel.RemoveUnusedMarkers(list, Markers);
        }
        protected virtual Point[] GetMarkerLocations()
        {
            return PolarModel.GetMarkerLocations(Markers, WindowRect, Viewport);
        }
        protected virtual List<int> GetActiveIndexes()
        {
            return PolarModel.GetActiveIndexes(Markers);
        }

        internal PolarTrendLineManager TrendLineManager { get; set; }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            this.TrendLineManager.AttachPolyLine(rootCanvas, PolarModel);
        }

        internal void ApplyClipping(Rect viewportRect, Rect windowRect)
        {
            if (PolarModel.ClipSeriesToBounds)
            {
                var geom = new GeometryGroup();
                PolarModel.RadiusAxis.DefineClipRegion(geom, viewportRect, windowRect);
                RootCanvas.Clip = geom;
            }
            else
            {
                if (RootCanvas.Clip != null)
                {
                    RootCanvas.ClearValue(
                        UIElement.ClipProperty);
                }
            }
        }

        internal void UpdateTrendlineBrush()
        {
            PolarModel.ClearValue(PolarBase.ActualTrendLineBrushProperty);
            if (PolarModel.TrendLineBrush != null)
            {
                PolarModel.SetBinding(PolarBase.ActualTrendLineBrushProperty, new Binding(Series.TrendLineBrushPropertyName) { Source = PolarModel });
            }
            else
            {
                PolarModel.SetBinding(PolarBase.ActualTrendLineBrushProperty, new Binding(Series.ActualBrushPropertyName) { Source = PolarModel });
            }
        }
        internal HashPool<object, Marker> Markers { get; set; }
        protected internal override void DoToAllMarkers(Action<Marker> action)
        {
            this.Markers.DoToAll(action);
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