using WW.UI.Common.Data;

namespace WW.UI.Common
{
    public class PropertyService : IPropertyService
    {
        Dictionary<string, string> _properties = new Dictionary<string, string>();

        private List<Property> PropertysList;

        public PropertyService()
        {
            _properties.Add("CustomerId", "C10001");
            _properties.Add("CustomerName", "Sachin Tendulkar");
            _properties.Add("SalesTotal", "10M");
            _properties.Add("ProductId", "F1Racer");
            _properties.Add("ProductQty", "5 pcs");

            int id = 0;
            PropertysList = _properties.Select(
                kv => new Property()
                {
                    Id = ++id,
                    Name = kv.Key,
                    Value = kv.Value
                }).ToList();

            //PropertysList = new List<Property>();

            //for (int i = 0; i < 10; i++)
            //{
            //    PropertysList.Add(new Property()
            //    { Id = i, Name = $"Property {i}", Value = $"Value{i}" });
            //}
        }

        public void DeleteProperty(int id)
        {
            var Property = PropertysList.FirstOrDefault(x => x.Id == id);
            if (Property != null)
            {
                PropertysList.Remove(Property);
            }
        }

        public Property GetPropertyById(int id)
        {
            var Property = PropertysList.SingleOrDefault(x => x.Id == id);
            return Property;
        }

        public List<Property> GetProperties()
        {
            return PropertysList.ToList();
        }

        public void SaveProperty(Property Property)
        {
            PropertysList.Add(Property);
        }
    }
}
