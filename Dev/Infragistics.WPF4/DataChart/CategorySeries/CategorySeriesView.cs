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

namespace Infragistics.Controls.Charts
{
    internal class CategorySeriesView
        : MarkerSeriesView, ISupportsMarkers
    {
        internal CategorySeries CategoryModel { get; set; }
        public CategorySeriesView(CategorySeries model)
            : base(model)
        {
            CategoryModel = (CategorySeries)model;

            this.BucketCalculator = this.CreateBucketCalculator();

            Markers =
               new Pool<Marker>()
               {
                   Create = MarkerCreate,
                   Activate = MarkerActivate,
                   Disactivate = MarkerDisactivate,
                   Destroy = MarkerDestroy
               };
        }

        public override void OnInit()
        {
            base.OnInit();

            ErrorBarsPath = new Path();
            Canvas.SetZIndex(ErrorBarsPath, 1000);
        }

        private Path ErrorBarsPath { get; set; }

        protected internal override void OnTemplateProvided()
        {
            base.OnTemplateProvided();
        }

        internal void HideErrorBars()
        {
            if (MarkerCanvas != null)
            {
                if (MarkerCanvas.Children.Contains(this.ErrorBarsPath))
                {
                    MarkerCanvas.Children.Remove(this.ErrorBarsPath);
                }
            }
        }

        internal void ShowErrorBars()
        {
            var series = Model.GetSeriesComponentsForView().Series as CategorySeries;

            if (series.ErrorBarSettings.Stroke != Path.StrokeProperty.GetMetadata(typeof(Path)).DefaultValue as Brush)
            {
                this.ErrorBarsPath.Stroke = series.ErrorBarSettings.Stroke;
            }
            if (series.ErrorBarSettings.StrokeThickness != (double)Path.StrokeThicknessProperty.GetMetadata(typeof(Path)).DefaultValue)
            {
                this.ErrorBarsPath.StrokeThickness = series.ErrorBarSettings.StrokeThickness;
            }
            if (series.ErrorBarSettings.ErrorBarStyle != null)
            {
                this.ErrorBarsPath.SetBinding(
                    Path.StyleProperty,
                    new Binding("ErrorBarStyle") { Source = series.ErrorBarSettings });
            }
            else
            {
                this.ErrorBarsPath.SetBinding(
                    Path.StyleProperty,
                    new Binding("DefaultErrorBarStyle") { Source = series.ErrorBarSettings });
            }

            if (!this.MarkerCanvas.Children.Contains(this.ErrorBarsPath))
            {
                this.MarkerCanvas.Children.Add(this.ErrorBarsPath);
            }
        }

        internal void UpdateErrorBars(PathGeometry errorBarsGeometry)
        {
            this.ErrorBarsPath.Data = errorBarsGeometry;
        }

        internal void UpdateMarkerTemplate(int markerCount, int itemIndex)
        {
            if (!MarkerModel.UseLightweightMarkers)
            {
                Marker marker = Markers[markerCount];
                (marker.Content as DataContext).Item = Model.FastItemsSource[itemIndex];
            }
        }
        internal CategoryBucketCalculator BucketCalculator { get; private set; }
        internal virtual CategoryBucketCalculator CreateBucketCalculator()
        {
            return new CategoryBucketCalculator(this);
        }
        internal Pool<Marker> Markers { get; set; }

        bool ISupportsMarkers.ShouldDisplayMarkers
        {
            get { return this.CategoryModel.ShouldDisplayMarkers(); }
        }

        void ISupportsMarkers.UpdateMarkerCount(int markerCount)
        {
            Markers.Count = markerCount;
        }

        void ISupportsMarkers.UpdateMarkerTemplate(int markerCount, int itemIndex)
        {
            this.UpdateMarkerTemplate(markerCount, itemIndex);
        }
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