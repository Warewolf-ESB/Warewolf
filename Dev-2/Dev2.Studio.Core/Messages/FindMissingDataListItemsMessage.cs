using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Messages
{
    public class FindMissingDataListItemsMessage : IMessage
    {
        public IDataListViewModel DataListViewModel { get; set; }

        public FindMissingDataListItemsMessage(IDataListViewModel dataListViewModel)
        {
            DataListViewModel = dataListViewModel;
        }
    }
}