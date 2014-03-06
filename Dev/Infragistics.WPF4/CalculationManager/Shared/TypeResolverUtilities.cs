using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media;

namespace Infragistics
{
	internal class TypeResolverUtilities
	{
		#region GetKnownType

		internal static Type GetKnownType(string value)
		{
			string lower = value.ToLowerInvariant();
			switch (value.ToLowerInvariant())
			{
				case "string":
					return typeof(string);
				case "bool":
				case "boolean":
					return typeof(bool);
				case "color":
					return typeof(Color);
				case "datetime":
					return typeof(DateTime);
				case "decimal":
					return typeof(decimal);
				case "double":
					return typeof(double);
				case "float":
				case "single":
					return typeof(float);
				case "int16":
				case "short":
					return typeof(Int16);
				case "int":
				case "int32":
					return typeof(Int32);
				case "int64":
				case "long":
					return typeof(Int64);
				case "uint16":
				case "ushort":
					return typeof(UInt16);
				case "uint":
				case "uint32":
					return typeof(UInt32);
				case "uint64":
				case "ulong":
				return typeof(UInt64);
				case "byte":
					return typeof(byte);
				case "sbyte":
					return typeof(sbyte);
				case "char":
					return typeof(char);
				case "bool?":
				case "boolean?":
				case "nullable<bool>":
				case "nullable<boolean>":
					return typeof(bool?);
				case "color?":
				case "nullable<color>":
					return typeof(Color?);
				case "datetime?":
				case "nullable<datetime>":
					return typeof(DateTime?);
				case "decimal?":
				case "nullable<decimal>":
					return typeof(decimal?);
				case "double?":
				case "nullable<double>":
					return typeof(double?);
				case "float?":
				case "single?":
				case "nullable<float>":
				case "nullable<single>":
					return typeof(float?);
				case "int16?":
				case "nullable<int16>":
				case "short?":
				case "nullable<short>":
				return typeof(Int16?);
				case "int?":
				case "int32?":
				case "nullable<int>":
				case "nullable<int32>":
					return typeof(Int32?);
				case "int64?":
				case "nullable<int64>":
				case "long?":
				case "nullable<long>":
					return typeof(Int64?);
				case "uint16?":
				case "nullable<uint16>":
				case "ushort?":
				case "nullable<ushort>":
					return typeof(UInt16?);
				case "uint?":
				case "uint32?":
				case "nullable<uint>":
				case "nullable<uint32>":
					return typeof(UInt32?);
				case "uint64?":
				case "nullable<uint64>":
				case "ulong?":
				case "nullable<ulong>":
					return typeof(UInt64?);
				case "byte?":
				case "nullable<byte>":
					return typeof(byte?);
				case "sbyte?":
				case "nullable<sbyte>":
					return typeof(sbyte?);
				case "char?":
				case "nullable<char>":
					return typeof(char?);
			}

			return null;
		}

		#endregion //GetKnownType

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

		#region ResolveType

		internal static Type ResolveType(object owner, Type explicitType, string typeName, Type fallbackType, string unknownTypeResourceId)
		{
			CoreUtilities.ValidateNotEmpty(unknownTypeResourceId, "unknownTypeResourceId");

			if (explicitType != null)
				return explicitType;

			Type type = null;
			if (!string.IsNullOrWhiteSpace(typeName))
			{
				type = TypeResolverUtilities.GetKnownType(typeName);

				if (type == null)
				{
					type = Type.GetType(typeName);

					if (type == null)
					{
						if (Debugger.IsAttached && Debugger.IsLogging())
						{
							Debugger.Log(40, "Global", TypeResolverUtilities.GetString("UnknownTypeName", typeName, owner) + Environment.NewLine);
						}
					}
				}
			}

			return type ?? fallbackType;
		}

		#endregion //ResolveType
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