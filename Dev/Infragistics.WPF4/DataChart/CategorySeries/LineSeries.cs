using System.Windows.Data;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Charts.VisualData;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart line series.
    /// <para>Displays trend over time or ordered categories.</para>
    /// <para>Useful when there are many data points and the order is important.</para>
    /// </summary>
    public sealed class LineSeries : HorizontalAnchoredCategorySeries
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new LineSeriesView(this);
        }
        internal LineSeriesView LineView { get; set; }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);
            LineView = (LineSeriesView)view;
        }

        #region Constructor and Initalisation
        /// <summary>
        /// Initializes a new instance of the LineSeries class. 
        /// </summary>
        public LineSeries()
        {
            DefaultStyleKey = typeof(LineSeries);
        }

        #endregion

        internal override CategoryMode PreferredCategoryMode(CategoryAxisBase axis)
        {
            return CategoryMode.Mode0;
        }

        /// <summary>
        /// Clears the rendering for the series.
        /// </summary>
        /// <param name="wipeClean">True if the cached visuals should also be cleared.</param>
        /// <param name="view">The SeriesView in context.</param>
        protected internal override void ClearRendering(bool wipeClean, SeriesView view)
        {
            base.ClearRendering(wipeClean, view);

            var lineView = (LineSeriesView)view;
            lineView.ClearLine();
        }
        internal override void RenderFrame(CategoryFrame frame, CategorySeriesView view)
        {
            base.RenderFrame(frame, view);



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            Func<int, double> x0 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y0 = delegate(int i) { return frame.Buckets[i][1]; };
            Func<int, double> x1 = delegate(int i) { return frame.Buckets[i][0]; };
            Func<int, double> y1 = delegate(int i) { return frame.Buckets[i][2]; };

            int bucketSize = view.BucketCalculator.BucketSize;
            LineSeriesView lineView = view as LineSeriesView;
            lineView.RasterizeLine(frame.Buckets.Count,
                x0, y0, x1, y1, this.UnknownValuePlotting, GetLineClipper(x0, frame.Buckets.Count - 1, view.Viewport, view.WindowRect),
                bucketSize, Resolution);

        }


        private const string UnknownValuePlottingPropertyName = "UnknownValuePlotting";

        /// <summary>
        /// Identifies the UnknownValuePlotting dependency property.
        /// </summary>
        public static readonly DependencyProperty UnknownValuePlottingProperty = DependencyProperty.Register(UnknownValuePlottingPropertyName, typeof(UnknownValuePlotting), typeof(LineSeries), new PropertyMetadata(UnknownValuePlotting.DontPlot, (sender, e) =>
            {
                (sender as LineSeries).RaisePropertyChanged(UnknownValuePlottingPropertyName, e.OldValue, e.NewValue);
            }));
        /// <summary>
        /// Determines how unknown values will be plotted on the chart.
        /// </summary>
        /// <remarks>
        /// Null and Double.NaN are two examples of unknown values.
        /// </remarks>
        [WidgetDefaultString("dontPlot")]
        public UnknownValuePlotting UnknownValuePlotting
        {
            get
            {
                return (UnknownValuePlotting)this.GetValue(UnknownValuePlottingProperty);
            }
            set
            {
                this.SetValue(UnknownValuePlottingProperty, value);
            }
        }

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
                case UnknownValuePlottingPropertyName:
                    RenderSeries(false);
                    this.NotifyThumbnailAppearanceChanged();
                    break;
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