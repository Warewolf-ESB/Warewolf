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
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Provides data for XamDataChart stacked series.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StackedSeriesCreatedEventArgs : EventArgs
    {
        internal StackedSeriesCreatedEventArgs(StackedFragmentSeries series)
        {
            Series = series;
        }

        internal StackedFragmentSeries Series { get; set; }

        /// <summary>
        /// Gets or sets the series brush.
        /// </summary>
        public Brush Brush 
        {
            get { return Series.Brush; }
            set { Series.Brush = value; }
        }

        /// <summary>
        /// Gets or sets the series legend item template.
        /// </summary>
        public DataTemplate LegendItemTemplate
        {
            get { return Series.LegendItemTemplate; }
            set { Series.LegendItemTemplate = value; }
        }

        /// <summary>
        /// Gets or sets the series legend item badge templae.
        /// </summary>
        public DataTemplate LegendItemBadgeTemplate
        {
            get { return Series.LegendItemBadgeTemplate; }
            set { Series.LegendItemBadgeTemplate = value; }
        }

        /// <summary>
        /// Gets or sets the visibility of the series legend.
        /// </summary>
        public Visibility LegendItemVisibility
        {
            get { return Series.LegendItemVisibility; }
            set { Series.LegendItemVisibility = value; }
        }

        /// <summary>
        /// Gets or sets the series outline brush.
        /// </summary>
        public Brush Outline
        {
            get { return Series.Outline; }
            set { Series.Outline = value; }
        }
        /// <summary>
        /// Gets or sets the series stroke dash array.
        /// </summary>
        public DoubleCollection DashArray
        {
            get { return Series.DashArray; }
            set { Series.DashArray = value; }
        }

        /// <summary>
        /// Gets or sets the series dash cap.
        /// </summary>
        public PenLineCap DashCap
        {
            get { return Series.DashCap; }
            set { Series.DashCap = value; }
        }

        /// <summary>
        /// Gets the index of the series.
        /// </summary>
        public int Index
        {
            get { return Series.Index; }
        }

        /// <summary>
        /// Gets or sets the series outline thickness.
        /// </summary>
        public double Thickness
        {
            get { return Series.Thickness; }
            set { Series.Thickness = value; }
        }

        /// <summary>
        /// Gets or sets the series title.
        /// </summary>
        public object Title
        {
            get { return Series.Title; }
            set { Series.Title = value; }
        }

        /// <summary>
        /// Gets or sets the series tooltip.
        /// </summary>
        public object ToolTip
        {
            get { return Series.ToolTip; }
            set { Series.ToolTip = value; }
        }


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Gets or sets the series marker brush.
        /// </summary>
        public Brush MarkerBrush
        {
            get { return Series.MarkerBrush; }
            set { Series.MarkerBrush = value; }
        }

        /// <summary>
        /// Gets or sets the series marker outline.
        /// </summary>
        public Brush MarkerOutline
        {
            get { return Series.MarkerOutline; }
            set { Series.MarkerOutline = value; }
        }

        /// <summary>
        /// Gets or sets the series marker style.
        /// </summary>
        public Style MarkerStyle
        {
            get { return Series.MarkerStyle; }
            set { Series.MarkerStyle = value; }
        }

        /// <summary>
        /// Gets or sets the series marker template.
        /// </summary>
        public DataTemplate MarkerTemplate
        {
            get { return Series.MarkerTemplate; }
            set { Series.MarkerTemplate = value; }
        }

        /// <summary>
        /// Gets or sets the series marker type.
        /// </summary>
        public MarkerType MarkerType
        {
            get { return Series.MarkerType; }
            set { Series.MarkerType = value; }
        }

        /// <summary>
        /// Gets or sets the start cap.
        /// </summary>
        public PenLineCap StartCap
        {
            get { return Series.ActualStartCap; }
            set { Series.StartCap = value; }
        }

        /// <summary>
        /// Gets or sets the end cap.
        /// </summary>
        public PenLineCap EndCap
        {
            get { return Series.ActualEndCap; }
            set { Series.EndCap = value; }
        }
    }

    /// <summary>
    /// Event handler for the SeriesCreated event.
    /// </summary>
    /// <param name="sender">event sender</param>
    /// <param name="e">event parameters</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void StackedSeriesCreatedEventHandler(object sender, StackedSeriesCreatedEventArgs e);
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