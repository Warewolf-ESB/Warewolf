using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Core
{
    public class DllListingModel : BindableBase, IDllListingModel
    {
        readonly IManagePluginSourceModel _updateManager;
        bool _isExpanded;
        bool _isVisible;
        readonly IDllListing _dllListing;
        IList<IDllListing> _children;
        string _filter;

        public DllListingModel(IManagePluginSourceModel updateManager, IDllListing dllListing)
        {
            _updateManager = updateManager;
            if(dllListing != null)
            {
                Name = dllListing.Name;
                FullName = dllListing.FullName;
                if(dllListing.Children != null && dllListing.Children.Count > 0)
                {
                    Children = new List<IDllListing>(dllListing.Children.Select(input => new DllListingModel(_updateManager, input)).ToList());
                }
                IsDirectory = dllListing.IsDirectory;
                IsVisible = true;
                _dllListing = dllListing;
            }
        }

        public string Name { get; set; }
        public IList<IDllListing> Children
        {
            get
            {
                if(_children != null)
                {
                    return String.IsNullOrEmpty(_filter) ? _children : new ObservableCollection<IDllListing>(_children.Where(a =>
                    {
                        var inner = a as IDllListingModel;
                        return inner != null && inner.IsVisible;
                    }));
                }
                return _children;
            }
            set
            {
                _children = value;
            }
        }
        public string FullName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                if (_isExpanded && _updateManager != null && (Children == null || Children.Count == 0))
                {
                    Children = new List<IDllListing>(_updateManager.GetDllListings(_dllListing).Select(input => new DllListingModel(_updateManager, input)).ToList());                    
                }
            }
        }
        public bool IsDirectory { get; set; }
        public bool IsExpanderVisible
        {
            get
            {
                return IsDirectory;
            }
        }

        public void Filter(string searchTerm)
        {
            _filter = searchTerm;
            if (_children != null)
            {
                foreach (var dllListing in _children)
                {
                    var dllListingModel = (IDllListingModel)dllListing;
                    dllListingModel.Filter(searchTerm);
                }
            }
            if (String.IsNullOrEmpty(searchTerm) || Name == "FileSystem" || Name == "GAC" || (_children!=null && _children.Count > 0 && _children.Any(model => ((IDllListingModel)model).IsVisible)))
            {
                IsVisible = true;
            }
            else
            {
                IsVisible = Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant());                
            }
            
            OnPropertyChanged(() => Children);
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsVisible);
            }
        }
    }

    
}