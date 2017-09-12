/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.TO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.SystemTemplates.Models
{
    public class Dev2DecisionStack : IDev2DataModel, IDev2FlowModel, IEquatable<Dev2DecisionStack>
    {
        private string _ver = "1.0.0";

        #region Properties

        public IList<Dev2Decision> TheStack { get; set; }

        public int TotalDecisions => TheStack.Count;

        [JsonIgnore]
        public string Version { get { return _ver; } set { _ver = value; } }

        [JsonConverter(typeof(StringEnumConverter))]
        public Dev2ModelType ModelName => Dev2ModelType.Dev2DecisionStack;

        [JsonConverter(typeof(StringEnumConverter))]
        public Dev2DecisionMode Mode { get; set; }

        public string TrueArmText { get; set; }

        public string FalseArmText { get; set; }

        public string DisplayText { get; set; }

        #endregion

        public string ToWebModel()
        {

            string result = JsonConvert.SerializeObject(this);

            return result;
        }

    
        public void AddModelItem(Dev2Decision item)
        {
            TheStack.Add(item);
        }

        public Dev2Decision GetModelItem(int idx)
        {
            return idx < TotalDecisions ? TheStack[idx] : null;
        }

        
        public string ToVBPersistableModel()

        {

            string result = ToWebModel();

            result = result.Replace("\"", "!"); // Quote so it is VB compliant
            return result;
        }

        public string GenerateUserFriendlyModel(IExecutionEnvironment env, Dev2DecisionMode mode, out ErrorResultTO errors)
        {
            StringBuilder result = new StringBuilder("");

            int cnt = 0;

            errors = new ErrorResultTO();
            // build the output for decisions
            foreach(Dev2Decision dd in TheStack)
            {
                result.Append(dd.GenerateUserFriendlyModel(env, Mode, out errors));
                // append mode if not at end
                if(cnt + 1 < TheStack.Count)
                {
                    result.Append(Mode);
                }
                result.AppendLine();
                cnt++;
            }

            // append the arms
            result.Append("THEN " + TrueArmText);
            result.AppendLine();
            result.Append("ELSE " + FalseArmText);

            return result.ToString();
        }

        
        public static string FromVBPersitableModelToJSON(string val)

        {
            return val.Replace("!", "\"");

        }

        /// <summary>
        /// Extracts the model from workflow persisted data.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public static string ExtractModelFromWorkflowPersistedData(string val)
        {
            int start = val.IndexOf("(", StringComparison.Ordinal);
            if(start > 0)
            {
                int end = val.IndexOf(@""",AmbientData", StringComparison.Ordinal);

                if(end > start)
                {
                    start += 2;
                    val = val.Substring(start, end - start);

                    return FromVBPersitableModelToJSON(val);
                }
            }

            return "";
        }

        /// <summary>
        /// Removes the dummy options from model.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public static string RemoveDummyOptionsFromModel(StringBuilder val)
        {

            var tmp = val.Replace(@"""EvaluationFn"":""Choose...""", @"""EvaluationFn"":""Choose""");

            // Hydrate and remove Choose options ;)

            try
            {
                Dev2DecisionStack dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(tmp.ToString());

                if(dds.TheStack != null)
                {
                    IList<Dev2Decision> toKeep = dds.TheStack.Where(item => item.EvaluationFn != enDecisionType.Choose).ToList();

                    dds.TheStack = toKeep;
                }

                tmp = new StringBuilder(JsonConvert.SerializeObject(dds));
            }
            catch(Exception ex)
            {
                Dev2Logger.Error("Dev2DecisionStack", ex, GlobalConstants.WarewolfError);
                // Best effort ;)
            }


            return tmp.ToString();

        }

        /// <summary>
        /// Removes the naughty chars from model.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public static string RemoveNaughtyCharsFromModel(string val)
        {
            var toReplace = new[] { "!", "[[]]", "&" };

            return toReplace.Aggregate(val, (current, r) => current.Replace(r, ""));
        }

        public bool Equals(Dev2DecisionStack other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var collectionEquals = CommonEqualityOps.CollectionEquals(TheStack, other.TheStack, new Dev2DecisionComparer());
            return collectionEquals
                && Mode == other.Mode
                && string.Equals(TrueArmText, other.TrueArmText)
                && string.Equals(FalseArmText, other.FalseArmText) 
                && string.Equals(DisplayText, other.DisplayText);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Dev2DecisionStack) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TheStack != null ? TheStack.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Mode;
                hashCode = (hashCode * 397) ^ (TrueArmText != null ? TrueArmText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FalseArmText != null ? FalseArmText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayText != null ? DisplayText.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
