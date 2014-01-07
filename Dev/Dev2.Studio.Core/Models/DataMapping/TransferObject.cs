using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Models
{
    public class TransferObject
    {

        public IList<IDev2Definition> outputScalarList { get; set; }
        public IList<IDev2Definition> inputScalarList { get; set; }
        public IRecordSetCollection outputRecSet { get; set; }
        public IRecordSetCollection inputRecSet { get; set; }
        public List<IDataListItemModel> dataList { get; set; }
        public List<IDataListItemModel> dataObjectNames { get; set; }
        public IList<IInputOutputViewModel> displayOutputData { get; set; }
        public IList<IInputOutputViewModel> displayInputData { get; set; }
    }
}
