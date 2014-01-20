namespace Dev2.Util
{
    public class AppSettings
    {
        public static string LocalHost { get; set; }

        public static string ServicesAddress
        {
            get
            {
                return LocalHost + "/wwwroot/services/Service/Resources/{0}";
            }
        }
    }
}