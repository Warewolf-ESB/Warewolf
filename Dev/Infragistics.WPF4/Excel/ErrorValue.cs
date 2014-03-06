using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;

namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents an error value in Microsoft Excel.
	/// </summary>



	public

		 class ErrorValue
	{
		#region Constants

		// MD 7/14/08 - Excel formula solving
		// These are used in other places. They have been made internal and the names have been capitalized.
		// All existing references have been updated.
		//private const byte emptyCellRangeIntersectionValue = 0x00;
		//private const byte divisionByZeroValue = 0x07;
		//private const byte wrongOperandTypeValue = 0x0F;
		//private const byte invalidCellReferenceValue = 0x17;
		//private const byte wrongFunctionNameValue = 0x1D;
		//private const byte valueRangeOverflowValue = 0x24;
		//private const byte argumentOrFunctionNotAvailableValue = 0x2A;
		internal const byte EmptyCellRangeIntersectionValue = 0x00;
		internal const byte DivisionByZeroValue = 0x07;
		internal const byte WrongOperandTypeValue = 0x0F;
		internal const byte InvalidCellReferenceValue = 0x17;
		internal const byte WrongFunctionNameValue = 0x1D;
		internal const byte ValueRangeOverflowValue = 0x24;
		internal const byte ArgumentOrFunctionNotAvailableValue = 0x2A;

		// MD 8/20/08 - Excel formula solving
		internal const byte CircularityValue = 0xFF;

		#endregion Constants

		#region Static Variables

		private static ErrorValue emptyCellRangeIntersection;
		private static ErrorValue divisionByZero;
		private static ErrorValue wrongOperandType;
		private static ErrorValue invalidCellReference;
		private static ErrorValue wrongFunctionName;
		private static ErrorValue valueRangeOverflow;
		private static ErrorValue argumentOrFunctionNotAvailable;

		// MD 8/20/08 - Excel formula solving
		private static ErrorValue circularity;

		// MD 4/18/08 - BR32154
		// Created a synchronization object for lazily creating the error values while being thread-safe 
		private readonly static object lazyCreateSyncLock = new object();

		#endregion Static Variables

		#region Member Variables

		private byte value;

		#endregion Member Variables

		#region Constructor

		private ErrorValue( byte value )
		{
			this.value = value;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Gets the string representation of the error value.
		/// </summary>
		/// <returns>The string representation of the error value.</returns>
		public override string ToString()
		{
			switch ( this.value )
			{
				case ErrorValue.EmptyCellRangeIntersectionValue:		return "#NULL!";
				case ErrorValue.DivisionByZeroValue:					return "#DIV/0!";
				case ErrorValue.WrongOperandTypeValue:					return "#VALUE!";
				case ErrorValue.InvalidCellReferenceValue:				return FormulaParser.ReferenceErrorValue;
				case ErrorValue.WrongFunctionNameValue:					return "#NAME?";
				case ErrorValue.ValueRangeOverflowValue:				return "#NUM!";
				case ErrorValue.ArgumentOrFunctionNotAvailableValue:	return "#N/A";

				// MD 8/20/08 - Excel formula solving
				case ErrorValue.CircularityValue:						return "#CIRCULARITY!";

				default:
					Utilities.DebugFail("Unknown error value: " + this.value);
					break;
			}

			return this.value.ToString( CultureInfo.CurrentCulture );
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Methods

		#region FromValue

		internal static ErrorValue FromValue( byte value )
		{
			switch ( value )
			{
				case ErrorValue.EmptyCellRangeIntersectionValue:		return ErrorValue.EmptyCellRangeIntersection;
				case ErrorValue.DivisionByZeroValue:					return ErrorValue.DivisionByZero;
				case ErrorValue.WrongOperandTypeValue:					return ErrorValue.WrongOperandType;
				case ErrorValue.InvalidCellReferenceValue:				return ErrorValue.InvalidCellReference;
				case ErrorValue.WrongFunctionNameValue:					return ErrorValue.WrongFunctionName;
				case ErrorValue.ValueRangeOverflowValue:				return ErrorValue.ValueRangeOverflow;
				case ErrorValue.ArgumentOrFunctionNotAvailableValue:	return ErrorValue.ArgumentOrFunctionNotAvailable;

				// MD 8/20/08 - Excel formula solving
				case ErrorValue.CircularityValue:						return ErrorValue.Circularity;

				default:
					Utilities.DebugFail( "Unknown error code" );
					return null;
			}
		}

		#endregion FromValue

		// MD 8/29/08 - Excel formula solving
		#region ToCalcErrorValue

		internal ExcelCalcErrorValue ToCalcErrorValue()
		{
			return new ExcelCalcErrorValue( CalcUtilities.GetCalcErrorCode( this.value ) );
		}  

		#endregion ToCalcErrorValue

		#endregion Methods

		#region Properties

		#region Static Properties

		#region ArgumentOrFunctionNotAvailable

		/// <summary>
		/// Gets the ErrorValue representing the #N/A error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when a value isn't available for some part of a formula.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #N/A error.</value>
		public static ErrorValue ArgumentOrFunctionNotAvailable
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.argumentOrFunctionNotAvailable == null )
				//    ErrorValue.argumentOrFunctionNotAvailable = new ErrorValue( ErrorValue.argumentOrFunctionNotAvailableValue );
				if ( ErrorValue.argumentOrFunctionNotAvailable == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.argumentOrFunctionNotAvailable == null )
							ErrorValue.argumentOrFunctionNotAvailable = new ErrorValue( ErrorValue.ArgumentOrFunctionNotAvailableValue );
					}
				}

				return ErrorValue.argumentOrFunctionNotAvailable;
			}
		}

		#endregion ArgumentOrFunctionNotAvailable

		// MD 8/20/08 - Excel formula solving
		#region Circularity

		/// <summary>
		/// Gets the ErrorValue representing a circularity error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// There is no error constant for a circularity in Microsoft Excel and a circularity cannot be the result of a formula in Microsoft Excel.
		/// However, for run-time purposes, after loading or before saving a workbook, this error value will be used for the result of formulas
		/// which cause circular references when the owning workbook has <see cref="Workbook.IterativeCalculationsEnabled"/> set to False.
		/// </p>
		/// <p class="body">
		/// In Microsoft Excel, setting a circular reference formula on a cell will show an error dialog the first time the problem occurs. Subsequent
		/// formulas violating the circular reference restriction will just evaluate to zero. Therefore, when this value is encountered in a cell, it 
		/// can be treated as a zero for calculation purposes. This error value will be returned though so an actual zero value in a cell can be 
		/// differentiated from a circularity error.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> Because there is no circularity error constant in Microsoft Excel, this error value cannot be assigned to a cell manually.
		/// Attempting to assign this error value to a cell will result in an InvalidOperationException to be thrown. This error value will only be
		/// valid as the result of a formula which has been applied to a cell.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing a circularity error.</value>
		public static ErrorValue Circularity
		{
			get
			{
				if ( ErrorValue.circularity == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.circularity == null )
							ErrorValue.circularity = new ErrorValue( ErrorValue.CircularityValue );
					}
				}

				return ErrorValue.circularity;
			}
		}

		#endregion Circularity

		#region DivisionByZero

		/// <summary>
		/// Gets the ErrorValue representing the #DIV/0! error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when a number is divided by zero.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #DIV/0! error.</value>
		public static ErrorValue DivisionByZero
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.divisionByZero == null )
				//    ErrorValue.divisionByZero = new ErrorValue( ErrorValue.divisionByZeroValue );
				if ( ErrorValue.divisionByZero == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.divisionByZero == null )
							ErrorValue.divisionByZero = new ErrorValue( ErrorValue.DivisionByZeroValue );
					}
				}

				return ErrorValue.divisionByZero;
			}
		}

		#endregion DivisionByZero

		#region EmptyCellRangeIntersection

		/// <summary>
		/// Gets the ErrorValue representing the #NULL! error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when there is an intersection of two references that do not contain any common cells.
		/// The intersection operator is a space between two references.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #NULL! error.</value>
		public static ErrorValue EmptyCellRangeIntersection
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.emptyCellRangeIntersection == null )
				//    ErrorValue.emptyCellRangeIntersection = new ErrorValue( ErrorValue.emptyCellRangeIntersectionValue );
				if ( ErrorValue.emptyCellRangeIntersection == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.emptyCellRangeIntersection == null )
							ErrorValue.emptyCellRangeIntersection = new ErrorValue( ErrorValue.EmptyCellRangeIntersectionValue );
					}
				}

				return ErrorValue.emptyCellRangeIntersection;
			}
		}

		#endregion EmptyCellRangeIntersection

		#region InvalidCellReference

		/// <summary>
		/// Gets the ErrorValue representing the #REF! error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when a cell reference or cell range reference is not valid.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #REF! error.</value>
		public static ErrorValue InvalidCellReference
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.invalidCellReference == null )
				//    ErrorValue.invalidCellReference = new ErrorValue( ErrorValue.invalidCellReferenceValue );
				if ( ErrorValue.invalidCellReference == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.invalidCellReference == null )
							ErrorValue.invalidCellReference = new ErrorValue( ErrorValue.InvalidCellReferenceValue );
					}
				}

				return ErrorValue.invalidCellReference;
			}
		}

		#endregion InvalidCellReference

		#region ValueRangeOverflow

		/// <summary>
		/// Gets the ErrorValue representing the #NUM! error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when there are invalid numeric values in a formula.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #NUM! error.</value>
		public static ErrorValue ValueRangeOverflow
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.valueRangeOverflow == null )
				//    ErrorValue.valueRangeOverflow = new ErrorValue( ErrorValue.valueRangeOverflowValue );
				if ( ErrorValue.valueRangeOverflow == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.valueRangeOverflow == null )
							ErrorValue.valueRangeOverflow = new ErrorValue( ErrorValue.ValueRangeOverflowValue );
					}
				}

				return ErrorValue.valueRangeOverflow;
			}
		}

		#endregion ValueRangeOverflow

		#region WrongFunctionName

		/// <summary>
		/// Gets the ErrorValue representing the #NAME? error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when text in a formula is not recognized.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #NAME? error.</value>
		public static ErrorValue WrongFunctionName
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.wrongFunctionName == null )
				//    ErrorValue.wrongFunctionName = new ErrorValue( ErrorValue.wrongFunctionNameValue );
				if ( ErrorValue.wrongFunctionName == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.wrongFunctionName == null )
							ErrorValue.wrongFunctionName = new ErrorValue( ErrorValue.WrongFunctionNameValue );
					}
				}

				return ErrorValue.wrongFunctionName;
			}
		}

		#endregion WrongFunctionName

		#region WrongOperandType

		/// <summary>
		/// Gets the ErrorValue representing the #VALUE! error.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This error occurs when an incorrect argument or operand is used in a function.
		/// </p>
		/// </remarks>
		/// <value>The ErrorValue representing the #VALUE! error.</value>
		public static ErrorValue WrongOperandType
		{
			get
			{
				// MD 4/18/08 - BR32154
				// The error values must be the same on all threads, so we cannot make the variables thread static, but they
				// do need to be thread safe, so we must lock on the sync object before lazily creating the value, so we don't
				// lazily create the same error value twice on different threads.
				//if ( ErrorValue.wrongOperandType == null )
				//    ErrorValue.wrongOperandType = new ErrorValue( ErrorValue.wrongOperandTypeValue );
				if ( ErrorValue.wrongOperandType == null )
				{
					lock ( ErrorValue.lazyCreateSyncLock )
					{
						if ( ErrorValue.wrongOperandType == null )
							ErrorValue.wrongOperandType = new ErrorValue( ErrorValue.WrongOperandTypeValue );
					}
				}

				return ErrorValue.wrongOperandType;
			}
		}

		#endregion WrongOperandType

		#endregion Static Properties

		#region Internal Properties

		#region Value

		internal byte Value
		{
			get { return this.value; }
		}

		#endregion Value

		#endregion Internal Properties

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