using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Infragistics.Controls;
using System.Windows.Media;



using System.Linq;
using System.IO;


namespace Infragistics.Controls.Maps
{
    

    class Tile
    {
        public int X;
        public int Y;
        public int Z;
        public Image Image;
        public Image GhostImage;
        public DateTime FadeStart;

        public Rect Rect
        {
            get
            {
                double width = Math.Pow(2.0, -Z);
                double height = Math.Pow(2.0, -Z);

                return new Rect(X * width, Y * height, width, height);
            }
        }
    }



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

    /// <summary>
    /// Interface for handling deferred rendering under Infragistics map controls.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IMapRenderDeferralHandler
    {
        /// <summary>
        /// Registers the given DependencyObject for deferred rendering.
        /// </summary>
        /// <param name="source">The DependencyObject to register for deferred rendering.</param>
        /// <param name="refresh">An action to take each time a refresh is called for.</param>
        void Register(DependencyObject source, Action<bool> refresh);
        /// <summary>
        /// Unregisters the given DependencyObject for deferred rendering.
        /// </summary>
        /// <param name="source">The DependencyObject to unregister for deferred rendering.</param>
        void UnRegister(DependencyObject source);
        /// <summary>
        /// Call for a deferred refresh.
        /// </summary>
        void DeferredRefresh();
    }

    /// <summary>
    /// The Infragistics MultiScaleImage control.
    /// </summary>
    public class XamMultiScaleImage : Control, INotifyPropertyChanged
    {
        #region constructor and initialisation
        /// <summary>
        /// XamMultiScaleImage constructor.
        /// </summary>
        public XamMultiScaleImage()
        {
            CanvasSize = Rect.Empty;



            View = new XamMultiScaleImageView(this);

            DefaultStyleKey = typeof(XamMultiScaleImage);

            ActualViewportOrigin = ViewportOrigin;
            ActualViewportWidth = ViewportWidth;
        }

        internal StackPool<Image> ImagePool;

        internal XamMultiScaleImageView View { get; set; }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ContentPresenter = GetTemplateChild(ContentPresenterElementName) as ContentPresenter;
        }

        private const string ContentPresenterElementName = "ContentPresenter";
        /// <summary>
        /// The main ContentPresenter UIElement which comes from the ControlTemplate of this MultiScaleImage control.
        /// </summary>
        public ContentPresenter ContentPresenter
        {
            get { return contentPresenter; }
            private set
            {
                if (ContentPresenter != value)
                {
                    if (ContentPresenter != null)
                    {
                        ContentPresenter.Content = null;
                    }

                    contentPresenter = value;

                    if (ContentPresenter != null)
                    {
                        ContentPresenter.Content = View.Canvas;
                    }

                    Refresh();
                }
            }
        }
        private ContentPresenter contentPresenter;


        #endregion

        private IMapRenderDeferralHandler _deferralHandler;
        /// <summary>
        /// The deferral handler to use for deferred refreshes.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IMapRenderDeferralHandler DeferralHandler
        {
            get
            {
                return _deferralHandler;
            }
            set
            {
                _deferralHandler = value;
                _deferralHandler.Register(this, RefreshInternal);
            }
        }

        #region Source Dependency Property
        /// <summary>
        /// The TileSource to use for tiles in this XamMultiScaleImage.
        /// </summary>
        public XamMultiScaleTileSource Source
        {
            get
            {
                return (XamMultiScaleTileSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        internal const string SourcePropertyName = "Source";
        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(SourcePropertyName, typeof(XamMultiScaleTileSource), typeof(XamMultiScaleImage),
            new PropertyMetadata(null, (sender, e) =>
            {
                ((XamMultiScaleImage)sender)
                    .OnPropertyChanged(new PropertyChangedEventArgs<XamMultiScaleTileSource>(SourcePropertyName, e.OldValue as XamMultiScaleTileSource, e.NewValue as XamMultiScaleTileSource));
            }));
        #endregion

        #region ViewportOrigin Dependency Property and ActualViewportOrigin Property
        /// <summary>
        /// The origin point of the MultiScaleImage Viewport.
        /// </summary>
        public Point ViewportOrigin
        {
            get
            {
                return (Point)GetValue(ViewportOriginProperty);
            }
            set
            {
                SetValue(ViewportOriginProperty, value);
            }
        }

        internal const string ViewportOriginPropertyName = "ViewportOrigin";
        /// <summary>
        /// Identifies the ViewportOrigin dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewportOriginProperty = DependencyProperty.Register(ViewportOriginPropertyName, typeof(Point), typeof(XamMultiScaleImage),
            new PropertyMetadata(new Point(0, 0), (sender, e) =>
            {
                ((XamMultiScaleImage)sender)
                    .OnPropertyChanged(new PropertyChangedEventArgs<Point>(ViewportOriginPropertyName, (Point)e.OldValue, (Point)e.NewValue));
            }));

        internal Point ActualViewportOrigin
        {
            get;
            private set;
        }
        #endregion

        #region ViewportWidth Dependency Property and ActualViewportWidth Property
        /// <summary>
        /// The Width of the MultiScaleImage Viewport.
        /// </summary>
        public double ViewportWidth
        {
            get
            {
                return (double)GetValue(ViewportWidthProperty);
            }
            set
            {
                SetValue(ViewportWidthProperty, value);
            }
        }

        internal const string ViewportWidthPropertyName = "ViewportWidth";
        /// <summary>
        /// Identifies the ViewportWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(ViewportWidthPropertyName, typeof(double), typeof(XamMultiScaleImage),
            new PropertyMetadata(1.0, (sender, e) =>
            {
                ((XamMultiScaleImage)sender)
                    .OnPropertyChanged(new PropertyChangedEventArgs<double>(ViewportWidthPropertyName, (double)e.OldValue, (double)e.NewValue));
            }));

        internal double ActualViewportWidth
        {
            get;
            private set;
        }
        #endregion

        #region UseSprings Dependency Property
        /// <summary>
        /// Boolean indicating whether or not dampening should be used during pan operations.
        /// </summary>
        public bool UseSprings
        {
            get
            {
                return (bool)GetValue(UseSpringsProperty);
            }
            set
            {
                SetValue(UseSpringsProperty, value);
            }
        }

        internal const string UseSpringsPropertyName = "UseSprings";
        /// <summary>
        /// Identifies the UseSprings dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSpringsProperty = DependencyProperty.Register(UseSpringsPropertyName, typeof(bool), typeof(XamMultiScaleImage),
            new PropertyMetadata(false, (sender, e) =>
            {
                ((XamMultiScaleImage)sender)
                    .OnPropertyChanged(new PropertyChangedEventArgs<bool>(UseSpringsPropertyName, (bool)e.OldValue, (bool)e.NewValue));
            }));
        #endregion

        #region SpringsEasingFunction Dependency Property
        /// <summary>
        /// The easing function to use for dampening during pan operations.
        /// </summary>
        public IEasingFunction SpringsEasingFunction
        {
            get
            {
                return (IEasingFunction)GetValue(SpringsEasingFunctionProperty);
            }
            set
            {
                SetValue(SpringsEasingFunctionProperty, value);
            }
        }

        internal const string SpringsEasingFunctionPropertyName = "SpringsEasingFunction";
        /// <summary>
        /// Identifies the SpringsEasingFunction dependency property.
        /// </summary>
        public static readonly DependencyProperty SpringsEasingFunctionProperty = DependencyProperty.Register(SpringsEasingFunctionPropertyName, typeof(IEasingFunction), typeof(XamMultiScaleImage),
            new PropertyMetadata(null, (sender, e) =>
            {
                ((XamMultiScaleImage)sender)
                    .OnPropertyChanged(new PropertyChangedEventArgs<IEasingFunction>(SpringsEasingFunctionPropertyName, (IEasingFunction)e.OldValue, (IEasingFunction)e.NewValue));
            }));
        #endregion
        /// <summary>
        /// Event raised whenever a property value on this XamMultiScaleImage has been changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method invoked whenever a property value on this XamMultiScaleImage has been changed.
        /// </summary>
        /// <param name="ea">The PropertyChangedEventArgs in context.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs ea)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, ea);
            }

            switch (ea.PropertyName)
            {
                case SourcePropertyName:
                    if (Source != null)
                    {
                        Source.MultiScaleImage = this;
                    }

                    // empty every thing, cancel everything, clear everything

                    // precache the root                                        
                    PurgeCache();
                    this.ResetTiles();
                    Refresh();
                    break;

                case ViewportOriginPropertyName:
                    Spring();
                    break;

                case ViewportWidthPropertyName:
                    Spring();
                    break;

                case UseSpringsPropertyName:
                    if (!UseSprings)
                    {
                        View.DisableSprings();
                    }

                    break;
            }
        }
        private int LevelOffset { get; set; }
        private int MaxLevel { get; set; }
        private void ResetTiles()
        {
            this.TrashActiveTiles();
            if (this.Source != null)
            {
                this.LevelOffset = Convert.ToInt32(Math.Log(this.Source.TileWidth, 2.0));
                this.MaxLevel = Convert.ToInt32(Math.Log(this.Source.ImageWidth, 2));
            }
        }
        internal void InvalidateTileLayer(int level, int tilePositionX, int tilePositionY, int tileLayer)
        {
            // todo: Invalidate only specific tile layers
            PurgeCache();
            ResetTiles();
            Refresh();
        }

        #region Springs
        private DateTime SpringStart;

        private Point anchorViewportOrigin;
        private double anchorViewportWidth;

        private void Spring()
        {
            if (UseSprings)
            {
                SpringStart = DateTime.Now;

                anchorViewportOrigin = ActualViewportOrigin;
                anchorViewportWidth = ActualViewportWidth;

                View.StartSpringTimer();
            }
            else
            {
                ActualViewportOrigin = ViewportOrigin;
                ActualViewportWidth = ViewportWidth;
                Refresh();
            }
        }

        internal void SpringTimer_Tick()
        {
            double duration = 2.0;





            TimeSpan t = (DateTime.Now - SpringStart);
            double totalSeconds = t.TotalSeconds;


            double p = MathUtil.Clamp((totalSeconds) / duration, 0.0, 1.0);

            double p1 = SpringsEasingFunction != null ? SpringsEasingFunction.Ease(p) : p;
            double p0 = 1.0 - p1;

            ActualViewportWidth = anchorViewportWidth * p0 + ViewportWidth * p1;
            ActualViewportOrigin = new Point(anchorViewportOrigin.X * p0 + ViewportOrigin.X * p1, anchorViewportOrigin.Y * p0 + ViewportOrigin.Y * p1);

            if (p >= 1.0)
            {
                View.StopSpringTimer();
                // generate an event to notify that the viewport has finished changing
            }
            else
            {
                // generate an event to notify that the viewport is changing
            }

            Refresh();
        }
        #endregion

        #region Refresh
        private List<Tile> activeTiles = new List<Tile>();

        private int ActiveTileIndex(int x, int y, int z)
        {
            for (int i = 0; i < activeTiles.Count; ++i)
            {
                if (activeTiles[i].X == x && activeTiles[i].Y == y && activeTiles[i].Z == z)
                {
                    return i;
                }
            }

            return -1;
        }

        internal void RefreshInternal(bool animate)
        {
            refreshDeferred = false;

            if (this.Source == null || !View.Ready() || CanvasSize.Width == 0.0 || CanvasSize.Height == 0.0)
            {
                return;
            }

            int horizontalCount = (int)Math.Ceiling(CanvasSize.Width / Source.TileWidth);
            int zz = (int)Math.Max(1.0, Math.Floor(-Math.Log(ActualViewportWidth / horizontalCount, 2.0)));
            if (zz > this.MaxLevel)
            {
                return;
            }



            var maxTiles = Convert.ToInt32(Math.Pow(2, zz));

            //////

            double width = ActualViewportWidth;
            double height = CanvasSize.Height * width / CanvasSize.Width;
            double wx = Source.ImageWidth / Math.Pow(2.0, zz);
            double wy = Source.ImageHeight / Math.Pow(2.0, zz);

            int u0 = Math.Max((int)Math.Floor((ActualViewportOrigin.X * Source.ImageWidth) / wx), 0);
            int u1 = Math.Min((int)Math.Ceiling(((ActualViewportOrigin.X + width) * Source.ImageWidth) / wx), maxTiles);
            int v0 = Math.Max((int)Math.Floor((ActualViewportOrigin.Y * Source.ImageHeight) / wy), 0);
            int v1 = Math.Min((int)Math.Ceiling(((ActualViewportOrigin.Y + height) * Source.ImageWidth) / wy), maxTiles);

            double ox = ((u0 * wx) - (ActualViewportOrigin.X * Source.ImageWidth)) / wx;
            double oy = ((v0 * wy) - (ActualViewportOrigin.Y * Source.ImageHeight)) / wy;
            double s = (width * Source.ImageWidth / wx) * (Source.TileWidth / CanvasSize.Width);

            //////

            List<Tile> newTiles = new List<Tile>();

            #region build list of new tiles
            for (int u = u0; u < u1; ++u)
            {
                for (int v = v0; v < v1; ++v)
                {
                    int index = ActiveTileIndex(u, v, zz);

                    if (index >= 0)
                    {
                        newTiles.Add(activeTiles[index]);
                        activeTiles.RemoveAt(index);
                    }
                    else
                    {
                        newTiles.Add(new Tile() { X = u, Y = v, Z = zz });
                    }
                }
            }
            #endregion

            ImagePool.DeferDisactivate = true;

            #region trash the remaining active tiles
            TrashActiveTiles();
            #endregion

            activeTiles = newTiles;

            for (int i = 0; i < activeTiles.Count; ++i)
            {
                if (activeTiles[i].Image == null)
                {
                    Debug.Assert(activeTiles[i].GhostImage == null);

                    activeTiles[i].Image = ImagePool.Pop();
                    activeTiles[i].Image.Opacity = 1.0;

                    View.SendToBackground(activeTiles[i].Image);

                    #region refresh the newly active tile
                    WriteableBitmap bitmap = GetCachedBitmap(activeTiles[i]); // get from cache

                    if (bitmap != null)
                    {
                        activeTiles[i].Image.Source = bitmap;
                    }
                    else
                    {
                        WriteableBitmap donor = null;
                        Tile tile = new Tile() { X = activeTiles[i].X, Y = activeTiles[i].Y, Z = activeTiles[i].Z };

                        while (tile.Z >= 0 && donor == null)
                        {
                            tile.X = tile.X >> 1;
                            tile.Y = tile.Y >> 1;
                            tile.Z = tile.Z - 1;

                            donor = GetCachedBitmap(tile);
                        }

                        if (donor != null)
                        {
                            int q = (int)Math.Pow(2, activeTiles[i].Z - tile.Z);

                            int size = 256 / q;
                            int left = size * (activeTiles[i].X % q);
                            int top = size * (activeTiles[i].Y % q);

                            activeTiles[i].GhostImage = ImagePool.Pop();
                            activeTiles[i].GhostImage.Opacity = 1.0;

                            View.SendToForeground(activeTiles[i].GhostImage);











                            size = Math.Max(1, size);
                            activeTiles[i].GhostImage.Source = new CroppedBitmap(donor, new Int32Rect(left, top, size, size));


                        }

                        View.Download(activeTiles[i]);                    // queue tile for load
                    }
                    #endregion
                }

                #region set the position and size of the tile
                double iw = Source.TileWidth / s;
                double ih = Source.TileHeight / s;
                double il = (activeTiles[i].X - u0 + ox) * iw;
                double it = (activeTiles[i].Y - v0 + oy) * ih;

                View.SetImagePosition(activeTiles[i].Image, il, it);

                activeTiles[i].Image.Width = iw + 0.5;
                activeTiles[i].Image.Height = ih + 0.5;

                if (activeTiles[i].GhostImage != null)
                {
                    View.SetImagePosition(activeTiles[i].GhostImage, il, it);
                    activeTiles[i].GhostImage.Width = iw + 0.5;
                    activeTiles[i].GhostImage.Height = ih + 0.5;
                }
                #endregion
            }

            ImagePool.DeferDisactivate = false;
            View.RefreshCompleted();

            //#if WPF
            //            //HACK: Note, this should not be necessary! A simple InvalidateArrange should suffice,
            //            //And even then, the modifications to Canvas.Left and Canvas.Top should suffice to invalidate
            //            //the canvas, but in reality. WPF is dropping some updates causing images to vanish.
            //            //This is only reproducable when there are things in the map that are causing the refresh to 
            //            //take longer. Like a lot of markers from SymbolSeries. This hack forces an arrange, which 
            //            //fixes the problem.
            //            if (View.Canvas.ActualWidth > 0 && View.Canvas.ActualHeight > 0)
            //            {
            //                View.Canvas.Arrange(new Rect(0, 0, View.Canvas.ActualWidth, View.Canvas.ActualHeight));
            //            }
            //#endif
        }

        private bool refreshDeferred = false;
        internal void Refresh()
        {
            if (this.Source == null || !View.Ready() || CanvasSize.Width == 0.0 || CanvasSize.Height == 0.0)
            {
                return;
            }

            if (refreshDeferred)
            {
                return;
            }

            refreshDeferred = true;
            View.Defer(RefreshInternal);
        }

        private void TrashActiveTiles()
        {
            for (int i = 0; i < activeTiles.Count; ++i)
            {
                View.CancelDownload(activeTiles[i]);                    // remove from pending and cancel if active
                CancelFade(activeTiles[i]);                 // make sure it's not fading and remove the ghost image

                if (activeTiles[i].Image != null)
                {
                    ImagePool.Push(activeTiles[i].Image);
                    activeTiles[i].Image.Source = null;
                    activeTiles[i].Image = null;
                }

                Debug.Assert(activeTiles[i].Image == null);
                Debug.Assert(activeTiles[i].GhostImage == null);
            }
        }

        #endregion

        #region Cache
        private void PurgeCache()
        {
            //
            // stuff to purge, in order until the cache is small enough
            //
            // higher resolution, doesn't intersect the the current window
            // higher resolution, intersects the current window
            // same resolution, doesn't intersect the current window
            // lower resolution, doesn't intersect the current window
            // lower resolution, intersects the current window

            cache.Clear();
        }
        private WriteableBitmap GetCachedBitmap(Tile tile)
        {
            for (int i = 0; i < cache.Count; ++i)
            {
                if (cache[i].First.X == tile.X && cache[i].First.Y == tile.Y && cache[i].First.Z == tile.Z)
                {
                    return cache[i].Second;
                }
            }

            return null;
        }
        internal void CacheBitmap(Tile tile, WriteableBitmap bitmap)
        {
            cache.Add(new Pair<Tile, WriteableBitmap>(tile, bitmap));
        }
        private readonly List<Pair<Tile, WriteableBitmap>> cache = new List<Pair<Tile, WriteableBitmap>>();
        #endregion


        public event ImageStreamValidEventHandler ImageStreamValid;

        internal bool IsImageStreamValid(Stream stream)
        {
            if (ImageStreamValid == null)
            {
                return true;
            }

            bool ret = ImageStreamValid(this, new ImageStreamValidEventArgs(stream));
            if (ret && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (ret && Source != null)
            {
                if (!Source.IsImageStreamValid(stream))
                {
                    ret = false;
                }
                if (ret && stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            return ret;
        }


        #region Fading
        private readonly List<Tile> fadingTiles = new List<Tile>();

        internal void StartFade(Tile tile)
        {
            Debug.Assert(tile.Image != null);
            // assert tile not active
            // assert tile not in pending
            // assert tile not already in fading

            if (tile.GhostImage != null)
            {
                tile.FadeStart = DateTime.Now;
                fadingTiles.Add(tile);

                View.StartFadeTimer();
            }
        }
        private void CancelFade(Tile tile)
        {
            if (tile.GhostImage != null)
            {
                ImagePool.Push(tile.GhostImage);
                tile.GhostImage.Source = null;
                tile.GhostImage = null;

                for (int i = 0; i < fadingTiles.Count; ++i)
                {
                    if (fadingTiles[i] == tile)
                    {
                        fadingTiles.RemoveAt(i);
                        break;
                    }
                }

                if (fadingTiles.Count == 0)
                {
                    View.StopFadeTimer();
                }

                Debug.Assert(tile.GhostImage == null);
            }
        }
        internal void FadeTimer_Tick()
        {
            DateTime now = DateTime.Now;
            double fadeDuration = 0.50;

            for (int i = 0; i < fadingTiles.Count; )
            {




                TimeSpan t = (now - fadingTiles[i].FadeStart);
                double totalSeconds = t.TotalSeconds;

                double p = (totalSeconds) / fadeDuration;

                p = MathUtil.Clamp(p, 0.0, 1.0);
                fadingTiles[i].GhostImage.Opacity = 1.0 - p;

                if (p >= 1.0)
                {
                    ImagePool.Push(fadingTiles[i].GhostImage);
                    fadingTiles[i].GhostImage.Source = null;
                    fadingTiles[i].GhostImage = null;

                    fadingTiles.RemoveAt(i);
                }
                else
                {
                    ++i;
                }

                View.FadingChanged();
            }

            if (fadingTiles.Count == 0)
            {
                View.StopFadeTimer();
            }
        }
        #endregion


        internal Rect CanvasSize { get; set; }

        internal void OnSpringsDisabled()
        {
            ActualViewportWidth = ViewportWidth;
            ActualViewportOrigin = ViewportOrigin;

            Refresh();
        }



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


        
    }

    /// <summary>
    /// Represents an immutable pair of values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value in the pair.</typeparam>
    /// <typeparam name="T2">The type of the second value in the pair.</typeparam>
    internal class Pair<T1, T2>
    {
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public T1 First { get; set; }
        public T2 Second { get; set; }
    }


    /// <summary>
    /// Provides information to be used to determine if a map image is valid.
    /// </summary>
    public class ImageStreamValidEventArgs
        : EventArgs
    {
        /// <summary>
        /// The stream containing the image bytes for validation.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Constructs an ImageStreamValidEventArgs.
        /// </summary>
        /// <param name="stream">The stream to be validated.</param>
        public ImageStreamValidEventArgs(Stream stream)
        {
            Stream = stream;
        }
    }

    /// <summary>
    /// A handler to assert if a given map image is valid.
    /// </summary>
    /// <param name="sender">The source of the image to validate.</param>
    /// <param name="args">Information about the image to be validated.</param>
    /// <returns></returns>
    public delegate bool ImageStreamValidEventHandler(object sender, ImageStreamValidEventArgs args);










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