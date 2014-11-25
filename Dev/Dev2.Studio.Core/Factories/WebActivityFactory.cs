
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{
    public interface IWebActivityFactory
    {
        IWebActivity CreateWebActivity(object webActivityWrappingObject, IContextualResourceModel resourceModel,
                                       string serviceName);
    }
    public class InstanceWebActivityFactory : IWebActivityFactory
    {
        public IWebActivity CreateWebActivity(object webActivityWrappingObject, IContextualResourceModel resourceModel,
                                              string serviceName)
        {
            return WebActivityFactory.CreateWebActivity(webActivityWrappingObject, resourceModel, serviceName);
        }
    }

    public static class WebActivityFactory
    {
        internal static IWebActivity CreateWebActivity()
        {
            IWebActivity webActivity = new WebActivity();
            return webActivity;
        }

        public static IWebActivity CreateWebActivity(object webActivityWrappingObject)
        {
            IWebActivity activity = CreateWebActivity();
            activity.WebActivityObject = webActivityWrappingObject;
            return activity;
        }

        public static IWebActivity CreateWebActivity(object webActivityWrappingObject, IContextualResourceModel resourceModel, string serviceName)
        {
            IWebActivity activity = CreateWebActivity();
            activity.WebActivityObject = webActivityWrappingObject;
            activity.ResourceModel = resourceModel;
            activity.ServiceName = serviceName;
            return activity;
        }
    }
}
