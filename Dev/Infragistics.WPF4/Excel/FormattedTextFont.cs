using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 11/8/11 - TFS85193
	/// <summary>
	/// Controls the formatting of a range of characters in <see cref="T:FormattedText"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The formatting of the string is controlled in a similar fashion as it would be in Microsoft Excel. In Excel, the user
	/// must select a portion of the text and set the various formatting properties of that selected text. 
	/// </p>
	/// <p class="body">
	/// With the <see cref="T:FormattedText"/>, a portion of the string is "selected" by calling either 
	/// <see cref="T:FormattedText.GetFont(int)"/> or <see cref="T:FormattedText.GetFont(int,int)"/>. 
	/// Formatting properties are then set on the returned FormattedTextFont and all characters in the font's 
	/// selection range are given these properties.
	/// </p>
	/// <p class="body">
	/// Getting the formatting properties of a FormattedTextFont will return the formatting of the first character 
	/// in font's selection range. 
	/// </p>
	/// </remarks>
	/// <seealso cref="T:FormattedText"/>
	/// <seealso cref="T:FormattedText.GetFont(int)"/>
	/// <seealso cref="T:FormattedText.GetFont(int,int)"/>



	public

		class FormattedTextFont : FormattedFontBase
	{
		#region Constructor

		internal FormattedTextFont(FormattedText formattedString, int startIndex, int length)
			: base(formattedString, startIndex, length) { }

		#endregion Constructor

		#region Base Class Overrides

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

			FormattedText formattedText = this.FormattedText;

			int paragraphIndex = 0;
			for (; paragraphIndex < formattedText.Paragraphs.Count; paragraphIndex++)
			{
				FormattedTextParagraph paragraph = formattedText.Paragraphs[paragraphIndex];

				// MD 1/29/12 - 12.1 - Cell Format Updates
				//this.GetRunsFromOwnerInRange(paragraph, workbook, runs, firstCharacterAfterFont, ref lastRunEnumerated);
				this.GetRunsFromOwnerInRange(paragraph, workbook, runs, firstCharacterAfterFont, fontDefaultsResolver, ref lastRunEnumerated);
				
				if (lastRunEnumerated != null && firstCharacterAfterFont <= lastRunEnumerated.FirstFormattedCharAbsolute)
					break;
			}
		}

		#endregion  // GetRunsInRange

		#region StartingRun

		internal override FormattingRunBase StartingRun
		{
			get
			{
				FormattedText formattedText = this.FormattedText;
				Workbook workbook = formattedText.Workbook;

				List<FormattingRunBase> nextRuns = null;
				for (int paragraphIndex = formattedText.Paragraphs.Count - 1; paragraphIndex >= 0; paragraphIndex--)
				{
					FormattedTextParagraph paragraph = formattedText.Paragraphs[paragraphIndex];
					List<FormattingRunBase> runs = paragraph.GetFormattingRuns(workbook);
					FormattingRunBase run = this.TryGetFirstRunControllingCharacter(runs, this.StartIndex);
					if (run != null)
					{
						// If this font starts in the newline of this paragraph, return the first run of the next paragraph instead, if possible.
						if (paragraph.StartIndex + paragraph.UnformattedString.Length <= this.StartIndex &&
							nextRuns != null &&
							nextRuns.Count != 0)
						{
							return nextRuns[0];
						}

						return run;
					}

					nextRuns = runs;
				}

				Utilities.DebugFail("This should never happen.");
				return null;
			}
		}

		#endregion StartingRun

		#endregion  // Base Class Overrides

		#region Properties

		#region Public Properties

		#region FormattedText

		/// <summary>
		/// Gets the <see cref="T:FormattedText"/> which is controlled by this font.
		/// </summary>
		/// <value>The FormattedText which is controlled by this font.</value>
		public FormattedText FormattedText
		{
			get { return (FormattedText)this.FormattedItem; }
		}

		#endregion FormattedText

		#endregion Public Properties

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