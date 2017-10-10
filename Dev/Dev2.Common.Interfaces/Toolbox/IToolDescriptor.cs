using System;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolDescriptor
    {
        Guid Id{get;}
        
        IWarewolfType Designer { get; }
        
        IWarewolfType Activity { get; }
        
        string Name { get; }
        
        string Icon { get; }
        
        Version Version { get; }
        
        bool IsSupported{get;}
        
        string Category { get; }
        
        ToolType ToolType{get;}
        string IconUri { get; }

        string FilterTag { get; set; }
        string ResourceToolTip { get; set; }
        string ResourceHelpText { get; set; }
    }

    public interface IWarewolfType  
    {
        string FullyQualifiedName{get;}
        Version Version { get; }
        string ContainingAssemblyPath { get; }
    }

    public enum ToolType
    {
        Native,
        User
    }
}