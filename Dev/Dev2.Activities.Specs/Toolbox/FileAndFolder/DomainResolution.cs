
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.DirectoryServices.ActiveDirectory;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder
{
    static class DomainResolution
    {
        public static string ResolveDomain(this string username)
        {
            var domainSeperator = username.IndexOf('\\');
            if(domainSeperator<0)
            {
                return username;
            }
            username = username.Substring(domainSeperator);
	        try
	        {
                var domainName = Domain.GetComputerDomain();
                return username.Insert(0, domainName.Name);
	        }
	        catch (ActiveDirectoryObjectNotFoundException)
	        {
                return username.Insert(0, ".");
	        }
        }
    }
}
