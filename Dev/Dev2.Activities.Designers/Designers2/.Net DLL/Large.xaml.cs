
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

namespace Dev2.Activities.Designers2.Net_DLL
{
    // Interaction logic for Large.xaml
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            SetInitialFocus();
            SetInitialHeight();
        }

        void Large_OnLoaded(object sender, RoutedEventArgs e)
        {
            ReloadToolHeight();
        }

        void SetInitialHeight()
        {
            MinHeight = 220;
            Height = 220;
            MaxHeight = 220;
            MainGrid.RowDefinitions[1].Height = GridLength.Auto;
            MainGrid.RowDefinitions[2].Height = GridLength.Auto;
        }
        void SetOutputInitialHeight()
        {
            if (Height > 220)
            {
                MinHeight = 290;
                Height = 290;
            }
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return MainGrid;
        }

        #endregion

        void ReloadToolHeight()
        {
            var inputContext = InputsControl.DataContext as DotNetDllViewModel;
            var outputContext = OutputsControl.DataContext as DotNetDllViewModel;

            if (inputContext != null)
            {
                SetInputGridHeight(inputContext);
            }
            if (outputContext != null)
            {
                SetOutputGridHeight(outputContext);
            }
        }

        void InputsTemplate_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetInitialHeight();
            var inputs = InputsControl.DataContext as DotNetDllViewModel;

            if (inputs != null)
            {
                SetInputGridHeight(inputs);
            }
        }
        void SetInputGridHeight(DotNetDllViewModel items)
        {
            if (items.Inputs != null && items.InputsVisible)
            {
                double gridHeight;
                double toolHeight = 230;
                double maxToolHeight = 230;
                if (items.Inputs.Count == 0)
                {
                    toolHeight = 290;
                    gridHeight = 60;
                }
                else if (items.Inputs.Count > 0 && items.Inputs.Count < 5)
                {
                    gridHeight = 30 * (items.Inputs.Count + 1);
                    toolHeight += gridHeight;
                    maxToolHeight = toolHeight;
                }
                else
                {
                    gridHeight = 30 * 6;
                    toolHeight += gridHeight;
                    maxToolHeight += 30 * (items.Inputs.Count + 1);
                    MainGrid.RowDefinitions[1].Height = new GridLength(10, GridUnitType.Star);
                }

                Height = toolHeight;
                MinHeight = toolHeight;
                MaxHeight = maxToolHeight;
                items.InputsMinHeight = gridHeight;
            }
        }

        void OutputsTemplate_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetOutputInitialHeight();
            var inputs = OutputsControl.DataContext as DotNetDllViewModel;

            if (inputs != null)
            {
                SetOutputGridHeight(inputs);
            }
            MainGrid.RowDefinitions[2].Height = new GridLength(10, GridUnitType.Star);
        }

        void SetOutputGridHeight(DotNetDllViewModel items)
        {
            if (items.Outputs != null && items.OutputsVisible)
            {
                double gridHeight;
                double toolHeight = 290;
                double maxToolHeight = 290;
                if (items.Outputs.Count == 0)
                {
                    toolHeight = 350;
                    gridHeight = 60;
                }
                else if (items.Outputs.Count > 0 && items.Outputs.Count < 5)
                {
                    gridHeight = 30 * (items.Outputs.Count + 1);
                    toolHeight += gridHeight;
                    maxToolHeight = toolHeight;
                }
                else
                {
                    gridHeight = 30 * 6;
                    toolHeight += gridHeight;
                    maxToolHeight += 30 * (items.Outputs.Count + 1);
                    MainGrid.RowDefinitions[2].Height = new GridLength(10, GridUnitType.Star);
                }

                Height = toolHeight;
                MinHeight = toolHeight;
                MaxHeight = maxToolHeight;
                items.OutputsMinHeight = gridHeight;
            }
        }
    }
}
