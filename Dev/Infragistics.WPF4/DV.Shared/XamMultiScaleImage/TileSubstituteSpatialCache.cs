using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Infragistics.Controls.Maps
{
    internal class TileSubstituteSpatialCache
    {
        internal struct TileData
        {
            public TileData(Image image, bool reloadTile) : this()
            {
                Image = image;
                ReloadTile = reloadTile;
            }

            public Image Image { get; set; }
            public bool ReloadTile { get; set; }
        }

        private readonly SpatialCache<BitmapImage> _cache;
        private readonly int _tileWidth;
        private readonly int _tileHeight;

        public TileSubstituteSpatialCache(SpatialCache<BitmapImage> cache, int tileWidth, int tileHeight)
        {
            _cache = cache;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        public int Count { get { return _cache.Count; } }

        public void AddTile(int level, int tileX, int tileY, BitmapImage bitmapImage)
        {
            _cache.AddItem(level, tileX, tileY, bitmapImage);
        }

        public bool ContainsTile(int level, int tileX, int tileY)
        {
            return _cache.ContainsItem(level, tileX, tileY);
        }


        public TileData GetTileOrSubstitute(int level, int tileX, int tileY)
        {
            BitmapImage bitmapImage = null;
            Image image = null;

            var offset = new List<Point>(3);

            while (level > 0)
            {
                bitmapImage = _cache.GetItem(level, tileX, tileY);

                if (bitmapImage != null)
                {                    
                    break;
                }

                offset.Add(new Point(Convert.ToInt32(tileX % 2.0), Convert.ToInt32(tileY % 2.0)));

                level--;
                tileX = Convert.ToInt32(tileX / 2);
                tileY = Convert.ToInt32(tileY / 2);
            }            

            if (bitmapImage != null)
            {
                if (offset.Count > 0)
                {
                    var scale = Math.Pow(2, offset.Count);

                    var offsetX = 0.0;
                    var offsetY = 0.0;

                    for (int i = offset.Count - 1, j = 1; i >= 0; i--, j++)
                    {
                        offsetX += offset[i].X * (_tileWidth / Math.Pow(2.0, j));
                        offsetY += offset[i].Y * (_tileHeight / Math.Pow(2.0, j));
                    }

                    var tg = new TransformGroup();
                    tg.Children.Add(new ScaleTransform { ScaleX = scale, ScaleY = scale });
                    tg.Children.Add(new TranslateTransform { X = -offsetX, Y = -offsetY });

                    var cropWidth = Convert.ToInt32(_tileWidth / Math.Pow(2.0, offset.Count));
                    var cropHeight = Convert.ToInt32(_tileWidth / Math.Pow(2.0, offset.Count));

                    if (cropWidth > 0 || cropHeight > 0)
                    {
                        var cb = new CroppedBitmap(bitmapImage, new Int32Rect(Convert.ToInt32(offsetX), Convert.ToInt32(offsetY), cropWidth, cropHeight));
                        var tb = new TransformedBitmap(cb, new ScaleTransform { ScaleX = scale, ScaleY = scale });

                        image = new Image { Source = tb };
                    }         
                }
                else
                {                    
                    image = new Image { Source = bitmapImage };                    
                }                       
            }

            var loadTile = offset.Count > 0 || image == null;

            return new TileData(image, loadTile);
        }

        public void RemoveTileAt(int level, int tileX, int tileY)
        {
            _cache.RemoveItemAt(level, tileX, tileY);
        }

        public void RemoveTilesAtLevel(int level)
        {
            _cache.RemoveItemsAtLevel(level);
        }

        public void ClearLevel(int level, int leftTile, int rightTile, int topTile, int bottomTile)
        {
            _cache.ClearLevel(level, leftTile, rightTile, topTile, bottomTile);    
        }

        public void Clear()
        {
            _cache.Clear();
        }

        internal void Reset()
        {
            _cache.Reset();
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