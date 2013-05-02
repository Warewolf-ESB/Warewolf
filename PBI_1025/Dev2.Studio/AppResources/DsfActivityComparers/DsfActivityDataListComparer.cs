using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework;
using System.Collections.ObjectModel;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio {
    public class DsfActivityDataListComparer {

        public static List<string> ContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            switch (objectToCheck.ItemType.Name) {
                case "DsfWebPageActivity":
                case "DsfActivity":
                return DsfActivityContainsDataListItem(objectToCheck, dataListItem);
                case "DsfAssignActivity":
                return DsfAssignActivityContainsDataListItem(objectToCheck, dataListItem);
                case "DsfForEachActivity":
                return DsfForEachActivityContainsDataListItem(objectToCheck, dataListItem);                
                case "DsfTransformActivity":
                return DsfTransformActivityContainsDataListItem(objectToCheck, dataListItem);
                case "DsfFileForEachActivity":
                return DsfFileForEachActivityContainsDataListItem(objectToCheck, dataListItem);
                case "DsfMultiAssignActivity":
                return DsfMultiAssignActivityContainsDataListItem(objectToCheck, dataListItem);
                case "DsfCountRecordsetActivity":
                return DsfCountRecordsetActivityContainsDataListItem(objectToCheck, dataListItem);
                default:
                throw new InvalidOperationException("Unexpected Dev2 activity encountered");
            }
        }

        static List<string> DsfTransformActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);

            var transformationProperty = objectToCheck.Properties["Transformation"];
            var transformationElementNameProperty = objectToCheck.Properties["TransformElementName"];
            var rootTagProperty = objectToCheck.Properties["RootTag"];

            if (transformationProperty != null) {
                var transformation = transformationProperty.ComputedValue == null ? string.Empty : transformationProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(transformation) && !string.IsNullOrEmpty(comparisonValue)) {
                    var verifyedParts = BuildParts(transformation);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("Transformation");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("Transformation");
                                break;
                            }
                        }
                    }
                }
            }

            if (transformationElementNameProperty != null) {
                var transformationElementName = transformationElementNameProperty.ComputedValue == null ? string.Empty : transformationElementNameProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(transformationElementName) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!transformationElementName.StartsWith("[[") && !transformationElementName.EndsWith("]]")) {
                        transformationElementName = "[[" + transformationElementName + "]]";
                    }
                    var verifyedParts = BuildParts(transformationElementName);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("TransformElementName");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("TransformElementName");
                                break;
                            }
                        }
                    }
                }
            }

            if (rootTagProperty != null) {
                var rootTag = rootTagProperty.ComputedValue == null ? string.Empty : rootTagProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(rootTag) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!rootTag.StartsWith("[[") && !rootTag.EndsWith("]]")) {
                        rootTag = "[[" + rootTag + "]]";
                    }
                    var verifyedParts = BuildParts(rootTag);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("RootTag");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("RootTag");
                                break;
                            }
                        }
                    }
                }
            }

            return containingFeilds;
        }

        //static List<string> DsfNewForEachActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
        //    List<string> containingFeilds = new List<string>();
        //    string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);

        //    var forEachElementNameProperty = objectToCheck.Properties["ForEachElementName"];
        //    var additionalDataProperty = objectToCheck.Properties["AdditionalData"];

        //    if (forEachElementNameProperty != null) {
        //        var foreachElementName = forEachElementNameProperty.ComputedValue == null ? string.Empty : forEachElementNameProperty.ComputedValue.ToString();
        //        if (!string.IsNullOrEmpty(foreachElementName) && !string.IsNullOrEmpty(comparisonValue)) {
        //            if (foreachElementName.StartsWith("[[") && foreachElementName.EndsWith("]]")) {
        //                var verifyedParts = BuildParts(foreachElementName);
        //                foreach (string item in verifyedParts) {
        //                    string test = dataListItem.DisplayName;
        //                    string test2 = dataListItem.DisplayName;
        //                    if (dataListItem.DisplayName.Contains("(")) {
        //                        int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
        //                        test = dataListItem.DisplayName.Remove(startIndex);
        //                        if (dataListItem.DisplayName.Contains(")")) {
        //                            startIndex = dataListItem.DisplayName.IndexOf(")");
        //                            test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
        //                            if (item.Contains(test) && item.Contains(test2)) {
        //                                containingFeilds.Add("foreachElementName");
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    else {
        //                        test = dataListItem.DisplayName;
        //                        if (item.Equals(test)) {
        //                            containingFeilds.Add("foreachElementName");
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return containingFeilds;
        //}

        static List<string> DsfForEachActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);

            var forEachElementNameProperty = objectToCheck.Properties["ForEachElementName"];
            //var additionalDataProperty = objectToCheck.Properties["AdditionalData"];

            if (forEachElementNameProperty != null) {
                var foreachElementName = forEachElementNameProperty.ComputedValue == null ? string.Empty : forEachElementNameProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(foreachElementName) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (foreachElementName.StartsWith("[[") && foreachElementName.EndsWith("]]")) {
                        var verifyedParts = BuildParts(foreachElementName);
                        foreach (string item in verifyedParts) {
                            string test = dataListItem.DisplayName;
                            string test2 = dataListItem.DisplayName;
                            if (dataListItem.DisplayName.Contains("(")) {
                                int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                                test = dataListItem.DisplayName.Remove(startIndex);
                                if (dataListItem.DisplayName.Contains(")")) {
                                    startIndex = dataListItem.DisplayName.IndexOf(")");
                                    test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                    if (item.Contains(test) && item.Contains(test2)) {
                                        containingFeilds.Add("foreachElementName");
                                        break;
                                    }
                                }
                            }
                            else {
                                test = dataListItem.DisplayName;
                                if (item.Equals(test)) {
                                    containingFeilds.Add("foreachElementName");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //if (additionalDataProperty != null) {
            //    var additionalData = additionalDataProperty.ComputedValue == null ? string.Empty : additionalDataProperty.ComputedValue.ToString();
            //    if (!string.IsNullOrEmpty(additionalData) && !string.IsNullOrEmpty(comparisonValue)) {
            //        if (!additionalData.StartsWith("[[") && !additionalData.EndsWith("]]")) {
            //            additionalData = "[[" + additionalData + "]]";
            //        }
            //        var verifyedParts = BuildParts(additionalData);
            //        foreach (string item in verifyedParts) {
            //            string test = dataListItem.DisplayName;
            //            string test2 = dataListItem.DisplayName;
            //            if (dataListItem.DisplayName.Contains("(")) {
            //                int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
            //                test = dataListItem.DisplayName.Remove(startIndex);
            //                if (dataListItem.DisplayName.Contains(")")) {
            //                    startIndex = dataListItem.DisplayName.IndexOf(")");
            //                    test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
            //                    if (item.Contains(test) && item.Contains(test2)) {
            //                        containingFeilds.Add("additionalData");
            //                        break;
            //                    }
            //                }
            //            }
            //            else {
            //                test = dataListItem.DisplayName;
            //                if (item.Equals(test)) {
            //                    containingFeilds.Add("additionalData");
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            return containingFeilds;
        }

        static List<string> DsfAssignActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            var fieldValueProperty = objectToCheck.Properties["FieldValue"];
            var fieldNameProperty = objectToCheck.Properties["FieldName"];

            string fieldValue = string.Empty;
            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);
            if (fieldNameProperty != null) {
                fieldValue = fieldNameProperty.ComputedValue == null ? string.Empty : fieldNameProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!fieldValue.StartsWith("[[") && !fieldValue.EndsWith("]]")) {
                        fieldValue = "[[" + fieldValue + "]]";
                    }
                    var verifyedParts = BuildParts(fieldValue);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("FieldName");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("FieldName");
                                break;
                            }
                        }
                    }
                }
            }

            if (fieldValueProperty != null) {
                fieldValue = fieldValueProperty.ComputedValue == null ? string.Empty : fieldValueProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(comparisonValue)) {
                    var verifyedParts = BuildParts(fieldValue);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("FieldValue");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("FieldValue");
                                break;
                            }
                        }
                    }


                }
            }

            return containingFeilds;
        }

        static List<string> DsfCountRecordsetActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            var RecordsetNameProperty = objectToCheck.Properties["RecordsetName"];
            var CountNumberProperty = objectToCheck.Properties["CountNumber"];

            string countNumber = string.Empty;
            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);
            if (CountNumberProperty != null) {
                countNumber = CountNumberProperty.ComputedValue == null ? string.Empty : CountNumberProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(countNumber) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!countNumber.StartsWith("[[") && !countNumber.EndsWith("]]")) {
                        countNumber = "[[" + countNumber + "]]";
                    }
                    var verifyedParts = BuildParts(countNumber);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("RecordsetName");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("RecordsetName");
                                break;
                            }
                        }
                    }
                }
            }

            if (RecordsetNameProperty != null) {
                countNumber = RecordsetNameProperty.ComputedValue == null ? string.Empty : RecordsetNameProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(countNumber) && !string.IsNullOrEmpty(comparisonValue)) {
                    var verifyedParts = BuildParts(countNumber);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("CountNumber");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("CountNumber");
                                break;
                            }
                        }
                    }


                }
            }

            return containingFeilds;
        }

        static List<string> DsfActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFields = new List<string>();

            var inputMappingProperty = objectToCheck.Properties["InputMapping"];
            var outputMappingProperty = objectToCheck.Properties["OutputMapping"];

            string inputMapping = string.Empty;
            string outputMapping = string.Empty;
            List<string> names = new List<string>();

            if (outputMappingProperty != null) {
                outputMapping = outputMappingProperty.ComputedValue == null ? string.Empty : outputMappingProperty.ComputedValue.ToString();

                if (!string.IsNullOrEmpty(outputMapping)) {
                    XElement data = XElement.Parse(outputMapping);
                    var nodes = data.DescendantsAndSelf("Output");
                    var maps = nodes.Attributes("Value").Where(c => !string.IsNullOrEmpty(c.Value));
                    if (maps.Any()) {
                        maps.ToList().ForEach(item => names.Add(item.Value));
                    }
                }
            }

            if (inputMappingProperty != null) {
                inputMapping = inputMappingProperty.ComputedValue == null ? string.Empty : inputMappingProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(inputMapping)) {
                    XElement data = XElement.Parse(inputMapping);
                    var nodes = data.DescendantsAndSelf("Input");
                    var maps = nodes.Attributes("Source");
                    if (maps.Any()) {
                        maps.ToList().ForEach(item => names.Add(item.Value));
                    }
                }
            }

            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);
            if (names.Contains(comparisonValue)) {
                containingFields.Add("Contains");
            }
            return containingFields;
        }

        static List<string> DsfFileForEachActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);

            var forEachElementNameProperty = objectToCheck.Properties["ForEachElementName"];
            var additionalDataProperty = objectToCheck.Properties["AdditionalData"];

            if (forEachElementNameProperty != null) {
                var foreachElementName = forEachElementNameProperty.ComputedValue == null ? string.Empty : forEachElementNameProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(foreachElementName) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!foreachElementName.StartsWith("[[") && !foreachElementName.EndsWith("]]")) {
                        foreachElementName = "[[" + foreachElementName + "]]";
                    }
                    var verifyedParts = BuildParts(foreachElementName);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("foreachElementName");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("foreachElementName");
                                break;
                            }
                        }
                    }
                }
            }

            if (additionalDataProperty != null) {
                var additionalData = additionalDataProperty.ComputedValue == null ? string.Empty : additionalDataProperty.ComputedValue.ToString();
                if (!string.IsNullOrEmpty(additionalData) && !string.IsNullOrEmpty(comparisonValue)) {
                    if (!additionalData.StartsWith("[[") && !additionalData.EndsWith("]]")) {
                        additionalData = "[[" + additionalData + "]]";
                    }
                    var verifyedParts = BuildParts(additionalData);
                    foreach (string item in verifyedParts) {
                        string test = dataListItem.DisplayName;
                        string test2 = dataListItem.DisplayName;
                        if (dataListItem.DisplayName.Contains("(")) {
                            int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                            test = dataListItem.DisplayName.Remove(startIndex);
                            if (dataListItem.DisplayName.Contains(")")) {
                                startIndex = dataListItem.DisplayName.IndexOf(")");
                                test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                if (item.Contains(test) && item.Contains(test2)) {
                                    containingFeilds.Add("additionalData");
                                    break;
                                }
                            }
                        }
                        else {
                            test = dataListItem.DisplayName;
                            if (item.Equals(test)) {
                                containingFeilds.Add("additionalData");
                                break;
                            }
                        }
                    }
                }
            }

            return containingFeilds;
        }

        static List<string> DsfMultiAssignActivityContainsDataListItem(ModelItem objectToCheck, IDataListItemModel dataListItem) {
            List<string> containingFeilds = new List<string>();
            ObservableCollection<ActivityDTO> multiAssignRows = objectToCheck.Properties["FieldsCollection"].ComputedValue as ObservableCollection<ActivityDTO>;
            foreach (ActivityDTO row in multiAssignRows) {
                var fieldValueProperty = row.FieldValue;
                var fieldNameProperty = row.FieldName;

                string fieldValue = string.Empty;
                string comparisonValue = string.IsNullOrEmpty(dataListItem.Name) ? string.Empty : string.Format("[[{0}]]", dataListItem.Name);
                if (fieldNameProperty != null) {
                    fieldValue = fieldNameProperty == null ? string.Empty : fieldNameProperty;
                    if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(comparisonValue)) {
                        if (!fieldValue.StartsWith("[[") && !fieldValue.EndsWith("]]")) {
                            fieldValue = "[[" + fieldValue + "]]";
                        }
                        var verifyedParts = BuildParts(fieldValue);
                        foreach (string item in verifyedParts) {
                            string test = dataListItem.DisplayName;
                            string test2 = dataListItem.DisplayName;
                            if (dataListItem.DisplayName.Contains("(")) {
                                int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                                test = dataListItem.DisplayName.Remove(startIndex);
                                if (dataListItem.DisplayName.Contains(")")) {
                                    startIndex = dataListItem.DisplayName.IndexOf(")");
                                    test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                    if (item.Contains(test) && item.Contains(test2)) {
                                        containingFeilds.Add(test);
                                        break;
                                    }
                                }
                            }
                            else {
                                test = dataListItem.DisplayName;
                                if (item.Equals(test)) {
                                    containingFeilds.Add(test);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (fieldValueProperty != null) {
                    fieldValue = fieldValueProperty == null ? string.Empty : fieldValueProperty;
                    if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(comparisonValue)) {
                        var verifyedParts = BuildParts(fieldValue);
                        foreach (string item in verifyedParts) {
                            string test = dataListItem.DisplayName;
                            string test2 = dataListItem.DisplayName;
                            if (dataListItem.DisplayName.Contains("(")) {
                                int startIndex = dataListItem.DisplayName.IndexOf("(") + 1;
                                test = dataListItem.DisplayName.Remove(startIndex);
                                if (dataListItem.DisplayName.Contains(")")) {
                                    startIndex = dataListItem.DisplayName.IndexOf(")");
                                    test2 = dataListItem.DisplayName.Substring(startIndex, dataListItem.DisplayName.Length - startIndex);
                                    if (item.Contains(test) && item.Contains(test2)) {
                                        containingFeilds.Add(test);
                                        break;
                                    }
                                }
                            }
                            else {
                                test = dataListItem.DisplayName;
                                if (item.Equals(test)) {
                                    containingFeilds.Add(test);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return containingFeilds;
        }

        private static IList<String> BuildParts(string item) {
            IDev2StudioDataLanguageParser languageParser = DataListFactory.CreateStudioLanguageParser();

            IList<String> resultData = languageParser.ParseForActivityDataItems(item);
            return resultData;
        }

        public static string RemoveRecordSetBrace(string item) {
            string fullyFormattedStringValue;
            if (item.Contains("(") && item.Contains(")")) {
                fullyFormattedStringValue = item.Remove(item.IndexOf("("));
            }
            else
                return item;
            return fullyFormattedStringValue;
        }
    }
}
