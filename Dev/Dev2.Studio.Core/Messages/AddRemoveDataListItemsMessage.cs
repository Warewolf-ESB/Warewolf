using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Messages
{
    public class AddRemoveDataListItemsMessage : IMessage
    {
        public IDataListViewModel DataListViewModel { get; set; }

        public AddRemoveDataListItemsMessage(IDataListViewModel dataListViewModel)
        {
            DataListViewModel = dataListViewModel;
        }
    }
}