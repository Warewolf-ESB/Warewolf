
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Interfaces;
using Dev2.Models;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;
using Microsoft.CSharp.RuntimeBinder;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.SelectAndApply
{
    public class SelectAndApplyDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        private string _dataSource;
        private string _alias;
        private Activity _applyActivity;

        public SelectAndApplyDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();

        }

        private void SetModelItemProperty(object value, [CallerMemberName]string propName = null)
        {
            ModelItem.SetProperty(propName, value);
        }
        private object GetModelPropertyName([CallerMemberName]string propName = null)
        {
            var propertyValue = ModelItem.GetProperty(propName);
            return propertyValue ?? string.Empty;
        }
        public string DataSource
        {
            get
            {
                _dataSource = GetModelPropertyName().ToString();
                return _dataSource;
            }
            set
            {
                _dataSource = value;
                SetModelItemProperty(value);
                OnPropertyChanged();
            }
        }
        public string Alias
        {
            get
            {
                _alias = GetModelPropertyName().ToString();
                return _alias;
            }
            set
            {
                _alias = value;
                SetModelItemProperty(value);
                OnPropertyChanged();
            }
        }

        public Activity ApplyActivity
        {
            get
            {
                return _applyActivity;
            }
            set
            {
                _applyActivity = value;
                ModelProperty modelProperty = ModelItem.Properties["ApplyActivity"];
                if(modelProperty != null)
                {
                    modelProperty.SetValue(value);
                }
                OnPropertyChanged();
            }
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
            if (!String.IsNullOrEmpty(modelItemString))
            {
                var objectData = dataObject.GetData(modelItemString);
                var data = objectData as List<ModelItem>;
                if (data != null && data.Count >= 1)
                {
                    foreach (var item in data)
                    {
                        activitiesCollection.Insert(activitiesCollection.Count, item);
                    }
                    return true;
                }
            }
            return SetModelItemForServiceTypes(dataObject);
        }

        public bool SetModelItemForServiceTypes(IDataObject dataObject)
        {
            if (dataObject != null && dataObject.GetDataPresent(GlobalConstants.ExplorerItemModelFormat))
            {
                var explorerItemModel = dataObject.GetData(GlobalConstants.ExplorerItemModelFormat);
                try
                {
                    ExplorerItemModel itemModel = explorerItemModel as ExplorerItemModel;
                    if (itemModel != null)
                    {
                        IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == itemModel.EnvironmentId);
                        if (environmentModel != null)
                        {
                            var resource = environmentModel.ResourceRepository.FindSingle(c => c.ID == itemModel.ResourceId) as IContextualResourceModel;

                            if (resource != null)
                            {
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                                d.IconPath = resource.IconPath;
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
                    }
                }
                catch (RuntimeBinderException e)
                {
                    Dev2Logger.Error(e);
                }
            }
            return false;
        }

        public bool MultipleItemsToSequence(IDataObject dataObject)
        {
            if (dataObject != null)
            {
                var formats = dataObject.GetFormats();
                if (!formats.Any())
                {
                    return false;
                }
                var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
                if (!String.IsNullOrEmpty(modelItemString))
                {
                    var objectData = dataObject.GetData(modelItemString);
                    var data = objectData as List<ModelItem>;

                    if (data != null && data.Count > 1)
                    {

                        return true; //This is to short circuit the multiple activities to Sequence re-introduce when we tackle this issue
                        //                        DsfSequenceActivity dsfSequenceActivity = new DsfSequenceActivity();
                        //                        foreach(var item in data)
                        //                        {
                        //                            object currentValue = item.GetCurrentValue();
                        //                            var activity = currentValue as Activity;
                        //                            if(activity != null)
                        //                            {
                        //                                dsfSequenceActivity.Activities.AddMode(activity);
                        //                            }
                        //                        }
                        //                        ModelItem modelItem = ModelItemUtils.CreateModelItem(dsfSequenceActivity);
                        //                        return modelItem;
                    }
                }
            }
            return false;
        }

        // DO NOT bind to these properties - these are here for convenience only!!!


        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
