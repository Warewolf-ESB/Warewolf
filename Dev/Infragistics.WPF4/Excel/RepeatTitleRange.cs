using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/25/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a range of contiguous rows or columns which should be repeated at the top or left or printed pages of the <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.ColumnsToRepeatAtLeft"/>
	/// <seealso cref="PrintOptions.RowsToRepeatAtTop"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 class RepeatTitleRange
	{
		#region Member Variables

		private readonly int endIndex;
		private readonly int startIndex;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="RepeatTitleRange"/> instance.
		/// </summary>
		/// <param name="startIndex">The index of the first row or column in the range.</param>
		/// <param name="endIndex">The index of the last row or column in the range.</param>
		/// <remarks>
		/// <p class="body">
		/// The order of the <paramref name="startIndex"/> and <paramref name="endIndex"/> does not matter. In other words, if startIndex is greater 
		/// than endIndex, they will be swapped when they are stored.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when either <paramref name="startIndex"/> or <paramref name="endIndex"/> is less than 0.
		/// </exception>
		/// <seealso cref="RowColumnBase.Index"/>
		public RepeatTitleRange(int startIndex, int endIndex)
		{
			if (startIndex < 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_StartIndexLessThanZero"), "startIndex");

			if (endIndex < 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_EndIndexLessThanZero"), "endIndex");

			this.startIndex = Math.Min(startIndex, endIndex);
			this.endIndex = Math.Max(startIndex, endIndex);
		}

		#endregion  // Constructor

		#region Properties

		/// <summary>
		/// Gets the index of the last row or column in the range.
		/// </summary>
		/// <seealso cref="RowColumnBase.Index"/>
		public int EndIndex
		{
			get { return this.endIndex; }
		}

		/// <summary>
		/// Gets the index of the first row or column in the range.
		/// </summary>
		/// <seealso cref="RowColumnBase.Index"/>
		public int StartIndex
		{
			get { return this.startIndex; }
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