namespace Dev2.Services.Security
{
    public enum AuthorizationContext
    {
        [Reason("You are not authorized.")]
        None,

        [Reason("You are not authorized to view this resource.")]
        View,

        [Reason("You are not authorized to execute this resource.")]
        Execute,

        [Reason("You are not authorized to add, update, delete or save this resource.")]
        Contribute,

        [Reason("You are not authorized to deploy to this server.")]
        DeployTo,

        [Reason("You are not authorized to deploy from this server.")]
        DeployFrom,

        Administrator,

        Any
    }
}
