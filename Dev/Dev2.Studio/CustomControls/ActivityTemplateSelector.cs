using Dev2.Activities.Designers2.Core;
using Dev2.Studio.ActivityDesigners;
using System;
using System.Activities.Presentation.View;
using System.Windows;
using System.Windows.Controls;

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
            }
            return null;
        }
    }
}
