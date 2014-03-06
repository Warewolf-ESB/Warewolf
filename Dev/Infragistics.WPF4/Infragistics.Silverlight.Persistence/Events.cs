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
using System.Reflection;
using System.Collections.ObjectModel;

namespace Infragistics.Persistence
{
	/// <summary>
	/// An object used to pass information about a property being saved or loaded. 
	/// </summary>
	public abstract class PropertyPersistenceEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="PropertyPersistenceEventArgs"/> object. 
		/// </summary>
		/// <param name="rootOwner"></param>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		protected PropertyPersistenceEventArgs(DependencyObject rootOwner, PropertyInfo pi, string propertyPath)
		{
			this.PropertyPath = propertyPath;
			this.PropertyInfo = pi;
			this.RootOwner = rootOwner;
		}
				
		/// <summary>
		/// Gets the full property path of an object being saved or loaded. 
		/// </summary>
		public string PropertyPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the PropertyInfo of an object being saved or loaded. 
		/// </summary>
		public PropertyInfo PropertyInfo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the root object that is actually being saved or loaded. 
		/// </summary>
		public DependencyObject RootOwner
		{
			get;
			private set;
		}
	}


	/// <summary>
	/// An object used to pass information of a property being saved. 
	/// </summary>
	public class SavePropertyPersistenceEventArgs : PropertyPersistenceEventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SavePropertyPersistenceEventArgs"/> object. 
		/// </summary>
		/// <param name="rootOwner"></param>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		/// <param name="value"></param>
		public SavePropertyPersistenceEventArgs(DependencyObject rootOwner, PropertyInfo pi, string propertyPath, object value)
			: base(rootOwner, pi, propertyPath)
		{
			this.Value = value;
		}

		/// <summary>
		/// Gets/sets an identifier that can be used to identify an property being loaded. 
		/// </summary>
		/// <remarks>
		/// If specified, during loading the framework will attempt to load an object from the Application.Current.Resources using this Identifier. 
		/// Otherwise, it may be used by the end developer to identify a specific property in an a event. 
		/// </remarks>
		public string Identifier
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets a string representation of the <see cref="Value"/> that will be used to store the property.
		/// </summary>
		public string SaveValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the actual value of the property being stored. 
		/// </summary>
		public object Value
		{
			get;
			set;
		}

		/// <summary>
		/// Get/sets whether the property shouldn't be stored. 
		/// </summary>
		public bool Cancel
		{
			get;
			set;
		}

	}

	/// <summary>
	/// An object used to pass information of a property being loaded. 
	/// </summary>
	public class LoadPropertyPersistenceEventArgs : PropertyPersistenceEventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="LoadPropertyPersistenceEventArgs"/> object. 
		/// </summary>
		/// <param name="rootOwner"></param>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		/// <param name="savedValue"></param>
		/// <param name="id"></param>
		/// <param name="owner"></param>
		/// <param name="value"></param>
		public LoadPropertyPersistenceEventArgs(DependencyObject rootOwner, PropertyInfo pi, string propertyPath, string savedValue, string id, object owner, object value)
			: base(rootOwner, pi, propertyPath)
		{
			this.SavedValue = savedValue;
			this.Identifier = id;
			this.Owner = owner;
			this.Value = value;
		}

		/// <summary>
		/// Gets an identifier that can be used to identify an property being loaded. 
		/// </summary>
		/// <remarks>
		/// If specified, during loading the framework will attempt to load an object from the Application.Current.Resources using this Identifier. 
		/// Otherwise, it may be used by the end developer to identify a specific property in an a event. 
		/// </remarks>
		public string Identifier
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the object to whom the property belongs. 
		/// </summary>
		public object Owner
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a string representation of the property that was being stored. 
		/// </summary>
		public string SavedValue
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the value of the property, if the property was a Value type.
		/// </summary>
		public object Value
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets/sets the value that should be used to restore the property. 
		/// </summary>
		/// <remarks>
		/// When set, this value will be attempted to be used to load the property.
		/// </remarks>
		public object LoadedValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets whether the property was loaded in the event, and thus shouldn't try to be loaded by the framework.
		/// </summary>
		public bool Handled
		{
			get;
			set;
		}
	}

	/// <summary>
	/// An object used to pass information for when an object has been saved. 
	/// </summary>
	public class PersistenceSavedEventArgs : EventArgs
	{

	}

	/// <summary>
	/// An object used to pass information for when an object has been loaded. 
	/// </summary>
	public class PersistenceLoadedEventArgs : EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="PersistenceLoadedEventArgs"/> object. 
		/// </summary>
		/// <param name="exceptions"></param>
		public PersistenceLoadedEventArgs(ReadOnlyCollection<PropertyPersistenceExceptionDetails> exceptions)
		{
			this.PropertyExceptions = exceptions;
		}

		/// <summary>
		/// Gets a list of <see cref="PropertyPersistenceExceptionDetails"/> objects, for properties that could not be loaded. 
		/// </summary>
		public ReadOnlyCollection<PropertyPersistenceExceptionDetails> PropertyExceptions
		{
			get;
			private set;
		}

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