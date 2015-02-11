using System;

namespace Warewolf.Studio.Models
{
    public class WarewolfPermissionsException : Exception
    {


        public WarewolfPermissionsException(string theUserDoesNotHaveTheRightTODeployTOThisResource):base(theUserDoesNotHaveTheRightTODeployTOThisResource)
        {

        }
    }
}
