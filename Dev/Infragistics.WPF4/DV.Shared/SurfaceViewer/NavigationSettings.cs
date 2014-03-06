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

namespace Infragistics.Controls
{

    /// <summary>
    /// Represens the navigations settings.
    /// </summary>
    public class NavigationSettings : DependencyObjectNotifier
    {
        #region Properties

        #region Public
        #region AllowPan

        /// <summary>
        /// Identifies the <see cref="AllowPan"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowPanProperty = DependencyProperty.Register("AllowPan", typeof(Boolean), typeof(NavigationSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowPanChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the user can move the <see cref="SurfaceViewer"/>.
        /// </summary>
        /// <value><c>true</c> if pan is allowed; otherwise, <c>false</c>.</value>
        public Boolean AllowPan
        {
            get { return (Boolean)this.GetValue(AllowPanProperty); }
            set { this.SetValue(AllowPanProperty, value); }
        }

        private static void AllowPanChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NavigationSettings settings = obj as NavigationSettings;
            if (settings != null)
            {
                settings.OnPropertyChanged("AllowPan");
            }
        }

        #endregion // AllowPan

        #region AllowZoom

        /// <summary>
        /// Identifies the <see cref="AllowZoom"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowZoomProperty = DependencyProperty.Register("AllowZoom", typeof(Boolean), typeof(NavigationSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowZoomChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether the user can zoom the <see cref="SurfaceViewer"/>.
        /// </summary>
        /// <value><c>true</c> if the zoom is allowed; otherwise, <c>false</c>.</value>
        public Boolean AllowZoom
        {
            get { return (Boolean)this.GetValue(AllowZoomProperty); }
            set { this.SetValue(AllowZoomProperty, value); }
        }

        private static void AllowZoomChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NavigationSettings settings = obj as NavigationSettings;
            if (settings != null)
            {
                settings.OnPropertyChanged("AllowZoom");
            }
        }

        #endregion // AllowZoom

        #endregion
        #endregion
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