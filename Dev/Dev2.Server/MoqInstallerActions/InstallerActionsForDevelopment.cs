using System;

namespace Dev2.MoqInstallerActions
{
    public class InstallerActionsForDevelopment : IMoqInstallerActions
    {
        public const string WarewolfGroup = "Warewolf Administrators";
        public const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";


        #region Implementation of MoqInstallerActions

        public void ExecuteMoqInstallerActions()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            if(!wso.DoesWarewolfGroupExist())
            {
                wso.AddWarewolfGroup();
            }
            else
            {
                wso.DeleteWarewolfGroup();
                wso.AddWarewolfGroup();
            }

            CreateWarewolfGroupAndAddCurrentUser();
            AddAdministratorsToWarewolfGroup();
        }

        #endregion

        #region Private Actions

        private void AddAdministratorsToWarewolfGroup()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            wso.AddAdministratorsGroupToWarewolf();
        }

        /// <summary>
        /// Creates the warewolf group and adds the current user.
        /// </summary>
        private void CreateWarewolfGroupAndAddCurrentUser()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Get the current executing user ;)
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent(false);
            var machineName = Environment.MachineName;
            var userAddString = wso.FormatUserForInsert(currentUser.Name, machineName);

            wso.AddUserToWarewolf(userAddString);
        }

        #endregion
    }
}
