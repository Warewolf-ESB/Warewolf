using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Interfaces
{
    public interface IWizardEditable
    {
        IBinaryDataList GetGeneralSettingData();
        IBinaryDataList GetWizardData();
        IBinaryDataList GetInputs();
        IBinaryDataList GetOutputs();
    }
}
