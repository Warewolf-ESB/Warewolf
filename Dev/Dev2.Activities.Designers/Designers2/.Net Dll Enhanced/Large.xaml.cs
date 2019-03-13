/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

namespace Dev2.Activities.Designers2.Net_Dll_Enhanced
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            SetInitialFocus();
        }

        protected override IInputElement GetInitialFocusElement() => MainGrid;

        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var minHeight = 180;
            if (DataContext is DotNetDllEnhancedViewModel dotNetDllEnhancedViewModel && (dotNetDllEnhancedViewModel.IsConstructorVisible || dotNetDllEnhancedViewModel.IsActionsVisible))
            {
                minHeight = 250;
            }

            MinHeight = minHeight;
            Height = double.NaN;
        }

        void ConstructorExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        void MethodExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        void ConstructorExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        void MethodExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void SetMinHeight()
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            MinHeight = 250;
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            if (dotNetDllEnhancedViewModel?.MethodsToRunList.Count == 1 && !ConstructorExpander.IsExpanded && dotNetDllEnhancedViewModel.MethodsToRunList[0].IsMethodExpanded)
            {
                MinHeight = 270;
            }
            else if (dotNetDllEnhancedViewModel?.MethodsToRunList.Count == 1 && ConstructorExpander.IsExpanded)
            {
                MinHeight = 330;
            }
            else if (dotNetDllEnhancedViewModel?.MethodsToRunList.Count > 1 && !ConstructorExpander.IsExpanded)
            {
                MinHeight = 330;
            }
            else
            {
                if (dotNetDllEnhancedViewModel?.MethodsToRunList.Count > 1 && ConstructorExpander.IsExpanded)
                {
                    MinHeight = 380;
                }
            }
        }

        void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            dotNetDllEnhancedViewModel?.UpdateMethodInputs();
            e.Handled = true;
        }

        void AutoCompleteBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            dotNetDllEnhancedViewModel?.UpdateMethodInputs();
            e.Handled = true;
        }
    }
}
