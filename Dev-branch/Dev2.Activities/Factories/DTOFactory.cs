using Dev2;
using Dev2.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DTOFactory
    {

        public ActivityDTO CreateNewDTO(ActivityDTO dto)
        {
            return new ActivityDTO("", "", 0);
        }

        public DataSplitDTO CreateNewDTO(DataSplitDTO dto)
        {
            return new DataSplitDTO("", "Index", "", 0);
        }

        public DataMergeDTO CreateNewDTO(DataMergeDTO dto)
        {
            return new DataMergeDTO("", "None", "", 0, "", "Left");
        }

        public ICaseConvertTO CreateNewDTO(CaseConvertTO dto)
        {
            return CaseConverterFactory.CreateCaseConverterTO("", "UPPER", "", 0);
        }

        public BaseConvertTO CreateNewDTO(BaseConvertTO dto)
        {
            return new BaseConvertTO("", "Text", "Base 64", "", 0);
        }
    }
}
