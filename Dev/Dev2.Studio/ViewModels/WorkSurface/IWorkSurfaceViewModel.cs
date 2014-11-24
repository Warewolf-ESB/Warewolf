
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.WorkSurface
{
    public interface IWorkSurfaceViewModel : IScreen
    {
        WorkSurfaceContext WorkSurfaceContext { get; set; }
        string IconPath { get; set; }
    }
}
