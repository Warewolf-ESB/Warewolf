﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Core
{
    public class FileListingModel : BindableBase, IFileListingModel,IEquatable<FileListingModel>
    {
        private readonly IEmailAttachmentModel _model;
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

        public FileListingModel(IEmailAttachmentModel model, IFileListing file,Action selected)
        {
            _model = model;
            _selectedAction = selected;
            if (file != null)
            {
                Name = file.Name;
                FullName = file.FullName;
                if (file.Children != null && file.Children.Count > 0)
                {
                    Children =
                        new AsyncObservableCollection<IFileListingModel>(
                            file.Children.Select(input => new FileListingModel(_model, input,selected)));
                }
                IsDirectory = file.IsDirectory;
                IsExpanderVisible = IsDirectory;
                IsVisible = true;
                _file = file;

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
                    return String.IsNullOrEmpty(_filter)
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
             
                }
                
                OnPropertyChanged(() => IsSelected);
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
                    if(dllListings != null)
                    {
                        Children =
                            new AsyncObservableCollection<IFileListingModel>(
                                dllListings.Select(input => new FileListingModel(_model, input,_selectedAction))
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
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (FullName != null ? FullName.GetHashCode() : 0);
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
        {   if(IsChecked)
                    acc.Add(FullName);
            if(Children!=null)
            {
                foreach(var dllListingModel in Children)
                {

                    dllListingModel.FilterSelected(acc);
                }
            }
            return acc;
        }

        public bool IsChecked { get
        {
            return _isChecked;
        }
            set
            {
                _isChecked = value;
                OnPropertyChanged(()=>IsChecked);
                _selectedAction();
            }
        }
    }
}