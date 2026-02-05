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
        private bool _includeSystemLog = true;
        private bool _includeResourcesXaml = true;
        private bool _includeResourcesJson = true;
        private int _numberOfLogLines = 1000;

        public NamedGuidWithEncryptedPayload ChatbotSource
        {
            get => _chatbotSource;
            set => SetProperty(ref _chatbotSource, value);
        }

        public bool IncludeSystemLog
        {
            get => _includeSystemLog;
            set => SetProperty(ref _includeSystemLog, value);
        }

        public bool IncludeResourcesXaml
        {
            get => _includeResourcesXaml;
            set => SetProperty(ref _includeResourcesXaml, value);
        }

        public bool IncludeResourcesJson
        {
            get => _includeResourcesJson;
            set => SetProperty(ref _includeResourcesJson, value);
        }

        public int NumberOfLogLines
        {
            get => _numberOfLogLines;
            set => SetProperty(ref _numberOfLogLines, value);
        }

        public ChatbotSettingsData Clone()
        {
            var result = (ChatbotSettingsData)MemberwiseClone();
            result._chatbotSource = ChatbotSource.Clone();
            result._includeSystemLog = IncludeSystemLog;
            result._includeResourcesXaml = IncludeResourcesXaml;
            result._includeResourcesJson = IncludeResourcesJson;
            result._numberOfLogLines = NumberOfLogLines;
            return result;
        }

        public bool Equals(ChatbotSettingsData obj)
        {
            if (obj is ChatbotSettingsData other)
            {
                var equals = ChatbotSource.Equals(other.ChatbotSource);
                equals &= IncludeSystemLog == other.IncludeSystemLog;
                equals &= IncludeResourcesXaml == other.IncludeResourcesXaml;
                equals &= IncludeResourcesJson == other.IncludeResourcesJson;
                equals &= NumberOfLogLines == other.NumberOfLogLines;
                return equals;
            }

            return false;
        }
    }
}
