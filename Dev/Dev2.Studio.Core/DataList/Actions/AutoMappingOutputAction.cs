using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.UndoFramework;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.DataList.Actions
{
    public class AutoMappingOutputAction : AbstractAction, IAutoMappingOutputAction
    {

        readonly IDataMappingViewModel _beforeAutoMapping;
        readonly IDataMappingViewModel _copyOfBeforeAutoMapping;

        public AutoMappingOutputAction(IDataMappingViewModel beforeAutoMapping, IWebActivity activity)
        {
            _beforeAutoMapping = beforeAutoMapping;
            _copyOfBeforeAutoMapping = new DataMappingViewModel(activity);


        }

        protected override void ExecuteCore()
        {
            _copyOfBeforeAutoMapping.CopyFrom(_beforeAutoMapping);

            foreach(IInputOutputViewModel item in _beforeAutoMapping.Outputs)
            {
                IInputOutputViewModel tempItem = LoadOutputAutoMapping(item);
                item.Value = tempItem.Value;
                item.MapsTo = tempItem.MapsTo;
            }
            _beforeAutoMapping.CreateXmlOutput(_beforeAutoMapping.Outputs, _beforeAutoMapping.Inputs);
        }


        public IInputOutputViewModel LoadOutputAutoMapping(IInputOutputViewModel item)
        {
            string value = string.Empty;
            if(item.Value == string.Empty)
            {
                IDataListItemModel recset = DataListSingleton.ActiveDataList.RecsetCollection.FirstOrDefault(x => x.Name == item.RecordSetName);
                if(recset != null)
                {
                    var val = recset.Children.FirstOrDefault(x => x.DisplayName == item.DisplayName);
                    if(val != null)
                    {
                        value = val.DisplayName;
                    }

                    if(value != null)
                    {
                        item.Value = value;
                    }
                }
                else
                {
                    IDataListItemModel scalar = DataListSingleton.ActiveDataList.ScalarCollection.FirstOrDefault(x => x.Name == item.DisplayName);
                    if(scalar != null)
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
