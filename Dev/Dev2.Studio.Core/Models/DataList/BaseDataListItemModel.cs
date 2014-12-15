
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models.DataList
{
    public abstract class BaseDataListItemModel : PropertyChangedBase
    {
        #region Fields

        private string _name;
        private string _displayName;
        private bool _isExpanded = true;
        private ObservableCollection<IDataListItemModel> _children;

        #endregion Fields

        #region Properties

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                Name = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = ValidateName(value);
                NotifyOfPropertyChange(() => Name);
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        public ObservableCollection<IDataListItemModel> Children
        {
            get { return _children ?? (_children = new ObservableCollection<IDataListItemModel>()); }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);                
            }
        }

        #endregion Properties

        #region Abstract Methods

        public abstract string ValidateName(string name);

        #endregion Abstract Methods
    }
}
