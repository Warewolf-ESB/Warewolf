namespace Dev2.Services.Security
{
    public enum AuthorizationContext
    {
        None,
        View,
        Execute,
        Contribute,
        DeployTo,
        DeployFrom
    }
}
