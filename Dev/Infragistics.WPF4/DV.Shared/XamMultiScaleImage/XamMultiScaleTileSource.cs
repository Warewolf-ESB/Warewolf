using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace Infragistics.Controls.Maps
{
    
    ///<summary>
    /// Used to specify the source of Multi scale images
    ///</summary>
    public abstract class XamMultiScaleTileSource : DependencyObject
    {
        /// <summary>
        /// XamMultiScaleTileSource constructor.
        /// </summary>
        /// <param name="imageWidth">The width of the Deep Zoom image.</param>
        /// <param name="imageHeight">The height of the Deep Zoom image.</param>
        /// <param name="tileWidth">The width of the tiles in the Deep Zoom image.</param>
        /// <param name="tileHeight">The height of the tiles in the Deep Zoom image.</param>
        /// <param name="tileOverlap">How much the tiles in the Deep Zoom image overlap.</param>
        protected XamMultiScaleTileSource(long imageWidth, long imageHeight, int tileWidth, int tileHeight, int tileOverlap)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            TileOverlap = tileOverlap;
        }
        /// <summary>
        /// The width of the Deep Zoom image.
        /// </summary>
        public long ImageWidth { get; private set; }
        /// <summary>
        /// The height of the Deep Zoom image.
        /// </summary>
        public long ImageHeight { get; private set; }
        /// <summary>
        /// The width of the tiles in the Deep Zoom image.
        /// </summary>
        public int TileWidth { get; private set; }
        /// <summary>
        /// The height of the tiles in the Deep Zoom image. 
        /// </summary>
        public int TileHeight { get; private set; }
        /// <summary>
        /// How much the tiles in the Deep Zoom image overlap.
        /// </summary>
        public int TileOverlap { get; private set; }

        internal XamMultiScaleImage MultiScaleImage;

        internal Uri GetTileUri(int tileLevel, int tilePositionX, int tilePositionY)
        {
            var tileImageLayerSources = new List<object>();

            GetTileLayers(tileLevel, tilePositionX, tilePositionY, tileImageLayerSources);

            Uri uri = null;

            if (tileImageLayerSources.Count > 0)
            {
                uri = tileImageLayerSources[0] as Uri;    
            }

            return uri;
        }
        /// <summary>
        /// Gets a collection of the tiles.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-coordinate position of the tile.</param>
        /// <param name="tilePositionY">Y-coordinate position of the tile.</param>
        /// <param name="tileImageLayerSources">Source of the tile image layer.</param>
        protected abstract void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources);

        /// <summary>
        /// Invalidates specified tile layers.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tilePositionX">The tile position X.</param>
        /// <param name="tilePositionY">The tile position Y.</param>
        /// <param name="tileLayer">The tile layer.</param>
        protected void InvalidateTileLayer(int level, int tilePositionX, int tilePositionY, int tileLayer)
        {
            if (MultiScaleImage != null)
            {
                MultiScaleImage.InvalidateTileLayer(level, tilePositionX, tilePositionY, tileLayer);
            }
        }


        /// <summary>
        /// Can be overridden to indicate whether images returned over the wire are valid to display or not.
        /// </summary>
        /// <param name="stream">The image data to validate.</param>
        /// <returns>True if the image data is valid for display.</returns>
        protected internal virtual bool IsImageStreamValid(Stream stream)
        {
            return true;
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