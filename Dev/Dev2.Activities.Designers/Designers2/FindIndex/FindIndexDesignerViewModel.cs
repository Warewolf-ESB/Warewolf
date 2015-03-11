
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Activities.Designers2.FindIndex
{
    public class FindIndexDesignerViewModel : ActivityDesignerViewModel
    {
        public FindIndexDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
            IndexList = new List<string> { "First Occurrence", "Last Occurrence", "All Occurrences" };
            DirectionList = new List<string> { "Left to Right", "Right to Left" };
            SelectedIndex = string.IsNullOrEmpty(Index) ? IndexList[0] : Index;
            SelectedDirection = string.IsNullOrEmpty(Direction) ? DirectionList[0] : Direction;
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
        
        // DO NOT bind to these properties - these are here for convenience only!!!
        string Index { set { SetProperty(value); } get { return GetProperty<string>(); } }
        string Direction { set { SetProperty(value); } get { return GetProperty<string>(); } }
        
        public override void Validate()
        {
        }
    }
}
