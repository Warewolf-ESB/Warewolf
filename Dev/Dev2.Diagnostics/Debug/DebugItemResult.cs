#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Diagnostics.Debug;


namespace Dev2.Diagnostics
{
    [DataContract]
    public class DebugItemResult : IDebugItemResult
    {
        [DataMember]
        public DebugItemResultType Type { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Variable { get; set; }

        [DataMember]
        public string TruncatedValue { get; set; }

        [DataMember]
        public string Operator { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public int GroupIndex { get; set; }

        [DataMember]
        public string MoreLink { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public bool TestStepHasError { get; set; }

        [DataMember]
        public bool MockSelected { get; set; }

        public string GetMoreLinkItem() => string.IsNullOrEmpty(Variable) ? Value : string.Format("{0} {1} {2}", Variable, Operator, Value);
        public IDebugItemResult Clone()
        {
            return new DebugItemResult
            {
                Type = this.Type,
                Label = Label,
                Variable = Variable,
                Operator = Operator,
                Value = Value,
                TruncatedValue = TruncatedValue,
                GroupName = GroupName,
                GroupIndex = GroupIndex,
                MoreLink = MoreLink,
                HasError = HasError,
                TestStepHasError = TestStepHasError,
                MockSelected = MockSelected,
            };
        }
    }
}
