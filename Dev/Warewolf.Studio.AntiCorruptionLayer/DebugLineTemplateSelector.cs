using System.Windows;
using System.Windows.Controls;
using Warewolf.Studio.AntiCorruptionLayer.ViewModels;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class DebugLineTemplateSelector : DataTemplateSelector
    {
        public string Key { get; set; }
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate GroupTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item is DebugLineGroup ? GroupTemplate : ItemTemplate;
        }
    }
}