namespace Dev2.Studio.Interfaces.DataList
{
    public interface IScalarItemModel : IDataListItemModel
    {
    
        string ValidateName(string name);

        void Filter(string searchText);
    }
}