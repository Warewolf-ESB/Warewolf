
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Factory
{
    public static class FeedbackFactory
    {
        public static IFeedbackAction CreateEmailFeedbackAction(Dictionary<string, string> attachedFiles, IEnvironmentModel server)
        {
            var emailFeedbackAction = new EmailFeedbackAction(attachedFiles, server);
            return emailFeedbackAction;
        }
    }
}
