using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.Toolbox;
using Warewolf.Core;
using System;

namespace Dev2.Studio.Core
{
    public static class ToolDescriptionExtentions
    {
        public static IToolDescriptor GetDescriptorFromAttribute(this Type type)
        {
            var isAssignableFrom = !type.IsAssignableFrom(typeof(IDev2Activity));
            if (!isAssignableFrom) return default(IToolDescriptor);
            var info = (ToolDescriptorInfo)type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First();
            var descriptor = new ToolDescriptor(info.Id, info.Designer
                , new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location)
                , info.Name
                , info.Icon
                , type.Assembly.GetName().Version
                , true, info.Category
                , ToolType.Native
                , info.IconUri
                , info.FilterTag
                , info.ResourceToolTip
                , info.ResourceHelpText);
            return descriptor;
        }
    }
}
