using System.Windows;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// CustomCursors class contains members, related with custom cursors for 
    /// </summary>
    public class CustomCursors : DependencyObject
    {
        #region DiagonalResizeCursor

        /// <summary>
        /// Identifies the <see cref="DiagonalResizeCursor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DiagonalResizeCursorProperty = DependencyProperty.Register("DiagonalResizeCursor", typeof(DataTemplate), typeof(CustomCursors), null);

        /// <summary>
        /// Gets or sets the horizontal resize cursor.
        /// </summary>
        /// <value>The horizontal resize cursor.</value>
        public DataTemplate DiagonalResizeCursor
        {
            get { return (DataTemplate)this.GetValue(DiagonalResizeCursorProperty); }
            set { this.SetValue(DiagonalResizeCursorProperty, value); }
        }

        #endregion // DiagonalResizeCursor

        #region HorizontalResizeCursor

        /// <summary>
        /// Identifies the <see cref="HorizontalResizeCursor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalResizeCursorProperty = DependencyProperty.Register("HorizontalResizeCursor", typeof(DataTemplate), typeof(CustomCursors), null);

        /// <summary>
        /// Gets or sets the horizontal resize cursor.
        /// </summary>
        /// <value>The horizontal resize cursor.</value>
        public DataTemplate HorizontalResizeCursor
        {
            get { return (DataTemplate)this.GetValue(HorizontalResizeCursorProperty); }
            set { this.SetValue(HorizontalResizeCursorProperty, value); }
        }

        #endregion // HorizontalResizeCursor

        #region RightDiagonalResizeCursor

        /// <summary>
        /// Identifies the <see cref="RightDiagonalResizeCursor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty RightDiagonalResizeCursorProperty = DependencyProperty.Register("RightDiagonalResizeCursor", typeof(DataTemplate), typeof(CustomCursors), null);

        /// <summary>
        /// Gets or sets the horizontal resize cursor.
        /// </summary>
        /// <value>The horizontal resize cursor.</value>
        public DataTemplate RightDiagonalResizeCursor
        {
            get { return (DataTemplate)this.GetValue(RightDiagonalResizeCursorProperty); }
            set { this.SetValue(RightDiagonalResizeCursorProperty, value); }
        }

        #endregion // DiagonalResizeCursor

        #region VerticalResizeCursor

        /// <summary>
        /// Identifies the <see cref="VerticalResizeCursor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VerticalResizeCursorProperty = DependencyProperty.Register("VerticalResizeCursor", typeof(DataTemplate), typeof(CustomCursors), null);

        /// <summary>
        /// Gets or sets the vertical resize cursor.
        /// </summary>
        /// <value>The vertical resize cursor.</value>
        public DataTemplate VerticalResizeCursor
        {
            get { return (DataTemplate)this.GetValue(VerticalResizeCursorProperty); }
            set { this.SetValue(VerticalResizeCursorProperty, value); }
        }

        #endregion // VerticalResizeCursor
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