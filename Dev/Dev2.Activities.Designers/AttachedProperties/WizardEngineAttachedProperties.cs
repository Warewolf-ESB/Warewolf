using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class WizardEngineAttachedProperties
    {
        public static bool GetDontOpenWizard(DependencyObject obj)
        {
            return (bool)obj.GetValue(DontOpenWizardProperty);
        }

        public static void SetDontOpenWizard(DependencyObject obj, bool value)
        {
            obj.SetValue(DontOpenWizardProperty, value);
        }

        // Using a DependencyProperty as the backing store for DontOpenWizard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DontOpenWizardProperty =
            DependencyProperty.RegisterAttached("DontOpenWizard", typeof(bool), typeof(WizardEngineAttachedProperties),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    }
}
