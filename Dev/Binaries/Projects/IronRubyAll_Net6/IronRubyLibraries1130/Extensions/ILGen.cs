using Microsoft.Scripting.Utils;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Scripting.Generation
{
    public static class ILGenExtensions
    {
        internal static string PropertyDoesNotExist => "Property doesn't exist on the provided type";
        public static void EmitPropertyGet(this Microsoft.Scripting.Generation.ILGen ilGen, Type type, string name)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(name, "name");
            PropertyInfo property = type.GetProperty(name);
            ContractUtils.Requires(property != null, "name", PropertyDoesNotExist);
            ilGen.EmitPropertyGet(property);
        }
    }
}
