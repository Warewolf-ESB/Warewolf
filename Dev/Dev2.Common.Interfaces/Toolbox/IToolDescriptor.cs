using System;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolDescriptor
    {
        Type Designer { get; }
        
        Type Model { get; }
        
        Type Activity { get; }
        
        string Name { get; }
        
        string Icon { get; }
        
        Version Version { get; }
        
        IHelpDescriptor Helpdescriptor { get; }
        
        bool IsSupported{get;}
        
        string Category { get; }
        
        ToolType ToolType{get;}
    }

    public enum ToolType
    {
        Native,
        User,
    }

    public interface IHelpDescriptor
    {
        string Name { get; }
        string Description { get; }
        string Icon { get; }
    }
}