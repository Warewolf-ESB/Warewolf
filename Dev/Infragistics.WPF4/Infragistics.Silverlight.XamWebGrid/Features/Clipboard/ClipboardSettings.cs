using System.Windows;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// An object that contains settings for performing Clipboard operations such as CopyToClipboard and Paste.
    /// </summary>
    public class ClipboardSettings : SettingsBase
    {
        #region AllowCopy

        /// <summary>
        /// Identifies the <see cref="AllowCopy"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowCopyProperty = DependencyProperty.Register("AllowCopy", typeof(bool), typeof(ClipboardSettings), new PropertyMetadata(false, new PropertyChangedCallback(AllowCopyChanged)));

        /// <summary>
        /// Gets/Sets whether hitting ctrl + c will copy selected rows or cells to the clipboard.
        /// </summary>
        public bool AllowCopy
        {
            get { return (bool)this.GetValue(AllowCopyProperty); }
            set { this.SetValue(AllowCopyProperty, value); }
        }

        private static void AllowCopyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // AllowCopy

        #region CopyOptions

        /// <summary>
        /// Identifies the <see cref="GridClipboardCopyOptions"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CopyOptionsProperty = DependencyProperty.Register("CopyOptions", typeof(GridClipboardCopyOptions), typeof(ClipboardSettings), new PropertyMetadata(GridClipboardCopyOptions.IncludeHeaders, new PropertyChangedCallback(CopyOptionsChanged)));

        /// <summary>
        /// Gets/Sets how data is copied to the clipbard.
        /// </summary>
        public GridClipboardCopyOptions CopyOptions
        {
            get { return (GridClipboardCopyOptions)this.GetValue(CopyOptionsProperty); }
            set { this.SetValue(CopyOptionsProperty, value); }
        }

        private static void CopyOptionsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // CopyOptions

        #region CopyType

        /// <summary>
        /// Identifies the <see cref="CopyType"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CopyTypeProperty = DependencyProperty.Register("CopyType", typeof(GridClipboardCopyType), typeof(ClipboardSettings), new PropertyMetadata(GridClipboardCopyType.Default, new PropertyChangedCallback(CopyTypeChanged)));

        /// <summary>
        /// Gets/Sets what type of data is copied to the clipboard.
        /// </summary>
        public GridClipboardCopyType CopyType
        {
            get { return (GridClipboardCopyType)this.GetValue(CopyTypeProperty); }
            set { this.SetValue(CopyTypeProperty, value); }
        }

        private static void CopyTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // CopyType

        #region AllowPaste

        /// <summary>
        /// Identifies the <see cref="AllowPaste"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowPasteProperty = DependencyProperty.Register("AllowPaste", typeof(bool), typeof(ClipboardSettings), new PropertyMetadata(false, new PropertyChangedCallback(AllowPasteChanged)));

        /// <summary>
        /// Gets/Sets whether hitting ctrl + v will paste the data from the clipboard.
        /// </summary>
        public bool AllowPaste
        {
            get { return (bool)this.GetValue(AllowPasteProperty); }
            set { this.SetValue(AllowPasteProperty, value); }
        }

        private static void AllowPasteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // AllowPaste       
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