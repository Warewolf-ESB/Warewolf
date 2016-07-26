/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using Dev2.Helpers;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    public class LatestWebGetter : ILatestGetter, IDisposable
    {
        readonly IDev2WebClient _webClient;

        public LatestWebGetter()
            : this(new Dev2WebClient(new WebClient()))
        {

        }

        public LatestWebGetter(IDev2WebClient webClient)
        {
            if(webClient == null)
            {
                throw new ArgumentNullException("webClient");
            }
            _webClient = webClient;
        }

        #region Implementation of IGetLatest

        public event EventHandler Invoked;

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _webClient.Dispose();
        }

        #endregion
    }
}
