
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Wizards
{
    public class DataListChangeNotificationViewModel : SimpleBaseViewModel
    {
        private DelegateCommand _okCommand;

        #region Constructor

        public DataListChangeNotificationViewModel(string message, IEnumerable<string> addedItems, IEnumerable<string> removedItems)
        {
            Message = message;

            List<string> items = new List<string>();
            items.AddRange(addedItems.Select(s => s + " - Added").OrderBy(s => s));
            items.AddRange(removedItems.Select(s => s + " - Removed").OrderBy(s => s));

            Items = new ObservableCollection<string>(items);
        }

        #endregion Constructor

        #region Properties

        public string Message { get; private set; }
        public ObservableCollection<string> Items { get; private set; }

        public ICommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new DelegateCommand(param => RequestClose(ViewModelDialogResults.Okay)));
            }
        }

        #endregion Properties
    }
}
