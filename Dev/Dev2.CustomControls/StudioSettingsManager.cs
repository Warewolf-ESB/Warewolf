using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;

namespace Dev2.CustomControls
{
    public class StudioSettingsManager
    {
        public static void LoadSettings(FrameworkElement sender, Dictionary<FrameworkElement, DependencyProperty> savedElements)
        {
            EnsureProperties(sender, savedElements);
            foreach (FrameworkElement element in savedElements.Keys)
            {
                try
                {
                    element.SetValue(savedElements[element], Properties.Settings.Default[sender.Name + "." + element.Name]);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static void SaveSettings(FrameworkElement sender, Dictionary<FrameworkElement, DependencyProperty> savedElements)
        {
            EnsureProperties(sender, savedElements);
            foreach (FrameworkElement element in savedElements.Keys)
            {
                Properties.Settings.Default[sender.Name + "." + element.Name] = element.GetValue(savedElements[element]);
            }
            Properties.Settings.Default.Save();
        }

        private static void EnsureProperties(FrameworkElement sender, Dictionary<FrameworkElement, DependencyProperty> savedElements)
        {
            foreach (FrameworkElement element in savedElements.Keys)
            {
                bool hasProperty =
                    Properties.Settings.Default.Properties[sender.Name + "." + element.Name] != null;

                if (!hasProperty)
                {
                    SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
                    UserScopedSettingAttribute attribute = new UserScopedSettingAttribute();
                    attributes.Add(attribute.GetType(), attribute);

                    SettingsProperty property = new SettingsProperty(sender.Name + "." + element.Name,
                        savedElements[element].DefaultMetadata.DefaultValue.GetType(), Properties.Settings.Default.Providers["LocalFileSettingsProvider"], false, null, SettingsSerializeAs.String, attributes, true, true);
                    Properties.Settings.Default.Properties.Add(property);
                }
            }
            Properties.Settings.Default.Reload();
        }
    }
}
