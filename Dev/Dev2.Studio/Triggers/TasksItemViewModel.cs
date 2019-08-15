/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Dev2.Triggers
{
    public abstract class TasksItemViewModel : DependencyObject, INotifyPropertyChanged
    {
        protected TasksItemViewModel()
        {
            CloseHelpCommand = new DelegateCommand(o => CloseHelp());
        }

        [JsonIgnore]
        public ICommand CloseHelpCommand { get; private set; }
        [JsonIgnore]
        public string HelpText
        {
            get => (string)GetValue(HelpTextProperty);
            set { SetValue(HelpTextProperty, value); }
        }

        [JsonIgnore]
        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register(nameof(HelpText), typeof(string), typeof(TasksItemViewModel), new PropertyMetadata(null, OnHelpTextChanged));

        static void OnHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(e.NewValue as string ?? "");
        }

        public bool IsDirty
        {
            get => (bool)GetValue(IsDirtyProperty);
            set
            {
                SetValue(IsDirtyProperty, value);
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register(nameof(IsDirty), typeof(bool), typeof(TasksItemViewModel), new PropertyMetadata(false));

        protected abstract void CloseHelp();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
