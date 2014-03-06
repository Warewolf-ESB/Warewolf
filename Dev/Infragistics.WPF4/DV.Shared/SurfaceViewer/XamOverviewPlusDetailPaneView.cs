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

namespace Infragistics.Controls
{
    internal class XamOverviewPlusDetailPaneView
    {
        protected XamOverviewPlusDetailPane Model { get; set; }
        public XamOverviewPlusDetailPaneView(XamOverviewPlusDetailPane model)
        {
            Model = model;
        }

        private FrameworkElement RootElement { get; set; }

        private Canvas _previewCanvas;
        public Canvas PreviewCanvas
        {
            get { return _previewCanvas; }
            private set { _previewCanvas = value; }
        }

        public virtual void OnInit()
        {
            this.RootCanvas = new Canvas() { IsHitTestVisible = false };
            this.RootCanvas.SizeChanged += (o, e) => { Model.OnContainerSizeChanged(); };

            this.PreviewCanvas = new Canvas();
            this.RootCanvas.Children.Add(this.PreviewCanvas);

            this.WorldPath = new Path() { IsHitTestVisible = true, Data = new RectangleGeometry() };
            this.WorldPath.SetBinding(Path.StyleProperty, new Binding(XamOverviewPlusDetailPane.WorldStylePropertyName) { Source = Model });
            this.RootCanvas.Children.Add(this.WorldPath);

            this.WindowPath = new Path() { IsHitTestVisible = false, Data = new GeometryGroup(), Clip = new RectangleGeometry() };
            this.WindowPath.SetBinding(Path.StyleProperty, new Binding(XamOverviewPlusDetailPane.WindowStylePropertyName) { Source = Model });
            (this.WindowPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
            (this.WindowPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
            this.RootCanvas.Children.Add(this.WindowPath);

            this.PreviewPath = new Path() { IsHitTestVisible = false, Data = new GeometryGroup(), Clip = new RectangleGeometry() };
            this.PreviewPath.SetBinding(Path.StyleProperty, new Binding(XamOverviewPlusDetailPane.PreviewStylePropertyName) { Source = Model });
            (this.PreviewPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
            (this.PreviewPath.Data as GeometryGroup).Children.Add(new RectangleGeometry());
            this.RootCanvas.Children.Add(this.PreviewPath);

            Model.SetBinding(XamOverviewPlusDetailPane.InnerHorizontalAlignmentProperty, new Binding("HorizontalAlignment") { Source = Model });
            Model.SetBinding(XamOverviewPlusDetailPane.InnerVerticalAlignmentProperty, new Binding("VerticalAlignment") { Source = Model });

            Model.MouseEnter += new MouseEventHandler(Model_MouseEnter);
            Model.MouseLeave += new MouseEventHandler(Model_MouseLeave);
            Model.KeyDown += new KeyEventHandler(Model_KeyDown);
            Model.MouseLeftButtonDown += new MouseButtonEventHandler(Model_MouseLeftButtonDown);
            Model.MouseMove += new MouseEventHandler(Model_MouseMove);
            Model.MouseLeftButtonUp += new MouseButtonEventHandler(Model_MouseLeftButtonUp);
            Model.MouseWheel += new MouseWheelEventHandler(Model_MouseWheel);
        }

        private void SetBinding(DependencyProperty dependencyProperty, Binding binding)
        {
            throw new NotImplementedException();
        }

        void Model_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            Model.OnMouseWheel(e.Delta / 1200.0);
        }

        void Model_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this.RootCanvas);
            e.Handled = Model.OnMouseLeftButtonUp(p);
        }

        void Model_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this.RootCanvas);
            Model.OnMouseMove(p, true, false);
        }

        void Model_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Point p = e.GetPosition(this.RootCanvas);
            Model.OnMouseLeftButtonDown(p);
        }

        void Model_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = Model.OnKeyDown(e.Key);
        }

        void Model_MouseLeave(object sender, MouseEventArgs e)
        {
            Model.OnMouseLeave();
        }

        private void Model_MouseEnter(object sender, MouseEventArgs e)
        {
            Model.OnMouseEnter();
        }

        internal void GoToFullState()
        {
            VisualStateManager.GoToState(Model, XamOverviewPlusDetailPane.StateFull, false);
        }

        internal void GoToMinimalState()
        {
            VisualStateManager.GoToState(Model, XamOverviewPlusDetailPane.StateMinimal, false);
        }

        internal void GoToZoomEnabledState()
        {
            VisualStateManager.GoToState(Model, XamOverviewPlusDetailPane.StateZoomEnabled, false);
        }

        internal void GoToZoomDisabledState()
        {
            VisualStateManager.GoToState(Model, XamOverviewPlusDetailPane.StateZoomDisabled, false);
        }

        private Canvas RootCanvas { get; set; }
        private ContentPresenter _contentPresenter;

        private ContentPresenter ContentPresenter
        {
            get { return _contentPresenter; }
            set
            {
                if (_contentPresenter != value)
                {
                    if (_contentPresenter != null)
                    {
                        _contentPresenter.Content = null;
                    }

                    _contentPresenter = value;

                    if (_contentPresenter != null)
                    {
                        _contentPresenter.Content = this.RootCanvas;
                    }
                }
            }
        }

        private ButtonBase ButtonZoomIn { get; set; }
        private ButtonBase ButtonZoomTo100 { get; set; }
        private ButtonBase ButtonScaleToFit { get; set; }
        private ButtonBase ButtonZoomOut { get; set; }

        private Panel ButtonsGrid { get; set; }

        private ToggleButton ButtonCursor { get; set; }
        private ToggleButton ButtonDragPan { get; set; }
        private ToggleButton ButtonDragZoom { get; set; }

        private Path WorldPath { get; set; }
        private Path WindowPath { get; set; }
        private Path PreviewPath { get; set; }
        private Slider SliderZoomLevel { get; set; }

        public virtual void OnTemplateProvided()
        {
            ContentPresenter =
                Model.FetchChildByName(XamOverviewPlusDetailPane.ContentPresenterElementName) as ContentPresenter;

            // init controls
            if (this.ButtonZoomIn != null)
            {
                this.ButtonZoomIn.Click -= ButtonZoomIn_Click;
            }
            this.ButtonZoomIn = Model.FetchChildByName(XamOverviewPlusDetailPane.ZoomInName) as ButtonBase;
            if (this.ButtonZoomIn != null)
            {
                this.ButtonZoomIn.Click += ButtonZoomIn_Click;
            }

            if (this.ButtonZoomOut != null)
            {
                this.ButtonZoomOut.Click -= ButtonZoomOut_Click;
            }
            this.ButtonZoomOut = Model.FetchChildByName(XamOverviewPlusDetailPane.ZoomOutName) as ButtonBase;
            if (this.ButtonZoomOut != null)
            {
                this.ButtonZoomOut.Click += ButtonZoomOut_Click;
            }

            if (this.ButtonZoomTo100 != null)
            {
                this.ButtonZoomTo100.Click -= ButtonZoomTo100_Click;
            }
            this.ButtonZoomTo100 = Model.FetchChildByName(XamOverviewPlusDetailPane.ZoomTo100Name) as ButtonBase;
            if (this.ButtonZoomTo100 != null)
            {
                this.ButtonZoomTo100.Click += ButtonZoomTo100_Click;
            }

            if (this.ButtonScaleToFit != null)
            {
                this.ButtonScaleToFit.Click -= ButtonScaleToFit_Click;
            }
            this.ButtonScaleToFit = Model.FetchChildByName(XamOverviewPlusDetailPane.ScaleToFitName) as ButtonBase;
            if (this.ButtonScaleToFit != null)
            {
                this.ButtonScaleToFit.Click += ButtonScaleToFit_Click;
            }

            this.SliderZoomLevel = Model.FetchChildByName(XamOverviewPlusDetailPane.ZoomLevelName) as Slider;

            // get visual state minimal
            this.RootElement = Model.FetchChildByName(XamOverviewPlusDetailPane.RootElementName) as FrameworkElement;

            if (this.ButtonCursor != null)
            {
                this.ButtonCursor.Click -= ButtonCursor_Click;
            }
            this.ButtonCursor = Model.FetchChildByName(XamOverviewPlusDetailPane.ButtonCursorName) as ToggleButton;
            if (this.ButtonCursor != null)
            {
                this.ButtonCursor.Click += ButtonCursor_Click;
            }


            if (this.ButtonDragPan != null)
            {
                this.ButtonDragPan.Click -= ButtonDragPan_Click;
            }
            this.ButtonDragPan = Model.FetchChildByName(XamOverviewPlusDetailPane.DragPanName) as ToggleButton;
            if (this.ButtonDragPan != null)
            {
                this.ButtonDragPan.Click += ButtonDragPan_Click;
            }


            if (this.ButtonDragZoom != null)
            {
                this.ButtonDragZoom.Click -= ButtonDragZoom_Click;
            }
            this.ButtonDragZoom = Model.FetchChildByName(XamOverviewPlusDetailPane.DragZoomName) as ToggleButton;
            if (this.ButtonDragZoom != null)
            {
                this.ButtonDragZoom.Click += ButtonDragZoom_Click;
            }

            this.ButtonsGrid = Model.FetchChildByName(XamOverviewPlusDetailPane.ButtonsGridName) as Panel;

            //IList vsGroups = VisualStateManager.GetVisualStateGroups(this.RootElement);
            //foreach (VisualStateGroup group in vsGroups)
            //{
            //    if (group.Name != SizeStates)
            //    {
            //        continue;
            //    }

            //    foreach (VisualState state in group.States)
            //    {
            //        if (state.Name == StateMinimal)
            //        {
            //            this.VisualStateMinimal = state;
            //            break;
            //        }
            //    }
            //}

            SetSurfaceBindings();

            Model.Refresh(false);

            SetRenderOrigin();

            if (Model.ShrinkToThumbnail)
            {
                VisualStateManager.GoToState(Model, XamOverviewPlusDetailPane.StateMinimal, false);
            } 
        }

        private void ButtonCursor_Click(object sender, RoutedEventArgs e)
        {
            if (this.ButtonsGrid.Visibility == Visibility.Visible)
            {
                this.ButtonsGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.ButtonsGrid.Visibility = Visibility.Visible;
            }
        }

        private void ButtonDragZoom_Click(object sender, RoutedEventArgs e)
        {
            this.ButtonDragPan.IsChecked = this.ButtonDragZoom.IsChecked == false;

            this.Model.OnDefaultInteraction(InteractionState.DragZoom);
        }

        private void ButtonDragPan_Click(object sender, RoutedEventArgs e)
        {
            this.ButtonDragZoom.IsChecked = this.ButtonDragPan.IsChecked == false;

            this.Model.OnDefaultInteraction(InteractionState.DragPan);
        }

        private void ButtonScaleToFit_Click(object sender, RoutedEventArgs e)
        {
            Model.OnScaleToFit();
        }

        private void ButtonZoomTo100_Click(object sender, RoutedEventArgs e)
        {
            Model.OnZoomTo100();
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (this.SliderZoomLevel != null)
            {
                this.SliderZoomLevel.Value -= this.SliderZoomLevel.SmallChange;
            }
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (this.SliderZoomLevel != null)
            {
                this.SliderZoomLevel.Value += this.SliderZoomLevel.SmallChange;
            }
        }

        public Rect Viewport
        {
            get
            {
                return this.RootCanvas.ActualWidth * this.RootCanvas.ActualHeight > 0.0 ? new Rect(0, 0, this.RootCanvas.ActualWidth, this.RootCanvas.ActualHeight) : Rect.Empty;
            }
        }

        internal void SetRenderOrigin()
        {
            if (this.RootElement == null)
            {
                return;
            }

            Point pt = new Point(0, 0);

            switch (Model.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    break;
                case HorizontalAlignment.Right:
                    pt.X = 1;
                    break;
                default:
                    pt.X = 0.5;
                    break;
            }

            switch (Model.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    break;
                case VerticalAlignment.Bottom:
                    pt.Y = 1;
                    break;
                default:
                    pt.Y = 0.5;
                    break;
            }


            this.RootElement.RenderTransformOrigin = pt;
        }

        internal void UpdateWorldRect(Rect world)
        {
            (this.WorldPath.Data as RectangleGeometry).Rect = world;
            this.WorldPath.Visibility = world.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        internal void UpdateWindowPath(Rect world, Rect window)
        {
            (this.WindowPath.Clip as RectangleGeometry).Rect = world;
            ((this.WindowPath.Data as GeometryGroup).Children[0] as RectangleGeometry).Rect = world.Inflate(new Thickness(2));
            ((this.WindowPath.Data as GeometryGroup).Children[1] as RectangleGeometry).Rect = window;
            this.WindowPath.Visibility = window.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        internal void UpdatePreviewPath(Rect world, Rect preview)
        {
            (this.PreviewPath.Clip as RectangleGeometry).Rect = world;
            ((this.PreviewPath.Data as GeometryGroup).Children[0] as RectangleGeometry).Rect = world.Inflate(new Thickness(2));
            ((this.PreviewPath.Data as GeometryGroup).Children[1] as RectangleGeometry).Rect = preview;
            this.PreviewPath.Visibility = preview.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        internal void SetSurfaceBindings()
        {
            if (Model.SurfaceViewer == null || this.SliderZoomLevel == null)
            {
                return;
            }

            this.SliderZoomLevel.DataContext = Model.SurfaceViewer;
            this.SliderZoomLevel.SetBinding(Slider.ValueProperty, new Binding("ZoomLevel") { Mode = BindingMode.TwoWay });
        }

        internal bool IsReady()
        {
            return this.SliderZoomLevel != null;
        }

        internal double GetSliderValue()
        {
            return SliderZoomLevel.Value;
        }

        internal void SetSliderMin(double minimum)
        {
            SliderZoomLevel.Minimum = minimum;
        }

        internal void SetSliderMax(double maximum)
        {
            SliderZoomLevel.Maximum = maximum;
        }

        internal void CancelMouseOperations()
        {
            Model.ReleaseMouseCapture();
        }

        internal bool CaptureMouse()
        {
            return Model.CaptureMouse();
        }

        internal void PositionPreview(Rect world)
        {
            bool sizeChanged = false;
            var oldSize = new Size(this.PreviewCanvas.Width, this.PreviewCanvas.Height);
            if (Math.Round(this.PreviewCanvas.Width) != Math.Round(world.Width))
            {
                sizeChanged = true;
                this.PreviewCanvas.Width = world.Width;
            }
            if (Math.Round(this.PreviewCanvas.Height) != Math.Round(world.Height))
            {
                sizeChanged = true;
                this.PreviewCanvas.Height = world.Height;
            }
            Canvas.SetLeft(this.PreviewCanvas, world.X);
            Canvas.SetTop(this.PreviewCanvas, world.Y);

            if (sizeChanged)
            {
                var newSize = new Size(this.PreviewCanvas.Width, this.PreviewCanvas.Height);
                Model.OnThumbnailSizeChanged(new PropertyChangedEventArgs<Size>("ThumbnailSize", oldSize, newSize));
            }
        }

        public bool UseDeltaZoom { get; set; }
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