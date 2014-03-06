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
	#region EditOrientation

	/// <summary>
	/// Enum associated with the <see cref="EditSectionBase.Orientation"/> property of the <see cref="EditSectionBase"/> class.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to specify this property or use this enum
	/// directly. The XamMaskedEditor will set the associated property automatically based on the
	/// mask.
	/// </para>
	/// <seealso cref="EditSectionBase.Orientation"/>
	/// </remarks>
	public enum EditOrientation
	{
		/// <summary>
		/// The section is a left-to-right section.
		/// </summary>
		LeftToRight,

		/// <summary>
		/// The section is a right-to-left section.
		/// </summary>
		RightToLeft
	}

	#endregion // EditOrientation

	#region DisplayCharIncludeMethod

	/// <summary>
	/// This enumeration is used to specify the value for <see cref="DisplayCharBase.IncludeMethod"/> 
	/// property. It dictates how a value of a DisplayChar is included when applying
	/// mask to get the text.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to specify this property or use this enum
	/// directly. The XamMaskedEditor will set the associated property automatically based on the
	/// mask.
	/// </para>
	/// <seealso cref="DisplayCharBase.IncludeMethod"/>
	/// </remarks>
	public enum DisplayCharIncludeMethod
	{
		/// <summary>
		/// Default is to look at the mask mode in deciding whether to include
		/// or not.
		/// </summary>
		Default		= 0,

		/// <summary>
		/// Always include the DisplayChar
		/// </summary>
		Always		= 1,

		/// <summary>
		/// Never include the DisplayChar
		/// </summary>
		Never		= 2
	}

	#endregion //DisplayCharIncludeMethod

	#region FilterType

	/// <summary>
	/// Used to specify the type of converting is to be done on an input character.
	/// </summary>
	internal enum FilterType
	{
		/// <summary>
		/// Unchanged, character will be unchanged
		/// </summary>
		Unchanged = 0,

		/// <summary>
		/// character will be converted to lower case
		/// </summary>
		LowerCase = 1,

		/// <summary>
		/// character will be converted to upper case
		/// </summary>
		UpperCase = 2
	};

	#endregion //FilterType

	#region MaskMode

	/// <summary>
	/// Used for specifying the mode to be applied when getting text from a 
	/// masked editor.
	/// </summary>
	/// <remarks>
	/// <seealso cref="XamMaskedEditor.DataMode"/>
	/// <seealso cref="XamMaskedEditor.DisplayMode"/>
	/// <seealso cref="XamMaskedEditor.ClipMode"/>
	/// <seealso cref="XamMaskedEditor.GetText(MaskMode)"/>
	/// </remarks>
	public enum MaskMode
	{
		/// <summary>
		/// Raw Data Mode. Only significant characters will be
		/// returned. Any prompt characters or literals will be excluded 
		/// from the text.
		/// </summary>
		Raw						= 0,

		/// <summary>
        /// (Default) Include Literal Characters. Data and literal characters will 
		/// be returned. Prompt characters will be omitted.
		/// </summary>
		IncludeLiterals			= 1,

		/// <summary>
		/// Include Prompt Characters. Data and prompt characters will be 
		/// returned. Literals will be omitted.
		/// </summary>
		IncludePromptChars		= 2,
 
		/// <summary>
		/// Include both Prompt Characters and Literals. Text will be 
		/// returned exactly as it appears in the object when a cell is
		/// in edit mode. Data, prompt character and literals will all be 
		/// included.
		/// </summary>
		IncludeBoth				= 3,
 
		/// <summary>
		/// Include Literals With Padding. Prompt characters will be 
		/// converted into pad characters (by default they are spaces,
		/// which are then included with literals and data when text 
		/// is returned.
		/// </summary>
		IncludeLiteralsWithPadding	= 4

	};
	
	#endregion    //MaskMode 

	#region	SignDisplayType






	internal enum SignDisplayType
	{
		/// <summary>
		/// No sign support in the section, so the section will always contain positive numbers.
		/// </summary>
		None,

		/// <summary>
		/// Sign will only be displayed when the value is negative.
		/// </summary>
		ShowWhenNegative,

		/// <summary>
		/// Sign will always be displayed (+ when positive, - when negative).
		/// </summary>
		ShowAlways
	}

	#endregion	// SignDisplayType
			
	#region MaskedEditTabNavigation

	/// <summary>
	/// Used for specifying XamMaskedEditor's <see cref="XamMaskedEditor.TabNavigation"/> property.
	/// </summary>
	/// <remarks>
	/// <seealso cref="XamMaskedEditor.TabNavigation"/>
	/// </remarks>
	public enum MaskedEditTabNavigation
	{
		/// <summary>
		/// Tab to the next control
		/// </summary>
		NextControl, 

		/// <summary>
		/// Tab to the next section or to the next control if focus is in the last section. 
		/// </summary>
		NextSection,
	}

	#endregion MaskedEditTabNavigation

	#region MaskSelectAllBehavior Enum

	/// <summary>
	/// Used for specifying XamMaskedEditor's <see cref="XamMaskedEditor.SelectAllBehavior"/> property.
	/// </summary>
	/// <remarks>
	/// <see cref="XamMaskedEditor.SelectAllBehavior"/>
	/// </remarks>
	public enum MaskSelectAllBehavior
	{
		/// <summary>
		/// Select all characters.
		/// </summary>
		SelectAllCharacters			= 0,

		/// <summary>
		/// Select entered characters, including intervening empty characters and adjacent literals.
		/// </summary>
		SelectEnteredCharacters		= 1
	}

	#endregion // MaskSelectAllBehavior Enum

	#region AutoFillDate

	// SSP 10/10/06 - NAS 6.3
	// 
	/// <summary>
	/// Used for specifying XamMaskedEditor's <see cref="XamMaskedEditor.AutoFillDate"/> property.
	/// </summary>
	/// <remarks>
	/// <seealso cref="XamMaskedEditor.AutoFillDate"/>
	/// </remarks>
	public enum AutoFillDate
	{
		/// <summary>
		/// Do not auto-fill.
		/// </summary>
		None,

		/// <summary>
		/// Auto-fill year.
		/// </summary>
		Year,

		/// <summary>
		/// Auto-fill month and year.
		/// </summary>
		MonthAndYear
	}

	#endregion // AutoFillDate

	#region SpinButtonDisplayMode Enum

	// SSP 10/5/09 - NAS10.1 Spin Buttons
	// 
	/// <summary>
	/// Used for specifying the XamMaskedEditor's <see cref="XamMaskedEditor.SpinButtonDisplayMode"/> property.
	/// </summary>
	/// <seealso cref="XamMaskedEditor.SpinButtonDisplayMode"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
	public enum SpinButtonDisplayMode
	{
		/// <summary>
		/// Never display the button.
		/// </summary>
		Never = 0,

		/// <summary>
		/// Always display the button.
		/// </summary>
		Always = 1,

		/// <summary>
		/// Display the button when the mouse is over. Note that this option will always display
		/// the button when the editor is in edit mode.
		/// </summary>
		MouseOver = 2,

		/// <summary>
		/// Display the button when the editor is focus.
		/// </summary>
		Focused = 3,

		/// <summary>
		/// Display the button only when in edit mode.
		/// </summary>
		OnlyInEditMode = 4
	}

	#endregion // SpinButtonDisplayMode Enum

	// MD 4/22/11 - TFS73181
	#region MaskDefaultType enum







	internal enum MaskDefaultType : byte
	{
		None,

		Byte,
		SByte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,

		Float,
		Double,

		Currency,

		DateTime,
	}

	#endregion // MaskDefaultType enum
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