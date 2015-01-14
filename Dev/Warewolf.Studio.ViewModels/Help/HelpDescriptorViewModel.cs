using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.Help
{
    public class HelpDescriptorViewModel: BindableBase,IHelpDescriptorViewModel
    {
        IHelpDescriptor _descriptor;
        bool _isEnabled;

        public HelpDescriptorViewModel(IHelpDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        #region Implementation of IHelpDescriptorViewModel

        /// <summary>
        /// Display name
        /// </summary>
        public string Name
        {
            get
            {
                return _descriptor.Name;
            }
        }
        /// <summary>
        /// The help text
        /// </summary>
        public string Description
        {
            get
            {
                return _descriptor.Description;
            }
        }
        /// <summary>
        /// The icon to display
        /// </summary>
        public DrawingImage Icon
        {
            get
            {
                return _descriptor.Icon;
            }
        }
        /// <summary>
        /// Is this help enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        #endregion
    }
}