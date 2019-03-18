#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FontAwesome.WPF;

namespace Dev2.Activities.Designers2.Core
{
    public class ActivityDesignerToggle : DependencyObject
    {
        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID, DependencyObject target, DependencyProperty dp) => Create(collapseImageSourceUri, collapseToolTip, expandImageSourceUri, expandToolTip, automationID, target, dp, false);
        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID, DependencyObject target, DependencyProperty dp, bool autoReset)
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

        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID) => Create(collapseImageSourceUri, collapseToolTip, expandImageSourceUri, expandToolTip, automationID, false);

        public static ActivityDesignerToggle Create(string collapseImageSourceUri, string collapseToolTip, string expandImageSourceUri, string expandToolTip, string automationID, bool autoReset)
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
            var image = new Image
            {
                Height = 14,
                Width = 14
            };

            if (Application.Current != null)
            {
                Brush brush = Application.Current.TryFindResource("WareWolfButtonBrush") as SolidColorBrush;

                switch (sourceUri)
                {
                    case "Question":
                        image.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Question, brush);
                        break;
                    case "ServiceQuickVariableInput":
                        image.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.ListAlt, brush);
                        break;
                    case "ServicePropertyEdit":
                        image.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Pencil, brush);
                        break;
                    case "ServiceHelp":
                        image.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Gears, brush);
                        break;
                    default:
                        image.Source = new BitmapImage(new Uri(sourceUri));
                        break;
                }
            }
            return image;
        }
    }
}
