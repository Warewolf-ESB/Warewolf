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
            TypeSwitch.Case<DataSplitDTO>(x => toReturn = new DataSplitDTO(initializeWith, (dto as DataSplitDTO).SplitType, (dto as DataSplitDTO).At, index, false, inserted)),
            TypeSwitch.Case<DataMergeDTO>(x => toReturn = new DataMergeDTO(initializeWith, (dto as DataMergeDTO).MergeType, (dto as DataMergeDTO).At, index, (dto as DataMergeDTO).Padding, (dto as DataMergeDTO).Alignment, inserted)),
            TypeSwitch.Case<CaseConvertTO>(x => toReturn = CaseConverterFactory.CreateCaseConverterTO(initializeWith, (dto as CaseConvertTO).ConvertType, (dto as CaseConvertTO).Result, index)),
            TypeSwitch.Case<BaseConvertTO>(x => toReturn = new BaseConvertTO(initializeWith, (dto as BaseConvertTO).FromType, (dto as BaseConvertTO).ToType, (dto as BaseConvertTO).ToExpression, index, inserted)),
            TypeSwitch.Case<GatherSystemInformationTO>(x => toReturn =
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime,
                    initializeWith, index, inserted)),
            TypeSwitch.Case<XPathDTO>(x => toReturn = new XPathDTO(initializeWith, "", index, false, inserted)),
            TypeSwitch.Case<FindRecordsTO>(() => toReturn = new FindRecordsTO("", "", index, false, inserted)),
            TypeSwitch.Default(() => toReturn = null));

            return toReturn;
        }       
    }
}
