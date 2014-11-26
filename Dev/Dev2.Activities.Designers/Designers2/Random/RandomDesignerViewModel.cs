
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;

namespace Dev2.Activities.Designers2.Random
{
    public class RandomDesignerViewModel : ActivityDesignerViewModel
    {
        public RandomDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            RandomTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enRandomType>();
            SelectedRandomType = Dev2EnumConverter.ConvertEnumValueToString(RandomType);
        }

        public IList<string> RandomTypes { get; private set; }

        public bool IsLengthPath
        {
            get { return (bool)GetValue(IsLengthPathProperty); }
            set { SetValue(IsLengthPathProperty, value); }
        }

        public static readonly DependencyProperty IsLengthPathProperty =
            DependencyProperty.Register("IsLengthPath", typeof(bool), typeof(RandomDesignerViewModel), new PropertyMetadata(false));

        public string LengthContent
        {
            get { return (string)GetValue(LengthContentProperty); }
            set { SetValue(LengthContentProperty, value); }
        }

        public static readonly DependencyProperty LengthContentProperty =
            DependencyProperty.Register("LengthContent", typeof(string), typeof(RandomDesignerViewModel), new PropertyMetadata(null));
        
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(RandomDesignerViewModel), new PropertyMetadata(null));

        public string SelectedRandomType
        {
            get { return (string)GetValue(SelectedRandomTypeProperty); }
            set { SetValue(SelectedRandomTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedRandomTypeProperty =
            DependencyProperty.Register("SelectedRandomType", typeof(string), typeof(RandomDesignerViewModel), new PropertyMetadata(null, OnSelectedRandomTypeChanged));

        static void OnSelectedRandomTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RandomDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                switch (value)
                {
                    case "GUID":
                        viewModel.IsLengthPath = false;
                        viewModel.Visibility = Visibility.Hidden;
                        viewModel.LengthContent = "Length";
                        break;

                    case "Numbers":
                        viewModel.IsLengthPath = false;
                        viewModel.Visibility = Visibility.Visible;
                        viewModel.LengthContent = "Range";
                        break;

                    default:
                        viewModel.IsLengthPath = true;
                        viewModel.Visibility = Visibility.Hidden;
                        viewModel.LengthContent = "Length";
                        break;

                }

                viewModel.RandomType = (enRandomType)Dev2EnumConverter.GetEnumFromStringDiscription(value, typeof(enRandomType)); 
            }
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        enRandomType RandomType { set { SetProperty(value); } get { return GetProperty<enRandomType>(); } }

        public override void Validate()
        {
        }
    }
}
