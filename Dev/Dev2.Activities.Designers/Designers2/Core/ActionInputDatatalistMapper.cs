using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.Studio.Core;

namespace Dev2.Activities.Designers2.Core
{
    public class ActionInputDatatalistMapper : IActionInputDatatalistMapper
    {
        #region Implementation of IActionInputDatatalistMapper

        public void MapInputsToDatalist(IEnumerable<IServiceInput> inputs)
        {
            if (inputs != null)
            {
                foreach (var serviceInput in inputs)
                {
                    if (!string.IsNullOrEmpty(serviceInput.Value))
                    {
                        continue;
                    }
                    MapInput(serviceInput);
                }
            }
        }

        private static void MapInput(IServiceInput serviceInput)
        {
            if (DataListSingleton.ActiveDataList != null)
            {
                MapInputToActiveDatalist(serviceInput);
            }
        }

        private static void MapInputToActiveDatalist(IServiceInput serviceInput)
        {
            if (!serviceInput.IsObject)
            {
                MapToScalar(serviceInput);
            }
            else
            {
                MapToObject(serviceInput);
            }
        }

        private static void MapToObject(IServiceInput serviceInput)
        {
            if (DataListSingleton.ActiveDataList.ComplexObjectCollection != null)
            {
                var value = serviceInput?.Name;
                if (value != null)
                {
                    value = value.Split('(').First().TrimEnd(' ');
                    var objectName = "@" + value;
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(objectName);
                    serviceInput.Value = variable;
                }
            }
        }

        private static void MapToScalar(IServiceInput serviceInput)
        {
            if (DataListSingleton.ActiveDataList.ScalarCollection != null)
            {
                var value = serviceInput?.Name;
                if (value != null)
                {
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(value.Split('(').First().TrimEnd(' '));
                    serviceInput.Value = variable;
                }
            }
        }

        #endregion
    }
}