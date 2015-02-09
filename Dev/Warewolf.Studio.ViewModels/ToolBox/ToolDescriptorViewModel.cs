using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolDescriptorViewModel : BindableBase,IToolDescriptorViewModel
    {
        IToolDescriptor _tool;
        bool _isEnabled;

              static ResourceDictionary Resources;
        static ToolDescriptorViewModel()
        {
            

        }

        public ToolDescriptorViewModel(IToolDescriptor tool, bool isEnabled)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{ {"tool",tool}});
            IsEnabled = isEnabled;
            Tool = tool;
        }

        #region Implementation of IToolDescriptorViewModel

        public IToolDescriptor Tool
        {
            get
            {
                return _tool;
            }
            private set
            {
                OnPropertyChanged("Tool");
                _tool = value;
            }
        }

        public string Name
        {
            get
            {
                return Tool.Name;
            }
        }

        public DrawingImage Icon
        {
            get
            {
                return GetImage( Tool.Icon,Tool.IconUri);
            }
        }

        DrawingImage GetImage(string icon,string iconUri)
        {
           return (DrawingImage)((ResourceDictionary)Application.LoadComponent(new Uri(iconUri,
               UriKind.RelativeOrAbsolute)))[icon];
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            private set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        #endregion
    }
}
