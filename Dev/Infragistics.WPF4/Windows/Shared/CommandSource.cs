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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Infragistics.Controls
{
	/// <summary>
	/// An object that describes what kind of Command should be attached to a particular object, and what should trigger the command.
	/// </summary>
	public abstract class CommandSource : DependencyObjectNotifier
    {
        #region Members

        bool _allowEventHandling = true;
        FrameworkElement _se;

        #endregion // Members

        #region Properties

        #region Public

        #region EventName

        /// <summary>
		/// Gets/Sets the name of an event that should trigger a command. 
		/// </summary>
		/// <remarks>
		/// For example, if you were setting the CommandSource for a button, you might want to set the EventName to "Click"
		/// </remarks>
		public string EventName
		{
			get;
			set;
		}

		#endregion // EventName

		#region SourceElement

		/// <summary>
		/// Gets the <see cref="FrameworkElement"/> that this command represents.
		/// </summary>
		/// <remarks>
		/// If you were to attach the <see cref="CommandSource"/> to a <see cref="Button"/>, this property would return that Button.
		/// </remarks>
		public FrameworkElement SourceElement
		{
            get
            {
                return this._se;
            }
            protected internal set
            {
                if (this._se != value)
                {
                    this._se = value;
                    this.OnPropertyChanged("SourceElement");
                }
            }
		}

		#endregion // SourceElement

		#region Target

		/// <summary>
		/// This is the <see cref="ICommandTarget"/> object that command should be triggered on. 
		/// </summary>
		/// <remarks>
		/// If not explicitly set, the Commanding Framework will walk up the VisualTree from the <see cref="CommandSource.SourceElement"/>
		/// until it finds an object that implements <see cref="ICommandTarget"/>
		/// </remarks>
		public object Target
		{
			get;
			set;
		}

		#endregion // Target

		#region TargetName

		/// <summary>
		/// If no target is resolved, then the Command will look for Globally registered targets. If this property is set, 
		/// it will look for an ICommandTarget that is a FrameworkElement and has the specified name.
		/// </summary>
		public string TargetName
		{
			get;
			set;
		}

		#endregion // TargetName

		#region Parameter

		#region Parameter

		/// <summary>
		/// Identifies the <see cref="Parameter"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register("Parameter", typeof(object), typeof(CommandSource), new PropertyMetadata(new PropertyChangedCallback(ParameterChanged)));

		/// <summary>
		/// If the Command that this <see cref="CommandSource"/> is supposed to trigger, requries additional information. 
		/// The information can be passed through this property.
		/// </summary>		
		public object Parameter
		{
			get { return (object)this.GetValue(ParameterProperty); }
			set { this.SetValue(ParameterProperty, value); }
		}

		private static void ParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			CommandSource source = (CommandSource)obj;
			source.OnPropertyChanged("Parameter");
		}

		#endregion // Parameter 

		#endregion // Parameter

		#region ParameterBinding

		/// <summary>
		/// Gets/Sets the Binding that will determine the parameter for this <see cref="CommandSource"/>
		/// </summary>
		public Binding ParameterBinding
		{
			get;
			set;
		}

		#endregion // ParameterBinding

		#region ParameterResolved

		/// <summary>
		/// Gets the actual parameter that was set. 
		/// If a ParameterBinding was setting, it will return the binding value. 
		/// If it wasn't set, it will return the value in the Parameter property. 
		/// </summary>
		public object ParameterResolved
		{
			get
			{
				if (this.ParameterBinding == null)
					return this.Parameter;
				else
				{
                    CommandSourceBindingContext context = new CommandSourceBindingContext();
                    if (this.SourceElement.DataContext != null)
                    {

                        Binding b = new Binding("DataContext");
                        b.Mode = System.Windows.Data.BindingMode.OneWay;
                        b.Source = this.SourceElement;
                        context.SetBinding(FrameworkElement.DataContextProperty, b);
                    }
                    else
                    {
                        context.DataContext = this.SourceElement;
                    }

                    context.SetBinding(CommandSourceBindingContext.ValueProperty, this.ParameterBinding);

					return context.Value;
				}
			}
		}

		#endregion // ParameterResolved

        #region Command

        /// <summary>
		/// Gets the <see cref="ICommand"/> that this <see cref="CommandSource"/> represents.
		/// </summary>
		public ICommand Command
		{
			get { return this.ResolveCommand(); }
		}

		#endregion // Command

		#region Handled

		/// <summary>
		/// This should only be used by the <see cref="ICommand"/> to handle the Event that triggered the command, so that it
		/// stops bubbling. 
		/// </summary>
		public bool Handled
		{
			get;
			set;
		}

		#endregion // Handled

        #region AllowEventHandling

        /// <summary>
        /// Gets/Sets if the Event for the attached command, should be Handled.
        /// </summary>
        public bool AllowEventHandling
        {
            get { return this._allowEventHandling; }
            set { this._allowEventHandling = value; }
        }

        #endregion // AllowEventHandling

        #endregion // Public

        #region Protected

        #region EventHandler
        /// <summary>
		/// Gets/Sets the EventHandler that was created based off of the <see cref="CommandSource.EventName"/> property. 
		/// </summary>
		protected internal Delegate EventHandler
		{
			get;
			set;
		}
		#endregion // EventHandler

		#endregion // Protected

		#endregion // Properties

		#region Methods

		#region Abstract

		/// <summary>
		/// Resolves the <see cref="ICommand"/> that this <see cref="CommandSource"/> represents.
		/// </summary>
		/// <returns></returns>
		protected abstract ICommand ResolveCommand();

		#endregion // Abstract

		#region Protected

		#region ResolveParameter

		/// <summary>
		/// This method resolves the parameter that should be passed to the <see cref="ICommand"/>.  
		/// It will use the specified parameter and check with the specified <see cref="ICommandTarget"/> to 
		/// see what parameter should be used. 
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected object ResolveParameter(ICommandTarget target)
		{
			object parameter = this.ParameterResolved;
			if (parameter == null && target != null)
				parameter = target.GetParameter(this);
			return parameter;
		}
		#endregion // ResolveParameter

		#region Execute
		
		/// <summary>
		/// Invokes the <see cref="ICommand"/> that is attached to this <see cref="CommandSource"/>
		/// </summary>
		/// <param name="target">The <see cref="ICommandTarget"/> that the command should be invoked on. </param>
		/// <param name="canExecute">Whether or not that the <see cref="ICommand"/> can exectue.</param>
		protected void Execute(ICommandTarget target, bool canExecute)
		{
			ICommand command = this.Command;
			CommandBase comBase = command as CommandBase;
			if (comBase != null)
			{
				comBase.CommandSource = this;
			}

			object param = this.ResolveParameter(target);
			if (canExecute)
			{
				if (command.CanExecute(param))
					command.Execute(param);
			}
			else
			{
				Control c = this.SourceElement as Control;
				if (c != null)
					c.IsEnabled = command.CanExecute(param);
			}
		}

		#endregion // Execute

		#region InvokeCommand

		/// <summary>
		/// Invokes the <see cref="ICommand"/> attached to the <see cref="CommandSource"/>
		/// </summary>
		/// <param name="execute">Whether or not the <see cref="ICommand"/> can exectue. </param>
		protected internal void InvokeCommand(bool execute)
		{
			FrameworkElement elem = this.SourceElement;

			ICommand command = this.Command;

			CommandBase comBase = command as CommandBase;
			if (comBase != null)
			{
				comBase.CommandSource = this;
			}
			if (this.Target == null)
			{
				bool emptyTargetName = String.IsNullOrEmpty(this.TargetName);
				if (emptyTargetName)
				{
					if (this.LookForCommandTarget(elem, execute, command))
						return;
				}

				Collection<ICommandTarget> targets = CommandSourceManager.GetTargets(command);
				foreach (ICommandTarget target in targets)
				{
					FrameworkElement targetElem = target as FrameworkElement;
					if (emptyTargetName || (targetElem != null && targetElem.Name == this.TargetName)) 
						this.Execute(target, execute);
				}
			}
			else
				this.Execute(this.Target as ICommandTarget, execute);
		}
		#endregion // InvokeCommand

		#region OriginEventArgs
		/// <summary>
		/// Gets the event argument that started the event 
		/// </summary>
		public EventArgs OriginEventArgs { get; protected set; }
		#endregion // OriginEventArgs

		#endregion // Protected

		#region Private

		private bool LookForCommandTarget(FrameworkElement parent, bool execute, ICommand command)
		{
			ItemsControl ic = null;
			while (parent != null)
			{
                ICommandTarget target = Commanding.GetCommandTarget(parent) as ICommandTarget;
                if (target != null && target.SupportsCommand(command))
                {
                    this.Execute(target, execute);
                    return true;
                }

				ItemsControl cntrl = parent as ItemsControl;
				if (cntrl != null)
					ic = cntrl;
				target = parent as ICommandTarget;
				if (target != null && target.SupportsCommand(command))
				{
					this.Execute(target, execute);
					return true;
				}
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				FrameworkElement oldParent = parent;

				parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;

				if (parent == null && oldParent != null)
				{
					parent = oldParent.Parent as FrameworkElement;
				}
			}

			if (ic != null)
				return this.LookForCommandTarget(ic.Parent as FrameworkElement, execute, command);

			return false;
		}

		#endregion // Private

		#endregion // Methods

		#region EventHandlers

		#region EventFired

		internal void EventFired(object sender, EventArgs e)
		{
			MouseButtonEventArgs mouseButtonEventArgs = e as MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.Handled)
				return;

			KeyEventArgs keyEventArgs = e as KeyEventArgs;
			if (keyEventArgs != null && keyEventArgs.Handled)
				return;

			this.OriginEventArgs = e;

			this.InvokeCommand(true);

			if (this.Handled && this.AllowEventHandling)
			{
				if (mouseButtonEventArgs != null)
					mouseButtonEventArgs.Handled = true;

				if (keyEventArgs != null)
					keyEventArgs.Handled = true;

				this.Handled = false;
			}

            this.OriginEventArgs = null;
		}

		#endregion // EventFired

		#endregion // EventHandlers

		#region CommandSourceBindingContext

		private class CommandSourceBindingContext : FrameworkElement
		{
			#region Value

			/// <summary>
			/// Identifies the <see cref="Value"/> dependency property. 
			/// </summary>
			public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(CommandSourceBindingContext), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

			public object Value
			{
				get { return (object)this.GetValue(ValueProperty); }
				set { this.SetValue(ValueProperty, value); }
			}

			private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
			{

			}

			#endregion // Value 
		}
		#endregion // CommandSourceBindingContext

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