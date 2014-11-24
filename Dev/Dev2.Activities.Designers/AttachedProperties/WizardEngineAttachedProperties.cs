
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
