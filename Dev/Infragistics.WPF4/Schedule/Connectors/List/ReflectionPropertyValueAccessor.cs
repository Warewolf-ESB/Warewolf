using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;






namespace Infragistics.Controls.Schedules

{
	internal abstract class ConvertedFieldValueAccessorBase : IFieldValueAccessor
	{
		private ConverterInfo _converterInfo;

		internal ConvertedFieldValueAccessorBase( ConverterInfo converterInfo )
		{
			_converterInfo = converterInfo;
		}

		protected abstract object GetValueRaw( object dataItem );

		protected abstract void SetValueRaw( object dataItem, object value );

		protected abstract Type UnderlyingValueType
		{
			get;
		}

		#region IFieldValueAccessor Members

		public object GetValue( object dataItem )
		{
			try
			{
				object val = this.GetValueRaw( dataItem );

				if ( null != _converterInfo )
					val = _converterInfo.Convert( val );

				return val;
			}
			catch ( Exception )
			{
				
				return null;
			}
		}

		public void SetValue( object dataItem, object newValue )
		{
			Type propertyType = this.UnderlyingValueType;
			object valueToSet = newValue;

			if ( null != valueToSet && !propertyType.IsInstanceOfType( valueToSet ) )
			{
				if ( null != _converterInfo )
					valueToSet = _converterInfo.ConvertBack( valueToSet, propertyType );
			}

			try
			{
				this.SetValueRaw( dataItem, valueToSet );
			}
			catch
			{
				
			}
		}

		#endregion
	}

	internal class ReflectionPropertyValueAccessorFactory
	{
		private LinqQueryManager _linqQueryManager;

		internal ReflectionPropertyValueAccessorFactory( LinqQueryManager linqQueryManager )
		{
			_linqQueryManager = linqQueryManager;
		}

		internal ReflectionPropertyValueAccessor CreateHelper( string fieldName, ConverterInfo converterInfo )
		{
			PropertyInfo propInfo = null != _linqQueryManager ? _linqQueryManager.GetProperty( fieldName ) : null;

			if ( null != propInfo )
				return new ReflectionPropertyValueAccessor( propInfo, converterInfo );

			return null;
		}
	}

	internal class ReflectionPropertyValueAccessor : ConvertedFieldValueAccessorBase
	{
		private PropertyInfo _propInfo;

		// SSP 4/21/11 TFS73037 - Performance
		// 
		private Func<object, object> _accessorFunction;

		internal ReflectionPropertyValueAccessor( PropertyInfo propInfo, ConverterInfo converterInfo )
			: base( converterInfo )
		{
			ScheduleUtilities.ValidateNotNull( propInfo );

			_propInfo = propInfo;

			// SSP 4/21/11 TFS73037 - Performance
			// 
			_accessorFunction = this.Compile( );
		}

		protected override Type UnderlyingValueType
		{
			get 
			{
				return _propInfo.PropertyType;
			}
		}

		// SSP 4/21/11 TFS73037 - Performance
		// 
		private Func<object, object> Compile( )
		{
			try
			{
				Type itemType = _propInfo.DeclaringType;
				Type propType = _propInfo.PropertyType;
				if ( null != itemType && null != propType )
				{
					ParameterExpression o = Expression.Parameter( typeof( object ), "o" );
					Expression item = Expression.TypeAs( o, itemType );
					Expression p = Expression.Property( item, _propInfo );

					if ( propType.IsValueType )
						p = Expression.TypeAs( p, typeof( object ) );

					Expression<Func<object, object>> f = Expression.Lambda<Func<object, object>>( p, o );

					return f.Compile( );
				}
			}
			catch
			{
				Debug.Assert( false );
			}

			return null;
		}

		protected override object GetValueRaw( object dataItem )
		{
			// SSP 4/21/11 TFS73037 - Performance
			// If we have compiled function then use that for better performance.
			// 
			if ( null != _accessorFunction )
			{
				try
				{
					return _accessorFunction( dataItem );
				}
				catch
				{
					// If there's an exception then null out compiled function so we don't use it again and
					// repeat exceptions. Fallback to prop info's GetValue below.
					// 
					_accessorFunction = null;
					Debug.Assert( false );
				}
			}

			return _propInfo.GetValue( dataItem, null );
		}

		protected override void SetValueRaw( object dataItem, object value )
		{
			_propInfo.SetValue( dataItem, value, null );
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