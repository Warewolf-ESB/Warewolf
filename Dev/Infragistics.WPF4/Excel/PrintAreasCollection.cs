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
	/// Gets the collection of print areas in a <see cref="Worksheet"/> or a worksheet's print settings in a <see cref="CustomView"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.PrintAreas"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 class PrintAreasCollection : ICollection<WorksheetRegion>
	{
		#region Member Variables

		private List<WorksheetRegion> regions;
		private PrintOptions owner;

		#endregion  // Member Variables

		#region Constructor

		internal PrintAreasCollection(PrintOptions owner)
		{
			this.owner = owner;
			this.regions = new List<WorksheetRegion>();
		}

		#endregion  // Constructor

		#region Interfaces

		#region ICollection<WorksheetRegion> Members

		void ICollection<WorksheetRegion>.CopyTo(WorksheetRegion[] array, int arrayIndex)
		{
			this.regions.CopyTo(array, arrayIndex);
		}

		bool ICollection<WorksheetRegion>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<WorksheetRegion> Members

		IEnumerator<WorksheetRegion> IEnumerable<WorksheetRegion>.GetEnumerator()
		{
			return this.regions.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.regions.GetEnumerator();
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a print area to the collection.
		/// </summary>
		/// <param name="printArea">The print area to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="printArea"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="printArea"/> overlaps with another print area in the collection.
		/// </exception>
		public void Add(WorksheetRegion printArea)
		{
			if (printArea == null)
				throw new ArgumentNullException("printArea");

			for (int i = 0; i < this.regions.Count; i++)
			{
				if (this.regions[i].IntersectsWith(printArea))
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PA_PrintAreasCannotOverlap"));
			}

			this.AddHelper(printArea);
			this.owner.UpdatePrintAreasNamedReference();
		}

		#endregion  // Add

		#region Clear

		/// <summary>
		/// Clears all print areas from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any page breaks are contained in a print area, they will be removed from their collection.
		/// </p>
		/// </remarks>
		/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
		/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
		public void Clear()
		{
			if (this.Count == 0)
				return;

			for (int i = this.Count - 1; i >= 0; i--)
				this.RemoveHelper(i);

			this.owner.UpdatePrintAreasNamedReference();
		}

		#endregion  // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified print area is in the collection.
		/// </summary>
		/// <param name="printArea">The print area to search for in the collection.</param>
		/// <returns>True if the print area is in the collection; False otherwise.</returns>
		public bool Contains(WorksheetRegion printArea)
		{
			if (printArea == null)
				return false;

			return this.regions.Contains(printArea);
		}

		#endregion  // Contains

		#region Remove

		/// <summary>
		/// Removes the specified print area from the collection.
		/// </summary>
		/// <param name="printArea">The print area to remove from the collection.</param>
		/// <remarks>
		/// <p class="body">
		/// If any page breaks are contained in the removed print area, they will be removed from their collection.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="printArea"/> is null.
		/// </exception>
		/// <returns>True if the print area was found and removed; False otherwise.</returns>
		/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
		/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
		public bool Remove(WorksheetRegion printArea)
		{
			if (printArea == null)
				throw new ArgumentNullException("printArea");

			int index = this.regions.IndexOf(printArea);

			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion  // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the print area at the specified index from the collection.
		/// </summary>
		/// <param name="index">The index of the print area to remove from the collection.</param>
		/// <remarks>
		/// <p class="body">
		/// If any page breaks are contained in the removed print area, they will be removed from their collection.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when <paramref name="index"/> is less than 0 or greater than or equal to the size of the collection.
		/// </exception>
		/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
		/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
		public void RemoveAt(int index)
		{
			if (index < 0 || this.Count <= index)
				throw new ArgumentOutOfRangeException("index");

			this.RemoveHelper(index);
			this.owner.UpdatePrintAreasNamedReference();
		}

		#endregion  // RemoveAt

		#endregion  // Public Methods

		#region Internal Methods

		#region AddHelper

		internal void AddHelper(WorksheetRegion printArea)
		{
			this.regions.Add(printArea);
		}

		#endregion  // AddHelper

		#region GetPrintArea

		internal WorksheetRegion GetPrintArea(int indexInPrintArea, int? min, int? max, bool isVertical)
		{
			if (min == null && max == null)
				return null;

			Worksheet worksheet = this.owner.AssociatedWorksheet;

			int minResolved = min.HasValue ? min.Value : 0;
			int maxResolved = max.HasValue
				? max.Value
				: (isVertical ? worksheet.Rows.MaxCount : worksheet.Columns.MaxCount) - 1;

			for (int i = 0; i < this.regions.Count; i++)
			{
				WorksheetRegion region = this.regions[i];

				if (isVertical)
				{
					if (region.FirstRow == min && region.LastRow == max)
					{
						if (region.FirstColumn <= indexInPrintArea && indexInPrintArea <= region.LastColumn)
							return region;
					}
				}
				else
				{
					if (region.FirstColumn == min && region.LastColumn == max)
					{
						if (region.FirstRow <= indexInPrintArea && indexInPrintArea <= region.LastRow)
							return region;
					}
				}
			}

			Debug.Assert(
				minResolved == 0 && maxResolved == (isVertical ? worksheet.Rows.MaxCount : worksheet.Columns.MaxCount) - 1,
				"Could not find the print area.");

			return null;
		}

		#endregion  // GetPrintArea

		#endregion  // Internal Methods

		#region Private Methods

		#region RemoveHelper

		private void RemoveHelper(int index)
		{
			WorksheetRegion printArea = this[index];

			for (int i = this.owner.HorizontalPageBreaks.Count - 1; i >= 0; i--)
			{
				if (this.owner.HorizontalPageBreaks[i].PrintArea == printArea)
					this.owner.HorizontalPageBreaks.RemoveAt(i);
			}

			for (int i = this.owner.VerticalPageBreaks.Count - 1; i >= 0; i--)
			{
				if (this.owner.VerticalPageBreaks[i].PrintArea == printArea)
					this.owner.VerticalPageBreaks.RemoveAt(i);
			}

			this.regions.RemoveAt(index);
		}

		#endregion  // RemoveHelper

		#endregion  // Private Methods
		
		#endregion  // Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of print areas in the collection.
		/// </summary>
		public int Count
		{
			get { return this.regions.Count; }
		}

		#endregion  // Count

		#region Indexer

		/// <summary>
		/// Gets the print area at the specified index.
		/// </summary>
		/// <param name="index">The index of the print area to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when <paramref name="index"/> is less than 0 or greater than or equal to the size of the collection.
		/// </exception>
		/// <returns>A <see cref="WorksheetRegion"/> instance representing a print area in the <see cref="Worksheet"/>.</returns>
		public WorksheetRegion this[int index]
		{
			get { return this.regions[index]; }
		}

		#endregion  // Indexer

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