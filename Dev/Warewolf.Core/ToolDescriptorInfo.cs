using System;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
    public class ToolDescriptorInfo : Attribute
    {

        // ReSharper disable once TooManyDependencies
        public ToolDescriptorInfo(string iconName, string name, ToolType toolType, string id, string assemblyname, string version ,string path, string category, string iconUri, string tag)
        {
            IconUri = iconUri;
            Category = category;
            Id = Guid.Parse(id);
            Designer = new WarewolfType(assemblyname,Version.Parse(version),path);
            ToolType = toolType;
            Name = name;
            Icon = iconName;
            FilterTag = Studio.Resources.Languages.ToolboxSearchTags.ResourceManager.GetString(tag) ?? "";
            ResourceToolTip = Studio.Resources.Languages.Tooltips.ResourceManager.GetString(tag) ?? "";
            ResourceHelpText = Studio.Resources.Languages.HelpText.ResourceManager.GetString(tag) ?? "";
        }

    

        public string Icon { get; private set; }

        public string Name { get; private set; }

        public ToolType ToolType { get; private set; }

        public Guid Id { get; private set; }

        public IWarewolfType Designer { get; private set; }
        public string Category { get; private set; }
        public string IconUri { get; private set; }
        public string FilterTag { get; set; }
        public string ResourceToolTip { get; set; }
        public string ResourceHelpText { get; set; }
    }
}