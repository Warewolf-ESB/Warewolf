/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
{
    /// <summary>
    /// Please note that this interface cannot be removed due to the fact that at some point
    /// The Compliler property on NativeActivity was serialized and so now the property cannot be removed
    /// and hence this interface cannot be removed.
    /// </summary>
    public interface IDataListCompiler : IDisposable
    {
        
    }
}
