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
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    internal class PieChartBaseView
    {
        public PieChartBaseView(PieChartBase model)
        {
            Model = model;
        }

        protected PieChartBase Model { get; set; }
        internal Canvas Root { get; set; }
        internal Canvas MainPieCanvas { get; set; }
        internal Canvas LabelCanvas { get; set; }
        internal StringFormatter ToolTipFormatter { get; set; }

        private MouseButtonEventHandler _mouseUpOverride;
        private MouseEventHandler _mouseEnterOverride;
        private MouseEventHandler _mouseMoveOverride;
        private MouseEventHandler _mouseLeaveOverride;

        public virtual void OnInit()
        {
            Root = new Canvas();
            MainPieCanvas = new Canvas();
            LabelCanvas = new Canvas();
            Root.Children.Add(MainPieCanvas);

            ToolTipFormatter = new StringFormatter();

            _mouseUpOverride = (o, e) =>
            {
                Slice slice = o as Slice;
                if (slice == null)
                    return;

                Model.SliceClicked(slice, e);
            };

            _mouseEnterOverride = (o, e) =>
            {
                FrameworkElement item = o as FrameworkElement;
                if (item == null || item.DataContext == null)
                    return;

                Model.ItemEntered(o, e); 
            };

            _mouseMoveOverride = (o, e) =>
            {
                FrameworkElement item = o as FrameworkElement;
                if (item == null || item.DataContext == null)
                    return;

                Model.ItemMouseMoved(o, e);
            };

            _mouseLeaveOverride = (o, e) =>
            {
                Model.ItemMouseLeft(o, e);
            };

            ToolTipPopup = new Popup()
            {
                DataContext = new PieSliceDataContext(),
                Child = new ContentControl()
                {
                    IsHitTestVisible = false
                }
            };

            Model.SizeChanged += (o, e) => SizeUpdated();

            Canvas.SetZIndex(LabelCanvas, 100);
            Root.Children.Add(LabelCanvas);
        }

        public virtual void OnTemplateProvided()
        {
            ContentControl toolTipControl = ToolTipPopup.Child as ContentControl;
            if (toolTipControl != null)
            {
                toolTipControl.SetBinding(PieChartBase.StyleProperty, new Binding(PieChartBase.ToolTipStylePropertyName) { Source = Model });
            }

            Model.RenderChart();
        }

        private void SizeUpdated()
        {
            Model.OnSizeUpdated();
        }

        internal void InitToolTipManager(PieChartToolTipManager toolTipManager)
        {
            toolTipManager.GetChartInfo = (FrameworkElement item) =>
            {
                Slice slice = item as Slice;
                StringFormatter formatter = null;
                if (slice != null && slice.IsOthersSlice)
                {
                    formatter = new StringFormatter { FormatString = Model.OthersCategoryText };
                }

                toolTipManager.ToolTip = Model.ToolTip;
                toolTipManager.ToolTipFormatter = formatter ?? ToolTipFormatter;
                toolTipManager.ToolTipPopup = ToolTipPopup;
            };

        }

        internal Popup ToolTipPopup { get; set; }

        internal void CloseToolTip()
        {
            ContentControl toolTipControl = ToolTipPopup != null ? ToolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null)
            {
                ToolTipPopup.IsOpen = false;
            }
        }

        internal void UpdateToolTip(object item, object args)
        {
            FrameworkElement target = item as FrameworkElement;
            MouseEventArgs e = args as MouseEventArgs;
            Model.ToolTipManager.UpdateToolTip(e, target);
        }

        internal void OnContentPresenterProvided()
        {
            Model.ContentPresenter.Content = Root;
        }

        internal Slice SliceCreate()
        {
            Slice slice = new Slice();
            slice.Owner = Model;

            //create property bindigs, add events

            slice.SetBinding(Control.ForegroundProperty, new Binding("Foreground") { Source = Model });
            slice.SetBinding(Control.FontFamilyProperty, new Binding("FontFamily") { Source = Model });
            slice.SetBinding(Control.FontSizeProperty, new Binding("FontSize") { Source = Model });
            slice.SetBinding(Control.FontStretchProperty, new Binding("FontStretch") { Source = Model });
            slice.SetBinding(Control.FontStyleProperty, new Binding("FontStyle") { Source = Model });
            slice.SetBinding(Control.FontWeightProperty, new Binding("FontWeight") { Source = Model });

            slice.MouseLeftButtonUp += _mouseUpOverride;
            slice.MouseEnter += _mouseEnterOverride;
            slice.MouseLeave += _mouseLeaveOverride;
            slice.MouseMove += _mouseMoveOverride;
            return slice;
        }

        internal void SliceActivate(Slice slice)
        {
            slice.Detach();
            MainPieCanvas.Children.Add(slice);
        }

        internal void SliceDisactivate(Slice slice)
        {
            if (MainPieCanvas.Children.Contains(slice))
            {
                MainPieCanvas.Children.Remove(slice);
            }
        }

        internal void SliceDestroy(Slice slice)
        {
            //clear property bindings, remove events
            slice.Owner = null;
            slice.Background = null;

            slice.ClearValue(Control.ForegroundProperty);
            slice.ClearValue(Control.FontFamilyProperty);
            slice.ClearValue(Control.FontSizeProperty);
            slice.ClearValue(Control.FontStretchProperty);
            slice.ClearValue(Control.FontStyleProperty);
            slice.ClearValue(Control.FontWeightProperty);

            slice.MouseLeftButtonUp -= _mouseUpOverride;
            slice.MouseEnter -= _mouseEnterOverride;
            slice.MouseLeave -= _mouseLeaveOverride;
            slice.MouseMove -= _mouseMoveOverride;
        }

        internal PieLabel LabelCreate()
        {
            PieLabel label = new PieLabel { IsHitTestVisible = false };
            label.LeaderLine = new Line();

            //create property bindigs, add events
            label.LeaderLine.SetBinding(FrameworkElement.VisibilityProperty, new Binding(PieChartBase.LeaderLineVisibilityPropertyName) { Source = Model });
            label.LeaderLine.SetBinding(FrameworkElement.StyleProperty, new Binding(PieChartBase.LeaderLineStylePropertyName) { Source = Model });
            return label;
        }

        internal void LabelActivate(PieLabel label)
        {
            label.Detach();
            label.LeaderLine.Detach();
            LabelCanvas.Children.Add(label.LeaderLine);
            LabelCanvas.Children.Add(label);
        }

        internal void LabelDisactivate(PieLabel label)
        {
            if (LabelCanvas.Children.Contains(label))
            {
                LabelCanvas.Children.Remove(label);
                LabelCanvas.Children.Remove(label.LeaderLine);
            }
        }

        internal void LabelDestroy(PieLabel label)
        {
            //clear property bindings, remove events
            label.LeaderLine.ClearValue(FrameworkElement.VisibilityProperty);
            label.LeaderLine.ClearValue(FrameworkElement.StyleProperty);
        }

        internal void SetSliceAppearance(Slice slice)
        {
            if (Model.OthersCategoryStyle != null
                && slice.IsOthersSlice
                && Model.ReadLocalValue(PieChartBase.OthersCategoryStyleProperty) != DependencyProperty.UnsetValue)
            {
                foreach (Setter setter in Model.OthersCategoryStyle.Setters)
                {
                    slice.ClearValue(setter.Property);
                }
                slice.Style = Model.OthersCategoryStyle;
            }
            else if (slice.IsSelected
                && Model.AllowSliceSelection
                && Model.SelectedStyle != null
                && Model.ReadLocalValue(PieChartBase.SelectedStyleProperty) != DependencyProperty.UnsetValue)
            {
                //The slice appears selected when IsSelected is true and the chart allows selection
                foreach (Setter setter in Model.SelectedStyle.Setters)
                {
                    slice.ClearValue(setter.Property);
                }

                slice.Style = Model.SelectedStyle;
            }
            else
            {
                slice.Style = null;

                //there isn't a way to apply a style to a slice class, so we can use the following bindings.
                slice.SetBinding(Control.ForegroundProperty, new Binding("Foreground") { Source = Model });
                slice.SetBinding(Control.FontFamilyProperty, new Binding("FontFamily") { Source = Model });
                slice.SetBinding(Control.FontSizeProperty, new Binding("FontSize") { Source = Model });
                slice.SetBinding(Control.FontStretchProperty, new Binding("FontStretch") { Source = Model });
                slice.SetBinding(Control.FontStyleProperty, new Binding("FontStyle") { Source = Model });
                slice.SetBinding(Control.FontWeightProperty, new Binding("FontWeight") { Source = Model });

                Brush background;
                Brush borderBrush;

                if (Model.Brushes != null && slice.Index >= 0 && Model.Brushes.Count > 0)
                {
                    background = Model.Brushes[slice.Index % Model.Brushes.Count];
                }
                else
                {
                    background = null;
                }

                if (Model.Outlines != null && slice.Index >= 0 && Model.Outlines.Count > 0)
                {
                    borderBrush = Model.Outlines[slice.Index % Model.Outlines.Count];
                }
                else
                {
                    borderBrush = null;
                }

                slice.Background = background;
                slice.BorderBrush = borderBrush;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            }
        }

        internal object GetLabel(Slice slice)
        {
            if (slice == null || slice.Label == null) return string.Empty;

            PieLabel pieLabel = slice.Label;
            object label = pieLabel.Label;
            DataTemplate labelTemplate = label as DataTemplate;
            StringFormatter labelFormatter = labelTemplate != null ? null : new StringFormatter { FormatString = label as string };
            if (labelTemplate != null)
            {
                return new ContentControl
                {
                    DataContext = slice.DataContext,
                    Content = labelTemplate.LoadContent(),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch
                };
            }

            if (label is TextBlock)
            {
                return (label as TextBlock).Text;
            }

            return label;
        }

        internal Rect GetLabelBounds(PieLabel label)
        {
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return new Rect(0.0, 0.0, label.DesiredSize.Width, label.DesiredSize.Height);
        }

        internal Size UpdatePieViewport()
        {
            MainPieCanvas.Width = Model.RenderSize.Width;
            MainPieCanvas.Height = Model.RenderSize.Height;
            return new Size(MainPieCanvas.Width, MainPieCanvas.Height);
        }

        internal void UpdateLabelPosition(PieLabel label, double x, double y)
        {
            label.RenderTransform = new TranslateTransform() { X = x, Y = y };
        }

        internal void UpdateToolTipContent(object toolTip)
        {
            ToolTipFormatter = toolTip is string
                                           ? new StringFormatter() { FormatString = toolTip as string }
                                           : null;

            ContentControl toolTipControl = ToolTipPopup != null ? ToolTipPopup.Child as ContentControl : null;

            if (toolTipControl != null)
            {
                if (toolTip is string && ToolTipFormatter != null)
                {
                    toolTipControl.Content = ToolTipFormatter.Format(ToolTipPopup.DataContext as PieSliceDataContext,
                                                                     null);
                }

                if (toolTip is UIElement)
                {
                    toolTipControl.Content = toolTip as UIElement;
                }
            }
        }

        internal void UpdateView()
        {
            
        }

        internal void LabelPreMeasure()
        {

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