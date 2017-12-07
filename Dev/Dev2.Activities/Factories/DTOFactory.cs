/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.TO;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public static class DTOFactory
    {
        public static IDev2TOFn CreateNewDTO(IDev2TOFn dto) => CreateNewDTO(dto, 0, false, "");
        public static IDev2TOFn CreateNewDTO(IDev2TOFn dto, int index, bool inserted, string initializeWith)
        {
            IDev2TOFn toReturn = null;

            TypeSwitch.Do(dto,
                
                TypeSwitch.Case<ActivityDto>(x => toReturn = new ActivityDto(initializeWith, "", index, inserted)),
                
                TypeSwitch.Case<DataSplitDto>(x =>
                {
                    var dataSplitDto = dto as DataSplitDto;
                    if (dataSplitDto != null)
                    {
                        toReturn = new DataSplitDto(initializeWith, dataSplitDto.SplitType, dataSplitDto.At, index, false, inserted);
                    }
                }),
                TypeSwitch.Case<DataMergeDTO>(x =>
                {
                    var dataMergeDto = dto as DataMergeDTO;
                    if (dataMergeDto != null)
                    {
                        toReturn = new DataMergeDTO(initializeWith, dataMergeDto.MergeType, dataMergeDto.At, index, dataMergeDto.Padding, dataMergeDto.Alignment, inserted);
                    }
                }),
                TypeSwitch.Case<CaseConvertTO>(x =>
                {
                    var caseConvertTO = dto as CaseConvertTO;
                    if (caseConvertTO != null)
                    {
                        toReturn = CaseConverterFactory.CreateCaseConverterTO(initializeWith, caseConvertTO.ConvertType, caseConvertTO.Result, index);
                    }
                }),
                TypeSwitch.Case<BaseConvertTO>(x =>
                {
                    var baseConvertTO = dto as BaseConvertTO;
                    if (baseConvertTO != null)
                    {
                        toReturn = new BaseConvertTO(initializeWith, baseConvertTO.FromType, baseConvertTO.ToType, baseConvertTO.ToExpression, index, inserted);
                    }
                }),
                
                TypeSwitch.Case<GatherSystemInformationTO>(x => toReturn =
                    new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime,
                        initializeWith, index, inserted)),
                TypeSwitch.Case<XPathDto>(x => toReturn = new XPathDto(initializeWith, "", index, inserted)),
                TypeSwitch.Case<FindRecordsTO>(() => toReturn = new FindRecordsTO("", "", index, inserted)),
                TypeSwitch.Case<DecisionTO>(() => toReturn = new DecisionTO(initializeWith, "", "", index, inserted)),
                TypeSwitch.Case<JsonMappingTo>(() => toReturn = new JsonMappingTo(initializeWith, index, inserted)),
                TypeSwitch.Case<SharepointSearchTo>(() => toReturn = new SharepointSearchTo(initializeWith, "=", "", index, inserted)),
                TypeSwitch.Case<SharepointReadListTo>(() => toReturn = new SharepointReadListTo("", initializeWith, "", "")),
                TypeSwitch.Case<AssignObjectDTO>(x => toReturn = new AssignObjectDTO(initializeWith, "", index, inserted)),
            TypeSwitch.Default(() => toReturn = null));

            return toReturn;
        }
    }
}