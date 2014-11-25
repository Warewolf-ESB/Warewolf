
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.UiAutomation
{
    public class AutomationIdCreaterViewModel : SimpleBaseViewModel
    {
        #region Fields

        // ReSharper disable InconsistentNaming
        public ICommand _OkCommand;
        public ICommand _CancelCommand;
        // ReSharper restore InconsistentNaming

        #endregion Fields

        #region Properties

        public string AutomationID { get; set; }

        #endregion Properties

        #region Methods

        public ICommand OkCommand
        {
            get
            {
                return _OkCommand ?? (_OkCommand = new DelegateCommand(param => SaveID()));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _CancelCommand ?? (_CancelCommand = new DelegateCommand(param => Cancel()));
            }
        }

        public void SaveID()
        {
            RequestClose(ViewModelDialogResults.Okay);
        }

        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion Methods
    }
}
