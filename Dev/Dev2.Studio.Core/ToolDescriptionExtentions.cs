using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.Toolbox;
using Warewolf.Core;

namespace Dev2.Studio.Core
{
    public static class ToolDescriptionExtentions
    {
        public static IToolDescriptor GetDescriptorFromAttribute(this object type)
        {
            var type1 = type.GetType();
            var isAssignableFrom = !type1.IsAssignableFrom(typeof(IDev2Activity));
            if (!isAssignableFrom) return default(IToolDescriptor);
            var info = (ToolDescriptorInfo)type1.GetCustomAttributes(typeof(ToolDescriptorInfo)).First();
            var descriptor = new ToolDescriptor(info.Id, info.Designer
                , new WarewolfType(type1.FullName, type1.Assembly.GetName().Version, type1.Assembly.Location)
                , info.Name
                , info.Icon
                , type1.Assembly.GetName().Version
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
