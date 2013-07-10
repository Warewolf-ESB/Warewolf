using Dev2;
using Dev2.Data.Enums;
using Dev2.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DTOFactory
    {

        public ActivityDTO CreateNewDTO(ActivityDTO dto,bool inserted = false)
        {
            return new ActivityDTO("", "", 0, inserted);
        }

        public DataSplitDTO CreateNewDTO(DataSplitDTO dto, bool inserted = false)
        {
            return new DataSplitDTO("", "Index", "", 0, false, inserted);
        }

        public DataMergeDTO CreateNewDTO(DataMergeDTO dto, bool inserted = false)
        {
            return new DataMergeDTO("", "None", "", 0, "", "Left", inserted);
        }

        public ICaseConvertTO CreateNewDTO(CaseConvertTO dto, bool inserted = false)
        {
            return CaseConverterFactory.CreateCaseConverterTO("", "UPPER", "", 0);
        }

        public BaseConvertTO CreateNewDTO(BaseConvertTO dto, bool inserted = false)
        {
            return new BaseConvertTO("", "Text", "Base 64", "", 0, inserted);
        }

        public GatherSystemInformationTO CreateNewDTO(GatherSystemInformationTO dto, bool inserted = false)
        {
            return new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, 0, inserted);
        }

        public XPathDTO CreateNewDTO(XPathDTO dto,bool inserted = false)
        {
            return new XPathDTO("", "", 0,false, inserted);
        }

    }
}
