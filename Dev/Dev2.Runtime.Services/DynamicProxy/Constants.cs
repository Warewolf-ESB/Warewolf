#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
namespace Dev2.Runtime.DynamicProxy
{
    static class Constants
    {
        internal static class ErrorMessages
        {
            internal const string ImportError = 
                "There was an error in importing the metadata.";

            internal const string CodeGenerationError = 
                "There was an error in generating the proxy code.";

            internal const string CompilationError = 
                "There was an error in compiling the proxy code.";

            internal const string UnknownContract =
                "The specified contract is not found in the proxy assembly.";

            internal const string EndpointNotFound = 
                "The endpoint associated with contract {1}:{0} is not found.";

            internal const string ProxyTypeNotFound = 
                "The proxy that implements the service contract {0} is not found.";

            internal const string ProxyCtorNotFound = 
                "The constructor matching the specified parameter types is not found.";

            internal const string ParameterValueMistmatch =
                "The type for each parameter values must be specified.";

            internal const string MethodNotFound =
                "The method {0} is not found.";

            internal const string WebAddressError =
                "The web source has an incomplete web address.";
        }
    } 
}
