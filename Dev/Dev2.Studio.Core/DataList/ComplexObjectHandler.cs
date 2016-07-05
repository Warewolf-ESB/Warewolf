﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Studio.Core.DataList
{
    public class ComplexObjectHandler: IComplexObjectHandler
    {
        private readonly DataListViewModel _vm;
        public ComplexObjectHandler(DataListViewModel vm)
        {
            _vm = vm;
        }

        #region Implementation of IComplexObjectHandler

        public void RemoveBlankComplexObjects()
        {
            var complexObjectItemModels = _vm.ComplexObjectCollection .Where(model => string.IsNullOrEmpty(model.DisplayName));
            var objectItemModels = complexObjectItemModels as IList<IComplexObjectItemModel> ?? complexObjectItemModels.ToList();
            if (objectItemModels.Count <= 1) return;
            for (var i = objectItemModels.Count; i > 0; i--)
                _vm.ComplexObjectCollection.Remove(objectItemModels[i - 1]);
        }

        public void AddComplexObject(IDataListVerifyPart part)
        {
            var paths = part.DisplayValue.Split('.');
            IComplexObjectItemModel itemModel = null;
            for (var index = 0; index < paths.Length; index++)
            {
                var path = paths[index];
                path = DataListUtil.ReplaceRecordsetIndexWithBlank(path);
                var pathToMatch = path.Replace("@", "");
                if (string.IsNullOrEmpty(pathToMatch)) return;

                var isArray = DataListUtil.IsArray(ref path);

                if (itemModel == null)
                {
                    itemModel =
                        _vm.ComplexObjectCollection.FirstOrDefault(
                            model => model.DisplayName == pathToMatch && model.IsArray == isArray);
                    if (itemModel != null) continue;
                    itemModel = new ComplexObjectItemModel(path) { IsArray = isArray };
                    _vm.ComplexObjectCollection.Add(itemModel);
                }
                else
                {
                    if (index == 0) continue;
                    var item =
                        itemModel.Children.FirstOrDefault(
                            model => model.DisplayName == pathToMatch && model.IsArray == isArray);
                    if (item == null)
                    {
                        item = new ComplexObjectItemModel(path) { Parent = itemModel, IsArray = isArray };
                        itemModel.Children.Add(item);
                    }
                    itemModel = item;
                }
            }
        }

        public IEnumerable<string> RefreshJsonObjects(IEnumerable<IComplexObjectItemModel> complexObjectItemModels)
        {
            var accList = new List<string>();
            foreach (var dataListItemModel in complexObjectItemModels)
            {
                if (string.IsNullOrEmpty(dataListItemModel.Name)) continue;
                var rec = "[[" + dataListItemModel.Name + "]]";
                accList.Add(rec);
                accList.AddRange(RefreshJsonObjects(dataListItemModel.Children.ToList()));
            }
            return accList;
        }

        public void RemoveUnusedChildComplexObjects(IComplexObjectItemModel parentItemModel, IComplexObjectItemModel itemToRemove)
        {
            for (int index = parentItemModel.Children.Count - 1; index >= 0; index--)
            {
                RemoveUnusedChildComplexObjects(parentItemModel.Children[index], itemToRemove);
                parentItemModel.Children.Remove(itemToRemove);
            }
        }

        public void SortComplexObjects(bool @ascending)
        {
            IList<IComplexObjectItemModel> newRecsetCollection = @ascending ? _vm.ComplexObjectCollection.OrderBy(c => c.DisplayName).ToList() : _vm.ComplexObjectCollection.OrderByDescending(c => c.DisplayName).ToList();
            _vm.ComplexObjectCollection.Clear();
            foreach (var item in newRecsetCollection.Where(c => !c.IsBlank))
            {
                _vm.ComplexObjectCollection.Add(item);
            }
        }

        public void GenerateComplexObjectFromJson(string parentObjectName, string json)
        {
            if (parentObjectName.Contains("."))
                parentObjectName = parentObjectName.Split('.')[0];
            var parentObj = _vm.ComplexObjectCollection.FirstOrDefault(model => model.Name == parentObjectName);
            if (parentObj == null)
            {
                parentObj = new ComplexObjectItemModel(parentObjectName);
                _vm.ComplexObjectCollection.Add(parentObj);
            }
            var objToProcess = JsonConvert.DeserializeObject(json) as JObject;
            if (objToProcess != null)
                ProcessObjectForComplexObjectCollection(parentObj, objToProcess);
        }

        public void ProcessObjectForComplexObjectCollection(IComplexObjectItemModel parentObj, JObject objToProcess)
        {
            var properties = objToProcess.Properties();
            foreach (var property in properties)
            {
                var displayname = property.Name;
                displayname = property.IsEnumerable() ? displayname + "()" : displayname;
                var childObj = parentObj.Children.FirstOrDefault(model => model.DisplayName == displayname);
                if (childObj == null)
                {
                    childObj = new ComplexObjectItemModel(displayname, parentObj) { IsArray = property.IsEnumerable() };
                    parentObj.Children.Add(childObj);
                }
                if (property.Value.IsObject())
                    ProcessObjectForComplexObjectCollection(childObj, property.Value as JObject);
                else
                {
                    if (!property.Value.IsEnumerable()) continue;
                    var arrayVal = property.Value as JArray;
                    if (arrayVal == null) continue;
                    var obj = arrayVal.FirstOrDefault() as JObject;
                    if (obj != null)
                        ProcessObjectForComplexObjectCollection(childObj, obj);
                }
            }
        }

        public string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray)
        {
            var name = isArray ? xmlNode.Name + "()" : xmlNode.Name;
            return name;
        }

      
        public void AddComplexObjectFromXmlNode(XmlNode xmlNode, ComplexObjectItemModel parent)
        {
            var isArray = false;
            var ioDirection = enDev2ColumnArgumentDirection.None;
            if (xmlNode.Attributes != null)
            {
                isArray =Common.ParseBoolAttribute(xmlNode.Attributes["IsArray"]);
                ioDirection = Common.ParseColumnIODirection(xmlNode.Attributes[GlobalConstants.DataListIoColDirection]);
            }
            var name = GetNameForArrayComplexObject(xmlNode, isArray);
            var complexObjectItemModel = new ComplexObjectItemModel(name) { IsArray = isArray, Parent = parent, ColumnIODirection = ioDirection };
            if (parent != null)
            {
                parent.Children.Add(complexObjectItemModel);
            }
            else
            {
                _vm.ComplexObjectCollection.Add(complexObjectItemModel);
            }
            if (xmlNode.HasChildNodes)
            {
                var children = xmlNode.ChildNodes;
                foreach (XmlNode childNode in children)
                {
                    AddComplexObjectFromXmlNode(childNode, complexObjectItemModel);
                }
            }
        }

        public void AddComplexObjectsToBuilder(StringBuilder result, IComplexObjectItemModel complexObjectItemModel)
        {
            result.Append("<");
            var name = complexObjectItemModel.DisplayName;
            if (complexObjectItemModel.IsArray || name.EndsWith("()"))
            {
                name = name.Replace("()", "");
            }
            result.AppendFormat("{0} {1}=\"{2}\"{3}=\"{4}\" IsJson=\"{5}\" IsArray=\"{6}\" {7}\"{8}\">"
                , name
                , Common.Description
                , complexObjectItemModel.Description
                , Common.IsEditable
                , complexObjectItemModel.IsEditable
                , true
                , complexObjectItemModel.IsArray
                , GlobalConstants.DataListIoColDirection
                , complexObjectItemModel.ColumnIODirection
                );

            var complexObjectItemModels = complexObjectItemModel.Children.Where(model => !string.IsNullOrEmpty(model.DisplayName) && !model.HasError);
            foreach (var itemModel in complexObjectItemModels)
            {
                AddComplexObjectsToBuilder(result, itemModel);
            }
            result.Append("</");
            result.Append(name);
            result.Append(">");
        }

        public void DetectUnusedComplexObjects(IEnumerable<IDataListVerifyPart> partsToVerify)
        {
            var items = _vm.ComplexObjectCollection.Flatten(model => model.Children).Where(model => !string.IsNullOrEmpty(model.DisplayName));
            var models = items as IList<IComplexObjectItemModel> ?? items.ToList();
            var unusedItems =
                from itemModel in models
                where !(from part in partsToVerify
                        select DataListUtil.ReplaceRecordsetIndexWithStar(part.DisplayValue).Replace("*", "")
                       ).Contains(DataListUtil.ReplaceRecordsetIndexWithStar(itemModel.Name).Replace("*", ""))
                select itemModel;
            foreach (var complexObjectItemModel in unusedItems)
            {
                complexObjectItemModel.IsUsed = false;
            }
            var usedItems =
                from itemModel in models
                where (from part in partsToVerify
                       select DataListUtil.ReplaceRecordsetIndexWithStar(part.DisplayValue).Replace("*", "")
                      ).Contains(DataListUtil.ReplaceRecordsetIndexWithStar(itemModel.Name).Replace("*", ""))
                select itemModel;
            foreach (var complexObjectItemModel in usedItems)
            {
                if (complexObjectItemModel.Parent != null)
                {
                    SetComplexObjectParentIsUsed(complexObjectItemModel);
                }
                else
                {
                    complexObjectItemModel.IsUsed = true;
                }
            }
            foreach (var complexObjectItemModel in _vm.ComplexObjectCollection)
            {
                var getChildrenToCheck = complexObjectItemModel.Children.Flatten(model => model.Children).Where(model => !string.IsNullOrEmpty(model.DisplayName));
                complexObjectItemModel.IsUsed = getChildrenToCheck.All(model => model.IsUsed) && complexObjectItemModel.IsUsed;
            }
        }

        public void SetComplexObjectParentIsUsed(IComplexObjectItemModel complexObjectItemModel)
        {
            complexObjectItemModel.IsUsed = true;
            if (complexObjectItemModel.Parent != null)
            {
                SetComplexObjectParentIsUsed(complexObjectItemModel.Parent);
            }
        }
        #endregion
    }
}
