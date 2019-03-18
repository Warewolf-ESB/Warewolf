#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common;
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
        ObservableCollection<IComplexObjectItemModel> _children;
        IComplexObjectItemModel _parent;
        bool _isParentObject;
        bool _isArray;
        string _searchText;

        public ComplexObjectItemModel(string displayname)
            : this(displayname, null, enDev2ColumnArgumentDirection.None, "", null, false, "", true, true, false, true)
        {
        }

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent)
            : this(displayname, parent, enDev2ColumnArgumentDirection.None, "", null, false, "", true, true, false, true)
        {
        }

        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection)
            : this(displayname, parent, dev2ColumnArgumentDirection, "", null, false, "", true, true, false, true)
        {
        }

        //public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, OptomizedObservableCollection<IComplexObjectItemModel> children, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded) 
        public ComplexObjectItemModel(string displayname, IComplexObjectItemModel parent, enDev2ColumnArgumentDirection dev2ColumnArgumentDirection, string description, OptomizedObservableCollection<IComplexObjectItemModel> children, bool hasError, string errorMessage, bool isEditable, bool isVisible, bool isSelected, bool isExpanded)
            : base(displayname, dev2ColumnArgumentDirection, description, hasError, errorMessage, isEditable, isVisible, isSelected, isExpanded)
        {
            Children = children;
            Parent = parent;
            if (parent == null && !Name.StartsWith("@", StringComparison.CurrentCulture))
            {
                Name = "@" + DisplayName;
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
                if (!string.IsNullOrEmpty(_searchText) && _children != null)
                {
                    return _children.Where(model => model.IsVisible).ToObservableCollection();
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
                return _isArray || DisplayName.EndsWith("()", StringComparison.CurrentCulture);
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
                jsonString.Append("\"" + DisplayName.TrimEnd('(', ')') + "\"");
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
                var anyChildrenVisible = _children != null && _children.Any(model => model.IsVisible);
                var displayNameContainsSearchString = !string.IsNullOrEmpty(DisplayName) && DisplayName.ToLower().Contains(searchText.ToLower());
                IsVisible = anyChildrenVisible || displayNameContainsSearchString;
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

        void AppendCloseArrayChar(StringBuilder jsonString)
        {
            if (IsArray)
            {
                jsonString.Append("]");
            }
        }

        static void AppendObjectCloseChar(StringBuilder jsonString)
        {
            jsonString.Append("}");
        }

        void AppendOpenArrayChar(StringBuilder jsonString)
        {
            if (IsArray)
            {
                jsonString.Append("[");
            }
        }

        static void AppendObjectOpenChar(StringBuilder jsonString)
        {
            jsonString.Append("{");
        }

        void BuildJsonForChildren(StringBuilder jsonString)
        {
            for (int index = 0; index < Children.Count; index++)
            {
                var complexObjectItemModel = Children[index];
                jsonString.Append(complexObjectItemModel.GetJson());
                if (complexObjectItemModel.Children.Count == 0)
                {
                    jsonString.Append("\"\"");
                }
                if (index != Children.Count - 1)
                {
                    jsonString.Append(",");
                }
            }
        }

        #region Overrides of DataListItemModel

        public override string ValidateName(string name)
        {
            var nameToCheck = name.Replace("@", "");
            var isArray = name.EndsWith("()", StringComparison.CurrentCulture);
            var parser = new Dev2DataLanguageParser();
            if (!string.IsNullOrEmpty(nameToCheck))
            {
                nameToCheck = DataListUtil.RemoveRecordsetBracketsFromValue(nameToCheck);

                if (!string.IsNullOrEmpty(nameToCheck))
                {
                    ValidateName(nameToCheck, parser);
                }
            }
            if (isArray)
            {
                nameToCheck = nameToCheck + "()";
            }
            return nameToCheck;
        }

        private void ValidateName(string name, Dev2DataLanguageParser parser)
        {
            var intellisenseResult = parser.ValidateName(name, "Complex Object");
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

        #endregion

        #region Overrides of DataListItemModel

        public override string ToString() => DisplayName;

        #endregion

        public bool Equals(IComplexObjectItemModel other)
        {
            var equals = Equals(IsArray, other.IsArray);
            equals &= Equals(HasError, other.HasError);
            equals &= Equals(Input, other.Input);
            equals &= Equals(Output, other.Output);
            equals &= string.Equals(Name, other.Name);
           
            var collectionEquals = CommonEqualityOps.CollectionEquals(Children, other.Children, new ComplexObjectItemModelComparer());
            return base.Equals(other) && equals && collectionEquals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((IComplexObjectItemModel)obj);
        }

        public override int GetHashCode() => 1;
    }
}