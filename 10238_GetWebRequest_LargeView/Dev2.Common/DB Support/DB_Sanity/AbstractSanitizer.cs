namespace Dev2.Common.DB
{
    public class AbstractSanitizer
    {

        internal string RemoveDelimiting(string payload)
        {
            return (payload.Replace("&lt;", "<").Replace("&gt;", ">"));
        }
    }
}
