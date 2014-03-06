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
using System.Collections.Generic;
using Infragistics.Controls.Charts.Messaging;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.Security;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the visual component of a sparkline.
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class XamSparklineView : Control
    {
        internal Popup ToolTipPopup { get; private set; }

        #region Initialization
        /// <summary>
        /// Creates a new instance of XamSparklineView.
        /// </summary>
        public XamSparklineView()
        {
            DefaultStyleKey = typeof(XamSparklineView);

            MessageHandlers[typeof(ClearMessage)] =
                new MessageEventHandler((m) => ClearLayer(m as ClearMessage));
            MessageHandlers[typeof(PolygonMessage)] =
               new MessageEventHandler((m) => UpdatePolygon(m as PolygonMessage));
            MessageHandlers[typeof(ColumnMessage)] =
                new MessageEventHandler((m) => UpdateColumns(m as ColumnMessage));
            MessageHandlers[typeof(WinLossColumnMessage)] =
                new MessageEventHandler((m) => UpdateWinLossColumns(m as WinLossColumnMessage));
            MessageHandlers[typeof(MarkerMessage)] =
                new MessageEventHandler((m) => UpdateMarkers(m as MarkerMessage));
            MessageHandlers[typeof(NormalRangeMessage)] =
                new MessageEventHandler((m) => UpdateRange(m as NormalRangeMessage));
            MessageHandlers[typeof(TrendLineMessage)] =
                new MessageEventHandler((m) => UpdateTrendLine(m as TrendLineMessage));
            MessageHandlers[typeof(ToolTipMessage)] =
                new MessageEventHandler((m) => UpdateToolTip(m as ToolTipMessage));
            MessageHandlers[typeof(ToolTipTemplateMessage)] =
                new MessageEventHandler((m) => UpdateToolTipTemplate(m as ToolTipTemplateMessage));

            MouseLeave += new MouseEventHandler(SparklineView_MouseLeave);
            MouseMove += new MouseEventHandler(SparklineView_MouseMove);

            Loaded += (o, e) =>
            {
                if (Connector == null)
                {
                    this.Connector = new SparklineConnector(this, this);
                    if (this.Connector.Model != null && this.ToolTipContent != null)
                    {
                        this.ToolTipContent.ContentTemplate = this.Connector.Model.ToolTip as DataTemplate;
                    }



                    this.ToolTipPopup.SetBinding(Popup.VisibilityProperty, new Binding(XamSparkline.ToolTipVisibilityPropertyName) { Source = this.Connector.Model });

                }
                SparkPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.BrushPropertyName) { Source = Connector.Model });
                NegativeSparkPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.NegativeBrushPropertyName) { Source = Connector.Model });

                SparkPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.BrushPropertyName) { Source = Connector.Model });
                NegativeSparkPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.NegativeBrushPropertyName) { Source = Connector.Model });

                this.SparkPath.SetBinding(Path.StrokeThicknessProperty, new Binding(XamSparkline.LineThicknessPropertyName) { Source = Connector.Model });
                this.NegativeSparkPath.SetBinding(Path.StrokeThicknessProperty, new Binding(XamSparkline.LineThicknessPropertyName) { Source = Connector.Model });

                this.SparkPath.SetBinding(Path.StrokeMiterLimitProperty, new Binding(XamSparkline.LineMiterLimitPropertyName) { Source = Connector.Model });
                this.NegativeSparkPath.SetBinding(Path.StrokeMiterLimitProperty, new Binding(XamSparkline.LineMiterLimitPropertyName) { Source = Connector.Model });

                TrendLinePath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.TrendLineBrushPropertyName) { Source = Connector.Model });
                TrendLinePath.SetBinding(Path.StrokeThicknessProperty, new Binding(XamSparkline.TrendLineThicknessPropertyName) { Source = Connector.Model });

                MarkersPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.MarkerBrushPropertyName) { Source = Connector.Model });
                NegativeMarkersPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.NegativeMarkerBrushPropertyName) { Source = Connector.Model });
                LowMarkersPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.LowMarkerBrushPropertyName) { Source = Connector.Model });
                HighMarkersPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.HighMarkerBrushPropertyName) { Source = Connector.Model });
                FirstMarkerPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.FirstMarkerBrushPropertyName) { Source = Connector.Model });
                LastMarkerPath.SetBinding(Path.FillProperty, new Binding(XamSparkline.LastMarkerBrushPropertyName) { Source = Connector.Model });

                MarkersPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.MarkerBrushPropertyName) { Source = Connector.Model });
                NegativeMarkersPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.NegativeMarkerBrushPropertyName) { Source = Connector.Model });
                LowMarkersPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.LowMarkerBrushPropertyName) { Source = Connector.Model });
                HighMarkersPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.HighMarkerBrushPropertyName) { Source = Connector.Model });
                FirstMarkerPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.FirstMarkerBrushPropertyName) { Source = Connector.Model });
                LastMarkerPath.SetBinding(Path.StrokeProperty, new Binding(XamSparkline.LastMarkerBrushPropertyName) { Source = Connector.Model });
                
                if (!SparkLayer.Children.Contains(SparkPath))
                {
                    SparkLayer.Children.Add(SparkPath);
                }
                if (!SparkLayer.Children.Contains(NegativeSparkPath))
                {
                    SparkLayer.Children.Add(NegativeSparkPath);
                }
                MarkersPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.MarkerVisibilityPropertyName) { Source = Connector.Model });
                NegativeMarkersPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.NegativeMarkerVisibilityPropertyName) { Source = Connector.Model });
                LowMarkersPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.LowMarkerVisibilityPropertyName) { Source = Connector.Model });
                HighMarkersPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.HighMarkerVisibilityPropertyName) { Source = Connector.Model });
                FirstMarkerPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.FirstMarkerVisibilityPropertyName) { Source = Connector.Model });
                LastMarkerPath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.LastMarkerVisibilityPropertyName) { Source = Connector.Model });

                RangePath.SetBinding(Path.VisibilityProperty, new Binding(XamSparkline.NormalRangeVisibilityPropertyName) { Source = Connector.Model });
                RangePath.SetBinding(Path.FillProperty, new Binding(XamSparkline.NormalRangeFillPropertyName) { Source = Connector.Model });
                if (!MarkerLayer.Children.Contains(MarkersPath))
                {
                    MarkerLayer.Children.Add(MarkersPath);
                }
                if (!MarkerLayer.Children.Contains(NegativeMarkersPath))
                {
                    MarkerLayer.Children.Add(NegativeMarkersPath);
                }
                if (!MarkerLayer.Children.Contains(LowMarkersPath))
                {
                    MarkerLayer.Children.Add(LowMarkersPath);
                }
                if (!MarkerLayer.Children.Contains(HighMarkersPath))
                {
                    MarkerLayer.Children.Add(HighMarkersPath);
                }
                if (!MarkerLayer.Children.Contains(FirstMarkerPath))
                {
                    MarkerLayer.Children.Add(FirstMarkerPath);
                }
                if (!MarkerLayer.Children.Contains(LastMarkerPath))
                {
                    MarkerLayer.Children.Add(LastMarkerPath);
                }
                if (!RangeLayer.Children.Contains(RangePath))
                {
                    RangeLayer.Children.Add(RangePath);
                }
                if (!TrendlineLayer.Children.Contains(TrendLinePath))
                {
                    TrendlineLayer.Children.Add(TrendLinePath);
                }
            };
            this.InitializeContent();

        }


        private void InitializeContent()
        {
            this.Content = new Grid();
            this.RangeLayer = new Grid();
            this.SparkLayer = new Grid();
            this.MarkerLayer = new Canvas();
            this.TrendlineLayer = new Grid();
            


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            this.ToolTipContent = new ContentPresenter();
            this.ToolTipContent.Visibility = System.Windows.Visibility.Collapsed;
            this.ToolTipPopup = new Popup()
            {
               Child = this.ToolTipContent
            };


            if (SafeSetter.IsSafe)
            {
                ToolTipPopup.AllowsTransparency = true;

                try
                {
                    // [DN January 18, 2012 : 99043]
                    typeof(Popup).GetProperty("HitTestable", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(this.ToolTipPopup, false, null);
                }
                catch (SecurityException) { }
                catch (MissingMethodException) { }


            }
            this.ToolTipPopup.PlacementTarget = Application.Current != null ? Application.Current.MainWindow : null;
            this.ToolTipPopup.Placement = PlacementMode.Relative;






            this.Content.Children.Add(SparkLayer);
            this.Content.Children.Add(MarkerLayer);
            this.Content.Children.Add(TrendlineLayer);
            this.Content.Children.Add(RangeLayer);



        }
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ContentArea = (ContentPresenter)GetTemplateChild("ContentArea");
            if (this.ContentArea != null)
            {
                this.ContentArea.Content = this.Content;
            }
        }
        #endregion

        #region Properties
        private ServiceProvider _serviceProvider;
        internal ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                ServiceProvider oldValue = _serviceProvider;
                _serviceProvider = value;
                OnServiceProviderChanged(oldValue, _serviceProvider);
            }
        }

        private MessageChannel _interactionMessages;
        internal MessageChannel InteractionMessages
        {
            get { return _interactionMessages; }
            set { _interactionMessages = value; }
        }

        private MessageChannel _renderingMessages;
        internal MessageChannel RenderingMessages
        {
            get { return _renderingMessages; }
            set { _renderingMessages = value; }
        }

        internal SparklineConnector Connector { get; set; }

        private Dictionary<Type, MessageEventHandler> _messageHandlers = new Dictionary<Type, MessageEventHandler>();
        internal Dictionary<Type, MessageEventHandler> MessageHandlers
        {
            get { return _messageHandlers; }
            set { _messageHandlers = value; }
        }

        private ContentPresenter _contentArea = null;
        /// <summary>
        /// Gets or sets the place holder of the sparkline's main content.
        /// </summary>
        protected ContentPresenter ContentArea
        {
            get { return _contentArea; }
            set { _contentArea = value; }
        }

        private Grid _content;
        internal Grid Content
        {
            get { return _content; }
            set { _content = value; }
        }

        #region Sparkline layers
        private Grid _sparkLayer;
        internal Grid SparkLayer
        {
            get { return _sparkLayer; }
            set { _sparkLayer = value; }
        }

        private Grid _rangeLayer;
        internal Grid RangeLayer
        {
            get { return _rangeLayer; }
            set { _rangeLayer = value; }
        }

        private Grid _trendlineLayer;
        internal Grid TrendlineLayer
        {
            get { return _trendlineLayer; }
            set { _trendlineLayer = value; }
        }

        private Canvas _markerLayer;
        internal Canvas MarkerLayer
        {
            get { return _markerLayer; }
            set { _markerLayer = value; }
        }


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        #endregion

        private Path _sparkPath = new Path();
        internal Path SparkPath
        {
            get { return _sparkPath; }
            set { _sparkPath = value; }
        }

        private Path _negativeSparkPath = new Path();
        internal Path NegativeSparkPath
        {
            get { return _negativeSparkPath; }
            set { _negativeSparkPath = value; }
        }

        private Path _trendLinePath = new Path();
        internal Path TrendLinePath
        {
            get { return _trendLinePath; }
            set { _trendLinePath = value; }
        }

        private Path _rangePath = new Path();
        internal Path RangePath
        {
            get { return _rangePath; }
            set { _rangePath = value; }
        }

        private Path _markersPath = new Path();
        internal Path MarkersPath
        {
            get { return _markersPath; }
            set { _markersPath = value; }
        }

        private Path _negativeMarkersPath = new Path();
        internal Path NegativeMarkersPath
        {
            get { return _negativeMarkersPath; }
            set { _negativeMarkersPath = value; }
        }

        private Path _lowMarkersPath = new Path();
        internal Path LowMarkersPath
        {
            get { return _lowMarkersPath; }
            set { _lowMarkersPath = value; }
        }

        private Path _highMarkersPath = new Path();
        internal Path HighMarkersPath
        {
            get { return _highMarkersPath; }
            set { _highMarkersPath = value; }
        }

        private Path _firstMarkerPath = new Path();
        internal Path FirstMarkerPath
        {
            get { return _firstMarkerPath; }
            set { _firstMarkerPath = value; }
        }

        private Path _lastMarkerPath = new Path();
        internal Path LastMarkerPath
        {
            get { return _lastMarkerPath; }
            set { _lastMarkerPath = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            bool newSize = this.Connector == null || this.Connector.Controller == null  || finalSize.Width != this.Connector.Controller.ViewportWidth || finalSize.Height != this.Connector.Controller.ViewportHeight;
            if (this.InteractionMessages != null && newSize)
            {
                this.InteractionMessages.SendMessage(new ViewportChangedMessage()
                {
                    NewHeight = finalSize.Height,
                    NewWidth = finalSize.Width
                });
                
                
                Content.Width = finalSize.Width;
                Content.Height = finalSize.Height;
            }
            return finalSize;
        }
        private const double IDEAL_WIDTH = 100.0;
        private const double IDEAL_HEIGHT = 50.0;
        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. Classes can override this method to define their own Measure pass behavior.
        /// </summary>
        /// <param name="availableSize">The available size that this object can give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) can be specified as a value to indicate that the object will size to whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based on its calculations of the allocated sizes for child objects; or based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);
            availableSize.Width = Math.Min(availableSize.Width, XamSparklineView.IDEAL_WIDTH);
            availableSize.Height = Math.Min(availableSize.Height, XamSparklineView.IDEAL_HEIGHT);
            
            return availableSize;
        }

        private void SparklineView_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveMessage m = new MouseMoveMessage();
            m.Position = e.GetPosition(this);
            InteractionMessages.SendMessage(m);
        }

        private void SparklineView_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveMessage m = new MouseLeaveMessage();
            InteractionMessages.SendMessage(m);
        }

        private void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                RenderingMessages.DetachTarget(MessageReceived);
                InteractionMessages = null;
            }

            if (newValue != null)
            {
                RenderingMessages = (MessageChannel)newValue.GetService("RenderingMessages");
                InteractionMessages = (MessageChannel)newValue.GetService("InteractionMessages");
                RenderingMessages.AttachTarget(MessageReceived);
                StartInteractionChannel(InteractionMessages);
            }
        }

        private void StartInteractionChannel(MessageChannel messageChannel)
        {
            InteractionMessages = messageChannel;
            this.InvalidateArrange();
           
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }

        private void MessageReceived(Message m)
        {
            MessageEventHandler h;
            if (MessageHandlers.TryGetValue(m.GetType(), out h))
            {
                h.Invoke(m);
            }
        }

        private void ClearLayer(ClearMessage message)
        {
            switch (message.Layer)
            {
                case SparkLayerType.MarkerLayer:
                    MarkersPath.Data = null;
                    FirstMarkerPath.Data = null;
                    LastMarkerPath.Data = null;
                    HighMarkersPath.Data = null;
                    LowMarkersPath.Data = null;
                    NegativeMarkersPath.Data = null;
                    break;
                case SparkLayerType.RangeLayer:
                    RangePath.Data = null;
                    break;
                case SparkLayerType.SparkLayer:
                    SparkPath.Data = null;
                    NegativeSparkPath.Data = null;
                    break;
                case SparkLayerType.ToolTipLayer:
                    this.ToolTipContent.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case SparkLayerType.TrendLayer:
                    TrendLinePath.Data = null;
                    break;
            }
        }

        private void UpdateMarkers(MarkerMessage markerMessage)
        {
            UpdateMarkers(MarkersPath, markerMessage.MarkerPoints, markerMessage.MarkerSize);
            UpdateMarkers(NegativeMarkersPath, markerMessage.NegativeMarkerPoints, markerMessage.NegativeMarkerSize);
            UpdateMarkers(LowMarkersPath, markerMessage.LowPoints, markerMessage.LowMarkerSize);
            UpdateMarkers(HighMarkersPath, markerMessage.HighPoints, markerMessage.HighMarkerSize);
            UpdateMarkers(FirstMarkerPath, markerMessage.FirstPoint, markerMessage.FirstMarkerSize);
            UpdateMarkers(LastMarkerPath, markerMessage.LastPoint, markerMessage.LastMarkerSize);
        }

        private void UpdateMarkers(Path path, Point point, double size)
        {
            List<Point> points = new List<Point> { point };
            UpdateMarkers(path, points, size);
        }

        private void UpdateMarkers(Path path, List<Point> points, double size)
        {
            path.Data = CreateMarkers(points, size);

            if (Connector != null
                && Connector.Controller != null
                && Connector.Controller.FramePreparer != null
                && Connector.Controller.Model.DisplayType != SparklineDisplayType.WinLoss)
            {
                TranslateTransform transform = new TranslateTransform { X = Connector.Controller.FramePreparer.Offset };
                path.RenderTransform = transform;
                path.InvalidateMeasure();
            }
            else
            {
                path.RenderTransform = null;
            }
        }

        private GeometryGroup CreateMarkers(List<Point> markers, double markerSize)
        {
            GeometryGroup geometry = new GeometryGroup { FillRule = FillRule.Nonzero };

            double viewportWidth = 0.0;
            double viewportHeight = 0.0;
            if (this.Connector != null && this.Connector.Controller != null)
            {
                viewportWidth = this.Connector.Controller.ViewportWidth;
                viewportHeight = this.Connector.Controller.ViewportHeight;
            }
            foreach (var marker in markers)
            {
                if (marker.X < 0 || marker.Y < 0 || marker.X > viewportWidth || marker.Y > viewportHeight)
                {
                    // marker is outside the view, don't render it.
                    continue;
                }
                EllipseGeometry e = new EllipseGeometry
                {
                    RadiusX = markerSize,
                    RadiusY = markerSize,
                    Center = marker
                };

                geometry.Children.Add(e);
            }

            return geometry;
        }

        private void UpdatePolygon(PolygonMessage pm)
        {
            PathGeometry geometry = new PathGeometry();

            foreach (var pointList in pm.Points)
            {
                int numPoints = pointList.Length;
                if (numPoints == 0)
                {
                    continue;
                }
                bool closed = this.Connector != null && this.Connector.Model != null && this.Connector.Model.DisplayType == SparklineDisplayType.Area;
                PathFigure figure = new PathFigure
                {
                    IsClosed = closed,
                    IsFilled = closed,
                    StartPoint = pointList[0]
                };
                
                for (int i = 1; i < numPoints; i++)
                {
                    LineSegment line = new LineSegment
                    {
                        Point = pointList[i]
                    };

                    figure.Segments.Add(line);
                }

                geometry.Figures.Add(figure);
            }
            SparkPath.Data = geometry;
        }

        private GeometryGroup CreateColumns(List<Point> columns, RenderingMessage message)
        {
            WinLossColumnMessage winlossMessage = message as WinLossColumnMessage;
            ColumnMessage columnMessage = message as ColumnMessage;
            GeometryGroup geometry = new GeometryGroup();

            double offset = winlossMessage != null ? winlossMessage.Offset : columnMessage.Offset;
            double crossing = winlossMessage != null ? winlossMessage.Crossing : columnMessage.Crossing;
            double a = offset * 0.1;

            foreach (var column in columns)
            {
                double width = offset * 2 - a * 2;
                double height = Math.Abs(crossing - column.Y);
                double x = column.X + a;
                double y = column.Y > crossing ? crossing : column.Y;

                RectangleGeometry r = new RectangleGeometry
                {
                    Rect = new Rect(x, y, width, height)
                };

                geometry.Children.Add(r);
            }

            return geometry;
        }

        private void UpdateColumns(ColumnMessage cm)
        {
            UpdateColumns(SparkPath, cm.Columns, cm);
            UpdateColumns(NegativeSparkPath, cm.NegativeColumns, cm);
        }

        private void UpdateColumns(Path path, Point point, RenderingMessage message)
        {
            List<Point> points = new List<Point> { point };
            UpdateColumns(path, points, message);
        }

        private void UpdateColumns(Path path, List<Point> points, RenderingMessage message)
        {
            path.Data = CreateColumns(points, message);
        }

        private void UpdateWinLossColumns(WinLossColumnMessage message)
        {
            UpdateColumns(SparkPath, message.Columns, message);
            UpdateColumns(NegativeSparkPath, message.NegativeColumns, message);

            UpdateColumns(MarkersPath, message.Columns, message);
            UpdateColumns(NegativeMarkersPath, message.NegativeColumns, message);
            UpdateColumns(LowMarkersPath, message.LowColumns, message);
            UpdateColumns(HighMarkersPath, message.HighColumns, message);
            UpdateColumns(FirstMarkerPath, message.FirstColumn, message);
            UpdateColumns(LastMarkerPath, message.LastColumn, message);

            MarkersPath.RenderTransform = null;
            NegativeMarkersPath.RenderTransform = null;
            FirstMarkerPath.RenderTransform = null;
            LastMarkerPath.RenderTransform = null;
            HighMarkersPath.RenderTransform = null;
            LowMarkersPath.RenderTransform = null;
        }

        private void UpdateRange(NormalRangeMessage message)
        {
            RangePath.Data = new RectangleGeometry
            {
                Rect = new Rect(message.X, message.Y, message.Width, message.Height)
            };
        }

        private void UpdateTrendLine(TrendLineMessage message)
        {
            if (message.Points.Length == 0)
                return;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure
            {
                IsClosed = false,
                IsFilled = false,
                StartPoint = message.Points[0]
            };

            int numPoints = message.Points.Length;

            for (int i = 1; i < numPoints; i++)
            {
                LineSegment line = new LineSegment
                {
                    Point = message.Points[i]
                };

                figure.Segments.Add(line);
            }

            geometry.Figures.Add(figure);

            TrendLinePath.Data = geometry;
        }

        private void UpdateToolTipTemplate(ToolTipTemplateMessage message)
        {
            this.ToolTipContent.ContentTemplate = message.Template as DataTemplate;

        }
        private ContentPresenter ToolTipContent { get; set; }
        private void UpdateToolTip(ToolTipMessage tooltipMessage)
        {
            if (tooltipMessage.Context == null)
                return;
            
            if (Connector.Model.ToolTipVisibility != System.Windows.Visibility.Visible)
            {
                this.ToolTipPopup.IsOpen = false;
                this.ToolTipContent.Visibility = Connector.Model.ToolTipVisibility;
                return;
            }

            this.ToolTipPopup.IsOpen = true;
            this.ToolTipContent.Visibility = System.Windows.Visibility.Visible;
            this.ToolTipContent.DataContext = tooltipMessage.Context;

            if (this.ToolTipContent.ContentTemplate == null)
            {
                this.ToolTipContent.Content = Connector.Model.ToolTip.ToString();
            }
            else
            {
                this.ToolTipContent.Content = this.ToolTipContent.DataContext;
            }

            this.ToolTipContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));




            double tooltipRight, tooltipBottom;

            UIElement rootVisual;



            rootVisual = Application.Current != null ? Application.Current.MainWindow : null;


            double xOffset, yOffset;
            // adjust offset so that tooltip won't overflow the application window bounds
            if (rootVisual != null)
            {
                double rootWidth, rootHeight;

                Window rootWindow = rootVisual as Window;
                if (rootWindow != null && rootWindow.Content is UIElement)
                {
                    UIElement content = (UIElement)rootWindow.Content;
                    rootWidth = content.RenderSize.Width;
                    rootHeight = content.RenderSize.Height;
                }
                else

                {
                    rootWidth = rootVisual.RenderSize.Width;
                    rootHeight = rootVisual.RenderSize.Height;
                }





                Point offset = this.TransformToVisual(rootVisual).Transform(new Point(0.0, 0.0));
                xOffset = offset.X + tooltipMessage.XOffset;
                yOffset = offset.Y + tooltipMessage.YOffset;
                tooltipRight = xOffset + this.ToolTipContent.DesiredSize.Width;
                tooltipBottom = yOffset + this.ToolTipContent.DesiredSize.Height;
                xOffset -= Math.Max(0.0, tooltipRight - rootWidth);
                yOffset -= Math.Max(0.0, tooltipBottom - rootHeight);

                
            }
            else
            {
                tooltipRight = tooltipMessage.XOffset + this.ToolTipContent.DesiredSize.Width;
                tooltipBottom = tooltipMessage.YOffset + this.ToolTipContent.DesiredSize.Height;
                // couldn't find root visual, offset to control/viewport bounds
                xOffset = tooltipMessage.XOffset - Math.Max(0.0, tooltipRight - this.RenderSize.Width);
                yOffset = tooltipMessage.YOffset - Math.Max(0.0, tooltipBottom - this.RenderSize.Height);
            }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            this.ToolTipPopup.HorizontalOffset = xOffset;
            this.ToolTipPopup.VerticalOffset = yOffset;


        }

        #endregion
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