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
using System.Collections.Generic;
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class ChatbotSettingsData : BindableBase, IEquatable<ChatbotSettingsData>
    {
        private NamedGuidWithEncryptedPayload _chatbotSource = new NamedGuidWithEncryptedPayload();
        private bool? _encryptDataSource;
        private bool _includeSystemLog = true;
        private bool _loadResourcesAsXaml = true;
        private int _numberOfLogLines = 1000;
        private List<Guid> _selectedResourceIds = new List<Guid>();

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

        public bool LoadResourcesAsXaml
        {
            get => _loadResourcesAsXaml;
            set => SetProperty(ref _loadResourcesAsXaml, value);
        }

        public int NumberOfLogLines
        {
            get => _numberOfLogLines;
            set => SetProperty(ref _numberOfLogLines, value);
        }

        public List<Guid> SelectedResourceIds
        {
            get => _selectedResourceIds;
            set => SetProperty(ref _selectedResourceIds, value ?? new List<Guid>());
        }

        public ChatbotSettingsData Clone()
        {
            var result = (ChatbotSettingsData)MemberwiseClone();
            result._chatbotSource = ChatbotSource.Clone();
            result._includeSystemLog = IncludeSystemLog;
            result._loadResourcesAsXaml = LoadResourcesAsXaml;
            result._numberOfLogLines = NumberOfLogLines;
            result._selectedResourceIds = new List<Guid>(SelectedResourceIds ?? new List<Guid>());
            return result;
        }

        public bool Equals(ChatbotSettingsData obj)
        {
            if (obj is ChatbotSettingsData other)
            {
                var equals = ChatbotSource.Equals(other.ChatbotSource);
                equals &= IncludeSystemLog == other.IncludeSystemLog;
                equals &= LoadResourcesAsXaml == other.LoadResourcesAsXaml;
                equals &= NumberOfLogLines == other.NumberOfLogLines;
                equals &= AreResourceListsEqual(SelectedResourceIds, other.SelectedResourceIds);
                return equals;
            }

            return false;
        }

        private static bool AreResourceListsEqual(List<Guid> list1, List<Guid> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i]) return false;
            }
            return true;
        }
    }
}
