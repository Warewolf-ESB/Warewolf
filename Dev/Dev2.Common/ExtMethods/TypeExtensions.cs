using System;

namespace Dev2.Common.ExtMethods
{
    /// <summary>
    /// Used to extract primitive types out for plugins ;)
    /// </summary>
    public static class TypeExtensions
    {
        public static Type GetTypeFromSimpleName(string typeName)
        {
            if(typeName == null)
                throw new ArgumentNullException("typeName");

            bool isArray = false, isNullable = false;

            if(typeName.IndexOf("[]", StringComparison.Ordinal) != -1)
            {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]", StringComparison.Ordinal), 2);
            }

            if(typeName.IndexOf("?", StringComparison.Ordinal) != -1)
            {
                isNullable = true;
                typeName = typeName.Remove(typeName.IndexOf("?", StringComparison.Ordinal), 1);
            }

            typeName = typeName.ToLower();

            // remove any system. stuff ;)
            typeName = typeName.Replace("system.", "");

            string parsedTypeName = null;
            switch(typeName)
            {
                case "bool":
                case "boolean":
                    parsedTypeName = "System.Boolean";
                    break;
                case "byte":
                    parsedTypeName = "System.Byte";
                    break;
                case "char":
                    parsedTypeName = "System.Char";
                    break;
                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;
                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;
                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;
                case "double":
                    parsedTypeName = "System.Double";
                    break;
                case "float":
                    parsedTypeName = "System.Single";
                    break;
                case "int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;
                case "int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;
                case "int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;
                case "object":
                    parsedTypeName = "System.Object";
                    break;
                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;
                case "string":
                    parsedTypeName = "System.String";
                    break;
                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;
                case "uint16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;
                case "uint32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;
                case "uint64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
                case "guid":
                    parsedTypeName = "System.Guid";
                    break;
            }

            if(parsedTypeName != null)
            {
                if(isArray)
                    parsedTypeName = parsedTypeName + "[]";

                if(isNullable)
                    parsedTypeName = String.Concat("System.Nullable`1[", parsedTypeName, "]");
            }
            else
                parsedTypeName = typeName;

            // Expected to throw an exception in case the type has not been recognized.
            return Type.GetType(parsedTypeName);
        }
    }
}
