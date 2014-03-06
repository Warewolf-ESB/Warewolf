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
    internal class ScatterBaseView
        : MarkerSeriesView
    {
        protected internal MarkerManagerBase MarkerManager { get; set; }
        private Point[] Locations { get; set; }
        protected ScatterBase ScatterModel { get; set; }
        private int[] Indexes { get; set; }
        public ScatterBaseView(ScatterBase model)
            : base(model)
        {
            ScatterModel = model;
            Markers = new HashPool<object, Marker>();
            InitMarkers(Markers);

            TrendLineManager = new ScatterTrendLineManager();
        }
 
        #region HorizontalErrorBarsPath
        private Path HorizontalErrorBarsPath { get; set; }
        #endregion //HorizontalErrorBarsPath

        #region VerticalErrorBarsPath
        private Path VerticalErrorBarsPath { get; set; }
        #endregion //VerticalErrorBarsPath

        internal ScatterTrendLineManager TrendLineManager { get; set; }

        public override void OnInit()
        {
            base.OnInit();

            HorizontalErrorBarsPath = new Path();
            VerticalErrorBarsPath = new Path();

            MarkerManager = CreateMarkerManager();
        }

        protected virtual MarkerManagerBase CreateMarkerManager()
        {
            return new NumericMarkerManager(
             (o) => this.Markers[o],
             (i) => this.ScatterModel.AxisInfoCache.FastItemsSource[i],
             RemoveUnusedMarkers,
             GetMarkerLocations,
             GetActiveIndexes,
             () => this.ScatterModel.MarkerCollisionAvoidance);
        }

        protected virtual void RemoveUnusedMarkers(IDictionary<object, OwnedPoint> list)
        {
            ScatterModel.RemoveUnusedMarkers(list, Markers);
        }
        protected virtual Point[] GetMarkerLocations()
        {
            Locations = ScatterModel.GetMarkerLocations(Markers, Locations, WindowRect, Viewport);
            return Locations;
        }
        protected virtual int[] GetActiveIndexes()
        {
            Indexes = ScatterModel.GetActiveIndexes(Markers, Indexes);
            return Indexes;
        }

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            this.TrendLineManager.AttachPolyLine(rootCanvas, Model);
        }

        internal void AttachHorizontalErrorBars()
        {
            if (ScatterModel.ErrorBarSettings.HorizontalStroke != Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush)
            {
                HorizontalErrorBarsPath.Stroke = ScatterModel.ErrorBarSettings.HorizontalStroke;
            }
            if (ScatterModel.ErrorBarSettings.HorizontalStrokeThickness != (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue)
            {
                HorizontalErrorBarsPath.StrokeThickness = ScatterModel.ErrorBarSettings.HorizontalStrokeThickness;
            }
            if (ScatterModel.ErrorBarSettings.HorizontalErrorBarStyle != null)
            {
                HorizontalErrorBarsPath.SetBinding(Path.StyleProperty, new Binding("HorizontalErrorBarStyle") { Source = ScatterModel.ErrorBarSettings });
            }
            else
            {
                HorizontalErrorBarsPath.SetBinding(Path.StyleProperty, new Binding("DefaultErrorBarStyle") { Source = ScatterModel.ErrorBarSettings });
            }

            if (ScatterModel.ErrorBarSettings.EnableErrorBarsHorizontal == EnableErrorBars.None)
            {
                if (this.RootCanvas.Children.Contains(this.HorizontalErrorBarsPath))
                {
                    this.RootCanvas.Children.Remove(this.HorizontalErrorBarsPath);
                }
                return;
            }

            if (!this.RootCanvas.Children.Contains(this.HorizontalErrorBarsPath))
            {
                this.RootCanvas.Children.Add(this.HorizontalErrorBarsPath);
            }
        }

        internal void ProvideHorizontalErrorBarGeometry(PathGeometry horizontalErrorBarsGeometry)
        {
            HorizontalErrorBarsPath.Data = horizontalErrorBarsGeometry;
        }

        internal void AttachVerticalErrorBars()
        {
            if (ScatterModel.ErrorBarSettings.VerticalStroke != Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush)
            {
                VerticalErrorBarsPath.Stroke = ScatterModel.ErrorBarSettings.VerticalStroke;
            }
            if (ScatterModel.ErrorBarSettings.VerticalStrokeThickness != (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue)
            {
                VerticalErrorBarsPath.StrokeThickness = ScatterModel.ErrorBarSettings.VerticalStrokeThickness;
            }
            if (ScatterModel.ErrorBarSettings.VerticalErrorBarStyle != null)
            {
                VerticalErrorBarsPath.SetBinding(Path.StyleProperty, new Binding("VerticalErrorBarStyle") { Source = ScatterModel.ErrorBarSettings });
            }
            else
            {
                VerticalErrorBarsPath.SetBinding(Path.StyleProperty, new Binding("DefaultErrorBarStyle") { Source = ScatterModel.ErrorBarSettings });
            }

            if (ScatterModel.ErrorBarSettings.EnableErrorBarsVertical == EnableErrorBars.None)
            {
                if (this.RootCanvas.Children.Contains(this.VerticalErrorBarsPath))
                {
                    this.RootCanvas.Children.Remove(this.VerticalErrorBarsPath);
                }
                return;
            }

            if (!this.RootCanvas.Children.Contains(this.VerticalErrorBarsPath))
            {
                this.RootCanvas.Children.Add(this.VerticalErrorBarsPath);
            }
        }

        internal void ProvideVerticalErrorBarGeometry(PathGeometry verticalErrorBarsGeometry)
        {
            VerticalErrorBarsPath.Data = verticalErrorBarsGeometry;
        }

        internal void HideErrorBars()
        {
            HorizontalErrorBarsPath.Data = null;
            VerticalErrorBarsPath.Data = null;
        }

        internal void UpdateTrendlineBrush()
        {
            ScatterModel.ClearValue(ScatterBase.ActualTrendLineBrushProperty);
            if (ScatterModel.TrendLineBrush != null)
            {
                ScatterModel.SetBinding(ScatterBase.ActualTrendLineBrushProperty, new Binding(Series.TrendLineBrushPropertyName) { Source = ScatterModel });
            }
            else
            {
                ScatterModel.SetBinding(ScatterBase.ActualTrendLineBrushProperty, new Binding(Series.ActualBrushPropertyName) { Source = ScatterModel });
            }
        }
        internal HashPool<object, Marker> Markers { get; set; }
        protected internal override void DoToAllMarkers(Action<Marker> action)
        {
            this.Markers.DoToAll(action);
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        protected internal virtual void ClearRendering(bool wipeClean)
        {
            if (wipeClean)
            {
                this.HideErrorBars();
                this.Markers.Clear();
            }
            this.TrendLineManager.ClearPoints();
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