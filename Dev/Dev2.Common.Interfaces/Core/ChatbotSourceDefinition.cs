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
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Interfaces.Core
{
    public class ChatbotSourceDefinition : IChatbotSource, IEquatable<ChatbotSourceDefinition>
    {
        [ExcludeFromCodeCoverage]
        public ChatbotSourceDefinition()
        {
        }

        public bool Equals(ChatbotSourceDefinition other)
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
            equals &= string.Equals(ApiKey, other.ApiKey);
            equals &= string.Equals(CompletionsEndpoint, other.CompletionsEndpoint);
            equals &= string.Equals(ModelsEndpoint, other.ModelsEndpoint);
            equals &= string.Equals(SelectedModel, other.SelectedModel);
            equals &= string.Equals(Provider, other.Provider);
            equals &= string.Equals(Name, other.Name);
            equals &= Id == other.Id;

            return equals;
        }

        public bool Equals(IChatbotSource other)
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
            equals &= string.Equals(ApiKey, other.ApiKey);
            equals &= string.Equals(CompletionsEndpoint, other.CompletionsEndpoint);
            equals &= string.Equals(ModelsEndpoint, other.ModelsEndpoint);
            equals &= string.Equals(SelectedModel, other.SelectedModel);
            equals &= string.Equals(Provider, other.Provider);

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
            return Equals((ChatbotSourceDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ApiKey?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (CompletionsEndpoint?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ModelsEndpoint?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SelectedModel?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Provider?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ChatbotSourceDefinition left, ChatbotSourceDefinition right) => Equals(left, right);

        public static bool operator !=(ChatbotSourceDefinition left, ChatbotSourceDefinition right) => !Equals(left, right);

        public string ApiKey { get; set; }
        public string CompletionsEndpoint { get; set; }
        public string ModelsEndpoint { get; set; }
        public string SelectedModel { get; set; }
        public string Provider { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
