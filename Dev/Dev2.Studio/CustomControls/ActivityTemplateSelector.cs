#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
