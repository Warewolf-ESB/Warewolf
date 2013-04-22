using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Core.Wizards.Interfaces
{
    public interface IActivitySettingsWizardCallbackHandler : IWizardCallbackHandler
    {        
        //IDataListCompiler Compiler { get; set; }
        Func<IDataListCompiler> CreateCompiler { get; set; }
        ModelItem Activity { get; set; }
        Guid DatalistID { get; set; }
    }
}
