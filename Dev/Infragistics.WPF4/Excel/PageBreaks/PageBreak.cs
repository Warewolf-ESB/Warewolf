using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Base class for horizontal and vertical page breaks in a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.ClearPageBreaks"/>
	/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetCell)"/>
	/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetColumn)"/>
	/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetRow)"/>
	/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
	/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
	/// <seealso cref="HorizontalPageBreak"/>
	/// <seealso cref="VerticalPageBreak"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 abstract class PageBreak
	{
		#region Member Variables

		private bool createdByPivotTable = false;

		// MD 3/26/12 - 12.1 - Table Support
		//private readonly int id;
		private int id;

		private bool manuallyCreated = true;
		private readonly int? max;
		private readonly int? min;
		private WorksheetRegion printArea;

		#endregion  // Member Variables

		#region Constructor

		internal PageBreak(int id, int? min, int? max)
		{
			this.id = id;
			this.min = min;
			this.max = max;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether this <see cref="PageBreak"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test.</param>
		/// <returns>True if the object is the same type as this PageBreak and has the same data; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (this.GetType() != obj.GetType())
				return false;

			PageBreak other = (PageBreak)obj;

			if (this.createdByPivotTable != other.createdByPivotTable)
				return false;

			if (this.manuallyCreated != other.manuallyCreated)
				return false;

			return this.CompareToHelper(other) == 0;
		}

		#endregion  // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="PageBreak"/>.
		/// </summary>
		/// <returns>A hash code for the instance.</returns>
		public override int GetHashCode()
		{
			int hashCode = 0;

			if (this.createdByPivotTable)
				hashCode |= 0x1000;

			if (this.manuallyCreated)
				hashCode |= 0x2000;

			hashCode ^= this.Id ^ this.MaxResolved ^ this.MinResolved;
			return hashCode;
		}

		#endregion  // GetHashCode

		#endregion  // Base Class Overrides

		#region Methods

		internal int CompareToHelper(PageBreak other)
		{
			int compareVal = this.Id - other.Id;

			if (compareVal != 0)
				return compareVal;

			compareVal = this.MinResolved - other.MinResolved;

			if (compareVal != 0)
				return compareVal;

			compareVal = this.MaxResolved - other.MaxResolved;

			if (compareVal != 0)
				return compareVal;

			if (this.printArea == other.printArea)
				return 0;

			if (this.printArea == null)
				return 1;

			if (other.printArea == null)
				return -1;

			return ((IComparer<WorksheetRegion>)WorksheetRegion.HorizontalSorter.Instance).Compare(this.printArea, other.printArea);
		}

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region PrintArea

		/// <summary>
		/// Gets the print area in which the page break occurs.
		/// </summary>
		/// <value>
		/// A <seealso cref="WorksheetRegion"/> instance that is the print area where the page break occurs or null if the 
		/// page break occurs across the entire sheet.
		/// </value>
		/// <seealso cref="PrintOptions.PrintAreas"/>
		public WorksheetRegion PrintArea
		{
			get { return this.printArea; }
			internal set { this.printArea = value; }
		}

		#endregion  // PrintArea

		#endregion  // Public Properties

		#region Internal Properties

		internal bool CreatedByPivotTable
		{
			get { return this.createdByPivotTable; }
			set { this.createdByPivotTable = value; }
		}

		internal int Id
		{
			get { return this.id; }

			// MD 3/26/12 - 12.1 - Table Support
			set { this.id = value; }
		}

		internal bool ManuallyCreated
		{
			get { return this.manuallyCreated; }
			set { this.manuallyCreated = value; }
		}

		internal int? Max
		{
			get { return this.max; }
		}

		internal int MaxResolved
		{
			get
			{
				int? max = this.Max;
				return max.HasValue ? max.Value : Int32.MaxValue;
			}
		}

		internal int? Min
		{
			get { return this.min; }
		}

		internal int MinResolved
		{
			get
			{
				int? min = this.Min;
				return min.HasValue ? min.Value : 0;
			}
		}

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