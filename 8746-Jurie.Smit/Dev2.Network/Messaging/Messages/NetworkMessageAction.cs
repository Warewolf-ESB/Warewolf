namespace Dev2.Network.Messaging.Messages
{
    public enum NetworkMessageAction
    {
        Unknown,
        Read, // Use to get settings & file is hash mismatch
        Write, // Use to save settings, first time
        Overwrite // Use if version conflict when using 'Write' and want to override
    }
}