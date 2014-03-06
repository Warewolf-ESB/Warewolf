using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 11/8/11 - TFS85193





	internal class FormattedTextRun : FormattingRunBase
	{
		#region Member Variables

		private WorkbookFontProxy font;

		// MD 7/23/12 - TFS117429
		private Dictionary<string, string> roundTrip2007Properties = new Dictionary<string, string>();

		#endregion Member Variables

		#region Constructor

		public FormattedTextRun(FormattedTextParagraph paragraph, int firstFormattedCharInParagraph)
			: base(paragraph, firstFormattedCharInParagraph) { }

		#endregion Constructor

		#region Base Class Overrides

		#region GetFont

		public override IWorkbookFont GetFont(Workbook workbook)
		{
			return this.GetFontInternal(workbook);
		}

		#endregion  // GetFont

		#region GetFontInternal

		public override WorkbookFontProxy GetFontInternal(Workbook workbook)
		{
			return this.GetFontInternal(workbook, ref this.font);
		}

		#endregion  // GetFontInternal

		#region HasFont

		public override bool HasFont
		{
			get { return this.font != null; }
		}

		#endregion HasFont

		// MD 7/23/12 - TFS117429
		#region InitializeFrom

		public override void InitializeFrom(FormattingRunBase otherRun, Workbook workbook)
		{
			FormattedTextRun other = (FormattedTextRun)otherRun;
			other.roundTrip2007Properties = new Dictionary<string, string>(this.roundTrip2007Properties);

			base.InitializeFrom(otherRun, workbook);
		}

		#endregion // InitializeFrom

		#endregion  // Base Class Overrides

		#region Properties

		#region FirstFormattedCharAbsolute






		public override int FirstFormattedCharAbsolute
		{
			get
			{
				return this.Paragraph.StartIndex + this.FirstFormattedCharInOwner;
			}
			set
			{
				int newValue = value - this.Paragraph.StartIndex;
				if (newValue <= 0)
				{
					Utilities.DebugFail("The first formatted character can never be less than zero.");
					return;
				}

				this.FirstFormattedCharInOwner = newValue;
			}
		}

		#endregion FirstFormattedCharAbsolute

		#region Paragraph

		public FormattedTextParagraph Paragraph
		{
			get { return (FormattedTextParagraph)this.Owner; }
		}

		#endregion // Paragraph

		// MD 7/23/12 - TFS117429
		#region RoundTrip2007Properties

		internal Dictionary<string, string> RoundTrip2007Properties
		{
			get { return roundTrip2007Properties; }
		}

		#endregion // RoundTrip2007Properties

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