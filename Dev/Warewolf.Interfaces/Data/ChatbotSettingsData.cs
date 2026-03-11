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
        private bool _loadResourcesAsXaml = false;
        private int _numberOfLogLines = 1000;
        private List<Guid> _selectedResourceIds = new List<Guid>();
        private string _userMessageColor = "#ff6600";
        private string _userMessageTextColor = "#ffffff";
        private string _botMessageColor = "#f8f9fa";
        private string _botMessageTextColor = "#333333";
        private int _slidingWindowSummaryLength = 200;
        private bool _enableSlidingWindowTrimming = false;

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

        public string UserMessageColor
        {
            get => _userMessageColor;
            set => SetProperty(ref _userMessageColor, value ?? "#ff6600");
        }

        public string UserMessageTextColor
        {
            get => _userMessageTextColor;
            set => SetProperty(ref _userMessageTextColor, value ?? "#ffffff");
        }

        public string BotMessageColor
        {
            get => _botMessageColor;
            set => SetProperty(ref _botMessageColor, value ?? "#f8f9fa");
        }

        public string BotMessageTextColor
        {
            get => _botMessageTextColor;
            set => SetProperty(ref _botMessageTextColor, value ?? "#333333");
        }

        public int SlidingWindowSummaryLength
        {
            get => _slidingWindowSummaryLength;
            set => SetProperty(ref _slidingWindowSummaryLength, value > 0 ? value : 200);
        }

        public bool EnableSlidingWindowTrimming
        {
            get => _enableSlidingWindowTrimming;
            set => SetProperty(ref _enableSlidingWindowTrimming, value);
        }

        public ChatbotSettingsData Clone()
        {
            var result = (ChatbotSettingsData)MemberwiseClone();
            result._chatbotSource = ChatbotSource.Clone();
            result._includeSystemLog = IncludeSystemLog;
            result._loadResourcesAsXaml = LoadResourcesAsXaml;
            result._numberOfLogLines = NumberOfLogLines;
            result._selectedResourceIds = new List<Guid>(SelectedResourceIds ?? new List<Guid>());
            result._userMessageColor = UserMessageColor;
            result._userMessageTextColor = UserMessageTextColor;
            result._botMessageColor = BotMessageColor;
            result._botMessageTextColor = BotMessageTextColor;
            result._slidingWindowSummaryLength = SlidingWindowSummaryLength;
            result._enableSlidingWindowTrimming = EnableSlidingWindowTrimming;
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
                equals &= UserMessageColor == other.UserMessageColor;
                equals &= UserMessageTextColor == other.UserMessageTextColor;
                equals &= BotMessageColor == other.BotMessageColor;
                equals &= BotMessageTextColor == other.BotMessageTextColor;
                equals &= SlidingWindowSummaryLength == other.SlidingWindowSummaryLength;
                equals &= EnableSlidingWindowTrimming == other.EnableSlidingWindowTrimming;
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
