#pragma warning disable
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
            var mergeVm = item as ToolConflictItem;
            var vm = mergeVm?.ActivityDesignerViewModel;
            if (vm == null)
            {
                return TemplateGenerator.CreateDataTemplate(() => new UserControl());
            }

            string assemblyName;
            string typeName;
            ActivityDesignerTemplate template;
            ActivityDesignerHelper.DesignerAttributes.TryGetValue(vm.ModelItem.ItemType, out var designerType);
            if (designerType != null)
            {
                assemblyName = designerType.Assembly.FullName;
                typeName = GetContainerActivityTypeName(designerType);
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
                else if (vm.ModelItem?.ItemType == typeof(DsfDecision) || vm.ModelItem?.ItemType == typeof(DsfFlowDecisionActivity))
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

        private static string GetContainerActivityTypeName(Type designerType)
        {
            string typeName;
            var name = designerType.Namespace;
            switch (designerType.Name)
            {
                case "SequenceDesigner":
                case "SelectAndApplyDesigner":
                case "ForeachDesigner":
                    typeName = name + ".SmallErrorView";
                    break;
                default:
                    typeName = name + ".Large";
                    break;
            }
            return typeName;
        }
    }
}
