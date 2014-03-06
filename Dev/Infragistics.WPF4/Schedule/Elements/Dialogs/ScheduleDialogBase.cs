using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for a control that represents a schedule dialog
	/// </summary>
	[DesignTimeVisible(false)]
	public abstract class ScheduleDialogBase<TChoice> : Control
		, IDialogElementProxyHost
		, ICommandTarget
		, IScheduleDialog
	{
		#region Member Variables

		private ChooserResult _result;
		private Dictionary<string, string> _localizedStrings;
		private DialogElementProxy _dialogElementProxy;
		private bool _isDialogInitialized;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ScheduleDialogBase&lt;TChoice&gt;"/>
		/// </summary>
		protected ScheduleDialogBase()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleDialogBase&lt;TChoice&gt;"/>
		/// </summary>
		/// <param name="chooserResult">A reference to a <see cref="ChooserResult"/> instance. The dialog will set the <see cref="ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		protected ScheduleDialogBase(ChooserResult chooserResult) : this()
		{
			CoreUtilities.ValidateNotNull(chooserResult, "chooserResult");
			this._result = chooserResult;
		}
		#endregion //Constructor

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.DialogElementProxy.Initialize();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.InitializeImpl));
		}
		#endregion //OnApplyTemplate

		/// <summary>
		/// Called before the System.Windows.UIElement.KeyDown event occurs
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.Enter)
			{
				// AS 3/31/11 TFS69820
				IScheduleDialog dlg = this;

				if (dlg.CanSaveAndClose)
				{
					dlg.SaveAndClose();
					e.Handled = true;
				}
			}
		}

		#region OnKeyUp
		/// <summary>
		/// Called before the System.Windows.UIElement.KeyUp event occurs
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Key == Key.Escape)
			{
				this.Close(false);
				e.Handled = true;
			}
		}
		#endregion //OnKeyUp

		#endregion //Base Class Overrides

		#region Properties

		#region Private Properties

		#region DialogElementProxy
		private DialogElementProxy DialogElementProxy
		{
			get
			{
				if (this._dialogElementProxy == null)
					this._dialogElementProxy = new DialogElementProxy(this);

				return this._dialogElementProxy;
			}
		}
		#endregion //DialogElementProxy

		#endregion //Private Properties

		#region Internal Properties

		#region CanSaveAndClose
		internal abstract bool CanSaveAndClose
		{
			get;
		}
		#endregion //CanSaveAndClose

		#region IsDialogInitialized
		internal bool IsDialogInitialized
		{
			get { return _isDialogInitialized; }
		} 
		#endregion //IsDialogInitialized

		#region Result
		internal ChooserResult Result
		{
			get { return _result; }
		} 
		#endregion //Result

		#endregion //Internal Properties

		#region Public Properties

		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (_localizedStrings == null)
				{
					_localizedStrings = new Dictionary<string, string>();
					this.InitializeLocalizedStrings(_localizedStrings);
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region GetCommandParameter
		internal virtual object GetCommandParameter(CommandSource source)
		{
			if (source.Command is ScheduleDialogCommandBase)
				return this;

			return null;
		}
		#endregion //GetCommandParameter

		#region Initialize
		internal virtual void Initialize()
		{
			// AS 4/11/11 TFS71618
			// If focus is not within the dialog then shift focus into it.
			//
			if (!PresentationUtilities.HasFocus(this))
			{
				PresentationUtilities.Focus(this);
			}
		} 
		#endregion //Initialize

		#region InitializeLocalizedStrings
		internal virtual void InitializeLocalizedStrings(Dictionary<string, string> localizedStrings)
		{
		} 
		#endregion //InitializeLocalizedStrings

		#region OnClosing
		internal virtual bool OnClosing()
		{
			// Nothing to do here.

			// Don't cancel the closing.
			return false;
		}
		#endregion //OnClosing

		#region Save
		internal abstract void Save();
		#endregion //Save

		#region SupportsCommand
		internal virtual bool SupportsCommand(ICommand command)
		{
			return command is ScheduleDialogCommandBase;
		}
		#endregion //SupportsCommand

		#endregion //InternalMethods

		#region Private Methods

		#region Close
		private void Close(bool result)
		{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			this.SetDialogResult(result);

		}
		#endregion //Close

		#region InitializeImpl
		private void InitializeImpl()
		{
			this.Initialize();
			_isDialogInitialized = true;
		} 
		#endregion //InitializeImpl

		#region SetDialogResult

		private void SetDialogResult(bool result)
		{
			if (this.DialogElementProxy != null)
				this.DialogElementProxy.SetDialogResult(result);
		}

		#endregion //SetDialogResult

		#endregion //Private Methods

		#endregion //Methods

		#region ChooserResult class
		/// <summary>
		/// A class that holds the choice made by the user in the <see cref="ScheduleDialogBase&lt;TChoice&gt;"/>. 
		/// </summary>
		public class ChooserResult
		{
			#region Constructor
			/// <summary>
			/// Creates an instance of the ChooserResult which should be passed in the constructor of the <see cref="ScheduleDialogBase&lt;TChoice&gt;"/>.
			/// The dialog will set the <see cref="Choice"/> property when the dialog closes to reflect the user's choice.
			/// </summary>
			/// <param name="userData">An opaque piece of user data - this parameter can be null.</param>
			public ChooserResult(object userData)
			{
				this.Choice = default(TChoice);
				this.UserData = userData;
			}
			#endregion Constructor

			#region Properties

			#region Choice
			/// <summary>
			/// Returns the choice made by the user in the dialog. 
			/// </summary>
			public TChoice Choice
			{
				get;
				internal set;
			}
			#endregion //Choice

			#region UserData
			/// <summary>
			/// Returns or sets an opaque piece of user data.
			/// </summary>
			public object UserData
			{
				get;
				set;
			}
			#endregion //UserData

			#endregion //Properties
		}
		#endregion //ChooserResult class

		#region IDialogElementProxyHost Members

		bool IDialogElementProxyHost.OnClosing()
		{
			return this.OnClosing();
		}

		#endregion //IDialogElementProxyHost Members

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetCommandParameter(source);
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		#endregion //ICommandTarget Members
	
		#region IScheduleDialog
		bool IScheduleDialog.CanSaveAndClose
		{
			get
			{
				return this.CanSaveAndClose;
			}
		}

		void IScheduleDialog.SaveAndClose()
		{
			this.Save();
			this.Close(true);
		}

		void IScheduleDialog.Close()
		{
			this.Close(false);
		} 
		#endregion //IScheduleDialog
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