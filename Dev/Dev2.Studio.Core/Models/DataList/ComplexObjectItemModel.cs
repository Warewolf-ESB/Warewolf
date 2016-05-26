using System.Collections.ObjectModel;
using System.Text;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class ComplexObjectItemModel : DataListItemModel, IComplexObjectItemModel
    {
        private ObservableCollection<IComplexObjectItemModel> _children;
        private IComplexObjectItemModel _parent;
        private bool _isParentObject;

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent = null, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", OptomizedObservableCollection<IComplexObjectItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Children = children;
            Parent = parent;
        }

        public ObservableCollection<IComplexObjectItemModel> Children
        {
            get
            {
                return _children ?? (_children = new ObservableCollection<IComplexObjectItemModel>());
            }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }
        public bool IsArray { get; set; }
        public IComplexObjectItemModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                if (_parent != null && !string.IsNullOrEmpty(DisplayName))
                {
                    Name = _parent.Name + "." + DisplayName;
                }
                IsParentObject = _parent == null;
            }
        }
        public bool IsParentObject
        {
            get
            {
                return _isParentObject;
            }
            set
            {
                _isParentObject = value;
                NotifyOfPropertyChange(() => IsParentObject);
            }
        }
        
      
        

      
        //public bool IsCheckBoxVisible
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}


     

//        public void Filter(string searchText)
//        {
//            Children.Clear();
//            if (_backupChildren != null)
//            {
//                foreach (IComplexObjectItemModel recordSetFieldItemModel in _backupChildren)
//                {
//                    Children.Add(recordSetFieldItemModel);
//                }
//            }
//
//            if (string.IsNullOrEmpty(searchText))
//            {
//                return;
//            }
//
//            if (!String.IsNullOrEmpty(searchText))
//            {
//                _backupChildren = _backupChildren ?? new ObservableCollection<IComplexObjectItemModel>();
//                foreach (var dataListItemModel in Children)
//                {
//                    _backupChildren.Add(dataListItemModel);
//                }
//            }
//
//            _backupChildren = Children;
//            Children = new ObservableCollection<IComplexObjectItemModel>(Children.Where(a => a.DisplayName.ToUpper().Contains(searchText.ToUpper())));
//        }

   

        //public string ValidateName(string name)
        //{
        //    Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
        //    if (!string.IsNullOrEmpty(name))
        //    {
        //            name = DataListUtil.RemoveRecordsetBracketsFromValue(name);
                
        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            var intellisenseResult = parser.ValidateName(name, "Recordset");
        //            if (intellisenseResult != null)
        //            {
        //                SetError(intellisenseResult.Message);
        //            }
        //            else
        //            {
        //                if (!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) &&
        //                    !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateVariable, StringComparison.InvariantCulture) &&
        //                    !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateRecordset, StringComparison.InvariantCulture) &&
        //                    !string.Equals(ErrorMessage, StringResources.ErrorMessageEmptyRecordSet, StringComparison.InvariantCulture))
        //                {
        //                    RemoveError();
        //                }
        //            }
        //        }
        //    }
        //    return name;
        //}

        public string GetJson()
        {
            var jsonString = new StringBuilder();
            if (!string.IsNullOrEmpty(DisplayName))
            {
                if (Parent == null)
                {
                    AppendObjectOpenChar(jsonString);
                }
                jsonString.Append("\"" + DisplayName + "\"");
                jsonString.Append(":");
                if (Children.Count > 0)
                {
                    AppendOpenArrayChar(jsonString);
                    AppendObjectOpenChar(jsonString);
                    BuildJsonForChildren(jsonString);
                    AppendObjectCloseChar(jsonString);
                    AppendCloseArrayChar(jsonString);
                }
                if (Parent == null)
                {
                    AppendObjectCloseChar(jsonString);
                }
            }
            return jsonString.ToString();
        }

        private void AppendCloseArrayChar(StringBuilder jsonString)
        {
            if(IsArray)
            {
                jsonString.Append("]");
            }
        }

        private static void AppendObjectCloseChar(StringBuilder jsonString)
        {
            jsonString.Append("}");
        }

        private void AppendOpenArrayChar(StringBuilder jsonString)
        {
            if(IsArray)
            {
                jsonString.Append("[");
            }
        }

        private static void AppendObjectOpenChar(StringBuilder jsonString)
        {
            jsonString.Append("{");
        }

        private void BuildJsonForChildren(StringBuilder jsonString)
        {
            for(int index = 0; index < Children.Count; index++)
            {
                var complexObjectItemModel = Children[index];
                jsonString.Append(complexObjectItemModel.GetJson());
                if(complexObjectItemModel.Children.Count == 0)
                {
                    jsonString.Append("\"\"");
                }
                if(index != Children.Count - 1)
                {
                    jsonString.Append(",");
                }
            }
        }
    }
}