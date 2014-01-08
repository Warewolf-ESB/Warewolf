
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models.DataList
{
    public class DataListHeaderItemModel : BaseDataListItemModel
    {
        #region Ctor

        internal DataListHeaderItemModel(string displayName)
        {
            DisplayName = displayName;
        }

        #endregion Ctor

        #region Override Methods

        public override string ValidateName(string name)
        {
            return name;
        }

        #endregion Override Methods
    }
}
