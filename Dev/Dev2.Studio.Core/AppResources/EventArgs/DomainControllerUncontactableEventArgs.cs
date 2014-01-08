// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.EventArgs
{
    public class DomainControllerUncontactableEventArgs : System.EventArgs
    {
        public string Domain { get; set; }

        public DomainControllerUncontactableEventArgs(string domain)
        {
            Domain = domain;
        }
    }
}
