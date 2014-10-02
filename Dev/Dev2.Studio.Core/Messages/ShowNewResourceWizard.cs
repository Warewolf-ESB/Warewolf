
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class ShowNewResourceWizard : IMessage
    {
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }

        public ShowNewResourceWizard(string resourceType)
        {
            ResourceType = resourceType;
        }

        public ShowNewResourceWizard(string resourceType, string resourcePath)
        {
            ResourceType = resourceType;
            ResourcePath = resourcePath;
        }
    }
}
