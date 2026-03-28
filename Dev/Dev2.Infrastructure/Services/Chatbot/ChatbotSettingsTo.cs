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

namespace Dev2.Services.Chatbot
{
    public class ChatbotSettingsTo
    {
        public bool IncludeSystemLog { get; set; } = true;
        public bool LoadResourcesAsXaml { get; set; } = true;
        public int NumberOfLogLines { get; set; } = 1000;
        public List<Guid> SelectedResourceIds { get; set; } = new List<Guid>();
        public string UserMessageColor { get; set; } = "#ff6600";
        public string UserMessageTextColor { get; set; } = "#ffffff";
        public string BotMessageColor { get; set; } = "#f8f9fa";
        public string BotMessageTextColor { get; set; } = "#333333";
        public int SlidingWindowSummaryLength { get; set; } = 200;
        public bool EnableSlidingWindowTrimming { get; set; } = false;
    }
}
