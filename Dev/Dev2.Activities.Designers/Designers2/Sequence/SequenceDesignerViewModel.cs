using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Models;
using Dev2.Providers.Logs;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;
using Microsoft.CSharp.RuntimeBinder;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.Sequence
{
    public class SequenceDesignerViewModel : ActivityDesignerViewModel
    {
        object _smallViewItem;

        public SequenceDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            AddTitleBarLargeToggle();
        }

        public object SmallViewItem
        {
            get
            {
                return _smallViewItem;
            }
            // ReSharper disable ValueParameterNotUsed
            set
            // ReSharper restore ValueParameterNotUsed
            {
                var test = value as ModelItem;

                if(test != null)
                {
                    if(test.ItemType != typeof(System.Activities.Statements.Sequence) && test.ItemType != typeof(DsfActivity))
                    {
                        dynamic mi = ModelItem;
                        ModelItemCollection activitiesCollection = mi.Activities;
                        activitiesCollection.Insert(activitiesCollection.Count, test);
                    }
                }
                _smallViewItem = null;
            }
        }

        public List<String> ActivityNames
        {
            get
            {
                var property = ModelItem.GetProperty("Activities");
                var activityNames = property as Collection<Activity>;
                if(activityNames != null)
                {
                    var fullListOfNames = activityNames.Select(item => item.DisplayName).ToList();
                    if(fullListOfNames.Count() <= 4)
                    {
                        return fullListOfNames.ToList();
                    }
                    var limitedList = fullListOfNames.Take(4).ToList();
                    limitedList.Add("...");
                    return limitedList;
                }
                return new List<string>();
            }
        }

        public bool SetModelItemForServiceTypes(IDataObject dataObject)
        {
            if(dataObject != null && dataObject.GetDataPresent(GlobalConstants.ExplorerItemModelFormat))
            {
                var explorerItemModel = dataObject.GetData(GlobalConstants.ExplorerItemModelFormat);
                try
                {
                    ExplorerItemModel itemModel = explorerItemModel as ExplorerItemModel;
                    if(itemModel != null)
                    {
                        IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == itemModel.EnvironmentId);
                        if(environmentModel != null)
                        {
                            var resource = environmentModel.ResourceRepository.FindSingle(c => c.ID == itemModel.ResourceId) as IContextualResourceModel;

                            if(resource != null)
                            {
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                                d.IconPath = resource.IconPath;
                                if(Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                                {
                                    dynamic mvm = Application.Current.MainWindow.DataContext;
                                    if(mvm != null && mvm.ActiveItem != null)
                                    {
                                        WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, mvm.ActiveItem.Environment);
                                    }
                                }

                                ModelItem modelItem = ModelItemUtils.CreateModelItem(d);
                                if(modelItem != null)
                                {
                                    dynamic mi = ModelItem;
                                    ModelItemCollection activitiesCollection = mi.Activities;
                                    activitiesCollection.Insert(activitiesCollection.Count, d);
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch(RuntimeBinderException e)
                {
                    this.LogError(e);
                }
            }
            return false;
        }

        public bool DoDrop(IDataObject dataObject)
        {
            var formats = dataObject.GetFormats();
            if(!formats.Any())
            {
                return false;
            }
            dynamic mi = ModelItem;
            ModelItemCollection activitiesCollection = mi.Activities;
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
            if(!String.IsNullOrEmpty(modelItemString))
            {
                var objectData = dataObject.GetData(modelItemString);
                var data = objectData as List<ModelItem>;
                if(data != null && data.Count > 1)
                {
                    foreach(var item in data)
                    {
                        activitiesCollection.Insert(activitiesCollection.Count, item);
                    }
                    return true;
                }
            }
            return SetModelItemForServiceTypes(dataObject);
        }

        public override void Validate()
        {
        }
    }
}