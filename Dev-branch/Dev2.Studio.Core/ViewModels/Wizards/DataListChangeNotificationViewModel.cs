using Dev2.Studio.Core.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Dev2.Studio.Core.ViewModels.Wizards
{
    public class DataListChangeNotificationViewModel : SimpleBaseViewModel
    {
        private RelayCommand _okCommand;

        #region Constructor

        public DataListChangeNotificationViewModel(string message, IList<string> addedItems, IList<string> removedItems)
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
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(param =>
                    {
                        RequestClose(ViewModelDialogResults.Okay);
                    });
                }

                return _okCommand;
            }

        }

        #endregion Properties
    }
}
