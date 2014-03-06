using System;
using System.Windows.Media;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
    #region SelectedColorItemChangedEventArgs
    /// <summary>
    /// An event argument for the tracking when a <see cref="ColorItem"/> value will changing.
    /// </summary>
    public class SelectedColorItemChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the orignial <see cref="ColorItem"/> object.
        /// </summary>
        public ColorItem OriginalColorItem { get; protected internal set; }

        /// <summary>
        /// Gets the new <see cref="ColorItem"/> object.
        /// </summary>
        public ColorItem NewColorItem { get; protected internal set; }
    }
    #endregion // SelectedColorItemChangedEventArgs

    #region SelectedColorChangedEventArgs
    /// <summary>
    /// An event argument for the tracking when a <see cref="Color"/> value will changing.
    /// </summary>
    public class SelectedColorChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the orignial <see cref="Color"/> object.
        /// </summary>
        public Color? OriginalColor { get; protected internal set; }

        /// <summary>
        /// Gets the new <see cref="Color"/> object.
        /// </summary>
        public Color? NewColor { get; protected internal set; }
    }
    #endregion // SelectedColorChangedEventArgs
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