
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;
using Dev2.Runtime.Configuration.Views;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    public class MockSettingsObjectA
    {
        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel) )]
        public MockSettingsObjectB SettingsB { get; set; }
        [SettingsObject(typeof(LoggingView), typeof(LoggingViewModel))]
        public MockSettingsObjectC SettingsC { get; set; }
    }
}
