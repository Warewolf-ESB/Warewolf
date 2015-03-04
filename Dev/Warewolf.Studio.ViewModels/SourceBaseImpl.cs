using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public abstract class SourceBaseImpl<T> : BindableBase, ISourceBase<T>, IDockViewModel
    {
        string _header;

        public SourceBaseImpl(ResourceType? image)
        {
            Image = image;
        }

        #region Implementation of ISourceBase<T>

        public T Item { get; set; }
        public bool HasChanged { get { return Item.Equals(ToModel()); } }

        abstract  public T ToModel();

        #endregion

        #region Implementation of IDockAware

        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                if (!ToModel().Equals(Item))
                    _header = "*" + _header;
                
                OnPropertyChanged(() => Header);
            }
        }
        public ResourceType? Image { get; private set; }

        #endregion

        public bool IsActive { get; set; }
        public event EventHandler IsActiveChanged;
        public abstract void UpdateHelpDescriptor(string helpText);
    }
}