namespace Dev2.Common.Interfaces.DB
{
    public interface IDataProviderSanitizer
    {

        string SanitizePayload(string xmlFormatedPayload);

        enSupportedDBTypes HandlesType();
    }
}
