using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IOutputsToolRegion : IToolRegion
    {

        ICollection<IServiceOutputMapping> Outputs { get; set; }
        bool OutputMappingEnabled { get; set; }
        bool IsOutputsEmptyRows { get; }
        string RecordsetName { get; set; }
        IOutputDescription Description { get; set; }
    }


    public interface IWebServiceGetViewModel
    {
        IOutputsToolRegion OutputsRegion { get; set; }
        IWebGetInputArea InputArea { get; set; }
        ISourceToolRegion<IWebServiceSource> SourceRegion { get; set; }
        bool GenerateOutputsVisible { get; set; }
       // bool TestSuccessful { get; set; }

        IWebService ToModel();

        void ErrorMessage(Exception exception, bool hasError);

        void SetDisplayName(string displayName);
    }
}