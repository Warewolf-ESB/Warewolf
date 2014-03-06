using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// Abstract base class for the sort conditions which describe how to sort data in a region.
	/// </summary>
	/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
	/// <seealso cref="WorksheetTableColumn.SortCondition"/>
	/// <seealso cref="OrderedSortCondition"/>
	/// <seealso cref="CustomListSortCondition"/>
	/// <seealso cref="FontColorSortCondition"/>
	/// <seealso cref="FillSortCondition"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 abstract class SortCondition
	{
		#region Member Variables

		private readonly SortDirection _sortDirection;

		#endregion // Member Variables

		#region Constructor

		internal SortCondition()
			: this(SortDirection.Ascending) { }

		internal SortCondition(SortDirection sortDirection)
		{
			Utilities.VerifyEnumValue<SortDirection>(sortDirection);
			_sortDirection = sortDirection;
		}

		#endregion // Constructor

		#region Methods

		#region CompareCells

		internal abstract int CompareCells<T>(SortSettings<T> owner, Worksheet worksheet, int rowIndexX, int rowIndexY, short columnIndex) where T : ISortable;

		#endregion // CompareCells

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="SortCondition"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			SortCondition other = obj as SortCondition;
			if (other == null)
				return false;

			return _sortDirection == other._sortDirection;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="SortCondition"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return _sortDirection.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region SortDirection

		/// <summary>
		/// Gets the value which indicates the sort direction represented by the sort condition.
		/// </summary>
		/// <value>Either SortDirection.Ascending or SortDirection.Descending.</value>
		public SortDirection SortDirection
		{
			get { return _sortDirection; }
		}

		#endregion // SortDirection

		#endregion // Public Properties

		#region Internal Properties

		internal abstract ST_SortBy SortByValue { get; }

		#endregion // Internal Properties

		#endregion // Properties
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