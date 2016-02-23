using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.Web_Service_Post
{
    public class WebServicePostViewModel : CustomToolWithRegionBase, IWebServicePostViewModel
    {
        public WebServicePostViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public WebServicePostViewModel(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem, regions)
        {
        }

        public WebServicePostViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow, IList<IToolRegion> regions)
            : base(modelItem, showExampleWorkflow, regions)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
        }

        [ExcludeFromCodeCoverage]
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            return null;
        }

        #endregion

        #region Implementation of IWebServicePostViewModel

        public IOutputsToolRegion OutputsRegion { get; set; }
        public IWebGetInputArea InputArea { get; set; }
        public ISourceToolRegion<IWebServiceSource> SourceRegion { get; set; }
        public bool GenerateOutputsVisible { get; set; }

        public IWebService ToModel()
        {
            return null;
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
        }

        public void SetDisplayName(string displayName)
        {
        }

        #endregion
    }
}
