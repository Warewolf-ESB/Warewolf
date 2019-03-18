#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Interfaces;
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

        public abstract void FromModel(T source);

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

        public void AfterSave(Guid environmentId, Guid resourceId)
        {
            var explorerViewModel = CustomContainer.Get<IShellViewModel>().ExplorerViewModel;
            var environmentViewModel = explorerViewModel.Environments.FirstOrDefault(model => model.Server.EnvironmentID == environmentId);
            if (environmentViewModel != null)
            {
                var env = CustomContainer.Get<IServerRepository>().Get(environmentId);
                var resource = env.ResourceRepository.LoadContextualResourceModel(resourceId);
                var item = environmentViewModel.FindByPath(resource.GetSavePath());
                var viewModel = environmentViewModel as EnvironmentViewModel;
                var savedItem = viewModel?.CreateExplorerItemFromResource(environmentViewModel.Server, item, false, false, resource);
                item.AddChild(savedItem);
            }
        }

        public Guid SelectedGuid { get; set; }

        public void Dispose() => OnDispose();

        protected virtual void OnDispose()
        {
        }

        public string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return "Failed";
            }
            var exceptionMsg = Resources.Languages.Core.ExceptionErrorLabel + exception.Message;

            if (exception.InnerException != null)
            {
                var innerExpceptionMsg = Resources.Languages.Core.InnerExceptionErrorLabel + exception.InnerException.Message;
                return exceptionMsg + Environment.NewLine + Environment.NewLine + innerExpceptionMsg;
            }
            return exceptionMsg;
        }

        protected virtual void OnIsActiveChanged()
        {
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}