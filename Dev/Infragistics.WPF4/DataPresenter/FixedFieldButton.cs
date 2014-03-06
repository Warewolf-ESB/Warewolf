using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.Windows.Internal;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
    /// <summary>
    /// A custom element used to provide UI within a <see cref="LabelPresenter"/> for changing the <see cref="Infragistics.Windows.DataPresenter.Field.FixedLocation"/> of a field.
    /// </summary>
    //[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FixedFieldButton : Control, IWeakEventListener
    {

		#region Constructor
        static FixedFieldButton()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FixedFieldButton), new FrameworkPropertyMetadata(typeof(FixedFieldButton)));
        }

        /// <summary>
        /// Initializes a new <see cref="FixedFieldButton"/>
        /// </summary>
        public FixedFieldButton()
        {
            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			// JJD 3/16/11 - TFS24163
			// Initialize new command properties which eliminate the overhead of routed commands
			this.SetValue(FixToNearEdgeCommandPropertyKey, new FixCommand(this, DataPresenterCommands.FixFieldNear));
			this.SetValue(FixToFarEdgeCommandPropertyKey, new FixCommand(this, DataPresenterCommands.FixFieldFar));
			this.SetValue(UnfixCommandPropertyKey, new FixCommand(this, DataPresenterCommands.UnfixField));
		} 
        #endregion //Constructor

        #region Base class overrides

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="FixedFieldButton"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.FixedFieldButtonAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new Infragistics.Windows.Automation.Peers.DataPresenter.FixedFieldButtonAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer 

        #endregion //Base class overrides

        #region Properties

		// JJD 3/16/11 - TFS24163
		// Added new command properties which eleminate the overhead of routed commands
		#region Command Properties	

			#region CurrentCommand

		private static readonly DependencyPropertyKey CurrentCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("CurrentCommand",
			typeof(ICommand), typeof(FixedFieldButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="CurrentCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentCommandProperty =
			CurrentCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the currently active command to bind to the button inside this element's template (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property will return null if the associated <see cref="Field"/>'s <see cref="Infragistics.Windows.DataPresenter.Field.AllowFixingResolved"/> returns 'NearOrFar' and its <see cref="Infragistics.Windows.DataPresenter.Field.FixedLocation"/> is 'Scrollable'. </para>
		/// </remarks>
		/// <seealso cref="CurrentCommandProperty"/>
		//[Description("Returns the currently active command to bind to the button inside this element's template (read-only)")]
		//[Category("Behavior")]
		public ICommand CurrentCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedFieldButton.CurrentCommandProperty);
			}
		}

			#endregion //CurrentCommand

			#region FixToNearEdgeCommand

		private static readonly DependencyPropertyKey FixToNearEdgeCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("FixToNearEdgeCommand",
			typeof(ICommand), typeof(FixedFieldButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="FixToNearEdgeCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToNearEdgeCommandProperty =
			FixToNearEdgeCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to fix the field to the near edge (read-only).
		/// </summary>
		/// <seealso cref="FixToNearEdgeCommandProperty"/>
		//[Description("Returns the command to fix the field to the near edge (read-only)")]
		//[Category("Behavior")]
		public ICommand FixToNearEdgeCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedFieldButton.FixToNearEdgeCommandProperty);
			}
		}

			#endregion //FixToNearEdgeCommand

			#region FixToFarEdgeCommand

		private static readonly DependencyPropertyKey FixToFarEdgeCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("FixToFarEdgeCommand",
			typeof(ICommand), typeof(FixedFieldButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="FixToFarEdgeCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToFarEdgeCommandProperty =
			FixToFarEdgeCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to fix the field to the far edge (read-only).
		/// </summary>
		/// <seealso cref="FixToFarEdgeCommandProperty"/>
		//[Description("Returns the command to fix the field to the far edge (read-only)")]
		//[Category("Behavior")]
		public ICommand FixToFarEdgeCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedFieldButton.FixToFarEdgeCommandProperty);
			}
		}

			#endregion //FixToFarEdgeCommand

			#region UnfixCommand

		private static readonly DependencyPropertyKey UnfixCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("UnfixCommand",
			typeof(ICommand), typeof(FixedFieldButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="UnfixCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UnfixCommandProperty =
			UnfixCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to unfix the field to make it scrollable (read-only).
		/// </summary>
		/// <seealso cref="UnfixCommandProperty"/>
		//[Description("Returns the command to unfix the field to make it scrollable (read-only)")]
		//[Category("Behavior")]
		public ICommand UnfixCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedFieldButton.UnfixCommandProperty);
			}
		}

			#endregion //UnfixCommand

		#endregion //Command Properties

        #region Field

        /// <summary>
        /// Identifies the <see cref="Field"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FieldProperty = DependencyProperty.Register("Field",
			typeof(Field), typeof(FixedFieldButton), new FrameworkPropertyMetadata(null, OnFieldChanged));

		// JJD 3/16/11 - TFS24163 - added
		private static void OnFieldChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FixedFieldButton instance = target as FixedFieldButton;

			instance.VerifyCommandState();

			Field oldfld = e.OldValue as Field;

			if ( oldfld != null )
				PropertyChangedEventManager.RemoveListener(oldfld, instance, string.Empty);

			Field fld = e.NewValue as Field;

			// listen for changes in the field's FixedLocation and AllowFixingResolved properties
			if (fld != null)
				PropertyChangedEventManager.AddListener(fld, instance, string.Empty);
		}

		//private void OnFieldPropertyChanged(object sender, 

        /// <summary>
        /// Returns or sets the Field whose FixedLocation will be affected by the element.
        /// </summary>
        /// <seealso cref="FieldProperty"/>
        //[Description("Returns or sets the Field whose FixedLocation will be affected by the element.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public Field Field
        {
            get
            {
                return (Field)this.GetValue(FixedFieldButton.FieldProperty);
            }
            set
            {
                this.SetValue(FixedFieldButton.FieldProperty, value);
            }
        }

        #endregion //Field

		#region FixToFarEdgePrompt

		/// <summary>
		/// Identifies the <see cref="FixToFarEdgePrompt"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToFarEdgePromptProperty = DependencyProperty.Register("FixToFarEdgePrompt",
			typeof(string), typeof(FixedFieldButton), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns/sets the prompt used for the 'Fix To Far Edge' item in the menu displayed when the FixedFieldButton is clicked.
		/// </summary>
		/// <seealso cref="FixToFarEdgePromptProperty"/>
		//[Description("Returns/sets the prompt used for the 'Fix To Far Edge' item in the menu displayed when the FixedFieldButton is clicked.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string FixToFarEdgePrompt
		{
			get
			{
				return (string)this.GetValue(FixedFieldButton.FixToFarEdgePromptProperty);
			}
			set
			{
				this.SetValue(FixedFieldButton.FixToFarEdgePromptProperty, value);
			}
		}

		#endregion //FixToFarEdgePrompt

		#region FixToNearEdgePrompt

		/// <summary>
		/// Identifies the <see cref="FixToNearEdgePrompt"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToNearEdgePromptProperty = DependencyProperty.Register("FixToNearEdgePrompt",
			typeof(string), typeof(FixedFieldButton), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns/sets the prompt used for the 'Fix To Near Edge' item in the menu displayed when the FixedFieldButton is clicked.
		/// </summary>
		/// <seealso cref="FixToNearEdgePromptProperty"/>
		//[Description("Returns/sets the prompt used for the 'Fix To Near Edge' item in the menu displayed when the FixedFieldButton is clicked.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string FixToNearEdgePrompt
		{
			get
			{
				return (string)this.GetValue(FixedFieldButton.FixToNearEdgePromptProperty);
			}
			set
			{
				this.SetValue(FixedFieldButton.FixToNearEdgePromptProperty, value);
			}
		}

		#endregion //FixToNearEdgePrompt

		#region UnfixPrompt

		/// <summary>
		/// Identifies the <see cref="UnfixPrompt"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UnfixPromptProperty = DependencyProperty.Register("UnfixPrompt",
			typeof(string), typeof(FixedFieldButton), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns/sets the prompt used for the 'Fix To Near Edge' item in the menu displayed when the FixedFieldButton is clicked.
		/// </summary>
		/// <seealso cref="UnfixPromptProperty"/>
		//[Description("Returns/sets the prompt used for the 'Unfix' item in the menu displayed when the FixedFieldButton is clicked.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string UnfixPrompt
		{
			get
			{
				return (string)this.GetValue(FixedFieldButton.UnfixPromptProperty);
			}
			set
			{
				this.SetValue(FixedFieldButton.UnfixPromptProperty, value);
			}
		}

		#endregion //UnfixPrompt

        #endregion //Properties

		#region Methods

		// JJD 3/16/11 - TFS24163 - added
		#region OnFieldPropertyChanged

		private void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "FixedLocation":
				case "AllowFixingResolved":
					this.VerifyCommandState();
					break;
			}
		}

		#endregion //OnFieldPropertyChanged	
    
		// JJD 3/16/11 - TFS24163 - added
		#region VerifyCommandState

		private void VerifyCommandState()
		{
			FixCommand command;

			command = this.FixToFarEdgeCommand as FixCommand;
			command.VerifyCanEecute();
			command = this.FixToNearEdgeCommand as FixCommand;
			command.VerifyCanEecute();
			command = this.UnfixCommand as FixCommand;
			command.VerifyCanEecute();

			ICommand currentCommand = null;
			Field fld = this.Field;

			if (fld != null)
			{
				FixedFieldLocation location = fld.FixedLocation;

				if (location == FixedFieldLocation.Scrollable)
				{
					// only set the current command if fixing is only allowed on
					// either the near or far edge not both
					switch (fld.AllowFixingResolved)
					{
						case AllowFieldFixing.Far:
							currentCommand = this.FixToFarEdgeCommand;
							break;

						case AllowFieldFixing.Near:
							currentCommand = this.FixToNearEdgeCommand;
							break;
					}
				}
				else
				{
					currentCommand = this.UnfixCommand;
				}
			}

			if (currentCommand != null)
				this.SetValue(CurrentCommandPropertyKey, currentCommand);
			else
				this.ClearValue(CurrentCommandPropertyKey);

		}

		#endregion //VerifyCommandState

		#endregion //Methods	
		
		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					if (sender is Field)
						this.OnFieldPropertyChanged(args);
					else
					{
						Debug.Fail("Invalid sender in ReceiveWeakEvent for FixedFieldButton, sender: " + sender != null ? sender.ToString() : "null");
						return false;
					}
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for FixedFieldButton, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for FixedFieldButton, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion

		// JJD 3/16/11 - TFS24163 - added
		#region FixCommand private class

		private class FixCommand : ICommand
		{
			#region Private Members

			private FixedFieldButton _owner;
			private RoutedCommand _command;
			private bool? _lastCanExecute;

			#endregion //Private Members

			#region Constructor

			internal FixCommand(FixedFieldButton owner, RoutedCommand command)
			{
				_owner = owner;
				_command = command;
			}

			#endregion //Constructor

			#region Events

			public event EventHandler CanExecuteChanged;

			#endregion //Events

			#region Methods

			#region Public Methods

			#region CanExecute

			public bool CanExecute(object parameter)
			{
				return this.CanExecuteHelper();
			}

			#endregion //CanExecute

			#region Execute

			public void Execute(object parameter)
			{
				// Call CanEecute first. This will allow wiring of the PreviewCanExecute and CanExecute
				// routed events
				if (_command.CanExecute(parameter, _owner))
					_command.Execute(parameter, _owner);
			}

			#endregion //Execute

			#region VerifyCanEecute

			public void VerifyCanEecute()
			{
				// only call CanExecuteHelper if it was called already
				if (_lastCanExecute.HasValue)
					this.CanExecuteHelper();
			}

			#endregion //VerifyCanEecute

			#endregion //Public Methods

			#region Private Methods

			#region CanExecuteHelper

			private bool CanExecuteHelper()
			{
				bool canExecute = false;

				Field fld = _owner.Field;

				if (fld != null)
				{
					if (_command == DataPresenterCommands.UnfixField)
					{
						canExecute = fld.FixedLocation != FixedFieldLocation.Scrollable;
					}
					else
					{
						AllowFieldFixing allowfixing = fld.AllowFixingResolved;

						if (_command == DataPresenterCommands.FixFieldFar)
							canExecute = allowfixing == AllowFieldFixing.Far || allowfixing == AllowFieldFixing.NearOrFar;
						else
						if (_command == DataPresenterCommands.FixFieldNear)
							canExecute = allowfixing == AllowFieldFixing.Near || allowfixing == AllowFieldFixing.NearOrFar;
					}
				}

				// on the first call we don't want to raise the event
				if (_lastCanExecute.HasValue == false)
					_lastCanExecute = canExecute;
				else
					if (_lastCanExecute.Value != canExecute)
					{
						_lastCanExecute = canExecute;

						// raise the changed event i there are any listeners
						if (this.CanExecuteChanged != null)
							this.CanExecuteChanged(this, EventArgs.Empty);
					}

				return canExecute;
			}

			#endregion //CanExecuteHelper

			#endregion //Private Methods

			#endregion //Methods
		}

		#endregion //FixCommand private class
	
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