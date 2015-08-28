
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
