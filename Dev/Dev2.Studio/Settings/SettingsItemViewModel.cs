/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dev2.Annotations;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dev2.Settings
{
    public abstract class SettingsItemViewModel : DependencyObject, INotifyPropertyChanged
    {
        
        public SettingsItemViewModel()
        {
            CloseHelpCommand = new DelegateCommand(o => CloseHelp());
        }

        [JsonIgnore]
        public ICommand CloseHelpCommand { get; private set; }
        [JsonIgnore]
        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        [JsonIgnore]
        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText", typeof(string), typeof(SettingsItemViewModel), new PropertyMetadata(null,OnHelpTextChanged));

        
        private static void OnHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(e.NewValue as string?? "");
        }
        
        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set
            {
                SetValue(IsDirtyProperty, value);
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(SettingsItemViewModel), new PropertyMetadata(false));

        protected abstract void CloseHelp();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName == "Dispatcher")
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
}
