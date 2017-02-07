/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

namespace Dev2.Activities.Designers2.Net_Dll_Enhanced
{
    // Interaction logic for Large.xaml
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            SetInitialFocus();
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return MainGrid;
        }

        #endregion

        private void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var minHeight = 180;
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            if (dotNetDllEnhancedViewModel != null)
            {
                if (dotNetDllEnhancedViewModel.IsConstructorVisible || dotNetDllEnhancedViewModel.IsActionsVisible)
                {
                    minHeight = 250;
                }
            }
            MinHeight = minHeight;
            Height = double.NaN;
        }

        private void ConstructorExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        private void MethodExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        private void ConstructorExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        private void MethodExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            Height = double.NaN;
            SetMinHeight();
        }

        private void SetMinHeight()
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
            else if (dotNetDllEnhancedViewModel?.MethodsToRunList.Count > 1 && ConstructorExpander.IsExpanded)
            {
                MinHeight = 380;
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            dotNetDllEnhancedViewModel?.UpdateMethodInputs();
            e.Handled = true;
        }

        private void AutoCompleteBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var dotNetDllEnhancedViewModel = DataContext as DotNetDllEnhancedViewModel;
            dotNetDllEnhancedViewModel?.UpdateMethodInputs();
            e.Handled = true;
        }
    }
}
