using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.Studio.Core;

namespace Dev2.Activities.Designers2.Core
{
    public class ActionInputDatalistMapper : IActionInputDatatalistMapper
    {
        readonly IActiveDataList _activeDataList;
        public ActionInputDatalistMapper()
            :this(DataListSingleton.Instance)
        {
        }

        public ActionInputDatalistMapper(IActiveDataList activeDataList)
        {
            _activeDataList = activeDataList;
        }

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

        private void MapInput(IServiceInput serviceInput)
        {
            if (_activeDataList.ActiveDataList != null)
            {
                MapInputToActiveDatalist(serviceInput);
            }
        }

        private void MapInputToActiveDatalist(IServiceInput serviceInput)
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

        private void MapToObject(IServiceInput serviceInput)
        {
            if (_activeDataList.ActiveDataList.ComplexObjectCollection != null)
            {
                var value = serviceInput.Name;
                if (value != null)
                {
                    value = value.Split('(').First().TrimEnd(' ');
                    var objectName = "@" + value;
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(objectName);
                    serviceInput.Value = variable;
                }
            }
        }

        private void MapToScalar(IServiceInput serviceInput)
        {
            if (_activeDataList.ActiveDataList.ScalarCollection != null)
            {
                var value = serviceInput.Name;
                if (value != null)
                {
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(value.Split('(').First().TrimEnd(' '));
                    serviceInput.Value = variable;
                }
            }
        }
    }
}