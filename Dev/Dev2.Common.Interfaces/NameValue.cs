using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces
{
    public class NameValue : INameValue
    {
        string _name;
        string _value;

        #region Implementation of INameValue

        public  NameValue()
        {
            Name = "";
            Value = "";
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        #endregion
    }
}