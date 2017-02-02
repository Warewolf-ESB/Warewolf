/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dev2.Studio.Core.AppResources.Attributes;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum ResourceType
    {
        [TreeCategory("WORKFLOWS")]
        [Description("Workflow service")]
        [Display(Order = 1)]
        WorkflowService,

        [TreeCategory("SERVICES")]
        [Description("Worker Service")]
        [Display(Order = 2)]
        Service,

        [TreeCategory("SOURCES")]
        [Description("Source")]
        [Display(Order = 3)]
        Source,

        [Description("Unknown")]
        Unknown,

        Server
    }

    public static class ResourceTypeExtensions
    {
        public static WorkSurfaceContext ToWorkSurfaceContext(this ResourceType resourceType)
        {
            switch(resourceType)
            {
                case ResourceType.WorkflowService:
                    return WorkSurfaceContext.Workflow;
                case ResourceType.Service:
                    return WorkSurfaceContext.Service;
                case ResourceType.Source:
                    return WorkSurfaceContext.SourceManager;
                case ResourceType.Unknown:
                    return WorkSurfaceContext.Unknown;
                default: return WorkSurfaceContext.Unknown;
            }
        }

    }
}
