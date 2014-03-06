using System;
using System.Collections.Generic;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart polar area series.
    /// </summary>
    public sealed class PolarAreaSeries
        : PolarLineSeriesBase
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new PolarAreaSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            PolarAreaView = (PolarAreaSeriesView)view;
        }
        internal PolarAreaSeriesView PolarAreaView { get; set; }

        /// <summary>
        /// Initializes a new PolarAreaSeries instance.
        /// </summary>
        public PolarAreaSeries()
        {
            DefaultStyleKey = typeof(PolarAreaSeries);
        }

        /// <summary>
        /// Overridden in derived classes to clear the series.
        /// </summary>
        protected override void ClearPoints(SeriesView view)
        {
            var areaView = (PolarAreaSeriesView)view;
            areaView.ClearPolarArea();
        }

        /// <summary>
        /// Overridden in derived classes to render the series.
        /// </summary>
        /// <param name="frame">The frame to render</param>
        /// <param name="view">The PolarBaseView in context.</param>
        internal override void RenderPoints(PolarFrame frame, PolarBaseView view)
        {
            if (frame.Points.Any() && frame.Points.First() != frame.Points.Last())
            {
                frame.Points.Add(frame.Points.First());
            }
            var areaView = (PolarAreaSeriesView)view;

            areaView.RenderPolarArea(frame.Points, Resolution);




        }

        /// <summary>
        /// Gets whether the shape of the series is a closed shape.
        /// </summary>
        protected override bool IsClosed
        {
            get
            {
                return true;
            }
        }

        private const string UnknownValuePlottingPropertyName = "UnknownValuePlotting";
        /// <summary>
        /// Identifies the UnknownValuePlotting dependency property.
        /// </summary>
        public static readonly DependencyProperty UnknownValuePlottingProperty =
            DependencyProperty.Register(
            UnknownValuePlottingPropertyName,
            typeof(UnknownValuePlotting),
            typeof(PolarAreaSeries),
            new PropertyMetadata(UnknownValuePlotting.DontPlot, (sender, e) =>
            {
                (sender as PolarAreaSeries).RaisePropertyChanged(
                    UnknownValuePlottingPropertyName,
                    e.OldValue,
                    e.NewValue);
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

        internal override UnknownValuePlotting GetUnknownValuePlotting()
        {
            return UnknownValuePlotting;
        }

        //internal override bool ClippingDisabled
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}
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