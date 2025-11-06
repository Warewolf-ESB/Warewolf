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
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IManageChatbotSourceViewModel
    {
        /// <summary>
        /// The API Key for authentication
        /// </summary>
        string ApiKey { get; set; }

        /// <summary>
        /// The Completions Endpoint URL
        /// </summary>
        string CompletionsEndpoint { get; set; }

        /// <summary>
        /// The Models Endpoint URL (used for testing connection)
        /// </summary>
        string ModelsEndpoint { get; set; }

        /// <summary>
        /// Test if connection is successful
        /// </summary>
        ICommand TestCommand { get; set; }

        /// <summary>
        /// Cancel the test connection
        /// </summary>
        ICommand CancelTestCommand { get; set; }

        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand SaveCommand { get; set; }

        /// <summary>
        /// Header text that is used on the view
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }

        /// <summary>
        /// Has test failed
        /// </summary>
        bool TestFailed { get; set; }

        /// <summary>
        /// Is testing in progress
        /// </summary>
        bool Testing { get; }

        /// <summary>
        /// The name of the resource
        /// </summary>
        string ResourceName { get; set; }
    }

    public interface IManageChatbotSourceModel
    {
        void TestConnection(IChatbotSource resource);

        void Save(IChatbotSource toSource);

        string ServerName { get; set; }

        IChatbotSource FetchSource(Guid id);
    }
}
