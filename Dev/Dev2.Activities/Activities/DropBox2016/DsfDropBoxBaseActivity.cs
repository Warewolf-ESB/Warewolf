#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using System;
using System.Net.Http;

namespace Dev2.Activities.DropBox2016
{
    public abstract class DsfDropBoxBaseActivity : DsfBaseActivity, IDisposable
    {
        protected readonly IDropboxClientFactory _dropboxClientFactory;
        protected IDropboxClient _dropboxClient;

        protected DsfDropBoxBaseActivity(IDropboxClientFactory dropboxClientFactory)
        {
            _dropboxClientFactory = dropboxClientFactory;
        }

        public void Dispose()
        {
            _dropboxClient.Dispose();
        }

        protected void SetupDropboxClient(string accessToken)
        {
            if (_dropboxClient != null)
            {
                return;
            }
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            _dropboxClient = _dropboxClientFactory.New(accessToken, httpClient);
        }
    }
}
