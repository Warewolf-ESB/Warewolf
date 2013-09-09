// FIX ME
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Dev2.Studio.Core.ViewModels;
//using Unlimited.Applications.BusinessDesignStudio.Activities;
//using System.Activities.Statements;
//using System.Activities.Presentation.Services;
//using System.Activities.Presentation.Model;
//using Dev2.Studio.Core.Interfaces;


//// This class does not agree with the integration server because of library issues on the server
////namespace Dev2.Integration.Tests.Mocks {
////    class ModelItemCreator {

////        public IWorkflowDesignerViewModel WorkflowDesignerViewModel;

////        public ModelItemCreator(IWorkflowDesignerViewModel wd) {
////            WorkflowDesignerViewModel = wd;
////        }

////        public IWebActivity CreateWebActivity(IWebActivityFactory factory, IResourceModel resourceModel) {
////            Type userInterfaceType = null;
////            userInterfaceType = typeof(DsfWebPageActivity);
////            var modelService = WorkflowDesignerViewModel.wfDesigner.Context.Services.GetService<ModelService>();

////            Flowchart fc = (Flowchart)modelService.Root.Content.ComputedValue;
////            FlowStep fs = new FlowStep();
////            dynamic wsa = Activator.CreateInstance(userInterfaceType);


////            fs.Action = wsa;
////            fc.StartNode = fs;

////            modelService.Root.Properties["Implementation"].Value.Properties["Nodes"].Collection.Add(fs);
////            ModelItem wsmodelitem = modelService.Root.Properties["Implementation"].Value.Properties["Nodes"].Collection.Last();
////            ModelItem amodelitem = wsmodelitem.Properties["Action"].Value;

////            IWebActivity webActivity = factory.CreateWebActivity(amodelitem, resourceModel, amodelitem.Properties["DisplayName"].ComputedValue.ToString());
////            webActivity.ResourceModel = resourceModel;
////            return webActivity;
////        }
////    }
////}
