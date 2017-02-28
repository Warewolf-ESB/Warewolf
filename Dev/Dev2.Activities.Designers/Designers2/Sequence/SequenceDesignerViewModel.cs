/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
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
        bool _addedFromDesignSurface;

        public SequenceDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            dynamic mi = ModelItem;
            ModelItemCollection activities = mi.Activities;
            activities.CollectionChanged += ActivitiesOnCollectionChanged;
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Flow_Sequence;
        }

        void ActivitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                dynamic mi = ModelItem;
                ModelItemCollection activities = mi.Activities;
                ModelItem.SetProperty("Activities", activities);
            }
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

                if (test != null && !_addedFromDesignSurface)
                {
                    if (test.ItemType != typeof(System.Activities.Statements.Sequence) && test.ItemType != typeof(DsfActivity))
                    {
                        dynamic mi = ModelItem;
                        ModelItemCollection activitiesCollection = mi.Activities;
                        activitiesCollection.Insert(activitiesCollection.Count, test);
                    }
                }
                if (test != null)
                {
                    _addedFromDesignSurface = false;
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
                if (activityNames != null)
                {
                    var fullListOfNames = activityNames.Select(item => item.DisplayName).ToList();
                    if (fullListOfNames.Count <= 4)
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
            if (dataObject != null && (dataObject.GetDataPresent(GlobalConstants.ExplorerItemModelFormat) || dataObject.GetDataPresent(GlobalConstants.UpgradedExplorerItemModelFormat)))
            {
                var explorerItemModel = dataObject.GetData(GlobalConstants.UpgradedExplorerItemModelFormat);
                Guid envId = new Guid();
                Guid resourceId = new Guid();

                if (explorerItemModel == null)
                {
                    return false;
                }
                IExplorerItemViewModel itemModel = explorerItemModel as IExplorerItemViewModel;
                if (itemModel != null)
                {
                    if (itemModel.Server != null)
                        envId = itemModel.Server.EnvironmentID;
                    resourceId = itemModel.ResourceId;
                }

                try
                {
                    IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == envId);
                    var resource = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);

                    if (resource != null)
                    {
                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, true);
                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                        if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                        {
                            dynamic mvm = Application.Current.MainWindow.DataContext;
                            if (mvm != null && mvm.ActiveItem != null)
                            {
                                WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, mvm.ActiveItem.Environment);
                            }
                        }

                        ModelItem modelItem = ModelItemUtils.CreateModelItem(d);
                        if (modelItem != null)
                        {
                            dynamic mi = ModelItem;
                            ModelItemCollection activitiesCollection = mi.Activities;
                            activitiesCollection.Insert(activitiesCollection.Count, d);
                            return true;
                        }
                    }
                }
                catch (RuntimeBinderException e)
                {
                    Dev2Logger.Error(e);
                }
            }
            return false;
        }

        public bool DoDrop(IDataObject dataObject)
        {
            var formats = dataObject.GetFormats();
            if (!formats.Any())
            {
                return false;
            }
            dynamic mi = ModelItem;
            ModelItemCollection activitiesCollection = mi.Activities;
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
            if (!string.IsNullOrEmpty(modelItemString))
            {
                var objectData = dataObject.GetData(modelItemString);
                var data = objectData as List<ModelItem>;
                if (data != null && data.Count >= 1)
                {
                    foreach (var item in data)
                    {
                        activitiesCollection.Insert(activitiesCollection.Count, item);
                    }
                    _addedFromDesignSurface = true;
                    return true;
                }
            }
            return SetModelItemForServiceTypes(dataObject);
        }

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
