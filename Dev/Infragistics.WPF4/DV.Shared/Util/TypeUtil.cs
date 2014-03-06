using System;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Type conversion utility class.
    /// </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// Gets the type converter for the current type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>TypeConverter, or null</returns>
        public static TypeConverter GetTypeConverter(this Type type)
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            TypeConverterAttribute converterAttr = customAttributes != null && customAttributes.Length > 0 ? customAttributes[0] as TypeConverterAttribute : null;
            Type converterType = converterAttr != null ? Type.GetType(converterAttr.ConverterTypeName) : null;

            return converterType != null ? Activator.CreateInstance(converterType) as TypeConverter : null;
        }

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent
        /// to the specified object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>Value as the specified type, or null if the conversion cannot be performed.</returns>
        public static object ChangeType(this Type type, object value)
        {
            if (value == null)
            {
                return value;
            }

            Type valueType = value.GetType();
            Object ret = type.IsAssignableFrom(valueType) ? value : null;

            if (ret == null)
            {
                try
                {
                    ret = Convert.ChangeType(value, type, null);
                }
                catch
                {
                }
            }

            if (ret == null)
            {
                TypeConverter typeConverter = type.GetTypeConverter();

                if (typeConverter != null && typeConverter.CanConvertFrom(valueType))
                {
                    try
                    {
                        ret = typeConverter.ConvertFrom(value);
                    }
                    catch
                    {
                    }
                }
            }

            if (ret == null)
            {
                TypeConverter typeConverter = valueType.GetTypeConverter();

                if (typeConverter != null && typeConverter.CanConvertTo(type))
                {
                    try
                    {
                        ret = typeConverter.ConvertTo(value, type);
                    }
                    catch
                    {
                    }
                }
            }

            return ret;
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