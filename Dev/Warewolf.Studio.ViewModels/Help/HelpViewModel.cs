using System;
using System.Collections.Generic;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.Help
{
    public class HelpWindowViewModel: BindableBase, IHelpWindowViewModel,IDisposable
    {
        IHelpDescriptorViewModel _currentHelpText;
        readonly IHelpDescriptorViewModel _defaultViewModel;

        public HelpWindowViewModel(IHelpDescriptorViewModel defaultViewModel,IHelpWindowModel model)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "defaultViewModel", defaultViewModel }, { "model", model } });
            _defaultViewModel = defaultViewModel;
            CurrentHelpText = _defaultViewModel;
            HelpModel = model;
            model.OnHelpTextReceived += model_OnHelpTextReceived;
        }

        public string HelpText
        {
            get
            {
                return CurrentHelpText.Description;
            }
        }

        public string HelpName
        {
            get
            {
                return CurrentHelpText.Name;
            }
        }

        public DrawingImage HelpImage
        {
            get
            {
                return CurrentHelpText.Icon;
            }
        }

        void model_OnHelpTextReceived(object sender, IHelpDescriptor desc)
        {
            try
            {
                CurrentHelpText = new HelpDescriptorViewModel(desc);
            }
            catch(Exception)
            {
                CurrentHelpText = _defaultViewModel;
                throw;
            }
           
        }

        public IHelpWindowModel HelpModel { get; private set; }

        #region Implementation of IHelpWindowViewModel

        /// <summary>
        /// Wpf component binds here
        /// </summary>
        public IHelpDescriptorViewModel CurrentHelpText
        {
            get
            {
                return _currentHelpText;
            }
            private set
            {
                _currentHelpText = value;
                OnPropertyChanged(() => HelpName);
                OnPropertyChanged(() => HelpText);
                OnPropertyChanged(() => HelpImage);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UnusedParameter.Local
        void Dispose(bool disposing)
        {
            HelpModel.OnHelpTextReceived -= model_OnHelpTextReceived;
        }

        #endregion
    }
}