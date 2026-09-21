/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/




namespace Dev2.Runtime.ServiceModel.Data

{
    public enum AuthenticationType
    {
        Windows,
        User,
        Anonymous,
        Public,
        Password,
        API_Key,
        /// <summary>Web sources: the key is appended to every request's query string as <c>?{ApiKeyName}={ApiKeyValue}</c>.</summary>
        ApiKeyQuery,
        /// <summary>Web sources: the key is sent on every request as the header <c>{ApiKeyName}: {ApiKeyValue}</c>.</summary>
        ApiKeyHeader,
        /// <summary>Web sources: every request carries <c>Authorization: Bearer {ApiKeyValue}</c>.</summary>
        Bearer
    }

    public static class AuthenticationTypeExtensions
    {
        /// <summary>True for the Web source types that send <c>ApiKeyValue</c> with each request.</summary>
        public static bool CarriesApiKey(this AuthenticationType authenticationType) =>
            authenticationType == AuthenticationType.ApiKeyQuery ||
            authenticationType == AuthenticationType.ApiKeyHeader ||
            authenticationType == AuthenticationType.Bearer;
    }
}
