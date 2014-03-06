using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// Represents a button control that displays a hyperlink.
    /// </summary>
    /// <remarks>
    /// The base class for <see cref="XamTagCloudItem"/>.
    /// </remarks>
    public abstract class HyperlinkButton : ButtonBase
    {
        #region NavigateUri

        /// <summary>
        /// Identifies the <see cref="NavigateUri"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.Register("NavigateUri", typeof(Uri), typeof(HyperlinkButton), new PropertyMetadata(new PropertyChangedCallback(NavigateUriChanged)));

        /// <summary>
        /// Gets or sets the URI to navigate to when the <see cref="HyperlinkButton"/> is clicked. 
        /// </summary>
        public Uri NavigateUri
        {
            get { return (Uri)this.GetValue(NavigateUriProperty); }
            set { this.SetValue(NavigateUriProperty, value); }
        }

        private static void NavigateUriChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // NavigateUri 

        #region TargetName

        /// <summary>
        /// Identifies the <see cref="TargetName"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register("TargetName", typeof(string), typeof(HyperlinkButton), new PropertyMetadata(new PropertyChangedCallback(TargetNameChanged)));

        /// <summary>
        /// Gets or sets the name of the target to navigate to.
        /// </summary>
        public string TargetName
        {
            get { return (string)this.GetValue(TargetNameProperty); }
            set { this.SetValue(TargetNameProperty, value); }
        }

        private static void TargetNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // TargetName 
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