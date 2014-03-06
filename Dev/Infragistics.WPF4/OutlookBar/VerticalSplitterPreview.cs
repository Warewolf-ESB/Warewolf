using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// A Control derived class used to represent the vertical splitter of <see cref="XamOutlookBar"/> that is used during a drag operation to represent the preview of where the splitter will be positioned.
    /// </summary>
	/// <remarks>The VerticalSplitterPreview is only used when the <see cref="XamOutlookBar.VerticalSplitterResizeMode"/> is set to <see cref="SplitterResizeMode"/>.Deferred.</remarks>
	/// <seealso cref="XamOutlookBar.VerticalSplitterPreviewStyleKey"/>
	/// <seealso cref="XamOutlookBar.VerticalSplitterResizeMode"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class VerticalSplitterPreview : Control
    {
        #region Constructors
        static VerticalSplitterPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VerticalSplitterPreview),
                new FrameworkPropertyMetadata(typeof(VerticalSplitterPreview)));
        }

		/// <summary>
		/// Initialize a new VerticalSplitterPreview
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/>. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public VerticalSplitterPreview()
		{
		}
        #endregion //Constructors
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