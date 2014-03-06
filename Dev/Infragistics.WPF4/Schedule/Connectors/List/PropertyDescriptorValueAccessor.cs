using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using Infragistics.Windows.Internal;



namespace Infragistics.Controls.Schedules
{

	internal class PropertyDescriptorValueAccessorFactory
	{
		private IEnumerable _list;
		private PropertyDescriptorProvider _provider;
		private PropertyDescriptorCollection _props;

		internal PropertyDescriptorValueAccessorFactory( IEnumerable list )
		{
			_list = list;
		}

		internal PropertyDescriptorValueAccessor CreateHelper( string fieldName, ConverterInfo converterInfo )
		{
			PropertyDescriptor propertyDescriptor = GetPropertyDescriptor( fieldName );
			if ( null != propertyDescriptor )
				return new PropertyDescriptorValueAccessor( propertyDescriptor, converterInfo );

			return null;
		}

		private PropertyDescriptor GetPropertyDescriptor( string fieldName )
		{
			if ( null == _provider )
			{
				_provider = PropertyDescriptorProvider.CreateProvider( null, _list );
				_props = null != _provider ? _provider.GetProperties( ) : null;
			}

			return null != _props ? _props[fieldName] : null;
		}
	}

	internal class PropertyDescriptorValueAccessor : ConvertedFieldValueAccessorBase
	{
		private PropertyDescriptor _propertyDescriptor;

		internal PropertyDescriptorValueAccessor( PropertyDescriptor propertyDescriptor, ConverterInfo converterInfo )
			: base( converterInfo )
		{
			ScheduleUtilities.ValidateNotNull( propertyDescriptor );

			_propertyDescriptor = propertyDescriptor;
		}

		protected override Type UnderlyingValueType
		{
			get
			{
				return _propertyDescriptor.PropertyType;
			}
		}

		protected override object GetValueRaw( object dataItem )
		{
			return _propertyDescriptor.GetValue( dataItem );
		}

		protected override void SetValueRaw( object dataItem, object value )
		{
			_propertyDescriptor.SetValue( dataItem, value );
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