using System;
using System.Collections;
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
        IWarewolfType Designer { get; }

        /// <summary>
        /// Server activity that this will instantiate
        /// </summary>
        IWarewolfType Activity { get; }
        
        /// <summary>
        /// Name of tool as per toolbox
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Icon that will be displayed
        /// </summary>
        string Icon { get; }
        /// <summary>
        /// Version as per dll
        /// </summary>
        Version Version { get; }
        /// <summary>
        /// Help text for help window
        /// </summary>
        //IHelpDescriptor Helpdescriptor { get; }
        
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
        string IconUri { get; }
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
        User,
    }
}