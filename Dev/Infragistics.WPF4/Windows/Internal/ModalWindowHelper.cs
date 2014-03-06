using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Infragistics.Windows.Internal
{
	// AS 6/22/11 TFS74670
	// Steve added this class so the xamRibbonWindow has a way of detecting that there is an open modal
	// xamDialogWindow.
	//
	/// <summary>
	/// Internal class, used to notify a window that a modal dialog is being displayed over it.
	/// </summary>
	internal class ModalWindowHelper : DependencyObject
	{
		#region IsModalWindowOpen
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		#endregion // IsModalWindowOpen

		// AS 6/22/12 TFS115111
		#region ModalDialogWindowCount

		/// <summary>
		/// Identifies the ModalDialogWindowCount attached dependency property
		/// </summary>
		/// <seealso cref="GetModalDialogWindowCount"/>
		/// <seealso cref="SetModalDialogWindowCount"/>
		public static readonly DependencyProperty ModalDialogWindowCountProperty = DependencyProperty.RegisterAttached("ModalDialogWindowCount",
			typeof(int), typeof(ModalWindowHelper),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits)
			);

		/// <summary>
		/// Gets the value of the attached ModalDialogWindowCount DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="ModalDialogWindowCountProperty"/>
		/// <seealso cref="SetModalDialogWindowCount"/>
		public static int GetModalDialogWindowCount(DependencyObject d)
		{
			return (int)d.GetValue(ModalWindowHelper.ModalDialogWindowCountProperty);
		}

		/// <summary>
		/// Sets the value of the attached ModalDialogWindowCount DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="ModalDialogWindowCountProperty"/>
		/// <seealso cref="GetModalDialogWindowCount"/>
		public static void SetModalDialogWindowCount(DependencyObject d, int value)
		{
			d.SetValue(ModalWindowHelper.ModalDialogWindowCountProperty, value);
		}

		#endregion //ModalDialogWindowCount
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