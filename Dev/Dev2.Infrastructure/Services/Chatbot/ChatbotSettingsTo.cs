/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
namespace Dev2.Services.Chatbot
{
    public class ChatbotSettingsTo
    {
        public bool IncludeSystemLog { get; set; } = true;
        public bool IncludeResourcesXaml { get; set; } = true;
        public bool IncludeResourcesJson { get; set; } = true;
        public int NumberOfLogLines { get; set; } = 1000;
    }
}
