using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;

namespace Dev2.Studio.Webs.Callbacks
{
    public class SaveNewWorkflowCallbackHandler
       : WebsiteCallbackHandler
    {
        #region Fields

        private readonly IContextualResourceModel _resourceModel;
        private bool _addToTabManager;

        #endregion

        public SaveNewWorkflowCallbackHandler(IContextualResourceModel resourceModel)
            : this(EnvironmentRepository.Instance,resourceModel,true)
        {
        }

        public SaveNewWorkflowCallbackHandler(IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(currentEnvironmentRepository)
        {
            _addToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }
   

        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            string resName = jsonObj.resourceName;
            string resCat = jsonObj.resourcePath;
            bool isClone = _resourceModel.IsDuplicate;             

            if (_resourceModel != null)
            {
                _resourceModel.IsNewWorkflow = false;
                if(_resourceModel.ResourceName != null && _resourceModel.ID != Guid.Empty)
                {
                    EventAggregator.Publish(new SaveResourceMessage(_resourceModel, true, false));   
                }

                IContextualResourceModel newResourceModel = ResourceModelFactory.CreateResourceModel(_resourceModel.Environment, "Workflow", resName);
                newResourceModel.Category = resCat;
                newResourceModel.ResourceName = resName;
                newResourceModel.ServiceDefinition = _resourceModel.ServiceDefinition; 
                newResourceModel.ID = Guid.NewGuid();

                #region Changes for Duplicate Menu Option ;)

                // do some low level manip ;)
                if(newResourceModel.ServiceDefinition != null)
                {
                    XElement xe = XElement.Parse(newResourceModel.ServiceDefinition);

                    // set Name attribute correctly ;)
                    var name = xe.Attributes().FirstOrDefault(c => c.Name == "Name");
                    if (name != null)
                    {
                        name.Value = resName;
                    }

                    // set Display Name attribute correctly
                    var dName = xe.Elements().FirstOrDefault(c => c.Name == "DisplayName");

                    if (dName != null)
                    {
                        dName.Value = resName;
                    }

                    // set category correctly ;)
                    var cat = xe.Elements().FirstOrDefault(c => c.Name == "Category");

                    if (cat != null)
                    {
                        cat.Value = resCat;
                    }

                    // adjust id
                    var id = xe.Attributes().FirstOrDefault(c => c.Name == "ID");

                    if (id != null && environmentModel.ID == Guid.Empty)
                    {

                        // ok, we need to assign a new GUID?!
                        id.Value = Guid.NewGuid().ToString();

                        // finally, adjust the version
                        var ver = xe.Attributes().FirstOrDefault(c => c.Name == "Version");

                        if (ver != null)
                        {
                            ver.Value = "1.0";
                        }

                        // its new does it go to tab?
                        _addToTabManager = true;
                    }

                    // now push it back ;)
                    newResourceModel.ServiceDefinition = xe.ToString(SaveOptions.None);
                }

                // The line below is silly, what if the user has data that matches the name?!?!
                //newResourceModel.WorkflowXaml = _resourceModel.WorkflowXaml.Replace(_resourceModel.DisplayName, resName);

                string xClass = String.Format("x:Class=\"{0}\"", _resourceModel.DisplayName);
                string xClassR = String.Format("x:Class=\"{0}\"", resName);

                if(_resourceModel.WorkflowXaml != null)
                {
                    newResourceModel.WorkflowXaml = _resourceModel.WorkflowXaml.Replace(xClass, xClassR);
                }

                string dNameF = String.Format("DisplayName=\"{0}\"", _resourceModel.DisplayName);
                string dNameR = String.Format("DisplayName=\"{0}\"", resName);
                if(_resourceModel.WorkflowXaml != null)
                {
                    newResourceModel.WorkflowXaml = newResourceModel.WorkflowXaml.Replace(dNameF, dNameR);
                }

                #endregion

                newResourceModel.DataList = _resourceModel.DataList;
                newResourceModel.IsNewWorkflow = false;

                EventAggregator.Publish(new UpdateResourceMessage(newResourceModel));
                
                EventAggregator.Publish(new SaveResourceMessage(newResourceModel, false, _addToTabManager));
                
                if (_addToTabManager)
                {
                    EventAggregator.Publish(new AddWorkSurfaceMessage(newResourceModel));
                }

                if (!isClone)
                {
                EventAggregator.Publish(new RemoveResourceAndCloseTabMessage(_resourceModel));
                EventAggregator.Publish(new RemoveNavigationResourceMessage(_resourceModel));
                NewWorkflowNames.Instance.Remove(_resourceModel.ResourceName);
                }
            }
            Close();
        }

        #endregion
    }
}
