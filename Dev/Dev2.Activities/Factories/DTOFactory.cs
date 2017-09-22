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
        public static IDev2TOFn CreateNewDTO(ActivityDTO dto, int index, bool inserted, string initializeWith)
        {
            return new ActivityDTO(initializeWith, "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(DataSplitDTO dto, int index, bool inserted, string initializeWith)
        {
            return new DataSplitDTO(initializeWith, dto.SplitType, dto.At, index, false, inserted);
        }

        public static IDev2TOFn CreateNewDTO(DataMergeDTO dto, int index, bool inserted, string initializeWith)
        {
            return new DataMergeDTO(initializeWith, dto.MergeType, dto.At, index, dto.Padding, dto.Alignment, inserted);
        }

        public static IDev2TOFn CreateNewDTO(CaseConvertTO dto, int index, bool inserted, string initializeWith)
        {
            return CaseConverterFactory.CreateCaseConverterTO(initializeWith, dto.ConvertType, dto.Result, index);
        }

        public static IDev2TOFn CreateNewDTO(BaseConvertTO dto, int index, bool inserted, string initializeWith)
        {
            return new BaseConvertTO(initializeWith, dto.FromType, dto.ToType, dto.ToExpression, index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(GatherSystemInformationTO dto, int index, bool inserted, string initializeWith)
        {
            return new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, initializeWith, index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(XPathDTO dto, int index, bool inserted, string initializeWith)
        {
            return new XPathDTO(initializeWith, "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(FindRecordsTO dto, int index, bool inserted, string initializeWith)
        {
            return new FindRecordsTO("", "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(DecisionTO dto, int index, bool inserted, string initializeWith)
        {
            return new DecisionTO(initializeWith, "", "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(JsonMappingTo dto, int index, bool inserted, string initializeWith)
        {
            return new JsonMappingTo(initializeWith, index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(SharepointSearchTo dto, int index, bool inserted, string initializeWith)
        {
            return new SharepointSearchTo(initializeWith, "=", "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(SharepointReadListTo dto, int index, bool inserted, string initializeWith)
        {
            return new SharepointReadListTo("", initializeWith, "", "");
        }

        public static IDev2TOFn CreateNewDTO(AssignObjectDTO dto, int index, bool inserted, string initializeWith)
        {
            return new AssignObjectDTO(initializeWith, "", index, inserted);
        }

        public static IDev2TOFn CreateNewDTO(IDev2TOFn dto, int index, bool inserted, string initializeWith)
        {
            return null;
        }

        public static IDev2TOFn CreateNewDTO(IDev2TOFn dto) => CreateNewDTO(dto, 0, false, "");
    }
}