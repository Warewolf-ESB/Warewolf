using System;
using System.Collections.Generic;
using System.Text;






using Infragistics.Controls.Schedules;

namespace Infragistics

{
	#region ErrorSeverity Enum

	/// <summary>
	/// Used to specifiy the <see cref="DataErrorInfo.Severity"/> property.
	/// </summary>
	public enum ErrorSeverity
	{
		/// <summary>
		/// Diagnostic errors are intended for developers. They are informational in nature.
		/// </summary>
		Diagnostic = 20,

		/// <summary>
		/// Warnigs are suitable for end use display.
		/// </summary>
		Warning = 40,

		/// <summary>
		/// Errors should be displayed to the user.
		/// </summary>
		Error = 50,

		/// <summary>
		/// Severe or blocking errors indicate a state where the control can not operate as expected.
		/// </summary>
		SevereError = 100
	} 

	#endregion // ErrorSeverity Enum

	#region DataErrorInfo Class

	/// <summary>
	/// Contains information about one or more errors.
	/// </summary>
	public class DataErrorInfo
	{
		#region Member Vars

		private Exception _exception;
		private IList<DataErrorInfo> _errorList;
		private string _userErrorText;
		private string _diagnosticText;
		private string _userErrorTextDefault;
		private string _diagnosticTextDefault;
		private object _context;
		private ErrorSeverity _severity = ErrorSeverity.Diagnostic;

		// JJD 4/4/11 - TFS69535
		private bool _isLocalTZTokenError;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="DataErrorInfo"/>.
		/// </summary>
		/// <param name="error">Error message.</param>
		public DataErrorInfo( string error )
		{
			_userErrorText = error;
			this.InitializeTexts( );
		}

		/// <summary>
		/// Initializes a new instance of <see cref="DataErrorInfo"/>.
		/// </summary>
		/// <param name="exception">The source exception.</param>
		public DataErrorInfo( Exception exception )
		{
			_exception = exception;
			this.InitializeTexts( );
		}

		/// <summary>
		/// Initializes a new instance of <see cref="DataErrorInfo"/>.
		/// </summary>
		/// <param name="errorList">List of errors.</param>
		public DataErrorInfo( IList<DataErrorInfo> errorList )
		{
			_errorList = errorList;

			// initialize the severity to the highest in the list 
			foreach ( DataErrorInfo error in errorList )
			{
				if ( error._severity > this._severity )
					this._severity = error._severity;
			}

			this.InitializeTexts( );
		}

		#endregion // Constructor

		#region Base class overrides

		#region ToString
		/// <summary>
		/// Returns a string representation of the error(s).
		/// </summary>
		/// <returns>A concatenated list of the error messages.</returns>
		public override string ToString( )
		{
			StringBuilder sb = new StringBuilder( );
			this.ToString( sb );
			return sb.ToString( );
		}
		#endregion // ToString

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region Context

		/// <summary>
		/// Gets the context object if any associated with the error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the error is specific to an activity, the context will the activity object itself.
		/// </para>
		/// </remarks>
		public object Context
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		#endregion // Context

		#region DiagnosticText

		/// <summary>
		/// Text that can be used to diagnose the error.
		/// </summary>
		public string DiagnosticText
		{
			get
			{
				if ( string.IsNullOrEmpty( _diagnosticText ) )
					return _diagnosticTextDefault;

				return _diagnosticText;
			}
			set
			{
				_diagnosticText = value;
			}
		}

		#endregion // DiagnosticText

		#region Exception

		/// <summary>
		/// Gets the Exception object if any associated with the error.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return _exception;
			}
			set
			{
				_exception = value;
			}
		}

		#endregion // Exception

		#region ErrorList

		/// <summary>
		/// If multiple errors occurred during an operation then this list will return the list of errors.
		/// </summary>
		public IList<DataErrorInfo> ErrorList
		{
			get
			{
				return _errorList;
			}
		}

		#endregion // ErrorList

		#region Severity

		/// <summary>
		/// Specifies the severity of the error.
		/// </summary>
		public ErrorSeverity Severity
		{
			get
			{
				return _severity;
			}
			set
			{
				_severity = value;
			}
		}

		#endregion // Severity

		#region UserErrorText

		/// <summary>
		/// Text to be displayed to the end user.
		/// </summary>
		public string UserErrorText
		{
			get
			{
				if ( string.IsNullOrEmpty( _userErrorText ) )
					return _userErrorTextDefault;

				return _userErrorText;
			}
			set
			{
				_userErrorText = value;
			}
		}

		#endregion // UserErrorText

		#endregion // Public Properties

		#region Internal Properties

		// JJD 4/4/11 - TFS69535 - added
		#region IsLocalTZTokenError

		internal bool IsLocalTZTokenError { get { return _isLocalTZTokenError; } }

		#endregion //IsLocalTZTokenError

		#endregion //Internal Properties	
        
		#endregion // Properties

		#region Methods

		#region CreateBlocking

		internal static DataErrorInfo CreateBlocking( object context, string formattedText, params object[] args )
		{
			string str = string.Format( formattedText, args );
			return new DataErrorInfo( str )
			{
				_context = context,
				_severity = ErrorSeverity.SevereError
			};
		}

		// JJD 4/4/11 - TFS69535 
		// Added overload with isLocalTZTokenError parameter
		internal static DataErrorInfo CreateBlocking( object context, string formattedText, bool isLocalTZTokenError, params object[] args )
		{
			string str = string.Format( formattedText, args );
			DataErrorInfo errorInfo = new DataErrorInfo( str )
			{
				_context = context,
				_severity = ErrorSeverity.SevereError
			};

			errorInfo._isLocalTZTokenError = isLocalTZTokenError;

			return errorInfo;
		}

		#endregion //CreateBlocking

		#region CreateError

		internal static DataErrorInfo CreateError( object context, string formattedText, params object[] args )
		{
			string str = string.Format( formattedText, args );
			return new DataErrorInfo( str )
			{
				_context = context,
				_severity = ErrorSeverity.Error
			};
		}

		#endregion //CreateError

		#region CreateWarning

		internal static DataErrorInfo CreateWarning( object context, string formattedText, params object[] args )
		{
			string str = string.Format( formattedText, args );
			return new DataErrorInfo( str )
			{
				_context = context,
				_severity = ErrorSeverity.Warning
			};
		}

		#endregion //CreateWarning

		#region CreateDiagnostic

		internal static DataErrorInfo CreateDiagnostic( object context, string formattedText, params object[] args )
		{
			string str = string.Format( formattedText, args );
			return new DataErrorInfo( str )
			{
				_context = context,
				_severity = ErrorSeverity.Diagnostic
			};
		}

		#endregion // CreateDiagnostic

		#region CreateFromList

		/// <summary>
		/// If the errorList is empty, returns null. If it has one item, returns that item. Otherwise it creates
		/// a new data error info that wraps the error list.
		/// </summary>
		/// <param name="errorList"></param>
		/// <returns></returns>
		internal static DataErrorInfo CreateFromList( IList<DataErrorInfo> errorList )
		{
			if ( null != errorList && errorList.Count > 0 )
			{
				if ( 1 == errorList.Count )
					return errorList[0];

				ErrorSeverity max = ErrorSeverity.Diagnostic;

				for ( int i = 0, count = errorList.Count; i < count; i++ )
				{
					DataErrorInfo ii = errorList[i];

					var iiSeverity = ii._severity;
					if ( iiSeverity > max )
						max = iiSeverity;
				}

				return new DataErrorInfo( errorList ) { _severity = max };
			}

			return null;
		}

		#endregion //CreateFromList

		#region InitializeTexts

		private void InitializeTexts( )
		{
			if ( string.IsNullOrEmpty( _userErrorText ) )
			{
				if ( null != _exception )
					_userErrorTextDefault = _exception.Message;
			}

			if ( string.IsNullOrEmpty( _diagnosticText ) )
			{
				StringBuilder sb = new StringBuilder( );
				this.ToString(sb, skipUserAndDiagnosticsTexts: true);

				_diagnosticTextDefault = sb.ToString( );
			}
		}

		#endregion // InitializeTexts

		#region ToString

		private void ToString( StringBuilder sb, string prefix = null, int indent = 0, bool skipUserAndDiagnosticsTexts = false )
		{
			int detailIndent = 0;
			bool indentDetailHeader = false;

			sb.Append(' ', indent);

			if ( !string.IsNullOrEmpty( prefix ) )
			{
				detailIndent += prefix.Length + 2;
				sb.Append(prefix).Append(") ");
			}

			if ( !skipUserAndDiagnosticsTexts && !string.IsNullOrEmpty(_userErrorText) )
			{
				sb.Append(_userErrorText).Append(Environment.NewLine);
				indentDetailHeader = true;
			}

			int sbCount = sb.Length;

			if ( !skipUserAndDiagnosticsTexts && !string.IsNullOrEmpty(_diagnosticText) )
			{
				sb.Append(' ', detailIndent).Append(_diagnosticText).Append(Environment.NewLine);
			}

			if ( _exception != null )
				ToString(sb, detailIndent, _exception);

			if ( _errorList != null )
			{
				if ( !string.IsNullOrEmpty( prefix ) )
					prefix = prefix + ".";

				for ( int i = 0, count = _errorList.Count; i < count; i++ )
				{
					DataErrorInfo error = _errorList[i];

					if ( null != error )
					{
						_errorList[i].ToString( sb, string.Format( "{0}{1}", prefix, i + 1 ), detailIndent );
					}
				}
			}

			if ( sb.Length > sbCount )
			{
				sb.Insert( sbCount, ScheduleUtilities.GetString( "LE_ErrorDetailsHeader" ) + Environment.NewLine );// "Error Details:" 

				if (indentDetailHeader)
					sb.Insert(sbCount, " ", detailIndent);
			}
		}

		private static void ToString( StringBuilder sb, int indent, Exception exception )
		{
			string prefix = ScheduleUtilities.GetString( "LE_InnerExceptionHeader" );
			bool isInitial = true;

			while ( exception != null )
			{
				sb.Append(' ', indent);

				if ( isInitial )
					isInitial = false;
				else
					sb.Append(prefix);

				sb.Append( exception.Message );
				sb.Append( Environment.NewLine );

				indent += 3;
				exception = exception.InnerException;
			}
		}

		#endregion // ToString

		#endregion // Methods
	} 

	#endregion // DataErrorInfo Class
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