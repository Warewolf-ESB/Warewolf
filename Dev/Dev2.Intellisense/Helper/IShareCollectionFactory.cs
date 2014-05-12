namespace Dev2.Intellisense.Helper
{
    public interface IShareCollectionFactory
    {
        ShareCollection CreateShareCollection();
        ShareCollection CreateShareCollection(string server);
    }

    public class ShareCollectionFactory : IShareCollectionFactory
    {
        public ShareCollection CreateShareCollection()
        {
            return  new ShareCollection();
        }

        public ShareCollection CreateShareCollection(string server)
        {
            return  new ShareCollection(server);
        }
    }
}
