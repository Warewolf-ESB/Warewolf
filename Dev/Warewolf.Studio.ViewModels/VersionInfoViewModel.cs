using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class VersionInfoViewModel: BindableBase, IVersionInfoViewModel
    {
        string _versionName;
        string _version;
        DateTime _versionDate;
        bool _canRollBack;
        bool _isVisible;
        string _versionHeader;
        string _reason;

        #region Implementation of IVersionInfoViewModel

        public  VersionInfoViewModel(IVersionInfo version)
        {
            VersionName = version.VersionNumber;
            VersionDate = version.DateTimeStamp;
            ResourceId = version.ResourceId;
            Version = version.VersionNumber;
            Reason = version.Reason;
            IsVisible = true;
        }

        public string VersionName
        {
            get
            {
                return _versionName;
            }
            set
            {
                _versionName = value;
                OnPropertyChanged(()=>VersionName);
            }
        }
        public Guid ResourceId { get; set; }
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnPropertyChanged(Version);
            }
        }
        public DateTime VersionDate
        {
            get
            {
                return _versionDate;
            }
            set
            {
                _versionDate = value;
                OnPropertyChanged(() => VersionDate);
            }
        }
        public bool CanRollBack
        {
            get
            {
                return _canRollBack;
            }
            set
            {
                _canRollBack = value;
                OnPropertyChanged(() => CanRollBack);
            }
        }
        public ICommand OpenCommand { get; set; }
        public ICommand RollbackCommand { get; set; }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => CanRollBack);
            }
        }
        public string VersionHeader
        {
            get
            {
                return  string.Format("{0} {1} {2}", Version, VersionDate.ToLongDateString(), Reason);
            }
            set
            {
                _versionHeader = value;
            }
        }
        public string Reason
        {
            get
            {
                return _reason;
            }
            set
            {

                _reason = value;
                OnPropertyChanged(Reason);
            }
        }

        #endregion
    }
}