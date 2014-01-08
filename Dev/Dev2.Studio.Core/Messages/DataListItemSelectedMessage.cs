using Dev2.Studio.Core.Interfaces.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DataListItemSelectedMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DataListItemSelectedMessage(IDataListItemModel dataListItemModel)
        {
            DataListItemModel = dataListItemModel;
        }

        public IDataListItemModel DataListItemModel { get; set; }
    }
}