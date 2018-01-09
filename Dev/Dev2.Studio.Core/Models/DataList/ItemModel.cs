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
        
        public ItemModel(bool _isEditable)
        {
            hasError = false;
            errorMessage = "";
            isEditable = _isEditable;
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
