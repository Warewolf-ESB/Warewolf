using System;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Infragistics.Windows.Controls.Markup
{
    /// <summary>
    /// Implements a markup extension that creates a <see cref="DropShadowEffect"/>
    /// </summary>
    public class SafeDropShadowEffectExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets a value that indicates the radius of the shadow's blur effect. 
        /// </summary>
        public double BlurRadius { get; set; }

        /// <summary>
        /// Gets or sets the color of the drop shadow. 
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the direction of the drop shadow. 
        /// </summary>
        public double Direction { get; set; }

        /// <summary>
        /// Gets or sets the opacity of the drop shadow.
        /// </summary>        
        public double Opacity { get; set; }

        /// <summary>
        /// Gets or sets the distance of the drop shadow below the texture. 
        /// </summary>
        public double ShadowDepth { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the system renders the drop shadow with emphasis on speed or quality. 
        /// </summary>
        public RenderingBias RenderingBias { get; set; }

        /// <summary>
        /// Returns an <see cref="DropShadowEffect"/> based on the properties of the markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>An <see cref="DropShadowEffect"/> or null if one could not be created.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
                return null;
            
            try
            {
                var effect = new DropShadowEffect();
                
                effect.BlurRadius = BlurRadius;
                effect.Color = Color;
                effect.Direction = Direction;
                effect.Opacity = Opacity;
                effect.RenderingBias = RenderingBias;
                effect.ShadowDepth = ShadowDepth;
                
                return effect;
            }
            catch
            {
                return null;
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