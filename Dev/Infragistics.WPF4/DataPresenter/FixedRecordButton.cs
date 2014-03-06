using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using System.Diagnostics;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// A custom element used to provide UI within a <see cref="RecordPresenter"/> for changing the <see cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/> of a record.
    /// </summary>
    //[ToolboxItem(false)]
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FixedRecordButton : Control
    {
		#region Private Members

		// JJD 3/16/11 - TFS24163 - added
		private PropertyValueTracker _tracker;

		#endregion //Private Members	
    
        #region Constructor
        static FixedRecordButton()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FixedRecordButton), new FrameworkPropertyMetadata(typeof(FixedRecordButton)));

			// JJD 3/16/11 - TFS24163
			// Added callcak for DataContext changed
			FrameworkElement.DataContextProperty.OverrideMetadata(typeof(FixedRecordButton), new FrameworkPropertyMetadata(OnDataContextChanged));

       }

        /// <summary>
        /// Initializes a new <see cref="FixedRecordButton"/>
        /// </summary>
        public FixedRecordButton()
        {
			// JJD 3/16/11 - TFS24163
			// Initialize new command properties which eliminate the overhead of routed commands
			this.SetValue(FixToTopCommandPropertyKey, new FixCommand(this, DataPresenterCommands.FixRecordTop));
			this.SetValue(FixToBottomCommandPropertyKey, new FixCommand(this, DataPresenterCommands.FixRecordBottom));
			this.SetValue(UnfixCommandPropertyKey, new FixCommand(this, DataPresenterCommands.UnfixRecord));
		}
        #endregion //Constructor

        #region Base class overrides

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="FixedRecordButton"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.FixedRecordButtonAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new Infragistics.Windows.Automation.Peers.DataPresenter.FixedRecordButtonAutomationPeer(this);
        }
            #endregion //OnCreateAutomationPeer

			// JJD 1/5/12 - TFS18884 - added
			#region OnPreviewMouseLeftButtonDown

		/// <summary>
		/// Invoked when the left mouse button is being pressed down within the element.
		/// </summary>
		/// <param name="e">Provides information about the preview mouse event being raised.</param>
		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			// JJD 1/5/12 - TFS18884
			// If fixing is allowed on top and bottom (which will result in a menu 
			// being displayed) and if the associated record is not the active rcd and it is activateable
			// then activate it now. Otherwise, when the mouse enters the menu focus
			// will shift causing the menu to close up.
			if (IsFixedOnBottomAllowed &&
				IsFixedOnTopAllowed)
			{
				Record rcd = this.DataContext as Record;

				if (rcd != null &&
					 rcd.IsActive == false &&
					 rcd.IsActivatable)
					rcd.IsActive = true;
			}

			base.OnPreviewMouseLeftButtonDown(e);
		}

			#endregion //OnPreviewMouseLeftButtonDown	
    
        #endregion //Base class overrides

        #region Properties

            #region Public Properties

				// JJD 3/16/11 - TFS24163
				// Added new command properties which eleminate the overhead of routed commands
				#region Command Properties	

					#region CurrentCommand

		private static readonly DependencyPropertyKey CurrentCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("CurrentCommand",
			typeof(ICommand), typeof(FixedRecordButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="CurrentCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentCommandProperty =
			CurrentCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the currently active command to bind to the button inside this element's template (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property will return null if <see cref="IsFixedOnTopAllowed"/> equals <see cref="IsFixedOnBottomAllowed"/> and the associated <see cref="Record"/>'s <see cref="Record.FixedLocation"/> is 'Scrollable'.</para>
		/// </remarks>
		/// <seealso cref="CurrentCommandProperty"/>
		[Description("Returns the command to unfix the record to make it scrollable (read-only)")]
		[Category("Behavior")]
		public ICommand CurrentCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedRecordButton.CurrentCommandProperty);
			}
		}

					#endregion //CurrentCommand

					#region FixToTopCommand

		private static readonly DependencyPropertyKey FixToTopCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("FixToTopCommand",
			typeof(ICommand), typeof(FixedRecordButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="FixToTopCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToTopCommandProperty =
			FixToTopCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to fix the record to the top (read-only).
		/// </summary>
		/// <seealso cref="FixToTopCommandProperty"/>
		[Description("Returns the command to fix the record to the top (read-only)")]
		[Category("Behavior")]
		public ICommand FixToTopCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedRecordButton.FixToTopCommandProperty);
			}
		}

					#endregion //FixToTopCommand

					#region FixToBottomCommand

		private static readonly DependencyPropertyKey FixToBottomCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("FixToBottomCommand",
			typeof(ICommand), typeof(FixedRecordButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="FixToBottomCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixToBottomCommandProperty =
			FixToBottomCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to fix the record to the bottom (read-only).
		/// </summary>
		/// <seealso cref="FixToBottomCommandProperty"/>
		[Description("Returns the command to fix the record to the bottom (read-only)")]
		[Category("Behavior")]
		public ICommand FixToBottomCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedRecordButton.FixToBottomCommandProperty);
			}
		}

					#endregion //FixToBottomCommand

					#region UnfixCommand

		private static readonly DependencyPropertyKey UnfixCommandPropertyKey =
			DependencyProperty.RegisterReadOnly("UnfixCommand",
			typeof(ICommand), typeof(FixedRecordButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="UnfixCommand"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UnfixCommandProperty =
			UnfixCommandPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the command to unfix the record to make it scrollable (read-only).
		/// </summary>
		/// <seealso cref="UnfixCommandProperty"/>
		[Description("Returns the command to unfix the record to make it scrollable (read-only)")]
		[Category("Behavior")]
		public ICommand UnfixCommand
		{
			get
			{
				return (ICommand)this.GetValue(FixedRecordButton.UnfixCommandProperty);
			}
		}

					#endregion //UnfixCommand

				#endregion //Command Properties	
    
                #region FixToBottomPrompt

        /// <summary>
        /// Identifies the <see cref="FixToBottomPrompt"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixToBottomPromptProperty = DependencyProperty.Register("FixToBottomPrompt",
            typeof(string), typeof(FixedRecordButton), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/sets the prompt used for the 'Fix To Bottom' item in the menu displayed when the FixedRecordButton is clicked.
        /// </summary>
        /// <seealso cref="FixToBottomPromptProperty"/>
        //[Description("Returns/sets the prompt used for the 'Fix To Bottom' item in the menu displayed when the FixedRecordButton is clicked.")]
        //[Category("Appearance")]
        [Bindable(true)]
        public string FixToBottomPrompt
        {
            get
            {
                return (string)this.GetValue(FixedRecordButton.FixToBottomPromptProperty);
            }
            set
            {
                this.SetValue(FixedRecordButton.FixToBottomPromptProperty, value);
            }
        }

                #endregion //FixToBottomPrompt

                #region FixToTopPrompt

        /// <summary>
        /// Identifies the <see cref="FixToTopPrompt"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixToTopPromptProperty = DependencyProperty.Register("FixToTopPrompt",
            typeof(string), typeof(FixedRecordButton), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/sets the prompt used for the 'Fix To Top' item in the menu displayed when the FixedRecordButton is clicked.
        /// </summary>
        /// <seealso cref="FixToTopPromptProperty"/>
        //[Description("Returns/sets the prompt used for the 'Fix To Top' item in the menu displayed when the FixedRecordButton is clicked.")]
        //[Category("Appearance")]
        [Bindable(true)]
        public string FixToTopPrompt
        {
            get
            {
                return (string)this.GetValue(FixedRecordButton.FixToTopPromptProperty);
            }
            set
            {
                this.SetValue(FixedRecordButton.FixToTopPromptProperty, value);
            }
        }

                #endregion //FixToTopPrompt

                #region IsFixedOnBottomAllowed

        /// <summary>
        /// Identifies the <see cref="IsFixedOnBottomAllowed"/> dependency property
        /// </summary>
        /// <seealso cref="IsFixedOnBottomAllowed"/>
        public static readonly DependencyProperty IsFixedOnBottomAllowedProperty = DependencyProperty.Register("IsFixedOnBottomAllowed",
			typeof(bool), typeof(FixedRecordButton), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, OnIsFixedAllowedChanged));

		// JJD 3/16/11 - TFS24163 - added
		private static void OnIsFixedAllowedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FixedRecordButton instance = target as FixedRecordButton;

			instance.VerifyCommandState();
		}


        /// <summary>
        /// Indicates whether fixing to the bottom edge is allowed for this record.
        /// </summary>
        /// <seealso cref="IsFixedOnBottomAllowedProperty"/>
        /// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
        //[Description("Indicates whether fixing to the bottom edge is allowed for this record.")]
        //[Category("Behavior")]
        public bool IsFixedOnBottomAllowed
        {
            get
            {
                return (bool)this.GetValue(FixedRecordButton.IsFixedOnBottomAllowedProperty);
            }
            set
            {
                this.SetValue(FixedRecordButton.IsFixedOnBottomAllowedProperty, value);
            }
        }

                #endregion //IsFixedOnBottomAllowed

                #region IsFixedOnTopAllowed

        /// <summary>
        /// Identifies the <see cref="IsFixedOnTopAllowed"/> dependency property
        /// </summary>
        /// <seealso cref="IsFixedOnTopAllowed"/>
        public static readonly DependencyProperty IsFixedOnTopAllowedProperty = DependencyProperty.Register("IsFixedOnTopAllowed",
			typeof(bool), typeof(FixedRecordButton), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, OnIsFixedAllowedChanged));

        /// <summary>
        /// Indicates whether fixing to the top edge is allowed for this record.
        /// </summary>
        /// <seealso cref="IsFixedOnTopAllowedProperty"/>
        /// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
        //[Description("Indicates whether fixing to the top edge is allowed for this record.")]
        //[Category("Behavior")]
        public bool IsFixedOnTopAllowed
        {
            get
            {
                return (bool)this.GetValue(FixedRecordButton.IsFixedOnTopAllowedProperty);
            }
            set
            {
                this.SetValue(FixedRecordButton.IsFixedOnTopAllowedProperty, value);
            }
        }

                #endregion //IsFixedOnTopAllowed

                #region UnfixPrompt

        /// <summary>
        /// Identifies the <see cref="UnfixPrompt"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UnfixPromptProperty = DependencyProperty.Register("UnfixPrompt",
            typeof(string), typeof(FixedRecordButton), new FrameworkPropertyMetadata());

        /// <summary>
        /// Returns/sets the prompt used for the 'Fix To Top' item in the menu displayed when the FixedRecordButton is clicked.
        /// </summary>
        /// <seealso cref="UnfixPromptProperty"/>
        //[Description("Returns/sets the prompt used for the 'Unfix' item in the menu displayed when the FixedRecordButton is clicked.")]
        //[Category("Appearance")]
        [Bindable(true)]
        public string UnfixPrompt
        {
            get
            {
                return (string)this.GetValue(FixedRecordButton.UnfixPromptProperty);
            }
            set
            {
                this.SetValue(FixedRecordButton.UnfixPromptProperty, value);
            }
        }

                #endregion //UnfixPrompt

            #endregion //Public Properties

        #endregion //Properties

		#region Methods

		// JJD 3/16/11 - TFS24163 - added
		#region OnDataContextChanged

		private static void OnDataContextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FixedRecordButton instance = target as FixedRecordButton;

			instance.VerifyCommandState();

			Record rcd = e.NewValue as Record;

			// listen for changes in th record's FixedLocation property
			if (rcd != null)
				instance._tracker = new PropertyValueTracker(rcd, "FixedLocation", instance.VerifyCommandState);
			else
				instance._tracker = null;
		}

		#endregion //OnDataContextChanged

		// JJD 3/16/11 - TFS24163 - added
		#region VerifyCommandState

		private void VerifyCommandState()
		{
			FixCommand command;

			command = this.FixToBottomCommand as FixCommand;
			command.VerifyCanEecute();
			command = this.FixToTopCommand as FixCommand;
			command.VerifyCanEecute();
			command = this.UnfixCommand as FixCommand;
			command.VerifyCanEecute();

			ICommand currentCommand = null;
			Record rcd = this.DataContext as Record;

			if (rcd != null)
			{
				FixedRecordLocation location = rcd.FixedLocation;

				if (location == FixedRecordLocation.Scrollable)
				{
					bool isFixedOnTopAllowed = this.IsFixedOnTopAllowed;
					bool isFixedOnBottomAllowed = this.IsFixedOnBottomAllowed;

					// only set the current command if fixing is only allowed on
					// either the top or bottom
					if (isFixedOnBottomAllowed != isFixedOnTopAllowed)
					{
						if (isFixedOnTopAllowed)
							currentCommand = this.FixToTopCommand;
						else
							currentCommand = this.FixToBottomCommand;
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
        
		// JJD 3/16/11 - TFS24163 - added
		#region FixCommand private class

		private class FixCommand : ICommand
		{
			#region Private Members

			private FixedRecordButton _owner;
			private RoutedCommand _command;
			private bool? _lastCanExecute;

			#endregion //Private Members

			#region Constructor

			internal FixCommand(FixedRecordButton owner, RoutedCommand command)
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

				Record rcd = _owner.DataContext as Record;

				if (rcd != null)
				{
					if (_command == DataPresenterCommands.FixRecordTop)
						canExecute = _owner.IsFixedOnTopAllowed;
					else
						if (_command == DataPresenterCommands.FixRecordBottom)
							canExecute = _owner.IsFixedOnBottomAllowed;
						else
							canExecute = rcd.FixedLocation != FixedRecordLocation.Scrollable;
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