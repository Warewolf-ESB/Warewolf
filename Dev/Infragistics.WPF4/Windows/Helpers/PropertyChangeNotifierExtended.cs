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
using System.Collections.Generic;




namespace Infragistics

{
	#region PropertyChangeNotifierExtended

	/// <summary>
	/// Abstract base class for objects that implement INotifyPropertyChanged.
	/// </summary>
	public abstract class PropertyChangeNotifierExtended : PropertyChangeNotifier, ISupportPropertyChangeNotifications, IPropertyChangeListener
	{
		#region Private Members

		private object _listeners;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangeNotifierExtended"/> class.
		/// </summary>
		protected PropertyChangeNotifierExtended( )
		{
		}

		#endregion // Constructors

		#region Base Overrides

		#region OnPropertyChanged

		/// <summary>
		/// Overridden. Called when property changed event is raised.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected override void OnPropertyChanged( string propertyName )
		{
			base.OnPropertyChanged( propertyName );

			this.NotifyListeners( this, propertyName, null );
		}

		#endregion // OnPropertyChanged 

		#endregion // Base Overrides

		#region Methods

		#region SetField
		/// <summary>
		/// Helper method used by property setters to change the value of a field and raise the PropertyChanged event.
		/// </summary>
		/// <typeparam name="T">The type of field being changed</typeparam>
		/// <param name="member">The field member passed by reference that will be updated/compared with the new value</param>
		/// <param name="newValue">The new value for the field</param>
		/// <param name="propertyName">The name of the property being changed</param>
		/// <returns>Returns false if the new value matches the existing member; otherwise true if the value is different</returns>
		protected virtual bool SetField<T>(ref T member, T newValue, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(member, newValue))
				return false;

			member = newValue;
			this.OnPropertyChanged(propertyName);
			return true;
		}
		#endregion //SetField

		#region Internal Methods

		#region AddListener

		internal void AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_listeners = ListenerList.Add( _listeners, listener, useWeakReference );
		}

		#endregion // AddListener

		#region NotifyListeners

		/// <summary>
		/// Notifies listeners of the property change.
		/// </summary>
		/// <param name="sender">Object whose property changed.</param>
		/// <param name="property">Property that changed.</param>
		/// <param name="extraInfo">Any extra info associated with the property change.</param>
		internal void NotifyListeners( object sender, string property, object extraInfo )
		{
			this.OnSubObjectPropertyChanged( sender, property, extraInfo );
		}

		#endregion // NotifyListeners

		#region OnSubObjectPropertyChanged

		/// <summary>
		/// Called when a subobject's, including this object's, property changes.
		/// </summary>
		/// <param name="sender">Object whose property changed.</param>
		/// <param name="property">Property that changed.</param>
		/// <param name="extraInfo">Any extra info associated with the property change.</param>
		internal virtual void OnSubObjectPropertyChanged( object sender, string property, object extraInfo )
		{
			if ( null != _listeners )
				ListenerList.RaisePropertyChanged<object, string>( _listeners, sender, property, extraInfo );
		}

		#endregion // OnSubObjectPropertyChanged

		#region RemoveListener

		internal void RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_listeners = ListenerList.Remove( _listeners, listener );
		}

		#endregion // RemoveListener

		#endregion // Internal Methods 

		#endregion // Methods

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			this.AddListener( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			this.RemoveListener( listener );
		}

		#endregion // ISupportPropertyChangeNotifications Implementation

		#region IPropertyChangeListener Implementation

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object sender, string property, object extraInfo )
		{
			this.NotifyListeners( sender, property, extraInfo );
		}

		#endregion // IPropertyChangeListener Implementation
	}

	#endregion // PropertyChangeNotifierExtended	
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