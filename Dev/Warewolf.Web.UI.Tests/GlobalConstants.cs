namespace Warewolf.Web.UI.Tests
{
    public static class GlobalConstants
    {

        public const string UserCredentialsShowingError = "'{0}' should not show the user credentials dialog";
        public const string UserCredentialsNotShowingError = "'{0}' is expecting the user credentials dialog";
        public const string IsAlertPresentError = "No alert that local Warewolf server is not running.";
        public const string LocalWarewolfServerError = "Local Warewolf Server Not Found";
        public const string ExpectedUserAuthError = "http://localhost:3142 is requesting your username and password.";
        public const string UserAuthAssertMessage = "'{0}' did not match the user credentials dialog error message.";
        public const string AlertText = "Alert text is not correct";
    }
}
