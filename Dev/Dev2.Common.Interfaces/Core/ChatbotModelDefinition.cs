/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces.Core
{
    public class ChatbotModelDefinition : IEquatable<ChatbotModelDefinition>
    {
        [ExcludeFromCodeCoverage]
        public ChatbotModelDefinition()
        {
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string OwnedBy { get; set; }

        public bool Equals(ChatbotModelDefinition other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equals = true;
            equals &= string.Equals(Id, other.Id);
            equals &= string.Equals(Object, other.Object);
            equals &= Created == other.Created;
            equals &= string.Equals(OwnedBy, other.OwnedBy);

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ChatbotModelDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Object?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Created.GetHashCode();
                hashCode = (hashCode * 397) ^ (OwnedBy?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ChatbotModelDefinition left, ChatbotModelDefinition right) => Equals(left, right);

        public static bool operator !=(ChatbotModelDefinition left, ChatbotModelDefinition right) => !Equals(left, right);
    }
}
