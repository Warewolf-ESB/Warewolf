using System;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Provides data for XamDataChart mouse button related events. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ChartMouseEventArgs : EventArgs
    {
        internal ChartMouseEventArgs(SeriesViewer chart, Series series, object item, MouseEventArgs originalEvent)
        {
            Chart = chart;
            Series = series;
            Item = item;
            OriginalEvent = originalEvent;
        }

        /// <summary>
        /// Provides a human readable description of the mouse button event.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Chart.Name + ", " + Series.Name + ", " + (Item != null ? Item.ToString() : "") + ", " + GetPosition(null).ToString();
        }
        [DontObfuscate]
        private MouseEventArgs OriginalEvent { get; set; }

        /// <summary>
        /// Returns the x- and y- coordinates of the mouse pointer position, optionally evaluated
        /// against the origin of a supplied UIElement.
        /// </summary>
        /// <param name="relativeTo">Any UIElement derived object that is contained by the Silverlight plug-in
        /// and connected to the object tree. To specify the object relative to the overall Silverlight
        /// coordinate system, use a relativeTo value of null.</param>
        /// <returns></returns>
        public Point GetPosition(UIElement relativeTo)
        {
            return OriginalEvent.GetPosition(relativeTo);
        }


        /// <summary>
        /// Gets an object that reports stylus device information, such as the collection of stylus
        /// points associated with the input.
        /// </summary>
        public StylusDevice StylusDevice
        {
            get { return OriginalEvent.StylusDevice; }
        }


        /// <summary>
        /// Gets a reference to the object that raised the event.
        /// </summary>
        public object OriginalSource
        {
            get { return OriginalEvent.OriginalSource; }
        }

        /// <summary>
        /// Gets the ItemsSource item associated with the current event.
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Gets the series associated with the current event.
        /// </summary>
        public Series Series { get; private set; }

        /// <summary>
        /// Gets the Chart associated with the current event.
        /// </summary>
        public SeriesViewer Chart { get; private set; }
    }

    /// <summary>
    /// Represents the method that will handle XamDataChart mouse related events.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void DataChartMouseEventHandler(object sender, ChartMouseEventArgs e);

    /// <summary>
    /// Provides data for XamDataChart legend mouse related events. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ChartLegendMouseEventArgs
        : ChartMouseEventArgs
    {
        internal ChartLegendMouseEventArgs(SeriesViewer chart, Series series, 
            object item, MouseEventArgs mouseEventArgs, object legendItem) 
            : base(chart,series,item, mouseEventArgs)
        {
            this.LegendItem = legendItem;
        }

        /// <summary>
        /// The legend item that was the target of the mouse event.
        /// </summary>
        public object LegendItem { get; private set; }
    }

    /// <summary>
    /// Represents the method that will handle XamDataChart legend mouse related events.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void DataChartLegendMouseEventHandler(object sender, ChartMouseEventArgs e);
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