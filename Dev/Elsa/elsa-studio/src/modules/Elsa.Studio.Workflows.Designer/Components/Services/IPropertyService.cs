using WW.UI.Common.Data;

namespace WW.UI.Common
{
    public interface IPropertyService
    {
        List<Property> GetProperties();
        Property GetPropertyById(int id);
        void SaveProperty(Property Property);
        void DeleteProperty(int id);
    }
}
