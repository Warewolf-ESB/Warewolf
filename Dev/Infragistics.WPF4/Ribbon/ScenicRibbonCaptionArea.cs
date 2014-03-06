using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
    // JJD 4/29/10 - NA 2010 volumne 2 - Scenic Ribbon
    /// <summary>
    /// A element that is used as the background for the window caption area when the Theme is set to 'Scenic'
    /// </summary>
    public class ScenicRibbonCaptionArea : Control
    {
		#region Constructor

		/// <summary>
        /// Initializes a new instance of a <see cref="ScenicRibbonCaptionArea"/> class.
		/// </summary>
		public ScenicRibbonCaptionArea()
		{
		}

        static ScenicRibbonCaptionArea()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScenicRibbonCaptionArea), new FrameworkPropertyMetadata(typeof(ScenicRibbonCaptionArea)));
         }

		#endregion //Constructor
    
		#region Base class overrides

			#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the element has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            this.SyncRoundedCorners();
		}

			#endregion //OnApplyTemplate
    
        #endregion //Base class overrides

        #region Properties

            #region Public Properties

                #region IsWithinRibbonWindow

        /// <summary>
        /// Identifies the <see cref="IsWithinRibbonWindow"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsWithinRibbonWindowProperty = DependencyProperty.Register("IsWithinRibbonWindow",
            typeof(bool), typeof(ScenicRibbonCaptionArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        private static void OnIsWithinRibbonWindowChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScenicRibbonCaptionArea srca = target as ScenicRibbonCaptionArea;

            if (srca != null)
                srca.SyncRoundedCorners();
        }
        /// <summary>
        /// Returns true if the <see cref="XamRibbon"/> is being hosted within a <see cref="XamRibbonWindow"/>, otherwise returns false.
        /// </summary>
        /// <seealso cref="IsWithinRibbonWindowProperty"/>
        [Bindable(true)]
        public bool IsWithinRibbonWindow
        {
            get
            {
                return (bool)this.GetValue(ScenicRibbonCaptionArea.IsWithinRibbonWindowProperty);
            }
            set
            {
                this.SetValue(ScenicRibbonCaptionArea.IsWithinRibbonWindowProperty, value);
            }
        }

                #endregion //IsWithinRibbonWindow

                #region RoundCorners

        private static readonly DependencyPropertyKey RoundCornersPropertyKey =
            DependencyProperty.RegisterReadOnly("RoundCorners",
            typeof(bool), typeof(ScenicRibbonCaptionArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="RoundCorners"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RoundCornersProperty =
            RoundCornersPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if the top left and top right of the caption area should be rounded.
        /// </summary>
        /// <seealso cref="RoundCornersProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        public bool RoundCorners
        {
            get
            {
                return (bool)this.GetValue(ScenicRibbonCaptionArea.RoundCornersProperty);
            }
        }

                #endregion //RoundCorners

            #endregion //Public Properties	
    
        #endregion //Properties

        #region Methods

            #region Private Methods

                #region SyncRoundedCorners

        private void SyncRoundedCorners()
        {
            bool roundCorners = this.IsWithinRibbonWindow;

            if (roundCorners == true)
            {
                XamRibbon ribbon = XamRibbon.GetRibbon(this);

                if (ribbon != null)
                {
                    IRibbonWindow rw = ribbon.RibbonWindow;

                    if (rw != null)
                        roundCorners = !rw.IsClassicOSThemeInternal;
                }
            }

            this.SetValue(RoundCornersPropertyKey, KnownBoxes.FromValue(roundCorners));
        }

                #endregion //SyncRoundedCorners

            #endregion //Private Methods	
    
        #endregion //Methods	
           
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