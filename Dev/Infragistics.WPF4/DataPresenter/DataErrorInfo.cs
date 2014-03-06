using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DataPresenter
{
    // AS 5/5/09 NA 2009.2 ClipboardSupport
	// Added wrapper class for DataError values so we can get the information 
	// passed back in the clipboard handling.
	//
	internal class DataErrorInfo
	{
		#region Member Variables

		private DataRecord _record;
		private Field _field;
		private Exception _exception;
		private DataErrorOperation _operation;
		private string _message; 

		#endregion //Member Variables

		#region Constructor
		internal DataErrorInfo(DataRecord record, Field field, Exception exception,
			DataErrorOperation operation, string message)
		{
			_record = record;
			_field = field;
			_exception = exception;
			_operation = operation;
			_message = message;

			if (string.IsNullOrEmpty(_message) && null != _exception)
				_message = _exception.Message;
		}
		#endregion //Constructor

		#region Properties
		internal DataRecord Record
		{
			get { return _record; }
		}

		internal Field Field
		{
			get { return _field; }
		}

		internal Cell Cell
		{
			get
			{
				if (_field != null && _record != null)
					return _record.Cells[_field];

				return null;
			}
		}

		internal Exception Exception
		{
			get { return _exception; }
		}

		internal DataErrorOperation Operation
		{
			get { return _operation; }
		}

		internal string Message
		{
			get { return _message; }
			set { _message = value; }
		} 
		#endregion //Properties
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