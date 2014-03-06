using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Windows.Input;
//using Infragistics.Windows.Input;
using Infragistics.Windows.Editors;
using System.Collections;

namespace Infragistics.Windows.Editors.Events
{
	#region EditModeStartingEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="ValueEditor.EditModeStarting"/>
	/// </summary>
	/// <seealso cref="ValueEditor.EditModeStarting"/>
	/// <seealso cref="ValueEditor.EditModeStarted"/>
	/// <seealso cref="ValueEditor.EditModeEnding"/>
	/// <seealso cref="ValueEditor.EditModeEnded"/>
	/// <seealso cref="ValueEditor.EditModeStartingEvent"/>
	public class EditModeStartingEventArgs : RoutedEventArgs
	{
		private bool _cancel;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeStartingEventArgs"/> class
		/// </summary>
		public EditModeStartingEventArgs()
		{
		}

		/// <summary>
		/// If set to true will cancel the operation
		/// </summary>
		public bool Cancel
		{
			get { return this._cancel; }
			set { this._cancel = value; }
		}

	}

	#endregion //EditModeStartingEventArgs class

	#region EditModeStartedEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="ValueEditor.EditModeStarted"/>
	/// </summary>
	/// <seealso cref="ValueEditor.EditModeStarting"/>
	/// <seealso cref="ValueEditor.EditModeStarted"/>
	/// <seealso cref="ValueEditor.EditModeEnding"/>
	/// <seealso cref="ValueEditor.EditModeEnded"/>
	/// <seealso cref="ValueEditor.EditModeStartedEvent"/>
	public class EditModeStartedEventArgs : RoutedEventArgs
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeStartedEventArgs"/> class
		/// </summary>
		public EditModeStartedEventArgs()
		{
		}
	}

	#endregion //EditModeStartedEventArgs class

	#region EditModeEndingEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="ValueEditor.EditModeEnding"/>
	/// </summary>
	/// <seealso cref="ValueEditor.EditModeValidationError"/>
	/// <seealso cref="ValueEditor.EditModeStarting"/>
	/// <seealso cref="ValueEditor.EditModeStarted"/>
	/// <seealso cref="ValueEditor.EditModeEnding"/>
	/// <seealso cref="ValueEditor.EditModeEnded"/>
	/// <seealso cref="ValueEditor.EditModeEndingEvent"/>
	public class EditModeEndingEventArgs : RoutedEventArgs
	{
		private bool _cancel;
		private bool _acceptChanges;
		private bool _force;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeEndingEventArgs"/> class
		/// </summary>
		public EditModeEndingEventArgs(bool acceptChanges, bool force)
		{
			this._force				= force;
			this._acceptChanges		= acceptChanges;
		}

		/// <summary>
		/// Gets/sets whether the changes will be accepted.
		/// </summary>
		public bool AcceptChanges
		{
			get { return this._acceptChanges; }
			set {this._acceptChanges = value; }
		}

		/// <summary>
		/// If set to true will cancel the operation
		/// </summary>
		public bool Cancel
		{
			get { return this._cancel; }
			set 
			{
				if (value == true &&
					 this._force == true)
#pragma warning disable 436
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_1")); 
#pragma warning restore 436

				this._cancel = value; 
			}
		}


		/// <summary>
		/// Indicates a forced exit of edit mode (read-only)
		/// </summary>
		public bool Force
		{
			get { return this._force; }
		}
	}

	#endregion //EditModeEndingEventArgs class

	#region EditModeEndedEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="ValueEditor.EditModeEnded"/>
	/// </summary>
	/// <seealso cref="ValueEditor.EditModeStarting"/>
	/// <seealso cref="ValueEditor.EditModeStarted"/>
	/// <seealso cref="ValueEditor.EditModeEnding"/>
	/// <seealso cref="ValueEditor.EditModeEnded"/>
	/// <seealso cref="ValueEditor.EditModeEndedEvent"/>
	public class EditModeEndedEventArgs : RoutedEventArgs
	{
		private bool _changesAccepted;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeEndedEventArgs"/> class
		/// </summary>
		public EditModeEndedEventArgs(bool changesAccepted)
		{
			this._changesAccepted = changesAccepted;
		}

		/// <summary>
		/// Returns whether the changes were accepted (read-only).
		/// </summary>
		public bool ChangesAccepted
		{
			get { return this._changesAccepted; }
		}
	}

	#endregion //EditModeEndedEventArgs class

	#region EditModeValidationErrorEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="ValueEditor.EditModeValidationError"/>
	/// </summary>
	/// <seealso cref="ValueEditor.EditModeStarting"/>
	/// <seealso cref="ValueEditor.EditModeStarted"/>
	/// <seealso cref="ValueEditor.EditModeEnding"/>
	/// <seealso cref="ValueEditor.EditModeEnded"/>
	public class EditModeValidationErrorEventArgs : RoutedEventArgs
	{
		private ValueEditor _editor;
		private bool _forceExitEditMode;
		private InvalidValueBehavior _invalidValueBehavior;
		private string _errorMessage;
		private Exception _exception;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeValidationErrorEventArgs"/> class
		/// </summary>
		/// <param name="editor">The editor for which the event is being raised.</param>
		/// <param name="forceExitEditMode">A boolean indicating whether edit mode is required to exit.</param>
		/// <param name="exception">The exception associated with the validation error</param>
		/// <param name="errorMessage">A string containing the error message associated with the validation error</param>
		public EditModeValidationErrorEventArgs( ValueEditor editor, bool forceExitEditMode, Exception exception, string errorMessage )
		{
			_editor = editor;
			_forceExitEditMode = forceExitEditMode;
			_exception = exception;
			_errorMessage = errorMessage;

			_invalidValueBehavior = editor.InvalidValueBehaviorResolved;
		}

		/// <summary>
		/// Gets the associated value editor.
		/// </summary>
		public ValueEditor Editor
		{
			get
			{
				return _editor;
			}
		}

		/// <summary>
		/// Indicates if the edit mode is being exitted forcefully. For example, when the
		/// application is being closed.
		/// </summary>
		public bool ForceExitEditMode
		{
			get { return _forceExitEditMode; }
		}

		/// <summary>
		/// Gets or sets the invalid value behavior.
		/// </summary>
		public InvalidValueBehavior InvalidValueBehavior
		{
			get
			{
				return _invalidValueBehavior;
			}
			set
			{
				_invalidValueBehavior = value;
			}
		}

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
			set
			{
				_errorMessage = value;
			}
		}

		/// <summary>
		/// Gets any exception associated with the validation error.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return _exception;
			}
		}
	}

	#endregion // EditModeValidationErrorEventArgs class
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