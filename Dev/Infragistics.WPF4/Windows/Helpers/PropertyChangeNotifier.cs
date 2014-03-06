using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;




namespace Infragistics

{
	#region NestedPropertyChangedEventArgs


	/// <summary>
	/// Event arg class used to specify full notification context when sub objects raise events
	/// and the listening object also implements INotifyPropertyChanged
	/// </summary>
	[Obsolete("This class has been obsoleted. To handle property changes for nested objects you should use a Binding with a nested path.", true)]
	public class NestedPropertyChangedEventArgs : PropertyChangedEventArgs
	{
		#region Private Members

		private object _nestedSender;
		private PropertyChangedEventArgs _nestedArgs;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NestedPropertyChangedEventArgs"/> class
		/// </summary>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="nestedSender">The sender whose property change notification triggered this change</param>
		/// <param name="nestedArgs">The nested arguments</param>
		[Obsolete("This class has been obsoleted. To handle property changes for nested objects you should use a Binding with a nested path.", true)]
		public NestedPropertyChangedEventArgs(string propertyName, object nestedSender, PropertyChangedEventArgs nestedArgs)
			: base(propertyName)
		{
			this._nestedSender = nestedSender;
			this._nestedArgs = nestedArgs;
		}

		#endregion //Constructors

		#region NestedSender

		/// <summary>
		/// Returns the sender whose property change notification triggered this change (read-only)
		/// </summary>
		public object NestedSender { get { return this._nestedSender; } }

		#endregion //NestedSender

		#region NestedArgs

		/// <summary>
		/// Returns the nested arguments (read-only)
		/// </summary>
		/// <remarks>May return a <see cref="PropertyChangedEventArgs"/> or another <see cref="NestedPropertyChangedEventArgs"/></remarks>
		public PropertyChangedEventArgs NestedArgs { get { return this._nestedArgs; } }

		#endregion //NestedArgs
	}


	#endregion //NestedPropertyChangedEventArgs	
	
	#region PropertyChangeNotifier

	/// <summary>
	/// Abstract base class for objects that implement INotifyPropertyChanged
	/// </summary>
	abstract public class PropertyChangeNotifier : INotifyPropertyChanged
	{
		#region Private Members

		private PropertyChangedEventHandler _propertyChangedHandler;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class
		/// </summary>
		protected PropertyChangeNotifier()
		{
		}

		#endregion //Constructors

		#region Properties

			#region Protected Properties

				// JJD 1/24/08 - BR29985 - added
				#region HasListeners

		/// <summary>
		/// Gets whether there are any listeners for the <see cref="PropertyChanged"/> event
		/// </summary>
		/// <value>True is there are listeners to the <see cref="PropertyChanged"/> event, otherwise false.</value>
		/// <seealso cref="OnHasListenersChanged"/>
		/// <seealso cref="PropertyChanged"/>
		/// <seealso cref="OnFirstListenerAdding"/>
		protected bool HasListeners { get { return this._propertyChangedHandler != null; } }

				#endregion //HasListeners

			#endregion //Protected Properties	
		
		#endregion //Properties	
	
		#region Methods

			#region Protected Methods

				#region OnFirstListenerAdding

		// SSP 2/3/09 - NAS9.1 Record Filtering
		// Added OnFirstListenerAdding method. ViewableRecordCollection uses this to verify
		// itself before anyone hooks into it so it doesn't raise notifications after it's
		// hooked into.
		// 
		/// <summary>
		/// Virtual method called when the first listener is being added to the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the default inpmplementation does nothing. This method is intended
		/// for use by derived classes that may want to be notified when the # of listeners to the 
		/// <see cref="PropertyChanged"/> event transitions from 0 to 1.
		/// </para>
		/// </remarks>
		/// <seealso cref="HasListeners"/>
		/// <seealso cref="PropertyChanged"/>
		/// <seealso cref="OnHasListenersChanged"/>
		protected virtual void OnFirstListenerAdding( )
		{
		}

				#endregion // OnFirstListenerAdding

				// JJD 1/24/08 - BR29985 - added
				#region OnHasListenersChanged

		/// <summary>
		/// Virtual method called when the HasListeners property changes.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the default inpmplementation does nothing. This method is intended
		/// for use by derived classes that may want to be notified when the # of listeners to the <see cref="PropertyChanged"/> 
		/// event transitions from 0 to 1 or 1 to 0;
		/// </para></remarks>
		/// <seealso cref="HasListeners"/>
		/// <seealso cref="PropertyChanged"/>
		/// <seealso cref="OnFirstListenerAdding"/>
		protected virtual void OnHasListenersChanged() { }

				#endregion //OnHasListenersChanged	
	
				#region OnPropertyChanged
		/// <summary>
		/// Used to raise the <see cref="PropertyChanged"/> event for the specified property name.
		/// </summary>
		/// <param name="propertyName">The name of the property that has changed.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = _propertyChangedHandler;

			if (null != handler)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
				#endregion // OnPropertyChanged

				#region RaisePropertyChangedEvent

		/// <summary>
		/// Raises the PropertyChanged event
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			// AS 5/12/10
			// Moved to a virtual method that follows the recommended pattern of OnEventName.
			//
			//if (this._propertyChangedHandler != null)
			//    this._propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
			this.OnPropertyChanged(propertyName);
		}


		/// <summary>
		/// Raises the PropertyChanged event
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="nestedSender">The sender whose property change notification triggered this change</param>
		/// <param name="nestedArgs">The nested arguments</param>
		[Obsolete("Notifications for nested property has been deprecated. To raise a property change notification for a non-nested property, use the OnPropertyChanged method.", true)]
		protected void RaisePropertyChangedEvent(string propertyName, object nestedSender, PropertyChangedEventArgs nestedArgs)
		{
			// AS 5/12/10
			// We're obsoleting this for a few reasons. First, we don't make use of it except that we do raise it 
			// in some cases but we never listen for it. Second, we want to share this with the SL assembly which 
			// won't need this but will need to maintain its object model which includes a protected virtual 
			// OnPropertyChanged method which only takes a propertyname.
			//
			//if (this._propertyChangedHandler != null)
			//{
			//    if (nestedSender == null && nestedArgs == null)
			//        this._propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
			//    else
			//        this._propertyChangedHandler(this, new NestedPropertyChangedEventArgs(propertyName, nestedSender, nestedArgs));
			//}
		}

				#endregion //RaisePropertyChangedEvent

			#endregion //Protected Methods

		#endregion //Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property has changed
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			add
			{
				// JJD 1/24/08 - BR29985 
				// get the current HasListeners state
				bool hadListeners = this.HasListeners;

				// SSP 2/3/09 - NAS9.1 Record Filtering
				// Added OnFirstListenerAdding method.
				// 
				if ( ! hadListeners )
					this.OnFirstListenerAdding( );

				this._propertyChangedHandler = System.Delegate.Combine(this._propertyChangedHandler, value) as PropertyChangedEventHandler;

				// JJD 1/24/08 - BR29985 
				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if (this.HasListeners != hadListeners)
					this.OnHasListenersChanged();
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				// JJD 1/24/08 - BR29985 
				// get the current HasListeners state
				bool hadListeners = this.HasListeners;

				this._propertyChangedHandler = System.Delegate.Remove(this._propertyChangedHandler, value) as PropertyChangedEventHandler;

				// JJD 1/24/08 - BR29985 
				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if (this.HasListeners != hadListeners)
					this.OnHasListenersChanged();
			}
		}

		#endregion
	}

	#endregion //PropertyChangeNotifier	
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