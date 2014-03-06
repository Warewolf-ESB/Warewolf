using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter data based on one or two custom conditions.
	/// </summary>
	/// <remarks>
	/// This filter type allows you to specify one or two filter conditions which have a comparison operator and value. 
	/// These two filter conditions can be combined with a logical and or a logical or operation.
	/// </remarks>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyCustomFilter(CustomFilterCondition)"/>
	/// <seealso cref="WorksheetTableColumn.ApplyCustomFilter(CustomFilterCondition,CustomFilterCondition,ConditionalOperator)"/>
	/// <seealso cref="CustomFilterCondition"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		class CustomFilter : Filter
	{
		#region Member Variables

		private CustomFilterCondition _condition1;
		private CustomFilterCondition _condition2;
		private ConditionalOperator _conditionalOperator;

		#endregion // Member Variables

		#region Constructor

		internal CustomFilter(WorksheetTableColumn owner, CustomFilterCondition condition)
			: this(owner, condition, null, ConditionalOperator.And) { }

		internal CustomFilter(WorksheetTableColumn owner, CustomFilterCondition condition1, CustomFilterCondition condition2, ConditionalOperator conditionalOperator)
			: base(owner)
		{
			if (condition1 == null)
				throw new ArgumentNullException("condition1");

			Utilities.VerifyEnumValue(conditionalOperator);

			_condition1 = condition1;
			_condition2 = condition2;
			_conditionalOperator = conditionalOperator;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			CustomFilter other = filter as CustomFilter;
			if (other == null)
				return false;

			return
				_conditionalOperator == other._conditionalOperator &&
				Object.Equals(_condition1, other._condition1) &&
				Object.Equals(_condition2, other._condition2);
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			object value = WorksheetRow.GetCellValue(row, columnIndex);
			bool meetsCondition1 = this.Condition1.MeetsCriteria(worksheet, row, columnIndex, value);

			if (this.Condition2 == null)
				return meetsCondition1;

			if (this.ConditionalOperator == ConditionalOperator.And)
			{
				if (meetsCondition1 == false)
					return false;
			}
			else
			{
				if (meetsCondition1)
					return true;
			}

			return this.Condition2.MeetsCriteria(worksheet, row, columnIndex, value);
		}

		#endregion // MeetsCriteria

		#region ShouldSaveIn2003Formats

		internal override bool ShouldSaveIn2003Formats(out bool needsAUTOFILTER12Record, out IList<string> allowedTextValues)
		{
			needsAUTOFILTER12Record = false;
			allowedTextValues = new List<string>();

			string resolvedString = _condition1.GetResolvedSearchString();
			if (255 < resolvedString.Length)
				return false;

			allowedTextValues.Add(resolvedString);

			if (_condition2 != null)
			{
				resolvedString = _condition2.GetResolvedSearchString();
				if (255 < resolvedString.Length)
					return false;

				allowedTextValues.Add(resolvedString);
			}

			return true;
		}

		#endregion // ShouldSaveIn2003Formats

		#endregion // Base Class Overrides

		#region Properties

		#region Condition1

		/// <summary>
		/// Gets or sets the first condition by which to filter the cells in the data range.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The first filter condition is required. Setting Condition1 to null will cause an exception to be thrown.
		/// </p>
		/// <p class="body">
		/// When both Condition1 and <see cref="Condition2"/> are set, the <see cref="ConditionalOperator"/> is used to determine
		/// how the conditions should be logically combined.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <seealso cref="Condition2"/>
		/// <seealso cref="ConditionalOperator"/>
		public CustomFilterCondition Condition1
		{
			get { return _condition1; }
			set
			{
				if (this.Condition1 == value)
					return;

				if (value == null)
					throw new ArgumentNullException("value");

				_condition1 = value;
				this.OnModified();
			}
		}

		#endregion // Condition1

		#region Condition2

		/// <summary>
		/// Gets or sets the second condition by which to filter the cells in the data range.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The second filter condition is optional. A value of null indicates that only <see cref="Condition1"/> should be used to 
		/// filter the data.
		/// </p>
		/// <p class="body">
		/// When both Condition1 and Condition2 are set, the <see cref="ConditionalOperator"/> is used to determine how the conditions 
		/// should be logically combined.
		/// </p>
		/// </remarks>
		/// <seealso cref="Condition1"/>
		/// <seealso cref="ConditionalOperator"/>
		public CustomFilterCondition Condition2
		{
			get { return _condition2; }
			set
			{
				if (this.Condition2 == value)
					return;

				_condition2 = value;
				this.OnModified();
			}
		}

		#endregion // Condition2

		#region ConditionalOperator

		/// <summary>
		/// Gets or sets the operator which defines how to logically combine <see cref="Condition1"/> and <see cref="Condition2"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When both Condition1 and Condition2 are set, the ConditionalOperator is used to determine how the conditions should be logically 
		/// combined. If only Condition1 is set and Condition2 is null, then ConditionalOperator is ignored.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ConditionalOperator"/> enumeration.
		/// </exception>
		/// <value>
		/// And to require both conditions to pass for the data to be filtered in. Or to allow data to be filtered in when one or both 
		/// conditions are met.
		/// </value>
		/// <seealso cref="Condition1"/>
		/// <seealso cref="Condition2"/>
		public ConditionalOperator ConditionalOperator
		{
			get { return _conditionalOperator; }
			set
			{
				if (this.ConditionalOperator == value)
					return;

				Utilities.VerifyEnumValue(value);

				_conditionalOperator = value;
				this.OnModified();
			}
		}

		#endregion // ConditionalOperator

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