using Dev2.Activities;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Studio.ActivityDesigners;
using Dev2.ViewModels.Merge;
using System;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.CustomControls
{
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var mergeVm = item as MergeToolModel;
            var vm = mergeVm?.ActivityDesignerViewModel;
            return vm == null ? null : GetDataTemplate(vm);
        }

        public static DataTemplate GetSelectedDataTemplate(ActivityDesignerViewModel vm)
        {
            return GetDataTemplate(vm);
        }

        private static DataTemplate GetDataTemplate(ActivityDesignerViewModel vm)
        {
            ActivityDesignerHelper.DesignerAttributes.TryGetValue(vm.ModelItem.ItemType, out var designerType);
            var instance = designerType != null ? GetAlteredView(designerType) : ReturnDecisionSwitchTypes(vm);

            if (!(instance?.Unwrap() is ActivityDesignerTemplate template))
            {
                return null;
            }

            template.DataContext = vm;
            return TemplateGenerator.CreateDataTemplate(() => template);
        }

        private static ObjectHandle ReturnDecisionSwitchTypes(ActivityDesignerViewModel vm)
        {
            const string assemblyFullName = "Dev2.Activities.Designers, Version=0.0.6465.12612, Culture=neutral, PublicKeyToken=null";
            const string namespacePath = "Dev2.Activities.Designers2";
            var typeName = string.Empty;
            if (vm.ModelItem?.ItemType == typeof(DsfDecision))
            {
                typeName = namespacePath + ".Decision.Large";
            }
            if (vm.ModelItem?.ItemType == typeof(DsfSwitch))
            {
                typeName = namespacePath + ".Switch.ConfigureSwitch";
            }
            return Activator.CreateInstance(assemblyFullName, typeName);
        }

        private static ObjectHandle GetAlteredView(Type designerType)
        {
            var alterView = false;
            Type type = null;
            if (designerType == typeof(SequenceDesigner))
            {
                alterView = true;
                type = typeof(Activities.Designers2.Sequence.Large);
            }
            if (!alterView && designerType == typeof(SelectAndApplyDesigner))
            {
                alterView = true;
                type = typeof(Activities.Designers2.SelectAndApply.Large);
            }
            if (!alterView && designerType == typeof(ForeachDesigner))
            {
                type = typeof(Activities.Designers2.Foreach.Large);
            }
            var instance = type != null
                ? (ObjectHandle) Activator.CreateInstance(type, new object[] {true})
                : Activator.CreateInstance(designerType.Assembly.FullName, designerType.Namespace + ".Large");

            return instance;
        }
    }
}