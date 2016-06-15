/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class SharepointServerSourceModel : ISharePointSourceModel
    {
        public IStudioUpdateManager _updateRepository { get; private set; }
        readonly string _serverName;

        public SharepointServerSourceModel(IStudioUpdateManager updateRepository, string serverName)
        {
            _updateRepository = updateRepository;
            _serverName = serverName;
        }

        #region Implementation of ISharePointSourceModel

        public void TestConnection(ISharepointServerSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(ISharepointServerSource resource)
        {
            _updateRepository.Save(resource);
        }

        public string ServerName { get; set; }

        #endregion
    }
}