using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 11/9/11 - TFS85193
	internal abstract class FormattingRunBase
	{
		#region Member Variables

		private IFormattedRunOwner owner;
		private int firstFormattedCharInOwner;

		#endregion  // Member Variables

		#region Constructor

		public FormattingRunBase(IFormattedRunOwner owner, int firstFormattedCharInOwner) 
		{
			Debug.Assert(owner != null, "The owner cannot be null.");
			Debug.Assert(firstFormattedCharInOwner >= 0, "The run cannot refer to characters outside of the string.");
			this.owner = owner;
			this.firstFormattedCharInOwner = firstFormattedCharInOwner;
		}

		#endregion  // Constructor

		#region Methods






		public abstract int FirstFormattedCharAbsolute { get; set; }

		public abstract IWorkbookFont GetFont(Workbook workbook);
		public abstract WorkbookFontProxy GetFontInternal(Workbook workbook);

		#region Clone






		// MD 2/2/12 - TFS100573
		//public FormattingRunBase Clone(IFormattedRunOwner newOwner)
		public FormattingRunBase Clone(Workbook workbook, IFormattedRunOwner newOwner)
		{
			FormattingRunBase clone = newOwner.CreateRun(this.FirstFormattedCharAbsolute);

			// MD 2/2/12 - TFS100573
			//clone.InitializeFrom(this, newOwner.Workbook ?? this.Owner.Workbook);
			clone.InitializeFrom(this, workbook);

			return clone;
		}

		#endregion  // Clone

		// MD 2/2/12 - TFS100573
		// Removed this so the caller is forced to specify a Workbook.
		#region Removed

		//#region GetFont

		//public IWorkbookFont GetFont()
		//{
		//    return this.GetFont(this.Owner.Workbook);
		//}

		//#endregion  // GetFont

		#endregion // Removed

		#region GetFontInternal

		public WorkbookFontProxy GetFontInternal()
		{
			return this.GetFontInternal(null);
		}

		public WorkbookFontProxy GetFontInternal(Workbook workbook, ref WorkbookFontProxy font)
		{
			if (font == null)
			{
				// MD 2/2/12 - TFS100573
				// The owner no longer has a workbook reference.
				//if (workbook == null)
				//    workbook = this.Owner.Workbook;

				// MD 1/8/12 - 12.1 - Cell Format Updates
				// There is really no need to cache the empty element anymore. We can just create a new one.
				//GenericCachedCollection<WorkbookFontData> fonts = null;
				//
				//WorkbookFontData emptyElement = null;
				//if (workbook != null)
				//{
				//    fonts = workbook.Fonts;
				//    emptyElement = fonts.EmptyElement;
				//}
				//else
				//{
				//    emptyElement = new WorkbookFontData(null);
				//}
				font = new WorkbookFontProxy(new WorkbookFontData(workbook), workbook);
			}

			return font;
		}

		#endregion  // GetFontInternal

		#region InitializeFrom

		public virtual void InitializeFrom(FormattingRunBase otherRun, Workbook workbook)
		{
			if (otherRun.HasFont)
				this.GetFont(workbook).SetFontFormatting(otherRun.GetFont(workbook));
		}

		#endregion  // InitializeFrom

		#endregion  // Methods

		#region Properties

		public abstract bool HasFont { get; }

		#region FirstFormattedCharInOwner

		public int FirstFormattedCharInOwner
		{
			get { return this.firstFormattedCharInOwner; }
			set { this.firstFormattedCharInOwner = value; }
		}

		#endregion  // FirstFormattedCharInOwner

		#region Owner

		public IFormattedRunOwner Owner
		{
			get { return this.owner; }
		}

		#endregion  // Owner

		#region UnformattedString






		public virtual string UnformattedString
		{
			get
			{
				List<FormattingRunBase> runs = this.Owner.GetFormattingRuns(null);
				int index = runs.IndexOf(this);

				if (index < 0)
					return string.Empty;

				object overallStringOwner = this.Owner.UnformattedString;
				if (overallStringOwner == null)
					return string.Empty;

				string text = overallStringOwner.ToString();
				int length = text.Length;
				FormattingRunBase nextRun = index < (runs.Count - 1) ? runs[index + 1] : null;
				int firstChar = this.firstFormattedCharInOwner;
				return nextRun == null ?
					text.Substring(firstChar) :
					text.Substring(firstChar, nextRun.firstFormattedCharInOwner - firstChar);
			}
		}

		#endregion UnformattedString

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