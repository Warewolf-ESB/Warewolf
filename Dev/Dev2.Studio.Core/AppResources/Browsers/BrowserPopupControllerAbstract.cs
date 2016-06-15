
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Browsers
{
    // BUG 9798 - 2013.06.25 - TWR : modified to handle both internal and external
    public abstract class BrowserPopupControllerAbstract : IBrowserPopupController
    {
        public abstract bool ShowPopup(string url);
    }
}
