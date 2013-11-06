using Dev2;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Dev2.Studio.Core.Helpers;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DTOFactory
    {

        public static IDev2TOFn CreateNewDTO(IDev2TOFn dto, int index = 0, bool inserted = false, string initializeWith = "")
        {
            IDev2TOFn toReturn = null;

            TypeSwitch.Do(dto,
            TypeSwitch.Case<ActivityDTO>(x => toReturn = new ActivityDTO(initializeWith, "", index, inserted)),
            TypeSwitch.Case<DataSplitDTO>(x => toReturn = new DataSplitDTO(initializeWith, "Index", "", index, false, inserted)),
            TypeSwitch.Case<DataMergeDTO>(x => toReturn = new DataMergeDTO(initializeWith, "None", "", index, "", "Left", inserted)),
            TypeSwitch.Case<CaseConvertTO>(x => toReturn = CaseConverterFactory.CreateCaseConverterTO(initializeWith, "UPPER", "", index)),
            TypeSwitch.Case<BaseConvertTO>(x => toReturn = new BaseConvertTO(initializeWith, "Text", "Base 64", "", index, inserted)),
            TypeSwitch.Case<GatherSystemInformationTO>(x => toReturn = 
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime,
                    initializeWith, index, inserted)),
            TypeSwitch.Case<XPathDTO>(x => toReturn = new XPathDTO(initializeWith, "", index, false, inserted)),
            TypeSwitch.Case<FindRecordsTO>(() => toReturn=new FindRecordsTO("","",index,false,inserted)),
            TypeSwitch.Default(() => toReturn =null));

            return toReturn;
        }
    }
}
