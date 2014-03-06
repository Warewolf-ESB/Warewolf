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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;

namespace Infragistics.Controls.Maps
{
    internal class XamMultiScaleImageView
    {
        protected XamMultiScaleImage Model { get; set; }

        public XamMultiScaleImageView(XamMultiScaleImage model)
        {
            Model = model;

            FadeTimer.Tick += (o,e) => Model.FadeTimer_Tick();
            SpringTimer.Tick += (o,e) => Model.SpringTimer_Tick();

            Model.ImagePool = new StackPool<Image>() { Create = Image_Create, Activate = Image_Activate, Deactivate = Image_Disactivate, Destroy = Image_Destroy };

            for (int i = 0; i < 4; ++i)
            {
                WebClient downloadClient = new WebClient();




                downloadClient.DownloadDataCompleted += DownloadClient_DownloadDataCompleted;


                DownloadClient.Add(downloadClient);
                DownloadTile.Add(null);
            }
            
            Model.CanvasSize = new Rect(0.0, 0.0, Canvas.ActualWidth, Canvas.ActualHeight);
            Canvas.SizeChanged += (o, e) =>
            {
                Model.CanvasSize = new Rect(0.0, 0.0, Canvas.ActualWidth, Canvas.ActualHeight);
                // [DN Oct 6 2011 : 90606] set the clip because canvases won't clip to bounds in silverlight.
                Canvas.Clip = new RectangleGeometry() { Rect = new Rect(0.0, 0.0, Canvas.ActualWidth, Canvas.ActualHeight) };
                Model.Refresh();
            };
        }

        internal readonly Canvas Canvas = new Canvas() { UseLayoutRounding = false };
        private readonly List<WebClient> DownloadClient = new List<WebClient>();

        private Image Image_Create()
        {
            return new Image() { UseLayoutRounding = false };
        }
        private void Image_Activate(Image image)
        {
            Canvas.Children.Add(image);
        }
        private void Image_Disactivate(Image image)
        {
            Canvas.Children.Remove(image);
        }
        private void Image_Destroy(Image image)
        {
        }

        internal void SetImagePosition(Image image, double il, double it)
        {
            Canvas.SetLeft(image, il);
            Canvas.SetTop(image, it);
        }

        internal void CancelDownload(Tile tile)
        {
            for (int i = 0; i < DownloadTile.Count; ++i)
            {
                if (tile == DownloadTile[i])
                {
                    DownloadClient[i].CancelAsync();

                    return;
                }
            }

            for (int i = 0; i < pendingTiles.Count; ++i)
            {
                if (pendingTiles[i] == tile)
                {
                    pendingTiles.RemoveAt(i);
                    break;
                }
            }
        }

        internal void Download(Tile tile)
        {
            // pending tiles will have a ghost
            // the priority depends on the size of the ghost
            // no ghost, small ghost, bigger ghost..

            pendingTiles.Add(tile);
            pendingTiles.Sort((a, b) =>
            {
                int sa = a.GhostImage != null ? (a.GhostImage.Source as BitmapSource).PixelWidth : 0;
                int sb = b.GhostImage != null ? (b.GhostImage.Source as BitmapSource).PixelWidth : 0;

                return sa.CompareTo(sb);
            }
            );
            BumpDownload();
        }
        private void BumpDownload()
        {
            XamMultiScaleTileSource tileSource;
            try
            {
                tileSource = this.Model.Source;
            }
            catch (UnauthorizedAccessException)
            {
                return; // the jig is up.  let's get out of here!
            }

            int index = -1;

            if (pendingTiles.Count > 0)
            {
                for (int i = 0; i < DownloadTile.Count; ++i)
                {
                    if (DownloadTile[i] == null)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index >= 0)
            {
                WebClient downloadClient = DownloadClient[index];

                DownloadTile[index] = pendingTiles[0];
                pendingTiles.RemoveAt(0);

                Uri uri = tileSource.GetTileUri(DownloadTile[index].Z + 8, DownloadTile[index].X, DownloadTile[index].Y);
                if (uri == null)
                {
                    DownloadTile[index] = null;
                    BumpDownload();
                }
                else
                {



                    downloadClient.DownloadDataAsync(uri, index);

                }
            }
        }

        private List<Tile> pendingTiles = new List<Tile>();


        private readonly List<Tile> DownloadTile = new List<Tile>();



#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)


        private void DownloadClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            int index = (int)e.UserState;
            Tile downloadTile = DownloadTile[index];

            DownloadTile[index] = null;

            XamMultiScaleTileSource tileSource;
            try
            {
                tileSource = this.Model.Source;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            if (e.Error == null && !e.Cancelled && tileSource != null)
            {
                bool valid = true;
                MemoryStream stream = new MemoryStream(e.Result);

                valid = Model.IsImageStreamValid(stream);

                if (valid)
                {
                    BitmapImage src = new BitmapImage();
                    src.BeginInit();
                    src.StreamSource = stream;
                    src.EndInit();
                
                    WriteableBitmap bitmap = new WriteableBitmap(src);

                    Model.CacheBitmap(downloadTile, bitmap);

                    if (downloadTile.Image != null)
                    {
                        downloadTile.Image.Source = null;
                        downloadTile.Image.Source = bitmap;
                        downloadTile.Image.InvalidateVisual();
    //#if WPF
    //                    //HACK: Note, this should not be necessary! A simple InvalidateArrange should suffice,
    //                    //And even then, the modifications to Canvas.Left and Canvas.Top should suffice to invalidate
    //                    //the canvas, but in reality. WPF is dropping some updates causing images to vanish.
    //                    //This is only reproducable when there are things in the map that are causing the refresh to 
    //                    //take longer. Like a lot of markers from SymbolSeries. This hack forces an arrange, which 
    //                    //fixes the problem.
    //                    if (Canvas.ActualWidth > 0 && Canvas.ActualHeight > 0)
    //                    {
    //                        Canvas.Arrange(new Rect(0, 0, Canvas.ActualWidth, Canvas.ActualHeight));
    //                    }
    //#endif
                    
                        Model.StartFade(downloadTile);
                    }
                }
            }

            BumpDownload();
        }




#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


        internal void SendToBackground(Image image)
        {
            Canvas.SetZIndex(image, 0);
        }

        internal void SendToForeground(Image image)
        {
            Canvas.SetZIndex(image, 1);
        }

        internal bool Ready()
        {
            if (Model.ContentPresenter == null)
            {
                return false;
            }

            return true;
        }

        internal void Defer(Action<bool> work)
        {
            if (Model.DeferralHandler != null)
            {
                Model.DeferralHandler.DeferredRefresh();
            }
            else
            {



                Model.Dispatcher.BeginInvoke((Action)(() => work(false)));

            }
        }

        private readonly DispatcherTimer FadeTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.05) };
        private DispatcherTimer SpringTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.05) };

        internal void StartSpringTimer()
        {
            if (!SpringTimer.IsEnabled)
            {
                SpringTimer.Start();
            }
        }

        internal void StopSpringTimer()
        {
            SpringTimer.Stop();
        }

        internal void StartFadeTimer()
        {
            if (!FadeTimer.IsEnabled)
            {
                FadeTimer.Start();
            }
        }

        internal void StopFadeTimer()
        {
            if (FadeTimer.IsEnabled)
            {
                FadeTimer.Stop();
            }
        }

        internal void DisableSprings()
        {
            if (SpringTimer.IsEnabled)
            {
                SpringTimer.Stop();
                Model.OnSpringsDisabled();
            }
        }

        internal void RefreshCompleted()
        {
            
        }

        internal void FadingChanged()
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