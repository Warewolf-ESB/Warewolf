using Dev2.Activities.Designers2.Core;
using Dev2.Studio.ActivityDesigners;
using System;
using System.Activities.Presentation.View;
using System.Windows;
using System.Windows.Controls;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.CustomControls
{
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vm = item as ActivityDesignerViewModel;
            if (vm != null)
            {
                DesignerView parentContentPane = FindDependencyParent.FindParent<DesignerView>(vm.ModelItem.View);

                ActivityDesignerTemplate template;
                Type designerType;
                ActivityDesignerHelper.DesignerAttributes.TryGetValue(vm.ModelItem.ItemType, out designerType);
                if (designerType != null)
                {
                    var inst = Activator.CreateInstance(designerType.Assembly.FullName, designerType.Namespace + ".Large");
                    template = inst.Unwrap() as ActivityDesignerTemplate;
                    if (template != null)
                    {
                        template.DataContext = item;
                        return TemplateGenerator.CreateDataTemplate(() => template);
                    }
                }
                else
                {
                    var assemblyFullName = "Dev2.Activities.Designers, Version=0.0.6465.12612, Culture=neutral, PublicKeyToken=null";
                    var namespacePath = "Dev2.Activities.Designers2";
                    var type = vm.ModelItem.ItemType;

                    if (type == typeof(DsfFlowDecisionActivity))
                    {
                        var inst = Activator.CreateInstance(assemblyFullName, namespacePath + ".Decision.Large");
                        template = inst.Unwrap() as ActivityDesignerTemplate;
                        if (template != null)
                        {
                            template.DataContext = item;
                            return TemplateGenerator.CreateDataTemplate(() => template);
                        }
                    }
                    if (type == typeof(DsfFlowSwitchActivity))
                    {
                        var inst = Activator.CreateInstance(assemblyFullName, namespacePath + ".Switch.ConfigureSwitch");
                        template = inst.Unwrap() as ActivityDesignerTemplate;
                        if (template != null)
                        {
                            template.DataContext = item;
                            return TemplateGenerator.CreateDataTemplate(() => template);
                        }
                    }
                }
            }
            return null;
        }
    }
}
