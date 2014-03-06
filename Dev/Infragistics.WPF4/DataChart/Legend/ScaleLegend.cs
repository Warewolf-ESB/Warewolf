using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;



using System.Linq;

using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represnts a legend that indicates the size and the color scale for a collection of series.
    /// </summary>

    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "LegendScale", Type = typeof(UIElement))]
    [TemplatePart(Name = "MinText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "MaxText", Type = typeof(TextBlock))]

    [DontObfuscate]
    [WidgetModule("ScatterChart")]
    public class ScaleLegend : LegendBase
    {
        internal override LegendBaseView CreateView()
        {
            return new ScaleLegendView(this);
        }
        internal override void OnViewCreated(LegendBaseView view)
        {
            base.OnViewCreated(view);

            ScaleView = (ScaleLegendView)view;
        }
        internal ScaleLegendView ScaleView { get; set; }


        #region Template Parts
        
        /// <summary>
        /// Gets the shape that represents the legend scale.
        /// </summary>
        public UIElement LegendScaleElement { get { return ScaleView.LegendScaleElement; } }

        /// <summary>
        /// Gets the TextBlock that shows the minimum scale value.
        /// </summary>
        public TextBlock MinText { get { return ScaleView.MinText; } }

        /// <summary>
        /// Gets the TextBlock that shows the maximum scale value.
        /// </summary>
        public TextBlock MaxText { get { return ScaleView.MaxText; } }

        #endregion

        /// <summary>
        /// Creates a new instance of ScaleLegend.
        /// </summary>
        public ScaleLegend()
        {
            DefaultStyleKey = typeof(ScaleLegend);
        }


        internal DependencyObject GetTemplateElement(string partName)
        {
            return GetTemplateChild(partName);
        }


        internal const string ParentVisibilityPropertyName = "ParentVisibility";
        internal Visibility ParentVisibility 
        { 
            get { return (Visibility)GetValue(ParentVisibilityProperty); } 
            set{SetValue(ParentVisibilityProperty, value);} 
        }
        internal static readonly DependencyProperty ParentVisibilityProperty = DependencyProperty.Register(ParentVisibilityPropertyName, typeof(Visibility), typeof(ScaleLegend),
            new PropertyMetadata((o, e) => 
            {
                if ((Visibility)e.NewValue != Visibility.Visible)
                {
                    (o as ScaleLegend).RestoreOriginalState();
                }
                else
                {
                    (o as ScaleLegend).RenderLegend();
                }
            }));

        internal const string SeriesMarkerBrushPropertyName = "SeriesMarkerBrush";
        internal Brush SeriesMarkerBrush
        {
            get { return (Brush)GetValue(SeriesMarkerBrushProperty); }
            set { SetValue(SeriesMarkerBrushProperty, value); }
        }
        internal static readonly DependencyProperty SeriesMarkerBrushProperty = DependencyProperty.Register(SeriesMarkerBrushPropertyName, typeof(Brush), typeof(ScaleLegend),
            new PropertyMetadata((o, e) => 
            {
                (o as ScaleLegend).RenderLegend();
            }));

        internal double MinimumValue { get; set; }
        internal double MaximumValue { get; set; }
        internal ObservableCollection<Brush> Brushes { get; set; }
        internal IFastItemColumn<double> SizeValueColumn { get; set; }
        internal IFastItemColumn<double> BrushValueColumn { get; set; }
        internal BrushScale BrushScale { get; set; }

        private BubbleSeries _series;
        internal BubbleSeries Series 
        { 
            get
            {
                return _series;
            }
            set
            {

                _series=value;
                SetBinding(ParentVisibilityProperty, new Binding("Visibility") { Source=_series });
                SetBinding(SeriesMarkerBrushProperty, new Binding(MarkerSeries.MarkerBrushPropertyName) { Source = _series});


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

            } 
        }

        
        internal void RestoreOriginalState()
        {
            ScaleView.RestoreOriginalState();
        }

        private Brush GetBrush(int index)
        {
            if (Series == null) return null;

            CustomPaletteBrushScale customPaletteBrushScale = Series.FillScale as CustomPaletteBrushScale;
            if (customPaletteBrushScale != null && Series.FastItemsSource != null)
            {
                return customPaletteBrushScale.GetBrush(index, Series.FastItemsSource.Count);
            }

            ValueBrushScale valueBrushScale = Series.FillScale as ValueBrushScale;
            if (valueBrushScale != null)
            {
                return valueBrushScale.GetBrushByIndex(index, BrushValueColumn);
            }

            return null;
        }

        private Color GetFirstColor(Brush brush)
        {
            if (brush == null)
            {
                return ScaleView.GetTransparentBrush();
            }
            return ColorUtil.GetColor(brush);
        }

        internal void InitializeLegend(Series series)
        {
            //make this work with bubble series only for now.
            BubbleSeries bubbleSeries = series as BubbleSeries;
            if (bubbleSeries == null || series.Visibility != Visibility.Visible) return;

            SizeValueColumn = bubbleSeries.InternalRadiusColumn;
            BrushScale = bubbleSeries.FillScale;
            BrushValueColumn = bubbleSeries.FillColumn;
            Series = bubbleSeries;

            //set legend brushes
            Brushes = new ObservableCollection<Brush>();

            RenderLegend();
        }

        internal void RenderLegend()
        {
            if (LegendScaleElement == null
                || BrushValueColumn == null
                || SizeValueColumn == null
                || SizeValueColumn.Count == 0)
            {
                return;
            }

            if (Series == null || Series.ActualLegend != this)
            {
                return;
            }

            bool useSeriesBrush = false;
            Shape legendScaleShapeElement = LegendScaleElement as Shape;
            if (legendScaleShapeElement != null)
            {
                var gradient = ScaleView.BuildGradient();

                for (int i = 0; i < SizeValueColumn.Count; i++)
                {
                    if (BrushScale == null || BrushScale.Brushes == null || BrushScale.Brushes.Count == 0)
                    {
                        if (Series != null)
                        {
                           useSeriesBrush = true;
                        }
                        break;
                    }

                    double scaledColorIndex = (BrushValueColumn[i] - BrushValueColumn.Minimum) / (BrushValueColumn.Maximum - BrushValueColumn.Minimum);
                    double colorOffset = (SizeValueColumn[i] - SizeValueColumn.Minimum) / (SizeValueColumn.Maximum - SizeValueColumn.Minimum);

                    if (double.IsNaN(scaledColorIndex))
                    {
                        //why can this happen? Because there's only one item in the series, or all items have the same value.
                        scaledColorIndex = 0;
                    }

                    if (double.IsNaN(colorOffset))
                    {
                        colorOffset = scaledColorIndex;
                    }

                    Color defaultColor = Series != null ? GetFirstColor(Series.ActualMarkerBrush) : ScaleView.GetTransparentBrush();
                    Brush brush = GetBrush(i);
                    Color color = brush != null ? GetFirstColor(brush) : defaultColor;

                    ScaleView.AddGradientStop(gradient, color, colorOffset);
                }

                ScaleView.SetScaleFill(legendScaleShapeElement, useSeriesBrush, gradient);
            }

            if (MinText != null)
            {



                MinText.Text = SizeValueColumn.Minimum.ToString();

            }

            if (MaxText != null)
            {



                MaxText.Text = SizeValueColumn.Maximum.ToString();

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