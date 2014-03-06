using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;


namespace Infragistics.Services



{
	internal static class DependencyPropertyUtilities
	{
		#region Member Variables

		private static readonly object MissingValue = DependencyProperty.UnsetValue;

		#endregion // Member Variables

		#region CreateMetadata

		internal static PropertyMetadata CreateMetadata(object defaultValue)
		{
			return CreateMetadata(defaultValue, null);
		}

		internal static PropertyMetadata CreateMetadata(PropertyChangedCallback propertyChangeCallback)
		{
			return CreateMetadata(MissingValue, propertyChangeCallback);
		}

		internal static PropertyMetadata CreateMetadata(object defaultValue, PropertyChangedCallback propertyChangeCallback)
		{
			return CreateMetadata(defaultValue, propertyChangeCallback, MetadataOptionFlags.None);
		}

		internal static PropertyMetadata CreateMetadata(object defaultValue, PropertyChangedCallback propertyChangeCallback, MetadataOptionFlags flags)
		{






			FrameworkPropertyMetadataOptions options = FrameworkPropertyMetadataOptions.None;

			if (flags != MetadataOptionFlags.None)
			{
				if ( (flags & MetadataOptionFlags.BindsTwoWayByDefault) == MetadataOptionFlags.BindsTwoWayByDefault)
					options |= FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;

				if ( (flags & MetadataOptionFlags.Journal) == MetadataOptionFlags.Journal)
					options |= FrameworkPropertyMetadataOptions.Journal;
			
			}

			if (defaultValue == MissingValue)
			{
				if ( options == FrameworkPropertyMetadataOptions.None)
					return new FrameworkPropertyMetadata(propertyChangeCallback);
				else
					return new FrameworkPropertyMetadata(null, options, propertyChangeCallback);
			}

			return new FrameworkPropertyMetadata(defaultValue, options, propertyChangeCallback);

		}
		#endregion //CreateMetadata

		#region GetDefaultValue

		// JJD 5/13/11 - Added overload that takes a Type instead of an instance
		/// <summary>
		/// Gets the default value for the specified property for the specified object.
		/// </summary>
		/// <param name="d">Dependency object whose property value is to be evaluated</param>
		/// <param name="dp">Property to evaluate</param>
		/// <returns></returns>
		public static object GetDefaultValue(DependencyObject d, DependencyProperty dp)
		{
			if (d != null)
				return GetDefaultValue(d.GetType(), dp);

			return null;
		}
		/// <summary>
		/// Gets the default value for the specified property for the specified type.
		/// </summary>
		/// <param name="type">the type whose property value is to be evaluated</param>
		/// <param name="dp">Property to evaluate</param>
		/// <returns></returns>
		public static object GetDefaultValue(Type type, DependencyProperty dp)
		{

			PropertyMetadata data = dp.GetMetadata(type);

			Debug.Assert(null != data);

			if (null == data)
				data = dp.DefaultMetadata;




			Debug.Assert(null != data);
			object val = null != data ? data.DefaultValue : null;
			
			// SSP 2/24/2011
			// 
			if ( DependencyProperty.UnsetValue == val )
				val = null;

			return val;
		}

		#endregion // GetDefaultValue

		#region GetName

		/// <summary>
		/// Returns the Name of the DependencyProperty. The property should have been registered using one of the Register methods of this class.
		/// </summary>
		internal static string GetName(DependencyProperty dp)
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			// For debugging purposes, track names for WPF as well.




			return dp.Name;

		}

		/// <summary>
		/// Returns the name of the or caches it if it is able to find the public static field returning the DependencyProperty
		/// </summary>
		internal static string GetName(DependencyObject d, DependencyProperty dp)
		{

			return dp.Name;


#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion // GetName

		#region GetType

		internal static Type GetType( DependencyProperty dp )
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			// For debugging purposes, track names for WPF as well.




			return dp.PropertyType;

		}

		#endregion // GetType

		#region Register

		// For debugging purposes, track names for WPF as well.





		// AS 3/16/11 - DependencyObjectCallbackWrapper
		internal static DependencyProperty Register(string name, Type propertyType, Type ownerType, object defaultValue, PropertyChangedCallback propertyChangeCallback)
		{





			PropertyMetadata metadata = propertyChangeCallback != null
				? CreateMetadata(defaultValue, propertyChangeCallback)
 				// JJD 07/20/11 - Always create the metadata object so that the default value is not lost
				//: null;
				: CreateMetadata(defaultValue);

			return Register(name, propertyType, ownerType, metadata, false);
		}

		internal static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata metadata)
		{




			return Register(name, propertyType, ownerType, metadata, false);
		}

		private static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata metadata, bool isAttached)
		{
			DependencyProperty dp = isAttached
				? DependencyProperty.RegisterAttached(name, propertyType, ownerType, metadata)
				: DependencyProperty.Register(name, propertyType, ownerType, metadata);

			// For debugging purposes, track names for WPF as well.


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			return dp;
		}

		#endregion // Register

		#region RegisterAttached
		internal static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata metadata)
		{
			return Register(name, propertyType, ownerType, metadata, true);
		} 
		#endregion // RegisterAttached

		#region RegisterAttachedReadOnly
		internal static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, object defaultValue, PropertyChangedCallback changeCallback)
		{
			return RegisterReadOnly(name, propertyType, ownerType, defaultValue, changeCallback, true);
		}
		#endregion // RegisterAttachedReadOnly

		#region RegisterReadOnly
		internal static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, object defaultValue, PropertyChangedCallback changeCallback)
		{
			return RegisterReadOnly(name, propertyType, ownerType, defaultValue, changeCallback, false);
		}

		internal static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, object defaultValue, PropertyChangedCallback changeCallback, bool isAttached)
		{
			DependencyPropertyKey key;



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			PropertyMetadata metadata = CreateMetadata(defaultValue, changeCallback);
			key = isAttached
				? DependencyProperty.RegisterAttachedReadOnly(name, propertyType, ownerType, metadata)
				: DependencyProperty.RegisterReadOnly(name, propertyType, ownerType, metadata);




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			return key;
		}
		#endregion // RegisterReadOnly

		// AS 5/10/12 TFS111333/TFS111595
		#region SetCurrentValue
		internal static void SetCurrentValue(DependencyObject d, DependencyProperty property, object newValue)
		{



			d.SetCurrentValue(property, newValue);

		} 
		#endregion //SetCurrentValue

		#region ShouldSerialize

		/// <summary>
		/// A helper method for figuring out whether a property needs to be serialized.
		/// </summary>
		/// <param name="d">Dependency object whose property value is to be evaluated</param>
		/// <param name="dp">Property to evaluate</param>
		/// <returns></returns>
		public static bool ShouldSerialize(DependencyObject d, DependencyProperty dp)
		{
			
			
			
			
			

			
			

			object defVal = GetDefaultValue(d, dp);
			object currVal = d.GetValue(dp);

			return !object.Equals(defVal, currVal);

		}

		#endregion // ShouldSerialize

		#region MetadataOptionFlags






		[Flags]
		internal enum MetadataOptionFlags
		{
			None = 0,
			BindsTwoWayByDefault = 1,
			Journal = 2
		}

		#endregion //MetadataOptionFlags

		// AS 3/16/11 - DependencyObjectCallbackWrapper
		#region DependencyObjectCallbackWrapper class


#region Infragistics Source Cleanup (Region)







































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //DependencyObjectCallbackWrapper class
	}

	// AS 3/16/11 - DependencyObjectCallbackWrapper
	#region ValueHolder class
	/// <summary>
	/// Simple dependency object with a single Value dependency property
	/// </summary>
	internal class ValueHolder : DependencyObject
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ValueHolder), null);

		public object Value
		{
			get { return this.GetValue(ValueProperty); }
			set { this.SetValue(ValueProperty, value); }
		}
	}
	#endregion //ValueHolder class

	#region PropertyChangeCallbackWrapper


#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

	#endregion // PropertyChangeCallbackWrapper

	#region DependencyPropertyKey


#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

	#endregion // DependencyPropertyKey

	#region DependencyObjectExtensions
	internal static class DependencyObjectExtensions
	{
		#region Methods

		#region ClearValue


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		#endregion // ClearValue

		#region SetValue


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		#endregion // SetValue

		#endregion // Methods
	}
	#endregion // DependencyObjectExtensions

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