using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Infragistics.Controls.Maps
{
    
    /// <summary>
    /// Represents a Cloud Made Image Tile Source
    /// </summary>
    public sealed class CloudMadeTileSource : MapTileSource
    {
        private const string TilePathMapnik = "http://{S}.tile.cloudmade.com/{K}/{P}/256/{Z}/{X}/{Y}.png";
        private readonly Random _random;

        /// <summary>
        /// Constructs a new custom tile source from a CloudMade image set.
        /// </summary>
        public CloudMadeTileSource()
            : base(134217728, 134217728, 256, 256, 0)
        {
            

            _random = new Random();
        }

        #region Key

        private const string KeyPropertyName = "Key";

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Key"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register(KeyPropertyName, typeof(string), typeof(CloudMadeTileSource), new PropertyMetadata(null));

        #endregion

        #region Parameter

        private const string ParameterPropertyName = "Parameter";
        /// <summary>
        /// The CloudMade parameter, which can be used to select the style of tile images to return.
        /// </summary>
        public string Parameter
        {
            get { return (string)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Parameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(ParameterPropertyName, typeof(string), typeof(CloudMadeTileSource), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// Adds the URI for the specified tile to the given list.
        /// </summary>
        /// <param name="tileLevel">The tile's hierarchy level.</param>
        /// <param name="tilePositionX">The tile's horizontal position.</param>
        /// <param name="tilePositionY">The tile's vertical position.</param>
        /// <param name="tileImageLayerSources">The output list of tile URIs.</param>
        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, IList<object> tileImageLayerSources)
        {
            int zoom = tileLevel - 8;

            if (zoom > 0)
            {
                string[] servers = new string[] { "a", "b", "c" };
                string uriString = TilePathMapnik;

                uriString = uriString.Replace("{S}", servers[_random.Next(servers.Length)]);
                uriString = uriString.Replace("{K}", Key == null ? "" : Key);
                uriString = uriString.Replace("{P}", Parameter == null ? "" : Parameter);
                uriString = uriString.Replace("{Z}", zoom.ToString());
                uriString = uriString.Replace("{X}", tilePositionX.ToString());
                uriString = uriString.Replace("{Y}", tilePositionY.ToString());

                tileImageLayerSources.Add(new Uri(uriString));
            }
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