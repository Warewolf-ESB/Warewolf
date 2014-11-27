
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;

namespace Dev2.ViewModels.Deploy
{
    public class DeployDialogTO
    {
        #region Properties

        public string SourceName { get; private set; }
        public string DestinationName { get; private set; }
        public int RowNumber { get; private set; }
        public IResourceModel DestinationResource { get; private set; }

        #endregion

        #region Ctor

        public DeployDialogTO(int rowNumber, string sourceName, string destinationName, IResourceModel destinationResourceModel)
        {
            RowNumber = rowNumber;
            SourceName = sourceName;
            DestinationName = destinationName;
            DestinationResource = destinationResourceModel;
        }

        #endregion
    }
}
