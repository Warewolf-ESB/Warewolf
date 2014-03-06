using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Effects;

namespace Infragistics.Windows.Controls.Markup
{
	/// <summary>
	/// Implements a markup extension that creates a <see cref="OuterGlowBitmapEffect"/>
	/// </summary>
	[MarkupExtensionReturnType(typeof(OuterGlowBitmapEffect))]
    public class SafeOuterGlowExtension : MarkupExtension
	{
		#region Member Variables

		private Color _glowColor = Colors.Gold;
		private double _glowSize = 5d;
		private double _noise = 0d;
		private double _opacity = 1d;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SafeOuterGlowExtension"/>
		/// </summary>
		public SafeOuterGlowExtension()
		{
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns or sets the color of the halo glow.
		/// </summary>
		/// <value>
		/// <p class="body">The color of the glow. The default value is Gold.</p>
		/// </value>
		public Color GlowColor
		{
			get { return this._glowColor; }
			set { this._glowColor = value; }
		}

		/// <summary>
		/// Returns or sets the thickness of the halo glow.
		/// </summary>
		/// <value>
		/// <p class="body">The thickness of the glow. The valid is range is from 1 through 199. The default is 5.</p>
		/// </value>
		public double GlowSize
		{
			get { return this._glowSize; }
			set { this._glowSize = value; }
		}

		/// <summary>
		/// Returns or sets the graininess of the halo glow.
		/// </summary>
		/// <value>
		/// <p class="body">The noise level of the glow. A value of 0 is consider no noise and a value of 1 indicates maximum noise. The default is 0.</p>
		/// </value>
		public double Noise
		{
			get { return this._noise; }
			set { this._noise = value; }
		}

		/// <summary>
		/// Returns or sets the transparency of the halo glow.
		/// </summary>
		/// <value>
		/// <p class="body">The opacity/transparency of the glow. A value of 0 is completely transparent and a value of 1 
		/// is completely opaque. The default is 1.</p>
		/// </value>
		public double Opacity
		{
			get { return this._opacity; }
			set { this._opacity = value; }
		} 
		#endregion //Properties

		#region Base class overrides
		/// <summary>
		/// Returns an <see cref="OuterGlowBitmapEffect"/> based on the properties of the markup extension.
		/// </summary>
		/// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
		/// <returns>An <see cref="OuterGlowBitmapEffect"/> or null if one could not be created.</returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (BrowserInteropHelper.IsBrowserHosted)
				return null;

			try
			{
				OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect();
				effect.GlowColor = this._glowColor;
				effect.GlowSize = this._glowSize;
				effect.Noise = this._noise;
				effect.Opacity = this._opacity;

				return effect;
			}
			catch
			{
				return null;
			}
		} 
		#endregion //Base class overrides
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