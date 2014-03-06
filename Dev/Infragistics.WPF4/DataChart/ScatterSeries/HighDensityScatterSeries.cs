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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// A chart series where a high volume of scatter points can be displayed.
    /// </summary>
    public class HighDensityScatterSeries
        : Series
    {
        /// <summary>
        /// Constructs a HighDensityScatterSeries.
        /// </summary>
        public HighDensityScatterSeries()
        {
            DefaultStyleKey = typeof(HighDensityScatterSeries);
        }

        private Image _image = new Image();
        private WriteableBitmap _bitmap;
        private Path _hoverPath = new Path() 
        { 
            Stroke = new SolidColorBrush(Colors.Green), 
            StrokeThickness = 1, 
            Data = new PathGeometry() 
        };

        /// <summary>
        /// Called when the template of the control has been provided.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            RootCanvas.Children.Add(_image);
            RootCanvas.Children.Add(_hoverPath);
        }

        #region XAxis Dependency Property
        /// <summary>
        /// Gets or sets the effective x-axis for the current object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericXAxis XAxis
        {
            get
            {
                return (NumericXAxis)GetValue(XAxisProperty);
            }
            set
            {
                SetValue(XAxisProperty, value);
            }
        }

        internal const string XAxisPropertyName = "XAxis";

        /// <summary>
        /// Identifies the XAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(XAxisPropertyName, typeof(NumericXAxis), typeof(HighDensityScatterSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HighDensityScatterSeries).RaisePropertyChanged(XAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region YAxis Depedency Property
        /// <summary>
        /// Gets or sets the effective y-axis for the current object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public NumericYAxis YAxis
        {
            get
            {
                return (NumericYAxis)GetValue(YAxisProperty);
            }
            set
            {
                SetValue(YAxisProperty, value);
            }
        }

        internal const string YAxisPropertyName = "YAxis";

        /// <summary>
        /// Identifies the YAxis dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(YAxisPropertyName, typeof(NumericYAxis), typeof(HighDensityScatterSeries),
            new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                (sender as HighDensityScatterSeries).RaisePropertyChanged(YAxisPropertyName, e.OldValue, e.NewValue);
            }));
        #endregion

        #region XMemberPath Dependency Property and XColumn Property
        /// <summary>
        /// Gets or sets the x value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string XMemberPath
        {
            get
            {
                return (string)GetValue(XMemberPathProperty);
            }
            set
            {
                SetValue(XMemberPathProperty, value);
            }
        }

        internal const string XMemberPathPropertyName = "XMemberPath";

        /// <summary>
        /// Identifies the XMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty XMemberPathProperty = DependencyProperty.Register(XMemberPathPropertyName, typeof(string), typeof(HighDensityScatterSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(XMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The IFastItemColumn containing X values.
        /// </summary>
        protected IFastItemColumn<double> XColumn
        {
            get { return xColumn; }
            private set
            {
                if (xColumn != value)
                {
                    IFastItemColumn<double> oldXColumn = XColumn;

                    xColumn = value;
                    RaisePropertyChanged(XColumnPropertyName, oldXColumn, XColumn);
                }
            }
        }
        private IFastItemColumn<double> xColumn;
        internal const string XColumnPropertyName = "XColumn";
        #endregion

        #region YMemberPath Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the y value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string YMemberPath
        {
            get
            {
                return (string)GetValue(YMemberPathProperty);
            }
            set
            {
                SetValue(YMemberPathProperty, value);
            }
        }

        internal const string YMemberPathPropertyName = "YMemberPath";

        /// <summary>
        /// Identifies the YMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty YMemberPathProperty = DependencyProperty.Register(YMemberPathPropertyName, typeof(string), typeof(HighDensityScatterSeries), new PropertyMetadata(null, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(YMemberPathPropertyName, e.OldValue, e.NewValue);
        }));
        /// <summary>
        /// The IFastItemColumn containing Y values.
        /// </summary>
        protected IFastItemColumn<double> YColumn
        {
            get { return yColumn; }
            private set
            {
                if (yColumn != value)
                {
                    IFastItemColumn<double> oldYColumn = YColumn;

                    yColumn = value;
                    RaisePropertyChanged(YColumnPropertyName, oldYColumn, YColumn);
                }
            }
        }
        private IFastItemColumn<double> yColumn;
        internal const string YColumnPropertyName = "YColumn";
        #endregion

        #region UseBruteForce Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the whether to use use brute force mode.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool UseBruteForce
        {
            get
            {
                return (bool)GetValue(UseBruteForceProperty);
            }
            set
            {
                SetValue(UseBruteForceProperty, value);
            }
        }

        internal const string UseBruteForcePropertyName = "UseBruteForce";

        /// <summary>
        /// Identifies the UseBruteForce dependency property.
        /// </summary>
        public static readonly DependencyProperty UseBruteForceProperty = DependencyProperty.Register(UseBruteForcePropertyName, typeof(bool), typeof(HighDensityScatterSeries), new PropertyMetadata(false, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(UseBruteForcePropertyName, e.OldValue, e.NewValue);
        }));

        #endregion


        #region ProgressiveLoad Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the whether to progressively load the data into the chart.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool ProgressiveLoad
        {
            get
            {
                return (bool)GetValue(ProgressiveLoadProperty);
            }
            set
            {
                SetValue(ProgressiveLoadProperty, value);
            }
        }

        internal const string ProgressiveLoadPropertyName = "ProgressiveLoad";

        /// <summary>
        /// Identifies the ProgressiveLoad dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressiveLoadProperty = DependencyProperty.Register(ProgressiveLoadPropertyName, typeof(bool), typeof(HighDensityScatterSeries), new PropertyMetadata(true, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(ProgressiveLoadPropertyName, e.OldValue, e.NewValue);
        }));

        #endregion

        #region MouseOverEnabled Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the whether the chart reacts to mouse move events.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool MouseOverEnabled
        {
            get
            {
                return (bool)GetValue(MouseOverEnabledProperty);
            }
            set
            {
                SetValue(MouseOverEnabledProperty, value);
            }
        }

        internal const string MouseOverEnabledPropertyName = "MouseOverEnabled";

        /// <summary>
        /// Identifies the MouseOverEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseOverEnabledProperty = DependencyProperty.Register(MouseOverEnabledPropertyName, typeof(bool), typeof(HighDensityScatterSeries), new PropertyMetadata(false, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(MouseOverEnabledPropertyName, e.OldValue, e.NewValue);
        }));

        #endregion

        #region SquareCutoffStyle Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the whether to use squares when halting a render traversal rather than the shape of the coalesced area.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool UseSquareCutoffStyle
        {
            get
            {
                return (bool)GetValue(UseSquareCutoffStyleProperty);
            }
            set
            {
                SetValue(UseSquareCutoffStyleProperty, value);
            }
        }

        internal const string UseSquareCutoffStylePropertyName = "UseSquareCutoffStyle";

        /// <summary>
        /// Identifies the SquareCutoffStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSquareCutoffStyleProperty = DependencyProperty.Register(UseSquareCutoffStylePropertyName, typeof(bool), typeof(HighDensityScatterSeries), new PropertyMetadata(false, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(UseSquareCutoffStylePropertyName, e.OldValue, e.NewValue);
        }));

        #endregion



#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


        #region HeatMinimum Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the density value that maps to the minimum heat color.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double HeatMinimum
        {
            get
            {
                return (double)GetValue(HeatMinimumProperty);
            }
            set
            {
                SetValue(HeatMinimumProperty, value);
            }
        }

        internal const string HeatMinimumPropertyName = "HeatMinimum";

        /// <summary>
        /// Identifies the HeatMinimum dependency property.
        /// </summary>
        public static readonly DependencyProperty HeatMinimumProperty = DependencyProperty.Register(HeatMinimumPropertyName, typeof(double), typeof(HighDensityScatterSeries), new PropertyMetadata(0.0, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(HeatMinimumPropertyName, e.OldValue, e.NewValue);
        }));

        #endregion

        #region HeatMaximum Dependency Property and YColumn Property
        /// <summary>
        /// Gets or sets the value that maps to the maximum heat color.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double HeatMaximum
        {
            get
            {
                return (double)GetValue(HeatMaximumProperty);
            }
            set
            {
                SetValue(HeatMaximumProperty, value);
            }
        }

        internal const string HeatMaximumPropertyName = "HeatMaximum";

        /// <summary>
        /// Identifies the HeatMaximum dependency property.
        /// </summary>
        public static readonly DependencyProperty HeatMaximumProperty = DependencyProperty.Register(HeatMaximumPropertyName, typeof(double), typeof(HighDensityScatterSeries), new PropertyMetadata(50.0, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as HighDensityScatterSeries).RaisePropertyChanged(HeatMaximumPropertyName, e.OldValue, e.NewValue);
        }));

        #endregion

        /// <summary>
        /// Called when the value of a property has been updated.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="propertyName">The name of the property that has been updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);

            switch (propertyName)
            {
                case FastItemsSourcePropertyName:
                    if (_tree != null)
                    {
                        _tree.ProgressiveThunkCompleted -= _tree_ProgressiveThunkCompleted;
                    }
                    _tree = null;
                    if (oldValue as FastItemsSource != null)
                    {
                        (oldValue as FastItemsSource).DeregisterColumn(XColumn);
                        (oldValue as FastItemsSource).DeregisterColumn(YColumn);
                        XColumn = null;
                        YColumn = null;
                    }

                    if (newValue as FastItemsSource != null)
                    {
                        XColumn = (newValue as FastItemsSource).RegisterColumn(XMemberPath);
                        YColumn = (newValue as FastItemsSource).RegisterColumn(YMemberPath);
                    }

                    if ((YAxis != null && !YAxis.UpdateRange()) ||
                        (XAxis != null && !XAxis.UpdateRange()))
                    {
                        RenderSeries(false);
                    }

                    break;

                case XAxisPropertyName:
                    _xAxis = XAxis;
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }

                    if ((XAxis != null && !XAxis.UpdateRange()) ||
                        (newValue == null && oldValue != null))
                    {
                        RenderSeries(false);
                    }
                    break;

                case YAxisPropertyName:
                    _yAxis = YAxis;
                    if (oldValue != null)
                    {
                        (oldValue as Axis).DeregisterSeries(this);
                    }

                    if (newValue != null)
                    {
                        (newValue as Axis).RegisterSeries(this);
                    }
                    if ((YAxis != null && !YAxis.UpdateRange()) ||
                        (newValue == null && oldValue != null))
                    {
                        RenderSeries(false);
                    }
                    break;

                case MouseOverEnabledPropertyName:
                    _mouseOverEnabled = MouseOverEnabled;
                    RenderSeries(false);
                    break;

                #region X Mapping and Column
                case XMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(XColumn);
                        XColumn = FastItemsSource.RegisterColumn(XMemberPath);
                    }

                    break;

                case XColumnPropertyName:
                    if (XAxis != null && !XAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;
                #endregion

                #region Y Mapping and Column
                case YMemberPathPropertyName:
                    if (FastItemsSource != null)
                    {
                        FastItemsSource.DeregisterColumn(YColumn);
                        YColumn = FastItemsSource.RegisterColumn(YMemberPath);
                    }

                    break;

                case YColumnPropertyName:
                    if (YAxis != null && !YAxis.UpdateRange())
                    {
                        RenderSeries(false);
                    }
                    break;

                case UseBruteForcePropertyName:
                    if (_tree != null)
                    {
                        _tree.ProgressiveThunkCompleted -= _tree_ProgressiveThunkCompleted;
                    }
                    _tree = null;
                    RenderSeries(false);
                    break;

                case HeatMinimumPropertyName:
                    RenderSeries(false);
                    break;

                case HeatMaximumPropertyName:
                    RenderSeries(false);
                    break;

                case UseSquareCutoffStylePropertyName:
                    RenderSeries(false);
                    break;






                #endregion
            }
        }

        private KDTree2D _tree = null;
        private ScalerParams _scalerParamsX;
        private ScalerParams _scalerParamsY;
        private NumericXAxis _xAxis;
        private NumericYAxis _yAxis;
        private bool _mouseOverEnabled;

        /// <summary>
        /// Called when the series needs to get rendered.
        /// </summary>
        /// <param name="animate">Whether or not the change to the series should be animated, if possible.</param>
        protected internal override void RenderSeriesOverride(bool animate)
        {
            base.RenderSeriesOverride(animate);

            if (YAxis == null ||
                XAxis == null ||
                YColumn == null ||
                XColumn == null ||
                YColumn.Count < 1 ||
                XColumn.Count < 1 ||
                YColumn.Count != XColumn.Count ||
                Viewport.IsEmpty)
            {
                return;
            }

            if (_tree == null && !UseBruteForce)
            {
                PointData[] points = new PointData[XColumn.Count];
                for (int i = 0; i < XColumn.Count; i++)
                {
                    points[i] =
                        new PointData()
                        {
                            X = XColumn[i],
                            Y = YColumn[i],
                            Index = i
                        };
                }

                if (ProgressiveLoad)
                {
                    _currentLevel = 1;
                    _expectedLevels = (int)Math.Log(points.Length, 2) + 3;

                    if (ProgressiveLoadStatusChanged != null)
                    {
                        ProgressiveLoadStatusChanged(this,
                            new ProgressiveLoadStatusEventArgs(
                                (int)((_currentLevel / (double)_expectedLevels) * 100)));
                    }

                    _tree = KDTree2D.GetProgressive(points, 1);
                    _tree.ProgressiveThunkCompleted += _tree_ProgressiveThunkCompleted;
                    if (!_tree.ProgressiveStep())
                    {
                        _tree.ProgressiveThunkCompleted -= _tree_ProgressiveThunkCompleted;
                    }
                }
                else
                {
                    DateTime before = DateTime.Now;
                    _tree = new KDTree2D(points, 1);
                    DateTime after = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine("kdtree generation: " + (after - before).TotalMilliseconds + "ms");
                }

            }

            if (ProgressiveLoad && !UseBruteForce)
            {
                LockedRender();
            }
            else
            {
                RenderBitmap();
            }
        }

        int _resolution;
        int _expectedLevels = 0;
        int _currentLevel = 0;
        bool _squareCutoffStyle = false;

        private void AssertMouseOver()
        {
            if (_mouseOverEnabled)
            {
                if (_itemIndexes == null ||
                    _itemIndexes.Length != (_imageWidth * _imageHeight))
                {
                    _itemIndexes = new int[_imageWidth * _imageHeight];
                }
                else
                {
                    Array.Clear(_itemIndexes, 0, _itemIndexes.Length);
                }
            }
        }

        private void RenderBitmap()
        {
            _scalerParamsX = new ScalerParams(
                SeriesViewer.ActualWindowRect, Viewport, _xAxis.IsInverted);
            _scalerParamsX.EffectiveViewportRect = this.SeriesViewer.EffectiveViewport;
            _scalerParamsY = new ScalerParams(
                SeriesViewer.ActualWindowRect, Viewport, _yAxis.IsInverted);
            _scalerParamsY.EffectiveViewportRect = this.SeriesViewer.EffectiveViewport;

            AssertBitmap();
            AssertMouseOver();

            _resolution = (int)Math.Round(Resolution);
            _squareCutoffStyle = UseSquareCutoffStyle;



            Array.Clear(_pixels, 0, _pixels.Length);
            if (UseBruteForce)
            {
                BruteForceRender();
            }
            else
            {
                UseTree();
            }



            _bitmap.WritePixels(
                new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight), 
                _pixels, 
                4 * _bitmap.PixelWidth, 
                0);

        }

        private void LockedRender()
        {
            lock (_tree.SyncLock)
            {
                RenderBitmap();
            }
        }

        /// <summary>
        /// Raised when the progressive loading state of the series has changed.
        /// </summary>
        public event EventHandler<ProgressiveLoadStatusEventArgs> ProgressiveLoadStatusChanged;

        void _tree_ProgressiveThunkCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                System.Diagnostics.Debug.WriteLine("thunk");
                if (_currentLevel < _expectedLevels - 1)
                {
                    _currentLevel++;
                }
                if (ProgressiveLoadStatusChanged != null)
                {
                    ProgressiveLoadStatusChanged(this,
                        new ProgressiveLoadStatusEventArgs(
                            (int)((_currentLevel / (double)_expectedLevels) * 100)));
                }

                LockedRender();
                if (!_tree.ProgressiveStep())
                {
                    _tree.ProgressiveThunkCompleted -= _tree_ProgressiveThunkCompleted;
                    if (ProgressiveLoadStatusChanged != null)
                    {
                        ProgressiveLoadStatusChanged(this,
                            new ProgressiveLoadStatusEventArgs(
                                100));
                    }
                }
            }));
        }

        private void BruteForceRender()
        {
            double[] xValues = new double[XColumn.Count];
            XColumn.CopyTo(xValues,0);
            XAxis.GetScaledValueList(xValues, _scalerParamsX);
            double[] yValues = new double[YColumn.Count];
            YColumn.CopyTo(yValues, 0);
            YAxis.GetScaledValueList(yValues, _scalerParamsY);

            for (var i = 0; i < xValues.Length; i++)
            {
                int posX = (int)xValues[i];
                int posY = (int)yValues[i];
                if (posX < 0 || posX >= _imageWidth || posY < 0 || posY >= _imageHeight)
                {
                    continue;
                }

                var pos = (posY * _imageWidth) + posX;
                _pixels[pos] = 255 << 24 | 0 << 16 | 0 << 8 | 0;
                _rendered++;
                if (_mouseOverEnabled)
                {
                    _itemIndexes[pos] = i + 1;
                }
            }
        }

        private List<KDTreeNode2D> _nodes;

        private void UseTree()
        {
            if (_nodes == null)
            {
                _nodes = new List<KDTreeNode2D>(
                    (int)Math.Round(Viewport.Width * Viewport.Height));
            }
            else
            {
                _nodes.Clear();
            }

            double minX = XAxis.GetUnscaledValue(Viewport.Left, _scalerParamsX);
            double maxX = XAxis.GetUnscaledValue(Viewport.Right, _scalerParamsX);
            double minY = YAxis.GetUnscaledValue(Viewport.Bottom, _scalerParamsY);
            double maxY = YAxis.GetUnscaledValue(Viewport.Top, _scalerParamsY);

            double onePixelX =
                Math.Abs(
                XAxis.GetUnscaledValue(Viewport.Left + Resolution, _scalerParamsX) -
                XAxis.GetUnscaledValue(Viewport.Left, _scalerParamsX));
            double onePixelY =
                Math.Abs(
                YAxis.GetUnscaledValue(Viewport.Top + Resolution, _scalerParamsY) -
                YAxis.GetUnscaledValue(Viewport.Top, _scalerParamsY));
            double pizelSize = Math.Min(onePixelX, onePixelY);
            
            SearchArgs args = new SearchArgs()
            {
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
                PixelSizeX = onePixelX,
                PixelSizeY = onePixelY



                ,MaxRenderDepth = int.MaxValue        

            };

            _tree.GetVisible(
                _nodes,
                args,
                XAxis.ActualMinimumValue,
                XAxis.ActualMaximumValue,
                YAxis.ActualMinimumValue,
                YAxis.ActualMaximumValue);
            System.Diagnostics.Debug.WriteLine("nodes returned: " + _nodes.Count);

            KDTreeNode2D current;

            _heatMinimum = HeatMinimum;
            _heatMaximum = HeatMaximum;

            //System.Diagnostics.Debug.WriteLine("maxNumber: " + maxNumber);
            _rendered = 0;
            for (var i = 0; i < _nodes.Count; i++)
            {
                current = _nodes[i];
                RenderNode(current);
            }
            System.Diagnostics.Debug.WriteLine("rendered: " + _rendered);
        }

        private double _heatMinimum;
        private double _heatMaximum;

        private void RenderNode(KDTreeNode2D current)
        {
            if (current.Unfinished)
            {
                return;
            }

            var pixelCutoff = current.SearchData != null && current.SearchData.IsCutoff;

            var otherCount = current.OtherPoints == null ? 0 : current.OtherPoints.Length;
            var val = (double)(current.DescendantCount - otherCount);
            if (val > 0)
            {
                val = (val - _heatMinimum) / (_heatMaximum - _heatMinimum);
                if (val < 0)
                {
                    val = 0;
                }
                if (val > 1)
                {
                    val = 1;
                }
            }
            else
            {
                val = 0;
            }

            //System.Diagnostics.Debug.Assert(current.Median != null);
            RenderPointData(current.Median, val, pixelCutoff, current.DescendantCount, current.SearchData);
            if (otherCount > 0 && !pixelCutoff)
            {
                PointData other;
                for (var i = 0; i < otherCount; i++)
                {
                    other = current.OtherPoints[i];
                    RenderPointData(other, 0, false, current.DescendantCount, current.SearchData);
                }
            }

            current.SearchData = null;
        }

        private void RenderPointData(PointData pointData, double p, bool isCutoff, int descendants, SearchData searchData)
        {
            int color = GetColorFromValue(p);
            var index = pointData.Index;

            if (isCutoff)
            {
                if (_squareCutoffStyle)
                {
                    var posX = (int)_xAxis.GetScaledValue(pointData.X, _scalerParamsX);
                    var posY = (int)_yAxis.GetScaledValue(pointData.Y, _scalerParamsY);
                    int mid = _resolution / 2;
                    int right = _resolution - mid;
                    int left = _resolution - right;

                    double area = _resolution * _resolution;
                    double alpha = descendants / area;
                    int alphaColor = GetAlphaColorFromValue(p, alpha);

                    for (int i = posX - left; i < posX + right; i++)
                    {
                        for (int j = posY - left; j < posY + right; j++)
                        {
                            RenderPixel(index, i, j, alphaColor, p);
                        }
                    }

                }
                else
                {
                    int minX = (int)_xAxis.GetScaledValue(searchData.MinX, _scalerParamsX);
                    int maxX = (int)_xAxis.GetScaledValue(searchData.MaxX, _scalerParamsX);
                    int maxY = (int)_yAxis.GetScaledValue(searchData.MinY, _scalerParamsY);
                    int minY = (int)_yAxis.GetScaledValue(searchData.MaxY, _scalerParamsY);
                    double area = (maxX - minX + 1) * (maxY - minY + 1);

                    double alpha = descendants / area;
                    if (alpha > 1.0)
                    {
                        alpha = 1.0;
                    }
                    if (alpha < .2)
                    {
                        alpha = .2;
                    }

                    int alphaColor = GetAlphaColorFromValue(p, alpha);

                    for (int i = minX; i <= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            RenderPixel(index, i, j, alphaColor, p);
                        }
                    }
                }
            }
            else
            {
                var posX = (int)_xAxis.GetScaledValue(pointData.X, _scalerParamsX);
                var posY = (int)_yAxis.GetScaledValue(pointData.Y, _scalerParamsY);
                RenderPixel(index, posX, posY, color, p);
            }
        }

        private int GetAlphaColorFromValue(double p, double alpha)
        {
            return ((int)(255.0 * alpha)) << 24 | (int)(p * alpha * 255.0) << 16 | 0 << 8 | 0;
        }

        private int GetColorFromValue(double p)
        {
            return 255 << 24 | (int)(p * 255.0) << 16 | 0 << 8 | 0;
        }

        private double GetValueFromColor(int color)
        {
            return (color >> 24 & 0xFF) / 255.0;
        }

        private void RenderPixel(int index, int posX, int posY, int color, double p)
        {
            if (posX < 0 || posX >= _imageWidth || posY < 0 || posY >= _imageHeight)
            {
                return;
            }

            var pos = (posY * _imageWidth) + posX;

            if (GetValueFromColor(_pixels[pos]) <= p)
            {
                _pixels[pos] = color;
            }
            if (_mouseOverEnabled)
            {
                _itemIndexes[pos] = index + 1;
            }

            _rendered++;
        }

        private int _imageWidth;
        private int _imageHeight;
        private int[] _pixels;
        private int _rendered = 0;


        private void AssertBitmap()
        {
            if (_bitmap == null ||
                _bitmap.PixelWidth != (int)Viewport.Width ||
                _bitmap.PixelHeight != (int)Viewport.Height)
            {





                _bitmap = new WriteableBitmap(
                    (int)Viewport.Width,
                    (int)Viewport.Height,
                    96.0,
                    96.0,
                    PixelFormats.Pbgra32, null);
                _pixels = new int[_bitmap.PixelHeight * _bitmap.PixelWidth];

                _image.Source = _bitmap;
                _imageWidth = _bitmap.PixelWidth;
                _imageHeight = _bitmap.PixelHeight;
            }
        }

        /// <summary>
        /// Called to create the view for the series..
        /// </summary>
        /// <returns>The crated view.</returns>
        protected override SeriesView CreateView()
        {
            return new HighDensityScatterSeriesView(this);
        }

        /// <summary>
        /// Called when the view is created.
        /// </summary>
        /// <param name="view">The created view.</param>
        protected internal override void OnViewCreated(SeriesView view)
        {
            base.OnViewCreated(view);

            SuperView = (HighDensityScatterSeriesView)view;
        }

        /// <summary>
        /// The view of the series.
        /// </summary>
        protected HighDensityScatterSeriesView SuperView { get; set; }

        /// <summary>
        /// Gets the range to use for the axes for the series.
        /// </summary>
        /// <param name="axis">The axis for which to return the range.</param>
        /// <returns>The range for the specified axis.</returns>
        protected internal override AxisRange GetRange(Axis axis)
        {
            if (axis != null && axis == XAxis && XColumn != null)
            {
                return new AxisRange(XColumn.Minimum, XColumn.Maximum);
            }

            if (axis != null && axis == YAxis && YColumn != null)
            {
                return new AxisRange(YColumn.Minimum, YColumn.Maximum);
            }

            return null;
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the viewport changing.
        /// </summary>
        /// <param name="oldViewportRect">The old viewport rectangle.</param>
        /// <param name="newViewportRect">The new viewport rectangle.</param>
        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            RenderSeries(false);
        }

        /// <summary>
        /// Overridden in derived classes when they want to respond to the chart's window changing.
        /// </summary>
        /// <param name="oldWindowRect">The old window rectangle of the chart.</param>
        /// <param name="newWindowRect">The new window rectangle of the chart.</param>
        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            RenderSeries(false);
        }

        private int[] _itemIndexes = null;
        /// <summary>
        /// Gets the item that is the best match for the specified world coordinates.
        /// </summary>
        /// <param name="world">The world coordinates to use.</param>
        /// <returns>The item that is the best match.</returns>
        protected override object GetItem(Point world)
        {
            if (!_mouseOverEnabled 
                || _itemIndexes == null 
                || _itemIndexes.Length != (_imageWidth * _imageHeight))
            {
                return null;
            }

            Rect windowRect = SeriesViewer.WindowRect;
            double windowX = (world.X - windowRect.Left) / windowRect.Width;
            double windowY = (world.Y - windowRect.Top) / windowRect.Height;

            int pixelX = (int)(Viewport.Width * windowX);
            int pixelY = (int)(Viewport.Height * windowY);

            int index = (_imageWidth * pixelY) + pixelX;
            if (index < 0 || index > _itemIndexes.Length - 1)
            {
                return null;
            }
            int itemIndex = _itemIndexes[index] - 1;
            if (itemIndex < 0 || itemIndex > FastItemsSource.Count)
            {
                return null;
            }
            return FastItemsSource[itemIndex];
        }

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    base.OnMouseMove(e);

        //    var pos = e.GetPosition(this);
        //    //System.Diagnostics.Debug.WriteLine("mouse: " + pos);
        //    //if (MouseOverEnabled)
        //    //{
        //    //    HandleMouseMove(pos);
        //    //}
        //}

        //private bool _mousePending = false;
        //private DateTime _lastMouseMove = DateTime.MinValue;
        //private bool _mouseWaiting = false;
        //private const int MouseMoveThrottle = 500;
        //private DispatcherTimer _mouseMoveTimer;
        //private Point _currMousePos;


        //private void HandleMouseMove(Point pos)
        //{
        //    _currMousePos = pos;
        //    if (_mouseMoveTimer == null)
        //    {
        //        _mouseMoveTimer = new DispatcherTimer()
        //        {
        //            Interval = TimeSpan.FromMilliseconds(MouseMoveThrottle),
        //        };
        //        _mouseMoveTimer.Tick += new EventHandler(_mouseMoveTimer_Tick);
        //    }

        //    if (_tree != null)
        //    {
        //        if (_mousePending)
        //        {
        //            return;
        //        }
        //        if (_mouseWaiting)
        //        {
        //            return;
        //        }
        //        if ((DateTime.Now - _lastMouseMove) < TimeSpan.FromMilliseconds(MouseMoveThrottle))
        //        {
        //            _mouseWaiting = true;
        //            _mouseMoveTimer.Start();
        //            return;
        //        }

        //        _mousePending = true;
        //        Thread t = new Thread(ProcessMouseMove);

        //        var x = _xAxis.GetUnscaledValue(_currMousePos.X, _scalerParamsX);
        //        var y = _yAxis.GetUnscaledValue(_currMousePos.Y, _scalerParamsY);
        //        t.Start(new MouseMoveThunk()
        //        {
        //            DesiredNeighborCount = DesiredNeighborsCount,
        //            Position = _currMousePos,
        //            AxisPosition = new Point(x, y)
        //        });
        //    }
        //}

        //void _mouseMoveTimer_Tick(object sender, EventArgs e)
        //{
        //    if (_mouseWaiting)
        //    {
        //        _mouseWaiting = false;
        //        _mouseMoveTimer.Stop();

        //        if (_mousePending)
        //        {
        //            return;
        //        }

        //        _mousePending = true;
        //        Thread t = new Thread(ProcessMouseMove);

        //        var x = _xAxis.GetUnscaledValue(_currMousePos.X, _scalerParamsX);
        //        var y = _yAxis.GetUnscaledValue(_currMousePos.Y, _scalerParamsY);
        //        t.Start(new MouseMoveThunk()
        //        {
        //            DesiredNeighborCount = DesiredNeighborsCount,
        //            Position = _currMousePos,
        //            AxisPosition = new Point(x, y)
        //        });
        //    }
        //}

        //private void ProcessMouseMove(object thunk)
        //{
        //    MouseMoveThunk m = (MouseMoveThunk)thunk;
        //    var list = new List<KNearestResult>(m.DesiredNeighborCount);

        //    KNearestResults res = new KNearestResults();
        //    res.Results = list;
        //    res.ConsideredCutoff = 1000;

        //    _tree.KNearest(res, m.AxisPosition.X, m.AxisPosition.Y, m.DesiredNeighborCount);
        //    this.Dispatcher.BeginInvoke(() =>
        //    {
        //        FinishMouseMove(res, m.AxisPosition.X, m.AxisPosition.Y, m.Position);
        //    });
        //}

        //private void FinishMouseMove(KNearestResults res, double x, double y, Point pos)
        //{
        //    _mousePending = false;
        //    _lastMouseMove = DateTime.Now;

        //    if (ScatterMouseOver != null)
        //    {
        //        List<object> ret = new List<object>();
        //        for (var i = 0; i < res.Results.Count; i++)
        //        {
        //            var current = res.Results[i];
        //            if (current.IsMedian)
        //            {
        //                ret.Add(current.Node.Median.Data);
        //            }
        //            else
        //            {
        //                ret.Add(current.Node.OtherPoints[current.Index].Data);
        //            }
        //        }
        //        ScatterMouseOver(this,
        //            new ScatterMouseOverEventArgs(
        //                new Point(x, y),
        //                pos,
        //                ret));
        //        UpdateHoverHighlights(res);
        //    }
        //}

        //private void UpdateHoverHighlights(KNearestResults res)
        //{
        //    var geom = (PathGeometry)_hoverPath.Data;
        //    geom.Figures.Clear();
        //    foreach (var item in res.Results)
        //    {
        //        PathFigure f = new PathFigure();
        //        Point center = new Point(
        //            _xAxis.GetScaledValue(item.X, _scalerParamsX),
        //            _yAxis.GetScaledValue(item.Y, _scalerParamsY));
        //        f.StartPoint = new Point(center.X,
        //            center.Y - 4);
        //        f.Segments.Add(new ArcSegment()
        //        {
        //            IsLargeArc = false,
        //            Size = new Size(4, 4),
        //            Point = new Point(center.X, center.Y + 4)
        //        });
        //        f.Segments.Add(new ArcSegment()
        //        {
        //            IsLargeArc = false,
        //            Size = new Size(4, 4),
        //            Point = new Point(center.X, center.Y - 4)
        //        });
        //        geom.Figures.Add(f);
        //    }
        //}

        public event EventHandler<ScatterMouseOverEventArgs> ScatterMouseOver;
    }

    /// <summary>
    /// Provides information about a mouse move computation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MouseMoveThunk
    {
        /// <summary>
        /// The position of the mouse.
        /// </summary>
        public Point Position { get; set; }
        /// <summary>
        /// The axis based position of the moust.
        /// </summary>
        public Point AxisPosition { get; set; }
        /// <summary>
        /// The desired neigbor count specified.
        /// </summary>
        public int DesiredNeighborCount { get; set; }
        /// <summary>
        /// The parameters of the x scaler.
        /// </summary>
        public ScalerParams ScalerParamsX { get; set; }
    }

    /// <summary>
    /// Describes information about a mouse move event for the high density scatter series.
    /// </summary>
    public class ScatterMouseOverEventArgs
        : EventArgs
    {
        /// <summary>
        /// The axis based position of the mouse.
        /// </summary>
        public Point AxisPosition { get; set; }
        /// <summary>
        /// The current mouse position.
        /// </summary>
        public Point MousePosition { get; set; }
        /// <summary>
        /// The nearest items to the mouse.
        /// </summary>
        public List<object> NearestItems { get; set; }

        /// <summary>
        /// Constructs a ScatterMouseOverEventArgs
        /// </summary>
        /// <param name="axisPosition">The axis based position of the mouse</param>
        /// <param name="mousePosition">The current position of the mouse.</param>
        /// <param name="nearestItems">The nearest items to the mouse.</param>
        public ScatterMouseOverEventArgs(
            Point axisPosition,
            Point mousePosition,
            List<object> nearestItems)
        {
            AxisPosition = axisPosition;
            MousePosition = mousePosition;
            NearestItems = nearestItems;
        }
    }

    /// <summary>
    /// Represents the view for a HighDensityScatterSeries.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class HighDensityScatterSeriesView
        : SeriesView
    {
        /// <summary>
        /// Constructs a HighDensityScatterSeriesView.
        /// </summary>
        /// <param name="model">The HighDensityScatterSeries for the view.</param>
        public HighDensityScatterSeriesView(HighDensityScatterSeries model)
            : base(model)
        {
            HighDensityScatterModel = model;
        }

        /// <summary>
        /// The HighDensityScatterSeries for the view.
        /// </summary>
        protected HighDensityScatterSeries HighDensityScatterModel { get; set; }
    }

    /// <summary>
    /// Provides information about the progressive load progress of the HighDensityScatterSeries.
    /// </summary>
    public class ProgressiveLoadStatusEventArgs
        : EventArgs
    {
        /// <summary>
        /// The current status from 0 to 100 of the progressive load.
        /// </summary>
        public int CurrentStatus { get; set; }

        /// <summary>
        /// Constructs a ProgressiveLoadStatusEventArgs
        /// </summary>
        /// <param name="currentStatus">The current status of the load.</param>
        public ProgressiveLoadStatusEventArgs(int currentStatus)
        {
            CurrentStatus = currentStatus;
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