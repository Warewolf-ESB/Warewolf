using System;
using System.Collections.ObjectModel;
using System.Text;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Models.DataList
{
    public class ComplexObjectItemModel : DataListItemModel, IComplexObjectItemModel
    {
        private ObservableCollection<IComplexObjectItemModel> _children;
        private IComplexObjectItemModel _parent;
        private bool _isParentObject;
        private bool _isArray;

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent = null, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection = enDev2ColumnArgumentDirection.None, string description = "", OptomizedObservableCollection<IComplexObjectItemModel> children = null, bool hasError = false, string errorMessage = "", bool isEditable = true, bool isVisible = true, bool isSelected = false, bool isExpanded = true) 
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