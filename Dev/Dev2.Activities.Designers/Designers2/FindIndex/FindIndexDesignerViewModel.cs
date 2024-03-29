/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.FindIndex
{
    public class FindIndexDesignerViewModel : ActivityDesignerViewModel
    {
        public FindIndexDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            IndexList = new List<string> { "First Occurrence", "Last Occurrence", "All Occurrences" };
            DirectionList = new List<string> { "Left to Right", "Right to Left" };
            SelectedIndex = string.IsNullOrEmpty(Index) ? IndexList[0] : Index;
            SelectedDirection = string.IsNullOrEmpty(Direction) ? DirectionList[0] : Direction;
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Data_Find_Index;
        }

        public IList<string> IndexList { get; private set; }
        public IList<string> DirectionList { get; private set; }

        public string SelectedIndex
        {
            get { return (string)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(string), typeof(FindIndexDesignerViewModel), new PropertyMetadata(null, OnSelectedIndexChanged));

        static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (FindIndexDesignerViewModel)d;
            var value = e.NewValue as string;

            if(!string.IsNullOrWhiteSpace(value))
            {
                viewModel.Index = value;
            }
        }

        public string SelectedDirection
        {
            get { return (string)GetValue(SelectedDirectionProperty); }
            set { SetValue(SelectedDirectionProperty, value); }
        }

        public static readonly DependencyProperty SelectedDirectionProperty =
            DependencyProperty.Register("SelectedDirection", typeof(string), typeof(FindIndexDesignerViewModel), new PropertyMetadata(null, OnSelectedDirectionChanged));

        static void OnSelectedDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (FindIndexDesignerViewModel)d;
            var value = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(value))
            {
                viewModel.Direction = value;
            }
        }
        
        string Index { set => SetProperty(value); get => GetProperty<string>(); }
        string Direction { set => SetProperty(value); get => GetProperty<string>(); }
        
        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
