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
