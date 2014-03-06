using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;





using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a font for the cell in the related context.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// If a property of a font has a default value, value of the previous font in resolution chain is used. The resolution priority 
	/// is the following: 
	/// <list type="number">
	/// <item>Cell (highest priority)</item>
	/// <item>Row</item>
	/// <item>Column</item>
	/// <item>Default Cell Format (lowest priority)</item>
	/// </list>
	/// For example, if a column font is set to blue and bold and a row font is set to italic and not bold, the font in the cell at 
	/// the intersection of the row and column would have blue, italic text in the saved workbook.
	/// </p>
	/// </remarks>



	public

		 interface IWorkbookFont
	{
		/// <summary>
		/// Sets all font properties to specific font formatting.
		/// </summary>
		/// <param name="source">Source font format.</param>
		void SetFontFormatting( IWorkbookFont source );

		/// <summary>
		/// Gets or sets the value which indicates whether the font is bold.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <value>The value which indicates whether the font is bold.</value>
		ExcelDefaultableBoolean Bold { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="ColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/17/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorkbookFont.Color is deprecated. It has been replaced by IWorkbookFont.ColorInfo.")] // MD 1/17/12 - 12.1 - Cell Format Updates
		Color Color { get; set; }

		// MD 1/17/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the foreground color of the font.
		/// </summary>
		/// <value>The foreground color of the font.</value>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo ColorInfo { get; set; }

		/// <summary>
		/// Gets or sets the font height in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A negative value indicates the default font height, in which case the <see cref="Workbook.DefaultFontHeight"/> is used.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is positive and outside the valid font height range of 20 and 8180.
		/// </exception>
		/// <value>The font height in twips (1/20th of a point).</value>
		int Height { get; set; }

		/// <summary>
		/// Gets or sets the value which indicates whether the font is italic.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <value>The value which indicates whether the font is italic.</value>
		ExcelDefaultableBoolean Italic { get; set; }

		/// <summary>
		/// Gets or sets the font family name.
		/// </summary>
		/// <value>The font family name.</value>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the value which indicates whether the font is struck out.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <value>The value which indicates whether the font is struck out.</value>
		ExcelDefaultableBoolean Strikeout { get; set; }

		/// <summary>
		/// Gets or sets the value which indicates whether the font is superscript or subscript.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="FontSuperscriptSubscriptStyle"/> enumeration.
		/// </exception>
		/// <value>The value which indicates whether the font is superscript or subscript.</value>
		FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle { get; set; }

		/// <summary>
		/// Gets or sets the underline style of the font.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="FontUnderlineStyle"/> enumeration.
		/// </exception>
		/// <value>The underline style of the font.</value>
		FontUnderlineStyle UnderlineStyle { get; set; }
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