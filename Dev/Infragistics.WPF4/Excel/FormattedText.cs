using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using System.IO;





using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 11/8/11 - TFS85193
	/// <summary>
	/// Represents text with multiple paragraphs and mixed formatting in a shape.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The formatting of the string is controlled in a similar fashion as it would be in Microsoft Excel. In Excel, the user
	/// must select a portion of the text and set the various formatting properties of that selected text. 
	/// </p>
	/// <p class="body">
	/// With the FormattedText, a portion of the text is "selected" by calling either <see cref="FormattedText.GetFont(int)"/> 
	/// or <see cref="FormattedText.GetFont(int,int)"/>. Formatting properties are then set on the returned 
	/// <see cref="FormattedTextFont"/> and all characters in the font's selection range are given these properties.
	/// </p>
	/// <p class="body">
	/// Getting the formatting properties of a <see cref="FormattedTextFont"/> will return the formatting of the first 
	/// character in font's selection range.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetShapeWithText.Text"/>
	[DebuggerDisplay("FormattedText: {ToString()}")]



	public

		 class FormattedText :

	    ICloneable,

		IFormattedItem
	{
		#region Constants

		internal const string NewLine = "\n";

		#endregion  // Constants

		#region Member Variables

		private string cachedUnformattedString;
		private IFormattedTextOwner owner;
		private FormattedTextParagraphCollection paragraphs;

		// MD 7/5/12 - TFS115687
		private Dictionary<string, string> roundTrip2007Properties = new Dictionary<string, string>();

		private VerticalTextAlignment verticalAlignment = VerticalTextAlignment.Top;

		#endregion Member Variables

		#region Constructor

		internal FormattedText()
		{
			this.paragraphs = new FormattedTextParagraphCollection(this);

			// MD 7/5/12 - TFS115687
			// By default, we want to write out 0 for the rtlCol attribute if it is not loaded.
			this.roundTrip2007Properties["rtlCol"] = "0";
		}

		/// <summary>
		/// Creates a new instance of the <see cref="FormattedText"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="unformattedString"/> is null.
		/// </exception>
		/// <param name="unformattedString">
		/// The string that will be displayed in the shape. Each line of the string will be placed in a separate paragraph of the 
		/// <see cref="Paragraphs"/> collection.
		/// </param>
		public FormattedText(string unformattedString)
			: this()
		{
			if (unformattedString == null)
				throw new ArgumentNullException("unformattedString", SR.GetString("LE_ArgumentNullException_UnformattedString"));

			List<int> lineIndexes;
			List<string> newLines;
			List<String> lines = Utilities.GetLines(unformattedString, out lineIndexes, out newLines);
			for (int i = 0; i < lineIndexes.Count; i++)
			{
				int newLineIndex = lineIndexes[i];
				string line = lines[i];

				string newLine = null;
				if (i < newLines.Count)
					newLine = newLines[i];

				FormattedTextParagraph paragraph = new FormattedTextParagraph(this, newLineIndex, line, newLine);
				this.paragraphs.InsertInternal(i, paragraph, newLine);
			}

			this.ClearCache();
		}

		#endregion Constructor

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Returns the string that represents the <see cref="FormattedText"/>, which is the unformatted string.
		/// </summary>
		/// <returns>The string that represents the FormattedText.</returns>
		public override string ToString()
		{
			if (this.cachedUnformattedString == null)
			{
				StringBuilder sb = new StringBuilder();

				for (int i = 0; i < this.paragraphs.Count; i++)
				{
					FormattedTextParagraph paragraph = this.paragraphs[i];
					sb.Append(paragraph.UnformattedString);

					if (i < this.paragraphs.Count - 1)
						sb.Append(paragraph.EndingNewLine);
				}

				this.cachedUnformattedString = sb.ToString();
			}

			return this.cachedUnformattedString;
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Interfaces

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		#endregion

		#region IFormattedItem Members

		object IFormattedItem.Owner
		{
			get { return this.owner; }
		}

		Workbook IFormattedItem.Workbook
		{
			get { return this.Workbook; }
		}

		IFormattedRunOwner IFormattedItem.GetOwnerAt(int startIndex)
		{
			for (int i = this.paragraphs.Count - 1; i >= 0; i--)
			{
				FormattedTextParagraph paragraph = paragraphs[i];
				if (paragraph.StartIndex <= startIndex)
					return paragraph;
			}

			return null;
		}

		void IFormattedItem.OnFormattingChanged()
		{
			this.OnFormattingChanged();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Clone

		/// <summary>
		/// Creates a new <see cref="FormattedText"/> that is a copy of this one.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This should be used if the same formatted text needs to be used in multiple shapes.
		/// The FormattedText class can only exist as the <see cref="WorksheetShapeWithText.Text"/>
		/// of one shape at a time. If the FormattedText is already the text of a shape, and needs
		/// to be set as the text of another shape, clone the FormattedText and set the returned
		/// clone as text of the shape.
		/// </p>
		/// <p class="body">
		/// The cloned FormattedText only takes its original configuration for this instance.
		/// If this instance is cloned and than changed, the clone will not be changed as well; it will
		/// remain as it was when it was cloned.
		/// </p>
		/// </remarks>
		/// <returns>A new FormattedText that is a copy of this one.</returns>
		public FormattedText Clone()
		{
			FormattedText clone = new FormattedText();
			clone.verticalAlignment = this.verticalAlignment;
			clone.Paragraphs.InitializeFrom(this.Paragraphs);
			clone.ClearCache();
			return clone;
		}

		#endregion Clone

		#region GetFont (int)

		/// <summary>
		/// Gets the font which controls the formatting properties in the string from the specified start index to 
		/// the end of the string.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the start index is greater than or equal to the length of the unformatted string, no exception 
		/// will be thrown. It will be thrown later when one of the formatting properties of the returned
		/// <see cref="FormattedTextFont"/> is set.
		/// </p>
		/// </remarks>
		/// <param name="startIndex">The index of the first character the returned font controls.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is less than zero.
		/// </exception>
		/// <returns>
		/// A FormattedTextFont instance which controls the formatting of the end portion of the string.
		/// </returns>
		public FormattedTextFont GetFont(int startIndex)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex", startIndex, SR.GetString("LE_ArgumentOutOfRangeException_NegativeStartIndex"));

			return new FormattedTextFont(this, startIndex, 0);
		}

		#endregion GetFont (int)

		#region GetFont (int, int)

		/// <summary>
		/// Gets the font which controls the formatting properties in the string from the specified start index for
		/// the specified number of characters.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the start index is greater than or equal to the length of the unformatted string, no exception 
		/// will be thrown. It will be thrown later when one of the formatting properties of the returned
		/// <see cref="FormattedTextFont"/> is set.
		/// </p>
		/// </remarks>
		/// <param name="startIndex">The index of the first character the returned font controls.</param>
		/// <param name="length">The number of characters after the start index controlled by the returned font.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is less than zero.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="length"/> is less than one. A zero length string cannot be controlled by a formatting font.
		/// </exception>
		/// <returns>
		/// A FormattedTextFont instance which controls the formatting of a portion of the string.
		/// </returns>
		public FormattedTextFont GetFont(int startIndex, int length)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex", startIndex, SR.GetString("LE_ArgumentOutOfRangeException_NegativeStartIndex"));

			if (length < 1)
				throw new ArgumentOutOfRangeException("length", length, SR.GetString("LE_ArgumentOutOfRangeException_LengthMustBePositive"));

			return new FormattedTextFont(this, startIndex, length);
		}

		#endregion GetFont (int, int)

		#endregion  // Public Methods

		#region Internal Methods

		#region ClearCache

		internal void ClearCache()
		{
			this.cachedUnformattedString = null;
		}

		#endregion  // ClearCache

		#region InitSerializationCache

		internal void InitSerializationCache(WorkbookSerializationManager serializationManager, IWorkbookFontDefaultsResolver fontDefaultsResolver)
		{
			// MD 1/18/12 - 12.1 - Cell Format Updates
			//FontResolverType fontResolverType = FontResolverType.Normal;
			//if (this.owner is WorksheetShapeWithText)
			//    fontResolverType = FontResolverType.ShapeWithText;

			// Add the formatted fonts from the formatted text to the serialization manager
			foreach (FormattedTextParagraph paragraph in this.Paragraphs)
			{
				foreach (FormattedTextRun run in paragraph.GetFormattingRuns(serializationManager.Workbook))
				{
					// MD 1/10/12 - 12.1 - Cell Format Updates
					//serializationManager.AddFont(run.GetFontInternal(serializationManager.Workbook).Element, fontDefaultsResolver, fontResolverType, true);
					serializationManager.AddFont(run.GetFontInternal(serializationManager.Workbook), fontDefaultsResolver);
				}
			}
		}

		#endregion  // InitSerializationCache

		#region OnFormattingChanged

		internal void OnFormattingChanged()
		{
			if (this.Owner != null)
				this.Owner.OnFormattingChanged(this);
		}

		#endregion  // OnFormattingChanged

		#region OnParagraphUnformattedStringChanged

		internal void OnParagraphUnformattedStringChanged(FormattedTextParagraph paragraph, int oldLengthResolved, int newLengthResolved)
		{
			int paragraphIndex = this.paragraphs.IndexOf(paragraph);

			if (paragraphIndex < 0)
			{
				Utilities.DebugFail("The paragraph is not in the collection.");
				return;
			}

			int difference = newLengthResolved - oldLengthResolved;
			for (int i = paragraphIndex + 1; i < this.paragraphs.Count; i++)
				this.paragraphs[i].StartIndex += difference;

			this.ClearCache();

			if (this.owner != null)
				this.owner.OnUnformattedStringChanged(this);
		}

		#endregion  // OnParagraphUnformattedStringChanged

		#region SetWorksheet

		internal void SetWorksheet(Worksheet worksheet)
		{
			Workbook workbook = null;
			if (worksheet != null)
				workbook = worksheet.Workbook;

			GenericCachedCollection<StringElement> newSharedStringTable =
				workbook == null ? null : workbook.SharedStringTable;
			GenericCachedCollection<WorkbookFontData> newCollection =
				workbook == null ? null : workbook.Fonts;

			foreach (FormattedTextParagraph paragraph in this.Paragraphs)
			{
				foreach (FormattingRunBase run in paragraph.GetFormattingRuns(workbook))
				{
					if (run.HasFont)
						run.GetFontInternal(workbook).OnRooted(newCollection);
				}
			}
		}

		#endregion // SetWorksheet

		#region VerifyNewOwner

		internal void VerifyNewOwner(WorksheetShapeWithText newOwner)
		{
			if (this.owner != null && this.owner != newOwner)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FormattedTextAlreadyOwned"));
		}

		#endregion  // VerifyNewOwner

		#endregion  // Internal Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Paragraphs

		/// <summary>
		/// Gets the paragraphs in the formatted text.
		/// </summary>
		public FormattedTextParagraphCollection Paragraphs
		{
			get { return this.paragraphs; }
		}

		#endregion  // Paragraphs

		#region VerticalAlignment

		/// <summary>
		/// Gets or sets the vertical alignment of the formatted text in the owning shape.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The specified value is not defined in the <see cref="VerticalTextAlignment"/> enumeration.
		/// </exception>
		public VerticalTextAlignment VerticalAlignment
		{
			get { return this.verticalAlignment; }
			set
			{
				if (this.VerticalAlignment == value)
					return;

				if (Enum.IsDefined(typeof(VerticalTextAlignment), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(VerticalTextAlignment));

				this.verticalAlignment = value;
				this.OnFormattingChanged();
			}
		}

		#endregion  // VerticalAlignment

		#endregion Public Properties

		#region Internal Properties

		#region Owner

		internal IFormattedTextOwner Owner
		{
			get { return this.owner; }
			set
			{
				if (this.owner == value)
					return;

				this.owner = value;

				Worksheet worksheet = this.owner == null ? null : this.owner.Worksheet;
				this.SetWorksheet(worksheet);
			}
		}

		#endregion Owner

		// MD 7/5/12 - TFS115687
		#region RoundTrip2007Properties

		internal Dictionary<string, string> RoundTrip2007Properties
		{
			get { return roundTrip2007Properties; }
		}

		#endregion // RoundTrip2007Properties

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (this.owner == null)
					return null;

				Worksheet worksheet = this.owner.Worksheet;
				if (worksheet == null)
					return null;

				return worksheet.Workbook;
			}
		}

		#endregion  // Workbook

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