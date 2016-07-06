using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces.DataList;
using ServiceStack.Common.Extensions;

namespace Dev2.Studio.Core.DataList
{
    public static class Common
    {
        public const string RootTag = "DataList";
        public const string Description = "Description";
        public const string IsEditable = "IsEditable";
        public static bool ParseIsEditable(XmlAttribute attr)
        {
            return ParseBoolAttribute(attr);
        }
        public static void CheckDataListItemsForDuplicates(IEnumerable<IDataListItemModel> itemsToCheck)
        {
            List<IGrouping<string, IDataListItemModel>> duplicates = itemsToCheck.ToLookup(x => x.DisplayName).ToList();
            foreach (var duplicate in duplicates)
            {
                if (duplicate.Count() > 1 && !String.IsNullOrEmpty(duplicate.Key))
                {
                    duplicate.ForEach(model => model.SetError(StringResources.ErrorMessageDuplicateValue));
                }
                else
                {
                    duplicate.ForEach(model =>
                    {
                        if (model.ErrorMessage != null && model.ErrorMessage.Contains(StringResources.ErrorMessageDuplicateValue))
                        {
                            model.RemoveError();
                        }
                    });
                }
            }
        }

        public static string ParseDescription(XmlAttribute attr)
        {
            var result = string.Empty;
            if (attr != null)
            {
                result = attr.Value;
            }
            return result;
        }

        public static bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = true;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
            }
            return result;
        }
        // ReSharper disable InconsistentNaming
        public static enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
            // ReSharper restore InconsistentNaming
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr == null)
            {
                return result;
            }
            if (!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }
    }
}