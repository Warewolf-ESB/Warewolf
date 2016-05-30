namespace Dev2.Common.Interfaces
{
    public interface IManageWcfSourceModel
    {
        void TestConnection(IWcfServerSource resource);

        void Save(IWcfServerSource toSpSource);

        string ServerName { get; set; }
    }
}
