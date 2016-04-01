
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

namespace Dev2.Activities.Designers2.ExchangeEmail
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        ExchangeEmailDesignerViewModel ViewModel { get { return DataContext as ExchangeEmailDesignerViewModel; } }

        protected override IInputElement GetInitialFocusElement()
        {
            return this;
        }

        public string ThePassword { get { return ThePasswordBox.Password; } set { ThePasswordBox.Password = value; } }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                ThePassword = viewModel.Password;
            }
        }

        void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                viewModel.Password = ThePassword;
            }
        }
    }
}
