/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#if !NOTNANOSERVER
using System.DirectoryServices.ActiveDirectory;
#endif

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
            string domainName = "";
#if !NOTNANOSERVER
			Domain getDomain = null;
            try
            {
                getDomain = Domain.GetComputerDomain();
            }
            catch (System.Exception)
            {
                //not on a domain
            }
            if (getDomain != null)
            {
                domainName = getDomain.Name;
            }
#endif
            return username.Insert(0, domainName);
        }
    }
}
