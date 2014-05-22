
namespace Dev2.MoqInstallerActions
{
    public interface IWarewolfSecurityOperations
    {
        void AddWarewolfGroup();

        bool DoesWarewolfGroupExist();

        bool IsUserInGroup(string username);

        void AddUserToWarewolf(string currentUser);

        void DeleteWarewolfGroup();

        string FormatUserForInsert(string currentUser, string machineName);

        void AddAdministratorsGroupToWarewolf();

        bool IsAdminMemberOfWarewolf();
    }
}
