using System;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;

namespace Infragistics.Controls.Maps
{

    /// <summary>
    /// Represents a BingMaps ImageTileSource
    /// </summary>
    public sealed class BingMapsTileSource : MapTileSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BingMapsTileSource"/> class.
        /// </summary>
        public BingMapsTileSource()
            : base(256 << 22, 256 << 22, 256, 256, 0)
        {

        }        

        /// <summary>
        /// Initializes a new instance of the <see cref="BingMapsTileSource"/> class.
        /// </summary>        
        /// <param name="tilePath">Tile image Uri.</param>
        /// <param name="subDomains">Collection of image Uri subdomains.</param>
        public BingMapsTileSource(string tilePath, ObservableCollection<string> subDomains)
            : base(256 << 22, 256 << 22, 256, 256, 0)
        {
            this.TilePath = tilePath;
            this.SubDomains = subDomains;
        }       

        /// <summary>
        /// Identifies the TilePath property.
        /// </summary>
        public static readonly DependencyProperty TilePathProperty = DependencyProperty.Register("TilePath", typeof(string), typeof(BingMapsTileSource), new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the map tile image uri.
        /// </summary>
        public string TilePath
        {
            get { return (string)GetValue(TilePathProperty); }
            set { SetValue(TilePathProperty, value); }
        }

        /// <summary>
        /// Identifies the SubDomainsProperty.
        /// </summary>
        public static readonly DependencyProperty SubDomainsProperty = DependencyProperty.Register("SubDomains", typeof(ObservableCollection<string>), typeof(BingMapsTileSource), new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the collection of image uri subdomains.
        /// </summary>
        public ObservableCollection<string> SubDomains
        {
            get { return (ObservableCollection<string>)GetValue(SubDomainsProperty); }
            set { SetValue(SubDomainsProperty, value); }
        }


        /// <summary>
        /// Identifies the CultureName property.
        /// </summary>
        public static readonly DependencyProperty CultureNameProperty = DependencyProperty.Register("CultureName", typeof(string), typeof(BingMapsTileSource), new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the culture name for this tile source.
        /// </summary>
        public string CultureName
        {
            get { return (string)GetValue(CultureNameProperty); }
            set { SetValue(CultureNameProperty, value); }
        }

        /// <summary>
        /// Gets a collection of the tiles.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-coordinate position of the tile.</param>
        /// <param name="tilePositionY">Y-coordinate position of the tile.</param>
        /// <param name="tileImageLayerSources">Source of the tile image layer.</param>
        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            ValidateTileSourceProperties();

            if (this.TilePath == null)
            {
                return;
            }

            tileLevel -= 8;

            if (tileLevel > 0)
            {
                string quadKey = GetQuadKey(tileLevel, tilePositionX, tilePositionY);
                string uriString = this.TilePath;

                uriString = uriString.Replace("{culture}", this.CultureName);
                uriString = uriString.Replace("{quadkey}", quadKey);



                var index = Convert.ToInt32(quadKey.Substring(quadKey.Length - 1, 1));

                uriString = uriString.Replace("{subdomain}", this.SubDomains[index]);
                    
                tileImageLayerSources.Add(new Uri(uriString));
            }
        }

        private void ValidateTileSourceProperties()
        {       

#pragma warning disable 436
            if (ReadLocalValue(TilePathProperty) == DependencyProperty.UnsetValue)
            {
                throw new Exception(SR.GetString("Exception_BingMapsTilePathNotSet"));
            }

            if (ReadLocalValue(SubDomainsProperty) == DependencyProperty.UnsetValue)
            {
                throw new Exception(SR.GetString("Exception_BingMapsSubDomainNotSet"));
            }
#pragma warning restore 436
            if (ReadLocalValue(CultureNameProperty) == DependencyProperty.UnsetValue)
            {
                this.CultureName = Thread.CurrentThread.CurrentCulture.Name;
            }

        }

        /// <summary>
        /// Gets the quadkey from tile position and level.
        /// </summary>
        /// <param name="tileLevel">Level of the tile.</param>
        /// <param name="tilePositionX">X-coordinate position of the tile.</param>
        /// <param name="tilePositionY">Y-coordinate position of the tile.</param>
        private string GetQuadKey(int tileLevel, int tilePositionX, int tilePositionY)
        {
            StringBuilder quadKey = new StringBuilder();

            for (int i = tileLevel; i > 0; --i)
            {
                char digit = '0';
                int mask = 1 << (i - 1);

                if ((tilePositionX & mask) != 0)
                {
                    digit++;
                }

                if ((tilePositionY & mask) != 0)
                {
                    digit++;
                    digit++;
                }

                quadKey.Append(digit);
            }

            return quadKey.ToString();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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