#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Converters;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Utilities;
using Dev2.Validation;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Data-BaseConversion", "Base Convert", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Base_Convert")]
    public class DsfBaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity, IEquatable<DsfBaseConvertActivity>
    {
        readonly Dev2BaseConversionFactory _fac = new Dev2BaseConversionFactory();


        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        public IList<BaseConvertTO> ConvertCollection { get; set; }

        public DsfBaseConvertActivity()
            : base("Base Conversion")
        {
            ConvertCollection = new List<BaseConvertTO>();
        }

        public override List<string> GetOutputs() => ConvertCollection.Select(to => to.ToExpression).ToList();

        protected override void CacheMetadata(NativeActivityMetadata metadata) => base.CacheMetadata(metadata);

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            var env = dataObject.Environment;
            try
            {
                TryExecute(dataObject, update, allErrors, env);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFBaseConvert", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                HandleErrors(dataObject, update, allErrors);
            }
        }

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, IExecutionEnvironment env)
        {
            CleanArgs();

            var inputIndex = 1;
            var outputIndex = 1;

            foreach (var item in ConvertCollection.Where(a => !String.IsNullOrEmpty(a.FromExpression)))
            {
                if (dataObject.IsDebugMode())
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", inputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugEvalResult(item.FromExpression, "Convert", env, update), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(item.FromType, "From"), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(item.ToType, "To"), debugItem);
                    _debugInputs.Add(debugItem);
                    inputIndex++;
                }

                try
                {
                    env.ApplyUpdate(item.FromExpression, TryConvertFunc(item, env, update), update);
                    IsSingleValueRule.ApplyIsSingleValueRule(item.FromExpression, allErrors);
                    if (dataObject.IsDebugMode())
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(item.FromExpression, "", env, update), debugItem);
                        _debugOutputs.Add(debugItem);
                        outputIndex++;
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("DSFBaseConvert", e, GlobalConstants.WarewolfError);
                    allErrors.AddError(e.Message);
                    if (dataObject.IsDebugMode())
                    {
                        outputIndex++;
                    }
                }
            }
        }

        private void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError(nameof(DsfBaseConvertActivity), allErrors);
                var errorString = allErrors.MakeDisplayReady();
                dataObject.Environment.AddError(errorString);
            }
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> TryConvertFunc(BaseConvertTO item, IExecutionEnvironment env, int update) => a =>
        {
            var from = _fac.CreateConverter((enDev2BaseConvertType)Dev2EnumConverter.GetEnumFromStringDiscription(item.FromType, typeof(enDev2BaseConvertType)));
            var to = _fac.CreateConverter((enDev2BaseConvertType)Dev2EnumConverter.GetEnumFromStringDiscription(item.ToType, typeof(enDev2BaseConvertType)));
            var broker = _fac.CreateBroker(@from, to);
            var value = a.ToString();
            if (a.IsNothing)
            {
                throw new Exception(string.Format(ErrorResource.NullScalarValue, item.FromExpression));
            }
            if (String.IsNullOrEmpty(value))
            {
                return DataStorage.WarewolfAtom.NewDataString("");
            }
            var upper = broker.Convert(value);
            var evalled = env.Eval(upper, update);
            if (evalled.IsWarewolfAtomResult)
            {
                if (evalled is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
                {
                    return warewolfAtomResult.Item;
                }
                return DataStorage.WarewolfAtom.Nothing;
            }
            return DataStorage.WarewolfAtom.NewDataString(CommonFunctions.evalResultToString(evalled));
        };

        void CleanArgs()
        {
            var count = 0;
            while (count < ConvertCollection.Count)
            {
                if (string.IsNullOrWhiteSpace(ConvertCollection[count].FromExpression))
                {
                    ConvertCollection.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            var mic = modelProperty?.Collection;

            if (mic != null)
            {
                var listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    var baseConvertTo = ConvertCollection.Last(c => !c.CanRemove());
                    var startIndex = ConvertCollection.IndexOf(baseConvertTo) + 1;
                    foreach (string s in listToAdd)
                    {
                        mic.Insert(startIndex, new BaseConvertTO(s, ConvertCollection[startIndex - 1].FromType, ConvertCollection[startIndex - 1].ToType, string.Empty, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
                else
                {
                    AddToCollection(listToAdd, modelItem);
                }
            }
        }

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            var mic = modelProperty?.Collection;

            if (mic != null)
            {
                var startIndex = 0;
                var firstRowConvertFromType = ConvertCollection[0].FromType;
                var firstRowConvertToType = ConvertCollection[0].ToType;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new BaseConvertTO(s, firstRowConvertFromType, firstRowConvertToType, string.Empty, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new BaseConvertTO(string.Empty, "Text", "Base 64", string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                var currentName = modelProperty.ComputedValue as string;
                if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FromExpression = a.FromExpression.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                var t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.FromExpression) && c.FromExpression.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.ToExpression = a.FromExpression.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            foreach (var item in ConvertCollection)
            {
                if (!string.IsNullOrEmpty(item.FromExpression) && item.FromExpression.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FromExpression, Value = item.FromExpression });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();

            foreach (var item in ConvertCollection)
            {
                if (!string.IsNullOrEmpty(item.FromExpression) && item.FromExpression.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.FromExpression, Value = item.FromExpression });
                }
            }

            return result;
        }

        public int GetCollectionCount() => throw new NotImplementedException();

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if (!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        public bool Equals(DsfBaseConvertActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var collectionEquals = CommonEqualityOps.CollectionEquals(ConvertCollection, other.ConvertCollection, new BaseConvertToComparer());
            return base.Equals(other)
                && collectionEquals;
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

            return Equals((DsfBaseConvertActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_fac != null ? _fac.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConvertCollection != null ? ConvertCollection.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "Convert Collection",
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ConvertCollection),
                    Type = StateVariable.StateType.InputOutput
                }
            };
        }
    }
}
