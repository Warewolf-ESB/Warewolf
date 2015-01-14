using System;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolDescriptor
    {
        /// <summary>
        /// Tool identifier. Originates from containing dll
        /// </summary>
        Guid Id{get;}
        /// <summary>
        /// The type that will be instantiated as the designer
        /// </summary>
        Type Designer { get; }

        /// <summary>
        /// something or the other; //todo: check what this was meant to do in diagram
        /// </summary>
        
        Type Model { get; }

        /// <summary>
        /// Server activity that this will instantiate
        /// </summary>
        Type Activity { get; }
        
        /// <summary>
        /// Name of tool as per toolbox
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Icon that will be displayed
        /// </summary>
        DrawingImage Icon { get; }
        /// <summary>
        /// Version as per dll
        /// </summary>
        Version Version { get; }
        /// <summary>
        /// Help text for help window
        /// </summary>
        IHelpDescriptor Helpdescriptor { get; }
        
        /// <summary>
        /// Is supported locally
        /// </summary>
        bool IsSupported{get;}
        
        /// <summary>
        /// Tool category for toolbox
        /// </summary>
        string Category { get; }
        
        /// <summary>
        /// Native or user
        /// </summary>
        ToolType ToolType{get;}
    }

    public enum ToolType
    {
        Native,
        User,
    }
}