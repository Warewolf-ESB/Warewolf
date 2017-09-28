using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class ComplexObjectItemModel : DataListItemModel, IComplexObjectItemModel
    {
        private ObservableCollection<IComplexObjectItemModel> _children;
        private IComplexObjectItemModel _parent;
        private bool _isParentObject;
        private bool _isArray;
        private string _searchText;

        public ComplexObjectItemModel(string displayname)
            : this(displayname, null, enDev2ColumnArgumentDirection.None, "", null, false, "", true, true, false, true)
        {
        }

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent)
            : this(displayname, parent, enDev2ColumnArgumentDirection.None, "", null, false, "", true, true, false, true)
        {
        }

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, OptomizedObservableCollection<IComplexObjectItemModel> children, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded) 
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Children = children;
            Parent = parent;
            if (parent == null)
            {
                if (!Name.StartsWith("@"))
                {
                    Name = "@" + DisplayName;
                }
            }
        }

        public override bool Input
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both
                       || _columnIODir == enDev2ColumnArgumentDirection.Input;
            }
            set
            {
                SetColumnIODirectionFromInput(value);
            }
        }

        public override bool Output
        {
            get
            {
                return _columnIODir == enDev2ColumnArgumentDirection.Both || _columnIODir == enDev2ColumnArgumentDirection.Output;
            }
            set
            {
                SetColumnIODirectionFromOutput(value);
            }
        }

        public ObservableCollection<IComplexObjectItemModel> Children
        {
            get
            {
                if (!string.IsNullOrEmpty(_searchText))
                {
                    if (_children != null)
                    {
                        return _children.Where(model => model.IsVisible).ToObservableCollection();
                    }
                }
                return _children ?? (_children = new ObservableCollection<IComplexObjectItemModel>());
            }
            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }
        public bool IsArray
        {
            get
            {
                return _isArray || DisplayName.EndsWith("()");
            }
            set
            {
                _isArray = value;
            }
        }
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

        public string GetJson()
        {
            var jsonString = new StringBuilder();
            if (!string.IsNullOrEmpty(DisplayName))
            {
                if (Parent == null)
                {
                    AppendObjectOpenChar(jsonString);
                }
                jsonString.Append("\"" + DisplayName.TrimEnd('(',')') + "\"");
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

        public void Filter(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                if (_children != null)
                {
                    foreach (var itemModel in _children)
                    {
                        itemModel.Filter(searchText);
                    }
                }
                IsVisible = _children != null && _children.Any(model => model.IsVisible) ? true : !string.IsNullOrEmpty(DisplayName) && DisplayName.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                IsVisible = true;
                if (_children != null)
                {
                    foreach (var itemModel in _children)
                    {
                        itemModel.IsVisible = true;
                    }
                }
            }
            _searchText = searchText;
            NotifyOfPropertyChange(() => Children);
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

        #region Overrides of DataListItemModel

        public override string ValidateName(string name)
        {
            var nameToCheck  = name.Replace("@", "");
            var isArray = name.EndsWith("()");
            Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(nameToCheck))
            {
                nameToCheck = DataListUtil.RemoveRecordsetBracketsFromValue(nameToCheck);

                if (!string.IsNullOrEmpty(nameToCheck))
                {
                    var intellisenseResult = parser.ValidateName(nameToCheck, "Complex Object");
                    if (intellisenseResult != null)
                    {
                        SetError(intellisenseResult.Message);
                    }
                    else
                    {
                        if (!string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateValue, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateVariable, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageDuplicateRecordset, StringComparison.InvariantCulture) &&
                            !string.Equals(ErrorMessage, StringResources.ErrorMessageEmptyRecordSet, StringComparison.InvariantCulture))
                        {
                            RemoveError();
                        }
                    }
                }
            }
            if (isArray)
            {
                nameToCheck = nameToCheck + "()";
            }
            return nameToCheck;
        }

        #endregion

        #region Overrides of DataListItemModel

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }
}