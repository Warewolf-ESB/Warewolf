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
	/// Represents a paragraph in <see cref="T:FormattedText"/>.
	/// </summary>
	/// <seealso cref="T:FormattedText.Paragraphs"/>
	[DebuggerDisplay("Paragraph: {UnformattedString}")]



	public

		class FormattedTextParagraph : IFormattedRunOwner
	{
		#region Member Variables

		private HorizontalTextAlignment alignment = HorizontalTextAlignment.Left;
		private readonly string endingNewLine;
		private List<FormattingRunBase> formattingRuns;
		private FormattedText formattedText;
		private int startIndex;
		private string unformattedString;

		#endregion  // Member Variables

		#region Constructor

		internal FormattedTextParagraph(FormattedText owner, int startIndex, string unformattedString, string endingNewLine)
		{
			if (unformattedString == null)
			{
				Utilities.DebugFail("The unformatted string cannot be null.");
				unformattedString = string.Empty;
			}

			this.formattedText = owner;
			this.StartIndex = startIndex;
			this.unformattedString = unformattedString;
			this.endingNewLine = endingNewLine ?? FormattedText.NewLine;

			this.formattingRuns = new List<FormattingRunBase>();
		}

		#endregion  // Constructor

		#region Interfaces

		#region IFormattedRunOwner Members

		void IFormattedRunOwner.AddRun(FormattingRunBase run)
		{
			FormattedTextRun textRun = run as FormattedTextRun;
			if (textRun == null)
			{
				Utilities.DebugFail("The run should be a FormattedTextRun instance.");
				return;
			}

			this.formattingRuns.Add(textRun);
		}

		FormattingRunBase IFormattedRunOwner.CreateRun(int absoluteStartIndex)
		{
			return new FormattedTextRun(this, absoluteStartIndex - this.startIndex);
		}

		List<FormattingRunBase> IFormattedRunOwner.GetFormattingRuns(Workbook workbook)
		{
			return this.GetFormattingRuns(workbook);
		}

		void IFormattedRunOwner.InsertRun(int runIndex, FormattingRunBase run)
		{
			FormattedTextRun textRun = run as FormattedTextRun;
			if (textRun == null)
			{
				Utilities.DebugFail("The run should be a FormattedTextRun instance.");
				return;
			}

			this.formattingRuns.Insert(runIndex, textRun);
		}

		// MD 2/2/12 - TFS100573
		//Workbook IFormattedRunOwner.Workbook
		//{
		//    get { return this.Workbook; }
		//}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Internal Methods

		#region AddFormattingRun

		internal void AddFormattingRun(FormattedTextRun run)
		{
			this.InsertFormattingRun(this.formattingRuns.Count, run);
		}

		#endregion  // AddFormattingRun

		#region Clone

		internal FormattedTextParagraph Clone(FormattedText newOwner)
		{
			FormattedTextParagraph clone = new FormattedTextParagraph(newOwner, this.startIndex, this.unformattedString, this.endingNewLine);
			clone.alignment = this.alignment;

			for (int i = 0; i < this.formattingRuns.Count; i++)
			{
				// MD 2/2/12 - TFS100573
				//clone.formattingRuns.Add(((FormattedTextRun)this.formattingRuns[i]).Clone(clone));
				clone.formattingRuns.Add(((FormattedTextRun)this.formattingRuns[i]).Clone(this.Workbook, clone));
			}

			return clone;
		}

		#endregion  // Clone

		#region GetFormattingRuns

		internal List<FormattingRunBase> GetFormattingRuns(Workbook workbook)
		{
			this.VerifyFormattingRuns(workbook);
			return this.formattingRuns;
		}

		#endregion  // GetFormattingRuns

		#region InsertFormattingRun

		internal void InsertFormattingRun(int runIndex, FormattedTextRun run)
		{
			this.formattingRuns.Insert(runIndex, run);
			this.VerifyFormattingRuns(null);
		}

		#endregion  // InsertFormattingRun

		#region OnRemovedFromFormattedString

		internal void OnRemovedFromFormattedString()
		{
			this.formattedText = null;
			this.StartIndex = 0;
		}

		#endregion  // OnRemovedFromFormattedString

		#endregion  // Internal Methods

		#region Private Methods

		#region VerifyFormattingRuns

		private void VerifyFormattingRuns(Workbook workbook)
		{
			if (workbook == null)
				workbook = this.Workbook;

			if (this.formattingRuns.Count == 0 || this.formattingRuns[0].FirstFormattedCharAbsolute != this.startIndex)
				this.formattingRuns.Insert(0, new FormattedTextRun(this, 0));
		}

		#endregion  // VerifyFormattingRuns

		#endregion  // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Alignment

		/// <summary>
		/// Gets or sets the alignment of the paragraph.
		/// </summary>
		public HorizontalTextAlignment Alignment
		{
			get { return this.alignment; }
			set
			{
				if (this.alignment == value)
					return;

				if (Enum.IsDefined(typeof(HorizontalTextAlignment), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(HorizontalTextAlignment));

				this.alignment = value;

				this.formattedText.OnFormattingChanged();
			}
		}

		#endregion // Alignment

		#region FormattingText

		/// <summary>
		/// Gets the owning <see cref="T:FormattedText"/> to which the paragraph belongs or null if the 
		/// paragraph has been removed from its owning formatted text.
		/// </summary>
		public FormattedText FormattedText
		{
			get { return this.formattedText; }
		}

		#endregion  // FormattingText

		#region StartIndex

		/// <summary>
		/// Gets the zero-based index of the paragraph's first character in the overall formatted text.
		/// </summary>
		public int StartIndex
		{
			get { return this.startIndex; }
			internal set 
			{
				Debug.Assert(value >= 0, "The start index should never be less than 0.");
				this.startIndex = value; 
			}
		}

		#endregion StartIndex

		#region UnformattedString

		/// <summary>
		/// Gets or sets the raw string of the paragraph.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the new unformatted string assigned is shorter than the previous unformatted string, all formatting
		/// outside the range of the new value will be lost.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is a null string.
		/// </exception>
		/// <value>The unformatted string.</value>
		public string UnformattedString
		{
			get { return this.unformattedString; }
			set
			{
				if (this.UnformattedString == value)
					return;

				if (value == null)
					throw new ArgumentNullException("unformattedString", SR.GetString("LE_ArgumentNullException_UnformattedString"));

				int oldLengthResolved = this.LengthResolved;
				this.unformattedString = value;

				Utilities.TrimFormattingRuns(this);

				if (this.formattedText != null)
					this.formattedText.OnParagraphUnformattedStringChanged(this, oldLengthResolved, this.LengthResolved);
			}
		}

		#endregion UnformattedString

		#endregion  // Public Properties

		#region Internal Properties

		#region EndingNewLine

		internal string EndingNewLine
		{
			get { return this.endingNewLine; }
		}

		#endregion  // EndingNewLine

		#region LengthResolved

		internal int LengthResolved
		{
			get { return this.GetLengthResolved(null); }
		}

		internal int GetLengthResolved(bool? isLastParagraph)
		{
			if (isLastParagraph.HasValue == false)
			{
				if (this.formattedText != null)
				{
					FormattedTextParagraphCollection paragraphs = this.formattedText.Paragraphs;
					isLastParagraph = (paragraphs[paragraphs.Count - 1] == this);
				}
				else
				{
					isLastParagraph = false;
				}
			}

			int length = this.unformattedString.Length;
			if (isLastParagraph.Value == false)
				length += this.endingNewLine.Length;

			return length;
		}

		#endregion  // LengthResolved

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (this.formattedText == null)
					return null;

				return this.formattedText.Workbook;
			}
		}

		#endregion  // Workbook

		#endregion  // Internal Properties

		#endregion  // Properties
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