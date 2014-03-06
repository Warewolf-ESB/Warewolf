using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a vertical page break in a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.VerticalPageBreaks"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class VerticalPageBreak : PageBreak, IComparable<VerticalPageBreak>
	{
		#region Constructor

		/// <summary>
		/// Creates a new <see cref="VerticalPageBreak"/> instance.
		/// </summary>
		/// <param name="firstColumOnPage">The 0-based index of the first column on the page after this break.</param>
		public VerticalPageBreak(int firstColumOnPage)
			: this(firstColumOnPage, null, null) { }

		/// <summary>
		/// Creates a new <see cref="VerticalPageBreak"/> instance.
		/// </summary>
		/// <param name="firstColumOnPage">The 0-based index of the first column on the page after this break.</param>
		/// <param name="printArea">The print area in which the page break should occur or null to break in the entire sheet.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="printArea"/> is not null and the <paramref name="firstColumOnPage"/> is outside the print area
		/// or the left column index of the print area.
		/// </exception>
		/// <seealso cref="PrintOptions.PrintAreas"/>
		public VerticalPageBreak(int firstColumOnPage, WorksheetRegion printArea)
			: this(firstColumOnPage, VerticalPageBreak.GetMinMax(printArea, true), VerticalPageBreak.GetMinMax(printArea, false)) 
		{
			this.PrintArea = printArea;

			if (printArea != null)
			{
				if (firstColumOnPage <= printArea.FirstColumn || printArea.LastColumn < firstColumOnPage)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_MustBeWithinPrintArea"), "printArea");
			}
		}

		internal VerticalPageBreak(int firstColumOnPage, int? min, int? max)
			: base(firstColumOnPage, min, max) { }

		#endregion  // Constructor

		#region Interfaces

		#region IComparable<VerticalPageBreak> Members

		int IComparable<VerticalPageBreak>.CompareTo(VerticalPageBreak other)
		{
			return this.CompareToHelper(other);
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region GetMinMax

		private static int? GetMinMax(WorksheetRegion printArea, bool getMin)
		{
			if (printArea == null)
				return null;

			if (getMin)
				return printArea.FirstRow;
			else
				return printArea.LastRow;
		}

		#endregion  // GetMinMax

		#endregion  // Methods

		#region Properties

		/// <summary>
		/// Gets the 0-based index of the first column on the page after this break.
		/// </summary>
		public int FirstColumnOnPage
		{
			get { return this.Id; }
		}

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