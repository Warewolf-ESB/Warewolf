/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces.DataList;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Core.Models.DataList
{
    public class DataListHeaderItemModel : PropertyChangedBase
    {
        private string _displayName;
        private IEnumerable<IDataListItemModel> _children;

        public DataListHeaderItemModel(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public IEnumerable<IDataListItemModel> Children
        {
            get { return _children ?? (_children = new ObservableCollection<IDataListItemModel>()); }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}