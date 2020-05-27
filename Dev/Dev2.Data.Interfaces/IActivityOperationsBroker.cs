/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data.Interfaces
{
    public interface IActivityOperationsBroker
    {
        string Get(IActivityIOOperationsEndPoint path);
        byte[] GetBytes(IActivityIOOperationsEndPoint path);
        string Get(IActivityIOOperationsEndPoint path, bool deferredRead);
        string PutRaw(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args);
        string Delete(IActivityIOOperationsEndPoint src);
        IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src, ReadTypes readTypes);
        string Create(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile);
        string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args);
        string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args);
        string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args);
        string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2ZipOperationTO args);
        string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2UnZipOperationTO args);
    }
}
