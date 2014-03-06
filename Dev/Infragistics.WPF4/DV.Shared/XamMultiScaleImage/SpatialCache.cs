using System;
using System.Collections.Generic;

namespace Infragistics.Controls.Maps
{
    internal class SpatialCache<T>
    {
        private readonly Dictionary<string, T>[] _images;

        public SpatialCache(int maxLevel)
        {
            _images = new Dictionary<string, T>[maxLevel + 1];
        }

        public int Count { get; private set; }

        public void AddItem(int level, int tileX, int tileY, T image)
        {
            if (_images[level] == null)
            {
                _images[level] = new Dictionary<string, T>();
            }

            var key = TilesIndexesToKey(tileX, tileY);

            if (_images[level].ContainsKey(key))
            {
                _images[level][key] = image;
            }
            else
            {
                _images[level].Add(key, image);

                Count++;
            }
        }

        public bool ContainsItem(int level, int tileX, int tileY)
        {
            if (level >= _images.Length || _images[level] == null)
            {
                return false;
            }

            return _images[level].ContainsKey(TilesIndexesToKey(tileX, tileY));
        }

        public T GetItem(int level, int tileX, int tileY)
        {
            if (level < _images.Length)
            {
                T bitmapImage;

                if (_images[level] != null && _images[level].TryGetValue(TilesIndexesToKey(tileX, tileY), out bitmapImage))
                {
                    return bitmapImage;
                }
            }

            return default(T);
        }

        public void RemoveItemAt(int level, int tileX, int tileY)
        {
            if (_images[level] == null)
            {
                return;
            }

            var key = TilesIndexesToKey(tileX, tileY);

            _images[level].Remove(key);

            Count--;
        }

        public void RemoveItemsAtLevel(int level)
        {
            if (_images[level] == null)
            {
                return;
            }

            var n = _images[level].Count;

            _images[level].Clear();

            Count -= n;
        }

        public void Reset()
        {
            foreach (var level in _images)
            {
                if (level != null)
                {
                    level.Clear();
                }
            }

            Count = 0;
        }

        public void ClearLevel(int level, int leftTile, int rightTile, int topTile, int bottomTile)
        {
            if (level >= _images.Length || _images[level] == null)
            {
                return;
            }

            var tilesToRemove = new List<string>();

            foreach (var keyValue in _images[level])
            {
                var tilesXYindexes = KeyToTilesIndexes(keyValue.Key);
                var x = tilesXYindexes.Item1;
                var y = tilesXYindexes.Item2;

                if (x < leftTile || x > rightTile || y < topTile || y > bottomTile)
                {
                    tilesToRemove.Add(keyValue.Key);
                }
            }

            foreach (var key in tilesToRemove)
            {
                _images[level].Remove(key);
                Count--;
            }
        }

        public void Clear()
        {
            // 1st, 2nd and 3rd levels are not cleared, their overhead is small

            for (int i = 4; i < _images.Length; i++)
            {
                if (_images[i] != null)
                {
                    var n = _images[i].Count;

                    _images[i].Clear();

                    Count -= n;
                }
            }
        }

        private static string TilesIndexesToKey(int tileX, int tileY)
        {
            return tileX + "x" + tileY;
        }

        private static Tuple<int, int> KeyToTilesIndexes(string key)
        {
            var tiles = key.Split(new[] { 'x' });

            return new Tuple<int, int>(Convert.ToInt32(tiles[0]), Convert.ToInt32(tiles[1]));
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