using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart scatter spline series
    /// </summary>
    public sealed class ScatterSplineSeries : ScatterBase
    {
        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the ScatterSplineSeries class. 
        /// </summary>
        public ScatterSplineSeries()
        {
            DefaultStyleKey = typeof(ScatterSplineSeries);

            PreviousFrame = new ScatterFrame();
            TransitionFrame = new ScatterFrame();
            CurrentFrame = new ScatterFrame();

          
        }

        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new ScatterSplineSeriesView(this);
        }

        #endregion

        #region Stiffness Dependency Property
        /// <summary>
        /// Gets or sets the Stiffness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        public double Stiffness
        {
            get
            {
                return (double)GetValue(StiffnessProperty);
            }
            set
            {
                SetValue(StiffnessProperty, value);
            }
        }

        internal const string StiffnessPropertyName = "Stiffness";

        /// <summary>
        /// Identifies the Stiffness dependency property.
        /// </summary>
        public static readonly DependencyProperty StiffnessProperty = DependencyProperty.Register(StiffnessPropertyName, typeof(double), typeof(ScatterSplineSeries),
            new PropertyMetadata(0.5, (sender, e) =>
            {
                (sender as ScatterSplineSeries).RaisePropertyChanged(StiffnessPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case StiffnessPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
            }
        }

        internal override void PrepareFrame(ScatterFrame frame, ScatterBaseView view)
        {
            base.PrepareFrame(frame, view);

            frame.Points.Clear();
            frame.LinePoints.Clear();

            Rect windowRect = view.WindowRect;
            Rect viewportRect = view.Viewport;

            double measure = Resolution * Resolution;
            int count =
                Math.Min(XColumn != null ? XColumn.Count : 0, YColumn != null ? YColumn.Count : 0);

            ScalerParams px = new ScalerParams(windowRect, viewportRect, AxisInfoCache.XAxisIsInverted);

            ScalerParams py = new ScalerParams(windowRect, viewportRect, AxisInfoCache.YAxisIsInverted);

            Func<int, double> X = (i) => AxisInfoCache.XAxis.GetScaledValue(XColumn[i], px);
            Func<int, double> Y = (i) => AxisInfoCache.YAxis.GetScaledValue(YColumn[i], py);

            Clipper clipper = new Clipper(viewportRect, false) { Target = frame.Points };

            for (int i = 0; i < count; )
            {
                int j = i;
                ++i;

                while (i < count && Measure(X, Y, j, i) < measure)
                {
                    ++i;
                }

                if (count > MaximumMarkers)
                {
                    clipper.Add(Centroid(X, Y, j, i - 1));
                }
                else
                {
                    OwnedPoint newPoint = new OwnedPoint();
                    newPoint.Point = new Point(X(j), Y(j));
                    newPoint.OwnerItem = FastItemsSource[j];

                    if (!frame.LinePoints.ContainsKey(newPoint.OwnerItem))
                    {
                        frame.LinePoints.Add(newPoint.OwnerItem, newPoint);
                    }
                }
            }

            if (count > MaximumMarkers)
            {
                clipper.Target = null;
            }
        }
        private Point Centroid(Func<int, double> X, Func<int, double> Y, int a, int b)
        {
            if (a == b)
            {
                return new Point(X(a), Y(a));
            }

            double cx = 0.0;
            double cy = 0.0;
            double weight = (double)(b - a + 1);

            for (int i = a; i <= b; ++i)
            {
                cx += X(i);
                cy += Y(i);
            }

            return new Point(cx / weight, cy / weight);
        }
        private double Measure(Func<int, double> X, Func<int, double> Y, int a, int b)
        {
            double x = X(b) - X(a);
            double y = Y(b) - Y(a);

            return x * x + y * y;
        }
        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            ScatterSplineSeriesView scatterSplineView = view as ScatterSplineSeriesView;

            ((PathGeometry)scatterSplineView.Path.Data).Reset();
        }

        internal override void RenderFrame(ScatterFrame frame, ScatterBaseView view)
        {
            ClearRendering(false, view);

            base.RenderFrame(frame, view);

            PrepLinePoints(frame);

            ScatterSplineSeriesView scatterSplineView = view as ScatterSplineSeriesView;

            #region render the path
            (scatterSplineView.Path.Data as PathGeometry).Figures = 
                Numeric.Spline2D(frame.Points.Count, 
                (i) => frame.Points[i].X, 
                (i) => frame.Points[i].Y, Stiffness);
            #endregion
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