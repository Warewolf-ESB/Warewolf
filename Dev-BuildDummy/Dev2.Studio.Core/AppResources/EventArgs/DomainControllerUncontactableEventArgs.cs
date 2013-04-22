namespace Dev2.Studio.Core.AppResources.EventArgs {
    public class DomainControllerUncontactableEventArgs : System.EventArgs {
        public string Domain { get; set; }

        public DomainControllerUncontactableEventArgs(string domain) {
            this.Domain = domain;
        }
    }
}
