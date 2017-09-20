namespace Dev2.Studio.Core.Models.DataList
{
    public class ItemModel
    {
        readonly bool hasError;
        readonly string errorMessage;
        readonly bool isEditable;
        readonly bool isVisable;
        readonly bool isSelected;

        public bool HasError => hasError;

        public string ErrorMessage => errorMessage;

        public bool IsEditable => isEditable;

        public bool IsVisable => isVisable;

        public bool IsSelected => isSelected;

        public ItemModel(bool _hasError, string _errorMessage, bool _isEditable, bool _isVisable, bool _isSelected)
        {
            hasError = _hasError;
            errorMessage = _errorMessage;
            isEditable = _isEditable;
            isVisable = _isVisable;
            isSelected = _isSelected;
        }

        public ItemModel(bool _hasError, string _errorMessage, bool _isEditable, bool _isVisable)
        {
            hasError = _hasError;
            errorMessage = _errorMessage;
            isEditable = _isEditable;
            isVisable = _isVisable;
            isSelected = false;
        }

        public ItemModel(bool _hasError, string _errorMessage, bool _isEditable)
        {
            hasError = _hasError;
            errorMessage = _errorMessage;
            isEditable = _isEditable;
            isVisable = true;
            isSelected = false;
        }

        public ItemModel(bool _hasError, string _errorMessage)
        {
            hasError = _hasError;
            errorMessage = _errorMessage;
            isEditable = true;
            isVisable = true;
            isSelected = false;
        }

        public ItemModel(bool _hasError)
        {
            hasError = _hasError;
            errorMessage = "";
            isEditable = true;
            isVisable = true;
            isSelected = false;
        }

        public ItemModel()
        {
            hasError = false;
            errorMessage = "";
            isEditable = true;
            isVisable = true;
            isSelected = false;
        }
    }
}
