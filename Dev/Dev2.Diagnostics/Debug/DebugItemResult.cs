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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Dev2.Common.Interfaces.Diagnostics.Debug;

// ReSharper disable once CheckNamespace
namespace Dev2.Diagnostics
{
    [Serializable]
    public class DebugItemResult : IDebugItemResult, IEquatable<DebugItemResult>
    {
        public DebugItemResultType Type { get; set; }
        public string Label { get; set; }
        public string Variable { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
        public string MoreLink { get; set; }
        public bool HasError { get; set; }
        public bool TestStepHasError { get; set; }
        public bool MockSelected { get; set; }

        #region IXmlSerializable

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DebugItemResult other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return Type == other.Type && string.Equals(Label, other.Label) && string.Equals(Variable, other.Variable) && string.Equals(Operator, other.Operator) && string.Equals(GroupName, other.GroupName) && GroupIndex == other.GroupIndex && string.Equals(Value, other.Value) && string.Equals(MoreLink, other.MoreLink);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DebugItemResult)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Label?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Variable?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Operator?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (GroupName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ GroupIndex;
                hashCode = (hashCode * 397) ^ (Value?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (MoreLink?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(DebugItemResult left, DebugItemResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DebugItemResult left, DebugItemResult right)
        {
            return !Equals(left, right);
        }

        #endregion

        public XmlSchema GetSchema()
        {
            return null;
        }

        public string GetMoreLinkItem()
        {
            return string.IsNullOrEmpty(Variable) ? Value : string.Format("{0} {1} {2}", Variable, Operator, Value);
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            GroupName = reader.GetAttribute("GroupName");
            int idx;
            int.TryParse(reader.GetAttribute("GroupIndex"), out idx);
            GroupIndex = idx;

            while(reader.Read())
            {
                if(reader.IsStartElement("Type"))
                {
                    var result = reader.ReadElementString("Type");
                    DebugItemResultType type;
                    Enum.TryParse(result, out type);
                    Type = type;
                }

                if(reader.IsStartElement("Label"))
                {
                    Label = reader.ReadElementString("Label");
                }

                if(reader.IsStartElement("Variable"))
                {
                    Variable = reader.ReadElementString("Variable");
                }

                if(reader.IsStartElement("Operator"))
                {
                    Value = reader.ReadElementString("Operator");
                }

                if(reader.IsStartElement("Value"))
                {
                    Value = reader.ReadElementString("Value");
                }

                if(reader.IsStartElement("MoreLink"))
                {
                    MoreLink = reader.ReadElementString("MoreLink");
                }

                if (reader.IsStartElement("HasError"))
                {
                    bool hasError;
                    bool.TryParse(reader.ReadElementString("HasError"), out hasError);
                    HasError = hasError;
                }

                if (reader.IsStartElement("TestStepHasError"))
                {
                    bool testStepHasError;
                    bool.TryParse(reader.ReadElementString("TestStepHasError"), out testStepHasError);
                    TestStepHasError = testStepHasError;
                }

                if (reader.IsStartElement("MockSelected"))
                {
                    bool mockSelected;
                    bool.TryParse(reader.ReadElementString("MockSelected"), out mockSelected);
                    MockSelected = mockSelected;
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "DebugItemResult")
                {
                    reader.ReadEndElement();
                    break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if(!string.IsNullOrWhiteSpace(GroupName))
            {
                writer.WriteAttributeString("GroupName", GroupName);
            }

            if(GroupIndex != 0)
            {
                writer.WriteAttributeString("GroupIndex", GroupIndex.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteElementString("Type", Type.ToString());
            writer.WriteElementString("Label", Label);
            writer.WriteElementString("Variable", Variable);
            writer.WriteElementString("Operator", Operator);
            writer.WriteElementString("Value", Value);
            writer.WriteElementString("HasError", HasError.ToString());
            writer.WriteElementString("TestStepHasError", TestStepHasError.ToString());
            writer.WriteElementString("MockSelected", MockSelected.ToString());

            if(!string.IsNullOrWhiteSpace(MoreLink))
            {
                writer.WriteElementString("MoreLink", MoreLink);
            }
        }

        #endregion
    }
}
