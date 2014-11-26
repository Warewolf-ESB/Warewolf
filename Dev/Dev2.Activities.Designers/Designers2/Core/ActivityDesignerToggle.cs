
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Dev2.Activities.Designers2.Core
{
    public class ActivityDesignerToggle : DependencyObject
    {
        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID, DependencyObject target, DependencyProperty dp, bool autoReset = false)
        {
            var toggle = new ActivityDesignerToggle
            {
                CollapseImageSourceUri = collapseImageSourceUri,
                CollapseToolTip = collapseToolTip,
                ExpandImageSourceUri = expandImageSourceUri,
                ExpandToolTip = expandToolTip,
                Image = CreateImage(expandImageSourceUri),
                ToolTip = expandToolTip,
                AutomationID = automationID,
                AutoReset = autoReset
            };

            if(target != null && dp != null)
            {
                BindingOperations.SetBinding(target, dp, new Binding("IsChecked")
                {
                    Source = toggle,
                    Mode = BindingMode.TwoWay
                });
            }

            return toggle;
        }

        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID, bool autoReset = false)
        {
            var toggle = new ActivityDesignerToggle
            {
                CollapseImageSourceUri = collapseImageSourceUri,
                CollapseToolTip = collapseToolTip,
                ExpandImageSourceUri = expandImageSourceUri,
                ExpandToolTip = expandToolTip,
                Image = CreateImage(expandImageSourceUri),
                ToolTip = expandToolTip,
                AutomationID = automationID,
                AutoReset = autoReset
            };

            return toggle;
        }

        // Prevent direct instantiation - use Create method instead
        ActivityDesignerToggle()
        {
        }

        public string CollapseImageSourceUri { get; private set; }
        public string CollapseToolTip { get; private set; }

        public string ExpandImageSourceUri { get; private set; }
        public string ExpandToolTip { get; private set; }

        bool AutoReset { get; set; }

        public string AutomationID
        {
            get { return (string)GetValue(AutomationIDProperty); }
            set { SetValue(AutomationIDProperty, value); }
        }

        public static readonly DependencyProperty AutomationIDProperty =
            DependencyProperty.Register("AutomationID", typeof(string), typeof(ActivityDesignerToggle), new PropertyMetadata(null));

        public Image Image
        {
            get { return (Image)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(Image), typeof(ActivityDesignerToggle));

        public string ToolTip
        {
            get { return (string)GetValue(ToolTipProperty); }
            set { SetValue(ToolTipProperty, value); }
        }

        public static readonly DependencyProperty ToolTipProperty =
            DependencyProperty.Register("ToolTip", typeof(string), typeof(ActivityDesignerToggle));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ActivityDesignerToggle), new PropertyMetadata(false, OnIsCheckedPropertyChanged));

        static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggle = (ActivityDesignerToggle)d;
            var isChecked = (bool)e.NewValue;

            if(isChecked)
            {
                toggle.Image = CreateImage(toggle.CollapseImageSourceUri);
                toggle.ToolTip = toggle.CollapseToolTip;
                if(toggle.AutoReset)
                {
                    toggle.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                    {
                        toggle.IsChecked = false;
                    }));
                }
            }
            else
            {
                toggle.Image = CreateImage(toggle.ExpandImageSourceUri);
                toggle.ToolTip = toggle.ExpandToolTip;
            }
        }

        static Image CreateImage(string sourceUri)
        {
            return new Image { Source = new BitmapImage(new Uri(sourceUri)) };
        }
    }
}
