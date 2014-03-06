using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Windows.Editors
{
	#region DisplayValueSource Enum

	// SSP 3/10/09 Display Value Task
	// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
	// 
	/// <summary>
	/// Used for specifying the XamComboEditor's <see cref="XamComboEditor.DisplayValueSource"/> property.
	/// </summary>
	/// <seealso cref="XamComboEditor.DisplayValue"/>
	/// <seealso cref="XamComboEditor.DisplayValueSource"/>
	public enum DisplayValueSource
	{
		/// <summary>
		/// Editor's <see cref="TextEditorBase.DisplayText"/> property's value is used as 
		/// the content of the edit portion of the editor.
		/// </summary>
		DisplayText = 0,

		/// <summary>
		/// Editor's <see cref="ValueEditor.Value"/> property's value is used as the 
		/// content of the edit portion of the editor.
		/// </summary>
		Value = 1,

		// SSP 3/12/10 TFS27090
		// Added SelectedItem to the DisplayValueSource enum.
		// 
		/// <summary>
		/// Editor's <see cref="XamComboEditor.SelectedItem"/> property's value is used
		/// as the content of the edit portion of the editor.
		/// </summary>
		SelectedItem
	}

	#endregion // DisplayValueSource Enum

	#region DropDownButtonDisplayMode Enum

	/// <summary>
	/// Used for specifying the XamComboEditor's <see cref="XamComboEditor.DropDownButtonDisplayMode"/> property.
	/// </summary>
    /// <seealso cref="XamComboEditor.DropDownButtonDisplayMode"/>
    /// <seealso cref="XamDateTimeEditor.DropDownButtonDisplayMode"/>
    public enum DropDownButtonDisplayMode
	{
		/// <summary>
		/// Always display the button.
		/// </summary>
		Always = 0,

		/// <summary>
		/// Display the button when the mouse is over. Note that this option will always display
		/// the button when the editor is in edit mode.
		/// </summary>
		MouseOver = 1,

		/// <summary>
		/// Display the button when the editor is focus.
		/// </summary>
		Focused = 2,

		/// <summary>
		/// Display the button only when in edit mode.
		/// </summary>
		OnlyInEditMode = 3
	}

	#endregion // DropDownButtonDisplayMode Enum
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