/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Dev2.Integration.Tests.Mocks
{
    internal class DataObjectChild
    {
        internal string child { get; set; }
        internal List<string> childattributes { get; set; }

        internal DataObjectChild()
        {
            child = "dtochild";
            childattributes = new List<string>() { "CanDoWork", "CanAcceptConnection", "IsMultithreaded" };
        }
        internal DataObjectChild(string childname)
        {
            child = childname;
            childattributes = new List<string>() { "CanDoWork", "CanAcceptConnection", "IsMultithreaded" };
        }

    }
}
