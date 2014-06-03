using System;

namespace Dev2.Services.Security.MoqInstallerActions
{
    public class InstallerActionsForDevelopment : IMoqInstallerActions
    {

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
