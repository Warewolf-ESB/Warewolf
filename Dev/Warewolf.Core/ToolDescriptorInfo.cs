using System;
using System.Windows;
using System.Windows.Media;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
    public class ToolDescriptorInfo : Attribute
    {
        static ResourceDictionary Resources;
        static ToolDescriptorInfo()
        {
            
            Resources =(ResourceDictionary)Application.LoadComponent(new Uri("/Warewolf.Studio.Themes.Luna;component/Images.xaml",
                UriKind.RelativeOrAbsolute));
        }
        // ReSharper disable once TooManyDependencies
        public ToolDescriptorInfo(string iconName, string name, ToolType toolType, string id, string assemblyname, string version ,string path, string category, string iconUri)
        {
            IconUri = iconUri;
            Category = category;
            Id = Guid.Parse(id);
            Designer = new WarewolfType(assemblyname,Version.Parse(version),path);
            ToolType = toolType;
            Name = name;
            Icon = iconName;
    
        }

        DrawingImage GetResourceFromString(string iconName)
        {
            return (DrawingImage)Resources[iconName];
        }

        public string Icon { get; private set; }

        public string Name { get; private set; }

        public ToolType ToolType { get; private set; }

        public Guid Id { get; private set; }

        public IWarewolfType Designer { get; private set; }
        public string Category { get; private set; }
        public string IconUri { get; private set; }
    }
}