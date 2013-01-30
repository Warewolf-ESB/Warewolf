using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Studio.Diagnostics
{
    public class InputOutputDataTemplateSelector : DataTemplateSelector
    {
        public string Key { get; set; }
        public DataTemplate ScalarTemplate { get; set; }
        public DataTemplate RecordSetTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //return base.SelectTemplate(item, container);
            if (item is DebugOuputInputOutputGroup)
            {
                return RecordSetTemplate;
            }

            return ScalarTemplate;
        }
    }
}
