using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Interfaces
{
    public interface IWizardEditable
    {
        IBinaryDataList GetGeneralSettingData();
        IBinaryDataList GetWizardData();
        IBinaryDataList GetInputs();
        IBinaryDataList GetOutputs();
    }
}
