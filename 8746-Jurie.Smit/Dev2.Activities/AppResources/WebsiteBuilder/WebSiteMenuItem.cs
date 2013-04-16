using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.AppResources.WebsiteBuilder
{
    [Serializable]
    public class WebSiteMenuItem
    {
        public Guid WebSiteMenuItemID { get; set; }
        public string DisplayName { get; set; }
        public string Link { get; set; }
        public Guid ParentWebSiteMenuItemID { get; set; }

        public WebSiteMenuItem()
        {

        }

        public WebSiteMenuItem(Guid _websiteMenuItemID, string _displayName, string _link, Guid _ParentWebsiteMenuItemID)
        {
            this.WebSiteMenuItemID = _websiteMenuItemID;
            this.DisplayName = _displayName;
            this.Link = _link;
            this.ParentWebSiteMenuItemID = _ParentWebsiteMenuItemID;
        }
    }
}
