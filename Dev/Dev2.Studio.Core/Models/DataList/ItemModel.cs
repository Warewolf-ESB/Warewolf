namespace Dev2.Studio.Core.Models.DataList
{
    public class ItemModel
    {
        public bool hasError;
        public string errorMessage;
        public bool isEditable;
        public bool isVisable;
        public bool isSelected;

        public ItemModel(bool _hasError = false, string _errorMessage = "", bool _isEditable = true, bool _isVisable = true, bool _isSelected = false)
        {
            hasError = _hasError;
            errorMessage = _errorMessage;
            isEditable = _isEditable;
            isVisable = _isVisable;
            isSelected = _isSelected;
        }
    }
}
