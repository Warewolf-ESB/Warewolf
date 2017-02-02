/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Warewolf.Studio.Core
{
    public class FileListingModel : BindableBase, IFileListingModel,IEquatable<FileListingModel>
    {
        private readonly IFileChooserModel _model;
        private bool _isExpanded;
        private bool _isVisible;
        private readonly IFileListing _file;
        private ObservableCollection<IFileListingModel> _children;
        private string _filter;
        private bool _progressVisibility;
        private int _currentProgress;
        bool _isSelected;
        bool _isExpanderVisible;
        bool _isChecked;
        Action _selectedAction;
        private bool _useIsSelected;

        public FileListingModel(IFileChooserModel model, IFileListing file, Action selected, bool useIsSelected = false)
        {
            _model = model;
            _selectedAction = selected;
            UseIsSelected = useIsSelected;
            if (file != null)
            {
                Name = file.Name;
                FullName = file.FullName;
                if (file.Children != null && file.Children.Count > 0)
                {
                    Children = new AsyncObservableCollection<IFileListingModel>(
                            file.Children.Select(input => new FileListingModel(_model, input, selected, useIsSelected)));
                }
                IsDirectory = file.IsDirectory;
                IsExpanderVisible = IsDirectory;
                IsVisible = true;
                _file = file;
            }
        }

        private bool UseIsSelected
        {
            get { return _useIsSelected; }
            set
            {
                _useIsSelected = value;
                OnPropertyChanged(() => UseIsSelected);
            }
        }

        public int TotalChildrenCount { get; set; }

        public string Name { get; set; }

        public ObservableCollection<IFileListingModel> Children
        {
            get
            {
                if (_children != null)
                {
                    return string.IsNullOrEmpty(_filter)
                        ? _children
                        : new AsyncObservableCollection<IFileListingModel>(_children.Where(a =>
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
                if (Children != null)
                {
                    return Children.Count;
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

        public ICommand ExpandingCommand { get; set; }

        public string FullName { get; set; }

        ICollection<IFileListing> IFileListing.Children { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    IsExpanded = true;
                }

                OnPropertyChanged(() => IsSelected);
                _selectedAction();
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;

                if (_isExpanded && _model != null && (Children == null || Children.Count == 0))
                {
                    var dllListings = _model.FetchFiles(_file);
                    if (dllListings != null)
                    {
                        Children =
                            new AsyncObservableCollection<IFileListingModel>(
                                dllListings.Select(input => new FileListingModel(_model, input, _selectedAction,UseIsSelected))
                                    .ToList());
                    }
                    IsExpanderVisible = Children != null && Children.Count > 0;
                }
                OnPropertyChanged(() => IsExpanded);
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
            _filter = searchTerm;
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

        public bool Equals(FileListingModel other)
        {
            // ReSharper disable once PossibleNullReferenceException
            return string.Equals(Name, other.Name) && string.Equals(FullName, other.FullName) &&
                   IsDirectory == other.IsDirectory;
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
            return Equals((FileListingModel)obj);
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

        public static bool operator ==(FileListingModel left, FileListingModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileListingModel left, FileListingModel right)
        {
            return !Equals(left, right);
        }

        #endregion

        public List<string> FilterSelected(List<string> acc)
        {
            var canAdd = UseIsSelected ? IsSelected : IsChecked;
            if (canAdd)
                acc.Add(FullName);
            if (Children != null)
            {
                foreach (var dllListingModel in Children)
                {
                    dllListingModel.FilterSelected(acc);
                }
            }
            return acc;
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged(() => IsChecked);
                _selectedAction();
            }
        }
    }
}