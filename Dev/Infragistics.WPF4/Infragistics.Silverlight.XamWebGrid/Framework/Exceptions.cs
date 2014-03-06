using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Globalization;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	#region InvalidColumnKeyException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when a <see cref="ColumnBase"/> is defined with a key that doesn'type have a corresponding <see cref="Infragistics.DataField"/>.
	/// </summary>
	public class InvalidColumnKeyException : Exception
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnKeyException"/> class.
		/// </summary>
		public InvalidColumnKeyException()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="keys">A string of all the keys that are invalid.</param>
		public InvalidColumnKeyException(string keys)
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidColumnKeyException"), "\"" + keys + "\""))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public InvalidColumnKeyException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // InvalidColumnKeyException

	#region EmptyColumnKeyException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when a <see cref="ColumnBase"/> is defined with a key that doesn'type have a corresponding <see cref="Infragistics.DataField"/>.
	/// </summary>
	public class EmptyColumnKeyException : Exception
	{

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="EmptyColumnKeyException"/> class.
		/// </summary>
		public EmptyColumnKeyException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("EmptyColumnKeyException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EmptyColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public EmptyColumnKeyException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EmptyColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public EmptyColumnKeyException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // EmptyColumnKeyException

	#region DuplicateColumnKeyException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when mulitiple columns are defined with the same key.
	/// </summary>
	public class DuplicateColumnKeyException : Exception
	{

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="DuplicateColumnKeyException"/> class.
		/// </summary>
		public DuplicateColumnKeyException()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DuplicateColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="key">The key that has been duplicated.</param>
		public DuplicateColumnKeyException(string key)
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("DuplicateColumnKeyException"), key))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DuplicateColumnKeyException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public DuplicateColumnKeyException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // DuplicateColumnKeyException

	#region TypeResolutionException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when the <see cref="TypeTypeConverter"/> can not resolve a specified type.
	/// </summary>
	public class TypeResolutionException : Exception
	{

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class.
		/// </summary>
		public TypeResolutionException()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class.
		/// </summary>
		public TypeResolutionException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class.
		/// </summary>
		/// <param propertyName="type">The type that could not be resolved</param>
		public TypeResolutionException(Type type)
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("TypeResolutionException"), type.ToString()))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeResolutionException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public TypeResolutionException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // TypeResolutionException

	#region InvalidColumnTypeMappingException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when an invalid column type is added to the <see cref="XamGrid.ColumnTypeMappings"/> collection.
	/// </summary>
	public class InvalidColumnTypeMappingException : Exception
	{

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnTypeMappingException"/> class.
		/// </summary>
		public InvalidColumnTypeMappingException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidColumnTypeMappingException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnTypeMappingException"/> class.
		/// </summary>
		public InvalidColumnTypeMappingException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidColumnTypeMappingException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public InvalidColumnTypeMappingException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion // Constructor

	}
	#endregion // InvalidColumnTypeMappingException

	#region InvalidPageIndexException
	/// <summary>
	/// An <see cref="Exception"/> that is thrown when an invalid page index is used.
	/// </summary>
	public class InvalidPageIndexException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPageIndexException"/> class.
		/// </summary>
		public InvalidPageIndexException()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPageIndexException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public InvalidPageIndexException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidPageIndexException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public InvalidPageIndexException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion // InvalidPageIndexException

	#region InvalidActiveCellException
	/// <summary>
	/// An <see cref="Exception"/> raised when an invalid active cell is detected in the control.
	/// </summary>
	public class InvalidActiveCellException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidActiveCellException"/> class.
		/// </summary>
		public InvalidActiveCellException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidActiveCellException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidActiveCellException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public InvalidActiveCellException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidActiveCellException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public InvalidActiveCellException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region ResizingColumnCannotBeRemovedException
	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class ResizingColumnCannotBeRemovedException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ResizingColumnCannotBeRemovedException"/> class.
		/// </summary>
		public ResizingColumnCannotBeRemovedException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("ResizingColumnCannotBeRemovedException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResizingColumnCannotBeRemovedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public ResizingColumnCannotBeRemovedException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResizingColumnCannotBeRemovedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public ResizingColumnCannotBeRemovedException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region NullDataException
	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class NullDataException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="NullDataException"/> class.
		/// </summary>
		public NullDataException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("NullDataException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NullDataException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public NullDataException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NullDataException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public NullDataException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region DataTypeMismatchException
	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class DataTypeMismatchException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
		/// </summary>
		public DataTypeMismatchException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("DataTypeMismatchException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public DataTypeMismatchException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public DataTypeMismatchException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region ColumnLayoutException
	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class ColumnLayoutException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnLayoutException"/> class.
		/// </summary>
		public ColumnLayoutException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("ColumnLayoutException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnLayoutException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public ColumnLayoutException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnLayoutException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public ColumnLayoutException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region ChildColumnIsSelectedAccessDeniedException
	/// <summary>
	/// An <see cref="Exception"/> raised when a <see cref="Column"/> on a ColumnLayout, has its IsSelected property set in Xaml.
	/// </summary>
	public class ChildColumnIsSelectedAccessDeniedException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsSelectedAccessDeniedException"/> class.
		/// </summary>
		public ChildColumnIsSelectedAccessDeniedException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("ChildColumnIsSelectedAccessDeniedException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsSelectedAccessDeniedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public ChildColumnIsSelectedAccessDeniedException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsSelectedAccessDeniedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public ChildColumnIsSelectedAccessDeniedException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region ChildColumnIsGroupByAccessDeniedException
	/// <summary>
	/// An <see cref="Exception"/> raised when a <see cref="Column"/> on a ColumnLayout, has its IsGroupBy property set in Xaml.
	/// </summary>
	public class ChildColumnIsGroupByAccessDeniedException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsGroupByAccessDeniedException"/> class.
		/// </summary>
		public ChildColumnIsGroupByAccessDeniedException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("ChildColumnIsGroupByAccessDeniedException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsGroupByAccessDeniedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public ChildColumnIsGroupByAccessDeniedException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildColumnIsGroupByAccessDeniedException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public ChildColumnIsGroupByAccessDeniedException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region InvalidRowIndexException
	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class InvalidRowIndexException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRowIndexException"/> class.
		/// </summary>
		public InvalidRowIndexException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidRowIndexException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRowIndexException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public InvalidRowIndexException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRowIndexException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public InvalidRowIndexException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}
	#endregion

	#region NullConditionalFormatEvaluationValueException

	/// <summary>
	/// An <see cref="Exception"/> raised when the column being resized is removed from the resizing columns collection.
	/// </summary>
	public class NullConditionalFormatEvaluationValueException : Exception
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="NullConditionalFormatEvaluationValueException"/> class.
		/// </summary>
		public NullConditionalFormatEvaluationValueException()
			: base(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("NullConditionalFormatEvaluationValueException")))
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NullConditionalFormatEvaluationValueException"/> class.
		/// </summary>
		/// <param propertyName="message">The message to be displayed in the exception.</param>
		public NullConditionalFormatEvaluationValueException(string message)
			: base(string.Format(CultureInfo.CurrentCulture, message))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NullConditionalFormatEvaluationValueException"/> class.
		/// </summary>
		/// <param propertyName="message">The message that should be displayed.</param>
		/// <param propertyName="innerException">An inner exception.</param>
		public NullConditionalFormatEvaluationValueException(string message, Exception innerException)
			: base(message, innerException)
		{

		}

		#endregion
	}

	#endregion // NullConditionalFormatEvaluationValueException

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