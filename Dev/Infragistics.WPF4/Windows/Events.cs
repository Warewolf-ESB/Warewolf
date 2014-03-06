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
using Infragistics.Windows.Controls;
using System.Collections;

namespace Infragistics.Windows.Controls.Events
{
	#region CancelableRoutedEventArgs

	/// <summary>
	/// An abstract base class for routed event args that are cancelable.
	/// </summary>
	public abstract class CancelableRoutedEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private bool _cancel;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="CancelableRoutedEventArgs"/> class. 
		/// </summary>
		public CancelableRoutedEventArgs()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CancelableRoutedEventArgs"/> class, using the supplied routed event identifier.
		/// </summary>
		/// <param name="routedEvent">The routed event identifier for this instance of the <see cref="CancelableRoutedEventArgs"/> class</param>
		public CancelableRoutedEventArgs(RoutedEvent routedEvent)
			: base(routedEvent)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CancelableRoutedEventArgs"/> class, using the supplied routed event identifier and 
		/// providing the opportunity to declare a different source for the event.
		/// </summary>
		/// <param name="routedEvent">The routed event identifier for this instance of the <see cref="CancelableRoutedEventArgs"/> class</param>
		/// <param name="source">An alternate source that will be reported when the event is handled. This prepopulates the <see cref="RoutedEventArgs.Source"/> property.</param>
		public CancelableRoutedEventArgs(RoutedEvent routedEvent, object source)
			: base(routedEvent, source)
		{
		}
		#endregion //Constructor

		#region Properties

		#region Cancel
		/// <summary>
		/// Gets/sets a boolean indicating whether the associated operation should be cancelled. 
		/// </summary>
		public bool Cancel
		{
			get { return this._cancel; }
			set { this._cancel = value; }
		}
		#endregion //Cancel

		#endregion //Properties
	}

	#endregion //CancelableRoutedEventArgs

	#region CollapseStateChangedEvent //commented out

    ///// <summary>
    ///// Event arguments for routed event 'CollapseStateChanged'
    ///// </summary>
    //public class CollapseStateChangedEventArgs : RoutedEventArgs
    //{
    //    private bool isCollapsed;

    //    /// <summary>
    //    /// Constructor
    //    /// </summary>
    //    /// <param name="isCollapsed"></param>
    //    public CollapseStateChangedEventArgs( bool isCollapsed )
    //    {
    //        this.isCollapsed = isCollapsed;
    //    }

    //    /// <summary>
    //    /// Invokes the event
    //    /// </summary>
    //    protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
    //    {
    //        ((CollapseStateChangedEventHandler)genericHandler)(genericTarget, this);
    //    }

    //    /// <summary>
    //    /// Returns true if the new state is collapsed (read-only)
    //    /// </summary>
    //    public bool IsCollapsed { get { return this.isCollapsed; } }
    //}

    #endregion CollapseStateChangedEvent

	#region ExecutingCommandEventArgs class

	/// <summary>
	/// Cancelable event arguments class that provides data for a <b>ExecutingCommand</b> event.
	/// </summary>
	public class ExecutingCommandEventArgs : CancelableRoutedEventArgs
	{
		#region Member Variables

		private RoutedCommand _command;

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        private object _parameter;

        // JJD 06/02/10 - TFS33112
        private bool _continueKeyRouting = true;    

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutingCommandEventArgs"/> with the specified <see cref="RoutedCommand"/>
		/// </summary>
		/// <param name="command">The command that will be executed</param>
		public ExecutingCommandEventArgs(RoutedCommand command) : this(command, null)
		{
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        /// <summary>
		/// Initializes a new instance of the <see cref="ExecutingCommandEventArgs"/> with the specified <see cref="RoutedCommand"/>
		/// </summary>
		/// <param name="command">The command that will be executed</param>
        /// <param name="parameter">The parameter for the command</param>
		public ExecutingCommandEventArgs(RoutedCommand command, object parameter)
		{
			this._command = command;
            this._parameter = parameter;
		} 
		#endregion //Constructor

		#region Properties

		#region Command
		/// <summary>
		/// Returns the command to be performed (read-only).
		/// </summary>
		public RoutedCommand Command { get { return this._command; } }

		#endregion //Command

        // JJD 06/02/10 - TFS33112
        #region ContinueKeyRouting

        /// <summary>
        /// Gets/sets whether to continue routing the key that triggered the command.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property only has meaning if <b>Cancel</b> is set to true. The default value is true.</para>
        /// </remarks>
        public bool ContinueKeyRouting
        {
            get { return this._continueKeyRouting; }
            set { this._continueKeyRouting = value; }
        }

        #endregion //ContinueKeyRouting	
    
        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        #region Parameter
        /// <summary>
        /// Returns the parameter used when executing the command.
        /// </summary>
        public object Parameter
        {
            get { return this._parameter; }
        } 
        #endregion //Parameter

		#endregion //Properties
	}

	#endregion //ExecutingCommandEventArgs class

	#region ExecutedCommandEventArgs class

	/// <summary>
	/// Provides data for a <b>ExecutedCommand</b> event.
	/// </summary>
	public class ExecutedCommandEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private RoutedCommand _command;

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        private object _parameter;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutedCommandEventArgs"/> with the specified <see cref="RoutedCommand"/>
		/// </summary>
		/// <param name="command">The command that has been executed</param>
		public ExecutedCommandEventArgs(RoutedCommand command) : this(command, null)
		{
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        /// <summary>
		/// Initializes a new instance of the <see cref="ExecutedCommandEventArgs"/> with the specified <see cref="RoutedCommand"/>
		/// </summary>
		/// <param name="command">The command that has been executed</param>
        /// <param name="parameter">The parameter for the command</param>
		public ExecutedCommandEventArgs(RoutedCommand command, object parameter)
		{
			this._command = command;
            this._parameter = parameter;
		} 
		#endregion //Constructor

		#region Properties

		#region Command
		/// <summary>
		/// Returns the command that was performed (read-only).
		/// </summary>
		public RoutedCommand Command { get { return this._command; } } 
		#endregion //Command

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        #region Parameter
        /// <summary>
        /// Returns the parameter used when executing the command.
        /// </summary>
        public object Parameter
        {
            get { return this._parameter; }
        } 
        #endregion //Parameter

		#endregion //Properties
	}

	#endregion //ExecutedCommandEventArgs class

    #region SelectedOperatorChangedEventArgs

    /// <summary>
    /// Event args for the <see cref="ComparisonOperatorSelector.SelectedOperatorChanged"/> event of the <see cref="ComparisonOperatorSelector"/>
    /// </summary>
    public class SelectedOperatorChangedEventArgs : RoutedEventArgs
    {
        #region Private Members
    
        private ComparisonOperator? _oldValue;
        private ComparisonOperator? _newValue;

   	    #endregion //Private Members	
    
        #region Constructor
    
        internal SelectedOperatorChangedEventArgs(ComparisonOperator? oldValue, ComparisonOperator? newValue)
        {
            this._newValue = newValue;
            this._oldValue = oldValue;
        }

   	    #endregion //Constructor	
    
        #region Properties

        /// <summary>
        /// Returns the old operator value (read-only).
        /// </summary>
        /// <seealso cref="ComparisonOperatorSelector.SelectedOperator"/>
        /// <seealso cref="ComparisonOperatorSelector.SelectedOperatorChanged"/>
        public ComparisonOperator? OldValue { get { return this._oldValue; } }

        /// <summary>
        /// Returns the new operator value (read-only).
        /// </summary>
        /// <seealso cref="ComparisonOperatorSelector.SelectedOperator"/>
        /// <seealso cref="ComparisonOperatorSelector.SelectedOperatorChanged"/>
        public ComparisonOperator? NewValue { get { return this._newValue; } }

        #endregion //Properties	
    
    }

    #endregion //SelectedOperatorChangedEventArgs	
    
	#region SortStatusChangedEventArgs class

	/// <summary>
	/// Provides data for an event involving a change in a <see cref="SortStatus"/>
	/// </summary>
	public class SortStatusChangedEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private SortStatus _previousValue;
		private SortStatus _currentValue;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="SortStatusChangedEventArgs"/> with the specified old and new <see cref="SortStatus"/> values.
		/// </summary>
		/// <param name="previousValue">The original sort status before the change occurred</param>
		/// <param name="currentValue">The new sort status</param>
		public SortStatusChangedEventArgs(SortStatus previousValue, SortStatus currentValue)
		{
			this._previousValue = previousValue;
			this._currentValue = currentValue;
		} 
		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the sort status before the change (read-only)
		/// </summary>
		public SortStatus PreviousValue { get { return this._previousValue; } }

		/// <summary>
		/// Returns the sort status after the change (read-only)
		/// </summary>
		public SortStatus CurrentValue { get { return this._currentValue; } } 

		#endregion //Properties
	}

	#endregion //SortStatusChangedEventArgs class


	#region TabControlDropDownOpeningEventArgs
	/// <summary>
	/// Event args for the <see cref="XamTabControl.DropDownOpening"/> event of the <see cref="XamTabControl"/>
	/// </summary>
	public class TabControlDropDownOpeningEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Initializes a new <see cref="TabControlDropDownOpeningEventArgs"/>
		/// </summary>
		public TabControlDropDownOpeningEventArgs()
		{
		}
	} 
	#endregion //TabControlDropDownOpeningEventArgs
    
    #region TabClosingEventArgs class

    /// <summary>
    /// Event arguments for routed event <see cref="TabItemEx.Closing"/>
    /// </summary>
    /// <seealso cref="TabItemEx.Closing"/>
    /// <seealso cref="TabItemEx.ClosingEvent"/>
    public class TabClosingEventArgs : CancelableRoutedEventArgs
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public TabClosingEventArgs()
        {
        }
    }

    #endregion //ClosingEventArgs class and delegate

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