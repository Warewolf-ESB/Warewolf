using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Calculations
{
	#region IItemPropertyValueAccessor interface

	internal interface IItemPropertyValueAccessor
	{
		string Name { get; }
		Type PropertyType { get; }
		object GetValue(object obj);
		void SetValue(object obj, object value);
	}

	#endregion //IItemPropertyValueAccessor interface	
    
	#region PropertyInfoValueAccessor class

	internal class PropertyInfoValueAccessor : IItemPropertyValueAccessor
	{
		#region Private Members

		private PropertyInfo _propInfo;

		#endregion //Private Members

		#region Constructor

		internal PropertyInfoValueAccessor(PropertyInfo propInfo)
		{
			_propInfo = propInfo;
		}

		#endregion //Constructor

		#region IItemPropertyValueAccessor Members

		public string Name { get { return _propInfo.Name; } }

		public Type PropertyType { get { return _propInfo.PropertyType; } }

		public object GetValue(object obj)
		{
			return _propInfo.GetValue(obj, null);
		}

		public void SetValue(object obj, object value)
		{
			_propInfo.SetValue(obj, value, null);
		}

		#endregion
	}

	#endregion //PropertyInfoValueAccessor class


	#region PropertyDescriptorValueAccessor class

	internal class PropertyDescriptorValueAccessor : IItemPropertyValueAccessor
	{
		#region Private Members

		private PropertyDescriptor _pd;

		#endregion //Private Members

		#region Constructor

		internal PropertyDescriptorValueAccessor(PropertyDescriptor pd)
		{
			_pd = pd;
		}

		#endregion //Constructor

		#region IItemPropertyValueAccessor Members

		public string Name { get { return _pd.Name; } }
		
		public Type PropertyType { get { return _pd.PropertyType; } }

		public object GetValue(object obj)
		{
			return _pd.GetValue(obj);
		}

		public void SetValue(object obj, object value)
		{
			_pd.SetValue(obj, value);
		}

		#endregion
	}

	#endregion //PropertyDescriptorValueAccessor class


	// JJD 10/27/11 - TFS92815 - Added
	#region KnownTypeValueAccessor
	
	// JJD 10/27/11 - TFS92815
	// Special case known types (e.g. string, int, double, dateTime etc.). For these types
	// we expose a single pseudo read-only property named 'Value' which return the item itself
	internal class KnownTypeValueAccessor : IItemPropertyValueAccessor
	{
		#region Private Members

		private Type _type;

		#endregion //Private Members

		#region Constants

		internal const string PropertyName = "Value";

		#endregion //Constants	
    
		#region Constructor

		internal KnownTypeValueAccessor(Type type)
		{
			_type = type;
		}

		#endregion //Constructor

		#region Methods

		#region IsValuePropertyName

		internal static bool IsValuePropertyName(string propName)
		{
			return propName != null && StringComparer.InvariantCultureIgnoreCase.Compare(propName, KnownTypeValueAccessor.PropertyName) == 0;
		}

		#endregion //IsValuePropertyName

		#endregion //Methods	
        
		#region IItemPropertyValueAccessor Members

		public string Name { get { return KnownTypeValueAccessor.PropertyName; } }

		public Type PropertyType { get { return _type; } }

		public object GetValue(object obj)
		{
			return obj;
		}

		public void SetValue(object obj, object value)
		{
			throw new NotSupportedException(SRUtil.GetString("ValuePropertyIsReadOnly", _type));
		}

		#endregion
	}

	#endregion //KnownTypeValueAccessor
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