//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Media.Imaging;
//using System.Windows.Threading;

//namespace Infragistics.Controls.Maps
//{
//#if WINDOWS_PHONE
//    /// <summary>
//    /// This class is a placeholder used to support multi-platform compilation from the same source.  The xamMultiScaleImage control is not included in NetAdvantage for Windows Phone.
//    /// </summary>
//    internal class XamMultiScaleImage : Control
//    {
//    }
//#else
//    ///<summary>
//    /// Enable users to open a multi-resolution image, which can be zoomed in on and panned across
//    ///</summary>
//    [TemplatePart(Name = ContentPresenterElementName, Type = typeof(ContentPresenter))]
//    [DesignTimeVisible(false)]
//    public class XamMultiScaleImage : Control, INotifyPropertyChanged
//    {
//        private class TileDownloadTask
//        {
//            private readonly int _level;
//            private readonly int _tileX;
//            private readonly int _tileY;

//            public TileDownloadTask(int level, int tileX, int tileY)
//                : this(level, tileX, tileY, 0, 0)
//            {
//            }

//            public TileDownloadTask(int level, int tileX, int tileY, double left, double top)
//            {
//                _level = level;
//                _tileX = tileX;
//                _tileY = tileY;
//                Left = left;
//                Top = top;
//            }

//            public int Level
//            {
//                get { return _level; }
//            }

//            public int TileX
//            {
//                get { return _tileX; }
//            }
//            public int TileY
//            {
//                get { return _tileY; }
//            }

//            public double Left { get; set; }

//            public double Top { get; set; }
//        }

//        internal void InvalidateTileLayer(int level, int tilePositionX, int tilePositionY, int tileLayer)
//        {            
//            ResetTiles();
//        }

//        #region Constructor and Initialisation
//        /// <summary>
//        /// Initialises a new XamMultiScaleImage object.
//        /// </summary>        
//        public XamMultiScaleImage()
//        {
//            DefaultStyleKey = typeof(XamMultiScaleImage);

//            _grid = new Grid();
//            _canvas = new Canvas { SnapsToDevicePixels = true };

//            _grid.SizeChanged += (o, e) =>
//                                     {
//#if SILVERLIGHT
//                Dispatcher.BeginInvoke(new Action(() => Grid_SizeChanged(e.NewSize.Width, e.NewSize.Height)));
//#else                
//                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Grid_SizeChanged(e.NewSize.Width, e.NewSize.Height)));                                         
//#endif
//                                     };

//            _grid.Children.Add(_canvas);

//            _cacheMaintenanceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.ContextIdle, CacheMaintenance, Dispatcher.CurrentDispatcher);
//            _cacheMaintenanceTimer.Stop();

//            _asyncTasks = new List<Tuple<WebClient, TileDownloadTask>>();
//            _preDownloadAsyncTasks = new List<Tuple<WebClient, TileDownloadTask>>();
//        }

//        private void Grid_SizeChanged(double width, double height)
//        {
//            _grid.Clip = new RectangleGeometry { Rect = new Rect(0.0, 0.0, width, height) };
//            Refresh();
//        }
//        /// <summary>
//        /// Method invoked whenever application code or internal processes call ApplyTemplate.
//        /// </summary>
//        public override void OnApplyTemplate()
//        {
//            base.OnApplyTemplate();
//            ContentPresenter = GetTemplateChild(ContentPresenterElementName) as ContentPresenter;
//        }

//        private const string ContentPresenterElementName = "ContentPresenter";

//        /// <summary>
//        /// Gets or sets the content presenter.
//        /// </summary>
//        /// <value>The content presenter.</value>
//        public ContentPresenter ContentPresenter
//        {
//            get { return _contentPresenter; }
//            private set
//            {
//                if (ContentPresenter != value)
//                {
//                    if (ContentPresenter != null)
//                    {
//                        ContentPresenter.Content = null;
//                    }

//                    _contentPresenter = value;

//                    if (ContentPresenter != null)
//                    {
//                        ContentPresenter.Content = _grid;
//                    }

//                    Refresh();
//                }
//            }
//        }
//        private ContentPresenter _contentPresenter;
//        private readonly Grid _grid;
//        private readonly Canvas _canvas;

//        private TileSubstituteSpatialCache _cache;
//        private readonly List<Tuple<WebClient, TileDownloadTask>> _asyncTasks;
//        private readonly List<Tuple<WebClient, TileDownloadTask>> _preDownloadAsyncTasks;
//        private readonly DispatcherTimer _cacheMaintenanceTimer;

//        private int _level;
//        private int _levelOffset;
//        private int _maxLevel;
//        private Tuple<int, int> _xTiles;
//        private Tuple<int, int> _yTiles;

//        #endregion

//        #region Source Dependency Property
//        /// <summary>
//        /// Gets or sets the XamMultiScaleTileSource object that is used as the source for the XamMultiScaleImage
//        /// </summary>
//        public XamMultiScaleTileSource Source
//        {
//            get
//            {
//                return (XamMultiScaleTileSource)GetValue(SourceProperty);
//            }
//            set
//            {
//                SetValue(SourceProperty, value);
//            }
//        }

//        internal const string SourcePropertyName = "Source";
//        /// <summary>
//        /// Identifies the Source dependency property.
//        /// </summary>
//        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(SourcePropertyName, typeof(XamMultiScaleTileSource), typeof(XamMultiScaleImage),
//            new PropertyMetadata(null, (sender, e) => ((XamMultiScaleImage)sender).OnPropertyChanged(new PropertyChangedEventArgs<XamMultiScaleTileSource>(SourcePropertyName, e.OldValue as XamMultiScaleTileSource, e.NewValue as XamMultiScaleTileSource))));
//        #endregion

//        #region ViewportOrigin Dependency Property
//        /// <summary>        
//        /// Gets or sets the top-left corner of the area of the image to be displayed.
//        /// </summary>
//        /// <returns>
//        /// The top-left corner of the rectangular area of the image to be displayed.
//        /// </returns>
//        public Point ViewportOrigin
//        {
//            get
//            {
//                return (Point)GetValue(ViewportOriginProperty);
//            }
//            set
//            {
//                SetValue(ViewportOriginProperty, value);
//            }
//        }

//        internal const string ViewportOriginPropertyName = "ViewportOrigin";
//        /// <summary>
//        /// Identifies the ViewportOrigin dependency property.
//        /// </summary>
//        public static readonly DependencyProperty ViewportOriginProperty = DependencyProperty.Register(ViewportOriginPropertyName, typeof(Point), typeof(XamMultiScaleImage),
//            new PropertyMetadata(new Point(0, 0), (sender, e) => ((XamMultiScaleImage)sender).OnPropertyChanged(new PropertyChangedEventArgs<Point>(ViewportOriginPropertyName, (Point)e.OldValue, (Point)e.NewValue))));
//        #endregion

//        #region ViewportWidth Dependency Property
//        /// <summary>
//        /// Gets or sets the width of the area of the image displayed.
//        /// </summary>
//        /// <returns>
//        /// The width of the area of the image displayed.
//        /// </returns>
//        public double ViewportWidth
//        {
//            get
//            {
//                return (double)GetValue(ViewportWidthProperty);
//            }
//            set
//            {
//                SetValue(ViewportWidthProperty, value);
//            }
//        }

//        internal const string ViewportWidthPropertyName = "ViewportWidth";
//        /// <summary>
//        /// Identifies the ViewportWidth dependency property.
//        /// </summary>
//        public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(ViewportWidthPropertyName, typeof(double), typeof(XamMultiScaleImage),
//            new PropertyMetadata(1.0, (sender, e) => ((XamMultiScaleImage)sender).OnPropertyChanged(new PropertyChangedEventArgs<double>(ViewportWidthPropertyName, (double)e.OldValue, (double)e.NewValue))));
//        #endregion

//        #region PropertyChanged
//        /// <summary>
//        /// Event raised when a property on this object changes.
//        /// </summary>
//        public event PropertyChangedEventHandler PropertyChanged;
//        /// <summary>
//        /// Method called when a property on this object changes.
//        /// </summary>
//        /// <param name="ea">The EventArgs in context.</param>
//        protected virtual void OnPropertyChanged(PropertyChangedEventArgs ea)
//        {
//            if (PropertyChanged != null)
//            {
//                PropertyChanged(this, ea);
//            }

//            switch (ea.PropertyName)
//            {
//                case SourcePropertyName:
//                    if (Source != null)
//                    {
//                        //Source.MultiScaleImage = this;

//                        ResetTiles();
//                    }
//                    else
//                    {
//                        _cache.Reset();
//                        _canvas.Children.Clear();
//                    }

//                    break;
//                case ViewportOriginPropertyName:
//                    Refresh();
//                    break;
//                case ViewportWidthPropertyName:
//                    Refresh();
//                    break;
//            }
//        }

//        private void ResetTiles()
//        {
//            CancelAsyncTasks(_asyncTasks);
//            CancelAsyncTasks(_preDownloadAsyncTasks);

//            _cache = new TileSubstituteSpatialCache(new SpatialCache<BitmapImage>(Convert.ToInt32(Math.Log(Source.ImageWidth, 2))), Source.TileWidth, Source.TileHeight);
//            _levelOffset = Convert.ToInt32(Math.Log(Source.TileWidth, 2.0));
//            _maxLevel = Convert.ToInt32(Math.Log(Source.ImageWidth, 2));

//            Refresh();
//        }

//        #endregion

//        /// <summary>
//        /// Clears the cache.
//        /// </summary>
//        public void ClearCache()
//        {
//            _cache.Clear();
//        }

//        private void Refresh()
//        {
//            double width = ViewportWidth;
//            double height = _canvas.ActualHeight * width / _canvas.ActualWidth;

//            if (width == 0 || double.IsNaN(width) || height == 0 || double.IsNaN(height) || 
//                this.Source == null ||
//                double.IsNaN(this.ViewportOrigin.X) || double.IsNaN(this.ViewportOrigin.Y))
//            {
//                return;
//            }

//            int vpTiles = Convert.ToInt32(Math.Ceiling(_canvas.ActualWidth / Source.TileWidth));
//            _level = Math.Max(Convert.ToInt32(Math.Floor(Math.Log(vpTiles / ViewportWidth, 2))), 1);
                         
//            if (_level > _maxLevel)
//            {
//                return;
//            }

//            int maxTiles = Convert.ToInt32(Math.Pow(2.0, _level));

//            int xTileStart = Math.Max(Convert.ToInt32(Math.Floor(ViewportOrigin.X * maxTiles)), 0);
//            int xTileEnd = Convert.ToInt32(Math.Min(Math.Ceiling((ViewportOrigin.X + ViewportWidth) * maxTiles), maxTiles));

//            double viewportHeight = _canvas.ActualHeight * ViewportWidth / _canvas.ActualWidth;
//            double ratio = ((double)Source.ImageHeight / Source.ImageWidth) * ((double)Source.TileWidth / Source.TileHeight);
//            int yTileStart = Math.Max(Convert.ToInt32(Math.Floor(ViewportOrigin.Y * maxTiles)), 0);
//            int yTileEnd = Convert.ToInt32(Math.Min(Math.Ceiling((ViewportOrigin.Y + viewportHeight) * maxTiles), maxTiles * ratio));

//            _xTiles = new Tuple<int, int>(xTileStart, xTileEnd);
//            _yTiles = new Tuple<int, int>(yTileStart, yTileEnd);

//            RefreshMap();

//            if (!DesignServices.IsInDesignModeStatic)
//            {
//                _cacheMaintenanceTimer.Stop();
//                _cacheMaintenanceTimer.Start();
//            }
//        }

//        private void CacheMaintenance(object sender, EventArgs e)
//        {
//            _cacheMaintenanceTimer.Stop();

//            if (_level < 4)
//            {
//                return;
//            }
            
//            var downLevel = _level - 1;
//            if (downLevel > _maxLevel)
//            {
//                return;
//            }

//            var maxTiles = Convert.ToInt32(Math.Pow(2, downLevel));

//            int buffer = 2;

//            var xTileStart = Math.Max(Convert.ToInt32(_xTiles.Item1 / 2) - buffer, 0);
//            var xTileEnd = Math.Min(Convert.ToInt32(_xTiles.Item2 / 2) + buffer, maxTiles);
//            var yTileStart = Math.Max(Convert.ToInt32(_yTiles.Item1 / 2) - buffer, 0);
//            var yTileEnd = Math.Min(Convert.ToInt32(_yTiles.Item2 / 2) + buffer, maxTiles);

//            var xTiles = new Tuple<int, int>(xTileStart, xTileEnd);
//            var yTiles = new Tuple<int, int>(yTileStart, yTileEnd);

//            PreDownloadTiles(downLevel, xTiles, yTiles);

//            buffer = _level % 2 == 1 ? 7 : 5;
//            PurgeCache(_level, buffer, _xTiles, _yTiles);
//            PurgeCache(downLevel, buffer * 2, xTiles, yTiles);
//        }

//        private void PreDownloadTiles(int level, Tuple<int, int> xTiles, Tuple<int, int> yTiles)
//        {
//            var tasksReused = new bool[_preDownloadAsyncTasks.Count];

//            for (int x = xTiles.Item1; x < xTiles.Item2; x++)
//            {
//                for (int y = yTiles.Item1; y < yTiles.Item2; y++)
//                {
//                    if (!_cache.ContainsTile(level, x, y))
//                    {
//                        TileDownloadTask task;
//                        var taskReused = false;

//                        for (int i = 0; i < _preDownloadAsyncTasks.Count; i++)
//                        {
//                            task = _preDownloadAsyncTasks[i].Item2;

//                            if (task.Level == level && task.TileX == x && task.TileY == y)
//                            {
//                                tasksReused[i] = true;
//                                taskReused = true;

//                                break;
//                            }
//                        }

//                        if (!taskReused)
//                        {
//                            if (this.Source != null)
//                            {
//                                var uri = this.Source.GetTileUri(level + _levelOffset, x, y);

//                                if (uri != null)
//                                {
//                                    task = new TileDownloadTask(level, x, y);
//                                    StartAsyncTileDownload(task, uri, _preDownloadAsyncTasks, TilePreDownloadCompleted);
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            RemoveNotReusedAsyncTasks(tasksReused, _preDownloadAsyncTasks);
//        }

//        private void PurgeCache(int level, int buffer, Tuple<int, int> xTiles, Tuple<int, int> yTiles)
//        {
//            _cache.ClearLevel(level, xTiles.Item1 - buffer, xTiles.Item2 + buffer, yTiles.Item1 - buffer, yTiles.Item2 + buffer);
//        }

//        private void RefreshMap()
//        {
//            var tasksReused = new bool[_asyncTasks.Count];

//            _canvas.Children.Clear();

//            double vpHeight = _canvas.ActualHeight * ViewportWidth / _canvas.ActualWidth;
//            int maxTiles = Convert.ToInt32(Math.Pow(2.0, _level));

//            _canvas.RenderTransform = new ScaleTransform
//            {
//                ScaleX = (_canvas.ActualWidth / (ViewportWidth * Source.TileWidth)) / maxTiles,
//                ScaleY = (_canvas.ActualHeight / (vpHeight * Source.TileHeight)) / maxTiles,
//            };

//            double offsetX = _xTiles.Item1 * Source.TileWidth - ViewportOrigin.X * Source.TileWidth * maxTiles;

//            for (int x = _xTiles.Item1; x < _xTiles.Item2; x++)
//            {
//                double offsetY = _yTiles.Item1 * Source.TileHeight - ViewportOrigin.Y * Source.TileHeight * maxTiles;

//                for (int y = _yTiles.Item1; y < _yTiles.Item2; y++)
//                {
//                    var cache = _cache.GetTileOrSubstitute(_level, x, y);

//                    if (cache.Image != null)
//                    {
//                        Canvas.SetLeft(cache.Image, offsetX);
//                        Canvas.SetTop(cache.Image, offsetY);

//                        //if (cache.ReloadTile)
//                        //{
//                        //    cache.Image.SnapsToDevicePixels = true;                            
//                        //    cache.Image.Effect = new BlurEffect { Radius = 3, KernelType = KernelType.Box, RenderingBias = RenderingBias.Quality };
//                        //}                                                                                                

//                        cache.Image.Width = Source.TileWidth;
//                        cache.Image.Height = Source.TileHeight;
//                        cache.Image.Stretch = Stretch.Fill;                       

//                        _canvas.Children.Add(cache.Image);
//                    }

//                    if (cache.ReloadTile)
//                    {
//                        TileDownloadTask task;
//                        var taskReused = false;

//                        for (int i = 0; i < _asyncTasks.Count; i++)
//                        {
//                            task = _asyncTasks[i].Item2;

//                            if (task.Level == _level && task.TileX == x && task.TileY == y)
//                            {
//                                task.Left = offsetX;
//                                task.Top = offsetY;

//                                tasksReused[i] = true;
//                                taskReused = true;

//                                break;
//                            }
//                        }

//                        if (!taskReused)
//                        {
//                            var uri = Source.GetTileUri(_level + _levelOffset, x, y);

//                            if (uri != null)
//                            {
//                                task = new TileDownloadTask(_level, x, y, offsetX, offsetY);
//                                StartAsyncTileDownload(task, uri, _asyncTasks, TileDownloadCompleted);
//                            }
//                        }
//                    }

//                    offsetY += Source.TileHeight;                   
//                }

//                offsetX += Source.TileWidth;                
//            }

//            RemoveNotReusedAsyncTasks(tasksReused, _asyncTasks);
//        }

//        private void TilePreDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
//        {
//            if (e.Cancelled || e.Error != null)
//            {
//                return;
//            }

//            var task = (TileDownloadTask)e.UserState;
//            var bitmapImage = GetBitmapImageFromBytes(e.Result);

//            _cache.AddTile(task.Level, task.TileX, task.TileY, bitmapImage);
//        }

//        private void TileDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
//        {
//            if (e.Cancelled || e.Error != null)
//            {
//                return;
//            }

//            var task = (TileDownloadTask)e.UserState;

//            if (task.Level != _level)
//            {
//                return;
//            }

//            var bitmapImage = GetBitmapImageFromBytes(e.Result);

//            _cache.AddTile(task.Level, task.TileX, task.TileY, bitmapImage);

//            var image = new Image { Source = bitmapImage, Opacity = 0.0, Width = Source.TileWidth, Height = Source.TileHeight, Stretch = Stretch.Fill };            

//            Canvas.SetLeft(image, task.Left);
//            Canvas.SetTop(image, task.Top);

//            _canvas.Children.Add(image);

//            var animation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(1.3))
//            {
//                AccelerationRatio = 0.2,
//                DecelerationRatio = 0.3
//            };

//            image.BeginAnimation(OpacityProperty, animation);
//        }

//        private static void CancelAsyncTasks(IList<Tuple<WebClient, TileDownloadTask>> asyncTasks)
//        {
//            foreach (var asyncTask in asyncTasks)
//            {
//                if (asyncTask.Item1.IsBusy)
//                {
//                    asyncTask.Item1.CancelAsync();
//                }
//            }

//            asyncTasks.Clear();
//        }

//        private static void RemoveNotReusedAsyncTasks(bool[] tasksReused, IList<Tuple<WebClient, TileDownloadTask>> taskList)
//        {
//            for (int i = tasksReused.Length - 1; i >= 0; i--)
//            {
//                if (!tasksReused[i])
//                {
//                    if (taskList[i].Item1.IsBusy)
//                    {
//                        taskList[i].Item1.CancelAsync();
//                    }

//                    taskList.RemoveAt(i);
//                }
//            }
//        }

//        private static void StartAsyncTileDownload(TileDownloadTask task, Uri uri, IList<Tuple<WebClient, TileDownloadTask>> taskList, DownloadDataCompletedEventHandler downloadCompleted)
//        {
//            var client = new WebClient();

//            taskList.Add(new Tuple<WebClient, TileDownloadTask>(client, task));

//            client.DownloadDataCompleted += downloadCompleted;
//            client.DownloadDataAsync(uri, task);
//        }

//        private static BitmapImage GetBitmapImageFromBytes(byte[] bytes)
//        {
//            var bitmapImage = new BitmapImage();
//            bitmapImage.BeginInit();
//            bitmapImage.StreamSource = new MemoryStream(bytes);
//            bitmapImage.EndInit();
//            return bitmapImage;
//        }
//    }
//#endif
//}

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