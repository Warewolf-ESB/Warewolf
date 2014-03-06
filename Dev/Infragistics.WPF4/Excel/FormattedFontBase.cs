using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 11/9/11 - TFS85193
	/// <summary>
	/// Abstract base class which controls the formatting of a range of characters in a <see cref="FormattedString"/> or <see cref="FormattedText"/>.
	/// </summary>
	/// <seealso cref="FormattedStringFont"/>
	/// <seealso cref="FormattedTextFont"/>



	public

		abstract class FormattedFontBase : IWorkbookFont
	{
		#region Member Variables

		private readonly IFormattedItem formattedItem;
		private readonly int startIndex;
		private readonly int length;

		#endregion Member Variables

		#region Constructor

		internal FormattedFontBase(IFormattedItem formattedItem, int startIndex, int length)
		{
			this.formattedItem = formattedItem;
			this.startIndex = startIndex;
			this.length = length;
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region SetFontFormatting

		/// <summary>
		/// Sets all properties of this font to the properties of the specified font.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, any default values on the specified font will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="source"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <param name="source">The font whose properties should be copied over to this font.</param>
		public void SetFontFormatting(IWorkbookFont source)
		{
			if (source == null)
				throw new ArgumentNullException("source", SR.GetString("LE_ArgumentNullException_SourceFont"));

			this.Name = source.Name;
			this.Height = source.Height;

			// MD 1/17/12 - 12.1 - Cell Format Updates
			//this.Color = source.Color;
			this.ColorInfo = source.ColorInfo;

			this.Bold = source.Bold;
			this.Italic = source.Italic;
			this.Strikeout = source.Strikeout;
			this.SuperscriptSubscriptStyle = source.SuperscriptSubscriptStyle;
			this.UnderlineStyle = source.UnderlineStyle;
		}

		#endregion SetFontFormatting

		#endregion Public Methods

		#region Internal Methods

		internal virtual void AfterSet() 
		{
			this.formattedItem.OnFormattingChanged();
		}

		internal virtual void BeforeSet() { }

		#region FontDefaultsResolver

		#endregion // FontDefaultsResolver

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region GetFontDefaultsResolver

		internal virtual IWorkbookFontDefaultsResolver GetFontDefaultsResolver(Workbook workbook)
		{
			return null;
		}

		#endregion // GetFontDefaultsResolver

		#region GetRunsInRange

		// MD 1/29/12 - 12.1 - Cell Format Updates
		// Added an IWorkbookFontDefaultsResolver parameter.
		//internal abstract void GetRunsInRange(Workbook workbook, List<FormattingRunBase> runs, int firstCharacterAfterFont, out FormattingRunBase lastRunEnumerated);
		internal abstract void GetRunsInRange(Workbook workbook,
			List<FormattingRunBase> runs,
			int firstCharacterAfterFont,
			IWorkbookFontDefaultsResolver fontDefaultsResolver,
			out FormattingRunBase lastRunEnumerated);

		#endregion // GetRunsInRange

		#region GetRunsFromOwnerInRange

		internal void GetRunsFromOwnerInRange(IFormattedRunOwner owner,
			Workbook workbook,
			List<FormattingRunBase> runs, 
			int firstCharacterAfterFont,
			IWorkbookFontDefaultsResolver fontDefaultsResolver,		// MD 1/27/12 - 12.1 - Cell Format Updates
			ref FormattingRunBase lastRunEnumerated)
		{
			int startIndexResolved = Math.Max(owner.StartIndex, this.startIndex);

			List<FormattingRunBase> runsInOwner = owner.GetFormattingRuns(workbook);
			for (int runIndex = 0; runIndex < runsInOwner.Count; runIndex++)
			{
				FormattingRunBase run = runsInOwner[runIndex];

				try
				{
					if (run.FirstFormattedCharAbsolute < startIndexResolved)
						continue;

					if (runs.Count == 0 && startIndexResolved < run.FirstFormattedCharAbsolute)
					{
						FormattingRunBase startingRun = owner.CreateRun(startIndexResolved);
						if (lastRunEnumerated != null)
							startingRun.InitializeFrom(lastRunEnumerated, workbook);
						// MD 1/27/12 - 12.1 - Cell Format Updates
						// When adding a run, resolve all font properties is a defaults resolver is specified.
						else if (fontDefaultsResolver != null)
							fontDefaultsResolver.ResolveDefaults(startingRun.GetFontInternal(workbook).Element);

						owner.InsertRun(runIndex, startingRun);
						run = startingRun;
					}

					if (firstCharacterAfterFont <= run.FirstFormattedCharAbsolute)
						break;

					runs.Add(run);
				}
				finally
				{
					lastRunEnumerated = run;
				}
			}
		}

		#endregion  // GetRunsFromOwnerInRange

		#region TryGetFirstRunControllingCharacter

		internal FormattingRunBase TryGetFirstRunControllingCharacter(List<FormattingRunBase> formattingRuns, int index)
		{
			for (int runIndex = formattingRuns.Count - 1; runIndex >= 0; runIndex--)
			{
				FormattingRunBase run = formattingRuns[runIndex];
				if (run.FirstFormattedCharAbsolute <= index)
					return run;
			}

			return null;
		}

		#endregion  // TryGetFirstRunControllingCharacter

		#endregion  // Internal Methods

		#region Private Methods

		#region GetControlledRuns

		// MD 1/29/12 - 12.1 - Cell Format Updates
		// Added an IWorkbookFontDefaultsResolver parameter.
		//private List<FormattingRunBase> GetControlledRuns()
		private List<FormattingRunBase> GetControlledRuns(IWorkbookFontDefaultsResolver fontDefaultsResolver)
		{
			Workbook workbook = this.formattedItem.Workbook;

			List<FormattingRunBase> runs = new List<FormattingRunBase>();

			string unformattedString = this.formattedItem.ToString();
			int firstCharacterAfterFont = (this.Length != 0)
				? this.StartIndex + this.Length
				: unformattedString.Length;

			FormattingRunBase lastRunEnumerated;

			// MD 1/29/12 - 12.1 - Cell Format Updates
			//this.GetRunsInRange(workbook, runs, firstCharacterAfterFont, out lastRunEnumerated);
			this.GetRunsInRange(workbook, runs, firstCharacterAfterFont, fontDefaultsResolver, out lastRunEnumerated);
			
			// If we didn't find any runs, there are no runs at or before this font's position, 
			// so add one at the start.
			if (runs.Count == 0)
			{
				IFormattedRunOwner owner = this.formattedItem.GetOwnerAt(this.startIndex);
				if (owner != null)
				{
					FormattingRunBase startingRun = owner.CreateRun(this.startIndex);
					if (lastRunEnumerated != null)
						startingRun.InitializeFrom(lastRunEnumerated, workbook);
					// MD 1/29/12 - 12.1 - Cell Format Updates
					// When adding a run, resolve all font properties is a defaults resolver is specified.
					else if (fontDefaultsResolver != null)
						fontDefaultsResolver.ResolveDefaults(startingRun.GetFontInternal(workbook).Element);

					runs.Add(startingRun);
					owner.AddRun(startingRun);
				}
			}

			// If we found runs, check to see if the last one runs outside the end of the string. If it does, insert a new run
			// at the next character after this font's range and give it that same format at the run before it.
			if (runs.Count > 0 && firstCharacterAfterFont < unformattedString.Length)
			{
				IFormattedRunOwner owner = this.formattedItem.GetOwnerAt(firstCharacterAfterFont);
				if (owner != null)
				{
					List<FormattingRunBase> runsInOwner = owner.GetFormattingRuns(workbook);
					FormattingRunBase runControllingOutOfRangeChar = this.TryGetFirstRunControllingCharacter(runsInOwner, firstCharacterAfterFont);

					if (runControllingOutOfRangeChar == null || 
						runControllingOutOfRangeChar.FirstFormattedCharAbsolute < firstCharacterAfterFont)
					{
						// Create a new formatting run and initialize the font to the font of the last formatting run's font
						FormattingRunBase lastRunOfRange = runs[runs.Count - 1];
						Debug.Assert(runControllingOutOfRangeChar == null || lastRunOfRange == runControllingOutOfRangeChar, "These should be the same");

						FormattingRunBase newRun = owner.CreateRun(firstCharacterAfterFont);
						newRun.InitializeFrom(lastRunOfRange, workbook);

						int index = 0;
						if (runControllingOutOfRangeChar != null)
							index = runsInOwner.IndexOf(runControllingOutOfRangeChar) + 1;

						owner.InsertRun(index, newRun);
					}
				}
			}

			return runs;
		}

		#endregion GetControlledRuns

		// MD 12/21/11 - 12.1 - Table Support
		#region GetFontProperty

		private TValue GetFontProperty<TValue>(Utilities.PropertyGetter<IWorkbookFont, TValue> propertyGetter, TValue defaultValue)
		{
			this.VerifyState();

			FormattingRunBase startingRun = this.StartingRun;
			if (startingRun != null)
				return propertyGetter(startingRun.GetFont(this.FormattedItem.Workbook));

			return defaultValue;
		}

		#endregion // GetFontProperty

		// MD 12/21/11 - 12.1 - Table Support
		#region SetFontProperty

		private void SetFontProperty<TValue>(TValue value, TValue defaultValue, Utilities.PropertySetter<IWorkbookFont, TValue> propertySetter)
		{
			// If the font does not allow default values, ignore them when they are set.
			if (this.AllowDefaultValues == false && Object.Equals(value, defaultValue))
				return;

			this.VerifyState();

			this.BeforeSet();

			Workbook workbook = this.formattedItem.Workbook;
			IWorkbookFontDefaultsResolver fontDefaultsResolver = this.GetFontDefaultsResolver(workbook);
			foreach (FormattingRunBase run in this.GetControlledRuns(fontDefaultsResolver))
			{
				if (run != null)
					propertySetter(run.GetFont(workbook), value);
			}

			this.AfterSet();
		}

		#endregion // SetFontProperty

		#region VerifyState

		private void VerifyState()
		{
			if (this.formattedItem.Owner == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FormattedStringNotOwned"));

			int endIndex = this.startIndex;

			if (0 < length)
				endIndex += length;

			if (this.formattedItem.ToString().Length < endIndex)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidCharacterRange"));
		}

		#endregion VerifyState

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Bold

		/// <summary>
		/// Gets or sets the value indicating whether the font is bold.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The value indicating whether the font is bold.</value>
		public ExcelDefaultableBoolean Bold
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().Bold;

				//return ExcelDefaultableBoolean.Default;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontBoldGetter, ExcelDefaultableBoolean.Default);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
				//    throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).Bold = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				Utilities.VerifyEnumValue(value);
				this.SetFontProperty(value, ExcelDefaultableBoolean.Default, Utilities.FontBoldSetter);
			}
		}

		#endregion Bold

		#region Color

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region Old Code

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
		///// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		///// </exception>
		///// <exception cref="InvalidOperationException">
		///// The font's selection range is outside the valid character range of the associated formatted string or text.
		///// </exception>
		///// <value>The fore color of the font.</value>
		//public Color Color
		//{
		//    get
		//    {
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // Refactored duplicate code into a GetFontProperty helper method.
		//        #region Refactored

		//        //this.VerifyState();

		//        //FormattingRunBase startingRun = this.StartingRun;
		//        //if (startingRun != null)
		//        //    return startingRun.GetFont().Color;

		//        //return Utilities.ColorEmpty;

		//        #endregion // Refactored
		//        return this.GetFontProperty(Utilities.FontColorGetter, Utilities.ColorEmpty);
		//    }
		//    set
		//    {
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // Refactored duplicate code into a SetFontProperty helper method.
		//        #region Refactored

		//        //this.VerifyState();

		//        //this.BeforeSet();
		//        //Workbook workbook = this.formattedItem.Workbook;
		//        //foreach (FormattingRunBase run in this.GetControlledRuns())
		//        //{
		//        //    if (run != null)
		//        //        run.GetFont(workbook).Color = value;
		//        //}
		//        //this.AfterSet();

		//        #endregion // Refactored
		//        this.SetFontProperty(value, Utilities.FontColorSetter);
		//    }
		//}

		#endregion // Old Code
		/// <summary>
		/// Obsolete. Use <see cref="ColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("The FormattedFontBase.Color is deprecated. It has been replaced by FormattedFontBase.ColorInfo.")]
		public Color Color
		{
			get
			{
				WorkbookColorInfo colorInfo = this.ColorInfo;
				if (colorInfo == null)
					return Utilities.ColorEmpty;

				return colorInfo.GetResolvedColor(this.formattedItem.Workbook);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.ColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion Color

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region ColorInfo

		/// <summary>
		/// Gets or sets the fore color of the font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The fore color of the font.</value>
		public WorkbookColorInfo ColorInfo
		{
			get
			{
				return this.GetFontProperty(Utilities.FontColorInfoGetter, null);
			}
			set
			{
				this.SetFontProperty(value, null, Utilities.FontColorInfoSetter);
			}
		}

		#endregion Color

		#region Height

		/// <summary>
		/// Gets or sets the height of the font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is positive and outside the valid font height range of 20 and 8180.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The height of the font.</value>
		public int Height
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().Height;

				//return -1;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontHeightGetter, -1);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).Height = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				this.SetFontProperty(value, -1, Utilities.FontHeightSetter);
			}
		}

		#endregion Height

		#region Italic

		/// <summary>
		/// Gets or sets the value indicating whether the font is italic.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The value indicating whether the font is italic.</value>
		public ExcelDefaultableBoolean Italic
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().Italic;

				//return ExcelDefaultableBoolean.Default;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontItalicGetter, ExcelDefaultableBoolean.Default);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
				//    throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).Italic = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				Utilities.VerifyEnumValue(value);
				this.SetFontProperty(value, ExcelDefaultableBoolean.Default, Utilities.FontItalicSetter);
			}
		}

		#endregion Italic

		#region Length

		/// <summary>
		/// Gets the number of characters covered by this font. Zero indicates the font controls from 
		/// the <see cref="StartIndex"/> to the end of the string.
		/// </summary>
		/// <value>
		/// The number of characters covered by this font. Zero indicates the font controls from the 
		/// StartIndex to the end of the string.
		/// </value>
		public int Length
		{
			get { return this.length; }
		}

		#endregion Length

		#region Name

		/// <summary>
		/// Gets or sets the name of the font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The name of the font.</value>
		public string Name
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().Name;

				//return null;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontNameGetter, null);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).Name = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				this.SetFontProperty(value, null, Utilities.FontNameSetter);
			}
		}

		#endregion Name

		#region StartIndex

		/// <summary>
		/// Gets the index of the first character covered by this font.
		/// </summary>
		/// <value>The index of the first character covered by this font.</value>
		public int StartIndex
		{
			get { return this.startIndex; }
		}

		#endregion StartIndex

		#region Strikeout

		/// <summary>
		/// Gets or sets the value indicating whether the font is struck out.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The value indicating whether the font is struck out.</value>
		public ExcelDefaultableBoolean Strikeout
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().Strikeout;

				//return ExcelDefaultableBoolean.Default;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontStrikeoutGetter, ExcelDefaultableBoolean.Default);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//if (Utilities.IsExcelDefaultableBooleanDefined(value) == false)
				//    throw new InvalidEnumArgumentException("value", (int)value, typeof(ExcelDefaultableBoolean));

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).Strikeout = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				Utilities.VerifyEnumValue(value);
				this.SetFontProperty(value, ExcelDefaultableBoolean.Default, Utilities.FontStrikeoutSetter);
			}
		}

		#endregion Strikeout

		#region SuperscriptSubscriptStyle

		/// <summary>
		/// Gets or sets the value indicating whether the font is superscript or subscript.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="FontSuperscriptSubscriptStyle"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The value indicating whether the font is superscript or subscript.</value>
		public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().SuperscriptSubscriptStyle;

				//return FontSuperscriptSubscriptStyle.Default;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontSuperscriptSubscriptStyleGetter, FontSuperscriptSubscriptStyle.Default);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//if (Utilities.IsFontSuperscriptSubscriptStyleDefined(value) == false)
				//    throw new InvalidEnumArgumentException("value", (int)value, typeof(FontSuperscriptSubscriptStyle));

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).SuperscriptSubscriptStyle = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				Utilities.VerifyEnumValue(value);
				this.SetFontProperty(value, FontSuperscriptSubscriptStyle.Default, Utilities.FontSuperscriptSubscriptStyleSetter);
			}
		}

		#endregion SuperscriptSubscriptStyle

		#region UnderlineStyle

		/// <summary>
		/// Gets or sets the underline style of the font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// All characters in the selection range of this font will be affected by setting this property.
		/// Getting this property will return a value which indicates the formatting of the first character
		/// in this font's range.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> default values cannot be used in <see cref="FormattedString"/> values applied to cells. If this font belongs to a FormattedString 
		/// which is the value of a cell, and a default value is assigned, it will be ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="FontUnderlineStyle"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The formatted string or text associated with the font is not assigned to a cell, comment, or shape.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The font's selection range is outside the valid character range of the associated formatted string or text.
		/// </exception>
		/// <value>The underline style of the font.</value>
		public FontUnderlineStyle UnderlineStyle
		{
			get
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a GetFontProperty helper method.
				#region Refactored

				//this.VerifyState();

				//FormattingRunBase startingRun = this.StartingRun;
				//if (startingRun != null)
				//    return startingRun.GetFont().UnderlineStyle;

				//return FontUnderlineStyle.Default;

				#endregion // Refactored
				return this.GetFontProperty(Utilities.FontUnderlineStyleGetter, FontUnderlineStyle.Default);
			}
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a SetFontProperty helper method.
				#region Refactored

				//if (Utilities.IsFontUnderlineStyleDefined(value) == false)
				//    throw new InvalidEnumArgumentException("value", (int)value, typeof(FontUnderlineStyle));

				//this.VerifyState();

				//this.BeforeSet();
				//Workbook workbook = this.formattedItem.Workbook;
				//foreach (FormattingRunBase run in this.GetControlledRuns())
				//{
				//    if (run != null)
				//        run.GetFont(workbook).UnderlineStyle = value;
				//}
				//this.AfterSet();

				#endregion // Refactored
				Utilities.VerifyEnumValue(value);
				this.SetFontProperty(value, FontUnderlineStyle.Default, Utilities.FontUnderlineStyleSetter);
			}
		}

		#endregion UnderlineStyle

		#endregion Public Properties

		#region Internal Properties

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region AllowDefaultValues

		internal virtual bool AllowDefaultValues
		{
			get { return true; }
		}

		#endregion // AllowDefaultValues

		#region FormattedItem

		internal IFormattedItem FormattedItem
		{
			get { return this.formattedItem; }
		}

		#endregion FormattedItem

		#region StartingFont






		internal abstract FormattingRunBase StartingRun { get; }

		#endregion StartingFont

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