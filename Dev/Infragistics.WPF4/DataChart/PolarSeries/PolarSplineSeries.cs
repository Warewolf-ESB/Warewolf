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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a XamDataChart polar spline series.
    /// </summary>
    public sealed class PolarSplineSeries
        : PolarLineSeriesBase
    {
        /// <summary>
        /// Called to create the view for the series.
        /// </summary>
        /// <returns>The created view.</returns>
        protected override SeriesView CreateView()
        {
            return new PolarSplineSeriesView(this);
        }

        /// <summary>
        /// Called when the view for the series is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            PolarSplineView = (PolarSplineSeriesView)view;
        }
        internal PolarSplineSeriesView PolarSplineView { get; set; }

        #region constructor and initialisation
        /// <summary>
        /// Initializes a new instance of the PolarSplineSeries class. 
        /// </summary>
        public PolarSplineSeries()
        {
            DefaultStyleKey = typeof(PolarSplineSeries);
        }
        #endregion

        #region Stiffness Dependency Property
        /// <summary>
        /// Gets or sets the Stiffness property.
        /// <para>This is a dependency property.</para>
        /// </summary>
        [WidgetDefaultNumber(0.5)]
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
        public static readonly DependencyProperty StiffnessProperty = DependencyProperty.Register(StiffnessPropertyName, typeof(double), typeof(PolarSplineSeries),
            new PropertyMetadata(0.5, (sender, e) =>
            {
                (sender as PolarSplineSeries).RaisePropertyChanged(StiffnessPropertyName, e.OldValue, e.NewValue);
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
                    break;
            }
        }

        /// <summary>
        /// Overridden in derived classes to clear the series.
        /// </summary>
        protected override void ClearPoints(SeriesView view)
        {
            var polarSplineView = (PolarSplineSeriesView)view;
            polarSplineView.ClearPolarSpline();
        }

        /// <summary>
        /// Overridden in derived classes to render the series.
        /// </summary>
        /// <param name="frame">The frame to render</param>
        /// <param name="view">The PolarBaseView in context.</param>
        internal override void RenderPoints(PolarFrame frame, PolarBaseView view)
        {
            ClearPoints(view);

            var polarSplineView = (PolarSplineSeriesView)view;
            //PrepLinePoints(frame);
            polarSplineView.RenderPolarSpline(frame.Points, Stiffness);
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