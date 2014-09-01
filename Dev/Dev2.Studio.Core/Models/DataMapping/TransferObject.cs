using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class TransferObject
    {

        public IList<IDev2Definition> OutputScalarList { get; set; }
        public IList<IDev2Definition> InputScalarList { get; set; }
        public IRecordSetCollection OutputRecSet { get; set; }
        public IRecordSetCollection InputRecSet { get; set; }
        public List<IDataListItemModel> DataList { get; set; }
        public List<IDataListItemModel> DataObjectNames { get; set; }
        public IList<IInputOutputViewModel> DisplayOutputData { get; set; }
        public IList<IInputOutputViewModel> DisplayInputData { get; set; }
    }
}
