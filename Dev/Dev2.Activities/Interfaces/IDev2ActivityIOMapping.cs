/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// Travis.Frisinger : 28.11.2012
    /// Moved there here for the ForEach activity
    /// </summary>
    public interface IDev2ActivityIOMapping : IDev2Activity
    {
        string InputMapping { get; set; }
        string OutputMapping { get; set; }
    }
}
