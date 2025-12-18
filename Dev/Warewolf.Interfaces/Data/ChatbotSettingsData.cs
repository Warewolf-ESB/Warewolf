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
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class ChatbotSettingsData : BindableBase, IEquatable<ChatbotSettingsData>
    {
        private NamedGuidWithEncryptedPayload _chatbotSource = new NamedGuidWithEncryptedPayload();
        private bool? _encryptDataSource;

        public NamedGuidWithEncryptedPayload ChatbotSource
        {
            get => _chatbotSource;
            set => SetProperty(ref _chatbotSource, value);
        }

        public bool? EncryptDataSource
        {
            get => _encryptDataSource ?? true;
            set => SetProperty(ref _encryptDataSource, value);
        }

        public ChatbotSettingsData Clone()
        {
            var result = (ChatbotSettingsData)MemberwiseClone();
            result._chatbotSource = ChatbotSource.Clone();
            return result;
        }

        public bool Equals(ChatbotSettingsData obj)
        {
            if (obj is ChatbotSettingsData other)
            {
                var equals = ChatbotSource.Equals(other.ChatbotSource);
                equals &= EncryptDataSource.Equals(other.EncryptDataSource);
                return equals;
            }

            return false;
        }
    }
}
