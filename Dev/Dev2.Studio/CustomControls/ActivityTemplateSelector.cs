﻿using Dev2.Activities;
using Dev2.Activities.Designers2.Core;
using Dev2.Studio.ActivityDesigners;
using Dev2.ViewModels.Merge;
using System;
using System.Windows;
using System.Windows.Controls;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.CustomControls
{
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var mergeVm = item as MergeToolModel;
            var vm = mergeVm?.ActivityDesignerViewModel;
            if (vm == null)
            {
                return null;
            }

            string assemblyName;
            string typeName;
            ActivityDesignerTemplate template;
            ActivityDesignerHelper.DesignerAttributes.TryGetValue(vm.ModelItem.ItemType, out var designerType);
            if (designerType != null)
            {
                template = GetContainerActivityTemplate(designerType, vm);
                if (template != null)
                {
                    return ReturnDataTemplate(template, vm);
                }
                assemblyName = designerType.Assembly.FullName;
                typeName = designerType.Namespace + ".Large";
            }
            else
            {
                if (vm.ModelItem?.ItemType == typeof(DsfSwitch))
                {
                    assemblyName = System.Reflection.Assembly.GetAssembly(typeof(Activities.Designers2.Switch.ConfigureSwitch)).FullName;
                    typeName = "Dev2.Activities.Designers2.Switch.ConfigureSwitch";
                    var instance = Activator.CreateInstance(assemblyName, typeName);

                    if (instance.Unwrap() is UserControl userControl)
                    {
                        userControl.DataContext = vm;
                        return TemplateGenerator.CreateDataTemplate(() => userControl);
                    }
                }
                else if (vm.ModelItem?.ItemType == typeof(DsfDecision) || vm.ModelItem?.ItemType ==typeof( DsfFlowDecisionActivity))
                {
                    assemblyName = System.Reflection.Assembly.GetAssembly(typeof(Activities.Designers2.Decision.Large)).FullName;
                    typeName = "Dev2.Activities.Designers2.Decision.Large";
                }
                else
                {
                    return null;
                }
            }
            var inst = Activator.CreateInstance(assemblyName, typeName);
            template = inst.Unwrap() as ActivityDesignerTemplate;
            return template != null ? ReturnDataTemplate(template, vm) : null;
        }

        private static DataTemplate ReturnDataTemplate(ActivityDesignerTemplate template, ActivityDesignerViewModel vm)
        {
            template.DataContext = vm;
            return TemplateGenerator.CreateDataTemplate(() => template);
        }

        private static ActivityDesignerTemplate GetContainerActivityTemplate(Type designerType, ActivityDesignerViewModel vm)
        {
            Type type;
            switch (designerType.Name)
            {
                case "SequenceDesigner":
                    type = typeof(Activities.Designers2.Sequence.Large);
                    break;
                case "SelectAndApplyDesigner":
                    type = typeof(Activities.Designers2.SelectAndApply.Large);
                    vm.ModelItem.Properties["ApplyActivityFunc"]?.SetValue(null);
                    break;
                case "ForeachDesigner":
                    type = typeof(Activities.Designers2.Foreach.Large);
                    vm.ModelItem.Properties["DataFunc"]?.SetValue(null);
                    break;
                default:
                    return null;
            }
            var insta = Activator.CreateInstance(type, new object[] { true });
            var template = insta as ActivityDesignerTemplate;
            return template;
        }
    }
}
