using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Diagnostics;
using Dev2.Studio.Diagnostics;

namespace Dev2.Studio.AppResources.Converters
{
    public class IDebugStateToTemplateSelectorConverter : IValueConverter
    {
        public IDebugStateToTemplateSelectorConverter()
        {
            DataTemplateSelectors = new InputOutputDataTemplateSelectorCollection();
        }

        public InputOutputDataTemplateSelectorCollection DataTemplateSelectors { get; set; }
        public DataTemplateSelector DefaultTemplateSelector { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDebugState debugState = value as IDebugState;

            if (debugState == null)
            {
                return Binding.DoNothing;
            }

            InputOutputDataTemplateSelector inputOutputDataTemplate = DataTemplateSelectors.FirstOrDefault(d => d.Key == debugState.Name);

            if (inputOutputDataTemplate == null)
            {
                return DefaultTemplateSelector;
            }

            return inputOutputDataTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
