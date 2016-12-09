using System;
using Dev2.Common.Interfaces.DB;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public abstract class SourceBaseImpl<T> : BindableBase, ISourceBase<T>, IDockViewModel,IDisposable
        where T:IEquatable<T>
    {
        string _header;

        protected SourceBaseImpl(string image)
        {
            Image = image;
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != "Header")
                {
                    OnPropertyChanged(() => Header);
                }
            };
        }


        #region Implementation of ISourceBase<T>

        public T Item { get; set; }
        public bool HasChanged
        {
            get
            {
                if (Item == null)
                {
                    ToModel();
                }
                var hasChanged = Item != null && !Item.Equals(ToModel());
                return hasChanged;
            }
        }

        public abstract T ToModel();

        public abstract string Name { get; set; }

        public abstract void FromModel(T service);

        #endregion

        #region Implementation of IDockAware

        public string Header
        {
            get
            {
                if (HasChanged)
                {
                    return _header + " *";
                }
                return _header;
            }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }
        public string Image { get; set; }

        #endregion

        public bool IsActive { get; set; }
        public abstract bool CanSave();

        public event EventHandler IsActiveChanged;
        public abstract void UpdateHelpDescriptor(string helpText);
        
        public abstract void Save();

        public Guid SelectedGuid { get; set; }

        public void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

        public string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return "Failed";
            }
            string exceptionMsg = Resources.Languages.Core.ExceptionErrorLabel + exception.Message;

            if (exception.InnerException != null)
            {
                string innerExpceptionMsg = Resources.Languages.Core.InnerExceptionErrorLabel + exception.InnerException.Message;
                return exceptionMsg + Environment.NewLine + Environment.NewLine + innerExpceptionMsg;
            }
            return exceptionMsg;
        }
    }
}