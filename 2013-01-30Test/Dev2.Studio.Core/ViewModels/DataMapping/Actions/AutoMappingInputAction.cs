using System.ComponentModel.Composition;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Actions
{
    public class AutoMappingInputAction : AbstractAction, IAutoMappingInputAction
    {

        readonly IDataMappingViewModel _beforeAutoMapping;
        readonly IDataMappingViewModel _copyOfBeforeAutoMapping;
        IWebActivity _activity;

        public AutoMappingInputAction(IDataMappingViewModel beforeAutoMapping, IWebActivity activity)
        {
            _beforeAutoMapping = beforeAutoMapping;
            _activity = activity;
            _copyOfBeforeAutoMapping = new DataMappingViewModel(activity);

        }

        protected override void ExecuteCore()
        {

            if (_copyOfBeforeAutoMapping.MainViewModel == null)
            {
                _copyOfBeforeAutoMapping.MainViewModel = _beforeAutoMapping.MainViewModel;
            }

            _copyOfBeforeAutoMapping.CopyFrom(_beforeAutoMapping);
            foreach (IInputOutputViewModel item in _beforeAutoMapping.Inputs)
            {

                IInputOutputViewModel tempItem = LoadInputAutoMapping(item);
                item.Value = tempItem.Value;
                item.MapsTo = tempItem.Value;

            }
            _beforeAutoMapping.CreateXmlOutput(_beforeAutoMapping.Outputs, _beforeAutoMapping.Inputs);
        }

        public IInputOutputViewModel LoadInputAutoMapping(IInputOutputViewModel item)
        {
            string _value = string.Empty;
            if (item.Value == string.Empty)
            {
                IDataListItemModel recset = DataListSingleton.ActiveDataList.RecsetCollection.FirstOrDefault(x => x.Name == item.RecordSetName);
                if (recset != null)
                {
                    var val = recset.Children.FirstOrDefault(x => x.DisplayName == item.DisplayName);
                    if(val != null)
                    {
                        _value = val.DisplayName;
                    }
                    
                    if (_value != null)
                    {
                        item.Value = _value;
                    }
                }
                else
                {
                    IDataListItemModel scalar = DataListSingleton.ActiveDataList.ScalarCollection.FirstOrDefault(x => x.Name == item.DisplayName);                  
                    if (scalar != null)
                    {
                        item.Value = scalar.DisplayName;
                    }
                }
            }
            return item;
        }

        protected override void UnExecuteCore()
        {
            _beforeAutoMapping.CopyFrom(_copyOfBeforeAutoMapping);
            _beforeAutoMapping.CreateXmlOutput(_beforeAutoMapping.Outputs, _beforeAutoMapping.Inputs);
        }
    }
}
