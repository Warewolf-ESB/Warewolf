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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    internal class AxisView
    {
        protected Axis Model { get; set; }
        public AxisView(Axis model)
        {
            Model = model;
        }

        public virtual void OnInit()
        {
            AxisLines = new Path() { Data = new GeometryGroup() };
            Strips = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false, Stroke = null };
            MajorLines = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false };
            MinorLines = new Path() { Data = new GeometryGroup(), IsHitTestVisible = false };

            Model.SizeChanged += SizeChanged;
            //            Axis a = Controller.GetAxisComponentsForView().Axis;
            //            AxisLines.MouseEnter += (o, e) => OnMouseEnter(e);
            //            AxisLines.MouseLeave += (o, e) => OnMouseLeave(e);
            //            AxisLines.MouseLeftButtonDown += (o, e) => OnMouseLeftButtonDown(e);
            //            AxisLines.MouseLeftButtonUp += (o, e) => OnMouseLeftButtonUp(e);
            //            AxisLines.MouseMove += (o, e) => OnMouseMove(e);
            //#if !WINDOWS_PHONE
            //            AxisLines.MouseRightButtonDown += (o, e) => OnMouseRightButtonDown(e);
            //            AxisLines.MouseRightButtonUp += (o, e) => OnMouseRightButtonUp(e);
            //#endif
        }

        internal void BindLabelPanelStyle()
        {
            Model.LabelPanel.SetBinding(
                AxisLabelPanelBase.StyleProperty, 
                new Binding(Axis.LabelPanelStylePropertyName) { Source = Model });
        }

        private void SizeChanged(object sender, SizeChangedEventArgs args)
        {
            Model.HandleRectChanged(new Rect(new Point(0,0), args.PreviousSize), 
                new Rect(new Point(0,0), args.NewSize));
        }

        /// <summary>
        /// Gets the visuals representing the Axis line.
        /// </summary>
        protected internal Path AxisLines { get; private set; }
        /// <summary>
        /// Gets the visuals representing the major lines.
        /// </summary>
        protected internal Path MajorLines { get; private set; }
        /// <summary>
        /// Gets the visuals representing the strips.
        /// </summary>
        protected internal Path Strips { get; private set; }
        /// <summary>
        /// Gets the visuals representing the minor lines.
        /// </summary>
        protected internal Path MinorLines { get; private set; }

        protected Canvas RootCanvas
        {
            get
            {
                return Model.GetAxisComponentsForView().RootCanvas;
            }
        }

        public virtual void OnTemplateProvided()
        {
            AxisLines.Detach();
            RootCanvas.Children.Add(AxisLines);
            AxisLines.SetBinding(Shape.StrokeProperty, new Binding(Axis.StrokePropertyName) { Source = AxisPropertiesSource });
            AxisLines.SetBinding(Shape.StrokeThicknessProperty, new Binding(Axis.StrokeThicknessPropertyName) { Source = AxisPropertiesSource });
            AxisLines.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Axis.StrokeDashArrayPropertyName)
            {
                Source = AxisPropertiesSource,
                Converter = new DoubleCollectionDuplicator()
            });

            SetupStrips();

            MajorLines.Detach();
            RootCanvas.Children.Add(MajorLines);
            MajorLines.SetBinding(Shape.StrokeProperty, new Binding(Axis.MajorStrokePropertyName) { Source = AxisPropertiesSource });
            MajorLines.SetBinding(Shape.StrokeThicknessProperty, new Binding(Axis.MajorStrokeThicknessPropertyName) { Source = AxisPropertiesSource });
            MajorLines.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Axis.MajorStrokeDashArrayPropertyName)
            {
                Source = AxisPropertiesSource,
                Converter = new DoubleCollectionDuplicator()
            });

            MinorLines.Detach();
            RootCanvas.Children.Add(MinorLines);
            MinorLines.SetBinding(Shape.StrokeProperty, new Binding(Axis.MinorStrokePropertyName) { Source = AxisPropertiesSource });
            MinorLines.SetBinding(Shape.StrokeThicknessProperty, new Binding(Axis.MinorStrokeThicknessPropertyName) { Source = AxisPropertiesSource });
            MinorLines.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Axis.MinorStrokeDashArrayPropertyName)
            {
                Source = AxisPropertiesSource,
                Converter = new DoubleCollectionDuplicator()
            });
        }

        /// <summary>
        /// Creates a TextBlock object.
        /// </summary>
        /// <returns>The created TextBlock.</returns>
        protected internal TextBlock TextBlockCreate()
        {
            TextBlock textBlock = new TextBlock();
            AxisLabelManager.BindLabel(textBlock);
            textBlock.DataContext = Model.CurrentLabelSettings;
            return textBlock;
        }

        /// <summary>
        /// Activates a TextBlock object from the pool.
        /// </summary>
        /// <param name="TextBlock">The TextBlock to activate.</param>
        protected internal void TextBlockActivate(TextBlock TextBlock)
        {
            TextBlock.Visibility = Visibility.Visible;
            if (TextBlock.Parent == null)
            {
                LabelPanel.Children.Add(TextBlock);
            }
        }

        /// <summary>
        /// Disactivates a TextBlock from the pool.
        /// </summary>
        /// <param name="textBlock">The TextBlock to disactivate.</param>
        protected internal void TextBlockDisactivate(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Destroys a TextBlock from the pool.
        /// </summary>
        /// <param name="textBlock">The TextBlock to destroy.</param>
        protected internal void TextBlockDestroy(TextBlock textBlock)
        {
            textBlock.DataContext = null;
            if (textBlock.Parent as Panel != null)
            {
                LabelPanel.Children.Remove(textBlock);
            }
        }

        protected Axis AxisPropertiesSource
        {
            get
            {
                return Model.GetAxisComponentsForView().Axis;
            }
        }

        protected AxisLabelPanelBase LabelPanel
        {
            get
            {
                return Model.GetAxisComponentsForView().LabelPanel;
            }
        }

        /// <summary>
        /// Initializes the axis strip visuals.
        /// </summary>
        protected virtual void SetupStrips()
        {
            Strips.Detach();
            RootCanvas.Children.Add(Strips);
            Strips.SetBinding(Shape.FillProperty, new Binding(Axis.StripPropertyName) { Source = AxisPropertiesSource });
        }

        internal void LabelNeedRearrange()
        {
            LabelPanel.InvalidateArrange();
        }

        internal bool Ready()
        {
            return LabelPanel != null;
        }

        internal void EnsureAutoExtent()
        {
            LabelPanel.EnsureExtentSet();
        }

        internal void ClearAllMarks()
        {
            LabelPanel.Children.Clear();
            ClearMarks((AxisLines.Data as GeometryGroup).Children);
            ClearMarks((Strips.Data as GeometryGroup).Children);
            ClearMarks((MajorLines.Data as GeometryGroup).Children);
            ClearMarks((MinorLines.Data as GeometryGroup).Children);
        }

        /// <summary>
        /// Clears the marks from a geometry.
        /// </summary>
        /// <param name="geometry">The geometry to clear the marks from.</param>
        protected internal void ClearMarks(GeometryCollection geometry)
        {
            //the geometry collections dont like just being cleared.
            
            geometry.Clear();
            LineGeometry newLine = new LineGeometry();
            geometry.Add(newLine);
            geometry.Remove(newLine);
        }

        internal void UpdateLineVisibility(Visibility visible)
        {
            if (AxisLines.Visibility != visible)
            {
                AxisLines.Visibility = visible;
            }
            if (MajorLines.Visibility != visible)
            {
                MajorLines.Visibility = visible;
            }
            if (Strips.Visibility != visible)
            {
                Strips.Visibility = visible;
            }
            if (MinorLines.Visibility != visible)
            {
                MinorLines.Visibility = visible;
            }
        }

        internal void ChangeLabelSettings(AxisLabelSettings settings)
        {
            foreach (var tb in LabelPanel.Children)
            {
                if (tb is TextBlock)
                {
                    (tb as TextBlock).DataContext = settings;
                }
            }
        }

        internal void ResetLabelPanel()
        {
            LabelPanel.Children.Clear();
        }

        protected AxisComponentsFromView AxisComponentsFromView { get; set; }
        internal AxisComponentsFromView GetAxisComponentsFromView()
        {
            if (AxisComponentsFromView == null)
            {
                AxisComponentsFromView = new AxisComponentsFromView();
            }
            AxisComponentsFromView.AxisLines = AxisLines;
            AxisComponentsFromView.MajorLines = MajorLines;
            AxisComponentsFromView.MinorLines = MinorLines;
            AxisComponentsFromView.Strips = Strips;
            return AxisComponentsFromView;
        }

        internal GeometryCollection GetAxisLinesGeometry()
        {
            return (AxisLines.Data as GeometryGroup).Children;
        }

        internal GeometryCollection GetStripsGeometry()
        {
            return (Strips.Data as GeometryGroup).Children;
        }

        internal GeometryCollection GetMajorLinesGeometry()
        {
            return (MajorLines.Data as GeometryGroup).Children;
        }

        internal GeometryCollection GetMinorLinesGeometry()
        {
            return (MinorLines.Data as GeometryGroup).Children;
        }

        internal void UpdateLabelPanel(Axis axis, Rect windowRect, Rect viewportRect)
        {
            this.LabelPanel.Axis = axis;
            this.LabelPanel.WindowRect = windowRect;
            this.LabelPanel.ViewportRect = viewportRect;
        }

        internal void ClearLabelPanel()
        {
            LabelPanel.Children.Clear();
        }

        internal void SetLabelPanelCrossingValue(double crossingValue)
        {
            this.LabelPanel.CrossingValue = crossingValue;
        }

        internal void SetLabelPanelInterval(double p)
        {
            this.LabelPanel.Interval = p;
        }

        internal void UpdateLabelPanelContent(List<object> dataContext, List<LabelPosition> positions)
        {
            LabelPanel.LabelDataContext = dataContext;
            LabelPanel.LabelPositions = positions;
        }

        internal void OnLabelPanelStyleChanged(object newValue)
        {
            
        }

        internal virtual object GetLabelValue(object dataItem)
        {
            DataTemplate labelTemplate = Model.Label as DataTemplate;
            StringFormatter labelFormatter = labelTemplate != null ? null : new StringFormatter() { FormatString = Model.Label as string };
            if (labelTemplate != null)
            {
                return new ContentControl()
                {
                    Content = new AxisItemDataContext()
                    {
                        Axis = Model,
                        Item = dataItem
                    },
                    ContentTemplate = labelTemplate,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch
                };
            }
            else
            {
                return labelFormatter.Format(dataItem, null);
            }
        }

        internal void EnsureRender()
        {

        }

        internal void DetachFromChart(SeriesViewer oldSeriesViewer)
        {
            
        }

        internal void AttachToChart(SeriesViewer newSeriesViewer)
        {
            
        }

        internal bool IsDisabled()
        {
            return Model.IsDisabled;
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