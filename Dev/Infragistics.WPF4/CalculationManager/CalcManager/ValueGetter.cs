using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;
using System.Linq;



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using System.Reflection;

namespace Infragistics.Calculations
{
	/// <summary>
	/// A class used to get and set value of a property or a binding from a data object.
	/// </summary>
	internal class ValueGetter : DependencyObject, ISupportPropertyChangeNotifications
	{
		#region Vars

		private PropertyChangeListenerList _listeners;
		internal readonly string _path;
		internal readonly Type _targetPropertyTypeIfKnown;

		#endregion // Vars

		#region Constructors

		public ValueGetter( object dataItem, string path )
		{
			Binding binding = new Binding( path );
			binding.Mode = BindingMode.TwoWay;
			binding.Source = dataItem;

			_path = path;

			// Find out the target property type which is used to determine if we should
			// pass FormattedValue into the binding or not. We pass FormattedValue if the
			// target is a string type so we can use the control's culture to convert
			// calculation result into the string.
			// 
			Type targetPropertyType = null;
			if ( !path.Contains( '.' ) )
			{
				PropertyInfo propInfo = dataItem.GetType( ).GetProperty( path );
				targetPropertyType = null != propInfo ? propInfo.PropertyType : null;
			}

			_targetPropertyTypeIfKnown = targetPropertyType;

			BindingOperations.SetBinding( this, ValueProperty, binding );
		}

		public ValueGetter( object dataItem, Binding binding )
		{
			_targetPropertyTypeIfKnown = null;

			binding.Source = dataItem;
			BindingOperations.SetBinding( this, ValueProperty, binding );
		} 

		#endregion // Constructors

		#region Properties

		#region Public Properties

		// MD 8/25/11 - TFS84804
		#region Path

		public string Path
		{
			get { return _path; }
		}

		#endregion  // Path

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyPropertyUtilities.Register(
			"Value",
			typeof( object ),
			typeof( ValueGetter ),
			null, 
			new PropertyChangedCallback( OnValueChanged ) 
		);

		private static void OnValueChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueGetter item = (ValueGetter)d;

			if ( null != item._listeners )
				item._listeners.OnPropertyValueChanged( item, "Value", e );
		}

		public object Value
		{
			get
			{
				return (object)this.GetValue( ValueProperty );
			}
			set
			{
				this.SetValue( ValueProperty, value );
			}
		}

		#endregion // Value 

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region GetDefaultBindingPath

		/// <summary>
		/// Gets the default binding path that should be used get and set the value on the control in the absense of explicitly
		/// specified binding or property to use.
		/// </summary>
		/// <param name="control">Control.</param>
		/// <returns>Binding path or null if the control is not recognized.</returns>
		internal static string GetDefaultBindingPath( object control )
		{
			string path = null;

			Type controlType = control.GetType( );
			Type type = controlType;

			do
			{
				string typeName = type.FullName;
				switch ( typeName )
				{
					case "Infragistics.Windows.Editors.ValueEditor":
					case "Infragistics.Controls.Editors.ValueInput":
					case "Infragistics.Controls.Editors.XamMaskedEditor":
					case "Infragistics.Controls.Editors.XamNumericSlider":
					case "Infragistics.Controls.Editors.XamSliderNumericThumb":
					case "Infragistics.Controls.Editors.XamDateTimeSlider":
					case "Infragistics.Controls.Editors.XamSliderDateTimeThumb":
					case "Infragistics.Controls.Editors.ByteSlider":
					case "Infragistics.Controls.Editors.ByteSliderThumb":
					case "System.Windows.Controls.Primitives.RangeBase":
						path = "Value";
						break;
					case "System.Windows.Controls.ContentControl":
						path = "Content";
						break;
					case "System.Windows.Controls.TextBlock":
					case "System.Windows.Controls.TextBox":
						path = "Text";
						break;
					case "System.Windows.Controls.CheckBox":
					case "System.Windows.Controls.Primitives.ToggleButton":
						path = "IsChecked";
						break;
					case "System.Windows.Controls.Primitives.Selector":
						path = "SelectedValue";
						break;
					case "System.Windows.Controls.DatePicker":
					case "System.Windows.Controls.Calendar":
					case "Infragistics.Windows.Editors.XamMonthCalendar":
					case "Infragistics.Controls.Editors.CalendarBase":
						path = "SelectedDate";
						break;
				}

				type = type.BaseType;
			}
			while ( null == path && null != type );

			// If the control has a DefaultProperty attribute then use that.
			// 
			if ( null == path )
			{
				var attribs = controlType.GetCustomAttributes( typeof( DefaultPropertyAttribute ), true );
				DefaultPropertyAttribute defaultPropAttrib = null == attribs ? null
					: attribs.FirstOrDefault( ii => ii is DefaultPropertyAttribute ) as DefaultPropertyAttribute;

				if ( null != defaultPropAttrib )
					path = defaultPropAttrib.Name;
			}

			return path;
		}

		#endregion // GetDefaultBindingPath 

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			if ( null == _listeners )
				_listeners = new PropertyChangeListenerList( );

			_listeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			if ( null != _listeners )
				_listeners.Remove( listener );
		} 

		#endregion // ISupportPropertyChangeNotifications Implementation
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