using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

namespace Infragistics.Windows.Themes
{
	#region StaticPropertyResourceKey class

	/// <summary>
	/// A resource key that is intended to be exposed as a static readonly property or field.
	/// </summary>
	[TypeConverter(typeof(StaticPropertyResourceKeyConverter))]
	public sealed class StaticPropertyResourceKey : ResourceKey
	{
		#region Private Members

		private Type _type;
		private string _propertyName;





		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the StaticPropertyResourceKey class.
		/// </summary>
		/// <param name="type">The type of the class that exposes a static read-only property or field of type ResourceKey.</param>
		/// <param name="propertyName">The name of the static read-only property or field (of type ResourceKey) exposed by type.</param>
		/// <exception cref="ArgumentNullException">Type is null</exception>
		/// <exception cref="ArgumentNullException">propertyName is null</exception>
		public StaticPropertyResourceKey(Type type, string propertyName)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (propertyName == null)
				throw new ArgumentNullException("propertyName");

			this._type = type;
			this._propertyName = propertyName;

            // JJD 4/6/10
            // Added dictionary to hold types that expose static resource keys
            StaticPropertyResourceKeyConverter.CacheType(type);



#region Infragistics Source Cleanup (Region)
































































#endregion // Infragistics Source Cleanup (Region)


		}

		#endregion //Constructor

		#region Base class overrides

			#region Assembly property

		/// <summary>
		/// Gets an assembly object that indicates which assembly's dictionary to look in for the value associated with this key. 
		/// </summary>
		public override Assembly Assembly
		{
			get { return this._type.Assembly; }
		}

			#endregion //Assembly property

			#region ToString method

		/// <summary>
		/// Returns a string that represents this object.
		/// </summary>
		/// <returns>A string that represents this object.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this._type.Name);
			sb.Append(".");
			sb.Append(this._propertyName);

			return sb.ToString();
		}

			#endregion //ToString method

		#endregion //Base class overrides

		#region Properties

			#region Type

		/// <summary>
		/// Returns the type of the class that exposes a static read-only property or field of type ResourceKey (read-only).
		/// </summary>
		/// <seealso cref="PropertyName"/>
		public Type Type
		{
			get
			{
				return this._type;
			}
		}

			#endregion //Type

			#region PropertyName

		/// <summary>
		/// Returns the name of the static read-only property or field (of type ResourceKey) exposed by type (read-only).
		/// </summary>
		/// <seealso cref="Type"/>
		public string PropertyName
		{
			get
			{
				return this._propertyName;
			}
		}

			#endregion //PropertyName

		#endregion //Properties
	}

	#endregion //StaticPropertyResourceKey class

	#region StaticPropertyResourceKeyConverter class

	internal class StaticPropertyResourceKeyConverter : TypeConverter
	{
        // JJD 4/6/10
        // Added dictionary to hold types that expose static resource keys
        private static Dictionary<string, Type> s_typesWithResourceKeys = new Dictionary<string, Type>();
        private static object s_Lock = new object();

        // JJD 4/6/10
        // Added dictionary to hold types that expose static resource keys
        internal static void CacheType(Type type)
        {

            // JJD 4/6/10
            // Added dictionary to hold types that expose static resource keys
            lock (s_Lock)
            {
                s_typesWithResourceKeys[type.Name] = type;
            }
        }


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException("sourceType");

            if (sourceType == typeof(string))
                return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if ((destinationType == typeof(MarkupExtension)) &&
				(context is IValueSerializerContext))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
            string str = value as string;

            // JJD 4/6/10
            // If the passed in value is a string see if it is in the format
            // 'TypeName.StaticPropertyName'.
            if (str != null)
            {
                string[] sections = str.Split('.');

                // JJD 4/6/10
                // We only support 2 section strings
                if (sections.Length == 2)
                {
                    Type type;

                    lock (s_Lock)
                    {
                        // JJD 4/6/10
                        // Check if the first section is the name of a type that we have cached
                        if (s_typesWithResourceKeys.TryGetValue(sections[0], out type))
                        {
                            // JJD 4/6/10
                            // First check for a static field
                            FieldInfo fldInfo = type.GetField(sections[1]);

                            if (fldInfo != null)
                                return fldInfo.GetValue(null);

                            // JJD 4/6/10
                            // Next check for a static property
                            PropertyInfo pinfo = type.GetProperty(sections[1]);

                            if (pinfo != null)
                            {
                                MethodInfo mi = pinfo.GetGetMethod();

                                if (mi != null)
                                    return mi.Invoke(null, null);
                            }

                            throw new FormatException(SR.GetString("LE_StringFormatException_2", type.FullName, sections[1]));
                        }
                        else
                        {
                            throw new FormatException(SR.GetString("LE_StringFormatException_3", sections[0]));
                        }
                    }
                }
                else
                {
                    throw new FormatException(SR.GetString("LE_StringFormatException_1", str));
                }
            }

            return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if (destinationType == typeof(MarkupExtension))
			{
				StaticPropertyResourceKey key = value as StaticPropertyResourceKey;

				if (key != null)
				{
					IValueSerializerContext vsc = context as IValueSerializerContext;

					if (vsc != null)
					{
						ValueSerializer serializer = vsc.GetValueSerializerFor(typeof(Type));

						if (serializer != null)
						{
							StringBuilder sb = new StringBuilder();
							sb.Append(serializer.ConvertToString(key.Type, vsc));
							sb.Append(".");
							sb.Append(key.PropertyName);
							return new StaticExtension(sb.ToString());
						}
					}
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	#endregion //StaticPropertyResourceKeyConverter class
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