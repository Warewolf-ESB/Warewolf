using Dev2.Composition;
using System;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public enum ViewModelDialogResults
    {
        Okay,
        Cancel,
    }

    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class BaseViewModel : SimpleBaseViewModel
    {
        #region Constructor

        protected BaseViewModel()
        {
            SatisfyImports();
        }

        #endregion // Constructor

        #region Protected Virtual Methods

        protected virtual void SatisfyImports()
        {
            //For testing scenarios - ability to fail silently when everythings not imported
            ImportService.TrySatisfyImports(this);
        }

        #endregion Protected Virtual Methods
    }
}
