using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Core
{
    public class DllListingModel : BindableBase, IDllListingModel, IEquatable<DllListingModel>
    {
        private readonly IManagePluginSourceModel _updateManager;
        private readonly IManageComPluginSourceModel _comUpdateManager;
        private bool _isExpanded;
        private bool _isVisible;
        private readonly IFileListing _dllListing;
        private ObservableCollection<IDllListingModel> _children;
        private string _filter;
        private bool _progressVisibility;
        private int _currentProgress;
        bool _isSelected;
        bool _isExpanderVisible;
        readonly bool _isCom;

        public DllListingModel(IManagePluginSourceModel updateManager, IFileListing dllListing)
        {
            _updateManager = updateManager;
            if (dllListing != null)
            {
                Name = dllListing.Name;
                FullName = dllListing.FullName;
                if (dllListing.Children != null && dllListing.Children.Count > 0)
                {
                    Children =
                        new AsyncObservableCollection<IDllListingModel>(
                            dllListing.Children.Select(input => new DllListingModel(_updateManager, input)));
                }
                IsDirectory = dllListing.IsDirectory;
                IsExpanderVisible = IsDirectory;
                IsVisible = true;
                _dllListing = dllListing;
                ExpandingCommand = new DelegateCommand(Expanding);
            }
            _isCom = false;
        }
        public DllListingModel(IManageComPluginSourceModel updateManager, IFileListing dllListing)
        {
            _comUpdateManager = updateManager;
            if (dllListing != null)
            {
                Name = dllListing.Name;
                FullName = dllListing.FullName;
                var listing = (dllListing as DllListing);
                if (listing != null)
                {
                    Is32Bit = listing.Is32Bit;
                    ClsId = listing.ClsId;
                }
                if (dllListing.Children != null && dllListing.Children.Count > 0)
                {
                    Children =
                        new AsyncObservableCollection<IDllListingModel>(
                            dllListing.Children.Select(input => new DllListingModel(_comUpdateManager, input)));
                }
                IsDirectory = dllListing.IsDirectory;
                IsExpanderVisible = IsDirectory;
                IsVisible = true;
                _dllListing = dllListing;
                ExpandingCommand = new DelegateCommand(Expanding);
            }
            _isCom = true;
        }

        private void Expanding()
        {
            if (!_isCom)
            {
                if (Name == "GAC" && IsExpanded)
                {
                    if (Children != null)
                    {
                        var gacChildren = _children.ToList();
                        if (gacChildren.Count > 5)
                        {
                            ProgressVisibility = true;
                            _children = new AsyncObservableCollection<IDllListingModel>(gacChildren.Take(5));
                            var allChildrenCount = gacChildren.Count;
                            TotalChildrenCount = allChildrenCount;
                            Task.Factory.StartNew(() =>
                            {
                                while (_children.Count < allChildrenCount)
                                {
                                    var items = gacChildren.Skip(ChildrenCount).Take(25);
                                    var col = _children as AsyncObservableCollection<IDllListingModel>;
                                    col?.AddRange(items.ToList());
                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                                    {
                                        CurrentProgress = (int)Math.Round((double)(100 * ChildrenCount) / TotalChildrenCount);
                                        OnPropertyChanged(() => ChildrenCount);
                                    }));
                                }
                            });
                        }
                    }
                }
            }
            else
            {
                if (IsExpanded)
                {
                    if (Children != null)
                    {
                        var comChildren = _children.ToList();
                        if (comChildren.Count > 5)
                        {
                            ProgressVisibility = true;
                            _children = new AsyncObservableCollection<IDllListingModel>(comChildren.Take(5));
                            var allChildrenCount = comChildren.Count;
                            TotalChildrenCount = allChildrenCount;
                            Task.Factory.StartNew(() =>
                            {
                                while (_children.Count < allChildrenCount)
                                {
                                    var items = comChildren.Skip(ChildrenCount).Take(25);
                                    var col = _children as AsyncObservableCollection<IDllListingModel>;
                                    col?.AddRange(items.ToList());
                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                                    {
                                        CurrentProgress = (int)Math.Round((double)(100 * ChildrenCount) / TotalChildrenCount);
                                        OnPropertyChanged(() => ChildrenCount);
                                    }));
                                }
                            });
                        }
                    }
                }
            }
        }

        public int TotalChildrenCount { get; set; }

        public string Name { get; set; }

        public ObservableCollection<IDllListingModel> Children
        {
            get
            {
                if (_children != null)
                {
                    return string.IsNullOrEmpty(_filter)
                        ? _children
                        : new AsyncObservableCollection<IDllListingModel>(_children.Where(a =>
                        {
                            var inner = a;
                            return inner != null && inner.IsVisible;
                        }));
                }

                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }

        public bool ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                OnPropertyChanged(() => ProgressVisibility);
            }
        }

        public int ChildrenCount
        {
            get
            {
                if (_children != null)
                {
                    return _children.Count;
                }
                return 0;
            }
        }
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    if (_currentProgress >= 100)
                    {
                        ProgressVisibility = false;
                    }
                    OnPropertyChanged(() => CurrentProgress);
                }
            }
        }
        public string ClsId { get; set; }
        public bool Is32Bit { get; set; }

        public ICommand ExpandingCommand { get; set; }

        public string FullName { get; set; }

        ICollection<IFileListing> IFileListing.Children { get; set; }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    IsExpanded = true;
                    Expanding();
                }

                OnPropertyChanged(() => IsSelected);
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (!_isCom)
                {
                    _isExpanded = value;
                    SetPluginIsExpanderVisible();
                    OnPropertyChanged(() => IsExpanded);
                    OnPropertyChanged(() => Children);
                }
                else
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsExpanded);
                    OnPropertyChanged(() => Children);

                }
            }
        }

        private void SetPluginIsExpanderVisible()
        {
            if (_isExpanded && _updateManager != null && (_children == null || _children.Count == 0))
            {
                var dllListings = _updateManager.GetDllListings(_dllListing);
                if (dllListings != null)
                {
                    _children =
                        new AsyncObservableCollection<IDllListingModel>(
                            dllListings.Select(input => new DllListingModel(_updateManager, input))
                                .ToList());
                }
                IsExpanderVisible = _children != null && _children.Count > 0;
            }
        }

        public bool IsDirectory { get; set; }

        public bool IsExpanderVisible
        {
            get { return _isExpanderVisible; }
            set
            {
                _isExpanderVisible = value;
                OnPropertyChanged(() => IsExpanderVisible);
            }
        }

        public void Filter(string searchTerm)
        {
            if (!_isCom)
            {
                _filter = searchTerm;

                if (_children != null)
                {
                    FilterChildren(searchTerm);
                }
                if (string.IsNullOrEmpty(searchTerm) || Name == "FileSystem" || Name == "GAC" ||
                    (_children != null && _children.Count > 0 &&
                     _children.Any(model => model.IsVisible)))
                {
                    IsVisible = true;
                }
                else
                {
                    SetIsVisible(searchTerm);
                }

                OnPropertyChanged(() => Children);
            }
            else
            {
                SetIsVisible(searchTerm);
            }
        }

        private void FilterChildren(string searchTerm)
        {
            foreach (var dllListing in _children)
            {
                var dllListingModel = dllListing;
                dllListingModel.Filter(searchTerm);
            }
        }

        private void SetIsVisible(string searchTerm)
        {
            IsVisible = Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant());
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsVisible);
            }
        }

        #region Equality members

        public bool Equals(DllListingModel other)
        {
            
            return string.Equals(Name, other.Name) && string.Equals(FullName, other.FullName) && IsDirectory == other.IsDirectory;
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DllListingModel)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (FullName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ IsDirectory.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DllListingModel left, DllListingModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DllListingModel left, DllListingModel right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
