using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 11/9/11 - TFS85193
	internal class CellFormattingRunPlaceholder : FormattingRunBase
	{
		#region Member Variables

		private IWorkbookFont font;

		#endregion  // Member Variables

		#region Constructor

		// MD 1/31/12 - TFS100573
		//public CellFormattingRunPlaceholder(FormattedStringElement owner, IWorkbookFont font)
		//    : base(owner, 0)
		public CellFormattingRunPlaceholder(StringElement stringElement, IWorkbookFont font)
			: base(new RunOwner(stringElement), 0)
		{
		    this.font = font;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region FirstFormattedCharAbsolute






		public override int FirstFormattedCharAbsolute
		{
			get { return 0; }
			set { Utilities.DebugFail("This should never be set."); }
		}

		#endregion  // FirstFormattedCharAbsolute

		#region GetFont

		public override IWorkbookFont GetFont(Workbook workbook)
		{
			return this.font;
		}

		#endregion  // GetFont

		#region GetFontInternal

		public override WorkbookFontProxy GetFontInternal(Workbook workbook)
		{
			Utilities.DebugFail("There is no WorkbookFontProxy for this type of run.");
			return null;
		}

		#endregion  // GetFontInternal

		#region HasFont

		public override bool HasFont
		{
			get { return true; }
		}

		#endregion  // HasFont

		#region InitializeFrom

		public override void InitializeFrom(FormattingRunBase otherRun, Workbook workbook)
		{
			Utilities.DebugFail("We should never try to initialize the cell formatting run from another run.");
		}

		#endregion  // InitializeFrom

		#region UnformattedString

		public override string UnformattedString
		{
			get
			{
				List<FormattingRunBase> runs = this.Owner.GetFormattingRuns(null);
				if (runs.Count == 0)
					return this.Owner.UnformattedString;

				return this.Owner.UnformattedString.Substring(0, runs[0].FirstFormattedCharInOwner);
			}
		}

		#endregion  // UnformattedString

		#endregion  // Base Class Overrides


		// MD 1/31/12 - TFS100573
		private class RunOwner : IFormattedRunOwner
		{
			private StringElement stringElement;

			public RunOwner(StringElement stringElement)
			{
				this.stringElement = stringElement;
			}

			#region Interfaces

			// MD 1/31/12 - TFS100573
			#region IFormattedRunOwner Members

			void IFormattedRunOwner.AddRun(FormattingRunBase run)
			{
				Utilities.DebugFail("This operation is not supported.");
			}

			FormattingRunBase IFormattedRunOwner.CreateRun(int absoluteStartIndex)
			{
				Utilities.DebugFail("This operation is not supported.");
				return new FormattedStringRun(this.stringElement as FormattedStringElement, absoluteStartIndex);
			}

			List<FormattingRunBase> IFormattedRunOwner.GetFormattingRuns(Workbook workbook)
			{
				Utilities.DebugFail("This operation is not supported.");

				FormattedStringElement formattedStringElement = this.stringElement as FormattedStringElement;
				if (formattedStringElement == null)
					return new List<FormattingRunBase>();

				return formattedStringElement.FormattingRuns;
			}

			void IFormattedRunOwner.InsertRun(int runIndex, FormattingRunBase run)
			{
				Utilities.DebugFail("This operation is not supported.");
			}

			int IFormattedRunOwner.StartIndex
			{
				get { return 0; }
			}

			string IFormattedRunOwner.UnformattedString
			{
				get { return this.stringElement.UnformattedString; }
			}

			// MD 2/2/12 - TFS100573
			//Workbook IFormattedRunOwner.Workbook
			//{
			//    get { return this.stringElement.Workbook; }
			//}

			#endregion

			#endregion // Interfaces
		}
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