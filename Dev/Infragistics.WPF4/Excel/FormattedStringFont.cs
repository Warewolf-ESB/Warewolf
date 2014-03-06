using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Controls the formatting of a range of characters in a <see cref="FormattedString"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The formatting of the string is controlled in a similar fashion as it would be in Microsoft Excel. In Excel, the user
	/// must select a portion of the text and set the various formatting properties of that selected text. 
	/// </p>
	/// <p class="body">
	/// With the <see cref="FormattedString"/>, a portion of the string is "selected" by calling either 
	/// <see cref="Excel.FormattedString.GetFont(int)">GetFont(int)</see> or 
	/// <see cref="Excel.FormattedString.GetFont(int,int)">GetFont(int,int)</see>. Formatting properties 
	/// are then set on the returned FormattedStringFont and all characters in the font's selection range are given these 
	/// properties.
	/// </p>
	/// <p class="body">
	/// Getting the formatting properties of a FormattedStringFont will return the formatting of the first character in font's 
	/// selection range. This is similar to Excel, which will update the formatting interface to reflect the formatting of the 
	/// first character in a selection range when a cell's text is selected.
	/// </p>
	/// </remarks>
	/// <seealso cref="T:FormattedString"/>
	/// <seealso cref="M:FormattedString.GetFont(int)"/>
	/// <seealso cref="M:FormattedString.GetFont(int,int)"/>



	public

		// MD 11/9/11 - TFS85193
		// Split this class into a base and derived type so we could share logic with the FormattedTextFont
		//class FormattedStringFont : IWorkbookFont
		class FormattedStringFont : FormattedFontBase
	{
		#region Member Variables

		// MD 11/9/11 - TFS85193
		// Moved these members to the base type
		//private FormattedString formattedString;
		//private int startIndex;
		//private int length;

		#endregion Member Variables

		#region Constructor

		// MD 11/9/11 - TFS85193
		//internal FormattedStringFont( FormattedString formattedString, int startIndex, int length )
		//{
		//    this.formattedString = formattedString;
		//    this.startIndex = startIndex;
		//    this.length = length;
		//}
		internal FormattedStringFont(FormattedString formattedString, int startIndex, int length)
			: base(formattedString, startIndex, length) { }

		#endregion Constructor

		#region Base Class Overrides

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region AllowDefaultValues

		internal override bool AllowDefaultValues
		{
			// When the formatting string is owned by a cell, we don't allow the font properties to have default values,
			// because two cells which different styles owning the same formatted string in the string table would need
			// to write out two separate string items if we allowed defaults.
			get { return (this.FormattedString.Owner is WorksheetCell) == false; }
		}

		#endregion // AllowDefaultValues

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region GetFontDefaultsResolver

		internal override IWorkbookFontDefaultsResolver GetFontDefaultsResolver(Workbook workbook)
		{
			WorksheetCell owningCell = this.FormattedString.Owner as WorksheetCell;
			if (owningCell == null)
				return null;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			//Worksheet worksheet = owningCell.Row.Worksheet;
			Worksheet worksheet = owningCell.Worksheet;
			if (worksheet == null)
				return null;

			return worksheet.GetCellFormatElementReadOnly(owningCell.Row, owningCell.ColumnIndexInternal);
		}

		#endregion // GetFontDefaultsResolver

		// MD 11/9/11 - TFS85193
		#region GetRunsInRange

		// MD 1/29/12 - 12.1 - Cell Format Updates
		//internal override void GetRunsInRange(Workbook workbook, List<FormattingRunBase> runs, int firstCharacterAfterFont, out FormattingRunBase lastRunEnumerated)
		internal override void GetRunsInRange(Workbook workbook, 
			List<FormattingRunBase> runs, 
			int firstCharacterAfterFont,
			IWorkbookFontDefaultsResolver fontDefaultsResolver, 
			out FormattingRunBase lastRunEnumerated)
		{
			lastRunEnumerated = null;

			FormattedString formattedString = this.FormattedString;

			// MD 1/31/12 - TFS100573
			//FormattedStringElement formattedStringElement = formattedString.Element;
			//if (formattedStringElement.HasFormatting)
			FormattedStringElement formattedStringElement = formattedString.Element as FormattedStringElement;
			if (formattedStringElement != null && formattedStringElement.HasFormatting)
			{
				// MD 1/29/12 - 12.1 - Cell Format Updates
				//this.GetRunsFromOwnerInRange(formattedStringElement, workbook, runs, firstCharacterAfterFont, ref lastRunEnumerated);
				this.GetRunsFromOwnerInRange(formattedStringElement, workbook, runs, firstCharacterAfterFont, fontDefaultsResolver, ref lastRunEnumerated);
			}
			
			if (lastRunEnumerated == null)
			{
				WorksheetCell owningCell = formattedString.Owner as WorksheetCell;

				// MD 2/29/12 - 12.1 - Table Support
				//if (owningCell != null)
				if (owningCell != null && owningCell.Row != null)
				{
					WorksheetCellFormatProxy proxy;
					if (owningCell.Row.TryGetCellFormat(owningCell.ColumnIndexInternal, out proxy))
					{
						// MD 1/31/12 - TFS100573
						//lastRunEnumerated = new CellFormattingRunPlaceholder(formattedStringElement, proxy.Font);
						lastRunEnumerated = new CellFormattingRunPlaceholder(formattedString.Element, proxy.Font);
					}
				}
			}
		}

		#endregion  // GetRunsInRange

		// MD 11/9/11 - TFS85193
		#region StartingRun

		internal override FormattingRunBase StartingRun
		{
			get
			{
				FormattedString formattedString = this.FormattedString;

				// MD 1/31/12 - TFS100573
				//FormattedStringElement formattedStringElement = formattedString.Element;
				//if (formattedStringElement.HasFormatting)
				FormattedStringElement formattedStringElement = formattedString.Element as FormattedStringElement;
				if (formattedStringElement != null && formattedStringElement.HasFormatting)
				{
					FormattingRunBase run = this.TryGetFirstRunControllingCharacter(formattedStringElement.FormattingRuns, this.StartIndex);
					if (run != null)
						return run;
				}

				WorksheetCell owningCell = formattedString.Owner as WorksheetCell;

				// MD 2/29/12 - 12.1 - Table Support
				//if (owningCell != null)
				if (owningCell != null && owningCell.Row != null)
				{
					// MD 1/29/12 - 12.1 - Cell Format Updates
					// We should return the resolved properties for formatted strings owned by cells, because we don't allow them to 
					// set default values on the font properties.
					//IWorkbookFont font = owningCell.Row.GetCellFormatInternal(owningCell.ColumnIndexInternal).Font;
					IWorkbookFont font = owningCell.Row.GetResolvedCellFormatInternal(owningCell.ColumnIndexInternal).Font;

					// MD 1/31/12 - TFS100573
					//return new CellFormattingRunPlaceholder(formattedStringElement, font);
					return new CellFormattingRunPlaceholder(formattedString.Element, font);
				}

				Utilities.DebugFail("There should have been an owning cell at this point.");
				return null;
			}
		}

		#endregion  // StartingRun

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		// MD 11/9/11 - TFS85193
		// Moved this to the base type
		#region Removed

		//#region SetFontFormatting

		///// <summary>
		///// Sets all properties of this font to the properties of the specified font.
		///// </summary>
		///// <exception cref="ArgumentNullException">
		///// <paramref name="source"/> is null.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <param name="source">The font whose properties should be copied over to this font.</param>
		//public void SetFontFormatting(IWorkbookFont source)
		//{
		//    if (source == null)
		//        throw new ArgumentNullException("source", SR.GetString("LE_ArgumentNullException_SourceFont"));

		//    this.Name = source.Name;
		//    this.Height = source.Height;
		//    this.Color = source.Color;
		//    this.Bold = source.Bold;
		//    this.Italic = source.Italic;
		//    this.Strikeout = source.Strikeout;
		//    this.SuperscriptSubscriptStyle = source.SuperscriptSubscriptStyle;
		//    this.UnderlineStyle = source.UnderlineStyle;
		//}

		//#endregion SetFontFormatting

		#endregion  // Removed

		#endregion Public Methods

		#region Private Methods

		// MD 4/12/11 - TFS67084
		#region AfterSet

		// MD 11/9/11 - TFS85193
		//private void AfterSet()
		internal override void AfterSet()
		{
			// MD 11/9/11 - TFS85193
			// Cached this because the member variable has been removed. Changed all references to the member to references to the local.
			FormattedString formattedString = this.FormattedString;

			uint oldElementKey = formattedString.Element.Key;

			formattedString.AfterSet();

			WorksheetCell owningCell = formattedString.Owner as WorksheetCell;

			// MD 2/29/12 - 12.1 - Table Support
			//if (owningCell != null)
			if (owningCell != null && owningCell.Row != null)
			{
				uint newElementKey = formattedString.Element.Key;

				if (oldElementKey != newElementKey)
				{
					// MD 5/31/11 - TFS75574
					// We need more information than just the key from the element, so just pass the element directly.
					//owningCell.Row.UpdateFormattedStringKeyOnCell(owningCell.ColumnIndexInternal, newElementKey);
					owningCell.Row.UpdateFormattedStringElementOnCell(owningCell.ColumnIndexInternal, formattedString.Element);
				}
			}

			base.AfterSet();
		} 

		#endregion  // AfterSet

		// MD 4/12/11 - TFS67084
		#region BeforeSet

		// MD 11/9/11 - TFS85193
		//private void BeforeSet()
		internal override void BeforeSet()
		{
			// MD 11/9/11 - TFS85193
			// The member has been removed.
			//this.formattedString.BeforeSet();
			this.FormattedString.BeforeSet();
		} 

		#endregion  // BeforeSet

		// MD 11/9/11 - TFS85193
		// Moved this to the base type
		#region Removed

		//#region VerifyState

		//private void VerifyState()
		//{
		//    // MD 9/2/08 - Cell Comments
		//    //IWorksheetCell owningCell = this.formattedString.OwningCell;
		//    IFormattedStringOwner owner = this.formattedString.Owner;

		//    // MD 9/2/08 - Cell Comments
		//    //if ( owningCell == null )
		//    if (owner == null)
		//        throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FormattedStringNotOwned"));

		//    int endIndex = this.startIndex;

		//    if (0 < length)
		//        endIndex += length;

		//    if (this.formattedString.UnformattedString.Length < endIndex)
		//        throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidCharacterRange"));
		//}

		//#endregion VerifyState

		#endregion  // Removed

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		// MD 11/9/11 - TFS85193
		// Moved all font properties to the base.
		#region Removed

		//#region Bold

		///// <summary>
		///// Gets or sets the value indicating whether the font is bold.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidEnumArgumentException">
		///// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The value indicating whether the font is bold.</value>
		//public ExcelDefaultableBoolean Bold
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Bold;
		//    }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Use the utility function instead of Enum.IsDefined. It is faster.
		//        //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
		//        if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
		//            throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Bold = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Bold

		//#region Color

		///// <summary>
		///// Gets or sets the fore color of the font.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The fore color of the font.</value>
		//public Color Color
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Color;
		//    }
		//    set
		//    {
		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Color = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Color

		#endregion  // Removed

		#region FormattedString

		/// <summary>
		/// Gets the formatted string which is controlled by this font.
		/// </summary>
		/// <value>The formatted string which is controlled by this font.</value>
		public FormattedString FormattedString
		{
			// MD 11/9/11 - TFS85193
			//get { return this.formattedString; }
			get { return (FormattedString)this.FormattedItem; }
		}

		#endregion FormattedString

		// MD 11/9/11 - TFS85193
		// Moved all font properties to the base.
		#region Removed

		//#region Height

		///// <summary>
		///// Gets or sets the height of the font.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="ArgumentOutOfRangeException">
		///// The value assigned is positive and outside the valid font height range of 20 and 8180.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The height of the font.</value>
		//public int Height
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Height;
		//    }
		//    set
		//    {
		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Height = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Height

		//#region Italic

		///// <summary>
		///// Gets or sets the value indicating whether the font is italic.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidEnumArgumentException">
		///// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The value indicating whether the font is italic.</value>
		//public ExcelDefaultableBoolean Italic
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Italic;
		//    }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Use the utility function instead of Enum.IsDefined. It is faster.
		//        //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
		//        if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
		//            throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Italic = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Italic

		//#region Length

		///// <summary>
		///// Gets the number of characters covered by this font. Zero indicates the font controls from 
		///// the <see cref="StartIndex"/> to the end of the string.
		///// </summary>
		///// <value>
		///// The number of characters covered by this font. Zero indicates the font controls from the 
		///// StartIndex to the end of the string.
		///// </value>
		//public int Length
		//{
		//    get { return this.length; }
		//}

		//#endregion Length

		//#region Name

		///// <summary>
		///// Gets or sets the name of the font.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The name of the font.</value>
		//public string Name
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Name;
		//    }
		//    set
		//    {
		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Name = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Name

		//#region StartIndex

		///// <summary>
		///// Gets the index of the first character covered by this font.
		///// </summary>
		///// <value>The index of the first character covered by this font.</value>
		//public int StartIndex
		//{
		//    get { return this.startIndex; }
		//}

		//#endregion StartIndex

		//#region Strikeout

		///// <summary>
		///// Gets or sets the value indicating whether the font is struck out.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidEnumArgumentException">
		///// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The value indicating whether the font is struck out.</value>
		//public ExcelDefaultableBoolean Strikeout
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.Strikeout;
		//    }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Use the utility function instead of Enum.IsDefined. It is faster.
		//        //if ( Enum.IsDefined( typeof( ExcelDefaultableBoolean ), value ) == false )
		//        if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
		//            throw new InvalidEnumArgumentException( "value", (int)value, typeof( ExcelDefaultableBoolean ) );

		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.Strikeout = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion Strikeout

		//#region SuperscriptSubscriptStyle

		///// <summary>
		///// Gets or sets the value indicating whether the font is superscript or subscript.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidEnumArgumentException">
		///// The value assigned is not defined in the <see cref="FontSuperscriptSubscriptStyle"/> enumeration.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The value indicating whether the font is superscript or subscript.</value>
		//public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.SuperscriptSubscriptStyle;
		//    }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Use the utility function instead of Enum.IsDefined. It is faster.
		//        //if ( Enum.IsDefined( typeof( FontSuperscriptSubscriptStyle ), value ) == false )
		//        if (Utilities.IsFontSuperscriptSubscriptStyleDefined(value) == false)
		//            throw new InvalidEnumArgumentException( "value", (int)value, typeof( FontSuperscriptSubscriptStyle ) );

		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.SuperscriptSubscriptStyle = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion SuperscriptSubscriptStyle

		//#region UnderlineStyle

		///// <summary>
		///// Gets or sets the underline style of the font.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// All characters in the selection range of this font will be affected by setting this property.
		///// Getting this property will return a value which indicates the formatting of the first character
		///// in this font's range.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidEnumArgumentException">
		///// The value assigned is not defined in the <see cref="FontUnderlineStyle"/> enumeration.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The <see cref="FormattedString"/> associated with the font is not the value of a <see cref="WorksheetCell"/>.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// This font's selection range is outside the valid character range of the associated FormattedString.
		///// </exception>
		///// <value>The underline style of the font.</value>
		//public FontUnderlineStyle UnderlineStyle
		//{
		//    get
		//    {
		//        this.VerifyState();
		//        return this.StartingFont.UnderlineStyle;
		//    }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Use the utility function instead of Enum.IsDefined. It is faster.
		//        //if ( Enum.IsDefined( typeof( FontUnderlineStyle ), value ) == false )
		//        if (Utilities.IsFontUnderlineStyleDefined(value) == false)
		//            throw new InvalidEnumArgumentException( "value", (int)value, typeof( FontUnderlineStyle ) );

		//        this.VerifyState();

		//        // MD 4/12/11 - TFS67084
		//        // Call the BeforeSet method, which also calls the proxy's BeforeSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //// Call BeforeSet to temporarily release the element.
		//        //FormattedStringProxy proxy = this.formattedString.Proxy;
		//        //proxy.BeforeSet();
		//        this.BeforeSet();

		//        foreach ( IWorkbookFont font in this.FontsInRange )
		//            font.UnderlineStyle = value;

		//        // MD 4/12/11 - TFS67084
		//        // Call the AfterSet method, which also calls the proxy's AfterSet method.
		//        //// MD 11/3/10 - TFS49093
		//        //proxy.AfterSet();
		//        this.AfterSet();
		//    }
		//}

		//#endregion UnderlineStyle

		#endregion  // Removed

		#endregion Public Properties

		#region Internal Properties

		// MD 11/9/11 - TFS85193
		#region Removed

		//        #region FontsInRange

		//        // This has to be hidden in the debugger, or viewing the property will mess up the 
		//        // formatting runs on the formatted string.
		//        [DebuggerBrowsable( DebuggerBrowsableState.Never )]
		//        internal List<IWorkbookFont> FontsInRange
		//        {
		//            get
		//            {
		//                // MD 9/2/08 - Cell Comments
		//                //Workbook workbook = this.formattedString.OwningCell.Worksheet.Workbook;
		//                Workbook workbook = this.formattedString.Owner.Worksheet.Workbook;

		//                List<IWorkbookFont> fonts = new List<IWorkbookFont>();

		//                int indexAfterRange = ( this.length != 0 )
		//                    ? indexAfterRange = this.startIndex + this.length
		//                    : indexAfterRange = this.formattedString.UnformattedString.Length;

		//                int runIndex = 0;

		//                // MD 11/3/10 - TFS49093
		//                // The formatted string data is now stored on the FormattedStringElement.
		//                //if ( this.formattedString.HasFormatting )
		//                // MD 4/12/11 - TFS67084
		//                // Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
		//                //FormattedStringElement formattedStringElement = this.formattedString.Proxy.Element;
		//                FormattedStringElement formattedStringElement = this.formattedString.Element;
		//                if (formattedStringElement.HasFormatting)
		//                {
		//                    // MD 11/3/10 - TFS49093
		//                    // The formatted string data is now stored on the FormattedStringElement.
		//                    //for ( ; runIndex < this.formattedString.FormattingRuns.Count; runIndex++ )
		//                    //{
		//                    //    FormattedStringRun run = this.formattedString.FormattingRuns[ runIndex ];
		//                    for (; runIndex < formattedStringElement.FormattingRuns.Count; runIndex++)
		//                    {
		//                        FormattedStringRun run = formattedStringElement.FormattingRuns[runIndex];

		//                        if ( run.FirstFormattedChar < this.startIndex )
		//                            continue;

		//                        if ( fonts.Count == 0 && this.startIndex < run.FirstFormattedChar )
		//                        {
		//                            FormattedStringRun startingRun = new FormattedStringRun( this.formattedString, this.startIndex, workbook );

		//                            // MD 8/23/11
		//                            // Found while fixing TFS84306
		//                            // If we need to initialize a new formatting run between two existing runs, we need to copy the previous 
		//                            // run's font, not the next run's font
		//                            //if ( run.HasFont )
		//                            //    startingRun.Font.SetFontFormatting( run.Font );
		//                            if (0 < runIndex)
		//                            {
		//                                FormattedStringRun previousRun = formattedStringElement.FormattingRuns[runIndex - 1];
		//                                if (previousRun.HasFont)
		//                                    startingRun.Font.SetFontFormatting(previousRun.Font);
		//                            }

		//                            // MD 11/3/10 - TFS49093
		//                            // The formatted string data is now stored on the FormattedStringElement.
		//                            //this.formattedString.FormattingRuns.Insert( runIndex, startingRun );
		//                            formattedStringElement.FormattingRuns.Insert(runIndex, startingRun);

		//                            // MD 8/23/11
		//                            // Found while fixing TFS84306
		//                            // We shouldn't increment the runIndex. Now that we inserted the new run into the collection, the runIndex
		//                            // points to it, so when we go around for another loop and runIndex is incremented automatically, we will hit
		//                            // the run we started processing again. That way, we can add it's font too if the font overlaps that run as 
		//                            // well. If we increment runIndex here, we will end up skipping the run we started processing.
		//                            //runIndex++;

		//                            run = startingRun;
		//                        }

		//                        if ( indexAfterRange <= run.FirstFormattedChar )
		//                            break;

		//                        fonts.Add( run.Font );
		//                    }
		//                }

		//                if ( fonts.Count == 0 )
		//                {
		//                    IWorkbookFont previousFont = null;

		//                    // MD 11/3/10 - TFS49093
		//                    // The formatted string data is now stored on the FormattedStringElement.
		//                    //if ( this.formattedString.HasFormatting )
		//                    //{
		//                    //    FormattedStringRun previousRun = this.formattedString.FormattingRuns[ this.formattedString.FormattingRuns.Count - 1 ];
		//                    if (formattedStringElement.HasFormatting)
		//                    {
		//                        FormattedStringRun previousRun = formattedStringElement.FormattingRuns[formattedStringElement.FormattingRuns.Count - 1];

		//                        if ( previousRun.HasFont )
		//                            previousFont = previousRun.Font;
		//                    }
		//                    // MD 9/2/08 - Cell Comments
		//                    //else if ( this.formattedString.OwningCell.HasCellFormat )
		//                    //    previousFont = this.formattedString.OwningCell.CellFormat.Font;
		//                    else
		//                    {
		//                        // MD 4/12/11 - TFS67084
		//                        // Go through the row to get the cell format instead of the cell.
		//                        //IWorksheetCell owningCell = this.formattedString.Owner as IWorksheetCell;
		//                        //
		//                        //if ( owningCell != null && owningCell.HasCellFormat )
		//                        //    previousFont = owningCell.CellFormat.Font;
		//                        //
		//                        WorksheetCell owningCell = this.formattedString.Owner as WorksheetCell;
		//                        if (owningCell != null)
		//                        {
		//                            WorksheetCellFormatProxy proxy;
		//                            if (owningCell.Row.TryGetCellFormat(owningCell.ColumnIndexInternal, out proxy))
		//                                previousFont = proxy.Font;
		//                        }
		//                    }

		//                    FormattedStringRun startingRun = new FormattedStringRun( this.formattedString, this.startIndex, workbook );

		//                    if ( previousFont != null )
		//                        startingRun.Font.SetFontFormatting( previousFont );

		//                    fonts.Add( startingRun.Font );

		//                    // MD 11/3/10 - TFS49093
		//                    // The formatted string data is now stored on the FormattedStringElement.
		//                    //this.formattedString.FormattingRuns.Add( startingRun );
		//                    formattedStringElement.FormattingRuns.Add(startingRun);
		//                    runIndex++;
		//                }

		//                if ( indexAfterRange < this.formattedString.UnformattedString.Length )
		//                {
		//                    // MD 11/3/10 - TFS49093
		//                    // The formatted string data is now stored on the FormattedStringElement.
		//                    //if ( this.formattedString.FormattingRuns.Count <= runIndex ||
		//                    //    indexAfterRange < this.formattedString.FormattingRuns[ runIndex ].FirstFormattedChar )
		//                    if (formattedStringElement.FormattingRuns.Count <= runIndex ||
		//                        indexAfterRange < formattedStringElement.FormattingRuns[runIndex].FirstFormattedChar)
		//                    {
		//                        // Create a new formatting run and initialize the font to the font of the last formating run's font
		//                        FormattedStringRun newRun = new FormattedStringRun( this.formattedString, indexAfterRange, workbook );
		//                        newRun.Font.SetFontFormatting( fonts[ fonts.Count - 1 ] );

		//                        // MD 11/3/10 - TFS49093
		//                        // The formatted string data is now stored on the FormattedStringElement.
		//                        //this.formattedString.FormattingRuns.Insert( runIndex, newRun );
		//                        formattedStringElement.FormattingRuns.Insert(runIndex, newRun);
		//                    }
		//                }

		//                return fonts;
		//            }
		//        }

		//        #endregion FontsInRange

		//        #region StartingFont

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the font of the first character in the selection range.
		//        /// </summary> 
		//#endif
		//        internal IWorkbookFont StartingFont
		//        {
		//            get
		//            {
		//                FormattedStringRun lastRun = null;

		//                // MD 11/3/10 - TFS49093
		//                // The formatted string data is now stored on the FormattedStringElement.
		//                //if ( this.formattedString.HasFormatting )
		//                //{
		//                //    foreach ( FormattedStringRun run in this.formattedString.FormattingRuns )
		//                // MD 4/12/11 - TFS67084
		//                // Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
		//                //FormattedStringElement formattedStringElement = this.formattedString.Proxy.Element;
		//                FormattedStringElement formattedStringElement = this.formattedString.Element;
		//                if (formattedStringElement.HasFormatting)
		//                {
		//                    foreach (FormattedStringRun run in formattedStringElement.FormattingRuns)
		//                    {
		//                        if ( this.startIndex < run.FirstFormattedChar )
		//                            break;

		//                        lastRun = run;
		//                    }
		//                }

		//                if ( lastRun == null )
		//                {
		//                    // MD 9/2/08 - Cell Comments
		//                    //return this.formattedString.OwningCell.CellFormat.Font;
		//                    // MD 4/12/11 - TFS67084
		//                    // Go through the row to get the cell format instead of the cell.
		//                    //IWorksheetCell owningCell = this.formattedString.Owner as IWorksheetCell;
		//                    //
		//                    //if ( owningCell != null )
		//                    //    return owningCell.CellFormat.Font;
		//                    //
		//                    WorksheetCell owningCell = this.formattedString.Owner as WorksheetCell;
		//                    if (owningCell != null)
		//                        return owningCell.Row.GetCellFormatInternal(owningCell.ColumnIndexInternal).Font;

		//                    Utilities.DebugFail( "There should have been an owning cell at this point." );
		//                    return null;
		//                }

		//                return lastRun.Font;
		//            }
		//        }

		//        #endregion StartingFont

		#endregion  // Removed

		#endregion Internal Properties

		#endregion Properties
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