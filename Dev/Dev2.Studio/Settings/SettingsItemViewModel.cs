
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dev2.Annotations;
using Dev2.Runtime.Configuration.ViewModels.Base;

namespace Dev2.Settings
{
    public abstract class SettingsItemViewModel : DependencyObject, INotifyPropertyChanged
    {
        protected SettingsItemViewModel()
        {
            CloseHelpCommand = new DelegateCommand(o => CloseHelp());
        }

        public ICommand CloseHelpCommand { get; private set; }

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText", typeof(string), typeof(SettingsItemViewModel), new PropertyMetadata(null));

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set
            {
                SetValue(IsDirtyProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(SettingsItemViewModel), new PropertyMetadata(false));

        protected abstract void CloseHelp();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
