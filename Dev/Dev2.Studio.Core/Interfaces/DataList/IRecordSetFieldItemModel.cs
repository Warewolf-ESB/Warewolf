namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IRecordSetFieldItemModel : IDataListItemModel
    {
        IRecordSetItemModel Parent { get; set; }

        //string DisplayName { get; set; }

        string ValidateName(string name);
    }
}