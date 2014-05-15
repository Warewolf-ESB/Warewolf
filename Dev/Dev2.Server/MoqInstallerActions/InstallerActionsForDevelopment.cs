using System;
using Dev2.InstallerActions;

namespace Dev2
{
    public class InstallerActionsForDevelopment : MoqInstallerActions
    {
        public const string WarewolfGroup = "Warewolf Administrators";
        public const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";


        #region Implementation of MoqInstallerActions

        public void ExecuteMoqInstallerActions()
        {
            CreateWarewolfGroupAndAddCurrentUser();
        }

        #endregion

        #region Private Actions

        /// <summary>
        /// Creates the warewolf group and adds the current user.
        /// </summary>
        private void CreateWarewolfGroupAndAddCurrentUser()
        {
            WarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // start from fresh each time ;)
            if(wso.DoesWarewolfGroupExist())
            {
                return;
            }

            wso.AddWarewolfGroup();

            // Get the current executing user ;)
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent(false);
            var machineName = Environment.MachineName;
            var userAddString = wso.FormatUserForInsert(currentUser.Name, machineName);

            wso.AddUserToWarewolf(userAddString);
        }

        #endregion
    }
}
