namespace Dev2.Common.DB
{
    public interface IDataProviderSanitizer
    {

        string SanitizePayload(string xmlFormatedPayload);

        enSupportedDBTypes HandlesType();
    }
}
