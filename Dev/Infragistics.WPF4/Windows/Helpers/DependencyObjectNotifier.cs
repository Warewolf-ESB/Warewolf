using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infragistics
{
	#region DependencyObjectNotifier

	/// <summary>
	/// Abstract base class for objects that implement INotifyPropertyChanged and derive from DependencyObject
	/// </summary>
	abstract public class DependencyObjectNotifier : DependencyObject, INotifyPropertyChanged
	{
		#region Private Members

		private PropertyChangedEventHandler _propertyChangedHandler;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyObjectNotifier"/> class
		/// </summary>
		protected DependencyObjectNotifier()
		{
		}

		#endregion //Constructors

		#region Methods

		#region Protected Methods

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
				this._propertyChangedHandler = System.Delegate.Combine(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
			}
			[MethodImpl(MethodImplOptions.Synchronized)]
			remove
			{
				this._propertyChangedHandler = System.Delegate.Remove(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
			}
		}

		#endregion
	}

	#endregion //DependencyObjectNotifier

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