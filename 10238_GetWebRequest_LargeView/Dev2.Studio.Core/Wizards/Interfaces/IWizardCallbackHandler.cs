using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Wizards.Interfaces
{
    public interface IWizardCallbackHandler
    {
        /// <summary>
        /// Completes the callback.
        /// </summary>
        void CompleteCallback();
        /// <summary>
        /// Cancels the callback.
        /// </summary>
        void CancelCallback();
    }
}
