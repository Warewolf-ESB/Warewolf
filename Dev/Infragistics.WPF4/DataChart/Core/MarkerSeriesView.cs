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
    internal abstract class MarkerSeriesView
        : SeriesView
    {
        protected MarkerSeries MarkerModel { get; set; }

        public MarkerSeriesView(MarkerSeries model)
            : base(model)
        {
            MarkerModel = (MarkerSeries)model;
        }

        private bool _lightweightMode = false;

        protected MarkerSeries MarkerPropertiesSource
        {
            get
            {
                return (MarkerSeries)Model.GetSeriesComponentsForView().Series;
            }
        }

        /// <summary>
        /// Creates a marker object.
        /// </summary>
        /// <returns>The created marker.</returns>
        protected internal Marker MarkerCreate()
        {
            Marker marker = new Marker();
            if (!_lightweightMode)
            {
                marker.Content = new DataContext() { Series = MarkerPropertiesSource };
                SetMarkerStyle(marker);
            }

            //marker.SetBinding(Marker.ContentTemplateProperty,
            //    new Binding(ActualMarkerTemplatePropertyName)
            //    {
            //        Source = this
            //    });
            marker.ContentTemplate = MarkerPropertiesSource.ActualMarkerTemplate;

            return marker;
        }
        /// <summary>
        /// Perform an action on all markers
        /// </summary>
        /// <returns></returns>
        protected internal abstract void DoToAllMarkers(Action<Marker> action);

        /// <summary>
        /// Updates the templates assigned to the markers.
        /// </summary>
        protected void UpdateMarkerTemplates()
        {   
            DoToAllMarkers(
                (m) =>
                {
                    m.ContentTemplate = MarkerPropertiesSource.ActualMarkerTemplate;
                    
                    m.UpdateLayout();
                });
        }

        /// <summary>
        /// Sets any available Marker style on the provided marker.
        /// </summary>
        /// <param name="marker">The marker to set the style for.</param>
        protected virtual void SetMarkerStyle(Marker marker)
        {
            marker.SetBinding(FrameworkElement.StyleProperty,
                new Binding()
                {
                    Path = new PropertyPath(MarkerSeries.MarkerStylePropertyName),
                    Source = MarkerPropertiesSource
                });
        }

        /// <summary>
        /// Activates a marker object from the pool.
        /// </summary>
        /// <param name="marker">The marker to activate.</param>
        protected internal void MarkerActivate(Marker marker)
        {
            marker.Visibility = Visibility.Visible;
            if (marker.Parent == null)
            {
                MarkerCanvas.Children.Add(marker);
            }
        }

        /// <summary>
        /// Disactivates a marker from the pool.
        /// </summary>
        /// <param name="marker">The marker to disactivate.</param>
        protected internal void MarkerDisactivate(Marker marker)
        {
            if (marker.Content != null)
            {
                (marker.Content as DataContext).Item = null;
            }

            marker.Visibility = Visibility.Collapsed;
            //if (marker.Parent as Panel != null)
            //{
            //    MarkerCanvas.Children.Remove(marker);
            //}
        }

        /// <summary>
        /// Destroys a marker from the pool.
        /// </summary>
        /// <param name="marker">The marker to destroy.</param>
        protected internal void MarkerDestroy(Marker marker)
        {
            marker.Content = null;
            marker.ClearValue(Marker.ContentTemplateProperty);
            if (marker.Parent as Panel != null)
            {
                MarkerCanvas.Children.Remove(marker);
            }
        }

        internal void DoUpdateMarkerTemplates()
        {
            UpdateMarkerTemplates();
        }

        internal void SetUseLightweightMode(bool useLightweight)
        {
            _lightweightMode = useLightweight;
        }

        internal bool HasCustomMarkerTemplate()
        {
            return this.MarkerModel.MarkerTemplate != null;
        }

        internal void ClearActualMarkerTemplate()
        {
            Model.ClearValue(MarkerSeries.ActualMarkerTemplateProperty);
        }

        internal void BindActualToCustomMarkerTemplate()
        {
            Model.SetBinding(MarkerSeries.ActualMarkerTemplateProperty, 
                new Binding(MarkerSeries.MarkerTemplatePropertyName) { Source = Model });
        }

        internal void BindActualToMarkerTemplate(string p)
        {
            Model.SetBinding(
                MarkerSeries.ActualMarkerTemplateProperty, 
                new Binding(p) { Source = Model.SeriesViewer });
        }

        internal void ClearActualMarkerBrush()
        {
            Model.ClearValue(
                MarkerSeries.ActualMarkerBrushProperty);
        }

        internal void BindActualToMarkerBrush()
        {
            Model.SetBinding(
                MarkerSeries.ActualMarkerBrushProperty, 
                new Binding(MarkerSeries.MarkerBrushPropertyName) 
                { Source = Model });
        }

        internal void ClearActualMarkerOutline()
        {
            Model.ClearValue(
                MarkerSeries.ActualMarkerOutlineProperty);
        }

        internal void BindActualToMarkerOutline()
        {
            Model.SetBinding(
                MarkerSeries.ActualMarkerOutlineProperty, 
                new Binding(
                    MarkerSeries.MarkerOutlinePropertyName) 
                    { 
                        Source = Model 
                    });
        }

        internal void RenderMarkers()
        {
            
        }
        internal void InitMarkers(HashPool<object, Marker> Markers)
        {
            Markers.Create = MarkerCreate;
            Markers.Destroy = MarkerDestroy;
            Markers.Activate = MarkerActivate;
            Markers.Disactivate = MarkerDisactivate;
        }

        internal void InitMarkers(Pool<Marker> Markers)
        {
            Markers.Create = MarkerCreate;
            Markers.Destroy = MarkerDestroy;
            Markers.Activate = MarkerActivate;
            Markers.Disactivate = MarkerDisactivate;
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