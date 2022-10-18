/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Services.Security
{
    public class AuthorizationRequestKey : Tuple<string, string, AuthorizationContext>
    {
        public AuthorizationRequestKey(string item1, string item2, AuthorizationContext item3)
            : base(item1, item2, item3)
        {
        }
    }
    public interface IAuthorizationRequest
    {
        AuthorizationRequestKey Key { get; }
        WebServerRequestType RequestType { get; }
        IPrincipal User { get; }
        Uri Url { get; }
        Microsoft.AspNetCore.Http.IQueryCollection QueryString { get; }
        string ResourcePath { get; }
    }

    //public interface INameValueCollection : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    //{
    //    string this[string key] { get; }
    //    IEnumerable<string> GetValues(string key);
    //    string Get(string key);
    //}
}
