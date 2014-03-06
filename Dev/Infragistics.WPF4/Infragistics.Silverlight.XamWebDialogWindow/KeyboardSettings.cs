using System.Windows;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// KeyboardSettings class contains members, related with keyboard navigation
    /// </summary>
    public class KeyboardSettings : DependencyObject
    {
        #region HorizontalStep

        /// <summary>
        /// Identifies the <see cref="HorizontalStep"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalStepProperty = DependencyProperty.Register("HorizontalStep", typeof(double), typeof(KeyboardSettings), new PropertyMetadata(3.0));

       /// <summary>
		/// Gets/Sets how many pixels will be moved horizontally when the Left or Right arrow key is pressed while the <see cref="XamDialogWindow"/> is active.
       /// </summary>
        public double HorizontalStep
        {
            get { return (double)this.GetValue(HorizontalStepProperty); }
            set { this.SetValue(HorizontalStepProperty, value); }
        }

        #endregion // HorizontalStep

        #region AllowKeyboardNavigation

        /// <summary>
        /// Identifies the <see cref="AllowKeyboardNavigation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowKeyboardNavigationProperty = DependencyProperty.Register("AllowKeyboardNavigation", typeof(bool), typeof(KeyboardSettings), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is key movable.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is key movable; otherwise, <c>false</c>.
        /// </value>
        public bool AllowKeyboardNavigation
        {
            get { return (bool)this.GetValue(AllowKeyboardNavigationProperty); }
            set { this.SetValue(AllowKeyboardNavigationProperty, value); }
        }

        #endregion // AllowKeyboardNavigation

        #region VerticalStep

        /// <summary>
        /// Identifies the <see cref="VerticalStep"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VerticalStepProperty = DependencyProperty.Register("VerticalStep", typeof(double), typeof(KeyboardSettings), new PropertyMetadata(3.0));

		/// <summary>
		/// Gets/Sets how many pixels will be moved vertically when the Up or Down arrow key is pressed while the <see cref="XamDialogWindow"/> is active.
		/// </summary>
        public double VerticalStep
        {
            get { return (double)this.GetValue(VerticalStepProperty); }
            set { this.SetValue(VerticalStepProperty, value); }
        }

        #endregion // VerticalStep
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