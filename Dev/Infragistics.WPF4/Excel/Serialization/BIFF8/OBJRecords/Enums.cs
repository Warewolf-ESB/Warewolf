using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	internal enum OBJRecordType : ushort
	{
		// MD 10/30/11 - TFS90733
		// This is no longer needed.
		//End								= 0x00,

		Macro							= 0x04,
		Button							= 0x05,
		GroupMarker						= 0x06,
		ClipboardFormat					= 0x07,
		PictureOptionFlags				= 0x08,
		PictureFormulaStyleMacro		= 0x09,
		CheckBoxLink					= 0x0A,
		RadioButton						= 0x0B,
		ScrollBar						= 0x0C,
		Note							= 0x0D,
		ScrollBarFormulaStyleMacro		= 0x0E,
		GroupBoxData					= 0x0F,
		EditControlData					= 0x10,
		RadioButtonData					= 0x11,
		CheckBoxData					= 0x12,
		ListBoxData						= 0x13,
		CheckBoxLinkFormulaStyleMacro	= 0x14,
		CommonObjectData				= 0x15,
	}

	internal enum ObjectType : ushort
	{
		Group					= 0x00,
		Line					= 0x01,
		Rectangle				= 0x02,
		Oval					= 0x03,
		Arc						= 0x04,
		Chart					= 0x05,
		Text					= 0x06,
		Button					= 0x07,
		Picture					= 0x08,
		Polygon					= 0x09,
		CheckBox				= 0x0B,
		OptionButton			= 0x0C,
		EditBox					= 0x0D,
		Label					= 0x0E,
		DialogBox				= 0x0F,
		Spinner					= 0x10,
		ScrollBar				= 0x11,
		ListBox					= 0x12,
		GroupBox				= 0x13,
		ComboBox				= 0x14,
		Comment					= 0x19,
		MicrosoftOfficeDrawing	= 0x1E
	}

	// MD 10/30/11 - TFS90733
	internal enum PictureClipboardFormat : ushort
	{
		Emf = 0x0002,
		Bitmap = 0x0009,
		Unspecified = 0xFFFF,
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