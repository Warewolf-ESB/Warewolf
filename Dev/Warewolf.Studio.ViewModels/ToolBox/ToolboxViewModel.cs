using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolboxViewModel:BindableBase,IToolboxViewModel
    {
        readonly IToolboxModel _localModel;
        readonly IToolboxModel _remoteModel;
        ICollection<IToolDescriptorViewModel> _tools;
        bool _isEnabled;

        public ToolboxViewModel(ICollection<IToolDescriptorViewModel> tools, IToolboxModel localModel,IToolboxModel remoteModel)
        {
            _localModel = localModel;
            _remoteModel = remoteModel;
            Tools = tools;
        }

        #region Implementation of IToolboxViewModel

        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolDescriptorViewModel> Tools
        {
            get
            {

                return _tools;
            }
            private set
            {
                OnPropertyChanged("Tools");
                _tools = value;
            }
        }
        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                OnPropertyChanged("Tools");
                _isEnabled = value;
            }
        }

        /// <summary>
        /// filters the list of tools available to the user.
        /// </summary>
        /// <param name="searchString"></param>
        public void Filter(string searchString)
        {
            // not sure about this warning ofr pure methods.
            // ReSharper disable MaximumChainedReferences
            Tools = new ObservableCollection<IToolDescriptorViewModel>( _remoteModel.GetTools()
                .Where(a => a.Name.Contains(searchString))
                .Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            // ReSharper restore MaximumChainedReferences
        }

        #endregion
    }
}
