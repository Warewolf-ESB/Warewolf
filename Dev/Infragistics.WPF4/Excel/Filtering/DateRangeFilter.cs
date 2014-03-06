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


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 2/25/12 - 12.1 - Table Support



	/// <summary>
	/// Abstract base class for filters which filter dates based on whether they are within a specified range of dates or not.
	/// </summary>
	/// <seealso cref="RelativeDateRangeFilter"/>
	/// <seealso cref="YearToDateFilter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 abstract class DateRangeFilter : DynamicValuesFilter
	{
		#region Member Variables

		private DateTime _end;
		private bool _isInitialized;
		private DateTime _start;

		#endregion // Member Variables

		#region Constructor

		internal DateRangeFilter(WorksheetTableColumn owner)
			: base(owner) { }

		internal DateRangeFilter(WorksheetTableColumn owner, DateTime start, DateTime end)
			: base(owner) 
		{
			_isInitialized = true;
			_start = start;
			_end = end;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			DateRangeFilter other = filter as DateRangeFilter;
			if (other == null)
				return false;

			return 
				_end == other._end && 
				_start == other._start;
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal sealed override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			DateTime value;
			if (worksheet.TryGetDateTimeFromCell(row, columnIndex, out value))
			{
				if (_start <= value && value < _end)
					return true;
			}

			return false;
		}

		#endregion // MeetsCriteria

		#region OnBeforeFilterColumn

		internal sealed override bool OnBeforeFilterColumn(Worksheet worksheet, int firstRowIndex, int lastRowIndex, short columnIndex)
		{
			this.ReinitializeRange();
			return true;
		}

		#endregion // OnBeforeFilterColumn

		#endregion // Base Class Overrides

		#region Methods

		#region ReinitializeRange

		internal void ReinitializeRange()
		{
			_isInitialized = true;
			this.ReinitializeRangeHelper(out _start, out _end);
		}

		internal abstract void ReinitializeRangeHelper(out DateTime start, out DateTime end);

		#endregion // ReinitializeRange

		#endregion // Methods

		#region Properties

		#region End

		/// <summary>
		/// Gets the exclusive end date of the filtered in date range.
		/// </summary>
		/// <seealso cref="Start"/>
		public DateTime End
		{
			get 
			{
				if (_isInitialized == false)
					this.ReinitializeRange();

				return _end; 
			}
		}

		#endregion // End

		#region Start

		/// <summary>
		/// Gets the inclusive start date of the filtered in date range.
		/// </summary>
		/// <seealso cref="End"/>
		public DateTime Start
		{
			get
			{
				if (_isInitialized == false)
					this.ReinitializeRange();

				return _start; 
			}
		}

		#endregion // Start

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