#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Core
{
    public class DllListingModel : BindableBase, IDllListingModel, IEquatable<DllListingModel>
    {
        readonly IManagePluginSourceModel _updateManager;
        readonly IManageComPluginSourceModel _comUpdateManager;
        bool _isExpanded;
        bool _isVisible;
        readonly IFileListing _dllListing;
        ObservableCollection<IDllListingModel> _children;
        string _filter;
        bool _progressVisibility;
        int _currentProgress;
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
            }
            _isCom = true;
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
        public string FullName { get; set; }
        ICollection<IFileListing> IFileListing.Children { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    IsExpanded = true;
                }

                OnPropertyChanged(() => IsSelected);
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
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

        void SetPluginIsExpanderVisible()
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
            get => _isExpanderVisible;
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

        void FilterChildren(string searchTerm)
        {
            foreach (var dllListing in _children)
            {
                var dllListingModel = dllListing;
                dllListingModel.Filter(searchTerm);
            }
        }

        void SetIsVisible(string searchTerm) => IsVisible = Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant());

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsVisible);
            }
        }

        #region Equality members

        public bool Equals(DllListingModel other) => string.Equals(Name, other.Name) && string.Equals(FullName, other.FullName) && IsDirectory == other.IsDirectory;


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

        public static bool operator ==(DllListingModel left, DllListingModel right) => Equals(left, right);

        public static bool operator !=(DllListingModel left, DllListingModel right) => !Equals(left, right);

        #endregion
    }
}
