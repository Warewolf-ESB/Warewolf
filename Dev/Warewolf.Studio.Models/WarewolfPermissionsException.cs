using System;

namespace Warewolf.Studio.Models
{
    class WarewolfPermissionsException : Exception
    {


        public WarewolfPermissionsException(string theUserDoesNotHaveTheRightTODeployTOThisResource):base(theUserDoesNotHaveTheRightTODeployTOThisResource)
        {

        }
    }
}
