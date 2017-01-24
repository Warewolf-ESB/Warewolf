using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase.DotNet
{

    public interface IDotNetViewModel
    {
        ISourceToolRegion<IPluginSource> SourceRegion { get; set; }
        INamespaceToolRegion<INamespaceItem> NamespaceRegion { get; set; }
        IConstructorRegion<IPluginConstructor> ConstructorRegion { get; set; }
        IActionToolRegion<IPluginAction> ActionRegion { get; set; }
        IDotNetInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }
        bool GenerateOutputsVisible { get; set; }
        IPluginService ToModel();
        void ErrorMessage(Exception exception, bool hasError);
        void SetDisplayName(string displayName);
    }

    public interface IDotNetEnhancedViewModel
    {
        ISourceToolRegion<IPluginSource> SourceRegion { get; set; }
        INamespaceToolRegion<INamespaceItem> NamespaceRegion { get; set; }
        IConstructorRegion<IPluginConstructor> ConstructorRegion { get; set; }
        IMethodToolRegion<IPluginAction> MethodRegion { get; set; }
        IDotNetConstructorInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }
        IPluginService ToModel();
        void ErrorMessage(Exception exception, bool hasError);
        void SetDisplayName(string displayName);

    }

    public interface IDotNetInputRegion : IToolRegion
    {
        ICollection<IServiceInput> Inputs { get; set; }
    }

    public interface IDotNetConstructorInputRegion : IToolRegion
    {
        ICollection<IServiceInput> Inputs { get; set; }
    }

    public interface IComViewModel
    {
        ISourceToolRegion<IComPluginSource> SourceRegion { get; set; }
        INamespaceToolRegion<INamespaceItem> NamespaceRegion { get; set; }
        IActionToolRegion<IPluginAction> ActionRegion { get; set; }
        IDotNetInputRegion InputArea { get; set; }
        IOutputsToolRegion OutputsRegion { get; set; }
        bool GenerateOutputsVisible { get; set; }
        IComPluginService ToModel();
        void ErrorMessage(Exception exception, bool hasError);
        void SetDisplayName(string displayName);
    }

    public interface IComInputRegion : IToolRegion
    {
        IList<IServiceInput> Inputs { get; set; }
    }
}
