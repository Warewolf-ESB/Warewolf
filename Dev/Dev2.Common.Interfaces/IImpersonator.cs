namespace Dev2.Common.Interfaces
{
    public interface IImpersonator
    {
        bool Impersonate(string userName, string domain, string password);
        void Undo();
    }
}