
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using Dev2.Studio.Core.AppResources.Attributes;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum WorkSurfaceContext
    {
        Unknown,
        Workflow,
        Service,
        SourceManager,
   
        [IconLocation("pack://application:,,,/images/TaskScheduler-32.png")]
        [Description("Scheduler")]
        Scheduler,

        [IconLocation("pack://application:,,,/images/Settings-32.png")]
        [Description("Settings")]
        Settings,

        [IconLocation("pack://application:,,,/images/DependencyGraph-16.png")]
        [Description("Dependency Visualiser")]
        DependencyVisualiser,

        [IconLocation("pack://application:,,,/images/HelpLanguage-32.png")]
        [Description("Language Help")]
        LanguageHelp,

        [IconLocation("/images/Deploy-32.png")]
        [Description("Deploy")]
        DeployResources,

        [IconLocation("Pack_Uri_Application_Image_Home", typeof(StringResources))]
        [Description("Start Page")]
        StartPage,
        [IconLocation("pack://application:,,,/images/DependencyGraph-16.png")]
        [Description("Reverse Dependency Visualiser")]
        ReverseDependencyVisualiser,

        Help
    }

}
