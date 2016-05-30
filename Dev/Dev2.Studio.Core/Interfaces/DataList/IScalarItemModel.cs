namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IScalarItemModel : IDataListItemModel
    {
        //string DisplayName { get; set; }

        string ValidateName(string name);
    }
}